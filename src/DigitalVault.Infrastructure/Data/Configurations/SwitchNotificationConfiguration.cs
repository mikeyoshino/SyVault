using DigitalVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalVault.Infrastructure.Data.Configurations;

public class SwitchNotificationConfiguration : IEntityTypeConfiguration<SwitchNotification>
{
    public void Configure(EntityTypeBuilder<SwitchNotification> builder)
    {
        builder.ToTable("SwitchNotifications");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.NotificationType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Channel)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Status)
            .HasMaxLength(50)
            .HasDefaultValue("sent");

        builder.Property(s => s.Subject)
            .HasMaxLength(500);

        builder.Property(s => s.Body)
            .HasMaxLength(5000);

        builder.Property(s => s.CheckInLinkToken)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(s => s.Switch)
            .WithMany(d => d.Notifications)
            .HasForeignKey(s => s.SwitchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(s => s.SwitchId);
        builder.HasIndex(s => s.NotificationType);
        builder.HasIndex(s => s.SentAt);
        builder.HasIndex(s => s.CheckInLinkToken)
            .IsUnique()
            .HasFilter("\"CheckInLinkToken\" IS NOT NULL");
    }
}
