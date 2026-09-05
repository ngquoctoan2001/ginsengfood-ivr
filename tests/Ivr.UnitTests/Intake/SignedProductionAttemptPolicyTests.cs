using Ivr.Domain.Confirmation;
using Ivr.Infrastructure.Intake;
using Ivr.Infrastructure.Providers.Fakes;

namespace Ivr.UnitTests.Intake;

/// <summary>
/// W-0198 / <c>OD-V1-08</c> + <c>OD-V1-16</c>. The attempt policy signed on 2026-09-05.
/// <para>
/// Two separate things are pinned here and they are worth keeping apart. One is that the signed
/// version may actually be used in <c>PRODUCTION_REAL</c> - registering a policy nobody can
/// resolve would be an empty gesture. The other is that <c>mock-lab-v1</c> did <b>not</b> come
/// along with it: <c>W-0151</c> asked that the candidate never be renamed or re-approved, because
/// a version whose approval changes underneath an accepted task changes what that task was
/// admitted under.
/// </para>
/// </summary>
public sealed class SignedProductionAttemptPolicyTests
{
    /// <summary>
    /// The point of signing. Both programs, all three modes, no throw - and the candidate still
    /// refused in the one mode that reaches a customer.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-POLICY-SIGNED-01")]
    public void TheSignedPolicyIsAdmittedInProductionAndTheCandidateStillIsNot()
    {
        foreach (AttemptPolicySnapshot signed in SignedProductionAttemptPolicies.Create())
        {
            signed.EnsureEnvironmentAllowed(ExecutionMode.Mock);
            signed.EnsureEnvironmentAllowed(ExecutionMode.LabRealSim);
            signed.EnsureEnvironmentAllowed(ExecutionMode.ProductionReal);
            Assert.Equal(AttemptPolicyApproval.OwnerApproved, signed.Approval);
        }

        foreach (AttemptPolicySnapshot candidate in CandidateAttemptPolicies.Create())
        {
            candidate.EnsureEnvironmentAllowed(ExecutionMode.Mock);
            candidate.EnsureEnvironmentAllowed(ExecutionMode.LabRealSim);
            Assert.Throws<InvalidOperationException>(() =>
                candidate.EnsureEnvironmentAllowed(ExecutionMode.ProductionReal));
        }
    }

    /// <summary>
    /// W-0151. Signing produced a new version rather than promoting the old one. If these two
    /// strings ever became equal, every task already admitted under the candidate would
    /// retroactively be holding a production-approved policy.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-POLICY-SIGNED-02")]
    public void SigningAddedAVersionRatherThanPromotingTheCandidate()
    {
        Assert.NotEqual(CandidateAttemptPolicies.Version, SignedProductionAttemptPolicies.Version);
        Assert.Equal("mock-lab-v1", CandidateAttemptPolicies.Version);
        Assert.Equal("gh-247-prod-v1", SignedProductionAttemptPolicies.Version);
        Assert.Equal(
            "OD-V1-08+OD-V1-16@2026-09-05",
            SignedProductionAttemptPolicies.SignedDecisionRef);

        Assert.All(
            CandidateAttemptPolicies.Create(),
            policy => Assert.Equal(AttemptPolicyApproval.CandidateMockLabOnly, policy.Approval));
    }

    /// <summary>
    /// The numbers themselves, spelled out rather than compared against the catalogue that
    /// produced them. <c>OD-V1-08</c> chose <c>D-10</c> over phase-8 precisely because phase-8 gave
    /// Golden Hour a 600-second window against a five-minute promise, so the window is the value
    /// most worth pinning: a silent drift back to 600 here is the decision being un-made.
    /// </summary>
    [Theory]
    [InlineData(IvrProgramCode.GoldenHour, 2, 150, 300)]
    [InlineData(IvrProgramCode.TwentyFourSeven, 2, 450, 900)]
    [Trait("TestId", "UT-POLICY-SIGNED-03")]
    public void TheSignedNumbersAreTheOnesTheOwnerSigned(
        IvrProgramCode program,
        int attempts,
        int secondAttemptOffsetSeconds,
        int windowSeconds)
    {
        AttemptPolicySnapshot policy = Assert.Single(
            SignedProductionAttemptPolicies.Create(),
            candidate => candidate.Program == program);

        Assert.Equal(SignedProductionAttemptPolicies.Version, policy.Version.Value);
        Assert.Equal(attempts, policy.MaxCustomerAttempts);
        Assert.Equal(
            [TimeSpan.Zero, TimeSpan.FromSeconds(secondAttemptOffsetSeconds)],
            policy.AttemptOffsets);
        Assert.Equal(TimeSpan.FromSeconds(windowSeconds), policy.ConfirmationWindowDuration);
    }

    /// <summary>
    /// A program missing from the signed set would resolve to nothing at all, so the set has to
    /// cover every program that exists rather than the ones that happened to be typed out.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-POLICY-SIGNED-04")]
    public void EveryProgramHasASignedPolicy()
    {
        Assert.Equal(
            Enum.GetValues<IvrProgramCode>().Order(),
            SignedProductionAttemptPolicies.Create().Select(policy => policy.Program).Order());
    }

    /// <summary>
    /// End of the resolve path, not just the catalogue. The in-memory registry is the entire
    /// registry when no database is configured, so a signed version it does not hold could not be
    /// rehearsed in the mode built for rehearsing it.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-POLICY-SIGNED-05")]
    public async Task TheInMemoryRegistryResolvesTheSignedVersionInProduction()
    {
        FakeAttemptPolicyRegistry registry = new(
        [
            .. CandidateAttemptPolicies.Create(),
            .. SignedProductionAttemptPolicies.Create(),
        ]);

        AttemptPolicySnapshot resolved = await registry.ResolveAsync(
            PolicyVersion.Create(SignedProductionAttemptPolicies.Version),
            IvrProgramCode.GoldenHour,
            ExecutionMode.ProductionReal,
            CancellationToken.None);

        Assert.Equal(SignedProductionAttemptPolicies.Version, resolved.Version.Value);
        Assert.Equal(TimeSpan.FromMinutes(5), resolved.ConfirmationWindowDuration);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await registry.ResolveAsync(
                PolicyVersion.Create(CandidateAttemptPolicies.Version),
                IvrProgramCode.GoldenHour,
                ExecutionMode.ProductionReal,
                CancellationToken.None));
    }
}
