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
using PersonalFinance.Domain.Finance.Ledgers;

namespace PersonalFinance.Application.Finance.Expenses.AddExpense;

public sealed class AddExpenseHandler(
    ILedgerRepository ledgerRepository,
    IAccountRepository accountRepository,
    ICategoryRepository categoryRepository,
    IJournalEntryRepository journalEntryRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    CategoryBackingAccountProvisioner backingAccountProvisioner,
    IValidator<AddExpenseCommand> validator)
{
    public async Task<Result<AddExpenseResponse>> HandleAsync(AddExpenseCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            return Result.Failure<AddExpenseResponse>(
                ResultError.Validation("Expense.Validation", validation.Errors[0].ErrorMessage));
        }

        var ledger = await ledgerRepository.GetByIdAsync(command.LedgerId, ct);
        if (ledger is null)
            return Result.Failure<AddExpenseResponse>(ResultError.NotFound("Ledger.NotFound", "Ledger not found."));

        var userIdResult = UserId.Create(currentUserService.UserId);
        if (userIdResult.IsFailure)
            return Result.Failure<AddExpenseResponse>(userIdResult.Error);

        var memberCheck = ledger.EnsureMember(userIdResult.Value);
        if (memberCheck.IsFailure)
            return Result.Failure<AddExpenseResponse>(memberCheck.Error);

        var category = await categoryRepository.GetByIdAsync(command.CategoryId, ct);
        if (category is null)
            return Result.Failure<AddExpenseResponse>(ResultError.NotFound("Category.NotFound", "Category not found."));

        if (!category.IsActive)
            return Result.Failure<AddExpenseResponse>(ResultError.Conflict("Category.Inactive", "Category is not active."));

        if (category.Kind != CategoryKind.Expense)
            return Result.Failure<AddExpenseResponse>(ResultError.Validation("Category.WrongKind", "Category must be an Expense category."));

        var paymentAccounts = await accountRepository.GetByIdsAsync(command.LedgerId, [command.PaymentAccountId], ct);
        var paymentAccount = paymentAccounts.SingleOrDefault();
        if (paymentAccount is null)
            return Result.Failure<AddExpenseResponse>(ResultError.NotFound("Account.NotFound", "Payment account not found in this ledger."));

        var installmentCount = command.InstallmentCount ?? 1;

        if (paymentAccount.Type != AccountType.CreditCard && installmentCount > 1)
        {
            return Result.Failure<AddExpenseResponse>(
                ResultError.Validation("Expense.InstallmentsRequireCreditCard", "Only Credit Card payments can be split into installments."));
        }

        var backingAccountResult = await backingAccountProvisioner.GetOrCreateAsync(command.LedgerId, category, dateTimeProvider.UtcNow, ct);
        if (backingAccountResult.IsFailure)
            return Result.Failure<AddExpenseResponse>(backingAccountResult.Error);

        var backingAccount = backingAccountResult.Value;

        var isCreditCard = paymentAccount.Type == AccountType.CreditCard;
        Guid? installmentPlanId = isCreditCard ? Guid.NewGuid() : null;

        var amounts = SplitAmount(command.Amount, installmentCount);
        JournalEntry? firstEntry = null;

        for (var i = 0; i < installmentCount; i++)
        {
            var installmentDate = command.Date.AddMonths(i);

            var entryResult = JournalEntry.Create(command.LedgerId, userIdResult.Value, installmentDate, command.Description ?? string.Empty);
            if (entryResult.IsFailure)
                return Result.Failure<AddExpenseResponse>(entryResult.Error);

            var entry = entryResult.Value;

            var amountResult = Money.Create(amounts[i]);
            if (amountResult.IsFailure)
                return Result.Failure<AddExpenseResponse>(amountResult.Error);

            var addDebitResult = entry.AddLine(backingAccount.Id, EntryType.Debit, amountResult.Value);
            if (addDebitResult.IsFailure)
                return Result.Failure<AddExpenseResponse>(addDebitResult.Error);

            var addCreditResult = entry.AddLine(paymentAccount.Id, EntryType.Credit, amountResult.Value);
            if (addCreditResult.IsFailure)
                return Result.Failure<AddExpenseResponse>(addCreditResult.Error);

            if (installmentPlanId.HasValue)
            {
                var assignResult = entry.AssignInstallment(installmentPlanId.Value, i + 1, installmentCount);
                if (assignResult.IsFailure)
                    return Result.Failure<AddExpenseResponse>(assignResult.Error);
            }

            var postResult = entry.Post(dateTimeProvider.UtcNow, ledger, [backingAccount, paymentAccount]);
            if (postResult.IsFailure)
                return Result.Failure<AddExpenseResponse>(postResult.Error);

            journalEntryRepository.Add(entry);
            firstEntry ??= entry;
        }

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new AddExpenseResponse(firstEntry!.Id, installmentPlanId, installmentCount));
    }

    private static decimal[] SplitAmount(decimal total, int installmentCount)
    {
        if (installmentCount == 1)
            return [total];

        var baseAmount = Math.Floor(total * 100m / installmentCount) / 100m;
        var firstAmount = total - (baseAmount * (installmentCount - 1));

        var amounts = new decimal[installmentCount];
        amounts[0] = firstAmount;
        for (var i = 1; i < installmentCount; i++)
            amounts[i] = baseAmount;

        return amounts;
    }
}
