using System.Security.Cryptography;
using System.Text;

namespace Ivr.Domain.Confirmation;

public static class DeterministicSnapshotHasher
{
    public static string Compute(params string?[] fields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        string canonical = string.Join(
            "\n",
            fields.Select(field => Convert.ToBase64String(
                Encoding.UTF8.GetBytes((field ?? string.Empty).Normalize(NormalizationForm.FormC)))));
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexStringLower(hash);
    }
}
