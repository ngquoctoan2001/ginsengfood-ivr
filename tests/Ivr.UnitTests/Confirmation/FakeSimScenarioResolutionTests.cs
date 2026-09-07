using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Providers.Fakes;

namespace Ivr.UnitTests.Confirmation;

/// <summary>
/// W-0203 / P1.2. How <see cref="FakeSimGateway"/> picks the scenario for one dial.
/// <para>
/// The prefix arm exists because the gateway is a singleton built once from the options snapshot:
/// before it, a scenario could only be scripted for an identifier that was already known at
/// process start, so a rehearsal admitting fresh tasks every round had to restart the worker
/// between rounds - and restarting the worker between rounds destroys the crash-recovery signal
/// such a rehearsal exists to measure.
/// </para>
/// </summary>
public sealed class FakeSimScenarioResolutionTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 5, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "UT-FAKE-SCEN-01")]
    public async Task ExactAttemptIdBeatsEveryOtherMatch()
    {
        FakeSimGateway gateway = Gateway(new()
        {
            ["attempt-1"] = new(SimProviderDisposition.Answered, "1"),
            ["E2E-CONFIRM-"] = new(SimProviderDisposition.Busy),
            ["E2E-CONFIRM-*"] = new(SimProviderDisposition.Busy),
            ["*"] = new(SimProviderDisposition.RingTimeout),
        });

        Assert.Equal("1", await DialAndCaptureAsync(gateway, "attempt-1", "E2E-CONFIRM-1"));
    }

    [Fact]
    [Trait("TestId", "UT-FAKE-SCEN-02")]
    public async Task ExactTaskIdBeatsAPrefixPattern()
    {
        FakeSimGateway gateway = Gateway(new()
        {
            ["E2E-CONFIRM-0001"] = new(SimProviderDisposition.Answered, "1"),
            ["E2E-CONFIRM-*"] = new(SimProviderDisposition.Busy),
            ["*"] = new(SimProviderDisposition.RingTimeout),
        });

        Assert.Equal("1", await DialAndCaptureAsync(gateway, "attempt-9", "E2E-CONFIRM-0001"));
    }

    [Fact]
    [Trait("TestId", "UT-FAKE-SCEN-03")]
    public async Task APrefixPatternMatchesEveryTaskBeneathIt()
    {
        FakeSimGateway gateway = Gateway(new()
        {
            ["E2E-CONFIRM-*"] = new(SimProviderDisposition.Answered, "1"),
            ["*"] = new(SimProviderDisposition.RingTimeout),
        });

        foreach (string taskId in new[]
                 {
                     "E2E-CONFIRM-101010-00010",
                     "E2E-CONFIRM-999999-99999",
                     "E2E-CONFIRM-",
                 })
        {
            Assert.Equal("1", await DialAndCaptureAsync(gateway, $"attempt-{taskId}", taskId));
        }
    }

    /// <summary>
    /// The tie-break that stops two neighbouring scenario families quietly swapping behaviour:
    /// the longer prefix wins, and it wins because the list is ordered once at construction
    /// rather than because a dictionary happened to enumerate that way.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-FAKE-SCEN-04")]
    public async Task TheLongestMatchingPrefixWins()
    {
        FakeSimGateway gateway = Gateway(new()
        {
            ["E2E-*"] = new(SimProviderDisposition.RingTimeout),
            ["E2E-ACK*"] = new(SimProviderDisposition.Busy),
            ["E2E-ACKDUP-*"] = new(SimProviderDisposition.Answered, "1"),
        });

        Assert.Equal("1", await DialAndCaptureAsync(gateway, "a1", "E2E-ACKDUP-0001"));
        Assert.Null(await DialAndCaptureAsync(gateway, "a2", "E2E-ACK429-0001"));
        Assert.Null(await DialAndCaptureAsync(gateway, "a3", "E2E-CONFIRM-0001"));
    }

    [Fact]
    [Trait("TestId", "UT-FAKE-SCEN-05")]
    public async Task PrefixesAreMatchedAgainstTheAttemptIdAsWellAsTheTask()
    {
        FakeSimGateway gateway = Gateway(new()
        {
            ["ATT-SLOW-*"] = new(SimProviderDisposition.Answered, "1"),
        });

        Assert.Equal("1", await DialAndCaptureAsync(gateway, "ATT-SLOW-77", "TASK-UNRELATED"));
    }

    /// <summary>
    /// Nothing about the prefix arm may weaken the original contract: an unscripted dial with no
    /// catch-all is still refused rather than guessed at.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-FAKE-SCEN-06")]
    public async Task AnUnscriptedDialIsStillRefusedWhenThereIsNoCatchAll()
    {
        FakeSimGateway gateway = Gateway(new()
        {
            ["E2E-CONFIRM-*"] = new(SimProviderDisposition.Answered, "1"),
        });

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => DialAndCaptureAsync(gateway, "attempt-x", "E2E-CANCEL-0001"));
    }

    /// <summary>
    /// A bare <c>*</c> is the catch-all it always was, not a zero-length prefix that would match
    /// everything ahead of the more specific patterns.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-FAKE-SCEN-07")]
    public async Task TheBareStarStaysTheLastResort()
    {
        FakeSimGateway gateway = Gateway(new()
        {
            ["*"] = new(SimProviderDisposition.RingTimeout),
            ["E2E-CONFIRM-*"] = new(SimProviderDisposition.Answered, "1"),
        });

        Assert.Equal("1", await DialAndCaptureAsync(gateway, "a1", "E2E-CONFIRM-1"));
        Assert.Null(await DialAndCaptureAsync(gateway, "a2", "SOMETHING-ELSE"));
    }

    private static FakeSimGateway Gateway(Dictionary<string, FakeSimScenario> scenarios) =>
        new(scenarios);

    private static async Task<string?> DialAndCaptureAsync(
        FakeSimGateway gateway,
        string attemptId,
        string taskId)
    {
        var resolver = new FakeDialTokenResolver(new Dictionary<string, string>
        {
            ["dial-token"] = "provider-destination-ref-1",
        });
        DialAuthorization authorization = await resolver.ResolveAsync(
            new DialTokenResolutionRequest(
                DialTokenReference.Create("dial-token", Now.AddMinutes(10)),
                AttemptId.Create(attemptId),
                TaskId.Create(taskId),
                3),
            Now,
            CancellationToken.None);
        SimCallSession session = await gateway.DialAsync(
            new SimDialRequest(
                AttemptId.Create(attemptId),
                TaskId.Create(taskId),
                $"SIM-MOCK-{attemptId}",
                Guid.NewGuid(),
                1,
                authorization,
                SimRecordingMode.Disabled),
            CancellationToken.None);
        if (!session.IsConnected)
        {
            await gateway.HangupAsync(session, CancellationToken.None);
            return null;
        }

        SimDtmfCapture capture = await gateway.CaptureDtmfAsync(
            session,
            TimeSpan.FromSeconds(5),
            CancellationToken.None);
        await gateway.HangupAsync(session, CancellationToken.None);
        return capture.Key;
    }
}
