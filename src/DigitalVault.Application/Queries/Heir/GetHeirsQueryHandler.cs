using DigitalVault.Application.Interfaces;
using DigitalVault.Shared.DTOs.Heir;
using MediatR;

namespace DigitalVault.Application.Queries.Heir;

public class GetHeirsQueryHandler : IRequestHandler<GetHeirsQuery, List<HeirDto>>
{
    private readonly IApplicationDbContext _context;

    public GetHeirsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<HeirDto>> Handle(GetHeirsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Heirs
            .Where(h => h.UserId == request.UserId && !h.IsDeleted);

        // Apply optional filter
        if (request.IsVerified.HasValue)
        {
            query = query.Where(h => h.IsVerified == request.IsVerified.Value);
        }

        var heirs = await query
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);

        return heirs.Select(h => new HeirDto
        {
            Id = h.Id,
            Email = h.Email,
            FullName = h.FullName,
            Relationship = h.Relationship,
            IsVerified = h.IsVerified,
            VerifiedAt = h.VerifiedAt,
            AccessLevel = h.AccessLevel.ToString(),
            CanAccessCategories = h.CanAccessCategories,
            CreatedAt = h.CreatedAt,
            UpdatedAt = h.UpdatedAt
        }).ToList();
    }
}
