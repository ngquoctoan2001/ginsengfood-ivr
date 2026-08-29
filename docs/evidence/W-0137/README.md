# W-0137 — Bản `.docx` V0.3 mồ côi và lệch nội dung

Ngày: `2026-08-28`
Baseline: `main@79f17b0`
Trạng thái: `TESTS_PASS`
Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

> **Cập nhật `2026-08-29` — W-0141:** các mục bên dưới giữ nguyên snapshot W-0137 tại thời điểm
> `OD-20` còn `PENDING`. Sau đó Module 8 / Project Owner đã ký phương án thu hồi và W-0141 đã đổi
> tên DOCX thành `MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN_SUPERSEDED.docx`, giữ nguyên bytes.
> Xem §8 và [`docs/evidence/W-0141/README.md`](../W-0141/README.md).

## 1. Nợ do W-0136 để lại

`W-0136` sửa spec `.md` sang 4G/VoLTE và **tự ghi nợ** rằng bản `.docx` cùng tên không được cập
nhật. W-0137 đo xem nợ đó lớn cỡ nào.

## 2. Đo thật, không suy từ ngày tháng

Giải nén `.docx` và đọc thẳng `word/document.xml`:

| Kiểm | `.docx` | `.md` |
| --- | --- | --- |
| Độ dài nội dung | `45.807` ký tự | `101.773` byte |
| `VoLTE` | **0 lần** | 3 lần |
| `GSM Gateway` | **2 lần** | chỉ trong dòng errata trích lại |
| Dòng errata `(sửa 27/08)` | **0** | 8 |
| Dòng errata `(sửa 28/08)` | **0** | 2 |

Kết luận: `.docx` **không phải bản Word của tài liệu hiện hành**. Nó là một bản **ngắn hơn một
nửa**, thiếu trọn đợt rà soát 27/08, và **vẫn yêu cầu thiết bị 2G** — đúng thứ đã bị bác ở errata
`21`.

## 3. Và không ai dùng nó

`grep` toàn repo: **không tài liệu nào tham chiếu bản V0.3 `.docx`**. Mọi tham chiếu hiện có trong
`specs/02-business-goals.md`, `specs/05-current-docs-review.md`,
`plan/ivr-orther/questions-to-module-3-and-3.1.md` và `_archive/01-reading-inventory.md` đều trỏ bản
**V0.2**.

Nên rủi ro không nằm trong repo. Nó nằm ở chỗ `.docx` là định dạng hay được **gửi ra ngoài** — một
bản có fact sai có thể tới tay vendor mà không ai trong repo nhận ra.

## 4. Đã làm

1. **Errata `22`** trong chính bản `.md`: tuyên bố `.md` là bản có hiệu lực, kèm đủ số đo ở §2 để
   người đọc tự kiểm.
2. **`OD-20` PENDING** trong `decisions-log.md` với ba hướng: thu hồi (đổi tên `_SUPERSEDED` hoặc
   xoá), tái sinh (sinh lại từ `.md` + gate chặn lệch), hoặc giữ nguyên. **Đề xuất hướng 1** vì
   khớp thực tế là không ai dùng nó.
3. **Không xoá.** Đây là tài liệu của Owner; IVR ghi nhận và đề xuất, không tự quyết.
4. Dọn index GitNexus đang stale (hook cảnh báo từ ba commit trước): `50.908` node / `70.150` edge /
   `300` flow.

## 5. Vì sao không dựng gate chặn lệch ngay

Đã cân nhắc và bỏ. Kiểm bằng `mtime` thì sai — git không giữ mtime nên clone sạch cho mọi file cùng
một dấu thời gian. Kiểm bằng ngày commit thì phải gọi `git` trong selftest, lệch với cách các
selftest hiện có làm việc. Kiểm bằng nội dung thì phải giải nén `.docx` trong Node, cần thêm phụ
thuộc.

Cả ba đều là hạ tầng cho **một cặp file** mà một trong hai có thể sắp bị thu hồi. Dựng gate trước
khi Owner trả lời `OD-20` là làm ngược thứ tự — nếu chọn hướng 1 thì gate thành rác ngay.

## 6. Verification

| Gate | Kết quả |
| --- | --- |
| errata `22` | có mặt |
| `OD-20` | có mặt, `PENDING` |
| `gate-status.mjs` | `GATE_STATUS_PASS` |
| `docs-selftest.mjs` | `API_DOCS_SELFTEST_PASS` |
| GitNexus index | refresh, hết stale |
| Code production | `0 file` |

## 7. Residual

- `OD-20` chờ Owner: thu hồi hay tái sinh `.docx`.
- Nếu chọn **tái sinh**, phải chốt luôn ai chịu trách nhiệm sinh lại mỗi lần `.md` đổi — không có
  câu đó thì lệch lại tái diễn.
- Bản **V0.2 `.docx`** (`1.9 MB`, tháng 6) vẫn là thứ nhiều tài liệu đang trỏ tới. W-0137 không
  đụng, nhưng đáng soi ở lượt sau: các `specs/` đang lấy V0.2 làm nguồn trong khi V0.3 đã supersede.

## 8. Follow-up W-0141 — quyết định đã thực thi

- `OD-20`: `IMPLEMENTED / OPTION_1_WITHDRAW`.
- Người ký: **Tôi — Module 8 / Project Owner**, `2026-08-29`.
- Source cũ: `docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.docx` — không còn tồn tại.
- Artifact giữ lại:
  `docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN_SUPERSEDED.docx` — tồn tại, `45.101` byte.
- SHA-256 trước/sau rename:
  `b2b95c9cb62e14b8138538b8447117040207641e5c565e4e1881f3a55af0935c` — không đổi.
- Không sửa nội dung DOCX, không xóa binary, không đụng bản V0.2 hoặc report DOCX.
- W-0137 giữ `TESTS_PASS`; rename không phải Release acceptance và không đóng external gate.
