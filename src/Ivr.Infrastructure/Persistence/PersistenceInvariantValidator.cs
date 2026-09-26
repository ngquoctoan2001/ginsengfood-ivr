using System.Text.Json;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ivr.Infrastructure.Persistence;

internal static class PersistenceInvariantValidator
{
    private static readonly HashSet<string> ForbiddenSpeechProperties = new(
        StringComparer.OrdinalIgnoreCase)
        {
            "address",
            "full_address",
            "health_note",
            "payment_detail",
            "phone",
            "phone_number",
            "raw_phone",
            "recording",
        };

    public static void Validate(ChangeTracker changeTracker)
    {
        ArgumentNullException.ThrowIfNull(changeTracker);

        foreach (EntityEntry<ConfirmationTaskEntity> entry in changeTracker
                     .Entries<ConfirmationTaskEntity>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            ValidateTask(entry.Entity);
            if (entry.State == EntityState.Modified
                && (entry.Property(entity => entity.ContractVersion).IsModified
                    || entry.Property(entity => entity.OfficialOrderId).IsModified
                    || entry.Property(entity => entity.OrderVersion).IsModified
                    || entry.Property(entity => entity.OrderState).IsModified
                    || entry.Property(entity => entity.PaymentMethodSnapshot).IsModified
                    || entry.Property(entity => entity.ProgramType).IsModified
                    || entry.Property(entity => entity.AttemptPolicyVersion).IsModified
                    || entry.Property(entity => entity.MaxAttempts).IsModified
                    || entry.Property(entity => entity.AttemptOffsetsSecondsJson).IsModified
                    || entry.Property(entity => entity.ConfirmationWindowStartedAt).IsModified
                    || entry.Property(entity => entity.ConfirmationWindowExpiresAt).IsModified
                    || entry.Property(entity => entity.PhoneRef).IsModified
                    || entry.Property(entity => entity.DialTokenCiphertext).IsModified
                    || entry.Property(entity => entity.DialTokenExpiresAt).IsModified
                    || entry.Property(entity => entity.PrivacySafeOrderSummaryJson).IsModified))
            {
                throw new InvalidOperationException(
                    "A persisted confirmation-task contract/policy/speech snapshot is immutable.");
            }
        }

        foreach (EntityEntry<AttemptPolicyEntity> entry in changeTracker
                     .Entries<AttemptPolicyEntity>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            ValidatePolicy(
                entry.Entity.MaxAttempts,
                entry.Entity.AttemptOffsetsSecondsJson,
                entry.Entity.ConfirmationWindowSeconds);
            if (entry.State == EntityState.Modified)
            {
                throw new InvalidOperationException(
                    "An attempt-policy version is immutable; create a new version instead.");
            }
        }

        foreach (EntityEntry<CallJobEntity> entry in changeTracker
                     .Entries<CallJobEntity>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            ValidatePolicy(
                entry.Entity.MaxAttempts,
                entry.Entity.AttemptOffsetsSecondsJson,
                entry.Entity.ConfirmationWindowSeconds);
            if (entry.State == EntityState.Modified
                && (entry.Property(entity => entity.AttemptPolicyCode).IsModified
                    || entry.Property(entity => entity.MaxAttempts).IsModified
                    || entry.Property(entity => entity.AttemptOffsetsSecondsJson).IsModified
                    || entry.Property(entity => entity.ConfirmationWindowSeconds).IsModified
                    || entry.Property(entity => entity.AttemptScheduleJson).IsModified))
            {
                throw new InvalidOperationException("A persisted call-job policy snapshot is immutable.");
            }
        }

        foreach (EntityEntry<ResultCallbackEntity> entry in changeTracker
                     .Entries<ResultCallbackEntity>()
                     .Where(entry => entry.State == EntityState.Modified))
        {
            if (entry.Property(entity => entity.IdempotencyKey).IsModified
                || entry.Property(entity => entity.IvrCallResultId).IsModified
                || entry.Property(entity => entity.TaskId).IsModified
                || entry.Property(entity => entity.OfficialOrderId).IsModified
                || entry.Property(entity => entity.ResultStatus).IsModified
                || entry.Property(entity => entity.ResultState).IsModified
                || entry.Property(entity => entity.RequiresCoreRevalidation).IsModified
                || entry.Property(entity => entity.PayloadJson).IsModified
                || entry.Property(entity => entity.PayloadSha256).IsModified)
            {
                throw new InvalidOperationException("A callback outbox payload is immutable.");
            }
        }

        foreach (EntityEntry<AuditLogEntity> entry in changeTracker
                     .Entries<AuditLogEntity>()
                     .Where(entry => entry.State == EntityState.Added))
        {
            ValidateAudit(entry.Entity);
        }
    }

    /// <summary>
    /// W-0362 / K-46. The checks <c>PostgresAuditLogger</c> runs on an audit event, run here on
    /// the row, along with the columns that logger never fills: the dispatch and scheduler stores,
    /// among others, add their audit rows directly and so never passed through it or its PII
    /// guard. The target is re-joined into the entity ref the logger checks, so a type and id that
    /// only read as restricted together are refused too. Field names are read back from the stored
    /// JSON, top level only, which is where the logger checks its data keys.
    /// <para>
    /// W-0365 / K-55. The three JSON columns are checked as decoded values too
    /// (<see cref="PiiGuard.EnsureSafeJsonText"/>). They are jsonb, so Postgres stores what the
    /// escapes spell: a +84 number written as <c>\u002B84…</c> or an accented address marker
    /// passed a text-only check and landed in the table in clear.
    /// </para>
    /// </summary>
    private static void ValidateAudit(AuditLogEntity audit)
    {
        PiiGuard.EnsureSafeText(audit.ActorId);
        PiiGuard.EnsureSafeText(audit.ActorType);
        PiiGuard.EnsureSafeText(audit.Action);
        PiiGuard.EnsureSafeText(string.Concat(audit.TargetType, ":", audit.TargetId));
        PiiGuard.EnsureSafeText(audit.Reason);
        PiiGuard.EnsureSafeText(audit.CorrelationId);
        PiiGuard.EnsureSafeJsonText(audit.BeforeStateJson);
        PiiGuard.EnsureSafeJsonText(audit.AfterStateJson);
        PiiGuard.EnsureSafeJsonText(audit.DataJson);
        using JsonDocument data = JsonDocument.Parse(audit.DataJson);
        if (data.RootElement.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty field in data.RootElement.EnumerateObject())
            {
                PiiGuard.EnsureSafeField(field.Name);
            }
        }
    }

    private static void ValidateTask(ConfirmationTaskEntity task)
    {
        ArgumentNullException.ThrowIfNull(task);
        int windowSeconds = checked((int)(task.ConfirmationWindowExpiresAt
            - task.ConfirmationWindowStartedAt).TotalSeconds);
        ValidatePolicy(task.MaxAttempts, task.AttemptOffsetsSecondsJson, windowSeconds);

        if (task.ExpiresAt != task.ConfirmationWindowExpiresAt)
        {
            throw new InvalidOperationException("Task expiry must equal the source confirmation-window expiry.");
        }

        if (task.DialTokenExpiresAt < task.ConfirmationWindowStartedAt
            || task.DialTokenExpiresAt > task.ConfirmationWindowExpiresAt)
        {
            throw new InvalidOperationException("Dial-token expiry must remain inside the confirmation window.");
        }

        if (!task.DialTokenCiphertext.StartsWith("enc:", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Only protected opaque dial-token ciphertext may be persisted.");
        }

        // Q-12 (PA1, 2026-09-26). The full guard used to read this text as stored, and the text is
        // not stored the same way twice: intake writes the serializer's output, which escapes every
        // accented letter, and a task read back from its jsonb column carries the letters
        // themselves. So "Tổ yến" was saved at intake and refused on every later save of the same
        // task, starting with eligibility's, and the order never reached a dial. The text is still
        // read whole, with the product guard, for telephone numbers, dial tokens and the markers
        // that only ever mean an address; the values are read decoded, field by field, each with
        // the guard intake admitted it under.
        PiiGuard.EnsureSafeProductText(task.PrivacySafeOrderSummaryJson);
        using JsonDocument summary = JsonDocument.Parse(task.PrivacySafeOrderSummaryJson);
        RejectForbiddenProperties(summary.RootElement);
        EnsureSafeSummaryValues(summary.RootElement);
    }

    private static void ValidatePolicy(
        int maxAttempts,
        string offsetsJson,
        int windowSeconds)
    {
        if (maxAttempts is < 1 or > 10)
        {
            throw new InvalidOperationException("Attempt count must be between 1 and 10.");
        }

        if (windowSeconds <= 0)
        {
            throw new InvalidOperationException("Confirmation window must be positive.");
        }

        int[] offsets = JsonSerializer.Deserialize<int[]>(offsetsJson)
            ?? throw new InvalidOperationException("Attempt offsets must be a JSON array.");
        if (offsets.Length != maxAttempts || offsets[0] != 0)
        {
            throw new InvalidOperationException(
                "Attempt offsets must start at zero and match the policy attempt count.");
        }

        for (int index = 0; index < offsets.Length; index++)
        {
            if (offsets[index] < 0 || offsets[index] >= windowSeconds)
            {
                throw new InvalidOperationException("Every attempt offset must fall inside the call window.");
            }

            if (index > 0 && offsets[index - 1] >= offsets[index])
            {
                throw new InvalidOperationException("Attempt offsets must be strictly increasing.");
            }
        }
    }

    /// <summary>
    /// Q-12. The stored summary's property names and string values, decoded, each with the guard
    /// intake admitted it under: an item's <c>public_name</c> and <c>unit_label</c> take the
    /// product guard (W-0243), and everything else the full guard.
    /// </summary>
    private static void EnsureSafeSummaryValues(JsonElement summary)
    {
        if (summary.ValueKind != JsonValueKind.Object)
        {
            EnsureSafeJsonValue(summary, productField: false);
            return;
        }

        foreach (JsonProperty property in summary.EnumerateObject())
        {
            PiiGuard.EnsureSafeText(property.Name);
            if (string.Equals(property.Name, "items", StringComparison.Ordinal)
                && property.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in property.Value.EnumerateArray())
                {
                    EnsureSafeSummaryItem(item);
                }
            }
            else
            {
                EnsureSafeJsonValue(property.Value, productField: false);
            }
        }
    }

    private static void EnsureSafeSummaryItem(JsonElement item)
    {
        if (item.ValueKind != JsonValueKind.Object)
        {
            EnsureSafeJsonValue(item, productField: false);
            return;
        }

        foreach (JsonProperty field in item.EnumerateObject())
        {
            PiiGuard.EnsureSafeText(field.Name);
            EnsureSafeJsonValue(
                field.Value,
                productField: field.Name is "public_name" or "unit_label");
        }
    }

    private static void EnsureSafeJsonValue(JsonElement element, bool productField)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                if (productField)
                {
                    PiiGuard.EnsureSafeProductText(element.GetString());
                }
                else
                {
                    PiiGuard.EnsureSafeText(element.GetString());
                }

                break;
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    PiiGuard.EnsureSafeText(property.Name);
                    EnsureSafeJsonValue(property.Value, productField: false);
                }

                break;
            case JsonValueKind.Array:
                foreach (JsonElement value in element.EnumerateArray())
                {
                    EnsureSafeJsonValue(value, productField: false);
                }

                break;
        }
    }

    private static void RejectForbiddenProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (ForbiddenSpeechProperties.Contains(property.Name))
                {
                    throw new InvalidOperationException(
                        "A forbidden PII property was rejected from the speech snapshot.");
                }

                RejectForbiddenProperties(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
            {
                RejectForbiddenProperties(item);
            }
        }
    }
}
