using DigitalVault.Shared.DTOs.Heir;
using MediatR;

namespace DigitalVault.Application.Queries.Heir;

public class GetHeirsQuery : IRequest<List<HeirDto>>
{
    public Guid UserId { get; set; }
    public bool? IsVerified { get; set; } // Optional filter
}
