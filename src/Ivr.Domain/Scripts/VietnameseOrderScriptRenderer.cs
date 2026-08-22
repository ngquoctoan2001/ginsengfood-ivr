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
    string ContentHash);

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
        string exactText = validTemplate
            .Replace("{{customer_display_name}}", summary.CustomerDisplayName, StringComparison.Ordinal)
            .Replace("{{order_code_short}}", summary.OrderCodeShort, StringComparison.Ordinal)
            .Replace("{{items_spoken}}", spokenItems, StringComparison.Ordinal)
            .Replace("{{total_amount_display}}", totalAmount, StringComparison.Ordinal)
            .Replace("{{delivery_area_short}}", summary.DeliveryArea.Value, StringComparison.Ordinal)
            .Replace("{{program_display_name}}", summary.ProgramDisplayName, StringComparison.Ordinal)
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
            contentHash);
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
    /// Whole quantities are spoken ("hai hộp", matching the approved W-0104 audio). A fractional
    /// quantity keeps the decimal form: Vietnamese reads "2,5" as "hai phẩy năm" reliably enough,
    /// and inventing a spoken form for fractions here would be guessing at wording nobody has
    /// approved. Concatenative playback cannot glue a decimal from recorded clips, so this is the
    /// one place that still needs a decision before W-0106 §4.8.6 ships.
    /// </summary>
    private static string FormatQuantity(decimal quantity, VietnameseNumberStyle numberStyle) =>
        quantity == decimal.Truncate(quantity)
            && quantity >= 0m
            && quantity <= VietnameseNumberSpeller.MaximumAmount
            ? VietnameseNumberSpeller.Spell(quantity, numberStyle)
            : quantity.ToString("0.##", VietnameseNumbers);
}
