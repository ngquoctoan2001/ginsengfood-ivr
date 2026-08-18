import fs from "node:fs/promises";
import process from "node:process";

// W-0038 / P5-4 §6.4, MASTER-05. Validates that a merge-request description actually carries the
// traceability it claims: a Work ID, a prompt ID, a filled mapping row, and every governance
// checkbox ticked.
//
// The template has existed since P0-2, but nothing checked whether it was FILLED IN. A template
// nobody verifies becomes a shape people delete the contents of and tick through — which reads
// as traceability while carrying none.
//
// Usage:
//   node check-mr-traceability.mjs --file <path>
//   node check-mr-traceability.mjs --text "<description>"
//   CI_MERGE_REQUEST_DESCRIPTION=... node check-mr-traceability.mjs

const args = process.argv.slice(2);

function argumentValue(name) {
  const index = args.indexOf(name);
  return index >= 0 && index + 1 < args.length ? args[index + 1] : null;
}

const filePath = argumentValue("--file");
const inlineText = argumentValue("--text");
const description = filePath !== null
  ? await fs.readFile(filePath, "utf8")
  : inlineText ?? process.env.CI_MERGE_REQUEST_DESCRIPTION ?? "";

const failures = [];

if (description.trim().length === 0) {
  failures.push("The merge request description is empty.");
}

// A real Work ID, not the placeholder the template ships with. Matched loosely first so the
// placeholder gets its own message: a gate that reports "missing" for something plainly present
// is a gate people stop believing.
const workIdToken = /Work ID:\s*`?(W-[A-Za-z0-9]{4})`?/u.exec(description);
const workId = workIdToken !== null && /^W-\d{4}$/u.test(workIdToken[1]) ? workIdToken : null;
if (workIdToken === null) {
  failures.push("No Work ID line of the form `Work ID: W-XXXX` is present.");
} else if (workId === null) {
  failures.push(
    `The Work ID is still the template placeholder (${workIdToken[1]}); use the real W-XXXX.`,
  );
}

const promptId = /Prompt ID:\s*`?(P\d+-\d+|N\/A)`?/u.exec(description);
if (promptId === null) {
  failures.push("No prompt ID of the form PX-Y (or N/A) is present.");
}

// At least one mapping row that is not the template's own example row.
const mappingRows = description
  .split(/\r?\n/u)
  .filter((line) => line.trim().startsWith("|"))
  .filter((line) => !/^\|\s*-+/u.test(line.trim()))
  .filter((line) => !line.includes("Source / decision"))
  .filter((line) => !line.includes("requirement/decision ID"));
if (mappingRows.length === 0) {
  failures.push("The traceability mapping table has no filled row.");
} else {
  const hasEvidence = mappingRows.some((row) => /docs\/evidence\/W-\d{4}/u.test(row));
  const hasResidual = mappingRows.some((row) =>
    /(NONE|NOT_RUN|BLOCKED_EXTERNAL|DEFERRED_TARGET)/u.test(row));
  if (!hasEvidence) {
    failures.push("No mapping row points at a docs/evidence/W-XXXX directory.");
  }

  if (!hasResidual) {
    failures.push(
      "No mapping row states a residual gate (NONE / NOT_RUN / BLOCKED_EXTERNAL / DEFERRED_TARGET).",
    );
  }
}

// Every checkbox must be ticked. An unticked governance box is the whole point of the gate.
const unticked = description
  .split(/\r?\n/u)
  .filter((line) => /^\s*-\s*\[\s*\]/u.test(line))
  .map((line) => line.trim());
if (unticked.length > 0) {
  failures.push(
    `${unticked.length} checklist item(s) are not ticked: ${unticked.slice(0, 3).join(" | ")}`,
  );
}

if (failures.length > 0) {
  process.stderr.write("MR_TRACEABILITY_FAIL\n");
  for (const failure of failures) {
    process.stderr.write(`  - ${failure}\n`);
  }

  process.exit(1);
}

process.stdout.write(`MR_TRACEABILITY_PASS work_id=${workId[1]}\n`);
