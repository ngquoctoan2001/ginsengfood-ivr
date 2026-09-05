using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Telephony;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Telephony;

/// <summary>
/// W-0198 / <c>OD-V1-17</c> + <c>OD-V1-05</c>. The reusable dial token.
/// <para>
/// Five documents described the token as "one-use per attempt". Policy needs at least two
/// customer dials plus technical retries, and no contract anywhere can re-issue a token - so
/// one-use was never a rule this system could keep, and what shipped instead was no rule at all:
/// the old bookkeeping refused a repeated attempt id and nothing else, which meant a token could
/// be resolved without limit as long as each resolve carried a fresh one.
/// </para>
/// <para>
/// These pin the replacement. A ceiling is what a leaked token actually runs into.
/// </para>
/// </summary>
public sealed class DialTokenResolveLedgerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 5, 9, 0, 0, TimeSpan.Zero);

    private static DialTokenResolutionRequest Request(
        string attemptId,
        string taskId = "TASK-247-0001",
        int maxResolves = 3,
        int expiresInMinutes = 30,
        string token = "enc:mock-sha256:AAAA") =>
        new(
            DialTokenReference.Create(token, Now.AddMinutes(expiresInMinutes)),
            AttemptId.Create(attemptId),
            TaskId.Create(taskId),
            maxResolves);

    /// <summary>
    /// The decision itself. Under "one-use" the second customer dial could never have happened,
    /// and the policy that requires it would have been unenforceable.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TOKEN-REUSE-01")]
    public void TheSameTokenDialsAgainOnASecondAttempt()
    {
        DialTokenResolveLedger ledger = new();

        DialTokenResolveDecision first = ledger.Evaluate(Request("attempt-1"), Now);
        DialTokenResolveDecision second = ledger.Evaluate(Request("attempt-2"), Now);

        Assert.True(first.Allowed);
        Assert.True(second.Allowed);
        Assert.Equal(1, first.ResolveCount);
        Assert.Equal(2, second.ResolveCount);
    }

    /// <summary>
    /// The property "one-use" was reaching for, kept: a token cannot dial more times than policy
    /// allows, however many fresh attempt ids are presented with it. Asserted twice over the
    /// boundary because a ceiling that lets one extra call through is the failure that reaches a
    /// customer.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TOKEN-REUSE-02")]
    public void TheCeilingHoldsAndKeepsHolding()
    {
        DialTokenResolveLedger ledger = new();

        Assert.True(ledger.Evaluate(Request("attempt-1", maxResolves: 2), Now).Allowed);
        Assert.True(ledger.Evaluate(Request("attempt-2", maxResolves: 2), Now).Allowed);

        DialTokenResolveDecision third = ledger.Evaluate(Request("attempt-3", maxResolves: 2), Now);
        DialTokenResolveDecision fourth = ledger.Evaluate(Request("attempt-4", maxResolves: 2), Now);

        Assert.False(third.Allowed);
        Assert.Equal(DialTokenRefusalCodes.ResolveLimitExceeded, third.RefusalCode);
        Assert.False(fourth.Allowed);
        Assert.Equal(DialTokenRefusalCodes.ResolveLimitExceeded, fourth.RefusalCode);
        Assert.Equal(2, fourth.ResolveCount);
    }

    /// <summary>
    /// A repeated attempt is a replay, not a new dial - and it must not spend budget, or a
    /// caller retrying the same attempt could exhaust a token without ever placing a call.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TOKEN-REUSE-03")]
    public void AReplayedAttemptIsRefusedAndCostsNothing()
    {
        DialTokenResolveLedger ledger = new();
        Assert.True(ledger.Evaluate(Request("attempt-1"), Now).Allowed);

        DialTokenResolveDecision replay = ledger.Evaluate(Request("attempt-1"), Now);

        Assert.False(replay.Allowed);
        Assert.Equal(DialTokenRefusalCodes.AttemptReplay, replay.RefusalCode);
        Assert.Equal(1, replay.ResolveCount);

        // Budget intact: two more genuine attempts still fit under a ceiling of three.
        Assert.True(ledger.Evaluate(Request("attempt-2"), Now).Allowed);
        Assert.True(ledger.Evaluate(Request("attempt-3"), Now).Allowed);
    }

    /// <summary>
    /// <c>OD-V1-17</c> binds the token to a task. A token arriving under a second task is either
    /// a caller mixing tasks up or a replay against a different order; neither is a call worth
    /// placing, and neither may spend the budget of the task that owns the token.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TOKEN-REUSE-04")]
    public void TheTokenIsBoundToTheFirstTaskThatUsedIt()
    {
        DialTokenResolveLedger ledger = new();
        Assert.True(ledger.Evaluate(Request("attempt-1", "TASK-247-0001"), Now).Allowed);

        DialTokenResolveDecision otherTask = ledger.Evaluate(
            Request("attempt-2", "TASK-247-9999"),
            Now);

        Assert.False(otherTask.Allowed);
        Assert.Equal(DialTokenRefusalCodes.TaskMismatch, otherTask.RefusalCode);

        DialTokenResolveDecision ownTask = ledger.Evaluate(Request("attempt-3", "TASK-247-0001"), Now);
        Assert.True(ownTask.Allowed);
        Assert.Equal(2, ownTask.ResolveCount);
    }

    /// <summary>
    /// Expiry is checked before anything is recorded, so an expired token neither dials nor
    /// leaves a resolve behind.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TOKEN-REUSE-05")]
    public void AnExpiredTokenIsRefusedAndRecordsNothing()
    {
        DialTokenResolveLedger ledger = new();

        DialTokenResolveDecision expired = ledger.Evaluate(
            Request("attempt-1", expiresInMinutes: 0),
            Now);

        Assert.False(expired.Allowed);
        Assert.Equal(DialTokenRefusalCodes.Expired, expired.RefusalCode);
        Assert.Equal(0, expired.ResolveCount);
    }

    /// <summary>
    /// A caller that states no ceiling is refused rather than treated as unlimited. This is the
    /// one input where a permissive default would silently delete the entire control, so it fails
    /// closed - and the dispatch store passes zero exactly when the policy row is missing.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Trait("TestId", "UT-TOKEN-REUSE-06")]
    public void AMissingCeilingIsRefusedRatherThanTreatedAsUnlimited(int maxResolves)
    {
        DialTokenResolveLedger ledger = new();

        DialTokenResolveDecision decision = ledger.Evaluate(
            Request("attempt-1", maxResolves: maxResolves),
            Now);

        Assert.False(decision.Allowed);
        Assert.Equal(DialTokenRefusalCodes.CeilingMissing, decision.RefusalCode);
    }

    /// <summary>
    /// Two tokens are two budgets. Sharing a ledger must not make one task's dials count against
    /// another's.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TOKEN-REUSE-07")]
    public void EachTokenCarriesItsOwnBudget()
    {
        DialTokenResolveLedger ledger = new();

        Assert.True(ledger.Evaluate(
            Request("attempt-1", "TASK-A", maxResolves: 1, token: "enc:mock-sha256:AAAA"),
            Now).Allowed);
        DialTokenResolveDecision other = ledger.Evaluate(
            Request("attempt-2", "TASK-B", maxResolves: 1, token: "enc:mock-sha256:BBBB"),
            Now);

        Assert.True(other.Allowed);
        Assert.Equal(1, other.ResolveCount);
    }

    /// <summary>
    /// End of the real path, not just the ledger: the MOCK vault refuses with the rule that
    /// refused, and audits every resolve - allowed and refused - with the attempt id, which is
    /// what <c>OD-V1-05</c> asked for. A refusal nobody can see is the silent skip it forbids.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TOKEN-REUSE-08")]
    public async Task TheMockVaultRefusesWithTheRuleAndAuditsEveryResolve()
    {
        InMemoryAuditLogger auditLogger = new(TimeProvider.System);
        MockDialTokenVault vault = new(
            Options.Create(new MockTelephonyOptions
            {
                TokenDestinations = { ["mock-token"] = "mock-destination-allowlisted" },
                DestinationAllowlist = ["mock-destination-allowlisted"],
            }),
            auditLogger);
        string fingerprint = vault.Protect("ivr-confirmation-task-dial-token", "mock-token");

        DialTokenResolutionRequest first = Request("attempt-1", maxResolves: 1, token: fingerprint);
        await vault.ResolveAsync(first, Now, CancellationToken.None);

        DialTokenRefusedException refused = await Assert.ThrowsAsync<DialTokenRefusedException>(
            async () => await vault.ResolveAsync(
                Request("attempt-2", maxResolves: 1, token: fingerprint),
                Now,
                CancellationToken.None));

        Assert.Equal(DialTokenRefusalCodes.ResolveLimitExceeded, refused.RefusalCode);

        AuditLogEntry[] entries = [.. auditLogger.Entries];
        Assert.Equal(2, entries.Length);
        Assert.All(entries, entry =>
            Assert.Equal(DialTokenResolveAudit.Action, entry.Action));
        Assert.Equal("attempt-1", entries[0].CorrelationId);
        Assert.Equal("attempt-2", entries[1].CorrelationId);
        Assert.Equal(DialTokenRefusalCodes.ResolveLimitExceeded, entries[1].Reason);

        // The token itself is never in the trail. It is the thing that dials.
        Assert.All(entries, entry =>
            Assert.DoesNotContain("mock-token", entry.DataJson, StringComparison.Ordinal));
    }
}
