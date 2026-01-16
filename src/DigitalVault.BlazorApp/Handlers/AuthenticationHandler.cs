using System.Net;

namespace DigitalVault.BlazorApp.Handlers;

/// <summary>
/// HTTP message handler that automatically refreshes expired access tokens
/// Intercepts 401 Unauthorized responses and attempts token refresh
/// </summary>
public class AuthenticationHandler : DelegatingHandler
{
    private readonly ILogger<AuthenticationHandler> _logger;
    private bool _isRefreshing = false;
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

    public AuthenticationHandler(ILogger<AuthenticationHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Send the original request
        var response = await base.SendAsync(request, cancellationToken);

        // If 401 Unauthorized and not already refreshing, try to refresh token
        if (response.StatusCode == HttpStatusCode.Unauthorized && !_isRefreshing)
        {
            await _refreshSemaphore.WaitAsync(cancellationToken);
            try
            {
                if (_isRefreshing)
                {
                    // Another thread is already refreshing, retry the request
                    return await base.SendAsync(request, cancellationToken);
                }

                _isRefreshing = true;
                _logger.LogInformation("Access token expired, attempting refresh...");

                // Try to refresh the token (cookies sent automatically)
                var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
                var refreshResponse = await base.SendAsync(refreshRequest, cancellationToken);

                if (refreshResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Token refreshed successfully, retrying original request");

                    // Clone and retry the original request (new cookies set by server)
                    var clonedRequest = await CloneHttpRequestMessageAsync(request);
                    response = await base.SendAsync(clonedRequest, cancellationToken);
                }
                else
                {
                    _logger.LogWarning("Token refresh failed with status: {StatusCode}", refreshResponse.StatusCode);
                    // Return the original 401 response (user needs to re-login)
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
            }
            finally
            {
                _isRefreshing = false;
                _refreshSemaphore.Release();
            }
        }

        return response;
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        // Copy headers
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy content if present
        if (request.Content != null)
        {
            var content = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);

            // Copy content headers
            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _refreshSemaphore?.Dispose();
        }
        base.Dispose(disposing);
    }
}
