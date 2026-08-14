using System.Net;
using System.Text.Json;
using Ivr.Contracts.Generated.SalesTarget.V1;
using Ivr.Contracts.Sales.CurrentGoldenHour;
using Ivr.Infrastructure.Contracts;
using Ivr.Infrastructure.Correlation;
using Ivr.Infrastructure.Persistence.Outbox;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Callbacks;

public interface ICurrentGoldenHourIdentityResolver
{
    public ValueTask<CurrentGoldenHourIdentity?> ResolveAsync(
        string taskId,
        CancellationToken cancellationToken);
}

public sealed class ConfiguredCurrentGoldenHourIdentityResolver(
    IOptions<CallbackDeliveryOptions> options) : ICurrentGoldenHourIdentityResolver
{
    public ValueTask<CurrentGoldenHourIdentity?> ResolveAsync(
        string taskId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskId);
        cancellationToken.ThrowIfCancellationRequested();
        if (!options.Value.CurrentGoldenHourIdentities.TryGetValue(
                taskId,
                out CurrentGoldenHourIdentityOptions? identity))
        {
            return ValueTask.FromResult<CurrentGoldenHourIdentity?>(null);
        }

        return ValueTask.FromResult<CurrentGoldenHourIdentity?>(new CurrentGoldenHourIdentity(
            identity.CallId,
            identity.ReservationId,
            identity.OrderId,
            identity.CustomerId));
    }
}

public sealed class CurrentGoldenHourCallbackTransport(
    ICurrentGoldenHourCallbackClient client,
    ICurrentGoldenHourIdentityResolver identityResolver,
    IOptions<CallbackDeliveryOptions> options,
    ICorrelationContext correlationContext) : ICurrentGoldenHourCallbackTransport
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<CallbackTransportResult> SendAsync(
        CallbackOutboxMessage message,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (!options.Value.CurrentGoldenHourCompatibilityEnabled)
        {
            return Invalid("CURRENT_GOLDEN_HOUR_COMPAT_DISABLED");
        }

        if (!string.Equals(message.ProgramCode, "GOLDEN_HOUR", StringComparison.Ordinal))
        {
            return Invalid("CURRENT_GOLDEN_HOUR_FORBIDS_24_7");
        }

        IvrResultCallbackV1? payload;
        try
        {
            CallbackPayloadIntegrity.EnsureValid(message);
            payload = JsonSerializer.Deserialize<IvrResultCallbackV1>(
                message.PayloadJson,
                SerializerOptions);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            return Invalid("CALLBACK_PAYLOAD_INVALID");
        }

        if (payload is null
            || !string.Equals(payload.Callback_id, message.CallbackId, StringComparison.Ordinal)
            || !string.Equals(payload.Task_id, message.TaskId, StringComparison.Ordinal)
            || !string.Equals(payload.Order_id, message.OfficialOrderId, StringComparison.Ordinal)
            || !payload.Is_final_for_ivr)
        {
            return Invalid("CALLBACK_PATH_BODY_MISMATCH");
        }

        if (payload.Result_type == ResultType.IVR_NO_ANSWER_FINAL
            && payload.Recommended_core_action
                != RecommendedCoreAction.CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT)
        {
            return Invalid("NO_ANSWER_ACTION_INVALID");
        }

        CurrentGoldenHourIdentity? identity = await identityResolver.ResolveAsync(
            message.TaskId,
            cancellationToken);
        if (identity is null)
        {
            return Invalid("CURRENT_GOLDEN_HOUR_IDENTITY_MISSING");
        }

        CurrentGoldenHourCallbackResult? compatibilityResult = MapResult(payload.Result_type);
        if (compatibilityResult is null)
        {
            return Invalid("CURRENT_GOLDEN_HOUR_RESULT_UNREPRESENTABLE");
        }

        var request = new CurrentGoldenHourCallbackRequest
        {
            CallId = identity.CallId,
            ReservationId = identity.ReservationId,
            OrderId = identity.OrderId,
            CustomerId = identity.CustomerId,
            Result = compatibilityResult.Value,
            OccurredAt = payload.Occurred_at,
            IdempotencyKey = message.IdempotencyKey,
        };
        try
        {
            using IDisposable correlationScope = correlationContext.Push(message.CorrelationId);
            CurrentGoldenHourApiResponse<CurrentGoldenHourCallbackResponse> response =
                await client.SubmitAsync(
                    request,
                    options.Value.CurrentGoldenHourInternalToken,
                    cancellationToken);
            if (!response.Success
                || response.Data is null
                || response.Data.CallId != identity.CallId
                || response.Data.ReservationId != identity.ReservationId
                || response.Data.OrderId != identity.OrderId
                || !string.Equals(
                    response.Data.IdempotencyKey,
                    message.IdempotencyKey,
                    StringComparison.Ordinal))
            {
                return new CallbackTransportResult(
                    CallbackTransportOutcome.Invalid,
                    (int)HttpStatusCode.OK,
                    "CURRENT_GOLDEN_HOUR_ACK_INVALID",
                    "CURRENT_GOLDEN_HOUR_ACK_INVALID");
            }

            return new CallbackTransportResult(
                CallbackTransportOutcome.Accepted,
                (int)HttpStatusCode.OK,
                "CURRENT_DELIVERY_ACCEPTED");
        }
        catch (CurrentGoldenHourCallbackException exception)
        {
            int? status = exception.StatusCode is null
                ? null
                : (int)exception.StatusCode.Value;
            if (exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return new CallbackTransportResult(
                    CallbackTransportOutcome.AuthRejected,
                    status,
                    "CURRENT_GOLDEN_HOUR_AUTH_REJECTED",
                    "CURRENT_GOLDEN_HOUR_AUTH_REJECTED");
            }

            if (exception.StatusCode == HttpStatusCode.TooManyRequests || status is >= 500)
            {
                return new CallbackTransportResult(
                    CallbackTransportOutcome.TransientFailure,
                    status,
                    "CURRENT_GOLDEN_HOUR_RETRYABLE",
                    "CURRENT_GOLDEN_HOUR_RETRYABLE");
            }

            return new CallbackTransportResult(
                CallbackTransportOutcome.Invalid,
                status,
                "CURRENT_GOLDEN_HOUR_REJECTED",
                "CURRENT_GOLDEN_HOUR_REJECTED");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new CallbackTransportResult(
                CallbackTransportOutcome.TransientFailure,
                null,
                "CURRENT_GOLDEN_HOUR_TIMEOUT",
                "CURRENT_GOLDEN_HOUR_TIMEOUT");
        }
        catch (HttpRequestException)
        {
            return new CallbackTransportResult(
                CallbackTransportOutcome.TransientFailure,
                null,
                "CURRENT_GOLDEN_HOUR_TRANSPORT_FAILURE",
                "CURRENT_GOLDEN_HOUR_TRANSPORT_FAILURE");
        }
        catch (JsonException)
        {
            return Invalid("CURRENT_GOLDEN_HOUR_ACK_INVALID");
        }
    }

    private static CurrentGoldenHourCallbackResult? MapResult(ResultType result) => result switch
    {
        ResultType.IVR_CONFIRMED => CurrentGoldenHourCallbackResult.Confirmed,
        ResultType.IVR_CUSTOMER_CANCELLED => CurrentGoldenHourCallbackResult.Rejected,
        ResultType.IVR_NO_ANSWER_FINAL => CurrentGoldenHourCallbackResult.NoAnswer,
        ResultType.IVR_TECHNICAL_EXCEPTION => CurrentGoldenHourCallbackResult.Failed,
        _ => null,
    };

    private static CallbackTransportResult Invalid(string code) => new(
        CallbackTransportOutcome.Invalid,
        null,
        code,
        code);
}
