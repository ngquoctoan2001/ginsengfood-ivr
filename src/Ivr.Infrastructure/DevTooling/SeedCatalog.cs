using System.Text.Json;
using System.Text.Json.Nodes;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Domain.DevTooling;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.DevTooling;

/// <summary>
/// The task fixtures, and whether their confirmation windows were moved to make them loadable.
/// </summary>
/// <param name="RebasedCount">How many fixtures had their window shifted.</param>
public sealed record SeedTaskCatalog(
    IReadOnlyList<SeedTaskFixture> Tasks,
    bool WindowsRebased,
    int RebasedCount);

/// <summary>One task fixture: the intake headers it ships with, and the task body itself.</summary>
public sealed record SeedTaskFixture(
    string Scenario,
    string IdempotencyKey,
    string CorrelationId,
    IvrConfirmationTaskV1 Body);

/// <summary>A dependency-health rehearsal profile from <c>seed/integration-status.sample.json</c>.</summary>
public sealed record IntegrationStatusProfile(
    string Id,
    string OrderCore,
    string SimGateway,
    string CrmDoNotCall,
    string EvidenceRegistry,
    string Expected);

public sealed class SeedCatalogException(string message) : InvalidOperationException(message);

/// <summary>
/// Reads the non-production fixtures under <c>seed/</c> (UI-07, W-0112).
/// <para>
/// File names are constants and the directory comes from configuration, so no caller-supplied
/// string ever reaches the filesystem. The surface is non-production only, but "only reachable in
/// staging" is not a reason to accept a path a request can steer.
/// </para>
/// </summary>
public sealed class SeedCatalog(IOptions<DevToolingOptions> options)
{
    public const string TaskFileName = "sales-target-v1.sample.json";
    public const string ScenarioFileName = "call-scenarios.sample.json";
    public const string IntegrationStatusFileName = "integration-status.sample.json";

    private static readonly JsonSerializerOptions TaskJson = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false,
    };

    private readonly DevToolingOptions settings = options?.Value
        ?? throw new ArgumentNullException(nameof(options));

    /// <summary>Whether a seed directory is configured and present.</summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(settings.SeedDirectory)
        && Directory.Exists(settings.SeedDirectory);

    public string SeedDirectory => settings.SeedDirectory;

    /// <summary>
    /// Timestamps a rebase moves. All four belong to one task's confirmation window, so they are
    /// shifted together by that task's own offset and the gaps between them survive untouched.
    /// </summary>
    public static readonly string[] WindowFields =
    [
        "created_at",
        "confirmation_window_started_at",
        "confirmation_window_expires_at",
        "dial_token_expires_at",
    ];

    /// <summary>
    /// Reads the task fixtures, optionally moving each confirmation window so that it starts at
    /// <paramref name="rebaseTo"/>.
    /// <para>
    /// The fixtures carry absolute instants in August 2026 with windows five to fifteen minutes
    /// long, so loaded as written every one of them is refused with
    /// <c>ORDER_NOT_CALLABLE_OR_WINDOW_EXPIRED</c>: the loader would return nine rejections and no
    /// usable rehearsal. Rebasing is what makes the fixture set loadable at all.
    /// </para>
    /// <para>
    /// Each task is shifted by its own offset rather than all of them by one. A single offset
    /// would preserve the file's two-hour stagger, which leaves exactly one task callable at a
    /// time and a rehearsal waiting two hours for the rest. That stagger is how the file
    /// describes a timeline to replay; it is not how a demo environment needs to look. What is
    /// lost by flattening it is reported rather than left for someone to discover.
    /// </para>
    /// </summary>
    public async Task<SeedTaskCatalog> ReadTasksAsync(
        DateTimeOffset? rebaseTo,
        CancellationToken cancellationToken)
    {
        JsonNode root = await ReadNodeAsync(TaskFileName, cancellationToken).ConfigureAwait(false);
        if (root["tasks"] is not JsonArray tasks)
        {
            throw new SeedCatalogException($"{TaskFileName} has no 'tasks' array.");
        }

        List<SeedTaskFixture> fixtures = [];
        int rebased = 0;
        foreach (JsonNode? entry in tasks)
        {
            if (entry?["body"] is not JsonObject body)
            {
                throw new SeedCatalogException($"{TaskFileName} has a task with no body.");
            }

            if (rebaseTo is not null && Rebase(body, rebaseTo.Value))
            {
                rebased++;
            }

            IvrConfirmationTaskV1 task = body.Deserialize<IvrConfirmationTaskV1>(TaskJson)
                ?? throw new SeedCatalogException($"{TaskFileName} has an unreadable task body.");
            string taskId = task.Task_id
                ?? throw new SeedCatalogException($"{TaskFileName} has a task with no task_id.");
            fixtures.Add(new SeedTaskFixture(
                ReadString(entry, "scenario") ?? taskId,
                ReadString(entry?["headers"], "Idempotency-Key")
                    ?? throw new SeedCatalogException(
                        $"{TaskFileName} task '{taskId}' has no Idempotency-Key header."),
                ReadString(entry?["headers"], "X-Correlation-Id")
                    ?? task.Correlation_id
                    ?? throw new SeedCatalogException(
                        $"{TaskFileName} task '{taskId}' has no correlation id."),
                task));
        }

        if (fixtures.Count > settings.MaximumSeedTasks)
        {
            throw new SeedCatalogException(
                $"{TaskFileName} declares {fixtures.Count} tasks; the configured maximum is "
                + $"{settings.MaximumSeedTasks}.");
        }

        return new SeedTaskCatalog(fixtures, rebaseTo is not null, rebased);
    }

    private static bool Rebase(JsonObject body, DateTimeOffset rebaseTo)
    {
        if (body["confirmation_window_started_at"] is not JsonValue anchor
            || !anchor.TryGetValue(out DateTimeOffset started))
        {
            return false;
        }

        TimeSpan offset = rebaseTo - started;
        foreach (string field in WindowFields)
        {
            if (body[field] is JsonValue value
                && value.TryGetValue(out DateTimeOffset instant))
            {
                body[field] = JsonValue.Create(instant + offset);
            }
        }

        return true;
    }

    public async Task<IReadOnlyList<ScenarioDefinition>> ReadScenariosAsync(
        CancellationToken cancellationToken)
    {
        using JsonDocument document = await ReadAsync(ScenarioFileName, cancellationToken)
            .ConfigureAwait(false);
        if (!document.RootElement.TryGetProperty("scenarios", out JsonElement scenarios)
            || scenarios.ValueKind != JsonValueKind.Array)
        {
            throw new SeedCatalogException($"{ScenarioFileName} has no 'scenarios' array.");
        }

        List<ScenarioDefinition> definitions = [];
        foreach (JsonElement entry in scenarios.EnumerateArray())
        {
            string id = ReadString(entry, "id")
                ?? throw new SeedCatalogException($"{ScenarioFileName} has a scenario with no id.");
            List<ScenarioAttempt> attempts = [];
            if (entry.TryGetProperty("attempts", out JsonElement declared)
                && declared.ValueKind == JsonValueKind.Array)
            {
                int ordinal = 0;
                foreach (JsonElement attempt in declared.EnumerateArray())
                {
                    ordinal++;
                    attempts.Add(new ScenarioAttempt(
                        attempt.TryGetProperty("attempt_number", out JsonElement number)
                            && number.ValueKind == JsonValueKind.Number
                                ? number.GetInt32()
                                : ordinal,
                        ReadString(attempt, "raw_call_status") ?? string.Empty,
                        ReadString(attempt, "raw_dtmf")));
                }
            }

            definitions.Add(new ScenarioDefinition(
                id,
                ReadString(entry, "task_ref"),
                attempts,
                ReadString(entry, "expected_result_type"),
                entry.TryGetProperty("expected_counted", out JsonElement counted)
                    && counted.ValueKind is JsonValueKind.True or JsonValueKind.False
                        ? counted.GetBoolean()
                        : null));
        }

        return definitions;
    }

    public async Task<IReadOnlyList<IntegrationStatusProfile>> ReadIntegrationProfilesAsync(
        CancellationToken cancellationToken)
    {
        using JsonDocument document = await ReadAsync(IntegrationStatusFileName, cancellationToken)
            .ConfigureAwait(false);
        if (!document.RootElement.TryGetProperty("profiles", out JsonElement profiles)
            || profiles.ValueKind != JsonValueKind.Array)
        {
            throw new SeedCatalogException($"{IntegrationStatusFileName} has no 'profiles' array.");
        }

        List<IntegrationStatusProfile> parsed = [];
        foreach (JsonElement entry in profiles.EnumerateArray())
        {
            parsed.Add(new IntegrationStatusProfile(
                ReadString(entry, "id")
                    ?? throw new SeedCatalogException(
                        $"{IntegrationStatusFileName} has a profile with no id."),
                ReadString(entry, "order_core") ?? "unknown",
                ReadString(entry, "sim_gateway") ?? "unknown",
                ReadString(entry, "crm_do_not_call") ?? "unknown",
                ReadString(entry, "evidence_registry") ?? "unknown",
                ReadString(entry, "expected") ?? string.Empty));
        }

        return parsed;
    }

    private async Task<JsonNode> ReadNodeAsync(string fileName, CancellationToken cancellationToken)
    {
        byte[] bytes = await ReadBytesAsync(fileName, cancellationToken).ConfigureAwait(false);
        try
        {
            return JsonNode.Parse(
                bytes,
                documentOptions: new JsonDocumentOptions
                {
                    MaxDepth = 64,
                    AllowTrailingCommas = false,
                })
                ?? throw new SeedCatalogException($"{fileName} is empty.");
        }
        catch (JsonException exception)
        {
            throw new SeedCatalogException($"{fileName} is not valid JSON: {exception.Message}");
        }
    }

    private static string? ReadString(JsonNode? node, string property) =>
        node is JsonObject holder
            && holder[property] is JsonValue value
            && value.TryGetValue(out string? text)
                ? text
                : null;

    private async Task<JsonDocument> ReadAsync(string fileName, CancellationToken cancellationToken)
    {
        byte[] bytes = await ReadBytesAsync(fileName, cancellationToken).ConfigureAwait(false);
        try
        {
            return JsonDocument.Parse(
                bytes,
                new JsonDocumentOptions { MaxDepth = 64, AllowTrailingCommas = false });
        }
        catch (JsonException exception)
        {
            throw new SeedCatalogException($"{fileName} is not valid JSON: {exception.Message}");
        }
    }

    private async Task<byte[]> ReadBytesAsync(string fileName, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            throw new SeedCatalogException(
                "No seed directory is configured. Set Ivr:DevTooling:SeedDirectory to the "
                + "repository's seed/ folder.");
        }

        string path = Path.Combine(settings.SeedDirectory, fileName);
        if (!File.Exists(path))
        {
            throw new SeedCatalogException($"The seed directory has no {fileName}.");
        }

        return await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
    }

    private static string? ReadString(JsonElement element, string property) =>
        element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(property, out JsonElement value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
}
