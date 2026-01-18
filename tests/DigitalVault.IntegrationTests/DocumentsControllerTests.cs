using System.Net.Http.Json;
using DigitalVault.API.Controllers;
using DigitalVault.Shared.DTOs.Documents;
using FluentAssertions;
using Xunit;

namespace DigitalVault.IntegrationTests;

public class DocumentsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DocumentsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUploadUrl_ReturnsOk_WithUrlAndKey()
    {
        // Arrange
        var request = new UploadRequest
        {
            FileName = "test-document.pdf",
            ContentType = "application/pdf"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/documents/presigned-url/upload", request);

        // Assert
        // Assert
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed: {response.StatusCode} - {content}");
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UploadUrlResponse>();

        result.Should().NotBeNull();
        result!.UploadUrl.Should().NotBeNullOrEmpty();
        result.ObjectKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateDocument_ReturnsCreated()
    {
        // Arrange
        var request = new DocumentDto
        {
            FamilyMemberId = Guid.NewGuid(),
            DocumentType = "Identity",
            S3ObjectKey = "test/key",
            EncryptedOriginalFileName = "enc_name",
            EncryptedFileExtension = ".enc",
            EncryptedFileSize = "1024",
            EncryptedMimeType = "application/pdf",
            EncryptionIV = "iv",
            EncryptionTag = "tag"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/documents", request);

        // Assert
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed: {response.StatusCode} - {content}");
        }

        response.EnsureSuccessStatusCode();
        // Depending on implementation, it might return the created document or just status
    }
}

public class UploadUrlResponse
{
    public string UploadUrl { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
}
