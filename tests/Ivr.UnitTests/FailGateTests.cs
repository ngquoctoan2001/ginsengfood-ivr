using System.Reflection;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Policies;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Auth;
using Ivr.Infrastructure.Callbacks;
using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests;

/// <summary>
/// W-0035 / P5-1 §6.5. The eight fail gates of <c>specs/testing/08-acceptance-criteria.md</c>,
/// one test each.
/// <para>
/// These are deliberately structural wherever a structure can carry the guarantee. A runtime test
/// proves a gate held on the path it exercised; a structural test proves there is no path. For an
/// acceptance gate — the list someone reads before deciding whether this system may call real
/// customers — the second kind is what the question actually deserves.
/// </para>
/// </summary>
public sealed class FailGateTests
{
    private static readonly string[] RuntimeSourceRoots = ["src"];

    [Fact]
    [Trait("TestId", "IT-FAILGATE-01")]
    public void GateOneIvrNeverTransitionsOrderState()
    {
        // D-02. IVR receives order_state and echoes the version it saw; it owns no verb that
        // changes an order. Every value it can put on the wire is named for what CORE should do,
        // never for something IVR did — the vocabulary itself has no way to express a transition.
        Assert.All(
            Enum.GetNames<Ivr.Contracts.Generated.SalesTarget.V1.RecommendedCoreAction>(),
            name => Assert.StartsWith("CORE_", name, StringComparison.Ordinal));

        // No outbound or persistence symbol may be named as an order-state write.
        AssertNoSourceSymbol(
            "SetOrderState",
            "UpdateOrderState",
            "TransitionOrder",
            "ConfirmOrder(",
            "CancelOrder(");
    }

    [Fact]
    [Trait("TestId", "IT-FAILGATE-02")]
    public void GateTwoIvrNeverTouchesPayment()
    {
        // payment_method_snapshot is read to decide callability and nothing else. There must be
        // no charge, refund, authorisation or payment-provider surface anywhere in the runtime.
        AssertNoSourceSymbol(
            "PaymentClient",
            "ChargeAsync",
            "RefundAsync",
            "AuthorizePayment",
            "CapturePayment",
            "PaymentGateway");
    }

    [Fact]
    [Trait("TestId", "IT-FAILGATE-03")]
    public void GateThreeIvrCannotSendANotification()
    {
        // Proven structurally in UT-NOTIF-SURFACE-01/STORE-02; restated here because the gate
        // list is what a reviewer reads, and a gate that points at another file is a gate that
        // gets skipped.
        FeatureFlagSnapshot snapshot =
            FeatureFlagSnapshot.SafeDefault(FeatureFlagEnvironments.Lab);
        Assert.False(snapshot.V1NotificationEnabled);
        AssertNoSourceSymbol("SendSms", "SendNotification", "PublishNotification");
    }

    [Fact]
    [Trait("TestId", "IT-FAILGATE-04")]
    public void GateFourNoCallIsPlacedOutsideTheAllowlistAndKillSwitch()
    {
        FeatureFlagSnapshot lab = FeatureFlagSnapshot.SafeDefault(FeatureFlagEnvironments.Lab);

        // The safe default is the restrictive one: no real customer calls, kill switch engaged,
        // allowlist empty. A fresh environment cannot dial anyone before someone decides it may.
        Assert.False(lab.RealCustomerCallAllowed);
        Assert.True(lab.GlobalDialKillSwitch);
        Assert.Empty(lab.LabDestinationAllowlist);

        // Enabling real customer calls is not an ordinary admin mutation.
        Assert.Throws<Ivr.Domain.Errors.IvrFailureException>(() =>
            FeatureFlagGuardrails.ValidateAdminMutation(
                lab,
                lab with { RealCustomerCallAllowed = true }));
    }

    [Fact]
    [Trait("TestId", "IT-FAILGATE-05")]
    public void GateFiveRawPhoneAndFullAddressCannotBeStoredOrLogged()
    {
        // The masked-phone column constraint and the PII guard are the two independent stops.
        Assert.False(PiiGuard.IsSafeText("Khách 0912345678 xác nhận"));
        Assert.True(PiiGuard.IsSafeText("Khách 84xxxxx5678 xác nhận"));

        // A semantic full address is rejected even when it carries no digits at all, which is
        // the case a pattern-only check would wave through.
        Assert.Throws<InvalidOperationException>(() =>
            ShortDeliveryArea.Create("Đường Nguyễn Huệ, Phường Bến Nghé"));
        Assert.Equal(
            "Phường Bến Nghé, Quận Một",
            ShortDeliveryArea.Create("Phường Bến Nghé, Quận Một").Value);
    }

    [Fact]
    [Trait("TestId", "IT-FAILGATE-06")]
    public void GateSixATechnicalFailureIsNeverCountedAsNoAnswer()
    {
        DateTimeOffset now = new(2026, 8, 18, 6, 0, 0, TimeSpan.Zero);
        var context = new AttemptNormalizationContext(
            AttemptNumber: 1,
            MaxAttempts: 2,
            OccurredAt: now,
            ConfirmationWindowExpiresAt: now.AddMinutes(5),
            PriorTechnicalRetryCount: 0,
            TechnicalRetryLimit: 2);

        foreach (Ivr.Domain.Ports.SimProviderDisposition technical in new[]
        {
            Ivr.Domain.Ports.SimProviderDisposition.Dropped,
            Ivr.Domain.Ports.SimProviderDisposition.NetworkError,
            Ivr.Domain.Ports.SimProviderDisposition.SimError,
            Ivr.Domain.Ports.SimProviderDisposition.AudioError,
            Ivr.Domain.Ports.SimProviderDisposition.DtmfError,
        })
        {
            NormalizedResult result = DispositionMapper.Normalize(technical, null, "E_TEST", context);

            // DT-02. A technical fault says nothing about whether the customer was reachable,
            // so counting it as an attempt would spend a customer's limited attempts on our bug.
            Assert.Equal(IvrResultType.IvrTechnicalException, result.ResultType);
            Assert.False(result.IsCounted);
            Assert.False(result.IsNoAnswer);
        }
    }

    [Fact]
    [Trait("TestId", "IT-FAILGATE-07")]
    public void GateSevenNoCandidateValueIsHardCodedAsProductionTruth()
    {
        // The attempt policy is data, and the candidate is labelled as candidate in the type
        // system rather than in a comment.
        Assert.All(
            CandidateAttemptPolicies.Create(),
            policy => Assert.Equal(
                AttemptPolicyApproval.CandidateMockLabOnly,
                policy.Approval));

        // Two other candidates that must refuse to boot rather than be treated as approved.
        var identity = new ServiceIdentityOptionsValidator();
        Assert.True(identity.Validate(
            null,
            new ServiceIdentityOptions { Mode = ServiceIdentityMode.Real }).Failed);

        var callback = new CallbackDeliveryOptionsValidator();
        Assert.True(callback.Validate(
            null,
            new CallbackDeliveryOptions
            {
                Enabled = true,
                Provider = Ivr.Contracts.Sales.SalesProviderNames.TargetV1,
            }).Failed);
    }

    [Fact]
    [Trait("TestId", "IT-FAILGATE-08")]
    public async Task GateEightNoReadinessIsClaimedWhileAGateLacksEvidence()
    {
        // Dependency probing does not exist yet, and the surface says so rather than reporting
        // a cheerful default. The alternative — cards that read UP because nothing checked —
        // is exactly the readiness claim this gate forbids.
        var options = Options.Create(new ServiceIdentityOptions());
        using var issuer = new MockOidcIssuer(TimeProvider.System, options);
        var validator = new ServiceJwtValidator(issuer, TimeProvider.System, options);

        issuer.SimulateKeySourceOutage(true);
        ServiceIdentityResult duringOutage = await validator.ValidateAsync(
            issuer.Issue("probe", [ServiceIdentityScopes.TaskWrite]),
            ServiceIdentityScopes.TaskWrite,
            CancellationToken.None);

        Assert.False(duringOutage.Succeeded);
        Assert.Equal(ServiceIdentityFailure.KeySourceUnavailable, duringOutage.Failure);
    }

    [Fact]
    [Trait("TestId", "UT-TRACE-01")]
    public void TheTraceabilityTableMatchesTheSuiteItClaimsToDescribe()
    {
        // W-0035 / P5-1 §6.4. A traceability table that drifts is worse than no table: it reads
        // as coverage that is no longer there. Generating it is only half the answer — this
        // asserts the committed file still describes the suite, so a renamed or deleted test
        // fails here instead of quietly leaving a stale row behind.
        string root = FindRepositoryRoot();
        string table = File.ReadAllText(Path.Combine(root, "docs", "traceability-tests.md"));

        string[] taggedIds = Directory
            .GetFiles(Path.Combine(root, "tests"), "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(File.ReadAllLines)
            .Select(line => System.Text.RegularExpressions.Regex.Match(
                line,
                """\[Trait\("TestId",\s*"([^"]+)"\)\]"""))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(taggedIds);

        // Match a table ROW, not any mention. The generator's own header names one test id in
        // prose; accepting a prose mention would let that one id pass without a row behind it.
        string[] rows = table
            .Split('\n')
            .Where(line => line.StartsWith("| `", StringComparison.Ordinal))
            .ToArray();
        foreach (string testId in taggedIds)
        {
            Assert.Contains(
                rows,
                row => row.StartsWith($"| `{testId}` |", StringComparison.Ordinal));
        }
    }

    /// <summary>
    /// Scans the runtime source for symbols that must not exist. Names, not loose words: a gate
    /// that fails on a comment gets deleted rather than respected.
    /// </summary>
    private static void AssertNoSourceSymbol(params string[] forbidden)
    {
        string root = FindRepositoryRoot();
        foreach (string relative in RuntimeSourceRoots)
        {
            foreach (string file in Directory.GetFiles(
                Path.Combine(root, relative),
                "*.cs",
                SearchOption.AllDirectories))
            {
                if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                    || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                {
                    continue;
                }

                string content = File.ReadAllText(file);
                foreach (string symbol in forbidden)
                {
                    Assert.DoesNotContain(symbol, content, StringComparison.Ordinal);
                }
            }
        }
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Ivr.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("The repository root was not found.");
    }
}
