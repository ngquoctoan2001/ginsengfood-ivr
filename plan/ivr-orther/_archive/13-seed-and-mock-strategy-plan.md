# 13 — Seed & Mock Strategy Plan

Kế hoạch seed/mock cho giai đoạn chưa có API thật. **Giai đoạn này chỉ lập kế hoạch — KHÔNG sinh JSON thật** (seed thật sinh ở p10).

## 1. Nguyên tắc

- Seed chỉ chạy **non-production**; SIM channel ở trạng thái disabled; recording OFF; `REAL_CUSTOMER_CALL_ALLOWED=NO`.
- **Không PII thật**: dùng dải số điện thoại test, dùng `phone_masked`/`phone_ref`/dial token giả.
- Mỗi seed scenario map tới ít nhất 1 smoke case của phase-8/09 (IVR-SMK-*).
- Phân tách rõ 3 lớp: **IVR-owned**, **giả lập sales**, **giả lập ops**.

## 2. Domain cần mock

| Domain | Lớp | Mục đích |
| --- | --- | --- |
| customer (projection) | giả lập sales | nhận diện/hiển thị masked |
| order (official, program) | giả lập sales | đối tượng xác nhận |
| product | giả lập ops | tên/sku cho script (nếu cần) |
| inventory + sale-lock/recall | giả lập ops | test blocker |
| IVR task (`IvrConfirmationTaskV1`) | IVR-owned input | intake/validation |
| IVR menu/script | IVR-owned | phím 1/0, biến được phép |
| call scenario | IVR-owned | mô phỏng SIM/DTMF |
| agent (admin/ops) | IVR-owned | RBAC action |
| integration status | IVR-owned | fail-safe (sales/ops/SIM up|down) |

## 3. Tình huống cần seed (map smoke)

| Tình huống | Lớp | Kỳ vọng | Map smoke |
| --- | --- | --- | --- |
| Khách có 1 đơn đủ điều kiện IVR | sales | task accepted → call job | IVR-SMK confirm |
| Khách có nhiều đơn | sales | mỗi order 1 task riêng | intake nhiều task |
| Khách không tồn tại / contact invalid | sales | `TASK_REJECTED_CONTACT_INVALID` | invalid phone |
| Đơn đang giao / đã giao | sales | (ngoài scope confirm) → không tạo task hoặc reject state | state-not-callable |
| Đơn bị hủy | sales | callback stale / no-op | stale callback |
| Đơn cần xác nhận (callable) | sales | happy path | confirm/cancel |
| Sản phẩm còn hàng | ops | không block | dispatch |
| Sản phẩm hết hàng / availability fail | ops | Core block khi revalidate | operational block |
| Sale-lock ACTIVE | ops | `TASK_BLOCKED_OPERATIONAL` / Core block sau phím 1 | race Sale Lock |
| Recall ACTIVE | ops | block | recall block |
| Sales API down | integration | không tạo/không tiếp task; admin review | fail-safe Order Core down |
| Ops-core API down | integration | không dispatch | fail-safe blocker check down |
| Webhook/callback duplicate | IVR | idempotent, trả ack cũ | duplicate callback |
| Missed call / no-answer | call scenario | attempt → `NO_ANSWER_ATTEMPT`/`FINAL` theo max=2 | GH 5′ (A2@T0+2:30), 24/7 15′ (A2@T0+7:30) — D-10 |
| Callback request (admin) | agent | technical retry / review | admin retry |
| Khách yêu cầu gặp nhân viên | call scenario | `IVR_CUSTOMER_NEEDS_SUPPORT` (nếu future key) → review | support handoff (nếu bật) |
| Technical exception (SIM/DTMF error) | call scenario | không count attempt; retry/review | technical≠no-answer |
| Trusted customer skip | sales | `TASK_SKIPPED_TRUSTED_CUSTOMER` | trusted skip |
| Confirmation window expired | call scenario | `IVR_CONFIRMATION_WINDOW_EXPIRED` | window expired |

## 4. Seed thuộc IVR

- `ivr-tasks.sample.json` (task hợp lệ + các biến thể reject/block), `ivr-menu.sample.json` (script/phím), `call-scenarios.sample.json` (kết quả SIM/DTMF), `agents.sample.json` (actor+permission), `integration-status.sample.json`.

## 5. Seed giả lập sales platform

- `customers.sample.json` (projection: customer_ref, trust_status, phone_masked/ref, program), `orders.sample.json` (official order, order_state, order_version, program_code, attempt_policy).
- Mock endpoint: task push (API-1) + result callback intake (API-2) trả các response semantic.

## 6. Seed giả lập ops-core

- `products.sample.json`, `inventory.sample.json` (còn/hết hàng, sale-lock/recall ACTIVE/NONE, suppression). Mock endpoint blocker-status (API-O1) trả state realtime giả.

## 7. Khi có API thật thì bỏ mock thế nào

- Đặt cờ cấu hình `INTEGRATION_MODE = MOCK | REAL` (per dependency: sales/ops/sim).
- Mỗi mock nằm sau một adapter/port có contract giống API thật → thay MOCK→REAL không đổi core logic.
- `seed/README.md` liệt kê từng mock, API thật tương ứng (theo [11](11-sales-platform-api-needs-draft.md), [12](12-ops-core-api-needs-draft.md)), và checklist gỡ.
- Ưu tiên gỡ sớm: API-1/API-2 (task/callback), API-O1 (blocker) — vì P0.

## 8. Prompt sinh seed thật

- **p10** (`prompts/p10-generate-seed-data.md`) sinh toàn bộ `seed/*`. Dependency: database (p07) + data (p06). Không chạy trước khi có DB design.
