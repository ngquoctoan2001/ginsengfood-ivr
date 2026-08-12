using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ivr.Contracts.Sales.CurrentGoldenHour;

/// <summary>
/// Result values implemented by the current Golden Hour Sales endpoint at
/// Sales commit a3aad246. These values are not Target V1 result types.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<CurrentGoldenHourCallbackResult>))]
public enum CurrentGoldenHourCallbackResult
{
    [JsonStringEnumMemberName("CONFIRMED")]
    Confirmed,

    [JsonStringEnumMemberName("REJECTED")]
    Rejected,

    [JsonStringEnumMemberName("NO_ANSWER")]
    NoAnswer,

    [JsonStringEnumMemberName("FAILED")]
    Failed,
}

/// <summary>
/// Compatibility-only callback request verified against the current Java DTO.
/// It intentionally has no contract version, order version, semantic action,
/// evidence reference, or Target V1 attempt metadata.
/// </summary>
public sealed record CurrentGoldenHourCallbackRequest
{
    [JsonPropertyName("callId")]
    public required long CallId { get; init; }

    [JsonPropertyName("reservationId")]
    public required long ReservationId { get; init; }

    [JsonPropertyName("orderId")]
    public required long OrderId { get; init; }

    [JsonPropertyName("customerId")]
    public required long CustomerId { get; init; }

    [JsonPropertyName("result")]
    public required CurrentGoldenHourCallbackResult Result { get; init; }

    [JsonPropertyName("occurredAt")]
    public DateTimeOffset? OccurredAt { get; init; }

    [JsonPropertyName("idempotencyKey")]
    public required string IdempotencyKey { get; init; }
}

/// <summary>
/// Compatibility-only response data returned by the current Java endpoint.
/// </summary>
public sealed record CurrentGoldenHourCallbackResponse
{
    public required long CallId { get; init; }

    public required long ReservationId { get; init; }

    public required long OrderId { get; init; }

    public required string BeforeStatus { get; init; }

    public required string AfterStatus { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public required string IdempotencyKey { get; init; }
}

/// <summary>
/// Current Sales <c>ApiResponse&lt;T&gt;</c> envelope. It does not provide the
/// semantic Target V1 ACK taxonomy.
/// </summary>
public sealed record CurrentGoldenHourApiResponse<T>
{
    public required bool Success { get; init; }

    public required string Message { get; init; }

    public T? Data { get; init; }

    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; init; }

    [JsonPropertyName("request_id")]
    public string? RequestId { get; init; }

    [JsonPropertyName("correlation_id")]
    public string? CorrelationId { get; init; }
}

/// <summary>
/// Sends only the verified current Golden Hour DTO to its compatibility route.
/// </summary>
public interface ICurrentGoldenHourCallbackClient
{
    /// <summary>
    /// Submits one compatibility callback using the transitional internal token.
    /// </summary>
    public Task<CurrentGoldenHourApiResponse<CurrentGoldenHourCallbackResponse>> SubmitAsync(
        CurrentGoldenHourCallbackRequest request,
        string internalToken,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// HTTP client for the current Golden Hour route. The token is supplied per call
/// so it is never stored in a DTO or generated contract.
/// </summary>
public sealed class CurrentGoldenHourCallbackClient(HttpClient httpClient)
    : ICurrentGoldenHourCallbackClient
{
    public const string EndpointPath = "/api/v1/internal/ivr/golden-hour/callbacks";

    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    /// <inheritdoc />
    public async Task<CurrentGoldenHourApiResponse<CurrentGoldenHourCallbackResponse>> SubmitAsync(
        CurrentGoldenHourCallbackRequest request,
        string internalToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(internalToken);

        using HttpRequestMessage message = new(HttpMethod.Post, EndpointPath)
        {
            Content = JsonContent.Create(request, options: SerializerOptions),
        };
        message.Headers.Add("X-Internal-Token", internalToken);

        using HttpResponseMessage response = await httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new CurrentGoldenHourCallbackException(response.StatusCode);
        }

        CurrentGoldenHourApiResponse<CurrentGoldenHourCallbackResponse>? envelope =
            await response.Content.ReadFromJsonAsync<
                CurrentGoldenHourApiResponse<CurrentGoldenHourCallbackResponse>>(
                    SerializerOptions,
                    cancellationToken);

        return envelope
            ?? throw new CurrentGoldenHourCallbackException(
                response.StatusCode,
                "The current compatibility endpoint returned an empty response.");
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(
            namingPolicy: null,
            allowIntegerValues: false));
        return options;
    }
}

/// <summary>
/// Redacted transport failure for the current compatibility endpoint.
/// </summary>
public sealed class CurrentGoldenHourCallbackException : HttpRequestException
{
    public CurrentGoldenHourCallbackException(
        HttpStatusCode statusCode,
        string message = "The current Golden Hour compatibility callback failed.")
        : base(message, inner: null, statusCode)
    {
    }
}
