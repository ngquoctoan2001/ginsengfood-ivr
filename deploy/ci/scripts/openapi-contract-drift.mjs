import crypto from "node:crypto";
import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import YAML from "yaml";

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const repositoryRoot = path.resolve(scriptDirectory, "../../..");
const manifestPath = path.join(
  repositoryRoot,
  "specs/api/openapi/contract-manifest.json",
);
const reportPath = path.join(
  repositoryRoot,
  "docs/contracts/openapi-contract-diff.md",
);
const acceptDraft = process.argv.includes("--accept-reviewed-draft");
const manifest = JSON.parse(await fs.readFile(manifestPath, "utf8"));

const descriptions = [];
for (const contract of manifest.contracts) {
  const absolutePath = path.join(repositoryRoot, contract.path);
  const content = await fs.readFile(absolutePath);
  const hash = crypto.createHash("sha256").update(content).digest("hex");

  if (!acceptDraft && hash !== contract.sha256) {
    throw new Error(
      `CONTRACT_DRIFT: ${contract.path} is ${hash}, pinned ${contract.sha256}. ` +
        "Review the human diff; do not auto-accept upstream changes.",
    );
  }

  if (acceptDraft) {
    contract.sha256 = hash;
  }

  descriptions.push(describeContract(contract, content.toString("utf8"), hash));
}

const report = renderReport(manifest, descriptions);
if (acceptDraft) {
  await fs.writeFile(manifestPath, `${JSON.stringify(manifest, null, 2)}\n`, "utf8");
  await fs.mkdir(path.dirname(reportPath), { recursive: true });
  await fs.writeFile(reportPath, report, "utf8");
  process.stdout.write("OPENAPI_REVIEWED_DRAFT_BASELINE_UPDATED=YES\n");
} else {
  const committedReport = await fs.readFile(reportPath, "utf8");
  if (committedReport !== report) {
    throw new Error(
      "CONTRACT_REPORT_DRIFT: regenerate only after reviewing the contract change.",
    );
  }

  process.stdout.write(`OPENAPI_HASHES_PINNED=${descriptions.length}\n`);
  process.stdout.write("OPENAPI_HUMAN_DIFF_CURRENT=YES\n");
}

function describeContract(contract, source, hash) {
  if (contract.path.endsWith(".json")) {
    const schema = JSON.parse(source);
    return {
      ...contract,
      hash,
      title: schema.title,
      version: contract.salesCurrentBaseline ?? "pinned-source",
      operations: [schema["x-current-endpoint"]],
      schemas: [schema.title, ...Object.keys(schema.$defs ?? {})],
      required: schema.required ?? [],
      notes: [
        `auth=${schema["x-current-auth"]}`,
        `unsupported Target fields=${(schema["x-unsupported-target-fields"] ?? []).join(", ")}`,
      ],
    };
  }

  const api = YAML.parse(source);
  const operations = [];
  for (const [route, pathItem] of Object.entries(api.paths ?? {})) {
    for (const method of ["get", "post", "put", "patch", "delete"]) {
      if (pathItem[method]) {
        operations.push(`${method.toUpperCase()} ${route} (${pathItem[method].operationId})`);
      }
    }
  }

  const schemas = Object.keys(api.components?.schemas ?? {}).sort();
  const task = api.components?.schemas?.IvrConfirmationTaskV1;
  const callback = api.components?.schemas?.IvrResultCallbackV1;
  const required = task?.required ?? callback?.required ?? [];

  return {
    ...contract,
    hash,
    title: api.info.title,
    version: api.info.version,
    operations,
    schemas,
    required,
    notes: [
      `servers=${(api.servers ?? []).map((server) => server.url).join(", ")}`,
      `security=${Object.keys(api.components?.securitySchemes ?? {}).join(", ")}`,
    ],
  };
}

function renderReport(currentManifest, contracts) {
  const lines = [
    "# OpenAPI Contract Baseline and Human-Readable Diff",
    "",
    `Contract state: \`${currentManifest.contractState}\`.`,
    "",
    "> Generated deterministically from the pinned manifest. A changed hash or report fails CI. Regeneration never upgrades DRAFT or accepts upstream drift by itself.",
    "",
    `Generator: \`${currentManifest.codeGenerator.name} ${currentManifest.codeGenerator.version}\`.`,
    `Verified current Sales source: \`${currentManifest.salesCurrentBaseline.repository}@${currentManifest.salesCurrentBaseline.commit}\`.`,
    "",
  ];

  for (const contract of contracts) {
    lines.push(`## ${contract.id}`, "");
    lines.push(`- Role/status: ${contract.role} / \`${contract.status}\``);
    lines.push(`- Source: \`${contract.path}\``);
    lines.push(`- SHA-256: \`${contract.hash}\``);
    lines.push(`- Title/version: ${contract.title} / \`${contract.version}\``);
    lines.push(`- Generated: ${contract.generated ? `\`${contract.generated}\`` : "hand-written isolated compatibility DTO"}`);
    lines.push(`- Operations (${contract.operations.length}): ${contract.operations.join("; ") || "schema-only"}`);
    lines.push(`- Schemas (${contract.schemas.length}): ${contract.schemas.join(", ")}`);
    lines.push(`- Primary required fields (${contract.required.length}): ${contract.required.join(", ")}`);
    lines.push(`- Notes: ${contract.notes.join("; ")}`, "");
  }

  lines.push(
    "## Review boundary",
    "",
    "- Target and current DTOs remain unrelated types and use different clients.",
    "- `https://sales-platform.invalid` and `https://identity.invalid` are deliberate placeholders, never production configuration.",
    "- Any required-field, enum, path, auth, ACK, or privacy change needs human review and a new pinned hash; the command requires the explicit `--accept-reviewed-draft` flag.",
    "- Real Sales approval, sandbox, auth and CDC evidence remain `BLOCKED_EXTERNAL`.",
  );

  return `${lines.join("\n")}\n`;
}
