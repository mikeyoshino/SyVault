namespace DigitalVault.Shared.DTOs.Auth;

public class SessionDto
{
    public Guid Id { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime LastUsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
