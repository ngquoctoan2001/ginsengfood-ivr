using System.Globalization;
using System.Diagnostics;
using System.Text.Json;
using Ivr.Dsar;
using Ivr.Domain.Retention;
using Ivr.Infrastructure.Analytics;
using Ivr.Infrastructure.Audit;
using Ivr.Infrastructure.Governance;
using Ivr.Infrastructure.Persistence;
using Ivr.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Ivr.IntegrationTests.Governance;

/// <summary>
/// W-0052 / P10-1 §8 — <c>COMP-DSAR-02</c> and <c>COMP-RETENTION-04</c> against real
/// PostgreSQL, because both properties are enforced by the database rather than by
/// the code that talks to it: audit immutability is a trigger, and a retention pass
/// is a transaction.
/// </summary>
[Collection(PostgresPersistenceTestGroup.Name)]
public sealed class ComplianceTests(PostgresPersistenceFixture fixture)
{
    private const string OrderCode = "GF-ORDER-DSAR-001";
    private const string OtherOrderCode = "GF-ORDER-DSAR-002";

    private static readonly DateTimeOffset Now = new(2026, 8, 14, 9, 30, 0, TimeSpan.Zero);

    [Fact]
    [Trait("TestId", "COMP-DSAR-13")]
    public async Task AnAuditFailureRollsBackTheErasure()
    {
        await fixture.ResetAsync();
        await SeedAsync(withNumber: true);
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        await context.Database.ExecuteSqlRawAsync("""
            CREATE FUNCTION reject_dsar_test_audit() RETURNS trigger LANGUAGE plpgsql AS
            $$ BEGIN IF NEW.action = 'IVR_DSAR_ERASE' THEN RAISE EXCEPTION 'test audit failure'; END IF;
            RETURN NEW; END $$;
            CREATE TRIGGER reject_dsar_test_audit BEFORE INSERT ON ivr_audit_log
            FOR EACH ROW EXECUTE FUNCTION reject_dsar_test_audit();
            """);
        try
        {
            await Assert.ThrowsAnyAsync<Exception>(() => Service().EraseAsync(
                OrderCode, "verified request DSAR-TEST", "operator-test", "dsar-rollback",
                dryRun: false, CancellationToken.None));
            ConfirmationTaskEntity task = await context.ConfirmationTasks.AsNoTracking()
                .SingleAsync(row => row.OrderCode == OrderCode);
            Assert.Null(task.AnonymizedAt);
            Assert.NotNull(task.PhoneE164);
            Assert.NotEqual("redacted", task.PhoneRef);
            Assert.Empty(await context.AuditLog.Where(row => row.Action == DsarService.EraseAuditAction).ToListAsync());
        }
        finally
        {
            await context.Database.ExecuteSqlRawAsync("""
                DROP TRIGGER reject_dsar_test_audit ON ivr_audit_log;
                DROP FUNCTION reject_dsar_test_audit();
                """);
        }
    }

    [Fact]
    [Trait("TestId", "COMP-DSAR-17")]
    public async Task TheCliProcessPreviewsThenErasesOnlyTheConfirmedOrder()
    {
        await fixture.ResetAsync();
        await SeedAsync(withNumber: true);
        string directory = CreateCliSandbox();
        try
        {
            CliResult identityRun = await RunCliAsync(directory, "--identity");
            Assert.Equal(0, identityRun.ExitCode);
            using JsonDocument identityDocument = JsonDocument.Parse(identityRun.Output);
            string identity = identityDocument.RootElement.GetProperty("CurrentIdentity").GetString()!;
            string policyFile = Path.Combine(directory, "dsar-operator.json");
            string[] previewArgs = ["--order-code", OrderCode, "--request-ref", "DSAR-CLI-CASE"];

            CliResult unconfigured = await RunCliAsync(directory, previewArgs);
            Assert.Equal(2, unconfigured.ExitCode);
            await File.WriteAllTextAsync(policyFile, JsonSerializer.Serialize(new DsarOperatorPolicy(1, "someone-else")));
            CliResult denied = await RunCliAsync(directory, previewArgs);
            Assert.Equal(3, denied.ExitCode);
            await using IvrDbContext context = await Factory().CreateDbContextAsync();
            Assert.Empty(await context.AuditLog.Where(row => row.Action == DsarService.EraseAuditAction).ToListAsync());

            await File.WriteAllTextAsync(policyFile, JsonSerializer.Serialize(new DsarOperatorPolicy(1, identity)));
            CliResult preview = await RunCliAsync(directory, previewArgs);
            Assert.Equal(0, preview.ExitCode);
            DsarCommandResult previewReport = JsonSerializer.Deserialize<DsarCommandResult>(preview.Output)!;
            Assert.Equal("PREVIEW", previewReport.Mode);
            Assert.True(previewReport.Erasure.DryRun);
            Assert.Equal(1, previewReport.Erasure.TasksMatched);
            Assert.Equal(0, previewReport.Erasure.TasksRedacted);
            ConfirmationTaskEntity before = await context.ConfirmationTasks.AsNoTracking().SingleAsync(row => row.OrderCode == OrderCode);
            Assert.NotNull(before.PhoneE164);
            Assert.DoesNotContain(before.PhoneE164, preview.Output, StringComparison.Ordinal);

            string[] executeArgs = [.. previewArgs, "--execute", "--confirm-order", OrderCode, "--subject-verified"];
            CliResult executed = await RunCliAsync(directory, executeArgs);
            Assert.Equal(0, executed.ExitCode);
            DsarCommandResult result = JsonSerializer.Deserialize<DsarCommandResult>(executed.Output)!;
            Assert.Equal("EXECUTED", result.Mode);
            Assert.Equal(1, result.Erasure.TasksRedacted);
            Assert.False(result.Erasure.DryRun);
            ConfirmationTaskEntity after = await context.ConfirmationTasks.AsNoTracking().SingleAsync(row => row.OrderCode == OrderCode);
            Assert.Null(after.PhoneE164);
            Assert.NotNull(after.AnonymizedAt);
            Assert.NotNull((await context.ConfirmationTasks.AsNoTracking().SingleAsync(row => row.OrderCode == OtherOrderCode)).PhoneE164);

            CliResult repeated = await RunCliAsync(directory, executeArgs);
            Assert.Equal(0, repeated.ExitCode);
            Assert.Equal(0, JsonSerializer.Deserialize<DsarCommandResult>(repeated.Output)!.Erasure.TasksMatched);
            var audits = await context.AuditLog.Where(row => row.Action == DsarService.EraseAuditAction).ToListAsync();
            Assert.Equal(3, audits.Count);
            Assert.All(audits, row => Assert.Equal($"operator:{identity}", row.ActorId));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string CreateCliSandbox()
    {
        string directory = Path.Combine(Path.GetTempPath(), "ivr-dsar-cli-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        // Project references copy executable runtime/deps files alongside the test assembly.
        foreach (string file in Directory.GetFiles(AppContext.BaseDirectory))
        {
            File.Copy(file, Path.Combine(directory, Path.GetFileName(file)));
        }

        return directory;
    }

    private async Task<CliResult> RunCliAsync(string directory, params string[] arguments)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = directory,
        };
        start.ArgumentList.Add(Path.Combine(directory, "Ivr.Dsar.dll"));
        foreach (string argument in arguments) start.ArgumentList.Add(argument);
        start.Environment["IVR_DSAR_CONNECTION_STRING"] = fixture.ConnectionString;
        using var process = new Process { StartInfo = start };
        Assert.True(process.Start());
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            throw;
        }

        string stderr = await error;
        Assert.DoesNotContain("ivr-test-password", stderr, StringComparison.Ordinal);
        return new CliResult(process.ExitCode, await output, stderr);
    }

    private sealed record CliResult(int ExitCode, string Output, string Error);

    // ------------------------------------------------------------- COMP-DSAR-02

    [Fact]
    [Trait("TestId", "COMP-DSAR-02")]
    public async Task FindReportsCountsAndTheLimitsBeforeAnythingIsPromised()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        DsarFindReport report = await Service().FindAsync(OrderCode, CancellationToken.None);

        Assert.True(report.Found);
        Assert.Equal(1, Rows(report, "ivr_confirmation_tasks"));
        Assert.Equal(1, Rows(report, "ivr_call_jobs"));
        Assert.Equal(1, Rows(report, "ivr_call_results"));

        // Counts, never values. A service that printed the stored personal data would be a new
        // way to read it, available to whoever can call the service.
        Assert.All(report.Holdings, holding => Assert.False(
            holding.Table.Contains("phone", StringComparison.OrdinalIgnoreCase)));

        // The limits are part of the answer, not a discovery made while answering. Four since
        // W-0314: customer_id is kept as the key IVR shares with Sales, and the requester hears so.
        Assert.Equal(4, report.NotErasable.Count);
        Assert.Contains(report.NotErasable, limit =>
            limit.Contains("append-only", StringComparison.Ordinal));
    }

    [Fact]
    [Trait("TestId", "COMP-DSAR-02")]
    public async Task AnUnknownOrderIsAnAnsweredRequestNotAnError()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        DsarFindReport report = await Service()
            .FindAsync("GF-ORDER-NOT-HERE", CancellationToken.None);

        Assert.False(report.Found);
        Assert.Empty(report.Holdings);
        // Still returns the limits: "we hold nothing" is an answer that has to be as complete as
        // any other, or the next request asks the same question again.
        Assert.NotEmpty(report.NotErasable);
    }

    [Fact]
    [Trait("TestId", "COMP-DSAR-02")]
    public async Task ADryRunChangesNothingAndIsStillAudited()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        DsarErasureReport report = await Service().EraseAsync(
            OrderCode,
            "subject erasure request 2026-08-19",
            "AGT-PRIVACY-01",
            "corr-dsar-dry",
            dryRun: true,
            CancellationToken.None);

        Assert.True(report.DryRun);
        Assert.Equal(0, report.TasksRedacted);

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        ConfirmationTaskEntity task = await context.ConfirmationTasks
            .SingleAsync(row => row.OrderCode == OrderCode);
        Assert.NotEqual("redacted", task.PhoneRef);
        Assert.Null(task.AnonymizedAt);

        // Audited anyway. A request that changed nothing is still a request that was answered,
        // and the answer has to be as durable as the erasure would have been.
        Assert.Single(await context.AuditLog
            .Where(entry => entry.Action == DsarService.EraseAuditAction)
            .ToListAsync());
    }

    [Fact]
    [Trait("TestId", "COMP-DSAR-02")]
    public async Task ErasureRedactsTheSubjectAndLeavesEveryOtherOrderAlone()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        DsarErasureReport report = await Service().EraseAsync(
            OrderCode,
            "subject erasure request 2026-08-19",
            "AGT-PRIVACY-01",
            "corr-dsar-real",
            dryRun: false,
            CancellationToken.None);

        Assert.False(report.DryRun);
        Assert.Equal(1, report.TasksRedacted);
        Assert.False(string.IsNullOrWhiteSpace(report.AuditRef));

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        ConfirmationTaskEntity erased = await context.ConfirmationTasks
            .SingleAsync(row => row.OrderCode == OrderCode);

        Assert.Equal("redacted", erased.PhoneRef);
        Assert.Equal("***", erased.PhoneMasked);
        Assert.Equal("REDACTED", erased.PhoneValidationStatus);
        Assert.Equal("enc:redacted", erased.DialTokenCiphertext);
        Assert.Equal("{}", erased.PrivacySafeOrderSummaryJson);
        Assert.NotNull(erased.AnonymizedAt);

        // The key the request arrived with survives. Erasing it would make every later request
        // about this order unanswerable, including the subject's own.
        Assert.Equal(OrderCode, erased.OrderCode);

        // Blast radius: exactly one order. A DSAR erasure that reached a second customer would be
        // a breach committed while honouring a privacy request.
        ConfirmationTaskEntity untouched = await context.ConfirmationTasks
            .SingleAsync(row => row.OrderCode == OtherOrderCode);
        Assert.NotEqual("redacted", untouched.PhoneRef);
        Assert.Null(untouched.AnonymizedAt);
    }

    [Fact]
    [Trait("TestId", "COMP-DSAR-02")]
    public async Task ErasureLeavesTheAuditTrailAndTheDeliveryRecordIntact()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        await using (IvrDbContext before = await Factory().CreateDbContextAsync())
        {
            Assert.Equal(1, await before.ResultCallbacks.CountAsync());
        }

        await Service().EraseAsync(
            OrderCode,
            "subject erasure request 2026-08-19",
            "AGT-PRIVACY-01",
            "corr-dsar-audit",
            dryRun: false,
            CancellationToken.None);

        await using IvrDbContext context = await Factory().CreateDbContextAsync();

        // The delivery record keeps its payload. Removing it leaves a record that cannot settle
        // the dispute it exists for; it expires with retention instead.
        ResultCallbackEntity callback = await context.ResultCallbacks.SingleAsync();
        Assert.Contains("order", callback.PayloadJson, StringComparison.OrdinalIgnoreCase);

        // And the audit trail gained a row rather than losing one.
        AuditLogEntity dsarEntry = await context.AuditLog
            .SingleAsync(entry => entry.Action == DsarService.EraseAuditAction);
        Assert.Equal("AGT-PRIVACY-01", dsarEntry.ActorId);
        Assert.Contains("erasure request", dsarEntry.Reason!, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "COMP-DSAR-02")]
    public async Task AnErasureWithoutARealReasonIsRefused()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        // The reason ends up in the audit row, and "ok" in that field is the same as no record.
        await Assert.ThrowsAsync<ArgumentException>(() => Service().EraseAsync(
            OrderCode,
            "ok",
            "AGT-PRIVACY-01",
            "corr-dsar-thin",
            dryRun: false,
            CancellationToken.None));

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        ConfirmationTaskEntity task = await context.ConfirmationTasks
            .SingleAsync(row => row.OrderCode == OrderCode);
        Assert.NotEqual("redacted", task.PhoneRef);
    }

    // -------------------------------------------------------- COMP-RETENTION-04

    [Fact]
    [Trait("TestId", "COMP-RETENTION-04")]
    public async Task EveryRetentionClassInTheCatalogIsClassifiedInTheGovernanceMap()
    {
        // The compliance claim is that the retention job enforces the policy the inventory
        // describes. That only holds if the two vocabularies line up: a data class the job
        // executes but nobody classified is a deletion happening under no stated policy.
        await Task.CompletedTask;

        foreach (string dataClass in RetentionDataClasses.All)
        {
            Assert.True(
                DataClassification.GovernedRetentionClasses.Contains(dataClass),
                $"the retention job executes '{dataClass}' but no table declares it.");
        }
    }

    [Fact]
    [Trait("TestId", "COMP-RETENTION-04")]
    public async Task EveryTableHoldingPersonalDataHasARetentionClassThatActuallyRemovesIt()
    {
        await Task.CompletedTask;

        // PiiDirect and PiiDerived tables must map to a class the P1-5 job executes. The two
        // exceptions are audit tables, which are PRESERVE on purpose and say so in the inventory.
        foreach ((string table, DataClassEntry entry) in DataClassification.Tables)
        {
            if (entry.Protection is not (DataProtectionClass.PiiDirect or DataProtectionClass.PiiDerived))
            {
                continue;
            }

            bool executed = RetentionDataClasses.All.Contains(entry.RetentionClass)
                || string.Equals(entry.RetentionClass, "analytics_derived", StringComparison.Ordinal);

            Assert.True(
                executed,
                $"{table} holds personal data under retention class '{entry.RetentionClass}', "
                + "which the P1-5 job never executes.");
        }
    }

    [Fact]
    [Trait("TestId", "COMP-RETENTION-04")]
    public async Task DerivedAnalyticsCopiesInheritTheirPeriodFromTheSourceRatherThanOwningOne()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        var hook = new AnalyticsRetentionHook(Factory(), TimeProvider.System);

        await using (IvrDbContext seed = await Factory().CreateDbContextAsync())
        {
            // A fact whose source result is already gone: exactly the state a retention pass on
            // the operational tables leaves behind.
            seed.AnalyticsFacts.Add(new Ivr.Infrastructure.Analytics.AnalyticsFactCallOutcomeEntity
            {
                IvrCallResultId = "RESULT-DSAR-GONE",
                IvrCallJobId = "JOB-DSAR-01",
                OrderRefHash = new string('b', 64),
                ProgramKey = "GOLDEN_HOUR",
                ScriptVariantKey = "SCRIPT-ORDER-CONFIRM:vA",
                ResultTypeKey = "IVR_CONFIRMED",
                FinalResultStatus = "IVR_CONFIRMED",
                IsFinal = true,
                IsCountedCustomerAttempt = true,
                CountedAttemptNumber = 1,
                EventAt = Now,
                EventDate = DateOnly.FromDateTime(Now.UtcDateTime),
                EventHour = Now.Hour,
                SecondsToResult = 60,
                LoadedAt = Now,
            });
            await seed.SaveChangesAsync();
        }

        int deleted = await hook.PurgeExpiredAsync(Now, dryRun: false, CancellationToken.None);

        // The warehouse period equals the source period by construction: there is no second
        // period to configure, so there is no way to configure the two inconsistently.
        Assert.Equal(1, deleted);
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        Assert.Equal(0, await context.AnalyticsFacts
            .CountAsync(fact => fact.IvrCallResultId == "RESULT-DSAR-GONE"));
    }

    /// <summary>
    /// The two numbers the audit row carries about one erasure must describe one instant.
    /// <para>
    /// They were taken from two statements: a COUNT, then the UPDATE. A task arriving or leaving
    /// between them left the permanent record saying one number was found and a different number
    /// changed — about the same erasure, for good, in the one artefact a data subject or a
    /// regulator would be shown. The UPDATE's own row count is now the match count, because its
    /// only predicate is the order code: every task it matched is a task it redacted.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "COMP-DSAR-05")]
    public async Task TheAuditRowsMatchedAndRedactedCountsDescribeOneInstant()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        await Service().EraseAsync(
            OrderCode,
            "subject erasure request 2026-09-09",
            "AGT-PRIVACY-01",
            "corr-dsar-consistency",
            dryRun: false,
            CancellationToken.None);

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        AuditLogEntity entry = await context.AuditLog.AsNoTracking()
            .SingleAsync(row => row.Action == DsarService.EraseAuditAction);

        using System.Text.Json.JsonDocument data =
            System.Text.Json.JsonDocument.Parse(entry.DataJson);
        int matched = data.RootElement.GetProperty("tasks_matched").GetInt32();
        int redacted = data.RootElement.GetProperty("tasks_redacted").GetInt32();

        Assert.Equal(1, redacted);
        Assert.Equal(redacted, matched);
        Assert.False(data.RootElement.GetProperty("dry_run").GetBoolean());
    }

    /// <summary>
    /// The find report must stay correct when an order carries more history than a handful of rows.
    /// <para>
    /// It used to pull every order id and job id into the process and send them back inside
    /// <c>IN (...)</c>, one parameter per row against PostgreSQL's limit of 65,535 — so a
    /// long-running order eventually became unanswerable, and the failure would surface as a
    /// driver error in the middle of a subject-access request. Thirty jobs does not reach that
    /// limit and is not meant to: it proves the subquery rewrite still counts the same things.
    /// The bound itself is structural and is held by
    /// <see cref="TheFindReportNeverMaterialisesAnIdentifierList"/>.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "COMP-DSAR-06")]
    public async Task TheFindReportIsCorrectForAnOrderWithManyJobs()
    {
        await fixture.ResetAsync();
        await SeedAsync();

        await using (IvrDbContext seeding = await Factory().CreateDbContextAsync())
        {
            ConfirmationTaskEntity task = await seeding.ConfirmationTasks
                .AsNoTracking()
                .SingleAsync(row => row.OrderCode == OrderCode);

            for (int index = 0; index < 30; index++)
            {
                seeding.CallJobs.Add(new CallJobEntity
                {
                    IvrCallJobId = $"JOB-DSAR-BULK-{index:D3}",
                    TaskId = task.TaskId,
                    OfficialOrderId = task.OfficialOrderId,
                    OrderVersionSnapshot = task.OrderVersion,
                    ProgramType = task.ProgramType,
                    AttemptPolicyCode = task.AttemptPolicyVersion,
                    Status = "CLOSED",
                    MaxAttempts = task.MaxAttempts,
                    AttemptOffsetsSecondsJson = task.AttemptOffsetsSecondsJson,
                    ConfirmationWindowSeconds = 900,
                    AttemptScheduleJson = "[]",
                    T0At = task.ConfirmationWindowStartedAt,
                    ExpiresAt = task.ConfirmationWindowExpiresAt,
                    Eligible = true,
                    EligibilityDecision = "ELIGIBLE_FOR_IVR",
                    QueueStatus = "HELD_MOCK",
                    ScriptVersion = "SCRIPT-ORDER-CONFIRM:vA",
                    PrivacyPolicyVersion = "privacy-v1",
                    InputSignalOnly = true,
                    NoDirectOrderUpdate = true,
                    CreatedAt = task.ConfirmationWindowStartedAt,
                    ClosedAt = Now,
                });
            }

            await seeding.SaveChangesAsync();
        }

        DsarFindReport report = await Service().FindAsync(OrderCode, CancellationToken.None);

        Assert.True(report.Found);
        Assert.Equal(1, Rows(report, "ivr_confirmation_tasks"));

        // The one seeded by SeedAsync plus the thirty above.
        Assert.Equal(31, Rows(report, "ivr_call_jobs"));
    }

    /// <summary>
    /// Asserted against the source, because the property is structural: the parameter ceiling is
    /// only reached with tens of thousands of rows, which no test is going to seed. What can be
    /// checked is that the identifier sets never leave the database — a reintroduced
    /// <c>ToArrayAsync</c> is the exact edit that would put them back on the wire.
    /// </summary>
    [Fact]
    [Trait("TestId", "COMP-DSAR-07")]
    public void TheFindReportNeverMaterialisesAnIdentifierList()
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src",
            "Ivr.Infrastructure",
            "Governance",
            "DsarService.cs"));

        int start = source.IndexOf("public async Task<DsarFindReport> FindAsync", StringComparison.Ordinal);
        Assert.True(start >= 0, "FindAsync was renamed; this guard no longer reads it.");
        int end = source.IndexOf("public async Task<DsarErasureReport> EraseAsync", StringComparison.Ordinal);
        Assert.True(end > start, "EraseAsync no longer follows FindAsync; this guard reads the wrong span.");

        string findAsync = source[start..end];
        foreach (string materialiser in new[] { "ToArrayAsync", "ToListAsync" })
        {
            Assert.False(
                findAsync.Contains(materialiser, StringComparison.Ordinal),
                $"DsarService.FindAsync calls {materialiser}: an identifier set pulled into the "
                + "process goes back to PostgreSQL as one parameter per row, and the statement "
                + "limit is 65,535.");
        }
    }

    // ------------------------------------------------------ W-0314 · COMP-DSAR-08..12

    /// <summary>
    /// W-0314. An erasure removes the number itself, and the Sales contact keys with it.
    /// <para>
    /// The inventory promised this for <c>phone_e164</c> from the day the column arrived (W-0310),
    /// and the shared redaction never touched it: the row came out stamped <c>anonymized_at</c>,
    /// looking erased, with the customer's number in the clear. The contact keys were erased by
    /// nothing at all once S3 removed the retention period that was supposed to take them.
    /// <c>customer_id</c> stays, like <c>order_code</c>, and the requester is told before anything
    /// starts.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "COMP-DSAR-08")]
    public async Task ErasureRemovesTheNumberAndTheContactKeysAndKeepsTheReconciliationKey()
    {
        await fixture.ResetAsync();
        await SeedAsync(withNumber: true);
        Assert.Equal(1, await RowsHoldingAReadableNumberAsync(OrderCode));

        await Erase("corr-dsar-number", dryRun: false);

        Assert.Equal(0, await RowsHoldingAReadableNumberAsync(OrderCode));

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        ConfirmationTaskEntity erased = await context.ConfirmationTasks
            .SingleAsync(row => row.OrderCode == OrderCode);
        Assert.Null(erased.PhoneE164);
        Assert.Null(erased.OfficialContactId);
        Assert.Null(erased.CustomerTrustStatus);
        Assert.Null(erased.TrustedSkipAllowed);
        Assert.NotNull(erased.AnonymizedAt);
        Assert.Equal("CUST-DSAR-01", erased.CustomerId);

        // Blast radius, again: the other customer's number is exactly where it was.
        Assert.Equal(1, await RowsHoldingAReadableNumberAsync(OtherOrderCode));
    }

    /// <summary>
    /// W-0314. A second request about the same order reaches the task that arrived after the first.
    /// <para>
    /// The trigger lets <c>anonymized_at</c> be set once, and the erasure matched every task of the
    /// order, erased or not. So a second request re-stamped the first task, the trigger refused, and
    /// the whole statement rolled back: a task Sales sent after the first erasure could never be
    /// erased at all. The dry run counts the same way, or its preview promises what the real run
    /// will not reach.
    /// </para>
    /// </summary>
    [Fact]
    [Trait("TestId", "COMP-DSAR-09")]
    public async Task ASecondErasureReachesTheTaskThatArrivedAfterTheFirst()
    {
        await fixture.ResetAsync();
        await SeedAsync(withNumber: true);
        await Erase("corr-dsar-first", dryRun: false);

        DateTimeOffset? firstStamp;
        await using (IvrDbContext between = await Factory().CreateDbContextAsync())
        {
            firstStamp = (await between.ConfirmationTasks.AsNoTracking()
                .SingleAsync(row => row.TaskId == "TASK-DSAR-01")).AnonymizedAt;

            // Sales sends the same order again, after the erasure.
            Seed(between, "03", OrderCode, withCallback: false, withNumber: true);
            await between.SaveChangesAsync();
        }

        await Erase("corr-dsar-preview", dryRun: true);
        DsarErasureReport second = await Erase("corr-dsar-second", dryRun: false);
        DsarErasureReport third = await Erase("corr-dsar-third", dryRun: false);

        Assert.Equal(1, second.TasksRedacted);
        Assert.Equal(0, third.TasksRedacted);
        Assert.Equal(0, await RowsHoldingAReadableNumberAsync(OrderCode));

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        ConfirmationTaskEntity first = await context.ConfirmationTasks.AsNoTracking()
            .SingleAsync(row => row.TaskId == "TASK-DSAR-01");
        Assert.Equal(firstStamp, first.AnonymizedAt);

        // Every answer is audited, and each says what it reached, not what the order ever held.
        Assert.Equal("matched=1 redacted=0", await TasksMatchedAsync(context, "corr-dsar-preview"));
        Assert.Equal("matched=1 redacted=1", await TasksMatchedAsync(context, "corr-dsar-second"));
        Assert.Equal("matched=0 redacted=0", await TasksMatchedAsync(context, "corr-dsar-third"));
    }

    /// <summary>
    /// W-0314. The number a task was accepted with is the number it is dialled on. The trigger held
    /// every other part of the snapshot and not the dial target, so an UPDATE could point an
    /// accepted confirmation call at somebody else's phone.
    /// </summary>
    [Fact]
    [Trait("TestId", "COMP-DSAR-10")]
    public async Task TheNumberATaskWasAcceptedWithCannotBeChangedAfterwards()
    {
        await fixture.ResetAsync();
        await SeedAsync(withNumber: true);

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        PostgresException refused = await Assert.ThrowsAsync<PostgresException>(() =>
            context.Database.ExecuteSqlRawAsync(
                "UPDATE ivr_confirmation_tasks SET phone_e164 = '+84900000009' "
                + "WHERE task_id = 'TASK-DSAR-01'"));

        Assert.Contains("snapshot is immutable", refused.MessageText, StringComparison.Ordinal);
    }

    /// <summary>
    /// W-0314, the rollout. A pod still running the erasure from before W-0314 is refused on a row
    /// that holds a number, so nothing is marked erased that was not. Only there: a token-only row,
    /// which that statement erases completely, goes through as before.
    /// </summary>
    [Fact]
    [Trait("TestId", "COMP-DSAR-11")]
    public async Task TheOldErasureIsRefusedExactlyWhereItWouldLeaveTheNumberBehind()
    {
        await fixture.ResetAsync();
        await using (IvrDbContext seed = await Factory().CreateDbContextAsync())
        {
            Seed(seed, "01", OrderCode, withCallback: false, withNumber: true);
            Seed(seed, "02", OtherOrderCode, withCallback: false, withNumber: false);
            await seed.SaveChangesAsync();
        }

        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        PostgresException refused = await Assert.ThrowsAsync<PostgresException>(() =>
            context.Database.ExecuteSqlRawAsync(PreW0314Erasure, OrderCode));
        Assert.Contains("snapshot is immutable", refused.MessageText, StringComparison.Ordinal);
        Assert.Equal(1, await RowsHoldingAReadableNumberAsync(OrderCode));

        Assert.Equal(1, await context.Database.ExecuteSqlRawAsync(PreW0314Erasure, OtherOrderCode));
    }

    /// <summary>
    /// W-0314, the rows erased before the fix. They carry <c>anonymized_at</c> and still hold the
    /// number and the contact keys, and no request can reach them again: the trigger refuses a
    /// second stamp, and the erasure now skips erased rows. So the migration clears them -- and only
    /// them; a live task keeps what it needs to be dialled.
    /// </summary>
    [Fact]
    [Trait("TestId", "COMP-DSAR-12")]
    public async Task TheMigrationClearsWhatEarlierErasuresLeftBehindAndNothingElse()
    {
        IDbContextFactory<IvrDbContext> factory = Factory();
        try
        {
            (_, string target) = await RebuildBeforeMigrationAsync(
                factory,
                "_W0314ErasureReachesTheNumber");

            await using (IvrDbContext legacy = await factory.CreateDbContextAsync())
            {
                Seed(legacy, "01", OrderCode, withCallback: false, withNumber: true);
                Seed(legacy, "02", OtherOrderCode, withCallback: false, withNumber: true);
                await legacy.SaveChangesAsync();

                // Erased the way it was done before: stamped, and the number left in place.
                Assert.Equal(1, await legacy.Database.ExecuteSqlRawAsync(PreW0314Erasure, OrderCode));
                Assert.Equal(1, await RowsHoldingAReadableNumberAsync(OrderCode));

                await legacy.GetService<IMigrator>().MigrateAsync(target);
            }

            Assert.Equal(0, await RowsHoldingAReadableNumberAsync(OrderCode));
            await using IvrDbContext context = await factory.CreateDbContextAsync();
            ConfirmationTaskEntity erased = await context.ConfirmationTasks.AsNoTracking()
                .SingleAsync(row => row.OrderCode == OrderCode);
            Assert.Null(erased.OfficialContactId);
            Assert.Null(erased.CustomerTrustStatus);
            Assert.Null(erased.TrustedSkipAllowed);
            Assert.Equal("CUST-DSAR-01", erased.CustomerId);

            // A live task keeps its number: the backfill reaches erased rows and nothing else.
            Assert.Equal(1, await RowsHoldingAReadableNumberAsync(OtherOrderCode));
        }
        finally
        {
            await fixture.ResetAsync();
        }
    }

    /// <summary>
    /// The erasure <see cref="DsarService"/> ran before W-0314, the way an old pod still runs it
    /// during the rollout: every redacted column except the number and the contact keys, and no
    /// filter on rows already erased.
    /// </summary>
    private const string PreW0314Erasure =
        "UPDATE ivr_confirmation_tasks SET phone_ref = 'redacted', phone_masked = '***', "
        + "phone_validation_status = 'REDACTED', dial_token_ciphertext = 'enc:redacted', "
        + "privacy_safe_order_summary_json = '{{}}'::jsonb, anonymized_at = now() "
        + "WHERE order_code = {0}";

    private Task<DsarErasureReport> Erase(string correlationId, bool dryRun) =>
        Service().EraseAsync(
            OrderCode,
            "subject erasure request 2026-09-18",
            "AGT-PRIVACY-01",
            correlationId,
            dryRun,
            CancellationToken.None);

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Ivr.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Ivr.sln was not found above the test binary.");
    }

    // ------------------------------------------------------------------ helpers

    private IDbContextFactory<IvrDbContext> Factory() =>
        fixture.Services.GetRequiredService<IDbContextFactory<IvrDbContext>>();

    private DsarService Service() => new(
        Factory(),
        fixture.Services.GetRequiredService<IAuditLogger>(),
        TimeProvider.System);

    private static int Rows(DsarFindReport report, string table) =>
        report.Holdings.Single(holding => holding.Table == table).RowCount;

    private async Task SeedAsync(bool withNumber = false)
    {
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        Seed(context, "01", OrderCode, withCallback: true, withNumber);
        Seed(context, "02", OtherOrderCode, withCallback: false, withNumber);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// The done-condition of W-0314 as one statement: rows of this order in which any column
    /// still carries a readable Vietnamese mobile number. The whole row rather than a list of
    /// columns, because a list is the thing that was wrong. <c>id</c> is left out: it is a random
    /// UUID, and a UUID can hold ten digits in a row by chance.
    /// </summary>
    private async Task<int> RowsHoldingAReadableNumberAsync(string orderCode)
    {
        await using IvrDbContext context = await Factory().CreateDbContextAsync();
        return await context.Database.SqlQuery<int>($$"""
            SELECT count(*)::int AS "Value"
            FROM ivr_confirmation_tasks t
            WHERE t.order_code = {{orderCode}}
              AND (to_jsonb(t) - 'id')::text ~ '(^|[^0-9])(\+?84|0)[0-9]{9}([^0-9]|$)'
            """).SingleAsync();
    }

    private static async Task<string> TasksMatchedAsync(IvrDbContext context, string correlationId)
    {
        AuditLogEntity entry = await context.AuditLog.AsNoTracking()
            .SingleAsync(row => row.CorrelationId == correlationId);
        using System.Text.Json.JsonDocument data = System.Text.Json.JsonDocument.Parse(entry.DataJson);
        return string.Create(
            CultureInfo.InvariantCulture,
            $"matched={data.RootElement.GetProperty("tasks_matched").GetInt32()} "
                + $"redacted={data.RootElement.GetProperty("tasks_redacted").GetInt32()}");
    }

    private static async Task<(string Previous, string Target)> RebuildBeforeMigrationAsync(
        IDbContextFactory<IvrDbContext> factory,
        string targetSuffix)
    {
        await using IvrDbContext context = await factory.CreateDbContextAsync();
        await context.Database.EnsureDeletedAsync();
        string[] migrations = [.. context.Database.GetMigrations()];
        int index = Array.FindIndex(
            migrations,
            migration => migration.EndsWith(targetSuffix, StringComparison.Ordinal));
        Assert.True(
            index > 0,
            $"Expected a migration ending in '{targetSuffix}' with at least one migration before "
                + $"it, but the chain ends [{string.Join(", ", migrations[^3..])}].");
        await context.GetService<IMigrator>().MigrateAsync(migrations[index - 1]);
        return (migrations[index - 1], migrations[index]);
    }

    private static void Seed(
        IvrDbContext context,
        string suffix,
        string orderCode,
        bool withCallback,
        bool withNumber = false)
    {
        string taskId = $"TASK-DSAR-{suffix}";
        string jobId = $"JOB-DSAR-{suffix}";
        string orderId = $"ORDER-DSAR-{suffix}";
        DateTimeOffset t0 = Now.AddMinutes(-5);

        context.ConfirmationTasks.Add(new ConfirmationTaskEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            ContractVersion = "ivr-order-confirmation.v1",
            IdempotencyKey = $"dsar-idem-{suffix}",
            CorrelationId = $"corr-dsar-{suffix}",
            OfficialOrderId = orderId,
            OrderCode = orderCode,
            OrderVersion = "1",
            OrderState = "CONFIRMING",
            PaymentMethodSnapshot = "ONLINE",
            IvrConfirmationRequired = true,
            RiskFlagsJson = "[]",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyVersion = "mock-lab-v1",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,450]",
            ConfirmationWindowStartedAt = t0,
            ConfirmationWindowExpiresAt = Now.AddHours(4),
            PhoneRef = $"phone-ref-dsar-{suffix}",
            PhoneMasked = "84xxxxx4567",
            PhoneValidationStatus = "VALID",
            DialTokenCiphertext = $"enc:dsar-token-{suffix}",
            DialTokenExpiresAt = Now.AddHours(4),
            PrivacySafeOrderSummaryJson = "{\"order_code_short\":\"GF-DSAR\"}",
            CallScriptTemplateId = "SCRIPT-ORDER-CONFIRM",
            CallScriptVersion = "SCRIPT-ORDER-CONFIRM:vA",
            EvidencePolicyVersion = "evidence-v1",
            PrivacyPolicyVersion = "privacy-v1",
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            CallRestriction = false,
            NotForQuoteCartDraft = true,
            NoDirectOrderUpdate = true,
            CreatedAt = t0,
            ExpiresAt = Now.AddHours(4),
            AcceptedAt = t0,
            // withNumber: the shape Module 3 sends since W-0311 -- the number itself -- plus the
            // Sales keys a task may carry. Every one of these but customer_id is personal data an
            // erasure must remove (W-0314).
            PhoneE164 = withNumber ? $"+849000000{suffix}" : null,
            CustomerId = withNumber ? $"CUST-DSAR-{suffix}" : null,
            OfficialContactId = withNumber ? $"CONTACT-DSAR-{suffix}" : null,
            CustomerTrustStatus = withNumber ? "TRUSTED" : null,
            TrustedSkipAllowed = withNumber ? true : null,
        });

        context.CallJobs.Add(new CallJobEntity
        {
            IvrCallJobId = jobId,
            TaskId = taskId,
            OfficialOrderId = orderId,
            OrderVersionSnapshot = "1",
            ProgramType = "GOLDEN_HOUR",
            AttemptPolicyCode = "mock-lab-v1",
            Status = "CLOSED",
            MaxAttempts = 2,
            AttemptOffsetsSecondsJson = "[0,450]",
            ConfirmationWindowSeconds = 900,
            AttemptScheduleJson = "[]",
            T0At = t0,
            ExpiresAt = Now.AddHours(4),
            Eligible = true,
            EligibilityDecision = "ELIGIBLE_FOR_IVR",
            QueueStatus = "HELD_MOCK",
            ScriptVersion = "SCRIPT-ORDER-CONFIRM:vA",
            PrivacyPolicyVersion = "privacy-v1",
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            CreatedAt = t0,
            ClosedAt = Now,
        });

        context.CallResults.Add(new CallResultEntity
        {
            IvrCallResultId = $"RESULT-DSAR-{suffix}",
            IvrCallJobId = jobId,
            TaskId = taskId,
            OfficialOrderId = orderId,
            OrderVersionSnapshot = "1",
            OrderVersionSeenByIvr = "1",
            FinalResultStatus = "IVR_CONFIRMED",
            ResultType = "IVR_CONFIRMED",
            IsCountedCustomerAttempt = true,
            IsFinalForIvr = true,
            RecommendedCoreAction = "REVALIDATE_AND_CONFIRM_ORDER",
            CoreOrderHandoffRequired = true,
            HumanReviewRequired = false,
            InputSignalOnly = true,
            NoDirectOrderUpdate = true,
            NoPaymentOrRevenueEffect = true,
            CreatedAt = Now,
        });

        if (!withCallback)
        {
            return;
        }

        context.ResultCallbacks.Add(new ResultCallbackEntity
        {
            CallbackId = $"CALLBACK-DSAR-{suffix}",
            IvrCallResultId = $"RESULT-DSAR-{suffix}",
            TaskId = taskId,
            OfficialOrderId = orderId,
            IdempotencyKey = $"dsar-callback-idem-{suffix}",
            ResultStatus = "IVR_CONFIRMED",
            ResultState = "PENDING_CORE_REVALIDATION",
            DeliveryStatus = "DELIVERED_ACCEPTED",
            RequiresCoreRevalidation = true,
            PayloadJson = string.Create(
                CultureInfo.InvariantCulture,
                $"{{\"order_ref\":\"{orderId}\",\"result\":\"IVR_CONFIRMED\"}}"),
            // ck_ivr_result_callbacks_hash: uppercase hex only.
            PayloadSha256 = new string('A', 64),
            CreatedAt = Now,
            SentAt = Now,
            AcknowledgedAt = Now,
        });
    }
}
