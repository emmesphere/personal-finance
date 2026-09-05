using PersonalFinance.Domain.Finance.Accounts;

namespace PersonalFinance.WebApi.Contracts;

internal sealed record CreateAccountRequest(string Name, AccountType Type, int? DueDateDay, decimal? OpeningBalance);
