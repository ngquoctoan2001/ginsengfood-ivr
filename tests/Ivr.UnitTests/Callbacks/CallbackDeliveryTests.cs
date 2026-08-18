using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ivr.Contracts.Generated.SalesTarget.V1;
using Ivr.Contracts.Sales;
using Ivr.Contracts.Sales.CurrentGoldenHour;
using Ivr.Domain.Confirmation;
using Ivr.Infrastructure.Callbacks;
using Ivr.Infrastructure.Correlation;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Persistence.Outbox;
using Microsoft.Extensions.Options;
using SalesRecommendedCoreAction = Ivr.Contracts.Generated.SalesTarget.V1.RecommendedCoreAction;

namespace Ivr.UnitTests.Callbacks;

public sealed class CallbackDeliveryTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 14, 1, 0, 0, TimeSpan.Zero);
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    [Theory]
    [Trait("TestId", "UT-CALLBACK-TARGET-ACK-01")]
    [InlineData(200, "ACCEPTED", CallbackTransportOutcome.Accepted)]
    [InlineData(200, "DUPLICATE_ACCEPTED", CallbackTransportOutcome.DuplicateAccepted)]
    [InlineData(200, "BLOCKED_BY_CORE", CallbackTransportOutcome.BlockedByCore)]
    [InlineData(200, "REVIEW_REQUIRED", CallbackTransportOutcome.ReviewRequired)]
    [InlineData(409, "REJECTED_STALE", CallbackTransportOutcome.RejectedStale)]
    [InlineData(409, "IDEMPOTENCY_CONFLICT", CallbackTransportOutcome.IdempotencyConflict)]
    [InlineData(422, "INVALID_RESULT", CallbackTransportOutcome.Invalid)]
    [InlineData(429, "RATE_LIMITED", CallbackTransportOutcome.TransientFailure)]
    [InlineData(500, "SERVER_FAILURE", CallbackTransportOutcome.TransientFailure)]
    [InlineData(503, "UNAVAILABLE", CallbackTransportOutcome.TransientFailure)]
    [InlineData(401, "UNAUTHORIZED", CallbackTransportOutcome.AuthRejected)]
    [InlineData(403, "FORBIDDEN", CallbackTransportOutcome.AuthRejected)]
    public async Task TargetTransportClassifiesEveryRequiredStatus(
        int status,
        string code,
        CallbackTransportOutcome expected)
    {
        CallbackOutboxMessage message = CreateMessage();
        using ScriptedHandler handler = new(call => Response(status, code, message));
        using TargetTransportHarness harness = CreateTargetTransport(handler);

        CallbackTransportResult result = await harness.Transport.SendAsync(
            message,
            CancellationToken.None);

        Assert.Equal(expected, result.Outcome);
        Assert.Equal(status, result.HttpStatus);
        Assert.Equal(1, handler.CallCount);
        Assert.Equal(
            "/api/v1/internal/orders/order-1/ivr-result-callbacks",
            handler.Requests[0].Path);
        Assert.Equal(message.IdempotencyKey, handler.Requests[0].IdempotencyKey);
        Assert.Equal(message.CorrelationId, handler.Requests[0].CorrelationId);
        Assert.Equal("Bearer", handler.Requests[0].Authorization?.Scheme);
    }

    [Fact]
    [Trait("TestId", "UT-CALLBACK-RETRY-IDENTITY-02")]
    public async Task RetryUsesTheExactSameBodyAndIdempotencyKey()
    {
        CallbackOutboxMessage message = CreateMessage();
        using ScriptedHandler handler = new(call => call == 1
            ? Response(503, "UNAVAILABLE", message)
            : Response(200, "ACCEPTED", message));
        using TargetTransportHarness harness = CreateTargetTransport(handler);

        CallbackTransportResult first = await harness.Transport.SendAsync(message, CancellationToken.None);
        CallbackTransportResult second = await harness.Transport.SendAsync(message, CancellationToken.None);

        Assert.Equal(CallbackTransportOutcome.TransientFailure, first.Outcome);
        Assert.Equal(CallbackTransportOutcome.Accepted, second.Outcome);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal(handler.Requests[0].Body, handler.Requests[1].Body);
        Assert.Equal(handler.Requests[0].IdempotencyKey, handler.Requests[1].IdempotencyKey);
        Assert.Equal(message.PayloadSha256, Sha256(handler.Requests[1].Body));
    }

    [Fact]
    [Trait("TestId", "UT-CALLBACK-TIMEOUT-03")]
    public async Task TargetTimeoutIsRetryableWithoutLeakingTransportDetails()
    {
        using ScriptedHandler handler = new(
            async (_, cancellationToken) =>
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return Response(200, "ACCEPTED", CreateMessage());
            });
        CallbackDeliveryOptions settings = CreateOptions();
        settings.RequestTimeoutSeconds = 1;
        using TargetTransportHarness harness = CreateTargetTransport(handler, settings);

        CallbackTransportResult result = await harness.Transport.SendAsync(
            CreateMessage(),
            CancellationToken.None);

        Assert.Equal(CallbackTransportOutcome.TransientFailure, result.Outcome);
        Assert.Equal("CALLBACK_TIMEOUT", result.Code);
        Assert.Equal("CALLBACK_TIMEOUT", result.SafeError);
    }

    [Fact]
    [Trait("TestId", "UT-CALLBACK-IDENTITY-GUARD-04")]
    public async Task PathBodyMismatchAndInvalidNoAnswerNeverReachSales()
    {
        using ScriptedHandler handler = new(_ => Response(200, "ACCEPTED", CreateMessage()));
        using TargetTransportHarness harness = CreateTargetTransport(handler);
        CallbackOutboxMessage mismatch = CreateMessage() with { OfficialOrderId = "order-other" };
        CallbackOutboxMessage invalidNoAnswer = CreateMessage(
            resultType: ResultType.IVR_NO_ANSWER_FINAL,
            action: SalesRecommendedCoreAction.CORE_REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST);

        CallbackTransportResult mismatchResult = await harness.Transport.SendAsync(
            mismatch,
            CancellationToken.None);
        CallbackTransportResult noAnswerResult = await harness.Transport.SendAsync(
            invalidNoAnswer,
            CancellationToken.None);

        Assert.Equal(CallbackTransportOutcome.Invalid, mismatchResult.Outcome);
        Assert.Equal(CallbackTransportOutcome.Invalid, noAnswerResult.Outcome);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    [Trait("TestId", "UT-CALLBACK-NOANSWER-05")]
    public async Task FinalNoAnswerCarriesOnlyNoStateChangeRecommendation()
    {
        CallbackOutboxMessage message = CreateMessage(
            resultType: ResultType.IVR_NO_ANSWER_FINAL,
            action: SalesRecommendedCoreAction.CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT);
        using ScriptedHandler handler = new(_ => Response(200, "ACCEPTED", message));
        using TargetTransportHarness harness = CreateTargetTransport(handler);

        CallbackTransportResult result = await harness.Transport.SendAsync(message, CancellationToken.None);

        Assert.Equal(CallbackTransportOutcome.Accepted, result.Outcome);
        using JsonDocument body = JsonDocument.Parse(handler.Requests.Single().Body);
        Assert.Equal(
            "CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT",
            body.RootElement.GetProperty("recommended_core_action").GetString());
    }

    [Fact]
    [Trait("TestId", "UT-CALLBACK-CORRELATION-05B")]
    public async Task SnapshotCorrelationRemainsAuthoritativeThroughFoundationHandler()
    {
        CallbackOutboxMessage message = CreateMessage();
        using ScriptedHandler inner = new(_ => Response(200, "ACCEPTED", message));
        var correlationContext = new CorrelationContext();
        using var propagation = new CorrelationPropagationHandler(correlationContext)
        {
            InnerHandler = inner,
        };
        using var httpClient = new HttpClient(propagation, disposeHandler: false)
        {
            BaseAddress = new Uri("http://fake-sales.local"),
        };
        CallbackDeliveryOptions settings = CreateOptions();
        var clock = new MutableTimeProvider(Now);
        var transport = new TargetV1CallbackTransport(
            httpClient,
            new RefreshingMockServiceTokenProvider(clock, Options.Create(settings)),
            Options.Create(settings),
            clock,
            correlationContext);

        CallbackTransportResult result = await transport.SendAsync(message, CancellationToken.None);

        Assert.Equal(CallbackTransportOutcome.Accepted, result.Outcome);
        Assert.Equal(message.CorrelationId, inner.Requests.Single().CorrelationId);
        Assert.Null(correlationContext.CorrelationId);
    }

    [Fact]
    [Trait("TestId", "UT-CALLBACK-GH-COMPAT-06")]
    public async Task CurrentGoldenHourTransportUsesOnlyPinnedPathHeaderAndDto()
    {
        CallbackDeliveryOptions settings = CreateOptions();
        settings.Provider = SalesProviderNames.CurrentGoldenHourCompat;
        settings.CurrentGoldenHourCompatibilityEnabled = true;
        settings.CurrentGoldenHourInternalToken = "mock-current-token";
        settings.CurrentGoldenHourIdentities["task-1"] = new CurrentGoldenHourIdentityOptions
        {
            CallId = 11,
            ReservationId = 12,
            OrderId = 13,
            CustomerId = 14,
        };
        using CurrentHandler handler = new();
        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("http://fake-sales.local"),
        };
        var client = new CurrentGoldenHourCallbackClient(httpClient);
        var transport = new CurrentGoldenHourCallbackTransport(
            client,
            new ConfiguredCurrentGoldenHourIdentityResolver(Options.Create(settings)),
            Options.Create(settings),
            new CorrelationContext());

        CallbackTransportResult result = await transport.SendAsync(
            CreateMessage(),
            CancellationToken.None);

        Assert.Equal(CallbackTransportOutcome.Accepted, result.Outcome);
        Assert.Equal(CurrentGoldenHourCallbackClient.EndpointPath, handler.Path);
        Assert.Equal("mock-current-token", handler.InternalToken);
        using JsonDocument body = JsonDocument.Parse(handler.Body!);
        Assert.Equal("CONFIRMED", body.RootElement.GetProperty("result").GetString());
        Assert.False(body.RootElement.TryGetProperty("order_version_seen_by_ivr", out _));
    }

    [Fact]
    [Trait("TestId", "UT-CALLBACK-GH-ISOLATION-07")]
    public async Task CurrentGoldenHourTransportRejectsTwentyFourSevenBeforeHttp()
    {
        CallbackDeliveryOptions settings = CreateOptions();
        settings.CurrentGoldenHourCompatibilityEnabled = true;
        settings.CurrentGoldenHourInternalToken = "mock-current-token";
        using CurrentHandler handler = new();
        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("http://fake-sales.local"),
        };
        var transport = new CurrentGoldenHourCallbackTransport(
            new CurrentGoldenHourCallbackClient(httpClient),
            new ConfiguredCurrentGoldenHourIdentityResolver(Options.Create(settings)),
            Options.Create(settings),
            new CorrelationContext());

        CallbackTransportResult result = await transport.SendAsync(
            CreateMessage() with { ProgramCode = "TWENTY_FOUR_SEVEN" },
            CancellationToken.None);

        Assert.Equal(CallbackTransportOutcome.Invalid, result.Outcome);
        Assert.Equal("CURRENT_GOLDEN_HOUR_FORBIDS_24_7", result.Code);
        Assert.Null(handler.Path);
    }

    [Fact]
    [Trait("TestId", "UT-CALLBACK-GH-INTEGRITY-07B")]
    public async Task CurrentGoldenHourTransportRejectsChangedPayloadBeforeHttp()
    {
        CallbackDeliveryOptions settings = CreateOptions();
        settings.CurrentGoldenHourCompatibilityEnabled = true;
        settings.CurrentGoldenHourInternalToken = "mock-current-token";
        using CurrentHandler handler = new();
        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("http://fake-sales.local"),
        };
        var transport = new CurrentGoldenHourCallbackTransport(
            new CurrentGoldenHourCallbackClient(httpClient),
            new ConfiguredCurrentGoldenHourIdentityResolver(Options.Create(settings)),
            Options.Create(settings),
            new CorrelationContext());
        CallbackOutboxMessage changed = CreateMessage() with
        {
            PayloadJson = CreateMessage().PayloadJson.Replace(
                "SAFE_REASON",
                "CHANGED_REASON",
                StringComparison.Ordinal),
        };

        CallbackTransportResult result = await transport.SendAsync(
            changed,
            CancellationToken.None);

        Assert.Equal(CallbackTransportOutcome.Invalid, result.Outcome);
        Assert.Equal("CALLBACK_PAYLOAD_INVALID", result.Code);
        Assert.Null(handler.Path);
    }

    [Fact]
    [Trait("TestId", "UT-OBS-TRACE-02")]
    public async Task EachDeliveryEmitsOneSpanCarryingTheTaskCorrelationAndItsOutcome()
    {
        // W-0040 / P6-1 §6.2, DF-05. The correlation id the task arrived with has to survive to
        // the outbound boundary. Without it, investigating one order means guessing which of a
        // batch's log lines belong together.
        var captured = new List<System.Diagnostics.Activity>();
        using var listener = new System.Diagnostics.ActivityListener
        {
            ShouldListenTo = source => source.Name == Ivr.Infrastructure.Observability.IvrTelemetry.ServiceName,
            Sample = (ref System.Diagnostics.ActivityCreationOptions<System.Diagnostics.ActivityContext> _) =>
                System.Diagnostics.ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = captured.Add,
        };
        System.Diagnostics.ActivitySource.AddActivityListener(listener);

        CallbackOutboxMessage message = CreateMessage();
        var outbox = new MemoryOutbox(message);
        var target = new StubTargetTransport(new CallbackTransportResult(
            CallbackTransportOutcome.Accepted,
            200,
            "ACCEPTED",
            null));
        CallbackDispatcher dispatcher = CreateDispatcher(outbox, target, CreateOptions());

        Assert.Single(await dispatcher.RunBatchAsync());

        System.Diagnostics.Activity span = Assert.Single(
            captured,
            activity => activity.OperationName == "ivr.callback.deliver");
        Assert.Equal(
            message.CorrelationId,
            span.GetTagItem(Ivr.Infrastructure.Observability.TelemetryTags.CorrelationId));
        Assert.Equal(
            message.ProgramCode,
            span.GetTagItem(Ivr.Infrastructure.Observability.TelemetryTags.Program));

        // The span records how the delivery ended, so a waterfall shows the outcome without a
        // second lookup into the database.
        Assert.Equal(
            "DELIVERED_ACCEPTED",
            span.GetTagItem(Ivr.Infrastructure.Observability.TelemetryTags.Outcome));

        // And nothing customer-identifying rode along.
        foreach (KeyValuePair<string, string?> tag in span.Tags)
        {
            Assert.Contains(tag.Key, Ivr.Infrastructure.Observability.TelemetryTags.Allowed);
            Assert.True(
                tag.Value is null || Ivr.Domain.Privacy.PiiGuard.IsSafeText(tag.Value),
                $"span tag {tag.Key} failed the PII guard");
        }
    }

    [Theory]
    [Trait("TestId", "UT-CALLBACK-STATE-08")]
    [InlineData(CallbackTransportOutcome.Accepted, 200, "DELIVERED_ACCEPTED", 0, false)]
    // W-0036 / P5-2 `CT-CB-02`. Sales saying "I already have this" must land in the SAME terminal
    // state as "accepted", and must not raise a review. It was the one outcome this theory did
    // not name — the transport classified it, but nothing asserted where it came to rest, so a
    // duplicate could have quietly become a review item an operator then has to triage.
    [InlineData(CallbackTransportOutcome.DuplicateAccepted, 200, "DELIVERED_ACCEPTED", 0, false)]
    [InlineData(CallbackTransportOutcome.BlockedByCore, 200, "DELIVERED_BLOCKED", 0, true)]
    [InlineData(CallbackTransportOutcome.ReviewRequired, 200, "DELIVERED_REVIEW", 0, true)]
    [InlineData(CallbackTransportOutcome.RejectedStale, 409, "REJECTED_STALE", 0, true)]
    [InlineData(CallbackTransportOutcome.IdempotencyConflict, 409, "IDEMPOTENCY_CONFLICT", 0, true)]
    [InlineData(CallbackTransportOutcome.Invalid, 422, "INVALID_DEAD_LETTER", 0, true)]
    [InlineData(CallbackTransportOutcome.AuthRejected, 401, "AUTH_REJECTED", 0, true)]
    [InlineData(CallbackTransportOutcome.TransientFailure, 503, "RETRY_PENDING", 1, false)]
    public async Task DispatcherMapsTransportOutcomeToAdminVisibleStateWithoutBlindRetry(
        CallbackTransportOutcome outcome,
        int status,
        string expectedState,
        int expectedRetry,
        bool expectedReview)
    {
        CallbackOutboxMessage message = CreateMessage();
        var outbox = new MemoryOutbox(message);
        var target = new StubTargetTransport(new CallbackTransportResult(
            outcome,
            status,
            outcome.ToString().ToUpperInvariant(),
            outcome == CallbackTransportOutcome.TransientFailure ? "SAFE_TRANSIENT" : null));
        CallbackDeliveryOptions settings = CreateOptions();
        CallbackDispatcher dispatcher = CreateDispatcher(outbox, target, settings);

        CallbackDispatchResult dispatched = Assert.Single(await dispatcher.RunBatchAsync());

        Assert.True(dispatched.Persisted);
        Assert.Equal(expectedState, outbox.Update?.DeliveryStatus);
        Assert.Equal(expectedRetry, outbox.Update?.RetryCount);
        Assert.Equal(expectedReview, outbox.Update?.RequiresReview);
        if (expectedState == "RETRY_PENDING")
        {
            Assert.NotNull(outbox.Update?.NextRetryAt);
        }
        else
        {
            Assert.Null(outbox.Update?.NextRetryAt);
        }
    }

    [Fact]
    [Trait("TestId", "UT-CALLBACK-RETRY-EXHAUSTED-09")]
    public async Task DispatcherStopsAfterConfiguredRetryBudget()
    {
        CallbackDeliveryOptions settings = CreateOptions();
        settings.MaxRetries = 1;
        var outbox = new MemoryOutbox(CreateMessage() with { RetryCount = 1 });
        CallbackDispatcher dispatcher = CreateDispatcher(
            outbox,
            new StubTargetTransport(new CallbackTransportResult(
                CallbackTransportOutcome.TransientFailure,
                503,
                "UNAVAILABLE",
                "SAFE_TRANSIENT")),
            settings);

        await dispatcher.RunBatchAsync();

        Assert.Equal("RETRY_EXHAUSTED", outbox.Update?.DeliveryStatus);
        Assert.Equal(1, outbox.Update?.RetryCount);
        Assert.True(outbox.Update?.RequiresReview);
        Assert.Null(outbox.Update?.NextRetryAt);
    }

    [Fact]
    [Trait("TestId", "UT-CALLBACK-TOKEN-CIRCUIT-10")]
    public async Task MockTokenRefreshAndCircuitReadinessAreDeterministic()
    {
        CallbackDeliveryOptions settings = CreateOptions();
        settings.MockTokenLifetimeSeconds = 60;
        settings.MockTokenRefreshSkewSeconds = 10;
        settings.CircuitFailureThreshold = 2;
        settings.CircuitOpenSeconds = 30;
        var clock = new MutableTimeProvider(Now);
        var tokenProvider = new RefreshingMockServiceTokenProvider(clock, Options.Create(settings));
        string first = (await tokenProvider.GetAsync("sales", CancellationToken.None))
            .RevealToTrustedTransport();
        clock.Advance(TimeSpan.FromSeconds(51));
        string refreshed = (await tokenProvider.GetAsync("sales", CancellationToken.None))
            .RevealToTrustedTransport();
        var circuit = new CallbackCircuitBreaker(clock, Options.Create(settings));

        circuit.RecordTransientFailure();
        circuit.RecordTransientFailure();

        Assert.NotEqual(first, refreshed);
        Assert.False(circuit.TryEnter());
        Assert.Equal("NOT_READY_CIRCUIT_OPEN", circuit.Snapshot().Readiness);
        clock.Advance(TimeSpan.FromSeconds(31));
        Assert.True(circuit.TryEnter());
        Assert.Equal("NOT_READY_CIRCUIT_HALF_OPEN", circuit.Snapshot().Readiness);
        circuit.RecordSuccess();
        Assert.Equal("READY", circuit.Snapshot().Readiness);
    }

    [Fact]
    public async Task UnexpectedTransportFailureBecomesRetryAndReleasesHalfOpenProbe()
    {
        CallbackDeliveryOptions settings = CreateOptions();
        settings.CircuitFailureThreshold = 1;
        settings.CircuitOpenSeconds = 1;
        var clock = new MutableTimeProvider(Now);
        var circuit = new CallbackCircuitBreaker(clock, Options.Create(settings));
        circuit.RecordTransientFailure();
        clock.Advance(TimeSpan.FromSeconds(2));
        var outbox = new MemoryOutbox(CreateMessage());
        var dispatcher = new CallbackDispatcher(
            outbox,
            new ThrowingTargetTransport(),
            new StubCurrentTransport(),
            circuit,
            Options.Create(settings),
            clock);

        CallbackDispatchResult result = Assert.Single(await dispatcher.RunBatchAsync());

        Assert.Equal("CALLBACK_TRANSPORT_UNEXPECTED_FAILURE", result.ResponseCode);
        Assert.Equal("RETRY_PENDING", outbox.Update?.DeliveryStatus);
        Assert.Equal("NOT_READY_CIRCUIT_OPEN", circuit.Snapshot().Readiness);
    }

    [Fact]
    [Trait("TestId", "UT-CALLBACK-CIRCUIT-TERMINAL-13")]
    public async Task TerminalLocalResultReleasesHalfOpenCircuitProbe()
    {
        CallbackDeliveryOptions settings = CreateOptions();
        settings.CircuitFailureThreshold = 1;
        settings.CircuitOpenSeconds = 1;
        var clock = new MutableTimeProvider(Now);
        var circuit = new CallbackCircuitBreaker(clock, Options.Create(settings));
        circuit.RecordTransientFailure();
        clock.Advance(TimeSpan.FromSeconds(2));

        var dispatcher = new CallbackDispatcher(
            new MemoryOutbox(CreateMessage()),
            new StubTargetTransport(new CallbackTransportResult(
                CallbackTransportOutcome.Invalid,
                null,
                "CALLBACK_PAYLOAD_INVALID",
                "CALLBACK_PAYLOAD_INVALID")),
            new StubCurrentTransport(),
            circuit,
            Options.Create(settings),
            clock);

        await dispatcher.RunBatchAsync();

        Assert.Equal("READY", circuit.Snapshot().Readiness);
    }

    [Fact]
    [Trait("TestId", "UT-CALLBACK-OPTIONS-11")]
    public void OptionsFailClosedForRealTargetAndImplicitCurrentCompatibility()
    {
        var validator = new CallbackDeliveryOptionsValidator();
        CallbackDeliveryOptions real = CreateOptions();
        real.Enabled = true;
        real.Provider = SalesProviderNames.TargetV1;
        CallbackDeliveryOptions current = CreateOptions();
        current.Enabled = true;
        current.Provider = SalesProviderNames.CurrentGoldenHourCompat;
        CallbackDeliveryOptions missingAudience = CreateOptions();
        missingAudience.Enabled = true;
        missingAudience.TokenAudience = " ";

        Assert.True(validator.Validate(null, real).Failed);
        Assert.True(validator.Validate(null, current).Failed);
        Assert.Contains(
            "TokenAudience",
            validator.Validate(null, missingAudience).FailureMessage,
            StringComparison.Ordinal);
        Assert.True(validator.Validate(null, CreateOptions()).Succeeded);
    }

    [Fact]
    [Trait("TestId", "UT-CALLBACK-SNAPSHOT-12")]
    public void AtomicSnapshotIsFinalImmutableTargetPayloadWithStableHash()
    {
        NormalizedResult final = DispositionMapper.Normalize(
            Ivr.Domain.Ports.SimProviderDisposition.RingTimeout,
            null,
            null,
            new AttemptNormalizationContext(2, 2, Now, Now.AddMinutes(1), 0, 1));
        CallJobEntity job = CreateJob();
        CallAttemptEntity attempt = CreateAttempt();
        ConfirmationTaskEntity task = CreateTask();

        ResultCallbackEntity callback = CallbackOutboxSnapshotFactory.Create(
            "RESULT-1",
            job,
            attempt,
            task,
            final,
            "evidence://ivr/W-0022/result-1",
            "audit://ivr/00000000-0000-0000-0000-000000000001",
            Now);

        Assert.Equal("READY", callback.DeliveryStatus);
        Assert.Equal("ivr-result:RESULT-1", callback.IdempotencyKey);
        Assert.Equal(Sha256(callback.PayloadJson), callback.PayloadSha256);
        using JsonDocument payload = JsonDocument.Parse(callback.PayloadJson);
        Assert.Equal("IVR_NO_ANSWER_FINAL", payload.RootElement.GetProperty("result_type").GetString());
        Assert.Equal(
            "CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT",
            payload.RootElement.GetProperty("recommended_core_action").GetString());
    }

    private static TargetTransportHarness CreateTargetTransport(
        HttpMessageHandler handler,
        CallbackDeliveryOptions? settings = null)
    {
        settings ??= CreateOptions();
        var clock = new MutableTimeProvider(Now);
        var httpClient = new HttpClient(handler, disposeHandler: false)
        {
            BaseAddress = new Uri("http://fake-sales.local"),
        };
        var tokenProvider = new RefreshingMockServiceTokenProvider(
            clock,
            Options.Create(settings));
        var transport = new TargetV1CallbackTransport(
            httpClient,
            tokenProvider,
            Options.Create(settings),
            clock,
            new CorrelationContext());
        return new TargetTransportHarness(httpClient, transport);
    }

    private static CallbackDispatcher CreateDispatcher(
        MemoryOutbox outbox,
        ITargetV1CallbackTransport target,
        CallbackDeliveryOptions settings)
    {
        var clock = new MutableTimeProvider(Now);
        return new CallbackDispatcher(
            outbox,
            target,
            new StubCurrentTransport(),
            new CallbackCircuitBreaker(clock, Options.Create(settings)),
            Options.Create(settings),
            clock);
    }

    private static CallbackDeliveryOptions CreateOptions() => new()
    {
        Enabled = true,
        Provider = SalesProviderNames.FakeTargetV1,
        TargetBaseUrl = "http://fake-sales.local",
        CurrentGoldenHourBaseUrl = "http://fake-sales.local",
        BatchSize = 10,
        PollIntervalMilliseconds = 100,
        LeaseDurationSeconds = 30,
        RequestTimeoutSeconds = 5,
        MaxRetries = 3,
        BaseRetryDelayMilliseconds = 10,
        MaxRetryDelayMilliseconds = 100,
        MaxJitterMilliseconds = 10,
        CircuitFailureThreshold = 5,
        CircuitOpenSeconds = 30,
        MockTokenLifetimeSeconds = 300,
        MockTokenRefreshSkewSeconds = 30,
    };

    private static CallbackOutboxMessage CreateMessage(
        ResultType resultType = ResultType.IVR_CONFIRMED,
        SalesRecommendedCoreAction action = SalesRecommendedCoreAction.CORE_REVALIDATE_AND_CONFIRM_ORDER)
    {
        var payload = new IvrResultCallbackV1
        {
            Contract_version = IvrResultCallbackV1Contract_version.IvrOrderConfirmation_v1,
            Callback_id = "CALLBACK-RESULT-1",
            Task_id = "task-1",
            Order_id = "order-1",
            Order_version_seen_by_ivr = "17",
            Result_type = resultType,
            Result_reason = "SAFE_REASON",
            Is_counted_customer_attempt = true,
            Is_final_for_ivr = true,
            Attempt_number = 2,
            Occurred_at = Now,
            Recommended_core_action = action,
            Evidence_ref = "evidence://ivr/W-0022/result-1",
            Audit_ref = "audit://ivr/00000000-0000-0000-0000-000000000001",
        };
        string json = JsonSerializer.Serialize(payload, SerializerOptions);
        return new CallbackOutboxMessage(
            "CALLBACK-RESULT-1",
            "task-1",
            "order-1",
            "GOLDEN_HOUR",
            "correlation-1",
            "ivr-result:RESULT-1",
            json,
            Sha256(json),
            0,
            "lease-1");
    }

    private static HttpResponseMessage Response(
        int status,
        string code,
        CallbackOutboxMessage message)
    {
        string body = status switch
        {
            200 or 409 => JsonSerializer.Serialize(new
            {
                code,
                callback_id = message.CallbackId,
                correlation_id = message.CorrelationId,
            }),
            _ => JsonSerializer.Serialize(new { code, correlation_id = message.CorrelationId }),
        };
        return new HttpResponseMessage((HttpStatusCode)status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
    }

    private static CallJobEntity CreateJob() => new()
    {
        IvrCallJobId = "job-1",
        TaskId = "task-1",
        OfficialOrderId = "order-1",
        OrderVersionSnapshot = "17",
        ProgramType = "GOLDEN_HOUR",
    };

    private static CallAttemptEntity CreateAttempt() => new()
    {
        IvrCallAttemptId = "attempt-1",
        IvrCallJobId = "job-1",
        TaskId = "task-1",
        AttemptNumber = 2,
        MaxAttemptsSnapshot = 2,
    };

    private static ConfirmationTaskEntity CreateTask() => new()
    {
        TaskId = "task-1",
        OfficialOrderId = "order-1",
        ProgramType = "GOLDEN_HOUR",
        CorrelationId = "correlation-1",
    };

    private static string Sha256(string value) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private sealed class ScriptedHandler : HttpMessageHandler
    {
        private readonly Func<int, CancellationToken, Task<HttpResponseMessage>> _response;

        public ScriptedHandler(Func<int, HttpResponseMessage> response)
            : this((call, _) => Task.FromResult(response(call)))
        {
        }

        public ScriptedHandler(Func<int, CancellationToken, Task<HttpResponseMessage>> response)
        {
            _response = response;
        }

        public int CallCount { get; private set; }

        public List<ObservedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Requests.Add(new ObservedRequest(
                request.RequestUri?.AbsolutePath ?? string.Empty,
                await request.Content!.ReadAsStringAsync(cancellationToken),
                request.Headers.GetValues("Idempotency-Key").Single(),
                request.Headers.GetValues("X-Correlation-Id").Single(),
                request.Headers.Authorization));
            return await _response(CallCount, cancellationToken);
        }
    }

    private sealed record ObservedRequest(
        string Path,
        string Body,
        string IdempotencyKey,
        string CorrelationId,
        AuthenticationHeaderValue? Authorization);

    private sealed class TargetTransportHarness(
        HttpClient httpClient,
        TargetV1CallbackTransport transport) : IDisposable
    {
        public TargetV1CallbackTransport Transport { get; } = transport;

        public void Dispose() => httpClient.Dispose();
    }

    private sealed class CurrentHandler : HttpMessageHandler
    {
        public string? Path { get; private set; }

        public string? InternalToken { get; private set; }

        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Path = request.RequestUri?.AbsolutePath;
            InternalToken = request.Headers.GetValues("X-Internal-Token").Single();
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            const string response = """
                {
                  "success": true,
                  "message": "Success",
                  "data": {
                    "callId": 11,
                    "reservationId": 12,
                    "orderId": 13,
                    "beforeStatus": "PENDING",
                    "afterStatus": "CONFIRMED",
                    "occurredAt": "2026-08-14T01:00:00Z",
                    "idempotencyKey": "ivr-result:RESULT-1"
                  }
                }
                """;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json"),
            };
        }
    }

    private sealed class StubTargetTransport(CallbackTransportResult result)
        : ITargetV1CallbackTransport
    {
        public Task<CallbackTransportResult> SendAsync(
            CallbackOutboxMessage message,
            CancellationToken cancellationToken) => Task.FromResult(result);
    }

    private sealed class ThrowingTargetTransport : ITargetV1CallbackTransport
    {
        public Task<CallbackTransportResult> SendAsync(
            CallbackOutboxMessage message,
            CancellationToken cancellationToken) =>
            throw new TimeoutException("synthetic unexpected transport failure");
    }

    private sealed class StubCurrentTransport : ICurrentGoldenHourCallbackTransport
    {
        public Task<CallbackTransportResult> SendAsync(
            CallbackOutboxMessage message,
            CancellationToken cancellationToken) => Task.FromResult(new CallbackTransportResult(
                CallbackTransportOutcome.Invalid,
                null,
                "UNEXPECTED_CURRENT_TRANSPORT"));
    }

    private sealed class MemoryOutbox(CallbackOutboxMessage message) : ICallbackOutboxRepository
    {
        private bool _dequeued;

        public CallbackDeliveryUpdate? Update { get; private set; }

        public Task<ResultCallbackEntity> EnqueueAsync(
            ResultCallbackEntity callback,
            CancellationToken cancellationToken = default) => Task.FromResult(callback);

        public Task<IReadOnlyList<CallbackOutboxMessage>> DequeueReadyAsync(
            int batchSize,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<CallbackOutboxMessage> result = _dequeued ? [] : [message];
            _dequeued = true;
            return Task.FromResult(result);
        }

        public Task<bool> CompleteDeliveryAsync(
            string callbackId,
            string leaseToken,
            CallbackDeliveryUpdate update,
            CancellationToken cancellationToken = default)
        {
            Update = update;
            return Task.FromResult(
                callbackId == message.CallbackId && leaseToken == message.LeaseToken);
        }
    }

    private sealed class MutableTimeProvider(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan duration) => _now = _now.Add(duration);
    }
}
