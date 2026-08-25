using System.Globalization;
using System.Text.Json;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Policies;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Observability;
using Ivr.Infrastructure.Repositories;
using Ivr.Infrastructure.Scheduling;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Application;

public interface IEligibilityCapacityProvider
{
    public ValueTask<EligibilityCapacitySnapshot> GetCapacityAsync(
        EligibilityTaskRecord task,
        DateTimeOffset evaluatedAt,
        CancellationToken cancellationToken = default);
}

public interface IEligibilityService
{
    public Task<EligibilityEvaluation> EvaluateAsync(
        string taskId,
        string correlationId,
        CancellationToken cancellationToken = default);
}

public sealed class SchedulerEligibilityCapacityProvider(
    ISchedulerCapacityService schedulerCapacity) : IEligibilityCapacityProvider
{
    public async ValueTask<EligibilityCapacitySnapshot> GetCapacityAsync(
        EligibilityTaskRecord task,
        DateTimeOffset evaluatedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        int[] offsets = DeserializeOffsets(task.CallJob.AttemptOffsetsSecondsJson);
        Ivr.Domain.Confirmation.IvrProgramCode program = task.CallJob.ProgramType switch
        {
            "GOLDEN_HOUR" => Ivr.Domain.Confirmation.IvrProgramCode.GoldenHour,
            "TWENTY_FOUR_SEVEN" => Ivr.Domain.Confirmation.IvrProgramCode.TwentyFourSeven,
            _ => throw new InvalidOperationException("Stored scheduler program is unknown."),
        };
        int riskScore = SchedulerCapacityMapper.RiskScore(task.Task.RiskFlagsJson);
        SchedulerCapacitySnapshot capacity = await schedulerCapacity.CalculateAsync(
            new SchedulerCapacityRequest(
                task.CallJob.IvrCallJobId,
                program,
                task.CallJob.T0At,
                task.CallJob.ExpiresAt,
                task.CallJob.CreatedAt,
                offsets,
                riskScore),
            evaluatedAt,
            cancellationToken).ConfigureAwait(false);
        return new EligibilityCapacitySnapshot(
            capacity.SourceAvailable,
            capacity.FitsBeforeDeadline,
            capacity.SessionId,
            capacity.ActiveChannelCount,
            capacity.PendingDispatches,
            capacity.ExpiredJobs,
            capacity.MissedDeadlineCount,
            capacity.ShortageReason,
            capacity.EvidenceRef);
    }

    private static int[] DeserializeOffsets(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<int[]>(json)
                ?? throw new InvalidOperationException("Stored attempt offsets are missing.");
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Stored attempt offsets are unreadable.", exception);
        }
    }
}

public sealed class EligibilityService(
    IEligibilityRepository repository,
    IEligibilityCapacityProvider capacityProvider,
    TimeProvider timeProvider,
    IOptions<IvrOptions> ivrOptions) : IEligibilityService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<EligibilityEvaluation> EvaluateAsync(
        string taskId,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskId);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);
        PiiGuard.EnsureSafeText(taskId);
        PiiGuard.EnsureSafeText(correlationId);
        EligibilityTaskRecord stored = await repository.FindAsync(taskId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException("The confirmation task was not found.");
        DateTimeOffset now = timeProvider.GetUtcNow();
        (string evidenceRef, bool evidenceAvailable) = FirstEvidenceOrFailClosed(stored);
        EligibilityCapacitySnapshot capacityNotEvaluated = new(
            true,
            true,
            "CAPACITY-NOT-EVALUATED",
            0,
            0,
            0,
            0,
            null,
            string.Concat(evidenceRef.Split('#')[0], "#eligibility/capacity-not-evaluated"));
        EligibilitySnapshot snapshot = Map(
            stored,
            capacityNotEvaluated,
            evidenceRef,
            evidenceAvailable,
            now,
            ivrOptions.Value.ReturningCustomerSkipEnabled);
        EligibilityEvaluation evaluation = EligibilityRules.Evaluate(snapshot);
        EligibilityCapacitySnapshot capacity = capacityNotEvaluated;
        if (evaluation.Eligible)
        {
            capacity = await GetCapacityFailClosedAsync(stored, now, cancellationToken)
                .ConfigureAwait(false);
            evaluation = EligibilityRules.Evaluate(snapshot with { Capacity = capacity });
        }

        // W-0041 / P6-2, DO-06. Fail-closed is what the downstream-health alert is built on, so it
        // has to be counted where it is decided: at rest, a task that was held and a task nobody
        // ever sent look identical. One measurement per evaluation carrying the reason that drove
        // it -- counting every reason would make a single hold read as several.
        if (!evaluation.Eligible && FailClosedDecisions.Contains(evaluation.Decision))
        {
            IvrTelemetry.RecordFailClosed(
                (TelemetryTags.Program, stored.Task.ProgramType),
                (TelemetryTags.Decision, evaluation.Decision),
                (TelemetryTags.ReasonCode, evaluation.Reasons.Count > 0
                    ? evaluation.Reasons[0].Code
                    : "UNSPECIFIED"));
        }

        return await repository.PersistAsync(
            taskId,
            evaluation,
            capacity,
            correlationId,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Decisions that mean the system refused to proceed because it could not prove it was safe
    /// to. <c>TASK_SKIPPED_TRUSTED_CUSTOMER</c> is deliberately absent: that is policy choosing
    /// not to call, not the system failing closed, and folding the two together would make a
    /// working trust rule look like a downstream outage on the alert.
    /// </summary>
    private static readonly HashSet<string> FailClosedDecisions = new(StringComparer.Ordinal)
    {
        EligibilityDecisions.BlockedOperational,
        EligibilityDecisions.HeldAdminReview,
        EligibilityDecisions.CapacityException,
    };

    private async ValueTask<EligibilityCapacitySnapshot> GetCapacityFailClosedAsync(
        EligibilityTaskRecord stored,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        try
        {
            EligibilityCapacitySnapshot capacity = await capacityProvider.GetCapacityAsync(
                stored,
                now,
                cancellationToken).ConfigureAwait(false);
            ArgumentException.ThrowIfNullOrWhiteSpace(capacity.EvidenceRef);
            PiiGuard.EnsureSafeText(JsonSerializer.Serialize(capacity, JsonOptions));
            return capacity;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return new EligibilityCapacitySnapshot(
                false,
                false,
                "CAPACITY-SOURCE-ERROR",
                0,
                0,
                0,
                0,
                "CAPACITY_SOURCE_UNREADABLE",
                "evidence://ivr/p2-2/capacity-source-error");
        }
    }

    private static EligibilitySnapshot Map(
        EligibilityTaskRecord stored,
        EligibilityCapacitySnapshot capacity,
        string evidenceRef,
        bool evidenceAvailable,
        DateTimeOffset now,
        bool returningCustomerSkipEnabled)
    {
        SellableStatusLine[]? sourceLines = DeserializeOrNull<SellableStatusLine[]>(
            stored.Task.SellableStatusJson);
        EligibilitySellableLine[]? lines = sourceLines?.Select(line =>
            new EligibilitySellableLine(
                line.Decision switch
                {
                    SellableStatusLineDecision.SELLABLE =>
                        EligibilitySellableDecision.Sellable,
                    SellableStatusLineDecision.NOT_SELLABLE =>
                        EligibilitySellableDecision.NotSellable,
                    SellableStatusLineDecision.BLOCKED =>
                        EligibilitySellableDecision.Blocked,
                    _ => EligibilitySellableDecision.Unknown,
                },
                line.Recall_hold,
                line.Sale_lock,
                line.Quality_hold,
                line.Stock_available,
                line.Batch_released,
                line.Trace_ready,
                line.Captured_at)).ToArray();
        string[] riskFlags = DeserializeOrNull<string[]>(stored.Task.RiskFlagsJson)
            ?? (string.IsNullOrWhiteSpace(stored.Task.RiskFlagsJson)
                ? []
                : ["RISK_SNAPSHOT_UNREADABLE"]);
        return new EligibilitySnapshot(
            stored.Task.OrderState,
            stored.Task.ProgramType,
            stored.Task.PaymentMethodSnapshot,
            stored.Task.IvrConfirmationRequired,
            stored.Task.NotForQuoteCartDraft,
            ReadEligibilityEvidence(
                stored.Task.EligibilitySnapshotJson,
                stored.Task.EligibilitySnapshotHash),
            lines,
            ReadVoiceContactEvidence(
                stored.Task.EligibilitySnapshotJson,
                stored.Task.CallRestriction),
            stored.Task.PhoneValidationStatus ?? string.Empty,
            stored.Task.DialTokenExpiresAt,
            stored.Task.ConfirmationWindowStartedAt,
            stored.Task.ConfirmationWindowExpiresAt,
            ReadTrustEvidence(
                stored.Task.EligibilitySnapshotJson,
                stored.Task.CustomerTrustStatus,
                stored.Task.TrustedSkipAllowed,
                riskFlags,
                returningCustomerSkipEnabled),
            capacity,
            evidenceAvailable,
            evidenceRef,
            now);
    }

    private static T? DeserializeOrNull<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    /// <summary>
    /// Projects the stored Sales <c>eligibility_snapshot</c> onto the typed shape IVR validates
    /// against (W-0030 / P4-2 §2.1-2.2). The expected shape is published as a linked evidence
    /// reference at <c>specs/api/evidence/eligibility-snapshot.v1.schema.json</c>; the wire field
    /// stays an open object until <c>OD-V1-03</c> closes, so this reader tolerates extra keys and
    /// reports what it could not interpret instead of throwing.
    /// </summary>
    private static EligibilityEvidence ReadEligibilityEvidence(string? json, string? snapshotHash)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return EligibilityEvidence.Absent;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Null || root.ValueKind == JsonValueKind.Undefined)
            {
                return EligibilityEvidence.Absent;
            }

            if (root.ValueKind != JsonValueKind.Object)
            {
                return EligibilityEvidence.Malformed(snapshotHash);
            }

            return new EligibilityEvidence(
                EligibilityEvidenceState.Present,
                ReadString(root, "decision"),
                ReadString(root, "source_version"),
                ReadTimestamp(root, "captured_at"),
                ReadBoolean(root, "source_available") ?? true,
                ReadStringArray(root, "blockers"),
                snapshotHash);
        }
        catch (JsonException)
        {
            return EligibilityEvidence.Malformed(snapshotHash);
        }
    }

    /// <summary>
    /// Projects the transactional voice-contact decision (W-0031 / P4-3 §2.1-2.2).
    /// <para>
    /// Reads exactly three keys under <c>voice_restriction</c>. It never looks at
    /// <c>sms_opt_out</c>, <c>marketing_consent</c>, <c>email_opt_out</c> or any other
    /// marketing-consent key, even when Sales includes them in the same bag: a customer who
    /// declined marketing has not declined a transactional order-confirmation call, and the two
    /// decisions have different legal bases. The domain type this returns has no field that could
    /// hold such a value, so the separation survives future edits to this reader.
    /// </para>
    /// The contract field <c>call_restriction</c> stays authoritative for the verdict; the bag
    /// only adds provenance the wire contract cannot carry yet (<c>OD-V1-03</c>).
    /// </summary>
    private static VoiceContactEvidence ReadVoiceContactEvidence(
        string? json,
        bool columnRestriction)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new VoiceContactEvidence(columnRestriction, true, null);
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                // Unreadable bags are already held by the eligibility evidence rules (W-0030).
                // Nothing here may quietly upgrade that into a usable voice decision.
                return VoiceContactEvidence.Unknown;
            }

            string? inheritedVersion = ReadString(root, "source_version");
            if (!root.TryGetProperty("voice_restriction", out JsonElement voice)
                || voice.ValueKind != JsonValueKind.Object)
            {
                return new VoiceContactEvidence(columnRestriction, true, inheritedVersion);
            }

            return new VoiceContactEvidence(
                ReadBoolean(voice, "restricted") ?? columnRestriction,
                ReadBoolean(voice, "source_available") ?? true,
                ReadString(voice, "source_version") ?? inheritedVersion);
        }
        catch (JsonException)
        {
            return VoiceContactEvidence.Unknown;
        }
    }

    /// <summary>
    /// Projects the risk evidence behind the returning-customer skip (W-0031 / P4-3 §2.3,
    /// owner decision <c>OD-15</c>).
    /// <para>
    /// <c>trust.resolver_version</c> falls back to the snapshot-level <c>source_version</c>, the
    /// same way the voice decision does: the risk evaluation belongs to the same evidence
    /// capture, and <c>source_version</c> is already mandatory in the rules that run before this
    /// one. That fallback is what leaves Sales a single remaining obligation —
    /// <c>trust.risk_evidence_available</c> — instead of a whole resolver contract.
    /// </para>
    /// Every read failure here leaves <c>riskEvidenceAvailable</c> false, which requires the call.
    /// </summary>
    private static TrustResolverEvidence ReadTrustEvidence(
        string? json,
        string? trustStatus,
        bool? skipAllowedBySales,
        IReadOnlyList<string> riskFlags,
        bool skipFeatureEnabled)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return TrustResolverEvidence.RequireIvr with
            {
                SkipFeatureEnabled = skipFeatureEnabled,
                SkipAllowedBySales = skipAllowedBySales,
                TrustStatus = trustStatus,
                RiskFlags = riskFlags,
            };
        }

        bool resolverAvailable = false;
        string? resolverVersion = null;
        bool riskEvidenceAvailable = false;
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                string? inheritedVersion = ReadString(root, "source_version");
                resolverVersion = inheritedVersion;
                if (root.TryGetProperty("trust", out JsonElement trust)
                    && trust.ValueKind == JsonValueKind.Object)
                {
                    resolverAvailable = ReadBoolean(trust, "resolver_available") ?? false;
                    resolverVersion = ReadString(trust, "resolver_version") ?? inheritedVersion;
                    riskEvidenceAvailable =
                        ReadBoolean(trust, "risk_evidence_available") ?? false;
                }
            }
        }
        catch (JsonException)
        {
            // Fall through to the require-IVR defaults already assigned above.
        }

        return new TrustResolverEvidence(
            skipFeatureEnabled,
            skipAllowedBySales,
            resolverAvailable,
            resolverVersion,
            trustStatus,
            riskEvidenceAvailable,
            riskFlags);
    }

    private static string? ReadString(JsonElement root, string property) =>
        root.TryGetProperty(property, out JsonElement value)
        && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static bool? ReadBoolean(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out JsonElement value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            // A present-but-non-boolean flag is a producer defect. Reporting false makes the
            // rules hold the task for review rather than silently treating it as available.
            _ => false,
        };
    }

    private static DateTimeOffset? ReadTimestamp(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out JsonElement value)
            || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return DateTimeOffset.TryParse(
            value.GetString(),
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out DateTimeOffset parsed)
            ? parsed
            : null;
    }

    private static string[] ReadStringArray(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out JsonElement value)
            || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var items = new List<string>();
        foreach (JsonElement item in value.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String
                && item.GetString() is { Length: > 0 } text)
            {
                items.Add(text);
            }
        }

        return items.ToArray();
    }

    private static (string EvidenceRef, bool Available) FirstEvidenceOrFailClosed(
        EligibilityTaskRecord stored)
    {
        string[]? evidenceRefs = DeserializeOrNull<string[]>(stored.Task.EvidenceRefsJson);
        string? sourceEvidence = evidenceRefs?.FirstOrDefault(PiiGuard.IsSafeText);
        bool available = !string.IsNullOrWhiteSpace(sourceEvidence);
        string evidenceRef = sourceEvidence
            ?? string.Concat("evidence://ivr/p2-2/source-missing/", stored.Task.TaskId);
        PiiGuard.EnsureSafeText(evidenceRef);
        return (evidenceRef, available);
    }
}

public static class EligibilityServiceCollectionExtensions
{
    public static IServiceCollection AddIvrEligibility(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.TryAddSingleton<IEligibilityService, EligibilityService>();
        services.TryAddSingleton<IEligibilityCapacityProvider,
            SchedulerEligibilityCapacityProvider>();

        return services;
    }
}
