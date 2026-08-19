using Ivr.Api.Auth;
using Ivr.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace Ivr.IntegrationTests.Auth;

/// <summary>
/// W-0047 / P7-5 §6.2. The rotation mechanism as the intake path actually consumes it.
/// <para>
/// Lives here rather than in Ivr.UnitTests because that project deliberately references only Domain
/// and Infrastructure — the layering ArchitectureDependencyTests enforces — and these need the API
/// layer's options type.
/// </para>
/// <para>
/// <c>SEC-ROT-01</c> proves the provider honours an overlap. These prove the API's legacy
/// credential is wired to that provider rather than to a single configured string — which is a
/// different claim, and the one that decides whether a real rotation drops requests.
/// </para>
/// </summary>
public sealed class OrderCoreCredentialSourceTests
{
    private const string Current = "current-order-core-token-not-a-real-secret";
    private const string Previous = "previous-order-core-token-not-a-real-secret";
    private static readonly DateTimeOffset Now = new(2026, 8, 19, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "SEC-ROT-06")]
    public void BothCredentialsAreAcceptedWhileARotationIsInFlight()
    {
        // The state a rolling restart actually produces: some pods hold the new value, some the
        // old, and callers are mid-migration. Accepting only one of them is the outage.
        OrderCoreCredentialSource source = Build(new OrderCoreAllowlistOptions
        {
            ServiceToken = Current,
            PreviousServiceToken = Previous,
            PreviousServiceTokenRetiresAt = Now.AddMinutes(15),
        });

        Assert.True(source.IsAccepted(Current));
        Assert.True(source.IsAccepted(Previous));
        Assert.False(source.IsAccepted("neither-of-the-two-configured-values"));
    }

    [Fact]
    [Trait("TestId", "SEC-ROT-07")]
    public void AnExpiredPreviousCredentialIsRefusedEvenThoughItIsStillConfigured()
    {
        // The variable is still sitting in the values file — the half of the runbook nobody ran.
        // The retirement instant is what closes the window, so forgetting to delete it costs
        // nothing. A configured-but-expired credential that still worked would be the rotation
        // that never finishes.
        OrderCoreCredentialSource source = Build(new OrderCoreAllowlistOptions
        {
            ServiceToken = Current,
            PreviousServiceToken = Previous,
            PreviousServiceTokenRetiresAt = Now.AddMinutes(-1),
        });

        Assert.True(source.IsAccepted(Current));
        Assert.False(source.IsAccepted(Previous));
    }

    [Fact]
    [Trait("TestId", "SEC-ROT-08")]
    public void AnUnconfiguredCredentialAuthenticatesNobody()
    {
        // Seeding an empty expected value would make an empty supplied value match, which would
        // authenticate nobody's request as everybody's. The absence has to mean "refuse", not
        // "compare two empty strings".
        OrderCoreCredentialSource source = Build(new OrderCoreAllowlistOptions
        {
            ServiceToken = string.Empty,
        });

        Assert.False(source.IsAccepted(string.Empty));
        Assert.False(source.IsAccepted(Current));
        Assert.Empty(source.Audit);
    }

    [Fact]
    [Trait("TestId", "SEC-ROT-09")]
    public void TheRotationIsRecordedWithoutRecordingEitherCredential()
    {
        OrderCoreCredentialSource source = Build(new OrderCoreAllowlistOptions
        {
            ServiceToken = Current,
            PreviousServiceToken = Previous,
            PreviousServiceTokenRetiresAt = Now.AddMinutes(15),
        });

        string rendered = string.Join(
            "\n",
            source.Audit.Select(entry => $"{entry.Generation} {entry.Kind} {entry.Fingerprint}"));

        Assert.DoesNotContain(Current, rendered, StringComparison.Ordinal);
        Assert.DoesNotContain(Previous, rendered, StringComparison.Ordinal);
        Assert.Contains(RotatingCredentialProvider.Fingerprint(Current), rendered, StringComparison.Ordinal);
        Assert.Contains(RotatingCredentialProvider.Fingerprint(Previous), rendered, StringComparison.Ordinal);
    }

    private static OrderCoreCredentialSource Build(OrderCoreAllowlistOptions options) =>
        new(Options.Create(options), new FixedClock(Now));

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
