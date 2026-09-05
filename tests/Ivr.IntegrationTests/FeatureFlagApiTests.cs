using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ivr.Api.Admin;
using Ivr.Api.Auth;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

public sealed class FeatureFlagApiTests
{
    [Fact]
    [Trait("TestId", "UT-FLAG-AUTHZ-05")]
    public async Task MutationWithoutExplicitRuntimeGatePermissionIsForbidden()
    {
        await using FeatureFlagApiTestApplication app =
            await FeatureFlagApiTestApplication.StartAsync();

        using HttpResponseMessage response = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Lab,
            new FeatureFlagChangeSet(GlobalDialKillSwitch: true),
            "emergency stop",
            permissions: IvrPermissions.FlagRead);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(
            IvrErrorCodes.ForbiddenCaller,
            await ReadErrorCodeAsync(response));
    }

    /// <summary>
    /// W-0192 changed which direction this gate blocks, and the change is the point.
    /// <para>
    /// It used to assert that an ungranted owner permission refused <em>engaging</em> the kill
    /// switch. That was the behaviour, and it was wrong: <c>W-0068</c> settled that stopping calls
    /// must always be available, so an ungranted permission was removing the one control that
    /// exists for an emergency. The gate now stands where the risk is — on the change that lets
    /// calls resume.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-FLAG-OWNERGATE-12")]
    public async Task UnapprovedOwnerPermissionBlocksResumingCallsButNeverStoppingThem()
    {
        await using FeatureFlagApiTestApplication app =
            await FeatureFlagApiTestApplication.StartAsync(
                runtimeAuthorizationApproved: false);

        using (HttpResponseMessage engaged = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Lab,
            new FeatureFlagChangeSet(GlobalDialKillSwitch: true),
            "emergency stop"))
        {
            Assert.Equal(HttpStatusCode.OK, engaged.StatusCode);
        }

        Assert.NotEmpty(app.Audit.Entries);
        int auditedAfterStop = app.Audit.Entries.Count;

        using HttpResponseMessage resumed = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Lab,
            new FeatureFlagChangeSet(GlobalDialKillSwitch: false),
            "resume dialling");

        Assert.Equal(HttpStatusCode.Conflict, resumed.StatusCode);
        Assert.Equal(IvrErrorCodes.OperationalBlocked, await ReadErrorCodeAsync(resumed));

        // Refused before anything was written, so the refusal cannot be mistaken later for a
        // change that happened.
        Assert.Equal(auditedAfterStop, app.Audit.Entries.Count);
    }

    [Fact]
    [Trait("TestId", "IT-FLAG-PRODGUARD-07")]
    public async Task ProductionAllowsRiskReductionAndRejectsRiskIncreaseAndImmutableFlags()
    {
        FeatureFlagSnapshot seed = ProductionSeed(
            globalDialKillSwitch: false,
            Set("prod-destination-a"));
        await using FeatureFlagApiTestApplication app =
            await FeatureFlagApiTestApplication.StartAsync([seed]);

        using HttpResponseMessage killOn = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Production,
            new FeatureFlagChangeSet(GlobalDialKillSwitch: true),
            "production emergency stop");
        Assert.Equal(HttpStatusCode.OK, killOn.StatusCode);

        using HttpResponseMessage killOff = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Production,
            new FeatureFlagChangeSet(GlobalDialKillSwitch: false),
            "resume production calls",
            approvalReference: "approval-1");
        await AssertPolicyMismatchAsync(killOff);

        using HttpResponseMessage narrow = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Production,
            new FeatureFlagChangeSet(LabDestinationAllowlist: []),
            "remove production destination");
        Assert.Equal(HttpStatusCode.OK, narrow.StatusCode);

        using HttpResponseMessage expand = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Production,
            new FeatureFlagChangeSet(
                LabDestinationAllowlist: ["prod-destination-b"]),
            "add production destination",
            approvalReference: "approval-1");
        await AssertPolicyMismatchAsync(expand);

        using HttpResponseMessage notifications = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Production,
            new FeatureFlagChangeSet(V1NotificationEnabled: true),
            "enable notifications",
            approvalReference: "approval-1");
        await AssertPolicyMismatchAsync(notifications);

        using HttpResponseMessage recording = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Production,
            new FeatureFlagChangeSet(RecordingEnabled: true),
            "enable recording",
            approvalReference: "approval-1");
        await AssertPolicyMismatchAsync(recording);
    }

    [Fact]
    [Trait("TestId", "IT-FLAG-EMERGENCY-10")]
    public async Task OnCallCanEnableProductionKillSwitchWithoutFourEyesAndActionIsAudited()
    {
        FeatureFlagSnapshot preExistingInvalidState =
            ProductionSeed(false, Set("prod-destination-a")) with
            {
                V1NotificationEnabled = true,
            };
        await using FeatureFlagApiTestApplication app =
            await FeatureFlagApiTestApplication.StartAsync([preExistingInvalidState]);

        using HttpResponseMessage response = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Production,
            new FeatureFlagChangeSet(GlobalDialKillSwitch: true),
            "incident emergency stop",
            approvalReference: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AuditLogEntry entry = Assert.Single(app.Audit.Entries);
        Assert.Equal("operator-1", entry.Actor);
        Assert.Equal("incident emergency stop", entry.Reason);
        Assert.Contains("GlobalDialKillSwitch", entry.DataJson, StringComparison.Ordinal);
        Assert.True((await app.Store.ReadFreshAsync(
            FeatureFlagEnvironments.Production)).GlobalDialKillSwitch);
    }

    /// <summary>
    /// The four-eyes requirement outside production (W-0110).
    /// <para>
    /// <c>IT-FLAG-PRODGUARD-07</c> proves production refuses a risk increase outright. This is
    /// the other half: in LAB, where a risk increase <em>is</em> reachable, it is reachable only
    /// with a verified approval. Without that, "four-eyes is enforced" rests on a test that
    /// never lets the change through in the first place — which would pass just as well if the
    /// approval check did not exist.
    /// </para>
    /// <para>
    /// It is the layer under the console form added in W-0110. The form refuses to submit a
    /// risk increase with no approval reference; this is what happens when something bypasses
    /// the form.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-FLAG-FOUREYES-14")]
    public async Task LabRiskIncreaseWithoutAnApprovalReferenceIsRefused()
    {
        await using FeatureFlagApiTestApplication app =
            await FeatureFlagApiTestApplication.StartAsync(
                [LabRealSeed(true, Set("lab-destination-a"))]);

        // Reducing risk needs nothing but a reason, and still works.
        using HttpResponseMessage narrow = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Lab,
            new FeatureFlagChangeSet(LabDestinationAllowlist: []),
            "shrink the lab allowlist");
        Assert.Equal(HttpStatusCode.OK, narrow.StatusCode);

        // Raising it without an approval reference is refused.
        using HttpResponseMessage releaseWithout = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Lab,
            new FeatureFlagChangeSet(GlobalDialKillSwitch: false),
            "resume lab dialling");
        await AssertPolicyMismatchAsync(releaseWithout);

        using HttpResponseMessage widenWithout = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Lab,
            new FeatureFlagChangeSet(LabDestinationAllowlist: ["lab-destination-b"]),
            "add a lab destination");
        await AssertPolicyMismatchAsync(widenWithout);

        // And the kill switch is still engaged afterwards: a refused mutation must not have
        // moved the gate part of the way.
        using HttpResponseMessage verified = await SendReadAsync(
            app,
            FeatureFlagEnvironments.Lab);
        Assert.Equal(HttpStatusCode.OK, verified.StatusCode);
        Assert.Contains(
            "\"globalDialKillSwitch\":true",
            await verified.Content.ReadAsStringAsync(),
            StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "IT-FLAG-KILLSWITCH-08")]
    public async Task KillSwitchImmediatelyBlocksOtherwiseValidLabDispatch()
    {
        await using FeatureFlagApiTestApplication app =
            await FeatureFlagApiTestApplication.StartAsync(
                [LabRealSeed(false, Set("lab-destination-a"))]);
        IDispatchGate gate = app.Services.GetRequiredService<IDispatchGate>();
        Assert.True((await gate.EvaluateAsync(
            FeatureFlagEnvironments.Lab,
            "lab-destination-a")).Allowed);

        using HttpResponseMessage response = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Lab,
            new FeatureFlagChangeSet(GlobalDialKillSwitch: true),
            "lab emergency stop");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        DispatchGateDecision blocked = await gate.EvaluateAsync(
            FeatureFlagEnvironments.Lab,
            "lab-destination-a");
        Assert.False(blocked.Allowed);
        Assert.Equal("GLOBAL_KILL_SWITCH_ON", blocked.Reason);
    }

    [Fact]
    [Trait("TestId", "IT-FLAG-FAILCLOSED-09")]
    public async Task ConfigOrAuditFailureBlocksDispatchAndAuditFailureCannotMutateState()
    {
        FeatureFlagSnapshot seed = LabRealSeed(false, Set("lab-destination-a"));
        await using FeatureFlagApiTestApplication app =
            await FeatureFlagApiTestApplication.StartAsync([seed]);
        IDispatchGate gate = app.Services.GetRequiredService<IDispatchGate>();

        app.Store.Available = false;
        DispatchGateDecision configBlocked = await gate.EvaluateAsync(
            FeatureFlagEnvironments.Lab,
            "lab-destination-a");
        Assert.False(configBlocked.Allowed);
        Assert.Equal("CONFIG_PROVIDER_UNAVAILABLE", configBlocked.Reason);

        app.Store.Available = true;
        app.RuntimeSafety.AuditProviderHealthy = false;
        DispatchGateDecision auditBlocked = await gate.EvaluateAsync(
            FeatureFlagEnvironments.Lab,
            "lab-destination-a");
        Assert.False(auditBlocked.Allowed);
        Assert.Equal("AUDIT_PROVIDER_UNAVAILABLE", auditBlocked.Reason);

        app.Audit.Available = false;
        using HttpResponseMessage mutation = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Lab,
            new FeatureFlagChangeSet(GlobalDialKillSwitch: true),
            "stop while audit down");
        Assert.Equal(HttpStatusCode.Conflict, mutation.StatusCode);
        Assert.Equal(IvrErrorCodes.OperationalBlocked, await ReadErrorCodeAsync(mutation));
        Assert.Equal(seed, await app.Store.ReadFreshAsync(FeatureFlagEnvironments.Lab));
    }

    [Fact]
    [Trait("TestId", "IT-FLAG-KILL-04")]
    public async Task RealToMockKillBatchIsAtomicAndItsRevisionIsImmediatelyVerifiable()
    {
        FeatureFlagSnapshot seed = LabRealSeed(false, Set("lab-destination-a"));
        await using FeatureFlagApiTestApplication app =
            await FeatureFlagApiTestApplication.StartAsync([seed]);

        using HttpResponseMessage response = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Lab,
            new FeatureFlagChangeSet(
                ExecutionMode: IvrOptions.MockExecutionMode,
                SimProvider: FeatureFlagValues.MockSimProvider,
                GlobalDialKillSwitch: true),
            "return lab to mock");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        FeatureFlagSnapshot stored = await app.Store.ReadFreshAsync(
            FeatureFlagEnvironments.Lab);
        Assert.Equal(seed.Revision + 1, stored.Revision);
        Assert.Equal(IvrOptions.MockExecutionMode, stored.ExecutionMode);
        Assert.Equal(FeatureFlagValues.MockSimProvider, stored.SimProvider);
        Assert.True(stored.GlobalDialKillSwitch);

        using HttpRequestMessage verifyRequest = new(
            HttpMethod.Get,
            $"/v1/ivr/order-confirmation/feature-flags/{FeatureFlagEnvironments.Lab}/kill-switch");
        TestAdminTokens.Authorize(verifyRequest, AdminScope.Read, "operator-1");
        using HttpResponseMessage verification = await app.Client.SendAsync(verifyRequest);
        Assert.Equal(HttpStatusCode.OK, verification.StatusCode);
        KillSwitchVerification? body = await verification.Content
            .ReadFromJsonAsync<KillSwitchVerification>();
        Assert.NotNull(body);
        Assert.Equal(stored.Revision, body.Revision);
        Assert.True(body.GlobalDialKillSwitch);
        Assert.False(body.RealCallsEnabled);
    }

    [Fact]
    [Trait("TestId", "IT-FLAG-IDEMP-13")]
    public async Task ReplayingSameMutationKeyReturnsOriginalSnapshotWithoutSecondAudit()
    {
        FeatureFlagSnapshot seed = LabRealSeed(false, Set("lab-destination-a"));
        await using FeatureFlagApiTestApplication app =
            await FeatureFlagApiTestApplication.StartAsync([seed]);
        string idempotencyKey = $"flag-replay-{Guid.NewGuid():N}";

        using HttpResponseMessage first = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Lab,
            new FeatureFlagChangeSet(GlobalDialKillSwitch: true),
            "idempotent emergency stop",
            idempotencyKey: idempotencyKey);
        using HttpResponseMessage replay = await SendMutationAsync(
            app,
            FeatureFlagEnvironments.Lab,
            new FeatureFlagChangeSet(GlobalDialKillSwitch: true),
            "idempotent emergency stop",
            idempotencyKey: idempotencyKey);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
        Assert.Equal(
            await first.Content.ReadAsStringAsync(),
            await replay.Content.ReadAsStringAsync());
        Assert.Single(app.Audit.Entries);
        Assert.Equal(seed.Revision + 1, (await app.Store.ReadFreshAsync(
            FeatureFlagEnvironments.Lab)).Revision);
    }

    private static FeatureFlagSnapshot LabRealSeed(
        bool globalDialKillSwitch,
        IReadOnlySet<string> allowlist) =>
        FeatureFlagSnapshot.SafeDefault(FeatureFlagEnvironments.Lab) with
        {
            Revision = 10,
            ExecutionMode = IvrOptions.LabRealSimExecutionMode,
            SimProvider = FeatureFlagValues.VendorSimProvider,
            GlobalDialKillSwitch = globalDialKillSwitch,
            LabDestinationAllowlist = allowlist,
        };

    private static FeatureFlagSnapshot ProductionSeed(
        bool globalDialKillSwitch,
        IReadOnlySet<string> allowlist) =>
        FeatureFlagSnapshot.SafeDefault(FeatureFlagEnvironments.Production) with
        {
            Revision = 20,
            ExecutionMode = IvrOptions.ProductionRealExecutionMode,
            SalesProvider = FeatureFlagValues.TargetV1,
            SimProvider = FeatureFlagValues.VendorSimProvider,
            AttemptPolicyVersion = "approved-v1",
            RealCustomerCallAllowed = true,
            LabDestinationAllowlist = allowlist,
            GlobalDialKillSwitch = globalDialKillSwitch,
        };

    /// <summary>
    /// W-0190. An environment name the catalogue does not carry is a client mistake, and has to
    /// read as one.
    /// <para>
    /// The name arrives as a path segment. Unchecked it reached <c>SafeDefault</c>, which throws
    /// <c>ArgumentOutOfRangeException</c> — so <c>GET .../feature-flags/development</c> (the
    /// plausible typo for <c>dev</c>) answered <c>500</c>. A 500 sends whoever is on call looking
    /// for a fault that is not there, and it is the one status a caller-supplied path segment must
    /// never be able to produce.
    /// </para>
    /// </summary>
    [Theory]
    [InlineData("development")]
    [InlineData("production")]
    [InlineData("DEV")]
    [InlineData("nonesuch")]
    [Trait("TestId", "IT-FLAG-UNKNOWNENV-13")]
    public async Task AnUnknownEnvironmentIsNotFoundRatherThanAnInternalError(string environment)
    {
        await using FeatureFlagApiTestApplication app =
            await FeatureFlagApiTestApplication.StartAsync();

        using HttpResponseMessage read = await SendReadAsync(app, environment);
        Assert.Equal(HttpStatusCode.NotFound, read.StatusCode);
        Assert.Equal(IvrErrorCodes.NotFound, await ReadErrorCodeAsync(read));

        using HttpRequestMessage killSwitchRequest = new(
            HttpMethod.Get,
            $"/v1/ivr/order-confirmation/feature-flags/{environment}/kill-switch");
        TestAdminTokens.Authorize(killSwitchRequest, AdminScope.Read, "operator-1");
        using HttpResponseMessage killSwitch = await app.Client.SendAsync(killSwitchRequest);
        Assert.Equal(HttpStatusCode.NotFound, killSwitch.StatusCode);

        using HttpResponseMessage mutation = await SendMutationAsync(
            app,
            environment,
            new FeatureFlagChangeSet(GlobalDialKillSwitch: true),
            "emergency stop");
        Assert.Equal(HttpStatusCode.NotFound, mutation.StatusCode);

        // And the refusal happened before anything was written.
        Assert.Empty(app.Audit.Entries);
    }

    private static async Task<HttpResponseMessage> SendMutationAsync(
        FeatureFlagApiTestApplication app,
        string environment,
        FeatureFlagChangeSet changes,
        string reason,
        string? approvalReference = null,
        string permissions = IvrPermissions.RuntimeGateAdmin,
        string? idempotencyKey = null)
    {
        using HttpRequestMessage request = new(
            HttpMethod.Post,
            $"/v1/ivr/order-confirmation/feature-flags/{environment}");
        TestAdminTokens.AuthorizeForPermission(request, permissions, "operator-1");
        request.Headers.Add(
            "Idempotency-Key",
            idempotencyKey ?? $"flag-test-{Guid.NewGuid():N}");
        request.Content = JsonContent.Create(
            new FeatureFlagMutationRequest(changes, reason, approvalReference));
        return await app.Client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> SendReadAsync(
        FeatureFlagApiTestApplication app,
        string environment)
    {
        using HttpRequestMessage request = new(
            HttpMethod.Get,
            $"/v1/ivr/order-confirmation/feature-flags/{environment}");
        TestAdminTokens.Authorize(request, AdminScope.Read, "operator-1");
        return await app.Client.SendAsync(request);
    }

    private static async Task<string?> ReadErrorCodeAsync(HttpResponseMessage response)
    {
        using JsonDocument body = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("error").GetProperty("code").GetString();
    }

    private static HashSet<string> Set(params string[] values) =>
        new HashSet<string>(values, StringComparer.Ordinal);

    private static async Task AssertPolicyMismatchAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(IvrErrorCodes.PolicyMismatch, await ReadErrorCodeAsync(response));
    }
}
