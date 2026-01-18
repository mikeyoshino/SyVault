using DigitalVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalVault.Infrastructure.Data.Configurations;

public class AccountCollaboratorConfiguration : IEntityTypeConfiguration<AccountCollaborator>
{
    public void Configure(EntityTypeBuilder<AccountCollaborator> builder)
    {
        builder.ToTable("AccountCollaborators");

        builder.HasKey(ac => ac.Id);

        builder.Property(ac => ac.PermissionLevel)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ac => ac.EncryptedMasterKeyForCollaborator)
            .IsRequired();

        builder.Property(ac => ac.InvitationStatus)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ac => ac.InvitationToken)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(ac => ac.Account)
            .WithMany(a => a.Collaborators)
            .HasForeignKey(ac => ac.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ac => ac.User)
            .WithMany(u => u.Collaborations)
            .HasForeignKey(ac => ac.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ac => ac.InvitedBy)
            .WithMany()
            .HasForeignKey(ac => ac.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ac => ac.RevokedBy)
            .WithMany()
            .HasForeignKey(ac => ac.RevokedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ac => ac.InvitationToken);
        builder.HasIndex(ac => new { ac.AccountId, ac.UserId }).IsUnique();
    }
}
