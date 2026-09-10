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
const ivrOpenApi = parse(
  await fs.readFile(
    path.join(repositoryRoot, "specs/api/openapi/ivr-order-confirmation.v1.yaml"),
    "utf8",
  ),
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

const cacheDefinitions = Object.entries(fragment).filter(
  ([, definition]) =>
    definition &&
    typeof definition === "object" &&
    definition.cache?.key?.files !== undefined,
);
for (const [definitionName, definition] of cacheDefinitions) {
  const cacheKeyFiles = definition.cache.key.files;
  assert(
    Array.isArray(cacheKeyFiles) && cacheKeyFiles.length > 0 && cacheKeyFiles.length <= 2,
    `${definitionName} cache:key:files must contain between one and two entries for GitLab compatibility.`,
  );
}
assertSameSet(
  fragment[".dotnet_cache"].cache.key.files,
  ["global.json", "dotnet-tools.json"],
  ".NET cache key inputs",
);

const requiredJobs = [
  "build_test_dotnet",
  "lint_dotnet",
  "openapi_lint",
  "security_scan",
  "pii_scan",
  // W-0114. The rolling-deploy schema gate. Listed here so deleting the job is a red pipeline
  // rather than a quiet loss of the only check on how migrations are written.
  "schema_compat_gate",
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
for (const jobName of [
  "build_test_dotnet",
  "lint_dotnet",
  "security_scan",
  "schema_compat_gate",
]) {
  assert(
    jobs[jobName].image === expectedDotnetImage,
    `${jobName} image must match global.json: ${expectedDotnetImage}.`,
  );
}
assert(
  jobs.security_scan.variables?.GIT_DEPTH === "0",
  "security_scan must disable shallow cloning so Gitleaks fingerprints stay tied to their source commits.",
);

const dotnetTestServices = jobs.build_test_dotnet.services ?? [];
assert(
  dotnetTestServices.length === 1 &&
    dotnetTestServices[0].name === "docker:29.6.2-dind" &&
    dotnetTestServices[0].alias === "docker",
  "build_test_dotnet must use the pinned Docker-in-Docker service for Testcontainers.",
);
const dotnetTestScript = (jobs.build_test_dotnet.script ?? []).join("\n");
assert(
  dotnetTestScript.includes("selftest-dotnet-policy.sh"),
  "build_test_dotnet must run the semantic test, coverage, and vulnerability policy self-test.",
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
assert(
  scanScript.includes('find "$target" -type f -print') &&
    !scanScript.includes("-name '*.txt'") &&
    scanScript.includes("no text files found"),
  "PII scanner must inspect all text artifacts and fail closed when a target yields none.",
);

const securityScanScript = await fs.readFile(
  path.join(scriptDirectory, "security-scan.sh"),
  "utf8",
);
assert(
  securityScanScript.includes('scan_commit="${CI_COMMIT_SHA:-HEAD}"') &&
    securityScanScript.includes('"${scan_commit}^{commit}"') &&
    securityScanScript.includes('--log-opts="$scan_commit"'),
  "Gitleaks history scan must be anchored to the validated pipeline commit.",
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

const documentedErrorCodes = extractMatches(
  await fs.readFile(path.join(repositoryRoot, "specs/api/06-error-codes.md"), "utf8"),
  /^\| `(?<code>IVR_[A-Z0-9_]+)` \|/gmu,
);
const sourceErrorCodes = extractMatches(
  await fs.readFile(path.join(repositoryRoot, "src/Ivr.Domain/Errors/IvrErrorCodes.cs"), "utf8"),
  /public const string \w+ = "(?<code>IVR_[A-Z0-9_]+)";/gu,
);
// W-0128. One catalogue again, not two. `ConsoleAccountErrorCode` existed so the account API
// could add codes without touching the enum every older operation already referenced; deleting
// that API deleted the reason for the split, and the two account-only codes went with it —
// 18 down to 16. The parity check is what matters and it is unchanged: the spec, `API-06` and
// `IvrErrorCodes.cs` must name the same set, so a code added to any one of them fails here until
// it reaches the other two.
const openApiErrorCodes = ivrOpenApi.components?.schemas?.ErrorCode?.enum ?? [];
assert(
  documentedErrorCodes.length === 16,
  `Stable error catalog must contain 16 codes; found ${documentedErrorCodes.length}.`,
);
assertSameSet(openApiErrorCodes, documentedErrorCodes, "CT-CI-10 OpenAPI/API-06 parity");
assertSameSet(sourceErrorCodes, documentedErrorCodes, "CT-CI-10 source/API-06 parity");

// W-0206. Every script entry must parse as a string.
//
// This gate said CI_CONFIG_SELFTEST_PASS while the first hosted pipeline ever run on this project
// was refused outright: `jobs:observability_helm:script config should be a string or a nested
// array of strings`. The cause was one unquoted line - `grep -q 'name: OTEL_...' file` - where the
// inner quotes are not YAML quoting, so the `: ` inside made YAML read the whole entry as a
// mapping. A gate that validates CI config and cannot see that is checking everything except the
// thing that stops the pipeline, so it is checked here now, on every job and every script hook.
// Across EVERY included fragment, not just ci.gitlab-ci.yml. The rest of this file inspects one
// of the thirteen, which is why the first version of this very check passed while the mutation was
// in place: it never looked at observability.gitlab-ci.yml at all.
for (const includePath of [".gitlab-ci.yml", ...includes]) {
  const absolute = path.join(repositoryRoot, includePath.replace(/^\//, ""));
  const parsed = parse(await fs.readFile(absolute, "utf8")) ?? {};

  for (const [jobName, definition] of Object.entries(parsed)) {
    if (definition === null || typeof definition !== "object" || reservedKeys.has(jobName)) {
      continue;
    }

    for (const hook of ["script", "before_script", "after_script"]) {
      const entries = definition[hook];
      if (entries === undefined) {
        continue;
      }

      assert(
        Array.isArray(entries) || typeof entries === "string",
        `${includePath} ${jobName}.${hook} must be a string or an array of strings.`,
      );

      const list = Array.isArray(entries) ? entries : [entries];
      for (const [index, entry] of list.entries()) {
        assert(
          typeof entry === "string",
          `${includePath} ${jobName}.${hook}[${index}] parsed as ` +
            `${JSON.stringify(entry).slice(0, 90)} instead of a string. An unquoted shell line ` +
            "containing a colon followed by a space is read by YAML as a key/value pair; wrap " +
            "the whole line in double quotes.",
        );
      }
    }
  }
}

// W-0258. Path confinement must exist once, and its root must be canonical.
//
// Ten validators each carried their own `isConfined`, each deriving REPOSITORY_ROOT with an
// unresolved `resolve(dirname(SCRIPT_PATH), "../../..")` and then comparing it against the
// `realpathSync` of an input. That comparison only holds while no component of the checkout path
// is a symlink — /tmp on macOS, a runner workspace link, a bind mount. Under one, `relative()`
// returns `..`-prefixed for every legitimate file and the gate refuses its own inputs while
// printing `real path escapes repository root`: a traversal accusation aimed at the environment.
//
// One copy had been fixed. The other nine never heard about it, because a fix cannot propagate
// through copies. So the check here is not "the root is resolved" — it is "there is only one
// place where that could be wrong."
{
  const scriptsDirectory = path.join(repositoryRoot, "deploy/ci/scripts");
  const libraryName = "repository-path-lib.mjs";
  const libraryPath = path.join(scriptsDirectory, libraryName);
  const libraryText = await fs.readFile(libraryPath, "utf8");

  assert(
    /export const REPOSITORY_ROOT = realpathSync\(/.test(libraryText),
    `${libraryName} must canonicalise REPOSITORY_ROOT with realpathSync, or every confinement `
      + "check it backs is wrong under a symlinked checkout.",
  );

  const offenders = [];
  for (const name of (await fs.readdir(scriptsDirectory)).filter((f) => f.endsWith(".mjs"))) {
    if (name === libraryName) {
      continue;
    }

    const text = await fs.readFile(path.join(scriptsDirectory, name), "utf8");
    if (/^(?:function isConfined|const isConfined\s*=)/m.test(text)) {
      offenders.push(name);
    }
  }

  assert(
    offenders.length === 0,
    `These scripts define their own isConfined instead of importing ${libraryName}: `
      + `${offenders.join(", ")}. A confinement helper in more than one shape means nobody can `
      + "say which one is the rule, and a fix to one of them reaches none of the others.",
  );
}

// W-0259. Copy-pasted validator helpers, held at their current count.
//
// W-0258 unified isConfined because its copies had diverged and the divergence had already cost
// something: the realpath fix reached one of ten. A census of the rest says that was not the
// exception. `assertIdentifier` has seven copies and seven distinct implementations — no two
// agree. `assertNoSensitiveValue`, which is the guard against a secret reaching an evidence
// bundle, has five copies and five implementations.
//
// Those are NOT unified here, and the reason is not effort. Their differences are load-bearing:
// character sets differ, length bounds differ, and two of the seven assertIdentifier copies call
// assertNoSensitiveValue while five do not. Picking one set of semantics changes which evidence
// bundles the gates accept, in a direction nobody has decided. That is an owner's call, not a
// refactor's.
//
// What can be done without deciding anything is to stop it getting worse. The baseline records
// what exists; this fails when a helper gains a copy or an implementation. It fails on a decrease
// too — that is progress, and the record should say so rather than quietly drift the other way.
{
  const scriptsDirectory = path.join(repositoryRoot, "deploy/ci/scripts");
  const censusPath = path.join(repositoryRoot, "deploy/ci/validator-helper-census.json");
  const baseline = JSON.parse(await fs.readFile(censusPath, "utf8")).helpers;
  const scriptNames = (await fs.readdir(scriptsDirectory)).filter((f) => f.endsWith(".mjs")).sort();
  const sources = await Promise.all(
    scriptNames.map((name) => fs.readFile(path.join(scriptsDirectory, name), "utf8")),
  );

  const drift = [];
  for (const [helper, expected] of Object.entries(baseline)) {
    const pattern = new RegExp(String.raw`^function ${helper}\((?:.|\n)*?^\}`, "gm");
    const bodies = new Set();
    let copies = 0;
    for (const text of sources) {
      for (const match of text.matchAll(pattern)) {
        copies += 1;
        // Whitespace-normalised, so reformatting one copy is not reported as a new
        // implementation. Anything that survives that is a real difference in what it does.
        bodies.add(match[0].replace(/\s+/gu, " ").trim());
      }
    }

    if (copies !== expected.copies || bodies.size !== expected.implementations) {
      drift.push(
        `${helper}: ${copies} copies / ${bodies.size} implementations, `
          + `baseline ${expected.copies} / ${expected.implementations}`,
      );
    }
  }

  assert(
    drift.length === 0,
    "Validator helper duplication moved from its recorded baseline:\n  "
      + `${drift.join("\n  ")}\n`
      + "If a copy was removed, lower deploy/ci/validator-helper-census.json to match. If one was "
      + "added, import the helper instead — a security check in more than one shape means nobody "
      + "can say which one is the rule.",
  );
}

// W-0260. The shared secret rule, and the one carve-out in it.
//
// Owner decision on 2026-09-09: of the seven assertIdentifier copies, the two that screened for
// secrets are the rule. Applying that to the other five is only safe if the rule itself is right,
// and it was not — the phone heuristic counts nine to fifteen digits separated by anything in
// `[\s().-]`, so `CONFIG-2026-09-04-01` read as a ten-digit phone number and a legitimate config
// version became a refusal. ISO dates are cut out before that test.
//
// A carve-out in a security check earns a test that proves it did not open a hole. The evasion to
// beat is a phone number wearing a date's shape.
{
  const { findSensitiveValue } = await import("./sensitive-value-lib.mjs");
  const cases = [
    // Legitimate identifiers this system actually uses. Every one of these has enough digits to
    // trip the raw phone pattern.
    ["CONFIG-2026-09-04-01", null],
    ["M8-07-SECTION-6.2026-09-04", null],
    ["C10-C11-C13-D06.2026-09-04", null],
    ["ATTEMPT-POLICY/BUNDLE/2026-09-04/V1", null],

    // Still caught, in every form the five unified copies knew between them.
    ["CALL:+84912345678", "a phone-like value"],
    ["+84 912 345 678", "a phone-like value"],
    ["USER@EXAMPLE.COM", "an email-like value"],
    ["12 Duong Le Loi", "a street-address-like value"],
    ["BEARER SECRET", "credential- or secret-like material"],

    // `BEARER:token` was caught by exactly one of the five. Taking the widest form is the reason
    // for unifying them rather than picking one.
    ["BEARER:ABCDEF", "credential- or secret-like material"],
    [
      "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.abcdefghijk",
      "credential- or secret-like material",
    ],

    // The evasion. A loose `\d{4}-\d{2}-\d{2}` would strip eight digits of this number and let the
    // rest through, so the date pattern pins plausible years, months and days instead.
    ["0912-34-5678", "a phone-like value"],
  ];

  for (const [value, expected] of cases) {
    const actual = findSensitiveValue(value);
    assert(
      actual === expected,
      `findSensitiveValue(${JSON.stringify(value)}) returned ${JSON.stringify(actual)}, `
        + `expected ${JSON.stringify(expected)}`,
    );
  }
}

// W-0267. One duplicate-key parser, and the eight validators that used to carry their own.
//
// Eight copies, five distinct implementations. Before unifying them they were run against the
// same eighteen adversarial documents and agreed on all eighteen — the five differed in shape,
// not in verdict, so picking the majority implementation changed nothing any gate accepts.
//
// Only three of the eight selftests ever fed a duplicate key. The other five reached this parser
// with no test touching it, which is why the cases live here now rather than in one validator.
{
  const { rejectDuplicateJsonKeys, JsonShapeError } = await import("./json-shape-lib.mjs");
  const cases = [
    ['{"a":1,"b":2}', null],
    ["{}", null],
    ['"hello"', null],
    ["42", null],

    // The duplicate has to be caught wherever it hides, not just at the root — a bundle that
    // buries it three levels down is the one a hand-written check misses.
    ['{"a":1,"a":2}', "duplicate JSON key: a"],
    ['{"outer":{"a":1,"a":2}}', "duplicate JSON key: a"],
    ['{"items":[{"a":1,"a":2}]}', "duplicate JSON key: a"],
    ['{"x":[[{"k":1,"k":2}]]}', "duplicate JSON key: k"],
    ['[{"a":1,"a":2}]', "duplicate JSON key: a"],
    ['{"a":{"z":1},"a":2}', "duplicate JSON key: a"],

    // The evasion that matters: a key spelled as an escape is the same key. Comparing raw source
    // text instead of decoded values would let this one through.
    ['{"a":1,"\\u0061":2}', "duplicate JSON key: a"],
    ['{"a":1,"\\u0062":2}', null],
    ['{"a\\"b":1,"a\\"b":2}', 'duplicate JSON key: a"b'],

    // A second document appended after the first is how a signed prefix gets reused.
    ['{"a":1} {"a":2}', "unexpected content after JSON document"],
    ['{"a":"x', "unterminated JSON string"],

    // An escaped newline is legal JSON and stays legal; a raw control character in the middle
    // of a string is not. Both are here because the first version of this test carried a
    // mangled literal that turned the escape into a real newline — and it passed, for the
    // wrong reason, asserting a restriction the parser does not have.
    ['{"a":"x\\ny"}', null],
    ['{"a":"x\ty"}', "invalid control character in JSON string"],
  ];

  for (const [document, expected] of cases) {
    let actual = null;
    try {
      rejectDuplicateJsonKeys(document);
    } catch (error) {
      assert(
        error instanceof JsonShapeError,
        `rejectDuplicateJsonKeys(${JSON.stringify(document)}) threw ${error.constructor.name}, `
          + "which the validator wrappers would rethrow instead of reporting as a shape failure",
      );
      actual = error.message;
    }

    assert(
      actual === expected,
      `rejectDuplicateJsonKeys(${JSON.stringify(document)}) gave ${JSON.stringify(actual)}, `
        + `expected ${JSON.stringify(expected)}`,
    );
  }
}

process.stdout.write("JSON_SHAPE_SINGLE_SOURCE_PASS — one duplicate-key parser, escapes decoded before comparison\n");
process.stdout.write("SENSITIVE_VALUE_RULE_PASS — one secret rule, dates excused, evasion still caught\n");
process.stdout.write("PATH_CONFINEMENT_SINGLE_SOURCE_PASS — one canonical root, one isConfined\n");
process.stdout.write("VALIDATOR_HELPER_CENSUS_PASS — helper duplication held at its baseline\n");
process.stdout.write("CT-CI-05 PASS — workflow routing and duplicate prevention\n");
process.stdout.write("CT-CI-07 PASS — every GitLab fragment is reachable\n");
process.stdout.write("CT-CI-08 PASS — every artifact producer feeds pii_scan\n");
process.stdout.write("CT-CI-10 PASS — OpenAPI, API-06 and source error catalogs match\n");
process.stdout.write("CACHE_KEY_FILES_PASS — every cache key uses at most two inputs\n");
process.stdout.write("SDK_IMAGE_PIN_PASS — .NET jobs match global.json\n");
process.stdout.write("TESTCONTAINERS_DIND_PASS — PostgreSQL tests have a pinned Docker service\n");
process.stdout.write("GITLEAKS_COMMIT_SCOPE_PASS — history scan is anchored to the validated pipeline commit\n");
process.stdout.write("OPENAPI_CODEGEN_GATE_PASS — hashes, report and generated code enforced\n");
process.stdout.write("SCRIPT_ENTRY_STRING_PASS — every job script line parses as a string\n");
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

function extractMatches(text, pattern) {
  return [...text.matchAll(pattern)].map((match) => match.groups.code);
}
