using Ivr.Api.Auth;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0122. A script-lifecycle actor, replacing the console account a test used to sign in as.
/// <para>
/// The segregation of duties these tests exist for is unchanged: an author still cannot approve
/// its own draft, and Content and Privacy/Legal still cannot be the same person. What moved is
/// only where the approvals come from — Module 3 owns operator identity now and asserts them per
/// request, so a test asserts them the same way.
/// </para>
/// </summary>
internal sealed record ScriptTestActor(string ActorId, string Permissions);

/// <summary>
/// Approval sets, spelled out rather than derived from a role.
/// <para>
/// The console had two roles and every Admin carried every approval, which is exactly what made
/// "three distinct people" hard to express. Naming the set per actor makes the requirement
/// visible in the test rather than implied by a role table somewhere else.
/// </para>
/// </summary>
internal static class ScriptPermissionSets
{
    /// <summary>
    /// Every script approval. All three admin actors carry the same set, exactly as the three
    /// Admin accounts did before.
    /// <para>
    /// The segregation these tests exist for has never come from permissions — it comes from
    /// identity. The domain refuses to let the actor who authored a draft sign it, and refuses to
    /// let one actor hold both production approvals. Giving the actors different permission sets
    /// would test a rule that does not exist and hide the one that does.
    /// </para>
    /// </summary>
    public const string Full =
        IvrPermissions.ScriptEdit + ","
        + IvrPermissions.ScriptReview + ","
        + IvrPermissions.ScriptApproveMock + ","
        + IvrPermissions.ScriptApproveLab + ","
        + IvrPermissions.ScriptApproveContent + ","
        + IvrPermissions.ScriptApprovePrivacyLegal + ","
        + IvrPermissions.ScriptRetire;

    /// <summary>Holds no script approval at all — the operator, and the negative case.</summary>
    public const string None = "";
}
