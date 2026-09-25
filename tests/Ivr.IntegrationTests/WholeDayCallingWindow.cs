using Microsoft.AspNetCore.Hosting;

namespace Ivr.IntegrationTests;

/// <summary>
/// W-0356 / K-05 (the chief's C15, found again in the test hosts). Since W-0298 the API refuses at
/// intake a task none of whose attempts lands inside calling hours, and a host that keeps the
/// default 08:00-21:08 window therefore refuses every task a test posts with real timestamps after
/// 21:08 Vietnam time. <c>IT-API-MATRIX-38</c> and <c>IT-DEV-SEED-03/04</c> went red every night
/// for that reason alone.
/// <para>
/// The same answer as <c>docker-compose.e2e.yml</c>: the gate stays enabled and is widened to the
/// whole day, so the decision still runs and always says yes. Hosts that test the hour rule itself
/// pin their own clock and window instead of using this.
/// </para>
/// </summary>
internal static class WholeDayCallingWindow
{
    public static IReadOnlyDictionary<string, string?> Settings { get; } =
        new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["Ivr:Scheduler:CallingWindow:Enabled"] = "true",
            ["Ivr:Scheduler:CallingWindow:UtcOffsetMinutes"] = "420",
            ["Ivr:Scheduler:CallingWindow:StartMinuteOfLocalDay"] = "0",
            ["Ivr:Scheduler:CallingWindow:EndMinuteOfLocalDay"] = "1440",
        };

    public static void Apply(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        foreach ((string key, string? value) in Settings)
        {
            builder.UseSetting(key, value);
        }
    }
}
