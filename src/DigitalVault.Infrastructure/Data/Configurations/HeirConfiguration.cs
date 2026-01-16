using DigitalVault.Domain.Entities;
using DigitalVault.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalVault.Infrastructure.Data.Configurations;

public class HeirConfiguration : IEntityTypeConfiguration<Heir>
{
    public void Configure(EntityTypeBuilder<Heir> builder)
    {
        builder.ToTable("Heirs");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(h => h.FullName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(h => h.Relationship)
            .HasMaxLength(100);

        builder.Property(h => h.VerificationToken)
            .HasMaxLength(500);

        builder.Property(h => h.PublicKey)
            .IsRequired();

        builder.Property(h => h.AccessLevel)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(AccessLevel.Full);

        // Store list as JSON
        builder.Property(h => h.CanAccessCategories)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        // Relationships
        builder.HasOne(h => h.User)
            .WithMany(u => u.Heirs)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.VaultAccesses)
            .WithOne(v => v.Heir)
            .HasForeignKey(v => v.HeirId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.AccessLogs)
            .WithOne(a => a.Heir)
            .HasForeignKey(a => a.HeirId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(h => h.UserId);
        builder.HasIndex(h => h.Email);
        builder.HasIndex(h => h.IsVerified);
        builder.HasIndex(h => h.IsDeleted);

        // Unique constraint
        builder.HasIndex(h => new { h.UserId, h.Email })
            .IsUnique();
    }
}
