#!/usr/bin/env node

import { createHash } from "node:crypto";
import {
  lstatSync,
  mkdtempSync,
  readFileSync,
  realpathSync,
  rmSync,
  writeFileSync,
} from "node:fs";
import { dirname, isAbsolute, join, relative, resolve, sep } from "node:path";
import { fileURLToPath } from "node:url";

const SCRIPT_PATH = fileURLToPath(import.meta.url);
const REPOSITORY_ROOT = resolve(dirname(SCRIPT_PATH), "../../..");
const MAX_INPUT_BYTES = 256 * 1024;
const MAX_SOURCE_BYTES = 5 * 1024 * 1024;
const SCHEMA_VERSION = "m8-external-decision-response-bundle.v1";
const WORK_ID = "W-0165";
const PLACEHOLDER = "PENDING_EXTERNAL_RESPONSE";
const NOT_APPLICABLE = "NOT_APPLICABLE";
const SOURCE_PINS = Object.freeze({
  dispatch_pack_path:
    "plan/ivr-orther/m8-12-external-decision-provenance-dispatch-pack-2026-09-03.md",
  dispatch_pack_sha256:
    "9da8e5698bc99df73338b3d6886e61f18c93e492431d07cb730074f6ef3aa499",
  artifact_manifest_path: "docs/evidence/W-0170/artifact-sha256.txt",
  artifact_manifest_sha256:
    "3352479690e424b88138654b1a91aa5c55908b19d47ee63870795b113e616471",
});

const DISPATCH_PACK = SOURCE_PINS.dispatch_pack_path;
const ARTIFACTS = Object.freeze({
  OD18: "plan/ivr-orther/questions-to-module-3-od18-authority.md",
  M805: "plan/ivr-orther/m8-05-program-result-contract-signoff-2026-09-03.md",
  M806: "plan/ivr-orther/m8-06-upstream-session-trace-signoff-2026-09-03.md",
  M807: "plan/ivr-orther/m8-07-target-v1-shared-callback-handoff-2026-09-03.md",
  M808: "plan/ivr-orther/m8-08-opt-out-suppression-decision-pack-2026-09-03.md",
  M809: "plan/ivr-orther/m8-09-revoke-freshness-decision-pack-2026-09-03.md",
  M810: "plan/ivr-orther/m8-10-contact-dial-token-production-decision-pack-2026-09-03.md",
  M811: "plan/ivr-orther/m8-11-attempt-policy-production-decision-pack-2026-09-03.md",
  W0128: "docs/evidence/W-0128/README.md",
  W0135: "docs/evidence/W-0135/README.md",
  T01: "docs/contracts/target-v1-closure-pack/T-01-program-matrix.md",
  T04: "docs/contracts/target-v1-closure-pack/T-04-dial-token.md",
  T09: "docs/contracts/target-v1-closure-pack/T-09-attempt-policy.md",
  V03: "docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md",
  R00: "docs/contracts/telephony-procurement-pack/R-00-voice-gateway-rfq.md",
  R06: "docs/contracts/telephony-procurement-pack/R-06-to-trinh-mua-thiet-bi.md",
});
const SHEET_RULES = new Map([
  ["S-01", { batches: ["D-01"], artifacts: [ARTIFACTS.OD18, DISPATCH_PACK] }],
  ["S-02", { batches: ["D-01"], artifacts: [ARTIFACTS.M805, ARTIFACTS.T01, DISPATCH_PACK] }],
  ["S-03", { batches: ["D-02"], artifacts: [ARTIFACTS.W0128, DISPATCH_PACK] }],
  ["S-04", { batches: ["D-01"], artifacts: [ARTIFACTS.M806, DISPATCH_PACK] }],
  ["S-05", { batches: ["D-01", "D-02"], artifacts: [ARTIFACTS.M807, DISPATCH_PACK] }],
  ["S-06", { batches: ["D-03"], artifacts: [ARTIFACTS.M808, DISPATCH_PACK] }],
  ["S-07", { batches: ["D-01"], artifacts: [ARTIFACTS.M809, DISPATCH_PACK] }],
  ["S-08", { batches: ["D-02"], artifacts: [ARTIFACTS.M810, ARTIFACTS.T04, DISPATCH_PACK] }],
  ["S-09", { batches: ["D-04"], artifacts: [ARTIFACTS.M811, ARTIFACTS.T09, DISPATCH_PACK] }],
  [
    "S-11",
    {
      batches: ["D-05"],
      artifacts: [ARTIFACTS.W0135, ARTIFACTS.V03, ARTIFACTS.R00, ARTIFACTS.R06, DISPATCH_PACK],
    },
  ],
]);
const KNOWN_DECISION_IDS = new Map([
  ["S-01", new Set(Array.from({ length: 5 }, (_, index) => `OD18-C${index + 1}`))],
  ["S-06", new Set(Array.from({ length: 11 }, (_, index) => `OPT-${String(index + 1).padStart(2, "0")}`))],
  ["S-07", new Set(Array.from({ length: 12 }, (_, index) => `RVK-${String(index + 1).padStart(2, "0")}`))],
  ["S-08", new Set(Array.from({ length: 15 }, (_, index) => `DTK-${String(index + 1).padStart(2, "0")}`))],
  ["S-09", new Set(Array.from({ length: 15 }, (_, index) => `ATP-${String(index + 1).padStart(2, "0")}`))],
]);
const ALLOWED_DECISIONS = new Set(["APPROVE", "APPROVE_WITH_CONDITIONS", "REJECT", "NEEDS_REVISION"]);
const ALLOWED_SCOPES = new Set(["CONTRACT", "LAB", "STAGING", "PRODUCTION", "PROCUREMENT"]);

const ROOT_KEYS = ["schema_version", "work_id", "status", "source", "responses", "safety"];
const SOURCE_KEYS = Object.keys(SOURCE_PINS);
const RESPONSE_KEYS = [
  "response_id",
  "dispatch_batch",
  "dispatch_receipt_ref",
  "sheet_id",
  "decision_ids",
  "decision",
  "decision_text",
  "signer_identity_alias",
  "signer_role",
  "signer_organization",
  "authority_source_ref",
  "accepted_artifacts",
  "responded_at",
  "received_at",
  "scope_environments",
  "effective_cutover",
  "rollback_or_rejection_path",
  "evidence_references",
  "residual_blockers",
  "external_response_ref",
  "external_response_sha256",
  "limitations",
  "state",
];
const ARTIFACT_KEYS = ["path", "sha256"];
const EFFECTIVE_KEYS = ["effective_at", "cutover_at", "compatibility_window"];
const BLOCKER_KEYS = ["blocker_id", "owner_alias", "description", "target_at"];
const SAFETY_KEYS = [
  "contains_personal_contact_details",
  "contains_credentials_or_secrets",
  "raw_external_response_embedded",
  "external_authority_verified",
  "approval_ledger_updated",
  "production_gate_promoted",
  "real_customer_call_allowed",
];

function fail(message) {
  throw new Error(message);
}

function sha256(bytes) {
  return createHash("sha256").update(bytes).digest("hex");
}

function assertExactKeys(value, expected, label) {
  if (value === null || typeof value !== "object" || Array.isArray(value)) {
    fail(`${label} must be an object`);
  }
  const actual = Object.keys(value).sort();
  const wanted = [...expected].sort();
  if (actual.length !== wanted.length || actual.some((key, index) => key !== wanted[index])) {
    fail(`${label} keys must be exactly: ${wanted.join(", ")}`);
  }
}

function isConfined(path) {
  const pathRelative = relative(REPOSITORY_ROOT, path);
  return (
    pathRelative !== "" &&
    pathRelative !== ".." &&
    !pathRelative.startsWith(`..${sep}`) &&
    !isAbsolute(pathRelative)
  );
}

function readConfinedBytes(inputPath, maximumBytes) {
  const resolvedPath = resolve(REPOSITORY_ROOT, inputPath);
  if (!isConfined(resolvedPath)) fail("path must stay inside the repository root");
  const entry = lstatSync(resolvedPath);
  if (!entry.isFile() || entry.isSymbolicLink()) fail("path must be a regular non-symlink file");
  if (entry.size > maximumBytes) fail(`file exceeds ${maximumBytes} bytes`);
  const realPath = realpathSync(resolvedPath);
  if (!isConfined(realPath)) fail("resolved path escapes the repository root");
  return readFileSync(realPath);
}

function readConfinedUtf8Input(inputPath) {
  const bytes = readConfinedBytes(inputPath, MAX_INPUT_BYTES);
  if (bytes.length >= 3 && bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf) {
    fail("UTF-8 BOM is not allowed");
  }
  try {
    return { bytes, text: new TextDecoder("utf-8", { fatal: true }).decode(bytes) };
  } catch {
    fail("input must be strict UTF-8");
  }
}

function rejectDuplicateJsonKeys(textValue) {
  let position = 0;
  const skipWhitespace = () => {
    while (/\s/u.test(textValue[position] ?? "")) position += 1;
  };
  const parseString = () => {
    if (textValue[position] !== '"') fail("invalid JSON string");
    const start = position;
    position += 1;
    while (position < textValue.length) {
      if (textValue[position] === "\\") {
        position += 2;
        continue;
      }
      if (textValue[position] === '"') {
        position += 1;
        try {
          return JSON.parse(textValue.slice(start, position));
        } catch {
          fail("invalid JSON string escape");
        }
      }
      if (textValue.charCodeAt(position) < 0x20) fail("invalid control character in JSON string");
      position += 1;
    }
    fail("unterminated JSON string");
  };
  const parseLiteral = () => {
    const match = /^(?:true|false|null|-?(?:0|[1-9]\d*)(?:\.\d+)?(?:[eE][+-]?\d+)?)/u.exec(
      textValue.slice(position),
    );
    if (!match) fail("invalid JSON value");
    position += match[0].length;
  };
  const parseArray = () => {
    position += 1;
    skipWhitespace();
    if (textValue[position] === "]") {
      position += 1;
      return;
    }
    while (position < textValue.length) {
      parseValue();
      skipWhitespace();
      if (textValue[position] === "]") {
        position += 1;
        return;
      }
      if (textValue[position] !== ",") fail("invalid JSON array separator");
      position += 1;
      skipWhitespace();
    }
    fail("unterminated JSON array");
  };
  const parseObject = () => {
    position += 1;
    const keys = new Set();
    skipWhitespace();
    if (textValue[position] === "}") {
      position += 1;
      return;
    }
    while (position < textValue.length) {
      const key = parseString();
      if (keys.has(key)) fail(`duplicate JSON key: ${key}`);
      keys.add(key);
      skipWhitespace();
      if (textValue[position] !== ":") fail("invalid JSON object separator");
      position += 1;
      skipWhitespace();
      parseValue();
      skipWhitespace();
      if (textValue[position] === "}") {
        position += 1;
        return;
      }
      if (textValue[position] !== ",") fail("invalid JSON object separator");
      position += 1;
      skipWhitespace();
    }
    fail("unterminated JSON object");
  };
  function parseValue() {
    skipWhitespace();
    const token = textValue[position];
    if (token === "{") parseObject();
    else if (token === "[") parseArray();
    else if (token === '"') parseString();
    else parseLiteral();
  }
  parseValue();
  skipWhitespace();
  if (position !== textValue.length) fail("unexpected content after JSON document");
}

function parseInput(inputPath) {
  const { bytes, text } = readConfinedUtf8Input(inputPath);
  rejectDuplicateJsonKeys(text);
  let document;
  try {
    document = JSON.parse(text);
  } catch (error) {
    fail(`malformed JSON: ${error.message}`);
  }
  return { bytes, document };
}

function parseAndVerifyManifest() {
  const manifestBytes = readConfinedBytes(SOURCE_PINS.artifact_manifest_path, MAX_SOURCE_BYTES);
  if (sha256(manifestBytes) !== SOURCE_PINS.artifact_manifest_sha256) {
    fail("artifact manifest drifted from the pinned SHA-256");
  }
  const text = new TextDecoder("utf-8", { fatal: true }).decode(manifestBytes);
  const entries = new Map();
  for (const line of text.split(/\r?\n/u).filter(Boolean)) {
    const match = /^([0-9a-f]{64})  ([^\r\n]+)$/u.exec(line);
    if (!match) fail("artifact manifest contains a malformed line");
    if (entries.has(match[2])) fail(`artifact manifest contains duplicate path ${match[2]}`);
    entries.set(match[2], match[1]);
  }
  if (entries.size !== 18) fail("artifact manifest must contain exactly 18 entries");
  for (const [path, expectedHash] of entries) {
    const bytes = readConfinedBytes(path, MAX_SOURCE_BYTES);
    if (sha256(bytes) !== expectedHash) fail(`${path} drifted from the artifact manifest`);
  }
  return entries;
}

function verifySourcePins(source) {
  assertExactKeys(source, SOURCE_KEYS, "source");
  for (const key of SOURCE_KEYS) {
    if (source[key] !== SOURCE_PINS[key]) fail(`source.${key} does not match the pinned value`);
  }
  const dispatchBytes = readConfinedBytes(DISPATCH_PACK, MAX_SOURCE_BYTES);
  if (sha256(dispatchBytes) !== SOURCE_PINS.dispatch_pack_sha256) {
    fail("M8-12 dispatch pack drifted from the pinned SHA-256");
  }
  return parseAndVerifyManifest();
}

function assertString(value, label, minimum, maximum) {
  if (typeof value !== "string" || value.length < minimum || value.length > maximum) {
    fail(`${label} must be a string of ${minimum}..${maximum} characters`);
  }
  if (value.trim() !== value) fail(`${label} must not have surrounding whitespace`);
  if (/[\u0000-\u001f\u007f]/u.test(value)) fail(`${label} contains a control character`);
}

function assertIdentifier(value, label) {
  assertString(value, label, 3, 160);
  if (!/^[A-Z0-9][A-Z0-9._:/-]+$/u.test(value)) fail(`${label} must be an uppercase alias/reference`);
}

function assertNoSensitiveValue(value, label) {
  if (/[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}/iu.test(value)) fail(`${label} contains an email-like value`);
  if (/(?:^|\D)(?:\+?\d[\s().-]*){9,15}(?:$|\D)/u.test(value)) fail(`${label} contains a phone-like value`);
  if (/\b\d{1,5}\s+(?:đường|duong|phố|pho|street|st\.?|road|rd\.?|avenue|ave\.?)\b/iu.test(value)) {
    fail(`${label} contains a street-address-like value`);
  }
  if (
    /(?:password|passwd|bearer\s+|api[_ -]?key|access[_ -]?token|private[_ -]?key|client[_ -]?secret)\s*[:=]?/iu.test(value) ||
    /\beyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\b/u.test(value)
  ) {
    fail(`${label} contains credential- or secret-like material`);
  }
}

function assertSafeText(value, label, minimum = 5, maximum = 500) {
  assertString(value, label, minimum, maximum);
  assertNoSensitiveValue(value, label);
}

function assertTimestamp(value, label) {
  assertString(value, label, 20, 35);
  if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,3})?(?:Z|[+-]\d{2}:\d{2})$/u.test(value)) {
    fail(`${label} must be ISO-8601 with an explicit timezone`);
  }
  if (!Number.isFinite(Date.parse(value))) fail(`${label} is not a valid timestamp`);
}

function assertStringArray(value, label, minimum, maximum) {
  if (!Array.isArray(value) || value.length < minimum || value.length > maximum) {
    fail(`${label} must contain ${minimum}..${maximum} values`);
  }
  const unique = new Set();
  value.forEach((item, index) => {
    assertSafeText(item, `${label}[${index}]`, 3, 300);
    if (unique.has(item)) fail(`${label} contains duplicate value ${item}`);
    unique.add(item);
  });
}

function validateDecisionIds(response, index) {
  assertStringArray(response.decision_ids, `responses[${index}].decision_ids`, 1, 50);
  if (!response.decision_ids.includes(response.sheet_id)) {
    fail(`responses[${index}].decision_ids must include ${response.sheet_id}`);
  }
  const known = KNOWN_DECISION_IDS.get(response.sheet_id);
  if (known) {
    const specific = response.decision_ids.filter((id) => id !== response.sheet_id);
    if (specific.length === 0) fail(`responses[${index}] requires at least one sheet-specific decision ID`);
    for (const id of specific) {
      if (!known.has(id)) fail(`responses[${index}].decision_ids contains out-of-contract ID ${id}`);
    }
  }
}

function validateAcceptedArtifacts(response, index, rule, manifest) {
  if (!Array.isArray(response.accepted_artifacts) || response.accepted_artifacts.length !== rule.artifacts.length) {
    fail(`responses[${index}].accepted_artifacts must contain the exact sheet artifact set`);
  }
  const actual = new Map();
  response.accepted_artifacts.forEach((artifact, artifactIndex) => {
    assertExactKeys(artifact, ARTIFACT_KEYS, `responses[${index}].accepted_artifacts[${artifactIndex}]`);
    if (actual.has(artifact.path)) fail(`responses[${index}].accepted_artifacts contains duplicate path`);
    if (!/^[0-9a-f]{64}$/u.test(artifact.sha256)) fail(`responses[${index}] artifact hash is invalid`);
    const manifestHash = manifest.get(artifact.path);
    if (!manifestHash || manifestHash !== artifact.sha256) {
      fail(`responses[${index}] artifact path/hash is not current in the pinned manifest`);
    }
    actual.set(artifact.path, artifact.sha256);
  });
  const expected = [...rule.artifacts].sort();
  const paths = [...actual.keys()].sort();
  if (paths.some((path, pathIndex) => path !== expected[pathIndex])) {
    fail(`responses[${index}].accepted_artifacts does not match ${response.sheet_id}`);
  }
}

function validateEffectiveCutover(response, index) {
  assertExactKeys(response.effective_cutover, EFFECTIVE_KEYS, `responses[${index}].effective_cutover`);
  const approving = ["APPROVE", "APPROVE_WITH_CONDITIONS"].includes(response.decision);
  if (!approving) {
    for (const key of EFFECTIVE_KEYS) {
      if (response.effective_cutover[key] !== NOT_APPLICABLE) {
        fail(`responses[${index}].effective_cutover.${key} must be ${NOT_APPLICABLE}`);
      }
    }
    return;
  }
  assertTimestamp(response.effective_cutover.effective_at, `responses[${index}].effective_cutover.effective_at`);
  assertTimestamp(response.effective_cutover.cutover_at, `responses[${index}].effective_cutover.cutover_at`);
  if (Date.parse(response.effective_cutover.cutover_at) < Date.parse(response.effective_cutover.effective_at)) {
    fail(`responses[${index}] cutover_at must not precede effective_at`);
  }
  assertSafeText(
    response.effective_cutover.compatibility_window,
    `responses[${index}].effective_cutover.compatibility_window`,
    10,
    500,
  );
}

function validateResidualBlockers(response, index) {
  if (!Array.isArray(response.residual_blockers) || response.residual_blockers.length > 50) {
    fail(`responses[${index}].residual_blockers must be an array with at most 50 entries`);
  }
  const mustHaveBlocker = ["APPROVE_WITH_CONDITIONS", "NEEDS_REVISION"].includes(response.decision);
  if (mustHaveBlocker && response.residual_blockers.length === 0) {
    fail(`responses[${index}] decision requires at least one residual blocker`);
  }
  if (response.decision === "APPROVE" && response.residual_blockers.length !== 0) {
    fail(`responses[${index}] APPROVE cannot retain residual blockers`);
  }
  const blockerIds = new Set();
  response.residual_blockers.forEach((blocker, blockerIndex) => {
    assertExactKeys(blocker, BLOCKER_KEYS, `responses[${index}].residual_blockers[${blockerIndex}]`);
    assertIdentifier(blocker.blocker_id, `responses[${index}].residual_blockers[${blockerIndex}].blocker_id`);
    assertIdentifier(blocker.owner_alias, `responses[${index}].residual_blockers[${blockerIndex}].owner_alias`);
    assertSafeText(blocker.description, `responses[${index}].residual_blockers[${blockerIndex}].description`, 10, 500);
    assertTimestamp(blocker.target_at, `responses[${index}].residual_blockers[${blockerIndex}].target_at`);
    if (blockerIds.has(blocker.blocker_id)) fail(`responses[${index}] has duplicate blocker ID`);
    blockerIds.add(blocker.blocker_id);
  });
}

function validateScopes(response, index) {
  assertStringArray(response.scope_environments, `responses[${index}].scope_environments`, 1, 5);
  for (const scope of response.scope_environments) {
    if (!ALLOWED_SCOPES.has(scope)) fail(`responses[${index}] contains unsupported scope ${scope}`);
  }
  if (response.sheet_id === "S-11" && !response.scope_environments.includes("PROCUREMENT")) {
    fail(`responses[${index}] S-11 must include PROCUREMENT scope`);
  }
}

function validateReadyResponse(response, index, manifest) {
  if (response.state !== "RECEIVED_UNVERIFIED") fail(`responses[${index}].state must be RECEIVED_UNVERIFIED`);
  const rule = SHEET_RULES.get(response.sheet_id);
  if (!rule) fail(`responses[${index}].sheet_id is not externally dispatchable`);
  if (!rule.batches.includes(response.dispatch_batch)) {
    fail(`responses[${index}] dispatch batch does not own ${response.sheet_id}`);
  }

  assertIdentifier(response.response_id, `responses[${index}].response_id`);
  assertIdentifier(response.dispatch_receipt_ref, `responses[${index}].dispatch_receipt_ref`);
  assertIdentifier(response.external_response_ref, `responses[${index}].external_response_ref`);
  assertIdentifier(response.signer_identity_alias, `responses[${index}].signer_identity_alias`);
  assertSafeText(response.signer_role, `responses[${index}].signer_role`, 3, 200);
  assertSafeText(response.signer_organization, `responses[${index}].signer_organization`, 3, 200);
  assertIdentifier(response.authority_source_ref, `responses[${index}].authority_source_ref`);
  if (!/^(CHARTER|ROLE_ASSIGNMENT|TICKET|APPROVAL_CHAIN|POLICY):/u.test(response.authority_source_ref)) {
    fail(`responses[${index}].authority_source_ref lacks an allowlisted authority prefix`);
  }
  if (!/^(TICKET|MESSAGE|RECEIPT):/u.test(response.dispatch_receipt_ref)) {
    fail(`responses[${index}].dispatch_receipt_ref lacks a dispatch receipt prefix`);
  }
  if (!/^(TICKET|MESSAGE|ARTIFACT|RESPONSE):/u.test(response.external_response_ref)) {
    fail(`responses[${index}].external_response_ref lacks a response reference prefix`);
  }

  if (!ALLOWED_DECISIONS.has(response.decision)) fail(`responses[${index}].decision is not explicit`);
  assertSafeText(response.decision_text, `responses[${index}].decision_text`, 20, 2000);
  if (/^(?:OK|YES|AGREE|APPROVED|TUY DEV|TÙY DEV)$/iu.test(response.decision_text)) {
    fail(`responses[${index}].decision_text is vague`);
  }
  validateDecisionIds(response, index);
  validateAcceptedArtifacts(response, index, rule, manifest);

  assertTimestamp(response.responded_at, `responses[${index}].responded_at`);
  assertTimestamp(response.received_at, `responses[${index}].received_at`);
  if (Date.parse(response.received_at) < Date.parse(response.responded_at)) {
    fail(`responses[${index}].received_at must not precede responded_at`);
  }
  validateScopes(response, index);
  validateEffectiveCutover(response, index);
  assertSafeText(response.rollback_or_rejection_path, `responses[${index}].rollback_or_rejection_path`, 20, 2000);
  assertStringArray(response.evidence_references, `responses[${index}].evidence_references`, 1, 50);
  validateResidualBlockers(response, index);
  if (!/^[0-9a-f]{64}$/u.test(response.external_response_sha256)) {
    fail(`responses[${index}].external_response_sha256 must be lowercase SHA-256`);
  }
  assertStringArray(response.limitations, `responses[${index}].limitations`, 1, 20);
}

function validatePendingResponse(response, index) {
  for (const key of RESPONSE_KEYS) {
    if (["accepted_artifacts", "residual_blockers"].includes(key)) {
      if (!Array.isArray(response[key]) || response[key].length !== 0) {
        fail(`responses[${index}].${key} must be empty in the template`);
      }
    } else if (["decision_ids", "scope_environments", "evidence_references", "limitations"].includes(key)) {
      if (!Array.isArray(response[key]) || response[key].length !== 1 || response[key][0] !== PLACEHOLDER) {
        fail(`responses[${index}].${key} must contain only ${PLACEHOLDER}`);
      }
    } else if (key === "effective_cutover") {
      assertExactKeys(response[key], EFFECTIVE_KEYS, `responses[${index}].effective_cutover`);
      for (const effectiveKey of EFFECTIVE_KEYS) {
        if (response[key][effectiveKey] !== PLACEHOLDER) fail(`template effective_cutover must remain pending`);
      }
    } else if (key === "state") {
      if (response[key] !== "NOT_READY") fail(`responses[${index}].state must be NOT_READY`);
    } else if (response[key] !== PLACEHOLDER) {
      fail(`responses[${index}].${key} must remain ${PLACEHOLDER}`);
    }
  }
}

function validateDocument(document, mode) {
  assertExactKeys(document, ROOT_KEYS, "root");
  if (document.schema_version !== SCHEMA_VERSION) fail("schema_version is not supported");
  if (document.work_id !== WORK_ID) fail("work_id must be W-0165");
  const manifest = verifySourcePins(document.source);

  assertExactKeys(document.safety, SAFETY_KEYS, "safety");
  for (const key of SAFETY_KEYS) {
    if (document.safety[key] !== false) fail(`safety.${key} must remain false`);
  }
  if (!Array.isArray(document.responses) || document.responses.length < 1 || document.responses.length > 50) {
    fail("responses must contain 1..50 records");
  }

  if (mode === "template") {
    if (document.status !== "PENDING_EXTERNAL_RESPONSE" || document.responses.length !== 1) {
      fail("template mode requires one pending response record");
    }
    assertExactKeys(document.responses[0], RESPONSE_KEYS, "responses[0]");
    validatePendingResponse(document.responses[0], 0);
    return { responseCount: 0, approvalLikeCount: 0 };
  }

  if (document.status !== "RESPONSE_RECEIVED_PENDING_VALIDATION") {
    fail("normal input status must be RESPONSE_RECEIVED_PENDING_VALIDATION");
  }
  const responseIds = new Set();
  let approvalLikeCount = 0;
  document.responses.forEach((response, index) => {
    assertExactKeys(response, RESPONSE_KEYS, `responses[${index}]`);
    validateReadyResponse(response, index, manifest);
    if (responseIds.has(response.response_id)) fail(`duplicate response_id ${response.response_id}`);
    responseIds.add(response.response_id);
    if (["APPROVE", "APPROVE_WITH_CONDITIONS"].includes(response.decision)) approvalLikeCount += 1;
  });
  return { responseCount: document.responses.length, approvalLikeCount };
}

function validateFile(inputPath, mode) {
  const { bytes, document } = parseInput(inputPath);
  return { ...validateDocument(document, mode), inputSha256: sha256(bytes) };
}

function templateResponse() {
  return {
    response_id: PLACEHOLDER,
    dispatch_batch: PLACEHOLDER,
    dispatch_receipt_ref: PLACEHOLDER,
    sheet_id: PLACEHOLDER,
    decision_ids: [PLACEHOLDER],
    decision: PLACEHOLDER,
    decision_text: PLACEHOLDER,
    signer_identity_alias: PLACEHOLDER,
    signer_role: PLACEHOLDER,
    signer_organization: PLACEHOLDER,
    authority_source_ref: PLACEHOLDER,
    accepted_artifacts: [],
    responded_at: PLACEHOLDER,
    received_at: PLACEHOLDER,
    scope_environments: [PLACEHOLDER],
    effective_cutover: {
      effective_at: PLACEHOLDER,
      cutover_at: PLACEHOLDER,
      compatibility_window: PLACEHOLDER,
    },
    rollback_or_rejection_path: PLACEHOLDER,
    evidence_references: [PLACEHOLDER],
    residual_blockers: [],
    external_response_ref: PLACEHOLDER,
    external_response_sha256: PLACEHOLDER,
    limitations: [PLACEHOLDER],
    state: "NOT_READY",
  };
}

function templateDocument() {
  return {
    schema_version: SCHEMA_VERSION,
    work_id: WORK_ID,
    status: "PENDING_EXTERNAL_RESPONSE",
    source: { ...SOURCE_PINS },
    responses: [templateResponse()],
    safety: {
      contains_personal_contact_details: false,
      contains_credentials_or_secrets: false,
      raw_external_response_embedded: false,
      external_authority_verified: false,
      approval_ledger_updated: false,
      production_gate_promoted: false,
      real_customer_call_allowed: false,
    },
  };
}

function artifactRecords(paths, manifest) {
  return paths.map((path) => ({ path, sha256: manifest.get(path) }));
}

function validResponse(sheetId, decision, manifest) {
  const rule = SHEET_RULES.get(sheetId);
  const specificId = sheetId === "S-06" ? "OPT-01" : null;
  const approving = ["APPROVE", "APPROVE_WITH_CONDITIONS"].includes(decision);
  const needsBlocker = ["APPROVE_WITH_CONDITIONS", "NEEDS_REVISION"].includes(decision);
  return {
    response_id: `RESPONSE:${sheetId}-SYNTHETIC-01`,
    dispatch_batch: rule.batches[0],
    dispatch_receipt_ref: `RECEIPT:${rule.batches[0]}-SYNTHETIC-01`,
    sheet_id: sheetId,
    decision_ids: specificId ? [sheetId, specificId] : [sheetId],
    decision,
    decision_text: "Explicit synthetic decision used only by the offline validator self-test.",
    signer_identity_alias: "AUTHORIZED_SIGNER_ALIAS",
    signer_role: "Accountable external decision owner",
    signer_organization: "External owner organization alias",
    authority_source_ref: `ROLE_ASSIGNMENT:${sheetId}-OWNER-01`,
    accepted_artifacts: artifactRecords(rule.artifacts, manifest),
    responded_at: "2026-09-03T13:00:00+07:00",
    received_at: "2026-09-03T13:05:00+07:00",
    scope_environments: sheetId === "S-11" ? ["PROCUREMENT"] : ["CONTRACT"],
    effective_cutover: approving
      ? {
          effective_at: "2026-09-04T00:00:00+07:00",
          cutover_at: "2026-09-05T00:00:00+07:00",
          compatibility_window: "Retain the previous contract until the shared verification window closes.",
        }
      : {
          effective_at: NOT_APPLICABLE,
          cutover_at: NOT_APPLICABLE,
          compatibility_window: NOT_APPLICABLE,
        },
    rollback_or_rejection_path:
      "Keep all dependent implementation disabled and return the sheet for explicit owner review.",
    evidence_references: [`TICKET:${sheetId}-SYNTHETIC-EVIDENCE-01`],
    residual_blockers: needsBlocker
      ? [
          {
            blocker_id: `BLOCKER:${sheetId}-01`,
            owner_alias: "EXTERNAL_OWNER_ALIAS",
            description: "Provide the remaining signed contract evidence before implementation starts.",
            target_at: "2026-09-10T17:00:00+07:00",
          },
        ]
      : [],
    external_response_ref: `RESPONSE:${sheetId}-SYNTHETIC-ARTIFACT-01`,
    external_response_sha256: "a".repeat(64),
    limitations: ["SYNTHETIC_SELF_TEST_ONLY"],
    state: "RECEIVED_UNVERIFIED",
  };
}

function inputDocument(response) {
  const document = templateDocument();
  document.status = "RESPONSE_RECEIVED_PENDING_VALIDATION";
  document.responses = [response];
  return document;
}

function clone(value) {
  return JSON.parse(JSON.stringify(value));
}

function runSelfTest() {
  const temporaryRoot = mkdtempSync(join(REPOSITORY_ROOT, ".w0165-selftest-"));
  const manifest = parseAndVerifyManifest();
  let refusals = 0;
  const writeCase = (name, value) => {
    const path = join(temporaryRoot, `${name}.json`);
    writeFileSync(path, typeof value === "string" ? value : `${JSON.stringify(value, null, 2)}\n`, "utf8");
    return path;
  };
  const expectFailure = (name, value, mode = "input") => {
    try {
      validateFile(writeCase(name, value), mode);
      fail(`self-test ${name} unexpectedly passed`);
    } catch (error) {
      if (error.message.includes("unexpectedly passed")) throw error;
      refusals += 1;
    }
  };

  try {
    const template = templateDocument();
    validateFile(writeCase("template-valid", template), "template");
    const approve = inputDocument(validResponse("S-06", "APPROVE_WITH_CONDITIONS", manifest));
    validateFile(writeCase("approval-valid", approve), "input");
    const reject = inputDocument(validResponse("S-11", "REJECT", manifest));
    validateFile(writeCase("rejection-valid", reject), "input");

    expectFailure("pending-normal-mode", template);
    const missingKey = clone(approve);
    delete missingKey.responses[0].signer_role;
    expectFailure("missing-key", missingKey);
    const extraKey = clone(approve);
    extraKey.responses[0].unexpected = true;
    expectFailure("extra-key", extraKey);
    const wrongSource = clone(approve);
    wrongSource.source.artifact_manifest_sha256 = "0".repeat(64);
    expectFailure("source-hash", wrongSource);
    const badSheet = clone(approve);
    badSheet.responses[0].sheet_id = "S-10";
    badSheet.responses[0].decision_ids = ["S-10"];
    expectFailure("non-dispatch-sheet", badSheet);
    const badBatch = clone(approve);
    badBatch.responses[0].dispatch_batch = "D-01";
    expectFailure("sheet-batch", badBatch);
    const missingArtifact = clone(approve);
    missingArtifact.responses[0].accepted_artifacts.pop();
    expectFailure("missing-artifact", missingArtifact);
    const extraArtifact = clone(approve);
    extraArtifact.responses[0].accepted_artifacts.push({
      path: ARTIFACTS.M809,
      sha256: manifest.get(ARTIFACTS.M809),
    });
    expectFailure("extra-artifact", extraArtifact);
    const badArtifactHash = clone(approve);
    badArtifactHash.responses[0].accepted_artifacts[0].sha256 = "0".repeat(64);
    expectFailure("artifact-hash", badArtifactHash);
    const badDecisionId = clone(approve);
    badDecisionId.responses[0].decision_ids = ["S-06", "DTK-01"];
    expectFailure("decision-id", badDecisionId);
    const vague = clone(approve);
    vague.responses[0].decision_text = "OK";
    expectFailure("vague-decision", vague);
    const email = clone(approve);
    email.responses[0].external_response_ref = "person@example.invalid";
    expectFailure("email", email);
    const phone = clone(approve);
    phone.responses[0].decision_text = "Call the signer at +84 912 345 678 before accepting this response.";
    expectFailure("phone", phone);
    const address = clone(approve);
    address.responses[0].decision_text = "Deliver the signed response to 12 đường Test before accepting it.";
    expectFailure("address", address);
    const secret = clone(approve);
    secret.responses[0].evidence_references = ["Bearer abcdefghijklmnopqrstuvwxyz"];
    expectFailure("secret", secret);
    const badTime = clone(approve);
    badTime.responses[0].responded_at = "2026-09-03";
    expectFailure("timestamp", badTime);
    const receivedBefore = clone(approve);
    receivedBefore.responses[0].received_at = "2026-09-03T12:59:00+07:00";
    expectFailure("received-before-response", receivedBefore);
    const cutoverBefore = clone(approve);
    cutoverBefore.responses[0].effective_cutover.cutover_at = "2026-09-03T23:00:00+07:00";
    expectFailure("cutover-before-effective", cutoverBefore);
    const badScope = clone(approve);
    badScope.responses[0].scope_environments = ["ALL_ENVIRONMENTS"];
    expectFailure("scope", badScope);
    const noConditionalBlocker = clone(approve);
    noConditionalBlocker.responses[0].residual_blockers = [];
    expectFailure("conditional-without-blocker", noConditionalBlocker);
    const approveWithBlocker = clone(approve);
    approveWithBlocker.responses[0].decision = "APPROVE";
    expectFailure("approve-with-blocker", approveWithBlocker);
    const duplicateIds = clone(approve);
    duplicateIds.responses = [clone(approve.responses[0]), clone(approve.responses[0])];
    expectFailure("duplicate-response-id", duplicateIds);
    const unsafe = clone(approve);
    unsafe.safety.approval_ledger_updated = true;
    expectFailure("unsafe-flag", unsafe);
    expectFailure("malformed-json", '{"schema_version":');
    expectFailure("duplicate-json-key", '{"schema_version":"a","schema_version":"b"}');
    expectFailure("oversized-input", " ".repeat(MAX_INPUT_BYTES + 1));
    try {
      validateFile(join(REPOSITORY_ROOT, "..", "outside-decision-response.json"), "input");
      fail("self-test outside-root unexpectedly passed");
    } catch (error) {
      if (error.message.includes("unexpectedly passed")) throw error;
      refusals += 1;
    }

    return { templateChecks: 1, validResponses: 2, refusals };
  } finally {
    rmSync(temporaryRoot, { recursive: true, force: true });
  }
}

function usage() {
  return [
    "Usage:",
    "  node deploy/ci/scripts/external-decision-response-validator.mjs --check-template <json>",
    "  node deploy/ci/scripts/external-decision-response-validator.mjs --input <json>",
    "  node deploy/ci/scripts/external-decision-response-validator.mjs --self-test",
  ].join("\n");
}

function main(argv) {
  if (argv.length === 1 && argv[0] === "--self-test") {
    const result = runSelfTest();
    console.log(
      `W0165_SELFTEST_PASS template=${result.templateChecks} valid=${result.validResponses} refusals=${result.refusals}`,
    );
    return;
  }
  if (argv.length !== 2 || !["--check-template", "--input"].includes(argv[0])) fail(usage());
  const mode = argv[0] === "--check-template" ? "template" : "input";
  const result = validateFile(argv[1], mode);
  const status = mode === "template" ? "RESPONSE_TEMPLATE_VALID_NOT_READY" : "RESPONSE_PROVENANCE_VALID_AUTHORITY_UNVERIFIED";
  console.log(
    `${status} responses=${result.responseCount} approval_like=${result.approvalLikeCount} sha256=${result.inputSha256}`,
  );
}

try {
  main(process.argv.slice(2));
} catch (error) {
  console.error(`W0165_VALIDATION_FAILED: ${error.message}`);
  process.exitCode = 1;
}
