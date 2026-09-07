#!/usr/bin/env node

// W-0204 / P2.1. Answers one question mechanically: is Target Contract V1 in a state where a
// signature would mean something?
//
// A signature is worth exactly as much as the identity of the thing signed. Today the contract is
// pinned in `contract-manifest.json` and guarded by `openapi-contract-drift.mjs`, which is the
// hard part and already done. What that gate does NOT cover is everything else the plan's P2.1
// asks to be frozen alongside the spec:
//
//   * the GENERATED CLIENT, which is what Module 3 actually compiles against. It was listed in the
//     manifest by path and never by hash, so the bytes M3 builds from could change without any
//     gate noticing.
//   * the SAME PIN WRITTEN TWICE. The closure pack that owners are asked to sign restated the
//     contract hash as prose, and prose does not get updated when a spec is rotated - so the pack
//     named a baseline that no longer exists. An owner signing it would have signed an
//     unidentifiable artifact.
//   * the PUBLISHED FIELD LIST. Owners approve `06-module-3-api-handover.md`, not the YAML. If the
//     table in that document and the `required` array in the spec drift apart, the approval is for
//     a contract nobody implements.
//   * the DRAFT STATE ITSELF. `TARGET_CONTRACT_V1=DRAFT` lived only in a JSON string and a ledger
//     row; nothing read it, so nothing stopped it being edited to say signed.
//
// This verifier never sends anything, never records an approval and never promotes a gate. It only
// refuses to let the contract look frozen when it is not.
//
// Usage:
//   node scripts/contract-freeze-verifier.mjs            # verify (CI)
//   node scripts/contract-freeze-verifier.mjs --write    # refresh derived pins and the inventory
//   node scripts/contract-freeze-verifier.mjs --selftest # prove each check can fail

import { createHash } from "node:crypto";
import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import YAML from "yaml";

import {
  CASE_RULES as SHARED_E2E_CASE_RULES,
  SOURCE_PINS as SHARED_E2E_SOURCE_PINS,
} from "./target-v1-shared-e2e-report-validator.mjs";

const SCRIPT_DIRECTORY = path.dirname(fileURLToPath(import.meta.url));
const REPOSITORY_ROOT = path.resolve(SCRIPT_DIRECTORY, "../../..");
const MANIFEST_PATH = "specs/api/openapi/contract-manifest.json";
const INVENTORY_PATH = "docs/contracts/target-v1-field-inventory.md";
const HANDOVER_PATH = "integration-requirements/06-module-3-api-handover.md";

const DRAFT_STATE = "TARGET_CONTRACT_V1=DRAFT";

/** Statuses a contract entry may carry while the contract as a whole is still a draft. */
const DRAFT_CONTRACT_STATUSES = new Set([
  "TARGET_DRAFT",
  "CURRENT_COMPAT_VERIFIED_AT_PINNED_SHA",
]);

/**
 * Documents that talk about the pinned baseline. Every 64-hex literal inside one of these has to
 * be a hash the manifest currently pins; anything else is a second copy of a pin that will go
 * stale, which is precisely how the closure pack came to name a baseline that no longer existed.
 *
 * The list is deliberately short. It is not "every document" - evidence packs legitimately carry
 * hashes of their own artifacts - it is the documents an owner reads when deciding what to sign.
 */
const PIN_GOVERNED_DOCUMENTS = [
  "docs/contracts/target-v1-closure-pack/README.md",
  "docs/contracts/openapi-contract-diff.md",
  "docs/api-versioning.md",
];

const HEX64 = /\b[0-9a-f]{64}\b/g;

class FreezeFailure extends Error {}

const failures = [];

function fail(check, message) {
  failures.push(`${check}: ${message}`);
}

async function readText(root, relativePath) {
  return fs.readFile(path.join(root, relativePath), "utf8");
}

async function sha256Of(root, relativePath) {
  const bytes = await fs.readFile(path.join(root, relativePath));
  return createHash("sha256").update(bytes).digest("hex");
}

function setDifference(left, right) {
  return [...left].filter((item) => !right.has(item));
}

// ---------------------------------------------------------------------------------------------
// FREEZE-01 — everything a signature would cover is pinned, and matches
// ---------------------------------------------------------------------------------------------

async function checkPins(root, manifest, { write }) {
  for (const contract of manifest.contracts) {
    const actual = await sha256Of(root, contract.path);
    if (contract.sha256 !== actual) {
      fail(
        "FREEZE-01",
        `${contract.path} hashes ${actual} but the manifest pins ${contract.sha256}. ` +
          "openapi:drift owns this pin; do not repin here.",
      );
    }

    if (!contract.generated) {
      continue;
    }

    // The generated client is what a consumer compiles. Pinning the spec and not the artifact
    // generated from it leaves the only bytes M3 actually links against unguarded.
    const generatedHash = await sha256Of(root, contract.generated);
    if (write) {
      contract.generatedSha256 = generatedHash;
      continue;
    }

    if (!contract.generatedSha256) {
      fail(
        "FREEZE-01",
        `${contract.id} declares generated artifact ${contract.generated} with no ` +
          "generatedSha256. Run with --write after reviewing the regenerated client.",
      );
    } else if (contract.generatedSha256 !== generatedHash) {
      fail(
        "FREEZE-01",
        `${contract.generated} hashes ${generatedHash} but the manifest pins ` +
          `${contract.generatedSha256}. Regenerate the client from the pinned spec, or repin ` +
          "deliberately with --write.",
      );
    }
  }
}

// ---------------------------------------------------------------------------------------------
// FREEZE-02 — one pin, written once
// ---------------------------------------------------------------------------------------------

async function checkNoDuplicatePins(root, manifest) {
  const known = new Set();
  for (const contract of manifest.contracts) {
    known.add(contract.sha256);
    if (contract.generatedSha256) {
      known.add(contract.generatedSha256);
    }
  }

  for (const documentPath of PIN_GOVERNED_DOCUMENTS) {
    let text;
    try {
      text = await readText(root, documentPath);
    } catch {
      fail("FREEZE-02", `${documentPath} is missing; the pin-governed set is out of date.`);
      continue;
    }

    for (const literal of text.match(HEX64) ?? []) {
      if (!known.has(literal)) {
        fail(
          "FREEZE-02",
          `${documentPath} states sha256 ${literal}, which the manifest does not pin. ` +
            "A restated pin goes stale the first time the contract is rotated; reference " +
            `${MANIFEST_PATH} instead of copying the number.`,
        );
      }
    }
  }
}

// ---------------------------------------------------------------------------------------------
// FREEZE-03 — the published field list is the contract
// ---------------------------------------------------------------------------------------------

/**
 * Reads the field names out of a markdown table under a heading, taking the first column of every
 * row that looks like `| \`field_name\` | ... |`. Deliberately literal: the point is to read what
 * the owner reads, not to re-derive it from the spec that is being checked against.
 */
function fieldsUnderHeading(markdown, heading) {
  const start = markdown.indexOf(heading);
  if (start < 0) {
    throw new FreezeFailure(`heading not found: ${heading}`);
  }

  const rest = markdown.slice(start + heading.length);
  const nextHeading = rest.search(/\n#{2,4} /);
  const section = nextHeading < 0 ? rest : rest.slice(0, nextHeading);
  const rows = [];
  for (const line of section.split("\n")) {
    const match = /^\|\s*`([A-Za-z0-9_]+)`\s*\|(.*)\|/.exec(line.trim());
    if (match) {
      rows.push({ field: match[1], rest: match[2] });
    }
  }

  if (rows.length === 0) {
    throw new FreezeFailure(`no field rows under: ${heading}`);
  }

  return rows;
}

async function checkPublishedSurface(root, specs) {
  let handover;
  try {
    handover = await readText(root, HANDOVER_PATH);
  } catch {
    fail("FREEZE-03", `${HANDOVER_PATH} is missing.`);
    return;
  }

  // Intake: the handover publishes one table of required fields.
  try {
    const published = new Set(
      fieldsUnderHeading(handover, "### 3.4. 22 field bắt buộc trên wire").map(
        (row) => row.field,
      ),
    );
    const required = new Set(specs.intake.components.schemas.IvrConfirmationTaskV1.required);
    const missing = setDifference(required, published);
    const extra = setDifference(published, required);
    if (missing.length > 0 || extra.length > 0) {
      fail(
        "FREEZE-03",
        `intake required fields differ between the spec and ${HANDOVER_PATH} §3.4 — ` +
          `spec-only [${missing.join(", ")}], document-only [${extra.join(", ")}].`,
      );
    }
  } catch (error) {
    fail("FREEZE-03", `intake field table unreadable: ${error.message}`);
  }

  // Callback: one table carrying both required and optional rows, distinguished by the second
  // column. Reading the requiredness from the same row the owner reads is the whole point.
  try {
    const rows = fieldsUnderHeading(
      handover,
      "### 4.2. Body IVR gửi: 13 field bắt buộc + 1 optional",
    );
    const publishedRequired = new Set(
      rows.filter((row) => row.rest.includes("Bắt buộc")).map((row) => row.field),
    );
    const publishedOptional = new Set(
      rows.filter((row) => row.rest.includes("Optional")).map((row) => row.field),
    );
    const schema = specs.callback.components.schemas.IvrResultCallbackV1;
    const required = new Set(schema.required);
    const optional = new Set(
      Object.keys(schema.properties).filter((name) => !required.has(name)),
    );

    const missingRequired = setDifference(required, publishedRequired);
    const extraRequired = setDifference(publishedRequired, required);
    if (missingRequired.length > 0 || extraRequired.length > 0) {
      fail(
        "FREEZE-03",
        `callback required fields differ between the spec and ${HANDOVER_PATH} §4.2 — ` +
          `spec-only [${missingRequired.join(", ")}], document-only [${extraRequired.join(", ")}].`,
      );
    }

    const missingOptional = setDifference(optional, publishedOptional);
    const extraOptional = setDifference(publishedOptional, optional);
    if (missingOptional.length > 0 || extraOptional.length > 0) {
      fail(
        "FREEZE-03",
        `callback optional fields differ between the spec and ${HANDOVER_PATH} §4.2 — ` +
          `spec-only [${missingOptional.join(", ")}], document-only [${extraOptional.join(", ")}].`,
      );
    }
  } catch (error) {
    fail("FREEZE-03", `callback field table unreadable: ${error.message}`);
  }
}

// ---------------------------------------------------------------------------------------------
// FREEZE-04 — the state string has to be earned
// ---------------------------------------------------------------------------------------------

function checkDraftHonesty(manifest, specs) {
  const state = manifest.contractState;
  if (state === DRAFT_STATE) {
    for (const contract of manifest.contracts) {
      if (!DRAFT_CONTRACT_STATUSES.has(contract.status)) {
        fail(
          "FREEZE-04",
          `contractState is ${DRAFT_STATE} but ${contract.id} claims status ` +
            `${contract.status}. One entry cannot be approved while the contract is not.`,
        );
      }
    }

    // W-0200 established that the version string and the lifecycle state answer two different
    // questions, and that dropping the pre-release suffix reads as an approval claim. While the
    // contract is DRAFT the IVR-owned specs must keep a suffix, so no consumer can mistake a
    // version number for a signature.
    for (const [name, spec] of Object.entries(specs)) {
      const version = spec.info?.version ?? "";
      if (!version.includes("-")) {
        fail(
          "FREEZE-04",
          `${name} spec is version ${version} with no pre-release suffix while the contract ` +
            "state is DRAFT. A bare SemVer version reads as an approved contract.",
        );
      }
    }
    return;
  }

  // Anything other than DRAFT is a claim that signatures exist. Refuse it unless the manifest
  // names where they are, and that path exists.
  if (!manifest.closureEvidence) {
    fail(
      "FREEZE-04",
      `contractState is ${state} but the manifest names no closureEvidence. A contract state ` +
        "cannot be promoted by editing a string.",
    );
  }
}

// ---------------------------------------------------------------------------------------------
// FREEZE-05 — one page an owner can actually sign
// ---------------------------------------------------------------------------------------------

function enumInventory(spec) {
  const rows = [];
  const walk = (node, trail) => {
    if (!node || typeof node !== "object") {
      return;
    }

    if (Array.isArray(node.enum)) {
      rows.push({ location: trail, values: [...node.enum] });
    }

    for (const [key, value] of Object.entries(node)) {
      if (key === "enum") {
        continue;
      }

      walk(value, trail ? `${trail}.${key}` : key);
    }
  };
  walk(spec.components?.schemas ?? {}, "");
  return rows.sort((left, right) => left.location.localeCompare(right.location));
}

function renderInventory(manifest, specs) {
  const intake = specs.intake.components.schemas.IvrConfirmationTaskV1;
  const callback = specs.callback.components.schemas.IvrResultCallbackV1;
  const intakeOptional = Object.keys(intake.properties).filter(
    (name) => !intake.required.includes(name),
  );
  const callbackOptional = Object.keys(callback.properties).filter(
    (name) => !callback.required.includes(name),
  );

  const lines = [
    "# Target V1 — Field and Enum Inventory",
    "",
    "> Generated by `deploy/ci/scripts/contract-freeze-verifier.mjs`. Do not edit by hand;",
    "> `FREEZE-05` fails when this file and the pinned specs disagree.",
    "",
    "This is the single page to review when approving the wire surface. It is derived from the",
    "pinned specs, so it cannot describe a contract other than the one the gates enforce - which",
    "is the whole reason it is generated rather than written.",
    "",
    `Contract state: \`${manifest.contractState}\`.`,
    "",
    "| Contract | Version | Pinned sha256 |",
    "| --- | --- | --- |",
    `| IVR intake (\`${manifest.contracts[0].path}\`) | \`${specs.intake.info.version}\` | \`${manifest.contracts[0].sha256}\` |`,
    `| Sales callback (\`${manifest.contracts[1].path}\`) | \`${specs.callback.info.version}\` | \`${manifest.contracts[1].sha256}\` |`,
    "",
    "## 1. Intake — `IvrConfirmationTaskV1`",
    "",
    `Required: **${intake.required.length}**. Optional: **${intakeOptional.length}**. ` +
      `\`additionalProperties: ${intake.additionalProperties}\`.`,
    "",
    "| # | Required field |",
    "| ---: | --- |",
    ...intake.required.map((name, index) => `| ${index + 1} | \`${name}\` |`),
    "",
    "| Optional field |",
    "| --- |",
    ...intakeOptional.map((name) => `| \`${name}\` |`),
    "",
    "## 2. Callback — `IvrResultCallbackV1`",
    "",
    `Required: **${callback.required.length}**. Optional: **${callbackOptional.length}**. ` +
      `\`additionalProperties: ${callback.additionalProperties}\`.`,
    "",
    "| # | Required field |",
    "| ---: | --- |",
    ...callback.required.map((name, index) => `| ${index + 1} | \`${name}\` |`),
    "",
    "| Optional field |",
    "| --- |",
    ...callbackOptional.map((name) => `| \`${name}\` |`),
    "",
    "## 3. Enumerations",
    "",
    "Adding a value is treated as breaking until every consumer proves unknown-value tolerance;",
    "see `docs/api-versioning.md`. Both directions of this list therefore need approval.",
    "",
  ];

  // Split rather than filtered. A one-value enum is still a contract constraint a consumer has to
  // satisfy - `recordingEnabled: false` is a promise, not decoration - so dropping those would
  // hide signable terms. But listing them beside the real value sets buries the ones a reviewer
  // has to think about, so they get their own table instead.
  let section = 0;
  for (const [label, spec] of [
    ["Intake contract", specs.intake],
    ["Callback contract", specs.callback],
  ]) {
    section += 1;
    const rows = enumInventory(spec);
    const valueSets = rows.filter((row) => row.values.length > 1);
    const constants = rows.filter((row) => row.values.length === 1);

    lines.push(`### 3.${section}. ${label}`, "");
    lines.push(`Value sets: **${valueSets.length}**. Pinned constants: **${constants.length}**.`, "");
    lines.push("| Location | Values |", "| --- | --- |");
    for (const row of valueSets) {
      lines.push(`| \`${row.location}\` | ${row.values.map((v) => `\`${v}\``).join(", ")} |`);
    }

    lines.push("", "<details><summary>Pinned constants</summary>", "");
    lines.push("| Location | Value |", "| --- | --- |");
    for (const row of constants) {
      lines.push(`| \`${row.location}\` | \`${row.values[0]}\` |`);
    }

    lines.push("", "</details>", "");
  }

  return `${lines.join("\n")}\n`;
}

async function checkInventory(root, manifest, specs, { write }) {
  const rendered = renderInventory(manifest, specs);
  const absolute = path.join(root, INVENTORY_PATH);
  if (write) {
    await fs.mkdir(path.dirname(absolute), { recursive: true });
    await fs.writeFile(absolute, rendered, "utf8");
    return;
  }

  let committed;
  try {
    committed = await fs.readFile(absolute, "utf8");
  } catch {
    fail("FREEZE-05", `${INVENTORY_PATH} is missing. Run with --write.`);
    return;
  }

  if (committed !== rendered) {
    fail(
      "FREEZE-05",
      `${INVENTORY_PATH} disagrees with the pinned specs. Regenerate with --write after ` +
        "reviewing the contract change.",
    );
  }
}

// ---------------------------------------------------------------------------------------------
// FREEZE-06 — what we ask Module 3 to prove must be something the contract permits
// ---------------------------------------------------------------------------------------------

/**
 * Which ACK codes a shared-E2E case outcome actually asserts. Outcomes that describe a transport
 * condition rather than an ACK body (`AUTH_REJECTED`, `RETRY_PENDING`, …) constrain nothing here:
 * 401, 429, 502 and a dropped connection are answers the contract never claimed to enumerate.
 */
const CASE_OUTCOME_ACK_CODES = new Map([
  ["ACCEPTED", ["ACCEPTED"]],
  ["DUPLICATE_ACCEPTED", ["DUPLICATE_ACCEPTED"]],
  ["IDEMPOTENCY_CONFLICT", ["IDEMPOTENCY_CONFLICT"]],
  ["REJECTED_STALE", ["REJECTED_STALE"]],
  ["BLOCKED_BY_CORE_OR_REVIEW_REQUIRED", ["BLOCKED_BY_CORE", "REVIEW_REQUIRED"]],
]);

/** status -> ACK codes the pinned callback contract permits at that status. */
function ackCodesByStatus(callbackSpec) {
  const operation = Object.values(callbackSpec.paths)[0]?.post;
  const byStatus = new Map();
  for (const [status, response] of Object.entries(operation?.responses ?? {})) {
    const ref = response.content?.["application/json"]?.schema?.$ref;
    if (!ref) continue;
    const schema = callbackSpec.components.schemas[ref.split("/").pop()];
    const codes = schema?.properties?.code?.enum;
    if (Array.isArray(codes)) byStatus.set(Number(status), new Set(codes));
  }

  return byStatus;
}

/**
 * The shared-E2E case sheet is the list of things Module 3 will be asked to demonstrate. It is
 * only worth asking for things the frozen contract allows - and until W-0207 nothing checked
 * that, so the sheet asked for `DUPLICATE_ACCEPTED` on HTTP 409. The contract binds that code to
 * 200 only, and the transport turns an ACK on the wrong status into a terminal dead letter
 * (`UT-CALLBACK-TARGET-ACK-CROSS-01`), so building to the sheet would have quietly dead-lettered
 * every exact replay.
 */
function checkSharedE2ECaseSheet(manifest, specs, caseRules, sourcePins) {
  const permitted = ackCodesByStatus(specs.callback);

  // The sheet pins its own copy of the callback contract hash. FREEZE-02 does not scan code, so
  // this is the one restated pin that could otherwise drift unnoticed.
  const callbackContract = manifest.contracts.find(
    (contract) => contract.path.endsWith("order-core-ivr-callback.target-v1.yaml"),
  );
  if (sourcePins?.m8_target_oas_sha256 !== callbackContract?.sha256) {
    fail(
      "FREEZE-06",
      `the shared-E2E validator pins callback OAS ${sourcePins?.m8_target_oas_sha256}, the ` +
        `manifest pins ${callbackContract?.sha256}. The sheet would be validated against a ` +
        "different contract than the one that is frozen.",
    );
  }

  const named = new Set();
  for (const rule of caseRules) {
    const codes = CASE_OUTCOME_ACK_CODES.get(rule.outcome) ?? [];
    for (const code of codes) named.add(code);
    if (codes.length === 0) continue;

    for (const status of rule.http) {
      if (status === null) continue;
      const allowed = permitted.get(status);
      if (!allowed) {
        fail(
          "FREEZE-06",
          `${rule.case_id} expects ACK ${codes.join("/")} on HTTP ${status}, but the pinned ` +
            "contract binds no ACK schema to that status.",
        );
        continue;
      }

      const forbidden = codes.filter((code) => !allowed.has(code));
      if (forbidden.length > 0) {
        fail(
          "FREEZE-06",
          `${rule.case_id} expects ACK ${forbidden.join("/")} on HTTP ${status}; the pinned ` +
            `contract permits only ${[...allowed].join("/")} there. An ACK on the wrong status ` +
            "is a terminal dead letter, not a near miss.",
        );
      }
    }
  }

  // The other direction: an ACK code the contract defines but nobody is asked to demonstrate is a
  // hole in the acceptance matrix, and holes in acceptance matrices are found in production.
  for (const codes of permitted.values()) {
    for (const code of codes) {
      if (!named.has(code)) {
        fail(
          "FREEZE-06",
          `the contract defines ACK ${code} but no shared-E2E case asks for it.`,
        );
      }
    }
  }
}

// ---------------------------------------------------------------------------------------------
// Driver
// ---------------------------------------------------------------------------------------------

async function loadSpecs(root, manifest) {
  return {
    intake: YAML.parse(await readText(root, manifest.contracts[0].path)),
    callback: YAML.parse(await readText(root, manifest.contracts[1].path)),
  };
}

async function run({
  write = false,
  root = REPOSITORY_ROOT,
  // Injectable so the selftest can mutate the sheet without editing the shipped validator. The
  // defaults are the real ones, so CI checks exactly what production ships.
  caseRules = SHARED_E2E_CASE_RULES,
  sourcePins = SHARED_E2E_SOURCE_PINS,
} = {}) {
  failures.length = 0;
  const manifest = JSON.parse(await readText(root, MANIFEST_PATH));
  const specs = await loadSpecs(root, manifest);

  await checkPins(root, manifest, { write });
  await checkNoDuplicatePins(root, manifest);
  await checkPublishedSurface(root, specs);
  checkDraftHonesty(manifest, specs);
  checkSharedE2ECaseSheet(manifest, specs, caseRules, sourcePins);
  await checkInventory(root, manifest, specs, { write });

  if (write) {
    await fs.writeFile(
      path.join(root, MANIFEST_PATH),
      `${JSON.stringify(manifest, null, 2)}\n`,
      "utf8",
    );
  }

  return { manifest, specs, failures: [...failures] };
}

function report(result) {
  if (result.failures.length > 0) {
    process.stdout.write("CONTRACT_FREEZE=FAIL\n");
    for (const item of result.failures) {
      process.stdout.write(`  - ${item}\n`);
    }

    return 1;
  }

  process.stdout.write("CONTRACT_FREEZE=PASS\n");
  process.stdout.write(`  contract_state=${result.manifest.contractState}\n`);
  process.stdout.write(
    `  intake=${result.specs.intake.info.version} ` +
      `required=${result.specs.intake.components.schemas.IvrConfirmationTaskV1.required.length}\n`,
  );
  process.stdout.write(
    `  callback=${result.specs.callback.info.version} ` +
      `required=${result.specs.callback.components.schemas.IvrResultCallbackV1.required.length}\n`,
  );
  process.stdout.write(`  pinned_artifacts=${result.manifest.contracts.length}\n`);
  return 0;
}

const invokedDirectly =
  process.argv[1] && path.resolve(process.argv[1]) === path.resolve(fileURLToPath(import.meta.url));
if (invokedDirectly) {
  process.exit(report(await run({ write: process.argv.includes("--write") })));
}

export {
  run,
  renderInventory,
  fieldsUnderHeading,
  MANIFEST_PATH,
  INVENTORY_PATH,
  HANDOVER_PATH,
  PIN_GOVERNED_DOCUMENTS,
  REPOSITORY_ROOT,
};
