namespace Ivr.Api.Auth;

public sealed class OrderCoreAllowlistOptions
{
    public const string SourceSystem = "order-core";
    public const string TokenConfigurationKey = "ORDER_CORE_SERVICE_TOKEN";

    /// <summary>W-0047 / P7-5. The value being rotated OUT, still accepted until it retires.</summary>
    public const string PreviousTokenConfigurationKey = "ORDER_CORE_SERVICE_TOKEN_PREVIOUS";

    /// <summary>
    /// When the previous value stops being accepted, as an ISO-8601 instant. Required whenever a
    /// previous value is configured: without it the overlap would depend on somebody remembering
    /// to remove the variable, and a rotation nobody finishes leaves the old credential valid
    /// forever.
    /// </summary>
    public const string PreviousTokenRetiresAtConfigurationKey = "ORDER_CORE_SERVICE_TOKEN_PREVIOUS_RETIRES_AT";

    public string ServiceToken { get; set; } = string.Empty;

    /// <summary>
    /// Optional. Set during a rotation so both values are accepted while callers move across.
    /// <para>
    /// Two configured values rather than reload-on-change, because the token arrives as an
    /// environment variable and a process cannot see its own environment change. During a rolling
    /// restart the fleet holds a mix of old and new pods, so a caller using either value would hit
    /// failures unless both are accepted — which is exactly what this pair is for.
    /// </para>
    /// </summary>
    public string PreviousServiceToken { get; set; } = string.Empty;

    /// <summary>The instant <see cref="PreviousServiceToken"/> stops being accepted.</summary>
    public DateTimeOffset? PreviousServiceTokenRetiresAt { get; set; }
}
