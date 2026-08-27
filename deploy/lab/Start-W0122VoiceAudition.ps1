<#
.SYNOPSIS
    Dựng profile Asterisk/MicroSIP cô lập để Owner nghe 11 giọng W-0122 ở 8 kHz.

.PARAMETER SkipBuild
    Dùng image Asterisk lab hiện có thay vì build lại.

.PARAMETER NoLaunchMicroSip
    Chỉ dựng/kiểm profile, không mở MicroSIP. Dùng cho automated runtime probe.
#>
[CmdletBinding()]
param(
    [switch]$SkipBuild,
    [switch]$NoLaunchMicroSip
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$audioRoot = Join-Path $repositoryRoot 'artifacts\w-0122-voice-audition'
$manifestPath = Join-Path $repositoryRoot 'docs\evidence\W-0122\audition-manifest.json'

if (-not (Test-Path -LiteralPath $audioRoot -PathType Container)) {
    throw "Thiếu audition artifact directory: $audioRoot"
}

$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json -Depth 20
if ($manifest.schema_version -ne 1 -or $manifest.work_id -ne 'W-0122' -or
    @($manifest.results).Count -ne 11) {
    throw 'Audition manifest phải là W-0122 schema 1 với đúng 11 kết quả.'
}

$audioFiles = @(Get-ChildItem -LiteralPath $audioRoot -File -Filter 'audition-*.wav')
if ($audioFiles.Count -ne 11) {
    throw "Audition directory phải có đúng 11 WAV; thực tế có $($audioFiles.Count)."
}

$expectedNames = @($manifest.results | ForEach-Object { [string]$_.file })
if (@($expectedNames | Sort-Object -Unique).Count -ne 11 -or
    ($expectedNames | Where-Object { $_ -notmatch '^audition-v3t-(north|central|south)-[a-z0-9-]+\.wav$' })) {
    throw 'Audition manifest có file trùng hoặc path/name ngoài allowlist.'
}
$actualNames = @($audioFiles.Name | Sort-Object)
if (($actualNames -join "`n") -cne (($expectedNames | Sort-Object) -join "`n")) {
    throw 'Audition directory không khớp exact file set trong manifest.'
}

foreach ($result in $manifest.results) {
    $audioPath = Join-Path $audioRoot $result.file
    if (-not (Test-Path -LiteralPath $audioPath -PathType Leaf)) {
        throw "Thiếu audition WAV: $($result.file)"
    }

    $actualHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $audioPath).Hash.ToLowerInvariant()
    if ($actualHash -cne [string]$result.sha256) {
        throw "Audition WAV hash drift: $($result.file)"
    }

    if ((Get-Item -LiteralPath $audioPath).Length -ne [long]$result.bytes) {
        throw "Audition WAV size drift: $($result.file)"
    }
}

$env:IVR_LAB_ARI_PASSWORD = "ari-$([Guid]::NewGuid().ToString('N'))"
$env:IVR_LAB_SIP_PASSWORD = "sip-$([Guid]::NewGuid().ToString('N'))"
$env:IVR_LAB_VOICE_VARIANT = 'A'
$compose = @(
    'compose',
    '--project-name', 'ivr-w0122-audition',
    '-f', 'docker-compose.dev.yml',
    '-f', 'docker-compose.softphone.yml',
    '-f', 'docker-compose.vieneu-tts-audition.yml'
)

Push-Location $repositoryRoot
try {
    $arguments = @($compose + @('up', '-d'))
    if (-not $SkipBuild) {
        $arguments += '--build'
    }
    $arguments += @('w0122-audition-verify', 'asterisk')

    & docker @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "W-0122 audition profile không khởi động được (exit $LASTEXITCODE)."
    }

    $verifyId = (& docker @($compose + @('ps', '-a', '-q', 'w0122-audition-verify'))).Trim()
    if (-not $verifyId) {
        throw 'Không tìm thấy audition verifier container.'
    }
    $verifyExit = (& docker inspect $verifyId --format '{{.State.ExitCode}}').Trim()
    if ($verifyExit -ne '0') {
        & docker logs $verifyId
        throw "Audition verifier fail (exit $verifyExit)."
    }

    $asteriskId = (& docker @($compose + @('ps', '-q', 'asterisk'))).Trim()
    if (-not $asteriskId) {
        throw 'Không tìm thấy Asterisk audition container.'
    }

    $healthy = $false
    for ($attempt = 1; $attempt -le 20; $attempt++) {
        $health = (& docker inspect $asteriskId --format '{{if .State.Health}}{{.State.Health.Status}}{{end}}').Trim()
        if ($health -eq 'healthy') {
            $healthy = $true
            break
        }
        Start-Sleep -Seconds 3
    }
    if (-not $healthy) {
        & docker logs $asteriskId
        throw 'Asterisk audition profile không healthy trong 60 giây.'
    }

    & docker exec $asteriskId asterisk -rx 'dialplan show 12200@ivr-lab'
    if ($LASTEXITCODE -ne 0) {
        throw 'Asterisk không load extension audition 12200.'
    }

    if (-not $NoLaunchMicroSip) {
        & (Join-Path $PSScriptRoot 'Install-Launch-MicroSip.ps1') `
            -SipPassword $env:IVR_LAB_SIP_PASSWORD
    }

    Write-Host 'W0122_AUDITION_PROFILE_READY files=11 extension_all=12200 outbound=DENIED' -ForegroundColor Green
    Write-Host '12201-12206: Bắc — Trúc Ly, Ngọc Linh, Đoan Trang, Mai Anh, Quỳnh Anh, Ngọc Huyền'
    Write-Host '12207: Trung — Ngọc Trân'
    Write-Host '12208-12211: Nam — Thục Đoan, Thùy Dung, Mỹ Duyên, Kim Thanh'
    if (-not $NoLaunchMicroSip) {
        Write-Host 'Trong MicroSIP, gọi 12200 để nghe cả 11 hoặc gọi từng số ở trên để nghe lại.'
    }
}
catch {
    Write-Warning 'Startup fail; đang rollback project audition cô lập.'
    & docker @($compose + @('down', '--remove-orphans')) | Out-Host
    throw
}
finally {
    Pop-Location
}
