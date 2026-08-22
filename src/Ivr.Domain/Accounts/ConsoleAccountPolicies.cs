using System.Text.RegularExpressions;

namespace Ivr.Domain.Accounts;

public static class ConsoleAccountRoles
{
    public const string Admin = "Admin";
    public const string Operator = "Operator";

    public static IReadOnlyList<string> All { get; } = [Admin, Operator];

    public static bool IsDefined(string? role) =>
        All.Contains(role, StringComparer.Ordinal);
}

public static class ConsoleAccountStatuses
{
    public const string Active = "ACTIVE";
    public const string Disabled = "DISABLED";
    public const string Deleted = "DELETED";

    public static IReadOnlyList<string> All { get; } = [Active, Disabled, Deleted];

    public static bool IsDefined(string? status) =>
        All.Contains(status, StringComparer.Ordinal);
}

public static partial class ConsoleUsernamePolicy
{
    public const int MinimumLength = 3;
    public const int MaximumLength = 64;

    [GeneratedRegex("^[a-z][a-z0-9._-]{2,63}$", RegexOptions.CultureInvariant)]
    private static partial Regex AllowedPattern();

    public static string Normalize(string? username) =>
        (username ?? string.Empty).Trim().ToLowerInvariant();

    public static bool IsValid(string? username)
    {
        string normalized = Normalize(username);
        return normalized.Length is >= MinimumLength and <= MaximumLength
            && AllowedPattern().IsMatch(normalized);
    }
}

/// <summary>
/// W-0105. Validation for a staff member's display name.
/// <para>
/// This column exists to hold a person's name, so it cannot be scanned with the customer-PII
/// guard: that guard's ASCII address branch matches <c>Duong</c>, <c>Ngo</c>, <c>Ap</c>,
/// <c>Thon</c> and <c>Hem</c> followed by a space, which rejects the unaccented spelling of two
/// very common Vietnamese surnames. What still matters here is that the field must not become a
/// back door for a contact number, and must not carry control characters into logs, headers or
/// the console.
/// </para>
/// </summary>
public static class ConsoleDisplayNamePolicy
{
    public const int MinimumLength = 1;
    public const int MaximumLength = 128;

    public static string Normalize(string? displayName) => (displayName ?? string.Empty).Trim();

    public static bool IsValid(string? displayName)
    {
        string normalized = Normalize(displayName);
        return normalized.Length is >= MinimumLength and <= MaximumLength
            && !normalized.Any(char.IsControl)
            && Ivr.Domain.Privacy.PiiGuard.IsSafeContactText(normalized);
    }
}

public static class ConsolePasswordPolicy
{
    public const int MinimumLength = 12;
    public const int MaximumLength = 128;

    public static bool IsValid(string? password, string username)
    {
        if (string.IsNullOrEmpty(password)
            || password.Length is < MinimumLength or > MaximumLength
            || password.Any(char.IsWhiteSpace)
            || password.Contains(username, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return password.Any(character => character is >= 'a' and <= 'z')
            && password.Any(character => character is >= 'A' and <= 'Z')
            && password.Any(char.IsAsciiDigit)
            && password.Any(character => !char.IsAsciiLetterOrDigit(character));
    }
}

public static class ConsoleLockoutPolicy
{
    public const int MaximumFailedAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public static DateTimeOffset? LockedUntil(int failedAttempts, DateTimeOffset now) =>
        failedAttempts >= MaximumFailedAttempts ? now.Add(LockoutDuration) : null;
}
