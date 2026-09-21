# Release compliance checklist — `W-0052` · `P10-1` → cổng `P9-1`

Ngày: `2026-08-19` · Dùng ở `P9-1` (release gate). **Không ô nào tự tick được.**

## Cập nhật 21/09/2026 — W-0330

Bảng §2–§4 bên dưới là bản lịch sử. Khi xét hiện tại, dùng các hiệu chỉnh sau cùng
[PIA cập nhật](pia.md#cập-nhật-kỹ-thuật-21092026--w-0330); không dùng dấu xanh lịch sử làm chứng cứ deploy.

| Dòng | Hiệu chỉnh hiện hành |
| --- | --- |
| T-01 | Điều kiện “không lưu số” không còn mô tả thiết kế: `phone_e164` được lưu từ W-0311. W-0314 sửa DSAR để xoá số; rủi ro S2 chưa được bảng này tự đóng. |
| T-06 | CLI đã có ở [W-0330](../evidence/W-0330/README.md), kèm COMP-DSAR-13..18 cho rollback audit, quyền OS, preview/một đơn/lặp lại và bảo toàn cấu hình kết nối. Bằng chứng local không thay lượt owner chạy trên môi trường được chọn. |
| T-09/T-10 | Test backup/restore lịch sử vẫn có phạm vi riêng. S3 giữ vĩnh viễn; không suy “restore bị retention xử lý” thành chắc chắn dữ liệu DSAR biến mất. Owner phải chứng minh cách áp lại yêu cầu sau restore. |
| S-02 | S3/W-0316 đã thay các kỳ hạn cũ; không điền số vào PeriodDays. S2/PIA và kiểm cấu hình của môi trường thật vẫn riêng. |
| S-03/S-04 | OD-V1-15 đã chốt whitelist; OD-V1-08/16 đã chốt attempt policy. Quyết định không chứng minh client M3 hoặc production đã chạy đúng. |
| S-07 | Permission đã có; DI đã dùng các verifier PostgreSQL thay Pending*. Kiểm bản ghi duyệt và nhiều actor ở môi trường đích; không còn mô tả production luôn từ chối do hard-code. |
| S-01/S-05/S-06 và hạ tầng | Không có chữ ký PIA/go-live hoặc bằng chứng hạ tầng mới trong W-0330. Toàn/owner xử lý đầu vào thực tế; agent không tự tick. |

Hướng dẫn thao tác: [DSAR từng bước](dsar-cli-step-by-step.md). REAL_CUSTOMER_CALL_ALLOWED=NO.

## 1. Cách đọc

Mỗi dòng có ba cột: **điều kiện**, **bằng chứng nào chứng minh nó**, và **ai xác nhận**. Một dòng
không có bằng chứng trỏ được tới là một dòng **chưa đạt**, kể cả khi ai đó đã tick.

Cột "Trạng thái hôm nay" là trạng thái đo được ngày `2026-08-19`, không phải lời hứa.

## 2. Cổng kỹ thuật — IVR tự chứng minh được

| # | Điều kiện | Bằng chứng | Trạng thái hôm nay |
| --- | --- | --- | --- |
| T-01 | Không lưu số điện thoại thật | `D-05`, check constraint `ck_ivr_confirmation_tasks_masked_phone` | ✅ |
| T-02 | PII guard chạy trên response và correlation id, fail-closed | `UT-FND-PII-12` | ✅ |
| T-03 | Recording OFF | DT-05, `recording_ref` null | ✅ |
| T-04 | Do-not-call là chặn cứng ở cả ba trạng thái | `COMP-DNC-03`, `UT-ELIG-VOICE-15` | ✅ |
| T-05 | Danh mục dữ liệu cá nhân khớp schema đang ship | `COMP-PII-01` | ✅ |
| T-06 | DSAR tìm/xoá đúng phạm vi, audit bất biến | `COMP-DSAR-02` · `COMP-DSAR-08..12` | ✅ đúng phạm vi, gồm số điện thoại (`W-0314`) · ⚠️ **chưa có lối chạy** ngoài test — `S8` |
| T-07 | Kho phân tích không chứa PII | `BI-PII-01` | ✅ |
| T-08 | TLS tới database ép ở chart, `Prefer` bị từ chối | `DG-CRYPTO-01` | ✅ |
| T-09 | Backup mã hoá + xác thực + restore đã kiểm | `DG-BACKUP-02` | ✅ |
| T-10 | Backup tuân retention, bản restore vẫn bị retention xử lý | `DG-RETENTION-04` | ✅ |
| T-11 | Mọi class retention job chạy đều được phân loại | `COMP-RETENTION-04` | ✅ |
| T-12 | Ladder: không job/chart nào mở được real call | `IT-CD-REAL-03`, `IT-K8S-GATE-02` | ✅ |

## 3. Cổng chữ ký — IVR **không** tự đóng được

| # | Điều kiện | Ai ký | Trạng thái |
| --- | --- | --- | --- |
| S-01 | PIA đã ký | Legal + Privacy | ❌ `DRAFT_UNSIGNED` |
| S-02 | Chu kỳ retention đã ký, và đã điền vào config từng env | Legal | ❌ `UNSIGNED` |
| S-03 | Whitelist trường script đọc cho khách | Privacy/Legal (`OD-V1-15`) | ❌ mở |
| S-04 | Attempt policy production | Product/Core (`W-0007`) | ❌ mở |
| S-05 | Cơ sở pháp lý cuộc gọi transactional | Legal | ❌ đề xuất kỹ thuật, chưa ký |
| S-06 | Sign-off go-live (DF-03) | Release owner | ❌ mở |
| S-07 | Permission sửa allowlist / kill switch | Permission Core (`OD-V1-20`) | ⚠️ permission **đã cấp cho `Admin`** 2026-08-22 (owner module IVR); chưa ✅ vì thiếu **hai** thứ: chữ ký Security/Platform + Release owner, và một `IRuntimeGateAuthorization` duyệt thật (bản production vẫn `false` → `409`) |

## 4. Cổng hạ tầng — chờ `W-0063`

| # | Điều kiện | Trạng thái |
| --- | --- | --- |
| I-01 | Mã hoá volume at-rest | ❌ thuộc storage class |
| I-02 | KMS cho khoá backup, có rotation | ❌ |
| I-03 | Multi-AZ cho database | ❌ drill chạy trên một host |
| I-04 | Cluster + credential 4 môi trường | ❌ |

## 5. Luật của cổng này

**Một dòng ở §3 hoặc §4 còn ❌ thì không go-live.** Không có "chấp nhận rủi ro tạm thời" cho §3: đó
là các mục mà rủi ro **không thuộc về** người bấm nút release.

§2 xanh hết **không** đủ. Nó chỉ nói hệ thống làm đúng thứ nó được thiết kế; §3 mới nói ai đã đồng ý
với thiết kế đó.

## 6. Điều checklist này không kiểm

- **Chưa gọi khách thật lần nào.** Mọi ✅ ở §2 nói về hệ thống chạy `MOCK`. Lần lab đầu tiên
  (`W-0008`) sẽ phải chạy lại checklist này.
- **Không kiểm được §2 khớp §3.** Ví dụ: T-11 khẳng định mọi class đều được phân loại, nhưng
  **không** khẳng định con số ngày ai đó điền vào config bằng con số đã ký ở `retention.md`. Đó là
  việc của người điền, và ở đây chỉ có thể ghi ra rằng không cổng nào phủ nó.
