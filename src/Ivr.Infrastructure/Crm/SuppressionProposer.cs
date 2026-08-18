using Ivr.Domain.Policies;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Crm;

/// <summary>
/// Statuses a suppression proposal can hold. Note what is missing: there is no `SUPPRESSED`.
/// IVR proposes and waits; the do-not-call registry belongs to CRM (`DO-CORR-2`), so a status
/// meaning "IVR has blocked this contact" would be a status IVR has no right to write.
/// </summary>
public static class SuppressionProposalStatus
{
    /// <summary>Written and waiting. CRM does not accept proposals yet, so this is where they sit.</summary>
    public const string PendingCrm = "PENDING_CRM";

    /// <summary>CRM acknowledged. Nothing can set this today; it exists so the queue has a terminal state.</summary>
    public const string AcceptedByCrm = "ACCEPTED_BY_CRM";
}

public sealed record SuppressionProposal(
    string ContactReference,
    SuppressionChannel Channel,
    string ReasonCode,
    int SignalCount,
    bool AdminConfirmed,
    string CorrelationId);

public interface ISuppressionProposer
{
    /// <summary>
    /// Records a do-not-call proposal for CRM. Returns the proposal id.
    /// Never blocks a contact locally, and never throws on a CRM outage — a proposal that cannot
    /// be delivered stays queued, because losing it would silently drop a customer's signal while
    /// dropping it loudly would be worse: an error path that suppresses on failure would block
    /// people precisely when the system is least able to explain why.
    /// </summary>
    public Task<string> ProposeAsync(
        SuppressionProposal proposal,
        DateTimeOffset now,
        CancellationToken cancellationToken);
}

/// <summary>
/// Queue-only proposer (W-0034 / P4-6 §6.3).
/// <para>
/// CRM exposes no suppression-propose endpoint yet, so this writes a durable queue row and an
/// audit entry and stops. It deliberately holds no HTTP client: adding one now would be an egress
/// surface with no counterpart, and <c>UT-ARCH-NO-OPS-EGRESS-05</c> would rightly fail it.
/// </para>
/// <para>
/// The queue reuses <c>ivr_review_items</c> under its own <c>SourceType</c> rather than adding a
/// table. A proposal already is a review-queue concept — an admin needs to see it, confirm or
/// reject it, and it needs the same retention. A parallel table would duplicate the lifecycle and
/// split the admin surface in two.
/// </para>
/// </summary>
public sealed class QueueOnlySuppressionProposer(IDbContextFactory<IvrDbContext> dbContextFactory)
    : ISuppressionProposer
{
    public const string ReviewSourceType = "IVR_OPTOUT_PROPOSAL";

    public async Task<string> ProposeAsync(
        SuppressionProposal proposal,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        ArgumentException.ThrowIfNullOrWhiteSpace(proposal.ContactReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(proposal.CorrelationId);

        string proposalId = string.Concat("OPTOUT-", proposal.ContactReference);
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        bool exists = await context.ReviewItems
            .AsNoTracking()
            .AnyAsync(item => item.ReviewItemId == proposalId, cancellationToken)
            .ConfigureAwait(false);
        if (exists)
        {
            // Re-proposing the same contact must not stack duplicate rows in CRM's future inbox.
            return proposalId;
        }

        context.ReviewItems.Add(new ReviewItemEntity
        {
            ReviewItemId = proposalId,
            SourceType = ReviewSourceType,
            SourceId = proposal.ContactReference,
            Reason = string.Concat(
                proposal.ReasonCode,
                ";channel=",
                proposal.Channel.ToString().ToUpperInvariant(),
                ";signals=",
                proposal.SignalCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ";admin_confirmed=",
                proposal.AdminConfirmed ? "true" : "false"),
            Status = SuppressionProposalStatus.PendingCrm,
            CorrelationId = proposal.CorrelationId,
            CreatedAt = now,
        });

        context.AuditLog.Add(new AuditLogEntity
        {
            AuditId = Guid.NewGuid(),
            Action = "IVR_OPTOUT_SUPPRESSION_PROPOSED",
            ActorId = proposal.AdminConfirmed ? "ADMIN_CONFIRMED" : "THRESHOLD_RULE",
            ActorType = proposal.AdminConfirmed ? "admin" : "service",
            TargetType = "SUPPRESSION_PROPOSAL",
            TargetId = proposalId,
            Reason = proposal.ReasonCode,
            CorrelationId = proposal.CorrelationId,
            DataJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                channel = proposal.Channel.ToString().ToUpperInvariant(),
                reason_code = proposal.ReasonCode,
                signal_count = proposal.SignalCount,
                admin_confirmed = proposal.AdminConfirmed,
                // Stated in the audit row itself so a later reader cannot mistake a queued
                // proposal for an effective block.
                suppressed_by_ivr = false,
                registry_owner = "CRM",
            }),
            CreatedAt = now,
        });

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return proposalId;
    }
}
