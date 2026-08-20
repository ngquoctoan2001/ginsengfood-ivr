using Ivr.Infrastructure.Callbacks;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ivr.Api.Health;

public sealed record ReadinessCheck(string Name, bool Ready, string Reason);

public sealed record ReadinessReport(bool Ready, IReadOnlyList<ReadinessCheck> Checks)
{
    /// <summary>
    /// 503 when anything is not ready. `DO-06`: a probe that answers 200 while a dependency is
    /// down does not merely fail to help — it actively routes traffic into the failure.
    /// </summary>
    public int StatusCode => Ready ? 200 : 503;
}

public interface IIvrReadinessProbe
{
    public Task<ReadinessReport> CheckAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Real readiness (W-0040 / P6-1 §6.4, DO-06).
/// <para>
/// Replaces the hardcoded `Healthy` that stood here since P0-1 and was labelled
/// `NOT_IMPLEMENTED_UNTIL_W-0040`. `P4-1` deliberately left it alone rather than half-wiring it.
/// </para>
/// <para>
/// Every reason string is a fixed phrase, never an exception message: a readiness body is served
/// to anything that asks, so it must not carry a connection string, a host name or a stack.
/// </para>
/// </summary>
public sealed class IvrReadinessProbe(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    CallbackCircuitBreaker? callbackCircuit = null,
    IOptions<CallbackDeliveryOptions>? callbackOptions = null) : IIvrReadinessProbe
{
    public async Task<ReadinessReport> CheckAsync(CancellationToken cancellationToken)
    {
        var checks = new List<ReadinessCheck>
        {
            await CheckDatabaseAsync(cancellationToken).ConfigureAwait(false),
            CheckCallbackPath(),
        };

        return new ReadinessReport(checks.TrueForAll(check => check.Ready), checks);
    }

    private async Task<ReadinessCheck> CheckDatabaseAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using IvrDbContext context = await dbContextFactory
                .CreateDbContextAsync(cancellationToken)
                .ConfigureAwait(false);
            bool reachable = await context.Database
                .CanConnectAsync(cancellationToken)
                .ConfigureAwait(false);
            if (!reachable)
            {
                return new ReadinessCheck("database", false, "unreachable");
            }

            // Reachable is not the same as usable. W-0046 recorded that nothing checked the
            // direction where NEW code meets an OLD schema: the chart runs migrations as a
            // pre-upgrade hook, but a deploy that skipped hooks, a hook that was disabled, or a
            // developer pointing at an un-migrated database all produce a pod that connects,
            // reports Healthy, takes traffic, and fails on the first query against a table that
            // is not there. Answering "may traffic come in" has to include "does the schema this
            // build was compiled against exist".
            IEnumerable<string> pending = await context.Database
                .GetPendingMigrationsAsync(cancellationToken)
                .ConfigureAwait(false);
            if (pending.Any())
            {
                // Fixed phrase, no migration names: a readiness body is served to anything that
                // asks, and the list of migrations a build expects is a description of the build.
                return new ReadinessCheck("database", false, "schema_behind");
            }

            return new ReadinessCheck("database", true, "reachable");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            // Any failure is unreadiness. Catching broadly is correct here: the probe's job is to
            // answer "may traffic come in", and an unexpected error is not a yes.
            return new ReadinessCheck("database", false, "unreachable");
        }
    }

    private ReadinessCheck CheckCallbackPath()
    {
        if (callbackOptions?.Value is not { Enabled: true } || callbackCircuit is null)
        {
            // Delivery is off in this host. That is a configuration, not a fault — the console
            // and the intake API are still able to serve.
            return new ReadinessCheck("sales_callback", true, "not_configured");
        }

        CallbackCircuitState circuit = callbackCircuit.Snapshot();
        return circuit.Readiness == "READY"
            ? new ReadinessCheck("sales_callback", true, "ready")
            : new ReadinessCheck("sales_callback", false, circuit.Readiness);
    }
}
