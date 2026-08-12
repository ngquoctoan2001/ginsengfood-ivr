# PROMPT P1-2 — Database Migrations (PostgreSQL / EF Core)

## 0. Meta
| | |
| --- | --- |
| **ID** | `P1-2` |
| **Phase** | 1 — Contracts & Data |
| **Prereq (blockedBy)** | `P0-3` |
| **Governance flag** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · EF Core · PostgreSQL |

## 1. ROLE
Bạn là **Database Engineer (.NET/Postgres)**. Bạn hiện thực schema `ivr_*` từ đặc tả DB, viết EF Core migration reversible, cài **CHECK constraint** ép policy (D-10), index cho scheduler, và cơ chế retention. Bạn coi DB là hợp đồng dữ liệu — constraint ở DB, không chỉ ở app.

## 2. CONTEXT
IVR sở hữu DB Postgres riêng (không dùng chung Order Core — DTS-01). Schema đã đặc tả (11 bảng gồm `ivr_raw_call_event`). Prompt này tạo migration đầu tiên: bảng, quan hệ, enum/status, constraint policy, index, và hook retention. Đây là nền cho intake/scheduler/callback (Phase 2) và các store foundation (P0-3).

## 3. SOURCE SPECS (đọc trước)
- `specs/database/00-index.md`, `specs/database/01-erd.md`, `specs/database/02-tables.md`, `specs/database/03-enums-and-status.md`, `specs/database/04-indexes.md`, `specs/database/05-retention-and-privacy.md`, `specs/database/06-migration-plan.md`
- `specs/data/01-data-ownership.md`, `specs/data/05-pii-policy.md`
- `plan/ivr-orther/decisions-log.md` §D-10 (attempt policy), §DS-01 (CONFIRMING+COD), §D-05 (PII/token), §DF-04 (audit/idempotency), §DF-07 (retention)

## 4. DECISIONS & CONSTRAINTS
- **D-10 (CHECK bắt buộc):** `max_attempts=2` cả hai program; GH `window=300/spacing=150`, 24-7 `window=900/spacing=450`; `attempt_number ≤ 2`; `is_counted=false` khi technical.
- **DS-01:** snapshot order lưu `order_status` (enum thật) + `payment_method_snapshot` — CHECK/nhắc chỉ nhận `CONFIRMING`+`COD` ở tầng intake (DB lưu snapshot, không tự transition).
- **DF-04:** bảng `ivr_audit_log` (append-only), `ivr_idempotency_keys` (unique key) — nối từ P0-3.
- **D-05 / DF-07:** KHÔNG cột lưu raw phone/recording; `dial_token` (nếu lưu) TTL ≤ window; cột PII mask/ref; retention job xoá theo policy.
- **DS-04:** có thể lưu `order_version` snapshot (từ task) nhưng biết Core chưa expose → nullable, không dùng làm race-guard cứng (target).

## 5. INPUTS / DEPENDENCIES
- Postgres (compose P0-1); `IvrDbContext` (P0-1).
- Entity foundation (P0-3): audit/idempotency/evidence.
- `seed/*.json` để đối chiếu field khi thiết kế cột.

## 6. BUILD STEPS
1. Định nghĩa EF Core entity + `IEntityTypeConfiguration` cho **11 bảng** theo `database/02-tables.md`, tối thiểu:
   - `ivr_confirmation_tasks` (task snapshot: order_id/order_code, program_code, max_attempts, window/spacing, order_status_snapshot, payment_method_snapshot, order_version_snapshot?, expires_at, sellable snapshot ref, script ref, evidence/privacy version…).
   - `ivr_call_jobs`, `ivr_call_attempts`, `ivr_raw_call_event`, `ivr_technical_exceptions`, `ivr_results`, `ivr_callbacks`, `ivr_admin_actions`, `ivr_audit_log`, `ivr_idempotency_keys`, `ivr_evidence`.
2. **Enum/status** (`database/03`): map sang Postgres enum hoặc check-constrained varchar (chọn nhất quán); result taxonomy, program_code, intake decision, call_status/stop_reason.
3. **CHECK constraints (D-10)**: `max_attempts = 2`; `(program_code='GOLDEN_HOUR' AND window=300 AND spacing=150) OR (program_code='TWENTY_FOUR_SEVEN' AND window=900 AND spacing=450)`; `attempt_number BETWEEN 1 AND 2`; technical → `is_counted_customer_attempt=false`.
4. **Unique**: idempotency_key; (task_id) natural key; callback_id; (job_id, attempt_number).
5. **Index (database/04)**: scheduler-deadline (next_attempt_at/expires_at), lookup theo order_id/correlation_id, status partial index cho queue.
6. **Migration**: tạo migration `InitialIvrSchema`; test **up + down** (reversible) trên Postgres thật (Testcontainers).
7. **Retention hook** (`database/05`, DF-07): cột `created_at`/`retention_class`; viết interface `IRetentionJob` + SQL/job xoá dữ liệu quá hạn theo class (raw_call_event/dtmf ngắn hạn; audit dài hạn) — job chạy thật ở P7 (CronJob), ở đây định nghĩa + unit test logic chọn bản ghi hết hạn.
8. Cập nhật `/health/ready` (P0-1) để check migration applied + DB reachable → 503 nếu chưa (fail-closed).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Infrastructure/Entities/**`, `Configurations/**` | Entity + fluent config |
| `db/migrations/**` (EF Core) | `InitialIvrSchema` up/down |
| `src/Ivr.Infrastructure/Retention/IRetentionJob.cs` + impl | Chọn + xoá theo retention class |
| `tests/Ivr.IntegrationTests/Database/**` | Migration + constraint tests (Testcontainers) |

**Chuẩn output:** constraint ở DB (không chỉ app); tên bảng/cột snake_case khớp specs; không cột PII thô.

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `IT-DB-MIG-01` | integration | migration up + down chạy sạch trên Postgres (Testcontainers). |
| `IT-DB-D10-02` | integration | insert `max_attempts=3` → CHECK vi phạm (reject); GH window≠300 → reject (D-10). |
| `IT-DB-IDEMP-03` | integration | trùng idempotency_key → unique violation. |
| `IT-DB-IDX-04` | integration | query scheduler-deadline dùng index (EXPLAIN) — không seq scan bảng lớn. |
| `UT-DB-RET-05` | unit | `IRetentionJob` chọn đúng bản ghi quá hạn theo class; không chọn còn hạn. |

Trace: `specs/testing/03-integration-test-plan.md`, `specs/database/06-migration-plan.md`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:**
- [ ] 5 test §8 xanh; migration reversible.
- [ ] CHECK D-10 thật sự chặn ở DB (IT-DB-D10-02 chứng minh).
- [ ] Không cột lưu raw phone/recording.

**Reviewer:** enum/status khớp `database/03`; index đúng truy vấn scheduler; retention class map đúng `database/05`; `/health/ready` fail-closed khi DB/migration chưa sẵn.

## 10. EVIDENCE EXPECTED
Migration up/down log, constraint-violation samples (D-10, idempotency), EXPLAIN index proof, retention selection test, `/health/ready`=503 khi DB down.

## 11. FORBIDDEN
- ❌ Cột lưu raw phone/dial_token→số/recording (D-05).
- ❌ Chung DB/schema với Order Core (DTS-01).
- ❌ Migration không reversible.
- ❌ Ép policy chỉ ở app mà bỏ CHECK ở DB (D-10).

## 12. DEFINITION OF DONE
- [ ] 11 bảng + enum + CHECK D-10 + index + retention hook; migration up/down xanh.
- [ ] 5 test §8 xanh trong CI; `/health/ready` nối DB.
- [ ] Evidence §10 đủ; không vi phạm Forbidden.
