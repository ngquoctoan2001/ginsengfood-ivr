<#
.SYNOPSIS
    Dựng Asterisk/MicroSIP software lab bằng dữ liệu giả.

.PARAMETER SkipBuild
    Dùng image hiện có thay vì build lại.

.PARAMETER InvokePreflightCall
    Gửi một task gọi fake sau khi mở MicroSIP. Mặc định không gọi để việc dựng lab không tạo
    một lượt ngoài kế hoạch nghiệm thu.

.PARAMETER VoiceVariant
    Biến thể A/B/C của W-0104 cho đường một-giọng lịch sử.
#>
[CmdletBinding()]
param(
    [switch]$SkipBuild,

    [switch]$InvokePreflightCall,

    [ValidateSet('A', 'B', 'C')]
    [string]$VoiceVariant = 'A'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$env:IVR_LAB_ARI_PASSWORD = "ari-$([Guid]::NewGuid().ToString('N'))"
$env:IVR_LAB_SIP_PASSWORD = "sip-$([Guid]::NewGuid().ToString('N'))"
$env:IVR_LAB_VOICE_VARIANT = $VoiceVariant
$compose = @(
    'compose',
    '-f', 'docker-compose.dev.yml',
    '-f', 'docker-compose.softphone.yml'
)

Push-Location $repositoryRoot
try {
    $arguments = @($compose + @('up', '-d'))
    if (-not $SkipBuild) {
        $arguments += '--build'
    }

    & docker @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "The W-0104 Docker profile failed to start (exit $LASTEXITCODE)."
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
