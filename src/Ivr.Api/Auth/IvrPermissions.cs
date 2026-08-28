namespace Ivr.Api.Auth;

/// <summary>
/// The permission strings this module still writes down.
/// <para>
/// This is no longer an authorisation catalogue. Endpoints name a tier
/// (<see cref="AdminPolicies"/>), and W-0122 deleted the console accounts these used to be
/// granted to. What survives are the strings some other contract still spells out: the
/// <c>Permission</c> stamped on every admin action, and the wire vocabulary of
/// <c>X-Script-Permissions</c>. Nothing here grants anything.
/// </para>
/// </summary>
public static class IvrPermissions
{
    /// <summary>
    /// Operation names stamped into <c>AdminAction.Permission</c>. <c>InternalAdminApiService</c>
    /// passes one alongside every mutation and refuses to persist an action whose builder chose a
    /// different string, so these still have to match the operation they sit beside.
    /// <para>
    /// <see cref="QueueView"/>, <see cref="FlagRead"/> and <see cref="RuntimeGateAdmin"/> are
    /// stamped on nothing. They survive only as the vocabulary
    /// <c>TestAdminTokens.ScopeForPermission</c> maps to a tier, for tests that vary the
    /// permission as a parameter rather than naming a tier. They go when those tests stop.
    /// </para>
    /// </summary>
    public const string QueueView = "IVR_QUEUE_VIEW";
    public const string QueuePause = "IVR_QUEUE_PAUSE";
    public const string QueueResume = "IVR_QUEUE_RESUME";
    public const string SimEnable = "IVR_SIM_ENABLE";
    public const string SimDisable = "IVR_SIM_DISABLE";
    public const string ManualRetry = "IVR_MANUAL_RETRY";
    public const string ResultReview = "IVR_RESULT_REVIEW";
    public const string FlagRead = "IVR_FLAG_READ";
    public const string RuntimeGateAdmin = "IVR_RUNTIME_GATE_ADMIN";

    /// <summary>
    /// Script lifecycle (W-0109). The wire values of <c>X-Script-Permissions</c>: Module 3 declares
    /// what its actor holds, and <c>ScriptLifecycleApiService</c> maps each one onto the matching
    /// <see cref="Ivr.Domain.Scripts.ScriptPermissions"/> entry so <c>ScriptActor.Demand</c> stays a
    /// real check rather than a formality that always passes.
    /// <para>
    /// Seven strings rather than one "manage scripts" grant. The four-eyes control is that Content
    /// and Privacy/Legal approvals must come from two different actors; collapsing them would erase
    /// that distinction on the wire, where it is now the only place it exists. It also keeps this
    /// list readable against `specs/ui/04`, which names all seven.
    /// </para>
    /// <para>
    /// These values are published in `integration-requirements/06-module-3-api-handover.md` §4A.5.
    /// Renaming one is a breaking change for Module 3, not a local rename.
    /// </para>
    /// </summary>
    public const string ScriptEdit = "IVR_SCRIPT_EDIT";
    public const string ScriptReview = "IVR_SCRIPT_REVIEW";
    public const string ScriptApproveMock = "IVR_SCRIPT_APPROVE_MOCK";
    public const string ScriptApproveLab = "IVR_SCRIPT_APPROVE_LAB";
    public const string ScriptApproveContent = "IVR_SCRIPT_APPROVE_CONTENT";
    public const string ScriptApprovePrivacyLegal = "IVR_SCRIPT_APPROVE_PRIVACY_LEGAL";
    public const string ScriptRetire = "IVR_SCRIPT_RETIRE";

    /// <summary>
    /// Cutting a call that is already in progress (W-0111). Stamped on the admin action like the
    /// queue and SIM operations above; the tier that reaches the endpoint is <c>danger</c>.
    /// </summary>
    public const string CallTerminate = "IVR_CALL_TERMINATE";
}
