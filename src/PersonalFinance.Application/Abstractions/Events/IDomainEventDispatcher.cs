using PersonalFinance.BuildingBlocks.Domain;

namespace PersonalFinance.Application.Abstractions.Events;
public  interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken);
}
