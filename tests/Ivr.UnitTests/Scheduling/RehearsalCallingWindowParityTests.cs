using System.Text.Json;
using System.Text.RegularExpressions;

namespace Ivr.UnitTests.Scheduling;

/// <summary>
/// W-0354 / C15 (chief worklist 2026-09-25). Since W-0298 the calling window is enforced twice: the
/// API refuses a task at intake when none of its attempts lands inside calling hours, and the
/// worker declines to dial outside them. The rehearsal stacks widened only the worker, so outside
/// 08:00-21:08 Vietnam time every task was refused at the API before the worker could dial it - in
/// the e2e smoke, in the sandbox Module 3 rehearses against, and in the local soak profile. Each
/// stack is only as open as its narrower half, so the two halves are held equal here.
/// </summary>
public sealed partial class RehearsalCallingWindowParityTests
{
    private static readonly string[] Keys =
    [
        "Enabled",
        "UtcOffsetMinutes",
        "StartMinuteOfLocalDay",
        "EndMinuteOfLocalDay",
    ];

    [Theory]
    [InlineData("docker-compose.e2e.yml")]
    [InlineData("docker-compose.sandbox.yml")]
    [Trait("TestId", "UT-CALLWINDOW-PARITY-01")]
    public void ComposeOverlayWidensTheApiExactlyAsItWidensTheWorker(string file)
    {
        string text = File.ReadAllText(Path.Combine(FindRepositoryRoot(), file));

        Dictionary<string, string> api = CallingWindowOf(text, "ivr-api");
        Dictionary<string, string> worker = CallingWindowOf(text, "ivr-worker");

        Assert.Equal(Keys.Length, worker.Count);
        Assert.Equal(worker, api);
        Assert.Equal("true", api["Enabled"]);
        Assert.Equal("0", api["StartMinuteOfLocalDay"]);
        Assert.Equal("1440", api["EndMinuteOfLocalDay"]);
    }

    [Fact]
    [Trait("TestId", "UT-CALLWINDOW-PARITY-02")]
    public void LocalMockE2EProfileWidensTheApiExactlyAsItWidensTheWorker()
    {
        string root = FindRepositoryRoot();
        JsonElement api = WindowFromProfile(Path.Combine(
            root, "src", "Ivr.Api", "appsettings.Profile.LocalMockE2E.json"));
        JsonElement worker = WindowFromProfile(Path.Combine(
            root, "src", "Ivr.Worker", "appsettings.Profile.LocalMockE2E.json"));

        foreach (string key in Keys)
        {
            Assert.Equal(worker.GetProperty(key).ToString(), api.GetProperty(key).ToString());
        }

        Assert.True(api.GetProperty("Enabled").GetBoolean());
        Assert.Equal(0, api.GetProperty("StartMinuteOfLocalDay").GetInt32());
        Assert.Equal(1440, api.GetProperty("EndMinuteOfLocalDay").GetInt32());
    }

    /// <summary>
    /// The four <c>Ivr__Scheduler__CallingWindow__*</c> values under one service. The overlays are
    /// flat key/value environments, so reading the service's block line by line is exact without a
    /// YAML dependency the test project does not otherwise need.
    /// </summary>
    private static Dictionary<string, string> CallingWindowOf(string compose, string service)
    {
        var found = new Dictionary<string, string>(StringComparer.Ordinal);
        bool inside = false;
        foreach (string raw in compose.Split('\n'))
        {
            string line = raw.TrimEnd('\r');
            Match header = ServiceHeader().Match(line);
            if (header.Success)
            {
                inside = header.Groups[1].Value == service;
                continue;
            }

            if (!inside)
            {
                continue;
            }

            Match setting = CallingWindowSetting().Match(line);
            if (setting.Success)
            {
                found[setting.Groups[1].Value] = setting.Groups[2].Value;
            }
        }

        Assert.True(found.Count > 0, $"{service} sets no Ivr__Scheduler__CallingWindow__* value.");
        return found;
    }

    private static JsonElement WindowFromProfile(string path)
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement
            .GetProperty("Ivr")
            .GetProperty("Scheduler")
            .GetProperty("CallingWindow")
            .Clone();
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Ivr.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not find the IVR repository root.");
    }

    [GeneratedRegex(@"^  ([a-z][a-z0-9-]*):\s*$")]
    private static partial Regex ServiceHeader();

    [GeneratedRegex(@"^\s+Ivr__Scheduler__CallingWindow__([A-Za-z]+):\s*""?([^""\s]+)""?\s*$")]
    private static partial Regex CallingWindowSetting();
}
