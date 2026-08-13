import crypto from "node:crypto";
import fs from "node:fs/promises";
import os from "node:os";
import path from "node:path";
import YAML from "yaml";
import {
  buildApiDocs,
  defaultOutputDirectory,
  generatedPortalFiles,
  repositoryRoot,
} from "./build-api-docs.mjs";

const temporaryRoot = await fs.mkdtemp(path.join(os.tmpdir(), "ivr-api-docs-"));
const generatedDirectory = path.join(temporaryRoot, "generated");

try {
  await buildApiDocs(generatedDirectory);
  await assertSameGeneratedFiles(defaultOutputDirectory, generatedDirectory);

  const mutatedDirectory = path.join(temporaryRoot, "mutated");
  await fs.cp(generatedDirectory, mutatedDirectory, { recursive: true });
  await fs.appendFile(path.join(mutatedDirectory, "index.html"), "<!-- intentional drift -->\n");
  let driftRejected = false;
  try {
    await assertSameGeneratedFiles(defaultOutputDirectory, mutatedDirectory);
  } catch (error) {
    driftRejected = String(error.message).includes("API_DOCS_DRIFT");
  }
  assert(driftRejected, "CT-DOC-01 negative drift fixture was not rejected.");

  await assertNoRealPiiExamples();
  await assertPortalBoundaries();
  await assertGeneratedLinks();
  await assertCiTopology();
  await assertBaselineManifest();

  process.stdout.write("CT-DOC-01 PASS — generated portal matches OpenAPI and drift is rejected\n");
  process.stdout.write("UT-DOC-PII-03 PASS — docs sources contain no real phone or full street address examples\n");
  process.stdout.write("DOC_BOUNDARY_PASS — Target draft and current compatibility stay separate\n");
  process.stdout.write("DOC_LINKS_PASS — every generated local portal link resolves\n");
  process.stdout.write("DOC_CI_TOPOLOGY_PASS — verify, oasdiff and non-prod Pages jobs are root-included\n");
  process.stdout.write("API_DOCS_SELFTEST_PASS\n");
} finally {
  await fs.rm(temporaryRoot, { recursive: true, force: true });
}

async function assertSameGeneratedFiles(expectedDirectory, actualDirectory) {
  for (const fileName of generatedPortalFiles) {
    const expected = await fs.readFile(path.join(expectedDirectory, fileName));
    const actual = await fs.readFile(path.join(actualDirectory, fileName));
    if (!expected.equals(actual)) {
      throw new Error(`API_DOCS_DRIFT: ${fileName} does not match its source.`);
    }
  }
}

async function assertNoRealPiiExamples() {
  const sourceFiles = [
    "specs/api/openapi/ivr-order-confirmation.v1.yaml",
    "specs/api/openapi/order-core-ivr-callback.target-v1.yaml",
    "specs/api/compat/current-golden-hour-callback.a3aad246.schema.json",
    "docs/integration-guide.md",
    "docs/api-versioning.md",
    "docs/api-changelog.md",
    "docs/api/changelog/ivr-order-confirmation.md",
    "docs/api/changelog/order-core-ivr-callback.md",
  ];
  const rawVietnamesePhone = /(?:\+84|0)[35789]\d{8}/u;
  const fullStreetAddress = /\b\d{1,5}[\w/-]*\s+(?:đường|phố|street|road|ngõ|hẻm|ấp)\b/iu;
  const assignedDialToken = /dial[_-]?token["']?\s*[:=]\s*["']?[A-Za-z0-9._-]{8,}/iu;

  for (const sourceFile of sourceFiles) {
    const content = await fs.readFile(path.join(repositoryRoot, sourceFile), "utf8");
    assert(!rawVietnamesePhone.test(content), `${sourceFile} contains a raw Vietnamese phone example.`);
    assert(!fullStreetAddress.test(content), `${sourceFile} contains a full street-address example.`);
    assert(!assignedDialToken.test(content), `${sourceFile} contains an assigned dialing-token example.`);
  }
}

async function assertPortalBoundaries() {
  const index = await fs.readFile(path.join(defaultOutputDirectory, "index.html"), "utf8");
  const compat = await fs.readFile(
    path.join(defaultOutputDirectory, "current-golden-hour-compat.html"),
    "utf8",
  );
  assert(index.includes("TARGET_DRAFT") && index.includes("NON-PRODUCTION ONLY"), "Portal must label Target and environment state.");
  assert(index.includes("current-golden-hour-compat.html"), "Portal must link current compatibility separately.");
  assert(compat.includes("This is not Target V1"), "Current compatibility page must reject Target equivalence.");
  assert(compat.includes("a3aad246d986fbc273cf41aaa93eec6659669656"), "Current compatibility page must show the verified Sales SHA.");
}

async function assertGeneratedLinks() {
  for (const fileName of generatedPortalFiles.filter((file) => file.endsWith(".html"))) {
    const html = await fs.readFile(path.join(defaultOutputDirectory, fileName), "utf8");
    const links = [...html.matchAll(/(?:href|src)=["']([^"'#?]+)["']/gu)].map((match) => match[1]);
    for (const link of links) {
      if (/^(?:https?:|data:|blob:|\/)/u.test(link)) {
        continue;
      }
      await fs.access(path.join(defaultOutputDirectory, link));
    }
  }
}

async function assertCiTopology() {
  const root = YAML.parse(await fs.readFile(path.join(repositoryRoot, ".gitlab-ci.yml"), "utf8"));
  const fragment = YAML.parse(
    await fs.readFile(path.join(repositoryRoot, "deploy/ci/docs.gitlab-ci.yml"), "utf8"),
  );
  const includes = (root.include ?? []).map((entry) => (typeof entry === "string" ? entry : entry.local));
  assert(includes.includes("/deploy/ci/docs.gitlab-ci.yml"), "Root GitLab config must include docs fragment.");
  assert((root.stages ?? []).includes("publish"), "Root GitLab config must expose the publish stage.");
  assert(root.variables?.API_DOCS_PUBLISH_NONPROD === "NO", "Docs publication must default denied.");

  for (const jobName of ["api_docs_verify", "api_contract_diff", "api_docs_pages"]) {
    assert(fragment[jobName], `Rendered docs pipeline is missing ${jobName}.`);
    assert(fragment[jobName].allow_failure === false, `${jobName} must fail closed.`);
  }
  const pages = fragment.api_docs_pages;
  assert(pages.pages === true, "api_docs_pages must be an explicit GitLab Pages job.");
  assert(
    (pages.script ?? []).some((line) =>
      String(line).includes("docs:build -- --output ../../public"),
    ),
    "Pages output must resolve to the repository-root public directory when npm --prefix deploy/ci is used.",
  );
  assert(pages.environment?.deployment_tier === "development", "API docs environment must be non-production.");
  assert(
    (pages.rules ?? []).some((rule) => String(rule.if ?? "").includes("API_DOCS_PUBLISH_NONPROD")),
    "Pages job must require the explicit non-prod publication gate.",
  );
}

async function assertBaselineManifest() {
  const manifest = JSON.parse(
    await fs.readFile(path.join(repositoryRoot, "specs/api/openapi/changelog-baseline.json"), "utf8"),
  );
  assert(manifest.tool?.version === "1.26.1", "oasdiff baseline must pin version 1.26.1.");
  assert(String(manifest.tool?.image).includes("@sha256:"), "oasdiff image must be digest pinned.");
  for (const contract of manifest.contracts ?? []) {
    const hash = crypto
      .createHash("sha256")
      .update(await fs.readFile(path.join(repositoryRoot, contract.baseline)))
      .digest("hex");
    assert(hash === contract.baselineSha256, `Changelog baseline drift: ${contract.baseline}.`);
  }
}

function assert(condition, message) {
  if (!condition) {
    throw new Error(message);
  }
}
