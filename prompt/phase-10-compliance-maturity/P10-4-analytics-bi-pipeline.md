# PROMPT P10-4 — Analytics / BI Pipeline

## 0. Meta
| | |
| --- | --- |
| **ID** | `P10-4` · **Phase** 10 — Compliance & Maturity |
| **Work ID** | `W-0055` (canonical tracker §5) |
| **Prereq** | `P6-1` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` |
| **Stack** | .NET 10 · data pipeline · warehouse |

## 1. ROLE
Bạn là **Senior Data Engineer**. Bạn xây pipeline đưa outcome cuộc gọi IVR vào kho phân tích để đo KPI nghiệp vụ (tỉ lệ xác nhận, no-answer, technical, theo program/thời gian), **privacy-safe** (aggregate, không PII). Bạn cấp dữ liệu cho reporting UI (P3-4) và ra quyết định vận hành.

## 2. CONTEXT
Dashboard vận hành (P6-2) là real-time kỹ thuật; nhưng phân tích xu hướng nghiệp vụ (tối ưu Golden Hour, hiệu quả script A/B, tỉ lệ đơn ảo giảm bao nhiêu) cần data pipeline + warehouse riêng. Đây là nền cho reporting UI (P3-4) và cải tiến liên tục.

## 3. SOURCE SPECS (đọc trước)
- `specs/architecture/06-observability.md`, `specs/data/05-pii-policy.md`, `specs/functional/05-result-normalization-callback.md`
- `plan/ivr-orther/decisions-log.md` §D-05 (PII), §DT-02 (result taxonomy), §D-14 (audit-only) · `P2-7` (script A/B variant)

## 4. DECISIONS & CONSTRAINTS
- **Privacy-safe:** chỉ aggregate/anonymized vào warehouse; **không** phone/PII; theo `order_ref`/hashed, không định danh khách.
- **KPI nghiệp vụ:** confirm rate, no-answer/technical/invalid rate, attempt-2 rate, avg time-to-confirm, per program/hour, per script variant (A/B — P2-7).
- **Không thay evidence/audit** (D-14 IVR audit-only); pipeline là phái sinh, read-only từ result/event.
- **Idempotent ETL**; late/duplicate event xử lý đúng.

## 5. INPUTS / DEPENDENCIES
- Result/event stream (P2-5/P2-6, P6-1); warehouse (`NEED_CONFIRMATION`: Postgres analytics schema / ClickHouse / BigQuery); BI tool.

## 6. BUILD STEPS
1. **ETL/stream**: từ result/event → fact table (call outcome) + dimension (program/time/script-variant), **strip PII** (chỉ ref/hash + aggregate).
2. **KPI models**: định nghĩa metric nghiệp vụ; materialized view/aggregate.
3. **Privacy filter**: guard chặn PII vào warehouse (test).
4. **Idempotent ETL**: dedupe, late-arrival, replay-safe.
5. **Serve**: API/view cho reporting UI (P3-4) + BI tool.
6. Data quality checks (row counts, freshness).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Analytics/**` hoặc `pipeline/**` | ETL/stream |
| `db/analytics/**` | Fact/dimension schema (PII-free) |
| `docs/kpi-catalog.md` | Định nghĩa KPI nghiệp vụ |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `BI-PII-01` | security | warehouse không chứa PII (chỉ ref/hash/aggregate — D-05). |
| `BI-KPI-02` | unit | KPI (confirm/no-answer/technical rate per program/hour/variant) tính đúng. |
| `BI-IDEMP-03` | integration | ETL idempotent; late/duplicate event → không double-count. |
| `BI-QUALITY-04` | integration | data quality (freshness/row-count) đạt; feed P3-4. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] PII-free warehouse; [ ] KPI đúng; [ ] idempotent ETL; [ ] read-only (không thay audit/evidence).
**Reviewer:** aggregate không tái định danh; A/B variant đo được; freshness đủ cho reporting.

## 10. EVIDENCE EXPECTED
Pipeline run, PII-free scan, KPI sample vs raw, idempotency test, data-quality report.

## 11. FORBIDDEN
- ❌ PII vào warehouse (D-05). ❌ Pipeline ghi ngược audit/evidence (D-14 read-only). ❌ Double-count late/duplicate. ❌ Tái định danh khách từ aggregate.

## 12. DEFINITION OF DONE
- [ ] ETL + KPI + privacy filter + serve; 4 test §8 xanh; evidence §10 đủ.
