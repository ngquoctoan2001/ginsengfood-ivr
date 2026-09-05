using System.Globalization;
using System.Text.Json;
using Ivr.Api.Admin;
using Ivr.Api.Intake;
using Ivr.Api.Internal;
using Ivr.Domain.Confirmation;
using Ivr.Domain.DevTooling;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.DevTooling;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Application;

public interface IDevToolingApiService
{
    public Task<SeedLoadApiResult> LoadSeedAsync(
        SeedLoadRequest request,
        string actorId,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken);

    public Task<ScenarioDryRunApiResult> DryRunScenarioAsync(
        string scenarioId,
        AdminMutationRequest request,
        string actorId,
        string correlationId,
        CancellationToken cancellationToken);

    public Task<IntegrationProfileApiResult> ApplyIntegrationProfileAsync(
        string profileId,
        AdminMutationRequest request,
        string actorId,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken);
}

/// <summary>
/// The UI-07 non-production developer surface: seed loader, scenario runner and
/// integration-status profiles (W-0112).
/// <para>
/// Every method re-checks that this deployment is non-production, independently of the route
/// guard that already refuses to map these endpoints in production. Two barriers rather than one,
/// because the failure being defended against is a future change that adds a route or a caller
/// and misses the guard — and the thing on the other side of that mistake is a seed loader
/// writing fixture customers into a live database.
/// </para>
/// </summary>
public sealed class DevToolingApiService(
    SeedCatalog catalog,
    ITaskIntakeService intakeService,
    IInternalAdminApiService adminService,
    IDbContextFactory<IvrDbContext> dbContextFactory,
    // Optional: only the PostgreSQL service set registers a writer. A host running the in-memory
    // doubles already has the candidate policies in its fake registry, so there is nothing to
    // register and nothing to fail about.
    IAttemptPolicyRegistryWriter? policyWriter,
    IOptions<IvrOptions> ivrOptions,
    IOptions<DevToolingOptions> devOptions,
    IHostEnvironment environment,
    TimeProvider timeProvider) : IDevToolingApiService
{
    public const string TaskDataset = "sales-target-v1";

    /// <summary>
    /// W-0190. Reported for a fixture this database already holds, instead of the raw
    /// idempotency-conflict code. Not a wire enum - <c>IvrSeedTaskOutcome.decision</c> is a free
    /// string in the contract - so naming the case costs no schema change.
    /// </summary>
    public const string AlreadySeededDecision = "SEED_TASK_ALREADY_LOADED";

    private static readonly JsonSerializerOptions CanonicalJson = new(JsonSerializerDefaults.Web);

    public async Task<SeedLoadApiResult> LoadSeedAsync(
        SeedLoadRequest request,
        string actorId,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        EnsureNonProduction();
        ArgumentNullException.ThrowIfNull(request);
        Validate(request.Reason, actorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

        DateTimeOffset now = timeProvider.GetUtcNow();
        int policies = await EnsureAttemptPoliciesAsync(actorId, correlationId, cancellationToken)
            .ConfigureAwait(false);
        SeedTaskCatalog catalogue = await ReadAsync(
            token => catalog.ReadTasksAsync(request.RebaseWindows ? now : null, token),
            cancellationToken).ConfigureAwait(false);
        IReadOnlyList<SeedTaskFixture> fixtures = catalogue.Tasks;

        ExecutionMode mode = ParseExecutionMode(ivrOptions.Value.ExecutionMode);
        List<SeedTaskOutcomeView> outcomes = new(fixtures.Count);
        int accepted = 0;
        foreach (SeedTaskFixture fixture in fixtures)
        {
            // Guaranteed by SeedCatalog, which refuses a fixture with no task_id.
            string taskId = fixture.Body.Task_id ?? fixture.Scenario;

            // Through the real intake service, not a direct INSERT. Fixture rows created by a
            // path production never uses are rows that can pass a rehearsal the live system
            // would have rejected — which is the failure a rehearsal exists to catch.
            TaskIntakeOutcome outcome;
            try
            {
                outcome = await intakeService.IntakeAsync(
                    new TaskIntakeCommand(
                        fixture.Body,
                        // Scoped so a reload does not collide with a task the intake endpoint
                        // admitted under the same fixture key.
                        string.Concat("devseed:", fixture.IdempotencyKey),
                        fixture.CorrelationId,
                        PayloadHash(fixture.Body),
                        mode),
                    cancellationToken).ConfigureAwait(false);
            }
            catch (IvrFailureException exception)
            {
                // Reported against the fixture rather than failing the whole load: one
                // conflicting fixture must not hide the eight that loaded.
                //
                // W-0190. The overwhelmingly common case is a second run of the same dataset.
                // The fixture key is the same but the rebased window makes the body different, so
                // intake answers IVR_IDEMPOTENCY_CONFLICT - correctly, and unhelpfully: an
                // operator pressing "load seed" twice got nine red conflicts and no way to tell
                // "already loaded" from "broken". Suffixing the key to force a fresh admission is
                // NOT the fix: task_id carries a unique index, so a second load under a new key
                // trades a clean conflict for a constraint violation.
                //
                // So the reload is reported as what it is. The existing job id comes with it,
                // which is the thing a rehearsal actually wants next.
                // The conflict code is itself the proof: the idempotency key is scoped with a
                // "devseed:" prefix, so nothing but a previous seed load of this same fixture can
                // have used it. No second lookup is needed to establish that this is a reload -
                // only to add the call job id, when the fixture produced one.
                bool alreadySeeded = exception.ErrorCode == IvrErrorCodes.IdempotencyConflict;
                string? existingJobId = alreadySeeded
                    ? await FindSeededJobIdAsync(taskId, cancellationToken).ConfigureAwait(false)
                    : null;
                outcomes.Add(new SeedTaskOutcomeView(
                    fixture.Scenario,
                    taskId,
                    alreadySeeded ? AlreadySeededDecision : exception.ErrorCode,
                    existingJobId,
                    [alreadySeeded
                        ? "This fixture is already loaded in this database; whatever it produced "
                          + "the first time is unchanged."
                        : exception.Message]));
                continue;
            }

            if (outcome.IsFailure)
            {
                outcomes.Add(new SeedTaskOutcomeView(
                    fixture.Scenario,
                    taskId,
                    outcome.FailureCode!,
                    null,
                    [outcome.FailureMessage ?? string.Empty]));
                continue;
            }

            if (outcome.Decision is TaskIntakeDecisions.AcceptedCallJobCreated
                or TaskIntakeDecisions.AcceptedDryRunOnly)
            {
                accepted++;
            }

            outcomes.Add(new SeedTaskOutcomeView(
                fixture.Scenario,
                taskId,
                outcome.Decision,
                outcome.IvrCallJobId,
                outcome.BlockedReasons));
        }

        return new SeedLoadApiResult(
            now,
            TaskDataset,
            ivrOptions.Value.ExecutionMode,
            fixtures.Count,
            accepted,
            catalogue.WindowsRebased,
            catalogue.RebasedCount,
            policies,
            outcomes,
            correlationId);
    }

    public async Task<ScenarioDryRunApiResult> DryRunScenarioAsync(
        string scenarioId,
        AdminMutationRequest request,
        string actorId,
        string correlationId,
        CancellationToken cancellationToken)
    {
        EnsureNonProduction();
        ArgumentNullException.ThrowIfNull(request);
        Validate(request.Reason, actorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(scenarioId);

        IReadOnlyList<ScenarioDefinition> scenarios = await ReadAsync(
            token => catalog.ReadScenariosAsync(token),
            cancellationToken).ConfigureAwait(false);
        ScenarioDefinition scenario = scenarios.SingleOrDefault(
                item => string.Equals(item.Id, scenarioId, StringComparison.Ordinal))
            ?? throw IvrErrors.NotFound("The scenario was not found in the seed catalogue.");

        DevToolingOptions settings = devOptions.Value;
        ScenarioDryRunReport report = CallScenarioDryRun.Execute(
            scenario,
            timeProvider.GetUtcNow(),
            TimeSpan.FromSeconds(settings.ScenarioWindowSeconds),
            settings.ScenarioTechnicalRetryLimit);

        return new ScenarioDryRunApiResult(
            timeProvider.GetUtcNow(),
            report.ScenarioId,
            report.TaskRef,
            report.Coverage == ScenarioCoverage.Replayed ? "REPLAYED" : "NOT_REPLAYABLE",
            report.ExpectedResultType,
            report.ExpectedCounted,
            report.ActualResultType,
            report.ActualCounted,
            report.Matches,
            [.. report.Attempts.Select(attempt => new ScenarioAttemptView(
                attempt.AttemptNumber,
                attempt.RawCallStatus,
                attempt.RawDtmf,
                attempt.ResultType,
                attempt.Counted,
                attempt.Final,
                attempt.Reason))],
            report.Notes,
            correlationId);
    }

    public async Task<IntegrationProfileApiResult> ApplyIntegrationProfileAsync(
        string profileId,
        AdminMutationRequest request,
        string actorId,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        EnsureNonProduction();
        ArgumentNullException.ThrowIfNull(request);
        Validate(request.Reason, actorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(profileId);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

        IReadOnlyList<IntegrationStatusProfile> profiles = await ReadAsync(
            token => catalog.ReadIntegrationProfilesAsync(token),
            cancellationToken).ConfigureAwait(false);
        IntegrationStatusProfile profile = profiles.SingleOrDefault(
                item => string.Equals(item.Id, profileId, StringComparison.Ordinal))
            ?? throw IvrErrors.NotFound("The integration-status profile was not found.");

        List<IntegrationProfileEffectView> effects =
        [
            await ApplySimGatewayAsync(
                profile,
                request,
                actorId,
                correlationId,
                idempotencyKey,
                cancellationToken).ConfigureAwait(false),

            // The remaining four are declared, not enforced. IVR holds no client for any of them
            // and no probe reports their health, so there is nothing in the running system for a
            // profile to switch. Saying so in the response is the difference between a rehearsal
            // and a screen that looks like one.
            NotWired("ORDER_CORE", profile.OrderCore,
                "Callback delivery is configuration; IVR runs no Order Core probe (W-0040)."),
            NotWired("CRM_DO_NOT_CALL", profile.CrmDoNotCall,
                "Voice restriction arrives inside the Sales task; IVR holds no CRM client."),
            NotWired("EVIDENCE_REGISTRY", profile.EvidenceRegistry,
                "Evidence is written locally; no external registry is contacted."),
        ];

        int enforced = effects.Count(effect => effect.Enforced);
        return new IntegrationProfileApiResult(
            timeProvider.GetUtcNow(),
            profile.Id,
            profile.Expected,
            enforced,
            effects.Count - enforced,
            effects,
            correlationId);
    }

    /// <summary>
    /// Registers the candidate attempt policies the fixtures name, and returns how many were new.
    /// <para>
    /// Without this the loader returns nine <c>TASK_HELD_POLICY_MISSING</c> decisions on a fresh
    /// database: the fixtures declare <c>mock-lab-v1</c>, and intake resolves that against the
    /// registry rather than trusting the task body. Seeding tasks without seeding what they
    /// depend on would have produced a loader that technically ran and delivered nothing.
    /// </para>
    /// <para>
    /// Registered for MOCK and LAB_REAL_SIM only. These are candidate policies, and
    /// <c>PostgresAttemptPolicyRegistryWriter</c> refuses to attach one to production regardless
    /// of what is asked for — so this passes the two modes it is entitled to rather than relying
    /// on that refusal.
    /// </para>
    /// </summary>
    /// <summary>
    /// The call job a previously loaded fixture produced, or <c>null</c> when it produced none.
    /// <para>
    /// Several fixtures exist precisely to be refused - a do-not-call customer, an expired window
    /// - so a null here is a normal outcome and not a sign the fixture is missing. Whether the
    /// fixture was loaded before is already settled by the idempotency conflict; this only adds
    /// the job id when there is one to add.
    /// </para>
    /// </summary>
    private async Task<string?> FindSeededJobIdAsync(
        string taskId,
        CancellationToken cancellationToken)
    {
        await using IvrDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        return await dbContext.CallJobs
            .AsNoTracking()
            .Where(job => job.TaskId == taskId)
            .Select(job => job.IvrCallJobId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<int> EnsureAttemptPoliciesAsync(
        string actorId,
        string correlationId,
        CancellationToken cancellationToken)
    {
        if (policyWriter is null)
        {
            return 0;
        }

        int registered = 0;
        foreach (AttemptPolicySnapshot policy in CandidateAttemptPolicies.Create())
        {
            try
            {
                await policyWriter.RegisterNewVersionAsync(
                    policy,
                    [ExecutionMode.Mock, ExecutionMode.LabRealSim],
                    actorId,
                    "Seed loader prerequisite (UI-07)",
                    correlationId,
                    cancellationToken).ConfigureAwait(false);
                registered++;
            }
            catch (InvalidOperationException)
            {
                // Already registered. The writer refuses to re-register a version, which is the
                // behaviour that makes a policy version immutable — so a repeat load skips it
                // rather than rewriting the policy every task in the database was admitted under.
            }
        }

        return registered;
    }

    /// <summary>
    /// The one dependency IVR genuinely owns. Delegated to the SIM channel admin operations
    /// rather than written here, so the fail-closed re-enable check applies unchanged and each
    /// channel keeps its own audit row. A second copy of that check is a copy that drifts.
    /// </summary>
    private async Task<IntegrationProfileEffectView> ApplySimGatewayAsync(
        IntegrationStatusProfile profile,
        AdminMutationRequest request,
        string actorId,
        string correlationId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        bool bringUp = profile.SimGateway is "up" or "mock_up";
        bool bringDown = profile.SimGateway is "down" or "mock_down";
        if (!bringUp && !bringDown)
        {
            return NotWired(
                "SIM_GATEWAY",
                profile.SimGateway,
                "The profile names no SIM gateway state this surface can apply.");
        }

        List<string> channelIds;
        await using (IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            channelIds =
            [
                .. await context.SimChannels
                    .AsNoTracking()
                    .Where(channel => channel.Enabled != bringUp)
                    .OrderBy(channel => channel.SimChannelId)
                    .Select(channel => channel.SimChannelId)
                    .ToListAsync(cancellationToken).ConfigureAwait(false),
            ];
        }

        int changed = 0;
        List<string> refused = [];
        foreach (string channelId in channelIds)
        {
            string scopedKey = string.Concat(idempotencyKey, ":", channelId);
            try
            {
                if (bringUp)
                {
                    await adminService.EnableChannelAsync(
                        channelId, request, actorId, correlationId, scopedKey, cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    await adminService.DisableChannelAsync(
                        channelId, request, actorId, correlationId, scopedKey, cancellationToken)
                        .ConfigureAwait(false);
                }

                changed++;
            }
            catch (IvrFailureException)
            {
                // A channel the fail-closed check refuses to re-enable is reported, not retried
                // and not worked around. A rehearsal that quietly forces a quarantined channel
                // back into service would be rehearsing the wrong system.
                refused.Add(channelId);
            }
        }

        string detail = string.Create(
            CultureInfo.InvariantCulture,
            $"{changed} channel(s) {(bringUp ? "enabled" : "disabled")}");
        if (refused.Count > 0)
        {
            detail = string.Concat(
                detail,
                "; ",
                refused.Count.ToString(CultureInfo.InvariantCulture),
                " refused by the fail-closed health check: ",
                string.Join(", ", refused));
        }

        return new IntegrationProfileEffectView(
            "SIM_GATEWAY",
            profile.SimGateway,
            true,
            detail);
    }

    private static IntegrationProfileEffectView NotWired(
        string dependency,
        string requestedState,
        string detail) => new(dependency, requestedState, false, detail);

    /// <summary>
    /// The second barrier. The route guard already refuses to map these endpoints outside a
    /// non-production deployment; this refuses to run them even if something maps them anyway.
    /// </summary>
    private void EnsureNonProduction()
    {
        if (!NonProductionSurface.IsAvailable(
                environment.EnvironmentName,
                ivrOptions.Value.ExecutionMode,
                ivrOptions.Value.RealCustomerCallAllowed))
        {
            throw IvrErrors.NotFound("The developer surface is not available in this deployment.");
        }
    }

    private static async Task<T> ReadAsync<T>(
        Func<CancellationToken, Task<T>> read,
        CancellationToken cancellationToken)
    {
        try
        {
            return await read(cancellationToken).ConfigureAwait(false);
        }
        catch (SeedCatalogException exception)
        {
            throw IvrErrors.OperationalBlocked(exception.Message);
        }
    }

    private static void Validate(string reason, string actorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 500)
        {
            throw IvrErrors.MalformedRequest("A reason between 1 and 500 characters is required.");
        }
    }

    /// <summary>
    /// Deferred to the intake endpoint's own canonicaliser. A locally written hash was wrong in
    /// two ways at once — lowercase hex, which the <c>ck_ivr_task_intake_outbox_hash</c> check
    /// constraint rejects outright, and a different canonical form, which would have made the
    /// same task body fingerprint differently depending on which door it came through.
    /// </summary>
    private static string PayloadHash(object body)
    {
        using JsonDocument document = JsonDocument.Parse(
            JsonSerializer.Serialize(body, CanonicalJson));
        return TaskIntakeEndpoint.CanonicalJsonSha256(document.RootElement);
    }

    private static ExecutionMode ParseExecutionMode(string value) => value switch
    {
        IvrOptions.MockExecutionMode => ExecutionMode.Mock,
        IvrOptions.LabRealSimExecutionMode => ExecutionMode.LabRealSim,
        IvrOptions.ProductionRealExecutionMode => ExecutionMode.ProductionReal,
        _ => throw new InvalidOperationException("Unsupported IVR execution mode."),
    };
}
