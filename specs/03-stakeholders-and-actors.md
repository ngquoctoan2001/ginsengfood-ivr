# SRS-03 — Stakeholders & Actors

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p02`
Nguồn: `docs/documents/4. phase/phase-8/02` (§3, §9); `docx` §1, §16, §18; `MASTER-01` (owner boundary).

## 1. System actors (hệ thống)

| Actor | Vai trò với IVR | IVR được làm | IVR KHÔNG được làm |
| --- | --- | --- | --- |
| Commerce Order Core / Order State Machine | Tạo `IvrConfirmationTaskV1`; nhận callback; **quyết định transition** | Nhận task, gửi result signal | Tự transition order |
| Operational Core | Cấp/kiểm blocker: Sale Lock, Recall, Suppression, availability | Consume blocker | Override/bỏ qua blocker |
| Customer Trust / Customer Memory Resolver | Cấp trust decision, risk flags, trusted skip | Consume quyết định | Hardcode trusted |
| Official Contact / Customer Profile Resolver | Cấp `phone_ref`/`phone_masked`/dial token | Gọi contact đã duyệt | Đọc full profile/address |
| SIM Gateway Adapter (Internal) | Dial, phát script, capture DTMF/call status | Dial, capture, health check | Gửi SMS / ghi order |
| Evidence Registry / Audit | Nơi ghi evidence/audit | Ghi evidence/audit | Tự mark accepted/PASS |
| Notification Owner | Gửi thông báo **sau** Core decision | Handoff yêu cầu (gián tiếp) | Tự gửi từ IVR/SIM |
| Downstream (AI Advisor/Facebook/Live/Ads/CRM) | Chỉ consume trạng thái đã được Core công nhận | — | Trigger IVR / dùng result production trước release |
| Payment/MISA/Finance | Không kết nối trực tiếp | — | Xác nhận payment/revenue |

## 2. Human actors (con người)

| Actor | Loại | Quan tâm | Quyền (RBAC) |
| --- | --- | --- | --- |
| Ops Admin / Incident Manager | Vận hành | Queue, capacity, SIM health, incident | `IVR_QUEUE_VIEW/PAUSE/RESUME`, `IVR_SIM_ENABLE/DISABLE`, `IVR_MANUAL_RETRY`, `IVR_RESULT_REVIEW` |
| Release Owner | Governance | Mở release gate, sign-off | Owner sign-off (không bypass P0) |
| Security / Privacy Owner | Compliance | PII, recording, RBAC, audit | Review/approve privacy & security |
| Commerce / Order Owner | Nghiệp vụ | Order state, contract, revalidation | Chốt order state machine, callback contract |
| Ops-Core / Traceability Owner | Nghiệp vụ | Sale-lock/recall/suppression | Chốt blocker contract |
| IVR Infrastructure Owner | Kỹ thuật | SIM gateway, protocol, capacity | Chốt SIM protocol/pool |
| Customer (callee / người được gọi) | Bên ngoài | Nhận cuộc gọi xác nhận; opt-out | (bấm 1/0; quyền opt-out/privacy) |
| Dev / QA | Triển khai | Contract, resolver, guard, smoke | Theo phân quyền dev/test |

## 3. Ghi chú ownership (MASTER-01)
- CONFIRMED: Order Core là **decision owner** của order state; IVR là **consumer/signal**. Nguồn: `MASTER-01 SRC-IVR-001`, phase-8/02 §9.
- CONFIRMED: Official Contact Registry là source-of-truth số điện thoại; IVR không hardcode. Nguồn: `MASTER-02 §18`.
- `NEED_CONFIRMATION`: Ánh xạ actor con người ↔ vai trò tổ chức thực tế (ai giữ Release Owner, Security Owner) — Q-U1/Q-K1 trong plan/15.
