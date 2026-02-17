using PersonalFinance.BuildingBlocks.Domain;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Domain.Finance.Ledgers;

public sealed class LedgerMember : Entity
{
    public UserId UserId { get; }
    public DateTime JoinedAt { get; }
    private LedgerMember(UserId userId, DateTime joinedAt)
    {
        UserId = userId;
        JoinedAt = joinedAt;
    }
    internal static LedgerMember Create(UserId userId, DateTime joinedAt)
        => new(userId, joinedAt);
}
