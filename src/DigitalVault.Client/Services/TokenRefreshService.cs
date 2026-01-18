using System.Net.Http.Json;
using DigitalVault.Shared.DTOs.Auth;
using DigitalVault.Shared.DTOs.Common;

namespace DigitalVault.Client.Services;

/// <summary>
/// Service that automatically refreshes access tokens before they expire
/// Runs a timer to refresh tokens 5 minutes before expiration
/// </summary>
public class TokenRefreshService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TokenRefreshService> _logger;
    private Timer? _refreshTimer;
    private DateTime? _tokenExpiry;
    private bool _isRefreshing = false;

    public TokenRefreshService(HttpClient httpClient, ILogger<TokenRefreshService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Start automatic token refresh timer
    /// </summary>
    /// <param name="tokenExpiry">When the current access token expires</param>
    public void StartAutoRefresh(DateTime tokenExpiry)
    {
        _tokenExpiry = tokenExpiry;

        // Refresh 5 minutes before expiry
        var refreshTime = tokenExpiry.AddMinutes(-5) - DateTime.UtcNow;

        if (refreshTime.TotalSeconds <= 0)
        {
            // Token expires in less than 5 minutes, refresh immediately
            _ = RefreshTokenAsync();
            return;
        }

        _logger.LogInformation("Scheduling token refresh in {Minutes} minutes", refreshTime.TotalMinutes);

        _refreshTimer?.Dispose();
        _refreshTimer = new Timer(async _ =>
        {
            await RefreshTokenAsync();
        }, null, refreshTime, Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// Stop automatic token refresh
    /// </summary>
    public void StopAutoRefresh()
    {
        _refreshTimer?.Dispose();
        _refreshTimer = null;
        _tokenExpiry = null;
        _logger.LogInformation("Token auto-refresh stopped");
    }

    /// <summary>
    /// Manually trigger token refresh
    /// </summary>
    public async Task<bool> RefreshTokenAsync()
    {
        if (_isRefreshing)
        {
            _logger.LogWarning("Token refresh already in progress, skipping");
            return false;
        }

        _isRefreshing = true;
        try
        {
            _logger.LogInformation("Refreshing access token...");

            var response = await _httpClient.PostAsync("/api/auth/refresh", null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();

                if (result?.Success == true && result.Data != null)
                {
                    _logger.LogInformation("Token refreshed successfully for user: {Email}", result.Data.Email);

                    // Schedule next refresh (tokens expire in 60 minutes)
                    StartAutoRefresh(DateTime.UtcNow.AddMinutes(60));

                    return true;
                }
            }

            _logger.LogWarning("Token refresh failed with status: {StatusCode}", response.StatusCode);
            StopAutoRefresh();
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            StopAutoRefresh();
            return false;
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
