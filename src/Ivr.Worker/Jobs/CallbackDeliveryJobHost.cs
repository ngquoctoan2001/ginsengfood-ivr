using Ivr.Infrastructure.Callbacks;
using Microsoft.Extensions.Options;

namespace Ivr.Worker.Jobs;

internal sealed partial class CallbackDeliveryJobHost(
    IServiceScopeFactory scopeFactory,
    CallbackCircuitBreaker circuitBreaker,
    IOptions<CallbackDeliveryOptions> options,
    WorkerLiveness liveness,
    TimeProvider timeProvider,
    ILogger<CallbackDeliveryJobHost> logger) : PollingJobHost(liveness, timeProvider)
{
    protected override string LoopName => "callback-delivery";

    protected override bool IsEnabled => options.Value.Enabled;

    protected override TimeSpan Period =>
        TimeSpan.FromMilliseconds(options.Value.PollIntervalMilliseconds);

    protected override async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        // A scope per pass, not per host: CallbackDispatcher pulls scoped dependencies, and one
        // scope held for the life of the worker would hand every pass the same DbContext.
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        CallbackDispatcher dispatcher = scope.ServiceProvider
            .GetRequiredService<CallbackDispatcher>();
        IReadOnlyList<CallbackDispatchResult> results =
            await dispatcher.RunBatchAsync(cancellationToken);
        if (results.Count > 0)
        {
            CallbackCircuitState circuit = circuitBreaker.Snapshot();
            LogBatch(logger, results.Count, circuit.Readiness);
        }
    }

    protected override void OnDisabled() => LogDisabled(logger);

    protected override void OnFailure(Exception exception, int consecutiveFailures) =>
        LogFailure(logger, exception, consecutiveFailures);

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
        Message = "Callback delivery failed closed; consecutive failures={ConsecutiveFailures}.")]
    private static partial void LogFailure(
        ILogger logger,
        Exception exception,
        int consecutiveFailures);
}
