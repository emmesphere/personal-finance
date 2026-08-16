using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalFinance.Domain.Finance.Budgets;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

public sealed class MonthlyBudgetConfiguration : IEntityTypeConfiguration<MonthlyBudget>
{
    public void Configure(EntityTypeBuilder<MonthlyBudget> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("monthly_budgets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LedgerId)
            .HasColumnName("ledger_id")
            .IsRequired();

        builder.Property(x => x.YearMonth)
            .HasConversion(
                v => v.ToInt(),
                v => YearMonth.FromInt(v))
            .HasColumnName("year_month")
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasConversion(
                v => v.Amount,
                v => Money.From(v))
            .HasColumnType("numeric(18,2)")
            .HasColumnName("amount")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => new { x.LedgerId, x.YearMonth })
            .IsUnique();
    }
}
