using PersonalFinance.Domain.Finance.Accounts;

namespace PersonalFinance.Application.Abstractions.Persistence;

public interface IAccountRepository
{
    Task<List<Account>> GetByIdsAsync(Guid ledgerId, IEnumerable<Guid> ids, CancellationToken ct);
}

