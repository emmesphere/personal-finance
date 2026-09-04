using PersonalFinance.Application.Finance.Identity.DeactivateUser;
using PersonalFinance.BuildingBlocks.Results;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Admin;

internal static class DeactivateUserEndpoint
{
    public static IEndpointRouteBuilder MapDeactivateUser(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPatch("/api/admin/users/{userId:guid}/deactivate", async (
            Guid userId,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(new DeactivateUserCommand(userId), ct);
            return result.ToHttp();
        }).RequireAuthorization("AdminOnly");

        return endpoints;
    }
}
