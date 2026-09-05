using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.DevTooling;

/// <summary>
/// Where the non-production developer surface reads its fixtures from (UI-07, W-0112).
/// </summary>
public sealed class DevToolingOptions
{
    public const string SectionName = "Ivr:DevTooling";

    /// <summary>
    /// Directory holding <c>seed/*.sample.json</c>. Empty by default, which disables the surface:
    /// a dev tool that guesses at a path is a dev tool that one day guesses at the wrong one.
    /// </summary>
    public string SeedDirectory { get; set; } = string.Empty;

    /// <summary>Simulated confirmation window for a scenario replay, in seconds.</summary>
    public int ScenarioWindowSeconds { get; set; } = 300;

    /// <summary>Technical retry limit used by a scenario replay.</summary>
    public int ScenarioTechnicalRetryLimit { get; set; } = 1;

    /// <summary>Upper bound on tasks a single seed load will admit.</summary>
    public int MaximumSeedTasks { get; set; } = 200;
}

public sealed class DevToolingOptionsValidator : IValidateOptions<DevToolingOptions>
{
    private static readonly string[] RequiredSeedFiles =
    [
        SeedCatalog.TaskFileName,
        SeedCatalog.ScenarioFileName,
        SeedCatalog.IntegrationStatusFileName,
    ];

    public ValidateOptionsResult Validate(string? name, DevToolingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        List<string> failures = [];
        if (!string.IsNullOrWhiteSpace(options.SeedDirectory))
        {
            if (!Directory.Exists(options.SeedDirectory))
            {
                failures.Add(
                    $"Ivr:DevTooling:SeedDirectory '{options.SeedDirectory}' does not exist. "
                    + "Restore the repository seed/ folder or configure its absolute path.");
            }
            else
            {
                foreach (string fileName in RequiredSeedFiles)
                {
                    if (!File.Exists(Path.Combine(options.SeedDirectory, fileName)))
                    {
                        failures.Add(
                            $"Ivr:DevTooling:SeedDirectory is missing required file "
                            + $"'{fileName}'. Restore seed/{fileName} before starting the API.");
                    }
                }
            }
        }

        if (options.ScenarioWindowSeconds is < 1 or > 86_400)
        {
            failures.Add("Ivr:DevTooling:ScenarioWindowSeconds must be between 1 and 86400.");
        }

        if (options.ScenarioTechnicalRetryLimit is < 0 or > 10)
        {
            failures.Add("Ivr:DevTooling:ScenarioTechnicalRetryLimit must be between 0 and 10.");
        }

        if (options.MaximumSeedTasks is < 1 or > 5_000)
        {
            failures.Add("Ivr:DevTooling:MaximumSeedTasks must be between 1 and 5000.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
