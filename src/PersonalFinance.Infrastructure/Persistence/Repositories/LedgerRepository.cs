using Microsoft.EntityFrameworkCore;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.Ledgers;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public sealed class LedgerRepository(PersonalFinanceDbContext context) : ILedgerRepository
{
    public async Task<Ledger?> GetByIdAsync(Guid id, CancellationToken ct)
        => await context.Ledgers.Include(l => l.Members)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken: ct);

    public async Task<List<Ledger>> ListByMemberUserIdAsync(Guid userId, CancellationToken ct)
    {
        var memberId = UserId.From(userId);

        return await context.Ledgers
            .Include(l => l.Members)
            .Where(l => l.Members.Any(m => m.UserId == memberId))
            .ToListAsync(ct);
    }

    public Task<int> CountAllAsync(CancellationToken ct) => context.Ledgers.CountAsync(ct);

    public void Add(Ledger ledger) => context.Ledgers.Add(ledger);
}
