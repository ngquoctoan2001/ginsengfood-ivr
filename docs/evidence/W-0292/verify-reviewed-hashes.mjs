// Read-only reproduction of the 50 SHA-256 false positives reviewed in W-0291.
import fs from "node:fs";
import path from "node:path";
import crypto from "node:crypto";
import { execFileSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../../..");
const review = JSON.parse(fs.readFileSync(path.join(root, "docs/evidence/W-0291/reviewed-findings.json"), "utf8"));
const cache = new Map();
const readBlob = (commit, file) => {
  if (!/^[a-f0-9]{40}$/.test(commit)) throw new Error("Expected immutable commit SHA");
  const key = `${commit}:${file}`;
  if (!cache.has(key)) cache.set(key, execFileSync("git", ["show", key], { cwd: root, maxBuffer: 8 * 1024 * 1024 }));
  return cache.get(key);
};
let candidateHashes = 0;
let portalHashes = 0;
for (const finding of review.findings.filter(row => row.category.endsWith("source_sha256"))) {
  const commit = finding.commit.replaceAll("-", "");
  const reportBytes = readBlob(commit, finding.file);
  const report = JSON.parse(reportBytes.toString("utf8"));
  const line = reportBytes.toString("utf8").split(/\r?\n/)[finding.line - 1];
  const entry = line.match(/"([^"]+)": "([a-f0-9-]+)"/);
  if (!entry) throw new Error(`Expected hash entry at ${finding.file}:${finding.line}`);
  const candidate = finding.category === "verified_candidate_source_sha256";
  const sourceCommit = candidate ? report.base_commit : commit;
  const sourcePath = entry[1] === "openapi_sha256" ? report.source : entry[1];
  const expected = entry[2].replaceAll("-", "");
  if (!/^[a-f0-9]{64}$/.test(expected)) throw new Error("Expected full SHA-256");
  const source = readBlob(sourceCommit, sourcePath);
  const lf = source.toString("utf8").replace(/\r\n/g, "\n");
  // Historical reports were generated from both Linux and Windows checkouts.
  // The blob itself is always checked first; only newline encoding may differ.
  const variants = [source, Buffer.from(lf), Buffer.from(lf.replace(/\n/g, "\r\n"))];
  if (!variants.some(bytes => crypto.createHash("sha256").update(bytes).digest("hex") === expected)) {
    throw new Error(`Source digest mismatch: ${finding.file}:${finding.line} -> ${sourcePath}`);
  }
  if (candidate) candidateHashes++; else portalHashes++;
  process.stdout.write(`HASH_VERIFIED ${commit.slice(0, 7)} ${finding.file}:${finding.line} -> ${sourcePath}\n`);
}
if (candidateHashes !== 44 || portalHashes !== 6) throw new Error("Expected exactly the 50 reviewed hash findings");
process.stdout.write(`REVIEWED_HASH_VERIFICATION_PASS candidate=${candidateHashes} portal=${portalHashes}\n`);
