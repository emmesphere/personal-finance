using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.Finance.JournalEntries;

namespace PersonalFinance.Application.Tests.Fakes;

public sealed class FakeJournalEntryRepository : IJournalEntryRepository
{
    public List<JournalEntry> Added { get; } = [];

    public void Add(JournalEntry entry) => Added.Add(entry);

    public Task<int> CountPostedInMonthAsync(int year, int month, CancellationToken ct)
        => Task.FromResult(Added.Count(e => e.Status == JournalEntryStatus.Posted && e.Date.Year == year && e.Date.Month == month));
}
