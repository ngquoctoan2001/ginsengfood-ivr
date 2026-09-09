using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using Ivr.Domain.Confirmation;
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
    /// <summary>
    /// Both idempotency stores must replay the same response shapes, because which one is wired is
    /// an environment decision the caller knows nothing about.
    /// <para>
    /// They had drifted: the Postgres store registered a converter for
    /// <see cref="IReadOnlySet{T}"/> and the in-memory store built its own options without one, so
    /// a response carrying a read-only set replayed in production and threw in MOCK — the mode
    /// used to rehearse the thing before it ships. Two constructions of "the same" options is the
    /// defect; they share one instance now.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-FND-IDEMP-04")]
    public async Task BothStoresReplayTheSameResponseShapes()
    {
        InMemoryIdempotencyStore store = new(TimeProvider.System);
        IReadOnlySet<string> reasons = new HashSet<string>(StringComparer.Ordinal)
        {
            "SAFE_REASON_A",
            "SAFE_REASON_B",
        };

        SetBearingResponse first = await store.ExecuteAsync(
            "set-bearing-1",
            "payload-a",
            _ => Task.FromResult(new SetBearingResponse("ELIGIBLE_FOR_IVR", reasons)));

        // The replay is the deserialization path, and the one that used to throw: the first call
        // returns the object the factory made without a round trip, so only the second proves the
        // snapshot can be read back at all.
        SetBearingResponse replay = await store.ExecuteAsync(
            "set-bearing-1",
            "payload-a",
            _ => Task.FromResult(new SetBearingResponse("unexpected", reasons)));

        Assert.Equal("ELIGIBLE_FOR_IVR", replay.Decision);
        Assert.Equal(first.Reasons.Order(StringComparer.Ordinal), replay.Reasons.Order(StringComparer.Ordinal));
    }

    private sealed record SetBearingResponse(string Decision, IReadOnlySet<string> Reasons);

    /// <summary>
    /// The store must stop growing, in the mode the shipped image actually runs.
    /// <para>
    /// It kept a response snapshot per key with no expiry and a <see cref="SemaphoreSlim"/> per
    /// key that was never removed or disposed. MOCK is the image default, so this is the store
    /// behind every soak run and pilot — the one place where "it dies with the process" is not an
    /// answer, because the process is meant to stay up.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-FND-IDEMP-05")]
    public async Task RetainedResponsesAreCappedAndTheNewestSurvives()
    {
        var clock = new SteppableClock(new DateTimeOffset(2026, 9, 9, 0, 0, 0, TimeSpan.Zero));
        var store = new InMemoryIdempotencyStore(clock, TimeSpan.FromDays(1), maximumRecords: 50);

        for (int index = 0; index < 500; index++)
        {
            await store.ExecuteAsync(
                $"burst-{index}",
                "payload-a",
                _ => Task.FromResult(new SampleResponse(index, "accepted")));
            clock.Advance(TimeSpan.FromSeconds(1));
        }

        // Bounded by the ceiling, not by how long the run lasted.
        Assert.True(store.Count <= 50, $"retained {store.Count} records past a ceiling of 50");

        // And it evicted the oldest, not whatever was convenient: the last key written still
        // replays without re-running the factory.
        bool reran = false;
        SampleResponse replay = await store.ExecuteAsync(
            "burst-499",
            "payload-a",
            _ =>
            {
                reran = true;
                return Task.FromResult(new SampleResponse(-1, "unexpected"));
            });
        Assert.False(reran);
        Assert.Equal(499, replay.Sequence);
    }

    [Fact]
    [Trait("TestId", "UT-FND-IDEMP-06")]
    public async Task ARecordPastItsWindowIsReExecutedRatherThanReplayed()
    {
        var clock = new SteppableClock(new DateTimeOffset(2026, 9, 9, 0, 0, 0, TimeSpan.Zero));
        var store = new InMemoryIdempotencyStore(clock, TimeSpan.FromHours(1));
        int executions = 0;

        await store.ExecuteAsync(
            "expiring",
            "payload-a",
            _ => Task.FromResult(new SampleResponse(++executions, "accepted")));
        Assert.Equal(1, executions);

        // Inside the window it still replays.
        clock.Advance(TimeSpan.FromMinutes(59));
        await store.ExecuteAsync(
            "expiring",
            "payload-a",
            _ => Task.FromResult(new SampleResponse(++executions, "unexpected")));
        Assert.Equal(1, executions);

        // Past it, the factory runs again. This is a real semantic, not an accident: it is what
        // retention does to ivr_idempotency_keys, and the alternative was growing until the
        // process died, which loses every key rather than the oldest one.
        clock.Advance(TimeSpan.FromMinutes(2));
        await store.ExecuteAsync(
            "expiring",
            "payload-a",
            _ => Task.FromResult(new SampleResponse(++executions, "accepted")));
        Assert.Equal(2, executions);
    }

    /// <summary>
    /// Asserted by shape rather than by behaviour, because the leak was invisible from the
    /// outside: a per-key lock dictionary looks identical to a bounded one until memory runs out.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-FND-IDEMP-07")]
    public void NoPerKeyLockDictionarySurvivesOnTheStore()
    {
        FieldInfo[] fields = typeof(InMemoryIdempotencyStore)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.DoesNotContain(
            fields,
            field => field.FieldType.IsGenericType
                && field.FieldType.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>)
                && field.FieldType.GetGenericArguments()[1] == typeof(SemaphoreSlim));

        // What replaced it: a fixed array, so the lock count cannot depend on how many distinct
        // keys the process has ever seen.
        Assert.Contains(fields, field => field.FieldType == typeof(SemaphoreSlim[]));
    }

    private sealed class SteppableClock(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset now = start;

        public override DateTimeOffset GetUtcNow() => now;

        public void Advance(TimeSpan delta) => now = now.Add(delta);
    }

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

    [Theory]
    [Trait("TestId", "UT-PII-PRODUCT-01")]
    [InlineData("Tổ yến")]
    [InlineData("Tổ yến chưng đường phèn")]
    [InlineData("Đường phèn")]
    [InlineData("Đường thốt nốt")]
    [InlineData("Chè đường phèn")]
    [InlineData("Mật ong đường mía")]
    [InlineData("Ấp trứng")]
    [InlineData("Thôn quê")]
    public void OrdinaryProductNamesReachAConfirmationCall(string publicName)
    {
        // Every one of these is refused by IsSafeText, because đường, tổ, ấp and thôn are address
        // markers as well as ordinary food words -- đường is sugar before it is a street, tổ is a
        // nest. Before W-0243 a bird's-nest or rock-sugar order could not be confirmed by phone at
        // all: intake rejected the task at the door.
        SpeechItem.Create(publicName, 1m, "hộp");
        Assert.True(PiiGuard.IsSafeProductText(publicName));

        // And the guard they used to fail still fails them. This is a narrower rule for one field,
        // not a rule that changed.
        Assert.False(PiiGuard.IsSafeText(publicName));
    }

    [Theory]
    [Trait("TestId", "UT-PII-PRODUCT-02")]
    [InlineData("số nhà 12")]
    [InlineData("Ngõ 5 Kim Mã")]
    [InlineData("Hẻm 3 Lê Lợi")]
    [InlineData("Ngách 12/4")]
    [InlineData("so nha 12")]
    [InlineData("ngo 5 Kim Ma")]
    [InlineData("0912345678")]
    [InlineData("+84912345678")]
    [InlineData("dial_token: abcdefgh12345")]
    public void TheFormsThatCarryARealAddressAreStillRefused(string publicName)
    {
        // số nhà, ngõ, hẻm and ngách mean an address and nothing else, so they stay -- along with
        // phone numbers and dial tokens. What is deliberately given up is narrower: a product name
        // reading "đường Nguyễn Huệ" now passes, which is the cost the owner accepted on
        // 2026-09-09 to let tổ yến be ordered.
        Assert.Throws<InvalidOperationException>(() => SpeechItem.Create(publicName, 1m, "hộp"));
        Assert.False(PiiGuard.IsSafeProductText(publicName));
    }

    [Fact]
    [Trait("TestId", "UT-PII-PRODUCT-03")]
    public void TheLooseningReachesOneFieldAndNoOther()
    {
        // The boundary, and the reason this is safe to do at all. A delivery area is an address
        // field, so it keeps IsSafeText and still refuses the same string a product name now
        // accepts. If this ever passes, the narrower guard has leaked out of the field it was
        // written for.
        SpeechItem.Create("Đường phèn", 1m, "hộp");
        Assert.Throws<InvalidOperationException>(() => ShortDeliveryArea.Create("Đường phèn"));

        // Same string, two answers, on purpose.
        Assert.True(PiiGuard.IsSafeProductText("Đường phèn"));
        Assert.False(PiiGuard.IsSafeText("Đường phèn"));
    }

    private sealed record SampleResponse(int Sequence, string Status);
}
