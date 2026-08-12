# API-01 — Conventions

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p05` · Nguồn: `phase-8/11` §2,§3; `TECH-01`; `MASTER-03`; DF-01/04/05/06, DO-06.

## 1. Version & base path
- Version `v1`. Không phá vỡ contract khi chưa có compatibility note.
- Base path nội bộ: `/v1/ivr/order-confirmation/*`.
- Boundary: **internal service-to-service** hoặc **admin RBAC**. KHÔNG public consumer API.

## 2. Headers chuẩn
| Header | Bắt buộc | Áp dụng | Ghi chú |
| --- | --- | --- | --- |
| `Authorization` | Có | Tất cả | Service token (DF-06) hoặc admin session (DF-01). Không anonymous. |
| `X-Correlation-Id` | Có | Tất cả | Trace xuyên Order Core → IVR → SIM adapter → Evidence (MASTER-03/DF-05). |
| `Idempotency-Key` | Có với POST rủi ro | tasks, result-callbacks, admin actions, technical-retries | Map vào idempotency store foundation (DF-04). |
| `X-Actor-Id` | Có với admin | Admin API | Backend xác thực lại; không tin client-only. |
| `X-Source-System` | Có với internal | task intake | Chỉ allowlisted (Order Core) — DF-06. |

## 3. Response envelope
- Nếu repo đã có common envelope → dùng lại. 
- **Error envelope** (đồng bộ với ops-core để cross-service nhất quán — DO-06):
```json
{ "error": { "code": "STRING_STABLE", "message": "…", "details": [{ "field": "…", "issue": "…" }], "correlationId": "…" } }
```
- `code` là chuỗi ổn định (không đổi nghĩa giữa version). Xem [06-error-codes.md](06-error-codes.md).

## 4. Auth & phân quyền
- **Task intake**: chỉ service identity **Order Core** trong allowlist (DF-06); `X-Source-System` + service token. SIM adapter **không** có quyền gọi task/ghi order.
- **Admin API**: RBAC permission `IVR_*` server-side (DF-01); mọi action có `reason` + audit.
- **Service credential của Order Core** (để Core gọi ops sellable gate khi revalidate) có perm `SellableCheck`/`RecallHoldView` (DO-03) — cấp bởi foundation (DF-06). *(Đây là cred của Order Core, không phải của IVR.)*

## 5. Idempotency & correlation
- Bắt buộc cho POST rủi ro. Chi tiết [07-idempotency-and-correlation.md](07-idempotency-and-correlation.md).

## 6. Fail-safe (P0)
- Order Core / ops-core / Trust / Evidence / SIM không khả dụng → **không dispatch cuộc gọi thật**; route hold/admin-review (D-06/DO-06; phase-8/02 §10).
- Ops-core `non-2xx / timeout / /health/ready=503` khi revalidate blocker → coi là "không xác thực được" → **fail-closed** (DO-06). *(Order Core là bên gọi ops, không phải IVR.)*
- `REAL_CUSTOMER_CALL_ALLOWED=NO`: mọi POST tạo cuộc gọi chỉ chạy **dry-run/mock** cho tới khi release gate pass (DF-03) và SIM được mua (DT-01).

## 7. Versioning & compatibility
- Thêm field optional = non-breaking. Đổi enum/bắt buộc = breaking → bump version + note.
- OpenAPI 3.1 là nguồn contract chuẩn (DF-02); thay đổi phải qua contract validator ở CI.
