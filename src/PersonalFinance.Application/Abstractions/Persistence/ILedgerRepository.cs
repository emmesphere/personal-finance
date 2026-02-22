using PersonalFinance.Domain.Finance.Ledgers;

namespace PersonalFinance.Application.Abstractions.Persistence;

public interface ILedgerRepository
{
    Task<Ledger?> GetByIdAsync(Guid id, CancellationToken ct);
}

