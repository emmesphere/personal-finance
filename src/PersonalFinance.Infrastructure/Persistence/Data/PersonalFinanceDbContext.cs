using Microsoft.EntityFrameworkCore;
using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.Budgets;
using PersonalFinance.Domain.Finance.Categories;
using PersonalFinance.Domain.Finance.JournalEntries;
using PersonalFinance.Domain.Finance.Ledgers;
using PersonalFinance.Domain.Identity.Users;

namespace PersonalFinance.Infrastructure.Persistence;

public sealed class PersonalFinanceDbContext : DbContext
{
	public PersonalFinanceDbContext(DbContextOptions<PersonalFinanceDbContext> options)
		: base(options) { }

	public DbSet<Ledger> Ledgers => Set<Ledger>();
	public DbSet<Account> Accounts => Set<Account>();
	public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
	public DbSet<User> Users => Set<User>();
	public DbSet<Category> Categories => Set<Category>();
	public DbSet<MonthlyBudget> MonthlyBudgets => Set<MonthlyBudget>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		ArgumentNullException.ThrowIfNull(modelBuilder);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(PersonalFinanceDbContext).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
