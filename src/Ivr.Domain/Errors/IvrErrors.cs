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

    public static IvrFailureException PolicyMismatch(string safeMessage) =>
        new(IvrErrorCodes.PolicyMismatch, safeMessage);

    public static IvrFailureException PiiPolicyViolation() =>
        new(
            IvrErrorCodes.PiiPolicyViolation,
            "The request violates the IVR privacy policy.");

    public static IvrFailureException OperationalBlocked(string safeMessage) =>
        new(IvrErrorCodes.OperationalBlocked, safeMessage);

    public static IvrFailureException NotFound(string safeMessage) =>
        new(IvrErrorCodes.NotFound, safeMessage);

    public static IvrFailureException InternalError() =>
        new(IvrErrorCodes.InternalError, "An internal error occurred.");

    public static IvrFailureException RateLimited() =>
        new(IvrErrorCodes.RateLimited, "Too many requests. Try again later.");

    /// <summary>
    /// W-0282 / B2. The same refusal, carrying the two numbers a client needs to back off without
    /// guessing: how long this account's window has left, and what its ceiling was.
    /// <para>
    /// They travel in <c>details</c> rather than in a <c>Retry-After</c> header because
    /// <c>ErrorEnvelope.details</c> is already a free-form string map in the published contract,
    /// so this adds no undeclared wire surface. Declaring the header properly means a distinct
    /// response component on all 38 operations and a contract release; that is recorded for the
    /// next contract version rather than smuggled in as runtime behaviour nobody documented.
    /// </para>
    /// </summary>
    public static IvrFailureException RateLimited(int retryAfterSeconds, int requestsPerWindow) =>
        new(
            IvrErrorCodes.RateLimited,
            "Too many requests. Try again later.",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["retry_after_seconds"] = retryAfterSeconds.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
                ["requests_per_window"] = requestsPerWindow.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
            });
}
