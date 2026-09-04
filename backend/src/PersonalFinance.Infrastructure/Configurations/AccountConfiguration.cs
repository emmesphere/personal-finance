using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalFinance.Domain.Finance.Accounts;
using PersonalFinance.Domain.Finance.Categories;

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

        builder.Property(x => x.CategoryId)
            .HasColumnName("category_id");

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.LedgerId, x.Name })
            .IsUnique();

        builder.HasIndex(x => new { x.LedgerId, x.CategoryId })
            .IsUnique()
            .HasFilter("category_id IS NOT NULL");
    }
}
