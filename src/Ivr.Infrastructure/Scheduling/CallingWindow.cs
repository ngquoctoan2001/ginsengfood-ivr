using System.Globalization;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Scheduling;

/// <summary>
/// W-0198 / <c>OD-V1-16</c>. The hours of day a customer may be telephoned.
/// <para>
/// Until this existed there was no such rule anywhere in the code. A task arriving at three in
/// the morning was dispatched at three in the morning, and nothing in the scheduler, the policy
/// registry or the database had an opinion about it. <c>D-10</c> settled how many times to call
/// and how far apart; it never said <em>when</em>, and the gap read as "always" rather than as a
/// missing decision. The owner signed <c>08:00–21:00</c> on 2026-09-05.
/// </para>
/// <para>
/// The offset is a fixed number rather than a time-zone id, and that is deliberate. Vietnam has
/// observed no daylight saving since 1975, so <c>+07:00</c> is exact all year; a fixed offset also
/// cannot fail on a container image that ships without a time-zone database, which is a failure
/// that would surface as "nobody was called today" rather than as an error. A deployment in
/// another country changes the number.
/// </para>
/// </summary>
public sealed class CallingWindowOptions
{
    public const string SectionName = "Ivr:Scheduler:CallingWindow";

    /// <summary>
    /// Off only for a deployment that has its own upstream control. Off means <b>no</b> hour
    /// restriction, so it is never the right setting for one that dials real customers.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Minutes east of UTC. Vietnam is <c>+07:00</c>, so 420.</summary>
    public int UtcOffsetMinutes { get; set; } = 420;

    /// <summary>First minute of the day a call may start, local. <c>08:00</c> is 480.</summary>
    public int StartMinuteOfLocalDay { get; set; } = 8 * 60;

    /// <summary>
    /// First minute of the day a call may <b>no longer</b> start, local. <c>21:00</c> is 1260, so
    /// 20:59 is inside the window and 21:00 is not.
    /// </summary>
    public int EndMinuteOfLocalDay { get; set; } = 21 * 60;
}

public sealed class CallingWindowOptionsValidator : IValidateOptions<CallingWindowOptions>
{
    public ValidateOptionsResult Validate(string? name, CallingWindowOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        List<string> failures = [];
        if (options.UtcOffsetMinutes is < -720 or > 840)
        {
            failures.Add($"{CallingWindowOptions.SectionName}:UtcOffsetMinutes must be between "
                + "-720 and 840.");
        }

        if (options.StartMinuteOfLocalDay is < 0 or > 1439)
        {
            failures.Add($"{CallingWindowOptions.SectionName}:StartMinuteOfLocalDay must be a "
                + "minute of the day.");
        }

        if (options.EndMinuteOfLocalDay is < 1 or > 1440)
        {
            failures.Add($"{CallingWindowOptions.SectionName}:EndMinuteOfLocalDay must be a "
                + "minute of the day.");
        }

        // An empty or inverted window would silently stop every call. Refusing at startup makes
        // that a deployment that does not start, rather than a night nobody was called.
        if (failures.Count == 0 && options.EndMinuteOfLocalDay <= options.StartMinuteOfLocalDay)
        {
            failures.Add($"{CallingWindowOptions.SectionName} must end after it starts; an empty "
                + "window would stop every call without saying so.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}

/// <summary>Whether calls may start now, and when the answer next changes.</summary>
/// <param name="Open">True when an attempt may be dispatched at this instant.</param>
/// <param name="LocalTime">The local wall-clock time the decision was made against.</param>
/// <param name="OpensAt">When the window next opens; equal to now when it is already open.</param>
public sealed record CallingWindowDecision(
    bool Open,
    TimeOnly LocalTime,
    DateTimeOffset OpensAt)
{
    public string Describe() => Open
        ? $"calling window open at {LocalTime.ToString("HH:mm", CultureInfo.InvariantCulture)} local"
        : $"outside the calling window at "
          + $"{LocalTime.ToString("HH:mm", CultureInfo.InvariantCulture)} local; opens "
          + OpensAt.ToString("u", CultureInfo.InvariantCulture);
}

/// <summary>
/// W-0198. Answers whether a call may start now, from configuration rather than from a constant.
/// </summary>
public sealed class CallingWindow(IOptions<CallingWindowOptions> options)
{
    private readonly CallingWindowOptions settings = options?.Value
        ?? throw new ArgumentNullException(nameof(options));

    public bool Enabled => settings.Enabled;

    public CallingWindowDecision Evaluate(DateTimeOffset utcNow)
    {
        DateTimeOffset local = utcNow.ToOffset(TimeSpan.FromMinutes(settings.UtcOffsetMinutes));
        int minuteOfDay = (local.Hour * 60) + local.Minute;
        TimeOnly localTime = TimeOnly.FromDateTime(local.DateTime);

        if (!settings.Enabled)
        {
            return new CallingWindowDecision(true, localTime, utcNow);
        }

        bool open = minuteOfDay >= settings.StartMinuteOfLocalDay
            && minuteOfDay < settings.EndMinuteOfLocalDay;
        if (open)
        {
            return new CallingWindowDecision(true, localTime, utcNow);
        }

        // Before the window opens today, or after it closed and therefore tomorrow.
        DateTimeOffset startOfLocalDay = new(
            local.Year, local.Month, local.Day, 0, 0, 0, local.Offset);
        DateTimeOffset opensAt = startOfLocalDay.AddMinutes(settings.StartMinuteOfLocalDay);
        if (minuteOfDay >= settings.EndMinuteOfLocalDay)
        {
            opensAt = opensAt.AddDays(1);
        }

        return new CallingWindowDecision(false, localTime, opensAt.ToUniversalTime());
    }
}
