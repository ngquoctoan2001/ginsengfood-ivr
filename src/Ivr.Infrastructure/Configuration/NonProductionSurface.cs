using System.Collections.Frozen;

namespace Ivr.Infrastructure.Configuration;

/// <summary>
/// Decides whether the non-production developer surface (UI-07 seed loader, scenario runner and
/// integration-status profiles, W-0112) may exist in this deployment.
/// <para>
/// A permission cannot express this rule. Permissions answer "may this actor do it"; the rule
/// here is "this must not be reachable at all, by anyone, in production" — and the actors who
/// would hold the permission are exactly the ones present during a production incident.
/// </para>
/// <para>
/// An allowlist of known non-production environments, not a denylist of production ones. A
/// deployment that invents a new environment name is refused rather than admitted, so the failure
/// mode of forgetting to update this list is a dev tool that is missing, not a seed loader
/// pointed at live customers.
/// </para>
/// </summary>
public static class NonProductionSurface
{
    private static readonly FrozenSet<string> NonProductionEnvironments = new[]
        {
            "Development",
            "Testing",
            "Test",
            "Staging",
            "Lab",
        }
        .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Whether the developer surface may be served. Every disagreement resolves to
    /// <see langword="false"/>.
    /// </summary>
    /// <param name="environmentName">The host environment name, e.g. <c>Staging</c>.</param>
    /// <param name="executionMode">The configured <c>IVR_EXECUTION_MODE</c>.</param>
    /// <param name="realCustomerCallAllowed">
    /// The runtime flag. It is checked independently of the other two because it is the one that
    /// states plainly that real people are being called; a deployment that sets it while still
    /// labelled <c>Staging</c> is calling real customers whatever its label says.
    /// </param>
    public static bool IsAvailable(
        string? environmentName,
        string? executionMode,
        bool realCustomerCallAllowed)
    {
        if (realCustomerCallAllowed)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(environmentName)
            || !NonProductionEnvironments.Contains(environmentName.Trim()))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(executionMode))
        {
            return false;
        }

        return executionMode.Trim() switch
        {
            IvrOptions.MockExecutionMode => true,
            IvrOptions.LabRealSimExecutionMode => true,
            _ => false,
        };
    }
}
