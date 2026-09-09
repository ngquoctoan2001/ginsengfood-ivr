using System.Security.Cryptography;
using System.Text;

namespace Ivr.Infrastructure.Auth;

/// <summary>
/// W-0047 / P7-5 §4. A shared credential that can be replaced without dropping a request.
/// <para>
/// A single-valued secret cannot be rotated without downtime: there is an instant where the caller
/// still holds the old value and the callee already expects the new one, and every request in that
/// window fails. So rotation is expressed as an OVERLAP -- two values accepted, one of them
/// preferred -- rather than as an assignment.
/// </para>
/// <para>
/// The overlap is bounded and the bound is enforced here rather than by whoever remembers to run
/// the second half of the runbook. A rotation nobody finishes leaves the compromised value valid
/// forever, which is the failure rotation exists to prevent (P7-5 §11).
/// </para>
/// </summary>
public sealed class RotatingCredentialProvider
{
    /// <summary>
    /// Floor on the length of an accepted credential.
    /// <para>
    /// This used to be described as what makes <see cref="Fingerprint"/> safe to record. It is
    /// not, and saying so was the more dangerous half of the arrangement: brute force is bounded
    /// by ENTROPY, not by length, and nothing here can require a configured value to be random.
    /// A twenty-four character passphrase a person chose falls to a dictionary in seconds no
    /// matter how the fingerprint is derived from it. What actually makes the fingerprint safe to
    /// record is the work factor on <see cref="Fingerprint"/>; the length floor is a separate,
    /// weaker guard that only rules out the very shortest values.
    /// </para>
    /// </summary>
    public const int MinimumSecretLength = 24;

    /// <summary>
    /// Domain separation for <see cref="Fingerprint"/>. Fixed rather than random because the
    /// fingerprint has to be reproducible across processes and machines to be traceable at all —
    /// which is exactly why it needs the iteration count below to be worth anything.
    /// </summary>
    private static readonly byte[] FingerprintSalt =
        Encoding.UTF8.GetBytes("ivr.credential-rotation.fingerprint.v1");

    /// <summary>
    /// OWASP's PBKDF2-HMAC-SHA256 recommendation. Measured at 34 ms per derivation on the
    /// development machine, and a credential is fingerprinted once when it is installed — at most
    /// twice per configured scope at startup — so the cost lands where nothing is waiting on it.
    /// </summary>
    private const int FingerprintIterations = 210_000;

    private readonly object sync = new();
    private readonly TimeProvider timeProvider;
    private readonly List<InstalledCredential> generations = [];

    /// <summary>
    /// Not bounded, deliberately. Trimming it would drop rotation history, and a compliance record
    /// that silently forgets is worse than one that grows: in this process it holds one entry per
    /// installed credential and one per rotation, which is one or two per configured scope for the
    /// life of the process. Nothing here rotates on a timer.
    /// </summary>
    private readonly List<CredentialRotationAudit> audit = [];

    private int nextGeneration = 1;

    public RotatingCredentialProvider(string initialSecret, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        this.timeProvider = timeProvider;
        Install(initialSecret, retireExisting: null, CredentialRotationKind.Initial);
    }

    /// <summary>
    /// Generations that would be accepted right now, newest first.
    /// <para>
    /// Describes them; does not hand them over. <see cref="CredentialGeneration"/> used to carry
    /// the secret as a <c>byte[]</c>, and this property is public, so anything holding the
    /// provider could read the credential straight back out — or serialise it into a diagnostics
    /// response without anyone noticing, since the type looked like metadata. The verifier lives
    /// in <see cref="InstalledCredential"/> now and never leaves the class.
    /// </para>
    /// </summary>
    public IReadOnlyList<CredentialGeneration> ActiveGenerations
    {
        get
        {
            lock (sync)
            {
                DateTimeOffset now = timeProvider.GetUtcNow();
                return
                [
                    .. generations
                        .Where(entry => entry.Descriptor.IsValidAt(now))
                        .Select(entry => entry.Descriptor)
                        .OrderByDescending(descriptor => descriptor.Generation),
                ];
            }
        }
    }

    /// <summary>Rotation history. Carries fingerprints and timestamps, never a secret value.</summary>
    public IReadOnlyList<CredentialRotationAudit> Audit
    {
        get
        {
            lock (sync)
            {
                return [.. audit];
            }
        }
    }

    /// <summary>
    /// Installs a new secret and keeps the previous one valid for <paramref name="overlap"/>.
    /// </summary>
    public CredentialGeneration Rotate(string newSecret, TimeSpan overlap)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(overlap, TimeSpan.Zero);
        return Install(newSecret, timeProvider.GetUtcNow().Add(overlap), CredentialRotationKind.Scheduled);
    }

    /// <summary>
    /// Installs a new secret and refuses every previous one immediately. Used when a value is
    /// believed to have leaked: an overlap would keep the leaked credential working for exactly as
    /// long as the attacker needs it (P7-5 §6.4).
    /// </summary>
    public CredentialGeneration RotateEmergency(string newSecret) =>
        Install(newSecret, timeProvider.GetUtcNow(), CredentialRotationKind.Emergency);

    /// <summary>
    /// True when <paramref name="supplied"/> matches any generation valid at this instant.
    /// </summary>
    public bool IsAccepted(string? supplied)
    {
        if (string.IsNullOrEmpty(supplied))
        {
            return false;
        }

        // Hashed before comparison, not compared raw. FixedTimeEquals returns false immediately
        // when the two spans differ in length -- that check happens before the constant-time loop
        // -- so comparing the supplied bytes against the stored bytes leaked the length of the
        // real credential through response time, whatever the loop below did. Two SHA-256 digests
        // are always thirty-two bytes, so there is no length to leak.
        byte[] suppliedDigest = SHA256.HashData(Encoding.UTF8.GetBytes(supplied));
        bool accepted = false;
        lock (sync)
        {
            DateTimeOffset now = timeProvider.GetUtcNow();
            foreach (InstalledCredential generation in generations)
            {
                // Every candidate is compared, and the result is accumulated rather than returned
                // early. Short-circuiting on the first match would make the response time reveal
                // WHICH generation matched -- and during an overlap that tells an attacker whether
                // the value they hold is the one being retired.
                bool match = generation.Descriptor.IsValidAt(now)
                    && CryptographicOperations.FixedTimeEquals(
                        suppliedDigest,
                        generation.SecretDigest);
                accepted |= match;
            }
        }

        return accepted;
    }

    private CredentialGeneration Install(
        string secret,
        DateTimeOffset? retireExisting,
        CredentialRotationKind kind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        if (secret.Length < MinimumSecretLength)
        {
            throw new InvalidOperationException(
                $"A rotated credential must be at least {MinimumSecretLength} characters. Below "
                + "that the audit fingerprint becomes guessable, and the trail would leak what it "
                + "describes.");
        }

        byte[] digest = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        lock (sync)
        {
            DateTimeOffset now = timeProvider.GetUtcNow();
            if (generations.Any(entry => entry.Descriptor.IsValidAt(now)
                && CryptographicOperations.FixedTimeEquals(digest, entry.SecretDigest)))
            {
                // Rotating to the value already in use looks like a rotation in the audit trail
                // and changes nothing. That is worse than not rotating, because the record says
                // the exposure was closed.
                throw new InvalidOperationException(
                    "The new credential is already active; rotating to the current value would "
                    + "record an exposure as closed without closing it.");
            }

            if (retireExisting is { } retireAt)
            {
                foreach (InstalledCredential entry in generations)
                {
                    entry.Descriptor = entry.Descriptor with { NotAfter = retireAt };
                }
            }

            var descriptor = new CredentialGeneration(
                nextGeneration++,
                Fingerprint(secret),
                now,
                null);
            generations.Add(new InstalledCredential(descriptor, digest));
            generations.RemoveAll(entry =>
                entry.Descriptor.NotAfter is { } end && end < now.AddDays(-1));

            audit.Add(new CredentialRotationAudit(
                descriptor.Generation,
                kind,
                descriptor.Fingerprint,
                now,
                retireExisting));
            return descriptor;
        }
    }

    /// <summary>
    /// One installed credential: what may be said about it, and what it is checked against.
    /// <para>
    /// The two are separated so the first can be public and the second cannot. Only the digest is
    /// retained — the provider verifies, it never needs to reveal — so the plaintext exists here
    /// no longer than the call that installed it.
    /// </para>
    /// </summary>
    private sealed class InstalledCredential(CredentialGeneration descriptor, byte[] secretDigest)
    {
        public CredentialGeneration Descriptor { get; set; } = descriptor;

        public byte[] SecretDigest { get; } = secretDigest;
    }

    /// <summary>
    /// Short, stable identifier for a secret. Recorded so a rotation can be traced across systems
    /// without the value ever appearing anywhere.
    /// <para>
    /// Derived with a work factor rather than hashed. It was a bare SHA-256 truncated to
    /// forty-eight bits, justified by <see cref="MinimumSecretLength"/> — but a length floor
    /// cannot make a guess expensive, and a configured credential is whatever an operator typed.
    /// Anyone holding a fingerprint could run a dictionary against it at the speed of a hash. At
    /// 210,000 PBKDF2 iterations each candidate costs about 34 ms, so a million-word dictionary
    /// costs roughly nine CPU-hours instead of well under a second.
    /// </para>
    /// <para>
    /// Still truncated to twelve hex characters, because the point is a short handle for humans
    /// tracing one rotation across two systems, and the work factor rather than the width is what
    /// carries the security. Nothing pins a fingerprint value — the tests derive the expected one
    /// through this same method — so this derivation can be revised again if the cost of guessing
    /// falls.
    /// </para>
    /// </summary>
    public static string Fingerprint(string secret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        byte[] derived = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(secret),
            FingerprintSalt,
            FingerprintIterations,
            HashAlgorithmName.SHA256,
            outputLength: 6);
        return Convert.ToHexString(derived).ToLowerInvariant();
    }
}

public enum CredentialRotationKind
{
    Initial,
    Scheduled,
    Emergency,
}

/// <summary>
/// One generation, described without describing the secret.
/// <para>
/// It used to carry the credential as a <c>byte[]</c>. Two things followed. The value was readable
/// through <see cref="RotatingCredentialProvider.ActiveGenerations"/>, which is public — so the
/// type that exists to avoid naming the secret was handing it out. And a positional record with an
/// array member gets reference equality for that member, so two generations holding the same
/// credential compared as different, quietly, wherever equality was used.
/// </para>
/// </summary>
public sealed record CredentialGeneration(
    int Generation,
    string Fingerprint,
    DateTimeOffset NotBefore,
    DateTimeOffset? NotAfter)
{
    public bool IsValidAt(DateTimeOffset instant) =>
        instant >= NotBefore && (NotAfter is null || instant < NotAfter);
}

/// <summary>
/// One rotation, described without describing the secret. Carries a fingerprint and timestamps and
/// nothing else: an audit row that quoted the value would be the leak it exists to record.
/// </summary>
public sealed record CredentialRotationAudit(
    int Generation,
    CredentialRotationKind Kind,
    string Fingerprint,
    DateTimeOffset RotatedAt,
    DateTimeOffset? PreviousRetiredAt);
