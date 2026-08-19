using Ivr.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Auth;

/// <summary>
/// W-0047 / P7-5 §6.2. Turns the configured token pair into a
/// <see cref="RotatingCredentialProvider"/>, so the legacy shared credential can be replaced
/// without a window where every caller fails.
/// <para>
/// The previous value is installed FIRST and then rotated out, which is the same call the runbook
/// describes rather than a second code path that happens to behave similarly. Its retirement
/// instant is enforced by the provider, so a stale
/// <c>ORDER_CORE_SERVICE_TOKEN_PREVIOUS</c> left in a values file stops working on its own — you
/// cannot forget to finish a rotation.
/// </para>
/// </summary>
public sealed class OrderCoreCredentialSource
{
    private readonly RotatingCredentialProvider? provider;

    public OrderCoreCredentialSource(
        IOptions<OrderCoreAllowlistOptions> options,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);
        OrderCoreAllowlistOptions value = options.Value;

        if (string.IsNullOrWhiteSpace(value.ServiceToken))
        {
            // No credential configured means the legacy path simply has nothing to accept. Left
            // null rather than seeded with an empty string: an empty expected value that happened
            // to be compared against an empty supplied value would authenticate nobody's request
            // as everybody's.
            provider = null;
            return;
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        bool hasPrevious = !string.IsNullOrWhiteSpace(value.PreviousServiceToken)
            && value.PreviousServiceTokenRetiresAt is { } retiresAt
            && retiresAt > now
            && !string.Equals(value.PreviousServiceToken, value.ServiceToken, StringComparison.Ordinal);

        if (!hasPrevious)
        {
            provider = new RotatingCredentialProvider(value.ServiceToken, timeProvider);
            return;
        }

        // Start from the outgoing value and rotate to the current one: the overlap is then the
        // provider's own bounded window, expiring exactly when configuration says it should.
        provider = new RotatingCredentialProvider(value.PreviousServiceToken, timeProvider);
        provider.Rotate(value.ServiceToken, value.PreviousServiceTokenRetiresAt!.Value - now);
    }

    /// <summary>Rotation history: fingerprints and timestamps, never a credential value.</summary>
    public IReadOnlyList<CredentialRotationAudit> Audit => provider?.Audit ?? [];

    public bool IsAccepted(string? supplied) => provider?.IsAccepted(supplied) ?? false;
}
