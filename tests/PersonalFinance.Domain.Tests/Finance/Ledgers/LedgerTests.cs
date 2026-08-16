using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.Ledgers;

using Shouldly;

namespace PersonalFinance.Domain.Tests.Finance.Ledgers;

public sealed class LedgerTests
{
    private static UserId NewUserId() => UserId.From(Guid.NewGuid());

    [Fact]
    public void Create_ShouldSucceed_AndAddOwnerAsMember()
    {
        var owner = NewUserId();

        var result = Ledger.Create("My Ledger", owner, DateTime.UtcNow);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Members.Count.ShouldBe(1);
        result.Value.IsMember(owner).ShouldBeTrue();
        result.Value.OwnerUserId.ShouldBe(owner);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenNameIsBlank(string name)
    {
        var result = Ledger.Create(name, NewUserId(), DateTime.UtcNow);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Ledger.Name.Empty");
    }

    [Fact]
    public void AddMember_ShouldBeIdempotent_WhenAlreadyMember()
    {
        var owner = NewUserId();
        var ledger = Ledger.Create("My Ledger", owner, DateTime.UtcNow).Value;

        var result = ledger.AddMember(owner, DateTime.UtcNow);

        result.IsSuccess.ShouldBeTrue();
        ledger.Members.Count.ShouldBe(1);
    }

    [Fact]
    public void AddMember_ShouldAddNewMember()
    {
        var owner = NewUserId();
        var other = NewUserId();
        var ledger = Ledger.Create("My Ledger", owner, DateTime.UtcNow).Value;

        var result = ledger.AddMember(other, DateTime.UtcNow);

        result.IsSuccess.ShouldBeTrue();
        ledger.Members.Count.ShouldBe(2);
        ledger.IsMember(other).ShouldBeTrue();
    }

    [Fact]
    public void RemoveMember_ShouldFail_WhenRemovingOwner()
    {
        var owner = NewUserId();
        var other = NewUserId();
        var ledger = Ledger.Create("My Ledger", owner, DateTime.UtcNow).Value;
        ledger.AddMember(other, DateTime.UtcNow);

        var result = ledger.RemoveMember(owner);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Ledger.Member.Owner");
    }

    [Fact]
    public void RemoveMember_ShouldRemoveNonOwner_WhenMoreThanOneMemberExists()
    {
        var owner = NewUserId();
        var other = NewUserId();
        var ledger = Ledger.Create("My Ledger", owner, DateTime.UtcNow).Value;
        ledger.AddMember(other, DateTime.UtcNow);

        var result = ledger.RemoveMember(other);

        result.IsSuccess.ShouldBeTrue();
        ledger.IsMember(other).ShouldBeFalse();
    }

    [Fact]
    public void RemoveMember_ShouldFail_WhenMemberNotFound()
    {
        var owner = NewUserId();
        var other = NewUserId();
        var stranger = NewUserId();
        var ledger = Ledger.Create("My Ledger", owner, DateTime.UtcNow).Value;
        ledger.AddMember(other, DateTime.UtcNow);

        var result = ledger.RemoveMember(stranger);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Ledger.Member.NotFound");
    }

    [Fact]
    public void EnsureMember_ShouldFail_WhenUserIsNotMember()
    {
        var owner = NewUserId();
        var stranger = NewUserId();
        var ledger = Ledger.Create("My Ledger", owner, DateTime.UtcNow).Value;

        var result = ledger.EnsureMember(stranger);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Ledger.Member.Required");
    }

    [Fact]
    public void Rename_ShouldFail_WhenNameIsBlank()
    {
        var ledger = Ledger.Create("My Ledger", NewUserId(), DateTime.UtcNow).Value;

        var result = ledger.Rename("   ");

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var ledger = Ledger.Create("My Ledger", NewUserId(), DateTime.UtcNow).Value;

        var result = ledger.Deactivate();

        result.IsSuccess.ShouldBeTrue();
        ledger.IsActive.ShouldBeFalse();
    }
}
