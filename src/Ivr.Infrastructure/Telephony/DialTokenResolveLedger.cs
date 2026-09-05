using System.Collections.Concurrent;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Observability;

namespace Ivr.Infrastructure.Telephony;

/// <summary>
/// W-0198 / <c>OD-V1-17</c> + <c>OD-V1-05</c>. Why a dial token was refused.
/// <para>
/// Each value is safe to log, to put in a technical error code and to show an operator: it names
/// the rule that refused, never the token, the fingerprint or the destination.
/// </para>
/// </summary>
public static class DialTokenRefusalCodes
{
    public const string Expired = "DIAL_TOKEN_EXPIRED";

    /// <summary>The token was first resolved under a different task. It is bound to the first.</summary>
    public const string TaskMismatch = "DIAL_TOKEN_TASK_MISMATCH";

    /// <summary>This attempt already resolved. Retrying an attempt is not a new dial.</summary>
    public const string AttemptReplay = "DIAL_TOKEN_ATTEMPT_REPLAY";

    /// <summary>The policy ceiling is used up. This is what replaced "one-use".</summary>
    public const string ResolveLimitExceeded = "DIAL_TOKEN_RESOLVE_LIMIT_EXCEEDED";

    /// <summary>The caller did not state a ceiling, so there is nothing to enforce.</summary>
    public const string CeilingMissing = "DIAL_TOKEN_RESOLVE_CEILING_MISSING";
}

/// <param name="ResolveCount">
/// Resolves already recorded against this token, including the one just allowed. Reported so an
/// audit row can say "2 of 3" rather than only "allowed".
/// </param>
public sealed record DialTokenResolveDecision(
    bool Allowed,
    string? RefusalCode,
    int ResolveCount,
    int MaxResolves);

/// <summary>
/// Enforces the reusable-dial-token contract signed on 2026-09-05
/// (<c>OD-V1-17</c> chose option (d), <c>OD-V1-05</c> spelled out the resolve rules).
/// <para>
/// Five documents called the token "one-use per attempt", but policy needs at least two customer
/// dials plus technical retries and no contract anywhere has a re-issue endpoint - so "one-use"
/// was never a rule the system could keep. It is replaced here by a <b>ceiling</b>, which keeps
/// the property that mattered: a leaked token still cannot dial more times than policy allows.
/// </para>
/// <para>
/// The ceiling arrives with the request rather than living here, because it is a policy number
/// (<c>max_customer_attempts</c> plus the technical-retry limit) and this class is bookkeeping. A
/// request that fails to state one is refused rather than treated as unlimited - a missing ceiling
/// is the one input where guessing would quietly remove the whole control.
/// </para>
/// <para>
/// Process-local, like the vaults that own it. That is a fidelity gap against a shared vault and
/// not a safety property: it bounds one worker's resolves, and the real bound in production comes
/// from the SIM vault. Recorded here so nobody reads a green test as the stronger claim.
/// </para>
/// </summary>
public sealed class DialTokenResolveLedger
{
    private sealed class TokenLedgerEntry
    {
        public string? TaskId { get; set; }

        public HashSet<string> Attempts { get; } = new(StringComparer.Ordinal);
    }

    private readonly ConcurrentDictionary<string, TokenLedgerEntry> entries =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Decides, and records the resolve when it is allowed. A refusal records nothing: a token
    /// refused for the wrong task must not consume the budget of the task it does belong to.
    /// </summary>
    public DialTokenResolveDecision Evaluate(
        DialTokenResolutionRequest request,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.DialToken.ExpiresAt <= now)
        {
            return new DialTokenResolveDecision(
                false,
                DialTokenRefusalCodes.Expired,
                0,
                request.MaxResolves);
        }

        if (request.MaxResolves <= 0)
        {
            return new DialTokenResolveDecision(
                false,
                DialTokenRefusalCodes.CeilingMissing,
                0,
                request.MaxResolves);
        }

        string fingerprint = request.DialToken.RevealToTrustedResolver();
        TokenLedgerEntry entry = entries.GetOrAdd(fingerprint, _ => new TokenLedgerEntry());

        // One lock per token rather than one for the ledger: two attempts on two different tasks
        // are genuinely independent, and the scheduler dials several channels at once.
        lock (entry)
        {
            if (entry.TaskId is null)
            {
                entry.TaskId = request.TaskId.Value;
            }
            else if (!string.Equals(entry.TaskId, request.TaskId.Value, StringComparison.Ordinal))
            {
                // Binding is to the FIRST task that used the token, which is the one Sales issued
                // it for. A token that turns up under a second task is either a mix-up in the
                // caller or a replay, and neither is a call worth placing.
                return new DialTokenResolveDecision(
                    false,
                    DialTokenRefusalCodes.TaskMismatch,
                    entry.Attempts.Count,
                    request.MaxResolves);
            }

            // Replay is checked before the ceiling so a repeated attempt reads as a replay even
            // when the budget happens to be spent. The two refusals want different responses:
            // one is a caller bug, the other is policy working as designed.
            if (entry.Attempts.Contains(request.AttemptId.Value))
            {
                return new DialTokenResolveDecision(
                    false,
                    DialTokenRefusalCodes.AttemptReplay,
                    entry.Attempts.Count,
                    request.MaxResolves);
            }

            if (entry.Attempts.Count >= request.MaxResolves)
            {
                return new DialTokenResolveDecision(
                    false,
                    DialTokenRefusalCodes.ResolveLimitExceeded,
                    entry.Attempts.Count,
                    request.MaxResolves);
            }

            entry.Attempts.Add(request.AttemptId.Value);
            return new DialTokenResolveDecision(
                true,
                null,
                entry.Attempts.Count,
                request.MaxResolves);
        }
    }
}

/// <summary>
/// A dial token was refused. Carries the rule that refused it so the dispatch loop can put that
/// rule into the technical error code instead of a generic "token rejected" - <c>OD-V1-05</c> asks
/// that an over-limit or expired resolve opens a review rather than passing silently, and a code
/// nobody can tell apart is the silent version.
/// </summary>
public sealed class DialTokenRefusedException : InvalidOperationException
{
    public DialTokenRefusedException(string refusalCode, string message)
        : base(message) => RefusalCode = refusalCode;

    public DialTokenRefusedException()
        : this(DialTokenRefusalCodes.CeilingMissing, "The dial token was refused.")
    {
    }

    public DialTokenRefusedException(string message)
        : this(DialTokenRefusalCodes.CeilingMissing, message)
    {
    }

    public DialTokenRefusedException(string message, Exception innerException)
        : base(message, innerException) => RefusalCode = DialTokenRefusalCodes.CeilingMissing;

    public string RefusalCode { get; } = DialTokenRefusalCodes.CeilingMissing;
}

/// <summary>
/// W-0198 / <c>OD-V1-05</c>. Every resolve leaves a record, allowed or refused.
/// <para>
/// The signed wording was "audit each resolve with the attempt id, and a refusal opens a review
/// rather than passing silently". A refused resolve is the interesting half - it is either a
/// caller mixing tokens up or a leaked token being spent - so it also moves the fail-closed
/// counter, which is what an alert can actually watch.
/// </para>
/// <para>
/// Never carries the token, the fingerprint or the destination. Task and attempt ids are already
/// audit-safe identifiers; the token is the thing that dials.
/// </para>
/// </summary>
public static class DialTokenResolveAudit
{
    public const string Action = "DIAL_TOKEN_RESOLVED";

    public static async ValueTask RecordAsync(
        IAuditLogger? auditLogger,
        string vaultName,
        DialTokenResolutionRequest request,
        DialTokenResolveDecision decision,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(decision);

        if (!decision.Allowed)
        {
            IvrTelemetry.RecordFailClosed(
                (TelemetryTags.ReasonCode, decision.RefusalCode));
        }

        if (auditLogger is null)
        {
            return;
        }

        await auditLogger.AppendAsync(
            new AuditEvent(
                vaultName,
                Action,
                request.TaskId.Value,
                decision.RefusalCode ?? "ALLOWED",
                request.AttemptId.Value,
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["attempt_id"] = request.AttemptId.Value,
                    ["allowed"] = decision.Allowed,
                    ["resolve_count"] = decision.ResolveCount,
                    ["max_resolves"] = decision.MaxResolves,
                }),
            cancellationToken).ConfigureAwait(false);
    }
}
