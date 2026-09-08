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
}
