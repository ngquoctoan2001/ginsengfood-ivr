[CmdletBinding()]
param(
    [switch]$PurgeData,
    [switch]$KeepMicroSip
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

# Docker Compose validates interpolated values even for `down`. These values are
# deliberately disposable placeholders; the running secrets never leave the
# process that started the lab, and `down` mounts nothing.
$placeholders = [ordered]@{
    IVR_LAB_ARI_PASSWORD                 = 'w0104-stop-placeholder-ari'
    IVR_LAB_SIP_PASSWORD                 = 'w0104-stop-placeholder-sip'
    IVR_VIENEU_NORTH_VOICE_ID            = 'stop-placeholder-north'
    IVR_VIENEU_CENTRAL_VOICE_ID          = 'stop-placeholder-central'
    IVR_VIENEU_SOUTH_VOICE_ID            = 'stop-placeholder-south'
    IVR_VIENEU_MODEL_BUNDLE              = $repositoryRoot
    IVR_VIENEU_VOICE_ACCEPTANCE_MANIFEST = $repositoryRoot
}
foreach ($name in $placeholders.Keys) {
    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) {
        Set-Item -Path "Env:$name" -Value $placeholders[$name]
    }
}

if (-not $KeepMicroSip) {
    Get-Process -Name 'MicroSIP' -ErrorAction SilentlyContinue | Stop-Process
}

$arguments = @(
    'compose',
    '-f', 'docker-compose.dev.yml',
    '-f', 'docker-compose.softphone.yml',
    '-f', 'docker-compose.vieneu-tts.yml',
    'down'
)

if ($PurgeData) {
    $arguments += '--volumes'
}

Push-Location $repositoryRoot
try {
    & docker @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "The VieNeu softphone lab failed to stop (exit $LASTEXITCODE)."
    }
}
finally {
    Pop-Location
}
