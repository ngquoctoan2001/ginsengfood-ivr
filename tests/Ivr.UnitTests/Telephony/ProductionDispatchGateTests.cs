using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.FeatureFlags;

namespace Ivr.UnitTests.Telephony;

/// <summary>
/// Q-28 (PA2, 2026-09-26). The production branch of the dispatch gate. It used to share the lab's
/// path - the lab allowlist first, which cannot hold a number, and a destination the privacy guard
/// refused on the first line - so no production call could pass and the branch had no test.
/// </summary>
public sealed class ProductionDispatchGateTests
{
    private const string Pilot =
        "pilot:jhapbmbcfcefgephpkljngbfbbhnobppnjlcdnnppcckmhnjclllmjdmhlmafnie";

    private static FeatureFlagSnapshot Production(
        bool realCustomerCallAllowed = true,
        params string[] labAllowlist) =>
        new(
            FeatureFlagEnvironments.Production,
            1,
            IvrOptions.ProductionRealExecutionMode,
            FeatureFlagValues.TargetV1,
            FeatureFlagValues.VendorSimProvider,
            "gh-247-prod-v1",
            realCustomerCallAllowed,
            new HashSet<string>(labAllowlist, StringComparer.Ordinal),
            false,
            false,
            false);

    private static FeatureFlagSnapshot Lab(params string[] labAllowlist) =>
        new(
            FeatureFlagEnvironments.Lab,
            1,
            IvrOptions.LabRealSimExecutionMode,
            FeatureFlagValues.FakeTargetV1,
            FeatureFlagValues.VendorSimProvider,
            FeatureFlagValues.MockLabAttemptPolicy,
            false,
            new HashSet<string>(labAllowlist, StringComparer.Ordinal),
            false,
            false,
            false);

    private sealed class Flags(FeatureFlagSnapshot snapshot) : IFeatureFlags
    {
        public Task<FeatureFlagReadResult> GetSnapshotAsync(
            string environment,
            bool forceFresh = false,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new FeatureFlagReadResult(snapshot, true, false));
    }

    private sealed class ReleaseGate(bool approved) : IProductionCallGate
    {
        public int Calls { get; private set; }

        public Task<bool> IsApprovedAsync(
            string environment,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(approved);
        }
    }

    private sealed class PilotGate(bool open, ProductionPilotDecision decision) : IProductionPilotGate
    {
        public int OpenCalls { get; private set; }

        public List<string> Asked { get; } = [];

        public Task<bool> IsOpenAsync(
            string environment,
            CancellationToken cancellationToken = default)
        {
            OpenCalls++;
            return Task.FromResult(open);
        }

        public Task<ProductionPilotDecision> EvaluateAsync(
            string environment,
            string destinationReference,
            CancellationToken cancellationToken = default)
        {
            Asked.Add(destinationReference);
            return Task.FromResult(decision);
        }
    }

    private static DispatchGate Gate(
        FeatureFlagSnapshot snapshot,
        ReleaseGate release,
        PilotGate pilot) =>
        new(new Flags(snapshot), release, new HealthyInMemoryRuntimeSafety(), pilot);

    private static PilotGate Listed() =>
        new(false, new ProductionPilotDecision(true, "PRODUCTION_PILOT_DESTINATION_APPROVED"));

    private static PilotGate NotListed() =>
        new(false, new ProductionPilotDecision(false, "DESTINATION_NOT_IN_PILOT"));

    /// <summary>
    /// The order: the environment's release first, then whether it has been opened past the pilot,
    /// and only then the pilot list, which is shown the fingerprint and nothing else.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TRUNK-GATE-02")]
    public async Task ProductionAsksTheReleaseThenWhetherItIsOpenThenThePilotList()
    {
        PilotGate untouched = Listed();
        DispatchGateDecision unreleased = await Gate(Production(), new ReleaseGate(false), untouched)
            .EvaluateAsync(FeatureFlagEnvironments.Production, Pilot);
        Assert.Equal(new DispatchGateDecision(false, "PRODUCTION_RELEASE_NOT_APPROVED"), unreleased);
        Assert.Equal(0, untouched.OpenCalls);
        Assert.Empty(untouched.Asked);

        var open = new PilotGate(true, new ProductionPilotDecision(false, "unused"));
        DispatchGateDecision opened = await Gate(Production(), new ReleaseGate(true), open)
            .EvaluateAsync(FeatureFlagEnvironments.Production, Pilot);
        Assert.Equal(new DispatchGateDecision(true, "PRODUCTION_OPEN_APPROVED"), opened);
        Assert.Empty(open.Asked);

        PilotGate listed = Listed();
        DispatchGateDecision onTheList = await Gate(Production(), new ReleaseGate(true), listed)
            .EvaluateAsync(FeatureFlagEnvironments.Production, Pilot);
        Assert.Equal(new DispatchGateDecision(true, "PRODUCTION_PILOT_DESTINATION_APPROVED"), onTheList);
        Assert.Equal(new[] { Pilot }, listed.Asked);

        DispatchGateDecision offTheList = await Gate(Production(), new ReleaseGate(true), NotListed())
            .EvaluateAsync(FeatureFlagEnvironments.Production, Pilot);
        Assert.Equal(new DispatchGateDecision(false, "DESTINATION_NOT_IN_PILOT"), offTheList);
    }

    /// <summary>
    /// The lab allowlist decides the lab and nothing in production: a reference on it is refused in
    /// production when the pilot list says no, and one missing from it is allowed when the pilot
    /// list says yes. The lab still uses it, and never asks the production gates.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TRUNK-GATE-03")]
    public async Task TheLabAllowlistDecidesTheLabAndNothingInProduction()
    {
        DispatchGateDecision allowlistedButNotPilot = await Gate(
                Production(true, Pilot),
                new ReleaseGate(true),
                NotListed())
            .EvaluateAsync(FeatureFlagEnvironments.Production, Pilot);
        Assert.False(allowlistedButNotPilot.Allowed);

        DispatchGateDecision pilotButNotAllowlisted = await Gate(
                Production(true),
                new ReleaseGate(true),
                Listed())
            .EvaluateAsync(FeatureFlagEnvironments.Production, Pilot);
        Assert.True(pilotButNotAllowlisted.Allowed);

        var release = new ReleaseGate(true);
        PilotGate pilot = Listed();
        DispatchGate lab = Gate(Lab("LAB-A"), release, pilot);
        Assert.Equal(
            new DispatchGateDecision(true, "LAB_DESTINATION_APPROVED"),
            await lab.EvaluateAsync(FeatureFlagEnvironments.Lab, "LAB-A"));
        Assert.Equal(
            new DispatchGateDecision(false, "DESTINATION_NOT_ALLOWLISTED"),
            await lab.EvaluateAsync(FeatureFlagEnvironments.Lab, "LAB-B"));
        Assert.Equal(0, release.Calls);
        Assert.Equal(0, pilot.OpenCalls);
        Assert.Empty(pilot.Asked);
    }

    /// <summary>
    /// Two things never reach the production gates. A production snapshot with real customer calls
    /// off is refused by the guardrails before either gate is asked, and a destination that still
    /// carries the number is refused by the privacy guard before anything is read at all.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TRUNK-GATE-04")]
    public async Task RealCallsOffOrANumberNeverReachTheProductionGates()
    {
        var release = new ReleaseGate(true);
        PilotGate pilot = Listed();

        DispatchGateDecision callsOff = await Gate(Production(realCustomerCallAllowed: false), release, pilot)
            .EvaluateAsync(FeatureFlagEnvironments.Production, Pilot);
        Assert.Equal(new DispatchGateDecision(false, "INVALID_RUNTIME_CONFIG"), callsOff);

        await Assert.ThrowsAnyAsync<Exception>(() => Gate(Production(), release, pilot)
            .EvaluateAsync(FeatureFlagEnvironments.Production, "sip:0912345678@sip.carrier.example.vn"));

        Assert.Equal(0, release.Calls);
        Assert.Equal(0, pilot.OpenCalls);
        Assert.Empty(pilot.Asked);
    }
}
