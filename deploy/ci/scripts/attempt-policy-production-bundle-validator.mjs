#!/usr/bin/env node

// W-0180 — Offline, metadata-only validator for a signed production attempt-policy bundle.
//
// The validator never chooses policy numbers, promotes mock-lab-v1, changes a registry or
// scheduler, connects to an external system, or authorizes a real customer call.

import { createHash } from "node:crypto";
import { lstatSync, readFileSync, realpathSync } from "node:fs";
import { dirname, isAbsolute, relative, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const SCRIPT_PATH = fileURLToPath(import.meta.url);
const REPOSITORY_ROOT = resolve(dirname(SCRIPT_PATH), "../../..");
const MAX_INPUT_BYTES = 512 * 1024;
const SCHEMA_VERSION = "m8-attempt-policy-production-bundle.v1";
const WORK_ID = "W-0180";
const PLACEHOLDER = "PENDING_EXTERNAL_DECISION";
const RESERVED_POLICY_VERSIONS = new Set(["mock-lab-v1", "UNAPPROVED", PLACEHOLDER]);

const SOURCE_PINS = Object.freeze({
  audit_evidence_path: "docs/evidence/W-0151/README.md",
  audit_evidence_sha256: "b0134983a9449a8f85db38d90cc62df2ac3413dc7aec465a9148440a394a1eca",
  closure_contract_path: "docs/contracts/target-v1-closure-pack/T-09-attempt-policy.md",
  closure_contract_sha256: "4046e3c1cbeb8d3983da0745f25056968d0960b04b410904369bdb20e987eb11",
  functional_spec_path: "specs/functional/03-scheduler-attempt-policy.md",
  functional_spec_sha256: "aebad839f97f3585c5ca28356d0031e40433857f2902c0a23d1c5447035ee5a9",
  domain_policy_path: "src/Ivr.Domain/Confirmation/AttemptPolicy.cs",
  domain_policy_sha256: "ae072510f5d2d65a4567ec1a3cff2e38abbd15238369d466306d077c51ceae8d",
});

const PROGRAMS = Object.freeze(["GOLDEN_HOUR", "ALWAYS_ON"]);
const DECISION_IDS = Object.freeze(
  Array.from({ length: 15 }, (_, index) => `ATP-${String(index + 1).padStart(2, "0")}`),
);
const REQUIRED_SIGNOFF_ROLES = Object.freeze(["PRODUCT", "ORDER_CORE", "M3_OWNER"]);
const EXTERNAL_EVIDENCE_KEYS = Object.freeze([
  "m3_producer_and_cdc",
  "registry_custody_and_recovery",
  "capacity_and_token_recalibration",
  "shared_e2e",
  "production_release_packet",
]);

const ROOT_KEYS = [
  "schema_version",
  "work_id",
  "status",
  "source",
  "bundle",
  "decision_coverage",
  "external_evidence",
  "signoffs",
  "safety",
];
const BUNDLE_KEYS = [
  "bundle_id",
  "policy_version",
  "bundle_sha256",
  "issued_at",
  "effective_at",
  "retire_previous_at",
  "authority_provenance",
  "programs",
  "timing",
  "counting",
  "technical_retry",
  "temporal_policy",
  "wire_contract",
  "registry_governance",
  "pre_dial_coherence",
  "capacity_and_token",
  "cutover_and_rollback",
  "rollout",
];
const PROGRAM_KEYS = [
  "program",
  "max_customer_attempts",
  "offsets_seconds",
  "attempt_window_seconds",
];
const ARTIFACT_KEYS = ["artifact_ref", "sha256", "producer_alias", "produced_at"];
const DECISION_KEYS = ["decision_id", "state", "decision_ref", "decision_sha256"];
const SIGNOFF_KEYS = [
  "role",
  "signer_alias",
  "verifier_alias",
  "authority_ref",
  "authority_sha256",
  "decision",
  "signed_at",
  "policy_version",
  "bundle_sha256",
];
const SAFETY_KEYS = [
  "contains_raw_rows_or_payload",
  "contains_contact_or_personal_data",
  "contains_credentials_or_secrets",
  "mock_lab_promoted",
  "scheduler_or_registry_changed",
  "production_enabled",
  "real_customer_call_allowed",
  "validator_claims_production_authorization",
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

function assertString(value, label, minimum = 3, maximum = 180) {
  if (typeof value !== "string" || value.length < minimum || value.length > maximum) {
    fail(`${label} must be a string of ${minimum}..${maximum} characters`);
  }
  if (value.trim() !== value) fail(`${label} must not have surrounding whitespace`);
  if (/[\u0000-\u001f\u007f]/u.test(value)) fail(`${label} contains a control character`);
}

function assertIdentifier(value, label) {
  assertString(value, label);
  if (!/^[A-Z0-9][A-Z0-9._:/-]+$/u.test(value)) {
    fail(`${label} must be an uppercase alias/reference`);
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

function assertInteger(value, label, minimum, maximum) {
  if (!Number.isInteger(value) || value < minimum || value > maximum) {
    fail(`${label} must be an integer in ${minimum}..${maximum}`);
  }
}

function isConfined(pathValue) {
  const rel = relative(REPOSITORY_ROOT, pathValue);
  return rel !== "" && !rel.startsWith("..") && !isAbsolute(rel);
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

function rejectDuplicateJsonKeys(textValue) {
  let position = 0;
  const skip = () => {
    while (/\s/u.test(textValue[position] ?? "")) position += 1;
  };
  const parseString = () => {
    if (textValue[position] !== '"') fail("invalid JSON string");
    const start = position++;
    while (position < textValue.length) {
      if (textValue[position] === "\\") {
        position += 2;
      } else if (textValue[position] === '"') {
        position += 1;
        try {
          return JSON.parse(textValue.slice(start, position));
        } catch {
          fail("invalid JSON string escape");
        }
      } else {
        if (textValue.charCodeAt(position) < 0x20) fail("invalid JSON control character");
        position += 1;
      }
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
  const parseValue = () => {
    skip();
    if (textValue[position] === "{") parseObject();
    else if (textValue[position] === "[") parseArray();
    else if (textValue[position] === '"') parseString();
    else parseLiteral();
  };
  const parseArray = () => {
    position += 1;
    skip();
    if (textValue[position] === "]") return void (position += 1);
    while (position < textValue.length) {
      parseValue();
      skip();
      if (textValue[position] === "]") return void (position += 1);
      if (textValue[position] !== ",") fail("invalid JSON array separator");
      position += 1;
    }
    fail("unterminated JSON array");
  };
  const parseObject = () => {
    position += 1;
    const keys = new Set();
    skip();
    if (textValue[position] === "}") return void (position += 1);
    while (position < textValue.length) {
      const key = parseString();
      if (keys.has(key)) fail(`duplicate JSON key: ${key}`);
      keys.add(key);
      skip();
      if (textValue[position] !== ":") fail("invalid JSON object separator");
      position += 1;
      parseValue();
      skip();
      if (textValue[position] === "}") return void (position += 1);
      if (textValue[position] !== ",") fail("invalid JSON object separator");
      position += 1;
      skip();
    }
    fail("unterminated JSON object");
  };
  parseValue();
  skip();
  if (position !== textValue.length) fail("unexpected content after JSON document");
}

function readStrictJson(inputPath) {
  const { bytes } = readConfinedFile(inputPath);
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
  return { bytes, document };
}

function canonicalize(value) {
  if (Array.isArray(value)) return `[${value.map(canonicalize).join(",")}]`;
  if (value && typeof value === "object") {
    return `{${Object.keys(value)
      .sort()
      .map((key) => `${JSON.stringify(key)}:${canonicalize(value[key])}`)
      .join(",")}}`;
  }
  return JSON.stringify(value);
}

function canonicalBundleHash(bundle) {
  const hashable = structuredClone(bundle);
  delete hashable.bundle_sha256;
  return sha256(Buffer.from(canonicalize(hashable), "utf8"));
}

function assertNoSensitiveStrings(value, label = "root") {
  if (typeof value === "string") {
    if (/[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}/iu.test(value)) {
      fail(`${label} contains an email-like value`);
    }
    if (/(?:ref|alias)$/u.test(label) && /(?:^|\D)(?:\+?\d[\s().-]*){9,15}(?:$|\D)/u.test(value)) {
      fail(`${label} contains a phone-like value`);
    }
    if (/(?:password|passwd|bearer\s+|api[_ -]?key|access[_ -]?token|private[_ -]?key|client[_ -]?secret)\s*[:=]?/iu.test(value)) {
      fail(`${label} contains credential- or secret-like material`);
    }
    if (/[?#]/u.test(value)) fail(`${label} must not contain a URL query or fragment`);
  } else if (Array.isArray(value)) {
    value.forEach((item, index) => assertNoSensitiveStrings(item, `${label}[${index}]`));
  } else if (value && typeof value === "object") {
    for (const [key, child] of Object.entries(value)) {
      assertNoSensitiveStrings(child, `${label}.${key}`);
    }
  }
}

function verifySourcePins(source) {
  assertExactKeys(source, Object.keys(SOURCE_PINS), "source");
  for (const [key, expected] of Object.entries(SOURCE_PINS)) {
    if (source[key] !== expected) fail(`source.${key} does not match the pinned value`);
  }
  for (const key of Object.keys(SOURCE_PINS).filter((name) => name.endsWith("_path"))) {
    const hashKey = key.replace(/_path$/u, "_sha256");
    const { bytes } = readConfinedFile(source[key], 5 * 1024 * 1024);
    if (sha256(bytes) !== source[hashKey]) fail(`${source[key]} drifted from its pinned SHA-256`);
  }
}

function validatePrograms(programs) {
  if (!Array.isArray(programs) || programs.length !== PROGRAMS.length) {
    fail(`bundle.programs must contain exactly ${PROGRAMS.length} programs`);
  }
  programs.forEach((program, index) => {
    const label = `bundle.programs[${index}]`;
    assertExactKeys(program, PROGRAM_KEYS, label);
    if (program.program !== PROGRAMS[index]) fail(`${label}.program must be ${PROGRAMS[index]}`);
    assertInteger(program.max_customer_attempts, `${label}.max_customer_attempts`, 1, 10);
    assertInteger(program.attempt_window_seconds, `${label}.attempt_window_seconds`, 1, 604800);
    if (!Array.isArray(program.offsets_seconds) || program.offsets_seconds.length !== program.max_customer_attempts) {
      fail(`${label}.offsets_seconds length must equal max_customer_attempts`);
    }
    program.offsets_seconds.forEach((offset, offsetIndex) => {
      assertInteger(offset, `${label}.offsets_seconds[${offsetIndex}]`, 0, 604799);
      if (offsetIndex === 0 && offset !== 0) fail(`${label}.offsets_seconds must start at zero`);
      if (offsetIndex > 0 && offset <= program.offsets_seconds[offsetIndex - 1]) {
        fail(`${label}.offsets_seconds must be strictly increasing`);
      }
      if (offset >= program.attempt_window_seconds) {
        fail(`${label}.offsets_seconds must remain inside attempt_window_seconds`);
      }
    });
  });
}

function validateBundle(bundle, expected) {
  assertExactKeys(bundle, BUNDLE_KEYS, "bundle");
  assertIdentifier(bundle.bundle_id, "bundle.bundle_id");
  assertString(bundle.policy_version, "bundle.policy_version", 3, 80);
  if (RESERVED_POLICY_VERSIONS.has(bundle.policy_version) || /mock|candidate|unapproved/iu.test(bundle.policy_version)) {
    fail("bundle.policy_version must be a new non-mock production version");
  }
  if (!/^[A-Za-z0-9][A-Za-z0-9._-]+$/u.test(bundle.policy_version)) {
    fail("bundle.policy_version contains unsupported characters");
  }
  assertSha256(bundle.bundle_sha256, "bundle.bundle_sha256");
  assertTimestamp(bundle.issued_at, "bundle.issued_at");
  assertTimestamp(bundle.effective_at, "bundle.effective_at");
  assertTimestamp(bundle.retire_previous_at, "bundle.retire_previous_at");
  if (Date.parse(bundle.effective_at) < Date.parse(bundle.issued_at)) {
    fail("bundle.effective_at must not precede issued_at");
  }
  if (Date.parse(bundle.retire_previous_at) <= Date.parse(bundle.effective_at)) {
    fail("bundle.retire_previous_at must follow effective_at");
  }

  assertExactKeys(bundle.authority_provenance, ["decision_pack_ref", "decision_pack_sha256", "decision_coverage_sha256", "signed_at"], "bundle.authority_provenance");
  assertIdentifier(bundle.authority_provenance.decision_pack_ref, "bundle.authority_provenance.decision_pack_ref");
  assertSha256(bundle.authority_provenance.decision_pack_sha256, "bundle.authority_provenance.decision_pack_sha256");
  assertSha256(bundle.authority_provenance.decision_coverage_sha256, "bundle.authority_provenance.decision_coverage_sha256");
  assertTimestamp(bundle.authority_provenance.signed_at, "bundle.authority_provenance.signed_at");
  if (bundle.authority_provenance.decision_pack_sha256 !== expected.expectedDecisionPackSha) {
    fail("decision pack hash does not match independently supplied hash");
  }

  validatePrograms(bundle.programs);
  assertExactKeys(bundle.timing, ["t0_definition", "clock_skew_seconds"], "bundle.timing");
  assertIdentifier(bundle.timing.t0_definition, "bundle.timing.t0_definition");
  assertInteger(bundle.timing.clock_skew_seconds, "bundle.timing.clock_skew_seconds", 0, 300);

  assertExactKeys(bundle.counting, ["customer_counted_outcomes", "terminal_outcomes", "invalid_input_counted", "technical_failure_counted", "manual_retry_rule"], "bundle.counting");
  for (const key of ["customer_counted_outcomes", "terminal_outcomes"]) {
    if (!Array.isArray(bundle.counting[key]) || bundle.counting[key].length === 0) {
      fail(`bundle.counting.${key} must be a non-empty array`);
    }
    const unique = new Set(bundle.counting[key]);
    if (unique.size !== bundle.counting[key].length) fail(`bundle.counting.${key} must be unique`);
    bundle.counting[key].forEach((entry, index) => assertIdentifier(entry, `bundle.counting.${key}[${index}]`));
  }
  if (bundle.counting.invalid_input_counted !== false || bundle.counting.technical_failure_counted !== false) {
    fail("invalid input and technical failure must remain outside customer-attempt counting");
  }
  assertIdentifier(bundle.counting.manual_retry_rule, "bundle.counting.manual_retry_rule");

  assertExactKeys(bundle.technical_retry, ["config_version", "max_retries_per_customer_attempt", "backoff_schedule_seconds", "retry_window_rule"], "bundle.technical_retry");
  assertIdentifier(bundle.technical_retry.config_version, "bundle.technical_retry.config_version");
  assertInteger(bundle.technical_retry.max_retries_per_customer_attempt, "bundle.technical_retry.max_retries_per_customer_attempt", 0, 10);
  if (!Array.isArray(bundle.technical_retry.backoff_schedule_seconds) || bundle.technical_retry.backoff_schedule_seconds.length !== bundle.technical_retry.max_retries_per_customer_attempt) {
    fail("bundle.technical_retry.backoff_schedule_seconds length must equal max retries");
  }
  bundle.technical_retry.backoff_schedule_seconds.forEach((seconds, index) => {
    assertInteger(seconds, `bundle.technical_retry.backoff_schedule_seconds[${index}]`, 1, 86400);
    if (index > 0 && seconds <= bundle.technical_retry.backoff_schedule_seconds[index - 1]) {
      fail("technical retry backoff must be strictly increasing");
    }
  });
  if (!new Set(["WITHIN_CUSTOMER_ATTEMPT_WINDOW", "NEVER_AFTER_ATTEMPT_WINDOW"]).has(bundle.technical_retry.retry_window_rule)) {
    fail("bundle.technical_retry.retry_window_rule is unsupported");
  }

  assertExactKeys(bundle.temporal_policy, ["timezone", "quiet_hours_policy_ref", "holiday_policy_ref", "window_crossing_rule"], "bundle.temporal_policy");
  assertString(bundle.temporal_policy.timezone, "bundle.temporal_policy.timezone", 3, 80);
  if (!/^[A-Za-z_]+\/[A-Za-z_+-]+$/u.test(bundle.temporal_policy.timezone)) fail("bundle.temporal_policy.timezone must be an IANA-style zone");
  assertIdentifier(bundle.temporal_policy.quiet_hours_policy_ref, "bundle.temporal_policy.quiet_hours_policy_ref");
  assertIdentifier(bundle.temporal_policy.holiday_policy_ref, "bundle.temporal_policy.holiday_policy_ref");
  if (!new Set(["DEFER", "TRUNCATE", "REJECT"]).has(bundle.temporal_policy.window_crossing_rule)) {
    fail("bundle.temporal_policy.window_crossing_rule is unsupported");
  }

  assertExactKeys(bundle.wire_contract, ["authoritative_source", "version_required", "snapshot_required", "mismatch_http_status", "mismatch_code", "producer_artifact_ref", "producer_artifact_sha256", "cdc_artifact_ref", "cdc_artifact_sha256"], "bundle.wire_contract");
  if (bundle.wire_contract.authoritative_source !== "M3") fail("bundle.wire_contract.authoritative_source must be M3");
  if (bundle.wire_contract.version_required !== true || bundle.wire_contract.snapshot_required !== true) {
    fail("wire contract must carry exact version and snapshot");
  }
  if (bundle.wire_contract.mismatch_http_status !== 409 || bundle.wire_contract.mismatch_code !== "IVR_POLICY_MISMATCH") {
    fail("wire mismatch behavior must remain exact 409 IVR_POLICY_MISMATCH");
  }
  assertIdentifier(bundle.wire_contract.producer_artifact_ref, "bundle.wire_contract.producer_artifact_ref");
  assertIdentifier(bundle.wire_contract.cdc_artifact_ref, "bundle.wire_contract.cdc_artifact_ref");
  assertSha256(bundle.wire_contract.producer_artifact_sha256, "bundle.wire_contract.producer_artifact_sha256");
  assertSha256(bundle.wire_contract.cdc_artifact_sha256, "bundle.wire_contract.cdc_artifact_sha256");

  assertExactKeys(bundle.registry_governance, ["controlled_writer_ref", "controlled_reader_ref", "four_eyes", "atomic_bundle_publish", "immutable_versions", "hard_delete_allowed", "effective_retire_rule", "custody_ref", "recovery_ref", "evidence_sha256", "audit_hash_required"], "bundle.registry_governance");
  for (const key of ["controlled_writer_ref", "controlled_reader_ref", "effective_retire_rule", "custody_ref", "recovery_ref"]) {
    assertIdentifier(bundle.registry_governance[key], `bundle.registry_governance.${key}`);
  }
  assertSha256(bundle.registry_governance.evidence_sha256, "bundle.registry_governance.evidence_sha256");
  for (const key of ["four_eyes", "atomic_bundle_publish", "immutable_versions", "audit_hash_required"]) {
    if (bundle.registry_governance[key] !== true) fail(`bundle.registry_governance.${key} must be true`);
  }
  if (bundle.registry_governance.hard_delete_allowed !== false) fail("registry hard delete must be disabled");

  assertExactKeys(bundle.pre_dial_coherence, ["strategy", "consistency_rule_ref", "checked_at", "unknown_policy_behavior", "registry_unavailable_behavior", "drift_behavior"], "bundle.pre_dial_coherence");
  if (!new Set(["EXACT_JOB_FLAG_VERSION_MATCH", "SIGNED_REGISTRY_RESOLVER"]).has(bundle.pre_dial_coherence.strategy)) {
    fail("bundle.pre_dial_coherence.strategy must contain the signed ATP-11 choice");
  }
  assertIdentifier(bundle.pre_dial_coherence.consistency_rule_ref, "bundle.pre_dial_coherence.consistency_rule_ref");
  if (bundle.pre_dial_coherence.checked_at !== "BEFORE_EACH_DIAL") fail("pre-dial coherence must run before each dial");
  for (const key of ["unknown_policy_behavior", "registry_unavailable_behavior", "drift_behavior"]) {
    if (bundle.pre_dial_coherence[key] !== "FAIL_CLOSED") fail(`bundle.pre_dial_coherence.${key} must be FAIL_CLOSED`);
  }

  assertExactKeys(bundle.capacity_and_token, ["model_ref", "evidence_sha256", "recalibrated_for_bundle", "channel_token_policy_ref", "rate_limit_policy_ref"], "bundle.capacity_and_token");
  assertIdentifier(bundle.capacity_and_token.model_ref, "bundle.capacity_and_token.model_ref");
  assertSha256(bundle.capacity_and_token.evidence_sha256, "bundle.capacity_and_token.evidence_sha256");
  if (bundle.capacity_and_token.recalibrated_for_bundle !== true) fail("capacity model must be recalibrated for this bundle");
  assertIdentifier(bundle.capacity_and_token.channel_token_policy_ref, "bundle.capacity_and_token.channel_token_policy_ref");
  assertIdentifier(bundle.capacity_and_token.rate_limit_policy_ref, "bundle.capacity_and_token.rate_limit_policy_ref");

  assertExactKeys(bundle.cutover_and_rollback, ["previous_policy_version", "in_flight_rule", "rollback_policy_version", "rollback_trigger_ref", "canary_plan_ref"], "bundle.cutover_and_rollback");
  assertString(bundle.cutover_and_rollback.previous_policy_version, "bundle.cutover_and_rollback.previous_policy_version", 3, 80);
  if (bundle.cutover_and_rollback.previous_policy_version === bundle.policy_version) fail("previous policy version must differ from the new version");
  if (!new Set(["RETAIN_ACCEPTED_SNAPSHOT", "REVALIDATE_BEFORE_DIAL"]).has(bundle.cutover_and_rollback.in_flight_rule)) {
    fail("bundle.cutover_and_rollback.in_flight_rule is unsupported");
  }
  if (bundle.cutover_and_rollback.rollback_policy_version !== bundle.cutover_and_rollback.previous_policy_version) {
    fail("rollback policy version must equal previous policy version");
  }
  assertIdentifier(bundle.cutover_and_rollback.rollback_trigger_ref, "bundle.cutover_and_rollback.rollback_trigger_ref");
  assertIdentifier(bundle.cutover_and_rollback.canary_plan_ref, "bundle.cutover_and_rollback.canary_plan_ref");

  assertExactKeys(bundle.rollout, ["environments", "shared_e2e_ref", "shared_e2e_sha256", "release_packet_ref", "release_packet_sha256", "production_default_disabled", "real_customer_call_allowed"], "bundle.rollout");
  if (JSON.stringify(bundle.rollout.environments) !== JSON.stringify(["STAGING", "UAT", "PRODUCTION_REAL"])) {
    fail("bundle.rollout.environments must be STAGING, UAT, PRODUCTION_REAL in order");
  }
  assertIdentifier(bundle.rollout.shared_e2e_ref, "bundle.rollout.shared_e2e_ref");
  assertSha256(bundle.rollout.shared_e2e_sha256, "bundle.rollout.shared_e2e_sha256");
  assertIdentifier(bundle.rollout.release_packet_ref, "bundle.rollout.release_packet_ref");
  assertSha256(bundle.rollout.release_packet_sha256, "bundle.rollout.release_packet_sha256");
  if (bundle.rollout.production_default_disabled !== true || bundle.rollout.real_customer_call_allowed !== false) {
    fail("validated bundle must keep production disabled and real customer calls unauthorized");
  }

  const computed = canonicalBundleHash(bundle);
  if (bundle.bundle_sha256 !== computed) fail("bundle.bundle_sha256 does not match canonical bundle bytes");
  if (bundle.bundle_sha256 !== expected.expectedBundleSha) fail("bundle hash does not match independently supplied hash");
}

function validateDecisionCoverage(decisions) {
  if (!Array.isArray(decisions) || decisions.length !== DECISION_IDS.length) {
    fail(`decision_coverage must contain exactly ${DECISION_IDS.length} decisions`);
  }
  const refs = new Set();
  const hashes = new Set();
  decisions.forEach((decision, index) => {
    const label = `decision_coverage[${index}]`;
    assertExactKeys(decision, DECISION_KEYS, label);
    if (decision.decision_id !== DECISION_IDS[index]) fail(`${label}.decision_id must be ${DECISION_IDS[index]}`);
    if (decision.state !== "APPROVED") fail(`${label}.state must be APPROVED`);
    assertIdentifier(decision.decision_ref, `${label}.decision_ref`);
    assertSha256(decision.decision_sha256, `${label}.decision_sha256`);
    if (refs.has(decision.decision_ref) || hashes.has(decision.decision_sha256)) {
      fail(`${label} must use decision-specific ref and hash`);
    }
    refs.add(decision.decision_ref);
    hashes.add(decision.decision_sha256);
  });
  return sha256(Buffer.from(canonicalize(decisions), "utf8"));
}

function validateArtifact(artifact, label) {
  assertExactKeys(artifact, ARTIFACT_KEYS, label);
  assertIdentifier(artifact.artifact_ref, `${label}.artifact_ref`);
  assertSha256(artifact.sha256, `${label}.sha256`);
  assertIdentifier(artifact.producer_alias, `${label}.producer_alias`);
  assertTimestamp(artifact.produced_at, `${label}.produced_at`);
}

function validateExternalEvidence(externalEvidence, bundle, expected) {
  assertExactKeys(externalEvidence, EXTERNAL_EVIDENCE_KEYS, "external_evidence");
  const expectedBindings = {
    m3_producer_and_cdc: expected.expectedM3ProducerSha,
    registry_custody_and_recovery: expected.expectedRegistrySha,
    capacity_and_token_recalibration: expected.expectedCapacitySha,
    shared_e2e: expected.expectedSharedE2ESha,
    production_release_packet: expected.expectedReleasePacketSha,
  };
  for (const key of EXTERNAL_EVIDENCE_KEYS) {
    validateArtifact(externalEvidence[key], `external_evidence.${key}`);
    if (externalEvidence[key].sha256 !== expectedBindings[key]) {
      fail(`external_evidence.${key}.sha256 does not match independently supplied hash`);
    }
  }
  if (bundle.wire_contract.producer_artifact_sha256 !== externalEvidence.m3_producer_and_cdc.sha256 || bundle.wire_contract.cdc_artifact_sha256 !== externalEvidence.m3_producer_and_cdc.sha256) {
    fail("wire producer and CDC hashes must bind to m3_producer_and_cdc evidence");
  }
  if (bundle.capacity_and_token.evidence_sha256 !== externalEvidence.capacity_and_token_recalibration.sha256) {
    fail("capacity hash must bind to capacity evidence");
  }
  if (bundle.registry_governance.evidence_sha256 !== externalEvidence.registry_custody_and_recovery.sha256) {
    fail("registry governance hash must bind to custody and recovery evidence");
  }
  if (bundle.rollout.shared_e2e_sha256 !== externalEvidence.shared_e2e.sha256) {
    fail("rollout shared-E2E hash must bind to external evidence");
  }
  if (bundle.rollout.release_packet_sha256 !== externalEvidence.production_release_packet.sha256) {
    fail("rollout release-packet hash must bind to external evidence");
  }
}

function validateSignoffs(signoffs, bundle) {
  if (!Array.isArray(signoffs) || signoffs.length !== REQUIRED_SIGNOFF_ROLES.length) {
    fail(`signoffs must contain exactly ${REQUIRED_SIGNOFF_ROLES.length} records`);
  }
  const signers = new Set();
  const authorityRefs = new Set();
  const authorityHashes = new Set();
  signoffs.forEach((signoff, index) => {
    const label = `signoffs[${index}]`;
    assertExactKeys(signoff, SIGNOFF_KEYS, label);
    if (signoff.role !== REQUIRED_SIGNOFF_ROLES[index]) fail(`${label}.role must be ${REQUIRED_SIGNOFF_ROLES[index]}`);
    assertIdentifier(signoff.signer_alias, `${label}.signer_alias`);
    assertIdentifier(signoff.verifier_alias, `${label}.verifier_alias`);
    if (signoff.signer_alias === signoff.verifier_alias) fail(`${label} signer and verifier must be separated`);
    if (signers.has(signoff.signer_alias)) fail(`${label}.signer_alias must be unique`);
    signers.add(signoff.signer_alias);
    assertIdentifier(signoff.authority_ref, `${label}.authority_ref`);
    assertSha256(signoff.authority_sha256, `${label}.authority_sha256`);
    if (authorityRefs.has(signoff.authority_ref) || authorityHashes.has(signoff.authority_sha256)) {
      fail(`${label} must use role-specific authority evidence`);
    }
    authorityRefs.add(signoff.authority_ref);
    authorityHashes.add(signoff.authority_sha256);
    if (signoff.decision !== "APPROVED_FOR_RUNTIME_REVIEW_ONLY") {
      fail(`${label}.decision must be APPROVED_FOR_RUNTIME_REVIEW_ONLY`);
    }
    assertTimestamp(signoff.signed_at, `${label}.signed_at`);
    if (signoff.policy_version !== bundle.policy_version || signoff.bundle_sha256 !== bundle.bundle_sha256) {
      fail(`${label} must bind the exact policy version and bundle hash`);
    }
  });
  signoffs.forEach((signoff, index) => {
    if (signers.has(signoff.verifier_alias)) fail(`signoffs[${index}].verifier_alias must not sign another role`);
  });
}

function validateSafety(safety) {
  assertExactKeys(safety, SAFETY_KEYS, "safety");
  for (const key of SAFETY_KEYS) if (safety[key] !== false) fail(`safety.${key} must be false`);
}

function validateExpected(expected) {
  for (const [key, value] of Object.entries(expected)) assertSha256(value, key);
}

function validateAttemptPolicyProductionBundle(document, expected) {
  validateExpected(expected);
  assertExactKeys(document, ROOT_KEYS, "root");
  if (document.schema_version !== SCHEMA_VERSION) fail(`schema_version must be ${SCHEMA_VERSION}`);
  if (document.work_id !== WORK_ID) fail(`work_id must be ${WORK_ID}`);
  if (document.status !== "SIGNED_POLICY_BUNDLE_COMPLETE") fail("status must be SIGNED_POLICY_BUNDLE_COMPLETE");
  verifySourcePins(document.source);
  assertNoSensitiveStrings(document);
  const decisionCoverageSha256 = validateDecisionCoverage(document.decision_coverage);
  if (document.bundle.authority_provenance.decision_coverage_sha256 !== decisionCoverageSha256) {
    fail("bundle authority provenance does not bind the exact ATP-01..ATP-15 decision coverage");
  }
  validateBundle(document.bundle, expected);
  validateExternalEvidence(document.external_evidence, document.bundle, expected);
  validateSignoffs(document.signoffs, document.bundle);
  validateSafety(document.safety);
  return { policyVersion: document.bundle.policy_version, bundleSha256: document.bundle.bundle_sha256 };
}

function assertPlaceholder(value, label) {
  if (value !== PLACEHOLDER) fail(`${label} must remain ${PLACEHOLDER}`);
}

function validatePendingTemplate(document) {
  assertExactKeys(document, ROOT_KEYS, "root");
  if (document.schema_version !== SCHEMA_VERSION || document.work_id !== WORK_ID) fail("template identity is invalid");
  if (document.status !== "OWNER_DECISION_REQUIRED") fail("template status must be OWNER_DECISION_REQUIRED");
  verifySourcePins(document.source);
  assertExactKeys(document.bundle, BUNDLE_KEYS, "bundle");
  for (const key of BUNDLE_KEYS.filter((key) => !["programs", "counting", "technical_retry", "rollout"].includes(key))) {
    if (typeof document.bundle[key] === "object" && document.bundle[key] !== null) {
      for (const [childKey, childValue] of Object.entries(document.bundle[key])) {
        assertPlaceholder(childValue, `bundle.${key}.${childKey}`);
      }
    } else {
      assertPlaceholder(document.bundle[key], `bundle.${key}`);
    }
  }
  if (!Array.isArray(document.bundle.programs) || document.bundle.programs.length !== PROGRAMS.length) fail("template must contain two programs");
  document.bundle.programs.forEach((program, index) => {
    assertExactKeys(program, PROGRAM_KEYS, `bundle.programs[${index}]`);
    if (program.program !== PROGRAMS[index]) fail(`template program ${index} is invalid`);
    assertPlaceholder(program.max_customer_attempts, `bundle.programs[${index}].max_customer_attempts`);
    assertPlaceholder(program.attempt_window_seconds, `bundle.programs[${index}].attempt_window_seconds`);
    if (!Array.isArray(program.offsets_seconds) || program.offsets_seconds.length !== 0) fail("template offsets must remain empty");
  });
  assertExactKeys(document.bundle.counting, ["customer_counted_outcomes", "terminal_outcomes", "invalid_input_counted", "technical_failure_counted", "manual_retry_rule"], "bundle.counting");
  for (const key of ["customer_counted_outcomes", "terminal_outcomes"]) if (!Array.isArray(document.bundle.counting[key]) || document.bundle.counting[key].length !== 0) fail(`template ${key} must remain empty`);
  for (const key of ["invalid_input_counted", "technical_failure_counted", "manual_retry_rule"]) assertPlaceholder(document.bundle.counting[key], `bundle.counting.${key}`);
  assertExactKeys(document.bundle.technical_retry, ["config_version", "max_retries_per_customer_attempt", "backoff_schedule_seconds", "retry_window_rule"], "bundle.technical_retry");
  for (const key of ["config_version", "max_retries_per_customer_attempt", "retry_window_rule"]) assertPlaceholder(document.bundle.technical_retry[key], `bundle.technical_retry.${key}`);
  if (!Array.isArray(document.bundle.technical_retry.backoff_schedule_seconds) || document.bundle.technical_retry.backoff_schedule_seconds.length !== 0) fail("template retry backoff must remain empty");
  assertExactKeys(document.bundle.rollout, ["environments", "shared_e2e_ref", "shared_e2e_sha256", "release_packet_ref", "release_packet_sha256", "production_default_disabled", "real_customer_call_allowed"], "bundle.rollout");
  if (!Array.isArray(document.bundle.rollout.environments) || document.bundle.rollout.environments.length !== 0) fail("template rollout environments must remain empty");
  for (const key of ["shared_e2e_ref", "shared_e2e_sha256", "release_packet_ref", "release_packet_sha256", "production_default_disabled", "real_customer_call_allowed"]) assertPlaceholder(document.bundle.rollout[key], `bundle.rollout.${key}`);

  if (!Array.isArray(document.decision_coverage) || document.decision_coverage.length !== DECISION_IDS.length) fail("template must contain ATP-01..ATP-15");
  document.decision_coverage.forEach((decision, index) => {
    assertExactKeys(decision, DECISION_KEYS, `decision_coverage[${index}]`);
    if (decision.decision_id !== DECISION_IDS[index] || decision.state !== "PENDING") fail(`template decision ${index} is invalid`);
    assertPlaceholder(decision.decision_ref, `decision_coverage[${index}].decision_ref`);
    assertPlaceholder(decision.decision_sha256, `decision_coverage[${index}].decision_sha256`);
  });
  assertExactKeys(document.external_evidence, EXTERNAL_EVIDENCE_KEYS, "external_evidence");
  for (const key of EXTERNAL_EVIDENCE_KEYS) {
    assertExactKeys(document.external_evidence[key], ARTIFACT_KEYS, `external_evidence.${key}`);
    for (const artifactKey of ARTIFACT_KEYS) assertPlaceholder(document.external_evidence[key][artifactKey], `external_evidence.${key}.${artifactKey}`);
  }
  if (!Array.isArray(document.signoffs) || document.signoffs.length !== REQUIRED_SIGNOFF_ROLES.length) fail("template must contain three signoffs");
  document.signoffs.forEach((signoff, index) => {
    assertExactKeys(signoff, SIGNOFF_KEYS, `signoffs[${index}]`);
    if (signoff.role !== REQUIRED_SIGNOFF_ROLES[index] || signoff.decision !== "PENDING") fail(`template signoff ${index} is invalid`);
    for (const key of SIGNOFF_KEYS.filter((key) => !["role", "decision"].includes(key))) assertPlaceholder(signoff[key], `signoffs[${index}].${key}`);
  });
  validateSafety(document.safety);
  return { decisionCount: document.decision_coverage.length };
}

function fixtureHash(label) {
  return sha256(Buffer.from(`W0180:${label}`, "utf8"));
}

function fixtureExpected() {
  return {
    expectedBundleSha: "0".repeat(64),
    expectedDecisionPackSha: fixtureHash("decision-pack"),
    expectedM3ProducerSha: fixtureHash("m3-producer"),
    expectedRegistrySha: fixtureHash("registry"),
    expectedCapacitySha: fixtureHash("capacity"),
    expectedSharedE2ESha: fixtureHash("e2e"),
    expectedReleasePacketSha: fixtureHash("release"),
  };
}

function validFixture() {
  const expected = fixtureExpected();
  const decisionCoverage = DECISION_IDS.map((decisionId, index) => ({ decision_id: decisionId, state: "APPROVED", decision_ref: `DECISION/${decisionId}/V1`, decision_sha256: fixtureHash(`decision-${index}`) }));
  const artifact = (name, producer) => ({
    artifact_ref: `ARTIFACT/${name.toUpperCase()}/V1`,
    sha256: expected[name],
    producer_alias: producer,
    produced_at: "2026-09-04T01:00:00Z",
  });
  const bundle = {
    bundle_id: "ATTEMPT-POLICY/BUNDLE/2026-09-04/V1",
    policy_version: "prod-2026-09-04-v1",
    bundle_sha256: "0".repeat(64),
    issued_at: "2026-09-04T02:00:00Z",
    effective_at: "2026-09-05T00:00:00Z",
    retire_previous_at: "2026-09-06T00:00:00Z",
    authority_provenance: { decision_pack_ref: "DECISION/ATP/2026-09-04/V1", decision_pack_sha256: expected.expectedDecisionPackSha, decision_coverage_sha256: sha256(Buffer.from(canonicalize(decisionCoverage), "utf8")), signed_at: "2026-09-04T01:30:00Z" },
    programs: [
      { program: "GOLDEN_HOUR", max_customer_attempts: 2, offsets_seconds: [0, 300], attempt_window_seconds: 900 },
      { program: "ALWAYS_ON", max_customer_attempts: 3, offsets_seconds: [0, 300, 600], attempt_window_seconds: 1200 },
    ],
    timing: { t0_definition: "TASK/ACCEPTED-AT", clock_skew_seconds: 30 },
    counting: { customer_counted_outcomes: ["NO_ANSWER", "BUSY"], terminal_outcomes: ["CONFIRMED", "REJECTED", "NO_ANSWER_FINAL"], invalid_input_counted: false, technical_failure_counted: false, manual_retry_rule: "POLICY/MANUAL-RETRY/V1" },
    technical_retry: { config_version: "TECH-RETRY/V1", max_retries_per_customer_attempt: 2, backoff_schedule_seconds: [30, 120], retry_window_rule: "WITHIN_CUSTOMER_ATTEMPT_WINDOW" },
    temporal_policy: { timezone: "Asia/Ho_Chi_Minh", quiet_hours_policy_ref: "POLICY/QUIET-HOURS/V1", holiday_policy_ref: "POLICY/HOLIDAYS/V1", window_crossing_rule: "DEFER" },
    wire_contract: { authoritative_source: "M3", version_required: true, snapshot_required: true, mismatch_http_status: 409, mismatch_code: "IVR_POLICY_MISMATCH", producer_artifact_ref: "ARTIFACT/M3-PRODUCER/V1", producer_artifact_sha256: expected.expectedM3ProducerSha, cdc_artifact_ref: "ARTIFACT/M3-CDC/V1", cdc_artifact_sha256: expected.expectedM3ProducerSha },
    registry_governance: { controlled_writer_ref: "SERVICE/POLICY-WRITER/V1", controlled_reader_ref: "SERVICE/POLICY-READER/V1", four_eyes: true, atomic_bundle_publish: true, immutable_versions: true, hard_delete_allowed: false, effective_retire_rule: "POLICY/RETIRE/V1", custody_ref: "CUSTODY/POLICY/V1", recovery_ref: "RECOVERY/POLICY/V1", evidence_sha256: expected.expectedRegistrySha, audit_hash_required: true },
    pre_dial_coherence: { strategy: "EXACT_JOB_FLAG_VERSION_MATCH", consistency_rule_ref: "POLICY/PRE-DIAL/V1", checked_at: "BEFORE_EACH_DIAL", unknown_policy_behavior: "FAIL_CLOSED", registry_unavailable_behavior: "FAIL_CLOSED", drift_behavior: "FAIL_CLOSED" },
    capacity_and_token: { model_ref: "MODEL/CAPACITY/V1", evidence_sha256: expected.expectedCapacitySha, recalibrated_for_bundle: true, channel_token_policy_ref: "POLICY/CHANNEL-TOKEN/V1", rate_limit_policy_ref: "POLICY/RATE-LIMIT/V1" },
    cutover_and_rollback: { previous_policy_version: "prod-2026-08-v1", in_flight_rule: "RETAIN_ACCEPTED_SNAPSHOT", rollback_policy_version: "prod-2026-08-v1", rollback_trigger_ref: "RUNBOOK/ROLLBACK/V1", canary_plan_ref: "RUNBOOK/CANARY/V1" },
    rollout: { environments: ["STAGING", "UAT", "PRODUCTION_REAL"], shared_e2e_ref: "REPORT/SHARED-E2E/V1", shared_e2e_sha256: expected.expectedSharedE2ESha, release_packet_ref: "RELEASE/PACKET/V1", release_packet_sha256: expected.expectedReleasePacketSha, production_default_disabled: true, real_customer_call_allowed: false },
  };
  bundle.bundle_sha256 = canonicalBundleHash(bundle);
  expected.expectedBundleSha = bundle.bundle_sha256;
  return {
    expected,
    document: {
      schema_version: SCHEMA_VERSION,
      work_id: WORK_ID,
      status: "SIGNED_POLICY_BUNDLE_COMPLETE",
      source: { ...SOURCE_PINS },
      bundle,
      decision_coverage: decisionCoverage,
      external_evidence: {
        m3_producer_and_cdc: artifact("expectedM3ProducerSha", "M3-PRODUCER"),
        registry_custody_and_recovery: artifact("expectedRegistrySha", "PLATFORM"),
        capacity_and_token_recalibration: artifact("expectedCapacitySha", "M8-CAPACITY"),
        shared_e2e: artifact("expectedSharedE2ESha", "M8-M3-E2E"),
        production_release_packet: artifact("expectedReleasePacketSha", "RELEASE-OWNER"),
      },
      signoffs: REQUIRED_SIGNOFF_ROLES.map((role, index) => ({ role, signer_alias: `${role}-SIGNER`, verifier_alias: `${role}-VERIFIER`, authority_ref: `AUTHORITY/${role}/V1`, authority_sha256: fixtureHash(`authority-${index}`), decision: "APPROVED_FOR_RUNTIME_REVIEW_ONLY", signed_at: "2026-09-04T03:00:00Z", policy_version: bundle.policy_version, bundle_sha256: bundle.bundle_sha256 })),
      safety: Object.fromEntries(SAFETY_KEYS.map((key) => [key, false])),
    },
  };
}

function clone(value) {
  return structuredClone(value);
}

function runSelfTest() {
  const fixture = validFixture();
  validateAttemptPolicyProductionBundle(fixture.document, fixture.expected);
  const mutations = [
    ["reserved version", (d) => { d.bundle.policy_version = "mock-lab-v1"; }],
    ["one program", (d) => { d.bundle.programs.pop(); }],
    ["offset length", (d) => { d.bundle.programs[0].offsets_seconds.pop(); }],
    ["offset zero", (d) => { d.bundle.programs[0].offsets_seconds[0] = 1; }],
    ["offset order", (d) => { d.bundle.programs[1].offsets_seconds[2] = 200; }],
    ["offset window", (d) => { d.bundle.programs[0].offsets_seconds[1] = 900; }],
    ["time order", (d) => { d.bundle.effective_at = "2026-09-03T00:00:00Z"; }],
    ["count invalid", (d) => { d.bundle.counting.invalid_input_counted = true; }],
    ["count technical", (d) => { d.bundle.counting.technical_failure_counted = true; }],
    ["retry count", (d) => { d.bundle.technical_retry.max_retries_per_customer_attempt = 3; }],
    ["retry order", (d) => { d.bundle.technical_retry.backoff_schedule_seconds = [120, 30]; }],
    ["timezone", (d) => { d.bundle.temporal_policy.timezone = "UTC"; }],
    ["wire version", (d) => { d.bundle.wire_contract.version_required = false; }],
    ["wire mismatch", (d) => { d.bundle.wire_contract.mismatch_http_status = 200; }],
    ["registry four eyes", (d) => { d.bundle.registry_governance.four_eyes = false; }],
    ["registry delete", (d) => { d.bundle.registry_governance.hard_delete_allowed = true; }],
    ["coherence choice", (d) => { d.bundle.pre_dial_coherence.strategy = PLACEHOLDER; }],
    ["coherence fail open", (d) => { d.bundle.pre_dial_coherence.drift_behavior = "FAIL_OPEN"; }],
    ["capacity", (d) => { d.bundle.capacity_and_token.recalibrated_for_bundle = false; }],
    ["rollback", (d) => { d.bundle.cutover_and_rollback.rollback_policy_version = "other"; }],
    ["rollout order", (d) => { d.bundle.rollout.environments.reverse(); }],
    ["production enabled", (d) => { d.bundle.rollout.production_default_disabled = false; }],
    ["bundle hash", (d) => { d.bundle.bundle_sha256 = "f".repeat(64); }],
    ["independent bundle hash", (_d, e) => { e.expectedBundleSha = "f".repeat(64); }],
    ["decision missing", (d) => { d.decision_coverage.pop(); }],
    ["decision pending", (d) => { d.decision_coverage[0].state = "PENDING"; }],
    ["external hash", (d) => { d.external_evidence.shared_e2e.sha256 = "f".repeat(64); }],
    ["signoff missing", (d) => { d.signoffs.pop(); }],
    ["signer verifier", (d) => { d.signoffs[0].verifier_alias = d.signoffs[0].signer_alias; }],
    ["signer reused", (d) => { d.signoffs[1].signer_alias = d.signoffs[0].signer_alias; }],
    ["signoff version", (d) => { d.signoffs[0].policy_version = "other"; }],
    ["safety", (d) => { d.safety.production_enabled = true; }],
    ["PII", (d) => { d.signoffs[0].authority_ref = "user@example.com"; }],
    ["secret", (d) => { d.signoffs[0].authority_ref = "API_KEY=VALUE"; }],
    ["extra root", (d) => { d.extra = true; }],
  ];
  let refusals = 0;
  for (const [label, mutate] of mutations) {
    const document = clone(fixture.document);
    const expected = clone(fixture.expected);
    mutate(document, expected);
    try {
      validateAttemptPolicyProductionBundle(document, expected);
      fail(`self-test mutation was accepted: ${label}`);
    } catch (error) {
      if (error.message.startsWith("self-test mutation was accepted")) throw error;
      refusals += 1;
    }
  }
  return { valid: 1, refusals };
}

function parseInputArgs(argv) {
  const flagMap = new Map();
  for (let index = 0; index < argv.length; index += 2) {
    const flag = argv[index];
    const value = argv[index + 1];
    if (!flag?.startsWith("--") || value === undefined || value.startsWith("--") || flagMap.has(flag)) fail("invalid or duplicate CLI arguments");
    flagMap.set(flag, value);
  }
  const required = ["--input", "--expected-bundle-sha", "--expected-decision-pack-sha", "--expected-m3-producer-sha", "--expected-registry-sha", "--expected-capacity-sha", "--expected-shared-e2e-sha", "--expected-release-packet-sha"];
  if (flagMap.size !== required.length || required.some((flag) => !flagMap.has(flag))) fail(`required arguments: ${required.join(" ")}`);
  return {
    inputPath: flagMap.get("--input"),
    expected: {
      expectedBundleSha: flagMap.get("--expected-bundle-sha"),
      expectedDecisionPackSha: flagMap.get("--expected-decision-pack-sha"),
      expectedM3ProducerSha: flagMap.get("--expected-m3-producer-sha"),
      expectedRegistrySha: flagMap.get("--expected-registry-sha"),
      expectedCapacitySha: flagMap.get("--expected-capacity-sha"),
      expectedSharedE2ESha: flagMap.get("--expected-shared-e2e-sha"),
      expectedReleasePacketSha: flagMap.get("--expected-release-packet-sha"),
    },
  };
}

function usage() {
  console.error("Usage:\n  node deploy/ci/scripts/attempt-policy-production-bundle-validator.mjs --check-template <json>\n  node deploy/ci/scripts/attempt-policy-production-bundle-validator.mjs --self-test\n  node deploy/ci/scripts/attempt-policy-production-bundle-validator.mjs --input <json> --expected-bundle-sha <64hex> --expected-decision-pack-sha <64hex> --expected-m3-producer-sha <64hex> --expected-registry-sha <64hex> --expected-capacity-sha <64hex> --expected-shared-e2e-sha <64hex> --expected-release-packet-sha <64hex>");
}

function main(argv) {
  if (argv.length === 1 && argv[0] === "--self-test") {
    const result = runSelfTest();
    console.log(`W0180_SELFTEST_PASS valid=${result.valid} refusals=${result.refusals}`);
    return;
  }
  if (argv.length === 2 && argv[0] === "--check-template") {
    const { bytes, document } = readStrictJson(argv[1]);
    const result = validatePendingTemplate(document);
    console.log(`ATTEMPT_POLICY_TEMPLATE_VALID_NOT_READY decisions=${result.decisionCount} sha256=${sha256(bytes)}`);
    return;
  }
  const { inputPath, expected } = parseInputArgs(argv);
  const { document } = readStrictJson(inputPath);
  const result = validateAttemptPolicyProductionBundle(document, expected);
  console.log(`ATTEMPT_POLICY_BUNDLE_VALID_ELIGIBLE_FOR_RUNTIME_REVIEW_ONLY policy_version=${result.policyVersion} bundle_sha256=${result.bundleSha256}`);
}

try {
  main(process.argv.slice(2));
} catch (error) {
  console.error(`ATTEMPT_POLICY_BUNDLE_REFUSED: ${error.message}`);
  usage();
  process.exitCode = 1;
}
