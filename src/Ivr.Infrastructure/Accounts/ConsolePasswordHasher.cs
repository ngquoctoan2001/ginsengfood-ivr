using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Ivr.Infrastructure.Accounts;

public enum ConsolePasswordVerificationResult
{
    Failed,
    Success,
    SuccessRehashNeeded,
}

public static class ConsolePasswordHasher
{
    private const string Marker = "PBKDF2-SHA512";
    private const int Iterations = 210_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public static string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA512,
            HashSize);
        return string.Join(
            '$',
            Marker,
            Iterations.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public static ConsolePasswordVerificationResult Verify(string encodedHash, string password)
    {
        if (string.IsNullOrEmpty(encodedHash) || password is null)
        {
            return ConsolePasswordVerificationResult.Failed;
        }

        try
        {
            string[] parts = encodedHash.Split('$');
            if (parts.Length != 4
                || !string.Equals(parts[0], Marker, StringComparison.Ordinal)
                || !int.TryParse(
                    parts[1],
                    System.Globalization.NumberStyles.None,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out int iterations)
                || iterations < 100_000)
            {
                return ConsolePasswordVerificationResult.Failed;
            }

            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expected = Convert.FromBase64String(parts[3]);
            if (salt.Length < SaltSize || expected.Length != HashSize)
            {
                return ConsolePasswordVerificationResult.Failed;
            }

            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA512,
                expected.Length);
            if (!CryptographicOperations.FixedTimeEquals(actual, expected))
            {
                return ConsolePasswordVerificationResult.Failed;
            }

            return iterations < Iterations
                ? ConsolePasswordVerificationResult.SuccessRehashNeeded
                : ConsolePasswordVerificationResult.Success;
        }
        catch (FormatException)
        {
            return ConsolePasswordVerificationResult.Failed;
        }
        catch (CryptographicException)
        {
            return ConsolePasswordVerificationResult.Failed;
        }
    }
}
