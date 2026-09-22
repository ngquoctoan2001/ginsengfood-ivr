#!/usr/bin/env python3
"""Read-only artifact-store verification and bounded restore proof; never starts services."""
import argparse
import datetime
import hashlib
import json
import os
from pathlib import Path
import platform
import shutil
import socket
import sys

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--expected-host', default='vps61')
parser.add_argument('--output', required=True, type=Path)
parser.add_argument('--local-test', action='store_true')
args = parser.parse_args()
root = Path(__file__).resolve().parent
scope = 'LOCAL_MIRROR_TEST' if args.local_test else 'S5_INTERNAL_ARTIFACT_STORE'
if not args.local_test and (platform.system() != 'Linux' or socket.gethostname() != args.expected_host):
    sys.exit('MIRROR_STOP: unexpected target host')
output = args.output.resolve()
if output == root or root in output.parents:
    sys.exit('MIRROR_STOP: output must be outside immutable release directory')
catalog = json.loads((root / 'catalog.json').read_text(encoding='utf-8'))
if catalog['REAL_CUSTOMER_CALL_ALLOWED'] != 'NO' or catalog['release'] != 'vieneu-w0340':
    sys.exit('MIRROR_STOP: release/production boundary mismatch')
if catalog['profile'] != {'tts_cpus':2,'tts_memory_gib':4,'segment_ms':30000,'queue_ms':90000,'preparation_ms':120000,'queue_limit':8}:
    sys.exit('MIRROR_STOP: profile drift')
expected = set(catalog['files']) | {'catalog.json','SHA256SUMS'}
actual = {p.relative_to(root).as_posix() for p in root.rglob('*') if p.is_file()}
if actual != expected or any(p.is_symlink() for p in root.rglob('*')):
    sys.exit('MIRROR_STOP: extra, missing or linked artifact')
checked = []
for relative, entry in catalog['files'].items():
    path = (root / relative).resolve()
    if root not in path.parents:
        sys.exit('MIRROR_STOP: artifact path escapes release')
    if path.stat().st_size != entry['bytes']:
        sys.exit('MIRROR_STOP: artifact size mismatch: ' + relative)
    digest = hashlib.sha256(path.read_bytes()).hexdigest()
    if digest != entry['sha256']:
        sys.exit('MIRROR_STOP: artifact hash mismatch: ' + relative)
    checked.append({'path':relative,'bytes':entry['bytes'],'sha256':digest})
if output.exists():
    sys.exit('MIRROR_STOP: use a new output directory; prior evidence is preserved')
os.umask(0o077)
output.mkdir(parents=True,exist_ok=False)
restore = output / 'restore-proof'
restore.mkdir()
for entry in checked:
    destination = restore / entry['path']
    destination.parent.mkdir(parents=True,exist_ok=True)
    shutil.copyfile(root / entry['path'], destination)
    if hashlib.sha256(destination.read_bytes()).hexdigest() != entry['sha256']:
        sys.exit('MIRROR_STOP: restored artifact hash mismatch')
receipt = {'scope':scope,'host':socket.gethostname(),'verified_utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),
    'release_root':str(root),'catalog_sha256':hashlib.sha256((root/'catalog.json').read_bytes()).hexdigest(),
    'files_verified':len(checked),'restore_files_verified':len(checked),'files':checked,'profile':catalog['profile'],
    'REAL_CUSTOMER_CALL_ALLOWED':'NO','S2':'OWNER_RISK_ACCEPTED_LICENSE_EVIDENCE_PENDING',
    'production':'BLOCKED','no_container_started':True,'transport':'owner SSH/SFTP; no OCI registry installed'}
(output/'receipt.json').write_text(json.dumps(receipt,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
# Returned evidence remains small. Restored image/model bytes stay local for recovery inspection.
shutil.copyfile(root/'catalog.json',output/'catalog.json')
print('MIRROR_FILES_AND_RESTORE_VERIFIED ' + json.dumps({'scope':scope,'files':len(checked),'production':'BLOCKED'}))
