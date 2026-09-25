using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Telephony;

namespace Ivr.UnitTests.Telephony;

/// <summary>
/// W-0354 / B15 (chief worklist 2026-09-25). Two records carry a customer's number in the clear
/// since option B (W-0310): <see cref="DialTokenResolutionRequest"/> and
/// <see cref="TelephonyDispatchContext"/>. A C# record's generated <c>ToString</c> prints every
/// public property, so one structured log of either would have written the number into a log line.
/// Nothing logs them whole today; these tests make sure that stays harmless if something starts to.
/// </summary>
public sealed class DirectPhoneRedactionTests
{
    private const string Number = "+84900000001";
    private static readonly DateTimeOffset Now = new(2026, 9, 25, 3, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "UT-PHONE-TOSTRING-01")]
    public void ResolutionRequestPrintsAMarkerInsteadOfTheNumber()
    {
        var request = new DialTokenResolutionRequest(
            DialTokenReference.Create("enc:direct-TASK-B15", Now.AddMinutes(15)),
            AttemptId.Create("ATTEMPT-B15"),
            TaskId.Create("TASK-B15"),
            4,
            Number);

        string printed = request.ToString();

        Assert.DoesNotContain(Number, printed, StringComparison.Ordinal);
        Assert.DoesNotContain("84900000001", printed, StringComparison.Ordinal);
        Assert.Contains("[REDACTED_PHONE_E164]", printed, StringComparison.Ordinal);
        // The rest of the record still reads as a record: the fields an operator needs to find the
        // attempt are there, and the token keeps its own redaction.
        Assert.Contains("TASK-B15", printed, StringComparison.Ordinal);
        Assert.Contains("[REDACTED_DIAL_TOKEN]", printed, StringComparison.Ordinal);
        Assert.Contains("MaxResolves = 4", printed, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "UT-PHONE-TOSTRING-02")]
    public void DispatchContextPrintsAMarkerInsteadOfTheNumber()
    {
        var context = new TelephonyDispatchContext(
            TaskId.Create("TASK-B15"),
            DialTokenReference.Create("enc:direct-TASK-B15", Now.AddMinutes(15)),
            PrivacySafeOrderSummary.Create(
                "Quý khách",
                "DH-B15",
                [SpeechItem.Create("Cháo sâm", 2, "hộp")],
                Money.Vnd(560_000),
                ShortDeliveryArea.Create("Quận 7"),
                "Giờ Vàng",
                null,
                SpeechSummaryLimits.Create(20, 20)),
            "SCRIPT-ORDER-CONFIRM",
            "v3-test-approved",
            4,
            Number);

        string printed = context.ToString();

        Assert.DoesNotContain(Number, printed, StringComparison.Ordinal);
        Assert.DoesNotContain("84900000001", printed, StringComparison.Ordinal);
        Assert.Contains("[REDACTED_PHONE_E164]", printed, StringComparison.Ordinal);
        Assert.Contains("SCRIPT-ORDER-CONFIRM", printed, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "UT-PHONE-TOSTRING-03")]
    public void ATaskSentTheOldWayPrintsNullRatherThanAMarker()
    {
        // A marker for a number that is not there would send someone looking for a leak that never
        // happened; null says the token path was used.
        var request = new DialTokenResolutionRequest(
            DialTokenReference.Create("enc:mock-token", Now.AddMinutes(15)),
            AttemptId.Create("ATTEMPT-B15-TOKEN"),
            TaskId.Create("TASK-B15-TOKEN"),
            4);

        string printed = request.ToString();

        Assert.Contains("DirectPhoneE164 = null", printed, StringComparison.Ordinal);
        Assert.DoesNotContain("[REDACTED_PHONE_E164]", printed, StringComparison.Ordinal);
    }
}
