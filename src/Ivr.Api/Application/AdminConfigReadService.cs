using System.Text.Json;
using Ivr.Api.Admin;
using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Domain.Scripts;
using Ivr.Infrastructure.Callbacks;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Scripts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Application;

public interface IAdminConfigReadService
{
    public Task<ScriptCatalogApiResult> GetScriptCatalogAsync(CancellationToken cancellationToken);

    public Task<IntegrationStatusApiResult> GetIntegrationStatusAsync(
        string environment,
        CancellationToken cancellationToken);

    public Task<ReviewQueueApiResult> ListReviewItemsAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}

/// <summary>
/// Back-office read projections (W-0096): script catalogue, integration status
/// and the human review queue.
///
/// Read-only by construction. Script lifecycle transitions stay in
/// <see cref="IScriptContentManager"/> and are not exposed: approval is an owner
/// action governed by `OD-V1-15`, not a console button.
/// </summary>
public sealed class AdminConfigReadService(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    IFeatureFlags featureFlags,
    IOptions<IvrOptions> ivrOptions,
    IOptions<ScriptContentOptions> scriptOptions,
    TimeProvider timeProvider,
    // Optional on purpose. This is a read service for admin screens; a host that serves the
    // console without running the callback delivery stack must still start. When the stack is
    // absent the ORDER_CORE card reports NOT_WIRED, which is the truth for that host.
    IOptions<CallbackDeliveryOptions>? callbackOptions = null,
    CallbackCircuitBreaker? callbackCircuit = null) : IAdminConfigReadService
{
    public const int MaxPageSize = 100;
    public const int DefaultPageSize = 25;

    /// <summary>
    /// Displayed so operators are reminded what may never enter a call script.
    /// Mirrors `specs/ui/04` and the phase-8 privacy list.
    /// </summary>
    private static readonly string[] ProhibitedVariables =
    [
        "FULL_ADDRESS",
        "MEMBER_TIER",
        "DIAMOND",
        "PAYMENT_DETAIL",
        "ORDER_HISTORY",
        "AI_CRM_CONTENT",
        "HEALTH",
    ];

    /// <summary>
    /// Storage spellings, matching `ck_ivr_script_approvals_type`. The mapper that
    /// owns this translation is internal to Ivr.Infrastructure, so the check
    /// constraint is the shared source of truth.
    /// </summary>
    private static readonly string[] RequiredApprovalTypes =
    [
        "MOCK_TEST",
        "LAB",
        "CONTENT",
        "PRIVACY_LEGAL",
    ];

    public async Task<ScriptCatalogApiResult> GetScriptCatalogAsync(
        CancellationToken cancellationToken)
    {
        await using IvrDbContext context = await dbContextFactory.CreateDbContextAsync(
            cancellationToken).ConfigureAwait(false);

        List<ScriptVersionEntity> versions = await context.ScriptVersions.AsNoTracking()
            .Include(version => version.Approvals)
            .OrderBy(version => version.TemplateId)
            .ThenBy(version => version.Version)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new ScriptCatalogApiResult(
            timeProvider.GetUtcNow(),
            ivrOptions.Value.ExecutionMode,
            scriptOptions.Value.ProductionTargetV1FieldsApproved,
            TargetV1SpeechPolicy.AllowedInputFields,
            ProhibitedVariables,
            [
                new DtmfKeyView("1", "CONFIRM", true),
                new DtmfKeyView("0", "CANCEL", true),
                // AS-07: human handoff is not enabled and cannot be turned on here.
                new DtmfKeyView("9", "NOT_ENABLED", false),
            ],
            RequiredApprovalTypes,
            versions.Select(MapVersion).ToArray());
    }

    public async Task<IntegrationStatusApiResult> GetIntegrationStatusAsync(
        string environment,
        CancellationToken cancellationToken)
    {
        string normalizedEnvironment = NormalizeEnvironment(environment);
        // A provider read failure returns the fail-closed default rather than
        // throwing, so the status screen still renders and still shows the kill
        // switch as engaged.
        FeatureFlagReadResult read = await featureFlags
            .GetSnapshotAsync(normalizedEnvironment, forceFresh: true, cancellationToken)
            .ConfigureAwait(false);
        FeatureFlagSnapshot flags = read.Snapshot;

        await using IvrDbContext context = await dbContextFactory.CreateDbContextAsync(
            cancellationToken).ConfigureAwait(false);
        DateTimeOffset now = timeProvider.GetUtcNow();

        int enabledChannels = await context.SimChannels.AsNoTracking()
            .CountAsync(channel => channel.Enabled, cancellationToken)
            .ConfigureAwait(false);
        int totalChannels = await context.SimChannels.AsNoTracking()
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
        DateTimeOffset? lastHealthCheck = await context.SimChannels.AsNoTracking()
            .MaxAsync(channel => (DateTimeOffset?)channel.LastHealthCheckAt, cancellationToken)
            .ConfigureAwait(false);

        List<CapacityIncidentEntity> incidents = await context.CapacityIncidents.AsNoTracking()
            .Where(incident => incident.Status == "OPEN")
            .OrderByDescending(incident => incident.OpenedAt)
            .Take(10)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<ReviewItemEntity> reviews = await context.ReviewItems.AsNoTracking()
            .Where(item => item.Status == "OPEN")
            .OrderByDescending(item => item.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new IntegrationStatusApiResult(
            now,
            flags.ExecutionMode,
            flags.SalesProvider,
            flags.SimProvider,
            flags.RealCustomerCallAllowed,
            flags.GlobalDialKillSwitch,
            flags.AttemptPolicyVersion,
            flags.Revision,
            // P6-1 (W-0040) owns real dependency probing. Until then IVR reports
            // only what it can observe about itself and marks the rest NOT_WIRED,
            // rather than painting an unprobed dependency green.
            DependencyProbingAvailable: false,
            BuildDependencies(
                flags,
                totalChannels,
                enabledChannels,
                lastHealthCheck,
                callbackOptions?.Value,
                callbackCircuit?.Snapshot(),
                timeProvider.GetUtcNow()),
            BuildFailClosedEvents(incidents, reviews));
    }

    public async Task<ReviewQueueApiResult> ListReviewItemsAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        RequireSafe(status, nameof(status));
        int effectivePage = page < 1 ? 1 : page;
        int effectivePageSize = pageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => pageSize,
        };

        await using IvrDbContext context = await dbContextFactory.CreateDbContextAsync(
            cancellationToken).ConfigureAwait(false);

        IQueryable<ReviewItemEntity> query = context.ReviewItems.AsNoTracking();
        if (status is not null)
        {
            query = query.Where(item => item.Status == status);
        }

        int totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        List<ReviewItemEntity> items = await query
            .OrderByDescending(item => item.CreatedAt)
            .ThenBy(item => item.ReviewItemId)
            .Skip((effectivePage - 1) * effectivePageSize)
            .Take(effectivePageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (items.Count == 0)
        {
            return new ReviewQueueApiResult(effectivePage, effectivePageSize, totalCount, []);
        }

        // A review item points at a task, a result or a callback. Resolve each back
        // to its call job so the console can link into the detail screen.
        Dictionary<string, JobContext> contexts = await ResolveJobContextsAsync(
            context,
            items,
            cancellationToken).ConfigureAwait(false);

        return new ReviewQueueApiResult(
            effectivePage,
            effectivePageSize,
            totalCount,
            items.Select(item =>
            {
                JobContext? jobContext = contexts.GetValueOrDefault(item.SourceId);
                return new ReviewQueueItemView(
                    item.ReviewItemId,
                    item.SourceType,
                    item.SourceId,
                    item.Reason,
                    item.Status,
                    item.Resolution,
                    item.CorrelationId,
                    jobContext?.IvrCallJobId,
                    jobContext?.OrderCodeShort,
                    jobContext?.ResultType,
                    item.CreatedAt,
                    item.ResolvedAt);
            }).ToArray());
    }

    private static async Task<Dictionary<string, JobContext>> ResolveJobContextsAsync(
        IvrDbContext context,
        List<ReviewItemEntity> items,
        CancellationToken cancellationToken)
    {
        string[] sourceIds = items.Select(item => item.SourceId).Distinct().ToArray();

        var byResult = await context.CallResults.AsNoTracking()
            .Where(result => sourceIds.Contains(result.IvrCallResultId))
            .Select(result => new
            {
                SourceId = result.IvrCallResultId,
                result.IvrCallJobId,
                result.TaskId,
                ResultType = (string?)result.ResultType,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var byCallback = await context.ResultCallbacks.AsNoTracking()
            .Where(callback => sourceIds.Contains(callback.CallbackId))
            .Join(
                context.CallResults.AsNoTracking(),
                callback => callback.IvrCallResultId,
                result => result.IvrCallResultId,
                (callback, result) => new
                {
                    SourceId = callback.CallbackId,
                    result.IvrCallJobId,
                    result.TaskId,
                    ResultType = (string?)result.ResultType,
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var byTask = await context.CallJobs.AsNoTracking()
            .Where(job => sourceIds.Contains(job.TaskId))
            .Select(job => new
            {
                SourceId = job.TaskId,
                job.IvrCallJobId,
                job.TaskId,
                ResultType = (string?)null,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var resolved = byResult.Concat(byCallback).Concat(byTask).ToList();
        string[] taskIds = resolved.Select(entry => entry.TaskId).Distinct().ToArray();
        Dictionary<string, string> summaries = await context.ConfirmationTasks.AsNoTracking()
            .Where(task => taskIds.Contains(task.TaskId))
            .Select(task => new { task.TaskId, task.PrivacySafeOrderSummaryJson })
            .ToDictionaryAsync(
                entry => entry.TaskId,
                entry => entry.PrivacySafeOrderSummaryJson,
                cancellationToken)
            .ConfigureAwait(false);

        Dictionary<string, JobContext> map = [];
        foreach (var entry in resolved)
        {
            map.TryAdd(
                entry.SourceId,
                new JobContext(
                    entry.IvrCallJobId,
                    ReadOrderCodeShort(summaries.GetValueOrDefault(entry.TaskId)),
                    entry.ResultType));
        }

        return map;
    }

    private static List<DependencyStatusView> BuildDependencies(
        FeatureFlagSnapshot flags,
        int totalChannels,
        int enabledChannels,
        DateTimeOffset? lastHealthCheck,
        CallbackDeliveryOptions? callback,
        CallbackCircuitState? circuit,
        DateTimeOffset observedAt) =>
    [
        // Observed: IVR owns this state and can report it truthfully.
        new DependencyStatusView(
            "SIM_GATEWAY",
            totalChannels == 0
                ? "DOWN"
                : enabledChannels == 0 ? "DOWN" : "UP",
            $"provider={flags.SimProvider}; channels {enabledChannels}/{totalChannels} enabled",
            $"Nhà cung cấp={flags.SimProvider}; {enabledChannels}/{totalChannels} kênh đang bật",
            "SIM down maps to IVR_TECHNICAL_EXCEPTION, never to no-answer (DT-02).",
            Observed: true,
            lastHealthCheck),
        new DependencyStatusView(
            "DIAL_KILL_SWITCH",
            flags.GlobalDialKillSwitch ? "DOWN" : "UP",
            flags.GlobalDialKillSwitch
                ? "kill switch engaged; dispatch blocked"
                : "kill switch released",
            null,
            "While engaged no call is dispatched in any mode.",
            Observed: true,
            null),

        // W-0029 / P4-1 §3.5. Observed, but only for what IVR genuinely knows: the selected
        // provider profile and the live state of its own outbound circuit. It is NOT an external
        // probe of Sales — that needs a real endpoint and belongs to W-0040. Reporting the
        // circuit as if it were Sales' health would be the same lie in a nicer shape.
        new DependencyStatusView(
            "ORDER_CORE",
            callback is not { Enabled: true } || circuit is null
                ? "NOT_WIRED"
                : circuit.Readiness == "READY" ? "UP" : "READY_503",
            string.Concat(
                "provider=",
                callback?.Provider ?? "not-configured",
                "; delivery=",
                callback is { Enabled: true } ? "enabled" : "disabled",
                "; circuit=",
                circuit?.Readiness ?? "not-running",
                "; consecutive_transient_failures=",
                (circuit?.ConsecutiveTransientFailures ?? 0).ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
                "; real endpoint still BLOCKED_EXTERNAL (G-CONTRACT / W-0002..W-0006)"),
            string.Concat(
                "Nhà cung cấp=",
                callback?.Provider ?? "chưa cấu hình",
                "; chuyển giao=",
                callback is { Enabled: true } ? "đang bật" : "đang tắt",
                "; circuit=",
                circuit?.Readiness ?? "chưa chạy",
                "; lỗi tạm thời liên tiếp=",
                (circuit?.ConsecutiveTransientFailures ?? 0).ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
                "; endpoint thật vẫn BLOCKED_EXTERNAL (G-CONTRACT / W-0002..W-0006)"),
            "Order Core down means no new task and bounded callback retry or admin review.",
            // Observed only when delivery is actually on. With delivery disabled there is
            // nothing being observed about Order Core, and saying otherwise would be the
            // placeholder problem again in a different place.
            Observed: callback is { Enabled: true } && circuit is not null,
            callback is { Enabled: true } && circuit is not null ? observedAt : null),
        new DependencyStatusView(
            "OPS_SELLABLE_GATE",
            "NOT_WIRED",
            "No ops health probe; /health/ready has no dependency signal until W-0040.",
            null,
            "ready=503 or down means fail-closed: no dispatch and no confirm (DO-06).",
            Observed: false,
            null),
        new DependencyStatusView(
            "CRM_DO_NOT_CALL",
            "NOT_WIRED",
            "Voice restriction and trust evidence arrive inside the Sales task (W-0031); "
            + "IVR holds no CRM client and probes nothing (UT-ARCH-NO-CRM-EGRESS-06).",
            null,
            "CRM down means opt-out cannot be determined, so no dispatch (DC-01).",
            Observed: false,
            null),
        new DependencyStatusView(
            "EVIDENCE_REGISTRY",
            "NOT_WIRED",
            "Evidence is written locally; no external registry probe exists.",
            null,
            "Evidence down means no final callback, so the job holds.",
            Observed: false,
            null),
    ];

    private static List<FailClosedEventView> BuildFailClosedEvents(
        List<CapacityIncidentEntity> incidents,
        List<ReviewItemEntity> reviews) =>
    [
        .. incidents.Select(incident => new FailClosedEventView(
            "CAPACITY_INCIDENT",
            incident.CapacityIncidentId,
            incident.HoldNewCalls
                ? $"{incident.Scope}: new calls held"
                : $"{incident.Scope}: open, dispatch not held",
            incident.HoldNewCalls,
            incident.SessionId,
            incident.OpenedAt)),
        .. reviews.Select(item => new FailClosedEventView(
            "REVIEW_ITEM",
            item.ReviewItemId,
            $"{item.SourceType}: {item.Reason}",
            null,
            item.CorrelationId,
            item.CreatedAt)),
    ];

    private static ScriptVersionView MapVersion(ScriptVersionEntity version)
    {
        string[] approvalTypes = version.Approvals
            .Select(approval => approval.ApprovalType)
            .ToArray();

        // A draft or retired version may hold a template that no longer passes the
        // Target V1 whitelist. That is information the operator needs, so it is
        // reported as a flag instead of failing the request.
        bool templateValid;
        try
        {
            _ = TargetV1SpeechPolicy.ValidateTemplate(version.TemplateText);
            templateValid = true;
        }
        catch (InvalidOperationException)
        {
            templateValid = false;
        }
        catch (ArgumentException)
        {
            templateValid = false;
        }

        return new ScriptVersionView(
            version.TemplateId,
            version.Version,
            version.Status,
            version.TemplateHash,
            ReadStringArray(version.AllowedInputFieldsJson),
            version.Approvals
                .OrderBy(approval => approval.ApprovedAt)
                .Select(approval => new ScriptApprovalView(
                    approval.ApprovalType,
                    approval.ActorId,
                    approval.Reason,
                    approval.CorrelationId,
                    approval.ApprovedAt))
                .ToArray(),
            RequiredApprovalTypes
                .Where(required => !approvalTypes.Contains(required, StringComparer.OrdinalIgnoreCase))
                .ToArray(),
            templateValid,
            templateValid && TargetV1SpeechPolicy.UsesProductionDecisionFields(version.TemplateText),
            version.CreatedBy,
            version.CreatedAt,
            version.SubmittedBy,
            version.SubmittedAt,
            version.RetiredBy,
            version.RetiredAt);
    }

    private static string? ReadOrderCodeShort(string? summaryJson)
    {
        if (string.IsNullOrWhiteSpace(summaryJson))
        {
            return null;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(summaryJson);
            return document.RootElement.TryGetProperty("order_code_short", out JsonElement value)
                && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string[] ReadStringArray(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string NormalizeEnvironment(string environment)
    {
        if (string.IsNullOrWhiteSpace(environment))
        {
            return FeatureFlagEnvironments.Development;
        }

        string trimmed = environment.Trim();
        return FeatureFlagEnvironments.All.Contains(trimmed)
            ? trimmed
            : throw IvrErrors.MalformedRequest("Unknown feature flag environment.");
    }

    private static void RequireSafe(string? value, string field)
    {
        if (value is null)
        {
            return;
        }

        if (value.Length > 128)
        {
            throw IvrErrors.MalformedRequest($"{field} is too long.");
        }

        try
        {
            PiiGuard.EnsureSafeText(value);
        }
        catch (InvalidOperationException)
        {
            throw IvrErrors.PiiPolicyViolation();
        }
    }

    private sealed record JobContext(
        string IvrCallJobId,
        string? OrderCodeShort,
        string? ResultType);
}
