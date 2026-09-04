using FluentValidation;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Application.Abstractions.Security;
using PersonalFinance.Application.Finance.Common;
using PersonalFinance.BuildingBlocks.Abstractions;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.JournalEntries;
using PersonalFinance.Domain.Finance.Ledgers;

namespace PersonalFinance.Application.Finance.Accounts.CreateAccount;

public sealed class CreateAccountHandler(
    ILedgerRepository ledgerRepository,
    IAccountRepository accountRepository,
    IJournalEntryRepository journalEntryRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    EquityBackingAccountProvisioner equityBackingAccountProvisioner,
    IValidator<CreateAccountCommand> validator)
{
    public async Task<Result<CreateAccountResponse>> HandleAsync(CreateAccountCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            return Result.Failure<CreateAccountResponse>(
                ResultError.Validation("Account.Validation", validation.Errors[0].ErrorMessage));
        }

        var ledger = await ledgerRepository.GetByIdAsync(command.LedgerId, ct);
        if (ledger is null)
            return Result.Failure<CreateAccountResponse>(
                ResultError.NotFound("Ledger.NotFound", "Ledger not found."));

        var userIdResult = UserId.Create(currentUserService.UserId);
        if (userIdResult.IsFailure)
            return Result.Failure<CreateAccountResponse>(userIdResult.Error);

        var memberCheck = ledger.EnsureMember(userIdResult.Value);
        if (memberCheck.IsFailure)
            return Result.Failure<CreateAccountResponse>(memberCheck.Error);

        if (await accountRepository.ExistsByNameAsync(command.LedgerId, command.Name, ct))
        {
            return Result.Failure<CreateAccountResponse>(
                ResultError.Conflict("Account.Name.Taken", "An account with this name already exists in this ledger."));
        }

        DueDate? dueDate = null;
        if (command.DueDateDay.HasValue)
        {
            var dueDateResult = DueDate.Create(command.DueDateDay.Value);
            if (dueDateResult.IsFailure)
                return Result.Failure<CreateAccountResponse>(dueDateResult.Error);

            dueDate = dueDateResult.Value;
        }

        var accountResult = Account.Create(
            command.LedgerId,
            command.Name,
            command.Type,
            dueDate,
            categoryId: null,
            dateTimeProvider.UtcNow);

        if (accountResult.IsFailure)
            return Result.Failure<CreateAccountResponse>(accountResult.Error);

        var account = accountResult.Value;

        accountRepository.Add(account);

        if (command.OpeningBalance.HasValue)
        {
            var openingBalanceResult = await PostOpeningBalanceAsync(
                ledger, account, command.OpeningBalance.Value, userIdResult.Value, ct);

            if (openingBalanceResult.IsFailure)
                return Result.Failure<CreateAccountResponse>(openingBalanceResult.Error);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new CreateAccountResponse(account.Id));
    }

    private async Task<Result> PostOpeningBalanceAsync(
        Ledger ledger,
        Account account,
        decimal openingBalance,
        UserId createdByUserId,
        CancellationToken ct)
    {
        var equityAccountResult = await equityBackingAccountProvisioner.GetOrCreateAsync(ledger.Id, dateTimeProvider.UtcNow, ct);
        if (equityAccountResult.IsFailure)
            return Result.Failure(equityAccountResult.Error);

        var equityAccount = equityAccountResult.Value;

        var amountResult = Money.Create(openingBalance);
        if (amountResult.IsFailure)
            return Result.Failure(amountResult.Error);

        var entryResult = JournalEntry.Create(ledger.Id, createdByUserId, DateOnly.FromDateTime(dateTimeProvider.UtcNow), "Opening balance");
        if (entryResult.IsFailure)
            return Result.Failure(entryResult.Error);

        var entry = entryResult.Value;

        // CreditCard and Loan are the user-creatable, credit-normal (liability) account types; an
        // opening balance on either represents debt already owed, so it's credited like any other
        // increase to a liability, with Equity taking the offsetting debit.
        var isCreditNormal = account.Type is AccountType.CreditCard or AccountType.Loan;

        var accountLineResult = entry.AddLine(account.Id, isCreditNormal ? EntryType.Credit : EntryType.Debit, amountResult.Value);
        if (accountLineResult.IsFailure)
            return Result.Failure(accountLineResult.Error);

        var equityLineResult = entry.AddLine(equityAccount.Id, isCreditNormal ? EntryType.Debit : EntryType.Credit, amountResult.Value);
        if (equityLineResult.IsFailure)
            return Result.Failure(equityLineResult.Error);

        var postResult = entry.Post(dateTimeProvider.UtcNow, ledger, [account, equityAccount]);
        if (postResult.IsFailure)
            return postResult;

        journalEntryRepository.Add(entry);
        return Result.Success();
    }
}
