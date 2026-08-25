using Ivr.Contracts.Sales;

namespace Ivr.Infrastructure.Configuration;

/// <summary>
/// Contains the non-secret runtime switches used by the IVR bootstrap.
/// </summary>
public sealed class IvrOptions
{
    public const string SectionName = "Ivr";

    public const string MockExecutionMode = "MOCK";

    public const string LabRealSimExecutionMode = "LAB_REAL_SIM";

    public const string ProductionRealExecutionMode = "PRODUCTION_REAL";

    public string ExecutionMode { get; set; } = "MOCK";

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

    public string SimProvider { get; set; } = "MOCK";

    public string ConnectionString { get; set; } = string.Empty;

    public bool RealCustomerCallAllowed { get; set; }

    /// <summary>
    /// Owner policy <c>OD-15</c>: do not place a confirmation call to a returning customer.
    /// <para>
    /// On by default — the owner decided the policy, not the deployment. It stays configurable
    /// because turning it off is the one-switch rollback if Sales risk evidence turns out to be
    /// wrong in production, and reaching for a redeploy in that moment would mean choosing
    /// between calling everyone and calling no one.
    /// </para>
    /// <para>
    /// Enabling it does not by itself skip anyone: <c>TrustResolverEvidence.CanSkip</c> still
    /// requires Sales to send versioned risk evidence for the order. Until Sales does, every
    /// eligible task is called and carries a <c>TRUST_RISK_EVIDENCE_UNAVAILABLE</c> advisory.
    /// </para>
    /// </summary>
    public bool ReturningCustomerSkipEnabled { get; set; } = true;
}
