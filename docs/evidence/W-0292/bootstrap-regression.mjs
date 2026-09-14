// Run from this repository: node docs/evidence/W-0292/bootstrap-regression.mjs
// Each mutation is confined to a disposable configuration copy under .artifacts.
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { createRequire } from "node:module";
import { spawnSync } from "node:child_process";

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../../..");
const require = createRequire(path.join(root, "deploy/ci/package.json"));
const YAML = require("yaml");
const scratch = fs.mkdtempSync(path.join(root, ".artifacts/w0292-bootstrap-"));
const ci = path.join(scratch, "deploy/ci");
fs.mkdirSync(path.join(ci, "scripts"), { recursive: true });
fs.copyFileSync(path.join(root, ".gitlab-ci.yml"), path.join(scratch, ".gitlab-ci.yml"));
for (const name of fs.readdirSync(path.join(root, "deploy/ci"))) {
  if (/\.ya?ml$/.test(name)) {
    fs.copyFileSync(path.join(root, "deploy/ci", name), path.join(ci, name));
  }
}
fs.copyFileSync(path.join(root, "deploy/ci/scripts/cd-selftest.mjs"), path.join(ci, "scripts/cd-selftest.mjs"));
fs.cpSync(path.join(root, "deploy/ci/node_modules/yaml"), path.join(ci, "node_modules/yaml"), { recursive: true });
const original = YAML.parse(fs.readFileSync(path.join(ci, "cd.gitlab-ci.yml"), "utf8"), { merge: true });
let refused = 0;
for (const job of ["deploy_dev", "deploy_staging"]) {
  for (const defect of ["entrypoint", "kubectl"]) {
    const candidate = structuredClone(original);
    if (defect === "entrypoint") candidate[job].image = "alpine/helm:3.16.3";
    else candidate[job].before_script = ["helm version --short"];
    fs.writeFileSync(path.join(ci, "cd.gitlab-ci.yml"), YAML.stringify(candidate));
    const run = spawnSync(process.execPath, [path.join(ci, "scripts/cd-selftest.mjs")], { encoding: "utf8" });
    const expected = defect === "entrypoint" ? `${job} must clear the Helm image entrypoint` : `${job} must install and check kubectl`;
    if (run.status !== 1 || !run.stderr.includes(expected)) throw new Error(`Wrong refusal for ${job}/${defect}: ${run.status} ${run.stderr}`);
    process.stdout.write(`BOOTSTRAP_REFUSAL_PASS ${job}/${defect}\n`);
    refused++;
  }
}
fs.writeFileSync(path.join(ci, "cd.gitlab-ci.yml"), YAML.stringify(original));
const valid = spawnSync(process.execPath, [path.join(ci, "scripts/cd-selftest.mjs")], { encoding: "utf8" });
if (valid.status !== 0 || !valid.stdout.includes("CD_SELFTEST_PASS")) throw new Error(`Valid config failed: ${valid.status} ${valid.stderr}`);
process.stdout.write(`BOOTSTRAP_REGRESSION_PASS negative=${refused} positive=1\n`);
