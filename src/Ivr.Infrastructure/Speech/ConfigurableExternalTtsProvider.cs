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
/// W-0354 / B16. The order's own data could not be turned into a script — a fractional
/// <c>total_amount</c>, an amount past the speller's range, a value a template refuses.
/// <para>
/// It derives from <see cref="TtsSynthesisException"/> on purpose, so both dispatch gateways map it
/// through the arm they already have for "no audio for this order": <c>AudioError</c> with the
/// channel reported <b>healthy</b>. Before this, the underlying <see cref="ArgumentException"/> fell
/// through to the generic arm, which reports the channel <b>unhealthy</b>; one bad order put a SIM
/// into quarantine, and three in ten minutes disabled it, taking capacity away from every other
/// order. The fault is in the data, and the data travels with the task, not with the SIM.
/// </para>
/// </summary>
public sealed class SpeechRenderRejectedException(Exception innerException)
    : TtsSynthesisException(
        TechnicalCode,
        "The order summary could not be rendered into the approved script.",
        innerException)
{
    public const string TechnicalCode = "SPEECH_RENDER_DATA_REJECTED";
}

/// <summary>
/// HTTP settings for the self-hosted VieNeu-TTS sidecar (W-0122).
/// <para>
/// VieNeu is the only speech engine. The sidecar shares the worker's network namespace, so the
/// endpoint is always loopback and order values never leave the pod. The request shape is data
/// rather than code so the shim's JSON contract lives in one visible place: Helm values and the
/// lab overlay carry it next to the endpoint.
/// </para>
/// </summary>
public sealed class ExternalTtsOptions
{
    /// <summary>Absolute loopback endpoint of the VieNeu sidecar.</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>Header carrying the credential, when the sidecar is given one.</summary>
    public string CredentialHeader { get; set; } = "Authorization";

    /// <summary>
    /// Scheme prefix for the credential, or empty when the header takes a bare key.
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

    /// <summary>Upper bound on a single response body, as a guard against a runaway engine.</summary>
    public int MaxResponseBytes { get; set; } = 4 * 1024 * 1024;

    public override string ToString() => "[REDACTED_EXTERNAL_TTS_OPTIONS]";
}

/// <summary>
/// Client for the VieNeu-TTS sidecar. It sends privacy-safe text, receives raw signed-linear PCM,
/// and writes it to a content-addressed file the media server can play.
/// <para>
/// <b>Raw PCM, not MP3.</b> Asterisk plays <c>.sln</c> family files natively and needs a codec
/// module for anything else, and decoding in-process would put an audio library inside the API.
/// The VieNeu shim therefore returns <c>audio/L16</c> at the configured rate, which keeps the
/// format assumption in one visible place instead of spread across a decode path.
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
            cancellationToken);

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
            cancellationToken);

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
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(options.Timeout);
        cancellationToken = deadline.Token;
        int busyRetries = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
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
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            // VieNeu rejects before inference when its bounded capacity is occupied. A
            // timed-out inference may still be running, so do not spend the scheduler's
            // technical retry on an immediate second 503. Retry only this known transient
            // status; neither HTTP errors nor synthesis timeouts restart the total budget.
            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                response.Dispose();
                int delayMilliseconds = Math.Min(250 << busyRetries, 1000);
                // Saturate the exponent; the shared deadline bounds both retries and body reads.
                // A fixed attempt ceiling used to expire before a cancelled S5 inference drained.
                busyRetries = Math.Min(busyRetries + 1, 2);
                await Task.Delay(delayMilliseconds, cancellationToken);
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                // The status code is the whole diagnostic. An error body can quote the text it was
                // asked to speak, which is order content, so it is never read or logged here.
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
                .ReadAsStreamAsync(cancellationToken);
            using var buffer = new MemoryStream();
            byte[] chunk = new byte[64 * 1024];
            int read;
            while ((read = await body.ReadAsync(chunk, cancellationToken)) > 0)
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
            await File.WriteAllBytesAsync(stagingPath, audio, cancellationToken);
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
