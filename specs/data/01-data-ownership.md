# DATA-01 — Data Ownership

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p06` · Nguồn: `phase-8/00 §5`, `/02`; `MASTER-01`; decisions.
Chiều IVR: **owner** (IVR sở hữu) · **snapshot** (nhận trong task, chỉ đọc) · **read** (đọc realtime, không sở hữu) · **write** (ghi) · **none** (không chạm).

## 1. Dữ liệu KHÔNG thuộc IVR (consume/none)
| Nhóm dữ liệu | Owner | IVR | Ghi chú / quyết định |
| --- | --- | --- | --- |
| Order state, order_code, transition | Commerce Order Core | **snapshot** (`order_state` đục + COD gate; `order_version` target/nullable) | KHÔNG write (D-01/D-02/DS-01/DS-04; P0-IVR-002) |
| Official Order eligibility (là official order?) | Order Core | snapshot | task chỉ trên Official Order (D-01) |
| Program & attempt policy | Commerce/Program Runtime | snapshot | GH/24-7, max 2, T0 (D-10) |
| Customer trust / risk flags | Customer Trust Resolver (CRM/business-platform) | snapshot | không hardcode (D-12/D-13) |
| Official contact / phone | Customer/Commerce (OfficialContactResolver) | snapshot (`phone_ref`/`masked`/`dial_token`) | RESTRICTED; token→số ở SIM vault (D-05) |
| Blocker: sale-lock/recall/quality/availability | **Operational Core** (qua sellable gate) | snapshot per-line + Core revalidate | Core fan-out (DO-01/02/03/CORR-1) |
| Do-not-call / opt-out / call-restriction | **CRM / Customer Identity** | snapshot (✅ DC-01; IR-CRM-01 rich fields pending) | KHÔNG thuộc ops (DO-CORR-2) |
| Payment / COD / MISA / verified revenue / commission | Payment/Finance/Commerce | **none** ngoài `payment_method_snapshot=COD` gate do Core cấp | IVR không xử lý/thay đổi payment, chỉ reject nếu không phải COD (DS-01; phase-8/02) |
| Product master / catalog / price / ingredient | ops (master) / commerce (catalog) | **none** (nếu cần tên → qua commerce/PACK-05) | DO-09 |
| Member tier / Diamond / CRM content / AI content | CRM / business-platform | **none** | cấm đọc trong script (phase-8/02 §11) |
| Full profile / full address / order history / health note | Customer/CRM | **none** | RESTRICTED cấm (phase-8/02 §11) |
| Permission / RBAC | Permission Core (Foundation) | read/consume | DF-01 |
| Evidence accepted state | Evidence Registry (Foundation) | **write** evidence refs; KHÔNG tự mark accepted | DF-03/phase-8/00 |

## 2. Dữ liệu IVR SỞ HỮU (owner)
| Entity | Owner | Ghi chú |
| --- | --- | --- |
| `ivr_confirmation_tasks` | IVR | snapshot task từ Order Core (không phải chân lý order) |
| `ivr_call_jobs`, `ivr_call_attempts`, `ivr_call_results`, `ivr_result_callbacks` | IVR | vòng đời gọi/kết quả/callback |
| `ivr_sim_channels`, `ivr_capacity_incidents`, `ivr_technical_exceptions`, `ivr_admin_actions` | IVR | vận hành/incident/audit action |
| `ivr_evidence_links` | IVR | link evidence/audit refs (không thay Evidence Registry) |
| `ivr_raw_call_event` (từ docx V0.2, OD-DR-03) | IVR | raw SIM/DTMF trước normalize; không lưu PII thô |

## 3. Quy tắc P0 ownership
- IVR **không** có bảng/cột nào cho phép sở hữu order state (chỉ snapshot/version) — phase-8/12 §2.
- IVR **không** write payment/inventory/recall/CRM.
- Order state realtime & blocker realtime = **Order Core** trách nhiệm khi revalidate (D-04/DO-03).
