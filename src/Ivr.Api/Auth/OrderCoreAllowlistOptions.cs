namespace Ivr.Api.Auth;

public sealed class OrderCoreAllowlistOptions
{
    public const string SourceSystem = "order-core";
    public const string TokenConfigurationKey = "ORDER_CORE_SERVICE_TOKEN";

    public string ServiceToken { get; set; } = string.Empty;
}
