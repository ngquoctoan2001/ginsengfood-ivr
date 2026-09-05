import assert from "node:assert/strict";

// Cheap source gate; EF UpOperations in the .NET gate additionally follows helper calls.
// Raw SQL is checked conservatively, including dynamic SQL (except trigger EXECUTE FUNCTION).
export function inspectExpandSource(source) {
  const withoutComments = source.replace(/\/\*[\s\S]*?\*\//g, " ").replace(/--[^\r\n]*/g, " ");
  const violations = [];
  for (const match of withoutComments.matchAll(/\.\s*(DropColumn|DropTable|RenameColumn|RenameTable|AlterColumn)\s*(?:<[^>]+>)?\s*\(/g)) {
    violations.push(`${match[1]} breaks a two-version overlap`);
  }
  const ddl = /\b(?:DROP\s+(?:TABLE|SCHEMA|DATABASE|OWNED)\b|TRUNCATE\b|ALTER\s+TABLE\b[^;]*(?:\bDROP\s+(?!CONSTRAINT\b)|\bRENAME\b|\bTYPE\b|\bSET\s+NOT\s+NULL\b)|EXECUTE\s+(?!FUNCTION\b|PROCEDURE\b))/gi;
  if (ddl.test(withoutComments)) violations.push("destructive or dynamic raw SQL in expand phase");
  for (const match of source.matchAll(/\.\s*AddColumn<[^>]+>\(([\s\S]*?)\);/g)) {
    if (/nullable:\s*false/.test(match[1]) && !/defaultValue(?:Sql)?\s*:/.test(match[1])) {
      violations.push("NOT NULL column without a default");
    }
  }
  return violations;
}

export function verifyExpandGuard() {
  const forbidden = [
    'builder.DropTable(name: "old");', 'b.DropColumn(name: "old");',
    'b.RenameTable(name: "old");', 'b.RenameColumn(name: "old");',
    'b.AlterColumn<string>(name: "old");',
    'b.AddColumn<int>(name: "required", nullable: false);',
    'b.Sql("DROP TABLE old");', 'b.Sql("DROP /* disguise */ TABLE old");',
    'b.Sql("TRUNCATE TABLE old");', 'b.Sql("ALTER TABLE old DROP COLUMN value");',
    'b.Sql("ALTER TABLE old RENAME TO newer");',
    'b.Sql("ALTER TABLE old ALTER COLUMN value TYPE integer");',
    'b.Sql("ALTER TABLE old ALTER COLUMN value SET NOT NULL");',
    `b.Sql("DO $$ BEGIN EXECUTE 'DR' || 'OP TABLE old'; END $$");`,
    'b.Sql("DROP SCHEMA public CASCADE");',
  ];
  for (const source of forbidden) assert.ok(inspectExpandSource(source).length, `guard missed: ${source}`);
  for (const source of [
    'b.AddColumn<int>(name: "optional", nullable: true);',
    'b.AddColumn<int>(name: "required", nullable: false, defaultValue: 0);',
    'b.Sql("CREATE TABLE IF NOT EXISTS example (id uuid PRIMARY KEY)");',
    'b.Sql("CREATE TRIGGER example AFTER INSERT ON t EXECUTE FUNCTION f()");',
  ]) assert.deepEqual(inspectExpandSource(source), []);
  process.stdout.write(`EXPAND_GUARD_SELFTEST_PASS — ${forbidden.length} negative cases, 4 additive controls\n`);
}
