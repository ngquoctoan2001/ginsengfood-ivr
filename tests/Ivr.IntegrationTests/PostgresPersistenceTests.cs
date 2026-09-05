using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ivr.Api.Application;
using Ivr.Domain.Errors;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Ivr.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class PostgresPersistenceTestGroup
    : ICollectionFixture<PostgresPersistenceFixture>
{
    public const string Name = "P1-2 PostgreSQL persistence";
}

public sealed class PostgresPersistenceFixture : IAsyncLifetime
{
    // No WithWaitStrategy override here, and that is deliberate. A bare `pg_isready` probes the
    // Unix socket, which the official image's temporary initdb server answers while TCP is still
    // unbound -- so the fixture would publish a connection string roughly half a second before the
    // real postmaster takes over port 5432. Testcontainers' own default probes `--host localhost`
    // over TCP and therefore waits for the second server. See docs/evidence/W-0040 section 6.
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("ivr_p1_2")
        .WithUsername("ivr_test")
        .WithPassword("ivr-test-password")
        .Build();

    public ServiceProvider Services { get; private set; } = null!;

    public string ConnectionString => container.GetConnectionString();

    /// <summary>
    /// Container log captured the instant <c>StartAsync</c> returned, so a test can assert what
    /// the wait strategy actually waited for rather than trusting that it waited for the right
    /// thing.
    /// </summary>
    public string StartupLog { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await container.StartAsync();
        (string stdout, string stderr) = await container.GetLogsAsync(timestampsEnabled: false);
        StartupLog = string.Join(Environment.NewLine, stdout, stderr);
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IVR_EXECUTION_MODE"] = IvrOptions.LabRealSimExecutionMode,
                ["SALES_PROVIDER"] = "FAKE_TARGET_V1",
                ["SIM_PROVIDER"] = "VENDOR",
                ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",
                ["ConnectionStrings:IvrDb"] = ConnectionString,
            })
            .Build();
        var services = new ServiceCollection();
        services.AddIvrFoundation(configuration);
        services.AddIvrEligibility(configuration);
        services.AddIvrFeatureFlags(configuration);
        Services = services.BuildServiceProvider(validateScopes: true);
    }

    public async Task DisposeAsync()
    {
        await Services.DisposeAsync();
        await container.DisposeAsync();
    }

    public async Task ResetAsync()
    {
        IDbContextFactory<IvrDbContext> factory = Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await using IvrDbContext dbContext = await factory.CreateDbContextAsync();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
    }
}

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class PostgresPersistenceTests(PostgresPersistenceFixture fixture)
{
    [Fact]
    [Trait("TestId", "IT-DB-BOOT-08")]
    public void TheFixtureWaitsForThePostmasterThatActuallyListensOnTcp()
    {
        // W-0040 section 6. The official postgres image starts a temporary initdb server on the
        // Unix socket alone, shuts it down, then starts the real one on TCP. A wait strategy that
        // probes the Unix socket returns inside that window and publishes a connection string
        // pointing at a port the forwarder accepts but cannot yet serve, which lands on whichever
        // test happens to run first as an EndOfStreamException mid-handshake -- a failure that
        // names the wrong culprit. If StartAsync waited for the right postmaster, the log it
        // returned on already says so.
        Assert.Contains(
            "listening on IPv4 address",
            fixture.StartupLog,
            StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "IT-DB-MIGRATE-01")]
    public async Task MigrationAppliesToEmptyDatabaseRollsBackAndRecreates()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = Factory();
        await using IvrDbContext dbContext = await factory.CreateDbContextAsync();
        string[] applied = (await dbContext.Database.GetAppliedMigrationsAsync()).ToArray();
        Assert.Contains(applied, name => name.EndsWith(
            "P1_2_InitialTargetV1Persistence",
            StringComparison.Ordinal));
        Assert.Equal(22, await CountIvrTablesAsync(dbContext));
        IRuntimeSafetyHealth runtimeSafety = fixture.Services
            .GetRequiredService<IRuntimeSafetyHealth>();
        Assert.True(await runtimeSafety.IsAuditProviderHealthyAsync());

        IMigrator migrator = dbContext.GetService<IMigrator>();
        await migrator.MigrateAsync(Migration.InitialDatabase);
        Assert.Equal(0, await CountIvrTablesAsync(dbContext));
        Assert.False(await runtimeSafety.IsAuditProviderHealthyAsync());

        await dbContext.Database.MigrateAsync();
        Assert.Equal(22, await CountIvrTablesAsync(dbContext));
        Assert.Equal(45, await dbContext.FeatureFlags.CountAsync());
        Assert.True(await runtimeSafety.IsAuditProviderHealthyAsync());
    }

    [Fact]
    [Trait("TestId", "IT-DB-TASK-02")]
    public async Task CanonicalTargetTaskPersistsAndPolicyRemainsDataDriven()
    {
        await fixture.ResetAsync();
        await using IvrDbContext dbContext = await Factory().CreateDbContextAsync();
        ConfirmationTaskEntity canonical = ReadCanonicalTask();
        dbContext.ConfirmationTasks.Add(canonical);
        dbContext.AttemptPolicies.Add(new AttemptPolicyEntity
        {
            PolicyVersion = "test-policy-three-attempts",
            ProgramType = "GOLDEN_HOUR",
            MaxAttempts = 3,
            AttemptOffsetsSecondsJson = "[0,100,200]",
            ConfirmationWindowSeconds = 300,
            AllowedExecutionModesJson = "[\"MOCK\",\"LAB_REAL_SIM\"]",
            ApprovedForProduction = false,
            CreatedAt = canonical.CreatedAt,
        });
        await dbContext.SaveChangesAsync();

        Assert.Equal(1, await dbContext.ConfirmationTasks.CountAsync());
        Assert.Equal(3, (await dbContext.AttemptPolicies.SingleAsync()).MaxAttempts);
        string constraints = await ReadCheckConstraintsAsync(dbContext);
        Assert.DoesNotContain("max_attempts = 2", constraints, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("attempt_number <= 2", constraints, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("300", constraints, StringComparison.Ordinal);
        Assert.DoesNotContain("150", constraints, StringComparison.Ordinal);
        Assert.DoesNotContain("900", constraints, StringComparison.Ordinal);
        Assert.DoesNotContain("450", constraints, StringComparison.Ordinal);

        await using (IvrDbContext duplicateContext = await Factory().CreateDbContextAsync())
        {
            ConfirmationTaskEntity duplicate = ReadCanonicalTask("duplicate-idempotency");
            duplicate.IdempotencyKey = canonical.IdempotencyKey;
            duplicateContext.ConfirmationTasks.Add(duplicate);
            await Assert.ThrowsAsync<DbUpdateException>(
                () => duplicateContext.SaveChangesAsync());
        }

        await using (IvrDbContext unsafeContext = await Factory().CreateDbContextAsync())
        {
            ConfirmationTaskEntity unsafeTask = ReadCanonicalTask("unsafe-task");
            unsafeTask.PrivacySafeOrderSummaryJson =
                "{\"customer_display_name\":\"test\",\"full_address\":\"forbidden\"}";
            unsafeContext.ConfirmationTasks.Add(unsafeTask);
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => unsafeContext.SaveChangesAsync());
        }

        await using (IvrDbContext rawPhoneContext = await Factory().CreateDbContextAsync())
        {
            ConfirmationTaskEntity rawPhone = ReadCanonicalTask("raw-phone");
            rawPhone.PhoneMasked = "0901234567";
            rawPhoneContext.ConfirmationTasks.Add(rawPhone);
            await Assert.ThrowsAsync<DbUpdateException>(
                () => rawPhoneContext.SaveChangesAsync());
        }

        await using (IvrDbContext matrixContext = await Factory().CreateDbContextAsync())
        {
            ConfirmationTaskEntity invalidMatrix = ReadCanonicalTask("invalid-matrix");
            invalidMatrix.PaymentMethodSnapshot = "COD";
            matrixContext.ConfirmationTasks.Add(invalidMatrix);
            await Assert.ThrowsAsync<DbUpdateException>(
                () => matrixContext.SaveChangesAsync());
        }

        await using (IvrDbContext invalidPolicyContext = await Factory().CreateDbContextAsync())
        {
            invalidPolicyContext.AttemptPolicies.Add(new AttemptPolicyEntity
            {
                PolicyVersion = "invalid-offset-order",
                ProgramType = "GOLDEN_HOUR",
                MaxAttempts = 3,
                AttemptOffsetsSecondsJson = "[0,100,100]",
                ConfirmationWindowSeconds = 300,
                AllowedExecutionModesJson = "[\"MOCK\"]",
                CreatedAt = canonical.CreatedAt,
            });
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => invalidPolicyContext.SaveChangesAsync());
        }

        await using (IvrDbContext immutableTaskContext = await Factory().CreateDbContextAsync())
        {
            ConfirmationTaskEntity persisted = await immutableTaskContext
                .ConfirmationTasks.SingleAsync(task => task.TaskId == canonical.TaskId);
            persisted.PrivacySafeOrderSummaryJson =
                "{\"customer_display_name\":\"changed\"}";
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => immutableTaskContext.SaveChangesAsync());
        }

        await using (IvrDbContext triggerContext = await Factory().CreateDbContextAsync())
        {
            string changedSummary = "{\"customer_display_name\":\"changed\"}";
            await Assert.ThrowsAsync<PostgresException>(
                () => triggerContext.Database.ExecuteSqlInterpolatedAsync($$"""
                    UPDATE ivr_confirmation_tasks
                    SET privacy_safe_order_summary_json = {{changedSummary}}::jsonb
                    WHERE task_id = {{canonical.TaskId}}
                    """));
            await Assert.ThrowsAsync<PostgresException>(
                () => triggerContext.Database.ExecuteSqlRawAsync(
                    "UPDATE ivr_attempt_policies SET max_attempts = 4 "
                    + "WHERE policy_version = 'test-policy-three-attempts'"));
        }
    }

    [Fact]
    [Trait("TestId", "IT-DB-ATTEMPT-03")]
    public async Task AttemptTriggerCopiesJobPolicyAndProtectsTechnicalAttemptAccounting()
    {
        await fixture.ResetAsync();
        await using IvrDbContext dbContext = await Factory().CreateDbContextAsync();
        ConfirmationTaskEntity task = ReadCanonicalTask();
        CallJobEntity job = CreateJob(task, maxAttempts: 3, "[0,100,200]");
        dbContext.AddRange(task, job);
        await dbContext.SaveChangesAsync();

        var attempt = new CallAttemptEntity
        {
            IvrCallAttemptId = "ATTEMPT-P1-2-001",
            IvrCallJobId = job.IvrCallJobId,
            TaskId = task.TaskId,
            AttemptNumber = 1,
            MaxAttemptsSnapshot = 9,
            ScheduledAt = task.CreatedAt,
            ScheduledWindowExpiresAt = task.ExpiresAt,
            Status = "NORMALIZED_TECHNICAL_RETRY",
            IsCountedCustomerAttempt = false,
            TechnicalRetryAllowed = true,
            TechnicalRetryCount = 1,
            TechnicalExceptionType = "SIM_IO",
            PolicyVersion = job.AttemptPolicyCode,
            ScriptVersion = job.ScriptVersion,
        };
        dbContext.CallAttempts.Add(attempt);
        await dbContext.SaveChangesAsync();
        await dbContext.Entry(attempt).ReloadAsync();
        Assert.Equal(3, attempt.MaxAttemptsSnapshot);

        dbContext.CallAttempts.Add(new CallAttemptEntity
        {
            IvrCallAttemptId = "ATTEMPT-P1-2-TECHNICAL-RETRY",
            IvrCallJobId = job.IvrCallJobId,
            TaskId = task.TaskId,
            AttemptNumber = 1,
            MaxAttemptsSnapshot = 1,
            ScheduledAt = task.CreatedAt,
            ScheduledWindowExpiresAt = task.ExpiresAt,
            Status = "NORMALIZED_REVIEW_REQUIRED",
            IsCountedCustomerAttempt = false,
            TechnicalRetryAllowed = false,
            TechnicalRetryCount = 2,
            TechnicalExceptionType = "SIM_IO",
            PolicyVersion = job.AttemptPolicyCode,
            ScriptVersion = job.ScriptVersion,
        });
        await dbContext.SaveChangesAsync();
        Assert.Equal(2, await dbContext.CallAttempts.CountAsync());

        dbContext.RawCallEvents.AddRange(
            new RawCallEventEntity
            {
                RawEventId = "RAW-EVENT-RINGING-001",
                IvrCallAttemptId = attempt.IvrCallAttemptId,
                IvrCallJobId = job.IvrCallJobId,
                RawCallStatus = "ringing",
                ReceivedAt = task.CreatedAt,
            },
            new RawCallEventEntity
            {
                RawEventId = "RAW-EVENT-TECHNICAL-001",
                IvrCallAttemptId = attempt.IvrCallAttemptId,
                IvrCallJobId = job.IvrCallJobId,
                RawCallStatus = "failed",
                TechnicalErrorCode = "SIM_IO",
                ReceivedAt = task.CreatedAt.AddSeconds(1),
            });
        await dbContext.SaveChangesAsync();
        Assert.Equal(2, await dbContext.RawCallEvents.CountAsync());

        await Assert.ThrowsAsync<PostgresException>(
            () => dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE ivr_call_attempts SET is_counted_customer_attempt = TRUE "
                + "WHERE ivr_call_attempt_id = 'ATTEMPT-P1-2-001'"));
        await Assert.ThrowsAsync<PostgresException>(
            () => dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE ivr_call_jobs SET max_attempts = 4 "
                + "WHERE ivr_call_job_id = 'JOB-TASK-TARGET-GH-0001'"));
    }

    [Fact]
    [Trait("TestId", "IT-DB-FLAG-04")]
    public async Task FeatureFlagMutationAuditAndIdempotencyCommitAtomically()
    {
        await fixture.ResetAsync();
        IFeatureFlagStore store = fixture.Services.GetRequiredService<IFeatureFlagStore>();
        IFeatureFlagCommandIdempotency idempotency = fixture.Services
            .GetRequiredService<IFeatureFlagCommandIdempotency>();
        FeatureFlagSnapshot before = await store.ReadFreshAsync(FeatureFlagEnvironments.Lab);
        FeatureFlagSnapshot proposed = before with
        {
            Revision = before.Revision + 1,
            GlobalDialKillSwitch = false,
        };
        AuditEvent audit = CreateAudit("corr-flag-atomic-001");

        FeatureFlagSnapshot first = await idempotency.ExecuteAsync(
            "flag-command-001",
            "HASH-A",
            cancellationToken => store.ApplyAuditedAsync(
                before,
                proposed,
                audit,
                cancellationToken));
        FeatureFlagSnapshot replay = await idempotency.ExecuteAsync(
            "flag-command-001",
            "HASH-A",
            _ => Task.FromException<FeatureFlagSnapshot>(
                new InvalidOperationException("Replay must not execute the factory.")));
        AssertFeatureSnapshotsEquivalent(first, replay);

        await using (IvrDbContext dbContext = await Factory().CreateDbContextAsync())
        {
            Assert.Equal(1, await dbContext.AuditLog.CountAsync());
            Assert.Equal(1, await dbContext.IdempotencyKeys.CountAsync());
            Assert.All(
                await dbContext.FeatureFlags
                    .Where(row => row.Environment == FeatureFlagEnvironments.Lab)
                    .ToListAsync(),
                row => Assert.Equal(1, row.Revision));
        }

        IvrFailureException conflict = await Assert.ThrowsAsync<IvrFailureException>(
            () => idempotency.ExecuteAsync(
                "flag-command-001",
                "HASH-B",
                _ => Task.FromResult(proposed)));
        Assert.Equal(IvrErrorCodes.IdempotencyConflict, conflict.ErrorCode);

        FeatureFlagSnapshot stable = await store.ReadFreshAsync(FeatureFlagEnvironments.Lab);
        await Assert.ThrowsAsync<IntentionalRollbackException>(
            () => idempotency.ExecuteAsync(
                "flag-command-rollback",
                "HASH-ROLLBACK",
                cancellationToken => MutateThenFailAsync(store, stable, cancellationToken)));
        FeatureFlagSnapshot afterRollback = await store.ReadFreshAsync(
            FeatureFlagEnvironments.Lab);
        AssertFeatureSnapshotsEquivalent(stable, afterRollback);
        await using IvrDbContext verification = await Factory().CreateDbContextAsync();
        Assert.Equal(1, await verification.AuditLog.CountAsync());
        Assert.Equal(1, await verification.IdempotencyKeys.CountAsync());
    }

    [Fact]
    [Trait("TestId", "IT-DB-AUDIT-07")]
    public async Task AuditRowsRejectDirectUpdateAndDeleteAndRemainUnchanged()
    {
        await fixture.ResetAsync();
        IAuditLogger auditLogger = fixture.Services.GetRequiredService<IAuditLogger>();
        AuditLogEntry entry = await auditLogger.AppendAsync(
            new AuditEvent(
                "integration-test",
                "AUDIT_APPEND_ONLY_PROOF",
                "audit:append-only",
                "Verify database trigger enforcement",
                "corr-audit-append-only-001",
                new Dictionary<string, object?> { ["testId"] = "IT-DB-AUDIT-07" }));

        await using (IvrDbContext updateContext = await Factory().CreateDbContextAsync())
        {
            PostgresException updateFailure = await Assert.ThrowsAsync<PostgresException>(
                () => updateContext.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE ivr_audit_log SET action = {'X'} WHERE audit_id = {entry.Id}"));
            Assert.Equal(PostgresErrorCodes.RaiseException, updateFailure.SqlState);
            Assert.Contains("append-only", updateFailure.MessageText, StringComparison.Ordinal);
        }

        await using (IvrDbContext deleteContext = await Factory().CreateDbContextAsync())
        {
            PostgresException deleteFailure = await Assert.ThrowsAsync<PostgresException>(
                () => deleteContext.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM ivr_audit_log WHERE audit_id = {entry.Id}"));
            Assert.Equal(PostgresErrorCodes.RaiseException, deleteFailure.SqlState);
            Assert.Contains("append-only", deleteFailure.MessageText, StringComparison.Ordinal);
        }

        await using IvrDbContext verification = await Factory().CreateDbContextAsync();
        AuditLogEntity persisted = await verification.AuditLog
            .AsNoTracking()
            .SingleAsync(row => row.AuditId == entry.Id);
        Assert.Equal("AUDIT_APPEND_ONLY_PROOF", persisted.Action);
        Assert.Equal("corr-audit-append-only-001", persisted.CorrelationId);
        Assert.Equal(1, await verification.AuditLog.CountAsync());
    }

    [Fact]
    [Trait("TestId", "IT-DB-OUTBOX-06")]
    public async Task CallbackOutboxLeasesExactlyOnceAndKeepsPayloadImmutable()
    {
        await fixture.ResetAsync();
        ResultCallbackEntity callback;
        await using (IvrDbContext dbContext = await Factory().CreateDbContextAsync())
        {
            ConfirmationTaskEntity task = ReadCanonicalTask();
            CallJobEntity job = CreateJob(task, task.MaxAttempts, task.AttemptOffsetsSecondsJson);
            CallResultEntity result = CreateResult(task, job);
            dbContext.AddRange(task, job, result);
            await dbContext.SaveChangesAsync();
            string payload =
                "{\"task_id\":\"TASK-TARGET-GH-0001\",\"result_type\":\"IVR_CONFIRMED\"}";
            callback = new ResultCallbackEntity
            {
                CallbackId = "CALLBACK-P1-2-001",
                IvrCallResultId = result.IvrCallResultId,
                TaskId = task.TaskId,
                OfficialOrderId = task.OfficialOrderId,
                IdempotencyKey = "callback-idempotency-001",
                ResultStatus = "IVR_CONFIRMED",
                ResultState = "PENDING_CORE_REVALIDATION",
                DeliveryStatus = "READY",
                RequiresCoreRevalidation = true,
                PayloadJson = payload,
                PayloadSha256 = Sha256(payload),
                CreatedAt = task.CreatedAt,
            };
        }

        ICallbackOutboxRepository outbox = fixture.Services
            .GetRequiredService<ICallbackOutboxRepository>();
        await outbox.EnqueueAsync(callback);
        Task<IReadOnlyList<CallbackOutboxMessage>> first = outbox.DequeueReadyAsync(
            10,
            TimeSpan.FromMinutes(1));
        Task<IReadOnlyList<CallbackOutboxMessage>> second = outbox.DequeueReadyAsync(
            10,
            TimeSpan.FromMinutes(1));
        IReadOnlyList<CallbackOutboxMessage>[] batches = await Task.WhenAll(first, second);
        CallbackOutboxMessage leased = Assert.Single(batches.SelectMany(batch => batch));
        Assert.Equal(callback.PayloadSha256, leased.PayloadSha256);

        // The assertion above compares the stored hash to itself. That is not nothing, but it is
        // not the property the outbox depends on either -- and the gap between the two hid a
        // defect that stopped EVERY callback from being delivered. `payload_json` was jsonb, so
        // PostgreSQL reordered the keys on the way in; the text that came back no longer hashed to
        // `payload_sha256`, CallbackPayloadIntegrity rejected it before the HTTP send, and the row
        // dead-lettered as CALLBACK_PAYLOAD_INVALID. Re-deriving the hash from the round-tripped
        // bytes is what makes the check mean what it claims.
        Assert.Equal(leased.PayloadSha256, Sha256(leased.PayloadJson));
        Assert.Equal(callback.PayloadJson, leased.PayloadJson);

        await using IvrDbContext verification = await Factory().CreateDbContextAsync();
        await Assert.ThrowsAsync<PostgresException>(
            () => verification.Database.ExecuteSqlRawAsync(
                "UPDATE ivr_result_callbacks SET payload_json = '{{\"changed\":true}}'::jsonb "
                + "WHERE callback_id = 'CALLBACK-P1-2-001'"));
    }

    private IDbContextFactory<IvrDbContext> Factory() => fixture.Services
        .GetRequiredService<IDbContextFactory<IvrDbContext>>();

    private static async Task<int> CountIvrTablesAsync(IvrDbContext dbContext)
    {
        await using var command = dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText =
            "SELECT COUNT(*) FROM information_schema.tables "
            + "WHERE table_schema = 'public' AND table_name LIKE 'ivr_%'";
        await dbContext.Database.OpenConnectionAsync();
        object? result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static async Task<string> ReadCheckConstraintsAsync(IvrDbContext dbContext)
    {
        await using var command = dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText =
            "SELECT string_agg(pg_get_constraintdef(oid), ' ') "
            + "FROM pg_constraint WHERE contype = 'c' "
            + "AND connamespace = 'public'::regnamespace";
        await dbContext.Database.OpenConnectionAsync();
        return (string?)await command.ExecuteScalarAsync() ?? string.Empty;
    }

    private static ConfirmationTaskEntity ReadCanonicalTask(string suffix = "canonical")
    {
        string seedPath = FindRepositoryFile("seed", "sales-target-v1.sample.json");
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(seedPath));
        JsonElement item = document.RootElement.GetProperty("tasks")[0];
        JsonElement body = item.GetProperty("body");
        DateTimeOffset startedAt = body.GetProperty(
            "confirmation_window_started_at").GetDateTimeOffset();
        DateTimeOffset expiresAt = body.GetProperty(
            "confirmation_window_expires_at").GetDateTimeOffset();
        string taskId = suffix == "canonical"
            ? body.GetProperty("task_id").GetString()!
            : $"TASK-{suffix.ToUpperInvariant()}";
        string idempotencyKey = suffix == "canonical"
            ? item.GetProperty("headers").GetProperty("Idempotency-Key").GetString()!
            : $"idem-{suffix}";
        return new ConfirmationTaskEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            ContractVersion = body.GetProperty("contract_version").GetString()!,
            IdempotencyKey = idempotencyKey,
            CorrelationId = body.GetProperty("correlation_id").GetString()!,
            OfficialOrderId = body.GetProperty("order_id").GetString()!,
            OrderCode = body.GetProperty("order_code").GetString()!,
            OrderVersion = body.GetProperty("order_version").GetString()!,
            OrderState = body.GetProperty("order_state").GetString()!,
            PaymentMethodSnapshot = body.GetProperty(
                "payment_method_snapshot").GetString()!,
            IvrConfirmationRequired = true,
            ProgramType = body.GetProperty("program_code").GetString()!,
            AttemptPolicyVersion = body.GetProperty(
                "attempt_policy_version").GetString()!,
            MaxAttempts = body.GetProperty("max_customer_attempts").GetInt32(),
            AttemptOffsetsSecondsJson = body.GetProperty(
                "attempt_offsets_seconds").GetRawText(),
            ConfirmationWindowStartedAt = startedAt,
            ConfirmationWindowExpiresAt = expiresAt,
            PhoneRef = body.GetProperty("phone_ref").GetString()!,
            PhoneMasked = body.GetProperty("phone_masked").GetString()!,
            PhoneValidationStatus = body.GetProperty(
                "phone_validation_status").GetString(),
            DialTokenCiphertext = $"enc:test:{suffix}",
            DialTokenExpiresAt = body.GetProperty(
                "dial_token_expires_at").GetDateTimeOffset(),
            PrivacySafeOrderSummaryJson = body.GetProperty(
                "privacy_safe_order_summary").GetRawText(),
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            EligibilitySnapshotJson = body.GetProperty(
                "eligibility_snapshot").GetRawText(),
            CallRestriction = body.GetProperty("call_restriction").GetBoolean(),
            NotForQuoteCartDraft = true,
            NoDirectOrderUpdate = true,
            CreatedAt = body.GetProperty("created_at").GetDateTimeOffset(),
            ExpiresAt = expiresAt,
        };
    }

    private static CallJobEntity CreateJob(
        ConfirmationTaskEntity task,
        int maxAttempts,
        string offsetsJson) =>
        new()
        {
            IvrCallJobId = $"JOB-{task.TaskId}",
            TaskId = task.TaskId,
            OfficialOrderId = task.OfficialOrderId,
            OrderVersionSnapshot = task.OrderVersion,
            ProgramType = task.ProgramType,
            AttemptPolicyCode = task.AttemptPolicyVersion,
            Status = "READY_FOR_SCHEDULER",
            MaxAttempts = maxAttempts,
            AttemptOffsetsSecondsJson = offsetsJson,
            ConfirmationWindowSeconds = checked((int)(task.ExpiresAt - task.CreatedAt).TotalSeconds),
            AttemptScheduleJson = offsetsJson,
            T0At = task.CreatedAt,
            ExpiresAt = task.ExpiresAt,
            Eligible = true,
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            QueueStatus = "QUEUED",
            ScriptVersion = "script-v1",
            PrivacyPolicyVersion = "privacy-v1",
            CreatedAt = task.CreatedAt,
        };

    private static CallResultEntity CreateResult(
        ConfirmationTaskEntity task,
        CallJobEntity job) =>
        new()
        {
            IvrCallResultId = $"RESULT-{task.TaskId}",
            IvrCallJobId = job.IvrCallJobId,
            TaskId = task.TaskId,
            OfficialOrderId = task.OfficialOrderId,
            OrderVersionSnapshot = task.OrderVersion,
            OrderVersionSeenByIvr = task.OrderVersion,
            FinalResultStatus = "IVR_CONFIRMED",
            ResultType = "IVR_CONFIRMED",
            DtmfKey = "1",
            IsCountedCustomerAttempt = true,
            IsFinalForIvr = true,
            RecommendedCoreAction = "REVALIDATE_AND_CONFIRM_ORDER",
            CoreOrderHandoffRequired = true,
            HumanReviewRequired = false,
            CreatedAt = task.CreatedAt,
        };

    private static AuditEvent CreateAudit(string correlationId) =>
        new(
            "integration-test",
            "ADMIN_FEATURE_FLAGS_CHANGE",
            "feature-flags:lab",
            "P1-2 atomic persistence test",
            correlationId,
            new Dictionary<string, object?> { ["testId"] = "IT-DB-FLAG-04" });

    private static void AssertFeatureSnapshotsEquivalent(
        FeatureFlagSnapshot expected,
        FeatureFlagSnapshot actual)
    {
        Assert.Equal(expected.Environment, actual.Environment);
        Assert.Equal(expected.Revision, actual.Revision);
        Assert.Equal(expected.ExecutionMode, actual.ExecutionMode);
        Assert.Equal(expected.SalesProvider, actual.SalesProvider);
        Assert.Equal(expected.SimProvider, actual.SimProvider);
        Assert.Equal(expected.AttemptPolicyVersion, actual.AttemptPolicyVersion);
        Assert.Equal(expected.RealCustomerCallAllowed, actual.RealCustomerCallAllowed);
        Assert.True(expected.LabDestinationAllowlist.SetEquals(
            actual.LabDestinationAllowlist));
        Assert.Equal(expected.GlobalDialKillSwitch, actual.GlobalDialKillSwitch);
        Assert.Equal(expected.V1NotificationEnabled, actual.V1NotificationEnabled);
        Assert.Equal(expected.RecordingEnabled, actual.RecordingEnabled);
    }

    private static async Task<FeatureFlagSnapshot> MutateThenFailAsync(
        IFeatureFlagStore store,
        FeatureFlagSnapshot before,
        CancellationToken cancellationToken)
    {
        FeatureFlagSnapshot proposed = before with
        {
            Revision = before.Revision + 1,
            GlobalDialKillSwitch = true,
        };
        await store.ApplyAuditedAsync(
            before,
            proposed,
            CreateAudit("corr-flag-rollback-001"),
            cancellationToken);
        throw new IntentionalRollbackException();
    }

    private static string Sha256(string payload) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(payload)));

    private static string FindRepositoryFile(params string[] segments)
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string candidate = segments.Aggregate(
                directory.FullName,
                Path.Combine);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            $"Repository fixture was not found: {Path.Combine(segments)}");
    }

    private sealed class IntentionalRollbackException : Exception;
}
