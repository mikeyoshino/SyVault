using Microsoft.JSInterop;
using System.Text.Json;

namespace DigitalVault.Client.Services;

/// <summary>
/// Secure storage service for Blazor WebAssembly
/// Uses browser's sessionStorage and localStorage via JS Interop
/// </summary>
public class SecureStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public SecureStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    // Generic Storage Methods (Session Storage)

    public async Task SaveAsync(string key, string value)
    {
        await SetSessionItemAsync(key, value);
    }

    public async Task<string?> GetAsync(string key)
    {
        return await GetSessionItemAsync<string>(key);
    }

    public async Task RemoveAsync(string key)
    {
        await RemoveSessionItemAsync(key);
    }

    // Session Storage (cleared when browser closes)

    public async Task SetSessionItemAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", key, json);
    }

    public async Task<T?> GetSessionItemAsync<T>(string key)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", key);
            return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public async Task RemoveSessionItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", key);
    }

    // Local Storage (persists across browser sessions)

    public async Task SetLocalItemAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    public async Task<T?> GetLocalItemAsync<T>(string key)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public async Task RemoveLocalItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }

    // Zero-knowledge encryption methods
    // Master Key is NEVER stored in plaintext
    // Only encrypted version persists in localStorage

    /// <summary>
    /// Save encrypted Master Key to localStorage (persistent, survives refresh)
    /// </summary>
    public async Task SaveEncryptedMasterKeyAsync(string encryptedMasterKey, byte[] salt)
    {
        var data = new EncryptedKeyData
        {
            EncryptedKey = encryptedMasterKey,
            Salt = Convert.ToBase64String(salt)
        };
        await SetLocalItemAsync("encryptedMasterKey", data);
    }

    /// <summary>
    /// Get encrypted Master Key from localStorage
    /// </summary>
    public async Task<EncryptedKeyData?> GetEncryptedMasterKeyAsync()
    {
        return await GetLocalItemAsync<EncryptedKeyData>("encryptedMasterKey");
    }

    /// <summary>
    /// Save decrypted Master Key to sessionStorage ONLY (cleared on browser close/refresh)
    /// </summary>
    public async Task SaveMasterKeyAsync(string masterKey)
    {
        // ONLY save to sessionStorage (temporary, in-memory)
        // NEVER save plaintext key to localStorage
        Console.WriteLine($"💾 Saving Master Key to sessionStorage (length: {masterKey?.Length ?? 0})");
        await SetSessionItemAsync("masterKey", masterKey);
        Console.WriteLine("✅ Master Key saved to sessionStorage");
    }

    /// <summary>
    /// Get decrypted Master Key from sessionStorage
    /// Returns null if vault is locked (requires password to unlock)
    /// </summary>
    public async Task<string?> GetMasterKeyAsync()
    {
        // Only check sessionStorage (temporary session)
        // If not found, vault is locked - user must unlock with password
        Console.WriteLine("🔍 Checking for Master Key in sessionStorage...");
        var key = await GetSessionItemAsync<string>("masterKey");

        if (string.IsNullOrEmpty(key))
        {
            Console.WriteLine("❌ Master Key NOT found in sessionStorage - Vault is LOCKED");
        }
        else
        {
            Console.WriteLine($"✅ Master Key found in sessionStorage (length: {key.Length})");
        }

        return key;
    }

    /// <summary>
    /// Check if vault is locked (Master Key not in session)
    /// </summary>
    public async Task<bool> IsVaultLockedAsync()
    {
        var masterKey = await GetMasterKeyAsync();
        return string.IsNullOrEmpty(masterKey);
    }

    public async Task ClearAuthDataAsync()
    {
        // Clear decrypted Master Key from session
        await RemoveSessionItemAsync("masterKey");

        // Clear encrypted Master Key from localStorage
        await RemoveLocalItemAsync("encryptedMasterKey");

        await RemoveSessionItemAsync("userInfo");
        await RemoveLocalItemAsync("userInfo");
    }
}

/// <summary>
/// Data structure for encrypted Master Key storage
/// </summary>
public class EncryptedKeyData
{
    public string EncryptedKey { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty; // Base64 encoded
}
