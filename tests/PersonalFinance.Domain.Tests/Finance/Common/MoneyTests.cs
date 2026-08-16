using PersonalFinance.Domain.Finance.Common;

using Shouldly;

namespace PersonalFinance.Domain.Tests.Finance.Common;

public sealed class MoneyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Create_ShouldFail_WhenAmountIsNotPositive(decimal amount)
    {
        var result = Money.Create(amount);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Money.Invalid");
    }

    [Fact]
    public void Create_ShouldRoundToTwoDecimalPlaces_AwayFromZero()
    {
        var result = Money.Create(10.005m);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Amount.ShouldBe(10.01m);
    }

    [Fact]
    public void Zero_ShouldHaveAmountZero()
    {
        Money.Zero.Amount.ShouldBe(0m);
    }

    [Fact]
    public void Equals_ShouldBeTrue_WhenAmountsMatch()
    {
        var a = Money.Create(10m).Value;
        var b = Money.Create(10m).Value;

        a.Equals(b).ShouldBeTrue();
    }
}
