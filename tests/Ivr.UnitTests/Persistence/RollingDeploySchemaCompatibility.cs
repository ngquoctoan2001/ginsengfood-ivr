using System.Collections.Frozen;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Ivr.UnitTests.Persistence;

/// <summary>
/// One violation of the rule that a migration must leave the PREVIOUS release's code able to run.
/// </summary>
/// <param name="MigrationId">The migration that carries the operation.</param>
/// <param name="Operation">The EF operation type, without the <c>Operation</c> suffix.</param>
/// <param name="Subject">Table, or <c>table.column</c>, the operation acts on.</param>
/// <param name="Why">What the previous release's code does that stops working.</param>
internal sealed record SchemaCompatibilityViolation(
    string MigrationId,
    string Operation,
    string Subject,
    string Why)
{
    public string Key => string.Create(
        CultureInfo.InvariantCulture,
        $"{MigrationId}::{Operation}::{Subject}");

    public override string ToString() => string.Create(
        CultureInfo.InvariantCulture,
        $"{MigrationId} — {Operation} on {Subject}: {Why}");
}

/// <summary>
/// Reads a migration's <c>Up</c> operations and reports the ones that would break the release
/// running immediately before it.
/// <para>
/// This is the direction <c>deploy/ci/rollback.md</c> §3 names. The chart applies migrations from
/// a <c>pre-upgrade</c> hook, so the schema moves forward while the old replicas are still
/// serving, and <c>helm rollback</c> puts the old image back on a schema that has already moved.
/// Both windows run OLD CODE ON A NEW SCHEMA, and neither is optional: the first happens on every
/// upgrade, the second on the rollback path the readiness board names as primary.
/// </para>
/// <para>
/// NOT the first check of this property. <c>IT-MIGRATE-03</c> in
/// <c>deploy/ci/scripts/progressive-selftest.mjs</c> has covered five of these shapes since
/// W-0046 by reading the migration's source text. That one stays: it runs in a node image with no
/// .NET toolchain, so it fails earlier and cheaper. This is the deeper of the two, and it exists
/// because reading text has two blind spots that reading operations does not.
/// </para>
/// <para>
/// First: the text scan finds <c>Up</c> by <c>indexOf</c> and stops at <c>Down</c>, so an
/// operation a helper method emits — perfectly ordinary in a hand-written migration — is invisible
/// to it and plain in <see cref="Migration.UpOperations"/>. Second: text sees the call, not the
/// arguments, so it must treat every <c>AlterColumn</c> as breaking, while the operation carries
/// its own before-and-after and can tell widening from narrowing.
/// </para>
/// <para>
/// Reading <see cref="Migration.UpOperations"/> is also why <c>Down</c> needs no special handling.
/// <c>Down</c> is full of drops by construction — that is what a down migration is — so a scan
/// that could not tell the two apart would report every migration in the repository.
/// </para>
/// </summary>
internal static partial class RollingDeploySchemaCompatibility
{
    /// <summary>
    /// <see cref="Migration.UpOperations"/> builds its operations against a provider, and the
    /// provider decides which <c>.Annotation</c> calls apply. Any string works for the shape this
    /// reads, but a wrong one silently drops Npgsql-specific operations.
    /// </summary>
    public const string NpgsqlProvider = "Npgsql.EntityFrameworkCore.PostgreSQL";

    /// <summary>
    /// The operations a migration applies going forward.
    /// <para>
    /// Exposed so a caller can prove the corpus was actually read. Every check below is of the
    /// form "no operation of this shape", which is also what an analyzer that silently produced
    /// an empty list would report.
    /// </para>
    /// </summary>
    public static IReadOnlyList<MigrationOperation> OperationsOf(Migration migration)
    {
        ArgumentNullException.ThrowIfNull(migration);
        migration.ActiveProvider = NpgsqlProvider;
        return migration.UpOperations;
    }

    public static IReadOnlyList<SchemaCompatibilityViolation> Inspect(
        string migrationId,
        Migration migration,
        IModel model)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(migrationId);
        ArgumentNullException.ThrowIfNull(migration);
        ArgumentNullException.ThrowIfNull(model);

        IReadOnlyList<MigrationOperation> operations = OperationsOf(migration);

        // A table this migration creates cannot be one the previous release wrote to, so nothing
        // done to it inside the same migration can break that release.
        FrozenSet<string> newTables = operations
            .OfType<CreateTableOperation>()
            .Select(operation => operation.Name)
            .ToFrozenSet(StringComparer.Ordinal);

        Dictionary<string, HashSet<string>> newColumns = operations
            .OfType<AddColumnOperation>()
            .GroupBy(operation => operation.Table, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(operation => operation.Name)
                    .ToHashSet(StringComparer.Ordinal),
                StringComparer.Ordinal);

        var violations = new List<SchemaCompatibilityViolation>();
        foreach (MigrationOperation operation in operations)
        {
            SchemaCompatibilityViolation? violation = Classify(
                migrationId,
                operation,
                newTables,
                newColumns,
                model);
            if (violation is not null)
            {
                violations.Add(violation);
            }
        }

        return violations;
    }

    private static SchemaCompatibilityViolation? Classify(
        string migrationId,
        MigrationOperation operation,
        FrozenSet<string> newTables,
        Dictionary<string, HashSet<string>> newColumns,
        IModel model) => operation switch
        {
            SqlOperation sql when DestructiveSql().IsMatch(SqlComments().Replace(sql.Sql, " "))
                && !HistoricalExpandBaseline.IsPinned(migrationId) =>
                new SchemaCompatibilityViolation(
                    migrationId,
                    "Sql",
                    "destructive-or-dynamic-ddl",
                    "raw SQL must not bypass the expand gate; dynamic SQL requires a separately reviewed contract release."),

            DropColumnOperation drop => new SchemaCompatibilityViolation(
                migrationId,
                "DropColumn",
                Subject(drop.Table, drop.Name),
                "the previous release still selects this column; EF emits it in every query that "
                    + "materialises the entity, so the first read is 42703 undefined_column."),

            DropTableOperation drop when !newTables.Contains(drop.Name) =>
                new SchemaCompatibilityViolation(
                    migrationId,
                    "DropTable",
                    drop.Name,
                    "the previous release still queries this table."),

            RenameColumnOperation rename => new SchemaCompatibilityViolation(
                migrationId,
                "RenameColumn",
                Subject(rename.Table, rename.Name),
                "a rename is a drop and an add seen from the previous release, which knows only "
                    + "the old name. Add the new column, ship the code, drop the old one a "
                    + "release later."),

            RenameTableOperation rename => new SchemaCompatibilityViolation(
                migrationId,
                "RenameTable",
                rename.Name,
                "the previous release still queries the old table name."),

            AddColumnOperation add
                when !add.IsNullable
                    && add.DefaultValue is null
                    && add.DefaultValueSql is null
                    && !newTables.Contains(add.Table) =>
                new SchemaCompatibilityViolation(
                    migrationId,
                    "AddColumn",
                    Subject(add.Table, add.Name),
                    "NOT NULL with no default: the previous release's INSERT does not name this "
                        + "column, so every write it attempts is 23502 not_null_violation."),

            AlterColumnOperation alter when Narrows(alter) is { } narrowing =>
                new SchemaCompatibilityViolation(
                    migrationId,
                    "AlterColumn",
                    Subject(alter.Table, alter.Name),
                    narrowing),

            AddUniqueConstraintOperation unique when !newTables.Contains(unique.Table) =>
                new SchemaCompatibilityViolation(
                    migrationId,
                    "AddUniqueConstraint",
                    Subject(unique.Table, string.Join('+', unique.Columns)),
                    "the previous release does not know the pair must be unique and will keep "
                        + "writing duplicates."),

            CreateIndexOperation index when index.IsUnique && !newTables.Contains(index.Table) =>
                new SchemaCompatibilityViolation(
                    migrationId,
                    "CreateIndex",
                    Subject(index.Table, string.Join('+', index.Columns)),
                    "a unique index is a uniqueness rule the previous release does not enforce."),

            AddCheckConstraintOperation check
                when !newTables.Contains(check.Table)
                    && PreexistingColumnsIn(check, newColumns, model) is { Count: > 0 } touched =>
                new SchemaCompatibilityViolation(
                    migrationId,
                    "AddCheckConstraint",
                    Subject(check.Table, string.Join('+', touched)),
                    "the constraint judges columns that already existed, so it can reject rows "
                        + "the previous release is still writing. A constraint over columns this "
                        + "same migration adds cannot."),

            _ => null,
        };

    /// <summary>
    /// How an <c>AlterColumn</c> narrows the column, or null when it only widens it.
    /// <para>
    /// Widening is safe in this direction — a longer varchar still accepts everything the previous
    /// release writes. Narrowing is not, in any of its three forms, and all three reject writes
    /// that are already in flight rather than failing at deploy time where somebody would see it.
    /// </para>
    /// </summary>
    private static string? Narrows(AlterColumnOperation alter)
    {
        if (!alter.IsNullable && (alter.OldColumn?.IsNullable ?? true))
        {
            return "tightening a nullable column to NOT NULL rejects the writes the previous "
                + "release is still making.";
        }

        if (alter.MaxLength is { } length
            && alter.OldColumn?.MaxLength is { } oldLength
            && length < oldLength)
        {
            return "shortening the column rejects values the previous release still writes.";
        }

        if (alter.OldColumn is { } old && alter.ClrType != old.ClrType)
        {
            return "changing the type rejects values the previous release still writes in the "
                + "old one.";
        }

        return null;
    }

    /// <summary>
    /// The columns a check constraint reads that the previous release was already writing.
    /// <para>
    /// A constraint that talks only about columns this migration adds is harmless: the previous
    /// release leaves them null, and the constraints written so far are all of that shape. One
    /// that judges an existing column is a new rule applied to writes already in flight.
    /// </para>
    /// <para>
    /// Column names come from the EF model rather than from guessing which SQL tokens are
    /// identifiers, so <c>IS</c>, <c>NULL</c> and <c>IN</c> are never mistaken for columns and a
    /// column called <c>value</c> is never missed.
    /// </para>
    /// <para>
    /// Known limit: "new" means "added by this migration", not "added by this release". A release
    /// that ships two migrations — one adding a column, the next constraining it — reads as
    /// breaking here even though the previous release never saw either. That is a false positive,
    /// which is the direction to be wrong in, and the reviewed-exemption list is where it gets
    /// recorded if it ever happens.
    /// </para>
    /// </summary>
    private static IReadOnlyList<string> PreexistingColumnsIn(
        AddCheckConstraintOperation check,
        Dictionary<string, HashSet<string>> newColumns,
        IModel model)
    {
        HashSet<string> added = newColumns.TryGetValue(check.Table, out HashSet<string>? columns)
            ? columns
            : [];

        // String literals first: 'North' must not be read as a column name, and a value that
        // happens to match one would otherwise make an additive constraint look breaking.
        string sql = StringLiterals().Replace(check.Sql ?? string.Empty, " ");
        HashSet<string> mentioned = Identifiers()
            .Matches(sql)
            .Select(match => match.Value)
            .ToHashSet(StringComparer.Ordinal);

        return [.. ColumnsOf(model, check.Table, check.Schema)
            .Where(column => mentioned.Contains(column) && !added.Contains(column))
            .Order(StringComparer.Ordinal)];
    }

    private static IEnumerable<string> ColumnsOf(IModel model, string table, string? schema)
    {
        StoreObjectIdentifier store = StoreObjectIdentifier.Table(table, schema);
        return model.GetEntityTypes()
            .Where(entity => string.Equals(entity.GetTableName(), table, StringComparison.Ordinal))
            .SelectMany(entity => entity.GetProperties())
            .Select(property => property.GetColumnName(store))
            .Where(column => !string.IsNullOrEmpty(column))
            .Select(column => column!)
            .Distinct(StringComparer.Ordinal);
    }

    private static string Subject(string table, string name) => string.Create(
        CultureInfo.InvariantCulture,
        $"{table}.{name}");

    [GeneratedRegex("'[^']*'", RegexOptions.CultureInvariant)]
    private static partial Regex StringLiterals();

    [GeneratedRegex("[A-Za-z_][A-Za-z0-9_]*", RegexOptions.CultureInvariant)]
    private static partial Regex Identifiers();

    [GeneratedRegex(@"/\*[\s\S]*?\*/|--[^\r\n]*", RegexOptions.CultureInvariant)]
    private static partial Regex SqlComments();

    [GeneratedRegex(@"\b(?:DROP\s+(?:TABLE|SCHEMA|DATABASE|OWNED)\b|TRUNCATE\b|ALTER\s+TABLE\b[^;]*(?:\bDROP\s+(?!CONSTRAINT\b)|\bRENAME\b|\bTYPE\b|\bSET\s+NOT\s+NULL\b)|EXECUTE\s+(?!FUNCTION\b|PROCEDURE\b))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DestructiveSql();
}
