using System.Text.Json;
using Ivr.Api.Application;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Policies;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Repositories;
using Ivr.Infrastructure.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.IntegrationTests;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class EligibilityPersistenceTests(PostgresPersistenceFixture fixture)
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 13, 7, 0, 0, TimeSpan.Zero);

    // W-0030 / P4-2. The old fixture carried only {"decision":"ELIGIBLE"}; the typed evidence
    // rules now also require a source version and a fresh capture stamp, so the fixture supplies
    // them. The rule was not relaxed to keep the old fixture passing — the fixture was corrected.
    private static readonly string EligibleSnapshotJson = JsonSerializer.Serialize(new
    {
        decision = "ELIGIBLE",
        source_version = "sales-eligibility-v1",
        captured_at = Now.AddSeconds(-30),
        source_available = true,
        blockers = Array.Empty<string>(),
    });

    [Fact]
    [Trait("TestId", "IT-ELIG-CAP-05")]
    public async Task CapacityShortageCreatesIncidentWithoutCountedAttemptOrDispatch()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPendingTaskAsync(
            factory,
            "TASK-ELIG-CAP-05",
            "JOB-ELIG-CAP-05",
            "CREATED",
            "READY_FOR_ELIGIBILITY");
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new CapacityShortageProvider(),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            "TASK-ELIG-CAP-05",
            "corr-elig-cap-05");

        Assert.False(result.Eligible);
        Assert.Equal(EligibilityDecisions.CapacityException, result.Decision);
        Assert.False(result.IsCountedCustomerAttempt);
        Assert.NotNull(result.CapacityIncidentId);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        ConfirmationTaskEntity task = await verification.ConfirmationTasks
            .AsNoTracking()
            .SingleAsync();
        CallJobEntity job = await verification.CallJobs.AsNoTracking().SingleAsync();
        CapacityIncidentEntity incident = await verification.CapacityIncidents
            .AsNoTracking()
            .SingleAsync();
        Assert.Equal(EligibilityDecisions.CapacityException, task.EligibilityDecision);
        Assert.Equal("CAPACITY_HELD", job.Status);
        Assert.Equal("HELD_CAPACITY", job.QueueStatus);
        Assert.Equal(incident.CapacityIncidentId, job.CapacityIncidentId);
        Assert.False(incident.HoldNewCalls);
        Assert.Equal(0, await verification.CallAttempts.CountAsync());
        Assert.NotEmpty(await verification.EvidenceLinks.AsNoTracking().ToListAsync());
        AuditLogEntity audit = await verification.AuditLog
            .AsNoTracking()
            .SingleAsync();
        Assert.Equal("ELIGIBILITY_EVALUATED", audit.Action);
        using JsonDocument auditData = JsonDocument.Parse(audit.DataJson);
        Assert.False(auditData.RootElement
            .GetProperty("is_counted_customer_attempt")
            .GetBoolean());
        TaskIntakeOutboxEntity outbox = await verification.TaskIntakeOutbox
            .AsNoTracking()
            .SingleAsync();
        Assert.Equal("PUBLISHED", outbox.Status);
    }

    [Fact]
    [Trait("TestId", "IT-ELIG-MOCK-06")]
    public async Task MockEligibleDecisionRemainsHeldWithNoEgressOrAttempt()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPendingTaskAsync(
            factory,
            "TASK-ELIG-MOCK-06",
            "JOB-ELIG-MOCK-06",
            "DRY_RUN",
            "HELD_MOCK");
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new CapacityAvailableProvider(),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            "TASK-ELIG-MOCK-06",
            "corr-elig-mock-06");

        Assert.True(result.Eligible);
        Assert.Equal(EligibilityDecisions.Eligible, result.Decision);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallJobEntity job = await verification.CallJobs.AsNoTracking().SingleAsync();
        TaskIntakeOutboxEntity outbox = await verification.TaskIntakeOutbox
            .AsNoTracking()
            .SingleAsync();
        Assert.True(job.Eligible);
        Assert.Equal("DRY_RUN", job.Status);
        Assert.Equal("HELD_MOCK", job.QueueStatus);
        Assert.Equal("HELD_MOCK", outbox.Status);
        Assert.Null(outbox.PublishedAt);
        Assert.Equal(0, await verification.CallAttempts.CountAsync());
        Assert.Equal(0, await verification.CapacityIncidents.CountAsync());
    }

    [Fact]
    [Trait("TestId", "IT-ELIG-DNC-07")]
    public async Task StoredPhoneRestrictionBlocksBeforeCapacityWithEvidenceAndNoAttempt()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPendingTaskAsync(
            factory,
            "TASK-ELIG-DNC-07",
            "JOB-ELIG-DNC-07",
            "DRY_RUN",
            "HELD_MOCK",
            callRestriction: true);
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new UnexpectedCapacityProvider(),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            "TASK-ELIG-DNC-07",
            "corr-elig-dnc-07");

        Assert.False(result.Eligible);
        Assert.Equal(EligibilityDecisions.BlockedOperational, result.Decision);
        Assert.Equal(
            EligibilityReasonCodes.PhoneCallRestricted,
            Assert.Single(result.Reasons).Code);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        CallJobEntity job = await verification.CallJobs.AsNoTracking().SingleAsync();
        Assert.Equal("BLOCKED", job.Status);
        Assert.Equal("BLOCKED", job.QueueStatus);
        Assert.Equal(0, await verification.CallAttempts.CountAsync());
        Assert.Equal(0, await verification.CapacityIncidents.CountAsync());
        Assert.NotEmpty(await verification.EvidenceLinks.AsNoTracking().ToListAsync());
    }

    [Fact]
    [Trait("TestId", "IT-ELIG-FAILCLOSED-08")]
    public async Task CapacityResponseWithoutEvidenceFailsClosed()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPendingTaskAsync(
            factory,
            "TASK-ELIG-FAILCLOSED-08",
            "JOB-ELIG-FAILCLOSED-08",
            "DRY_RUN",
            "HELD_MOCK");
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new MissingEvidenceCapacityProvider(),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            "TASK-ELIG-FAILCLOSED-08",
            "corr-elig-failclosed-08");

        Assert.False(result.Eligible);
        Assert.Equal(EligibilityDecisions.HeldAdminReview, result.Decision);
        Assert.Equal(
            EligibilityReasonCodes.CapacitySourceUnavailable,
            Assert.Single(result.Reasons).Code);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(0, await verification.CallAttempts.CountAsync());
        Assert.Equal(0, await verification.CapacityIncidents.CountAsync());
        ReviewItemEntity review = await verification.ReviewItems.AsNoTracking().SingleAsync();
        Assert.Equal("ELIGIBILITY_DECISION", review.SourceType);
        Assert.Equal("TASK-ELIG-FAILCLOSED-08", review.SourceId);
        Assert.Equal("OPEN", review.Status);
    }

    [Theory]
    [Trait("TestId", "IT-ELIG-EVIDENCE-10")]
    [InlineData("pass", null, true, null)]
    [InlineData("block", "blocked", false, EligibilityReasonCodes.EligibilitySnapshotBlocked)]
    [InlineData("stale", "stale", false, EligibilityReasonCodes.EligibilitySnapshotStale)]
    [InlineData("source-down", "unavailable", false,
        EligibilityReasonCodes.EligibilitySourceUnavailable)]
    [InlineData("unreadable", "unreadable", false,
        EligibilityReasonCodes.EligibilitySnapshotUnreadable)]
    public async Task SalesEligibilityEvidenceIsValidatedFailClosedOnRealStorage(
        string caseId,
        string? scenario,
        bool expectedEligible,
        string? expectedReasonCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        string taskId = string.Concat("TASK-ELIG-EV-", caseId.ToUpperInvariant());
        string snapshotJson = SnapshotFor(scenario);
        await SeedPendingTaskAsync(
            factory,
            taskId,
            string.Concat("JOB-ELIG-EV-", caseId.ToUpperInvariant()),
            "CREATED",
            "READY_FOR_ELIGIBILITY",
            eligibilitySnapshotJson: snapshotJson);
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new CapacityAvailableProvider(),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            taskId,
            string.Concat("corr-elig-ev-", caseId));

        Assert.Equal(expectedEligible, result.Eligible);
        if (expectedReasonCode is not null)
        {
            Assert.Equal(expectedReasonCode, Assert.Single(result.Reasons).Code);
        }

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        // Nothing that fails evidence validation may leave a dispatched attempt behind.
        Assert.Equal(0, await verification.CallAttempts.CountAsync());

        // The snapshot digest is persisted for every case, including the rejected ones: the
        // evidence trail must show which bytes were judged, not only the ones that passed.
        ConfirmationTaskEntity task = await verification.ConfirmationTasks.AsNoTracking()
            .SingleAsync(entity => entity.TaskId == taskId);
        Assert.Equal(
            DeterministicSnapshotHasher.Compute(snapshotJson),
            task.EligibilitySnapshotHash);

        if (expectedEligible)
        {
            Assert.Contains(
                result.EvidenceRefs,
                reference => reference.EndsWith(
                    string.Concat("#eligibility/snapshot/", task.EligibilitySnapshotHash),
                    StringComparison.Ordinal));
        }
    }

    [Fact]
    [Trait("TestId", "IT-ELIG-EVIDENCE-11")]
    public async Task EligibilitySnapshotHashColumnRejectsAnythingThatIsNotADigest()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPendingTaskAsync(
            factory,
            "TASK-ELIG-EV-HASHGUARD",
            "JOB-ELIG-EV-HASHGUARD",
            "CREATED",
            "READY_FOR_ELIGIBILITY");

        await using IvrDbContext context = await factory.CreateDbContextAsync();
        ConfirmationTaskEntity task = await context.ConfirmationTasks
            .SingleAsync(entity => entity.TaskId == "TASK-ELIG-EV-HASHGUARD");

        // Uppercase hex, a truncated digest, and a raw snapshot body must all be refused by the
        // database itself — the column is a digest field, not a second place to park evidence.
        foreach (string invalid in new[]
        {
            "ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789",
            "abc123",
            "{\"decision\":\"ELIGIBLE\"}",
        })
        {
            task.EligibilitySnapshotHash = invalid;
            await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
            context.ChangeTracker.Clear();
            task = await context.ConfirmationTasks
                .SingleAsync(entity => entity.TaskId == "TASK-ELIG-EV-HASHGUARD");
        }
    }

    [Theory]
    [Trait("TestId", "IT-ELIG-VOICE-13")]
    [InlineData("allowed", true, null)]
    [InlineData("restricted", false, EligibilityReasonCodes.PhoneCallRestricted)]
    [InlineData("resolver-down", false,
        EligibilityReasonCodes.PhoneCallRestrictionSourceUnavailable)]
    [InlineData("marketing-noise", true, null)]
    public async Task VoiceRestrictionEvidenceIsHonouredAndMarketingConsentIsNot(
        string caseId,
        bool expectedEligible,
        string? expectedReasonCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caseId);
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        string taskId = string.Concat("TASK-ELIG-VOICE-", caseId.ToUpperInvariant());
        await SeedPendingTaskAsync(
            factory,
            taskId,
            string.Concat("JOB-ELIG-VOICE-", caseId.ToUpperInvariant()),
            "CREATED",
            "READY_FOR_ELIGIBILITY",
            eligibilitySnapshotJson: VoiceSnapshotFor(caseId));
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new CapacityAvailableProvider(),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            taskId,
            string.Concat("corr-elig-voice-", caseId));

        Assert.Equal(expectedEligible, result.Eligible);
        if (expectedReasonCode is not null)
        {
            Assert.Equal(expectedReasonCode, Assert.Single(result.Reasons).Code);
        }

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(0, await verification.CallAttempts.CountAsync());
    }

    [Fact]
    [Trait("TestId", "IT-M3-AUTHORITY-05")]
    [Trait("TestId", "IT-M3-AUTHORITY-06")]
    public async Task TrustedMetadataCannotCreateASkipDecisionOrSkippedJob()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        // OD-18 compatibility case: an older M3 producer may still send the complete OD-15 trust
        // bag. IVR accepts the payload during the compatibility window but must ignore it for the
        // call/no-call decision. A valid M3 task therefore remains eligible and queued.
        await SeedPendingTaskAsync(
            factory,
            "TASK-ELIG-TRUST-14",
            "JOB-ELIG-TRUST-14",
            "CREATED",
            "READY_FOR_ELIGIBILITY",
            eligibilitySnapshotJson: JsonSerializer.Serialize(new
            {
                decision = "ELIGIBLE",
                source_version = "sales-eligibility-v1",
                captured_at = Now.AddSeconds(-30),
                source_available = true,
                trust = new { risk_evidence_available = true },
            }));
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new CapacityAvailableProvider(),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            "TASK-ELIG-TRUST-14",
            "corr-elig-trust-14");

        Assert.True(result.Eligible);
        Assert.Equal(EligibilityDecisions.Eligible, result.Decision);
        Assert.False(result.IsCountedCustomerAttempt);
        Assert.Empty(result.Advisories);

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(0, await verification.CallAttempts.CountAsync());
        ConfirmationTaskEntity task = await verification.ConfirmationTasks
            .SingleAsync(item => item.TaskId == "TASK-ELIG-TRUST-14");
        CallJobEntity job = await verification.CallJobs
            .SingleAsync(item => item.TaskId == "TASK-ELIG-TRUST-14");
        Assert.Equal(EligibilityDecisions.Eligible, task.EligibilityDecision);
        Assert.Equal(EligibilityDecisions.Eligible, job.EligibilityDecision);
        Assert.Equal("READY_FOR_SCHEDULER", job.Status);
        Assert.Equal("QUEUED", job.QueueStatus);
    }

    [Fact]
    [Trait("TestId", "IT-M3-AUTHORITY-08")]
    public async Task HistoricalTrustedSkipRowsRemainReadableDuringTheRollbackWindow()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPendingTaskAsync(
            factory,
            "TASK-M3-AUTHORITY-LEGACY-08",
            "JOB-M3-AUTHORITY-LEGACY-08",
            "CREATED",
            "READY_FOR_ELIGIBILITY");

        // HISTORICAL_EVIDENCE / LEGACY_READ only. New runtime has no producer for this value;
        // inserting it here proves old rows and a previous-image rollback remain readable.
        const string legacyDecision = "TASK_SKIPPED_TRUSTED_CUSTOMER";
        await using (IvrDbContext writer = await factory.CreateDbContextAsync())
        {
            ConfirmationTaskEntity task = await writer.ConfirmationTasks.SingleAsync(
                item => item.TaskId == "TASK-M3-AUTHORITY-LEGACY-08");
            CallJobEntity job = await writer.CallJobs.SingleAsync(
                item => item.TaskId == "TASK-M3-AUTHORITY-LEGACY-08");
            task.EligibilityDecision = legacyDecision;
            job.EligibilityDecision = legacyDecision;
            job.Status = "SKIPPED";
            job.QueueStatus = "SKIPPED";
            job.ClosedAt = Now;
            job.ClosedReason = legacyDecision;
            await writer.SaveChangesAsync();
        }

        await using IvrDbContext reader = await factory.CreateDbContextAsync();
        ConfirmationTaskEntity storedTask = await reader.ConfirmationTasks
            .AsNoTracking()
            .SingleAsync(item => item.TaskId == "TASK-M3-AUTHORITY-LEGACY-08");
        CallJobEntity storedJob = await reader.CallJobs
            .AsNoTracking()
            .SingleAsync(item => item.TaskId == "TASK-M3-AUTHORITY-LEGACY-08");
        Assert.Equal(legacyDecision, storedTask.EligibilityDecision);
        Assert.Equal(legacyDecision, storedJob.EligibilityDecision);
        Assert.Equal("SKIPPED", storedJob.Status);
        Assert.Equal("SKIPPED", storedJob.QueueStatus);
    }

    [Fact]
    [Trait("TestId", "IT-M3-AUTHORITY-13")]
    public async Task ThePreflightSqlRunsOnTheRealSchemaAndCountsTheRetiredShape()
    {
        // W-0125. tools/ops/od18-legacy-skip-preflight.sql is the query somebody will run against a
        // production database the day credentials exist. Checked-in SQL that has never touched the
        // real schema is a promise, not a tool: a wrong column name or a text-vs-jsonb comparison
        // would surface at the worst moment, on the environment that matters, to whoever inherited
        // it. Running it here against the migrated schema makes every future edit to it a test
        // subject, and pins its predicate to the runtime counter's.
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();

        // One row of trusted-skip history, exactly as an OD-15 deployment would have left it.
        await SeedPendingTaskAsync(
            factory,
            "TASK-M3-PREFLIGHT-HISTORY",
            "JOB-M3-PREFLIGHT-HISTORY",
            "CREATED",
            "READY_FOR_ELIGIBILITY",
            eligibilitySnapshotJson: """
                {"decision":"ELIGIBLE","source_version":"sales-eligibility-v1",
                 "captured_at":"2026-08-13T06:59:00+00:00","source_available":true,
                 "blockers":[],"trust":{"risk_evidence_available":true}}
                """,
            customerTrustStatus: "TRUSTED",
            trustedSkipAllowed: true,
            riskFlagsJson: "[]");
        await using (IvrDbContext writer = await factory.CreateDbContextAsync())
        {
            ConfirmationTaskEntity task = await writer.ConfirmationTasks.SingleAsync(
                item => item.TaskId == "TASK-M3-PREFLIGHT-HISTORY");
            CallJobEntity job = await writer.CallJobs.SingleAsync(
                item => item.TaskId == "TASK-M3-PREFLIGHT-HISTORY");
            task.EligibilityDecision = "TASK_SKIPPED_TRUSTED_CUSTOMER";
            job.EligibilityDecision = "TASK_SKIPPED_TRUSTED_CUSTOMER";
            job.Status = "SKIPPED";
            job.QueueStatus = "SKIPPED";
            await writer.SaveChangesAsync();
        }

        // A task carrying the retired shape but never skipped, and a risk-flagged one. Together
        // they stop every count collapsing into "one row matches everything".
        await SeedPendingTaskAsync(
            factory,
            "TASK-M3-PREFLIGHT-SHAPE-ONLY",
            "JOB-M3-PREFLIGHT-SHAPE-ONLY",
            "CREATED",
            "READY_FOR_ELIGIBILITY",
            eligibilitySnapshotJson: """
                {"decision":"ELIGIBLE","source_version":"sales-eligibility-v1",
                 "captured_at":"2026-08-13T06:59:00+00:00","source_available":true,
                 "blockers":[],"trust":{"risk_evidence_available":true}}
                """,
            riskFlagsJson: "[]");
        await SeedPendingTaskAsync(
            factory,
            "TASK-M3-PREFLIGHT-RISK-FLAGGED",
            "JOB-M3-PREFLIGHT-RISK-FLAGGED",
            "CREATED",
            "READY_FOR_ELIGIBILITY",
            eligibilitySnapshotJson: """
                {"decision":"ELIGIBLE","source_version":"sales-eligibility-v1",
                 "captured_at":"2026-08-13T06:59:00+00:00","source_available":true,
                 "blockers":[],"trust":{"risk_evidence_available":true}}
                """,
            riskFlagsJson: """["COD_FAIL_HISTORY"]""");

        Dictionary<string, string?> metrics = await RunPreflightAsync(factory);

        Assert.Equal("1", metrics["tasks_with_retired_decision"]);
        Assert.Equal("1", metrics["jobs_with_retired_decision"]);
        Assert.Equal("1", metrics["jobs_in_skipped_status"]);
        Assert.Equal("1", metrics["jobs_skipped_status_from_trusted_skip"]);
        Assert.Equal("1", metrics["tasks_with_trusted_skip_allowed_sent"]);
        Assert.Equal("1", metrics["tasks_with_customer_trust_status_sent"]);

        // The row with a risk flag is excluded, exactly as the runtime counter excludes it.
        Assert.Equal("2", metrics["tasks_matching_retired_skip_shape"]);
        Assert.NotNull(metrics["retired_decision_first_seen"]);
    }

    /// <summary>
    /// Executes the checked-in preflight against the migrated test schema. The psql meta-commands
    /// are dropped rather than emulated — they only frame the output for a human reading a
    /// terminal, and a test that needed them would be testing psql instead of the query.
    /// </summary>
    private static async Task<Dictionary<string, string?>> RunPreflightAsync(
        IDbContextFactory<IvrDbContext> factory)
    {
        string sql = await File.ReadAllTextAsync(FindRepositoryFile(
            "tools", "ops", "od18-legacy-skip-preflight.sql"));
        // Strip psql meta-commands and comments BEFORE splitting on the statement separator.
        // Dropping comments is not cosmetic: a semicolon inside English prose — "Run it per
        // environment; the numbers decide..." — splits mid-sentence and hands Postgres the second
        // half as a statement. The runner must not be breakable by someone writing a comment well.
        string[] statements = [.. string
            .Join(
                '\n',
                sql.Split('\n')
                    .Where(line => !line.TrimStart().StartsWith('\\'))
                    .Select(line => line.Split("--", StringSplitOptions.None)[0]))
            .Split(';')
            .Select(statement => statement.Trim())
            .Where(statement => statement.Contains("SELECT", StringComparison.Ordinal))];

        Assert.Equal(9, statements.Length);

        var metrics = new Dictionary<string, string?>(StringComparer.Ordinal);
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        System.Data.Common.DbConnection connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        foreach (string statement in statements)
        {
            await using System.Data.Common.DbCommand command = connection.CreateCommand();
            command.CommandText = statement;
            await using System.Data.Common.DbDataReader reader = await command.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync(), $"preflight statement returned no row: {statement}");
            metrics[reader.GetString(0)] = await reader.IsDBNullAsync(1)
                ? null
                : reader.GetValue(1).ToString();
        }

        return metrics;
    }

    private static string FindRepositoryFile(params string[] segments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string candidate = segments.Aggregate(directory.FullName, Path.Combine);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(Path.Combine(segments));
    }

    [Theory]
    [Trait("TestId", "IT-M3-AUTHORITY-07")]
    [InlineData("legacy-empty-evaluated", "[]", true)]
    [InlineData("risk-flagged", """["COD_FAIL_HISTORY"]""", true)]
    [InlineData("new-customer", """["NEW_CUSTOMER","VERIFIED_ORDER_COUNT_0"]""", true)]
    [InlineData("unevaluated", "[]", false)]
    public async Task TrustAndRiskMetadataNeverChangeTheEligibilityDecision(
        string caseId,
        string riskFlagsJson,
        bool riskEvidenceAvailable)
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPendingTaskAsync(
            factory,
            $"TASK-ELIG-TRUST-15-{caseId}",
            $"JOB-ELIG-TRUST-15-{caseId}",
            "CREATED",
            "READY_FOR_ELIGIBILITY",
            eligibilitySnapshotJson: JsonSerializer.Serialize(new
            {
                decision = "ELIGIBLE",
                source_version = "sales-eligibility-v1",
                captured_at = Now.AddSeconds(-30),
                source_available = true,
                trust = new { risk_evidence_available = riskEvidenceAvailable },
            }),
            riskFlagsJson: riskFlagsJson);
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new CapacityAvailableProvider(),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            $"TASK-ELIG-TRUST-15-{caseId}",
            $"corr-elig-trust-15-{caseId}");

        Assert.True(result.Eligible);
        Assert.Equal(EligibilityDecisions.Eligible, result.Decision);
        Assert.Empty(result.Advisories);
    }

    private static string VoiceSnapshotFor(string caseId)
    {
        object voice = caseId switch
        {
            "restricted" => new { restricted = true, source_available = true, source_version = "sales-voice-v1" },
            "resolver-down" => new { restricted = false, source_available = false, source_version = "sales-voice-v1" },
            _ => new { restricted = false, source_available = true, source_version = "sales-voice-v1" },
        };

        if (caseId != "marketing-noise")
        {
            return JsonSerializer.Serialize(new
            {
                decision = "ELIGIBLE",
                source_version = "sales-eligibility-v1",
                captured_at = Now.AddSeconds(-30),
                source_available = true,
                voice_restriction = voice,
            });
        }

        // Same allowed voice decision, but the bag also carries every marketing-consent signal
        // set to its most restrictive value. None of them may reach the voice decision.
        return JsonSerializer.Serialize(new
        {
            decision = "ELIGIBLE",
            source_version = "sales-eligibility-v1",
            captured_at = Now.AddSeconds(-30),
            source_available = true,
            voice_restriction = voice,
            sms_opt_out = true,
            marketing_consent = false,
            email_opt_out = true,
            newsletter_subscribed = false,
            promo_calls_allowed = false,
        });
    }

    [Fact]
    [Trait("TestId", "PT-FAILCLOSED-03")]
    public async Task WhenTheCapacitySourceIsDownUnderLoadEveryTaskIsHeldAndNoneDispatches()
    {
        // W-0037 / P5-3 §6.4, DO-06. The failure mode that matters is the permissive one: a
        // capacity source that is slow or down must never be read as "plenty of room".
        // Concurrency is the interesting case, because a race is where an "unknown" most easily
        // gets rounded up to "fine".
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        const int taskCount = 12;
        for (int index = 0; index < taskCount; index++)
        {
            string suffix = index.ToString("D3", System.Globalization.CultureInfo.InvariantCulture);
            await SeedPendingTaskAsync(
                factory,
                string.Concat("TASK-PT-FC-", suffix),
                string.Concat("JOB-PT-FC-", suffix),
                "CREATED",
                "READY_FOR_ELIGIBILITY");
        }

        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new UnavailableCapacityProvider(),
            new FixedTimeProvider(Now));

        EligibilityEvaluation[] evaluations = await Task.WhenAll(
            Enumerable.Range(0, taskCount).Select(index => service.EvaluateAsync(
                string.Concat(
                    "TASK-PT-FC-",
                    index.ToString("D3", System.Globalization.CultureInfo.InvariantCulture)),
                string.Concat(
                    "corr-pt-fc-",
                    index.ToString(System.Globalization.CultureInfo.InvariantCulture)))));

        Assert.All(evaluations, evaluation =>
        {
            Assert.False(evaluation.Eligible);
            Assert.False(evaluation.IsCountedCustomerAttempt);
            Assert.Equal(
                EligibilityReasonCodes.CapacitySourceUnavailable,
                Assert.Single(evaluation.Reasons).Code);
        });

        // Not one dispatch, and every task still present and accounted for.
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(0, await verification.CallAttempts.CountAsync());
        Assert.Equal(taskCount, await verification.CallJobs.CountAsync());
    }

    [Fact]
    [Trait("TestId", "SEC-PII-04")]
    public async Task NothingWrittenDuringALoadRunCarriesAPhoneNumberOrAStreetAddress()
    {
        // W-0037 / P5-3 §6.5, D-05. The CI gate scans FILES. Nothing scanned what the service
        // actually wrote to the database, which is where a leak would land at runtime — an audit
        // payload, an evidence ref or a review reason built from a customer field.
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        const int taskCount = 8;
        for (int index = 0; index < taskCount; index++)
        {
            string suffix = index.ToString("D3", System.Globalization.CultureInfo.InvariantCulture);
            await SeedPendingTaskAsync(
                factory,
                string.Concat("TASK-SEC-PII-", suffix),
                string.Concat("JOB-SEC-PII-", suffix),
                "CREATED",
                "READY_FOR_ELIGIBILITY");
        }

        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new CapacityShortageProvider(),
            new FixedTimeProvider(Now));
        await Task.WhenAll(Enumerable.Range(0, taskCount).Select(index => service.EvaluateAsync(
            string.Concat(
                "TASK-SEC-PII-",
                index.ToString("D3", System.Globalization.CultureInfo.InvariantCulture)),
            string.Concat(
                "corr-sec-pii-",
                index.ToString(System.Globalization.CultureInfo.InvariantCulture)))));

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        // Materialise first: DataJson is a json column, so composing the string in the query
        // would ask PostgreSQL to concatenate json with text.
        var written = new List<string>();
        written.AddRange((await verification.AuditLog.AsNoTracking().ToListAsync())
            .Select(row => string.Join(' ', row.DataJson, row.TargetId, row.Reason ?? "")));
        written.AddRange((await verification.Evidence.AsNoTracking().ToListAsync())
            .Select(row => string.Join(' ', row.EvidenceRef, row.PayloadRef ?? "")));
        written.AddRange((await verification.EvidenceLinks.AsNoTracking().ToListAsync())
            .Select(row => row.EvidenceRef));
        written.AddRange((await verification.ReviewItems.AsNoTracking().ToListAsync())
            .Select(row => string.Join(' ', row.Reason, row.SourceId)));
        written.AddRange((await verification.CapacityIncidents.AsNoTracking().ToListAsync())
            .Select(row => string.Join(' ', row.CapacityIncidentId, row.ShortageReason ?? "")));

        Assert.NotEmpty(written);
        foreach (string value in written)
        {
            // Same guard the runtime uses on its own writes, applied from the outside to what
            // actually landed. A raw MSISDN or a semantic street address fails it.
            Assert.True(
                Ivr.Domain.Privacy.PiiGuard.IsSafeText(value),
                $"A persisted row failed the PII guard: {value[..Math.Min(60, value.Length)]}");
        }
    }

    private sealed class UnavailableCapacityProvider : IEligibilityCapacityProvider
    {
        public ValueTask<EligibilityCapacitySnapshot> GetCapacityAsync(
            EligibilityTaskRecord task,
            DateTimeOffset evaluatedAt,
            CancellationToken cancellationToken = default) =>
            throw new TimeoutException("Capacity source did not answer within the budget.");
    }

    private static string SnapshotFor(string? scenario) => scenario switch
    {
        null => EligibleSnapshotJson,
        "blocked" => JsonSerializer.Serialize(new
        {
            decision = "BLOCKED",
            source_version = "sales-eligibility-v1",
            captured_at = Now.AddSeconds(-30),
            source_available = true,
        }),
        "stale" => JsonSerializer.Serialize(new
        {
            decision = "ELIGIBLE",
            source_version = "sales-eligibility-v1",
            captured_at = Now.AddHours(-3),
            source_available = true,
        }),
        "unavailable" => JsonSerializer.Serialize(new
        {
            decision = "ELIGIBLE",
            source_version = "sales-eligibility-v1",
            captured_at = Now.AddSeconds(-30),
            source_available = false,
        }),
        "unreadable" => "[\"not-an-object\"]",
        _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
    };

    [Fact]
    [Trait("TestId", "IT-ELIG-SCHED-09")]
    public async Task SchedulerCapacitySourceUnavailableFailsClosedWithoutAttempt()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPendingTaskAsync(
            factory,
            "TASK-ELIG-SCHED-09",
            "JOB-ELIG-SCHED-09",
            "CREATED",
            "READY_FOR_ELIGIBILITY");
        var service = new EligibilityService(
            new PostgresEligibilityRepository(factory),
            new SchedulerEligibilityCapacityProvider(
                new UnavailableSchedulerCapacityService()),
            new FixedTimeProvider(Now));

        EligibilityEvaluation result = await service.EvaluateAsync(
            "TASK-ELIG-SCHED-09",
            "corr-elig-sched-09");

        Assert.False(result.Eligible);
        Assert.Equal(EligibilityDecisions.HeldAdminReview, result.Decision);
        Assert.Equal(
            EligibilityReasonCodes.CapacitySourceUnavailable,
            Assert.Single(result.Reasons).Code);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(0, await verification.CallAttempts.CountAsync());
        Assert.Equal(0, await verification.CapacityIncidents.CountAsync());
        Assert.Equal(
            "TASK-ELIG-SCHED-09",
            (await verification.ReviewItems.AsNoTracking().SingleAsync()).SourceId);
    }

    /// <summary>
    /// ARCH-05 §1, the "before attempt" rows: CRM do-not-call, Evidence Registry, and the
    /// contact half of Trust/Contact resolver. Each promises the same thing —
    /// the source cannot answer, so IVR does not dispatch.
    /// <para>
    /// Every test above proves the first half: the decision is a hold, and no attempt row exists
    /// yet. None proved the second, because none ever ran the scheduler afterwards. "No attempt
    /// exists" is a statement about the past; "no call will be placed" is a claim about a separate
    /// component with its own claim query and its own predicates.
    /// </para>
    /// <para>
    /// THREE guards stand between a hold and a dispatch, and the behavioural assertion alone
    /// cannot tell them apart — which the first negative check proved by surviving: dropping
    /// <c>job.eligible IS TRUE</c> from the claim query changed nothing, because the status and
    /// queue-status predicates still refused the row on their own. A check that would have stayed
    /// green through that regression is not the check its own comment claimed to be. So each guard
    /// is asserted BY NAME as well, and a change to any one of them fails with a sentence naming
    /// it rather than with a silence that some other predicate happened to cover.
    /// </para>
    /// </summary>
    [Theory]
    [Trait("TestId", "IT-ELIG-NODISPATCH-15")]
    [InlineData("crm-do-not-call", EligibilityReasonCodes.PhoneCallRestrictionSourceUnavailable)]
    [InlineData("evidence-registry", EligibilityReasonCodes.EvidenceMissing)]
    [InlineData("eligibility-source", EligibilityReasonCodes.EligibilitySourceUnavailable)]
    public async Task AHeldTaskIsNeverHandedToTheSchedulerForDispatch(
        string row,
        string expectedReason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(row);
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        string taskId = $"TASK-NODISPATCH-{row.ToUpperInvariant()}";
        await SeedUnavailableSourceAsync(factory, taskId, row);
        await SeedDispatchChannelAsync(factory, $"SIM-{row}");

        EligibilityEvaluation evaluation = await EvaluatorFor(factory)
            .EvaluateAsync(taskId, $"corr-nodispatch-{row}");

        Assert.False(evaluation.Eligible, $"{row}: still eligible with its source unavailable.");
        Assert.Contains(evaluation.Reasons, reason => reason.Code == expectedReason);

        // The half nothing measured: the component that actually places calls, asked directly.
        SchedulerDispatchLease? lease = await ClaimFor(factory);
        Assert.True(
            lease is null,
            $"{row}: the scheduler claimed {lease?.JobId} after the task was held. ARCH-05 says "
            + "this row does not dispatch, and a hold on its own does not stop it.");

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(0, await verification.CallAttempts.CountAsync());

        // Each guard named. The claim query needs eligible AND the ready status AND the queued
        // queue-status; the hold has to close all three, or the next person to relax one of them
        // finds out from production rather than from here.
        CallJobEntity held = await verification.CallJobs
            .AsNoTracking()
            .SingleAsync(job => job.TaskId == taskId);
        Assert.False(held.Eligible, $"{row}: the held job is still marked eligible.");
        Assert.Equal("HELD_ADMIN_REVIEW", held.Status);
        Assert.Equal("HELD_ADMIN_REVIEW", held.QueueStatus);
    }

    [Fact]
    [Trait("TestId", "IT-ELIG-NODISPATCH-15")]
    public async Task TheSameSchedulerDoesClaimAHealthyTask()
    {
        // The control, and the theory above means nothing without it. "The scheduler claimed
        // nothing" is also exactly what a scheduler that CANNOT claim anything looks like — a
        // missing channel, a window already closed, a policy row nobody seeded. Proving it claims
        // here, with the same seeder and the same store, is what turns four refusals into evidence.
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedUnavailableSourceAsync(factory, "TASK-NODISPATCH-CONTROL", "none");
        await SeedDispatchChannelAsync(factory, "SIM-control");

        EligibilityEvaluation evaluation = await EvaluatorFor(factory)
            .EvaluateAsync("TASK-NODISPATCH-CONTROL", "corr-nodispatch-control");

        Assert.True(
            evaluation.Eligible,
            "the control task was held: "
            + string.Join(", ", evaluation.Reasons.Select(reason => reason.Code)));

        SchedulerDispatchLease lease = Assert.IsType<SchedulerDispatchLease>(await ClaimFor(factory));
        Assert.Equal("JOB-TASK-NODISPATCH-CONTROL", lease.JobId);
    }

    private static EligibilityService EvaluatorFor(IDbContextFactory<IvrDbContext> factory) => new(
        new PostgresEligibilityRepository(factory),
        new SchedulerEligibilityCapacityProvider(
            new MockSchedulerCapacityService(Options.Create(new SchedulerOptions
            {
                Enabled = true,
                MockChannelCount = 4,
                ExpectedCallDurationSeconds = 30,
            }))),
        new FixedTimeProvider(Now));

    private static Task<SchedulerDispatchLease?> ClaimFor(IDbContextFactory<IvrDbContext> factory) =>
        new PostgresSchedulerStore(factory, new FixedTimeProvider(Now))
            .TryClaimDueDispatchAsync(
                "nodispatch-worker",
                IvrOptions.LabRealSimExecutionMode,
                TimeSpan.FromMinutes(2));

    /// <summary>One row of the ARCH-05 matrix, expressed as the data Sales would have sent.</summary>
    private static Task SeedUnavailableSourceAsync(
        IDbContextFactory<IvrDbContext> factory,
        string taskId,
        string row)
    {
        string jobId = $"JOB-{taskId}";
        return row switch
        {
            // The do-not-call resolver saying, explicitly, that it could not answer. Not knowing
            // whether a number is on the list is not permission to dial it.
            "crm-do-not-call" => SeedPendingTaskAsync(
                factory, taskId, jobId, "CREATED", "READY_FOR_ELIGIBILITY",
                eligibilitySnapshotJson: JsonSerializer.Serialize(new
                {
                    decision = "ELIGIBLE",
                    source_version = "sales-eligibility-v1",
                    captured_at = Now.AddSeconds(-30),
                    source_available = true,
                    blockers = Array.Empty<string>(),
                    voice_restriction = new
                    {
                        source_available = false,
                        source_version = "sales-voice-v1",
                    },
                })),

            // No evidence reference means nothing to point at afterwards. A call that cannot be
            // evidenced cannot be defended, so it is not placed.
            "evidence-registry" => SeedPendingTaskAsync(
                factory, taskId, jobId, "CREATED", "READY_FOR_ELIGIBILITY",
                evidenceRefsJson: "[]"),

            // Sales telling IVR its own eligibility source was unreachable.
            "eligibility-source" => SeedPendingTaskAsync(
                factory, taskId, jobId, "CREATED", "READY_FOR_ELIGIBILITY",
                eligibilitySnapshotJson: JsonSerializer.Serialize(new
                {
                    decision = "ELIGIBLE",
                    source_version = "sales-eligibility-v1",
                    captured_at = Now.AddSeconds(-30),
                    source_available = false,
                    blockers = Array.Empty<string>(),
                })),

            _ => SeedPendingTaskAsync(
                factory, taskId, jobId, "CREATED", "READY_FOR_ELIGIBILITY"),
        };
    }

    private static async Task SeedDispatchChannelAsync(
        IDbContextFactory<IvrDbContext> factory,
        string channelId)
    {
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        context.SimChannels.Add(new SimChannelEntity
        {
            SimChannelId = channelId,
            SimNumberRef = $"sim-ref-{channelId}",
            Enabled = true,
            Status = "IDLE",
            AdapterMode = "VENDOR",
            ExecutionMode = IvrOptions.LabRealSimExecutionMode,
            ProviderName = "VENDOR",
            LastHealthCheckAt = Now,
        });
        await context.SaveChangesAsync();
    }

    private static async Task SeedPendingTaskAsync(
        IDbContextFactory<IvrDbContext> factory,
        string taskId,
        string jobId,
        string jobStatus,
        string outboxStatus,
        bool callRestriction = false,
        string? eligibilitySnapshotJson = null,
        bool omitSnapshotHash = false,
        string? customerTrustStatus = null,
        // LEGACY_READ fixture input retained for OD-15 rollback rows. OD-18 runtime ignores it;
        // nullable preserves the historical wire/database shape without assigning active meaning.
        bool? trustedSkipAllowed = null,
        string? evidenceRefsJson = null,
        string? riskFlagsJson = null)
    {
        DateTimeOffset startedAt = Now.AddMinutes(-1);
        DateTimeOffset expiresAt = Now.AddMinutes(4);
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        context.ConfirmationTasks.Add(new ConfirmationTaskEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            ContractVersion = "ivr-order-confirmation.v1",
            // Derived from the task id: the hardcoded key was fine while every test seeded one
            // task, and became a unique-index collision the moment a load test seeded twelve.
            IdempotencyKey = string.Concat("order-core:", taskId, ":idem"),
            CorrelationId = "corr-elig-cap-05",
            OfficialOrderId = "ORDER-ELIG-CAP-05",
            OrderCode = "GF-CAP-05",
            OrderVersion = "1",
            OrderState = "CONFIRMING",
            PaymentMethodSnapshot = "ONLINE",
            IvrConfirmationRequired = true,
            CustomerTrustStatus = customerTrustStatus,
            TrustedSkipAllowed = trustedSkipAllowed,
            RiskFlagsJson = riskFlagsJson ?? "[]",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyVersion = "gh-v1-candidate",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowStartedAt = startedAt,
            ConfirmationWindowExpiresAt = expiresAt,
            PhoneRef = "phone-ref-elig-cap-05",
            PhoneMasked = "84xxxxx0005",
            PhoneValidationStatus = "VALID",
            DialTokenCiphertext = "enc:integration-cap-05",
            DialTokenExpiresAt = expiresAt,
            PrivacySafeOrderSummaryJson = "{}",
            CallScriptTemplateId = "SCRIPT-ORDER-CONFIRM",
            CallScriptVersion = "v1-test-approved",
            EvidencePolicyVersion = "test-evidence-v1",
            PrivacyPolicyVersion = "test-privacy-v1",
            EligibilityDecision = null,
            EligibilitySnapshotJson = eligibilitySnapshotJson ?? EligibleSnapshotJson,
            EligibilitySnapshotHash = omitSnapshotHash
                ? null
                : DeterministicSnapshotHasher.Compute(
                    eligibilitySnapshotJson ?? EligibleSnapshotJson),
            CallRestriction = callRestriction,
            NotForQuoteCartDraft = true,
            NoDirectOrderUpdate = true,
            CreatedAt = startedAt,
            ExpiresAt = expiresAt,
            AcceptedAt = startedAt,
            EvidenceRefsJson = evidenceRefsJson ?? "[\"evidence://integration/p2-2/task\"]",
        });
        context.CallJobs.Add(new CallJobEntity
        {
            IvrCallJobId = jobId,
            TaskId = taskId,
            OfficialOrderId = "ORDER-ELIG-CAP-05",
            OrderVersionSnapshot = "1",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyCode = "gh-v1-candidate",
            Status = jobStatus,
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowSeconds = 300,
            AttemptScheduleJson = JsonSerializer.Serialize(new[]
            {
                startedAt,
                startedAt.AddSeconds(150),
            }),
            T0At = startedAt,
            ExpiresAt = expiresAt,
            Eligible = false,
            EligibilityDecision = EligibilityDecisions.Pending,
            QueueStatus = "HELD_ELIGIBILITY",
            ScriptVersion = "SCRIPT-ORDER-CONFIRM:v1-test-approved",
            PrivacyPolicyVersion = "test-privacy-v1",
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            CreatedAt = startedAt,
            EvidenceRefsJson = "[\"evidence://integration/p2-2/task\"]",
        });
        context.TaskIntakeOutbox.Add(new TaskIntakeOutboxEntity
        {
            OutboxId = Guid.NewGuid(),
            TaskId = taskId,
            IvrCallJobId = jobId,
            EventType = "IVR_TASK_READY_FOR_ELIGIBILITY",
            Status = outboxStatus,
            CorrelationId = "corr-elig-cap-05",
            PayloadSha256 = new string('A', 64),
            CreatedAt = startedAt,
        });
        await context.SaveChangesAsync();
    }

    private sealed class CapacityShortageProvider : IEligibilityCapacityProvider
    {
        public ValueTask<EligibilityCapacitySnapshot> GetCapacityAsync(
            EligibilityTaskRecord task,
            DateTimeOffset evaluatedAt,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(new EligibilityCapacitySnapshot(
                true,
                false,
                "SESSION-CAP-05",
                1,
                20,
                0,
                1,
                "NO_CHANNEL_BEFORE_DEADLINE",
                "evidence://integration/p2-2/capacity-shortage"));
    }

    private sealed class CapacityAvailableProvider : IEligibilityCapacityProvider
    {
        public ValueTask<EligibilityCapacitySnapshot> GetCapacityAsync(
            EligibilityTaskRecord task,
            DateTimeOffset evaluatedAt,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(new EligibilityCapacitySnapshot(
                true,
                true,
                "SESSION-MOCK-06",
                1,
                0,
                0,
                0,
                null,
                "evidence://integration/p2-2/capacity-available"));
    }

    private sealed class UnexpectedCapacityProvider : IEligibilityCapacityProvider
    {
        public ValueTask<EligibilityCapacitySnapshot> GetCapacityAsync(
            EligibilityTaskRecord task,
            DateTimeOffset evaluatedAt,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException(
                "Capacity must not be evaluated after a voice restriction blocks the task.");
    }

    private sealed class MissingEvidenceCapacityProvider : IEligibilityCapacityProvider
    {
        public ValueTask<EligibilityCapacitySnapshot> GetCapacityAsync(
            EligibilityTaskRecord task,
            DateTimeOffset evaluatedAt,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(new EligibilityCapacitySnapshot(
                true,
                true,
                "SESSION-MISSING-EVIDENCE",
                1,
                0,
                0,
                0,
                null,
                string.Empty));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
