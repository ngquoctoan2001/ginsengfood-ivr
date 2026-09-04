#!/usr/bin/env node

// W-0178 — Offline, metadata-only validator for M3 D-06 callback revalidation evidence.
// It does not contact M3/Ops, read credentials or raw payloads, mutate runtime state,
// select revoke strategy, enable delivery, or authorize production/customer calls.

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

const SCRIPT_PATH = fileURLToPath(import.meta.url);
const REPOSITORY_ROOT = resolve(dirname(SCRIPT_PATH), "../../..");
const MAX_INPUT_BYTES = 512 * 1024;
const SCHEMA_VERSION = "m3-d06-revalidation-evidence.v1";
const WORK_ID = "W-0178";
const PLACEHOLDER = "PENDING_EXTERNAL_EVIDENCE";

const SOURCE_PINS = Object.freeze({
  w0149_evidence_path: "docs/evidence/W-0149/README.md",
  w0149_evidence_sha256: "e93225b6c3401090eb4980fa69433439ce1b3779dec4049d0cef563b6b0437e0",
  m3_requirements_path: "integration-requirements/01-sales-platform-requirements.md",
  m3_requirements_sha256: "68fc49cdd979fee66153a6fa5748623a69dfff68bb06054304009d46011ba894",
  m3_handover_path: "integration-requirements/06-module-3-api-handover.md",
  m3_handover_sha256: "b676a32d4ba51b9f345eb3d32e21d793216f4011e98bbfc9dc8d2867997ba08a",
  m8_target_oas_path: "specs/api/openapi/order-core-ivr-callback.target-v1.yaml",
  m8_target_oas_sha256: "af0cb5cc3f47aaa4c8e232418c216b228fd996e316fe129a7cbf1d4636659697",
  shared_e2e_validator_path: "deploy/ci/scripts/target-v1-shared-e2e-report-validator.mjs",
  shared_e2e_validator_sha256: "a0abd96deb8130f274988c6964d8966c50cb19c0f4b87ef565c221690cdafc89",
  requirement_scope: "C10-C11-C13-D06.2026-09-04",
});

const SOURCE_FILES = Object.freeze([
  ["w0149_evidence_path", "w0149_evidence_sha256"],
  ["m3_requirements_path", "m3_requirements_sha256"],
  ["m3_handover_path", "m3_handover_sha256"],
  ["m8_target_oas_path", "m8_target_oas_sha256"],
  ["shared_e2e_validator_path", "shared_e2e_validator_sha256"],
]);

const CASE_RULES = Object.freeze([
  {
    case_id: "D06-01-CURRENT-ORDER-ACCEPTED",
    scenario: "CURRENT_VERSION_STATE_AND_NO_BLOCKER",
    decision: "ACCEPTED_AFTER_REVALIDATION",
    blocker: "NONE",
    http: [200], state: "DIFFERENT", revision: "SAME",
    assertions: [
      "m3_revalidation_completed_before_ack",
      "version_and_state_current",
      "program_and_payment_callable",
      "inventory_recall_sale_lock_quality_hold_clear",
      "evidence_freshness_within_contract",
      "exactly_one_order_transition",
    ],
  },
  {
    case_id: "D06-02-STALE-ORDER-VERSION",
    scenario: "ORDER_VERSION_CHANGED_AFTER_IVR_INTAKE",
    decision: "REJECTED_STALE",
    blocker: "STALE_ORDER_VERSION",
    http: [409], state: "SAME", revision: "DIFFERENT",
    assertions: [
      "m3_revalidation_completed_before_ack",
      "version_mismatch_detected",
      "no_order_transition",
      "terminal_stale_ack_recorded",
    ],
  },
  {
    case_id: "D06-03-ORDER-STATE-CHANGED",
    scenario: "ORDER_CANCELLED_OR_NO_LONGER_CALLABLE",
    decision: "BLOCKED_BY_CORE",
    blocker: "ORDER_STATE_CHANGED",
    http: [200, 409], state: "SAME", revision: "DIFFERENT",
    assertions: [
      "m3_revalidation_completed_before_ack",
      "current_order_state_checked",
      "no_order_transition",
      "blocked_ack_recorded",
    ],
  },
  {
    case_id: "D06-04-RECALL-ACTIVE",
    scenario: "RECALL_BECAME_ACTIVE_AFTER_IVR_INTAKE",
    decision: "BLOCKED_BY_CORE",
    blocker: "RECALL_ACTIVE",
    http: [200, 409], state: "SAME", revision: "SAME",
    assertions: [
      "m3_revalidation_completed_before_ack",
      "recall_read_from_authoritative_ops_source",
      "no_order_transition",
      "blocked_ack_recorded",
    ],
  },
  {
    case_id: "D06-05-SALE-LOCK-ACTIVE",
    scenario: "SALE_LOCK_BECAME_ACTIVE_AFTER_IVR_INTAKE",
    decision: "BLOCKED_BY_CORE",
    blocker: "SALE_LOCK_ACTIVE",
    http: [200, 409], state: "SAME", revision: "SAME",
    assertions: [
      "m3_revalidation_completed_before_ack",
      "sale_lock_read_from_authoritative_ops_source",
      "no_order_transition",
      "blocked_ack_recorded",
    ],
  },
  {
    case_id: "D06-06-QUALITY-HOLD-ACTIVE",
    scenario: "QUALITY_HOLD_BECAME_ACTIVE_AFTER_IVR_INTAKE",
    decision: "BLOCKED_BY_CORE",
    blocker: "QUALITY_HOLD_ACTIVE",
    http: [200, 409], state: "SAME", revision: "SAME",
    assertions: [
      "m3_revalidation_completed_before_ack",
      "quality_hold_read_from_authoritative_ops_source",
      "no_order_transition",
      "blocked_ack_recorded",
    ],
  },
  {
    case_id: "D06-07-PROGRAM-PAYMENT-INELIGIBLE",
    scenario: "PROGRAM_OR_PAYMENT_NO_LONGER_CALLABLE",
    decision: "BLOCKED_BY_CORE",
    blocker: "PROGRAM_PAYMENT_INELIGIBLE",
    http: [200, 409], state: "SAME", revision: "SAME",
    assertions: [
      "m3_revalidation_completed_before_ack",
      "program_and_payment_revalidated",
      "no_order_transition",
      "blocked_ack_recorded",
    ],
  },
  {
    case_id: "D06-08-EVIDENCE-EXPIRED",
    scenario: "BUSINESS_EVIDENCE_EXPIRED_BEFORE_CALLBACK",
    decision: "BLOCKED_BY_CORE",
    blocker: "EVIDENCE_EXPIRED",
    http: [200, 409], state: "SAME", revision: "SAME",
    assertions: [
      "m3_revalidation_completed_before_ack",
      "evidence_age_or_valid_until_checked",
      "expired_evidence_failed_closed",
      "no_order_transition",
    ],
  },
  {
    case_id: "D06-09-SOURCE-UNAVAILABLE",
    scenario: "REQUIRED_REVALIDATION_SOURCE_UNAVAILABLE",
    decision: "REVALIDATION_UNAVAILABLE_FAIL_CLOSED",
    blocker: "SOURCE_UNAVAILABLE",
    http: [null, 502, 503, 504], state: "SAME", revision: "SAME",
    assertions: [
      "all_required_sources_were_attempted",
      "no_accepted_ack_before_revalidation",
      "no_order_transition",
      "retry_or_review_disposition_recorded",
    ],
  },
  {
    case_id: "D06-10-EXACT-REPLAY",
    scenario: "SAME_IDEMPOTENCY_KEY_AND_IMMUTABLE_BODY",
    decision: "DUPLICATE_ACCEPTED",
    blocker: "NONE",
    http: [409], state: "SAME", revision: "SAME",
    assertions: [
      "same_idempotency_key",
      "same_immutable_body_hash",
      "prior_decision_returned",
      "no_duplicate_order_transition",
    ],
  },
  {
    case_id: "D06-11-CHANGED-BODY-REPLAY",
    scenario: "SAME_IDEMPOTENCY_KEY_AND_CHANGED_BODY",
    decision: "IDEMPOTENCY_CONFLICT",
    blocker: "MUTATED_CALLBACK_BODY",
    http: [409], state: "SAME", revision: "SAME",
    assertions: [
      "same_idempotency_key",
      "different_immutable_body_hash",
      "conflict_recorded",
      "no_order_transition",
    ],
  },
  {
    case_id: "D06-12-OUTAGE-RECOVERY",
    scenario: "REVALIDATION_RECOVERS_AFTER_BOUNDED_RETRY",
    decision: "ACCEPTED_AFTER_REVALIDATION",
    blocker: "NONE",
    http: [200], state: "DIFFERENT", revision: "SAME",
    assertions: [
      "initial_unavailable_attempt_failed_closed",
      "same_idempotency_key_and_body_retried",
      "all_sources_revalidated_after_recovery",
      "exactly_one_order_transition",
    ],
  },
]);

const REQUIRED_SIGNOFF_ROLES = Object.freeze([
  "PROJECT_OWNER", "M3_OWNER", "PRODUCT_OWNER", "OPS_SOURCE_OWNER",
  "SECURITY", "PLATFORM", "RELEASE_OWNER",
]);

const ROOT_KEYS = [
  "schema_version", "work_id", "status", "source", "candidate", "external_evidence",
  "revalidation_matrix", "matrix_summary", "signoffs", "safety",
];
const CANDIDATE_KEYS = [
  "m8_repo_ref", "m8_commit_sha", "m3_repo_ref", "m3_commit_sha",
  "environment_id", "config_version", "run_started_at", "run_completed_at",
];
const EXTERNAL_EVIDENCE_KEYS = [
  "m3_authoritative_oas", "m3_revalidation_implementation", "m3_consumer_cdc",
  "ops_truth_contract", "security_auth_custody", "platform_sandbox_network_tls",
];
const ARTIFACT_KEYS = ["artifact_ref", "sha256", "producer_alias", "produced_at"];
const CASE_KEYS = [
  "case_id", "scenario", "status", "m8_commit_sha", "m3_commit_sha", "environment_id",
  "config_version", "started_at", "completed_at", "callback_id_alias",
  "idempotency_key_fingerprint_sha256", "immutable_body_sha256", "revalidation_evidence_ref",
  "revalidation_evidence_sha256", "source_revision_before", "source_revision_after",
  "state_before_sha256", "state_after_sha256", "observed_http_status", "observed_decision",
  "observed_blocker", "assertions_passed",
];
const SUMMARY_KEYS = [
  "required_cases", "passed_cases", "failed_cases", "pending_cases", "complete_matrix",
  "all_required_blockers_covered", "same_candidate_environment_config", "selected_green_cases_only",
];
const SIGNOFF_KEYS = [
  "role", "signer_alias", "verifier_alias", "authority_ref", "authority_sha256", "decision",
  "reviewed_at", "m8_commit_sha", "m3_commit_sha", "environment_id", "config_version",
];
const SAFETY_KEYS = [
  "raw_request_or_response_embedded", "contains_credentials_or_secrets", "contains_personal_data",
  "local_mock_claimed_as_m3_evidence", "revoke_strategy_selected_by_report",
  "revoke_endpoint_or_runtime_added", "delivery_guard_removed", "production_enabled",
  "real_customer_call_allowed", "report_authorizes_production",
];

const SOURCE_KEYS = Object.keys(SOURCE_PINS);
const EXTERNAL_PIN_KEYS = Object.freeze({
  m3_authoritative_oas: "m3_oas_sha256",
  m3_revalidation_implementation: "m3_implementation_sha256",
  m3_consumer_cdc: "m3_cdc_sha256",
  ops_truth_contract: "ops_truth_sha256",
  security_auth_custody: "security_auth_sha256",
  platform_sandbox_network_tls: "platform_evidence_sha256",
});

function fail(message) {
  throw new Error(message);
}

function sha256(value) {
  return createHash("sha256").update(value).digest("hex");
}

function hashLabel(label) {
  return sha256(Buffer.from(label, "utf8"));
}

function assertRecord(value, label) {
  if (value === null || typeof value !== "object" || Array.isArray(value)) {
    fail(`${label} must be an object`);
  }
}

function assertExactKeys(value, keys, label) {
  assertRecord(value, label);
  const actual = Object.keys(value).sort();
  const expected = [...keys].sort();
  if (JSON.stringify(actual) !== JSON.stringify(expected)) {
    fail(`${label} keys mismatch: expected ${expected.join(",")}; got ${actual.join(",")}`);
  }
}

function assertString(value, label) {
  if (typeof value !== "string" || value.trim() !== value || value.length === 0) {
    fail(`${label} must be a non-empty trimmed string`);
  }
  return value;
}

function assertEnum(value, allowed, label) {
  if (!allowed.includes(value)) {
    fail(`${label} must be one of ${allowed.join(",")}`);
  }
}

function assertSha(value, label, { placeholderAllowed = false } = {}) {
  if (placeholderAllowed && value === PLACEHOLDER) return;
  if (typeof value !== "string" || !/^[a-f0-9]{64}$/.test(value)) {
    fail(`${label} must be a lowercase SHA-256`);
  }
}

function assertGitSha(value, label, { placeholderAllowed = false } = {}) {
  if (placeholderAllowed && value === PLACEHOLDER) return;
  if (typeof value !== "string" || !/^[a-f0-9]{40}$/.test(value)) {
    fail(`${label} must be a full lowercase Git SHA`);
  }
}

function assertIdentifier(value, label, { placeholderAllowed = false } = {}) {
  if (placeholderAllowed && value === PLACEHOLDER) return;
  assertString(value, label);
  if (!/^[A-Z0-9][A-Z0-9._:/-]{2,127}$/.test(value)) {
    fail(`${label} must be an uppercase metadata alias/ref without free text`);
  }
}

function parseTimestamp(value, label, { placeholderAllowed = false } = {}) {
  if (placeholderAllowed && value === PLACEHOLDER) return null;
  assertString(value, label);
  if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{3})?Z$/.test(value)) {
    fail(`${label} must be an ISO-8601 UTC timestamp`);
  }
  const parsed = Date.parse(value);
  if (!Number.isFinite(parsed)) fail(`${label} is not a valid timestamp`);
  return parsed;
}

function assertNoSensitiveScalars(value, label = "report") {
  if (Array.isArray(value)) {
    value.forEach((entry, index) => assertNoSensitiveScalars(entry, `${label}[${index}]`));
    return;
  }
  if (value !== null && typeof value === "object") {
    Object.entries(value).forEach(([key, entry]) => assertNoSensitiveScalars(entry, `${label}.${key}`));
    return;
  }
  if (typeof value !== "string") return;
  if (/^[a-f0-9]{40,64}$/.test(value)) return;
  if (/\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b/i.test(value)) fail(`${label} contains an email-like value`);
  if (/^\+?\d[\d .()-]{7,}\d$/.test(value)) fail(`${label} contains a phone-like value`);
  if (/\b(?:bearer\s+[a-z0-9._~-]+|password\s*[:=]|api[_-]?key\s*[:=]|authorization\s*[:=])/i.test(value)) {
    fail(`${label} contains a credential-like value`);
  }
  if (/^https?:\/\/[^\s]+[?&][^\s]+$/i.test(value)) fail(`${label} contains a URL query string`);
}

class DuplicateSafeJsonParser {
  constructor(text) {
    this.text = text;
    this.offset = 0;
  }

  parse() {
    const value = this.parseValue("$");
    this.skipWhitespace();
    if (this.offset !== this.text.length) fail(`unexpected JSON token at offset ${this.offset}`);
    return value;
  }

  skipWhitespace() {
    while (/\s/.test(this.text[this.offset] ?? "")) this.offset += 1;
  }

  parseValue(path) {
    this.skipWhitespace();
    const ch = this.text[this.offset];
    if (ch === "{") return this.parseObject(path);
    if (ch === "[") return this.parseArray(path);
    if (ch === '"') return this.parseString();
    const rest = this.text.slice(this.offset);
    const primitive = rest.match(/^(?:true|false|null|-?(?:0|[1-9]\d*)(?:\.\d+)?(?:[eE][+-]?\d+)?)/)?.[0];
    if (!primitive) fail(`invalid JSON value at ${path}`);
    this.offset += primitive.length;
    return JSON.parse(primitive);
  }

  parseString() {
    const start = this.offset;
    this.offset += 1;
    let escaped = false;
    while (this.offset < this.text.length) {
      const ch = this.text[this.offset];
      this.offset += 1;
      if (!escaped && ch === '"') return JSON.parse(this.text.slice(start, this.offset));
      if (!escaped && ch === "\\") escaped = true;
      else escaped = false;
    }
    fail(`unterminated JSON string at offset ${start}`);
  }

  parseObject(path) {
    const result = {};
    const keys = new Set();
    this.offset += 1;
    this.skipWhitespace();
    if (this.text[this.offset] === "}") {
      this.offset += 1;
      return result;
    }
    while (true) {
      this.skipWhitespace();
      if (this.text[this.offset] !== '"') fail(`object key expected at ${path}`);
      const key = this.parseString();
      if (keys.has(key)) fail(`duplicate JSON key ${path}.${key}`);
      keys.add(key);
      this.skipWhitespace();
      if (this.text[this.offset] !== ":") fail(`colon expected after ${path}.${key}`);
      this.offset += 1;
      result[key] = this.parseValue(`${path}.${key}`);
      this.skipWhitespace();
      const separator = this.text[this.offset];
      this.offset += 1;
      if (separator === "}") return result;
      if (separator !== ",") fail(`comma or closing brace expected at ${path}`);
    }
  }

  parseArray(path) {
    const result = [];
    this.offset += 1;
    this.skipWhitespace();
    if (this.text[this.offset] === "]") {
      this.offset += 1;
      return result;
    }
    while (true) {
      result.push(this.parseValue(`${path}[${result.length}]`));
      this.skipWhitespace();
      const separator = this.text[this.offset];
      this.offset += 1;
      if (separator === "]") return result;
      if (separator !== ",") fail(`comma or closing bracket expected at ${path}`);
    }
  }
}

function isConfined(path) {
  const rel = relative(REPOSITORY_ROOT, path);
  return rel !== "" && !rel.startsWith("..") && !isAbsolute(rel);
}

function readStrictJson(inputPath) {
  const requested = resolve(REPOSITORY_ROOT, inputPath);
  if (!isConfined(requested)) fail("input path must remain inside the repository");
  const info = lstatSync(requested);
  if (!info.isFile() || info.isSymbolicLink()) fail("input must be a regular non-symlink file");
  if (info.size > MAX_INPUT_BYTES) fail(`input exceeds ${MAX_INPUT_BYTES} bytes`);
  const actual = realpathSync(requested);
  if (!isConfined(actual)) fail("resolved input path escapes the repository");
  const bytes = readFileSync(actual);
  let text;
  try {
    text = new TextDecoder("utf-8", { fatal: true }).decode(bytes);
  } catch {
    fail("input must be valid UTF-8");
  }
  if (text.charCodeAt(0) === 0xfeff) fail("UTF-8 BOM is not allowed");
  return new DuplicateSafeJsonParser(text).parse();
}

function validateLocalSources(source) {
  assertExactKeys(source, SOURCE_KEYS, "source");
  for (const key of SOURCE_KEYS) {
    if (source[key] !== SOURCE_PINS[key]) fail(`source.${key} does not match the W-0178 contract`);
  }
  for (const [pathKey, hashKey] of SOURCE_FILES) {
    const absolute = resolve(REPOSITORY_ROOT, source[pathKey]);
    if (!isConfined(absolute)) fail(`source.${pathKey} escapes the repository`);
    const info = lstatSync(absolute);
    if (!info.isFile() || info.isSymbolicLink()) fail(`source.${pathKey} is not a regular file`);
    const observed = sha256(readFileSync(absolute));
    if (observed !== source[hashKey]) fail(`local source hash drift: ${source[pathKey]}`);
  }
}

function validateCandidate(candidate, expected, pending) {
  assertExactKeys(candidate, CANDIDATE_KEYS, "candidate");
  for (const key of ["m8_repo_ref", "m3_repo_ref", "environment_id", "config_version"]) {
    assertIdentifier(candidate[key], `candidate.${key}`, { placeholderAllowed: pending });
  }
  assertGitSha(candidate.m8_commit_sha, "candidate.m8_commit_sha", { placeholderAllowed: pending });
  assertGitSha(candidate.m3_commit_sha, "candidate.m3_commit_sha", { placeholderAllowed: pending });
  const started = parseTimestamp(candidate.run_started_at, "candidate.run_started_at", { placeholderAllowed: pending });
  const completed = parseTimestamp(candidate.run_completed_at, "candidate.run_completed_at", { placeholderAllowed: pending });
  if (pending) {
    for (const key of CANDIDATE_KEYS) if (candidate[key] !== PLACEHOLDER) fail(`pending candidate.${key} must be placeholder`);
    return { started: null, completed: null };
  }
  if (candidate.m8_commit_sha !== expected.m8_commit_sha) fail("candidate M8 SHA does not match independent CLI pin");
  if (candidate.m3_commit_sha !== expected.m3_commit_sha) fail("candidate M3 SHA does not match independent CLI pin");
  if (completed < started) fail("candidate run_completed_at precedes run_started_at");
  return { started, completed };
}

function validateExternalEvidence(evidence, expected, pending, runStarted) {
  assertExactKeys(evidence, EXTERNAL_EVIDENCE_KEYS, "external_evidence");
  for (const name of EXTERNAL_EVIDENCE_KEYS) {
    const artifact = evidence[name];
    assertExactKeys(artifact, ARTIFACT_KEYS, `external_evidence.${name}`);
    assertIdentifier(artifact.artifact_ref, `${name}.artifact_ref`, { placeholderAllowed: pending });
    assertSha(artifact.sha256, `${name}.sha256`, { placeholderAllowed: pending });
    assertIdentifier(artifact.producer_alias, `${name}.producer_alias`, { placeholderAllowed: pending });
    const producedAt = parseTimestamp(artifact.produced_at, `${name}.produced_at`, { placeholderAllowed: pending });
    if (pending) {
      for (const key of ARTIFACT_KEYS) if (artifact[key] !== PLACEHOLDER) fail(`pending ${name}.${key} must be placeholder`);
      continue;
    }
    const expectedHash = expected[EXTERNAL_PIN_KEYS[name]];
    if (artifact.sha256 !== expectedHash) fail(`${name} hash does not match independent CLI pin`);
    if (producedAt > runStarted) fail(`${name} was produced after the evidence run started`);
  }
}

function assertRelation(before, after, relation, label) {
  if (relation === "SAME" && before !== after) fail(`${label} must remain unchanged`);
  if (relation === "DIFFERENT" && before === after) fail(`${label} must change`);
}

function validateMatrix(matrix, candidate, pending, runWindow) {
  if (!Array.isArray(matrix) || matrix.length !== CASE_RULES.length) {
    fail(`revalidation_matrix must contain exactly ${CASE_RULES.length} cases`);
  }
  const callbackAliases = new Set();
  for (let index = 0; index < CASE_RULES.length; index += 1) {
    const rule = CASE_RULES[index];
    const item = matrix[index];
    const label = `revalidation_matrix[${index}]`;
    assertExactKeys(item, CASE_KEYS, label);
    if (item.case_id !== rule.case_id || item.scenario !== rule.scenario) {
      fail(`${label} must be ${rule.case_id}/${rule.scenario} in canonical order`);
    }
    if (pending) {
      if (item.status !== "PENDING") fail(`${label}.status must be PENDING`);
      for (const key of CASE_KEYS) {
        if (["case_id", "scenario", "status", "observed_http_status", "assertions_passed"].includes(key)) continue;
        if (item[key] !== PLACEHOLDER) fail(`${label}.${key} must be placeholder`);
      }
      if (item.observed_http_status !== null) fail(`${label}.observed_http_status must be null`);
      if (!Array.isArray(item.assertions_passed) || item.assertions_passed.length !== 0) {
        fail(`${label}.assertions_passed must be empty`);
      }
      continue;
    }

    if (item.status !== "PASS") fail(`${label}.status must be PASS`);
    if (item.m8_commit_sha !== candidate.m8_commit_sha || item.m3_commit_sha !== candidate.m3_commit_sha) {
      fail(`${label} is not bound to both candidate SHAs`);
    }
    if (item.environment_id !== candidate.environment_id || item.config_version !== candidate.config_version) {
      fail(`${label} is not bound to candidate environment/config`);
    }
    const started = parseTimestamp(item.started_at, `${label}.started_at`);
    const completed = parseTimestamp(item.completed_at, `${label}.completed_at`);
    if (started < runWindow.started || completed > runWindow.completed || completed < started) {
      fail(`${label} timestamps fall outside the candidate run window`);
    }
    assertIdentifier(item.callback_id_alias, `${label}.callback_id_alias`);
    if (callbackAliases.has(item.callback_id_alias)) fail(`${label}.callback_id_alias must be unique`);
    callbackAliases.add(item.callback_id_alias);
    for (const key of [
      "idempotency_key_fingerprint_sha256", "immutable_body_sha256", "revalidation_evidence_sha256",
      "source_revision_before", "source_revision_after", "state_before_sha256", "state_after_sha256",
    ]) assertSha(item[key], `${label}.${key}`);
    assertIdentifier(item.revalidation_evidence_ref, `${label}.revalidation_evidence_ref`);
    const allowedNoResponse = item.observed_http_status === null && rule.http.includes(null);
    if (!allowedNoResponse && (!Number.isInteger(item.observed_http_status) || !rule.http.includes(item.observed_http_status))) {
      fail(`${label}.observed_http_status is not allowed for ${rule.case_id}`);
    }
    if (item.observed_decision !== rule.decision) fail(`${label}.observed_decision mismatch`);
    if (item.observed_blocker !== rule.blocker) fail(`${label}.observed_blocker mismatch`);
    assertRelation(item.source_revision_before, item.source_revision_after, rule.revision, `${label} source revision`);
    assertRelation(item.state_before_sha256, item.state_after_sha256, rule.state, `${label} order state`);
    if (JSON.stringify(item.assertions_passed) !== JSON.stringify(rule.assertions)) {
      fail(`${label}.assertions_passed must match the canonical ordered assertion set`);
    }
  }

  if (!pending) {
    const exactReplay = matrix[9];
    const changedReplay = matrix[10];
    if (exactReplay.idempotency_key_fingerprint_sha256 !== changedReplay.idempotency_key_fingerprint_sha256) {
      fail("D06-10 and D06-11 must exercise the same idempotency-key fingerprint");
    }
    if (exactReplay.immutable_body_sha256 === changedReplay.immutable_body_sha256) {
      fail("D06-11 must change the immutable callback body relative to D06-10");
    }
  }
}

function validateSummary(summary, pending) {
  assertExactKeys(summary, SUMMARY_KEYS, "matrix_summary");
  const expected = pending
    ? {
        required_cases: 12, passed_cases: 0, failed_cases: 0, pending_cases: 12,
        complete_matrix: false, all_required_blockers_covered: false,
        same_candidate_environment_config: false, selected_green_cases_only: false,
      }
    : {
        required_cases: 12, passed_cases: 12, failed_cases: 0, pending_cases: 0,
        complete_matrix: true, all_required_blockers_covered: true,
        same_candidate_environment_config: true, selected_green_cases_only: false,
      };
  for (const [key, value] of Object.entries(expected)) {
    if (summary[key] !== value) fail(`matrix_summary.${key} must be ${value}`);
  }
}

function validateSignoffs(signoffs, candidate, pending, runCompleted) {
  if (!Array.isArray(signoffs) || signoffs.length !== REQUIRED_SIGNOFF_ROLES.length) {
    fail(`signoffs must contain exactly ${REQUIRED_SIGNOFF_ROLES.length} entries`);
  }
  const signers = new Set();
  for (let index = 0; index < REQUIRED_SIGNOFF_ROLES.length; index += 1) {
    const item = signoffs[index];
    const role = REQUIRED_SIGNOFF_ROLES[index];
    const label = `signoffs[${index}]`;
    assertExactKeys(item, SIGNOFF_KEYS, label);
    if (item.role !== role) fail(`${label}.role must be ${role} in canonical order`);
    if (pending) {
      if (item.decision !== "PENDING") fail(`${label}.decision must be PENDING`);
      for (const key of SIGNOFF_KEYS) {
        if (["role", "decision"].includes(key)) continue;
        if (item[key] !== PLACEHOLDER) fail(`${label}.${key} must be placeholder`);
      }
      continue;
    }
    assertIdentifier(item.signer_alias, `${label}.signer_alias`);
    assertIdentifier(item.verifier_alias, `${label}.verifier_alias`);
    assertIdentifier(item.authority_ref, `${label}.authority_ref`);
    assertSha(item.authority_sha256, `${label}.authority_sha256`);
    if (item.decision !== "APPROVED") fail(`${label}.decision must be APPROVED`);
    const reviewedAt = parseTimestamp(item.reviewed_at, `${label}.reviewed_at`);
    if (reviewedAt < runCompleted) fail(`${label}.reviewed_at precedes run completion`);
    if (item.m8_commit_sha !== candidate.m8_commit_sha || item.m3_commit_sha !== candidate.m3_commit_sha) {
      fail(`${label} is not bound to both candidate SHAs`);
    }
    if (item.environment_id !== candidate.environment_id || item.config_version !== candidate.config_version) {
      fail(`${label} is not bound to candidate environment/config`);
    }
    if (item.signer_alias === item.verifier_alias) fail(`${label} signer and verifier must differ`);
    if (signers.has(item.signer_alias)) fail(`${label}.signer_alias must be unique`);
    signers.add(item.signer_alias);
  }
  if (!pending) {
    for (const [index, item] of signoffs.entries()) {
      if (signers.has(item.verifier_alias)) {
        fail(`signoffs[${index}].verifier_alias must not be any required signer`);
      }
    }
  }
}

function validateSafety(safety) {
  assertExactKeys(safety, SAFETY_KEYS, "safety");
  for (const key of SAFETY_KEYS) {
    if (safety[key] !== false) fail(`safety.${key} must be false`);
  }
}

function validateReport(report, expected = null, mode = "completed") {
  const pending = mode === "template";
  assertExactKeys(report, ROOT_KEYS, "report");
  if (report.schema_version !== SCHEMA_VERSION) fail(`schema_version must be ${SCHEMA_VERSION}`);
  if (report.work_id !== WORK_ID) fail(`work_id must be ${WORK_ID}`);
  const requiredStatus = pending ? "M3_D06_EVIDENCE_NOT_RECEIVED" : "M3_D06_EVIDENCE_COMPLETE";
  if (report.status !== requiredStatus) fail(`status must be ${requiredStatus}`);
  validateLocalSources(report.source);
  if (!pending) validateExpectedPins(expected);
  const runWindow = validateCandidate(report.candidate, expected ?? {}, pending);
  validateExternalEvidence(report.external_evidence, expected ?? {}, pending, runWindow.started);
  validateMatrix(report.revalidation_matrix, report.candidate, pending, runWindow);
  validateSummary(report.matrix_summary, pending);
  validateSignoffs(report.signoffs, report.candidate, pending, runWindow.completed);
  validateSafety(report.safety);
  assertNoSensitiveScalars(report);
  return report;
}

function validateExpectedPins(expected) {
  const required = [
    "m8_commit_sha", "m3_commit_sha", "m3_oas_sha256", "m3_implementation_sha256",
    "m3_cdc_sha256", "ops_truth_sha256", "security_auth_sha256", "platform_evidence_sha256",
  ];
  assertExactKeys(expected, required, "independent CLI pins");
  assertGitSha(expected.m8_commit_sha, "--m8-commit-sha");
  assertGitSha(expected.m3_commit_sha, "--m3-commit-sha");
  for (const key of required.slice(2)) assertSha(expected[key], `--${key.replaceAll("_", "-")}`);
}

function makePendingTemplate() {
  const pendingArtifact = () => ({
    artifact_ref: PLACEHOLDER,
    sha256: PLACEHOLDER,
    producer_alias: PLACEHOLDER,
    produced_at: PLACEHOLDER,
  });
  return {
    schema_version: SCHEMA_VERSION,
    work_id: WORK_ID,
    status: "M3_D06_EVIDENCE_NOT_RECEIVED",
    source: { ...SOURCE_PINS },
    candidate: Object.fromEntries(CANDIDATE_KEYS.map((key) => [key, PLACEHOLDER])),
    external_evidence: Object.fromEntries(EXTERNAL_EVIDENCE_KEYS.map((key) => [key, pendingArtifact()])),
    revalidation_matrix: CASE_RULES.map((rule) => ({
      case_id: rule.case_id,
      scenario: rule.scenario,
      status: "PENDING",
      m8_commit_sha: PLACEHOLDER,
      m3_commit_sha: PLACEHOLDER,
      environment_id: PLACEHOLDER,
      config_version: PLACEHOLDER,
      started_at: PLACEHOLDER,
      completed_at: PLACEHOLDER,
      callback_id_alias: PLACEHOLDER,
      idempotency_key_fingerprint_sha256: PLACEHOLDER,
      immutable_body_sha256: PLACEHOLDER,
      revalidation_evidence_ref: PLACEHOLDER,
      revalidation_evidence_sha256: PLACEHOLDER,
      source_revision_before: PLACEHOLDER,
      source_revision_after: PLACEHOLDER,
      state_before_sha256: PLACEHOLDER,
      state_after_sha256: PLACEHOLDER,
      observed_http_status: null,
      observed_decision: PLACEHOLDER,
      observed_blocker: PLACEHOLDER,
      assertions_passed: [],
    })),
    matrix_summary: {
      required_cases: 12,
      passed_cases: 0,
      failed_cases: 0,
      pending_cases: 12,
      complete_matrix: false,
      all_required_blockers_covered: false,
      same_candidate_environment_config: false,
      selected_green_cases_only: false,
    },
    signoffs: REQUIRED_SIGNOFF_ROLES.map((role) => ({
      role,
      signer_alias: PLACEHOLDER,
      verifier_alias: PLACEHOLDER,
      authority_ref: PLACEHOLDER,
      authority_sha256: PLACEHOLDER,
      decision: "PENDING",
      reviewed_at: PLACEHOLDER,
      m8_commit_sha: PLACEHOLDER,
      m3_commit_sha: PLACEHOLDER,
      environment_id: PLACEHOLDER,
      config_version: PLACEHOLDER,
    })),
    safety: Object.fromEntries(SAFETY_KEYS.map((key) => [key, false])),
  };
}

function makeFixture() {
  const report = makePendingTemplate();
  const expected = {
    m8_commit_sha: "1".repeat(40),
    m3_commit_sha: "2".repeat(40),
    m3_oas_sha256: hashLabel("M3-OAS"),
    m3_implementation_sha256: hashLabel("M3-IMPLEMENTATION"),
    m3_cdc_sha256: hashLabel("M3-CDC"),
    ops_truth_sha256: hashLabel("OPS-TRUTH"),
    security_auth_sha256: hashLabel("SECURITY-AUTH"),
    platform_evidence_sha256: hashLabel("PLATFORM-EVIDENCE"),
  };
  report.status = "M3_D06_EVIDENCE_COMPLETE";
  report.candidate = {
    m8_repo_ref: "M8-REPO",
    m8_commit_sha: expected.m8_commit_sha,
    m3_repo_ref: "M3-REPO",
    m3_commit_sha: expected.m3_commit_sha,
    environment_id: "SHARED-E2E-01",
    config_version: "CONFIG-2026-09-04-01",
    run_started_at: "2026-09-04T01:00:00Z",
    run_completed_at: "2026-09-04T01:30:00Z",
  };
  EXTERNAL_EVIDENCE_KEYS.forEach((name, index) => {
    report.external_evidence[name] = {
      artifact_ref: `ARTIFACT-${index + 1}`,
      sha256: expected[EXTERNAL_PIN_KEYS[name]],
      producer_alias: `PRODUCER-${index + 1}`,
      produced_at: "2026-09-04T00:30:00Z",
    };
  });
  report.revalidation_matrix = CASE_RULES.map((rule, index) => {
    const sameRevision = hashLabel(`REVISION-${index}`);
    const sameState = hashLabel(`STATE-${index}`);
    const replayKey = hashLabel("REPLAY-KEY");
    return {
      case_id: rule.case_id,
      scenario: rule.scenario,
      status: "PASS",
      m8_commit_sha: expected.m8_commit_sha,
      m3_commit_sha: expected.m3_commit_sha,
      environment_id: report.candidate.environment_id,
      config_version: report.candidate.config_version,
      started_at: `2026-09-04T01:${String(index * 2).padStart(2, "0")}:00Z`,
      completed_at: `2026-09-04T01:${String(index * 2 + 1).padStart(2, "0")}:00Z`,
      callback_id_alias: `CALLBACK-${String(index + 1).padStart(2, "0")}`,
      idempotency_key_fingerprint_sha256: [9, 10].includes(index) ? replayKey : hashLabel(`KEY-${index}`),
      immutable_body_sha256: hashLabel(`BODY-${index}`),
      revalidation_evidence_ref: `D06-EVIDENCE-${String(index + 1).padStart(2, "0")}`,
      revalidation_evidence_sha256: hashLabel(`EVIDENCE-${index}`),
      source_revision_before: sameRevision,
      source_revision_after: rule.revision === "SAME" ? sameRevision : hashLabel(`REVISION-AFTER-${index}`),
      state_before_sha256: sameState,
      state_after_sha256: rule.state === "SAME" ? sameState : hashLabel(`STATE-AFTER-${index}`),
      observed_http_status: rule.http[0],
      observed_decision: rule.decision,
      observed_blocker: rule.blocker,
      assertions_passed: [...rule.assertions],
    };
  });
  report.matrix_summary = {
    required_cases: 12,
    passed_cases: 12,
    failed_cases: 0,
    pending_cases: 0,
    complete_matrix: true,
    all_required_blockers_covered: true,
    same_candidate_environment_config: true,
    selected_green_cases_only: false,
  };
  report.signoffs = REQUIRED_SIGNOFF_ROLES.map((role, index) => ({
    role,
    signer_alias: `SIGNER-${index + 1}`,
    verifier_alias: `INDEPENDENT-VERIFIER-${index + 1}`,
    authority_ref: `AUTHORITY-${index + 1}`,
    authority_sha256: hashLabel(`AUTHORITY-${index + 1}`),
    decision: "APPROVED",
    reviewed_at: "2026-09-04T02:00:00Z",
    m8_commit_sha: expected.m8_commit_sha,
    m3_commit_sha: expected.m3_commit_sha,
    environment_id: report.candidate.environment_id,
    config_version: report.candidate.config_version,
  }));
  return { report, expected };
}

function clone(value) {
  return JSON.parse(JSON.stringify(value));
}

function expectRefusal(label, action) {
  try {
    action();
  } catch {
    return;
  }
  fail(`self-test mutation was accepted: ${label}`);
}

function runSelfTest() {
  const { report, expected } = makeFixture();
  validateReport(report, expected, "completed");
  validateReport(makePendingTemplate(), null, "template");

  const mutations = [
    ["wrong status", (x) => { x.status = "PRODUCTION_READY"; }],
    ["source pin drift", (x) => { x.source.requirement_scope = "OTHER"; }],
    ["candidate SHA drift", (x) => { x.candidate.m8_commit_sha = "3".repeat(40); }],
    ["artifact pin drift", (x) => { x.external_evidence.m3_authoritative_oas.sha256 = "4".repeat(64); }],
    ["artifact after run", (x) => { x.external_evidence.m3_authoritative_oas.produced_at = "2026-09-04T01:01:00Z"; }],
    ["case removed", (x) => { x.revalidation_matrix.pop(); }],
    ["case reordered", (x) => { [x.revalidation_matrix[3], x.revalidation_matrix[4]] = [x.revalidation_matrix[4], x.revalidation_matrix[3]]; }],
    ["case candidate drift", (x) => { x.revalidation_matrix[3].m3_commit_sha = "5".repeat(40); }],
    ["case outside run", (x) => { x.revalidation_matrix[3].started_at = "2026-09-04T00:59:00Z"; }],
    ["duplicate callback alias", (x) => { x.revalidation_matrix[4].callback_id_alias = x.revalidation_matrix[3].callback_id_alias; }],
    ["wrong recall decision", (x) => { x.revalidation_matrix[3].observed_decision = "ACCEPTED_AFTER_REVALIDATION"; }],
    ["wrong sale-lock blocker", (x) => { x.revalidation_matrix[4].observed_blocker = "NONE"; }],
    ["wrong quality-hold HTTP", (x) => { x.revalidation_matrix[5].observed_http_status = 2000; }],
    ["missing assertion", (x) => { x.revalidation_matrix[7].assertions_passed.pop(); }],
    ["revision relation", (x) => { x.revalidation_matrix[1].source_revision_after = x.revalidation_matrix[1].source_revision_before; }],
    ["state relation", (x) => { x.revalidation_matrix[0].state_after_sha256 = x.revalidation_matrix[0].state_before_sha256; }],
    ["replay key mismatch", (x) => { x.revalidation_matrix[10].idempotency_key_fingerprint_sha256 = hashLabel("OTHER-KEY"); }],
    ["replay body unchanged", (x) => { x.revalidation_matrix[10].immutable_body_sha256 = x.revalidation_matrix[9].immutable_body_sha256; }],
    ["green-only selection", (x) => { x.matrix_summary.selected_green_cases_only = true; }],
    ["signer reused", (x) => { x.signoffs[1].signer_alias = x.signoffs[0].signer_alias; }],
    ["signer equals verifier", (x) => { x.signoffs[0].verifier_alias = x.signoffs[0].signer_alias; }],
    ["verifier is another signer", (x) => { x.signoffs[0].verifier_alias = x.signoffs[1].signer_alias; }],
    ["signoff before run", (x) => { x.signoffs[0].reviewed_at = "2026-09-04T01:29:00Z"; }],
    ["unsafe production flag", (x) => { x.safety.production_enabled = true; }],
    ["email PII", (x) => { x.signoffs[0].authority_ref = ["USER", "EXAMPLE.COM"].join("@"); }],
    ["credential", (x) => { x.external_evidence.m3_consumer_cdc.artifact_ref = ["BEARER", "SECRET"].join(" "); }],
    ["unknown root key", (x) => { x.unexpected = false; }],
  ];
  for (const [label, mutate] of mutations) {
    const changed = clone(report);
    mutate(changed);
    expectRefusal(label, () => validateReport(changed, expected, "completed"));
  }

  const wrongPins = { ...expected, m8_commit_sha: "9".repeat(40) };
  expectRefusal("wrong independent M8 pin", () => validateReport(report, wrongPins, "completed"));

  const artifactsRoot = resolve(REPOSITORY_ROOT, "ci-artifacts");
  mkdirSync(artifactsRoot, { recursive: true });
  const tempRoot = mkdtempSync(resolve(artifactsRoot, "w0178-selftest-"));
  if (!isConfined(tempRoot)) fail("self-test temp path escaped repository");
  try {
    const validPath = resolve(tempRoot, "valid.json");
    writeFileSync(validPath, `${JSON.stringify(report)}\n`, "utf8");
    validateReport(readStrictJson(validPath), expected, "completed");

    const duplicatePath = resolve(tempRoot, "duplicate.json");
    writeFileSync(duplicatePath, '{"schema_version":"a","schema_version":"b"}\n', "utf8");
    expectRefusal("duplicate JSON key", () => readStrictJson(duplicatePath));

    const oversizedPath = resolve(tempRoot, "oversized.json");
    writeFileSync(oversizedPath, "x".repeat(MAX_INPUT_BYTES + 1), "utf8");
    expectRefusal("oversized input", () => readStrictJson(oversizedPath));

    expectRefusal("outside repository", () => readStrictJson(resolve(REPOSITORY_ROOT, "..", "w0178-outside.json")));
  } finally {
    rmSync(tempRoot, { recursive: true, force: true });
  }
  const refusals = mutations.length + 4;
  process.stdout.write(`W0178_SELFTEST_PASS template=1 valid=1 refusals=${refusals}\n`);
}

function parseArgs(argv) {
  const result = {};
  for (let index = 0; index < argv.length; index += 1) {
    const key = argv[index];
    if (key === "--self-test") result.selfTest = true;
    else if (key === "--print-template") result.printTemplate = true;
    else if (key === "--check-template") result.template = argv[++index];
    else if (key === "--input") result.input = argv[++index];
    else if (key.startsWith("--")) result[key.slice(2).replaceAll("-", "_")] = argv[++index];
    else fail(`unknown argument: ${key}`);
  }
  return result;
}

function usage() {
  return [
    "Usage:",
    "  node deploy/ci/scripts/d06-revalidation-evidence-validator.mjs --self-test",
    "  node deploy/ci/scripts/d06-revalidation-evidence-validator.mjs --print-template",
    "  node deploy/ci/scripts/d06-revalidation-evidence-validator.mjs --check-template <path>",
    "  node deploy/ci/scripts/d06-revalidation-evidence-validator.mjs --input <path> \\",
    "    --m8-commit-sha <40hex> --m3-commit-sha <40hex> \\",
    "    --m3-oas-sha256 <64hex> --m3-implementation-sha256 <64hex> --m3-cdc-sha256 <64hex> \\",
    "    --ops-truth-sha256 <64hex> --security-auth-sha256 <64hex> --platform-evidence-sha256 <64hex>",
  ].join("\n");
}

function main() {
  const args = parseArgs(process.argv.slice(2));
  if (args.selfTest) {
    runSelfTest();
    return;
  }
  if (args.printTemplate) {
    process.stdout.write(`${JSON.stringify(makePendingTemplate(), null, 2)}\n`);
    return;
  }
  if (args.template) {
    validateReport(readStrictJson(args.template), null, "template");
    process.stdout.write(`D06_TEMPLATE_VALID_NOT_READY cases=${CASE_RULES.length} production_authorized=false\n`);
    return;
  }
  if (args.input) {
    const expected = {
      m8_commit_sha: args.m8_commit_sha,
      m3_commit_sha: args.m3_commit_sha,
      m3_oas_sha256: args.m3_oas_sha256,
      m3_implementation_sha256: args.m3_implementation_sha256,
      m3_cdc_sha256: args.m3_cdc_sha256,
      ops_truth_sha256: args.ops_truth_sha256,
      security_auth_sha256: args.security_auth_sha256,
      platform_evidence_sha256: args.platform_evidence_sha256,
    };
    validateReport(readStrictJson(args.input), expected, "completed");
    process.stdout.write(
      `D06_EVIDENCE_VALID_FOR_SHARED_E2E_REVIEW_ONLY cases=${CASE_RULES.length} ` +
      `m8=${expected.m8_commit_sha} m3=${expected.m3_commit_sha} production_authorized=false\n`,
    );
    return;
  }
  fail(usage());
}

try {
  main();
} catch (error) {
  process.stderr.write(`W0178_VALIDATION_FAILED: ${error.message}\n`);
  process.exitCode = 1;
}
