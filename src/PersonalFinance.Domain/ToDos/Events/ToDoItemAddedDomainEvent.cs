using PersonalFinance.BuildingBlocks.Domain;

namespace PersonalFinance.Domain.ToDos.Events;
public sealed record ToDoItemAddedDomainEvent(
    DateTime EventTime,
    Guid ListId,
    Guid ItemId,
    string Title
) : IDomainEvent;
