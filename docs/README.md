# DigitalVault Documentation

เอกสารประกอบสำหรับโปรเจค DigitalVault - ระบบจัดเก็บเอกสารดิจิทัลแบบ Zero-Knowledge Encryption

## 📚 เอกสารที่มี

### 1. [Backend Architecture](./backend_architecture.md)
สถาปัตยกรรมหลังบ้านแบบครบถ้วน ประกอบด้วย:
- **Database Schema** (10 ตาราง)
  - Users, Accounts, FamilyMembers
  - Documents, DocumentMetadata
  - FileAttachments, Notes
  - AccountCollaborators, UserKeyPairs
  - AuditLogs
- **Zero-Knowledge Encryption Workflows**
  - Registration Flow
  - Login Flow
  - Document Upload/Download
  - Collaborator Invitation
- **API Endpoints** ทั้งหมด
- **AWS S3 Configuration**
- **Security Best Practices**
- **Code Examples** (C# + JavaScript)

### 2. [Collaborator Use Cases](./collaborator_use_cases.md)
ตัวอย่างการใช้งานระบบ Collaborator แบบละเอียด:
- **สถานการณ์จริง**: ครอบครัวคุณสมชาย
- **3 Permission Levels**: Owner, Admin, Editor, Viewer
- **Use Cases**:
  - เชิญภรรยาเป็น Admin
  - เชิญลูกสาวเป็น Editor
  - เชิญลูกชายเป็น Viewer
- **ตารางเปรียบเทียบสิทธิ์**
- **UI Mockups**
- **Audit Log ตัวอย่าง**

## 🏗️ สถาปัตยกรรมระบบ

```
┌─────────────────────────────────────────────────────┐
│                  Blazor WebAssembly                 │
│              (Zero-Knowledge Client)                │
│  ┌──────────────────────────────────────────────┐   │
│  │ • Client-side Encryption (AES-256-GCM)       │   │
│  │ • Master Key Management (Memory Only)        │   │
│  │ • RSA Key Pair Generation                    │   │
│  └──────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
                         ↓ HTTPS
┌─────────────────────────────────────────────────────┐
│              ASP.NET Core 8.0 API                   │
│  ┌──────────────────────────────────────────────┐   │
│  │ • JWT Authentication                         │   │
│  │ • Pre-signed URL Generation                  │   │
│  │ • Encrypted Metadata Storage                 │   │
│  │ • Collaborator Management                    │   │
│  └──────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
         ↓                                    ↓
┌──────────────────┐              ┌──────────────────┐
│  SQL Server /    │              │     AWS S3       │
│  PostgreSQL      │              │  (Encrypted      │
│  (Encrypted      │              │   Documents)     │
│   Metadata)      │              │                  │
└──────────────────┘              └──────────────────┘
```

## 🔐 Zero-Knowledge Encryption

### หลักการสำคัญ
- ✅ ข้อมูลทั้งหมดเข้ารหัสที่ Client ก่อนส่ง Server
- ✅ Master Key ไม่เคยส่งไปที่ Server
- ✅ Server ไม่สามารถอ่านข้อมูลได้
- ✅ ใช้ AES-256-GCM สำหรับเข้ารหัสข้อมูล
- ✅ ใช้ RSA-4096 สำหรับแชร์ Master Key
- ✅ ใช้ Argon2id สำหรับ Password Hashing

### Encryption Flow
```
User Password
    ↓
Argon2id (Key Derivation)
    ↓
Derived Key → Encrypt Master Key → Store in DB
    ↓
Master Key (in memory)
    ↓
AES-256-GCM → Encrypt Data → Upload to S3
```

## 👥 Multi-Account & Collaboration

### Account Structure
- 1 User สามารถมีได้หลาย Accounts (Vaults)
- แต่ละ Account มี Master Key แยกกัน
- สามารถแชร์ Account ให้คนอื่นได้

### Permission Levels
| Level | View | Edit | Delete | Invite | Manage |
|-------|------|------|--------|--------|--------|
| Owner | ✅ | ✅ | ✅ | ✅ | ✅ |
| Admin | ✅ | ✅ | ✅ | ✅ | ✅ |
| Editor | ✅ | ✅ | ✅ | ❌ | ❌ |
| Viewer | ✅ | ❌ | ❌ | ❌ | ❌ |

## 🚀 Technology Stack

### Frontend
- **Framework**: Blazor WebAssembly (.NET 8)
- **Crypto**: SubtleCrypto API (Web Crypto)
- **UI**: Tailwind CSS
- **State**: In-memory (secure)

### Backend
- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server / PostgreSQL
- **ORM**: Entity Framework Core
- **Auth**: JWT Bearer Tokens
- **Storage**: AWS S3
- **SDK**: AWSSDK.S3

### Security
- **Encryption**: AES-256-GCM
- **Key Exchange**: RSA-4096
- **Password Hash**: Argon2id
- **Key Derivation**: PBKDF2 / Argon2id

## 📋 Implementation Phases

### Phase 1: Core Authentication ✅
- [x] User registration with Zero-Knowledge encryption
- [x] Login/Logout with Master Key derivation
- [x] JWT token management
- [x] Multi-account support

### Phase 2: Family Members 🔄
- [ ] CRUD operations for family members
- [ ] Client-side encryption/decryption
- [ ] Thai document types support

### Phase 3: AWS S3 Integration 📝
- [ ] Pre-signed URL generation
- [ ] Document upload/download workflow
- [ ] S3 bucket configuration
- [ ] Thumbnail generation

### Phase 4: Collaboration 📝
- [ ] RSA key pair generation
- [ ] Collaborator invitation system
- [ ] Permission management
- [ ] Audit logging

### Phase 5: Additional Features 📝
- [ ] Notes system
- [ ] File attachments
- [ ] Search (client-side)
- [ ] Export/Import

## 🔗 Quick Links

- [Backend Architecture Details](./backend_architecture.md)
- [Collaborator Use Cases](./collaborator_use_cases.md)

## 📝 Notes

- เอกสารนี้จะถูกอัพเดทตามการพัฒนาโปรเจค
- สำหรับคำถามหรือข้อเสนอแนะ กรุณาติดต่อทีมพัฒนา

---

**Last Updated**: 2026-01-18
**Version**: 1.0.0
