namespace Ivr.Infrastructure.Retention;

/// <summary>
/// Configures the opt-in retention host and owner-supplied periods.
/// Missing periods deliberately leave the corresponding class unarmed.
/// </summary>
public sealed class RetentionOptions
{
    public const string SectionName = "Ivr:Retention";

    public bool Enabled { get; set; }

    public bool DryRun { get; set; } = true;

    /// <summary>
    /// W-0047 / P7-5. Run one pass and exit, so a CronJob pod terminates instead of hanging.
    /// <para>
    /// Without this the worker performs its retention pass and then stays alive, because the
    /// scheduler, normalisation and callback hosts keep the process running -- a CronJob pod that
    /// never exits is recorded as failed, which is exactly what happened when W-0044 first tried
    /// to schedule one.
    /// </para>
    /// </summary>
    public bool RunOnce { get; set; }

    public int BatchSize { get; set; } = 250;

    public int NotConfiguredAlertAfterDays { get; set; } = 7;

    public string[] DataClasses { get; set; } = [];

    public Dictionary<string, int?> PeriodDays { get; set; } =
        new(StringComparer.Ordinal);
}
