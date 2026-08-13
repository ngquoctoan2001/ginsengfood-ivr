namespace Ivr.Infrastructure.Scripts;

public sealed class ScriptVersionEntity
{
    public Guid Id { get; set; }
    public string TemplateId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string TemplateText { get; set; } = string.Empty;
    public string TemplateHash { get; set; } = string.Empty;
    public string AllowedInputFieldsJson { get; set; } = "[]";
    public string CreatedBy { get; set; } = string.Empty;
    public string CreateReason { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string? SubmittedBy { get; set; }
    public string? SubmitReason { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public string? RetiredBy { get; set; }
    public string? RetireReason { get; set; }
    public DateTimeOffset? RetiredAt { get; set; }
    public List<ScriptApprovalEntity> Approvals { get; } = [];
}

public sealed class ScriptApprovalEntity
{
    public Guid Id { get; set; }
    public Guid ScriptVersionId { get; set; }
    public string ApprovalType { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public DateTimeOffset ApprovedAt { get; set; }
    public ScriptVersionEntity ScriptVersion { get; set; } = null!;
}
