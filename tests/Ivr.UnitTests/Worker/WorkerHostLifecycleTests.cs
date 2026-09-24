using Ivr.Domain.Retention;
using Ivr.Infrastructure.Callbacks;
using Ivr.Infrastructure.Correlation;
using Ivr.Infrastructure.Persistence.Entities;
using Ivr.Infrastructure.Persistence.Outbox;
using Ivr.Infrastructure.Retention;
using Ivr.Worker;
using Ivr.Worker.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests.Worker;

/// <summary>
/// W-0353 / W-0094. Two lifecycle fixes W-0094 made in 2026-08 without a test: the retention pass
/// stopped taking the whole worker down with it, and the callback pump stopped holding one
/// dispatcher, with its typed HTTP clients, for the life of the process.
/// </summary>
public sealed class WorkerHostLifecycleTests
{
    /// <summary>
    /// The long-running host shares the process with the scheduler, the normaliser and the callback
    /// pump, so a finished retention pass is no reason to stop it. The run-once host is the CronJob
    /// entry point, and a pod that did not exit after its pass would be recorded as failed. Both are
    /// resolved through the container with a lifetime that counts stop requests, so a host that
    /// started asking for the lifetime would be handed this one and caught.
    /// </summary>
    [Theory]
    [Trait("TestId", "UT-WORKER-RETENTION-05")]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public async Task OnlyTheRunOnceRetentionHostStopsTheWorker(bool runOnce, int expectedStopRequests)
    {
        var job = new CountingRetentionJob();
        var lifetime = new CountingLifetime();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IRetentionJob>(job);
        services.AddSingleton<IHostApplicationLifetime>(lifetime);
        services.AddSingleton(Options.Create(new RetentionOptions { Enabled = true }));

        // One or the other, never both, the way src/Ivr.Worker/Program.cs registers them.
        if (runOnce)
        {
            services.AddHostedService<RetentionRunOnceHost>();
        }
        else
        {
            services.AddHostedService<RetentionJobHost>();
        }

        await using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        IHostedService host = Assert.Single(provider.GetServices<IHostedService>());

        await host.StartAsync(CancellationToken.None);

        // The long-running host is a BackgroundService: StartAsync returns once its pass is under
        // way, not when it ends. Joined here so the count below is read after the pass, not during.
        if (host is BackgroundService background && background.ExecuteTask is { } pass)
        {
            await pass.WaitAsync(TimeSpan.FromSeconds(10));
        }

        await host.StopAsync(CancellationToken.None);

        Assert.Equal(1, job.Runs);
        Assert.Equal(expectedStopRequests, lifetime.StopRequests);
    }

    /// <summary>
    /// The dispatcher and the current Golden Hour transport are scoped, and the pump opens a scope
    /// for each pass and closes it afterwards. Built under the worker's own C5 guard
    /// (<c>ValidateScopes</c> and <c>ValidateOnBuild</c>), which is what refuses a singleton that
    /// holds a scoped service: a host that took the dispatcher in its constructor fails there,
    /// instead of quietly keeping one transport for as long as the worker runs.
    /// </summary>
    [Fact]
    [Trait("TestId", "UT-WORKER-CALLBACK-SCOPE-06")]
    public async Task EveryCallbackPassGetsItsOwnScopeAndClosesIt()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Ivr:CallbackDelivery:Enabled"] = "true",
                ["Ivr:CallbackDelivery:PollIntervalMilliseconds"] = "100",
            })
            .Build();

        // The production registrations, read before the recording transport below replaces one.
        var registrations = new ServiceCollection();
        registrations.AddIvrCallbackDelivery(configuration);
        Assert.Equal(ServiceLifetime.Scoped, LifetimeOf<CallbackDispatcher>(registrations));
        Assert.Equal(
            ServiceLifetime.Scoped,
            LifetimeOf<ICurrentGoldenHourCallbackTransport>(registrations));

        var outbox = new EmptyOutbox();
        var ledger = new TransportLedger();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ICorrelationContext, CorrelationContext>();
        services.AddSingleton<ICallbackOutboxRepository>(outbox);
        services.AddIvrCallbackDelivery(configuration);

        // Registered after the production transport, so the dispatcher is handed this one. Scoped,
        // as the production one is; every instance is one scope the pump opened.
        services.AddSingleton(ledger);
        services.AddScoped<ICurrentGoldenHourCallbackTransport, RecordingTransport>();
        services.AddSingleton<WorkerLiveness>();
        services.AddHostedService<CallbackDeliveryJobHost>();

        await using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<CallbackDispatcher>());
        IHostedService host = Assert.Single(provider.GetServices<IHostedService>());

        await host.StartAsync(CancellationToken.None);
        await outbox.ThirdPass.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await host.StopAsync(CancellationToken.None);

        // One transport per pass, and every one of them released with its scope. A pump that held
        // one scope for its lifetime would show a single transport across all the passes.
        Assert.True(outbox.Passes >= 3);
        Assert.Equal(outbox.Passes, ledger.Created);
        Assert.Equal(ledger.Created, ledger.Disposed);
    }

    private static ServiceLifetime LifetimeOf<TService>(IServiceCollection services) =>
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(TService)).Lifetime;

    private sealed class CountingRetentionJob : IRetentionJob
    {
        public int Runs;

        public Task<RetentionRunReport> RunAsync(
            RetentionRunOptions options,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Runs);
            return Task.FromResult(new RetentionRunReport(
                Guid.NewGuid(),
                options.DryRun,
                DateTimeOffset.UnixEpoch,
                DateTimeOffset.UnixEpoch,
                []));
        }
    }

    private sealed class CountingLifetime : IHostApplicationLifetime
    {
        public int StopRequests;

        public CancellationToken ApplicationStarted => CancellationToken.None;

        public CancellationToken ApplicationStopping => CancellationToken.None;

        public CancellationToken ApplicationStopped => CancellationToken.None;

        public void StopApplication() => Interlocked.Increment(ref StopRequests);
    }

    private sealed class EmptyOutbox : ICallbackOutboxRepository
    {
        public int Passes;

        public TaskCompletionSource ThirdPass { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<IReadOnlyList<CallbackOutboxMessage>> DequeueReadyAsync(
            int batchSize,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref Passes) >= 3)
            {
                ThirdPass.TrySetResult();
            }

            return Task.FromResult<IReadOnlyList<CallbackOutboxMessage>>([]);
        }

        public Task<ResultCallbackEntity> EnqueueAsync(
            ResultCallbackEntity callback,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("An empty outbox receives nothing.");

        public Task<bool> CompleteDeliveryAsync(
            string callbackId,
            string leaseToken,
            CallbackDeliveryUpdate update,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("An empty outbox has nothing to complete.");
    }

    private sealed class TransportLedger
    {
        public int Created;

        public int Disposed;
    }

    private sealed class RecordingTransport : ICurrentGoldenHourCallbackTransport, IDisposable
    {
        private readonly TransportLedger ledger;

        public RecordingTransport(TransportLedger ledger)
        {
            this.ledger = ledger;
            Interlocked.Increment(ref ledger.Created);
        }

        public Task<CallbackTransportResult> SendAsync(
            CallbackOutboxMessage message,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException("An empty outbox leaves nothing to send.");

        public void Dispose() => Interlocked.Increment(ref ledger.Disposed);
    }
}
