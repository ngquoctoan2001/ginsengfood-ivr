using System.Text.Json;
using System.Text.Json.Nodes;
using Ivr.Domain.Policies;
using Ivr.Domain.Scripts;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

// Synthetic database graph; no production data, live adapter, or dependency replacement.
internal sealed class ApiMatrixFixture(PostgresPersistenceFixture fixture)
{
    private static DateTimeOffset Now => DateTimeOffset.UtcNow;
    private IDbContextFactory<IvrDbContext> Factory() =>
        fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>();
    public async Task SeedGraphAsync(
        bool includeTerminalResult = true,
        string attemptStatus = "NORMALIZED_TECHNICAL_RETRY",
        bool activeChannel = false,
        bool eligible = true)
    {
        DateTimeOffset startedAt = Now.AddMinutes(-1);
        DateTimeOffset deadline = Now.AddMinutes(4);
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        context.ConfirmationTasks.Add(new ConfirmationTaskEntity
        {
            Id = Guid.NewGuid(),
            TaskId = "TASK-P2-8",
            ContractVersion = "ivr-order-confirmation.v1",
            IdempotencyKey = "p2-8-task-idempotency",
            CorrelationId = "corr-p2-8-seed",
            OfficialOrderId = "ORDER-P2-8",
            OrderCode = "GF-P2-8",
            OrderVersion = "1",
            OrderState = "CONFIRMING",
            PaymentMethodSnapshot = "ONLINE",
            IvrConfirmationRequired = true,
            RiskFlagsJson = "[]",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyVersion = "gh-v1-candidate",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,150]",
            ConfirmationWindowStartedAt = startedAt,
            ConfirmationWindowExpiresAt = deadline,
            PhoneRef = "phone-ref-p2-8",
            PhoneMasked = "84xxxxx0065",
            PhoneValidationStatus = "VALID",
            DialTokenCiphertext = "enc:p2-8-test-token",
            DialTokenExpiresAt = deadline,
            PrivacySafeOrderSummaryJson = "{}",
            CallScriptTemplateId = "SCRIPT-ORDER-CONFIRM",
            CallScriptVersion = "v1-test-approved",
            EvidencePolicyVersion = "evidence-v1",
            PrivacyPolicyVersion = "privacy-v1",
            EligibilityDecision = eligible ? EligibilityDecisions.Eligible : null,
            EligibilitySnapshotJson = JsonSerializer.Serialize(new
            {
                decision = "ELIGIBLE",
                source_version = "matrix-sales-v1",
                captured_at = startedAt,
                source_available = true,
                blockers = Array.Empty<string>(),
            }),
            CallRestriction = false,
            NotForQuoteCartDraft = true,
            NoDirectOrderUpdate = true,
            CreatedAt = startedAt,
            ExpiresAt = deadline,
            AcceptedAt = startedAt,
            EvidenceRefsJson = "[\"evidence://ivr/p2-8/task\"]",
        });
        context.CallJobs.Add(new CallJobEntity
        {
            IvrCallJobId = "JOB-P2-8",
            TaskId = "TASK-P2-8",
            OfficialOrderId = "ORDER-P2-8",
            OrderVersionSnapshot = "1",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyCode = "gh-v1-candidate",
            Status = "DRY_RUN",
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
            Eligible = eligible,
            EligibilityDecision = eligible
                ? EligibilityDecisions.Eligible
                : EligibilityDecisions.Pending,
            QueueStatus = "HELD_MOCK",
            ScriptVersion = "SCRIPT-ORDER-CONFIRM:v1-test-approved",
            PrivacyPolicyVersion = "privacy-v1",
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            CreatedAt = startedAt,
            EvidenceRefsJson = "[\"evidence://ivr/p2-8/job\"]",
        });
        context.TaskIntakeOutbox.Add(new TaskIntakeOutboxEntity
        {
            OutboxId = Guid.NewGuid(),
            TaskId = "TASK-P2-8",
            IvrCallJobId = "JOB-P2-8",
            EventType = "IVR_TASK_READY_FOR_ELIGIBILITY",
            Status = "HELD_MOCK",
            CorrelationId = "corr-p2-8-seed",
            PayloadSha256 = new string('B', 64),
            CreatedAt = startedAt,
        });
        context.CallAttempts.Add(new CallAttemptEntity
        {
            IvrCallAttemptId = "ATTEMPT-P2-8",
            IvrCallJobId = "JOB-P2-8",
            TaskId = "TASK-P2-8",
            AttemptNumber = 1,
            MaxAttemptsSnapshot = 2,
            ScheduledAt = startedAt,
            ScheduledWindowExpiresAt = deadline,
            EndedAt = startedAt.AddSeconds(5),
            Status = attemptStatus,
            ResultStatus = "IVR_TECHNICAL_EXCEPTION",
            IsCountedCustomerAttempt = false,
            TechnicalRetryAllowed = true,
            TechnicalRetryCount = 0,
            TechnicalExceptionType = "MOCK_ADAPTER_FAULT",
            SimChannelId = "SIM-P2-8",
            PolicyVersion = "gh-v1-candidate",
            ScriptVersion = "v1-test-approved",
            EvidenceRefsJson = "[\"evidence://ivr/p2-8/attempt\"]",
        });
        context.SimChannels.Add(new SimChannelEntity
        {
            SimChannelId = "SIM-P2-8",
            SimNumberRef = "sim-ref-p2-8",
            Enabled = true,
            Status = activeChannel ? "ACTIVE_CALL" : "IDLE",
            AdapterMode = "MOCK",
            ExecutionMode = IvrOptions.MockExecutionMode,
            ProviderName = "MOCK",
            ActiveCallJobId = activeChannel ? "JOB-P2-8" : null,
            LastHealthCheckAt = Now,
            LeaseToken = activeChannel ? Guid.NewGuid() : null,
            LeaseFencingGeneration = activeChannel ? 1 : 0,
            LeasedByWorkerId = activeChannel ? "active-worker" : null,
            LeaseAcquiredAt = activeChannel ? startedAt : null,
            LeaseExpiresAt = activeChannel ? deadline : null,
        });
        context.TechnicalExceptions.Add(new TechnicalExceptionEntity
        {
            TechnicalExceptionId = "TECH-P2-8",
            IvrCallAttemptId = "ATTEMPT-P2-8",
            ExceptionType = "MOCK_ADAPTER_FAULT",
            CustomerAttemptCounted = false,
            TechnicalRetryAllowed = true,
            TechnicalRetryCount = 0,
            CorrelationId = "corr-p2-8-tech",
            CreatedAt = startedAt,
        });
        if (includeTerminalResult)
        {
            context.CallResults.Add(new CallResultEntity
            {
                IvrCallResultId = "RESULT-P2-8",
                IvrCallJobId = "JOB-P2-8",
                TaskId = "TASK-P2-8",
                OfficialOrderId = "ORDER-P2-8",
                OrderVersionSnapshot = "1",
                OrderVersionSeenByIvr = "1",
                FinalResultStatus = "IVR_NO_ANSWER_FINAL",
                ResultType = "IVR_NO_ANSWER_FINAL",
                ResultReason = "MAX_CUSTOMER_ATTEMPTS_REACHED",
                IsCountedCustomerAttempt = true,
                IsFinalForIvr = true,
                RecommendedCoreAction = "NO_STATE_CHANGE_WAIT_FOR_TIMEOUT",
                CoreOrderHandoffRequired = true,
                HumanReviewRequired = true,
                InputSignalOnly = true,
                NoDirectOrderUpdate = true,
                NoPaymentOrRevenueEffect = true,
                CreatedAt = startedAt.AddSeconds(10),
                EvidenceRefsJson = "[\"evidence://ivr/p2-8/result\"]",
            });
            context.ResultCallbacks.Add(new ResultCallbackEntity
            {
                CallbackId = "CALLBACK-P2-8",
                IvrCallResultId = "RESULT-P2-8",
                TaskId = "TASK-P2-8",
                OfficialOrderId = "ORDER-P2-8",
                IdempotencyKey = "callback-p2-8",
                ResultStatus = "IVR_NO_ANSWER_FINAL",
                ResultState = "PENDING_CORE_REVALIDATION",
                DeliveryStatus = "READY",
                RequiresCoreRevalidation = true,
                PayloadJson = "{}",
                PayloadSha256 = new string('A', 64),
                CreatedAt = startedAt.AddSeconds(10),
            });
            context.ReviewItems.Add(new ReviewItemEntity
            {
                ReviewItemId = "REVIEW-P2-8",
                SourceType = "IVR_CALL_RESULT",
                SourceId = "RESULT-P2-8",
                Reason = "verify final result evidence",
                Status = "OPEN",
                CorrelationId = "corr-p2-8-review",
                CreatedAt = startedAt.AddSeconds(10),
            });
        }

        await context.SaveChangesAsync();
    }

    public static JsonObject CreateBody(
        string program = "GOLDEN_HOUR",
        string payment = "ONLINE")
    {
        DateTimeOffset start = DateTimeOffset.UtcNow.AddMinutes(-1);
        int windowSeconds = program == "GOLDEN_HOUR" ? 300 : 900;
        int secondOffset = program == "GOLDEN_HOUR" ? 150 : 450;
        return new JsonObject
        {
            ["contract_version"] = "ivr-order-confirmation.v1",
            ["task_id"] = string.Concat("TASK-API-", program),
            ["correlation_id"] = "corr-api-p2-1",
            ["created_at"] = start,
            ["order_id"] = string.Concat("ORDER-API-", program),
            ["order_code"] = "GF-API-001",
            ["order_code_short"] = "API001",
            ["order_version"] = "17",
            ["order_state"] = "CONFIRMING",
            ["payment_method_snapshot"] = payment,
            ["ivr_confirmation_required"] = true,
            ["is_ivr_callable"] = true,
            ["program_code"] = program,
            ["confirmation_window_started_at"] = start,
            ["confirmation_window_expires_at"] = start.AddSeconds(windowSeconds),
            ["attempt_policy_version"] = CandidateAttemptPolicies.Version,
            ["max_customer_attempts"] = 2,
            ["attempt_offsets_seconds"] = new JsonArray(0, secondOffset),
            ["phone_ref"] = "phone-ref-api-p2-1",
            ["phone_masked"] = "84xxxxx0001",
            ["phone_validation_status"] = "VALID",
            ["dial_token"] = "dial-token-api-p2-1",
            ["dial_token_expires_at"] = start.AddSeconds(windowSeconds),
            ["privacy_safe_order_summary"] = new JsonObject
            {
                ["customer_display_name"] = "chị An",
                ["order_code_short"] = "API001",
                ["items"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["public_name"] = "Nước hồng sâm",
                        ["quantity"] = 2,
                        ["unit_label"] = "hộp",
                    },
                },
                ["total_amount"] = 560_000,
                ["currency"] = "VND",
                ["delivery_area_short"] = "Phường Bến Nghé, Quận Một",
                ["program_display_name"] = program == "GOLDEN_HOUR"
                    ? "Giờ Vàng"
                    : "Bán hàng hai mươi tư trên bảy",
                ["locale"] = "vi-VN",
            },
            ["call_restriction"] = false,
            ["eligibility_snapshot"] = new JsonObject
            {
                ["decision"] = "ELIGIBLE",
                ["source"] = "api-test",
            },
            ["evidence_ref"] = "evidence://api/p2-1",
        };
    }

    public async Task SeedAnalyticsBucketAsync()
    {
        await using IvrDbContext db = await Factory().CreateDbContextAsync();
        ConfirmationTaskEntity task = await db.ConfirmationTasks.AsNoTracking().SingleAsync();
        CallJobEntity job = await db.CallJobs.AsNoTracking().SingleAsync();
        CallResultEntity result = await db.CallResults.AsNoTracking().SingleAsync();
        for (int index = 1; index <= 5; index++)
        {
            var nextTask = JsonSerializer.Deserialize<ConfirmationTaskEntity>(JsonSerializer.Serialize(task))!;
            nextTask.Id = Guid.NewGuid();
            nextTask.TaskId += "-" + index;
            nextTask.OfficialOrderId += "-" + index;
            nextTask.IdempotencyKey += "-" + index;
            var nextJob = JsonSerializer.Deserialize<CallJobEntity>(JsonSerializer.Serialize(job))!;
            nextJob.IvrCallJobId += "-" + index;
            nextJob.TaskId = nextTask.TaskId;
            nextJob.OfficialOrderId = nextTask.OfficialOrderId;
            var nextResult = JsonSerializer.Deserialize<CallResultEntity>(JsonSerializer.Serialize(result))!;
            nextResult.IvrCallResultId += "-" + index;
            nextResult.IvrCallJobId = nextJob.IvrCallJobId;
            nextResult.TaskId = nextTask.TaskId;
            nextResult.OfficialOrderId = nextTask.OfficialOrderId;
            db.AddRange(nextTask, nextJob, nextResult);
        }
        await db.SaveChangesAsync();
    }

    public static JsonObject Draft() => new()
    {
        ["template_id"] = TargetV1SpeechPolicy.MockTemplateId,
        ["version"] = "v-matrix",
        ["template_text"] = TargetV1SpeechPolicy.CanonicalVietnameseTemplate,
        ["reason"] = "Matrix synthetic draft",
    };
}
