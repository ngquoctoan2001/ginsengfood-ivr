using System.Text.Json;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.Policies;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

public sealed class MockEligibilityCapacityProvider : IEligibilityCapacityProvider
{
    public ValueTask<EligibilityCapacitySnapshot> GetCapacityAsync(
        EligibilityTaskRecord task,
        DateTimeOffset evaluatedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(new EligibilityCapacitySnapshot(
            true,
            true,
            "MOCK-CAPACITY-P2-2",
            1,
            0,
            0,
            0,
            null,
            "evidence://mock/p2-2/capacity-available"));
    }
}

public sealed class FailClosedEligibilityCapacityProvider : IEligibilityCapacityProvider
{
    public ValueTask<EligibilityCapacitySnapshot> GetCapacityAsync(
        EligibilityTaskRecord task,
        DateTimeOffset evaluatedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(new EligibilityCapacitySnapshot(
            false,
            false,
            "CAPACITY-SOURCE-UNAVAILABLE",
            0,
            0,
            0,
            0,
            "CAPACITY_PROVIDER_NOT_CONFIGURED",
            "evidence://ivr/p2-2/capacity-source-unavailable"));
    }
}

public sealed class EligibilityService(
    IEligibilityRepository repository,
    IEligibilityCapacityProvider capacityProvider,
    TimeProvider timeProvider) : IEligibilityService
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
            now);
        EligibilityEvaluation evaluation = EligibilityRules.Evaluate(snapshot);
        EligibilityCapacitySnapshot capacity = capacityNotEvaluated;
        if (evaluation.Eligible)
        {
            capacity = await GetCapacityFailClosedAsync(stored, now, cancellationToken)
                .ConfigureAwait(false);
            evaluation = EligibilityRules.Evaluate(snapshot with { Capacity = capacity });
        }

        return await repository.PersistAsync(
            taskId,
            evaluation,
            capacity,
            correlationId,
            cancellationToken).ConfigureAwait(false);
    }

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
        DateTimeOffset now)
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
            ReadEligibilityDecision(stored.Task.EligibilitySnapshotJson),
            lines,
            stored.Task.CallRestriction,
            false,
            stored.Task.PhoneValidationStatus ?? string.Empty,
            stored.Task.DialTokenExpiresAt,
            stored.Task.ConfirmationWindowStartedAt,
            stored.Task.ConfirmationWindowExpiresAt,
            stored.Task.CustomerTrustStatus,
            stored.Task.TrustedSkipAllowed == true,
            riskFlags,
            false,
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

    private static string? ReadEligibilityDecision(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty(
                    "decision",
                    out JsonElement decision)
                || decision.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            return decision.GetString();
        }
        catch (JsonException)
        {
            return null;
        }
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
        string executionMode = configuration["IVR_EXECUTION_MODE"]
            ?? configuration[$"{IvrOptions.SectionName}:{nameof(IvrOptions.ExecutionMode)}"]
            ?? IvrOptions.MockExecutionMode;
        services.TryAddSingleton<IEligibilityService, EligibilityService>();
        if (string.Equals(
                executionMode,
                IvrOptions.MockExecutionMode,
                StringComparison.OrdinalIgnoreCase))
        {
            services.TryAddSingleton<IEligibilityCapacityProvider,
                MockEligibilityCapacityProvider>();
        }
        else
        {
            services.TryAddSingleton<IEligibilityCapacityProvider,
                FailClosedEligibilityCapacityProvider>();
        }

        return services;
    }
}
