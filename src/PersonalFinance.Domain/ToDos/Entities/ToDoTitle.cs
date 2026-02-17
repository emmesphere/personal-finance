using PersonalFinance.BuildingBlocks.Domain;
using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.Domain.ToDos.Entities;

public sealed class ToDoTitle : ValueObject
{
	public string Value { get; }
	protected override IEnumerable<object?> GetEqualityComponents()
	{
		yield return Value.ToUpperInvariant();
	}

	private ToDoTitle(string value) => Value = value;

	public static Result<ToDoTitle> Create(string title)
	{
		if (string.IsNullOrWhiteSpace(title))
			Result.Failure<ToDoTitle>(ResultError.Validation("EmptyTitle", "Title cannot be empty."));

		return Result.Success(new ToDoTitle(title));
	}

	public override string ToString()
	{
		return Value;
	}
}
