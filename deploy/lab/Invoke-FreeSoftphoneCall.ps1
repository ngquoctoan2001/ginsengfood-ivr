[CmdletBinding()]
param(
    [string]$ApiBaseUrl = 'http://127.0.0.1:58080',
    [ValidateRange(10, 180)]
    [int]$RegistrationTimeoutSeconds = 60,
    [ValidateRange(20, 300)]
    [int]$ResultTimeoutSeconds = 120
)

$ErrorActionPreference = 'Stop'
$asteriskContainer = 'ginsengfood-ivr-dev-asterisk-1'
$postgresContainer = 'ginsengfood-ivr-dev-postgres-1'

Write-Host 'Waiting for MicroSIP LAB-A registration...'
$registered = $false
$deadline = [DateTimeOffset]::UtcNow.AddSeconds($RegistrationTimeoutSeconds)
while ([DateTimeOffset]::UtcNow -lt $deadline) {
    $contacts = & docker exec $asteriskContainer asterisk -rx 'pjsip show contacts' 2>$null
    if ($LASTEXITCODE -eq 0 -and ($contacts -join "`n") -match 'LAB-A/sip:LAB-A@') {
        $registered = $true
        break
    }

    Start-Sleep -Seconds 2
}

if (-not $registered) {
    throw 'LAB-A is not registered. Keep MicroSIP open and confirm its status shows Online.'
}

& (Join-Path $PSScriptRoot 'Show-MicroSipLab.ps1')

$now = [DateTimeOffset]::UtcNow
$started = $now.AddSeconds(-10)
$expires = $started.AddMinutes(5)
$suffix = $now.ToString('yyyyMMddHHmmss')
$taskId = "TASK-LAB-$suffix"
$correlationId = "corr-lab-$suffix"
$payload = [ordered]@{
    contract_version = 'ivr-order-confirmation.v1'
    task_id = $taskId
    correlation_id = $correlationId
    created_at = $started.ToString('yyyy-MM-ddTHH:mm:ssZ')
    order_id = "ORDER-LAB-$suffix"
    order_code = "GF-LAB-$suffix"
    order_code_short = 'E2E001'
    order_version = '1'
    order_state = 'CONFIRMING'
    payment_method_snapshot = 'ONLINE'
    ivr_confirmation_required = $true
    is_ivr_callable = $true
    program_code = 'GOLDEN_HOUR'
    confirmation_window_started_at = $started.ToString('yyyy-MM-ddTHH:mm:ssZ')
    confirmation_window_expires_at = $expires.ToString('yyyy-MM-ddTHH:mm:ssZ')
    attempt_policy_version = 'lab-softphone-v1'
    max_customer_attempts = 1
    attempt_offsets_seconds = @(0)
    phone_ref = 'phone-ref-w0104-fake'
    phone_masked = '84xxxxx0001'
    phone_validation_status = 'VALID'
    dial_token = "dial-token-lab-$suffix"
    dial_token_expires_at = $expires.ToString('yyyy-MM-ddTHH:mm:ssZ')
    privacy_safe_order_summary = [ordered]@{
        customer_display_name = 'anh/chị Giang'
        order_code_short = 'E2E001'
        items = @(
            [ordered]@{
                public_name = 'Cháo sâm diêm mạch - hạt sen'
                quantity = 2
                unit_label = 'hộp'
            }
        )
        total_amount = 560000
        currency = 'VND'
        delivery_area_short = 'Phường Phú Khương, tỉnh Vĩnh Long'
        program_display_name = 'Giờ Vàng'
        locale = 'vi-VN'
    }
    call_restriction = $false
    sellable_status = @(
        [ordered]@{
            sku_id = 'SKU-LAB-FAKE-1'
            decision = 'SELLABLE'
            captured_at = $started.ToString('yyyy-MM-ddTHH:mm:ssZ')
            recall_hold = $false
            sale_lock = $false
            quality_hold = $false
            stock_available = $true
            batch_released = $true
            trace_ready = $true
        }
    )
    eligibility_snapshot = [ordered]@{
        decision = 'ELIGIBLE'
        source_version = 'w0104-fake-sales-v1'
        captured_at = $started.ToString('yyyy-MM-ddTHH:mm:ssZ')
        source_available = $true
        blockers = @()
        voice_restriction = [ordered]@{
            restricted = $false
            source_available = $true
            source_version = 'w0104-fake-voice-v1'
        }
    }
    call_script_template_id = 'SCRIPT-ORDER-CONFIRM'
    call_script_version = 'v3-test-approved'
    evidence_policy_version = 'w0104-lab-evidence-v1'
    privacy_policy_version = 'w0104-lab-privacy-v1'
    evidence_ref = "evidence://w0104/$taskId"
}

$taskHeaders = @{
    'X-Source-System' = 'order-core'
    Authorization = 'Bearer dev-ordercore-token-not-a-real-secret'
    'X-Correlation-Id' = $correlationId
    'Idempotency-Key' = "idem-$taskId"
}
$intakeRequest = @{
    Method = 'Post'
    Uri = "$ApiBaseUrl/v1/ivr/order-confirmation/tasks"
    Headers = $taskHeaders
    ContentType = 'application/json'
    Body = $payload | ConvertTo-Json -Depth 20 -Compress
}
$intake = Invoke-RestMethod @intakeRequest
if ($intake.decision -ne 'TASK_ACCEPTED_CALL_JOB_CREATED') {
    throw "Fake task intake was not accepted: $($intake | ConvertTo-Json -Compress)."
}

$internalHeaders = @{
    'X-Source-System' = 'ivr-worker'
    'X-Service-Scope' = 'ivr.internal.write'
    Authorization = 'Bearer dev-internal-token-not-a-real-secret'
    'X-Correlation-Id' = $correlationId
    'Idempotency-Key' = "idem-elig-$taskId"
}
$eligibilityRequest = @{
    Method = 'Post'
    Uri = "$ApiBaseUrl/v1/ivr/order-confirmation/eligibility-checks"
    Headers = $internalHeaders
    ContentType = 'application/json'
    Body = @{ task_id = $taskId } | ConvertTo-Json -Compress
}
$eligibility = Invoke-RestMethod @eligibilityRequest
if ($eligibility.decision -ne 'ELIGIBLE_FOR_IVR') {
    throw "Fake task eligibility was not accepted: $($eligibility | ConvertTo-Json -Compress)."
}

Write-Host "Fake order $taskId queued. Answer MicroSIP and press 1 to confirm or 0 to cancel."
$resultDeadline = [DateTimeOffset]::UtcNow.AddSeconds($ResultTimeoutSeconds)
$result = ''
while ([DateTimeOffset]::UtcNow -lt $resultDeadline) {
    $sql = @"
SELECT COALESCE((
    SELECT result.result_type || '|' || result.is_final_for_ivr::text || '|' ||
           result.is_counted_customer_attempt::text
    FROM ivr_call_results result
    JOIN ivr_call_jobs job ON job.ivr_call_job_id = result.ivr_call_job_id
    WHERE job.task_id = '$taskId'
    ORDER BY result.created_at DESC
    LIMIT 1), 'PENDING');
"@
    $result = (& docker exec $postgresContainer psql -U ivr -d ivr -Atc $sql 2>$null |
        Select-Object -Last 1).Trim()
    if ($result -and $result -ne 'PENDING') {
        break
    }

    Start-Sleep -Seconds 2
}

if (-not $result -or $result -eq 'PENDING') {
    throw "No normalized result appeared for $taskId within $ResultTimeoutSeconds seconds."
}

Write-Host "W-0104 result: task=$taskId; result=$result"
