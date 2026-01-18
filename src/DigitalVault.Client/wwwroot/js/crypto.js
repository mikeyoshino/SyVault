// Zero-Knowledge Cryptography Helper using Web Crypto API
// All encryption/decryption happens client-side - server never sees plaintext keys

window.cryptoHelper = {
    // Generate random 256-bit (32-byte) master encryption key
    generateMasterKey: function () {
        const masterKey = crypto.getRandomValues(new Uint8Array(32));
        return btoa(String.fromCharCode(...masterKey));
    },

    // Generate random bytes
    generateRandomBytes: function (length) {
        const bytes = crypto.getRandomValues(new Uint8Array(length));
        return btoa(String.fromCharCode(...bytes));
    },

    // Derive 256-bit key from password using PBKDF2-SHA256
    deriveKeyFromPassword: async function (password, saltArray, iterations) {
        const encoder = new TextEncoder();
        const passwordBuffer = encoder.encode(password);
        const salt = new Uint8Array(saltArray);

        // Import password as key material
        const keyMaterial = await crypto.subtle.importKey(
            'raw',
            passwordBuffer,
            'PBKDF2',
            false,
            ['deriveBits', 'deriveKey']
        );

        // Derive 256-bit key
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

        // Export and return as base64 (for passing between JS calls)
        const exportedKey = await crypto.subtle.exportKey('raw', derivedKey);
        return btoa(String.fromCharCode(...new Uint8Array(exportedKey)));
    },

    // Encrypt master key with password-derived key (AES-256-GCM)
    encryptMasterKey: async function (masterKeyBase64, passwordDerivedKeyBase64) {
        // Parse master key
        const masterKey = Uint8Array.from(atob(masterKeyBase64), c => c.charCodeAt(0));

        // Import password-derived key
        const keyData = Uint8Array.from(atob(passwordDerivedKeyBase64), c => c.charCodeAt(0));
        const cryptoKey = await crypto.subtle.importKey(
            'raw',
            keyData,
            'AES-GCM',
            false,
            ['encrypt']
        );

        // Generate random IV (12 bytes for GCM)
        const iv = crypto.getRandomValues(new Uint8Array(12));

        // Encrypt
        const encrypted = await crypto.subtle.encrypt(
            {
                name: 'AES-GCM',
                iv: iv
            },
            cryptoKey,
            masterKey
        );

        // Combine IV + encrypted data
        const combined = new Uint8Array(iv.length + encrypted.byteLength);
        combined.set(iv);
        combined.set(new Uint8Array(encrypted), iv.length);

        // Return as base64
        return btoa(String.fromCharCode(...combined));
    },

    // Decrypt master key with password-derived key (AES-256-GCM)
    decryptMasterKey: async function (encryptedMasterKeyBase64, passwordDerivedKeyBase64) {
        try {
            // Parse encrypted data
            const combined = Uint8Array.from(atob(encryptedMasterKeyBase64), c => c.charCodeAt(0));
            const iv = combined.slice(0, 12);
            const encrypted = combined.slice(12);

            // Import password-derived key
            const keyData = Uint8Array.from(atob(passwordDerivedKeyBase64), c => c.charCodeAt(0));
            const cryptoKey = await crypto.subtle.importKey(
                'raw',
                keyData,
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

            // Return as base64
            return btoa(String.fromCharCode(...new Uint8Array(decrypted)));
        } catch (error) {
            console.error('Decryption failed:', error);
            throw new Error('ไม่สามารถถอดรหัสได้ กรุณาตรวจสอบรหัสผ่าน');
        }
    },

    // Encrypt vault data with master key (AES-256-GCM)
    encryptData: async function (plaintext, masterKeyBase64) {
        const encoder = new TextEncoder();
        const data = encoder.encode(plaintext);

        // Import master key
        const keyData = Uint8Array.from(atob(masterKeyBase64), c => c.charCodeAt(0));
        const cryptoKey = await crypto.subtle.importKey(
            'raw',
            keyData,
            'AES-GCM',
            false,
            ['encrypt']
        );

        // Generate random IV
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
    },

    // Encrypt binary data (for files)
    encryptBytes: async function (dataArray, masterKeyBase64) {
        // Import master key
        const keyData = Uint8Array.from(atob(masterKeyBase64), c => c.charCodeAt(0));
        const cryptoKey = await crypto.subtle.importKey(
            'raw',
            keyData,
            'AES-GCM',
            false,
            ['encrypt']
        );

        // Generate random IV
        const iv = crypto.getRandomValues(new Uint8Array(12));

        // Encrypt
        const encrypted = await crypto.subtle.encrypt(
            {
                name: 'AES-GCM',
                iv: iv
            },
            cryptoKey,
            new Uint8Array(dataArray)
        );

        // Return object with explicit IV and Data for flexible handling
        // Convert to standard arrays for Blazor interoperability
        return {
            iv: Array.from(iv),
            encryptedData: Array.from(new Uint8Array(encrypted)),
            tag: [] // GCM tag is appended to ciphertext in Web Crypto, so usually we handle it together or separate if needed. 
            // Note: Web Crypto AES-GCM appends tag at end of ciphertext automatically.
        };
    },

    // Decrypt vault data with master key (AES-256-GCM)
    decryptData: async function (encryptedBase64, masterKeyBase64) {
        try {
            // Parse encrypted data
            const combined = Uint8Array.from(atob(encryptedBase64), c => c.charCodeAt(0));
            const iv = combined.slice(0, 12);
            const encrypted = combined.slice(12);

            // Import master key
            const keyData = Uint8Array.from(atob(masterKeyBase64), c => c.charCodeAt(0));
            const cryptoKey = await crypto.subtle.importKey(
                'raw',
                keyData,
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
        } catch (error) {
            console.error('Data decryption failed:', error);
            throw new Error('ไม่สามารถถอดรหัสข้อมูลได้');
        }
    },

    // Verify browser supports required crypto features
    isSupported: function () {
        return !!(window.crypto && window.crypto.subtle);
    }
};

// Initialize and verify crypto support
if (!cryptoHelper.isSupported()) {
    console.error('Web Crypto API is not supported in this browser');
    alert('เบราว์เซอร์นี้ไม่รองรับการเข้ารหัส กรุณาใช้เบราว์เซอร์รุ่นใหม่');
}
