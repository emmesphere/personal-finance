using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.Application.Finance.Categories.ListCategories;

public sealed class ListCategoriesHandler(ICategoryRepository categoryRepository)
{
    public async Task<Result<IReadOnlyCollection<CategorySummary>>> HandleAsync(ListCategoriesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var categories = await categoryRepository.ListActiveByKindAsync(query.Kind, ct);

        IReadOnlyCollection<CategorySummary> summaries = categories
            .Select(c => new CategorySummary(c.Id, c.Name, c.Kind, c.IsSystemDefined))
            .ToArray();

        return Result.Success(summaries);
    }
}
