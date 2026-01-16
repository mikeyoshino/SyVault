# Frontend Security Implementation

## Overview

The Blazor WebAssembly frontend implements **true zero-knowledge encryption** with multiple layers of security to ensure user data remains private.

## Security Architecture

### 1. Token Storage Strategy

**sessionStorage vs localStorage**

We use both storage mechanisms strategically:

```csharp
// Access Token - Can be remembered
if (rememberMe)
    localStorage.setItem("accessToken", token); // Persists across sessions
else
    sessionStorage.setItem("accessToken", token); // Cleared on browser close

// Master Key - ALWAYS session only
sessionStorage.setItem("masterKey", key); // NEVER in localStorage!
```

**Why this approach?**
- **sessionStorage**: Cleared when tab/browser closes - perfect for sensitive master keys
- **localStorage**: Persists across browser restarts - acceptable for JWT tokens with expiry
- **NEVER store master key in localStorage** - would persist to disk

### 2. Client-Side Encryption (AES-256-GCM)

All cryptographic operations happen in the browser using Web Crypto API:

**Algorithm**: AES-256-GCM
- **256-bit key**: Maximum security
- **GCM mode**: Authenticated encryption (prevents tampering)
- **Random IV**: 12-byte unique IV for each encryption
- **PBKDF2**: 100,000 iterations for key derivation

**Implementation Details:**
```javascript
// Master key generation
const masterKey = crypto.getRandomValues(new Uint8Array(32)); // 256 bits

// Key derivation from password
crypto.subtle.deriveKey({
    name: 'PBKDF2',
    salt: salt, // 16 random bytes
    iterations: 100000, // OWASP recommendation
    hash: 'SHA-256'
}, ...)

// Encryption with AES-256-GCM
crypto.subtle.encrypt({
    name: 'AES-GCM',
    iv: crypto.getRandomValues(new Uint8Array(12)) // Random IV
}, key, data)
```

### 3. Zero-Knowledge Guarantee

**What the server knows:**
- ✅ Email address
- ✅ Password hash (bcrypt/argon2)
- ✅ Encrypted master key
- ✅ Key derivation salt & iterations
- ✅ Encrypted vault data

**What the server NEVER knows:**
- ❌ Master key (plaintext)
- ❌ Vault data (plaintext)
- ❌ User's password (only hash)

**Security flow:**
1. User enters password
2. Client derives key from password (PBKDF2)
3. Client encrypts master key with derived key
4. Encrypted master key sent to server
5. Server stores encrypted version
6. On login, server returns encrypted master key
7. Client decrypts locally with password

## Security Measures Implemented

### 1. CSRF Protection
- All state-changing operations require authentication
- JWT tokens include user-specific claims
- No cookies used (token-based auth)

### 2. XSS Protection
- Blazor automatically escapes HTML
- No `dangerouslySetInnerHTML` equivalent used
- All user input sanitized
- Content Security Policy headers (recommend adding)

### 3. Secure Communication
- **Production**: HTTPS only (add `<RequireHttpsAttribute>`)
- **Development**: HTTP acceptable for localhost
- TLS 1.2+ minimum (configure in web server)

### 4. Password Security

**Client-side validation:**
```csharp
[Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
[MinLength(8, ErrorMessage = "รหัสผ่านต้องมีอย่างน้อย 8 ตัวอักษร")]
```

**Server-side validation** (already implemented):
- Minimum 8 characters
- Requires uppercase, lowercase, number, special character
- FluentValidation enforces rules

### 5. Session Management

**Token Expiry:**
- Access tokens expire after 60 minutes
- Master key cleared on browser close
- Automatic logout on token expiry

**Secure Logout:**
```csharp
public async Task LogoutAsync()
{
    await _secureStorage.ClearAuthDataAsync(); // Clears all auth data
    // Redirects to login page
}
```

### 6. Memory Security

**Master Key Handling:**
- Stored only in sessionStorage (memory)
- Never written to disk
- Cleared on:
  - Browser close
  - Tab close
  - Explicit logout
  - Token expiry

**Sensitive Data:**
- Vault data decrypted only when viewed
- Plaintext never persisted
- Cleared from DOM after use

## Attack Vector Mitigation

### 1. Brute Force Protection

**Server-side** (recommend implementing):
- Rate limiting on login endpoint
- Account lockout after N failed attempts
- CAPTCHA after 3 failed attempts
- IP-based throttling

**Client-side:**
- Strong password requirements
- Password strength indicator (todo)

### 2. Man-in-the-Middle (MITM)

**Protection:**
- HTTPS in production (SSL/TLS)
- Certificate pinning (optional, for mobile)
- HSTS headers (recommend)

### 3. Phishing

**Protection:**
- Display domain in UI
- Security indicators (lock icon)
- Email verification for account changes
- MFA support (already implemented)

### 4. Database Breach

**Impact minimized by:**
- Master keys encrypted (server can't decrypt)
- Vault data encrypted (server can't decrypt)
- Passwords hashed (bcrypt/argon2)
- Even with full database access, attacker needs user passwords

### 5. Browser-based Attacks

**sessionStorage XSS risk:**
- Blazor escapes all output
- No eval() or innerHTML used
- Content Security Policy (recommend)

**Protection measures:**
```html
<!-- Add to index.html -->
<meta http-equiv="Content-Security-Policy"
      content="default-src 'self';
               script-src 'self' 'unsafe-inline' https://cdn.tailwindcss.com;
               style-src 'self' 'unsafe-inline' https://fonts.googleapis.com;
               font-src 'self' https://fonts.gstatic.com;">
```

## Recommended Production Settings

### 1. API Configuration

**appsettings.Production.json:**
```json
{
  "Jwt": {
    "Issuer": "https://yourdomain.com",
    "Audience": "https://yourdomain.com",
    "TokenExpirationMinutes": 15,  // Shorter in production
    "RefreshTokenExpirationDays": 7
  },
  "Security": {
    "RequireHttps": true,
    "RequireMfa": false, // Consider true for high-value accounts
    "MaxLoginAttempts": 5,
    "LockoutDurationMinutes": 30
  }
}
```

### 2. Blazor Configuration

**wwwroot/appsettings.Production.json:**
```json
{
  "ApiBaseUrl": "https://api.yourdomain.com",
  "Security": {
    "EnableCsp": true,
    "EnableHsts": true,
    "MaxAge": 31536000
  }
}
```

### 3. Web Server Configuration

**nginx example:**
```nginx
# Force HTTPS
server {
    listen 80;
    return 301 https://$host$request_uri;
}

server {
    listen 443 ssl http2;

    # SSL Configuration
    ssl_certificate /path/to/cert.pem;
    ssl_certificate_key /path/to/key.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    # Security Headers
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;

    # CSP
    add_header Content-Security-Policy "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.tailwindcss.com;" always;
}
```

## Testing Security

### 1. Manual Testing Checklist

- [ ] Master key cleared on browser close
- [ ] Master key cleared on logout
- [ ] Token refreshed before expiry
- [ ] Invalid token redirects to login
- [ ] Encrypted data unreadable on server
- [ ] Password requirements enforced
- [ ] MFA works correctly
- [ ] HTTPS enforced in production
- [ ] CORS configured correctly
- [ ] Rate limiting works

### 2. Automated Security Testing

**Tools to use:**
- **OWASP ZAP**: Vulnerability scanning
- **Burp Suite**: Manual penetration testing
- **npm audit**: Frontend dependency vulnerabilities
- **dotnet list package --vulnerable**: Backend vulnerabilities
- **SSL Labs**: SSL/TLS configuration testing

### 3. Code Security Review

**Check for:**
- No hardcoded secrets
- No sensitive data in logs
- Proper input validation
- Output encoding
- Error messages don't leak info
- Dependencies up to date

## Incident Response

### If Master Key Compromised

**User should:**
1. Change password immediately
2. Old encrypted master key becomes invalid
3. New master key generated
4. Re-encrypt all vault data with new master key

**Implementation needed:**
```csharp
public async Task RotateMasterKeyAsync(string oldPassword, string newPassword)
{
    // 1. Decrypt old master key with old password
    var oldMasterKey = await DecryptMasterKeyAsync(oldPassword);

    // 2. Decrypt all vault data
    var allData = await DecryptAllVaultDataAsync(oldMasterKey);

    // 3. Generate new master key
    var newMasterKey = await GenerateNewMasterKeyAsync();

    // 4. Encrypt master key with new password
    var encryptedNewMasterKey = await EncryptMasterKeyAsync(newMasterKey, newPassword);

    // 5. Re-encrypt all vault data
    await ReEncryptAllVaultDataAsync(allData, newMasterKey);

    // 6. Update server
    await UpdateMasterKeyAsync(encryptedNewMasterKey);
}
```

## Compliance

### GDPR Considerations
- ✅ Zero-knowledge ensures data privacy
- ✅ User can export all data
- ✅ User can delete all data
- ✅ Data minimization (only email, phone)
- ✅ Purpose limitation (vault storage only)

### Security Standards
- ✅ OWASP Top 10 mitigation
- ✅ AES-256 (FIPS 140-2 approved)
- ✅ PBKDF2 with 100k iterations (NIST recommendation)
- ✅ TLS 1.2+ (PCI DSS requirement)

## Future Security Enhancements

### 1. Enhanced Authentication
- [ ] Biometric authentication (WebAuthn)
- [ ] Hardware security keys (FIDO2)
- [ ] SMS-based OTP backup
- [ ] Email-based OTP backup

### 2. Advanced Encryption
- [ ] End-to-end encrypted sharing
- [ ] Encrypted file attachments
- [ ] Encrypted search (homomorphic encryption)
- [ ] Quantum-resistant algorithms (post-quantum cryptography)

### 3. Monitoring & Alerts
- [ ] Login anomaly detection
- [ ] Device fingerprinting
- [ ] Geo-location tracking
- [ ] Email alerts for suspicious activity

### 4. Audit & Compliance
- [ ] Audit log all access
- [ ] Compliance dashboard
- [ ] Regular security audits
- [ ] Bug bounty program

## Resources

- [Web Crypto API Documentation](https://developer.mozilla.org/en-US/docs/Web/API/Web_Crypto_API)
- [OWASP Cryptographic Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cryptographic_Storage_Cheat_Sheet.html)
- [NIST Password Guidelines](https://pages.nist.gov/800-63-3/)
- [Zero-Knowledge Proof Explained](https://en.wikipedia.org/wiki/Zero-knowledge_proof)
