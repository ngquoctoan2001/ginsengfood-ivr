using System.Collections.Frozen;

namespace Ivr.Infrastructure.FeatureFlags;

public static class FeatureFlagEnvironments
{
    public const string Development = "dev";
    public const string Staging = "staging";
    public const string Lab = "lab";
    public const string Pilot = "pilot";
    public const string Production = "prod";

    public static IReadOnlySet<string> All { get; } = new[]
        {
            Development,
            Staging,
            Lab,
            Pilot,
            Production,
        }
        .ToFrozenSet(StringComparer.Ordinal);
}

public static class FeatureFlagKeys
{
    public const string ExecutionMode = "executionMode";
    public const string SalesProvider = "salesProvider";
    public const string SimProvider = "simProvider";
    public const string AttemptPolicyVersion = "attemptPolicyVersion";
    public const string RealCustomerCallAllowed = "realCustomerCallAllowed";
    public const string LabDestinationAllowlist = "labDestinationAllowlist";
    public const string GlobalDialKillSwitch = "globalDialKillSwitch";
    public const string V1NotificationEnabled = "v1NotificationEnabled";
    public const string RecordingEnabled = "recordingEnabled";

    public static IReadOnlySet<string> All { get; } = new[]
        {
            ExecutionMode,
            SalesProvider,
            SimProvider,
            AttemptPolicyVersion,
            RealCustomerCallAllowed,
            LabDestinationAllowlist,
            GlobalDialKillSwitch,
            V1NotificationEnabled,
            RecordingEnabled,
        }
        .ToFrozenSet(StringComparer.Ordinal);
}

public static class FeatureFlagValues
{
    public const string FakeTargetV1 = "FAKE_TARGET_V1";
    public const string CurrentGoldenHourCompat = "CURRENT_GOLDEN_HOUR_COMPAT";
    public const string TargetV1 = "TARGET_V1";
    public const string MockSimProvider = "MOCK";
    public const string VendorSimProvider = "VENDOR";
    public const string MockLabAttemptPolicy = "mock-lab-v1";
    public const string UnapprovedAttemptPolicy = "UNAPPROVED";
}

public sealed record FeatureFlagSnapshot(
    string Environment,
    long Revision,
    string ExecutionMode,
    string SalesProvider,
    string SimProvider,
    string AttemptPolicyVersion,
    bool RealCustomerCallAllowed,
    IReadOnlySet<string> LabDestinationAllowlist,
    bool GlobalDialKillSwitch,
    bool V1NotificationEnabled,
    bool RecordingEnabled)
{
    public static FeatureFlagSnapshot SafeDefault(string environment)
    {
        if (!FeatureFlagEnvironments.All.Contains(environment))
        {
            throw new ArgumentOutOfRangeException(nameof(environment), environment, "Unknown environment.");
        }

        return new FeatureFlagSnapshot(
            environment,
            0,
            Configuration.IvrOptions.MockExecutionMode,
            FeatureFlagValues.FakeTargetV1,
            FeatureFlagValues.MockSimProvider,
            FeatureFlagValues.MockLabAttemptPolicy,
            false,
            FrozenSet<string>.Empty,
            true,
            false,
            false);
    }

    public FeatureFlagSnapshot Apply(FeatureFlagChangeSet changes)
    {
        ArgumentNullException.ThrowIfNull(changes);
        return this with
        {
            Revision = checked(Revision + 1),
            ExecutionMode = changes.ExecutionMode ?? ExecutionMode,
            SalesProvider = changes.SalesProvider ?? SalesProvider,
            SimProvider = changes.SimProvider ?? SimProvider,
            AttemptPolicyVersion = changes.AttemptPolicyVersion ?? AttemptPolicyVersion,
            RealCustomerCallAllowed = changes.RealCustomerCallAllowed ?? RealCustomerCallAllowed,
            LabDestinationAllowlist = changes.LabDestinationAllowlist is null
                ? LabDestinationAllowlist
                : changes.LabDestinationAllowlist.ToFrozenSet(StringComparer.Ordinal),
            GlobalDialKillSwitch = changes.GlobalDialKillSwitch ?? GlobalDialKillSwitch,
            V1NotificationEnabled = changes.V1NotificationEnabled ?? V1NotificationEnabled,
            RecordingEnabled = changes.RecordingEnabled ?? RecordingEnabled,
        };
    }
}

public sealed record FeatureFlagChangeSet(
    string? ExecutionMode = null,
    string? SalesProvider = null,
    string? SimProvider = null,
    string? AttemptPolicyVersion = null,
    bool? RealCustomerCallAllowed = null,
    IReadOnlyCollection<string>? LabDestinationAllowlist = null,
    bool? GlobalDialKillSwitch = null,
    bool? V1NotificationEnabled = null,
    bool? RecordingEnabled = null)
{
    public IReadOnlySet<string> ChangedKeys()
    {
        List<string> keys = [];
        AddIfPresent(ExecutionMode, FeatureFlagKeys.ExecutionMode, keys);
        AddIfPresent(SalesProvider, FeatureFlagKeys.SalesProvider, keys);
        AddIfPresent(SimProvider, FeatureFlagKeys.SimProvider, keys);
        AddIfPresent(AttemptPolicyVersion, FeatureFlagKeys.AttemptPolicyVersion, keys);
        AddIfPresent(RealCustomerCallAllowed, FeatureFlagKeys.RealCustomerCallAllowed, keys);
        AddIfPresent(LabDestinationAllowlist, FeatureFlagKeys.LabDestinationAllowlist, keys);
        AddIfPresent(GlobalDialKillSwitch, FeatureFlagKeys.GlobalDialKillSwitch, keys);
        AddIfPresent(V1NotificationEnabled, FeatureFlagKeys.V1NotificationEnabled, keys);
        AddIfPresent(RecordingEnabled, FeatureFlagKeys.RecordingEnabled, keys);
        return keys.ToFrozenSet(StringComparer.Ordinal);
    }

    private static void AddIfPresent<T>(T? value, string key, List<string> keys)
    {
        if (value is not null)
        {
            keys.Add(key);
        }
    }
}

public sealed record FeatureFlagReadResult(
    FeatureFlagSnapshot Snapshot,
    bool ProviderReadable,
    bool FromCache);
