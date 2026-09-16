using System.Text.Json.Serialization;

namespace Ivr.Api.Admin;

/// <summary>
/// One row of the append-only audit log, as the admin surface publishes it (W-0307).
/// </summary>
/// <remarks>
/// <para>
/// Every property names its wire field explicitly, the way the other admin contracts in this
/// folder do. The first version of this record left them off and relied on the serializer's
/// default, which produces camelCase — so the runtime published <c>accessAuditId</c> while the
/// OpenAPI document declared <c>access_audit_id</c>. The integration tests did not catch it
/// because they deserialize into this same record and a round trip agrees with itself no matter
/// what the names are. <c>IT-API-MATRIX-38</c> caught it, because it reads the response by the
/// field names the contract publishes.
/// </para>
/// <para>
/// The three JSON columns are carried through as strings rather than parsed into a shape, because
/// every writer puts a different shape in them and inventing a union here would be a second source
/// of truth for something the table already holds. They are safe to publish for a reason that is
/// enforced rather than assumed: every live writer runs <c>PiiGuard</c> over the payload before the
/// row is inserted, and <c>PiiMaskingFilter</c> refuses the whole response if one ever gets past
/// that. Two independent gates, neither of which is this record.
/// </para>
/// </remarks>
public sealed record AuditEvidenceRowView(
    [property: JsonPropertyName("audit_id")] string AuditId,
    [property: JsonPropertyName("actor_id")] string ActorId,
    [property: JsonPropertyName("actor_type")] string ActorType,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("target_type")] string TargetType,
    [property: JsonPropertyName("target_id")] string TargetId,
    [property: JsonPropertyName("reason")] string? Reason,
    [property: JsonPropertyName("correlation_id")] string CorrelationId,
    [property: JsonPropertyName("data_json")] string DataJson,
    [property: JsonPropertyName("before_state_json")] string? BeforeStateJson,
    [property: JsonPropertyName("after_state_json")] string? AfterStateJson,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);

/// <summary>
/// The answer to "show me everything that happened to this object, and prove nobody edited it".
/// </summary>
/// <param name="AccessAuditId">
/// The audit row this read itself wrote. Reading an audit trail is the access most worth recording
/// -- it is what someone covering their tracks would do -- so the response hands back the id of its
/// own entry rather than leaving the caller to go looking for it.
/// </param>
/// <param name="Truncated">
/// True when the store held more rows than <c>limit</c>. Stated as a field rather than left to the
/// caller to infer from <c>rows.Count == limit</c>, because that inference is wrong exactly when
/// the count lands on the limit by coincidence, and an auditor who thinks they have the whole trail
/// when they do not is worse off than one who knows they do not.
/// </param>
public sealed record AuditEvidenceApiResult(
    [property: JsonPropertyName("target_type")] string TargetType,
    [property: JsonPropertyName("target_id")] string TargetId,
    [property: JsonPropertyName("actor_id")] string ActorId,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("correlation_id")] string CorrelationId,
    [property: JsonPropertyName("access_audit_id")] string AccessAuditId,
    [property: JsonPropertyName("limit")] int Limit,
    [property: JsonPropertyName("truncated")] bool Truncated,
    [property: JsonPropertyName("rows")] IReadOnlyList<AuditEvidenceRowView> Rows);
