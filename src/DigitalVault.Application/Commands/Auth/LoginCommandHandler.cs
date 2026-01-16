using DigitalVault.Application.Interfaces;
using DigitalVault.Shared.DTOs.Auth;
using MediatR;

namespace DigitalVault.Application.Commands.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMfaService _mfaService;

    public LoginCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IMfaService mfaService)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _mfaService = mfaService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.Salt))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Check MFA if enabled
        if (user.MfaEnabled)
        {
            if (string.IsNullOrEmpty(request.MfaCode))
            {
                throw new UnauthorizedAccessException("MFA code required");
            }

            if (string.IsNullOrEmpty(user.MfaSecret))
            {
                throw new UnauthorizedAccessException("MFA secret not found");
            }

            // Verify MFA code
            if (!_mfaService.VerifyCode(user.MfaSecret, request.MfaCode))
            {
                throw new UnauthorizedAccessException("Invalid MFA code");
            }
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
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
