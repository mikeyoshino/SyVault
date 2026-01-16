using DigitalVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalVault.Infrastructure.Data.Configurations;

public class HeirVaultAccessConfiguration : IEntityTypeConfiguration<HeirVaultAccess>
{
    public void Configure(EntityTypeBuilder<HeirVaultAccess> builder)
    {
        builder.ToTable("HeirVaultAccesses");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.EncryptedDataKey)
            .IsRequired();

        builder.Property(h => h.CanAccess)
            .HasDefaultValue(true);

        // Relationships
        builder.HasOne(h => h.Heir)
            .WithMany(heir => heir.VaultAccesses)
            .HasForeignKey(h => h.HeirId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.VaultEntry)
            .WithMany(v => v.HeirAccesses)
            .HasForeignKey(h => h.VaultEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(h => h.HeirId);
        builder.HasIndex(h => h.VaultEntryId);

        // Unique constraint
        builder.HasIndex(h => new { h.HeirId, h.VaultEntryId })
            .IsUnique();
    }
}
