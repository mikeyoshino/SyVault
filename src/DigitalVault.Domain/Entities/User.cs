using DigitalVault.Domain.Common;

namespace DigitalVault.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    // Authentication (for login only)
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;

    // Account Info
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Relationships
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<AccountCollaborator> Collaborations { get; set; } = new List<AccountCollaborator>();
    public UserKeyPair? KeyPair { get; set; }
}
