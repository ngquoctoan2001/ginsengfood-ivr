# FR — Eligibility & Blockers

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p03`
Nguồn: `phase-8/03` (điều kiện gọi/niềm tin/contact), `docx` §5 (Entry Gate), §7 (Eligibility Resolver); `phase-2/06` (sale-lock/recall).

**Actor:** IVR Eligibility Resolver (đọc snapshot từ Order Core, Trust, Official Contact, Operational Core, Program Policy).
**Precondition:** Task đã intake.
**Trigger:** Trước khi tạo/ dispatch CallJob (và Order Core re-check khi callback).
**Postcondition:** `ELIGIBLE` → cho phép scheduler; ngược lại reject/hold/block/skip.

## FR
| ID | Yêu cầu | Nguồn | Acceptance hint |
| --- | --- | --- | --- |
| FR-IVR-ELIG-001 | Chỉ Official Order ở **order_state được phép gọi** (chưa cancel/expire/delivered/verified) mới PASS | docx §5,§7 | State không callable → reject/stale |
| FR-IVR-ELIG-002 | Bắt buộc có `order_code` | docx §7 | Không order_code → không gọi |
| FR-IVR-ELIG-003 | `program_code` xác định window (Giờ Vàng vs 24/7) | docx §7,§8 | Program không rõ → owner review/no dispatch |
| FR-IVR-ELIG-004 | Contact hợp lệ: `phone_ref`/dial_token, `phone_validation_status=PASS`; UI/log chỉ `phone_masked` | phase-8/03; docx §7,§17 | Phone invalid → `IVR_INVALID_PHONE_FINAL`/admin review (OD-DR-05) |
| FR-IVR-ELIG-005 | **Trusted skip**: skip IVR chỉ khi resolver = TRUSTED + `trusted_skip_allowed` + no risk/blocker; **không hardcode**. ⚠️ **DC-06: resolver CHƯA có → hiện default LUÔN gọi IVR (không skip)** cho tới khi CRM/business-platform build resolver | phase-8/03; docx §7, M8-OD-002; DC-06 | Trusted+no-risk → `TASK_SKIPPED_TRUSTED_CUSTOMER` (khi resolver có); nay: require IVR |
| FR-IVR-ELIG-006 | **Blocker check** (2 nguồn): (a) **ops-core** qua sellable gate → `SellableStatus.Decision ∈ {NOT_SELLABLE, BLOCKED}` hoặc cờ `RecallHold`/`SaleLock`/`QualityHold` → không dispatch; (b) **CRM/business-platform** → do-not-call/opt-out/call-restriction → không dispatch | phase-8/00 FR-004, /02; docx §5 Block Gate; **DO-01/DO-CORR-2** | Blocker active → `TASK_BLOCKED_OPERATIONAL` (P0) |
| FR-IVR-ELIG-007 | Kiểm `quote_expiry`/`order_deadline` — không gọi nếu đã hết hiệu lực | docx §7 | Hết hạn → không gọi |
| FR-IVR-ELIG-008 | Kiểm capacity: không nhận call job vượt capacity nếu chắc chắn miss deadline | docx §7 Capacity Gate | Vượt capacity → capacity incident + alert |
| FR-IVR-ELIG-009 | Resolver **fail-safe**: nếu source (trust/contact/blocker) không khả dụng → không dispatch, route review theo policy an toàn | phase-8/02 §6,§10 | Source down → no dispatch |
| FR-IVR-ELIG-010 | Không tự kéo dài window, không tự bỏ qua recall/sale lock | docx §7 | Vi phạm → FAIL |

## Blocker sources (✅ đã khóa — xem `plan/ivr-orther/decisions-log.md` DO-01..DO-07 + D-06)
- **Sale Lock (recall-triggered) / Recall / Quality hold / Availability: ops-core** qua **sellable gate** `POST /api/v1/admin/availability/check` → `SellableStatus` (Decision + `RecallHold`/`SaleLock`/`BatchReleased`/`StockAvailable`/`TraceReady`/`QualityHold`). **Ops-core không biết `order_id`** ⇒ **Order Core fan-out** order → từng dòng SKU/batch, gọi check, **nhúng mảng SellableStatus per-line** vào task (snapshot, có `captured_at`). **DO-01/DO-02/DO-CORR-1/DO-CORR-3.**
  - ⚠️ Lưu ý: ops-core hôm nay **sale-lock = do recall** (chưa có sale-lock thương mại độc lập).
- **Revalidate realtime khi callback: Order Core gọi ops-core** (IVR không gọi trực tiếp); Core dùng service-cred `SellableCheck`/`RecallHoldView`; non-2xx/timeout/`/health/ready`=503 → **fail-closed** (không dispatch/không confirm). **D-06/DO-03/DO-06.**
- **Do-not-call / opt-out / call-restriction: business-platform Customer Identity** (`consent_suppression_markers` theo `channel_type+contact_hash`) — **DC-01**. Endpoint `POST /api/v1/admin/customer-identity/crm-ads-eligibility` (`channelType=PHONE_CALL`, `category=TRANSACTIONAL`). **Order Core gọi** & nhúng `call_restriction` vào task; fail-closed nếu lỗi. ⚠️ response cần bổ sung `do_not_call/opt_out_scope/reason/effective_at`. Channel-specific: opt-out SMS **không** chặn voice (DC-02). IVR confirmation **không** áp CRM marketing quiet/cap (DC-03).
- **Trust decision / risk flags: business-platform** — ⚠️ **`CustomerTrustResolver`/`trusted_skip_allowed` CHƯA có** (DC-06, out-of-scope P3.2). ⇒ **trusted-skip hiện KHÔNG khả dụng → default LUÔN gọi IVR** (D-12 fail-safe: resolver unavailable → require IVR). Khi có resolver mới bật skip.
- Optional "hold sớm": webhook `ops-core.sellable.sku-became-not-sellable.v1` (dedupe `EventId`) — **DO-04**; không thay revalidate.
