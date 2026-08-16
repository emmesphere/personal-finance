using PersonalFinance.Domain.Finance.JournalEntries;

namespace PersonalFinance.Application.Abstractions.Persistence;

public interface IJournalEntryRepository
{
    void Add(JournalEntry entry);

    Task<int> CountPostedInMonthAsync(int year, int month, CancellationToken ct);
}

