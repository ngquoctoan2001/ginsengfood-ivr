#!/usr/bin/env bash
set -euo pipefail
umask 077
root="$(cd -- "$(dirname -- "$0")" && pwd -P)"
[[ "$(hostname -s)" == vps61 ]] || { echo 'REFUSED: expected vps61'; exit 2; }
[[ "$root" =~ ^/home/ssv/ivr-full-flow-w0344-[0-9]{8}-[0-9]{6}-[a-f0-9]{8}$ ]] || { echo 'REFUSED: unexpected run directory'; exit 2; }
cd "$root"
sha256sum -c ivr-full-flow-w0344.tar.gz.sha256
[[ ! -e full-flow-s5 && ! -e result ]] || { echo 'REFUSED: run directory already used'; exit 2; }
python3 - <<'PY'
import pathlib, tarfile
root = pathlib.Path.cwd()
with tarfile.open('ivr-full-flow-w0344.tar.gz') as archive:
    for member in archive.getmembers():
        path = pathlib.PurePosixPath(member.name)
        if path.is_absolute() or '..' in path.parts or not path.parts or path.parts[0] != 'full-flow-s5':
            raise ValueError('Archive path escape')
        if not (member.isdir() or member.isfile()):
            raise ValueError('Archive links/devices are forbidden')
    archive.extractall(root)
PY
# Keep immutable fixtures readable by UID1654 inside read-only bind mounts.
chmod 755 full-flow-s5 full-flow-s5/fixtures full-flow-s5/images
find full-flow-s5 -type f -exec chmod 644 {} +
echo 'S5 full-flow: 7 synthetic cases; one worker; TTS 2CPU/4GiB; no real calls.'
set +e
python3 -B full-flow-s5/launcher.py \
  --models /home/ssv/ivr-artifact-mirror/releases/vieneu-w0340/models \
  --output "$root/result" --run 2>&1 | tee launcher.log
status=${PIPESTATUS[0]}
set -e
if [[ -f result.tar.gz ]]; then
  sha256sum result.tar.gz > result.tar.gz.sha256
  echo "W0344_RECEIPT_READY $root/result.tar.gz"
else
  echo 'W0344_NO_RECEIPT: preserve launcher.log for diagnosis'
fi
exit "$status"
