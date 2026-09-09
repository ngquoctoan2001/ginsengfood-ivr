#!/bin/sh
set -eu

report_path="${1:-/tmp/ct-doc-02-breaking.txt}"
control_path="${report_path}.control"
trap 'rm -f "$control_path"' EXIT HUP INT TERM

# W-0250: without this the missing-binary case falls through to the control check below and is
# reported as "unchanged contract was reported as breaking" - a confident, wrong diagnosis about
# the contract when the truth is that the tool is absent. CI runs this inside the pinned oasdiff
# image; on a developer host it usually is not installed.
if ! command -v oasdiff >/dev/null 2>&1; then
  echo "CT-DOC-02 FAIL - oasdiff is not on PATH; run this in the pinned oasdiff image" >&2
  exit 1
fi

if ! oasdiff breaking \
  deploy/ci/fixtures/openapi/docs-base.yaml \
  deploy/ci/fixtures/openapi/docs-base.yaml \
  --format text --fail-on WARN > "$control_path" 2>&1; then
  echo "CT-DOC-02 FAIL — unchanged contract was reported as breaking" >&2
  exit 1
fi

set +e
oasdiff breaking \
  deploy/ci/fixtures/openapi/docs-base.yaml \
  deploy/ci/fixtures/openapi/docs-breaking.yaml \
  --format text --fail-on WARN > "$report_path" 2>&1
status=$?
set -e

if [ "$status" -eq 0 ]; then
  echo "CT-DOC-02 FAIL — fixture breaking change was not detected" >&2
  exit 1
fi

if ! grep -Fq '[api-path-removed-without-deprecation]' "$report_path" \
  || ! grep -Fq 'API GET /tasks/{taskId}' "$report_path"; then
  echo "CT-DOC-02 FAIL — oasdiff failed without the expected removed-route diagnostic" >&2
  exit 1
fi

echo "CT-DOC-02 PASS — oasdiff rejected the removed operation fixture"
