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
