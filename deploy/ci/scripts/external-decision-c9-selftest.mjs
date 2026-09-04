#!/usr/bin/env node

// W-0179 — Focused, synthetic C9/S-06 end-to-end self-test for the existing
// W-0164 routing, W-0165 response and W-0170 closure validators.
//
// This script never dispatches a message, verifies a real authority, writes an
// approval ledger or authorizes runtime/production use.

import { createHash } from "node:crypto";
import {
  mkdirSync,
  mkdtempSync,
  readFileSync,
  rmSync,
  writeFileSync,
} from "node:fs";
import { dirname, relative, resolve, sep } from "node:path";
import { fileURLToPath } from "node:url";
import { spawnSync } from "node:child_process";

const SCRIPT_PATH = fileURLToPath(import.meta.url);
const REPOSITORY_ROOT = resolve(dirname(SCRIPT_PATH), "../../..");
const ARTIFACT_ROOT = resolve(REPOSITORY_ROOT, "ci-artifacts");

const ROUTING_VALIDATOR = "deploy/ci/scripts/external-decision-routing-validator.mjs";
const RESPONSE_VALIDATOR = "deploy/ci/scripts/external-decision-response-validator.mjs";
const CLOSURE_VALIDATOR = "deploy/ci/scripts/external-decision-closure-validator.mjs";
const ROUTING_TEMPLATE = "docs/evidence/W-0164/recipient-routing-input.template.json";
const RESPONSE_TEMPLATE = "docs/evidence/W-0165/decision-response-input.template.json";
const CLOSURE_TEMPLATE = "docs/evidence/W-0170/decision-closure-input.template.json";
const MANIFEST = "docs/evidence/W-0170/artifact-sha256.txt";
const C9_DECISION_PACK =
  "plan/ivr-orther/m8-08-opt-out-suppression-decision-pack-2026-09-03.md";
const DISPATCH_PACK =
  "plan/ivr-orther/m8-12-external-decision-provenance-dispatch-pack-2026-09-03.md";

const RECEIPT_ID = "RECEIPT:D-03-C9-SYNTHETIC-01";
const ROUTED_RECIPIENT = "C9_DECISION_OWNER_GROUP";
const AUTHORITY_GROUPS = Object.freeze([
  { group: "PROJECT_OWNER", decisions: ["OPT-01", "OPT-02"] },
  { group: "CRM_M31", decisions: ["OPT-03", "OPT-04", "OPT-05"] },
  { group: "M3_CONTRACT", decisions: ["OPT-06", "OPT-07"] },
  { group: "LEGAL_PRIVACY", decisions: ["OPT-08", "OPT-09"] },
  { group: "PRODUCT", decisions: ["OPT-10", "OPT-11"] },
]);
const OPT_DECISIONS = Object.freeze(
  Array.from({ length: 11 }, (_, index) => `OPT-${String(index + 1).padStart(2, "0")}`),
);

function fail(message) {
  throw new Error(message);
}

function clone(value) {
  return JSON.parse(JSON.stringify(value));
}

function sha256(bytes) {
  return createHash("sha256").update(bytes).digest("hex");
}

function readJson(repositoryPath) {
  return JSON.parse(readFileSync(resolve(REPOSITORY_ROOT, repositoryPath), "utf8"));
}

function parseManifest() {
  return new Map(
    readFileSync(resolve(REPOSITORY_ROOT, MANIFEST), "utf8")
      .trim()
      .split(/\r?\n/u)
      .map((line) => {
        const match = /^([0-9a-f]{64})  (.+)$/u.exec(line);
        if (!match) fail(`invalid manifest line: ${line}`);
        return [match[2], match[1]];
      }),
  );
}

function repositoryRelative(pathValue) {
  const pathRelative = relative(REPOSITORY_ROOT, pathValue);
  if (
    pathRelative === "" ||
    pathRelative === ".." ||
    pathRelative.startsWith(`..${sep}`)
  ) {
    fail(`path escaped repository root: ${pathValue}`);
  }
  return pathRelative.replaceAll("\\", "/");
}

function writeJson(pathValue, document) {
  const bytes = Buffer.from(`${JSON.stringify(document, null, 2)}\n`, "utf8");
  writeFileSync(pathValue, bytes, { flag: "wx" });
  return sha256(bytes);
}

function runValidator(scriptRelativePath, inputRelativePath, expectedPrefix) {
  const result = spawnSync(
    process.execPath,
    [resolve(REPOSITORY_ROOT, scriptRelativePath), "--input", inputRelativePath],
    { cwd: REPOSITORY_ROOT, encoding: "utf8", windowsHide: true },
  );
  const output = `${result.stdout ?? ""}${result.stderr ?? ""}`.trim();
  if (result.status !== 0 || !output.includes(expectedPrefix)) {
    fail(`${scriptRelativePath} failed: ${output || `exit ${result.status}`}`);
  }
  return output;
}

function expectClosureRefusal(inputRelativePath, label) {
  const result = spawnSync(
    process.execPath,
    [resolve(REPOSITORY_ROOT, CLOSURE_VALIDATOR), "--input", inputRelativePath],
    { cwd: REPOSITORY_ROOT, encoding: "utf8", windowsHide: true },
  );
  const output = `${result.stdout ?? ""}${result.stderr ?? ""}`.trim();
  if (result.status === 0 || !output.includes("W0170_VALIDATION_FAILED:")) {
    fail(`mutation ${label} was not refused: ${output || `exit ${result.status}`}`);
  }
}

function buildC9RoutingFixture() {
  const document = readJson(ROUTING_TEMPLATE);
  document.status = "PARTIAL_READY";
  const batch = document.batches.find((candidate) => candidate.batch === "D-03");
  if (!batch) fail("routing template has no D-03 batch");
  Object.assign(batch, {
    recipient_identity: ROUTED_RECIPIENT,
    role_organization: "C9 CROSS FUNCTIONAL DECISION OWNERS",
    authority_source_ref: "ROLE_ASSIGNMENT:C9-D-03-OWNERS",
    channel_kind: "GITLAB_ISSUE",
    destination_ref: "PROJECT:EXTERNAL-GOVERNANCE/D-03",
    due_at: "2026-09-10T17:00:00+07:00",
    dispatch_authorized_by: "M8_OWNER",
    dispatch_authorized_at: "2026-09-04T08:00:00+07:00",
    state: "READY_FOR_HASH_RECHECK_AND_DISPATCH",
  });
  return document;
}

function buildC9ResponseFixture() {
  const manifest = parseManifest();
  const acceptedArtifacts = [C9_DECISION_PACK, DISPATCH_PACK].map((path) => {
    const artifactHash = manifest.get(path);
    if (!artifactHash) fail(`manifest is missing ${path}`);
    return { path, sha256: artifactHash };
  });
  const document = readJson(RESPONSE_TEMPLATE);
  document.status = "RESPONSE_RECEIVED_PENDING_VALIDATION";
  document.responses = AUTHORITY_GROUPS.map(({ group, decisions }, index) => ({
    response_id: `RESPONSE:S-06-${group}-SYNTHETIC-01`,
    dispatch_batch: "D-03",
    dispatch_receipt_ref: RECEIPT_ID,
    sheet_id: "S-06",
    decision_ids: ["S-06", ...decisions],
    decision: "APPROVE",
    decision_text:
      `Explicit synthetic S-06 approval by ${group} for W-0179 validation only; ` +
      "it does not represent an external decision.",
    signer_identity_alias: `${group}_SIGNER`,
    signer_role: `${group} accountable decision owner`,
    signer_organization: `C9 ${group} organization alias`,
    authority_source_ref: `ROLE_ASSIGNMENT:C9-${group}`,
    accepted_artifacts: clone(acceptedArtifacts),
    responded_at: `2026-09-04T10:0${index}:00+07:00`,
    received_at: `2026-09-04T10:1${index}:00+07:00`,
    scope_environments: ["CONTRACT"],
    effective_cutover: {
      effective_at: "2026-09-05T00:00:00+07:00",
      cutover_at: "2026-09-06T00:00:00+07:00",
      compatibility_window:
        "Keep the C9 runtime disabled until signed implementation and shared E2E evidence pass.",
    },
    rollback_or_rejection_path:
      "Keep suppression egress disabled and retain inbound call restriction fail closed.",
    evidence_references: [`TICKET:C9-${group}-SYNTHETIC-EVIDENCE`],
    residual_blockers: [],
    external_response_ref: `RESPONSE:C9-${group}-SYNTHETIC-ARTIFACT`,
    external_response_sha256: `${index + 1}`.repeat(64),
    limitations: ["SYNTHETIC_W0179_SELF_TEST_ONLY"],
    state: "RECEIVED_UNVERIFIED",
  }));
  return document;
}

function buildC9ClosureFixture() {
  const document = readJson(CLOSURE_TEMPLATE);
  document.status = "CLOSURE_REVIEW_PENDING";
  document.dispatch_receipts = [
    {
      receipt_id: RECEIPT_ID,
      batch: "D-03",
      routing_input_sha256: "PENDING_CASE_HASH",
      system_of_record_kind: "GITLAB_ISSUE",
      destination_ref: "PROJECT:EXTERNAL-GOVERNANCE/D-03",
      system_of_record_ref: "TICKET:D-03-C9-SYNTHETIC-01",
      external_receipt_sha256: "a".repeat(64),
      sender_identity_alias: "M8_DISPATCHER",
      recipient_identity_aliases: [ROUTED_RECIPIENT],
      sent_at: "2026-09-04T09:00:00+07:00",
      delivered_at: "2026-09-04T09:01:00+07:00",
      delivery_state: "DELIVERED",
    },
  ];
  document.authority_attestations = AUTHORITY_GROUPS.map(({ group }, index) => ({
    attestation_id: `ATTESTATION:S-06-${group}-SYNTHETIC-01`,
    response_id: `RESPONSE:S-06-${group}-SYNTHETIC-01`,
    sheet_id: "S-06",
    authority_group: group,
    authority_source_ref: `ROLE_ASSIGNMENT:C9-${group}`,
    authority_evidence_ref: `ROLE_ASSIGNMENT:C9-${group}`,
    authority_evidence_sha256: "bcdef"[index].repeat(64),
    verified_by_alias: "CHIEF_AUDITOR",
    verified_at: "2026-09-04T11:00:00+07:00",
    state: "AUTHORITY_VERIFIED",
  }));
  document.sheet_closures = [
    {
      sheet_id: "S-06",
      required_authority_groups: AUTHORITY_GROUPS.map(({ group }) => group),
      accepted_response_ids: AUTHORITY_GROUPS.map(
        ({ group }) => `RESPONSE:S-06-${group}-SYNTHETIC-01`,
      ),
      receipt_ids: [RECEIPT_ID],
      decision_ids: ["S-06", ...OPT_DECISIONS],
      state: "DECISION_PROVENANCE_CLOSED",
    },
  ];
  return document;
}

function materializeCase(rootDirectory, label, routing, response, closure) {
  const caseDirectory = resolve(rootDirectory, label);
  mkdirSync(caseDirectory);

  const routingPath = resolve(caseDirectory, "routing.json");
  const routingHash = writeJson(routingPath, routing);
  const responsePath = resolve(caseDirectory, "responses.json");
  const responseHash = writeJson(responsePath, response);

  closure.routing_input = {
    path: repositoryRelative(routingPath),
    sha256: routingHash,
  };
  closure.response_bundle = {
    path: repositoryRelative(responsePath),
    sha256: responseHash,
  };
  closure.dispatch_receipts.forEach((receipt) => {
    receipt.routing_input_sha256 = routingHash;
  });

  const closurePath = resolve(caseDirectory, "closure.json");
  writeJson(closurePath, closure);
  return {
    routing: repositoryRelative(routingPath),
    response: repositoryRelative(responsePath),
    closure: repositoryRelative(closurePath),
  };
}

function runC9SuppressionClosureSelfTest() {
  mkdirSync(ARTIFACT_ROOT, { recursive: true });
  const temporaryDirectory = mkdtempSync(resolve(ARTIFACT_ROOT, "w0179-c9-selftest-"));
  let refusals = 0;
  try {
    const baseRouting = buildC9RoutingFixture();
    const baseResponse = buildC9ResponseFixture();
    const baseClosure = buildC9ClosureFixture();
    const positive = materializeCase(
      temporaryDirectory,
      "positive",
      clone(baseRouting),
      clone(baseResponse),
      clone(baseClosure),
    );
    runValidator(ROUTING_VALIDATOR, positive.routing, "ROUTING_INPUT_VALID");
    runValidator(
      RESPONSE_VALIDATOR,
      positive.response,
      "RESPONSE_PROVENANCE_VALID_AUTHORITY_UNVERIFIED",
    );
    runValidator(
      CLOSURE_VALIDATOR,
      positive.closure,
      "DECISION_PROVENANCE_CLOSURE_VALID_NO_GATE_PROMOTION sheets=1 sheet_ids=S-06",
    );

    const refusalCases = [
      [
        "missing-authority-attestation",
        (_routing, _response, closure) => closure.authority_attestations.pop(),
      ],
      [
        "missing-opt-decision",
        (_routing, _response, closure) => closure.sheet_closures[0].decision_ids.pop(),
      ],
      [
        "wrong-s06-batch",
        (_routing, response) => {
          response.responses[0].dispatch_batch = "D-01";
        },
      ],
      [
        "conditional-approval",
        (_routing, response) => {
          response.responses[1].decision = "APPROVE_WITH_CONDITIONS";
          response.responses[1].residual_blockers = [
            {
              blocker_id: "C9-SYNTHETIC-BLOCKER",
              owner_alias: "CRM_M31_OWNER",
              description: "Synthetic unresolved condition used only to prove fail-closed closure.",
              target_at: "2026-09-09T17:00:00+07:00",
            },
          ];
        },
      ],
      [
        "signer-verifier-collision",
        (_routing, response, closure) => {
          closure.authority_attestations[2].verified_by_alias =
            response.responses[2].signer_identity_alias;
        },
      ],
      [
        "wrong-authority-group",
        (_routing, _response, closure) => {
          closure.authority_attestations[4].authority_group = "SECURITY";
        },
      ],
    ];

    refusalCases.forEach(([label, mutate]) => {
      const routing = clone(baseRouting);
      const response = clone(baseResponse);
      const closure = clone(baseClosure);
      mutate(routing, response, closure);
      const candidate = materializeCase(
        temporaryDirectory,
        label,
        routing,
        response,
        closure,
      );
      expectClosureRefusal(candidate.closure, label);
      refusals += 1;
    });

    return {
      valid: 1,
      refusals,
      authorities: AUTHORITY_GROUPS.length,
      decisions: OPT_DECISIONS.length,
    };
  } finally {
    if (temporaryDirectory.startsWith(`${ARTIFACT_ROOT}${sep}`)) {
      rmSync(temporaryDirectory, { recursive: true, force: true });
    }
  }
}

function main(argv) {
  if (argv.length !== 1 || argv[0] !== "--self-test") {
    fail("Usage: node deploy/ci/scripts/external-decision-c9-selftest.mjs --self-test");
  }
  const result = runC9SuppressionClosureSelfTest();
  console.log(
    `W0179_C9_SELFTEST_PASS valid=${result.valid} refusals=${result.refusals} ` +
      `authorities=${result.authorities} decisions=${result.decisions}`,
  );
}

try {
  main(process.argv.slice(2));
} catch (error) {
  console.error(`W0179_C9_SELFTEST_FAILED: ${error.message}`);
  process.exitCode = 1;
}
