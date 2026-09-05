// W-0197: inventory is generated from the canonical contract, never a hand-written route list.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import YAML from 'yaml';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../..');
const source = 'specs/api/openapi/ivr-order-confirmation.v1.yaml';
const bytes = fs.readFileSync(path.join(root, source));
const document = YAML.parse(bytes.toString('utf8'));
const resolve = value => value?.$ref
  ? value.$ref.slice(2).split('/').reduce((node, part) => node[part], document) : value;
const operations = Object.entries(document.paths).flatMap(([route, item]) =>
  Object.entries(item).filter(([method, op]) => /^(get|post|put|patch|delete)$/.test(method) && op.operationId)
    .map(([method, op]) => ({
      id: op.operationId, method: method.toUpperCase(), path: route,
      parameters: [...(item.parameters ?? []), ...(op.parameters ?? [])].map(resolve),
      requestSchema: resolve(op.requestBody)?.content?.['application/json']?.schema ?? null,
      responses: Object.fromEntries(Object.entries(op.responses).map(([code, response]) => [code, resolve(response)])),
    })));
if (operations.length !== 38 || new Set(operations.map(op => op.id)).size !== 38)
  throw new Error(`Expected 38 unique operations, found ${operations.length}; reconcile matrix fixtures.`);
const inventory = { source, sha256: crypto.createHash('sha256').update(bytes).digest('hex'),
  version: document.info.version, prefix: '/v1/ivr/order-confirmation',
  result_codes: document.components.schemas.ResultType.enum, operations };
const files = [...new Set(execFileSync('git', ['ls-files', '--cached', '--others', '--exclude-standard',
  'src', 'tests', 'deploy/ci', 'specs/api', 'seed', 'Directory.Build.props', 'Directory.Build.targets',
  'Directory.Packages.props', 'global.json'], { cwd: root, encoding: 'utf8' }).trim().split(/\r?\n/))]
  .filter(file => /\.(cs|csproj|props|targets|json|yaml|yml|mjs|ps1)$/.test(file)).sort();
inventory.source_files = Object.fromEntries(files.map(file => [file,
  crypto.createHash('sha256').update(fs.readFileSync(path.join(root, file))).digest('hex')]));
inventory.source_sha256 = crypto.createHash('sha256').update(JSON.stringify(inventory.source_files)).digest('hex');
inventory.base_commit = execFileSync('git', ['rev-parse', 'HEAD'], { cwd: root, encoding: 'utf8' }).trim();
process.stdout.write(JSON.stringify(inventory));
