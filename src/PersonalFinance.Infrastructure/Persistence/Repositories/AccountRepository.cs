using Microsoft.EntityFrameworkCore;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.Finance.Accounts;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository(PersonalFinanceDbContext context) : IAccountRepository
{
    public async Task<List<Account>> GetByIdsAsync(Guid ledgerId, IEnumerable<Guid> ids, CancellationToken ct) => await context.Accounts
            .Where(a => a.LedgerId == ledgerId && ids.Contains(a.Id))
            .ToListAsync(ct);

}
