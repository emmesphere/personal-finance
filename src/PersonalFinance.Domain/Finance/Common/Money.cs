using System.Globalization;

using PersonalFinance.BuildingBlocks.Domain;
using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.Domain.Finance.Common;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }

    private Money() { }

    private Money(decimal amount) => Amount = amount;

    internal static Money From(decimal amount) => new(amount);

    public static Result<Money> Create(decimal amount)
    {
        if (amount <= 0m)
            return Result.Failure<Money>(ResultError.Validation("Money.Invalid", "Amount must be greater than zero."));

        var rounded = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        return Result.Success(new Money(rounded));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }

    public override string ToString() => Amount.ToString("0.00", CultureInfo.InvariantCulture);

}
