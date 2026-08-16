using PersonalFinance.Application.Finance.Reports.GetAdminSummary;
using PersonalFinance.BuildingBlocks.Results;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Admin;

internal static class GetAdminSummaryEndpoint
{
    public static IEndpointRouteBuilder MapGetAdminSummary(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/admin/summary", async (IMessageBus bus, CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<GetAdminSummaryResponse>>(new GetAdminSummaryQuery(), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToHttp();
        }).RequireAuthorization("AdminOnly");

        return endpoints;
    }
}
