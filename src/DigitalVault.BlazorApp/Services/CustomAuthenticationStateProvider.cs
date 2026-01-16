using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace DigitalVault.BlazorApp.Services;

/// <summary>
/// Custom authentication state provider for Blazor WebAssembly
/// Checks if user has master key in sessionStorage (cookies are httpOnly)
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly SecureStorageService _secureStorage;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;

    public CustomAuthenticationStateProvider(
        SecureStorageService secureStorage,
        ILogger<CustomAuthenticationStateProvider> logger)
    {
        _secureStorage = secureStorage;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Check if master key exists in sessionStorage
            // (Tokens are in httpOnly cookies, but we need master key to decrypt vault data)
            var masterKey = await _secureStorage.GetMasterKeyAsync();

            if (!string.IsNullOrEmpty(masterKey))
            {
                // User is authenticated
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, "User"),
                    new Claim(ClaimTypes.Role, "User")
                };

                var identity = new ClaimsIdentity(claims, "Custom Authentication");
                var user = new ClaimsPrincipal(identity);

                _logger.LogInformation("User authenticated (master key found)");
                return new AuthenticationState(user);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication state");
        }

        // User is not authenticated
        _logger.LogInformation("User not authenticated (no master key)");
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    /// <summary>
    /// Notify that authentication state has changed (call after login/logout)
    /// </summary>
    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
