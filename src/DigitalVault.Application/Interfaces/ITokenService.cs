using DigitalVault.Domain.Entities;

namespace DigitalVault.Application.Interfaces;

public interface ITokenService
{
    // Existing methods
    string GenerateAccessToken(User user, Guid? accountId = null);
    string GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);

    // NEW: Refresh token management
    Task<RefreshToken> StoreRefreshTokenAsync(
        Guid userId,
        string token,
        string deviceName,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> ValidateRefreshTokenAsync(string token);

    Task<(string NewAccessToken, string NewRefreshToken)> RotateRefreshTokenAsync(
        RefreshToken oldToken);

    Task RevokeRefreshTokenAsync(string token, string? reason = null);

    Task RevokeAllUserTokensAsync(Guid userId);

    Task<List<RefreshToken>> GetUserActiveSessionsAsync(Guid userId);

    Task<int> CleanupExpiredTokensAsync();
}
