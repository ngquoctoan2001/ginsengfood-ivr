using Ivr.Domain.Confirmation;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Scheduling;

namespace Ivr.UnitTests.Scheduling;

/// <summary>
/// W-0354 / B13 (chief worklist 2026-09-25). The dispatch gateway now asks the deployment for its
/// mode instead of passing a hard-coded LAB_REAL_SIM to the script registry and the speech service.
/// These pin the mapping it asks through. The dispatch-level checks at PRODUCTION_REAL - a
/// lab-approved script refused, the production whitelist enforced - belong to the change that opens
/// production (SIP-04), because until then IsReady refuses PRODUCTION_REAL before either runs.
/// </summary>
public sealed class SchedulerExecutionContextTests
{
    [Theory]
    [InlineData(IvrOptions.MockExecutionMode, ExecutionMode.Mock)]
    [InlineData(IvrOptions.LabRealSimExecutionMode, ExecutionMode.LabRealSim)]
    [InlineData(IvrOptions.ProductionRealExecutionMode, ExecutionMode.ProductionReal)]
    [InlineData("lab_real_sim", ExecutionMode.LabRealSim)]
    [Trait("TestId", "UT-DISPATCH-MODE-01")]
    public void TheDeploymentModeReachesTheDomainUnchanged(string configured, ExecutionMode expected)
    {
        // Case-insensitive, as IvrOptionsValidator accepts the mode and IsReady compares it.
        Assert.Equal(expected, new SchedulerExecutionContext(configured).ToDomainMode());
    }

    [Theory]
    [InlineData("")]
    [InlineData("LAB")]
    [InlineData("PRODUCTION")]
    [Trait("TestId", "UT-DISPATCH-MODE-02")]
    public void AnUnknownModeIsRefusedRatherThanDefaultedToLab(string configured)
    {
        // Falling back to a mode is what the hard-coded LAB_REAL_SIM amounted to; an unknown value
        // must stop the dispatch instead of choosing one.
        Assert.Throws<InvalidOperationException>(
            () => new SchedulerExecutionContext(configured).ToDomainMode());
    }
}
