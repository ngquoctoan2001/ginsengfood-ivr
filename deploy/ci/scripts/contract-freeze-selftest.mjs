#!/usr/bin/env node

// W-0204 / P2.1. Proves every check in `contract-freeze-verifier.mjs` can actually fail.
//
// W-0126 recorded the failure mode this exists to avoid: a gate whose expectation is derived from
// the same place as the thing it checks cannot go red, so it passes for ever and is mistaken for
// evidence. Each case below breaks exactly one thing in a throwaway copy of the repository and
// requires the matching check id to appear. A case that fails to fail is itself a failure.
//
// Nothing here touches the working tree: every mutation happens under a temporary directory.

import { createHash } from "node:crypto";
import { cpSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { mkdir } from "node:fs/promises";
import { tmpdir } from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";

import {
  run,
  MANIFEST_PATH,
  INVENTORY_PATH,
  HANDOVER_PATH,
  PIN_GOVERNED_DOCUMENTS,
  REPOSITORY_ROOT,
} from "./contract-freeze-verifier.mjs";

/** Only the files the verifier reads, so a case cannot accidentally depend on anything else. */
const COPIED_PATHS = [
  MANIFEST_PATH,
  INVENTORY_PATH,
  HANDOVER_PATH,
  "specs/api/openapi/ivr-order-confirmation.v1.yaml",
  "specs/api/openapi/order-core-ivr-callback.target-v1.yaml",
  "specs/api/compat/current-golden-hour-callback.a3aad246.schema.json",
  "src/Ivr.Contracts/Generated/IvrServer/V1/IvrServerModels.g.cs",
  "src/Ivr.Contracts/Generated/SalesTarget/V1/SalesTargetV1Client.g.cs",
  ...PIN_GOVERNED_DOCUMENTS,
];

async function materialise() {
  const root = mkdtempSync(path.join(tmpdir(), "ivr-contract-freeze-"));
  for (const relativePath of COPIED_PATHS) {
    const destination = path.join(root, relativePath);
    await mkdir(path.dirname(destination), { recursive: true });
    cpSync(path.join(REPOSITORY_ROOT, relativePath), destination);
  }

  return root;
}

function editJson(root, relativePath, mutate) {
  const absolute = path.join(root, relativePath);
  const document = JSON.parse(readFileSync(absolute, "utf8"));
  mutate(document);
  writeFileSync(absolute, `${JSON.stringify(document, null, 2)}\n`, "utf8");
}

function editText(root, relativePath, mutate) {
  const absolute = path.join(root, relativePath);
  writeFileSync(absolute, mutate(readFileSync(absolute, "utf8")), "utf8");
}

function sha256Of(root, relativePath) {
  return createHash("sha256")
    .update(readFileSync(path.join(root, relativePath)))
    .digest("hex");
}

const CASES = [
  {
    id: "baseline",
    expect: null,
    mutate() {},
  },
  {
    id: "generated-client-changed",
    expect: "FREEZE-01",
    mutate(root) {
      editText(
        root,
        "src/Ivr.Contracts/Generated/IvrServer/V1/IvrServerModels.g.cs",
        (text) => `${text}\n// drift\n`,
      );
    },
  },
  {
    id: "generated-client-unpinned",
    expect: "FREEZE-01",
    mutate(root) {
      editJson(root, MANIFEST_PATH, (manifest) => {
        delete manifest.contracts[0].generatedSha256;
      });
    },
  },
  {
    id: "spec-changed-without-repin",
    expect: "FREEZE-01",
    mutate(root) {
      editText(
        root,
        "specs/api/openapi/ivr-order-confirmation.v1.yaml",
        (text) => `${text}\n# drift\n`,
      );
    },
  },
  {
    id: "stale-pin-restated-in-a-signable-document",
    expect: "FREEZE-02",
    mutate(root) {
      editText(
        root,
        "docs/contracts/target-v1-closure-pack/README.md",
        (text) =>
          `${text}\n\nIVR internal API: ` +
          "`sha256:b59a644e5bcaca3ad33b2b91523e14ec65196027b4a37a6b3c73d6842e8676b9`\n",
      );
    },
  },
  {
    id: "published-intake-field-list-drifts-from-the-spec",
    expect: "FREEZE-03",
    mutate(root) {
      editText(root, HANDOVER_PATH, (text) =>
        text.replace(
          "| `evidence_ref` | string | Con trỏ evidence để đối soát |",
          "| `evidence_reference` | string | Con trỏ evidence để đối soát |",
        ),
      );
    },
  },
  {
    id: "published-callback-requiredness-drifts-from-the-spec",
    expect: "FREEZE-03",
    mutate(root) {
      editText(root, HANDOVER_PATH, (text) =>
        text.replace(
          "| `audit_ref` | Bắt buộc | Audit reference |",
          "| `audit_ref` | Optional | Audit reference |",
        ),
      );
    },
  },
  {
    id: "one-contract-claims-approval-while-the-whole-is-draft",
    expect: "FREEZE-04",
    mutate(root) {
      editJson(root, MANIFEST_PATH, (manifest) => {
        manifest.contracts[0].status = "TARGET_APPROVED";
      });
    },
  },
  {
    id: "draft-suffix-dropped-while-still-draft",
    expect: "FREEZE-04",
    // The interesting case, and the fiddliest: dropping the pre-release suffix is a real
    // contract edit, so every derived pin moves with it. Rather than tolerate the resulting
    // noise, this case performs the whole legitimate rotation - repin the spec, carry the new
    // hash into the human diff report, regenerate the inventory - so the ONLY thing left to
    // complain about is the version string itself. That is exactly W-0200's point: a rotation
    // can be perfectly executed and still be a false approval claim.
    mutate(root) {
      const specPath = "specs/api/openapi/ivr-order-confirmation.v1.yaml";
      const before = sha256Of(root, specPath);
      editText(root, specPath, (text) =>
        text.replace("version: 1.0.0-draft.23", "version: 1.0.0"),
      );
      const after = sha256Of(root, specPath);
      editJson(root, MANIFEST_PATH, (manifest) => {
        manifest.contracts[0].sha256 = after;
      });
      editText(root, "docs/contracts/openapi-contract-diff.md", (text) =>
        text.split(before).join(after).split("1.0.0-draft.23").join("1.0.0"),
      );
    },
    rewriteBeforeAssert: true,
  },
  {
    id: "state-promoted-without-closure-evidence",
    expect: "FREEZE-04",
    mutate(root) {
      editJson(root, MANIFEST_PATH, (manifest) => {
        manifest.contractState = "TARGET_CONTRACT_V1=SIGNED";
      });
    },
    allowAlso: ["FREEZE-05"],
  },
  {
    id: "inventory-no-longer-describes-the-pinned-spec",
    expect: "FREEZE-05",
    mutate(root) {
      editText(root, INVENTORY_PATH, (text) =>
        text.replace("Required: **22**", "Required: **21**"),
      );
    },
  },
];

export async function runSelftest() {
  let passed = 0;
  const problems = [];

  for (const testCase of CASES) {
    const root = await materialise();
    try {
      testCase.mutate(root);
      if (testCase.rewriteBeforeAssert) {
        // Regenerate everything the tooling would legitimately regenerate, so the assertion is
        // about the check under test rather than about derived files trailing behind.
        await run({ root, write: true });
      }

      const { failures } = await run({ root });
      const codes = new Set(failures.map((item) => item.split(":")[0]));

      if (testCase.expect === null) {
        if (failures.length !== 0) {
          problems.push(`${testCase.id}: expected a clean copy to pass, got ${failures.join(" | ")}`);
          continue;
        }
      } else if (!codes.has(testCase.expect)) {
        problems.push(
          `${testCase.id}: expected ${testCase.expect}, got ` +
            `[${[...codes].join(", ") || "no failure at all"}]`,
        );
        continue;
      } else {
        // A mutation that trips half the gate proves nothing about the check it was aimed at.
        const allowed = new Set([testCase.expect, ...(testCase.allowAlso ?? [])]);
        const unexpected = [...codes].filter((code) => !allowed.has(code));
        if (unexpected.length > 0) {
          problems.push(`${testCase.id}: also tripped [${unexpected.join(", ")}]`);
          continue;
        }
      }

      passed += 1;
      process.stdout.write(`  ok   ${testCase.id}\n`);
    } finally {
      rmSync(root, { recursive: true, force: true });
    }
  }

  for (const problem of problems) {
    process.stdout.write(`  FAIL ${problem}\n`);
  }

  process.stdout.write(
    `CONTRACT_FREEZE_SELFTEST=${problems.length === 0 ? "PASS" : "FAIL"} ` +
      `${passed}/${CASES.length}\n`,
  );
  return problems.length === 0 ? 0 : 1;
}

const invokedDirectly =
  process.argv[1] && path.resolve(process.argv[1]) === path.resolve(fileURLToPath(import.meta.url));
if (invokedDirectly) {
  process.exit(await runSelftest());
}
