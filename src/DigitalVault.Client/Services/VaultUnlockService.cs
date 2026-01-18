using Microsoft.JSInterop;

namespace DigitalVault.Client.Services;

/// <summary>
/// Service to handle vault unlocking after page refresh
/// Decrypts Master Key from localStorage using user's password
/// </summary>
public class VaultUnlockService
{
    private readonly SecureStorageService _storage;
    private readonly CryptoService _crypto;
    private readonly IJSRuntime _jsRuntime;

    public VaultUnlockService(
        SecureStorageService storage,
        CryptoService crypto,
        IJSRuntime jsRuntime)
    {
        _storage = storage;
        _crypto = crypto;
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Check if vault is currently locked
    /// </summary>
    public async Task<bool> IsVaultLockedAsync()
    {
        return await _storage.IsVaultLockedAsync();
    }

    /// <summary>
    /// Check if encrypted Master Key exists (user has logged in before)
    /// </summary>
    public async Task<bool> HasEncryptedKeyAsync()
    {
        var encryptedKey = await _storage.GetEncryptedMasterKeyAsync();
        return encryptedKey != null;
    }

    /// <summary>
    /// Unlock vault by decrypting Master Key with user's password
    /// </summary>
    /// <param name="password">User's password</param>
    /// <returns>True if unlock successful, false if wrong password</returns>
    public async Task<UnlockResult> UnlockVaultAsync(string password)
    {
        try
        {
            // 1. Get encrypted Master Key from localStorage
            var encryptedKeyData = await _storage.GetEncryptedMasterKeyAsync();

            if (encryptedKeyData == null)
            {
                return new UnlockResult
                {
                    Success = false,
                    ErrorMessage = "No encrypted key found. Please log in again."
                };
            }

            // 2. Derive key from password using stored salt
            var salt = Convert.FromBase64String(encryptedKeyData.Salt);
            var passwordDerivedKey = await _crypto.DeriveKeyFromPasswordAsync(
                password,
                salt,
                100000 // Same iterations as login
            );

            // 3. Decrypt Master Key
            string masterKey;
            try
            {
                masterKey = await _crypto.DecryptMasterKeyAsync(
                    encryptedKeyData.EncryptedKey,
                    passwordDerivedKey
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decryption failed: {ex.Message}");
                return new UnlockResult
                {
                    Success = false,
                    ErrorMessage = "Incorrect password"
                };
            }

            // 4. Save decrypted Master Key to sessionStorage
            await _storage.SaveMasterKeyAsync(masterKey);

            Console.WriteLine("✅ Vault unlocked successfully");

            return new UnlockResult
            {
                Success = true,
                MasterKey = masterKey
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unlock error: {ex.Message}");
            return new UnlockResult
            {
                Success = false,
                ErrorMessage = $"Unlock failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Lock vault by clearing sessionStorage
    /// </summary>
    public async Task LockVaultAsync()
    {
        await _storage.RemoveAsync("masterKey");
        Console.WriteLine("🔒 Vault locked");
    }
}

public class UnlockResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? MasterKey { get; set; }
}
