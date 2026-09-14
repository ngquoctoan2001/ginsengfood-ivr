using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ivr.Api.Internal;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class EligibilityConcurrencyTests(PostgresPersistenceFixture fixture)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("TestId", "IT-ELIG-CONCURRENT-01")]
    public async Task ParallelEligibilityCommitsOnceAndPreservesReplayAfterRestart(bool sameTask)
    {
        await fixture.ResetAsync();
        await new ApiMatrixFixture(fixture).SeedGraphAsync(includeTerminalResult: false, eligible: false);
        var factory = fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>();
        string[] taskIds = Enumerable.Range(0, 10)
            .Select(index => sameTask || index == 0 ? "TASK-P2-8" : $"TASK-P2-8-{index}").ToArray();
        if (!sameTask)
        {
            await using IvrDbContext db = await factory.CreateDbContextAsync();
            ConfirmationTaskEntity task = await db.ConfirmationTasks.AsNoTracking().SingleAsync();
            CallJobEntity job = await db.CallJobs.AsNoTracking().SingleAsync();
            TaskIntakeOutboxEntity outbox = await db.TaskIntakeOutbox.AsNoTracking().SingleAsync();
            for (int index = 1; index < taskIds.Length; index++)
            {
                var nextTask = JsonSerializer.Deserialize<ConfirmationTaskEntity>(JsonSerializer.Serialize(task))!;
                nextTask.Id = Guid.NewGuid();
                nextTask.TaskId = taskIds[index];
                nextTask.OfficialOrderId += $"-{index}";
                nextTask.IdempotencyKey += $"-{index}";
                var nextJob = JsonSerializer.Deserialize<CallJobEntity>(JsonSerializer.Serialize(job))!;
                nextJob.IvrCallJobId += $"-{index}";
                nextJob.TaskId = nextTask.TaskId;
                nextJob.OfficialOrderId = nextTask.OfficialOrderId;
                var nextOutbox = JsonSerializer.Deserialize<TaskIntakeOutboxEntity>(JsonSerializer.Serialize(outbox))!;
                nextOutbox.OutboxId = Guid.NewGuid();
                nextOutbox.TaskId = nextTask.TaskId;
                nextOutbox.IvrCallJobId = nextJob.IvrCallJobId;
                db.AddRange(nextTask, nextJob, nextOutbox);
            }
            await db.SaveChangesAsync();
        }

        string firstBody;
        await using (InternalAdminApiTestApplication app = await InternalAdminApiTestApplication.StartAsync(fixture.ConnectionString))
        {
            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            Task<HttpResponseMessage>[] pending = taskIds.Select(async (taskId, index) =>
            {
                await start.Task;
                return await SendAsync(app.Client, taskId, $"elig-parallel-key-{index}");
            }).ToArray();
            start.SetResult();
            HttpResponseMessage[] responses = await Task.WhenAll(pending);
            try
            {
                Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
                firstBody = await responses[0].Content.ReadAsStringAsync();
            }
            finally { foreach (HttpResponseMessage response in responses) response.Dispose(); }
        }

        await using (InternalAdminApiTestApplication restarted = await InternalAdminApiTestApplication.StartAsync(fixture.ConnectionString))
        {
            using HttpResponseMessage replay = await SendAsync(restarted.Client, taskIds[0], "elig-parallel-key-0");
            Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
            Assert.Equal(firstBody, await replay.Content.ReadAsStringAsync());
            using HttpResponseMessage conflict = await SendAsync(restarted.Client, "TASK-DIFFERENT", "elig-parallel-key-0");
            Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
            Assert.Contains("IVR_IDEMPOTENCY_CONFLICT", await conflict.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        }

        await using IvrDbContext observed = await factory.CreateDbContextAsync();
        Assert.Equal(sameTask ? 1 : 10, await observed.AuditLog.CountAsync(audit => audit.Action == "ELIGIBILITY_EVALUATED"));
        Assert.Equal(10, await observed.IdempotencyKeys.CountAsync(key => key.Key.StartsWith("elig-parallel-key-")));
        Assert.Equal(0, await observed.CallResults.CountAsync());
        Assert.Equal(0, await observed.ResultCallbacks.CountAsync());
    }

    private static async Task<HttpResponseMessage> SendAsync(HttpClient client, string taskId, string key)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/ivr/order-confirmation/eligibility-checks")
        {
            Content = JsonContent.Create(new EligibilityLifecycleRequest(taskId)),
        };
        request.Headers.Add("Authorization", $"Bearer {InternalAdminApiTestApplication.InternalToken}");
        request.Headers.Add("X-Source-System", "ivr-worker");
        request.Headers.Add("X-Service-Scope", "ivr.internal.write");
        request.Headers.Add("X-Correlation-Id", "corr-elig-concurrent");
        request.Headers.Add("Idempotency-Key", key);
        return await client.SendAsync(request);
    }
}
