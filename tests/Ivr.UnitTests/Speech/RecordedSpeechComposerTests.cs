using System.Collections.Immutable;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Speech;

namespace Ivr.UnitTests.Speech;

public sealed class RecordedSpeechComposerTests
{
    private static readonly RecordedSpeechCatalog Catalog = RecordedSpeechCatalog.Create(
        new Dictionary<string, string>
        {
            ["Sâm Ngọc Linh"] = "item-sam-ngoc-linh",
            ["Mật ong rừng"] = "item-mat-ong-rung",
            ["Trà gừng"] = "item-tra-gung",
        },
        new Dictionary<string, string>
        {
            ["hộp"] = "unit-hop",
            ["chai"] = "unit-chai",
        });

    private static SpeechItem Item(string name, decimal quantity, string? unit) =>
        SpeechItem.Create(name, quantity, unit);

    private static string Speak(ImmutableArray<SpeechNumberClip> clips) =>
        SpeechNumberClip.Join(clips);

    [Fact]
    [Trait("TestId", "UT-VOICE-3B-01")]
    public void RecordedProductsAreNamedAndJoinedWithAnd()
    {
        Assert.True(RecordedSpeechComposer.TryCompose(
            [Item("Sâm Ngọc Linh", 2m, "hộp"), Item("Mật ong rừng", 1m, "chai")],
            20,
            VietnameseNumberStyle.Northern,
            Catalog,
            out ImmutableArray<SpeechNumberClip> clips));

        Assert.Equal("hai hộp Sâm Ngọc Linh và một chai Mật ong rừng", Speak(clips));

        // Quantity comes from bank A, so a fractional one still works without a clip of its own.
        Assert.True(RecordedSpeechComposer.TryCompose(
            [Item("Trà gừng", 2.5m, null)],
            20,
            VietnameseNumberStyle.Northern,
            Catalog,
            out ImmutableArray<SpeechNumberClip> fractional));
        Assert.Equal("hai phẩy năm Trà gừng", Speak(fractional));
    }

    [Fact]
    [Trait("TestId", "UT-VOICE-3B-02")]
    public void AProductWithNoClipFoldsIntoThePhraseOverflowAlreadyUses()
    {
        // 3b, the owner's rule of 2026-09-08: name what is recorded, fold the rest. The phrase is
        // the one the renderer has always used past MaximumSpokenItems, not a new invention.
        Assert.True(RecordedSpeechComposer.TryCompose(
            [Item("Sâm Ngọc Linh", 1m, "hộp"), Item("Cao hồng sâm", 3m, "hộp")],
            20,
            VietnameseNumberStyle.Northern,
            Catalog,
            out ImmutableArray<SpeechNumberClip> clips));

        Assert.Equal("một hộp Sâm Ngọc Linh và một sản phẩm khác", Speak(clips));
    }

    [Fact]
    [Trait("TestId", "UT-VOICE-3B-03")]
    public void ARecordedProductWithAnUnrecordedUnitIsNotSpeakableEither()
    {
        // Dropping the unit would read "hai Sâm Ngọc Linh" for two boxes. The customer is being
        // asked to confirm a quantity, so losing the unit loses the thing being confirmed.
        Assert.True(RecordedSpeechComposer.TryCompose(
            [Item("Sâm Ngọc Linh", 2m, "thùng"), Item("Trà gừng", 1m, "hộp")],
            20,
            VietnameseNumberStyle.Northern,
            Catalog,
            out ImmutableArray<SpeechNumberClip> clips));

        Assert.Equal("một hộp Trà gừng và một sản phẩm khác", Speak(clips));
    }

    [Fact]
    [Trait("TestId", "UT-VOICE-3B-04")]
    public void NothingRecordedMeansDoNotCall()
    {
        // The floor. "Quý khách có đơn hàng gồm một sản phẩm" confirms nothing, and phoning a
        // customer to read it is worse than not phoning; the caller routes this to admin review.
        Assert.False(RecordedSpeechComposer.TryCompose(
            [Item("Cao hồng sâm", 1m, "hộp"), Item("Nấm linh chi", 2m, "gói")],
            20,
            VietnameseNumberStyle.Northern,
            Catalog,
            out ImmutableArray<SpeechNumberClip> clips));
        Assert.Empty(clips);

        // A deployment that forgot to load the bank lands here rather than in silence.
        Assert.False(RecordedSpeechComposer.TryCompose(
            [Item("Sâm Ngọc Linh", 1m, "hộp")],
            20,
            VietnameseNumberStyle.Northern,
            RecordedSpeechCatalog.Empty,
            out _));
    }

    [Fact]
    [Trait("TestId", "UT-VOICE-3B-05")]
    public void OverflowAndUnrecordedItemsAreCountedTogether()
    {
        // "N sản phẩm khác" means "N products I did not name", whichever reason they went unnamed.
        Assert.True(RecordedSpeechComposer.TryCompose(
            [
                Item("Sâm Ngọc Linh", 1m, "hộp"),
                Item("Mật ong rừng", 1m, "chai"),
                Item("Trà gừng", 1m, "hộp"),
                Item("Cao hồng sâm", 1m, "hộp"),
            ],
            2,
            VietnameseNumberStyle.Northern,
            Catalog,
            out ImmutableArray<SpeechNumberClip> clips));

        Assert.Equal(
            "một hộp Sâm Ngọc Linh và một chai Mật ong rừng và hai sản phẩm khác",
            Speak(clips));
    }

    [Fact]
    [Trait("TestId", "UT-VOICE-4B-07")]
    public void BankCIsOneUnitAndItsCasingDoesNotDecideWhetherAnOrderIsSpoken()
    {
        // Owner settled 4b on 2026-09-09: one unit, "hộp". Asserted here so the decision is
        // executable rather than only written down.
        Assert.Equal(["hộp"], RecordedSpeechCatalog.SettledUnitClipIds.Keys);

        RecordedSpeechCatalog catalog = RecordedSpeechCatalog.Create(
            new Dictionary<string, string> { ["Sâm Ngọc Linh"] = "item-sam-ngoc-linh" },
            RecordedSpeechCatalog.SettledUnitClipIds);

        // "Hộp" with a capital H is already in this repository, in two Confirmation test files.
        // Under an exact-match lookup it would miss, and 3b would quietly fold a perfectly
        // ordinary order into "một sản phẩm khác". Casing on a common noun is field formatting,
        // not meaning.
        foreach (string unit in new[] { "hộp", "Hộp", "HỘP", "  hộp  " })
        {
            Assert.True(RecordedSpeechComposer.TryCompose(
                [Item("Sâm Ngọc Linh", 2m, unit)],
                20,
                VietnameseNumberStyle.Northern,
                catalog,
                out ImmutableArray<SpeechNumberClip> clips));
            Assert.Equal("hai hộp Sâm Ngọc Linh", Speak(clips));
        }

        // Accents are not folded in either direction: hộp, hợp and họp are three different words,
        // and an unaccented "hop" is not one of them. A second, speakable line is needed to see
        // the fold at all -- on its own an unspeakable item trips the floor instead, which is
        // the floor doing its job.
        Assert.True(RecordedSpeechComposer.TryCompose(
            [Item("Sâm Ngọc Linh", 1m, "hộp"), Item("Sâm Ngọc Linh", 2m, "hop")],
            20,
            VietnameseNumberStyle.Northern,
            catalog,
            out ImmutableArray<SpeechNumberClip> folded));
        Assert.Equal("một hộp Sâm Ngọc Linh và một sản phẩm khác", Speak(folded));

        Assert.False(RecordedSpeechComposer.TryCompose(
            [Item("Sâm Ngọc Linh", 2m, "hop")],
            20,
            VietnameseNumberStyle.Northern,
            catalog,
            out _));
    }

    [Fact]
    [Trait("TestId", "UT-VOICE-3B-06")]
    public void ClipIdsAreStableAcrossRegionsAndOnlyTheLexiconMoves()
    {
        static ImmutableArray<SpeechNumberClip> Compose(VietnameseNumberStyle style)
        {
            Assert.True(RecordedSpeechComposer.TryCompose(
                [Item("Sâm Ngọc Linh", 1_000m, "hộp"), Item("Cao hồng sâm", 1m, "hộp")],
                20,
                style,
                Catalog,
                out ImmutableArray<SpeechNumberClip> clips));
            return clips;
        }

        // The thousand goes on a quantity, not on the fold count: the fold counts *items*, and
        // items[] is capped at 100 by the contract, so no realistic order ever folds a thousand of
        // them. A quantity reaches the scale words, and num-thousand is the clip whose text moves.
        Assert.Equal(
            Compose(VietnameseNumberStyle.Northern).Select(clip => clip.Id),
            Compose(VietnameseNumberStyle.Southern).Select(clip => clip.Id));
        Assert.Contains("nghìn", Speak(Compose(VietnameseNumberStyle.Northern)));
        Assert.Contains("ngàn", Speak(Compose(VietnameseNumberStyle.Southern)));
    }

    [Theory]
    [Trait("TestId", "UT-VOICE-3B-07")]
    [InlineData("Phường Bến Nghé, TPHCM", "area-ho-chi-minh", "Hồ Chí Minh")]
    [InlineData("Phường Cửa Nam, thành phố Hà Nội", "area-ha-noi", "Hà Nội")]
    [InlineData("Phường Phú Khương, tỉnh Vĩnh Long", "area-vinh-long", "Vĩnh Long")]
    [InlineData("Phường X, tỉnh Hải Dương", "area-hai-phong", "Hải Phòng")]
    public void AnAreaBecomesOneProvinceClip(string area, string expectedId, string expectedText)
    {
        // The last row is the merger showing through at clip level: a pre-merger name plays the
        // clip of the unit that absorbed it, so a customer in Hải Dương hears "Hải Phòng". Pinned
        // rather than left implicit, because it is the cost W-0241 accepted and someone will
        // eventually read it as a bug.
        Assert.True(RecordedSpeechComposer.TryComposeArea(area, out SpeechNumberClip clip));
        Assert.Equal(expectedId, clip.Id);
        Assert.Equal(expectedText, clip.Text);
    }

    [Theory]
    [Trait("TestId", "UT-VOICE-3B-08")]
    [InlineData("phường 12, quận Bình Thạnh")]
    [InlineData("Phường Bến Nghé, Quận 1")]
    [InlineData("")]
    [InlineData(null)]
    public void AnAreaWithNoProvinceRefusesInsteadOfPlayingNothing(string? area)
    {
        // "giao đến " and ". Bấm phím một…" are recorded fixed segments on either side of this
        // value, so an empty run does not shorten the sentence -- it asks the customer to confirm
        // a delivery to nowhere. False forces the caller to decide; what it should decide is open
        // and coupled to 4c.
        Assert.False(RecordedSpeechComposer.TryComposeArea(area, out SpeechNumberClip clip));
        Assert.Equal(default, clip);
    }

    [Fact]
    [Trait("TestId", "UT-VOICE-3B-09")]
    public void BankEIsWhollyDerivableFromCodeUnlikeTheProductBank()
    {
        // The difference worth stating: bank D waits on twenty names nobody has written down yet,
        // while bank E is complete today because the area is spoken as a province and the province
        // table is compiled in. Nothing has to arrive for this recording script to be produced.
        ImmutableArray<SpeechNumberClip> bank = RecordedSpeechComposer.AreaClipBank();
        Assert.Equal(34, bank.Length);
        Assert.Equal(bank.Length, bank.Select(clip => clip.Id).Distinct(StringComparer.Ordinal).Count());

        // Every clip in the bank is reachable from its own spoken text, so the recording script and
        // the runtime lookup cannot describe different sets.
        foreach (SpeechNumberClip clip in bank)
        {
            Assert.True(RecordedSpeechComposer.TryComposeArea(clip.Text, out SpeechNumberClip round));
            Assert.Equal(clip, round);
        }
    }
}