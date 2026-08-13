#!/usr/bin/env sh
set -eu

repository_root=$(CDPATH= cd -- "$(dirname -- "$0")/../../.." && pwd)
pattern_file="$repository_root/deploy/ci/pii-patterns.txt"
scanner="$repository_root/deploy/ci/scripts/scan-pii.sh"
fixture_root=$(mktemp -d)
trap 'rm -rf "$fixture_root"' EXIT HUP INT TERM

bad_fixture="$fixture_root/pii.txt"
good_fixture="$fixture_root/safe.txt"

cat > "$bad_fixture" <<'EOF'
0901234567
+84901234567
84901234567
123 đường Nguyễn Huệ
123 Đường Nguyễn Huệ
123 ĐƯỜNG NGUYỄN HUỆ
123 duong Nguyen Hue
123 DUONG NGUYEN HUE
123 ĐưỜnG Nguyễn Huệ
Số nhà 45
SỐ NHÀ 45
SO NHA 45
Số NHÀ 45
Ngõ 12
NGÕ 12
Hẻm 9
HẺM 9
Ngách 3
NGÁCH 3
NGACH 3
nGáCh 3
Thôn Bình An
Ấp Một
Tổ 8
DIAL_TOKEN: abc12345xyz
"dial_token": "json12345token"
dial-token=equals12345token
EOF

cat > "$good_fixture" <<'EOF'
560000 VND
Quận 7
Quan 7
Phường Bến Nghé
Thành phố Thủ Đức
IVR bootstrap accepted
OpenAPI parse completed
Markdown map generated
<method name="get_Dial_token" signature="()" />
EOF

expected_bad_lines=$(wc -l < "$bad_fixture" | tr -d ' ')

for locale in C C.UTF-8 POSIX; do
  actual_bad_lines=$(LC_ALL="$locale" grep -nE -f "$pattern_file" "$bad_fixture" | wc -l | tr -d ' ')
  if [ "$actual_bad_lines" -ne "$expected_bad_lines" ]; then
    echo "CT-CI-06d failed for $locale: $actual_bad_lines/$expected_bad_lines" >&2
    exit 1
  fi

  if LC_ALL="$locale" grep -nE -f "$pattern_file" "$good_fixture"; then
    echo "CT-CI-06b false positive under $locale" >&2
    exit 1
  fi
done

if LC_ALL=C.UTF-8 sh "$scanner" "$bad_fixture"; then
  echo "CT-CI-06 expected scanner failure for PII fixture" >&2
  exit 1
fi

LC_ALL=C.UTF-8 sh "$scanner" "$good_fixture"

artifact_root="$fixture_root/ci-artifacts/dotnet/test-output"
mkdir -p "$artifact_root"
printf '123 Đường Nguyễn Huệ\n' > "$artifact_root/foreign-job.log"
if LC_ALL=C.UTF-8 sh "$scanner" "$fixture_root/ci-artifacts"; then
  echo "CT-CI-06c expected cross-job artifact PII to fail" >&2
  exit 1
fi

sql_root="$fixture_root/sql-artifact"
mkdir -p "$sql_root"
printf "INSERT INTO evidence(note) VALUES ('0901234567');\n" > "$sql_root/result.sql"
if LC_ALL=C.UTF-8 sh "$scanner" "$sql_root"; then
  echo "CT-CI-06h expected SQL artifact PII to fail" >&2
  exit 1
fi

extensionless_root="$fixture_root/extensionless-artifact"
mkdir -p "$extensionless_root"
printf 'DIAL_TOKEN: extensionless12345token\n' > "$extensionless_root/evidence"
if LC_ALL=C.UTF-8 sh "$scanner" "$extensionless_root"; then
  echo "CT-CI-06h expected extensionless artifact PII to fail" >&2
  exit 1
fi

set +e
LC_ALL=C.UTF-8 sh "$scanner" "$fixture_root/missing-target" > /dev/null 2>&1
missing_status=$?
set -e
if [ "$missing_status" -ne 2 ]; then
  echo "CT-CI-06h expected a missing target to fail closed with exit 2" >&2
  exit 1
fi

binary_root="$fixture_root/binary-only"
mkdir -p "$binary_root"
printf '\000\001\002' > "$binary_root/screenshot.png"
set +e
LC_ALL=C.UTF-8 sh "$scanner" "$binary_root" > /dev/null 2>&1
binary_status=$?
set -e
if [ "$binary_status" -ne 2 ]; then
  echo "CT-CI-06h expected a target with no text artifacts to fail closed" >&2
  exit 1
fi

echo "CT-CI-06 PASS — phone/address/token fixtures rejected"
echo "CT-CI-06b PASS — uppercase cases rejected; safe cases accepted"
echo "CT-CI-06c PASS — downloaded-artifact simulation rejected"
echo "CT-CI-06d PASS — C, C.UTF-8, and POSIX results identical"
echo "CT-CI-06e PASS — unaccented address cases rejected"
echo "CT-CI-06f PASS — mixed-case Vietnamese cases rejected"
echo "CT-CI-06g PASS — Cobertura dial-token symbol is not treated as a token value"
echo "CT-CI-06h PASS — SQL and extensionless text are scanned; missing/empty-text targets fail closed"
