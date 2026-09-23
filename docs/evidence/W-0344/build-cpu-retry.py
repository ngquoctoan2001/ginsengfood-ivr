#!/usr/bin/env python3
"""Build the retry patch: the r1 portable Asterisk image, byte for byte, plus the current case driver.

r1 (22/09 14:06) replaced only the Asterisk image and was never sent to S5.
r2 (23/09) added cases.py that presses DTMF only after the whole prompt was received. Two S5 runs:
  093745 stopped at TTS readiness (mirror models unreadable by uid 1654; fixed in the installer);
  103255 reached the first call, but the SIP peer could not read its 0600 config.
r3 (23/09) is r2 plus cases.py making the peer's four bind-mounted config files 0644.
Nothing else may differ from the S5 base kit.
Run from any directory on the Windows host; no Docker or network access is needed.
"""
import hashlib
import importlib.util
import io
import json
from pathlib import Path
import sys
import tarfile

ROOT = Path(__file__).resolve().parents[3]
DOC = ROOT / 'docs/evidence/W-0344'
BASE = ROOT / '.artifacts/W-0344/full-flow-s5'  # same bytes as the kit already on S5 (run 135115)
R1 = ROOT / '.artifacts/W-0344/cpu-portable-r1/ivr-full-flow-w0344-cpu-r1.tar.gz'
R1_SHA256 = 'dfaaadd98eb4d7636f2799d2c4a16bef95e4f90d32c68cd0ebf647e4e63d8498'  # pinned in 5e61436
CASES = ROOT / 'deploy/lab/full-flow-s5/cases.py'
REVISION = 'r3'
OUT = ROOT / ('.artifacts/W-0344/cpu-portable-' + REVISION)
NAME = 'ivr-full-flow-w0344-cpu-' + REVISION + '.tar.gz'
SUPERSEDES = {'archive': 'ivr-full-flow-w0344-cpu-r2.tar.gz',
              'sha256': '634c903b54fb8a7ac8ee2c2459f48327a816075d6ac8c412891343f943660dee',
              'sent_to_s5': True, 's5_runs': ['20260923-093745-1c9bd160', '20260923-103255-bf2e4b7c']}
BASE_REMOTE_KIT = '/home/ssv/ivr-full-flow-w0344-20260922-135115-e0b16d79/full-flow-s5'


def sha256(data):
    return hashlib.sha256(data).hexdigest()


def file_sha256(path):
    value = hashlib.sha256()
    with path.open('rb') as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b''):
            value.update(chunk)
    return value.hexdigest()


def member(name, data):
    info = tarfile.TarInfo(name)
    info.size, info.mode, info.mtime = len(data), 0o644, 0
    return info, io.BytesIO(data)


def write_pins(target, checksum):
    installer = DOC / 'install-s5-cpu-retry.py'
    pins = {'work_id': 'W-0344', 'REAL_CUSTOMER_CALL_ALLOWED': 'NO', 'package': 'cpu-' + REVISION,
            'archive_bytes': target.stat().st_size, 'base_remote_kit': BASE_REMOTE_KIT,
            'files': {p.relative_to(ROOT).as_posix(): file_sha256(p) for p in (target, checksum, installer)}}
    (DOC / 'cpu-retry-pins.json').write_text(json.dumps(pins, indent=2) + '\n', encoding='utf-8', newline='\n')


def repin():
    # The installer is transported next to the archive, not inside it; an installer-only fix
    # keeps the archive bytes and re-pins just the three transported files.
    target, checksum = OUT / NAME, OUT / (NAME + '.sha256')
    if checksum.read_text(encoding='ascii').split()[0] != file_sha256(target):
        raise SystemExit('Archive differs from its checksum file')
    write_pins(target, checksum)
    print('W0344_CPU_RETRY_REPINNED installer=%s' % file_sha256(DOC / 'install-s5-cpu-retry.py'), flush=True)


def main():
    if '--repin' in sys.argv:
        return repin()
    if OUT.exists():
        raise SystemExit('Refuse to overwrite ' + str(OUT))
    if file_sha256(R1) != R1_SHA256:
        raise SystemExit('r1 archive differs from its pin; the image cannot be carried over')
    base_manifest = BASE / 'manifest.json'
    base = json.loads(base_manifest.read_text(encoding='utf-8'))
    cases = CASES.read_bytes()
    if b'\r' in cases:
        raise SystemExit('cases.py must be LF; it is hashed here and executed on Linux')
    with tarfile.open(R1) as r1:
        old_patch = json.load(r1.extractfile('patch.json'))
        r1_manifest = json.loads(r1.extractfile('manifest.json').read())
        image = r1.getmember('images/asterisk.tar')
        stream, digest = r1.extractfile(image), hashlib.sha256()
        for chunk in iter(lambda: stream.read(1024 * 1024), b''):
            digest.update(chunk)
        if digest.hexdigest() != r1_manifest['files']['images/asterisk.tar']:
            raise SystemExit('r1 image bytes do not match the r1 manifest')
        if old_patch['base_manifest_sha256'] != file_sha256(base_manifest):
            raise SystemExit('Local base kit is not the one r1 was built against')
        manifest = json.loads(json.dumps(r1_manifest))
        manifest['files']['cases.py'] = sha256(cases)
        expected = json.loads(json.dumps(base))
        for key in ('images/asterisk.tar', 'cases.py'):
            expected['files'][key] = manifest['files'][key]
        expected['images']['asterisk'] = manifest['images']['asterisk']
        if manifest != expected:
            raise SystemExit('package manifest would change more than the image and case driver')
        encoded = (json.dumps(manifest, indent=2) + '\n').encode()
        change = {'work_id': 'W-0344', 'REAL_CUSTOMER_CALL_ALLOWED': 'NO',
                  'base_manifest_sha256': old_patch['base_manifest_sha256'],
                  'new_manifest_sha256': sha256(encoded),
                  'replacement': 'asterisk image unchanged from cpu-r1 (x86-64 generic, BUILD_NATIVE disabled) '
                                 '+ cases.py sends DTMF only after received RTP covers the measured audio '
                                 'and makes the SIP peer config files 0644',
                  'source_version': old_patch['source_version'],
                  'build_inputs_sha256': old_patch['build_inputs_sha256'],
                  'image_source': {'archive': R1.name, 'sha256': R1_SHA256},
                  'cases_py_sha256': sha256(cases),
                  'supersedes': SUPERSEDES}
        OUT.mkdir(parents=True)
        target = OUT / NAME
        with tarfile.open(target, 'w:gz', compresslevel=1) as tar:
            for name, data in (('patch.json', (json.dumps(change, indent=2) + '\n').encode()),
                               ('manifest.json', encoded), ('cases.py', cases)):
                tar.addfile(*member(name, data))
            info = tarfile.TarInfo('images/asterisk.tar')
            info.size, info.mode, info.mtime = image.size, 0o644, 0
            tar.addfile(info, r1.extractfile(image))
    checksum = OUT / (NAME + '.sha256')
    checksum.write_text(file_sha256(target) + '  ' + NAME + '\n', encoding='ascii', newline='\n')
    (DOC / 'cpu-retry-manifest.json').write_bytes(encoded)
    write_pins(target, checksum)
    installer = DOC / 'install-s5-cpu-retry.py'
    spec = importlib.util.spec_from_file_location('retry', installer)
    retry = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(retry)
    retry.prepare_retry(BASE, target, OUT / 'full-flow-s5')
    print('W0344_CPU_%s_BUILT bytes=%d sha256=%s' % (REVISION.upper(), target.stat().st_size, file_sha256(target)), flush=True)


if __name__ == '__main__':
    main()
