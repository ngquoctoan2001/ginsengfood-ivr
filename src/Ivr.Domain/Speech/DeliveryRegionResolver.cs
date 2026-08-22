using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Ivr.Domain.Speech;

/// <summary>
/// Maps a privacy-safe delivery area string onto one of the three IVR voice regions.
/// <para>
/// This is a pure function of data the IVR already holds. It deliberately does <b>not</b> add a
/// field to <c>PrivacySafeOrderSummary</c>: that record has 95 dependent symbols across two
/// execution flows, so widening it to carry a region would turn a voice change into a contract
/// change. The region is derived at the speech layer instead, and nothing upstream moves.
/// </para>
/// <para>
/// The table is the 34 provincial units created by Nghị quyết 202/2025/QH15 (effective
/// 2025-07-01), plus the 29 pre-merger province names they absorbed. The aliases are not
/// cosmetic: Sales master data and in-flight orders can still carry the old names, and without
/// them every such order would silently fall back to the default voice.
/// </para>
/// </summary>
public static class DeliveryRegionResolver
{
    /// <summary>
    /// Prefixes stripped from a candidate token before lookup. The 2025 reform removed the
    /// district tier, so a delivery area is normally just ward plus province.
    /// </summary>
    private static readonly ImmutableArray<string> UnitPrefixes =
    [
        "thanh pho ",
        "tinh ",
        "tp ",
        "t p ",
    ];

    private static readonly FrozenDictionary<string, VietnamRegion> ProvinceRegions =
        BuildProvinceRegions();

    /// <summary>
    /// Province lookup keys ordered longest-first, so "ba ria vung tau" is considered before a
    /// shorter key that happens to share a prefix.
    /// </summary>
    private static readonly ImmutableArray<string[]> ProvinceKeyWords = ProvinceRegions
        .Keys
        .Select(key => key.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        .OrderByDescending(words => words.Length)
        .ThenBy(words => string.Join(' ', words), StringComparer.Ordinal)
        .ToImmutableArray();

    /// <summary>
    /// The 34 current provincial units keyed by match form. Exposed for tests and for the
    /// configuration validator; callers must not mutate it.
    /// </summary>
    public static FrozenDictionary<string, VietnamRegion> ProvinceRegionTable => ProvinceRegions;

    /// <summary>
    /// Resolves the region for a delivery area, or <see langword="null"/> when no provincial
    /// unit can be identified.
    /// <para>
    /// Returning null rather than guessing is deliberate. A wrong guess plays the wrong regional
    /// voice to a real customer and nobody finds out; a null is counted by
    /// <c>ivr_tts_region_unresolved_total</c> and shows up as a Sales data-quality signal.
    /// </para>
    /// </summary>
    public static VietnamRegion? TryResolve(string? deliveryAreaShort)
    {
        if (string.IsNullOrWhiteSpace(deliveryAreaShort))
        {
            return null;
        }

        string normalized = VietnameseTextNormalizer.ToMatchKey(deliveryAreaShort);
        if (normalized.Length == 0)
        {
            return null;
        }

        // Exact token match first: "phường Phú Khương, tỉnh Vĩnh Long" splits on the comma and
        // the province is the trailing token. Scanning right-to-left means a ward that happens
        // to share a province name loses to the real province at the end of the string.
        string[] tokens = deliveryAreaShort.Split(',', StringSplitOptions.RemoveEmptyEntries);
        for (int index = tokens.Length - 1; index >= 0; index--)
        {
            string candidate = StripUnitPrefix(VietnameseTextNormalizer.ToMatchKey(tokens[index]));
            if (candidate.Length > 0 && ProvinceRegions.TryGetValue(candidate, out VietnamRegion exact))
            {
                return exact;
            }
        }

        return ScanForLatestProvince(normalized);
    }

    /// <summary>
    /// Fallback for strings the comma split cannot handle — a missing separator, a trailing
    /// "Việt Nam", or an extra word glued onto the province token. Matches on whole-word
    /// sequences, never raw substrings, so "hue" cannot be found inside "thue".
    /// </summary>
    private static VietnamRegion? ScanForLatestProvince(string normalized)
    {
        string[] words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int bestPosition = -1;
        int bestLength = 0;
        VietnamRegion? best = null;

        foreach (string[] provinceWords in ProvinceKeyWords)
        {
            int position = LastIndexOfSequence(words, provinceWords);

            // Latest occurrence wins because the province sits at the end of an address; a
            // longer name breaks a tie so "an giang" cannot beat a longer overlapping key.
            if (position < 0
                || position < bestPosition
                || (position == bestPosition && provinceWords.Length <= bestLength))
            {
                continue;
            }

            bestPosition = position;
            bestLength = provinceWords.Length;
            best = ProvinceRegions[string.Join(' ', provinceWords)];
        }

        return best;
    }

    private static int LastIndexOfSequence(string[] words, string[] sequence)
    {
        for (int start = words.Length - sequence.Length; start >= 0; start--)
        {
            bool matched = true;
            for (int offset = 0; offset < sequence.Length; offset++)
            {
                if (!string.Equals(words[start + offset], sequence[offset], StringComparison.Ordinal))
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return start;
            }
        }

        return -1;
    }

    private static string StripUnitPrefix(string candidate)
    {
        foreach (string prefix in UnitPrefixes)
        {
            if (candidate.StartsWith(prefix, StringComparison.Ordinal))
            {
                return candidate[prefix.Length..];
            }
        }

        return candidate;
    }

    private static FrozenDictionary<string, VietnamRegion> BuildProvinceRegions()
    {
        var table = new Dictionary<string, VietnamRegion>(StringComparer.Ordinal);

        // ---- MIỀN BẮC — 15 current units -------------------------------------------------
        Add(table, VietnamRegion.North, "Hà Nội");
        Add(table, VietnamRegion.North, "Hải Phòng", "Hải Dương");
        Add(table, VietnamRegion.North, "Quảng Ninh");
        Add(table, VietnamRegion.North, "Cao Bằng");
        Add(table, VietnamRegion.North, "Lạng Sơn");
        Add(table, VietnamRegion.North, "Lai Châu");
        Add(table, VietnamRegion.North, "Điện Biên");
        Add(table, VietnamRegion.North, "Sơn La");
        Add(table, VietnamRegion.North, "Lào Cai", "Yên Bái");
        Add(table, VietnamRegion.North, "Tuyên Quang", "Hà Giang");
        Add(table, VietnamRegion.North, "Thái Nguyên", "Bắc Kạn", "Bắc Cạn");
        Add(table, VietnamRegion.North, "Phú Thọ", "Vĩnh Phúc", "Hòa Bình");
        Add(table, VietnamRegion.North, "Bắc Ninh", "Bắc Giang");
        Add(table, VietnamRegion.North, "Hưng Yên", "Thái Bình");
        Add(table, VietnamRegion.North, "Ninh Bình", "Hà Nam", "Nam Định");

        // ---- MIỀN TRUNG — 11 current units -----------------------------------------------
        Add(table, VietnamRegion.Central, "Thanh Hóa");
        Add(table, VietnamRegion.Central, "Nghệ An");
        Add(table, VietnamRegion.Central, "Hà Tĩnh");
        Add(table, VietnamRegion.Central, "Quảng Trị", "Quảng Bình");
        Add(table, VietnamRegion.Central, "Huế", "Thừa Thiên Huế");
        Add(table, VietnamRegion.Central, "Đà Nẵng", "Quảng Nam");
        Add(table, VietnamRegion.Central, "Quảng Ngãi", "Kon Tum");
        Add(table, VietnamRegion.Central, "Gia Lai", "Bình Định");
        Add(table, VietnamRegion.Central, "Đắk Lắk", "Đắc Lắc", "Phú Yên");
        Add(table, VietnamRegion.Central, "Khánh Hòa", "Ninh Thuận");
        Add(table, VietnamRegion.Central, "Lâm Đồng", "Đắk Nông", "Đắc Nông", "Bình Thuận");

        // ---- MIỀN NAM — 8 current units ---------------------------------------------------
        Add(
            table,
            VietnamRegion.South,
            "Hồ Chí Minh",
            "TPHCM",
            "HCM",
            "Sài Gòn",
            "Bình Dương",
            "Bà Rịa Vũng Tàu",
            "Vũng Tàu");
        Add(table, VietnamRegion.South, "Đồng Nai", "Bình Phước");
        Add(table, VietnamRegion.South, "Tây Ninh", "Long An");
        Add(table, VietnamRegion.South, "Cần Thơ", "Sóc Trăng", "Hậu Giang");
        Add(table, VietnamRegion.South, "Vĩnh Long", "Bến Tre", "Trà Vinh");
        Add(table, VietnamRegion.South, "Đồng Tháp", "Tiền Giang");
        Add(table, VietnamRegion.South, "An Giang", "Kiên Giang");
        Add(table, VietnamRegion.South, "Cà Mau", "Bạc Liêu");

        return table.ToFrozenDictionary(StringComparer.Ordinal);
    }

    private static void Add(
        Dictionary<string, VietnamRegion> table,
        VietnamRegion region,
        params string[] names)
    {
        foreach (string name in names)
        {
            table.Add(VietnameseTextNormalizer.ToMatchKey(name), region);
        }
    }
}
