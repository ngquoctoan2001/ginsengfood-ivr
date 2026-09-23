param([switch]$VerifyOnly, [string]$ResumeRunId)
$ErrorActionPreference = 'Stop'
$taskRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$pins = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'cpu-retry-pins.json') -Raw | ConvertFrom-Json
# The pinned archive names the package revision; a new revision needs a rebuild and re-pin, not an edit here.
$archiveEntry = @($pins.files.PSObject.Properties.Name | Where-Object { $_ -match '^\.artifacts/W-0344/cpu-portable-r[0-9]+/ivr-full-flow-w0344-cpu-r[0-9]+\.tar\.gz$' })
if ($archiveEntry.Count -ne 1) { throw 'Pins must name exactly one retry archive' }
$archive = [IO.Path]::GetFullPath((Join-Path $taskRoot $archiveEntry[0]))
$artifactDir = Split-Path -Parent $archive
foreach ($entry in $pins.files.PSObject.Properties) {
    $path = [IO.Path]::GetFullPath((Join-Path $taskRoot $entry.Name))
    if (-not $path.StartsWith($taskRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Pin path outside workspace' }
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $entry.Value) { throw "Hash mismatch: $($entry.Name)" }
}
$installerPin = $pins.files.'docs/evidence/W-0344/install-s5-cpu-retry.py'
if ($installerPin -notmatch '^[a-f0-9]{64}$') { throw 'Installer checksum entry missing' }
if ($pins.base_remote_kit -notmatch '^/home/ssv/ivr-full-flow-w0344-[0-9]{8}-[0-9]{6}-[a-f0-9]{8}/full-flow-s5$') { throw 'Unexpected base kit path' }
if ($VerifyOnly) { Write-Output 'W0344_CPU_RETRY_CHECKSUMS_PASS'; return }
$ssh = Join-Path $env:WINDIR 'System32/OpenSSH/ssh.exe'
$scp = Join-Path $env:WINDIR 'System32/OpenSSH/scp.exe'
$tar = Join-Path $env:WINDIR 'System32/tar.exe'
if (-not (Test-Path -LiteralPath $tar)) { throw 'Windows tar.exe is required to inspect the receipt' }
# The test runs detached on S5: a dropped SSH session stops the progress view, never the test.
$follow = @'
while ! test -f run.status; do
  if ! kill -0 "$(cat run.pid 2>/dev/null)" 2>/dev/null && ! test -f run.status; then
    echo 'W0344_RETRY_INTERRUPTED: tien trinh da dung ma chua co run.status; giu nguyen thu muc'; exit 5
  fi
  sleep 20; tail -n 1 launcher.log 2>/dev/null
done
exit "$(cat run.status)"
'@
if ($ResumeRunId) {
    if ($ResumeRunId -notmatch '^[0-9]{8}-[0-9]{6}-[a-f0-9]{8}$') { throw 'ResumeRunId must look like 20260923-101500-1a2b3c4d' }
    $runId = $ResumeRunId
    $remoteDir = '/home/ssv/ivr-full-flow-w0344-cpu-' + $runId
    $localDir = Join-Path $artifactDir ('s5-' + $runId)
    New-Item -ItemType Directory -Path $localDir -Force | Out-Null
    if (Test-Path -LiteralPath (Join-Path $localDir 'receipt-download.tar.gz')) { throw "Receipt da co o $localDir; giu nguyen, khong tai de" }
    Write-Host "Cho luot $runId ket thuc tren S5 (khong chay lai). Nhap mat khau SSH khi duoc hoi."
    $script = "cd $remoteDir || exit 2`n" + $follow
} else {
    $runId = (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0,8)
    $remoteDir = '/home/ssv/ivr-full-flow-w0344-cpu-' + $runId
    $localDir = Join-Path $artifactDir ('s5-' + $runId)
    New-Item -ItemType Directory -Path $localDir -ErrorAction Stop | Out-Null
    Write-Host "RunId $runId. Chi chuyen ban va Asterisk + cases.py. Nhap mat khau SSH trong terminal khi duoc hoi."
    & $ssh -o StrictHostKeyChecking=yes -o ConnectTimeout=15 ssv@192.168.1.61 "mkdir -m 700 $remoteDir"
    if ($LASTEXITCODE -ne 0) { throw 'Khong tao duoc thu muc retry rieng tren S5' }
    $checksum = $archive + '.sha256'
    $installer = Join-Path $PSScriptRoot 'install-s5-cpu-retry.py'
    & $scp -o StrictHostKeyChecking=yes $archive $checksum $installer "ssv@192.168.1.61:${remoteDir}/"
    if ($LASTEXITCODE -ne 0) { throw 'Chuyen ban va that bai; chua chay full-flow' }
    $start = @'
cd __REMOTE__ || exit 2
printf '%s  %s\n' '__PIN__' install-s5-cpu-retry.py | sha256sum -c - || exit 2
set -o noclobber
{ : > run.started; } 2>/dev/null || { echo 'W0344_RETRY_ALREADY_STARTED: dung -ResumeRunId, khong chay lai'; exit 3; }
set +o noclobber
cat > run-detached.sh <<'EOS'
python3 -B ./install-s5-cpu-retry.py --base-kit "$1" > launcher.log 2>&1 < /dev/null
status=$?
if test -f result.tar.gz; then
  sha256sum result.tar.gz > result.tar.gz.sha256
  tar -czf receipt-download.tar.gz result.tar.gz result.tar.gz.sha256 launcher.log
else
  tar -czf receipt-download.tar.gz launcher.log
fi
printf '%s\n' "$status" > run.status.tmp && mv run.status.tmp run.status
EOS
setsid bash ./run-detached.sh __BASE__ < /dev/null > /dev/null 2>&1 &
printf '%s\n' "$!" > run.pid
echo "W0344_RETRY_DETACHED pid=$!"
tail --pid="$!" -n +1 -F launcher.log 2>/dev/null
'@
    $script = $start.Replace('__REMOTE__', $remoteDir).Replace('__PIN__', $installerPin).Replace('__BASE__', $pins.base_remote_kit) + "`n" + $follow
}
$script = $script.Replace("`r`n", "`n")
# Base64 preserves the literal bash syntax through Windows OpenSSH argument parsing.
$encoded = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($script))
& $ssh -o StrictHostKeyChecking=yes -o ServerAliveInterval=20 -o ServerAliveCountMax=6 ssv@192.168.1.61 "printf '%s' '$encoded' | base64 -d | bash"
$runExit = $LASTEXITCODE
if ($runExit -eq 255) {
    @{ WorkId='W-0344'; RunId=$runId; RunExitCode=$runExit; RemoteDirectory=$remoteDir; State='SSH_DROPPED_TEST_CONTINUES' } |
        ConvertTo-Json | Set-Content -LiteralPath (Join-Path $localDir 'handoff-ssh-dropped.json') -Encoding utf8
    throw "Mat ket noi SSH; bai kiem van chay tren S5. Lay ket qua bang: & '$PSCommandPath' -ResumeRunId '$runId'"
}
# 3 and 5 come only from the bash above; the installer and launcher exit 0, 1 or 2.
if ($runExit -eq 3 -or $runExit -eq 5) { throw "Luot $runId can xem lai (exit=$runExit); khong chay lai, giu nguyen $remoteDir" }
# One receipt archive means one password prompt for the entire download.
& $scp -o StrictHostKeyChecking=yes "ssv@192.168.1.61:${remoteDir}/receipt-download.tar.gz" $localDir
$receiptExit = $LASTEXITCODE
@{ WorkId='W-0344'; RunId=$runId; RunExitCode=$runExit; ReceiptExitCode=$receiptExit; RemoteDirectory=$remoteDir; LocalDirectory=$localDir } |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $localDir 'handoff-result.json') -Encoding utf8
if ($receiptExit -ne 0) { throw "Chua lay duoc receipt; giu nguyen $remoteDir tren S5. Thu lai: & '$PSCommandPath' -ResumeRunId '$runId'" }
$download = Join-Path $localDir 'receipt-download.tar.gz'
$members = @(& $tar -tzf $download)
if ($LASTEXITCODE -ne 0 -or $members.Count -eq 0) { throw 'Invalid receipt archive' }
foreach ($member in $members) {
    if ($member -notin @('result.tar.gz','result.tar.gz.sha256','launcher.log')) { throw 'Unexpected receipt archive member' }
}
if (@($members | Select-Object -Unique).Count -ne $members.Count) { throw 'Duplicate receipt member' }
& $tar -xzf $download -C $localDir
if ($LASTEXITCODE -ne 0) { throw 'Receipt extraction failed' }
if (-not (Test-Path -LiteralPath (Join-Path $localDir 'result.tar.gz.sha256'))) { throw "Installer failed before full-flow; xem $localDir/launcher.log" }
$expected = ((Get-Content -LiteralPath (Join-Path $localDir 'result.tar.gz.sha256') -Raw).Trim() -split '\s+')[0]
$actual = (Get-FileHash -LiteralPath (Join-Path $localDir 'result.tar.gz') -Algorithm SHA256).Hash.ToLowerInvariant()
if ($actual -ne $expected) { throw 'Receipt hash mismatch' }
Write-Output "W0344_RECEIPT_VERIFIED $localDir"
if ($runExit -ne 0) { throw "Luot S5 chua dat (exit=$runExit); da lay receipt ve de kiem tra" }
Write-Output 'W0344_S5_COMMAND_COMPLETED: gui cac dong cuoi de kiem tra receipt.'
