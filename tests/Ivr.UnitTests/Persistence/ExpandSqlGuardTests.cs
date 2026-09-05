using Ivr.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ivr.UnitTests.Persistence;

public sealed class ExpandSqlGuardTests
{
    [Theory]
    [Trait("TestId", "UT-SCHEMA-EXPAND-05")]
    [InlineData("DROP TABLE old_table")]
    [InlineData("DROP /* hidden */ TABLE old_table")]
    [InlineData("DROP SCHEMA public CASCADE")]
    [InlineData("TRUNCATE old_table")]
    [InlineData("ALTER TABLE old_table DROP COLUMN value")]
    [InlineData("ALTER TABLE old_table DROP value")]
    [InlineData("ALTER TABLE old_table RENAME TO new_table")]
    [InlineData("ALTER TABLE old_table ALTER COLUMN value TYPE integer")]
    [InlineData("ALTER TABLE old_table ALTER COLUMN value SET NOT NULL")]
    [InlineData("DO $$ BEGIN EXECUTE 'DR' || 'OP TABLE old_table'; END $$")]
    public void RawSqlAndHelpersCannotBypassTheExpandGate(string sql)
    {
        using var db = Context();
        Assert.Single(RollingDeploySchemaCompatibility.Inspect("test", new RawSql(sql), db.Model));
        Assert.Single(RollingDeploySchemaCompatibility.Inspect("test", new HelperDrop(), db.Model));
    }

    [Theory]
    [Trait("TestId", "UT-SCHEMA-EXPAND-06")]
    [InlineData("CREATE TABLE IF NOT EXISTS example (id uuid PRIMARY KEY)")]
    [InlineData("CREATE TRIGGER example AFTER INSERT ON t EXECUTE FUNCTION f()")]
    public void AdditiveSqlRemainsAllowed(string sql)
    {
        using var db = Context();
        Assert.Empty(RollingDeploySchemaCompatibility.Inspect("test", new RawSql(sql), db.Model));
    }

    private static IvrDbContext Context() => new(new DbContextOptionsBuilder<IvrDbContext>()
        .UseNpgsql("Host=localhost;Port=1;Database=unused;Username=unused").Options);

    private sealed class RawSql(string sql) : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => Emit(migrationBuilder, sql);

        private static void Emit(MigrationBuilder builder, string sql) => builder.Sql(sql);
    }

    private sealed class HelperDrop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder) => Emit(migrationBuilder);

        private static void Emit(MigrationBuilder builder) => builder.DropTable("old_table");
    }
}
