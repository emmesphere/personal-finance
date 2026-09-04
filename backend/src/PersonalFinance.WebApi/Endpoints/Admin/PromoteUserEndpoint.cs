using PersonalFinance.Application.Finance.Identity.PromoteUser;
using PersonalFinance.BuildingBlocks.Results;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Admin;

internal static class PromoteUserEndpoint
{
    public static IEndpointRouteBuilder MapPromoteUser(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPatch("/api/admin/users/{userId:guid}/promote", async (
            Guid userId,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(new PromoteUserCommand(userId), ct);
            return result.ToHttp();
        }).RequireAuthorization("AdminOnly");

        return endpoints;
    }
}
