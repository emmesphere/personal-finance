using PersonalFinance.Domain.Finance.Budgets;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Application.Abstractions.Persistence;

public interface IMonthlyBudgetRepository
{
    Task<MonthlyBudget?> GetAsync(Guid ledgerId, YearMonth yearMonth, CancellationToken ct);

    void Add(MonthlyBudget budget);
}
