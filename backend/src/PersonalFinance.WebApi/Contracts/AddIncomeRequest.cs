namespace PersonalFinance.WebApi.Contracts;

internal sealed record AddIncomeRequest(
    Guid CategoryId,
    Guid ReceivingAccountId,
    decimal Amount,
    DateOnly Date,
    string? Description);
