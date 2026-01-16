# Zero-Knowledge Encryption Flow

## Overview

The Digital Vault implements true **zero-knowledge encryption** where the server never knows the user's master encryption key in plaintext. All encryption and decryption happens client-side.

## Architecture

### Key Components

1. **Master Key**: Random 256-bit key used to encrypt/decrypt all vault data
2. **Password**: User's account password (used for authentication AND key derivation)
3. **Key Derivation Salt**: Random 16-byte salt stored in database
4. **Encrypted Master Key**: Master key encrypted with password-derived key (stored in database)

### Security Principle

- Server stores: `EncryptedMasterKey`, `KeyDerivationSalt`, `KeyDerivationIterations`
- Server NEVER knows: Master key in plaintext, actual vault data in plaintext
- Client derives encryption key from password locally
- Client decrypts master key locally
- Client encrypts/decrypts all data locally

## Registration Flow

### Client-Side Steps

```javascript
// 1. User provides password
const password = "UserPassword123!";

// 2. Generate random master encryption key (256-bit)
const masterKey = crypto.getRandomValues(new Uint8Array(32));
const masterKeyBase64 = btoa(String.fromCharCode(...masterKey));

// 3. Get salt and iterations from backend (or use defaults)
// Backend returns KeyDerivationSalt when you call /api/auth/register
// For registration, you need to generate these yourself:
const salt = crypto.getRandomValues(new Uint8Array(16));
const iterations = 100000;

// 4. Derive key from password using PBKDF2
const passwordKey = await deriveKeyFromPassword(password, salt, iterations);

// 5. Encrypt master key with password-derived key
const encryptedMasterKey = await encryptMasterKey(masterKey, passwordKey);

// 6. Send to server
const response = await fetch('/api/auth/register', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: "user@example.com",
    password: password,
    encryptedMasterKey: encryptedMasterKey
  })
});
```

### Server-Side Actions

```csharp
// 1. Receive registration request
// 2. Hash password for authentication (separate from encryption)
// 3. Store encrypted master key (never decrypted server-side)
// 4. Return success with encrypted master key
```

## Login Flow

### Client-Side Steps

```javascript
// 1. User provides password
const password = "UserPassword123!";

// 2. Login to get encrypted master key and salt
const response = await fetch('/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: "user@example.com",
    password: password
  })
});

const { accessToken, user } = await response.json();

// 3. Extract encryption parameters
const { encryptedMasterKey, keyDerivationSalt, keyDerivationIterations } = user;

// 4. Derive key from password
const salt = new Uint8Array(keyDerivationSalt);
const passwordKey = await deriveKeyFromPassword(password, salt, keyDerivationIterations);

// 5. Decrypt master key locally
const masterKey = await decryptMasterKey(encryptedMasterKey, passwordKey);

// 6. Keep master key in memory (DO NOT persist!)
// Store in secure memory store (e.g., React Context with security)
sessionStorage.setItem('masterKey', btoa(String.fromCharCode(...masterKey)));
```

### Server-Side Actions

```csharp
// 1. Authenticate user with password hash
// 2. Return encrypted master key + salt + iterations
// 3. Server NEVER decrypts the master key
```

## Vault Data Operations

### Encrypting Vault Data (Client-Side)

```javascript
// When user wants to save sensitive data
const vaultData = {
  title: "My Bank Account",
  username: "john@example.com",
  password: "BankPassword123"
};

// 1. Get master key from memory
const masterKeyBase64 = sessionStorage.getItem('masterKey');
const masterKey = Uint8Array.from(atob(masterKeyBase64), c => c.charCodeAt(0));

// 2. Encrypt data with master key
const encryptedData = await encryptData(JSON.stringify(vaultData), masterKey);

// 3. Send encrypted data to server
await fetch('/api/vault', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${accessToken}`
  },
  body: JSON.stringify({
    title: "Encrypted Entry",
    encryptedData: encryptedData
  })
});
```

### Decrypting Vault Data (Client-Side)

```javascript
// When user wants to view sensitive data
// 1. Fetch encrypted data from server
const response = await fetch('/api/vault/entry-id', {
  headers: { 'Authorization': `Bearer ${accessToken}` }
});

const { encryptedData } = await response.json();

// 2. Get master key from memory
const masterKeyBase64 = sessionStorage.getItem('masterKey');
const masterKey = Uint8Array.from(atob(masterKeyBase64), c => c.charCodeAt(0));

// 3. Decrypt data with master key
const decryptedJson = await decryptData(encryptedData, masterKey);
const vaultData = JSON.parse(decryptedJson);

console.log(vaultData); // { title: "My Bank Account", username: "john@example.com", ... }
```

## Cryptographic Implementation

### Key Derivation (PBKDF2)

```javascript
async function deriveKeyFromPassword(password, salt, iterations) {
  const encoder = new TextEncoder();
  const passwordBuffer = encoder.encode(password);

  // Import password as key material
  const keyMaterial = await crypto.subtle.importKey(
    'raw',
    passwordBuffer,
    'PBKDF2',
    false,
    ['deriveBits', 'deriveKey']
  );

  // Derive 256-bit key using PBKDF2
  const derivedKey = await crypto.subtle.deriveKey(
    {
      name: 'PBKDF2',
      salt: salt,
      iterations: iterations,
      hash: 'SHA-256'
    },
    keyMaterial,
    { name: 'AES-GCM', length: 256 },
    true,
    ['encrypt', 'decrypt']
  );

  return derivedKey;
}
```

### Encrypting Master Key

```javascript
async function encryptMasterKey(masterKey, passwordKey) {
  // Generate IV for AES-GCM
  const iv = crypto.getRandomValues(new Uint8Array(12));

  // Encrypt master key
  const encrypted = await crypto.subtle.encrypt(
    {
      name: 'AES-GCM',
      iv: iv
    },
    passwordKey,
    masterKey
  );

  // Combine IV + encrypted data
  const combined = new Uint8Array(iv.length + encrypted.byteLength);
  combined.set(iv);
  combined.set(new Uint8Array(encrypted), iv.length);

  // Return as base64
  return btoa(String.fromCharCode(...combined));
}
```

### Decrypting Master Key

```javascript
async function decryptMasterKey(encryptedMasterKeyBase64, passwordKey) {
  // Decode from base64
  const combined = Uint8Array.from(atob(encryptedMasterKeyBase64), c => c.charCodeAt(0));

  // Extract IV and encrypted data
  const iv = combined.slice(0, 12);
  const encrypted = combined.slice(12);

  // Decrypt
  const decrypted = await crypto.subtle.decrypt(
    {
      name: 'AES-GCM',
      iv: iv
    },
    passwordKey,
    encrypted
  );

  return new Uint8Array(decrypted);
}
```

### Encrypting Vault Data

```javascript
async function encryptData(plaintext, masterKey) {
  const encoder = new TextEncoder();
  const data = encoder.encode(plaintext);

  // Import master key
  const cryptoKey = await crypto.subtle.importKey(
    'raw',
    masterKey,
    'AES-GCM',
    false,
    ['encrypt']
  );

  // Generate IV
  const iv = crypto.getRandomValues(new Uint8Array(12));

  // Encrypt
  const encrypted = await crypto.subtle.encrypt(
    {
      name: 'AES-GCM',
      iv: iv
    },
    cryptoKey,
    data
  );

  // Combine IV + encrypted data
  const combined = new Uint8Array(iv.length + encrypted.byteLength);
  combined.set(iv);
  combined.set(new Uint8Array(encrypted), iv.length);

  return btoa(String.fromCharCode(...combined));
}
```

### Decrypting Vault Data

```javascript
async function decryptData(encryptedBase64, masterKey) {
  // Decode from base64
  const combined = Uint8Array.from(atob(encryptedBase64), c => c.charCodeAt(0));

  // Extract IV and encrypted data
  const iv = combined.slice(0, 12);
  const encrypted = combined.slice(12);

  // Import master key
  const cryptoKey = await crypto.subtle.importKey(
    'raw',
    masterKey,
    'AES-GCM',
    false,
    ['decrypt']
  );

  // Decrypt
  const decrypted = await crypto.subtle.decrypt(
    {
      name: 'AES-GCM',
      iv: iv
    },
    cryptoKey,
    encrypted
  );

  const decoder = new TextDecoder();
  return decoder.decode(decrypted);
}
```

## Security Considerations

### Master Key Storage
- **NEVER** persist master key to disk/localStorage
- Store ONLY in memory (sessionStorage or React Context)
- Clear on logout or page close
- Re-derive from password on each login

### Password Requirements
- Minimum 12 characters
- Must include uppercase, lowercase, number, special character
- Never sent to server except during authentication

### Zero-Knowledge Guarantee
- Server stores encrypted data only
- Server cannot decrypt any user data
- Even if database is compromised, data remains encrypted
- Only user's password can decrypt the master key

### Key Rotation
If user changes password:
1. Decrypt master key with old password
2. Re-encrypt master key with new password-derived key
3. Update EncryptedMasterKey in database
4. All vault data remains unchanged (still encrypted with same master key)

## API Endpoints

### POST /api/auth/register
**Request:**
```json
{
  "email": "user@example.com",
  "password": "Password123!",
  "phoneNumber": "+1234567890",
  "encryptedMasterKey": "base64-encrypted-master-key"
}
```

**Response:**
```json
{
  "accessToken": "jwt-token",
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "keyDerivationSalt": [1,2,3,...],
    "keyDerivationIterations": 100000,
    "encryptedMasterKey": "base64-encrypted-master-key"
  }
}
```

### POST /api/auth/login
**Request:**
```json
{
  "email": "user@example.com",
  "password": "Password123!"
}
```

**Response:**
```json
{
  "accessToken": "jwt-token",
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "keyDerivationSalt": [1,2,3,...],
    "keyDerivationIterations": 100000,
    "encryptedMasterKey": "base64-encrypted-master-key"
  }
}
```

## Testing

See `test-zero-knowledge.html` for a working example implementation.

## Best Practices

1. **Never log sensitive data**: Don't log master key, decrypted data, or passwords
2. **Use HTTPS**: Always use HTTPS in production to protect data in transit
3. **Clear memory on logout**: Explicitly clear master key from memory
4. **Rate limit authentication**: Prevent brute force attacks on passwords
5. **Use strong password policy**: Enforce minimum requirements
6. **Enable MFA**: Add additional layer of security with TOTP
7. **Audit logging**: Log access attempts (but not sensitive data)
8. **Regular security audits**: Review cryptographic implementation regularly
