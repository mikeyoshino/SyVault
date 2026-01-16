using DigitalVault.Domain.Common;

namespace DigitalVault.Domain.Entities;

public class SwitchNotification : BaseEntity
{
    public Guid SwitchId { get; set; }
    public DeadManSwitch Switch { get; set; } = null!;

    public string NotificationType { get; set; } = string.Empty; // 'reminder', 'grace_period', 'trigger_warning', 'triggered'
    public string Channel { get; set; } = string.Empty; // 'email', 'sms', 'push'

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }
    public string Status { get; set; } = "sent"; // 'sent', 'delivered', 'failed'

    // Content
    public string? Subject { get; set; }
    public string? Body { get; set; }

    // Response Tracking
    public string? CheckInLinkToken { get; set; }
    public DateTime? CheckInLinkExpiresAt { get; set; }
    public DateTime? WasClickedAt { get; set; }
}
