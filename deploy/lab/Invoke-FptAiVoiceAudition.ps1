<#
.SYNOPSIS
    W-0106 Giai đoạn 1 — sinh mẫu giọng FPT.AI cho ba miền để owner nghe và chọn.

.DESCRIPTION
    Gọi FPT.AI TTS v5 cho từng giọng nữ vùng miền, tải MP3 về và in SHA-256.

    Kịch bản dùng đúng template v3-test-approved và đúng lexicon số của từng miền do
    VietnameseNumberSpeller sinh ra ("nghìn" cho Bắc, "ngàn" cho Trung/Nam). Nghe mẫu sinh
    bằng lexicon sai miền là nghe sai thứ sắp chạy thật.

    DỮ LIỆU FAKE. Không số điện thoại, không địa chỉ đầy đủ, không đơn hàng thật.
    REAL_CUSTOMER_CALL_ALLOWED=NO không thay đổi.

.PARAMETER ApiKey
    API key từ console.fpt.ai. KHÔNG commit key vào git — truyền qua tham số hoặc biến môi
    trường FPT_AI_API_KEY.

.PARAMETER Region
    Miền cần sinh mẫu. Mặc định Central: đây là điểm nghẽn của cả work item vì FPT.AI chỉ có
    ĐÚNG MỘT giọng nữ miền Trung (myan). Nếu giọng đó trượt thì lựa chọn vendor phải tính lại,
    nên phải biết sớm chứ không để đến cuối.

.EXAMPLE
    $env:FPT_AI_API_KEY = '<key>'
    ./deploy/lab/Invoke-FptAiVoiceAudition.ps1 -Region Central

.EXAMPLE
    ./deploy/lab/Invoke-FptAiVoiceAudition.ps1 -Region All -OutputDirectory ./artifacts/w-0106
#>
[CmdletBinding()]
param(
    [string]$ApiKey = $env:FPT_AI_API_KEY,

    [ValidateSet('North', 'Central', 'South', 'All')]
    [string]$Region = 'Central',

    [string]$OutputDirectory = './artifacts/w-0106-voice-audition',

    [ValidateRange(-3, 3)]
    [int]$Speed = 0,

    [ValidateRange(10, 300)]
    [int]$TimeoutSeconds = 120
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    throw 'Thiếu API key. Đặt $env:FPT_AI_API_KEY hoặc truyền -ApiKey. Không commit key vào git.'
}

# Template v3-test-approved. Phần cố định giống hệt nhau ở cả ba miền — chỉ ô biến đổi.
$scriptTemplate = 'Xin chào Quý khách. Đây là cuộc gọi tự động để xác nhận đơn hàng từ ' +
    'Ginsengfood. Quý khách có đơn hàng gồm hai hộp Cháo sâm diêm mạch - hạt sen, tổng tiền ' +
    '{0}, giao đến {1}. Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng.'

# Giọng nữ FPT.AI theo miền. Miền Trung chỉ có myan — không có phương án thay thế cùng vendor.
$regionPlan = [ordered]@{
    North   = @{
        Voices = @('banmai', 'thuminh')
        Amount = 'năm trăm sáu mươi nghìn đồng'
        Area   = 'phường Cửa Nam, thành phố Hà Nội'
    }
    Central = @{
        Voices = @('myan')
        Amount = 'năm trăm sáu mươi ngàn đồng'
        Area   = 'phường Hải Châu, thành phố Đà Nẵng'
    }
    South   = @{
        Voices = @('lannhi', 'linhsan')
        Amount = 'năm trăm sáu mươi ngàn đồng'
        Area   = 'phường Phú Khương, tỉnh Vĩnh Long'
    }
}

$selectedRegions = if ($Region -eq 'All') { @($regionPlan.Keys) } else { @($Region) }
$null = New-Item -ItemType Directory -Force -Path $OutputDirectory
$results = [System.Collections.Generic.List[object]]::new()

foreach ($regionName in $selectedRegions) {
    $plan = $regionPlan[$regionName]
    $text = [string]::Format($scriptTemplate, $plan.Amount, $plan.Area)

    foreach ($voice in $plan.Voices) {
        Write-Host "[$regionName] Đang sinh mẫu giọng '$voice'..." -ForegroundColor Cyan

        # FPT.AI TTS v5 trả link async ngay, audio xuất hiện sau vài giây.
        $response = Invoke-RestMethod -Method Post -Uri 'https://api.fpt.ai/hmi/tts/v5' -Headers @{
            'api_key'       = $ApiKey
            'voice'         = $voice
            'speed'         = "$Speed"
            'format'        = 'mp3'
            'Cache-Control' = 'no-cache'
        } -ContentType 'text/plain; charset=utf-8' -Body ([System.Text.Encoding]::UTF8.GetBytes($text))

        if ($response.error -ne 0 -or [string]::IsNullOrWhiteSpace($response.async)) {
            throw "FPT.AI từ chối yêu cầu cho giọng '$voice': $($response.message)"
        }

        $outputFile = Join-Path $OutputDirectory "w0106-$($regionName.ToLowerInvariant())-$voice.mp3"
        $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
        $downloaded = $false

        while ((Get-Date) -lt $deadline) {
            try {
                Invoke-WebRequest -Uri $response.async -OutFile $outputFile -ErrorAction Stop
                # Link tồn tại trước khi audio sẵn sàng, nên file rỗng không phải là thành công.
                if ((Get-Item $outputFile).Length -gt 0) { $downloaded = $true; break }
            }
            catch {
                Start-Sleep -Seconds 3
            }
        }

        if (-not $downloaded) {
            throw "Hết $TimeoutSeconds giây mà audio giọng '$voice' vẫn chưa sẵn sàng."
        }

        $hash = (Get-FileHash -Path $outputFile -Algorithm SHA256).Hash.ToLowerInvariant()
        $results.Add([pscustomobject]@{
            Region    = $regionName
            Voice     = $voice
            File      = $outputFile
            Bytes     = (Get-Item $outputFile).Length
            Sha256    = $hash
            RequestId = $response.request_id
        })
    }
}

Write-Host ''
$results | Format-Table Region, Voice, Bytes, Sha256 -AutoSize
Write-Host ''
Write-Host 'Dữ liệu FAKE, chỉ để owner chọn giọng. Chưa phải evidence LAB, chưa phải production.' -ForegroundColor Yellow
Write-Host 'Bước tiếp theo: nghe MIỀN TRUNG (myan) trước — đó là điểm nghẽn của W-0106.' -ForegroundColor Yellow
