using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Ivr.Contracts.Generated.SalesTarget.V1;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Correlation;
using Ivr.Infrastructure.Persistence.Outbox;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Callbacks;

public sealed class TargetV1CallbackTransport(
    HttpClient httpClient,
    IServiceTokenProvider tokenProvider,
    IOptions<CallbackDeliveryOptions> options,
    TimeProvider timeProvider,
    ICorrelationContext correlationContext) : ITargetV1CallbackTransport
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<CallbackTransportResult> SendAsync(
        CallbackOutboxMessage message,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        IvrResultCallbackV1? payload;
        try
        {
            CallbackPayloadIntegrity.EnsureValid(message);
            payload = JsonSerializer.Deserialize<IvrResultCallbackV1>(
                message.PayloadJson,
                SerializerOptions);
            ValidatePayload(message, payload);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            return new CallbackTransportResult(
                CallbackTransportOutcome.Invalid,
                null,
                "CALLBACK_PAYLOAD_INVALID",
                "CALLBACK_PAYLOAD_INVALID");
        }

        ServiceAccessToken token = await tokenProvider.GetAsync(
            options.Value.TokenAudience,
            cancellationToken);
        if (token.ExpiresAt <= timeProvider.GetUtcNow())
        {
            return new CallbackTransportResult(
                CallbackTransportOutcome.AuthRejected,
                null,
                "SERVICE_TOKEN_EXPIRED",
                "SERVICE_TOKEN_EXPIRED");
        }

        using HttpRequestMessage request = new(
            HttpMethod.Post,
            string.Concat(
                "/api/v1/internal/orders/",
                Uri.EscapeDataString(message.OfficialOrderId),
                "/ivr-result-callbacks"))
        {
            Content = new StringContent(
                message.PayloadJson,
                Encoding.UTF8,
                "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token.RevealToTrustedTransport());
        request.Headers.Add("Idempotency-Key", message.IdempotencyKey);
        request.Headers.Add("X-Correlation-Id", message.CorrelationId);

        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.Value.RequestTimeoutSeconds));
        using IDisposable correlationScope = correlationContext.Push(message.CorrelationId);
        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                timeout.Token);
            return await ClassifyAsync(response, message, timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new CallbackTransportResult(
                CallbackTransportOutcome.TransientFailure,
                null,
                "CALLBACK_TIMEOUT",
                "CALLBACK_TIMEOUT");
        }
        catch (HttpRequestException)
        {
            return new CallbackTransportResult(
                CallbackTransportOutcome.TransientFailure,
                null,
                "CALLBACK_TRANSPORT_FAILURE",
                "CALLBACK_TRANSPORT_FAILURE");
        }
        catch (JsonException)
        {
            return new CallbackTransportResult(
                CallbackTransportOutcome.Invalid,
                null,
                "CALLBACK_ACK_INVALID",
                "CALLBACK_ACK_INVALID");
        }
    }

    private static async Task<CallbackTransportResult> ClassifyAsync(
        HttpResponseMessage response,
        CallbackOutboxMessage message,
        CancellationToken cancellationToken)
    {
        int status = (int)response.StatusCode;
        if (response.StatusCode == HttpStatusCode.OK)
        {
            CallbackAck200? ack = await response.Content.ReadFromJsonAsync<CallbackAck200>(
                SerializerOptions,
                cancellationToken);
            if (ack is null
                || !string.Equals(ack.Callback_id, message.CallbackId, StringComparison.Ordinal)
                || !string.Equals(ack.Correlation_id, message.CorrelationId, StringComparison.Ordinal))
            {
                return InvalidAck(status);
            }

            return ack.Code switch
            {
                CallbackAck200Code.ACCEPTED => Result(
                    CallbackTransportOutcome.Accepted,
                    status,
                    "ACCEPTED"),
                CallbackAck200Code.DUPLICATE_ACCEPTED => Result(
                    CallbackTransportOutcome.DuplicateAccepted,
                    status,
                    "DUPLICATE_ACCEPTED"),
                CallbackAck200Code.BLOCKED_BY_CORE => Result(
                    CallbackTransportOutcome.BlockedByCore,
                    status,
                    "BLOCKED_BY_CORE"),
                CallbackAck200Code.REVIEW_REQUIRED => Result(
                    CallbackTransportOutcome.ReviewRequired,
                    status,
                    "REVIEW_REQUIRED"),
                _ => InvalidAck(status),
            };
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            CallbackAck409? ack = await response.Content.ReadFromJsonAsync<CallbackAck409>(
                SerializerOptions,
                cancellationToken);
            if (ack is null
                || !string.Equals(ack.Callback_id, message.CallbackId, StringComparison.Ordinal)
                || !string.Equals(ack.Correlation_id, message.CorrelationId, StringComparison.Ordinal))
            {
                return InvalidAck(status);
            }

            return ack.Code switch
            {
                CallbackAck409Code.REJECTED_STALE => Result(
                    CallbackTransportOutcome.RejectedStale,
                    status,
                    "REJECTED_STALE"),
                CallbackAck409Code.IDEMPOTENCY_CONFLICT => Result(
                    CallbackTransportOutcome.IdempotencyConflict,
                    status,
                    "IDEMPOTENCY_CONFLICT"),
                _ => InvalidAck(status),
            };
        }

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return Result(
                CallbackTransportOutcome.AuthRejected,
                status,
                "CALLBACK_AUTH_REJECTED",
                "CALLBACK_AUTH_REJECTED");
        }

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            return Result(
                CallbackTransportOutcome.Invalid,
                status,
                "CALLBACK_UNPROCESSABLE",
                "CALLBACK_UNPROCESSABLE");
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests || status >= 500)
        {
            return Result(
                CallbackTransportOutcome.TransientFailure,
                status,
                "CALLBACK_RETRYABLE_RESPONSE",
                "CALLBACK_RETRYABLE_RESPONSE");
        }

        return Result(
            CallbackTransportOutcome.Invalid,
            status,
            "CALLBACK_UNSUPPORTED_RESPONSE",
            "CALLBACK_UNSUPPORTED_RESPONSE");
    }

    private static void ValidatePayload(
        CallbackOutboxMessage message,
        IvrResultCallbackV1? payload)
    {
        if (payload is null
            || !string.Equals(payload.Callback_id, message.CallbackId, StringComparison.Ordinal)
            || !string.Equals(payload.Task_id, message.TaskId, StringComparison.Ordinal)
            || !string.Equals(payload.Order_id, message.OfficialOrderId, StringComparison.Ordinal)
            || !payload.Is_final_for_ivr)
        {
            throw new InvalidOperationException("Callback path or body identity mismatch.");
        }

        if (payload.Result_type == ResultType.IVR_NO_ANSWER_FINAL
            && payload.Recommended_core_action
                != RecommendedCoreAction.CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT)
        {
            throw new InvalidOperationException("Final no-answer may not request an order transition.");
        }
    }

    private static CallbackTransportResult InvalidAck(int status) => Result(
        CallbackTransportOutcome.Invalid,
        status,
        "CALLBACK_ACK_INVALID",
        "CALLBACK_ACK_INVALID");

    private static CallbackTransportResult Result(
        CallbackTransportOutcome outcome,
        int status,
        string code,
        string? safeError = null) => new(outcome, status, code, safeError);
}
