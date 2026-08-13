using Ivr.Domain.Confirmation;
using Ivr.Domain.Scripts;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Scripts;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Scripts;

public sealed class ScriptContentTests
{
    [Fact]
    [Trait("TestId", "UT-SCRIPT-SEED-01")]
    public async Task MockSeedIsApprovedButLabAndProductionRemainFailClosed()
    {
        using InMemoryScriptRegistry registry = CreateRegistry(productionFieldsApproved: false);

        ApprovedScript? mock = await registry.TryGetApproved(
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock);
        ApprovedScript? lab = await registry.TryGetApproved(
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.LabRealSim);
        ApprovedScript? production = await registry.TryGetApproved(
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.ProductionReal);

        Assert.NotNull(mock);
        Assert.Null(lab);
        Assert.Null(production);
        Assert.Equal(ScriptLifecycleStatus.Approved, mock.Version.Status);
        Assert.Contains(mock.Version.Approvals, approval =>
            approval.Type == ScriptApprovalType.MockTest);
    }

    [Fact]
    [Trait("TestId", "UT-SCRIPT-LIFECYCLE-02")]
    public async Task LifecycleEnforcesRbacFourEyesAuditModeGatesAndRetirement()
    {
        var audit = new InMemoryAuditLogger(TimeProvider.System);
        using var registry = new InMemoryScriptRegistry(
            audit,
            TimeProvider.System,
            Options.Create(new ScriptContentOptions
            {
                ProductionTargetV1FieldsApproved = true,
            }));
        ScriptDraftDefinition definition = ScriptDraftDefinition.Create(
            "SCRIPT-ORDER-CONFIRM",
            "v2",
            TargetV1SpeechPolicy.CanonicalVietnameseTemplate);
        ScriptActor author = Actor("author", ScriptPermissions.Edit);
        ScriptActor reviewer = Actor("reviewer", ScriptPermissions.Review);
        ScriptActor mockApprover = Actor("mock-approver", ScriptPermissions.ApproveMock);
        ScriptActor labApprover = Actor("lab-approver", ScriptPermissions.ApproveLab);
        ScriptActor contentApprover = Actor("content-approver", ScriptPermissions.ApproveContent);
        ScriptActor privacyApprover = Actor(
            "privacy-approver",
            ScriptPermissions.ApprovePrivacyLegal);
        ScriptActor retireActor = Actor("retirer", ScriptPermissions.Retire);

        ScriptVersionSnapshot draft = await registry.CreateDraftAsync(
            definition,
            author,
            "Create reviewed Target V1 script",
            "corr-script-v2-create");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await registry.ApproveAsync(
                definition.Key,
                ScriptApprovalType.MockTest,
                reviewer,
                "Invalid permission",
                "corr-script-v2-invalid"));
        ScriptVersionSnapshot review = await registry.SubmitForReviewAsync(
            definition.Key,
            reviewer,
            "Submit exact content for review",
            "corr-script-v2-review");
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await registry.ApproveAsync(
                definition.Key,
                ScriptApprovalType.MockTest,
                Actor("author", ScriptPermissions.ApproveMock),
                "Creator cannot self approve",
                "corr-script-v2-self"));

        ScriptVersionSnapshot mockApproved = await registry.ApproveAsync(
            definition.Key,
            ScriptApprovalType.MockTest,
            mockApprover,
            "Approve synthetic MOCK script",
            "corr-script-v2-mock");
        Assert.NotNull(await registry.TryGetApproved(
            definition.Key.TemplateId,
            definition.Key.Version,
            ExecutionMode.Mock));
        Assert.Null(await registry.TryGetApproved(
            definition.Key.TemplateId,
            definition.Key.Version,
            ExecutionMode.LabRealSim));

        await registry.ApproveAsync(
            definition.Key,
            ScriptApprovalType.Lab,
            labApprover,
            "Approve allowlisted LAB script",
            "corr-script-v2-lab");
        await registry.ApproveAsync(
            definition.Key,
            ScriptApprovalType.Content,
            contentApprover,
            "Approve Vietnamese content",
            "corr-script-v2-content");
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await registry.ApproveAsync(
                definition.Key,
                ScriptApprovalType.PrivacyLegal,
                Actor("content-approver", ScriptPermissions.ApprovePrivacyLegal),
                "Same actor cannot approve Privacy Legal",
                "corr-script-v2-same-actor"));
        ScriptVersionSnapshot productionApproved = await registry.ApproveAsync(
            definition.Key,
            ScriptApprovalType.PrivacyLegal,
            privacyApprover,
            "Approve Target V1 privacy fields",
            "corr-script-v2-privacy");

        Assert.NotNull(await registry.TryGetApproved(
            definition.Key.TemplateId,
            definition.Key.Version,
            ExecutionMode.LabRealSim));
        Assert.NotNull(await registry.TryGetApproved(
            definition.Key.TemplateId,
            definition.Key.Version,
            ExecutionMode.ProductionReal));
        Assert.Equal(draft.TemplateHash, review.TemplateHash);
        Assert.Equal(draft.TemplateHash, mockApproved.TemplateHash);
        Assert.Equal(draft.TemplateHash, productionApproved.TemplateHash);

        await registry.RetireAsync(
            definition.Key,
            retireActor,
            "Retire superseded script version",
            "corr-script-v2-retire");
        Assert.Null(await registry.TryGetApproved(
            definition.Key.TemplateId,
            definition.Key.Version,
            ExecutionMode.Mock));
        Assert.Equal(7, audit.Entries.Count);
        Assert.All(audit.Entries, entry =>
        {
            Assert.DoesNotContain("Xin chào", entry.DataJson, StringComparison.Ordinal);
            Assert.DoesNotContain("customer_display_name", entry.DataJson, StringComparison.Ordinal);
        });
    }

    [Fact]
    [Trait("TestId", "UT-SCRIPT-PROD-GATE-03")]
    public async Task ProductionTargetV1FieldsStayBlockedWithoutOwnerDecision()
    {
        using InMemoryScriptRegistry registry = CreateRegistry(productionFieldsApproved: false);
        ScriptDraftDefinition definition = ScriptDraftDefinition.Create(
            "SCRIPT-ORDER-CONFIRM",
            "v3",
            TargetV1SpeechPolicy.CanonicalVietnameseTemplate);
        await registry.CreateDraftAsync(
            definition,
            Actor("author-v3", ScriptPermissions.Edit),
            "Create production candidate",
            "corr-script-v3-create");
        await registry.SubmitForReviewAsync(
            definition.Key,
            Actor("reviewer-v3", ScriptPermissions.Review),
            "Review production candidate",
            "corr-script-v3-review");
        await registry.ApproveAsync(
            definition.Key,
            ScriptApprovalType.Content,
            Actor("content-v3", ScriptPermissions.ApproveContent),
            "Content approval",
            "corr-script-v3-content");
        await registry.ApproveAsync(
            definition.Key,
            ScriptApprovalType.PrivacyLegal,
            Actor("privacy-v3", ScriptPermissions.ApprovePrivacyLegal),
            "Privacy approval",
            "corr-script-v3-privacy");

        Assert.Null(await registry.TryGetApproved(
            definition.Key.TemplateId,
            definition.Key.Version,
            ExecutionMode.ProductionReal));
    }

    [Theory]
    [Trait("TestId", "UT-SCRIPT-TEMPLATE-GUARD-04")]
    [InlineData("Xin chào {{unknown_field}}. Bấm phím 1, bấm phím 0.")]
    [InlineData("<b>Xin chào</b> {{customer_display_name}} {{order_code_short}} {{items_spoken}} {{total_amount_display}} {{delivery_area_short}}. Bấm phím 1, bấm phím 0.")]
    [InlineData("Xin chào {{customer_display_name}} {{order_code_short}} {{items_spoken}} {{total_amount_display}} {{delivery_area_short}}. Gọi 0901234567. Bấm phím 1, bấm phím 0.")]
    [InlineData("Xin chào {{customer_display_name}} {{order_code_short}} {{items_spoken}} {{total_amount_display}} {{delivery_area_short}}. Bấm phím 9, bấm phím 1, bấm phím 0.")]
    public void TemplateRejectsUnknownHtmlPiiAndUnsupportedInstructions(string template)
    {
        Assert.ThrowsAny<Exception>(() => ScriptDraftDefinition.Create("SCRIPT-TEST", "v1", template));
    }

    [Fact]
    [Trait("TestId", "UT-SCRIPT-INPUT-GUARD-05")]
    public void TemplateAndSpeechInputRejectMissingOversizedControlAndFullAddressData()
    {
        Assert.ThrowsAny<Exception>(() => ScriptDraftDefinition.Create(
            "SCRIPT-TEST",
            "v1",
            string.Concat(TargetV1SpeechPolicy.CanonicalVietnameseTemplate, "\u0001")));
        Assert.ThrowsAny<Exception>(() => ScriptDraftDefinition.Create(
            "SCRIPT-TEST",
            "v1",
            string.Concat(
                TargetV1SpeechPolicy.CanonicalVietnameseTemplate,
                new string('x', 2_000))));
        Assert.ThrowsAny<Exception>(() => Summary(
            [SpeechItem.Create("Sản phẩm", 1, null)],
            100_000m,
            "12 đường Nguyễn Huệ"));
        Assert.Equal(
            "Thành phố Hồ Chí Minh",
            ShortDeliveryArea.Create("Thành phố Hồ Chí Minh").Value);
        Assert.ThrowsAny<Exception>(() => ShortDeliveryArea.Create(
            "Thành phố Hồ Chí Minh, đường Nguyễn Huệ"));
        Assert.ThrowsAny<Exception>(() => PrivacySafeOrderSummary.Create(
            string.Empty,
            "DH-A1",
            [SpeechItem.Create("Sản phẩm", 1, null)],
            Money.Vnd(100_000m),
            ShortDeliveryArea.Create("Quận 1"),
            "Golden Hour",
            null,
            SpeechSummaryLimits.Create(20, 5)));
        Assert.ThrowsAny<Exception>(() => SpeechItem.Create(
            "Liên hệ 0901234567",
            1,
            null));
        Assert.ThrowsAny<Exception>(() => SpeechItem.Create(
            new string('x', 161),
            1,
            null));
    }

    [Fact]
    [Trait("TestId", "UT-SCRIPT-RENDER-GOLDEN-06")]
    public async Task RendererPreservesVietnameseOneItemCurrencyAreaInstructionsAndHashes()
    {
        using InMemoryScriptRegistry registry = CreateRegistry(productionFieldsApproved: false);
        ApprovedScript approved = Assert.IsType<ApprovedScript>(await registry.TryGetApproved(
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock));
        PrivacySafeOrderSummary summary = Summary(
            [SpeechItem.Create("Cháo sâm Ginsengfood", 2, "hộp")],
            1_234_567m,
            "Quận 1");

        ScriptPreview preview = new VietnameseOrderScriptRenderer().Render(approved, summary);

        Assert.Equal(
            "Xin chào anh/chị Anh Minh. Anh/chị có đơn DH-A1 gồm 2 hộp Cháo sâm Ginsengfood, tổng tiền 1.234.567 đồng, giao đến Quận 1. Bấm phím 1 để xác nhận đơn hàng, bấm phím 0 để hủy.",
            preview.ExactText);
        Assert.Equal(summary.ComputeHash(), preview.InputSnapshot.InputHash);
        Assert.Equal("vi-VN", preview.InputSnapshot.Locale);
        Assert.True(preview.EstimatedDuration > TimeSpan.Zero);
        Assert.Equal(64, preview.ContentHash.Length);
    }

    [Fact]
    [Trait("TestId", "UT-SCRIPT-RENDER-COLLAPSE-07")]
    public async Task RendererJoinsManyItemsAndCollapsesRemainderDeterministically()
    {
        using InMemoryScriptRegistry registry = CreateRegistry(productionFieldsApproved: false);
        ApprovedScript approved = Assert.IsType<ApprovedScript>(await registry.TryGetApproved(
            TargetV1SpeechPolicy.MockTemplateId,
            TargetV1SpeechPolicy.MockTemplateVersion,
            ExecutionMode.Mock));
        PrivacySafeOrderSummary summary = Summary(
            [
                SpeechItem.Create("Sản phẩm A", 1, null),
                SpeechItem.Create("Sản phẩm B", 2, "gói"),
                SpeechItem.Create("Sản phẩm C", 3.5m, "kg"),
                SpeechItem.Create("Sản phẩm D", 4, null),
                SpeechItem.Create("Sản phẩm E", 5, null),
            ],
            9_999_999_999m,
            "Khu vực Thủ Đức");

        ScriptPreview first = new VietnameseOrderScriptRenderer().Render(approved, summary);
        ScriptPreview second = new VietnameseOrderScriptRenderer().Render(approved, summary);

        Assert.Contains(
            "1 Sản phẩm A, 2 gói Sản phẩm B, 3,5 kg Sản phẩm C, và 2 sản phẩm khác",
            first.ExactText,
            StringComparison.Ordinal);
        Assert.Contains("9.999.999.999 đồng", first.ExactText, StringComparison.Ordinal);
        Assert.Equal(first.ContentHash, second.ContentHash);
    }

    private static InMemoryScriptRegistry CreateRegistry(bool productionFieldsApproved) =>
        new(
            new InMemoryAuditLogger(TimeProvider.System),
            TimeProvider.System,
            Options.Create(new ScriptContentOptions
            {
                ProductionTargetV1FieldsApproved = productionFieldsApproved,
            }));

    private static ScriptActor Actor(string actorId, params string[] permissions) =>
        ScriptActor.Create(actorId, permissions);

    private static PrivacySafeOrderSummary Summary(
        IEnumerable<SpeechItem> items,
        decimal total,
        string area) =>
        PrivacySafeOrderSummary.Create(
            "Anh Minh",
            "DH-A1",
            items,
            Money.Vnd(total),
            ShortDeliveryArea.Create(area),
            "Golden Hour",
            null,
            SpeechSummaryLimits.Create(20, 5));
}
