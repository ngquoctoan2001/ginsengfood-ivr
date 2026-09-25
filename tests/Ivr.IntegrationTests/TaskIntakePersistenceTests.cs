using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Policies;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Observability;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Persistence.Security;
using Ivr.Infrastructure.Scripts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Telephony;

namespace Ivr.IntegrationTests;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class TaskIntakePersistenceTests(PostgresPersistenceFixture fixture)
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 13, 6, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "IT-INTAKE-DB-01")]
    public async Task ConcurrentIntakePersistsExactlyOneAtomicUnitAndImmutableSnapshots()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPoliciesAsync(factory);
        var clock = new FixedTimeProvider(Now);
        var store = new PostgresTaskIntakeStore(factory, clock);
        var service = new TaskIntakeService(
            store,
            new PostgresAttemptPolicyRegistry(factory),
            new PostgresScriptRegistry(
                factory,
                clock,
                Options.Create(new ScriptContentOptions())),
            new MockOnlyOpaqueValueProtector(),
            SpeechSummaryLimits.Create(100, 100),
            clock,
            new CallingWindow(Options.Create(new CallingWindowOptions())),
            Options.Create(new IvrOptions
            {
                ExecutionMode = IvrOptions.MockExecutionMode,
                SalesProvider = "FAKE_TARGET_V1",
                SimProvider = "MOCK",
                RealCustomerCallAllowed = false,
            }));
        IvrConfirmationTaskV1 source = CreateTask();
        var command = new TaskIntakeCommand(
            source,
            "idem-postgres-p2-1",
            source.Correlation_id!,
            new string('A', 64),
            ExecutionMode.Mock);
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == IvrTelemetry.ServiceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded,
        };
        ActivitySource.AddActivityListener(activityListener);

        TaskIntakeOutcome[] outcomes = await Task.WhenAll(
            Enumerable.Range(0, 8).Select(_ => service.IntakeAsync(command)));

        Assert.Single(outcomes.Select(outcome => outcome.IvrCallJobId).Distinct());
        Assert.All(outcomes, outcome => Assert.Equal(
            TaskIntakeDecisions.AcceptedDryRunOnly,
            outcome.Decision));
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(1, await verification.ConfirmationTasks.CountAsync());
        Assert.Equal(1, await verification.CallJobs.CountAsync());
        Assert.Equal(1, await verification.TaskIntakeOutbox.CountAsync());
        Assert.Equal(1, await verification.IdempotencyKeys.CountAsync());
        Assert.Equal(1, await verification.AuditLog.CountAsync());
        ConfirmationTaskEntity task = await verification.ConfirmationTasks
            .AsNoTracking()
            .SingleAsync();
        Assert.NotNull(task.TraceParent);
        Assert.True(ActivityContext.TryParse(
            task.TraceParent,
            task.TraceState,
            isRemote: true,
            out ActivityContext persistedContext));
        Assert.NotEqual(default, persistedContext.TraceId);
        Assert.Equal("SCRIPT-ORDER-CONFIRM", task.CallScriptTemplateId);
        Assert.Equal(
            Ivr.Domain.Scripts.TargetV1SpeechPolicy.MockTemplateVersion,
            task.CallScriptVersion);
        Assert.StartsWith("enc:mock-sha256:", task.DialTokenCiphertext,
            StringComparison.Ordinal);
        Assert.DoesNotContain(source.Dial_token!, task.DialTokenCiphertext,
            StringComparison.Ordinal);
        TaskIntakeOutboxEntity outbox = await verification.TaskIntakeOutbox
            .AsNoTracking()
            .SingleAsync();
        Assert.Equal("HELD_MOCK", outbox.Status);
        Assert.Equal(new string('A', 64), outbox.PayloadSha256);

        await verification.Database.OpenConnectionAsync();
        await using (var defaultCommand = new NpgsqlCommand(
            """
            SELECT count(*)
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = 'ivr_confirmation_tasks'
              AND column_name IN (
                  'call_script_template_id',
                  'call_script_version',
                  'evidence_policy_version',
                  'privacy_policy_version',
                  'trace_parent',
                  'trace_state')
              AND column_default IS NOT NULL
            """,
            (NpgsqlConnection)verification.Database.GetDbConnection()))
        {
            Assert.Equal(0L, await defaultCommand.ExecuteScalarAsync());
        }

        await Assert.ThrowsAsync<PostgresException>(() =>
            verification.Database.ExecuteSqlRawAsync(
                "UPDATE ivr_confirmation_tasks SET call_script_version = 'tampered'"));
    }

    [Fact]
    [Trait("TestId", "IT-INTAKE-DB-02")]
    public async Task RestrictedTaskIsRejectedBeforePersistence()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPoliciesAsync(factory);
        var clock = new FixedTimeProvider(Now);
        var service = new TaskIntakeService(
            new PostgresTaskIntakeStore(factory, clock),
            new PostgresAttemptPolicyRegistry(factory),
            new PostgresScriptRegistry(
                factory,
                clock,
                Options.Create(new ScriptContentOptions())),
            new MockOnlyOpaqueValueProtector(),
            SpeechSummaryLimits.Create(100, 100),
            clock,
            new CallingWindow(Options.Create(new CallingWindowOptions())),
            Options.Create(new IvrOptions()));
        IvrConfirmationTaskV1 source = CreateTask(callRestriction: true);

        TaskIntakeOutcome outcome = await service.IntakeAsync(new TaskIntakeCommand(
            source,
            "idem-postgres-rejected",
            source.Correlation_id!,
            new string('B', 64),
            ExecutionMode.Mock));

        Assert.Equal(TaskIntakeDecisions.BlockedOperational, outcome.Decision);
        Assert.Equal(Ivr.Domain.Errors.IvrErrorCodes.OperationalBlocked, outcome.FailureCode);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(0, await verification.ConfirmationTasks.CountAsync());
        Assert.Equal(0, await verification.CallJobs.CountAsync());
        Assert.Equal(0, await verification.TaskIntakeOutbox.CountAsync());
        Assert.Equal(1, await verification.IdempotencyKeys.CountAsync());
        AuditLogEntity audit = await verification.AuditLog.SingleAsync();
        Assert.DoesNotContain(source.Dial_token!, audit.DataJson, StringComparison.Ordinal);
        Assert.DoesNotContain(source.Phone_ref, audit.DataJson, StringComparison.Ordinal);
    }

    /// <summary>
    /// Three layers bound <c>dial_token_expires_at</c>, and their intersection is a single point.
    /// <para>
    /// Intake refuses an expiry BEFORE the window end (<c>ContactRejectionReason</c>). Persistence
    /// refuses one AFTER it (<c>PersistenceInvariantValidator.ValidateTask</c>). Dispatch refuses
    /// one after <c>lease.Deadline</c> a third time
    /// (<c>PostgresTelephonyDispatchStore.LoadAsync</c>), and that deadline is <c>job.expires_at</c>,
    /// which intake sets from the same window end - so the third guard restates the second rather
    /// than adding a bound. Together: the only accepted value is equality.
    /// </para>
    /// <para>
    /// The asymmetry is the point of this test. Too early is refused cleanly at the edge with a
    /// reason code Module 3 can read. Too late passes the contact gate and fails inside the
    /// transaction instead, which is the worst place for a producer to discover a contract.
    /// </para>
    /// <para>
    /// <c>OD-V1-17</c> closed on 2026-09-05 with TTL = window + 60s, and the middle case below is
    /// that number failing. On 2026-09-09 the owner replaced that clause: the TTL is equality, the
    /// value all three guards already admit (W-0246). So this test changed meaning without changing
    /// a line - it used to record that a signed decision could not be implemented, and now it holds
    /// the signed decision itself. Still meant to fail if the TTL moves again; update it
    /// deliberately rather than deleting it.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-INTAKE-DB-03")]
    public async Task DialTokenExpiryIsBoundToExactlyTheConfirmationWindowEnd()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPoliciesAsync(factory);
        var clock = new FixedTimeProvider(Now);
        TaskIntakeService service = CreateService(factory, clock);

        // Equality: the one value every layer accepts.
        TaskIntakeOutcome accepted = await service.IntakeAsync(new TaskIntakeCommand(
            CreateTask(taskId: "TASK-PG-TTL-EXACT"),
            "idem-ttl-exact",
            "corr-postgres-p2-1",
            new string('E', 64),
            ExecutionMode.Mock));

        Assert.Equal(TaskIntakeDecisions.AcceptedDryRunOnly, accepted.Decision);

        // Window + 60s: the clause OD-V1-17 carried until 2026-09-09. Until W-0302 the contact gate
        // let it through and the persistence invariant threw inside the transaction, which
        // ErrorEnvelopeMiddleware turned into 500 IVR_INTERNAL_ERROR - the one answer a producer is
        // entitled to retry, for a payload that could never be accepted. It is refused at the edge
        // now, symmetrically with the early case below.
        TaskIntakeOutcome tooLate = await service.IntakeAsync(new TaskIntakeCommand(
            CreateTask(
                taskId: "TASK-PG-TTL-LATE",
                dialTokenExpiryOffsetFromWindowEnd: TimeSpan.FromSeconds(60)),
            "idem-ttl-late",
            "corr-postgres-p2-1",
            new string('F', 64),
            ExecutionMode.Mock));

        Assert.Equal(TaskIntakeDecisions.RejectedContactInvalid, tooLate.Decision);
        Assert.Equal(
            new[] { EligibilityReasonCodes.DialTokenExpiresAfterWindow },
            tooLate.BlockedReasons);

        // Window - 60s: refused at the edge instead, with a reason code and no partial write.
        TaskIntakeOutcome tooEarly = await service.IntakeAsync(new TaskIntakeCommand(
            CreateTask(
                taskId: "TASK-PG-TTL-EARLY",
                dialTokenExpiryOffsetFromWindowEnd: TimeSpan.FromSeconds(-60)),
            "idem-ttl-early",
            "corr-postgres-p2-1",
            new string('G', 64),
            ExecutionMode.Mock));

        Assert.Equal(TaskIntakeDecisions.RejectedContactInvalid, tooEarly.Decision);
        Assert.Equal(
            new[] { EligibilityReasonCodes.DialTokenExpiresBeforeWindow },
            tooEarly.BlockedReasons);

        // Only the equality case reached the database.
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(
            ["TASK-PG-TTL-EXACT"],
            await verification.ConfirmationTasks
                .Select(task => task.TaskId)
                .ToArrayAsync());
    }

    // W-0312. A number of the contract's shape. Nothing in this suite dials.
    private const string SentNumber = "+84900000001";

    [Fact]
    [Trait("TestId", "IT-INTAKE-NUMBER-DB-01")]
    public async Task ANumberOnlyTaskPersistsItsNumberAndItsOwnDirectDialReference()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPoliciesAsync(factory);
        TaskIntakeService service = CreateService(
            factory,
            new FixedTimeProvider(Now),
            new UnavailableOpaqueValueProtector());
        IvrConfirmationTaskV1 source = CreateTask(
            taskId: "TASK-PG-NUMBER",
            includeToken: false,
            phoneE164: SentNumber);

        TaskIntakeOutcome outcome = await service.IntakeAsync(new TaskIntakeCommand(
            source,
            "idem-pg-number",
            source.Correlation_id!,
            new string('1', 64),
            ExecutionMode.Mock));

        Assert.Equal(TaskIntakeDecisions.AcceptedDryRunOnly, outcome.Decision);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        ConfirmationTaskEntity task = await verification.ConfirmationTasks
            .AsNoTracking()
            .SingleAsync();
        Assert.Equal(SentNumber, task.PhoneE164);
        Assert.Equal(ExpectedDirectReference("TASK-PG-NUMBER"), task.DialTokenCiphertext);
        Assert.Equal(source.Confirmation_window_expires_at, task.DialTokenExpiresAt);
    }

    /// <summary>
    /// W-0312. Where no protector exists a token beside a number is validated and then dropped: the
    /// row keeps the task's direct-dial reference, so a value nothing could protect never reaches
    /// the database.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-INTAKE-NUMBER-DB-02")]
    public async Task ATokenBesideANumberIsNeverStoredWhereNoProtectorExists()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPoliciesAsync(factory);
        TaskIntakeService service = CreateService(
            factory,
            new FixedTimeProvider(Now),
            new UnavailableOpaqueValueProtector());
        IvrConfirmationTaskV1 source = CreateTask(
            taskId: "TASK-PG-NUMBER-TOKEN",
            phoneE164: SentNumber);

        TaskIntakeOutcome outcome = await service.IntakeAsync(new TaskIntakeCommand(
            source,
            "idem-pg-number-token",
            source.Correlation_id!,
            new string('2', 64),
            ExecutionMode.Mock));

        Assert.Equal(TaskIntakeDecisions.AcceptedDryRunOnly, outcome.Decision);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        ConfirmationTaskEntity task = await verification.ConfirmationTasks
            .AsNoTracking()
            .SingleAsync();
        Assert.Equal(ExpectedDirectReference("TASK-PG-NUMBER-TOKEN"), task.DialTokenCiphertext);
        Assert.DoesNotContain(source.Dial_token!, task.DialTokenCiphertext, StringComparison.Ordinal);
        Assert.Equal(SentNumber, task.PhoneE164);
    }

    /// <summary>
    /// W-0312 changed nothing where a protector exists. LAB dials the alias its fingerprint maps to,
    /// so a token beside a number must still be protected there, exactly as before.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-INTAKE-NUMBER-DB-03")]
    public async Task ATokenBesideANumberIsStillProtectedWhereAProtectorExists()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPoliciesAsync(factory);
        TaskIntakeService service = CreateService(factory, new FixedTimeProvider(Now));
        IvrConfirmationTaskV1 source = CreateTask(
            taskId: "TASK-PG-NUMBER-PROTECTED",
            phoneE164: SentNumber);

        TaskIntakeOutcome outcome = await service.IntakeAsync(new TaskIntakeCommand(
            source,
            "idem-pg-number-protected",
            source.Correlation_id!,
            new string('3', 64),
            ExecutionMode.Mock));

        Assert.Equal(TaskIntakeDecisions.AcceptedDryRunOnly, outcome.Decision);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        ConfirmationTaskEntity task = await verification.ConfirmationTasks
            .AsNoTracking()
            .SingleAsync();
        Assert.StartsWith("enc:mock-sha256:", task.DialTokenCiphertext, StringComparison.Ordinal);
        Assert.Equal(SentNumber, task.PhoneE164);
    }

    /// <summary>
    /// W-0312. The reason a direct-dial reference is one per task, run end to end: two number-only
    /// tasks through intake into Postgres, then each dialled against the durable ledger with what
    /// intake stored. The ledger binds a reference to the first task that dials with it, so if the
    /// two ever shared one the second would be refused as bound to another task.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-INTAKE-NUMBER-DB-04")]
    public async Task TwoNumberOnlyTasksCanEachDialAgainstTheDurableLedger()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPoliciesAsync(factory);
        TaskIntakeService service = CreateService(
            factory,
            new FixedTimeProvider(Now),
            new UnavailableOpaqueValueProtector());
        foreach (string suffix in new[] { "A", "B" })
        {
            IvrConfirmationTaskV1 source = CreateTask(
                taskId: string.Concat("TASK-PG-NUMBER-", suffix),
                orderId: string.Concat("ORDER-PG-NUMBER-", suffix),
                includeToken: false,
                phoneE164: SentNumber);
            TaskIntakeOutcome outcome = await service.IntakeAsync(new TaskIntakeCommand(
                source,
                string.Concat("idem-pg-number-", suffix),
                source.Correlation_id!,
                new string(suffix[0], 64),
                ExecutionMode.Mock));
            Assert.Equal(TaskIntakeDecisions.AcceptedDryRunOnly, outcome.Decision);
        }

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        ConfirmationTaskEntity[] tasks = await verification.ConfirmationTasks
            .AsNoTracking()
            .OrderBy(task => task.TaskId)
            .ToArrayAsync();
        Assert.Equal(2, tasks.Length);
        Assert.NotEqual(tasks[0].DialTokenCiphertext, tasks[1].DialTokenCiphertext);

        var ledger = new PostgresDialTokenResolveLedger(factory);
        foreach (ConfirmationTaskEntity task in tasks)
        {
            var decision = await ledger.EvaluateAsync(
                new DialTokenResolutionRequest(
                    DialTokenReference.Create(task.DialTokenCiphertext, task.DialTokenExpiresAt),
                    AttemptId.Create(string.Concat("attempt-", task.TaskId)),
                    TaskId.Create(task.TaskId),
                    MaxResolves: 3,
                    DirectPhoneE164: task.PhoneE164),
                Now);
            Assert.True(decision.Allowed, decision.RefusalCode);
        }
    }

    /// <summary>
    /// W-0354 / C15 (chief worklist 2026-09-25). IR-07's correction of 25/09 tells Module 3 that a task
    /// refused because the calling window is shut may be sent again from 08:00 under the same
    /// task_id, with a new window and a new Idempotency-Key - because the refusal stores nothing.
    /// That sentence is a promise to another team, so it is pinned here: the refused task leaves no
    /// row, and the morning send of the same task_id is accepted.
    /// </summary>
    [Fact]
    [Trait("TestId", "IT-INTAKE-NIGHT-REUSE-04")]
    public async Task ANightTaskLeavesNoRowSoItsTaskIdIsAcceptedInTheMorningUnderANewKey()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPoliciesAsync(factory);
        DateTimeOffset night = new(2026, 8, 13, 16, 0, 0, TimeSpan.Zero);   // 23:00 in Vietnam
        DateTimeOffset morning = new(2026, 8, 14, 1, 30, 0, TimeSpan.Zero); // 08:30 in Vietnam

        TaskIntakeOutcome refused = await CreateService(
                factory,
                new FixedTimeProvider(night),
                new UnavailableOpaqueValueProtector())
            .IntakeAsync(new TaskIntakeCommand(
                CreateTask(
                    taskId: "TASK-PG-NIGHT",
                    orderId: "ORDER-PG-NIGHT",
                    includeToken: false,
                    phoneE164: SentNumber,
                    windowStart: night.AddMinutes(-1)),
                "idem-pg-night-evening",
                "corr-postgres-p2-1",
                new string('7', 64),
                ExecutionMode.Mock));

        Assert.Equal(TaskIntakeDecisions.BlockedOperational, refused.Decision);
        Assert.Contains("CALLING_WINDOW_CLOSED_FOR_WHOLE_CONFIRMATION_WINDOW", refused.BlockedReasons);
        await using (IvrDbContext afterNight = await factory.CreateDbContextAsync())
        {
            Assert.Equal(0, await afterNight.ConfirmationTasks.CountAsync());
            Assert.Equal(0, await afterNight.CallJobs.CountAsync());
        }

        TaskIntakeOutcome accepted = await CreateService(
                factory,
                new FixedTimeProvider(morning),
                new UnavailableOpaqueValueProtector())
            .IntakeAsync(new TaskIntakeCommand(
                CreateTask(
                    taskId: "TASK-PG-NIGHT",
                    orderId: "ORDER-PG-NIGHT",
                    includeToken: false,
                    phoneE164: SentNumber,
                    windowStart: morning.AddMinutes(-1)),
                "idem-pg-night-morning",
                "corr-postgres-p2-1",
                new string('8', 64),
                ExecutionMode.Mock));

        Assert.Equal(TaskIntakeDecisions.AcceptedDryRunOnly, accepted.Decision);
        await using IvrDbContext afterMorning = await factory.CreateDbContextAsync();
        ConfirmationTaskEntity stored = await afterMorning.ConfirmationTasks.AsNoTracking().SingleAsync();
        Assert.Equal("TASK-PG-NIGHT", stored.TaskId);
    }

    private static string ExpectedDirectReference(string taskId) =>
        string.Concat(
            "enc:direct:",
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(taskId))));

    private static TaskIntakeService CreateService(
        IDbContextFactory<IvrDbContext> factory,
        TimeProvider clock,
        IOpaqueValueProtector? protector = null) =>
        new(
            new PostgresTaskIntakeStore(factory, clock),
            new PostgresAttemptPolicyRegistry(factory),
            new PostgresScriptRegistry(
                factory,
                clock,
                Options.Create(new ScriptContentOptions())),
            protector ?? new MockOnlyOpaqueValueProtector(),
            SpeechSummaryLimits.Create(100, 100),
            clock,
            new CallingWindow(Options.Create(new CallingWindowOptions())),
            Options.Create(new IvrOptions
            {
                ExecutionMode = IvrOptions.MockExecutionMode,
                SalesProvider = "FAKE_TARGET_V1",
                SimProvider = "MOCK",
                RealCustomerCallAllowed = false,
            }));

    [Fact]
    [Trait("TestId", "IT-INTAKE-DB-04")]
    public async Task PostgresNewKeyReevaluatesTransientPolicyHold()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await SeedPoliciesAsync(factory);
        var clock = new FixedTimeProvider(Now);
        var service = new TaskIntakeService(
            new PostgresTaskIntakeStore(factory, clock),
            new PostgresAttemptPolicyRegistry(factory),
            new PostgresScriptRegistry(factory, clock, Options.Create(new ScriptContentOptions())),
            new MockOnlyOpaqueValueProtector(),
            SpeechSummaryLimits.Create(100, 100),
            clock,
            new CallingWindow(Options.Create(new CallingWindowOptions())),
            Options.Create(new IvrOptions()));
        IvrConfirmationTaskV1 missing = CreateTask(policyVersion: "not-yet-present");

        TaskIntakeOutcome held = await service.IntakeAsync(new TaskIntakeCommand(
            missing,
            "idem-transient-db-1",
            missing.Correlation_id!,
            new string('C', 64),
            ExecutionMode.Mock));
        TaskIntakeOutcome accepted = await service.IntakeAsync(new TaskIntakeCommand(
            CreateTask(),
            "idem-transient-db-2",
            missing.Correlation_id!,
            new string('D', 64),
            ExecutionMode.Mock));

        Assert.Equal(TaskIntakeDecisions.HeldPolicyMissing, held.Decision);
        Assert.NotNull(accepted.IvrCallJobId);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(2, await verification.IdempotencyKeys.CountAsync());
        Assert.Equal(1, await verification.ConfirmationTasks.CountAsync());
    }

    private static async Task SeedPoliciesAsync(IDbContextFactory<IvrDbContext> factory)
    {
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        context.AttemptPolicies.AddRange(
            new AttemptPolicyEntity
            {
                PolicyVersion = CandidateAttemptPolicies.Version,
                ProgramType = "GOLDEN_HOUR",
                MaxAttempts = 2,
                AttemptOffsetsSecondsJson = "[0,150]",
                ConfirmationWindowSeconds = 300,
                AllowedExecutionModesJson = "[\"MOCK\",\"LAB_REAL_SIM\"]",
                ApprovedForProduction = false,
                CreatedAt = Now,
            },
            new AttemptPolicyEntity
            {
                PolicyVersion = CandidateAttemptPolicies.Version,
                ProgramType = "TWENTY_FOUR_SEVEN",
                MaxAttempts = 2,
                AttemptOffsetsSecondsJson = "[0,450]",
                ConfirmationWindowSeconds = 900,
                AllowedExecutionModesJson = "[\"MOCK\",\"LAB_REAL_SIM\"]",
                ApprovedForProduction = false,
                CreatedAt = Now,
            });
        await context.SaveChangesAsync();
    }

    private static IvrConfirmationTaskV1 CreateTask(
        bool callRestriction = false,
        string policyVersion = CandidateAttemptPolicies.Version,
        TimeSpan? dialTokenExpiryOffsetFromWindowEnd = null,
        string? taskId = null,
        string? orderId = null,
        bool includeToken = true,
        string? phoneE164 = null,
        DateTimeOffset? windowStart = null)
    {
        DateTimeOffset start = windowStart ?? Now.AddMinutes(-1);
        return new IvrConfirmationTaskV1
        {
            Contract_version = IvrConfirmationTaskV1Contract_version.IvrOrderConfirmation_v1,
            Task_id = taskId ?? (callRestriction ? "TASK-PG-REJECT" : "TASK-PG-ACCEPT"),
            Correlation_id = "corr-postgres-p2-1",
            Created_at = start,
            Order_id = orderId ?? (callRestriction ? "ORDER-PG-REJECT" : "ORDER-PG-ACCEPT"),
            Order_code = "GF-PG-001",
            Order_code_short = "PG001",
            Order_version = "17",
            Order_state = "CONFIRMING",
            Payment_method_snapshot = IvrConfirmationTaskV1Payment_method_snapshot.ONLINE,
            Ivr_confirmation_required = true,
            Is_ivr_callable = true,
            Program_code = ProgramCode.GOLDEN_HOUR,
            Confirmation_window_started_at = start,
            Confirmation_window_expires_at = start.AddMinutes(5),
            Attempt_policy_version = policyVersion,
            Max_customer_attempts = 2,
            Attempt_offsets_seconds = [0, 150],
            Phone_ref = "phone-ref-pg-p2-1",
            Phone_masked = "84xxxxx0001",
            Phone_validation_status = IvrConfirmationTaskV1Phone_validation_status.VALID,
            Dial_token = includeToken ? "dial-token-pg-p2-1" : null,
            Dial_token_expires_at = includeToken
                ? start.AddMinutes(5) + (dialTokenExpiryOffsetFromWindowEnd ?? TimeSpan.Zero)
                : null,
            Phone_e164 = phoneE164,
            Privacy_safe_order_summary = new Ivr.Contracts.Generated.IvrServer.V1.PrivacySafeOrderSummary
            {
                Customer_display_name = "chị An",
                Order_code_short = "PG001",
                Items =
                [
                    new OrderSpeechItem
                    {
                        Public_name = "Nước hồng sâm",
                        Quantity = 2,
                        Unit_label = "hộp",
                    },
                ],
                Total_amount = 560_000,
                Currency = PrivacySafeOrderSummaryCurrency.VND,
                Delivery_area_short = "Phường Bến Nghé, Quận Một",
                Program_display_name = "Giờ Vàng",
                Locale = PrivacySafeOrderSummaryLocale.ViVN,
            },
            Call_restriction = callRestriction,
            Eligibility_snapshot = new { decision = "ELIGIBLE", source = "postgres-test" },
            Evidence_ref = "evidence://postgres/p2-1",
        };
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
