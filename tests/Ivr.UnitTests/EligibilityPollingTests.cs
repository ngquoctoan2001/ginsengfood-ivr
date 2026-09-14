using System.Net;
using Ivr.Infrastructure.Eligibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.UnitTests;

public sealed class EligibilityPollingTests
{
    [Fact]
    [Trait("TestId", "UT-ELIG-LOOP-01")]
    public async Task DisabledWorkerDoesNotReadOrSend()
    {
        var store = new PendingStore();
        using var transport = new Transport();
        var runtime = new EligibilityPollingRuntime(store, transport,
            Options.Create(new EligibilityPollingOptions()));
        Assert.Equal(0, await runtime.RunOnceAsync());
        Assert.Equal(0, store.Reads);
        Assert.Empty(transport.Keys);
    }

    [Fact]
    [Trait("TestId", "UT-ELIG-LOOP-02")]
    public async Task LostResponseRetriesTheSameKeyAfterRestartAndDoesNotStarveTheBatch()
    {
        var store = new PendingStore();
        using var transport = new Transport { LoseFirstResponse = true };
        await Assert.ThrowsAsync<HttpRequestException>(() => Runtime(store, transport).RunOnceAsync());
        Assert.Equal(2, transport.Keys.Count);
        Assert.Equal(2, await Runtime(store, transport).RunOnceAsync());
        Assert.Equal(transport.Keys[0], transport.Keys[2]);
        Assert.Equal(transport.Keys[1], transport.Keys[3]);
        Assert.NotEqual(transport.Keys[0], transport.Keys[1]);
        Assert.All(transport.Sources, value => Assert.Equal("ivr-worker/ivr.internal.write", value));
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [Trait("TestId", "UT-ELIG-LOOP-03")]
    public async Task FailedHttpResponsesRemainFailuresForTheHostBackoff(HttpStatusCode status)
    {
        using var transport = new Transport { Status = status };
        await Assert.ThrowsAsync<HttpRequestException>(() => Runtime(new PendingStore(), transport).RunOnceAsync());
        Assert.Equal(2, transport.Keys.Count);
    }

    [Fact]
    [Trait("TestId", "UT-ELIG-LOOP-04")]
    public void EnabledLoopCannotStartWithoutAnInternalCredentialAndOrigin()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Ivr:EligibilityPolling:Enabled"] = "true",
        }).Build();
        var services = new ServiceCollection();
        services.AddIvrEligibilityPolling(config);
        using ServiceProvider provider = services.BuildServiceProvider();
        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<
            IOptions<EligibilityPollingOptions>>().Value);
    }

    private static EligibilityPollingRuntime Runtime(PendingStore store, Transport transport) =>
        new(store, transport, Options.Create(new EligibilityPollingOptions
        {
            Enabled = true,
            ApiBaseUrl = "http://ivr-api.test",
            ServiceToken = "synthetic-internal-token",
        }));

    private sealed class PendingStore : IEligibilityPendingStore
    {
        public int Reads { get; private set; }
        public Task<IReadOnlyList<PendingEligibilityTask>> ReadAsync(int batchSize, CancellationToken cancellationToken)
        {
            Reads++;
            IReadOnlyList<PendingEligibilityTask> tasks =
            [new("TASK-A", "JOB-A", "corr-a"), new("TASK-B", "JOB-B", "corr-b")];
            return Task.FromResult(tasks);
        }
    }

    private sealed class Transport : HttpMessageHandler, IHttpClientFactory
    {
        public List<string> Keys { get; } = [];
        public List<string> Sources { get; } = [];
        public bool LoseFirstResponse { get; init; }
        public HttpStatusCode Status { get; init; } = HttpStatusCode.OK;
        public HttpClient CreateClient(string name) => new(this, disposeHandler: false);
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Keys.Add(request.Headers.GetValues("Idempotency-Key").Single());
            Sources.Add(request.Headers.GetValues("X-Source-System").Single() + "/"
                + request.Headers.GetValues("X-Service-Scope").Single());
            if (LoseFirstResponse && Keys.Count == 1)
                throw new HttpRequestException("Synthetic response lost after commit.");
            return Task.FromResult(new HttpResponseMessage(Status));
        }
    }
}
