<#
.SYNOPSIS
    W-0106/W-0122 — chuyển 12 file MP3 hoặc WAV đoạn cố định về PCM 8 kHz và ghim SHA-256.

.DESCRIPTION
    Bổ sung cho `Convert-LabVoiceAudio.ps1`, KHÔNG thay thế. File kia dựng bản thu nguyên cuộc
    gọi của W-0106 (một file cho cả cuộc); file này dựng các ĐOẠN cố định để ghép động — cuộc
    gọi được lắp từ đoạn thu sẵn cộng phần giá trị đơn do TTS sinh.

    Danh sách đoạn KHÔNG viết tay ở đây. Nó đọc từ `deploy/lab/speech-segments.json`, vốn được
    sinh từ chính template đã duyệt bằng `deploy/ci/scripts/generate-speech-segments.mjs`. Sửa
    một chữ trong template ⇒ đổi `textSha256` ⇒ tên file đổi theo ⇒ bản thu cũ không còn được
    tra ra. Đó là điểm mấu chốt: bản thu và lời thoại không thể lệch nhau trong im lặng.

    Tên file: `ivr-seg-<miền>-<16 ký tự đầu của textSha256>.wav`. Đặt theo NỘI DUNG chứ không
    theo thứ tự, để một lần đổi thứ tự câu trong template không làm mọi tra cứu vẫn "thành công"
    mà phát sai thứ tự.

    ffmpeg chạy bitexact + `-map_metadata -1` vì cùng lý do như W-0106 Giai đoạn 4: không có nó,
    metadata encoder lọt vào WAV và cùng một MP3 nguồn ra hash khác nhau giữa hai bản ffmpeg.

.PARAMETER SourceDirectory
    Thư mục chứa file nguồn, đặt tên `<miền>-s<ordinal><SourceExtension>` — ví dụ
    `north-s1.mp3` hoặc `north-s1.wav`. `ordinal` lấy đúng từ `speech-segments.json`.

.PARAMETER SourceExtension
    Định dạng nguồn tường minh: `.mp3` (mặc định, giữ đường W-0119) hoặc `.wav` (VieNeu/W-0122).
    Script không tự đoán và không dò fallback sang extension khác.

.PARAMETER OutputDirectory
    Thư mục nhận PCM/manifests. Bỏ trống để dùng `deploy/lab/asterisk/audio` như trước. Tham số
    này cho phép regression chạy trong sandbox mà không ghi đè evidence audio của repo.

.PARAMETER Region
    Chỉ xử lý một miền. Bỏ trống để xử lý cả ba.

.EXAMPLE
    ./deploy/lab/Convert-LabSegmentAudio.ps1 -SourceDirectory ./artifacts/w-0106-segments

.EXAMPLE
    ./deploy/lab/Convert-LabSegmentAudio.ps1 -SourceDirectory ./artifacts/w-0122-fixed `
        -SourceExtension .wav

.EXAMPLE
    # In ra đúng những câu cần thu, trước khi mở ElevenLabs.
    ./deploy/lab/Convert-LabSegmentAudio.ps1 -ListOnly
#>
[CmdletBinding()]
param(
    [string]$SourceDirectory,

    [ValidateSet('.mp3', '.wav')]
    [string]$SourceExtension = '.mp3',

    [string]$OutputDirectory,

    [ValidateSet('north', 'central', 'south')]
    [string[]]$Region,

    [string]$FfmpegPath = 'ffmpeg',

    [switch]$ListOnly,

    [switch]$SkipManifestUpdate
)

$ErrorActionPreference = 'Stop'

# Asterisk chạy `sha256sum --check --strict` trên chính những file này bên trong container
# Linux, nơi một ký tự CR cuối dòng trở thành một phần của tên file. `Set-Content` nối dòng
# bằng [Environment]::NewLine — tức CRLF trên Windows — nên mỗi lần chạy script này trên
# Windows lại sinh ra một SHA256SUMS mà image Asterisk từ chối boot, trong khi bản đã commit
# vẫn LF nhờ .gitattributes và `git status` vẫn sạch. Ghi LF tường minh, không phụ thuộc máy.
function Write-LfFile {
    param([Parameter(Mandatory)][string]$Path, [string[]]$Lines)

    $text = ($Lines -join "`n") + "`n"
    [System.IO.File]::WriteAllText($Path, $text, [System.Text.UTF8Encoding]::new($false))

    # Kiểm ngay trên máy vừa ghi. Git normalise lúc commit nên CI không bao giờ nhìn thấy bản
    # CRLF; chỗ duy nhất bắt được là ở đây, hoặc lúc Asterisk từ chối boot.
    if ([System.IO.File]::ReadAllBytes($Path) -contains 13) {
        throw "Ghi ra ky tu CR trong $Path. Asterisk kiem file nay bang sha256sum trong container Linux, o do CR thanh mot phan cua ten file."
    }
}

$audioDirectory = if ($OutputDirectory) {
    [System.IO.Path]::GetFullPath($OutputDirectory)
} else {
    Join-Path $PSScriptRoot 'asterisk/audio'
}
$manifestPath = Join-Path $PSScriptRoot 'speech-segments.json'

if (-not (Test-Path -LiteralPath $manifestPath)) {
    throw "Không thấy $manifestPath. Chạy: node deploy/ci/scripts/generate-speech-segments.mjs"
}

$plan = Get-Content -LiteralPath $manifestPath -Raw -Encoding utf8 | ConvertFrom-Json
$fixedSegments = @($plan.segments | Where-Object { $_.kind -eq 'Fixed' })
$regions = if ($Region) { $Region } else { $plan.regions }

if ($fixedSegments.Count -eq 0) {
    throw 'speech-segments.json không có đoạn cố định nào — template hỏng?'
}

# Bảng "cần thu những câu nào". In trước khi làm bất cứ việc gì, kể cả khi không -ListOnly:
# người thu giọng cần đúng bảng này, và nó phải là thứ họ thấy đầu tiên chứ không phải thứ họ
# phải đi tìm trong tài liệu.
Write-Host ''
Write-Host "Đoạn cố định cần thu — $($fixedSegments.Count) câu × $($regions.Count) miền = $($fixedSegments.Count * $regions.Count) file" -ForegroundColor Cyan
Write-Host ''
foreach ($segment in $fixedSegments) {
    $shortHash = $segment.textSha256.Substring(0, 16)
    Write-Host "  s$($segment.ordinal)  [$shortHash]" -ForegroundColor Yellow
    Write-Host "      $($segment.text.Trim())"
}
Write-Host ''
Write-Host 'Đọc liền mạch, KHÔNG thêm/bớt chữ. Dấu phẩy đầu câu là chỗ nối, đọc như nối câu.' -ForegroundColor DarkGray
Write-Host ''

if ($ListOnly) {
    return
}

if (-not $SourceDirectory) {
    throw 'Thiếu -SourceDirectory. Dùng -ListOnly nếu chỉ muốn xem danh sách câu cần thu.'
}

if (-not (Test-Path -LiteralPath $SourceDirectory -PathType Container)) {
    throw "Thư mục nguồn không tồn tại: $SourceDirectory"
}

if (-not (Test-Path -LiteralPath $audioDirectory -PathType Container)) {
    throw "Thư mục đầu ra không tồn tại: $audioDirectory"
}

if (-not (Get-Command $FfmpegPath -ErrorAction SilentlyContinue)) {
    throw "Không tìm thấy ffmpeg ('$FfmpegPath'). Cài rồi chạy lại, hoặc truyền -FfmpegPath."
}

$results = [System.Collections.Generic.List[object]]::new()

foreach ($regionName in $regions) {
    foreach ($segment in $fixedSegments) {
        $sourcePath = Join-Path $SourceDirectory "$regionName-s$($segment.ordinal)$SourceExtension"
        if (-not (Test-Path -LiteralPath $sourcePath)) {
            throw "Thiếu file nguồn: $sourcePath"
        }

        $shortHash = $segment.textSha256.Substring(0, 16)
        $targetName = "ivr-seg-$regionName-$shortHash.wav"
        $targetPath = Join-Path $audioDirectory $targetName

        Write-Host "[$regionName s$($segment.ordinal)] chuyển sang PCM 16-bit/8 kHz/mono..." -ForegroundColor Cyan

        & $FfmpegPath -hide_banner -loglevel error -y `
            -fflags +bitexact `
            -i $sourcePath `
            -af 'loudnorm=I=-16:TP=-1.5:LRA=11,aresample=8000' `
            -ar 8000 -ac 1 -c:a pcm_s16le `
            -flags +bitexact -fflags +bitexact -map_metadata -1 `
            $targetPath
        if ($LASTEXITCODE -ne 0) {
            throw "ffmpeg thất bại: $regionName s$($segment.ordinal)."
        }

        # Decode the generated WAV to a null sink instead of calling `ffmpeg -i` without an output.
        # The latter prints valid stream metadata but deliberately exits 1, so a fully successful
        # run left $LASTEXITCODE=1 and any `&&`/CI caller read it as a failure. Same fix, same
        # reason as Convert-LabVoiceAudio.ps1 — the two probes must not drift apart again.
        $probe = & $FfmpegPath -hide_banner -loglevel info -i $targetPath -f null - 2>&1 | Out-String
        if ($LASTEXITCODE -ne 0) {
            throw "ffmpeg không đọc lại được file đầu ra: $targetName"
        }
        if ($probe -notmatch '8000 Hz' -or $probe -notmatch 'mono' -or $probe -notmatch 'pcm_s16le') {
            throw "File ra không đúng PCM s16le/8000 Hz/mono: $targetName"
        }

        $durationSeconds = 0.0
        if ($probe -match 'Duration:\s*(\d+):(\d+):(\d+(?:\.\d+)?)') {
            $durationSeconds = ([int]$Matches[1] * 3600) + ([int]$Matches[2] * 60) + [double]$Matches[3]
        }

        if ($durationSeconds -le 0) {
            throw "Không đọc được độ dài của $targetName — cấu hình cần DurationMilliseconds thật."
        }

        $results.Add([pscustomobject]@{
            Region          = $regionName
            Ordinal         = $segment.ordinal
            TextHash        = $segment.textSha256
            File            = $targetName
            MediaReference  = "sound:ivr-seg-$regionName-$shortHash"
            Milliseconds    = [int][math]::Round($durationSeconds * 1000)
            Sha256          = (Get-FileHash -LiteralPath $targetPath -Algorithm SHA256).Hash.ToLowerInvariant()
            SourceHash      = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash.ToLowerInvariant()
            SourceExtension = $SourceExtension
        })
    }
}

# Giữ nguyên mọi dòng cũ của W-0104/W-0106; chỉ thay các dòng đoạn của chính lần chạy này.
# entrypoint kiểm toàn bộ file lúc boot, nên xóa dòng cũ là làm hỏng evidence trước đó.
$sumsPath = Join-Path $audioDirectory 'SHA256SUMS'
$producedFiles = $results | ForEach-Object { $_.File }
$existingLines = if (Test-Path -LiteralPath $sumsPath) {
    @(Get-Content -LiteralPath $sumsPath)
} else {
    @()
}
$existing = @($existingLines | Where-Object {
    $line = $_
    -not ($producedFiles | Where-Object { $line -match [regex]::Escape($_) + '$' })
})
$added = $results | ForEach-Object { "$($_.Sha256)  $($_.File)" }
Write-LfFile -Path $sumsPath -Lines ($existing + $added)

if (-not $SkipManifestUpdate) {
    $segmentManifestPath = Join-Path $audioDirectory 'segments-manifest.txt'
    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add($(if ($SourceExtension -eq '.wav') { 'work_id=W-0122' } else { 'work_id=W-0106-A1' }))
    $lines.Add("template_id=$($plan.templateId)")
    $lines.Add("template_version=$($plan.templateVersion)")
    $lines.Add("template_sha256=$($plan.templateSha256)")
    $lines.Add('output_format=pcm_s16le-8000hz-mono')
    $lines.Add("source_extension=$SourceExtension")
    $lines.Add('production_provider_authorized=NO')
    $lines.Add('real_customer_data_used=NO')
    foreach ($row in $results) {
        $key = "seg_$($row.Region)_s$($row.Ordinal)"
        $lines.Add("${key}_text_sha256=$($row.TextHash)")
        $lines.Add("${key}_media_reference=$($row.MediaReference)")
        # Số nguyên mili giây: không có dấu thập phân thì không có chuyện culture đổi dấu phẩy
        # thành dấu chấm trong một file evidence.
        $lines.Add("${key}_duration_ms=$($row.Milliseconds)")
        $lines.Add("${key}_source_sha256=$($row.SourceHash)")
        $lines.Add("${key}_sha256=$($row.Sha256)")
    }

    Write-LfFile -Path $segmentManifestPath -Lines $lines
}

# Khối cấu hình sinh sẵn. Người vận hành dán thẳng, không chép tay 12 mã băm 64 ký tự — chép tay
# một ký tự sai ở đây là một câu không tra ra được, và nó chỉ lộ ra lúc đang gọi khách.
$configuration = [ordered]@{}
foreach ($regionName in $regions) {
    $entries = @($results | Where-Object { $_.Region -eq $regionName } | ForEach-Object {
        [ordered]@{
            TextHash             = $_.TextHash
            MediaReference       = $_.MediaReference
            DurationMilliseconds = $_.Milliseconds
        }
    })
    $configuration[(Get-Culture).TextInfo.ToTitleCase($regionName)] = [ordered]@{ FixedSegments = $entries }
}

$configurationPath = Join-Path $audioDirectory 'segments-appsettings.json'
[ordered]@{
    Ivr = [ordered]@{
        Speech = [ordered]@{
            Tts = [ordered]@{
                Segmentation   = [ordered]@{ Enabled = $true; FixedSegments = 'Catalog' }
                RegionalVoices = $configuration
            }
        }
    }
} | ConvertTo-Json -Depth 8 | ForEach-Object { Write-LfFile -Path $configurationPath -Lines ($_ -split "`r?`n") }

# Cùng nội dung, dạng biến môi trường double-underscore. Lý do phải có bản thứ hai: lab cấu hình
# service HOÀN TOÀN bằng `environment:` trong `docker-compose.softphone.yml` — không có chỗ nào
# mount appsettings.json. Nên khối JSON ở trên, dù đúng shape, KHÔNG dán được vào lab; ai đó sẽ
# phải dịch tay 12 mục × 3 trường, tức là chép tay đúng 12 mã băm 64 ký tự mà cả file này lẫn
# compose đều ghi rõ là không được chép tay.
$envPath = Join-Path $audioDirectory 'segments-compose-env.yml'
$envLines = [System.Collections.Generic.List[string]]::new()
$envLines.Add('# SINH TỰ ĐỘNG bởi deploy/lab/Convert-LabSegmentAudio.ps1 — không sửa tay.')
$envLines.Add('#')
$envLines.Add('# Dán vào anchor `x-asterisk-lab-env` của docker-compose.softphone.yml, thay hai dòng')
$envLines.Add('# Segmentation__* đang là "false" ở đó.')
$envLines.Add('#')
$envLines.Add('# Trước khi bật, CẢ HAI nửa phải có thật:')
$envLines.Add('#   - nửa thu sẵn: 12 file PCM đã nằm trong image Asterisk (script này vừa ghi + SHA256SUMS);')
$envLines.Add('#   - nửa tổng hợp: endpoint TTS thật ở Ivr__Speech__Tts__External__* (OD-VOICE-01).')
$envLines.Add('# Bật khi catalog còn thiếu một câu ⇒ service TỪ CHỐI khởi động. Đó là hành vi đúng:')
$envLines.Add('# một câu thiếu phải chặn deploy, không phải làm ngắn cuộc gọi.')
if ($regions.Count -lt 3) {
    $envLines.Add('#')
    $envLines.Add("# ⚠️ CHẠY MỘT PHẦN: chỉ có miền $($regions -join ', '). Thiếu miền nào thì validator chặn")
    $envLines.Add('# khởi động. Chạy lại không kèm -Region để sinh đủ ba miền trước khi dán.')
}
$envLines.Add('')
$envLines.Add('  Ivr__Speech__Tts__RegionalVoices__Enabled: "true"')
$envLines.Add('  Ivr__Speech__Tts__Segmentation__Enabled: "true"')
$envLines.Add('  Ivr__Speech__Tts__Segmentation__FixedSegments: "Catalog"')
foreach ($regionName in $regions) {
    $regionKey = (Get-Culture).TextInfo.ToTitleCase($regionName)
    $prefix = "  Ivr__Speech__Tts__RegionalVoices__${regionKey}__FixedSegments"
    $index = 0
    foreach ($row in @($results | Where-Object { $_.Region -eq $regionName })) {
        $envLines.Add("${prefix}__${index}__TextHash: `"$($row.TextHash)`"")
        $envLines.Add("${prefix}__${index}__MediaReference: `"$($row.MediaReference)`"")
        $envLines.Add("${prefix}__${index}__DurationMilliseconds: `"$($row.Milliseconds)`"")
        $index++
    }
}
Write-LfFile -Path $envPath -Lines $envLines

Write-Host ''
$results | Format-Table Region, Ordinal, Milliseconds, MediaReference -AutoSize
Write-Host ''
Write-Host "Cấu hình đã sinh (JSON, cho deployment có appsettings): $configurationPath" -ForegroundColor Green
Write-Host "Cấu hình đã sinh (env, DÁN VÀO COMPOSE LAB):            $envPath" -ForegroundColor Green
Write-Host 'Bước tiếp: dựng lại image Asterisk, bật Segmentation.Enabled=true, gọi thử MicroSIP.' -ForegroundColor Yellow
