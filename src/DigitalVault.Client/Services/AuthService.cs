using System.Net.Http.Json;
using System.Text.Json;
using DigitalVault.Shared.DTOs.Auth;
using DigitalVault.Shared.DTOs.Common;

namespace DigitalVault.Client.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly SecureStorageService _secureStorage;
    private readonly CryptoService _cryptoService;
    private readonly TokenRefreshService _tokenRefreshService;
    private readonly CustomAuthenticationStateProvider _authStateProvider;

    public AuthService(
        HttpClient httpClient,
        SecureStorageService secureStorage,
        CryptoService cryptoService,
        TokenRefreshService tokenRefreshService,
        CustomAuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _secureStorage = secureStorage;
        _cryptoService = cryptoService;
        _tokenRefreshService = tokenRefreshService;
        _authStateProvider = authStateProvider;
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string? phoneNumber = null)
    {
        try
        {
            // Step 1: Generate random 256-bit master key (client-side only!)
            // var masterKey = await _cryptoService.GenerateMasterKeyAsync();
            // TODO: Move Master Key generation to Account Creation step

            // Step 5: Send to server
            var registerRequest = new RegisterRequest
            {
                Email = email,
                Password = password,
                PhoneNumber = phoneNumber
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", registerRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ApiResponse<UserDto>? errorResponse = null;

                try
                {
                    errorResponse = JsonSerializer.Deserialize<ApiResponse<UserDto>>(
                        errorContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch { }

                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = errorResponse?.Message ?? "การลงทะเบียนล้มเหลว"
                };
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();

            if (result?.Success == true && result.Data != null)
            {
                // Tokens now in httpOnly cookies (set by server automatically)

                // Save master key to session storage (NEVER to local storage!)
                // await _secureStorage.SaveMasterKeyAsync(masterKey); // Removing this as masterKey is not available yet

                // Start automatic token refresh (tokens expire in 60 minutes)
                _tokenRefreshService.StartAutoRefresh(DateTime.UtcNow.AddMinutes(60));

                // Save login flag
                await _secureStorage.SaveAsync("isLoggedIn", "true");
                await _secureStorage.SaveAsync("userEmail", result.Data.Email);

                // Notify authentication state changed
                _authStateProvider.NotifyAuthenticationStateChanged();

                return new AuthResult
                {
                    Success = true,
                    User = result.Data,
                    MasterKey = null // Master Key will be created in next step (Account Setup)
                };
            }

            return new AuthResult
            {
                Success = false,
                ErrorMessage = result?.Message ?? "เกิดข้อผิดพลาดในการลงทะเบียน"
            };
        }
        catch (Exception ex)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = $"เกิดข้อผิดพลาด: {ex.Message}"
            };
        }
    }

    public async Task<AuthResult> LoginAsync(string email, string password, string? mfaCode = null, bool rememberMe = false)
    {
        try
        {
            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password,
                MfaCode = mfaCode
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ApiResponse<UserDto>? errorResponse = null;

                try
                {
                    errorResponse = JsonSerializer.Deserialize<ApiResponse<UserDto>>(
                        errorContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch { }

                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = errorResponse?.Message ?? "การเข้าสู่ระบบล้มเหลว"
                };
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();

            if (result?.Success == true && result.Data != null)
            {
                // Tokens now in httpOnly cookies (set by server automatically)
                var user = result.Data;

                // Save login flag
                await _secureStorage.SaveAsync("isLoggedIn", "true");
                await _secureStorage.SaveAsync("userEmail", user.Email);

                // TODO: Fetch Account list to get EncryptedMasterKey
                // For now, return success without MasterKey (will be handled by AccountService)

                // Start automatic token refresh (tokens expire in 60 minutes)
                _tokenRefreshService.StartAutoRefresh(DateTime.UtcNow.AddMinutes(60));

                // Notify authentication state changed
                _authStateProvider.NotifyAuthenticationStateChanged();

                return new AuthResult
                {
                    Success = true,
                    User = result.Data,
                    MasterKey = null // MasterKey will be loaded later
                };
            }

            return new AuthResult
            {
                Success = false,
                ErrorMessage = result?.Message ?? "เกิดข้อผิดพลาดในการเข้าสู่ระบบ"
            };
        }
        catch (Exception ex)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = $"เกิดข้อผิดพลาด: {ex.Message}"
            };
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            // Call server to revoke refresh token and clear cookies
            await _httpClient.PostAsync("/api/auth/logout", null);
        }
        catch
        {
            // Continue with local cleanup even if server call fails
        }

        // Stop auto-refresh
        _tokenRefreshService.StopAutoRefresh();

        // Clear authenticaton data
        await _secureStorage.RemoveAsync("isLoggedIn");
        await _secureStorage.RemoveAsync("userEmail");
        await _secureStorage.ClearAuthDataAsync();

        // Notify authentication state changed
        _authStateProvider.NotifyAuthenticationStateChanged();
    }

    public async Task LogoutAllDevicesAsync()
    {
        try
        {
            // Call server to revoke all refresh tokens
            await _httpClient.PostAsync("/api/auth/logout-all", null);
        }
        catch
        {
            // Continue with local cleanup even if server call fails
        }

        // Stop auto-refresh
        _tokenRefreshService.StopAutoRefresh();

        // Clear authenticaton data
        await _secureStorage.RemoveAsync("isLoggedIn");
        await _secureStorage.RemoveAsync("userEmail");
        await _secureStorage.ClearAuthDataAsync();

        // Notify authentication state changed
        _authStateProvider.NotifyAuthenticationStateChanged();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        // Check if user is logged in
        var isLoggedIn = await _secureStorage.GetAsync("isLoggedIn");
        return !string.IsNullOrEmpty(isLoggedIn) && bool.Parse(isLoggedIn);
    }

    public async Task<string?> GetMasterKeyAsync()
    {
        return await _secureStorage.GetMasterKeyAsync();
    }
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public UserDto? User { get; set; }
    public string? MasterKey { get; set; }
}
