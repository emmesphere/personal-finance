using PersonalFinance.BuildingBlocks.Domain;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Domain.Finance.Ledgers;

public sealed class LedgerMember : Entity
{
    public UserId UserId { get; }
    public Guid LedgerId { get; }
    public DateTime JoinedAt { get; }

    private LedgerMember()
    {
        UserId = default!;
        JoinedAt = default;
    }

    private LedgerMember(UserId userId, Guid ledgerId, DateTime joinedAt)
    {
        UserId = userId;
        JoinedAt = joinedAt;
        LedgerId = ledgerId;
    }

    internal static LedgerMember Create(UserId userId, Guid ledgerId, DateTime joinedAt)
        => new(userId, ledgerId, joinedAt);
}
