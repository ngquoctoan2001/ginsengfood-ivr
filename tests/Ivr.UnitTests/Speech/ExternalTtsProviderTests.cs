using System.Net;
using System.Text;
using Ivr.Domain.Scripts;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Speech;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Speech;

/// <summary>
/// The external synthesizer and the startup gates around it.
/// <para>
/// This provider used to be a seam that threw. Making it speak HTTP put three new ways to be
/// wrong into the call path — a body the vendor mangles, audio the media server cannot play, and
/// a duration nobody measured — so each is pinned here rather than left to the lab to discover.
/// </para>
/// </summary>
public sealed class ExternalTtsProviderTests : IDisposable
{
    private readonly string mediaDirectory = Path.Combine(
        Path.GetTempPath(),
        "ivr-tts-tests",
        Guid.NewGuid().ToString("N"));

    private readonly List<CapturingHandler> handlers = [];

    /// <summary>
    /// PCM in, playable reference out, and a duration derived from the byte count rather than
    /// guessed. One second at 8 kHz mono 16-bit is exactly 16 000 bytes.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TTS-EXT-PCM-01")]
    public async Task PcmResponseBecomesAContentAddressedPlayableFile()
    {
        byte[] audio = new byte[16_000];
        Random.Shared.NextBytes(audio);
        CapturingHandler handler = Handler(audio);
        ConfigurableExternalTtsProvider provider = CreateProvider(handler);

        RenderedAudio rendered = await provider.SynthesizeAsync(
            Script("Năm trăm sáu mươi nghìn đồng."),
            TtsOptions.Create(voiceId: "voice-north"),
            CancellationToken.None);

        Assert.Equal(TimeSpan.FromSeconds(1), rendered.Duration);
        Assert.StartsWith("sound:ivr-dyn-", rendered.ContentRef, StringComparison.Ordinal);
        Assert.Single(rendered.Segments);

        string digest = rendered.ContentRef["sound:ivr-dyn-".Length..];
        string written = Path.Combine(mediaDirectory, digest + ".sln");
        Assert.True(File.Exists(written), $"Expected the provider to write {written}.");
        Assert.Equal(audio, await File.ReadAllBytesAsync(written));

        // Content-addressed: the same sentence resolves to the same file rather than piling up
        // one copy per call.
        RenderedAudio again = await provider.SynthesizeAsync(
            Script("Năm trăm sáu mươi nghìn đồng."),
            TtsOptions.Create(voiceId: "voice-north"),
            CancellationToken.None);
        Assert.Equal(rendered.ContentRef, again.ContentRef);
        Assert.Single(Directory.GetFiles(mediaDirectory));
    }

    /// <summary>
    /// The configured body is a string, so a product name containing a quote must not be able to
    /// restructure the JSON around it.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TTS-EXT-ESCAPE-02")]
    public async Task RequestValuesAreJsonEscapedBeforeTheyEnterTheBody()
    {
        CapturingHandler handler = Handler(new byte[16_000]);
        ConfigurableExternalTtsProvider provider = CreateProvider(handler);

        await provider.SynthesizeAsync(
            Script("Sản phẩm \"đặc biệt\" của Ginsengfood."),
            TtsOptions.Create(voiceId: "voice-north"),
            CancellationToken.None);

        Assert.NotNull(handler.RequestBody);
        Assert.Contains("\"voice\":\"voice-north\"", handler.RequestBody, StringComparison.Ordinal);
        Assert.Contains("\"rate\":8000", handler.RequestBody, StringComparison.Ordinal);

        // The customer-facing quote leaves as \\u0022 and the Vietnamese as \\uXXXX. That is the
        // default serializer's strict encoder, and it is worth pinning rather than treating as
        // incidental: the escaped quote cannot terminate the string it sits in, and the escaped
        // diacritics survive a transport that mangles non-ASCII.
        Assert.Contains("\\u0022", handler.RequestBody, StringComparison.Ordinal);
        Assert.DoesNotContain("đặc biệt", handler.RequestBody, StringComparison.Ordinal);

        // The property actually being claimed: a quote inside a product name does not restructure
        // the request, and the vendor still receives the exact sentence.
        using System.Text.Json.JsonDocument parsed =
            System.Text.Json.JsonDocument.Parse(handler.RequestBody!);
        Assert.Equal(
            "Sản phẩm \"đặc biệt\" của Ginsengfood.",
            parsed.RootElement.GetProperty("text").GetString());
    }

    /// <summary>
    /// An odd byte count is not 16-bit mono PCM. Accepting it would hand the dialplan a file of
    /// the wrong length and a duration computed from a body that was cut short.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TTS-EXT-NOTPCM-03")]
    public async Task ABodyThatIsNotSixteenBitPcmIsRefused()
    {
        ConfigurableExternalTtsProvider provider = CreateProvider(
            Handler(new byte[15_999]));

        TtsSynthesisException failure = await Assert.ThrowsAsync<TtsSynthesisException>(() =>
            provider.SynthesizeAsync(
                Script("Nội dung an toàn."),
                TtsOptions.Create(),
                CancellationToken.None));

        Assert.Equal("TTS_AUDIO_NOT_PCM", failure.TechnicalErrorCode);
        Assert.False(Directory.Exists(mediaDirectory) && Directory.GetFiles(mediaDirectory).Length > 0);
    }

    /// <summary>
    /// A vendor error carries the status code and nothing else: its body can quote the text it
    /// was asked to speak, which is order content.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TTS-EXT-HTTPERROR-04")]
    public async Task AVendorErrorReportsTheStatusCodeAndNotTheResponseBody()
    {
        ConfigurableExternalTtsProvider provider = CreateProvider(Handler(
            [],
            HttpStatusCode.PaymentRequired,
            "quota exceeded rendering: Chị Mai, đơn DH-01"));

        TtsSynthesisException failure = await Assert.ThrowsAsync<TtsSynthesisException>(() =>
            provider.SynthesizeAsync(
                Script("Nội dung an toàn."),
                TtsOptions.Create(),
                CancellationToken.None));

        Assert.Equal("TTS_PROVIDER_HTTP_ERROR", failure.TechnicalErrorCode);
        Assert.Contains("402", failure.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("Chị Mai", failure.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// A streamed response declares no length, so the bound has to hold while reading too.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TTS-EXT-TOOLARGE-05")]
    public async Task AResponseLargerThanTheConfiguredBoundIsRefused()
    {
        TtsProviderOptions configured = Configured();
        configured.External.MaxResponseBytes = 2_048;
        ConfigurableExternalTtsProvider provider = CreateProvider(
            Handler(new byte[16_000]),
            configured);

        TtsSynthesisException failure = await Assert.ThrowsAsync<TtsSynthesisException>(() =>
            provider.SynthesizeAsync(
                Script("Nội dung an toàn."),
                TtsOptions.Create(),
                CancellationToken.None));

        Assert.Equal("TTS_AUDIO_TOO_LARGE", failure.TechnicalErrorCode);
    }

    /// <summary>
    /// Order values would travel in clear text over plain HTTP to another host. Loopback stays
    /// allowed because that is how a format-converting sidecar is reached.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-TTS-EXT-CFG-06")]
    public void ExternalConfigurationIsCheckedAtStartup()
    {
        var validator = new TtsProviderOptionsValidator();

        Assert.True(validator.Validate(null, Configured(external =>
            external.Endpoint = "http://tts.example.com/v1/speak")).Failed);
        Assert.True(validator.Validate(null, Configured(external =>
            external.Endpoint = "http://127.0.0.1:8080/v1/speak")).Succeeded);
        Assert.True(validator.Validate(null, Configured(external =>
            external.RequestBodyTemplate = "{\"voice\":\"{{voice_id}}\"}")).Failed);
        Assert.True(validator.Validate(null, Configured(external =>
            external.MediaOutputDirectory = string.Empty)).Failed);
        Assert.True(validator.Validate(null, Configured(external =>
            external.MediaReferencePrefix = "http://elsewhere/")).Failed);

        TtsProviderOptions wrongRate = Configured();
        wrongRate.SampleRate = 22_050;
        Assert.True(validator.Validate(null, wrongRate).Failed);

        TtsProviderOptions wrongFormat = Configured();
        wrongFormat.OutputFormat = "audio/mpeg";
        Assert.True(validator.Validate(null, wrongFormat).Failed);
    }

    /// <summary>
    /// Turning segmentation on with an incomplete catalog must stop the deployment. The
    /// alternative is finding out on the first call of the day, mid-conversation.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SEG-FAILSTART-07")]
    public void AnIncompleteRecordingCatalogFailsStartup()
    {
        var validator = new TtsProviderOptionsValidator();
        var required = TargetV1SpeechPolicy.FixedSegmentHashes(
            TargetV1SpeechPolicy.CanonicalVietnameseTemplate);

        TtsProviderOptions complete = SegmentedOptions();
        complete.FixedSegments =
        [
            .. required.Select((hash, index) => new FixedSegmentMediaEntry
            {
                TextHash = hash,
                MediaReference = $"sound:ivr-fixed-{index}",
                DurationMilliseconds = 2_000,
            }),
        ];
        Assert.True(validator.Validate(null, complete).Succeeded);

        TtsProviderOptions missingOne = SegmentedOptions();
        missingOne.FixedSegments = complete.FixedSegments[..^1];
        ValidateOptionsResult failed = validator.Validate(null, missingOne);
        Assert.True(failed.Failed);
        Assert.Contains(
            failed.Failures!,
            failure => failure.Contains("missing a recording", StringComparison.Ordinal));

        // A recorded reference that is not a sound reference cannot be played at all.
        TtsProviderOptions badReference = SegmentedOptions();
        badReference.FixedSegments =
        [
            .. complete.FixedSegments.Select(entry => new FixedSegmentMediaEntry
            {
                TextHash = entry.TextHash,
                MediaReference = "https://cdn.example.com/greeting.mp3",
                DurationMilliseconds = entry.DurationMilliseconds,
            }),
        ];
        Assert.True(validator.Validate(null, badReference).Failed);
    }

    /// <summary>
    /// Synthesizing the fixed prose is a MOCK convenience. Against a real vendor it re-buys the
    /// same 203 characters on every cold cache, which is the cost difference the hybrid exists
    /// to capture.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SEG-FAILSTART-08")]
    public void SynthesizingFixedProseIsRestrictedToTheMockProvider()
    {
        var validator = new TtsProviderOptionsValidator();

        TtsProviderOptions mock = new()
        {
            ExecutionMode = "MOCK",
            Provider = TtsProviderOptions.FakeProvider,
            Segmentation = new SpeechSegmentationOptions
            {
                Enabled = true,
                FixedSegments = FixedSegmentSource.Provider,
            },
        };
        Assert.True(validator.Validate(null, mock).Succeeded);

        TtsProviderOptions external = Configured();
        external.Segmentation = new SpeechSegmentationOptions
        {
            Enabled = true,
            FixedSegments = FixedSegmentSource.Provider,
        };
        ValidateOptionsResult result = validator.Validate(null, external);
        Assert.True(result.Failed);
        Assert.Contains(
            result.Failures!,
            failure => failure.Contains("recorded catalog", StringComparison.Ordinal));
    }

    /// <summary>
    /// Generated audio outlives the in-memory cache entry that referenced it, and it speaks
    /// order values. Without a sweep the media directory grows for the life of the deployment.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SEG-RETENTION-09")]
    public async Task GeneratedMediaIsPurgedOnceItLeavesTheRetentionWindow()
    {
        TtsProviderOptions configured = Configured();
        configured.SpeechSnapshotRetentionSeconds = 900;
        ConfigurableExternalTtsProvider provider = CreateProvider(
            Handler(new byte[16_000]),
            configured);
        await provider.SynthesizeAsync(
            Script("Nội dung an toàn."),
            TtsOptions.Create(),
            CancellationToken.None);
        Assert.Single(Directory.GetFiles(mediaDirectory));

        var hook = new SpeechMediaFileRetentionHook(Microsoft.Extensions.Options.Options.Create(configured));
        DateTimeOffset now = DateTimeOffset.UtcNow;

        Assert.Equal(0, await hook.PurgeExpiredAsync(now, dryRun: false, CancellationToken.None));
        Assert.Single(Directory.GetFiles(mediaDirectory));

        DateTimeOffset later = now.AddSeconds(configured.SpeechSnapshotRetentionSeconds + 60);
        Assert.Equal(1, await hook.PurgeExpiredAsync(later, dryRun: true, CancellationToken.None));
        Assert.Single(Directory.GetFiles(mediaDirectory));

        Assert.Equal(1, await hook.PurgeExpiredAsync(later, dryRun: false, CancellationToken.None));
        Assert.Empty(Directory.GetFiles(mediaDirectory));
    }

    public void Dispose()
    {
        foreach (CapturingHandler handler in handlers)
        {
            handler.Dispose();
        }

        if (Directory.Exists(mediaDirectory))
        {
            Directory.Delete(mediaDirectory, recursive: true);
        }
    }

    private CapturingHandler Handler(
        byte[] audio,
        HttpStatusCode status = HttpStatusCode.OK,
        string? errorBody = null)
    {
        CapturingHandler handler = new(audio, status, errorBody);
        handlers.Add(handler);
        return handler;
    }

    private static SpeechScript Script(string text) => SpeechScript.Create(
        TargetV1SpeechPolicy.MockTemplateId,
        TargetV1SpeechPolicy.MockTemplateVersion,
        text,
        "content-hash",
        "summary-hash");

    private TtsProviderOptions Configured(Action<ExternalTtsOptions>? configure = null)
    {
        TtsProviderOptions configured = new()
        {
            ExecutionMode = "LAB_REAL_SIM",
            Provider = TtsProviderOptions.ExternalProvider,
            OutputFormat = ConfigurableExternalTtsProvider.RequiredOutputFormat,
            SampleRate = 8_000,
            Credential = "test-credential",
            External = new ExternalTtsOptions
            {
                Endpoint = "https://tts.example.com/v1/speak",
                RequestBodyTemplate =
                    "{\"text\":\"{{text}}\",\"voice\":\"{{voice_id}}\",\"rate\":{{sample_rate}}}",
                MediaOutputDirectory = mediaDirectory,
            },
        };
        configure?.Invoke(configured.External);
        return configured;
    }

    private TtsProviderOptions SegmentedOptions()
    {
        TtsProviderOptions configured = Configured();
        configured.Segmentation = new SpeechSegmentationOptions
        {
            Enabled = true,
            FixedSegments = FixedSegmentSource.Catalog,
        };
        return configured;
    }

    private ConfigurableExternalTtsProvider CreateProvider(
        CapturingHandler handler,
        TtsProviderOptions? configured = null) => new(
        new StubHttpClientFactory(handler),
        Microsoft.Extensions.Options.Options.Create(configured ?? Configured()));

    private sealed class StubHttpClientFactory(CapturingHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class CapturingHandler(
        byte[] audio,
        HttpStatusCode status = HttpStatusCode.OK,
        string? errorBody = null) : HttpMessageHandler
    {
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(status)
            {
                Content = status == HttpStatusCode.OK
                    ? new ByteArrayContent(audio)
                    : new StringContent(errorBody ?? string.Empty, Encoding.UTF8),
            };
        }
    }
}
