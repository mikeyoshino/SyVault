namespace DigitalVault.Shared.DTOs.Heir;

public class AddHeirRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string PublicKey { get; set; } = string.Empty; // Base64 encoded RSA public key
    public string AccessLevel { get; set; } = "Full"; // Full, Limited, ReadOnly
    public List<string>? CanAccessCategories { get; set; } // If AccessLevel is Limited
}
