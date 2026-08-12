using System.Net;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Ivr.Contracts.Generated.IvrServer.V1;
using Ivr.Contracts.Sales;
using Ivr.Contracts.Sales.CurrentGoldenHour;
using SalesTarget = Ivr.Contracts.Generated.SalesTarget.V1;

namespace Ivr.ContractTests;

public sealed class SalesContractScaffoldTests
{
    [Fact]
    [Trait("TestId", "CT-CONTRACT-SEPARATION-01")]
    public void TargetInternalCurrentAndWireDtosAreNotAssignable()
    {
        Type internalTarget = typeof(IvrConfirmationResultCallbackTargetV1);
        Type targetWire = typeof(SalesTarget.IvrResultCallbackV1);
        Type currentWire = typeof(CurrentGoldenHourCallbackRequest);

        Assert.False(internalTarget.IsAssignableFrom(targetWire));
        Assert.False(targetWire.IsAssignableFrom(internalTarget));
        Assert.False(currentWire.IsAssignableFrom(targetWire));
        Assert.False(targetWire.IsAssignableFrom(currentWire));
    }

    [Theory]
    [Trait("TestId", "CT-CONTRACT-PROVIDER-02")]
    [InlineData(SalesProviderKind.FakeTargetV1, "GOLDEN_HOUR", SalesCallbackContractKind.TargetV1)]
    [InlineData(SalesProviderKind.FakeTargetV1, "TWENTY_FOUR_SEVEN", SalesCallbackContractKind.TargetV1)]
    [InlineData(SalesProviderKind.TargetV1, "GOLDEN_HOUR", SalesCallbackContractKind.TargetV1)]
    [InlineData(SalesProviderKind.TargetV1, "TWENTY_FOUR_SEVEN", SalesCallbackContractKind.TargetV1)]
    [InlineData(SalesProviderKind.CurrentGoldenHourCompat, "GOLDEN_HOUR", SalesCallbackContractKind.CurrentGoldenHourCompat)]
    public void ContractSelectionIsTypedAndProgramBound(
        SalesProviderKind provider,
        string program,
        SalesCallbackContractKind expected)
    {
        Assert.Equal(expected, SalesCallbackContractSelector.Select(provider, program));
    }

    [Fact]
    [Trait("TestId", "CT-CONTRACT-PROVIDER-03")]
    public void CurrentGoldenHourCompatRejectsTwentyFourSeven()
    {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => SalesCallbackContractSelector.Select(
                SalesProviderKind.CurrentGoldenHourCompat,
                "TWENTY_FOUR_SEVEN"));

        Assert.Contains("cannot receive 24/7", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [Trait("TestId", "CT-CONTRACT-TARGET-ACK-04")]
    [InlineData("ACCEPTED", SalesTarget.CallbackAck200Code.ACCEPTED)]
    [InlineData("DUPLICATE_ACCEPTED", SalesTarget.CallbackAck200Code.DUPLICATE_ACCEPTED)]
    [InlineData("BLOCKED_BY_CORE", SalesTarget.CallbackAck200Code.BLOCKED_BY_CORE)]
    [InlineData("REVIEW_REQUIRED", SalesTarget.CallbackAck200Code.REVIEW_REQUIRED)]
    public async Task GeneratedTargetClientParsesEverySuccessSemanticCode(
        string semanticCode,
        SalesTarget.CallbackAck200Code expected)
    {
        using TargetResponseHandler handler = new(
            HttpStatusCode.OK,
            $$"""{"code":"{{semanticCode}}","callback_id":"callback-1","correlation_id":"correlation-1"}""");
        using HttpClient httpClient = new(handler);
        SalesTarget.SalesTargetV1Client client = new(httpClient)
        {
            BaseUrl = "http://fake-sales.local",
        };

        SalesTarget.CallbackAck200 response = await client
            .SubmitIvrOrderConfirmationResultAsync(
                "order-1",
                "idempotency-0001",
                "correlation-1",
                CreateTargetCallback());

        Assert.Equal(expected, response.Code);
        Assert.True(handler.ContractObserved);
    }

    [Theory]
    [Trait("TestId", "CT-CONTRACT-TARGET-ERROR-05")]
    [InlineData(409, "REJECTED_STALE")]
    [InlineData(409, "IDEMPOTENCY_CONFLICT")]
    [InlineData(422, "INVALID_OUTCOME")]
    [InlineData(429, "RATE_LIMITED")]
    [InlineData(500, "SERVER_FAILURE")]
    [InlineData(503, "SERVICE_UNAVAILABLE")]
    public async Task GeneratedTargetClientTypesEveryConflictInvalidAndRetryableStatus(
        int statusCode,
        string semanticCode)
    {
        string body = statusCode == 409
            ? $$"""{"code":"{{semanticCode}}","callback_id":"callback-1","correlation_id":"correlation-1"}"""
            : $$"""{"code":"{{semanticCode}}","correlation_id":"correlation-1"}""";
        using TargetResponseHandler handler = new((HttpStatusCode)statusCode, body);
        using HttpClient httpClient = new(handler);
        SalesTarget.SalesTargetV1Client client = new(httpClient)
        {
            BaseUrl = "http://fake-sales.local",
        };

        Exception? thrown = await Record.ExceptionAsync(
            () => client.SubmitIvrOrderConfirmationResultAsync(
                "order-1",
                "idempotency-0001",
                "correlation-1",
                CreateTargetCallback()));
        SalesTarget.ApiException exception = Assert.IsAssignableFrom<SalesTarget.ApiException>(
            thrown);

        Assert.Equal(statusCode, exception.StatusCode);
        Assert.True(handler.ContractObserved);
        if (statusCode == 409)
        {
            Assert.IsType<SalesTarget.ApiException<SalesTarget.CallbackAck409>>(exception);
        }
        else
        {
            Assert.IsType<SalesTarget.ApiException<SalesTarget.ErrorResponse>>(exception);
        }
    }

    [Fact]
    [Trait("TestId", "CT-CONTRACT-CURRENT-06")]
    public async Task CurrentClientUsesOnlyVerifiedRouteHeaderAndDto()
    {
        using CurrentResponseHandler handler = new();
        using HttpClient httpClient = new(handler) { BaseAddress = new Uri("http://fake-sales.local") };
        CurrentGoldenHourCallbackClient client = new(httpClient);

        CurrentGoldenHourApiResponse<CurrentGoldenHourCallbackResponse> response =
            await client.SubmitAsync(
                new CurrentGoldenHourCallbackRequest
                {
                    CallId = 1,
                    ReservationId = 2,
                    OrderId = 3,
                    CustomerId = 4,
                    Result = CurrentGoldenHourCallbackResult.Confirmed,
                    OccurredAt = DateTimeOffset.Parse(
                        "2026-08-12T08:30:00Z",
                        CultureInfo.InvariantCulture),
                    IdempotencyKey = "current-compat-0001",
                },
                "compat-test-token");

        Assert.True(response.Success);
        Assert.Equal(3, response.Data?.OrderId);
        Assert.True(handler.ContractObserved);
    }

    [Fact]
    [Trait("TestId", "CT-CONTRACT-WIREMOCK-07")]
    public async Task WireMockCatalogCoversBothProgramsAndAllAckErrorClasses()
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "WireMock",
            "fake-sales-mappings.json");
        using JsonDocument document = JsonDocument.Parse(await File.ReadAllTextAsync(path));
        JsonElement[] mappings = document.RootElement.GetProperty("Mappings")
            .EnumerateArray()
            .ToArray();

        string[] programs = mappings
            .Where(mapping => mapping.TryGetProperty("x-fixture-program", out _))
            .Select(mapping => mapping.GetProperty("x-fixture-program").GetString() ?? string.Empty)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        int[] statuses = mappings
            .Select(mapping => mapping.GetProperty("Response").GetProperty("StatusCode").GetInt32())
            .Distinct()
            .Order()
            .ToArray();

        Assert.Equal(["GOLDEN_HOUR+ONLINE", "TWENTY_FOUR_SEVEN+COD"], programs);
        Assert.Equal([200, 409, 422, 429, 500, 503], statuses);
        Assert.Contains(mappings, mapping =>
            mapping.GetProperty("Scenario").GetString() == "current-gh-accepted");
        Assert.Contains(mappings, mapping =>
            mapping.GetProperty("Scenario").GetString() == "current-gh-rejected");
    }

    private static SalesTarget.IvrResultCallbackV1 CreateTargetCallback() => new()
    {
        Contract_version = SalesTarget.IvrResultCallbackV1Contract_version.IvrOrderConfirmation_v1,
        Callback_id = "callback-1",
        Task_id = "task-1",
        Order_id = "order-1",
        Order_version_seen_by_ivr = "17",
        Result_type = SalesTarget.ResultType.IVR_CONFIRMED,
        Is_counted_customer_attempt = true,
        Is_final_for_ivr = true,
        Attempt_number = 1,
        Occurred_at = DateTimeOffset.Parse(
            "2026-08-12T08:30:00Z",
            CultureInfo.InvariantCulture),
        Recommended_core_action = SalesTarget.RecommendedCoreAction.CORE_REVALIDATE_AND_CONFIRM_ORDER,
        Evidence_ref = "evidence://fake/result-1",
        Audit_ref = "audit://fake/callback-1",
    };

    private sealed class TargetResponseHandler(HttpStatusCode statusCode, string body)
        : HttpMessageHandler
    {
        public bool ContractObserved { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            string requestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            using JsonDocument document = JsonDocument.Parse(requestBody);
            ContractObserved = request.Method == HttpMethod.Post
                && request.RequestUri?.AbsolutePath
                    == "/api/v1/internal/orders/order-1/ivr-result-callbacks"
                && request.Headers.Contains("Idempotency-Key")
                && request.Headers.Contains("X-Correlation-Id")
                && document.RootElement.GetProperty("order_id").GetString() == "order-1"
                && document.RootElement.GetProperty("order_version_seen_by_ivr").GetString() == "17";

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };
        }
    }

    private sealed class CurrentResponseHandler : HttpMessageHandler
    {
        public bool ContractObserved { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            string requestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            using JsonDocument document = JsonDocument.Parse(requestBody);
            ContractObserved = request.Method == HttpMethod.Post
                && request.RequestUri?.AbsolutePath
                    == CurrentGoldenHourCallbackClient.EndpointPath
                && request.Headers.GetValues("X-Internal-Token").Single() == "compat-test-token"
                && document.RootElement.GetProperty("result").GetString() == "CONFIRMED"
                && !document.RootElement.TryGetProperty("order_version_seen_by_ivr", out _);

            const string responseBody = """
                {
                  "success": true,
                  "message": "Success",
                  "data": {
                    "callId": 1,
                    "reservationId": 2,
                    "orderId": 3,
                    "beforeStatus": "PENDING",
                    "afterStatus": "CONFIRMED",
                    "occurredAt": "2026-08-12T08:30:00Z",
                    "idempotencyKey": "current-compat-0001"
                  }
                }
                """;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json"),
            };
        }
    }
}
