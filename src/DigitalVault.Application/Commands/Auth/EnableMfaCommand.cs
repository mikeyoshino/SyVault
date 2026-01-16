using DigitalVault.Shared.DTOs.Auth;
using MediatR;

namespace DigitalVault.Application.Commands.Auth;

public class EnableMfaCommand : IRequest<EnableMfaResponse>
{
    public Guid UserId { get; set; }
}
