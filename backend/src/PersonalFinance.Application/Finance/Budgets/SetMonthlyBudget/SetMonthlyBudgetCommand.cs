namespace PersonalFinance.Application.Finance.Budgets.SetMonthlyBudget;

public sealed record SetMonthlyBudgetCommand(Guid LedgerId, int Year, int Month, decimal Amount);
