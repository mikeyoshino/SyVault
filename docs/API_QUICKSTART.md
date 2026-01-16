# Digital Vault API - Quick Start Guide

## 🚀 Start the Application

### 1. Start Infrastructure (Docker)

```bash
cd docker
docker-compose up -d
```

This will start:
- **PostgreSQL** (port 5432)
- **Redis** (port 6379)
- **Azurite** (Blob Storage Emulator) (port 10000)
- **Mailpit** (Email Testing) (port 8025)
- **Adminer** (Database UI) (port 8082) - **Note: Changed from 8080**
- **Redis Insight** (Redis UI) (port 5540)

### 2. Run the API

```bash
dotnet run --project src/DigitalVault.API
```

The API will start on:
- **HTTPS**: https://localhost:5001
- **HTTP**: http://localhost:5000

### 3. Access Swagger UI

Open your browser and navigate to:
```
https://localhost:5001/swagger
```

---

## 🧪 Testing the API

### Authentication Flow

#### 1. Register a New User

**POST** `/api/auth/register`

```json
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "phoneNumber": "+66812345678"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "random_base64_string",
    "expiresAt": "2024-01-16T10:00:00Z",
    "user": {
      "id": "guid",
      "email": "user@example.com",
      "emailVerified": false,
      "phoneNumber": "+66812345678",
      "mfaEnabled": false,
      "subscriptionTier": "Free",
      "subscriptionExpiresAt": null,
      "keyDerivationSalt": "base64_encoded_salt",
      "keyDerivationIterations": 100000
    }
  },
  "message": "Registration successful"
}
```

#### 2. Login

**POST** `/api/auth/login`

```json
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "mfaCode": null
}
```

**Response:** Same as registration

#### 3. Use the Access Token

Copy the `accessToken` from the response and click the **Authorize** button in Swagger UI:

```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

### Vault Operations

#### 1. Create a Vault Entry

**POST** `/api/vault` (Requires Authentication)

```json
{
  "title": "My Bank Account",
  "category": "BankAccount",
  "encryptedDataKey": "base64_encoded_encrypted_dek",
  "encryptedContent": "base64_encoded_encrypted_content",
  "iv": "base64_encoded_iv",
  "isSharedWithHeirs": true
}
```

**Notes:**
- `encryptedDataKey`: The DEK (Data Encryption Key) encrypted with the user's master key
- `encryptedContent`: The actual sensitive data encrypted with the DEK
- `iv`: Initialization vector for AES-GCM encryption
- For testing, you can use dummy base64 strings like: `SGVsbG8gV29ybGQh` (Hello World!)

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "title": "My Bank Account",
    "category": "BankAccount",
    "encryptedDataKey": "...",
    "encryptedContent": "...",
    "blobStorageUrl": null,
    "iv": "...",
    "encryptionAlgorithm": "AES-256-GCM",
    "isSharedWithHeirs": true,
    "createdAt": "2024-01-16T00:00:00Z",
    "updatedAt": "2024-01-16T00:00:00Z"
  },
  "message": "Vault entry created successfully"
}
```

#### 2. Get All Vault Entries

**GET** `/api/vault` (Requires Authentication)

Optional query parameter:
- `category`: Filter by category (e.g., `?category=Password`)

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "title": "My Bank Account",
      "category": "BankAccount",
      ...
    }
  ],
  "message": "Retrieved 1 vault entries"
}
```

#### 3. Get Single Vault Entry

**GET** `/api/vault/{id}` (Requires Authentication)

#### 4. Delete Vault Entry

**DELETE** `/api/vault/{id}` (Requires Authentication)

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid"
  },
  "message": "Vault entry deleted successfully"
}
```

#### 5. Get Vault Statistics

**GET** `/api/vault/statistics` (Requires Authentication)

**Response:**
```json
{
  "success": true,
  "data": {
    "totalEntries": 3,
    "categoryCounts": [
      { "category": "Password", "count": 2 },
      { "category": "BankAccount", "count": 1 }
    ],
    "sharedWithHeirs": 2,
    "lastCreated": "2024-01-16T00:00:00Z"
  },
  "message": "Statistics retrieved successfully"
}
```

---

## 📊 Subscription Tier Limits

### Free Tier
- **Max Vault Entries**: 3
- **Max Heirs**: 1
- **Storage**: 10 MB
- **Check-in Interval**: 90 days (fixed)

When you try to create a 4th entry, you'll get:
```json
{
  "success": false,
  "message": "Free tier limit reached. Upgrade to Premium for unlimited entries."
}
```

---

## 🔐 Password Validation Rules

Passwords must:
- Be at least 8 characters long
- Contain at least one uppercase letter (A-Z)
- Contain at least one lowercase letter (a-z)
- Contain at least one digit (0-9)
- Contain at least one special character (!@#$%^&*)

Example valid password: `SecurePass123!`

---

## 🗄️ Database Access

### Adminer (Database Management)
```
http://localhost:8082

System: PostgreSQL
Server: postgres
Username: postgres
Password: Dev@Password123
Database: digitalvault
```

### Redis Insight (Redis Management)
```
http://localhost:5540

When first opening, add connection:
Host: redis
Port: 6379
Password: Dev@Redis123
```

---

## 📧 Email Testing (Mailpit)

All emails are caught by Mailpit during development:
```
http://localhost:8025
```

Mailpit is a modern replacement for MailHog with better UI and ARM64 support.

You can view:
- Registration emails (when implemented)
- Password reset emails (when implemented)
- Dead Man's Switch notifications (when implemented)

---

## 🐛 Troubleshooting

### Database Connection Error

If you see:
```
Error connecting to database
```

**Solution:**
```bash
# Check if PostgreSQL is running
docker ps

# Restart containers
cd docker
docker-compose down
docker-compose up -d

# Wait 10 seconds for PostgreSQL to be ready
sleep 10

# Run API again
dotnet run --project src/DigitalVault.API
```

### Migration Error

If database schema is out of date:
```bash
dotnet ef database update -p src/DigitalVault.Infrastructure -s src/DigitalVault.API
```

### Port Already in Use

If port 5001 is already in use, change in `launchSettings.json`:
```json
"applicationUrl": "https://localhost:5011;http://localhost:5010"
```

---

## 🔍 Health Check

**GET** `/health`

```json
{
  "status": "healthy",
  "timestamp": "2024-01-16T00:00:00Z",
  "version": "1.0.0"
}
```

---

## 📝 Available Vault Categories

- `Password`
- `Document`
- `CryptoWallet`
- `BankAccount`
- `Insurance`
- `Property`
- `SocialMedia`
- `Other`

---

## 🎯 Next Steps

1. **Test Client-Side Encryption**: Implement Web Crypto API in Blazor
2. **Add Heir Management**: Create heirs and share vault entries
3. **Setup Dead Man's Switch**: Configure automatic check-in reminders
4. **Deploy to Cloud**: Deploy to Azure or AWS

---

## 🚨 Security Notes

⚠️ **Development Environment Only**
- The JWT secret key in `appsettings.json` is for development only
- Change it to a strong secret in production: `openssl rand -base64 64`
- Never commit production secrets to git
- Use Azure Key Vault or AWS Secrets Manager in production

⚠️ **Zero-Knowledge Architecture**
- The master password should NEVER be sent to the server
- All encryption/decryption happens on the client side
- The server only stores encrypted blobs

---

For more information, see:
- [Architecture Documentation](../ARCHITECTURE.md)
- [API Documentation](./API_DOCUMENTATION.md) (to be created)
- [Security Whitepaper](./SECURITY_WHITEPAPER.md) (to be created)
