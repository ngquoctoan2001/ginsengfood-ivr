# ARCH-03 — Integration Architecture

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p08` · Nguồn: `phase-8/02`,`/17`; `MASTER-04`; D-03/D-04/D-09/DO-03/DO-04/DF-05/DF-06.

## 1. Kiểu tích hợp
| Tích hợp | Producer → Consumer | Kiểu | Transport | Auth |
| --- | --- | --- | --- | --- |
| Task | Order Core → IVR | **sync command** (push) | Internal REST `POST /tasks` (D-03) | service token + allowlist (DF-06) |
| Result callback | IVR → Order Core | **sync + retry** | `POST {orderCore}/v1/orders/{id}/ivr-result-callbacks` (D-04) | service token |
| Blocker check/revalidate | **Order Core** → Ops | sync read | `POST /api/v1/admin/availability/check` (DO-03) | Core service-cred `SellableCheck` |
| Blocker "hold sớm" | Ops → IVR/Core | **async event** | webhook `ops-core.sellable.sku-became-not-sellable.v1` (DO-04) | dedupe `EventId` |
| IVR-required | Sales 3.1 → Order Core | async event | `order.ivr_required_decisioned` (D-09) | — |
| Admin action | Admin → IVR | sync command | Internal admin REST | RBAC `IVR_*` (DF-01) |
| Evidence write | IVR → Evidence Registry | sync | Internal writer | — |
| Event publication (optional) | IVR → consumers | async | outbox pattern tái dùng ops-core (DF-05) | signal only, không thay callback |

## 2. Allowlist & identity (DF-06)
- Chỉ service identity **Order Core** được `POST /tasks` (`X-Source-System=order-core` + token). 
- SIM adapter, Admin UI, kênh khác **không** được tạo task/ghi order.
- Downstream (AI/Facebook/Live/CRM) chỉ consume trạng thái Core-approved; không trigger IVR (phase-8/02).

## 3. Resolver/Guard (MASTER-04)
- Trước dispatch: Eligibility Resolver hợp nhất snapshot (trust/contact/blocker/window/capacity) → Guard PASS/BLOCK/SKIP/REVIEW.
- Khi callback: **Order Core** là Guard cuối (revalidate realtime blocker qua ops — DO-03); IVR chỉ đưa signal.
- Không hardcode; thiếu source → fail-closed (MASTER-04 no-hardcode).

## 4. Error propagation
- IVR API dùng error envelope + stable `code` (xem `api/06-error-codes` §1c). 
- Ops error codes (DO-06) do **Order Core** nhận khi revalidate; Core map sang `CALLBACK_BLOCKED_BY_CORE`/fail-closed.

## 5. Idempotency/correlation xuyên tích hợp
- `Idempotency-Key` cho task/callback/admin/retry; `X-Correlation-Id` giữ nguyên xuyên Order Core → IVR → SIM → Evidence → Core (DF-04/05, MASTER-03).
