using Ivr.Contracts.Sales;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Callbacks;

public sealed class CallbackDeliveryOptions
{
    public const string SectionName = "Ivr:CallbackDelivery";

    public bool Enabled { get; set; }

    public string Provider { get; set; } = SalesProviderNames.FakeTargetV1;

    public string TargetBaseUrl { get; set; } = "http://127.0.0.1:5999";

    public string CurrentGoldenHourBaseUrl { get; set; } = "http://127.0.0.1:5999";

    public bool CurrentGoldenHourCompatibilityEnabled { get; set; }

    public string CurrentGoldenHourInternalToken { get; set; } = string.Empty;

    public Dictionary<string, CurrentGoldenHourIdentityOptions> CurrentGoldenHourIdentities { get; set; } = [];

    public string TokenAudience { get; set; } = "sales-order-core";

    public int BatchSize { get; set; } = 16;

    public int PollIntervalMilliseconds { get; set; } = 1000;

    public int LeaseDurationSeconds { get; set; } = 30;

    public int RequestTimeoutSeconds { get; set; } = 5;

    public int MaxRetries { get; set; } = 3;

    public int BaseRetryDelayMilliseconds { get; set; } = 250;

    public int MaxRetryDelayMilliseconds { get; set; } = 5000;

    public int MaxJitterMilliseconds { get; set; } = 100;

    public int CircuitFailureThreshold { get; set; } = 5;

    public int CircuitOpenSeconds { get; set; } = 30;

    public int MockTokenLifetimeSeconds { get; set; } = 300;

    public int MockTokenRefreshSkewSeconds { get; set; } = 30;
}

public sealed class CurrentGoldenHourIdentityOptions
{
    public long CallId { get; set; }

    public long ReservationId { get; set; }

    public long OrderId { get; set; }

    public long CustomerId { get; set; }
}

public sealed class CallbackDeliveryOptionsValidator : IValidateOptions<CallbackDeliveryOptions>
{
    public ValidateOptionsResult Validate(string? name, CallbackDeliveryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!SalesProviderNames.TryParse(options.Provider, out SalesProviderKind provider))
        {
            return ValidateOptionsResult.Fail("CallbackDelivery.Provider is unsupported.");
        }

        if (options.Enabled && provider == SalesProviderKind.TargetV1)
        {
            return ValidateOptionsResult.Fail(
                "Real TARGET_V1 delivery remains BLOCKED_EXTERNAL on W-0006/OD-V1-07 (no approved auth "
                + "profile, no sandbox credential). P4-1 wired the provider; it did not unblock it. "
                + "Use FAKE_TARGET_V1 locally.");
        }

        if (options.Enabled
            && provider is SalesProviderKind.TargetV1 or SalesProviderKind.FakeTargetV1
            && string.IsNullOrWhiteSpace(options.TokenAudience))
        {
            return ValidateOptionsResult.Fail(
                "CallbackDelivery.TokenAudience is required for Target V1 delivery.");
        }

        if (options.Enabled
            && provider == SalesProviderKind.CurrentGoldenHourCompat
            && (!options.CurrentGoldenHourCompatibilityEnabled
                || string.IsNullOrWhiteSpace(options.CurrentGoldenHourInternalToken)))
        {
            return ValidateOptionsResult.Fail(
                "Current Golden Hour compatibility requires its explicit enable flag and internal token.");
        }

        if (!IsSafeBaseUrl(options.TargetBaseUrl)
            || !IsSafeBaseUrl(options.CurrentGoldenHourBaseUrl))
        {
            return ValidateOptionsResult.Fail(
                "Callback base URLs must be absolute HTTP(S) URLs without embedded credentials.");
        }

        if (options.BatchSize is < 1 or > 100
            || options.PollIntervalMilliseconds is < 100 or > 60000
            || options.LeaseDurationSeconds is < 5 or > 600
            || options.RequestTimeoutSeconds is < 1 or > 60
            || options.MaxRetries is < 0 or > 10
            || options.BaseRetryDelayMilliseconds is < 10 or > 60000
            || options.MaxRetryDelayMilliseconds < options.BaseRetryDelayMilliseconds
            || options.MaxRetryDelayMilliseconds > 300000
            || options.MaxJitterMilliseconds is < 0 or > 10000
            || options.CircuitFailureThreshold is < 1 or > 100
            || options.CircuitOpenSeconds is < 1 or > 3600
            || options.MockTokenLifetimeSeconds is < 60 or > 3600
            || options.MockTokenRefreshSkewSeconds < 0
            || options.MockTokenRefreshSkewSeconds >= options.MockTokenLifetimeSeconds)
        {
            return ValidateOptionsResult.Fail("Callback delivery timing, retry or circuit settings are outside safe bounds.");
        }

        bool invalidIdentity = options.CurrentGoldenHourIdentities.Values.Any(identity =>
            identity.CallId <= 0
            || identity.ReservationId <= 0
            || identity.OrderId <= 0
            || identity.CustomerId <= 0);
        return invalidIdentity
            ? ValidateOptionsResult.Fail("Current Golden Hour identity values must all be positive.")
            : ValidateOptionsResult.Success;
    }

    private static bool IsSafeBaseUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out Uri? uri)
        && uri.Scheme is "http" or "https"
        && string.IsNullOrEmpty(uri.UserInfo);
}
