using DigitalVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalVault.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(rt => rt.Id);

        // Token (unique, indexed for fast lookup)
        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);
        builder.HasIndex(rt => rt.Token).IsUnique();

        // Device tracking
        builder.Property(rt => rt.DeviceName).HasMaxLength(200);
        builder.Property(rt => rt.IpAddress).HasMaxLength(50);
        builder.Property(rt => rt.UserAgent).HasMaxLength(500);

        // Indexes for query performance
        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => rt.ExpiresAt);
        builder.HasIndex(rt => rt.RevokedAt);
        builder.HasIndex(rt => new { rt.UserId, rt.RevokedAt, rt.ExpiresAt });

        // Relationship: Many RefreshTokens → One User (cascade delete)
        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
