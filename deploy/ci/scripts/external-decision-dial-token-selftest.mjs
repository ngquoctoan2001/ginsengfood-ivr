#!/usr/bin/env node

// W-0184 — Focused, synthetic B5+C12/S-08 end-to-end self-test for the
// existing W-0164 routing, W-0165 response and W-0170 closure validators.
//
// This script never dispatches a message, verifies a real authority, reads a
// contact, writes an approval ledger or authorizes runtime/production use.

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
const DIAL_TOKEN_DECISION_PACK =
  "plan/ivr-orther/m8-10-contact-dial-token-production-decision-pack-2026-09-03.md";
const DIAL_TOKEN_CLOSURE_TICKET =
  "docs/contracts/target-v1-closure-pack/T-04-dial-token.md";
const DISPATCH_PACK =
  "plan/ivr-orther/m8-12-external-decision-provenance-dispatch-pack-2026-09-03.md";

const RECEIPT_ID = "RECEIPT:D-02-DIAL-TOKEN-SYNTHETIC-01";
const ROUTED_RECIPIENT = "DIAL_TOKEN_DECISION_OWNER_GROUP";
const AUTHORITY_GROUPS = Object.freeze([
  { group: "M3_PRODUCER", decisions: ["DTK-01", "DTK-02"] },
  { group: "SECURITY", decisions: ["DTK-03", "DTK-04", "DTK-05"] },
  { group: "PLATFORM", decisions: ["DTK-06", "DTK-07"] },
  { group: "TELEPHONY_VENDOR", decisions: ["DTK-08", "DTK-09"] },
  { group: "PRODUCT", decisions: ["DTK-10", "DTK-11"] },
  { group: "LEGAL_PRIVACY", decisions: ["DTK-12", "DTK-13"] },
  { group: "RELEASE", decisions: ["DTK-14", "DTK-15"] },
]);
const DIAL_TOKEN_DECISIONS = Object.freeze(
  Array.from({ length: 15 }, (_, index) =>
    `DTK-${String(index + 1).padStart(2, "0")}`,
  ),
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

function buildDialTokenRoutingFixture() {
  const document = readJson(ROUTING_TEMPLATE);
  document.status = "PARTIAL_READY";
  const batch = document.batches.find((candidate) => candidate.batch === "D-02");
  if (!batch) fail("routing template has no D-02 batch");
  Object.assign(batch, {
    recipient_identity: ROUTED_RECIPIENT,
    role_organization: "DIAL TOKEN CROSS FUNCTIONAL DECISION OWNERS",
    authority_source_ref: "ROLE_ASSIGNMENT:DIAL-TOKEN-D-02-OWNERS",
    channel_kind: "GITLAB_ISSUE",
    destination_ref: "PROJECT:EXTERNAL-GOVERNANCE/D-02",
    due_at: "2026-09-10T17:00:00+07:00",
    dispatch_authorized_by: "M8_OWNER",
    dispatch_authorized_at: "2026-09-04T08:00:00+07:00",
    state: "READY_FOR_HASH_RECHECK_AND_DISPATCH",
  });
  return document;
}

function buildDialTokenResponseFixture() {
  const manifest = parseManifest();
  const acceptedArtifacts = [
    DIAL_TOKEN_DECISION_PACK,
    DIAL_TOKEN_CLOSURE_TICKET,
    DISPATCH_PACK,
  ].map((path) => {
    const artifactHash = manifest.get(path);
    if (!artifactHash) fail(`manifest is missing ${path}`);
    return { path, sha256: artifactHash };
  });
  const document = readJson(RESPONSE_TEMPLATE);
  document.status = "RESPONSE_RECEIVED_PENDING_VALIDATION";
  document.responses = AUTHORITY_GROUPS.map(({ group, decisions }, index) => ({
    response_id: `RESPONSE:S-08-${group}-SYNTHETIC-01`,
    dispatch_batch: "D-02",
    dispatch_receipt_ref: RECEIPT_ID,
    sheet_id: "S-08",
    decision_ids: ["S-08", ...decisions],
    decision: "APPROVE",
    decision_text:
      `Explicit synthetic S-08 approval by ${group} for W-0184 validation only; ` +
      "it does not represent an external decision.",
    signer_identity_alias: `${group}_SIGNER`,
    signer_role: `${group} accountable decision owner`,
    signer_organization: `DIAL TOKEN ${group} organization alias`,
    authority_source_ref: `ROLE_ASSIGNMENT:DIAL-TOKEN-${group}`,
    accepted_artifacts: clone(acceptedArtifacts),
    responded_at: `2026-09-04T10:0${index}:00+07:00`,
    received_at: `2026-09-04T10:1${index}:00+07:00`,
    scope_environments: ["CONTRACT"],
    effective_cutover: {
      effective_at: "2026-09-05T00:00:00+07:00",
      cutover_at: "2026-09-06T00:00:00+07:00",
      compatibility_window:
        "Keep contact resolution disabled until signed implementation and shared E2E evidence pass.",
    },
    rollback_or_rejection_path:
      "Keep production resolver and telephony egress disabled; continue fail closed.",
    evidence_references: [`TICKET:DIAL-TOKEN-${group}-SYNTHETIC-EVIDENCE`],
    residual_blockers: [],
    external_response_ref: `RESPONSE:DIAL-TOKEN-${group}-SYNTHETIC-ARTIFACT`,
    external_response_sha256: `${index + 1}`.repeat(64),
    limitations: ["SYNTHETIC_W0184_SELF_TEST_ONLY"],
    state: "RECEIVED_UNVERIFIED",
  }));
  return document;
}

function buildDialTokenClosureFixture() {
  const evidenceHashCharacters = "bcdef12";
  const document = readJson(CLOSURE_TEMPLATE);
  document.status = "CLOSURE_REVIEW_PENDING";
  document.dispatch_receipts = [
    {
      receipt_id: RECEIPT_ID,
      batch: "D-02",
      routing_input_sha256: "PENDING_CASE_HASH",
      system_of_record_kind: "GITLAB_ISSUE",
      destination_ref: "PROJECT:EXTERNAL-GOVERNANCE/D-02",
      system_of_record_ref: "TICKET:D-02-DIAL-TOKEN-SYNTHETIC-01",
      external_receipt_sha256: "a".repeat(64),
      sender_identity_alias: "M8_DISPATCHER",
      recipient_identity_aliases: [ROUTED_RECIPIENT],
      sent_at: "2026-09-04T09:00:00+07:00",
      delivered_at: "2026-09-04T09:01:00+07:00",
      delivery_state: "DELIVERED",
    },
  ];
  document.authority_attestations = AUTHORITY_GROUPS.map(({ group }, index) => ({
    attestation_id: `ATTESTATION:S-08-${group}-SYNTHETIC-01`,
    response_id: `RESPONSE:S-08-${group}-SYNTHETIC-01`,
    sheet_id: "S-08",
    authority_group: group,
    authority_source_ref: `ROLE_ASSIGNMENT:DIAL-TOKEN-${group}`,
    authority_evidence_ref: `ROLE_ASSIGNMENT:DIAL-TOKEN-${group}`,
    authority_evidence_sha256: evidenceHashCharacters[index].repeat(64),
    verified_by_alias: "CHIEF_AUDITOR",
    verified_at: "2026-09-04T11:00:00+07:00",
    state: "AUTHORITY_VERIFIED",
  }));
  document.sheet_closures = [
    {
      sheet_id: "S-08",
      required_authority_groups: AUTHORITY_GROUPS.map(({ group }) => group),
      accepted_response_ids: AUTHORITY_GROUPS.map(
        ({ group }) => `RESPONSE:S-08-${group}-SYNTHETIC-01`,
      ),
      receipt_ids: [RECEIPT_ID],
      decision_ids: ["S-08", ...DIAL_TOKEN_DECISIONS],
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

function runDialTokenS08ClosureSelfTest() {
  mkdirSync(ARTIFACT_ROOT, { recursive: true });
  const temporaryDirectory = mkdtempSync(
    resolve(ARTIFACT_ROOT, "w0184-dial-token-selftest-"),
  );
  let refusals = 0;
  try {
    const baseRouting = buildDialTokenRoutingFixture();
    const baseResponse = buildDialTokenResponseFixture();
    const baseClosure = buildDialTokenClosureFixture();
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
      "DECISION_PROVENANCE_CLOSURE_VALID_NO_GATE_PROMOTION sheets=1 sheet_ids=S-08",
    );

    const refusalCases = [
      [
        "missing-authority-attestation",
        (_routing, _response, closure) => closure.authority_attestations.pop(),
      ],
      [
        "missing-dtk-decision",
        (_routing, _response, closure) => closure.sheet_closures[0].decision_ids.pop(),
      ],
      [
        "wrong-s08-batch",
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
              blocker_id: "DIAL-TOKEN-SYNTHETIC-BLOCKER",
              owner_alias: "SECURITY_OWNER",
              description:
                "Synthetic unresolved condition used only to prove fail-closed closure.",
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
          closure.authority_attestations[3].authority_group = "ORDER_CORE";
        },
      ],
      [
        "missing-t04-artifact",
        (_routing, response) => {
          response.responses[4].accepted_artifacts =
            response.responses[4].accepted_artifacts.filter(
              ({ path }) => path !== DIAL_TOKEN_CLOSURE_TICKET,
            );
        },
      ],
      [
        "wrong-receipt-batch",
        (_routing, _response, closure) => {
          closure.dispatch_receipts[0].batch = "D-03";
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
      decisions: DIAL_TOKEN_DECISIONS.length,
    };
  } finally {
    if (temporaryDirectory.startsWith(`${ARTIFACT_ROOT}${sep}`)) {
      rmSync(temporaryDirectory, { recursive: true, force: true });
    }
  }
}

function main(argv) {
  if (argv.length !== 1 || argv[0] !== "--self-test") {
    fail(
      "Usage: node deploy/ci/scripts/external-decision-dial-token-selftest.mjs --self-test",
    );
  }
  const result = runDialTokenS08ClosureSelfTest();
  console.log(
    `W0184_DIAL_TOKEN_SELFTEST_PASS valid=${result.valid} refusals=${result.refusals} ` +
      `authorities=${result.authorities} decisions=${result.decisions}`,
  );
}

try {
  main(process.argv.slice(2));
} catch (error) {
  console.error(`W0184_DIAL_TOKEN_SELFTEST_FAILED: ${error.message}`);
  process.exitCode = 1;
}
