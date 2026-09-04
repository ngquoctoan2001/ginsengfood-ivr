#!/usr/bin/env node

// W-0174 — Offline validator for the Target V1 shared-E2E evidence report.
//
// This CLI validates metadata, hashes, provenance, sign-offs and complete case
// coverage. It never contacts M3, reads credentials or raw request/response
// payloads, changes a database, enables callback delivery or authorizes a real
// customer call.

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
const SCHEMA_VERSION = "target-v1-shared-e2e-report.v1";
const WORK_ID = "W-0174";
const PLACEHOLDER = "PENDING_EXTERNAL_EVIDENCE";

const SOURCE_PINS = Object.freeze({
  m8_target_oas_path: "specs/api/openapi/order-core-ivr-callback.target-v1.yaml",
  m8_target_oas_sha256: "af0cb5cc3f47aaa4c8e232418c216b228fd996e316fe129a7cbf1d4636659697",
  matrix_contract: "M8-07-SECTION-6.2026-09-04",
});

const CASE_RULES = Object.freeze([
  {
    case_id: "TV1-E2E-01-GOLDEN-HOUR-ACCEPTED",
    outcome: "ACCEPTED",
    http: [200],
    assertions: [
      "generic_target_v1_endpoint_used",
      "m3_received_exact_body_and_headers",
      "m3_revalidated_order_state",
      "ack_identity_matched_request",
      "one_decision_effect",
    ],
  },
  {
    case_id: "TV1-E2E-02-24X7-ACCEPTED",
    outcome: "ACCEPTED",
    http: [200],
    assertions: [
      "generic_target_v1_endpoint_used",
      "golden_hour_compat_endpoint_not_used",
      "m3_revalidated_order_state",
      "ack_identity_matched_request",
      "one_decision_effect",
    ],
  },
  {
    case_id: "TV1-E2E-03-EXACT-REPLAY",
    outcome: "DUPLICATE_ACCEPTED",
    http: [409],
    assertions: [
      "same_idempotency_key",
      "same_immutable_body_hash",
      "ack_identity_matched_request",
      "one_decision_effect",
      "no_duplicate_state_transition",
    ],
  },
  {
    case_id: "TV1-E2E-04-CHANGED-BODY-REPLAY",
    outcome: "IDEMPOTENCY_CONFLICT",
    http: [409],
    assertions: [
      "same_idempotency_key",
      "different_body_hash",
      "no_state_transition",
      "no_retry_with_mutated_payload",
    ],
  },
  {
    case_id: "TV1-E2E-05-STALE-VERSION-STATE",
    outcome: "REJECTED_STALE",
    http: [409],
    assertions: [
      "m3_revalidated_order_state",
      "no_state_transition",
      "m8_did_not_retry",
      "terminal_review_recorded",
    ],
  },
  {
    case_id: "TV1-E2E-06-CORE-BLOCKER",
    outcome: "BLOCKED_BY_CORE_OR_REVIEW_REQUIRED",
    http: [200, 409],
    assertions: [
      "m3_revalidated_order_state",
      "order_truth_remained_with_m3",
      "m8_did_not_override_core",
      "no_unauthorized_state_transition",
    ],
  },
  {
    case_id: "TV1-E2E-07-AUTH-NEGATIVE",
    outcome: "AUTH_REJECTED",
    http: [401, 403],
    assertions: [
      "invalid_identity_or_scope_rejected",
      "no_callback_body_processed",
      "m8_did_not_retry_blindly",
      "terminal_review_recorded",
    ],
  },
  {
    case_id: "TV1-E2E-08-INVALID-SCHEMA-RESULT",
    outcome: "INVALID_DEAD_LETTER",
    http: [422],
    assertions: [
      "invalid_schema_or_result_rejected",
      "no_state_transition",
      "m8_did_not_retry",
      "dead_letter_or_review_recorded",
    ],
  },
  {
    case_id: "TV1-E2E-09-RATE-LIMIT",
    outcome: "RETRY_PENDING",
    http: [429],
    assertions: [
      "retry_after_observed",
      "next_retry_not_before_retry_after",
      "same_idempotency_key",
      "same_immutable_body_hash",
      "no_duplicate_state_transition",
    ],
  },
  {
    case_id: "TV1-E2E-10-M3-OUTAGE-TIMEOUT",
    outcome: "RECOVERED_AFTER_BOUNDED_RETRY",
    http: [null, 502, 503, 504],
    assertions: [
      "bounded_retry_budget_enforced",
      "circuit_open_half_open_recover_observed",
      "same_idempotency_key",
      "same_immutable_body_hash",
      "no_callback_lost_or_duplicated",
    ],
  },
  {
    case_id: "TV1-E2E-11-NO-ANSWER-FINAL",
    outcome: "CORE_NO_STATE_CHANGE_WAIT_FOR_TIMEOUT",
    http: [200],
    assertions: [
      "advisory_ack_observed",
      "m8_did_not_cancel_order",
      "m3_order_state_unchanged",
      "timeout_owner_remained_m3",
    ],
  },
]);

const STATE_MUST_REMAIN_UNCHANGED = new Set([
  "TV1-E2E-03-EXACT-REPLAY",
  "TV1-E2E-04-CHANGED-BODY-REPLAY",
  "TV1-E2E-05-STALE-VERSION-STATE",
  "TV1-E2E-07-AUTH-NEGATIVE",
  "TV1-E2E-08-INVALID-SCHEMA-RESULT",
  "TV1-E2E-09-RATE-LIMIT",
  "TV1-E2E-11-NO-ANSWER-FINAL",
]);

const REQUIRED_SIGNOFF_ROLES = Object.freeze([
  "M8_OWNER",
  "M3_OWNER",
  "SECURITY",
  "PLATFORM",
  "RELEASE_OWNER",
]);

const ROOT_KEYS = [
  "schema_version",
  "work_id",
  "status",
  "source",
  "candidate",
  "external_evidence",
  "callback_matrix",
  "matrix_summary",
  "signoffs",
  "safety",
];
const CANDIDATE_KEYS = [
  "m8_repo_ref",
  "m8_commit_sha",
  "m3_repo_ref",
  "m3_commit_sha",
  "environment_id",
  "config_version",
  "run_started_at",
  "run_completed_at",
];
const EXTERNAL_EVIDENCE_KEYS = [
  "m3_authoritative_oas",
  "m3_consumer_and_cdc",
  "security_auth_and_custody",
  "platform_sandbox_network_tls",
];
const ARTIFACT_KEYS = ["artifact_ref", "sha256", "producer_alias", "produced_at"];
const CASE_KEYS = [
  "case_id",
  "status",
  "m8_commit_sha",
  "m3_commit_sha",
  "environment_id",
  "config_version",
  "started_at",
  "completed_at",
  "callback_id_alias",
  "idempotency_key_fingerprint_sha256",
  "request_metadata_ref",
  "request_metadata_sha256",
  "response_metadata_ref",
  "response_metadata_sha256",
  "state_before_sha256",
  "state_after_sha256",
  "observed_http_status",
  "observed_outcome",
  "assertions",
];
const SUMMARY_KEYS = [
  "required_cases",
  "passed_cases",
  "failed_cases",
  "pending_cases",
  "complete_matrix",
  "same_candidate_environment_config",
  "selected_green_cases_only",
];
const SIGNOFF_KEYS = [
  "role",
  "signer_alias",
  "verifier_alias",
  "authority_ref",
  "authority_sha256",
  "decision",
  "reviewed_at",
  "m8_commit_sha",
  "m3_commit_sha",
  "environment_id",
  "config_version",
];
const SAFETY_KEYS = [
  "raw_request_or_response_embedded",
  "contains_credentials_or_secrets",
  "contains_personal_data",
  "local_mock_claimed_as_shared_e2e",
  "delivery_guard_removed",
  "production_enabled",
  "real_customer_call_allowed",
  "report_authorizes_guard_removal",
];

function fail(message) {
  throw new Error(message);
}

function sha256(bytes) {
  return createHash("sha256").update(bytes).digest("hex");
}

function assertExactKeys(value, expected, label) {
  if (!value || typeof value !== "object" || Array.isArray(value)) {
    fail(`${label} must be an object`);
  }
  const actual = Object.keys(value).sort();
  const wanted = [...expected].sort();
  if (actual.length !== wanted.length || actual.some((key, index) => key !== wanted[index])) {
    fail(`${label} must contain exactly: ${wanted.join(", ")}`);
  }
}

function isConfined(pathValue) {
  const relativePath = relative(REPOSITORY_ROOT, pathValue);
  return relativePath !== "" && !relativePath.startsWith("..") && !isAbsolute(relativePath);
}

function readConfinedFile(inputPath, maximumBytes = MAX_INPUT_BYTES) {
  const resolved = resolve(REPOSITORY_ROOT, inputPath);
  if (!isConfined(resolved)) fail(`path is outside repository root: ${inputPath}`);
  const stat = lstatSync(resolved);
  if (!stat.isFile() || stat.isSymbolicLink()) {
    fail(`path must be a regular non-symlink file: ${inputPath}`);
  }
  if (!isConfined(realpathSync(resolved))) fail(`real path escapes repository root: ${inputPath}`);
  if (stat.size > maximumBytes) fail(`file exceeds ${maximumBytes} bytes: ${inputPath}`);
  return { resolved, bytes: readFileSync(resolved) };
}

function readStrictJson(inputPath) {
  const { resolved, bytes } = readConfinedFile(inputPath);
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

function assertNoSensitiveValue(value, label) {
  if (/[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}/iu.test(value)) {
    fail(`${label} contains an email-like value`);
  }
  if (/(?:^|\D)(?:\+?\d[\s().-]*){9,15}(?:$|\D)/u.test(value)) {
    fail(`${label} contains a phone-like value`);
  }
  if (/\b\d{1,5}\s+(?:\u0111\u01b0\u1eddng|\x64uong|ph\u1ed1|pho|street|st\.?|road|rd\.?|avenue|ave\.?)\b/iu.test(value)) {
    fail(`${label} contains a street-address-like value`);
  }
  if (/(?:password|passwd|bearer\s+|api[_ -]?key|access[_ -]?token|private[_ -]?key|client[_ -]?secret)\s*[:=]?/iu.test(value)) {
    fail(`${label} contains credential- or secret-like material`);
  }
  if (/[?#]/u.test(value)) fail(`${label} must not contain a URL query or fragment`);
}

function assertIdentifier(value, label) {
  assertString(value, label, 3, 180);
  if (!/^[A-Z0-9][A-Z0-9._:/-]+$/u.test(value)) {
    fail(`${label} must be an uppercase alias/reference`);
  }
  assertNoSensitiveValue(value, label);
}

function assertGitSha(value, label) {
  if (typeof value !== "string" || !/^[0-9a-f]{40}$/u.test(value)) {
    fail(`${label} must be an exact lowercase 40-character Git SHA`);
  }
}

function assertSha256(value, label) {
  if (typeof value !== "string" || !/^[0-9a-f]{64}$/u.test(value)) {
    fail(`${label} must be lowercase SHA-256`);
  }
}

function assertTimestamp(value, label) {
  assertString(value, label, 20, 35);
  if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,3})?(?:Z|[+-]\d{2}:\d{2})$/u.test(value)) {
    fail(`${label} must be ISO-8601 with an explicit timezone`);
  }
  if (!Number.isFinite(Date.parse(value))) fail(`${label} is not a valid timestamp`);
}

function assertFalseSafety(safety) {
  assertExactKeys(safety, SAFETY_KEYS, "safety");
  for (const key of SAFETY_KEYS) {
    if (safety[key] !== false) fail(`safety.${key} must be false`);
  }
}

function verifySourcePins(source) {
  assertExactKeys(source, Object.keys(SOURCE_PINS), "source");
  for (const [key, expected] of Object.entries(SOURCE_PINS)) {
    if (source[key] !== expected) fail(`source.${key} does not match the pinned value`);
  }
  const { bytes } = readConfinedFile(source.m8_target_oas_path, 5 * 1024 * 1024);
  if (sha256(bytes) !== source.m8_target_oas_sha256) {
    fail(`${source.m8_target_oas_path} drifted from its pinned SHA-256`);
  }
}

function assertArtifact(artifact, label) {
  assertExactKeys(artifact, ARTIFACT_KEYS, label);
  assertIdentifier(artifact.artifact_ref, `${label}.artifact_ref`);
  assertSha256(artifact.sha256, `${label}.sha256`);
  assertIdentifier(artifact.producer_alias, `${label}.producer_alias`);
  assertTimestamp(artifact.produced_at, `${label}.produced_at`);
}

function validateSharedE2EEvidence(externalEvidence, expected, candidate) {
  assertExactKeys(externalEvidence, EXTERNAL_EVIDENCE_KEYS, "external_evidence");
  for (const key of EXTERNAL_EVIDENCE_KEYS) {
    assertArtifact(externalEvidence[key], `external_evidence.${key}`);
    if (Date.parse(externalEvidence[key].produced_at) > Date.parse(candidate.run_started_at)) {
      fail(`external_evidence.${key}.produced_at must not follow the shared-E2E run start`);
    }
  }
  const bindings = [
    ["m3_authoritative_oas", "expectedM3OasSha"],
    ["m3_consumer_and_cdc", "expectedConsumerCdcSha"],
    ["security_auth_and_custody", "expectedAuthSha"],
    ["platform_sandbox_network_tls", "expectedPlatformSha"],
  ];
  for (const [artifactKey, expectedKey] of bindings) {
    if (externalEvidence[artifactKey].sha256 !== expected[expectedKey]) {
      fail(`external_evidence.${artifactKey}.sha256 does not match independently supplied hash`);
    }
  }
}

function validateCandidate(candidate, expected) {
  assertExactKeys(candidate, CANDIDATE_KEYS, "candidate");
  assertIdentifier(candidate.m8_repo_ref, "candidate.m8_repo_ref");
  assertIdentifier(candidate.m3_repo_ref, "candidate.m3_repo_ref");
  assertGitSha(candidate.m8_commit_sha, "candidate.m8_commit_sha");
  assertGitSha(candidate.m3_commit_sha, "candidate.m3_commit_sha");
  assertIdentifier(candidate.environment_id, "candidate.environment_id");
  assertIdentifier(candidate.config_version, "candidate.config_version");
  assertTimestamp(candidate.run_started_at, "candidate.run_started_at");
  assertTimestamp(candidate.run_completed_at, "candidate.run_completed_at");
  if (Date.parse(candidate.run_completed_at) < Date.parse(candidate.run_started_at)) {
    fail("candidate.run_completed_at must not precede run_started_at");
  }
  if (candidate.m8_commit_sha !== expected.expectedM8Sha) {
    fail("candidate.m8_commit_sha does not match independently supplied SHA");
  }
  if (candidate.m3_commit_sha !== expected.expectedM3Sha) {
    fail("candidate.m3_commit_sha does not match independently supplied SHA");
  }
}

function validateSharedE2ECaseMatrix(cases, candidate) {
  if (!Array.isArray(cases) || cases.length !== CASE_RULES.length) {
    fail(`callback_matrix must contain exactly ${CASE_RULES.length} cases`);
  }
  cases.forEach((testCase, index) => {
    const rule = CASE_RULES[index];
    const label = `callback_matrix[${index}]`;
    assertExactKeys(testCase, CASE_KEYS, label);
    if (testCase.case_id !== rule.case_id) fail(`${label}.case_id must be ${rule.case_id}`);
    if (testCase.status !== "PASS") fail(`${label}.status must be PASS`);
    assertGitSha(testCase.m8_commit_sha, `${label}.m8_commit_sha`);
    assertGitSha(testCase.m3_commit_sha, `${label}.m3_commit_sha`);
    for (const key of ["m8_commit_sha", "m3_commit_sha", "environment_id", "config_version"]) {
      if (testCase[key] !== candidate[key]) fail(`${label}.${key} must match candidate.${key}`);
    }
    assertTimestamp(testCase.started_at, `${label}.started_at`);
    assertTimestamp(testCase.completed_at, `${label}.completed_at`);
    const startedAt = Date.parse(testCase.started_at);
    const completedAt = Date.parse(testCase.completed_at);
    if (completedAt < startedAt) fail(`${label}.completed_at must not precede started_at`);
    if (startedAt < Date.parse(candidate.run_started_at) || completedAt > Date.parse(candidate.run_completed_at)) {
      fail(`${label} timestamps must be inside the candidate run window`);
    }
    assertIdentifier(testCase.callback_id_alias, `${label}.callback_id_alias`);
    assertSha256(testCase.idempotency_key_fingerprint_sha256, `${label}.idempotency_key_fingerprint_sha256`);
    assertIdentifier(testCase.request_metadata_ref, `${label}.request_metadata_ref`);
    assertSha256(testCase.request_metadata_sha256, `${label}.request_metadata_sha256`);
    assertIdentifier(testCase.response_metadata_ref, `${label}.response_metadata_ref`);
    assertSha256(testCase.response_metadata_sha256, `${label}.response_metadata_sha256`);
    assertSha256(testCase.state_before_sha256, `${label}.state_before_sha256`);
    assertSha256(testCase.state_after_sha256, `${label}.state_after_sha256`);
    if (
      STATE_MUST_REMAIN_UNCHANGED.has(rule.case_id) &&
      testCase.state_before_sha256 !== testCase.state_after_sha256
    ) {
      fail(`${label} must prove unchanged state with identical before/after hashes`);
    }
    if (!rule.http.includes(testCase.observed_http_status)) {
      fail(`${label}.observed_http_status is not allowed for ${rule.case_id}`);
    }
    if (testCase.observed_outcome !== rule.outcome) {
      fail(`${label}.observed_outcome must be ${rule.outcome}`);
    }
    assertExactKeys(testCase.assertions, rule.assertions, `${label}.assertions`);
    for (const assertion of rule.assertions) {
      if (testCase.assertions[assertion] !== true) {
        fail(`${label}.assertions.${assertion} must be true`);
      }
    }
  });
}

function validateSummary(summary) {
  assertExactKeys(summary, SUMMARY_KEYS, "matrix_summary");
  const expected = {
    required_cases: CASE_RULES.length,
    passed_cases: CASE_RULES.length,
    failed_cases: 0,
    pending_cases: 0,
    complete_matrix: true,
    same_candidate_environment_config: true,
    selected_green_cases_only: false,
  };
  for (const [key, value] of Object.entries(expected)) {
    if (summary[key] !== value) fail(`matrix_summary.${key} must be ${value}`);
  }
}

function validateSignoffs(signoffs, candidate) {
  if (!Array.isArray(signoffs) || signoffs.length !== REQUIRED_SIGNOFF_ROLES.length) {
    fail(`signoffs must contain exactly ${REQUIRED_SIGNOFF_ROLES.length} records`);
  }
  const signerAliases = new Set();
  const authorityReferences = new Set();
  const authorityHashes = new Set();
  signoffs.forEach((signoff, index) => {
    const role = REQUIRED_SIGNOFF_ROLES[index];
    const label = `signoffs[${index}]`;
    assertExactKeys(signoff, SIGNOFF_KEYS, label);
    if (signoff.role !== role) fail(`${label}.role must be ${role}`);
    assertIdentifier(signoff.signer_alias, `${label}.signer_alias`);
    assertIdentifier(signoff.verifier_alias, `${label}.verifier_alias`);
    if (signoff.signer_alias === signoff.verifier_alias) {
      fail(`${label} signer_alias and verifier_alias must be separated`);
    }
    if (signerAliases.has(signoff.signer_alias)) {
      fail(`${label}.signer_alias must be unique across authority roles`);
    }
    signerAliases.add(signoff.signer_alias);
    assertIdentifier(signoff.authority_ref, `${label}.authority_ref`);
    assertSha256(signoff.authority_sha256, `${label}.authority_sha256`);
    if (authorityReferences.has(signoff.authority_ref) || authorityHashes.has(signoff.authority_sha256)) {
      fail(`${label} must use role-specific authority evidence`);
    }
    authorityReferences.add(signoff.authority_ref);
    authorityHashes.add(signoff.authority_sha256);
    if (signoff.decision !== "ACCEPTED_FOR_GUARD_REVIEW_ONLY") {
      fail(`${label}.decision must be ACCEPTED_FOR_GUARD_REVIEW_ONLY`);
    }
    assertTimestamp(signoff.reviewed_at, `${label}.reviewed_at`);
    if (Date.parse(signoff.reviewed_at) < Date.parse(candidate.run_completed_at)) {
      fail(`${label}.reviewed_at must not precede the completed shared-E2E run`);
    }
    for (const key of ["m8_commit_sha", "m3_commit_sha", "environment_id", "config_version"]) {
      if (signoff[key] !== candidate[key]) fail(`${label}.${key} must match candidate.${key}`);
    }
  });
  signoffs.forEach((signoff, index) => {
    if (signerAliases.has(signoff.verifier_alias)) {
      fail(`signoffs[${index}].verifier_alias must not sign another authority role`);
    }
  });
}

function validateExpectedPins(expected) {
  assertGitSha(expected.expectedM8Sha, "--expected-m8-sha");
  assertGitSha(expected.expectedM3Sha, "--expected-m3-sha");
  assertSha256(expected.expectedM3OasSha, "--expected-m3-oas-sha");
  assertSha256(expected.expectedConsumerCdcSha, "--expected-consumer-cdc-sha");
  assertSha256(expected.expectedAuthSha, "--expected-auth-sha");
  assertSha256(expected.expectedPlatformSha, "--expected-platform-sha");
}

function validateSharedE2EReport(document, expected) {
  validateExpectedPins(expected);
  assertExactKeys(document, ROOT_KEYS, "root");
  if (document.schema_version !== SCHEMA_VERSION) fail(`schema_version must be ${SCHEMA_VERSION}`);
  if (document.work_id !== WORK_ID) fail(`work_id must be ${WORK_ID}`);
  if (document.status !== "SHARED_E2E_EVIDENCE_COMPLETE") {
    fail("status must be SHARED_E2E_EVIDENCE_COMPLETE");
  }
  verifySourcePins(document.source);
  validateCandidate(document.candidate, expected);
  validateSharedE2EEvidence(document.external_evidence, expected, document.candidate);
  validateSharedE2ECaseMatrix(document.callback_matrix, document.candidate);
  validateSummary(document.matrix_summary);
  validateSignoffs(document.signoffs, document.candidate);
  assertFalseSafety(document.safety);
  return { caseCount: document.callback_matrix.length };
}

function assertPlaceholder(value, label) {
  if (value !== PLACEHOLDER) fail(`${label} must remain ${PLACEHOLDER}`);
}

function validatePendingTemplate(document) {
  assertExactKeys(document, ROOT_KEYS, "root");
  if (document.schema_version !== SCHEMA_VERSION) fail(`schema_version must be ${SCHEMA_VERSION}`);
  if (document.work_id !== WORK_ID) fail(`work_id must be ${WORK_ID}`);
  if (document.status !== "EXTERNAL_E2E_NOT_RUN") fail("template status must be EXTERNAL_E2E_NOT_RUN");
  verifySourcePins(document.source);
  assertExactKeys(document.candidate, CANDIDATE_KEYS, "candidate");
  for (const key of CANDIDATE_KEYS) assertPlaceholder(document.candidate[key], `candidate.${key}`);
  assertExactKeys(document.external_evidence, EXTERNAL_EVIDENCE_KEYS, "external_evidence");
  for (const key of EXTERNAL_EVIDENCE_KEYS) {
    assertExactKeys(document.external_evidence[key], ARTIFACT_KEYS, `external_evidence.${key}`);
    for (const artifactKey of ARTIFACT_KEYS) {
      assertPlaceholder(
        document.external_evidence[key][artifactKey],
        `external_evidence.${key}.${artifactKey}`,
      );
    }
  }
  if (!Array.isArray(document.callback_matrix) || document.callback_matrix.length !== CASE_RULES.length) {
    fail(`template callback_matrix must contain exactly ${CASE_RULES.length} cases`);
  }
  document.callback_matrix.forEach((testCase, index) => {
    const rule = CASE_RULES[index];
    const label = `callback_matrix[${index}]`;
    assertExactKeys(testCase, CASE_KEYS, label);
    if (testCase.case_id !== rule.case_id) fail(`${label}.case_id must be ${rule.case_id}`);
    if (testCase.status !== "PENDING_EXTERNAL_E2E") {
      fail(`${label}.status must be PENDING_EXTERNAL_E2E`);
    }
    for (const key of [
      "m8_commit_sha",
      "m3_commit_sha",
      "environment_id",
      "config_version",
      "started_at",
      "completed_at",
      "callback_id_alias",
      "idempotency_key_fingerprint_sha256",
      "request_metadata_ref",
      "request_metadata_sha256",
      "response_metadata_ref",
      "response_metadata_sha256",
      "state_before_sha256",
      "state_after_sha256",
      "observed_outcome",
    ]) {
      assertPlaceholder(testCase[key], `${label}.${key}`);
    }
    if (testCase.observed_http_status !== null) {
      fail(`${label}.observed_http_status must be null while pending`);
    }
    assertExactKeys(testCase.assertions, rule.assertions, `${label}.assertions`);
    for (const assertion of rule.assertions) {
      if (testCase.assertions[assertion] !== false) {
        fail(`${label}.assertions.${assertion} must be false while pending`);
      }
    }
  });
  assertExactKeys(document.matrix_summary, SUMMARY_KEYS, "matrix_summary");
  const pendingSummary = {
    required_cases: CASE_RULES.length,
    passed_cases: 0,
    failed_cases: 0,
    pending_cases: CASE_RULES.length,
    complete_matrix: false,
    same_candidate_environment_config: false,
    selected_green_cases_only: false,
  };
  for (const [key, value] of Object.entries(pendingSummary)) {
    if (document.matrix_summary[key] !== value) fail(`matrix_summary.${key} must be ${value}`);
  }
  if (!Array.isArray(document.signoffs) || document.signoffs.length !== REQUIRED_SIGNOFF_ROLES.length) {
    fail(`template signoffs must contain exactly ${REQUIRED_SIGNOFF_ROLES.length} records`);
  }
  document.signoffs.forEach((signoff, index) => {
    const label = `signoffs[${index}]`;
    assertExactKeys(signoff, SIGNOFF_KEYS, label);
    if (signoff.role !== REQUIRED_SIGNOFF_ROLES[index]) {
      fail(`${label}.role must be ${REQUIRED_SIGNOFF_ROLES[index]}`);
    }
    for (const key of SIGNOFF_KEYS.filter((key) => key !== "role")) {
      assertPlaceholder(signoff[key], `${label}.${key}`);
    }
  });
  assertFalseSafety(document.safety);
  return { caseCount: document.callback_matrix.length };
}

function validateFile(inputPath, mode, expected) {
  const { bytes, document } = readStrictJson(inputPath);
  const result = mode === "template"
    ? validatePendingTemplate(document)
    : validateSharedE2EReport(document, expected);
  return { ...result, inputSha256: sha256(bytes) };
}

function fixtureHash(label) {
  return sha256(Buffer.from(`W-0174-SYNTHETIC-${label}`, "utf8"));
}

function fixtureExpected() {
  return {
    expectedM8Sha: "1".repeat(40),
    expectedM3Sha: "2".repeat(40),
    expectedM3OasSha: "3".repeat(64),
    expectedConsumerCdcSha: "4".repeat(64),
    expectedAuthSha: "5".repeat(64),
    expectedPlatformSha: "6".repeat(64),
  };
}

function validFixture(expected = fixtureExpected()) {
  const candidate = {
    m8_repo_ref: "REPO:M8/GINSENGFOOD-IVR",
    m8_commit_sha: expected.expectedM8Sha,
    m3_repo_ref: "REPO:M3/ORDER-CORE",
    m3_commit_sha: expected.expectedM3Sha,
    environment_id: "SANDBOX:SHARED-E2E-01",
    config_version: "CONFIG:TARGET-V1-01",
    run_started_at: "2026-09-04T10:00:00+07:00",
    run_completed_at: "2026-09-04T12:00:00+07:00",
  };
  const evidence = (artifactRef, artifactSha, producerAlias) => ({
    artifact_ref: artifactRef,
    sha256: artifactSha,
    producer_alias: producerAlias,
    produced_at: "2026-09-04T09:00:00+07:00",
  });
  const callbackMatrix = CASE_RULES.map((rule, index) => ({
    case_id: rule.case_id,
    status: "PASS",
    m8_commit_sha: candidate.m8_commit_sha,
    m3_commit_sha: candidate.m3_commit_sha,
    environment_id: candidate.environment_id,
    config_version: candidate.config_version,
    started_at: `2026-09-04T10:${String(index * 2).padStart(2, "0")}:00+07:00`,
    completed_at: `2026-09-04T10:${String(index * 2 + 1).padStart(2, "0")}:00+07:00`,
    callback_id_alias: `CALLBACK:SYNTHETIC-${String(index + 1).padStart(2, "0")}`,
    idempotency_key_fingerprint_sha256: fixtureHash(`IDEMPOTENCY-${index}`),
    request_metadata_ref: `EVIDENCE:CASE-${String(index + 1).padStart(2, "0")}/REQUEST-METADATA`,
    request_metadata_sha256: fixtureHash(`REQUEST-${index}`),
    response_metadata_ref: `EVIDENCE:CASE-${String(index + 1).padStart(2, "0")}/RESPONSE-METADATA`,
    response_metadata_sha256: fixtureHash(`RESPONSE-${index}`),
    state_before_sha256: fixtureHash(`STATE-BEFORE-${index}`),
    state_after_sha256: STATE_MUST_REMAIN_UNCHANGED.has(rule.case_id)
      ? fixtureHash(`STATE-BEFORE-${index}`)
      : fixtureHash(`STATE-AFTER-${index}`),
    observed_http_status: rule.http[0],
    observed_outcome: rule.outcome,
    assertions: Object.fromEntries(rule.assertions.map((assertion) => [assertion, true])),
  }));
  return {
    schema_version: SCHEMA_VERSION,
    work_id: WORK_ID,
    status: "SHARED_E2E_EVIDENCE_COMPLETE",
    source: { ...SOURCE_PINS },
    candidate,
    external_evidence: {
      m3_authoritative_oas: evidence("EVIDENCE:M3/AUTHORITATIVE-OAS", expected.expectedM3OasSha, "M3_OWNER"),
      m3_consumer_and_cdc: evidence("EVIDENCE:M3/CONSUMER-CDC", expected.expectedConsumerCdcSha, "M3_OWNER"),
      security_auth_and_custody: evidence("EVIDENCE:SECURITY/AUTH-CUSTODY", expected.expectedAuthSha, "SECURITY_OWNER"),
      platform_sandbox_network_tls: evidence("EVIDENCE:PLATFORM/SANDBOX-NETWORK-TLS", expected.expectedPlatformSha, "PLATFORM_OWNER"),
    },
    callback_matrix: callbackMatrix,
    matrix_summary: {
      required_cases: CASE_RULES.length,
      passed_cases: CASE_RULES.length,
      failed_cases: 0,
      pending_cases: 0,
      complete_matrix: true,
      same_candidate_environment_config: true,
      selected_green_cases_only: false,
    },
    signoffs: REQUIRED_SIGNOFF_ROLES.map((role, index) => ({
      role,
      signer_alias: `${role}_SIGNER`,
      verifier_alias: "CHIEF_AUDITOR",
      authority_ref: `AUTHORITY:${role}/SHARED-E2E-01`,
      authority_sha256: fixtureHash(`AUTHORITY-${index}`),
      decision: "ACCEPTED_FOR_GUARD_REVIEW_ONLY",
      reviewed_at: "2026-09-04T13:00:00+07:00",
      m8_commit_sha: candidate.m8_commit_sha,
      m3_commit_sha: candidate.m3_commit_sha,
      environment_id: candidate.environment_id,
      config_version: candidate.config_version,
    })),
    safety: Object.fromEntries(SAFETY_KEYS.map((key) => [key, false])),
  };
}

function clone(value) {
  return JSON.parse(JSON.stringify(value));
}

function expectFailure(base, expected, mutation, label) {
  const candidate = clone(base);
  mutation(candidate);
  let failed = false;
  try {
    validateSharedE2EReport(candidate, expected);
  } catch {
    failed = true;
  }
  if (!failed) fail(`self-test mutation was accepted: ${label}`);
}

function runSelfTest() {
  const expected = fixtureExpected();
  const valid = validFixture(expected);
  const result = validateSharedE2EReport(valid, expected);
  if (result.caseCount !== CASE_RULES.length) fail("positive self-test did not validate all cases");

  const mutations = [
    ["root-extra-key", (v) => (v.unexpected = true)],
    ["status", (v) => (v.status = "EXTERNAL_E2E_NOT_RUN")],
    ["m8-sha", (v) => (v.candidate.m8_commit_sha = "7".repeat(40))],
    ["m3-sha", (v) => (v.candidate.m3_commit_sha = "8".repeat(40))],
    ["source-oas-hash", (v) => (v.source.m8_target_oas_sha256 = "9".repeat(64))],
    ["m3-oas-hash", (v) => (v.external_evidence.m3_authoritative_oas.sha256 = "a".repeat(64))],
    ["consumer-cdc-hash", (v) => (v.external_evidence.m3_consumer_and_cdc.sha256 = "b".repeat(64))],
    ["auth-hash", (v) => (v.external_evidence.security_auth_and_custody.sha256 = "c".repeat(64))],
    ["platform-hash", (v) => (v.external_evidence.platform_sandbox_network_tls.sha256 = "d".repeat(64))],
    ["missing-case", (v) => v.callback_matrix.pop()],
    ["duplicate-case", (v) => (v.callback_matrix[1] = clone(v.callback_matrix[0]))],
    ["wrong-case-order", (v) => v.callback_matrix.reverse()],
    ["case-not-pass", (v) => (v.callback_matrix[0].status = "FAIL")],
    ["case-m8-sha", (v) => (v.callback_matrix[0].m8_commit_sha = "7".repeat(40))],
    ["case-m3-sha", (v) => (v.callback_matrix[0].m3_commit_sha = "8".repeat(40))],
    ["case-environment", (v) => (v.callback_matrix[0].environment_id = "SANDBOX:OTHER")],
    ["case-config", (v) => (v.callback_matrix[0].config_version = "CONFIG:OTHER")],
    ["case-outcome", (v) => (v.callback_matrix[0].observed_outcome = "DUPLICATE_ACCEPTED")],
    ["case-http", (v) => (v.callback_matrix[0].observed_http_status = 409)],
    ["case-assertion-missing", (v) => delete v.callback_matrix[0].assertions.one_decision_effect],
    ["case-assertion-false", (v) => (v.callback_matrix[0].assertions.one_decision_effect = false)],
    ["unchanged-state-hash", (v) => (v.callback_matrix[2].state_after_sha256 = "e".repeat(64))],
    ["case-time-outside-run", (v) => (v.callback_matrix[0].started_at = "2026-09-04T09:59:00+07:00")],
    ["case-pii", (v) => (v.callback_matrix[0].callback_id_alias = "PERSON:USER@EXAMPLE.COM")],
    ["case-secret-like", (v) => (v.callback_matrix[0].request_metadata_ref = "EVIDENCE:BEARER TOKEN")],
    ["summary-pass-count", (v) => (v.matrix_summary.passed_cases = 10)],
    ["summary-selected-green", (v) => (v.matrix_summary.selected_green_cases_only = true)],
    ["missing-signoff", (v) => v.signoffs.pop()],
    ["signoff-order", (v) => v.signoffs.reverse()],
    ["same-signer-verifier", (v) => (v.signoffs[0].verifier_alias = v.signoffs[0].signer_alias)],
    ["duplicate-cross-role-signer", (v) => (v.signoffs[1].signer_alias = v.signoffs[0].signer_alias)],
    ["cross-role-signer-verifier", (v) => (v.signoffs[0].verifier_alias = v.signoffs[1].signer_alias)],
    ["signoff-decision", (v) => (v.signoffs[0].decision = "APPROVED_FOR_PRODUCTION")],
    ["signoff-before-run", (v) => (v.signoffs[0].reviewed_at = "2026-09-04T11:00:00+07:00")],
    ["signoff-candidate", (v) => (v.signoffs[0].m3_commit_sha = "8".repeat(40))],
    ["raw-payload", (v) => (v.safety.raw_request_or_response_embedded = true)],
    ["credentials", (v) => (v.safety.contains_credentials_or_secrets = true)],
    ["guard-removed", (v) => (v.safety.delivery_guard_removed = true)],
    ["production-enabled", (v) => (v.safety.production_enabled = true)],
    ["real-call", (v) => (v.safety.real_customer_call_allowed = true)],
    ["authorizes-removal", (v) => (v.safety.report_authorizes_guard_removal = true)],
    [
      "future-external-evidence",
      (v) => (v.external_evidence.platform_sandbox_network_tls.produced_at = "2026-09-04T10:01:00+07:00"),
    ],
  ];
  mutations.forEach(([label, mutation]) => expectFailure(valid, expected, mutation, label));

  const wrongExpected = { ...expected, expectedM8Sha: "f".repeat(40) };
  let wrongPinRejected = false;
  try {
    validateSharedE2EReport(valid, wrongExpected);
  } catch {
    wrongPinRejected = true;
  }
  if (!wrongPinRejected) fail("independently supplied M8 SHA mismatch was accepted");

  const artifactsRoot = resolve(REPOSITORY_ROOT, "ci-artifacts");
  mkdirSync(artifactsRoot, { recursive: true });
  const tempDirectory = mkdtempSync(resolve(artifactsRoot, "w0174-selftest-"));
  try {
    const validPath = resolve(tempDirectory, "valid.json");
    writeFileSync(validPath, `${JSON.stringify(valid, null, 2)}\n`, { encoding: "utf8", flag: "wx" });
    validateFile(relative(REPOSITORY_ROOT, validPath), "input", expected);

    const duplicatePath = resolve(tempDirectory, "duplicate.json");
    writeFileSync(duplicatePath, '{"schema_version":"x","schema_version":"y"}\n', {
      encoding: "utf8",
      flag: "wx",
    });
    let duplicateRejected = false;
    try {
      readStrictJson(relative(REPOSITORY_ROOT, duplicatePath));
    } catch {
      duplicateRejected = true;
    }
    if (!duplicateRejected) fail("duplicate JSON key was accepted");

    const oversizedPath = resolve(tempDirectory, "oversized.json");
    writeFileSync(oversizedPath, Buffer.alloc(MAX_INPUT_BYTES + 1, 0x20), { flag: "wx" });
    let oversizedRejected = false;
    try {
      readStrictJson(relative(REPOSITORY_ROOT, oversizedPath));
    } catch {
      oversizedRejected = true;
    }
    if (!oversizedRejected) fail("oversized input was accepted");

    let outsideRejected = false;
    try {
      readStrictJson(resolve(REPOSITORY_ROOT, "..", "outside-w0174.json"));
    } catch {
      outsideRejected = true;
    }
    if (!outsideRejected) fail("outside-repository path was accepted");
  } finally {
    if (isConfined(tempDirectory) && tempDirectory.startsWith(artifactsRoot)) {
      rmSync(tempDirectory, { recursive: true, force: true });
    }
  }

  return { valid: 1, refusals: mutations.length + 4 };
}

function usage() {
  return [
    "Usage:",
    "  node deploy/ci/scripts/target-v1-shared-e2e-report-validator.mjs --check-template <json>",
    "  node deploy/ci/scripts/target-v1-shared-e2e-report-validator.mjs --input <json> \\",
    "    --expected-m8-sha <40hex> --expected-m3-sha <40hex> \\",
    "    --expected-m3-oas-sha <64hex> --expected-consumer-cdc-sha <64hex> \\",
    "    --expected-auth-sha <64hex> --expected-platform-sha <64hex>",
    "  node deploy/ci/scripts/target-v1-shared-e2e-report-validator.mjs --self-test",
  ].join("\n");
}

function parseInputArgs(argv) {
  if (argv[0] !== "--input" || argv.length < 4 || argv.length % 2 !== 0) fail(usage());
  const flagMap = new Map([
    ["--expected-m8-sha", "expectedM8Sha"],
    ["--expected-m3-sha", "expectedM3Sha"],
    ["--expected-m3-oas-sha", "expectedM3OasSha"],
    ["--expected-consumer-cdc-sha", "expectedConsumerCdcSha"],
    ["--expected-auth-sha", "expectedAuthSha"],
    ["--expected-platform-sha", "expectedPlatformSha"],
  ]);
  const expected = {};
  for (let index = 2; index < argv.length; index += 2) {
    const property = flagMap.get(argv[index]);
    if (!property || expected[property] !== undefined) fail(usage());
    expected[property] = argv[index + 1];
  }
  if (Object.keys(expected).length !== flagMap.size) fail(usage());
  return { inputPath: argv[1], expected };
}

function main(argv) {
  if (argv.length === 1 && argv[0] === "--self-test") {
    const result = runSelfTest();
    console.log(`W0174_SELFTEST_PASS valid=${result.valid} refusals=${result.refusals}`);
    return;
  }
  if (argv.length === 2 && argv[0] === "--check-template") {
    const result = validateFile(argv[1], "template");
    console.log(
      `SHARED_E2E_TEMPLATE_VALID_NOT_READY cases=${result.caseCount} sha256=${result.inputSha256}`,
    );
    return;
  }
  const { inputPath, expected } = parseInputArgs(argv);
  const result = validateFile(inputPath, "input", expected);
  console.log(
    `SHARED_E2E_REPORT_VALID_ELIGIBLE_FOR_GUARD_REVIEW_ONLY cases=${result.caseCount} ` +
      `m8_sha=${expected.expectedM8Sha} m3_sha=${expected.expectedM3Sha} ` +
      `report_sha256=${result.inputSha256} delivery_guard_removed=false`,
  );
}

try {
  main(process.argv.slice(2));
} catch (error) {
  console.error(`W0174_VALIDATION_FAILED: ${error.message}`);
  process.exitCode = 1;
}
