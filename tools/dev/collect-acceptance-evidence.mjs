// W-0319. Run on a clean checkout, from Git Bash on Windows (the sweep invokes sh).
// No import-existing-results mode: provenance is captured around the commands that produced them.
import assert from "node:assert/strict";
import { spawn, spawnSync } from "node:child_process";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import {
  RUN_SCHEMA, GATE_MANIFEST, TEST_COMMAND, SWEEP_COMMAND, sha256, assertRunSource,
  expectedTestAssemblies, readPinnedArtifact, validateRunEvidence,
} from "../../deploy/ci/scripts/acceptance-evidence-lib.mjs";

function git(root, args) {
  const result = spawnSync("git", args, { cwd: root, encoding: "utf8", windowsHide: true, maxBuffer: 32 << 20 });
  assert.equal(result.status, 0, result.error?.message ?? result.stderr);
  return result.stdout;
}

export function captureSource(root) {
  return {
    commit: git(root, ["rev-parse", "HEAD"]).trim(),
    tree: git(root, ["rev-parse", "HEAD^{tree}"]).trim(),
    clean: git(root, ["status", "--porcelain", "--untracked-files=all"]).trim() === "",
  };
}

export async function runCommand(command, { root, output, logFile }) {
  const argv = [...command.slice(1)];
  if (command === TEST_COMMAND) argv[argv.length - 1] = path.join(output, "trx");
  const fd = fs.openSync(logFile, "wx");
  try {
    return await new Promise((resolve, reject) => {
      const child = spawn(command[0], argv, { cwd: root, windowsHide: true, stdio: ["ignore", "pipe", "pipe"] });
      const write = (chunk) => { fs.writeSync(fd, chunk); process.stdout.write(chunk); };
      child.stdout.on("data", write);
      child.stderr.on("data", write);
      child.once("error", reject);
      child.once("close", (code) => resolve(code ?? -1));
    });
  } finally {
    fs.closeSync(fd);
  }
}

function filesUnder(directory) {
  return fs.readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const full = path.join(directory, entry.name);
    return entry.isDirectory() ? filesUnder(full) : [full];
  });
}

export async function collectAcceptanceEvidence({
  root, output, snapshot = () => captureSource(root), run = runCommand,
  readSource = (commit, file) => git(root, ["show", `${commit}:${file}`]),
}) {
  const source = snapshot();
  assertRunSource(source, source);
  const manifestBytes = readSource(source.commit, GATE_MANIFEST);
  const assemblies = expectedTestAssemblies(readSource(source.commit, "Ivr.sln"), (file) => readSource(source.commit, file));
  // A new directory makes it impossible for a failed invocation to inherit old successful output.
  assert(!fs.existsSync(output), "output already exists; choose a new directory");
  fs.mkdirSync(output, { recursive: true });
  const bundle = { schema: RUN_SCHEMA, gateManifestSha256: sha256(manifestBytes) };
  const pin = (file) => ({ path: path.relative(output, file).split(path.sep).join("/"), sha256: sha256(fs.readFileSync(file)) });
  for (const [name, command] of [["tests", TEST_COMMAND], ["sweep", SWEEP_COMMAND]]) {
    const before = snapshot();
    assertRunSource(before, source);
    const startedAt = new Date().toISOString();
    const logFile = path.join(output, `${name}.log`);
    const exitCode = await run(command, { root, output, logFile });
    const finishedAt = new Date().toISOString();
    const after = snapshot();
    assertRunSource(after, source);
    assert.equal(exitCode, 0, `${name} failed; logs retained, no acceptance manifest written`);
    bundle[name] = { before, after, startedAt, finishedAt, command, exitCode };
    if (name === "tests") {
      bundle.tests.files = filesUnder(path.join(output, "trx")).filter((file) => file.endsWith(".trx")).map(pin);
      // Pin before the sweep: a gate must not replace the solution results with its own filtered run.
    } else {
      bundle.sweep.file = pin(logFile);
    }
  }
  validateRunEvidence(bundle, {
    source, manifestBytes, assemblies, readArtifact: (artifact) => readPinnedArtifact(output, artifact),
  });
  const target = path.join(output, "acceptance-run.json");
  fs.writeFileSync(target, JSON.stringify(bundle, null, 2) + "\n", { flag: "wx" });
  return target;
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const index = process.argv.indexOf("--out");
  try {
    assert(index !== -1 && process.argv[index + 1], "usage: node tools/dev/collect-acceptance-evidence.mjs --out <new-directory>");
    const target = await collectAcceptanceEvidence({
      root: path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../.."),
      output: path.resolve(process.argv[index + 1]),
    });
    process.stdout.write(`ACCEPTANCE_EVIDENCE_COLLECTED ${target}\n`);
  } catch (error) {
    process.stderr.write(`ACCEPTANCE_EVIDENCE_REFUSED ${error.message}\n`);
    process.exitCode = 1;
  }
}
