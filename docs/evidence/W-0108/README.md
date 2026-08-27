# W-0108 — Chuỗi ghép audio động (fixed + biến thiên)

Ngày: `2026-08-22`
Baseline: `main@573dc8a`
Trạng thái: `TESTS_PASS` — cả 4 suite đã chạy: **704 / 705** pass; lỗi duy nhất thuộc luồng khác (§7).
Plan: [`remaining-work-plan-2026-08-22.md` §A1](../../../plan/ivr-orther/remaining-work-plan-2026-08-22.md)
Nối tiếp: `W-0106` §4.6 (kiến trúc lai), `OD-VOICE-01`, `OD-V1-19`

> Trạng thái này **không phải** owner UAT và **không phải** `ACCEPTED`.
> `REAL_CUSTOMER_CALL_ALLOWED=NO` không thay đổi. `Segmentation.Enabled` mặc định `false`.

---

## 1. Vấn đề đã đóng

Trước W-0108, một cuộc gọi phát **đúng một file audio**. Ở LAB file đó là bản thu chung, nên
một kết quả "gọi được, khách bấm 1, ghi đúng disposition" chứng minh chặng quay số và **không**
chứng minh khách nghe đúng đơn của mình.

`VietnameseOrderScriptRenderer` đã sinh đúng câu cho từng đơn từ W-0106. Cái thiếu là thứ biến
câu đó thành âm thanh. W-0108 lắp phần đó.

---

## 2. Phạm vi đã triển khai

### 2.1 Domain — mô hình đoạn

| Thành phần | Việc |
| --- | --- |
| [`SpeechSegment`](../../../src/Ivr.Domain/Speech/SpeechSegment.cs) | Một mảnh của kịch bản: `Fixed` (văn xuôi) hoặc `Dynamic` (giá trị đơn), kèm `TextHash` = SHA-256 của nội dung đã chuẩn hoá NFC |
| `SpeechSegmentValidation` | Từ chối danh sách rỗng, lệch thứ tự, hoặc **không ghép lại đúng** `ExactText` |
| [`TargetV1SpeechPolicy.SegmentTemplate`](../../../src/Ivr.Domain/Scripts/TargetV1SpeechPolicy.cs) | Tách template đã duyệt tại biên `{{placeholder}}` |
| `TargetV1SpeechPolicy.FixedSegmentHashes` | Danh sách chính xác các câu cần thu |
| [`SpeechScript.Segments`](../../../src/Ivr.Domain/Speech/ITtsProvider.cs) | Thứ tự phát; không truyền ⇒ một đoạn phủ toàn văn (hành vi cũ) |
| [`RenderedAudio.Segments` / `PlaylistHash`](../../../src/Ivr.Domain/Speech/RenderedAudio.cs) | Audio là danh sách có thứ tự; `PlaylistHash` = SHA-256 trên chuỗi content-ref |

**Quyết định thiết kế đáng ghi.** `exactText` giờ được **dựng từ** các đoạn chứ không dựng song
song với chúng. Trước đây renderer nối chuỗi bằng một dãy `Replace`; nếu sau này ai đó tách đoạn
ở chỗ khác, văn bản mà PII guard và hạn mức ký tự soi sẽ khác với các mảnh mà khách nghe. Dựng
từ một nguồn làm khả năng đó biến mất theo cấu trúc, không phải theo kỷ luật.

### 2.2 Bản đồ tách đoạn — template v3

Sinh bởi [`generate-speech-segments.mjs`](../../../deploy/ci/scripts/generate-speech-segments.mjs)
→ [`deploy/lab/speech-segments.json`](../../../deploy/lab/speech-segments.json).

| # | Loại | Nội dung |
| --- | --- | --- |
| 1 | Fixed | `Xin chào Quý khách. Đây là cuộc gọi tự động để xác nhận đơn hàng từ Ginsengfood. Quý khách có đơn hàng gồm ` |
| 2 | Dynamic | `items_spoken` |
| 3 | Fixed | `, tổng tiền ` |
| 4 | Dynamic | `total_amount_display` |
| 5 | Fixed | `, giao đến ` |
| 6 | Dynamic | `delivery_area_short` |
| 7 | Fixed | `. Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng.` |

**4 đoạn cố định × 3 miền = 12 file**, đúng như W-0106 §4.6 dự toán.
**203 / 266 ký tự là cố định** — con số này đo được, không trích lại: `UT-SEG-MANIFEST-12`
assert `fixedCharacters == 203`.

### 2.3 Infrastructure — điều phối lai

| Thành phần | Việc |
| --- | --- |
| [`SpeechSynthesisService.SynthesizeSegmentedAsync`](../../../src/Ivr.Infrastructure/Speech/SpeechSynthesisService.cs) | Duyệt từng đoạn: `Fixed` → tra catalog; `Dynamic` → cache theo nội dung → provider |
| [`AudioCacheKey.CreateForSegment`](../../../src/Ivr.Infrastructure/Speech/AudioCache.cs) | Khoá theo **nội dung đoạn**, không theo `summaryHash` của cả cuộc |
| [`RegionalVoiceMap.FixedSegmentCatalog`](../../../src/Ivr.Infrastructure/Speech/RegionalVoiceMap.cs) | Catalog theo từng giọng; giọng lạ ⇒ catalog rỗng, **không** rơi sang giọng khác |
| `TtsUsageMeter.RecordSegment` + `ivr_tts_segments_total{kind,source}` | Đếm 3 nhánh: catalog / cache / provider |

**Vì sao khoá theo nội dung, không theo cuộc gọi.** Hai đơn khác hẳn nhau nhưng giao cùng một
phường dùng chung đúng đoạn `delivery_area_short`. Khoá theo `summaryHash` coi chúng là không
liên quan và trả tiền hai lần cho cùng một câu. `UT-SEG-CACHESHARE-06` đo đúng chênh lệch đó:
đơn thứ hai chỉ tốn **2** lần gọi provider thay vì 7.

### 2.4 Provider ngoài — HTTP thật, trung lập nhà cung cấp

[`ConfigurableExternalTtsProvider`](../../../src/Ivr.Infrastructure/Speech/ConfigurableExternalTtsProvider.cs)
trước đây là một seam luôn ném `TTS_NOT_CONFIGURED`. Giờ nó nói HTTP.

- **Không tên nhà cung cấp nào trong code.** Endpoint, header credential, scheme và **thân
  request JSON** đều là cấu hình. `OD-VOICE-01` đã đảo hướng ba lần; mỗi lần đảo sẽ là một lần
  sửa code nếu viết theo SDK của một hãng.
- **PCM thô, không MP3.** Asterisk phát họ `.sln` không cần codec module; giải mã MP3 trong
  tiến trình sẽ nhét một thư viện audio vào API. Hãng nào không xuất được PCM ở tần số đã cấu
  hình thì đặt sidecar chuyển đổi phía trước — đó là câu trả lời của deployment, và nó giữ giả
  định định dạng ở **một** chỗ nhìn thấy được.
- **Độ dài tính từ số byte**, không đoán: `bytes / 2 / sampleRate`. Số byte lẻ ⇒ không phải PCM
  16-bit mono ⇒ `TTS_AUDIO_NOT_PCM`.
- **Tên file theo nội dung** + ghi qua file tạm rồi `move`: crash giữa chừng không để lại file
  cụt mang cái tên mà cache sẽ vui vẻ đưa cho dialplan mãi mãi.
- **Lỗi vendor chỉ báo status code.** Thân lỗi có thể trích lại chính câu vừa gửi đi, tức là nội
  dung đơn. `UT-TTS-EXT-HTTPERROR-04` assert thông điệp **không** chứa dữ liệu đó.

### 2.5 Phát danh sách

[`AsteriskAriSimGateway.BuildMediaList`](../../../src/Ivr.Infrastructure/Telephony/AsteriskAriSimGateway.cs)
ghép các đoạn thành **một** tham số `media` phân tách bằng dấu phẩy — ARI phát cả danh sách như
một thao tác.

Gửi từng đoạn một request sẽ để một cú cúp máy giữa hai đoạn tạo ra cuộc gọi mà khách đã nghe
**nửa đơn hàng** — và từ phía dialplan, nửa cuộc đó không phân biệt được với một cuộc trọn vẹn.

Mọi đoạn đều được kiểm, không chỉ đoạn đầu. Dấu phẩy nằm trong một content-ref cũng bị từ chối:
nó sẽ tách thành hai mục và đẩy lệch mọi câu phía sau.

### 2.6 A7 — số thập phân đọc bằng chữ

`VietnameseNumberSpeller.SpellQuantity`: `2,5` → `"hai phẩy năm"`. Phần thập phân đọc **từng
chữ số**: `0,25` → `"không phẩy hai năm"`, không phải `"không phẩy hai mươi lăm"` — gộp lại mời
người nghe hiểu ra một con số khác, mà đây là con số khách sắp bấm phím để duyệt.

Số tiền vẫn bắt buộc nguyên: VND không có đơn vị phụ khi đọc.

---

## 3. Đối chiếu tiêu chí nghiệm thu

| Tiêu chí (plan §A1) | Test | Kết quả |
| --- | --- | --- |
| Hai đơn khác nhau ⇒ hai chuỗi audio khác nhau, kiểm bằng hash danh sách media | `UT-SEG-PLAYLIST-04` | ✅ so bằng `PlaylistHash`; đồng thời chứng minh đoạn chào **giống nhau**, nên so `ContentRef` đầu là không đủ |
| Thiếu một đoạn ⇒ ném lỗi có mã, không phát cuộc gọi thiếu nội dung | `UT-SEG-MISSING-07` | ✅ `TTS_FIXED_SEGMENT_NOT_RECORDED` |
| Cache ấm: đơn thứ hai cùng nội dung ⇒ 0 lần gọi vendor | `UT-SEG-CACHE-05` | ✅ `provider.Calls` giữ nguyên 7 |
| Đoạn cố định thiếu file ⇒ **fail-start**, không fail lúc đang gọi | `UT-SEG-FAILSTART-07` | ✅ validator từ chối lúc khởi động |
| Gọi thử MicroSIP 3 miền, nghe đúng tên/món/số tiền | — | ⛔ **chưa** — chờ 12 MP3 (§6) |

Bốn tiêu chí tự động đã đạt. Tiêu chí thứ năm cần audio thật và chỉ owner làm được.

---

## 4. Kết quả kiểm chứng

| Suite | Kết quả |
| --- | --- |
| `Ivr.UnitTests` | **449 / 449** pass (từ 420 trước W-0108; **+29** test mới) |
| `Ivr.IntegrationTests` | **228 / 228** pass |
| `Ivr.ChaosTests` | **6 / 6** pass |
| `Ivr.ContractTests` | **21 / 22** — lỗi duy nhất **không thuộc W-0108**, xem §7.1 |
| **Tổng** | **704 / 705** |
| `dotnet build Ivr.sln` | 0 warning / 0 error |
| `dotnet format Ivr.sln --verify-no-changes` | PASS |
| Traceability | **421** tagged test; `UT-TRACE-01` xanh |
| `docker compose config` (dev + softphone) | PASS |
| Quét PII trên file W-0108 | sạch (xem §7 về 2 file của luồng khác) |

### Mutation test — guard có răng

Theo tiền lệ `A-0318`/`A-0319`:

| Đột biến | Kỳ vọng | Thực tế |
| --- | --- | --- |
| Sửa `", tổng tiền "` → `", tổng cộng "` trong `speech-segments.json` | `UT-SEG-MANIFEST-12` đỏ | ✅ đỏ |
| Cùng đột biến, chạy `generate-speech-segments.mjs --check` | exit 1 | ✅ `SPEECH_SEGMENTS_DRIFT`, exit 1 |

---

## 5. Ba thứ cố ý **không** làm

1. **Không bật mặc định.** `Segmentation.Enabled = false`. Bật lên là đổi thứ khách nghe, nên
   đó là quyết định của deployment chứ không phải hệ quả phụ của việc nâng cấp. Nhánh
   whole-script cũ giữ nguyên từng byte — `UT-SEG-DEFAULTOFF-09` khoá điều đó.
2. **Không đổi contract.** Không operation mới, không field mới, không `oasdiff`, không re-pin
   `contract-manifest.json`. Toàn bộ nằm sau `ITtsProvider` và `ISimGateway`.
3. **Không tự tổng hợp đoạn cố định ngoài MOCK.** `FixedSegments=Provider` chỉ hợp lệ với fake
   provider. Với vendor thật nó mua lại đúng 203 ký tự đó mỗi lần cache nguội — tức là ném đi
   chính khoản chênh lệch mà kiến trúc lai sinh ra. Validator chặn ở startup.

---

## 6. Việc còn lại của W-0108

| # | Việc | Ai | Chặn bởi |
| --- | --- | --- | --- |
| 6.1 | Thu/render **12 MP3** đoạn cố định (4 câu × 3 miền) — hướng dẫn từng bước: [`segment-render-kit.md`](segment-render-kit.md) | Owner | ~~`OD-VOICE-01`~~ **đã gỡ chặn `2026-08-27`**: owner quyết dùng free tier cho lab. Chỉ còn cần một phiên ElevenLabs; tốn **609 ký tự** |
| 6.2 | Chạy `Convert-LabSegmentAudio.ps1 -SourceDirectory ...` | Dev | 6.1 |
| 6.3 | Dán khối `segments-compose-env.yml` vào anchor `x-asterisk-lab-env` (khối này đã gồm `Segmentation__Enabled=true` và `RegionalVoices__Enabled=true`). Bản `segments-appsettings.json` chỉ dùng cho deployment có mount appsettings — **không** dán được vào compose, xem §9.2 | Dev | 6.2 |
| 6.4 | Gọi 6 lượt MicroSIP × 3 miền, **nghe** đúng đơn của từng lượt | Owner | 6.3 |
| 6.5 | Cấu hình endpoint TTS thật cho đoạn biến thiên | Dev + Infra | `OD-VOICE-01` **nửa production** (mua gói) — nửa lab đã mở, nhưng phần biến thiên cần endpoint sống nên vẫn chờ |

Lệnh in ra đúng 4 câu cần thu, không phải đi tìm trong tài liệu:

```bash
pwsh ./deploy/lab/Convert-LabSegmentAudio.ps1 -ListOnly
```

---

## 7. Những gì bản này **chưa** chứng minh

- **Chưa ai nghe.** Không có file audio thật nào tồn tại. Mọi khẳng định về audio ở đây là về
  *chuỗi xử lý*, không về *âm thanh*.
- **Chưa đo cache trên dữ liệu thật.** Con số "đơn thứ hai tốn 2 lần gọi" là trên fixture; tỉ lệ
  trúng cache thật phụ thuộc phân bố phường/xã và các nhóm SKU thực tế.
- **Quét PII toàn repo đang đỏ** ở `docs/evidence/W-0107/live-capture-findings.md` và
  `docs/evidence/W-0107/vocabulary-review.md`. Hai file đó thuộc **W-0107**, không thuộc W-0108;
  ghi ở đây để không ai đọc nhầm là sạch.

### 7.1 Lỗi contract test duy nhất — thuộc luồng khác, đã chứng minh

`CT-SALES-*` so SHA-256 của `specs/api/openapi/ivr-order-confirmation.v1.yaml` với giá trị ghim
trong `contract-manifest.json`. Một luồng song song đang sửa **một dòng** `summary` của
`POST /feature-flags/{environment}` theo `OD-V1-20` mà chưa re-pin manifest:

| | SHA-256 |
| --- | --- |
| Bản đã commit = giá trị ghim = test kỳ vọng | `98d226b1…` |
| Bản trong cây làm việc (sau sửa của luồng kia) | `7b921b3a…` |

Cùng một dòng đó cũng làm `docs-selftest.mjs` đỏ với `API_DOCS_DRIFT` trên
`ivr-order-confirmation-v1.html`: portal đã sinh không còn khớp nguồn.

Đã chứng minh chứ không suy đoán, bằng cùng một thí nghiệm có thể đảo ngược — tạm khôi phục file
OpenAPI về bản commit, chạy, rồi trả lại nguyên trạng sửa đổi của luồng kia:

| Trạng thái file OpenAPI | `Ivr.ContractTests` | `docs-selftest.mjs` |
| --- | --- | --- |
| Bản commit | **22 / 22** | `API_DOCS_SELFTEST_PASS` |
| Có sửa đổi của luồng `OD-V1-20` | 21 / 22 | `API_DOCS_DRIFT` |

W-0108 **không đụng** OpenAPI: 0 operation mới, 0 field mới. Luồng `OD-V1-20` còn ba việc phải
làm cho chính sửa đổi đó: re-pin `contract-manifest.json`, sinh lại portal API docs, và xử lý
baseline/`oasdiff` tương ứng.

---

## 8. Lệnh tái lập

```bash
node deploy/ci/scripts/generate-speech-segments.mjs --check
dotnet build src/Ivr.Domain/Ivr.Domain.csproj --nologo
dotnet build src/Ivr.Infrastructure/Ivr.Infrastructure.csproj --nologo
dotnet test tests/Ivr.UnitTests/Ivr.UnitTests.csproj --nologo
node deploy/ci/scripts/generate-test-traceability.mjs --check
pwsh ./deploy/lab/Convert-LabSegmentAudio.ps1 -ListOnly
```

---

## 9. Kiểm chứng khô chuỗi bàn giao (`2026-08-26`)

Chạy trước khi owner tốn tiền và thời gian render, bằng **audio giả** (12 sine tone sinh bằng
ffmpeg, đặt đúng tên `<miền>-s<số>.mp3`) trong sandbox tách khỏi repo. Mục đích duy nhất: chứng
minh 12 file thật sẽ chạy đúng ngay lần đầu.

Kết quả: **12 MP3 vào → 12 PCM s16le/8 kHz/mono ra**, `SHA256SUMS` + `segments-manifest.txt` +
khối cấu hình đều sinh đúng. Nhưng lượt chạy phát hiện **hai lỗi thật**, cả hai đã sửa.

### 9.1 · `$LASTEXITCODE=1` sau một lượt chạy thành công

`Convert-LabSegmentAudio.ps1` kiểm định dạng file ra bằng `ffmpeg -hide_banner -i <file>` — gọi
ffmpeg **không có output file**. Lệnh đó in metadata stream hợp lệ nhưng **luôn thoát mã 1**, nên
một lượt chuyển đổi thành công hoàn toàn vẫn để lại `$LASTEXITCODE=1`. Mọi caller dùng `&&`, hoặc
CI, đọc kết quả đó là **thất bại**.

Đây là **lỗi đã từng được tìm ra và sửa ở file anh em**: `Convert-LabVoiceAudio.ps1:137` dùng
`-loglevel info -i <file> -f null -` kèm kiểm `$LASTEXITCODE`, và có sẵn comment giải thích đúng
cơ chế này. `Convert-LabSegmentAudio.ps1` (viết cho W-0108) giữ dạng cũ.

Đã sửa về đúng dạng của file anh em, kèm comment nói rõ hai probe không được lệch nhau lần nữa.
Sau khi sửa: `LASTEXITCODE = 0`, 12 file ra.

### 9.2 · Khối cấu hình "dán thẳng được" **không dán được vào lab**

Script sinh `segments-appsettings.json` — JSON lồng, đúng shape của `TtsProviderOptions`.
`docker-compose.softphone.yml:49` mô tả nó là *"ready-to-paste block"*.

Nhưng lab cấu hình service **hoàn toàn** bằng biến môi trường double-underscore trong anchor
`x-asterisk-lab-env`; **không có chỗ nào mount `appsettings.json`** (kiểm cả
`docker-compose.dev.yml` lẫn `docker-compose.softphone.yml`). JSON lồng **không dán được** vào
một `environment:` mapping.

Hệ quả thực tế: ai đó sẽ phải dịch tay 12 mục × 3 trường = **36 biến có chỉ số mảng**, trong đó có
**12 mã băm 64 ký tự** — đúng thứ mà cả script lẫn compose đều ghi rõ là không được chép tay, vì
một ký tự sai chỉ lộ ra lúc đang gọi khách.

Đã bổ sung: script nay sinh **thêm** `segments-compose-env.yml` — khối biến môi trường thụt 2
khoảng trắng, dán thẳng vào anchor. Giữ nguyên bản JSON cho deployment có appsettings.

Khối này gồm cả `RegionalVoices__Enabled: "true"`: thiếu nó thì `CatalogsByVoice` đọc catalog
**toàn cục** (rỗng) thay vì ba catalog theo miền, và service từ chối khởi động vì "missing a
recording for part of the approved script" — một lỗi đúng nhưng khó truy.

Nếu chạy kèm `-Region` (một hoặc hai miền), file sinh ra mang cảnh báo **CHẠY MỘT PHẦN** ngay
trong header, vì validator đòi đủ ba miền.

### 9.3 · Bằng chứng

| Phép đo | Kết quả |
| --- | --- |
| 12 MP3 giả → PCM | 12/12, đúng `pcm_s16le` / `8000 Hz` / `mono` |
| `$LASTEXITCODE` sau khi sửa 9.1 | **0** (trước khi sửa: 1) |
| Khối env splice vào `x-asterisk-lab-env` + `docker compose config --quiet` | **exit 0** |
| Số khoá `FixedSegments__*` sau khi compose resolve | **36** trên `ivr-api`, **36** trên `ivr-worker` |
| `Ivr__Speech__Tts__Segmentation__Enabled` sau resolve | `true` |
| `docker compose config` trên hai file compose **chưa sửa** | exit 0 (không hồi quy) |
| GitNexus impact `Convert-LabSegmentAudio.ps1` upstream | **LOW** — 0 symbol, 0 execution flow |
| PII scan `docs/evidence/W-0108` | `PII_SCAN_PASS files=2` |

### 9.4 · Ranh giới của lượt kiểm này

Nó chứng minh **chuỗi công cụ** chạy. Nó **không** chứng minh gì về âm thanh: audio đầu vào là
sine tone. Tiêu chí nghiệm thu thứ 5 ở §3 — *nghe đúng tên/món/số tiền của đơn tương ứng* — vẫn
`⛔ chưa`, và vẫn cần 12 file thật cộng một buổi nghe trên MicroSIP.

Hướng dẫn render cho owner: [`segment-render-kit.md`](segment-render-kit.md).
