// Run from a disposable Linux checkout with Node, .NET and sh available.
import assert from "node:assert/strict";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { spawnSync } from "node:child_process";

assert.equal(process.platform, "linux", "These executable stubs require a Linux checkout.");
const root = process.cwd();
const script = "deploy/ci/scripts/review-gate-selftest.mjs";
assert(fs.existsSync(path.join(root, script)));
const lookup = (name) => {
  const result = spawnSync("sh", ["-c", `command -v ${name}`], {encoding: "utf8"});
  assert.equal(result.status, 0);
  return result.stdout.trim();
};
const dotnet = lookup("dotnet");
const shell = lookup("sh");
const quote = (value) => `'${value.replaceAll("'", "'\\''")}'`;
const cases = [
  {name: "missing SDK", executable: "dotnet", body: "echo 'A compatible .NET SDK was not found.' >&2\nexit 155", expected: "CT-GATE-01: expected the semantic rejection"},
  {name: "compiler error", executable: "dotnet", body: "echo 'error CS0000: compiler failure' >&2\nexit 1", expected: "CT-GATE-01: expected the semantic rejection"},
  {name: "terminated tool", executable: "dotnet", body: "kill -TERM $$", expected: "Review gate could not execute dotnet"},
  {name: "executable absent", noPath: true, expected: "Review gate could not execute dotnet"},
  {name: "PII tool error", executable: "sh", body: `case "$1" in *scan-pii.sh) echo 'pattern file unavailable' >&2; exit 1 ;; esac\nexec ${quote(shell)} "$@"`, expected: "CT-GATE-02: expected the semantic rejection"},
  {name: "coverage tool error", executable: "dotnet", body: `if [ "$1" = run ]; then echo 'project unavailable' >&2; exit 1; fi\nexec ${quote(dotnet)} "$@"`, expected: "CT-GATE-03: expected the semantic rejection"},
  {name: "traceability tool error", executable: "node", body: "echo 'module unavailable' >&2\nexit 1", expected: "CT-GATE-04 empty description: expected the semantic rejection"},
  {name: "empty successful MR output", executable: "node", body: `case "$3" in *complete-mr.md) exit 0 ;; esac\nexec ${quote(process.execPath)} "$@"`, expected: "CT-GATE-04: a complete description was rejected"},
];

const temporary = fs.mkdtempSync(path.join(os.tmpdir(), "ivr-review-controls-"));
try {
  for (const [index, entry] of cases.entries()) {
    const bin = path.join(temporary, String(index));
    fs.mkdirSync(bin);
    if (entry.executable) fs.writeFileSync(path.join(bin, entry.executable), `#!${shell}\n${entry.body}\n`, {mode: 0o755});
    const result = spawnSync(process.execPath, [script], {
      cwd: root, encoding: "utf8", timeout: 120_000,
      env: {...process.env, NuGetAudit: "false", PATH: entry.noPath ? bin : `${bin}:${process.env.PATH}`},
    });
    const output = `${result.stdout ?? ""}\n${result.stderr ?? ""}`;
    assert.equal(result.status, 1, `${entry.name}: wrong status ${result.status}\n${output}`);
    assert(output.includes(entry.expected), `${entry.name}: wrong failure\n${output}`);
    assert(!output.includes("REVIEW_GATE_SELFTEST_PASS"), `${entry.name}: false green`);
    assert(!fs.existsSync(path.join(root, "src/Ivr.Domain/__gate_selftest.cs")), "planted source was not cleaned");
    console.log(`REJECTED ${entry.name}`);
  }
  const positive = spawnSync(process.execPath, [script], {
    cwd: root, encoding: "utf8", timeout: 120_000, env: {...process.env, NuGetAudit: "false"},
  });
  assert.equal(positive.status, 0, positive.stderr);
  assert(positive.stdout.includes("REVIEW_GATE_SELFTEST_PASS"));
  console.log(positive.stdout.trim());
  console.log(`REVIEW_ENVIRONMENT_REGRESSION_PASS negative=${cases.length} positive=1`);
} finally {
  fs.rmSync(temporary, {recursive: true, force: true});
}
