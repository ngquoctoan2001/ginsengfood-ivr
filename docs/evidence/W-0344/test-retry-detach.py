#!/usr/bin/env python3
"""Exercise the remote bash of retry-s5-full-flow.ps1 in a Debian container with a fake installer.

Six scenarios, nine checks: a normal run (progress streams, IVR_FLOW_PROGRAM reaches the installer),
a failing installer, an SSH drop (SIGHUP then SIGKILL of the foreground session group) followed by
-ResumeRunId, a killed test that resume must report instead of waiting forever, a second start in
the same directory, and a wrong installer pin.
Needs Docker and the local image ivr-w0344-asterisk:22.10.1-portable-r1 (bash, setsid, GNU tail).
"""
import json
from pathlib import Path
import re
import subprocess
import sys
import tempfile

ROOT = Path(__file__).resolve().parents[3]
PS1 = ROOT / 'docs/evidence/W-0344/retry-s5-full-flow.ps1'
IMAGE = 'ivr-w0344-asterisk:22.10.1-portable-r1'

FAKE_INSTALLER = r'''#!/bin/bash
# Stands in for "python3 -B ./install-s5-cpu-retry.py": progress, a receipt, a chosen exit code.
echo "FAKE_INSTALLER $* program=${IVR_FLOW_PROGRAM:-unset}"
for i in $(seq 1 "${FAKE_SLEEP:-1}"); do echo "W0344_PROGRESS seconds=$i"; sleep 1; done
if [ -z "${FAKE_NO_RESULT:-}" ]; then mkdir -p result && echo '{}' > result/summary.json && tar -czf result.tar.gz result; fi
exit "${FAKE_EXIT:-0}"
'''

RUNNER = r'''#!/bin/bash
set -u
export PATH=/t/bin:$PATH
make_run() {  # $1 = pin override (optional)
  local d; d=$(mktemp -d /tmp/ivr-full-flow-w0344-cpu-XXXXXX)
  cp /t/stub-installer "$d/install-s5-cpu-retry.py"
  local pin; pin=${1:-$(sha256sum "$d/install-s5-cpu-retry.py" | cut -d' ' -f1)}
  sed -e "s#__REMOTE__#$d#" -e "s#__PIN__#$pin#" -e "s#__BASE__#/tmp/base-kit#" -e "s#__PROGRAM__#TWENTY_FOUR_SEVEN#" /t/start.tpl > "$d.start"
  printf '\n' >> "$d.start"; cat /t/follow.sh >> "$d.start"
  echo "$d"
}
members() { tar -tzf "$1/receipt-download.tar.gz" 2>/dev/null | sort | tr '\n' ',' ; }
report() { echo "RESULT $1 exit=$2 status=$(cat "$3/run.status" 2>/dev/null || echo none) members=$(members "$3")"; }

d=$(make_run); FAKE_SLEEP=2 FAKE_EXIT=0 bash "$d.start" > "$d.out" 2>&1; report normal $? "$d"
grep -q 'W0344_PROGRESS seconds=2' "$d.out" && echo "RESULT normal_progress_streamed yes"
grep -q 'program=TWENTY_FOUR_SEVEN' "$d/launcher.log" && echo "RESULT program_reaches_installer yes"

d=$(make_run); FAKE_SLEEP=1 FAKE_EXIT=1 FAKE_NO_RESULT=1 bash "$d.start" > "$d.out" 2>&1; report failing $? "$d"

d=$(make_run); FAKE_SLEEP=8 setsid bash "$d.start" > "$d.out" 2>&1 &
fg=$!; sleep 3; kill -HUP -- "-$fg" 2>/dev/null; sleep 1; kill -KILL -- "-$fg" 2>/dev/null; wait "$fg" 2>/dev/null
alive=no; kill -0 "$(cat "$d/run.pid")" 2>/dev/null && alive=yes
echo "RESULT drop_test_alive_after_hangup $alive"
(cd "$d" && bash /t/follow.sh > "$d.resume" 2>&1); report drop_resume $? "$d"

d=$(make_run); FAKE_SLEEP=40 setsid bash "$d.start" > "$d.out" 2>&1 &
fg=$!; sleep 3; kill -KILL -- "-$fg" 2>/dev/null; wait "$fg" 2>/dev/null
kill -KILL -- "-$(cat "$d/run.pid")" 2>/dev/null; sleep 1
(cd "$d" && bash /t/follow.sh > "$d.resume" 2>&1); code=$?
grep -q W0344_RETRY_INTERRUPTED "$d.resume" && report killed_resume "$code" "$d"

d=$(make_run); FAKE_SLEEP=1 bash "$d.start" > /dev/null 2>&1; FAKE_SLEEP=1 bash "$d.start" > "$d.again" 2>&1; code=$?
grep -q W0344_RETRY_ALREADY_STARTED "$d.again" && report second_start "$code" "$d"

d=$(make_run 0000000000000000000000000000000000000000000000000000000000000000); bash "$d.start" > "$d.out" 2>&1; code=$?
started=no; test -e "$d/run.started" && started=yes
echo "RESULT wrong_pin exit=$code started=$started"
'''

EXPECTED = {
    'normal': 'exit=0 status=0 members=launcher.log,result.tar.gz,result.tar.gz.sha256,',
    'normal_progress_streamed': 'yes',
    'program_reaches_installer': 'yes',
    'failing': 'exit=1 status=1 members=launcher.log,',
    'drop_test_alive_after_hangup': 'yes',
    'drop_resume': 'exit=0 status=0 members=launcher.log,result.tar.gz,result.tar.gz.sha256,',
    'killed_resume': 'exit=5 status=none members=',
    'second_start': 'exit=3 status=0 members=launcher.log,result.tar.gz,result.tar.gz.sha256,',
    'wrong_pin': 'exit=2 started=no',
}


def block(text, name):
    match = re.search(r'^\s*\$' + name + r" = @'\n(.*?)\n'@$", text, re.S | re.M)
    if not match:
        raise SystemExit('missing here-string $' + name)
    return match[1] + '\n'


def main():
    text = PS1.read_text(encoding='utf-8').replace('\r\n', '\n')
    with tempfile.TemporaryDirectory() as temp:
        t = Path(temp)
        (t / 'bin').mkdir()
        files = {'bin/python3': FAKE_INSTALLER, 'run.sh': RUNNER, 'stub-installer': '# pinned stand-in\n',
                 'start.tpl': block(text, 'start'), 'follow.sh': block(text, 'follow')}
        for name, content in files.items():
            (t / name).write_text(content, encoding='utf-8', newline='\n')
        run = subprocess.run(['docker', 'run', '--rm', '--network', 'none', '--entrypoint', 'bash',
                              '--mount', f'type=bind,src={t},dst=/t,readonly', IMAGE, '/t/run.sh'],
                             capture_output=True, text=True, timeout=300)
    results = dict(line.split(' ', 2)[1:] for line in run.stdout.splitlines() if line.startswith('RESULT '))
    failures = {k: (results.get(k), v) for k, v in EXPECTED.items() if results.get(k) != v}
    print(json.dumps({'results': results, 'failures': failures, 'stderr': run.stderr[-800:]}, indent=2))
    if failures or run.returncode:
        sys.exit(1)
    print('W0344_RETRY_DETACH_TESTS_PASS cases=%d' % len(EXPECTED))


if __name__ == '__main__':
    main()
