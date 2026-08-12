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
const currentCompatSchemaPath = path.join(
  repositoryRoot,
  "specs/api/compat/current-golden-hour-callback.a3aad246.schema.json",
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
const exactMatrix = new Set(
  seed.tasks.map(
    (task) => `${task.body.program_code}+${task.body.payment_method_snapshot}`,
  ),
);
assertSetEquals(
  exactMatrix,
  new Set(["GOLDEN_HOUR+ONLINE", "TWENTY_FOUR_SEVEN+COD"]),
  "Target task fixture matrix",
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

const requiredSchemaNegativeIds = [
  "NEG-SCHEMA-FLAG-01",
  "NEG-SCHEMA-SPEECH-01",
  "NEG-SCHEMA-ORDER-VERSION-01",
  "NEG-SCHEMA-POLICY-VERSION-01",
  "NEG-SCHEMA-DIAL-TOKEN-01",
];
const schemaNegativeIds = new Set(seed.schema_negative.map((fixture) => fixture.id));
for (const fixtureId of requiredSchemaNegativeIds) {
  if (!schemaNegativeIds.has(fixtureId)) {
    throw new Error(`Required schema-negative fixture is missing: ${fixtureId}.`);
  }
}

const currentCompatSchema = JSON.parse(
  await fs.readFile(currentCompatSchemaPath, "utf8"),
);
const validateCurrentCompat = ajv.compile(currentCompatSchema);
const currentCompatFixture = {
  callId: 1001,
  reservationId: 2001,
  orderId: 3001,
  customerId: 4001,
  result: "CONFIRMED",
  occurredAt: "2026-08-12T08:30:00Z",
  idempotencyKey: "current-compat-0001",
};
assertValidation(
  validateCurrentCompat,
  currentCompatFixture,
  true,
  "current-compat:verified-shape",
);
assertValidation(
  validateCurrentCompat,
  { ...currentCompatFixture, order_version_seen_by_ivr: "17" },
  false,
  "current-compat:target-field-rejected",
);

const summary = [
  `OPENAPI_FILES_VALID=${yamlFiles.length}`,
  `TARGET_TASKS_SCHEMA_VALID=${seed.tasks.length}`,
  `SCHEMA_NEGATIVE_REJECTED=${seed.schema_negative.length}`,
  `DOMAIN_NEGATIVE_SCHEMA_VALID=${seed.domain_negative.length}`,
  `CURRENT_COMPAT_SCHEMA_VALID=1`,
  `CURRENT_COMPAT_TARGET_FIELD_REJECTED=1`,
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

function assertSetEquals(actual, expected, label) {
  if (actual.size !== expected.size
      || [...actual].some((value) => !expected.has(value))) {
    throw new Error(
      `${label} was ${JSON.stringify([...actual])}; expected ${JSON.stringify([...expected])}.`,
    );
  }
}
