<#
.SYNOPSIS
    Drives one complete confirmation-call lifecycle locally, in MOCK, and checks the result of
    every scenario against what the specification says it must be (W-0193).

.DESCRIPTION
    The worker ships with scheduler, normalisation, callback delivery and MOCK telephony all
    DISABLED. That is the correct default - a stack a developer starts to look at the console must
    not dial anything - but it also means `docker compose up` produces a system that accepts tasks
    and then does nothing with them, and every rehearsal so far has re-derived the switch-on
    configuration by hand.

    This script is that configuration, written down once. It starts PostgreSQL, applies migrations,
    starts a fake Sales endpoint, starts the API and the worker with MOCK telephony armed, admits
    six tasks, waits for them to run, and asserts the result taxonomy - including the two
    invariants that matter most: a technical exception is never counted as a customer attempt, and
    only a final result reaches the callback outbox.

    What it deliberately does NOT relax: IVR_EXECUTION_MODE stays MOCK, SIM_PROVIDER stays MOCK and
    REAL_CUSTOMER_CALL_ALLOWED stays NO. MockSchedulerDispatchGateway.IsReady requires all three, so
    this script cannot reach a vendor even if one were configured. The kill switch is the only
    safety lifted, and it is lifted against a fake gateway.

.PARAMETER KeepRunning
    Leave the API, worker and fake Sales running after the assertions pass, for poking at the
    console. Without it everything this script started is stopped again.

.PARAMETER SkipBuild
    Reuse the existing Release build instead of rebuilding.
#>
[CmdletBinding()]
param(
    [switch] $KeepRunning,
    [switch] $SkipBuild
)

$ErrorActionPreference = "Stop"
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "../..")).Path
$apiUrl = "http://127.0.0.1:5005"
$apiBase = "$apiUrl/v1/ivr/order-confirmation"
$salesPort = 18085
$salesContainer = "ivr-e2e-fake-sales"
$postgresContainer = "ginsengfood-ivr-dev-postgres-1"
$runId = "E2E{0}" -f (Get-Date -Format "HHmmss")

$orderCoreToken = "dev-ordercore-token-not-a-real-secret"
$internalToken = "dev-internal-token-not-a-real-secret"
$readToken = "dev-admin-read-token-not-a-real-secret"

# scenario -> provider disposition, DTMF key, expected final result, expected counted flag,
# and whether a callback must be delivered. This table IS the specification under test.
#
# No no-answer case here, and the reason is time rather than coverage: IVR_NO_ANSWER_ATTEMPT only
# becomes IVR_NO_ANSWER_FINAL after the second attempt, which the candidate policy schedules 150s
# (Golden Hour) or 450s (24/7) out. A rehearsal nobody runs because it takes eight minutes is
# worth less than one that runs in forty seconds, so the multi-attempt path stays with the
# integration suite, which controls its own clock. IT-SCHED-* covers it.
$scenarios = @(
    @{ Name = "CONFIRM";   Disposition = "Answered";           Dtmf = "1"; Program = "GOLDEN_HOUR";       Payment = "ONLINE"; Result = "IVR_CONFIRMED";           Counted = $true;  Callback = $true }
    @{ Name = "CANCEL";    Disposition = "Answered";           Dtmf = "0"; Program = "GOLDEN_HOUR";       Payment = "ONLINE"; Result = "IVR_CUSTOMER_CANCELLED";  Counted = $true;  Callback = $true }
    @{ Name = "BADNUMBER"; Disposition = "InvalidDestination"; Dtmf = $null; Program = "TWENTY_FOUR_SEVEN"; Payment = "COD";  Result = "IVR_INVALID_PHONE_FINAL"; Counted = $false; Callback = $true }
    @{ Name = "WRONGKEY";  Disposition = "Answered";           Dtmf = "7"; Program = "TWENTY_FOUR_SEVEN"; Payment = "COD";    Result = "IVR_WRONG_INPUT";         Counted = $true;  Callback = $false }
    @{ Name = "TECHNICAL"; Disposition = "AudioError";         Dtmf = $null; Program = "TWENTY_FOUR_SEVEN"; Payment = "COD";  Result = "IVR_TECHNICAL_EXCEPTION"; Counted = $false; Callback = $false }
)

$started = [System.Collections.Generic.List[object]]::new()
$failures = [System.Collections.Generic.List[string]]::new()

function Write-Step([string] $text) {
    Write-Host ""
    Write-Host "== $text" -ForegroundColor Cyan
}

function Invoke-Psql([string] $sql) {
    $result = docker exec $postgresContainer psql -U ivr -d ivr -t -A -c $sql 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "psql failed: $result"
    }
    return ($result | Where-Object { $_ -ne "" })
}

function Set-MockTelephonyEnvironment {
    # The safety posture, unchanged.
    $env:IVR_EXECUTION_MODE = "MOCK"
    $env:IVR_ADAPTER_MODE = "MOCK"
    $env:REAL_CUSTOMER_CALL_ALLOWED = "NO"
    $env:SALES_PROVIDER = "FAKE_TARGET_V1"
    $env:SIM_PROVIDER = "MOCK"
    $env:ConnectionStrings__IvrDb = "Host=127.0.0.1;Port=55433;Database=ivr;Username=ivr"

    # The four worker loops, in the order a task passes through them.
    $env:Ivr__Scheduler__Enabled = "true"
    $env:Ivr__Scheduler__PollIntervalMilliseconds = "500"
    $env:Ivr__Scheduler__MockChannelCount = "2"
    $env:Ivr__Normalization__Enabled = "true"
    $env:Ivr__Normalization__PollIntervalMilliseconds = "500"
    $env:Ivr__CallbackDelivery__Enabled = "true"
    $env:Ivr__CallbackDelivery__PollIntervalMilliseconds = "500"
    $env:Ivr__CallbackDelivery__TargetBaseUrl = "http://127.0.0.1:$salesPort"

    $env:Ivr__Telephony__Mock__Enabled = "true"
    $env:Ivr__Telephony__Mock__KillSwitchEngaged = "false"
    $env:Ivr__Telephony__Mock__DtmfTimeoutSeconds = "5"
    $env:Ivr__Telephony__Mock__CooldownSeconds = "0"
    $env:Ivr__Telephony__Mock__DestinationAllowlist__0 = "mock-destination-allowlisted"

    # A `*` cannot appear in a PowerShell variable path, so these two families go through the
    # environment API directly. Every dial token maps to the one allowlisted fake destination.
    [Environment]::SetEnvironmentVariable(
        "Ivr__Telephony__Mock__TokenDestinations__*", "mock-destination-allowlisted")

    foreach ($scenario in $scenarios) {
        $taskId = "${runId}_$($scenario.Name)"
        $prefix = "Ivr__Telephony__Mock__Scenarios__$taskId"
        [Environment]::SetEnvironmentVariable("${prefix}__Disposition", $scenario.Disposition)
        [Environment]::SetEnvironmentVariable("${prefix}__DialDelayMilliseconds", "0")
        [Environment]::SetEnvironmentVariable("${prefix}__PlayDelayMilliseconds", "0")
        [Environment]::SetEnvironmentVariable("${prefix}__CaptureDelayMilliseconds", "0")
        if ($null -ne $scenario.Dtmf) {
            [Environment]::SetEnvironmentVariable("${prefix}__DtmfKey", $scenario.Dtmf)
        }
        if ($scenario.Disposition -eq "AudioError") {
            [Environment]::SetEnvironmentVariable(
                "${prefix}__TechnicalErrorCode", "MOCK_AUDIO_ERROR_E2E")
        }
    }

    # A task the run did not name still dials rather than failing in a way that looks like a
    # defect in the stack.
    [Environment]::SetEnvironmentVariable("Ivr__Telephony__Mock__Scenarios__*__Disposition", "Answered")
    [Environment]::SetEnvironmentVariable("Ivr__Telephony__Mock__Scenarios__*__DtmfKey", "1")
    [Environment]::SetEnvironmentVariable("Ivr__Telephony__Mock__Scenarios__*__DialDelayMilliseconds", "0")
    [Environment]::SetEnvironmentVariable("Ivr__Telephony__Mock__Scenarios__*__PlayDelayMilliseconds", "0")
    [Environment]::SetEnvironmentVariable("Ivr__Telephony__Mock__Scenarios__*__CaptureDelayMilliseconds", "0")
}

function New-TaskBody([hashtable] $scenario, [string] $taskId) {
    $windowSeconds = if ($scenario.Program -eq "GOLDEN_HOUR") { 300 } else { 900 }
    $secondOffset = if ($scenario.Program -eq "GOLDEN_HOUR") { 150 } else { 450 }
    $start = (Get-Date).ToUniversalTime().AddSeconds(-10)
    $end = $start.AddSeconds($windowSeconds)
    $programName = if ($scenario.Program -eq "GOLDEN_HOUR") { "Gio Vang" } else { "Ban hang 24/7" }

    # Hand-built JSON rather than ConvertTo-Json: Windows PowerShell renders a one-element array
    # as an object, which the intake schema rejects with a message that does not say so.
    return @"
{
  "contract_version": "ivr-order-confirmation.v1",
  "task_id": "$taskId",
  "correlation_id": "corr-$taskId",
  "created_at": "$($start.ToString('o'))",
  "order_id": "ORD-$taskId",
  "order_code": "GF-E2E-001",
  "order_code_short": "E2E001",
  "order_version": "17",
  "order_state": "CONFIRMING",
  "payment_method_snapshot": "$($scenario.Payment)",
  "ivr_confirmation_required": true,
  "is_ivr_callable": true,
  "program_code": "$($scenario.Program)",
  "confirmation_window_started_at": "$($start.ToString('o'))",
  "confirmation_window_expires_at": "$($end.ToString('o'))",
  "attempt_policy_version": "mock-lab-v1",
  "max_customer_attempts": 2,
  "attempt_offsets_seconds": [0, $secondOffset],
  "phone_ref": "phone-ref-$taskId",
  "phone_masked": "84xxxxx0001",
  "phone_validation_status": "VALID",
  "dial_token": "dial-token-$taskId",
  "dial_token_expires_at": "$($end.ToString('o'))",
  "privacy_safe_order_summary": {
    "customer_display_name": "chi An",
    "order_code_short": "E2E001",
    "items": [{ "public_name": "Nuoc hong sam", "quantity": 2, "unit_label": "hop" }],
    "total_amount": 560000,
    "currency": "VND",
    "delivery_area_short": "Phuong Ben Nghe, Quan Mot",
    "program_display_name": "$programName",
    "locale": "vi-VN"
  },
  "call_restriction": false,
  "eligibility_snapshot": {
    "decision": "ELIGIBLE",
    "source_version": "sales-eligibility-v1",
    "captured_at": "$($start.ToString('o'))",
    "source_available": true,
    "blockers": [],
    "voice_restriction": {
      "restricted": false,
      "source_available": true,
      "source_version": "sales-voice-v1"
    }
  },
  "evidence_ref": "evidence://local-e2e/$taskId"
}
"@
}

function Stop-Started {
    foreach ($item in $started) {
        if ($item.Kind -eq "process" -and -not $item.Process.HasExited) {
            Stop-Process -Id $item.Process.Id -Force -ErrorAction SilentlyContinue
        }
        elseif ($item.Kind -eq "container") {
            docker rm -f $item.Name 2>&1 | Out-Null
        }
    }
}

Push-Location $repositoryRoot
try {
    if (-not $SkipBuild) {
        Write-Step "Building"
        dotnet build Ivr.sln -c Release --nologo | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "Build failed." }
    }

    Write-Step "PostgreSQL and migrations"
    docker compose -f docker-compose.dev.yml up -d postgres | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Could not start PostgreSQL." }
    $ready = $false
    foreach ($attempt in 1..40) {
        docker exec $postgresContainer pg_isready -U ivr -d ivr 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) { $ready = $true; break }
        Start-Sleep -Milliseconds 500
    }
    if (-not $ready) { throw "PostgreSQL did not become ready." }
    & (Join-Path $PSScriptRoot "Update-IvrDatabase.ps1") | Out-Null

    Write-Step "Fake Sales (Target V1 callback)"
    docker rm -f $salesContainer 2>&1 | Out-Null
    docker run -d --name $salesContainer `
        -p "127.0.0.1:${salesPort}:8080" `
        -v "$repositoryRoot/deploy/docker/fake-sales:/home/wiremock/mappings:ro" `
        wiremock/wiremock:3.9.1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Could not start the fake Sales container." }
    $started.Add(@{ Kind = "container"; Name = $salesContainer })

    Set-MockTelephonyEnvironment

    Write-Step "API and worker"
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:DOTNET_ENVIRONMENT = "Development"
    $env:ASPNETCORE_URLS = $apiUrl

    # Both processes log every EF command at Information, so their output goes to files. Left on
    # the console it buries the one thing this script exists to print.
    $logDirectory = Join-Path $repositoryRoot "ci-artifacts/local-e2e"
    New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null
    $apiLog = Join-Path $logDirectory "api.log"
    $workerLog = Join-Path $logDirectory "worker.log"

    $api = Start-Process -PassThru -NoNewWindow -FilePath "dotnet" `
        -RedirectStandardOutput $apiLog -RedirectStandardError "$apiLog.err" `
        -ArgumentList @("run", "--project", "src/Ivr.Api", "-c", "Release", "--no-build")
    $started.Add(@{ Kind = "process"; Process = $api })

    $live = $false
    foreach ($attempt in 1..60) {
        try {
            $probe = Invoke-WebRequest -Uri "$apiUrl/health/ready" -UseBasicParsing -TimeoutSec 3
            if ($probe.StatusCode -eq 200) { $live = $true; break }
        }
        catch { Start-Sleep -Milliseconds 750 }
    }
    if (-not $live) { throw "The API did not become ready." }

    $worker = Start-Process -PassThru -NoNewWindow -FilePath "dotnet" `
        -RedirectStandardOutput $workerLog -RedirectStandardError "$workerLog.err" `
        -ArgumentList @("run", "--project", "src/Ivr.Worker", "-c", "Release", "--no-build")
    $started.Add(@{ Kind = "process"; Process = $worker })
    Start-Sleep -Seconds 6
    if ($worker.HasExited) {
        throw "The worker exited during startup; see $workerLog."
    }
    Write-Host "  logs: $logDirectory"

    Write-Step "Admitting $($scenarios.Count) tasks"
    foreach ($scenario in $scenarios) {
        $taskId = "${runId}_$($scenario.Name)"
        $intakeHeaders = @{
            Authorization      = "Bearer $orderCoreToken"
            "X-Source-System"  = "order-core"
            "X-Correlation-Id" = "corr-$taskId"
            "Idempotency-Key"  = "idem-$taskId"
        }
        $intake = Invoke-RestMethod -Method Post -Uri "$apiBase/tasks" `
            -Headers $intakeHeaders -ContentType "application/json" `
            -Body ([Text.Encoding]::UTF8.GetBytes((New-TaskBody $scenario $taskId)))

        $eligibilityHeaders = @{
            Authorization      = "Bearer $internalToken"
            "X-Source-System"  = "ivr-worker"
            "X-Service-Scope"  = "ivr.internal.write"
            "X-Correlation-Id" = "corr-$taskId"
            "Idempotency-Key"  = "elig-$taskId"
        }
        $eligibility = Invoke-RestMethod -Method Post -Uri "$apiBase/eligibility-checks" `
            -Headers $eligibilityHeaders -ContentType "application/json" `
            -Body "{`"task_id`":`"$taskId`"}"

        Write-Host ("  {0,-12} intake={1} eligibility={2}" -f `
            $scenario.Name, $intake.decision, $eligibility.decision)
        if ($eligibility.decision -ne "ELIGIBLE_FOR_IVR") {
            $failures.Add("$($scenario.Name): eligibility returned $($eligibility.decision).")
        }
    }

    Write-Step "Waiting for the pipeline"
    $expectedFinal = ($scenarios | Where-Object { $_.Callback }).Count
    $delivered = 0
    foreach ($attempt in 1..40) {
        $delivered = [int](Invoke-Psql (
            "select count(*) from ivr_result_callbacks " +
            "where task_id like '${runId}\_%' and delivery_status = 'DELIVERED_ACCEPTED'"))
        Write-Host ("  t={0,3}s delivered={1}/{2}" -f ($attempt * 3), $delivered, $expectedFinal)
        if ($delivered -ge $expectedFinal) { break }
        Start-Sleep -Seconds 3
    }

    Write-Step "Checking every scenario"
    foreach ($scenario in $scenarios) {
        $taskId = "${runId}_$($scenario.Name)"
        $row = Invoke-Psql (
            "select result_type || '|' || is_counted_customer_attempt || '|' || " +
            "coalesce((select delivery_status from ivr_result_callbacks c " +
            "where c.task_id = r.task_id limit 1), 'none') " +
            "from ivr_call_results r where r.task_id = '$taskId' " +
            "order by r.created_at desc limit 1")

        if (-not $row) {
            $failures.Add("$($scenario.Name): no result was produced.")
            Write-Host ("  FAIL {0,-12} no result" -f $scenario.Name) -ForegroundColor Red
            continue
        }

        # A boolean concatenated in SQL renders as `true`/`false`, not psql's column display `t`/`f`.
        $parts = ($row | Select-Object -First 1).Split("|")
        $actualResult = $parts[0]
        $actualCounted = $parts[1] -eq "true"
        $actualDelivery = $parts[2]
        $expectedDelivery = if ($scenario.Callback) { "DELIVERED_ACCEPTED" } else { "none" }

        $ok = $actualResult -eq $scenario.Result `
            -and $actualCounted -eq $scenario.Counted `
            -and $actualDelivery -eq $expectedDelivery
        if ($ok) {
            Write-Host ("  ok   {0,-12} {1} counted={2} callback={3}" -f `
                $scenario.Name, $actualResult, $actualCounted, $actualDelivery) -ForegroundColor Green
        }
        else {
            $failures.Add(
                "$($scenario.Name): got $actualResult/counted=$actualCounted/$actualDelivery, " +
                "expected $($scenario.Result)/counted=$($scenario.Counted)/$expectedDelivery.")
            Write-Host ("  FAIL {0,-12} {1} counted={2} callback={3}" -f `
                $scenario.Name, $actualResult, $actualCounted, $actualDelivery) -ForegroundColor Red
        }
    }

    # The invariant worth asserting separately, because it is the one a plausible-looking bug
    # would break silently: no non-customer outcome may ever be stored as a customer attempt.
    $miscounted = [int](Invoke-Psql (
        "select count(*) from ivr_call_results where is_counted_customer_attempt is true " +
        "and result_type in ('IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION'," +
        "'IVR_INVALID_PHONE_FINAL','IVR_OPERATIONAL_BLOCKED','IVR_POLICY_BLOCKED')"))
    if ($miscounted -ne 0) {
        $failures.Add("$miscounted non-customer result(s) are stored as counted customer attempts.")
    }

    Write-Host ""
    if ($failures.Count -eq 0) {
        Write-Host "Local end-to-end run passed." -ForegroundColor Green
    }
    else {
        Write-Host "Local end-to-end run FAILED:" -ForegroundColor Red
        $failures | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    }

    if ($KeepRunning) {
        Write-Host ""
        Write-Host "API $apiUrl - worker running - fake Sales http://127.0.0.1:$salesPort"
        Write-Host "Stop with: docker rm -f $salesContainer; then close the dotnet processes."
        $started.Clear()
    }
}
finally {
    Stop-Started
    Pop-Location
}

if ($failures.Count -ne 0) {
    exit 1
}
