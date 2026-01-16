using Microsoft.AspNetCore.Authentication.Cookies;
using DigitalVault.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages for Login/Register
builder.Services.AddRazorPages();

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "HeritageVault.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS in production
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // Keep logged in for 30 days
        options.SlidingExpiration = true; // Extend expiration on each request
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
    });

builder.Services.AddAuthorization(options =>
{
    // Default policy requires authentication
    options.FallbackPolicy = options.DefaultPolicy;

    // Named policy for explicit use
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});

// Configure YARP Reverse Proxy
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add HttpClient for BFF to call API
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
});

// Add antiforgery for CSRF protection
builder.Services.AddAntiforgery();

var app = builder.Build();

// Configure middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Log all incoming requests (before authentication)
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var method = context.Request.Method;
    app.Logger.LogInformation("📥 Incoming: {Method} {Path}", method, path);
    await next();
    app.Logger.LogInformation("📤 Response: {Method} {Path} → {StatusCode}", method, path, context.Response.StatusCode);
});

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// JWT injection middleware (must be after authentication)
app.UseMiddleware<JwtInjectionMiddleware>();

// Map Razor Pages for auth (MUST be before YARP to avoid being proxied)
app.MapRazorPages().AllowAnonymous(); // Login/Register are explicitly marked with [AllowAnonymous]

// Map YARP reverse proxy with authentication
// Routes are configured in appsettings.json with per-route authorization policies
//   - /api/* -> API server (localhost:5135) [Requires Auth]
//   - /* -> Blazor WASM server (localhost:5067) [Requires Auth]
app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.UseAuthentication();
    proxyPipeline.UseAuthorization();
}); // Authorization enforced by route-level policies in appsettings.json

app.Logger.LogInformation("BFF started. Proxying:");
app.Logger.LogInformation("  - /api/* → http://localhost:5135 (API)");
app.Logger.LogInformation("  - /* → http://localhost:5067 (Blazor WASM)");

app.Run();
