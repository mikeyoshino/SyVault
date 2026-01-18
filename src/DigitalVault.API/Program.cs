using System.Text;
using DigitalVault.API.Middleware;
using DigitalVault.Application;
using DigitalVault.Application.Interfaces;
using DigitalVault.Infrastructure;
using DigitalVault.Infrastructure.Data;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using Amazon.S3;
using DigitalVault.Infrastructure.Services;
using DigitalVault.Infrastructure.Repositories;
using DigitalVault.Logic.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Entity Framework Core with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("DigitalVault.Infrastructure");
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        });

    // Enable sensitive data logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// Configure Hangfire with PostgreSQL storage
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
    }));

// Add Hangfire server
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 1; // Single worker for development
});

builder.Services.Configure<DigitalVault.Infrastructure.Configuration.AwsSettings>(builder.Configuration.GetSection("AWS"));
var awsOptions = builder.Configuration.GetAWSOptions("AWS");

// MinIO / Custom Endpoint Support: Explicitly set credentials if provided in config
var awsConfigSection = builder.Configuration.GetSection("AWS");
var awsAccessKey = awsConfigSection["AccessKey"];
var awsSecretKey = awsConfigSection["SecretKey"];
var serviceUrl = awsConfigSection["ServiceURL"];

if (!string.IsNullOrEmpty(awsAccessKey) && !string.IsNullOrEmpty(awsSecretKey))
{
    awsOptions.Credentials = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
}

// Force ServiceURL if present (crucial for MinIO)
if (!string.IsNullOrEmpty(serviceUrl))
{
    // awsOptions.Config // Config is internal/protected in some versions, but mapped via json usually.
    // However, if automatic mapping fails, we might need to rely on the library.
    // But usually GetAWSOptions handles ServiceURL if it's in the JSON.
    // The main issue was likely Credentials.
}

// Configure S3 client manually for MinIO HTTP support
var forcePathStyle = awsConfigSection.GetValue<bool>("ForcePathStyle", true);
var region = awsConfigSection["Region"] ?? "us-east-1";

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = new Amazon.S3.AmazonS3Config
    {
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region),
        ForcePathStyle = forcePathStyle,
        UseHttp = true // CRITICAL: Use HTTP for local MinIO
    };

    if (!string.IsNullOrEmpty(serviceUrl))
    {
        // Ensure protocol is included
        if (!serviceUrl.StartsWith("http://") && !serviceUrl.StartsWith("https://"))
        {
            serviceUrl = "http://" + serviceUrl;
        }
        config.ServiceURL = serviceUrl;
    }

    if (!string.IsNullOrEmpty(awsAccessKey) && !string.IsNullOrEmpty(awsSecretKey))
    {
        var credentials = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
        return new Amazon.S3.AmazonS3Client(credentials, config);
    }

    return new Amazon.S3.AmazonS3Client(config);
});
builder.Services.AddScoped<IStorageService, S3Service>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<FamilyMemberService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

// Clear default claim mapping to preserve original claim names (e.g. "AccountId")
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Read token from cookie first, fallback to Authorization header
    options.MapInboundClaims = false; // Disable default mapping to preserve custom claims (account_id)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Check cookie first (for browser clients)
            if (context.Request.Cookies.TryGetValue("accessToken", out var token) && !string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            // Fallback to Authorization header (for API clients like Swagger/Postman)
            else if (string.IsNullOrEmpty(context.Token))
            {
                var authorization = context.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authorization.Substring("Bearer ".Length).Trim();
                }
            }
            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure Cookie Policy for authentication cookies
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    options.Secure = builder.Environment.IsProduction()
        ? CookieSecurePolicy.Always  // HTTPS only in production
        : CookieSecurePolicy.None;   // Allow HTTP in development
});

// Configure Swagger/OpenAPI with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Digital Vault API",
        Version = "v1",
        Description = "Zero-Knowledge Digital Inheritance Platform API"
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5000",  // BFF HTTP
                "https://localhost:5001", // BFF HTTPS
                "http://localhost:5067",  // Blazor WebAssembly direct (dev only)
                "https://localhost:7067", // Blazor HTTPS direct (dev only)
                "https://localhost:5002",
                "http://localhost:5003",
                "https://localhost:7000"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline

// Global error handling
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Digital Vault API v1");
        options.RoutePrefix = "swagger";
    });
}

// app.UseHttpsRedirection(); // Disable HTTPS redirection in Dev to allow YARP HTTP forwarding
app.UseCors("AllowBlazorClient");

// Use Hangfire Dashboard (only in development for security)
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });
}

// Apply cookie policy before authentication
app.UseCookiePolicy();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}))
.WithName("HealthCheck")
.WithTags("Health");

// Apply migrations on startup in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        await db.Database.MigrateAsync();
        app.Logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the database");
    }
}

// Schedule recurring jobs for Dead Man's Switch
RecurringJob.AddOrUpdate<IDeadManSwitchJobService>(
    "check-overdue-switches",
    service => service.CheckOverdueSwitchesAsync(),
    Cron.Hourly); // Run every hour

RecurringJob.AddOrUpdate<IDeadManSwitchJobService>(
    "process-grace-period-switches",
    service => service.ProcessGracePeriodSwitchesAsync(),
    "0 */6 * * *"); // Run every 6 hours

// Schedule cleanup job for expired refresh tokens
RecurringJob.AddOrUpdate<ITokenService>(
    "cleanup-expired-tokens",
    service => service.CleanupExpiredTokensAsync(),
    "0 2 * * *"); // Run daily at 2 AM

app.Logger.LogInformation("Hangfire recurring jobs scheduled successfully");

app.Run();

public partial class Program { }
