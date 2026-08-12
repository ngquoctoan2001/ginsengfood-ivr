# PROMPT P3-4 — Reporting & Analytics UI

## 0. Meta
| | |
| --- | --- |
| **ID** | `P3-4` · **Phase** 3 — Admin UI |
| **Prereq** | `P3-2`, `P10-4` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | Next.js · analytics/reporting API |

## 1. ROLE
Bạn là **Senior Frontend Engineer chuyên dashboard dữ liệu**. Bạn xây màn báo cáo/analytics cho IVR Order Confirmation, tập trung KPI nghiệp vụ, xu hướng theo program/thời gian/script variant, drill-down privacy-safe, và export dữ liệu đã sanitize. UI này chỉ đọc dữ liệu aggregate, không điều khiển cuộc gọi, không suy diễn order state.

## 2. CONTEXT
P3-2 phục vụ vận hành realtime theo call log/detail; P3-4 phục vụ phân tích xu hướng và cải tiến nghiệp vụ. Dữ liệu đầu vào đến từ pipeline P10-4: outcome cuộc gọi đã aggregate/anonymized, không chứa raw phone hay PII. Màn này giúp owner xem tỉ lệ confirm/no-answer/technical, hiệu quả Golden Hour/24-7, và chất lượng script/A-B theo thời gian.

## 3. SOURCE SPECS (đọc trước)
- `prompt/phase-10-compliance-maturity/P10-4-analytics-bi-pipeline.md`
- `specs/data/05-pii-policy.md`
- `specs/functional/05-result-normalization-callback.md`
- `specs/architecture/06-observability.md`
- `specs/testing/05-e2e-test-plan.md`, `specs/testing/07-security-privacy-test-plan.md`
- `plan/ivr-orther/decisions-log.md` §D-05, §DT-02, §D-14

## 4. DECISIONS & CONSTRAINTS
- **D-05:** không render raw phone/full profile/full address/payment/health note; mọi drill-down chỉ tới aggregate hoặc masked refs.
- **DT-02:** result taxonomy phải phân biệt confirm, customer-cancel, no-answer, invalid-phone, technical, operational-blocked.
- **D-14:** IVR audit-only; reporting UI read-only, không ghi CRM/Order Core/evidence.
- **P10-4:** analytics feed phải PII-free, idempotent, có freshness/data-quality status. UI không tự tính KPI từ raw call logs nếu analytics API đã cung cấp metric.
- Export chỉ dùng aggregate/sanitized dataset; không export evidence refs có thể tái định danh nếu chưa được duyệt.

## 5. INPUTS / DEPENDENCIES
- Analytics API/view từ P10-4: KPI summary, trend series, program breakdown, script-variant breakdown, data-quality/freshness.
- Auth/RBAC từ P3-1; quyền tối thiểu `IVR_QUEUE_VIEW` hoặc permission reporting riêng nếu nền tảng bổ sung.
- UI shell/API client/filter components từ P3-1/P3-2.

## 6. BUILD STEPS
1. **Reporting route**: thêm `/reports` hoặc `/analytics` trong admin UI, guard bằng permission, reuse shell/i18n/error handling.
2. **Filter model**: time range, program (`GOLDEN_HOUR`/`TWENTY_FOUR_SEVEN`), result type, script variant, environment; encode filter vào URL query.
3. **KPI cards**: confirm rate, no-answer rate, technical rate, invalid-phone rate, attempt-2 rate, avg time-to-final, total eligible tasks.
4. **Trend charts**: line/bar theo ngày/giờ/program; so sánh GH vs 24-7; hiển thị freshness/data-quality banner từ P10-4.
5. **Breakdown tables**: result taxonomy, script variant/A-B, program/time bucket; sort/pagination; empty/error states.
6. **Privacy-safe drill-down**: chỉ drill tới masked/aggregated bucket; nếu cần link call detail thì dùng masked `order_code`/correlation ref và giữ RBAC/evidence boundary của P3-2.
7. **Export**: CSV/JSON aggregate đã sanitize; ghi audit export với reason; chặn export khi query có nguy cơ quá nhỏ/tái định danh (`min_bucket_size`).
8. **Dashboard consistency**: phân biệt rõ reporting historical vs dashboard realtime (P6-2/P3-2); không dùng reporting để quyết định dispatch live.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `admin-ui/app/reports/**` | Reporting/analytics routes |
| `admin-ui/components/reports/**` | KPI cards, charts, breakdown tables, export dialog |
| `admin-ui/lib/analytics/**` | Client + types for analytics API |
| `admin-ui/i18n/vi.json` | Chuỗi UI tiếng Việt cho reporting |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-UI-REPORT-01` | component | KPI cards render đúng metric, format %/time đúng. |
| `UT-UI-REPORT-02` | component | filter URL state round-trip; program/time/result filters gọi API đúng. |
| `UT-UI-REPORT-PII-03` | security/component | không render raw phone/full profile/payment; export không chứa PII. |
| `UT-UI-REPORT-EXPORT-04` | unit/component | export yêu cầu reason, audit call, chặn bucket nhỏ dưới `min_bucket_size`. |
| `E2E-UI-REPORT-05` | e2e | user có quyền xem report, đổi filter, thấy trend/table/freshness; user thiếu quyền bị chặn. |

Trace: `specs/testing/05`, `specs/testing/07`, `P10-4` test `BI-QUALITY-04`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] read-only; [ ] không PII; [ ] KPI lấy từ analytics API; [ ] export sanitized + audit; [ ] empty/error/freshness states rõ.
**Reviewer:** kiểm nguy cơ tái định danh qua bucket nhỏ; so sánh KPI sample với P10-4; đảm bảo không nhầm reporting historical với operational realtime.

## 10. EVIDENCE EXPECTED
Screenshot reporting overview, filter/trend/breakdown, freshness banner, export sanitized sample, PII scan/export proof, RBAC denied screenshot.

## 11. FORBIDDEN
- ❌ Render/export raw phone, full profile, address, payment, health note.
- ❌ Ghi ngược Order Core/CRM/evidence hoặc tạo admin action điều khiển call/order.
- ❌ Tự tính KPI từ raw PII logs khi P10-4 đã cung cấp aggregate.
- ❌ Export bucket quá nhỏ có nguy cơ tái định danh.

## 12. DEFINITION OF DONE
- [ ] Reporting UI + analytics client + export guard hoàn tất.
- [ ] 5 test §8 xanh.
- [ ] Evidence §10 đủ.
- [ ] P10-4 data-quality/freshness hiển thị trong UI.
