using DigitalVault.Domain.Entities;
using DigitalVault.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalVault.Infrastructure.Data.Configurations;

public class DeadManSwitchConfiguration : IEntityTypeConfiguration<DeadManSwitch>
{
    public void Configure(EntityTypeBuilder<DeadManSwitch> builder)
    {
        builder.ToTable("DeadManSwitches");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.CheckInIntervalDays)
            .HasDefaultValue(90);

        builder.Property(d => d.GracePeriodDays)
            .HasDefaultValue(14);

        builder.Property(d => d.IsActive)
            .HasDefaultValue(true);

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(SwitchStatus.Active);

        builder.Property(d => d.EmergencyEmail)
            .HasMaxLength(255);

        builder.Property(d => d.EmergencyPhone)
            .HasMaxLength(20);

        // Store lists as JSON
        builder.Property(d => d.ReminderDays)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList())
            .Metadata.SetValueComparer(new ValueComparer<List<int>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        builder.Property(d => d.NotificationChannels)
            .HasConversion(
                v => string.Join(',', v.Select(c => (int)c)),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => (NotificationChannel)int.Parse(s))
                    .ToList())
            .Metadata.SetValueComparer(new ValueComparer<List<NotificationChannel>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        // Relationships
        builder.HasOne(d => d.User)
            .WithOne(u => u.DeadManSwitch)
            .HasForeignKey<DeadManSwitch>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.CheckIns)
            .WithOne(c => c.Switch)
            .HasForeignKey(c => c.SwitchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Notifications)
            .WithOne(n => n.Switch)
            .HasForeignKey(n => n.SwitchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(d => d.UserId)
            .IsUnique();

        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.NextCheckInDueDate);
        builder.HasIndex(d => d.IsActive);
    }
}
