
using PersonalFinance.BuildingBlocks.Abstractions;

namespace PersonalFinance.Infrastructure.Time;
public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
