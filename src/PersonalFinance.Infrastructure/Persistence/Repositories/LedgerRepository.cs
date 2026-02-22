using Microsoft.EntityFrameworkCore;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.Finance.Ledgers;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public sealed class LedgerRepository(PersonalFinanceDbContext context) : ILedgerRepository
{
    public async Task<Ledger?> GetByIdAsync(Guid id, CancellationToken ct)
        => await context.Ledgers.Include(l => l.Members)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken: ct);

}
