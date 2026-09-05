<#
.SYNOPSIS
    Exact-commit two-binary PostgreSQL backup/restore, overlap, rollback and forward-recovery drill.
.DESCRIPTION
    Requires a clean committed candidate, Docker (Linux containers), .NET 10 and the repository
    dotnet tool manifest. Starts only loopback MOCK APIs; never a worker or external provider.
    Uses a dedicated disposable container, not the developer's database. Keeps logs, dump and
    previous source checkout under ci-artifacts/expand-contract for inspection.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $PreviousRef
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
$runId = [Guid]::NewGuid().ToString('N')
$runRoot = Join-Path $repositoryRoot "ci-artifacts/expand-contract/$runId"
$previousTree = Join-Path $runRoot 'previous-source'
$containerName = "ivr-expand-$runId"
$processes = [System.Collections.Generic.List[System.Diagnostics.Process]]::new()
$checks = [System.Collections.Generic.List[string]]::new()
$containerCreated = $false

function Invoke-Checked([string] $File, [string[]] $Arguments) {
    & $File @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$File failed with exit code $LASTEXITCODE" }
}

function Get-FreePort {
    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, 0)
    $listener.Start()
    try { return $listener.LocalEndpoint.Port } finally { $listener.Stop() }
}

function Invoke-Sql([string] $Database, [string] $Sql) {
    $result = & docker exec $containerName psql -X -v ON_ERROR_STOP=1 -U ivr_expand -d $Database -Atc $Sql
    if ($LASTEXITCODE -ne 0) { throw "PostgreSQL assertion failed for $Database" }
    return ($result -join "`n").Trim()
}

function Start-Api([string] $Tree, [string] $Label) {
    $port = Get-FreePort
    $project = Join-Path $Tree 'src/Ivr.Api'
    $env:ASPNETCORE_URLS = "http://127.0.0.1:$port"
    $process = Start-Process -FilePath dotnet -WindowStyle Hidden -PassThru `
        -WorkingDirectory $project -ArgumentList @('bin/Release/net10.0/Ivr.Api.dll') `
        -RedirectStandardOutput (Join-Path $runRoot "$Label.stdout.log") `
        -RedirectStandardError (Join-Path $runRoot "$Label.stderr.log")
    $processes.Add($process)
    $api = @{ Process = $process; Url = $env:ASPNETCORE_URLS; Label = $Label }
    $deadline = [DateTime]::UtcNow.AddSeconds(45)
    do {
        if ($process.HasExited) { throw "$Label exited; inspect its local logs" }
        try {
            $response = Invoke-WebRequest "$($api.Url)/health/live" -TimeoutSec 2 -SkipHttpErrorCheck
            if ($response.StatusCode -eq 200) { return $api }
        } catch { }
        Start-Sleep -Milliseconds 300
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "$Label did not become live in 45 seconds"
}

function Stop-Api($Api) {
    if (!$Api.Process.HasExited) {
        Stop-Process -Id $Api.Process.Id -Force
        $Api.Process.WaitForExit(5000) | Out-Null
    }
}

function Assert-Ready($Api, [int] $Expected) {
    $response = Invoke-WebRequest "$($Api.Url)/health/ready" -TimeoutSec 10 -SkipHttpErrorCheck
    if ($response.StatusCode -ne $Expected) { throw "$($Api.Label) readiness expected $Expected, got $($response.StatusCode)" }
    if ($Expected -eq 503 -and $response.Content -notmatch 'schema_behind') { throw 'Expected schema_behind' }
}

function Assert-Functional($Api) {
    Assert-Ready $Api 200
    $headers = @{
        Authorization = 'Bearer dev-admin-write-token-not-a-real-secret'
        'X-Service-Scope' = 'ivr.admin.write'
        'X-Actor-Id' = 'expand-contract-fixture'
        'X-Correlation-Id' = "$runId-$($Api.Label)"
        'Idempotency-Key' = "$runId-$($Api.Label)"
    }
    $body = @{ reason = 'Synthetic PostgreSQL compatibility drill'; evidence_ref = "evidence://expand/$runId"; rebase_windows = $true } | ConvertTo-Json
    $root = "$($Api.Url)/v1/ivr/order-confirmation/dev"
    $seed = Invoke-RestMethod "$root/seed:load" -Method Post -Headers $headers -ContentType application/json -Body $body -TimeoutSec 30
    if ($seed.task_count -ne 9 -or @($seed.tasks | Where-Object ivr_call_job_id).Count -ne 8) { throw 'Seed must report nine tasks and eight jobs' }
    $headers['Idempotency-Key'] += '-scenario'
    $scenario = Invoke-RestMethod "$root/scenarios/SCN-001-confirm:dry-run" -Method Post -Headers $headers -ContentType application/json -Body $body -TimeoutSec 30
    if ($scenario.coverage -ne 'REPLAYED' -or $scenario.actual_result_type -ne 'IVR_CONFIRMED' -or !$scenario.matches) { throw 'Confirm replay did not match' }
    $checks.Add("$($Api.Label): ready=200, seed=9/8, SCN-001-confirm=IVR_CONFIRMED")
    Write-Host $checks[$checks.Count - 1]
}

function Get-TaskFingerprint([string] $Database) {
    return Invoke-Sql $Database @'
SELECT md5(string_agg(row_to_json(t)::text, '' ORDER BY task_id)) FROM ivr_confirmation_tasks t;
'@
}

Push-Location $repositoryRoot
try {
    $candidateSha = (& git rev-parse --verify HEAD).Trim()
    $previousSha = (& git rev-parse --verify "$PreviousRef^{commit}").Trim()
    if ($LASTEXITCODE -ne 0 -or $previousSha -notmatch '^[0-9a-f]{40}$' -or $candidateSha -eq $previousSha) { throw 'Provide a distinct, existing previous commit' }
    if (@(& git status --porcelain --untracked-files=normal).Count -ne 0) { throw 'Commit the candidate first; exact-SHA evidence refuses a dirty tree' }
    New-Item -ItemType Directory -Path $runRoot | Out-Null
    Invoke-Checked git @('worktree', 'add', '--detach', $previousTree, $previousSha)
    foreach ($tree in @($previousTree, $repositoryRoot)) {
        Push-Location $tree
        try {
            Invoke-Checked dotnet @('tool', 'restore')
            Invoke-Checked dotnet @('build', 'src/Ivr.Api/Ivr.Api.csproj', '-c', 'Release', '--nologo')
        } finally { Pop-Location }
    }

    Invoke-Checked docker @('run', '-d', '--name', $containerName, '-e', 'POSTGRES_USER=ivr_expand',
        '-e', 'POSTGRES_PASSWORD=synthetic-local-only', '-e', 'POSTGRES_DB=source',
        '-p', '127.0.0.1::5432', 'postgres:16-alpine')
    $containerCreated = $true
    $deadline = [DateTime]::UtcNow.AddSeconds(45)
    do {
        & docker exec $containerName pg_isready -h 127.0.0.1 -U ivr_expand -d source *> $null
        if ($LASTEXITCODE -eq 0) { break }
        if ([DateTime]::UtcNow -ge $deadline) { throw 'Dedicated PostgreSQL did not start' }
        Start-Sleep -Milliseconds 500
    } while ($true)
    $port = (& docker inspect --format '{{(index (index .NetworkSettings.Ports "5432/tcp") 0).HostPort}}' $containerName).Trim()
    $connection = "Host=127.0.0.1;Port=$port;Database=source;Username=ivr_expand;Password=synthetic-local-only"
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:DOTNET_ENVIRONMENT = 'Development'
    $env:IVR_EXECUTION_MODE = 'MOCK'
    $env:IVR_ADAPTER_MODE = 'MOCK'
    $env:SALES_PROVIDER = 'FAKE_TARGET_V1'
    $env:SIM_PROVIDER = 'MOCK'
    $env:REAL_CUSTOMER_CALL_ALLOWED = 'NO'
    $env:ConnectionStrings__IvrDb = $connection
    Push-Location $previousTree
    try {
        Invoke-Checked dotnet @('tool', 'run', 'dotnet-ef', 'database', 'update', '--no-build', '--configuration', 'Release',
            '--project', 'src/Ivr.Infrastructure', '--startup-project', 'src/Ivr.Infrastructure', '--connection', $connection)
    } finally { Pop-Location }
    $old = Start-Api $previousTree 'n-minus-1-source'
    Assert-Functional $old
    Stop-Api $old
    $originalFingerprint = Get-TaskFingerprint 'source'
    $previousMigration = Invoke-Sql 'source' 'SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId" DESC LIMIT 1'

    # Restore the real N-1 database backup, including data AND EF migration history, into a copy.
    Invoke-Checked docker @('exec', $containerName, 'pg_dump', '-U', 'ivr_expand', '-d', 'source', '-Fc', '-f', '/tmp/source.dump')
    Invoke-Checked docker @('cp', "${containerName}:/tmp/source.dump", (Join-Path $runRoot 'source.dump'))
    Invoke-Checked docker @('exec', $containerName, 'createdb', '-U', 'ivr_expand', 'upgrade_copy')
    Invoke-Checked docker @('exec', $containerName, 'pg_restore', '-U', 'ivr_expand', '-d', 'upgrade_copy', '--exit-on-error', '/tmp/source.dump')
    $env:ConnectionStrings__IvrDb = $connection.Replace('Database=source;', 'Database=upgrade_copy;')
    if ((Get-TaskFingerprint 'upgrade_copy') -ne $originalFingerprint) { throw 'Restored copy differs from source' }
    $old = Start-Api $previousTree 'n-minus-1-copy'
    Assert-Functional $old
    $new = Start-Api $repositoryRoot 'n-before-expand'
    Assert-Ready $new 503
    Invoke-Checked dotnet @('tool', 'run', 'dotnet-ef', 'database', 'update', '--no-build', '--configuration', 'Release',
        '--project', 'src/Ivr.Infrastructure', '--startup-project', 'src/Ivr.Infrastructure', '--connection', $env:ConnectionStrings__IvrDb)
    Assert-Functional $old
    Assert-Functional $new
    $checks.Add('N-1 -> N upgrade: PASS; old and new binaries both serve on forward schema')

    # This is the adjacent-version overlap (N/N+1 deployment slots): two DISTINCT source SHAs
    # simultaneously serve and write on one forward schema. It cannot certify arbitrary future code.
    $checks.Add('Adjacent-version N/N+1 overlap: PASS; distinct previous/candidate SHAs concurrently active')
    Stop-Api $new
    Stop-Api $old
    $old = Start-Api $previousTree 'rollback-n-to-previous'
    Assert-Functional $old
    Stop-Api $old
    $new = Start-Api $repositoryRoot 'forward-recovery-n'
    Assert-Functional $new
    if ((Get-TaskFingerprint 'upgrade_copy') -ne $originalFingerprint) { throw 'Task data changed across upgrade/rollback/forward recovery' }
    if ((Get-TaskFingerprint 'source') -ne $originalFingerprint) { throw 'Source database was mutated by the copy drill' }
    if ((& git rev-parse HEAD).Trim() -ne $candidateSha -or @(& git status --porcelain).Count -ne 0) { throw 'Candidate source changed during the drill' }
    $checks.Add('rollback N and forward recovery: PASS; original task fingerprint preserved')
    $evidence = [ordered]@{
        status = 'LOCAL_POSTGRESQL_PASS'; run_id = $runId; utc = [DateTime]::UtcNow.ToString('o')
        candidate_sha = $candidateSha; previous_sha = $previousSha; previous_migration = $previousMigration
        overlap = 'Two distinct previous/candidate source SHAs concurrently serving the same forward schema'
        api_sha256 = (Get-FileHash src/Ivr.Api/bin/Release/net10.0/Ivr.Api.dll).Hash
        infrastructure_sha256 = (Get-FileHash src/Ivr.Infrastructure/bin/Release/net10.0/Ivr.Infrastructure.dll).Hash
        previous_infrastructure_sha256 = (Get-FileHash (Join-Path $previousTree 'src/Ivr.Infrastructure/bin/Release/net10.0/Ivr.Infrastructure.dll')).Hash
        backup_sha256 = (Get-FileHash (Join-Path $runRoot 'source.dump')).Hash
        task_fingerprint = $originalFingerprint; postgres = (Invoke-Sql 'upgrade_copy' 'SHOW server_version')
        applied_migrations = (Invoke-Sql 'upgrade_copy' 'SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId"') -split "`n"
        checks = $checks.ToArray(); safety = 'MOCK / MOCK / NO; no worker; no external calls'
        hosted_ci = 'NOT_RUN'; cluster_rollout = 'NOT_RUN'; contract_cleanup = 'DEFERRED_LATER_RELEASE'
    }
    $evidence | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $runRoot 'rollback-evidence.json') -Encoding utf8
    Write-Host "EXPAND_CONTRACT_DRILL_PASS candidate=$candidateSha evidence=$runRoot/rollback-evidence.json"
}
finally {
    foreach ($process in $processes) {
        if (!$process.HasExited) { Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue }
    }
    if ($containerCreated) { & docker rm -f $containerName | Out-Null }
    Pop-Location
}
