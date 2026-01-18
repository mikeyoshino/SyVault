using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using DigitalVault.Shared.DTOs.Auth;
using DigitalVault.Shared.DTOs.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DigitalVault.Web.Pages;

[AllowAnonymous]
[IgnoreAntiforgeryToken] // Bypass Antiforgery for this page to avoid 400 errors on AJAX
public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LoginModel> _logger;

    [BindProperty]
    [Required(ErrorMessage = "กรุณากรอกอีเมล")]
    [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    public string? ErrorMessage { get; set; }

    public LoginModel(IHttpClientFactory httpClientFactory, ILogger<LoginModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public void OnGet()
    {
        // Display login page
    }

    public async Task<IActionResult> OnPostAsync()
    {
        bool isAjax = Request.Query["ajax"] == "true" ||
                      Request.Headers["Accept"].ToString().Contains("application/json") ||
                      Request.Headers["Content-Type"].ToString().Contains("application/json");

        if (!ModelState.IsValid)
        {
            if (isAjax) return new JsonResult(new { success = false, message = "ข้อมูลไม่ถูกต้อง" });
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var loginRequest = new LoginRequest
            {
                Email = Email,
                Password = Password,
                MfaCode = null
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Login failed for {Email}: {Error}", Email, errorContent);

                if (isAjax) return new JsonResult(new { success = false, message = "อีเมลหรือรหัสผ่านไม่ถูกต้อง" });

                ErrorMessage = "อีเมลหรือรหัสผ่านไม่ถูกต้อง";
                return Page();
            }

            var resultContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<UserDto>>(
                resultContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Success == true && result.Data != null)
            {
                var user = result.Data;

                // Create cookie claims including encryption metadata
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Email), // Critical for Identity.Name
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("userId", user.Id.ToString())
                };

                // Encryption data will be loaded from Accounts later

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                    AllowRefresh = true
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    authProperties);

                _logger.LogInformation("User logged in successfully via BFF: {Email}", Email);

                if (isAjax)
                {
                    // Return success JSON with User Data (Encryption Params)
                    return new JsonResult(new
                    {
                        success = true,
                        data = user,
                        redirectUrl = "/"
                    });
                }
                else
                {
                    // Standard Redirect (Fallback - Note: ZKE won't work on Client yet)
                    return Redirect("/");
                }
            }

            var msg = result?.Message ?? "เกิดข้อผิดพลาดในการเข้าสู่ระบบ";
            if (isAjax) return new JsonResult(new { success = false, message = msg });

            ErrorMessage = msg;
            return Page();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during login for {Email}", Email);
            if (isAjax) return new JsonResult(new { success = false, message = "ไม่สามารถเชื่อมต่อกับเซิร์ฟเวอร์ได้" });

            ErrorMessage = "ไม่สามารถเชื่อมต่อกับเซิร์ฟเวอร์ได้";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for {Email}", Email);
            if (isAjax) return new JsonResult(new { success = false, message = $"เกิดข้อผิดพลาด: {ex.Message}" });

            ErrorMessage = $"เกิดข้อผิดพลาด: {ex.Message}";
            return Page();
        }
    }
}
