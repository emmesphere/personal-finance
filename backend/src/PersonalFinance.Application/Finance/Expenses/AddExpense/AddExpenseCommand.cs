namespace PersonalFinance.Application.Finance.Expenses.AddExpense;

public sealed record AddExpenseCommand(
    Guid LedgerId,
    Guid CategoryId,
    Guid PaymentAccountId,
    decimal Amount,
    DateOnly Date,
    string? Description,
    int? InstallmentCount);
