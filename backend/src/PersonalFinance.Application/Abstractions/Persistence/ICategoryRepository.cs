using PersonalFinance.Domain.Finance.Categories;

namespace PersonalFinance.Application.Abstractions.Persistence;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<List<Category>> ListActiveByKindAsync(CategoryKind? kind, CancellationToken ct);

    void Add(Category category);
}
