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

.PARAMETER NorthVoiceId
    Voice ID thật của Thắm, copy trực tiếp từ ElevenLabs app.

.PARAMETER CentralVoiceId
    Voice ID thật của Zara, copy trực tiếp từ ElevenLabs app.

.PARAMETER SouthVoiceId
    Voice ID thật của Giang, copy trực tiếp từ ElevenLabs app.

.PARAMETER ElevenLabsAccountLabel
    Nhãn tài khoản không nhạy cảm dùng để truy nguồn lượt render, ví dụ
    `ssavigroup-owner`. Không dùng email, API key hoặc token.

.PARAMETER GeneratedAt
    Thời điểm sinh MP3 nguồn, có múi giờ. Phải truyền tường minh để evidence không ghi nhầm
    thời điểm chạy conversion thành thời điểm render ElevenLabs.

.EXAMPLE
    ./deploy/lab/Convert-LabVoiceAudio.ps1 `
        -NorthMp3 ./artifacts/w-0106-voice-audition/tham.mp3 `
        -CentralMp3 ./artifacts/w-0106-voice-audition/zara.mp3 `
        -SouthMp3 ./artifacts/w-0106-voice-audition/giang.mp3 `
        -NorthVoiceId '<THAM_VOICE_ID_FROM_APP>' `
        -CentralVoiceId '<ZARA_VOICE_ID_FROM_APP>' `
        -SouthVoiceId '<GIANG_VOICE_ID_FROM_APP>' `
        -ElevenLabsAccountLabel 'ssavigroup-owner' `
        -GeneratedAt '2026-08-24T15:30:00+07:00'
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$NorthMp3,
    [Parameter(Mandatory)][string]$CentralMp3,
    [Parameter(Mandatory)][string]$SouthMp3,

    [ValidatePattern('^[A-Za-z0-9_-]{8,128}$')]
    [string]$NorthVoiceId,

    [ValidatePattern('^[A-Za-z0-9_-]{8,128}$')]
    [string]$CentralVoiceId,

    [ValidatePattern('^[A-Za-z0-9_-]{8,128}$')]
    [string]$SouthVoiceId,

    [ValidatePattern('^[A-Za-z0-9][A-Za-z0-9._-]{0,79}$')]
    [string]$ElevenLabsAccountLabel,

    # Per-region ElevenLabs settings, exactly as used. ElevenLabs encodes them in the file it
    # hands you — `..._pvc_sp100_s75_sb75_v3.mp3` means speed 1.00, stability 0.75, similarity
    # 0.75 — so read them off the filename rather than from memory.
    [hashtable]$RenderSettings = @{},

    [DateTimeOffset]$GeneratedAt = [DateTimeOffset]::MinValue,

    [string]$FfmpegPath = 'ffmpeg',

    [switch]$SkipManifestUpdate
)

$ErrorActionPreference = 'Stop'
$audioDirectory = Join-Path $PSScriptRoot 'asterisk/audio'

if (-not $SkipManifestUpdate) {
    $requiredMetadata = @{
        NorthVoiceId             = $NorthVoiceId
        CentralVoiceId           = $CentralVoiceId
        SouthVoiceId             = $SouthVoiceId
        ElevenLabsAccountLabel   = $ElevenLabsAccountLabel
    }
    foreach ($entry in $requiredMetadata.GetEnumerator()) {
        if ([string]::IsNullOrWhiteSpace($entry.Value)) {
            throw "Thiếu -$($entry.Key). Manifest W-0106 không được ghi nếu thiếu voice ID thật hoặc nhãn tài khoản."
        }
    }
    if ($GeneratedAt -eq [DateTimeOffset]::MinValue) {
        throw 'Thiếu -GeneratedAt. Phải ghi thời điểm render ElevenLabs thật, không suy từ lúc conversion.'
    }

    # Fail closed on settings the same way the script already fails closed on voice IDs. Three
    # regions, three settings triples, no defaults: a default here is how "0.40" ended up in an
    # evidence file next to a render that used 0.75.
    foreach ($regionName in @('north', 'central', 'south')) {
        $settings = $RenderSettings[$regionName]
        if ($null -eq $settings) {
            throw "Thiếu -RenderSettings['$regionName']. Ví dụ: -RenderSettings @{ north = @{ stability = '0.75'; similarity = '0.75'; speed = '1.00' }; ... }"
        }
        foreach ($key in @('stability', 'similarity', 'speed')) {
            $value = [string]$settings[$key]
            if ($value -notmatch '^\d+(\.\d+)?$') {
                throw "-RenderSettings['$regionName']['$key'] phải là số thập phân đọc thẳng từ tên file ElevenLabs (ví dụ '0.75', '1.09'). Nhận được: '$value'."
            }
        }
    }
}

if (-not (Get-Command $FfmpegPath -ErrorAction SilentlyContinue)) {
    throw "Không tìm thấy ffmpeg ('$FfmpegPath'). Cài rồi chạy lại, hoặc truyền -FfmpegPath."
}

$plan = @(
    @{ Region = 'north';   Source = $NorthMp3;   Voice = 'Thắm'; VoiceId = $NorthVoiceId     }
    @{ Region = 'central'; Source = $CentralMp3; Voice = 'Zara'; VoiceId = $CentralVoiceId   }
    @{ Region = 'south';   Source = $SouthMp3;   Voice = 'Giang'; VoiceId = $SouthVoiceId    }
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

    # Decode the generated WAV to a null sink instead of calling `ffmpeg -i` without an output.
    # The latter prints valid stream metadata but deliberately exits 1, leaving callers with a
    # false failure even though conversion and manifest writing succeeded.
    $probe = & $FfmpegPath -hide_banner -loglevel info -i $targetPath -f null - 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0) {
        throw "ffmpeg không đọc lại được file đầu ra miền '$($item.Region)'."
    }
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
        VoiceId    = $item.VoiceId
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
    # Drop the whole previous W-0106 block, header included. Filtering only `^w0106_` left the
    # `work_id_regional=` header behind, so every re-run stacked another header and another blank
    # line onto an evidence file whose job is to be stable. Trailing blanks go too, otherwise the
    # gap grows by one line per run.
    $manifest = @(Get-Content -LiteralPath $manifestPath |
        Where-Object { $_ -notmatch '^w0106_' -and $_ -notmatch '^work_id_regional=' })
    while ($manifest.Count -gt 0 -and [string]::IsNullOrWhiteSpace($manifest[-1])) {
        $manifest = $manifest[0..($manifest.Count - 2)]
    }
    $manifest += ''
    $manifest += 'work_id_regional=W-0106'
    $manifest += 'w0106_generator=elevenlabs-web-app'
    $manifest += 'w0106_model=eleven_v3'
    $manifest += 'w0106_language=auto-detect'
    # Settings are recorded PER VOICE, from -RenderSettings, and never assumed. They used to be
    # four hardcoded lines claiming stability=0.40 / speed=-3% for every render. On 2026-08-26
    # the owner rendered with 0.75/0.50/0.50 and speed 1.00/1.00/1.09 and chose to keep that
    # spread, which made those four lines false while sitting next to the SHA-256 of the very
    # files they described. A manifest that demands the real voice ID but invents the settings
    # is only half an evidence file.
    $manifest += "w0106_generated_at=$($GeneratedAt.ToString('o', [System.Globalization.CultureInfo]::InvariantCulture))"
    $manifest += "w0106_elevenlabs_account_label=$ElevenLabsAccountLabel"
    $manifest += 'w0106_output_format=pcm_s16le-8000hz-mono'
    $manifest += 'w0106_script_version=v3-test-approved'
    $manifest += 'w0106_listening_acceptance=DEFERRED_OD_VOICE_05'
    foreach ($row in $results) {
        # Invariant decimal separator, spelled out. PowerShell 7 already expands numbers with
        # the invariant culture inside strings, so this is belt-and-braces rather than a fix —
        # but this line ends up pinned next to a SHA-256 in an evidence file, and a reader
        # should not have to know that quirk to trust that "16.770625" is not "16,770625" on
        # some other machine. This host reports en-US with a COMMA decimal separator, which is
        # exactly the kind of setup that turns an implicit assumption into corrupt evidence.
        $seconds = $row.Seconds.ToString([System.Globalization.CultureInfo]::InvariantCulture)
        $settings = $RenderSettings[$row.Region]
        $manifest += "w0106_$($row.Region)_voice_name=$($row.Voice)"
        $manifest += "w0106_$($row.Region)_voice_id=$($row.VoiceId)"
        $manifest += "w0106_$($row.Region)_stability=$($settings.stability)"
        $manifest += "w0106_$($row.Region)_similarity=$($settings.similarity)"
        $manifest += "w0106_$($row.Region)_speed=$($settings.speed)"
        $manifest += "w0106_$($row.Region)_source_sha256=$($row.SourceHash)"
        $manifest += "w0106_$($row.Region)_duration_seconds=$seconds"
        $manifest += "w0106_$($row.Region)_sha256=$($row.Sha256)"
    }

    $manifest += 'w0106_settings_shared_across_voices=NO'
    $manifest += 'w0106_production_provider_authorized=NO'
    $manifest += 'w0106_real_customer_data_used=NO'
    Set-Content -LiteralPath $manifestPath -Value $manifest -Encoding utf8
}

Write-Host ''
$results | Format-Table Region, Voice, Seconds, Sha256 -AutoSize
Write-Host ''
if ($SkipManifestUpdate) {
    Write-Host 'Đã bỏ qua cập nhật manifest theo -SkipManifestUpdate.' -ForegroundColor Yellow
}
else {
    Write-Host 'Manifest W-0106 đã ghi đủ voice ID và settings render THẬT của từng giọng.' -ForegroundColor Green
}
Write-Host 'Bước tiếp: điền duration thật trong compose, bật RegionalVoices, rồi dựng lại image Asterisk.' -ForegroundColor Yellow
