<#
.SYNOPSIS
    W-0106 Giai đoạn 4 — chuyển MP3 ba miền về PCM 8 kHz và ghim SHA-256.

.DESCRIPTION
    Nhận ba file MP3 sinh từ ElevenLabs (Thắm/Bắc, Zara/Trung, Giang/Nam), chuẩn hóa loudness,
    hạ về PCM signed 16-bit / 8 kHz / mono, rồi cập nhật SHA256SUMS và manifest.txt.

    Đặt tên `-region-north|central|south` chứ KHÔNG dùng `-n|-c|-s`: hậu tố `-c` đã thuộc về
    voice C của W-0104 (`ivr-lab-order-confirmation-c.wav`) và sẽ đè lên evidence cũ.

    ffmpeg chạy ở chế độ bitexact để không nhét metadata encoder vào WAV — nếu không, cùng một
    file nguồn sẽ ra hash khác nhau giữa hai phiên bản ffmpeg và việc ghim checksum thành vô nghĩa.

    Chuẩn hóa loudness TRƯỚC khi hạ 8 kHz, theo yêu cầu §3 của W-0104 voice proposal: làm ngược
    lại thì giọng dễ nhỏ hoặc vỡ tiếng trên PCMU.

.PARAMETER NorthMp3
    MP3 giọng Bắc (Thắm).

.PARAMETER CentralMp3
    MP3 giọng Trung (Zara).

.PARAMETER SouthMp3
    MP3 giọng Nam (Giang).

.EXAMPLE
    ./deploy/lab/Convert-LabVoiceAudio.ps1 `
        -NorthMp3 ./artifacts/w-0106-voice-audition/tham.mp3 `
        -CentralMp3 ./artifacts/w-0106-voice-audition/zara.mp3 `
        -SouthMp3 ./artifacts/w-0106-voice-audition/giang.mp3
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$NorthMp3,
    [Parameter(Mandatory)][string]$CentralMp3,
    [Parameter(Mandatory)][string]$SouthMp3,

    [string]$FfmpegPath = 'ffmpeg',

    [switch]$SkipManifestUpdate
)

$ErrorActionPreference = 'Stop'
$audioDirectory = Join-Path $PSScriptRoot 'asterisk/audio'

if (-not (Get-Command $FfmpegPath -ErrorAction SilentlyContinue)) {
    throw "Không tìm thấy ffmpeg ('$FfmpegPath'). Cài rồi chạy lại, hoặc truyền -FfmpegPath."
}

$plan = @(
    @{ Region = 'north';   Source = $NorthMp3;   Voice = 'Thắm'  }
    @{ Region = 'central'; Source = $CentralMp3; Voice = 'Zara'  }
    @{ Region = 'south';   Source = $SouthMp3;   Voice = 'Giang' }
)

$results = [System.Collections.Generic.List[object]]::new()

foreach ($item in $plan) {
    if (-not (Test-Path -LiteralPath $item.Source)) {
        throw "Không thấy file nguồn cho miền '$($item.Region)': $($item.Source)"
    }

    $sourceHash = (Get-FileHash -LiteralPath $item.Source -Algorithm SHA256).Hash.ToLowerInvariant()
    $targetName = "ivr-lab-order-confirmation-region-$($item.Region).wav"
    $targetPath = Join-Path $audioDirectory $targetName

    Write-Host "[$($item.Region)] $($item.Voice): chuyển sang PCM 16-bit/8 kHz/mono..." -ForegroundColor Cyan

    & $FfmpegPath -hide_banner -loglevel error -y `
        -fflags +bitexact `
        -i $item.Source `
        -af 'loudnorm=I=-16:TP=-1.5:LRA=11,aresample=8000' `
        -ar 8000 -ac 1 -c:a pcm_s16le `
        -flags +bitexact -fflags +bitexact -map_metadata -1 `
        $targetPath
    if ($LASTEXITCODE -ne 0) {
        throw "ffmpeg thất bại khi chuyển đổi miền '$($item.Region)'."
    }

    $probe = & $FfmpegPath -hide_banner -i $targetPath 2>&1 | Out-String
    if ($probe -notmatch '8000 Hz' -or $probe -notmatch 'mono' -or $probe -notmatch 'pcm_s16le') {
        throw "File đầu ra miền '$($item.Region)' không đúng PCM s16le/8000 Hz/mono."
    }

    $durationSeconds = 0.0
    if ($probe -match 'Duration:\s*(\d+):(\d+):(\d+(?:\.\d+)?)') {
        $durationSeconds = ([int]$Matches[1] * 3600) + ([int]$Matches[2] * 60) + [double]$Matches[3]
    }

    $results.Add([pscustomobject]@{
        Region     = $item.Region
        Voice      = $item.Voice
        File       = $targetName
        Seconds    = [math]::Round($durationSeconds, 3)
        Sha256     = (Get-FileHash -LiteralPath $targetPath -Algorithm SHA256).Hash.ToLowerInvariant()
        SourceHash = $sourceHash
    })
}

# SHA256SUMS giữ nguyên ba dòng W-0104 và thêm ba dòng vùng miền. Không xóa dòng cũ: chúng là
# evidence lịch sử và entrypoint vẫn kiểm toàn bộ file khi boot.
$sumsPath = Join-Path $audioDirectory 'SHA256SUMS'
$existing = @(Get-Content -LiteralPath $sumsPath | Where-Object { $_ -notmatch 'region-(north|central|south)\.wav$' })
$added = $results | ForEach-Object { "$($_.Sha256)  $($_.File)" }
Set-Content -LiteralPath $sumsPath -Value ($existing + $added) -Encoding ascii

if (-not $SkipManifestUpdate) {
    $manifestPath = Join-Path $audioDirectory 'manifest.txt'
    $manifest = @(Get-Content -LiteralPath $manifestPath | Where-Object { $_ -notmatch '^w0106_' })
    $manifest += ''
    $manifest += 'work_id_regional=W-0106'
    $manifest += 'w0106_generator=elevenlabs-web-app'
    $manifest += 'w0106_model=eleven_v3'
    $manifest += 'w0106_output_format=pcm_s16le-8000hz-mono'
    $manifest += 'w0106_script_version=v3-test-approved'
    $manifest += 'w0106_listening_acceptance=DEFERRED_OD_VOICE_05'
    foreach ($row in $results) {
        $manifest += "w0106_$($row.Region)_voice_name=$($row.Voice)"
        $manifest += "w0106_$($row.Region)_source_sha256=$($row.SourceHash)"
        $manifest += "w0106_$($row.Region)_duration_seconds=$($row.Seconds)"
        $manifest += "w0106_$($row.Region)_sha256=$($row.Sha256)"
    }

    $manifest += 'w0106_production_provider_authorized=NO'
    $manifest += 'w0106_real_customer_data_used=NO'
    Set-Content -LiteralPath $manifestPath -Value $manifest -Encoding utf8
}

Write-Host ''
$results | Format-Table Region, Voice, Seconds, Sha256 -AutoSize
Write-Host ''
Write-Host 'CHƯA điền voice ID vào manifest — phải copy ID thật từ ElevenLabs app.' -ForegroundColor Yellow
Write-Host 'Bước tiếp: dựng lại image Asterisk để nạp ba file mới, rồi gọi 6 lượt MicroSIP.' -ForegroundColor Yellow
