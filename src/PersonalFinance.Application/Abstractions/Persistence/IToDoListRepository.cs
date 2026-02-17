using PersonalFinance.Domain.ToDos.Entities;

namespace PersonalFinance.Application.Abstractions.Persistence;

public interface IToDoListRepository
{
    Task<ToDoList?> GetDefaultAsync(CancellationToken ct);
    void Upsert(ToDoList list);
}
