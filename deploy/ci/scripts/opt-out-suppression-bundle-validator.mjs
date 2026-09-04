#!/usr/bin/env node

import { createHash } from "node:crypto";
import {
  lstatSync,
  mkdtempSync,
  mkdirSync,
  readFileSync,
  realpathSync,
  rmSync,
  writeFileSync,
} from "node:fs";
import { dirname, isAbsolute, relative, resolve, sep } from "node:path";
import { fileURLToPath } from "node:url";

const SCRIPT_PATH = fileURLToPath(import.meta.url);
const REPOSITORY_ROOT = resolve(dirname(SCRIPT_PATH), "../../..");
const ARTIFACT_ROOT = resolve(REPOSITORY_ROOT, "ci-artifacts");
const MAX_INPUT_BYTES = 768 * 1024;
const SCHEMA_VERSION = "m8-opt-out-suppression-decision-bundle.v1";
const WORK_ID = "W-0187";
const PENDING = "PENDING_EXTERNAL_INPUT";
const HEX64 = /^[a-f0-9]{64}$/u;
const HEX40 = /^[a-f0-9]{40}$/u;

const SOURCE_PINS = Object.freeze([
  [
    "plan/ivr-orther/m8-08-opt-out-suppression-decision-pack-2026-09-03.md",
    "ec0c4e9be8500b094295a66809a8994f5c2724a74bff05f299b87fdb8becd047",
  ],
  [
    "docs/evidence/W-0161/README.md",
    "9a9de5faad8c9fe4a7c45866ca38561dffda6423d2e63ddf9ba3595a369d832b",
  ],
  [
    "integration-requirements/01-sales-platform-requirements.md",
    "68fc49cdd979fee66153a6fa5748623a69dfff68bb06054304009d46011ba894",
  ],
  [
    "integration-requirements/05-open-contract-questions.md",
    "51a7734983a82b64d4610e24825ec055a9055492485f578e81f02eb2fb86d5c6",
  ],
  [
    "integration-requirements/06-module-3-api-handover.md",
    "b676a32d4ba51b9f345eb3d32e21d793216f4011e98bbfc9dc8d2867997ba08a",
  ],
  [
    "src/Ivr.Domain/Policies/OptOutSuppression.cs",
    "77f980556ee188c2e5bbbed9850229e0f5aa2a9973ae5965b9e1cf2bb4667c2c",
  ],
  [
    "src/Ivr.Domain/Policies/EligibilityRules.cs",
    "e8cb49ae03ccfd178b20002f977e1de9e0f1d425383c0d2192864a7d61ef3a58",
  ],
  [
    "src/Ivr.Infrastructure/Crm/SuppressionProposer.cs",
    "04eb94fe74de4ebc8613b37c0bdaf17ebb9e2171d639368291bd6abe311be78e",
  ],
  [
    "src/Ivr.Infrastructure/Intake/TaskIntakeService.cs",
    "6cf48edf19bb31dd4befb1e211bcf468e3d32b8fc39d0ea742ef9e2d934fbd4d",
  ],
]);

const DECISION_IDS = Array.from({ length: 11 }, (_, index) =>
  `OPT-${String(index + 1).padStart(2, "0")}`,
);
const ARTIFACT_IDS = [
  "CRM_PROPOSAL_CONTRACT",
  "CRM_REGISTRY_CONTRACT",
  "CRM_IDENTITY_CONTRACT",
  "M3_RELAY_PRODUCER_CONTRACT",
  "LEGAL_PRIVACY_PACKET",
  "SECURITY_PLATFORM_PACKET",
  "SHARED_E2E_PLAN",
  "RELEASE_PACKET",
];
const SIGNOFF_ROLES = [
  "PROJECT_OWNER_M8",
  "CRM_M31",
  "M3_ORDER_CORE",
  "LEGAL_PRIVACY",
  "PRODUCT",
  "SECURITY_PLATFORM",
];
const TEST_IDS = [
  "C9-T01-EXPLICIT-SIGNAL-PROOF",
  "C9-T02-REJECTED-NON-MUTATION",
  "C9-T03-DTMF-NON-OPTOUT",
  "C9-T04-OPAQUE-IDENTITY-NO-RAW-CONTACT",
  "C9-T05-IDEMPOTENT-DUPLICATE",
  "C9-T06-CHANGED-BODY-CONFLICT",
  "C9-T07-ACK-INVALID-RETRY-DLQ",
  "C9-T08-OUTAGE-RECOVERY",
  "C9-T09-REVERSAL-NEWER-PROOF",
  "C9-T10-ACTIVE-RESTRICTION-BLOCK",
  "C9-T11-FRESHNESS-REVOKE",
  "C9-T12-RETENTION-DSAR-LEGAL-HOLD",
  "C9-T13-ADMIN-AUTHORIZATION",
];
const ACK_STATES = [
  "ACCEPTED",
  "DUPLICATE",
  "REJECTED_INVALID",
  "RETRYABLE",
  "TERMINAL_REJECTED",
];
const RETENTION_CLASSES = ["OBSERVATION", "PROPOSAL", "ACK", "IDEMPOTENCY_KEY", "AUDIT"];

const ROOT_KEYS = [
  "schema_version",
  "work_id",
  "status",
  "source_artifacts",
  "context",
  "decisions",
  "production_contract",
  "test_plans",
  "external_artifacts",
  "signoffs",
  "safety",
];
const SOURCE_KEYS = ["path", "sha256"];
const CONTEXT_KEYS = [
  "contract_version",
  "m8_candidate_sha",
  "m3_candidate_sha",
  "decision_bundle_sha256",
  "reviewed_at",
];
const DECISION_KEYS = ["decision_id", "state", "approval_ref", "approval_sha256"];
const CONTRACT_KEYS = [
  "signal",
  "weak_signal",
  "identity",
  "topology",
  "idempotency",
  "ack_lifecycle",
  "writer",
  "reversal",
  "retention",
  "inbound_freshness",
  "admin_authority",
];
const SIGNAL_KEYS = [
  "signal_kind",
  "wording_script_version",
  "wording_approval_ref",
  "proof_schema_ref",
  "proof_retention_class",
  "proof_required",
  "rejected_is_opt_out",
  "dtmf_zero_is_opt_out",
  "dtmf_one_is_opt_out",
];
const WEAK_KEYS = [
  "mode",
  "threshold_count",
  "window_seconds",
  "dedupe_key_ref",
  "auto_registry_mutation",
  "creates_review_only",
];
const IDENTITY_KEYS = [
  "reference_field",
  "namespace",
  "issuer",
  "stability_contract_ref",
  "rotation_merge_contract_ref",
  "opaque_only",
  "raw_phone_allowed",
  "ivr_owned_hashing",
];
const TOPOLOGY_KEYS = [
  "route",
  "contract_ref",
  "service_identity_ref",
  "network_path_ref",
  "direct_ivr_to_crm_egress",
];
const IDEMPOTENCY_KEYS = [
  "key_components",
  "same_body_outcome",
  "changed_body_outcome",
  "retention_seconds",
];
const ACK_KEYS = ["states", "correlation_field", "retry_policy_ref", "dlq_ref"];
const WRITER_KEYS = [
  "registry_owner",
  "authorization_ref",
  "audit_actor_field",
  "negative_authorization_test_ref",
  "delegated_writer_authority",
  "ivr_writes_effective_suppression",
];
const REVERSAL_KEYS = [
  "state_machine_ref",
  "newer_proof_required",
  "effective_timestamp_field",
  "reversal_timestamp_field",
  "merge_unlink_contract_ref",
  "appeal_procedure_ref",
];
const RETENTION_KEYS = ["pending_crm_bounded", "legal_hold_supported", "rows"];
const RETENTION_ROW_KEYS = ["data_class", "retention_days", "owner", "purge_test_ref"];
const INBOUND_KEYS = [
  "authority",
  "pre_task_revalidation",
  "pre_attempt_revalidation",
  "mid_window_strategy",
  "unknown_outcome",
  "unavailable_outcome",
  "active_restriction_blocks_dispatch",
  "d06_contract_ref",
];
const ADMIN_KEYS = [
  "permission",
  "review_action",
  "review_outcome_contract_ref",
  "audit_ref",
  "dual_control_required",
  "direct_registry_mutation",
];
const TEST_KEYS = ["test_id", "state", "owner", "plan_ref", "plan_sha256"];
const ARTIFACT_KEYS = ["artifact_id", "ref", "sha256"];
const SIGNOFF_KEYS = [
  "role",
  "signer_alias",
  "verifier_alias",
  "authority_ref",
  "approval_ref",
  "approval_sha256",
  "approved_at",
  "scope",
  "bound_decision_bundle_sha256",
  "state",
];
const SAFETY_KEYS = [
  "contains_raw_contact",
  "contains_credentials_or_secrets",
  "raw_external_response_embedded",
  "consent_inferred_from_rejected_or_dtmf",
  "direct_ivr_crm_egress_enabled",
  "local_effective_suppression_writer_enabled",
  "production_gate_promoted",
  "real_customer_call_allowed",
];

function fail(message) {
  throw new Error(message);
}

function sha256(bytes) {
  return createHash("sha256").update(bytes).digest("hex");
}

function clone(value) {
  return JSON.parse(JSON.stringify(value));
}

function canonicalize(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalize).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.keys(value)
      .sort()
      .map((key) => `${JSON.stringify(key)}:${canonicalize(value[key])}`)
      .join(",")}}`;
  }
  return JSON.stringify(value);
}

function decisionBundleHash(document) {
  return sha256(
    Buffer.from(
      canonicalize({
        contract_version: document.context.contract_version,
        decisions: document.decisions,
        production_contract: document.production_contract,
        test_plans: document.test_plans,
        external_artifacts: document.external_artifacts,
      }),
      "utf8",
    ),
  );
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

function assertString(value, label, minimum = 3, maximum = 300) {
  if (typeof value !== "string" || value.length < minimum || value.length > maximum) {
    fail(`${label} must be a string of ${minimum}..${maximum} characters`);
  }
  if (value.trim() !== value) fail(`${label} must not have surrounding whitespace`);
  if (/[\u0000-\u001f\u007f]/u.test(value)) fail(`${label} contains a control character`);
}

function assertSafeRef(value, label) {
  assertString(value, label, 4, 400);
  if (/[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}/iu.test(value)) {
    fail(`${label} contains an email-like value`);
  }
  if (/(?:^|\D)(?:\+?\d[\s().-]*){9,15}(?:$|\D)/u.test(value)) {
    fail(`${label} contains a phone-like value`);
  }
  if (/[?#]/u.test(value)) fail(`${label} must not contain query or fragment material`);
  if (
    /(?:password|passwd|bearer\s+|api[_ -]?key|access[_ -]?token|private[_ -]?key|client[_ -]?secret)\s*[:=]?/iu.test(
      value,
    )
  ) {
    fail(`${label} contains credential- or secret-like material`);
  }
}

function assertHash(value, label) {
  if (typeof value !== "string" || !HEX64.test(value)) fail(`${label} must be lowercase SHA-256`);
}

function assertCommit(value, label) {
  if (typeof value !== "string" || !HEX40.test(value)) fail(`${label} must be lowercase 40-hex`);
}

function assertTimestamp(value, label) {
  assertString(value, label, 20, 35);
  if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,3})?(?:Z|[+-]\d{2}:\d{2})$/u.test(value)) {
    fail(`${label} must be ISO-8601 with explicit timezone`);
  }
  if (!Number.isFinite(Date.parse(value))) fail(`${label} is not a valid timestamp`);
}

function isConfined(path) {
  const rel = relative(REPOSITORY_ROOT, path);
  return rel !== "" && rel !== ".." && !rel.startsWith(`..${sep}`) && !isAbsolute(rel);
}

function readConfinedUtf8File(inputPath) {
  const resolved = resolve(REPOSITORY_ROOT, inputPath);
  if (!isConfined(resolved)) fail("input path must stay inside the repository root");
  const entry = lstatSync(resolved);
  if (!entry.isFile() || entry.isSymbolicLink()) fail("input must be a regular non-symlink file");
  if (entry.size > MAX_INPUT_BYTES) fail(`input exceeds ${MAX_INPUT_BYTES} bytes`);
  const real = realpathSync(resolved);
  if (!isConfined(real)) fail("resolved input path escapes repository root");
  const bytes = readFileSync(real);
  if (bytes.length >= 3 && bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf) {
    fail("UTF-8 BOM is not allowed");
  }
  let text;
  try {
    text = new TextDecoder("utf-8", { fatal: true }).decode(bytes);
  } catch {
    fail("input must be strict UTF-8");
  }
  return { bytes, text };
}

function rejectDuplicateJsonKeys(text) {
  let position = 0;
  function whitespace() {
    while (/\s/u.test(text[position] ?? "")) position += 1;
  }
  function stringValue() {
    if (text[position] !== '"') fail("invalid JSON string");
    const start = position++;
    while (position < text.length) {
      if (text[position] === "\\") {
        position += 2;
        continue;
      }
      if (text[position] === '"') {
        position += 1;
        try {
          return JSON.parse(text.slice(start, position));
        } catch {
          fail("invalid JSON string escape");
        }
      }
      if (text.charCodeAt(position) < 0x20) fail("invalid JSON control character");
      position += 1;
    }
    fail("unterminated JSON string");
  }
  function literal() {
    const match = /^(?:true|false|null|-?(?:0|[1-9]\d*)(?:\.\d+)?(?:[eE][+-]?\d+)?)/u.exec(
      text.slice(position),
    );
    if (!match) fail("invalid JSON value");
    position += match[0].length;
  }
  function array() {
    position += 1;
    whitespace();
    if (text[position] === "]") return void (position += 1);
    while (position < text.length) {
      value();
      whitespace();
      if (text[position] === "]") return void (position += 1);
      if (text[position] !== ",") fail("invalid JSON array separator");
      position += 1;
      whitespace();
    }
    fail("unterminated JSON array");
  }
  function object() {
    position += 1;
    const keys = new Set();
    whitespace();
    if (text[position] === "}") return void (position += 1);
    while (position < text.length) {
      const key = stringValue();
      if (keys.has(key)) fail(`duplicate JSON key: ${key}`);
      keys.add(key);
      whitespace();
      if (text[position] !== ":") fail("invalid JSON object separator");
      position += 1;
      whitespace();
      value();
      whitespace();
      if (text[position] === "}") return void (position += 1);
      if (text[position] !== ",") fail("invalid JSON object separator");
      position += 1;
      whitespace();
    }
    fail("unterminated JSON object");
  }
  function value() {
    whitespace();
    if (text[position] === "{") object();
    else if (text[position] === "[") array();
    else if (text[position] === '"') stringValue();
    else literal();
  }
  value();
  whitespace();
  if (position !== text.length) fail("unexpected content after JSON document");
}

function parseInput(inputPath) {
  const { bytes, text } = readConfinedUtf8File(inputPath);
  rejectDuplicateJsonKeys(text);
  let document;
  try {
    document = JSON.parse(text);
  } catch (error) {
    fail(`malformed JSON: ${error.message}`);
  }
  return { bytes, document };
}

function verifySourceArtifacts(rows) {
  if (!Array.isArray(rows) || rows.length !== SOURCE_PINS.length) {
    fail(`source_artifacts must contain exactly ${SOURCE_PINS.length} rows`);
  }
  rows.forEach((row, index) => {
    assertExactKeys(row, SOURCE_KEYS, `source_artifacts[${index}]`);
    const [path, expectedHash] = SOURCE_PINS[index];
    if (row.path !== path || row.sha256 !== expectedHash) {
      fail(`source_artifacts[${index}] does not match the pinned artifact`);
    }
    const bytes = readConfinedUtf8File(path).bytes;
    if (sha256(bytes) !== expectedHash) fail(`${path} drifted from pinned SHA-256`);
  });
}

function validateOrderedRows(rows, ids, keys, idField, label) {
  if (!Array.isArray(rows) || rows.length !== ids.length) {
    fail(`${label} must contain exactly ${ids.length} rows`);
  }
  rows.forEach((row, index) => {
    assertExactKeys(row, keys, `${label}[${index}]`);
    if (row[idField] !== ids[index]) fail(`${label}[${index}].${idField} must be ${ids[index]}`);
  });
}

function validatePending(document) {
  if (document.status !== "PENDING_EXTERNAL_INPUT") fail("pending template status is invalid");
  assertExactKeys(document.context, CONTEXT_KEYS, "context");
  for (const key of CONTEXT_KEYS) {
    if (document.context[key] !== PENDING) fail(`context.${key} must remain pending`);
  }
  validateOrderedRows(document.decisions, DECISION_IDS, DECISION_KEYS, "decision_id", "decisions");
  document.decisions.forEach((row, index) => {
    for (const key of ["state", "approval_ref", "approval_sha256"]) {
      if (row[key] !== PENDING) fail(`decisions[${index}].${key} must remain pending`);
    }
  });

  const pendingSections = [
    ["signal", SIGNAL_KEYS],
    ["weak_signal", WEAK_KEYS],
    ["identity", IDENTITY_KEYS],
    ["topology", TOPOLOGY_KEYS],
    ["idempotency", IDEMPOTENCY_KEYS],
    ["ack_lifecycle", ACK_KEYS],
    ["writer", WRITER_KEYS],
    ["reversal", REVERSAL_KEYS],
    ["inbound_freshness", INBOUND_KEYS],
    ["admin_authority", ADMIN_KEYS],
  ];
  for (const [sectionName, keys] of pendingSections) {
    const section = document.production_contract[sectionName];
    assertExactKeys(section, keys, `production_contract.${sectionName}`);
    for (const key of keys) {
      if (section[key] !== PENDING) {
        fail(`production_contract.${sectionName}.${key} must remain pending`);
      }
    }
  }
  const retention = document.production_contract.retention;
  assertExactKeys(retention, RETENTION_KEYS, "production_contract.retention");
  if (
    retention.pending_crm_bounded !== PENDING ||
    retention.legal_hold_supported !== PENDING ||
    !Array.isArray(retention.rows) ||
    retention.rows.length !== 0
  ) {
    fail("production_contract.retention must remain pending with zero rows");
  }

  validateOrderedRows(document.test_plans, TEST_IDS, TEST_KEYS, "test_id", "test_plans");
  document.test_plans.forEach((row) => {
    if (row.state !== "PLANNED_NOT_RUN") fail(`${row.test_id} must remain PLANNED_NOT_RUN`);
    if (row.owner !== PENDING || row.plan_ref !== PENDING || row.plan_sha256 !== PENDING) {
      fail(`${row.test_id} pending fields drifted`);
    }
  });
  validateOrderedRows(
    document.external_artifacts,
    ARTIFACT_IDS,
    ARTIFACT_KEYS,
    "artifact_id",
    "external_artifacts",
  );
  document.external_artifacts.forEach((row) => {
    if (row.ref !== PENDING || row.sha256 !== PENDING) fail(`${row.artifact_id} must remain pending`);
  });
  validateOrderedRows(document.signoffs, SIGNOFF_ROLES, SIGNOFF_KEYS, "role", "signoffs");
  document.signoffs.forEach((row) => {
    for (const key of SIGNOFF_KEYS.filter((key) => key !== "role")) {
      if (row[key] !== PENDING) fail(`${row.role}.${key} must remain pending`);
    }
  });
  return { decisions: DECISION_IDS.length, tests: TEST_IDS.length };
}

function assertBoolean(value, expected, label) {
  if (value !== expected) fail(`${label} must be ${expected}`);
}

function validateProductionContract(contract) {
  assertExactKeys(contract, CONTRACT_KEYS, "production_contract");

  const signal = contract.signal;
  assertExactKeys(signal, SIGNAL_KEYS, "production_contract.signal");
  if (signal.signal_kind !== "EXPLICIT_CUSTOMER_ACTION_WITH_PROOF") {
    fail("signal_kind must be EXPLICIT_CUSTOMER_ACTION_WITH_PROOF");
  }
  for (const key of [
    "wording_script_version",
    "wording_approval_ref",
    "proof_schema_ref",
    "proof_retention_class",
  ]) assertSafeRef(signal[key], `signal.${key}`);
  assertBoolean(signal.proof_required, true, "signal.proof_required");
  assertBoolean(signal.rejected_is_opt_out, false, "signal.rejected_is_opt_out");
  assertBoolean(signal.dtmf_zero_is_opt_out, false, "signal.dtmf_zero_is_opt_out");
  assertBoolean(signal.dtmf_one_is_opt_out, false, "signal.dtmf_one_is_opt_out");

  const weak = contract.weak_signal;
  assertExactKeys(weak, WEAK_KEYS, "production_contract.weak_signal");
  if (!new Set(["DISABLED", "MANUAL_REVIEW_ONLY"]).has(weak.mode)) fail("weak_signal.mode is invalid");
  assertBoolean(weak.auto_registry_mutation, false, "weak_signal.auto_registry_mutation");
  assertSafeRef(weak.dedupe_key_ref, "weak_signal.dedupe_key_ref");
  if (weak.mode === "DISABLED") {
    if (weak.threshold_count !== 0 || weak.window_seconds !== 0 || weak.creates_review_only !== false) {
      fail("disabled weak signal must have zero threshold/window and no review creation");
    }
  } else {
    if (!Number.isInteger(weak.threshold_count) || weak.threshold_count < 2 || weak.threshold_count > 100) {
      fail("manual-review threshold_count must be 2..100");
    }
    if (!Number.isInteger(weak.window_seconds) || weak.window_seconds < 60 || weak.window_seconds > 31_536_000) {
      fail("manual-review window_seconds must be 60..31536000");
    }
    assertBoolean(weak.creates_review_only, true, "weak_signal.creates_review_only");
  }

  const identity = contract.identity;
  assertExactKeys(identity, IDENTITY_KEYS, "production_contract.identity");
  for (const key of ["reference_field", "namespace", "issuer", "stability_contract_ref", "rotation_merge_contract_ref"]) {
    assertSafeRef(identity[key], `identity.${key}`);
  }
  if (!/^[a-z][a-z0-9_]{2,63}$/u.test(identity.reference_field)) {
    fail("identity.reference_field must be a lower-snake-case contract field");
  }
  if (/(?:raw|phone|e164|msisdn|customer_id)/iu.test(identity.reference_field)) {
    fail("identity.reference_field must not name a raw or direct customer identifier");
  }
  assertBoolean(identity.opaque_only, true, "identity.opaque_only");
  assertBoolean(identity.raw_phone_allowed, false, "identity.raw_phone_allowed");
  assertBoolean(identity.ivr_owned_hashing, false, "identity.ivr_owned_hashing");

  const topology = contract.topology;
  assertExactKeys(topology, TOPOLOGY_KEYS, "production_contract.topology");
  if (!new Set(["M3_RELAY", "CRM_PULL_QUEUE"]).has(topology.route)) fail("topology.route is invalid");
  for (const key of ["contract_ref", "service_identity_ref", "network_path_ref"]) {
    assertSafeRef(topology[key], `topology.${key}`);
  }
  assertBoolean(topology.direct_ivr_to_crm_egress, false, "topology.direct_ivr_to_crm_egress");

  const idempotency = contract.idempotency;
  assertExactKeys(idempotency, IDEMPOTENCY_KEYS, "production_contract.idempotency");
  const requiredParts = ["SIGNAL_ID", "POLICY_VERSION", "PROPOSAL_VERSION"];
  if (!Array.isArray(idempotency.key_components) || idempotency.key_components.length < requiredParts.length) {
    fail("idempotency.key_components is incomplete");
  }
  requiredParts.forEach((part) => {
    if (!idempotency.key_components.includes(part)) fail(`idempotency key is missing ${part}`);
  });
  if (idempotency.key_components.length === 1 && idempotency.key_components[0] === "CONTACT_REFERENCE") {
    fail("contact-only idempotency is forbidden");
  }
  if (idempotency.same_body_outcome !== "RETURN_ORIGINAL_OUTCOME") fail("same-body outcome is invalid");
  if (idempotency.changed_body_outcome !== "CONFLICT") fail("changed-body outcome must be CONFLICT");
  if (!Number.isInteger(idempotency.retention_seconds) || idempotency.retention_seconds < 86_400) {
    fail("idempotency retention_seconds must be at least one day");
  }

  const ack = contract.ack_lifecycle;
  assertExactKeys(ack, ACK_KEYS, "production_contract.ack_lifecycle");
  if (!Array.isArray(ack.states) || ack.states.length !== ACK_STATES.length || ack.states.some((x, i) => x !== ACK_STATES[i])) {
    fail("ack_lifecycle.states must match the canonical ordered set");
  }
  for (const key of ["correlation_field", "retry_policy_ref", "dlq_ref"]) assertSafeRef(ack[key], `ack_lifecycle.${key}`);

  const writer = contract.writer;
  assertExactKeys(writer, WRITER_KEYS, "production_contract.writer");
  if (writer.registry_owner !== "CRM_CUSTOMER_IDENTITY") fail("effective registry owner must be CRM_CUSTOMER_IDENTITY");
  for (const key of ["authorization_ref", "audit_actor_field", "negative_authorization_test_ref"]) {
    assertSafeRef(writer[key], `writer.${key}`);
  }
  if (typeof writer.delegated_writer_authority !== "boolean") {
    fail("writer.delegated_writer_authority must be an explicit boolean decision");
  }
  assertBoolean(writer.ivr_writes_effective_suppression, false, "writer.ivr_writes_effective_suppression");

  const reversal = contract.reversal;
  assertExactKeys(reversal, REVERSAL_KEYS, "production_contract.reversal");
  for (const key of REVERSAL_KEYS.filter((key) => key !== "newer_proof_required")) {
    assertSafeRef(reversal[key], `reversal.${key}`);
  }
  assertBoolean(reversal.newer_proof_required, true, "reversal.newer_proof_required");

  const retention = contract.retention;
  assertExactKeys(retention, RETENTION_KEYS, "production_contract.retention");
  assertBoolean(retention.pending_crm_bounded, true, "retention.pending_crm_bounded");
  assertBoolean(retention.legal_hold_supported, true, "retention.legal_hold_supported");
  validateOrderedRows(retention.rows, RETENTION_CLASSES, RETENTION_ROW_KEYS, "data_class", "retention.rows");
  retention.rows.forEach((row, index) => {
    if (!Number.isInteger(row.retention_days) || row.retention_days < 1 || row.retention_days > 3650) {
      fail(`retention.rows[${index}].retention_days must be 1..3650`);
    }
    assertSafeRef(row.owner, `retention.rows[${index}].owner`);
    assertSafeRef(row.purge_test_ref, `retention.rows[${index}].purge_test_ref`);
  });

  const inbound = contract.inbound_freshness;
  assertExactKeys(inbound, INBOUND_KEYS, "production_contract.inbound_freshness");
  if (inbound.authority !== "M3_WITH_CRM_SOURCE") fail("inbound authority must be M3_WITH_CRM_SOURCE");
  assertBoolean(inbound.pre_task_revalidation, true, "inbound.pre_task_revalidation");
  if (typeof inbound.pre_attempt_revalidation !== "boolean") {
    fail("inbound.pre_attempt_revalidation must be boolean");
  }
  if (!new Set(["D06_REVOKE_CALLBACK", "SHORT_TTL_AND_RECHECK"]).has(inbound.mid_window_strategy)) {
    fail("inbound mid-window strategy is invalid");
  }
  if (!inbound.pre_attempt_revalidation && inbound.mid_window_strategy !== "D06_REVOKE_CALLBACK") {
    fail("no pre-attempt revalidation requires a D-06 revoke callback strategy");
  }
  if (inbound.unknown_outcome !== "FAIL_CLOSED" || inbound.unavailable_outcome !== "FAIL_CLOSED") {
    fail("unknown and unavailable inbound outcomes must fail closed");
  }
  assertBoolean(inbound.active_restriction_blocks_dispatch, true, "inbound.active_restriction_blocks_dispatch");
  assertSafeRef(inbound.d06_contract_ref, "inbound.d06_contract_ref");

  const admin = contract.admin_authority;
  assertExactKeys(admin, ADMIN_KEYS, "production_contract.admin_authority");
  for (const key of ["permission", "review_outcome_contract_ref", "audit_ref"]) assertSafeRef(admin[key], `admin_authority.${key}`);
  if (admin.review_action !== "ANNOTATE_OR_CREATE_PROPOSAL") fail("admin review action is invalid");
  if (typeof admin.dual_control_required !== "boolean") fail("admin dual_control_required must be boolean");
  assertBoolean(admin.direct_registry_mutation, false, "admin_authority.direct_registry_mutation");
}

function validateCompleted(document, expected) {
  if (document.status !== "COMPLETE_PENDING_IMPLEMENTATION_REVIEW") fail("completed status is invalid");
  assertExactKeys(document.context, CONTEXT_KEYS, "context");
  assertSafeRef(document.context.contract_version, "context.contract_version");
  if (/mock|candidate|draft|unapproved/iu.test(document.context.contract_version)) {
    fail("contract_version must not be mock/candidate/draft/unapproved");
  }
  assertCommit(document.context.m8_candidate_sha, "context.m8_candidate_sha");
  assertCommit(document.context.m3_candidate_sha, "context.m3_candidate_sha");
  assertHash(document.context.decision_bundle_sha256, "context.decision_bundle_sha256");
  assertTimestamp(document.context.reviewed_at, "context.reviewed_at");
  if (document.context.m8_candidate_sha !== expected.m8Commit) fail("M8 candidate SHA does not match independent pin");
  if (document.context.m3_candidate_sha !== expected.m3Commit) fail("M3 candidate SHA does not match independent pin");

  validateOrderedRows(document.decisions, DECISION_IDS, DECISION_KEYS, "decision_id", "decisions");
  document.decisions.forEach((row, index) => {
    if (row.state !== "APPROVED") fail(`decisions[${index}].state must be APPROVED`);
    assertSafeRef(row.approval_ref, `decisions[${index}].approval_ref`);
    assertHash(row.approval_sha256, `decisions[${index}].approval_sha256`);
  });

  validateProductionContract(document.production_contract);

  validateOrderedRows(document.test_plans, TEST_IDS, TEST_KEYS, "test_id", "test_plans");
  document.test_plans.forEach((row, index) => {
    if (row.state !== "SIGNED_PLAN_NOT_EXECUTED") fail(`test_plans[${index}].state must be SIGNED_PLAN_NOT_EXECUTED`);
    assertSafeRef(row.owner, `test_plans[${index}].owner`);
    assertSafeRef(row.plan_ref, `test_plans[${index}].plan_ref`);
    assertHash(row.plan_sha256, `test_plans[${index}].plan_sha256`);
  });

  validateOrderedRows(
    document.external_artifacts,
    ARTIFACT_IDS,
    ARTIFACT_KEYS,
    "artifact_id",
    "external_artifacts",
  );
  document.external_artifacts.forEach((row, index) => {
    assertSafeRef(row.ref, `external_artifacts[${index}].ref`);
    assertHash(row.sha256, `external_artifacts[${index}].sha256`);
    if (row.sha256 !== expected.artifacts[row.artifact_id]) {
      fail(`${row.artifact_id} SHA-256 does not match independent pin`);
    }
  });

  const computed = decisionBundleHash(document);
  if (document.context.decision_bundle_sha256 !== computed) fail("decision bundle canonical SHA-256 mismatch");
  if (expected.bundleHash !== computed) fail("decision bundle does not match independent pin");

  validateOrderedRows(document.signoffs, SIGNOFF_ROLES, SIGNOFF_KEYS, "role", "signoffs");
  const signers = new Set();
  document.signoffs.forEach((row, index) => {
    for (const key of ["signer_alias", "verifier_alias", "authority_ref", "approval_ref"]) {
      assertSafeRef(row[key], `signoffs[${index}].${key}`);
    }
    if (row.signer_alias === row.verifier_alias) fail(`signoffs[${index}] signer and verifier must differ`);
    if (signers.has(row.signer_alias)) fail(`duplicate signer alias ${row.signer_alias}`);
    signers.add(row.signer_alias);
    assertHash(row.approval_sha256, `signoffs[${index}].approval_sha256`);
    assertTimestamp(row.approved_at, `signoffs[${index}].approved_at`);
    if (row.scope !== "CONTRACT_AND_IMPLEMENTATION_REVIEW") fail(`signoffs[${index}].scope is invalid`);
    if (row.bound_decision_bundle_sha256 !== computed) fail(`signoffs[${index}] is not bound to decision bundle`);
    if (row.state !== "APPROVED") fail(`signoffs[${index}].state must be APPROVED`);
  });

  return { decisions: DECISION_IDS.length, tests: TEST_IDS.length, signoffs: SIGNOFF_ROLES.length };
}

export function validateOptOutSuppressionBundle(document, mode, expected = null) {
  assertExactKeys(document, ROOT_KEYS, "root");
  if (document.schema_version !== SCHEMA_VERSION) fail("schema_version is not supported");
  if (document.work_id !== WORK_ID) fail("work_id must be W-0187");
  verifySourceArtifacts(document.source_artifacts);
  assertExactKeys(document.production_contract, CONTRACT_KEYS, "production_contract");
  assertExactKeys(document.safety, SAFETY_KEYS, "safety");
  for (const key of SAFETY_KEYS) assertBoolean(document.safety[key], false, `safety.${key}`);
  if (mode === "template") return validatePending(document);
  if (!expected) fail("completed mode requires independent expected pins");
  return validateCompleted(document, expected);
}

function pendingObject(keys) {
  return Object.fromEntries(keys.map((key) => [key, PENDING]));
}

export function buildOptOutSuppressionTemplate() {
  return {
    schema_version: SCHEMA_VERSION,
    work_id: WORK_ID,
    status: "PENDING_EXTERNAL_INPUT",
    source_artifacts: SOURCE_PINS.map(([path, hash]) => ({ path, sha256: hash })),
    context: pendingObject(CONTEXT_KEYS),
    decisions: DECISION_IDS.map((decision_id) => ({
      decision_id,
      state: PENDING,
      approval_ref: PENDING,
      approval_sha256: PENDING,
    })),
    production_contract: {
      signal: pendingObject(SIGNAL_KEYS),
      weak_signal: pendingObject(WEAK_KEYS),
      identity: pendingObject(IDENTITY_KEYS),
      topology: pendingObject(TOPOLOGY_KEYS),
      idempotency: pendingObject(IDEMPOTENCY_KEYS),
      ack_lifecycle: pendingObject(ACK_KEYS),
      writer: pendingObject(WRITER_KEYS),
      reversal: pendingObject(REVERSAL_KEYS),
      retention: { pending_crm_bounded: PENDING, legal_hold_supported: PENDING, rows: [] },
      inbound_freshness: pendingObject(INBOUND_KEYS),
      admin_authority: pendingObject(ADMIN_KEYS),
    },
    test_plans: TEST_IDS.map((test_id) => ({
      test_id,
      state: "PLANNED_NOT_RUN",
      owner: PENDING,
      plan_ref: PENDING,
      plan_sha256: PENDING,
    })),
    external_artifacts: ARTIFACT_IDS.map((artifact_id) => ({
      artifact_id,
      ref: PENDING,
      sha256: PENDING,
    })),
    signoffs: SIGNOFF_ROLES.map((role) => ({ role, ...pendingObject(SIGNOFF_KEYS.filter((key) => key !== "role")) })),
    safety: Object.fromEntries(SAFETY_KEYS.map((key) => [key, false])),
  };
}

function syntheticHash(index) {
  return ("abcdef"[index % 6]).repeat(64);
}

function buildCompletedFixture() {
  const document = buildOptOutSuppressionTemplate();
  document.status = "COMPLETE_PENDING_IMPLEMENTATION_REVIEW";
  document.context = {
    contract_version: "opt-out-suppression-v1.0.0",
    m8_candidate_sha: "a".repeat(40),
    m3_candidate_sha: "b".repeat(40),
    decision_bundle_sha256: "0".repeat(64),
    reviewed_at: "2026-09-04T15:00:00+07:00",
  };
  document.decisions = DECISION_IDS.map((decision_id, index) => ({
    decision_id,
    state: "APPROVED",
    approval_ref: `APPROVAL:C9-${decision_id}`,
    approval_sha256: syntheticHash(index),
  }));
  document.production_contract = {
    signal: {
      signal_kind: "EXPLICIT_CUSTOMER_ACTION_WITH_PROOF",
      wording_script_version: "C9-WORDING-V1",
      wording_approval_ref: "APPROVAL:C9-WORDING-V1",
      proof_schema_ref: "CONTRACT:C9-EXPLICIT-PROOF-V1",
      proof_retention_class: "RETENTION:C9-EXPLICIT-PROOF",
      proof_required: true,
      rejected_is_opt_out: false,
      dtmf_zero_is_opt_out: false,
      dtmf_one_is_opt_out: false,
    },
    weak_signal: {
      mode: "MANUAL_REVIEW_ONLY",
      threshold_count: 3,
      window_seconds: 2_592_000,
      dedupe_key_ref: "CONTRACT:C9-WEAK-SIGNAL-DEDUPE-V1",
      auto_registry_mutation: false,
      creates_review_only: true,
    },
    identity: {
      reference_field: "customer_contact_channel_id",
      namespace: "CRM-CONTACT-CHANNEL-V1",
      issuer: "CRM_M31",
      stability_contract_ref: "CONTRACT:C9-IDENTITY-STABILITY-V1",
      rotation_merge_contract_ref: "CONTRACT:C9-IDENTITY-ROTATION-MERGE-V1",
      opaque_only: true,
      raw_phone_allowed: false,
      ivr_owned_hashing: false,
    },
    topology: {
      route: "M3_RELAY",
      contract_ref: "CONTRACT:C9-M3-RELAY-V1",
      service_identity_ref: "WORKLOAD-IDENTITY:C9-M3-RELAY",
      network_path_ref: "NETWORK-POLICY:C9-M3-TO-CRM",
      direct_ivr_to_crm_egress: false,
    },
    idempotency: {
      key_components: ["SIGNAL_ID", "POLICY_VERSION", "PROPOSAL_VERSION"],
      same_body_outcome: "RETURN_ORIGINAL_OUTCOME",
      changed_body_outcome: "CONFLICT",
      retention_seconds: 2_592_000,
    },
    ack_lifecycle: {
      states: [...ACK_STATES],
      correlation_field: "proposal_id",
      retry_policy_ref: "CONTRACT:C9-ACK-RETRY-V1",
      dlq_ref: "RUNBOOK:C9-ACK-DLQ-V1",
    },
    writer: {
      registry_owner: "CRM_CUSTOMER_IDENTITY",
      authorization_ref: "AUTHZ:C9-CRM-WRITER-V1",
      audit_actor_field: "actor_identity_alias",
      negative_authorization_test_ref: "TEST:C9-FORBIDDEN-WRITER",
      delegated_writer_authority: false,
      ivr_writes_effective_suppression: false,
    },
    reversal: {
      state_machine_ref: "CONTRACT:C9-REVERSAL-STATE-V1",
      newer_proof_required: true,
      effective_timestamp_field: "effective_at",
      reversal_timestamp_field: "reversed_at",
      merge_unlink_contract_ref: "CONTRACT:C9-MERGE-UNLINK-V1",
      appeal_procedure_ref: "RUNBOOK:C9-APPEAL-V1",
    },
    retention: {
      pending_crm_bounded: true,
      legal_hold_supported: true,
      rows: RETENTION_CLASSES.map((data_class, index) => ({
        data_class,
        retention_days: [30, 90, 90, 30, 365][index],
        owner: index === 0 ? "M8_REVIEW_OWNER" : "CRM_DATA_OWNER",
        purge_test_ref: `TEST:C9-PURGE-${data_class}`,
      })),
    },
    inbound_freshness: {
      authority: "M3_WITH_CRM_SOURCE",
      pre_task_revalidation: true,
      pre_attempt_revalidation: true,
      mid_window_strategy: "D06_REVOKE_CALLBACK",
      unknown_outcome: "FAIL_CLOSED",
      unavailable_outcome: "FAIL_CLOSED",
      active_restriction_blocks_dispatch: true,
      d06_contract_ref: "CONTRACT:C9-D06-REVOKE-V1",
    },
    admin_authority: {
      permission: "IVR_OPTOUT_REVIEW",
      review_action: "ANNOTATE_OR_CREATE_PROPOSAL",
      review_outcome_contract_ref: "CONTRACT:C9-ADMIN-REVIEW-V1",
      audit_ref: "AUDIT:C9-ADMIN-DECISION-V1",
      dual_control_required: true,
      direct_registry_mutation: false,
    },
  };
  document.test_plans = TEST_IDS.map((test_id, index) => ({
    test_id,
    state: "SIGNED_PLAN_NOT_EXECUTED",
    owner: `C9_TEST_OWNER_${String(index + 1).padStart(2, "0")}`,
    plan_ref: `TEST-PLAN:${test_id}`,
    plan_sha256: syntheticHash(index + 2),
  }));
  document.external_artifacts = ARTIFACT_IDS.map((artifact_id, index) => ({
    artifact_id,
    ref: `ARTIFACT:C9-${artifact_id}`,
    sha256: syntheticHash(index + 5),
  }));
  document.context.decision_bundle_sha256 = decisionBundleHash(document);
  document.signoffs = SIGNOFF_ROLES.map((role, index) => ({
    role,
    signer_alias: `${role}_SIGNER`,
    verifier_alias: `${role}_INDEPENDENT_VERIFIER`,
    authority_ref: `ROLE-ASSIGNMENT:C9-${role}`,
    approval_ref: `APPROVAL:C9-${role}`,
    approval_sha256: syntheticHash(index + 9),
    approved_at: `2026-09-04T1${index}:30:00+07:00`,
    scope: "CONTRACT_AND_IMPLEMENTATION_REVIEW",
    bound_decision_bundle_sha256: document.context.decision_bundle_sha256,
    state: "APPROVED",
  }));
  return document;
}

function expectedFrom(document) {
  return {
    bundleHash: document.context.decision_bundle_sha256,
    m8Commit: document.context.m8_candidate_sha,
    m3Commit: document.context.m3_candidate_sha,
    artifacts: Object.fromEntries(document.external_artifacts.map((row) => [row.artifact_id, row.sha256])),
  };
}

function writeJson(path, document) {
  writeFileSync(path, `${JSON.stringify(document, null, 2)}\n`, { encoding: "utf8", flag: "wx" });
}

export function runOptOutSuppressionBundleSelfTest() {
  const templatePath = "docs/evidence/W-0187/opt-out-suppression-decision-bundle.template.json";
  const storedTemplate = parseInput(templatePath).document;
  const generatedTemplate = buildOptOutSuppressionTemplate();
  if (canonicalize(storedTemplate) !== canonicalize(generatedTemplate)) {
    fail("stored template drifted from generated canonical template");
  }
  validateOptOutSuppressionBundle(storedTemplate, "template");

  let refusals = 0;
  const pendingMutation = clone(storedTemplate);
  delete pendingMutation.production_contract.signal.proof_required;
  try {
    validateOptOutSuppressionBundle(pendingMutation, "template");
    fail("pending template schema mutation was not refused");
  } catch (error) {
    if (error.message === "pending template schema mutation was not refused") throw error;
    refusals += 1;
  }

  const base = buildCompletedFixture();
  const expected = expectedFrom(base);
  validateOptOutSuppressionBundle(base, "completed", expected);
  const disabledWeakSignal = clone(base);
  disabledWeakSignal.production_contract.weak_signal = {
    ...disabledWeakSignal.production_contract.weak_signal,
    mode: "DISABLED",
    threshold_count: 0,
    window_seconds: 0,
    creates_review_only: false,
  };
  disabledWeakSignal.context.decision_bundle_sha256 = decisionBundleHash(disabledWeakSignal);
  disabledWeakSignal.signoffs.forEach((signoff) => {
    signoff.bound_decision_bundle_sha256 = disabledWeakSignal.context.decision_bundle_sha256;
  });
  validateOptOutSuppressionBundle(
    disabledWeakSignal,
    "completed",
    expectedFrom(disabledWeakSignal),
  );
  const mutations = [
    ["source-drift", (d) => (d.source_artifacts[0].sha256 = "0".repeat(64))],
    ["m8-pin", (d, e) => (e.m8Commit = "c".repeat(40))],
    ["m3-pin", (d, e) => (e.m3Commit = "c".repeat(40))],
    ["bundle-pin", (d, e) => (e.bundleHash = "c".repeat(64))],
    ["decision-missing", (d) => d.decisions.pop()],
    ["decision-order", (d) => d.decisions.reverse()],
    ["decision-state", (d) => (d.decisions[0].state = "PENDING")],
    ["decision-hash", (d) => (d.decisions[0].approval_sha256 = "bad")],
    ["rejected-optout", (d) => (d.production_contract.signal.rejected_is_opt_out = true)],
    ["dtmf-zero-optout", (d) => (d.production_contract.signal.dtmf_zero_is_opt_out = true)],
    ["dtmf-one-optout", (d) => (d.production_contract.signal.dtmf_one_is_opt_out = true)],
    ["proof-optional", (d) => (d.production_contract.signal.proof_required = false)],
    ["weak-auto-mutation", (d) => (d.production_contract.weak_signal.auto_registry_mutation = true)],
    ["weak-threshold", (d) => (d.production_contract.weak_signal.threshold_count = 1)],
    ["identity-field", (d) => (d.production_contract.identity.reference_field = "raw_phone")],
    ["identity-raw-phone", (d) => (d.production_contract.identity.raw_phone_allowed = true)],
    ["identity-owned-hash", (d) => (d.production_contract.identity.ivr_owned_hashing = true)],
    ["direct-egress", (d) => (d.production_contract.topology.direct_ivr_to_crm_egress = true)],
    ["topology-route", (d) => (d.production_contract.topology.route = "DIRECT_IVR_CRM")],
    ["idempotency-components", (d) => (d.production_contract.idempotency.key_components = ["CONTACT_REFERENCE"])],
    ["changed-body", (d) => (d.production_contract.idempotency.changed_body_outcome = "OVERWRITE")],
    ["ack-state", (d) => d.production_contract.ack_lifecycle.states.pop()],
    ["writer-owner", (d) => (d.production_contract.writer.registry_owner = "IVR")],
    ["writer-delegation-assumed", (d) => (d.production_contract.writer.delegated_writer_authority = "ASSUMED")],
    ["writer-effective", (d) => (d.production_contract.writer.ivr_writes_effective_suppression = true)],
    ["reversal-proof", (d) => (d.production_contract.reversal.newer_proof_required = false)],
    ["retention-row", (d) => d.production_contract.retention.rows.pop()],
    ["retention-unbounded", (d) => (d.production_contract.retention.pending_crm_bounded = false)],
    ["retention-days", (d) => (d.production_contract.retention.rows[0].retention_days = 0)],
    ["pre-task", (d) => (d.production_contract.inbound_freshness.pre_task_revalidation = false)],
    [
      "pre-attempt-without-revoke",
      (d) => {
        d.production_contract.inbound_freshness.pre_attempt_revalidation = false;
        d.production_contract.inbound_freshness.mid_window_strategy = "SHORT_TTL_AND_RECHECK";
      },
    ],
    ["inbound-fail-open", (d) => (d.production_contract.inbound_freshness.unavailable_outcome = "ALLOW")],
    ["restriction-dispatch", (d) => (d.production_contract.inbound_freshness.active_restriction_blocks_dispatch = false)],
    ["admin-action", (d) => (d.production_contract.admin_authority.review_action = "WRITE_REGISTRY")],
    ["admin-direct-write", (d) => (d.production_contract.admin_authority.direct_registry_mutation = true)],
    ["test-missing", (d) => d.test_plans.pop()],
    ["test-executed-claim", (d) => (d.test_plans[0].state = "PASS")],
    ["artifact-missing", (d) => d.external_artifacts.pop()],
    ["artifact-pin", (d, e) => (e.artifacts.CRM_PROPOSAL_CONTRACT = "c".repeat(64))],
    ["signoff-missing", (d) => d.signoffs.pop()],
    ["signer-verifier", (d) => (d.signoffs[0].verifier_alias = d.signoffs[0].signer_alias)],
    ["signoff-bundle", (d) => (d.signoffs[0].bound_decision_bundle_sha256 = "c".repeat(64))],
    ["safety-contact", (d) => (d.safety.contains_raw_contact = true)],
    ["safety-inference", (d) => (d.safety.consent_inferred_from_rejected_or_dtmf = true)],
    ["safety-production", (d) => (d.safety.production_gate_promoted = true)],
    ["safety-real-call", (d) => (d.safety.real_customer_call_allowed = true)],
    ["extra-key", (d) => (d.production_contract.extra = true)],
  ];
  for (const [label, mutate] of mutations) {
    const candidate = clone(base);
    const candidateExpected = clone(expected);
    mutate(candidate, candidateExpected);
    try {
      validateOptOutSuppressionBundle(candidate, "completed", candidateExpected);
      fail(`mutation ${label} was not refused`);
    } catch (error) {
      if (error.message === `mutation ${label} was not refused`) throw error;
      refusals += 1;
    }
  }

  mkdirSync(ARTIFACT_ROOT, { recursive: true });
  const temporaryDirectory = mkdtempSync(resolve(ARTIFACT_ROOT, "w0187-parser-selftest-"));
  try {
    const malformedCases = [
      ["duplicate-key.json", '{"schema_version":"x","schema_version":"y"}\n'],
      ["bom.json", Buffer.concat([Buffer.from([0xef, 0xbb, 0xbf]), Buffer.from("{}\n")])],
      ["oversized.json", Buffer.alloc(MAX_INPUT_BYTES + 1, 0x20)],
    ];
    for (const [name, bytes] of malformedCases) {
      const path = resolve(temporaryDirectory, name);
      writeFileSync(path, bytes, { flag: "wx" });
      try {
        parseInput(relative(REPOSITORY_ROOT, path));
        fail(`parser mutation ${name} was not refused`);
      } catch (error) {
        if (error.message === `parser mutation ${name} was not refused`) throw error;
        refusals += 1;
      }
    }
    try {
      readConfinedUtf8File("../outside-repository.json");
      fail("parser mutation path-escape was not refused");
    } catch (error) {
      if (error.message === "parser mutation path-escape was not refused") throw error;
      refusals += 1;
    }
  } finally {
    if (temporaryDirectory.startsWith(`${ARTIFACT_ROOT}${sep}`)) {
      rmSync(temporaryDirectory, { recursive: true, force: true });
    }
  }

  return { template: 1, valid: 2, refusals };
}

function parseArguments(argv) {
  const result = { mode: null, input: null, expected: { artifacts: {} } };
  const artifactFlags = new Map(ARTIFACT_IDS.map((id) => [`--expected-${id.toLowerCase().replaceAll("_", "-")}-sha`, id]));
  for (let index = 0; index < argv.length; index += 1) {
    const arg = argv[index];
    if (arg === "--self-test") result.mode = "self-test";
    else if (arg === "--check-template") {
      result.mode = "template";
      result.input = argv[++index];
    } else if (arg === "--input") {
      result.mode = "completed";
      result.input = argv[++index];
    } else if (arg === "--expected-decision-bundle-sha") result.expected.bundleHash = argv[++index];
    else if (arg === "--expected-m8-commit-sha") result.expected.m8Commit = argv[++index];
    else if (arg === "--expected-m3-commit-sha") result.expected.m3Commit = argv[++index];
    else if (artifactFlags.has(arg)) result.expected.artifacts[artifactFlags.get(arg)] = argv[++index];
    else fail(`unknown argument: ${arg}`);
  }
  return result;
}

function main(argv) {
  const args = parseArguments(argv);
  if (args.mode === "self-test") {
    const result = runOptOutSuppressionBundleSelfTest();
    console.log(
      `W0187_OPTOUT_BUNDLE_SELFTEST_PASS template=${result.template} ` +
        `valid=${result.valid} refusals=${result.refusals}`,
    );
    return;
  }
  if (!args.input || !new Set(["template", "completed"]).has(args.mode)) {
    fail("use --self-test, --check-template <json>, or --input <json> with independent expected pins");
  }
  const { bytes, document } = parseInput(args.input);
  const result = validateOptOutSuppressionBundle(document, args.mode, args.expected);
  if (args.mode === "template") {
    console.log(`OPTOUT_SUPPRESSION_TEMPLATE_VALID_NOT_READY decisions=${result.decisions} tests=${result.tests} sha256=${sha256(bytes)}`);
  } else {
    console.log(
      `OPTOUT_SUPPRESSION_BUNDLE_VALID_ELIGIBLE_FOR_IMPLEMENTATION_REVIEW_ONLY decisions=${result.decisions} tests=${result.tests} signoffs=${result.signoffs}`,
    );
  }
}

try {
  main(process.argv.slice(2));
} catch (error) {
  console.error(`W0187_OPTOUT_BUNDLE_VALIDATION_FAILED: ${error.message}`);
  process.exitCode = 1;
}
