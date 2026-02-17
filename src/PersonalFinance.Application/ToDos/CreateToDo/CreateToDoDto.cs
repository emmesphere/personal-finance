namespace PersonalFinance.Application.ToDos.CreateToDo;

public sealed record CreateToDoDto(Guid ListId, Guid ItemId, string Title, bool IsDone);
