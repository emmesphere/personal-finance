using PersonalFinance.Application.Finance.Reports.GetYearlySummary;
using PersonalFinance.BuildingBlocks.Results;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Reports;

internal static class GetYearlySummaryEndpoint
{
    public static IEndpointRouteBuilder MapGetYearlySummary(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/ledgers/{ledgerId:guid}/reports/yearly-summary", async (
            Guid ledgerId,
            int year,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var query = new GetYearlySummaryQuery(ledgerId, year);
            var result = await bus.InvokeAsync<Result<GetYearlySummaryResponse>>(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : result.ToHttp();
        });

        return endpoints;
    }
}
