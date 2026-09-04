using PersonalFinance.Application.Finance.Reports.GetDashboard;
using PersonalFinance.BuildingBlocks.Results;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Reports;

internal static class GetDashboardEndpoint
{
    public static IEndpointRouteBuilder MapGetDashboard(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/ledgers/{ledgerId:guid}/reports/dashboard", async (
            Guid ledgerId,
            int year,
            int month,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var query = new GetDashboardQuery(ledgerId, year, month);
            var result = await bus.InvokeAsync<Result<GetDashboardResponse>>(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : result.ToHttp();
        });

        return endpoints;
    }
}
