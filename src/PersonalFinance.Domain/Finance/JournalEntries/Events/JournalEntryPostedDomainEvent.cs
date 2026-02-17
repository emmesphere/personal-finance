using PersonalFinance.BuildingBlocks.Domain;

namespace PersonalFinance.Domain.Finance.JournalEntries;

public sealed record JournalEntryPostedDomainEvent(
    DateTime EventTime,
    Guid LedgerId,
    Guid JournalEntryId,
    DateOnly Date,
    decimal TotalAmount
) : IDomainEvent;