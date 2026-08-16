using PersonalFinance.Application.Finance.JournalEntries.PostJournalEntry;
using PersonalFinance.BuildingBlocks.Exceptions;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.WebApi.Contracts;

using Wolverine;

namespace PersonalFinance.WebApi.Endpoints.JournalEntries;

internal static class PostJournalEntryEndpoint
{
    public static IEndpointRouteBuilder MapPostJournalEntry(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/ledgers/{ledgerId:guid}/journal-entry/post", async (
            Guid ledgerId,
            PostJournalEntryRequest request,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            try
            {
                var command = new PostJournalEntryCommand(ledgerId,
                    request.Date, request.Description,
                    request.Lines);
                Result<PostJournalEntryResponse> result =
                    await bus.InvokeAsync<Result<PostJournalEntryResponse>>(command, ct);

                return result.IsSuccess ? Results.Created() : result.ToHttp();
            }
            catch (DomainException de)
            {
                return Results.Problem(title: "Business rule violation", detail: de.Message,
                    statusCode: StatusCodes.Status400BadRequest);
            }
        });

        return endpoints;
    }
}
