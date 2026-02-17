using PersonalFinance.Application.Abstractions.Persistence;

namespace PersonalFinance.Infrastructure.Persistence.InMemory;
public sealed class InMemoryUnitOfWork : IUnitOfWork, IDisposable
{
    public void Dispose()
    {
        
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(0);
    }
}

