<#
.SYNOPSIS
    Prepares the local database, starts the Development API, loads all nine seed fixtures and
    replays SCN-001-confirm without requiring private environment variables (W-0191).

.DESCRIPTION
    This is the reproducible developer-surface bootstrap. It validates the committed seed catalog
    before touching Docker, applies migrations through the canonical local:prepare command, starts
    only Ivr.Api on an available loopback port, loads the seed set and asserts the confirm dry-run.

    The safety posture is fixed to MOCK / MOCK / NO and no worker is started. Eight fixtures create
    dry-run-only jobs; the ninth is expected to remain blocked by call_restriction. Re-running the
    command is supported: the eight existing jobs are reported rather than duplicated.

.PARAMETER SkipBuild
    Reuse an existing Release build. Database preparation and runtime assertions still run.
#>
[CmdletBinding()]
param(
    [switch] $SkipBuild
)

$ErrorActionPreference = "Stop"
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "../..")).Path
$apiProjectDirectory = Join-Path $repositoryRoot "src/Ivr.Api"
$seedDirectory = Join-Path $repositoryRoot "seed"
$requiredSeedFiles = @(
    "sales-target-v1.sample.json",
    "call-scenarios.sample.json",
    "integration-status.sample.json"
)
$developmentWriteToken = "dev-admin-write-token-not-a-real-secret"
$apiProcess = $null

function Write-Step([string] $Text) {
    Write-Host ""
    Write-Host "== $Text" -ForegroundColor Cyan
}

function Assert-Command([string] $Name) {
    if ($null -eq (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' is unavailable. Install the prerequisite documented in README.md, then retry pnpm dev:bootstrap."
    }
}

function Assert-SeedCatalog {
    if (-not (Test-Path -LiteralPath $seedDirectory -PathType Container)) {
        throw "Seed directory is missing: $seedDirectory. Restore the repository seed/ folder before running pnpm dev:bootstrap."
    }

    foreach ($fileName in $requiredSeedFiles) {
        $path = Join-Path $seedDirectory $fileName
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            throw "Required seed file is missing: $path. Restore seed/$fileName before running pnpm dev:bootstrap."
        }
    }
}

function Get-LoopbackPort {
    $listener = [System.Net.Sockets.TcpListener]::new(
        [System.Net.IPAddress]::Loopback,
        0)
    try {
        $listener.Start()
        return ([System.Net.IPEndPoint] $listener.LocalEndpoint).Port
    }
    finally {
        $listener.Stop()
    }
}

function Wait-Api([string] $BaseUrl, [System.Diagnostics.Process] $Process) {
    foreach ($attempt in 1..80) {
        if ($Process.HasExited) {
            throw "Ivr.Api exited during startup. Inspect the bootstrap logs for the actionable configuration error."
        }

        try {
            $response = Invoke-WebRequest `
                -Uri "$BaseUrl/health/live" `
                -UseBasicParsing `
                -TimeoutSec 2
            if ($response.StatusCode -eq 200) {
                return
            }
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
    }

    throw "Ivr.Api did not become live within 40 seconds. Inspect the bootstrap logs."
}

function Invoke-DeveloperPost(
    [string] $Uri,
    [string] $CorrelationId,
    [string] $IdempotencyKey,
    [hashtable] $Body) {
    $headers = @{
        Authorization      = "Bearer $developmentWriteToken"
        "X-Service-Scope" = "ivr.admin.write"
        "X-Actor-Id" = "local-bootstrap"
        "X-Correlation-Id" = $CorrelationId
        "Idempotency-Key" = $IdempotencyKey
    }

    try {
        return Invoke-RestMethod `
            -Method Post `
            -Uri $Uri `
            -Headers $headers `
            -ContentType "application/json" `
            -Body ($Body | ConvertTo-Json -Depth 6) `
            -TimeoutSec 30
    }
    catch {
        $detail = $_.ErrorDetails.Message
        if ([string]::IsNullOrWhiteSpace($detail)) {
            $detail = $_.Exception.Message
        }

        throw "Developer API request failed at '$Uri': $detail"
    }
}

Push-Location $repositoryRoot
try {
    Write-Step "Validating prerequisites and seed catalog"
    Assert-Command "dotnet"
    Assert-Command "docker"
    Assert-Command "pnpm"
    Assert-SeedCatalog
    Write-Host "  seed: 3 required files present"

    if (-not $SkipBuild) {
        Write-Step "Building Ivr.Api (Release)"
        dotnet build src/Ivr.Api/Ivr.Api.csproj -c Release --nologo
        if ($LASTEXITCODE -ne 0) {
            throw "Ivr.Api build failed with exit code $LASTEXITCODE."
        }
    }

    Write-Step "Preparing PostgreSQL and applying migrations"
    pnpm local:prepare
    if ($LASTEXITCODE -ne 0) {
        throw "pnpm local:prepare failed with exit code $LASTEXITCODE."
    }

    $port = Get-LoopbackPort
    $apiUrl = "http://127.0.0.1:$port"
    $developerRoot = "$apiUrl/v1/ivr/order-confirmation/dev"
    $runId = "{0}-{1}" -f (Get-Date -Format "yyyyMMdd-HHmmss"), ([Guid]::NewGuid().ToString("N").Substring(0, 8))
    $logDirectory = Join-Path $repositoryRoot "ci-artifacts/dev-bootstrap/$runId"
    New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null
    $stdoutLog = Join-Path $logDirectory "api.stdout.log"
    $stderrLog = Join-Path $logDirectory "api.stderr.log"

    Write-Step "Starting Development API in fail-closed MOCK mode"
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:DOTNET_ENVIRONMENT = "Development"
    $env:ASPNETCORE_URLS = $apiUrl
    $env:IVR_EXECUTION_MODE = "MOCK"
    $env:IVR_ADAPTER_MODE = "MOCK"
    $env:SALES_PROVIDER = "FAKE_TARGET_V1"
    $env:SIM_PROVIDER = "MOCK"
    $env:REAL_CUSTOMER_CALL_ALLOWED = "NO"

    $apiProcess = Start-Process `
        -PassThru `
        -WindowStyle Hidden `
        -WorkingDirectory $apiProjectDirectory `
        -FilePath "dotnet" `
        -RedirectStandardOutput $stdoutLog `
        -RedirectStandardError $stderrLog `
        -ArgumentList @("run", "--project", "Ivr.Api.csproj", "-c", "Release", "--no-build", "--no-launch-profile")
    Wait-Api $apiUrl $apiProcess
    Write-Host "  API: $apiUrl"
    Write-Host "  logs: $logDirectory"

    Write-Step "Loading the nine committed task fixtures"
    $seed = Invoke-DeveloperPost `
        -Uri "$developerRoot/seed:load" `
        -CorrelationId "bootstrap-$runId-seed" `
        -IdempotencyKey "bootstrap-$runId-seed" `
        -Body @{
            reason = "Reproducible local development bootstrap"
            evidence_ref = "evidence://local-bootstrap/$runId"
            rebase_windows = $true
        }

    $seedOutcomes = @($seed.tasks)
    $jobBacked = @($seedOutcomes | Where-Object {
        -not [string]::IsNullOrWhiteSpace([string] $_.ivr_call_job_id)
    })
    $restricted = @($seedOutcomes | Where-Object {
        $_.task_id -eq "TASK-TARGET-247-0005"
    })
    if ([int] $seed.task_count -ne 9 -or $seedOutcomes.Count -ne 9) {
        throw "Expected 9 seed outcomes, got task_count=$($seed.task_count) and outcomes=$($seedOutcomes.Count)."
    }
    if ($jobBacked.Count -ne 8) {
        throw "Expected 8 dry-run jobs (one fixture is call-restricted), got $($jobBacked.Count)."
    }
    if ($restricted.Count -ne 1 -or -not [string]::IsNullOrWhiteSpace([string] $restricted[0].ivr_call_job_id)) {
        throw "The call-restricted fixture was not preserved as a no-job outcome."
    }
    Write-Host "  seed: 9/9 outcomes, 8 dry-run jobs, 1 call-restricted fixture"

    Write-Step "Replaying SCN-001-confirm without dispatch"
    $scenario = Invoke-DeveloperPost `
        -Uri "$developerRoot/scenarios/SCN-001-confirm:dry-run" `
        -CorrelationId "bootstrap-$runId-scenario" `
        -IdempotencyKey "bootstrap-$runId-scenario" `
        -Body @{
            reason = "Verify the committed confirm scenario"
            evidence_ref = "evidence://local-bootstrap/$runId"
        }
    if ($scenario.coverage -ne "REPLAYED" `
        -or $scenario.actual_result_type -ne "IVR_CONFIRMED" `
        -or $scenario.matches -ne $true) {
        throw "SCN-001-confirm mismatch: coverage=$($scenario.coverage), result=$($scenario.actual_result_type), matches=$($scenario.matches)."
    }

    Write-Host "  scenario: REPLAYED -> IVR_CONFIRMED (matches=true)"
    Write-Host ""
    Write-Host "Development bootstrap passed." -ForegroundColor Green
}
finally {
    if ($null -ne $apiProcess -and -not $apiProcess.HasExited) {
        Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
        $apiProcess.WaitForExit(5000) | Out-Null
    }
    Pop-Location
}
