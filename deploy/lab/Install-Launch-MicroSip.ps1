[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-Za-z0-9._-]{12,120}$')]
    [string]$SipPassword
)

$ErrorActionPreference = 'Stop'
$toolRoot = Join-Path $PSScriptRoot '.local-tools\MicroSIP-3.22.12'
$archive = Join-Path $PSScriptRoot '.local-tools\MicroSIP-3.22.12.zip'
$downloadUrl = 'https://www.microsip.org/download/MicroSIP-3.22.12.zip'
$expectedSha256 = '59738CA40C217A87DA43A57FF891CC1D5C45C16EE62F578B2CCAB05BCA9B2362'
$expectedExecutableSha256 = '132E749F6D4F5D6A90C45BE2ED2FF993A96C8F9094D35FF3164A0EA882D7FE1A'

if (Get-Process -Name 'MicroSIP' -ErrorAction SilentlyContinue) {
    throw 'MicroSIP is already running. Close it before launching the isolated W-0104 profile.'
}

if (-not (Test-Path -LiteralPath $toolRoot)) {
    $localRoot = Split-Path -Parent $archive
    New-Item -ItemType Directory -Path $localRoot -Force | Out-Null
    if (-not (Test-Path -LiteralPath $archive)) {
        Write-Host 'Downloading the official MicroSIP 3.22.12 portable archive...'
        Invoke-WebRequest -Uri $downloadUrl -OutFile $archive
    }

    $actualSha256 = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
    if (-not $actualSha256.Equals($expectedSha256, [StringComparison]::OrdinalIgnoreCase)) {
        throw "MicroSIP archive checksum mismatch. Expected $expectedSha256."
    }

    Expand-Archive -LiteralPath $archive -DestinationPath $toolRoot -Force
}

$executable = Get-ChildItem -LiteralPath $toolRoot -Filter 'MicroSIP.exe' -Recurse |
    Select-Object -First 1
if ($null -eq $executable) {
    throw "MicroSIP.exe was not found after extracting $archive."
}

$actualExecutableSha256 = (Get-FileHash -LiteralPath $executable.FullName -Algorithm SHA256).Hash
if (-not $actualExecutableSha256.Equals(
        $expectedExecutableSha256,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "MicroSIP executable checksum mismatch. Expected $expectedExecutableSha256."
}

$iniPath = Join-Path $executable.DirectoryName 'MicroSIP.ini'
$ini = @"
[Settings]
accountId=1
singleMode=1
enableLog=0
checkUpdates=0
DTMFMethod=2

[Account1]
label=W0104 LAB-A
server=127.0.0.1:5060
proxy=
domain=127.0.0.1:5060
authID=LAB-A
username=LAB-A
password=$SipPassword
displayName=IVR-LAB-A
transport=udp
publish=0
ice=0
rememberPassword=1
"@
# Win32 profile APIs used by MicroSIP treat a UTF-8 BOM as part of the first
# section name. Keep this lab-only INI ASCII so [Settings] and accountId load.
[System.IO.File]::WriteAllText($iniPath, $ini, [System.Text.Encoding]::ASCII)

Write-Host 'Launching MicroSIP with the isolated LAB-A account.'
Write-Host 'Keep it open. When the IVR call appears, answer and press 1 or 0.'
Start-Process -FilePath $executable.FullName -WorkingDirectory $executable.DirectoryName
