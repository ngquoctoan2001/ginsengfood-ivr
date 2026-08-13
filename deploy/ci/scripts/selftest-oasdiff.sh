#!/bin/sh
set -eu

report_path="${1:-/tmp/ct-doc-02-breaking.txt}"

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

if ! grep -Eqi 'delete|deleted|removed|breaking' "$report_path"; then
  echo "CT-DOC-02 FAIL — oasdiff failed without a breaking-change diagnostic" >&2
  exit 1
fi

echo "CT-DOC-02 PASS — oasdiff rejected the removed operation fixture"
