using Ivr.Contracts.Sales;
using Ivr.Domain.Confirmation;
using Ivr.Infrastructure.FeatureFlags;

namespace Ivr.Infrastructure.Configuration;

/// <summary>
/// Contains the non-secret runtime switches used by the IVR bootstrap.
/// </summary>
public sealed class IvrOptions
{
    public const string SectionName = "Ivr";

    public const string MockExecutionMode = ExecutionModes.Mock;

    public const string LabRealSimExecutionMode = ExecutionModes.LabRealSim;

    public const string ProductionRealExecutionMode = ExecutionModes.ProductionReal;

    public string ExecutionMode { get; set; } = MockExecutionMode;

    public string SalesProvider { get; set; } = "FAKE_TARGET_V1";

    /// <summary>
    /// Returns the configured provider as a closed typed value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when startup validation was bypassed and the value is unsupported.
    /// </exception>
    public SalesProviderKind GetSalesProviderKind()
    {
        return SalesProviderNames.TryParse(SalesProvider, out SalesProviderKind provider)
            ? provider
            : throw new InvalidOperationException("SALES_PROVIDER is not supported.");
    }

    public string SimProvider { get; set; } = FeatureFlagValues.MockSimProvider;

    public string ConnectionString { get; set; } = string.Empty;

    public bool RealCustomerCallAllowed { get; set; }
}
