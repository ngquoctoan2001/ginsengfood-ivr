# API-07 — Idempotency & Correlation

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p05` · Nguồn: `phase-8/04` §13, `/07` §14; `MASTER-03`; DF-04 (idempotency store), DF-05 (correlation/outbox).

## 1. Idempotency
- Dùng **idempotency store của foundation** (DF-04), không tự chế.
- Bắt buộc cho POST rủi ro: **task intake, result-callback (outbound tới Core), admin action, technical-retry**.
- Scope key: theo endpoint + actor + business id (task_id/callback_id).

| Tình huống | Hành vi |
| --- | --- |
| Same key, same payload | Trả kết quả cũ (no-op) |
| Same key, khác payload | `409 Conflict` |
| Same `task_id`, khác key | Conflict trừ khi exact duplicate mapped |
| Retry sau transient IVR error | Safe retry cùng key |
| Retry sau khi task rejected | Trả cùng rejection; không tạo job sau trừ task/version mới |
| Duplicate callback | Trả ack cũ; không tạo transition mới |

- Callback retry (D-04): chỉ khi timeout/5xx/`TECHNICAL_RETRY_ALLOWED`; **cùng idempotency key**; không tạo result mới, không tăng customer attempt, không đổi result status, không bypass stale guard, bounded (OD-10 chốt count/backoff).

## 2. Correlation
- `X-Correlation-Id` bắt buộc mọi request; **giữ nguyên** xuyên chuỗi: Order Core → IVR task intake → eligibility → scheduler → SIM adapter → result normalizer → callback → Order Core → Evidence (MASTER-03/DF-05).
- Với webhook consume từ ops-core (DO-04): dedupe theo `EventId` (header `X-Idempotency-Key`), giữ `X-Correlation-Id`.

## 3. Trace linkage (MASTER-03 / DO-07)
- Mỗi bước ghi evidence/audit kèm: `task_id`, `order_id`, `correlation_id`, `idempotency_key`, `evidence_ref`, `audit_ref`; `order_version` chỉ ghi khi IR-SALES-OC1 expose.
- Khi block bởi ops: ghi thêm `sale_lock_id`/`recall_case_id` (Guid) + `scope` (DO-07).

## 4. Race guard
- **Target V1:** callback bắt buộc `order_version_seen_by_ivr`; Sales trả `409 REJECTED_STALE` khi version/state không còn hợp lệ. Same key+same payload → `DUPLICATE_ACCEPTED`; same key+different payload → `409 IDEMPOTENCY_CONFLICT`.
- **Current compatibility:** Golden Hour adapter may omit version and receive coarse 200/422 behavior, but it stays isolated by provider/record type and never counts as Target evidence.
