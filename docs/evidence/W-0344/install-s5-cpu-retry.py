#!/usr/bin/env python3
"""Apply the verified Asterisk image and case-driver replacement to a fresh copy of the original S5 kit,
then give the non-root TTS sidecar a readable, hash-checked copy of the pinned models."""
import argparse
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import shutil
import socket
import subprocess
import sys
import tarfile


def digest(path):
    value = hashlib.sha256()
    with path.open('rb') as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b''):
            value.update(chunk)
    return value.hexdigest()


def contained(root, relative):
    name = PurePosixPath(relative)
    if name.is_absolute() or '..' in name.parts or '\\' in relative:
        raise ValueError('Path escape in kit')
    path = root.joinpath(*name.parts)
    path.resolve().relative_to(root.resolve())
    return path


def validate_replacement(base, revised):
    # Only the portable PBX and the hash-bound DTMF wait correction may change.
    expected = json.loads(json.dumps(base))
    expected['images']['asterisk'] = revised['images']['asterisk']
    expected['files']['images/asterisk.tar'] = revised['files']['images/asterisk.tar']
    expected['files']['cases.py'] = revised['files']['cases.py']
    if expected != revised or revised['images']['asterisk']['archive'] != 'images/asterisk.tar':
        raise ValueError('CPU retry may replace only the Asterisk image and bound case driver')
    if base['images']['asterisk'] == revised['images']['asterisk']:
        raise ValueError('CPU retry must use a different image')


def prepare_retry(base_dir, patch, destination):
    if destination.exists():
        raise ValueError('Refuse to overwrite a prior run')
    base_dir = base_dir.resolve()
    with tarfile.open(patch) as archive:
        members = archive.getmembers()
        required = {'patch.json', 'manifest.json', 'images/asterisk.tar', 'cases.py'}
        if len(members) != len(required) or {m.name for m in members} != required or any(not m.isfile() for m in members):
            raise ValueError('Patch contains unexpected members or links')
        change = json.load(archive.extractfile('patch.json'))
        if change.get('REAL_CUSTOMER_CALL_ALLOWED') != 'NO':
            raise ValueError('Customer-call boundary missing')
        if digest(base_dir / 'manifest.json') != change['base_manifest_sha256']:
            raise ValueError('Wrong base kit; preserve it and return the receipt')
        base = json.loads((base_dir / 'manifest.json').read_text(encoding='utf-8'))
        revised_bytes = archive.extractfile('manifest.json').read()
        if hashlib.sha256(revised_bytes).hexdigest() != change['new_manifest_sha256']:
            raise ValueError('Replacement manifest hash mismatch')
        revised = json.loads(revised_bytes)
        validate_replacement(base, revised)
        actual = {p.relative_to(base_dir).as_posix() for p in base_dir.rglob('*')
                  if p.is_file() and '__pycache__' not in p.parts and p.name != 'manifest.json'}
        if actual != set(base['files']):
            raise ValueError('Base kit has extra or missing files')
        for relative, sha in base['files'].items():
            path = contained(base_dir, relative)
            if path.is_symlink() or digest(path) != sha:
                raise ValueError('Base kit changed: ' + relative)
        destination.mkdir(parents=True, exist_ok=False)
        for relative in revised['files']:
            target = contained(destination, relative)
            target.parent.mkdir(parents=True, exist_ok=True)
            if relative in ('images/asterisk.tar', 'cases.py'):
                with target.open('xb') as stream:
                    shutil.copyfileobj(archive.extractfile(relative), stream)
            else:
                shutil.copyfile(contained(base_dir, relative), target)
            if digest(target) != revised['files'][relative]:
                raise ValueError('Reconstructed file hash mismatch: ' + relative)
            target.chmod(0o644)
        (destination / 'manifest.json').write_bytes(revised_bytes)
        (destination / 'manifest.json').chmod(0o644)
        for path in [destination] + [p for p in destination.rglob('*') if p.is_dir()]:
            path.chmod(0o755)
    return change


def stage_models(source, lock, destination):
    # The W-0340 mirror is ssv-only (0700/0600) but the TTS image runs as uid 1654, so mounting the
    # mirror directly left the sidecar not_ready on S5 (run 20260923-093745-1c9bd160). Copy each
    # pinned file into a run-private tree it can read; the mirror itself is never modified.
    if destination.exists():
        raise ValueError('Refuse to overwrite staged models')
    source = source.resolve()
    destination.mkdir(parents=True)
    for item in lock['artifacts']:
        origin = contained(source, item['bundle_path'])
        if origin.is_symlink() or digest(origin) != item['sha256'] or origin.stat().st_size != item['size_bytes']:
            raise ValueError('Mirror model changed: ' + item['bundle_path'])
        target = contained(destination, item['bundle_path'])
        target.parent.mkdir(parents=True, exist_ok=True)
        with origin.open('rb') as reader, target.open('xb') as writer:
            shutil.copyfileobj(reader, writer)
        if digest(target) != item['sha256']:
            raise ValueError('Staged model hash mismatch: ' + item['bundle_path'])
        target.chmod(0o644)
    for path in [destination] + [p for p in destination.rglob('*') if p.is_dir()]:
        path.chmod(0o755)
    return len(lock['artifacts'])


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--base-kit', type=Path, required=True)
    args = parser.parse_args()
    if socket.gethostname().split('.')[0] != 'vps61' or not sys.platform.startswith('linux'):
        raise ValueError('Run this installer on vps61 only')
    root = Path(__file__).resolve().parent
    root.relative_to(Path('/home/ssv'))
    if not root.name.startswith('ivr-full-flow-w0344-cpu-'):
        raise ValueError('Expected a fresh W0344 CPU retry directory')
    os.umask(0o077)
    args.base_kit.resolve().relative_to(Path('/home/ssv'))
    patch = root / 'ivr-full-flow-w0344-cpu-r2.tar.gz'
    expected = (root / (patch.name + '.sha256')).read_text().split()[0]
    if digest(patch) != expected:
        raise ValueError('Patch transport checksum mismatch')
    kit = root / 'full-flow-s5'
    prepare_retry(args.base_kit, patch, kit)
    lock = json.loads((kit / 'fixtures/MODELS.lock').read_text(encoding='utf-8-sig'))
    models = root / 'models-readable'
    staged = stage_models(Path('/home/ssv/ivr-artifact-mirror/releases/vieneu-w0340/models'), lock, models)
    print('W0344_CPU_PATCH_VERIFIED image=asterisk dtmf_wait=received_RTP models_staged=%d original_kit_preserved=YES' % staged,
          flush=True)
    try:
        # The original launcher checks host resources, models, bindings, seven cases and cleanup.
        return subprocess.call([sys.executable, '-B', str(kit / 'launcher.py'), '--models', str(models),
                                '--output', str(root / 'result'), '--run'])
    finally:
        shutil.rmtree(models, ignore_errors=True)
        print('W0344_STAGED_MODELS_REMOVED ' + ('NO' if models.exists() else 'YES'), flush=True)


if __name__ == '__main__':
    raise SystemExit(main())
