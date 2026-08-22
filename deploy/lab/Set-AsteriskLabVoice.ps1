[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('A', 'B')]
    [string]$Variant,

    [string]$AsteriskContainer = 'ginsengfood-ivr-dev-asterisk-1'
)

$ErrorActionPreference = 'Stop'
$normalizedVariant = $Variant.ToLowerInvariant()
$sourceFile = "/opt/ivr-lab/audio/ivr-lab-order-confirmation-$normalizedVariant.wav"
$targetFile = '/var/lib/asterisk/sounds/ivr-lab-order-confirmation.wav'

$running = & docker inspect --format '{{.State.Running}}' $AsteriskContainer 2>$null
if ($LASTEXITCODE -ne 0 -or $running -ne 'true') {
    throw "Asterisk container '$AsteriskContainer' is not running."
}

& docker exec $AsteriskContainer sh -c 'cd /opt/ivr-lab/audio && sha256sum --check --strict SHA256SUMS'
if ($LASTEXITCODE -ne 0) {
    throw 'Pinned W-0104 audio checksum validation failed.'
}

& docker exec $AsteriskContainer sh -c "cp '$sourceFile' '$targetFile.tmp' && mv '$targetFile.tmp' '$targetFile'"
if ($LASTEXITCODE -ne 0) {
    throw "Unable to activate W-0104 voice variant $Variant."
}

$voiceId = if ($Variant -eq 'A') { 'vi-VN-HoaiMyNeural' } else { 'vi-VN-NamMinhNeural' }
Write-Host "W-0104 voice $Variant active: $voiceId (fake data, MicroSIP lab only)."
