using System.Text.Json;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.ChaosTests;

/// <summary>
/// W-0042 / P6-3. Fixtures are built from the same canonical seed the integration suite reads
/// (<c>seed/sales-target-v1.sample.json</c>), so a chaos run exercises the row shapes real intake
/// produces rather than a shape invented here. Copied rather than shared: a chaos suite compiled
/// against another test assembly's internals breaks whenever that assembly is refactored, and the
/// failure then reads as a resilience regression.
/// </summary>
internal static class ChaosFixtures
{
    /// <summary>
    /// Fixed clock shared by the copied seed helpers, matching the integration suite's anchor so
    /// the seeded windows and offsets line up with the rows real intake would produce.
    /// </summary>
    internal static readonly DateTimeOffset Now = new(2026, 8, 13, 9, 0, 0, TimeSpan.Zero);

    internal static ConfirmationTaskEntity ReadCanonicalTask(string suffix = "canonical")
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

    internal static CallJobEntity CreateJob(
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

    internal static CallResultEntity CreateResult(
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

    internal static string FindRepositoryFile(params string[] segments)
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
    internal static async Task SeedReadyJobAsync(
        IDbContextFactory<IvrDbContext> factory,
        string taskId,
        string jobId,
        DateTimeOffset startedAt,
        DateTimeOffset? expiresAt = null)
    {
        DateTimeOffset deadline = expiresAt ?? startedAt.AddMinutes(5);
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        context.ConfirmationTasks.Add(new ConfirmationTaskEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            ContractVersion = "ivr-order-confirmation.v1",
            IdempotencyKey = string.Concat("scheduler:", taskId),
            CorrelationId = string.Concat("corr-", taskId),
            OfficialOrderId = string.Concat("ORDER-", taskId),
            OrderCode = string.Concat("GF-", taskId),
            OrderVersion = "1",
            OrderState = "CONFIRMING",
            PaymentMethodSnapshot = "ONLINE",
            IvrConfirmationRequired = true,
            RiskFlagsJson = "[]",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyVersion = CandidateAttemptPolicies.Version,
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowStartedAt = startedAt,
            ConfirmationWindowExpiresAt = deadline,
            PhoneRef = string.Concat("phone-ref-", taskId),
            PhoneMasked = "84xxxxx0020",
            PhoneValidationStatus = "VALID",
            DialTokenCiphertext = string.Concat("enc:", taskId),
            DialTokenExpiresAt = deadline,
            PrivacySafeOrderSummaryJson = "{}",
            CallScriptTemplateId = "SCRIPT-ORDER-CONFIRM",
            CallScriptVersion = "v1-test-approved",
            EvidencePolicyVersion = "evidence-v1",
            PrivacyPolicyVersion = "privacy-v1",
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            EligibilitySnapshotJson = "{\"decision\":\"ELIGIBLE\"}",
            CallRestriction = false,
            CreatedAt = startedAt,
            ExpiresAt = deadline,
            AcceptedAt = startedAt,
        });
        context.CallJobs.Add(new CallJobEntity
        {
            IvrCallJobId = jobId,
            TaskId = taskId,
            OfficialOrderId = string.Concat("ORDER-", taskId),
            OrderVersionSnapshot = "1",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyCode = CandidateAttemptPolicies.Version,
            Status = "READY_FOR_SCHEDULER",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowSeconds = 300,
            AttemptScheduleJson = JsonSerializer.Serialize(new[]
            {
                startedAt,
                startedAt.AddSeconds(150),
            }),
            T0At = startedAt,
            ExpiresAt = deadline,
            Eligible = true,
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            QueueStatus = "QUEUED",
            ScriptVersion = "SCRIPT-ORDER-CONFIRM:v1-test-approved",
            PrivacyPolicyVersion = "privacy-v1",
            CreatedAt = startedAt,
        });
        await context.SaveChangesAsync();
    }

    internal static async Task SeedChannelAsync(
        IDbContextFactory<IvrDbContext> factory,
        string channelId,
        int priorFailCount = 0)
    {
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        context.SimChannels.Add(new SimChannelEntity
        {
            SimChannelId = channelId,
            SimNumberRef = string.Concat("sim-ref-", channelId),
            Enabled = true,
            Status = "IDLE",
            AdapterMode = "VENDOR",
            ExecutionMode = IvrOptions.LabRealSimExecutionMode,
            ProviderName = "VENDOR",
            LastHealthCheckAt = Now,
            // Lets a scenario start one failure below the DT-04 threshold so the next one crosses
            // it, without cycling three leases to get there.
            FailCount = priorFailCount,
        });
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// A task, its job, its final result and one READY outbox row, which is the smallest complete
    /// state a callback scenario can start from. Lives here rather than in a scenario file because
    /// three scenarios need exactly this shape and each used to carry its own byte-identical copy.
    /// </summary>
    internal static async Task<ResultCallbackEntity> SeedReadyCallbackAsync(
        ChaosEnvironment chaos,
        string suffix)
    {
        ArgumentNullException.ThrowIfNull(chaos);
        ConfirmationTaskEntity task = ReadCanonicalTask(suffix);
        CallJobEntity job = CreateJob(task, task.MaxAttempts, task.AttemptOffsetsSecondsJson);
        CallResultEntity result = CreateResult(task, job);

        await using (IvrDbContext context = await chaos.DbContextFactory.CreateDbContextAsync())
        {
            context.AddRange(task, job, result);
            await context.SaveChangesAsync();
        }

        string payload = $"{{\"task_id\":\"{task.TaskId}\",\"result_type\":\"IVR_CONFIRMED\"}}";
        var callback = new ResultCallbackEntity
        {
            CallbackId = $"CALLBACK-{suffix}",
            IvrCallResultId = result.IvrCallResultId,
            TaskId = task.TaskId,
            OfficialOrderId = task.OfficialOrderId,
            IdempotencyKey = $"callback-idem-{suffix}",
            ResultStatus = "IVR_CONFIRMED",
            ResultState = "PENDING_CORE_REVALIDATION",
            DeliveryStatus = "READY",
            RequiresCoreRevalidation = true,
            PayloadJson = payload,
            PayloadSha256 = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(payload))),
            CreatedAt = task.CreatedAt,
        };
        await chaos.Services
            .GetRequiredService<ICallbackOutboxRepository>()
            .EnqueueAsync(callback);
        return callback;
    }
}
