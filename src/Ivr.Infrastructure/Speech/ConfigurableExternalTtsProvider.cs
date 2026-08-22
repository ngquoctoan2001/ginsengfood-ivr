using System.Collections.Frozen;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ivr.Domain.Speech;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Speech;

public class TtsSynthesisException(
    string technicalErrorCode,
    string message,
    Exception? innerException = null) : Exception(message, innerException)
{
    public string TechnicalErrorCode { get; } = technicalErrorCode;
}

public sealed class TtsProviderNotConfiguredException(string message)
    : TtsSynthesisException("TTS_NOT_CONFIGURED", message);

/// <summary>
/// Vendor-neutral HTTP settings for the external synthesizer.
/// <para>
/// No vendor name appears anywhere in this codebase, and that is deliberate rather than tidy:
/// <c>OD-VOICE-01</c> reversed direction three times, and each reversal would have been a code
/// change if the protocol had been written against one vendor's SDK. What changes here is a
/// request body string and a credential.
/// </para>
/// </summary>
public sealed class ExternalTtsOptions
{
    /// <summary>Absolute endpoint. Plain HTTP is accepted only against a loopback host.</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>Header carrying the credential. Vendors differ; the header name is data.</summary>
    public string CredentialHeader { get; set; } = "Authorization";

    /// <summary>
    /// Scheme prefix for the credential, or empty for vendors whose header takes a bare key.
    /// </summary>
    public string CredentialScheme { get; set; } = "Bearer";

    /// <summary>
    /// JSON request body with <c>{{text}}</c>, <c>{{voice_id}}</c>, <c>{{locale}}</c>,
    /// <c>{{speaking_rate}}</c>, <c>{{output_format}}</c> and <c>{{sample_rate}}</c> tokens.
    /// Substituted values are JSON-escaped before they are inserted.
    /// </summary>
    public string RequestBodyTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Directory the returned audio is written to. It has to be a path the media server can
    /// read, which in the lab is the volume Asterisk mounts for its sounds.
    /// </summary>
    public string MediaOutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Asterisk sound-reference prefix for generated files, without the content hash.
    /// </summary>
    public string MediaReferencePrefix { get; set; } = "sound:ivr-dyn-";

    /// <summary>Upper bound on a single response body, as a guard against a runaway vendor.</summary>
    public int MaxResponseBytes { get; set; } = 4 * 1024 * 1024;

    public override string ToString() => "[REDACTED_EXTERNAL_TTS_OPTIONS]";
}

/// <summary>
/// Generic HTTP synthesizer. It sends privacy-safe text, receives raw signed-linear PCM, and
/// writes it to a content-addressed file the media server can play.
/// <para>
/// <b>Raw PCM, not MP3.</b> Asterisk plays <c>.sln</c> family files natively and needs a codec
/// module for anything else, and decoding in-process would put an audio library inside the API.
/// A vendor that cannot emit PCM at the configured rate belongs behind a converting sidecar —
/// that is a deployment answer, and it keeps the format assumption in one visible place instead
/// of spread across a decode path.
/// </para>
/// <para>
/// <b>Content-addressed filenames.</b> The same sentence in the same voice always lands on the
/// same file, so concurrent calls converge instead of racing, and the audio cache and the disk
/// agree without a second index. Files are removed by
/// <see cref="SpeechMediaFileRetentionHook"/>, not by this class.
/// </para>
/// </summary>
public sealed class ConfigurableExternalTtsProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<TtsProviderOptions> providerOptions) : ITtsProvider
{
    public const string HttpClientName = "ivr-tts-external";

    /// <summary>
    /// Sample rates Asterisk has a raw signed-linear extension for. A rate outside this list has
    /// no playable raw container, so it is refused at configuration time rather than producing a
    /// file the media server silently skips.
    /// </summary>
    private static readonly FrozenDictionary<int, string> RawPcmExtensions =
        new Dictionary<int, string>
        {
            [8_000] = ".sln",
            [12_000] = ".sln12",
            [16_000] = ".sln16",
            [24_000] = ".sln24",
            [32_000] = ".sln32",
            [44_100] = ".sln44",
            [48_000] = ".sln48",
        }.ToFrozenDictionary();

    public const string RequiredOutputFormat = "audio/L16";

    public static bool IsSupportedSampleRate(int sampleRate) =>
        RawPcmExtensions.ContainsKey(sampleRate);

    public async Task<RenderedAudio> SynthesizeAsync(
        SpeechScript script,
        TtsOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(script);
        ArgumentNullException.ThrowIfNull(options);
        TtsProviderOptions configured = providerOptions.Value;
        ExternalTtsOptions external = configured.External;
        if (string.IsNullOrWhiteSpace(external.Endpoint)
            || string.IsNullOrWhiteSpace(external.RequestBodyTemplate)
            || string.IsNullOrWhiteSpace(external.MediaOutputDirectory))
        {
            throw new TtsProviderNotConfiguredException(
                "The external TTS provider needs an endpoint, a request body template and a media directory.");
        }

        if (!RawPcmExtensions.TryGetValue(configured.SampleRate, out string? extension))
        {
            throw new TtsProviderNotConfiguredException(
                "The configured sample rate has no raw signed-linear container.");
        }

        byte[] audio = await RequestAudioAsync(
            script,
            options,
            configured,
            external,
            cancellationToken).ConfigureAwait(false);

        // Two bytes per frame, one channel. Anything that does not divide evenly is not the PCM
        // this provider requires, and guessing a duration from a truncated body would hand the
        // dialplan a length the audio does not have.
        if (audio.Length == 0 || audio.Length % 2 != 0)
        {
            throw new TtsSynthesisException(
                "TTS_AUDIO_NOT_PCM",
                "The TTS provider returned a body that is not 16-bit mono PCM.");
        }

        TimeSpan duration = TimeSpan.FromSeconds(
            audio.Length / 2d / configured.SampleRate);
        if (duration > options.MaxDuration)
        {
            throw new TtsSynthesisException(
                "TTS_MAX_DURATION_EXCEEDED",
                "The synthesized audio exceeds the configured duration bound.");
        }

        string contentDigest = Convert.ToHexString(SHA256.HashData(audio))
            .ToLowerInvariant()[..32];
        string fileName = string.Concat(contentDigest, extension);
        await WriteMediaAsync(
            external.MediaOutputDirectory,
            fileName,
            audio,
            cancellationToken).ConfigureAwait(false);

        return RenderedAudio.Create(
            configured.OutputFormat,
            configured.SampleRate,
            duration,
            string.Concat(external.MediaReferencePrefix, contentDigest));
    }

    private async Task<byte[]> RequestAudioAsync(
        SpeechScript script,
        TtsOptions options,
        TtsProviderOptions configured,
        ExternalTtsOptions external,
        CancellationToken cancellationToken)
    {
        HttpClient client = httpClientFactory.CreateClient(HttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Post, external.Endpoint)
        {
            Content = new StringContent(
                BuildRequestBody(external.RequestBodyTemplate, script, options, configured),
                Encoding.UTF8,
                "application/json"),
        };
        if (!string.IsNullOrWhiteSpace(configured.Credential))
        {
            if (string.IsNullOrWhiteSpace(external.CredentialScheme))
            {
                request.Headers.TryAddWithoutValidation(
                    external.CredentialHeader,
                    configured.Credential);
            }
            else
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    external.CredentialScheme,
                    configured.Credential);
            }
        }

        using HttpResponseMessage response = await client
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            // The status code is the whole diagnostic. A vendor error body can quote the text it
            // was asked to speak, which is order content, so it is never read or logged here.
            throw new TtsSynthesisException(
                "TTS_PROVIDER_HTTP_ERROR",
                $"The TTS provider returned HTTP {(int)response.StatusCode}.");
        }

        long? declaredLength = response.Content.Headers.ContentLength;
        if (declaredLength > external.MaxResponseBytes)
        {
            throw new TtsSynthesisException(
                "TTS_AUDIO_TOO_LARGE",
                "The TTS provider declared a response larger than the configured bound.");
        }

        using Stream body = await response.Content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        using var buffer = new MemoryStream();
        byte[] chunk = new byte[64 * 1024];
        int read;
        while ((read = await body.ReadAsync(chunk, cancellationToken).ConfigureAwait(false)) > 0)
        {
            if (buffer.Length + read > external.MaxResponseBytes)
            {
                // Checked while streaming as well as from the header: a chunked response
                // declares no length, and the bound has to hold for the case that omits it.
                throw new TtsSynthesisException(
                    "TTS_AUDIO_TOO_LARGE",
                    "The TTS provider streamed more audio than the configured bound.");
            }

            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }

    /// <summary>
    /// Substitutes request values into the configured JSON body. Every value is JSON-escaped, so
    /// a product name containing a quote cannot restructure the request.
    /// </summary>
    internal static string BuildRequestBody(
        string template,
        SpeechScript script,
        TtsOptions options,
        TtsProviderOptions configured) => template
        .Replace("{{text}}", JsonEscape(script.ExactText), StringComparison.Ordinal)
        .Replace("{{voice_id}}", JsonEscape(options.VoiceId), StringComparison.Ordinal)
        .Replace("{{locale}}", JsonEscape(options.Locale), StringComparison.Ordinal)
        .Replace(
            "{{speaking_rate}}",
            options.SpeakingRate.ToString(System.Globalization.CultureInfo.InvariantCulture),
            StringComparison.Ordinal)
        .Replace("{{output_format}}", JsonEscape(configured.OutputFormat), StringComparison.Ordinal)
        .Replace(
            "{{sample_rate}}",
            configured.SampleRate.ToString(System.Globalization.CultureInfo.InvariantCulture),
            StringComparison.Ordinal);

    private static string JsonEscape(string value)
    {
        string encoded = JsonSerializer.Serialize(value);

        // Serialize returns the value with its surrounding quotes; the template supplies those.
        return encoded[1..^1];
    }

    private static async Task WriteMediaAsync(
        string directory,
        string fileName,
        byte[] audio,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(directory);
        string finalPath = Path.Combine(directory, fileName);
        if (File.Exists(finalPath))
        {
            // Content-addressed: an existing file with this name already holds these bytes.
            return;
        }

        // Written beside the target and moved into place, so a crash mid-write cannot leave a
        // truncated file under a name the cache will happily hand to the dialplan forever.
        string stagingPath = string.Concat(finalPath, ".", Guid.NewGuid().ToString("N"), ".tmp");
        try
        {
            await File.WriteAllBytesAsync(stagingPath, audio, cancellationToken)
                .ConfigureAwait(false);
            File.Move(stagingPath, finalPath, overwrite: false);
        }
        catch (IOException) when (File.Exists(finalPath))
        {
            // Another call wrote the same content first. Identical bytes, so nothing to do.
        }
        finally
        {
            if (File.Exists(stagingPath))
            {
                File.Delete(stagingPath);
            }
        }
    }
}
