using PersonalFinance.Domain.Finance.Common;

using Shouldly;

namespace PersonalFinance.Domain.Tests.Finance.Common;

public sealed class YearMonthTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void Create_ShouldFail_WhenMonthOutOfRange(int month)
    {
        var result = YearMonth.Create(2026, month);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("YearMonth.Month.Invalid");
    }

    [Fact]
    public void Create_ShouldFail_WhenYearIsNotPositive()
    {
        var result = YearMonth.Create(0, 5);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("YearMonth.Year.Invalid");
    }

    [Fact]
    public void Create_ShouldSucceed_ForValidYearAndMonth()
    {
        var result = YearMonth.Create(2026, 8);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Year.ShouldBe(2026);
        result.Value.Month.ShouldBe(8);
    }

    [Fact]
    public void ToInt_And_FromInt_ShouldRoundTrip()
    {
        var yearMonth = YearMonth.From(2026, 8);

        var roundTripped = YearMonth.FromInt(yearMonth.ToInt());

        roundTripped.ShouldBe(yearMonth);
    }

    [Fact]
    public void Equals_ShouldBeTrue_WhenYearAndMonthMatch()
    {
        YearMonth.From(2026, 8).Equals(YearMonth.From(2026, 8)).ShouldBeTrue();
    }

    [Fact]
    public void ToString_ShouldFormatAsYyyyDashMm()
    {
        YearMonth.From(2026, 8).ToString().ShouldBe("2026-08");
    }
}
