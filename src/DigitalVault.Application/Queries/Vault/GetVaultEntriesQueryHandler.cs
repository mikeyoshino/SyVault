using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Enums;
using DigitalVault.Shared.DTOs.Vault;
using MediatR;

namespace DigitalVault.Application.Queries.Vault;

public class GetVaultEntriesQueryHandler : IRequestHandler<GetVaultEntriesQuery, List<VaultEntryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetVaultEntriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<VaultEntryDto>> Handle(GetVaultEntriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.VaultEntries
            .Where(v => v.UserId == request.UserId && !v.IsDeleted);

        // Filter by category if provided
        if (!string.IsNullOrEmpty(request.Category) && Enum.TryParse<VaultCategory>(request.Category, out var category))
        {
            query = query.Where(v => v.Category == category);
        }

        var entries = await query
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);

        return entries.Select(v => new VaultEntryDto
        {
            Id = v.Id,
            Title = v.Title,
            Category = v.Category.ToString(),
            EncryptedDataKey = v.EncryptedDataKey,
            EncryptedContent = v.EncryptedContent,
            BlobStorageUrl = v.BlobStorageUrl,
            IV = v.IV,
            EncryptionAlgorithm = v.EncryptionAlgorithm,
            IsSharedWithHeirs = v.IsSharedWithHeirs,
            CreatedAt = v.CreatedAt,
            UpdatedAt = v.UpdatedAt
        }).ToList();
    }
}
