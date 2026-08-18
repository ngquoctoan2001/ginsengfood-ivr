using System.Text.Json;

namespace Ivr.ChaosTests;

/// <summary>
/// W-0042 / P6-3 §9 and §11. The blast-radius limit is only worth claiming if something enforces
/// it. A chaos config is one edited hostname away from pointing at a shared environment, and that
/// edit looks harmless in review.
/// </summary>
public sealed class BlastRadiusGuard
{
    [Fact]
    [Trait("TestId", "CHAOS-GUARD-05")]
    public void NoFaultInjectionTargetCanReachAnythingOutsideItsOwnThrowawayNetwork()
    {
        string path = Path.Combine(
            ChaosFixtures.FindRepositoryFile("deploy", "chaos", "toxiproxy.staging.json"));
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));

        JsonElement[] proxies = [.. document.RootElement.EnumerateArray()];
        Assert.NotEmpty(proxies);

        foreach (JsonElement proxy in proxies)
        {
            string upstream = proxy.GetProperty("upstream").GetString() ?? string.Empty;
            string host = upstream.Split(':')[0];

            // A container alias created for the run, or the loopback. Anything else is a name that
            // resolves somewhere a chaos run has no business reaching.
            Assert.True(
                host.StartsWith("chaos-", StringComparison.Ordinal)
                    || host is "127.0.0.1" or "localhost",
                $"Fault-injection upstream '{upstream}' names a host outside the throwaway "
                + "network. Chaos must not be able to reach a shared environment (P6-3 §11).");

            // Listening on the loopback only would be wrong here (the proxy must be reachable from
            // sibling containers) but the LISTEN side must never carry a routable upstream name.
            Assert.StartsWith("0.0.0.0:", proxy.GetProperty("listen").GetString(), StringComparison.Ordinal);
        }
    }
}
