using System.Security.Cryptography;
using System.Text;
using Ivr.Domain.Speech;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Speech;

/// <summary>
/// Network-free MOCK provider that deterministically models a generated PCM tone.
/// </summary>
public sealed class FakeDeterministicTtsProvider(
    IOptions<TtsProviderOptions> providerOptions) : ITtsProvider
{
    public Task<RenderedAudio> SynthesizeAsync(
        SpeechScript script,
        TtsOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(script);
        ArgumentNullException.ThrowIfNull(options);
        TtsProviderOptions configured = providerOptions.Value;
        if (!string.Equals(
                configured.ExecutionMode,
                "MOCK",
                StringComparison.OrdinalIgnoreCase)
            || !string.Equals(
                configured.Provider,
                TtsProviderOptions.FakeProvider,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new TtsProviderNotConfiguredException(
                "The deterministic TTS provider is restricted to MOCK mode.");
        }

        double charactersPerSecond = 14d * (double)options.SpeakingRate;
        TimeSpan duration = TimeSpan.FromSeconds(Math.Clamp(
            script.ExactText.Length / charactersPerSecond,
            1d,
            options.MaxDuration.TotalSeconds));
        int frames = checked((int)Math.Ceiling(
            duration.TotalSeconds * configured.SampleRate));
        string seed = string.Join(
            '\u001f',
            script.ContentHash,
            options.Locale,
            options.VoiceId,
            options.SpeakingRate,
            configured.OutputFormat,
            configured.SampleRate,
            frames);
        byte[] seedHash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        int frequency = 320 + seedHash[0];
        string pcmDigest = ComputeToneDigest(
            frequency,
            configured.SampleRate,
            Math.Min(frames, configured.SampleRate));
        string contentDigest = Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(string.Concat(seed, '\u001f', pcmDigest))))
            .ToLowerInvariant();
        return Task.FromResult(RenderedAudio.Create(
            configured.OutputFormat,
            configured.SampleRate,
            duration,
            string.Concat("memory://tts/fake/", contentDigest)));
    }

    private static string ComputeToneDigest(int frequency, int sampleRate, int frames)
    {
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> sampleBytes = stackalloc byte[2];
        for (int index = 0; index < frames; index++)
        {
            short sample = checked((short)Math.Round(
                Math.Sin(2d * Math.PI * frequency * index / sampleRate) * 8_000d));
            sampleBytes[0] = (byte)(sample & 0xff);
            sampleBytes[1] = (byte)((sample >> 8) & 0xff);
            hash.AppendData(sampleBytes);
        }

        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }
}
