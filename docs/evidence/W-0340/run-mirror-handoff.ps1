param([switch]$VerifyOnly)
$ErrorActionPreference = 'Stop'
$taskRepo = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$taskRoot = Join-Path $taskRepo '.artifacts/W-0340'
$taskArchive = Join-Path $taskRoot 'vieneu-mirror-w0340.tar.gz'
$taskChecksum = Join-Path $taskRoot 'vieneu-mirror-w0340.tar.gz.sha256'
$taskInstall = Join-Path $taskRoot 'install-vieneu-mirror-w0340.sh'
$taskExpectedArchive = @('c0f8f1ff','818a09d2','251bf297','9c4962f4','66eb3fab','8dddb078','09f6b558','926d0888') -join ''
$taskExpectedInstall = @('e7cb46e2','5cc42def','b18c2ed8','00a3df24','1e8b150f','17ad5b6d','089818d1','b62ca93d') -join ''
if ((Get-FileHash -Algorithm SHA256 -LiteralPath $taskArchive).Hash.ToLowerInvariant() -ne $taskExpectedArchive) { throw 'Mirror archive checksum mismatch' }
if ((Get-FileHash -Algorithm SHA256 -LiteralPath $taskInstall).Hash.ToLowerInvariant() -ne $taskExpectedInstall) { throw 'Mirror installer checksum mismatch' }
$taskExpectedSum = "$taskExpectedArchive  vieneu-mirror-w0340.tar.gz`n$taskExpectedInstall  install-vieneu-mirror-w0340.sh`n"
if ([IO.File]::ReadAllText($taskChecksum) -cne $taskExpectedSum) { throw 'Checksum file mismatch or non-LF line endings' }
if ($VerifyOnly) { Write-Output 'MIRROR_HANDOFF_CHECKSUMS_PASS'; return }
Write-Output 'Step 1/3: copy verified artifacts to vps61. Enter SSH password in this terminal when prompted.'
scp $taskArchive $taskChecksum $taskInstall 'ssv@192.168.1.61:~/'
if ($LASTEXITCODE -ne 0) { throw 'Copy failed; no installation requested' }
Write-Output 'Step 2/3: verify, store and restore artifacts. No container or call is started.'
ssh ssv@192.168.1.61 'cd /home/ssv && sha256sum -c vieneu-mirror-w0340.tar.gz.sha256 && bash ./install-vieneu-mirror-w0340.sh'
if ($LASTEXITCODE -ne 0) { throw 'Target verification failed; preserve its output for inspection' }
Write-Output 'Step 3/3: return receipt.'
scp 'ssv@192.168.1.61:~/vieneu-mirror-w0340-result.tar.gz' $taskRoot
if ($LASTEXITCODE -ne 0) { throw 'Receipt download failed; target files remain in place' }
Write-Output "MIRROR_RECEIPT_RETURNED $taskRoot/vieneu-mirror-w0340-result.tar.gz; production remains blocked"
