namespace Ivr.Domain.Policies;

/// <summary>
/// The only channel IVR ever proposes suppression for (DC-02). It is an enum of one on purpose:
/// IVR observes voice calls and nothing else, so it has no standing to say anything about SMS,
/// email or marketing preferences — those belong to whoever owns that channel.
/// </summary>
public enum SuppressionChannel
{
    PhoneCall,
}

public static class OptOutReasonCodes
{
    /// <summary>Customer actively rejected the call. `DT-02` maps this to NO_ANSWER + review.</summary>
    public const string CallRejected = "REJECTED_REVIEW_REQUIRED";

    public const string BelowThreshold = "OPTOUT_BELOW_THRESHOLD";
    public const string ThresholdReached = "OPTOUT_THRESHOLD_REACHED";
    public const string AdminConfirmed = "OPTOUT_ADMIN_CONFIRMED";
    public const string SingleSignalNeverProposes = "OPTOUT_SINGLE_SIGNAL_HELD";
}

/// <summary>What IVR decided to do about an accumulated opt-out signal.</summary>
public enum SuppressionOutcome
{
    /// <summary>Keep watching. Nothing leaves IVR, nothing is blocked.</summary>
    Hold,

    /// <summary>Propose do-not-call to CRM. Still a proposal — never a block.</summary>
    Propose,
}

public sealed record SuppressionDecision(
    SuppressionOutcome Outcome,
    SuppressionChannel Channel,
    string ReasonCode,
    int SignalCount,
    bool AdminConfirmed)
{
    /// <summary>
    /// Always false. Present so the invariant is a value someone can assert on rather than a
    /// sentence in a document: whatever IVR decides, it never suppresses locally (DO-CORR-2).
    /// The do-not-call registry belongs to CRM; IVR only ever tells them what it saw.
    /// </summary>
    public static bool SuppressedLocally => false;
}

public sealed record OptOutThresholdPolicy(int MinimumSignals)
{
    /// <summary>
    /// A single rejected call is not an opt-out. People decline calls because they are driving,
    /// in a meeting, or do not recognise the number. Suppressing on one signal would silently
    /// remove customers who never asked to be removed, so the floor is two and is enforced here
    /// rather than left to configuration.
    /// </summary>
    public const int AbsoluteFloor = 2;

    public static OptOutThresholdPolicy Default { get; } = new(3);

    public OptOutThresholdPolicy Validated() => MinimumSignals < AbsoluteFloor
        ? throw new InvalidOperationException(
            $"Opt-out threshold must be at least {AbsoluteFloor}: a single declined call is not "
            + "an opt-out, and no configuration may lower that floor.")
        : this;
}

/// <summary>
/// <b>DEAD_BY_OD-V1-23 (W-0304, 2026-09-16). Nothing in production calls this.</b>
/// <para>
/// The eight call sites are all in tests. That is not an oversight to fix by wiring it up: the
/// threshold below infers an opt-out from repeated <c>Rejected</c> calls, and OD-V1-23 closed on
/// 2026-09-10 with the opposite position - V1 is <i>explicit-only</i>, and V1 has no explicit
/// opt-out signal at all. DTMF-0 is the cancel-order key (the locked script says so, and
/// <c>TargetV1SpeechPolicy</c> enforces it); key 9 is out of scope and the same policy rejects any
/// template mentioning it. So there is no signal for this policy to count, and the 2/3 constant is
/// recorded in the register as a gap with no authority behind it, not as a rule.
/// </para>
/// <para>
/// Kept rather than deleted, for three reasons that are each sufficient:
/// (1) <c>deploy/ci/scripts/opt-out-suppression-bundle-validator.mjs</c> pins this file by SHA-256
/// and reads it, so deleting it turns that gate red with no replacement;
/// (2) OPT-01..11 in the W-0187 bundle are pending a quorum that Legal/Privacy and CRM/M3 have not
/// given, and this file is the artifact those decisions are about;
/// (3) the reasoning below - why one declined call is never an opt-out - is the part worth keeping
/// whatever V2 decides.
/// </para>
/// <para>
/// Before calling this from production code, OD-V1-23 must be reopened and OPT-01..11 signed. It is
/// not enough to add a caller.
/// </para>
/// </summary>
public static class OptOutSuppressionPolicy
{
    /// <summary>
    /// Decides whether an accumulated opt-out signal is strong enough to tell CRM about
    /// (W-0034 / P4-6 §6.2). Pure: it reads counts, it does not write, dial, or block.
    /// </summary>
    public static SuppressionDecision Decide(
        int signalCount,
        OptOutThresholdPolicy threshold,
        bool adminConfirmed)
    {
        ArgumentNullException.ThrowIfNull(threshold);
        ArgumentOutOfRangeException.ThrowIfNegative(signalCount);
        OptOutThresholdPolicy effective = threshold.Validated();

        // An administrator who has looked at the case may act before the threshold — that is the
        // point of the review queue. They may not act on nothing, and they may not act on one
        // signal: a human confirming a single declined call is confirming an inference, not a
        // request, and the audit trail would record a decision the customer never made.
        if (adminConfirmed && signalCount >= OptOutThresholdPolicy.AbsoluteFloor)
        {
            return new SuppressionDecision(
                SuppressionOutcome.Propose,
                SuppressionChannel.PhoneCall,
                OptOutReasonCodes.AdminConfirmed,
                signalCount,
                true);
        }

        if (signalCount < OptOutThresholdPolicy.AbsoluteFloor)
        {
            return new SuppressionDecision(
                SuppressionOutcome.Hold,
                SuppressionChannel.PhoneCall,
                OptOutReasonCodes.SingleSignalNeverProposes,
                signalCount,
                adminConfirmed);
        }

        return signalCount >= effective.MinimumSignals
            ? new SuppressionDecision(
                SuppressionOutcome.Propose,
                SuppressionChannel.PhoneCall,
                OptOutReasonCodes.ThresholdReached,
                signalCount,
                false)
            : new SuppressionDecision(
                SuppressionOutcome.Hold,
                SuppressionChannel.PhoneCall,
                OptOutReasonCodes.BelowThreshold,
                signalCount,
                false);
    }
}
