using Microsoft.EntityFrameworkCore;

using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.Finance.JournalEntries;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public sealed class JournalEntryRepository(PersonalFinanceDbContext context) : IJournalEntryRepository
{
    public void Add(JournalEntry entry)
    {
        context.JournalEntries.Add(entry);
    }

    public Task<int> CountPostedInMonthAsync(int year, int month, CancellationToken ct)
        => context.JournalEntries.CountAsync(
            je => je.Status == JournalEntryStatus.Posted && je.Date.Year == year && je.Date.Month == month, ct);
}
