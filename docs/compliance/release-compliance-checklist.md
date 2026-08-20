# Release compliance checklist — `W-0052` · `P10-1` → cổng `P9-1`

Ngày: `2026-08-19` · Dùng ở `P9-1` (release gate). **Không ô nào tự tick được.**

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
| T-06 | DSAR tìm/xoá đúng phạm vi, audit bất biến | `COMP-DSAR-02` | ✅ |
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
| S-07 | Permission sửa allowlist / kill switch | Permission Core (`OD-V1-20`) | ❌ chưa gán vai trò nào |

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
