using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.Finance.Accounts;

namespace PersonalFinance.Application.Tests.Fakes;

public sealed class FakeAccountRepository : IAccountRepository
{
    private readonly List<Account> _accounts = [];

    public void Seed(params Account[] accounts) => _accounts.AddRange(accounts);

    public Task<List<Account>> GetByIdsAsync(Guid ledgerId, IEnumerable<Guid> ids, CancellationToken ct)
    {
        var idSet = ids.ToHashSet();
        return Task.FromResult(_accounts.Where(a => a.LedgerId == ledgerId && idSet.Contains(a.Id)).ToList());
    }

    public Task<List<Account>> ListByLedgerAsync(Guid ledgerId, CancellationToken ct)
        => Task.FromResult(_accounts.Where(a => a.LedgerId == ledgerId).ToList());

    public Task<bool> ExistsByNameAsync(Guid ledgerId, string name, CancellationToken ct)
        => Task.FromResult(_accounts.Exists(a => a.LedgerId == ledgerId && a.Name == name));

    public Task<Account?> GetCategoryBackingAccountAsync(Guid ledgerId, Guid categoryId, CancellationToken ct)
        => Task.FromResult(_accounts.FirstOrDefault(a => a.LedgerId == ledgerId && a.CategoryId == categoryId));

    public void Add(Account account) => _accounts.Add(account);
}
