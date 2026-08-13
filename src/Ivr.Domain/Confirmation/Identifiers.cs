namespace Ivr.Domain.Confirmation;

public abstract record DomainStringValue
{
    protected DomainStringValue(string value, int maximumLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Value exceeds {maximumLength} characters.");
        }

        Value = normalized;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record TaskId : DomainStringValue
{
    private TaskId(string value) : base(value, 120) { }

    public static TaskId Create(string value) => new(value);
}

public sealed record OrderId : DomainStringValue
{
    private OrderId(string value) : base(value, 120) { }

    public static OrderId Create(string value) => new(value);
}

public sealed record CallbackId : DomainStringValue
{
    private CallbackId(string value) : base(value, 120) { }

    public static CallbackId Create(string value) => new(value);
}

public sealed record AttemptId : DomainStringValue
{
    private AttemptId(string value) : base(value, 120) { }

    public static AttemptId Create(string value) => new(value);
}

public sealed record CallJobId : DomainStringValue
{
    private CallJobId(string value) : base(value, 120) { }

    public static CallJobId Create(string value) => new(value);
}

public sealed record CorrelationId : DomainStringValue
{
    private CorrelationId(string value) : base(value, 120) { }

    public static CorrelationId Create(string value) => new(value);
}

public sealed record OrderVersion : DomainStringValue
{
    private OrderVersion(string value) : base(value, 120) { }

    public static OrderVersion Create(string value) => new(value);
}

public sealed record PolicyVersion : DomainStringValue
{
    private PolicyVersion(string value) : base(value, 120) { }

    public static PolicyVersion Create(string value) => new(value);
}

public sealed record EvidenceReference : DomainStringValue
{
    private EvidenceReference(string value) : base(value, 500) { }

    public static EvidenceReference Create(string value) => new(value);
}

public sealed record AuditReference : DomainStringValue
{
    private AuditReference(string value) : base(value, 500) { }

    public static AuditReference Create(string value) => new(value);
}

public sealed record DialTokenReference
{
    private DialTokenReference(string value, DateTimeOffset expiresAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        OpaqueReferenceGuard.EnsureNotRawPhone(value);
        if (value.Length > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Dial-token reference exceeds 500 characters.");
        }

        Value = value;
        ExpiresAt = expiresAt;
    }

    internal string Value { get; }

    public DateTimeOffset ExpiresAt { get; }

    public static DialTokenReference Create(string opaqueValue, DateTimeOffset expiresAt) =>
        new(opaqueValue, expiresAt);

    public string RevealToTrustedResolver() => Value;

    public override string ToString() => "[REDACTED_DIAL_TOKEN]";
}

internal static class OpaqueReferenceGuard
{
    internal static void EnsureNotRawPhone(string value)
    {
        string compact = string.Concat(value.Where(character =>
            char.IsDigit(character) || character == '+'));
        bool containsOnlyPhoneCharacters = value.All(character =>
            char.IsDigit(character)
            || character is '+' or '-' or '(' or ')' or ' '
            || char.IsWhiteSpace(character));
        string digits = string.Concat(compact.Where(char.IsDigit));
        if (containsOnlyPhoneCharacters
            && digits.Length is >= 10 and <= 12
            && (digits.StartsWith('0') || digits.StartsWith("84", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("Raw phone data cannot be used as an opaque reference.");
        }
    }
}
