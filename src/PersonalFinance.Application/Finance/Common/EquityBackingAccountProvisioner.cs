using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Accounts;

namespace PersonalFinance.Application.Finance.Common;

public sealed class EquityBackingAccountProvisioner(IAccountRepository accountRepository)
{
    public const string AccountName = "Opening Balance Equity";

    public async Task<Result<Account>> GetOrCreateAsync(Guid ledgerId, DateTime createdAt, CancellationToken ct)
    {
        var existing = await accountRepository.GetEquityBackingAccountAsync(ledgerId, ct);
        if (existing is not null)
            return Result.Success(existing);

        var accountResult = Account.Create(ledgerId, AccountName, AccountType.Equity, dueDate: null, categoryId: null, createdAt);
        if (accountResult.IsFailure)
            return accountResult;

        accountRepository.Add(accountResult.Value);
        return accountResult;
    }
}
