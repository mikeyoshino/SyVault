using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using DigitalVault.Shared.DTOs.Common;
using DigitalVault.Shared.DTOs.Auth;

namespace DigitalVault.Client.Services;

/// <summary>
/// Custom authentication state provider for Blazor WebAssembly
/// Checks if user has master key in sessionStorage (cookies are httpOnly)
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly SecureStorageService _secureStorage;
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;

    public CustomAuthenticationStateProvider(
        SecureStorageService secureStorage,
        HttpClient httpClient,
        ILogger<CustomAuthenticationStateProvider> logger)
    {
        _secureStorage = secureStorage;
        _httpClient = httpClient;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // 1. Storage Check (Fast Path)
            var isLoggedIn = await _secureStorage.GetAsync("isLoggedIn");
            if (!string.IsNullOrEmpty(isLoggedIn) && bool.Parse(isLoggedIn))
            {
                var email = await _secureStorage.GetAsync("userEmail") ?? "User";
                return CreateAuthState(email);
            }

            // 2. Cookie Check (Fallback for Server Redirects / Refresh)
            // If storage is empty, maybe we have a valid HttpOnly cookie?
            _logger.LogInformation("No local auth flag found, checking server session...");

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
                // CRITICAL: Ensure cookies are sent with the request even for cross-origin
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
                    if (result?.Success == true && result.Data != null)
                    {
                        // Found valid cookie! Bootstrap storage
                        await _secureStorage.SaveAsync("isLoggedIn", "true");
                        await _secureStorage.SaveAsync("userEmail", result.Data.Email);

                        _logger.LogInformation("Server session valid. Restored auth state.");
                        return CreateAuthState(result.Data.Email);
                    }
                }
            }
            catch (Exception ex)
            {
                // Ignore errors (401, Network, etc) - just means not logged in
                _logger.LogInformation("Server session check failed: {Message}", ex.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication state");
        }

        // User is not authenticated
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    private AuthenticationState CreateAuthState(string email)
    {
        var claims = new[]
       {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Role, "User")
        };

        var identity = new ClaimsIdentity(claims, "Custom Authentication");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
