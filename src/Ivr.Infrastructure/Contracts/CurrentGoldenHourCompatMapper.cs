using Ivr.Contracts.Sales.CurrentGoldenHour;
using Ivr.Domain.Confirmation;
using SalesTargetResultType = Ivr.Contracts.Generated.SalesTarget.V1.ResultType;

namespace Ivr.Infrastructure.Contracts;

public sealed record CurrentGoldenHourIdentity(
    long CallId,
    long ReservationId,
    long OrderId,
    long CustomerId);

public static class CurrentGoldenHourCompatMapper
{
    public static CurrentGoldenHourCallbackResult? ToCompatibilityResult(
        SalesTargetResultType result) => result switch
        {
            SalesTargetResultType.IVR_CONFIRMED => CurrentGoldenHourCallbackResult.Confirmed,
            SalesTargetResultType.IVR_CUSTOMER_CANCELLED => CurrentGoldenHourCallbackResult.Rejected,
            SalesTargetResultType.IVR_NO_ANSWER_FINAL => CurrentGoldenHourCallbackResult.NoAnswer,
            SalesTargetResultType.IVR_TECHNICAL_EXCEPTION => CurrentGoldenHourCallbackResult.Failed,
            _ => null,
        };

    public static CurrentGoldenHourCallbackRequest ToCompatibilityWire(
        CurrentGoldenHourIdentity identity,
        CallResultSnapshot source,
        string idempotencyKey)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if (source.Program != IvrProgramCode.GoldenHour)
        {
            throw new InvalidOperationException("The current compatibility endpoint supports Golden Hour only.");
        }

        return new CurrentGoldenHourCallbackRequest
        {
            CallId = identity.CallId,
            ReservationId = identity.ReservationId,
            OrderId = identity.OrderId,
            CustomerId = identity.CustomerId,
            Result = MapResult(source.ResultType),
            OccurredAt = source.OccurredAt,
            IdempotencyKey = idempotencyKey,
        };
    }

    private static CurrentGoldenHourCallbackResult MapResult(IvrResultType result) => result switch
    {
        IvrResultType.IvrConfirmed => CurrentGoldenHourCallbackResult.Confirmed,
        IvrResultType.IvrCustomerCancelled => CurrentGoldenHourCallbackResult.Rejected,
        IvrResultType.IvrNoAnswerAttempt or IvrResultType.IvrNoAnswerFinal => CurrentGoldenHourCallbackResult.NoAnswer,
        IvrResultType.IvrTechnicalException => CurrentGoldenHourCallbackResult.Failed,
        _ => throw new InvalidOperationException("Result cannot be represented safely by the current compatibility contract."),
    };
}
