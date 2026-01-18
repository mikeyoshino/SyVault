using DigitalVault.Domain.Common;

namespace DigitalVault.Domain.Entities;

public class Note : BaseEntity
{
    public Guid FamilyMemberId { get; set; }
    public Guid AccountId { get; set; }

    // Encrypted Content
    public string? EncryptedTitle { get; set; }
    public string EncryptedContent { get; set; } = string.Empty;

    // Relationships
    public FamilyMember FamilyMember { get; set; } = null!;
    public Account Account { get; set; } = null!;
}
