using PersonalFinance.Domain.Finance.Common;

using Shouldly;

namespace PersonalFinance.Domain.Tests.Finance.Common;

public sealed class UserIdTests
{
    [Fact]
    public void Create_ShouldFail_WhenGuidIsEmpty()
    {
        var result = UserId.Create(Guid.Empty);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("UserId.Empty");
    }

    [Fact]
    public void Create_ShouldSucceed_WhenGuidIsNotEmpty()
    {
        var guid = Guid.NewGuid();

        var result = UserId.Create(guid);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(guid);
    }

    [Fact]
    public void Equals_ShouldBeTrue_WhenValuesMatch()
    {
        var guid = Guid.NewGuid();

        UserId.From(guid).Equals(UserId.From(guid)).ShouldBeTrue();
    }
}
