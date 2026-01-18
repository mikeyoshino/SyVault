using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalVault.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentMetadata> DocumentMetadata => Set<DocumentMetadata>();
    public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<AccountCollaborator> AccountCollaborators => Set<AccountCollaborator>();
    public DbSet<UserKeyPair> UserKeyPairs => Set<UserKeyPair>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update UpdatedAt timestamp for all modified entities
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Domain.Common.BaseEntity entity)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
