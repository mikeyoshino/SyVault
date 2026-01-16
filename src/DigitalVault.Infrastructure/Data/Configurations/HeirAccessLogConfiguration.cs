using DigitalVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalVault.Infrastructure.Data.Configurations;

public class HeirAccessLogConfiguration : IEntityTypeConfiguration<HeirAccessLog>
{
    public void Configure(EntityTypeBuilder<HeirAccessLog> builder)
    {
        builder.ToTable("HeirAccessLogs");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.AccessType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(h => h.IpAddress)
            .HasMaxLength(45);

        builder.Property(h => h.UserAgent)
            .HasMaxLength(500);

        builder.Property(h => h.FailureReason)
            .HasMaxLength(1000);

        builder.Property(h => h.WasSuccessful)
            .HasDefaultValue(true);

        // Relationships
        builder.HasOne(h => h.Heir)
            .WithMany(heir => heir.AccessLogs)
            .HasForeignKey(h => h.HeirId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.VaultEntry)
            .WithMany()
            .HasForeignKey(h => h.VaultEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.User)
            .WithMany()
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(h => h.HeirId);
        builder.HasIndex(h => h.VaultEntryId);
        builder.HasIndex(h => h.UserId);
        builder.HasIndex(h => h.AccessedAt);
        builder.HasIndex(h => h.AccessType);
    }
}
