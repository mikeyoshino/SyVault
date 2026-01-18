using DigitalVault.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Blazorise;

namespace DigitalVault.Client.Features.FamilyDocuments.Pages;

public partial class PersonDetail
{
    [Parameter]
    public string? PersonId { get; set; }

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private DocumentService DocumentService { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private IMessageService MessageService { get; set; } = default!;

    [Inject]
    private INotificationService NotificationService { get; set; } = default!;

    [Inject]
    private FamilyMemberService FamilyMemberService { get; set; } = default!;

    private bool _showIdCardInfo = false;
    private bool _showDriverLicenseInfo = false;
    private bool _showPassportInfo = false;
    private bool _showHouseRegInfo = false;
    private bool _showBirthCertInfo = false;

    // Changes: Track uploading state per document type
    private Dictionary<string, bool> _uploadingStates = new();

    private List<DigitalVault.Shared.DTOs.Documents.DocumentDto> _documents = new();

    private DigitalVault.Shared.DTOs.FamilyMembers.FamilyMemberDto? _member;

    // Image viewer state
    private Guid? _loadingImage;
    private string? _viewerImageUrl;
    private bool _showViewer;

    protected override async Task OnInitializedAsync()
    {
        await LoadDocuments();
    }

    private async Task LoadDocuments()
    {
        if (PersonId != null && Guid.TryParse(PersonId, out var id))
        {
            // Load Member Details
            _member = await FamilyMemberService.GetFamilyMemberAsync(id);
            if (_member == null)
            {
                await NotificationService.Error("Family member not found");
                Navigation.NavigateTo("/familyid");
                return;
            }

            // Load Documents
            _documents = await DocumentService.GetDocumentsAsync(id);
        }
    }

    private string GetDocumentIcon(string documentType)
    {
        return documentType switch
        {
            "IdCard_Front" or "IdCard_Back" => "fa-id-card",
            "DriverLicense_Front" or "DriverLicense_Back" => "fa-id-card-alt",
            "Passport" => "fa-passport",
            "BirthCertificate" => "fa-baby",
            "MarriageCertificate" => "fa-ring",
            _ => "fa-file"
        };
    }

    private async Task ViewImage(Guid documentId)
    {
        Console.WriteLine($"🖼️ ViewImage called for document: {documentId}");
        _loadingImage = documentId;
        try
        {
            Console.WriteLine($"📥 Downloading and decrypting document...");
            var result = await DocumentService.DownloadAndDecryptDocumentAsync(documentId, Guid.Parse(PersonId));

            Console.WriteLine($"✅ Download result - Data null: {result.Data == null}, Metadata null: {result.Metadata == null}");

            if (result.Data != null)
            {
                Console.WriteLine($"📊 Data length: {result.Data.Length} bytes");
                // Create data URL from decrypted bytes
                _viewerImageUrl = $"data:image/jpeg;base64,{Convert.ToBase64String(result.Data)}";
                Console.WriteLine($"🎨 Created data URL, length: {_viewerImageUrl.Length}");
                _showViewer = true;
                StateHasChanged();
                Console.WriteLine($"✅ Viewer should now be visible");
            }
            else
            {
                Console.WriteLine($"❌ No data returned from decrypt");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error viewing image: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            _loadingImage = null;
            StateHasChanged();
        }
    }

    private async Task OpenDocument(Guid documentId)
    {
        try
        {
            var url = await DocumentService.GetPreviewUrlAsync(documentId);
            if (!string.IsNullOrEmpty(url))
            {
                await JSRuntime.InvokeVoidAsync("open", url, "_blank");
            }
        }
        catch (Exception ex)
        {
            await NotificationService.Error($"ไม่สามารถเปิดไฟล์ได้: {ex.Message}");
        }
    }

    private async Task DeleteDocument(Guid documentId)
    {
        Console.WriteLine($"🗑️ DeleteDocument called for: {documentId}");
        try
        {
            Console.WriteLine("💬 Showing Blazorise confirmation dialog...");

            // Use Blazorise MessageService for confirmation with proper button text
            if (await MessageService.Confirm(
                "คุณแน่ใจหรือไม่ว่าต้องการลบไฟล์นี้? การดำเนินการนี้ไม่สามารถย้อนกลับได้",
                "ยืนยันการลบ",
                options =>
                {
                    options.ConfirmButtonText = "ลบ";
                    options.CancelButtonText = "ยกเลิก";
                }))
            {
                Console.WriteLine("📥 Calling DocumentService.DeleteDocumentAsync...");
                await DocumentService.DeleteDocumentAsync(documentId);
                Console.WriteLine("🔄 Reloading documents...");
                await LoadDocuments();
                Console.WriteLine("✅ Document deleted successfully");
                await NotificationService.Success("ลบไฟล์สำเร็จ");
            }
            else
            {
                Console.WriteLine("❌ User cancelled delete");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Delete error: {ex.Message}");
            await NotificationService.Error($"เกิดข้อผิดพลาด: {ex.Message}");
        }
    }

    private void ToggleIdCardInfo() => _showIdCardInfo = !_showIdCardInfo;
    private void ToggleDriverLicenseInfo() => _showDriverLicenseInfo = !_showDriverLicenseInfo;
    private void TogglePassportInfo() => _showPassportInfo = !_showPassportInfo;
    private void ToggleHouseRegInfo() => _showHouseRegInfo = !_showHouseRegInfo;
    private void ToggleBirthCertInfo() => _showBirthCertInfo = !_showBirthCertInfo;

    private async Task UploadFile(InputFileChangeEventArgs e, string documentType)
    {
        try
        {
            if (PersonId == null || !Guid.TryParse(PersonId, out var personGuid))
            {
                await NotificationService.Error("Invalid Person ID");
                return;
            }

            _uploadingStates[documentType] = true;
            // Force UI refresh so the spinner appears immediately
            StateHasChanged();

            // Slightly delay to ensure UI renders the spinner before heavy encryption starts (which blocks UI thread)
            await Task.Delay(50);

            await NotificationService.Info("กำลังอัปโหลด...");
            var success = await DocumentService.UploadDocumentAsync(e.File, personGuid, documentType);
            if (success)
            {
                await NotificationService.Success("อัปโหลดเอกสารสำเร็จ");
                await LoadDocuments();
            }
            else
            {
                await NotificationService.Error("อัปโหลดไม่สำเร็จ");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Upload error: {ex.Message}");
            await NotificationService.Error($"เกิดข้อผิดพลาด: {ex.Message}");
        }
        finally
        {
            _uploadingStates[documentType] = false;
            StateHasChanged();
        }
    }

    private DigitalVault.Shared.DTOs.Documents.DocumentDto? GetDocumentByType(string type)
    {
        return _documents.FirstOrDefault(d => d.DocumentType == type);
    }
}
