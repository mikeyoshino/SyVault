using Microsoft.JSInterop;

namespace DigitalVault.BlazorApp.Services;

/// <summary>
/// Service for client-side cryptographic operations using Web Crypto API
/// Implements zero-knowledge encryption where server never sees plaintext keys
/// </summary>
public class CryptoService
{
    private readonly IJSRuntime _jsRuntime;

    public CryptoService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Generate a random 256-bit (32-byte) master encryption key
    /// </summary>
    public async Task<string> GenerateMasterKeyAsync()
    {
        return await _jsRuntime.InvokeAsync<string>("cryptoHelper.generateMasterKey");
    }

    /// <summary>
    /// Derive an encryption key from a password using PBKDF2
    /// </summary>
    /// <param name="password">User's password</param>
    /// <param name="salt">Salt bytes (16 bytes recommended)</param>
    /// <param name="iterations">Number of iterations (100000 recommended)</param>
    public async Task<string> DeriveKeyFromPasswordAsync(string password, byte[] salt, int iterations)
    {
        return await _jsRuntime.InvokeAsync<string>(
            "cryptoHelper.deriveKeyFromPassword",
            password,
            salt,
            iterations
        );
    }

    /// <summary>
    /// Encrypt master key with password-derived key (AES-256-GCM)
    /// </summary>
    /// <param name="masterKey">Base64 master key</param>
    /// <param name="passwordDerivedKey">Derived key handle from deriveKeyFromPassword</param>
    public async Task<string> EncryptMasterKeyAsync(string masterKey, string passwordDerivedKey)
    {
        return await _jsRuntime.InvokeAsync<string>(
            "cryptoHelper.encryptMasterKey",
            masterKey,
            passwordDerivedKey
        );
    }

    /// <summary>
    /// Decrypt master key with password-derived key (AES-256-GCM)
    /// </summary>
    /// <param name="encryptedMasterKey">Base64 encrypted master key</param>
    /// <param name="passwordDerivedKey">Derived key handle from deriveKeyFromPassword</param>
    public async Task<string> DecryptMasterKeyAsync(string encryptedMasterKey, string passwordDerivedKey)
    {
        return await _jsRuntime.InvokeAsync<string>(
            "cryptoHelper.decryptMasterKey",
            encryptedMasterKey,
            passwordDerivedKey
        );
    }

    /// <summary>
    /// Encrypt data with master key (AES-256-GCM)
    /// </summary>
    /// <param name="plaintext">Data to encrypt</param>
    /// <param name="masterKey">Base64 master key</param>
    public async Task<string> EncryptDataAsync(string plaintext, string masterKey)
    {
        return await _jsRuntime.InvokeAsync<string>(
            "cryptoHelper.encryptData",
            plaintext,
            masterKey
        );
    }

    /// <summary>
    /// Decrypt data with master key (AES-256-GCM)
    /// </summary>
    /// <param name="encryptedData">Base64 encrypted data</param>
    /// <param name="masterKey">Base64 master key</param>
    public async Task<string> DecryptDataAsync(string encryptedData, string masterKey)
    {
        return await _jsRuntime.InvokeAsync<string>(
            "cryptoHelper.decryptData",
            encryptedData,
            masterKey
        );
    }

    /// <summary>
    /// Generate random bytes for salt
    /// </summary>
    /// <param name="length">Number of bytes to generate</param>
    public async Task<byte[]> GenerateRandomBytesAsync(int length)
    {
        var base64 = await _jsRuntime.InvokeAsync<string>("cryptoHelper.generateRandomBytes", length);
        return Convert.FromBase64String(base64);
    }
}
