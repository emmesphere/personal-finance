using PersonalFinance.Application.Abstractions.Persistence;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;
public sealed class UnitOfWork(PersonalFinanceDbContext context) : IUnitOfWork
{

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await context.SaveChangesAsync(ct);
}
