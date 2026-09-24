// W-0319. Provenance and completeness checks for local acceptance runs. These hashes detect
// accidental mixing or alteration; they are not a signature by a release owner.
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import fs from "node:fs";
import path from "node:path";
import { extendedInvocations, OUTPUT_PREFIX } from "./gate-sweep.mjs";

export const RUN_SCHEMA = "ivr-acceptance-run/v1";
export const GATE_MANIFEST = "deploy/ci/gate-invocations.json";
export const TEST_COMMAND = ["dotnet", "test", "Ivr.sln", "--logger", "trx", "--results-directory", "trx"];
export const SWEEP_COMMAND = ["node", "deploy/ci/scripts/gate-sweep.mjs"];
// W-0352. Optional third run: the gates the offline sweep skips because they need images or a network.
export const EXTENDED_SWEEP_COMMAND = ["node", "deploy/ci/scripts/gate-sweep.mjs", "--extended"];
export const sha256 = (bytes) => createHash("sha256").update(bytes).digest("hex");

export function assertRunSource(actual, expected) {
  assert.match(actual?.commit ?? "", /^[0-9a-f]{40}$/u, "missing full source commit");
  assert.match(actual?.tree ?? "", /^[0-9a-f]{40}$/u, "missing source tree");
  assert.equal(actual.commit, expected.commit, "evidence belongs to a different commit");
  assert.equal(actual.tree, expected.tree, "evidence belongs to a different tree");
  assert.equal(actual.clean, true, "evidence was collected from a dirty checkout");
}

export function fullSweepVerdict(log, manifest) {
  if (log === null) return { ok: null, reason: "chưa có log gate sweep" };
  try {
    assert(manifest?.gates && typeof manifest.gates === "object", "missing gate manifest at the reviewed commit");
    const runnable = Object.entries(manifest.gates).filter(([, gate]) => Array.isArray(gate.argv));
    const skipped = Object.keys(manifest.gates).length - runnable.length;
    assert(runnable.length > 0, "manifest has no runnable gates");
    const lines = log.trim().split(/\r?\n/u);
    const summaries = lines.filter((line) => line.startsWith("GATE_SWEEP_"));
    const expected = `GATE_SWEEP_PASS ${runnable.length}/${runnable.length} run, ${skipped} skipped by manifest`;
    assert.deepEqual(summaries, [expected], "sweep is incomplete, failed, or contains multiple runs");
    assert.equal(lines.at(-1), expected, "sweep summary is not the final line");
    assert(!lines.some((line) => /^\s*FAIL\s/u.test(line)), "sweep contains a failed gate");
    const passed = lines.filter((line) => /^\s*ok\s/u.test(line)).map((line) => {
      const match = /^\s*ok\s+(\S+)\s+\d+(?:\.\d+)?s\s+(.+)$/u.exec(line);
      assert(match, "malformed gate result");
      const [, name, token] = match;
      assert.equal(token, manifest.gates[name]?.expect, `wrong pass token for ${name}`);
      return name;
    });
    assert.deepEqual(passed.sort(), runnable.map(([name]) => name).sort(), "missing, duplicate, or unexpected gate results");
    return { ok: true, reason: `GATE_SWEEP_PASS ${runnable.length}/${runnable.length}` };
  } catch (error) {
    return { ok: false, reason: `gate sweep không hợp lệ: ${error.message.split("\n")[0]}` };
  }
}

/**
 * W-0352. The extended run of `gate-sweep.mjs --extended`: every invocation in the manifest, in
 * order, each in its own section that ends with its own result line. Returns the TestIds each gate
 * printed as "<TestId> PASS" inside its own sections, which is all C2 reads from this log.
 */
export function extendedSweepVerdict(log, manifest) {
  if (log === null) return { ok: null, reason: "chưa có lượt gate mở rộng", printed: new Map() };
  try {
    assert(manifest?.gates && typeof manifest.gates === "object", "missing gate manifest at the reviewed commit");
    const invocations = extendedInvocations(manifest.gates);
    assert(invocations.length > 0, "manifest has no extended gates");
    const lines = log.trim().split(/\r?\n/u);
    const expected = `EXTENDED_SWEEP_PASS ${invocations.length}/${invocations.length} run`;
    assert.deepEqual(lines.filter((line) => line.startsWith("EXTENDED_SWEEP_")), [expected],
      "extended sweep is incomplete, failed, or contains multiple runs");
    assert.equal(lines.at(-1), expected, "extended sweep summary is not the final line");
    const printed = new Map();
    const closed = [];
    let open = null;
    for (const line of lines.slice(0, -1)) {
      if (line.startsWith(OUTPUT_PREFIX)) {
        assert(open, "gate output outside a gate section");
        const id = /^([A-Z][A-Z0-9]*(?:-[A-Z0-9]+)+[a-z]?) PASS(?:\s|$)/u.exec(line.slice(OUTPUT_PREFIX.length));
        if (id) printed.set(open.name, new Set([...(printed.get(open.name) ?? []), id[1]]));
        continue;
      }
      const header = /^==== (.+)$/u.exec(line);
      if (header) {
        assert(!open, `section ${open?.label} has no result line`);
        open = invocations[closed.length];
        assert.equal(header[1], open?.label, "missing, extra, or reordered extended gate");
        continue;
      }
      // The sweep's own notes, such as taking over a lock a crashed run left behind.
      if (line.startsWith("  ---- ") && !open) continue;
      const result = /^\s*(ok|FAIL)\s+(.+?)\s+\d+(?:\.\d+)?s\s+(.+)$/u.exec(line);
      assert(result, `unexpected line in extended sweep: ${line.slice(0, 80)}`);
      assert(open && result[2] === open.label, "a result line does not close its own section");
      assert.equal(result[1], "ok", `extended gate failed: ${open.label}`);
      assert.equal(result[3], open.entry.expect, `wrong pass token for ${open.label}`);
      closed.push(open.label);
      open = null;
    }
    assert.deepEqual(closed, invocations.map((invocation) => invocation.label),
      "missing, duplicate, or reordered extended gate results");
    return { ok: true, reason: expected.replace(" run", ""), printed };
  } catch (error) {
    return { ok: false, reason: `lượt gate mở rộng không hợp lệ: ${error.message.split("\n")[0]}`, printed: new Map() };
  }
}

export function expectedTestAssemblies(solution, readSource) {
  const assemblies = [];
  for (const match of solution.matchAll(/^Project\([^\n]+? = "([^"]+)", "([^"]+\.csproj)"/gmu)) {
    const project = readSource(match[2].replaceAll("\\", "/"));
    if (/Microsoft\.NET\.Test\.Sdk|<IsTestProject>\s*true\s*<\/IsTestProject>/u.test(project)) {
      assemblies.push((/<AssemblyName>([^<]+)<\/AssemblyName>/u.exec(project)?.[1] ?? match[1]) + ".dll");
    }
  }
  assert(assemblies.length > 0, "solution has no test projects");
  return assemblies.sort();
}

function xmlAttributes(text) {
  return Object.fromEntries([...text.matchAll(/([\w:]+)="([^"]*)"/gu)].map((match) => [match[1], match[2]]));
}

function instant(value) {
  assert.equal(typeof value, "string", "missing run timestamp");
  const result = Date.parse(value);
  assert(Number.isFinite(result), "invalid run timestamp");
  return result;
}

export function checkTrx(xml, run) {
  const times = xmlAttributes(/<Times\b([^>]*)\/?>/u.exec(xml)?.[1] ?? "");
  const start = instant(times.start), finish = instant(times.finish);
  assert(start >= instant(run.startedAt) && finish >= start && finish <= instant(run.finishedAt),
    "TRX was not produced during this test run");
  const counters = xmlAttributes(/<Counters\b([^>]*)\/?>/u.exec(xml)?.[1] ?? "");
  const total = Number(counters.total);
  assert(Number.isInteger(total) && total > 0, "TRX contains no tests");
  assert.equal(Number(counters.executed), total, "TRX has unexecuted tests");
  assert.equal(Number(counters.passed), total, "TRX has failed or skipped tests");
  const results = [...xml.matchAll(/<UnitTestResult\b([^>]*)\/?>/gu)].map((match) => xmlAttributes(match[1]));
  assert.equal(results.length, total, "TRX counter/result mismatch");
  assert(results.every((result) => result.outcome === "Passed"), "TRX contains non-passing results");
  const definitions = new Map([...xml.matchAll(/<UnitTest\b([^>]*)>([\s\S]*?)<\/UnitTest>/gu)]
    .map((match) => [xmlAttributes(match[1]).id, xmlAttributes(/<TestMethod\b([^>]*)\/?>/u.exec(match[2])?.[1] ?? "")]));
  const assemblies = new Set(results.map((result) => {
    const definition = definitions.get(result.testId);
    assert(definition?.name && definition.className && definition.codeBase, "TRX result has no test method definition");
    return path.posix.basename(definition.codeBase.replaceAll("\\", "/"));
  }));
  assert.equal(assemblies.size, 1, "TRX mixes test assemblies");
  return { assembly: [...assemblies][0], count: total };
}

export function readPinnedArtifact(directory, artifact) {
  assert(artifact && typeof artifact.path === "string", "missing artifact path");
  assert(!artifact.path.includes("\\") && !artifact.path.includes(":") && !path.posix.isAbsolute(artifact.path)
    && artifact.path.split("/").every((part) => part && part !== "." && part !== ".."), "unsafe artifact path");
  const base = fs.realpathSync(directory);
  const file = fs.realpathSync(path.join(base, artifact.path));
  const relative = path.relative(base, file);
  assert(relative && !relative.startsWith("..") && !path.isAbsolute(relative), "artifact escapes bundle");
  const bytes = fs.readFileSync(file);
  assert.equal(sha256(bytes), artifact.sha256, `artifact hash mismatch: ${artifact.path}`);
  return bytes.toString("utf8");
}

// Both producers and consumers use this check. No mtime comparison: copying a file may change its
// mtime, but must not change which commit, invocation, or test interval its bytes prove.
export function validateRunEvidence(bundle, { source, manifestBytes, assemblies, readArtifact }) {
  assert.equal(bundle.schema, RUN_SCHEMA, "missing or unsupported acceptance run manifest");
  assert.equal(bundle.gateManifestSha256, sha256(manifestBytes), "gate manifest differs from the reviewed commit");
  for (const [name, command] of [["tests", TEST_COMMAND], ["sweep", SWEEP_COMMAND]]) {
    const run = bundle[name];
    assert(run, `missing ${name} run`);
    assertRunSource(run.before, source);
    assertRunSource(run.after, source);
    assert.deepEqual(run.command, command, `${name} used a partial or unsupported command`);
    assert.equal(run.exitCode, 0, `${name} command failed`);
    assert(instant(run.finishedAt) >= instant(run.startedAt), "invalid run interval");
  }
  assert(instant(bundle.sweep.startedAt) >= instant(bundle.tests.finishedAt), "tests and sweep overlap");
  const files = bundle.tests.files;
  assert(Array.isArray(files) && files.length === assemblies.length, "missing or extra test project results");
  assert.equal(new Set(files.map((file) => file.path)).size, files.length, "duplicate TRX artifacts");
  const texts = files.map((file) => {
    assert(file.path.endsWith(".trx"), "test artifact is not TRX");
    return readArtifact(file);
  });
  const summaries = texts.map((text) => checkTrx(text, bundle.tests));
  assert.deepEqual(summaries.map((summary) => summary.assembly).sort(), [...assemblies].sort(), "missing or duplicate test assembly");
  const manifest = JSON.parse(manifestBytes);
  const sweep = fullSweepVerdict(readArtifact(bundle.sweep.file), manifest);
  assert.equal(sweep.ok, true, sweep.reason);
  // W-0352. The extended run is optional; a bundle that has one must have a sound one.
  let extended = null;
  if (bundle.extended !== undefined) {
    const run = bundle.extended;
    assertRunSource(run?.before, source);
    assertRunSource(run.after, source);
    assert.deepEqual(run.command, EXTENDED_SWEEP_COMMAND, "extended used a partial or unsupported command");
    assert.equal(run.exitCode, 0, "extended command failed");
    assert(instant(run.finishedAt) >= instant(run.startedAt), "invalid run interval");
    assert(instant(run.startedAt) >= instant(bundle.sweep.finishedAt), "sweep and extended gates overlap");
    extended = extendedSweepVerdict(readArtifact(run.file), manifest);
    assert.equal(extended.ok, true, extended.reason);
  }
  return { texts, count: summaries.reduce((sum, summary) => sum + summary.count, 0), sweep, extended };
}
