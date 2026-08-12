# IVR-17 - Integration Design

Trạng thái: `SDS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Vai trò: Thiết kế tích hợp giữa IVR và các hệ thống liên quan.

## 1. Mục tiêu

Tài liệu này trả lời rõ IVR kết nối với hệ thống nào, kết nối bằng gì, dữ liệu gì đi qua kết nối đó, failure xử lý ra sao, và boundary ownership nằm ở đâu.

## 2. Integration map

| Hệ thống | Direction | Kết nối bằng gì | Payload/chức năng | Owner quyết định cuối |
| --- | --- | --- | --- | --- |
| Commerce Order Core | Order Core -> IVR, IVR -> Order Core | Internal API/callback | Task, result callback, core ack | Order Core |
| Operational Core | IVR/Order Core -> Operational Core | Internal read/check API | Sale Lock, Recall, suppression, availability, traceability blocker | Operational Core |
| Customer Trust Resolver | Order Core/IVR -> Resolver | Internal resolver/API | Trusted skip, risk flags | Customer/CRM owner |
| Official Contact Resolver | Order Core/IVR -> Resolver | Privacy-safe projection/API | Official contact, phone ref, masked phone, dial token | Customer/Commerce owner |
| SIM Gateway Adapter | IVR -> SIM Gateway | Internal adapter protocol | Dial, call status, DTMF, technical error | IVR Infrastructure |
| Evidence Registry/Audit | IVR -> Evidence | Internal writer/API | Evidence refs, audit refs | Evidence/Foundation |
| Admin Web/Ops Console | Admin -> IVR API | Admin API | Queue, SIM, retry, review | IVR/Admin owner |
| Notification Owner | Order Core -> Notification | Internal handoff after core decision | Customer notification after official state change | Notification owner |
| AI/Facebook/Live/CRM | No direct command | Read only after core-approved state if policy allows | No raw IVR command | Respective channel owner |
| Payment/MISA/Finance | No direct IVR integration | None from IVR | IVR never confirms payment/revenue/accounting | Finance/MISA owner |

## 3. Order Core integration

### 3.1 Order Core -> IVR

Contract: `IvrConfirmationTaskV1`  
Transport: internal API `POST /v1/ivr/order-confirmation/tasks`

Required semantics:

- Only Official Order.
- Includes order version.
- Includes idempotency/correlation.
- Includes program policy.
- Includes official contact projection.
- Includes blocker snapshots or refs.
- Includes evidence/privacy policy versions.

Failure:

| Failure | Handling |
| --- | --- |
| IVR unavailable | Order Core records task delivery failure and retries bounded or holds order per policy. |
| IVR rejects task | Order Core records reason; no IVR call. |
| Duplicate task | IVR returns idempotent previous result. |
| Stale order version | IVR rejects/holds task. |

### 3.2 IVR -> Order Core

Contract: `IvrConfirmationResultCallbackV1`  
Transport: internal callback/API `POST /v1/ivr/order-confirmation/result-callbacks` or Order Core equivalent route in implementation.

Required semantics:

- Callback is input signal only.
- Order Core revalidates before transition.
- `order_version_seen_by_ivr` is mandatory.
- Callback idempotency is mandatory.
- Callback ack must differentiate accepted/stale/blocked/review.

## 4. Operational Core integration

Purpose:

- Sale Lock.
- Recall.
- Suppression.
- Availability/sellable status.
- Warehouse/traceability constraints if order release depends on them.

Rules:

- IVR must not override Operational Core blockers.
- If Operational Core check is unavailable and no owner-approved fallback exists, scheduler must not dispatch call.
- Order Core must re-check blockers when receiving callback.

Implementation options:

| Option | Use |
| --- | --- |
| Synchronous internal read/check API | Preferred for pre-dispatch and callback revalidation. |
| Cached projection | Allowed only if TTL/source owner approves and fail-safe behavior is clear. |
| Event-only dependency | Not enough for final revalidation unless Order Core owns current projection. |

## 5. Customer trust and contact integration

Customer trust resolver outputs:

- `trusted_customer_decision`.
- `trusted_skip_allowed`.
- `risk_flags`.
- `policy_version`.
- `evidence_ref`.

Official contact resolver outputs:

- `official_contact_id`.
- `phone_ref`.
- `phone_masked`.
- `phone_validation_status`.
- Optional `dial_token` if approved.

Rules:

- IVR cannot pick arbitrary phone number.
- IVR cannot read full profile/full address.
- Trusted skip is not an IVR decision.
- Phone validation technical error is not invalid phone final.

## 6. SIM Gateway integration

Internal SIM Gateway Adapter boundary:

| Input | Output |
| --- | --- |
| `sim_channel_id` | `provider_call_id` if available. |
| `dial_token` or approved phone access | Raw call status. |
| Script template/version | DTMF key/status. |
| Attempt context | Timing and technical errors. |

Required adapter behavior:

- One SIM only one active outbound call.
- Adapter has no Order Core credential.
- Adapter does not send SMS/notification.
- Adapter returns technical failure distinctly.
- Adapter redacts logs.

Owner Decision Required:

- Exact SIM Gateway hardware/API protocol.
- Whether recording is enabled.
- Provider-specific mapping for `BUSY`, `REJECTED`, `UNREACHABLE`, `CALL_DROPPED`.

## 7. Evidence/Audit integration

IVR writes evidence/audit for:

- Task intake.
- Eligibility decision.
- Attempt dispatch.
- SIM reservation/release.
- DTMF/call result.
- Result normalization.
- Callback/ack.
- Admin action.
- Capacity incident.
- Technical exception.

Failure rule:

- If evidence required for final callback cannot be written, route to technical exception/admin review. Do not allow Order Core transition based on evidence-less signal.

## 8. Admin integration

Admin Web consumes:

- Queue status.
- CallJob detail.
- Attempt/result detail.
- SIM channel health.
- Capacity incidents.
- Technical exceptions.
- Evidence/audit refs.

Admin writes:

- Queue pause/resume.
- SIM disable/enable.
- Manual technical retry.
- Result review note.

All admin writes require permission, reason, audit, and no-policy-bypass invariant.

## 9. Notification integration

IVR does not call Notification directly.

Allowed flow:

1. IVR sends result callback to Order Core.
2. Order Core revalidates.
3. Order Core transitions order if allowed.
4. Order Core or notification owner sends notification based on official state.

Blocked:

- No-answer max auto notification from IVR.
- Customer cancel notification directly from SIM Adapter.
- Technical exception notification as cancellation.

## 10. Events/AsyncAPI integration

Events are signal/visibility events:

- `ivr-confirmation-requested`.
- `ivr-confirmed`.
- `ivr-customer-cancelled`.
- `ivr-no-answer-final`.
- `ivr-invalid-phone-final`.
- `ivr-technical-exception`.
- `ivr-operational-blocked`.
- `ivr-capacity-incident-opened`.

AsyncAPI currently maps event names to payload refs only. Broker/topic/retry/outbox remains future approved toolchain.

## 11. Integration acceptance criteria

- Every connected system has owner, direction, contract, and failure behavior.
- No direct IVR -> Payment/MISA/Revenue/Warehouse mutation.
- No direct channel/AI/Facebook/CRM -> IVR command.
- SIM Adapter cannot update order.
- Evidence failure cannot be ignored for final decisions.
