using Ivr.Domain.Retention;
using Ivr.Infrastructure.Retention;
using Microsoft.Extensions.Options;

namespace Ivr.Worker.Jobs;

/// <summary>
/// Runs one opt-in retention pass so a future Kubernetes CronJob can invoke the worker unchanged.
/// </summary>
public sealed partial class RetentionJobHost(
    IRetentionJob retentionJob,
    IOptions<RetentionOptions> options,
    IHostApplicationLifetime applicationLifetime,
    ILogger<RetentionJobHost> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        RetentionOptions configured = options.Value;
        if (!configured.Enabled)
        {
            LogDisabled(logger);
            return;
        }

        try
        {
            RetentionRunReport report = await retentionJob.RunAsync(
                new RetentionRunOptions(
                    configured.DryRun,
                    configured.DataClasses,
                    BatchSize: configured.BatchSize),
                stoppingToken);
            LogCompleted(
                logger,
                report.RunId,
                report.DryRun,
                report.DeletedCount,
                report.AnonymizedCount,
                report.LegalHoldCount);

            foreach (RetentionClassReport item in report.Classes.Where(
                         item => item.Status == RetentionClassRunStatus.NotConfigured
                             && item.NotConfiguredAgeDays >= configured.NotConfiguredAlertAfterDays))
            {
                LogNotConfigured(
                    logger,
                    item.DataClass,
                    item.NotConfiguredAgeDays.GetValueOrDefault(),
                    configured.NotConfiguredAlertAfterDays);
            }
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }

    [LoggerMessage(
        EventId = 1100,
        Level = LogLevel.Information,
        Message = "Retention host disabled; no lifecycle pass was run")]
    private static partial void LogDisabled(ILogger logger);

    [LoggerMessage(
        EventId = 1101,
        Level = LogLevel.Information,
        Message = "Retention run {RunId} completed; dryRun={DryRun}; deleted={Deleted}; anonymized={Anonymized}; legalHold={LegalHold}")]
    private static partial void LogCompleted(
        ILogger logger,
        Guid runId,
        bool dryRun,
        long deleted,
        long anonymized,
        long legalHold);

    [LoggerMessage(
        EventId = 1102,
        Level = LogLevel.Warning,
        Message = "Retention class {DataClass} has been NOT_CONFIGURED for {AgeDays} days, exceeding the {AlertAfterDays}-day threshold")]
    private static partial void LogNotConfigured(
        ILogger logger,
        string dataClass,
        double ageDays,
        int alertAfterDays);
}
