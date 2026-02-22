using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalFinance.Domain.Finance.Common;
using PersonalFinance.Domain.Finance.Ledgers;

namespace PersonalFinance.Infrastructure.Configurations;
public sealed class LedgerConfiguration : IEntityTypeConfiguration<Ledger>
{
    public void Configure(EntityTypeBuilder<Ledger> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("ledgers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.OwnerUserId)
            .HasConversion(
                v => v.Value,
                v => UserId.From(v))
            .HasColumnName("owner_user_id")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Navigation(x => x.Members)
            .HasField("_members")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany<LedgerMember>(x => x.Members, mb =>
        {
            mb.ToTable("ledger_members");

            mb.WithOwner()
                .HasForeignKey(x=>x.LedgerId);

            mb.HasKey(x => x.Id);

            mb.Property(x=>x.LedgerId)
                .HasColumnName("ledger_id")
                .IsRequired();

            mb.Property(x => x.UserId)
                .HasConversion(
                    v => v.Value,
                    v => UserId.From(v))
                .HasColumnName("user_id")
                .IsRequired();

            mb.Property(x => x.JoinedAt)
                .HasColumnName("joined_at")
                .IsRequired();

            mb.HasIndex(x => new { x.LedgerId, x.UserId })
            .IsUnique();
        });
    }

}

