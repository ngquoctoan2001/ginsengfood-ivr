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

    private static readonly (
        FrozenDictionary<string, VietnamRegion> Regions,
        FrozenDictionary<string, string> DisplayNames) Tables = BuildTables();

    private static FrozenDictionary<string, VietnamRegion> ProvinceRegions => Tables.Regions;

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
    /// Match form to the <b>current</b> provincial name, spoken. Every alias in a group points at
    /// the group's first name, so "Hải Dương" resolves to "Hải Phòng": an in-flight order carrying
    /// a pre-merger name is still read with the name that exists today, and the recording bank
    /// needs only the 34 current units rather than those plus the 29 they absorbed.
    /// <para>
    /// One clip per province, not one per province per voice. The province selects the region and
    /// the region selects the voice, so a province is only ever spoken by its own regional voice —
    /// there is no call in which the Northern voice says "Vĩnh Long".
    /// </para>
    /// </summary>
    private static FrozenDictionary<string, string> ProvinceDisplayNames => Tables.DisplayNames;

    /// <summary>
    /// The spoken provincial name for a delivery area, or <see langword="null"/> when no
    /// provincial unit can be identified — the same silence <see cref="TryResolve"/> returns, for
    /// the same reason: a wrong guess reads the wrong place to a real customer.
    /// </summary>
    public static string? TryResolveProvinceName(string? deliveryAreaShort)
    {
        string? key = TryMatchProvinceKey(deliveryAreaShort);
        return key is null ? null : ProvinceDisplayNames[key];
    }

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
        string? key = TryMatchProvinceKey(deliveryAreaShort);
        return key is null ? null : ProvinceRegions[key];
    }

    /// <summary>
    /// The one match. Region and spoken name are both projections of it, deliberately that way
    /// round: two independent scans would be two chances to disagree about which province a string
    /// names, and the voice and the words would then describe different places in the same call.
    /// </summary>
    private static string? TryMatchProvinceKey(string? deliveryAreaShort)
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
            if (candidate.Length > 0 && ProvinceRegions.ContainsKey(candidate))
            {
                return candidate;
            }
        }

        return ScanForLatestProvince(normalized);
    }

    /// <summary>
    /// Fallback for strings the comma split cannot handle — a missing separator, a trailing
    /// "Việt Nam", or an extra word glued onto the province token. Matches on whole-word
    /// sequences, never raw substrings, so "hue" cannot be found inside "thue".
    /// </summary>
    private static string? ScanForLatestProvince(string normalized)
    {
        string[] words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int bestPosition = -1;
        int bestLength = 0;
        string? best = null;

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
            best = string.Join(' ', provinceWords);
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

    private static (
        FrozenDictionary<string, VietnamRegion>,
        FrozenDictionary<string, string>) BuildTables()
    {
        var table = new Dictionary<string, VietnamRegion>(StringComparer.Ordinal);
        var display = new Dictionary<string, string>(StringComparer.Ordinal);

        // ---- MIỀN BẮC — 15 current units -------------------------------------------------
        Add(table, display, VietnamRegion.North, "Hà Nội");
        Add(table, display, VietnamRegion.North, "Hải Phòng", "Hải Dương");
        Add(table, display, VietnamRegion.North, "Quảng Ninh");
        Add(table, display, VietnamRegion.North, "Cao Bằng");
        Add(table, display, VietnamRegion.North, "Lạng Sơn");
        Add(table, display, VietnamRegion.North, "Lai Châu");
        Add(table, display, VietnamRegion.North, "Điện Biên");
        Add(table, display, VietnamRegion.North, "Sơn La");
        Add(table, display, VietnamRegion.North, "Lào Cai", "Yên Bái");
        Add(table, display, VietnamRegion.North, "Tuyên Quang", "Hà Giang");
        Add(table, display, VietnamRegion.North, "Thái Nguyên", "Bắc Kạn", "Bắc Cạn");
        Add(table, display, VietnamRegion.North, "Phú Thọ", "Vĩnh Phúc", "Hòa Bình");
        Add(table, display, VietnamRegion.North, "Bắc Ninh", "Bắc Giang");
        Add(table, display, VietnamRegion.North, "Hưng Yên", "Thái Bình");
        Add(table, display, VietnamRegion.North, "Ninh Bình", "Hà Nam", "Nam Định");

        // ---- MIỀN TRUNG — 11 current units -----------------------------------------------
        Add(table, display, VietnamRegion.Central, "Thanh Hóa");
        Add(table, display, VietnamRegion.Central, "Nghệ An");
        Add(table, display, VietnamRegion.Central, "Hà Tĩnh");
        Add(table, display, VietnamRegion.Central, "Quảng Trị", "Quảng Bình");
        Add(table, display, VietnamRegion.Central, "Huế", "Thừa Thiên Huế");
        Add(table, display, VietnamRegion.Central, "Đà Nẵng", "Quảng Nam");
        Add(table, display, VietnamRegion.Central, "Quảng Ngãi", "Kon Tum");
        Add(table, display, VietnamRegion.Central, "Gia Lai", "Bình Định");
        Add(table, display, VietnamRegion.Central, "Đắk Lắk", "Đắc Lắc", "Phú Yên");
        Add(table, display, VietnamRegion.Central, "Khánh Hòa", "Ninh Thuận");
        Add(table, display, VietnamRegion.Central, "Lâm Đồng", "Đắk Nông", "Đắc Nông", "Bình Thuận");

        // ---- MIỀN NAM — 8 current units ---------------------------------------------------
        Add(
            table,
            display,
            VietnamRegion.South,
            "Hồ Chí Minh",
            "TPHCM",
            "HCM",
            "Sài Gòn",
            "Bình Dương",
            "Bà Rịa Vũng Tàu",
            "Vũng Tàu");
        Add(table, display, VietnamRegion.South, "Đồng Nai", "Bình Phước");
        Add(table, display, VietnamRegion.South, "Tây Ninh", "Long An");
        Add(table, display, VietnamRegion.South, "Cần Thơ", "Sóc Trăng", "Hậu Giang");
        Add(table, display, VietnamRegion.South, "Vĩnh Long", "Bến Tre", "Trà Vinh");
        Add(table, display, VietnamRegion.South, "Đồng Tháp", "Tiền Giang");
        Add(table, display, VietnamRegion.South, "An Giang", "Kiên Giang");
        Add(table, display, VietnamRegion.South, "Cà Mau", "Bạc Liêu");

        return (
            table.ToFrozenDictionary(StringComparer.Ordinal),
            display.ToFrozenDictionary(StringComparer.Ordinal));
    }

    // Both tables are filled from the same argument list, in one pass. A separate hand-written
    // list of spoken names could drift from the match keys, and a drifted entry is a province the
    // call names wrongly -- with the right voice, which makes it harder to notice.
    private static void Add(
        Dictionary<string, VietnamRegion> table,
        Dictionary<string, string> display,
        VietnamRegion region,
        params string[] names)
    {
        foreach (string name in names)
        {
            string key = VietnameseTextNormalizer.ToMatchKey(name);
            table.Add(key, region);
            display.Add(key, names[0]);
        }
    }
}
