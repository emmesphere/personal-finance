using PersonalFinance.Application.Finance.Identity.DemoteUser;
using PersonalFinance.BuildingBlocks.Results;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Admin;

internal static class DemoteUserEndpoint
{
    public static IEndpointRouteBuilder MapDemoteUser(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPatch("/api/admin/users/{userId:guid}/demote", async (
            Guid userId,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(new DemoteUserCommand(userId), ct);
            return result.ToHttp();
        }).RequireAuthorization("AdminOnly");

        return endpoints;
    }
}
