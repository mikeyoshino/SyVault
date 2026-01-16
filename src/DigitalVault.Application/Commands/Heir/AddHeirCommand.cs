using DigitalVault.Shared.DTOs.Heir;
using MediatR;

namespace DigitalVault.Application.Commands.Heir;

public class AddHeirCommand : IRequest<HeirDto>
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string PublicKey { get; set; } = string.Empty; // Base64 encoded RSA public key
    public string AccessLevel { get; set; } = "Full";
    public List<string>? CanAccessCategories { get; set; }
}
