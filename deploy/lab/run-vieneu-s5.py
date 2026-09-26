#!/usr/bin/env python3
"""Linux-only, offline VieNeu measurement. No SIP, published ports or production services."""
import argparse
import datetime as dt
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import platform
import socket
import subprocess
import sys
import uuid

# Q-24 (PA3, 2026-09-26). What these tools create on the shared test server stays inside Module 8's
# allocation: container and compose names begin with m8_, a published port is one of 6800-6899,
# and results are written below /home/ssv/m8. Resources that earlier runs left under their old
# names are not touched here; they wait, read-only, to be cleaned up by label.
M8_PREFIX = 'm8_'
M8_ROOT = PurePosixPath('/home/ssv/m8')
M8_PORTS = range(6800, 6900)


def m8_name(stem):
    return M8_PREFIX + stem + '-' + uuid.uuid4().hex[:12]


def require_m8_output(output):
    # Strictly below the root: a run writes into its own directory, never into the root itself.
    if M8_ROOT not in PurePosixPath(Path(output).as_posix()).parents:
        raise ValueError('On S5 the result directory must be inside ' + str(M8_ROOT) + ' (Q-24)')


def require_m8_port(port):
    if port not in M8_PORTS:
        raise ValueError('On S5 a published port must be one of 6800-6899 (Q-24)')


def prepare_output(output, on_s5):
    # Checked before the directory exists, so a refused run leaves nothing behind on the server.
    if on_s5:
        require_m8_output(output)
    output.mkdir(parents=True, exist_ok=False)


def sha256(path):
    result = hashlib.sha256()
    with path.open('rb') as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b''):
            result.update(chunk)
    return result.hexdigest()


def capture(*args):
    return subprocess.check_output(args, text=True, encoding='utf-8').strip()


def verify_bundle(root):
    manifest = json.loads((root / 'manifest.json').read_text())
    if manifest.get('real_customer_call_allowed') != 'NO':
        raise ValueError('Bundle must keep real customer calls disabled')
    required = {'tts-image.tar', 'probe_load.py', 'worker-texts.json',
                'voice-acceptance-manifest.json', 'approved-audio.json'}
    if not required.issubset(manifest['files']):
        raise ValueError('Incomplete bundle manifest')
    for relative, expected in manifest['files'].items():
        path = (root / relative).resolve()
        path.relative_to(root)
        if not path.is_file() or sha256(path) != expected:
            raise ValueError('Missing or changed bundle file: ' + relative)
    for field in ('image_index', 'image_config'):
        value = manifest[field]
        if not value.startswith('sha256:') or len(value) != 71:
            raise ValueError('Invalid pinned image digest')
        int(value[7:], 16)
    fixture = json.loads((root / 'worker-texts.json').read_text(encoding='utf-8-sig'))
    if fixture.get('fixtureOnly') is not True or fixture.get('realCustomerCallAllowed') != 'NO':
        raise ValueError('Only synthetic worker texts are allowed')
    return manifest


def inventory():
    if platform.system() != 'Linux':
        raise ValueError('Run this launcher on the Ubuntu target, not Windows')
    # Pin the current context throughout the run; reject competing overrides.
    context = capture('docker', 'context', 'show')
    endpoint = capture('docker', 'context', 'inspect', context,
                       '--format', '{{.Endpoints.docker.Host}}')
    if os.environ.get('DOCKER_HOST') and os.environ['DOCKER_HOST'] != endpoint:
        raise ValueError('DOCKER_HOST differs from the selected context; reconcile the Docker target first')
    if not endpoint.startswith('unix://'):
        raise ValueError('S5 measurement requires the local Unix Docker socket, not a remote context')
    docker = ['docker', '--context', context]
    info = json.loads(capture(*docker, 'info', '--format', '{{json .}}'))
    memory = {line.split(':')[0]: int(line.split()[1]) * 1024
              for line in Path('/proc/meminfo').read_text().splitlines()
              if line.startswith(('MemTotal:', 'MemAvailable:'))}
    snapshot = {'host_name': socket.gethostname(), 'kernel': platform.release(),
                'architecture': platform.machine(), 'logical_cpus': os.cpu_count(),
                'host_memory': memory, 'os_release': Path('/etc/os-release').read_text(),
                'cpu_summary': capture('lscpu'), 'docker_context': context,
                'docker': {key: info.get(key) for key in ('Name', 'NCPU', 'MemTotal', 'ServerVersion', 'OSType', 'Architecture')},
                'container_stats': capture(*docker, 'stats', '--no-stream', '--format', '{{json .}}').splitlines(),
                'container_memory_limits': [],
                'real_customer_call_allowed': 'NO'}
    for identifier in capture(*docker, 'ps', '-q').splitlines():
        row = json.loads(capture(*docker, 'inspect', identifier, '--format',
                                '{"name":{{json .Name}},"memory_limit_bytes":{{.HostConfig.Memory}}}'))
        snapshot['container_memory_limits'].append(row)
    return docker, snapshot


def validate_target(snapshot, expected_host, cpus, memory_gib):
    if not expected_host or snapshot['host_name'].split('.')[0] != expected_host:
        raise ValueError('Host does not match --expected-host; do not label another machine S5')
    if snapshot['architecture'] not in ('x86_64', 'amd64'):
        raise ValueError('This bundle is pinned to Linux amd64')
    daemon = snapshot['docker']
    if daemon['OSType'] != 'linux' or daemon['Name'].split('.')[0] != expected_host:
        raise ValueError('Docker daemon identity differs from the target; send inventory.json for review')
    ratio = daemon['MemTotal'] / snapshot['host_memory']['MemTotal']
    if not 0.95 <= ratio <= 1.05:
        raise ValueError('Host/Docker RAM mismatch; send inventory.json before a load run')
    if not 1 <= cpus <= 4 or not 2 <= memory_gib <= 4:
        raise ValueError('Initial diagnostic is bounded to 1-4 CPU and 2-4 GiB RAM')
    if cpus > min(daemon['NCPU'], snapshot['logical_cpus']) - 2:
        raise ValueError('Leave at least two CPUs outside the probe quota')
    if (memory_gib + 2) * 1024**3 > snapshot['host_memory']['MemAvailable']:
        raise ValueError('Not enough available RAM for the requested quota plus 2 GiB headroom')


def container_command(docker, root, output, image, name, cpus, memory_gib):
    selections = json.loads((root / 'voice-acceptance-manifest.json').read_text())['selections']
    voices = ','.join(selections[region]['voice_id'] for region in ('North', 'Central', 'South'))
    args = docker + ['run', '--rm', '--name', name, '--pull', 'never', '--network', 'none',
                     '--read-only', '--user', '{}:{}'.format(os.getuid(), os.getgid()),
                     '--cpus', str(cpus), '--memory', str(memory_gib) + 'g',
                     '--memory-swap', str(memory_gib) + 'g', '--pids-limit', '256',
                     '--tmpfs', '/tmp:rw,noexec,nosuid,size=128m', '--cap-drop', 'ALL',
                     '--security-opt', 'no-new-privileges']
    for item in ('IVR_EXECUTION_MODE=LAB_REAL_SIM', 'REAL_CUSTOMER_CALL_ALLOWED=NO',
                 'VIE_NEU_MAX_CONCURRENCY=1', 'VIE_NEU_ORT_THREADS=1',
                 'OPENBLAS_NUM_THREADS=1', 'OMP_NUM_THREADS=1', 'MKL_NUM_THREADS=1',
                 'VIE_NEU_ALLOWED_VOICE_IDS=' + voices):
        args += ['-e', item]
    for source, target in (('models', '/models'), ('voice-acceptance-manifest.json', '/run/ivr-tts/voice-acceptance-manifest.json'),
                           ('worker-texts.json', '/texts.json'), ('probe_load.py', '/probe.py')):
        args += ['--mount', 'type=bind,src={},dst={},readonly'.format(root / source, target)]
    args += ['--mount', 'type=bind,src={},dst=/out'.format(output), '--entrypoint', 'python',
             image, '/probe.py', '--texts', '/texts.json', '--out', '/out', '--scope', 'S5_TARGET']
    return args


def run_vieneu_s5():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--bundle', type=Path, default=Path(__file__).resolve().parent)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--expected-host')
    parser.add_argument('--run', action='store_true', help='Without this flag, only collect read-only inventory')
    parser.add_argument('--cpus', type=int, default=2)
    parser.add_argument('--memory-gib', type=int, default=4)
    args = parser.parse_args()
    output = args.output.resolve()
    prepare_output(output, args.run or bool(args.expected_host))
    docker, snapshot = inventory()
    (output / 'inventory.json').write_text(json.dumps(snapshot, indent=2) + '\n')
    print('INVENTORY_SAVED ' + str(output / 'inventory.json'), flush=True)
    if not args.run:
        return
    validate_target(snapshot, args.expected_host, args.cpus, args.memory_gib)
    root = args.bundle.resolve()
    if ',' in str(root) or ',' in str(output):
        raise ValueError('Docker bind paths must not contain commas')
    manifest = verify_bundle(root)
    image = None
    for loaded in (False, True):
        for reference in (manifest['image_index'], manifest['image_config']):
            inspect = subprocess.run(docker + ['image', 'inspect', reference], capture_output=True, text=True)
            if inspect.returncode == 0:
                info = json.loads(inspect.stdout)[0]
                if info['Id'] in (manifest['image_index'], manifest['image_config']) and info['Architecture'] == 'amd64' and info['Os'] == 'linux':
                    image = info['Id']
                    break
        if image:
            break
        if not loaded:
            with (output / 'image-load.log').open('w') as log:
                subprocess.run(docker + ['load', '--input', str(root / 'tts-image.tar')], stdout=log, stderr=subprocess.STDOUT, check=True)
    if not image:
        raise ValueError('Pinned image not found after loading the verified archive')
    name = m8_name('ivr-s5-probe')
    binding = {'scope': 'S5_TARGET', 'image': image, 'image_index': manifest['image_index'],
               'image_config': manifest['image_config'], 'manifest_sha256': sha256(root / 'manifest.json'),
               'cpu_limit': args.cpus, 'memory_gib': args.memory_gib, 'real_customer_call_allowed': 'NO',
               'started_utc': dt.datetime.now(dt.timezone.utc).isoformat()}
    (output / 'host.json').write_text(json.dumps(binding, indent=2) + '\n')
    print('PROBE_STARTED CPU={} RAM={}GiB; progress: {}/run.log'.format(args.cpus, args.memory_gib, output), flush=True)
    try:
        with (output / 'run.log').open('w') as log:
            result = subprocess.run(container_command(docker, root, output, image, name, args.cpus, args.memory_gib),
                                    stdout=log, stderr=subprocess.STDOUT, timeout=1800)
        binding['exit_code'] = result.returncode
        if result.returncode:
            raise ValueError('Probe failed; keep and return the result directory, including run.log')
        load = json.loads((output / 'load.json').read_text())
        expected = json.loads((root / 'approved-audio.json').read_text())
        successes = [row for row in load['requests'] if row['status'] == 200]
        for row in successes:
            key = '{}:{}:{}'.format(row['region'], row['variant'], row['ordinal'])
            if row['pcm_sha256'] != expected[key]:
                raise ValueError('Audio checksum differs from the approved fixture: ' + key)
        binding['successful_pcm_equal'] = len(successes)
        binding['all_probe_assertions_pass'] = load.get('all_assertions_pass') is True
        if not binding['all_probe_assertions_pass']:
            raise ValueError('Probe did not complete all assertions')
        print('S5_MEASUREMENT_COMPLETE ' + json.dumps(load['sequential']), flush=True)
        print('Measurement complete is not production approval. Return the entire result directory.', flush=True)
    finally:
        subprocess.run(docker + ['rm', '-f', name], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        binding['ended_utc'] = dt.datetime.now(dt.timezone.utc).isoformat()
        (output / 'host.json').write_text(json.dumps(binding, indent=2) + '\n')


if __name__ == '__main__':
    try:
        run_vieneu_s5()
    except (ValueError, OSError, subprocess.SubprocessError) as error:
        print('S5_STOP: ' + str(error), file=sys.stderr)
        sys.exit(1)
