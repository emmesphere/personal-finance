using PersonalFinance.BuildingBlocks.Domain;

namespace PersonalFinance.Domain.ToDos.Entities;

public sealed class ToDoItem : Entity
{
	public ToDoTitle Title { get; }
	public bool IsDone { get; private set; }

	private ToDoItem(ToDoTitle title) => Title = title;

	public static ToDoItem Create(ToDoTitle title) => new(title);

	public void MarkDone() => IsDone = true;
}
