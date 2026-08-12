#!/usr/bin/env sh
set -eu

export LC_ALL="${LC_ALL:-C.UTF-8}"

repository_root=$(CDPATH= cd -- "$(dirname -- "$0")/../../.." && pwd)
pattern_file="$repository_root/deploy/ci/pii-patterns.txt"

if [ ! -f "$pattern_file" ]; then
  echo "PII pattern file not found: $pattern_file" >&2
  exit 2
fi

if [ "$#" -eq 0 ]; then
  set -- "$repository_root/docs/evidence" "$repository_root/ci-artifacts"
fi

file_list=$(mktemp)
match_log=$(mktemp)
raw_match=$(mktemp)
trap 'rm -f "$file_list" "$match_log" "$raw_match"' EXIT HUP INT TERM

for target in "$@"; do
  if [ -f "$target" ]; then
    printf '%s\n' "$target" >> "$file_list"
  elif [ -d "$target" ]; then
    find "$target" -type f \( \
      -name '*.txt' -o -name '*.log' -o -name '*.md' -o \
      -name '*.xml' -o -name '*.json' -o -name '*.yaml' -o \
      -name '*.yml' -o -name '*.csv' -o -name '*.trx' \
    \) -print >> "$file_list"
  fi
done

if [ ! -s "$file_list" ]; then
  echo "PII_SCAN_PASS files=0 locale=$LC_ALL"
  exit 0
fi

matched=0
scanned=0
while IFS= read -r file; do
  scanned=$((scanned + 1))
  : > "$raw_match"
  set +e
  grep -nE -f "$pattern_file" "$file" > "$raw_match"
  grep_status=$?
  set -e

  case "$grep_status" in
    0)
      matched=1
      sed "s|^\\([0-9][0-9]*\\):.*$|$file:\\1:[REDACTED]|" \
        "$raw_match" >> "$match_log"
      ;;
    1) ;;
    *) echo "PII scanner could not read $file" >&2; exit 2 ;;
  esac
done < "$file_list"

if [ "$matched" -eq 1 ]; then
  echo "PII_SCAN_FAIL locale=$LC_ALL" >&2
  cat "$match_log" >&2
  exit 1
fi

echo "PII_SCAN_PASS files=$scanned locale=$LC_ALL"
