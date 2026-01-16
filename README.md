# Digital Vault - Dead Man's Switch

ระบบจัดเก็บข้อมูลลับแบบ Zero-Knowledge พร้อมฟีเจอร์ Dead Man's Switch ที่จะส่งต่อข้อมูลให้กับผู้รับมรดกโดยอัตโนมัติ

## Tech Stack

- **.NET 8** - Backend API
- **Blazor WebAssembly** - Frontend
- **Entity Framework Core 8** - ORM
- **PostgreSQL/SQL Server** - Database
- **Azure Blob Storage / AWS S3** - File Storage
- **Redis** - Caching
- **Hangfire** - Background Jobs

## Project Structure

```
DigitalVault/
├── src/
│   ├── DigitalVault.Domain/         # Domain models & interfaces
│   ├── DigitalVault.Application/    # Business logic (CQRS)
│   ├── DigitalVault.Infrastructure/ # Data access & services
│   ├── DigitalVault.API/            # Web API
│   ├── DigitalVault.BlazorApp/      # Blazor WebAssembly
│   └── DigitalVault.Shared/         # Shared DTOs
├── tests/
│   ├── DigitalVault.UnitTests/
│   └── DigitalVault.IntegrationTests/
├── docker/
│   └── docker-compose.yml
└── docs/
    └── ARCHITECTURE.md
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker Desktop
- PostgreSQL (หรือ SQL Server)
- Redis

### Setup

1. **Clone repository**
```bash
git clone <repository-url>
cd syDigitalVault
```

2. **Start infrastructure (Docker)**
```bash
cd docker
docker-compose up -d
```

3. **Update connection strings**

Edit `src/DigitalVault.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=digitalvault;Username=postgres;Password=yourpassword"
  }
}
```

4. **Run migrations**
```bash
cd src/DigitalVault.API
dotnet ef database update
```

5. **Run API**
```bash
dotnet run --project src/DigitalVault.API
```

6. **Run Blazor App (in another terminal)**
```bash
dotnet run --project src/DigitalVault.BlazorApp
```

7. **Access**
- API: https://localhost:5001
- Swagger: https://localhost:5001/swagger
- Blazor App: https://localhost:5002

## Features

### Phase 1 (MVP)
- [x] User authentication & authorization
- [x] Client-side encryption (Zero-Knowledge)
- [x] Vault entry CRUD
- [x] File upload/download
- [x] Dead Man's Switch configuration
- [x] Check-in system
- [x] Email notifications

### Phase 2
- [ ] Heir management
- [ ] Heir verification
- [ ] RSA key pair for heirs
- [ ] Heir access portal
- [ ] Multi-factor authentication

### Phase 3
- [ ] Subscription tiers
- [ ] Stripe payment integration
- [ ] Landing page
- [ ] Terms of Service / Privacy Policy
- [ ] Security audit

### Phase 4
- [ ] Mobile app (Blazor Hybrid / MAUI)
- [ ] Browser extension
- [ ] B2B partnerships
- [ ] Advanced analytics

## Security

### Zero-Knowledge Architecture

- **Master Password**: ไม่ถูกส่งไปยัง Server
- **Client-Side Encryption**: ใช้ AES-256-GCM
- **Key Derivation**: PBKDF2 (100,000 iterations)
- **Data at Rest**: เข้ารหัสอีกชั้นใน Azure Blob Storage
- **Transport**: HTTPS (TLS 1.3)

### Audit Logging

- ทุกการเข้าถึง Vault
- ทุกการ Check-in
- ทุกการเปลี่ยนแปลงการตั้งค่า
- ทุกการเข้าถึงของ Heir

## Development

### Build

```bash
dotnet build
```

### Test

```bash
dotnet test
```

### Code Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Database Migrations

```bash
# Add migration
dotnet ef migrations add MigrationName -p src/DigitalVault.Infrastructure -s src/DigitalVault.API

# Update database
dotnet ef database update -p src/DigitalVault.Infrastructure -s src/DigitalVault.API
```

## Deployment

### Docker

```bash
docker build -t digitalvault-api -f src/DigitalVault.API/Dockerfile .
docker run -p 5000:80 digitalvault-api
```

### Azure

```bash
az webapp up --name digitalvault-api --resource-group digitalvault-rg
```

## License

Proprietary - All rights reserved

## Contact

สำหรับคำถามหรือข้อเสนอแนะ:
- Email: support@digitalvault.app
- Website: https://digitalvault.app

---

**IMPORTANT:** ระบบนี้จัดเก็บข้อมูลที่มีความสำคัญสูง กรุณาใช้ Master Password ที่แข็งแรงและไม่ควรลืม เพราะเราไม่สามารถกู้คืนได้ (Zero-Knowledge)
