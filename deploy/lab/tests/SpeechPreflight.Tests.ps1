# W-0320: execute the actual entry scripts with all external side effects replaced by refusals.
[CmdletBinding()]
param([Parameter(Mandatory)][string]$LabRoot)
$ErrorActionPreference = 'Stop'
$scratch = Join-Path ([System.IO.Path]::GetTempPath()) ('ivr-preflight-test-' + [Guid]::NewGuid().ToString('N'))
$testLab = Join-Path $scratch 'deploy/lab'
New-Item -ItemType Directory -Path $testLab -Force | Out-Null
try {
    foreach ($file in 'Start-FreeSoftphoneLab.ps1', 'Invoke-FreeSoftphoneCall.ps1', 'fake-orders.json') {
        Copy-Item -LiteralPath (Join-Path $LabRoot $file) -Destination (Join-Path $testLab $file)
    }
    foreach ($file in 'Install-Launch-MicroSip.ps1', 'Show-MicroSipLab.ps1') {
        Set-Content -LiteralPath (Join-Path $testLab $file) -Value 'throw "TEST_ONLY_UI_BOUNDARY"'
    }
    $manifest = Join-Path $scratch 'voices.json'
    Set-Content -LiteralPath $manifest -Value '{"selections":{"North":{"voice_id":"test-north","speaking_rate":1},"Central":{"voice_id":"test-central","speaking_rate":1},"South":{"voice_id":"test-south","speaking_rate":1}}}'
    function global:node {
        $global:preflightInvocations++
        if ($args[0] -notlike '*check-speech-preflight.mjs' -or $args[1] -ne '--timeout-seconds') {
            throw 'WRONG_PREFLIGHT_INVOCATION'
        }
        $global:LASTEXITCODE = $global:preflightExitCode
    }
    function global:docker {
        $global:dockerInvocations++
        $global:LASTEXITCODE = 0
        if ($args -contains 'psql') { return 'IVR_CONFIRMED|true|true' }
        if ($global:headlessRouteTest) {
            if ($args -contains 'pjsip show endpoint LAB-A') { return " aors : $global:headlessAor" }
            if ($args -contains 'pjsip show contacts') { return 'Contact: LAB-A-AUTO/sip:LAB-A@headless-peer:5060 Avail' }
        }
        if ($args[0] -eq 'exec') { return 'LAB-A/sip:LAB-A@' }
    }
    function global:Invoke-RestMethod { throw 'UNSAFE_TASK_INTAKE_REACHED' }
    foreach ($exitCode in 1, 0) {
      $global:preflightExitCode = $exitCode
      foreach ($name in 'Start-FreeSoftphoneLab.ps1', 'Invoke-FreeSoftphoneCall.ps1') {
        $global:preflightInvocations = 0
        $global:dockerInvocations = 0
        $failure = ''
        try {
            if ($name -eq 'Start-FreeSoftphoneLab.ps1') {
                & (Join-Path $testLab $name) -ModelBundle $scratch -VoiceAcceptanceManifest $manifest -SkipBuild -InvokePreflightCall
            }
            else { & (Join-Path $testLab $name) }
        }
        catch { $failure = $_.Exception.Message }
        $expectedFailure = if ($exitCode -eq 1) { 'VieNeu/media preflight failed. No lab call was submitted.' } else { 'TEST_ONLY_UI_BOUNDARY' }
        if ($failure -ne $expectedFailure -or $global:preflightInvocations -ne 1) {
            throw "ENTRY_GUARD_FAILED: $name ($failure)"
        }
        $expectedDocker = if ($name -eq 'Start-FreeSoftphoneLab.ps1' -or $exitCode -eq 0) { 1 } else { 0 }
        if ($global:dockerInvocations -ne $expectedDocker) { throw "ENTRY_SIDE_EFFECT_REACHED: $name" }
        $outcome = if ($exitCode -eq 1) { 'REFUSAL' } else { 'CONTINUE' }
        Write-Output "LAB_SPEECH_ENTRY_${outcome}_PASS script=$name intake=NOT_REACHED"
      }
    }
    # Execute NoUi all the way through intake using stubs, inspecting the actual payload.
    function global:Invoke-RestMethod {
        param($Method, $Uri, $Headers, $ContentType, $Body)
        if ($Uri -like '*/eligibility-checks') { return @{ decision = 'ELIGIBLE_FOR_IVR' } }
        if ($Uri -like '*/tasks') {
            $global:lastIntake = $Body | ConvertFrom-Json
            return @{ decision = 'TASK_ACCEPTED_CALL_JOB_CREATED' }
        }
        throw 'UNEXPECTED_STUB_REQUEST'
    }
    foreach ($region in 'North', 'Central', 'South') {
        foreach ($variant in 'A', 'B', 'MultiItem', 'LongName') {
            $global:preflightInvocations = 0
            $global:lastIntake = $null
            & (Join-Path $testLab 'Invoke-FreeSoftphoneCall.ps1') -Region $region -OrderVariant $variant -NoUi | Out-Null
            $summary = $global:lastIntake.privacy_safe_order_summary
            $expected = switch ($variant) {
                'A' { @(2, 560000, 'Cháo sâm diêm mạch - hạt sen', 1) }
                'B' { @(3, 735000, 'Cháo sâm yến mạch', 1) }
                'MultiItem' { @(2, 1255000, 'Cháo sâm diêm mạch hạt sen', 3) }
                'LongName' { @(4, 1180000, 'Cháo sâm diêm mạch hạt sen yến mạch đậu xanh nấm hương và rau củ dùng cho bữa sáng gia đình', 1) }
            }
            $expectedArea = @{ North='Phường Cửa Nam, thành phố Hà Nội'; Central='Phường Hải Châu, thành phố Đà Nẵng'; South='Phường Phú Khương, tỉnh Vĩnh Long' }[$region]
            if ($global:preflightInvocations -ne 1 -or $summary.items[0].quantity -ne $expected[0] -or
                $summary.total_amount -ne $expected[1] -or $summary.items[0].public_name -ne $expected[2] -or
                $summary.items.Count -ne $expected[3] -or $summary.delivery_area_short -ne $expectedArea -or
                $global:lastIntake.attempt_policy_version -ne 'lab-softphone-v1') {
                throw 'LAB_ORDER_PAYLOAD_MISMATCH'
            }
            if ($variant -eq 'MultiItem' -and ($summary.items[1].quantity -ne 3 -or
                $summary.items[2].public_name -ne 'Trà sâm gừng mật ong' -or $summary.items[2].quantity -ne 1)) {
                throw 'LAB_ORDER_ITEMS_TRUNCATED'
            }
            Write-Output "LAB_ORDER_VARIANT_PASS region=$region variant=$variant preflight=REQUIRED network=STUB"
        }
    }
    $global:headlessRouteTest = $true
    $global:headlessAor = 'LAB-A-AUTO'
    $global:lastIntake = $null
    & (Join-Path $testLab 'Invoke-FreeSoftphoneCall.ps1') -NoUi -HeadlessPeer | Out-Null
    if ($null -eq $global:lastIntake) { throw 'HEADLESS_INTAKE_NOT_REACHED' }
    $global:headlessAor = 'LAB-A'
    $global:lastIntake = $null
    $failure = ''
    try { & (Join-Path $testLab 'Invoke-FreeSoftphoneCall.ps1') -NoUi -HeadlessPeer | Out-Null }
    catch { $failure = $_.Exception.Message }
    if ($failure -ne 'Headless LAB-A route is not pinned to LAB-A-AUTO.' -or $null -ne $global:lastIntake) {
        throw "HEADLESS_ROUTE_REFUSAL_FAILED: $failure"
    }
    Write-Output 'HEADLESS_ROUTE_GUARD_PASS static_peer=ACCEPTED microsip_route=REFUSED intake_on_refusal=NOT_REACHED'
}
finally {
    # Only the directory created by this test, under the system temporary directory.
    $resolved = [System.IO.Path]::GetFullPath($scratch)
    $temporaryRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
    if (-not $resolved.StartsWith($temporaryRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'UNSAFE_TEST_CLEANUP' }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
