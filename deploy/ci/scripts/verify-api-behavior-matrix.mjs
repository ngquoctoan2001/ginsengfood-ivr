import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import YAML from 'yaml';
import Ajv from 'ajv/dist/2020.js';
import addFormats from 'ajv-formats';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../..');
const input = path.resolve(root, process.argv[2] ?? '.artifacts/api-matrix/http-observations.json');
const output = path.resolve(root, process.argv[3] ?? '.artifacts/api-matrix/report.json');
const raw = fs.readFileSync(path.join(root, 'specs/api/openapi/ivr-order-confirmation.v1.yaml'));
const contract = YAML.parse(raw.toString('utf8'));
const report = JSON.parse(fs.readFileSync(input, 'utf8'));
const failures = [...report.behavior_failures];
const currentInventory = JSON.parse(execFileSync(process.execPath, ['deploy/ci/scripts/api-behavior-matrix.mjs'], { cwd: root, encoding: 'utf8', maxBuffer: 10 * 1024 * 1024 }));
if (currentInventory.source_sha256 !== report.inventory.source_sha256) failures.push('Source changed since the HTTP matrix ran; rerun the test');
const canonicalOperations = Object.entries(contract.paths).flatMap(([route, item]) =>
  Object.entries(item).filter(([, operation]) => operation?.operationId)
    .map(([method, operation]) => `${operation.operationId}|${method.toUpperCase()}|${route}`)).sort();
if (JSON.stringify(canonicalOperations) !== JSON.stringify(report.inventory.operations.map(op => `${op.id}|${op.method}|${op.path}`).sort()))
  failures.push('Inventory is not the complete unique OpenAPI operation set');
const expectedRoutes = report.inventory.operations.map(op => `${op.method} ${report.inventory.prefix}${op.path}`).sort();
if (JSON.stringify(expectedRoutes) !== JSON.stringify([...report.runtime_routes].sort())) failures.push('Runtime endpoint set differs from OpenAPI');
if (report.inventory.sha256 !== crypto.createHash('sha256').update(raw).digest('hex')) failures.push('Stale OpenAPI inventory');
const resolve = value => value?.$ref ? value.$ref.slice(2).split('/').reduce((node, part) => node[part], contract) : value;
const ajv = new Ajv({ strict: false, allErrors: true });
addFormats(ajv);
const validators = new Map();
const forbidden = new Set(['phone', 'phonenumber', 'rawphone', 'address', 'fulladdress', 'payment', 'paymentdetail', 'recording', 'dialtoken']);
function privacy(value, at = '$') {
  if (Array.isArray(value)) return value.flatMap((item, index) => privacy(item, `${at}[${index}]`));
  if (value && typeof value === 'object') return Object.entries(value).flatMap(([key, item]) =>
    [...(forbidden.has(key.replace(/[^a-z0-9]/gi, '').toLowerCase()) ? [`${at}.${key}: restricted field`] : []), ...privacy(item, `${at}.${key}`)]);
  if (typeof value === 'string' && (/(?:^|[^a-z0-9])(?:0\d{9}|\+?84\d{9})(?:$|[^a-z0-9])/i.test(value)
    || /(?:đường|số nhà|ngõ|hẻm|ngách)\s+/iu.test(value)
    || /(?:recording|payment)[_-]?(?:url|token|secret)\s*[:=]/i.test(value))) return [`${at}: restricted value`];
  return [];
}
// Every object with an enumerated property set is an allowlist, including schemas where
// additionalProperties was accidentally omitted. Explicit dictionary schemas remain dictionaries.
function closeObjects(value) {
  if (!value || typeof value !== 'object') return;
  if (value.properties && value.additionalProperties === undefined) value.additionalProperties = false;
  for (const child of Object.values(value)) closeObjects(child);
}
const components = structuredClone(contract.components);
closeObjects(components);
function validateResponse(op, observation) {
  const response = resolve(contract.paths[op.path][op.method.toLowerCase()].responses[String(observation.status)]);
  const schema = response?.content?.['application/json']?.schema;
  const errors = privacy(observation.response);
  if (!schema) errors.push(`Undocumented HTTP ${observation.status} JSON response`);
  else {
    const key = `${op.id}:${observation.status}`;
    if (!validators.has(key)) validators.set(key, ajv.compile({ components, ...structuredClone(schema) }));
    const validate = validators.get(key);
    if (!validate(observation.response)) errors.push(...validate.errors.map(error => `${error.instancePath || '$'} ${error.message} ${JSON.stringify(error.params)}`));
  }
  if (observation.content_type !== 'application/json' || !observation.response_is_json) errors.push('Response is not JSON');
  if (observation.status >= 400 && observation.response?.error?.correlationId !== observation.response_correlation_id)
    errors.push('Error-envelope correlation ID differs from the response header');
  return errors;
}
const operationReports = report.inventory.operations.map(op => {
  const cases = report.cases.filter(item => item.operation_id === op.id && !item.case.startsWith('setup_'));
  const problems = [];
  for (const required of ['happy', 'auth_missing', 'auth_invalid', 'auth_wrong_tier', 'scope_wrong', 'correlation_malformed', 'correlation_missing'])
    if (!cases.some(item => item.case === required && item.status !== undefined)) problems.push(`Missing executed ${required}`);
  if (!cases.some(item => item.case.startsWith('malformed_'))) problems.push('Missing malformed dimension');
  if (!cases.some(item => ['not_found', 'not_found_conflict', 'payload_conflict'].includes(item.case))) problems.push('Missing not-found/conflict dimension');
  if (op.method !== 'GET')
    for (const required of ['retry_same_key', 'replay_same_payload', 'payload_conflict'])
      if (!cases.some(item => item.case === required)) problems.push(`Missing ${required}`);
  for (const item of cases) {
    if (item.applicability === 'NOT_APPLICABLE') {
      if (!item.reason) problems.push(`${item.case}: unjustified N/A`);
      continue;
    }
    item.response_checks = validateResponse(op, item);
    if (item.case === 'happy' && item.happy_semantics_pass !== true) problems.push('happy: business outcome was not verified');
    if (['retry_same_key', 'replay_same_payload'].includes(item.case) && item.replay_equal !== true) problems.push(`${item.case}: no equal-response proof`);
    if (item.case === 'replay_same_payload' && item.persisted_state_unchanged !== true) problems.push('replay_same_payload: no unchanged-state proof');
    if (!item.status_pass || !item.correlation_pass || item.replay_equal === false
        || item.persisted_state_unchanged === false || item.happy_semantics_pass === false) problems.push(`${item.case}: behavioral assertion failed`);
    problems.push(...item.response_checks.map(error => `${item.case}: ${error}`));
  }
  return { operation_id: op.id, method: op.method, path: op.path,
    runtime_route_match: report.runtime_routes.includes(`${op.method} ${report.inventory.prefix}${op.path}`),
    executed: cases.filter(item => item.status !== undefined).length,
    not_applicable: cases.filter(item => item.applicability === 'NOT_APPLICABLE'),
    verdict: problems.length ? 'FAIL' : 'PASS', failures: problems, cases };
});
if (operationReports.length !== 38) failures.push('Expected 38 operation reports');
if (report.cases.some(item => !report.inventory.operations.some(op => op.id === item.operation_id))) failures.push('Unknown operation evidence');
const taxonomy = contract.components.schemas.ResultType.enum;
const codes = report.result_codes ?? [];
if (codes.length !== 11 || JSON.stringify(codes.map(code => code.wire).sort()) !== JSON.stringify([...taxonomy].sort()))
  failures.push('Missing exact 11-code wire evidence');
const runtime = codes.filter(code => code.runtime_result);
const preCall = codes.filter(code => !code.runtime_result);
if (runtime.length !== 9 || runtime.some(code => !code.in_openapi || !code.wire_matches || code.http_status !== 200))
  failures.push('Nine runtime wire codes were not verified over HTTP');
if (preCall.length !== 2 || preCall.some(code => !code.in_openapi || !code.call_result_construction_rejected
  || !code.pre_call_no_job_or_attempt || !code.pre_call_outcome_pass))
  failures.push('Pre-call blocked-code boundary was not verified');
// Keep hashes losslessly reconstructible while avoiding a run of ten digits being mistaken
// for a subscriber number by the repository's text-artifact scanner. HTTP bodies are untouched.
const groupedHash = value => value.match(/.{1,8}/g).join('-');
const result = { schema_version: 'ivr.api-behavior-matrix.report.v1', generated_at: report.generated_at,
  source: report.inventory.source, openapi_sha256: groupedHash(report.inventory.sha256), composition_root: report.composition_root,
  hash_encoding: 'SHA-256 hex grouped by 8 characters; remove hyphens to recover standard hex',
  base_commit: report.inventory.base_commit, source_sha256: groupedHash(report.inventory.source_sha256),
  source_files: Object.fromEntries(Object.entries(report.inventory.source_files).map(([file, hash]) => [file, groupedHash(hash)])),
  safety: report.safety, database: report.database, operation_count: operationReports.length,
  internal_db_sql_states: report.internal_db_sql_states ?? [],
  runtime_routes: report.runtime_routes,
  result_codes: codes,
  passed_operations: operationReports.filter(op => op.verdict === 'PASS').length,
  executed_requests: operationReports.reduce((sum, op) => sum + op.executed, 0),
  verdict: failures.length || operationReports.some(op => op.verdict !== 'PASS') ? 'FAIL' : 'PASS',
  failures, operations: operationReports };
fs.mkdirSync(path.dirname(output), { recursive: true });
fs.writeFileSync(output, JSON.stringify(result, null, 2) + '\n');
console.log(JSON.stringify({ verdict: result.verdict, passed: result.passed_operations, operations: result.operation_count,
  requests: result.executed_requests, failures: operationReports.filter(op => op.failures.length).map(op => ({ id: op.operation_id, failures: op.failures })) }, null, 2));
process.exitCode = result.verdict === 'PASS' ? 0 : 1;
