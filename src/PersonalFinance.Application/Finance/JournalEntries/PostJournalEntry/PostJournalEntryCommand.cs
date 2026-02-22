using System.Collections.ObjectModel;

using PersonalFinance.Application.Abstractions.Messaging;
using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.Application.Finance.JournalEntries.PostJournalEntry;

public sealed record PostJournalEntryCommand(
    Guid LedgerId,
    Guid CreatedByUserId,
    DateOnly Date,
    string Description,
    IReadOnlyCollection<PostJournalEntryLineDto> Lines
) : ICommand<Result<PostJournalEntryResponse>>;
