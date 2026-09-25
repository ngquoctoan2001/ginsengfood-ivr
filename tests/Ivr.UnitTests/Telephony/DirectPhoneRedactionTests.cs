using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Domain.Privacy;
using Ivr.Domain.Speech;
using Ivr.Infrastructure.Telephony;
using Ivr.Worker;

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

    // The national significant number: found in every spelling of the number, with or without the
    // country code, the plus or a leading zero.
    private const string Nsn = "900000001";
    private static readonly DateTimeOffset Now = new(2026, 9, 25, 3, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "UT-PHONE-TOSTRING-01")]
    public void ResolutionRequestPrintsAMarkerInsteadOfTheNumber()
    {
        string printed = Request(Number).ToString();

        Assert.DoesNotContain(Number, printed, StringComparison.Ordinal);
        Assert.DoesNotContain(Nsn, printed, StringComparison.Ordinal);
        Assert.True(PiiGuard.IsSafeText(printed), printed);
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
        string printed = Context(Number).ToString();

        Assert.DoesNotContain(Number, printed, StringComparison.Ordinal);
        Assert.DoesNotContain(Nsn, printed, StringComparison.Ordinal);
        Assert.True(PiiGuard.IsSafeText(printed), printed);
        Assert.Contains("[REDACTED_PHONE_E164]", printed, StringComparison.Ordinal);
        Assert.Contains("SCRIPT-ORDER-CONFIRM", printed, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "UT-PHONE-TOSTRING-03")]
    public void ATaskSentTheOldWayPrintsNullRatherThanAMarker()
    {
        // A marker for a number that is not there would send someone looking for a leak that never
        // happened; null says the token path was used. Both records, since W-0361.
        var request = new DialTokenResolutionRequest(
            DialTokenReference.Create("enc:mock-token", Now.AddMinutes(15)),
            AttemptId.Create("ATTEMPT-B15-TOKEN"),
            TaskId.Create("TASK-B15-TOKEN"),
            4);

        foreach (string printed in new[] { request.ToString(), Context(null).ToString() })
        {
            Assert.Contains("DirectPhoneE164 = null", printed, StringComparison.Ordinal);
            Assert.DoesNotContain("[REDACTED_PHONE_E164]", printed, StringComparison.Ordinal);
            Assert.True(PiiGuard.IsSafeText(printed), printed);
        }
    }

    /// <summary>
    /// W-0361 / K-38. A serializer reads properties, not <c>ToString</c>, so the redaction above
    /// never reached JSON: a logger that serializes its arguments, or a diagnostic dump, would have
    /// written the number.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-PHONE-TOSTRING-04")]
    public void NeitherRecordSerializesTheNumber()
    {
        foreach (JsonSerializerOptions options in new[] { JsonSerializerOptions.Default, JsonSerializerOptions.Web })
        {
            string request = JsonSerializer.Serialize(Request(Number), options);
            string context = JsonSerializer.Serialize(Context(Number), options);

            Assert.DoesNotContain(Nsn, request, StringComparison.Ordinal);
            Assert.DoesNotContain(Nsn, context, StringComparison.Ordinal);
            Assert.True(PiiGuard.IsSafeText(request), request);
            Assert.True(PiiGuard.IsSafeText(context), context);

            // Left out rather than blanked, so the JSON does not claim a number was there.
            Assert.DoesNotContain("DirectPhoneE164", request, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("DirectPhoneE164", context, StringComparison.OrdinalIgnoreCase);

            // The rest of the record still serializes.
            Assert.Contains("MaxResolves", request, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("SCRIPT-ORDER-CONFIRM", context, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// W-0361 / K-38. B15 fixed two records by name, which protects nothing from a third. This finds
    /// every record in the domain, infrastructure and worker assemblies with a string property that
    /// could hold the number - named for a phone, an E.164 value or an MSISDN, other than the masked,
    /// reference and status forms that exist precisely so the number does not have to travel - plants
    /// the number in each such property, and requires that neither <c>ToString</c> nor JSON gives it
    /// back.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-PHONE-TOSTRING-05")]
    public void EveryRecordThatCanHoldTheNumberKeepsItOutOfItsOutput()
    {
        Type[] carriers =
        [
            .. new[]
                {
                    typeof(DialTokenResolutionRequest).Assembly,
                    typeof(TelephonyDispatchContext).Assembly,
                    typeof(IvrHeartbeat).Assembly,
                }
                .Distinct()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(IsConcreteRecord)
                .Where(type => NumberProperties(type).Any())
                .OrderBy(type => type.FullName, StringComparer.Ordinal),
        ];

        // Not vacuous: the rule that would find a third record finds the two B15 fixed.
        Assert.Contains(typeof(DialTokenResolutionRequest), carriers);
        Assert.Contains(typeof(TelephonyDispatchContext), carriers);

        foreach (Type record in carriers)
        {
            object instance = RuntimeHelpers.GetUninitializedObject(record);
            foreach (PropertyInfo property in NumberProperties(record))
            {
                Plant(instance, property);
            }

            string printed = instance.ToString()!;
            string json = JsonSerializer.Serialize(instance, record);
            Assert.False(
                printed.Contains(Nsn, StringComparison.Ordinal) || !PiiGuard.IsSafeText(printed),
                $"{record.FullName}.ToString() prints the number: give it a PrintMembers that "
                    + "prints a marker, as DialTokenResolutionRequest does.");
            Assert.False(
                json.Contains(Nsn, StringComparison.Ordinal),
                $"{record.FullName} writes the number into JSON: mark the property "
                    + "[property: JsonIgnore], as DialTokenResolutionRequest does.");
        }
    }

    private static DialTokenResolutionRequest Request(string? number) =>
        new(
            DialTokenReference.Create("enc:direct-TASK-B15", Now.AddMinutes(15)),
            AttemptId.Create("ATTEMPT-B15"),
            TaskId.Create("TASK-B15"),
            4,
            number);

    private static TelephonyDispatchContext Context(string? number) =>
        new(
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
            number);

    /// <summary>
    /// A record class has the compiler's <c>&lt;Clone&gt;$</c>; a record struct has no clone but
    /// still gets a generated <c>PrintMembers</c>. Abstract and open generic records cannot be
    /// instantiated, and every concrete record that inherits their properties is checked anyway.
    /// </summary>
    private static bool IsConcreteRecord(Type type) =>
        !type.IsAbstract
        && !type.ContainsGenericParameters
        && (type.GetMethod("<Clone>$", BindingFlags.Instance | BindingFlags.Public) is not null
            || (type.IsValueType
                && type.GetMethod("PrintMembers", BindingFlags.Instance | BindingFlags.NonPublic) is not null));

    private static IEnumerable<PropertyInfo> NumberProperties(Type type) =>
        type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(property => property.PropertyType == typeof(string)
                && property.GetIndexParameters().Length == 0
                && CouldHoldTheNumber(property.Name));

    private static bool CouldHoldTheNumber(string name) =>
        (name.Contains("Phone", StringComparison.OrdinalIgnoreCase)
            || name.Contains("E164", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Msisdn", StringComparison.OrdinalIgnoreCase))
        && !name.EndsWith("Masked", StringComparison.Ordinal)
        && !name.EndsWith("Ref", StringComparison.Ordinal)
        && !name.EndsWith("Status", StringComparison.Ordinal);

    private static void Plant(object instance, PropertyInfo property)
    {
        FieldInfo? backing = property.DeclaringType!.GetField(
            $"<{property.Name}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (backing is not null)
        {
            backing.SetValue(instance, Number);
            return;
        }

        Assert.True(
            property.CanWrite,
            $"{property.DeclaringType.FullName}.{property.Name} has no backing field or setter to "
                + "plant the number in; teach this test how to fill it rather than skipping it.");
        property.SetValue(instance, Number);
    }
}
