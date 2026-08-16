using PersonalFinance.Application.Finance.Identity.ListUsers;
using PersonalFinance.BuildingBlocks.Results;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Admin;

internal static class ListUsersEndpoint
{
    public static IEndpointRouteBuilder MapListUsers(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/admin/users", async (IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<IReadOnlyCollection<UserSummary>>>(new ListUsersQuery(), ct);

            return result.IsSuccess ? Results.Ok(result.Value) : result.ToHttp();
        }).RequireAuthorization("AdminOnly");

        return endpoints;
    }
}
