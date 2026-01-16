using DigitalVault.Shared.DTOs.Heir;
using MediatR;

namespace DigitalVault.Application.Commands.Heir;

public class UpdateHeirCommand : IRequest<HeirDto>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? FullName { get; set; }
    public string? Relationship { get; set; }
    public string? AccessLevel { get; set; }
    public List<string>? CanAccessCategories { get; set; }
}
