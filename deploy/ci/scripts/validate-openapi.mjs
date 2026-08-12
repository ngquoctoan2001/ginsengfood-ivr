import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import SwaggerParser from "@apidevtools/swagger-parser";
import Ajv2020 from "ajv/dist/2020.js";
import addFormats from "ajv-formats";

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const repositoryRoot = path.resolve(scriptDirectory, "../../..");
const openApiDirectory = path.join(repositoryRoot, "specs/api/openapi");
const targetContractPath = path.join(
  openApiDirectory,
  "ivr-order-confirmation.v1.yaml",
);
const seedPath = path.join(repositoryRoot, "seed/sales-target-v1.sample.json");

const yamlFiles = (await fs.readdir(openApiDirectory))
  .filter((fileName) => fileName.endsWith(".yaml"))
  .sort();

if (yamlFiles.length === 0) {
  throw new Error("No OpenAPI YAML files were found.");
}

for (const fileName of yamlFiles) {
  await SwaggerParser.validate(path.join(openApiDirectory, fileName));
  process.stdout.write(`OPENAPI_PARSE_PASS=${fileName}\n`);
}

const dereferencedApi = await SwaggerParser.dereference(targetContractPath);
const taskSchema = structuredClone(
  dereferencedApi.components.schemas.IvrConfirmationTaskV1,
);
const ajv = new Ajv2020({ allErrors: true, strict: false });
addFormats(ajv);
const validateTask = ajv.compile(taskSchema);
const seed = JSON.parse(await fs.readFile(seedPath, "utf8"));
const taskByScenario = new Map(
  seed.tasks.map((task) => [task.scenario, task.body]),
);

for (const task of seed.tasks) {
  assertValidation(validateTask, task.body, true, `task:${task.scenario}`);
}

for (const fixture of seed.schema_negative) {
  const candidate = buildFixture(taskByScenario, fixture);
  assertValidation(validateTask, candidate, false, fixture.id);
}

for (const fixture of seed.domain_negative) {
  const candidate = buildFixture(taskByScenario, fixture);
  assertValidation(validateTask, candidate, true, fixture.id);
}

const summary = [
  `OPENAPI_FILES_VALID=${yamlFiles.length}`,
  `TARGET_TASKS_SCHEMA_VALID=${seed.tasks.length}`,
  `SCHEMA_NEGATIVE_REJECTED=${seed.schema_negative.length}`,
  `DOMAIN_NEGATIVE_SCHEMA_VALID=${seed.domain_negative.length}`,
].join("\n");

process.stdout.write(`${summary}\n`);

const summaryArgumentIndex = process.argv.indexOf("--summary");
if (summaryArgumentIndex >= 0) {
  const outputArgument = process.argv[summaryArgumentIndex + 1];
  if (!outputArgument) {
    throw new Error("--summary requires an output path.");
  }

  const outputPath = path.resolve(repositoryRoot, outputArgument);
  await fs.mkdir(path.dirname(outputPath), { recursive: true });
  await fs.writeFile(outputPath, `${summary}\n`, "utf8");
}

function buildFixture(taskMap, fixture) {
  const sourceBody = taskMap.get(fixture.from);
  if (!sourceBody) {
    throw new Error(`${fixture.id} references unknown scenario ${fixture.from}.`);
  }

  const candidate = structuredClone(sourceBody);

  for (const [propertyPath, value] of Object.entries(fixture.replace ?? {})) {
    setPath(candidate, propertyPath, value);
  }

  for (const [propertyPath, value] of Object.entries(fixture.add ?? {})) {
    setPath(candidate, propertyPath, value);
  }

  if (fixture.remove) {
    deletePath(candidate, fixture.remove);
  }

  return candidate;
}

function setPath(target, propertyPath, value) {
  const segments = propertyPath.split(".");
  const leaf = segments.pop();
  let cursor = target;

  for (const segment of segments) {
    cursor[segment] ??= {};
    cursor = cursor[segment];
  }

  cursor[leaf] = value;
}

function deletePath(target, propertyPath) {
  const segments = propertyPath.split(".");
  const leaf = segments.pop();
  let cursor = target;

  for (const segment of segments) {
    cursor = cursor?.[segment];
    if (cursor === undefined) {
      return;
    }
  }

  delete cursor[leaf];
}

function assertValidation(validator, value, expected, testId) {
  const actual = validator(value);
  if (actual !== expected) {
    const detail = JSON.stringify(validator.errors ?? [], null, 2);
    throw new Error(
      `${testId} schema result was ${actual}; expected ${expected}. ${detail}`,
    );
  }
}
