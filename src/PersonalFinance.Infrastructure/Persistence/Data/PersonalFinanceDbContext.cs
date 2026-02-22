using Microsoft.EntityFrameworkCore;
using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.JournalEntries;
using PersonalFinance.Domain.Finance.Ledgers;

namespace PersonalFinance.Infrastructure.Persistence;

public sealed class PersonalFinanceDbContext : DbContext
{
	public PersonalFinanceDbContext(DbContextOptions<PersonalFinanceDbContext> options)
		: base(options) { }

	public DbSet<Ledger> Ledgers => Set<Ledger>();
	public DbSet<Account> Accounts => Set<Account>();
	public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		ArgumentNullException.ThrowIfNull(modelBuilder);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(PersonalFinanceDbContext).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
