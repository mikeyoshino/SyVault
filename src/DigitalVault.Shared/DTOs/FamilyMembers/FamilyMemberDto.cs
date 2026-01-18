namespace DigitalVault.Shared.DTOs.FamilyMembers;

public class FamilyMemberDto
{
    public Guid Id { get; set; }
    public string EncryptedFirstName { get; set; } = string.Empty;
    public string EncryptedLastName { get; set; } = string.Empty;
    public string? EncryptedRelationship { get; set; }
    public string? AvatarColor { get; set; }
    public string? InitialsPlainText { get; set; }
}
