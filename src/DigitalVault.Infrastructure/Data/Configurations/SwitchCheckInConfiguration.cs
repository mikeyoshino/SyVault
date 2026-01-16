using DigitalVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalVault.Infrastructure.Data.Configurations;

public class SwitchCheckInConfiguration : IEntityTypeConfiguration<SwitchCheckIn>
{
    public void Configure(EntityTypeBuilder<SwitchCheckIn> builder)
    {
        builder.ToTable("SwitchCheckIns");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.CheckInMethod)
            .HasMaxLength(50);

        builder.Property(s => s.IpAddress)
            .HasMaxLength(45);

        builder.Property(s => s.UserAgent)
            .HasMaxLength(500);

        builder.Property(s => s.Location)
            .HasMaxLength(255);

        // Relationships
        builder.HasOne(s => s.Switch)
            .WithMany(d => d.CheckIns)
            .HasForeignKey(s => s.SwitchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(s => s.SwitchId);
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.CheckInAt);
    }
}
