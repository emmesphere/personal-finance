namespace PersonalFinance.WebApi.Contracts;

internal sealed record AddExpenseRequest(
    Guid CategoryId,
    Guid PaymentAccountId,
    decimal Amount,
    DateOnly Date,
    string? Description,
    int? InstallmentCount);
