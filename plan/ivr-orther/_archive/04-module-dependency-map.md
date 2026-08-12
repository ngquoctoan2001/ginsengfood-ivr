# 04 — Module Dependency Map

Bản đồ phụ thuộc giữa IVR và các module khác. Chiều tích hợp: `read` / `snapshot` (nhận trong task) / `write` / `callback` / `event`. Priority: P0 (bắt buộc để IVR chạy tối thiểu) / P1 / P2.

## 1. Sơ đồ tổng quan

```text
IVR-Orther (IVR Order Confirmation / phase-8)
  -> Sales Platform / Order Core (module 3 / 3.1)          [liên kết mạnh nhất]
      <- IvrConfirmationTaskV1        (snapshot task: order, contact, program, blockers)
      -> IvrConfirmationResultCallbackV1 (callback: result signal)
      <- (revalidate) Order Core quyết định transition
  -> Operational Core (module 1 / 2)
      <- sale_lock / recall / suppression / availability   (snapshot hoặc read)
  -> Shared System (foundation)
      auth/service-identity, RBAC, audit, evidence registry, correlation/idempotency, notification (chỉ sau Core decision)
  -> Telephony / Internal SIM Gateway
      dial, DTMF capture, call status/disposition, (recording metadata nếu bật)
```

## 2. Sales Platform / Order Core (module 3 / 3.1)

| Dependency | Vì sao IVR cần | Dữ liệu cần lấy | Chiều | Priority | Trạng thái docs | Rủi ro nếu chưa có |
| --- | --- | --- | --- | --- | --- | --- |
| Official Order + order_code | Đối tượng để xác nhận; script đọc `order_code_short` | `order_id`, `order_code_short`, `order_version`, `order_state` | snapshot (task) | P0 | Confirmed (phase-8/04) | Không có task hợp lệ → IVR không chạy |
| Order state machine | Biết state IVR-callable; Core revalidate | tập state hợp lệ | read/callback | P0 | Unclear (tên state chưa chốt) | Callback sai thời điểm |
| Program & attempt policy | GH 5′/2 cuộc (A2@T0+2:30) & 24/7 15′/2 cuộc (A2@T0+7:30) — **D-10** | `program_code`, `max_attempts`, `attempt_schedule` | snapshot (task) | P0 | ✅ D-10 | Sai số attempt/window |
| Customer trust decision | Quyết định trusted skip | `customer_trust_status`, `trusted_skip_allowed`, `risk_flags` | snapshot (task) | P0 | Confirmed (nguồn Trust Resolver) | Gọi nhầm khách trusted |
| Official contact / phone | Số để gọi (privacy-safe) | `official_contact_id`, `phone_ref`, `phone_masked`, `phone_validation_status`, dial token | snapshot (task) | P0 | Confirmed | Không dial được / lộ PII |
| Call script (approved) | Nội dung phát | `call_script_template_id`, `call_script_version`, `allowed_script_variables` | snapshot (task) | P0 | Confirmed | Không phát đúng script |
| Result callback intake | Gửi signal về Core | ack/stale/blocked/review | callback + response | P0 | Confirmed (phase-8/07,/11) | IVR result không tới Core |
| `IVRRequiredDecision` (phase-3.1) | Quyết định order có cần IVR không | `required`, `risk_reasons`, `quota_release_policy` | read/event | P1 | Confirmed contract (phase-3.1/07) | Không biết khi nào tạo task; tension thứ tự order_code |
| Payment/shipping status | (chỉ nếu mở inbound tra cứu) | payment_status, shipping ETA | read | P2 | Out-of-scope phase-8 | Không cần cho outbound confirm |
| **Do-not-call / opt-out / call-restriction** (blocker thương mại) | Không gọi khách đã từ chối/opt-out | do-not-call status | snapshot (task) + read | P0 | ⏳ MỚI — CRM/business-platform (DO-CORR-2); cần Module 3.1 cấp nguồn | Gọi khách opt-out → vi phạm |
| Customer note / call log về hồ sơ | Ghi kết quả gọi vào CRM | note/outcome | write | P2 | ✅ D-14: chỉ audit nội bộ, không ghi CRM (CRM nhận event) | — |

## 3. Operational Core (module 1 / 2)

| Dependency | Vì sao IVR cần | Dữ liệu cần lấy | Chiều | Priority | Trạng thái docs | Rủi ro nếu chưa có |
| --- | --- | --- | --- | --- | --- | --- |
| **Sellable gate** (gộp: sale-lock recall-triggered + recall + quality hold + availability) | Không xác nhận đơn khi dòng hàng không bán được | `SellableStatus` per-line (`Decision`+`RecallHold`/`SaleLock`/`QualityHold`/`StockAvailable`…) qua `POST /api/v1/admin/availability/check` | snapshot per-line (Core fan-out) + Core revalidate realtime | P0 | ✅ DO-01/DO-02/DO-03 | Chốt đơn dòng hàng bị chặn |
| Recall detail (nếu cần) | Ghi evidence khi block | `GET /v1/recall-cases/{id}`, `sale_lock_id`/`recall_case_id` (Guid), evidence_refs | read | P1 | ✅ DO-07 | Thiếu trace evidence |
| ~~Suppression (do-not-call/opt-out)~~ | **KHÔNG thuộc ops-core** → CRM/business-platform | (chuyển §2) | — | P0 | ✅ DO-CORR-2 → xem §2 | Nếu để nhầm ở ops → không có nguồn |
| Availability / stock | Blocker tồn kho | (nằm trong SellableStatus; ops không mở riêng cho IVR) | qua Core/commerce | P1 | ✅ DO-05 | — |
| Public trace / recall lookup | (chỉ nếu inbound: khách hỏi batch an toàn?) | product_name, status VALID/RECALLED | read | P2 | Out-of-scope phase-8 | Không cần cho outbound confirm |
| Product master (tên SKU) | (nếu script cần tên sản phẩm) | public name | read | P2 | ASSUMPTION | Thường lấy qua commerce, không trực tiếp ops |

- ✅ **CHỐT (DO-01..DO-03, DO-CORR-1):** IVR **không** gọi ops trực tiếp. **Order Core** fan-out order → dòng SKU/batch → gọi sellable gate → nhúng snapshot per-line vào task (pre-dispatch) **và** revalidate realtime khi callback. Ops-core không biết `order_id`. Xem [decisions-log.md](decisions-log.md) DO-*, [12](12-ops-core-api-needs-draft.md).
- ✅ **CHỐT (DO-CORR-2):** blocker **do-not-call/opt-out** = **CRM/business-platform (§2)**, KHÔNG phải ops-core.

## 4. Shared System (foundation)

| Dependency | Vì sao cần | Chiều | Priority | Trạng thái docs | Rủi ro |
| --- | --- | --- | --- | --- | --- |
| Auth / service identity allowlist | Chỉ Order Core/service ủy quyền được tạo task; SIM adapter không quyền order | read/enforce | P0 | Confirmed (phase-8/02,/11; TECH-01) | Task giả mạo |
| RBAC (admin permission) | Admin action (pause/resume, sim enable/disable, retry, review) | enforce | P0 | Confirmed (phase-8/08,/11) | Admin bypass P0 |
| Audit log | Ghi mọi action/state | write | P0 | Confirmed (TECH-01, phase-8/08) | Không truy vết |
| Evidence Registry | Ghi evidence cho task/attempt/result/admin; gate release | write | P0 | Confirmed (MASTER-05, phase-8) | Không mở được release gate |
| Correlation / Idempotency store | Chống duplicate, trace xuyên hệ thống | read/write | P0 | Confirmed (MASTER-03, TECH-01) | Duplicate transition |
| Notification owner | Gửi thông báo — **chỉ sau Core decision** | handoff | P1 | Confirmed (phase-8/02) | IVR tự gửi (FAIL) |
| Event bus (AsyncAPI) | Publish signal event (không thay callback) | event | P2 | Confirmed nhưng toolchain chưa duyệt | Không có visibility event |

## 5. Telephony / Internal SIM Gateway

| Dependency | Vì sao cần | Dữ liệu | Chiều | Priority | Trạng thái docs | Rủi ro |
| --- | --- | --- | --- | --- | --- | --- |
| SIM dial | Thực hiện cuộc gọi | dial token/phone | write/command | P0 | Confirmed (phase-8/06) | Không gọi được |
| DTMF capture | Nhận phím 1/0 | `dtmf_key` | read | P0 | Confirmed | Không nhận xác nhận |
| Call status/disposition | Phân biệt no-answer/busy/technical | raw_call_status | read | P0 | Confirmed | Nhầm technical ↔ no-answer (FAIL) |
| SIM health | Enable/disable an toàn | health status | read | P1 | Confirmed | Gọi trên SIM lỗi |
| Recording metadata | (nếu bật recording) | recording ref | read | P2 | Owner Decision + RISK privacy | Vi phạm privacy |
| Inbound webhook | (chỉ nếu chuyển provider ngoài) | call events | webhook | P2 | NEED_CONFIRMATION (không mặc định) | N/A với internal SIM |

## 6. Ghi chú ưu tiên tổng thể

- P0 để có luồng outbound confirm tối thiểu: Order Core (task+callback), foundation (auth/audit/evidence/idempotency), SIM gateway (dial/DTMF/status), ops blocker (sale-lock/recall/suppression).
- P1/P2: `IVRRequiredDecision` tích hợp chặt, availability realtime, notification handoff, event bus, recording, và toàn bộ nhánh inbound (nếu owner mở scope).
