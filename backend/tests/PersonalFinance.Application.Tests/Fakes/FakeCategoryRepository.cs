using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.Finance.Categories;

namespace PersonalFinance.Application.Tests.Fakes;

public sealed class FakeCategoryRepository : ICategoryRepository
{
    private readonly List<Category> _categories = [];

    public void Seed(Category category) => _categories.Add(category);

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_categories.FirstOrDefault(c => c.Id == id));

    public Task<List<Category>> ListActiveByKindAsync(CategoryKind? kind, CancellationToken ct)
    {
        var query = _categories.Where(c => c.IsActive);
        if (kind.HasValue)
            query = query.Where(c => c.Kind == kind.Value);

        return Task.FromResult(query.ToList());
    }

    public void Add(Category category) => _categories.Add(category);
}
