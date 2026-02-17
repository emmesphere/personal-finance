using PersonalFinance.BuildingBlocks.Domain;

using Shouldly;

namespace PersonalFinance.BuildingBlocks.Tests.Domain;
public sealed class ValueObjectTests
{
    private sealed class Money : ValueObject
    {
        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public decimal Amount { get; }
        public string Currency { get; }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    [Fact]
    public void Equals_ShouldBeTrue_WhenComponentsMatch()
    {
        var a = new Money(10m, "EUR");
        var b = new Money(10m, "EUR");

        a.Equals(b).ShouldBeTrue();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Fact]
    public void Equals_ShouldBeFalse_WhenComponentsDiffer()
    {
        var a = new Money(10m, "EUR");
        var b = new Money(11m, "EUR");

        a.Equals(b).ShouldBeFalse();
    }

}
