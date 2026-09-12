using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Quota;

/// <summary>
/// W-0282 / B2. Per-service-account request ceiling for the sandbox Module 3 calls into.
/// <para>
/// The contract has always promised this. Every one of the 38 operations in
/// <c>ivr-order-confirmation.v1.yaml</c> declares a <c>429</c> response and <c>IVR_RATE_LIMITED</c>
/// is a published error code — and until this type existed, <c>IvrErrors.RateLimited()</c> had no
/// callers anywhere in the solution. A caller who read the contract and wrote a backoff path could
/// never have exercised it, which is the same shape of defect as an authentication scheme that was
/// deleted while its clients kept sending the old headers: declared, believed, never run.
/// </para>
/// <para>
/// Disabled by default, and that is not timidity. A ceiling is a capacity judgement, and IVR has
/// not measured its own capacity yet — that is <c>E3</c>, after a real SIM. Shipping an unmeasured
/// limit switched on everywhere would reject legitimate traffic on a guess, which is worse than
/// the honest absence this replaces. The sandbox turns it on because a sandbox exists precisely so
/// that a partner's client meets a <c>429</c> somewhere cheap.
/// </para>
/// </summary>
public sealed class ServiceQuotaOptions
{
    public const string SectionName = "Ivr:ServiceQuota";

    /// <summary>Whether the ceiling is enforced at all. Off unless a deployment asks for it.</summary>
    public bool Enabled { get; set; }

    /// <summary>Length of the fixed window, in seconds.</summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>Requests one account may make inside a window before it is refused.</summary>
    public int RequestsPerWindow { get; set; } = 600;

    /// <summary>
    /// Per-account overrides, keyed by the account identity the middleware derives. Module 3's
    /// sandbox account gets a tighter ceiling than the default so that its client meets the
    /// refusal in a rehearsal rather than in production.
    /// </summary>
    public Dictionary<string, ServiceQuotaAccountOptions> Accounts { get; } =
        new(StringComparer.Ordinal);
}

/// <summary>One account's override. A null field falls back to the deployment default.</summary>
public sealed class ServiceQuotaAccountOptions
{
    public int? RequestsPerWindow { get; set; }

    public int? WindowSeconds { get; set; }
}

/// <summary>
/// Refuses a configuration that would make the ceiling meaningless, at startup rather than on the
/// first request. A window of zero seconds or a ceiling of zero requests would refuse every
/// caller, and a deployment discovers that from a failed start, not from a partner's incident.
/// </summary>
public sealed class ServiceQuotaOptionsValidator : IValidateOptions<ServiceQuotaOptions>
{
    public const int MaximumWindowSeconds = 3600;

    public const int MaximumRequestsPerWindow = 1_000_000;

    public ValidateOptionsResult Validate(string? name, ServiceQuotaOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // An unused section is not a broken section. Validating a disabled quota would turn a
        // leftover key in a values file into a failed deployment of something it does not affect.
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        List<string> failures = [];
        if (options.WindowSeconds is < 1 or > MaximumWindowSeconds)
        {
            failures.Add(
                $"{ServiceQuotaOptions.SectionName}:WindowSeconds must be between 1 and "
                + $"{MaximumWindowSeconds}.");
        }

        if (options.RequestsPerWindow is < 1 or > MaximumRequestsPerWindow)
        {
            failures.Add(
                $"{ServiceQuotaOptions.SectionName}:RequestsPerWindow must be between 1 and "
                + $"{MaximumRequestsPerWindow}.");
        }

        foreach ((string account, ServiceQuotaAccountOptions override_) in options.Accounts)
        {
            if (string.IsNullOrWhiteSpace(account))
            {
                failures.Add($"{ServiceQuotaOptions.SectionName}:Accounts has an unnamed entry.");
                continue;
            }

            if (override_.WindowSeconds is { } window and (< 1 or > MaximumWindowSeconds))
            {
                failures.Add(
                    $"{ServiceQuotaOptions.SectionName}:Accounts:{account}:WindowSeconds is "
                    + $"{window}; it must be between 1 and {MaximumWindowSeconds}.");
            }

            if (override_.RequestsPerWindow is { } ceiling
                and (< 1 or > MaximumRequestsPerWindow))
            {
                failures.Add(
                    $"{ServiceQuotaOptions.SectionName}:Accounts:{account}:RequestsPerWindow is "
                    + $"{ceiling}; it must be between 1 and {MaximumRequestsPerWindow}.");
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
