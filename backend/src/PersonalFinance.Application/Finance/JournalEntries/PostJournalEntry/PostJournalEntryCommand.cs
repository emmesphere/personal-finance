namespace PersonalFinance.Application.Finance.JournalEntries.PostJournalEntry;

public sealed record PostJournalEntryCommand(
    Guid LedgerId,
    DateOnly Date,
    string Description,
    IReadOnlyCollection<PostJournalEntryLineDto> Lines
);
