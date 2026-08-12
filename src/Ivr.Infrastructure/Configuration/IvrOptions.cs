namespace Ivr.Infrastructure.Configuration;

/// <summary>
/// Contains the non-secret runtime switches used by the IVR bootstrap.
/// </summary>
public sealed class IvrOptions
{
    public const string SectionName = "Ivr";

    public const string MockExecutionMode = "MOCK";

    public string ExecutionMode { get; set; } = "MOCK";

    public string SalesProvider { get; set; } = "FAKE_TARGET_V1";

    public string SimProvider { get; set; } = "MOCK";

    public string ConnectionString { get; set; } = string.Empty;

    public bool RealCustomerCallAllowed { get; set; }
}
