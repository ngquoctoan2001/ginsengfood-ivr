// W-0319. Provenance and completeness checks for local acceptance runs. These hashes detect
// accidental mixing or alteration; they are not a signature by a release owner.
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import fs from "node:fs";
import path from "node:path";

export const RUN_SCHEMA = "ivr-acceptance-run/v1";
export const GATE_MANIFEST = "deploy/ci/gate-invocations.json";
export const TEST_COMMAND = ["dotnet", "test", "Ivr.sln", "--logger", "trx", "--results-directory", "trx"];
export const SWEEP_COMMAND = ["node", "deploy/ci/scripts/gate-sweep.mjs"];
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
  const sweep = fullSweepVerdict(readArtifact(bundle.sweep.file), JSON.parse(manifestBytes));
  assert.equal(sweep.ok, true, sweep.reason);
  return { texts, count: summaries.reduce((sum, summary) => sum + summary.count, 0), sweep };
}
