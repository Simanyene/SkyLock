using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SkyLock.Services;

public static class PasswordHashService
{
    private const int Iterations = 600000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public static string CreateHash(
        string password,
        out string passwordSalt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        passwordSalt = Convert.ToBase64String(salt);

        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(
        string password,
        string storedHash,
        string storedSalt)
    {
        if (string.IsNullOrEmpty(password) ||
            string.IsNullOrWhiteSpace(storedHash) ||
            string.IsNullOrWhiteSpace(storedSalt))
        {
            return false;
        }

        try
        {
            byte[] salt = Convert.FromBase64String(storedSalt);
            byte[] expectedHash = Convert.FromBase64String(storedHash);

            if (salt.Length != SaltSize ||
                expectedHash.Length != HashSize)
            {
                return false;
            }

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return CryptographicOperations.FixedTimeEquals(
                actualHash,
                expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}