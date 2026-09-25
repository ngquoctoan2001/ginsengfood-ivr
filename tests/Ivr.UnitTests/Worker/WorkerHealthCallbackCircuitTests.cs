using System.Text;
using Ivr.Infrastructure.Callbacks;
using Ivr.Worker;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Worker;

/// <summary>
/// W-0360 / K-36. The callback circuit breaker lives in the worker, and the API's
/// <c>/health/ready</c> can only ever say <c>not_configured</c> for it because the API delivers
/// nothing. Until the worker's <c>/healthz</c> carried it, the one place an operator could see that
/// Sales had stopped answering was a log line per batch.
/// </summary>
public sealed class WorkerHealthCallbackCircuitTests
{
    private static readonly DateTimeOffset T0 = new(2026, 9, 25, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "UT-WORKER-HEALTH-CIRCUIT-01")]
    public void AnOpenCircuitIsInTheBodyAndNotInTheStatusCode()
    {
        var delivery = new CallbackDeliveryOptions
        {
            Enabled = true,
            CircuitFailureThreshold = 2,
            CircuitOpenSeconds = 30,
        };
        var circuit = new CallbackCircuitBreaker(new FixedTimeProvider(T0), Options.Create(delivery));
        circuit.RecordTransientFailure();
        circuit.RecordTransientFailure();
        var live = new WorkerLivenessReport(WorkerLivenessStatus.Live, []);

        (int statusCode, byte[] body) = WorkerHealthEndpoint.BuildResponse(
            live,
            null,
            WorkerHealthEndpoint.CircuitToReport(delivery, circuit));
        string json = Encoding.UTF8.GetString(body);

        // Sales not answering is not a reason to restart the worker: a restart closes the circuit
        // and sends the next batch straight back into the outage.
        Assert.Equal(200, statusCode);
        Assert.Contains(
            "\"callback_circuit\":{\"readiness\":\"NOT_READY_CIRCUIT_OPEN\",\"open\":true,"
            + "\"consecutive_transient_failures\":2,\"open_until\":\"2026-09-25T08:00:30+00:00\"}",
            json,
            StringComparison.Ordinal);

        // Reported beside the liveness verdict, never instead of it: a stalled loop still fails.
        var stalled = new WorkerLivenessReport(
            WorkerLivenessStatus.Stalled,
            [new WorkerLoopHealth("callback-delivery", true, T0, true, 0, null)]);
        (int stalledCode, byte[] stalledBody) = WorkerHealthEndpoint.BuildResponse(
            stalled,
            null,
            circuit.Snapshot());

        Assert.Equal(503, stalledCode);
        Assert.Contains(
            "NOT_READY_CIRCUIT_OPEN",
            Encoding.UTF8.GetString(stalledBody),
            StringComparison.Ordinal);

        // And it closes on the next success.
        circuit.RecordSuccess();
        (_, byte[] closedBody) = WorkerHealthEndpoint.BuildResponse(
            live,
            null,
            WorkerHealthEndpoint.CircuitToReport(delivery, circuit));

        Assert.Contains(
            "\"callback_circuit\":{\"readiness\":\"READY\",\"open\":false,"
            + "\"consecutive_transient_failures\":0,\"open_until\":null}",
            Encoding.UTF8.GetString(closedBody),
            StringComparison.Ordinal);
    }

    /// <summary>
    /// A worker that does not deliver callbacks has a breaker nothing ever trips. Reporting it
    /// would say READY about a path that is not running, so the section is null instead.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-WORKER-HEALTH-CIRCUIT-02")]
    public void AWorkerThatDeliversNoCallbacksReportsNoCircuit()
    {
        var delivery = new CallbackDeliveryOptions { Enabled = false };
        var circuit = new CallbackCircuitBreaker(new FixedTimeProvider(T0), Options.Create(delivery));
        var live = new WorkerLivenessReport(WorkerLivenessStatus.Live, []);

        Assert.Null(WorkerHealthEndpoint.CircuitToReport(delivery, circuit));

        (int statusCode, byte[] body) = WorkerHealthEndpoint.BuildResponse(
            live,
            null,
            WorkerHealthEndpoint.CircuitToReport(delivery, circuit));
        string json = Encoding.UTF8.GetString(body);

        Assert.Equal(200, statusCode);
        Assert.Contains("\"callback_circuit\":null", json, StringComparison.Ordinal);
        Assert.DoesNotContain("READY", json, StringComparison.Ordinal);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
