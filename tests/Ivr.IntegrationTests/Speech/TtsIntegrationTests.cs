using Ivr.Domain.Retention;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Retention;
using Ivr.Infrastructure.Speech;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.IntegrationTests.Speech;

[Collection(RetentionJobTestGroup.Name)]
public sealed class TtsAudioCacheIntegrationTests(RetentionJobFixture fixture)
{
    [Fact]
    public async Task FirstWaiterCancellationDoesNotPoisonSharedSynthesis()
    {
        IAudioCache cache = fixture.Services.GetRequiredService<IAudioCache>();
        AudioCacheKey key = AudioCacheKey.Create(
            "SCRIPT-CANCEL-SAFETY",
            "v1",
            "summary-cancel-safety",
            "voice-a",
            "vi-VN");
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int providerCalls = 0;
        async Task<RenderedAudio> Factory(CancellationToken token)
        {
            Interlocked.Increment(ref providerCalls);
            started.SetResult();
            await release.Task.WaitAsync(token);
            return RenderedAudio.Create(
                "audio/L16",
                8_000,
                TimeSpan.FromSeconds(1),
                "memory://tts/cache/cancel-safe");
        }

        using var firstCancellation = new CancellationTokenSource();
        Task<AudioCacheResult> first = cache.GetOrCreateAsync(
            key,
            DateTimeOffset.UtcNow.AddMinutes(1),
            Factory,
            firstCancellation.Token);
        await started.Task;
        firstCancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first);
        Task<AudioCacheResult> second = cache.GetOrCreateAsync(
            key,
            DateTimeOffset.UtcNow.AddMinutes(1),
            Factory,
            CancellationToken.None);
        release.SetResult();

        AudioCacheResult result = await second;
        Assert.True(result.CacheHit);
        Assert.Equal(1, providerCalls);
    }

    [Fact]
    [Trait("TestId", "IT-TTS-CACHE-08")]
    public async Task CacheKeyTtlAndRetentionHookAreBoundedAndDeterministic()
    {
        await fixture.ResetAsync();
        IAudioCache cache = fixture.Services.GetRequiredService<IAudioCache>();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        int providerCalls = 0;
        AudioCacheKey key = AudioCacheKey.Create(
            "SCRIPT-ORDER-CONFIRM",
            "v1",
            "summary-hash",
            "voice-a",
            "vi-VN");
        AudioCacheKey voiceVariant = AudioCacheKey.Create(
            "SCRIPT-ORDER-CONFIRM",
            "v1",
            "summary-hash",
            "voice-b",
            "vi-VN");
        AudioCacheKey versionVariant = AudioCacheKey.Create(
            "SCRIPT-ORDER-CONFIRM",
            "v2",
            "summary-hash",
            "voice-a",
            "vi-VN");
        AudioCacheKey localeVariant = AudioCacheKey.Create(
            "SCRIPT-ORDER-CONFIRM",
            "v1",
            "summary-hash",
            "voice-a",
            "en-US");
        Assert.Equal(4, new[]
        {
            key.CacheId,
            voiceVariant.CacheId,
            versionVariant.CacheId,
            localeVariant.CacheId,
        }.Distinct(StringComparer.Ordinal).Count());
        DateTimeOffset deadline = now.AddSeconds(30);
        Func<CancellationToken, Task<RenderedAudio>> factory = _ =>
        {
            providerCalls++;
            return Task.FromResult(RenderedAudio.Create(
                "audio/L16",
                8_000,
                TimeSpan.FromSeconds(2),
                "memory://tts/cache/integration-safe"));
        };

        AudioCacheResult miss = await cache.GetOrCreateAsync(
            key,
            deadline,
            factory,
            CancellationToken.None);
        AudioCacheResult hit = await cache.GetOrCreateAsync(
            key,
            now.AddMinutes(5),
            factory,
            CancellationToken.None);
        await cache.GetOrCreateAsync(
            voiceVariant,
            deadline,
            factory,
            CancellationToken.None);

        Assert.False(miss.CacheHit);
        Assert.True(hit.CacheHit);
        Assert.Equal(2, providerCalls);
        Assert.True(hit.ExpiresAt <= deadline);
        Assert.DoesNotContain("summary-hash", key.CacheId, StringComparison.Ordinal);
        IRetentionJob retention = fixture.Services.GetRequiredService<IRetentionJob>();
        await retention.RunAsync(
            new RetentionRunOptions(
                DryRun: true,
                DataClasses: [RetentionDataClasses.SpeechSnapshot],
                Now: now.AddMinutes(1)),
            CancellationToken.None);
        Assert.Equal(2, cache.Count);
        await retention.RunAsync(
            new RetentionRunOptions(
                DryRun: false,
                DataClasses: [RetentionDataClasses.SpeechSnapshot],
                Now: now.AddMinutes(1)),
            CancellationToken.None);
        Assert.Equal(0, cache.Count);
    }
}

public sealed class TtsModeIsolationIntegrationTests
{
    [Fact]
    [Trait("TestId", "IT-TTS-MODE-09")]
    public void MockRegistersOnlyNetworkFreeProviderAndRejectsExternalConfiguration()
    {
        IConfiguration configuration = Configuration(new Dictionary<string, string?>
        {
            ["IVR_EXECUTION_MODE"] = IvrOptions.MockExecutionMode,
            ["SALES_PROVIDER"] = "FAKE_TARGET_V1",
            ["SIM_PROVIDER"] = "MOCK",
            ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",
            ["Ivr:Speech:Tts:Provider"] = TtsProviderOptions.FakeProvider,
        });
        var services = new ServiceCollection();
        services.AddIvrFoundation(configuration);
        Assert.DoesNotContain(services, descriptor =>
            descriptor.ServiceType == typeof(ITtsProvider)
            && descriptor.ImplementationType == typeof(ConfigurableExternalTtsProvider));
        using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
        Assert.IsType<FakeDeterministicTtsProvider>(provider.GetRequiredService<ITtsProvider>());
        NoEgressIlGuard.AssertNoCallTargets(typeof(FakeDeterministicTtsProvider));
        TtsProviderOptions bound = provider.GetRequiredService<IOptions<TtsProviderOptions>>().Value;
        Assert.Empty(bound.Endpoint);
        Assert.Empty(bound.Credential);

        IConfiguration unsafeConfiguration = Configuration(new Dictionary<string, string?>
        {
            ["IVR_EXECUTION_MODE"] = IvrOptions.MockExecutionMode,
            ["SALES_PROVIDER"] = "FAKE_TARGET_V1",
            ["SIM_PROVIDER"] = "MOCK",
            ["REAL_CUSTOMER_CALL_ALLOWED"] = "NO",
            ["Ivr:Speech:Tts:Provider"] = TtsProviderOptions.ExternalProvider,
            ["Ivr:Speech:Tts:Endpoint"] = "https://tts.invalid.test",
            ["Ivr:Speech:Tts:Credential"] = "synthetic-secret",
        });
        var unsafeServices = new ServiceCollection();
        unsafeServices.AddIvrFoundation(unsafeConfiguration);
        using ServiceProvider unsafeProvider = unsafeServices.BuildServiceProvider(validateScopes: true);
        Assert.Throws<OptionsValidationException>(() =>
            _ = unsafeProvider.GetRequiredService<IOptions<TtsProviderOptions>>().Value);
    }

    private static IConfiguration Configuration(
        IReadOnlyDictionary<string, string?> values) => new ConfigurationBuilder()
        .AddInMemoryCollection(values)
        .Build();
}
