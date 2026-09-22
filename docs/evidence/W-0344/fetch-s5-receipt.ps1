param(
    [Parameter(Mandatory=$true)][ValidatePattern('^[0-9]{8}-[0-9]{6}-[a-f0-9]{8}$')][string]$RunId,
    [switch]$VerifyOnly
)
$ErrorActionPreference = 'Stop'
$taskRepo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$taskRemote = '/home/ssv/ivr-full-flow-w0344-' + $RunId + '/result.tar.gz'
# A single read-only SSH command. No installer, Docker or recovery start command is invoked.
# Text transport avoids binary stdout corruption in Windows PowerShell 5.1.
$taskCommand = "sha256sum $taskRemote && base64 -w 0 $taskRemote"
if ($VerifyOnly) {
    Write-Output "W0344_FETCH_READ_ONLY_ARGUMENTS_PASS $taskRemote"
    return
}
$taskSsh = Join-Path $env:WINDIR 'System32/OpenSSH/ssh.exe'
$taskAttempt = Join-Path $taskRepo ('.artifacts/W-0344/s5-' + $RunId + '/fetch-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0,8))
New-Item -ItemType Directory -Path $taskAttempt | Out-Null
Write-Host 'Chi doc receipt da co qua mot phien SSH; khong chay lai bai kiem. Nhap mat khau tai terminal.'
$taskEnvelope = @(& $taskSsh -o StrictHostKeyChecking=yes -o ConnectTimeout=15 -o ServerAliveInterval=15 -o ServerAliveCountMax=4 ssv@192.168.1.61 $taskCommand)
$taskExit = $LASTEXITCODE
@{ WorkId='W-0344'; RunId=$RunId; RemoteReceipt=$taskRemote; SshExit=$taskExit; Operation='READ_ONLY_RECEIPT_FETCH'; RealCustomerCalls='NO' } |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $taskAttempt 'fetch-status.json') -Encoding utf8
if ($taskExit -ne 0) {
    throw "SSH chua doc duoc receipt (exit=$taskExit). Du lieu server giu nguyen; co the chay lai cung lenh fetch."
}
if ($taskEnvelope.Count -ne 2 -or $taskEnvelope[0] -cnotmatch '^([a-f0-9]{64})  (/home/ssv/ivr-full-flow-w0344-[0-9]{8}-[0-9]{6}-[a-f0-9]{8}/result\.tar\.gz)$') {
    throw 'Receipt response missing checksum or payload; no file accepted'
}
$taskExpectedHash = $Matches[1]
if ($Matches[2] -cne $taskRemote) { throw 'Receipt path mismatch' }
if ($taskEnvelope[1].Length -gt 44 * 1024 * 1024 -or $taskEnvelope[1] -cnotmatch '^[A-Za-z0-9+/]+={0,2}$') {
    throw 'Receipt response is too large or invalid base64'
}
$taskBytes = [Convert]::FromBase64String($taskEnvelope[1])
if ($taskBytes.Length -gt 32 * 1024 * 1024) { throw 'Receipt exceeds 32 MiB bound' }
$taskHasher = [Security.Cryptography.SHA256]::Create()
try { $taskActualHash = [BitConverter]::ToString($taskHasher.ComputeHash($taskBytes)).Replace('-','').ToLowerInvariant() }
finally { $taskHasher.Dispose() }
if ($taskActualHash -cne $taskExpectedHash) { throw 'Receipt checksum mismatch; no archive written' }
$taskReceipt = Join-Path $taskAttempt 'result.tar.gz'
[IO.File]::WriteAllBytes($taskReceipt, $taskBytes)
[IO.File]::WriteAllText(($taskReceipt + '.sha256'), "$taskExpectedHash  result.tar.gz`n", [Text.UTF8Encoding]::new($false))
Write-Output "W0344_RECEIPT_FETCHED_AND_HASH_VERIFIED $taskReceipt"
Write-Output 'Gui dong ket qua nay cho Codex doc stack-start.log. Checksum dung khong co nghia bai kiem da dat.'
