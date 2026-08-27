# IVR-04 - Order Core -> IVR Task Contract

> **HISTORICAL_BASELINE / SUPERSEDED OVERLAY — 2026-08-27:** Theo `OD-18`, task hợp lệ là chỉ thị
> M3 đã quyết định cần gọi. Trust fields cũ chỉ còn `LEGACY_READ`; xem OpenAPI `draft.21` và
> `W-0123`. Giữ contract text cũ bên dưới để truy vết.

> [!IMPORTANT]
> Superseded by D-10 (2026-07-02): cac rule attempt cu trong file nay chi con gia tri lich su. Rule hien hanh: `MAX_ATTEMPT = 2` cho ca `GOLDEN_HOUR` va `TWENTY_FOUR_SEVEN`; GH window 5 phut A2@`T0+2:30`; 24/7 window 15 phut A2@`T0+7:30`. Xem `plan/ivr-orther/decisions-log.md` D-10.

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa semantic contract cho task mà Order Core gửi sang IVR.

## 1. Mục tiêu

`IvrConfirmationTaskV1` là contract nội bộ để Order Core yêu cầu IVR xét/gọi xác nhận đơn. Tài liệu này chưa tạo JSON Schema/OpenAPI thật, nhưng khóa field, owner, validation và reject rule để phase sau sinh contract.

## 2. Nguồn tham chiếu

| Nguồn | Vai trò |
| --- | --- |
| `docs/documents/1. master/04-MASTER-03-TRACEABILITY-ID.md` | Correlation, idempotency, trace. |
| `docs/documents/3. tech/02-TECH-01-FOUNDATION-RBAC-AUDIT-IDEMPOTENCY-EVIDENCE-REGISTRY.md` | Foundation audit/evidence/idempotency. |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md` | Pack boundary và program-based attempt policy. |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Module contract và lifecycle. |

## 3. Tổng quan contract

| Thuộc tính | Giá trị |
| --- | --- |
| Contract name | `IvrConfirmationTaskV1` |
| Direction | Order Core -> IVR Runtime |
| Producer | Commerce Order Core / Order State Machine |
| Consumer | IVR Runtime Orchestrator |
| Transport | Internal service contract, implementation decision ở phase sau |
| Idempotency | Bắt buộc |
| Correlation | Bắt buộc |
| Production use | Chỉ sau IVR-09 release gate |

## 4. Trường dữ liệu bắt buộc

| Field | Type semantic | Required | Owner | Ghi chú |
| --- | --- | --- | --- | --- |
| `task_id` | string | Có | Order Core | ID duy nhất của task. |
| `task_version` | string | Có | Order Core | `v1`. |
| `idempotency_key` | string | Có | Order Core | Chống duplicate task. |
| `correlation_id` | string | Có | Order Core | Trace xuyên hệ thống. |
| `created_at` | datetime | Có | Order Core | Thời điểm tạo task. |
| `expires_at` | datetime | Có | Order Core | Không vượt program window. |
| `order_id` | string | Có | Order Core | Official Order id. |
| `order_code_short` | string | Có | Order Core | Biến script được phép đọc. |
| `order_version` | string/integer | Có | Order Core | Race-condition guard. |
| `order_state` | enum | Có | Order Core | Phải là IVR-callable state. |
| `program_code` | enum | Có | Commerce Runtime | `GOLDEN_HOUR`, `TWENTY_FOUR_SEVEN`. |
| `attempt_policy_code` | enum | Có | Policy | Program-based policy. |
| `max_attempts` | integer | Có | Policy | 2 cho Golden Hour, 3 cho 24/7. |
| `attempt_schedule` | array | Có | Policy | T0/T0+5/T0+10 tùy chương trình. |
| `customer_ref` | string | Có | Customer/Commerce | Không phải full profile. |
| `customer_trust_status` | enum | Có | Trust Resolver | Snapshot trust. |
| `trusted_skip_allowed` | boolean | Có | Trust Resolver | Nếu true và không có risk, IVR không gọi. |
| `risk_flags` | array | Có | Commerce/Risk | Chỉ source-backed. |
| `official_contact_id` | string | Có | Customer/Commerce | Contact được gọi. |
| `phone_ref` | string | Có | Customer/Commerce | Secure reference. |
| `phone_masked` | string | Có | Customer/Commerce | Admin display. |
| `phone_e164_or_adapter_token` | string | Có điều kiện | Customer/Commerce | Chỉ nếu privacy/security cho phép. |
| `phone_validation_status` | enum | Có | Phone Resolver | Không unknown ở production. |
| `call_script_template_id` | string | Có | IVR Owner | Script đã duyệt. |
| `call_script_version` | string | Có | IVR Owner | Version đã duyệt. |
| `allowed_script_variables` | object | Có | Order Core | Chỉ biến được duyệt. |
| `sale_lock_snapshot` | object/ref | Có | Operational Core | Blocking snapshot/ref. |
| `recall_snapshot` | object/ref | Có | Operational Core | Blocking snapshot/ref. |
| `suppression_snapshot` | object/ref | Có | Operational/Gateway | Blocking snapshot/ref. |
| `call_restriction_snapshot` | object/ref | Có | Compliance/Customer | Opt-out/legal restriction. |
| `evidence_policy_version` | string | Có | Evidence Owner | Bắt buộc. |
| `privacy_policy_version` | string | Có | Privacy Owner | Bắt buộc. |

## 5. Validation tại task intake

IVR phải reject hoặc hold task nếu:

- Task thiếu `idempotency_key` hoặc `correlation_id`.
- `order_id` không phải Official Order.
- `order_state` không phải IVR-callable.
- `expires_at` đã qua.
- `program_code` không được hỗ trợ.
- `max_attempts` không khớp chương trình.
- Thiếu official contact hoặc phone projection.
- Phone invalid/unknown mà không có fallback policy.
- Sale Lock/Recall/Suppression/call restriction active.
- Evidence/privacy policy version thiếu.
- Script template/version chưa approved.

## 6. Output sau intake

| Intake result | Ý nghĩa |
| --- | --- |
| `TASK_ACCEPTED_CALL_JOB_CREATED` | Task hợp lệ, tạo CallJob. |
| `TASK_REJECTED_NOT_OFFICIAL_ORDER` | Không phải Official Order. |
| `TASK_REJECTED_POLICY_MISMATCH` | Attempt policy hoặc program mismatch. |
| `TASK_REJECTED_CONTACT_INVALID` | Contact/phone không hợp lệ. |
| `TASK_BLOCKED_OPERATIONAL` | Sale Lock/Recall/Suppression/call restriction. |
| `TASK_HELD_ADMIN_REVIEW` | Cần owner/admin review. |

## 7. Audit/evidence

Mỗi task intake phải ghi:

- `task_id`.
- `order_id`.
- `order_version`.
- `program_code`.
- `decision`.
- `reject_or_block_reason`.
- `correlation_id`.
- `idempotency_key`.
- `policy_version`.
- `evidence_ref`.
- `audit_ref`.

## 8. Tiêu chí chấp nhận

- Contract mô tả đủ dữ liệu Order Core phải gửi.
- Contract không chứa dữ liệu riêng tư vượt policy.
- Reject rule rõ, không để implementer tự suy luận.
- Sẵn sàng để phase sau sinh schema `IvrConfirmationTaskV1`.

## 9. Field-level contract expansion

| Field group | Fields | Validation | Reject/Hold reason |
| --- | --- | --- | --- |
| Identity | `task_id`, `task_version`, `idempotency_key`, `correlation_id` | Non-empty, unique/idempotent, version `v1`. | `TASK_REJECTED_INVALID_TRACE`. |
| Order | `order_id`, `order_code_short`, `order_version`, `order_state` | Official Order, callable state, current version. | `TASK_REJECTED_NOT_OFFICIAL_ORDER`, `TASK_REJECTED_STATE_NOT_CALLABLE`. |
| Program policy | `program_code`, `attempt_policy_code`, `max_attempts`, `attempt_schedule`, `expires_at` | Golden Hour 2/10; 24/7 3/15. | `TASK_REJECTED_POLICY_MISMATCH`. |
| Customer trust | `customer_ref`, `customer_trust_status`, `trusted_skip_allowed`, `risk_flags` | Resolver-backed, no hardcode. | `TASK_HELD_TRUST_POLICY_UNAVAILABLE`. |
| Contact | `official_contact_id`, `phone_ref`, `phone_masked`, optional dial token | Official contact only, phone valid. | `TASK_REJECTED_CONTACT_INVALID`. |
| Script | `call_script_template_id`, `call_script_version`, `allowed_script_variables` | Approved script and approved variables only. | `TASK_REJECTED_SCRIPT_NOT_APPROVED`. |
| Blockers | Sale Lock, Recall, Suppression, call restriction snapshots | No active blocker. | `TASK_BLOCKED_OPERATIONAL`. |
| Governance | `evidence_policy_version`, `privacy_policy_version` | Present and approved for target environment. | `TASK_HELD_POLICY_MISSING`. |

## 10. Required request invariants

Task payload must assert or imply:

- `not_for_quote_cart_draft = true`.
- `no_direct_order_update = true`.
- `call_purpose = ORDER_CONFIRMATION_ONLY`.
- `input_signal_only = true`.
- `program_code` and `max_attempts` match.
- `expires_at` does not exceed program window.
- `order_version` is included for callback race guard.

If implementation does not include these exact boolean fields, equivalent server-side validation and audit evidence are required.

## 11. Intake processing sequence

```text
receive task
  -> authenticate caller
  -> validate schema/required fields
  -> open idempotency scope
  -> reject duplicate conflict or return existing decision
  -> validate Official Order and order state
  -> resolve attempt policy
  -> validate official contact/phone
  -> check trusted skip/risk flags
  -> check operational blockers
  -> validate script/privacy/evidence policy
  -> create task record
  -> create CallJob if eligible
  -> write audit/evidence
  -> return intake decision
```

## 12. Intake result taxonomy

| Result | Creates CallJob? | Dispatch allowed? | Notes |
| --- | --- | --- | --- |
| `TASK_ACCEPTED_CALL_JOB_CREATED` | Yes | Only if release/scheduler gate allows. | Normal path. |
| `TASK_ACCEPTED_DRY_RUN_ONLY` | Yes | No real SIM. | For test/staging. |
| `TASK_SKIPPED_TRUSTED_CUSTOMER` | No | No | Order Core owns continuation. |
| `TASK_REJECTED_NOT_OFFICIAL_ORDER` | No | No | Quote/Cart/Draft. |
| `TASK_REJECTED_POLICY_MISMATCH` | No | No | Program/max/window mismatch. |
| `TASK_REJECTED_CONTACT_INVALID` | No | No | Invalid phone/contact. |
| `TASK_BLOCKED_OPERATIONAL` | No | No | Sale Lock/Recall/Suppression/etc. |
| `TASK_HELD_ADMIN_REVIEW` | No or held | No | Missing source/ambiguous policy. |

## 13. Idempotency rules

| Scenario | Required behavior |
| --- | --- |
| Same key, same payload | Return existing intake result. |
| Same key, different payload | Reject conflict. |
| Same `task_id`, different key | Reject conflict unless exact duplicate mapped. |
| Retry after transient IVR error | Safe retry using same key. |
| Retry after task rejected | Return same rejection, do not create job later unless new task/version. |

## 14. API/database mapping

| Contract field | API payload | DB column/entity |
| --- | --- | --- |
| `task_id` | `task_id` | `ivr_confirmation_tasks.task_id` |
| `official_order_id` | `official_order_id` / `order_id` | `official_order_id` |
| `order_version` | `order_version` | `order_version` |
| `program_code` | `program_type` | `program_type` |
| `max_attempts` | `max_attempts` | `max_attempts` |
| `attempt_schedule` | `attempt_schedule` | `attempt_schedule_json` |
| `phone_ref` | `phone_ref` | `phone_ref` |
| `phone_masked` | `phone_masked` | `phone_masked` |
| `eligibility_decision` | `eligibility_decision` | `eligibility_decision` |
| `evidence_ref` | `evidence_refs` | `evidence_refs_json` |
| `audit_ref` | `audit_refs` | `audit_refs_json` |

## 15. Negative task scenarios

| Scenario | Expected |
| --- | --- |
| Task missing correlation | Reject. |
| Task missing idempotency | Reject. |
| Program `GOLDEN_HOUR` with `max_attempts = 3` | Reject policy mismatch. |
| Program `TWENTY_FOUR_SEVEN` with only 2 attempts | Reject policy mismatch. |
| Phone invalid | Reject or hold, no CallJob dispatch. |
| Release flag disabled | Accept dry-run/hold, no real SIM dispatch. |
| Script version not approved | Reject/hold. |

## 16. Task acceptance tests

| Test ID | Given | Then |
| --- | --- | --- |
| IVR04-TASK-001 | Valid Golden Hour task | CallJob with 2 attempts. |
| IVR04-TASK-002 | Valid 24/7 task | CallJob with 3 attempts. |
| IVR04-TASK-003 | Duplicate idempotency same payload | Existing result. |
| IVR04-TASK-004 | Duplicate idempotency different payload | Conflict. |
| IVR04-TASK-005 | Quote task | Reject. |
| IVR04-TASK-006 | Active Sale Lock | Block. |
| IVR04-TASK-007 | Missing evidence policy | Hold/review. |
