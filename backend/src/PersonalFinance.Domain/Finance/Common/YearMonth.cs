using System.Globalization;

using PersonalFinance.BuildingBlocks.Domain;
using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.Domain.Finance.Common;

public sealed class YearMonth : ValueObject
{
    public int Year { get; }
    public int Month { get; }

    private YearMonth() { }

    private YearMonth(int year, int month)
    {
        Year = year;
        Month = month;
    }

    public static YearMonth From(int year, int month) => new(year, month);

    public static Result<YearMonth> Create(int year, int month)
    {
        if (year < 1)
            return Result.Failure<YearMonth>(ResultError.Validation("YearMonth.Year.Invalid", "Year must be a positive number."));

        if (month is < 1 or > 12)
            return Result.Failure<YearMonth>(ResultError.Validation("YearMonth.Month.Invalid", "Month must be between 1 and 12."));

        return Result.Success(new YearMonth(year, month));
    }

    public int ToInt() => (Year * 100) + Month;

    public static YearMonth FromInt(int value) => new(value / 100, value % 100);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Year;
        yield return Month;
    }

    public override string ToString() => $"{Year.ToString("D4", CultureInfo.InvariantCulture)}-{Month.ToString("D2", CultureInfo.InvariantCulture)}";
}
