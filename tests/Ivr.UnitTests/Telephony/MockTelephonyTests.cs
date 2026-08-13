using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Persistence.Security;
using Ivr.Infrastructure.Scheduling;
using Ivr.Infrastructure.Telephony;
using Ivr.UnitTests.Confirmation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.UnitTests.Telephony;

public sealed class MockTelephonyTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 13, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "UT-TEL-SPEECH-01")]
    public async Task SpeechRendererProducesApprovedVietnameseOrderTextAndMetadata()
    {
        var renderer = new FakeSpeechRenderer();

        RenderedSpeech speech = await renderer.RenderAsync(
            TestData.Summary(),
            "SCRIPT-ORDER-CONFIRM",
            "v1-test-approved",
            ExecutionMode.Mock,
            CancellationToken.None);

        Assert.Equal(
            "Xin chào anh/chị Anh Đạt. Anh/chị có đơn DH-2026-001 gồm 2 Hộp Sâm lát, "
            + "tổng tiền 1.250.000 đồng, giao đến Phường Bến Nghé, Quận 1. "
            + "Bấm phím 1 để xác nhận đơn hàng, bấm phím 0 để hủy.",
            speech.ExactText);
        Assert.Equal("vi-VN", speech.Locale);
        Assert.Equal("FAKE_TEXT_ONLY", speech.AudioFormat);
        Assert.Equal(0, speech.CollapsedItemCount);
        Assert.NotEmpty(speech.ContentHash);
        Assert.Equal("[REDACTED_RENDERED_SPEECH]", speech.ToString());
    }

    [Fact]
    [Trait("TestId", "UT-TEL-SPEECH-02")]
    public async Task SpeechRendererCollapsesExtraItemsWithoutDroppingTotalOrInstructions()
    {
        PrivacySafeOrderSummary summary = PrivacySafeOrderSummary.Create(
            "Chị Mai",
            "DH-04",
            [
                SpeechItem.Create("Sâm lát", 1, "hộp"),
                SpeechItem.Create("Trà sâm", 2, "gói"),
                SpeechItem.Create("Mật ong", 3, "chai"),
                SpeechItem.Create("Kẹo sâm", 4, "túi"),
            ],
            Money.Vnd(9_876_500),
            ShortDeliveryArea.Create("Quận 7"),
            "24 trên 7",
            null,
            SpeechSummaryLimits.Create(20, 20));

        RenderedSpeech speech = await new FakeSpeechRenderer().RenderAsync(
            summary,
            "SCRIPT-ORDER-CONFIRM",
            "v1-test-approved",
            ExecutionMode.Mock,
            CancellationToken.None);

        Assert.Contains("và 1 sản phẩm khác", speech.ExactText, StringComparison.Ordinal);
        Assert.Contains("9.876.500 đồng", speech.ExactText, StringComparison.Ordinal);
        Assert.Contains("Bấm phím 1", speech.ExactText, StringComparison.Ordinal);
        Assert.Contains("bấm phím 0", speech.ExactText, StringComparison.Ordinal);
        Assert.DoesNotContain("Kẹo sâm", speech.ExactText, StringComparison.Ordinal);
        Assert.Equal(1, speech.CollapsedItemCount);
    }

    [Fact]
    [Trait("TestId", "UT-TEL-PRONUNCIATION-09")]
    public async Task SpeechRendererAppliesOnlyExactApprovedPronunciationHints()
    {
        PrivacySafeOrderSummary summary = PrivacySafeOrderSummary.Create(
            "Anh Nam",
            "DH-KGC",
            [SpeechItem.Create("KGC Plus", 1, "hộp")],
            Money.Vnd(500_000),
            ShortDeliveryArea.Create("Quận 3"),
            "Giờ Vàng",
            new Dictionary<string, string>
            {
                ["KGC Plus"] = "K G C pờ lớt",
            },
            SpeechSummaryLimits.Create(20, 20));

        RenderedSpeech speech = await new FakeSpeechRenderer().RenderAsync(
            summary,
            "SCRIPT-ORDER-CONFIRM",
            "v1-test-approved",
            ExecutionMode.Mock,
            CancellationToken.None);

        Assert.Contains("1 hộp K G C pờ lớt", speech.ExactText, StringComparison.Ordinal);
        Assert.DoesNotContain("KGC Plus", speech.ExactText, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "UT-TEL-TOKEN-03")]
    public async Task DialTokenResolverEnforcesExpiryAllowlistAndOneUsePerAttempt()
    {
        var resolver = new FakeDialTokenResolver(
            new Dictionary<string, string>
            {
                ["token-ok"] = "mock-destination-allowlisted",
                ["token-denied"] = "mock-destination-denied",
            },
            ["mock-destination-allowlisted"]);
        var request = new DialTokenResolutionRequest(
            DialTokenReference.Create("token-ok", Now.AddMinutes(5)),
            AttemptId.Create("attempt-token"));

        DialAuthorization authorization = await resolver.ResolveAsync(
            request,
            Now,
            CancellationToken.None);

        Assert.Equal("[REDACTED_DIAL_AUTHORIZATION]", authorization.ToString());
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await resolver.ResolveAsync(request, Now, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await resolver.ResolveAsync(
                new DialTokenResolutionRequest(
                    DialTokenReference.Create("token-ok", Now),
                    AttemptId.Create("attempt-expired")),
                Now,
                CancellationToken.None));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await resolver.ResolveAsync(
                new DialTokenResolutionRequest(
                    DialTokenReference.Create("token-denied", Now.AddMinutes(5)),
                    AttemptId.Create("attempt-denied")),
                Now,
                CancellationToken.None));
    }

    [Fact]
    [Trait("TestId", "UT-TEL-SCENARIO-04")]
    public async Task FakeGatewayCoversEveryProviderDispositionWithoutRecording()
    {
        SimProviderDisposition[] dispositions = Enum.GetValues<SimProviderDisposition>();
        foreach (SimProviderDisposition disposition in dispositions)
        {
            string attemptId = string.Concat("attempt-", disposition);
            var gateway = new FakeSimGateway(new Dictionary<string, FakeSimScenario>
            {
                [attemptId] = new(
                    disposition,
                    disposition == SimProviderDisposition.Answered ? "9" : null,
                    disposition.ToString().ToUpperInvariant()),
            });
            SimCallSession call = await gateway.DialAsync(
                Request(attemptId, "SIM-MATRIX"),
                CancellationToken.None);
            RenderedSpeech speech = await SpeechAsync();

            if (disposition == SimProviderDisposition.AudioError)
            {
                MockSimOperationException error = await Assert.ThrowsAsync<MockSimOperationException>(
                    async () => await gateway.PlayAsync(call, speech, CancellationToken.None));
                Assert.Equal(SimProviderDisposition.AudioError, error.Disposition);
            }
            else
            {
                await gateway.PlayAsync(call, speech, CancellationToken.None);
            }

            if (disposition == SimProviderDisposition.DtmfError)
            {
                MockSimOperationException error = await Assert.ThrowsAsync<MockSimOperationException>(
                    async () => await gateway.CaptureDtmfAsync(
                        call,
                        TimeSpan.FromSeconds(5),
                        CancellationToken.None));
                Assert.Equal(SimProviderDisposition.DtmfError, error.Disposition);
            }
            else
            {
                SimDtmfCapture dtmf = await gateway.CaptureDtmfAsync(
                    call,
                    TimeSpan.FromSeconds(5),
                    CancellationToken.None);
                if (disposition == SimProviderDisposition.Answered)
                {
                    Assert.Equal("9", dtmf.Key);
                }
            }

            SimDispositionReport report = await gateway.GetDispositionAsync(
                call,
                CancellationToken.None);
            Assert.Equal(disposition, report.Disposition);
            await gateway.HangupAsync(call, CancellationToken.None);
        }
    }

    [Fact]
    [Trait("TestId", "UT-TEL-CHANNEL-05")]
    public async Task FakeGatewayAllowsOnlyOneActiveCallPerChannel()
    {
        var gateway = new FakeSimGateway(new Dictionary<string, FakeSimScenario>
        {
            ["attempt-a"] = new(SimProviderDisposition.Answered, "1"),
            ["attempt-b"] = new(SimProviderDisposition.Answered, "0"),
        });
        SimCallSession first = await gateway.DialAsync(
            Request("attempt-a", "SIM-SINGLE"),
            CancellationToken.None);

        MockSimOperationException conflict = await Assert.ThrowsAsync<MockSimOperationException>(
            async () => await gateway.DialAsync(
                Request("attempt-b", "SIM-SINGLE"),
                CancellationToken.None));

        Assert.Equal("MOCK_CHANNEL_ALREADY_ACTIVE", conflict.TechnicalErrorCode);
        await gateway.HangupAsync(first, CancellationToken.None);
        SimCallSession second = await gateway.DialAsync(
            Request("attempt-b", "SIM-SINGLE"),
            CancellationToken.None);
        await gateway.HangupAsync(second, CancellationToken.None);
    }

    [Fact]
    [Trait("TestId", "UT-TEL-DTMF-10")]
    public async Task FakeGatewayDistinguishesNoInputFromInvalidKey()
    {
        var gateway = new FakeSimGateway(new Dictionary<string, FakeSimScenario>
        {
            ["attempt-none"] = new(SimProviderDisposition.Answered),
            ["attempt-invalid"] = new(SimProviderDisposition.Answered, "9"),
        });
        SimCallSession noInputCall = await gateway.DialAsync(
            Request("attempt-none", "SIM-DTMF-NONE"),
            CancellationToken.None);
        SimDtmfCapture noInput = await gateway.CaptureDtmfAsync(
            noInputCall,
            TimeSpan.FromSeconds(5),
            CancellationToken.None);
        await gateway.HangupAsync(noInputCall, CancellationToken.None);
        SimCallSession invalidCall = await gateway.DialAsync(
            Request("attempt-invalid", "SIM-DTMF-INVALID"),
            CancellationToken.None);
        SimDtmfCapture invalid = await gateway.CaptureDtmfAsync(
            invalidCall,
            TimeSpan.FromSeconds(5),
            CancellationToken.None);
        await gateway.HangupAsync(invalidCall, CancellationToken.None);

        Assert.True(noInput.NoInput);
        Assert.Null(noInput.Key);
        Assert.False(invalid.NoInput);
        Assert.Equal("9", invalid.Key);
        Assert.Contains(
            gateway.Events,
            providerEvent => providerEvent.Type == SimProviderEventType.DtmfCaptured
                && providerEvent.StatusCode == "INVALID");
    }

    [Fact]
    [Trait("TestId", "UT-TEL-SAFETY-06")]
    public void MockConfigurationFailsClosedAndAdapterHasNoNetworkTransport()
    {
        var validator = new MockTelephonyOptionsValidator();
        Microsoft.Extensions.Options.ValidateOptionsResult invalid = validator.Validate(
            null,
            new MockTelephonyOptions
            {
                Enabled = true,
                KillSwitchEngaged = false,
                TokenDestinations = new Dictionary<string, string>
                {
                    ["token"] = "0901234567",
                },
                DestinationAllowlist = ["0901234567"],
                Scenarios = new Dictionary<string, MockSimScenarioOptions>
                {
                    ["attempt"] = new() { Disposition = "Answered", DtmfKey = "1" },
                },
            });
        Assert.False(invalid.Succeeded);
        Assert.NotNull(invalid.Failures);
        Assert.Contains(
            invalid.Failures!,
            failure => failure.Contains("raw phone", StringComparison.OrdinalIgnoreCase));
        Microsoft.Extensions.Options.ValidateOptionsResult killed = validator.Validate(
            null,
            new MockTelephonyOptions
            {
                Enabled = true,
                KillSwitchEngaged = true,
                TokenDestinations = new Dictionary<string, string>
                {
                    ["token"] = "mock-destination",
                },
                DestinationAllowlist = ["mock-destination"],
                Scenarios = new Dictionary<string, MockSimScenarioOptions>
                {
                    ["attempt"] = new() { Disposition = "Answered", DtmfKey = "1" },
                },
            });
        Assert.False(killed.Succeeded);
        Assert.NotNull(killed.Failures);
        Assert.Contains(
            killed.Failures!,
            failure => failure.Contains("KillSwitch", StringComparison.Ordinal));

        Type gatewayType = typeof(FakeSimGateway);
        FieldInfo[] transportFields = gatewayType.GetFields(
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.DoesNotContain(transportFields, field =>
            typeof(HttpClient).IsAssignableFrom(field.FieldType)
            || typeof(Socket).IsAssignableFrom(field.FieldType)
            || typeof(TcpClient).IsAssignableFrom(field.FieldType));
        Assert.DoesNotContain(
            gatewayType.Assembly.GetReferencedAssemblies(),
            assembly => assembly.Name is "SSH.NET" or "System.IO.Ports");
    }

    [Fact]
    [Trait("TestId", "UT-TEL-DI-08")]
    public void MockBootstrapWiresOnlyDeterministicNoEgressProviders()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IVR_EXECUTION_MODE"] = IvrOptions.MockExecutionMode,
                ["SALES_PROVIDER"] = "FAKE_TARGET_V1",
                ["SIM_PROVIDER"] = "MOCK",
                ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",
                ["ConnectionStrings:IvrDb"] =
                    "Host=localhost;Port=55433;Database=ivr_unit;Username=ivr",
            })
            .Build();
        var services = new ServiceCollection();
        services.AddIvrFoundation(configuration);
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        Assert.IsType<MockSchedulerDispatchGateway>(
            provider.GetRequiredService<ISchedulerDispatchGateway>());
        Assert.IsType<MockDialTokenVault>(provider.GetRequiredService<IDialTokenResolver>());
        Assert.Same(
            provider.GetRequiredService<IDialTokenResolver>(),
            provider.GetRequiredService<IOpaqueValueProtector>());
        Assert.IsType<FakeSimGateway>(provider.GetRequiredService<ISimGateway>());
        Assert.IsType<ApprovedVietnameseSpeechRenderer>(
            provider.GetRequiredService<ISpeechRenderer>());
        Assert.False(provider.GetRequiredService<ISchedulerDispatchGateway>().IsReady);
    }

    [Fact]
    [Trait("TestId", "UT-TEL-VAULT-11")]
    public async Task MockVaultProtectsTokenFingerprintAndResolvesOnlyInMemoryAllowlistedDestination()
    {
        var options = new MockTelephonyOptions
        {
            TokenDestinations = new Dictionary<string, string>
            {
                ["opaque-test-token"] = "mock-destination-allowlisted",
            },
            DestinationAllowlist = ["mock-destination-allowlisted"],
        };
        var vault = new MockDialTokenVault(
            Microsoft.Extensions.Options.Options.Create(options));
        string fingerprint = vault.Protect(
            "ivr-confirmation-task-dial-token",
            "opaque-test-token");

        DialAuthorization authorization = await vault.ResolveAsync(
            new DialTokenResolutionRequest(
                DialTokenReference.Create(fingerprint, Now.AddMinutes(5)),
                AttemptId.Create("attempt-vault")),
            Now,
            CancellationToken.None);

        Assert.StartsWith("enc:mock-sha256:", fingerprint, StringComparison.Ordinal);
        Assert.DoesNotContain("opaque-test-token", fingerprint, StringComparison.Ordinal);
        Assert.Equal("[REDACTED_DIAL_AUTHORIZATION]", authorization.ToString());
        Assert.Throws<InvalidOperationException>(() => vault.Unprotect(
            "ivr-confirmation-task-dial-token",
            fingerprint));
    }

    [Fact]
    [Trait("TestId", "UT-TEL-RECORDING-07")]
    public async Task FakeGatewayRejectsRecordingEnabled()
    {
        var gateway = new FakeSimGateway(new Dictionary<string, FakeSimScenario>
        {
            ["attempt-record"] = new(SimProviderDisposition.Answered, "1"),
        });
        SimDialRequest unsafeRequest = Request("attempt-record", "SIM-REC") with
        {
            RecordingMode = SimRecordingMode.Enabled,
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await gateway.DialAsync(unsafeRequest, CancellationToken.None));
        Assert.Empty(gateway.Events);
    }

    [Fact]
    [Trait("TestId", "UT-TEL-DELAY-12")]
    public async Task FakeGatewayDelayHonorsCancellationWithoutStartingCall()
    {
        var gateway = new FakeSimGateway(new Dictionary<string, FakeSimScenario>
        {
            ["attempt-delay"] = new(
                SimProviderDisposition.Answered,
                "1",
                DialDelay: TimeSpan.FromMinutes(1)),
        });
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await gateway.DialAsync(
                Request("attempt-delay", "SIM-DELAY"),
                cancellation.Token));
        Assert.Empty(gateway.Events);
    }

    [Fact]
    [Trait("TestId", "UT-TEL-NONMOCK-13")]
    public void NonMockBootstrapCannotInstantiateP2FourFakeTelephonyProvider()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IVR_EXECUTION_MODE"] = IvrOptions.LabRealSimExecutionMode,
                ["SALES_PROVIDER"] = "FAKE_TARGET_V1",
                ["SIM_PROVIDER"] = "VENDOR",
                ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",
                ["ConnectionStrings:IvrDb"] =
                    "Host=localhost;Port=55433;Database=ivr_unit;Username=ivr",
            })
            .Build();
        var services = new ServiceCollection();
        services.AddIvrFoundation(configuration);
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);

        Assert.IsType<UnavailableSchedulerDispatchGateway>(
            provider.GetRequiredService<ISchedulerDispatchGateway>());
        Assert.Null(provider.GetService<ISimGateway>());
        Assert.Null(provider.GetService<IDialTokenResolver>());
        Assert.Null(provider.GetService<ISpeechRenderer>());
    }

    private static SimDialRequest Request(string attemptId, string channelId) => new(
        AttemptId.Create(attemptId),
        TaskId.Create(string.Concat("task-", attemptId)),
        channelId,
        Guid.NewGuid(),
        1,
        DialAuthorization.CreateTrusted("mock-destination-allowlisted"),
        SimRecordingMode.Disabled);

    private static ValueTask<RenderedSpeech> SpeechAsync() =>
        new FakeSpeechRenderer().RenderAsync(
            TestData.Summary(),
            "SCRIPT-ORDER-CONFIRM",
            "v1-test-approved",
            ExecutionMode.Mock,
            CancellationToken.None);
}
