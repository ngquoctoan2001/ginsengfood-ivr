#!/usr/bin/env node

// W-0183 — Offline, metadata-only validator for a signed B5+C12 contact/dial-token
// production bundle. It never selects a token model, contacts an issuer/resolver,
// reads secrets or raw contact data, changes runtime code, or authorizes real calls.

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
const MAX_INPUT_BYTES = 768 * 1024;
const SCHEMA_VERSION = "m8-contact-dial-token-production-bundle.v1";
const WORK_ID = "W-0183";
const PLACEHOLDER = "PENDING_EXTERNAL_DECISION";

const SOURCE_PINS = Object.freeze({
  audit_evidence_path: "docs/evidence/W-0150/README.md",
  audit_evidence_sha256: "cae6e65885e951c4fcbda0d2344b4c13d763205a6b5f9ce729352e10ee71dc98",
  closure_contract_path: "docs/contracts/target-v1-closure-pack/T-04-dial-token.md",
  closure_contract_sha256: "e7df35e7711a59ed21076a4330a2e6c67e08d3213cd62813981cd89efc184f9f",
  telephony_requirements_path: "integration-requirements/03-telephony-sim-requirements.md",
  telephony_requirements_sha256: "8025bb2809c026d7c04340373a600e8ef70399cf394320ca2ce6e6e58b28811e",
  m3_handover_path: "integration-requirements/06-module-3-api-handover.md",
  m3_handover_sha256: "b676a32d4ba51b9f345eb3d32e21d793216f4011e98bbfc9dc8d2867997ba08a",
  task_oas_path: "specs/api/openapi/ivr-order-confirmation.v1.yaml",
  task_oas_sha256: "5809f8b06f52ab3979040885280a7c9a35bba5fc97624b3e0a8360a3df096bb3",
  resolver_port_path: "src/Ivr.Domain/Ports/ProviderPorts.cs",
  resolver_port_sha256: "24fd793c268c3f1aad1955615ce6535f235ccb301b69d6517905bb6ce7b8db5e",
  intake_service_path: "src/Ivr.Infrastructure/Intake/TaskIntakeService.cs",
  intake_service_sha256: "6cf48edf19bb31dd4befb1e211bcf468e3d32b8fc39d0ea742ef9e2d934fbd4d",
  requirement_scope: "B5-C12-DTK.2026-09-04",
});

const SOURCE_FILES = Object.freeze([
  ["audit_evidence_path", "audit_evidence_sha256"],
  ["closure_contract_path", "closure_contract_sha256"],
  ["telephony_requirements_path", "telephony_requirements_sha256"],
  ["m3_handover_path", "m3_handover_sha256"],
  ["task_oas_path", "task_oas_sha256"],
  ["resolver_port_path", "resolver_port_sha256"],
  ["intake_service_path", "intake_service_sha256"],
]);

const DECISION_RULES = Object.freeze([
  ["DTK-01", "CONTACT_AUTHORITY_AND_PRODUCER", ["authority_named", "producer_contract_pinned", "positive_negative_cdc_attached"]],
  ["DTK-02", "CONTACT_REQUIREDNESS_AND_TAXONOMY", ["validation_rule_explicit", "invalid_inconclusive_action_explicit", "compatibility_plan_signed"]],
  ["DTK-03", "ISSUER_AND_TOKEN_FORMAT", ["issuer_versioned", "token_opaque_non_pii", "entropy_and_size_limits_signed"]],
  ["DTK-04", "SUBJECT_SCOPE_AUDIENCE_BINDING", ["task_environment_provider_binding", "cross_scope_replay_denied", "negative_tests_attached"]],
  ["DTK-05", "SCALAR_REISSUE_OR_BUNDLE_MODEL", ["one_model_selected", "wire_delta_declared", "migration_cutover_declared"]],
  ["DTK-06", "TTL_AND_TIME_SEMANTICS", ["window_coverage_signed", "clock_skew_signed", "mid_window_expiry_action_signed"]],
  ["DTK-07", "RESOLVER_TOPOLOGY_AND_OUTPUT", ["raw_e164_boundary_named", "ivr_receives_opaque_reference", "data_flow_diagram_approved"]],
  ["DTK-08", "RESOLVE_PROTOCOL_AND_AUTH", ["authoritative_protocol_pinned", "auth_audience_scope_signed", "timeout_error_idempotency_signed"]],
  ["DTK-09", "CUSTODY_KEY_AND_CREDENTIAL", ["no_mapping_key_in_ivr", "workload_identity_least_privilege", "cluster_evidence_attached"]],
  ["DTK-10", "ROTATION_AND_REVOCATION", ["key_version_overlap_signed", "emergency_revoke_defined", "drill_evidence_attached"]],
  ["DTK-11", "REPLAY_CONCURRENCY_AND_ONE_USE", ["same_attempt_behavior_signed", "different_attempt_behavior_signed", "atomic_parallel_proof_attached"]],
  ["DTK-12", "FAILURE_RETRY_AND_REFRESH", ["fail_closed_not_counted", "bounded_deadline_retry", "refresh_route_explicit"]],
  ["DTK-13", "AUDIT_PRIVACY_AND_RETENTION", ["split_audit_correlation", "sensitive_values_forbidden", "retention_purge_access_signed"]],
  ["DTK-14", "TELEPHONY_CAPABILITY_AND_SAFETY", ["vendor_destination_capability", "recording_off_and_caller_id", "allowlist_kill_switch_and_failure_matrix"]],
  ["DTK-15", "ROLLOUT_ROLLBACK_AND_RELEASE", ["contract_custody_network_first", "sandbox_lab_pilot_sequence", "exact_sha_e2e_rollback_go_no_go"]],
].map(([decision_id, topic, assertions]) => ({ decision_id, topic, assertions })));

const CASE_RULES = Object.freeze([
  ["DTK-E2E-01", "VALID_CONTACT_AND_AUTHORIZATION", "AUTHORIZED_TO_TRUSTED_GATEWAY", false, ["contact_validated", "token_bound_and_current", "opaque_destination_only"]],
  ["DTK-E2E-02", "MISSING_OR_INVALID_CONTACT_STATUS", "CONTACT_REJECTED_FAIL_CLOSED", false, ["producer_or_intake_rejected", "no_resolution", "no_dial"]],
  ["DTK-E2E-03", "TOKEN_EXPIRED_AT_INTAKE", "TOKEN_REJECTED_EXPIRED", false, ["issuer_time_checked", "no_persistence_plaintext", "no_dial"]],
  ["DTK-E2E-04", "TOKEN_EXPIRES_BEFORE_WINDOW_END", "TOKEN_REJECTED_TTL_COVERAGE", false, ["window_coverage_checked", "no_resolution", "no_dial"]],
  ["DTK-E2E-05", "TTL_MAX_OR_EXACT_BOUNDARY", "TTL_POLICY_ENFORCED", false, ["signed_cross_field_invariant_used", "clock_skew_applied", "boundary_result_recorded"]],
  ["DTK-E2E-06", "CROSS_TASK_ENVIRONMENT_OR_PROVIDER_REPLAY", "REPLAY_REJECTED_SCOPE_MISMATCH", false, ["subject_scope_audience_checked", "no_destination_disclosed", "no_dial"]],
  ["DTK-E2E-07", "SAME_ATTEMPT_REPLAY", "SAME_ATTEMPT_POLICY_ENFORCED", false, ["same_attempt_rule_used", "deterministic_receipt_returned", "no_duplicate_dial"]],
  ["DTK-E2E-08", "DIFFERENT_ATTEMPT_REUSE", "TOKEN_MODEL_POLICY_ENFORCED", false, ["different_attempt_rule_used", "selected_model_enforced", "result_audited"]],
  ["DTK-E2E-09", "RESOLVER_TIMEOUT_OR_OUTAGE", "TECHNICAL_FAILURE_FAIL_CLOSED", false, ["bounded_timeout", "customer_attempt_not_counted", "deadline_and_alert_enforced"]],
  ["DTK-E2E-10", "KEY_ROTATION_AND_EMERGENCY_REVOKE", "ROTATION_REVOCATION_ENFORCED", false, ["key_version_checked", "overlap_bounded", "revoked_authorization_denied"]],
  ["DTK-E2E-11", "CONCURRENT_RESOLVE_CONSUMPTION", "ATOMIC_CONSUMPTION_ENFORCED", false, ["parallel_race_executed", "single_authorization_outcome", "no_duplicate_dial"]],
  ["DTK-E2E-12", "RESOLVE_OR_DIAL_RESPONSE_LOST", "IDEMPOTENT_RECOVERY_NO_DUPLICATE_DIAL", false, ["stable_idempotency_key", "receipt_reconciled", "no_second_call_created"]],
  ["DTK-E2E-13", "AUDIT_REDACTION_RETENTION_AND_PURGE", "PRIVACY_CONTROLS_ENFORCED", false, ["correlation_joinable", "sensitive_values_absent", "retention_and_purge_verified"]],
  ["DTK-E2E-14", "ROLLBACK_AND_KILL_SWITCH", "ROLLBACK_FAIL_CLOSED", false, ["production_flag_remained_false", "kill_switch_default_on", "rollback_restored_safe_state"]],
].map(([case_id, scenario, outcome, counted, assertions]) => ({ case_id, scenario, outcome, counted, assertions })));

const EXTERNAL_EVIDENCE_KEYS = Object.freeze([
  "m3_contact_producer_contract",
  "issuer_token_spec_and_cdc",
  "security_threat_model",
  "platform_custody_auth_network",
  "telephony_vendor_capability",
  "privacy_retention_and_purge",
  "shared_cdc_e2e_plan",
  "cutover_rollback_release_packet",
]);

const EXTERNAL_PIN_KEYS = Object.freeze({
  m3_contact_producer_contract: "m3_contact_sha256",
  issuer_token_spec_and_cdc: "issuer_token_sha256",
  security_threat_model: "security_threat_sha256",
  platform_custody_auth_network: "platform_custody_sha256",
  telephony_vendor_capability: "telephony_capability_sha256",
  privacy_retention_and_purge: "privacy_retention_sha256",
  shared_cdc_e2e_plan: "shared_e2e_plan_sha256",
  cutover_rollback_release_packet: "release_packet_sha256",
});

const REQUIRED_SIGNOFF_ROLES = Object.freeze([
  "PROJECT_OWNER", "M8_OWNER", "M3_CONTACT_OWNER", "PRODUCT_OWNER", "SECURITY",
  "PLATFORM", "TELEPHONY_VENDOR", "PRIVACY_LEGAL", "RELEASE_OWNER",
]);

const TOKEN_MODELS = Object.freeze([
  "SCALAR_REUSABLE_WITHIN_TTL_PER_ATTEMPT_AUTH",
  "PER_ATTEMPT_TOKEN_ARRAY",
  "REISSUE_ENDPOINT",
  "TOKEN_BUNDLE",
]);

const ROOT_KEYS = ["schema_version", "work_id", "status", "source", "candidate", "bundle", "decision_coverage", "external_evidence", "validation_matrix", "matrix_summary", "signoffs", "safety"];
const CANDIDATE_KEYS = ["m8_repo_ref", "m8_commit_sha", "m3_repo_ref", "m3_commit_sha", "environment_id", "config_version", "run_started_at", "run_completed_at"];
const BUNDLE_KEYS = ["bundle_id", "contract_version", "bundle_sha256", "decision_coverage_sha256", "issued_at", "effective_at", "contact_contract", "token_model", "ttl_policy", "trust_boundary", "resolve_protocol", "custody", "replay_concurrency", "failure_retry", "audit_privacy", "telephony_safety", "cutover_rollback"];
const CONTACT_KEYS = ["authority_alias", "producer_ref", "phone_validation_contract", "invalid_contact_action", "raw_e164_in_task"];
const TOKEN_KEYS = ["model", "requires_wire_change", "reissue_contract_ref", "token_opaque", "token_contains_pii", "same_attempt_behavior", "different_attempt_behavior"];
const TTL_KEYS = ["coverage", "clock_skew_seconds", "not_before_required", "mid_window_expiry_action", "cross_field_invariant_ref"];
const TRUST_KEYS = ["ivr_sees_raw_e164", "ivr_stores_mapping_key", "resolver_output_to_ivr", "raw_e164_location", "vendor_destination_mode", "diagram_ref", "diagram_sha256"];
const PROTOCOL_KEYS = ["spec_ref", "spec_sha256", "protocol_version", "auth_mode", "audience", "scope", "timeout_milliseconds", "idempotency_contract_ref", "error_taxonomy_ref", "sandbox_conformance_ref"];
const CUSTODY_KEYS = ["secret_store_alias", "workload_identity_alias", "mapping_or_decryption_keys_in_ivr", "least_privilege_enforced", "rotation_overlap_seconds", "emergency_revoke_supported", "access_review_ref", "access_review_sha256"];
const REPLAY_KEYS = ["atomic_consumption", "cross_task_denied", "cross_environment_denied", "cross_provider_denied", "same_attempt_behavior", "different_attempt_behavior", "parallel_test_ref", "parallel_test_sha256"];
const FAILURE_KEYS = ["resolver_fail_closed", "technical_failure_counted_as_customer_attempt", "bounded_retry", "max_technical_retries", "breaker_enabled", "deadline_enforced", "retry_policy_ref", "refresh_route"];
const AUDIT_KEYS = ["ivr_audit_outcome_only", "logs_raw_token", "logs_ciphertext", "logs_destination_handle", "logs_raw_e164", "correlation_joinable", "retention_policy_ref", "retention_policy_sha256", "purge_proof_ref", "purge_proof_sha256"];
const TELEPHONY_KEYS = ["capability_ref", "capability_sha256", "recording_disabled", "allowlist_enforced", "kill_switch_available", "caller_id_policy_ref", "disposition_matrix_ref", "dtmf_contract_ref"];
const CUTOVER_KEYS = ["shared_e2e_plan_ref", "shared_e2e_plan_sha256", "contract_before_runtime", "sandbox_required", "allowlisted_lab_required", "pilot_required", "rollback_ref", "rollback_sha256", "kill_switch_default_on", "production_real_enabled", "exact_candidate_binding"];
const DECISION_KEYS = ["decision_id", "topic", "state", "selection_ref", "decision_ref", "decision_sha256", "assertions_passed"];
const ARTIFACT_KEYS = ["artifact_ref", "sha256", "producer_alias", "produced_at"];
const CASE_KEYS = ["case_id", "scenario", "state", "m8_commit_sha", "m3_commit_sha", "test_owner_alias", "test_spec_ref", "test_spec_sha256", "expected_outcome", "expected_customer_attempt_counted", "real_customer_call_required", "assertions_covered"];
const SUMMARY_KEYS = ["required_cases", "approved_test_plans", "pending_cases", "complete_matrix", "all_cases_same_candidate", "green_cases_selected_only", "runtime_execution_claimed"];
const SIGNOFF_KEYS = ["role", "signer_alias", "verifier_alias", "authority_ref", "authority_sha256", "decision", "signed_at", "m8_commit_sha", "m3_commit_sha", "bundle_sha256"];
const SAFETY_KEYS = ["contains_raw_contact_or_e164", "contains_raw_token", "contains_ciphertext", "contains_destination_handle", "contains_credentials_or_secrets", "contains_customer_data", "mock_or_lab_claimed_as_production", "vault_resolver_or_adapter_added", "secret_mounted_or_egress_enabled", "production_real_enabled", "real_customer_call_allowed", "validator_claims_production_authorization"];

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
  if (value === null || typeof value !== "object" || Array.isArray(value)) fail(`${label} must be an object`);
}

function assertExactKeys(value, expected, label) {
  assertRecord(value, label);
  const actual = Object.keys(value).sort();
  const wanted = [...expected].sort();
  if (JSON.stringify(actual) !== JSON.stringify(wanted)) {
    fail(`${label} keys mismatch: expected ${wanted.join(",")}; got ${actual.join(",")}`);
  }
}

function assertString(value, label, minimum = 2, maximum = 180) {
  if (typeof value !== "string" || value.length < minimum || value.length > maximum || value.trim() !== value) {
    fail(`${label} must be a trimmed string of ${minimum}..${maximum} characters`);
  }
  if (/[\u0000-\u001f\u007f]/u.test(value)) fail(`${label} contains a control character`);
}

function assertIdentifier(value, label) {
  assertString(value, label);
  if (!/^[A-Z0-9][A-Z0-9._:/-]+$/u.test(value)) fail(`${label} must be an uppercase alias/reference`);
}

function assertSha(value, label) {
  if (typeof value !== "string" || !/^[a-f0-9]{64}$/u.test(value)) fail(`${label} must be lowercase SHA-256`);
}

function assertGitSha(value, label) {
  if (typeof value !== "string" || !/^[a-f0-9]{40}$/u.test(value)) fail(`${label} must be a full lowercase Git SHA`);
}

function parseTimestamp(value, label) {
  assertString(value, label, 20, 35);
  if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,3})?Z$/u.test(value)) {
    fail(`${label} must be ISO-8601 UTC`);
  }
  const parsed = Date.parse(value);
  if (!Number.isFinite(parsed)) fail(`${label} is not a valid timestamp`);
  return parsed;
}

function assertInteger(value, label, minimum, maximum) {
  if (!Number.isInteger(value) || value < minimum || value > maximum) {
    fail(`${label} must be an integer in ${minimum}..${maximum}`);
  }
}

function isConfined(pathValue) {
  const rel = relative(REPOSITORY_ROOT, pathValue);
  return rel !== "" && !rel.startsWith("..") && !isAbsolute(rel);
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
    while (/\s/u.test(this.text[this.offset] ?? "")) this.offset += 1;
  }

  parseValue(path) {
    this.skipWhitespace();
    const ch = this.text[this.offset];
    if (ch === "{") return this.parseObject(path);
    if (ch === "[") return this.parseArray(path);
    if (ch === '"') return this.parseString();
    const primitive = this.text.slice(this.offset).match(/^(?:true|false|null|-?(?:0|[1-9]\d*)(?:\.\d+)?(?:[eE][+-]?\d+)?)/u)?.[0];
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

function canonicalize(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalize).join(",")}]`;
  if (value !== null && typeof value === "object") {
    return `{${Object.keys(value).sort().map((key) => `${JSON.stringify(key)}:${canonicalize(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}

function canonicalBundleHash(bundle) {
  const copy = structuredClone(bundle);
  delete copy.bundle_sha256;
  return sha256(Buffer.from(canonicalize(copy), "utf8"));
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
  if (typeof value !== "string" || /^[a-f0-9]{40,64}$/u.test(value)) return;
  if (/\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b/iu.test(value)) fail(`${label} contains an email-like value`);
  if (/^\+?\d[\d .()-]{7,}\d$/u.test(value)) fail(`${label} contains a phone-like value`);
  if (/\b(?:bearer\s+[a-z0-9._~-]+|password\s*[:=]|api[_-]?key\s*[:=]|authorization\s*[:=])/iu.test(value)) {
    fail(`${label} contains a credential-like value`);
  }
  if (/^https?:\/\/[^\s]+[?&][^\s]+$/iu.test(value)) fail(`${label} contains a URL query string`);
}

function validateLocalSources(source) {
  assertExactKeys(source, Object.keys(SOURCE_PINS), "source");
  for (const key of Object.keys(SOURCE_PINS)) {
    if (source[key] !== SOURCE_PINS[key]) fail(`source.${key} does not match W-0183`);
  }
  for (const [pathKey, hashKey] of SOURCE_FILES) {
    const absolute = resolve(REPOSITORY_ROOT, source[pathKey]);
    if (!isConfined(absolute)) fail(`source.${pathKey} escapes repository`);
    const info = lstatSync(absolute);
    if (!info.isFile() || info.isSymbolicLink()) fail(`source.${pathKey} must be a regular file`);
    if (sha256(readFileSync(absolute)) !== source[hashKey]) fail(`local source hash drift: ${source[pathKey]}`);
  }
}

function validateExpected(expected) {
  const keys = [
    "m8_commit_sha", "m3_commit_sha", "bundle_sha256", "m3_contact_sha256",
    "issuer_token_sha256", "security_threat_sha256", "platform_custody_sha256",
    "telephony_capability_sha256", "privacy_retention_sha256", "shared_e2e_plan_sha256",
    "release_packet_sha256",
  ];
  assertExactKeys(expected, keys, "independent CLI pins");
  assertGitSha(expected.m8_commit_sha, "--m8-commit-sha");
  assertGitSha(expected.m3_commit_sha, "--m3-commit-sha");
  for (const key of keys.slice(2)) assertSha(expected[key], `--${key.replaceAll("_", "-")}`);
}

function validateCandidate(candidate, expected) {
  assertExactKeys(candidate, CANDIDATE_KEYS, "candidate");
  for (const key of ["m8_repo_ref", "m3_repo_ref", "environment_id", "config_version"]) {
    assertIdentifier(candidate[key], `candidate.${key}`);
  }
  assertGitSha(candidate.m8_commit_sha, "candidate.m8_commit_sha");
  assertGitSha(candidate.m3_commit_sha, "candidate.m3_commit_sha");
  if (candidate.m8_commit_sha !== expected.m8_commit_sha) fail("candidate M8 SHA does not match independent pin");
  if (candidate.m3_commit_sha !== expected.m3_commit_sha) fail("candidate M3 SHA does not match independent pin");
  const started = parseTimestamp(candidate.run_started_at, "candidate.run_started_at");
  const completed = parseTimestamp(candidate.run_completed_at, "candidate.run_completed_at");
  if (completed < started) fail("candidate review completed before it started");
  return { started, completed };
}

function requireTrue(value, label) {
  if (value !== true) fail(`${label} must be true`);
}

function requireFalse(value, label) {
  if (value !== false) fail(`${label} must be false`);
}

function requireEnum(value, allowed, label) {
  if (!allowed.includes(value)) fail(`${label} must be one of ${allowed.join(",")}`);
}

function validateBundle(bundle, expected, reviewWindow) {
  assertExactKeys(bundle, BUNDLE_KEYS, "bundle");
  assertIdentifier(bundle.bundle_id, "bundle.bundle_id");
  assertIdentifier(bundle.contract_version, "bundle.contract_version");
  assertSha(bundle.bundle_sha256, "bundle.bundle_sha256");
  assertSha(bundle.decision_coverage_sha256, "bundle.decision_coverage_sha256");
  if (canonicalBundleHash(bundle) !== bundle.bundle_sha256) fail("bundle.bundle_sha256 is not the canonical bundle hash");
  if (bundle.bundle_sha256 !== expected.bundle_sha256) fail("bundle hash does not match independent CLI pin");
  const issued = parseTimestamp(bundle.issued_at, "bundle.issued_at");
  const effective = parseTimestamp(bundle.effective_at, "bundle.effective_at");
  if (effective < issued) fail("bundle effective_at precedes issued_at");
  if (effective > reviewWindow.started) fail("bundle was not effective when review started");

  const contact = bundle.contact_contract;
  assertExactKeys(contact, CONTACT_KEYS, "bundle.contact_contract");
  assertIdentifier(contact.authority_alias, "contact.authority_alias");
  assertIdentifier(contact.producer_ref, "contact.producer_ref");
  requireEnum(contact.phone_validation_contract, ["REQUIRED_VALID_ONLY", "PRODUCER_FILTERED_REQUIRED_VALID"], "contact.phone_validation_contract");
  requireEnum(contact.invalid_contact_action, ["DO_NOT_ISSUE_TASK", "REJECT_AT_INTAKE"], "contact.invalid_contact_action");
  requireFalse(contact.raw_e164_in_task, "contact.raw_e164_in_task");

  const token = bundle.token_model;
  assertExactKeys(token, TOKEN_KEYS, "bundle.token_model");
  requireEnum(token.model, TOKEN_MODELS, "token.model");
  requireTrue(token.token_opaque, "token.token_opaque");
  requireFalse(token.token_contains_pii, "token.token_contains_pii");
  requireEnum(token.same_attempt_behavior, ["IDEMPOTENT_SAME_AUTHORIZATION", "DETERMINISTIC_REPLAY_REJECT"], "token.same_attempt_behavior");
  const differentByModel = {
    SCALAR_REUSABLE_WITHIN_TTL_PER_ATTEMPT_AUTH: "ALLOWED_WITH_ATTEMPT_BINDING",
    PER_ATTEMPT_TOKEN_ARRAY: "DISTINCT_TOKEN_REQUIRED",
    REISSUE_ENDPOINT: "REISSUE_REQUIRED",
    TOKEN_BUNDLE: "BUNDLE_SLOT_REQUIRED",
  };
  if (token.different_attempt_behavior !== differentByModel[token.model]) fail("token different-attempt behavior contradicts selected model");
  const wireChangeRequired = token.model !== "SCALAR_REUSABLE_WITHIN_TTL_PER_ATTEMPT_AUTH";
  if (token.requires_wire_change !== wireChangeRequired) fail("token.requires_wire_change contradicts selected model");
  assertIdentifier(token.reissue_contract_ref, "token.reissue_contract_ref");
  if (token.model === "REISSUE_ENDPOINT" && token.reissue_contract_ref === "NONE") fail("reissue model requires a signed reissue contract ref");
  if (token.model !== "REISSUE_ENDPOINT" && token.reissue_contract_ref !== "NONE") fail("reissue contract ref must be NONE for the selected model");

  const ttl = bundle.ttl_policy;
  assertExactKeys(ttl, TTL_KEYS, "bundle.ttl_policy");
  requireEnum(ttl.coverage, ["EXACT_WINDOW_END", "AT_LEAST_WINDOW_WITH_SIGNED_MAX"], "ttl.coverage");
  assertInteger(ttl.clock_skew_seconds, "ttl.clock_skew_seconds", 0, 300);
  if (typeof ttl.not_before_required !== "boolean") fail("ttl.not_before_required must be boolean");
  requireEnum(ttl.mid_window_expiry_action, ["CLOSE_TASK", "REISSUE_VIA_SIGNED_CONTRACT", "CREATE_NEW_TASK"], "ttl.mid_window_expiry_action");
  if (ttl.mid_window_expiry_action === "REISSUE_VIA_SIGNED_CONTRACT" && token.model !== "REISSUE_ENDPOINT") {
    fail("TTL reissue action requires the REISSUE_ENDPOINT model");
  }
  assertIdentifier(ttl.cross_field_invariant_ref, "ttl.cross_field_invariant_ref");

  const trust = bundle.trust_boundary;
  assertExactKeys(trust, TRUST_KEYS, "bundle.trust_boundary");
  requireFalse(trust.ivr_sees_raw_e164, "trust.ivr_sees_raw_e164");
  requireFalse(trust.ivr_stores_mapping_key, "trust.ivr_stores_mapping_key");
  if (trust.resolver_output_to_ivr !== "OPAQUE_PROVIDER_REFERENCE") fail("resolver output to IVR must remain opaque");
  requireEnum(trust.raw_e164_location, ["EXTERNAL_VAULT_GATEWAY_ONLY", "TELEPHONY_VENDOR_BOUNDARY_ONLY"], "trust.raw_e164_location");
  requireEnum(trust.vendor_destination_mode, ["OPAQUE_REFERENCE", "E164_INSIDE_TRUSTED_GATEWAY"], "trust.vendor_destination_mode");
  assertIdentifier(trust.diagram_ref, "trust.diagram_ref");
  assertSha(trust.diagram_sha256, "trust.diagram_sha256");

  const protocol = bundle.resolve_protocol;
  assertExactKeys(protocol, PROTOCOL_KEYS, "bundle.resolve_protocol");
  for (const key of ["spec_ref", "protocol_version", "audience", "scope", "idempotency_contract_ref", "error_taxonomy_ref", "sandbox_conformance_ref"]) {
    assertIdentifier(protocol[key], `protocol.${key}`);
  }
  assertSha(protocol.spec_sha256, "protocol.spec_sha256");
  requireEnum(protocol.auth_mode, ["MTLS", "WORKLOAD_JWT", "MTLS_AND_WORKLOAD_JWT", "SIGNED_PROVIDER_SDK"], "protocol.auth_mode");
  assertInteger(protocol.timeout_milliseconds, "protocol.timeout_milliseconds", 50, 10000);

  const custody = bundle.custody;
  assertExactKeys(custody, CUSTODY_KEYS, "bundle.custody");
  assertIdentifier(custody.secret_store_alias, "custody.secret_store_alias");
  assertIdentifier(custody.workload_identity_alias, "custody.workload_identity_alias");
  requireFalse(custody.mapping_or_decryption_keys_in_ivr, "custody.mapping_or_decryption_keys_in_ivr");
  requireTrue(custody.least_privilege_enforced, "custody.least_privilege_enforced");
  assertInteger(custody.rotation_overlap_seconds, "custody.rotation_overlap_seconds", 0, 86400);
  requireTrue(custody.emergency_revoke_supported, "custody.emergency_revoke_supported");
  assertIdentifier(custody.access_review_ref, "custody.access_review_ref");
  assertSha(custody.access_review_sha256, "custody.access_review_sha256");

  const replay = bundle.replay_concurrency;
  assertExactKeys(replay, REPLAY_KEYS, "bundle.replay_concurrency");
  for (const key of ["atomic_consumption", "cross_task_denied", "cross_environment_denied", "cross_provider_denied"]) {
    requireTrue(replay[key], `replay.${key}`);
  }
  if (replay.same_attempt_behavior !== token.same_attempt_behavior) fail("replay same-attempt behavior must match token model");
  if (replay.different_attempt_behavior !== token.different_attempt_behavior) fail("replay different-attempt behavior must match token model");
  assertIdentifier(replay.parallel_test_ref, "replay.parallel_test_ref");
  assertSha(replay.parallel_test_sha256, "replay.parallel_test_sha256");

  const failure = bundle.failure_retry;
  assertExactKeys(failure, FAILURE_KEYS, "bundle.failure_retry");
  requireTrue(failure.resolver_fail_closed, "failure.resolver_fail_closed");
  requireFalse(failure.technical_failure_counted_as_customer_attempt, "failure.technical_failure_counted_as_customer_attempt");
  requireTrue(failure.bounded_retry, "failure.bounded_retry");
  assertInteger(failure.max_technical_retries, "failure.max_technical_retries", 0, 10);
  requireTrue(failure.breaker_enabled, "failure.breaker_enabled");
  requireTrue(failure.deadline_enforced, "failure.deadline_enforced");
  assertIdentifier(failure.retry_policy_ref, "failure.retry_policy_ref");
  requireEnum(failure.refresh_route, ["CLOSE_TASK", "CREATE_NEW_TASK", "SIGNED_REISSUE_ENDPOINT"], "failure.refresh_route");
  if ((failure.refresh_route === "SIGNED_REISSUE_ENDPOINT") !== (token.model === "REISSUE_ENDPOINT")) {
    fail("failure refresh route contradicts token model");
  }

  const audit = bundle.audit_privacy;
  assertExactKeys(audit, AUDIT_KEYS, "bundle.audit_privacy");
  requireTrue(audit.ivr_audit_outcome_only, "audit.ivr_audit_outcome_only");
  for (const key of ["logs_raw_token", "logs_ciphertext", "logs_destination_handle", "logs_raw_e164"]) requireFalse(audit[key], `audit.${key}`);
  requireTrue(audit.correlation_joinable, "audit.correlation_joinable");
  for (const key of ["retention_policy_ref", "purge_proof_ref"]) assertIdentifier(audit[key], `audit.${key}`);
  for (const key of ["retention_policy_sha256", "purge_proof_sha256"]) assertSha(audit[key], `audit.${key}`);

  const telephony = bundle.telephony_safety;
  assertExactKeys(telephony, TELEPHONY_KEYS, "bundle.telephony_safety");
  for (const key of ["capability_ref", "caller_id_policy_ref", "disposition_matrix_ref", "dtmf_contract_ref"]) assertIdentifier(telephony[key], `telephony.${key}`);
  assertSha(telephony.capability_sha256, "telephony.capability_sha256");
  requireTrue(telephony.recording_disabled, "telephony.recording_disabled");
  requireTrue(telephony.allowlist_enforced, "telephony.allowlist_enforced");
  requireTrue(telephony.kill_switch_available, "telephony.kill_switch_available");

  const cutover = bundle.cutover_rollback;
  assertExactKeys(cutover, CUTOVER_KEYS, "bundle.cutover_rollback");
  for (const key of ["shared_e2e_plan_ref", "rollback_ref"]) assertIdentifier(cutover[key], `cutover.${key}`);
  for (const key of ["shared_e2e_plan_sha256", "rollback_sha256"]) assertSha(cutover[key], `cutover.${key}`);
  for (const key of ["contract_before_runtime", "sandbox_required", "allowlisted_lab_required", "pilot_required", "kill_switch_default_on", "exact_candidate_binding"]) requireTrue(cutover[key], `cutover.${key}`);
  requireFalse(cutover.production_real_enabled, "cutover.production_real_enabled");
}

function validateDecisionCoverage(decisions, bundle) {
  if (!Array.isArray(decisions) || decisions.length !== DECISION_RULES.length) {
    fail(`decision_coverage must contain exactly ${DECISION_RULES.length} entries`);
  }
  decisions.forEach((decision, index) => {
    const rule = DECISION_RULES[index];
    const label = `decision_coverage[${index}]`;
    assertExactKeys(decision, DECISION_KEYS, label);
    if (decision.decision_id !== rule.decision_id || decision.topic !== rule.topic) {
      fail(`${label} must be ${rule.decision_id}/${rule.topic} in canonical order`);
    }
    if (decision.state !== "APPROVED") fail(`${label}.state must be APPROVED`);
    assertIdentifier(decision.selection_ref, `${label}.selection_ref`);
    assertIdentifier(decision.decision_ref, `${label}.decision_ref`);
    assertSha(decision.decision_sha256, `${label}.decision_sha256`);
    if (JSON.stringify(decision.assertions_passed) !== JSON.stringify(rule.assertions)) {
      fail(`${label}.assertions_passed must match the canonical assertion set`);
    }
  });
  const observed = sha256(Buffer.from(canonicalize(decisions), "utf8"));
  if (observed !== bundle.decision_coverage_sha256) fail("decision coverage hash mismatch");
}

function validateExternalEvidence(externalEvidence, expected, reviewStarted) {
  assertExactKeys(externalEvidence, EXTERNAL_EVIDENCE_KEYS, "external_evidence");
  for (const name of EXTERNAL_EVIDENCE_KEYS) {
    const artifact = externalEvidence[name];
    const label = `external_evidence.${name}`;
    assertExactKeys(artifact, ARTIFACT_KEYS, label);
    assertIdentifier(artifact.artifact_ref, `${label}.artifact_ref`);
    assertSha(artifact.sha256, `${label}.sha256`);
    assertIdentifier(artifact.producer_alias, `${label}.producer_alias`);
    const producedAt = parseTimestamp(artifact.produced_at, `${label}.produced_at`);
    if (artifact.sha256 !== expected[EXTERNAL_PIN_KEYS[name]]) {
      fail(`${label}.sha256 does not match independent CLI pin`);
    }
    if (producedAt > reviewStarted) fail(`${label} was produced after validation review started`);
  }
}

function validateMatrix(matrix, candidate) {
  if (!Array.isArray(matrix) || matrix.length !== CASE_RULES.length) {
    fail(`validation_matrix must contain exactly ${CASE_RULES.length} cases`);
  }
  matrix.forEach((item, index) => {
    const rule = CASE_RULES[index];
    const label = `validation_matrix[${index}]`;
    assertExactKeys(item, CASE_KEYS, label);
    if (item.case_id !== rule.case_id || item.scenario !== rule.scenario) {
      fail(`${label} must be ${rule.case_id}/${rule.scenario} in canonical order`);
    }
    if (item.state !== "APPROVED_TEST_PLAN") fail(`${label}.state must be APPROVED_TEST_PLAN`);
    if (item.m8_commit_sha !== candidate.m8_commit_sha || item.m3_commit_sha !== candidate.m3_commit_sha) {
      fail(`${label} is not bound to exact M8/M3 candidate`);
    }
    assertIdentifier(item.test_owner_alias, `${label}.test_owner_alias`);
    assertIdentifier(item.test_spec_ref, `${label}.test_spec_ref`);
    assertSha(item.test_spec_sha256, `${label}.test_spec_sha256`);
    if (item.expected_outcome !== rule.outcome) fail(`${label}.expected_outcome mismatch`);
    if (item.expected_customer_attempt_counted !== rule.counted) fail(`${label}.expected_customer_attempt_counted mismatch`);
    requireFalse(item.real_customer_call_required, `${label}.real_customer_call_required`);
    if (JSON.stringify(item.assertions_covered) !== JSON.stringify(rule.assertions)) {
      fail(`${label}.assertions_covered must match canonical order`);
    }
  });
}

function validateSummary(summary) {
  assertExactKeys(summary, SUMMARY_KEYS, "matrix_summary");
  const expected = {
    required_cases: CASE_RULES.length,
    approved_test_plans: CASE_RULES.length,
    pending_cases: 0,
    complete_matrix: true,
    all_cases_same_candidate: true,
    green_cases_selected_only: false,
    runtime_execution_claimed: false,
  };
  for (const [key, value] of Object.entries(expected)) {
    if (summary[key] !== value) fail(`matrix_summary.${key} must be ${value}`);
  }
}

function validateSignoffs(signoffs, candidate, bundle, reviewWindow) {
  if (!Array.isArray(signoffs) || signoffs.length !== REQUIRED_SIGNOFF_ROLES.length) {
    fail(`signoffs must contain exactly ${REQUIRED_SIGNOFF_ROLES.length} entries`);
  }
  const signers = new Set();
  signoffs.forEach((item, index) => {
    const role = REQUIRED_SIGNOFF_ROLES[index];
    const label = `signoffs[${index}]`;
    assertExactKeys(item, SIGNOFF_KEYS, label);
    if (item.role !== role) fail(`${label}.role must be ${role} in canonical order`);
    assertIdentifier(item.signer_alias, `${label}.signer_alias`);
    assertIdentifier(item.verifier_alias, `${label}.verifier_alias`);
    assertIdentifier(item.authority_ref, `${label}.authority_ref`);
    assertSha(item.authority_sha256, `${label}.authority_sha256`);
    if (item.decision !== "APPROVED_FOR_IMPLEMENTATION_REVIEW") {
      fail(`${label}.decision must be APPROVED_FOR_IMPLEMENTATION_REVIEW`);
    }
    const signedAt = parseTimestamp(item.signed_at, `${label}.signed_at`);
    if (signedAt > reviewWindow.started) fail(`${label} was signed after validation review started`);
    if (item.m8_commit_sha !== candidate.m8_commit_sha || item.m3_commit_sha !== candidate.m3_commit_sha) {
      fail(`${label} is not bound to exact candidates`);
    }
    if (item.bundle_sha256 !== bundle.bundle_sha256) fail(`${label} is not bound to bundle hash`);
    if (item.signer_alias === item.verifier_alias) fail(`${label} signer and verifier must differ`);
    if (signers.has(item.signer_alias)) fail(`${label}.signer_alias must be unique`);
    signers.add(item.signer_alias);
  });
  signoffs.forEach((item, index) => {
    if (signers.has(item.verifier_alias)) fail(`signoffs[${index}].verifier_alias must not be any required signer`);
  });
}

function validateSafety(safety) {
  assertExactKeys(safety, SAFETY_KEYS, "safety");
  for (const key of SAFETY_KEYS) requireFalse(safety[key], `safety.${key}`);
}

function validateDialTokenProductionBundle(document, expected) {
  assertExactKeys(document, ROOT_KEYS, "report");
  if (document.schema_version !== SCHEMA_VERSION) fail(`schema_version must be ${SCHEMA_VERSION}`);
  if (document.work_id !== WORK_ID) fail(`work_id must be ${WORK_ID}`);
  if (document.status !== "CONTACT_DIAL_TOKEN_DECISIONS_COMPLETE") {
    fail("status must be CONTACT_DIAL_TOKEN_DECISIONS_COMPLETE");
  }
  validateExpected(expected);
  validateLocalSources(document.source);
  const reviewWindow = validateCandidate(document.candidate, expected);
  validateBundle(document.bundle, expected, reviewWindow);
  validateDecisionCoverage(document.decision_coverage, document.bundle);
  validateExternalEvidence(document.external_evidence, expected, reviewWindow.started);
  validateMatrix(document.validation_matrix, document.candidate);
  validateSummary(document.matrix_summary);
  validateSignoffs(document.signoffs, document.candidate, document.bundle, reviewWindow);
  validateSafety(document.safety);
  assertNoSensitiveScalars(document);
  return document;
}

function pendingObject(keys) {
  return Object.fromEntries(keys.map((key) => [key, PLACEHOLDER]));
}

function makePendingTemplate() {
  const bundle = {
    bundle_id: PLACEHOLDER,
    contract_version: PLACEHOLDER,
    bundle_sha256: PLACEHOLDER,
    decision_coverage_sha256: PLACEHOLDER,
    issued_at: PLACEHOLDER,
    effective_at: PLACEHOLDER,
    contact_contract: {
      authority_alias: PLACEHOLDER,
      producer_ref: PLACEHOLDER,
      phone_validation_contract: PLACEHOLDER,
      invalid_contact_action: PLACEHOLDER,
      raw_e164_in_task: null,
    },
    token_model: {
      model: PLACEHOLDER,
      requires_wire_change: null,
      reissue_contract_ref: PLACEHOLDER,
      token_opaque: null,
      token_contains_pii: null,
      same_attempt_behavior: PLACEHOLDER,
      different_attempt_behavior: PLACEHOLDER,
    },
    ttl_policy: {
      coverage: PLACEHOLDER,
      clock_skew_seconds: null,
      not_before_required: null,
      mid_window_expiry_action: PLACEHOLDER,
      cross_field_invariant_ref: PLACEHOLDER,
    },
    trust_boundary: {
      ivr_sees_raw_e164: null,
      ivr_stores_mapping_key: null,
      resolver_output_to_ivr: PLACEHOLDER,
      raw_e164_location: PLACEHOLDER,
      vendor_destination_mode: PLACEHOLDER,
      diagram_ref: PLACEHOLDER,
      diagram_sha256: PLACEHOLDER,
    },
    resolve_protocol: {
      spec_ref: PLACEHOLDER,
      spec_sha256: PLACEHOLDER,
      protocol_version: PLACEHOLDER,
      auth_mode: PLACEHOLDER,
      audience: PLACEHOLDER,
      scope: PLACEHOLDER,
      timeout_milliseconds: null,
      idempotency_contract_ref: PLACEHOLDER,
      error_taxonomy_ref: PLACEHOLDER,
      sandbox_conformance_ref: PLACEHOLDER,
    },
    custody: {
      secret_store_alias: PLACEHOLDER,
      workload_identity_alias: PLACEHOLDER,
      mapping_or_decryption_keys_in_ivr: null,
      least_privilege_enforced: null,
      rotation_overlap_seconds: null,
      emergency_revoke_supported: null,
      access_review_ref: PLACEHOLDER,
      access_review_sha256: PLACEHOLDER,
    },
    replay_concurrency: {
      atomic_consumption: null,
      cross_task_denied: null,
      cross_environment_denied: null,
      cross_provider_denied: null,
      same_attempt_behavior: PLACEHOLDER,
      different_attempt_behavior: PLACEHOLDER,
      parallel_test_ref: PLACEHOLDER,
      parallel_test_sha256: PLACEHOLDER,
    },
    failure_retry: {
      resolver_fail_closed: null,
      technical_failure_counted_as_customer_attempt: null,
      bounded_retry: null,
      max_technical_retries: null,
      breaker_enabled: null,
      deadline_enforced: null,
      retry_policy_ref: PLACEHOLDER,
      refresh_route: PLACEHOLDER,
    },
    audit_privacy: {
      ivr_audit_outcome_only: null,
      logs_raw_token: null,
      logs_ciphertext: null,
      logs_destination_handle: null,
      logs_raw_e164: null,
      correlation_joinable: null,
      retention_policy_ref: PLACEHOLDER,
      retention_policy_sha256: PLACEHOLDER,
      purge_proof_ref: PLACEHOLDER,
      purge_proof_sha256: PLACEHOLDER,
    },
    telephony_safety: {
      capability_ref: PLACEHOLDER,
      capability_sha256: PLACEHOLDER,
      recording_disabled: null,
      allowlist_enforced: null,
      kill_switch_available: null,
      caller_id_policy_ref: PLACEHOLDER,
      disposition_matrix_ref: PLACEHOLDER,
      dtmf_contract_ref: PLACEHOLDER,
    },
    cutover_rollback: {
      shared_e2e_plan_ref: PLACEHOLDER,
      shared_e2e_plan_sha256: PLACEHOLDER,
      contract_before_runtime: null,
      sandbox_required: null,
      allowlisted_lab_required: null,
      pilot_required: null,
      rollback_ref: PLACEHOLDER,
      rollback_sha256: PLACEHOLDER,
      kill_switch_default_on: null,
      production_real_enabled: false,
      exact_candidate_binding: null,
    },
  };
  return {
    schema_version: SCHEMA_VERSION,
    work_id: WORK_ID,
    status: "CONTACT_DIAL_TOKEN_DECISIONS_NOT_RECEIVED",
    source: { ...SOURCE_PINS },
    candidate: pendingObject(CANDIDATE_KEYS),
    bundle,
    decision_coverage: DECISION_RULES.map((rule) => ({
      decision_id: rule.decision_id,
      topic: rule.topic,
      state: "PENDING",
      selection_ref: PLACEHOLDER,
      decision_ref: PLACEHOLDER,
      decision_sha256: PLACEHOLDER,
      assertions_passed: [...rule.assertions],
    })),
    external_evidence: Object.fromEntries(EXTERNAL_EVIDENCE_KEYS.map((key) => [key, pendingObject(ARTIFACT_KEYS)])),
    validation_matrix: CASE_RULES.map((rule) => ({
      case_id: rule.case_id,
      scenario: rule.scenario,
      state: "PENDING_TEST_PLAN",
      m8_commit_sha: PLACEHOLDER,
      m3_commit_sha: PLACEHOLDER,
      test_owner_alias: PLACEHOLDER,
      test_spec_ref: PLACEHOLDER,
      test_spec_sha256: PLACEHOLDER,
      expected_outcome: rule.outcome,
      expected_customer_attempt_counted: rule.counted,
      real_customer_call_required: false,
      assertions_covered: [...rule.assertions],
    })),
    matrix_summary: {
      required_cases: CASE_RULES.length,
      approved_test_plans: 0,
      pending_cases: CASE_RULES.length,
      complete_matrix: false,
      all_cases_same_candidate: false,
      green_cases_selected_only: false,
      runtime_execution_claimed: false,
    },
    signoffs: REQUIRED_SIGNOFF_ROLES.map((role) => ({
      role,
      signer_alias: PLACEHOLDER,
      verifier_alias: PLACEHOLDER,
      authority_ref: PLACEHOLDER,
      authority_sha256: PLACEHOLDER,
      decision: "PENDING",
      signed_at: PLACEHOLDER,
      m8_commit_sha: PLACEHOLDER,
      m3_commit_sha: PLACEHOLDER,
      bundle_sha256: PLACEHOLDER,
    })),
    safety: Object.fromEntries(SAFETY_KEYS.map((key) => [key, false])),
  };
}

function validatePendingTemplate(document) {
  validateLocalSources(document.source);
  const expected = makePendingTemplate();
  if (canonicalize(document) !== canonicalize(expected)) fail("pending template shape or placeholder values drifted");
  assertNoSensitiveScalars(document);
  return document;
}

function makeFixture() {
  const expected = {
    m8_commit_sha: "1".repeat(40),
    m3_commit_sha: "2".repeat(40),
    bundle_sha256: "0".repeat(64),
    m3_contact_sha256: hashLabel("M3-CONTACT"),
    issuer_token_sha256: hashLabel("ISSUER-TOKEN"),
    security_threat_sha256: hashLabel("SECURITY-THREAT"),
    platform_custody_sha256: hashLabel("PLATFORM-CUSTODY"),
    telephony_capability_sha256: hashLabel("TELEPHONY-CAPABILITY"),
    privacy_retention_sha256: hashLabel("PRIVACY-RETENTION"),
    shared_e2e_plan_sha256: hashLabel("SHARED-E2E-PLAN"),
    release_packet_sha256: hashLabel("RELEASE-PACKET"),
  };
  const report = makePendingTemplate();
  report.status = "CONTACT_DIAL_TOKEN_DECISIONS_COMPLETE";
  report.candidate = {
    m8_repo_ref: "M8-REPO",
    m8_commit_sha: expected.m8_commit_sha,
    m3_repo_ref: "M3-REPO",
    m3_commit_sha: expected.m3_commit_sha,
    environment_id: "CONTRACT-REVIEW-01",
    config_version: "CONFIG-REVIEW-01",
    run_started_at: "2026-09-04T01:00:00Z",
    run_completed_at: "2026-09-04T01:10:00Z",
  };
  report.decision_coverage = DECISION_RULES.map((rule, index) => ({
    decision_id: rule.decision_id,
    topic: rule.topic,
    state: "APPROVED",
    selection_ref: `SELECTION-${String(index + 1).padStart(2, "0")}`,
    decision_ref: `DECISION-${String(index + 1).padStart(2, "0")}`,
    decision_sha256: hashLabel(`DECISION-${index + 1}`),
    assertions_passed: [...rule.assertions],
  }));
  report.bundle = {
    bundle_id: "DIAL-TOKEN-PRODUCTION-BUNDLE-01",
    contract_version: "DTK-CONTRACT-V1",
    bundle_sha256: "0".repeat(64),
    decision_coverage_sha256: sha256(Buffer.from(canonicalize(report.decision_coverage), "utf8")),
    issued_at: "2026-09-04T00:00:00Z",
    effective_at: "2026-09-04T00:30:00Z",
    contact_contract: {
      authority_alias: "M3-OFFICIAL-CONTACT",
      producer_ref: "M3-CONTACT-PRODUCER-V1",
      phone_validation_contract: "REQUIRED_VALID_ONLY",
      invalid_contact_action: "DO_NOT_ISSUE_TASK",
      raw_e164_in_task: false,
    },
    token_model: {
      model: "SCALAR_REUSABLE_WITHIN_TTL_PER_ATTEMPT_AUTH",
      requires_wire_change: false,
      reissue_contract_ref: "NONE",
      token_opaque: true,
      token_contains_pii: false,
      same_attempt_behavior: "IDEMPOTENT_SAME_AUTHORIZATION",
      different_attempt_behavior: "ALLOWED_WITH_ATTEMPT_BINDING",
    },
    ttl_policy: {
      coverage: "EXACT_WINDOW_END",
      clock_skew_seconds: 30,
      not_before_required: true,
      mid_window_expiry_action: "CREATE_NEW_TASK",
      cross_field_invariant_ref: "TTL-INVARIANT-V1",
    },
    trust_boundary: {
      ivr_sees_raw_e164: false,
      ivr_stores_mapping_key: false,
      resolver_output_to_ivr: "OPAQUE_PROVIDER_REFERENCE",
      raw_e164_location: "EXTERNAL_VAULT_GATEWAY_ONLY",
      vendor_destination_mode: "E164_INSIDE_TRUSTED_GATEWAY",
      diagram_ref: "TRUST-DIAGRAM-V1",
      diagram_sha256: hashLabel("TRUST-DIAGRAM"),
    },
    resolve_protocol: {
      spec_ref: "RESOLVE-PROTOCOL-V1",
      spec_sha256: hashLabel("RESOLVE-PROTOCOL"),
      protocol_version: "RESOLVER-V1",
      auth_mode: "MTLS_AND_WORKLOAD_JWT",
      audience: "DIAL-TOKEN-RESOLVER",
      scope: "DIAL-TOKEN-RESOLVE",
      timeout_milliseconds: 1000,
      idempotency_contract_ref: "RESOLVE-IDEMPOTENCY-V1",
      error_taxonomy_ref: "RESOLVE-ERRORS-V1",
      sandbox_conformance_ref: "SANDBOX-CONFORMANCE-PLAN-V1",
    },
    custody: {
      secret_store_alias: "PLATFORM-SECRET-STORE",
      workload_identity_alias: "IVR-WORKER-IDENTITY",
      mapping_or_decryption_keys_in_ivr: false,
      least_privilege_enforced: true,
      rotation_overlap_seconds: 3600,
      emergency_revoke_supported: true,
      access_review_ref: "ACCESS-REVIEW-V1",
      access_review_sha256: hashLabel("ACCESS-REVIEW"),
    },
    replay_concurrency: {
      atomic_consumption: true,
      cross_task_denied: true,
      cross_environment_denied: true,
      cross_provider_denied: true,
      same_attempt_behavior: "IDEMPOTENT_SAME_AUTHORIZATION",
      different_attempt_behavior: "ALLOWED_WITH_ATTEMPT_BINDING",
      parallel_test_ref: "PARALLEL-REPLAY-PLAN-V1",
      parallel_test_sha256: hashLabel("PARALLEL-REPLAY-PLAN"),
    },
    failure_retry: {
      resolver_fail_closed: true,
      technical_failure_counted_as_customer_attempt: false,
      bounded_retry: true,
      max_technical_retries: 2,
      breaker_enabled: true,
      deadline_enforced: true,
      retry_policy_ref: "RESOLVER-RETRY-V1",
      refresh_route: "CREATE_NEW_TASK",
    },
    audit_privacy: {
      ivr_audit_outcome_only: true,
      logs_raw_token: false,
      logs_ciphertext: false,
      logs_destination_handle: false,
      logs_raw_e164: false,
      correlation_joinable: true,
      retention_policy_ref: "RETENTION-POLICY-V1",
      retention_policy_sha256: hashLabel("RETENTION-POLICY"),
      purge_proof_ref: "PURGE-PROOF-PLAN-V1",
      purge_proof_sha256: hashLabel("PURGE-PROOF-PLAN"),
    },
    telephony_safety: {
      capability_ref: "VENDOR-CAPABILITY-V1",
      capability_sha256: hashLabel("VENDOR-CAPABILITY"),
      recording_disabled: true,
      allowlist_enforced: true,
      kill_switch_available: true,
      caller_id_policy_ref: "CALLER-ID-POLICY-V1",
      disposition_matrix_ref: "DISPOSITION-MATRIX-V1",
      dtmf_contract_ref: "DTMF-CONTRACT-V1",
    },
    cutover_rollback: {
      shared_e2e_plan_ref: "SHARED-E2E-PLAN-V1",
      shared_e2e_plan_sha256: expected.shared_e2e_plan_sha256,
      contract_before_runtime: true,
      sandbox_required: true,
      allowlisted_lab_required: true,
      pilot_required: true,
      rollback_ref: "ROLLBACK-PLAN-V1",
      rollback_sha256: hashLabel("ROLLBACK-PLAN"),
      kill_switch_default_on: true,
      production_real_enabled: false,
      exact_candidate_binding: true,
    },
  };
  report.bundle.bundle_sha256 = canonicalBundleHash(report.bundle);
  expected.bundle_sha256 = report.bundle.bundle_sha256;
  report.external_evidence = Object.fromEntries(EXTERNAL_EVIDENCE_KEYS.map((name, index) => [name, {
    artifact_ref: `EXTERNAL-ARTIFACT-${index + 1}`,
    sha256: expected[EXTERNAL_PIN_KEYS[name]],
    producer_alias: `EXTERNAL-PRODUCER-${index + 1}`,
    produced_at: "2026-09-04T00:40:00Z",
  }]));
  report.validation_matrix = CASE_RULES.map((rule, index) => ({
    case_id: rule.case_id,
    scenario: rule.scenario,
    state: "APPROVED_TEST_PLAN",
    m8_commit_sha: expected.m8_commit_sha,
    m3_commit_sha: expected.m3_commit_sha,
    test_owner_alias: `TEST-OWNER-${index + 1}`,
    test_spec_ref: `TEST-SPEC-${index + 1}`,
    test_spec_sha256: hashLabel(`TEST-SPEC-${index + 1}`),
    expected_outcome: rule.outcome,
    expected_customer_attempt_counted: rule.counted,
    real_customer_call_required: false,
    assertions_covered: [...rule.assertions],
  }));
  report.matrix_summary = {
    required_cases: CASE_RULES.length,
    approved_test_plans: CASE_RULES.length,
    pending_cases: 0,
    complete_matrix: true,
    all_cases_same_candidate: true,
    green_cases_selected_only: false,
    runtime_execution_claimed: false,
  };
  report.signoffs = REQUIRED_SIGNOFF_ROLES.map((role, index) => ({
    role,
    signer_alias: `SIGNER-${index + 1}`,
    verifier_alias: `INDEPENDENT-VERIFIER-${index + 1}`,
    authority_ref: `AUTHORITY-${index + 1}`,
    authority_sha256: hashLabel(`AUTHORITY-${index + 1}`),
    decision: "APPROVED_FOR_IMPLEMENTATION_REVIEW",
    signed_at: "2026-09-04T00:50:00Z",
    m8_commit_sha: expected.m8_commit_sha,
    m3_commit_sha: expected.m3_commit_sha,
    bundle_sha256: report.bundle.bundle_sha256,
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
  const fixture = makeFixture();
  validateDialTokenProductionBundle(fixture.report, fixture.expected);
  validatePendingTemplate(makePendingTemplate());

  const positiveModels = [
    ["SCALAR_REUSABLE_WITHIN_TTL_PER_ATTEMPT_AUTH", false, "NONE", "ALLOWED_WITH_ATTEMPT_BINDING", "CREATE_NEW_TASK", "CREATE_NEW_TASK"],
    ["PER_ATTEMPT_TOKEN_ARRAY", true, "NONE", "DISTINCT_TOKEN_REQUIRED", "CREATE_NEW_TASK", "CREATE_NEW_TASK"],
    ["REISSUE_ENDPOINT", true, "REISSUE-CONTRACT-V1", "REISSUE_REQUIRED", "REISSUE_VIA_SIGNED_CONTRACT", "SIGNED_REISSUE_ENDPOINT"],
    ["TOKEN_BUNDLE", true, "NONE", "BUNDLE_SLOT_REQUIRED", "CREATE_NEW_TASK", "CREATE_NEW_TASK"],
  ];
  for (const [model, wire, reissueRef, differentAttempt, expiryAction, refreshRoute] of positiveModels) {
    const report = clone(fixture.report);
    const expected = clone(fixture.expected);
    report.bundle.token_model.model = model;
    report.bundle.token_model.requires_wire_change = wire;
    report.bundle.token_model.reissue_contract_ref = reissueRef;
    report.bundle.token_model.different_attempt_behavior = differentAttempt;
    report.bundle.ttl_policy.mid_window_expiry_action = expiryAction;
    report.bundle.replay_concurrency.different_attempt_behavior = differentAttempt;
    report.bundle.failure_retry.refresh_route = refreshRoute;
    report.bundle.bundle_sha256 = canonicalBundleHash(report.bundle);
    expected.bundle_sha256 = report.bundle.bundle_sha256;
    report.signoffs.forEach((entry) => { entry.bundle_sha256 = report.bundle.bundle_sha256; });
    validateDialTokenProductionBundle(report, expected);
  }

  const mutations = [
    ["wrong status", (r) => { r.status = "PRODUCTION_READY"; }],
    ["source drift", (r) => { r.source.requirement_scope = "OTHER"; }],
    ["candidate M8 drift", (r) => { r.candidate.m8_commit_sha = "3".repeat(40); }],
    ["review time reversed", (r) => { r.candidate.run_completed_at = "2026-09-04T00:59:00Z"; }],
    ["contact raw e164", (r) => { r.bundle.contact_contract.raw_e164_in_task = true; }, "bundle"],
    ["contact validation unknown", (r) => { r.bundle.contact_contract.phone_validation_contract = "OPTIONAL"; }, "bundle"],
    ["token opaque false", (r) => { r.bundle.token_model.token_opaque = false; }, "bundle"],
    ["scalar claims wire change", (r) => { r.bundle.token_model.requires_wire_change = true; }, "bundle"],
    ["scalar has reissue ref", (r) => { r.bundle.token_model.reissue_contract_ref = "REISSUE-V1"; }, "bundle"],
    ["token behavior mismatch", (r) => { r.bundle.token_model.different_attempt_behavior = "DISTINCT_TOKEN_REQUIRED"; }, "bundle"],
    ["TTL clock skew too large", (r) => { r.bundle.ttl_policy.clock_skew_seconds = 301; }, "bundle"],
    ["TTL reissue contradiction", (r) => { r.bundle.ttl_policy.mid_window_expiry_action = "REISSUE_VIA_SIGNED_CONTRACT"; }, "bundle"],
    ["IVR sees raw e164", (r) => { r.bundle.trust_boundary.ivr_sees_raw_e164 = true; }, "bundle"],
    ["resolver output not opaque", (r) => { r.bundle.trust_boundary.resolver_output_to_ivr = "RAW-E164"; }, "bundle"],
    ["protocol timeout zero", (r) => { r.bundle.resolve_protocol.timeout_milliseconds = 0; }, "bundle"],
    ["unsupported auth", (r) => { r.bundle.resolve_protocol.auth_mode = "STATIC-SECRET"; }, "bundle"],
    ["mapping key in IVR", (r) => { r.bundle.custody.mapping_or_decryption_keys_in_ivr = true; }, "bundle"],
    ["least privilege false", (r) => { r.bundle.custody.least_privilege_enforced = false; }, "bundle"],
    ["rotation overlap excessive", (r) => { r.bundle.custody.rotation_overlap_seconds = 86401; }, "bundle"],
    ["atomic consumption false", (r) => { r.bundle.replay_concurrency.atomic_consumption = false; }, "bundle"],
    ["cross provider allowed", (r) => { r.bundle.replay_concurrency.cross_provider_denied = false; }, "bundle"],
    ["replay rule mismatch", (r) => { r.bundle.replay_concurrency.same_attempt_behavior = "DETERMINISTIC_REPLAY_REJECT"; }, "bundle"],
    ["resolver not fail closed", (r) => { r.bundle.failure_retry.resolver_fail_closed = false; }, "bundle"],
    ["technical failure counted", (r) => { r.bundle.failure_retry.technical_failure_counted_as_customer_attempt = true; }, "bundle"],
    ["unbounded retry", (r) => { r.bundle.failure_retry.bounded_retry = false; }, "bundle"],
    ["refresh route contradiction", (r) => { r.bundle.failure_retry.refresh_route = "SIGNED_REISSUE_ENDPOINT"; }, "bundle"],
    ["raw token logged", (r) => { r.bundle.audit_privacy.logs_raw_token = true; }, "bundle"],
    ["destination handle logged", (r) => { r.bundle.audit_privacy.logs_destination_handle = true; }, "bundle"],
    ["recording enabled", (r) => { r.bundle.telephony_safety.recording_disabled = false; }, "bundle"],
    ["allowlist absent", (r) => { r.bundle.telephony_safety.allowlist_enforced = false; }, "bundle"],
    ["sandbox not required", (r) => { r.bundle.cutover_rollback.sandbox_required = false; }, "bundle"],
    ["production enabled", (r) => { r.bundle.cutover_rollback.production_real_enabled = true; }, "bundle"],
    ["decision removed", (r) => { r.decision_coverage.pop(); }, "decision"],
    ["decision reordered", (r) => { [r.decision_coverage[2], r.decision_coverage[3]] = [r.decision_coverage[3], r.decision_coverage[2]]; }, "decision"],
    ["decision pending", (r) => { r.decision_coverage[2].state = "PENDING"; }, "decision"],
    ["decision assertion missing", (r) => { r.decision_coverage[10].assertions_passed.pop(); }, "decision"],
    ["external artifact missing", (r) => { delete r.external_evidence.security_threat_model; }],
    ["external artifact pin drift", (r) => { r.external_evidence.security_threat_model.sha256 = "4".repeat(64); }],
    ["external artifact late", (r) => { r.external_evidence.security_threat_model.produced_at = "2026-09-04T01:01:00Z"; }],
    ["test plan removed", (r) => { r.validation_matrix.pop(); }],
    ["test plan reordered", (r) => { [r.validation_matrix[5], r.validation_matrix[6]] = [r.validation_matrix[6], r.validation_matrix[5]]; }],
    ["test plan pending", (r) => { r.validation_matrix[8].state = "PENDING_TEST_PLAN"; }],
    ["test plan candidate drift", (r) => { r.validation_matrix[8].m3_commit_sha = "5".repeat(40); }],
    ["test plan wrong outcome", (r) => { r.validation_matrix[8].expected_outcome = "RETRY_FOREVER"; }],
    ["test plan counts customer attempt", (r) => { r.validation_matrix[8].expected_customer_attempt_counted = true; }],
    ["test plan requires real call", (r) => { r.validation_matrix[13].real_customer_call_required = true; }],
    ["test plan assertion missing", (r) => { r.validation_matrix[10].assertions_covered.pop(); }],
    ["green plans only", (r) => { r.matrix_summary.green_cases_selected_only = true; }],
    ["runtime execution claimed", (r) => { r.matrix_summary.runtime_execution_claimed = true; }],
    ["signoff wrong role", (r) => { r.signoffs[3].role = "OTHER"; }],
    ["signer reused", (r) => { r.signoffs[1].signer_alias = r.signoffs[0].signer_alias; }],
    ["signer equals verifier", (r) => { r.signoffs[0].verifier_alias = r.signoffs[0].signer_alias; }],
    ["verifier is another signer", (r) => { r.signoffs[0].verifier_alias = r.signoffs[1].signer_alias; }],
    ["signoff after review start", (r) => { r.signoffs[0].signed_at = "2026-09-04T01:01:00Z"; }],
    ["signoff bundle mismatch", (r) => { r.signoffs[0].bundle_sha256 = "6".repeat(64); }],
    ["unsafe adapter flag", (r) => { r.safety.vault_resolver_or_adapter_added = true; }],
    ["unsafe real call flag", (r) => { r.safety.real_customer_call_allowed = true; }],
    ["email-like alias", (r) => { r.signoffs[0].authority_ref = ["USER", "EXAMPLE.COM"].join("@"); }],
    ["credential-like ref", (r) => { r.external_evidence.issuer_token_spec_and_cdc.artifact_ref = ["BEARER", "SECRET"].join(" "); }],
    ["unknown root key", (r) => { r.unexpected = false; }],
  ];

  for (const [label, mutate, rehash] of mutations) {
    const report = clone(fixture.report);
    const expected = clone(fixture.expected);
    mutate(report);
    if (rehash === "decision") {
      report.bundle.decision_coverage_sha256 = sha256(Buffer.from(canonicalize(report.decision_coverage), "utf8"));
    }
    if (rehash === "bundle" || rehash === "decision") {
      report.bundle.bundle_sha256 = canonicalBundleHash(report.bundle);
      expected.bundle_sha256 = report.bundle.bundle_sha256;
      report.signoffs.forEach((entry) => { entry.bundle_sha256 = report.bundle.bundle_sha256; });
    }
    expectRefusal(label, () => validateDialTokenProductionBundle(report, expected));
  }

  const wrongPins = clone(fixture.expected);
  wrongPins.bundle_sha256 = "9".repeat(64);
  expectRefusal("wrong independent bundle pin", () => validateDialTokenProductionBundle(fixture.report, wrongPins));

  const artifactsRoot = resolve(REPOSITORY_ROOT, "ci-artifacts");
  mkdirSync(artifactsRoot, { recursive: true });
  const tempRoot = mkdtempSync(resolve(artifactsRoot, "w0183-selftest-"));
  if (!isConfined(tempRoot)) fail("self-test temp path escaped repository");
  try {
    const validPath = resolve(tempRoot, "valid.json");
    writeFileSync(validPath, `${JSON.stringify(fixture.report)}\n`, "utf8");
    validateDialTokenProductionBundle(readStrictJson(validPath), fixture.expected);

    const duplicatePath = resolve(tempRoot, "duplicate.json");
    writeFileSync(duplicatePath, '{"schema_version":"a","schema_version":"b"}\n', "utf8");
    expectRefusal("duplicate JSON key", () => readStrictJson(duplicatePath));

    const oversizedPath = resolve(tempRoot, "oversized.json");
    writeFileSync(oversizedPath, "x".repeat(MAX_INPUT_BYTES + 1), "utf8");
    expectRefusal("oversized input", () => readStrictJson(oversizedPath));

    expectRefusal("outside repository", () => readStrictJson(resolve(REPOSITORY_ROOT, "..", "w0183-outside.json")));
  } finally {
    rmSync(tempRoot, { recursive: true, force: true });
  }

  process.stdout.write(`W0183_SELFTEST_PASS template=1 valid_models=${positiveModels.length} refusals=${mutations.length + 4}\n`);
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
    "  node deploy/ci/scripts/dial-token-production-bundle-validator.mjs --self-test",
    "  node deploy/ci/scripts/dial-token-production-bundle-validator.mjs --print-template",
    "  node deploy/ci/scripts/dial-token-production-bundle-validator.mjs --check-template <path>",
    "  node deploy/ci/scripts/dial-token-production-bundle-validator.mjs --input <path> \\",
    "    --m8-commit-sha <40hex> --m3-commit-sha <40hex> --bundle-sha256 <64hex> \\",
    "    --m3-contact-sha256 <64hex> --issuer-token-sha256 <64hex> \\",
    "    --security-threat-sha256 <64hex> --platform-custody-sha256 <64hex> \\",
    "    --telephony-capability-sha256 <64hex> --privacy-retention-sha256 <64hex> \\",
    "    --shared-e2e-plan-sha256 <64hex> --release-packet-sha256 <64hex>",
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
    validatePendingTemplate(readStrictJson(args.template));
    process.stdout.write(`DIAL_TOKEN_TEMPLATE_VALID_NOT_READY decisions=15 test_plans=14 production_authorized=false\n`);
    return;
  }
  if (args.input) {
    const expected = {
      m8_commit_sha: args.m8_commit_sha,
      m3_commit_sha: args.m3_commit_sha,
      bundle_sha256: args.bundle_sha256,
      m3_contact_sha256: args.m3_contact_sha256,
      issuer_token_sha256: args.issuer_token_sha256,
      security_threat_sha256: args.security_threat_sha256,
      platform_custody_sha256: args.platform_custody_sha256,
      telephony_capability_sha256: args.telephony_capability_sha256,
      privacy_retention_sha256: args.privacy_retention_sha256,
      shared_e2e_plan_sha256: args.shared_e2e_plan_sha256,
      release_packet_sha256: args.release_packet_sha256,
    };
    validateDialTokenProductionBundle(readStrictJson(args.input), expected);
    process.stdout.write(
      `DIAL_TOKEN_DECISION_BUNDLE_VALID_ELIGIBLE_FOR_IMPLEMENTATION_REVIEW_ONLY decisions=15 ` +
      `test_plans=14 m8=${expected.m8_commit_sha} m3=${expected.m3_commit_sha} production_authorized=false\n`,
    );
    return;
  }
  fail(usage());
}

try {
  main();
} catch (error) {
  process.stderr.write(`W0183_VALIDATION_FAILED: ${error.message}\n`);
  process.exitCode = 1;
}
