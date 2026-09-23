#!/usr/bin/env python3
"""Check the pinned retry patch end to end before it goes to S5.

Proves: the pins match the files, the S5 base kit is the one run 135115 verified, the image
bytes are r1's, only cases.py and the image differ from the base kit, no assertion changed,
the reconstructed kit matches the manifest, and the remote bash block parses.
"""
from collections import Counter
import difflib
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys
import tarfile

ROOT = Path(__file__).resolve().parents[3]
DOC = ROOT / 'docs/evidence/W-0344'
BASE = ROOT / '.artifacts/W-0344/full-flow-s5'
R1 = ROOT / '.artifacts/W-0344/cpu-portable-r1'
BASH = Path('C:/Program Files/Git/bin/bash.exe')


def sha(path):
    value = hashlib.sha256()
    with Path(path).open('rb') as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b''):
            value.update(chunk)
    return value.hexdigest()


def asserts(text):
    return [line.strip() for line in text.splitlines() if re.search(r'\bassert\b', line)]


def main():
    # Pins and manifest live with the work item that built the package (W-0344 by default);
    # the installer, retry script and S5 base-kit receipt are always W-0344's.
    evid = ROOT / sys.argv[sys.argv.index('--evidence-dir') + 1] if '--evidence-dir' in sys.argv else DOC
    pins = json.loads((evid / 'cpu-retry-pins.json').read_text(encoding='utf-8'))
    for relative, digest in pins['files'].items():
        assert sha(ROOT / relative) == digest, 'pin mismatch: ' + relative
    archives = [k for k in pins['files']
                if re.fullmatch(r'\.artifacts/W-0344/cpu-portable-r[0-9]+/ivr-full-flow-w0344-cpu-r[0-9]+\.tar\.gz', k)]
    assert len(archives) == 1, archives
    archive = ROOT / archives[0]
    OUT = archive.parent
    assert (OUT / (archive.name + '.sha256')).read_text().split()[0] == sha(archive)
    s5_first = json.loads((DOC / 's5-first-run.json').read_text(encoding='utf-8'))
    base_manifest_sha = sha(BASE / 'manifest.json')
    assert base_manifest_sha == s5_first['summary']['kit_manifest_sha256'], 'local base kit is not the S5 kit'
    base = json.loads((BASE / 'manifest.json').read_text(encoding='utf-8'))
    with tarfile.open(R1 / 'ivr-full-flow-w0344-cpu-r1.tar.gz') as r1:
        r1_manifest = json.load(r1.extractfile('manifest.json'))
    with tarfile.open(archive) as tar:
        names = sorted(tar.getnames())
        assert names == ['cases.py', 'images/asterisk.tar', 'manifest.json', 'patch.json'], names
        assert all(m.isfile() and m.mode == 0o644 for m in tar.getmembers())
        change = json.load(tar.extractfile('patch.json'))
        manifest_bytes = tar.extractfile('manifest.json').read()
        cases = tar.extractfile('cases.py').read()
        image = hashlib.sha256()
        stream = tar.extractfile('images/asterisk.tar')
        for chunk in iter(lambda: stream.read(1024 * 1024), b''):
            image.update(chunk)
    manifest = json.loads(manifest_bytes)
    assert (evid / 'cpu-retry-manifest.json').read_bytes() == manifest_bytes
    assert change['REAL_CUSTOMER_CALL_ALLOWED'] == 'NO' and change['base_manifest_sha256'] == base_manifest_sha
    assert change['new_manifest_sha256'] == hashlib.sha256(manifest_bytes).hexdigest()
    assert image.hexdigest() == manifest['files']['images/asterisk.tar'] == r1_manifest['files']['images/asterisk.tar']
    assert manifest['images']['asterisk'] == r1_manifest['images']['asterisk'] != base['images']['asterisk']
    # The repo's driver moves on after a package is pinned; only the pinned bytes must be self-consistent.
    matches_repo = cases == (ROOT / 'deploy/lab/full-flow-s5/cases.py').read_bytes()
    assert hashlib.sha256(cases).hexdigest() == manifest['files']['cases.py']
    changed_files = sorted(k for k in set(base['files']) | set(manifest['files'])
                           if base['files'].get(k) != manifest['files'].get(k))
    assert changed_files == ['cases.py', 'images/asterisk.tar'], changed_files
    changed_images = sorted(k for k in base['images'] if base['images'][k] != manifest['images'][k])
    assert changed_images == ['asterisk'] and set(base) == set(manifest)
    assert all(base[k] == manifest[k] for k in base if k not in ('files', 'images'))
    old_text = (BASE / 'cases.py').read_text(encoding='utf-8')
    new_text = cases.decode('utf-8')
    removed_asserts = Counter(asserts(old_text)) - Counter(asserts(new_text))
    assert not removed_asserts, 'a base assertion was removed or changed: %r' % list(removed_asserts)[:3]
    added_asserts = list((Counter(asserts(new_text)) - Counter(asserts(old_text))).elements())
    diff = [line for line in difflib.unified_diff(old_text.splitlines(), new_text.splitlines(), 'base/cases.py', 'package/cases.py', lineterm='', n=0)]
    removed = [l for l in diff if l.startswith('-') and not l.startswith('---')]
    added = [l for l in diff if l.startswith('+') and not l.startswith('+++')]
    kit = OUT / 'full-flow-s5'
    rebuilt = {p.relative_to(kit).as_posix(): sha(p) for p in kit.rglob('*') if p.is_file() and p.name != 'manifest.json'}
    assert rebuilt == manifest['files'], 'reconstructed kit differs from the package manifest'
    assert (kit / 'manifest.json').read_bytes() == manifest_bytes
    ps1 = (DOC / 'retry-s5-full-flow.ps1').read_text(encoding='utf-8').replace('\r\n', '\n')
    assert 'Pins must name exactly one retry archive' in ps1 and 'cpu-r2.tar.gz' not in ps1
    blocks = {name: re.search(r'^\s*\$' + name + r" = @'\n(.*?)\n'@$", ps1, re.S | re.M)[1] + '\n'
              for name in ('start', 'follow')}
    commands = {'retry-command.sh': blocks['start'] + blocks['follow'],
                'resume-command.sh': 'cd /home/ssv/ivr-full-flow-w0344-cpu-20260923-000000-00000000 || exit 2\n' + blocks['follow']}
    for name, body in commands.items():
        (OUT / name).write_text(body, encoding='utf-8', newline='\n')
        subprocess.run([str(BASH), '-n', str(OUT / name)], check=True)
    installer = (DOC / 'install-s5-cpu-retry.py').read_text(encoding='utf-8')
    assert 'def find_patch(root)' in installer and 'cpu-r2.tar.gz' not in installer and '\r' not in installer
    r1_proof = json.loads((R1 / 'retry-verification.json').read_text(encoding='utf-8'))
    proof = {'status': 'PASS', 'package': pins['package'], 'archive_sha256': sha(archive), 'archive_bytes': archive.stat().st_size,
             'members': names, 'base_kit_manifest_sha256': base_manifest_sha, 'base_matches_s5_run_135115': True,
             'new_manifest_sha256': change['new_manifest_sha256'], 'changed_files_vs_base': changed_files,
             'changed_images_vs_base': changed_images, 'asterisk_image_bytes_same_as_r1': True,
             'asterisk_image': manifest['images']['asterisk'],
             'portable_build_carried_from_r1': {k: r1_proof[k] for k in ('portable_compile_commands', 'native_compile_commands', 'build_native_enabled', 'asterisk_binary_sha256')},
             'cases_py': {'base_sha256': sha(BASE / 'cases.py'), 'package_sha256': manifest['files']['cases.py'],
                          'assert_lines': len(asserts(new_text)), 'base_assert_lines_kept': True,
                          'assert_lines_added': added_asserts, 'matches_repo_cases_py': matches_repo,
                          'lines_removed': len(removed), 'lines_added': len(added), 'diff': diff},
             'reconstructed_files_match_manifest': len(rebuilt), 'retry_and_resume_bash_syntax': 'PASS',
             'installer_sha256': sha(DOC / 'install-s5-cpu-retry.py'), 'REAL_CUSTOMER_CALL_ALLOWED': 'NO'}
    (OUT / 'retry-verification.json').write_text(json.dumps(proof, indent=2, ensure_ascii=False) + '\n', encoding='utf-8', newline='\n')
    print('W0344_CPU_RETRY_VERIFIED package=%s files=%d asserts=%d added_asserts=%d diff=-%d/+%d repo_driver=%s'
          % (pins['package'], len(rebuilt), len(asserts(new_text)), len(added_asserts), len(removed), len(added),
             'same' if matches_repo else 'newer'))


if __name__ == '__main__':
    main()
