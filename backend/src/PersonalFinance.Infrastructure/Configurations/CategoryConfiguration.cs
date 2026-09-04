using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalFinance.Domain.Finance.Categories;
using PersonalFinance.Domain.Finance.Common;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasConversion<string>()
            .HasColumnName("kind")
            .IsRequired();

        builder.Property(x => x.CreatedByUserId)
            .HasConversion(
                v => v == null ? (Guid?)null : v.Value,
                v => v == null ? null : UserId.From(v.Value))
            .HasColumnName("created_by_user_id");

        builder.Property(x => x.IsSystemDefined)
            .HasColumnName("is_system_defined")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
