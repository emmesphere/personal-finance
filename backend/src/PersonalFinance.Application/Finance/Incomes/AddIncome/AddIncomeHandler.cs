using FluentValidation;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Application.Abstractions.Security;
using PersonalFinance.Application.Finance.Common;
using PersonalFinance.BuildingBlocks.Abstractions;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.Categories;
using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.JournalEntries;

namespace PersonalFinance.Application.Finance.Incomes.AddIncome;

public sealed class AddIncomeHandler(
    ILedgerRepository ledgerRepository,
    IAccountRepository accountRepository,
    ICategoryRepository categoryRepository,
    IJournalEntryRepository journalEntryRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    CategoryBackingAccountProvisioner backingAccountProvisioner,
    IValidator<AddIncomeCommand> validator)
{
    private static readonly AccountType[] EligibleReceivingAccountTypes =
    [
        AccountType.BankAccount,
        AccountType.Wallet,
        AccountType.Benefit,
        AccountType.Debit,
    ];

    public async Task<Result<AddIncomeResponse>> HandleAsync(AddIncomeCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            return Result.Failure<AddIncomeResponse>(
                ResultError.Validation("Income.Validation", validation.Errors[0].ErrorMessage));
        }

        var ledger = await ledgerRepository.GetByIdAsync(command.LedgerId, ct);
        if (ledger is null)
            return Result.Failure<AddIncomeResponse>(ResultError.NotFound("Ledger.NotFound", "Ledger not found."));

        var userIdResult = UserId.Create(currentUserService.UserId);
        if (userIdResult.IsFailure)
            return Result.Failure<AddIncomeResponse>(userIdResult.Error);

        var memberCheck = ledger.EnsureMember(userIdResult.Value);
        if (memberCheck.IsFailure)
            return Result.Failure<AddIncomeResponse>(memberCheck.Error);

        var category = await categoryRepository.GetByIdAsync(command.CategoryId, ct);
        if (category is null)
            return Result.Failure<AddIncomeResponse>(ResultError.NotFound("Category.NotFound", "Category not found."));

        if (!category.IsActive)
            return Result.Failure<AddIncomeResponse>(ResultError.Conflict("Category.Inactive", "Category is not active."));

        if (category.Kind != CategoryKind.Income)
            return Result.Failure<AddIncomeResponse>(ResultError.Validation("Category.WrongKind", "Category must be an Income category."));

        var receivingAccounts = await accountRepository.GetByIdsAsync(command.LedgerId, [command.ReceivingAccountId], ct);
        var receivingAccount = receivingAccounts.SingleOrDefault();
        if (receivingAccount is null)
            return Result.Failure<AddIncomeResponse>(ResultError.NotFound("Account.NotFound", "Receiving account not found in this ledger."));

        if (!EligibleReceivingAccountTypes.Contains(receivingAccount.Type))
        {
            return Result.Failure<AddIncomeResponse>(
                ResultError.Validation("Account.NotEligibleForIncome", "Income cannot be received into this account type."));
        }

        var backingAccountResult = await backingAccountProvisioner.GetOrCreateAsync(command.LedgerId, category, dateTimeProvider.UtcNow, ct);
        if (backingAccountResult.IsFailure)
            return Result.Failure<AddIncomeResponse>(backingAccountResult.Error);

        var backingAccount = backingAccountResult.Value;

        var entryResult = JournalEntry.Create(command.LedgerId, userIdResult.Value, command.Date, command.Description ?? string.Empty);
        if (entryResult.IsFailure)
            return Result.Failure<AddIncomeResponse>(entryResult.Error);

        var entry = entryResult.Value;

        var amountResult = Money.Create(command.Amount);
        if (amountResult.IsFailure)
            return Result.Failure<AddIncomeResponse>(amountResult.Error);

        var addDebitResult = entry.AddLine(receivingAccount.Id, EntryType.Debit, amountResult.Value);
        if (addDebitResult.IsFailure)
            return Result.Failure<AddIncomeResponse>(addDebitResult.Error);

        var addCreditResult = entry.AddLine(backingAccount.Id, EntryType.Credit, amountResult.Value);
        if (addCreditResult.IsFailure)
            return Result.Failure<AddIncomeResponse>(addCreditResult.Error);

        var postResult = entry.Post(dateTimeProvider.UtcNow, ledger, [receivingAccount, backingAccount]);
        if (postResult.IsFailure)
            return Result.Failure<AddIncomeResponse>(postResult.Error);

        journalEntryRepository.Add(entry);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new AddIncomeResponse(entry.Id));
    }
}
