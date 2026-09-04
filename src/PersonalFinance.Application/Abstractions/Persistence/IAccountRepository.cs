using PersonalFinance.Domain.Finance.Accounts;

namespace PersonalFinance.Application.Abstractions.Persistence;

public interface IAccountRepository
{
    Task<List<Account>> GetByIdsAsync(Guid ledgerId, IEnumerable<Guid> ids, CancellationToken ct);

    Task<List<Account>> ListByLedgerAsync(Guid ledgerId, CancellationToken ct);

    Task<bool> ExistsByNameAsync(Guid ledgerId, string name, CancellationToken ct);

    Task<Account?> GetCategoryBackingAccountAsync(Guid ledgerId, Guid categoryId, CancellationToken ct);

    Task<Account?> GetEquityBackingAccountAsync(Guid ledgerId, CancellationToken ct);

    void Add(Account account);
}
