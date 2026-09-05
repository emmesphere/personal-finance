namespace PersonalFinance.Application.Finance.Incomes.AddIncome;

public sealed record AddIncomeCommand(
    Guid LedgerId,
    Guid CategoryId,
    Guid ReceivingAccountId,
    decimal Amount,
    DateOnly Date,
    string? Description);
