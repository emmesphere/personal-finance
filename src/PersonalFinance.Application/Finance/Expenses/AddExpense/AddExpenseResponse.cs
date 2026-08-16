namespace PersonalFinance.Application.Finance.Expenses.AddExpense;

public sealed record AddExpenseResponse(Guid JournalEntryId, Guid? InstallmentPlanId, int InstallmentCount);
