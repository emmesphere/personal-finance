using PersonalFinance.Application.Finance.Categories.DeactivateCategory;
using PersonalFinance.BuildingBlocks.Results;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Categories;

internal static class DeactivateCategoryEndpoint
{
    public static IEndpointRouteBuilder MapDeactivateCategory(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPatch("/api/categories/{categoryId:guid}/deactivate", async (
            Guid categoryId,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var command = new DeactivateCategoryCommand(categoryId);
            var result = await bus.InvokeAsync<Result>(command, ct);

            return result.ToHttp();
        });

        return endpoints;
    }
}
