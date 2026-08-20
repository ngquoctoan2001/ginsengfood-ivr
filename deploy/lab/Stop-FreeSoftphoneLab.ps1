[CmdletBinding()]
param(
    [switch]$PurgeData,
    [switch]$KeepMicroSip
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

# Docker Compose validates interpolated values even for `down`. These values are
# deliberately disposable placeholders; the running secrets never leave the
# process that started the lab.
if ([string]::IsNullOrWhiteSpace($env:IVR_LAB_ARI_PASSWORD)) {
    $env:IVR_LAB_ARI_PASSWORD = 'w0104-stop-placeholder-ari'
}

if ([string]::IsNullOrWhiteSpace($env:IVR_LAB_SIP_PASSWORD)) {
    $env:IVR_LAB_SIP_PASSWORD = 'w0104-stop-placeholder-sip'
}

if (-not $KeepMicroSip) {
    Get-Process -Name 'MicroSIP' -ErrorAction SilentlyContinue | Stop-Process
}

$arguments = @(
    'compose',
    '-f', 'docker-compose.dev.yml',
    '-f', 'docker-compose.softphone.yml',
    'down'
)

if ($PurgeData) {
    $arguments += '--volumes'
}

Push-Location $repositoryRoot
try {
    & docker @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "The W-0104 Docker profile failed to stop (exit $LASTEXITCODE)."
    }
}
finally {
    Pop-Location
}
