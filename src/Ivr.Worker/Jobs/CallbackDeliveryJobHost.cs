using Ivr.Infrastructure.Callbacks;
using Microsoft.Extensions.Options;

namespace Ivr.Worker.Jobs;

public sealed partial class CallbackDeliveryJobHost(
    IServiceScopeFactory scopeFactory,
    CallbackCircuitBreaker circuitBreaker,
    IOptions<CallbackDeliveryOptions> options,
    WorkerLiveness liveness,
    ILogger<CallbackDeliveryJobHost> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        CallbackDeliveryOptions snapshot = options.Value;
        if (!snapshot.Enabled)
        {
            LogDisabled(logger);
            // Registered even though it will not run, so the report can tell a loop that
            // was turned OFF from a loop that was never wired: the first is a decision,
            // the second is a defect, and only one of them is worth a restart.
            liveness.RegisterDisabled("callback-delivery");
            return;
        }

        using var timer = new PeriodicTimer(
            TimeSpan.FromMilliseconds(snapshot.PollIntervalMilliseconds));
        // Registered explicitly. A loop that forgot to register would be silently exempt
        // from the liveness check, and the loops worth watching are exactly the ones
        // somebody added without thinking about health.
        liveness.Register(
            "callback-delivery",
            TimeSpan.FromMilliseconds(snapshot.PollIntervalMilliseconds));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
                CallbackDispatcher dispatcher = scope.ServiceProvider
                    .GetRequiredService<CallbackDispatcher>();
                IReadOnlyList<CallbackDispatchResult> results =
                    await dispatcher.RunBatchAsync(stoppingToken).ConfigureAwait(false);
                if (results.Count > 0)
                {
                    CallbackCircuitState circuit = circuitBreaker.Snapshot();
                    LogBatch(logger, results.Count, circuit.Readiness);
                }

                liveness.Tick("callback-delivery");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                LogFailure(logger, exception);
                liveness.Fault("callback-delivery", exception);
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                break;
            }
        }
    }

    [LoggerMessage(
        EventId = 2330,
        Level = LogLevel.Information,
        Message = "Callback delivery is disabled by configuration.")]
    private static partial void LogDisabled(ILogger logger);

    [LoggerMessage(
        EventId = 2331,
        Level = LogLevel.Information,
        Message = "Callback delivery processed {Count} signal(s); readiness={Readiness}.")]
    private static partial void LogBatch(ILogger logger, int count, string readiness);

    [LoggerMessage(
        EventId = 2332,
        Level = LogLevel.Error,
        Message = "Callback delivery failed closed.")]
    private static partial void LogFailure(ILogger logger, Exception exception);
}
