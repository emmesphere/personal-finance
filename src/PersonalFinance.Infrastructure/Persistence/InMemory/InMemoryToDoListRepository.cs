using System.Collections.Concurrent;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.ToDos.Entities;

namespace PersonalFinance.Infrastructure.Persistence.InMemory;

public sealed class InMemoryToDoListRepository : IToDoListRepository
{
    private const string DefaultKey = "default";
    private static readonly ConcurrentDictionary<string, ToDoList> Store = new();

    public Task<ToDoList?> GetDefaultAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Store.TryGetValue(DefaultKey, out var list);
        return Task.FromResult<ToDoList?>(list);
    }

    public void Upsert(ToDoList list)
    {
        ArgumentNullException.ThrowIfNull(list);
        Store[DefaultKey] = list;
    }
}

