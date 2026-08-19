using Ivr.Domain.Retention;
using Ivr.Infrastructure.Retention;
using Microsoft.Extensions.Options;

namespace Ivr.Worker.Jobs;

/// <summary>
/// W-0047 / P7-5. Runs exactly one retention pass and then stops the host, so a Kubernetes CronJob
/// pod terminates and the Job is recorded as completed.
/// <para>
/// Separate from <see cref="RetentionJobHost"/> rather than a flag inside it. The long-running host
/// must never stop the process — it shares the worker with the scheduler and the callback pump, and
/// a retention pass finishing is not a reason to take those down. Two hosts make that impossible to
/// get wrong by editing one condition.
/// </para>
/// </summary>
public sealed partial class RetentionRunOnceHost(
    IRetentionJob retentionJob,
    IOptions<RetentionOptions> options,
    IHostApplicationLifetime lifetime,
    ILogger<RetentionRunOnceHost> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        RetentionOptions configured = options.Value;
        try
        {
            if (!configured.Enabled)
            {
                // Disabled is a successful no-op, not a failure. A CronJob that fails because
                // retention is switched off would page somebody about a deliberate configuration.
                LogDisabled(logger);
                return;
            }

            RetentionRunReport report = await retentionJob.RunAsync(
                new RetentionRunOptions(
                    configured.DryRun,
                    configured.DataClasses,
                    BatchSize: configured.BatchSize),
                cancellationToken);

            LogCompleted(
                logger,
                report.RunId,
                report.DryRun,
                report.DeletedCount,
                report.AnonymizedCount,
                report.LegalHoldCount);
        }
        finally
        {
            // Stopped in a finally so a throwing pass still terminates the pod. Without this the
            // container would hang after a failure, and a hung pod reads as "still working"
            // rather than as the failure it is.
            lifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(
        EventId = 6100,
        Level = LogLevel.Information,
        Message = "Retention run-once skipped: retention is disabled.")]
    private static partial void LogDisabled(ILogger logger);

    [LoggerMessage(
        EventId = 6101,
        Level = LogLevel.Information,
        Message = "Retention run-once completed. run={RunId} dryRun={DryRun} deleted={Deleted} "
            + "anonymized={Anonymized} legalHold={LegalHold}")]
    private static partial void LogCompleted(
        ILogger logger,
        Guid runId,
        bool dryRun,
        long deleted,
        long anonymized,
        long legalHold);
}
