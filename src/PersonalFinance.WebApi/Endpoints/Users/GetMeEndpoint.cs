using PersonalFinance.Application.Finance.Identity.GetMe;
using PersonalFinance.BuildingBlocks.Results;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Users;

internal static class GetMeEndpoint
{
    public static IEndpointRouteBuilder MapGetMe(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/me", async (IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<MeResponse>>(new GetMeQuery(), ct);

            return result.IsSuccess ? Results.Ok(result.Value) : result.ToHttp();
        });

        return endpoints;
    }
}
