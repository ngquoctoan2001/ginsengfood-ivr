# PROMPT P3-2 — Dashboard, Call Log & Call Detail

## 0. Meta
| | |
| --- | --- |
| **ID** | `P3-2` · **Phase** 3 — Admin UI |
| **Work ID** | `W-0026` (canonical tracker §5) |
| **Prereq** | `P3-1`, `P2-8` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | Next.js · `Ivr.Api` admin endpoints |

## 1. ROLE
Bạn là **Senior Frontend Engineer**. Bạn xây các màn giám sát vận hành: dashboard trạng thái hàng đợi/SIM, call log tra cứu, và call detail hiển thị evidence/timeline. Bạn trình bày dữ liệu **privacy-safe** và không tạo lối tắt vượt Order Core.

## 2. CONTEXT
Đây là "phòng điều khiển" IVR: ops thấy có bao nhiêu task, attempt, kết quả, SIM health; tra cứu 1 cuộc gọi để review evidence khi cần. Cần API admin (list/detail) — nếu chưa có, prompt này định nghĩa contract cần và mock, ghi vào integration nếu thuộc backend.

## 3. SOURCE SPECS (đọc trước)
- `specs/ui/01-dashboard.md`, `specs/ui/02-call-log.md`, `specs/ui/03-call-detail.md`
- `specs/api/03-admin-api.md`, `specs/functional/08-evidence-audit-privacy.md`
- `plan/ivr-orther/decisions-log.md` §D-02 · §D-05 · §DF-01

## 4. DECISIONS & CONSTRAINTS
- **D-02:** call detail hiển thị result là **signal + Core outcome** (đọc từ Core/callback state); UI không suy/không đổi order state.
- **D-05:** phone masked; DTMF hiển thị nghiệp vụ (1/0) nhưng không lộ PII; recording OFF (không có player).
- **DF-01:** dashboard/log cần `IVR_QUEUE_VIEW`/`IVR_RESULT_REVIEW`; action review có audit.
- Evidence view: hiển thị link evidence/audit (task→attempt→result→callback), correlation id.

## 5. INPUTS / DEPENDENCIES
- API client (P3-1); admin list/detail endpoints (`Ivr.Api` — bổ sung nếu thiếu).
- Seed để render dev (`seed/*`).

## 6. BUILD STEPS
1. **Dashboard**: card tổng (task theo trạng thái, attempt hôm nay, success/no-answer/technical rate, SIM pool health, capacity). Auto-refresh nhẹ. Filter theo program/thời gian.
2. **Call Log**: bảng phân trang/tìm kiếm (theo order_code, correlation_id, trạng thái, program, ngày); cột masked phone; export CSV masked (tuỳ chọn).
3. **Call Detail**: policy-versioned attempt timeline, speech metadata (không PII), disposition→result, technical exception, Target callback semantic ACK; Golden Hour 200/422 có nhãn `CURRENT_COMPAT`, evidence/audit/correlation. Review chỉ ghi audit, không đổi order.
4. Xử lý loading/error/empty; envelope error render.
5. Không action transition order; chỉ IVR admin action (review/note).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `admin-ui/app/dashboard/**` | Dashboard |
| `admin-ui/app/calls/**` | Call log + detail |
| `admin-ui/components/calls/**` | Timeline, evidence panel, filters |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `E2E-UI-LOG-01` | e2e | filter theo order_code/status trả đúng; masked phone. |
| `E2E-UI-DETAIL-02` | e2e | detail hiển thị A1/A2 timeline + result + callback Core code + evidence links. |
| `UT-UI-NOORDER-03` | component | không có control transition order (D-02). |
| `UT-UI-REVIEW-04` | component | result review yêu cầu `reason` + gọi audit; ẩn nếu thiếu quyền. |

Trace: `specs/testing/05`, `specs/ui/01-03`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] PII masked mọi nơi; [ ] không control order; [ ] evidence/correlation hiển thị; [ ] RBAC gate.
**Reviewer:** dashboard số liệu đọc từ API (không tính sai client); detail phản ánh Core outcome trung thực (kể cả 422).

## 10. EVIDENCE EXPECTED
Screenshot dashboard/log/detail, masked phone proof, review-action audit record, error/empty states.

## 11. FORBIDDEN
- ❌ Control transition order (D-02). ❌ Lộ số/recording (D-05). ❌ Tính KPI sai lệch ở client thay vì API.

## 12. DEFINITION OF DONE
- [ ] 3 màn + evidence view; 4 test §8 xanh; evidence §10 đủ.
