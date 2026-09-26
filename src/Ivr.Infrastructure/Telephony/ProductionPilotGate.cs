using Ivr.Infrastructure.FeatureFlags;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ivr.Infrastructure.Telephony;

/// <summary>
/// Q-28 (PA2, 2026-09-26). The production pilot list, answered from the trunk configuration and
/// the approval rows of <c>ivr_runtime_gate_approvals</c>.
/// <para>
/// The list is configuration (<see cref="SipTrunkOptions.PilotDestinations"/>), but configuration
/// alone opens nothing. It counts only while a live four-eyes <c>PRODUCTION_PILOT_LIST</c> approval
/// for the environment binds exactly this list, so a deployment that adds one entry has closed the
/// pilot until someone other than the proposer approves the new list. A store that cannot be read
/// answers "not approved", as every gate reading that table does.
/// </para>
/// </summary>
public sealed class PostgresProductionPilotGate(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider,
    IOptions<SipTrunkOptions> sipTrunkOptions) : IProductionPilotGate
{
    public const string ListEmpty = "PRODUCTION_PILOT_LIST_EMPTY";

    public const string ListNotApproved = "PRODUCTION_PILOT_LIST_NOT_APPROVED";

    public const string NotInPilot = "DESTINATION_NOT_IN_PILOT";

    public const string PilotDestinationApproved = "PRODUCTION_PILOT_DESTINATION_APPROVED";

    public Task<bool> IsOpenAsync(
        string environment,
        CancellationToken cancellationToken = default) =>
        RuntimeGateApprovalReader.AnyLiveForEnvironmentAsync(
            dbContextFactory,
            timeProvider,
            RuntimeGateApprovalKinds.ProductionCallOpen,
            environment,
            cancellationToken);

    public async Task<ProductionPilotDecision> EvaluateAsync(
        string environment,
        string destinationReference,
        CancellationToken cancellationToken = default)
    {
        List<string> pilot = sipTrunkOptions.Value.PilotDestinations;
        if (pilot.Count == 0)
        {
            return new ProductionPilotDecision(false, ListEmpty);
        }

        bool approved = await RuntimeGateApprovalReader.AnyLiveForEnvironmentAndChangeAsync(
            dbContextFactory,
            timeProvider,
            RuntimeGateApprovalKinds.ProductionPilotList,
            environment,
            ProductionPilotFingerprint.ListHash(pilot),
            cancellationToken);
        if (!approved)
        {
            return new ProductionPilotDecision(false, ListNotApproved);
        }

        return pilot.Contains(destinationReference, StringComparer.Ordinal)
            ? new ProductionPilotDecision(true, PilotDestinationApproved)
            : new ProductionPilotDecision(false, NotInPilot);
    }
}
