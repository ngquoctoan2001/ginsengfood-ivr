# Báo cáo tiến độ IVR Order Confirmation — Tổng quan

**Ngày lập:** 2026-08-26 · **Module:** 8 — IVR Order Confirmation (`ginsengfood-ivr`)
**Baseline:** `main@bdde72c` (W-0118 / OD-15, 2026-08-25)
**Phương pháp:** đọc trực tiếp `src/**`, `admin-ui/**`, `tests/**`, `specs/**`, `docs/**`,
`plan/**`, `integration-requirements/**`, `prompt/_execution/prompt-execution-tracker.md`.
**Không** đọc lại các báo cáo cũ (`docs/reports/2026-08-15`, `2026-08-22`) theo yêu cầu.

> **Đây không phải tracker thứ hai.** Nguồn tiến độ duy nhất vẫn là
> [`prompt/_execution/prompt-execution-tracker.md`](../../../prompt/_execution/prompt-execution-tracker.md).
> Bộ báo cáo này là **ảnh chụp ngày 26/08** để đọc và trình bày, không thay thế tracker.

---

## 0. Bộ báo cáo gồm 6 file

| File | Trả lời câu hỏi |
| --- | --- |
| **00-TONG-QUAN.md** (file này) | Tình hình chung trong 2 trang |
| [01-NANG-LUC-HE-THONG.md](01-NANG-LUC-HE-THONG.md) | **Hệ thống làm được gì** — liệt kê chi tiết từng chức năng, từng endpoint, từng màn hình |
| [02-TIEN-DO-CHI-TIET.md](02-TIEN-DO-CHI-TIET.md) | Đã làm những gì, theo từng phase P0→P11 và từng Work ID |
| [03-CAN-GI-TU-MODULE-3.md](03-CAN-GI-TU-MODULE-3.md) | **Cần gì từ Module 3** và các bên ngoài khác |
| [04-TON-DONG-VA-RUI-RO.md](04-TON-DONG-VA-RUI-RO.md) | Còn tồn đọng gì, rủi ro nào |
| [05-KE-HOACH-HOAN-THIEN.md](05-KE-HOACH-HOAN-THIEN.md) | **Kế hoạch chi tiết đến khi hoàn thiện** |

---

## 1. Kết luận một trang

**Phần mềm đã xong. Phần tích hợp thật thì chưa bắt đầu được.**

Nói chính xác hơn, bằng ba câu tách bạch:

1. **Toàn bộ 12 phase kỹ thuật (P0 → P11) đã có implementation và test.** 51 HTTP route,
   16 trang console, 10 background service, 30 bảng PostgreSQL, 774 test .NET + 223 test console đều
   xanh, 474 test có gắn mã truy vết. Luồng đi từ *nhận task* → *kiểm điều kiện* → *lập lịch* → *quay số* → *đọc lời
   thoại tiếng Việt theo miền* → *nhận phím* → *chuẩn hoá kết quả* → *gọi callback về Sales*
   chạy được **đầu-cuối** trên môi trường mock và đã được **quay số thật qua Asterisk + softphone**
   (W-0104, `ACCEPTED`).

2. **Nhưng hệ thống chưa gọi được một khách hàng thật nào**, và điều đó **không phải do IVR**.
   Nó bị chặn bởi **11 cổng ngoài** mà chủ sở hữu là Sales/Module 3, Security/Platform,
   Legal/Privacy, Infra/vendor và Release owner — không cổng nào đội IVR tự đóng được.

3. **Cổng lớn nhất, và duy nhất chặn cứng cả hai chiều, là Module 3.** Module 3 chưa có
   producer đẩy task 24/7 COD, và **chưa có endpoint callback generic** — nghĩa là chương trình
   24/7 hiện **không có lối trả kết quả nào**. Không có hai thứ đó, mọi phần còn lại của IVR chỉ
   là một hệ thống hoàn chỉnh không có đầu vào và không có đầu ra.

---

## 2. Đang đứng ở nấc nào

Thang bốn nấc theo `docs/release/readiness-board.md`:

| Nấc | Điều kiện vào | Đạt chưa |
| --- | --- | --- |
| 1 · `IMPLEMENTATION_COMPLETE_BEHIND_MOCKS` | mọi prompt có evidence được **chấp nhận**, không cổng nào `BLOCKED_INTERNAL` | ❌ |
| 2 · `LAB_REAL_SIM_VERIFIED` | 1 SIM thật chạy xong lab protocol, có allowlist + kill switch evidence | ❌ |
| 3 · `REAL_SALES_INTEGRATION_VERIFIED` | Target V1 đã ký, contract test chạy trên Sales sandbox thật | ❌ |
| 4 · `PRODUCTION_REAL_ELIGIBLE` | đo được capacity 32 eSIM, evidence legal/security được chấp nhận, `DF-03` đã ký | ❌ |

**Đang ở nấc 0.**

Điều này cần được đọc đúng. Nấc 1 **không** đo "code đã viết xong chưa" — nó đo
**"evidence đã được người có thẩm quyền chấp nhận chưa"**. 5/119 work item ở trạng thái
`ACCEPTED`; phần còn lại cao nhất là `EVIDENCE_SUBMITTED`. Đây là khoảng cách **thủ tục**,
không phải khoảng cách **kỹ thuật** — nhưng governance của dự án (`MASTER-05`) không cho phép
đánh đồng hai thứ đó, và báo cáo này giữ đúng nguyên tắc ấy.

> **Không có phần trăm ở đâu trong bộ báo cáo này.** Một con số phần trăm mời người đọc hiểu
> "94% xong" là gần xong, trong khi phần còn lại là toàn bộ những cổng **không ai trong đội IVR
> đóng được**. Báo cáo dùng **số đếm**, không dùng tỉ lệ.

---

## 3. Phân bố 119 work item

| Trạng thái | Số lượng | Nghĩa |
| --- | ---: | --- |
| `TESTS_PASS` | 78 | code xong, test xanh, chờ reviewer/owner ký |
| `EVIDENCE_SUBMITTED` | 19 | đã nộp evidence pack, chờ chấp nhận |
| `BLOCKED_EXTERNAL` | 15 | **chờ bên ngoài** — không tự gỡ được |
| `ACCEPTED` | 5 | đã được owner chấp nhận |
| `DEFERRED_TARGET` | 2 | cố ý hoãn (notification, opt-out loop) |
| **Tổng** | **119** | |

---

## 4. Bốn con số đáng nhớ

| | Con số | Nghĩa |
| --- | ---: | --- |
| Test tự động | **774 / 774** .NET + **223 / 223** console | unit 486 · integration 258 · contract 22 · chaos 8 — **chạy lại và xác minh độc lập ngày 26/08** trên `main@bdde72c`, cả hai suite exit code 0, 0 failed |
| Cổng ngoài còn mở | **11** | `G-CONTRACT`, `G-SPEECH`, `G-DIAL`, `G-AUTH`, `G-POLICY`, `G-LAB-SIM`, `G-ESIM32`, `G-LEGAL`, `G-RELEASE`, `G-GITLAB`, `G-PLATFORM` |
| Quyết định còn mở | **21** `OD-V1-*` + 2 `OD-VOICE-*` | không được đóng bằng suy luận |
| Việc Module 3 phải làm | **4 hạng mục** (A/B/C/D) | 2 hạng mục đầu chặn cứng ngày cắm thật |

---

## 5. Nếu chỉ đọc được một mục

**Việc cần làm ngay, không chờ ai:**

1. Owner **nghe và ký** 3 giọng đọc (`OD-VOICE-05`) và **mua gói ElevenLabs Starter `$6`**
   (`OD-VOICE-01`) → mở khoá 12 file audio đoạn cố định → chạy được buổi nghiệm thu
   "khách nghe đúng đơn của chính mình".
2. **Gửi [`integration-requirements/06-module-3-api-handover.md`](../../../integration-requirements/06-module-3-api-handover.md)
   cho Module 3 và đòi ngày trả lời.** Tài liệu đã viết xong, có payload copy-paste được và ô ký;
   nó chỉ đang nằm trong repo.
3. **Chốt ma trận `program × payment`** — đây là điểm mâu thuẫn số 1: tài liệu business
   (`DS-01`) nói IVR-callable là **COD-only**, còn Target V1 đang enforce
   `GOLDEN_HOUR+ONLINE` ở **4 tầng độc lập**. Sai ma trận = **100% task bị từ chối, im lặng**.

Chi tiết ba việc này và toàn bộ critical path nằm ở [05-KE-HOACH-HOAN-THIEN.md](05-KE-HOACH-HOAN-THIEN.md).

---

## 6. Điều bộ báo cáo này **không** nói

- **Không nói "xong hết prompt" là sẵn sàng go-live.** Nấc 1 còn chưa đạt, và nó là nấc thấp nhất.
- **Không đóng cổng ngoài nào.** Chỉ artifact thật (OpenAPI đã ký, credential đã test, lab report,
  chữ ký Legal) mới đóng được cổng — một bản báo cáo thì không.
- **Không tuyên bố `PRODUCTION_READY`, `CONTRACT_LOCKED` hay "chỉ cấu hình là chạy".**
- **Không thay evidence.** `TESTS_PASS` ≠ `ACCEPTED`; mock evidence ≠ lab evidence ≠ real evidence.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` ở **cả 4 môi trường**, và báo cáo này không đổi điều đó.
