<#
.SYNOPSIS
    Runs the OD-18 data preflight against one environment and prints evidence, read-only.

.DESCRIPTION
    W-0123 removed the IVR-side trusted skip without dropping a column, an enum value or a
    lifecycle status, because no target database was reachable to count what depended on them.
    The gate was recorded ENV_BLOCKED rather than assumed empty. This script is how that gate
    closes: point it at an environment, paste the output into the evidence pack, and the
    assumption becomes a number.

    Every statement in the .sql file is a SELECT and this script adds none of its own. It never
    writes, and it must never be edited into something that does.

    The query is verified on every test run: IT-M3-AUTHORITY-13 executes the same file against a
    migrated PostgreSQL schema, so a wrong column name or a text-vs-jsonb comparison fails in CI
    rather than in front of whoever finally has production credentials.

.PARAMETER ConnectionString
    A libpq connection string or URI, e.g. "postgresql://reader@host:5432/ivr".
    Prefer a READ-ONLY role. Do not pass a password on the command line — use PGPASSWORD, a
    .pgpass file, or your platform's secret store, so the value never reaches shell history.

.PARAMETER Environment
    Free-text label recorded in the output header, e.g. staging or production. The counts are
    meaningless in the evidence pack without knowing which environment produced them.

.EXAMPLE
    $env:PGPASSWORD = (Get-Secret ivr-readonly)
    ./tools/ops/Invoke-Od18Preflight.ps1 `
        -ConnectionString "postgresql://ivr_reader@db.internal:5432/ivr" `
        -Environment staging
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $ConnectionString,

    [Parameter(Mandatory = $true)]
    [string] $Environment
)

$ErrorActionPreference = "Stop"
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "../..")).Path
$sqlPath = Join-Path $PSScriptRoot "od18-legacy-skip-preflight.sql"

if (-not (Test-Path -LiteralPath $sqlPath)) {
    throw "Preflight query not found at $sqlPath."
}

$psql = Get-Command psql -ErrorAction SilentlyContinue
if ($null -eq $psql) {
    # Deliberately not falling back to a .NET client. A preflight that silently changes how it
    # connects also silently changes what it is allowed to do; a missing tool should send the
    # operator to a decision, not to a different code path.
    throw @"
psql was not found on PATH.

Run the query with whatever client the environment sanctions instead:
    $sqlPath

Then record the results in the same shape this script prints, so the evidence pack stays
comparable across environments.
"@
}

Write-Output "=== OD18_PREFLIGHT ==="
Write-Output "environment=$Environment"
Write-Output "query=$([IO.Path]::GetRelativePath($repositoryRoot, $sqlPath) -replace '\\', '/')"
Write-Output "query_sha256=$((Get-FileHash -LiteralPath $sqlPath -Algorithm SHA256).Hash.ToLowerInvariant())"
Write-Output "run_at_utc=$([DateTimeOffset]::UtcNow.ToString('o'))"
Write-Output ""

# --tuples-only + --no-align keep the output diffable and pasteable; ON_ERROR_STOP means a failed
# statement ends the run instead of leaving a partial set of counts that reads like a complete one.
& $psql.Source `
    --dbname $ConnectionString `
    --file $sqlPath `
    --tuples-only `
    --no-align `
    --field-separator '=' `
    --set ON_ERROR_STOP=1

if ($LASTEXITCODE -ne 0) {
    throw "psql exited with $LASTEXITCODE. No counts were recorded; do not report a partial run as a preflight."
}

Write-Output ""
Write-Output @"
Next steps:
  - Record migration_count, migration_latest and migration_inventory with the deployment evidence.
    Any mismatch with the approved target manifest is SCHEMA_DRIFT; stop before interpreting rows.
  - task_legacy_column_count must be 5, job_legacy_column_count must be 3 and
    legacy_constraint_count must be 3. All three constraint-presence metrics must be true. Any
    other result is SCHEMA_DRIFT; do not rename a migration or repair the target from this script.
  - tasks_with_retired_decision > 0  => the enum value must stay in the check constraint. Any work
    item proposing to remove it has to state a retention and archive plan first.
  - jobs_in_skipped_status > jobs_skipped_status_from_trusted_skip => the difference is unrelated
    lifecycle history. Do not count it as trusted-skip evidence.
  - tasks_matching_retired_skip_shape > 0 => Module 3 has been sending the retired shape. Cross-
    check against ivr_legacy_skip_candidate_total after deploy and answer OD18-C1 in
    plan/ivr-orther/questions-to-module-3-od18-authority.md with these numbers attached.
  - All zero => record it. A measured zero is what W-0123 was missing; it is not the same claim as
    an untested assumption that happened to be right.
"@
