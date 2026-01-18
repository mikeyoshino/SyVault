using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using DigitalVault.Client;
using DigitalVault.Client.Services;
using DigitalVault.Client.Handlers;
using Blazorise;
using Blazorise.Tailwind;
using Blazorise.Icons.FontAwesome;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register authentication handler
builder.Services.AddTransient<AuthenticationHandler>();

// Configure HttpClient with authentication handler
// Note: Cookies are automatically handled by the browser in Blazor WebAssembly
builder.Services.AddScoped(sp =>
{
    var authHandler = sp.GetRequiredService<AuthenticationHandler>();

    // Use default HttpMessageHandler (browser handles cookies automatically)
    authHandler.InnerHandler = new HttpClientHandler();

    var httpClient = new HttpClient(authHandler)
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) // BFF proxy
    };

    return httpClient;
});

// Register services for zero-knowledge encryption and token refresh
builder.Services.AddScoped<CryptoService>();
builder.Services.AddScoped<SecureStorageService>();
builder.Services.AddScoped<TokenRefreshService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<FamilyMemberService>();
builder.Services.AddScoped<VaultUnlockService>();

// Register authentication state provider
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// Add authorization services
builder.Services.AddAuthorizationCore();

// Register Blazorise services with Tailwind provider
builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddTailwindProviders()
    .AddFontAwesomeIcons();

await builder.Build().RunAsync();
