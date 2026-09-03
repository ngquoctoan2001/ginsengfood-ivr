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
const MAX_INPUT_BYTES = 64 * 1024;
const SCHEMA_VERSION = "m8-external-decision-routing.v1";
const WORK_ID = "W-0164";
const PLACEHOLDER = "PENDING_OWNER_INPUT";
const BATCH_IDS = ["D-01", "D-02", "D-03", "D-04", "D-05"];
const ALLOWED_CHANNEL_KINDS = new Set([
  "GITLAB_ISSUE",
  "JIRA_TICKET",
  "EMAIL_ALIAS",
  "SLACK_CHANNEL",
  "TEAMS_CHANNEL",
  "OTHER_APPROVED",
]);
const SOURCE_PINS = Object.freeze({
  dispatch_pack_path:
    "plan/ivr-orther/m8-12-external-decision-provenance-dispatch-pack-2026-09-03.md",
  dispatch_pack_sha256:
    "691568b3fa48e613ecab1c52835e40f483073698d4aa1c8b1a41df5d42d34fe0",
  artifact_manifest_path: "docs/evidence/W-0152/artifact-sha256.txt",
  artifact_manifest_sha256:
    "49ed4c153bb71db1cad6c1af446fe3c3c1892cd40b4d8355441868d60c349406",
  message_kit_path:
    "plan/ivr-orther/m8-13-external-decision-dispatch-message-kit-2026-09-03.md",
  message_kit_sha256:
    "261b33fd4832793240b837e090efe7424929278d454da98a9a454cfdcfacc103",
});

const ROOT_KEYS = ["schema_version", "work_id", "status", "source", "batches", "safety"];
const SOURCE_KEYS = Object.keys(SOURCE_PINS);
const BATCH_KEYS = [
  "batch",
  "recipient_identity",
  "role_organization",
  "authority_source_ref",
  "channel_kind",
  "destination_ref",
  "due_at",
  "dispatch_authorized_by",
  "dispatch_authorized_at",
  "state",
];
const SAFETY_KEYS = [
  "contains_personal_contact_details",
  "contains_credentials_or_secrets",
  "external_dispatch_performed",
  "receipt_recorded",
];
const ROUTING_VALUE_FIELDS = [
  "recipient_identity",
  "role_organization",
  "authority_source_ref",
  "channel_kind",
  "destination_ref",
  "dispatch_authorized_by",
];
const ALL_INPUT_FIELDS = [
  ...ROUTING_VALUE_FIELDS,
  "due_at",
  "dispatch_authorized_at",
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

function readConfinedUtf8File(inputPath) {
  const resolvedPath = resolve(REPOSITORY_ROOT, inputPath);
  if (!isConfined(resolvedPath)) {
    fail("input path must stay inside the repository root");
  }

  const entry = lstatSync(resolvedPath);
  if (!entry.isFile() || entry.isSymbolicLink()) {
    fail("input path must be a regular non-symlink file");
  }
  if (entry.size > MAX_INPUT_BYTES) {
    fail(`input exceeds ${MAX_INPUT_BYTES} bytes`);
  }

  const realPath = realpathSync(resolvedPath);
  if (!isConfined(realPath)) {
    fail("resolved input path escapes the repository root");
  }

  const bytes = readFileSync(realPath);
  if (bytes.length >= 3 && bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf) {
    fail("UTF-8 BOM is not allowed");
  }

  let textValue;
  try {
    textValue = new TextDecoder("utf-8", { fatal: true }).decode(bytes);
  } catch {
    fail("input must be strict UTF-8");
  }
  return { bytes, text: textValue };
}

function rejectDuplicateJsonKeys(textValue) {
  let position = 0;

  function skipWhitespace() {
    while (/\s/u.test(textValue[position] ?? "")) position += 1;
  }

  function parseString() {
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
  }

  function parseLiteral() {
    const match = /^(?:true|false|null|-?(?:0|[1-9]\d*)(?:\.\d+)?(?:[eE][+-]?\d+)?)/u.exec(
      textValue.slice(position),
    );
    if (!match) fail("invalid JSON value");
    position += match[0].length;
  }

  function parseArray() {
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
  }

  function parseObject() {
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
  }

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
  const { bytes, text: textValue } = readConfinedUtf8File(inputPath);
  rejectDuplicateJsonKeys(textValue);
  let document;
  try {
    document = JSON.parse(textValue);
  } catch (error) {
    fail(`malformed JSON: ${error.message}`);
  }
  return { bytes, document };
}

function verifySourcePins(source) {
  assertExactKeys(source, SOURCE_KEYS, "source");
  for (const key of SOURCE_KEYS) {
    if (source[key] !== SOURCE_PINS[key]) {
      fail(`source.${key} does not match the W-0164 pinned value`);
    }
  }

  for (const prefix of ["dispatch_pack", "artifact_manifest", "message_kit"]) {
    const artifactPath = SOURCE_PINS[`${prefix}_path`];
    const expectedHash = SOURCE_PINS[`${prefix}_sha256`];
    const artifactBytes = readConfinedUtf8File(artifactPath).bytes;
    if (sha256(artifactBytes) !== expectedHash) {
      fail(`${artifactPath} drifted from the pinned SHA-256`);
    }
  }
}

function assertString(value, label, minimum, maximum) {
  if (typeof value !== "string" || value.length < minimum || value.length > maximum) {
    fail(`${label} must be a string of ${minimum}..${maximum} characters`);
  }
  if (value.trim() !== value) fail(`${label} must not have leading or trailing whitespace`);
  if (/[\u0000-\u001f\u007f]/u.test(value)) fail(`${label} contains a control character`);
}

function assertNoSensitiveValue(value, label) {
  if (/[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}/iu.test(value)) {
    fail(`${label} contains a personal email-like value`);
  }
  if (/(?:^|\D)(?:\+?\d[\s().-]*){9,15}(?:$|\D)/u.test(value)) {
    fail(`${label} contains a phone-like value`);
  }
  if (/\b\d{1,5}\s+(?:đường|duong|phố|pho|street|st\.?|road|rd\.?|avenue|ave\.?)\b/iu.test(value)) {
    fail(`${label} contains a street-address-like value`);
  }
  if (
    /(?:password|passwd|bearer\s+|api[_ -]?key|access[_ -]?token|private[_ -]?key|client[_ -]?secret)\s*[:=]?/iu.test(
      value,
    ) ||
    /\beyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\b/u.test(value)
  ) {
    fail(`${label} contains credential- or secret-like material`);
  }
}

function assertTimestamp(value, label) {
  assertString(value, label, 20, 35);
  if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,3})?(?:Z|[+-]\d{2}:\d{2})$/u.test(value)) {
    fail(`${label} must be ISO-8601 with an explicit timezone`);
  }
  if (!Number.isFinite(Date.parse(value))) fail(`${label} is not a valid timestamp`);
}

function validateReadyBatch(batch, index) {
  for (const field of ALL_INPUT_FIELDS) {
    if (batch[field] === PLACEHOLDER) fail(`batches[${index}].${field} is still pending`);
  }

  assertString(batch.recipient_identity, `batches[${index}].recipient_identity`, 3, 160);
  assertString(batch.role_organization, `batches[${index}].role_organization`, 3, 200);
  assertString(batch.authority_source_ref, `batches[${index}].authority_source_ref`, 5, 300);
  assertString(batch.channel_kind, `batches[${index}].channel_kind`, 3, 32);
  assertString(batch.destination_ref, `batches[${index}].destination_ref`, 3, 300);
  assertString(batch.dispatch_authorized_by, `batches[${index}].dispatch_authorized_by`, 3, 160);
  if (!ALLOWED_CHANNEL_KINDS.has(batch.channel_kind)) {
    fail(`batches[${index}].channel_kind is not allowlisted`);
  }

  for (const field of ROUTING_VALUE_FIELDS) {
    assertNoSensitiveValue(batch[field], `batches[${index}].${field}`);
  }
  assertTimestamp(batch.due_at, `batches[${index}].due_at`);
  assertTimestamp(batch.dispatch_authorized_at, `batches[${index}].dispatch_authorized_at`);
  if (Date.parse(batch.due_at) <= Date.parse(batch.dispatch_authorized_at)) {
    fail(`batches[${index}].due_at must be after dispatch_authorized_at`);
  }
}

function validatePendingBatch(batch, index) {
  for (const field of ALL_INPUT_FIELDS) {
    if (batch[field] !== PLACEHOLDER) {
      fail(`batches[${index}] NOT_READY row must keep ${field} at ${PLACEHOLDER}`);
    }
  }
}

function validateDocument(document, mode) {
  assertExactKeys(document, ROOT_KEYS, "root");
  if (document.schema_version !== SCHEMA_VERSION) fail("schema_version is not supported");
  if (document.work_id !== WORK_ID) fail("work_id must be W-0164");
  verifySourcePins(document.source);

  assertExactKeys(document.safety, SAFETY_KEYS, "safety");
  for (const key of SAFETY_KEYS) {
    if (document.safety[key] !== false) fail(`safety.${key} must remain false`);
  }

  if (!Array.isArray(document.batches) || document.batches.length !== BATCH_IDS.length) {
    fail("batches must contain exactly D-01..D-05");
  }

  let readyCount = 0;
  document.batches.forEach((batch, index) => {
    assertExactKeys(batch, BATCH_KEYS, `batches[${index}]`);
    if (batch.batch !== BATCH_IDS[index]) {
      fail(`batches[${index}].batch must be ${BATCH_IDS[index]}`);
    }
    if (batch.state === "NOT_READY") validatePendingBatch(batch, index);
    else if (batch.state === "READY_FOR_HASH_RECHECK_AND_DISPATCH") {
      validateReadyBatch(batch, index);
      readyCount += 1;
    } else fail(`batches[${index}].state is invalid`);
  });

  const expectedStatus =
    readyCount === 0
      ? "PENDING_OWNER_INPUT"
      : readyCount === BATCH_IDS.length
        ? "READY_FOR_HASH_RECHECK_AND_DISPATCH"
        : "PARTIAL_READY";
  if (document.status !== expectedStatus) {
    fail(`status must be ${expectedStatus} for ${readyCount} ready batch(es)`);
  }

  if (mode === "template") {
    if (readyCount !== 0) fail("template mode requires all rows to be NOT_READY");
  } else if (readyCount === 0) {
    fail("dispatch-readiness mode requires at least one fully ready batch");
  }

  return { readyCount, pendingCount: BATCH_IDS.length - readyCount };
}

function validateFile(inputPath, mode) {
  const { bytes, document } = parseInput(inputPath);
  const counts = validateDocument(document, mode);
  return { ...counts, inputSha256: sha256(bytes) };
}

function pendingBatch(batch) {
  return {
    batch,
    recipient_identity: PLACEHOLDER,
    role_organization: PLACEHOLDER,
    authority_source_ref: PLACEHOLDER,
    channel_kind: PLACEHOLDER,
    destination_ref: PLACEHOLDER,
    due_at: PLACEHOLDER,
    dispatch_authorized_by: PLACEHOLDER,
    dispatch_authorized_at: PLACEHOLDER,
    state: "NOT_READY",
  };
}

function templateDocument() {
  return {
    schema_version: SCHEMA_VERSION,
    work_id: WORK_ID,
    status: "PENDING_OWNER_INPUT",
    source: { ...SOURCE_PINS },
    batches: BATCH_IDS.map(pendingBatch),
    safety: {
      contains_personal_contact_details: false,
      contains_credentials_or_secrets: false,
      external_dispatch_performed: false,
      receipt_recorded: false,
    },
  };
}

function readyBatch(batch) {
  return {
    batch,
    recipient_identity: `${batch}_RECIPIENT_ALIAS`,
    role_organization: `${batch}_AUTHORIZED_OWNER_ROLE`,
    authority_source_ref: `ROLE_ASSIGNMENT:${batch}-AUTHORITY-REF`,
    channel_kind: "GITLAB_ISSUE",
    destination_ref: `PROJECT:EXTERNAL-GOVERNANCE/${batch}`,
    due_at: "2026-09-10T17:00:00+07:00",
    dispatch_authorized_by: "MODULE_8_OWNER_ALIAS",
    dispatch_authorized_at: "2026-09-03T12:00:00+07:00",
    state: "READY_FOR_HASH_RECHECK_AND_DISPATCH",
  };
}

function clone(value) {
  return JSON.parse(JSON.stringify(value));
}

function runSelfTest() {
  const temporaryRoot = mkdtempSync(join(REPOSITORY_ROOT, ".w0164-selftest-"));
  let refusals = 0;
  const writeCase = (name, value) => {
    const path = join(temporaryRoot, `${name}.json`);
    writeFileSync(path, typeof value === "string" ? value : `${JSON.stringify(value, null, 2)}\n`, "utf8");
    return path;
  };
  const expectFailure = (name, value, mode = "input") => {
    const path = writeCase(name, value);
    try {
      validateFile(path, mode);
      fail(`self-test ${name} unexpectedly passed`);
    } catch (error) {
      if (error.message.includes("unexpectedly passed")) throw error;
      refusals += 1;
    }
  };

  try {
    const template = templateDocument();
    validateFile(writeCase("template-valid", template), "template");

    const partial = clone(template);
    partial.batches[0] = readyBatch("D-01");
    partial.status = "PARTIAL_READY";
    const partialResult = validateFile(writeCase("partial-valid", partial), "input");
    if (partialResult.readyCount !== 1 || partialResult.pendingCount !== 4) {
      fail("self-test partial ready counts are wrong");
    }

    const full = clone(template);
    full.batches = BATCH_IDS.map(readyBatch);
    full.status = "READY_FOR_HASH_RECHECK_AND_DISPATCH";
    const fullResult = validateFile(writeCase("full-valid", full), "input");
    if (fullResult.readyCount !== 5 || fullResult.pendingCount !== 0) {
      fail("self-test full ready counts are wrong");
    }

    expectFailure("pending-normal-mode", template);
    const missingBatch = clone(full);
    missingBatch.batches.pop();
    expectFailure("missing-batch", missingBatch);
    const duplicateBatch = clone(full);
    duplicateBatch.batches[1].batch = "D-01";
    expectFailure("duplicate-or-out-of-order-batch", duplicateBatch);
    const extraKey = clone(full);
    extraKey.batches[0].unexpected = true;
    expectFailure("extra-key", extraKey);
    const wrongHash = clone(full);
    wrongHash.source.message_kit_sha256 = "0".repeat(64);
    expectFailure("wrong-source-hash", wrongHash);
    const email = clone(full);
    email.batches[0].destination_ref = "person@example.invalid";
    expectFailure("personal-email", email);
    const phone = clone(full);
    phone.batches[0].recipient_identity = "+84 912 345 678";
    expectFailure("phone", phone);
    const address = clone(full);
    address.batches[0].destination_ref = "12 đường Test";
    expectFailure("street-address", address);
    const secret = clone(full);
    secret.batches[0].destination_ref = "Bearer abcdefghijklmnopqrstuvwxyz";
    expectFailure("secret", secret);
    const badTimestamp = clone(full);
    badTimestamp.batches[0].due_at = "2026-09-10";
    expectFailure("bad-timestamp", badTimestamp);
    const reversedTime = clone(full);
    reversedTime.batches[0].due_at = "2026-09-02T12:00:00+07:00";
    expectFailure("due-before-authorization", reversedTime);
    const unknownChannel = clone(full);
    unknownChannel.batches[0].channel_kind = "PERSONAL_SMS";
    expectFailure("unknown-channel", unknownChannel);
    const mixedPending = clone(template);
    mixedPending.batches[0].recipient_identity = "PARTIAL_ALIAS";
    expectFailure("mixed-pending-row", mixedPending, "template");
    const wrongStatus = clone(partial);
    wrongStatus.status = "READY_FOR_HASH_RECHECK_AND_DISPATCH";
    expectFailure("wrong-root-status", wrongStatus);
    const unsafeFlag = clone(full);
    unsafeFlag.safety.receipt_recorded = true;
    expectFailure("unsafe-flag", unsafeFlag);
    expectFailure("malformed-json", '{"schema_version":');
    expectFailure("duplicate-json-key", '{"schema_version":"a","schema_version":"b"}');
    expectFailure("oversized-input", " ".repeat(MAX_INPUT_BYTES + 1));

    try {
      validateFile(join(REPOSITORY_ROOT, "..", "outside-routing-input.json"), "input");
      fail("self-test outside-root unexpectedly passed");
    } catch (error) {
      if (error.message.includes("unexpectedly passed")) throw error;
      refusals += 1;
    }

    return { templateChecks: 1, validInputs: 2, refusals };
  } finally {
    rmSync(temporaryRoot, { recursive: true, force: true });
  }
}

function usage() {
  return [
    "Usage:",
    "  node deploy/ci/scripts/external-decision-routing-validator.mjs --check-template <json>",
    "  node deploy/ci/scripts/external-decision-routing-validator.mjs --input <json>",
    "  node deploy/ci/scripts/external-decision-routing-validator.mjs --self-test",
  ].join("\n");
}

function main(argv) {
  if (argv.length === 1 && argv[0] === "--self-test") {
    const result = runSelfTest();
    console.log(`W0164_SELFTEST_PASS template=${result.templateChecks} valid=${result.validInputs} refusals=${result.refusals}`);
    return;
  }

  if (argv.length !== 2 || !["--check-template", "--input"].includes(argv[0])) {
    fail(usage());
  }

  const mode = argv[0] === "--check-template" ? "template" : "input";
  const result = validateFile(argv[1], mode);
  const status = mode === "template" ? "TEMPLATE_VALID_NOT_READY" : "ROUTING_INPUT_VALID";
  console.log(
    `${status} ready=${result.readyCount} pending=${result.pendingCount} sha256=${result.inputSha256}`,
  );
}

try {
  main(process.argv.slice(2));
} catch (error) {
  console.error(`W0164_VALIDATION_FAILED: ${error.message}`);
  process.exitCode = 1;
}
