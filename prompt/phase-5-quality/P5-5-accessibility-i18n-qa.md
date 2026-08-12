# PROMPT P5-5 — Accessibility, i18n & Cross-Browser QA

## 0. Meta
| | |
| --- | --- |
| **ID** | `P5-5` · **Phase** 5 — Quality Engineering |
| **Prereq** | `P3-*` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | Next.js · Playwright/axe · visual regression |

## 1. ROLE
Bạn là **Senior Frontend QA / Accessibility Engineer**. Bạn đảm bảo admin UI dùng được cho mọi người vận hành: đạt chuẩn a11y (WCAG), tiếng Việt đúng ngữ cảnh, chạy nhất quán trên nhiều trình duyệt/độ phân giải, và không hồi quy giao diện.

## 2. CONTEXT
Admin UI (P3-*) là công cụ vận hành hằng ngày của ops. UI khó dùng/không tiếp cận được = lỗi vận hành, bỏ sót cảnh báo. Prompt này bổ sung lớp QA chuyên biệt cho UI mà unit/e2e chức năng (P5-2) không phủ.

## 3. SOURCE SPECS (đọc trước)
- `specs/ui/00-index.md` (toàn bộ màn), `specs/ui/08-role-permission-ui.md`
- `plan/ivr-orther/decisions-log.md` §DTS-03 (i18n vi), §D-05 (PII masked)

## 4. DECISIONS & CONSTRAINTS
- **A11y:** WCAG 2.1 AA (keyboard nav, contrast, ARIA, focus, screen-reader labels).
- **i18n:** tiếng Việt đầy đủ, format ngày/tiền VN; không hardcode string; không lỗi thiếu key.
- **Cross-browser/responsive:** Chrome/Edge/Firefox + desktop/tablet; layout không vỡ.
- **Visual regression:** snapshot các màn chính; PII vẫn masked ở mọi viewport.

## 5. INPUTS / DEPENDENCIES
- Admin UI (P3-*); axe-core, Playwright, visual-regression tool (Playwright snapshots/Percy — `NEED_CONFIRMATION`).

## 6. BUILD STEPS
1. **A11y automated**: axe trên mọi màn chính; fail nếu vi phạm nghiêm trọng; keyboard-only nav test.
2. **i18n QA**: kiểm thiếu key, chuỗi hardcode, format VN; test đổi locale không vỡ.
3. **Cross-browser/responsive**: Playwright chạy đa browser + viewport; layout assertions.
4. **Visual regression**: baseline snapshot các màn; diff trong CI; mask PII.
5. Tích hợp CI (nối P5-4 gate); báo cáo a11y/visual.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `tests/ui-a11y/**` | axe + keyboard nav |
| `tests/ui-visual/**` | visual regression baseline |
| `deploy/ci/ui-qa.yml` | Job a11y/i18n/visual |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UI-A11Y-01` | a11y | màn chính pass axe (no serious); keyboard nav đầy đủ. |
| `UI-I18N-02` | ui | không thiếu key/hardcode; format ngày/tiền VN. |
| `UI-XBROWSER-03` | ui | Chrome/Edge/Firefox + tablet: layout không vỡ. |
| `UI-VISUAL-04` | visual | snapshot khớp baseline; PII masked mọi viewport. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] WCAG AA; [ ] i18n đủ; [ ] đa browser/viewport; [ ] visual + PII mask.
**Reviewer:** a11y thực dùng được keyboard/screen-reader; baseline hợp lý.

## 10. EVIDENCE EXPECTED
axe report, keyboard-nav demo, i18n coverage, cross-browser matrix, visual diff report.

## 11. FORBIDDEN
- ❌ Bỏ qua a11y nghiêm trọng. ❌ Hardcode chuỗi (phải i18n). ❌ PII lộ ở bất kỳ viewport. 

## 12. DEFINITION OF DONE
- [ ] A11y + i18n + cross-browser + visual; 4 test §8 xanh CI; evidence §10 đủ.
