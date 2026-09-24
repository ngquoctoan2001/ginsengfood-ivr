import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";
import {
  RUN_SCHEMA, GATE_MANIFEST, TEST_COMMAND, SWEEP_COMMAND, EXTENDED_SWEEP_COMMAND, sha256, fullSweepVerdict,
  extendedSweepVerdict, validateRunEvidence, readPinnedArtifact, expectedTestAssemblies,
} from "./acceptance-evidence-lib.mjs";
import { OUTPUT_PREFIX, printsToken } from "./gate-sweep.mjs";
import { captureSource, collectAcceptanceEvidence } from "../../../tools/dev/collect-acceptance-evidence.mjs";

let checks = 0;
const source = { commit: "a".repeat(40), tree: "b".repeat(40), clean: true };
const manifest = { gates: {
  "a.mjs": { argv: [], expect: "A_PASS" }, "b.sh": { argv: ["--self-test"], expect: "B_PASS" },
  "library.mjs": { sweepable: false, reason: "library" },
} };
const manifestBytes = JSON.stringify(manifest);
const sweepLog = "  ok   a.mjs 0.1s  A_PASS\n  ok   b.sh 1.2s  B_PASS\nGATE_SWEEP_PASS 2/2 run, 1 skipped by manifest\n";
const startedAt = "2026-09-21T00:00:00.000Z", finishedAt = "2026-09-21T00:00:01.000Z";

function trx(start = startedAt, finish = finishedAt) {
  return `<TestRun><Times start="${start}" finish="${finish}" />
<TestDefinitions><UnitTest id="one"><TestMethod name="First" className="Ivr.UnitTests.XTests" codeBase="C:\\fixture\\Ivr.UnitTests.dll" /></UnitTest></TestDefinitions>
<Results><UnitTestResult testId="one" outcome="Passed" /></Results>
<ResultSummary outcome="Completed"><Counters total="1" executed="1" passed="1" /></ResultSummary></TestRun>`;
}

function fixture() {
  const artifacts = new Map([["trx/tests.trx", trx()], ["sweep.log", sweepLog]]);
  const pin = (name) => ({ path: name, sha256: sha256(artifacts.get(name)) });
  const run = (command) => ({ before: { ...source }, after: { ...source }, startedAt, finishedAt, command, exitCode: 0 });
  const bundle = {
    schema: RUN_SCHEMA, gateManifestSha256: sha256(manifestBytes),
    tests: { ...run(TEST_COMMAND), files: [pin("trx/tests.trx")] },
    sweep: { ...run(SWEEP_COMMAND), startedAt: finishedAt, file: pin("sweep.log") },
  };
  const options = { source, manifestBytes, assemblies: ["Ivr.UnitTests.dll"], readArtifact: (file) => {
    const text = artifacts.get(file.path);
    assert.equal(sha256(text), file.sha256, "hash mismatch");
    return text;
  } };
  return { bundle, artifacts, options };
}

function refused(label, mutate, pattern) {
  const value = fixture();
  mutate(value);
  assert.throws(() => validateRunEvidence(value.bundle, value.options), pattern, label);
  checks += 1;
}

assert.equal(validateRunEvidence(fixture().bundle, fixture().options).count, 1); checks += 1;
for (const log of [
  "GATE_SWEEP_PASS 1/1 run, 1 skipped by manifest",
  "GATE_SWEEP_PASS 0/0 run, 1 skipped by manifest",
  "GATE_SWEEP_PASS 2/2 run, 1 skipped by manifest", // count-only forgery
  sweepLog.replace("b.sh", "a.mjs").replace("B_PASS", "A_PASS"),
  sweepLog.replace("b.sh", "extra.sh"),
  sweepLog.replace("B_PASS", "WRONG_PASS"),
  sweepLog + sweepLog,
  sweepLog.replace("PASS 2/2", "FAIL 1/2"),
  sweepLog.replace("1 skipped", "0 skipped"),
]) {
  assert.equal(fullSweepVerdict(log, manifest).ok, false, log); checks += 1;
}
refused("old test commit", ({ bundle }) => { bundle.tests.before.commit = "c".repeat(40); }, /different commit/u);
refused("old sweep commit", ({ bundle }) => { bundle.sweep.before.commit = "c".repeat(40); }, /different commit/u);
refused("source changed during tests", ({ bundle }) => { bundle.tests.after.tree = "c".repeat(40); }, /different tree/u);
refused("dirty start", ({ bundle }) => { bundle.tests.before.clean = false; }, /dirty checkout/u);
refused("dirty end", ({ bundle }) => { bundle.sweep.after.clean = false; }, /dirty checkout/u);
refused("missing SHA", ({ bundle }) => { delete bundle.tests.before.commit; }, /full source commit/u);
refused("changed manifest", ({ bundle }) => { bundle.gateManifestSha256 = "c".repeat(64); }, /manifest differs/u);
refused("filtered test command", ({ bundle }) => { bundle.tests.command = [...TEST_COMMAND, "--filter", "First"]; }, /partial or unsupported/u);
refused("partial sweep command", ({ bundle }) => { bundle.sweep.command = [...SWEEP_COMMAND, "--only", "a"]; }, /partial or unsupported/u);
refused("test command failed", ({ bundle }) => { bundle.tests.exitCode = 1; }, /command failed/u);
refused("overlapping runs", ({ bundle }) => { bundle.sweep.startedAt = startedAt; }, /overlap/u);
refused("missing test project", ({ options }) => { options.assemblies.push("Ivr.ContractTests.dll"); }, /missing or extra/u);
refused("duplicated TRX", ({ bundle, options }) => {
  bundle.tests.files.push(bundle.tests.files[0]); options.assemblies.push("Ivr.ContractTests.dll");
}, /duplicate TRX/u);
refused("TRX bytes changed", ({ artifacts }) => { artifacts.set("trx/tests.trx", trx() + " "); }, /hash mismatch/u);
refused("sweep bytes changed", ({ artifacts }) => { artifacts.set("sweep.log", sweepLog + " "); }, /hash mismatch/u);
refused("old TRX even with freshly calculated hash", ({ bundle, artifacts }) => {
  const old = trx("2026-09-18T00:00:00Z", "2026-09-18T00:00:01Z");
  artifacts.set("trx/tests.trx", old); bundle.tests.files[0].sha256 = sha256(old);
}, /not produced during/u);
refused("failed/skipped result", ({ bundle, artifacts }) => {
  const skipped = trx().replace('outcome="Passed"', 'outcome="NotExecuted"');
  artifacts.set("trx/tests.trx", skipped); bundle.tests.files[0].sha256 = sha256(skipped);
}, /non-passing/u);
refused("wrong assembly", ({ options }) => { options.assemblies = ["Other.dll"]; }, /missing or duplicate test assembly/u);

// W-0352. The extended run: gates the offline sweep skips, each in its own prefixed section.
const extendedManifest = { gates: { ...manifest.gates,
  "c.sh": { sweepable: false, reason: "needs a network", extended: [
    { argv: [], expect: "C_PASS" }, { argv: ["--again"], expect: "C_PASS" }] } } };
const extendedManifestBytes = JSON.stringify(extendedManifest);
const extendedSweepLog = sweepLog.replace("1 skipped", "2 skipped");
const extendedLog = ["==== c.sh", `${OUTPUT_PREFIX}UT-C-01 PASS — first`, `${OUTPUT_PREFIX}building layer 3/9`,
  "  ok   c.sh 3.0s  C_PASS", "==== c.sh --again", `${OUTPUT_PREFIX}UT-C-02 PASS`,
  `${OUTPUT_PREFIX}UT-C-03 PASS_WITH_NOT_PROVEN=X`, "  ok   c.sh --again 1.0s  C_PASS", "EXTENDED_SWEEP_PASS 2/2 run", ""].join("\n");
const green = extendedSweepVerdict(extendedLog, extendedManifest);
assert.equal(green.ok, true, green.reason);
assert.deepEqual([...green.printed.get("c.sh")].sort(), ["UT-C-01", "UT-C-02"], "only a plain PASS line counts"); checks += 1;
assert.equal(extendedSweepVerdict(null, extendedManifest).ok, null); checks += 1;
for (const log of [
  extendedLog.replace("PASS 2/2", "PASS 1/1"),
  extendedLog.replace("  ok   c.sh --again", "  FAIL c.sh --again"),
  extendedLog.replace(/==== c\.sh --again[\s\S]*?--again 1\.0s {2}C_PASS\n/u, ""),
  extendedLog.replace("3.0s  C_PASS", "3.0s  WRONG_PASS"),
  extendedLog.replace(`${OUTPUT_PREFIX}UT-C-01`, "UT-C-01"),
  extendedLog.replace("==== c.sh --again", "==== c.sh --other"),
  extendedLog.replace("  ok   c.sh 3.0s  C_PASS\n", ""),
  extendedLog + extendedLog,
]) {
  assert.equal(extendedSweepVerdict(log, extendedManifest).ok, false, log); checks += 1;
}
assert.equal(extendedSweepVerdict(extendedLog, manifest).ok, false, "a manifest with no extended gates proves nothing"); checks += 1;
assert.deepEqual(["X_PASS", "X_PASS_WITH_NOT_PROVEN=Y", "X_PASSED", "X_PASS extra", "  X_PASS"].map((line) => printsToken(line, "X_PASS")),
  [true, false, false, true, false], "the sweep's own token check reads whole lines"); checks += 1;

function withExtended(value) {
  value.options.manifestBytes = extendedManifestBytes;
  value.bundle.gateManifestSha256 = sha256(extendedManifestBytes);
  value.artifacts.set("sweep.log", extendedSweepLog);
  value.bundle.sweep.file = { path: "sweep.log", sha256: sha256(extendedSweepLog) };
  value.artifacts.set("extended.log", extendedLog);
  value.bundle.extended = { before: { ...source }, after: { ...source }, startedAt: finishedAt,
    finishedAt: "2026-09-21T00:00:02.000Z", command: EXTENDED_SWEEP_COMMAND, exitCode: 0,
    file: { path: "extended.log", sha256: sha256(extendedLog) } };
  return value;
}
{
  const value = withExtended(fixture());
  const verified = validateRunEvidence(value.bundle, value.options);
  assert.equal(verified.extended.ok, true);
  assert(verified.extended.printed.get("c.sh").has("UT-C-01")); checks += 1;
  assert.equal(validateRunEvidence(fixture().bundle, fixture().options).extended, null, "the extended run is optional"); checks += 1;
}
for (const [label, mutate, pattern] of [
  ["extended from a dirty tree", ({ bundle }) => { bundle.extended.after.clean = false; }, /dirty checkout/u],
  ["extended at another commit", ({ bundle }) => { bundle.extended.before.commit = "c".repeat(40); }, /different commit/u],
  ["partial extended command", ({ bundle }) => { bundle.extended.command = [...SWEEP_COMMAND, "--only", "c"]; }, /partial or unsupported/u],
  ["extended command failed", ({ bundle }) => { bundle.extended.exitCode = 1; }, /extended command failed/u],
  ["extended overlaps the sweep", ({ bundle }) => { bundle.extended.startedAt = startedAt; }, /overlap/u],
  ["extended bytes changed", ({ artifacts }) => { artifacts.set("extended.log", `${extendedLog} `); }, /hash mismatch/u],
  ["extended gate failed", ({ bundle, artifacts }) => {
    const red = extendedLog.replace("  ok   c.sh --again", "  FAIL c.sh --again");
    artifacts.set("extended.log", red); bundle.extended.file.sha256 = sha256(red);
  }, /gate mở rộng không hợp lệ/u],
]) {
  const value = withExtended(fixture());
  mutate(value);
  assert.throws(() => validateRunEvidence(value.bundle, value.options), pattern, label); checks += 1;
}

// Real Git/CLI fixture: a separate throwaway repository, always main. No refs or files in the
// user's repository are touched. Synthetic test results are labelled and never used for acceptance.
const temporary = fs.mkdtempSync(path.join(os.tmpdir(), "ivr-acceptance-selftest-"));
const root = path.join(temporary, "repo"); fs.mkdirSync(root);
function command(executable, args, cwd = root) {
  const result = spawnSync(executable, args, { cwd, encoding: "utf8", windowsHide: true, maxBuffer: 16 << 20 });
  assert(!result.error, result.error?.message);
  return result;
}
function write(relative, content) {
  const target = path.join(root, relative); fs.mkdirSync(path.dirname(target), { recursive: true });
  fs.writeFileSync(target, content);
}
function git(args) {
  const result = command("git", args); assert.equal(result.status, 0, result.stderr); return result.stdout;
}
try {
  git(["init", "--initial-branch=main"]);
  git(["config", "user.name", "Acceptance fixture"]); git(["config", "user.email", "fixture@example.invalid"]);
  write(GATE_MANIFEST, manifestBytes);
  write("Ivr.sln", 'Project("{TEST}") = "Ivr.UnitTests", "tests\\Ivr.UnitTests\\Ivr.UnitTests.csproj", "{ONE}"\n');
  write("tests/Ivr.UnitTests/Ivr.UnitTests.csproj", '<Project><PackageReference Include="Microsoft.NET.Test.Sdk" /></Project>');
  write("tests/Ivr.UnitTests/XTests.cs", 'namespace Ivr.UnitTests; public class XTests { public void First() {} }');
  write("prompt/_execution/prompt-execution-tracker.md", '## 5. Planned implementation register\n| `W-0010` | `P2-1` | sample | — | TESTS_PASS | owner | files | tests | — |\n## 6. End\n');
  write("docs/release/gate-status.yaml", '  - id: "W-0010"\n    evidence: "docs/evidence/W-0010/README.md"\n');
  write("docs/traceability-tests.md", '| `UT-X` | 1 |\n| `UT-X-01` | unit | `First` | `tests/Ivr.UnitTests/XTests.cs` |\n');
  write("docs/evidence/W-0010/README.md", 'REAL_CUSTOMER_CALL_ALLOWED=NO\n[Required tests](test-report.md)\n');
  write("docs/evidence/W-0010/test-report.md", 'UT-X-01\n');
  const decisionPath = "docs/evidence/W-0099/README.md";
  const decisionText = "Synthetic owner decision: replace the retired assertion with UT-X-01.\n";
  write(decisionPath, decisionText);
  git(["add", "deploy", "Ivr.sln", "tests", "prompt", "docs"]); git(["commit", "-m", "test: synthetic acceptance fixture"]);
  const baseline = captureSource(root);
  const run = async (cmd, { output, logFile }) => {
    if (cmd === TEST_COMMAND) {
      fs.mkdirSync(path.join(output, "trx"));
      const now = new Date().toISOString();
      fs.writeFileSync(path.join(output, "trx/tests.trx"), trx(now, now));
      fs.writeFileSync(logFile, "SYNTHETIC TEST FIXTURE, not an IVR solution run\n");
    } else fs.writeFileSync(logFile, sweepLog);
    return 0;
  };
  const output = path.join(temporary, "valid");
  const target = await collectAcceptanceEvidence({ root, output, run }); checks += 1;
  const cli = fileURLToPath(new URL("./acceptance-batches.mjs", import.meta.url));
  const evaluate = (extra = []) => command(process.execPath, [cli, "--root", root, "--evidence", target, ...extra]);
  const valid = evaluate(); assert.equal(valid.status, 0, valid.stderr);
  assert(valid.stdout.includes(baseline.commit)); assert(valid.stdout.includes("**ĐẠT**")); checks += 1;
  assert.equal(evaluate(["--worktree"]).status, 1); checks += 1;
  assert.notEqual(command(process.execPath, [cli, "--root", root, "--trx", path.join(output, "trx")]).status, 0); checks += 1;
  const without = command(process.execPath, [cli, "--root", root]);
  assert(!without.stdout.includes("**ĐẠT**")); assert(without.stdout.includes("**CHƯA KIỂM**")); checks += 1;
  // Exercise the real pinned Git reader, not just a mocked retirement validator.
  const pinnedDecision = { commit: baseline.commit, path: decisionPath,
    sha256: sha256(decisionText), quote: decisionText.trim() };
  for (const [label, decision, expected] of [
    ["valid-retirement", pinnedDecision, "**ĐẠT**"],
    ["wrong-decision-hash", { ...pinnedDecision, sha256: "c".repeat(64) }, "hash mismatch"],
    ["wrong-decision-quote", { ...pinnedDecision, quote: "This statement does not occur in the decision." }, "quote is missing"],
    ["unknown-decision-commit", { ...pinnedDecision, commit: "d".repeat(40) }, "merge-base"],
    ["unsafe-decision-path", { ...pinnedDecision, path: "docs/../outside.md" }, "repository-relative"],
  ]) {
    write("docs/evidence/W-0010/acceptance-tests.json", JSON.stringify({
      schema: "ivr-acceptance-tests/v1", workId: "W-0010", scope: "software",
      retirements: [{ testId: "UT-OLD-01", reason: "The fixture's original requirement was replaced.",
        replacementTestIds: ["UT-X-01"], decision }],
    }));
    write("docs/evidence/W-0010/test-report.md", "UT-X-01 UT-OLD-01\n");
    git(["add", "docs/evidence/W-0010"]); git(["commit", "-m", `test: ${label}`]);
    const pinnedRun = await collectAcceptanceEvidence({ root, output: path.join(temporary, label), run });
    const checked = command(process.execPath, [cli, "--root", root, "--evidence", pinnedRun]);
    assert.equal(checked.status, 0, checked.stderr);
    assert(checked.stdout.includes(expected), checked.stdout);
    if (label !== "valid-retirement") assert(!checked.stdout.includes("**ĐẠT**"));
    checks += 1;
  }
  // mtime is neither an identity nor a signature. Touching an old commit's files cannot rescue it.
  write("new.txt", "second commit\n"); git(["add", "new.txt"]); git(["commit", "-m", "test: later fixture commit"]);
  fs.utimesSync(target, new Date(), new Date());
  const old = evaluate(); assert.equal(old.status, 1); assert(old.stdout.includes("different commit")); checks += 1;
  await assert.rejects(collectAcceptanceEvidence({ root, output, run }), /output already exists/u); checks += 1;
  for (const name of ["dirty", "drift", "failed", "missing", "partial"]) {
    const out = path.join(temporary, name);
    let calls = 0;
    const current = captureSource(root);
    const snapshot = () => {
      calls += 1;
      if (name === "dirty") return { ...current, clean: false };
      if (name === "drift" && calls >= 3) return { ...current, commit: "d".repeat(40) };
      return current;
    };
    const badRun = async (cmd, options) => {
      await run(cmd, options);
      if (name === "failed") return 1;
      if (name === "missing" && cmd === TEST_COMMAND) fs.unlinkSync(path.join(options.output, "trx/tests.trx"));
      if (name === "partial" && cmd === SWEEP_COMMAND) fs.writeFileSync(options.logFile, "GATE_SWEEP_PASS 1/1 run, 1 skipped by manifest\n");
      return 0;
    };
    await assert.rejects(collectAcceptanceEvidence({ root, output: out, snapshot, run: badRun }));
    assert(!fs.existsSync(path.join(out, "acceptance-run.json"))); checks += 1;
  }
  // W-0352. End to end: the collector records an extended run, and the list counts a claim of a
  // gate the offline sweep skips only through it.
  git(["rm", "-q", "docs/evidence/W-0010/acceptance-tests.json"]);
  write("docs/evidence/W-0010/test-report.md", "UT-X-01 CT-DOC-02\n");
  write(GATE_MANIFEST, JSON.stringify({ gates: { ...manifest.gates, "selftest-oasdiff.sh": { sweepable: false,
    reason: "needs the pinned oasdiff image", extended: [{ argv: [], expect: "CT-DOC-02 PASS" }] } } }));
  git(["add", "deploy", "docs"]); git(["commit", "-m", "test: a claim of an extended gate"]);
  const oasdiffLog = ["==== selftest-oasdiff.sh", `${OUTPUT_PREFIX}CT-DOC-02 PASS — synthetic fixture, no oasdiff ran`,
    "  ok   selftest-oasdiff.sh 2.0s  CT-DOC-02 PASS", "EXTENDED_SWEEP_PASS 1/1 run", ""].join("\n");
  const withGates = async (cmd, options) => {
    await run(cmd, options);
    if (cmd === SWEEP_COMMAND) fs.writeFileSync(options.logFile, extendedSweepLog);
    if (cmd === EXTENDED_SWEEP_COMMAND) fs.writeFileSync(options.logFile, oasdiffLog);
    return 0;
  };
  const extendedRun = await collectAcceptanceEvidence({ root, output: path.join(temporary, "extended"), extended: true, run: withGates });
  assert.deepEqual(JSON.parse(fs.readFileSync(extendedRun, "utf8")).extended.command, EXTENDED_SWEEP_COMMAND);
  const listed = command(process.execPath, [cli, "--root", root, "--evidence", extendedRun]);
  assert.equal(listed.status, 0, listed.stderr);
  assert(listed.stdout.includes("EXTENDED_SWEEP_PASS 1/1") && listed.stdout.includes("**ĐẠT**"), listed.stdout); checks += 1;
  const plainRun = await collectAcceptanceEvidence({ root, output: path.join(temporary, "no-extended"), run: withGates });
  const unlisted = command(process.execPath, [cli, "--root", root, "--evidence", plainRun]);
  assert(unlisted.stdout.includes("**CHƯA KIỂM**") && !unlisted.stdout.includes("**ĐẠT**"), unlisted.stdout);
  assert(unlisted.stdout.includes("--extended"), "the list says which run is missing"); checks += 1;
  const artifact = JSON.parse(fs.readFileSync(target, "utf8")).tests.files[0];
  assert.throws(() => readPinnedArtifact(output, { ...artifact, path: "../outside.trx" }), /unsafe/u); checks += 1;
  fs.appendFileSync(path.join(output, artifact.path), "changed");
  assert.throws(() => readPinnedArtifact(output, artifact), /hash mismatch/u); checks += 1;
} finally {
  const resolved = fs.realpathSync(temporary);
  assert(path.dirname(resolved).toLowerCase() === fs.realpathSync(os.tmpdir()).toLowerCase());
  assert(path.basename(resolved).startsWith("ivr-acceptance-selftest-"));
  fs.rmSync(resolved, { recursive: true, force: true });
}
process.stdout.write(`ACCEPTANCE_EVIDENCE_SELFTEST_PASS ${checks} checks (synthetic fixtures; no IVR acceptance)\n`);
