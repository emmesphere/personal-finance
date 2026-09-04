using PersonalFinance.Application.Finance.Accounts.ListAccounts;
using PersonalFinance.BuildingBlocks.Results;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Accounts;

internal static class ListAccountsEndpoint
{
    public static IEndpointRouteBuilder MapListAccounts(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/ledgers/{ledgerId:guid}/accounts", async (
            Guid ledgerId,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var query = new ListAccountsQuery(ledgerId);
            var result = await bus.InvokeAsync<Result<IReadOnlyCollection<AccountSummary>>>(query, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : result.ToHttp();
        });

        return endpoints;
    }
}
