using System.Net.Http.Json;
using System.Text.Json;
using DigitalVault.Shared.DTOs.Auth;
using DigitalVault.Shared.DTOs.Common;

namespace DigitalVault.BlazorApp.Services;

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
            var masterKey = await _cryptoService.GenerateMasterKeyAsync();

            // Step 2: Generate random salt for key derivation
            var salt = await _cryptoService.GenerateRandomBytesAsync(16);
            const int iterations = 100000;

            // Step 3: Derive encryption key from password
            var passwordDerivedKey = await _cryptoService.DeriveKeyFromPasswordAsync(
                password, salt, iterations);

            // Step 4: Encrypt master key with password-derived key
            var encryptedMasterKey = await _cryptoService.EncryptMasterKeyAsync(
                masterKey, passwordDerivedKey);

            // Step 5: Send to server (including salt used for encryption!)
            var registerRequest = new RegisterRequest
            {
                Email = email,
                Password = password,
                PhoneNumber = phoneNumber,
                KeyDerivationSalt = salt,  // Send the salt we used
                KeyDerivationIterations = iterations,
                EncryptedMasterKey = encryptedMasterKey
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
                await _secureStorage.SaveMasterKeyAsync(masterKey);

                // Start automatic token refresh (tokens expire in 60 minutes)
                _tokenRefreshService.StartAutoRefresh(DateTime.UtcNow.AddMinutes(60));

                // Notify authentication state changed
                _authStateProvider.NotifyAuthenticationStateChanged();

                return new AuthResult
                {
                    Success = true,
                    User = result.Data,
                    MasterKey = masterKey
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

                // Decrypt master key locally (zero-knowledge!)
                var user = result.Data;

                // Debug logging
                Console.WriteLine($"User Email: {user.Email}");
                Console.WriteLine($"KeyDerivationSalt length: {user.KeyDerivationSalt?.Length ?? 0}");
                Console.WriteLine($"KeyDerivationIterations: {user.KeyDerivationIterations}");
                Console.WriteLine($"EncryptedMasterKey length: {user.EncryptedMasterKey?.Length ?? 0}");

                // Validate encryption data
                if (user.KeyDerivationSalt == null || user.KeyDerivationSalt.Length == 0)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = "บัญชีนี้สร้างด้วยระบบเก่า กรุณาลงทะเบียนใหม่ (This account was created with the old system. Please register a new account)"
                    };
                }

                if (string.IsNullOrEmpty(user.EncryptedMasterKey))
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = "ไม่พบข้อมูลการเข้ารหัส กรุณาลงทะเบียนใหม่ (Encryption data not found. Please register a new account)"
                    };
                }

                var passwordDerivedKey = await _cryptoService.DeriveKeyFromPasswordAsync(
                    password,
                    user.KeyDerivationSalt,
                    user.KeyDerivationIterations);

                var masterKey = await _cryptoService.DecryptMasterKeyAsync(
                    user.EncryptedMasterKey,
                    passwordDerivedKey);

                // Save master key to session storage
                await _secureStorage.SaveMasterKeyAsync(masterKey);

                // Start automatic token refresh (tokens expire in 60 minutes)
                _tokenRefreshService.StartAutoRefresh(DateTime.UtcNow.AddMinutes(60));

                // Notify authentication state changed
                _authStateProvider.NotifyAuthenticationStateChanged();

                return new AuthResult
                {
                    Success = true,
                    User = result.Data,
                    MasterKey = masterKey
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

        // Clear master key from session storage
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

        // Clear master key from session storage
        await _secureStorage.ClearAuthDataAsync();

        // Notify authentication state changed
        _authStateProvider.NotifyAuthenticationStateChanged();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        // Only check master key (tokens are in httpOnly cookies)
        var masterKey = await _secureStorage.GetMasterKeyAsync();
        return !string.IsNullOrEmpty(masterKey);
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
