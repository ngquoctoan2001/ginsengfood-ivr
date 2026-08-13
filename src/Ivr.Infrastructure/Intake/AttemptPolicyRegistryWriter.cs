using System.Data;
using System.Text.Json;
using Ivr.Domain.Confirmation;
using Ivr.Domain.Privacy;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivr.Infrastructure.Intake;

public interface IAttemptPolicyRegistryWriter
{
    public Task RegisterNewVersionAsync(
        AttemptPolicySnapshot policy,
        IReadOnlyCollection<ExecutionMode> allowedExecutionModes,
        string actorId,
        string reason,
        string correlationId,
        CancellationToken cancellationToken = default);
}

public sealed class PostgresAttemptPolicyRegistryWriter(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    TimeProvider timeProvider) : IAttemptPolicyRegistryWriter
{
    public async Task RegisterNewVersionAsync(
        AttemptPolicySnapshot policy,
        IReadOnlyCollection<ExecutionMode> allowedExecutionModes,
        string actorId,
        string reason,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(allowedExecutionModes);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);
        PiiGuard.EnsureSafeText(actorId);
        PiiGuard.EnsureSafeText(reason);
        PiiGuard.EnsureSafeText(correlationId);
        ExecutionMode[] modes = allowedExecutionModes
            .Distinct()
            .OrderBy(mode => mode)
            .ToArray();
        if (modes.Length == 0 || modes.Any(mode => !Enum.IsDefined(mode)))
        {
            throw new InvalidOperationException("Attempt policy requires known execution modes.");
        }

        foreach (ExecutionMode mode in modes)
        {
            policy.EnsureEnvironmentAllowed(mode);
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        await using IvrDbContext context = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken).ConfigureAwait(false);
        string program = policy.Program switch
        {
            IvrProgramCode.GoldenHour => "GOLDEN_HOUR",
            IvrProgramCode.TwentyFourSeven => "TWENTY_FOUR_SEVEN",
            _ => throw new InvalidOperationException("Unknown IVR program."),
        };
        bool exists = await context.AttemptPolicies.AnyAsync(
            candidate => candidate.PolicyVersion == policy.Version.Value
                && candidate.ProgramType == program,
            cancellationToken).ConfigureAwait(false);
        if (exists)
        {
            throw new InvalidOperationException(
                "Attempt-policy versions are immutable; register a new version.");
        }

        string[] modeValues = modes.Select(ToStorageValue).ToArray();
        context.AttemptPolicies.Add(new AttemptPolicyEntity
        {
            PolicyVersion = policy.Version.Value,
            ProgramType = program,
            MaxAttempts = policy.MaxCustomerAttempts,
            AttemptOffsetsSecondsJson = JsonSerializer.Serialize(
                policy.AttemptOffsets.Select(offset => checked((int)offset.TotalSeconds))),
            ConfirmationWindowSeconds = checked((int)policy.ConfirmationWindowDuration.TotalSeconds),
            AllowedExecutionModesJson = JsonSerializer.Serialize(modeValues),
            ApprovedForProduction = policy.Approval == AttemptPolicyApproval.OwnerApproved,
            CreatedAt = now,
        });
        context.AuditLog.Add(new AuditLogEntity
        {
            AuditId = Guid.NewGuid(),
            ActorId = actorId,
            ActorType = "operator",
            Action = "ATTEMPT_POLICY_VERSION_REGISTERED",
            TargetType = "attempt-policy",
            TargetId = string.Concat(policy.Version.Value, ":", program),
            Reason = reason,
            CorrelationId = correlationId,
            DataJson = JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["snapshot_hash"] = policy.ComputeHash(),
                ["approval"] = policy.Approval.ToString(),
                ["allowed_execution_modes"] = modeValues,
            }),
            CreatedAt = now,
        });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string ToStorageValue(ExecutionMode mode) => mode switch
    {
        ExecutionMode.Mock => "MOCK",
        ExecutionMode.LabRealSim => "LAB_REAL_SIM",
        ExecutionMode.ProductionReal => "PRODUCTION_REAL",
        _ => throw new InvalidOperationException("Unknown execution mode."),
    };
}
