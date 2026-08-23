using System.Reflection;
using Ivr.Domain.DevTooling;
using Ivr.Infrastructure.Configuration;

namespace Ivr.UnitTests.DevTooling;

/// <summary>
/// The non-production guard on the UI-07 developer surface (W-0112).
/// <para>
/// These assert the predicate rather than the HTTP response, because the predicate is what runs
/// twice — once when the routes are mapped and once inside the service. A test that only drove
/// the HTTP surface would prove the first barrier and say nothing about the second.
/// </para>
/// </summary>
public sealed class NonProductionSurfaceTests
{
    [Theory]
    [InlineData("Development", IvrOptions.MockExecutionMode)]
    [InlineData("Testing", IvrOptions.MockExecutionMode)]
    [InlineData("Staging", IvrOptions.MockExecutionMode)]
    [InlineData("Lab", IvrOptions.LabRealSimExecutionMode)]
    [Trait("TestId", "UT-DEVGUARD-01")]
    public void ANonProductionDeploymentMayServeTheSurface(string environment, string mode)
    {
        Assert.True(NonProductionSurface.IsAvailable(environment, mode, false));
    }

    [Fact]
    [Trait("TestId", "UT-DEVGUARD-02")]
    public void ProductionIsRefusedByEveryOneOfTheThreeInputsOnItsOwn()
    {
        // The environment says production.
        Assert.False(NonProductionSurface.IsAvailable(
            "Production",
            IvrOptions.MockExecutionMode,
            false));

        // The environment is labelled Staging but the mode is the production one. This is the
        // case a label-only check would admit, and it is the realistic one: a staging deployment
        // pointed at PRODUCTION_REAL is dialling the same phone network as production.
        Assert.False(NonProductionSurface.IsAvailable(
            "Staging",
            IvrOptions.ProductionRealExecutionMode,
            false));

        // Label and mode both look safe, but the deployment says it is calling real customers.
        Assert.False(NonProductionSurface.IsAvailable(
            "Staging",
            IvrOptions.MockExecutionMode,
            true));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("prod")]
    [InlineData("production")]
    [InlineData("Prod-EU")]
    [InlineData("Sandbox")]
    [Trait("TestId", "UT-DEVGUARD-03")]
    public void AnEnvironmentNameThatIsNotOnTheAllowlistIsRefused(string? environment)
    {
        Assert.False(NonProductionSurface.IsAvailable(
            environment,
            IvrOptions.MockExecutionMode,
            false));
    }

    /// <summary>
    /// The allowlist direction is the whole point, so it is asserted rather than assumed.
    /// "Sandbox" above is a plausible future environment name; the answer being <c>false</c> is
    /// what makes forgetting to update the list produce a missing dev tool rather than a seed
    /// loader pointed at whatever that deployment turns out to be.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-DEVGUARD-04")]
    public void AnUnknownExecutionModeIsRefusedRatherThanAssumedSafe()
    {
        Assert.False(NonProductionSurface.IsAvailable("Development", "SOMETHING_NEW", false));
        Assert.False(NonProductionSurface.IsAvailable("Development", null, false));
        Assert.False(NonProductionSurface.IsAvailable("Development", "  ", false));
    }
}

/// <summary>
/// Replaying a seed scenario through the disposition mapper (W-0112).
/// </summary>
public sealed class CallScenarioDryRunTests
{
    private static readonly DateTimeOffset WindowStart =
        new(2026, 8, 23, 3, 0, 0, TimeSpan.Zero);

    private static readonly TimeSpan Window = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The claim this work item makes about dry runs — that they cannot place a call — is
    /// structural, so it is asserted structurally. <see cref="CallScenarioDryRun"/> lives in
    /// <c>Ivr.Domain</c>, which <c>ArchitectureDependencyTests</c> already pins as referencing no
    /// infrastructure; this narrows that to the type itself and to the port it would have to hold
    /// to dial. A behavioural test could only show that this particular input placed no call.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-DRYRUN-01")]
    public void TheReplayEngineHoldsNoPortItCouldDialThrough()
    {
        Type engine = typeof(CallScenarioDryRun);
        Assert.True(engine.IsAbstract && engine.IsSealed, "The engine must be static.");
        Assert.Empty(engine.GetFields(BindingFlags.Instance | BindingFlags.NonPublic));

        // Nothing it touches may name a gateway. ISimGateway is the only way to reach a network.
        foreach (MethodInfo method in engine.GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                Assert.DoesNotContain(
                    "Gateway",
                    parameter.ParameterType.Name,
                    StringComparison.Ordinal);
            }
        }

        Assert.DoesNotContain(
            engine.GetFields(BindingFlags.Static | BindingFlags.NonPublic),
            field => field.FieldType.Name.Contains("Gateway", StringComparison.Ordinal));
    }

    [Fact]
    [Trait("TestId", "UT-DRYRUN-02")]
    public void AnAnsweredKeyOneReplaysAsAConfirmedCountedResult()
    {
        ScenarioDryRunReport report = Run(new ScenarioDefinition(
            "SCN-001-confirm",
            "TASK-TARGET-GH-0001",
            [new ScenarioAttempt(1, "answered", "1")],
            "IVR_CONFIRMED",
            true));

        Assert.Equal(ScenarioCoverage.Replayed, report.Coverage);
        Assert.Equal("IVR_CONFIRMED", report.ActualResultType);
        Assert.True(report.ActualCounted);
        Assert.True(report.Matches);
        Assert.Empty(report.Notes);
    }

    /// <summary>
    /// Two ring timeouts, and the second one is the one that ends the job. The intermediate
    /// attempt is reported as well, because a rehearsal that only shows the verdict cannot show
    /// an operator why a customer was called twice.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-DRYRUN-03")]
    public void TheLastAttemptDecidesTheResultAndTheEarlierOnesStayVisible()
    {
        ScenarioDryRunReport report = Run(new ScenarioDefinition(
            "SCN-003-no-answer-final",
            "TASK-TARGET-247-0001",
            [
                new ScenarioAttempt(1, "ring_timeout", null),
                new ScenarioAttempt(2, "ring_timeout", null),
            ],
            "IVR_NO_ANSWER_FINAL",
            true));

        Assert.True(report.Matches);
        Assert.Equal(2, report.Attempts.Count);
        Assert.Equal("IVR_NO_ANSWER_ATTEMPT", report.Attempts[0].ResultType);
        Assert.False(report.Attempts[0].Final);
        Assert.Equal("IVR_NO_ANSWER_FINAL", report.Attempts[1].ResultType);
        Assert.True(report.Attempts[1].Final);
    }

    /// <summary>
    /// <c>SCN-007</c> expects <c>IVR_CONFIRMATION_WINDOW_EXPIRED</c> from a single ring timeout.
    /// The disposition mapper never returns that type — the expiry sweep does. Reporting a
    /// mismatch would send whoever reads it looking for a bug in the mapper, so the engine says
    /// out-of-scope instead and offers no verdict at all.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-DRYRUN-04")]
    public void AResultTheMapperCannotProduceIsOutOfScopeRatherThanAMismatch()
    {
        ScenarioDryRunReport report = Run(new ScenarioDefinition(
            "SCN-007-window-expired",
            "TASK-TARGET-247-0002",
            [new ScenarioAttempt(1, "ring_timeout", null)],
            "IVR_CONFIRMATION_WINDOW_EXPIRED",
            true));

        Assert.Equal(ScenarioCoverage.NotReplayable, report.Coverage);
        Assert.Null(report.Matches);
        Assert.Null(report.ActualResultType);
        Assert.Contains(
            report.Notes,
            note => note.Contains("confirmation-window sweep", StringComparison.Ordinal));
    }

    [Fact]
    [Trait("TestId", "UT-DRYRUN-05")]
    public void AScenarioWithNoAttemptsIsOutOfScopeRatherThanSilentlyPassing()
    {
        ScenarioDryRunReport report = Run(new ScenarioDefinition(
            "SCN-008-operational-block-recall",
            "TASK-TARGET-GH-0002",
            [],
            "IVR_OPERATIONAL_BLOCKED",
            null));

        Assert.Equal(ScenarioCoverage.NotReplayable, report.Coverage);
        Assert.Null(report.Matches);
    }

    /// <summary>
    /// A typo in the seed file must not replay as a technical exception. The mapper turns a null
    /// disposition into exactly that, so without an explicit vocabulary a misspelled status would
    /// make <c>SCN-006</c> pass while proving nothing.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-DRYRUN-06")]
    public void AnUnknownRawCallStatusIsRefusedRatherThanTreatedAsTechnical()
    {
        ScenarioDryRunReport report = Run(new ScenarioDefinition(
            "SCN-XXX-typo",
            null,
            [new ScenarioAttempt(1, "sim_eror", null)],
            "IVR_TECHNICAL_EXCEPTION",
            false));

        Assert.Equal(ScenarioCoverage.NotReplayable, report.Coverage);
        Assert.Contains(
            report.Notes,
            note => note.Contains("not a provider disposition", StringComparison.Ordinal));
    }

    /// <summary>
    /// A disagreement between the seed file and the mapper is reported as a mismatch with both
    /// sides named. This is the case the runner exists for: it is how a scenario that no longer
    /// describes the system gets noticed before an acceptance session runs on it.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-DRYRUN-07")]
    public void ADisagreementIsReportedWithBothSidesNamed()
    {
        ScenarioDryRunReport report = Run(new ScenarioDefinition(
            "SCN-999-wrong",
            null,
            [new ScenarioAttempt(1, "answered", "0")],
            "IVR_CONFIRMED",
            true));

        Assert.Equal(ScenarioCoverage.Replayed, report.Coverage);
        Assert.False(report.Matches);
        Assert.Equal("IVR_CUSTOMER_CANCELLED", report.ActualResultType);
        Assert.Contains(
            report.Notes,
            note => note.Contains("IVR_CONFIRMED", StringComparison.Ordinal)
                && note.Contains("IVR_CUSTOMER_CANCELLED", StringComparison.Ordinal));
    }

    /// <summary>
    /// The counted flag is compared separately from the result type. A scenario can name the
    /// right result and the wrong attempt-budget effect, and that combination is the one that
    /// quietly charges a customer for a call they never answered.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-DRYRUN-08")]
    public void TheCountedFlagIsComparedOnItsOwn()
    {
        ScenarioDryRunReport wrongCount = Run(new ScenarioDefinition(
            "SCN-998-count",
            null,
            [new ScenarioAttempt(1, "unreachable", null)],
            "IVR_INVALID_PHONE_FINAL",
            true));

        Assert.False(wrongCount.Matches);
        Assert.False(wrongCount.ActualCounted);
        Assert.Contains(
            wrongCount.Notes,
            note => note.Contains("customer_attempt_counted", StringComparison.Ordinal));

        // Null means the file asserts nothing about it, so it cannot be the cause of a mismatch.
        ScenarioDryRunReport unasserted = Run(new ScenarioDefinition(
            "SCN-997-count",
            null,
            [new ScenarioAttempt(1, "unreachable", null)],
            "IVR_INVALID_PHONE_FINAL",
            null));

        Assert.True(unasserted.Matches);
    }

    private static ScenarioDryRunReport Run(ScenarioDefinition scenario) =>
        CallScenarioDryRun.Execute(scenario, WindowStart, Window, 1);
}
