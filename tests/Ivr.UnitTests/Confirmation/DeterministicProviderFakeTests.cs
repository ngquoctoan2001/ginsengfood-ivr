using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Providers.Fakes;

namespace Ivr.UnitTests.Confirmation;

public sealed class DeterministicProviderFakeTests
{
    [Fact]
    public async Task EveryProviderPortHasDeterministicFake()
    {
        DateTimeOffset now = new(2026, 8, 13, 0, 0, 0, TimeSpan.Zero);
        FakeSystemClock clock = new(now);
        FakeIdentifierGenerator ids = new(["id-1"]);
        FakeDialTokenResolver resolver = new(new Dictionary<string, string>
        {
            ["dial-token-1"] = "provider-destination-ref-1",
        });
        DialAuthorization authorization = await resolver.ResolveAsync(
            new DialTokenResolutionRequest(
                DialTokenReference.Create("dial-token-1", now.AddMinutes(10)),
                AttemptId.Create("attempt-1")),
            now,
            CancellationToken.None);
        FakeSpeechRenderer renderer = new();
        RenderedSpeech speech = await renderer.RenderAsync(
            TestData.Summary(),
            "SCRIPT-ORDER-CONFIRM",
            "v1-test-approved",
            ExecutionMode.Mock,
            CancellationToken.None);
        FakeSimGateway gateway = new(new Dictionary<string, FakeSimScenario>
        {
            ["attempt-1"] = new(SimProviderDisposition.Answered, "1"),
        });
        SimCallSession call = await gateway.DialAsync(
            new SimDialRequest(
                AttemptId.Create("attempt-1"),
                TaskId.Create("task-1"),
                "SIM-MOCK-001",
                Guid.NewGuid(),
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
        FakeOrderCoreCallbackClient callbackClient = new([ack]);
        CallbackAcknowledgement returnedAck = await callbackClient.SubmitAsync(
            TestData.Result(),
            CancellationToken.None);
        FakeServiceTokenProvider tokenProvider = new(new Dictionary<string, ServiceAccessToken>
        {
            ["sales"] = ServiceAccessToken.CreateTrusted("fake-service-token", now.AddMinutes(5)),
        });
        ServiceAccessToken token = await tokenProvider.GetAsync("sales", CancellationToken.None);

        InMemoryDomainAuditSink audit = new();
        await audit.AppendAsync(
            new DomainAuditRecord(
                AuditReference.Create("audit-1"),
                "CALL_RESULT_CREATED",
                now,
                CorrelationId.Create("correlation-1")),
            CancellationToken.None);
        InMemoryDomainEvidenceSink evidence = new();
        await evidence.AppendAsync(
            new DomainEvidenceRecord(
                EvidenceReference.Create("evidence-1"),
                "CALL_RESULT",
                TestData.Result().ComputeHash(),
                now),
            CancellationToken.None);

        Assert.Equal(now, clock.UtcNow);
        Assert.Equal("id-1", ids.NewIdentifier());
        Assert.Equal("[REDACTED_DIAL_AUTHORIZATION]", authorization.ToString());
        Assert.Equal("[REDACTED_RENDERED_SPEECH]", speech.ToString());
        Assert.Equal("1", dtmf.Key);
        Assert.Equal(SimProviderDisposition.Answered, disposition.Disposition);
        Assert.Equal(SimChannelHealthState.Healthy, health.State);
        Assert.Equal(6, gateway.Events.Count);
        Assert.Equal(ack, returnedAck);
        Assert.Equal("[REDACTED_SERVICE_TOKEN]", token.ToString());
        Assert.Single(audit.Records);
        Assert.Single(evidence.Records);
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
}
