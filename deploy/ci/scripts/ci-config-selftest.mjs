import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { parse } from "yaml";

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const repositoryRoot = path.resolve(scriptDirectory, "../../..");
const rootCiPath = path.join(repositoryRoot, ".gitlab-ci.yml");
const fragmentDirectory = path.join(repositoryRoot, "deploy/ci");
const fragmentPath = path.join(fragmentDirectory, "ci.gitlab-ci.yml");
const rootCi = parse(await fs.readFile(rootCiPath, "utf8"));
const fragment = parse(await fs.readFile(fragmentPath, "utf8"));
const globalJson = JSON.parse(
  await fs.readFile(path.join(repositoryRoot, "global.json"), "utf8"),
);

const reservedKeys = new Set([
  "after_script",
  "before_script",
  "cache",
  "default",
  "image",
  "include",
  "services",
  "stages",
  "variables",
  "workflow",
]);
const jobs = Object.fromEntries(
  Object.entries(fragment).filter(
    ([name, definition]) =>
      !name.startsWith(".") &&
      !reservedKeys.has(name) &&
      definition &&
      typeof definition === "object",
  ),
);

const requiredJobs = [
  "build_test_dotnet",
  "lint_dotnet",
  "build_lint_ui",
  "openapi_lint",
  "security_scan",
  "pii_scan",
];

for (const jobName of requiredJobs) {
  assert(jobs[jobName], `Required job ${jobName} is missing.`);
}

for (const [jobName, job] of Object.entries(jobs)) {
  assert(
    job.allow_failure === false,
    `${jobName} must set allow_failure: false explicitly.`,
  );
}

const expectedDotnetImage = `mcr.microsoft.com/dotnet/sdk:${globalJson.sdk.version}`;
for (const jobName of ["build_test_dotnet", "lint_dotnet", "security_scan"]) {
  assert(
    jobs[jobName].image === expectedDotnetImage,
    `${jobName} image must match global.json: ${expectedDotnetImage}.`,
  );
}

const dotnetTestServices = jobs.build_test_dotnet.services ?? [];
assert(
  dotnetTestServices.length === 1 &&
    dotnetTestServices[0].name === "docker:29.6.2-dind" &&
    dotnetTestServices[0].alias === "docker",
  "build_test_dotnet must use the pinned Docker-in-Docker service for Testcontainers.",
);
assert(
  jobs.build_test_dotnet.variables?.DOCKER_HOST === "tcp://docker:2375" &&
    jobs.build_test_dotnet.variables?.DOCKER_TLS_CERTDIR === "" &&
    jobs.build_test_dotnet.variables?.TESTCONTAINERS_HOST_OVERRIDE === "docker",
  "build_test_dotnet Testcontainers variables are incomplete.",
);

const includes = (rootCi.include ?? []).map((entry) =>
  typeof entry === "string" ? entry : entry.local,
);
assert(
  (rootCi.include ?? []).every(
    (entry) => typeof entry === "string" || Object.hasOwn(entry, "local"),
  ),
  "Remote or template GitLab includes are not allowed in the baseline.",
);

const fragments = (await fs.readdir(fragmentDirectory))
  .filter((name) => name.endsWith(".gitlab-ci.yml"))
  .map((name) => `/deploy/ci/${name}`)
  .sort();
assertSameSet(includes, fragments, "CT-CI-07 fragment reachability");

const workflowRules = rootCi.workflow?.rules ?? [];
const expectedWorkflowRules = [
  '$CI_PIPELINE_SOURCE == "merge_request_event"',
  '$CI_PIPELINE_SOURCE == "push" && $CI_COMMIT_BRANCH == $CI_DEFAULT_BRANCH',
  '$CI_PIPELINE_SOURCE == "web"',
];
assert(
  workflowRules.length === 4 && workflowRules[3].when === "never",
  "Workflow must end with an explicit when: never rule.",
);
assertSameSet(
  workflowRules.slice(0, 3).map((rule) => rule.if),
  expectedWorkflowRules,
  "CT-CI-05 workflow rules",
);

const scenarios = [
  ["merge_request_event", "feature", "main", true],
  ["push", "main", "main", true],
  ["push", "feature", "main", false],
  ["web", "feature", "main", true],
  ["schedule", "main", "main", false],
  ["schedule", "feature", "main", false],
];

for (const [source, branch, defaultBranch, expected] of scenarios) {
  const actual = evaluateWorkflow(source, branch, defaultBranch);
  assert(
    actual === expected,
    `CT-CI-05 routing mismatch for ${source}/${branch}: ${actual}.`,
  );
}

const artifactJobs = Object.entries(jobs)
  .filter(([, job]) => job.artifacts)
  .map(([name]) => name)
  .sort();
const piiNeeds = jobs.pii_scan.needs ?? [];
const piiNeedNames = piiNeeds.map((need) => need.job).sort();
assertSameSet(piiNeedNames, artifactJobs, "CT-CI-08 artifact topology");
assert(
  piiNeeds.every((need) => need.artifacts === true),
  "Every pii_scan need must download artifacts.",
);
assert(
  jobs.pii_scan.variables?.LC_ALL === "C.UTF-8",
  "pii_scan must set LC_ALL=C.UTF-8 explicitly.",
);

const patterns = await fs.readFile(
  path.join(fragmentDirectory, "pii-patterns.txt"),
  "utf8",
);
assert(!patterns.includes("\b"), "PII patterns must not contain backspace/control escapes.");
assert(
  !/\[[^\]]*[^\x00-\x7F]/u.test(patterns),
  "PII patterns must not use multibyte bracket expressions.",
);
assert(
  patterns.trimEnd().split("\n").length === 6,
  "PII pattern inventory must contain exactly six reviewed patterns.",
);

const scanScript = await fs.readFile(
  path.join(scriptDirectory, "scan-pii.sh"),
  "utf8",
);
assert(
  scanScript.includes("grep -nE -f"),
  "PII scanner must execute grep -nE -f against the pattern file.",
);
assert(
  scanScript.includes("[REDACTED]") && !scanScript.includes('cat "$file"'),
  "PII scanner must redact matched values in CI logs.",
);

const jobScripts = Object.values(jobs)
  .flatMap((job) => job.script ?? [])
  .join("\n");
const scriptFiles = await Promise.all(
  (await fs.readdir(scriptDirectory))
    .filter((name) => name.endsWith(".sh") || name.endsWith(".mjs"))
    .map((name) => fs.readFile(path.join(scriptDirectory, name), "utf8")),
);
const allScripts = [jobScripts, ...scriptFiles].join("\n");
for (const variable of [
  "CI_OPENAPI_SELFTEST_INVALID",
  "CI_DOTNET_SELFTEST_FAIL",
  "CI_COVERAGE_SELFTEST_LOW",
  "CI_SECRET_SELFTEST",
  "CI_PII_SELFTEST_ARTIFACT",
]) {
  assert(allScripts.includes(variable), `Negative pipeline switch ${variable} is missing.`);
}

const githubWorkflowDirectory = path.join(repositoryRoot, ".github/workflows");
let githubWorkflowCount = 0;
try {
  githubWorkflowCount = (await fs.readdir(githubWorkflowDirectory)).length;
} catch (error) {
  if (error.code !== "ENOENT") {
    throw error;
  }
}
assert(githubWorkflowCount === 0, "Active GitHub Actions workflows are forbidden.");

const ciPackage = JSON.parse(
  await fs.readFile(path.join(fragmentDirectory, "package.json"), "utf8"),
);
const openApiFiles = (await fs.readdir(path.join(repositoryRoot, "specs/api/openapi")))
  .filter((name) => name.endsWith(".yaml"));
for (const openApiFile of openApiFiles) {
  assert(
    ciPackage.scripts["openapi:lint"].includes(openApiFile),
    `OpenAPI lint command does not include ${openApiFile}.`,
  );
}

const openApiJobScript = (jobs.openapi_lint.script ?? []).join("\n");
assert(
  openApiJobScript.includes("openapi:drift"),
  "OpenAPI job must enforce pinned hashes and the human-readable report.",
);
const dotnetLintScript = (jobs.lint_dotnet.script ?? []).join("\n");
for (const requiredCodegenToken of [
  "dotnet tool restore",
  "dotnet nswag openapi2csclient",
  "ivr-order-confirmation.v1.yaml",
  "order-core-ivr-callback.target-v1.yaml",
  "git diff --exit-code -- src/Ivr.Contracts/Generated",
]) {
  assert(
    dotnetLintScript.includes(requiredCodegenToken),
    `OpenAPI codegen drift gate is missing ${requiredCodegenToken}.`,
  );
}

process.stdout.write("CT-CI-05 PASS — workflow routing and duplicate prevention\n");
process.stdout.write("CT-CI-07 PASS — every GitLab fragment is reachable\n");
process.stdout.write("CT-CI-08 PASS — every artifact producer feeds pii_scan\n");
process.stdout.write("SDK_IMAGE_PIN_PASS — .NET jobs match global.json\n");
process.stdout.write("TESTCONTAINERS_DIND_PASS — PostgreSQL tests have a pinned Docker service\n");
process.stdout.write("OPENAPI_CODEGEN_GATE_PASS — hashes, report and generated code enforced\n");
process.stdout.write("CI_CONFIG_SELFTEST_PASS\n");

function evaluateWorkflow(source, branch, defaultBranch) {
  if (source === "merge_request_event") {
    return true;
  }

  if (source === "push" && branch && branch === defaultBranch) {
    return true;
  }

  return source === "web";
}

function assert(condition, message) {
  if (!condition) {
    throw new Error(message);
  }
}

function assertSameSet(actual, expected, context) {
  const actualSorted = [...actual].sort();
  const expectedSorted = [...expected].sort();
  assert(
    JSON.stringify(actualSorted) === JSON.stringify(expectedSorted),
    `${context}: actual=${actualSorted.join(",")} expected=${expectedSorted.join(",")}`,
  );
}
