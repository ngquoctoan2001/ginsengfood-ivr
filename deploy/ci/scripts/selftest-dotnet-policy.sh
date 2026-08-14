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
