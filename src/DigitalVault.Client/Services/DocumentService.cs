using System.Net.Http.Json;
// using DigitalVault.Domain.Entities; // Domain is not directly referenced in Client usually, use Shared DTOs
using Microsoft.AspNetCore.Components.Forms;


namespace DigitalVault.Client.Services;

public class DocumentService
{
    private readonly HttpClient _httpClient;
    private readonly CryptoService _cryptoService;
    private readonly SecureStorageService _storage;

    public DocumentService(HttpClient httpClient, CryptoService cryptoService, SecureStorageService storage)
    {
        _httpClient = httpClient;
        _cryptoService = cryptoService;
        _storage = storage;
    }

    public async Task<bool> UploadDocumentAsync(IBrowserFile file, Guid familyMemberId, string documentType)
    {
        // 1. Get Master Key
        var masterKey = await _storage.GetMasterKeyAsync();
        if (string.IsNullOrEmpty(masterKey)) throw new Exception("Vault locked");

        // 2. Read file
        using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024); // 50MB limit
        var fileBytes = new byte[stream.Length];
        await stream.ReadAsync(fileBytes);

        // 3. Encrypt file
        var encryptionResult = await _cryptoService.EncryptBytesAsync(fileBytes, masterKey);

        // 4. Encrypt Metadata
        var encryptedFilename = await _cryptoService.EncryptDataAsync(file.Name, masterKey);
        var encryptedExt = await _cryptoService.EncryptDataAsync(Path.GetExtension(file.Name), masterKey);
        var encryptedMime = await _cryptoService.EncryptDataAsync(file.ContentType, masterKey);
        var encryptedSize = await _cryptoService.EncryptDataAsync(file.Size.ToString(), masterKey);

        // 5. Get Presigned URL
        var presignedResponse = await _httpClient.PostAsJsonAsync("api/documents/presigned-url/upload", new
        {
            FileName = file.Name,
            ContentType = file.ContentType
        });

        if (!presignedResponse.IsSuccessStatusCode) return false;

        var presignedData = await presignedResponse.Content.ReadFromJsonAsync<PresignedUrlResponse>();

        // 6. Upload to S3 (Directly)
        // We use a separate HttpClient to avoid attaching API auth headers to S3 request
        using var s3Client = new HttpClient();
        var content = new ByteArrayContent(encryptionResult.EncryptedData);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

        var s3Response = await s3Client.PutAsync(presignedData.UploadUrl, content);
        if (!s3Response.IsSuccessStatusCode) return false;

        // 7. Save Metadata to API
        var metadata = new
        {
            FamilyMemberId = familyMemberId,
            DocumentType = documentType,
            S3ObjectKey = presignedData.ObjectKey,
            EncryptedOriginalFileName = encryptedFilename,
            EncryptedFileExtension = encryptedExt,
            EncryptedFileSize = encryptedSize,
            EncryptedMimeType = encryptedMime,
            EncryptionIV = Convert.ToBase64String(encryptionResult.Iv),
            EncryptionTag = "" // GCM tag is often part of ciphertext in WebCrypto, implicit
        };

        var finalResponse = await _httpClient.PostAsJsonAsync("api/documents", metadata);
        return finalResponse.IsSuccessStatusCode;
    }
}

public class PresignedUrlResponse
{
    public string UploadUrl { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
}
