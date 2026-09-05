using PersonalFinance.Application.Finance.Categories.ListCategories;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.Domain.Finance.Categories;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Categories;

internal static class ListCategoriesEndpoint
{
    public static IEndpointRouteBuilder MapListCategories(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/categories", async (
            CategoryKind? kind,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var query = new ListCategoriesQuery(kind);
            var result = await bus.InvokeAsync<Result<IReadOnlyCollection<CategorySummary>>>(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : result.ToHttp();
        });

        return endpoints;
    }
}
