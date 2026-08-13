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
            DialTokenReference.Create("dial-token-1", now.AddMinutes(10)),
            now,
            CancellationToken.None);
        FakeSpeechRenderer renderer = new();
        RenderedSpeech speech = await renderer.RenderAsync(
            TestData.Summary(),
            "template-1",
            "version-1",
            CancellationToken.None);
        SimDialResult expectedDial = new(
            SimCallOutcome.DtmfConfirmed,
            now,
            now.AddMinutes(1),
            "provider-call-ref-1");
        FakeSimGateway gateway = new(new Dictionary<string, SimDialResult>
        {
            ["attempt-1"] = expectedDial,
        });
        SimDialResult actualDial = await gateway.DialAsync(
            new SimDialRequest(
                AttemptId.Create("attempt-1"),
                TaskId.Create("task-1"),
                authorization,
                speech),
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
        Assert.Equal(expectedDial, actualDial);
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
