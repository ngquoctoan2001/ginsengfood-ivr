using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Ivr.Domain.Privacy;

namespace Ivr.Domain.Confirmation;

public sealed record SpeechSummaryLimits
{
    private SpeechSummaryLimits(int maximumItems, int maximumPronunciationHints)
    {
        if (maximumItems is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumItems));
        }

        if (maximumPronunciationHints is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumPronunciationHints));
        }

        MaximumItems = maximumItems;
        MaximumPronunciationHints = maximumPronunciationHints;
    }

    public int MaximumItems { get; }

    public int MaximumPronunciationHints { get; }

    public static SpeechSummaryLimits Create(int maximumItems, int maximumPronunciationHints) =>
        new(maximumItems, maximumPronunciationHints);
}

public sealed record SpeechItem
{
    private SpeechItem(string publicName, decimal quantity, string? unitLabel)
    {
        PublicName = publicName;
        Quantity = quantity;
        UnitLabel = unitLabel;
    }

    public string PublicName { get; }

    public decimal Quantity { get; }

    public string? UnitLabel { get; }

    public static SpeechItem Create(string publicName, decimal quantity, string? unitLabel)
    {
        string safeName = PrivacySafeOrderSummary.EnsureSafeBounded(publicName, 160, nameof(publicName));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        string? safeUnit = string.IsNullOrWhiteSpace(unitLabel)
            ? null
            : PrivacySafeOrderSummary.EnsureSafeBounded(unitLabel, 40, nameof(unitLabel));
        return new SpeechItem(safeName, quantity, safeUnit);
    }
}

public sealed record Money
{
    private Money(decimal amount)
    {
        Amount = amount;
    }

    public decimal Amount { get; }

    public string Currency { get; } = "VND";

    public static Money Vnd(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        return new Money(amount);
    }
}

public sealed record ShortDeliveryArea
{
    private static readonly string[] RestrictedAddressMarkers =
    [
        "duong ",
        "đường ",
        "pho ",
        "phố ",
        "so nha ",
        "số nhà ",
        "ngo ",
        "ngõ ",
        "hem ",
        "hẻm ",
        "ngach ",
        "ngách ",
    ];

    private ShortDeliveryArea(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ShortDeliveryArea Create(string value)
    {
        string safeValue = PrivacySafeOrderSummary.EnsureSafeBounded(value, 160, nameof(value));
        string normalized = RemoveDiacritics(safeValue).ToLowerInvariant();
        string markerScanText = normalized.Replace(
            "thanh pho ",
            " ",
            StringComparison.Ordinal);
        if (char.IsDigit(safeValue.TrimStart()[0])
            || HasSlashHouseNumber(safeValue)
            || RestrictedAddressMarkers.Any(marker =>
                markerScanText.Contains(RemoveDiacritics(marker), StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("Delivery area contains full-address detail.");
        }

        return new ShortDeliveryArea(safeValue);
    }

    private static bool HasSlashHouseNumber(string value)
    {
        for (int index = 1; index < value.Length - 1; index++)
        {
            if (value[index] == '/'
                && char.IsDigit(value[index - 1])
                && char.IsDigit(value[index + 1]))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Folds Vietnamese diacritics for the address-marker scan.
    /// <para>
    /// This deliberately stays a separate copy from
    /// <see cref="Ivr.Domain.Speech.VietnameseTextNormalizer"/>: that one is a lookup helper and
    /// this one is a privacy guard, so widening place-name matching must not be able to loosen
    /// the address check as a side effect. <c>UT-VOICE-REGION-09</c> pins the two against a
    /// shared corpus.
    /// </para>
    /// <para>
    /// It used to call <c>Normalize(NormalizationForm.FormD)</c>, which is a silent no-op in the
    /// deployed chiseled runtime (no ICU, invariant globalization). The fold then returned its
    /// input untouched, so the <c>"thanh pho "</c> exemption below never matched real Vietnamese
    /// input and this guard rejected every "thành phố …" delivery area — the six centrally
    /// governed cities — while passing on any developer machine. Restoring the intended
    /// behaviour is a bug fix, not a policy change: the accepted set is exactly what the
    /// with-ICU behaviour and the existing tests already describe.
    /// </para>
    /// <para>
    /// The table below is a deliberate duplicate of the one in the normalizer. Sharing it would
    /// re-merge the two folds that the class note above keeps apart; duplicating a fixed Unicode
    /// table is the cheaper risk, and <c>UT-VOICE-REGION-09</c> fails if the two ever disagree.
    /// </para>
    /// </summary>
    private const string AccentedLetters =
        "àáảãạăằắẳẵặâầấẩẫậ" + "èéẻẽẹêềếểễệ" + "ìíỉĩị"
        + "òóỏõọôồốổỗộơờớởỡợ" + "ùúủũụưừứửữự" + "ỳýỷỹỵ" + "đ"
        + "ÀÁẢÃẠĂẰẮẲẴẶÂẦẤẨẪẬ" + "ÈÉẺẼẸÊỀẾỂỄỆ" + "ÌÍỈĨỊ"
        + "ÒÓỎÕỌÔỒỐỔỖỘƠỜỚỞỠỢ" + "ÙÚỦŨỤƯỪỨỬỮỰ" + "ỲÝỶỸỴ" + "Đ";

    private const string FoldedLetters =
        "aaaaaaaaaaaaaaaaa" + "eeeeeeeeeee" + "iiiii"
        + "ooooooooooooooooo" + "uuuuuuuuuuu" + "yyyyy" + "d"
        + "AAAAAAAAAAAAAAAAA" + "EEEEEEEEEEE" + "IIIII"
        + "OOOOOOOOOOOOOOOOO" + "UUUUUUUUUUU" + "YYYYY" + "D";

    private static readonly FrozenDictionary<char, char> DiacriticFolds = BuildFolds();

    private static FrozenDictionary<char, char> BuildFolds()
    {
        if (AccentedLetters.Length != FoldedLetters.Length)
        {
            throw new InvalidOperationException(
                "The delivery-area fold table is misaligned: the accented and folded strings differ in length.");
        }

        Dictionary<char, char> folds = new(AccentedLetters.Length);
        for (int index = 0; index < AccentedLetters.Length; index++)
        {
            folds[AccentedLetters[index]] = FoldedLetters[index];
        }

        return folds.ToFrozenDictionary();
    }

    private static string RemoveDiacritics(string value)
    {
        StringBuilder result = new(value.Length);
        foreach (char character in value)
        {
            result.Append(DiacriticFolds.TryGetValue(character, out char folded)
                ? folded
                : character);
        }

        return result.ToString();
    }
}

public sealed class PrivacySafeOrderSummary
{
    private PrivacySafeOrderSummary(
        string customerDisplayName,
        string orderCodeShort,
        ImmutableArray<SpeechItem> items,
        Money total,
        ShortDeliveryArea deliveryArea,
        string programDisplayName,
        ImmutableSortedDictionary<string, string> pronunciationHints)
    {
        CustomerDisplayName = customerDisplayName;
        OrderCodeShort = orderCodeShort;
        Items = items;
        Total = total;
        DeliveryArea = deliveryArea;
        ProgramDisplayName = programDisplayName;
        PronunciationHints = pronunciationHints;
    }

    public string CustomerDisplayName { get; }

    public string OrderCodeShort { get; }

    public ImmutableArray<SpeechItem> Items { get; }

    public Money Total { get; }

    public ShortDeliveryArea DeliveryArea { get; }

    public string ProgramDisplayName { get; }

    public string Locale { get; } = "vi-VN";

    public ImmutableSortedDictionary<string, string> PronunciationHints { get; }

    public static PrivacySafeOrderSummary Create(
        string customerDisplayName,
        string orderCodeShort,
        IEnumerable<SpeechItem> items,
        Money total,
        ShortDeliveryArea deliveryArea,
        string programDisplayName,
        IReadOnlyDictionary<string, string>? pronunciationHints,
        SpeechSummaryLimits limits)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(total);
        ArgumentNullException.ThrowIfNull(deliveryArea);
        ArgumentNullException.ThrowIfNull(limits);
        ImmutableArray<SpeechItem> safeItems = [.. items];
        if (safeItems.Length is 0 || safeItems.Length > limits.MaximumItems)
        {
            throw new InvalidOperationException("Speech item count is outside configured limits.");
        }

        ImmutableSortedDictionary<string, string>.Builder hints =
            ImmutableSortedDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        if (pronunciationHints is not null)
        {
            if (pronunciationHints.Count > limits.MaximumPronunciationHints)
            {
                throw new InvalidOperationException("Pronunciation hint count exceeds configured limits.");
            }

            foreach ((string key, string value) in pronunciationHints)
            {
                hints.Add(EnsureSafeBounded(key, 80, nameof(pronunciationHints)),
                    EnsureSafeBounded(value, 160, nameof(pronunciationHints)));
            }
        }

        return new PrivacySafeOrderSummary(
            EnsureSafeBounded(customerDisplayName, 80, nameof(customerDisplayName)),
            EnsureSafeBounded(orderCodeShort, 40, nameof(orderCodeShort)),
            safeItems,
            total,
            deliveryArea,
            EnsureSafeBounded(programDisplayName, 80, nameof(programDisplayName)),
            hints.ToImmutable());
    }

    public string ComputeHash()
    {
        IEnumerable<string> itemFields = Items.SelectMany(item => new[]
        {
            item.PublicName,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.UnitLabel ?? string.Empty,
        });
        IEnumerable<string> hintFields = PronunciationHints.SelectMany(pair => new[] { pair.Key, pair.Value });
        return DeterministicSnapshotHasher.Compute(
            [
                CustomerDisplayName,
                OrderCodeShort,
                Total.Amount.ToString(CultureInfo.InvariantCulture),
                Total.Currency,
                DeliveryArea.Value,
                ProgramDisplayName,
                Locale,
                .. itemFields,
                .. hintFields,
            ]);
    }

    internal static string EnsureSafeBounded(string value, int maximumLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A required privacy-safe field was empty.", parameterName);
        }

        string normalized = value.Trim().Normalize(NormalizationForm.FormC);
        if (normalized.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        PiiGuard.EnsureSafeText(normalized);
        return normalized;
    }
}
