using DigitalVault.Application.Interfaces;
using DigitalVault.Shared.DTOs.Vault;
using MediatR;

namespace DigitalVault.Application.Queries.Vault;

public class GetVaultEntryQueryHandler : IRequestHandler<GetVaultEntryQuery, VaultEntryDto?>
{
    private readonly IApplicationDbContext _context;

    public GetVaultEntryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VaultEntryDto?> Handle(GetVaultEntryQuery request, CancellationToken cancellationToken)
    {
        var entry = await _context.VaultEntries
            .FirstOrDefaultAsync(v => v.Id == request.Id && v.UserId == request.UserId && !v.IsDeleted, cancellationToken);

        if (entry == null)
        {
            return null;
        }

        return new VaultEntryDto
        {
            Id = entry.Id,
            Title = entry.Title,
            Category = entry.Category.ToString(),
            EncryptedDataKey = entry.EncryptedDataKey,
            EncryptedContent = entry.EncryptedContent,
            BlobStorageUrl = entry.BlobStorageUrl,
            IV = entry.IV,
            EncryptionAlgorithm = entry.EncryptionAlgorithm,
            IsSharedWithHeirs = entry.IsSharedWithHeirs,
            CreatedAt = entry.CreatedAt,
            UpdatedAt = entry.UpdatedAt
        };
    }
}
