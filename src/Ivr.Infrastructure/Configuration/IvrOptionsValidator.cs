using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Configuration;

public sealed class IvrOptionsValidator : IValidateOptions<IvrOptions>
{
    private static readonly HashSet<string> SupportedExecutionModes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            IvrOptions.MockExecutionMode,
            "LAB",
            "PRODUCTION_REAL",
        };

    public ValidateOptionsResult Validate(string? name, IvrOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        List<string> failures = [];

        if (!SupportedExecutionModes.Contains(options.ExecutionMode))
        {
            failures.Add("IVR_EXECUTION_MODE must be MOCK, LAB, or PRODUCTION_REAL.");
        }

        Require(options.SalesProvider, "SALES_PROVIDER", failures);
        Require(options.SimProvider, "SIM_PROVIDER", failures);
        Require(options.ConnectionString, "ConnectionStrings__IvrDb", failures);

        if (options.RealCustomerCallAllowed)
        {
            failures.Add("REAL_CUSTOMER_CALL_ALLOWED must remain NO before the release gate.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void Require(string value, string key, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            failures.Add($"{key} is required.");
        }
    }
}
