using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Scripts;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Speech;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Speech;

public sealed class SpeechPreparationQueueTests
{
    [Fact]
    [Trait("TestId", "UT-TTS-QUEUE-01")]
    public async Task CompleteOrdersAreFifoAndQueueWaitDoesNotSpendSegmentTimeout()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var identities = new List<string>();
        var provider = new Provider(async (script, token) =>
        {
            identities.Add(script.SummaryHash);
            if (identities.Count == 1) { entered.SetResult(); await release.Task.WaitAsync(token); }
            await Task.Delay(180, token);
        });
        var usage = new TtsUsageMeter();
        SpeechSynthesisService service = Service(provider, usage, timeout: 500);
        Task<RenderedSpeech> first = Run(service, 1);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
        Task<RenderedSpeech> second = Run(service, 2);
        Task<RenderedSpeech> third = Run(service, 3);
        release.SetResult();
        RenderedSpeech[] results = await Task.WhenAll(first, second, third);
        Assert.All(results, result => Assert.Equal(7, result.Audio!.Segments.Length));
        Assert.Equal(9, identities.Count);
        Assert.Equal(3, identities.Distinct().Count());
        for (int i = 0; i < 3; i++) Assert.All(identities.Skip(i * 3).Take(3), id => Assert.Equal(identities[i * 3], id));
    }

    [Fact]
    [Trait("TestId", "UT-TTS-QUEUE-02")]
    public async Task FullQueueRejectsAndCancelledWaiterFreesCapacityWithoutCallingProvider()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int calls = 0;
        var provider = new Provider(async (_, token) =>
        {
            if (Interlocked.Increment(ref calls) == 1) { entered.SetResult(); await release.Task.WaitAsync(token); }
        });
        var usage = new TtsUsageMeter();
        var service = Service(provider, usage, limit: 1);
        Task<RenderedSpeech> active = Run(service, 1);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
        using var cancelled = new CancellationTokenSource();
        Task<RenderedSpeech> queued = Run(service, 2, token: cancelled.Token);
        var full = await Assert.ThrowsAsync<TtsSynthesisException>(() => Run(service, 3));
        Assert.Equal("TTS_QUEUE_FULL", full.TechnicalErrorCode);
        cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => queued);
        Task<RenderedSpeech> replacement = Run(service, 4);
        Assert.Equal(1, calls);
        release.SetResult();
        await Task.WhenAll(active, replacement);
        Assert.Equal(6, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("TestId", "UT-TTS-QUEUE-03")]
    public async Task WaitingStopsAtQueueBudgetOrOrderExpiryAndNextOrderCanRun(bool expires)
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int calls = 0;
        var provider = new Provider(async (_, token) =>
        {
            if (Interlocked.Increment(ref calls) == 1) { entered.SetResult(); await release.Task.WaitAsync(token); }
        });
        var usage = new TtsUsageMeter();
        var service = Service(provider, usage, queueTimeout: expires ? 5000 : 100);
        Task<RenderedSpeech> active = Run(service, 1);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
        var error = await Assert.ThrowsAsync<TtsSynthesisException>(() => Run(service, 2,
            expires: DateTimeOffset.UtcNow.AddMilliseconds(expires ? 100 : 5000)));
        Assert.Equal(expires ? "TTS_CACHE_WINDOW_EXPIRED" : "TTS_QUEUE_TIMEOUT", error.TechnicalErrorCode);
        Assert.Equal(1, calls);
        release.SetResult();
        await active;
        await Run(service, 3);
        Assert.Equal(6, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("TestId", "UT-TTS-QUEUE-04")]
    public async Task ActiveFailureOrExpiryReleasesTurnWithoutPartialPlaylist(bool expires)
    {
        int calls = 0;
        var provider = new Provider(async (_, token) =>
        {
            if (Interlocked.Increment(ref calls) != 1) return;
            if (expires) await Task.Delay(Timeout.InfiniteTimeSpan, token);
            else throw new TtsSynthesisException("TEST_FAILURE", "Synthetic provider failure.");
        });
        var usage = new TtsUsageMeter();
        var service = Service(provider, usage);
        var error = await Assert.ThrowsAsync<TtsSynthesisException>(() => Run(service, 1,
            expires: DateTimeOffset.UtcNow.AddMilliseconds(expires ? 100 : 5000)));
        Assert.Equal(expires ? "TTS_CACHE_WINDOW_EXPIRED" : "TEST_FAILURE", error.TechnicalErrorCode);
        RenderedSpeech next = await Run(service, 2);
        Assert.Equal(7, next.Audio!.Segments.Length);
        Assert.Equal(4, calls);
    }

    [Theory]
    [InlineData(0, 30000)]
    [InlineData(33, 30000)]
    [InlineData(8, 9)]
    [InlineData(8, 120001)]
    [Trait("TestId", "UT-TTS-QUEUE-05")]
    public void InvalidQueueBoundsFailStartup(int limit, int timeout)
    {
        var options = new TtsProviderOptions { PreparationQueueLimit = limit, PreparationQueueTimeoutMilliseconds = timeout };
        Assert.True(new TtsProviderOptionsValidator().Validate(null, options).Failed);
    }

    private static SpeechSynthesisService Service(ITtsProvider provider, TtsUsageMeter usage,
        int limit = 8, int queueTimeout = 30000, int timeout = 5000)
    {
        var options = Options.Create(new TtsProviderOptions
        {
            Provider = TtsProviderOptions.ExternalProvider,
            PreparationQueueLimit = limit, PreparationQueueTimeoutMilliseconds = queueTimeout,
            TimeoutMilliseconds = timeout,
            Segmentation = new SpeechSegmentationOptions { Enabled = true, FixedSegments = FixedSegmentSource.Catalog },
            FixedSegments = TargetV1SpeechPolicy.FixedSegmentHashes(TargetV1SpeechPolicy.CanonicalVietnameseTemplate)
                .Select((hash, i) => new FixedSegmentMediaEntry { TextHash = hash, MediaReference = $"sound:fixed-{i}", DurationMilliseconds = 100 }).ToArray(),
        });
        return new SpeechSynthesisService(provider, new NoCache(), new TtsRequestBudget(TimeProvider.System),
            usage, new RegionalVoiceMap(options), options, TimeProvider.System);
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

    private sealed class Provider(Func<SpeechScript, CancellationToken, Task> call) : ITtsProvider
    {
        public async Task<RenderedAudio> SynthesizeAsync(SpeechScript script, TtsOptions options, CancellationToken cancellationToken)
        {
            await call(script, cancellationToken);
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
