using DigitalVault.Domain.Entities;
using DigitalVault.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalVault.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Salt)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.MfaSecret)
            .HasMaxLength(500);

        builder.Property(u => u.KeyDerivationSalt)
            .IsRequired();

        builder.Property(u => u.SubscriptionTier)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(SubscriptionTier.Free);

        // Relationships
        builder.HasMany(u => u.VaultEntries)
            .WithOne(v => v.User)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Heirs)
            .WithOne(h => h.User)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.DeadManSwitch)
            .WithOne(s => s.User)
            .HasForeignKey<DeadManSwitch>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(u => u.CreatedAt);
        builder.HasIndex(u => u.IsDeleted);
        builder.HasIndex(u => u.SubscriptionTier);
    }
}
