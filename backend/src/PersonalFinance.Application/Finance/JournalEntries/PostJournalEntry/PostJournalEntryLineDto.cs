using PersonalFinance.Domain.Finance.JournalEntries;

namespace PersonalFinance.Application.Finance.JournalEntries.PostJournalEntry;
public sealed record PostJournalEntryLineDto(
    Guid AccountId,
    EntryType Type,
    decimal Amount);
