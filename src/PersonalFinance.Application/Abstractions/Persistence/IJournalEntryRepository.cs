using PersonalFinance.Domain.Finance.JournalEntries;

namespace PersonalFinance.Application.Abstractions.Persistence;

public interface IJournalEntryRepository
{
    void Add(JournalEntry entry);
}

