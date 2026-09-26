#!/usr/bin/env python3
"""Offline synthetic order -> speech -> SIP/RFC4733 -> PostgreSQL, isolated on S5."""
import argparse
import datetime as dt
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import re
import secrets
import shutil
import socket
import subprocess
import sys
import tarfile
import time
import traceback
import uuid

SHA = '66a6baa2019efe69a1ede3b6179fce5b49643a6d'
EXPECTED = dict(tasks=7, attempts=7, counted_customer_attempts=4, technical_attempts=3,
                results=9, final_results=6, callbacks=6, miscounted_technical_attempts=0,
                miscounted_noncustomer_results=0, duplicate_finals=0,
                duplicate_callbacks=0, callbacks_for_nonfinal_results=0)
CASES = ['confirm', 'customer-cancel', 'technical-retry', 'queued-expiry',
         'tts-expiry', 'no-input', 'operator-cancel']
SERVICES = {'postgres', 'ivr-migrate', 'seed', 'lab-seed', 'media-init',
            'fake-sales', 'asterisk', 'ivr-api', 'ivr-worker', 'ivr-tts'}

# Q-24 (PA3, 2026-09-26). The allocation s5_common (run-vieneu-s5.py) enforces: compose and
# container names begin with m8_, the published API port is one of 6800-6899, and results go
# below /home/ssv/m8. Restated rather than imported, because the kit's s5_common is loaded only
# after the kit has been verified and the result directory is made before that. A local
# rehearsal (--local-lab) is not on the shared server and keeps any port and directory.
M8_PREFIX = 'm8_'
M8_ROOT = PurePosixPath('/home/ssv/m8')
M8_PORTS = range(6800, 6900)
DEFAULT_PORT = 6843


def project_name(now):
    return M8_PREFIX + 'ivr-flow-' + now.strftime('%Y%m%d%H%M%S') + '-' + uuid.uuid4().hex[:6]


def require_s5_allocation(output, port):
    if M8_ROOT not in PurePosixPath(Path(output).as_posix()).parents:
        raise ValueError('On S5 the result directory must be inside ' + str(M8_ROOT) + ' (Q-24)')
    if port not in M8_PORTS:
        raise ValueError('On S5 the API port must be one of 6800-6899 (Q-24)')


def prepare_output(output, port, local_lab):
    # Checked before the directory exists, so a refused run leaves nothing behind on the server.
    if not local_lab:
        require_s5_allocation(output, port)
    output.mkdir(parents=True, exist_ok=False)


def sha256(path):
    value = hashlib.sha256()
    with path.open('rb') as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b''):
            value.update(chunk)
    return value.hexdigest()


def save(path, value):
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def capture(*args):
    return subprocess.check_output(args, encoding='utf-8', stderr=subprocess.STDOUT, timeout=60).strip()


def safe_path(root, relative):
    if not isinstance(relative, str) or '\\' in relative or Path(relative).is_absolute():
        raise ValueError('Invalid manifest path')
    path = (root / relative).resolve()
    path.relative_to(root.resolve())
    if path.is_symlink() or not path.is_file():
        raise ValueError('Missing regular file: ' + relative)
    return path


def verify_kit(root):
    manifest = json.loads((root / 'manifest.json').read_text(encoding='utf-8'))
    if manifest.get('candidate') != SHA or manifest.get('REAL_CUSTOMER_CALL_ALLOWED') != 'NO':
        raise ValueError('Wrong application candidate or customer-call boundary')
    files = manifest.get('files', {})
    required = {'launcher.py', 'cases.py', 's5_common.py', 'compose.template.json',
                'fixtures/MODELS.lock', 'fixtures/voice-acceptance-manifest.json',
                'fixtures/seed.sql', 'fixtures/lab-seed.sql', 'fixtures/reconciliation.sql',
                'fixtures/fake-orders.json', 'fixtures/measurement.json',
                'fixtures/worker-texts.json', 'fixtures/worker-texts-expanded.json'}
    if not required.issubset(files):
        raise ValueError('Incomplete kit manifest')
    actual = {p.relative_to(root).as_posix() for p in root.rglob('*')
              if p.is_file() and '__pycache__' not in p.parts and p.name != 'manifest.json'}
    if actual != set(files):
        raise ValueError('Unexpected or omitted files in kit')
    for relative, digest in files.items():
        if sha256(safe_path(root, relative)) != digest:
            raise ValueError('Kit hash mismatch: ' + relative)
    if set(manifest['images']) != SERVICES:
        raise ValueError('Incomplete image set')
    for spec in manifest['images'].values():
        for key in ('index', 'config'):
            if not re.fullmatch(r'sha256:[0-9a-f]{64}', spec[key]):
                raise ValueError('Image digest required')
        if spec['archive'] not in files:
            raise ValueError('Unbound image archive')
    for name in ('worker-texts.json', 'worker-texts-expanded.json'):
        fixture = json.loads((root / 'fixtures' / name).read_text(encoding='utf-8-sig'))
        if fixture.get('fixtureOnly') is not True or fixture.get('realCustomerCallAllowed') != 'NO':
            raise ValueError('Only synthetic fixtures are permitted')
    return manifest


def verify_models(root, models):
    lock = json.loads((root / 'fixtures/MODELS.lock').read_text(encoding='utf-8-sig'))
    checked = []
    for item in lock['artifacts']:
        path = safe_path(models, item['bundle_path'])
        if sha256(path) != item['sha256'] or path.stat().st_size != item['size_bytes']:
            raise ValueError('Model hash/size mismatch: ' + item['bundle_path'])
        checked.append({'path': item['bundle_path'], 'sha256': item['sha256']})
    if len(checked) != 13:
        raise ValueError('Expected all 13 pinned model/card artifacts')
    return checked


def validate_profile(config):
    services = config['services']
    if set(services) != SERVICES:
        raise ValueError('Unexpected compose service')
    for name in ('ivr-api', 'ivr-worker'):
        env = services[name]['environment']
        required = {'IVR_EXECUTION_MODE': 'LAB_REAL_SIM', 'REAL_CUSTOMER_CALL_ALLOWED': 'NO',
                    'Ivr__Telephony__Asterisk__DestinationAlias': 'LAB-A',
                    'ConnectionStrings__IvrDb': 'Host=postgres;Database=ivr;Username=ivr;GSS Encryption Mode=Disable',
                    'Ivr__CallbackDelivery__TargetBaseUrl': 'http://fake-sales:8080'}
        if any(env.get(k) != v for k, v in required.items()):
            raise ValueError('Lab boundary changed: ' + name)
    worker = services['ivr-worker']['environment']
    required = {'Ivr__Speech__Tts__TimeoutMilliseconds': '30000',
                'Ivr__Speech__Tts__PreparationTimeoutMilliseconds': '120000',
                'Ivr__Speech__Tts__PreparationQueueTimeoutMilliseconds': '90000',
                'Ivr__Speech__Tts__PreparationQueueLimit': '8'}
    if any(worker.get(k) != v for k, v in required.items()):
        raise ValueError('Unmeasured speech timing profile')
    tts = services['ivr-tts']
    if (tts.get('cpus'), tts.get('mem_limit'), tts.get('memswap_limit')) != (2, '4g', '4g'):
        raise ValueError('TTS must retain 2 CPU / 4 GiB')
    if tts.get('network_mode') != 'service:ivr-worker' or not tts.get('read_only'):
        raise ValueError('TTS isolation changed')
    if set(config['networks']) != {'lab', 'internal', 'frontend'} or any(
            not config['networks'][n].get('internal') for n in ('lab', 'internal')):
        raise ValueError('SIP and backend networks must block external routing')
    for name, service in services.items():
        if service.get('pull_policy') != 'never' or 'build' in service:
            raise ValueError('Only offline pinned images are allowed')
        if service.get('privileged') or service.get('network_mode') == 'host':
            raise ValueError('Privileged/host network is forbidden')
        if name != 'ivr-api' and service.get('ports'):
            raise ValueError('Only the loopback API may publish a port')
        if name != 'ivr-api' and 'frontend' in service.get('networks', []):
            raise ValueError('Only API may join the loopback ingress network')
    return config


def pick_image(docker, spec):
    for digest in (spec['index'], spec['config']):
        found = subprocess.run(docker + ['image', 'inspect', digest], capture_output=True, encoding='utf-8')
        if found.returncode == 0:
            data = json.loads(found.stdout)[0]
            if data['Id'] not in (spec['index'], spec['config']) or (data['Os'], data['Architecture']) != ('linux', 'amd64'):
                raise ValueError('Loaded image identity/platform mismatch')
            return data['Id']
    return None


def existing_services(docker, exclude_project=None):
    result = {}
    for identifier in capture(*docker, 'ps', '-aq').splitlines():
        data = json.loads(capture(*docker, 'inspect', identifier))[0]
        labels = data['Config'].get('Labels') or {}
        if exclude_project and (labels.get('com.docker.compose.project') == exclude_project or labels.get('ivr.flow.project') == exclude_project):
            continue
        result[data['Name']] = {'id': data['Id'], 'started_at': data['State']['StartedAt'],
                                'restart_count': data['RestartCount'], 'status': data['State']['Status']}
    return result


def owns_container(data, project):
    labels = data['Config'].get('Labels') or {}
    return labels.get('com.docker.compose.project') == project or (
        data['Name'] == '/' + project + '-peer' and labels.get('ivr.work') == 'W0344'
        and labels.get('ivr.flow.project') == project)


def cleanup(docker, project, output):
    removed = []
    for identifier in capture(*docker, 'ps', '-aq').splitlines():
        data = json.loads(capture(*docker, 'inspect', identifier))[0]
        if not owns_container(data, project):
            continue
        if data['State']['Paused']:
            capture(*docker, 'unpause', identifier)
        name = data['Name'].lstrip('/')
        log = subprocess.run(docker + ['logs', '--timestamps', identifier], capture_output=True, encoding='utf-8', errors='replace', timeout=30)
        (output / (name + '.log')).write_text(log.stdout + log.stderr, encoding='utf-8')
        capture(*docker, 'rm', '-f', identifier)
        removed.append(name)
    for identifier in capture(*docker, 'network', 'ls', '-q', '--filter', 'label=com.docker.compose.project=' + project).splitlines():
        data = json.loads(capture(*docker, 'network', 'inspect', identifier))[0]
        if data['Labels'].get('com.docker.compose.project') != project or data['Containers']:
            raise ValueError('Network ownership or empty-network guard failed')
        capture(*docker, 'network', 'rm', identifier)
    if capture(*docker, 'ps', '-aq', '--filter', 'label=com.docker.compose.project=' + project):
        raise ValueError('Owned containers remain after cleanup')
    save(output / 'cleanup.json', {'removed_containers': removed, 'networks_removed': True,
                                  'named_volumes_preserved': [project + '-pg', project + '-media']})


def package_receipt(output):
    # Explicit public allowlist. Never export compose credentials, pjsip configuration or a DB dump.
    paths = [p for p in output.rglob('*') if p.is_file() and (
        (p.parent == output and p.suffix in ('.log', '.json') and p.name != 'compose.private.json') or
        (p.parent == output / 'run-2' and p.suffix in ('.log', '.json')))]
    save(output / 'receipt-manifest.json', {p.relative_to(output).as_posix(): sha256(p) for p in paths})
    receipt = output.with_suffix('.tar.gz')
    with tarfile.open(receipt, 'w:gz') as archive:
        for path in paths + [output / 'receipt-manifest.json']:
            archive.add(path, arcname=output.name + '/' + path.relative_to(output).as_posix())
    return receipt


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--bundle', type=Path, default=Path(__file__).resolve().parent)
    parser.add_argument('--models', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--port', type=int, default=DEFAULT_PORT)
    parser.add_argument('--run', action='store_true')
    parser.add_argument('--local-lab', action='store_true', help='Explicit rehearsal; cannot claim S5 evidence')
    args = parser.parse_args()
    if sys.flags.optimize:
        raise ValueError('Assertions must not be disabled')
    os.environ.pop('PYTHONOPTIMIZE', None)
    root, models, output = args.bundle.resolve(), args.models.resolve(), args.output.resolve()
    if root in output.parents or output == root:
        raise ValueError('Output must be outside the immutable kit')
    prepare_output(output, args.port, args.local_lab)
    project = project_name(dt.datetime.now(dt.timezone.utc))
    result = {'work_id': 'W-0344', 'candidate': SHA, 'REAL_CUSTOMER_CALL_ALLOWED': 'NO',
              'scope': 'LOCAL_REHEARSAL' if args.local_lab else 'S5_TARGET_SYNTHETIC_SIP',
              'project': project, 'started_utc': dt.datetime.now(dt.timezone.utc).isoformat(), 'status': 'FAIL'}
    docker = None
    before = None
    started = False
    code = 1
    try:
        manifest = verify_kit(root)
        save(output / 'kit-manifest.json', manifest)
        result['kit_manifest_sha256'] = sha256(root / 'manifest.json')
        save(output / 'model-bindings.json', verify_models(root, models))
        sys.path.insert(0, str(root))
        import s5_common
        if args.local_lab:
            context = capture('docker', 'context', 'show')
            docker = ['docker', '--context', context]
            info = json.loads(capture(*docker, 'info', '--format', '{{json .}}'))
            if info['OSType'] != 'linux':
                raise ValueError('Linux Docker required for rehearsal')
            inventory = {'host_name': socket.gethostname(), 'docker_context': context,
                         'docker': {k: info[k] for k in ('Name', 'NCPU', 'MemTotal', 'OSType')}, 'scope': 'LOCAL_REHEARSAL'}
        else:
            docker, inventory = s5_common.inventory()
            s5_common.validate_target(inventory, 'vps61', 2, 4)
            if inventory['host_memory']['MemAvailable'] < 9 * 1024**3 or inventory['docker']['NCPU'] < 8:
                raise ValueError('Full lab needs 9 GiB currently available RAM and at least 8 logical CPUs')
        save(output / 'inventory.json', inventory)
        if not args.run:
            result['status'] = 'INVENTORY_ONLY'; code = 0
            return code
        if not 1024 <= args.port <= 65535:
            raise ValueError('Invalid loopback port')
        with socket.socket() as probe:
            probe.bind(('127.0.0.1', args.port))
        if shutil.disk_usage(output).free < 4 * 1024**3:
            raise ValueError('At least 4 GiB free disk required')
        before = existing_services(docker)
        save(output / 'services-before.json', before)
        for kind, names in [('volume', [project + '-pg', project + '-media']),
                            ('network', [project + '-lab', project + '-internal'])]:
            for name in names:
                if subprocess.run(docker + [kind, 'inspect', name], capture_output=True).returncode == 0:
                    raise ValueError('Existing resource: ' + name)
        images = {}
        loaded = set()
        for service, spec in manifest['images'].items():
            image = pick_image(docker, spec)
            if image is None:
                if spec['archive'] in loaded:
                    raise ValueError('Archive did not restore pinned image')
                print('W0344_LOAD_IMAGE ' + service, flush=True)
                with (output / ('load-' + service + '.log')).open('w', encoding='utf-8') as log:
                    subprocess.run(docker + ['load', '-i', str(root / spec['archive'])], stdout=log, stderr=subprocess.STDOUT, check=True, timeout=300)
                loaded.add(spec['archive']); image = pick_image(docker, spec)
            if image is None:
                raise ValueError('Image missing after verified offline load')
            images[service] = image
        config = validate_profile(json.loads((root / 'compose.template.json').read_text()))
        config['name'] = project
        for name in ('lab', 'internal', 'frontend'):
            config['networks'][name]['name'] = project + '-' + name
        for name in ('pg', 'media'):
            config['volumes'][name]['name'] = project + '-' + name
        services = config['services']
        for name in services:
            services[name]['image'] = images[name]
        for name, fixture in [('seed', 'seed.sql'), ('lab-seed', 'lab-seed.sql')]:
            services[name]['volumes'] = [str(root / 'fixtures' / fixture) + ':/seed.sql:ro']
        services['ivr-api']['ports'] = ['127.0.0.1:' + str(args.port) + ':8080']
        ari = 'ari-' + secrets.token_hex(16)
        services['asterisk']['environment'] = {'IVR_LAB_ARI_PASSWORD': ari, 'IVR_LAB_SIP_PASSWORD': 'sip-' + secrets.token_hex(16)}
        for name in ('ivr-api', 'ivr-worker'):
            services[name]['environment']['Ivr__Telephony__Asterisk__Password'] = ari
        services['ivr-tts']['volumes'] = [str(models) + ':/models:ro',
            str(root / 'fixtures/voice-acceptance-manifest.json') + ':/run/ivr-tts/voice-acceptance-manifest.json:ro']
        save(output / 'compose.private.json', config)
        save(output / 'runtime-manifest.json', {'candidate': SHA, 'images': images, 'project': project,
                                               'tts_cpus': 2, 'tts_memory_gib': 4, 'api_port': args.port})
        started = True
        with (output / 'stack-start.log').open('w', encoding='utf-8') as log:
            subprocess.run(docker + ['compose', '-f', str(output / 'compose.private.json'), 'up', '-d', '--no-build', '--pull', 'never'],
                           stdout=log, stderr=subprocess.STDOUT, check=True, timeout=300)
        def inspect(service):
            data = json.loads(capture(*docker, 'inspect', project + '-' + service + '-1'))[0]
            if not owns_container(data, project) or data['Image'] != images[service]:
                raise ValueError('Runtime ownership/image mismatch')
            return data
        deadline = time.monotonic() + 180
        while time.monotonic() < deadline:
            if all(inspect(s)['State'].get('Health', {}).get('Status') == 'healthy' for s in ('ivr-api', 'ivr-tts', 'asterisk')):
                break
            time.sleep(2)
        else:
            raise ValueError('Isolated stack failed readiness')
        bindings = []
        dll_dir = output / 'dlls'; dll_dir.mkdir()
        for binding in manifest['dll_bindings']:
            info = inspect(binding['service'])
            if info['Config']['Labels'].get('org.opencontainers.image.revision') != SHA:
                raise ValueError('Wrong application revision label')
            target = dll_dir / (binding['service'] + '-' + binding['assembly'] + '.dll')
            capture(*docker, 'cp', info['Id'] + ':/app/' + binding['assembly'] + '.dll', str(target))
            actual = sha256(target)
            if actual != binding['sha256']:
                raise ValueError('Runtime DLL differs from accepted W0339: ' + binding['assembly'])
            bindings.append(dict(binding, actual_sha256=actual))
        migration = inspect('ivr-migrate')
        if migration['State']['ExitCode'] != 0 or migration['Config']['Labels'].get('org.opencontainers.image.revision') != SHA:
            raise ValueError('Migration failed or wrong revision')
        capture(*docker, 'cp', migration['Id'] + ':/app/efbundle', str(dll_dir / 'efbundle'))
        if sha256(dll_dir / 'efbundle') != manifest['migration_sha256']:
            raise ValueError('Migration executable differs from accepted W0339')
        tts = inspect('ivr-tts')
        resource = {k: tts['HostConfig'][k] for k in ('NanoCpus', 'Memory', 'MemorySwap', 'ReadonlyRootfs')}
        if resource != dict(NanoCpus=2000000000, Memory=4294967296, MemorySwap=4294967296, ReadonlyRootfs=True):
            raise ValueError('Runtime TTS resource mismatch')
        if any(m['RW'] for m in tts['Mounts'] if m['Type'] == 'bind'):
            raise ValueError('Model mount must be read-only')
        save(output / 'runtime-bindings.json', {'dlls': bindings, 'migration_sha256': manifest['migration_sha256'], 'tts': resource})
        env = dict(os.environ, IVR_FLOW_OUTPUT=str(output), IVR_FLOW_PROJECT=project,
                   IVR_FLOW_PORT=str(args.port), IVR_FLOW_SCOPE=result['scope'],
                   IVR_FLOW_DOCKER_CONTEXT=inventory['docker_context'], PYTHONUNBUFFERED='1')
        print('W0344_STACK_READY cases=7 scope=' + result['scope'], flush=True)
        with (output / 'flow.log').open('w', encoding='utf-8') as log:
            process = subprocess.Popen([sys.executable, str(root / 'cases.py')], env=env, stdout=log, stderr=subprocess.STDOUT)
            started_at = time.monotonic()
            try:
                while process.poll() is None:
                    try:
                        process.wait(timeout=20)
                    except subprocess.TimeoutExpired:
                        elapsed = int(time.monotonic() - started_at)
                        completed = (output / 'flow.log').read_text(encoding='utf-8').count('W0344_CASE_PASS')
                        print(f'W0344_PROGRESS seconds={elapsed} completed={completed}/7', flush=True)
                        if elapsed > 1500:
                            raise TimeoutError('Full-flow harness exceeded 1500 seconds')
                if process.returncode:
                    raise RuntimeError('Full-flow cases failed; see flow.log')
            finally:
                if process.poll() is None:
                    process.kill(); process.wait(timeout=10)
        flow = json.loads((output / 'run-2/result.json').read_text(encoding='utf-8'))
        if flow['status'] != 'PASS' or [c['case'] for c in flow['cases']] != CASES or not all(c['pass'] for c in flow['cases']):
            raise ValueError('Incomplete seven-case proof')
        query = (root / 'fixtures/reconciliation.sql').read_text()
        totals = json.loads(capture(*docker, 'exec', project + '-postgres-1', 'psql', '-U', 'ivr', '-d', 'ivr', '-Atc', query))
        save(output / 'reconciliation.json', {'actual': totals, 'expected': EXPECTED, 'status': 'PASS' if totals == EXPECTED else 'FAIL'})
        if totals != EXPECTED:
            raise ValueError('Database reconciliation mismatch')
        save(output / 'model-bindings-after.json', verify_models(root, models))
        result.update(status='PASS', cases_passed=7, reconciliation=totals)
        code = 0
    except BaseException as error:
        result['error'] = repr(error)
        (output / 'error.log').write_text(traceback.format_exc(), encoding='utf-8')
        print('W0344_ERROR ' + repr(error), flush=True)
    finally:
        try:
            if started:
                cleanup(docker, project, output)
            if before is not None:
                after = existing_services(docker, project)
                unchanged = all(after.get(name) == value for name, value in before.items())
                save(output / 'services-after.json', {'services': after, 'original_services_unchanged': unchanged})
                if not unchanged:
                    raise ValueError('Existing service state changed during this run; inspect receipts')
        except BaseException as error:
            result['cleanup_error'] = repr(error); result['status'] = 'FAIL'; code = 1
        result['finished_utc'] = dt.datetime.now(dt.timezone.utc).isoformat()
        save(output / 'summary.json', result)
        receipt = package_receipt(output)
        print('W0344_' + result['scope'] + '_' + result['status'], flush=True)
        print('W0344_RECEIPT ' + str(receipt), flush=True)
    return code


if __name__ == '__main__':
    raise SystemExit(main())
