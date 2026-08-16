using PersonalFinance.Domain.Finance.Accounts;

namespace PersonalFinance.Application.Finance.Accounts.ListAccounts;

public sealed record AccountSummary(
    Guid Id,
    string Name,
    AccountType Type,
    int? DueDateDay,
    bool IsActive);
