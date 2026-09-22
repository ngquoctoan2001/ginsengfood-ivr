#!/usr/bin/env python3
"""Offline worker speech probe. Existing services and production call flags are untouched."""
import argparse
import datetime as dt
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys
import time
import uuid

sys.dont_write_bytecode = True


def digest(path):
    value = hashlib.sha256()
    with path.open('rb') as stream:
        for chunk in iter(lambda: stream.read(1048576), b''):
            value.update(chunk)
    return value.hexdigest()


def verify_kit(root):
    manifest = json.loads((root / 'manifest.json').read_text())
    if manifest['REAL_CUSTOMER_CALL_ALLOWED'] != 'NO':
        raise ValueError('Customer calls must remain disabled')
    required = {'runtime-image.tar', 's5_common.py', 'monitor.py', 'orders.json',
                'expected.json', 'probe/VieNeuWorkerProbe.dll', 'probe/Ivr.Infrastructure.dll',
                'run-worker-s5.py'}
    actual = {path.relative_to(root).as_posix() for path in root.rglob('*')
              if path.is_file() and path.name != 'manifest.json' and '__pycache__' not in path.parts}
    if not required.issubset(manifest['files']) or actual != set(manifest['files']):
        raise ValueError('Incomplete kit manifest')
    for name, expected in manifest['files'].items():
        path = (root / name).resolve()
        path.relative_to(root)
        if digest(path) != expected:
            raise ValueError('Changed kit file: ' + name)
    return manifest


def pinned_image(docker, index, config, archive, log):
    for loaded in (False, True):
        for reference in (index, config):
            result = subprocess.run(docker + ['image', 'inspect', reference], capture_output=True, text=True)
            if result.returncode == 0:
                info = json.loads(result.stdout)[0]
                if info['Id'] in (index, config) and info['Architecture'] == 'amd64' and info['Os'] == 'linux':
                    return info['Id']
        if not loaded:
            with log.open('w') as stream:
                subprocess.run(docker + ['load', '-i', str(archive)], stdout=stream, stderr=subprocess.STDOUT, check=True)
    raise ValueError('Pinned image not available')


def commands(docker, base, kit, out, tts_image, runtime_image, name, scope, seconds):
    identity = '{}:{}'.format(os.getuid(), os.getgid()) if hasattr(os, 'getuid') else '1654:1654'
    safe = ['--pull', 'never', '--read-only', '--user', identity, '--pids-limit', '256',
            '--tmpfs', '/tmp:rw,noexec,nosuid,size=128m', '--cap-drop', 'ALL',
            '--security-opt', 'no-new-privileges', '-e', 'REAL_CUSTOMER_CALL_ALLOWED=NO']
    tts = docker + ['run', '-d', '--name', name, '--network', 'none', '--cpus', '2',
                    '--memory', '4g', '--memory-swap', '4g'] + safe
    for item in ('IVR_EXECUTION_MODE=LAB_REAL_SIM', 'VIE_NEU_MAX_CONCURRENCY=1', 'VIE_NEU_ORT_THREADS=1',
                 'OPENBLAS_NUM_THREADS=1', 'OMP_NUM_THREADS=1', 'MKL_NUM_THREADS=1',
                 'VIE_NEU_ALLOWED_VOICE_IDS=v3t-north-ngoc-linh,v3t-central-ngoc-tran,v3t-south-my-duyen'):
        tts += ['-e', item]
    for source, target in ((base / 'models', '/models'), (base / 'voice-acceptance-manifest.json', '/run/ivr-tts/voice-acceptance-manifest.json'),
                           (kit / 'monitor.py', '/monitor.py')):
        tts += ['--mount', 'type=bind,src={},dst={},readonly'.format(source, target)]
    tts += ['--mount', 'type=bind,src={},dst=/out'.format(out), '--entrypoint', 'python', tts_image, '/monitor.py']
    client = docker + ['run', '--rm', '--name', name + '-client', '--network', 'container:' + name,
                       '--cpus', '0.5', '--memory', '512m', '--memory-swap', '512m'] + safe
    for source, target in ((kit, '/kit'), (base, '/base')):
        client += ['--mount', 'type=bind,src={},dst={},readonly'.format(source, target)]
    client += ['--mount', 'type=bind,src={},dst=/out'.format(out), '--entrypoint', 'dotnet', runtime_image,
               '/kit/probe/VieNeuWorkerProbe.dll', scope, str(seconds)]
    return tts, client


def run_worker_s5():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--base-bundle', required=True, type=Path)
    parser.add_argument('--output', required=True, type=Path)
    parser.add_argument('--expected-host')
    parser.add_argument('--run', action='store_true')
    parser.add_argument('--local-lab', action='store_true', help='Explicit local scope; never produces S5 evidence')
    parser.add_argument('--soak-seconds', type=int, default=450)
    parser.add_argument('--runs', type=int, default=2)
    args = parser.parse_args()
    if not 30 <= args.soak_seconds <= 900 or not 1 <= args.runs <= 2:
        raise ValueError('Bounded to one/two runs, each 30-900 soak seconds')
    root, base, output = Path(__file__).resolve().parent, args.base_bundle.resolve(), args.output.resolve()
    if any(',' in str(path) for path in (root, base, output)):
        raise ValueError('Bind paths cannot contain commas')
    manifest = verify_kit(root)
    if digest(base / 'manifest.json') != manifest['base_manifest_sha256']:
        raise ValueError('Wrong original S5 bundle')
    spec = importlib.util.spec_from_file_location('s5_common', root / 's5_common.py')
    common = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(common)
    base_manifest = common.verify_bundle(base)
    output.mkdir(parents=True, exist_ok=False)
    if args.local_lab:
        docker, inventory, scope = ['docker'], {'scope': 'LOCAL_LAB'}, 'LOCAL_LAB'
    else:
        docker, inventory = common.inventory()
        scope = 'S5_TARGET'
    (output / 'inventory.json').write_text(json.dumps(inventory, indent=2) + '\n')
    if not args.run:
        print('INVENTORY_SAVED; no container started')
        return
    if not args.local_lab:
        common.validate_target(inventory, args.expected_host, 2, 4)
        if inventory['host_memory']['MemAvailable'] < 6.5 * 1024**3:
            raise ValueError('Need 4 GiB TTS + 0.5 GiB client + 2 GiB host headroom')
    tts_image = pinned_image(docker, base_manifest['image_index'], base_manifest['image_config'], base / 'tts-image.tar', output / 'tts-load.log')
    runtime = pinned_image(docker, manifest['runtime_index'], manifest['runtime_config'], root / 'runtime-image.tar', output / 'runtime-load.log')
    binding = {'scope': scope, 'REAL_CUSTOMER_CALL_ALLOWED': 'NO', 'tts_image': tts_image,
               'runtime_image': runtime, 'kit_manifest_sha256': digest(root / 'manifest.json'),
               'tts_cpus': 2, 'tts_memory_gib': 4, 'client_cpus': 0.5, 'client_memory_mib': 512,
               'started_utc': dt.datetime.now(dt.timezone.utc).isoformat(), 'runs': []}
    try:
        for index in range(args.runs):
            out = output / ('run-' + str(index + 1))
            out.mkdir()
            name = 'ivr-w0335-' + uuid.uuid4().hex[:12]
            tts_cmd, client_cmd = commands(docker, base, root, out, tts_image, runtime, name, scope, args.soak_seconds)
            print('RUN_START {} scope={} TTS=2CPU/4GiB; log={}'.format(index + 1, scope, out / 'client.log'), flush=True)
            try:
                subprocess.run(tts_cmd, check=True, stdout=subprocess.DEVNULL)
                for _ in range(120):
                    if (out / 'model.json').exists():
                        break
                    info = json.loads(common.capture(*docker, 'inspect', name))[0]
                    if not info['State']['Running']:
                        raise ValueError('TTS exited during startup')
                    time.sleep(0.5)
                else:
                    raise ValueError('Model startup timeout')
                with (out / 'client.log').open('w') as log:
                    result = subprocess.run(client_cmd, stdout=log, stderr=subprocess.STDOUT, timeout=args.soak_seconds + 900)
                if result.returncode or not (out / 'completion.json').exists():
                    raise ValueError('Client failed; return all logs')
                data = json.loads((out / 'worker.json').read_text())
                completed = json.loads((out / 'completion.json').read_text())
                model = json.loads((out / 'model.json').read_text())
                if data['scope'] != scope or data['REAL_CUSTOMER_CALL_ALLOWED'] != 'NO':
                    raise ValueError('Probe scope mismatch')
                if not completed['completed'] or completed['soak_elapsed_ms'] < args.soak_seconds * 1000:
                    raise ValueError('Sustained interval incomplete')
                if model['capacity'] != 1 or model['ort_threads'] != 1 or not model['samples']:
                    raise ValueError('Unexpected model capacity or threads')
                if any(s['cpu.max'] != '200000 100000' or s['memory.max'] != '4294967296' for s in model['samples']):
                    raise ValueError('Measured quota differs from 2 CPU / 4 GiB')
                if any(not row['pcm_equal'] or row['playlist_segments'] != 7 for row in data['jobs'] if row['error'] is None):
                    raise ValueError('A successful order lacks complete matching audio')
                errors = [row for row in data['jobs'] if row['error']]
                binding['runs'].append({'run': index + 1, 'jobs': len(data['jobs']), 'errors': len(errors),
                                        'pcm_equal': sum(row['pcm_equal'] for row in data['jobs'])})
                print('RUN_COMPLETE ' + json.dumps(binding['runs'][-1]), flush=True)
            finally:
                subprocess.run(docker + ['rm', '-f', name + '-client'], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
                subprocess.run(docker + ['stop', '-t', '10', name], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
                with (out / 'tts.log').open('w') as log:
                    subprocess.run(docker + ['logs', name], stdout=log, stderr=subprocess.STDOUT)
                subprocess.run(docker + ['rm', '-f', name], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    finally:
        binding['ended_utc'] = dt.datetime.now(dt.timezone.utc).isoformat()
        (output / 'host.json').write_text(json.dumps(binding, indent=2) + '\n')
    print('MEASUREMENT_COMPLETE_NOT_PRODUCTION_APPROVAL; return entire result directory', flush=True)


if __name__ == '__main__':
    try:
        run_worker_s5()
    except (ValueError, OSError, subprocess.SubprocessError) as error:
        print('S5_STOP: ' + str(error), file=sys.stderr)
        sys.exit(1)
