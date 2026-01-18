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

                // SECURE KEY MANAGEMENT - Fetch from Database
                // 1. Fetch encrypted Master Key from server
                EncryptedMasterKeyDto? serverEncryptedKey = null;
                try
                {
                    var keyResponse = await _httpClient.GetFromJsonAsync<ApiResponse<EncryptedMasterKeyDto>>(
                        "/api/auth/encrypted-master-key"
                    );
                    serverEncryptedKey = keyResponse?.Data;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to fetch encrypted key from server: {ex.Message}");
                }

                string masterKey;
                byte[] salt;

                if (serverEncryptedKey != null && !string.IsNullOrEmpty(serverEncryptedKey.EncryptedMasterKey))
                {
                    // 2. Decrypt existing Master Key from server
                    Console.WriteLine("Found encrypted Master Key in database");

                    salt = Convert.FromBase64String(serverEncryptedKey.MasterKeySalt);
                    var passwordDerivedKey = await _cryptoService.DeriveKeyFromPasswordAsync(
                        password,
                        salt,
                        100000
                    );

                    try
                    {
                        masterKey = await _cryptoService.DecryptMasterKeyAsync(
                            serverEncryptedKey.EncryptedMasterKey,
                            passwordDerivedKey
                        );
                        Console.WriteLine("✅ Decrypted Master Key from database");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to decrypt Master Key: {ex.Message}");
                        throw new Exception("Incorrect password or corrupted encryption key");
                    }
                }
                else
                {
                    // 3. First time login - Master Key not in database yet
                    // This will be set up during registration or first document upload
                    Console.WriteLine("⚠️ No Master Key in database - will be created on first use");

                    // Generate temporary Master Key for this session
                    // In production, this should trigger a setup flow
                    masterKey = await _cryptoService.GenerateMasterKeyAsync();
                    salt = await _cryptoService.GenerateRandomBytesAsync(16);

                    var passwordDerivedKey = await _cryptoService.DeriveKeyFromPasswordAsync(
                        password,
                        salt,
                        100000
                    );

                    var encryptedMasterKey = await _cryptoService.EncryptMasterKeyAsync(
                        masterKey,
                        passwordDerivedKey
                    );

                    // Save to database via API
                    try
                    {
                        var saveDto = new EncryptedMasterKeyDto
                        {
                            EncryptedMasterKey = encryptedMasterKey,
                            MasterKeySalt = Convert.ToBase64String(salt),
                            AuthenticationTag = "" // Not used in current implementation
                        };

                        var saveResponse = await _httpClient.PostAsJsonAsync(
                            "/api/auth/encrypted-master-key",
                            saveDto
                        );

                        if (saveResponse.IsSuccessStatusCode)
                        {
                            Console.WriteLine("✅ Encrypted Master Key saved to database");
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Failed to save encrypted key to database: {saveResponse.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error saving encrypted key to database: {ex.Message}");
                    }

                    // Also save to localStorage as backup
                    await _secureStorage.SaveEncryptedMasterKeyAsync(encryptedMasterKey, salt);
                }

                // 4. Save decrypted Master Key to sessionStorage (temporary, fast access)
                await _secureStorage.SaveMasterKeyAsync(masterKey);

                Console.WriteLine("✅ Master Key ready for use");

                // Start automatic token refresh (tokens expire in 60 minutes)
                _tokenRefreshService.StartAutoRefresh(DateTime.UtcNow.AddMinutes(60));

                // Notify authentication state changed
                _authStateProvider.NotifyAuthenticationStateChanged();

                return new AuthResult
                {
                    Success = true,
                    User = result.Data,
                    MasterKey = masterKey // Return the decrypted key for immediate use
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
