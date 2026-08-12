using Ivr.Contracts.Sales;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Configuration;

public sealed class IvrOptionsValidator : IValidateOptions<IvrOptions>
{
    private static readonly HashSet<string> SupportedExecutionModes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            IvrOptions.MockExecutionMode,
            IvrOptions.LabRealSimExecutionMode,
            IvrOptions.ProductionRealExecutionMode,
        };

    public ValidateOptionsResult Validate(string? name, IvrOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        List<string> failures = [];

        if (!SupportedExecutionModes.Contains(options.ExecutionMode))
        {
            failures.Add(
                "IVR_EXECUTION_MODE must be MOCK, LAB_REAL_SIM, or PRODUCTION_REAL.");
        }

        if (!SalesProviderNames.TryParse(options.SalesProvider, out SalesProviderKind provider))
        {
            failures.Add(
                "SALES_PROVIDER must be FAKE_TARGET_V1, CURRENT_GOLDEN_HOUR_COMPAT, or TARGET_V1.");
        }

        Require(options.SimProvider, "SIM_PROVIDER", failures);
        Require(options.ConnectionString, "ConnectionStrings__IvrDb", failures);

        if (!string.IsNullOrWhiteSpace(options.SimProvider)
            && options.SimProvider is not "MOCK" and not "VENDOR")
        {
            failures.Add("SIM_PROVIDER must be MOCK or VENDOR.");
        }

        if (SupportedExecutionModes.Contains(options.ExecutionMode)
            && SalesProviderNames.TryParse(options.SalesProvider, out provider))
        {
            ValidateModeProviderCombination(options, provider, failures);
        }

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

    private static void ValidateModeProviderCombination(
        IvrOptions options,
        SalesProviderKind provider,
        List<string> failures)
    {
        bool valid = options.ExecutionMode switch
        {
            IvrOptions.MockExecutionMode =>
                provider == SalesProviderKind.FakeTargetV1
                && options.SimProvider == "MOCK",
            IvrOptions.LabRealSimExecutionMode =>
                provider == SalesProviderKind.FakeTargetV1
                && options.SimProvider == "VENDOR",
            IvrOptions.ProductionRealExecutionMode =>
                provider == SalesProviderKind.TargetV1
                && options.SimProvider == "VENDOR",
            _ => false,
        };

        if (!valid)
        {
            failures.Add(
                "IVR_EXECUTION_MODE, SALES_PROVIDER, and SIM_PROVIDER are incompatible. "
                + "Current Golden Hour compatibility has no approved runtime mode and remains fail-closed.");
        }
    }
}
