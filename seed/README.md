# IVR Seed / Mock Data (NON-PRODUCTION ONLY)

> **Target V1 canonical fixture (2026-08-12):** use `sales-target-v1.sample.json`. Các JSON cũ là legacy/current-compat scenarios với giả định COD-only/D-10 cũ; không dùng để sinh Target V1 domain/DTO.

Trạng thái: `SEED_MOCK` · Sinh bởi: `plan/ivr-orther/_archive/prompts/p10-generate-seed-data.md` (lịch sử)
Nguồn: `specs/database/*`, `specs/data/*`, `specs/workflows/*`; smoke `phase-8/09`. (Chiến lược seed gốc nằm ở `plan/ivr-orther/_archive/13-seed-and-mock-strategy-plan.md` — lịch sử, không phải authority.)

## ⚠️ Cảnh báo
- **CHỈ dùng non-production.** KHÔNG seed vào production.
- **KHÔNG PII thật:** phone dùng dải test `84xxxxx…`; chỉ `phone_ref`/`phone_masked`/`dial_token` giả — **không** số thật, không lưu raw phone.
- **Recording OFF**, **SIM `adapter_mode=MOCK`**, `enabled=false`, `REAL_CUSTOMER_CALL_ALLOWED=NO` (DT-01/DT-05).

## Phân lớp file
| File | Lớp | Giả lập cái gì |
| --- | --- | --- |
| `customers.sample.json` | sim-sales | projection khách từ Order Core/CRM (trust, contact) |
| `orders.sample.json` | sim-sales | Official Order (order_code, state, program) |
| `products.sample.json` | sim-ops | SKU/batch public name |
| `inventory.sample.json` | sim-ops | `SellableStatus` per sku/batch (sale-lock/recall/stock) |
| `ivr-tasks.sample.json` | **LEGACY** | Task shape **trước Target V1** — KHÔNG phải `IvrConfirmationTaskV1`, KHÔNG đẩy vào `POST /tasks` (thiếu 12 required field, thừa 11 field bị `additionalProperties:false` từ chối). Chỉ giữ để đọc lịch sử. |
| `call-scenarios.sample.json` | **IVR-owned** | kịch bản SIM/DTMF → result mong đợi; `task_ref` trỏ vào `sales-target-v1.sample.json` |
| `ivr-menu.sample.json` | **IVR-owned** | call script + phím 1/0 |
| `agents.sample.json` | **IVR-owned** | admin/ops actor + permission |
| `integration-status.sample.json` | **IVR-owned** | up/down của Order Core/ops/SIM/CRM (test fail-safe) |

## Cách dùng
1. Chạy IVR với `IVR_EXECUTION_MODE=MOCK` (canonical key, governance §6), `SALES_PROVIDER=FAKE_TARGET_V1` và `SIM_PROVIDER=MOCK`.
2. Nạp `customers/orders/products/inventory` vào mock của Order Core/ops.
3. Fake Sales producer đẩy `sales-target-v1.sample.json` vào `POST /tasks`.
4. Mock SIM adapter đọc `call-scenarios` để phát `raw_call_status` mô phỏng → Result Normalizer (DT-02).
5. `integration-status` bật/tắt dependency để test fail-closed.

## Cách gỡ mock khi có API thật (theo integration-requirements)
| Seed | Thay bằng | Điều kiện |
| --- | --- | --- |
| `orders`/`customers` | Order Core task push thật (IR-SALES-01) | có endpoint task |
| `inventory` (sellable) | ops `availability/check` thật (IR-OPS-01/02) | có `captured_at` |
| `call-scenarios` (MOCK adapter) | SIM gateway thật (IR-TEL-01) | mua SIM + release gate |
| `integration-status` | health thật (`/health/ready`, IR-OPS-04) | — |
| `call_restriction` trong task | CRM do-not-call (DC-01/Q-C1 resolved; IR-CRM-01 rich fields P1) | có nguồn CRM |

Ưu tiên gỡ sớm: task/callback (Sales), sellable gate (Ops) — vì P0.

## Lớp test của negative fixture

`sales-target-v1.sample.json` tách negative theo **lớp bị chặn**, vì một payload sai schema không bao giờ tới được domain layer:

| Mảng | Lớp | Kết quả mong đợi |
| --- | --- | --- |
| `schema_negative` | OpenAPI schema | HTTP `422` + `ErrorEnvelope.code` |
| `domain_negative` | domain/policy (payload schema-VALID) | HTTP `200` + `IvrTaskIntakeResult.decision`, hoặc `422 IVR_PII_POLICY_VIOLATION` / `409 IVR_IDEMPOTENCY_CONFLICT` |
| `callback_scenarios` | Sales semantic ACK | `CallbackAck200`/`CallbackAck409` + `delivery_status` nội bộ |

`NEG-DOMAIN-PII-01` là fixture quan trọng nhất của nhóm privacy: `delivery_area_short` **hợp lệ theo schema** (không có chữ số) nhưng vẫn là địa chỉ đường phố, nên nó thực sự kiểm tra semantic detector `FR-IVR-INTAKE-005` — khác với `NEG-SCHEMA-PII-01` chỉ kiểm tra `additionalProperties:false`.

Replay identical của intake phải trả lại **chính decision/job ID ban đầu** (`TASK_ACCEPTED_DRY_RUN_ONLY` trong MOCK), không phát minh decision `...DUPLICATE_REPLAY`. Fixture còn khóa missing flag/evidence, stale window, script chưa approved và 8 concurrent identical replay hội tụ về đúng 1 task/job/outbox.

## Coverage smoke (map `phase-8/09` IVR-SMK / docx M8-P0)
Xem `call-scenarios.sample.json.scenarios[].smoke_ref`. Bao phủ: confirm, cancel, no-answer(2 attempts), window-expired, invalid-phone, technical≠no-answer, sale-lock/recall block, race, trusted-skip, duplicate callback, capacity, needs-support (KEY_9 not-enabled), sales/ops down (fail-closed).
