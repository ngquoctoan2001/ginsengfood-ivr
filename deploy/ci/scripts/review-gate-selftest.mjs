import { spawnSync } from "node:child_process";
import fs from "node:fs/promises";
import os from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";

// W-0038 / P5-4 §8. Proves the review gate REJECTS four kinds of violation.
//
// A gate that has only ever been seen green is a gate nobody knows the shape of. Each check here
// feeds the real tool a deliberately bad input and fails if the tool is happy with it — so the
// gate is verified from the direction that matters.

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const repositoryRoot = path.resolve(scriptDirectory, "../../..");
const temporaryRoot = await fs.mkdtemp(path.join(os.tmpdir(), "ivr-review-gate-"));

function run(command, commandArguments, options = {}) {
  return spawnSync(command, commandArguments, {
    cwd: repositoryRoot,
    encoding: "utf8",
    shell: false,
    ...options,
  });
}

function assert(condition, message) {
  if (!condition) {
    throw new Error(message);
  }
}

try {
  // CT-GATE-01 — a D-02 order-transition symbol must be rejected.
  // The fail-gate suite scans src/ for names that would only exist if IVR wrote order state.
  // Plant one and require the scan to notice.
  const plantedFile = path.join(repositoryRoot, "src", "Ivr.Domain", "__gate_selftest.cs");
  await fs.writeFile(
    plantedFile,
    "namespace Ivr.Domain;\n\n"
      + "// Deliberate D-02 violation planted by review-gate-selftest.mjs.\n"
      + "internal static class GateSelfTest\n{\n"
      + "    public static void SetOrderState(string value) { _ = value; }\n}\n",
    "utf8",
  );
  let orderTransitionRejected = false;
  try {
    const scan = run("dotnet", [
      "test",
      "tests/Ivr.UnitTests/Ivr.UnitTests.csproj",
      "--nologo",
      "--filter",
      "TestId=IT-FAILGATE-01",
    ]);
    orderTransitionRejected = scan.status !== 0;
  } finally {
    await fs.rm(plantedFile, { force: true });
  }

  assert(orderTransitionRejected, "CT-GATE-01: a planted order-transition symbol was not rejected.");

  // CT-GATE-02 — a raw phone number in a scanned artifact must be rejected.
  const piiDirectory = path.join(temporaryRoot, "pii");
  await fs.mkdir(piiDirectory, { recursive: true });
  await fs.writeFile(
    path.join(piiDirectory, "leak.txt"),
    "operator note: called 0912345678 and confirmed\n",
    "utf8",
  );
  const piiScan = run("sh", ["deploy/ci/scripts/scan-pii.sh", piiDirectory]);
  assert(piiScan.status !== 0, "CT-GATE-02: a raw phone number passed the PII scan.");

  // CT-GATE-03 — coverage below the gate must fail. Uses the committed low fixture so the
  // check exercises the same tool and the same report shape CI uses.
  const coverage = run("dotnet", [
    "run",
    "--project",
    "deploy/ci/tools/Ivr.CiPolicy/Ivr.CiPolicy.csproj",
    "--",
    "coverage",
    "deploy/ci/fixtures/coverage/low",
    "80",
  ]);
  assert(coverage.status !== 0, "CT-GATE-03: a coverage report below the gate was accepted.");

  // CT-GATE-04 — a merge request without traceability must be blocked, and a complete one
  // must pass. Both directions matter: a checker that rejects everything is not a gate either.
  const emptyMr = run("node", [
    "deploy/ci/scripts/check-mr-traceability.mjs",
    "--text",
    "Fixed a thing.",
  ]);
  assert(emptyMr.status !== 0, "CT-GATE-04: a description with no traceability was accepted.");

  const templateMr = run("node", [
    "deploy/ci/scripts/check-mr-traceability.mjs",
    "--file",
    ".gitlab/merge_request_templates/Default.md",
  ]);
  assert(
    templateMr.status !== 0,
    "CT-GATE-04: the unfilled template itself was accepted as traceable.",
  );

  const completeMrPath = path.join(temporaryRoot, "complete-mr.md");
  await fs.writeFile(
    completeMrPath,
    [
      "| specs/testing/08 D-02 | N/A — no contract change | `IT-FAILGATE-01` via `dotnet test` | `docs/evidence/W-0038/` | NONE |",
      "",
      "Work ID: `W-0038`",
      "",
      "Prompt ID: `P5-4`",
      "",
      "- [x] Source spec path and requirement/decision ID are supplied.",
      "- [x] No direct order transition, payment/revenue processing, or customer notification was added.",
    ].join("\n"),
    "utf8",
  );
  const completeMr = run("node", [
    "deploy/ci/scripts/check-mr-traceability.mjs",
    "--file",
    completeMrPath,
  ]);
  assert(
    completeMr.status === 0,
    `CT-GATE-04: a complete description was rejected. ${completeMr.stderr}`,
  );

  process.stdout.write("CT-GATE-01 PASS — a planted order-transition symbol is rejected\n");
  process.stdout.write("CT-GATE-02 PASS — a raw phone number fails the PII scan\n");
  process.stdout.write("CT-GATE-03 PASS — coverage below the gate fails\n");
  process.stdout.write("CT-GATE-04 PASS — an untraceable merge request is blocked\n");
  process.stdout.write("REVIEW_GATE_SELFTEST_PASS\n");
} finally {
  await fs.rm(temporaryRoot, { recursive: true, force: true });
}
