#!/usr/bin/env node
// W-0354. The hosted `openapi_lint` job runs Redocly over both contracts; the local gate sweep did
// not. From draft.29 (W-0307, 2026-09-16) to draft.33 every hosted pipeline was red on three
// `nullable: true` keywords that OpenAPI 3.1 does not have, while the local sweep - the one the
// acceptance collector records - reported every gate green. Nobody opened the hosted results, and
// nothing local could have told them. This gate runs the same lint, with the same config, on the
// same two files.
//
// A lint that has never been seen to fail proves nothing, so the gate also runs it where it must
// fail: the invalid fixture the hosted job uses for its own self-test, and a copy of the live IVR
// spec with the exact defect re-introduced. If either of those comes back clean, the gate is red.
import { spawnSync } from "node:child_process";
import { mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import os from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";

const REPOSITORY_ROOT = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../../..");
const CLI = path.join(REPOSITORY_ROOT, "deploy/ci/node_modules/@redocly/cli/bin/cli.js");
const CONFIG = path.join(REPOSITORY_ROOT, ".redocly.yaml");
const IVR_SPEC = "specs/api/openapi/ivr-order-confirmation.v1.yaml";
const SPECS = [IVR_SPEC, "specs/api/openapi/order-core-ivr-callback.target-v1.yaml"];
const INVALID_FIXTURE = "deploy/ci/fixtures/openapi/invalid-openapi.yaml";

function lint(files) {
  const result = spawnSync(process.execPath, [CLI, "lint", ...files, "--config", CONFIG], {
    cwd: REPOSITORY_ROOT,
    encoding: "utf8",
    maxBuffer: 32 * 1024 * 1024,
  });
  if (result.error) {
    throw result.error;
  }
  return { status: result.status, output: `${result.stdout ?? ""}${result.stderr ?? ""}` };
}

function fail(message, output = "") {
  process.stdout.write(`OPENAPI_LINT_FAIL — ${message}\n`);
  if (output) {
    process.stdout.write(`${output.trimEnd()}\n`);
  }
  process.exit(1);
}

// 1. The live contracts, exactly as the hosted job lints them.
const live = lint(SPECS);
if (live.status !== 0) {
  fail("Redocly reports errors in the live contracts; the hosted openapi_lint job fails on these.", live.output);
}

// 2. The fixture the hosted job's own self-test feeds it.
const fixture = lint([INVALID_FIXTURE]);
if (fixture.status === 0) {
  fail(`${INVALID_FIXTURE} linted clean; this gate cannot tell a broken contract from a good one.`);
}

// 3. The defect that stayed red for nine days, put back into a copy of the live spec. Any string
// property written on one line will do; the first one found is made "nullable" the OpenAPI 3.0 way.
const source = readFileSync(path.join(REPOSITORY_ROOT, IVR_SPEC), "utf8");
const target = /^(\s+[a-z_]+): \{ type: string \}\r?$/mu;
if (!target.test(source)) {
  fail(`no one-line string property left in ${IVR_SPEC} to build the negative control from.`);
}
const scratch = mkdtempSync(path.join(os.tmpdir(), "ivr-openapi-lint-"));
try {
  const mutated = path.join(scratch, "ivr-order-confirmation.nullable.yaml");
  writeFileSync(mutated, source.replace(target, "$1: { type: string, nullable: true }"));
  const regression = lint([mutated]);
  if (regression.status === 0 || !/nullable/u.test(regression.output)) {
    fail("a spec with `nullable: true` in OpenAPI 3.1 linted clean; the W-0307 defect would not be caught.", regression.output);
  }
} finally {
  rmSync(scratch, { recursive: true, force: true });
}

process.stdout.write(
  "OPENAPI_LINT_PASS — both contracts lint clean under .redocly.yaml, and the lint rejects the "
    + "invalid fixture and a 3.0-style `nullable` put back into the IVR spec\n",
);
