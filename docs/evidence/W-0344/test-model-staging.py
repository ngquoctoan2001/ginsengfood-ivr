#!/usr/bin/env python3
"""Reproduce S5 run 20260923-093745 locally with Unix permissions, then show stage_models fixes it.

Docker Desktop ignores host permissions on Windows bind mounts, which is why every local
rehearsal passed. This test puts the real pinned models in a Docker volume with the S5 mirror's
layout (owner uid 1000, dirs 0700, files 0600), stages a copy with install-s5-cpu-retry.stage_models
running as uid 1000 under umask 077, and starts the real TTS image as its own user (uid 1654) twice:
once on the mirror layout, once on the staged copy. Expected: not_ready, then ready.
Needs Docker, ivr-tts:w0326 and .artifacts/W-0340/vieneu-w0340/models.
"""
import json
from pathlib import Path
import subprocess
import sys
import time
import uuid

ROOT = Path(__file__).resolve().parents[3]
IMAGE = 'ivr-tts:w0326'
KIT = ROOT / '.artifacts/W-0344/cpu-portable-r2/full-flow-s5'
MODELS = ROOT / '.artifacts/W-0340/vieneu-w0340/models'

PREPARE = r'''
import importlib.util, json, os, shutil, stat, sys
from pathlib import Path
shutil.copytree('/src-models', '/vol/mirror')
for base, dirs, files in os.walk('/vol/mirror'):
    os.chown(base, 1000, 1000); os.chmod(base, 0o700)
    for name in files:
        path = os.path.join(base, name); os.chown(path, 1000, 1000); os.chmod(path, 0o600)
os.mkdir('/vol/run'); os.chown('/vol/run', 1000, 1000); os.chmod('/vol/run', 0o700)
pid = os.fork()
if pid == 0:
    os.setgid(1000); os.setuid(1000); os.umask(0o077)
    spec = importlib.util.spec_from_file_location('retry', '/repo/docs/evidence/W-0344/install-s5-cpu-retry.py')
    retry = importlib.util.module_from_spec(spec); spec.loader.exec_module(retry)
    lock = json.loads(Path('/kit/fixtures/MODELS.lock').read_text(encoding='utf-8-sig'))
    print('STAGED', retry.stage_models(Path('/vol/mirror'), lock, Path('/vol/run/models-readable')))
    os._exit(0)
_, status = os.waitpid(pid, 0)
modes = sorted({oct(stat.S_IMODE(os.stat(os.path.join(b, f)).st_mode)) for b, _, fs in os.walk('/vol/run/models-readable') for f in fs})
print('STAGE_EXIT', status, 'FILE_MODES', modes, 'MIRROR_FILE_MODE', oct(stat.S_IMODE(os.stat('/vol/mirror/metadata/vieneu-model-card.md').st_mode)))
'''


def docker(*args, check=True, timeout=600):
    return subprocess.run(['docker', *args], capture_output=True, text=True, check=check, timeout=timeout)


def start_tts(volume, subpath, env):
    name = 'ivr-w0344-modelperm-' + uuid.uuid4().hex[:8]
    args = ['run', '-d', '--name', name, '--network', 'none', '--read-only', '--cap-drop', 'ALL',
            '--security-opt', 'no-new-privileges', '--cpus', '2', '--memory', '4g', '--memory-swap', '4g',
            '--tmpfs', '/tmp:rw,noexec,nosuid,nodev,size=64m,mode=0750,uid=1654,gid=1654',
            '--mount', f'type=volume,src={volume},dst=/models,readonly,volume-subpath={subpath}',
            '--mount', f'type=bind,src={KIT / "fixtures/voice-acceptance-manifest.json"},dst=/run/ivr-tts/voice-acceptance-manifest.json,readonly']
    for key, value in env.items():
        args += ['-e', f'{key}={value}']
    docker(*args, IMAGE)
    try:
        deadline = time.monotonic() + 240
        while time.monotonic() < deadline:
            out = docker('logs', name, check=False)
            logs = out.stdout + out.stderr
            line = next((l for l in logs.splitlines() if 'tts_event=startup' in l), None)
            if line:
                user = docker('inspect', '--format', '{{.Config.User}}', name).stdout.strip()
                return {'subpath': subpath, 'startup': line.split('status=')[1].split()[0], 'container_user': user or 'image default'}
            time.sleep(2)
        return {'subpath': subpath, 'startup': 'TIMEOUT'}
    finally:
        docker('rm', '-f', name, check=False)


def main():
    env = json.loads((KIT / 'compose.template.json').read_text(encoding='utf-8'))['services']['ivr-tts']['environment']
    volume = 'ivr-w0344-modelperm-' + uuid.uuid4().hex[:8]
    docker('volume', 'create', volume)
    try:
        prep = docker('run', '--rm', '--network', 'none', '--user', '0', '--entrypoint', 'python',
                      '--mount', f'type=bind,src={ROOT},dst=/repo,readonly', '--mount', f'type=bind,src={KIT},dst=/kit,readonly',
                      '--mount', f'type=bind,src={MODELS},dst=/src-models,readonly', '--mount', f'type=volume,src={volume},dst=/vol',
                      IMAGE, '-c', PREPARE)
        mirror = start_tts(volume, 'mirror', env)
        staged = start_tts(volume, 'run/models-readable', env)
    finally:
        docker('volume', 'rm', '-f', volume, check=False)
    result = {'prepare': prep.stdout.strip().splitlines(), 'mirror_layout': mirror, 'staged_copy': staged,
              'expected': {'mirror_layout': 'not_ready', 'staged_copy': 'ready'}}
    print(json.dumps(result, indent=2))
    if mirror['startup'] != 'not_ready' or staged['startup'] != 'ready':
        sys.exit(1)
    print('W0344_MODEL_STAGING_REPRO_PASS mirror=not_ready staged=ready')


if __name__ == '__main__':
    main()
