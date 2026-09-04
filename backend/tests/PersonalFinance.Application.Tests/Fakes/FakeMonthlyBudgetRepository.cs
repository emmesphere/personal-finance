using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.Finance.Budgets;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Application.Tests.Fakes;

public sealed class FakeMonthlyBudgetRepository : IMonthlyBudgetRepository
{
    private readonly List<MonthlyBudget> _budgets = [];

    public Task<MonthlyBudget?> GetAsync(Guid ledgerId, YearMonth yearMonth, CancellationToken ct)
        => Task.FromResult(_budgets.FirstOrDefault(b => b.LedgerId == ledgerId && b.YearMonth.Equals(yearMonth)));

    public void Add(MonthlyBudget budget) => _budgets.Add(budget);
}
