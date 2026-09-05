using System.Text.Json;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Ports;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Intake;

public static class CandidateAttemptPolicies
{
    public const string Version = "mock-lab-v1";

    public static IReadOnlyList<AttemptPolicySnapshot> Create() =>
    [
        AttemptPolicySnapshot.Create(
            PolicyVersion.Create(Version),
            IvrProgramCode.GoldenHour,
            2,
            [TimeSpan.Zero, TimeSpan.FromSeconds(150)],
            TimeSpan.FromMinutes(5),
            AttemptPolicyApproval.CandidateMockLabOnly),
        AttemptPolicySnapshot.Create(
            PolicyVersion.Create(Version),
            IvrProgramCode.TwentyFourSeven,
            2,
            [TimeSpan.Zero, TimeSpan.FromSeconds(450)],
            TimeSpan.FromMinutes(15),
            AttemptPolicyApproval.CandidateMockLabOnly),
    ];
}

/// <summary>
/// W-0196 / <c>OD-V1-08</c> + <c>OD-V1-16</c>. The attempt policy the owner signed on 2026-09-05.
/// <para>
/// The numbers are <c>D-10</c>'s, and the choice between three conflicting sources was made on
/// one ground: phase-8 gave Golden Hour a 600-second window, and "Giờ Vàng" is a five-minute
/// promise, so that source contradicts itself. <c>D-10</c> is newer, is internally consistent with
/// the five-minute window, and is what the scheduler, the database and every test already run on.
/// </para>
/// <para>
/// This is a <b>separate version</b> from <c>mock-lab-v1</c> rather than a promotion of it, and
/// deliberately so: <c>W-0151</c> asked that nothing rename or re-approve the candidate, because a
/// version that changes approval underneath a running job changes what that job was admitted
/// under. Accepted tasks keep the snapshot they were admitted with either way.
/// </para>
/// <para>
/// The hours of day a call may be placed are <b>not</b> here. They are not per-program and they
/// are not versioned with the policy; see <c>CallingWindowOptions</c>.
/// </para>
/// </summary>
public static class SignedProductionAttemptPolicies
{
    public const string Version = "gh-247-prod-v1";

    public const string SignedDecisionRef = "OD-V1-08+OD-V1-16@2026-09-05";

    public static IReadOnlyList<AttemptPolicySnapshot> Create() =>
    [
        AttemptPolicySnapshot.Create(
            PolicyVersion.Create(Version),
            IvrProgramCode.GoldenHour,
            2,
            [TimeSpan.Zero, TimeSpan.FromSeconds(150)],
            TimeSpan.FromMinutes(5),
            AttemptPolicyApproval.OwnerApproved),
        AttemptPolicySnapshot.Create(
            PolicyVersion.Create(Version),
            IvrProgramCode.TwentyFourSeven,
            2,
            [TimeSpan.Zero, TimeSpan.FromSeconds(450)],
            TimeSpan.FromMinutes(15),
            AttemptPolicyApproval.OwnerApproved),
    ];
}

public sealed class PostgresAttemptPolicyRegistry(
    IDbContextFactory<IvrDbContext> dbContextFactory) : IAttemptPolicyRegistry
{
    public async ValueTask<AttemptPolicySnapshot> ResolveAsync(
        PolicyVersion version,
        IvrProgramCode program,
        ExecutionMode executionMode,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(version);
        string programValue = program switch
        {
            IvrProgramCode.GoldenHour => "GOLDEN_HOUR",
            IvrProgramCode.TwentyFourSeven => "TWENTY_FOUR_SEVEN",
            _ => throw new InvalidOperationException("Unknown IVR program."),
        };
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        AttemptPolicyEntity entity = await context.AttemptPolicies
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.PolicyVersion == version.Value
                    && candidate.ProgramType == programValue,
                cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Versioned attempt policy was not found.");
        string modeValue = executionMode switch
        {
            ExecutionMode.Mock => "MOCK",
            ExecutionMode.LabRealSim => "LAB_REAL_SIM",
            ExecutionMode.ProductionReal => "PRODUCTION_REAL",
            _ => throw new InvalidOperationException("Unknown execution mode."),
        };
        string[] allowedModes = JsonSerializer.Deserialize<string[]>(
                entity.AllowedExecutionModesJson)
            ?? [];
        if (!allowedModes.Contains(modeValue, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                "Attempt policy is not approved for the execution mode.");
        }

        int[] offsets = JsonSerializer.Deserialize<int[]>(
                entity.AttemptOffsetsSecondsJson)
            ?? throw new InvalidOperationException("Attempt-policy offsets are invalid.");
        AttemptPolicySnapshot snapshot = AttemptPolicySnapshot.Create(
            PolicyVersion.Create(entity.PolicyVersion),
            program,
            entity.MaxAttempts,
            offsets.Select(offset => TimeSpan.FromSeconds(offset)),
            TimeSpan.FromSeconds(entity.ConfirmationWindowSeconds),
            entity.ApprovedForProduction
                ? AttemptPolicyApproval.OwnerApproved
                : AttemptPolicyApproval.CandidateMockLabOnly);
        snapshot.EnsureEnvironmentAllowed(executionMode);
        return snapshot;
    }
}
