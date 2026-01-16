using DigitalVault.Application.Interfaces;
using DigitalVault.Shared.DTOs.Heir;
using MediatR;

namespace DigitalVault.Application.Queries.Heir;

public class GetHeirQueryHandler : IRequestHandler<GetHeirQuery, HeirDto?>
{
    private readonly IApplicationDbContext _context;

    public GetHeirQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HeirDto?> Handle(GetHeirQuery request, CancellationToken cancellationToken)
    {
        var heir = await _context.Heirs
            .Where(h => h.Id == request.Id && h.UserId == request.UserId && !h.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (heir == null)
        {
            return null;
        }

        return new HeirDto
        {
            Id = heir.Id,
            Email = heir.Email,
            FullName = heir.FullName,
            Relationship = heir.Relationship,
            IsVerified = heir.IsVerified,
            VerifiedAt = heir.VerifiedAt,
            AccessLevel = heir.AccessLevel.ToString(),
            CanAccessCategories = heir.CanAccessCategories,
            CreatedAt = heir.CreatedAt,
            UpdatedAt = heir.UpdatedAt
        };
    }
}
