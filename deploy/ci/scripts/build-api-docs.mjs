import crypto from "node:crypto";
import fs from "node:fs/promises";
import path from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath, pathToFileURL } from "node:url";
import { marked } from "marked";
import YAML from "yaml";

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
export const repositoryRoot = path.resolve(scriptDirectory, "../../..");
export const defaultOutputDirectory = path.join(repositoryRoot, "docs/api");
export const generatedPortalFiles = [
  "api-changelog.html",
  "api-versioning.html",
  "current-golden-hour-compat.html",
  "index.html",
  "integration-guide.html",
  "ivr-order-confirmation-v1.0.0-to-v1.0.0-draft.2.html",
  "ivr-order-confirmation-v1.0.0-draft.2-to-v1.0.0-draft.20.html",
  "ivr-order-confirmation-v1.0.0-draft.20-to-v1.0.0-draft.22.html",
  "ivr-order-confirmation-v1.0.0-draft.22-to-v1.0.0-draft.23.html",
  "ivr-order-confirmation-v1.html",
  "ivr-order-confirmation-changelog.html",
  "order-core-ivr-callback-changelog.html",
  "order-core-ivr-callback-target-v1.html",
  "portal-manifest.json",
  "styles.css",
];

const referenceDefinitions = [
  {
    source: "specs/api/openapi/ivr-order-confirmation.v1.yaml",
    output: "ivr-order-confirmation-v1.html",
    title: "IVR Order Confirmation — Target V1 Draft",
  },
  {
    source: "specs/api/openapi/order-core-ivr-callback.target-v1.yaml",
    output: "order-core-ivr-callback-target-v1.html",
    title: "Sales Order Core Callback — Target V1 Draft",
  },
];

const markdownDefinitions = [
  { source: "docs/integration-guide.md", output: "integration-guide.html", title: "Integration Guide" },
  { source: "docs/api-versioning.md", output: "api-versioning.html", title: "Versioning and Deprecation" },
  {
    source: "docs/api-changelog.md",
    output: "api-changelog.html",
    title: "Contract Changelog",
    linkRewrites: {
      "api-versioning.md": "api-versioning.html",
      "api/changelog/ivr-order-confirmation.md": "ivr-order-confirmation-changelog.html",
      "api/changelog/order-core-ivr-callback.md": "order-core-ivr-callback-changelog.html",
      "api/changelog/ivr-order-confirmation.v1.0.0-to-v1.0.0-draft.2.md":
        "ivr-order-confirmation-v1.0.0-to-v1.0.0-draft.2.html",
      "api/changelog/ivr-order-confirmation.v1.0.0-draft.2-to-v1.0.0-draft.20.md":
        "ivr-order-confirmation-v1.0.0-draft.2-to-v1.0.0-draft.20.html",
      "api/changelog/ivr-order-confirmation.v1.0.0-draft.20-to-v1.0.0-draft.22.md":
        "ivr-order-confirmation-v1.0.0-draft.20-to-v1.0.0-draft.22.html",
      "api/changelog/ivr-order-confirmation.v1.0.0-draft.22-to-v1.0.0-draft.23.md":
        "ivr-order-confirmation-v1.0.0-draft.22-to-v1.0.0-draft.23.html",
    },
  },
  {
    source: "docs/api/changelog/ivr-order-confirmation.v1.0.0-to-v1.0.0-draft.2.md",
    output: "ivr-order-confirmation-v1.0.0-to-v1.0.0-draft.2.html",
    title: "Archived IVR Contract Transition",
  },
  // W-0124 F2. The draft.2 comparison window is closed, not deleted. OD-17 removed
  // `sellable_status` with owner approval, which is a breaking change the cumulative gate is
  // supposed to report — so leaving draft.2 as the live baseline left `--fail-on WARN` red on
  // every pipeline for a change that was already signed off, and a gate that always fails stops
  // being read. Rotating the baseline restores the signal; keeping this frozen report is what
  // stops the rotation from erasing the removal it was rotated past.
  {
    source: "docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.2-to-v1.0.0-draft.20.md",
    output: "ivr-order-confirmation-v1.0.0-draft.2-to-v1.0.0-draft.20.html",
    title: "Archived IVR Contract Transition (draft.2 → draft.20)",
  },
  // W-0128. Second rotation, same reason as the one above. Deleting the account system removed
  // eleven paths with owner approval, which the cumulative gate reports as eleven warnings --
  // enough to hold `--fail-on WARN` red on every pipeline for a decision already made. Rotating
  // to draft.22 restores the signal; freezing this report is what stops the rotation from
  // erasing the removals it was rotated past.
  {
    source: "docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.20-to-v1.0.0-draft.22.md",
    output: "ivr-order-confirmation-v1.0.0-draft.20-to-v1.0.0-draft.22.html",
    title: "Archived IVR Contract Transition (draft.20 → draft.22)",
  },
  // W-0202. Third rotation, and the first one that archives a tightening rather than a
  // removal. draft.23 constrained x-correlation-id across many operations, which oasdiff reports
  // as eleven warnings — enough to hold `--fail-on WARN` red on every pipeline for a change the
  // owner approved. Rotating restores the signal; freezing this report is what stops the rotation
  // from erasing the constraint it was rotated past.
  {
    source: "docs/api/changelog/ivr-order-confirmation.v1.0.0-draft.22-to-v1.0.0-draft.23.md",
    output: "ivr-order-confirmation-v1.0.0-draft.22-to-v1.0.0-draft.23.html",
    title: "Archived IVR Contract Transition (draft.22 → draft.23)",
  },
  {
    source: "docs/api/changelog/ivr-order-confirmation.md",
    output: "ivr-order-confirmation-changelog.html",
    title: "IVR API Changelog",
  },
  {
    source: "docs/api/changelog/order-core-ivr-callback.md",
    output: "order-core-ivr-callback-changelog.html",
    title: "Sales Callback Changelog",
  },
];

export async function buildApiDocs(outputDirectory = defaultOutputDirectory) {
  await fs.mkdir(outputDirectory, { recursive: true });

  for (const reference of referenceDefinitions) {
    await runRedoc(reference, outputDirectory);
  }

  for (const document of markdownDefinitions) {
    const markdown = await fs.readFile(path.join(repositoryRoot, document.source), "utf8");
    let body = marked.parse(markdown);
    for (const [sourceLink, portalLink] of Object.entries(document.linkRewrites ?? {})) {
      body = body.replaceAll(`href="${sourceLink}"`, `href="${portalLink}"`);
    }
    const html = renderPage(document.title, body, document.source);
    await fs.writeFile(path.join(outputDirectory, document.output), html, "utf8");
  }

  const contractManifest = JSON.parse(
    await fs.readFile(path.join(repositoryRoot, "specs/api/openapi/contract-manifest.json"), "utf8"),
  );
  const compat = JSON.parse(
    await fs.readFile(
      path.join(repositoryRoot, "specs/api/compat/current-golden-hour-callback.a3aad246.schema.json"),
      "utf8",
    ),
  );
  const apis = await Promise.all(
    referenceDefinitions.map(async (reference) => ({
      ...reference,
      api: YAML.parse(await fs.readFile(path.join(repositoryRoot, reference.source), "utf8")),
    })),
  );

  await fs.writeFile(
    path.join(outputDirectory, "current-golden-hour-compat.html"),
    renderCurrentCompatibility(compat, contractManifest),
    "utf8",
  );
  await fs.writeFile(
    path.join(outputDirectory, "index.html"),
    renderIndex(apis, contractManifest),
    "utf8",
  );
  await fs.writeFile(path.join(outputDirectory, "styles.css"), portalStyles, "utf8");

  const sourcePaths = [
    ...referenceDefinitions.map((reference) => reference.source),
    "specs/api/compat/current-golden-hour-callback.a3aad246.schema.json",
    "specs/api/openapi/contract-manifest.json",
    "specs/api/openapi/changelog-baseline.json",
    ...markdownDefinitions.map((document) => document.source),
    "docs/api/changelog/ivr-order-confirmation.md",
    "docs/api/changelog/order-core-ivr-callback.md",
  ];
  const sourceHashes = Object.fromEntries(
    await Promise.all(
      sourcePaths.sort().map(async (sourcePath) => [
        sourcePath,
        crypto
          .createHash("sha256")
          .update(await fs.readFile(path.join(repositoryRoot, sourcePath)))
          .digest("hex"),
      ]),
    ),
  );
  const portalManifest = {
    manifestVersion: 1,
    portalEnvironment: "NON_PRODUCTION_ONLY",
    contractState: contractManifest.contractState,
    renderer: "@redocly/cli 2.46.1",
    markdownRenderer: "marked 18.0.9",
    generatedFiles: generatedPortalFiles,
    sourceHashes,
  };
  await fs.writeFile(
    path.join(outputDirectory, "portal-manifest.json"),
    `${JSON.stringify(portalManifest, null, 2)}\n`,
    "utf8",
  );

  process.stdout.write(`API_DOCS_GENERATED=${generatedPortalFiles.length}\n`);
  process.stdout.write(`API_DOCS_OUTPUT=${path.relative(repositoryRoot, outputDirectory)}\n`);
}

async function runRedoc(reference, outputDirectory) {
  const cliPath = path.join(repositoryRoot, "deploy/ci/node_modules/@redocly/cli/bin/cli.js");
  const result = spawnSync(
    process.execPath,
    [
      cliPath,
      "build-docs",
      path.join(repositoryRoot, reference.source),
      "--output",
      path.join(outputDirectory, reference.output),
      "--title",
      reference.title,
      "--disableGoogleFont",
      "--config",
      path.join(repositoryRoot, ".redocly.yaml"),
    ],
    { cwd: repositoryRoot, encoding: "utf8" },
  );

  if (result.status !== 0) {
    throw new Error(
      `REDOC_BUILD_FAILED ${reference.source}: ${result.stderr || result.stdout || result.error}`,
    );
  }

  const outputPath = path.join(outputDirectory, reference.output);
  const rendered = await fs.readFile(outputPath, "utf8");
  await fs.writeFile(
    outputPath,
    rendered.replace(/[ \t]+(?=\r?\n)/gu, ""),
    "utf8",
  );
}

function renderIndex(apis, contractManifest) {
  const cards = apis
    .map(({ api, output, source }) => {
      const operationCount = Object.values(api.paths ?? {}).reduce(
        (total, pathItem) =>
          total + ["get", "post", "put", "patch", "delete"].filter((method) => pathItem[method]).length,
        0,
      );
      return `<article class="card target">
        <div class="badge">TARGET_DRAFT</div>
        <h2>${escapeHtml(api.info.title)}</h2>
        <p>Version <code>${escapeHtml(api.info.version)}</code> · ${operationCount} operation(s)</p>
        <p class="source">Generated from <code>${escapeHtml(source)}</code></p>
        <a class="button" href="${output}">Open generated reference</a>
      </article>`;
    })
    .join("\n");

  return `<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Ginsengfood IVR Developer Portal</title><link rel="stylesheet" href="styles.css"></head>
<body><main class="container">
  <header class="hero"><div class="badge warning">NON-PRODUCTION ONLY</div>
    <h1>Ginsengfood IVR Developer Portal</h1>
    <p>Generated contract reference for Order Core, Ops, CRM and IVR developers.</p>
    <p><strong>${escapeHtml(contractManifest.contractState)}</strong>. Rendering does not approve or enable a provider.</p>
  </header>
  <section class="grid">${cards}
    <article class="card compat"><div class="badge compat-badge">CURRENT COMPAT</div>
      <h2>Golden Hour callback at pinned Sales SHA</h2>
      <p>Separate compatibility contract. Golden Hour only; no Target semantic ACK.</p>
      <a class="button secondary" href="current-golden-hour-compat.html">Open compatibility reference</a>
    </article>
  </section>
  <section class="links"><h2>Integration and lifecycle</h2><ul>
    <li><a href="integration-guide.html">Order Core / Ops / CRM integration guide</a></li>
    <li><a href="api-versioning.html">Versioning, deprecation and Sunset policy</a></li>
    <li><a href="api-changelog.html">Generated contract changelog index</a></li>
  </ul></section>
  <footer>No real phone, full address, token, credential or production endpoint belongs in this portal.</footer>
</main></body></html>\n`;
}

function renderCurrentCompatibility(schema, contractManifest) {
  const properties = Object.entries(schema.properties ?? {})
    .map(([name, definition]) => {
      const type = Array.isArray(definition.type) ? definition.type.join(" | ") : definition.type;
      const required = (schema.required ?? []).includes(name) ? "yes" : "no";
      const values = definition.enum ? `<br><small>${definition.enum.map(escapeHtml).join(", ")}</small>` : "";
      return `<tr><td><code>${escapeHtml(name)}</code></td><td>${escapeHtml(type ?? "schema")}${values}</td><td>${required}</td></tr>`;
    })
    .join("\n");
  const unsupported = (schema["x-unsupported-target-fields"] ?? [])
    .map((field) => `<li><code>${escapeHtml(field)}</code></li>`)
    .join("");

  return renderPage(
    "Current Golden Hour compatibility",
    `<div class="badge compat-badge">CURRENT_COMPAT_VERIFIED_AT_PINNED_SHA</div>
    <p><strong>This is not Target V1.</strong> It is verified against
      <code>${escapeHtml(contractManifest.salesCurrentBaseline.repository)}@${escapeHtml(contractManifest.salesCurrentBaseline.commit)}</code>.</p>
    <h2>Wire contract</h2>
    <p><code>${escapeHtml(schema["x-current-endpoint"])}</code></p>
    <p>Authentication: <code>${escapeHtml(schema["x-current-auth"])}</code></p>
    <p>Success envelope: <code>${escapeHtml(schema["x-current-success-envelope"])}</code></p>
    <table><thead><tr><th>Field</th><th>Type / values</th><th>Required</th></tr></thead><tbody>${properties}</tbody></table>
    <h2>Unsupported Target fields</h2><ul class="columns">${unsupported}</ul>
    <div class="notice">Compatibility is Golden Hour only, runtime-disabled by default and has no approved sunset date.</div>`,
    "specs/api/compat/current-golden-hour-callback.a3aad246.schema.json",
  );
}

function renderPage(title, body, source) {
  return `<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>${escapeHtml(title)} · Ginsengfood IVR</title><link rel="stylesheet" href="styles.css"></head>
<body><main class="container document"><nav><a href="index.html">← Portal home</a></nav>
<header><div class="badge warning">NON-PRODUCTION ONLY</div><h1>${escapeHtml(title)}</h1>
<p class="source">Source: <code>${escapeHtml(source)}</code></p></header>${body}
<footer>Generated content. Edit the named source, not this HTML.</footer></main></body></html>\n`;
}

function escapeHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}

const portalStyles = `:root{color-scheme:light;--ink:#152119;--muted:#59655d;--line:#dbe4dd;--green:#176b3a;--soft:#f2f8f4;--amber:#8a4b00;--amber-bg:#fff4df;--blue:#1f4f8f}*{box-sizing:border-box}body{margin:0;background:#f7faf8;color:var(--ink);font:16px/1.6 system-ui,-apple-system,Segoe UI,sans-serif}.container{max-width:1120px;margin:auto;padding:40px 24px}.hero,.document>header{padding:32px;border:1px solid var(--line);border-radius:20px;background:white}.hero h1{font-size:clamp(2rem,5vw,3.4rem);line-height:1.05;margin:.35em 0}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(280px,1fr));gap:18px;margin:24px 0}.card,.links,.notice{border:1px solid var(--line);border-radius:16px;background:white;padding:24px}.card h2{line-height:1.25}.badge{display:inline-block;padding:4px 10px;border-radius:999px;background:var(--soft);color:var(--green);font-size:.75rem;font-weight:800;letter-spacing:.05em}.warning{background:var(--amber-bg);color:var(--amber)}.compat-badge{background:#eaf2ff;color:var(--blue)}.button{display:inline-block;margin-top:10px;padding:10px 14px;border-radius:10px;background:var(--green);color:white;text-decoration:none;font-weight:700}.secondary{background:var(--blue)}a{color:#125b34}.source,footer{color:var(--muted);font-size:.9rem}footer{margin-top:32px;border-top:1px solid var(--line);padding-top:18px}.document{max-width:900px;background:white}.document nav{margin-bottom:20px}.document h2{margin-top:2em}.document pre{padding:16px;overflow:auto;background:#102018;color:#eaf5ed;border-radius:10px}.document code{overflow-wrap:anywhere}.document table{width:100%;border-collapse:collapse;display:block;overflow:auto}.document th,.document td{padding:9px 12px;border:1px solid var(--line);text-align:left}.columns{columns:2}.notice{background:var(--amber-bg);margin:24px 0}@media(max-width:600px){.container{padding:20px 14px}.columns{columns:1}}`;

const isDirectExecution = process.argv[1] && pathToFileURL(path.resolve(process.argv[1])).href === import.meta.url;
if (isDirectExecution) {
  const outputFlag = process.argv.indexOf("--output");
  const outputDirectory = outputFlag >= 0 ? path.resolve(process.argv[outputFlag + 1]) : defaultOutputDirectory;
  await buildApiDocs(outputDirectory);
}
