using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Privacy;

namespace Ivr.Domain.Scripts;

public sealed record ScriptRenderOptions
{
    private ScriptRenderOptions(int maximumSpokenItems, int maximumCharacters, int wordsPerMinute)
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
    }

    public int MaximumSpokenItems { get; }

    public int MaximumCharacters { get; }

    public int WordsPerMinute { get; }

    public static ScriptRenderOptions Default { get; } = new(3, 1_200, 150);

    public static ScriptRenderOptions Create(
        int maximumSpokenItems,
        int maximumCharacters,
        int wordsPerMinute) =>
        new(maximumSpokenItems, maximumCharacters, wordsPerMinute);
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
    private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");
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

        string spokenItems = FormatItems(summary.Items, effectiveOptions.MaximumSpokenItems);
        string totalAmount = string.Concat(
            summary.Total.Amount.ToString("N0", VietnameseCulture),
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

    private static string FormatItems(IReadOnlyList<SpeechItem> items, int maximumSpokenItems)
    {
        string[] spoken = items
            .Take(maximumSpokenItems)
            .Select(item => string.Join(
                " ",
                new[]
                {
                    FormatQuantity(item.Quantity),
                    item.UnitLabel,
                    item.PublicName,
                }.Where(value => !string.IsNullOrWhiteSpace(value))))
            .ToArray();
        int remainder = items.Count - spoken.Length;
        if (remainder > 0)
        {
            return string.Concat(string.Join(", ", spoken), ", và ", remainder, " sản phẩm khác");
        }

        return spoken.Length switch
        {
            1 => spoken[0],
            2 => string.Join(" và ", spoken),
            _ => string.Concat(string.Join(", ", spoken[..^1]), " và ", spoken[^1]),
        };
    }

    private static string FormatQuantity(decimal quantity) =>
        quantity.ToString(quantity == decimal.Truncate(quantity) ? "0" : "0.##", VietnameseCulture);
}
