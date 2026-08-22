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
| 6.1 | Thu/render **12 MP3** đoạn cố định (4 câu × 3 miền) | Owner | phiên ElevenLabs; `OD-VOICE-01` |
| 6.2 | Chạy `Convert-LabSegmentAudio.ps1 -SourceDirectory ...` | Dev | 6.1 |
| 6.3 | Dán khối `segments-appsettings.json` vào compose, bật `Segmentation.Enabled=true` | Dev | 6.2 |
| 6.4 | Gọi 6 lượt MicroSIP × 3 miền, **nghe** đúng đơn của từng lượt | Owner | 6.3 |
| 6.5 | Cấu hình endpoint TTS thật cho đoạn biến thiên | Dev + Infra | `OD-VOICE-01` (mua gói) |

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
