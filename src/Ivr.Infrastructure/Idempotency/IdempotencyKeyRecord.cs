namespace Ivr.Infrastructure.Idempotency;

public sealed record IdempotencyKeyRecord(
    string Key,
    string PayloadHash,
    string ResponseSnapshot,
    DateTimeOffset CreatedAt);
