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
    // NOTE: Auth tokens now stored in httpOnly cookies (managed by browser)
    // Only master key remains in sessionStorage for client-side encryption

    public async Task SaveMasterKeyAsync(string masterKey)
    {
        // CRITICAL: Master key should ONLY be stored in session storage (cleared on close)
        // NEVER store in local storage (would persist to disk)
        await SetSessionItemAsync("masterKey", masterKey);
    }

    public async Task<string?> GetMasterKeyAsync()
    {
        // Check SessionStorage first (Temp session)
        var key = await GetSessionItemAsync<string>("masterKey");
        if (!string.IsNullOrEmpty(key)) return key;

        // Fallback to LocalStorage (Persistent session)
        return await GetLocalItemAsync<string>("masterKey");
    }

    public async Task ClearAuthDataAsync()
    {
        // Clear from both locations to ensure complete logout
        await RemoveSessionItemAsync("masterKey");
        await RemoveLocalItemAsync("masterKey");

        await RemoveSessionItemAsync("userInfo");
        await RemoveLocalItemAsync("userInfo");
    }
}
