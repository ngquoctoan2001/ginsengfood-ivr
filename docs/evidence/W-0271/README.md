# W-0271 — Owner ký 35 nghiệm thu, và con số nói thật về nấc 1

Ngày: 2026-09-10 · Baseline: `main@d407dbe` · Trạng thái: **ACCEPTED** (ghi nhận quyết định owner).

Không đổi mã sản phẩm. Toàn bộ thay đổi là **trạng thái governance** trong tracker, cộng bảng
readiness được sinh lại từ đó.

## 1. Quyết định

Ngày 2026-09-10, owner ký nghiệm thu **cả bốn lô** — 35 work item đang ở `EVIDENCE_SUBMITTED`:

| Lô | Số | Work ID |
| --- | ---: | --- |
| 1 — đối chiếu hợp đồng M8 và bàn giao Module 3 | 13 | `W-0142`, `W-0143`, `W-0145`…`W-0154`, `W-0160` |
| 2 — red-team và sửa governance | 13 | `W-0062`, `W-0067`…`W-0077`, `W-0093` |
| 3 — governance phát hành `P11` | 4 | `W-0057`…`W-0060` |
| 4 — rà soát và kế hoạch | 5 | `W-0001`, `W-0102`, `W-0133`, `W-0166`, `W-0175` |

Mỗi dòng được ghi kèm câu phạm vi, theo đúng tiền lệ `W-0104`/`W-0106`/`W-0119`/`W-0120`:

> **chỉ software/MOCK với dữ liệu fake**. Không mở quyền gọi khách thật;
> `REAL_CUSTOMER_CALL_ALLOWED=NO` ở cả 4 môi trường. SIM/carrier/32-eSIM, Sales API thật và quyền
> production vẫn `NOT_RUN/BLOCKED_EXTERNAL`.

Phân bố sau khi ghi: `EVIDENCE_SUBMITTED` **35 → 0**, `ACCEPTED` **8 → 43**.

## 2. Một đính chính của tôi, trước khi owner chốt

Khi trình bày phương án, tôi nói việc ký này **mở nấc 1**. **Sai.** Đã đo lại và báo owner trước khi
ghi:

| | planned | UNPLANNED |
| --- | ---: | ---: |
| `TESTS_PASS` | **57** | 130 |
| `EVIDENCE_SUBMITTED` (trước lượt này) | **18** | 17 |
| `BLOCKED_EXTERNAL` | **15** | 3 |

Nấc 1 đòi **mọi planned prompt** có evidence `ACCEPTED`. Trong 35 item chỉ **18** là planned. Sau khi
ký vẫn còn **57 planned ở `TESTS_PASS`** và **15 planned ở `BLOCKED_EXTERNAL`**.

Bảng readiness sinh lại sau lượt này xác nhận: **nấc 1 vẫn `❌`**.

Cái ký này thật sự đem lại: **18/95 planned prompt** sang `ACCEPTED`, và **toàn bộ bộ tài liệu bàn
giao Module 3 (lô 1) đã được ký** — tức thứ gửi đi được ngay.

## 3. Vì sao 57 item kia không ký kèm được

`prompt/README-governance.md:81`:

> Code/test pass chỉ đủ `TESTS_PASS`; external/live acceptance phải có evidence riêng.

Chúng ở `TESTS_PASS` chứ không phải `EVIDENCE_SUBMITTED` — tức chưa ai **nộp** evidence cho chúng.
Đó là một bước quy trình riêng, không phải một chữ ký, và tôi **không tự chuyển** trạng thái nào
ngoài 35 dòng owner đã duyệt.

## 4. Cách làm

`docs/release/readiness-board.md` và `docs/release/gate-status.yaml` là **file sinh tự động**
(`gate-status.mjs --write`), nên chỉ sửa tracker rồi sinh lại — không sửa tay nội dung được sinh.

```
GATE_STATUS_WRITTEN gates=11 work=254 decisions=2
```

## 5. Kết quả

Gate sweep: **37/39 run**, 2 FAIL — cả hai là pin
`integration-requirements/06-module-3-api-handover.md` của agent khác, đỏ từ trước lượt này.

Không chạy test .NET: lượt này không chạm mã nguồn nào.

## 6. Còn lại sau lượt này

Bốn nấc vẫn `❌`. Đường đi tiếp, theo đúng thứ tự phụ thuộc:

- **57 planned prompt ở `TESTS_PASS`** cần nộp evidence rồi mới ký được — bước quy trình, cần owner
  quyết có nộp theo lô không.
- **15 planned prompt `BLOCKED_EXTERNAL`** không ký được bằng chữ ký nội bộ.
- **Nấc 2** chỉ cần **một SIM thật** chạy xong lab protocol — **không phụ thuộc Module 3**. Đây là
  đường dài nhất vì phụ thuộc Viettel (số cố định + brandname).

Quyết định owner cùng ngày, thực thi ở work item riêng:

- **`"MOCK"` hai chủ sở hữu** → tách bằng hai hằng số, **giữ nguyên giá trị wire** (không migration).
- **Retention `PeriodDays`** → tôi dựng bản đề xuất để pháp chế điền số; tôi không đề xuất con số.
- **`phone_validation_status`** → không còn là quyết định: phát hiện `W-0264` sai về hành vi, xem
  work item sau.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN
