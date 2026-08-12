using System.Collections.Frozen;

namespace Ivr.Api.Auth;

public static class IvrPermissions
{
    public const string QueueView = "IVR_QUEUE_VIEW";
    public const string QueuePause = "IVR_QUEUE_PAUSE";
    public const string QueueResume = "IVR_QUEUE_RESUME";
    public const string SimEnable = "IVR_SIM_ENABLE";
    public const string SimDisable = "IVR_SIM_DISABLE";
    public const string ManualRetry = "IVR_MANUAL_RETRY";
    public const string ResultReview = "IVR_RESULT_REVIEW";
    public const string FlagRead = "IVR_FLAG_READ";
    public const string RuntimeGateAdmin = "IVR_RUNTIME_GATE_ADMIN";

    public static IReadOnlySet<string> All { get; } = new[]
        {
            QueueView,
            QueuePause,
            QueueResume,
            SimEnable,
            SimDisable,
            ManualRetry,
            ResultReview,
            FlagRead,
            RuntimeGateAdmin,
        }
        .ToFrozenSet(StringComparer.Ordinal);
}
