using DigitalVault.Shared.DTOs.Heir;
using MediatR;

namespace DigitalVault.Application.Commands.Heir;

public class VerifyHeirCommand : IRequest<HeirDto>
{
    public string VerificationToken { get; set; } = string.Empty;
}
