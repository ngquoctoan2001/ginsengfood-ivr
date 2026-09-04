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
const SCHEMA_VERSION = "m8-upstream-session-signoff.v1";
const WORK_ID = "W-0181";
const TEMPLATE_STATUS = "PENDING_M3_SIGNOFF";
const COMPLETE_STATUS = "M3_SIGNOFF_COMPLETE";
const PLACEHOLDER = "PENDING_EXTERNAL_ARTIFACT";
const SOURCE_PINS = Object.freeze({
  w0146_evidence_path: "docs/evidence/W-0146/README.md",
  w0146_evidence_sha256_lf:
    "0bce4b3fcc0e6d1145f676e619405d8319e0480d4d0af255fd693adfb73b849b",
  m3_handover_path: "integration-requirements/06-module-3-api-handover.md",
  m3_handover_sha256_lf:
    "b676a32d4ba51b9f345eb3d32e21d793216f4011e98bbfc9dc8d2867997ba08a",
});

const ROOT_KEYS = [
  "schema_version",
  "work_id",
  "status",
  "source",
  "decision",
  "producer",
  "cdc",
  "cutover",
  "signoff",
  "safety",
];
const SOURCE_KEYS = Object.keys(SOURCE_PINS);
const DECISION_KEYS = [
  "outcome",
  "field_name",
  "type",
  "min_length",
  "max_length",
  "format",
  "case_sensitive",
  "golden_hour_required",
  "twenty_four_seven_prohibited",
  "null_prohibited",
  "owner_service_alias",
  "issue_point_ref",
  "namespace_rule_ref",
  "stability_rule_ref",
  "retention_policy_ref",
  "privacy_classification",
];
const PRODUCER_KEYS = [
  "repository_alias",
  "commit_sha",
  "client_revision_ref",
  "contract_artifact_ref",
  "contract_artifact_sha256",
];
const CDC_KEYS = [
  "report_ref",
  "report_sha256",
  "m8_candidate_sha",
  "m3_candidate_sha",
  "golden_hour_case_passed",
  "twenty_four_seven_case_passed",
  "replay_case_passed",
  "changed_session_conflict_passed",
  "capacity_incident_case_passed",
];
const CUTOVER_KEYS = [
  "store_phase_at",
  "producer_enable_at",
  "enforce_phase_at",
  "compatibility_window_ref",
  "rollback_ref",
  "target_db_inventory_ref",
  "migration_not_started",
];
const SIGNOFF_KEYS = [
  "signer_identity_alias",
  "signer_role",
  "signer_organization_alias",
  "signed_at",
  "authority_ref",
  "signature_ref",
  "signature_sha256",
  "independent_verifier_alias",
  "verification_at",
];
const SAFETY_KEYS = [
  "contains_personal_contact_details",
  "contains_credentials_or_secrets",
  "raw_external_payload_embedded",
  "openapi_changed",
  "runtime_or_database_changed",
  "production_gate_promoted",
  "real_customer_call_allowed",
];
const EXPECTED_FLAGS = new Map([
  ["--expected-m8-candidate-sha", "m8_candidate_sha"],
  ["--expected-m3-producer-sha", "m3_producer_sha"],
  ["--expected-cdc-sha", "cdc_sha"],
  ["--expected-signature-sha", "signature_sha"],
]);

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

function strictUtf8(bytes, label) {
  if (bytes.length >= 3 && bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf) {
    fail(`${label} must not contain a UTF-8 BOM`);
  }
  try {
    return new TextDecoder("utf-8", { fatal: true }).decode(bytes);
  } catch {
    fail(`${label} must be strict UTF-8`);
  }
}

function canonicalLfSha256(path) {
  const bytes = readConfinedBytes(path, MAX_SOURCE_BYTES);
  const text = strictUtf8(bytes, path);
  const normalized = text.replaceAll("\r\n", "\n");
  if (normalized.includes("\r")) fail(`${path} contains a lone carriage return`);
  return sha256(Buffer.from(normalized, "utf8"));
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

function readJson(inputPath) {
  const bytes = readConfinedBytes(inputPath, MAX_INPUT_BYTES);
  const text = strictUtf8(bytes, "input");
  rejectDuplicateJsonKeys(text);
  try {
    return { bytes, document: JSON.parse(text) };
  } catch {
    fail("input must be valid JSON");
  }
}

function assertString(value, label, minimum = 1, maximum = 256) {
  if (typeof value !== "string" || value.length < minimum || value.length > maximum) {
    fail(`${label} must be a string with length ${minimum}..${maximum}`);
  }
  if (value !== value.trim() || /[\u0000-\u001f\u007f]/u.test(value)) {
    fail(`${label} contains control or edge whitespace`);
  }
}

function assertNoSensitiveValue(value, label) {
  if (/[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}/iu.test(value)) {
    fail(`${label} contains an email-like value`);
  }
  if (/(?:^|\D)(?:\+?\d[\s().-]*){9,15}(?:$|\D)/u.test(value)) {
    fail(`${label} contains a phone-like value`);
  }
  if (/\b\d{1,5}\s+(?:\u0111\u01b0\u1eddng|\u0064uong|phố|pho|street|st\.?|road|rd\.?|avenue|ave\.?)\b/iu.test(value)) {
    fail(`${label} contains a street-address-like value`);
  }
  if (
    /(?:password|passwd|bearer(?:\s+|[:=])|api[_ -]?key|access[_ -]?token|private[_ -]?key|client[_ -]?secret)\s*[:=]?/iu.test(value) ||
    /\beyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\b/u.test(value)
  ) {
    fail(`${label} contains credential- or secret-like material`);
  }
}

function assertSafeReference(value, label) {
  assertString(value, label, 5, 256);
  assertNoSensitiveValue(value, label);
  if (!/^[A-Z0-9][A-Z0-9._:/#@+-]+$/u.test(value)) {
    fail(`${label} must be an uppercase alias/reference`);
  }
}

function assertHex(value, length, label) {
  if (typeof value !== "string" || !new RegExp(`^[0-9a-f]{${length}}$`, "u").test(value)) {
    fail(`${label} must be lowercase ${length}-hex`);
  }
}

function parseTimestamp(value, label) {
  assertString(value, label, 20, 35);
  if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,3})?(?:Z|[+-]\d{2}:\d{2})$/u.test(value)) {
    fail(`${label} must be an ISO-8601 timestamp with timezone`);
  }
  const parsed = Date.parse(value);
  if (!Number.isFinite(parsed)) fail(`${label} is not a valid timestamp`);
  return parsed;
}

function assertSource(source) {
  assertExactKeys(source, SOURCE_KEYS, "source");
  for (const [key, expected] of Object.entries(SOURCE_PINS)) {
    if (source[key] !== expected) fail(`source.${key} must match the pinned W-0146 source`);
  }
  for (const [pathKey, hashKey] of [
    ["w0146_evidence_path", "w0146_evidence_sha256_lf"],
    ["m3_handover_path", "m3_handover_sha256_lf"],
  ]) {
    const actual = canonicalLfSha256(source[pathKey]);
    if (actual !== source[hashKey]) fail(`${source[pathKey]} drifted from the canonical-LF source pin`);
  }
}

function assertDecision(decision, templateMode) {
  assertExactKeys(decision, DECISION_KEYS, "decision");
  if (decision.outcome !== (templateMode ? "PENDING" : "ACCEPT")) {
    fail(`decision.outcome must be ${templateMode ? "PENDING" : "ACCEPT"}`);
  }
  const exact = {
    field_name: "golden_hour_session_id",
    type: "STRING",
    min_length: 1,
    max_length: 128,
    format: "OPAQUE_NO_CONTROL_NO_EDGE_WHITESPACE",
    case_sensitive: true,
    golden_hour_required: true,
    twenty_four_seven_prohibited: true,
    null_prohibited: true,
    owner_service_alias: "MODULE3_GOLDEN_HOUR_CORE",
    privacy_classification: "TECHNICAL_IDENTIFIER_NO_PII",
  };
  for (const [key, expected] of Object.entries(exact)) {
    if (decision[key] !== expected) fail(`decision.${key} must be ${JSON.stringify(expected)}`);
  }
  for (const key of [
    "issue_point_ref",
    "namespace_rule_ref",
    "stability_rule_ref",
    "retention_policy_ref",
  ]) {
    if (templateMode) {
      if (decision[key] !== PLACEHOLDER) fail(`decision.${key} must be the pending placeholder`);
    } else {
      assertSafeReference(decision[key], `decision.${key}`);
    }
  }
}

function assertSafety(safety) {
  assertExactKeys(safety, SAFETY_KEYS, "safety");
  for (const key of SAFETY_KEYS) {
    if (safety[key] !== false) fail(`safety.${key} must remain false`);
  }
}

function assertTemplateExternal(document) {
  for (const [sectionName, keys] of [
    ["producer", PRODUCER_KEYS],
    ["cdc", CDC_KEYS],
    ["cutover", CUTOVER_KEYS],
    ["signoff", SIGNOFF_KEYS],
  ]) {
    assertExactKeys(document[sectionName], keys, sectionName);
  }
  for (const key of PRODUCER_KEYS) {
    if (document.producer[key] !== PLACEHOLDER) fail(`producer.${key} must be pending`);
  }
  for (const key of ["report_ref", "report_sha256", "m8_candidate_sha", "m3_candidate_sha"]) {
    if (document.cdc[key] !== PLACEHOLDER) fail(`cdc.${key} must be pending`);
  }
  for (const key of CDC_KEYS.filter((key) => key.endsWith("_passed"))) {
    if (document.cdc[key] !== false) fail(`cdc.${key} must be false in the pending template`);
  }
  for (const key of CUTOVER_KEYS.filter((key) => key !== "migration_not_started")) {
    if (document.cutover[key] !== PLACEHOLDER) fail(`cutover.${key} must be pending`);
  }
  if (document.cutover.migration_not_started !== true) {
    fail("cutover.migration_not_started must be true");
  }
  for (const key of SIGNOFF_KEYS) {
    if (document.signoff[key] !== PLACEHOLDER) fail(`signoff.${key} must be pending`);
  }
}

function assertCompletedExternal(document, expectedPins) {
  assertExactKeys(document.producer, PRODUCER_KEYS, "producer");
  assertExactKeys(document.cdc, CDC_KEYS, "cdc");
  assertExactKeys(document.cutover, CUTOVER_KEYS, "cutover");
  assertExactKeys(document.signoff, SIGNOFF_KEYS, "signoff");

  assertSafeReference(document.producer.repository_alias, "producer.repository_alias");
  assertHex(document.producer.commit_sha, 40, "producer.commit_sha");
  assertSafeReference(document.producer.client_revision_ref, "producer.client_revision_ref");
  assertSafeReference(document.producer.contract_artifact_ref, "producer.contract_artifact_ref");
  assertHex(document.producer.contract_artifact_sha256, 64, "producer.contract_artifact_sha256");

  assertSafeReference(document.cdc.report_ref, "cdc.report_ref");
  assertHex(document.cdc.report_sha256, 64, "cdc.report_sha256");
  assertHex(document.cdc.m8_candidate_sha, 40, "cdc.m8_candidate_sha");
  assertHex(document.cdc.m3_candidate_sha, 40, "cdc.m3_candidate_sha");
  for (const key of CDC_KEYS.filter((key) => key.endsWith("_passed"))) {
    if (document.cdc[key] !== true) fail(`cdc.${key} must be true`);
  }
  if (document.cdc.m3_candidate_sha !== document.producer.commit_sha) {
    fail("cdc.m3_candidate_sha must equal producer.commit_sha");
  }

  const storeAt = parseTimestamp(document.cutover.store_phase_at, "cutover.store_phase_at");
  const producerAt = parseTimestamp(document.cutover.producer_enable_at, "cutover.producer_enable_at");
  const enforceAt = parseTimestamp(document.cutover.enforce_phase_at, "cutover.enforce_phase_at");
  if (!(storeAt <= producerAt && producerAt < enforceAt)) {
    fail("cutover order must be store_phase_at <= producer_enable_at < enforce_phase_at");
  }
  for (const key of ["compatibility_window_ref", "rollback_ref", "target_db_inventory_ref"]) {
    assertSafeReference(document.cutover[key], `cutover.${key}`);
  }
  if (document.cutover.migration_not_started !== true) {
    fail("cutover.migration_not_started must prove code and DB are still unchanged");
  }

  for (const key of [
    "signer_identity_alias",
    "signer_role",
    "signer_organization_alias",
    "authority_ref",
    "signature_ref",
    "independent_verifier_alias",
  ]) {
    assertSafeReference(document.signoff[key], `signoff.${key}`);
  }
  if (document.signoff.signer_role !== "MODULE3_GOLDEN_HOUR_CONTRACT_OWNER") {
    fail("signoff.signer_role must be MODULE3_GOLDEN_HOUR_CONTRACT_OWNER");
  }
  if (document.signoff.signer_identity_alias === document.signoff.independent_verifier_alias) {
    fail("signer and independent verifier must be different aliases");
  }
  const signedAt = parseTimestamp(document.signoff.signed_at, "signoff.signed_at");
  const verifiedAt = parseTimestamp(document.signoff.verification_at, "signoff.verification_at");
  if (verifiedAt < signedAt) fail("verification_at must not precede signed_at");
  assertHex(document.signoff.signature_sha256, 64, "signoff.signature_sha256");

  if (document.cdc.m8_candidate_sha !== expectedPins.m8_candidate_sha) {
    fail("M8 candidate SHA does not match the independent reviewer pin");
  }
  if (document.producer.commit_sha !== expectedPins.m3_producer_sha) {
    fail("M3 producer SHA does not match the independent reviewer pin");
  }
  if (document.cdc.report_sha256 !== expectedPins.cdc_sha) {
    fail("CDC hash does not match the independent reviewer pin");
  }
  if (document.signoff.signature_sha256 !== expectedPins.signature_sha) {
    fail("signature hash does not match the independent reviewer pin");
  }
}

function validateDocument(document, mode, expectedPins = null) {
  assertExactKeys(document, ROOT_KEYS, "root");
  if (document.schema_version !== SCHEMA_VERSION) fail(`schema_version must be ${SCHEMA_VERSION}`);
  if (document.work_id !== WORK_ID) fail(`work_id must be ${WORK_ID}`);
  assertSource(document.source);
  assertDecision(document.decision, mode === "template");
  assertSafety(document.safety);

  if (mode === "template") {
    if (document.status !== TEMPLATE_STATUS) fail(`status must be ${TEMPLATE_STATUS}`);
    assertTemplateExternal(document);
  } else {
    if (document.status !== COMPLETE_STATUS) fail(`status must be ${COMPLETE_STATUS}`);
    if (!expectedPins) fail("independent reviewer pins are required");
    assertCompletedExternal(document, expectedPins);
    const serialized = JSON.stringify(document);
    if (serialized.includes(PLACEHOLDER)) fail("completed input must not contain pending placeholders");
    assertNoSensitiveValue(serialized, "document");
  }
}

function validateFile(path, mode, expectedPins = null) {
  const { bytes, document } = readJson(path);
  validateDocument(document, mode, expectedPins);
  return { inputSha256: sha256(bytes) };
}

function templateDocument() {
  return {
    schema_version: SCHEMA_VERSION,
    work_id: WORK_ID,
    status: TEMPLATE_STATUS,
    source: { ...SOURCE_PINS },
    decision: {
      outcome: "PENDING",
      field_name: "golden_hour_session_id",
      type: "STRING",
      min_length: 1,
      max_length: 128,
      format: "OPAQUE_NO_CONTROL_NO_EDGE_WHITESPACE",
      case_sensitive: true,
      golden_hour_required: true,
      twenty_four_seven_prohibited: true,
      null_prohibited: true,
      owner_service_alias: "MODULE3_GOLDEN_HOUR_CORE",
      issue_point_ref: PLACEHOLDER,
      namespace_rule_ref: PLACEHOLDER,
      stability_rule_ref: PLACEHOLDER,
      retention_policy_ref: PLACEHOLDER,
      privacy_classification: "TECHNICAL_IDENTIFIER_NO_PII",
    },
    producer: Object.fromEntries(PRODUCER_KEYS.map((key) => [key, PLACEHOLDER])),
    cdc: {
      report_ref: PLACEHOLDER,
      report_sha256: PLACEHOLDER,
      m8_candidate_sha: PLACEHOLDER,
      m3_candidate_sha: PLACEHOLDER,
      golden_hour_case_passed: false,
      twenty_four_seven_case_passed: false,
      replay_case_passed: false,
      changed_session_conflict_passed: false,
      capacity_incident_case_passed: false,
    },
    cutover: {
      store_phase_at: PLACEHOLDER,
      producer_enable_at: PLACEHOLDER,
      enforce_phase_at: PLACEHOLDER,
      compatibility_window_ref: PLACEHOLDER,
      rollback_ref: PLACEHOLDER,
      target_db_inventory_ref: PLACEHOLDER,
      migration_not_started: true,
    },
    signoff: Object.fromEntries(SIGNOFF_KEYS.map((key) => [key, PLACEHOLDER])),
    safety: Object.fromEntries(SAFETY_KEYS.map((key) => [key, false])),
  };
}

function validCompletedDocument() {
  const document = templateDocument();
  document.status = COMPLETE_STATUS;
  document.decision.outcome = "ACCEPT";
  document.decision.issue_point_ref = "M3:GH_SESSION_ACTIVATION_STEP";
  document.decision.namespace_rule_ref = "M3:GH_SESSION_NAMESPACE_V1";
  document.decision.stability_rule_ref = "M3:GH_SESSION_STABILITY_V1";
  document.decision.retention_policy_ref = "M3:GH_SESSION_RETENTION_V1";
  document.producer = {
    repository_alias: "MODULE3_ORDER_CORE",
    commit_sha: "2".repeat(40),
    client_revision_ref: "M3:GENERATED_CLIENT_REVISION_01",
    contract_artifact_ref: "M3:OPENAPI_ARTIFACT_01",
    contract_artifact_sha256: "3".repeat(64),
  };
  document.cdc = {
    report_ref: "CDC:UPSTREAM_SESSION_REPORT_01",
    report_sha256: "4".repeat(64),
    m8_candidate_sha: "1".repeat(40),
    m3_candidate_sha: "2".repeat(40),
    golden_hour_case_passed: true,
    twenty_four_seven_case_passed: true,
    replay_case_passed: true,
    changed_session_conflict_passed: true,
    capacity_incident_case_passed: true,
  };
  document.cutover = {
    store_phase_at: "2026-09-10T09:00:00+07:00",
    producer_enable_at: "2026-09-11T09:00:00+07:00",
    enforce_phase_at: "2026-09-15T09:00:00+07:00",
    compatibility_window_ref: "M3:COMPATIBILITY_WINDOW_01",
    rollback_ref: "M3:ROLLBACK_RUNBOOK_01",
    target_db_inventory_ref: "M8:TARGET_DB_INVENTORY_01",
    migration_not_started: true,
  };
  document.signoff = {
    signer_identity_alias: "M3_CONTRACT_OWNER_ALIAS",
    signer_role: "MODULE3_GOLDEN_HOUR_CONTRACT_OWNER",
    signer_organization_alias: "MODULE3",
    signed_at: "2026-09-09T14:00:00+07:00",
    authority_ref: "M3:ROLE_ASSIGNMENT_01",
    signature_ref: "M3:SIGNATURE_ARTIFACT_01",
    signature_sha256: "5".repeat(64),
    independent_verifier_alias: "M8_CHIEF_AUDITOR_ALIAS",
    verification_at: "2026-09-09T14:30:00+07:00",
  };
  return document;
}

function expectedPins() {
  return {
    m8_candidate_sha: "1".repeat(40),
    m3_producer_sha: "2".repeat(40),
    cdc_sha: "4".repeat(64),
    signature_sha: "5".repeat(64),
  };
}

function clone(value) {
  return JSON.parse(JSON.stringify(value));
}

function runSelfTest() {
  const temporaryRoot = mkdtempSync(join(REPOSITORY_ROOT, ".w0178-selftest-"));
  let refusals = 0;
  const writeCase = (name, value) => {
    const path = join(temporaryRoot, `${name}.json`);
    writeFileSync(path, typeof value === "string" ? value : `${JSON.stringify(value, null, 2)}\n`, "utf8");
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
    delete missing.decision.field_name;
    expectFailure("missing-key", missing);
    const extra = clone(valid);
    extra.decision.alias = "session_id";
    expectFailure("extra-key", extra);
    const sourceDrift = clone(valid);
    sourceDrift.source.w0146_evidence_sha256_lf = "0".repeat(64);
    expectFailure("source-pin", sourceDrift);
    const rejection = clone(valid);
    rejection.decision.outcome = "REJECT_WITH_REPLACEMENT";
    expectFailure("rejection-needs-revision", rejection);
    const alias = clone(valid);
    alias.decision.field_name = "session_id";
    expectFailure("field-alias", alias);
    const wrongOwner = clone(valid);
    wrongOwner.decision.owner_service_alias = "M8";
    expectFailure("wrong-owner", wrongOwner);
    const programRule = clone(valid);
    programRule.decision.twenty_four_seven_prohibited = false;
    expectFailure("program-rule", programRule);
    const normalize = clone(valid);
    normalize.decision.case_sensitive = false;
    expectFailure("normalization", normalize);
    const badLength = clone(valid);
    badLength.decision.max_length = 256;
    expectFailure("length", badLength);
    const producerPin = clone(valid);
    producerPin.producer.commit_sha = "6".repeat(40);
    producerPin.cdc.m3_candidate_sha = "6".repeat(40);
    expectFailure("producer-pin", producerPin);
    const cdcPin = clone(valid);
    cdcPin.cdc.report_sha256 = "6".repeat(64);
    expectFailure("cdc-pin", cdcPin);
    const signaturePin = clone(valid);
    signaturePin.signoff.signature_sha256 = "6".repeat(64);
    expectFailure("signature-pin", signaturePin);
    const m8Pin = clone(valid);
    m8Pin.cdc.m8_candidate_sha = "6".repeat(40);
    expectFailure("m8-pin", m8Pin);
    const crossRepo = clone(valid);
    crossRepo.cdc.m3_candidate_sha = "6".repeat(40);
    expectFailure("cross-repo", crossRepo);
    const partialCdc = clone(valid);
    partialCdc.cdc.replay_case_passed = false;
    expectFailure("partial-cdc", partialCdc);
    const cutoverOrder = clone(valid);
    cutoverOrder.cutover.enforce_phase_at = "2026-09-10T08:00:00+07:00";
    expectFailure("cutover-order", cutoverOrder);
    const migrated = clone(valid);
    migrated.cutover.migration_not_started = false;
    expectFailure("migration-started", migrated);
    const sameSigner = clone(valid);
    sameSigner.signoff.independent_verifier_alias = sameSigner.signoff.signer_identity_alias;
    expectFailure("same-signer", sameSigner);
    const wrongRole = clone(valid);
    wrongRole.signoff.signer_role = "PROJECT_MEMBER";
    expectFailure("wrong-role", wrongRole);
    const earlyVerify = clone(valid);
    earlyVerify.signoff.verification_at = "2026-09-09T13:59:00+07:00";
    expectFailure("early-verification", earlyVerify);
    const unsafe = clone(valid);
    unsafe.safety.openapi_changed = true;
    expectFailure("unsafe-openapi", unsafe);
    const realCall = clone(valid);
    realCall.safety.real_customer_call_allowed = true;
    expectFailure("real-call", realCall);
    const placeholder = clone(valid);
    placeholder.producer.client_revision_ref = PLACEHOLDER;
    expectFailure("placeholder", placeholder);
    const email = clone(valid);
    email.signoff.signature_ref = "OWNER@EXAMPLE.INVALID";
    expectFailure("email", email);
    const phone = clone(valid);
    phone.cutover.rollback_ref = ["CALL:+84", "912345678"].join("");
    expectFailure("phone", phone);
    const secret = clone(valid);
    secret.signoff.authority_ref = "BEARER:ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    expectFailure("secret", secret);
    const badTimestamp = clone(valid);
    badTimestamp.signoff.signed_at = "2026-09-09";
    expectFailure("timestamp", badTimestamp);
    expectFailure("malformed", '{"schema_version":');
    expectFailure("duplicate-key", '{"schema_version":"a","schema_version":"b"}');
    expectFailure("oversized", " ".repeat(MAX_INPUT_BYTES + 1));
    try {
      validateFile(join(REPOSITORY_ROOT, "..", "outside-w0178.json"), "input", expectedPins());
      fail("self-test outside-root unexpectedly passed");
    } catch (error) {
      if (error.message.includes("unexpectedly passed")) throw error;
      refusals += 1;
    }

    return { templateChecks: 1, valid: 1, refusals };
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
  if (seen.size !== EXPECTED_FLAGS.size) fail("all four independent reviewer pins are required");
  assertHex(expected.m8_candidate_sha, 40, "expected M8 candidate SHA");
  assertHex(expected.m3_producer_sha, 40, "expected M3 producer SHA");
  assertHex(expected.cdc_sha, 64, "expected CDC SHA");
  assertHex(expected.signature_sha, 64, "expected signature SHA");
  return { inputPath, expected };
}

function usage() {
  return [
    "Usage:",
    "  node deploy/ci/scripts/upstream-session-signoff-validator.mjs --check-template <json>",
    "  node deploy/ci/scripts/upstream-session-signoff-validator.mjs --self-test",
    "  node deploy/ci/scripts/upstream-session-signoff-validator.mjs --input <json> \\",
    "    --expected-m8-candidate-sha <40hex> --expected-m3-producer-sha <40hex> \\",
    "    --expected-cdc-sha <64hex> --expected-signature-sha <64hex>",
  ].join("\n");
}

function main(argv) {
  if (argv.length === 1 && argv[0] === "--self-test") {
    const result = runSelfTest();
    console.log(
      `W0181_SELFTEST_PASS template=${result.templateChecks} valid=${result.valid} refusals=${result.refusals}`,
    );
    return;
  }
  if (argv.length === 2 && argv[0] === "--check-template") {
    const result = validateFile(argv[1], "template");
    console.log(`UPSTREAM_SESSION_TEMPLATE_VALID_NOT_READY sha256=${result.inputSha256}`);
    return;
  }
  const { inputPath, expected } = parseInputArguments(argv);
  const result = validateFile(inputPath, "input", expected);
  console.log(
    `UPSTREAM_SESSION_SIGNOFF_VALID_ELIGIBLE_FOR_IMPLEMENTATION_REVIEW_ONLY sha256=${result.inputSha256}`,
  );
}

try {
  main(process.argv.slice(2));
} catch (error) {
  console.error(`W0181_VALIDATION_FAILED: ${error.message}`);
  process.exitCode = 1;
}
