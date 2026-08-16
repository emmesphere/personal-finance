using Microsoft.EntityFrameworkCore;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.Finance.Accounts;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository(PersonalFinanceDbContext context) : IAccountRepository
{
    public async Task<List<Account>> GetByIdsAsync(Guid ledgerId, IEnumerable<Guid> ids, CancellationToken ct) => await context.Accounts
            .Where(a => a.LedgerId == ledgerId && ids.Contains(a.Id))
            .ToListAsync(ct);

    public async Task<List<Account>> ListByLedgerAsync(Guid ledgerId, CancellationToken ct) => await context.Accounts
            .Where(a => a.LedgerId == ledgerId)
            .ToListAsync(ct);

    public Task<bool> ExistsByNameAsync(Guid ledgerId, string name, CancellationToken ct)
        => context.Accounts.AnyAsync(a => a.LedgerId == ledgerId && a.Name == name, ct);

    public Task<Account?> GetCategoryBackingAccountAsync(Guid ledgerId, Guid categoryId, CancellationToken ct)
        => context.Accounts.FirstOrDefaultAsync(a => a.LedgerId == ledgerId && a.CategoryId == categoryId, ct);

    public void Add(Account account) => context.Accounts.Add(account);
}
