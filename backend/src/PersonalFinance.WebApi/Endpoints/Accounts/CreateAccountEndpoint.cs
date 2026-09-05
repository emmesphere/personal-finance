using PersonalFinance.Application.Finance.Accounts.CreateAccount;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.WebApi.Contracts;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Accounts;

internal static class CreateAccountEndpoint
{
    public static IEndpointRouteBuilder MapCreateAccount(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/ledgers/{ledgerId:guid}/accounts", async (
            Guid ledgerId,
            CreateAccountRequest request,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var command = new CreateAccountCommand(ledgerId, request.Name, request.Type, request.DueDateDay, request.OpeningBalance);
            var result = await bus.InvokeAsync<Result<CreateAccountResponse>>(command, ct);

            return result.IsSuccess
                ? Results.Created($"/api/ledgers/{ledgerId}/accounts/{result.Value.AccountId}", result.Value)
                : result.ToHttp();
        });

        return endpoints;
    }
}
