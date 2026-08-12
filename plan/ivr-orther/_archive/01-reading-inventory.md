# 01 — Reading Inventory

Tổng số file `.md` trong `docs/`: **179**. Kho gồm `docs/documents/{0. appendices, 1. master, 2. pack, 3. tech, 4. phase/phase-1..8, 6. canonical}` + `docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.2_CLEAN_FINAL.docx`.

Chú thích cột: **Nhóm** = phân loại; **Rel** = mức liên quan IVR (H/M/L); **Đọc** = `Chi tiết` (đọc kỹ trong phiên) / `Scan` (đọc qua subagent/tóm tắt).

## A. IVR / Call Center (phase-8, PACK-09, TECH-09)

| Path (dưới `docs/documents/`) | Nhóm | Rel | Đọc | Ghi chú & keyword |
| --- | --- | --- | --- | --- |
| `4. phase/phase-8/00-QUẢN TRỊ NGUỒN SỰ THẬT VÀ PHẠM VI.md` | IVR | H | Chi tiết | Governance/scope/source-of-truth; `IVR_RESULT_IS_INPUT_SIGNAL_ONLY`, `INTERNAL_SIM_GATEWAY_SERVER`, governance gates, P0 rules |
| `4. phase/phase-8/01-MỤC ĐÍCH KINH DOANH VÀ CA SỬ DỤNG XÁC NHẬN.md` | IVR | H | Scan | Business purpose; ORDER_CONFIRMATION_ONLY, anti-fake, phím 1/0, trusted skip |
| `4. phase/phase-8/02-RANH GIỚI SỞ HỮU VÀ HỆ THỐNG KẾT NỐI.md` | IVR/Arch | H | Chi tiết | Ownership matrix, connected systems, data allowed/prohibited, failure contracts |
| `4. phase/phase-8/03-ĐIỀU KIỆN GỌI NIỀM TIN KHÁCH HÀNG VÀ LIÊN HỆ CHÍNH THỨC.md` | IVR | H | Scan | Eligibility, trusted skip, phone validation, official contact, `phone_ref/masked` |
| `4. phase/phase-8/04-HỢP ĐỒNG TỪ LÕI ĐƠN HÀNG ĐẾN TÁC VỤ IVR.md` | IVR/API | H | Chi tiết | `IvrConfirmationTaskV1`, intake validation, idempotency, intake taxonomy |
| `4. phase/phase-8/05-CHÍNH SÁCH GỌI LẠI BỘ LẬP LỊCH VÀ HÀNG ĐỢI.md` | IVR/Workflow | H | Scan | Attempt policy (rule cũ 2/10 & 3/15 — **đã thay bằng D-10**), scheduler, `ONE_SIM_ONE_ACTIVE_CALL` |
| `4. phase/phase-8/06-BỘ CHUYỂN ĐỔI CỔNG SIM NỘI BỘ.md` | IVR/API | M | Scan | SIM adapter boundary, DTMF capture, call disposition, no order write |
| `4. phase/phase-8/07-CHUẨN HÓA KẾT QUẢ VÀ CALLBACK VỀ LÕI ĐƠN HÀNG.md` | IVR/API | H | Chi tiết | `IvrConfirmationResultCallbackV1`, result taxonomy, revalidation, race matrix |
| `4. phase/phase-8/08-GIÁM SÁT QUẢN TRỊ BẰNG CHỨNG KIỂM TOÁN VÀ RIÊNG TƯ.md` | IVR/Security | H | Scan | Admin RBAC, evidence, privacy, recording OFF, retention |
| `4. phase/phase-8/09-MA TRẬN KIỂM THỬ KHÓI VÀ CỔNG PHÁT HÀNH.md` | IVR/Testing | H | Scan | IVR-SMK-001..030, release gate, `REAL_CUSTOMER_CALL_ALLOWED` |
| `4. phase/phase-8/10-KIẾN TRÚC TRIỂN KHAI.md` | IVR/Arch | M | Scan | Service blocks, CallJob/Attempt, layers |
| `4. phase/phase-8/11-THIẾT KẾ API.md` | IVR/API | H | Chi tiết | `/v1/ivr/order-confirmation/*`, headers, error mapping, admin permission |
| `4. phase/phase-8/12-THIẾT KẾ CƠ SỞ DỮ LIỆU.md` | IVR/DB | H | Chi tiết | 9–10 bảng `ivr_*`, ERD, constraints, retention/privacy |
| `4. phase/phase-8/13-THIẾT KẾ HÀM VÀ DỊCH VỤ.md` | IVR/Arch | M | Scan | IvrTaskIntakeService, DeadlineAwareScheduler, ResultNormalizer |
| `4. phase/phase-8/14-ĐIỀU PHỐI QUY TRÌNH.md` | IVR/Workflow | H | Scan | 8 workflow: confirm/cancel/no-answer/invalid/technical/race/trusted/capacity |
| `4. phase/phase-8/15-BẢO MẬT RIÊNG TƯ VÀ KIỂM TOÁN.md` | IVR/Security | M | Scan | Threat model, phone token, PII redaction, dual-evidence override |
| `4. phase/phase-8/16-YÊU CẦU PHI CHỨC NĂNG.md` | IVR/NFR | M | Scan | Reliability, capacity SIM 12/24/32, concurrency, observability |
| `4. phase/phase-8/17-THIẾT KẾ TÍCH HỢP.md` | IVR/Integration | H | Scan | Integration design (con trỏ cho integration-requirements) |
| `4. phase/phase-8/18-TRIỂN KHAI QUAN SÁT VÀ SỔ TAY VẬN HÀNH.md` | IVR/Ops | M | Scan | Observability/runbook |
| `4. phase/phase-8/19-KẾ HOẠCH KIỂM THỬ KHÓI VÀ PHÁT HÀNH.md` | IVR/Testing | M | Scan | Smoke/release plan |
| `4. phase/phase-8/20-KẾ HOẠCH GIAI ĐOẠN TÁC VỤ VÀ VIỆC CẦN LÀM.md` | IVR/PM | M | Scan | Task/backlog plan |
| `4. phase/phase-8/21-QUYẾT ĐỊNH DỌN DẸP CHÍNH SÁCH GỌI LẠI.md` | IVR/Workflow | M | Scan | Callback/attempt cleanup decision |
| `4. phase/phase-8/22-ĐƯỜNG CƠ SỞ ĐẦU VÀO IVR.md` | IVR | M | Scan | IVR input baseline |
| `4. phase/phase-8/23-XÁC NHẬN ĐƠN HÀNG BẰNG IVR.md` | IVR/Workflow | H | Scan | Order confirmation via IVR (use case detail) |
| `4. phase/phase-8/24-ĐÓNG KHOẢNG TRỐNG TIỀN SRS IVR.md` | IVR | H | Scan | Gap closure; `Owner Decision Required` list |
| `4. phase/phase-8/25-MA TRẬN TRUY VẾT SRS IVR.md` | IVR | H | Scan | Traceability matrix IVR-00..09 → source |
| `4. phase/phase-8/26-BẢNG GOM GIAI ĐOẠN 8.md` | IVR | M | Scan | Phase-8 rollup |
| `2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md` | IVR | H | Scan | Pack source; config baseline, fail-closed |
| `3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | IVR/Tech | H | Scan | Technical source; module contract 01–06, classification |
| `docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.2_CLEAN_FINAL.docx` | IVR | H | `TODO` | Bản .docx tổng hợp module 8; chưa parse (cần đọc bằng công cụ docx ở p01) |

## B. Sales Platform / Module 3 / Module 3.1

| Path | Nhóm | Rel | Đọc | Ghi chú & keyword |
| --- | --- | --- | --- | --- |
| `4. phase/phase-3/00-PHÂN TÍCH HIỆN TRẠNG.md` | Sales | H | Scan | Commerce runtime gap; IVR là "reserved pack" trong phase-3 §13.6 |
| `4. phase/phase-3/05-ĐƠN CHÍNH THỨC MÃ ĐƠN VÀ MÁY TRẠNG THÁI.md` | Sales | H | Scan | Official order, order_code, state machine; "no CustomerConfirmation → no order_code" |
| `4. phase/phase-3/06-THANH TOÁN GIAO HÀNG HÓA ĐƠN VÀ THUẾ.md` | Sales | M | Scan | Payment/shipping/invoice/VAT |
| `4. phase/phase-3/07-DOANH THU XÁC THỰC HOA HỒNG ROAS VÀ BÀN GIAO.md` | Sales | M | Scan | Verified revenue, commission, ROAS |
| `4. phase/phase-3/01..04, 08..11` | Sales | M/L | Scan | Sellable gate, quote/cart, smoke, handoff SRS |
| `4. phase/phase-3.1/07-THANH TOÁN VẬN CHUYỂN IVR VÀ ĐƠN HÀNG.md` | Sales/IVR | **H (CRITICAL)** | Scan | **`IVRRequiredDecision`, high-risk detection, "no order_code before IVR pass", quota release, IVR không tự hủy/payment** |
| `4. phase/phase-3.1/00-PHÂN TÍCH HIỆN TRẠNG GIAI ĐOẠN 3.1.md` | Sales | H | Scan | Member, price, program, Golden Hour, Diamond, CRM, AI, IVR decision |
| `4. phase/phase-3.1/05-CRM 12 THÁNG VÀ VÒNG ĐỜI THÀNH VIÊN.md` | Sales/CRM | M | Scan | CRM lifecycle D0–M12 |
| `4. phase/phase-3.1/06-CỐ VẤN AI CHÍNH SÁCH LÕI VÀ TÌNH HUỐNG THỰC CHIẾN.md` | Sales/AI | M | Scan | AI advisor policy |
| `4. phase/phase-3.1/02,03,04,08..11` + `5. bo sung/00..05` | Sales | M/L | Scan | Membership pricing, Diamond commission, golden-hour messaging, CRM automation; `IVR_confirmation_extra_time = 5 phút` (bo sung/03) |
| `3. tech/05-TECH-04-COMMERCE-RUNTIME-SELLABLE-GATE-QUOTE-CART-ORDER-PAYMENT-SHIPPING.md` | Sales/Tech | H | Scan | Commerce owner of Official Order, QuoteSnapshot, payment, verified revenue |
| `3. tech/06-TECH-05-AI-ADVISOR-RUNTIME-...-SALES-ORDER-DRAFT-HANDOFF.md` | Sales/Tech | H | Scan | AI advisor consumption boundary, order draft handoff |
| `2. pack/05-PACK-05-AI-ADVISOR-CHANNEL.md` | Sales/Pack | M | Scan | AI advisor pack, human handoff triggers |
| `2. pack/04-PACK-04-MISA-ACCOUNTING-HANDOFF.md` | Sales/Finance | L | `TODO` | MISA accounting handoff (IVR không chạm) |

## C. Ops-Core / Module 1 / Module 2

| Path | Nhóm | Rel | Đọc | Ghi chú & keyword |
| --- | --- | --- | --- | --- |
| `4. phase/phase-1/02-SẢN PHẨM SKU NGUYÊN LIỆU VÀ ĐƠN VỊ TÍNH.md` | Ops | H | Scan | 20 SKU baseline, material canonical, UOM, dietary |
| `4. phase/phase-1/03-CÔNG THỨC BOM PHIÊN BẢN G1.md` | Ops | H | Scan | Recipe/BOM/formula version |
| `4. phase/phase-1/04-KHÓA KÍCH HOẠT SẢN PHẨM.md` | Ops | M | Scan | Product activation, "Product Active ≠ Sellable", block reasons |
| `4. phase/phase-1/00,01,05..11` | Ops | M/L | Scan | Product master design, seed governance, print/accounting forms |
| `4. phase/phase-2/00-PHÂN TÍCH VẬN HÀNH LÕI.md` | Ops | H | Scan | 12 phiếu/lệnh, boundary raw→batch→warehouse→inventory |
| `4. phase/phase-2/05-MẺ SẢN XUẤT QC PHÁT HÀNH KHO VÀ TỒN.md` | Ops | H | Scan | Batch lifecycle, QC, release, finished goods inventory |
| `4. phase/phase-2/06-TRUY XUẤT QR THU HỒI VÀ KHÓA BÁN.md` | Ops | **H** | Scan | **Traceability QR, recall, sale-lock state; public trace whitelist** |
| `4. phase/phase-2/01..04, 07` | Ops | M | Scan | Ops-core tech design, material issue, QC, smoke |
| `3. tech/03-TECH-02-PRODUCT-SKU-INGREDIENT-RECIPE-FORMULA-PRODUCT-ACTIVATION.md` | Ops/Tech | H | Scan | Product domain principles, "active ≠ sellable" |
| `3. tech/04-TECH-03-OPERATIONAL-CORE-...-TRACEABILITY-RECALL-SALE-LOCK.md` | Ops/Tech | H | Scan | Ops-core source-of-truth, sale-lock/recall owner |
| `2. pack/01-PACK-01-OPERATIONAL-CORE.md` | Ops/Pack | H | Scan | Ops-core pack (raw lot, batch, inventory, trace, recall) |
| `2. pack/02,03-PACK-02/03-...` | Ops/Pack | M | Scan | Product master pack, demand/MRP/procurement |

## D. Shared Architecture / Master / Foundation

| Path | Nhóm | Rel | Đọc | Ghi chú & keyword |
| --- | --- | --- | --- | --- |
| `00-DOC-READING-ORDER.md` | Shared | L | Scan | Index/reading order |
| `00-AI-EVALUATION-DEV-READINESS.md` | Shared | H | Scan | Chuẩn đánh giá dev-readiness (dùng cho p14) |
| `1. master/01-MASTER-00-INDEX-REGISTRY.md` | Shared | H | Scan | Pack registry canonical, global non-violate rules, IVR §5.9 |
| `1. master/02-MASTER-01-SOURCE-OF-TRUTH.md` | Shared | H | Scan | SoT registry 7 loại; `SRC-IVR-001` IVR Confirmation |
| `1. master/03-MASTER-02-CROSS-PACK-DEPENDENCY.md` | Shared | H | Scan | 16 business domain P0, dependency, official contact registry §18 |
| `1. master/04-MASTER-03-TRACEABILITY-ID.md` | Shared | H | Scan | ID/correlation/idempotency; DOMAIN-12 ORDER/IVR §27 |
| `1. master/05-MASTER-04-RUNTIME-RESOLUTION-GUARD.md` | Shared | H | Scan | Resolver status, guard decision, no-hardcode |
| `1. master/06-MASTER-05-EVIDENCE-SMOKE-COMPLETION-GATE.md` | Shared | H | Scan | Evidence package, 5-layer smoke, completion gate |
| `1. master/07..10-MASTER-06/07/08/09` | Shared | M | Scan | Reserved future integration, release control, decision log, cross-phase runtime lock |
| `3. tech/01-TECH-00-...MASTER-PLAN.md` | Shared/Tech | H | Scan | Technical implementation master plan, system boundary |
| `3. tech/02-TECH-01-FOUNDATION-RBAC-AUDIT-IDEMPOTENCY-EVIDENCE-REGISTRY.md` | Shared/Tech | H | Scan | RBAC, audit, idempotency, evidence registry conventions |
| `3. tech/11-TECH-10-GLOBAL-SMOKE-UAT-...-PRODUCTION-READINESS-CONTROL.md` | Shared/Tech | H | Scan | Global smoke/UAT/release gateway |
| `3. tech/12-TECH-11-...ROADMAP...` , `13-TECH-12-...PHASE-BACKLOG...`, `14-TECH-13-...DEV-PROMPT-PACK...` | Shared/Tech | H | Scan | Roadmap, backlog matrix, dev prompt pack template (dùng cho p13) |
| `2. pack/10-PACK-10-COMPLETION-EVIDENCE-GATEWAY.md` | Shared/Pack | H | Scan | Evidence registry, completion control, gateway gate |
| `0. appendices/01..05` | Shared/Ref | L | Scan | Operational forms, auto-form rules, printing code, MISA mapping, material taxonomy |
| `6. canonical/01-CANONICAL-CRM-MEMBER-LIFECYCLE-RUNTIME.md` | Shared/Canonical | M | Scan | CRM lifecycle runtime (IVR có thể trigger sau Core decision) |
| `6. canonical/02-CANONICAL-FINANCE-DIAMOND-COMMISSION-PAYOUT-RUNTIME.md` | Shared/Canonical | M | Scan | Diamond commission runtime |
| `6. canonical/03-CANONICAL-EVIDENCE-SMOKE-GATE-CUSTOMER-TO-CASH-CARE.md` + `README.md` | Shared/Canonical | H | Scan | Customer-to-cash-care runtime; IVR là control point giữa order-confirm và payment |

## E. Nhóm còn lại / cần review thêm

| Path | Nhóm | Ghi chú |
| --- | --- | --- |
| `docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.2_CLEAN_FINAL.docx` | Unknown/Need Review | `TODO`: parse bằng công cụ docx; đối chiếu với phase-8 md để tìm khác biệt version (V0.2 CLEAN FINAL) |
| `4. phase/phase-4/.codex-doc-memory/*` | Unknown | Metadata sinh tự động của công cụ (markdown-doc-map) — không phải nội dung nghiệp vụ |
| `4. phase/phase-4..7` (AI advisor, Facebook, Ads, MC-AI-Live) | Sales-adjacent | Rel thấp với IVR; đọc khi cần bối cảnh kênh; chưa đọc chi tiết |

## F. Ghi nhận GAP về tài liệu tham chiếu bị thiếu

- `GAP`: phase-8 nhiều lần trích `docs/source-map.md` — **file không tồn tại** trên đĩa. Cần tạo/định vị ở p01.
- `GAP`: phase-8 trích `docs/documents/4. phase/phase-8/ivr-pre-srs-gap-closure.md` — **không tồn tại** với tên đó; nội dung tương ứng nằm ở `24-ĐÓNG KHOẢNG TRỐNG TIỀN SRS IVR.md`. Lệch tên file → ghi nhận để chuẩn hóa.
- `GAP`: phase-8/11 trích contract `openapi/business-platform/ivr-order-confirmation.v1.yaml` — **chưa có** thư mục `openapi/`.
