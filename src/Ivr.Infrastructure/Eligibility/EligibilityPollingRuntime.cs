using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Ivr.Domain.Policies;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Eligibility;

public sealed class EligibilityPollingOptions
{
    public const string SectionName = "Ivr:EligibilityPolling";
    public bool Enabled { get; set; }
    public string ApiBaseUrl { get; set; } = string.Empty;
    public string ServiceToken { get; set; } = string.Empty;
    public int BatchSize { get; set; } = 16;
    public int PollIntervalMilliseconds { get; set; } = 1000;
    public int RequestTimeoutSeconds { get; set; } = 10;
}

public sealed record PendingEligibilityTask(string TaskId, string JobId, string CorrelationId);

public interface IEligibilityPendingStore
{
    public Task<IReadOnlyList<PendingEligibilityTask>> ReadAsync(
        int batchSize, CancellationToken cancellationToken);
}

/// <summary>
/// Reads durable intake work; the existing eligibility endpoint owns every state change.
/// No lease is held across HTTP. Concurrent workers use the same idempotency key, and the
/// endpoint serializes persistence per task. A crash before the commit leaves work pending;
/// a crash after it leaves the persisted decision, which this query no longer selects.
/// </summary>
public sealed class PostgresEligibilityPendingStore(
    IDbContextFactory<IvrDbContext> factory,
    IOptions<IvrOptions> execution,
    TimeProvider timeProvider) : IEligibilityPendingStore
{
    public async Task<IReadOnlyList<PendingEligibilityTask>> ReadAsync(
        int batchSize, CancellationToken cancellationToken)
    {
        await using IvrDbContext db = await factory.CreateDbContextAsync(cancellationToken);
        DateTimeOffset now = timeProvider.GetUtcNow();
        bool mock = execution.Value.ExecutionMode == IvrOptions.MockExecutionMode;
        return await (
            from job in db.CallJobs.AsNoTracking()
            join task in db.ConfirmationTasks.AsNoTracking() on job.TaskId equals task.TaskId
            where job.EligibilityDecision == EligibilityDecisions.Pending
                && job.ClosedAt == null && job.ExpiresAt > now && task.RevokedAt == null
                && (mock
                    ? job.Status == "DRY_RUN" && job.QueueStatus == "HELD_MOCK"
                    : job.Status == "CREATED" && job.QueueStatus == "HELD_ELIGIBILITY")
            orderby job.ExpiresAt, job.IvrCallJobId
            select new PendingEligibilityTask(task.TaskId, job.IvrCallJobId, task.CorrelationId))
            .Take(batchSize).ToListAsync(cancellationToken);
    }
}

public sealed class EligibilityPollingRuntime(
    IEligibilityPendingStore store,
    IHttpClientFactory clients,
    IOptions<EligibilityPollingOptions> options)
{
    public const string HttpClientName = "ivr-eligibility-internal";

    public async Task<int> RunOnceAsync(CancellationToken cancellationToken = default)
    {
        EligibilityPollingOptions settings = options.Value;
        if (!settings.Enabled)
        {
            return 0;
        }

        IReadOnlyList<PendingEligibilityTask> pending = await store.ReadAsync(
            settings.BatchSize, cancellationToken);
        using HttpClient client = clients.CreateClient(HttpClientName);
        int failures = 0;
        foreach (PendingEligibilityTask task in pending)
        {
            // The job id distinguishes a new intake from a purged/recreated task id. No raw
            // destination or credential is put into a key or log.
            string key = "elig-worker-" + Convert.ToHexStringLower(
                SHA256.HashData(Encoding.UTF8.GetBytes(task.JobId)));
            using var request = new HttpRequestMessage(HttpMethod.Post,
                new Uri(new Uri(settings.ApiBaseUrl.TrimEnd('/') + "/"),
                    "v1/ivr/order-confirmation/eligibility-checks"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
            request.Headers.Add("X-Source-System", "ivr-worker");
            request.Headers.Add("X-Service-Scope", "ivr.internal.write");
            request.Headers.Add("X-Correlation-Id", task.CorrelationId);
            request.Headers.Add("Idempotency-Key", key);
            request.Content = JsonContent.Create(new { task_id = task.TaskId });
            try
            {
                using HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    failures++;
                }
            }
            catch (HttpRequestException)
            {
                failures++;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                failures++;
            }
        }

        if (failures > 0)
        {
            // PollingJobHost supplies bounded backoff and unhealthy-loop visibility. Failed
            // requests leave the database untouched, and one failure cannot starve this batch.
            throw new HttpRequestException(
                $"Eligibility batch has {failures} failed request(s); pending work will be retried.");
        }

        return pending.Count;
    }
}

public static class EligibilityPollingServiceCollectionExtensions
{
    public static IServiceCollection AddIvrEligibilityPolling(
        this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddOptions<EligibilityPollingOptions>()
            .Bind(configuration.GetSection(EligibilityPollingOptions.SectionName))
            .Configure(options => options.ServiceToken = configuration["IVR_INTERNAL_SERVICE_TOKEN"] ?? string.Empty)
            .Validate(options => !options.Enabled ||
                (Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out Uri? uri)
                    && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                    && string.IsNullOrEmpty(uri.UserInfo) && string.IsNullOrEmpty(uri.Query)
                    && string.IsNullOrEmpty(uri.Fragment) && uri.AbsolutePath == "/"
                    && !string.IsNullOrWhiteSpace(options.ServiceToken)),
                "Enabled eligibility polling requires an HTTP(S) API origin and internal credential.")
            .Validate(options => options.BatchSize is >= 1 and <= 256
                && options.PollIntervalMilliseconds is >= 100 and <= 60000
                && options.RequestTimeoutSeconds is >= 1 and <= 60,
                "Eligibility polling bounds are invalid.")
            .ValidateOnStart();
        services.AddSingleton<IEligibilityPendingStore, PostgresEligibilityPendingStore>();
        services.AddSingleton<EligibilityPollingRuntime>();
        services.AddHttpClient(EligibilityPollingRuntime.HttpClientName, (provider, client) =>
            client.Timeout = TimeSpan.FromSeconds(provider.GetRequiredService<
                IOptions<EligibilityPollingOptions>>().Value.RequestTimeoutSeconds))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        return services;
    }
}
