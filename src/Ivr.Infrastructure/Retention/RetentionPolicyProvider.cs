using Ivr.Domain.Retention;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Retention;

/// <summary>
/// Resolves an owner-supplied period for a canonical data class.
/// </summary>
public interface IRetentionPolicyProvider
{
    /// <summary>
    /// Returns null when Legal/Privacy has not configured the class.
    /// </summary>
    public ValueTask<TimeSpan?> GetPeriodAsync(
        string dataClass,
        CancellationToken cancellationToken);
}

/// <summary>
/// Reads retention periods from the canonical <c>Ivr:Retention:PeriodDays</c> section.
/// </summary>
public sealed class ConfiguredRetentionPolicyProvider(
    IOptionsMonitor<RetentionOptions> options) : IRetentionPolicyProvider
{
    public ValueTask<TimeSpan?> GetPeriodAsync(
        string dataClass,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!RetentionDataClasses.All.Contains(dataClass)
            || !options.CurrentValue.PeriodDays.TryGetValue(dataClass, out int? days)
            || days is null or <= 0)
        {
            return ValueTask.FromResult<TimeSpan?>(null);
        }

        return ValueTask.FromResult<TimeSpan?>(TimeSpan.FromDays(days.Value));
    }
}
