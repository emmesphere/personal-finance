using Microsoft.EntityFrameworkCore;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.Finance.Budgets;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public sealed class MonthlyBudgetRepository(PersonalFinanceDbContext context) : IMonthlyBudgetRepository
{
    public Task<MonthlyBudget?> GetAsync(Guid ledgerId, YearMonth yearMonth, CancellationToken ct)
        => context.MonthlyBudgets.FirstOrDefaultAsync(b => b.LedgerId == ledgerId && b.YearMonth == yearMonth, ct);

    public void Add(MonthlyBudget budget) => context.MonthlyBudgets.Add(budget);
}
