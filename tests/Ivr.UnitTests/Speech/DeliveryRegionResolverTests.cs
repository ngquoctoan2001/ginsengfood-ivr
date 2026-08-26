using Ivr.Domain.Confirmation;
using Ivr.Domain.Speech;

namespace Ivr.UnitTests.Speech;

public sealed class DeliveryRegionResolverTests
{
    /// <summary>
    /// The 34 provincial units created by Nghị quyết 202/2025/QH15, effective 2025-07-01.
    /// </summary>
    public static TheoryData<string, VietnamRegion> CurrentProvinces() => new()
    {
        // MIỀN BẮC — 15
        { "Hà Nội", VietnamRegion.North },
        { "Hải Phòng", VietnamRegion.North },
        { "Quảng Ninh", VietnamRegion.North },
        { "Cao Bằng", VietnamRegion.North },
        { "Lạng Sơn", VietnamRegion.North },
        { "Lai Châu", VietnamRegion.North },
        { "Điện Biên", VietnamRegion.North },
        { "Sơn La", VietnamRegion.North },
        { "Lào Cai", VietnamRegion.North },
        { "Tuyên Quang", VietnamRegion.North },
        { "Thái Nguyên", VietnamRegion.North },
        { "Phú Thọ", VietnamRegion.North },
        { "Bắc Ninh", VietnamRegion.North },
        { "Hưng Yên", VietnamRegion.North },
        { "Ninh Bình", VietnamRegion.North },

        // MIỀN TRUNG — 11
        { "Thanh Hóa", VietnamRegion.Central },
        { "Nghệ An", VietnamRegion.Central },
        { "Hà Tĩnh", VietnamRegion.Central },
        { "Quảng Trị", VietnamRegion.Central },
        { "Huế", VietnamRegion.Central },
        { "Đà Nẵng", VietnamRegion.Central },
        { "Quảng Ngãi", VietnamRegion.Central },
        { "Gia Lai", VietnamRegion.Central },
        { "Đắk Lắk", VietnamRegion.Central },
        { "Khánh Hòa", VietnamRegion.Central },
        { "Lâm Đồng", VietnamRegion.Central },

        // MIỀN NAM — 8
        { "Hồ Chí Minh", VietnamRegion.South },
        { "Đồng Nai", VietnamRegion.South },
        { "Tây Ninh", VietnamRegion.South },
        { "Cần Thơ", VietnamRegion.South },
        { "Vĩnh Long", VietnamRegion.South },
        { "Đồng Tháp", VietnamRegion.South },
        { "An Giang", VietnamRegion.South },
        { "Cà Mau", VietnamRegion.South },
    };

    /// <summary>
    /// The 29 province names abolished by the 2025 merger. Sales master data and in-flight
    /// orders still carry them, so each must land on the region of its successor unit.
    /// </summary>
    public static TheoryData<string, VietnamRegion> AbolishedProvinces() => new()
    {
        { "Hải Dương", VietnamRegion.North },
        { "Yên Bái", VietnamRegion.North },
        { "Hà Giang", VietnamRegion.North },
        { "Bắc Kạn", VietnamRegion.North },
        { "Vĩnh Phúc", VietnamRegion.North },
        { "Hòa Bình", VietnamRegion.North },
        { "Bắc Giang", VietnamRegion.North },
        { "Thái Bình", VietnamRegion.North },
        { "Hà Nam", VietnamRegion.North },
        { "Nam Định", VietnamRegion.North },

        { "Quảng Bình", VietnamRegion.Central },
        { "Quảng Nam", VietnamRegion.Central },
        { "Kon Tum", VietnamRegion.Central },
        { "Bình Định", VietnamRegion.Central },
        { "Phú Yên", VietnamRegion.Central },
        { "Ninh Thuận", VietnamRegion.Central },
        { "Đắk Nông", VietnamRegion.Central },
        { "Bình Thuận", VietnamRegion.Central },

        { "Bình Dương", VietnamRegion.South },
        { "Bà Rịa - Vũng Tàu", VietnamRegion.South },
        { "Bình Phước", VietnamRegion.South },
        { "Long An", VietnamRegion.South },
        { "Sóc Trăng", VietnamRegion.South },
        { "Hậu Giang", VietnamRegion.South },
        { "Bến Tre", VietnamRegion.South },
        { "Trà Vinh", VietnamRegion.South },
        { "Tiền Giang", VietnamRegion.South },
        { "Kiên Giang", VietnamRegion.South },
        { "Bạc Liêu", VietnamRegion.South },
    };

    [Theory]
    [MemberData(nameof(CurrentProvinces))]
    [Trait("TestId", "UT-VOICE-REGION-01")]
    public void EveryCurrentProvinceResolvesToItsRegion(string province, VietnamRegion expected)
    {
        Assert.Equal(expected, DeliveryRegionResolver.TryResolve($"phường Trung Tâm, tỉnh {province}"));
        Assert.Equal(expected, DeliveryRegionResolver.TryResolve(province));
    }

    [Theory]
    [MemberData(nameof(AbolishedProvinces))]
    [Trait("TestId", "UT-VOICE-REGION-02")]
    public void EveryAbolishedProvinceResolvesToItsSuccessorRegion(
        string province,
        VietnamRegion expected) =>
        Assert.Equal(expected, DeliveryRegionResolver.TryResolve($"phường Trung Tâm, tỉnh {province}"));

    [Fact]
    [Trait("TestId", "UT-VOICE-REGION-03")]
    public void AllThirtyFourCurrentProvincesAreCoveredAndSplitFifteenElevenEight()
    {
        TheoryData<string, VietnamRegion> current = CurrentProvinces();
        (string Province, VietnamRegion Region)[] rows = current
            .Select(row => ((string)row[0]!, (VietnamRegion)row[1]!))
            .ToArray();

        Assert.Equal(34, rows.Length);
        Assert.Equal(15, rows.Count(row => row.Region == VietnamRegion.North));
        Assert.Equal(11, rows.Count(row => row.Region == VietnamRegion.Central));
        Assert.Equal(8, rows.Count(row => row.Region == VietnamRegion.South));
        Assert.Equal(29, AbolishedProvinces().Count);
    }

    [Theory]
    [Trait("TestId", "UT-VOICE-REGION-04")]
    // Diacritics, casing, unit prefixes and separators all vary in real Sales data.
    [InlineData("phường Phú Khương, tỉnh Vĩnh Long")]
    [InlineData("phuong Phu Khuong, tinh Vinh Long")]
    [InlineData("PHƯỜNG PHÚ KHƯƠNG, TỈNH VĨNH LONG")]
    [InlineData("Phú Khương, Vĩnh Long")]
    [InlineData("phường Phú Khương, tỉnh Vĩnh Long, Việt Nam")]
    [InlineData("  phường Phú Khương ,  tỉnh   Vĩnh Long  ")]
    public void VinhLongIsSouthRegardlessOfSpellingOrTrailingCountry(string deliveryArea) =>
        // Phú Khương was a ward of the old Bến Tre province; after the merger it is Vĩnh Long,
        // and Vĩnh Long is Southern. This is the exact string in the approved W-0104 audio.
        Assert.Equal(VietnamRegion.South, DeliveryRegionResolver.TryResolve(deliveryArea));

    [Theory]
    [Trait("TestId", "UT-VOICE-REGION-05")]
    [InlineData("Thành phố Huế")]
    [InlineData("tỉnh Thừa Thiên Huế")]
    [InlineData("phường Vỹ Dạ, thành phố Huế")]
    public void HueResolvesThroughItsCurrentAndFormerNames(string deliveryArea) =>
        Assert.Equal(VietnamRegion.Central, DeliveryRegionResolver.TryResolve(deliveryArea));

    [Theory]
    [Trait("TestId", "UT-VOICE-REGION-06")]
    [InlineData("Phường Bến Nghé, TP. Hồ Chí Minh")]
    [InlineData("Phường Bến Nghé, TPHCM")]
    [InlineData("Phường 12, Thành phố Hồ Chí Minh")]
    [InlineData("Phường Thắng Tam, Bà Rịa - Vũng Tàu")]
    [InlineData("Phường Phú Lợi, tỉnh Bình Dương")]
    public void HoChiMinhCityAndItsAbsorbedProvincesResolveSouth(string deliveryArea) =>
        Assert.Equal(VietnamRegion.South, DeliveryRegionResolver.TryResolve(deliveryArea));

    [Fact]
    [Trait("TestId", "UT-VOICE-REGION-07")]
    public void ProvinceAtTheEndWinsOverAWardThatSharesAProvinceName()
    {
        // "Long An" is a Southern province and also a plausible commune name. Reading the
        // address right-to-left is what stops a Northern order being read in a Southern voice.
        Assert.Equal(
            VietnamRegion.North,
            DeliveryRegionResolver.TryResolve("xã Long An, tỉnh Ninh Bình"));
        Assert.Equal(
            VietnamRegion.Central,
            DeliveryRegionResolver.TryResolve("xã Hà Nội, tỉnh Nghệ An"));
        Assert.Equal(
            VietnamRegion.South,
            DeliveryRegionResolver.TryResolve("xã An Giang, tỉnh Đồng Tháp"));
    }

    [Theory]
    [Trait("TestId", "UT-VOICE-REGION-08")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("phường 12, quận Bình Thạnh")]
    [InlineData("khu vực chưa xác định")]
    [InlineData("Thuế")]
    public void UnknownAreaReturnsNullInsteadOfGuessing(string? deliveryArea) =>
        // "Thuế" contains "hue" as raw text. Matching on whole words rather than substrings is
        // what keeps it from being read in a Central voice.
        Assert.Null(DeliveryRegionResolver.TryResolve(deliveryArea));

    [Fact]
    [Trait("TestId", "UT-VOICE-REGION-09")]
    public void MatchingNormalizerAgreesWithThePrivacyGuardOnDiacriticFolding()
    {
        // VietnameseTextNormalizer deliberately duplicates the diacritic folding that
        // ShortDeliveryArea keeps private, so that widening place-name matching can never
        // loosen the full-address guard. This pins the two together: if the guard's folding
        // changes, this fails rather than the duplication drifting silently.
        string[] corpus =
        [
            "Đường Nguyễn Huệ", "phường Phú Khương", "Bà Rịa - Vũng Tàu", "Đắk Lắk", "Huế",
        ];

        foreach (string sample in corpus)
        {
            Assert.Equal(
                sample.Length,
                VietnameseTextNormalizer.RemoveDiacritics(sample).Length);
            Assert.DoesNotContain('đ', VietnameseTextNormalizer.RemoveDiacritics(sample));
        }

        // The guard still rejects a street address; region matching does not weaken it.
        Assert.Throws<InvalidOperationException>(
            () => ShortDeliveryArea.Create("Đường Nguyễn Huệ, phường Bến Nghé"));
    }

    [Fact]
    [Trait("TestId", "UT-VOICE-REGION-10")]
    public void EveryAreaAcceptedByThePrivacyGuardForTheThirtyFourUnitsAlsoResolves()
    {
        // The two rules must agree in practice: an area string privacy accepts should not then
        // fall through to the fallback voice.
        foreach (object?[] row in CurrentProvinces())
        {
            string province = (string)row[0]!;
            ShortDeliveryArea area = ShortDeliveryArea.Create($"phường Trung Tâm, tỉnh {province}");

            Assert.Equal((VietnamRegion)row[1]!, DeliveryRegionResolver.TryResolve(area.Value));
        }
    }

    /// <summary>
    /// The six centrally governed cities, written the way Sales actually writes them.
    /// </summary>
    public static TheoryData<string, VietnamRegion> CentrallyGovernedCities() => new()
    {
        { "Hà Nội", VietnamRegion.North },
        { "Hải Phòng", VietnamRegion.North },
        { "Huế", VietnamRegion.Central },
        { "Đà Nẵng", VietnamRegion.Central },
        { "Hồ Chí Minh", VietnamRegion.South },
        { "Cần Thơ", VietnamRegion.South },
    };

    [Theory]
    [MemberData(nameof(CentrallyGovernedCities))]
    [Trait("TestId", "UT-VOICE-REGION-11")]
    public void ACentrallyGovernedCityIsNotMistakenForAStreetAddress(
        string city,
        VietnamRegion expected)
    {
        // UT-VOICE-REGION-10 writes every unit as "tỉnh {province}", so nothing above ever
        // exercised "thành phố" — and "phố" is one of the street markers the privacy guard
        // rejects. The guard exempts the "thành phố " prefix, but that exemption is applied to
        // diacritic-folded text, so it only works when the folding actually folds.
        //
        // On 2026-08-26 it did not: the deployed runtime is a chiseled image with no ICU, where
        // string.Normalize is a silent no-op, so the fold returned its input untouched and every
        // one of these six cities was rejected as a full address. Six cities is not an edge case;
        // it is where the orders are.
        ShortDeliveryArea area = ShortDeliveryArea.Create($"phường Trung Tâm, thành phố {city}");

        Assert.Equal(expected, DeliveryRegionResolver.TryResolve(area.Value));
    }

    [Fact]
    [Trait("TestId", "UT-VOICE-REGION-12")]
    public void DiacriticFoldingDoesNotDependOnUnicodeNormalization()
    {
        // Precomposed (NFC) input is what arrives on the wire, and NFC is exactly the form that
        // needs decomposition to fold. Asserting the folded value directly — rather than only
        // asserting lengths, which UT-VOICE-REGION-09 does — is what makes this fail if the
        // implementation ever goes back to depending on the host having ICU.
        Assert.Equal(
            "thanh pho Ha Noi",
            VietnameseTextNormalizer.RemoveDiacritics("thành phố Hà Nội"));
        Assert.Equal(
            "Duong Nguyen Hue",
            VietnameseTextNormalizer.RemoveDiacritics("Đường Nguyễn Huệ"));
        Assert.Equal(
            "Dak Lak, Ba Ria - Vung Tau",
            VietnameseTextNormalizer.RemoveDiacritics("Đắk Lắk, Bà Rịa - Vũng Tàu"));

        // Same property through the privacy guard: the exemption fires, the street marker does
        // not, and a real street is still refused.
        ShortDeliveryArea.Create("thành phố Hà Nội");
        Assert.Throws<InvalidOperationException>(() => ShortDeliveryArea.Create("phố Hà Nội"));
    }
}
