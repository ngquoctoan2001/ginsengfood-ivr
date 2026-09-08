# W-0235 — `3b` thành code, và một tool bắt buộc báo thành công mà không làm gì

Ngày: 2026-09-08 · Baseline: `main@8756e02` · Trạng thái: **TESTS_PASS**.

Owner chốt `3b` theo đề xuất ở `m8-16 §7.2`. Đây là phần thi hành.

## 1. `4` không chặn — nó là dữ liệu, không phải hình dạng

`3b` quyết **đường tra cứu**; `4` chỉ đổ **danh sách** vào đó. Nên cơ chế dựng được ngay, còn bank C
và E đến sau mà không phải sửa lại gì.

`RecordedSpeechCatalog` nhận hai từ điển và trả clip; `RecordedSpeechCatalog.Empty` là trạng thái
một deployment quên nạp bank rơi vào — và nó **từ chối gọi**, thay vì đọc thầm *"đơn hàng gồm một
sản phẩm"*.

## 2. Luật `3b`, đúng như đã đề xuất

| Trường hợp | Kết quả |
| --- | --- |
| tên hàng **có** clip, đơn vị có clip (hoặc không có đơn vị) | **đọc tên** |
| tên hàng **không** có clip | gộp vào *"và N sản phẩm khác"* |
| tên hàng có clip nhưng **đơn vị không** | **cũng gộp** — xem §3 |
| vượt `MaximumSpokenItems` | gộp, như xưa nay |
| **không món nào đọc được** | `TryCompose` trả **`false`** — chặn đáy |

Cụm gộp là **cụm renderer vẫn dùng** cho phần vượt ngưỡng, không phải phát minh mới — nên nó tái
dùng một cơ chế đã có test thay vì dựng cơ chế thứ hai.

## 3. Một quyết định nhỏ tôi tự chốt, và lý do

Món **có** clip tên nhưng **không** có clip đơn vị: gộp luôn, không đọc trần tên.

Bỏ đơn vị thì *"hai hộp Sâm Ngọc Linh"* thành *"hai Sâm Ngọc Linh"*. Khách đang được hỏi để xác
nhận **số lượng** — bỏ đơn vị là bỏ mất chính thứ đang được xác nhận. Ghim ở `UT-VOICE-3B-03`.

## 4. Chặn đáy, và vì sao nó phải là `false` chứ không phải chuỗi rỗng

`TryCompose` trả `false` khi **không món nào** đọc được. Trả về chuỗi rỗng thì caller vẫn dựng được
một câu và vẫn quay số; trả `false` buộc caller phải quyết định — và quyết định đúng là
`REVALIDATE_AND_HOLD_ADMIN_REVIEW`, không phải quay số.

**Chưa nối vào đường render thật, có chủ đích.** Bank C và D còn rỗng, nên bật lúc này là **mọi** món
đều gộp và **mọi** cuộc gọi thành *"N sản phẩm"* — hoặc bị chặn đáy chặn hết. Nối vào là việc của
lượt sau `4`, khi có dữ liệu thật để nạp.

## 5. Tool bắt buộc báo thành công mà không sửa gì

`SpeechNumberClip` nay mang cả clip không phải số (`num-dong` từ `W-0234`, và sắp là `item-*`,
`unit-*`), nên tên đã hẹp hơn thứ nó chứa. `CLAUDE.md` bắt buộc dùng `gitnexus_rename`, không được
find-and-replace. Chạy thì:

```json
{ "status": "success", "old_name": "SpeechNumberClip", "new_name": "SpeechClip",
  "files_affected": 0, "total_edits": 0, "applied": true }
```

Trong khi thực tế còn **35 chỗ** dùng tên cũ trên 4 file. `git status` rỗng — nó **không sửa gì**,
và vẫn báo `success`.

**Nên không đổi tên lượt này.** Không dùng find-and-replace vì luật cấm; không dùng tool vì tool
không chạy. Ghi lại ở đây vì một tool bắt buộc trả `success` cho một thao tác nó không thực hiện là
thứ lượt sau sẽ tin nhầm — y như `gate-status.mjs` chết từ `W-0207` mà không ai biết.

Vết tên để lại nguyên, kèm ghi chú. Đổi tên là việc riêng khi tool chạy được, hoặc khi owner cho
phép cách khác.

## 6. Kiểm chứng

```text
dotnet test Ivr.sln            911/911 PASS, 0 failed, 0 skipped   (905 → 911)
traceability                   TEST_TRACEABILITY_WRITTEN=568        (562 → 568)
gate-status.mjs                GATE_STATUS_PASS — 233 work items
contract-freeze-verifier.mjs   CONTRACT_FREEZE=PASS
docs-selftest.mjs              API_DOCS_SELFTEST_PASS
```

Sáu test `UT-VOICE-3B-01..06`, mỗi luật một cái, cộng một cái ghim `Id` không đổi giữa ba miền còn
`Text` thì có.

> **Một test tôi viết sai, code đúng.** `UT-VOICE-3B-06` ban đầu đặt `1000` vào **số lượng món bị
> gộp** để ép ra từ `nghìn` — nhưng `folded` đếm **số món**, mà `items[]` bị chặn `100` nên không
> đơn hàng thật nào gộp nổi một nghìn món. Chuyển `1000` sang **số lượng** của món có clip thì đúng.

Không sửa renderer, không nối vào đường render thật, không mở gate nào.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 7. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| 4 | danh sách **đơn vị** và **vùng giao** | Vận hành |
| — | nối composer vào renderer, và `delivery_area_short` | M8, **sau** `4` |
| — | ai báo cho IVR khi Sales thêm sản phẩm (`m8-16 §7.2`) | Vận hành |

`4` là câu cuối. Xong nó thì M8 nối được và không còn chờ ai.
