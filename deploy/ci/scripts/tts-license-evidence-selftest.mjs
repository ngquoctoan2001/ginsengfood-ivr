import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { cpSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { resolve, join } from "node:path";
import { verifyLicenseEvidence } from "./tts-license-evidence.mjs";

export function runLicenseEvidenceSelftest(repoRoot) {
  const sourceRoot = resolve(repoRoot, "deploy/tts/licenses");
  const lock = JSON.parse(readFileSync(resolve(repoRoot, "deploy/tts/models/MODELS.lock"), "utf8"));
  assert.deepEqual(verifyLicenseEvidence(lock, sourceRoot), { documents: 4, models: 2, artifacts: 13 });
  const cases = JSON.parse(readFileSync(resolve(repoRoot, "deploy/tts/tests/fixtures/license-evidence-cases.json"), "utf8"));
  const temp = mkdtempSync(join(tmpdir(), "ivr-license-evidence-"));
  try {
    for (const test of cases) {
      const root = join(temp, test.id);
      cpSync(sourceRoot, root, { recursive: true });
      const candidate = structuredClone(lock);
      const manifest = JSON.parse(readFileSync(join(root, "LICENSES.json"), "utf8"));
      if (test.file) {
        if (test.remove) rmSync(join(root, test.file));
        else writeFileSync(join(root, test.file), test.value);
      } else {
        let target = test.target === "lock" ? candidate : manifest;
        for (const key of test.path.slice(0, -1)) target = target[key];
        const last = test.path.at(-1);
        if (test.remove) delete target[last];
        else target[last] = test.value;
        if (test.target === "manifest") {
          const bytes = Buffer.from(`${JSON.stringify(manifest, null, 2)}\n`);
          writeFileSync(join(root, "LICENSES.json"), bytes);
          // Rebind deliberately: exercise semantic checks beyond the outer hash.
          candidate.license_evidence.sha256 = createHash("sha256").update(bytes).digest("hex");
        }
      }
      assert.throws(() => verifyLicenseEvidence(candidate, root), undefined, test.id);
    }
  } finally {
    rmSync(temp, { recursive: true });
  }
  process.stdout.write(`TTS_LICENSE_EVIDENCE_SELFTEST_PASS positive=1 rejected=${cases.length}\n`);
}
