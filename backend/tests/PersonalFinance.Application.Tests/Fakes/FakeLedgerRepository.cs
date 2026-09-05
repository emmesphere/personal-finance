using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.Ledgers;

namespace PersonalFinance.Application.Tests.Fakes;

public sealed class FakeLedgerRepository : ILedgerRepository
{
    private readonly Dictionary<Guid, Ledger> _ledgers = [];

    public void Seed(Ledger ledger)
    {
        ArgumentNullException.ThrowIfNull(ledger);
        _ledgers[ledger.Id] = ledger;
    }

    public Task<Ledger?> GetByIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_ledgers.GetValueOrDefault(id));

    public Task<List<Ledger>> ListByMemberUserIdAsync(Guid userId, CancellationToken ct)
    {
        var memberId = UserId.From(userId);
        return Task.FromResult(_ledgers.Values.Where(l => l.IsMember(memberId)).ToList());
    }

    public Task<int> CountAllAsync(CancellationToken ct) => Task.FromResult(_ledgers.Count);

    public void Add(Ledger ledger)
    {
        ArgumentNullException.ThrowIfNull(ledger);
        _ledgers[ledger.Id] = ledger;
    }
}
