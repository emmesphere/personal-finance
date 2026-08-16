using PersonalFinance.Domain.Finance.Accounts;

namespace PersonalFinance.Application.Finance.Accounts.CreateAccount;

public sealed record CreateAccountCommand(
    Guid LedgerId,
    string Name,
    AccountType Type,
    int? DueDateDay);
