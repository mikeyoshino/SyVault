using DigitalVault.Domain.Common;

namespace DigitalVault.Domain.Entities;

public class SwitchCheckIn : BaseEntity
{
    public Guid SwitchId { get; set; }
    public DeadManSwitch Switch { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CheckInAt { get; set; } = DateTime.UtcNow;
    public string? CheckInMethod { get; set; } // 'web', 'mobile', 'email_link', 'sms_link'
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Location { get; set; } // Optional: City, Country
}
