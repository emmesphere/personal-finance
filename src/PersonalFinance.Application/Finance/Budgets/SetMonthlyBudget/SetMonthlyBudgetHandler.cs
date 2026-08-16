using FluentValidation;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Application.Abstractions.Security;
using PersonalFinance.BuildingBlocks.Abstractions;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Budgets;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Application.Finance.Budgets.SetMonthlyBudget;

public sealed class SetMonthlyBudgetHandler(
    ILedgerRepository ledgerRepository,
    IMonthlyBudgetRepository monthlyBudgetRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IValidator<SetMonthlyBudgetCommand> validator)
{
    public async Task<Result<SetMonthlyBudgetResponse>> HandleAsync(SetMonthlyBudgetCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            return Result.Failure<SetMonthlyBudgetResponse>(
                ResultError.Validation("MonthlyBudget.Validation", validation.Errors[0].ErrorMessage));
        }

        var ledger = await ledgerRepository.GetByIdAsync(command.LedgerId, ct);
        if (ledger is null)
            return Result.Failure<SetMonthlyBudgetResponse>(ResultError.NotFound("Ledger.NotFound", "Ledger not found."));

        var userIdResult = UserId.Create(currentUserService.UserId);
        if (userIdResult.IsFailure)
            return Result.Failure<SetMonthlyBudgetResponse>(userIdResult.Error);

        var memberCheck = ledger.EnsureMember(userIdResult.Value);
        if (memberCheck.IsFailure)
            return Result.Failure<SetMonthlyBudgetResponse>(memberCheck.Error);

        var yearMonthResult = YearMonth.Create(command.Year, command.Month);
        if (yearMonthResult.IsFailure)
            return Result.Failure<SetMonthlyBudgetResponse>(yearMonthResult.Error);

        var amountResult = Money.Create(command.Amount);
        if (amountResult.IsFailure)
            return Result.Failure<SetMonthlyBudgetResponse>(amountResult.Error);

        var existing = await monthlyBudgetRepository.GetAsync(command.LedgerId, yearMonthResult.Value, ct);
        if (existing is not null)
        {
            existing.SetAmount(amountResult.Value, dateTimeProvider.UtcNow);
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success(new SetMonthlyBudgetResponse(existing.Id));
        }

        var budgetResult = MonthlyBudget.Create(command.LedgerId, yearMonthResult.Value, amountResult.Value, dateTimeProvider.UtcNow);
        if (budgetResult.IsFailure)
            return Result.Failure<SetMonthlyBudgetResponse>(budgetResult.Error);

        var budget = budgetResult.Value;
        monthlyBudgetRepository.Add(budget);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new SetMonthlyBudgetResponse(budget.Id));
    }
}
