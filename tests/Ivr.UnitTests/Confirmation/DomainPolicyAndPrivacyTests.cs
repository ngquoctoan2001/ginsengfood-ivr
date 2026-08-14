using Ivr.Domain.Confirmation;

namespace Ivr.UnitTests.Confirmation;

public sealed class DomainPolicyAndPrivacyTests
{
    public static TheoryData<IvrProgramCode, PaymentMethod, bool, bool> ProgramPaymentCases => new()
    {
        { IvrProgramCode.GoldenHour, PaymentMethod.Online, true, true },
        { IvrProgramCode.GoldenHour, PaymentMethod.CashOnDelivery, true, false },
        { IvrProgramCode.TwentyFourSeven, PaymentMethod.Online, true, false },
        { IvrProgramCode.TwentyFourSeven, PaymentMethod.CashOnDelivery, true, true },
        { IvrProgramCode.GoldenHour, PaymentMethod.Online, false, false },
        { IvrProgramCode.TwentyFourSeven, PaymentMethod.CashOnDelivery, false, false },
    };

    [Theory]
    [MemberData(nameof(ProgramPaymentCases))]
    public void ProgramPaymentMatrixIsFailClosed(
        IvrProgramCode program,
        PaymentMethod payment,
        bool required,
        bool accepted)
    {
        if (accepted)
        {
            ProgramPayment result = ProgramPaymentPolicy.EnsureAllowed(program, payment, required);
            Assert.True(result.IvrConfirmationRequired);
            return;
        }

        Assert.Throws<InvalidOperationException>(() =>
            ProgramPaymentPolicy.EnsureAllowed(program, payment, required));
    }

    [Fact]
    public void CandidatePolicyIsRejectedInProductionButAllowedInMockAndLab()
    {
        AttemptPolicySnapshot policy = TestData.Policy(AttemptPolicyApproval.CandidateMockLabOnly);

        policy.EnsureEnvironmentAllowed(ExecutionMode.Mock);
        policy.EnsureEnvironmentAllowed(ExecutionMode.LabRealSim);
        Assert.Throws<InvalidOperationException>(() =>
            policy.EnsureEnvironmentAllowed(ExecutionMode.ProductionReal));
    }

    [Fact]
    public void AttemptPolicyRejectsUnorderedOffsetsAndOffsetsOutsideWindow()
    {
        Assert.Throws<InvalidOperationException>(() => AttemptPolicySnapshot.Create(
            PolicyVersion.Create("policy-v1"),
            IvrProgramCode.GoldenHour,
            2,
            [TimeSpan.Zero, TimeSpan.Zero],
            TimeSpan.FromMinutes(5),
            AttemptPolicyApproval.OwnerApproved));
        Assert.Throws<InvalidOperationException>(() => AttemptPolicySnapshot.Create(
            PolicyVersion.Create("policy-v1"),
            IvrProgramCode.GoldenHour,
            2,
            [TimeSpan.Zero, TimeSpan.FromMinutes(5)],
            TimeSpan.FromMinutes(5),
            AttemptPolicyApproval.OwnerApproved));
    }

    [Fact]
    public void UnicodeSpeechSummaryPreservesSafeVietnameseContentAndVnd()
    {
        PrivacySafeOrderSummary summary = TestData.Summary();

        Assert.Equal("Anh Đạt", summary.CustomerDisplayName);
        Assert.Equal("Hộp", summary.Items[0].UnitLabel);
        Assert.Equal("VND", summary.Total.Currency);
        Assert.Equal("vi-VN", summary.Locale);
        Assert.Equal("Phường Bến Nghé, Quận 1", summary.DeliveryArea.Value);
    }

    [Theory]
    [InlineData("Quận 1, Thành phố Hồ Chí Minh")]
    [InlineData("Khu đô thị cao cấp, Quận 7")]
    public void ShortAreaAcceptsCityAndBenignWords(string value)
    {
        Assert.Equal(value, ShortDeliveryArea.Create(value).Value);
    }

    [Theory]
    [InlineData("phone")]
    [InlineData("dial-token")]
    [InlineData("street-keyword")]
    [InlineData("slash-address")]
    [InlineData("numeric-street")]
    public void PrivacyGuardRejectsPhoneTokenAndFullAddress(string caseId)
    {
        string unsafeValue = caseId switch
        {
            "phone" => "0901234567",
            "dial-token" => "dial-token: abcdefghijk",
            "street-keyword" => "Đường Nguyễn Huệ, Phường Bến Nghé",
            "slash-address" => "12/3 Lê Lợi",
            "numeric-street" => "123 Nguyễn Huệ",
            _ => throw new ArgumentOutOfRangeException(nameof(caseId)),
        };

        Assert.Throws<InvalidOperationException>(() => ShortDeliveryArea.Create(unsafeValue));
    }

    [Fact]
    public void SpeechItemCountIsConfigurationDriven()
    {
        SpeechItem item = SpeechItem.Create("Sâm lát", 1, "Hộp");
        Assert.Throws<InvalidOperationException>(() => PrivacySafeOrderSummary.Create(
            "Chị Mai",
            "DH-1",
            [item, item],
            Money.Vnd(100_000),
            ShortDeliveryArea.Create("Quận 7, TP Hồ Chí Minh"),
            "Giờ Vàng",
            null,
            SpeechSummaryLimits.Create(1, 0)));
    }

    [Theory]
    [InlineData("local")]
    [InlineData("international-spaced")]
    public void DialTokenReferenceRejectsRawPhoneData(string caseId)
    {
        string rawPhone = caseId switch
        {
            "local" => "0901234567",
            "international-spaced" => "+84 901 234 567",
            _ => throw new ArgumentOutOfRangeException(nameof(caseId)),
        };

        Assert.Throws<InvalidOperationException>(() =>
            DialTokenReference.Create(
                rawPhone,
                new DateTimeOffset(2026, 8, 13, 1, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public void SnapshotHashIsDeterministicAndCollectionsAreImmutable()
    {
        PrivacySafeOrderSummary first = TestData.Summary(new Dictionary<string, string>
        {
            ["Sâm"] = "xâm",
            ["Đạt"] = "đạt",
        });
        PrivacySafeOrderSummary second = TestData.Summary(new Dictionary<string, string>
        {
            ["Đạt"] = "đạt",
            ["Sâm"] = "xâm",
        });

        Assert.Equal(first.ComputeHash(), second.ComputeHash());
        Assert.Throws<NotSupportedException>(() =>
            ((IList<SpeechItem>)first.Items).Add(SpeechItem.Create("Khác", 1, null)));
        Assert.Throws<NotSupportedException>(() =>
            ((IDictionary<string, string>)first.PronunciationHints).Add("new", "value"));
    }

    [Fact]
    public void PolicyHashChangesWhenPolicyFieldChanges()
    {
        AttemptPolicySnapshot first = TestData.Policy(AttemptPolicyApproval.OwnerApproved);
        AttemptPolicySnapshot second = AttemptPolicySnapshot.Create(
            PolicyVersion.Create("mock-lab-v2"),
            IvrProgramCode.GoldenHour,
            2,
            [TimeSpan.Zero, TimeSpan.FromSeconds(150)],
            TimeSpan.FromSeconds(300),
            AttemptPolicyApproval.OwnerApproved);

        Assert.NotEqual(first.ComputeHash(), second.ComputeHash());
    }
}

internal static class TestData
{
    internal static AttemptPolicySnapshot Policy(AttemptPolicyApproval approval) =>
        AttemptPolicySnapshot.Create(
            PolicyVersion.Create("mock-lab-v1"),
            IvrProgramCode.GoldenHour,
            2,
            [TimeSpan.Zero, TimeSpan.FromSeconds(150)],
            TimeSpan.FromSeconds(300),
            approval);

    internal static PrivacySafeOrderSummary Summary(
        IReadOnlyDictionary<string, string>? hints = null) =>
        PrivacySafeOrderSummary.Create(
            "Anh Đạt",
            "DH-2026-001",
            [SpeechItem.Create("Sâm lát", 2, "Hộp")],
            Money.Vnd(1_250_000),
            ShortDeliveryArea.Create("Phường Bến Nghé, Quận 1"),
            "Giờ Vàng",
            hints,
            SpeechSummaryLimits.Create(20, 20));

    internal static CallResultSnapshot Result(
        IvrResultType resultType = IvrResultType.IvrConfirmed,
        bool counted = true,
        CoreActionRecommendation action = CoreActionRecommendation.RevalidateAndConfirmOrder,
        IvrProgramCode program = IvrProgramCode.GoldenHour) =>
        CallResultSnapshot.Create(
            CallbackId.Create("callback-1"),
            TaskId.Create("task-1"),
            OrderId.Create("order-1"),
            OrderVersion.Create("order-version-1"),
            program,
            resultType,
            "safe-result",
            counted,
            true,
            2,
            new DateTimeOffset(2026, 8, 13, 1, 2, 3, TimeSpan.Zero),
            action,
            EvidenceReference.Create("evidence-1"),
            AuditReference.Create("audit-1"));
}
