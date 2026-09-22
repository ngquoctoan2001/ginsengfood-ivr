param([switch]$VerifyOnly)
$ErrorActionPreference = 'Stop'
$taskRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$artifactDir = Join-Path $taskRoot '.artifacts/W-0344'
$pins = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'handoff-pins.json') -Raw | ConvertFrom-Json
foreach ($entry in $pins.files.PSObject.Properties) {
    $path = [IO.Path]::GetFullPath((Join-Path $taskRoot $entry.Name))
    if (-not $path.StartsWith($taskRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Pin path outside workspace' }
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $entry.Value) { throw "Hash mismatch: $($entry.Name)" }
}
$installerPin = $pins.files.'docs/evidence/W-0344/install-s5-full-flow.sh'
if ($installerPin -notmatch '^[a-f0-9]{64}$') { throw 'Installer checksum entry missing' }
if ($VerifyOnly) { Write-Output 'W0344_HANDOFF_CHECKSUMS_PASS'; return }
$runId = (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0,8)
$remoteDir = '/home/ssv/ivr-full-flow-w0344-' + $runId
$localDir = Join-Path $artifactDir ('s5-' + $runId)
New-Item -ItemType Directory -Path $localDir -ErrorAction Stop | Out-Null
$ssh = Join-Path $env:WINDIR 'System32/OpenSSH/ssh.exe'
$scp = Join-Path $env:WINDIR 'System32/OpenSSH/scp.exe'
Write-Host 'Nhap mat khau SSH trong terminal khi duoc hoi. Khong gui mat khau vao chat.'
& $ssh -o StrictHostKeyChecking=yes -o ConnectTimeout=15 ssv@192.168.1.61 "mkdir -m 700 $remoteDir"
if ($LASTEXITCODE -ne 0) { throw 'Khong tao duoc thu muc rieng tren S5' }
$archive = Join-Path $artifactDir 'ivr-full-flow-w0344.tar.gz'
$checksum = $archive + '.sha256'
$installer = Join-Path $PSScriptRoot 'install-s5-full-flow.sh'
& $scp -o StrictHostKeyChecking=yes $archive $checksum $installer "ssv@192.168.1.61:${remoteDir}/"
if ($LASTEXITCODE -ne 0) { throw 'Chuyen goi that bai; chua chay full-flow' }
# Start under an independent session; reconnect the same run if SSH drops.
& (Join-Path $PSScriptRoot 'resume-s5-full-flow.ps1') -RunId $runId
