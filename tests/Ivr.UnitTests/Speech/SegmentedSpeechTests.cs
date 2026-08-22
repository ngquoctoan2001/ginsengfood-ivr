using System.Collections.Immutable;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Scripts;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Providers.Fakes;
using Ivr.Infrastructure.Speech;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Speech;

/// <summary>
/// Hybrid segmented playback (W-0106 §4.6 / A1).
/// <para>
/// Before this, a call played one file. In the lab that file was a generic recording, so a green
/// "the call connected and the customer pressed 1" proved the dial path and proved nothing about
/// whether the customer heard their own order. These tests are written against that specific
/// confusion: the load-bearing assertion is that two different orders produce two different
/// sequences of audio.
/// </para>
/// </summary>
public sealed class SegmentedSpeechTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 22, 9, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// The canonical template splits into four spoken-prose pieces and three order values —
    /// the 4 x 3 regions = 12 recordings W-0106 §4.6 budgets for.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SEG-SPLIT-01")]
    public void CanonicalTemplateSplitsIntoFourFixedAndThreeVariablePieces()
    {
        ImmutableArray<SpeechSegmentTemplate> pieces = TargetV1SpeechPolicy.SegmentTemplate(
            TargetV1SpeechPolicy.CanonicalVietnameseTemplate);

        Assert.Equal(7, pieces.Length);
        Assert.Equal(4, pieces.Count(piece => piece.Kind == SpeechSegmentKind.Fixed));
        Assert.Equal(
            ["items_spoken", "total_amount_display", "delivery_area_short"],
            pieces
                .Where(piece => piece.Kind == SpeechSegmentKind.Dynamic)
                .Select(piece => piece.PlaceholderName));
        Assert.Equal(Enumerable.Range(1, 7), pieces.Select(piece => piece.Ordinal));

        // The fixed share is what makes the recorded-catalog design worth building: those
        // characters are paid for once, ever, instead of on every call.
        int fixedCharacters = pieces
            .Where(piece => piece.Kind == SpeechSegmentKind.Fixed)
            .Sum(piece => piece.Text.Length);
        Assert.True(
            fixedCharacters > TargetV1SpeechPolicy.CanonicalVietnameseTemplate.Length / 2,
            $"Expected most of the template to be fixed prose, got {fixedCharacters} characters.");

        Assert.Equal(4, TargetV1SpeechPolicy
            .FixedSegmentHashes(TargetV1SpeechPolicy.CanonicalVietnameseTemplate)
            .Length);
    }

    /// <summary>
    /// Editing one word of the template has to move the identity of the piece that contains it,
    /// or a recording of the old wording keeps resolving and the customer keeps hearing it.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SEG-TEMPLATEDRIFT-02")]
    public void ChangingTheTemplateWordingChangesTheFixedSegmentIdentities()
    {
        ImmutableArray<string> canonical = TargetV1SpeechPolicy.FixedSegmentHashes(
            TargetV1SpeechPolicy.CanonicalVietnameseTemplate);
        ImmutableArray<string> edited = TargetV1SpeechPolicy.FixedSegmentHashes(
            TargetV1SpeechPolicy.CanonicalVietnameseTemplate.Replace(
                "tổng tiền",
                "tổng cộng",
                StringComparison.Ordinal));

        Assert.Equal(canonical.Length, edited.Length);
        Assert.NotEqual(canonical[1], edited[1]);
        Assert.Equal(canonical[0], edited[0]);
    }

    /// <summary>
    /// Two variables separated only by a space would produce a fixed piece that is a recording
    /// of a space. Refused at the template, not papered over at render time.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SEG-ADJACENT-03")]
    public void AdjacentTemplateVariablesAreRefused()
    {
        string template =
            "Xin chào. Đơn hàng gồm {{items_spoken}} {{total_amount_display}}, "
            + "giao đến {{delivery_area_short}}. "
            + "Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng.";

        InvalidOperationException failure = Assert.Throws<InvalidOperationException>(
            () => TargetV1SpeechPolicy.SegmentTemplate(template));

        Assert.Contains("whitespace", failure.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The acceptance criterion for A1, stated as directly as it can be: different orders must
    /// not produce the same call.
    /// <para>
    /// Compared on the playlist hash rather than on the first reference, because the first
    /// reference is the greeting and the greeting is identical for everyone — comparing it would
    /// pass even if the pipeline played a generic recording to both customers, which is the exact
    /// defect this work item exists to remove.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SEG-PLAYLIST-04")]
    public async Task TwoDifferentOrdersProduceTwoDifferentAudioSequences()
    {
        SpeechSynthesisService service = CreateService(out TtsUsageMeter usage);
        PrivacySafeOrderSummary first = Summary([SpeechItem.Create("Sâm lát", 1, "hộp")], 560_000m);
        PrivacySafeOrderSummary second = Summary([SpeechItem.Create("Trà sâm", 3, "gói")], 1_250_000m);

        RenderedSpeech firstCall = await SynthesizeAsync(service, first);
        RenderedSpeech secondCall = await SynthesizeAsync(service, second);

        Assert.NotNull(firstCall.Audio);
        Assert.NotNull(secondCall.Audio);
        Assert.True(firstCall.Audio.IsPlaylist);
        Assert.Equal(7, firstCall.Audio.Segments.Length);
        Assert.NotEqual(secondCall.Audio.PlaylistHash, firstCall.Audio.PlaylistHash);

        // The greeting really is shared, which is why the first reference cannot be the check.
        Assert.Equal(secondCall.Audio.Segments[0].ContentRef, firstCall.Audio.Segments[0].ContentRef);
        Assert.NotEqual(secondCall.Audio.Segments[1].ContentRef, firstCall.Audio.Segments[1].ContentRef);

        // Every piece is traceable to the sentence it speaks, so "this file says that" is
        // checkable after the fact rather than assumed.
        Assert.All(
            firstCall.Audio.Segments,
            segment => Assert.Equal(64, segment.SegmentHash.Length));
        Assert.Equal(
            firstCall.Audio.Duration,
            firstCall.Audio.Segments.Aggregate(TimeSpan.Zero, (total, s) => total + s.Duration));
        // Nine, not fourteen: the second order re-used the four prose pieces and the delivery
        // area it shares with the first, and only paid for its own items and total.
        Assert.Equal(9, usage.SegmentSnapshot().DynamicSynthesized);
        Assert.Equal(5, usage.SegmentSnapshot().DynamicFromCache);
    }

    /// <summary>
    /// Warm cache, second identical order: no vendor call at all. This is the number the cost
    /// model in W-0106 §4.6 rests on, so it is asserted rather than assumed.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SEG-CACHE-05")]
    public async Task RepeatingAnOrderCallsTheProviderZeroTimes()
    {
        var provider = new CountingTtsProvider();
        SpeechSynthesisService service = CreateService(out TtsUsageMeter usage, provider: provider);
        PrivacySafeOrderSummary summary = Summary([SpeechItem.Create("Sâm lát", 1, "hộp")], 560_000m);

        RenderedSpeech first = await SynthesizeAsync(service, summary);
        int afterFirst = provider.Calls;
        RenderedSpeech replay = await SynthesizeAsync(service, summary);

        Assert.Equal(7, afterFirst);
        Assert.Equal(afterFirst, provider.Calls);
        Assert.Equal(first.Audio!.PlaylistHash, replay.Audio!.PlaylistHash);
        Assert.Equal(7, usage.SegmentSnapshot().DynamicFromCache);
    }

    /// <summary>
    /// The property whole-call caching cannot have: two orders with nothing in common except a
    /// delivery ward still share that piece.
    /// <para>
    /// A cache keyed on the whole-call summary hash treats these as unrelated and pays for the
    /// same sentence twice. That difference is the entire argument for keying by content.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SEG-CACHESHARE-06")]
    public async Task DifferentOrdersShareTheSentencesTheyHaveInCommon()
    {
        var provider = new CountingTtsProvider();
        SpeechSynthesisService service = CreateService(out _, provider: provider);

        await SynthesizeAsync(service, Summary([SpeechItem.Create("Sâm lát", 1, "hộp")], 560_000m));
        int afterFirst = provider.Calls;
        await SynthesizeAsync(service, Summary([SpeechItem.Create("Trà sâm", 3, "gói")], 1_250_000m));

        // Four prose pieces plus the shared delivery area; only items and total are new.
        Assert.Equal(7, afterFirst);
        Assert.Equal(afterFirst + 2, provider.Calls);
    }

    /// <summary>
    /// A missing recording must stop the call, not shorten it.
    /// <para>
    /// Playing the pieces that did resolve produces a call that sounds complete and states a
    /// different order — opening, silence where the items were, then a total. A customer presses
    /// 1 on that. A technical failure gets retried; a wrong confirmation gets acted on.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SEG-MISSING-07")]
    public async Task AMissingRecordingFailsTheCallInsteadOfPlayingPartOfIt()
    {
        ImmutableArray<string> required = TargetV1SpeechPolicy.FixedSegmentHashes(
            TargetV1SpeechPolicy.CanonicalVietnameseTemplate);
        TtsProviderOptions configured = SegmentedOptions(FixedSegmentSource.Catalog);

        // Everything recorded except the closing instruction — the piece that tells the customer
        // which key confirms.
        configured.FixedSegments =
        [
            .. required
                .Take(required.Length - 1)
                .Select((hash, index) => new FixedSegmentMediaEntry
                {
                    TextHash = hash,
                    MediaReference = $"sound:ivr-fixed-{index}",
                    DurationMilliseconds = 2_000,
                }),
        ];
        SpeechSynthesisService service = CreateService(out _, configured: configured);

        TtsSynthesisException failure = await Assert.ThrowsAsync<TtsSynthesisException>(
            () => SynthesizeAsync(service, Summary([SpeechItem.Create("Sâm lát", 1, "hộp")], 560_000m)));

        Assert.Equal("TTS_FIXED_SEGMENT_NOT_RECORDED", failure.TechnicalErrorCode);
    }

    /// <summary>
    /// With a complete catalog the prose costs nothing at runtime, and only the order's own
    /// values reach the synthesizer.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SEG-CATALOG-08")]
    public async Task RecordedProseIsPlayedFromTheCatalogAndNeverSynthesized()
    {
        TtsProviderOptions configured = SegmentedOptions(FixedSegmentSource.Catalog);
        configured.FixedSegments = FullCatalog();
        var provider = new CountingTtsProvider();
        SpeechSynthesisService service = CreateService(
            out TtsUsageMeter usage,
            provider: provider,
            configured: configured);

        RenderedSpeech call = await SynthesizeAsync(
            service,
            Summary([SpeechItem.Create("Sâm lát", 1, "hộp")], 560_000m));

        Assert.Equal(3, provider.Calls);
        TtsSegmentSnapshot segments = usage.SegmentSnapshot();
        Assert.Equal(4, segments.FixedFromCatalog);
        Assert.Equal(3, segments.DynamicSynthesized);
        Assert.StartsWith("sound:ivr-fixed-", call.Audio!.Segments[0].ContentRef, StringComparison.Ordinal);

        // Only the order values were sent anywhere. The prose stayed on this machine.
        Assert.Equal(3, usage.Snapshot().ProviderRequests);
    }

    /// <summary>
    /// Segmentation is off unless a deployment turns it on. Upgrading must not change what a
    /// customer hears.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SEG-DEFAULTOFF-09")]
    public async Task SegmentationIsOffByDefaultAndPlaybackStaysASingleFile()
    {
        SpeechSynthesisService service = CreateService(
            out _,
            configured: new TtsProviderOptions
            {
                ExecutionMode = "MOCK",
                Provider = TtsProviderOptions.FakeProvider,
            });

        RenderedSpeech call = await SynthesizeAsync(
            service,
            Summary([SpeechItem.Create("Sâm lát", 1, "hộp")], 560_000m));

        Assert.NotNull(call.Audio);
        Assert.False(call.Audio.IsPlaylist);
        Assert.Single(call.Audio.Segments);
    }

    /// <summary>
    /// The segment list and the text every gate inspects have to describe the same call. A
    /// caller that hands over a mismatched list is rejected, because playback follows the
    /// segments while the PII guard and the character budget follow the text.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SEG-REASSEMBLE-10")]
    public void SegmentsThatDoNotReassembleIntoTheScriptTextAreRejected()
    {
        ArgumentException failure = Assert.Throws<ArgumentException>(() => SpeechScript.Create(
            "SCRIPT-ORDER-CONFIRM",
            "v3-test-approved",
            "Xin chào Quý khách.",
            "content-hash",
            "summary-hash",
            [
                SpeechSegment.CreateFixed(1, "Xin chào "),
                SpeechSegment.CreateDynamic(2, "customer_display_name", "một người khác."),
            ]));

        Assert.Contains("reassemble", failure.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Out-of-order or gapped ordinals would play the sentences in an order nobody approved.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SEG-ORDER-11")]
    public void SegmentsMustBeContiguousAndInPlaybackOrder()
    {
        ArgumentException failure = Assert.Throws<ArgumentException>(() => SpeechScript.Create(
            "SCRIPT-ORDER-CONFIRM",
            "v3-test-approved",
            "Xin chào Quý khách.",
            "content-hash",
            "summary-hash",
            [
                SpeechSegment.CreateFixed(1, "Xin chào "),
                SpeechSegment.CreateFixed(3, "Quý khách."),
            ]));

        Assert.Contains("contiguous", failure.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The committed segment manifest and the runtime have to agree about what the script says.
    /// <para>
    /// The manifest tells a voice engineer which sentences to record and pins the identity each
    /// recording is filed under; the runtime looks recordings up by that identity. They are
    /// produced by different tools in different languages, so "they agree" is a claim, and this
    /// is where it gets checked. If they drift, the deployment either fails to find a recording
    /// or finds one made from wording nobody approved.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-SEG-MANIFEST-12")]
    public void TheCommittedSegmentManifestMatchesWhatTheRuntimeComputes()
    {
        string root = FindRepositoryRoot();
        using System.Text.Json.JsonDocument manifest = System.Text.Json.JsonDocument.Parse(
            File.ReadAllText(Path.Combine(root, "deploy", "lab", "speech-segments.json")));
        System.Text.Json.JsonElement rootElement = manifest.RootElement;

        Assert.Equal(
            TargetV1SpeechPolicy.MockTemplateId,
            rootElement.GetProperty("templateId").GetString());
        Assert.Equal(
            TargetV1SpeechPolicy.MockTemplateVersion,
            rootElement.GetProperty("templateVersion").GetString());

        ImmutableArray<SpeechSegmentTemplate> expected = TargetV1SpeechPolicy.SegmentTemplate(
            TargetV1SpeechPolicy.CanonicalVietnameseTemplate);
        System.Text.Json.JsonElement segments = rootElement.GetProperty("segments");
        Assert.Equal(expected.Length, segments.GetArrayLength());

        for (int index = 0; index < expected.Length; index++)
        {
            System.Text.Json.JsonElement actual = segments[index];
            SpeechSegmentTemplate piece = expected[index];
            Assert.Equal(piece.Ordinal, actual.GetProperty("ordinal").GetInt32());
            Assert.Equal(piece.Kind.ToString(), actual.GetProperty("kind").GetString());
            Assert.Equal(piece.PlaceholderName, actual.GetProperty("placeholder").GetString());
            if (piece.Kind != SpeechSegmentKind.Fixed)
            {
                continue;
            }

            Assert.Equal(piece.Text, actual.GetProperty("text").GetString());
            Assert.Equal(
                SpeechSegment.ComputeTextHash(piece.Text),
                actual.GetProperty("textSha256").GetString());
        }

        // The number the cost model rests on, asserted rather than quoted: most of the script is
        // prose that gets recorded once and never synthesized again.
        Assert.Equal(203, rootElement.GetProperty("fixedCharacters").GetInt32());
        Assert.Equal(4, rootElement.GetProperty("fixedSegmentCount").GetInt32());
        Assert.Equal(3, rootElement.GetProperty("dynamicSegmentCount").GetInt32());
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Ivr.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("The repository root was not found.");
    }

    private static TtsProviderOptions SegmentedOptions(FixedSegmentSource source) => new()
    {
        ExecutionMode = "MOCK",
        Provider = TtsProviderOptions.FakeProvider,
        Segmentation = new SpeechSegmentationOptions
        {
            Enabled = true,
            FixedSegments = source,
        },
    };

    private static FixedSegmentMediaEntry[] FullCatalog() =>
    [
        .. TargetV1SpeechPolicy
            .FixedSegmentHashes(TargetV1SpeechPolicy.CanonicalVietnameseTemplate)
            .Select((hash, index) => new FixedSegmentMediaEntry
            {
                TextHash = hash,
                MediaReference = $"sound:ivr-fixed-{index}",
                DurationMilliseconds = 2_000,
            }),
    ];

    private static PrivacySafeOrderSummary Summary(
        IEnumerable<SpeechItem> items,
        decimal amount) => PrivacySafeOrderSummary.Create(
        "Chị Mai",
        "DH-SEG-01",
        items,
        Money.Vnd(amount),
        ShortDeliveryArea.Create("Quận 7"),
        "24 trên 7",
        null,
        SpeechSummaryLimits.Create(20, 20));

    private static async Task<RenderedSpeech> SynthesizeAsync(
        SpeechSynthesisService service,
        PrivacySafeOrderSummary summary)
    {
        RenderedSpeech text = await new FakeSpeechRenderer().RenderAsync(
            summary,
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock,
            CancellationToken.None);
        return await service.SynthesizeAsync(
            text,
            summary,
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock,
            Now.AddMinutes(5),
            CancellationToken.None);
    }

    private static SpeechSynthesisService CreateService(
        out TtsUsageMeter usage,
        ITtsProvider? provider = null,
        TtsProviderOptions? configured = null)
    {
        var time = new FixedTimeProvider(Now);
        usage = new TtsUsageMeter();
        IOptions<TtsProviderOptions> options = Options.Create(
            configured ?? SegmentedOptions(FixedSegmentSource.Provider));
        return new SpeechSynthesisService(
            provider ?? new FakeDeterministicTtsProvider(options),
            new AudioCache(time),
            new TtsRequestBudget(time),
            usage,
            new RegionalVoiceMap(options),
            options,
            time);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class CountingTtsProvider : ITtsProvider
    {
        public int Calls { get; private set; }

        public Task<RenderedAudio> SynthesizeAsync(
            SpeechScript script,
            TtsOptions options,
            CancellationToken cancellationToken)
        {
            Calls++;

            // Distinct audio per distinct text, so a test comparing playlists is comparing
            // something the provider actually varied.
            return Task.FromResult(RenderedAudio.Create(
                "audio/L16",
                8_000,
                TimeSpan.FromSeconds(1),
                string.Concat("sound:ivr-dyn-", script.ContentHash)));
        }
    }
}
