using System.Net.Http.Json;
using DigitalVault.Shared.DTOs.Documents;
using Microsoft.AspNetCore.Components.Forms;


namespace DigitalVault.Client.Services;

public class DocumentService
{
    private readonly HttpClient _httpClient;
    private readonly CryptoService _crypto;
    private readonly SecureStorageService _storage;

    public DocumentService(HttpClient httpClient, CryptoService crypto, SecureStorageService storage)
    {
        _httpClient = httpClient;
        _crypto = crypto;
        _storage = storage;
    }

    public async Task<bool> UploadDocumentAsync(IBrowserFile file, Guid familyMemberId, string documentType)
    {
        // 1. Get Master Key from session
        var masterKey = await _storage.GetMasterKeyAsync();

        if (string.IsNullOrEmpty(masterKey))
        {
            throw new Exception("Vault locked - please unlock with your password");
        }

        // 2. Read file
        using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024); // 50MB limit
        var fileBytes = new byte[stream.Length];
        await stream.ReadAsync(fileBytes);

        // 3. Encrypt file
        var encryptionResult = await _crypto.EncryptBytesAsync(fileBytes, masterKey);

        // 4. Encrypt Metadata
        var encryptedFileName = await _crypto.EncryptDataAsync(file.Name, masterKey);
        var encryptedFileExt = await _crypto.EncryptDataAsync(Path.GetExtension(file.Name), masterKey);
        var encryptedFileSize = await _crypto.EncryptDataAsync(file.Size.ToString(), masterKey);
        var encryptedMimeType = await _crypto.EncryptDataAsync(file.ContentType, masterKey);

        // 5. Get Presigned URL
        Console.WriteLine("📡 Requesting presigned URL from API...");
        var presignedResponse = await _httpClient.PostAsJsonAsync("api/documents/presigned-url/upload", new
        {
            FileName = file.Name,
            ContentType = file.ContentType
        });

        Console.WriteLine($"📡 Presigned URL response status: {presignedResponse.StatusCode}");

        if (!presignedResponse.IsSuccessStatusCode)
        {
            var errorContent = await presignedResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ Presigned URL request failed: {presignedResponse.StatusCode}");
            Console.WriteLine($"❌ Error details: {errorContent}");
            return false;
        }

        Console.WriteLine("✅ Presigned URL received successfully");

        var presignedData = await presignedResponse.Content.ReadFromJsonAsync<PresignedUrlResponse>();

        if (presignedData == null)
        {
            Console.WriteLine("❌ Failed to parse presigned URL response");
            return false;
        }

        // 6. Upload to S3 (Directly)
        // We use a separate HttpClient to avoid attaching API auth headers to S3 request
        using var s3Client = new HttpClient();
        var encryptedBytes = Convert.FromBase64String(encryptionResult.EncryptedData);
        var content = new ByteArrayContent(encryptedBytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

        // Note: SSE header removed for MinIO compatibility in dev
        // content.Headers.Add("x-amz-server-side-encryption", "AES256");

        var s3Response = await s3Client.PutAsync(presignedData.UploadUrl, content);
        if (!s3Response.IsSuccessStatusCode) return false;

        // 7. Save Metadata to API
        var metadata = new
        {
            FamilyMemberId = familyMemberId,
            DocumentType = documentType,
            S3ObjectKey = presignedData.ObjectKey,
            EncryptedOriginalFileName = encryptedFileName,
            EncryptedFileExtension = encryptedFileExt,
            EncryptedFileSize = encryptedFileSize,
            EncryptedMimeType = encryptedMimeType,
            EncryptionIV = encryptionResult.Iv, // Already base64 string from JavaScript
            EncryptionTag = "" // GCM tag is often part of ciphertext in WebCrypto, implicit
        };

        var finalResponse = await _httpClient.PostAsJsonAsync("api/documents", metadata);
        return finalResponse.IsSuccessStatusCode;
    }
    public async Task<List<DocumentDto>> GetDocumentsAsync(Guid familyMemberId)
    {
        return await _httpClient.GetFromJsonAsync<List<DocumentDto>>($"api/documents/family/{familyMemberId}") ?? new List<DocumentDto>();
    }

    public async Task DeleteDocumentAsync(Guid documentId)
    {
        await _httpClient.DeleteAsync($"api/documents/{documentId}");
    }

    public async Task<string> GetPreviewUrlAsync(Guid documentId)
    {
        var response = await _httpClient.GetFromJsonAsync<PreviewUrlResponse>($"api/documents/{documentId}/preview-url");
        return response?.Url ?? string.Empty;
    }

    public async Task<(byte[]? Data, DocumentDto? Metadata)> DownloadAndDecryptDocumentAsync(Guid documentId, Guid familyMemberId)
    {
        try
        {
            Console.WriteLine($"📥 Starting download for document {documentId}");

            // 1. Get document metadata
            var documents = await GetDocumentsAsync(familyMemberId);
            var document = documents.FirstOrDefault(d => d.Id == documentId);
            if (document == null)
            {
                Console.WriteLine($"❌ Document not found in list");
                return (null, null);
            }
            Console.WriteLine($"✅ Found document metadata, IV: {document.EncryptionIV?.Substring(0, 10)}...");

            // 2. Get presigned download URL
            var downloadUrl = await GetPreviewUrlAsync(documentId);
            if (string.IsNullOrEmpty(downloadUrl))
            {
                Console.WriteLine($"❌ Failed to get presigned URL");
                return (null, null);
            }
            Console.WriteLine($"✅ Got presigned URL: {downloadUrl.Substring(0, 50)}...");

            // 3. Download encrypted file from S3
            using var s3Client = new HttpClient();
            Console.WriteLine($"📡 Downloading from S3...");
            var encryptedBytes = await s3Client.GetByteArrayAsync(downloadUrl);
            Console.WriteLine($"✅ Downloaded {encryptedBytes.Length} encrypted bytes");

            // 4. Get master key
            var masterKey = await _storage.GetMasterKeyAsync();
            if (string.IsNullOrEmpty(masterKey))
            {
                Console.WriteLine($"❌ No master key found");
                return (null, null);
            }
            Console.WriteLine($"✅ Got master key (length: {masterKey.Length})");

            // 5. Decrypt using IV from document metadata
            Console.WriteLine($"🔓 Decrypting with IV...");
            var decryptedBytes = await _crypto.DecryptBytesAsync(encryptedBytes, document.EncryptionIV, masterKey);
            Console.WriteLine($"✅ Decrypted {decryptedBytes.Length} bytes");

            return (decryptedBytes, document);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error downloading/decrypting: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            return (null, null);
        }
    }
}

public class PreviewUrlResponse
{
    public string Url { get; set; } = string.Empty;
}

public class PresignedUrlResponse
{
    public string UploadUrl { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
}
