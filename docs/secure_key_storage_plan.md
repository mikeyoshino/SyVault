# Secure Master Key Storage Implementation

## Security Requirements
- ✅ Master Key persists across page refreshes
- ✅ Master Key cleared on logout
- ✅ Master Key never stored in plaintext
- ✅ Resistant to XSS attacks (encrypted at rest)
- ✅ Resistant to disk theft (password required)

## Architecture

### Storage Layers
```
User Password (never stored)
    ↓ PBKDF2 (100k iterations)
Password-Derived Key (in memory only)
    ↓ AES-256-GCM
Encrypted Master Key → localStorage (persistent)
    ↓ Decrypt on unlock
Master Key → sessionStorage (temporary)
    ↓ AES-256-GCM
Encrypted Files → S3
```

### Flow Diagrams

#### Login Flow
```
1. User enters password
2. Derive key from password (PBKDF2)
3. Check if encrypted Master Key exists in localStorage
   - If YES: Decrypt it with derived key
   - If NO: Generate new Master Key, encrypt it, save to localStorage
4. Save decrypted Master Key to sessionStorage
5. Clear password from memory
```

#### Page Refresh Flow
```
1. Check sessionStorage for Master Key
   - If found: Use it (fast path)
   - If not found: Check localStorage for encrypted Master Key
2. If encrypted key exists:
   - Show "Unlock Vault" prompt
   - User enters password
   - Derive key from password
   - Decrypt Master Key
   - Save to sessionStorage
3. If no encrypted key: Redirect to login
```

#### Logout Flow
```
1. Clear sessionStorage (Master Key)
2. Clear localStorage (Encrypted Master Key)
3. Clear all auth tokens
4. Redirect to login
```

## Implementation Plan

### Phase 1: Update SecureStorageService
- [x] Add `SaveEncryptedMasterKeyAsync(encryptedKey, salt)`
- [x] Add `GetEncryptedMasterKeyAsync()` → returns {encryptedKey, salt}
- [x] Update `SaveMasterKeyAsync` to NOT save to localStorage
- [x] Update `GetMasterKeyAsync` to check sessionStorage only

### Phase 2: Update AuthService
- [x] On login: Derive key from password
- [x] Generate or decrypt Master Key
- [x] Encrypt Master Key with derived key
- [x] Save encrypted version to localStorage
- [x] Save decrypted version to sessionStorage

### Phase 3: Create VaultUnlockService
- [x] Detect when Master Key is missing from sessionStorage
- [x] Show unlock prompt (password input)
- [x] Decrypt Master Key from localStorage
- [x] Restore to sessionStorage

### Phase 4: Update UI Components
- [ ] Add "Unlock Vault" modal component
- [ ] Integrate unlock check in App.razor or MainLayout
- [ ] Show lock icon when vault is locked

## Security Considerations

### ✅ Protections
- **XSS Attack**: Even if attacker steals localStorage, they get encrypted key (useless without password)
- **Disk Theft**: Encrypted key on disk requires user's password to decrypt
- **Memory Dump**: Password-derived key never persisted, only in memory during unlock
- **Session Hijacking**: Master Key in sessionStorage, cleared on browser close

### ⚠️ Remaining Risks
- **Keylogger**: Can capture password during unlock (requires OS-level malware)
- **Memory Scraping**: Master Key vulnerable while in sessionStorage (requires advanced attack)
- **Phishing**: User could be tricked into entering password on fake site

### 🔐 Mitigation Strategies
1. **Auto-lock timer**: Clear sessionStorage after inactivity
2. **Biometric unlock**: Use WebAuthn for password-less unlock (future)
3. **Hardware security**: Integrate with YubiKey or similar (future)
4. **Content Security Policy**: Prevent XSS attacks at source

## Files to Modify

1. `SecureStorageService.cs` - Storage layer
2. `AuthService.cs` - Login/logout flow
3. `VaultUnlockService.cs` (new) - Unlock logic
4. `UnlockVaultModal.razor` (new) - UI component
5. `App.razor` or `MainLayout.razor` - Integration point

## Testing Checklist

- [ ] Login → Master Key generated and encrypted
- [ ] Refresh page → Vault locked, unlock prompt appears
- [ ] Enter password → Vault unlocks, files accessible
- [ ] Logout → All keys cleared
- [ ] Wrong password → Unlock fails gracefully
- [ ] Multiple tabs → Unlock in one tab unlocks all tabs (via localStorage event)
