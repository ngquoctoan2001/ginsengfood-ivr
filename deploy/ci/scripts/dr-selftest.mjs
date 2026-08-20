// W-0053 / P10-2 §8 — the four drills, run for real against PostgreSQL in Docker.
//
// §11 forbids "DR on paper", so nothing here reads a YAML file and calls it proof. What each drill
// does is stated in its own comment, and so is what it does NOT establish — the honest limit of a
// single-host drill is that it can prove the mechanism and cannot prove the topology.
//
//   DG-CRYPTO-01  TLS is enforced by the server, not hoped for by the client; and the chart cannot
//                 render a connection string that silently falls back to plaintext.
//   DG-BACKUP-02  A real dump is encrypted in flight, is refused when tampered with, and restores
//                 into a fresh database with the same row counts.
//   DG-DR-03      A synchronous standby is promoted after the primary is killed, and the row
//                 committed immediately before the kill survives.
//   DG-RETENTION-04  Backups are pruned by age, and a restored backup still carries the retention
//                 marks that make its expired rows deletable.
import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const repositoryRoot = path.resolve(scriptDirectory, "../../..");

const POSTGRES = "postgres:16-alpine";
const HELM = "alpine/helm:3.16.3";
const NETWORK = "ivr-dr-selftest";
const PRIMARY = "ivr-dr-primary";
const STANDBY = "ivr-dr-standby";
const PLAIN = "ivr-dr-plaintext";
const HELMBOX = "ivr-dr-helm";
const PASSWORD = "ivr-dr-password";
const REPLICATION_PASSWORD = "ivr-dr-replication";
const DATABASE = "ivr_dr";
const DATA = "/var/lib/postgresql/data";

/** Promotion budget. A drill with no stated target cannot fail on time. */
const RTO_BUDGET_SECONDS = 60;

const created = [];

/** touch -t stamp for N days ago: YYYYMMDDhhmm, the one form busybox and GNU agree on. */
function stampDaysAgo(days) {
  const when = new Date(Date.now() - (days * 24 * 60 * 60 * 1000));
  const pad = (value) => String(value).padStart(2, "0");
  return [
    when.getUTCFullYear(),
    pad(when.getUTCMonth() + 1),
    pad(when.getUTCDate()),
    pad(when.getUTCHours()),
    pad(when.getUTCMinutes()),
  ].join("");
}

/** Progress marker. A drill that can hang must say where it got to. */
function step(label) {
  process.stdout.write(`  .. ${label}
`);
}

function docker(args, options = {}) {
  return execFileSync("docker", args, {
    encoding: "utf8",
    stdio: options.inherit ? "inherit" : ["ignore", "pipe", "pipe"],
    maxBuffer: 64 * 1024 * 1024,
    ...options,
  });
}

function tryDocker(args) {
  try {
    return { ok: true, output: docker(args) };
  } catch (error) {
    return { ok: false, output: `${error.stdout ?? ""}${error.stderr ?? ""}` };
  }
}

function sh(container, command, user) {
  const args = ["exec"];
  if (user) {
    args.push("--user", user);
  }
  args.push(container, "sh", "-c", command);
  return docker(args);
}

function trySh(container, command, user) {
  const args = ["exec"];
  if (user) {
    args.push("--user", user);
  }
  args.push(container, "sh", "-c", command);
  return tryDocker(args);
}

function psql(container, sql, { database = DATABASE, sslmode = "require", user = "postgres" } = {}) {
  const url = `postgresql://${user}:${PASSWORD}@127.0.0.1:5432/${database}?sslmode=${sslmode}`;
  return sh(container, `PGPASSWORD='${PASSWORD}' psql -AtqX "${url}" -c "${sql}"`, "postgres").trim();
}

function tryPsql(container, sql, options = {}) {
  const { database = DATABASE, sslmode = "require", user = "postgres" } = options;
  const url = `postgresql://${user}:${PASSWORD}@127.0.0.1:5432/${database}?sslmode=${sslmode}`;
  return trySh(container, `PGPASSWORD='${PASSWORD}' psql -AtqX "${url}" -c "${sql}"`, "postgres");
}

async function waitFor(label, probe, timeoutMs = 90_000) {
  const deadline = Date.now() + timeoutMs;
  let last = "";
  while (Date.now() < deadline) {
    const result = probe();
    if (result.ok) {
      return result;
    }
    last = result.output;
    await new Promise((resolve) => setTimeout(resolve, 1_000));
  }
  throw new Error(`timed out waiting for ${label}: ${last}`);
}

// ------------------------------------------------------------------ environment

async function up() {
  down();
  docker(["network", "create", NETWORK]);
  created.push(() => tryDocker(["network", "rm", NETWORK]));

  // The plaintext control. Without it, "the TLS server refused a non-TLS client" is equally
  // consistent with "the client cannot do non-TLS at all", and the drill would prove nothing.
  docker([
    "run", "-d", "--name", PLAIN, "--network", NETWORK,
    "-e", `POSTGRES_PASSWORD=${PASSWORD}`, "-e", `POSTGRES_DB=${DATABASE}`,
    POSTGRES,
  ]);
  created.push(() => tryDocker(["rm", "-f", PLAIN]));

  docker([
    "run", "-d", "--name", PRIMARY, "--network", NETWORK,
    "-e", `POSTGRES_PASSWORD=${PASSWORD}`, "-e", `POSTGRES_DB=${DATABASE}`,
    POSTGRES,
    "-c", "wal_level=replica", "-c", "max_wal_senders=10", "-c", "hot_standby=on",
  ]);
  created.push(() => tryDocker(["rm", "-f", PRIMARY]));

  await waitFor("primary accepting connections", () =>
    trySh(PRIMARY, "pg_isready -h 127.0.0.1 -U postgres", "postgres"));
  await waitFor("plaintext control accepting connections", () =>
    trySh(PLAIN, "pg_isready -h 127.0.0.1 -U postgres", "postgres"));

  // openssl is not in the image. The production backup image bakes it in; here it is installed at
  // drill time so the drill exercises the real script rather than a reimplementation of it.
  sh(PRIMARY, "apk add --no-cache openssl >/dev/null 2>&1");

  sh(PRIMARY, [
    `openssl req -new -x509 -days 1 -nodes -text -out ${DATA}/server.crt`,
    `-keyout ${DATA}/server.key -subj "/CN=${PRIMARY}"`,
  ].join(" "), "postgres");
  sh(PRIMARY, `chmod 600 ${DATA}/server.key`, "postgres");
  sh(PRIMARY, [
    `printf "ssl = on\\nssl_cert_file = '${DATA}/server.crt'\\nssl_key_file = '${DATA}/server.key'\\n"`,
    `>> ${DATA}/postgresql.conf`,
  ].join(" "), "postgres");

  // Every TCP rule becomes hostssl. This is the line that turns "we would like TLS" into a server
  // policy: a client asking for a plaintext TCP session now has no matching rule at all.
  sh(PRIMARY, `sed -i 's/^host /hostssl /' ${DATA}/pg_hba.conf`, "postgres");
  sh(PRIMARY, `printf "host replication repl 0.0.0.0/0 scram-sha-256\\n" >> ${DATA}/pg_hba.conf`, "postgres");

  docker(["restart", PRIMARY]);
  await waitFor("primary back with TLS", () =>
    trySh(PRIMARY, "pg_isready -h 127.0.0.1 -U postgres", "postgres"));

  psql(PRIMARY,
    `CREATE ROLE repl WITH REPLICATION LOGIN PASSWORD '${REPLICATION_PASSWORD}';`,
    { database: "postgres" });
}

function down() {
  for (const name of [STANDBY, PRIMARY, PLAIN, HELMBOX]) {
    tryDocker(["rm", "-f", name]);
  }
  tryDocker(["network", "rm", NETWORK]);
}

// ----------------------------------------------------------------- DG-CRYPTO-01

async function inTransitEncryptionIsEnforced() {
  step("DG-CRYPTO-01 server policy");
  // Half one: the server. A connection that negotiated TLS reports it in pg_stat_ssl, which is the
  // server's own account of the session rather than the client's intention.
  const ssl = psql(PRIMARY, "SELECT ssl FROM pg_stat_ssl WHERE pid = pg_backend_pid();");
  assert.equal(ssl, "t", "a sslmode=require session did not report ssl=true on the server.");

  const plaintextAttempt = tryPsql(PRIMARY, "SELECT 1;", { sslmode: "disable" });
  assert(!plaintextAttempt.ok, "the TLS-only primary accepted a plaintext connection.");
  assert(
    /no pg_hba.conf entry|SSL off|no encryption/i.test(plaintextAttempt.output),
    `plaintext was refused for an unexpected reason: ${plaintextAttempt.output}`);

  // Positive control: the same client, the same flag, a server without the policy.
  const control = tryPsql(PLAIN, "SELECT 1;", { sslmode: "disable" });
  assert(
    control.ok,
    `the plaintext control refused too, so the refusal above proves nothing: ${control.output}`);

  // Half two: the chart. A server can only enforce what the client asks for, and Npgsql's default
  // of Prefer falls back to plaintext without an error. What the chart renders is therefore part
  // of the control, not documentation about it.
  const expected = {
    dev: { mode: "Disable", trust: "false" },
    staging: { mode: "Require", trust: "true" },
    lab: { mode: "Require", trust: "true" },
    prod: { mode: "Require", trust: "false" },
  };

  docker(["run", "-d", "--name", HELMBOX, "--entrypoint", "sh", HELM, "-c", "sleep 900"]);
  created.push(() => tryDocker(["rm", "-f", HELMBOX]));
  docker(["cp", path.join(repositoryRoot, "deploy/helm/ivr"), `${HELMBOX}:/ivr`]);

  for (const [environment, want] of Object.entries(expected)) {
    const rendered = docker([
      "exec", HELMBOX, "helm", "template", "ivr", "/ivr", "-f", `/ivr/values-${environment}.yaml`,
    ]);
    const match = /SSL Mode=([A-Za-z]+);Trust Server Certificate=(true|false)/.exec(rendered);
    assert(match, `${environment} rendered no SSL Mode at all.`);
    assert.equal(match[1], want.mode, `${environment} rendered SSL Mode=${match[1]}.`);
    assert.equal(match[2], want.trust, `${environment} rendered Trust Server Certificate=${match[2]}.`);
  }

  // Break each rule and require the render to fail. A ladder rule nobody has tried to break is a
  // comment (P7-2 section 2).
  const violations = [
    ["values-prod.yaml", "database.sslMode=Prefer", "falls back to plaintext in silence"],
    ["values-staging.yaml", "database.sslMode=Disable", "Only dev may run without TLS"],
    ["values-prod.yaml", "database.trustServerCertificate=true", "not a machine in the middle"],
  ];
  for (const [values, override, expectedText] of violations) {
    const attempt = tryDocker([
      "exec", HELMBOX, "helm", "template", "ivr", "/ivr", "-f", `/ivr/${values}`, "--set", override,
    ]);
    assert(!attempt.ok, `${override} rendered successfully; the guard is not enforcing.`);
    assert(
      attempt.output.includes(expectedText),
      `${override} failed for the wrong reason: ${attempt.output}`);
  }

  process.stdout.write(
    "DG-CRYPTO-01 PASS — the server refuses plaintext (with a positive control), and no environment "
    + "can render a fallback-capable connection string\n");
}

// ----------------------------------------------------------------- DG-BACKUP-02

async function encryptedBackupRestores() {
  step("DG-BACKUP-02 encrypted dump");
  await seedAsync();

  sh(PRIMARY, "mkdir -p /backups /scripts && chown postgres /backups /scripts");
  for (const script of ["backup.sh", "restore.sh", "prune.sh"]) {
    docker(["cp", path.join(repositoryRoot, "deploy/backup", script), `${PRIMARY}:/scripts/${script}`]);
  }
  sh(PRIMARY, "sed -i 's/\\r$//' /scripts/*.sh && chmod +x /scripts/*.sh");
  sh(PRIMARY, "openssl rand -hex 32 > /backups/master.key && chmod 600 /backups/master.key", "postgres");

  const env = [
    `PGHOST=127.0.0.1 PGUSER=postgres PGPASSWORD='${PASSWORD}' PGDATABASE=${DATABASE} PGSSLMODE=require`,
    "IVR_BACKUP_KEY_FILE=/backups/master.key IVR_BACKUP_DIR=/backups",
  ].join(" ");

  const started = Date.now();
  const output = sh(PRIMARY, `${env} sh /scripts/backup.sh`, "postgres");
  const backupSeconds = (Date.now() - started) / 1000;
  assert(output.includes("BACKUP_OK"), `backup did not report success: ${output}`);

  const artefact = /BACKUP_OK (\S+)/.exec(output)[1];

  // The property that a two-step "dump then encrypt" cannot offer. If any plaintext SQL had been
  // written, this finds it.
  const plaintextFiles = sh(PRIMARY, "ls -1 /backups | grep -c '\\.sql$' || true").trim();
  assert.equal(plaintextFiles, "0", "a plaintext .sql file was left in the backup directory.");

  // And the ciphertext must not be readable as SQL. A weak check on its own; meaningful next to
  // the row-count comparison after the restore below.
  const looksEncrypted = trySh(PRIMARY, `grep -qa "CREATE TABLE" ${artefact}`);
  assert(!looksEncrypted.ok, "the artefact contains readable SQL; it is not encrypted.");

  const sourceCounts = countsOf(PRIMARY, DATABASE);

  // Tamper first, restore second. Doing it in this order means the successful restore afterwards
  // also proves the tamper detection did not simply break everything.
  sh(PRIMARY, [
    `cp ${artefact} /backups/tampered.sql.enc`,
    `&& cp ${artefact.replace(".sql.enc", ".iv")} /backups/tampered.iv`,
    `&& cp ${artefact.replace(".sql.enc", ".hmac")} /backups/tampered.hmac`,
  ].join(" "), "postgres");
  // Flip one byte in the middle of the ciphertext. With AES-CTR and no MAC this would flip exactly
  // one byte of the restored SQL, which is enough to change a value or a predicate.
  const size = Number.parseInt(sh(PRIMARY, `wc -c < /backups/tampered.sql.enc`, "postgres").trim(), 10);
  assert(size > 64, `the artefact is implausibly small (${size} bytes).`);
  const beforeTamper = sh(PRIMARY, "md5sum /backups/tampered.sql.enc", "postgres").split(" ")[0];
  sh(PRIMARY, [
    "printf 'X' |",
    `dd of=/backups/tampered.sql.enc bs=1 seek=${Math.floor(size / 2)} conv=notrunc 2>/dev/null`,
  ].join(" "), "postgres");
  const afterTamper = sh(PRIMARY, "md5sum /backups/tampered.sql.enc", "postgres").split(" ")[0];
  // Precondition, not decoration. If the tamper silently failed, the refusal asserted below would
  // be a test of nothing that passes for the wrong reason.
  assert.notEqual(
    afterTamper,
    beforeTamper,
    "the tamper did not change the artefact, so the refusal check would be vacuous.");

  const tampered = trySh(PRIMARY, [
    `PGHOST=127.0.0.1 PGUSER=postgres PGPASSWORD='${PASSWORD}' PGSSLMODE=require`,
    "IVR_BACKUP_KEY_FILE=/backups/master.key IVR_RESTORE_TARGET=ivr_restore_tampered",
    "sh /scripts/restore.sh /backups/tampered.sql.enc",
  ].join(" "), "postgres");
  assert(!tampered.ok, "a tampered artefact was restored.");
  assert(
    tampered.output.includes("RESTORE_REFUSED"),
    `the tampered artefact failed for the wrong reason: ${tampered.output}`);

  const restoreStarted = Date.now();
  const restored = sh(PRIMARY, [
    `PGHOST=127.0.0.1 PGUSER=postgres PGPASSWORD='${PASSWORD}' PGSSLMODE=require`,
    "IVR_BACKUP_KEY_FILE=/backups/master.key IVR_RESTORE_TARGET=ivr_restore_ok",
    `sh /scripts/restore.sh ${artefact}`,
  ].join(" "), "postgres");
  const restoreSeconds = (Date.now() - restoreStarted) / 1000;
  assert(restored.includes("RESTORE_OK"), `restore did not report success: ${restored}`);

  const restoredCounts = countsOf(PRIMARY, "ivr_restore_ok");
  assert.deepEqual(
    restoredCounts,
    sourceCounts,
    "the restored database does not match the source row counts.");

  process.stdout.write(
    `DG-BACKUP-02 PASS — encrypted in flight, tampering refused before decryption, restored `
    + `${sourceCounts.length} tables with identical counts `
    + `(backup ${backupSeconds.toFixed(1)}s, restore ${restoreSeconds.toFixed(1)}s)\n`);

  return { artefact };
}

function countsOf(container, database) {
  const sql = [
    "SELECT table_name FROM information_schema.tables",
    "WHERE table_schema='public' AND table_type='BASE TABLE' ORDER BY table_name",
  ].join(" ");
  const tables = psql(container, sql, { database }).split("\n").filter(Boolean);
  return tables.map((table) => {
    const count = psql(container, `SELECT count(*) FROM public.${table}`, { database });
    return `${table}=${count}`;
  });
}

async function seedAsync() {
  // A small stand-in for the operational schema, carrying the one column the retention drill needs:
  // retain_until, which is what makes a restored row still deletable rather than newly born.
  const statements = [
    "CREATE TABLE IF NOT EXISTS ivr_dr_tasks ("
    + "task_id text PRIMARY KEY, phone_masked text NOT NULL, retain_until timestamptz)",
    "CREATE TABLE IF NOT EXISTS ivr_dr_results ("
    + "result_id text PRIMARY KEY, task_id text NOT NULL, result_type text NOT NULL)",
    "INSERT INTO ivr_dr_tasks VALUES "
    + "('TASK-DR-01','84xxxxx1111', now() - interval '400 days'),"
    + "('TASK-DR-02','84xxxxx2222', now() + interval '30 days'),"
    + "('TASK-DR-03','84xxxxx3333', now() - interval '400 days') ON CONFLICT DO NOTHING",
    "INSERT INTO ivr_dr_results VALUES "
    + "('RESULT-DR-01','TASK-DR-01','IVR_CONFIRMED'),"
    + "('RESULT-DR-02','TASK-DR-02','IVR_NO_ANSWER_FINAL') ON CONFLICT DO NOTHING",
  ];
  for (const statement of statements) {
    psql(PRIMARY, statement);
  }
}

// -------------------------------------------------------------------- DG-DR-03

async function synchronousStandbyPromotes() {
  // Synchronous replication is configured on the primary FIRST, then the standby is built from it.
  // That is the realistic order, and it matters: pg_basebackup copies postgresql.auto.conf, so the
  // standby inherits synchronous_standby_names and carries it into promotion. The trap that falls
  // out of this is asserted below rather than avoided.
  step("DG-DR-03 configuring synchronous replication");
  psql(PRIMARY, "ALTER SYSTEM SET synchronous_standby_names = 'standby1'", { database: "postgres" });
  psql(PRIMARY, "SELECT pg_reload_conf()", { database: "postgres" });

  step("DG-DR-03 base backup");
  docker([
    "run", "-d", "--name", STANDBY, "--network", NETWORK, "--user", "postgres",
    "-e", `PGPASSWORD=${REPLICATION_PASSWORD}`,
    "--entrypoint", "sh", POSTGRES, "-c",
    [
      `rm -rf ${DATA}/* &&`,
      `pg_basebackup -h ${PRIMARY} -p 5432 -U repl -D ${DATA} -R -X stream`,
      `-d "host=${PRIMARY} user=repl password=${REPLICATION_PASSWORD} application_name=standby1" &&`,
      `chmod 700 ${DATA} &&`,
      "exec postgres",
    ].join(" "),
  ]);
  created.push(() => tryDocker(["rm", "-f", STANDBY]));

  // sync_state, not merely "a row exists". A streaming standby appears in pg_stat_replication the
  // moment its WAL receiver connects, which happens while pg_basebackup is still copying files —
  // waiting on presence alone would race the copy and prove nothing about durability.
  await waitFor("standby registered as sync", () => {
    const state = tryPsql(PRIMARY,
      "SELECT sync_state FROM pg_stat_replication WHERE application_name='standby1'",
      { database: "postgres" });
    return state.ok && state.output.includes("sync") ? state : { ok: false, output: state.output };
  });

  // Synchronous, not asynchronous, and the difference is the whole RPO claim. Async replication has
  // a real RPO: the primary acknowledges a commit the standby has not received, so killing the
  // primary loses it. Synchronous commit costs a network round trip per write and buys RPO=0, which
  // this drill asserts rather than assumes.
  step("DG-DR-03 committing the row that must survive");
  psql(PRIMARY, "SET synchronous_commit = on; "
    + "INSERT INTO ivr_dr_results VALUES ('RESULT-DR-LASTCOMMIT','TASK-DR-01','IVR_CONFIRMED')");

  // SIGKILL, not a graceful stop. A clean shutdown flushes and exercises a different thing: this
  // drill is about losing a machine, not about a machine leaving politely.
  step("DG-DR-03 killing primary");
  const outageStarted = Date.now();
  docker(["kill", "-s", "KILL", PRIMARY]);

  step("DG-DR-03 promoting standby");
  sh(STANDBY, `pg_ctl promote -D ${DATA}`, "postgres");
  await waitFor("standby out of recovery", () => {
    const recovery = trySh(STANDBY,
      `psql -AtqX -d ${DATABASE} -c "SELECT pg_is_in_recovery()"`, "postgres");
    return recovery.ok && recovery.output.trim() === "f"
      ? recovery
      : { ok: false, output: recovery.output };
  });
  const promoteSeconds = (Date.now() - outageStarted) / 1000;

  const survived = sh(STANDBY,
    `psql -AtqX -d ${DATABASE} -c "SELECT count(*) FROM ivr_dr_results WHERE result_id='RESULT-DR-LASTCOMMIT'"`,
    "postgres").trim();
  assert.equal(survived, "1", "the row committed immediately before the kill did not survive.");

  // THE TRAP, asserted rather than avoided. The promoted node is out of recovery, accepts
  // connections, answers reads and passes a health check -- and every write blocks in SyncRep,
  // waiting for a synchronous standby that no longer exists.
  //
  // The first attempt at this check used statement_timeout and hung, which turned out to be the
  // more useful finding: statement_timeout does NOT rescue a backend waiting for synchronous
  // replication, because the wait happens at COMMIT, after the statement has finished. The
  // transaction is already durable locally; what is missing is the acknowledgement. So the check
  // is a wall clock on the client, and the honest description of the state is "committed here,
  // unacknowledged, invisible to the caller" rather than "the write failed".
  step("DG-DR-03 confirming the inherited-sync trap");
  const trapStarted = Date.now();
  const blocked = trySh(STANDBY, [
    `timeout 10 psql -AtqX -d ${DATABASE} -c`,
    `"INSERT INTO ivr_dr_results VALUES ('RESULT-DR-TRAP','TASK-DR-01','IVR_CONFIRMED')"`,
  ].join(" "), "postgres");
  const trapSeconds = (Date.now() - trapStarted) / 1000;
  assert(
    !blocked.ok,
    "the promoted standby returned from a write while still requiring a synchronous standby. The "
    + "inherited-configuration trap this drill exists to prove has gone away, and the failover "
    + "script's second step is now unverified.");
  assert(
    trapSeconds >= 9,
    `the write failed after only ${trapSeconds.toFixed(1)}s, so it failed for some reason other `
    + `than blocking: ${blocked.output}`);

  // The runbook, executed rather than described. deploy/dr/failover.sh is what an operator runs,
  // so it is what the drill runs -- a runbook verified by a reimplementation of itself verifies
  // the reimplementation.
  step("DG-DR-03 running deploy/dr/failover.sh");
  // /tmp, not /: the standby container runs as postgres, so exec has no write access at the root.
  sh(STANDBY, "mkdir -p /tmp/scripts", "postgres");
  docker(["cp", path.join(repositoryRoot, "deploy/dr/failover.sh"), `${STANDBY}:/tmp/scripts/failover.sh`]);
  sh(STANDBY, "sed -i 's/\r$//' /tmp/scripts/failover.sh && chmod +x /tmp/scripts/failover.sh", "postgres");
  const failover = sh(STANDBY,
    `PGDATA=${DATA} IVR_DR_VERIFY_DATABASE=${DATABASE} sh /tmp/scripts/failover.sh`, "postgres");
  assert(failover.includes("DR_FAILOVER_CLEARING_SYNC"), `failover did not clear the inherited synchronous requirement: ${failover}`);
  assert(failover.includes("DR_FAILOVER_WRITE_OK"), `failover did not prove the node writes: ${failover}`);
  assert(failover.includes("DR_FAILOVER_OK"), `failover did not complete: ${failover}`);

  const writable = sh(STANDBY, [
    `psql -AtqX -d ${DATABASE} -c`,
    `"SET statement_timeout='15s'; INSERT INTO ivr_dr_results`,
    `VALUES ('RESULT-DR-AFTER','TASK-DR-01','IVR_CONFIRMED') RETURNING result_id"`,
  ].join(" "), "postgres").trim();
  assert.equal(writable, "RESULT-DR-AFTER", "the promoted standby still did not accept writes.");
  const rtoSeconds = (Date.now() - outageStarted) / 1000;

  assert(
    rtoSeconds <= RTO_BUDGET_SECONDS,
    `service was restored ${rtoSeconds.toFixed(1)}s after the kill, over the ${RTO_BUDGET_SECONDS}s budget.`);

  process.stdout.write(
    `DG-DR-03 PASS_SINGLE_HOST — RPO=0 (the commit taken immediately before SIGKILL survived); `
    + `promote at ${promoteSeconds.toFixed(1)}s, first successful write at ${rtoSeconds.toFixed(1)}s `
    + `(budget ${RTO_BUDGET_SECONDS}s). Between those two moments the node was out of recovery and `
    + `a write blocked in SyncRep for ${trapSeconds.toFixed(0)}s until the client gave up -- `
    + `statement_timeout does not rescue it, because the wait is at COMMIT. Promotion alone `
    + `restores nothing; deploy/dr/failover.sh does. NOT multi-AZ -- one host, two containers.
`);

  return { rtoSeconds };
}

// ------------------------------------------------------------- DG-RETENTION-04

async function backupsObeyRetention({ artefact }) {
  // Half one: the catalogue is pruned by age. Dry run first, because the default of a scheduled
  // delete has to be the safe one.
  // On the primary, and before DG-DR-03 kills it. Running the drills the other way round would
  // leave this one querying a container that no longer exists.
  step("DG-RETENTION-04 catalogue prune");

  // Two artefacts: one older than the limit, one inside it. The stamp is computed here and passed
  // absolutely, because busybox touch rejects the relative form GNU touch accepts and a drill that
  // depends on which coreutils the image happens to ship is a drill that breaks on a base bump.
  for (const [name, days] of [["ivr-full-old", 400], ["ivr-full-recent", 1]]) {
    const stamp = stampDaysAgo(days);
    for (const suffix of ["sql.enc", "iv", "hmac", "meta"]) {
      sh(PRIMARY, `printf 'x' > /backups/${name}.${suffix}`, "postgres");
    }
    sh(PRIMARY, `find /backups -name '${name}.*' -exec touch -t ${stamp} {} +`, "postgres");
  }

  const dryRun = sh(PRIMARY,
    "IVR_BACKUP_DIR=/backups IVR_BACKUP_MAX_AGE_DAYS=90 sh /scripts/prune.sh", "postgres");
  assert(dryRun.includes("PRUNE_WOULD_DELETE /backups/ivr-full-old.sql.enc"), dryRun);
  assert(!dryRun.includes("ivr-full-recent"), `the in-window artefact was listed: ${dryRun}`);
  const stillThere = sh(PRIMARY, "ls -1 /backups | grep -c 'ivr-full-old' || true", "postgres").trim();
  assert.equal(stillThere, "4", "the dry run deleted files.");

  const real = sh(PRIMARY,
    "IVR_BACKUP_DIR=/backups IVR_BACKUP_MAX_AGE_DAYS=90 IVR_BACKUP_DRY_RUN=false sh /scripts/prune.sh",
    "postgres");
  assert(real.includes("PRUNE_DELETED /backups/ivr-full-old.sql.enc"), real);
  const gone = sh(PRIMARY, "ls -1 /backups | grep -c 'ivr-full-old' || true", "postgres").trim();
  assert.equal(gone, "0", "the expired artefact and its siblings were not all removed.");
  const kept = sh(PRIMARY, "ls -1 /backups | grep -c 'ivr-full-recent' || true", "postgres").trim();
  assert.equal(kept, "4", "the in-window artefact was deleted.");

  // Half two, and the half that is usually missed. Pruning the catalogue proves nothing about the
  // data inside a backup that IS still in the window: restoring it brings back rows that expired
  // long ago. What makes that safe is that retain_until travels in the dump, so the retention job
  // still sees them as expired instead of treating the restore as a fresh start.
  const expired = psql(PRIMARY,
    "SELECT count(*) FROM ivr_dr_tasks WHERE retain_until < now()",
    { database: "ivr_restore_ok" });
  assert.equal(expired, "2", `restored rows lost their retention marks (found ${expired} expired).`);

  const notExpired = psql(PRIMARY,
    "SELECT count(*) FROM ivr_dr_tasks WHERE retain_until >= now()",
    { database: "ivr_restore_ok" });
  assert.equal(notExpired, "1", "the in-window row was not restored correctly.");

  process.stdout.write(
    "DG-RETENTION-04 PASS — expired artefacts pruned (dry run first, siblings included), and a "
    + `restored backup still carries retain_until on ${expired} expired rows so the retention job `
    + `can remove them (artefact ${path.basename(artefact)})\n`);
}

// ------------------------------------------------------------------------ main

try {
  await up();
  await inTransitEncryptionIsEnforced();
  const backup = await encryptedBackupRestores();
  await backupsObeyRetention(backup);
  await synchronousStandbyPromotes();

  process.stdout.write(
    "DR_SELFTEST_PASS_SINGLE_HOST=DG-DR-03 — the four drills ran against real PostgreSQL. "
    + "Multi-AZ and at-rest volume encryption remain NOT_RUN: both are cluster properties and no "
    + "cluster exists (W-0063). See docs/dr-topology.md.\n");
} finally {
  for (const cleanup of created.reverse()) {
    cleanup();
  }
  down();
}
