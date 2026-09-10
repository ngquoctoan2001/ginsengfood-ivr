using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using IvrServer = Ivr.Contracts.Generated.IvrServer.V1;

namespace Ivr.IntegrationTests.Intake;

/// <summary>
/// The intake endpoint's strict-schema allowlists, held against the generated contract.
///
/// <para><c>TaskIntakeEndpoint</c> validates the request body by hand, against six
/// <c>HashSet&lt;string&gt;</c> of roughly fifty wire field names. Those names also exist in
/// <c>IvrServerModels.g.cs</c>, generated from
/// <c>specs/api/openapi/ivr-order-confirmation.v1.yaml</c>. Two independent definitions of one wire
/// schema, kept in step by whoever remembers.</para>
///
/// <para>The failure mode is silent in both directions. Add a field to the spec, regenerate, and
/// the endpoint still rejects it as unknown — while every test passes, because each side is
/// self-consistent. Remove one and the endpoint keeps requiring it. This is the test that was
/// missing, not a second opinion about the schema.</para>
///
/// <para><b>Why the lists are not simply derived from the contract.</b> Making the endpoint read
/// the generated type would change what intake accepts, and the one divergence below shows it would
/// reject bodies that succeed today. Which side is right is a contract decision with a producer on
/// the other end of it, so this pins the two together and names the difference instead of choosing
/// for the owner.</para>
/// </summary>
public sealed class TaskIntakeSchemaParityTests
{
    /// <summary>
    /// <c>phone_validation_status</c> is in the spec's <c>required</c> list (line 1193 of
    /// <c>ivr-order-confirmation.v1.yaml</c>) and generated as <c>required</c>, but the endpoint
    /// accepts a task without it.
    /// <para>
    /// Listed rather than fixed. Requiring it would reject intake bodies that are accepted today,
    /// which is a breaking change to a service-to-service API — the producer is Module 3, and
    /// whether the contract or the endpoint is wrong is theirs and the owner's to settle. What this
    /// test does is stop it being invisible.
    /// </para>
    /// </summary>
    private static readonly string[] RequiredInContractButOptionalAtIntake =
        ["phone_validation_status"];

    [Fact]
    [Trait("TestId", "UT-INTAKE-PARITY-01")]
    public void TheTaskAllowlistNamesExactlyTheContractsFields()
    {
        AssertSameSet(
            WireNames(typeof(IvrServer.IvrConfirmationTaskV1)),
            EndpointSet("TaskProperties"),
            "task");
    }

    [Fact]
    [Trait("TestId", "UT-INTAKE-PARITY-02")]
    public void TheTaskRequiredListMatchesTheContractExceptWhereDeclared()
    {
        HashSet<string> contract = RequiredWireNames(typeof(IvrServer.IvrConfirmationTaskV1));
        HashSet<string> endpoint = EndpointSet("RequiredTaskProperties");

        // The declared exception, asserted rather than excused: if someone settles the question by
        // making intake require it, this line fails and the exception list has to come out.
        Assert.Equal(
            RequiredInContractButOptionalAtIntake.Order(StringComparer.Ordinal),
            contract.Except(endpoint).Order(StringComparer.Ordinal));

        // Nothing the endpoint demands may be absent from the contract. A field required at intake
        // and unknown to the spec is a body no conforming producer could ever send.
        Assert.Empty(endpoint.Except(contract));
    }

    [Fact]
    [Trait("TestId", "UT-INTAKE-PARITY-03")]
    public void TheSpeechSummaryAllowlistsMatchTheContract()
    {
        AssertSameSet(
            WireNames(typeof(IvrServer.PrivacySafeOrderSummary)),
            EndpointSet("SpeechProperties"),
            "privacy_safe_order_summary");
        AssertSameSet(
            RequiredWireNames(typeof(IvrServer.PrivacySafeOrderSummary)),
            EndpointSet("RequiredSpeechProperties"),
            "privacy_safe_order_summary required");
    }

    [Fact]
    [Trait("TestId", "UT-INTAKE-PARITY-04")]
    public void TheSpeechItemAllowlistsMatchTheContract()
    {
        AssertSameSet(
            WireNames(typeof(IvrServer.OrderSpeechItem)),
            EndpointSet("SpeechItemProperties"),
            "privacy_safe_order_summary.items[]");
        AssertSameSet(
            RequiredWireNames(typeof(IvrServer.OrderSpeechItem)),
            EndpointSet("RequiredSpeechItemProperties"),
            "privacy_safe_order_summary.items[] required");
    }

    /// <summary>
    /// Every field the endpoint insists is a non-blank string must be a string on the wire.
    /// <para>
    /// An enum counts: the generator emits a C# enum for a closed string set, and those arrive as
    /// JSON strings. A field that is a number or an object in the contract but string-checked at
    /// intake would reject every conforming body, which is the kind of mistake that only shows up
    /// in production traffic.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-INTAKE-PARITY-05")]
    public void EveryStringCheckedFieldIsAStringInTheContract()
    {
        foreach ((Type contract, string setName) in new[]
        {
            (typeof(IvrServer.IvrConfirmationTaskV1), "RequiredTaskStringProperties"),
            (typeof(IvrServer.PrivacySafeOrderSummary), "RequiredSpeechStringProperties"),
        })
        {
            Dictionary<string, Type> byWireName = WireProperties(contract).ToDictionary(
                property => WireName(property)!,
                property => Unwrap(property.PropertyType),
                StringComparer.Ordinal);

            foreach (string field in EndpointSet(setName))
            {
                Assert.True(
                    byWireName.TryGetValue(field, out Type? type),
                    $"{setName} names '{field}', which {contract.Name} does not declare.");
                Assert.True(
                    type == typeof(string) || type!.IsEnum,
                    $"{setName} string-checks '{field}', which the contract types as {type!.Name}.");
            }
        }
    }

    private static void AssertSameSet(
        HashSet<string> contract,
        HashSet<string> endpoint,
        string what)
    {
        Assert.Equal(
            contract.Order(StringComparer.Ordinal),
            endpoint.Order(StringComparer.Ordinal));
        Assert.NotEmpty(contract);
        Assert.True(contract.Count > 0, $"{what} resolved to no fields; the probe is reading nothing.");
    }

    /// <summary>
    /// The contract's wire properties, one entry per field name.
    /// <para>
    /// Resolved most-derived-first because the generator splits a schema across a base class and
    /// its subclass and declares some fields on both — <c>IvrConfirmationTaskV1</c> and its
    /// <c>Anonymous</c> base both carry <c>program_code</c>. Taking whichever
    /// <see cref="Type.GetProperties()"/> happened to return first would make the required set
    /// depend on reflection ordering, which is not specified.
    /// </para>
    /// </summary>
    private static IReadOnlyList<PropertyInfo> WireProperties(Type contract)
    {
        Dictionary<string, PropertyInfo> resolved = new(StringComparer.Ordinal);
        for (Type? level = contract; level is not null; level = level.BaseType)
        {
            foreach (PropertyInfo property in level.GetProperties(
                         BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (WireName(property) is { } wire)
                {
                    resolved.TryAdd(wire, property);
                }
            }
        }

        return [.. resolved.Values];
    }

    private static HashSet<string> WireNames(Type contract) =>
        WireProperties(contract).Select(WireName).OfType<string>().ToHashSet(StringComparer.Ordinal);

    private static HashSet<string> RequiredWireNames(Type contract) =>
        WireProperties(contract)
            .Where(property => property.GetCustomAttribute<RequiredMemberAttribute>() is not null)
            .Select(WireName)
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);

    private static string? WireName(PropertyInfo property) =>
        property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;

    private static Type Unwrap(Type type) => Nullable.GetUnderlyingType(type) ?? type;

    /// <summary>
    /// Reads one of the endpoint's private allowlists. Reflection rather than a second copy here:
    /// a copy in the test would be a third definition of the same schema, and this test exists
    /// because two was already one too many.
    /// </summary>
    private static HashSet<string> EndpointSet(string name)
    {
        FieldInfo field = typeof(Ivr.Api.Intake.TaskIntakeEndpoint)
            .GetField(name, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                $"TaskIntakeEndpoint no longer has a '{name}' allowlist; this guard reads nothing.");
        return (HashSet<string>)field.GetValue(null)!;
    }
}
