using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Ivr.UnitTests.Persistence;

internal static class HistoricalExpandBaseline
{
    public static bool IsPinned(string migrationId)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Ivr.sln")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            return false;
        }

        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(directory.FullName, "deploy/ci/migration-expand-baseline.json")));
        string boundary = manifest.RootElement.GetProperty("supportedLegacySchema").GetString()!;
        if (string.CompareOrdinal(migrationId, boundary) >= 0
            || !manifest.RootElement.GetProperty("legacySqlSourceSha256").TryGetProperty(migrationId, out JsonElement expected))
        {
            return false;
        }

        string source = File.ReadAllText(Path.Combine(directory.FullName,
            "src/Ivr.Infrastructure/Persistence/Migrations", migrationId + ".cs"))
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        string actual = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)));
        return string.Equals(expected.GetString(), actual, StringComparison.Ordinal);
    }
}
