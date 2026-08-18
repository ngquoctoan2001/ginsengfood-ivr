using System.Text;
using System.Text.Json;
using Ivr.Contracts.Sales;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ivr.UnitTests.Auth;

/// <summary>
/// W-0032 / P4-4. Mock service-identity suite. Everything here runs against the in-process mock
/// issuer and proves nothing about production auth, which stays BLOCKED_EXTERNAL on W-0006.
/// </summary>
public sealed class ServiceIdentityTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 18, 6, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "UT-AUTH-JWT-01")]
    public async Task ValidTokenIsAcceptedWithSubjectAndScopes()
    {
        using Harness harness = Harness.Create();
        string token = harness.Issuer.Issue(
            "sales-platform",
            [ServiceIdentityScopes.TaskWrite]);

        ServiceIdentityResult result = await harness.Validator.ValidateAsync(
            token,
            ServiceIdentityScopes.TaskWrite,
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(ServiceIdentityFailure.None, result.Failure);
        Assert.Equal("sales-platform", result.Subject);
        Assert.Contains(ServiceIdentityScopes.TaskWrite, result.Scopes);
    }

    [Fact]
    [Trait("TestId", "UT-AUTH-JWT-02")]
    public async Task WrongIssuerAudienceScopeOrSignatureIsRejectedWithItsOwnFailure()
    {
        using Harness harness = Harness.Create();
        using Harness attacker = Harness.Create();

        (string Token, string Scope, ServiceIdentityFailure Expected)[] cases =
        [
            (harness.Issuer.Issue(
                "sales-platform",
                [ServiceIdentityScopes.TaskWrite],
                issuerOverride: "https://someone-elses-issuer.invalid"),
                ServiceIdentityScopes.TaskWrite,
                ServiceIdentityFailure.IssuerRejected),
            (harness.Issuer.Issue(
                "sales-platform",
                [ServiceIdentityScopes.TaskWrite],
                audienceOverride: "some-other-service"),
                ServiceIdentityScopes.TaskWrite,
                ServiceIdentityFailure.AudienceRejected),
            (harness.Issuer.Issue("sales-platform", [ServiceIdentityScopes.AdminRead]),
                ServiceIdentityScopes.TaskWrite,
                ServiceIdentityFailure.ScopeMissing),
            // Correctly shaped, correctly claimed — signed by a key we do not trust.
            (attacker.Issuer.Issue("sales-platform", [ServiceIdentityScopes.TaskWrite]),
                ServiceIdentityScopes.TaskWrite,
                ServiceIdentityFailure.UnknownKey),
        ];

        foreach ((string token, string scope, ServiceIdentityFailure expected) in cases)
        {
            ServiceIdentityResult result = await harness.Validator.ValidateAsync(
                token,
                scope,
                CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Equal(expected, result.Failure);
            Assert.Null(result.Subject);
        }
    }

    [Fact]
    [Trait("TestId", "UT-AUTH-JWT-03")]
    public async Task ExpiredAndNotYetValidTokensAreRejected()
    {
        using Harness harness = Harness.Create();
        string expired = harness.Issuer.Issue(
            "sales-platform",
            [ServiceIdentityScopes.TaskWrite],
            lifetime: TimeSpan.FromSeconds(60),
            notBefore: Now.AddHours(-2));
        string notYet = harness.Issuer.Issue(
            "sales-platform",
            [ServiceIdentityScopes.TaskWrite],
            notBefore: Now.AddHours(2));

        Assert.Equal(
            ServiceIdentityFailure.Expired,
            (await harness.Validator.ValidateAsync(
                expired,
                ServiceIdentityScopes.TaskWrite,
                CancellationToken.None)).Failure);
        Assert.Equal(
            ServiceIdentityFailure.NotYetValid,
            (await harness.Validator.ValidateAsync(
                notYet,
                ServiceIdentityScopes.TaskWrite,
                CancellationToken.None)).Failure);
    }

    [Fact]
    [Trait("TestId", "UT-AUTH-JWT-04")]
    public async Task UnsignedAndMalformedTokensAreRejectedRatherThanTrusted()
    {
        using Harness harness = Harness.Create();

        // The classic bypass: a token that claims everything and signs nothing. The algorithm
        // allowlist is what makes this impossible, which is why it is an explicit list of one.
        string header = Base64UrlEncoder.Encode(
            Encoding.UTF8.GetBytes("""{"alg":"none","typ":"JWT"}"""));
        string payload = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(
            new
            {
                iss = "https://ivr-mock-issuer.invalid",
                aud = "ivr-order-confirmation",
                sub = "sales-platform",
                scope = ServiceIdentityScopes.TaskWrite,
                exp = Now.AddHours(1).ToUnixTimeSeconds(),
                nbf = Now.AddMinutes(-1).ToUnixTimeSeconds(),
            })));
        string algNone = string.Concat(header, ".", payload, ".");

        foreach (string candidate in new[] { algNone, "not-a-token", "a.b.c", "", "   " })
        {
            ServiceIdentityResult result = await harness.Validator.ValidateAsync(
                candidate,
                ServiceIdentityScopes.TaskWrite,
                CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.NotEqual(ServiceIdentityFailure.None, result.Failure);
        }
    }

    [Fact]
    [Trait("TestId", "UT-AUTH-JWT-05")]
    public async Task VerifiedTokenWithoutAServiceIdentityIsRejected()
    {
        using Harness harness = Harness.Create();
        // Signed by the trusted key, correct issuer/audience/scope — but no `sub`. A caller that
        // cannot be attributed cannot be audited, so a valid signature is not enough.
        string token = harness.Issuer.IssueWithoutSubjectForNegativeTest(
            [ServiceIdentityScopes.TaskWrite]);

        ServiceIdentityResult result = await harness.Validator.ValidateAsync(
            token,
            ServiceIdentityScopes.TaskWrite,
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ServiceIdentityFailure.SubjectMissing, result.Failure);
    }

    [Fact]
    [Trait("TestId", "UT-AUTH-JWKS-06")]
    public async Task KeySourceOutageFailsClosedAndRecoversWithoutRestart()
    {
        using Harness harness = Harness.Create();
        string token = harness.Issuer.Issue(
            "sales-platform",
            [ServiceIdentityScopes.TaskWrite]);

        harness.Issuer.SimulateKeySourceOutage(true);
        ServiceIdentityResult duringOutage = await harness.Validator.ValidateAsync(
            token,
            ServiceIdentityScopes.TaskWrite,
            CancellationToken.None);

        // An identity-provider outage must not become an open door, and "it verified fine a
        // minute ago" is not evidence about this request.
        Assert.False(duringOutage.Succeeded);
        Assert.Equal(ServiceIdentityFailure.KeySourceUnavailable, duringOutage.Failure);
        Assert.Equal("KeySourceUnavailable", duringOutage.SafeReason);

        harness.Issuer.SimulateKeySourceOutage(false);
        Assert.True((await harness.Validator.ValidateAsync(
            token,
            ServiceIdentityScopes.TaskWrite,
            CancellationToken.None)).Succeeded);
    }

    [Fact]
    [Trait("TestId", "UT-AUTH-JWKS-07")]
    public async Task RotatedSigningKeyInvalidatesTokensMintedByTheOldKey()
    {
        using Harness before = Harness.Create();
        using Harness after = Harness.Create();
        string oldToken = before.Issuer.Issue(
            "sales-platform",
            [ServiceIdentityScopes.TaskWrite]);

        Assert.NotEqual(before.Issuer.KeyId, after.Issuer.KeyId);
        Assert.True((await before.Validator.ValidateAsync(
            oldToken,
            ServiceIdentityScopes.TaskWrite,
            CancellationToken.None)).Succeeded);
        Assert.True((await before.Validator.ValidateAsync(
            oldToken,
            ServiceIdentityScopes.TaskWrite,
            CancellationToken.None)).Succeeded);

        // After rotation the old token is not merely stale — it is unverifiable.
        ServiceIdentityResult afterRotation = await after.Validator.ValidateAsync(
            oldToken,
            ServiceIdentityScopes.TaskWrite,
            CancellationToken.None);
        Assert.False(afterRotation.Succeeded);
        Assert.Equal(ServiceIdentityFailure.UnknownKey, afterRotation.Failure);

        // Two validations, two key-source reads: the validator consults the source per request
        // rather than caching a private copy at startup, which is what lets a rotation take
        // effect without a restart.
        Assert.Equal(2, before.Issuer.ResolveCount);
    }

    [Fact]
    [Trait("TestId", "UT-AUTH-EGRESS-08")]
    public async Task ConcurrentCallersShareOneTokenAcquisitionAndRefreshBeforeExpiry()
    {
        using Harness harness = Harness.Create();
        using var provider = new MockClientCredentialsTokenProvider(
            harness.Issuer,
            harness.Time,
            harness.Options);

        ServiceAccessToken[] first = await Task.WhenAll(
            Enumerable.Range(0, 32).Select(_ =>
                provider.GetAsync("sales-order-core", CancellationToken.None).AsTask()));

        // A burst at expiry must produce one acquisition, not one per caller — otherwise our own
        // retry storm rate-limits us against a real issuer at the worst possible moment.
        Assert.Equal(1, provider.AcquisitionCount);
        Assert.Single(first.Select(token => token.RevealToTrustedTransport()).Distinct(
            StringComparer.Ordinal));

        // Move to inside the refresh skew: still valid, but close enough that a call must not
        // carry it. The provider refreshes rather than handing out a token about to die.
        harness.Time.Advance(TimeSpan.FromSeconds(
            harness.Options.Value.MockTokenLifetimeSeconds
            - harness.Options.Value.RefreshSkewSeconds + 1));
        await provider.GetAsync("sales-order-core", CancellationToken.None);
        Assert.Equal(2, provider.AcquisitionCount);
    }

    [Fact]
    [Trait("TestId", "UT-AUTH-SECRET-09")]
    public async Task NeitherTokensNorKeysSurfaceInLoggableText()
    {
        using Harness harness = Harness.Create();
        string token = harness.Issuer.Issue(
            "sales-platform",
            [ServiceIdentityScopes.TaskWrite]);
        ServiceAccessToken access = ServiceAccessToken.CreateTrusted(token, Now.AddMinutes(5));

        // ToString is what ends up in a log line, an exception message or a serialized envelope.
        Assert.Equal("[REDACTED_SERVICE_TOKEN]", access.ToString());
        Assert.DoesNotContain(token, access.ToString(), StringComparison.Ordinal);

        // A rejection reason is safe to log and to return as a readiness reason: it names the
        // failure class only, never any part of the credential that was presented.
        ServiceIdentityResult rejected = await harness.Validator.ValidateAsync(
            token[..^4],
            ServiceIdentityScopes.TaskWrite,
            CancellationToken.None);
        Assert.False(rejected.Succeeded);
        foreach (string fragment in token.Split('.'))
        {
            Assert.DoesNotContain(fragment, rejected.SafeReason, StringComparison.Ordinal);
        }

        // The published JWKS carries the public half only.
        string jwks = JsonSerializer.Serialize(harness.Issuer.ExportPublicJwks());
        foreach (string forbidden in new[] { "\"d\"", "\"p\"", "\"q\"", "PRIVATE" })
        {
            Assert.DoesNotContain(forbidden, jwks, StringComparison.Ordinal);
        }
    }

    [Fact]
    [Trait("TestId", "UT-AUTH-MTLS-10")]
    public void MutualTlsCannotBeTurnedOnWithoutAnApprovedProfileAndRealModeCannotBoot()
    {
        var validator = new ServiceIdentityOptionsValidator();

        Assert.True(validator.Validate(null, new ServiceIdentityOptions()).Succeeded);

        // The hook exists so there is somewhere for the decision to land; enabling it without a
        // signed profile is refused at startup rather than silently accepted.
        ValidateOptionsResult mtls = validator.Validate(
            null,
            new ServiceIdentityOptions { MutualTlsEnabled = true });
        Assert.True(mtls.Failed);
        Assert.Contains("OD-V1-07", string.Join(' ', mtls.Failures!), StringComparison.Ordinal);

        // No deployment can quietly claim production auth from mock evidence.
        ValidateOptionsResult real = validator.Validate(
            null,
            new ServiceIdentityOptions { Mode = ServiceIdentityMode.Real });
        Assert.True(real.Failed);
        Assert.Contains("BLOCKED_EXTERNAL", string.Join(' ', real.Failures!), StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "UT-AUTH-COMPAT-11")]
    public void LegacySharedSecretIsRefusedUnderTheTargetProfile()
    {
        // Compatibility credential lives with the compatibility paths. Under TARGET_V1 it is
        // refused outright, so the target path can never run on a static secret.
        Assert.True(ServiceIdentityCompatPolicy.LegacyCredentialAccepted(
            SalesProviderNames.FakeTargetV1));
        Assert.True(ServiceIdentityCompatPolicy.LegacyCredentialAccepted(
            SalesProviderNames.CurrentGoldenHourCompat));
        Assert.False(ServiceIdentityCompatPolicy.LegacyCredentialAccepted(
            SalesProviderNames.TargetV1));

        // An unrecognised or absent profile is not a reason to be permissive.
        Assert.False(ServiceIdentityCompatPolicy.LegacyCredentialAccepted("SOMETHING_ELSE"));
        Assert.False(ServiceIdentityCompatPolicy.LegacyCredentialAccepted(null));
    }

    private sealed class Harness : IDisposable
    {
        private Harness(
            MockOidcIssuer issuer,
            ServiceJwtValidator validator,
            AdvanceableTimeProvider time,
            IOptions<ServiceIdentityOptions> options)
        {
            Issuer = issuer;
            Validator = validator;
            Time = time;
            Options = options;
        }

        public MockOidcIssuer Issuer { get; }

        public ServiceJwtValidator Validator { get; }

        public AdvanceableTimeProvider Time { get; }

        public IOptions<ServiceIdentityOptions> Options { get; }

        public static Harness Create()
        {
            var time = new AdvanceableTimeProvider(Now);
            IOptions<ServiceIdentityOptions> options =
                Microsoft.Extensions.Options.Options.Create(new ServiceIdentityOptions());
            var issuer = new MockOidcIssuer(time, options);
            return new Harness(issuer, new ServiceJwtValidator(issuer, time, options), time, options);
        }

        public void Dispose() => Issuer.Dispose();
    }

    private sealed class AdvanceableTimeProvider(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset _now = start;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan delta) => _now = _now.Add(delta);
    }
}
