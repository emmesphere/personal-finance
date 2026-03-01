using PersonalFinance.Application.Abstractions.Events;
using PersonalFinance.BuildingBlocks.Domain;
using Wolverine;

namespace PersonalFinance.Infrastructure.Messaging;

public sealed class WolverineDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMessageBus _bus;

    public WolverineDomainEventDispatcher(IMessageBus bus) => _bus = bus;

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken)
    {
        if (events is null) return;

        foreach (var @event in events)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _bus.PublishAsync(@event);
        }
    }

}
