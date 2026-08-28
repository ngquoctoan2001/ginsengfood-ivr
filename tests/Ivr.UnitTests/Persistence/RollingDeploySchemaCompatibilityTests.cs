using System.Collections.Frozen;
using System.Globalization;
using System.Reflection;
using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Ivr.UnitTests.Persistence;

/// <summary>
/// W-0114. The half of rolling-deploy safety that runs without a database: every migration must
/// leave the release before it able to keep serving.
/// <para>
/// <c>deploy/ci/rollback.md</c> §3 wrote the constraint down and closed with "và nó chưa có test
/// nào ép". That sentence was true when it was written and stopped being true at W-0046, which
/// added the text-level <c>IT-MIGRATE-03</c> without coming back to update it. This widens that
/// check and moves it onto the typed operation model; both now run, for the reasons in
/// <see cref="RollingDeploySchemaCompatibility"/>.
/// </para>
/// <para>
/// The other half — the new binary meeting the old schema — needs a real Postgres and lives in
/// <c>IT-SCHEMA-NEWCODE-01</c>.
/// </para>
/// </summary>
public sealed class RollingDeploySchemaCompatibilityTests
{
    /// <summary>
    /// Migrations whose breaking operation was reviewed and accepted, with the reason.
    /// <para>
    /// Deliberately a constant in the gate's own file rather than a data file somewhere else: an
    /// exemption should show up in review as "somebody edited the schema-compatibility gate", not
    /// as a line in a JSON nobody opens. The key is
    /// <c>{migration}::{operation}::{table.column}</c> — narrow enough that it stops matching if
    /// the migration is edited, so an exemption cannot quietly cover a second change.
    /// </para>
    /// <para>
    /// W-0115 is the first reviewed exception. Its 16 enum sets were derived from every writer in
    /// release N-1, and its migration runs a data preflight before adding any constraint. The
    /// result equality has a separate reason because both N-1 write paths already assign the same
    /// value to both columns. Keeping a reason beside every exact key makes an exemption reviewable
    /// without teaching the classifier to silently accept a broader operation shape.
    /// </para>
    /// </summary>
    private static readonly FrozenDictionary<string, string> ReviewedExemptions =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_sim_channels.status"] = EnumReason,
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_review_items.source_type"] = EnumReason,
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_review_items.status"] = EnumReason,
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_result_callbacks.delivery_status"] = EnumReason,
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_result_callbacks.result_state"] = EnumReason,
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_result_callbacks.result_status"] = EnumReason,
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_confirmation_tasks.eligibility_decision"] = EnumReason,
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_capacity_incidents.scope"] = EnumReason,
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_capacity_incidents.status"] = EnumReason,
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_call_results.final_result_status+result_type"] = ResultEqualityReason,
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_call_results.recommended_core_action"] = EnumReason,
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_call_results.result_type"] = EnumReason,
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_call_jobs.eligibility_decision"] = EnumReason,
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_call_jobs.queue_status"] = EnumReason,
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_call_jobs.status"] = EnumReason,
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_call_attempts.result_status"] = EnumReason,
            ["20260824021636_W0115ClosedEnumChecks::AddCheckConstraint::ivr_call_attempts.status"] = EnumReason,
            ["20260827020347_W0116WindowExpiredCloseStatus::AddCheckConstraint::ivr_call_jobs.queue_status"] = WideningReason,
            ["20260827020347_W0116WindowExpiredCloseStatus::AddCheckConstraint::ivr_call_jobs.status"] = WideningReason,
            ["20260827022343_W0117CountedAttemptInvariant::AddCheckConstraint::ivr_call_results.is_counted_customer_attempt+result_type"] = CountedAttemptReason,
            ["20260827024438_W0118AttemptCountedInvariant::AddCheckConstraint::ivr_call_attempts.is_counted_customer_attempt+result_status"] = AttemptCountedReason,
            ["20260828040458_W0122DropConsoleAccounts::DropTable::ivr_console_accounts"] = ConsoleRetirementReason,
            ["20260828040458_W0122DropConsoleAccounts::DropTable::ivr_console_sessions"] = ConsoleRetirementReason,
        }.ToFrozenDictionary(StringComparer.Ordinal);

    private const string EnumReason =
        "W-0115 audited every N-1 writer and the migration preflights existing values before DDL; "
        + "IT-DBENUM-MIGRATE-05 covers valid N-1 data, rejection, and all exact constraints.";

    private const string WideningReason =
        "W-0116 only adds values to two closed sets ('WINDOW_EXPIRED', 'CLOSED_WINDOW_EXPIRED'); "
        + "every value the N-1 release writes stays allowed, so the constraint cannot reject a row "
        + "the old replicas are still producing. The N-1 direction is the one this test guards, "
        + "and it is the safe one here — the unsafe direction is Down, which narrows the sets and "
        + "will refuse to run while any job still carries the new values, by design.";

    private const string CountedAttemptReason =
        "W-0117 narrows rather than widens, so the N-1 writers were audited one by one. "
        + "ivr_call_results has exactly two: ResultRepository.CreateResult and the scheduler's "
        + "confirmation-window sweep, and both assign the column from NormalizedResult.IsCounted. "
        + "DispositionMapper returns IsCounted=false for every technical and capacity result, "
        + "and never returns the operational or policy types at all — those are decided at "
        + "eligibility, before any result row exists. No N-1 writer can produce a rejected row. "
        + "The migration preflights anyway and names offending result ids, so a schema that "
        + "disagrees with this audit fails loudly instead of on an opaque check violation.";

    private const string AttemptCountedReason =
        "W-0118 narrows, so the N-1 writers were audited one by one. ivr_call_attempts has four: "
        + "PostgresSchedulerStore (attempt creation), InternalAdminApiService (technical retry) "
        + "and PostgresTelephonyDispatchStore (disposition recorded) all assign the literal false, "
        + "and ResultRepository.NormalizeNextAsync assigns result_status and "
        + "is_counted_customer_attempt from the same NormalizedResult on adjacent lines, so the "
        + "two cannot disagree. DispositionMapper returns IsCounted=false for every technical and "
        + "capacity result and never returns the operational or policy types. No N-1 writer can "
        + "produce a rejected row, and the migration preflights anyway, naming offending "
        + "attempt ids rather than failing on an opaque check violation.";

    private const string ConsoleRetirementReason =
        "W-0128 retires console account authentication outright: Module 3 owns operator identity "
        + "now and reaches IVR as a service across three credential tiers. The guard's warning is "
        + "correct in general -- an N-1 replica would query these tables -- and does not apply "
        + "here because the N-1 code that queried them is deleted in the same change, not merely "
        + "stopped from writing. There is no release in which a replica both survives this "
        + "migration and needs these tables, so the rolling-deploy hazard the rule protects "
        + "against cannot arise. Deploy note: this one is not safe to roll back past, and Down "
        + "restores the empty shape rather than the accounts.";

    private const string ResultEqualityReason =
        "W-0115 preflights existing rows; both N-1 writers assign final_result_status and "
        + "result_type from the same normalized result, covered by IT-DBENUM-MIGRATE-05.";

    [Fact]
    [Trait("TestId", "UT-SCHEMA-BACKCOMPAT-01")]
    public void EveryMigrationLeavesThePreviousReleaseAbleToRun()
    {
        // The chart applies migrations from a pre-upgrade hook (deploy/helm/ivr/templates/
        // jobs.yaml), so on every upgrade the schema moves forward while the old replicas are
        // still serving traffic, and `helm rollback` — the readiness board's primary rollback —
        // puts the old image back on a schema that stays forward. Both are old code on a new
        // schema, and neither is avoidable by ordering.
        IModel model = BuildModel();
        var violations = new List<SchemaCompatibilityViolation>();
        var inspected = new List<MigrationOperation>();
        Assert.DoesNotContain(
            ReviewedExemptions.Values,
            reason => string.IsNullOrWhiteSpace(reason));
        foreach ((string id, Migration migration) in DiscoverMigrations())
        {
            inspected.AddRange(RollingDeploySchemaCompatibility.OperationsOf(migration));
            violations.AddRange(RollingDeploySchemaCompatibility
                .Inspect(id, migration, model)
                .Where(violation => !ReviewedExemptions.ContainsKey(violation.Key)));
        }

        // Every assertion in this test is of the form "no operation of this shape", which is
        // exactly what a reader that returned nothing would also report. So first prove the
        // corpus was read: the first migration creates tables, and later ones add columns.
        Assert.Contains(inspected, operation => operation is CreateTableOperation);
        Assert.Contains(inspected, operation => operation is AddColumnOperation);

        Assert.True(
            violations.Count == 0,
            "Migrations that the previous release cannot survive:"
                + Environment.NewLine
                + string.Join(
                    Environment.NewLine,
                    violations.Select(violation => "  - " + violation)));
    }

    [Fact]
    [Trait("TestId", "UT-SCHEMA-BACKCOMPAT-02")]
    public void TheGateRejectsEveryShapeItClaimsToCatch()
    {
        // A gate nobody has watched reject anything is a gate that reports PASS for a reason
        // nobody has checked. Each case below is a migration that would break a rolling deploy.
        IModel model = BuildModel();

        Assert.Equal("DropColumn", Single(new DropsAColumn(), model).Operation);
        Assert.Equal("DropTable", Single(new DropsATable(), model).Operation);
        Assert.Equal("RenameColumn", Single(new RenamesAColumn(), model).Operation);
        Assert.Equal("AddColumn", Single(new AddsARequiredColumn(), model).Operation);
        Assert.Equal("AlterColumn", Single(new TightensAColumnToNotNull(), model).Operation);
        Assert.Equal("AlterColumn", Single(new ShortensAColumn(), model).Operation);
        Assert.Equal("CreateIndex", Single(new AddsAUniqueIndex(), model).Operation);

        // The check-constraint case is the one with a judgement in it, so it is asserted from
        // both sides. `attempt_number` already exists and is written by the release before this
        // one; a constraint over it can start rejecting those writes mid-deploy.
        SchemaCompatibilityViolation constraint = Single(new ConstrainsAnExistingColumn(), model);
        Assert.Equal("AddCheckConstraint", constraint.Operation);
        Assert.Contains("attempt_number", constraint.Subject, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("TestId", "UT-SCHEMA-BACKCOMPAT-03")]
    public void TheGateAcceptsTheAdditiveShapesAReleaseActuallyNeeds()
    {
        // The control. A gate that flags everything is as useless as one that flags nothing, and
        // this is the shape W-0113 shipped: nullable columns, a filtered index over them, and a
        // constraint that judges only the columns the same migration added.
        IModel model = BuildModel();

        Assert.Empty(RollingDeploySchemaCompatibility.Inspect("t", new AddsNullableColumns(), model));
        Assert.Empty(RollingDeploySchemaCompatibility.Inspect("t", new AddsATable(), model));
        Assert.Empty(RollingDeploySchemaCompatibility.Inspect("t", new ConstrainsOnlyNewColumns(), model));

        // A default makes a NOT NULL column writable by code that never names it, which is the
        // whole reason the rule is "NOT NULL without a default" and not "NOT NULL".
        Assert.Empty(RollingDeploySchemaCompatibility.Inspect("t", new AddsARequiredColumnWithDefault(), model));

        // And widening. A longer column still accepts everything the previous release writes, so
        // flagging every AlterColumn -- which the text-level IT-MIGRATE-03 does -- would refuse a
        // change that is safe in this direction.
        Assert.Empty(RollingDeploySchemaCompatibility.Inspect("t", new WidensAColumn(), model));
    }

    [Fact]
    [Trait("TestId", "UT-SCHEMA-MIGRATION-04")]
    public void EveryMigrationClassIsOneEfCanActuallyFind()
    {
        // EF discovers migrations by [Migration], not by base type. A class that derives from
        // Migration without the attribute compiles, reviews as applied, and never runs — the
        // schema silently lacks whatever it describes, and the failure surfaces later as a
        // missing column in production.
        Type[] declared = [.. typeof(IvrDbContext).Assembly
            .GetTypes()
            .Where(type => typeof(Migration).IsAssignableFrom(type) && !type.IsAbstract)];
        Assert.NotEmpty(declared);

        string[] undiscoverable = [.. declared
            .Where(type => type.GetCustomAttribute<MigrationAttribute>() is null)
            .Select(type => type.Name)];
        Assert.True(
            undiscoverable.Length == 0,
            "Migration classes EF will never run: " + string.Join(", ", undiscoverable));

        // Ids order the deploy. Two migrations sharing one id, or an id that does not sort with
        // its timestamp, makes "the previous migration" ambiguous — and this whole gate is
        // defined in terms of it.
        List<string> ids = [.. DiscoverMigrations().Select(pair => pair.Id)];
        Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
        List<string> sorted = [.. ids.Order(StringComparer.Ordinal)];
        Assert.Equal(sorted, ids);
    }

    private static SchemaCompatibilityViolation Single(Migration migration, IModel model) =>
        Assert.Single(RollingDeploySchemaCompatibility.Inspect(
            migration.GetType().Name,
            migration,
            model));

    internal static IModel BuildModel()
    {
        // No connection is opened. UseNpgsql is needed because the model carries provider-specific
        // configuration, and reading Model builds it in memory.
        DbContextOptions<IvrDbContext> options = new DbContextOptionsBuilder<IvrDbContext>()
            .UseNpgsql("Host=localhost;Port=1;Database=ivr;Username=ivr;Password=unused")
            .Options;
        using var context = new IvrDbContext(options);
        return context.Model;
    }

    internal static IReadOnlyList<(string Id, Migration Migration)> DiscoverMigrations() =>
        [.. typeof(IvrDbContext).Assembly
            .GetTypes()
            .Where(type => typeof(Migration).IsAssignableFrom(type) && !type.IsAbstract)
            .Select(type => (
                Id: type.GetCustomAttribute<MigrationAttribute>()?.Id ?? type.Name,
                Migration: (Migration)Activator.CreateInstance(type)!))
            .OrderBy(pair => pair.Id, StringComparer.Ordinal)];

    // The synthetic migrations below exist only to be rejected or accepted. They are never
    // applied to anything: Inspect reads UpOperations, which the builder produces in memory.
    private sealed class DropsAColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder!.DropColumn(name: "policy_version", table: "ivr_call_attempts");

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }

    private sealed class DropsATable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder!.DropTable(name: "ivr_call_attempts");

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }

    private sealed class RenamesAColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder!.RenameColumn(
                name: "voice_id",
                table: "ivr_call_attempts",
                newName: "dispatched_voice_id");

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }

    private sealed class AddsARequiredColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder!.AddColumn<string>(
                name: "voice_engine",
                table: "ivr_call_attempts",
                type: "text",
                nullable: false);

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }

    private sealed class AddsARequiredColumnWithDefault : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder!.AddColumn<string>(
                name: "voice_engine",
                table: "ivr_call_attempts",
                type: "text",
                nullable: false,
                defaultValue: "UNKNOWN");

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }

    private sealed class TightensAColumnToNotNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder!.AlterColumn<string>(
                name: "voice_id",
                table: "ivr_call_attempts",
                type: "character varying(120)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldNullable: true);

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }

    private sealed class ShortensAColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder!.AlterColumn<string>(
                name: "voice_id",
                table: "ivr_call_attempts",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120,
                oldNullable: true);

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }

    private sealed class WidensAColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder!.AlterColumn<string>(
                name: "voice_id",
                table: "ivr_call_attempts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120,
                oldNullable: true);

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }

    private sealed class AddsAUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder!.CreateIndex(
                name: "IX_ivr_call_attempts_voice_id",
                table: "ivr_call_attempts",
                column: "voice_id",
                unique: true);

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }

    private sealed class ConstrainsAnExistingColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder!.AddCheckConstraint(
                name: "ck_ivr_call_attempts_attempt_number_two",
                table: "ivr_call_attempts",
                sql: "attempt_number <= 2");

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }

    private sealed class ConstrainsOnlyNewColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);
            migrationBuilder.AddColumn<string>(
                name: "voice_engine",
                table: "ivr_call_attempts",
                type: "text",
                nullable: true);
            migrationBuilder.AddCheckConstraint(
                name: "ck_ivr_call_attempts_voice_engine",
                table: "ivr_call_attempts",
                sql: "voice_engine IS NULL OR voice_engine IN ('North', 'Central', 'South')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }

    private sealed class AddsNullableColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);
            migrationBuilder.AddColumn<string>(
                name: "voice_engine",
                table: "ivr_call_attempts",
                type: "text",
                nullable: true);
            migrationBuilder.CreateIndex(
                name: "IX_ivr_call_attempts_voice_engine",
                table: "ivr_call_attempts",
                column: "voice_engine",
                filter: "voice_engine IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }

    private sealed class AddsATable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder!.CreateTable(
                name: "ivr_schema_gate_probe",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                },
                constraints: table => table.PrimaryKey("pk_ivr_schema_gate_probe", x => x.id));

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
