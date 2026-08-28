using Ivr.Api.Auth;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0122. Fixed admin service credentials for tests, replacing the console sign-in that used to
/// stand in front of every admin call.
/// <para>
/// Tests used to create an account, sign in and carry a session token. With operator identity now
/// owned by Module 3 there is nothing to sign into, so a test presents the tier it needs the same
/// way Module 3 will: a bearer token plus the matching scope header.
/// </para>
/// </summary>
internal static class TestAdminTokens
{
    public const string Read = "test-admin-read-token-not-a-real-secret";
    public const string Write = "test-admin-write-token-not-a-real-secret";
    public const string Danger = "test-admin-danger-token-not-a-real-secret";

    public const string DefaultActor = "test-operator";

    /// <summary>Configuration entries a test host needs so the three tiers resolve.</summary>
    public static IEnumerable<KeyValuePair<string, string?>> ConfigurationEntries()
    {
        yield return new(AdminAccessOptions.ReadTokenConfigurationKey, Read);
        yield return new(AdminAccessOptions.WriteTokenConfigurationKey, Write);
        yield return new(AdminAccessOptions.DangerTokenConfigurationKey, Danger);
    }

    public static string TokenFor(AdminScope scope) => scope switch
    {
        AdminScope.Read => Read,
        AdminScope.Write => Write,
        AdminScope.Danger => Danger,
        _ => throw new ArgumentOutOfRangeException(nameof(scope)),
    };

    /// <summary>
    /// Maps a legacy permission name to the tier that now covers it, for tests that vary the
    /// permission as a parameter rather than naming one.
    /// </summary>
    public static AdminScope ScopeForPermission(string permission) => permission switch
    {
        IvrPermissions.RuntimeGateAdmin or IvrPermissions.CallTerminate
            or IvrPermissions.SimDisable or IvrPermissions.SimEnable
            or IvrPermissions.ManualRetry or IvrPermissions.QueuePause
            or IvrPermissions.QueueResume => AdminScope.Danger,
        IvrPermissions.QueueView or IvrPermissions.FlagRead => AdminScope.Read,
        _ => AdminScope.Write,
    };

    public static void AuthorizeForPermission(
        HttpRequestMessage request,
        string permission,
        string actorId = DefaultActor) =>
        Authorize(request, ScopeForPermission(permission), actorId);

    /// <summary>Applies the tier's credential and headers to an outgoing test request.</summary>
    public static void Authorize(
        HttpRequestMessage request,
        AdminScope scope,
        string actorId = DefaultActor,
        string? reason = "Acceptance rehearsal")
    {
        ArgumentNullException.ThrowIfNull(request);
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenFor(scope));
        request.Headers.Add(AdminScopeGuard.ScopeHeaderName, AdminScopeGuard.ScopeValueOf(scope));
        request.Headers.Add(AdminScopeGuard.ActorHeaderName, actorId);
        if (scope == AdminScope.Danger && reason is not null)
        {
            request.Headers.Add(AdminScopeGuard.ReasonHeaderName, reason);
        }
    }
}
