# W-0228 — Owner trả lời chỗ rẽ, và cái giá được đếm từ code

Ngày: 2026-09-08 · Baseline: `main@55b11c1` · Trạng thái: **TESTS_PASS**.

Owner, `2026-09-08`: *"`items_spoken` thu trước được, catalog GinsengFood chỉ vài chục món."*

Đó là câu trả lời cho chỗ rẽ `W-0226`/`W-0227` ghi vào cả bốn tài liệu. Lượt này ghi nhận nó, và
đếm cái giá **suy ra từ code** thay vì ước lượng.

## 1. Vì sao đếm được chính xác

`OD-V1-19` bỏ tên khách — thứ duy nhất vô hạn. Ba placeholder còn lại đều phân rã thành **bộ đóng**:

| Bank | Nguồn suy ra | Mỗi miền |
| --- | --- | ---: |
| **A** — số | `VietnameseNumberSpeller` toàn bộ token + `" đồng"` (`renderer:169`) | **22** |
| **B** — nối | `và`, `sản phẩm khác`; dấu phẩy là khoảng lặng | **2** |
| **C** — đơn vị | `item.UnitLabel` | `~10` |
| **D** — tên hàng | `item.PublicName` — *"vài chục món"* | `~40` |
| **E** — vùng giao | `delivery_area_short` | chưa biết |

Bank A tách theo miền vì `VietnameseNumberStyle` khai `Northern = ("nghìn","linh")` còn
`CentralDefault = Southern = ("ngàn","lẻ")` — không dùng chung được.

Tổng `≈ 222 + 3E`, và **không tăng theo độ dài đơn hàng**: `MaximumSpokenItems` bị chặn `1..20`,
phần dư gộp thành *"và N sản phẩm khác"*.

Đặc tả đầy đủ: [`m8-16`](../../../plan/ivr-orther/m8-16-recorded-speech-bank-spec-2026-09-08.md).

## 2. Nhưng chưa gate nào đóng được hôm nay

Đây là chỗ dễ đọc nhầm nhất, nên nói thẳng.

`Segmentation` hiện chỉ phục vụ đoạn **cố định** từ file. Đoạn **động** vẫn tới provider —
`TtsTelemetry` ghi nguyên văn:

```csharp
/// <param name="DynamicSynthesized">Variable pieces that reached the provider.</param>
```

**Không có cơ chế nào phục vụ đoạn động từ ngân hàng ghi âm.** Đó là code chưa viết, không phải một
cờ chưa bật. Nên `INF-A` (mirror) và 16 CVE Security đi *theo hướng* thôi cần, nhưng **hôm nay
chưa**. Cả ba phiếu đã được sửa để nói đúng điều đó thay vì nói *"đã đóng"*.

## 3. Rủi ro thật không phải số lượng clip

Ghép từng **từ** cho *"năm trăm sáu mươi nghìn đồng"* là **sáu mối nối trong một số tiền**. Tiếng
Việt có thanh điệu và ngữ điệu cấp cụm; nối clip rời nghe máy móc dù từng clip đúng.

Repo **đã biết** — `VietnameseOrderScriptRenderer.cs:163-164`:

> *renderer used to emit `"560.000 đồng"` while the audio the owner approved in `W-0104` says
> `"năm trăm sáu mươi nghìn đồng"`*

Bản owner duyệt là câu đọc **liền mạch**, không phải sáu clip ghép.

| | Cách | Clip/miền | Mối nối |
| --- | --- | ---: | --- |
| `R-1` | thu từng từ | **22** | nhiều |
| `R-2` | thu cụm `0..99` liền mạch + thang | **~120** | ít hơn hẳn — cách IVR truyền thống |

`R-2` đắt gấp ~5 lần **một lần duy nhất** ở bước thu, và nó quyết định khách nghe thấy gì.

> **6 cuộc MicroSIP ở `today-03 §3.2`, cột *"6 mối nối `1→2→3→4→5→6→7`"*, chính là bài kiểm cho
> câu này.** Thiết kế từ `29/08`, chưa chạy; nay có thêm một lý do.

## 4. Hai hệ quả chưa ai nêu

### 4.1 — 12 đoạn cố định hiện có là audio **do VieNeu render**

Giữ chúng ⇒ `L3` (quyền thương mại ba preset) **vẫn phải trả lời**, dù model không còn chạy lúc gọi.
Thu lại bằng giọng người ⇒ `L1`–`L4` đóng hết, nhưng chữ ký giọng `28/08` hết hiệu lực và phát sinh
**hợp đồng giọng người** — câu hỏi Legal khác, thay chỗ chứ không xoá.

Và vế dễ bỏ sót nhất: giữ bản VieNeu cho đoạn cố định còn thu người cho đoạn động ⇒ **lệch chất
giọng ngay tại mối nối**, đúng chỗ khách đang nghe.

### 4.2 — `pronunciationHints` mất tác dụng, và đó là đổi hành vi contract

`PrivacySafeSpeech` cho tới **100** hint mỗi task; renderer dùng chúng để đọc đúng tên hàng:

```csharp
pronunciationHints.GetValueOrDefault(item.PublicName, item.PublicName)
```

Cơ chế đó tồn tại **vì** TTS đọc sai tên riêng. Một clip đã thu thì không đọc lại được.

> Nếu chọn *"hint thắng ⇒ rơi về TTS"* thì **vẫn còn phụ thuộc TTS lúc chạy**, và toàn bộ lập luận
> đóng `INF-A`/Security ở §2 **không còn đúng**. Đây là quyết định contract, phải ghi ra chứ không
> để trôi vào — M3 vẫn gửi hint và sẽ không thấy nó có tác dụng.

## 5. Kiểm chứng

```text
gate-status.mjs                GATE_STATUS_PASS — 226 work items
contract-freeze-verifier.mjs   CONTRACT_FREEZE=PASS
docs-selftest.mjs              API_DOCS_SELFTEST_PASS
today-03 thân gói              git hash-object = 2f6c951f… — vẫn nguyên byte sau khi thêm P.6
link check                     m8-16 resolve từ 00-index, 1.4, ba phiếu và today-03
```

`dotnet test` không chạy lại: không file `.cs`/`.mjs`/`.py` nào đổi. Không sửa code, không sửa
`MODELS.lock`, không mở gate nào. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Còn lại — bốn quyết định chặn M8

`m8-16 §9`: thu từng từ hay thu cụm · giữ hay thu lại 12 đoạn cố định · hint bị bỏ qua hay thắng ·
danh sách đơn vị và vùng giao.

**M8 chưa dựng được cơ chế bank khi bốn câu này chưa chốt** — mỗi lựa chọn đổi hình dạng của bank và
của đường tra cứu. Đó là việc M8 **sẽ** làm, không phải việc M8 đang chờ ai làm hộ.
