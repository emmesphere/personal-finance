using System.Globalization;

using PersonalFinance.BuildingBlocks.Domain;
using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.Domain.Finance.Accounts;

public sealed class DueDate : ValueObject
{
    public int Day { get; }

    private DueDate(int day) => Day = day;

    public static Result<DueDate> Create(int day)
    {
        if (day is < 1 or > 31)
            return Result.Failure<DueDate>(ResultError.Validation("DueDate.Invalid", "DueDate day must be between 1 and 31."));

        return Result.Success(new DueDate(day));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Day;
    }

    public override string ToString() => Day.ToString(CultureInfo.InvariantCulture);
}
