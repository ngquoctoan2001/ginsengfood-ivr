<#
.SYNOPSIS
    Dựng Asterisk/MicroSIP software lab bằng dữ liệu giả, đọc bằng VieNeu-TTS.

.DESCRIPTION
    Lab chỉ có một bộ đọc: VieNeu-TTS tự host (W-0122), chạy làm sidecar của worker qua
    `docker-compose.vieneu-tts.yml`. Script luôn nạp đủ ba file compose; thiếu overlay thì worker
    không có bộ đọc nào và cuộc gọi fail closed.

    Ba giọng miền lấy thẳng từ manifest nghiệm thu Owner đã ký (OD-VOICE-06), không có bản chép
    tay thứ hai. Model không nằm trong git: -ModelBundle phải trỏ tới bundle đã qua
    `deploy/tts/scripts/verify-model.py --mode nonprod` (xem deploy/tts/README.md).

.PARAMETER ModelBundle
    Thư mục bundle model VieNeu đã kiểm. Bắt buộc.

.PARAMETER VoiceAcceptanceManifest
    Manifest nghiệm thu giọng Owner đã ký. Mặc định
    `docs/evidence/W-0122/voice-acceptance-manifest.json`.

.PARAMETER SkipBuild
    Dùng image hiện có thay vì build lại.

.PARAMETER InvokePreflightCall
    Gửi một task gọi fake sau khi mở MicroSIP. Mặc định không gọi để việc dựng lab không tạo
    một lượt ngoài kế hoạch nghiệm thu.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$ModelBundle,

    [string]$VoiceAcceptanceManifest,

    [switch]$SkipBuild,

    [switch]$InvokePreflightCall
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

if (-not $VoiceAcceptanceManifest) {
    $VoiceAcceptanceManifest = Join-Path $repositoryRoot 'docs\evidence\W-0122\voice-acceptance-manifest.json'
}

if (-not (Test-Path -LiteralPath $ModelBundle -PathType Container)) {
    throw "Không thấy bundle model VieNeu: $ModelBundle"
}

if (-not (Test-Path -LiteralPath $VoiceAcceptanceManifest -PathType Leaf)) {
    throw "Không thấy manifest nghiệm thu giọng: $VoiceAcceptanceManifest"
}

$acceptance = Get-Content -LiteralPath $VoiceAcceptanceManifest -Raw -Encoding utf8 | ConvertFrom-Json
$invariant = [System.Globalization.CultureInfo]::InvariantCulture
foreach ($region in 'North', 'Central', 'South') {
    $selection = $acceptance.selections.$region
    if (-not $selection -or [string]::IsNullOrWhiteSpace($selection.voice_id)) {
        throw "Manifest nghiệm thu không có giọng cho miền $region."
    }

    $upper = $region.ToUpperInvariant()
    Set-Item -Path "Env:IVR_VIENEU_${upper}_VOICE_ID" -Value $selection.voice_id
    Set-Item -Path "Env:IVR_VIENEU_${upper}_SPEAKING_RATE" `
        -Value ([double]$selection.speaking_rate).ToString('0.0##', $invariant)
}

$env:IVR_VIENEU_MODEL_BUNDLE = (Resolve-Path -LiteralPath $ModelBundle).Path
$env:IVR_VIENEU_VOICE_ACCEPTANCE_MANIFEST = (Resolve-Path -LiteralPath $VoiceAcceptanceManifest).Path
$env:IVR_LAB_ARI_PASSWORD = "ari-$([Guid]::NewGuid().ToString('N'))"
$env:IVR_LAB_SIP_PASSWORD = "sip-$([Guid]::NewGuid().ToString('N'))"
$compose = @(
    'compose',
    '-f', 'docker-compose.dev.yml',
    '-f', 'docker-compose.softphone.yml',
    '-f', 'docker-compose.vieneu-tts.yml'
)

Push-Location $repositoryRoot
try {
    $arguments = @($compose + @('up', '-d'))
    if (-not $SkipBuild) {
        $arguments += '--build'
    }

    & docker @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "The VieNeu softphone lab failed to start (exit $LASTEXITCODE)."
    }

    & (Join-Path $PSScriptRoot 'Install-Launch-MicroSip.ps1') `
        -SipPassword $env:IVR_LAB_SIP_PASSWORD

    if ($InvokePreflightCall) {
        & (Join-Path $PSScriptRoot 'Invoke-FreeSoftphoneCall.ps1')
    }
    else {
        Write-Host 'Lab đã khởi động; chưa gửi task gọi. Chạy Invoke-FreeSoftphoneCall.ps1 khi sẵn sàng nghe.' -ForegroundColor Green
    }
}
finally {
    Pop-Location
}
