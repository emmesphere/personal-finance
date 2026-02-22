using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalFinance.Domain.Finance.Accounts;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("accounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LedgerId)
            .HasColumnName("ledger_id")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasColumnName("type")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(x => x.DueDate)
            .HasConversion(
                v => v == null ? (int?)null : v.Day,
                v => v == null ? null : DueDate.From(v.Value))
            .HasColumnName("due_day");

        builder.HasIndex(x => new { x.LedgerId, x.Name })
            .IsUnique();
    }
}
