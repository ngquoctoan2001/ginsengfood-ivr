using System.Text.Json;
using Ivr.Domain.Policies;
using Ivr.Infrastructure.Crm;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ivr.IntegrationTests;

[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class OptOutProposalPersistenceTests(PostgresPersistenceFixture fixture)
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 18, 6, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "IT-OPTOUT-PROPOSE-03")]
    public async Task AProposalIsQueuedAndAuditedWhileNothingIsSuppressedInsideIvr()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();
        var proposer = new QueueOnlySuppressionProposer(factory);
        var proposal = new SuppressionProposal(
            "contact-ref-optout-03",
            SuppressionChannel.PhoneCall,
            OptOutReasonCodes.ThresholdReached,
            SignalCount: 3,
            AdminConfirmed: false,
            CorrelationId: "corr-optout-03");

        string proposalId = await proposer.ProposeAsync(proposal, Now, CancellationToken.None);

        // CRM does not accept proposals yet, so the row must be durable and waiting — losing it
        // would silently drop a signal the customer actually gave.
        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        ReviewItemEntity queued = await verification.ReviewItems.AsNoTracking()
            .SingleAsync(item => item.ReviewItemId == proposalId);
        Assert.Equal(QueueOnlySuppressionProposer.ReviewSourceType, queued.SourceType);
        Assert.Equal(SuppressionProposalStatus.PendingCrm, queued.Status);
        Assert.Contains("PHONECALL", queued.Reason, StringComparison.Ordinal);
        Assert.Contains("signals=3", queued.Reason, StringComparison.Ordinal);

        // Nothing anywhere says IVR suppressed the contact. The registry belongs to CRM, so a
        // status or audit claiming a local block would be IVR asserting authority it lacks.
        Assert.DoesNotContain(
            await verification.ReviewItems.AsNoTracking().ToListAsync(),
            item => item.Status.Contains("SUPPRESS", StringComparison.OrdinalIgnoreCase));

        AuditLogEntity audit = await verification.AuditLog.AsNoTracking()
            .SingleAsync(entry => entry.Action == "IVR_OPTOUT_SUPPRESSION_PROPOSED");
        Assert.Equal("SUPPRESSION_PROPOSAL", audit.TargetType);
        Assert.Equal("THRESHOLD_RULE", audit.ActorId);
        using JsonDocument data = JsonDocument.Parse(audit.DataJson);
        Assert.False(data.RootElement.GetProperty("suppressed_by_ivr").GetBoolean());
        Assert.Equal("CRM", data.RootElement.GetProperty("registry_owner").GetString());
        Assert.Equal("PHONECALL", data.RootElement.GetProperty("channel").GetString());

        // P4-6 §6.4. The console review queue lists ivr_review_items filtered only by status,
        // so a queued proposal is discoverable by an administrator without a new screen. It
        // resolves to no call job on purpose: a proposal is about a contact, not about one call.
        List<ReviewItemEntity> openQueue = await verification.ReviewItems.AsNoTracking()
            .Where(item => item.Status == SuppressionProposalStatus.PendingCrm)
            .ToListAsync();
        Assert.Contains(openQueue, item => item.ReviewItemId == proposalId);

        // Re-proposing the same contact must not stack duplicates in CRM's future inbox.
        string replay = await proposer.ProposeAsync(proposal, Now.AddMinutes(5), CancellationToken.None);
        Assert.Equal(proposalId, replay);
        await using IvrDbContext second = await factory.CreateDbContextAsync();
        Assert.Equal(
            1,
            await second.ReviewItems.CountAsync(item =>
                item.SourceType == QueueOnlySuppressionProposer.ReviewSourceType));
    }

    [Fact]
    [Trait("TestId", "IT-OPTOUT-FAILSAFE-04")]
    public async Task AHeldDecisionWritesNothingSoAQuietSystemNeverBlocksAnyone()
    {
        await fixture.ResetAsync();
        IDbContextFactory<IvrDbContext> factory = fixture.Services
            .GetRequiredService<IDbContextFactory<IvrDbContext>>();

        // Below the threshold the policy holds, and holding must be genuinely inert: no queue
        // row, no audit entry, nothing a later reader could mistake for a decision.
        SuppressionDecision held = OptOutSuppressionPolicy.Decide(
            1,
            OptOutThresholdPolicy.Default,
            adminConfirmed: false);
        Assert.Equal(SuppressionOutcome.Hold, held.Outcome);

        await using IvrDbContext verification = await factory.CreateDbContextAsync();
        Assert.Equal(
            0,
            await verification.ReviewItems.CountAsync(item =>
                item.SourceType == QueueOnlySuppressionProposer.ReviewSourceType));
        Assert.Equal(
            0,
            await verification.AuditLog.CountAsync(entry =>
                entry.Action == "IVR_OPTOUT_SUPPRESSION_PROPOSED"));
    }
}
