#!/usr/bin/env node
import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import YAML from "yaml";

const repoRoot = resolve(import.meta.dirname, "../../..");
const manifest = JSON.parse(readFileSync(
  resolve(repoRoot, "docs/evidence/W-0122/audition-manifest.json"), "utf8",
));
const checksumText = readFileSync(
  resolve(repoRoot, "deploy/lab/asterisk/w0122-audition/SHA256SUMS"), "utf8",
).trim();
const dialplan = readFileSync(
  resolve(repoRoot, "deploy/lab/asterisk/w0122-audition/extensions.conf"), "utf8",
);
const dialplanCode = dialplan.split(/\r?\n/).map(line => line.replace(/;.*/, "")).join("\n");
const compose = YAML.parse(readFileSync(
  resolve(repoRoot, "docker-compose.vieneu-tts-audition.yml"), "utf8",
));

function assert(condition, message) {
  if (!condition) throw new Error(message);
}

assert(manifest.schema_version === 1 && manifest.work_id === "W-0122", "manifest identity drift");
assert(Array.isArray(manifest.results) && manifest.results.length === 11, "manifest roster must be 11");
assert(new Set(manifest.results.map(result => result.file)).size === 11, "manifest files must be unique");
for (const result of manifest.results) {
  assert(/^audition-v3t-(?:north|central|south)-[a-z0-9-]+\.wav$/.test(result.file), "manifest file outside allowlist");
  assert(/^[a-f0-9]{64}$/.test(result.sha256), "manifest digest invalid");
  assert(Number.isSafeInteger(result.bytes) && result.bytes > 44, "manifest size invalid");
}

const checksumEntries = checksumText.split(/\r?\n/).map(line => {
  const match = /^([a-f0-9]{64})  (audition-[a-z0-9-]+\.wav)$/.exec(line);
  assert(match, `invalid checksum line: ${line}`);
  return { sha256: match[1], file: match[2] };
});
assert(checksumEntries.length === 11, "checksum roster must be 11");

for (const [index, result] of manifest.results.entries()) {
  assert(result.file === checksumEntries[index].file, `checksum file order drift at ${index}`);
  assert(result.sha256 === checksumEntries[index].sha256, `checksum hash drift at ${index}`);
  const stem = result.file.slice(0, -4);
  assert(dialplan.split(stem).length - 1 === 2, `dialplan must reference ${stem} twice`);
}

const expectedExtensions = ["12200", ...Array.from({ length: 11 }, (_, index) => String(12201 + index))];
const actualExtensions = [...dialplan.matchAll(/^exten => (\d+),1,/gm)].map(match => match[1]);
assert(JSON.stringify(actualExtensions) === JSON.stringify(expectedExtensions), "audition extension map drift");
assert(/^exten => _X!,1,/m.test(dialplan), "catch-all rejection missing");
assert(!/\b(?:Dial|Stasis|System|SHELL)\s*\(/i.test(dialplanCode), "outbound or shell application forbidden");

const verifier = compose.services?.["w0122-audition-verify"];
const asterisk = compose.services?.asterisk;
assert(verifier && asterisk, "audition services missing");
assert(verifier.image === "busybox:1.37.0-musl@sha256:29989570aeecad61a019f684218ea74d4b8c1c74f9e0abeb34ca926b81174ee1", "verifier image drift");
assert(verifier.network_mode === "none" && verifier.read_only === true, "verifier isolation drift");
assert(Array.isArray(verifier.cap_drop) && verifier.cap_drop.includes("ALL"), "verifier caps drift");
assert(verifier.volumes.every(value => typeof value === "string" && value.endsWith(":ro")), "verifier mounts must be read-only");
assert(asterisk.volumes.some(value => value.endsWith("/w0122-audition:ro")), "audition audio mount missing");
assert(asterisk.volumes.some(value => value.endsWith("/etc/asterisk/extensions.conf:ro")), "audition dialplan mount missing");
assert(asterisk.depends_on?.["w0122-audition-verify"]?.condition === "service_completed_successfully", "Asterisk must wait for verifier");
assert(!Object.hasOwn(asterisk, "ports"), "audition overlay must not add ports");

process.stdout.write("TTS_AUDITION_SELFTEST_PASS voices=11 extensions=12 outbound=DENIED\n");
