using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Application.Abstractions.Security;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Application.Finance.Reports.GetDashboard;

public sealed class GetDashboardHandler(
    ILedgerRepository ledgerRepository,
    IMonthlyBudgetRepository monthlyBudgetRepository,
    IFinanceReportQueries reportQueries,
    ICurrentUserService currentUserService)
{
    public async Task<Result<GetDashboardResponse>> HandleAsync(GetDashboardQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var ledger = await ledgerRepository.GetByIdAsync(query.LedgerId, ct);
        if (ledger is null)
            return Result.Failure<GetDashboardResponse>(ResultError.NotFound("Ledger.NotFound", "Ledger not found."));

        var userIdResult = UserId.Create(currentUserService.UserId);
        if (userIdResult.IsFailure)
            return Result.Failure<GetDashboardResponse>(userIdResult.Error);

        var memberCheck = ledger.EnsureMember(userIdResult.Value);
        if (memberCheck.IsFailure)
            return Result.Failure<GetDashboardResponse>(memberCheck.Error);

        var yearMonthResult = YearMonth.Create(query.Year, query.Month);
        if (yearMonthResult.IsFailure)
            return Result.Failure<GetDashboardResponse>(yearMonthResult.Error);

        var yearMonth = yearMonthResult.Value;

        var totalBalance = await reportQueries.GetTotalBalanceAsync(query.LedgerId, ct);
        var expensesByCategory = await reportQueries.GetExpensesByCategoryAsync(query.LedgerId, yearMonth, ct);
        var budget = await monthlyBudgetRepository.GetAsync(query.LedgerId, yearMonth, ct);

        var totalExpenses = expensesByCategory.Sum(c => c.Amount);

        return Result.Success(new GetDashboardResponse(
            totalBalance,
            totalExpenses,
            budget?.Amount.Amount,
            expensesByCategory));
    }
}
