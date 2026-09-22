param([switch]$VerifyOnly)
$ErrorActionPreference = 'Stop'
$taskRepo = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$taskRoot = Join-Path $taskRepo '.artifacts/W-0343'
$taskArchive = Join-Path $taskRoot 'vieneu-release-w0343.tar.gz'
$taskChecksum = Join-Path $taskRoot 'vieneu-release-w0343.tar.gz.sha256'
$taskInstall = Join-Path $taskRoot 'install-vieneu-w0343.py'
$taskArchiveHash = '668f02f753b490cf29fe1d9ffd73f481f23db514deeb3ffa389923353008d3ff'
$taskInstallHash = '6e3023145a057e28b507df25e7f1dc35eb97def93748f4799d83b5a7bbc650a0'
$taskCatalogHash = '39de4a043c02f4cf318c242eda5fc450315922f8ea52d54c77b625da78019b3a'
if ((Get-FileHash -Algorithm SHA256 -LiteralPath $taskArchive).Hash.ToLowerInvariant() -ne $taskArchiveHash) { throw 'Archive checksum mismatch' }
if ((Get-FileHash -Algorithm SHA256 -LiteralPath $taskInstall).Hash.ToLowerInvariant() -ne $taskInstallHash) { throw 'Installer checksum mismatch' }
$taskExpectedSum = "$taskArchiveHash  vieneu-release-w0343.tar.gz`n$taskInstallHash  install-vieneu-w0343.py`n"
if ([IO.File]::ReadAllText($taskChecksum) -cne $taskExpectedSum) { throw 'Checksum file mismatch or non-LF endings' }
if ($VerifyOnly) { Write-Output 'S5_HANDOFF_CHECKSUMS_PASS'; return }
$taskSsh = Join-Path $env:WINDIR 'System32/OpenSSH/ssh.exe'
$taskScp = Join-Path $env:WINDIR 'System32/OpenSSH/scp.exe'
$taskTar = Join-Path $env:WINDIR 'System32/tar.exe'
foreach ($taskTool in @($taskSsh, $taskScp, $taskTar)) { if (-not (Test-Path -LiteralPath $taskTool)) { throw "Missing tool: $taskTool" } }
if (Test-Path -LiteralPath (Join-Path $taskRoot 'vieneu-release-w0343-result.tar.gz')) { throw 'A receipt already exists locally; preserve it before another handoff' }
Write-Output 'Step 1/3: copy candidate to vps61. Enter SSH password in this terminal when prompted.'
& $taskScp $taskArchive $taskChecksum $taskInstall 'ssv@192.168.1.61:~/'
if ($LASTEXITCODE -ne 0) { throw 'Copy failed; deployment not requested' }
Write-Output 'Step 2/3: verify mirror, start/restart candidate, then run two 450-second soak windows plus comparison/recovery cases. TTS=2CPU/4GiB, profile=30/90/120. No calls.'
& $taskSsh ssv@192.168.1.61 'cd /home/ssv && sha256sum -c vieneu-release-w0343.tar.gz.sha256 && python3 -B ./install-vieneu-w0343.py'
$taskRemoteExit = $LASTEXITCODE
Write-Output 'Step 3/3: return the full result, including failures if any.'
& $taskScp 'ssv@192.168.1.61:~/vieneu-release-w0343-result.tar.gz' 'ssv@192.168.1.61:~/vieneu-release-w0343-result.tar.gz.sha256' $taskRoot
if ($LASTEXITCODE -ne 0) { throw "Could not fetch result; remote exit=$taskRemoteExit. Keep terminal output for review." }
$taskResult = Join-Path $taskRoot 'vieneu-release-w0343-result.tar.gz'
$taskResultSum = [IO.File]::ReadAllText("$taskResult.sha256")
if ($taskResultSum -cnotmatch '^([a-f0-9]{64})  vieneu-release-w0343-result\.tar\.gz\n$') { throw 'Invalid receipt checksum file' }
if ((Get-FileHash -Algorithm SHA256 -LiteralPath $taskResult).Hash.ToLowerInvariant() -ne $Matches[1]) { throw 'Returned result checksum mismatch' }
if ($taskRemoteExit -ne 0) { throw "Target check failed (exit=$taskRemoteExit). Full evidence returned to $taskResult" }
$taskReceiptText = & $taskTar -xOf $taskResult result/receipt.json
if ($LASTEXITCODE -ne 0) { throw 'Missing deployment receipt' }
$taskReceipt = ($taskReceiptText -join "`n") | ConvertFrom-Json
if ($taskReceipt.catalog_sha256 -ne $taskCatalogHash -or $taskReceipt.scope -ne 'S5_TARGET' -or
    $taskReceipt.status -ne 'PASS' -or $taskReceipt.REAL_CUSTOMER_CALL_ALLOWED -ne 'NO' -or
    $taskReceipt.production -ne 'BLOCKED' -or $taskReceipt.existing_services_unchanged -ne $true -or
    $taskReceipt.profile.tts_cpus -ne 2 -or $taskReceipt.profile.tts_memory_gib -ne 4 -or
    $taskReceipt.profile.segment_ms -ne 30000 -or $taskReceipt.profile.queue_ms -ne 90000 -or
    $taskReceipt.profile.preparation_ms -ne 120000 -or @($taskReceipt.runs).Count -ne 2) { throw 'Receipt scope/profile/result mismatch' }
Write-Output "S5_DEPLOYMENT_CHECK_RECEIPT_RETURNED $taskResult; production BLOCKED; REAL_CUSTOMER_CALL_ALLOWED=NO"
