// W-0059 / P11-3 §8 — the five checks that keep the compliance pack from lying.
//
// These are review artefacts, so the failure mode is not a crash: it is a document that reads as
// approved, or complete, or traceable, and is none of those. Each check below picks one of those
// three and makes it fail loudly.
//
//   LEGAL-RET-01    every data class has an owner, a value or LEGAL_SIGNOFF_REQUIRED, and a purge
//                   mechanism.
//   LEGAL-PII-02    the raw-phone / recording / token constraints match D-05 and DT-05, and no
//                   document anywhere claims recording is on.
//   LEGAL-DSAR-03   the DSAR process documents the audit-immutability limit rather than promising
//                   an erasure it cannot perform.
//   SIGNOFF-DF03-04 the DF-03 record cannot exist without filled approval fields.
//   GATE-EVID-05    every evidence reference in the sign-off input resolves to a directory that
//                   exists.
import assert from "node:assert/strict";
import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const repositoryRoot = path.resolve(scriptDirectory, "../../..");

const SIGNOFF_REQUIRED = "LEGAL_SIGNOFF_REQUIRED";
const DF03_RECORD = "specs/decisions/DF-03-signoff.md";

/**
 * The class vocabulary the retention job executes.
 *
 * <p>Read from the C# source, not copied. The first version of this file kept a hand-written list,
 * and W-0059 recorded that as a real gap: a class added on both the C# side and the governance map
 * while nobody updated this array would have gone unnoticed here, which is the one place that
 * checks the documents. Parsing the constants closes it — a new class fails the document checks
 * below until somebody writes it down.</p>
 *
 * <p>The three classes with no C# constant are appended explicitly, because they are not executed
 * by the retention job at all: PRESERVE decisions and one inherited dependency. Listing them here
 * with a reason is the difference between "not executed" and "forgotten".</p>
 */
async function retentionDataClasses() {
  const source = await read("src/Ivr.Domain/Retention/RetentionDataClasses.cs");
  const executed = [...source.matchAll(/public const string \w+ = "([a-z_]+)";/g)]
    .map((match) => match[1]);
  assert(
    executed.length > 0,
    "no retention classes parsed from RetentionDataClasses.cs; the parser has drifted.");

  // Not executed by the job, and each for a stated reason (see ivr-data-inventory.md section 3).
  const notExecuted = ["audit_log", "active_config", "retention_control", "analytics_derived"];
  return [...executed, ...notExecuted];
}

const LEGACY_DATA_CLASSES = [
  "task_metadata",
  "speech_snapshot",
  "attempt_metadata",
  "result_metadata",
  "callback_metadata",
  "raw_call_event",
  "evidence_link",
  "idempotency_key",
  "review_item",
  "audit_log",
  "active_config",
  "retention_control",
  "analytics_derived",
];

async function read(relativePath) {
  return fs.readFile(path.join(repositoryRoot, relativePath), "utf8");
}

async function exists(relativePath) {
  try {
    await fs.access(path.join(repositoryRoot, relativePath));
    return true;
  } catch {
    return false;
  }
}

/** Markdown table rows, split on the pipe, trimmed. Header and separator dropped. */
function tableRows(markdown) {
  return markdown
    .split("\n")
    .filter((line) => line.trimStart().startsWith("|"))
    .map((line) => line.split("|").slice(1, -1).map((cell) => cell.trim()))
    .filter((cells) => cells.length > 1 && !cells.every((cell) => /^-+$/.test(cell)));
}

// ---------------------------------------------------------------- LEGAL-RET-01

async function everyClassHasOwnerValueAndMechanism() {
  const dataClasses = await retentionDataClasses();

  // The hand-written list stays, as a tripwire rather than as the source. If parsing the C# ever
  // silently returns something different, this says so instead of quietly checking a shorter list.
  assert.deepEqual(
    [...dataClasses].sort(),
    [...LEGACY_DATA_CLASSES].sort(),
    "the classes parsed from RetentionDataClasses.cs no longer match the reviewed list; a class "
    + "was added or removed and the compliance pack has not been updated.");

  const inventory = await read("docs/compliance/ivr-data-inventory.md");
  const options = await read("docs/compliance/ivr-retention-options.md");
  const policy = await read("specs/decisions/DF-07-retention-policy.md");

  const policyRows = tableRows(policy);
  const missing = [];

  for (const dataClass of dataClasses) {
    // Present in all three, so a class cannot be dropped from the decision while surviving in the
    // inventory that feeds it.
    for (const [name, document] of [["inventory", inventory], ["options", options]]) {
      if (!document.includes(`\`${dataClass}\``)) {
        missing.push(`${dataClass} is absent from the ${name}.`);
      }
    }

    const row = policyRows.find((cells) => cells[0].includes(`\`${dataClass}\``));
    if (!row) {
      missing.push(`${dataClass} has no row in the DF-07 decision record.`);
      continue;
    }

    const [, owner, mechanism, period] = row;
    if (!owner || owner === "—") {
      missing.push(`${dataClass} has no retention owner.`);
    }
    if (!mechanism || mechanism === "—") {
      missing.push(`${dataClass} has no purge mechanism.`);
    }

    // A period is either a signed value or an explicit admission that nobody signed it. What it
    // must never be is a number that arrived without a signature.
    const signed = period.includes(SIGNOFF_REQUIRED)
      || period.includes("vĩnh viễn")
      || period.includes("= chu kỳ nguồn");
    if (!signed) {
      missing.push(
        `${dataClass} carries the period "${period}" with no signature and no `
        + `${SIGNOFF_REQUIRED} marker.`);
    }
  }

  assert.deepEqual(missing, [], `retention pack is incomplete:\n  ${missing.join("\n  ")}`);

  assert(
    policy.includes(`Trạng thái: **\`${SIGNOFF_REQUIRED}\`**`),
    "the DF-07 record does not declare itself unsigned, so a reader could take it as policy.");

  process.stdout.write(
    `LEGAL-RET-01 PASS — ${dataClasses.length} data classes (parsed from C#), each with an owner, a purge `
    + "mechanism and either a signature or an explicit admission there is none\n");
}

// ---------------------------------------------------------------- LEGAL-PII-02

async function pIIConstraintsMatchTheDecisions() {
  const recording = await read("specs/decisions/DT-05-recording-off-policy.md");
  const legalPack = await read("docs/compliance/ivr-pdpa-legal-basis-pack.md");
  const inventory = await read("docs/compliance/data-inventory.md");

  assert(
    recording.includes(`Trạng thái: **\`${SIGNOFF_REQUIRED}\`**`),
    "the DT-05 record does not declare itself unsigned.");

  // Four reopen conditions, not three. A recording decision that lists "get consent" and stops has
  // skipped the part where somebody has to be told they are being recorded.
  for (const condition of ["Cơ sở pháp lý riêng", "lưu đồng ý", "Thông báo", "Chu kỳ lưu riêng"]) {
    assert(
      recording.includes(condition),
      `the DT-05 reopen conditions do not mention "${condition}".`);
  }

  // D-05: the token resolves to a number only at the SIM boundary, and IVR never stores one.
  assert(
    legalPack.includes("Dial token chỉ giải ra số ở ranh giới SIM"),
    "the legal pack does not state the D-05 token boundary.");
  assert(
    inventory.includes("IVR **không bao giờ** thấy số"),
    "the field inventory does not state that IVR never sees the number.");

  // Nothing anywhere may claim recording is on. Checked across the whole pack rather than in the
  // one document about it, because the claim that matters is the one a reader finds first.
  const pack = [
    "docs/compliance/pia.md",
    "docs/compliance/data-inventory.md",
    "docs/compliance/ivr-data-inventory.md",
    "docs/compliance/ivr-pdpa-legal-basis-pack.md",
    "docs/release/df03-signoff-input.md",
  ];
  for (const document of pack) {
    const text = await read(document);
    assert(
      !/recording\s*(=|:)?\s*ON|ghi âm\s*:?\s*BẬT/i.test(text),
      `${document} claims recording is on.`);
  }

  process.stdout.write(
    "LEGAL-PII-02 PASS — recording OFF with four reopen conditions, D-05 token boundary stated, "
    + `and no document in the pack claims otherwise (${pack.length} checked)\n`);
}

// --------------------------------------------------------------- LEGAL-DSAR-03

async function dsarDocumentsWhatItCannotDo() {
  const runbook = await read("docs/compliance/dsar-runbook.md");
  const legalPack = await read("docs/compliance/ivr-pdpa-legal-basis-pack.md");

  // The three limits, each in both documents. A runbook that promises erasure the system cannot
  // perform sets up a broken promise to a data subject, which is worse than a narrower promise.
  const limits = [
    ["audit", /append-only/i],
    ["order_code", /order_code/],
    ["callback payload", /payload/i],
  ];
  for (const [name, pattern] of limits) {
    assert(pattern.test(runbook), `the DSAR runbook does not document the ${name} limit.`);
    assert(pattern.test(legalPack), `the legal pack does not document the ${name} limit.`);
  }

  // And the fourth, which lives outside the database and is the one most likely to be forgotten.
  assert(
    /backup/i.test(runbook),
    "the DSAR runbook does not mention that backups are outside the reach of an erasure.");

  // Lawful handling, not silent refusal: the limits are stated BEFORE a request is processed.
  assert(
    runbook.includes("**trước** khi bắt đầu") || runbook.includes("trước** khi"),
    "the DSAR runbook does not require stating the limits before processing a request.");

  process.stdout.write(
    "LEGAL-DSAR-03 PASS — three in-database limits plus the backup limit, documented in both the "
    + "runbook and the legal pack, and stated before a request is processed\n");
}

// ------------------------------------------------------------- SIGNOFF-DF03-04

async function theSignoffRecordCannotExistUnapproved() {
  const present = await exists(DF03_RECORD);

  if (!present) {
    // The expected state today, and the check still has to say what it would demand, so that the
    // day the file appears the requirement is already written down rather than invented then.
    process.stdout.write(
      `SIGNOFF-DF03-04 PASS — ${DF03_RECORD} does not exist. It may only be created with owner, `
      + "security and privacy approval fields filled; this check fails on a record without them.\n");
    return;
  }

  const record = await read(DF03_RECORD);
  const rows = tableRows(record);
  const approvals = ["Chủ sở hữu", "Security", "Privacy"];

  for (const role of approvals) {
    const row = rows.find((cells) => cells[0].includes(role));
    assert(row, `${DF03_RECORD} has no approval row for ${role}.`);
    const name = row[1] ?? "";
    assert(
      name.length > 0 && !name.includes("(trống)") && !name.includes("_"),
      `${DF03_RECORD} exists with an empty ${role} approval. A sign-off record without a signer `
      + "is a sign-off nobody gave.");
  }

  // The residual limitations have to travel with the signature. A signer who was not shown them
  // did not agree to them.
  assert(
    /giới hạn tồn dư|residual/i.test(record),
    `${DF03_RECORD} does not carry the residual limitations.`);

  process.stdout.write("SIGNOFF-DF03-04 PASS — the DF-03 record carries all three signatures\n");
}

// ---------------------------------------------------------------- GATE-EVID-05

async function everyEvidenceReferenceResolves() {
  const input = await read("docs/release/df03-signoff-input.md");
  const referenced = [...input.matchAll(/docs\/evidence\/(W-\d{4})/g)]
    .map((match) => match[1]);

  assert(referenced.length > 0, "the sign-off input references no evidence at all.");

  const missing = [];
  for (const work of new Set(referenced)) {
    if (!await exists(`docs/evidence/${work}/README.md`)) {
      missing.push(work);
    }
  }
  assert.deepEqual(
    missing,
    [],
    `the sign-off input references evidence that does not exist: ${missing.join(", ")}`);

  // The phase whose evidence is absent must be visible as absent. An accepted-evidence list that
  // silently omits the phase with no evidence reads as complete coverage.
  assert(
    /P8 lab \| `W-0048`, `W-0049` \| — \|/.test(input),
    "the sign-off input does not show the P8 row as empty; a missing phase must be visible.");

  // MASTER-05, stated where the list is, not somewhere else.
  assert(
    input.includes("evidence đã nộp không phải evidence đã được chấp nhận"),
    "the sign-off input does not distinguish submitted evidence from accepted evidence.");

  process.stdout.write(
    `GATE-EVID-05 PASS — ${new Set(referenced).size} evidence references all resolve, the empty `
    + "P8 row is visible, and submitted is distinguished from accepted\n");
}

// ------------------------------------------------------------------------ main

await everyClassHasOwnerValueAndMechanism();
await pIIConstraintsMatchTheDecisions();
await dsarDocumentsWhatItCannotDo();
await theSignoffRecordCannotExistUnapproved();
await everyEvidenceReferenceResolves();

process.stdout.write(
  "COMPLIANCE_PACK_SELFTEST_PASS — the pack is structurally complete and declares itself unsigned. "
  + "Structural completeness is not approval: DF-07, DT-05 and DF-03 all remain "
  + `${SIGNOFF_REQUIRED}, and P9-1 must read that as no-go.\n`);
