# PROMPT P1-5 — Retention Job & Data Lifecycle

## 0. Meta
| | |
| --- | --- |
| **ID** | `P1-5` |
| **Work ID** | `W-0064` (canonical tracker §5) |
| **Phase** | 1 — Contracts and Data |
| **Prereq (blockedBy)** | `P1-2` |
| **Blocks** | `P7-2` (CronJob), `P9-2` (retention ops drill), `P10-1` (DSAR/PDPA), `P10-2` (backup retention) |
| **Governance flag** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_EXECUTION_MODE=MOCK` |
| **Stack** | .NET 10 · PostgreSQL |
| **Execution mode** | `MOCK` |

## 1. ROLE
Bạn là **Senior .NET/Data Engineer**. Bạn xây job xoá/ẩn danh dữ liệu theo data class và retention period, an toàn với legal hold, có dry-run và audit đầy đủ.

## 2. CONTEXT
`P7-2` schedule một CronJob chạy `IRetentionJob` và gán cho “(P1-2)”. `IRetentionJob` xuất hiện **đúng một lần** trong toàn repo — chính dòng đó. `P1-2` chỉ thêm “retention columns”, không tạo job/purge/interface nào. `P9-2`, `P10-1`, `P10-2` đều giả định retention job đã tồn tại và chạy được. Slice này tạo thứ mà bốn prompt kia phụ thuộc vào.

## 3. SOURCE SPECS (đọc trước khi code — bắt buộc)
- `specs/database/05-retention-and-privacy.md` (data class + retention)
- `specs/database/02-tables.md` §8 (bảng foundation), §9 (phân loại nguồn cột)
- `specs/data/05-pii-policy.md`, `specs/functional/08-evidence-audit-privacy.md`
- `plan/ivr-orther/decisions-log.md` §DF-07 (retention), §DT-05 (recording off)
- `prompt/README-governance.md` §2, §4

## 4. DECISIONS & CONSTRAINTS
- **DF-07 (⏳ chưa chốt số):** retention period cho từng data class **chưa có giá trị được Legal ký**. Job phải đọc period từ config theo data class và **fail-closed** (không xoá gì) khi period chưa được cấu hình — thà giữ dữ liệu còn hơn xoá sai.
- **Audit là append-only:** `ivr_audit_log` không bao giờ bị job xoá trong V1; nếu Legal yêu cầu, phải là quyết định riêng có ghi trong `OD-V1-11`.
- **Legal hold thắng retention:** bản ghi có legal hold không bị xoá kể cả khi quá hạn.
- **Không xoá evidence đã accepted** (governance §7).

## 5. INPUTS / DEPENDENCIES
- `REAL_AVAILABLE`: schema + EF context (P1-2).
- `OWNER_DECISION_REQUIRED`: giá trị retention period từng data class (DF-07 / `OD-V1-11`, Legal/Privacy).
- `BLOCKED_EXTERNAL`: không — job chạy được với config test trong MOCK.

## 6. BUILD STEPS
1. `IRetentionJob` trong `Ivr.Domain`: `Task<RetentionRunReport> RunAsync(RetentionRunOptions options, CancellationToken ct)`; `RetentionRunOptions { DryRun, DataClasses, Now }`.
2. Bảng cấu hình data class → period, đọc từ `ivr_feature_flags`/config: `task_metadata`, `attempt_metadata`, `result_metadata`, `callback_metadata`, `raw_call_event`, `speech_snapshot`, `evidence_link`, `idempotency_key`, `review_item`. Thiếu period cho class nào → **skip class đó và báo `NOT_CONFIGURED`**, không xoá.
3. Chiến lược mỗi class: `DELETE` hoặc `ANONYMIZE` (giữ hàng, xoá trường nhạy cảm) — khai báo rõ trong `specs/database/05-retention-and-privacy.md`.
4. `DryRun=true` mặc định: chỉ đếm và báo cáo, không ghi. Chạy thật phải bật tường minh.
5. Legal hold: cột/bảng `legal_hold_until`; bản ghi trong hold bị loại khỏi mọi batch.
6. Batch + resumable: xoá theo lô có giới hạn, ghi checkpoint, an toàn khi bị kill giữa chừng; không giữ transaction dài khoá bảng scheduler.
7. Ghi `RetentionRunReport` vào `ivr_audit_log` + `docs/evidence/W-0064/`: class, số hàng xét, số hàng xoá/ẩn danh, số bị giữ do legal hold, số `NOT_CONFIGURED`.
8. Metric + alert khi job fail hoặc khi một class ở `NOT_CONFIGURED` quá N ngày (nối `P6-2`).
9. Cập nhật `specs/database/05-retention-and-privacy.md`: mỗi bảng ở `02-tables.md` §8 phải có data class + chiến lược, kể cả bảng foundation mới.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Domain/Retention/IRetentionJob.cs`, `RetentionRunOptions.cs`, `RetentionRunReport.cs` | Port + model |
| `src/Ivr.Infrastructure/Retention/RetentionJob.cs` | Implementation batch/resumable |
| `src/Ivr.Worker/Jobs/RetentionJobHost.cs` | Host để `P7-2` schedule |
| `specs/database/05-retention-and-privacy.md` (cập nhật) | data class × strategy × period-source |
| `tests/Ivr.IntegrationTests/Retention/**` | test theo §8 |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-RET-CONFIG-01` | unit | Class thiếu period → `NOT_CONFIGURED`, không xoá hàng nào. |
| `IT-RET-DRYRUN-02` | integration | `DryRun=true` không thay đổi dữ liệu nhưng báo cáo đúng số lượng. |
| `IT-RET-DELETE-03` | integration | Quá hạn → xoá/ẩn danh đúng class, không đụng class khác. |
| `IT-RET-HOLD-04` | integration | Bản ghi có legal hold không bị xoá dù quá hạn. |
| `IT-RET-AUDIT-05` | integration | `ivr_audit_log` không bị job xoá; run report được ghi. |
| `IT-RET-RESUME-06` | integration | Kill giữa batch rồi chạy lại → không mất/không xoá trùng, checkpoint đúng. |
| `IT-RET-PII-07` | integration | Sau khi ẩn danh, scan không còn trường nhạy cảm của bản ghi đó. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:**
- [ ] Fail-closed khi thiếu period.
- [ ] Legal hold luôn thắng.
- [ ] Không xoá audit/evidence đã accepted.
- [ ] Không khoá bảng scheduler.

**Reviewer (GitLab MR):** kiểm data class phủ hết bảng ở `02-tables.md` §7 §8; kiểm dry-run là mặc định.

## 10. EVIDENCE EXPECTED
Ghi vào `docs/evidence/W-0064/`: dry-run report, real-run report trên DB test, bảng data class × strategy × period, kết quả 7 nhóm test, PII scan sau ẩn danh.

## 11. FORBIDDEN
- ❌ Tự chọn retention period thay Legal (DF-07 / `OD-V1-11`).
- ❌ Xoá `ivr_audit_log` hoặc evidence đã accepted.
- ❌ Xoá khi thiếu cấu hình (phải fail-closed).
- ❌ Chạy real-run mặc định.

## 12. DEFINITION OF DONE
- [ ] Build + test + lint pass; 7 nhóm test §8 xanh.
- [ ] `specs/database/05-retention-and-privacy.md` phủ hết bảng, kể cả foundation.
- [ ] `P7-2` có thể schedule mà không cần sửa code.
- [ ] Đạt tối đa `TESTS_PASS`. Retention period thật vẫn `OWNER_DECISION_REQUIRED`.

## 13. TRACKER UPDATE (bắt buộc)
- Before: `W-0064` → `IN_PROGRESS` + baseline/prereq.
- During: checkpoint; dependency phát sinh lấy Work ID kế tiếp.
- After: files, commands/results, evidence links, residual gate `DF-07`; chỉ reviewer/owner chuyển `ACCEPTED`.
