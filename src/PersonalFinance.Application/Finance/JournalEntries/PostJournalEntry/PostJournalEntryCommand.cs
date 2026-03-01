namespace PersonalFinance.Application.Finance.JournalEntries.PostJournalEntry;

public sealed record PostJournalEntryCommand(
    Guid LedgerId,
    Guid CreatedByUserId,
    DateOnly Date,
    string Description,
    IReadOnlyCollection<PostJournalEntryLineDto> Lines
);
