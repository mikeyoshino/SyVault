using DigitalVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalVault.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Account> Accounts { get; }
    DbSet<FamilyMember> FamilyMembers { get; }
    DbSet<Document> Documents { get; }
    DbSet<DocumentMetadata> DocumentMetadata { get; }
    DbSet<FileAttachment> FileAttachments { get; }
    DbSet<Note> Notes { get; }
    DbSet<AccountCollaborator> AccountCollaborators { get; }
    DbSet<UserKeyPair> UserKeyPairs { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
