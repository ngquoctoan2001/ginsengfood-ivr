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
  process.stdout.write("DOC_CI_TOPOLOGY_PASS — verify, oasdiff, Pages, contract/e2e, quality-gate, UI QA, observability, chaos, image, chart, DR and delivery jobs are root-included\n");
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

  // W-0036 / P5-2 §7. The contract and e2e fragment must be root-included and fail closed.
  // Writing the file is not the deliverable — a fragment nobody includes runs nowhere, and a
  // suite that may fail is a suite nobody reads.
  assert(
    includes.includes("/deploy/ci/contract-e2e.gitlab-ci.yml"),
    "Root GitLab config must include the contract/e2e fragment.",
  );
  const contractE2e = YAML.parse(
    await fs.readFile(path.join(repositoryRoot, "deploy/ci/contract-e2e.gitlab-ci.yml"), "utf8"),
  );
  // W-0038 / P5-4 §7. Same treatment for the review gate: written, included, fails closed.
  assert(
    includes.includes("/deploy/ci/quality-gate.gitlab-ci.yml"),
    "Root GitLab config must include the quality-gate fragment.",
  );
  const qualityGate = YAML.parse(
    await fs.readFile(path.join(repositoryRoot, "deploy/ci/quality-gate.gitlab-ci.yml"), "utf8"),
  );
  for (const jobName of ["review_gate_selftest", "mr_traceability_gate", "compliance_pack_selftest",
    "gate_status_mirror", "capacity_selftest"]) {
    assert(qualityGate[jobName], `Rendered quality gate is missing ${jobName}.`);
    assert(qualityGate[jobName].allow_failure === false, `${jobName} must fail closed.`);
  }

  // W-0039 / P5-5 §7. The console QA job, same treatment.
  assert(
    includes.includes("/deploy/ci/ui-qa.gitlab-ci.yml"),
    "Root GitLab config must include the UI QA fragment.",
  );
  const uiQa = YAML.parse(
    await fs.readFile(path.join(repositoryRoot, "deploy/ci/ui-qa.gitlab-ci.yml"), "utf8"),
  );
  assert(uiQa.ui_qa, "Rendered UI QA pipeline is missing ui_qa.");
  assert(uiQa.ui_qa.allow_failure === false, "ui_qa must fail closed.");

  // W-0041 / P6-2 section 7. The observability fragment, same treatment. This one matters more
  // than most: its whole job is to catch dashboards and alerts that drifted away from the
  // instrumentation, so a gate that runs nowhere leaves exactly the failure it was built for.
  assert(
    includes.includes("/deploy/ci/observability.gitlab-ci.yml"),
    "Root GitLab config must include the observability fragment.",
  );
  const observability = YAML.parse(
    await fs.readFile(path.join(repositoryRoot, "deploy/ci/observability.gitlab-ci.yml"), "utf8"),
  );
  for (const jobName of ["observability_rules", "observability_contract"]) {
    assert(observability[jobName], `Rendered observability pipeline is missing ${jobName}.`);
    assert(observability[jobName].allow_failure === false, `${jobName} must fail closed.`);
  }
  // promtool is the rule evaluator, not the server: the image entrypoint has to be blanked or the
  // job starts Prometheus and waits forever instead of running the checks.
  assert(
    Array.isArray(observability.observability_rules.image?.entrypoint)
      && observability.observability_rules.image.entrypoint.length === 1
      && observability.observability_rules.image.entrypoint[0] === "",
    "observability_rules must blank the Prometheus image entrypoint.",
  );

  // W-0042 / P6-3 section 7. The chaos fragment, same treatment. A resilience gate that runs
  // nowhere leaves the system unproven under exactly the faults it will meet.
  assert(
    includes.includes("/deploy/ci/chaos.gitlab-ci.yml"),
    "Root GitLab config must include the chaos fragment.",
  );
  const chaos = YAML.parse(
    await fs.readFile(path.join(repositoryRoot, "deploy/ci/chaos.gitlab-ci.yml"), "utf8"),
  );
  assert(chaos.chaos_suite, "Rendered chaos pipeline is missing chaos_suite.");
  assert(chaos.chaos_suite.allow_failure === false, "chaos_suite must fail closed.");
  // The scenarios create their own containers, so the job needs a Docker daemon; without it every
  // scenario errors on setup and the suite reads as broken tooling rather than as a fault found.
  assert(
    (chaos.chaos_suite.services ?? []).some((service) =>
      String(service.name ?? service).includes("dind")),
    "chaos_suite must provide a Docker daemon for the fault-injection containers.",
  );

  // W-0043 / P7-1 section 8. The image fragment, same treatment. This gate is the only thing that
  // looks at the published artifact rather than at the code inside it.
  assert(
    includes.includes("/deploy/ci/images.gitlab-ci.yml"),
    "Root GitLab config must include the images fragment.",
  );
  const images = YAML.parse(
    await fs.readFile(path.join(repositoryRoot, "deploy/ci/images.gitlab-ci.yml"), "utf8"),
  );
  assert(images.image_selftest, "Rendered images pipeline is missing image_selftest.");
  assert(images.image_selftest.allow_failure === false, "image_selftest must fail closed.");

  // W-0044 / P7-2 section 8. The chart gate, same treatment.
  assert(
    includes.includes("/deploy/ci/k8s.gitlab-ci.yml"),
    "Root GitLab config must include the k8s fragment.",
  );
  const k8s = YAML.parse(
    await fs.readFile(path.join(repositoryRoot, "deploy/ci/k8s.gitlab-ci.yml"), "utf8"),
  );
  assert(k8s.k8s_selftest, "Rendered k8s pipeline is missing k8s_selftest.");
  assert(k8s.k8s_selftest.allow_failure === false, "k8s_selftest must fail closed.");

  // W-0053 / P10-2 section 8. The DR drills, same treatment. This fragment is the one whose
  // absence would be hardest to notice: nothing else in the pipeline touches backup, restore or
  // promotion, so a DR job that runs nowhere leaves those three claims resting entirely on prose.
  assert(
    includes.includes("/deploy/ci/dr.gitlab-ci.yml"),
    "Root GitLab config must include the dr fragment.",
  );
  const dr = YAML.parse(
    await fs.readFile(path.join(repositoryRoot, "deploy/ci/dr.gitlab-ci.yml"), "utf8"),
  );
  assert(dr.dr_selftest, "Rendered DR pipeline is missing dr_selftest.");
  assert(dr.dr_selftest.allow_failure === false, "dr_selftest must fail closed.");
  assert(
    (dr.dr_selftest.script ?? []).some((line) => String(line).includes("dr-selftest.mjs")),
    "dr_selftest must actually run the drills rather than only declaring them.",
  );

  // W-0045 / P7-3 section 8. The delivery pipeline and the gate that checks it.
  for (const fragment of ["cd.gitlab-ci.yml", "promote.gitlab-ci.yml"]) {
    assert(
      includes.includes(`/deploy/ci/${fragment}`),
      `Root GitLab config must include the ${fragment} fragment.`,
    );
  }
  // deploy and promote are separate stages: a promotion a human presses must not be schedulable in
  // the same stage as the automatic deploys it comes after.
  for (const stage of ["deploy", "promote"]) {
    assert((root.stages ?? []).includes(stage), `Root GitLab config must expose the ${stage} stage.`);
  }
  const cd = YAML.parse(
    await fs.readFile(path.join(repositoryRoot, "deploy/ci/cd.gitlab-ci.yml"), "utf8"),
    { merge: true },
  );
  assert(cd.cd_selftest, "Rendered CD pipeline is missing cd_selftest.");
  assert(cd.cd_selftest.allow_failure === false, "cd_selftest must fail closed.");
  assert(cd.progressive_selftest, "Rendered CD pipeline is missing progressive_selftest.");
  assert(cd.progressive_selftest.allow_failure === false, "progressive_selftest must fail closed.");

  for (const jobName of ["contract_suite", "e2e_flow_suite"]) {
    assert(contractE2e[jobName], `Rendered contract/e2e pipeline is missing ${jobName}.`);
    assert(contractE2e[jobName].allow_failure === false, `${jobName} must fail closed.`);
    assert(
      contractE2e[jobName].variables?.REAL_CUSTOMER_CALL_ALLOWED === "NO",
      `${jobName} must pin REAL_CUSTOMER_CALL_ALLOWED=NO.`,
    );
    assert(
      contractE2e[jobName].variables?.IVR_ADAPTER_MODE === "MOCK",
      `${jobName} must pin IVR_ADAPTER_MODE=MOCK.`,
    );
  }
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
