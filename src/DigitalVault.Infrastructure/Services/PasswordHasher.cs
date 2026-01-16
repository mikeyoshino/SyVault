using System.Security.Cryptography;
using System.Text;
using DigitalVault.Application.Interfaces;

namespace DigitalVault.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int Iterations = 10000;
    private const int HashSize = 32;

    public (string Hash, string Salt) HashPassword(string password)
    {
        // Generate salt
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);

        // Hash password with PBKDF2
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256);

        var hashBytes = pbkdf2.GetBytes(HashSize);
        var hash = Convert.ToBase64String(hashBytes);

        return (hash, salt);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256);

        var hashBytes = pbkdf2.GetBytes(HashSize);
        var computedHash = Convert.ToBase64String(hashBytes);

        return computedHash == hash;
    }
}
