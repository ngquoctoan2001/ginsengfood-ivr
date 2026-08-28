using Ivr.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Auth;

/// <summary>
/// W-0128. Owns the bounded current/previous overlap for each admin tier.
/// <para>
/// A token is a capability: a value shared by two tiers would silently widen the lower tier to
/// the higher one. Construction therefore refuses every duplicate across all current and previous
/// slots. Previous values are accepted only until their absolute retirement instant, so a rollout
/// restart cannot extend the overlap.
/// </para>
/// </summary>
public sealed class AdminCredentialSource
{
    private readonly Dictionary<AdminScope, RotatingCredentialProvider> providers;

    public AdminCredentialSource(IOptions<AdminAccessOptions> options, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);
        AdminAccessOptions value = options.Value;
        ValidateConfiguration(value);

        var configured = new Dictionary<AdminScope, RotatingCredentialProvider>();
        AddProvider(
            configured,
            AdminScope.Read,
            value.ReadToken,
            value.ReadTokenPrevious,
            value.ReadTokenPreviousRetiresAt,
            timeProvider);
        AddProvider(
            configured,
            AdminScope.Write,
            value.WriteToken,
            value.WriteTokenPrevious,
            value.WriteTokenPreviousRetiresAt,
            timeProvider);
        AddProvider(
            configured,
            AdminScope.Danger,
            value.DangerToken,
            value.DangerTokenPrevious,
            value.DangerTokenPreviousRetiresAt,
            timeProvider);
        providers = configured;
    }

    public AdminScope? Match(string? supplied)
    {
        if (string.IsNullOrWhiteSpace(supplied))
        {
            return null;
        }

        foreach (AdminScope scope in new[] { AdminScope.Danger, AdminScope.Write, AdminScope.Read })
        {
            if (providers.TryGetValue(scope, out RotatingCredentialProvider? provider)
                && provider.IsAccepted(supplied))
            {
                return scope;
            }
        }

        return null;
    }

    private static void AddProvider(
        Dictionary<AdminScope, RotatingCredentialProvider> target,
        AdminScope scope,
        string current,
        string previous,
        DateTimeOffset? previousRetiresAt,
        TimeProvider timeProvider)
    {
        if (string.IsNullOrWhiteSpace(current))
        {
            return;
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        bool hasActivePrevious = !string.IsNullOrWhiteSpace(previous)
            && previousRetiresAt is { } retiresAt
            && retiresAt > now;
        if (!hasActivePrevious)
        {
            target.Add(scope, new RotatingCredentialProvider(current, timeProvider));
            return;
        }

        var provider = new RotatingCredentialProvider(previous, timeProvider);
        provider.Rotate(current, previousRetiresAt!.Value - now);
        target.Add(scope, provider);
    }

    private static void ValidateConfiguration(AdminAccessOptions options)
    {
        (string Current, string Previous, DateTimeOffset? RetiresAt)[] tiers =
        [
            (options.ReadToken, options.ReadTokenPrevious, options.ReadTokenPreviousRetiresAt),
            (options.WriteToken, options.WriteTokenPrevious, options.WriteTokenPreviousRetiresAt),
            (options.DangerToken, options.DangerTokenPrevious, options.DangerTokenPreviousRetiresAt),
        ];
        if (tiers.Any(tier =>
            (!string.IsNullOrWhiteSpace(tier.Current)
                && tier.Current.Length < RotatingCredentialProvider.MinimumSecretLength)
            || (!string.IsNullOrWhiteSpace(tier.Previous)
                && tier.Previous.Length < RotatingCredentialProvider.MinimumSecretLength)
            || (!string.IsNullOrWhiteSpace(tier.Previous)
                && (string.IsNullOrWhiteSpace(tier.Current) || tier.RetiresAt is null))))
        {
            throw new InvalidOperationException(
                "Each admin rotation requires a current token, an absolute retirement instant, "
                + $"and tokens of at least {RotatingCredentialProvider.MinimumSecretLength} characters.");
        }

        string[] configured =
        [
            options.ReadToken,
            options.ReadTokenPrevious,
            options.WriteToken,
            options.WriteTokenPrevious,
            options.DangerToken,
            options.DangerTokenPrevious,
        ];
        string? duplicate = configured
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .GroupBy(value => value, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1)
            ?.Key;
        if (duplicate is not null)
        {
            throw new InvalidOperationException(
                "Admin credentials must be distinct across every tier and rotation slot; "
                + "a duplicate would widen the lower tier to the higher tier.");
        }
    }
}
