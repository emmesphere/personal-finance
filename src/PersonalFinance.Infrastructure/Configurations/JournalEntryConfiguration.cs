using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.JournalEntries;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

public sealed class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("journal_entries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LedgerId)
            .HasColumnName("ledger_id")
            .IsRequired();

        builder.Property(x => x.CreatedByUserId)
            .HasConversion(
                v => v.Value,
                v => UserId.From(v))
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(x => x.Date)
            .HasColumnName("date")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .HasColumnName("description")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.PostedAt)
            .HasColumnName("posted_at");        

        builder.OwnsMany(x => x.Lines, lb =>
        {
            lb.ToTable("entry_lines");
            lb.HasKey(x => x.Id);

            lb.WithOwner().HasForeignKey(x => x.JournalEntryId);

            lb.Property<Guid>(x => x.JournalEntryId).IsRequired();

            lb.Property(x => x.AccountId)
                .HasColumnName("account_id")
                .IsRequired();

            lb.Property(x => x.Type)
                .HasConversion<string>()
                .HasColumnName("type")
                .IsRequired();

            lb.Property(x => x.Amount)
                .HasConversion(
                    v => v.Amount,
                    v => Money.From(v))
                .HasColumnType("numeric(18,2)")
                .HasColumnName("amount")
                .IsRequired();

            lb.HasIndex(x => x.JournalEntryId);
            lb.HasIndex(x => x.AccountId);
        });

        builder.Navigation(x => x.Lines).HasField("_lines");
        builder.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
