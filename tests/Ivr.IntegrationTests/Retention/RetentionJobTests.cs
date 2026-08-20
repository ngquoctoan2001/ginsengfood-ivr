using System.Text.Json;
using Ivr.Domain.Retention;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Retention;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Ivr.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class RetentionJobTestGroup : ICollectionFixture<RetentionJobFixture>
{
    public const string Name = "P1-5 retention lifecycle";
}

public sealed class RetentionJobFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("ivr_p1_5")
        .WithUsername("ivr_test")
        .WithPassword("ivr-test-password")
        .Build();

    public ServiceProvider Services { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await container.StartAsync();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IVR_EXECUTION_MODE"] = IvrOptions.LabRealSimExecutionMode,
                ["SALES_PROVIDER"] = "FAKE_TARGET_V1",
                ["SIM_PROVIDER"] = "VENDOR",
                ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",
                ["ConnectionStrings:IvrDb"] = container.GetConnectionString(),
                [$"Ivr:Retention:PeriodDays:{RetentionDataClasses.IdempotencyKey}"] = "1",
                [$"Ivr:Retention:PeriodDays:{RetentionDataClasses.SpeechSnapshot}"] = "1",
                [$"Ivr:Retention:PeriodDays:{RetentionDataClasses.EvidenceLink}"] = "1",
                [$"Ivr:Retention:PeriodDays:{RetentionDataClasses.ReviewItem}"] = "1",
                [$"Ivr:Retention:PeriodDays:{RetentionDataClasses.TaskMetadata}"] = "1",
            })
            .Build();
        var services = new ServiceCollection();
        services.AddIvrFoundation(configuration);
        services.AddIvrRetention(configuration);
        Services = services.BuildServiceProvider(validateScopes: true);
    }

    public async Task DisposeAsync()
    {
        await Services.DisposeAsync();
        await container.DisposeAsync();
    }

    public async Task ResetAsync()
    {
        IDbContextFactory<IvrDbContext> factory = Factory();
        await using IvrDbContext dbContext = await factory.CreateDbContextAsync();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
    }

    public IDbContextFactory<IvrDbContext> Factory() => Services
        .GetRequiredService<IDbContextFactory<IvrDbContext>>();
}

[Collection(RetentionJobTestGroup.Name)]
public sealed class RetentionJobTests(RetentionJobFixture fixture)
{
    private static readonly DateTimeOffset Now = new(2026, 8, 13, 9, 30, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "IT-RET-DRYRUN-02")]
    public async Task DryRunReportsEligibleRowsWithoutMutatingTargetData()
    {
        await fixture.ResetAsync();
        await InsertIdempotencyAsync("dry-run-old", Now.AddDays(-10));

        RetentionRunReport report = await RunAsync(
            RetentionDataClasses.IdempotencyKey,
            dryRun: true);

        RetentionClassReport dataClass = Assert.Single(report.Classes);
        Assert.Equal(RetentionClassRunStatus.DryRun, dataClass.Status);
        Assert.Equal(1, dataClass.ExaminedCount);
        Assert.Equal(0, report.DeletedCount);
        await using IvrDbContext dbContext = await fixture.Factory().CreateDbContextAsync();
        Assert.Equal(1, await dbContext.IdempotencyKeys.CountAsync());
        Assert.Equal(1, await dbContext.AuditLog.CountAsync());
    }

    [Fact]
    [Trait("TestId", "IT-RET-DELETE-03")]
    public async Task DeleteRemovesOnlyExpiredRowsInTheRequestedClass()
    {
        await fixture.ResetAsync();
        await InsertIdempotencyAsync("delete-old", Now.AddDays(-10));
        await InsertIdempotencyAsync("delete-recent", Now.AddHours(-4));
        await using (IvrDbContext seed = await fixture.Factory().CreateDbContextAsync())
        {
            seed.Evidence.AddRange(
                CreateEvidence("unaccepted", acceptedAt: null),
                CreateEvidence("accepted", acceptedAt: Now.AddDays(-5)));
            seed.ReviewItems.Add(new ReviewItemEntity
            {
                ReviewItemId = "review-outside-class",
                SourceType = "retention-test",
                SourceId = "safe-source",
                Reason = "test fixture",
                Status = "OPEN",
                CorrelationId = "corr-review-outside-class",
                CreatedAt = Now.AddDays(-10),
                RetentionClass = RetentionDataClasses.ReviewItem,
            });
            await seed.SaveChangesAsync();
        }

        RetentionRunReport report = await RunAsync(
            RetentionDataClasses.EvidenceLink,
            dryRun: false);

        Assert.Equal(1, report.DeletedCount);
        await using IvrDbContext dbContext = await fixture.Factory().CreateDbContextAsync();
        Assert.True(await dbContext.IdempotencyKeys.AnyAsync(item => item.Key == "delete-old"));
        Assert.True(await dbContext.IdempotencyKeys.AnyAsync(item => item.Key == "delete-recent"));
        Assert.True(await dbContext.ReviewItems.AnyAsync());
        Assert.False(await dbContext.Evidence.AnyAsync(item => item.EvidenceRef == "evidence-unaccepted"));
        Assert.True(await dbContext.Evidence.AnyAsync(item => item.EvidenceRef == "evidence-accepted"));
    }

    [Fact]
    [Trait("TestId", "IT-RET-HOLD-04")]
    public async Task LegalHoldAlwaysWinsOverAnExpiredRetentionPeriod()
    {
        await fixture.ResetAsync();
        await InsertIdempotencyAsync(
            "held-old",
            Now.AddDays(-10),
            legalHoldUntil: Now.AddDays(30));

        RetentionRunReport report = await RunAsync(
            RetentionDataClasses.IdempotencyKey,
            dryRun: false);

        RetentionClassReport dataClass = Assert.Single(report.Classes);
        Assert.Equal(1, dataClass.LegalHoldCount);
        Assert.Equal(0, dataClass.DeletedCount);
        await using IvrDbContext dbContext = await fixture.Factory().CreateDbContextAsync();
        Assert.True(await dbContext.IdempotencyKeys.AnyAsync(item => item.Key == "held-old"));
    }

    [Fact]
    public async Task HeldIntakeOutboxBlocksParentTaskAndJobDeletion()
    {
        await fixture.ResetAsync();
        DateTimeOffset old = Now.AddDays(-10);
        ConfirmationTaskEntity task = CreateConfirmationTask("outbox-hold", old);
        task.RetentionClass = RetentionDataClasses.TaskMetadata;
        var job = new CallJobEntity
        {
            IvrCallJobId = "JOB-RET-OUTBOX-HOLD",
            TaskId = task.TaskId,
            OfficialOrderId = task.OfficialOrderId,
            OrderVersionSnapshot = task.OrderVersion,
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyCode = "retention-test-v1",
            Status = "CLOSED",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowSeconds = 300,
            AttemptScheduleJson = "[]",
            T0At = old,
            ExpiresAt = old.AddMinutes(5),
            QueueStatus = "CLOSED",
            ScriptVersion = "SCRIPT-ORDER-CONFIRM@v1-test-approved",
            CreatedAt = old,
            ClosedAt = old.AddMinutes(5),
            RetentionClass = RetentionDataClasses.TaskMetadata,
        };
        var outbox = new TaskIntakeOutboxEntity
        {
            OutboxId = Guid.NewGuid(),
            TaskId = task.TaskId,
            IvrCallJobId = job.IvrCallJobId,
            EventType = "RETENTION_TEST",
            Status = "PUBLISHED",
            CorrelationId = task.CorrelationId,
            PayloadSha256 = new string('A', 64),
            CreatedAt = old,
            LegalHoldUntil = Now.AddDays(30),
            RetentionClass = RetentionDataClasses.TaskMetadata,
        };
        await using (IvrDbContext seed = await fixture.Factory().CreateDbContextAsync())
        {
            seed.ConfirmationTasks.Add(task);
            seed.CallJobs.Add(job);
            seed.TaskIntakeOutbox.Add(outbox);
            await seed.SaveChangesAsync();
        }

        RetentionRunReport report = await RunAsync(
            RetentionDataClasses.TaskMetadata,
            dryRun: false);

        RetentionClassReport dataClass = Assert.Single(report.Classes);
        Assert.Equal(1, dataClass.LegalHoldCount);
        Assert.Equal(2, dataClass.DependencyBlockedCount);
        Assert.Equal(0, dataClass.DeletedCount);
        await using IvrDbContext verification = await fixture.Factory().CreateDbContextAsync();
        Assert.Equal(1, await verification.TaskIntakeOutbox.CountAsync());
        Assert.Equal(1, await verification.CallJobs.CountAsync());
        Assert.Equal(1, await verification.ConfirmationTasks.CountAsync());
    }

    [Fact]
    public async Task MissingPeriodCreatesNotConfiguredReportAndCheckpoint()
    {
        await fixture.ResetAsync();

        RetentionRunReport report = await RunAsync(
            RetentionDataClasses.RawCallEvent,
            dryRun: false);

        RetentionClassReport dataClass = Assert.Single(report.Classes);
        Assert.Equal(RetentionClassRunStatus.NotConfigured, dataClass.Status);
        Assert.Equal(0, dataClass.NotConfiguredAgeDays);
        await using IvrDbContext verification = await fixture.Factory().CreateDbContextAsync();
        RetentionCheckpointEntity checkpoint = await verification.RetentionCheckpoints
            .SingleAsync();
        Assert.Equal("NOT_CONFIGURED", checkpoint.Status);
        Assert.Equal("__policy__", checkpoint.Segment);
        Assert.NotNull(checkpoint.FirstNotConfiguredAt);
    }

    [Fact]
    [Trait("TestId", "IT-RET-AUDIT-05")]
    public async Task RunAppendsPrivacySafeAuditWithoutWeakeningAppendOnlyProtection()
    {
        await fixture.ResetAsync();
        await InsertIdempotencyAsync("audit-secret-row", Now.AddDays(-10));
        await using (IvrDbContext seed = await fixture.Factory().CreateDbContextAsync())
        {
            seed.AuditLog.Add(CreateAudit("existing-audit", "existing.action"));
            await seed.SaveChangesAsync();
        }

        RetentionRunReport report = await RunAsync(
            RetentionDataClasses.IdempotencyKey,
            dryRun: false);

        await using IvrDbContext dbContext = await fixture.Factory().CreateDbContextAsync();
        Assert.Equal(2, await dbContext.AuditLog.CountAsync());
        AuditLogEntity runAudit = await dbContext.AuditLog.SingleAsync(
            item => item.TargetId == report.RunId.ToString("N"));
        Assert.DoesNotContain("audit-secret-row", runAudit.DataJson, StringComparison.Ordinal);
        Assert.Contains("DeletedCount", runAudit.DataJson, StringComparison.Ordinal);
        await Assert.ThrowsAsync<PostgresException>(
            () => dbContext.Database.ExecuteSqlRawAsync(
                "DELETE FROM ivr_audit_log WHERE target_id = 'existing-audit'"));
    }

    [Fact]
    [Trait("TestId", "IT-RET-RESUME-06")]
    public async Task CancelledRunResumesFromCommittedBatchesWithoutDuplicateMutation()
    {
        await fixture.ResetAsync();
        await InsertIdempotencyAsync("resume-1", Now.AddDays(-10));
        await InsertIdempotencyAsync("resume-2", Now.AddDays(-10));
        await InsertIdempotencyAsync("resume-3", Now.AddDays(-10));
        using var cancellation = new CancellationTokenSource();
        var interruptingTelemetry = new CancelAfterFirstBatchTelemetry(cancellation);
        var interruptedJob = new RetentionJob(
            fixture.Factory(),
            fixture.Services.GetRequiredService<IRetentionPolicyProvider>(),
            interruptingTelemetry,
            TimeProvider.System);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => interruptedJob.RunAsync(
                new RetentionRunOptions(
                    DryRun: false,
                    DataClasses: [RetentionDataClasses.IdempotencyKey],
                    Now: Now,
                    BatchSize: 1),
                cancellation.Token));

        await using (IvrDbContext interrupted = await fixture.Factory().CreateDbContextAsync())
        {
            Assert.Equal(2, await interrupted.IdempotencyKeys.CountAsync());
            RetentionCheckpointEntity checkpoint = await interrupted.RetentionCheckpoints.SingleAsync();
            Assert.Equal("IN_PROGRESS", checkpoint.Status);
            Assert.Equal(1, checkpoint.ProcessedCount);
        }

        RetentionRunReport resumed = await RunAsync(
            RetentionDataClasses.IdempotencyKey,
            dryRun: false,
            batchSize: 1);

        Assert.Equal(2, resumed.DeletedCount);
        await using IvrDbContext completed = await fixture.Factory().CreateDbContextAsync();
        Assert.Empty(await completed.IdempotencyKeys.ToListAsync());
        RetentionCheckpointEntity finalCheckpoint = await completed.RetentionCheckpoints.SingleAsync();
        Assert.Equal("COMPLETED", finalCheckpoint.Status);
        Assert.Equal(2, finalCheckpoint.ProcessedCount);
    }

    [Fact]
    [Trait("TestId", "IT-RET-PII-07")]
    public async Task AnonymizeRemovesSpeechSnapshotPiiAndPreservesContractIdentity()
    {
        await fixture.ResetAsync();
        ConfirmationTaskEntity task = CreateConfirmationTask("pii", Now.AddDays(-10));
        await using (IvrDbContext seed = await fixture.Factory().CreateDbContextAsync())
        {
            seed.ConfirmationTasks.Add(task);
            await seed.SaveChangesAsync();
        }

        RetentionRunReport report = await RunAsync(
            RetentionDataClasses.SpeechSnapshot,
            dryRun: false);

        Assert.Equal(1, report.AnonymizedCount);
        await using IvrDbContext dbContext = await fixture.Factory().CreateDbContextAsync();
        ConfirmationTaskEntity redacted = await dbContext.ConfirmationTasks.SingleAsync();
        Assert.Equal(task.TaskId, redacted.TaskId);
        Assert.Equal(task.OfficialOrderId, redacted.OfficialOrderId);
        Assert.Equal("redacted", redacted.PhoneRef);
        Assert.Equal("***", redacted.PhoneMasked);
        Assert.Equal("enc:redacted", redacted.DialTokenCiphertext);
        // W-0052 / P10-1. phone_validation_status joined the redaction when the field inventory
        // was built: leaving VALID behind after redacting the reference it describes keeps a weak
        // signal about a person whose data was supposed to be gone.
        Assert.Equal("REDACTED", redacted.PhoneValidationStatus);
        Assert.Equal("{}", redacted.PrivacySafeOrderSummaryJson);
        Assert.NotNull(redacted.AnonymizedAt);
        string serialized = JsonSerializer.Serialize(redacted);
        Assert.DoesNotContain("chị An", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("phone-ref-sensitive", serialized, StringComparison.Ordinal);
    }

    private async Task<RetentionRunReport> RunAsync(
        string dataClass,
        bool dryRun,
        int batchSize = 250) => await fixture.Services
        .GetRequiredService<IRetentionJob>()
        .RunAsync(
            new RetentionRunOptions(
                DryRun: dryRun,
                DataClasses: [dataClass],
                Now: Now,
                BatchSize: batchSize),
            CancellationToken.None);

    private async Task InsertIdempotencyAsync(
        string key,
        DateTimeOffset createdAt,
        DateTimeOffset? legalHoldUntil = null)
    {
        await using IvrDbContext dbContext = await fixture.Factory().CreateDbContextAsync();
        dbContext.IdempotencyKeys.Add(new IdempotencyKeyEntity
        {
            Scope = "retention-test",
            Key = key,
            PayloadHash = "sha256-safe-test-hash",
            ResponseSnapshotJson = "{}",
            CreatedAt = createdAt,
            ExpiresAt = createdAt.AddHours(1),
            LegalHoldUntil = legalHoldUntil,
            RetentionClass = RetentionDataClasses.IdempotencyKey,
        });
        await dbContext.SaveChangesAsync();
    }

    private static AuditLogEntity CreateAudit(string targetId, string action) => new()
    {
        AuditId = Guid.NewGuid(),
        ActorId = "retention-test",
        ActorType = "service",
        Action = action,
        TargetType = "test",
        TargetId = targetId,
        CorrelationId = $"corr-{targetId}",
        DataJson = "{}",
        CreatedAt = Now,
        RetentionClass = "audit_log",
    };

    private static EvidenceEntity CreateEvidence(
        string suffix,
        DateTimeOffset? acceptedAt) => new()
        {
            EvidenceRef = $"evidence-{suffix}",
            Kind = "retention-test",
            CorrelationId = $"corr-evidence-{suffix}",
            WorkId = "W-0064",
            PayloadRef = $"docs/evidence/W-0064/{suffix}",
            CreatedAt = Now.AddDays(-10),
            AcceptedAt = acceptedAt,
            RetentionClass = RetentionDataClasses.EvidenceLink,
        };

    private static ConfirmationTaskEntity CreateConfirmationTask(
        string suffix,
        DateTimeOffset createdAt) => new()
        {
            Id = Guid.NewGuid(),
            TaskId = $"TASK-RET-{suffix.ToUpperInvariant()}",
            ContractVersion = "ivr-order-confirmation.v1",
            IdempotencyKey = $"idem-ret-{suffix}",
            CorrelationId = $"corr-ret-{suffix}",
            OfficialOrderId = $"ORDER-RET-{suffix.ToUpperInvariant()}",
            OrderCode = $"GF-RET-{suffix.ToUpperInvariant()}",
            OrderVersion = "1",
            OrderState = "CONFIRMING",
            PaymentMethodSnapshot = "ONLINE",
            IvrConfirmationRequired = true,
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyVersion = "retention-test-v1",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowStartedAt = createdAt,
            ConfirmationWindowExpiresAt = createdAt.AddMinutes(5),
            PhoneRef = "phone-ref-sensitive",
            PhoneMasked = "84xxxxx0001",
            PhoneValidationStatus = "VALID",
            DialTokenCiphertext = "enc:sensitive-token",
            DialTokenExpiresAt = createdAt.AddMinutes(5),
            PrivacySafeOrderSummaryJson =
                "{\"customer_display_name\":\"chị An\",\"items\":[{\"public_name\":\"Nước hồng sâm\",\"quantity\":1}]}",
            EligibilityDecision = "ELIGIBLE",
            EligibilitySnapshotJson = "{\"decision\":\"ELIGIBLE\"}",
            CallRestriction = false,
            CreatedAt = createdAt,
            ExpiresAt = createdAt.AddMinutes(5),
            RetentionClass = RetentionDataClasses.SpeechSnapshot,
        };

    private sealed class CancelAfterFirstBatchTelemetry(
        CancellationTokenSource cancellation) : IRetentionTelemetry
    {
        private int committedBatches;

        public ValueTask BatchCommittedAsync(
            string dataClass,
            string segment,
            int affectedRows,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (affectedRows > 0 && Interlocked.Increment(ref committedBatches) == 1)
            {
                cancellation.Cancel();
            }

            return ValueTask.CompletedTask;
        }

        public void RecordNotConfigured(string dataClass, double ageDays)
        {
        }

        public void RecordClassCompleted(string dataClass, long affectedRows)
        {
        }

        public void RecordRunFailed()
        {
        }
    }
}
