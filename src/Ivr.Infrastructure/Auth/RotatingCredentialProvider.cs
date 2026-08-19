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
    /// Below this length a truncated fingerprint is guessable by brute force, which would turn the
    /// audit trail into an oracle for the secret it is supposed to describe. The minimum is what
    /// makes <see cref="Fingerprint"/> safe to record.
    /// </summary>
    public const int MinimumSecretLength = 24;

    private readonly object sync = new();
    private readonly TimeProvider timeProvider;
    private readonly List<CredentialGeneration> generations = [];
    private readonly List<CredentialRotationAudit> audit = [];
    private int nextGeneration = 1;

    public RotatingCredentialProvider(string initialSecret, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        this.timeProvider = timeProvider;
        Install(initialSecret, retireExisting: null, CredentialRotationKind.Initial);
    }

    /// <summary>Generations that would be accepted right now, newest first.</summary>
    public IReadOnlyList<CredentialGeneration> ActiveGenerations
    {
        get
        {
            lock (sync)
            {
                DateTimeOffset now = timeProvider.GetUtcNow();
                return [.. generations.Where(entry => entry.IsValidAt(now)).OrderByDescending(entry => entry.Generation)];
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

        byte[] suppliedBytes = Encoding.UTF8.GetBytes(supplied);
        bool accepted = false;
        lock (sync)
        {
            DateTimeOffset now = timeProvider.GetUtcNow();
            foreach (CredentialGeneration generation in generations)
            {
                // Every candidate is compared, and the result is accumulated rather than returned
                // early. Short-circuiting on the first match would make the response time reveal
                // WHICH generation matched -- and during an overlap that tells an attacker whether
                // the value they hold is the one being retired.
                bool match = generation.IsValidAt(now)
                    && CryptographicOperations.FixedTimeEquals(suppliedBytes, generation.SecretBytes);
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

        lock (sync)
        {
            DateTimeOffset now = timeProvider.GetUtcNow();
            if (generations.Any(entry => entry.IsValidAt(now)
                && CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(secret),
                    entry.SecretBytes)))
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
                for (int index = 0; index < generations.Count; index++)
                {
                    generations[index] = generations[index] with { NotAfter = retireAt };
                }
            }

            var installed = new CredentialGeneration(
                nextGeneration++,
                Encoding.UTF8.GetBytes(secret),
                Fingerprint(secret),
                now,
                null);
            generations.Add(installed);
            generations.RemoveAll(entry => entry.NotAfter is { } end && end < now.AddDays(-1));

            audit.Add(new CredentialRotationAudit(
                installed.Generation,
                kind,
                installed.Fingerprint,
                now,
                retireExisting));
            return installed;
        }
    }

    /// <summary>
    /// Short, stable identifier for a secret. Recorded so a rotation can be traced across systems
    /// without the value ever appearing anywhere; safe only because
    /// <see cref="MinimumSecretLength"/> is enforced above.
    /// </summary>
    public static string Fingerprint(string secret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(secret)))[..12].ToLowerInvariant();
    }
}

public enum CredentialRotationKind
{
    Initial,
    Scheduled,
    Emergency,
}

public sealed record CredentialGeneration(
    int Generation,
    byte[] SecretBytes,
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
