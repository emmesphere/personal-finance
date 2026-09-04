using PersonalFinance.Domain.Finance.Ledgers;

namespace PersonalFinance.Application.Abstractions.Persistence;

public interface ILedgerRepository
{
    Task<Ledger?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<List<Ledger>> ListByMemberUserIdAsync(Guid userId, CancellationToken ct);

    Task<int> CountAllAsync(CancellationToken ct);

    void Add(Ledger ledger);
}

