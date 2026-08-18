using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Ivr.Infrastructure.Auth;

/// <summary>
/// Scopes IVR's own surfaces require from a calling service (W-0032 / P4-4 §2.2).
/// Proposed in <c>specs/api/01-conventions.md</c>; still pending <c>OD-V1-07</c>, so these names
/// are what the mock issuer mints and what the validator enforces — not an approved profile.
/// </summary>
public static class ServiceIdentityScopes
{
    public const string TaskWrite = "ivr.task.write";
    public const string InternalWrite = "ivr.internal.write";
    public const string AdminRead = "ivr.admin.read";
    public const string AdminWrite = "ivr.admin.write";

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        TaskWrite,
        InternalWrite,
        AdminRead,
        AdminWrite,
    };
}

/// <summary>Which identity profile the process runs under.</summary>
public enum ServiceIdentityMode
{
    /// <summary>In-process mock issuer with a per-process key. Never a production claim.</summary>
    Mock,

    /// <summary>Owner-approved external issuer. Blocked until <c>OD-V1-07</c> closes.</summary>
    Real,
}

public sealed class ServiceIdentityOptions
{
    public const string SectionName = "ServiceIdentity";

    public ServiceIdentityMode Mode { get; set; } = ServiceIdentityMode.Mock;

    public string Issuer { get; set; } = "https://ivr-mock-issuer.invalid";

    public string Audience { get; set; } = "ivr-order-confirmation";

    /// <summary>Audience IVR asks for when calling Sales.</summary>
    public string EgressAudience { get; set; } = "sales-order-core";

    /// <summary>Tolerated clock difference. Zero is not usable across real hosts; keep it small.</summary>
    public int ClockSkewSeconds { get; set; } = 30;

    /// <summary>Mock-issued token lifetime.</summary>
    public int MockTokenLifetimeSeconds { get; set; } = 300;

    /// <summary>Refresh this far before expiry so a call never carries a token about to die.</summary>
    public int RefreshSkewSeconds { get; set; } = 60;

    /// <summary>
    /// mTLS is an owner decision (<c>OD-V1-07</c>) and is not approved. The flag exists so the
    /// hook has a home and a negative test; turning it on without a signed profile is refused.
    /// </summary>
    public bool MutualTlsEnabled { get; set; }

    public bool MutualTlsProfileApproved { get; set; }

    /// <summary>
    /// Legacy shared-secret header. Accepted only under the current Golden Hour compatibility
    /// provider profile, never as Target V1 authentication (P4-4 §4).
    /// </summary>
    public bool CurrentCompatTokenEnabled { get; set; }
}

public sealed class ServiceIdentityOptionsValidator : IValidateOptions<ServiceIdentityOptions>
{
    public ValidateOptionsResult Validate(string? name, ServiceIdentityOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            failures.Add("ServiceIdentity.Issuer must be set.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add("ServiceIdentity.Audience must be set.");
        }

        if (options.ClockSkewSeconds is < 0 or > 300)
        {
            failures.Add("ServiceIdentity.ClockSkewSeconds must be between 0 and 300.");
        }

        if (options.MockTokenLifetimeSeconds is < 30 or > 3600)
        {
            failures.Add("ServiceIdentity.MockTokenLifetimeSeconds must be between 30 and 3600.");
        }

        if (options.RefreshSkewSeconds < 0
            || options.RefreshSkewSeconds >= options.MockTokenLifetimeSeconds)
        {
            failures.Add(
                "ServiceIdentity.RefreshSkewSeconds must be non-negative and shorter than the "
                + "token lifetime, otherwise every token is refreshed on issue.");
        }

        // The only mTLS state that boots is "off", or "on with an approved profile" — which no
        // configuration can currently produce, because no profile has been signed.
        if (options.MutualTlsEnabled && !options.MutualTlsProfileApproved)
        {
            failures.Add(
                "ServiceIdentity.MutualTlsEnabled requires an owner-approved profile "
                + "(OD-V1-07 / W-0006). Mock evidence never approves it.");
        }

        if (options.Mode == ServiceIdentityMode.Real)
        {
            failures.Add(
                "ServiceIdentity.Mode=Real is BLOCKED_EXTERNAL until the production auth profile "
                + "and sandbox credentials exist (W-0006 / OD-V1-07).");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}

public enum ServiceIdentityFailure
{
    None,
    Malformed,
    SignatureInvalid,
    UnsupportedAlgorithm,
    UnknownKey,
    IssuerRejected,
    AudienceRejected,
    Expired,
    NotYetValid,
    ScopeMissing,
    SubjectMissing,
    KeySourceUnavailable,
}

public sealed record ServiceIdentityResult(
    bool Succeeded,
    ServiceIdentityFailure Failure,
    string? Subject,
    IReadOnlySet<string> Scopes)
{
    public static ServiceIdentityResult Reject(ServiceIdentityFailure failure) =>
        new(false, failure, null, new HashSet<string>(StringComparer.Ordinal));

    public static ServiceIdentityResult Accept(string subject, IReadOnlySet<string> scopes) =>
        new(true, ServiceIdentityFailure.None, subject, scopes);

    /// <summary>
    /// Safe to log and to return in a readiness reason: names the failure class only. It never
    /// contains the token, a claim value, a key, or any part of the presented credential.
    /// </summary>
    public string SafeReason => Failure.ToString();
}

/// <summary>Supplies the public keys a token may be validated against.</summary>
public interface IServiceSigningKeySource
{
    /// <summary>Null means the key source could not answer — which is a rejection, not a pass.</summary>
    public IReadOnlyList<SecurityKey>? TryGetKeys();

    /// <summary>How many times keys were re-read. Lets tests prove caching and rotation.</summary>
    public int ResolveCount { get; }
}

/// <summary>
/// Validates a service-identity JWT (W-0032 / P4-4 §2.2, §2.7).
/// <para>
/// Fails closed on every path: an unknown key, an unavailable key source, an unsupported
/// algorithm and an unreadable token are all rejections. There is no branch where a token that
/// could not be verified is treated as good enough to proceed, and no configuration that turns
/// verification off.
/// </para>
/// </summary>
public interface IServiceJwtValidator
{
    public ValueTask<ServiceIdentityResult> ValidateAsync(
        string? token,
        string requiredScope,
        CancellationToken cancellationToken);
}

public sealed class ServiceJwtValidator(
    IServiceSigningKeySource keySource,
    TimeProvider timeProvider,
    IOptions<ServiceIdentityOptions> options) : IServiceJwtValidator
{
    // Only asymmetric RS256. Listing it explicitly is the point: it makes `alg: none` and an
    // HS256 token signed with the public key impossible, which is the classic JWT bypass pair.
    private static readonly string[] AcceptedAlgorithms = [SecurityAlgorithms.RsaSha256];

    private readonly JsonWebTokenHandler _handler = new();

    public async ValueTask<ServiceIdentityResult> ValidateAsync(
        string? token,
        string requiredScope,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requiredScope);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(token))
        {
            return ServiceIdentityResult.Reject(ServiceIdentityFailure.Malformed);
        }

        IReadOnlyList<SecurityKey>? keys = keySource.TryGetKeys();
        if (keys is not { Count: > 0 })
        {
            // Identity provider outage. Rejecting is the whole point of fail-closed: an outage
            // must not become an open door, and a cached "it was fine last time" is not evidence.
            return ServiceIdentityResult.Reject(ServiceIdentityFailure.KeySourceUnavailable);
        }

        ServiceIdentityOptions settings = options.Value;
        var parameters = new TokenValidationParameters
        {
            ValidIssuer = settings.Issuer,
            ValidateIssuer = true,
            ValidAudience = settings.Audience,
            ValidateAudience = true,
            // Lifetime is checked below against the injected clock instead of the machine
            // clock, so expiry behaviour is deterministic and testable rather than dependent on
            // when the suite happens to run.
            ValidateLifetime = false,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keys,
            ValidAlgorithms = AcceptedAlgorithms,
            ClockSkew = TimeSpan.FromSeconds(settings.ClockSkewSeconds),
        };

        TokenValidationResult result;
        try
        {
            result = await _handler.ValidateTokenAsync(token, parameters).ConfigureAwait(false);
        }
        catch (ArgumentException)
        {
            return ServiceIdentityResult.Reject(ServiceIdentityFailure.Malformed);
        }

        if (!result.IsValid)
        {
            return ServiceIdentityResult.Reject(Classify(result.Exception));
        }

        if (result.SecurityToken is not JsonWebToken jwt)
        {
            return ServiceIdentityResult.Reject(ServiceIdentityFailure.Malformed);
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        TimeSpan skew = TimeSpan.FromSeconds(settings.ClockSkewSeconds);
        if (jwt.ValidTo != DateTime.MinValue && jwt.ValidTo.Add(skew) <= now.UtcDateTime)
        {
            return ServiceIdentityResult.Reject(ServiceIdentityFailure.Expired);
        }

        if (jwt.ValidFrom != DateTime.MinValue && jwt.ValidFrom.Subtract(skew) > now.UtcDateTime)
        {
            return ServiceIdentityResult.Reject(ServiceIdentityFailure.NotYetValid);
        }

        string? subject = jwt.Subject;
        if (string.IsNullOrWhiteSpace(subject))
        {
            // A verified token with no service identity cannot be attributed to a caller, and an
            // unattributable caller cannot be audited. That is a rejection, not a detail.
            return ServiceIdentityResult.Reject(ServiceIdentityFailure.SubjectMissing);
        }

        HashSet<string> scopes = ReadScopes(jwt);
        return scopes.Contains(requiredScope)
            ? ServiceIdentityResult.Accept(subject, scopes)
            : ServiceIdentityResult.Reject(ServiceIdentityFailure.ScopeMissing);
    }

    private static HashSet<string> ReadScopes(JsonWebToken jwt)
    {
        var scopes = new HashSet<string>(StringComparer.Ordinal);
        foreach (string claimType in new[] { "scope", "scp" })
        {
            if (!jwt.TryGetClaim(claimType, out System.Security.Claims.Claim? claim)
                || string.IsNullOrWhiteSpace(claim.Value))
            {
                continue;
            }

            foreach (string scope in claim.Value.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                scopes.Add(scope);
            }
        }

        return scopes;
    }

    private static ServiceIdentityFailure Classify(Exception? exception) => exception switch
    {
        SecurityTokenExpiredException => ServiceIdentityFailure.Expired,
        SecurityTokenNotYetValidException => ServiceIdentityFailure.NotYetValid,
        SecurityTokenInvalidIssuerException => ServiceIdentityFailure.IssuerRejected,
        SecurityTokenInvalidAudienceException => ServiceIdentityFailure.AudienceRejected,
        SecurityTokenInvalidAlgorithmException => ServiceIdentityFailure.UnsupportedAlgorithm,
        SecurityTokenSignatureKeyNotFoundException => ServiceIdentityFailure.UnknownKey,
        SecurityTokenInvalidSignatureException => ServiceIdentityFailure.SignatureInvalid,
        SecurityTokenMalformedException => ServiceIdentityFailure.Malformed,
        // Anything unclassified is still a rejection. Defaulting to "signature invalid" keeps the
        // safe reason honest about the only thing we know: the token was not accepted.
        _ => ServiceIdentityFailure.SignatureInvalid,
    };
}

/// <summary>
/// Whether the legacy shared-secret credential may still authenticate (W-0032 / P4-4 §2.5).
/// <para>
/// The rule is deliberately expressed against the provider profile rather than a free-standing
/// flag: the compat credential exists to serve the current Golden Hour path, and the mock profile
/// that the whole test suite runs under. Under <c>TARGET_V1</c> it is refused outright — you
/// cannot run the target profile on a static secret, whatever the configuration says.
/// </para>
/// Sunset is tracked as a closure artifact in
/// <c>docs/contracts/target-v1-closure-pack/T-07-production-auth.md</c>; IVR cannot set the date
/// unilaterally because the legacy credential is shared with Sales.
/// </summary>
public static class ServiceIdentityCompatPolicy
{
    public static bool LegacyCredentialAccepted(string? salesProviderProfile) =>
        salesProviderProfile switch
        {
            Ivr.Contracts.Sales.SalesProviderNames.FakeTargetV1 => true,
            Ivr.Contracts.Sales.SalesProviderNames.CurrentGoldenHourCompat => true,
            Ivr.Contracts.Sales.SalesProviderNames.TargetV1 => false,
            // An unrecognised profile is not a reason to be permissive.
            _ => false,
        };
}

/// <summary>
/// In-process OIDC-shaped issuer for Compose, CI and tests (W-0032 / P4-4 §2.1).
/// <para>
/// The RSA key is generated per process and never written to source or evidence. That is a
/// deliberate deviation from "deterministic keys": a committed private key would be a real secret
/// in the repository, would trip the gitleaks gate, and would be one copy-paste away from being
/// reused somewhere that matters. Tests get their determinism by sharing one issuer instance with
/// the validator, which is what reproducible test outcomes actually require.
/// </para>
/// </summary>
public sealed class MockOidcIssuer : IServiceSigningKeySource, IDisposable
{
    private readonly RSA _rsa;
    private readonly RsaSecurityKey _signingKey;
    private readonly JsonWebTokenHandler _handler = new();
    private readonly TimeProvider _timeProvider;
    private readonly IOptions<ServiceIdentityOptions> _options;
    private int _resolveCount;
    private bool _keySourceAvailable = true;

    public MockOidcIssuer(TimeProvider timeProvider, IOptions<ServiceIdentityOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _timeProvider = timeProvider;
        _options = options;
        if (options.Value.Mode != ServiceIdentityMode.Mock)
        {
            throw new InvalidOperationException(
                "The mock issuer only runs in ServiceIdentityMode.Mock. Production identity is "
                + "BLOCKED_EXTERNAL (W-0006 / OD-V1-07).");
        }

        _rsa = RSA.Create(2048);
        _signingKey = new RsaSecurityKey(_rsa)
        {
            KeyId = Convert.ToHexStringLower(
                SHA256.HashData(_rsa.ExportRSAPublicKey()))[..16],
        };
    }

    public string KeyId => _signingKey.KeyId!;

    public int ResolveCount => _resolveCount;

    /// <summary>Simulates a JWKS/identity-provider outage for the fail-closed tests.</summary>
    public void SimulateKeySourceOutage(bool unavailable) => _keySourceAvailable = !unavailable;

    public IReadOnlyList<SecurityKey>? TryGetKeys()
    {
        Interlocked.Increment(ref _resolveCount);
        return _keySourceAvailable ? [_signingKey] : null;
    }

    /// <summary>Public half only, in JWKS shape. Never exposes the private key.</summary>
    public IReadOnlyDictionary<string, object> ExportPublicJwks()
    {
        RSAParameters publicParameters = _rsa.ExportParameters(includePrivateParameters: false);
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["keys"] = new[]
            {
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["kty"] = "RSA",
                    ["use"] = "sig",
                    ["alg"] = SecurityAlgorithms.RsaSha256,
                    ["kid"] = KeyId,
                    ["n"] = Base64UrlEncoder.Encode(publicParameters.Modulus!),
                    ["e"] = Base64UrlEncoder.Encode(publicParameters.Exponent!),
                },
            },
        };
    }

    public string Issue(
        string subject,
        IEnumerable<string> scopes,
        TimeSpan? lifetime = null,
        DateTimeOffset? notBefore = null,
        string? issuerOverride = null,
        string? audienceOverride = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentNullException.ThrowIfNull(scopes);
        ServiceIdentityOptions settings = _options.Value;
        DateTimeOffset now = _timeProvider.GetUtcNow();
        DateTimeOffset issuedAt = notBefore ?? now;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuerOverride ?? settings.Issuer,
            Audience = audienceOverride ?? settings.Audience,
            NotBefore = issuedAt.UtcDateTime,
            IssuedAt = now.UtcDateTime,
            Expires = issuedAt
                .Add(lifetime ?? TimeSpan.FromSeconds(settings.MockTokenLifetimeSeconds))
                .UtcDateTime,
            SigningCredentials = new SigningCredentials(
                _signingKey,
                SecurityAlgorithms.RsaSha256),
            Claims = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["sub"] = subject,
                ["scope"] = string.Join(' ', scopes),
            },
        };

        return _handler.CreateToken(descriptor);
    }

    /// <summary>
    /// Mints a correctly signed token that carries no <c>sub</c>. This exists so the negative
    /// path — a verified token that cannot be attributed to any caller — can be exercised by a
    /// real token rather than a stub. It lives on the mock issuer, which refuses to construct
    /// outside <see cref="ServiceIdentityMode.Mock"/>, so it cannot reach a production process.
    /// </summary>
    public string IssueWithoutSubjectForNegativeTest(IEnumerable<string> scopes)
    {
        ArgumentNullException.ThrowIfNull(scopes);
        ServiceIdentityOptions settings = _options.Value;
        DateTimeOffset now = _timeProvider.GetUtcNow();
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            NotBefore = now.UtcDateTime,
            IssuedAt = now.UtcDateTime,
            Expires = now.AddSeconds(settings.MockTokenLifetimeSeconds).UtcDateTime,
            SigningCredentials = new SigningCredentials(
                _signingKey,
                SecurityAlgorithms.RsaSha256),
            Claims = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["scope"] = string.Join(' ', scopes),
            },
        };

        return _handler.CreateToken(descriptor);
    }

    public void Dispose()
    {
        _rsa.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Egress token provider (W-0032 / P4-4 §2.3): acquires a service token, caches it, and refreshes
/// before expiry. Refreshes are single-flight — a burst of concurrent callbacks must produce one
/// token acquisition, not one per caller, or a real issuer would be rate-limited by our own retry
/// storm at exactly the moment the previous token expired.
/// </summary>
public sealed class MockClientCredentialsTokenProvider(
    MockOidcIssuer issuer,
    TimeProvider timeProvider,
    IOptions<ServiceIdentityOptions> options)
    : Ivr.Domain.Ports.IServiceTokenProvider, IDisposable
{
    private readonly SemaphoreSlim _refreshGate = new(1, 1);
    private Ivr.Domain.Ports.ServiceAccessToken? _token;
    private int _acquisitionCount;

    /// <summary>Lets a test prove the refresh race produced one acquisition, not many.</summary>
    public int AcquisitionCount => Volatile.Read(ref _acquisitionCount);

    public async ValueTask<Ivr.Domain.Ports.ServiceAccessToken> GetAsync(
        string audience,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audience);
        cancellationToken.ThrowIfCancellationRequested();

        if (TryGetFresh(out Ivr.Domain.Ports.ServiceAccessToken? cached))
        {
            return cached;
        }

        await _refreshGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Re-check inside the gate: whoever won the race already refreshed for everyone.
            if (TryGetFresh(out cached))
            {
                return cached;
            }

            ServiceIdentityOptions settings = options.Value;
            DateTimeOffset now = timeProvider.GetUtcNow();
            DateTimeOffset expiresAt = now.AddSeconds(settings.MockTokenLifetimeSeconds);
            string raw = issuer.Issue(
                "ivr-order-confirmation",
                [ServiceIdentityScopes.TaskWrite],
                audienceOverride: audience);
            Interlocked.Increment(ref _acquisitionCount);
            _token = Ivr.Domain.Ports.ServiceAccessToken.CreateTrusted(raw, expiresAt);
            return _token;
        }
        finally
        {
            _refreshGate.Release();
        }
    }

    private bool TryGetFresh(out Ivr.Domain.Ports.ServiceAccessToken token)
    {
        Ivr.Domain.Ports.ServiceAccessToken? current = _token;
        DateTimeOffset now = timeProvider.GetUtcNow();
        int skew = options.Value.RefreshSkewSeconds;
        if (current is not null && current.ExpiresAt > now.AddSeconds(skew))
        {
            token = current;
            return true;
        }

        token = null!;
        return false;
    }

    public void Dispose()
    {
        _refreshGate.Dispose();
        GC.SuppressFinalize(this);
    }
}
