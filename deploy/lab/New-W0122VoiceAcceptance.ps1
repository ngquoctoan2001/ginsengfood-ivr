<#
.SYNOPSIS
    Sinh Owner voice-acceptance manifest của W-0122 từ lựa chọn của Owner, rồi chạy gate để kiểm.

.DESCRIPTION
    Manifest phải có đúng 23 key, 11 candidate_results đúng thứ tự roster, timestamp RFC3339 kèm
    offset, và tám binding hash/profile. Viết tay gần như chắc chắn bị gate từ chối vài lần — và
    mỗi lần bị từ chối là một lần dễ nảy ra ý "sửa cho nó qua", đúng thứ mà gate sinh ra để chặn.

    Script lấy toàn bộ binding từ deploy/tts/shim/voices.json — cùng declared authority mà shim
    dùng — và chỉ hỏi Owner đúng những thứ chỉ Owner mới biết: ba giọng đã chọn, ai nghe, nghe
    trên tuyến nào, giọng nào bị loại thẳng.

    Script KHÔNG chứng nhận thay Owner. Nó không chạy nếu thiếu người nghe, tuyến nghe, approval
    reference, hoặc thiếu khẳng định tường minh rằng đã nghe đủ 11 giọng.

.PARAMETER North
    voice_id hoặc tên preset của giọng miền Bắc đã chọn. Ví dụ: v3t-north-truc-ly hoặc "Trúc Ly".

.PARAMETER Rejected
    Những voice_id/preset bị loại thẳng vì chất giọng, thay vì chỉ "không được chọn". Owner nên
    dùng cái này: "REJECTED" và "NOT_SELECTED" nói hai chuyện khác nhau với người đọc sau này.

.PARAMETER ConfirmAllElevenHeard
    Bắt buộc. Manifest khẳng định Owner đã nghe đủ 11 giọng qua đúng tuyến 8 kHz. Đó là lời khẳng
    định của một con người, nên nó phải được gõ ra, không được là mặc định của script.

.EXAMPLE
    .\deploy\lab\New-W0122VoiceAcceptance.ps1 `
        -North "Trúc Ly" -Central "Ngọc Trân" -South "Thùy Dung" `
        -Rejected "Mai Anh","Kim Thanh" `
        -Listener "Nguyen Quoc Toan (IVR owner)" `
        -DeviceAndLabRoute "MicroSIP 3.21 -> Asterisk lab 8 kHz, tai nghe có dây" `
        -ApprovalReference "OD-VOICE-06 owner sign-off 2026-08-28" `
        -ConfirmAllElevenHeard
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$North,
    [Parameter(Mandatory)][string]$Central,
    [Parameter(Mandatory)][string]$South,
    [Parameter(Mandatory)][string]$Listener,
    [Parameter(Mandatory)][string]$DeviceAndLabRoute,
    [Parameter(Mandatory)][string]$ApprovalReference,
    [string[]]$Rejected = @(),
    [hashtable]$Notes = @{},
    [string]$OutputPath,
    [switch]$ConfirmAllElevenHeard,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$voiceConfigPath = Join-Path $repositoryRoot 'deploy/tts/shim/voices.json'
$gatePath = Join-Path $repositoryRoot 'deploy/ci/scripts/tts-voice-acceptance-gate.mjs'

if (-not $ConfirmAllElevenHeard) {
    throw 'Thiếu -ConfirmAllElevenHeard. Manifest khẳng định Owner đã nghe đủ 11 giọng qua tuyến Asterisk/MicroSIP 8 kHz; script không tự khẳng định thay.'
}

if (-not $OutputPath) {
    $OutputPath = Join-Path $repositoryRoot 'docs/evidence/W-0122/voice-acceptance-manifest.json'
}
if ((Test-Path -LiteralPath $OutputPath) -and -not $Force) {
    throw "Đã có $OutputPath. Dùng -Force nếu thật sự muốn ghi đè một acceptance đã ký."
}

$config = Get-Content -Raw -LiteralPath $voiceConfigPath -Encoding utf8 | ConvertFrom-Json -Depth 20
$roster = @($config.voices)
if ($roster.Count -ne 11) {
    throw "Roster phải có đúng 11 giọng; voices.json đang có $($roster.Count). Audition cũ mất hiệu lực."
}

function Resolve-Candidate {
    param([string]$Value, [string]$Region)

    $match = @($roster | Where-Object { $_.voice_id -ceq $Value -or $_.preset -ceq $Value })
    if ($match.Count -ne 1) {
        $available = ($roster | Where-Object { $_.region -eq $Region } |
            ForEach-Object { "$($_.voice_id) ($($_.preset))" }) -join ', '
        throw "Không nhận ra giọng '$Value'. Miền ${Region} có: $available"
    }
    if ($match[0].region -ne $Region) {
        throw "'$Value' thuộc miền $($match[0].region), không phải ${Region}. Mỗi miền phải chọn giọng của chính miền đó."
    }
    return $match[0]
}

$selected = [ordered]@{
    North   = Resolve-Candidate -Value $North -Region 'North'
    Central = Resolve-Candidate -Value $Central -Region 'Central'
    South   = Resolve-Candidate -Value $South -Region 'South'
}
$selectedIds = @($selected.Values | ForEach-Object { $_.voice_id })
if (@($selectedIds | Sort-Object -Unique).Count -ne 3) {
    throw 'Ba miền phải là ba giọng khác nhau.'
}

$rejectedIds = @()
foreach ($value in $Rejected) {
    $match = @($roster | Where-Object { $_.voice_id -ceq $value -or $_.preset -ceq $value })
    if ($match.Count -ne 1) { throw "Không nhận ra giọng bị loại: '$value'" }
    if ($selectedIds -contains $match[0].voice_id) {
        throw "'$value' vừa được chọn vừa bị loại. Chọn một trong hai."
    }
    $rejectedIds += $match[0].voice_id
}

$results = foreach ($item in $roster) {
    $verdict = if ($selectedIds -contains $item.voice_id) { 'SELECTED' }
               elseif ($rejectedIds -contains $item.voice_id) { 'REJECTED' }
               else { 'NOT_SELECTED' }
    [ordered]@{
        voice_id = $item.voice_id
        region   = $item.region
        listened = $true
        verdict  = $verdict
        notes    = if ($Notes.ContainsKey($item.voice_id)) { [string]$Notes[$item.voice_id] }
                   elseif ($Notes.ContainsKey($item.preset)) { [string]$Notes[$item.preset] }
                   else { $null }
    }
}

$manifest = [ordered]@{
    schema_version           = 1
    work_id                  = 'W-0122'
    status                   = 'OWNER_ACCEPTED'
    stale_relisten_required  = $false
    source_commit            = $config.source_commit
    model_artifacts          = @(
        [ordered]@{ repo = 'pnnbao-ump/VieNeu-TTS-v3-Turbo'; revision = $config.model_revision }
        [ordered]@{ repo = 'OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX'; revision = $config.codec_revision }
    )
    voice_manifest_sha256    = $config.voice_manifest_sha256
    dependency_lock_sha256   = $config.dependency_lock_sha256
    runtime_lock_sha256      = $config.runtime_lock_sha256
    model_lock_sha256        = $config.model_lock_sha256
    audition_script_sha256   = $config.audition_script_sha256
    audition_manifest_sha256 = $config.audition_manifest_sha256
    audition_renderer_sha256 = $config.audition_renderer_sha256
    listening_profile_id     = $config.listening_profile_id
    listening_route          = 'ASTERISK_MICROSIP_8KHZ'
    listener                 = $Listener.Trim()
    listened_at              = [DateTimeOffset]::Now.ToString('yyyy-MM-ddTHH:mm:sszzz')
    device_and_lab_route     = $DeviceAndLabRoute.Trim()
    approval_reference       = $ApprovalReference.Trim()
    all_11_candidates_listened = $true
    selections               = [ordered]@{}
    candidate_results        = @($results)
    notes                    = $null
}
foreach ($region in 'North', 'Central', 'South') {
    $item = $selected[$region]
    $manifest.selections[$region] = [ordered]@{
        voice_id      = $item.voice_id
        preset        = $item.preset
        speaking_rate = $item.speaking_rate
        owner_notes   = if ($Notes.ContainsKey($item.voice_id)) { [string]$Notes[$item.voice_id] }
                        elseif ($Notes.ContainsKey($item.preset)) { [string]$Notes[$item.preset] }
                        else { $null }
    }
}

$json = ($manifest | ConvertTo-Json -Depth 8) -replace "`r`n", "`n"
$directory = Split-Path -Parent $OutputPath
if ($directory -and -not (Test-Path -LiteralPath $directory)) {
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
}
[System.IO.File]::WriteAllText($OutputPath, $json + "`n", [System.Text.UTF8Encoding]::new($false))

# Cùng validator mà CI và shim dùng. Nếu nó đỏ, manifest chưa phải bằng chứng gì cả.
& node $gatePath --acceptance $OutputPath
if ($LASTEXITCODE -ne 0) {
    Remove-Item -LiteralPath $OutputPath -Force
    throw 'Gate từ chối manifest vừa sinh; đã xoá file để không ai nhầm nó là acceptance.'
}

Write-Host ''
Write-Host "W0122_VOICE_ACCEPTANCE_WRITTEN $OutputPath" -ForegroundColor Green
Write-Host 'Bước tiếp theo: render 12 file catalog, rồi mới tới 6 cuộc gọi lab. Chưa mở gate nào khác.'
