using DigitalVault.Shared.DTOs.Auth;
using MediatR;

namespace DigitalVault.Application.Commands.Auth;

public class RegisterCommand : IRequest<AuthResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    // Key derivation info (generated client-side)
    public byte[] KeyDerivationSalt { get; set; } = Array.Empty<byte>();
    public int KeyDerivationIterations { get; set; } = 100000;

    public string EncryptedMasterKey { get; set; } = string.Empty;

    // Device tracking for refresh tokens
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
