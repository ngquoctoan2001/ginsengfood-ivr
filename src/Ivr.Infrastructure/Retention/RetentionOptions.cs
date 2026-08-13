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

    public int BatchSize { get; set; } = 250;

    public int NotConfiguredAlertAfterDays { get; set; } = 7;

    public string[] DataClasses { get; set; } = [];

    public Dictionary<string, int?> PeriodDays { get; set; } =
        new(StringComparer.Ordinal);
}
