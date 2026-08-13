#!/usr/bin/env sh
set -eu

if [ "$#" -ne 3 ]; then
  echo "usage: generate-oasdiff-changelog.sh <base> <revision> <output>" >&2
  exit 2
fi

raw_output=$(mktemp)
trap 'rm -f "$raw_output"' EXIT HUP INT TERM

oasdiff changelog "$1" "$2" --format markdown > "$raw_output"
awk '
  { lines[NR] = $0 }
  END {
    last = NR
    while (last > 0 && lines[last] == "") {
      last--
    }
    for (line = 1; line <= last; line++) {
      print lines[line]
    }
  }
' "$raw_output" > "$3"
