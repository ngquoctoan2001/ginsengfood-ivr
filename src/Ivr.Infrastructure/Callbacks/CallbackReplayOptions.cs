using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Callbacks;

/// <summary>
/// Q-19 (PA2, 2026-09-26). How old a dead-lettered result callback may be and still be put back on
/// the outbox by an operator.
/// <para>
/// A replay sends the bytes of the first attempt under the same idempotency key, and it is safe
/// only while Module 3 still holds that key: past that, the replay is a new outcome to Module 3,
/// and an <c>IVR_CONFIRMED</c> from days ago would confirm an order whose state has moved on, with
/// nothing but Module 3's revalidation between it and the customer. The default is the seven days
/// Module 3 proposed for keeping the key (M3-10); Module 3 may change the number later, within
/// one to thirty days.
/// </para>
/// <para>
/// Bound by the API, which serves the replay route. The worker's <c>CallbackDeliveryOptions</c>
/// is not bound there, so it cannot carry this.
/// </para>
/// </summary>
public sealed class CallbackReplayOptions
{
    public const string SectionName = "Ivr:Callbacks:Replay";

    public const int MinimumMaxAgeDays = 1;

    public const int MaximumMaxAgeDays = 30;

    /// <summary>Days after the callback was created that a replay is still allowed.</summary>
    public int MaxAgeDays { get; set; } = 7;

    public TimeSpan MaxAge => TimeSpan.FromDays(MaxAgeDays);
}

public sealed class CallbackReplayOptionsValidator : IValidateOptions<CallbackReplayOptions>
{
    public ValidateOptionsResult Validate(string? name, CallbackReplayOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.MaxAgeDays is < CallbackReplayOptions.MinimumMaxAgeDays
            or > CallbackReplayOptions.MaximumMaxAgeDays
            ? ValidateOptionsResult.Fail(
                $"{CallbackReplayOptions.SectionName}:MaxAgeDays must be between "
                + $"{CallbackReplayOptions.MinimumMaxAgeDays} and {CallbackReplayOptions.MaximumMaxAgeDays}.")
            : ValidateOptionsResult.Success;
    }
}
