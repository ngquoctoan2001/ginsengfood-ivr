[CmdletBinding()]
param(
    [switch]$KeepMicroSip
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

if ([string]::IsNullOrWhiteSpace($env:IVR_LAB_ARI_PASSWORD)) {
    $env:IVR_LAB_ARI_PASSWORD = 'w0122-stop-placeholder-ari'
}
if ([string]::IsNullOrWhiteSpace($env:IVR_LAB_SIP_PASSWORD)) {
    $env:IVR_LAB_SIP_PASSWORD = 'w0122-stop-placeholder-sip'
}

if (-not $KeepMicroSip) {
    Get-Process -Name 'MicroSIP' -ErrorAction SilentlyContinue | Stop-Process
}

$arguments = @(
    'compose',
    '--project-name', 'ivr-w0122-audition',
    '-f', 'docker-compose.dev.yml',
    '-f', 'docker-compose.softphone.yml',
    '-f', 'docker-compose.vieneu-tts-audition.yml',
    'down', '--remove-orphans'
)

Push-Location $repositoryRoot
try {
    & docker @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "W-0122 audition profile không dừng sạch (exit $LASTEXITCODE)."
    }
}
finally {
    Pop-Location
}

