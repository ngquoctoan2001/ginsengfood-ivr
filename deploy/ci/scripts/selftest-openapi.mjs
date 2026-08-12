import path from "node:path";
import { fileURLToPath } from "node:url";
import SwaggerParser from "@apidevtools/swagger-parser";

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const invalidFixture = path.resolve(
  scriptDirectory,
  "../fixtures/openapi/invalid-openapi.yaml",
);

let rejected = false;

try {
  await SwaggerParser.validate(invalidFixture);
} catch {
  rejected = true;
}

if (!rejected) {
  throw new Error("CT-CI-01 failed: invalid OpenAPI unexpectedly validated.");
}

process.stdout.write("CT-CI-01 PASS — invalid OpenAPI rejected\n");
