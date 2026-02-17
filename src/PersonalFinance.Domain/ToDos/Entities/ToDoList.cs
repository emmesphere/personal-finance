using PersonalFinance.BuildingBlocks.Domain;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.ToDos.Events;

namespace PersonalFinance.Domain.ToDos.Entities;

public sealed class ToDoList : AggregateRoot
{
    private readonly List<ToDoItem> _items = new();
    public IReadOnlyCollection<ToDoItem> Items => _items.AsReadOnly();

    public Result<ToDoItem> AddItem(ToDoTitle title, DateTime eventTime)
    {
        var exists = _items.Exists(i => i.Equals(title));

        if (exists)
            return Result.Failure<ToDoItem>(ResultError.Conflict("DuplicateTitle", "There is already an item with same title"));


        var item = ToDoItem.Create(title);
        _items.Add(item);

        AddDomainEvent(new ToDoItemAddedDomainEvent(
            eventTime,
            Id,
            item.Id,
            item.Title.Value));

        return Result.Success(item);
    }
}
