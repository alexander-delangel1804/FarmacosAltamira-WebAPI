using Konscious.Security.Cryptography;
using FarmacosAltamira.Application.Common.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace FarmacosAltamira.Application.Seguridad.Services;

public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 3;
    private const int MemorySize = 65536; // 64 MB
    private const int Parallelism = 2;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = Parallelism,
            Iterations = Iterations,
            MemorySize = MemorySize
        };
        var hash = argon2.GetBytes(HashSize);
        var hashBytes = new byte[SaltSize + HashSize];
        Buffer.BlockCopy(salt, 0, hashBytes, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, hashBytes, SaltSize, HashSize);
        return Convert.ToBase64String(hashBytes);
    }

    public bool Verify(string password, string hash)
    {
        try
        {
            var hashBytes = Convert.FromBase64String(hash);
            if (hashBytes.Length != SaltSize + HashSize)
                return false;

            var salt = new byte[SaltSize];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);

            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = Parallelism,
                Iterations = Iterations,
                MemorySize = MemorySize
            };
            var computedHash = argon2.GetBytes(HashSize);

            return CryptographicOperations.FixedTimeEquals(computedHash, hashBytes.AsSpan(SaltSize, HashSize));
        }
        catch
        {
            return false;
        }
    }
}