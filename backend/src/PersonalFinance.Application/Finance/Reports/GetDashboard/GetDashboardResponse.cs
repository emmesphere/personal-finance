using PersonalFinance.Application.Abstractions.Persistence;

namespace PersonalFinance.Application.Finance.Reports.GetDashboard;

public sealed record GetDashboardResponse(
    decimal TotalBalance,
    decimal TotalExpenses,
    decimal? BudgetAmount,
    IReadOnlyCollection<CategoryAmount> ExpensesByCategory);
