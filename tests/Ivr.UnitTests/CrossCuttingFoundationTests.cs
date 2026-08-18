using System.Text;
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
            () => PiiGuard.EnsureSafeText("contact 090-123-4567"));
        Assert.Throws<InvalidOperationException>(
            () => PiiGuard.EnsureSafeText("contact +84 (90) 123 4567"));
        Assert.Throws<InvalidOperationException>(
            () => PiiGuard.EnsureSafeText("ĐưỜnG confidential"));
        Assert.Throws<InvalidOperationException>(
            () => PiiGuard.EnsureSafeText("tổ 5"));
        Assert.True(PiiGuard.IsSafeText("send to queue"));
        Assert.True(PiiGuard.IsSafeText("dịch vụ cao cấp"));
        CorrelationContext correlationContext = new();
        Assert.Throws<InvalidOperationException>(
            () => correlationContext.Push("0912341234"));

        for (int index = 0; index < 1000; index++)
        {
            string generatedCorrelationId = CorrelationIdGenerator.Create();
            Assert.True(PiiGuard.IsSafeText(generatedCorrelationId));
            Assert.StartsWith("corr-", generatedCorrelationId, StringComparison.Ordinal);
            Assert.All(
                generatedCorrelationId[5..].Split('-'),
                segment => Assert.Equal(4, segment.Length));
        }
    }

    [Fact]
    [Trait("TestId", "UT-FND-PII-12")]
    public void TheGuardScansALargeCleanBodyWithinBudgetInsteadOfTimingOut()
    {
        // W-0040 section 6. PiiMaskingFilter serialises the whole response body and hands it to
        // the guard, so this input is bounded by nothing -- an admin list runs to hundreds of
        // kilobytes. The old budget was 100 ms against an interpreted pattern costing roughly
        // 0.19 ms/KB, which a body this size cannot finish inside; and .NET charges regex
        // timeouts against wall clock rather than CPU, so a loaded host tripped it on inputs far
        // smaller than this one -- including a 128-character correlation header.
        var builder = new StringBuilder(1_100_000);
        while (builder.Length < 1_000_000)
        {
            builder.Append("{\"taskId\":\"TASK-a1b2c3d4\",\"program\":\"GOLDEN_HOUR\",\"status\":\"DELIVERED_ACCEPTED\"},");
        }

        string clean = builder.ToString();
        Assert.True(PiiGuard.IsSafeText(clean));

        // The budget moved; the detection set did not. The same body carrying one restricted
        // value is still caught, at the far end where a short-circuiting scan cannot cheat.
        Assert.False(PiiGuard.IsSafeText(clean + "contact 0912341234"));
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
