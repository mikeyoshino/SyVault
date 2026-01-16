using DigitalVault.Domain.Common;

namespace DigitalVault.Domain.Entities;

public class RefreshToken : BaseEntity
{
    // Token identification
    public string Token { get; set; } = string.Empty;  // Base64 hashed token
    public Guid UserId { get; set; }  // FK to User

    // Lifecycle
    public DateTime ExpiresAt { get; set; }  // 7 days from creation
    public DateTime? RevokedAt { get; set; }  // NULL = active, NOT NULL = revoked
    public string? ReplacedByToken { get; set; }  // Track rotation chain

    // Device tracking
    public string DeviceName { get; set; } = string.Empty;  // "Chrome on Windows"
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;

    // Computed properties
    public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;
    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;
    public bool IsRevoked => RevokedAt != null;
}
