using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ivr.Domain.Confirmation;
using Ivr.Infrastructure.Contracts;
using Ivr.Infrastructure.Persistence.Entities;

namespace Ivr.Infrastructure.Callbacks;

public static class CallbackOutboxSnapshotFactory
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static ResultCallbackEntity Create(
        string resultId,
        CallJobEntity job,
        CallAttemptEntity attempt,
        ConfirmationTaskEntity task,
        NormalizedResult normalized,
        string evidenceRef,
        string auditRef,
        DateTimeOffset occurredAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resultId);
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(attempt);
        ArgumentNullException.ThrowIfNull(task);
        if (!string.Equals(task.ProgramType, job.ProgramType, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Task and job program snapshots do not match.");
        }

        return Create(
            resultId,
            job,
            attempt.AttemptNumber,
            normalized,
            evidenceRef,
            auditRef,
            occurredAt);
    }

    public static ResultCallbackEntity Create(
        string resultId,
        CallJobEntity job,
        int attemptNumber,
        NormalizedResult normalized,
        string evidenceRef,
        string auditRef,
        DateTimeOffset occurredAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resultId);
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(normalized);
        if (!normalized.IsFinal)
        {
            throw new InvalidOperationException("Only final IVR results may enter the callback outbox.");
        }

        if (!ResultContractPolicy.IsFinalCallbackResult(normalized.ResultType))
        {
            throw new InvalidOperationException(
                "Only the six final IVR result types may enter the callback outbox.");
        }

        string callbackId = string.Concat("CALLBACK-", resultId);
        CallResultSnapshot snapshot = CallResultSnapshot.Create(
            CallbackId.Create(callbackId),
            TaskId.Create(job.TaskId),
            OrderId.Create(job.OfficialOrderId),
            OrderVersion.Create(job.OrderVersionSnapshot),
            MapProgram(job.ProgramType),
            normalized.ResultType,
            normalized.Reason,
            normalized.IsCounted,
            normalized.IsFinal,
            attemptNumber,
            occurredAt,
            normalized.RecommendedCoreAction,
            EvidenceReference.Create(evidenceRef),
            AuditReference.Create(auditRef));
        string payload = JsonSerializer.Serialize(
            TargetV1CallbackMapper.ToSalesWire(snapshot),
            SerializerOptions);

        return new ResultCallbackEntity
        {
            CallbackId = callbackId,
            IvrCallResultId = resultId,
            TaskId = job.TaskId,
            OfficialOrderId = job.OfficialOrderId,
            IdempotencyKey = string.Concat("ivr-result:", resultId),
            ResultStatus = normalized.ResultStatus,
            ResultState = "PENDING_CORE_REVALIDATION",
            DeliveryStatus = "READY",
            RequiresCoreRevalidation = true,
            PayloadJson = payload,
            PayloadSha256 = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(payload))),
            CreatedAt = occurredAt,
        };
    }

    private static IvrProgramCode MapProgram(string programType) => programType switch
    {
        "GOLDEN_HOUR" => IvrProgramCode.GoldenHour,
        "TWENTY_FOUR_SEVEN" => IvrProgramCode.TwentyFourSeven,
        _ => throw new InvalidOperationException("Unsupported callback program type."),
    };
}
