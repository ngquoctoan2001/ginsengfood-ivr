namespace Ivr.Domain.Errors;

/// <summary>
/// Factory for stable failures. Callers use these methods instead of scattering
/// error-code string literals.
/// </summary>
public static class IvrErrors
{
    public static IvrFailureException Unauthenticated() =>
        new(IvrErrorCodes.Unauthenticated, "Authentication is required.");

    public static IvrFailureException ForbiddenCaller() =>
        new(IvrErrorCodes.ForbiddenCaller, "The caller is not permitted.");

    public static IvrFailureException MalformedRequest(string safeMessage) =>
        new(IvrErrorCodes.MalformedRequest, safeMessage);

    public static IvrFailureException IdempotencyConflict() =>
        new(
            IvrErrorCodes.IdempotencyConflict,
            "The idempotency key was already used with a different payload.");

    public static IvrFailureException InternalError() =>
        new(IvrErrorCodes.InternalError, "An internal error occurred.");
}
