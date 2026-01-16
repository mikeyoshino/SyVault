using DigitalVault.Shared.DTOs.Heir;
using MediatR;

namespace DigitalVault.Application.Queries.Heir;

public class GetHeirQuery : IRequest<HeirDto?>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}
