using Microsoft.AspNetCore.Mvc;

using PersonalFinance.Application.Abstractions.Messaging;
using PersonalFinance.Application.Finance.JournalEntries.PostJournalEntry;
using PersonalFinance.BuildingBlocks.Exceptions;
using PersonalFinance.BuildingBlocks.Results;
using PersonalFinance.WebApi.Contracts;

namespace PersonalFinance.WebApi.Endpoints.JournalEntries;

internal static class PostJournalEntryEndpoint
{
    public static IEndpointRouteBuilder MapPostJournalEntry(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/ledgers/{ledgerId:guid}/journal-entry/post", async (
            Guid ledgerId, 
            PostJournalEntryRequest request,
            ICommandDispatcher dispatcher,
            CancellationToken ct) =>
        {
            try
            {
                var command = new PostJournalEntryCommand(ledgerId, 
                    request.CreatedByUserId, request.Date, request.Description, 
                    request.Lines);
                var result = await dispatcher.Dispatch(command, ct);
                return result.ToHttp();
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
