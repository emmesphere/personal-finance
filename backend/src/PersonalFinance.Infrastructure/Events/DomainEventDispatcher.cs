using PersonalFinance.Application.Abstractions.Events;
using PersonalFinance.BuildingBlocks.Domain;

using Microsoft.Extensions.Logging;

namespace PersonalFinance.Infrastructure.Events;
public partial class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(ILogger<DomainEventDispatcher> logger) => _logger = logger;
    public Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(events);

        cancellationToken.ThrowIfCancellationRequested();

        foreach (var item in events)
        {
            LogDomainEvent(_logger, item.GetType().Name, item.EventTime);
        }

        return Task.CompletedTask;
    }

    [LoggerMessage(
       EventId = 1,
       Level = LogLevel.Information,
       Message = "Domain Event: {EventType} at {EventTime}")]
    private static partial void LogDomainEvent(
       ILogger logger,
       string eventType,
       DateTime eventTime);
}
