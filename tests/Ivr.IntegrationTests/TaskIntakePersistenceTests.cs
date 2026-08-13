using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Policies;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Persistence.Security;
using Ivr.Infrastructure.Scripts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

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
        Assert.Equal("SCRIPT-ORDER-CONFIRM", task.CallScriptTemplateId);
        Assert.Equal("v1-test-approved", task.CallScriptVersion);
        Assert.StartsWith("enc:mock-sha256:", task.DialTokenCiphertext,
            StringComparison.Ordinal);
        Assert.DoesNotContain(source.Dial_token, task.DialTokenCiphertext,
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
                  'privacy_policy_version')
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
    public async Task RestrictedTaskPersistsPendingForEligibilityDecision()
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
            Options.Create(new IvrOptions()));
        IvrConfirmationTaskV1 source = CreateTask(callRestriction: true);

        TaskIntakeOutcome outcome = await service.IntakeAsync(new TaskIntakeCommand(
            source,
            "idem-postgres-rejected",
            source.Correlation_id!,
            new string('B', 64),
            ExecutionMode.Mock));

        Assert.Equal(TaskIntakeDecisions.AcceptedDryRunOnly, outcome.Decision);
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        ConfirmationTaskEntity task = await verification.ConfirmationTasks.SingleAsync();
        CallJobEntity job = await verification.CallJobs.SingleAsync();
        Assert.True(task.CallRestriction);
        Assert.False(job.Eligible);
        Assert.Equal(EligibilityDecisions.Pending, job.EligibilityDecision);
        Assert.Equal(1, await verification.TaskIntakeOutbox.CountAsync());
        Assert.Equal(1, await verification.IdempotencyKeys.CountAsync());
        AuditLogEntity audit = await verification.AuditLog.SingleAsync();
        Assert.DoesNotContain(source.Dial_token, audit.DataJson, StringComparison.Ordinal);
        Assert.DoesNotContain(source.Phone_ref, audit.DataJson, StringComparison.Ordinal);
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

    private static IvrConfirmationTaskV1 CreateTask(bool callRestriction = false)
    {
        DateTimeOffset start = Now.AddMinutes(-1);
        return new IvrConfirmationTaskV1
        {
            Contract_version = IvrConfirmationTaskV1Contract_version.IvrOrderConfirmation_v1,
            Task_id = callRestriction ? "TASK-PG-REJECT" : "TASK-PG-ACCEPT",
            Correlation_id = "corr-postgres-p2-1",
            Created_at = start,
            Order_id = callRestriction ? "ORDER-PG-REJECT" : "ORDER-PG-ACCEPT",
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
            Attempt_policy_version = CandidateAttemptPolicies.Version,
            Max_customer_attempts = 2,
            Attempt_offsets_seconds = [0, 150],
            Phone_ref = "phone-ref-pg-p2-1",
            Phone_masked = "84xxxxx0001",
            Phone_validation_status = "VALID",
            Dial_token = "dial-token-pg-p2-1",
            Dial_token_expires_at = start.AddMinutes(5),
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
