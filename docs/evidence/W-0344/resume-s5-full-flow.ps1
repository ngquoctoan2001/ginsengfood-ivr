param(
    [Parameter(Mandatory=$true)][ValidatePattern('^[0-9]{8}-[0-9]{6}-[a-f0-9]{8}$')][string]$RunId,
    [switch]$VerifyOnly
)
$ErrorActionPreference = 'Stop'
$taskRepo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$taskPins = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'handoff-pins.json') -Raw | ConvertFrom-Json
$taskArchivePin = $taskPins.files.'.artifacts/W-0344/ivr-full-flow-w0344.tar.gz'
$taskInstallerPin = $taskPins.files.'docs/evidence/W-0344/install-s5-full-flow.sh'
$taskHelper = Join-Path $PSScriptRoot 'recover-s5-full-flow.py'
foreach ($taskPin in @($taskArchivePin, $taskInstallerPin)) { if ($taskPin -notmatch '^[a-f0-9]{64}$') { throw 'Missing handoff pin' } }
if ($VerifyOnly) { Write-Output 'W0344_RECOVERY_ARGUMENTS_PASS'; return }
$taskRemote = '/home/ssv/ivr-full-flow-w0344-' + $RunId
$taskLocal = Join-Path $taskRepo ('.artifacts/W-0344/s5-' + $RunId)
New-Item -ItemType Directory -Force -Path $taskLocal | Out-Null
$taskAttempt = Join-Path $taskLocal ('recovery-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0,8))
New-Item -ItemType Directory -Path $taskAttempt | Out-Null
$taskSsh = Join-Path $env:WINDIR 'System32/OpenSSH/ssh.exe'
$taskScp = Join-Path $env:WINDIR 'System32/OpenSSH/scp.exe'
Write-Host 'Tai su dung goi da chep; chi gui helper nho. Nhap mat khau SSH tai terminal.'
$taskHelperHash = (Get-FileHash -LiteralPath $taskHelper -Algorithm SHA256).Hash.ToLowerInvariant()
$taskHelperSum = Join-Path $taskAttempt 'recover-s5-full-flow.py.sha256'
[IO.File]::WriteAllText($taskHelperSum, "$taskHelperHash  recover-s5-full-flow.py`n", [Text.UTF8Encoding]::new($false))
& $taskScp -o StrictHostKeyChecking=yes $taskHelper $taskHelperSum "ssv@192.168.1.61:${taskRemote}/"
if ($LASTEXITCODE -ne 0) { throw 'Khong chep duoc helper; chua yeu cau chay lai' }
# All remote words below are fixed paths, validated run IDs or lowercase hex digests.
$taskCommand = "cd $taskRemote && sha256sum -c recover-s5-full-flow.py.sha256 && python3 -B recover-s5-full-flow.py --root $taskRemote --archive-sha256 $taskArchivePin --installer-sha256 $taskInstallerPin"
Write-Host "Helper SHA256=$taskHelperHash. Thu muc: $taskRemote"
& $taskSsh -o StrictHostKeyChecking=yes -o ServerAliveInterval=20 -o ServerAliveCountMax=6 ssv@192.168.1.61 $taskCommand
$taskExit = $LASTEXITCODE
@{ WorkId='W-0344'; RemoteDirectory=$taskRemote; ExitCode=$taskExit; HelperSha256=$taskHelperHash; RealCustomerCalls='NO' } |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $taskAttempt 'recovery-handoff.json') -Encoding utf8
if ($taskExit -eq 255) {
    throw "SSH lai ngat. Neu da khoi dong, bai kiem van chay doc lap. Chay lai cung lenh resume voi RunId $RunId; khong tao luot moi."
}
& $taskScp -o StrictHostKeyChecking=yes "ssv@192.168.1.61:${taskRemote}/recovery-diagnostic.tar.gz" "ssv@192.168.1.61:${taskRemote}/recovery-diagnostic.tar.gz.sha256" $taskAttempt
if ($LASTEXITCODE -ne 0) { throw "Chua lay duoc chan doan; giu nguyen $taskRemote" }
$taskDiagnostic = Join-Path $taskAttempt 'recovery-diagnostic.tar.gz'
$taskExpected = ((Get-Content -LiteralPath ($taskDiagnostic + '.sha256') -Raw).Trim() -split '\s+')[0]
if ($taskExpected -notmatch '^[a-f0-9]{64}$' -or (Get-FileHash -LiteralPath $taskDiagnostic -Algorithm SHA256).Hash.ToLowerInvariant() -ne $taskExpected) { throw 'Diagnostic checksum mismatch' }
if ($taskExit -ne 0) { throw "Da lay chan doan ve $taskAttempt (exit=$taskExit). Khong tu xoa hoac chay de luot dang do; gui output de kiem tra." }
& $taskScp -o StrictHostKeyChecking=yes "ssv@192.168.1.61:${taskRemote}/result.tar.gz" "ssv@192.168.1.61:${taskRemote}/result.tar.gz.sha256" $taskAttempt
if ($LASTEXITCODE -ne 0) { throw "Receipt chua tai xong; chay lai cung lenh resume voi RunId $RunId" }
$taskResult = Join-Path $taskAttempt 'result.tar.gz'
$taskExpected = ((Get-Content -LiteralPath ($taskResult + '.sha256') -Raw).Trim() -split '\s+')[0]
if ($taskExpected -notmatch '^[a-f0-9]{64}$' -or (Get-FileHash -LiteralPath $taskResult -Algorithm SHA256).Hash.ToLowerInvariant() -ne $taskExpected) { throw 'Receipt checksum mismatch' }
Write-Output "W0344_RECOVERY_RECEIPT_VERIFIED $taskAttempt"
Write-Output 'Receipt da ve, can Codex kiem ket qua 7 ca; production van tat.'
