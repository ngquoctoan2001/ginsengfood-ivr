#!/usr/bin/env sh
set -eu

repository_root=$(CDPATH= cd -- "$(dirname -- "$0")/../../.." && pwd)
artifact_root="${1:-$repository_root/ci-artifacts/dotnet/policy-selftest}"
fixture_project="$repository_root/deploy/ci/fixtures/failing-test/Ivr.CiFailingTests.csproj"
coverage_low="$repository_root/deploy/ci/fixtures/coverage/low"
policy_project="$repository_root/deploy/ci/tools/Ivr.CiPolicy/Ivr.CiPolicy.csproj"
vulnerability_fixtures="$repository_root/deploy/ci/fixtures/vulnerabilities"

mkdir -p "$artifact_root"

dotnet test "$fixture_project" --configuration Release --list-tests \
  > "$artifact_root/ct-ci-02-discovery.log" 2>&1
grep -F "CtCi02DeliberatelyFails" "$artifact_root/ct-ci-02-discovery.log" > /dev/null

set +e
dotnet test "$fixture_project" --configuration Release \
  > "$artifact_root/ct-ci-02-expected-failure.log" 2>&1
test_status=$?
set -e
if [ "$test_status" -eq 0 ] \
  || ! grep -F "CtCi02DeliberatelyFails" "$artifact_root/ct-ci-02-expected-failure.log" > /dev/null \
  || ! grep -F "CT-CI-02 expected failure" "$artifact_root/ct-ci-02-expected-failure.log" > /dev/null; then
  echo "CT-CI-02 did not observe the intended failing test" >&2
  exit 1
fi

set +e
dotnet test "$repository_root/deploy/ci/fixtures/failing-test/typo.csproj" \
  > "$artifact_root/ct-ci-02-invalid-path.log" 2>&1
typo_status=$?
set -e
if [ "$typo_status" -eq 0 ] \
  || grep -F "CtCi02DeliberatelyFails" "$artifact_root/ct-ci-02-invalid-path.log" > /dev/null; then
  echo "CT-CI-02 invalid-path control did not fail closed" >&2
  exit 1
fi

set +e
dotnet run --project "$policy_project" --configuration Release --no-build -- \
  coverage "$coverage_low" 60 \
  > "$artifact_root/ct-ci-03-expected-failure.log" 2>&1
coverage_status=$?
set -e
if [ "$coverage_status" -ne 1 ] \
  || ! grep -F "below the required 60.00%" "$artifact_root/ct-ci-03-expected-failure.log" > /dev/null \
  || ! grep -F "EXCLUDED_SOURCE_CLASSES=2" "$artifact_root/ct-ci-03-expected-failure.log" > /dev/null; then
  echo "CT-CI-03 did not observe the intended low-coverage failure" >&2
  exit 1
fi

set +e
dotnet run --project "$policy_project" --configuration Release --no-build -- \
  coverage "$repository_root/deploy/ci/fixtures/coverage/typo" 60 \
  > "$artifact_root/ct-ci-03-invalid-path.log" 2>&1
coverage_typo_status=$?
set -e
if [ "$coverage_typo_status" -ne 1 ] \
  || ! grep -F "No coverage.cobertura.xml report found" "$artifact_root/ct-ci-03-invalid-path.log" > /dev/null; then
  echo "CT-CI-03 invalid-path control did not fail closed" >&2
  exit 1
fi

dotnet run --project "$policy_project" --configuration Release --no-build -- \
  vulnerabilities "$vulnerability_fixtures/clean.json" high \
  > "$artifact_root/vulnerability-clean.log" 2>&1

assert_vulnerability_failure() {
  fixture=$1
  expected=$2
  log=$3
  set +e
  dotnet run --project "$policy_project" --configuration Release --no-build -- \
    vulnerabilities "$fixture" high > "$log" 2>&1
  status=$?
  set -e
  if [ "$status" -ne 1 ] || ! grep -F "$expected" "$log" > /dev/null; then
    echo "Vulnerability policy self-test failed for $fixture" >&2
    exit 1
  fi
}

assert_vulnerability_failure \
  "$vulnerability_fixtures/high.json" \
  "1 finding(s) at or above high" \
  "$artifact_root/vulnerability-high.log"
assert_vulnerability_failure \
  "$vulnerability_fixtures/empty-object.json" \
  "invalid or incomplete schema" \
  "$artifact_root/vulnerability-empty-object.log"
assert_vulnerability_failure \
  "$vulnerability_fixtures/empty-projects.json" \
  "invalid or incomplete schema" \
  "$artifact_root/vulnerability-empty-projects.log"
assert_vulnerability_failure \
  "$vulnerability_fixtures/malformed.json" \
  "not valid JSON" \
  "$artifact_root/vulnerability-malformed.log"
assert_vulnerability_failure \
  "$vulnerability_fixtures/unknown-severity.json" \
  "unknown or malformed severity" \
  "$artifact_root/vulnerability-unknown-severity.log"

echo "CT-CI-02 PASS — the discovered deliberate test failure, not any non-zero exit, is required"
echo "CT-CI-03 PASS — the measured low-coverage result, not a missing path, is required"
echo "CT-CI-09 PASS — NuGet vulnerability JSON is schema-validated and unknown severities fail closed"

# W-0294. Only testcase names change. Raw failure data must remain visible to the PII gate.
junit_fixture_root=$(mktemp -d)
case "$junit_fixture_root" in
  /tmp/tmp.*) ;;
  *) echo "Unexpected JUnit fixture temp root" >&2; exit 1 ;;
esac
trap 'rm -rf "$junit_fixture_root"' EXIT HUP INT TERM
mkdir -p "$junit_fixture_root/valid" "$junit_fixture_root/invalid"
cat > "$junit_fixture_root/valid/unit-test-result.xml" <<'EOF'
<testsuites><testsuite tests="4" failures="1" skipped="1" time="2.5">
<testcase classname="Example" name="Rejects(value: &quot;0901234567&quot;)" time="0.5"><properties><property name="TestId" value="CT-CI-JUNIT" /></properties><failure message="synthetic failure">raw-output-0901234567</failure></testcase>
<testcase classname="Example" name="Rejects(value: &quot;0901234567&quot;)" time="0.5"><skipped /></testcase>
<testcase classname="Example" name="Rejects(value: &quot;123 Đường Nguyễn Huệ&quot;)" time="0.5" />
<testcase classname="Example" name="PlainFact" time="1" />
<system-out>keep console output</system-out></testsuite></testsuites>
EOF
dotnet run --project "$policy_project" --configuration Release --no-build -- \
  junit "$junit_fixture_root/valid" > "$artifact_root/junit-valid.log" 2>&1
grep -F 'JUNIT_REPORT_PASS files=1 cases=4 renamed=3' "$artifact_root/junit-valid.log" > /dev/null
grep -F 'tests="4" failures="1" skipped="1" time="2.5"' "$junit_fixture_root/valid/unit-test-result.xml" > /dev/null
grep -F '<failure message="synthetic failure">raw-output-0901234567</failure>' "$junit_fixture_root/valid/unit-test-result.xml" > /dev/null
grep -F '<property name="TestId" value="CT-CI-JUNIT"' "$junit_fixture_root/valid/unit-test-result.xml" > /dev/null
grep -F '<skipped' "$junit_fixture_root/valid/unit-test-result.xml" > /dev/null
grep -F '<system-out>keep console output</system-out>' "$junit_fixture_root/valid/unit-test-result.xml" > /dev/null
grep -F 'name="PlainFact"' "$junit_fixture_root/valid/unit-test-result.xml" > /dev/null
test "$(grep -o 'name="Rejects\[[^"]*' "$junit_fixture_root/valid/unit-test-result.xml" | sort -u | wc -l | tr -d ' ')" -eq 3
cp "$junit_fixture_root/valid/unit-test-result.xml" "$junit_fixture_root/first.xml"
dotnet run --project "$policy_project" --configuration Release --no-build -- \
  junit "$junit_fixture_root/valid" > "$artifact_root/junit-idempotent.log" 2>&1
grep -F 'renamed=0' "$artifact_root/junit-idempotent.log" > /dev/null
cmp "$junit_fixture_root/first.xml" "$junit_fixture_root/valid/unit-test-result.xml"
set +e
sh "$repository_root/deploy/ci/scripts/scan-pii.sh" "$junit_fixture_root/valid" > "$artifact_root/junit-output-pii.log" 2>&1
junit_output_status=$?
set -e
test "$junit_output_status" -eq 1
grep -F 'PII_SCAN_FAIL' "$artifact_root/junit-output-pii.log" > /dev/null

for variant in malformed wrong-root empty missing-name dtd missing-directory empty-directory; do
  target="$junit_fixture_root/invalid"
  case "$variant" in
    malformed) printf '<testsuites>' > "$target/invalid-test-result.xml" ;;
    wrong-root) printf '<coverage />' > "$target/invalid-test-result.xml" ;;
    empty) printf '<testsuites><testsuite /></testsuites>' > "$target/invalid-test-result.xml" ;;
    missing-name) printf '<testsuites><testsuite><testcase classname="Example" /></testsuite></testsuites>' > "$target/invalid-test-result.xml" ;;
    dtd) printf '<!DOCTYPE testsuites [<!ENTITY data "synthetic">]><testsuites><testsuite><testcase classname="Example" name="&data;" /></testsuite></testsuites>' > "$target/invalid-test-result.xml" ;;
    missing-directory) target="$junit_fixture_root/missing" ;;
    empty-directory) target="$junit_fixture_root/empty"; mkdir "$target" ;;
  esac
  set +e
  dotnet run --project "$policy_project" --configuration Release --no-build -- \
    junit "$target" > "$artifact_root/junit-$variant.log" 2>&1
  junit_status=$?
  set -e
  if [ "$junit_status" -ne 1 ] || ! grep -F 'JUNIT_REPORT_FAIL' "$artifact_root/junit-$variant.log" > /dev/null; then
    echo "JUnit report self-test did not reject $variant" >&2
    exit 1
  fi
done
echo "CT-CI-JUNIT PASS — unique case IDs, unchanged results, idempotence, 7 refusals, and raw failure PII still rejected"
