# W-0136 — Spec §13.2: thêm yêu cầu vô tuyến vào tài liệu nguồn

Ngày: `2026-08-28`
Baseline: `main@7487c4a`
Trạng thái: `TESTS_PASS`
Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Nợ do W-0135 để lại

`W-0135` sửa hồ sơ mua sắm sang 4G/VoLTE, nhưng `R-00:140` đã tự cảnh báo: *"§13.2 của tài liệu
Module 8 vẫn còn dùng từ 'GSM' và chưa có ràng buộc VoLTE — cần sửa nguồn, nếu không lần sau lại
gộp ra bản sai."*

Sửa hồ sơ mà không sửa nguồn thì lần gộp sau lại ra bản 2G. W-0136 đóng nợ đó. **Docs-only.**

## 2. Verify trước khi sửa

| Kiểm | Kết quả |
| --- | --- |
| Số tiêu chí trong §13.2 nói về công nghệ vô tuyến | **0/7** — toàn bộ nói về API, disposition, DTMF, CDR, SIP |
| Số lần chữ "VoLTE" xuất hiện trong toàn spec | **0** |
| Đường tích hợp §13.2 gọi thiết bị là gì | **"GSM Gateway"** — tức 2G, ngừng hoạt động `15/09/2026` |

Nói cách khác: spec vừa **không** yêu cầu công nghệ nào, vừa **gợi ý** đúng loại thiết bị sắp chết.

## 3. Đã sửa

1. **Thêm yêu cầu #0 vào §13.2**: thoại phải chạy VoLTE (4G); vendor nêu rõ model có module VoLTE
   hay chỉ LTE data + CSFB; là điều kiện loại trừ đầu tiên. Lý do viết đúng theo `W-0135`: 2G chết
   `15/09/2026`, còn CSFB **không** chết cùng lúc mà rơi về 3G tới **tháng 9/2028** — nên lý do loại
   là **horizon**, không phải "chết sau một tháng".
2. **Đường tích hợp**: `GSM Gateway → SIM → nhà mạng` thành `cổng thoại 4G/VoLTE → SIM → nhà mạng`.
3. **Errata `21`** trong bảng đính chính sẵn có của spec.

Sau sửa: "VoLTE" đi từ `0` lên `3` lần; `"GSM Gateway"` chỉ còn trong chính dòng errata trích lại
wording cũ.

## 4. Cố ý KHÔNG sửa

Tên bước 5 **"GSM/SIM Call Execution"** (§6) và mô tả component *"Thực hiện cuộc gọi qua GSM/SIM"*
(§10) giữ nguyên. Đó là **tên bước và tên component**, không phải yêu cầu thiết bị, và đổi sẽ lan
sang `docs/documents/4. phase/phase-8/22-*.md`. Một lượt đổi nửa vời còn tệ hơn không đổi.

Đã ghi thẳng vào errata `21` để không ai đọc chúng như ràng buộc mua.

## 5. Nợ mới, ghi rõ

Bản **`.docx`** cùng tên (`MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.docx`) **không** được cập
nhật. Đây đúng thông lệ đang có — lượt sửa trước (`cf2d884`) cũng chỉ đụng `.md` — nhưng hệ quả là
hai bản đã lệch nhau, và bản `.docx` mới là thứ hay được gửi ra ngoài. Cần Owner quyết cách đồng bộ.

## 6. Một lỗi quy trình của tôi

Tôi **sửa spec trước khi cấp Work ID**, trái rule 3 của ledger (*ghi owner/baseline/acceptance và
Activity `START` trước khi làm*). Đã ghi thành `A-0398 START/PROCESS_NOTE` đúng như đã xảy ra thay
vì viết lại thứ tự cho đẹp.

Nội dung không bị ảnh hưởng: phần verify vẫn đi trước phần sửa. Nhưng thứ tự ledger thì sai, và ghi
lại để lần sau không lặp.

## 7. Verification

| Gate | Kết quả |
| --- | --- |
| `grep VoLTE` trong spec | `0 → 3` |
| `grep "GSM Gateway"` | chỉ còn trong dòng errata trích lại |
| errata `21` | có mặt |
| `gate-status.mjs` | `GATE_STATUS_PASS` — 11 gate, **134** work item, 21 open decision |
| `docs-selftest.mjs` | `API_DOCS_SELFTEST_PASS` |
| `compliance-pack-selftest.mjs` | `COMPLIANCE_PACK_SELFTEST_PASS` |
| Code production | `0 file` |

## 8. Residual

- `.docx` lệch với `.md` — cần Owner quyết cách đồng bộ.
- Tên bước/component còn chữ "GSM" — cố ý, đã ghi errata.
- Số kênh pilot và attempt policy vẫn chưa ký (`W-0135` §6).
