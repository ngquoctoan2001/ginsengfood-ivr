using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Scripts;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Speech;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Speech;

public sealed class SpeechPreparationDeadlineTests
{
    [Fact]
    [Trait("TestId", "UT-TTS-DEADLINE-01")]
    public async Task TotalDeadlineDoesNotRestartForEachSegmentAndReleasesTurn()
    {
        bool slow = true;
        int calls = 0;
        var service = Service(async token =>
        {
            calls++;
            if (slow) await Task.Delay(200, token);
        }, total: 350);
        var error = await Assert.ThrowsAsync<TtsSynthesisException>(() => Run(service, 1));
        Assert.Equal("TTS_PREPARATION_TIMEOUT", error.TechnicalErrorCode);
        Assert.InRange(calls, 1, 2);
        slow = false;
        Assert.Equal(7, (await Run(service, 2)).Audio!.Segments.Length);
    }

    [Fact]
    [Trait("TestId", "UT-TTS-DEADLINE-02")]
    public async Task QueueWaitSpendsTheSameTotalDeadlineAsSynthesis()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int calls = 0;
        var service = Service(async token =>
        {
            int call = Interlocked.Increment(ref calls);
            if (call == 1) { entered.SetResult(); await release.Task.WaitAsync(token); }
            if (call > 3) await Task.Delay(200, token);
        }, total: 600);
        Task<RenderedSpeech> first = Run(service, 1);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
        Task<RenderedSpeech> queued = Run(service, 2);
        await Task.Delay(350);
        release.SetResult();
        Assert.Equal(7, (await first).Audio!.Segments.Length);
        var error = await Assert.ThrowsAsync<TtsSynthesisException>(() => queued);
        Assert.Equal("TTS_PREPARATION_TIMEOUT", error.TechnicalErrorCode);
        Assert.InRange(calls, 4, 5);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("TestId", "UT-TTS-DEADLINE-03")]
    public async Task EarlierOrderExpiryOrCallerCancellationKeepsItsMeaning(bool caller)
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var service = Service(async token =>
        {
            entered.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
        }, total: 2000);
        using var cancellation = new CancellationTokenSource();
        Task<RenderedSpeech> task = Run(service, 1,
            caller ? DateTimeOffset.UtcNow.AddMinutes(1) : DateTimeOffset.UtcNow.AddMilliseconds(200), cancellation.Token);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
        if (caller)
        {
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        }
        else
        {
            var error = await Assert.ThrowsAsync<TtsSynthesisException>(() => task);
            Assert.Equal("TTS_CACHE_WINDOW_EXPIRED", error.TechnicalErrorCode);
        }
    }

    [Fact]
    [Trait("TestId", "UT-TTS-DEADLINE-04")]
    public async Task ProviderReturningAfterCancellationCannotReturnAPlaylist()
    {
        var service = Service(_ => Task.Delay(250, CancellationToken.None), total: 100);
        var error = await Assert.ThrowsAsync<TtsSynthesisException>(() => Run(service, 1));
        Assert.Equal("TTS_PREPARATION_TIMEOUT", error.TechnicalErrorCode);
    }

    [Theory]
    [InlineData(9)]
    [InlineData(120001)]
    [Trait("TestId", "UT-TTS-DEADLINE-05")]
    public void UnboundedPreparationIsRejectedAtStartup(int total)
    {
        Assert.True(new TtsProviderOptionsValidator().Validate(null,
            new TtsProviderOptions { PreparationTimeoutMilliseconds = total }).Failed);
    }

    private static SpeechSynthesisService Service(Func<CancellationToken, Task> call, int total)
    {
        var configured = Options.Create(new TtsProviderOptions
        {
            Provider = TtsProviderOptions.ExternalProvider,
            TimeoutMilliseconds = 5000, PreparationTimeoutMilliseconds = total,
            PreparationQueueTimeoutMilliseconds = 5000,
            Segmentation = new SpeechSegmentationOptions { Enabled = true, FixedSegments = FixedSegmentSource.Catalog },
            FixedSegments = TargetV1SpeechPolicy.FixedSegmentHashes(TargetV1SpeechPolicy.CanonicalVietnameseTemplate)
                .Select((hash, i) => new FixedSegmentMediaEntry { TextHash = hash, MediaReference = $"sound:fixed-{i}", DurationMilliseconds = 100 }).ToArray(),
        });
        return new SpeechSynthesisService(new Provider(call), new NoCache(), new TtsRequestBudget(TimeProvider.System),
            new TtsUsageMeter(), new RegionalVoiceMap(configured), configured, TimeProvider.System);
    }

    private static async Task<RenderedSpeech> Run(SpeechSynthesisService service, int order,
        DateTimeOffset? expires = null, CancellationToken token = default)
    {
        var summary = PrivacySafeOrderSummary.Create("Khách thử", $"LAB-{order}",
            [SpeechItem.Create("Sản phẩm thử", order, "hộp")], Money.Vnd(100000 + order),
            ShortDeliveryArea.Create("Quận 7"), "24 trên 7", null, SpeechSummaryLimits.Create(20, 20));
        RenderedSpeech rendered = await new FakeSpeechRenderer().RenderAsync(summary,
            TargetV1SpeechPolicy.MockTemplateId, TargetV1SpeechPolicy.MockTemplateVersion, ExecutionMode.Mock, token);
        return await service.SynthesizeAsync(rendered, summary, TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion, ExecutionMode.LabRealSim, expires ?? DateTimeOffset.UtcNow.AddMinutes(2), token);
    }

    private sealed class Provider(Func<CancellationToken, Task> call) : ITtsProvider
    {
        public async Task<RenderedAudio> SynthesizeAsync(SpeechScript script, TtsOptions options, CancellationToken cancellationToken)
        {
            await call(cancellationToken);
            return RenderedAudio.Create("audio/L16", 8000, TimeSpan.FromMilliseconds(100), "sound:synthetic");
        }
    }

    private sealed class NoCache : IAudioCache
    {
        public int Count => 0;
        public async Task<AudioCacheResult> GetOrCreateAsync(AudioCacheKey key, DateTimeOffset expiresAt,
            Func<CancellationToken, Task<RenderedAudio>> factory, CancellationToken cancellationToken)
            => new(await factory(cancellationToken), false, expiresAt);
        public Task<int> PurgeExpiredAsync(DateTimeOffset now, bool dryRun, CancellationToken cancellationToken) => Task.FromResult(0);
    }
}
