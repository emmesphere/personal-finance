namespace PersonalFinance.BuildingBlocks.Abstractions;
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
