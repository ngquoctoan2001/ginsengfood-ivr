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
$command = "cd $remoteDir && printf '%s  %s\n' '$installerPin' install-s5-full-flow.sh | sha256sum -c - && bash ./install-s5-full-flow.sh"
& $ssh -o StrictHostKeyChecking=yes -o ServerAliveInterval=20 -o ServerAliveCountMax=6 ssv@192.168.1.61 $command
$runExit = $LASTEXITCODE
# Retrieve failed runs as well; do not turn an execution error into missing evidence.
& $scp -o StrictHostKeyChecking=yes "ssv@192.168.1.61:${remoteDir}/result.tar.gz" "ssv@192.168.1.61:${remoteDir}/result.tar.gz.sha256" "ssv@192.168.1.61:${remoteDir}/launcher.log" $localDir
$receiptExit = $LASTEXITCODE
@{ WorkId='W-0344'; RunExitCode=$runExit; ReceiptExitCode=$receiptExit; RemoteDirectory=$remoteDir; LocalDirectory=$localDir } |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $localDir 'handoff-result.json') -Encoding utf8
if ($receiptExit -ne 0) { throw "Chua lay duoc day du receipt; giu nguyen $remoteDir tren S5" }
$expected = ((Get-Content -LiteralPath (Join-Path $localDir 'result.tar.gz.sha256') -Raw).Trim() -split '\s+')[0]
$actual = (Get-FileHash -LiteralPath (Join-Path $localDir 'result.tar.gz') -Algorithm SHA256).Hash.ToLowerInvariant()
if ($actual -ne $expected) { throw 'Receipt hash mismatch' }
Write-Output "W0344_RECEIPT_VERIFIED $localDir"
if ($runExit -ne 0) { throw "Luot S5 chua dat (exit=$runExit); da lay receipt ve de kiem tra" }
Write-Output 'W0344_S5_COMMAND_COMPLETED: gui vi tri receipt va cac dong ket qua cuoi cho Codex kiem tra.'
