#!/usr/bin/env sh
set -eu

repository_root=$(CDPATH= cd -- "$(dirname -- "$0")/../../.." && pwd)
gitleaks_version="${GITLEAKS_VERSION:-8.30.0}"
archive="gitleaks_${gitleaks_version}_linux_x64.tar.gz"
checksums="gitleaks_${gitleaks_version}_checksums.txt"
release_base="https://github.com/gitleaks/gitleaks/releases/download/v${gitleaks_version}"
work_directory=$(mktemp -d)
negative_file="$repository_root/.ci-secret-selftest.env"
trap 'rm -rf "$work_directory"; rm -f "$negative_file"' EXIT HUP INT TERM

curl --fail --silent --show-error --location \
  "$release_base/$archive" \
  --output "$work_directory/$archive"
curl --fail --silent --show-error --location \
  "$release_base/$checksums" \
  --output "$work_directory/$checksums"

(
  cd "$work_directory"
  grep " $archive" "$checksums" | sha256sum --check --strict
  tar -xzf "$archive" gitleaks
)

dotnet restore "$repository_root/Ivr.sln" --locked-mode
dotnet list "$repository_root/Ivr.sln" package \
  --vulnerable --include-transitive --format json --no-restore \
  > "$work_directory/dotnet-vulnerabilities.json"
dotnet run \
  --project "$repository_root/deploy/ci/tools/Ivr.CiPolicy/Ivr.CiPolicy.csproj" \
  --configuration Release -- \
  vulnerabilities "$work_directory/dotnet-vulnerabilities.json" high

npm --prefix "$repository_root/admin-ui" audit --audit-level=high
npm --prefix "$repository_root/deploy/ci" audit --audit-level=high

mkdir -p "$work_directory/negative"
printf 'GITHUB_TOKEN=%s%s\n' \
  'ghp_123456789012345678' \
  '901234567890123456' \
  > "$work_directory/negative/fake.env"

set +e
"$work_directory/gitleaks" dir "$work_directory/negative" \
  --config "$repository_root/.gitleaks.toml" \
  --exit-code 42 --no-banner --redact
negative_status=$?
set -e

if [ "$negative_status" -ne 42 ]; then
  echo "CT-CI-04 expected Gitleaks exit 42, received $negative_status" >&2
  exit 1
fi
echo "CT-CI-04 PASS — fake GitHub PAT rejected"

if [ "${CI_SECRET_SELFTEST:-0}" = "1" ]; then
  printf 'GITHUB_TOKEN=%s%s\n' \
    'ghp_123456789012345678' \
    '901234567890123456' \
    > "$negative_file"
  "$work_directory/gitleaks" dir "$repository_root" \
    --config "$repository_root/.gitleaks.toml" \
    --no-banner --redact
fi

"$work_directory/gitleaks" git "$repository_root" \
  --config "$repository_root/.gitleaks.toml" \
  --no-banner --redact

echo "SECURITY_SCAN_PASS gitleaks=$gitleaks_version nuget=HIGH npm=HIGH"
