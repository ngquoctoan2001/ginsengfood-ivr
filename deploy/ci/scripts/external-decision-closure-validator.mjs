#!/usr/bin/env node

// W-0170 — Offline, metadata-only verifier for external-decision dispatch receipts,
// independently verified signer authority and per-sheet closure quorum.
//
// This CLI never sends a message, reads a raw external response, writes an approval ledger,
// promotes a gate or authorizes a real customer call.

import { createHash } from "node:crypto";
import {
  lstatSync,
  mkdirSync,
  mkdtempSync,
  readFileSync,
  realpathSync,
  rmSync,
  writeFileSync,
} from "node:fs";
import { dirname, isAbsolute, relative, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { spawnSync } from "node:child_process";

const SCRIPT_PATH = fileURLToPath(import.meta.url);
const REPOSITORY_ROOT = resolve(dirname(SCRIPT_PATH), "../../..");
const MAX_INPUT_BYTES = 512 * 1024;
const MAX_REFERENCED_BYTES = 512 * 1024;
const SCHEMA_VERSION = "m8-external-decision-closure.v1";
const WORK_ID = "W-0170";
const PLACEHOLDER = "PENDING_EXTERNAL_EVIDENCE";

const SOURCE_PINS = Object.freeze({
  dispatch_pack_path:
    "plan/ivr-orther/m8-12-external-decision-provenance-dispatch-pack-2026-09-03.md",
  dispatch_pack_sha256:
    "9da8e5698bc99df73338b3d6886e61f18c93e492431d07cb730074f6ef3aa499",
  artifact_manifest_path: "docs/evidence/W-0170/artifact-sha256.txt",
  artifact_manifest_sha256:
    "3352479690e424b88138654b1a91aa5c55908b19d47ee63870795b113e616471",
  message_kit_path:
    "plan/ivr-orther/m8-13-external-decision-dispatch-message-kit-2026-09-03.md",
  message_kit_sha256:
    "95632b90ab99df6892ba6f0a231e9429c5021fd200d982609c52584e1b920ca3",
  routing_validator_path: "deploy/ci/scripts/external-decision-routing-validator.mjs",
  routing_validator_sha256:
    "e70eb8b90e2a5697219f375baab7e6c0d6cb7d58053310ca8fd47caf07180d45",
  response_validator_path: "deploy/ci/scripts/external-decision-response-validator.mjs",
  response_validator_sha256:
    "1d14a46eeceb4a59586e23cd84668be50836831ef71c3a50c53a85386d72e1dc",
});

const SHEET_RULES = new Map([
  [
    "S-01",
    {
      authorities: [
        ["M3_CONTRACT", ["D-01"]],
        ["LEGAL_PRIVACY", ["D-01"]],
      ],
      decisions: Array.from({ length: 5 }, (_, index) => `OD18-C${index + 1}`),
    },
  ],
  [
    "S-02",
    {
      authorities: [
        ["M3_CONTRACT", ["D-01"]],
        ["PRODUCT", ["D-01"]],
        ["ORDER_CORE", ["D-01"]],
      ],
      decisions: [],
    },
  ],
  [
    "S-03",
    {
      authorities: [
        ["M3_OPERATOR", ["D-02"]],
        ["SECURITY", ["D-02"]],
        ["PLATFORM", ["D-02"]],
      ],
      decisions: [],
    },
  ],
  ["S-04", { authorities: [["M3_PRODUCER", ["D-01"]]], decisions: [] }],
  [
    "S-05",
    {
      authorities: [
        ["M3_CONTRACT", ["D-01"]],
        ["SECURITY", ["D-02"]],
        ["PLATFORM", ["D-02"]],
      ],
      decisions: [],
    },
  ],
  [
    "S-06",
    {
      authorities: [
        ["PROJECT_OWNER", ["D-03"]],
        ["CRM_M31", ["D-03"]],
        ["M3_CONTRACT", ["D-03"]],
        ["LEGAL_PRIVACY", ["D-03"]],
        ["PRODUCT", ["D-03"]],
      ],
      decisions: Array.from({ length: 11 }, (_, index) => `OPT-${String(index + 1).padStart(2, "0")}`),
    },
  ],
  [
    "S-07",
    {
      authorities: [
        ["PROJECT_OWNER", ["D-01"]],
        ["M3_CONTRACT", ["D-01"]],
        ["ORDER_CORE", ["D-01"]],
        ["PRODUCT", ["D-01"]],
      ],
      decisions: Array.from({ length: 12 }, (_, index) => `RVK-${String(index + 1).padStart(2, "0")}`),
    },
  ],
  [
    "S-08",
    {
      authorities: [
        ["M3_PRODUCER", ["D-02"]],
        ["SECURITY", ["D-02"]],
        ["PLATFORM", ["D-02"]],
        ["TELEPHONY_VENDOR", ["D-02"]],
        ["PRODUCT", ["D-02"]],
        ["LEGAL_PRIVACY", ["D-02"]],
        ["RELEASE", ["D-02"]],
      ],
      decisions: Array.from({ length: 15 }, (_, index) => `DTK-${String(index + 1).padStart(2, "0")}`),
    },
  ],
  [
    "S-09",
    {
      authorities: [
        ["PRODUCT", ["D-04"]],
        ["ORDER_CORE", ["D-04"]],
        ["M3_PRODUCER", ["D-04"]],
        ["PLATFORM", ["D-04"]],
        ["M8_OWNER", ["D-04"]],
        ["RELEASE", ["D-04"]],
      ],
      decisions: Array.from({ length: 15 }, (_, index) => `ATP-${String(index + 1).padStart(2, "0")}`),
    },
  ],
  [
    "S-11",
    {
      authorities: [
        ["M8_OWNER", ["D-05"]],
        ["PRODUCT", ["D-05"]],
        ["INFRA_PROCUREMENT", ["D-05"]],
        ["TELEPHONY_VENDOR", ["D-05"]],
      ],
      decisions: [],
    },
  ],
]);

const ROOT_KEYS = [
  "schema_version",
  "work_id",
  "status",
  "source",
  "routing_input",
  "response_bundle",
  "dispatch_receipts",
  "authority_attestations",
  "sheet_closures",
  "safety",
];
const SOURCE_KEYS = Object.keys(SOURCE_PINS);
const FILE_REF_KEYS = ["path", "sha256"];
const RECEIPT_KEYS = [
  "receipt_id",
  "batch",
  "routing_input_sha256",
  "system_of_record_kind",
  "destination_ref",
  "system_of_record_ref",
  "external_receipt_sha256",
  "sender_identity_alias",
  "recipient_identity_aliases",
  "sent_at",
  "delivered_at",
  "delivery_state",
];
const ATTESTATION_KEYS = [
  "attestation_id",
  "response_id",
  "sheet_id",
  "authority_group",
  "authority_source_ref",
  "authority_evidence_ref",
  "authority_evidence_sha256",
  "verified_by_alias",
  "verified_at",
  "state",
];
const CLOSURE_KEYS = [
  "sheet_id",
  "required_authority_groups",
  "accepted_response_ids",
  "receipt_ids",
  "decision_ids",
  "state",
];
const SAFETY_KEYS = [
  "contains_personal_contact_details",
  "contains_credentials_or_secrets",
  "raw_external_artifact_embedded",
  "external_authority_assumed",
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
  if (!value || typeof value !== "object" || Array.isArray(value)) fail(`${label} must be an object`);
  const actual = Object.keys(value).sort();
  const wanted = [...expected].sort();
  if (actual.length !== wanted.length || actual.some((key, index) => key !== wanted[index])) {
    fail(`${label} must contain exactly: ${wanted.join(", ")}`);
  }
}

function isConfined(pathValue) {
  const rel = relative(REPOSITORY_ROOT, pathValue);
  return rel !== "" && !rel.startsWith("..") && !isAbsolute(rel);
}

function readConfinedFile(inputPath, maximumBytes = MAX_REFERENCED_BYTES) {
  const resolved = resolve(REPOSITORY_ROOT, inputPath);
  if (!isConfined(resolved)) fail(`path is outside repository root: ${inputPath}`);
  const stat = lstatSync(resolved);
  if (!stat.isFile() || stat.isSymbolicLink()) fail(`path must be a regular non-symlink file: ${inputPath}`);
  if (!isConfined(realpathSync(resolved))) fail(`real path escapes repository root: ${inputPath}`);
  if (stat.size > maximumBytes) fail(`file exceeds ${maximumBytes} bytes: ${inputPath}`);
  return { resolved, bytes: readFileSync(resolved) };
}

function readStrictJson(inputPath, maximumBytes = MAX_REFERENCED_BYTES) {
  const { resolved, bytes } = readConfinedFile(inputPath, maximumBytes);
  if (bytes.length >= 3 && bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf) {
    fail(`UTF-8 BOM is not allowed: ${inputPath}`);
  }
  const textValue = new TextDecoder("utf-8", { fatal: true }).decode(bytes);
  rejectDuplicateJsonKeys(textValue);
  let document;
  try {
    document = JSON.parse(textValue);
  } catch (error) {
    fail(`invalid JSON in ${inputPath}: ${error.message}`);
  }
  return { resolved, bytes, document };
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

function assertString(value, label, minimum, maximum) {
  if (typeof value !== "string" || value.length < minimum || value.length > maximum) {
    fail(`${label} must be a string of ${minimum}..${maximum} characters`);
  }
  if (value.trim() !== value) fail(`${label} must not have surrounding whitespace`);
  if (/[\u0000-\u001f\u007f]/u.test(value)) fail(`${label} contains a control character`);
}

function assertIdentifier(value, label) {
  assertString(value, label, 3, 180);
  if (!/^[A-Z0-9][A-Z0-9._:/-]+$/u.test(value)) fail(`${label} must be an uppercase alias/reference`);
  assertNoSensitiveValue(value, label);
}

function assertSha256(value, label) {
  if (typeof value !== "string" || !/^[0-9a-f]{64}$/u.test(value)) fail(`${label} must be lowercase SHA-256`);
}

function assertTimestamp(value, label) {
  assertString(value, label, 20, 35);
  if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,3})?(?:Z|[+-]\d{2}:\d{2})$/u.test(value)) {
    fail(`${label} must be ISO-8601 with an explicit timezone`);
  }
  if (!Number.isFinite(Date.parse(value))) fail(`${label} is not a valid timestamp`);
}

function assertNoSensitiveValue(value, label) {
  if (/[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}/iu.test(value)) fail(`${label} contains an email-like value`);
  if (/(?:^|\D)(?:\+?\d[\s().-]*){9,15}(?:$|\D)/u.test(value)) fail(`${label} contains a phone-like value`);
  if (/\b\d{1,5}\s+(?:đường|duong|phố|pho|street|st\.?|road|rd\.?|avenue|ave\.?)\b/iu.test(value)) {
    fail(`${label} contains a street-address-like value`);
  }
  if (/(?:password|passwd|bearer\s+|api[_ -]?key|access[_ -]?token|private[_ -]?key|client[_ -]?secret)\s*[:=]?/iu.test(value)) {
    fail(`${label} contains credential- or secret-like material`);
  }
}

function assertSafeValue(value, label, minimum = 3, maximum = 300) {
  assertString(value, label, minimum, maximum);
  assertNoSensitiveValue(value, label);
}

function assertUniqueStringArray(value, label, minimum, maximum, identifierOnly = false) {
  if (!Array.isArray(value) || value.length < minimum || value.length > maximum) {
    fail(`${label} must contain ${minimum}..${maximum} values`);
  }
  const seen = new Set();
  value.forEach((item, index) => {
    if (identifierOnly) assertIdentifier(item, `${label}[${index}]`);
    else assertSafeValue(item, `${label}[${index}]`);
    if (seen.has(item)) fail(`${label} contains duplicate value ${item}`);
    seen.add(item);
  });
}

function verifySourcePins(source) {
  assertExactKeys(source, SOURCE_KEYS, "source");
  for (const key of SOURCE_KEYS) {
    if (source[key] !== SOURCE_PINS[key]) fail(`source.${key} does not match the pinned value`);
  }
  for (const [pathKey, hashKey] of [
    ["dispatch_pack_path", "dispatch_pack_sha256"],
    ["artifact_manifest_path", "artifact_manifest_sha256"],
    ["message_kit_path", "message_kit_sha256"],
    ["routing_validator_path", "routing_validator_sha256"],
    ["response_validator_path", "response_validator_sha256"],
  ]) {
    const { bytes } = readConfinedFile(source[pathKey], 5 * 1024 * 1024);
    if (sha256(bytes) !== source[hashKey]) fail(`${source[pathKey]} drifted from its pinned SHA-256`);
  }
}

function verifyFileReference(reference, label) {
  assertExactKeys(reference, FILE_REF_KEYS, label);
  assertSafeValue(reference.path, `${label}.path`, 3, 400);
  assertSha256(reference.sha256, `${label}.sha256`);
  const result = readStrictJson(reference.path);
  if (sha256(result.bytes) !== reference.sha256) fail(`${label} content does not match its SHA-256`);
  return result;
}

function runPrerequisiteValidator(scriptRelativePath, inputPath, expectedPrefix, label) {
  const scriptPath = resolve(REPOSITORY_ROOT, scriptRelativePath);
  const result = spawnSync(process.execPath, [scriptPath, "--input", inputPath], {
    cwd: REPOSITORY_ROOT,
    encoding: "utf8",
    windowsHide: true,
  });
  const output = `${result.stdout ?? ""}${result.stderr ?? ""}`.trim();
  if (result.status !== 0 || !output.includes(expectedPrefix)) {
    fail(`${label} prerequisite validation failed: ${output || `exit ${result.status}`}`);
  }
}

function readyRoutingByBatch(routingDocument) {
  return new Map(
    routingDocument.batches
      .filter((batch) => batch.state === "READY_FOR_HASH_RECHECK_AND_DISPATCH")
      .map((batch) => [batch.batch, batch]),
  );
}

function validateReceipts(receipts, routingDocument, routingSha256) {
  if (!Array.isArray(receipts) || receipts.length < 1 || receipts.length > 20) {
    fail("dispatch_receipts must contain 1..20 records");
  }
  const readyByBatch = readyRoutingByBatch(routingDocument);
  const receiptById = new Map();
  receipts.forEach((receipt, index) => {
    assertExactKeys(receipt, RECEIPT_KEYS, `dispatch_receipts[${index}]`);
    assertIdentifier(receipt.receipt_id, `dispatch_receipts[${index}].receipt_id`);
    if (!receipt.receipt_id.startsWith("RECEIPT:")) fail(`dispatch_receipts[${index}].receipt_id must start RECEIPT:`);
    if (receiptById.has(receipt.receipt_id)) fail(`duplicate receipt_id ${receipt.receipt_id}`);
    const routing = readyByBatch.get(receipt.batch);
    if (!routing) fail(`dispatch_receipts[${index}] batch is not ready in routing input`);
    if (receipt.routing_input_sha256 !== routingSha256) fail(`dispatch_receipts[${index}] routing hash mismatch`);
    if (receipt.system_of_record_kind !== routing.channel_kind) fail(`dispatch_receipts[${index}] channel mismatch`);
    if (receipt.destination_ref !== routing.destination_ref) fail(`dispatch_receipts[${index}] destination mismatch`);
    assertIdentifier(receipt.system_of_record_ref, `dispatch_receipts[${index}].system_of_record_ref`);
    if (!/^(?:TICKET|MESSAGE|RECEIPT):/u.test(receipt.system_of_record_ref)) {
      fail(`dispatch_receipts[${index}].system_of_record_ref has unsupported prefix`);
    }
    assertSha256(receipt.external_receipt_sha256, `dispatch_receipts[${index}].external_receipt_sha256`);
    assertIdentifier(receipt.sender_identity_alias, `dispatch_receipts[${index}].sender_identity_alias`);
    assertUniqueStringArray(receipt.recipient_identity_aliases, `dispatch_receipts[${index}].recipient_identity_aliases`, 1, 20, true);
    if (!receipt.recipient_identity_aliases.includes(routing.recipient_identity)) {
      fail(`dispatch_receipts[${index}] omits the routed primary recipient`);
    }
    assertTimestamp(receipt.sent_at, `dispatch_receipts[${index}].sent_at`);
    assertTimestamp(receipt.delivered_at, `dispatch_receipts[${index}].delivered_at`);
    if (Date.parse(receipt.sent_at) < Date.parse(routing.dispatch_authorized_at)) {
      fail(`dispatch_receipts[${index}] predates dispatch authorization`);
    }
    if (Date.parse(receipt.sent_at) >= Date.parse(routing.due_at)) {
      fail(`dispatch_receipts[${index}] was sent at or after the requested response due time`);
    }
    if (Date.parse(receipt.delivered_at) < Date.parse(receipt.sent_at)) {
      fail(`dispatch_receipts[${index}] delivered_at precedes sent_at`);
    }
    if (receipt.delivery_state !== "DELIVERED") fail(`dispatch_receipts[${index}] must be DELIVERED`);
    receiptById.set(receipt.receipt_id, receipt);
  });
  return receiptById;
}

function validateResponseReceiptBindings(responseDocument, receiptById) {
  const responseById = new Map();
  responseDocument.responses.forEach((response, index) => {
    if (responseById.has(response.response_id)) fail(`duplicate response_id ${response.response_id}`);
    const receipt = receiptById.get(response.dispatch_receipt_ref);
    if (!receipt) fail(`responses[${index}] has no matching hash-bound dispatch receipt`);
    if (receipt.batch !== response.dispatch_batch) fail(`responses[${index}] receipt batch mismatch`);
    if (Date.parse(response.responded_at) < Date.parse(receipt.sent_at)) {
      fail(`responses[${index}] predates its dispatch receipt`);
    }
    responseById.set(response.response_id, response);
  });
  return responseById;
}

function validateAuthorityAttestations(attestations, responseById) {
  if (!Array.isArray(attestations) || attestations.length < 1 || attestations.length > 100) {
    fail("authority_attestations must contain 1..100 records");
  }
  const attestationByResponse = new Map();
  const attestationIds = new Set();
  attestations.forEach((attestation, index) => {
    assertExactKeys(attestation, ATTESTATION_KEYS, `authority_attestations[${index}]`);
    assertIdentifier(attestation.attestation_id, `authority_attestations[${index}].attestation_id`);
    if (attestationIds.has(attestation.attestation_id)) fail(`duplicate attestation_id ${attestation.attestation_id}`);
    if (attestationByResponse.has(attestation.response_id)) fail(`response ${attestation.response_id} has multiple authority groups`);
    const response = responseById.get(attestation.response_id);
    if (!response) fail(`authority_attestations[${index}] references an unknown response`);
    if (attestation.sheet_id !== response.sheet_id) fail(`authority_attestations[${index}] sheet mismatch`);
    assertIdentifier(attestation.authority_group, `authority_attestations[${index}].authority_group`);
    if (attestation.authority_source_ref !== response.authority_source_ref) {
      fail(`authority_attestations[${index}] authority source does not match response`);
    }
    assertIdentifier(attestation.authority_evidence_ref, `authority_attestations[${index}].authority_evidence_ref`);
    if (!/^(?:CHARTER|ROLE_ASSIGNMENT|TICKET|APPROVAL_CHAIN|POLICY):/u.test(attestation.authority_evidence_ref)) {
      fail(`authority_attestations[${index}].authority_evidence_ref has unsupported prefix`);
    }
    assertSha256(attestation.authority_evidence_sha256, `authority_attestations[${index}].authority_evidence_sha256`);
    assertIdentifier(attestation.verified_by_alias, `authority_attestations[${index}].verified_by_alias`);
    if (attestation.verified_by_alias === response.signer_identity_alias) {
      fail(`authority_attestations[${index}] violates signer/verifier separation of duties`);
    }
    assertTimestamp(attestation.verified_at, `authority_attestations[${index}].verified_at`);
    if (Date.parse(attestation.verified_at) < Date.parse(response.received_at)) {
      fail(`authority_attestations[${index}] predates response receipt`);
    }
    if (attestation.state !== "AUTHORITY_VERIFIED") fail(`authority_attestations[${index}] must be AUTHORITY_VERIFIED`);
    attestationIds.add(attestation.attestation_id);
    attestationByResponse.set(attestation.response_id, attestation);
  });
  return attestationByResponse;
}

function exactArray(actual, expected, label) {
  if (!Array.isArray(actual) || actual.length !== expected.length || actual.some((value, index) => value !== expected[index])) {
    fail(`${label} must be exactly: ${expected.join(", ")}`);
  }
}

function validateSheetClosures(closures, responseById, receiptById, attestationByResponse) {
  if (!Array.isArray(closures) || closures.length < 1 || closures.length > SHEET_RULES.size) {
    fail(`sheet_closures must contain 1..${SHEET_RULES.size} records`);
  }
  const sheetIds = new Set();
  closures.forEach((closure, index) => {
    assertExactKeys(closure, CLOSURE_KEYS, `sheet_closures[${index}]`);
    if (sheetIds.has(closure.sheet_id)) fail(`duplicate sheet closure ${closure.sheet_id}`);
    const rule = SHEET_RULES.get(closure.sheet_id);
    if (!rule) fail(`sheet_closures[${index}] is not externally closable`);
    const expectedGroups = rule.authorities.map(([group]) => group);
    exactArray(closure.required_authority_groups, expectedGroups, `sheet_closures[${index}].required_authority_groups`);
    assertUniqueStringArray(closure.accepted_response_ids, `sheet_closures[${index}].accepted_response_ids`, expectedGroups.length, expectedGroups.length, true);
    assertUniqueStringArray(closure.receipt_ids, `sheet_closures[${index}].receipt_ids`, 1, 5, true);
    const expectedDecisionIds = [closure.sheet_id, ...rule.decisions];
    exactArray(closure.decision_ids, expectedDecisionIds, `sheet_closures[${index}].decision_ids`);
    if (closure.state !== "DECISION_PROVENANCE_CLOSED") {
      fail(`sheet_closures[${index}].state must be DECISION_PROVENANCE_CLOSED`);
    }

    const actualGroups = [];
    const actualReceiptIds = new Set();
    const coveredDecisionIds = new Set();
    closure.accepted_response_ids.forEach((responseId) => {
      const response = responseById.get(responseId);
      if (!response || response.sheet_id !== closure.sheet_id) {
        fail(`sheet_closures[${index}] references a response for another or unknown sheet`);
      }
      if (response.decision !== "APPROVE" || response.residual_blockers.length !== 0) {
        fail(`sheet_closures[${index}] requires unconditional APPROVE responses with no blockers`);
      }
      const attestation = attestationByResponse.get(responseId);
      if (!attestation) fail(`sheet_closures[${index}] response lacks independent authority attestation`);
      const authorityRule = rule.authorities.find(([group]) => group === attestation.authority_group);
      if (!authorityRule) fail(`sheet_closures[${index}] contains unexpected authority group ${attestation.authority_group}`);
      if (!authorityRule[1].includes(response.dispatch_batch)) {
        fail(`sheet_closures[${index}] authority group ${attestation.authority_group} came through the wrong batch`);
      }
      actualGroups.push(attestation.authority_group);
      actualReceiptIds.add(response.dispatch_receipt_ref);
      response.decision_ids.forEach((decisionId) => coveredDecisionIds.add(decisionId));
    });

    exactArray(actualGroups, expectedGroups, `sheet_closures[${index}] authority quorum`);
    const expectedReceipts = [...actualReceiptIds].sort();
    const declaredReceipts = [...closure.receipt_ids].sort();
    exactArray(declaredReceipts, expectedReceipts, `sheet_closures[${index}].receipt_ids`);
    for (const receiptId of declaredReceipts) {
      if (!receiptById.has(receiptId)) fail(`sheet_closures[${index}] references unknown receipt ${receiptId}`);
    }
    for (const decisionId of expectedDecisionIds) {
      if (!coveredDecisionIds.has(decisionId)) fail(`sheet_closures[${index}] does not cover decision ${decisionId}`);
    }
    sheetIds.add(closure.sheet_id);
  });
  return { closedSheetCount: closures.length, closedSheets: [...sheetIds] };
}

function validatePendingTemplate(document) {
  if (document.status !== "PENDING_EXTERNAL_EVIDENCE") fail("template status must be PENDING_EXTERNAL_EVIDENCE");
  for (const [label, reference] of [
    ["routing_input", document.routing_input],
    ["response_bundle", document.response_bundle],
  ]) {
    assertExactKeys(reference, FILE_REF_KEYS, label);
    if (reference.path !== PLACEHOLDER || reference.sha256 !== PLACEHOLDER) fail(`${label} must remain pending`);
  }
  for (const field of ["dispatch_receipts", "authority_attestations", "sheet_closures"]) {
    if (!Array.isArray(document[field]) || document[field].length !== 0) fail(`template ${field} must be empty`);
  }
  return { closedSheetCount: 0, closedSheets: [] };
}

function validateDocument(document, mode) {
  assertExactKeys(document, ROOT_KEYS, "root");
  if (document.schema_version !== SCHEMA_VERSION) fail("schema_version is not supported");
  if (document.work_id !== WORK_ID) fail("work_id must be W-0170");
  verifySourcePins(document.source);
  assertExactKeys(document.safety, SAFETY_KEYS, "safety");
  for (const key of SAFETY_KEYS) {
    if (document.safety[key] !== false) fail(`safety.${key} must remain false`);
  }
  if (mode === "template") return validatePendingTemplate(document);
  if (document.status !== "CLOSURE_REVIEW_PENDING") fail("input status must be CLOSURE_REVIEW_PENDING");

  const routing = verifyFileReference(document.routing_input, "routing_input");
  const response = verifyFileReference(document.response_bundle, "response_bundle");
  runPrerequisiteValidator(SOURCE_PINS.routing_validator_path, document.routing_input.path, "ROUTING_INPUT_VALID", "W-0164");
  runPrerequisiteValidator(SOURCE_PINS.response_validator_path, document.response_bundle.path, "RESPONSE_PROVENANCE_VALID_AUTHORITY_UNVERIFIED", "W-0165");

  const receiptById = validateReceipts(document.dispatch_receipts, routing.document, document.routing_input.sha256);
  const responseById = validateResponseReceiptBindings(response.document, receiptById);
  const attestationByResponse = validateAuthorityAttestations(document.authority_attestations, responseById);
  return validateSheetClosures(document.sheet_closures, responseById, receiptById, attestationByResponse);
}

function validateFile(inputPath, mode) {
  const result = readStrictJson(inputPath, MAX_INPUT_BYTES);
  const counts = validateDocument(result.document, mode);
  return { ...counts, inputSha256: sha256(result.bytes) };
}

function templateDocument() {
  return {
    schema_version: SCHEMA_VERSION,
    work_id: WORK_ID,
    status: "PENDING_EXTERNAL_EVIDENCE",
    source: { ...SOURCE_PINS },
    routing_input: { path: PLACEHOLDER, sha256: PLACEHOLDER },
    response_bundle: { path: PLACEHOLDER, sha256: PLACEHOLDER },
    dispatch_receipts: [],
    authority_attestations: [],
    sheet_closures: [],
    safety: Object.fromEntries(SAFETY_KEYS.map((key) => [key, false])),
  };
}

function parseManifest() {
  const textValue = readFileSync(resolve(REPOSITORY_ROOT, SOURCE_PINS.artifact_manifest_path), "utf8");
  return new Map(
    textValue
      .trim()
      .split(/\r?\n/u)
      .map((line) => {
        const match = /^([0-9a-f]{64})  (.+)$/u.exec(line);
        if (!match) fail(`invalid manifest line: ${line}`);
        return [match[2], match[1]];
      }),
  );
}

function routingFixture() {
  const templatePath = resolve(REPOSITORY_ROOT, "docs/evidence/W-0164/recipient-routing-input.template.json");
  const document = JSON.parse(readFileSync(templatePath, "utf8"));
  document.status = "PARTIAL_READY";
  for (const batchId of ["D-01", "D-02"]) {
    const batch = document.batches.find((value) => value.batch === batchId);
    batch.recipient_identity = batchId === "D-01" ? "M3_PRIMARY_OWNER" : "SECURITY_PRIMARY_OWNER";
    batch.role_organization = batchId === "D-01" ? "M3 CONTRACT OWNER" : "SECURITY PLATFORM OWNER";
    batch.authority_source_ref = `ROLE_ASSIGNMENT:${batchId}-OWNER`;
    batch.channel_kind = "GITLAB_ISSUE";
    batch.destination_ref = `PROJECT:EXTERNAL-GOVERNANCE/${batchId}`;
    batch.due_at = "2026-09-10T17:00:00+07:00";
    batch.dispatch_authorized_by = "M8_OWNER";
    batch.dispatch_authorized_at = "2026-09-03T12:00:00+07:00";
    batch.state = "READY_FOR_HASH_RECHECK_AND_DISPATCH";
  }
  return document;
}

function responseFixture(receiptD01, receiptD02) {
  const manifest = parseManifest();
  const templatePath = resolve(REPOSITORY_ROOT, "docs/evidence/W-0165/decision-response-input.template.json");
  const document = JSON.parse(readFileSync(templatePath, "utf8"));
  document.status = "RESPONSE_RECEIVED_PENDING_VALIDATION";
  const artifacts = [
    "plan/ivr-orther/m8-07-target-v1-shared-callback-handoff-2026-09-03.md",
    SOURCE_PINS.dispatch_pack_path,
  ].map((path) => ({ path, sha256: manifest.get(path) }));
  const response = (group, batch, receiptId, index) => ({
    response_id: `RESPONSE:S-05-${group}-01`,
    dispatch_batch: batch,
    dispatch_receipt_ref: receiptId,
    sheet_id: "S-05",
    decision_ids: ["S-05"],
    decision: "APPROVE",
    decision_text: `Explicit synthetic approval for S-05 by ${group} used only in the W-0170 self-test.`,
    signer_identity_alias: `${group}_SIGNER`,
    signer_role: `${group} accountable decision owner`,
    signer_organization: `${group} organization alias`,
    authority_source_ref: `ROLE_ASSIGNMENT:S-05-${group}`,
    accepted_artifacts: artifacts,
    responded_at: `2026-09-03T14:0${index}:00+07:00`,
    received_at: `2026-09-03T14:1${index}:00+07:00`,
    scope_environments: ["CONTRACT"],
    effective_cutover: {
      effective_at: "2026-09-04T00:00:00+07:00",
      cutover_at: "2026-09-05T00:00:00+07:00",
      compatibility_window: "Retain the previous contract until shared verification has completed.",
    },
    rollback_or_rejection_path: "Keep callback delivery disabled and restore the prior contract if shared verification fails.",
    evidence_references: [`TICKET:S-05-${group}-EVIDENCE`],
    residual_blockers: [],
    external_response_ref: `RESPONSE:S-05-${group}-ARTIFACT`,
    external_response_sha256: `${index + 1}`.repeat(64),
    limitations: ["SYNTHETIC_SELF_TEST_ONLY"],
    state: "RECEIVED_UNVERIFIED",
  });
  document.responses = [
    response("M3_CONTRACT", "D-01", receiptD01, 0),
    response("SECURITY", "D-02", receiptD02, 1),
    response("PLATFORM", "D-02", receiptD02, 2),
  ];
  return document;
}

function writeJson(pathValue, document) {
  writeFileSync(pathValue, `${JSON.stringify(document, null, 2)}\n`, { encoding: "utf8", flag: "wx" });
  return sha256(readFileSync(pathValue));
}

function closureFixture(routingPath, routingHash, responsePath, responseHash) {
  const receiptD01 = "RECEIPT:D-01-SYNTHETIC-01";
  const receiptD02 = "RECEIPT:D-02-SYNTHETIC-01";
  const document = templateDocument();
  document.status = "CLOSURE_REVIEW_PENDING";
  document.routing_input = { path: routingPath, sha256: routingHash };
  document.response_bundle = { path: responsePath, sha256: responseHash };
  const receipt = (receiptId, batch, recipients, minute) => ({
    receipt_id: receiptId,
    batch,
    routing_input_sha256: routingHash,
    system_of_record_kind: "GITLAB_ISSUE",
    destination_ref: `PROJECT:EXTERNAL-GOVERNANCE/${batch}`,
    system_of_record_ref: `TICKET:${batch}-SYNTHETIC-01`,
    external_receipt_sha256: (batch === "D-01" ? "a" : "b").repeat(64),
    sender_identity_alias: "M8_DISPATCHER",
    recipient_identity_aliases: recipients,
    sent_at: `2026-09-03T13:${minute}:00+07:00`,
    delivered_at: `2026-09-03T13:${Number(minute) + 1}:00+07:00`,
    delivery_state: "DELIVERED",
  });
  document.dispatch_receipts = [
    receipt(receiptD01, "D-01", ["M3_PRIMARY_OWNER"], "10"),
    receipt(receiptD02, "D-02", ["SECURITY_PRIMARY_OWNER", "PLATFORM_OWNER"], "20"),
  ];
  const groups = ["M3_CONTRACT", "SECURITY", "PLATFORM"];
  document.authority_attestations = groups.map((group, index) => ({
    attestation_id: `ATTESTATION:S-05-${group}-01`,
    response_id: `RESPONSE:S-05-${group}-01`,
    sheet_id: "S-05",
    authority_group: group,
    authority_source_ref: `ROLE_ASSIGNMENT:S-05-${group}`,
    authority_evidence_ref: `ROLE_ASSIGNMENT:S-05-${group}`,
    authority_evidence_sha256: `${index + 4}`.repeat(64),
    verified_by_alias: "CHIEF_AUDITOR",
    verified_at: "2026-09-03T15:00:00+07:00",
    state: "AUTHORITY_VERIFIED",
  }));
  document.sheet_closures = [
    {
      sheet_id: "S-05",
      required_authority_groups: groups,
      accepted_response_ids: groups.map((group) => `RESPONSE:S-05-${group}-01`),
      receipt_ids: [receiptD01, receiptD02],
      decision_ids: ["S-05"],
      state: "DECISION_PROVENANCE_CLOSED",
    },
  ];
  return document;
}

function clone(value) {
  return JSON.parse(JSON.stringify(value));
}

function expectFailure(base, mutation, label) {
  const candidate = clone(base);
  mutation(candidate);
  let failed = false;
  try {
    validateDocument(candidate, "input");
  } catch {
    failed = true;
  }
  if (!failed) fail(`self-test mutation was accepted: ${label}`);
}

function runSelfTest() {
  const artifactsRoot = resolve(REPOSITORY_ROOT, "ci-artifacts");
  mkdirSync(artifactsRoot, { recursive: true });
  const tempDirectory = mkdtempSync(resolve(artifactsRoot, "w0169-selftest-"));
  if (!isConfined(tempDirectory)) fail("self-test directory escaped repository root");
  try {
    const routingPath = resolve(tempDirectory, "routing.json");
    const routingHash = writeJson(routingPath, routingFixture());
    const receiptD01 = "RECEIPT:D-01-SYNTHETIC-01";
    const receiptD02 = "RECEIPT:D-02-SYNTHETIC-01";
    const responsePath = resolve(tempDirectory, "responses.json");
    const responseHash = writeJson(responsePath, responseFixture(receiptD01, receiptD02));
    const relativeRouting = relative(REPOSITORY_ROOT, routingPath).replaceAll("\\", "/");
    const relativeResponse = relative(REPOSITORY_ROOT, responsePath).replaceAll("\\", "/");
    const valid = closureFixture(relativeRouting, routingHash, relativeResponse, responseHash);
    const result = validateDocument(valid, "input");
    if (result.closedSheetCount !== 1 || result.closedSheets[0] !== "S-05") fail("positive self-test did not close S-05");

    const mutations = [
      ["receipt-hash", (v) => (v.dispatch_receipts[0].external_receipt_sha256 = "bad")],
      ["receipt-ref", (v) => (v.dispatch_receipts[0].receipt_id = "RECEIPT:UNKNOWN")],
      ["routing-hash", (v) => (v.dispatch_receipts[0].routing_input_sha256 = "a".repeat(64))],
      ["channel", (v) => (v.dispatch_receipts[0].system_of_record_kind = "JIRA_TICKET")],
      ["destination", (v) => (v.dispatch_receipts[0].destination_ref = "PROJECT:WRONG")],
      ["primary-recipient", (v) => (v.dispatch_receipts[0].recipient_identity_aliases = ["OTHER_OWNER"])],
      ["phone-like-alias", (v) => (v.dispatch_receipts[0].recipient_identity_aliases = ["84901234567"])],
      ["sent-before-authority", (v) => (v.dispatch_receipts[0].sent_at = "2026-09-03T11:00:00+07:00")],
      ["delivery-order", (v) => (v.dispatch_receipts[0].delivered_at = "2026-09-03T12:59:00+07:00")],
      ["delivery-state", (v) => (v.dispatch_receipts[0].delivery_state = "SENT")],
      ["missing-attestation", (v) => v.authority_attestations.pop()],
      ["authority-source", (v) => (v.authority_attestations[0].authority_source_ref = "ROLE_ASSIGNMENT:OTHER")],
      ["same-signer-verifier", (v) => (v.authority_attestations[0].verified_by_alias = "M3_CONTRACT_SIGNER")],
      ["authority-hash", (v) => (v.authority_attestations[0].authority_evidence_sha256 = "bad")],
      ["authority-state", (v) => (v.authority_attestations[0].state = "UNVERIFIED")],
      ["missing-quorum", (v) => v.sheet_closures[0].accepted_response_ids.pop()],
      ["wrong-quorum-order", (v) => v.sheet_closures[0].required_authority_groups.reverse()],
      ["wrong-receipts", (v) => (v.sheet_closures[0].receipt_ids = [receiptD01])],
      ["wrong-decision-ids", (v) => (v.sheet_closures[0].decision_ids = ["S-06"])],
      ["wrong-state", (v) => (v.sheet_closures[0].state = "SHEET_CLOSED")],
      ["safety", (v) => (v.safety.production_gate_promoted = true)],
    ];
    mutations.forEach(([label, mutation]) => expectFailure(valid, mutation, label));
    return { valid: 1, refusals: mutations.length };
  } finally {
    if (isConfined(tempDirectory) && tempDirectory.startsWith(artifactsRoot)) {
      rmSync(tempDirectory, { recursive: true, force: true });
    }
  }
}

function usage() {
  return [
    "Usage:",
    "  node deploy/ci/scripts/external-decision-closure-validator.mjs --check-template <json>",
    "  node deploy/ci/scripts/external-decision-closure-validator.mjs --input <json>",
    "  node deploy/ci/scripts/external-decision-closure-validator.mjs --self-test",
  ].join("\n");
}

function main(argv) {
  if (argv.length === 1 && argv[0] === "--self-test") {
    const result = runSelfTest();
    console.log(`W0170_SELFTEST_PASS valid=${result.valid} refusals=${result.refusals}`);
    return;
  }
  if (argv.length !== 2 || !["--check-template", "--input"].includes(argv[0])) fail(usage());
  const mode = argv[0] === "--check-template" ? "template" : "input";
  const result = validateFile(argv[1], mode);
  if (mode === "template") {
    console.log(`CLOSURE_TEMPLATE_VALID_NOT_READY sha256=${result.inputSha256}`);
  } else {
    console.log(
      `DECISION_PROVENANCE_CLOSURE_VALID_NO_GATE_PROMOTION sheets=${result.closedSheetCount} ` +
        `sheet_ids=${result.closedSheets.join(",")} sha256=${result.inputSha256}`,
    );
  }
}

try {
  main(process.argv.slice(2));
} catch (error) {
  console.error(`W0170_VALIDATION_FAILED: ${error.message}`);
  process.exitCode = 1;
}
