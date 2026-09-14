import fs from 'node:fs';
import { spawnSync } from 'node:child_process';
const cases = [
  ['contract SDK drift', 'contract-e2e', s => s.replace('sdk:10.0.201', 'sdk:10.0'), 'image must match global.json'],
  ['chaos SDK drift', 'chaos', s => s.replace('sdk:10.0.201', 'sdk:10.0'), 'image must match global.json'],
  ['observability SDK drift', 'observability', s => s.replace('sdk:10.0.201', 'sdk:10.0'), 'image must match global.json'],
  ['review SDK drift', 'quality-gate', s => s.replace('sdk:10.0.201', 'sdk:10.0'), 'image must match global.json'],
  ['sweep Node-only image', 'quality-gate', s => s.replace(/(gate_sweep:[\s\S]*?image: )[^\n]+/, '$1node:24-bookworm-slim'), 'gate_sweep requires the pinned .NET SDK'],
  ['sweep missing DinD', 'quality-gate', s => s.replace('name: docker:29.6.2-dind', 'name: docker:29.6.2-cli'), 'gate_sweep requires Docker-in-Docker'],
  ['sweep missing Node 24', 'quality-gate', s => s.replace('docker cp gate-node-toolchain:/usr/local/. /usr/local/', 'echo no-node'), 'gate_sweep bootstrap missing'],
  ['sweep missing Buildx', 'quality-gate', s => s.replace('docker.io docker-buildx git', 'docker.io git'), 'gate_sweep bootstrap missing'],
  ['sweep missing policy build', 'quality-gate', s => s.replace('dotnet build deploy/ci/tools/Ivr.CiPolicy/Ivr.CiPolicy.csproj --configuration Release --no-restore', 'echo no-policy-build'), 'gate_sweep bootstrap missing'],
  ['TTS missing dependencies', 'tts', s => s.replace('npm --prefix deploy/ci ci --no-audit --no-fund', 'echo no-install'), 'tts_candidate_selftest must install'],
];
for (const [name, fragment, mutate, expected] of cases) {
  const file = `deploy/ci/${fragment}.gitlab-ci.yml`;
  const original = fs.readFileSync(file, 'utf8');
  try {
    fs.writeFileSync(file, mutate(original));
    const result = spawnSync(process.execPath, ['deploy/ci/scripts/ci-config-selftest.mjs'], {encoding:'utf8'});
    if (result.status === 0 || !`${result.stdout}${result.stderr}`.includes(expected)) throw new Error(`${name}: wrong verdict ${result.status}\n${result.stderr}`);
    console.log(`REJECTED ${name}`);
  } finally { fs.writeFileSync(file, original); }
}
const valid = spawnSync(process.execPath, ['deploy/ci/scripts/ci-config-selftest.mjs'], {encoding:'utf8'});
if (valid.status !== 0 || !valid.stdout.includes('CI_CONFIG_SELFTEST_PASS')) throw new Error(valid.stderr);
console.log(`CONFIG_REGRESSION_PASS negatives=${cases.length} positive=1`);
