#!/usr/bin/env python3
"""Reproduce why the SIP peer could not read its config on S5 (run 20260923-103255-bf2e4b7c).

On S5, cases.py wrote the peer's pjsip.conf under umask 077: mode 0600, owner ssv (uid 1000). The
peer runs the Asterisk image as root with --cap-drop ALL, so without CAP_DAC_OVERRIDE it cannot
read another user's 0600 file. Docker Desktop on Windows hides this. The test builds both modes in
a Docker volume with real Unix ownership and reads them the way the peer does.
Expected: 0600 is refused, and 0644 (what cases.py now sets) is readable.
Needs Docker and ivr-w0344-asterisk:22.10.1-portable-r1.
"""
import json
import subprocess
import sys
import uuid

IMAGE = 'ivr-w0344-asterisk:22.10.1-portable-r1'
PREPARE = ('set -e; for m in 0600 0644; do mkdir -p /vol/peer-$m; printf "[transport-udp]\\ntype=transport\\n" > /vol/peer-$m/pjsip.conf; '
           'chown -R 1000:1000 /vol/peer-$m; chmod 0755 /vol/peer-$m; chmod $m /vol/peer-$m/pjsip.conf; done; stat -c "%a %u:%g %n" /vol/peer-*/pjsip.conf')


def docker(*args):
    return subprocess.run(['docker', *args], capture_output=True, text=True, timeout=300)


def read_as_peer(volume, mode):
    # Same identity and privileges as the peer container in cases.py: image user (root), no capabilities.
    run = docker('run', '--rm', '--network', 'none', '--cap-drop', 'ALL', '--security-opt', 'no-new-privileges',
                 '--mount', f'type=volume,src={volume},dst=/etc/asterisk-peer,readonly,volume-subpath=peer-{mode}',
                 '--entrypoint', 'bash', IMAGE, '-c', 'id -u; cat /etc/asterisk-peer/pjsip.conf')
    return {'mode': mode, 'exit': run.returncode, 'uid': run.stdout.split('\n', 1)[0],
            'readable': run.returncode == 0 and 'transport-udp' in run.stdout,
            'error': run.stderr.strip()[-120:]}


def main():
    volume = 'ivr-w0344-peerperm-' + uuid.uuid4().hex[:8]
    docker('volume', 'create', volume)
    try:
        prep = docker('run', '--rm', '--network', 'none', '--user', '0', '--mount', f'type=volume,src={volume},dst=/vol',
                      '--entrypoint', 'bash', IMAGE, '-c', PREPARE)
        results = [read_as_peer(volume, '0600'), read_as_peer(volume, '0644')]
    finally:
        docker('volume', 'rm', '-f', volume)
    print(json.dumps({'prepared': prep.stdout.split(), 'reads': results}, indent=2))
    if results[0]['readable'] or not results[1]['readable']:
        sys.exit(1)
    print('W0344_PEER_CONFIG_PERMISSION_REPRO_PASS 0600=refused 0644=readable')


if __name__ == '__main__':
    main()
