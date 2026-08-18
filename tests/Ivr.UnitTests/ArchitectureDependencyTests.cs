using System.Xml.Linq;

namespace Ivr.UnitTests;

public sealed class ArchitectureDependencyTests
{
    [Fact]
    [Trait("TestId", "UT-BOOT-03")]
    public void DomainAssemblyDoesNotReferenceInfrastructure()
    {
        string repositoryRoot = FindRepositoryRoot();
        Dictionary<string, string[]> approvedReferences = new(StringComparer.Ordinal)
        {
            ["Ivr.Domain"] = [],
            ["Ivr.Contracts"] = [],
            ["Ivr.Infrastructure"] = ["Ivr.Contracts", "Ivr.Domain"],
            ["Ivr.Api"] = ["Ivr.Contracts", "Ivr.Infrastructure"],
            ["Ivr.Worker"] = ["Ivr.Contracts", "Ivr.Infrastructure"],
        };
        string[] projectFiles = Directory.GetFiles(
            Path.Combine(repositoryRoot, "src"),
            "*.csproj",
            SearchOption.AllDirectories);

        Assert.Equal(
            approvedReferences.Keys.Order(StringComparer.Ordinal),
            projectFiles.Select(Path.GetFileNameWithoutExtension).Order(StringComparer.Ordinal));

        foreach (string projectFile in projectFiles)
        {
            string projectName = Path.GetFileNameWithoutExtension(projectFile);
            string projectDirectory = Path.GetDirectoryName(projectFile)!;
            XDocument project = XDocument.Load(projectFile);
            string[] actualReferences = project
                .Descendants("ProjectReference")
                .Select(reference => reference.Attribute("Include")?.Value)
                .Where(include => !string.IsNullOrWhiteSpace(include))
                .Select(include => GetReferencedProjectName(include!, projectDirectory))
                .Order(StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(
                approvedReferences[projectName].Order(StringComparer.Ordinal),
                actualReferences);
        }
    }

    [Fact]
    [Trait("TestId", "UT-ARCH-NO-OPS-EGRESS-05")]
    public void OutboundHttpSurfaceIsOnlySalesCallbackAndCarriesNoOpsCredential()
    {
        // P4-2 §2.6 and D-02: IVR consumes Sales-owned blocker evidence but must never become a
        // second Ops orchestrator. That invariant is only real if adding an Ops client, webhook
        // or credential fails a test — otherwise it is a sentence in a document.
        string repositoryRoot = FindRepositoryRoot();
        string[] sourceFiles = Directory.GetFiles(
            Path.Combine(repositoryRoot, "src"),
            "*.cs",
            SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();

        // The complete outbound HTTP surface. Both entries are the Sales callback: Target V1 and
        // the pinned current Golden Hour compatibility path. Nothing else may dial out.
        string[] approvedHttpClients =
        [
            "ICurrentGoldenHourCallbackClient, CurrentGoldenHourCallbackClient",
            "ITargetV1CallbackTransport, TargetV1CallbackTransport",
        ];
        List<string> registeredHttpClients = [];
        foreach (string file in sourceFiles)
        {
            foreach (string line in File.ReadLines(file))
            {
                int marker = line.IndexOf("AddHttpClient<", StringComparison.Ordinal);
                if (marker < 0)
                {
                    continue;
                }

                string tail = line[(marker + "AddHttpClient<".Length)..];
                int close = tail.IndexOf('>', StringComparison.Ordinal);
                registeredHttpClients.Add(close < 0 ? tail.Trim() : tail[..close].Trim());
            }
        }

        Assert.Equal(
            approvedHttpClients,
            registeredHttpClients.Order(StringComparer.Ordinal).ToArray());

        // Base URLs and service credentials the code may read. An Ops endpoint or Ops token
        // would need a new key here, so the allowlist is what makes the boundary enforceable.
        string[] approvedExternalConfigurationKeys =
        [
            "CurrentGoldenHourBaseUrl",
            "CurrentGoldenHourInternalToken",
            "IVR_INTERNAL_SERVICE_TOKEN",
            "ORDER_CORE_SERVICE_TOKEN",
            "TargetBaseUrl",
        ];
        string[] forbiddenEgressMarkers =
        [
            "OpsBaseUrl",
            "OpsClient",
            "OpsWebhook",
            "OperationsBaseUrl",
            "OPS_BASE_URL",
            "OPS_SERVICE_TOKEN",
            "OPS_WEBHOOK_URL",
        ];
        foreach (string file in sourceFiles)
        {
            string content = File.ReadAllText(file);
            foreach (string marker in forbiddenEgressMarkers)
            {
                Assert.DoesNotContain(marker, content, StringComparison.Ordinal);
            }
        }

        // Guards the allowlist itself: if a base-URL/token property is renamed or added, this
        // assertion fails and forces the reviewer back to the boundary decision.
        string optionsFile = Path.Combine(
            repositoryRoot,
            "src",
            "Ivr.Infrastructure",
            "Callbacks",
            "CallbackDeliveryOptions.cs");
        string optionsContent = File.ReadAllText(optionsFile);
        foreach (string key in approvedExternalConfigurationKeys.Where(
            key => !key.Contains('_', StringComparison.Ordinal)))
        {
            Assert.Contains(key, optionsContent, StringComparison.Ordinal);
        }
    }

    [Fact]
    [Trait("TestId", "UT-ARCH-NO-CRM-EGRESS-06")]
    public void NoCrmConsentMutationOrNotificationEgressExists()
    {
        // P4-3 §2.5 and §3: IVR consumes the voice-restriction and trust evidence Sales supplies.
        // It never writes back to CRM, never mutates consent, and publishes no customer-facing
        // notification in V1. The HTTP allowlist in UT-ARCH-NO-OPS-EGRESS-05 already bounds who
        // IVR can talk to; this bounds what it could be built to say.
        string repositoryRoot = FindRepositoryRoot();
        string[] sourceFiles = Directory.GetFiles(
            Path.Combine(repositoryRoot, "src"),
            "*.cs",
            SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();

        // Deliberately names of things that would have to be *written* to break the boundary,
        // not loose words: "consent" and "notification" appear legitimately in policy text and
        // in the v1NotificationEnabled kill-guard, so matching those would fail for the wrong
        // reason and get the guard deleted rather than respected.
        string[] forbiddenSymbols =
        [
            "CrmClient",
            "ICrmClient",
            "CrmBaseUrl",
            "CRM_BASE_URL",
            "UpdateConsent",
            "MutateConsent",
            "WriteConsent",
            "SetMarketingConsent",
            "SendSms",
            "SendNotification",
            "NotificationPublisher",
            "PublishNotification",
            "SendCustomerMessage",
        ];

        foreach (string file in sourceFiles)
        {
            string content = File.ReadAllText(file);
            foreach (string symbol in forbiddenSymbols)
            {
                Assert.DoesNotContain(symbol, content, StringComparison.Ordinal);
            }
        }
    }

    [Theory]
    [Trait("TestId", "UT-BOOT-03-LINUX-PATH")]
    [InlineData(@"..\Ivr.Contracts\Ivr.Contracts.csproj")]
    [InlineData("../Ivr.Contracts/Ivr.Contracts.csproj")]
    public void ProjectReferenceNameSupportsWindowsAndUnixSeparators(string include)
    {
        ArgumentNullException.ThrowIfNull(include);
        string projectDirectory = Path.Combine(Path.GetTempPath(), "Ivr.Api");

        Assert.Equal("Ivr.Contracts", GetReferencedProjectName(include, projectDirectory));
    }

    private static string GetReferencedProjectName(string include, string projectDirectory)
    {
        string platformPath = include
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);

        return Path.GetFileNameWithoutExtension(
            Path.GetFullPath(platformPath, projectDirectory));
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
}
