using PersonalFinance.BuildingBlocks.Domain;

using Shouldly;

namespace PersonalFinance.BuildingBlocks.Tests.Domain;

public sealed class AggregateRootTests
{
    private sealed record DummyEvent(DateTime EventTime) : IDomainEvent;

    private sealed class DummyAggregate : AggregateRoot
    {
        public void Raise(IDomainEvent ev) => AddDomainEvent(ev);
    }

    [Fact]
    public void ShouldStoreAndClearDomainEvents()
    {
        var agg = new DummyAggregate();

        agg.DomainEvents.Count.ShouldBe(0);

        agg.Raise(new DummyEvent(DateTime.UtcNow));
        agg.DomainEvents.Count.ShouldBe(1);

        agg.ClearDomainEvents();
        agg.DomainEvents.Count.ShouldBe(0);
    }
}
