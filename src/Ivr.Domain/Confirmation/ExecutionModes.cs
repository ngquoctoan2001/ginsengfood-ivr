namespace Ivr.Domain.Confirmation;

/// <summary>
/// The wire and storage spelling of <see cref="ExecutionMode"/>, and the one conversion to it.
/// <para>
/// The mapping existed three times — a <c>FrozenDictionary</c> in the script lifecycle service and
/// two hand-written <c>switch</c> expressions in the attempt-policy registries — each repeating the
/// same three literals. Three copies of a total function over a three-member enum is three places
/// to forget when a fourth member arrives, and the compiler cannot help: every one of them has a
/// <c>_ =></c> arm that turns a missing case into a runtime throw.
/// </para>
/// <para>
/// It lives beside the enum rather than beside <c>IvrOptions</c> because the enum is in Domain and
/// Domain depends on nothing. The options class takes its constants from here, not the other way
/// round.
/// </para>
/// </summary>
public static class ExecutionModes
{
    public const string Mock = "MOCK";

    public const string LabRealSim = "LAB_REAL_SIM";

    public const string ProductionReal = "PRODUCTION_REAL";

    /// <summary>Every spelling this system accepts, in enum order.</summary>
    public static IReadOnlyList<string> All { get; } = [Mock, LabRealSim, ProductionReal];

    public static string ToWireValue(ExecutionMode mode) => mode switch
    {
        ExecutionMode.Mock => Mock,
        ExecutionMode.LabRealSim => LabRealSim,
        ExecutionMode.ProductionReal => ProductionReal,
        _ => throw new InvalidOperationException("Unknown execution mode."),
    };
}
