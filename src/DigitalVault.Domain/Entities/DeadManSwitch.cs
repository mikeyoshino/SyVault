using DigitalVault.Domain.Common;
using DigitalVault.Domain.Enums;

namespace DigitalVault.Domain.Entities;

public class DeadManSwitch : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Switch Configuration
    public int CheckInIntervalDays { get; set; } = 90; // 30, 90, 180, 365
    public int GracePeriodDays { get; set; } = 14;

    // Status
    public bool IsActive { get; set; } = true;
    public DateTime LastCheckInAt { get; set; } = DateTime.UtcNow;
    public DateTime NextCheckInDueDate { get; set; }

    // Trigger Status
    public SwitchStatus Status { get; set; } = SwitchStatus.Active;
    public DateTime? GracePeriodStartedAt { get; set; }
    public DateTime? TriggeredAt { get; set; }

    // Notification Preferences
    public List<int> ReminderDays { get; set; } = new() { 7, 3, 1 };
    public List<NotificationChannel> NotificationChannels { get; set; } = new() { NotificationChannel.Email };

    // Emergency Contacts
    public string? EmergencyEmail { get; set; }
    public string? EmergencyPhone { get; set; }

    // Relationships
    public ICollection<SwitchCheckIn> CheckIns { get; set; } = new List<SwitchCheckIn>();
    public ICollection<SwitchNotification> Notifications { get; set; } = new List<SwitchNotification>();
}
