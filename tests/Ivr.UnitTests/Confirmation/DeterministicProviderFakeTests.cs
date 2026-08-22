using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Providers.Fakes;

namespace Ivr.UnitTests.Confirmation;

public sealed class DeterministicProviderFakeTests
{
    [Fact]
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
                AttemptId.Create("attempt-1")),
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
