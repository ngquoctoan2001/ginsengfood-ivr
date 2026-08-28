#!/usr/bin/env node
// W-0122 / W-0126 F6. Convert-LabSegmentAudio.ps1 produces the fixed catalog the Owner listens to,
// and W-0122 Phase 3.2 is going to change it again. Its regression was a one-time manual run, so
// nothing would have caught a later edit. This drives the real script in a digest-pinned PowerShell
// container: the segment roster it derives filenames from, and every input it must refuse.
import { randomUUID } from "node:crypto";
import { spawnSync } from "node:child_process";
import { readFileSync } from "node:fs";
import { resolve } from "node:path";

const repoRoot = resolve(import.meta.dirname, "../../..");
const labRoot = resolve(repoRoot, "deploy/lab");
const image = "mcr.microsoft.com/powershell@sha256:"
  + "91cdd71ef0cbf76143501321d33613e1b0711d19236dd05a9fd1195da304df93";
const container = `ivr-lab-converter-${randomUUID().replaceAll("-", "").slice(0, 12)}`;
const script = "/work/lab/Convert-LabSegmentAudio.ps1";

const plan = JSON.parse(readFileSync(resolve(labRoot, "speech-segments.json"), "utf8"));
const fixed = plan.segments.filter(segment => segment.kind === "Fixed");
if (fixed.length !== 4 || plan.regions.length !== 3) {
  throw new Error(`expected 4 fixed segments across 3 regions, found ${fixed.length}/${plan.regions.length}`);
}

try {
  docker([
    "create", "--name", container, "--network", "none", "--entrypoint", "pwsh", image,
    "-NoProfile", "-Command", "while ($true) { Start-Sleep -Seconds 60 }",
  ]);
  docker(["start", container]);
  docker(["exec", container, "mkdir", "-p", "/work/lab", "/work/src", "/work/out"]);
  docker(["cp", `${labRoot}/.`, `${container}:/work/lab`]);

  // The roster drives every output filename (ivr-seg-<region>-<first 16 of textSha256>.wav), so a
  // silent change to segmentation or hashing shows up here before it reaches an Owner listening
  // session that would have to be repeated.
  const listing = pwsh([script, "-ListOnly"]);
  if (listing.status !== 0) throw new Error("-ListOnly did not succeed");
  for (const segment of fixed) {
    const marker = `s${segment.ordinal}  [${segment.textSha256.slice(0, 16)}]`;
    if (!listing.stdout.includes(marker)) {
      throw new Error(`-ListOnly did not report segment ${marker}`);
    }
  }
  const expectedCount = `${fixed.length} câu × ${plan.regions.length} miền = ${fixed.length * plan.regions.length} file`;
  if (!listing.stdout.includes(expectedCount)) {
    throw new Error(`-ListOnly did not report "${expectedCount}"`);
  }
  process.stdout.write(`LAB_CONVERTER_ROSTER_PASS segments=${fixed.length} regions=${plan.regions.length}\n`);

  // Every way of asking the converter to guess instead of being told. -FfmpegPath /bin/sh is not a
  // converter: it only lets the missing-source case reach the file check without pulling ffmpeg
  // into this image.
  const refusals = [
    ["no-source-directory", [script], "Thiếu -SourceDirectory"],
    ["absent-source-directory", [script, "-SourceDirectory", "/work/absent"], "Thư mục nguồn không tồn tại"],
    ["absent-output-directory", [script, "-SourceDirectory", "/work/src", "-OutputDirectory", "/work/absent"], "Thư mục đầu ra không tồn tại"],
    ["unknown-source-extension", [script, "-SourceDirectory", "/work/src", "-SourceExtension", ".flac"], "SourceExtension"],
    ["unknown-region", [script, "-SourceDirectory", "/work/src", "-Region", "atlantis"], "Region"],
    ["missing-converter", [script, "-SourceDirectory", "/work/src", "-OutputDirectory", "/work/out"], "Không tìm thấy ffmpeg"],
    [
      "missing-source-file",
      [script, "-SourceDirectory", "/work/src", "-OutputDirectory", "/work/out", "-FfmpegPath", "/bin/sh"],
      "Thiếu file nguồn",
    ],
  ];
  for (const [name, args, expected] of refusals) {
    const result = pwsh(args);
    const output = result.stdout + result.stderr;
    if (result.status === 0 || !output.includes(expected)) {
      throw new Error(`converter did not fail closed on ${name}: expected "${expected}"`);
    }
    process.stdout.write(`LAB_CONVERTER_REFUSAL_PASS case=${name}\n`);
  }

  process.stdout.write(
    `LAB_CONVERTER_SELFTEST_PASS roster=${fixed.length}x${plan.regions.length} refusals=${refusals.length}\n`,
  );
} finally {
  spawnSync("docker", ["rm", "--force", container], { stdio: "ignore" });
}

function pwsh(args) {
  return docker(["exec", container, "pwsh", "-NoProfile", "-File", ...args], false);
}

function docker(args, expectSuccess = true) {
  const result = spawnSync("docker", args, { encoding: "utf8", maxBuffer: 16 * 1024 * 1024 });
  if (expectSuccess && result.status !== 0) {
    throw new Error(`docker command failed (${result.status}): ${result.stderr || ""}`);
  }
  return { status: result.status, stdout: result.stdout || "", stderr: result.stderr || "" };
}
