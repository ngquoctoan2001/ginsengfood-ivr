#!/usr/bin/env node

// W-0182 — Offline, metadata-only intake for the W-0160 monotonic registry decision pack.
// This tool never selects a provider, connects to a registry, writes an adapter, starts
// calibration, promotes a gate, or authorizes a real customer call.

import { createHash } from "node:crypto";
import { lstatSync, mkdtempSync, readFileSync, realpathSync, rmSync, writeFileSync } from "node:fs";
import { dirname, isAbsolute, join, relative, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const SCRIPT_PATH = fileURLToPath(import.meta.url);
const REPOSITORY_ROOT = resolve(dirname(SCRIPT_PATH), "../../..");
const MAX_INPUT_BYTES = 512 * 1024;
const MAX_SOURCE_BYTES = 5 * 1024 * 1024;
const SCHEMA_VERSION = "m8-capacity-checkpoint-registry-decision-pack.v1";
const WORK_ID = "W-0182";
const TEMPLATE_STATUS = "PENDING_EXTERNAL_APPROVAL";
const COMPLETE_STATUS = "EXTERNAL_APPROVAL_COMPLETE";
const PLACEHOLDER = "PENDING_EXTERNAL_DECISION";

const SOURCE_PINS = Object.freeze({
  w0160_evidence_path: "docs/evidence/W-0160/README.md",
  w0160_evidence_sha256: "01d27f785fd96e7aadfad2ac659b26c6247d7cba8a72174cdba2270ebafe02e7",
  m8_15_contract_path:
    "plan/ivr-orther/m8-15-capacity-ledger-checkpoint-registry-contract-2026-09-03.md",
  m8_15_contract_sha256: "e1d0fd37d610a1696b8e6b4117469ea3f8e929eff72dc95121e3ce9679200417",
  capacity_validator_path: "deploy/ci/scripts/capacity-data-intake-validator.mjs",
  capacity_validator_sha256: "4208614b44f55e8b9dc39b304021a7004e693b7dbb72ead84ab6d2cc2ed9ef83",
});

const LOCAL_SOURCE_PAIRS = [
  ["w0160_evidence_path", "w0160_evidence_sha256"],
  ["capacity_validator_path", "capacity_validator_sha256"],
];

const OWNER_SETS = Object.freeze([
  ["PLATFORM", "SECURITY", "MODULE8"],
  ["PLATFORM", "MODULE8", "SECURITY"],
  ["PLATFORM", "MODULE8", "SECURITY"],
  ["MODULE8", "PLATFORM", "SECURITY"],
  ["MODULE8", "SECURITY", "PLATFORM"],
  ["PLATFORM", "SECURITY", "MODULE8"],
  ["PLATFORM", "SECURITY", "MODULE8"],
  ["PLATFORM", "MODULE8", "SECURITY"],
  ["SECURITY", "PLATFORM", "MODULE8"],
  ["SECURITY", "PLATFORM", "MODULE8"],
  ["SECURITY", "PLATFORM", "MODULE8"],
  ["PLATFORM", "SECURITY", "MODULE8"],
  ["PLATFORM", "SECURITY", "MODULE8"],
  ["PLATFORM", "SECURITY", "MODULE8"],
  ["PLATFORM", "SECURITY", "MODULE8"],
]);

const DECISION_IDS = Object.freeze(
  Array.from({ length: 15 }, (_, index) => `CHK-${String(index + 1).padStart(2, "0")}`),
);

const DECISION_EVIDENCE_KEYS = Object.freeze([
  "provider_capability",
  "canonical_schema_fixtures",
  "canonical_schema_fixtures",
  "canonical_schema_fixtures",
  "canonical_schema_fixtures",
  "provider_capability",
  "provider_capability",
  "sandbox_cutover_conformance",
  "iam_kms_network_retention",
  "iam_kms_network_retention",
  "iam_kms_network_retention",
  "recovery_failover_drill",
  "recovery_failover_drill",
  "sandbox_cutover_conformance",
  "sandbox_cutover_conformance",
]);

const EVIDENCE_KEYS = Object.freeze([
  "provider_capability",
  "canonical_schema_fixtures",
  "iam_kms_network_retention",
  "recovery_failover_drill",
  "sandbox_cutover_conformance",
  "approval_signature_bundle",
]);

const REQUIRED_APPROVAL_ROLES = Object.freeze([
  "PLATFORM_OWNER",
  "SECURITY_OWNER",
  "MODULE8_PROJECT_OWNER",
]);

const EXPECTED_FLAGS = new Map([
  ["--expected-contract-sha", "contract"],
  ["--expected-provider-capability-sha", "provider_capability"],
  ["--expected-schema-fixtures-sha", "canonical_schema_fixtures"],
  ["--expected-custody-retention-sha", "iam_kms_network_retention"],
  ["--expected-recovery-drill-sha", "recovery_failover_drill"],
  ["--expected-sandbox-cutover-sha", "sandbox_cutover_conformance"],
  ["--expected-approval-bundle-sha", "approval_signature_bundle"],
]);

const ROOT_KEYS = [
  "schema_version",
  "work_id",
  "status",
  "source",
  "provider_profile",
  "registry_contract",
  "decision_coverage",
  "external_evidence",
  "approvals",
  "independent_verification",
  "safety",
];

const PROVIDER_KEYS = [
  "provider_alias",
  "product_profile_ref",
  "target_environment",
  "account_boundary_alias",
  "region_alias",
  "consistency_model",
  "native_revision_token_supported",
  "strong_read_supported",
  "atomic_record_head_audit_cas_supported",
  "immutable_sequence_records_supported",
  "immutable_audit_supported",
  "server_assigned_utc_supported",
  "delete_history_allowed",
  "client_max_selection_allowed",
  "cache_fallback_allowed",
  "last_write_wins_allowed",
  "capability_artifact_ref",
  "capability_artifact_sha256",
];

const REGISTRY_KEYS = [
  "registry_scope",
  "ledger_id",
  "partition_ref",
  "record_schema_version",
  "canonical_encoding_ref",
  "canonical_encoding_sha256",
  "sequence_format",
  "genesis_rule",
  "latest_read_rule",
  "commit_atomicity",
  "request_id_scope",
  "writer_principal_alias",
  "reader_principal_alias",
  "auditor_principal_alias",
  "retention_policy_ref",
  "break_glass_ref",
  "recovery_ref",
  "cutover_ref",
];

const DECISION_KEYS = [
  "decision_id",
  "state",
  "owners",
  "decision_ref",
  "evidence_key",
  "evidence_sha256",
  "decided_at",
];
const ARTIFACT_KEYS = ["artifact_ref", "sha256", "producer_alias", "produced_at"];
const APPROVAL_KEYS = [
  "role",
  "signer_alias",
  "authority_ref",
  "approval_ref",
  "signature_sha256",
  "decision",
  "signed_at",
  "contract_sha256",
];
const VERIFICATION_KEYS = [
  "verifier_alias",
  "verification_ref",
  "verified_at",
  "contract_sha256",
  "approval_bundle_sha256",
];
const SAFETY_KEYS = [
  "contains_raw_rows_or_payload",
  "contains_source_paths",
  "contains_personal_signer_identity",
  "contains_credentials_or_secrets",
  "provider_selected_by_validator",
  "adapter_implementation_started",
  "external_registry_connected",
  "external_submissions_received",
  "calibration_started",
  "production_gate_promoted",
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

function assertString(value, label, minimum = 3, maximum = 200) {
  if (typeof value !== "string" || value.length < minimum || value.length > maximum) {
    fail(`${label} must be a string of ${minimum}..${maximum} characters`);
  }
  if (value !== value.trim() || /[\u0000-\u001f\u007f]/u.test(value)) {
    fail(`${label} contains control or edge whitespace`);
  }
}

function assertIdentifier(value, label) {
  assertString(value, label);
  if (!/^[A-Z0-9][A-Z0-9._:/+-]+$/u.test(value)) {
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

function assertNoSensitiveStrings(value, label = "root") {
  if (typeof value === "string") {
    if (/[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}/iu.test(value)) {
      fail(`${label} contains an email-like value`);
    }
    if (/(?:ref|alias)$/u.test(label) && /(?:^|\D)(?:\+?\d[\s().-]*){9,15}(?:$|\D)/u.test(value)) {
      fail(`${label} contains a phone-like value`);
    }
    if (/\b\d{1,5}\s+(?:\u0111\u01b0\u1eddng|\u0064uong|phố|pho|street|st\.?|road|rd\.?|avenue|ave\.?)\b/iu.test(value)) {
      fail(`${label} contains a street-address-like value`);
    }
    if (/(?:password|passwd|bearer(?:\s+|[:=])|api[_ -]?key|access[_ -]?token|private[_ -]?key|client[_ -]?secret)\s*[:=]?/iu.test(value)) {
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

function isConfined(pathValue) {
  const rel = relative(REPOSITORY_ROOT, pathValue);
  return rel !== "" && !rel.startsWith("..") && !isAbsolute(rel);
}

function readConfinedFile(inputPath, maximumBytes = MAX_INPUT_BYTES) {
  const resolved = resolve(REPOSITORY_ROOT, inputPath);
  if (!isConfined(resolved)) fail(`path is outside repository root: ${inputPath}`);
  let stat;
  try {
    stat = lstatSync(resolved);
  } catch {
    fail(`path is not readable: ${inputPath}`);
  }
  if (!stat.isFile() || stat.isSymbolicLink()) {
    fail(`path must be a regular non-symlink file: ${inputPath}`);
  }
  if (!isConfined(realpathSync(resolved))) fail(`real path escapes repository root: ${inputPath}`);
  if (stat.size === 0 || stat.size > maximumBytes) {
    fail(`file size must be 1..${maximumBytes} bytes: ${inputPath}`);
  }
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
      if (textValue[position] === "\\") position += 2;
      else if (textValue[position] === '"') {
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
  let textValue;
  try {
    textValue = new TextDecoder("utf-8", { fatal: true }).decode(bytes);
  } catch {
    fail(`input is not strict UTF-8: ${inputPath}`);
  }
  rejectDuplicateJsonKeys(textValue);
  let document;
  try {
    document = JSON.parse(textValue);
  } catch {
    fail(`input must be valid JSON: ${inputPath}`);
  }
  return { bytes, document };
}

function verifySourcePins(source) {
  assertExactKeys(source, Object.keys(SOURCE_PINS), "source");
  for (const [key, expected] of Object.entries(SOURCE_PINS)) {
    if (source[key] !== expected) fail(`source.${key} does not match the pinned value`);
  }
  for (const [pathKey, hashKey] of LOCAL_SOURCE_PAIRS) {
    const { bytes } = readConfinedFile(source[pathKey], MAX_SOURCE_BYTES);
    if (sha256(bytes) !== source[hashKey]) {
      fail(`${source[pathKey]} drifted from its pinned SHA-256`);
    }
  }
}

function templateDocument() {
  const externalEvidence = Object.fromEntries(
    EVIDENCE_KEYS.map((key) => [key, {
      artifact_ref: PLACEHOLDER,
      sha256: PLACEHOLDER,
      producer_alias: PLACEHOLDER,
      produced_at: PLACEHOLDER,
    }]),
  );
  return {
    schema_version: SCHEMA_VERSION,
    work_id: WORK_ID,
    status: TEMPLATE_STATUS,
    source: { ...SOURCE_PINS },
    provider_profile: {
      provider_alias: PLACEHOLDER,
      product_profile_ref: PLACEHOLDER,
      target_environment: "PRODUCTION",
      account_boundary_alias: PLACEHOLDER,
      region_alias: PLACEHOLDER,
      consistency_model: PLACEHOLDER,
      native_revision_token_supported: false,
      strong_read_supported: false,
      atomic_record_head_audit_cas_supported: false,
      immutable_sequence_records_supported: false,
      immutable_audit_supported: false,
      server_assigned_utc_supported: false,
      delete_history_allowed: false,
      client_max_selection_allowed: false,
      cache_fallback_allowed: false,
      last_write_wins_allowed: false,
      capability_artifact_ref: PLACEHOLDER,
      capability_artifact_sha256: PLACEHOLDER,
    },
    registry_contract: {
      registry_scope: "CAPACITY_DATA_INTAKE_LEDGER",
      ledger_id: PLACEHOLDER,
      partition_ref: PLACEHOLDER,
      record_schema_version: "m8-capacity-intake-checkpoint-registry-record.v1",
      canonical_encoding_ref: PLACEHOLDER,
      canonical_encoding_sha256: PLACEHOLDER,
      sequence_format: "POSITIVE_DECIMAL_STRING_NO_LEADING_ZERO",
      genesis_rule: "STRONG_READ_AND_CREATE_IF_ABSENT_SEQUENCE_1",
      latest_read_rule: "AUTHORITATIVE_LINEARIZABLE_COMMITTED_ONLY",
      commit_atomicity: "RECORD_HEAD_AUDIT_SINGLE_TRANSACTION",
      request_id_scope: "UNIQUE_WITHIN_PARTITION_SAME_PAYLOAD_IDEMPOTENT",
      writer_principal_alias: PLACEHOLDER,
      reader_principal_alias: PLACEHOLDER,
      auditor_principal_alias: PLACEHOLDER,
      retention_policy_ref: PLACEHOLDER,
      break_glass_ref: PLACEHOLDER,
      recovery_ref: PLACEHOLDER,
      cutover_ref: PLACEHOLDER,
    },
    decision_coverage: DECISION_IDS.map((decisionId, index) => ({
      decision_id: decisionId,
      state: "PENDING",
      owners: [...OWNER_SETS[index]],
      decision_ref: PLACEHOLDER,
      evidence_key: DECISION_EVIDENCE_KEYS[index],
      evidence_sha256: PLACEHOLDER,
      decided_at: PLACEHOLDER,
    })),
    external_evidence: externalEvidence,
    approvals: REQUIRED_APPROVAL_ROLES.map((role) => ({
      role,
      signer_alias: PLACEHOLDER,
      authority_ref: PLACEHOLDER,
      approval_ref: PLACEHOLDER,
      signature_sha256: PLACEHOLDER,
      decision: "PENDING",
      signed_at: PLACEHOLDER,
      contract_sha256: SOURCE_PINS.m8_15_contract_sha256,
    })),
    independent_verification: {
      verifier_alias: PLACEHOLDER,
      verification_ref: PLACEHOLDER,
      verified_at: PLACEHOLDER,
      contract_sha256: SOURCE_PINS.m8_15_contract_sha256,
      approval_bundle_sha256: PLACEHOLDER,
    },
    safety: Object.fromEntries(SAFETY_KEYS.map((key) => [key, false])),
  };
}

function validateTemplate(document) {
  const expected = templateDocument();
  if (JSON.stringify(document) !== JSON.stringify(expected)) {
    fail("pending template shape or fixed stop-rule value drifted");
  }
}

function validateProvider(provider, evidence) {
  assertExactKeys(provider, PROVIDER_KEYS, "provider_profile");
  for (const key of [
    "provider_alias",
    "product_profile_ref",
    "account_boundary_alias",
    "region_alias",
    "capability_artifact_ref",
  ]) {
    assertIdentifier(provider[key], `provider_profile.${key}`);
  }
  if (provider.target_environment !== "PRODUCTION") {
    fail("provider_profile.target_environment must be PRODUCTION");
  }
  if (provider.consistency_model !== "LINEARIZABLE") {
    fail("provider_profile.consistency_model must be LINEARIZABLE");
  }
  for (const key of [
    "native_revision_token_supported",
    "strong_read_supported",
    "atomic_record_head_audit_cas_supported",
    "immutable_sequence_records_supported",
    "immutable_audit_supported",
    "server_assigned_utc_supported",
  ]) {
    if (provider[key] !== true) fail(`provider_profile.${key} must be true`);
  }
  for (const key of [
    "delete_history_allowed",
    "client_max_selection_allowed",
    "cache_fallback_allowed",
    "last_write_wins_allowed",
  ]) {
    if (provider[key] !== false) fail(`provider_profile.${key} must be false`);
  }
  assertSha256(provider.capability_artifact_sha256, "provider_profile.capability_artifact_sha256");
  const capability = evidence.provider_capability;
  if (
    provider.capability_artifact_ref !== capability.artifact_ref
    || provider.capability_artifact_sha256 !== capability.sha256
  ) {
    fail("provider capability ref/hash must match external_evidence.provider_capability");
  }
}

function validateRegistry(registry, evidence) {
  assertExactKeys(registry, REGISTRY_KEYS, "registry_contract");
  const fixed = {
    registry_scope: "CAPACITY_DATA_INTAKE_LEDGER",
    record_schema_version: "m8-capacity-intake-checkpoint-registry-record.v1",
    sequence_format: "POSITIVE_DECIMAL_STRING_NO_LEADING_ZERO",
    genesis_rule: "STRONG_READ_AND_CREATE_IF_ABSENT_SEQUENCE_1",
    latest_read_rule: "AUTHORITATIVE_LINEARIZABLE_COMMITTED_ONLY",
    commit_atomicity: "RECORD_HEAD_AUDIT_SINGLE_TRANSACTION",
    request_id_scope: "UNIQUE_WITHIN_PARTITION_SAME_PAYLOAD_IDEMPOTENT",
  };
  for (const [key, expected] of Object.entries(fixed)) {
    if (registry[key] !== expected) fail(`registry_contract.${key} must be ${expected}`);
  }
  for (const key of [
    "ledger_id",
    "partition_ref",
    "canonical_encoding_ref",
    "writer_principal_alias",
    "reader_principal_alias",
    "auditor_principal_alias",
    "retention_policy_ref",
    "break_glass_ref",
    "recovery_ref",
    "cutover_ref",
  ]) {
    assertIdentifier(registry[key], `registry_contract.${key}`);
  }
  assertSha256(registry.canonical_encoding_sha256, "registry_contract.canonical_encoding_sha256");
  if (registry.canonical_encoding_sha256 !== evidence.canonical_schema_fixtures.sha256) {
    fail("canonical encoding hash must match canonical schema/fixtures evidence");
  }
  const principals = [
    registry.writer_principal_alias,
    registry.reader_principal_alias,
    registry.auditor_principal_alias,
  ];
  if (new Set(principals).size !== principals.length) {
    fail("writer, reader and auditor principals must be distinct aliases");
  }
}

function validateExternalEvidence(externalEvidence, expectedPins) {
  assertExactKeys(externalEvidence, EVIDENCE_KEYS, "external_evidence");
  for (const key of EVIDENCE_KEYS) {
    const artifact = externalEvidence[key];
    assertExactKeys(artifact, ARTIFACT_KEYS, `external_evidence.${key}`);
    assertIdentifier(artifact.artifact_ref, `external_evidence.${key}.artifact_ref`);
    assertSha256(artifact.sha256, `external_evidence.${key}.sha256`);
    assertIdentifier(artifact.producer_alias, `external_evidence.${key}.producer_alias`);
    assertTimestamp(artifact.produced_at, `external_evidence.${key}.produced_at`);
    if (artifact.sha256 !== expectedPins[key]) {
      fail(`external_evidence.${key}.sha256 does not match independent reviewer pin`);
    }
  }
}

function validateDecisions(decisions, externalEvidence) {
  if (!Array.isArray(decisions) || decisions.length !== DECISION_IDS.length) {
    fail(`decision_coverage must contain exactly ${DECISION_IDS.length} rows`);
  }
  decisions.forEach((decision, index) => {
    const label = `decision_coverage[${index}]`;
    assertExactKeys(decision, DECISION_KEYS, label);
    if (decision.decision_id !== DECISION_IDS[index]) {
      fail(`${label}.decision_id must be ${DECISION_IDS[index]}`);
    }
    if (decision.state !== "ACCEPTED") fail(`${label}.state must be ACCEPTED`);
    if (JSON.stringify(decision.owners) !== JSON.stringify(OWNER_SETS[index])) {
      fail(`${label}.owners must match the W-0160 owner order`);
    }
    if (decision.evidence_key !== DECISION_EVIDENCE_KEYS[index]) {
      fail(`${label}.evidence_key must be ${DECISION_EVIDENCE_KEYS[index]}`);
    }
    assertIdentifier(decision.decision_ref, `${label}.decision_ref`);
    assertSha256(decision.evidence_sha256, `${label}.evidence_sha256`);
    assertTimestamp(decision.decided_at, `${label}.decided_at`);
    const evidence = externalEvidence[decision.evidence_key];
    if (decision.evidence_sha256 !== evidence.sha256) {
      fail(`${label}.evidence_sha256 must match its named external evidence`);
    }
    if (Date.parse(decision.decided_at) < Date.parse(evidence.produced_at)) {
      fail(`${label}.decided_at must not precede its evidence`);
    }
  });
}

function validateApprovals(approvals, verification, externalEvidence) {
  if (!Array.isArray(approvals) || approvals.length !== REQUIRED_APPROVAL_ROLES.length) {
    fail(`approvals must contain exactly ${REQUIRED_APPROVAL_ROLES.length} rows`);
  }
  const operationalEvidenceTimes = EVIDENCE_KEYS
    .filter((key) => key !== "approval_signature_bundle")
    .map((key) => Date.parse(externalEvidence[key].produced_at));
  const latestOperationalEvidence = Math.max(...operationalEvidenceTimes);
  const signerAliases = [];
  approvals.forEach((approval, index) => {
    const label = `approvals[${index}]`;
    assertExactKeys(approval, APPROVAL_KEYS, label);
    if (approval.role !== REQUIRED_APPROVAL_ROLES[index]) {
      fail(`${label}.role must be ${REQUIRED_APPROVAL_ROLES[index]}`);
    }
    for (const key of ["signer_alias", "authority_ref", "approval_ref"]) {
      assertIdentifier(approval[key], `${label}.${key}`);
    }
    assertSha256(approval.signature_sha256, `${label}.signature_sha256`);
    if (approval.decision !== "APPROVE") fail(`${label}.decision must be APPROVE`);
    assertTimestamp(approval.signed_at, `${label}.signed_at`);
    if (Date.parse(approval.signed_at) < latestOperationalEvidence) {
      fail(`${label}.signed_at must not precede operational evidence`);
    }
    if (approval.contract_sha256 !== SOURCE_PINS.m8_15_contract_sha256) {
      fail(`${label}.contract_sha256 must bind exact M8-15`);
    }
    signerAliases.push(approval.signer_alias);
  });
  if (new Set(signerAliases).size !== signerAliases.length) {
    fail("approval signer aliases must be unique across roles");
  }

  assertExactKeys(verification, VERIFICATION_KEYS, "independent_verification");
  assertIdentifier(verification.verifier_alias, "independent_verification.verifier_alias");
  assertIdentifier(verification.verification_ref, "independent_verification.verification_ref");
  assertTimestamp(verification.verified_at, "independent_verification.verified_at");
  if (signerAliases.includes(verification.verifier_alias)) {
    fail("independent verifier must differ from every approval signer");
  }
  if (verification.contract_sha256 !== SOURCE_PINS.m8_15_contract_sha256) {
    fail("independent verification must bind exact M8-15");
  }
  if (verification.approval_bundle_sha256 !== externalEvidence.approval_signature_bundle.sha256) {
    fail("independent verification must bind the approval signature bundle hash");
  }
  const latestApproval = Math.max(...approvals.map((approval) => Date.parse(approval.signed_at)));
  const bundleProducedAt = Date.parse(externalEvidence.approval_signature_bundle.produced_at);
  if (bundleProducedAt < latestApproval) {
    fail("approval signature bundle must be produced after all three approvals");
  }
  if (Date.parse(verification.verified_at) < bundleProducedAt) {
    fail("independent verification must follow the approval signature bundle");
  }
}

function validateSafety(safety) {
  assertExactKeys(safety, SAFETY_KEYS, "safety");
  for (const key of SAFETY_KEYS) {
    if (safety[key] !== false) fail(`safety.${key} must be false`);
  }
}

function validateCapacityRegistryDecisionPack(document, mode, expectedPins = null) {
  assertExactKeys(document, ROOT_KEYS, "root");
  if (document.schema_version !== SCHEMA_VERSION) fail(`schema_version must be ${SCHEMA_VERSION}`);
  if (document.work_id !== WORK_ID) fail(`work_id must be ${WORK_ID}`);
  verifySourcePins(document.source);
  validateSafety(document.safety);

  if (mode === "template") {
    if (document.status !== TEMPLATE_STATUS) fail(`status must be ${TEMPLATE_STATUS}`);
    validateTemplate(document);
    return;
  }

  if (document.status !== COMPLETE_STATUS) fail(`status must be ${COMPLETE_STATUS}`);
  if (!expectedPins) fail("independent reviewer pins are required");
  if (expectedPins.contract !== SOURCE_PINS.m8_15_contract_sha256) {
    fail("independent contract hash does not match exact M8-15");
  }
  validateExternalEvidence(document.external_evidence, expectedPins);
  validateProvider(document.provider_profile, document.external_evidence);
  validateRegistry(document.registry_contract, document.external_evidence);
  validateDecisions(document.decision_coverage, document.external_evidence);
  validateApprovals(
    document.approvals,
    document.independent_verification,
    document.external_evidence,
  );
  const serialized = JSON.stringify(document);
  if (serialized.includes(PLACEHOLDER)) fail("completed input must not contain pending placeholders");
  assertNoSensitiveStrings(document);
}

function validateFile(inputPath, mode, expectedPins = null) {
  const { bytes, document } = readStrictJson(inputPath);
  validateCapacityRegistryDecisionPack(document, mode, expectedPins);
  return { inputSha256: sha256(bytes) };
}

function validCompletedDocument() {
  const document = templateDocument();
  document.status = COMPLETE_STATUS;
  document.provider_profile = {
    provider_alias: "PLATFORM_TRUST_STORE_01",
    product_profile_ref: "PLATFORM:REGISTRY_PROFILE_01",
    target_environment: "PRODUCTION",
    account_boundary_alias: "PLATFORM:TRUST_BOUNDARY_01",
    region_alias: "PLATFORM:PRIMARY_REGION_01",
    consistency_model: "LINEARIZABLE",
    native_revision_token_supported: true,
    strong_read_supported: true,
    atomic_record_head_audit_cas_supported: true,
    immutable_sequence_records_supported: true,
    immutable_audit_supported: true,
    server_assigned_utc_supported: true,
    delete_history_allowed: false,
    client_max_selection_allowed: false,
    cache_fallback_allowed: false,
    last_write_wins_allowed: false,
    capability_artifact_ref: "PLATFORM:CAPABILITY_REPORT_01",
    capability_artifact_sha256: "a".repeat(64),
  };
  document.registry_contract = {
    registry_scope: "CAPACITY_DATA_INTAKE_LEDGER",
    ledger_id: "CAPACITY_LEDGER_PRODUCTION_01",
    partition_ref: "PLATFORM:PRODUCTION_CAPACITY_PARTITION_01",
    record_schema_version: "m8-capacity-intake-checkpoint-registry-record.v1",
    canonical_encoding_ref: "M8:REGISTRY_SCHEMA_FIXTURES_01",
    canonical_encoding_sha256: "b".repeat(64),
    sequence_format: "POSITIVE_DECIMAL_STRING_NO_LEADING_ZERO",
    genesis_rule: "STRONG_READ_AND_CREATE_IF_ABSENT_SEQUENCE_1",
    latest_read_rule: "AUTHORITATIVE_LINEARIZABLE_COMMITTED_ONLY",
    commit_atomicity: "RECORD_HEAD_AUDIT_SINGLE_TRANSACTION",
    request_id_scope: "UNIQUE_WITHIN_PARTITION_SAME_PAYLOAD_IDEMPOTENT",
    writer_principal_alias: "M8_REGISTRY_WRITER_01",
    reader_principal_alias: "M8_REGISTRY_READER_01",
    auditor_principal_alias: "SECURITY_REGISTRY_AUDITOR_01",
    retention_policy_ref: "SECURITY:RETENTION_POLICY_01",
    break_glass_ref: "SECURITY:BREAK_GLASS_POLICY_01",
    recovery_ref: "PLATFORM:RECOVERY_RUNBOOK_01",
    cutover_ref: "PLATFORM:CUTOVER_PACKET_01",
  };
  const hashes = {
    provider_capability: "a".repeat(64),
    canonical_schema_fixtures: "b".repeat(64),
    iam_kms_network_retention: "c".repeat(64),
    recovery_failover_drill: "d".repeat(64),
    sandbox_cutover_conformance: "e".repeat(64),
    approval_signature_bundle: "f".repeat(64),
  };
  document.external_evidence = Object.fromEntries(
    EVIDENCE_KEYS.map((key, index) => [key, {
      artifact_ref: `EVIDENCE:${key.toUpperCase()}:01`,
      sha256: hashes[key],
      producer_alias: index < 2 ? "PLATFORM_OWNER_TEAM" : "SECURITY_PLATFORM_TEAM",
      produced_at: `2026-09-10T09:${String(index * 5).padStart(2, "0")}:00+07:00`,
    }]),
  );
  document.provider_profile.capability_artifact_ref =
    document.external_evidence.provider_capability.artifact_ref;
  document.decision_coverage = DECISION_IDS.map((decisionId, index) => ({
    decision_id: decisionId,
    state: "ACCEPTED",
    owners: [...OWNER_SETS[index]],
    decision_ref: `DECISION:${decisionId}:01`,
    evidence_key: DECISION_EVIDENCE_KEYS[index],
    evidence_sha256: hashes[DECISION_EVIDENCE_KEYS[index]],
    decided_at: `2026-09-10T10:${String(index).padStart(2, "0")}:00+07:00`,
  }));
  document.approvals = REQUIRED_APPROVAL_ROLES.map((role, index) => ({
    role,
    signer_alias: `${role}_SIGNER_01`,
    authority_ref: `${role}:AUTHORITY_01`,
    approval_ref: `${role}:APPROVAL_01`,
    signature_sha256: String(index + 1).repeat(64),
    decision: "APPROVE",
    signed_at: `2026-09-10T11:${String(index * 5).padStart(2, "0")}:00+07:00`,
    contract_sha256: SOURCE_PINS.m8_15_contract_sha256,
  }));
  document.external_evidence.approval_signature_bundle.produced_at = "2026-09-10T11:15:00+07:00";
  document.independent_verification = {
    verifier_alias: "INDEPENDENT_M8_AUDITOR_01",
    verification_ref: "M8:INDEPENDENT_VERIFICATION_01",
    verified_at: "2026-09-10T11:30:00+07:00",
    contract_sha256: SOURCE_PINS.m8_15_contract_sha256,
    approval_bundle_sha256: hashes.approval_signature_bundle,
  };
  return document;
}

function expectedPins() {
  return {
    contract: SOURCE_PINS.m8_15_contract_sha256,
    provider_capability: "a".repeat(64),
    canonical_schema_fixtures: "b".repeat(64),
    iam_kms_network_retention: "c".repeat(64),
    recovery_failover_drill: "d".repeat(64),
    sandbox_cutover_conformance: "e".repeat(64),
    approval_signature_bundle: "f".repeat(64),
  };
}

function clone(value) {
  return structuredClone(value);
}

function runSelfTest() {
  const temporaryRoot = mkdtempSync(join(REPOSITORY_ROOT, ".w0182-selftest-"));
  let refusals = 0;
  const writeCase = (name, value) => {
    const path = join(temporaryRoot, `${name}.json`);
    const bytes = Buffer.isBuffer(value)
      ? value
      : Buffer.from(typeof value === "string" ? value : `${JSON.stringify(value, null, 2)}\n`, "utf8");
    writeFileSync(path, bytes);
    return path;
  };
  const expectFailure = (name, value, mode = "input", pins = expectedPins()) => {
    try {
      validateFile(writeCase(name, value), mode, pins);
      fail(`self-test ${name} unexpectedly passed`);
    } catch (error) {
      if (error.message.includes("unexpectedly passed")) throw error;
      refusals += 1;
    }
  };

  try {
    const template = templateDocument();
    validateFile(writeCase("template-valid", template), "template");
    const valid = validCompletedDocument();
    validateFile(writeCase("completed-valid", valid), "input", expectedPins());

    expectFailure("pending-as-completed", template);
    const missing = clone(valid);
    delete missing.provider_profile.provider_alias;
    expectFailure("missing-key", missing);
    const extra = clone(valid);
    extra.registry_contract.unsupported = true;
    expectFailure("extra-key", extra);
    const sourceDrift = clone(valid);
    sourceDrift.source.w0160_evidence_sha256 = "0".repeat(64);
    expectFailure("source-pin", sourceDrift);
    expectFailure("contract-reviewer-pin", valid, "input", { ...expectedPins(), contract: "0".repeat(64) });

    for (const key of EVIDENCE_KEYS) {
      const pins = { ...expectedPins(), [key]: "0".repeat(64) };
      expectFailure(`independent-pin-${key}`, valid, "input", pins);
    }

    const missingDecision = clone(valid);
    missingDecision.decision_coverage.pop();
    expectFailure("missing-decision", missingDecision);
    const duplicateDecision = clone(valid);
    duplicateDecision.decision_coverage[1].decision_id = "CHK-01";
    expectFailure("duplicate-decision", duplicateDecision);
    const pendingDecision = clone(valid);
    pendingDecision.decision_coverage[0].state = "PENDING";
    expectFailure("pending-decision", pendingDecision);
    const wrongOwners = clone(valid);
    wrongOwners.decision_coverage[0].owners = ["PLATFORM"];
    expectFailure("wrong-owners", wrongOwners);
    const wrongEvidenceKey = clone(valid);
    wrongEvidenceKey.decision_coverage[0].evidence_key = "canonical_schema_fixtures";
    expectFailure("wrong-evidence-key", wrongEvidenceKey);
    const wrongEvidenceHash = clone(valid);
    wrongEvidenceHash.decision_coverage[0].evidence_sha256 = "0".repeat(64);
    expectFailure("wrong-evidence-hash", wrongEvidenceHash);
    const earlyDecision = clone(valid);
    earlyDecision.decision_coverage[0].decided_at = "2026-09-10T08:00:00+07:00";
    expectFailure("early-decision", earlyDecision);

    const providerMutations = [
      ["eventual-read", (value) => { value.provider_profile.consistency_model = "EVENTUAL"; }],
      ["strong-read-false", (value) => { value.provider_profile.strong_read_supported = false; }],
      ["atomic-cas-false", (value) => { value.provider_profile.atomic_record_head_audit_cas_supported = false; }],
      ["immutable-record-false", (value) => { value.provider_profile.immutable_sequence_records_supported = false; }],
      ["immutable-audit-false", (value) => { value.provider_profile.immutable_audit_supported = false; }],
      ["delete-history", (value) => { value.provider_profile.delete_history_allowed = true; }],
      ["client-max", (value) => { value.provider_profile.client_max_selection_allowed = true; }],
      ["cache-fallback", (value) => { value.provider_profile.cache_fallback_allowed = true; }],
      ["last-write-wins", (value) => { value.provider_profile.last_write_wins_allowed = true; }],
      ["provider-capability-cross-link", (value) => { value.provider_profile.capability_artifact_ref = "PLATFORM:OTHER_REPORT"; }],
    ];
    for (const [name, mutate] of providerMutations) {
      const value = clone(valid);
      mutate(value);
      expectFailure(name, value);
    }

    const wrongScope = clone(valid);
    wrongScope.registry_contract.registry_scope = "OTHER";
    expectFailure("wrong-scope", wrongScope);
    const wrongSequence = clone(valid);
    wrongSequence.registry_contract.sequence_format = "INTEGER";
    expectFailure("wrong-sequence", wrongSequence);
    const wrongLatest = clone(valid);
    wrongLatest.registry_contract.latest_read_rule = "CLIENT_MAX";
    expectFailure("wrong-latest", wrongLatest);
    const wrongAtomicity = clone(valid);
    wrongAtomicity.registry_contract.commit_atomicity = "TWO_STEP";
    expectFailure("wrong-atomicity", wrongAtomicity);
    const samePrincipal = clone(valid);
    samePrincipal.registry_contract.auditor_principal_alias = samePrincipal.registry_contract.writer_principal_alias;
    expectFailure("same-principal", samePrincipal);
    const encodingDrift = clone(valid);
    encodingDrift.registry_contract.canonical_encoding_sha256 = "0".repeat(64);
    expectFailure("encoding-evidence-drift", encodingDrift);

    const missingApproval = clone(valid);
    missingApproval.approvals.pop();
    expectFailure("missing-approval", missingApproval);
    const wrongRole = clone(valid);
    wrongRole.approvals[0].role = "DEVELOPER";
    expectFailure("wrong-approval-role", wrongRole);
    const duplicateSigner = clone(valid);
    duplicateSigner.approvals[1].signer_alias = duplicateSigner.approvals[0].signer_alias;
    expectFailure("duplicate-signer", duplicateSigner);
    const signerVerifier = clone(valid);
    signerVerifier.independent_verification.verifier_alias = signerVerifier.approvals[0].signer_alias;
    expectFailure("signer-is-verifier", signerVerifier);
    const rejectedApproval = clone(valid);
    rejectedApproval.approvals[0].decision = "CONDITIONAL";
    expectFailure("conditional-approval", rejectedApproval);
    const wrongApprovalContract = clone(valid);
    wrongApprovalContract.approvals[0].contract_sha256 = "0".repeat(64);
    expectFailure("approval-contract-drift", wrongApprovalContract);
    const earlyApproval = clone(valid);
    earlyApproval.approvals[0].signed_at = "2026-09-10T08:00:00+07:00";
    expectFailure("approval-before-evidence", earlyApproval);
    const earlyBundle = clone(valid);
    earlyBundle.external_evidence.approval_signature_bundle.produced_at = "2026-09-10T11:00:00+07:00";
    expectFailure("bundle-before-approvals", earlyBundle);
    const earlyVerification = clone(valid);
    earlyVerification.independent_verification.verified_at = "2026-09-10T11:00:00+07:00";
    expectFailure("verification-before-bundle", earlyVerification);
    const wrongVerificationBundle = clone(valid);
    wrongVerificationBundle.independent_verification.approval_bundle_sha256 = "0".repeat(64);
    expectFailure("verification-bundle-drift", wrongVerificationBundle);

    const unsafe = clone(valid);
    unsafe.safety.adapter_implementation_started = true;
    expectFailure("adapter-started", unsafe);
    const calibrated = clone(valid);
    calibrated.safety.calibration_started = true;
    expectFailure("calibration-started", calibrated);
    const realCall = clone(valid);
    realCall.safety.real_customer_call_allowed = true;
    expectFailure("real-call", realCall);
    const placeholder = clone(valid);
    placeholder.registry_contract.cutover_ref = PLACEHOLDER;
    expectFailure("placeholder", placeholder);
    const email = clone(valid);
    email.approvals[0].approval_ref = "OWNER@EXAMPLE.INVALID";
    expectFailure("email", email);
    const phone = clone(valid);
    phone.approvals[0].approval_ref = ["CALL:+84", "912345678"].join("");
    expectFailure("phone", phone);
    const secret = clone(valid);
    secret.approvals[0].authority_ref = "BEARER:ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    expectFailure("secret", secret);

    expectFailure("malformed", '{"schema_version":');
    expectFailure("duplicate-json-key", '{"schema_version":"a","schema_version":"b"}');
    expectFailure("bom", Buffer.concat([Buffer.from([0xef, 0xbb, 0xbf]), Buffer.from("{}")]))
    expectFailure("oversized", Buffer.alloc(MAX_INPUT_BYTES + 1, 0x20));
    try {
      validateFile(resolve(REPOSITORY_ROOT, "..", "outside-w0182.json"), "input", expectedPins());
      fail("self-test outside-root unexpectedly passed");
    } catch (error) {
      if (error.message.includes("unexpectedly passed")) throw error;
      refusals += 1;
    }

    return { template: 1, valid: 1, refusals };
  } finally {
    rmSync(temporaryRoot, { recursive: true, force: true });
  }
}

function parseInputArguments(argv) {
  if (argv.length < 2 || argv[0] !== "--input") fail(usage());
  const inputPath = argv[1];
  const expected = {};
  const seen = new Set();
  for (let index = 2; index < argv.length; index += 2) {
    const flag = argv[index];
    const value = argv[index + 1];
    if (!EXPECTED_FLAGS.has(flag) || value === undefined || seen.has(flag)) fail(usage());
    seen.add(flag);
    expected[EXPECTED_FLAGS.get(flag)] = value;
  }
  if (seen.size !== EXPECTED_FLAGS.size) fail("all seven independent reviewer pins are required");
  for (const [key, value] of Object.entries(expected)) assertSha256(value, `expected ${key} SHA-256`);
  return { inputPath, expected };
}

function usage() {
  return [
    "Usage:",
    "  node deploy/ci/scripts/capacity-registry-decision-pack-validator.mjs --check-template <json>",
    "  node deploy/ci/scripts/capacity-registry-decision-pack-validator.mjs --self-test",
    "  node deploy/ci/scripts/capacity-registry-decision-pack-validator.mjs --input <json> \\",
    "    --expected-contract-sha <64hex> --expected-provider-capability-sha <64hex> \\",
    "    --expected-schema-fixtures-sha <64hex> --expected-custody-retention-sha <64hex> \\",
    "    --expected-recovery-drill-sha <64hex> --expected-sandbox-cutover-sha <64hex> \\",
    "    --expected-approval-bundle-sha <64hex>",
  ].join("\n");
}

function main(argv) {
  if (argv.length === 1 && argv[0] === "--self-test") {
    const result = runSelfTest();
    console.log(
      `W0182_SELFTEST_PASS template=${result.template} valid=${result.valid}`
        + ` refusals=${result.refusals} decisions=15 approvals=3`,
    );
    return;
  }
  if (argv.length === 2 && argv[0] === "--check-template") {
    const result = validateFile(argv[1], "template");
    console.log(`CAPACITY_REGISTRY_DECISION_TEMPLATE_VALID_NOT_READY sha256=${result.inputSha256}`);
    return;
  }
  const { inputPath, expected } = parseInputArguments(argv);
  const result = validateFile(inputPath, "input", expected);
  console.log(
    `CAPACITY_REGISTRY_DECISION_PACK_VALID_ELIGIBLE_FOR_ADAPTER_REVIEW_ONLY sha256=${result.inputSha256}`,
  );
}

try {
  main(process.argv.slice(2));
} catch (error) {
  console.error(`W0182_VALIDATION_FAILED: ${error.message}`);
  process.exitCode = 1;
}
