using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Application.Abstractions.Persistence;

public sealed record CategoryAmount(Guid CategoryId, string CategoryName, decimal Amount);

public sealed record MonthlyAmount(int Month, decimal Amount);

public interface IFinanceReportQueries
{
    Task<decimal> GetTotalBalanceAsync(Guid ledgerId, CancellationToken ct);

    Task<IReadOnlyCollection<CategoryAmount>> GetExpensesByCategoryAsync(Guid ledgerId, YearMonth yearMonth, CancellationToken ct);

    Task<IReadOnlyCollection<MonthlyAmount>> GetYearlyExpenseTotalsAsync(Guid ledgerId, int year, CancellationToken ct);
}
