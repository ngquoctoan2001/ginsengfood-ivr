#!/usr/bin/env node

import { createHash } from "node:crypto";
import {
  appendFileSync,
  closeSync,
  existsSync,
  fsyncSync,
  lstatSync,
  mkdirSync,
  mkdtempSync,
  openSync,
  readFileSync,
  realpathSync,
  rmSync,
  statSync,
  unlinkSync,
  writeFileSync,
} from "node:fs";
import { tmpdir } from "node:os";
import { dirname, isAbsolute, relative, resolve, sep } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

const SCRIPT_PATH = fileURLToPath(import.meta.url);
const REPOSITORY_ROOT = resolve(dirname(SCRIPT_PATH), "../../..");
const VALIDATOR_PATH = "deploy/ci/scripts/capacity-data-intake-validator.mjs";
const SOURCE_CONTRACT_PATH = "plan/ivr-orther/m8-14-capacity-calibration-data-intake-bundle-2026-09-03.md";
const BUNDLE_SCHEMA = "m8-capacity-intake-bundle.v1";
const RECEIPT_SCHEMA = "m8-capacity-intake-validation-receipt.v1";
const LEDGER_ENTRY_SCHEMA = "m8-capacity-intake-ledger-entry.v1";
const LEDGER_CHECKPOINT_SCHEMA = "m8-capacity-intake-ledger-head-checkpoint.v1";
const EXPECTED_WORK_ID = "W-0154";
const EXTERNAL_STATUS = "EXTERNAL_OWNER_ATTESTED";
const TEST_STATUS = "TEST_ONLY";
const MAX_ARTIFACT_BYTES = 50 * 1024 * 1024;
const MAX_LEDGER_BYTES = 50 * 1024 * 1024;
const PROGRAMMES = ["GOLDEN_HOUR", "TWENTY_FOUR_SEVEN"];
const VALIDATION_SCOPE = [
  "BUNDLE_SCHEMA_AND_FOUR_GROUP_COMPLETENESS",
  "SOURCE_CONTRACT_AND_ARTIFACT_SHA256",
  "ROOT_CONFINED_REGULAR_ARTIFACT_PATHS",
  "PROVENANCE_AND_SIGNER_METADATA_SHAPE",
  "PII_SECRET_AND_SENSITIVE_FIELD_GUARDS",
  "TIMING_ROW_INVARIANTS",
  "ARRIVAL_ROLLING_WINDOW_RECONSTRUCTABILITY",
  "POLICY_OUTCOME_COHERENCE_AND_RECONCILIATION",
  "INFRA_MULTI_CHANNEL_FAILURE_EVIDENCE_SHAPE",
];
const GROUP_SCHEMAS = new Map([
  ["TIMING", "m8-capacity-timing.v1"],
  ["ARRIVAL", "m8-capacity-arrival.v1"],
  ["POLICY_OUTCOME", "m8-capacity-policy-outcome.v1"],
  ["INFRA_RESERVE", "m8-capacity-infra-reserve.v1"],
]);

const BUNDLE_KEYS = [
  "schema_version",
  "work_id",
  "status",
  "bundle_id",
  "created_at_utc",
  "source_contract",
  "source_contract_sha256",
  "submissions",
];
const SUBMISSION_KEYS = [
  "submission_id",
  "data_group",
  "artifact_path",
  "artifact_sha256",
  "artifact_format",
  "schema_version",
  "source_system",
  "source_version",
  "observation_start_utc",
  "observation_end_utc",
  "timezone_context",
  "record_count",
  "filtering_rule",
  "pii_statement",
  "signer_identity",
  "signer_role",
  "signer_org",
  "authority_source",
  "signed_at",
  "limitations",
];
const RECEIPT_KEYS = [
  "schema_version",
  "work_id",
  "status",
  "validated_at_utc",
  "bundle_id",
  "bundle_status",
  "bundle_manifest_sha256",
  "source_contract",
  "source_contract_sha256",
  "validator",
  "validation",
  "submissions",
  "safety",
  "limitations",
];
const RECEIPT_SUBMISSION_KEYS = [
  "submission_id",
  "data_group",
  "schema_version",
  "artifact_sha256",
  "source_system",
  "source_version",
  "observation_start_utc",
  "observation_end_utc",
  "record_count",
  "signer_identity_alias",
  "signer_role",
  "signer_org",
  "authority_source",
  "signed_at",
];
const RECEIPT_LIMITATIONS = [
  "SIGNER_AND_AUTHORITY_ARE_METADATA_ONLY_NOT_EXTERNALLY_VERIFIED",
  "SCHEMA_HASH_AND_PII_PASS_DO_NOT_PROVE_BUSINESS_DATA_CORRECTNESS",
  "RECEIPT_IS_NOT_CALIBRATION_SHARED_E2E_OR_PRODUCTION_APPROVAL",
];
const LEDGER_ENTRY_KEYS = [
  "schema_version",
  "work_id",
  "status",
  "appended_at_utc",
  "idempotency_key",
  "receipt_sha256",
  "receipt_status",
  "bundle_id",
  "bundle_manifest_sha256",
  "verification",
  "previous_entry_sha256",
  "safety",
];
const LEDGER_VERIFICATION_KEYS = [
  "work_id",
  "status",
  "validator_sha256",
  "source_contract_sha256",
  "authority",
  "group_count",
  "total_records",
];
const LEDGER_SAFETY_KEYS = [
  "raw_rows_persisted",
  "receipt_path_persisted",
  "submission_metadata_persisted",
  "credential_material_persisted",
  "external_authority_verified",
  "calibration_status",
  "production_gate_promoted",
  "real_customer_call_allowed",
];
const LEDGER_CHECKPOINT_KEYS = [
  "schema_version",
  "work_id",
  "status",
  "checkpointed_at_utc",
  "ledger_id",
  "ledger_entry_schema_version",
  "entry_count",
  "ledger_sha256",
  "last_entry_sha256",
  "last_receipt_sha256",
  "source_contract_sha256",
  "checkpoint_validator_sha256",
  "authority",
  "safety",
  "limitations",
];
const LEDGER_CHECKPOINT_SAFETY_KEYS = [
  "raw_rows_persisted",
  "ledger_path_persisted",
  "submission_metadata_persisted",
  "credential_material_persisted",
  "external_trust_store_verified",
  "external_authority_verified",
  "calibration_status",
  "production_gate_promoted",
  "real_customer_call_allowed",
];
const LEDGER_CHECKPOINT_LIMITATIONS = [
  "LATEST_CHECKPOINT_HASH_MUST_BE_STORED_OUTSIDE_LEDGER_AND_CHECKPOINT",
  "CALLER_SUPPLIED_TRUST_ANCHOR_IS_NOT_EXTERNALLY_VERIFIED_BY_THIS_TOOL",
  "CHECKPOINT_IS_NOT_CALIBRATION_SHARED_E2E_OR_PRODUCTION_APPROVAL",
];

const TIMING_ROW_KEYS = [
  "run_label",
  "attempt_label",
  "programme",
  "execution_mode",
  "carrier_label",
  "scenario",
  "disposition",
  "started_at_utc",
  "ended_at_utc",
  "available_again_at_utc",
  "occupancy_ms",
  "cooldown_ms",
  "full_cycle_ms",
  "cdr_correlation_ref",
  "gateway_model",
  "firmware_version",
  "codec_profile",
];
const ARRIVAL_ROW_KEYS = [
  "dataset_id",
  "programme",
  "session_definition_id",
  "business_timezone",
  "bucket_start_utc",
  "bucket_end_utc",
  "eligible_order_count",
  "source_query_version",
  "eligibility_filter_version",
  "data_quality_flag",
];
const POLICY_ROW_KEYS = [
  "policy_version",
  "programme",
  "execution_mode",
  "max_customer_attempts",
  "offsets_seconds",
  "confirmation_window_seconds",
  "effective_from_utc",
  "retire_at_utc",
  "bundle_sha256",
  "product_signer",
  "order_core_signer",
  "m3_producer_version",
];
const OUTCOME_ROW_KEYS = [
  "dataset_id",
  "programme",
  "policy_version",
  "attempt_ordinal",
  "normalized_disposition",
  "outcome_count",
  "total_valid_attempts",
  "observation_start_utc",
  "observation_end_utc",
  "retry_eligible",
  "technical_retry_classification",
  "data_quality_flag",
];
const TOPOLOGY_ROW_KEYS = [
  "submission_id",
  "topology_version",
  "vendor_model",
  "firmware_version",
  "carrier_scope",
  "tested_channel_count",
  "per_channel_concurrency",
  "account_quota",
  "reserve_factor",
  "reserve_rationale",
  "quarantine_policy_ref",
  "failover_policy_ref",
  "test_report_sha256",
  "observation_start_utc",
  "observation_end_utc",
];
const SCENARIO_ROW_KEYS = [
  "scenario_id",
  "available_channels",
  "quarantined_channels",
  "failed_provider_or_gateway",
  "offered_attempts",
  "completed_attempts",
  "deadline_expired_attempts",
  "recovery_seconds",
  "result",
  "evidence_ref",
];

const PHONE_PATTERN = /(?<![0-9A-Za-z])(?:0[0-9]{9}|(?:84|\+84)[0-9]{9}|0[0-9]{2}[\s.-][0-9]{3}[\s.-][0-9]{4}|(?:84|\+84)[\s.-]*\(?[0-9]{2}\)?[\s.-][0-9]{3}[\s.-][0-9]{4})(?![0-9A-Za-z])/u;
const EMAIL_PATTERN = /(?<![\w.+-])[\w.+-]+@[\w.-]+\.[A-Za-z]{2,}(?![\w.-])/u;
const ADDRESS_PATTERN = /(?<![\p{L}\p{N}])(?:(?:duong|so nha|ngo|hem|ngach|thon|ap)\s+[A-Za-z0-9]|(?:đường|số nhà|ngõ|hẻm|ngách|thôn|ấp|tổ)\s+)/iu;
const SECRET_VALUE_PATTERN = /-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----|\bBearer\s+[A-Za-z0-9._~+\/-]{8,}|\bsk-[A-Za-z0-9_-]{12,}|(?:dial[_-]?token)["'`: =]+[A-Za-z0-9._-]{8,}/iu;
const FORBIDDEN_NORMALIZED_KEYS = new Set([
  "authorization",
  "accesstoken",
  "refreshtoken",
  "dialtoken",
  "password",
  "secret",
  "secretvalue",
  "phone",
  "phonenumber",
  "msisdn",
  "customerid",
  "customername",
  "fulladdress",
  "streetaddress",
]);
const PLACEHOLDER_PATTERN = /^(?:<.*>|TBD|TODO|UNKNOWN|NOT_RECEIVED|OWNER_TO_FILL|PENDING)$/iu;
const SAFE_IDENTIFIER_PATTERN = /^[A-Za-z0-9][A-Za-z0-9._:-]{1,127}$/u;
const ENUM_PATTERN = /^[A-Z][A-Z0-9_]{1,63}$/u;

export function validateCapacityDataIntakeBundle(bundleDirectory, options = {}) {
  const allowTestOnly = options.allowTestOnly === true;
  const root = realpathSync(resolve(bundleDirectory));
  assert(statSync(root).isDirectory(), "bundle directory must be a directory");

  const manifestPath = resolve(root, "bundle-manifest.json");
  const manifestRaw = readBoundedFile(manifestPath, "bundle manifest");
  assertNoSensitiveContent(manifestRaw, "bundle manifest");
  const manifest = parseJson(manifestRaw, "bundle manifest");
  assertExactKeys(manifest, BUNDLE_KEYS, "bundle manifest");
  assert(manifest.schema_version === BUNDLE_SCHEMA, "bundle schema_version is unsupported");
  assert(manifest.work_id === EXPECTED_WORK_ID, "bundle work_id must reference W-0154");
  assert(
    manifest.status === EXTERNAL_STATUS || (allowTestOnly && manifest.status === TEST_STATUS),
    "bundle status is not an external-owner attestation",
  );
  assertSafeIdentifier(manifest.bundle_id, "bundle_id");
  parseUtc(manifest.created_at_utc, "created_at_utc");
  assert(manifest.source_contract === SOURCE_CONTRACT_PATH, "source_contract path drift");
  assertSha256(manifest.source_contract_sha256, "source_contract_sha256");
  const contractHash = sha256File(resolve(REPOSITORY_ROOT, SOURCE_CONTRACT_PATH));
  assert(manifest.source_contract_sha256 === contractHash, "source contract hash drift");
  assert(Array.isArray(manifest.submissions), "submissions must be an array");
  assert(manifest.submissions.length === GROUP_SCHEMAS.size, "bundle must contain exactly four submissions");

  const submissionIds = new Set();
  const artifactPaths = new Set();
  const observedGroups = new Set();
  const groupResults = [];
  for (const submission of manifest.submissions) {
    assertExactKeys(submission, SUBMISSION_KEYS, "submission envelope");
    validateSubmissionEnvelope(submission, allowTestOnly);
    assert(!submissionIds.has(submission.submission_id), "duplicate submission_id");
    assert(!artifactPaths.has(submission.artifact_path), "duplicate artifact_path");
    assert(!observedGroups.has(submission.data_group), "duplicate data_group");
    submissionIds.add(submission.submission_id);
    artifactPaths.add(submission.artifact_path);
    observedGroups.add(submission.data_group);

    const artifact = loadArtifact(root, submission);
    const rowCount = validateArtifact(submission, artifact);
    assert(rowCount === submission.record_count, "record_count does not match artifact rows");
    groupResults.push({
      submission_id: submission.submission_id,
      data_group: submission.data_group,
      schema_version: submission.schema_version,
      artifact_sha256: submission.artifact_sha256,
      source_system: submission.source_system,
      source_version: submission.source_version,
      observation_start_utc: submission.observation_start_utc,
      observation_end_utc: submission.observation_end_utc,
      record_count: rowCount,
      signer_identity_alias: submission.signer_identity,
      signer_role: submission.signer_role,
      signer_org: submission.signer_org,
      authority_source: submission.authority_source,
      signed_at: submission.signed_at,
    });
  }

  assertSetEquals(observedGroups, new Set(GROUP_SCHEMAS.keys()), "data-group completeness");
  const manifestSha256 = sha256(manifestRaw);
  return {
    status: "CAPACITY_DATA_INTAKE_VALID",
    authority: "METADATA_ONLY_NOT_EXTERNALLY_VERIFIED",
    bundle_status: manifest.status,
    bundle_id: manifest.bundle_id,
    manifest_sha256: manifestSha256,
    source_contract: manifest.source_contract,
    source_contract_sha256: manifest.source_contract_sha256,
    groups: groupResults.sort((left, right) => left.data_group.localeCompare(right.data_group)),
    total_records: groupResults.reduce((sum, item) => sum + item.record_count, 0),
  };
}

export function writeCapacityDataIntakeReceipt(validationResult, receiptPath, options = {}) {
  const receipt = buildCapacityDataIntakeReceipt(validationResult, options);
  const serialized = `${JSON.stringify(receipt, null, 2)}\n`;
  assertNoSensitiveContent(serialized, "validation receipt");
  assertNoForbiddenKeys(receipt, "validation receipt");

  assert(typeof receiptPath === "string" && receiptPath.toLowerCase().endsWith(".json"), "receipt path must end with .json");
  const absolutePath = resolve(receiptPath);
  mkdirSync(dirname(absolutePath), { recursive: true });
  writeFileSync(absolutePath, serialized, { encoding: "utf8", flag: "wx", mode: 0o600 });
  return {
    artifact_path: absolutePath,
    receipt_sha256: sha256(serialized),
    receipt,
    serialized,
  };
}

export function buildCapacityDataIntakeReceipt(validationResult, options = {}) {
  assertPlainObject(validationResult, "validation result");
  assert(validationResult.status === "CAPACITY_DATA_INTAKE_VALID", "receipt requires a passed validation result");
  assert(
    validationResult.bundle_status === EXTERNAL_STATUS
      || (options.allowTestOnly === true && validationResult.bundle_status === TEST_STATUS),
    "receipt refuses non-attested validation status",
  );
  assertSafeIdentifier(validationResult.bundle_id, "receipt bundle_id");
  assertSha256(validationResult.manifest_sha256, "receipt manifest_sha256");
  assert(validationResult.source_contract === SOURCE_CONTRACT_PATH, "receipt source_contract drift");
  assertSha256(validationResult.source_contract_sha256, "receipt source_contract_sha256");
  assert(
    validationResult.source_contract_sha256 === sha256File(resolve(REPOSITORY_ROOT, SOURCE_CONTRACT_PATH)),
    "receipt source contract hash does not match current bytes",
  );
  assert(
    validationResult.authority === "METADATA_ONLY_NOT_EXTERNALLY_VERIFIED",
    "receipt authority boundary drift",
  );
  assert(Array.isArray(validationResult.groups) && validationResult.groups.length === 4, "receipt requires four validated groups");

  const validatedAtUtc = options.validatedAtUtc ?? new Date().toISOString();
  parseUtc(validatedAtUtc, "receipt validated_at_utc");
  const receiptStatus = validationResult.bundle_status === TEST_STATUS
    ? "TEST_ONLY_VALIDATION_RECEIPT"
    : "CAPACITY_DATA_INTAKE_VALIDATION_RECEIPT";
  const submissions = validationResult.groups.map(group => ({
    submission_id: group.submission_id,
    data_group: group.data_group,
    schema_version: group.schema_version,
    artifact_sha256: group.artifact_sha256,
    source_system: group.source_system,
    source_version: group.source_version,
    observation_start_utc: group.observation_start_utc,
    observation_end_utc: group.observation_end_utc,
    record_count: group.record_count,
    signer_identity_alias: group.signer_identity_alias,
    signer_role: group.signer_role,
    signer_org: group.signer_org,
    authority_source: group.authority_source,
    signed_at: group.signed_at,
  }));
  const observedGroups = new Set(submissions.map(item => item.data_group));
  assertSetEquals(observedGroups, new Set(GROUP_SCHEMAS.keys()), "receipt data groups");
  const allowTestOnly = validationResult.bundle_status === TEST_STATUS;
  for (const submission of submissions) {
    assertSafeIdentifier(submission.submission_id, "receipt submission_id");
    assert(submission.schema_version === GROUP_SCHEMAS.get(submission.data_group), "receipt group schema drift");
    assertSha256(submission.artifact_sha256, "receipt artifact_sha256");
    assertMeaningfulString(submission.source_system, "receipt source_system", allowTestOnly);
    assertMeaningfulString(submission.source_version, "receipt source_version", allowTestOnly);
    const observationStart = parseUtc(submission.observation_start_utc, "receipt observation_start_utc");
    const observationEnd = parseUtc(submission.observation_end_utc, "receipt observation_end_utc");
    assert(observationEnd > observationStart, "receipt observation window must be positive");
    assertSafeIdentifier(submission.signer_identity_alias, "receipt signer_identity_alias");
    assertMeaningfulString(submission.signer_role, "receipt signer_role", allowTestOnly);
    assertMeaningfulString(submission.signer_org, "receipt signer_org", allowTestOnly);
    assertMeaningfulString(submission.authority_source, "receipt authority_source", allowTestOnly);
    parseSignedAt(submission.signed_at, "receipt signed_at");
    assertNonNegativeInteger(submission.record_count, "receipt record_count");
  }
  const totalRecords = submissions.reduce((sum, submission) => sum + submission.record_count, 0);
  assert(validationResult.total_records === totalRecords, "receipt total_records does not reconcile");

  const receipt = {
    schema_version: RECEIPT_SCHEMA,
    work_id: "W-0156",
    status: receiptStatus,
    validated_at_utc: validatedAtUtc,
    bundle_id: validationResult.bundle_id,
    bundle_status: validationResult.bundle_status,
    bundle_manifest_sha256: validationResult.manifest_sha256,
    source_contract: validationResult.source_contract,
    source_contract_sha256: validationResult.source_contract_sha256,
    validator: {
      path: VALIDATOR_PATH,
      sha256: sha256File(SCRIPT_PATH),
    },
    validation: {
      status: validationResult.status,
      scope: VALIDATION_SCOPE,
      authority: validationResult.authority,
      group_count: submissions.length,
      total_records: validationResult.total_records,
    },
    submissions,
    safety: {
      raw_rows_persisted: false,
      credential_material_persisted: false,
      external_authority_verified: false,
      calibration_status: "NOT_RUN",
      production_gate_promoted: false,
      real_customer_call_allowed: "NO",
    },
    limitations: RECEIPT_LIMITATIONS,
  };
  assertNoSensitiveContent(JSON.stringify(receipt), "validation receipt");
  assertNoForbiddenKeys(receipt, "validation receipt");
  return receipt;
}

export function verifyCapacityDataIntakeReceiptFile(receiptPath, expectedReceiptSha256, options = {}) {
  assertSha256(expectedReceiptSha256, "expected receipt SHA-256");
  assert(typeof receiptPath === "string" && receiptPath.toLowerCase().endsWith(".json"), "receipt path must end with .json");
  const absolutePath = resolve(receiptPath);
  const entry = lstatSync(absolutePath);
  assert(!entry.isSymbolicLink(), "receipt path must not be a symbolic link");
  assert(entry.isFile(), "receipt path must reference a regular file");
  assert(entry.size > 0, "validation receipt is empty");
  assert(entry.size <= MAX_ARTIFACT_BYTES, "validation receipt exceeds 50 MiB");
  const receiptBytes = readFileSync(absolutePath);
  const actualReceiptSha256 = sha256(receiptBytes);
  assert(actualReceiptSha256 === expectedReceiptSha256, "receipt SHA-256 does not match trusted expected hash");
  const receiptRaw = receiptBytes.toString("utf8");
  assert(Buffer.from(receiptRaw, "utf8").equals(receiptBytes), "validation receipt must be valid UTF-8");
  assertNoSensitiveContent(receiptRaw, "validation receipt");
  const receipt = parseJson(receiptRaw, "validation receipt");
  assertNoForbiddenKeys(receipt, "validation receipt");
  assertExactKeys(receipt, RECEIPT_KEYS, "validation receipt");

  assert(receipt.schema_version === RECEIPT_SCHEMA, "receipt schema_version is unsupported");
  assert(receipt.work_id === "W-0156", "receipt work_id must reference W-0156");
  const allowTestOnly = options.allowTestOnly === true;
  const isExternalReceipt = receipt.status === "CAPACITY_DATA_INTAKE_VALIDATION_RECEIPT"
    && receipt.bundle_status === EXTERNAL_STATUS;
  const isTestReceipt = receipt.status === "TEST_ONLY_VALIDATION_RECEIPT"
    && receipt.bundle_status === TEST_STATUS;
  assert(isExternalReceipt || (allowTestOnly && isTestReceipt), "receipt status is not externally attested");
  parseUtc(receipt.validated_at_utc, "receipt validated_at_utc");
  assertSafeIdentifier(receipt.bundle_id, "receipt bundle_id");
  assertSha256(receipt.bundle_manifest_sha256, "receipt bundle_manifest_sha256");

  assert(receipt.source_contract === SOURCE_CONTRACT_PATH, "receipt source_contract path drift");
  assertSha256(receipt.source_contract_sha256, "receipt source_contract_sha256");
  const currentSourceContractSha256 = sha256File(resolve(REPOSITORY_ROOT, SOURCE_CONTRACT_PATH));
  assert(receipt.source_contract_sha256 === currentSourceContractSha256, "receipt no longer matches current source contract");

  assertExactKeys(receipt.validator, ["path", "sha256"], "receipt validator");
  assert(receipt.validator.path === VALIDATOR_PATH, "receipt validator path drift");
  assertSha256(receipt.validator.sha256, "receipt validator sha256");
  const currentValidatorSha256 = sha256File(SCRIPT_PATH);
  assert(receipt.validator.sha256 === currentValidatorSha256, "receipt no longer matches current validator");

  assertExactKeys(receipt.validation, ["status", "scope", "authority", "group_count", "total_records"], "receipt validation summary");
  assert(receipt.validation.status === "CAPACITY_DATA_INTAKE_VALID", "receipt validation status is not PASS");
  assertExactArray(receipt.validation.scope, VALIDATION_SCOPE, "receipt validation scope");
  assert(receipt.validation.authority === "METADATA_ONLY_NOT_EXTERNALLY_VERIFIED", "receipt authority boundary drift");
  assert(receipt.validation.group_count === GROUP_SCHEMAS.size, "receipt group_count must be four");
  assertPositiveInteger(receipt.validation.total_records, "receipt total_records");

  assert(Array.isArray(receipt.submissions), "receipt submissions must be an array");
  assert(receipt.submissions.length === GROUP_SCHEMAS.size, "receipt must contain exactly four submissions");
  const expectedGroupOrder = [...GROUP_SCHEMAS.keys()].sort((left, right) => left.localeCompare(right));
  const submissionIds = new Set();
  const observedGroups = new Set();
  let totalRecords = 0;
  for (const [index, submission] of receipt.submissions.entries()) {
    assertExactKeys(submission, RECEIPT_SUBMISSION_KEYS, "receipt submission");
    assertSafeIdentifier(submission.submission_id, "receipt submission_id");
    assert(!submissionIds.has(submission.submission_id), "duplicate receipt submission_id");
    submissionIds.add(submission.submission_id);
    assert(submission.data_group === expectedGroupOrder[index], "receipt submissions are not in canonical group order");
    assert(!observedGroups.has(submission.data_group), "duplicate receipt data_group");
    observedGroups.add(submission.data_group);
    assert(submission.schema_version === GROUP_SCHEMAS.get(submission.data_group), "receipt group schema drift");
    assertSha256(submission.artifact_sha256, "receipt artifact_sha256");
    assertMeaningfulString(submission.source_system, "receipt source_system", isTestReceipt);
    assertMeaningfulString(submission.source_version, "receipt source_version", isTestReceipt);
    const observationStart = parseUtc(submission.observation_start_utc, "receipt observation_start_utc");
    const observationEnd = parseUtc(submission.observation_end_utc, "receipt observation_end_utc");
    assert(observationEnd > observationStart, "receipt observation window must be positive");
    assertPositiveInteger(submission.record_count, "receipt record_count");
    totalRecords += submission.record_count;
    assertSafeIdentifier(submission.signer_identity_alias, "receipt signer_identity_alias");
    assertMeaningfulString(submission.signer_role, "receipt signer_role", isTestReceipt);
    assertMeaningfulString(submission.signer_org, "receipt signer_org", isTestReceipt);
    assertMeaningfulString(submission.authority_source, "receipt authority_source", isTestReceipt);
    parseSignedAt(submission.signed_at, "receipt signed_at");
  }
  assertSetEquals(observedGroups, new Set(GROUP_SCHEMAS.keys()), "receipt data-group completeness");
  assert(totalRecords === receipt.validation.total_records, "receipt total_records does not reconcile");

  assertExactKeys(
    receipt.safety,
    [
      "raw_rows_persisted",
      "credential_material_persisted",
      "external_authority_verified",
      "calibration_status",
      "production_gate_promoted",
      "real_customer_call_allowed",
    ],
    "receipt safety",
  );
  assert(receipt.safety.raw_rows_persisted === false, "receipt claims raw rows were persisted");
  assert(receipt.safety.credential_material_persisted === false, "receipt claims credential material was persisted");
  assert(receipt.safety.external_authority_verified === false, "receipt inferred external authority verification");
  assert(receipt.safety.calibration_status === "NOT_RUN", "receipt calibration status drift");
  assert(receipt.safety.production_gate_promoted === false, "receipt promoted the production gate");
  assert(receipt.safety.real_customer_call_allowed === "NO", "receipt real-call safety boundary drift");
  assertExactArray(receipt.limitations, RECEIPT_LIMITATIONS, "receipt limitations");

  return {
    status: "CAPACITY_DATA_INTAKE_RECEIPT_VALID",
    authority: receipt.validation.authority,
    receipt_path: absolutePath,
    receipt_sha256: actualReceiptSha256,
    receipt_status: receipt.status,
    bundle_status: receipt.bundle_status,
    bundle_id: receipt.bundle_id,
    bundle_manifest_sha256: receipt.bundle_manifest_sha256,
    group_count: receipt.validation.group_count,
    total_records: receipt.validation.total_records,
    validator_sha256: currentValidatorSha256,
    source_contract_sha256: currentSourceContractSha256,
  };
}

export function appendVerifiedCapacityReceiptToLedger(
  receiptPath,
  expectedReceiptSha256,
  ledgerPath,
  options = {},
) {
  const verification = verifyCapacityDataIntakeReceiptFile(
    receiptPath,
    expectedReceiptSha256,
    { allowTestOnly: options.allowTestOnly === true },
  );
  assert(typeof ledgerPath === "string" && ledgerPath.toLowerCase().endsWith(".jsonl"), "ledger path must end with .jsonl");
  const appendedAtUtc = options.appendedAtUtc ?? new Date().toISOString();
  parseUtc(appendedAtUtc, "ledger appended_at_utc");

  const absoluteLedgerPath = resolve(ledgerPath);
  mkdirSync(dirname(absoluteLedgerPath), { recursive: true });
  const lockPath = `${absoluteLedgerPath}.lock`;
  let lockDescriptor;
  let lockAcquired = false;
  try {
    try {
      lockDescriptor = openSync(lockPath, "wx", 0o600);
      lockAcquired = true;
    } catch (error) {
      if (error && typeof error === "object" && error.code === "EEXIST") {
        throw new Error("intake ledger is locked by another writer");
      }
      throw error;
    }

    const ledger = readAndValidateCapacityIntakeLedger(absoluteLedgerPath, {
      allowTestOnly: options.allowTestOnly === true,
    });
    const duplicate = ledger.entries.find(item => item.entry.idempotency_key === verification.receipt_sha256);
    if (duplicate) {
      return {
        status: "CAPACITY_DATA_INTAKE_LEDGER_ALREADY_PRESENT",
        appended: false,
        idempotency_key: verification.receipt_sha256,
        entry_sha256: duplicate.entry_sha256,
        entry_count: ledger.entries.length,
        ledger_path: absoluteLedgerPath,
        ledger_sha256: ledger.ledger_sha256,
      };
    }

    const previousEntrySha256 = ledger.entries.length === 0
      ? null
      : ledger.entries.at(-1).entry_sha256;
    const entry = buildCapacityIntakeLedgerEntry(verification, previousEntrySha256, appendedAtUtc);
    const serialized = `${JSON.stringify(entry)}\n`;
    assertNoSensitiveContent(serialized, "intake ledger entry");
    assertNoForbiddenKeys(entry, "intake ledger entry");
    validateCapacityIntakeLedgerEntry(entry, previousEntrySha256, {
      allowTestOnly: options.allowTestOnly === true,
    });
    assert(
      ledger.byte_length + Buffer.byteLength(serialized, "utf8") <= MAX_LEDGER_BYTES,
      "intake ledger append would exceed 50 MiB",
    );

    if (existsSync(absoluteLedgerPath)) assertRegularLedgerFile(absoluteLedgerPath);
    const ledgerDescriptor = openSync(absoluteLedgerPath, "a", 0o600);
    try {
      writeFileSync(ledgerDescriptor, serialized, "utf8");
      fsyncSync(ledgerDescriptor);
    } finally {
      closeSync(ledgerDescriptor);
    }
    return {
      status: "CAPACITY_DATA_INTAKE_LEDGER_APPENDED",
      appended: true,
      idempotency_key: verification.receipt_sha256,
      entry_sha256: sha256(serialized),
      entry_count: ledger.entries.length + 1,
      ledger_path: absoluteLedgerPath,
      ledger_sha256: sha256File(absoluteLedgerPath),
    };
  } finally {
    if (lockDescriptor !== undefined) closeSync(lockDescriptor);
    if (lockAcquired) unlinkSync(lockPath);
  }
}

function buildCapacityIntakeLedgerEntry(verification, previousEntrySha256, appendedAtUtc) {
  assertPlainObject(verification, "receipt verification result");
  assert(verification.status === "CAPACITY_DATA_INTAKE_RECEIPT_VALID", "ledger requires a passed W-0157 verification result");
  assertSha256(verification.receipt_sha256, "ledger receipt_sha256");
  assertSafeIdentifier(verification.bundle_id, "ledger bundle_id");
  assertSha256(verification.bundle_manifest_sha256, "ledger bundle_manifest_sha256");
  assertSha256(verification.validator_sha256, "ledger validator_sha256");
  assertSha256(verification.source_contract_sha256, "ledger source_contract_sha256");
  assert(verification.group_count === GROUP_SCHEMAS.size, "ledger verification group_count must be four");
  assertPositiveInteger(verification.total_records, "ledger verification total_records");
  if (previousEntrySha256 !== null) assertSha256(previousEntrySha256, "ledger previous_entry_sha256");
  const isTestReceipt = verification.receipt_status === "TEST_ONLY_VALIDATION_RECEIPT"
    && verification.bundle_status === TEST_STATUS;
  const status = isTestReceipt ? "TEST_ONLY_LEDGER_ENTRY" : "CAPACITY_DATA_INTAKE_LEDGER_ENTRY";
  return {
    schema_version: LEDGER_ENTRY_SCHEMA,
    work_id: "W-0158",
    status,
    appended_at_utc: appendedAtUtc,
    idempotency_key: verification.receipt_sha256,
    receipt_sha256: verification.receipt_sha256,
    receipt_status: verification.receipt_status,
    bundle_id: verification.bundle_id,
    bundle_manifest_sha256: verification.bundle_manifest_sha256,
    verification: {
      work_id: "W-0157",
      status: verification.status,
      validator_sha256: verification.validator_sha256,
      source_contract_sha256: verification.source_contract_sha256,
      authority: verification.authority,
      group_count: verification.group_count,
      total_records: verification.total_records,
    },
    previous_entry_sha256: previousEntrySha256,
    safety: {
      raw_rows_persisted: false,
      receipt_path_persisted: false,
      submission_metadata_persisted: false,
      credential_material_persisted: false,
      external_authority_verified: false,
      calibration_status: "NOT_RUN",
      production_gate_promoted: false,
      real_customer_call_allowed: "NO",
    },
  };
}

function readAndValidateCapacityIntakeLedger(ledgerPath, options = {}) {
  if (!existsSync(ledgerPath)) {
    return { entries: [], ledger_sha256: null, byte_length: 0 };
  }
  const entry = assertRegularLedgerFile(ledgerPath);
  assert(entry.size > 0, "existing intake ledger is empty");
  assert(entry.size <= MAX_LEDGER_BYTES, "intake ledger exceeds 50 MiB");
  const bytes = readFileSync(ledgerPath);
  const raw = bytes.toString("utf8");
  assert(Buffer.from(raw, "utf8").equals(bytes), "intake ledger must be valid UTF-8");
  assert(raw.endsWith("\n"), "intake ledger must end with LF");
  assertNoSensitiveContent(raw, "intake ledger");
  const lines = raw.slice(0, -1).split("\n");
  assert(lines.every(line => line.length > 0 && !line.includes("\r")), "intake ledger contains a blank or non-LF line");
  const entries = [];
  const idempotencyKeys = new Set();
  let expectedPreviousEntrySha256 = null;
  for (const [index, line] of lines.entries()) {
    const parsed = parseJson(line, `intake ledger entry ${index + 1}`);
    assert(line === JSON.stringify(parsed), `intake ledger entry ${index + 1} is not canonical JSON`);
    assertNoForbiddenKeys(parsed, `intake ledger entry ${index + 1}`);
    validateCapacityIntakeLedgerEntry(parsed, expectedPreviousEntrySha256, options);
    assert(!idempotencyKeys.has(parsed.idempotency_key), "intake ledger contains a duplicate idempotency key");
    idempotencyKeys.add(parsed.idempotency_key);
    const entrySha256 = sha256(`${line}\n`);
    entries.push({ entry: parsed, entry_sha256: entrySha256 });
    expectedPreviousEntrySha256 = entrySha256;
  }
  return { entries, ledger_sha256: sha256(bytes), byte_length: bytes.length };
}

function assertRegularLedgerFile(ledgerPath) {
  const entry = lstatSync(ledgerPath);
  assert(!entry.isSymbolicLink(), "ledger path must not be a symbolic link");
  assert(entry.isFile(), "ledger path must reference a regular file");
  return entry;
}

function validateCapacityIntakeLedgerEntry(entry, expectedPreviousEntrySha256, options = {}) {
  assertExactKeys(entry, LEDGER_ENTRY_KEYS, "intake ledger entry");
  assert(entry.schema_version === LEDGER_ENTRY_SCHEMA, "intake ledger entry schema_version is unsupported");
  assert(entry.work_id === "W-0158", "intake ledger entry work_id must reference W-0158");
  const isExternalEntry = entry.status === "CAPACITY_DATA_INTAKE_LEDGER_ENTRY"
    && entry.receipt_status === "CAPACITY_DATA_INTAKE_VALIDATION_RECEIPT";
  const isTestEntry = entry.status === "TEST_ONLY_LEDGER_ENTRY"
    && entry.receipt_status === "TEST_ONLY_VALIDATION_RECEIPT";
  assert(isExternalEntry || (options.allowTestOnly === true && isTestEntry), "intake ledger entry status is not external");
  parseUtc(entry.appended_at_utc, "ledger appended_at_utc");
  assertSha256(entry.idempotency_key, "ledger idempotency_key");
  assertSha256(entry.receipt_sha256, "ledger receipt_sha256");
  assert(entry.idempotency_key === entry.receipt_sha256, "ledger idempotency key must equal receipt SHA-256");
  assertSafeIdentifier(entry.bundle_id, "ledger bundle_id");
  assertSha256(entry.bundle_manifest_sha256, "ledger bundle_manifest_sha256");
  if (expectedPreviousEntrySha256 === null) {
    assert(entry.previous_entry_sha256 === null, "first ledger entry must have null previous_entry_sha256");
  } else {
    assertSha256(entry.previous_entry_sha256, "ledger previous_entry_sha256");
    assert(entry.previous_entry_sha256 === expectedPreviousEntrySha256, "intake ledger hash chain is broken");
  }

  assertExactKeys(entry.verification, LEDGER_VERIFICATION_KEYS, "ledger verification summary");
  assert(entry.verification.work_id === "W-0157", "ledger verification work_id must reference W-0157");
  assert(entry.verification.status === "CAPACITY_DATA_INTAKE_RECEIPT_VALID", "ledger verification status is not PASS");
  assertSha256(entry.verification.validator_sha256, "ledger validator_sha256");
  assertSha256(entry.verification.source_contract_sha256, "ledger source_contract_sha256");
  assert(entry.verification.authority === "METADATA_ONLY_NOT_EXTERNALLY_VERIFIED", "ledger authority boundary drift");
  assert(entry.verification.group_count === GROUP_SCHEMAS.size, "ledger group_count must be four");
  assertPositiveInteger(entry.verification.total_records, "ledger total_records");

  assertExactKeys(entry.safety, LEDGER_SAFETY_KEYS, "ledger safety");
  assert(entry.safety.raw_rows_persisted === false, "ledger claims raw rows were persisted");
  assert(entry.safety.receipt_path_persisted === false, "ledger claims receipt path was persisted");
  assert(entry.safety.submission_metadata_persisted === false, "ledger claims submission metadata was persisted");
  assert(entry.safety.credential_material_persisted === false, "ledger claims credential material was persisted");
  assert(entry.safety.external_authority_verified === false, "ledger inferred external authority verification");
  assert(entry.safety.calibration_status === "NOT_RUN", "ledger calibration status drift");
  assert(entry.safety.production_gate_promoted === false, "ledger promoted the production gate");
  assert(entry.safety.real_customer_call_allowed === "NO", "ledger real-call safety boundary drift");
}

export function writeCapacityIntakeLedgerHeadCheckpoint(
  ledgerPath,
  ledgerId,
  checkpointPath,
  options = {},
) {
  assert(
    typeof ledgerPath === "string" && ledgerPath.toLowerCase().endsWith(".jsonl"),
    "ledger path must end with .jsonl",
  );
  assertSafeIdentifier(ledgerId, "checkpoint ledger_id");
  const allowTestOnly = options.allowTestOnly === true;
  const ledger = readAndValidateCapacityIntakeLedger(resolve(ledgerPath), { allowTestOnly });
  assert(ledger.entries.length > 0, "cannot checkpoint an empty intake ledger");
  const allExternal = ledger.entries.every(item => item.entry.status === "CAPACITY_DATA_INTAKE_LEDGER_ENTRY");
  const allTest = ledger.entries.every(item => item.entry.status === "TEST_ONLY_LEDGER_ENTRY");
  assert(allExternal || (allowTestOnly && allTest), "checkpoint refuses mixed or non-external ledger entries");
  const checkpointedAtUtc = options.checkpointedAtUtc ?? new Date().toISOString();
  parseUtc(checkpointedAtUtc, "checkpointed_at_utc");
  const last = ledger.entries.at(-1);
  const checkpoint = {
    schema_version: LEDGER_CHECKPOINT_SCHEMA,
    work_id: "W-0159",
    status: allTest ? "TEST_ONLY_LEDGER_HEAD_CHECKPOINT" : "CAPACITY_DATA_INTAKE_LEDGER_HEAD_CHECKPOINT",
    checkpointed_at_utc: checkpointedAtUtc,
    ledger_id: ledgerId,
    ledger_entry_schema_version: LEDGER_ENTRY_SCHEMA,
    entry_count: ledger.entries.length,
    ledger_sha256: ledger.ledger_sha256,
    last_entry_sha256: last.entry_sha256,
    last_receipt_sha256: last.entry.receipt_sha256,
    source_contract_sha256: last.entry.verification.source_contract_sha256,
    checkpoint_validator_sha256: sha256File(SCRIPT_PATH),
    authority: "METADATA_ONLY_NOT_EXTERNALLY_VERIFIED",
    safety: {
      raw_rows_persisted: false,
      ledger_path_persisted: false,
      submission_metadata_persisted: false,
      credential_material_persisted: false,
      external_trust_store_verified: false,
      external_authority_verified: false,
      calibration_status: "NOT_RUN",
      production_gate_promoted: false,
      real_customer_call_allowed: "NO",
    },
    limitations: LEDGER_CHECKPOINT_LIMITATIONS,
  };
  validateCapacityIntakeLedgerHeadCheckpoint(checkpoint, { allowTestOnly });
  const serialized = `${JSON.stringify(checkpoint, null, 2)}\n`;
  assertNoSensitiveContent(serialized, "ledger-head checkpoint");
  assertNoForbiddenKeys(checkpoint, "ledger-head checkpoint");
  assert(
    typeof checkpointPath === "string" && checkpointPath.toLowerCase().endsWith(".json"),
    "checkpoint path must end with .json",
  );
  const absoluteCheckpointPath = resolve(checkpointPath);
  mkdirSync(dirname(absoluteCheckpointPath), { recursive: true });
  writeFileSync(absoluteCheckpointPath, serialized, { encoding: "utf8", flag: "wx", mode: 0o600 });
  return {
    status: "CAPACITY_DATA_INTAKE_LEDGER_CHECKPOINT_WRITTEN",
    checkpoint_path: absoluteCheckpointPath,
    checkpoint_sha256: sha256(serialized),
    ledger_id: checkpoint.ledger_id,
    entry_count: checkpoint.entry_count,
    ledger_sha256: checkpoint.ledger_sha256,
    last_entry_sha256: checkpoint.last_entry_sha256,
    last_receipt_sha256: checkpoint.last_receipt_sha256,
    checkpoint,
  };
}

export function verifyCapacityIntakeLedgerHeadCheckpoint(
  ledgerPath,
  checkpointPath,
  expectedCheckpointSha256,
  options = {},
) {
  assertSha256(expectedCheckpointSha256, "expected checkpoint SHA-256");
  assert(
    typeof ledgerPath === "string" && ledgerPath.toLowerCase().endsWith(".jsonl"),
    "ledger path must end with .jsonl",
  );
  assert(
    typeof checkpointPath === "string" && checkpointPath.toLowerCase().endsWith(".json"),
    "checkpoint path must end with .json",
  );
  const absoluteCheckpointPath = resolve(checkpointPath);
  const checkpointEntry = lstatSync(absoluteCheckpointPath);
  assert(!checkpointEntry.isSymbolicLink(), "checkpoint path must not be a symbolic link");
  assert(checkpointEntry.isFile(), "checkpoint path must reference a regular file");
  assert(checkpointEntry.size > 0, "ledger-head checkpoint is empty");
  assert(checkpointEntry.size <= MAX_ARTIFACT_BYTES, "ledger-head checkpoint exceeds 50 MiB");
  const checkpointBytes = readFileSync(absoluteCheckpointPath);
  const actualCheckpointSha256 = sha256(checkpointBytes);
  assert(
    actualCheckpointSha256 === expectedCheckpointSha256,
    "checkpoint SHA-256 does not match trusted expected hash",
  );
  const checkpointRaw = checkpointBytes.toString("utf8");
  assert(Buffer.from(checkpointRaw, "utf8").equals(checkpointBytes), "ledger-head checkpoint must be valid UTF-8");
  assertNoSensitiveContent(checkpointRaw, "ledger-head checkpoint");
  const checkpoint = parseJson(checkpointRaw, "ledger-head checkpoint");
  assertNoForbiddenKeys(checkpoint, "ledger-head checkpoint");
  const allowTestOnly = options.allowTestOnly === true;
  validateCapacityIntakeLedgerHeadCheckpoint(checkpoint, { allowTestOnly });

  const absoluteLedgerPath = resolve(ledgerPath);
  const ledger = readAndValidateCapacityIntakeLedger(absoluteLedgerPath, { allowTestOnly });
  assert(ledger.entries.length > 0, "checkpoint verification refuses an empty intake ledger");
  const last = ledger.entries.at(-1);
  const ledgerIsTestOnly = ledger.entries.every(item => item.entry.status === "TEST_ONLY_LEDGER_ENTRY");
  const checkpointIsTestOnly = checkpoint.status === "TEST_ONLY_LEDGER_HEAD_CHECKPOINT";
  assert(ledgerIsTestOnly === checkpointIsTestOnly, "checkpoint and ledger execution modes do not match");
  assert(ledger.ledger_sha256 === checkpoint.ledger_sha256, "intake ledger hash does not match trusted checkpoint");
  assert(ledger.entries.length === checkpoint.entry_count, "intake ledger entry count does not match trusted checkpoint");
  assert(last.entry_sha256 === checkpoint.last_entry_sha256, "intake ledger head does not match trusted checkpoint");
  assert(last.entry.receipt_sha256 === checkpoint.last_receipt_sha256, "last receipt hash does not match trusted checkpoint");
  assert(
    last.entry.verification.source_contract_sha256 === checkpoint.source_contract_sha256,
    "ledger source contract does not match trusted checkpoint",
  );
  return {
    status: "CAPACITY_DATA_INTAKE_LEDGER_CHECKPOINT_VERIFY_PASS",
    authority: checkpoint.authority,
    checkpoint_sha256: actualCheckpointSha256,
    ledger_id: checkpoint.ledger_id,
    entry_count: checkpoint.entry_count,
    ledger_sha256: checkpoint.ledger_sha256,
    last_entry_sha256: checkpoint.last_entry_sha256,
    last_receipt_sha256: checkpoint.last_receipt_sha256,
  };
}

function validateCapacityIntakeLedgerHeadCheckpoint(checkpoint, options = {}) {
  assertExactKeys(checkpoint, LEDGER_CHECKPOINT_KEYS, "ledger-head checkpoint");
  assert(checkpoint.schema_version === LEDGER_CHECKPOINT_SCHEMA, "ledger-head checkpoint schema_version is unsupported");
  assert(checkpoint.work_id === "W-0159", "ledger-head checkpoint work_id must reference W-0159");
  const isExternal = checkpoint.status === "CAPACITY_DATA_INTAKE_LEDGER_HEAD_CHECKPOINT";
  const isTest = checkpoint.status === "TEST_ONLY_LEDGER_HEAD_CHECKPOINT";
  assert(isExternal || (options.allowTestOnly === true && isTest), "ledger-head checkpoint status is not external");
  parseUtc(checkpoint.checkpointed_at_utc, "checkpointed_at_utc");
  assertSafeIdentifier(checkpoint.ledger_id, "checkpoint ledger_id");
  assert(
    checkpoint.ledger_entry_schema_version === LEDGER_ENTRY_SCHEMA,
    "checkpoint ledger entry schema_version drift",
  );
  assertPositiveInteger(checkpoint.entry_count, "checkpoint entry_count");
  assertSha256(checkpoint.ledger_sha256, "checkpoint ledger_sha256");
  assertSha256(checkpoint.last_entry_sha256, "checkpoint last_entry_sha256");
  assertSha256(checkpoint.last_receipt_sha256, "checkpoint last_receipt_sha256");
  assertSha256(checkpoint.source_contract_sha256, "checkpoint source_contract_sha256");
  assertSha256(checkpoint.checkpoint_validator_sha256, "checkpoint validator_sha256");
  assert(checkpoint.authority === "METADATA_ONLY_NOT_EXTERNALLY_VERIFIED", "checkpoint authority boundary drift");
  assertExactKeys(checkpoint.safety, LEDGER_CHECKPOINT_SAFETY_KEYS, "checkpoint safety");
  assert(checkpoint.safety.raw_rows_persisted === false, "checkpoint claims raw rows were persisted");
  assert(checkpoint.safety.ledger_path_persisted === false, "checkpoint claims ledger path was persisted");
  assert(checkpoint.safety.submission_metadata_persisted === false, "checkpoint claims submission metadata was persisted");
  assert(checkpoint.safety.credential_material_persisted === false, "checkpoint claims credential material was persisted");
  assert(checkpoint.safety.external_trust_store_verified === false, "checkpoint inferred external trust-store verification");
  assert(checkpoint.safety.external_authority_verified === false, "checkpoint inferred external authority verification");
  assert(checkpoint.safety.calibration_status === "NOT_RUN", "checkpoint calibration status drift");
  assert(checkpoint.safety.production_gate_promoted === false, "checkpoint promoted the production gate");
  assert(checkpoint.safety.real_customer_call_allowed === "NO", "checkpoint real-call safety boundary drift");
  assertExactArray(checkpoint.limitations, LEDGER_CHECKPOINT_LIMITATIONS, "checkpoint limitations");
}

function validateSubmissionEnvelope(submission, allowTestOnly) {
  assertSafeIdentifier(submission.submission_id, "submission_id");
  assert(GROUP_SCHEMAS.has(submission.data_group), "data_group is unsupported");
  assert(submission.schema_version === GROUP_SCHEMAS.get(submission.data_group), "group schema_version mismatch");
  assert(submission.artifact_format === "JSON", "artifact_format must be JSON");
  assertSafeRelativePath(submission.artifact_path, "artifact_path");
  assertSha256(submission.artifact_sha256, "artifact_sha256");
  assertMeaningfulString(submission.source_system, "source_system", allowTestOnly);
  assertMeaningfulString(submission.source_version, "source_version", allowTestOnly);
  const start = parseUtc(submission.observation_start_utc, "observation_start_utc");
  const end = parseUtc(submission.observation_end_utc, "observation_end_utc");
  assert(end > start, "submission observation window must be positive");
  assertMeaningfulString(submission.timezone_context, "timezone_context", allowTestOnly);
  assertNonNegativeInteger(submission.record_count, "record_count");
  assert(submission.record_count > 0, "record_count must be positive");
  assertMeaningfulString(submission.filtering_rule, "filtering_rule", allowTestOnly);
  assert(submission.pii_statement === "PII_SAFE", "pii_statement must be PII_SAFE");
  assertSafeIdentifier(submission.signer_identity, "signer_identity");
  assertMeaningfulString(submission.signer_role, "signer_role", allowTestOnly);
  assertMeaningfulString(submission.signer_org, "signer_org", allowTestOnly);
  assertMeaningfulString(submission.authority_source, "authority_source", allowTestOnly);
  parseSignedAt(submission.signed_at, "signed_at");
  assertMeaningfulString(submission.limitations, "limitations", allowTestOnly, { allowNone: true });
  assertNoForbiddenKeys(submission, "submission envelope");
}

function loadArtifact(root, submission) {
  const candidate = resolve(root, submission.artifact_path);
  const relativePath = relative(root, candidate);
  assert(relativePath !== "" && !relativePath.startsWith(`..${sep}`) && relativePath !== "..", "artifact_path escapes bundle root");
  const entry = lstatSync(candidate);
  assert(!entry.isSymbolicLink(), "artifact_path must not be a symbolic link");
  assert(entry.isFile(), "artifact_path must reference a regular file");
  const realCandidate = realpathSync(candidate);
  assert(realCandidate.startsWith(`${root}${sep}`), "artifact real path escapes bundle root");
  const raw = readBoundedFile(realCandidate, "submission artifact");
  assert(sha256(raw) === submission.artifact_sha256, "artifact SHA-256 mismatch");
  assertNoSensitiveContent(raw, "submission artifact");
  const artifact = parseJson(raw, "submission artifact");
  assertNoForbiddenKeys(artifact, "submission artifact");
  return artifact;
}

function validateArtifact(submission, artifact) {
  assertPlainObject(artifact, "submission artifact");
  assert(artifact.schema_version === submission.schema_version, "artifact schema_version mismatch");
  assert(artifact.data_group === submission.data_group, "artifact data_group mismatch");
  switch (submission.data_group) {
    case "TIMING":
      return validateTimingArtifact(submission, artifact);
    case "ARRIVAL":
      return validateArrivalArtifact(submission, artifact);
    case "POLICY_OUTCOME":
      return validatePolicyOutcomeArtifact(submission, artifact);
    case "INFRA_RESERVE":
      return validateInfraReserveArtifact(submission, artifact);
    default:
      throw new Error("unsupported data group");
  }
}

function validateTimingArtifact(submission, artifact) {
  assertExactKeys(artifact, ["schema_version", "data_group", "rows"], "TIMING artifact");
  assertNonEmptyArray(artifact.rows, "TIMING rows");
  const programmes = new Set();
  const identities = new Set();
  for (const row of artifact.rows) {
    assertExactKeys(row, TIMING_ROW_KEYS, "TIMING row");
    assertSafeIdentifier(row.run_label, "TIMING run_label");
    assertSafeIdentifier(row.attempt_label, "TIMING attempt_label");
    assertProgram(row.programme, "TIMING programme");
    programmes.add(row.programme);
    assert(row.execution_mode === "LAB_REAL_SIM", "TIMING execution_mode must be LAB_REAL_SIM");
    assertSafeIdentifier(row.carrier_label, "TIMING carrier_label");
    assertSafeIdentifier(row.cdr_correlation_ref, "TIMING cdr_correlation_ref");
    assert(ENUM_PATTERN.test(row.scenario), "TIMING scenario must be a normalized enum");
    for (const key of ["gateway_model", "firmware_version", "codec_profile"]) {
      assertMeaningfulString(row[key], `TIMING ${key}`, false);
    }
    assert(ENUM_PATTERN.test(row.disposition), "TIMING disposition must be a normalized enum");
    const start = parseUtc(row.started_at_utc, "TIMING started_at_utc");
    const end = parseUtc(row.ended_at_utc, "TIMING ended_at_utc");
    const available = parseUtc(row.available_again_at_utc, "TIMING available_again_at_utc");
    assert(end >= start, "TIMING ended_at_utc precedes started_at_utc");
    assert(available >= end, "TIMING available_again_at_utc precedes ended_at_utc");
    assertPositiveInteger(row.occupancy_ms, "TIMING occupancy_ms");
    assertNonNegativeInteger(row.cooldown_ms, "TIMING cooldown_ms");
    assertPositiveInteger(row.full_cycle_ms, "TIMING full_cycle_ms");
    assert(row.occupancy_ms === end - start, "TIMING occupancy_ms invariant failed");
    assert(row.cooldown_ms === available - end, "TIMING cooldown_ms invariant failed");
    assert(row.full_cycle_ms === row.occupancy_ms + row.cooldown_ms, "TIMING full_cycle_ms invariant failed");
    assertWithinWindow(start, available, submission, "TIMING row");
    const identity = `${row.run_label}\u0000${row.attempt_label}`;
    assert(!identities.has(identity), "duplicate TIMING run/attempt identity");
    identities.add(identity);
  }
  assertProgrammesComplete(programmes, "TIMING");
  return artifact.rows.length;
}

function validateArrivalArtifact(submission, artifact) {
  assertExactKeys(artifact, ["schema_version", "data_group", "rows"], "ARRIVAL artifact");
  assertNonEmptyArray(artifact.rows, "ARRIVAL rows");
  const programmes = new Set();
  const grouped = new Map();
  for (const row of artifact.rows) {
    assertExactKeys(row, ARRIVAL_ROW_KEYS, "ARRIVAL row");
    assertSafeIdentifier(row.dataset_id, "ARRIVAL dataset_id");
    assertProgram(row.programme, "ARRIVAL programme");
    programmes.add(row.programme);
    assertMeaningfulString(row.session_definition_id, "ARRIVAL session_definition_id", false);
    assertMeaningfulString(row.business_timezone, "ARRIVAL business_timezone", false);
    assertMeaningfulString(row.source_query_version, "ARRIVAL source_query_version", false);
    assertMeaningfulString(row.eligibility_filter_version, "ARRIVAL eligibility_filter_version", false);
    assert(row.data_quality_flag === "OK", "ARRIVAL data_quality_flag must be OK");
    assertNonNegativeInteger(row.eligible_order_count, "ARRIVAL eligible_order_count");
    const start = parseUtc(row.bucket_start_utc, "ARRIVAL bucket_start_utc");
    const end = parseUtc(row.bucket_end_utc, "ARRIVAL bucket_end_utc");
    const duration = end - start;
    assert(duration > 0 && duration <= 300_000, "ARRIVAL bucket must be positive and at most five minutes");
    assert(300_000 % duration === 0 && 900_000 % duration === 0, "ARRIVAL bucket cannot reconstruct rolling 5m/15m windows");
    assertWithinWindow(start, end, submission, "ARRIVAL row");
    const key = `${row.dataset_id}\u0000${row.programme}`;
    const group = grouped.get(key) ?? [];
    group.push({ start, end });
    grouped.set(key, group);
  }
  for (const windows of grouped.values()) {
    windows.sort((left, right) => left.start - right.start);
    for (let index = 1; index < windows.length; index += 1) {
      assert(windows[index].start === windows[index - 1].end, "ARRIVAL buckets contain a gap or overlap");
    }
  }
  assertProgrammesComplete(programmes, "ARRIVAL");
  return artifact.rows.length;
}

function validatePolicyOutcomeArtifact(submission, artifact) {
  assertExactKeys(artifact, ["schema_version", "data_group", "policy_rows", "outcome_rows"], "POLICY_OUTCOME artifact");
  assertNonEmptyArray(artifact.policy_rows, "POLICY_OUTCOME policy_rows");
  assertNonEmptyArray(artifact.outcome_rows, "POLICY_OUTCOME outcome_rows");
  assert(artifact.policy_rows.length === PROGRAMMES.length, "POLICY_OUTCOME must contain exactly two policy rows");
  const policies = new Map();
  const bundleHashes = new Set();
  for (const row of artifact.policy_rows) {
    assertExactKeys(row, POLICY_ROW_KEYS, "POLICY_OUTCOME policy row");
    assertProgram(row.programme, "policy programme");
    assert(row.execution_mode === "PRODUCTION", "policy execution_mode must be PRODUCTION");
    assertMeaningfulString(row.policy_version, "policy_version", false);
    assertPositiveInteger(row.max_customer_attempts, "max_customer_attempts");
    assert(Array.isArray(row.offsets_seconds), "offsets_seconds must be an array");
    assert(row.offsets_seconds.length === row.max_customer_attempts, "offset count must equal max_customer_attempts");
    row.offsets_seconds.forEach((value, index) => assertNonNegativeInteger(value, `offsets_seconds[${index}]`));
    assert(row.offsets_seconds[0] === 0, "first attempt offset must be zero");
    for (let index = 1; index < row.offsets_seconds.length; index += 1) {
      assert(row.offsets_seconds[index] > row.offsets_seconds[index - 1], "attempt offsets must be strictly increasing");
    }
    assertPositiveInteger(row.confirmation_window_seconds, "confirmation_window_seconds");
    assert(row.offsets_seconds.at(-1) < row.confirmation_window_seconds, "last attempt offset must precede window expiry");
    const effective = parseUtc(row.effective_from_utc, "effective_from_utc");
    if (row.retire_at_utc !== null) {
      const retire = parseUtc(row.retire_at_utc, "retire_at_utc");
      assert(retire > effective, "retire_at_utc must follow effective_from_utc");
    }
    assertSha256(row.bundle_sha256, "bundle_sha256");
    bundleHashes.add(row.bundle_sha256);
    assertSafeIdentifier(row.product_signer, "product_signer");
    assertSafeIdentifier(row.order_core_signer, "order_core_signer");
    assertSafeIdentifier(row.m3_producer_version, "m3_producer_version");
    assert(!policies.has(row.programme), "duplicate policy programme");
    policies.set(row.programme, row);
  }
  assertProgrammesComplete(new Set(policies.keys()), "POLICY_OUTCOME policy");
  assert(bundleHashes.size === 1, "two-program policy rows must share one canonical bundle_sha256");

  const outcomeProgrammes = new Set();
  const seenOrdinals = new Map();
  const reconciliation = new Map();
  for (const row of artifact.outcome_rows) {
    assertExactKeys(row, OUTCOME_ROW_KEYS, "POLICY_OUTCOME outcome row");
    assertSafeIdentifier(row.dataset_id, "outcome dataset_id");
    assertProgram(row.programme, "outcome programme");
    outcomeProgrammes.add(row.programme);
    const policy = policies.get(row.programme);
    assert(policy && row.policy_version === policy.policy_version, "outcome policy_version does not match policy row");
    assertPositiveInteger(row.attempt_ordinal, "attempt_ordinal");
    assert(row.attempt_ordinal <= policy.max_customer_attempts, "attempt_ordinal exceeds signed policy");
    assert(ENUM_PATTERN.test(row.normalized_disposition), "normalized_disposition must be an enum");
    assertNonNegativeInteger(row.outcome_count, "outcome_count");
    assertPositiveInteger(row.total_valid_attempts, "total_valid_attempts");
    const start = parseUtc(row.observation_start_utc, "outcome observation_start_utc");
    const end = parseUtc(row.observation_end_utc, "outcome observation_end_utc");
    assert(end > start, "outcome observation window must be positive");
    assertWithinWindow(start, end, submission, "outcome row");
    assert(typeof row.retry_eligible === "boolean", "retry_eligible must be boolean");
    assert(ENUM_PATTERN.test(row.technical_retry_classification), "technical_retry_classification must be an enum");
    assert(row.data_quality_flag === "OK", "outcome data_quality_flag must be OK");
    const ordinalKey = `${row.programme}\u0000${row.attempt_ordinal}`;
    const ordinals = seenOrdinals.get(row.programme) ?? new Set();
    ordinals.add(row.attempt_ordinal);
    seenOrdinals.set(row.programme, ordinals);
    const key = `${row.dataset_id}\u0000${row.programme}\u0000${row.policy_version}\u0000${row.attempt_ordinal}`;
    const current = reconciliation.get(key) ?? { count: 0, total: row.total_valid_attempts };
    assert(current.total === row.total_valid_attempts, "outcome total_valid_attempts drift within a slice");
    current.count += row.outcome_count;
    reconciliation.set(key, current);
    if (row.retry_eligible) {
      assert(row.attempt_ordinal < policy.max_customer_attempts, "terminal attempt cannot be retry_eligible");
    }
  }
  assertProgrammesComplete(outcomeProgrammes, "POLICY_OUTCOME outcomes");
  for (const [programme, policy] of policies) {
    const expected = new Set(Array.from({ length: policy.max_customer_attempts }, (_, index) => index + 1));
    assertSetEquals(seenOrdinals.get(programme) ?? new Set(), expected, `outcome ordinals for ${programme}`);
  }
  for (const totals of reconciliation.values()) {
    assert(totals.count === totals.total, "outcome counts do not reconcile to total_valid_attempts");
  }
  return artifact.policy_rows.length + artifact.outcome_rows.length;
}

function validateInfraReserveArtifact(submission, artifact) {
  assertExactKeys(artifact, ["schema_version", "data_group", "topology_rows", "scenario_rows"], "INFRA_RESERVE artifact");
  assertNonEmptyArray(artifact.topology_rows, "INFRA_RESERVE topology_rows");
  assertNonEmptyArray(artifact.scenario_rows, "INFRA_RESERVE scenario_rows");
  const topologyIds = new Set();
  let maximumTestedChannels = 0;
  for (const row of artifact.topology_rows) {
    assertExactKeys(row, TOPOLOGY_ROW_KEYS, "INFRA_RESERVE topology row");
    assertSafeIdentifier(row.submission_id, "topology submission_id");
    assert(!topologyIds.has(row.submission_id), "duplicate topology submission_id");
    topologyIds.add(row.submission_id);
    for (const key of ["topology_version", "vendor_model", "firmware_version", "carrier_scope", "reserve_rationale", "quarantine_policy_ref", "failover_policy_ref"]) {
      assertMeaningfulString(row[key], `INFRA_RESERVE ${key}`, false);
    }
    assertPositiveInteger(row.tested_channel_count, "tested_channel_count");
    assert(row.tested_channel_count >= 2, "multi-channel evidence cannot be inferred from one channel");
    assertPositiveInteger(row.per_channel_concurrency, "per_channel_concurrency");
    assert(row.per_channel_concurrency === 1, "per_channel_concurrency must preserve ONE_SIM_ONE_ACTIVE_CALL");
    assertPositiveInteger(row.account_quota, "account_quota");
    assert(row.account_quota >= row.tested_channel_count, "account_quota is below tested channels");
    assert(typeof row.reserve_factor === "number" && Number.isFinite(row.reserve_factor), "reserve_factor must be finite");
    assert(row.reserve_factor > 0, "reserve_factor must be positive");
    assertSha256(row.test_report_sha256, "test_report_sha256");
    const start = parseUtc(row.observation_start_utc, "topology observation_start_utc");
    const end = parseUtc(row.observation_end_utc, "topology observation_end_utc");
    assert(end > start, "topology observation window must be positive");
    assertWithinWindow(start, end, submission, "topology row");
    maximumTestedChannels = Math.max(maximumTestedChannels, row.tested_channel_count);
  }

  const scenarioIds = new Set();
  let hasFailureOrQuarantine = false;
  for (const row of artifact.scenario_rows) {
    assertExactKeys(row, SCENARIO_ROW_KEYS, "INFRA_RESERVE scenario row");
    assertSafeIdentifier(row.scenario_id, "scenario_id");
    assert(!scenarioIds.has(row.scenario_id), "duplicate scenario_id");
    scenarioIds.add(row.scenario_id);
    assertNonNegativeInteger(row.available_channels, "available_channels");
    assertNonNegativeInteger(row.quarantined_channels, "quarantined_channels");
    assert(row.available_channels + row.quarantined_channels <= maximumTestedChannels, "scenario channels exceed tested topology");
    if (row.failed_provider_or_gateway !== "NONE") {
      assertSafeIdentifier(row.failed_provider_or_gateway, "failed_provider_or_gateway");
    }
    assertPositiveInteger(row.offered_attempts, "offered_attempts");
    assertNonNegativeInteger(row.completed_attempts, "completed_attempts");
    assertNonNegativeInteger(row.deadline_expired_attempts, "deadline_expired_attempts");
    assert(row.completed_attempts + row.deadline_expired_attempts <= row.offered_attempts, "scenario outcome counts exceed offered attempts");
    assertNonNegativeNumber(row.recovery_seconds, "recovery_seconds");
    assert(row.result === "PASS" || row.result === "FAIL", "scenario result must be PASS or FAIL");
    assertSafeIdentifier(row.evidence_ref, "evidence_ref");
    if (row.quarantined_channels > 0 || row.failed_provider_or_gateway !== "NONE") {
      hasFailureOrQuarantine = true;
    }
  }
  assert(artifact.scenario_rows.length >= 2, "INFRA_RESERVE requires at least two scenarios");
  assert(hasFailureOrQuarantine, "INFRA_RESERVE has no quarantine/failure scenario");
  return artifact.topology_rows.length + artifact.scenario_rows.length;
}

function assertWithinWindow(start, end, submission, label) {
  const envelopeStart = Date.parse(submission.observation_start_utc);
  const envelopeEnd = Date.parse(submission.observation_end_utc);
  assert(start >= envelopeStart && end <= envelopeEnd, `${label} escapes submission observation window`);
}

function assertNoSensitiveContent(raw, label) {
  assert(!PHONE_PATTERN.test(raw), `${label} contains a raw phone/MSISDN pattern`);
  assert(!EMAIL_PATTERN.test(raw), `${label} contains an email address`);
  assert(!ADDRESS_PATTERN.test(raw), `${label} contains a street-address pattern`);
  assert(!SECRET_VALUE_PATTERN.test(raw), `${label} contains a secret/token pattern`);
}

function assertNoForbiddenKeys(value, label) {
  if (Array.isArray(value)) {
    value.forEach(item => assertNoForbiddenKeys(item, label));
    return;
  }
  if (!isPlainObject(value)) return;
  for (const [key, child] of Object.entries(value)) {
    const normalized = key.toLowerCase().replace(/[^a-z0-9]/gu, "");
    assert(!FORBIDDEN_NORMALIZED_KEYS.has(normalized), `${label} contains a forbidden sensitive field`);
    assertNoForbiddenKeys(child, label);
  }
}

function assertExactKeys(value, expectedKeys, label) {
  assertPlainObject(value, label);
  const actual = Object.keys(value).sort();
  const expected = [...expectedKeys].sort();
  assert(actual.length === expected.length && actual.every((key, index) => key === expected[index]), `${label} fields do not match schema`);
}

function assertPlainObject(value, label) {
  assert(isPlainObject(value), `${label} must be an object`);
}

function isPlainObject(value) {
  return value !== null && typeof value === "object" && !Array.isArray(value);
}

function assertNonEmptyArray(value, label) {
  assert(Array.isArray(value) && value.length > 0, `${label} must be a non-empty array`);
}

function assertProgram(value, label) {
  assert(PROGRAMMES.includes(value), `${label} is unsupported`);
}

function assertProgrammesComplete(actual, label) {
  assertSetEquals(actual, new Set(PROGRAMMES), `${label} programme coverage`);
}

function assertSetEquals(actual, expected, label) {
  assert(actual.size === expected.size && [...expected].every(value => actual.has(value)), `${label} is incomplete`);
}

function assertExactArray(actual, expected, label) {
  assert(Array.isArray(actual), `${label} must be an array`);
  assert(actual.length === expected.length && actual.every((value, index) => value === expected[index]), `${label} has drifted`);
}

function assertSafeIdentifier(value, label) {
  assert(typeof value === "string" && SAFE_IDENTIFIER_PATTERN.test(value) && !PLACEHOLDER_PATTERN.test(value), `${label} is invalid`);
}

function assertMeaningfulString(value, label, allowTestOnly, options = {}) {
  assert(typeof value === "string" && value.trim().length >= 2, `${label} must be a non-empty string`);
  if (options.allowNone && value === "NONE") return;
  if (allowTestOnly && value.includes("TEST_ONLY")) return;
  assert(!value.includes("TEST_ONLY"), `${label} contains a TEST_ONLY marker`);
  assert(!PLACEHOLDER_PATTERN.test(value.trim()), `${label} contains a placeholder`);
}

function assertSafeRelativePath(value, label) {
  assert(typeof value === "string" && value.length > 0 && !value.includes("\0"), `${label} is invalid`);
  assert(!isAbsolute(value), `${label} must be relative`);
  const normalized = value.replace(/\\/gu, "/");
  assert(!normalized.split("/").includes(".."), `${label} contains parent traversal`);
  assert(normalized.endsWith(".json"), `${label} must reference JSON`);
}

function assertSha256(value, label) {
  assert(typeof value === "string" && /^[a-f0-9]{64}$/u.test(value), `${label} must be lowercase SHA-256`);
  assert(!/^0{64}$/u.test(value), `${label} cannot be an all-zero placeholder`);
}

function assertPositiveInteger(value, label) {
  assert(Number.isSafeInteger(value) && value > 0, `${label} must be a positive integer`);
}

function assertNonNegativeInteger(value, label) {
  assert(Number.isSafeInteger(value) && value >= 0, `${label} must be a non-negative integer`);
}

function assertNonNegativeNumber(value, label) {
  assert(typeof value === "number" && Number.isFinite(value) && value >= 0, `${label} must be a non-negative number`);
}

function parseUtc(value, label) {
  assert(typeof value === "string" && /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{3})?Z$/u.test(value), `${label} must be an ISO-8601 UTC timestamp`);
  const parsed = Date.parse(value);
  assert(Number.isFinite(parsed), `${label} is not a valid timestamp`);
  return parsed;
}

function parseSignedAt(value, label) {
  assert(typeof value === "string" && /(?:Z|[+-]\d{2}:\d{2})$/u.test(value), `${label} must include a timezone`);
  const parsed = Date.parse(value);
  assert(Number.isFinite(parsed), `${label} is not a valid timestamp`);
  return parsed;
}

function readBoundedFile(path, label) {
  const size = statSync(path).size;
  assert(size > 0, `${label} is empty`);
  assert(size <= MAX_ARTIFACT_BYTES, `${label} exceeds 50 MiB`);
  return readFileSync(path, "utf8");
}

function parseJson(raw, label) {
  try {
    return JSON.parse(raw);
  } catch {
    throw new Error(`${label} is not valid JSON`);
  }
}

function sha256(value) {
  return createHash("sha256").update(value).digest("hex");
}

function sha256File(path) {
  return sha256(readFileSync(path));
}

function assert(condition, message) {
  if (!condition) throw new Error(message);
}

function safeError(error) {
  const message = error instanceof Error ? error.message : "validation failed";
  return message
    .replace(PHONE_PATTERN, "[REDACTED_PHONE]")
    .replace(EMAIL_PATTERN, "[REDACTED_EMAIL]")
    .replace(SECRET_VALUE_PATTERN, "[REDACTED_SECRET]");
}

async function main() {
  const args = process.argv.slice(2);
  if (args.length === 1 && args[0] === "--self-test") {
    runSelfTest();
    return;
  }
  const checkpointLedgerIndex = args.indexOf("--checkpoint-intake-ledger");
  const ledgerIdIndex = args.indexOf("--ledger-id");
  const checkpointOutIndex = args.indexOf("--checkpoint-out");
  if (checkpointLedgerIndex >= 0 || ledgerIdIndex >= 0 || checkpointOutIndex >= 0) {
    if (
      checkpointLedgerIndex < 0
      || ledgerIdIndex < 0
      || checkpointOutIndex < 0
      || !args[checkpointLedgerIndex + 1]
      || !args[ledgerIdIndex + 1]
      || !args[checkpointOutIndex + 1]
      || args.filter(value => value === "--checkpoint-intake-ledger").length !== 1
      || args.filter(value => value === "--ledger-id").length !== 1
      || args.filter(value => value === "--checkpoint-out").length !== 1
      || args.length !== 6
    ) {
      throw new Error(
        "use --checkpoint-intake-ledger <ledger.jsonl> --ledger-id <safe-alias>"
          + " --checkpoint-out <checkpoint.json>",
      );
    }
    const result = writeCapacityIntakeLedgerHeadCheckpoint(
      args[checkpointLedgerIndex + 1],
      args[ledgerIdIndex + 1],
      args[checkpointOutIndex + 1],
    );
    process.stdout.write(
      `${result.status} ledger_id=${result.ledger_id}`
        + ` entries=${result.entry_count}`
        + ` ledger_sha256=${result.ledger_sha256}`
        + ` last_entry_sha256=${result.last_entry_sha256}`
        + ` checkpoint_sha256=${result.checkpoint_sha256}`
        + " overwrite=DENIED raw_rows=NO calibration=NOT_RUN\n",
    );
    return;
  }
  const verifyLedgerIndex = args.indexOf("--verify-intake-ledger");
  const checkpointIndex = args.indexOf("--checkpoint");
  const expectedCheckpointSha256Index = args.indexOf("--expected-checkpoint-sha256");
  if (verifyLedgerIndex >= 0 || checkpointIndex >= 0 || expectedCheckpointSha256Index >= 0) {
    if (
      verifyLedgerIndex < 0
      || checkpointIndex < 0
      || expectedCheckpointSha256Index < 0
      || !args[verifyLedgerIndex + 1]
      || !args[checkpointIndex + 1]
      || !args[expectedCheckpointSha256Index + 1]
      || args.filter(value => value === "--verify-intake-ledger").length !== 1
      || args.filter(value => value === "--checkpoint").length !== 1
      || args.filter(value => value === "--expected-checkpoint-sha256").length !== 1
      || args.length !== 6
    ) {
      throw new Error(
        "use --verify-intake-ledger <ledger.jsonl> --checkpoint <checkpoint.json>"
          + " --expected-checkpoint-sha256 <trusted-hash>",
      );
    }
    const result = verifyCapacityIntakeLedgerHeadCheckpoint(
      args[verifyLedgerIndex + 1],
      args[checkpointIndex + 1],
      args[expectedCheckpointSha256Index + 1],
    );
    process.stdout.write(
      `${result.status} ledger_id=${result.ledger_id}`
        + ` entries=${result.entry_count}`
        + ` ledger_sha256=${result.ledger_sha256}`
        + ` last_entry_sha256=${result.last_entry_sha256}`
        + ` checkpoint_sha256=${result.checkpoint_sha256}`
        + ` authority=${result.authority}`
        + " trust_anchor=CALLER_SUPPLIED calibration=NOT_RUN\n",
    );
    return;
  }
  const verifyReceiptIndex = args.indexOf("--verify-receipt");
  const expectedReceiptSha256Index = args.indexOf("--expected-receipt-sha256");
  const appendLedgerIndex = args.indexOf("--append-intake-ledger");
  if (verifyReceiptIndex >= 0 || expectedReceiptSha256Index >= 0 || appendLedgerIndex >= 0) {
    const verifyModeLength = appendLedgerIndex >= 0 ? 6 : 4;
    if (
      verifyReceiptIndex < 0
      || expectedReceiptSha256Index < 0
      || !args[verifyReceiptIndex + 1]
      || !args[expectedReceiptSha256Index + 1]
      || (appendLedgerIndex >= 0 && !args[appendLedgerIndex + 1])
      || args.filter(value => value === "--verify-receipt").length !== 1
      || args.filter(value => value === "--expected-receipt-sha256").length !== 1
      || args.filter(value => value === "--append-intake-ledger").length > 1
      || args.length !== verifyModeLength
    ) {
      throw new Error(
        "use --verify-receipt <receipt.json> --expected-receipt-sha256 <trusted-hash>"
          + " [--append-intake-ledger <ledger.jsonl>]",
      );
    }
    if (appendLedgerIndex >= 0) {
      const result = appendVerifiedCapacityReceiptToLedger(
        args[verifyReceiptIndex + 1],
        args[expectedReceiptSha256Index + 1],
        args[appendLedgerIndex + 1],
      );
      process.stdout.write(
        `${result.status} idempotency_key=${result.idempotency_key}`
          + ` appended=${result.appended ? "YES" : "NO"}`
          + ` entries=${result.entry_count}`
          + ` entry_sha256=${result.entry_sha256}`
          + ` ledger_sha256=${result.ledger_sha256}`
          + " raw_rows=NO calibration=NOT_RUN\n",
      );
      return;
    }
    const result = verifyCapacityDataIntakeReceiptFile(
      args[verifyReceiptIndex + 1],
      args[expectedReceiptSha256Index + 1],
    );
    process.stdout.write(
      `CAPACITY_DATA_INTAKE_RECEIPT_VERIFY_PASS bundle=${result.bundle_id}`
        + ` groups=${result.group_count}`
        + ` records=${result.total_records}`
        + ` receipt_sha256=${result.receipt_sha256}`
        + ` authority=${result.authority}`
        + " ledger_precheck=PASS_METADATA_ONLY calibration=NOT_RUN\n",
    );
    return;
  }
  const bundleIndex = args.indexOf("--bundle-dir");
  const receiptIndex = args.indexOf("--receipt-out");
  const expectedLength = receiptIndex >= 0 ? 4 : 2;
  if (
    bundleIndex < 0
    || !args[bundleIndex + 1]
    || (receiptIndex >= 0 && !args[receiptIndex + 1])
    || args.filter(value => value === "--bundle-dir").length !== 1
    || args.filter(value => value === "--receipt-out").length > 1
    || args.length !== expectedLength
  ) {
    throw new Error(
      "use --bundle-dir <directory> [--receipt-out <receipt.json>],"
        + " --verify-receipt <receipt.json> --expected-receipt-sha256 <trusted-hash>"
        + " [--append-intake-ledger <ledger.jsonl>],"
        + " --checkpoint-intake-ledger <ledger.jsonl> --ledger-id <safe-alias>"
        + " --checkpoint-out <checkpoint.json>,"
        + " --verify-intake-ledger <ledger.jsonl> --checkpoint <checkpoint.json>"
        + " --expected-checkpoint-sha256 <trusted-hash>, or --self-test",
    );
  }
  const result = validateCapacityDataIntakeBundle(args[bundleIndex + 1]);
  let receiptResult;
  if (receiptIndex >= 0) {
    receiptResult = writeCapacityDataIntakeReceipt(result, args[receiptIndex + 1]);
  }
  process.stdout.write(
    `CAPACITY_DATA_INTAKE_PASS bundle=${result.bundle_id}`
      + ` groups=${result.groups.length}`
      + ` records=${result.total_records}`
      + ` manifest_sha256=${result.manifest_sha256}`
      + ` authority=${result.authority}\n`,
  );
  if (receiptResult) {
    process.stdout.write(
      `CAPACITY_DATA_INTAKE_RECEIPT_WRITTEN sha256=${receiptResult.receipt_sha256}`
        + " raw_rows=NO overwrite=DENIED calibration=NOT_RUN\n",
    );
  }
}

function runSelfTest() {
  const scratch = mkdtempSync(resolve(tmpdir(), "ivr-capacity-intake-"));
  try {
    const validDir = resolve(scratch, "valid");
    writeValidTestBundle(validDir);
    const result = validateCapacityDataIntakeBundle(validDir, { allowTestOnly: true });
    assert(result.groups.length === 4 && result.total_records === 15, "valid fixture summary drift");
    process.stdout.write("CAP-INTAKE-VALID-01 PASS — TEST_ONLY four-group bundle accepted only by self-test path\n");

    let testOnlyReceiptRejected = false;
    try {
      writeCapacityDataIntakeReceipt(result, resolve(scratch, "rejected-test-receipt.json"));
    } catch {
      testOnlyReceiptRejected = true;
    }
    assert(testOnlyReceiptRejected, "normal receipt path accepted TEST_ONLY validation result");
    const fixedReceiptOptions = {
      allowTestOnly: true,
      validatedAtUtc: "2026-09-03T06:30:00Z",
    };
    const receiptA = buildCapacityDataIntakeReceipt(result, fixedReceiptOptions);
    const receiptB = buildCapacityDataIntakeReceipt(result, fixedReceiptOptions);
    assert(JSON.stringify(receiptA) === JSON.stringify(receiptB), "receipt is not deterministic at a fixed clock");
    const receiptPath = resolve(scratch, "validation-receipt.json");
    const receiptResult = writeCapacityDataIntakeReceipt(result, receiptPath, fixedReceiptOptions);
    const receiptBytes = readFileSync(receiptPath, "utf8");
    assert(receiptResult.receipt_sha256 === sha256(receiptBytes), "receipt hash does not match exact bytes");
    assert(!receiptBytes.includes('"rows":'), "receipt persisted raw rows");
    assert(receiptResult.receipt.safety.raw_rows_persisted === false, "receipt raw-row safety flag drift");
    assert(receiptResult.receipt.safety.external_authority_verified === false, "receipt inferred external authority");
    let overwriteRejected = false;
    try {
      writeCapacityDataIntakeReceipt(result, receiptPath, fixedReceiptOptions);
    } catch {
      overwriteRejected = true;
    }
    assert(overwriteRejected, "receipt writer overwrote an existing artifact");
    const tamperedResult = structuredClone(result);
    tamperedResult.total_records += 1;
    let tamperedResultRejected = false;
    try {
      buildCapacityDataIntakeReceipt(tamperedResult, fixedReceiptOptions);
    } catch {
      tamperedResultRejected = true;
    }
    assert(tamperedResultRejected, "receipt builder accepted an inconsistent validation result");
    process.stdout.write(
      "CAP-INTAKE-RECEIPT-05 PASS — deterministic fixed-clock receipt is hash-bound, PII-safe, raw-row-free, tamper-rejecting and no-overwrite\n",
    );

    const verifiedReceipt = verifyCapacityDataIntakeReceiptFile(
      receiptPath,
      receiptResult.receipt_sha256,
      { allowTestOnly: true },
    );
    assert(verifiedReceipt.group_count === 4 && verifiedReceipt.total_records === 15, "verified receipt summary drift");
    let normalModeTestReceiptRejected = false;
    try {
      verifyCapacityDataIntakeReceiptFile(receiptPath, receiptResult.receipt_sha256);
    } catch {
      normalModeTestReceiptRejected = true;
    }
    assert(normalModeTestReceiptRejected, "normal verify mode accepted a TEST_ONLY receipt");

    const verifyRefusals = [
      ["missing-trust-anchor", undefined, undefined],
      ["wrong-trust-anchor", undefined, "1".repeat(64)],
      ["byte-tamper", raw => `${raw} `, receiptResult.receipt_sha256],
      ["validator-drift", value => { value.validator.sha256 = "1".repeat(64); }],
      ["source-contract-drift", value => { value.source_contract_sha256 = "1".repeat(64); }],
      ["provenance-drift", value => { value.submissions[0].signer_identity_alias = "<owner>"; }],
      ["count-drift", value => { value.validation.total_records += 1; }],
      ["safety-drift", value => { value.safety.production_gate_promoted = true; }],
      ["schema-drift", value => { value.unexpected = true; }],
      ["pii-injection", value => { value.bundle_id = ["+84", " 90", " 123", " 4567"].join(""); }],
    ];
    for (const [name, mutate, fixedExpectedHash] of verifyRefusals) {
      const mutatedReceiptPath = resolve(scratch, `verify-${name}.json`);
      let mutatedBytes = receiptBytes;
      if (typeof mutate === "function") {
        if (name === "byte-tamper") {
          mutatedBytes = mutate(receiptBytes);
        } else {
          const value = structuredClone(receiptResult.receipt);
          mutate(value);
          mutatedBytes = `${JSON.stringify(value, null, 2)}\n`;
        }
      }
      writeFileSync(mutatedReceiptPath, mutatedBytes, "utf8");
      const expectedHash = fixedExpectedHash === undefined && name !== "missing-trust-anchor"
        ? sha256(mutatedBytes)
        : fixedExpectedHash;
      let rejected = false;
      try {
        verifyCapacityDataIntakeReceiptFile(mutatedReceiptPath, expectedHash, { allowTestOnly: true });
      } catch {
        rejected = true;
      }
      assert(rejected, `receipt verification negative control was accepted: ${name}`);
      process.stdout.write(`CAP-INTAKE-RECEIPT-VERIFY-REFUSAL PASS mutation=${name}\n`);
    }
    process.stdout.write(
      `CAP-INTAKE-RECEIPT-VERIFY-06 PASS — trusted-hash receipt accepted; normal mode rejects TEST_ONLY;`
        + ` verify_refusals=${verifyRefusals.length}\n`,
    );

    const normalLedgerPath = resolve(scratch, "normal-mode-ledger.jsonl");
    let normalLedgerRejected = false;
    try {
      appendVerifiedCapacityReceiptToLedger(
        receiptPath,
        receiptResult.receipt_sha256,
        normalLedgerPath,
        { appendedAtUtc: "2026-09-03T06:40:00Z" },
      );
    } catch {
      normalLedgerRejected = true;
    }
    assert(normalLedgerRejected && !existsSync(normalLedgerPath), "normal ledger mode accepted TEST_ONLY receipt");

    const ledgerPath = resolve(scratch, "capacity-intake-ledger.jsonl");
    const firstAppend = appendVerifiedCapacityReceiptToLedger(
      receiptPath,
      receiptResult.receipt_sha256,
      ledgerPath,
      { allowTestOnly: true, appendedAtUtc: "2026-09-03T06:41:00Z" },
    );
    assert(firstAppend.appended && firstAppend.entry_count === 1, "first ledger append summary drift");
    const firstLedgerBytes = readFileSync(ledgerPath, "utf8");
    assert(!firstLedgerBytes.includes('"rows"'), "ledger persisted raw rows");
    assert(!firstLedgerBytes.includes(receiptPath), "ledger persisted the receipt path");
    assert(!firstLedgerBytes.includes("signer_identity_alias"), "ledger persisted submission signer metadata");

    const duplicateAppend = appendVerifiedCapacityReceiptToLedger(
      receiptPath,
      receiptResult.receipt_sha256,
      ledgerPath,
      { allowTestOnly: true, appendedAtUtc: "2026-09-03T06:42:00Z" },
    );
    assert(!duplicateAppend.appended && duplicateAppend.entry_count === 1, "duplicate receipt was not an idempotent no-op");
    assert(readFileSync(ledgerPath, "utf8") === firstLedgerBytes, "duplicate receipt changed ledger bytes");

    const secondReceiptPath = resolve(scratch, "validation-receipt-second.json");
    const secondReceiptResult = writeCapacityDataIntakeReceipt(
      result,
      secondReceiptPath,
      { allowTestOnly: true, validatedAtUtc: "2026-09-03T06:31:00Z" },
    );
    const secondAppend = appendVerifiedCapacityReceiptToLedger(
      secondReceiptPath,
      secondReceiptResult.receipt_sha256,
      ledgerPath,
      { allowTestOnly: true, appendedAtUtc: "2026-09-03T06:43:00Z" },
    );
    assert(secondAppend.appended && secondAppend.entry_count === 2, "second unique receipt was not appended");
    const twoEntryLedgerBytes = readFileSync(ledgerPath, "utf8");
    const ledgerView = readAndValidateCapacityIntakeLedger(ledgerPath, { allowTestOnly: true });
    assert(ledgerView.entries.length === 2, "ledger reader did not preserve two entries");
    assert(
      ledgerView.entries[1].entry.previous_entry_sha256 === ledgerView.entries[0].entry_sha256,
      "ledger previous-entry hash chain drift",
    );

    const tamperedReceiptPath = resolve(scratch, "ledger-tampered-receipt.json");
    writeFileSync(tamperedReceiptPath, `${receiptBytes} `, "utf8");
    const rejectedLedgerPath = resolve(scratch, "rejected-receipt-ledger.jsonl");
    let tamperedReceiptLedgerRejected = false;
    try {
      appendVerifiedCapacityReceiptToLedger(
        tamperedReceiptPath,
        receiptResult.receipt_sha256,
        rejectedLedgerPath,
        { allowTestOnly: true, appendedAtUtc: "2026-09-03T06:44:00Z" },
      );
    } catch {
      tamperedReceiptLedgerRejected = true;
    }
    assert(
      tamperedReceiptLedgerRejected && !existsSync(rejectedLedgerPath),
      "tampered receipt created or changed a ledger",
    );

    const tamperedLedgerPath = resolve(scratch, "tampered-ledger.jsonl");
    const ledgerLines = twoEntryLedgerBytes.slice(0, -1).split("\n");
    const tamperedFirstEntry = JSON.parse(ledgerLines[0]);
    tamperedFirstEntry.bundle_id = "TEST_ONLY_TAMPERED_BUNDLE";
    writeFileSync(tamperedLedgerPath, `${JSON.stringify(tamperedFirstEntry)}\n${ledgerLines[1]}\n`, "utf8");
    const tamperedLedgerBefore = readFileSync(tamperedLedgerPath, "utf8");
    let tamperedLedgerRejected = false;
    try {
      appendVerifiedCapacityReceiptToLedger(
        receiptPath,
        receiptResult.receipt_sha256,
        tamperedLedgerPath,
        { allowTestOnly: true, appendedAtUtc: "2026-09-03T06:45:00Z" },
      );
    } catch {
      tamperedLedgerRejected = true;
    }
    assert(tamperedLedgerRejected, "writer accepted a broken ledger hash chain");
    assert(readFileSync(tamperedLedgerPath, "utf8") === tamperedLedgerBefore, "writer changed a tampered ledger");

    const rawRowLedgerPath = resolve(scratch, "raw-row-ledger.jsonl");
    const rawRowEntry = structuredClone(ledgerView.entries[0].entry);
    rawRowEntry.rows = [];
    writeFileSync(rawRowLedgerPath, `${JSON.stringify(rawRowEntry)}\n`, "utf8");
    const rawRowLedgerBefore = readFileSync(rawRowLedgerPath, "utf8");
    let rawRowLedgerRejected = false;
    try {
      appendVerifiedCapacityReceiptToLedger(
        secondReceiptPath,
        secondReceiptResult.receipt_sha256,
        rawRowLedgerPath,
        { allowTestOnly: true, appendedAtUtc: "2026-09-03T06:46:00Z" },
      );
    } catch {
      rawRowLedgerRejected = true;
    }
    assert(rawRowLedgerRejected, "writer accepted a ledger entry carrying raw rows");
    assert(readFileSync(rawRowLedgerPath, "utf8") === rawRowLedgerBefore, "writer changed a raw-row ledger");

    const lockPath = `${ledgerPath}.lock`;
    writeFileSync(lockPath, "TEST_ONLY_HELD_LOCK\n", { encoding: "utf8", flag: "wx", mode: 0o600 });
    let lockConflictRejected = false;
    try {
      appendVerifiedCapacityReceiptToLedger(
        receiptPath,
        receiptResult.receipt_sha256,
        ledgerPath,
        { allowTestOnly: true, appendedAtUtc: "2026-09-03T06:47:00Z" },
      );
    } catch {
      lockConflictRejected = true;
    }
    assert(lockConflictRejected, "writer ignored an existing cooperative lock");
    assert(readFileSync(ledgerPath, "utf8") === twoEntryLedgerBytes, "lock conflict changed ledger bytes");
    unlinkSync(lockPath);
    process.stdout.write(
      "CAP-INTAKE-LEDGER-07 PASS — verified-only append, metadata-only shape, idempotent duplicate,"
        + " hash chain, receipt/ledger tamper refusal and cooperative lock are fail-closed\n",
    );

    const normalCheckpointPath = resolve(scratch, "normal-mode-checkpoint.json");
    let normalCheckpointRejected = false;
    try {
      writeCapacityIntakeLedgerHeadCheckpoint(
        ledgerPath,
        "CAPACITY-INTAKE-TEST-LEDGER",
        normalCheckpointPath,
        { checkpointedAtUtc: "2026-09-03T06:50:00Z" },
      );
    } catch {
      normalCheckpointRejected = true;
    }
    assert(
      normalCheckpointRejected && !existsSync(normalCheckpointPath),
      "normal checkpoint writer accepted TEST_ONLY ledger",
    );

    const checkpointPath = resolve(scratch, "ledger-head-checkpoint.json");
    const checkpointResult = writeCapacityIntakeLedgerHeadCheckpoint(
      ledgerPath,
      "CAPACITY-INTAKE-TEST-LEDGER",
      checkpointPath,
      { allowTestOnly: true, checkpointedAtUtc: "2026-09-03T06:51:00Z" },
    );
    const checkpointBytes = readFileSync(checkpointPath, "utf8");
    assert(checkpointResult.entry_count === 2, "checkpoint entry count drift");
    assert(checkpointResult.checkpoint_sha256 === sha256(checkpointBytes), "checkpoint exact hash drift");
    assert(!checkpointBytes.includes('"rows"'), "checkpoint persisted raw rows");
    assert(!checkpointBytes.includes(ledgerPath), "checkpoint persisted ledger path");
    assert(!checkpointBytes.includes("signer_identity_alias"), "checkpoint persisted signer metadata");

    const checkpointVerification = verifyCapacityIntakeLedgerHeadCheckpoint(
      ledgerPath,
      checkpointPath,
      checkpointResult.checkpoint_sha256,
      { allowTestOnly: true },
    );
    assert(
      checkpointVerification.entry_count === 2
        && checkpointVerification.last_entry_sha256 === checkpointResult.last_entry_sha256,
      "checkpoint verification summary drift",
    );
    let normalCheckpointVerificationRejected = false;
    try {
      verifyCapacityIntakeLedgerHeadCheckpoint(
        ledgerPath,
        checkpointPath,
        checkpointResult.checkpoint_sha256,
      );
    } catch {
      normalCheckpointVerificationRejected = true;
    }
    assert(normalCheckpointVerificationRejected, "normal checkpoint verifier accepted TEST_ONLY evidence");

    let checkpointOverwriteRejected = false;
    try {
      writeCapacityIntakeLedgerHeadCheckpoint(
        ledgerPath,
        "CAPACITY-INTAKE-TEST-LEDGER",
        checkpointPath,
        { allowTestOnly: true, checkpointedAtUtc: "2026-09-03T06:52:00Z" },
      );
    } catch {
      checkpointOverwriteRejected = true;
    }
    assert(checkpointOverwriteRejected, "checkpoint writer overwrote an existing artifact");
    assert(readFileSync(checkpointPath, "utf8") === checkpointBytes, "checkpoint overwrite attempt changed bytes");

    const checkpointRefusals = [
      ["missing-trust-anchor", undefined, undefined],
      ["wrong-trust-anchor", undefined, "1".repeat(64)],
      ["byte-tamper", raw => `${raw} `, checkpointResult.checkpoint_sha256],
      ["ledger-hash-drift", value => { value.ledger_sha256 = "1".repeat(64); }],
      ["raw-row-field", value => { value.rows = []; }],
    ];
    for (const [name, mutate, fixedExpectedHash] of checkpointRefusals) {
      const mutatedCheckpointPath = resolve(scratch, `checkpoint-${name}.json`);
      let mutatedBytes = checkpointBytes;
      if (typeof mutate === "function") {
        if (name === "byte-tamper") {
          mutatedBytes = mutate(checkpointBytes);
        } else {
          const value = structuredClone(checkpointResult.checkpoint);
          mutate(value);
          mutatedBytes = `${JSON.stringify(value, null, 2)}\n`;
        }
      }
      writeFileSync(mutatedCheckpointPath, mutatedBytes, "utf8");
      const expectedHash = fixedExpectedHash === undefined && name !== "missing-trust-anchor"
        ? sha256(mutatedBytes)
        : fixedExpectedHash;
      let rejected = false;
      try {
        verifyCapacityIntakeLedgerHeadCheckpoint(
          ledgerPath,
          mutatedCheckpointPath,
          expectedHash,
          { allowTestOnly: true },
        );
      } catch {
        rejected = true;
      }
      assert(rejected, `checkpoint verification negative control was accepted: ${name}`);
      process.stdout.write(`CAP-INTAKE-CHECKPOINT-REFUSAL PASS mutation=${name}\n`);
    }

    const rolledBackLedgerPath = resolve(scratch, "rolled-back-ledger.jsonl");
    writeFileSync(rolledBackLedgerPath, firstLedgerBytes, "utf8");
    let validPrefixRollbackRejected = false;
    try {
      verifyCapacityIntakeLedgerHeadCheckpoint(
        rolledBackLedgerPath,
        checkpointPath,
        checkpointResult.checkpoint_sha256,
        { allowTestOnly: true },
      );
    } catch {
      validPrefixRollbackRejected = true;
    }
    assert(validPrefixRollbackRejected, "trusted checkpoint accepted a valid-prefix ledger rollback");

    const truncatedLedgerPath = resolve(scratch, "truncated-ledger.jsonl");
    writeFileSync(truncatedLedgerPath, twoEntryLedgerBytes.slice(0, -5), "utf8");
    let partialTruncationRejected = false;
    try {
      verifyCapacityIntakeLedgerHeadCheckpoint(
        truncatedLedgerPath,
        checkpointPath,
        checkpointResult.checkpoint_sha256,
        { allowTestOnly: true },
      );
    } catch {
      partialTruncationRejected = true;
    }
    assert(partialTruncationRejected, "trusted checkpoint accepted a partially truncated ledger");

    const advancedLedgerPath = resolve(scratch, "advanced-ledger.jsonl");
    writeFileSync(advancedLedgerPath, twoEntryLedgerBytes, "utf8");
    const thirdReceiptPath = resolve(scratch, "validation-receipt-third.json");
    const thirdReceiptResult = writeCapacityDataIntakeReceipt(
      result,
      thirdReceiptPath,
      { allowTestOnly: true, validatedAtUtc: "2026-09-03T06:32:00Z" },
    );
    appendVerifiedCapacityReceiptToLedger(
      thirdReceiptPath,
      thirdReceiptResult.receipt_sha256,
      advancedLedgerPath,
      { allowTestOnly: true, appendedAtUtc: "2026-09-03T06:53:00Z" },
    );
    let appendAfterCheckpointRejected = false;
    try {
      verifyCapacityIntakeLedgerHeadCheckpoint(
        advancedLedgerPath,
        checkpointPath,
        checkpointResult.checkpoint_sha256,
        { allowTestOnly: true },
      );
    } catch {
      appendAfterCheckpointRejected = true;
    }
    assert(appendAfterCheckpointRejected, "stale checkpoint accepted a ledger appended after checkpointing");
    process.stdout.write(
      "CAP-INTAKE-CHECKPOINT-08 PASS — immutable metadata checkpoint and trusted-hash verifier reject"
        + " checkpoint tamper, valid-prefix rollback, partial truncation and post-checkpoint append\n",
    );

    let testOnlyRejected = false;
    try {
      validateCapacityDataIntakeBundle(validDir);
    } catch {
      testOnlyRejected = true;
    }
    assert(testOnlyRejected, "CLI acceptance path accepted TEST_ONLY fixture");
    process.stdout.write("CAP-INTAKE-MODE-02 PASS — normal acceptance path rejects TEST_ONLY data\n");

    const disguisedExternalDir = resolve(scratch, "disguised-external");
    writeValidTestBundle(disguisedExternalDir);
    mutateManifest(disguisedExternalDir, value => { value.status = EXTERNAL_STATUS; });
    let disguisedExternalRejected = false;
    try {
      validateCapacityDataIntakeBundle(disguisedExternalDir);
    } catch {
      disguisedExternalRejected = true;
    }
    assert(disguisedExternalRejected, "external status hid TEST_ONLY provenance markers");
    process.stdout.write("CAP-INTAKE-MODE-03 PASS — external status cannot hide TEST_ONLY provenance\n");

    let pendingTemplateRejected = false;
    try {
      validateCapacityDataIntakeBundle(resolve(REPOSITORY_ROOT, "docs/evidence/W-0155/templates"));
    } catch {
      pendingTemplateRejected = true;
    }
    assert(pendingTemplateRejected, "pending external-owner template was accepted");
    process.stdout.write("CAP-INTAKE-TEMPLATE-04 PASS — pending template is fail-closed\n");

    const refusals = [
      ["missing-group", directory => mutateManifest(directory, value => value.submissions.pop())],
      ["contract-hash", directory => mutateManifest(directory, value => { value.source_contract_sha256 = "1".repeat(64); })],
      ["artifact-hash", directory => appendFileSync(resolve(directory, "timing.json"), " ")],
      ["path-traversal", directory => mutateManifest(directory, value => { value.submissions[0].artifact_path = "../outside.json"; })],
      ["provenance-placeholder", directory => mutateManifest(directory, value => { value.submissions[0].signer_role = "<owner điền>"; })],
      ["signer-not-alias", directory => mutateManifest(directory, value => { value.submissions[0].signer_identity = "Synthetic Person Name"; })],
      ["raw-phone", directory => mutateArtifact(directory, "timing.json", value => {
        value.rows[0].carrier_label = ["+84", " 90", " 123", " 4567"].join("");
      })],
      ["dial-token-field", directory => mutateArtifact(directory, "timing.json", value => {
        value.rows[0]["dial_" + "token"] = ["unsafe", "-token-value"].join("");
      })],
      ["timing-invariant", directory => mutateArtifact(directory, "timing.json", value => { value.rows[0].occupancy_ms = 1; })],
      ["arrival-gap", directory => mutateArtifact(directory, "arrival.json", value => { value.rows[1].bucket_start_utc = "2026-09-01T00:06:00Z"; })],
      ["outcome-reconciliation", directory => mutateArtifact(directory, "policy-outcome.json", value => { value.outcome_rows[0].outcome_count = 9; })],
      ["single-channel-extrapolation", directory => mutateArtifact(directory, "infra-reserve.json", value => { value.topology_rows[0].tested_channel_count = 1; })],
      ["infra-counts", directory => mutateArtifact(directory, "infra-reserve.json", value => { value.scenario_rows[0].completed_attempts = 11; })],
      ["record-count", directory => mutateManifest(directory, value => { value.submissions[0].record_count += 1; })],
    ];
    for (const [name, mutate] of refusals) {
      const directory = resolve(scratch, name);
      writeValidTestBundle(directory);
      mutate(directory);
      let rejected = false;
      try {
        validateCapacityDataIntakeBundle(directory, { allowTestOnly: true });
      } catch {
        rejected = true;
      }
      assert(rejected, `negative control was accepted: ${name}`);
      process.stdout.write(`CAP-INTAKE-REFUSAL PASS mutation=${name}\n`);
    }
    process.stdout.write(
      `CAPACITY_DATA_INTAKE_SELFTEST_PASS valid=1 mode_guard=2 template_guard=1 receipt_guard=7`
        + ` receipt_verify_guard=${verifyRefusals.length + 2} ledger_guard=9 checkpoint_guard=13`
        + ` refusals=${refusals.length}`
        + " external_submissions=0 calibration=NOT_RUN\n",
    );
  } finally {
    rmSync(scratch, { recursive: true, force: true });
  }
}

function writeValidTestBundle(directory) {
  mkdirSync(directory, { recursive: true });
  const artifacts = createTestArtifacts();
  for (const [name, value] of Object.entries(artifacts)) writeJson(resolve(directory, name), value);
  const contractHash = sha256File(resolve(REPOSITORY_ROOT, SOURCE_CONTRACT_PATH));
  const envelopeDefaults = {
    artifact_format: "JSON",
    observation_start_utc: "2026-09-01T00:00:00Z",
    observation_end_utc: "2026-09-01T01:00:00Z",
    timezone_context: "Asia/Bangkok",
    filtering_rule: "TEST_ONLY_ALL_VALID_ROWS_NO_PII",
    pii_statement: "PII_SAFE",
    signed_at: "2026-09-03T04:00:00Z",
    limitations: "TEST_ONLY_SYNTHETIC_FIXTURE",
  };
  const definitions = [
    ["SUB-TIMING-TEST-01", "TIMING", "timing.json", "LAB_GATEWAY_EXPORT", "lab-export-test-v1", 2, "lab-operator-test-01", "LAB_OPERATOR", "TELEPHONY_LAB", "AUTH-LAB-TEST-01"],
    ["SUB-ARRIVAL-TEST-01", "ARRIVAL", "arrival.json", "M3_AGGREGATE_EXPORT", "m3-query-test-v1", 4, "m3-data-owner-test-01", "DATA_OWNER", "MODULE_3", "AUTH-M3-TEST-01"],
    ["SUB-POLICY-TEST-01", "POLICY_OUTCOME", "policy-outcome.json", "ORDER_CORE_POLICY_EXPORT", "policy-export-test-v1", 6, "product-owner-test-01", "PRODUCT_OWNER", "PRODUCT_ORDER_CORE", "AUTH-POLICY-TEST-01"],
    ["SUB-INFRA-TEST-01", "INFRA_RESERVE", "infra-reserve.json", "TELEPHONY_LAB_REPORT", "infra-report-test-v1", 3, "infra-owner-test-01", "INFRA_OWNER", "PLATFORM_TELEPHONY", "AUTH-INFRA-TEST-01"],
  ];
  const submissions = definitions.map(definition => {
    const [submissionId, group, artifactPath, sourceSystem, sourceVersion, count, signer, role, org, authority] = definition;
    return {
      submission_id: submissionId,
      data_group: group,
      artifact_path: artifactPath,
      artifact_sha256: sha256File(resolve(directory, artifactPath)),
      artifact_format: envelopeDefaults.artifact_format,
      schema_version: GROUP_SCHEMAS.get(group),
      source_system: sourceSystem,
      source_version: sourceVersion,
      observation_start_utc: envelopeDefaults.observation_start_utc,
      observation_end_utc: envelopeDefaults.observation_end_utc,
      timezone_context: envelopeDefaults.timezone_context,
      record_count: count,
      filtering_rule: envelopeDefaults.filtering_rule,
      pii_statement: envelopeDefaults.pii_statement,
      signer_identity: signer,
      signer_role: role,
      signer_org: org,
      authority_source: authority,
      signed_at: envelopeDefaults.signed_at,
      limitations: envelopeDefaults.limitations,
    };
  });
  writeJson(resolve(directory, "bundle-manifest.json"), {
    schema_version: BUNDLE_SCHEMA,
    work_id: EXPECTED_WORK_ID,
    status: TEST_STATUS,
    bundle_id: "BUNDLE-TEST-ONLY-0001",
    created_at_utc: "2026-09-03T04:00:00Z",
    source_contract: SOURCE_CONTRACT_PATH,
    source_contract_sha256: contractHash,
    submissions,
  });
}

function createTestArtifacts() {
  const timingRows = [
    createTimingRow("GH", "GOLDEN_HOUR", "2026-09-01T00:00:00Z"),
    createTimingRow("247", "TWENTY_FOUR_SEVEN", "2026-09-01T00:10:00Z"),
  ];
  const arrivalRows = [
    createArrivalRow("GH", "GOLDEN_HOUR", "2026-09-01T00:00:00Z", "2026-09-01T00:05:00Z", 12),
    createArrivalRow("GH", "GOLDEN_HOUR", "2026-09-01T00:05:00Z", "2026-09-01T00:10:00Z", 8),
    createArrivalRow("247", "TWENTY_FOUR_SEVEN", "2026-09-01T00:00:00Z", "2026-09-01T00:05:00Z", 4),
    createArrivalRow("247", "TWENTY_FOUR_SEVEN", "2026-09-01T00:05:00Z", "2026-09-01T00:10:00Z", 5),
  ];
  const policyBundleHash = "a".repeat(64);
  const policyRows = PROGRAMMES.map((programme, index) => ({
    policy_version: `prod-policy-test-${index + 1}`,
    programme,
    execution_mode: "PRODUCTION",
    max_customer_attempts: 2,
    offsets_seconds: programme === "GOLDEN_HOUR" ? [0, 150] : [0, 450],
    confirmation_window_seconds: programme === "GOLDEN_HOUR" ? 300 : 900,
    effective_from_utc: "2026-09-01T00:00:00Z",
    retire_at_utc: null,
    bundle_sha256: policyBundleHash,
    product_signer: "product-owner-test-01",
    order_core_signer: "order-core-owner-test-01",
    m3_producer_version: "m3-producer-test-v1",
  }));
  const outcomeRows = policyRows.flatMap(policy => [1, 2].map(ordinal => ({
    dataset_id: `OUTCOME-${policy.programme}`,
    programme: policy.programme,
    policy_version: policy.policy_version,
    attempt_ordinal: ordinal,
    normalized_disposition: ordinal === 1 ? "NO_ANSWER" : "CONFIRMED",
    outcome_count: 10,
    total_valid_attempts: 10,
    observation_start_utc: "2026-09-01T00:00:00Z",
    observation_end_utc: "2026-09-01T01:00:00Z",
    retry_eligible: ordinal === 1,
    technical_retry_classification: "NOT_TECHNICAL_RETRY",
    data_quality_flag: "OK",
  })));
  return {
    "timing.json": { schema_version: GROUP_SCHEMAS.get("TIMING"), data_group: "TIMING", rows: timingRows },
    "arrival.json": { schema_version: GROUP_SCHEMAS.get("ARRIVAL"), data_group: "ARRIVAL", rows: arrivalRows },
    "policy-outcome.json": { schema_version: GROUP_SCHEMAS.get("POLICY_OUTCOME"), data_group: "POLICY_OUTCOME", policy_rows: policyRows, outcome_rows: outcomeRows },
    "infra-reserve.json": {
      schema_version: GROUP_SCHEMAS.get("INFRA_RESERVE"),
      data_group: "INFRA_RESERVE",
      topology_rows: [{
        submission_id: "TOPOLOGY-TEST-01",
        topology_version: "topology-test-v1",
        vendor_model: "gateway-test-model",
        firmware_version: "firmware-test-v1",
        carrier_scope: "carrier-test-alias",
        tested_channel_count: 4,
        per_channel_concurrency: 1,
        account_quota: 4,
        reserve_factor: 1.25,
        reserve_rationale: "SYNTHETIC_FAILURE_HEADROOM",
        quarantine_policy_ref: "QUARANTINE-TEST-01",
        failover_policy_ref: "FAILOVER-TEST-01",
        test_report_sha256: "b".repeat(64),
        observation_start_utc: "2026-09-01T00:00:00Z",
        observation_end_utc: "2026-09-01T01:00:00Z",
      }],
      scenario_rows: [
        { scenario_id: "SCENARIO-HEALTHY-TEST", available_channels: 4, quarantined_channels: 0, failed_provider_or_gateway: "NONE", offered_attempts: 10, completed_attempts: 10, deadline_expired_attempts: 0, recovery_seconds: 0, result: "PASS", evidence_ref: "EVIDENCE-TEST-HEALTHY" },
        { scenario_id: "SCENARIO-QUARANTINE-TEST", available_channels: 3, quarantined_channels: 1, failed_provider_or_gateway: "gateway-test-alias", offered_attempts: 10, completed_attempts: 9, deadline_expired_attempts: 1, recovery_seconds: 30, result: "PASS", evidence_ref: "EVIDENCE-TEST-QUARANTINE" },
      ],
    },
  };
}

function createTimingRow(label, programme, startText) {
  const start = Date.parse(startText);
  const end = start + 40_000;
  const available = end + 10_000;
  return {
    run_label: `RUN-${label}-TEST`,
    attempt_label: `ATTEMPT-${label}-TEST-01`,
    programme,
    execution_mode: "LAB_REAL_SIM",
    carrier_label: "CARRIER-TEST-ALIAS",
    scenario: "ANSWERED_TEST",
    disposition: "CONFIRMED",
    started_at_utc: new Date(start).toISOString().replace(".000Z", "Z"),
    ended_at_utc: new Date(end).toISOString().replace(".000Z", "Z"),
    available_again_at_utc: new Date(available).toISOString().replace(".000Z", "Z"),
    occupancy_ms: 40_000,
    cooldown_ms: 10_000,
    full_cycle_ms: 50_000,
    cdr_correlation_ref: `CDR-${label}-TEST-01`,
    gateway_model: "gateway-test-model",
    firmware_version: "firmware-test-v1",
    codec_profile: "codec-test-l16-8k",
  };
}

function createArrivalRow(dataset, programme, start, end, count) {
  return {
    dataset_id: `ARRIVAL-${dataset}-TEST`,
    programme,
    session_definition_id: `SESSION-${dataset}-SIGNED-TEST`,
    business_timezone: "Asia/Bangkok",
    bucket_start_utc: start,
    bucket_end_utc: end,
    eligible_order_count: count,
    source_query_version: "m3-query-test-v1",
    eligibility_filter_version: "eligibility-filter-test-v1",
    data_quality_flag: "OK",
  };
}

function mutateManifest(directory, mutate) {
  const path = resolve(directory, "bundle-manifest.json");
  const value = JSON.parse(readFileSync(path, "utf8"));
  mutate(value);
  writeJson(path, value);
}

function mutateArtifact(directory, artifactName, mutate) {
  const artifactPath = resolve(directory, artifactName);
  const artifact = JSON.parse(readFileSync(artifactPath, "utf8"));
  mutate(artifact);
  writeJson(artifactPath, artifact);
  mutateManifest(directory, manifest => {
    const submission = manifest.submissions.find(item => item.artifact_path === artifactName);
    assert(submission, "self-test submission not found");
    submission.artifact_sha256 = sha256File(artifactPath);
  });
}

function writeJson(path, value) {
  writeFileSync(path, `${JSON.stringify(value, null, 2)}\n`, "utf8");
}

const isDirectExecution = process.argv[1]
  && pathToFileURL(resolve(process.argv[1])).href === import.meta.url;
if (isDirectExecution) {
  main().catch(error => {
    process.stderr.write(`CAPACITY_DATA_INTAKE_FAIL — ${safeError(error)}\n`);
    process.exitCode = 1;
  });
}
