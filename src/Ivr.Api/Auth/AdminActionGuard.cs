using Ivr.Domain.Errors;

namespace Ivr.Api.Auth;

public static class AdminActionGuard
{
    public static string RequireReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw IvrErrors.MalformedRequest("An admin action requires a reason.");
        }

        return reason.Trim();
    }
}
