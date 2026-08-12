# IVR Seed / Mock Data (NON-PRODUCTION ONLY)

> **Target V1 canonical fixture (2026-08-12):** use `sales-target-v1.sample.json`. Các JSON cũ là legacy/current-compat scenarios với giả định COD-only/D-10 cũ; không dùng để sinh Target V1 domain/DTO.

Trạng thái: `SEED_MOCK` · Sinh bởi: `plan/ivr-orther/prompts/p10-generate-seed-data.md`
Nguồn: `plan/ivr-orther/13-seed-and-mock-strategy-plan.md`, `specs/srs/database/*`, `data/*`, `workflows/*`; smoke `phase-8/09`.

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
| `ivr-tasks.sample.json` | **IVR-owned** | `IvrConfirmationTaskV1` mẫu (nhúng sellable_status snapshot) |
| `call-scenarios.sample.json` | **IVR-owned** | kịch bản SIM/DTMF → result mong đợi |
| `ivr-menu.sample.json` | **IVR-owned** | call script + phím 1/0 |
| `agents.sample.json` | **IVR-owned** | admin/ops actor + permission |
| `integration-status.sample.json` | **IVR-owned** | up/down của Order Core/ops/SIM/CRM (test fail-safe) |

## Cách dùng
1. Chạy IVR với `EXECUTION_MODE=MOCK`, `SALES_PROVIDER=FAKE_TARGET_V1` và `SIM_PROVIDER=MOCK`.
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

## Coverage smoke (map `phase-8/09` IVR-SMK / docx M8-P0)
Xem `call-scenarios.sample.json.scenarios[].smoke_ref`. Bao phủ: confirm, cancel, no-answer(2 attempts), window-expired, invalid-phone, technical≠no-answer, sale-lock/recall block, race, trusted-skip, duplicate callback, capacity, needs-support (KEY_9 not-enabled), sales/ops down (fail-closed).
