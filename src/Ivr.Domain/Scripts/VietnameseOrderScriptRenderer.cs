using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Privacy;
using Ivr.Domain.Speech;

namespace Ivr.Domain.Scripts;

public sealed record ScriptRenderOptions
{
    private ScriptRenderOptions(
        int maximumSpokenItems,
        int maximumCharacters,
        int wordsPerMinute,
        VietnamRegion fallbackRegion)
    {
        if (maximumSpokenItems is < 1 or > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumSpokenItems));
        }

        if (maximumCharacters is < 200 or > 4_000)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumCharacters));
        }

        if (wordsPerMinute is < 80 or > 250)
        {
            throw new ArgumentOutOfRangeException(nameof(wordsPerMinute));
        }

        MaximumSpokenItems = maximumSpokenItems;
        MaximumCharacters = maximumCharacters;
        WordsPerMinute = wordsPerMinute;
        FallbackRegion = fallbackRegion;
    }

    public int MaximumSpokenItems { get; }

    public int MaximumCharacters { get; }

    public int WordsPerMinute { get; }

    /// <summary>
    /// Region used for spoken-number wording when the delivery area names no province.
    /// <para>
    /// This is the same fallback the regional voice map uses, and it is passed in rather than
    /// defaulted independently on purpose: if the renderer fell back North while the voice map
    /// fell back South, a Southern voice would say "nghìn" and nothing would fail.
    /// </para>
    /// </summary>
    public VietnamRegion FallbackRegion { get; }

    public static ScriptRenderOptions Default { get; } = new(3, 1_200, 150, VietnamRegion.North);

    public static ScriptRenderOptions Create(
        int maximumSpokenItems,
        int maximumCharacters,
        int wordsPerMinute,
        VietnamRegion fallbackRegion = VietnamRegion.North) =>
        new(maximumSpokenItems, maximumCharacters, wordsPerMinute, fallbackRegion);
}

public sealed record ScriptInputItemSnapshot(string PublicName, decimal Quantity, string? UnitLabel);

public sealed record ScriptInputSnapshot(
    string CustomerDisplayName,
    string OrderCodeShort,
    IReadOnlyList<ScriptInputItemSnapshot> Items,
    decimal TotalAmount,
    string Currency,
    string DeliveryAreaShort,
    string ProgramDisplayName,
    string Locale,
    string InputHash);

public sealed record ScriptPreview(
    string ScriptReference,
    string ExactText,
    TimeSpan EstimatedDuration,
    ScriptInputSnapshot InputSnapshot,
    string TemplateHash,
    string ContentHash)
{
    /// <summary>
    /// Playback order split at the template's placeholder boundaries. Empty only for a preview
    /// built by an older renderer; consumers treat empty as "speak the whole text in one piece".
    /// </summary>
    public ImmutableArray<SpeechSegment> Segments { get; init; } = [];
}

public interface IScriptPreviewRenderer
{
    public ScriptPreview Render(
        ApprovedScript script,
        PrivacySafeOrderSummary summary,
        ScriptRenderOptions? options = null);
}

public sealed class VietnameseOrderScriptRenderer : IScriptPreviewRenderer
{
    /// <summary>
    /// Vietnamese number formatting, constructed here instead of looked up from ICU.
    /// <para>
    /// <c>CultureInfo.GetCultureInfo("vi-VN")</c> is what this used to be, and it made the SHIPPED
    /// WORKER IMAGE unable to speak: the chiseled runtime base runs in globalization-invariant
    /// mode, so the lookup threw <see cref="CultureNotFoundException"/> inside a static
    /// constructor. That surfaces as <c>TypeInitializationException</c> on the first render, which
    /// the dispatch gateway maps to a generic technical failure -- and after three of those DT-04
    /// auto-disabled the only SIM channel. Every test passed, because tests run on a host with ICU.
    /// </para>
    /// <para>
    /// Two separators is not enough locale data to be worth a runtime dependency, and building
    /// them here buys something ICU cannot: a customer hears the same number on every machine,
    /// rather than whatever the base image's ICU version happens to say this year.
    /// <c>UT-SCRIPT-VI-FORMAT-08</c> pins the values against real ICU where ICU exists.
    /// </para>
    /// </summary>
    private static readonly NumberFormatInfo VietnameseNumbers = CreateVietnameseNumbers();

    private static NumberFormatInfo CreateVietnameseNumbers()
    {
        var format = (NumberFormatInfo)NumberFormatInfo.InvariantInfo.Clone();
        format.NumberGroupSeparator = ".";
        format.NumberDecimalSeparator = ",";
        format.NumberGroupSizes = [3];
        format.NumberNegativePattern = 1;
        return format;
    }

    private static readonly Regex WhitespacePattern = new(
        "\\s+",
        RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100));

    public ScriptPreview Render(
        ApprovedScript script,
        PrivacySafeOrderSummary summary,
        ScriptRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(script);
        ArgumentNullException.ThrowIfNull(summary);
        ScriptRenderOptions effectiveOptions = options ?? ScriptRenderOptions.Default;
        string validTemplate = TargetV1SpeechPolicy.ValidateTemplate(script.Version.TemplateText);

        // The region is derived here rather than carried on the summary. PrivacySafeOrderSummary
        // has 95 dependent symbols across two execution flows, so adding a field to it would
        // turn a voice change into a contract change; ShortDeliveryArea already holds everything
        // the mapping needs.
        VietnamRegion region =
            DeliveryRegionResolver.TryResolve(summary.DeliveryArea.Value)
            ?? effectiveOptions.FallbackRegion;
        VietnameseNumberStyle numberStyle = VietnameseNumberStyle.ForRegion(region);

        string spokenItems = FormatItems(
            summary.Items,
            summary.PronunciationHints,
            effectiveOptions.MaximumSpokenItems,
            numberStyle);

        // Spoken words, not digits. The renderer used to emit "560.000 đồng" while the audio the
        // owner approved in W-0104 says "năm trăm sáu mươi nghìn đồng" — that sample had been
        // typed by hand, so nobody had ever heard the digits path. How an engine reads "560.000"
        // is engine-specific, and this is the number the customer is pressing a key to confirm.
        string totalAmount = string.Concat(
            VietnameseNumberSpeller.Spell(summary.Total.Amount, numberStyle),
            " đồng");

        // Substitution happens per segment rather than over the whole string, and the full text
        // is then assembled from those segments. It is the same output as the previous chain of
        // Replace calls, with one property added: the text the gates inspect and the pieces the
        // call plays are produced by the same statement, so they cannot describe different calls.
        ImmutableArray<SpeechSegment> segments = BuildSegments(
            validTemplate,
            summary,
            spokenItems,
            totalAmount);
        string exactText = SpeechSegment
            .Concatenate(segments)
            .Normalize(NormalizationForm.FormC);

        if (exactText.Length > effectiveOptions.MaximumCharacters)
        {
            throw new InvalidOperationException("Rendered script exceeds the configured speech limit.");
        }

        PiiGuard.EnsureSafeText(exactText);
        string inputHash = summary.ComputeHash();
        ScriptInputSnapshot inputSnapshot = new(
            summary.CustomerDisplayName,
            summary.OrderCodeShort,
            summary.Items.Select(item => new ScriptInputItemSnapshot(
                item.PublicName,
                item.Quantity,
                item.UnitLabel)).ToArray(),
            summary.Total.Amount,
            summary.Total.Currency,
            summary.DeliveryArea.Value,
            summary.ProgramDisplayName,
            summary.Locale,
            inputHash);
        int wordCount = WhitespacePattern.Split(exactText.Trim()).Length;
        int estimatedSeconds = Math.Max(
            1,
            (int)Math.Ceiling(wordCount * 60d / effectiveOptions.WordsPerMinute));
        string contentHash = DeterministicSnapshotHasher.Compute(
            script.Version.TemplateHash,
            inputHash,
            exactText);

        return new ScriptPreview(
            script.Version.Key.ToString(),
            exactText,
            TimeSpan.FromSeconds(estimatedSeconds),
            inputSnapshot,
            script.Version.TemplateHash,
            contentHash)
        {
            Segments = segments,
        };
    }

    /// <summary>
    /// Substitutes order values into the template's variable pieces and keeps the prose pieces
    /// verbatim, preserving playback order.
    /// </summary>
    private static ImmutableArray<SpeechSegment> BuildSegments(
        string validTemplate,
        PrivacySafeOrderSummary summary,
        string spokenItems,
        string totalAmount)
    {
        var segments = ImmutableArray.CreateBuilder<SpeechSegment>();
        int ordinal = 0;
        foreach (SpeechSegmentTemplate piece in TargetV1SpeechPolicy.SegmentTemplate(validTemplate))
        {
            if (piece.Kind == SpeechSegmentKind.Fixed)
            {
                segments.Add(SpeechSegment.CreateFixed(++ordinal, piece.Text));
                continue;
            }

            string value = piece.PlaceholderName switch
            {
                "customer_display_name" => summary.CustomerDisplayName,
                "order_code_short" => summary.OrderCodeShort,
                "items_spoken" => spokenItems,
                "total_amount_display" => totalAmount,
                "delivery_area_short" => summary.DeliveryArea.Value,
                "program_display_name" => summary.ProgramDisplayName,

                // Unreachable while the template validator and this switch agree on the
                // whitelist. Throwing rather than substituting an empty string means a widened
                // whitelist fails loudly here instead of silently dropping a sentence from a
                // customer's call.
                _ => throw new InvalidOperationException(
                    "The script template uses a variable the renderer cannot substitute."),
            };
            segments.Add(SpeechSegment.CreateDynamic(++ordinal, piece.PlaceholderName, value));
        }

        return segments.ToImmutable();
    }

    private static string FormatItems(
        IReadOnlyList<SpeechItem> items,
        IReadOnlyDictionary<string, string> pronunciationHints,
        int maximumSpokenItems,
        VietnameseNumberStyle numberStyle)
    {
        string[] spoken = items
            .Take(maximumSpokenItems)
            .Select(item => string.Join(
                " ",
                new[]
                {
                    FormatQuantity(item.Quantity, numberStyle),
                    item.UnitLabel,
                    pronunciationHints.GetValueOrDefault(item.PublicName, item.PublicName),
                }.Where(value => !string.IsNullOrWhiteSpace(value))))
            .ToArray();
        int remainder = items.Count - spoken.Length;
        if (remainder > 0)
        {
            // Spoken too: "và một sản phẩm khác", not "và 1 sản phẩm khác".
            return string.Concat(
                string.Join(", ", spoken),
                ", và ",
                VietnameseNumberSpeller.Spell(remainder, numberStyle),
                " sản phẩm khác");
        }

        return spoken.Length switch
        {
            1 => spoken[0],
            2 => string.Join(" và ", spoken),
            _ => string.Concat(string.Join(", ", spoken[..^1]), " và ", spoken[^1]),
        };
    }

    /// <summary>
    /// Quantities are spoken, fractional ones included: "hai hộp", "hai phẩy năm ký".
    /// <para>
    /// Fractions used to keep the digit form <c>"2,5"</c> on the reasoning that engines read it
    /// acceptably. Two things retired that. Segmented playback assembles a call from recorded
    /// and cached pieces, and there is no clip for "2,5" — the digit form was the one input the
    /// pipeline could not voice at all. And "acceptably" was never verified by listening; it was
    /// the same assumption that produced <c>"560.000 đồng"</c> against approved audio saying
    /// "năm trăm sáu mươi nghìn đồng".
    /// </para>
    /// <para>
    /// The digit fallback survives only for quantities outside the speller's range, where a
    /// wrong reading is better than a failed call — and a quantity that large is a data problem
    /// the call itself cannot fix.
    /// </para>
    /// </summary>
    private static string FormatQuantity(decimal quantity, VietnameseNumberStyle numberStyle)
    {
        if (quantity < 0m || quantity > VietnameseNumberSpeller.MaximumAmount)
        {
            return quantity.ToString("0.##", VietnameseNumbers);
        }

        try
        {
            return VietnameseNumberSpeller.SpellQuantity(quantity, numberStyle);
        }
        catch (ArgumentOutOfRangeException)
        {
            // More decimal places than a spoken quantity carries. Reading it digit by digit
            // would be a sentence long; the digit form at least stays short and wrong in a way
            // the operator can see in the call log.
            return quantity.ToString("0.##", VietnameseNumbers);
        }
    }
}
