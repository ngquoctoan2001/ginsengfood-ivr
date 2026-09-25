using Ivr.Domain.Scripts;

namespace Ivr.UnitTests.Scripts;

/// <summary>
/// W-0361 / K-38 (remediation plan 2026-09-25). <see cref="ScriptPreview"/> and
/// <see cref="ScriptInputSnapshot"/> are records, and a record's generated <c>ToString</c> prints
/// every member: the customer's display name, the delivery area and the exact text of the call.
/// Every other type that carries what the customer will hear already prints a marker.
/// </summary>
public sealed class ScriptRecordRedactionTests
{
    private const string CustomerName = "chị An";
    private const string DeliveryArea = "Phường Bến Nghé, Quận Một";
    private const string ExactText = "Chào chị An, đơn giao tới Phường Bến Nghé, Quận Một.";

    [Fact]
    [Trait("TestId", "UT-SCRIPT-TOSTRING-01")]
    public void NeitherRecordPrintsWhatTheCustomerWillHear()
    {
        var snapshot = new ScriptInputSnapshot(
            CustomerName,
            "CT001",
            [new ScriptInputItemSnapshot("Nước hồng sâm", 2, "hộp")],
            560_000m,
            "VND",
            DeliveryArea,
            "Giờ Vàng",
            "vi-VN",
            "input-hash");
        var preview = new ScriptPreview(
            "SCRIPT-ORDER-CONFIRM:v-k38",
            ExactText,
            TimeSpan.FromSeconds(12),
            snapshot,
            "template-hash",
            "content-hash");

        Assert.Equal("[REDACTED_SCRIPT_INPUT_SNAPSHOT]", snapshot.ToString());
        Assert.Equal("[REDACTED_SCRIPT_PREVIEW]", preview.ToString());
        foreach (string printed in new[] { snapshot.ToString(), preview.ToString(), $"{preview}" })
        {
            Assert.DoesNotContain(CustomerName, printed, StringComparison.Ordinal);
            Assert.DoesNotContain("Bến Nghé", printed, StringComparison.Ordinal);
        }

        // Only the text form changed: equality still compares the content.
        Assert.Equal(snapshot, snapshot with { });
        Assert.NotEqual(snapshot, snapshot with { CustomerDisplayName = "anh Bình" });
    }
}
