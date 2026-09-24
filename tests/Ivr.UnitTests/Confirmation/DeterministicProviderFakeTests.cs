using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Providers.Fakes;

namespace Ivr.UnitTests.Confirmation;

public sealed class DeterministicProviderFakeTests
{
    [Fact]
    [Trait("TestId", "UT-FAKE-PORT-08")]
    public async Task EveryProviderPortHasDeterministicFake()
    {
        ProviderFakeSnapshot first = await RunProviderFakesOnce();
        ProviderFakeSnapshot second = await RunProviderFakesOnce();

        Assert.Equal(first, second);
    }

    private static async Task<ProviderFakeSnapshot> RunProviderFakesOnce()
    {
        DateTimeOffset now = new(2026, 8, 13, 0, 0, 0, TimeSpan.Zero);
        var clock = new FakeSystemClock(now);
        var ids = new FakeIdentifierGenerator(["id-1"]);
        var resolver = new FakeDialTokenResolver(new Dictionary<string, string>
        {
            ["dial-token-1"] = "provider-destination-ref-1",
        });
        DialAuthorization authorization = await resolver.ResolveAsync(
            new DialTokenResolutionRequest(
                DialTokenReference.Create("dial-token-1", now.AddMinutes(10)),
                AttemptId.Create("attempt-1"),
                TaskId.Create("TASK-FAKE-1"),
                3),
            now,
            CancellationToken.None);
        var renderer = new FakeSpeechRenderer();
        RenderedSpeech speech = await renderer.RenderAsync(
            TestData.Summary(),
            "SCRIPT-ORDER-CONFIRM",
            Ivr.Domain.Scripts.TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock,
            CancellationToken.None);
        var gateway = new FakeSimGateway(new Dictionary<string, FakeSimScenario>
        {
            ["attempt-1"] = new(SimProviderDisposition.Answered, "1"),
        });
        SimCallSession call = await gateway.DialAsync(
            new SimDialRequest(
                AttemptId.Create("attempt-1"),
                TaskId.Create("task-1"),
                "SIM-MOCK-001",
                Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                1,
                authorization,
                SimRecordingMode.Disabled),
            CancellationToken.None);
        await gateway.PlayAsync(call, speech, CancellationToken.None);
        SimDtmfCapture dtmf = await gateway.CaptureDtmfAsync(
            call,
            TimeSpan.FromSeconds(10),
            CancellationToken.None);
        SimDispositionReport disposition = await gateway.GetDispositionAsync(
            call,
            CancellationToken.None);
        await gateway.HangupAsync(call, CancellationToken.None);
        SimGatewayHealth health = await gateway.CheckHealthAsync(
            "SIM-MOCK-001",
            CancellationToken.None);

        CallbackAcknowledgement ack = new(
            CallbackAcknowledgementCode.Accepted,
            CallbackId.Create("callback-1"),
            CorrelationId.Create("correlation-1"));
        var callbackClient = new FakeOrderCoreCallbackClient([ack]);
        CallbackAcknowledgement returnedAck = await callbackClient.SubmitAsync(
            TestData.Result(),
            CancellationToken.None);
        var tokenProvider = new FakeServiceTokenProvider(new Dictionary<string, ServiceAccessToken>
        {
            ["sales"] = ServiceAccessToken.CreateTrusted("fake-service-token", now.AddMinutes(5)),
        });
        ServiceAccessToken token = await tokenProvider.GetAsync("sales", CancellationToken.None);

        var audit = new InMemoryDomainAuditSink();
        await audit.AppendAsync(
            new DomainAuditRecord(
                AuditReference.Create("audit-1"),
                "CALL_RESULT_CREATED",
                now,
                CorrelationId.Create("correlation-1")),
            CancellationToken.None);
        var evidence = new InMemoryDomainEvidenceSink();
        await evidence.AppendAsync(
            new DomainEvidenceRecord(
                EvidenceReference.Create("evidence-1"),
                "CALL_RESULT",
                TestData.Result().ComputeHash(),
                now),
            CancellationToken.None);

        return new ProviderFakeSnapshot(
            clock.UtcNow,
            ids.NewIdentifier(),
            authorization.ToString(),
            speech.ToString(),
            speech.ContentHash,
            dtmf.Key,
            disposition.Disposition,
            health.State,
            gateway.Events.Count,
            returnedAck,
            token.ToString(),
            Assert.Single(audit.Records).CorrelationId.Value,
            Assert.Single(evidence.Records).SnapshotHash);
    }

    [Fact]
    [Trait("TestId", "UT-FAKE-REGISTRY-09")]
    public async Task RegistryLookupIsVersionAndProgramSpecific()
    {
        FakeAttemptPolicyRegistry registry = new([TestData.Policy(AttemptPolicyApproval.OwnerApproved)]);

        AttemptPolicySnapshot resolved = await registry.ResolveAsync(
            PolicyVersion.Create("mock-lab-v1"),
            IvrProgramCode.GoldenHour,
            ExecutionMode.Mock,
            CancellationToken.None);
        Assert.Equal(IvrProgramCode.GoldenHour, resolved.Program);

        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await registry.ResolveAsync(
                PolicyVersion.Create("missing"),
                IvrProgramCode.GoldenHour,
                ExecutionMode.Mock,
                CancellationToken.None));
    }

    /// <summary>
    /// W-0353 / W-0094. The fake SIM keeps a bounded history. It is one instance for the life of a
    /// worker, and a MOCK rehearsal running for hours used to grow its event log and its
    /// played-speech map without limit; W-0094 capped them at 4,096 events and 1,024 played
    /// speeches. The newest entries have to survive the trim: whoever reads the fake after a long
    /// run wants the last calls, not the first ones.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-FAKE-HISTORY-10")]
    public async Task TheFakeSimKeepsABoundedHistoryAndItsNewestEntries()
    {
        var gateway = new FakeSimGateway(new Dictionary<string, FakeSimScenario>
        {
            ["*"] = new(SimProviderDisposition.Answered, "1"),
        });
        for (int index = 0; index < 4_106; index += 1)
        {
            await gateway.CheckHealthAsync($"SIM-{index:D5}", CancellationToken.None);
        }

        Assert.Equal(4_096, gateway.Events.Count);
        Assert.Equal("SIM-00010", gateway.Events.First().SimChannelId);
        Assert.Equal("SIM-04105", gateway.Events.Last().SimChannelId);

        DialAuthorization authorization = DialAuthorization.CreateTrusted("provider-destination-ref-1");
        RenderedSpeech speech = await new FakeSpeechRenderer().RenderAsync(
            TestData.Summary(),
            "SCRIPT-ORDER-CONFIRM",
            Ivr.Domain.Scripts.TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock,
            CancellationToken.None);
        for (int index = 0; index < 1_029; index += 1)
        {
            SimCallSession call = await gateway.DialAsync(
                new SimDialRequest(
                    AttemptId.Create($"attempt-{index}"),
                    TaskId.Create($"task-{index}"),
                    "SIM-MOCK-001",
                    Guid.NewGuid(),
                    1,
                    authorization,
                    SimRecordingMode.Disabled),
                CancellationToken.None);
            await gateway.PlayAsync(call, speech, CancellationToken.None);
            await gateway.HangupAsync(call, CancellationToken.None);
        }

        // Which older entry leaves is not asserted: the played-speech map is a concurrent
        // dictionary, which keeps no insertion order, so "oldest" is only the first key it yields.
        Assert.Equal(1_024, gateway.PlayedSpeech.Count);
        Assert.Contains("mock-call:attempt-1028", gateway.PlayedSpeech.Keys);
        Assert.Equal(4_096, gateway.Events.Count);
    }

    private sealed record ProviderFakeSnapshot(
        DateTimeOffset UtcNow,
        string Identifier,
        string AuthorizationDisplay,
        string SpeechDisplay,
        string SpeechContentHash,
        string? Dtmf,
        SimProviderDisposition Disposition,
        SimChannelHealthState Health,
        int EventCount,
        CallbackAcknowledgement Acknowledgement,
        string TokenDisplay,
        string AuditCorrelationId,
        string EvidenceContentHash);
}
