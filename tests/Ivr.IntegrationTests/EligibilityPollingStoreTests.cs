using Ivr.Domain.Policies;
using Ivr.Infrastructure.Configuration;
using Ivr.Infrastructure.Eligibility;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ivr.IntegrationTests;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class EligibilityPollingStoreTests(PostgresPersistenceFixture fixture)
{
    [Theory]
    [InlineData("pending", 1)]
    [InlineData("lab-pending", 1)]
    [InlineData("decided", 0)]
    [InlineData("revoked", 0)]
    [InlineData("expired", 0)]
    [InlineData("review", 0)]
    [InlineData("closed", 0)]
    [Trait("TestId", "IT-ELIG-LOOP-01")]
    public async Task OnlyLivePendingIntakeWorkCanBeEvaluated(string state, int expected)
    {
        await fixture.ResetAsync();
        await new ApiMatrixFixture(fixture).SeedGraphAsync(includeTerminalResult: false, eligible: false);
        var factory = fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>();
        await using (IvrDbContext db = await factory.CreateDbContextAsync())
        {
            var job = await db.CallJobs.SingleAsync();
            var task = await db.ConfirmationTasks.SingleAsync();
            if (state == "decided") job.EligibilityDecision = EligibilityDecisions.Eligible;
            if (state == "lab-pending")
            {
                job.Status = "CREATED";
                job.QueueStatus = "HELD_ELIGIBILITY";
            }
            if (state == "revoked")
            {
                task.RevokedAt = DateTimeOffset.UtcNow;
                task.RevokeReason = "SYNTHETIC_RECALL";
                task.RevokeOrderVersion = "2";
            }
            if (state == "expired") job.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1);
            if (state == "review")
            {
                job.Status = "HELD_ADMIN_REVIEW";
                job.QueueStatus = "HELD_ADMIN_REVIEW";
            }
            if (state == "closed")
            {
                job.ClosedAt = DateTimeOffset.UtcNow;
                job.ClosedReason = "SYNTHETIC_CLOSED";
            }
            await db.SaveChangesAsync();
        }

        var store = new PostgresEligibilityPendingStore(factory,
            Options.Create(new IvrOptions
            {
                ExecutionMode = state == "lab-pending" ? IvrOptions.LabRealSimExecutionMode : IvrOptions.MockExecutionMode,
            }), TimeProvider.System);
        Assert.Equal(expected, (await store.ReadAsync(16, CancellationToken.None)).Count);
    }
}
