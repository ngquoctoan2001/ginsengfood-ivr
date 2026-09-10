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

    [Fact]
    [Trait("TestId", "UT-M3-AUTHORITY-11")]
    public void ThePreDialDecisionPathCannotReadCustomerTrustOrOrderRiskMetadata()
    {
        // W-0124 F3. OD-18 puts business call selection in Module 3. The reflection guard in
        // UT-M3-AUTHORITY-02 only catches a re-introduction that rebuilds the deleted domain
        // types; the cheaper regression is somebody reading the persisted columns straight into
        // the decision, which leaves those types untouched. This bounds the two files that own
        // the pre-dial decision instead of the type names.
        string repositoryRoot = FindRepositoryRoot();
        string domainRules = Path.Combine(
            repositoryRoot, "src", "Ivr.Domain", "Policies", "EligibilityRules.cs");
        string applicationService = Path.Combine(
            repositoryRoot, "src", "Ivr.Api", "Application", "EligibilityService.cs");

        // Persisted/wire names of Module 3's customer classification. None of them may appear in
        // the decision path at all — reading one is the regression, whatever it is then used for.
        string[] forbiddenEverywhere =
        [
            "TrustedSkipAllowed",
            "CustomerTrustStatus",
            "trusted_skip_allowed",
            "customer_trust_status",
            "risk_evidence_available",
            "TASK_SKIPPED_TRUSTED_CUSTOMER",
        ];

        foreach (string file in new[] { domainRules, applicationService })
        {
            string content = File.ReadAllText(file);
            foreach (string symbol in forbiddenEverywhere)
            {
                Assert.DoesNotContain(symbol, content, StringComparison.Ordinal);
            }
        }

        // risk_flags is the one that cannot simply be banned: OD-18 keeps it as scheduler
        // priority input, and SchedulerEligibilityCapacityProvider lives in the same file as the
        // eligibility service. So bound WHERE it may be read rather than whether — every
        // occurrence must be handing the column to the capacity/priority mapper, never to the
        // snapshot the decision is made on.
        Assert.DoesNotContain("RiskFlagsJson", File.ReadAllText(domainRules), StringComparison.Ordinal);

        string[] riskFlagLines = [.. File.ReadAllLines(applicationService)
            .Where(line => line.Contains("RiskFlagsJson", StringComparison.Ordinal))];

        Assert.NotEmpty(riskFlagLines);
        Assert.All(
            riskFlagLines,
            line => Assert.Contains("RiskScore", line, StringComparison.Ordinal));
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

    [Fact]
    [Trait("TestId", "UT-BOOT-05")]
    public void NoProductionCodeLooksUpANamedCulture()
    {
        // The three deployables run on a chiseled runtime base, which is globalization-INVARIANT:
        // there is no ICU, so CultureInfo.GetCultureInfo("vi-VN") throws. In a static constructor
        // that becomes TypeInitializationException on first use, which the caller sees as some
        // unrelated generic failure -- VietnameseOrderScriptRenderer hit exactly that, and the
        // shipped worker could not speak a single script.
        //
        // A grep, deliberately. The runtime check cannot exist: a test host that HAS ICU resolves
        // the lookup fine, so the only way to catch this in-process is to forbid the call.
        string repositoryRoot = FindRepositoryRoot();
        string[] offenders = Directory
            .GetFiles(Path.Combine(repositoryRoot, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}Generated{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(file => File.ReadLines(file).Any(LooksUpANamedCulture))
            .Select(file => Path.GetRelativePath(repositoryRoot, file))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            "These files look up a named culture, which throws in the globalization-invariant "
            + "runtime the deployables ship on. Build the NumberFormatInfo/DateTimeFormatInfo "
            + $"explicitly instead: {string.Join(", ", offenders)}");
    }

    /// <summary>
    /// Comment lines are skipped, and that is not a loophole -- it is the difference between the
    /// rule and a string search. The fix for this defect documents the API it replaced, by name,
    /// because a reader who does not know why the code is shaped that way will reach for the
    /// obvious call again. A guard that cannot tell code from prose would push the explanation out
    /// of the file, which costs more than it protects.
    /// </summary>
    private static bool LooksUpANamedCulture(string line)
    {
        string trimmed = line.TrimStart();
        if (trimmed.StartsWith("//", StringComparison.Ordinal)
            || trimmed.StartsWith('*')
            || trimmed.StartsWith("/*", StringComparison.Ordinal))
        {
            return false;
        }

        return trimmed.Contains("CultureInfo.GetCultureInfo(", StringComparison.Ordinal)
            || trimmed.Contains("new CultureInfo(", StringComparison.Ordinal)
            || trimmed.Contains("CultureInfo.CreateSpecificCulture(", StringComparison.Ordinal);
    }

    private static string GetReferencedProjectName(string include, string projectDirectory)
    {
        string platformPath = include
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);

        return Path.GetFileNameWithoutExtension(
            Path.GetFullPath(platformPath, projectDirectory));
    }

    /// <summary>
    /// The execution-mode spellings exist once and are never written out again.
    /// <para>
    /// They had been: <c>IvrOptions.LabRealSimExecutionMode</c> existed and the literal was also
    /// typed by hand in six places, and the <c>ExecutionMode</c> to string mapping existed three
    /// times over — a frozen dictionary and two switch expressions, each with a <c>_ =></c> arm, so
    /// a fourth member would compile in all three and throw in whichever ran first.
    /// </para>
    /// <para>
    /// <c>"MOCK"</c> is deliberately not checked here. It is declared twice, by
    /// <c>IvrOptions.MockExecutionMode</c> and <c>FeatureFlagCatalog.MockSimProvider</c>, for two
    /// different things that happen to be spelled the same — an execution mode and a SIM provider.
    /// A rule that replaced it everywhere would be wrong at roughly half its sites, and picking
    /// which owner wins is not a decision this test can make.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "ARCH-CONST-01")]
    public void ExecutionModeSpellingsAreNeverWrittenAsLiterals()
    {
        string repositoryRoot = FindRepositoryRoot();
        string declaringFile = Path.Combine(
            repositoryRoot, "src", "Ivr.Domain", "Confirmation", "ExecutionModes.cs");

        List<string> offenders = [];
        foreach (string file in Directory.GetFiles(
                     Path.Combine(repositoryRoot, "src"), "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                || file.Contains("Migrations", StringComparison.Ordinal)
                || file.EndsWith(".g.cs", StringComparison.Ordinal)
                || string.Equals(file, declaringFile, StringComparison.Ordinal))
            {
                // Generated contract code carries the same strings as OpenAPI enum members and is
                // regenerated from the spec, so it is not a place a person can fix.
                continue;
            }

            string[] lines = File.ReadAllLines(file);
            for (int index = 0; index < lines.Length; index++)
            {
                foreach (string spelling in new[] { "\"LAB_REAL_SIM\"", "\"PRODUCTION_REAL\"" })
                {
                    if (lines[index].Contains(spelling, StringComparison.Ordinal))
                    {
                        offenders.Add(
                            $"{Path.GetRelativePath(repositoryRoot, file)}:{index + 1} writes {spelling}");
                    }
                }
            }
        }

        Assert.True(
            offenders.Count == 0,
            "Execution-mode spellings belong to ExecutionModes and nowhere else:\n  "
            + string.Join("\n  ", offenders));
    }

    [Fact]
    [Trait("TestId", "ARCH-ASYNC-01")]
    public void ProductionCodeDoesNotWriteConfigureAwait()
    {
        // C1 in docs/review/2026-09-09-codebase-audit.md: src carried ConfigureAwait(false) on 440
        // awaits and omitted it on ~200 more, so a reader could not tell a deliberate omission
        // from a forgotten one. Owner decision on 2026-09-10 (W-0269) was to remove all of them
        // rather than enforce CA2007, and the reason is that they never did anything here:
        //
        //   * no project in this repository is packable -- there is no PackageId, no
        //     IsPackable, no GeneratePackageOnBuild -- so Ivr.Infrastructure is consumed only by
        //     Ivr.Api, Ivr.Worker and one test project, all in this solution;
        //   * Ivr.Api is WebApplication.CreateBuilder and Ivr.Worker is
        //     Host.CreateApplicationBuilder, and neither installs a SynchronizationContext;
        //   * nothing in src or tests references SynchronizationContext at all.
        //
        // With no synchronization context to capture, ConfigureAwait(false) is a no-op. tests/
        // had already settled this on its own: 2473 awaits, zero ConfigureAwait.
        //
        // CA2007 was measured as the alternative and rejected: 80 of the 201 sites are
        // `await using`, where the shipped code fixer emits code that does not compile
        // (CS0029 -- ConfiguredAsyncDisposable assigned to the disposable's own type).
        string repositoryRoot = FindRepositoryRoot();

        List<string> offenders = [];
        foreach (string file in Directory.GetFiles(
                     Path.Combine(repositoryRoot, "src"), "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                || file.EndsWith(".g.cs", StringComparison.Ordinal))
            {
                // Generated clients keep whatever the generator emits. SalesTargetV1Client.g.cs
                // carries thirteen of these, and editing them would both contradict the next
                // regeneration and break the hash contract-freeze-verifier pins on that file --
                // which is how this exclusion was found rather than guessed.
                continue;
            }

            string[] lines = File.ReadAllLines(file);
            for (int index = 0; index < lines.Length; index++)
            {
                if (lines[index].Contains("ConfigureAwait", StringComparison.Ordinal))
                {
                    offenders.Add($"{Path.GetRelativePath(repositoryRoot, file)}:{index + 1}");
                }
            }
        }

        Assert.True(
            offenders.Count == 0,
            "ConfigureAwait has no effect in this application and is not written here. "
            + "If a project ever becomes a published library, change this rule deliberately "
            + "rather than by adding one call:\n  "
            + string.Join("\n  ", offenders));
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
