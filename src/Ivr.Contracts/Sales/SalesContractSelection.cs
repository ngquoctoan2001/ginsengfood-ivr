namespace Ivr.Contracts.Sales;

/// <summary>
/// Names accepted by the canonical <c>SALES_PROVIDER</c> configuration key.
/// </summary>
public static class SalesProviderNames
{
    public const string FakeTargetV1 = "FAKE_TARGET_V1";

    public const string CurrentGoldenHourCompat = "CURRENT_GOLDEN_HOUR_COMPAT";

    public const string TargetV1 = "TARGET_V1";

    /// <summary>
    /// Parses a configured provider name without accepting aliases.
    /// </summary>
    public static bool TryParse(string? value, out SalesProviderKind provider)
    {
        provider = value switch
        {
            FakeTargetV1 => SalesProviderKind.FakeTargetV1,
            CurrentGoldenHourCompat => SalesProviderKind.CurrentGoldenHourCompat,
            TargetV1 => SalesProviderKind.TargetV1,
            _ => default,
        };

        return value is FakeTargetV1 or CurrentGoldenHourCompat or TargetV1;
    }
}

/// <summary>
/// Typed Sales-provider selection. Values deliberately do not share DTO types.
/// </summary>
public enum SalesProviderKind
{
    FakeTargetV1,
    CurrentGoldenHourCompat,
    TargetV1,
}

/// <summary>
/// The callback wire contract selected for a task.
/// </summary>
public enum SalesCallbackContractKind
{
    TargetV1,
    CurrentGoldenHourCompat,
}

/// <summary>
/// Selects a callback contract while preventing 24/7 results from reaching the
/// current Golden Hour endpoint.
/// </summary>
public static class SalesCallbackContractSelector
{
    public const string GoldenHourProgram = "GOLDEN_HOUR";

    public const string TwentyFourSevenProgram = "TWENTY_FOUR_SEVEN";

    /// <summary>
    /// Returns the isolated wire contract for a typed provider and program.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the provider/program pair is unsupported.
    /// </exception>
    public static SalesCallbackContractKind Select(
        SalesProviderKind provider,
        string programCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(programCode);

        if (programCode is not GoldenHourProgram and not TwentyFourSevenProgram)
        {
            throw new InvalidOperationException("The program is not supported by Target V1.");
        }

        return provider switch
        {
            SalesProviderKind.FakeTargetV1 => SalesCallbackContractKind.TargetV1,
            SalesProviderKind.TargetV1 => SalesCallbackContractKind.TargetV1,
            SalesProviderKind.CurrentGoldenHourCompat
                when programCode == GoldenHourProgram =>
                    SalesCallbackContractKind.CurrentGoldenHourCompat,
            SalesProviderKind.CurrentGoldenHourCompat => throw new InvalidOperationException(
                "The current Golden Hour compatibility endpoint cannot receive 24/7 results."),
            _ => throw new InvalidOperationException("The Sales provider is not supported."),
        };
    }
}
