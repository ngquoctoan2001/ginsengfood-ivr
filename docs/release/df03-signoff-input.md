# DF-03 sign-off input — `W-0059` · `P11-3`

Ngày: `2026-08-19` · Trạng thái: **`NOT_READY_FOR_SIGNATURE`**
· Đầu ra dùng cho: `P9-1` release gate

## 0. Tài liệu này là **đầu vào**, không phải chữ ký

Bản ghi cuối cùng là `specs/decisions/DF-03-signoff.md`, và **file đó chưa tồn tại**. Cổng
`SIGNOFF-DF03-04` đỏ nếu nó xuất hiện mà thiếu ô phê duyệt của chủ sở hữu + security/privacy đã điền.

`MASTER-05`: **evidence đã nộp không phải evidence đã được chấp nhận**, và một bản báo cáo không phải
một cổng đã qua. Danh sách §4 dưới đây là **đã nộp**.

## 1. Phạm vi xin ký

| | |
| --- | --- |
| Cái gì | mở `REAL_CUSTOMER_CALL_ALLOWED` cho môi trường `prod` |
| Không bao gồm | lab một SIM (`W-0008`, `OD-V1-20`), 32 eSIM (`G-ESIM32`), ghi âm (DT-05) |
| Chế độ hiện tại | `MOCK` ở cả 4 môi trường; cờ real call `false` ở cả 4 |

## 2. Blast radius nếu ký

**Ai bị ảnh hưởng.** Mọi khách có đơn Golden Hour ONLINE hoặc 24/7 COD lọt qua điều kiện — tức
người thật nhận cuộc gọi tự động từ số của doanh nghiệp.

**Cái gì giới hạn nó.**

| Cơ chế | Trạng thái |
| --- | --- |
| kill switch toàn cục | **đã dựng**, ép ở chart: chế độ khác `MOCK` mà tắt kill switch thì render hỏng |
| allowlist đích (lab) | **đã dựng**; `LAB_REAL_SIM` mà allowlist rỗng → render hỏng |
| attempt policy: max, cửa sổ, offset | **cơ chế đã dựng**; **chính sách production chưa ký** (`W-0007`) |
| do-not-call | **đã dựng**, chặn cứng ở cả ba trạng thái |
| trần số kênh SIM | một SIM ở lab; 32 eSIM **chưa mua** |
| rollback | `helm rollback --atomic` + `after_script`; **chưa lượt deploy nào từng chạy** |

**Cái gì KHÔNG giới hạn nó.** Không có giới hạn theo tỉ lệ phần trăm khách, không có canary theo
lưu lượng khách thật (Argo Rollouts chưa cài), và **không có cách dừng một cuộc gọi đang diễn ra** —
kill switch chặn cuộc **mới**, không cắt ngang cuộc đang chạy.

## 3. Điều kiện tiên quyết — trạng thái đo được `2026-08-19`

| # | Điều kiện | Trạng thái |
| --- | --- | --- |
| P-01 | PIA đã ký | ❌ `DRAFT_UNSIGNED` |
| P-02 | Chu kỳ retention đã ký (DF-07) và đã điền config | ❌ `LEGAL_SIGNOFF_REQUIRED` |
| P-03 | Recording OFF được chốt (DT-05) | ❌ dự thảo, chưa ký |
| P-04 | Cơ sở pháp lý cuộc gọi | ❌ đề xuất kỹ thuật, chưa ký |
| P-05 | Whitelist trường script (`OD-V1-15`) | ❌ mở |
| P-06 | Attempt policy production (`W-0007`) | ❌ mở |
| P-07 | Permission sửa allowlist/kill switch (`OD-V1-20`) | ❌ chưa gán vai trò nào |
| P-08 | Hợp đồng Sales Target V1 (`W-0002`..`W-0006`) | ❌ `BLOCKED_EXTERNAL` |
| P-09 | Lab một SIM đạt (`W-0008`) | ❌ `BLOCKED_EXTERNAL` |
| P-10 | 32 eSIM có năng lực đo được (`G-ESIM32`) | ❌ `BLOCKED_EXTERNAL` |
| P-11 | Cluster + registry + secret store (`W-0063`) | ❌ `BLOCKED_EXTERNAL` |
| P-12 | Phê duyệt MR độc lập trên GitLab (`W-0061`) | ❌ cần Premium/Ultimate + reviewer thứ hai |
| P-13 | Evidence được reviewer **chấp nhận**, không chỉ nộp | ❌ chưa mục nào `ACCEPTED` trừ 4 |

**13/13 chưa đạt.** Không mục nào IVR tự đóng được.

## 4. Evidence đã nộp (không phải đã chấp nhận)

| Phase | Work | Evidence | Trạng thái |
| --- | --- | --- | --- |
| P5 quality | `W-0035`..`W-0039` | `docs/evidence/W-0035` … `W-0039` | `TESTS_PASS` |
| P6 observability | `W-0040`..`W-0042` | `docs/evidence/W-0040` … `W-0042` | `TESTS_PASS` |
| P7 deployment | `W-0043`..`W-0047` | `docs/evidence/W-0043` … `W-0047` | `TESTS_PASS`; deploy/canary runtime vẫn `NOT_RUN` |
| P8 lab | `W-0048`, `W-0049` | — | `BLOCKED_EXTERNAL`, **chưa có** |
| P10 compliance | `W-0052`, `W-0053`, `W-0055` | `docs/evidence/W-0052`, `W-0053`, `W-0055` | `TESTS_PASS` |
| P11 closure | `W-0057`, `W-0058` | `docs/evidence/W-0057`, `W-0058` | `EVIDENCE_SUBMITTED` |

**Hàng P8 trống, và đó là hàng quan trọng nhất.** Không có evidence nào từ một cuộc gọi thật, vì
chưa có cuộc gọi thật nào.

## 5. Giới hạn tồn dư phải ghi vào bản ghi ký

Người ký phải thấy những thứ này, không phải tìm ra sau:

1. **Chưa pipeline CD nào từng chạy** — không runner, không registry (`W-0061`, `W-0063`). Mọi
   evidence deploy là `NOT_RUN`.
2. **Chưa canary/blue-green nào từng chạy** — Argo Rollouts chưa cài, không Prometheus nào nhận
   metric.
3. ~~**`NETPOL_ENFORCEMENT` chưa chứng minh.**~~ **Đã đóng `2026-08-19`** — và kết luận cũ ("cần
   CNI thực thi được") là **sai**: cluster vẫn thực thi, phép đo bị đua với thời điểm kube-router
   cài luật. `docs/evidence/W-0044` §5.
4. **DR drill chạy trên một host**, không phải multi-AZ. Không mã hoá volume at-rest, không KMS,
   không PITR.
5. **Chưa cổng nào ép chiều ngược của migration** — code mới phải chịu được schema cũ.
6. **Rotation drill chạy trên một container**, chưa phải rolling restart nhiều replica.
7. **Khoá ký JWT không rotate được** — `MockOidcIssuer` sinh RSA theo tiến trình (`W-0006`).
8. **`P4-5` / `P4-6` là `DEFERRED_TARGET` có chủ ý** — V1 không gửi thông báo, và IVR không giữ
   registry do-not-call.
9. **Không có cách cắt ngang một cuộc gọi đang diễn ra.**

## 6. Đề xuất cho `P9-1`

**NO-GO.** Không phải vì một mục nào hỏng, mà vì **13/13 điều kiện tiên quyết chưa đạt** và hàng
evidence quan trọng nhất (P8, cuộc gọi thật) trống.

Đường ngắn nhất tới GO không đi qua thêm việc kỹ thuật nào ở IVR. Nó đi qua: chữ ký (P-01..P-06),
một quyết định permission (P-07), một hợp đồng (P-08), và mua sắm (P-09..P-11).
