# PROMPT P3-1 — Admin UI Foundation (Next.js)

## 0. Meta
| | |
| --- | --- |
| **ID** | `P3-1` · **Phase** 3 — Admin UI |
| **Work ID** | `W-0025` (canonical tracker §5) |
| **Prereq** | `P0-3`, `P2-1`, `P2-8` (backend admin API) |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | Next.js (React/TS) · gọi `Ivr.Api` |

## 1. ROLE
Bạn là **Senior Frontend Engineer (Next.js/TypeScript)**. Bạn dựng nền admin UI: auth/RBAC, layout, API client type-safe, i18n tiếng Việt, xử lý loading/error nhất quán. UI của bạn **chỉ giám sát + admin action có kiểm soát**, tuyệt đối không bypass Order Core.

## 2. CONTEXT
Ops/admin cần màn theo dõi hàng đợi IVR, xem call log/evidence, cấu hình. Đây là bước nền cho các màn nghiệp vụ (P3-2/3-3). UI gọi `Ivr.Api` (đã có health + intake; các API admin bổ sung dần). RBAC `IVR_*` enforce **server-side** (P0-3) — UI chỉ ẩn/hiện, không tự quyết quyền.

## 3. SOURCE SPECS (đọc trước)
- `specs/ui/00-index.md`, `specs/ui/08-role-permission-ui.md`
- `specs/api/03-admin-api.md`, `specs/api/06-error-codes.md` (envelope render)
- `plan/ivr-orther/decisions-log.md` §DF-01 (RBAC) · §D-02 (không bypass Core) · §D-05 (PII mask) · §DTS-03

## 4. DECISIONS & CONSTRAINTS
- **DF-01:** permission `IVR_*`; UI đọc permission từ token/claim, ẩn action không đủ quyền — nhưng **server vẫn enforce** (UI không phải nguồn quyền).
- **D-02:** UI không có nút "confirm/cancel order"; chỉ admin action IVR (pause/resume queue, manual retry, result review) — mọi action có `reason` + audit.
- **D-05:** hiển thị phone **masked**; không render số thật/recording.
- **DTS-03:** i18n `vi`; TypeScript strict.

## 5. INPUTS / DEPENDENCIES
- `Ivr.Api` (P0-1/P0-3): health, error envelope, auth.
- Auth provider: reuse platform SSO/JWT (mock ở dev — `NEED_CONFIRMATION`).
- Design: component library (default Tailwind + headless — `NEED_CONFIRMATION`).

## 6. BUILD STEPS
1. App Router layout: shell (nav, header, môi trường badge "MOCK/dev"), theme, responsive.
2. **Auth**: login/session (JWT/SSO mock dev), lưu token an toàn (httpOnly cookie), refresh; guard route theo permission.
3. **RBAC client**: `usePermissions()` + `<RequirePermission perm="IVR_QUEUE_VIEW">`; ẩn action không đủ quyền (server vẫn chặn).
4. **API client** type-safe: sinh từ OpenAPI (P1-1) hoặc fetch wrapper; tự đính `X-Correlation-Id`; parse error envelope → toast/inline chuẩn (`code`+message).
5. **i18n vi**: resource string, format ngày/tiền VN.
6. **UX chung**: loading skeleton, error boundary, empty state, confirm dialog cho admin action (bắt nhập `reason`).
7. Env banner + governance notice (REAL_CALL=NO) hiển thị rõ.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `admin-ui/app/**` | Layout, auth, route guard |
| `admin-ui/lib/api/**` | API client + error envelope parser |
| `admin-ui/lib/auth/**`, `rbac/**` | Session + permission |
| `admin-ui/i18n/vi.json` | Chuỗi tiếng Việt |
| `admin-ui/components/**` | Shell, RequirePermission, dialogs, states |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-UI-RBAC-01` | component | thiếu permission → action ẩn; có → hiện. |
| `UT-UI-ERR-02` | component | error envelope `{error:{code,...}}` → render message + code đúng. |
| `UT-UI-CORR-03` | unit | mọi request đính `X-Correlation-Id`. |
| `UT-UI-PII-04` | component | phone luôn masked; không render số thật. |
| `E2E-UI-AUTH-05` | e2e | chưa login → redirect; login → vào được; logout. |

Trace: `specs/testing/05-e2e-test-plan.md`, `specs/ui/*`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] không có action transition order; [ ] PII masked; [ ] RBAC ẩn+server enforce; [ ] i18n vi; [ ] error envelope chuẩn.
**Reviewer:** token lưu an toàn; correlation propagate; strict TS không `any`.

## 10. EVIDENCE EXPECTED
Screenshot shell + login flow, RBAC ẩn/hiện demo, error-envelope render, masked phone, correlation header network capture.

## 11. FORBIDDEN
- ❌ Nút confirm/cancel order (D-02). ❌ Render số thật/recording (D-05). ❌ Tin quyền từ client-only (server enforce). ❌ Bật ở prod khi chưa qua release gate.

## 12. DEFINITION OF DONE
- [ ] UI foundation + auth/RBAC/i18n/error; 5 test §8 xanh; evidence §10 đủ.
