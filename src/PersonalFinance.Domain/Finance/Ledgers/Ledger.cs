using PersonalFinance.BuildingBlocks.Domain;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Domain.Finance.Ledgers;

public sealed class Ledger : AggregateRoot
{
    private readonly List<LedgerMember> _members = new();

    public string Name { get; private set; } = string.Empty;
    public UserId OwnerUserId { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<LedgerMember> Members => _members.AsReadOnly();

    private Ledger() { }

    private Ledger(string name, UserId ownerUserId, DateTime createdAt)
    {
        Name = name;
        OwnerUserId = ownerUserId;
        CreatedAt = createdAt;
        IsActive = true;

        _members.Add(LedgerMember.Create(ownerUserId, createdAt));
    }

    public static Result<Ledger> Create(string name, UserId ownerUserId, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Ledger>(ResultError.Validation("Ledger.Name.Empty", "Ledger name is required."));

        return Result.Success(new Ledger(name.Trim(), ownerUserId, createdAt));
    }

    public bool IsMember(UserId userId)
        => _members.Any(m => m.UserId.Equals(userId));

    public Result AddMember(UserId userId, DateTime joinedAt)
    {
        if (IsMember(userId))
            return Result.Success();

        _members.Add(LedgerMember.Create(userId, joinedAt));
        return Result.Success();
    }

    public Result RemoveMember(UserId userId)
    {
        if (OwnerUserId.Equals(userId))
            return Result.Failure(ResultError.Conflict("Ledger.Member.Owner", "Owner cannot be removed."));

        if (_members.Count <= 1)
            return Result.Failure(ResultError.Conflict("Ledger.Member.Last", "Ledger must have at least one member."));

        var removed = _members.RemoveAll(m => m.UserId.Equals(userId));

        return removed > 0
            ? Result.Success()
            : Result.Failure(ResultError.NotFound("Ledger.Member.NotFound", "Member not found in ledger."));
    }

    public Result EnsureMember(UserId userId)
        => IsMember(userId)
            ? Result.Success()
            : Result.Failure(ResultError.Conflict("Ledger.Member.Required", "User is not a member of this ledger."));

    public Result Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(ResultError.Validation("Ledger.Name.Empty", "Ledger name is required."));

        Name = name.Trim();
        return Result.Success();
    }

    public Result Deactivate()
    {
        IsActive = false;
        return Result.Success();
    }
}