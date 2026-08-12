using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Configuration;

namespace Ivr.Infrastructure.FeatureFlags;

public static class FeatureFlagGuardrails
{
    public static void ValidateAdminMutation(
        FeatureFlagSnapshot before,
        FeatureFlagSnapshot after)
    {
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(after);

        if (!before.RealCustomerCallAllowed && after.RealCustomerCallAllowed)
        {
            throw IvrErrors.PolicyMismatch(
                "realCustomerCallAllowed can only be enabled by the approved release workflow.");
        }

        if (!before.V1NotificationEnabled && after.V1NotificationEnabled)
        {
            throw IvrErrors.PolicyMismatch("V1 notifications are immutable-off.");
        }

        if (!before.RecordingEnabled && after.RecordingEnabled)
        {
            throw IvrErrors.PolicyMismatch("Call recording is immutable-off.");
        }
    }

    public static void ValidateEffective(FeatureFlagSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (!FeatureFlagEnvironments.All.Contains(snapshot.Environment))
        {
            throw IvrErrors.PolicyMismatch("Feature flag environment is invalid.");
        }

        Require(snapshot.ExecutionMode, FeatureFlagKeys.ExecutionMode);
        Require(snapshot.SalesProvider, FeatureFlagKeys.SalesProvider);
        Require(snapshot.SimProvider, FeatureFlagKeys.SimProvider);
        Require(snapshot.AttemptPolicyVersion, FeatureFlagKeys.AttemptPolicyVersion);
        foreach (string destinationReference in snapshot.LabDestinationAllowlist)
        {
            Require(destinationReference, FeatureFlagKeys.LabDestinationAllowlist);
            PiiGuard.EnsureSafeText(destinationReference);
        }

        if (snapshot.V1NotificationEnabled || snapshot.RecordingEnabled)
        {
            throw IvrErrors.PolicyMismatch(
                "Notification and recording controls must remain disabled.");
        }

        if ((snapshot.Environment is FeatureFlagEnvironments.Development
                or FeatureFlagEnvironments.Staging)
            && (!snapshot.GlobalDialKillSwitch
                || snapshot.LabDestinationAllowlist.Count > 0))
        {
            throw IvrErrors.PolicyMismatch(
                "Lower environments must keep the dial kill switch on and allowlist empty.");
        }

        switch (snapshot.ExecutionMode)
        {
            case IvrOptions.MockExecutionMode:
                RequirePair(
                    snapshot,
                    FeatureFlagValues.FakeTargetV1,
                    FeatureFlagValues.MockSimProvider,
                    realCustomerCallAllowed: false);
                break;
            case IvrOptions.LabRealSimExecutionMode:
                if (snapshot.Environment is not FeatureFlagEnvironments.Lab
                    and not FeatureFlagEnvironments.Pilot)
                {
                    throw IvrErrors.PolicyMismatch(
                        "LAB_REAL_SIM is only valid in lab or pilot.");
                }

                RequirePair(
                    snapshot,
                    FeatureFlagValues.FakeTargetV1,
                    FeatureFlagValues.VendorSimProvider,
                    realCustomerCallAllowed: false);
                break;
            case IvrOptions.ProductionRealExecutionMode:
                if (snapshot.Environment != FeatureFlagEnvironments.Production)
                {
                    throw IvrErrors.PolicyMismatch(
                        "PRODUCTION_REAL is only valid in production.");
                }

                RequirePair(
                    snapshot,
                    FeatureFlagValues.TargetV1,
                    FeatureFlagValues.VendorSimProvider,
                    realCustomerCallAllowed: true);
                if (snapshot.AttemptPolicyVersion is FeatureFlagValues.MockLabAttemptPolicy
                    or FeatureFlagValues.UnapprovedAttemptPolicy)
                {
                    throw IvrErrors.PolicyMismatch(
                        "Production calling requires an approved attempt policy.");
                }

                break;
            default:
                throw IvrErrors.PolicyMismatch("Execution mode is not supported.");
        }

        if (snapshot.SalesProvider == FeatureFlagValues.CurrentGoldenHourCompat)
        {
            throw IvrErrors.PolicyMismatch(
                "Compatibility state cannot be used as runtime business truth.");
        }
    }

    public static FeatureFlagRiskAssessment AssessRisk(
        FeatureFlagSnapshot before,
        FeatureFlagSnapshot after)
    {
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(after);
        HashSet<string> increases = new(StringComparer.Ordinal);
        if (ModeRank(after.ExecutionMode) > ModeRank(before.ExecutionMode))
        {
            increases.Add(FeatureFlagKeys.ExecutionMode);
        }

        if (ProviderRank(after.SalesProvider) > ProviderRank(before.SalesProvider))
        {
            increases.Add(FeatureFlagKeys.SalesProvider);
        }

        if (before.SimProvider == FeatureFlagValues.MockSimProvider
            && after.SimProvider == FeatureFlagValues.VendorSimProvider)
        {
            increases.Add(FeatureFlagKeys.SimProvider);
        }

        if (before.GlobalDialKillSwitch && !after.GlobalDialKillSwitch)
        {
            increases.Add(FeatureFlagKeys.GlobalDialKillSwitch);
        }

        if (!before.RealCustomerCallAllowed && after.RealCustomerCallAllowed)
        {
            increases.Add(FeatureFlagKeys.RealCustomerCallAllowed);
        }

        if (after.LabDestinationAllowlist.Except(
                before.LabDestinationAllowlist,
                StringComparer.Ordinal).Any())
        {
            increases.Add(FeatureFlagKeys.LabDestinationAllowlist);
        }

        if (!string.Equals(
                before.AttemptPolicyVersion,
                after.AttemptPolicyVersion,
                StringComparison.Ordinal))
        {
            increases.Add(FeatureFlagKeys.AttemptPolicyVersion);
        }

        return new FeatureFlagRiskAssessment(increases);
    }

    public static bool IsUnconditionalRiskReduction(
        FeatureFlagSnapshot before,
        FeatureFlagSnapshot after,
        IReadOnlySet<string> changedKeys)
    {
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(after);
        ArgumentNullException.ThrowIfNull(changedKeys);
        if (changedKeys.Count != 1)
        {
            return false;
        }

        string key = changedKeys.Single();
        return key switch
        {
            FeatureFlagKeys.GlobalDialKillSwitch =>
                after.GlobalDialKillSwitch,
            FeatureFlagKeys.RealCustomerCallAllowed =>
                !after.RealCustomerCallAllowed,
            FeatureFlagKeys.LabDestinationAllowlist =>
                after.LabDestinationAllowlist.IsSubsetOf(
                    before.LabDestinationAllowlist),
            _ => false,
        };
    }

    private static void RequirePair(
        FeatureFlagSnapshot snapshot,
        string salesProvider,
        string simProvider,
        bool realCustomerCallAllowed)
    {
        if (snapshot.SalesProvider != salesProvider
            || snapshot.SimProvider != simProvider
            || snapshot.RealCustomerCallAllowed != realCustomerCallAllowed)
        {
            throw IvrErrors.PolicyMismatch("Execution mode and provider flags are incompatible.");
        }
    }

    private static void Require(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw IvrErrors.MalformedRequest($"{field} is required.");
        }
    }

    private static int ModeRank(string mode) => mode switch
    {
        IvrOptions.MockExecutionMode => 0,
        IvrOptions.LabRealSimExecutionMode => 1,
        IvrOptions.ProductionRealExecutionMode => 2,
        _ => int.MaxValue,
    };

    private static int ProviderRank(string provider) => provider switch
    {
        FeatureFlagValues.FakeTargetV1 => 0,
        FeatureFlagValues.CurrentGoldenHourCompat => 1,
        FeatureFlagValues.TargetV1 => 2,
        _ => int.MaxValue,
    };
}

public sealed record FeatureFlagRiskAssessment(IReadOnlySet<string> IncreasedRiskKeys)
{
    public bool IsRiskIncrease => IncreasedRiskKeys.Count > 0;
}
