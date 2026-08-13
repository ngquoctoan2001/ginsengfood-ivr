using System.Data;
using System.Data.Common;
using System.Text.Json;
using Ivr.Domain.Retention;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ivr.Infrastructure.Retention;

/// <summary>
/// Executes child-first, short-transaction retention batches against trusted catalog targets.
/// </summary>
public sealed class RetentionJob(
    IDbContextFactory<IvrDbContext> dbContextFactory,
    IRetentionPolicyProvider policyProvider,
    IRetentionTelemetry telemetry,
    TimeProvider timeProvider) : IRetentionJob
{
    private const string PolicyCheckpointSegment = "__policy__";

    public async Task<RetentionRunReport> RunAsync(
        RetentionRunOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.BatchSize is < 1 or > 10_000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "Retention batch size must be between 1 and 10,000.");
        }

        DateTimeOffset startedAt = options.Now ?? timeProvider.GetUtcNow();
        Guid runId = Guid.NewGuid();
        IReadOnlyList<string> dataClasses = ResolveDataClasses(options.DataClasses);
        var reports = new List<RetentionClassReport>(dataClasses.Count);

        try
        {
            foreach (string dataClass in dataClasses)
            {
                cancellationToken.ThrowIfCancellationRequested();
                reports.Add(await RunClassAsync(
                    runId,
                    dataClass,
                    options,
                    startedAt,
                    cancellationToken));
            }

            DateTimeOffset completedAt = options.Now ?? timeProvider.GetUtcNow();
            var report = new RetentionRunReport(
                runId,
                options.DryRun,
                startedAt,
                completedAt,
                reports);
            await AppendAuditAsync(report, cancellationToken);
            return report;
        }
        catch
        {
            telemetry.RecordRunFailed();
            throw;
        }
    }

    private async Task<RetentionClassReport> RunClassAsync(
        Guid runId,
        string dataClass,
        RetentionRunOptions options,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        RetentionStrategy strategy = RetentionTargetCatalog.GetStrategy(dataClass);
        TimeSpan? period = await policyProvider.GetPeriodAsync(dataClass, cancellationToken);
        if (period is null)
        {
            DateTimeOffset firstSeen = await RecordCheckpointAsync(
                runId,
                dataClass,
                PolicyCheckpointSegment,
                "NOT_CONFIGURED",
                0,
                now,
                firstNotConfiguredAt: now,
                cancellationToken);
            telemetry.RecordNotConfigured(dataClass, (now - firstSeen).TotalDays);
            return new RetentionClassReport(
                dataClass,
                strategy,
                RetentionClassRunStatus.NotConfigured,
                0,
                0,
                0,
                0,
                0,
                0,
                (now - firstSeen).TotalDays);
        }

        DateTimeOffset cutoff = now.Subtract(period.Value);
        long examined = 0;
        long deleted = 0;
        long anonymized = 0;
        long legalHold = 0;
        long protectedRows = 0;
        long dependencyBlocked = 0;

        foreach (RetentionTarget target in RetentionTargetCatalog.Get(dataClass))
        {
            RetentionInspection inspection = await InspectAsync(
                target,
                cutoff,
                now,
                cancellationToken);
            examined += inspection.Examined;
            legalHold += inspection.LegalHold;
            protectedRows += inspection.Protected;
            dependencyBlocked += inspection.DependencyBlocked;

            if (options.DryRun)
            {
                continue;
            }

            long affected = await MutateTargetAsync(
                runId,
                dataClass,
                target,
                cutoff,
                now,
                options.BatchSize,
                cancellationToken);
            if (strategy == RetentionStrategy.Delete)
            {
                deleted += affected;
            }
            else
            {
                anonymized += affected;
            }
        }

        long affectedRows = deleted + anonymized;
        telemetry.RecordClassCompleted(dataClass, affectedRows);
        return new RetentionClassReport(
            dataClass,
            strategy,
            options.DryRun
                ? RetentionClassRunStatus.DryRun
                : RetentionClassRunStatus.Completed,
            examined,
            deleted,
            anonymized,
            legalHold,
            protectedRows,
            dependencyBlocked);
    }

    private async Task<RetentionInspection> InspectAsync(
        RetentionTarget target,
        DateTimeOffset cutoff,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        DbConnection connection = dbContext.Database.GetDbConnection();
        string expired = BuildExpiredSql(target);
        string legalSafe = "(t.legal_hold_until IS NULL OR t.legal_hold_until <= @now)";
        string notProtected = target.ProtectedSql is null
            ? "TRUE"
            : $"NOT ({target.ProtectedSql})";
        string notBlocked = target.DependencyBlockedSql is null
            ? "TRUE"
            : $"NOT ({target.DependencyBlockedSql})";

        long actionable = await ExecuteCountAsync(
            connection,
            $"SELECT COUNT(*) FROM {target.TableName} AS t "
            + $"WHERE {expired} AND {legalSafe} AND {notProtected} AND {notBlocked}",
            cutoff,
            now,
            cancellationToken);
        long held = await ExecuteCountAsync(
            connection,
            $"SELECT COUNT(*) FROM {target.TableName} AS t "
            + $"WHERE {expired} AND t.legal_hold_until > @now",
            cutoff,
            now,
            cancellationToken);
        long protectedRows = target.ProtectedSql is null
            ? 0
            : await ExecuteCountAsync(
                connection,
                $"SELECT COUNT(*) FROM {target.TableName} AS t "
                + $"WHERE {expired} AND {legalSafe} AND ({target.ProtectedSql})",
                cutoff,
                now,
                cancellationToken);
        long blocked = target.DependencyBlockedSql is null
            ? 0
            : await ExecuteCountAsync(
                connection,
                $"SELECT COUNT(*) FROM {target.TableName} AS t "
                + $"WHERE {expired} AND {legalSafe} AND {notProtected} "
                + $"AND ({target.DependencyBlockedSql})",
                cutoff,
                now,
                cancellationToken);

        return new RetentionInspection(
            actionable + held + protectedRows + blocked,
            held,
            protectedRows,
            blocked);
    }

    private async Task<long> MutateTargetAsync(
        Guid runId,
        string dataClass,
        RetentionTarget target,
        DateTimeOffset cutoff,
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken)
    {
        long affectedTotal = 0;
        int affected;
        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
                cancellationToken);
            await using IDbContextTransaction transaction = await dbContext.Database
                .BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            DbConnection connection = dbContext.Database.GetDbConnection();
            string mutationSql = BuildMutationSql(target);
            await using DbCommand command = connection.CreateCommand();
            command.Transaction = transaction.GetDbTransaction();
            command.CommandText = mutationSql;
            AddParameter(command, "@cutoff", DbType.DateTimeOffset, cutoff);
            AddParameter(command, "@now", DbType.DateTimeOffset, now);
            AddParameter(command, "@batch_size", DbType.Int32, batchSize);
            affected = await command.ExecuteNonQueryAsync(cancellationToken);

            await UpsertCheckpointAsync(
                connection,
                transaction.GetDbTransaction(),
                runId,
                dataClass,
                target.Segment,
                affected < batchSize ? "COMPLETED" : "IN_PROGRESS",
                affected,
                now,
                null,
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            affectedTotal += affected;
            await telemetry.BatchCommittedAsync(
                dataClass,
                target.Segment,
                affected,
                CancellationToken.None);
            cancellationToken.ThrowIfCancellationRequested();
        }
        while (affected == batchSize);

        return affectedTotal;
    }

    private async Task<DateTimeOffset> RecordCheckpointAsync(
        Guid runId,
        string dataClass,
        string segment,
        string status,
        long processedCount,
        DateTimeOffset now,
        DateTimeOffset? firstNotConfiguredAt,
        CancellationToken cancellationToken)
    {
        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        await using IDbContextTransaction transaction = await dbContext.Database
            .BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        DateTimeOffset firstSeen = await UpsertCheckpointAsync(
            dbContext.Database.GetDbConnection(),
            transaction.GetDbTransaction(),
            runId,
            dataClass,
            segment,
            status,
            processedCount,
            now,
            firstNotConfiguredAt,
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return firstSeen;
    }

    private async Task AppendAuditAsync(
        RetentionRunReport report,
        CancellationToken cancellationToken)
    {
        await using IvrDbContext dbContext = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);
        string correlationId = $"retention-{report.RunId:N}";
        dbContext.AuditLog.Add(new AuditLogEntity
        {
            AuditId = Guid.NewGuid(),
            ActorId = "ivr-retention-job",
            ActorType = "service",
            Action = report.DryRun ? "retention.dry_run" : "retention.execute",
            TargetType = "retention_run",
            TargetId = report.RunId.ToString("N"),
            Reason = "P1-5 configured data lifecycle",
            CorrelationId = correlationId,
            DataJson = JsonSerializer.Serialize(new
            {
                report.RunId,
                report.DryRun,
                report.StartedAt,
                report.CompletedAt,
                Classes = report.Classes.Select(item => new
                {
                    item.DataClass,
                    Strategy = item.Strategy.ToString(),
                    Status = item.Status.ToString(),
                    item.ExaminedCount,
                    item.DeletedCount,
                    item.AnonymizedCount,
                    item.LegalHoldCount,
                    item.ProtectedCount,
                    item.DependencyBlockedCount,
                    item.NotConfiguredAgeDays,
                }),
            }),
            CreatedAt = report.CompletedAt,
            RetentionClass = "audit_log",
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<string> ResolveDataClasses(
        IReadOnlyCollection<string>? requested)
    {
        if (requested is null || requested.Count == 0)
        {
            return RetentionDataClasses.DefaultExecutionOrder;
        }

        var requestedSet = requested.ToHashSet(StringComparer.Ordinal);
        string? unknown = requestedSet.FirstOrDefault(
            item => !RetentionDataClasses.All.Contains(item));
        if (unknown is not null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requested),
                unknown,
                "Unknown retention data class.");
        }

        return RetentionDataClasses.DefaultExecutionOrder
            .Where(requestedSet.Contains)
            .ToArray();
    }

    private static string BuildExpiredSql(RetentionTarget target)
    {
        string sql = $"t.{target.TimestampColumn} IS NOT NULL "
            + $"AND t.{target.TimestampColumn} <= @cutoff "
            + "AND (t.retain_until IS NULL OR t.retain_until <= @now)";
        if (target.Strategy == RetentionStrategy.Anonymize)
        {
            sql += " AND t.anonymized_at IS NULL";
        }

        if (target.ExtraEligibilitySql is not null)
        {
            sql += $" AND ({target.ExtraEligibilitySql})";
        }

        return sql;
    }

    private static string BuildMutationSql(RetentionTarget target)
    {
        string expired = BuildExpiredSql(target);
        string protectedFilter = target.ProtectedSql is null
            ? string.Empty
            : $" AND NOT ({target.ProtectedSql})";
        string dependencyFilter = target.DependencyBlockedSql is null
            ? string.Empty
            : $" AND NOT ({target.DependencyBlockedSql})";
        string candidates = $"SELECT t.ctid FROM {target.TableName} AS t "
            + $"WHERE {expired} "
            + "AND (t.legal_hold_until IS NULL OR t.legal_hold_until <= @now)"
            + protectedFilter
            + dependencyFilter
            + $" ORDER BY t.{target.TimestampColumn}, t.ctid "
            + "LIMIT @batch_size FOR UPDATE SKIP LOCKED";

        if (target.Strategy == RetentionStrategy.Delete)
        {
            return $"WITH candidates AS ({candidates}) "
                + $"DELETE FROM {target.TableName} AS target USING candidates "
                + "WHERE target.ctid = candidates.ctid";
        }

        return $"WITH candidates AS ({candidates}) "
            + $"UPDATE {target.TableName} AS target SET {target.AnonymizeSetSql}, "
            + "anonymized_at = @now FROM candidates "
            + "WHERE target.ctid = candidates.ctid";
    }

    private static async Task<long> ExecuteCountAsync(
        DbConnection connection,
        string sql,
        DateTimeOffset cutoff,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameter(command, "@cutoff", DbType.DateTimeOffset, cutoff);
        AddParameter(command, "@now", DbType.DateTimeOffset, now);
        object? value = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static async Task<DateTimeOffset> UpsertCheckpointAsync(
        DbConnection connection,
        DbTransaction transaction,
        Guid runId,
        string dataClass,
        string segment,
        string status,
        long processedCount,
        DateTimeOffset now,
        DateTimeOffset? firstNotConfiguredAt,
        CancellationToken cancellationToken)
    {
        await using DbCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            "INSERT INTO ivr_retention_checkpoints "
            + "(data_class, segment, run_id, status, processed_count, "
            + "first_not_configured_at, last_run_at, updated_at) "
            + "VALUES (@data_class, @segment, @run_id, @status, @processed_count, "
            + "@first_not_configured_at, @now, @now) "
            + "ON CONFLICT (data_class, segment) DO UPDATE SET "
            + "run_id = EXCLUDED.run_id, status = EXCLUDED.status, "
            + "processed_count = CASE "
            + "WHEN ivr_retention_checkpoints.run_id = EXCLUDED.run_id "
            + "THEN ivr_retention_checkpoints.processed_count + EXCLUDED.processed_count "
            + "ELSE EXCLUDED.processed_count END, "
            + "first_not_configured_at = CASE "
            + "WHEN EXCLUDED.status = 'NOT_CONFIGURED' "
            + "THEN COALESCE(ivr_retention_checkpoints.first_not_configured_at, "
            + "EXCLUDED.first_not_configured_at) ELSE NULL END, "
            + "last_run_at = EXCLUDED.last_run_at, updated_at = EXCLUDED.updated_at "
            + "RETURNING COALESCE(first_not_configured_at, @now)";
        AddParameter(command, "@data_class", DbType.String, dataClass);
        AddParameter(command, "@segment", DbType.String, segment);
        AddParameter(command, "@run_id", DbType.Guid, runId);
        AddParameter(command, "@status", DbType.String, status);
        AddParameter(command, "@processed_count", DbType.Int64, processedCount);
        AddParameter(
            command,
            "@first_not_configured_at",
            DbType.DateTimeOffset,
            firstNotConfiguredAt ?? (object)DBNull.Value);
        AddParameter(command, "@now", DbType.DateTimeOffset, now);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result is DateTimeOffset value ? value : now;
    }

    private static void AddParameter(
        DbCommand command,
        string name,
        DbType type,
        object value)
    {
        DbParameter parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = type;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private sealed record RetentionInspection(
        long Examined,
        long LegalHold,
        long Protected,
        long DependencyBlocked);
}
