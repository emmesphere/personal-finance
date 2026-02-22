using System.Collections.ObjectModel;

using PersonalFinance.Application.Finance.JournalEntries.PostJournalEntry;

namespace PersonalFinance.WebApi.Contracts;

internal sealed record PostJournalEntryRequest(
    Guid CreatedByUserId,
    DateOnly Date,
    string Description,
        IReadOnlyCollection<PostJournalEntryLineDto> Lines);
