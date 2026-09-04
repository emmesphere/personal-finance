using PersonalFinance.Application.Finance.Budgets.SetMonthlyBudget;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.WebApi.Contracts;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Budgets;

internal static class SetMonthlyBudgetEndpoint
{
    public static IEndpointRouteBuilder MapSetMonthlyBudget(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/ledgers/{ledgerId:guid}/budgets/{year:int}/{month:int}", async (
            Guid ledgerId,
            int year,
            int month,
            SetMonthlyBudgetRequest request,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var command = new SetMonthlyBudgetCommand(ledgerId, year, month, request.Amount);
            var result = await bus.InvokeAsync<Result<SetMonthlyBudgetResponse>>(command, ct);

            return result.IsSuccess ? Results.Ok(result.Value) : result.ToHttp();
        });

        return endpoints;
    }
}
