[CmdletBinding()]
param(
    [switch]$SkipBuild,

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
    & (Join-Path $PSScriptRoot 'Invoke-FreeSoftphoneCall.ps1')
}
finally {
    Pop-Location
}
