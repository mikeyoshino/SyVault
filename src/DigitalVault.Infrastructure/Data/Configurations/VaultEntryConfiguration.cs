using DigitalVault.Domain.Entities;
using DigitalVault.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalVault.Infrastructure.Data.Configurations;

public class VaultEntryConfiguration : IEntityTypeConfiguration<VaultEntry>
{
    public void Configure(EntityTypeBuilder<VaultEntry> builder)
    {
        builder.ToTable("VaultEntries");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(v => v.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(v => v.EncryptedDataKey)
            .IsRequired();

        builder.Property(v => v.IV)
            .IsRequired();

        builder.Property(v => v.EncryptionAlgorithm)
            .HasMaxLength(50)
            .HasDefaultValue("AES-256-GCM");

        builder.Property(v => v.BlobStorageUrl)
            .HasMaxLength(1000);

        builder.Property(v => v.BlobStorageKey)
            .HasMaxLength(500);

        builder.Property(v => v.IsSharedWithHeirs)
            .HasDefaultValue(true);

        // Relationships
        builder.HasOne(v => v.User)
            .WithMany(u => u.VaultEntries)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.HeirAccesses)
            .WithOne(h => h.VaultEntry)
            .HasForeignKey(h => h.VaultEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(v => v.UserId);
        builder.HasIndex(v => v.Category);
        builder.HasIndex(v => v.CreatedAt);
        builder.HasIndex(v => v.IsDeleted);
    }
}
