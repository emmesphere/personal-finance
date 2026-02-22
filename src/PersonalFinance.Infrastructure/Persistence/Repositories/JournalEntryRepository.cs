using PersonalFinance.Application.Abstractions.Persistence;
using PersonalFinance.Domain.Finance.JournalEntries;

namespace PersonalFinance.Infrastructure.Persistence.Repositories;

public sealed class JournalEntryRepository(PersonalFinanceDbContext context) : IJournalEntryRepository
{
    public void Add(JournalEntry entry)
    {
        context.JournalEntries.Add(entry);
    }
}
