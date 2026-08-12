using Ivr.Domain.Errors;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Correlation;
using Ivr.Infrastructure.Evidence;
using Ivr.Infrastructure.Idempotency;

namespace Ivr.UnitTests;

public sealed class CrossCuttingFoundationTests
{
    [Fact]
    [Trait("TestId", "UT-FND-IDEMP-01")]
    public async Task IdempotencyStoreReplaysSamePayloadAndRejectsDifferentPayload()
    {
        InMemoryIdempotencyStore store = new(TimeProvider.System);
        int executions = 0;

        SampleResponse first = await store.ExecuteAsync(
            "order-1",
            "payload-a",
            _ => Task.FromResult(new SampleResponse(++executions, "accepted")));
        SampleResponse replay = await store.ExecuteAsync(
            "order-1",
            "payload-a",
            _ => Task.FromResult(new SampleResponse(++executions, "unexpected")));

        Assert.Equal(first, replay);
        Assert.Equal(1, executions);

        IvrFailureException conflict = await Assert.ThrowsAsync<IvrFailureException>(
            () => store.ExecuteAsync(
                "order-1",
                "payload-b",
                _ => Task.FromResult(new SampleResponse(++executions, "unexpected"))));
        Assert.Equal(IvrErrorCodes.IdempotencyConflict, conflict.ErrorCode);
        Assert.Equal(1, executions);

        int piiExecutions = 0;
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.ExecuteAsync(
                "order-2",
                "payload-safe",
                _ => Task.FromResult(
                    new SampleResponse(++piiExecutions, "contact 0912341234"))));
        SampleResponse afterRejectedSnapshot = await store.ExecuteAsync(
            "order-2",
            "payload-safe",
            _ => Task.FromResult(new SampleResponse(++piiExecutions, "accepted")));
        Assert.Equal(2, piiExecutions);
        Assert.Equal("accepted", afterRejectedSnapshot.Status);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.ExecuteAsync(
                "0912341234",
                "payload-safe",
                _ => Task.FromResult(new SampleResponse(1, "unexpected"))));
    }

    [Fact]
    [Trait("TestId", "UT-FND-AUDIT-06")]
    public async Task AuditLoggerIsAppendOnlyAndRejectsUnsafeAdministrativeEvents()
    {
        InMemoryAuditLogger logger = new(TimeProvider.System);
        AuditLogEntry entry = await logger.AppendAsync(
            new AuditEvent(
                "operator-1",
                "ADMIN_QUEUE_PAUSE",
                "queue-main",
                "approved maintenance",
                "corr-audit-1",
                new Dictionary<string, object?> { ["result"] = "paused" }));

        Assert.Equal("approved maintenance", entry.Reason);
        Assert.Equal("corr-audit-1", entry.CorrelationId);
        Assert.Single(logger.Entries);
        Assert.DoesNotContain(
            typeof(IAuditLogger).GetMethods(),
            method => method.Name.StartsWith("Update", StringComparison.Ordinal)
                || method.Name.StartsWith("Delete", StringComparison.Ordinal));

        IvrFailureException missingReason = await Assert.ThrowsAsync<IvrFailureException>(
            () => logger.AppendAsync(
                new AuditEvent(
                    "operator-1",
                    "ADMIN_QUEUE_RESUME",
                    "queue-main",
                    null,
                    "corr-audit-2",
                    new Dictionary<string, object?>())));
        Assert.Equal(IvrErrorCodes.MalformedRequest, missingReason.ErrorCode);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => logger.AppendAsync(
                new AuditEvent(
                    "operator-1",
                    "JOB_CREATED",
                    "job-1",
                    null,
                    "corr-audit-3",
                    new Dictionary<string, object?> { ["note"] = "contact 0912341234" })));
        Assert.Single(logger.Entries);
    }

    [Fact]
    [Trait("TestId", "UT-FND-EVID-10")]
    public async Task EvidenceRegistryIsAppendOnlyUniqueAndPiiSafe()
    {
        InMemoryEvidenceStore store = new(TimeProvider.System);
        EvidenceRecord record = await store.AppendAsync(
            new EvidenceWrite(
                "evidence-1",
                "CALL_SIGNAL",
                "corr-evidence-1",
                "object-store://safe-reference"));

        Assert.Equal("evidence-1", record.EvidenceRef);
        Assert.Single(store.Records);

        IvrFailureException duplicate = await Assert.ThrowsAsync<IvrFailureException>(
            () => store.AppendAsync(
                new EvidenceWrite(
                    "evidence-1",
                    "CALL_SIGNAL",
                    "corr-evidence-1",
                    "object-store://second-reference")));
        Assert.Equal(IvrErrorCodes.IdempotencyConflict, duplicate.ErrorCode);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.AppendAsync(
                new EvidenceWrite(
                    "evidence-2",
                    "CALL_SIGNAL",
                    "corr-evidence-2",
                    "contact 0912341234")));
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.AppendAsync(
                new EvidenceWrite(
                    "evidence-3",
                    "CALL_SIGNAL",
                    "0912341234",
                    "object-store://safe-reference")));
        Assert.Single(store.Records);
    }

    [Fact]
    [Trait("TestId", "UT-FND-PII-07")]
    public void PiiMaskerUsesTheApprovedFormatAndGuardRejectsRawPii()
    {
        Assert.Equal("09****1234", PiiMasker.Mask("0912341234"));
        Assert.Throws<InvalidOperationException>(() => PiiGuard.EnsureSafeField("raw_phone"));
        Assert.Throws<InvalidOperationException>(
            () => PiiGuard.EnsureSafeText("contact 0912341234"));
        Assert.Throws<InvalidOperationException>(
            () => PiiGuard.EnsureSafeText("ĐưỜnG confidential"));
        Assert.Throws<InvalidOperationException>(
            () => PiiGuard.EnsureSafeText("tổ 5"));
        Assert.True(PiiGuard.IsSafeText("send to queue"));
        CorrelationContext correlationContext = new();
        Assert.Throws<InvalidOperationException>(
            () => correlationContext.Push("0912341234"));
    }

    [Fact]
    [Trait("TestId", "UT-FND-ERRCAT-11")]
    public void ErrorDetailsAreValidatedAndSnapshottedBeforeExposure()
    {
        Dictionary<string, string> mutableDetails = new(StringComparer.Ordinal)
        {
            ["field"] = "safe-value",
        };
        IvrFailureException failure = new(
            IvrErrorCodes.MalformedRequest,
            "Safe failure.",
            mutableDetails);

        mutableDetails["raw_phone"] = "0912341234";

        Assert.Single(failure.Details);
        Assert.Equal("safe-value", failure.Details["field"]);
        Assert.False(failure.Details.ContainsKey("raw_phone"));
        Assert.Throws<NotSupportedException>(
            () => ((IDictionary<string, string>)failure.Details).Add(
                "another",
                "value"));
    }

    [Fact]
    [Trait("TestId", "UT-FND-CONFIG-09")]
    public void ConfigurationValidatorFailsClosedForUnsafeOrIncompleteSettings()
    {
        IvrOptionsValidator validator = new();
        Microsoft.Extensions.Options.ValidateOptionsResult result = validator.Validate(
            null,
            new IvrOptions
            {
                ExecutionMode = "PRODUCTION_REAL",
                SalesProvider = string.Empty,
                SimProvider = string.Empty,
                ConnectionString = string.Empty,
                RealCustomerCallAllowed = true,
            });

        Assert.False(result.Succeeded);
        IEnumerable<string> failures = Assert.IsAssignableFrom<IEnumerable<string>>(
            result.Failures);
        Assert.Contains(failures, failure => failure.Contains(
            "ConnectionStrings__IvrDb",
            StringComparison.Ordinal));
        Assert.Contains(failures, failure => failure.Contains(
            "REAL_CUSTOMER_CALL_ALLOWED",
            StringComparison.Ordinal));
        Assert.Contains(failures, failure => failure.Contains(
            "SALES_PROVIDER",
            StringComparison.Ordinal));
        Assert.Contains(failures, failure => failure.Contains(
            "SIM_PROVIDER",
            StringComparison.Ordinal));
    }

    private sealed record SampleResponse(int Sequence, string Status);
}
