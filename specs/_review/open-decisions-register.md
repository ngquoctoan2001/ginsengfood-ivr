# REVIEW — Open Decisions Register

Trạng thái: `REVIEW` · Sinh bởi: `p14` · Gom mọi quyết định/việc còn mở. Nguồn: `plan/ivr-orther/decisions-log.md` (Còn treo), `data/04-missing-data`, `testing/08`.
Không đóng bằng suy luận — chỉ đóng khi owner sign-off (ghi vào `decisions-log`/`specs/decisions/`).

## P0 — chặn gọi khách thật
| ID | Nội dung | Owner | Chặn gì | Trạng thái |
| --- | --- | --- | --- | --- |
| ~~OQ-01 / Q-C1~~ ✅ | do-not-call/opt-out — **ĐÃ CÓ NGUỒN (DC-01)**: `crm-ads-eligibility` (PHONE_CALL); check `eligible` dùng được ngay → **hết chặn P0**. Còn lại = **build P1** (extend response + Core wiring `call_restriction`) = IR-CRM-01 | CRM/Customer Identity | ✅ RESOLVED (nguồn); build P1 |
| DT-01 | SIM gateway **protocol** (mua SIM) | Infra/procurement | gọi thật (đang MOCK) | ⏳ mua sắm |
| DF-03 | Release **sign-off** + pilot scope | Release Owner (bạn) + security/privacy | mở `REAL_CUSTOMER_CALL_ALLOWED` | ⏳ khi tới gate |

## P1 — cần trước integration/production đầy đủ
| ID | Nội dung | Owner | Chặn gì | Trạng thái |
| --- | --- | --- | --- | --- |
| ~~DG-03~~ ✅ | **Order-state enum + transition** → **RESOLVED (DS-01..05)** từ source. Thật: `order_status` enum + **IVR-callable = CONFIRMING+COD**; transition confirm→CONFIRMED/cancel→CANCELLED/timeout→EXPIRED; no-answer/technical không transition. Đã đồng bộ specs/seed. | Order Core | ✅ đã trả lời |
| IR-SALES-OC1 (DS-04) | Core expose `order_version` + callback nhận `order_version_seen_by_ivr` (race-guard) — nay GAP | Order Core | race-guard chưa dùng được | ⏳ build P1 |
| IR-SALES-OC2 (DS-03) | Richer callback codes (nay chỉ `422`) | Order Core | contract callback | ⏳ build P2 |
| IR-SALES-OC3 (DS-02) | Explicit no-answer/technical transition (nay order chờ `timeout→EXPIRED`, không hủy chủ động) | Order Core | notify/hủy sớm khi no-answer | ⏳ build P2 |
| DO-02 | `captured_at`/ETag trên SellableStatus+lock | Ops-Core | độ tươi snapshot | ⏳ ops bổ sung |
| DT-02 | Re-verify disposition với telco thật | Infra | chính xác no-answer vs technical | ⏳ khi có SIM |
| DT-04 | Số SIM pool launch thật | Infra/procurement | capacity thật | ⏳ mua sắm |
| DT-06 | Caller-ID/brandname | Telco | trải nghiệm/anti-spam | ⏳ mua sắm |
| DF-07 | Retention duration từng loại | Owner + Legal | privacy review | ⏳ Legal |
| DT-05 | Recording bật/tắt + consent | Owner + Legal | (nếu bật) | ✅ OFF; bật cần legal |
| OD-10 | Technical retry count/backoff | IVR Owner | config retry | ⏳ |
| DC-05 (QC5/OD-16) | **Implement event sau Core decision** (`ORDER_CONFIRMED/CANCELLED/EXPIRED`; transition `ivr-confirm/ivr-reject/timeout`) + CRM notification template (nay chưa publish; notification no-op) | Order Core + CRM | thông báo sau gọi | ⏳ build |
| DC-06 (QC6/Q-F3) | **Build CustomerTrustResolver** (`trusted_skip_allowed/risk_flags`) để bật trusted-skip (nay chưa có → default require-IVR) | CRM/business-platform | tối ưu skip (không chặn) | ⏳ (out-of-scope P3.2) |
| IR-CRM-01 (DC-01) | **Extend `crm-ads-eligibility` response** (`do_not_call/opt_out_scope/reason/effective_at`) + Core wiring `call_restriction` | CRM/Customer Identity | rich do-not-call (nay `eligible` đã đủ block cơ bản) | ⏳ build |
| Q-F2 | Bật KEY_9 "gặp CSKH" không | Ops/CSKH | (hiện NOT_ENABLED) | ⏳ |

## Chuẩn hóa nội bộ (không chặn)
| ID | Nội dung | Trạng thái |
| --- | --- | --- |
| OD-DR-02 | ID scheme chính (`IVR-*` vs `M8-*`) + bảng ánh xạ | ⏳ dùng kép (testing/09) |
| OD-DR-05 | Invalid phone → cancel hay admin review | ✅ DT-02 default + policy |
| Terminology | thống nhất `24/7` vs `24-7` | cosmetic |
| structure | tách `non-functional/`, `modules/` riêng | tùy chọn vòng sau |

## Nguyên tắc
- Mọi `⏳` phải có owner + hạn khi đưa vào sprint. `REAL_CUSTOMER_CALL_ALLOWED=NO` tới khi P0 đóng + release gate pass (DF-03).
- p14 chạy lại sau mỗi vòng cập nhật để giữ register này chuẩn.

## Trạng thái review round 2 (2026-07-02)
- ✅ Đã kiểm `prompt/*` + `api/06 §1c` — không phát sinh open decision mới.
- **Cập nhật CRM (DC-01..06):** Q-C1 **đã có nguồn** (DC-01) → **P0 giảm còn 2**: DT-01 (mua SIM), DF-03 (sign-off). Thêm build items DC-05 (events), DC-06 (trust resolver), IR-CRM-01 (extend response) — đều P1/P2, không chặn gọi thật.
- Trust-skip hiện **không khả dụng** (DC-06) → IVR default require-IVR (không skip) — nhất quán D-12 fail-safe.
- ✅ TODO nhẹ đã xử lý: OpenAPI `ErrorEnvelope.code` đã dùng enum 15 stable code (xem normalization-report R2.1/R4.2).

## Trạng thái review round 4 (2026-07-06)
- ✅ Q-C1 và DG-03 vẫn ở trạng thái resolved; không còn `PENDING` nào dùng để mô tả hai mục này trong specs current.
- ✅ Task/callback race-guard đã đồng bộ current/target: current task dùng `order_state=CONFIRMING` + `payment_method_snapshot=COD` và không require `order_version`; current callback dùng `200`/`422` + state/COD/sellable recheck; IR-SALES-OC1 mới thêm `order_version`/`order_version_seen_by_ivr`; IR-SALES-OC2 mới thêm semantic `CALLBACK_*`.
- P0 chưa production-ready vì DT-01 + DF-03 vẫn mở; đây là gate vận hành, không phải conflict spec.
