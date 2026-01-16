using System.Security.Cryptography;
using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Entities;
using DigitalVault.Domain.Enums;
using DigitalVault.Shared.DTOs.Auth;
using MediatR;

namespace DigitalVault.Application.Commands.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Hash password for account authentication
        var (passwordHash, salt) = _passwordHasher.HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = passwordHash,
            Salt = salt,
            KeyDerivationSalt = request.KeyDerivationSalt, // Use client-provided salt (critical for decryption!)
            KeyDerivationIterations = request.KeyDerivationIterations,
            EncryptedMasterKey = request.EncryptedMasterKey, // Store encrypted master key (zero knowledge)
            SubscriptionTier = SubscriptionTier.Free,
            EmailVerified = false,
            MfaEnabled = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token with device info
        var deviceName = string.IsNullOrEmpty(request.UserAgent)
            ? "Unknown Device"
            : (request.UserAgent.Contains("Chrome") ? "Chrome" :
               request.UserAgent.Contains("Firefox") ? "Firefox" :
               request.UserAgent.Contains("Safari") ? "Safari" : "Unknown") +
              " on " +
              (request.UserAgent.Contains("Windows") ? "Windows" :
               request.UserAgent.Contains("Mac") ? "macOS" :
               request.UserAgent.Contains("Linux") ? "Linux" : "Unknown");

        await _tokenService.StoreRefreshTokenAsync(
            user.Id,
            refreshToken,
            deviceName,
            request.IpAddress,
            request.UserAgent,
            cancellationToken);

        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                EmailVerified = user.EmailVerified,
                PhoneNumber = user.PhoneNumber,
                MfaEnabled = user.MfaEnabled,
                SubscriptionTier = user.SubscriptionTier.ToString(),
                SubscriptionExpiresAt = user.SubscriptionExpiresAt,
                KeyDerivationSalt = user.KeyDerivationSalt,
                KeyDerivationIterations = user.KeyDerivationIterations,
                EncryptedMasterKey = user.EncryptedMasterKey
            }
        };
    }
}
