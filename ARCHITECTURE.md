# Digital Vault - Dead Man's Switch System Architecture

## System Overview

Digital Vault เป็นระบบ Zero-Knowledge Digital Inheritance Platform ที่พัฒนาด้วย C#/.NET 8 ช่วยให้ผู้ใช้สามารถเก็บข้อมูลลับและส่งต่อให้กับผู้รับมรดก (Heirs) โดยอัตโนมัติเมื่อไม่สามารถ Check-in ได้ภายในระยะเวลาที่กำหนด

### Core Principles
1. **Zero-Knowledge Architecture** - แม้แต่เจ้าของระบบก็ไม่สามารถเข้าถึงข้อมูลของผู้ใช้ได้
2. **Client-Side Encryption** - เข้ารหัสทุกอย่างก่อนส่งไปยัง Server
3. **Dead Man's Switch** - ระบบตรวจสอบอัตโนมัติและส่งต่อข้อมูลเมื่อถึงเงื่อนไข
4. **Trust & Legal Compliance** - ปฏิบัติตาม PDPA และกฎหมายมรดกไทย

---

## Tech Stack (Full C#/.NET)

### Backend
- **.NET 8** (ASP.NET Core Web API)
- **Entity Framework Core 8** (ORM)
- **SQL Server** หรือ **PostgreSQL** (Metadata Storage)
- **Azure Blob Storage** หรือ **AWS S3** (Encrypted File Storage)
- **Redis** (Caching & Session Management)
- **Hangfire** (Background Jobs & Scheduled Tasks)
- **SignalR** (Real-time notifications)
- **MediatR** (CQRS pattern)
- **FluentValidation** (Input validation)
- **Serilog** (Structured logging)

### Frontend
- **Blazor WebAssembly** (.NET 8) - เขียน C# ทั้งหมด ไม่ต้องใช้ JavaScript
- **MudBlazor** หรือ **Radzen Blazor** (UI Component Library)
- **Blazored.LocalStorage** (Client-side storage)
- **C# WebCrypto Wrapper** (Client-side encryption ผ่าน JS Interop)

### Alternative Frontend (ถ้าต้องการ SEO ดีกว่า)
- **ASP.NET Core MVC + Razor Pages**
- **Bootstrap 5** (UI Framework)
- **JavaScript Interop** สำหรับ Web Crypto API

### Security & Encryption
- **System.Security.Cryptography** (.NET built-in)
- **Bouncy Castle** (Advanced crypto operations)
- **AES-256-GCM** (Client-side encryption)
- **RSA-2048** (Heir key pairs)
- **PBKDF2** (Key derivation)
- **JWT** (Authentication tokens)
- **Azure Key Vault** / **AWS Secrets Manager** (Server secrets)

### Infrastructure
- **Docker** + **Docker Compose** (Development)
- **Azure App Service** / **AWS Elastic Beanstalk** (Production)
- **Azure SQL Database** / **AWS RDS**
- **GitHub Actions** (CI/CD)
- **Azure Application Insights** (Monitoring)

---

## Solution Structure

```
DigitalVault/
├── src/
│   ├── DigitalVault.Domain/              # Domain models & interfaces
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── VaultEntry.cs
│   │   │   ├── Heir.cs
│   │   │   ├── DeadManSwitch.cs
│   │   │   └── ...
│   │   ├── ValueObjects/
│   │   │   ├── EncryptedData.cs
│   │   │   ├── EncryptionKey.cs
│   │   │   └── CheckInInterval.cs
│   │   ├── Enums/
│   │   │   ├── SwitchStatus.cs
│   │   │   ├── SubscriptionTier.cs
│   │   │   └── NotificationChannel.cs
│   │   └── Interfaces/
│   │       ├── IRepository.cs
│   │       ├── IEncryptionService.cs
│   │       └── INotificationService.cs
│   │
│   ├── DigitalVault.Application/         # Business logic (CQRS)
│   │   ├── Commands/
│   │   │   ├── CreateVaultEntry/
│   │   │   │   ├── CreateVaultEntryCommand.cs
│   │   │   │   ├── CreateVaultEntryCommandHandler.cs
│   │   │   │   └── CreateVaultEntryValidator.cs
│   │   │   ├── PerformCheckIn/
│   │   │   ├── AddHeir/
│   │   │   └── ...
│   │   ├── Queries/
│   │   │   ├── GetVaultEntries/
│   │   │   ├── GetSwitchStatus/
│   │   │   └── ...
│   │   ├── DTOs/
│   │   │   ├── VaultEntryDto.cs
│   │   │   ├── HeirDto.cs
│   │   │   └── ...
│   │   ├── Services/
│   │   │   ├── EncryptionService.cs
│   │   │   ├── NotificationService.cs
│   │   │   ├── DeadManSwitchService.cs
│   │   │   └── HeirAccessService.cs
│   │   └── Interfaces/
│   │
│   ├── DigitalVault.Infrastructure/      # Data access & external services
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── UserConfiguration.cs
│   │   │   │   ├── VaultEntryConfiguration.cs
│   │   │   │   └── ...
│   │   │   └── Migrations/
│   │   ├── Repositories/
│   │   │   ├── VaultRepository.cs
│   │   │   ├── HeirRepository.cs
│   │   │   └── ...
│   │   ├── Services/
│   │   │   ├── BlobStorageService.cs
│   │   │   ├── EmailService.cs
│   │   │   ├── SmsService.cs
│   │   │   └── CacheService.cs
│   │   └── BackgroundJobs/
│   │       ├── DeadManSwitchCheckJob.cs
│   │       ├── NotificationJob.cs
│   │       └── DataCleanupJob.cs
│   │
│   ├── DigitalVault.API/                 # Web API
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── VaultController.cs
│   │   │   ├── HeirController.cs
│   │   │   ├── SwitchController.cs
│   │   │   └── HeirAccessController.cs
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   ├── RateLimitingMiddleware.cs
│   │   │   └── AuditLoggingMiddleware.cs
│   │   ├── Filters/
│   │   │   ├── ValidateModelAttribute.cs
│   │   │   └── AuthorizeAttribute.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   │
│   ├── DigitalVault.BlazorApp/           # Blazor WebAssembly Client
│   │   ├── Pages/
│   │   │   ├── Index.razor
│   │   │   ├── Login.razor
│   │   │   ├── Vault/
│   │   │   │   ├── VaultList.razor
│   │   │   │   ├── VaultCreate.razor
│   │   │   │   └── VaultEdit.razor
│   │   │   ├── Heirs/
│   │   │   │   ├── HeirList.razor
│   │   │   │   └── HeirAdd.razor
│   │   │   ├── Switch/
│   │   │   │   ├── SwitchDashboard.razor
│   │   │   │   └── CheckIn.razor
│   │   │   └── HeirPortal/
│   │   │       └── HeirAccess.razor
│   │   ├── Services/
│   │   │   ├── ApiClient.cs
│   │   │   ├── ClientEncryptionService.cs
│   │   │   ├── AuthService.cs
│   │   │   └── StateContainer.cs
│   │   ├── Components/
│   │   │   ├── VaultEntryCard.razor
│   │   │   ├── HeirCard.razor
│   │   │   └── SwitchStatusWidget.razor
│   │   ├── wwwroot/
│   │   │   ├── js/
│   │   │   │   └── cryptoInterop.js  # Web Crypto API wrapper
│   │   │   └── index.html
│   │   └── Program.cs
│   │
│   └── DigitalVault.Shared/              # Shared models between API & Client
│       ├── DTOs/
│       ├── Enums/
│       └── Constants/
│
├── tests/
│   ├── DigitalVault.UnitTests/
│   │   ├── Application/
│   │   ├── Domain/
│   │   └── Infrastructure/
│   ├── DigitalVault.IntegrationTests/
│   │   ├── API/
│   │   └── BackgroundJobs/
│   └── DigitalVault.E2ETests/
│
├── docs/
│   ├── ARCHITECTURE.md                    # This file
│   ├── API_DOCUMENTATION.md
│   ├── SECURITY_WHITEPAPER.md
│   └── DEPLOYMENT_GUIDE.md
│
├── docker/
│   ├── docker-compose.yml
│   ├── docker-compose.override.yml
│   └── Dockerfile
│
├── .github/
│   └── workflows/
│       ├── build.yml
│       ├── test.yml
│       └── deploy.yml
│
├── DigitalVault.sln
└── README.md
```

---

## Domain Models (C# Entities)

### User.cs
```csharp
namespace DigitalVault.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneVerified { get; set; }

    // Authentication (account password, NOT master encryption key)
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;

    // MFA
    public bool MfaEnabled { get; set; }
    public string? MfaSecret { get; set; }

    // Key Derivation Info (sent to client for key derivation)
    public byte[] KeyDerivationSalt { get; set; } = Array.Empty<byte>();
    public int KeyDerivationIterations { get; set; } = 100000;

    // Subscription
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.Free;
    public DateTime? SubscriptionExpiresAt { get; set; }

    // Relationships
    public ICollection<VaultEntry> VaultEntries { get; set; } = new List<VaultEntry>();
    public ICollection<Heir> Heirs { get; set; } = new List<Heir>();
    public DeadManSwitch? DeadManSwitch { get; set; }

    // Metadata
    public DateTime? LastLoginAt { get; set; }
    public bool IsDeleted { get; set; }
}
```

### VaultEntry.cs
```csharp
namespace DigitalVault.Domain.Entities;

public class VaultEntry : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Entry Info (title is encrypted on client)
    public string Title { get; set; } = string.Empty;
    public VaultCategory Category { get; set; }

    // Encrypted Data
    public byte[] EncryptedDataKey { get; set; } = Array.Empty<byte>(); // DEK encrypted with master key
    public byte[]? EncryptedContent { get; set; } // Small data (passwords, notes)
    public string? BlobStorageUrl { get; set; } // For large files
    public string? BlobStorageKey { get; set; }

    // Encryption Metadata
    public byte[] IV { get; set; } = Array.Empty<byte>(); // Initialization vector
    public string EncryptionAlgorithm { get; set; } = "AES-256-GCM";

    // Access Control
    public bool IsSharedWithHeirs { get; set; } = true;

    // Relationships
    public ICollection<HeirVaultAccess> HeirAccesses { get; set; } = new List<HeirVaultAccess>();

    // Metadata
    public bool IsDeleted { get; set; }
}
```

### Heir.cs
```csharp
namespace DigitalVault.Domain.Entities;

public class Heir : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Heir Information
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty; // Encrypted
    public string? Relationship { get; set; } // 'spouse', 'child', 'sibling', etc.

    // Verification
    public bool IsVerified { get; set; }
    public string? VerificationToken { get; set; }
    public DateTime? VerificationExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }

    // Access Keys (for Zero-Knowledge)
    public byte[] PublicKey { get; set; } = Array.Empty<byte>(); // Heir's RSA public key
    public byte[]? EncryptedPrivateKey { get; set; } // Encrypted with heir's password

    // Permissions
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Full;
    public List<string> CanAccessCategories { get; set; } = new();

    // Relationships
    public ICollection<HeirVaultAccess> VaultAccesses { get; set; } = new List<HeirVaultAccess>();

    // Metadata
    public bool IsDeleted { get; set; }
}
```

### DeadManSwitch.cs
```csharp
namespace DigitalVault.Domain.Entities;

public class DeadManSwitch : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Switch Configuration
    public int CheckInIntervalDays { get; set; } = 90; // 30, 90, 180, 365
    public int GracePeriodDays { get; set; } = 14;

    // Status
    public bool IsActive { get; set; } = true;
    public DateTime LastCheckInAt { get; set; } = DateTime.UtcNow;
    public DateTime NextCheckInDueDate { get; set; }

    // Trigger Status
    public SwitchStatus Status { get; set; } = SwitchStatus.Active;
    public DateTime? GracePeriodStartedAt { get; set; }
    public DateTime? TriggeredAt { get; set; }

    // Notification Preferences
    public List<int> ReminderDays { get; set; } = new() { 7, 3, 1 };
    public List<NotificationChannel> NotificationChannels { get; set; } = new() { NotificationChannel.Email };

    // Emergency Contacts
    public string? EmergencyEmail { get; set; }
    public string? EmergencyPhone { get; set; }

    // Relationships
    public ICollection<SwitchCheckIn> CheckIns { get; set; } = new List<SwitchCheckIn>();
    public ICollection<SwitchNotification> Notifications { get; set; } = new List<SwitchNotification>();
}
```

### Enums

```csharp
public enum SubscriptionTier
{
    Free = 0,
    Premium = 1,
    Family = 2
}

public enum VaultCategory
{
    Password = 1,
    Document = 2,
    CryptoWallet = 3,
    BankAccount = 4,
    Insurance = 5,
    Property = 6,
    SocialMedia = 7,
    Other = 99
}

public enum SwitchStatus
{
    Active = 1,
    GracePeriod = 2,
    Triggered = 3,
    Paused = 4
}

public enum NotificationChannel
{
    Email = 1,
    SMS = 2,
    Push = 3
}

public enum AccessLevel
{
    Limited = 1,
    Full = 2
}
```

---

## Zero-Knowledge Encryption (C# Implementation)

### Client-Side (Blazor WASM with JS Interop)

#### wwwroot/js/cryptoInterop.js
```javascript
// Web Crypto API wrapper for Blazor
window.cryptoInterop = {
    // Generate encryption key from master password
    async deriveKey(masterPassword, salt, iterations) {
        const encoder = new TextEncoder();
        const passwordBuffer = encoder.encode(masterPassword);

        const keyMaterial = await crypto.subtle.importKey(
            'raw',
            passwordBuffer,
            'PBKDF2',
            false,
            ['deriveBits', 'deriveKey']
        );

        const key = await crypto.subtle.deriveKey(
            {
                name: 'PBKDF2',
                salt: salt,
                iterations: iterations,
                hash: 'SHA-256'
            },
            keyMaterial,
            { name: 'AES-GCM', length: 256 },
            false,
            ['encrypt', 'decrypt']
        );

        return key;
    },

    // Encrypt data
    async encrypt(data, key) {
        const encoder = new TextEncoder();
        const dataBuffer = encoder.encode(data);
        const iv = crypto.getRandomValues(new Uint8Array(12));

        const encryptedBuffer = await crypto.subtle.encrypt(
            { name: 'AES-GCM', iv: iv },
            key,
            dataBuffer
        );

        return {
            encryptedData: Array.from(new Uint8Array(encryptedBuffer)),
            iv: Array.from(iv)
        };
    },

    // Decrypt data
    async decrypt(encryptedData, iv, key) {
        const decryptedBuffer = await crypto.subtle.decrypt(
            { name: 'AES-GCM', iv: new Uint8Array(iv) },
            key,
            new Uint8Array(encryptedData)
        );

        const decoder = new TextDecoder();
        return decoder.decode(decryptedBuffer);
    },

    // Generate random salt
    generateSalt() {
        return Array.from(crypto.getRandomValues(new Uint8Array(16)));
    },

    // Generate RSA key pair for heirs
    async generateRSAKeyPair() {
        const keyPair = await crypto.subtle.generateKey(
            {
                name: 'RSA-OAEP',
                modulusLength: 2048,
                publicExponent: new Uint8Array([1, 0, 1]),
                hash: 'SHA-256'
            },
            true,
            ['encrypt', 'decrypt']
        );

        const publicKey = await crypto.subtle.exportKey('spki', keyPair.publicKey);
        const privateKey = await crypto.subtle.exportKey('pkcs8', keyPair.privateKey);

        return {
            publicKey: Array.from(new Uint8Array(publicKey)),
            privateKey: Array.from(new Uint8Array(privateKey))
        };
    }
};
```

#### Services/ClientEncryptionService.cs (Blazor)
```csharp
using Microsoft.JSInterop;

namespace DigitalVault.BlazorApp.Services;

public class ClientEncryptionService
{
    private readonly IJSRuntime _jsRuntime;
    private object? _cachedKey; // In-memory only, never persisted

    public ClientEncryptionService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<byte[]> GenerateSaltAsync()
    {
        var salt = await _jsRuntime.InvokeAsync<int[]>("cryptoInterop.generateSalt");
        return salt.Select(i => (byte)i).ToArray();
    }

    public async Task<object> DeriveKeyAsync(string masterPassword, byte[] salt, int iterations)
    {
        var key = await _jsRuntime.InvokeAsync<object>(
            "cryptoInterop.deriveKey",
            masterPassword,
            salt,
            iterations
        );

        _cachedKey = key; // Cache in memory
        return key;
    }

    public async Task<(byte[] EncryptedData, byte[] IV)> EncryptAsync(string data)
    {
        if (_cachedKey == null)
            throw new InvalidOperationException("Key not initialized. Call DeriveKeyAsync first.");

        var result = await _jsRuntime.InvokeAsync<EncryptResult>(
            "cryptoInterop.encrypt",
            data,
            _cachedKey
        );

        return (result.EncryptedData, result.IV);
    }

    public async Task<string> DecryptAsync(byte[] encryptedData, byte[] iv)
    {
        if (_cachedKey == null)
            throw new InvalidOperationException("Key not initialized. Call DeriveKeyAsync first.");

        var decrypted = await _jsRuntime.InvokeAsync<string>(
            "cryptoInterop.decrypt",
            encryptedData,
            iv,
            _cachedKey
        );

        return decrypted;
    }

    public void ClearKey()
    {
        _cachedKey = null; // Clear from memory on logout
    }

    private class EncryptResult
    {
        public byte[] EncryptedData { get; set; } = Array.Empty<byte>();
        public byte[] IV { get; set; } = Array.Empty<byte>();
    }
}
```

### Server-Side (C# - for heir key generation)

#### Application/Services/EncryptionService.cs
```csharp
using System.Security.Cryptography;

namespace DigitalVault.Application.Services;

public class EncryptionService : IEncryptionService
{
    // Generate RSA key pair for heirs (alternative to client-side)
    public (byte[] PublicKey, byte[] PrivateKey) GenerateRSAKeyPair()
    {
        using var rsa = RSA.Create(2048);

        var publicKey = rsa.ExportSubjectPublicKeyInfo();
        var privateKey = rsa.ExportPkcs8PrivateKey();

        return (publicKey, privateKey);
    }

    // Encrypt data with RSA public key (for DEK encryption to heirs)
    public byte[] EncryptWithPublicKey(byte[] data, byte[] publicKey)
    {
        using var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(publicKey, out _);

        return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
    }

    // Decrypt data with RSA private key
    public byte[] DecryptWithPrivateKey(byte[] encryptedData, byte[] privateKey)
    {
        using var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(privateKey, out _);

        return rsa.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);
    }

    // Encrypt heir's private key with their password (server-side helper)
    public byte[] EncryptPrivateKeyWithPassword(byte[] privateKey, string password)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateIV();

        // Derive key from password
        var key = DeriveKeyFromPassword(password, aes.IV);

        using var encryptor = aes.CreateEncryptor(key, aes.IV);
        using var ms = new MemoryStream();

        // Write IV first
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(privateKey, 0, privateKey.Length);
        }

        return ms.ToArray();
    }

    private byte[] DeriveKeyFromPassword(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            100000,
            HashAlgorithmName.SHA256
        );

        return pbkdf2.GetBytes(32); // 256 bits
    }

    // Hash password for account authentication (NOT encryption)
    public string HashPassword(string password, out string salt)
    {
        // Use ASP.NET Core Identity PasswordHasher or custom implementation
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        salt = Convert.ToBase64String(saltBytes);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            saltBytes,
            10000, // Lower iterations for account password (not encryption key)
            HashAlgorithmName.SHA256
        );

        var hash = pbkdf2.GetBytes(32);
        return Convert.ToBase64String(hash);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            saltBytes,
            10000,
            HashAlgorithmName.SHA256
        );

        var hashBytes = pbkdf2.GetBytes(32);
        var inputHash = Convert.ToBase64String(hashBytes);

        return inputHash == hash;
    }
}
```

---

## Dead Man's Switch Implementation (C#)

### Infrastructure/BackgroundJobs/DeadManSwitchCheckJob.cs

```csharp
using Hangfire;
using Microsoft.Extensions.Logging;

namespace DigitalVault.Infrastructure.BackgroundJobs;

public class DeadManSwitchCheckJob
{
    private readonly ILogger<DeadManSwitchCheckJob> _logger;
    private readonly IDeadManSwitchService _switchService;
    private readonly INotificationService _notificationService;
    private readonly IHeirAccessService _heirAccessService;

    public DeadManSwitchCheckJob(
        ILogger<DeadManSwitchCheckJob> logger,
        IDeadManSwitchService switchService,
        INotificationService notificationService,
        IHeirAccessService heirAccessService)
    {
        _logger = logger;
        _switchService = switchService;
        _notificationService = notificationService;
        _heirAccessService = heirAccessService;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting Dead Man's Switch check job at {Time}", DateTime.UtcNow);

        var switches = await _switchService.GetAllActiveSwitchesAsync();

        foreach (var switchEntity in switches)
        {
            try
            {
                await ProcessSwitchAsync(switchEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing switch {SwitchId}", switchEntity.Id);
            }
        }

        _logger.LogInformation("Completed Dead Man's Switch check job");
    }

    private async Task ProcessSwitchAsync(DeadManSwitch switchEntity)
    {
        var daysSinceLastCheckIn = (DateTime.UtcNow - switchEntity.LastCheckInAt).TotalDays;

        // 1. Send reminders before due date
        if (IsReminderDue(switchEntity, daysSinceLastCheckIn))
        {
            await SendReminderNotificationAsync(switchEntity, daysSinceLastCheckIn);
        }

        // 2. Enter grace period
        else if (daysSinceLastCheckIn >= switchEntity.CheckInIntervalDays &&
                 switchEntity.Status == SwitchStatus.Active)
        {
            await EnterGracePeriodAsync(switchEntity);
        }

        // 3. Trigger switch (release to heirs)
        else if (switchEntity.Status == SwitchStatus.GracePeriod &&
                 switchEntity.GracePeriodStartedAt.HasValue &&
                 (DateTime.UtcNow - switchEntity.GracePeriodStartedAt.Value).TotalDays >= switchEntity.GracePeriodDays)
        {
            await TriggerSwitchAsync(switchEntity);
        }
    }

    private bool IsReminderDue(DeadManSwitch switchEntity, double daysSinceLastCheckIn)
    {
        var daysUntilDue = switchEntity.CheckInIntervalDays - daysSinceLastCheckIn;

        foreach (var reminderDay in switchEntity.ReminderDays.OrderByDescending(d => d))
        {
            if (daysUntilDue <= reminderDay && daysUntilDue > reminderDay - 1)
            {
                // Check if we already sent notification today
                var lastNotification = switchEntity.Notifications
                    .Where(n => n.NotificationType == "reminder" && n.SentAt.Date == DateTime.UtcNow.Date)
                    .FirstOrDefault();

                return lastNotification == null;
            }
        }

        return false;
    }

    private async Task SendReminderNotificationAsync(DeadManSwitch switchEntity, double daysSinceLastCheckIn)
    {
        var daysUntilDue = Math.Ceiling(switchEntity.CheckInIntervalDays - daysSinceLastCheckIn);

        _logger.LogInformation("Sending reminder for switch {SwitchId}, {Days} days until due",
            switchEntity.Id, daysUntilDue);

        await _notificationService.SendReminderAsync(
            switchEntity.UserId,
            (int)daysUntilDue,
            switchEntity.NotificationChannels
        );
    }

    private async Task EnterGracePeriodAsync(DeadManSwitch switchEntity)
    {
        _logger.LogWarning("Switch {SwitchId} entering grace period", switchEntity.Id);

        switchEntity.Status = SwitchStatus.GracePeriod;
        switchEntity.GracePeriodStartedAt = DateTime.UtcNow;
        await _switchService.UpdateAsync(switchEntity);

        await _notificationService.SendGracePeriodNotificationAsync(
            switchEntity.UserId,
            switchEntity.GracePeriodDays,
            switchEntity.NotificationChannels
        );
    }

    private async Task TriggerSwitchAsync(DeadManSwitch switchEntity)
    {
        _logger.LogCritical("TRIGGERING switch {SwitchId} for user {UserId}",
            switchEntity.Id, switchEntity.UserId);

        switchEntity.Status = SwitchStatus.Triggered;
        switchEntity.TriggeredAt = DateTime.UtcNow;
        await _switchService.UpdateAsync(switchEntity);

        // Grant access to all heirs
        await _heirAccessService.GrantAccessToAllHeirsAsync(switchEntity.UserId);

        // Notify heirs
        var heirs = await _heirAccessService.GetHeirsAsync(switchEntity.UserId);
        foreach (var heir in heirs)
        {
            await _notificationService.SendHeirAccessNotificationAsync(heir);
        }

        // Notify emergency contacts (if any)
        if (!string.IsNullOrEmpty(switchEntity.EmergencyEmail))
        {
            await _notificationService.SendEmergencyContactNotificationAsync(
                switchEntity.EmergencyEmail,
                switchEntity.UserId
            );
        }
    }
}
```

### Configure Hangfire in Program.cs

```csharp
// Add Hangfire
builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHangfireServer();

// After app.Build()
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Schedule recurring jobs
RecurringJob.AddOrUpdate<DeadManSwitchCheckJob>(
    "dead-man-switch-check",
    job => job.ExecuteAsync(),
    Cron.Daily(0) // Run every day at midnight UTC
);
```

---

## API Controllers (C#)

### Controllers/VaultController.cs

```csharp
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalVault.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VaultController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<VaultController> _logger;

    public VaultController(IMediator mediator, ILogger<VaultController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("entries")]
    public async Task<ActionResult<List<VaultEntryDto>>> GetEntries(
        [FromQuery] VaultCategory? category = null)
    {
        var query = new GetVaultEntriesQuery
        {
            UserId = GetCurrentUserId(),
            Category = category
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("entries/{id}")]
    public async Task<ActionResult<VaultEntryDto>> GetEntry(Guid id)
    {
        var query = new GetVaultEntryQuery
        {
            Id = id,
            UserId = GetCurrentUserId()
        };

        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost("entries")]
    public async Task<ActionResult<VaultEntryDto>> CreateEntry(
        [FromBody] CreateVaultEntryCommand command)
    {
        command.UserId = GetCurrentUserId();

        var result = await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetEntry),
            new { id = result.Id },
            result
        );
    }

    [HttpPut("entries/{id}")]
    public async Task<ActionResult<VaultEntryDto>> UpdateEntry(
        Guid id,
        [FromBody] UpdateVaultEntryCommand command)
    {
        command.Id = id;
        command.UserId = GetCurrentUserId();

        var result = await _mediator.Send(command);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpDelete("entries/{id}")]
    public async Task<ActionResult> DeleteEntry(Guid id)
    {
        var command = new DeleteVaultEntryCommand
        {
            Id = id,
            UserId = GetCurrentUserId()
        };

        await _mediator.Send(command);

        return NoContent();
    }

    [HttpPost("entries/{id}/upload")]
    public async Task<ActionResult<string>> UploadFile(
        Guid id,
        [FromForm] IFormFile file)
    {
        var command = new UploadVaultFileCommand
        {
            VaultEntryId = id,
            UserId = GetCurrentUserId(),
            File = file
        };

        var blobUrl = await _mediator.Send(command);

        return Ok(new { blobUrl });
    }

    [HttpGet("entries/{id}/download")]
    public async Task<ActionResult> DownloadFile(Guid id)
    {
        var query = new DownloadVaultFileQuery
        {
            VaultEntryId = id,
            UserId = GetCurrentUserId()
        };

        var (stream, contentType, fileName) = await _mediator.Send(query);

        return File(stream, contentType, fileName);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("Invalid user token");

        return userId;
    }
}
```

### Controllers/SwitchController.cs

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SwitchController : ControllerBase
{
    private readonly IMediator _mediator;

    public SwitchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<DeadManSwitchDto>> GetSwitch()
    {
        var query = new GetSwitchQuery { UserId = GetCurrentUserId() };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DeadManSwitchDto>> CreateOrUpdateSwitch(
        [FromBody] CreateOrUpdateSwitchCommand command)
    {
        command.UserId = GetCurrentUserId();
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpPost("check-in")]
    public async Task<ActionResult> PerformCheckIn()
    {
        var command = new PerformCheckInCommand
        {
            UserId = GetCurrentUserId(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers["User-Agent"].ToString()
        };

        await _mediator.Send(command);

        return Ok(new { message = "Check-in successful", nextDueDate = DateTime.UtcNow.AddDays(90) });
    }

    [HttpGet("status")]
    public async Task<ActionResult<SwitchStatusDto>> GetStatus()
    {
        var query = new GetSwitchStatusQuery { UserId = GetCurrentUserId() };
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    [HttpPost("pause")]
    public async Task<ActionResult> PauseSwitch()
    {
        var command = new PauseSwitchCommand { UserId = GetCurrentUserId() };
        await _mediator.Send(command);

        return Ok(new { message = "Switch paused" });
    }

    [HttpPost("resume")]
    public async Task<ActionResult> ResumeSwitch()
    {
        var command = new ResumeSwitchCommand { UserId = GetCurrentUserId() };
        await _mediator.Send(command);

        return Ok(new { message = "Switch resumed" });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId");
        return Guid.Parse(userIdClaim!.Value);
    }
}
```

---

## Database Configuration (Entity Framework Core)

### ApplicationDbContext.cs

```csharp
using Microsoft.EntityFrameworkCore;

namespace DigitalVault.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<VaultEntry> VaultEntries => Set<VaultEntry>();
    public DbSet<Heir> Heirs => Set<Heir>();
    public DbSet<HeirVaultAccess> HeirVaultAccesses => Set<HeirVaultAccess>();
    public DbSet<DeadManSwitch> DeadManSwitches => Set<DeadManSwitch>();
    public DbSet<SwitchCheckIn> SwitchCheckIns => Set<SwitchCheckIn>();
    public DbSet<SwitchNotification> SwitchNotifications => Set<SwitchNotifications>();
    public DbSet<HeirAccessLog> HeirAccessLogs => Set<HeirAccessLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

### Configurations/UserConfiguration.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalVault.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Salt)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.KeyDerivationSalt)
            .IsRequired();

        builder.Property(u => u.SubscriptionTier)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Relationships
        builder.HasMany(u => u.VaultEntries)
            .WithOne(v => v.User)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Heirs)
            .WithOne(h => h.User)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.DeadManSwitch)
            .WithOne(s => s.User)
            .HasForeignKey<DeadManSwitch>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(u => u.CreatedAt);
        builder.HasIndex(u => u.IsDeleted);
    }
}
```

---

## Blazor Pages Example

### Pages/Vault/VaultList.razor

```razor
@page "/vault"
@using DigitalVault.Shared.DTOs
@inject ApiClient ApiClient
@inject NavigationManager Navigation
@inject ClientEncryptionService Encryption

<PageTitle>My Vault</PageTitle>

<MudContainer MaxWidth="MaxWidth.Large" Class="mt-4">
    <MudText Typo="Typo.h4" GutterBottom="true">My Digital Vault</MudText>

    @if (_isLoading)
    {
        <MudProgressLinear Indeterminate="true" />
    }
    else
    {
        <MudGrid>
            <MudItem xs="12" md="3">
                <MudSelect T="VaultCategory?" @bind-Value="_selectedCategory" Label="Category">
                    <MudSelectItem Value="@((VaultCategory?)null)">All Categories</MudSelectItem>
                    @foreach (var category in Enum.GetValues<VaultCategory>())
                    {
                        <MudSelectItem Value="@category">@category.ToString()</MudSelectItem>
                    }
                </MudSelect>
            </MudItem>
            <MudItem xs="12" md="9" Class="d-flex justify-end">
                <MudButton Variant="Variant.Filled"
                          Color="Color.Primary"
                          StartIcon="@Icons.Material.Filled.Add"
                          OnClick="NavigateToCreate">
                    Add Entry
                </MudButton>
            </MudItem>
        </MudGrid>

        @if (_entries.Count == 0)
        {
            <MudPaper Class="pa-4 mt-4" Elevation="0">
                <MudText Typo="Typo.body1" Align="Align.Center">
                    No vault entries yet. Create your first one!
                </MudText>
            </MudPaper>
        }
        else
        {
            <MudGrid Class="mt-4">
                @foreach (var entry in _entries)
                {
                    <MudItem xs="12" md="6" lg="4">
                        <VaultEntryCard Entry="entry"
                                       OnView="HandleView"
                                       OnEdit="HandleEdit"
                                       OnDelete="HandleDelete" />
                    </MudItem>
                }
            </MudGrid>
        }
    }
</MudContainer>

@code {
    private List<VaultEntryDto> _entries = new();
    private VaultCategory? _selectedCategory;
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadEntriesAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_selectedCategory.HasValue)
        {
            await LoadEntriesAsync();
        }
    }

    private async Task LoadEntriesAsync()
    {
        _isLoading = true;

        try
        {
            _entries = await ApiClient.GetVaultEntriesAsync(_selectedCategory);
        }
        catch (Exception ex)
        {
            // Handle error
            Console.WriteLine($"Error loading entries: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void NavigateToCreate()
    {
        Navigation.NavigateTo("/vault/create");
    }

    private void HandleView(Guid id)
    {
        Navigation.NavigateTo($"/vault/{id}");
    }

    private void HandleEdit(Guid id)
    {
        Navigation.NavigateTo($"/vault/{id}/edit");
    }

    private async Task HandleDelete(Guid id)
    {
        // Show confirmation dialog
        // await ApiClient.DeleteVaultEntryAsync(id);
        // await LoadEntriesAsync();
    }
}
```

---

## Development Roadmap

### Phase 1: Foundation (Weeks 1-8)

**Week 1-2: Project Setup**
- [ ] Create .NET 8 solution with all projects
- [ ] Setup Entity Framework Core
- [ ] Create database migrations
- [ ] Configure Docker Compose for local development
- [ ] Setup CI/CD with GitHub Actions

**Week 3-4: Core Features**
- [ ] Implement authentication (JWT)
- [ ] Implement VaultEntry CRUD
- [ ] Integrate Azure Blob Storage
- [ ] Build Blazor authentication pages

**Week 5-6: Encryption**
- [ ] Implement client-side encryption (JS Interop)
- [ ] Build C# encryption services
- [ ] Create key management system
- [ ] Implement zero-knowledge architecture

**Week 7-8: Dead Man's Switch MVP**
- [ ] Implement DeadManSwitch entity
- [ ] Build Hangfire background jobs
- [ ] Create check-in endpoints
- [ ] Integrate email service (SendGrid)

### Phase 2: Heir Management (Weeks 9-12)

- [ ] Implement Heir CRUD
- [ ] Build heir verification flow
- [ ] Create RSA key pair generation
- [ ] Implement HeirVaultAccess logic
- [ ] Build heir portal pages (Blazor)

### Phase 3: Polish & Launch (Weeks 13-16)

- [ ] Create landing page (marketing site)
- [ ] Write Terms of Service & Privacy Policy
- [ ] Implement subscription tiers
- [ ] Integrate Stripe payments
- [ ] Security audit & penetration testing
- [ ] Deploy to production

---

## Next Steps

1. Initialize Git repository: `git init`
2. Create .NET solution: `dotnet new sln -n DigitalVault`
3. Create projects (see solution structure above)
4. Review and approve this architecture document
5. Begin Phase 1 implementation

คุณพร้อมที่จะเริ่มสร้างโปรเจค C# นี้แล้วหรือยังครับ? ผมสามารถช่วยสร้าง solution structure และเริ่ม implement ได้เลย
