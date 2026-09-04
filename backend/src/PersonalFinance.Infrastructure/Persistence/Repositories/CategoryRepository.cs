using Microsoft.EntityFrameworkCore;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.Finance.Categories;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(PersonalFinanceDbContext context) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct)
        => context.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<List<Category>> ListActiveByKindAsync(CategoryKind? kind, CancellationToken ct)
    {
        var query = context.Categories.Where(c => c.IsActive);

        if (kind.HasValue)
            query = query.Where(c => c.Kind == kind.Value);

        return await query.ToListAsync(ct);
    }

    public void Add(Category category) => context.Categories.Add(category);
}
