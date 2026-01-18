using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using DigitalVault.Shared.DTOs.Auth;
using DigitalVault.Shared.DTOs.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DigitalVault.Web.Pages;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RegisterModel> _logger;

    [BindProperty]
    [Required(ErrorMessage = "กรุณากรอกอีเมล")]
    [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "รหัสผ่านต้องมีความยาวอย่างน้อย 8 ตัวอักษร")]
    // [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{8,}$", ErrorMessage = "รหัสผ่านต้องมีตัวพิมพ์เล็ก, ตัวพิมพ์ใหญ่, ตัวเลข และอักขระพิเศษ")]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "กรุณายืนยันรหัสผ่าน")]
    [Compare("Password", ErrorMessage = "รหัสผ่านไม่ตรงกัน")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [BindProperty]
    [Phone(ErrorMessage = "เบอร์โทรศัพท์ไม่ถูกต้อง")]
    public string? PhoneNumber { get; set; }

    // Encryption Keys (Populated by client-side JS)
    [BindProperty]
    public string EncryptedMasterKey { get; set; } = string.Empty;

    [BindProperty]
    public string KeyDerivationSalt { get; set; } = string.Empty;

    [BindProperty]
    public string KeyDerivationIterations { get; set; } = "100000";

    public string? ErrorMessage { get; set; }

    public RegisterModel(IHttpClientFactory httpClientFactory, ILogger<RegisterModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Debugging: Log what we received (excluding password)
        _logger.LogInformation("Registration attempt for {Email}. Salt received: {SaltLength}",
            Email, KeyDerivationSalt?.Length ?? 0);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (string.IsNullOrEmpty(EncryptedMasterKey) || string.IsNullOrEmpty(KeyDerivationSalt))
        {
            ErrorMessage = "เกิดข้อผิดพลาดในการสร้างกุญแจเข้ารหัส กรุณาลองใหม่อีกครั้ง";
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Convert Base64 strings back to bytes/int
            byte[] saltBytes;
            try
            {
                saltBytes = Convert.FromBase64String(KeyDerivationSalt);
            }
            catch
            {
                ErrorMessage = "ข้อมูลความปลอดภัยไม่ถูกต้อง";
                return Page();
            }

            if (!int.TryParse(KeyDerivationIterations, out int iterations))
            {
                iterations = 100000;
            }

            var registerRequest = new RegisterRequest
            {
                Email = Email,
                Password = Password,
                ConfirmPassword = ConfirmPassword,
                PhoneNumber = PhoneNumber,
                EncryptedMasterKey = EncryptedMasterKey,
                KeyDerivationSalt = saltBytes,
                KeyDerivationIterations = iterations
            };

            var content = new StringContent(
                JsonSerializer.Serialize(registerRequest),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/auth/register", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                // Try to parse structured error
                try
                {
                    var errorResult = JsonSerializer.Deserialize<ApiResponse<UserDto>>(
                        errorContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    ErrorMessage = errorResult?.Message ?? "การสมัครสมาชิกไม่สำเร็จ";
                }
                catch
                {
                    ErrorMessage = "การสมัครสมาชิกไม่สำเร็จ";
                }

                _logger.LogWarning("Registration failed for {Email}: {Error}", Email, errorContent);
                return Page();
            }

            // Success! Redirect to login
            return Redirect("/Login?registered=true");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for {Email}", Email);
            ErrorMessage = "เกิดข้อผิดพลาดที่ไม่คาดคิด กรุณาลองใหม่ภายหลัง";
            return Page();
        }
    }
}
