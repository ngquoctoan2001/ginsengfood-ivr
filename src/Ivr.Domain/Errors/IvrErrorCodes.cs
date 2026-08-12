using System.Collections.Frozen;

namespace Ivr.Domain.Errors;

/// <summary>
/// Stable error codes from API-06 section 1c. Do not add a code without updating
/// the source specification and its HTTP mapping.
/// </summary>
public static class IvrErrorCodes
{
    public const string Unauthenticated = "IVR_UNAUTHENTICATED";
    public const string ForbiddenCaller = "IVR_FORBIDDEN_CALLER";
    public const string MalformedRequest = "IVR_MALFORMED_REQUEST";
    public const string MissingTrace = "IVR_MISSING_TRACE";
    public const string IdempotencyConflict = "IVR_IDEMPOTENCY_CONFLICT";
    public const string VersionConflict = "IVR_VERSION_CONFLICT";
    public const string NotOfficialOrder = "IVR_NOT_OFFICIAL_ORDER";
    public const string StateNotCallable = "IVR_STATE_NOT_CALLABLE";
    public const string PolicyMismatch = "IVR_POLICY_MISMATCH";
    public const string ContactInvalid = "IVR_CONTACT_INVALID";
    public const string ScriptNotApproved = "IVR_SCRIPT_NOT_APPROVED";
    public const string OperationalBlocked = "IVR_OPERATIONAL_BLOCKED";
    public const string NotFound = "IVR_NOT_FOUND";
    public const string RateLimited = "IVR_RATE_LIMITED";
    public const string InternalError = "IVR_INTERNAL_ERROR";

    private static readonly FrozenSet<string> DefinedCodes = new[]
        {
            Unauthenticated,
            ForbiddenCaller,
            MalformedRequest,
            MissingTrace,
            IdempotencyConflict,
            VersionConflict,
            NotOfficialOrder,
            StateNotCallable,
            PolicyMismatch,
            ContactInvalid,
            ScriptNotApproved,
            OperationalBlocked,
            NotFound,
            RateLimited,
            InternalError,
        }
        .ToFrozenSet(StringComparer.Ordinal);

    public static IReadOnlySet<string> All => DefinedCodes;

    public static bool IsDefined(string code) => DefinedCodes.Contains(code);
}
