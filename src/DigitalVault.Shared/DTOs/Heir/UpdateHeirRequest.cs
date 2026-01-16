namespace DigitalVault.Shared.DTOs.Heir;

public class UpdateHeirRequest
{
    public string? FullName { get; set; }
    public string? Relationship { get; set; }
    public string? AccessLevel { get; set; }
    public List<string>? CanAccessCategories { get; set; }
}
