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
