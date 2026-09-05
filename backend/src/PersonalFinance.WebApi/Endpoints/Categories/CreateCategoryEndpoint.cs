using PersonalFinance.Application.Finance.Categories.CreateCategory;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.WebApi.Contracts;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Categories;

internal static class CreateCategoryEndpoint
{
    public static IEndpointRouteBuilder MapCreateCategory(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/categories", async (
            CreateCategoryRequest request,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var command = new CreateCategoryCommand(request.Name, request.Kind);
            var result = await bus.InvokeAsync<Result<CreateCategoryResponse>>(command, ct);

            return result.IsSuccess
                ? Results.Created($"/api/categories/{result.Value.CategoryId}", result.Value)
                : result.ToHttp();
        });

        return endpoints;
    }
}
