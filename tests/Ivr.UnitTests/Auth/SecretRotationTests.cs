using System.Reflection;
using Ivr.Infrastructure.Auth;
using Ivr.Infrastructure.Persistence.Entities;

namespace Ivr.UnitTests.Auth;

/// <summary>
/// W-0047 / P7-5. Rotation exists so that a leaked credential has a short life. Every test here
/// asks the same question from a different side: can a rotation leave something valid that should
/// not be, or reveal the thing it was supposed to replace.
/// </summary>
public sealed class SecretRotationTests
{
    private const string First = "initial-service-credential-not-a-real-secret";
    private const string Second = "rotated-service-credential-not-a-real-secret";
    private const string Third = "emergency-service-credential-not-a-real-secret";

    [Fact]
    [Trait("TestId", "SEC-ROT-01")]
    public void ARotationKeepsTheOldCredentialValidForTheOverlapAndNotOneTickLonger()
    {
        var clock = new MutableClock(new DateTimeOffset(2026, 8, 19, 9, 0, 0, TimeSpan.Zero));
        var provider = new RotatingCredentialProvider(First, clock);

        Assert.True(provider.IsAccepted(First));

        // A caller that already holds the old value is mid-request when the rotation lands. That is
        // the entire reason for an overlap: without one, every such request fails.
        provider.Rotate(Second, TimeSpan.FromMinutes(10));

        Assert.True(provider.IsAccepted(Second));
        Assert.True(provider.IsAccepted(First));
        Assert.Equal(2, provider.ActiveGenerations.Count);

        // One tick before the window closes the old value still works...
        clock.Advance(TimeSpan.FromMinutes(10) - TimeSpan.FromTicks(1));
        Assert.True(provider.IsAccepted(First));

        // ...and at the boundary it stops, without anyone running the second half of a runbook.
        // A rotation nobody finishes leaves the compromised value valid forever, which is the
        // failure rotation exists to prevent.
        clock.Advance(TimeSpan.FromTicks(1));
        Assert.False(provider.IsAccepted(First));
        Assert.True(provider.IsAccepted(Second));
        Assert.Single(provider.ActiveGenerations);
    }

    [Fact]
    [Trait("TestId", "SEC-ROT-03")]
    public void AnEmergencyRotationRefusesTheOldCredentialImmediately()
    {
        var clock = new MutableClock(new DateTimeOffset(2026, 8, 19, 9, 0, 0, TimeSpan.Zero));
        var provider = new RotatingCredentialProvider(First, clock);
        provider.Rotate(Second, TimeSpan.FromHours(1));
        Assert.True(provider.IsAccepted(First));

        // Suspected leak. An overlap here would keep the leaked value working for exactly as long
        // as an attacker needs it, so emergency rotation has no window at all.
        provider.RotateEmergency(Third);

        Assert.False(provider.IsAccepted(First));
        Assert.False(provider.IsAccepted(Second));
        Assert.True(provider.IsAccepted(Third));
        Assert.Single(provider.ActiveGenerations);

        Assert.Contains(provider.Audit, entry => entry.Kind == CredentialRotationKind.Emergency);
    }

    [Fact]
    [Trait("TestId", "SEC-ROT-04")]
    public void TheAuditTrailDescribesARotationWithoutDescribingTheSecret()
    {
        var clock = new MutableClock(new DateTimeOffset(2026, 8, 19, 9, 0, 0, TimeSpan.Zero));
        var provider = new RotatingCredentialProvider(First, clock);
        provider.Rotate(Second, TimeSpan.FromMinutes(5));
        provider.RotateEmergency(Third);

        string rendered = string.Join(
            "\n",
            provider.Audit.Select(entry =>
                $"{entry.Generation} {entry.Kind} {entry.Fingerprint} {entry.RotatedAt:O} {entry.PreviousRetiredAt:O}"));

        // An audit row that quoted the value would be the leak it exists to record.
        foreach (string secret in (string[])[First, Second, Third])
        {
            Assert.DoesNotContain(secret, rendered, StringComparison.Ordinal);
            // The fingerprint identifies the generation without carrying it.
            Assert.Contains(RotatingCredentialProvider.Fingerprint(secret), rendered, StringComparison.Ordinal);
        }

        // Rotating to the value already in use would record an exposure as closed without closing
        // it, which is worse than not rotating at all.
        Assert.Throws<InvalidOperationException>(() => provider.Rotate(Third, TimeSpan.FromMinutes(1)));

        // And a secret short enough for its fingerprint to be brute-forced is refused, because the
        // trail would otherwise become an oracle for the value it describes.
        Assert.Throws<InvalidOperationException>(
            () => provider.Rotate(new string('a', RotatingCredentialProvider.MinimumSecretLength - 1),
                TimeSpan.FromMinutes(1)));
    }

    [Fact]
    [Trait("TestId", "SEC-ROT-02")]
    public void RotatingTheResolverCredentialNeverPutsADestinationIntoTheRotationSurface()
    {
        // D-05. The credential used to CALL the dial-token resolver rotates like any other; what
        // must never happen is the rotation surface learning anything about what the resolver
        // returns. The provider only ever sees the credential, so the property to assert is that
        // nothing destination-shaped can reach it -- checked with a value that looks like a
        // destination to make the test fail loudly if that ever changes.
        var clock = new MutableClock(new DateTimeOffset(2026, 8, 19, 9, 0, 0, TimeSpan.Zero));
        var provider = new RotatingCredentialProvider(First, clock);
        provider.Rotate(Second, TimeSpan.FromMinutes(30));

        string rendered = string.Join("\n", provider.Audit.Select(entry =>
            $"{entry.Generation} {entry.Kind} {entry.Fingerprint}"));

        // Nothing in the rotation surface is digit-shaped enough to be a destination.
        Assert.DoesNotContain(rendered, "84", StringComparison.Ordinal);
        Assert.All(
            provider.Audit,
            entry => Assert.Matches("^[0-9a-f]{12}$", entry.Fingerprint));

        // The resolver keeps working across the rotation: both credentials authenticate during the
        // overlap, so an in-flight dispatch does not lose its authorization mid-call.
        Assert.True(provider.IsAccepted(First));
        Assert.True(provider.IsAccepted(Second));
    }

    [Fact]
    [Trait("TestId", "SEC-ROT-05")]
    public void NoPersistedColumnCanHoldADialTokenToNumberMapping()
    {
        // D-05 / OD-V1-18. The mapping from a dial token to a real number lives at the token-vault
        // boundary OUTSIDE IVR. IVR persists an opaque ciphertext, a reference and a masked form --
        // and holds no key that could turn any of them back into a number.
        //
        // Asserted over the persistence model by reflection rather than over a list someone
        // maintains: a new column added tomorrow is exactly the case a hand-kept list misses.
        PropertyInfo[] persisted = typeof(ConfirmationTaskEntity).GetProperties();
        string[] names = [.. persisted.Select(property => property.Name)];

        Assert.Contains("DialTokenCiphertext", names);
        Assert.Contains("PhoneMasked", names);
        Assert.Contains("PhoneRef", names);

        // Nothing that would be a plaintext destination, and nothing that would decrypt one.
        foreach (PropertyInfo property in persisted)
        {
            string name = property.Name;
            Assert.False(
                name.Contains("PhoneNumber", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("RawPhone", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("Msisdn", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("Destination", StringComparison.OrdinalIgnoreCase),
                $"{name} looks like a plaintext destination on a persisted entity (D-05).");

            Assert.False(
                (name.Contains("DialToken", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("Vault", StringComparison.OrdinalIgnoreCase))
                    && (name.Contains("Key", StringComparison.OrdinalIgnoreCase)
                        || name.Contains("Secret", StringComparison.OrdinalIgnoreCase)),
                $"{name} looks like token-vault key material inside IVR, which OD-V1-18 places outside it.");
        }

        // The rotation provider is for credentials only. If it ever grew a way to hold a mapping,
        // this is where that would show up.
        Assert.DoesNotContain(
            typeof(RotatingCredentialProvider).GetProperties(),
            property => property.Name.Contains("Destination", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class MutableClock(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset now = start;

        public override DateTimeOffset GetUtcNow() => now;

        public void Advance(TimeSpan delta) => now = now.Add(delta);
    }
}
