using PersonalFinance.Application.Finance.Expenses.AddExpense;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.WebApi.Contracts;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.Expenses;

internal static class AddExpenseEndpoint
{
    public static IEndpointRouteBuilder MapAddExpense(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/ledgers/{ledgerId:guid}/expenses", async (
            Guid ledgerId,
            AddExpenseRequest request,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var command = new AddExpenseCommand(
                ledgerId,
                request.CategoryId,
                request.PaymentAccountId,
                request.Amount,
                request.Date,
                request.Description,
                request.InstallmentCount);

            var result = await bus.InvokeAsync<Result<AddExpenseResponse>>(command, ct);

            return result.IsSuccess ? Results.Created($"/api/journal-entries/{result.Value.JournalEntryId}", result.Value) : result.ToHttp();
        });

        return endpoints;
    }
}
