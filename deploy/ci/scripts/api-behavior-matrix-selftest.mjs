import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';
import assert from 'node:assert/strict';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../..');
const baseline = JSON.parse(fs.readFileSync(path.join(root, '.artifacts/api-matrix/http-observations.json'), 'utf8'));
const temporary = fs.mkdtempSync(path.join(root, '.artifacts/api-matrix/validator-selftest-'));
const happy = report => report.cases.find(item => item.operation_id === 'getQueue' && item.case === 'happy');
const tests = [
  ['valid', () => {}, true],
  ['missing-operation', report => report.inventory.operations.pop(), false],
  ['duplicate-operation', report => report.inventory.operations[0] = structuredClone(report.inventory.operations[1]), false],
  ['missing-runtime-route', report => report.runtime_routes.pop(), false],
  ['undocumented-status', report => happy(report).status = 201, false],
  ['missing-auth', report => report.cases = report.cases.filter(item => !(item.operation_id === 'getQueue' && item.case === 'auth_missing')), false],
  ['unknown-field', report => happy(report).response.unlisted = true, false],
  ['missing-required', report => delete happy(report).response.paused, false],
  ...['phone', 'address', 'payment', 'recording'].map(field => [field, report => happy(report).response[field] = 'synthetic-sensitive-canary', false]),
  ['raw-phone-value', report => report.cases.find(item => item.operation_id === 'getCallJob' && item.case === 'happy').response.status = '0934567890', false],
  ['missing-correlation', report => happy(report).correlation_pass = false, false],
  ['envelope-correlation', report => report.cases.find(item => item.case === 'auth_missing').response.error.correlationId = 'wrong-correlation', false],
  ['missing-replay-proof', report => delete report.cases.find(item => item.case === 'replay_same_payload').persisted_state_unchanged, false],
  ['pre-call-admitted', report => report.result_codes.find(code => !code.runtime_result).call_result_construction_rejected = false, false],
  ['stale-source', report => report.inventory.source_sha256 = 'stale', false],
];
for (const [name, mutate, expected] of tests) {
  const candidate = structuredClone(baseline);
  mutate(candidate);
  const input = path.join(temporary, `${name}.json`);
  const output = path.join(temporary, `${name}.report.json`);
  fs.writeFileSync(input, JSON.stringify(candidate));
  const child = spawnSync(process.execPath, ['deploy/ci/scripts/verify-api-behavior-matrix.mjs', input, output],
    { cwd: root, encoding: 'utf8', timeout: 30000, windowsHide: true });
  assert.equal(child.status === 0, expected, `${name}: unexpected validator exit ${child.status}: ${child.stderr}`);
  const result = JSON.parse(fs.readFileSync(output, 'utf8'));
  assert.equal(result.verdict === 'PASS', expected, name);
}
console.log(`API_MATRIX_VALIDATOR_SELFTEST=PASS (${tests.length} cases; 1 valid, ${tests.length - 1} refusals)`);
