using PersonalFinance.Application.Finance.Incomes.AddIncome;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.WebApi.Contracts;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Incomes;

internal static class AddIncomeEndpoint
{
    public static IEndpointRouteBuilder MapAddIncome(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/ledgers/{ledgerId:guid}/incomes", async (
            Guid ledgerId,
            AddIncomeRequest request,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var command = new AddIncomeCommand(
                ledgerId, request.CategoryId, request.ReceivingAccountId, request.Amount, request.Date, request.Description);

            var result = await bus.InvokeAsync<Result<AddIncomeResponse>>(command, ct);

            return result.IsSuccess ? Results.Created($"/api/journal-entries/{result.Value.JournalEntryId}", result.Value) : result.ToHttp();
        });

        return endpoints;
    }
}
