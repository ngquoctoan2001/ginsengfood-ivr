# PROMPT P0-3 — Cross-Cutting Foundation

## 0. Meta
| | |
| --- | --- |
| **ID** | `P0-3` |
| **Work ID** | `W-0012` (canonical tracker §5) |
| **Phase** | 0 — Foundation & Project Setup |
| **Prereq (blockedBy)** | `P0-1` |
| **Governance flag** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_EXECUTION_MODE=MOCK` |
| **Stack** | .NET 10 · PostgreSQL |

## 1. ROLE
Bạn là **Senior .NET Backend Engineer** phụ trách nền tảng cross-cutting. Bạn xây các "primitive" mà mọi slice nghiệp vụ sẽ dựa vào: cấu hình/secret, RBAC, audit append-only, idempotency, correlation, evidence registry, error envelope. Bạn thiết kế chúng thành middleware/service tái sử dụng, có test, fail-closed.

## 2. CONTEXT
Trước khi build intake/scheduler/callback (Phase 2), cần bộ khung ngang bảo đảm **bảo mật, truy vết, chống trùng, bằng chứng** đồng nhất. Đây là hiện thực các quyết định foundation DF-01/DF-04/DF-05/DF-06 + response model của `api/06`. Các slice sau chỉ việc "cắm vào" thay vì tự chế.

## 3. SOURCE SPECS (đọc trước)
- `specs/api/01-conventions.md`, `specs/api/06-error-codes.md` (§1b response model, §1c 16 mã `IVR_*`), `specs/api/07-idempotency-and-correlation.md`
- `specs/architecture/02-module-boundaries.md`, `specs/architecture/05-resilience.md`
- `specs/data/05-pii-policy.md`
- `plan/ivr-orther/decisions-log.md` §DF-01/DF-04/DF-05/DF-06 · §D-05 (PII)

## 4. DECISIONS & CONSTRAINTS
- **DF-01 (RBAC):** permission `IVR_QUEUE_VIEW/PAUSE/RESUME`, `IVR_SIM_ENABLE/DISABLE`, `IVR_MANUAL_RETRY`, `IVR_RESULT_REVIEW`; enforce **server-side**; admin action bắt buộc `reason` + audit.
- **DF-04 (idempotency + audit):** audit **append-only**; idempotency store dùng để chống replay (same key+payload → kết quả cũ; same key+payload khác → conflict `IVR_IDEMPOTENCY_CONFLICT`).
- **DF-05 (correlation):** `X-Correlation-Id` xuyên suốt; sinh nếu thiếu; propagate sang mọi outbound + log.
- **DF-06 (allowlist):** chỉ `X-Source-System=order-core` + token hợp lệ được gọi command intake (enforce ở P2-1, nhưng middleware allowlist dựng ở đây).
- **api/06 §1b:** response model = **200 + decision** cho outcome nghiệp vụ hợp lệ; **4xx + envelope** cho lỗi; `code` ∈ 16 mã §1c.
- **D-05 (PII):** không log raw phone; helper mask sẵn.

## 5. INPUTS / DEPENDENCIES
- Postgres (P0-1) cho bảng `ivr_audit_log`, `ivr_idempotency_keys`, `ivr_evidence` (định nghĩa migration ở P1-2; ở đây định nghĩa entity + interface, migration nối sau).
- Permission source: **reuse Permission Core** (DF-01). Ở `MOCK` dùng JWT claim hoặc header giả lập `X-Permissions`; production dùng JWT claim thật ở P4-4.
- **Ràng buộc bắt buộc:** provider đọc `X-Permissions` chỉ được đăng ký khi `executionMode == MOCK`. Ở mọi mode khác, startup **fail** nếu mock permission provider được đăng ký, và header `X-Permissions` bị **bỏ qua hoàn toàn**. Header này không nằm trong bất kỳ contract nào và không được document như public input.
- `NEED_CONFIRMATION`: secret store (env dev → Vault prod, chốt P7).

## 6. BUILD STEPS
1. **Config & secrets** (`Ivr.Infrastructure/Config`): `IvrOptions` (adapter mode, connection, feature flags gồm `RealCustomerCallAllowed=false`), bind từ env; validate on startup (fail fast nếu thiếu); secret chỉ qua env/secret-provider, không file plaintext.
2. **Correlation middleware** (`Ivr.Api`): đọc/sinh `X-Correlation-Id`; đẩy vào `ILogger` scope; expose `ICorrelationContext`; propagate qua `HttpClient` handler (DelegatingHandler) cho outbound.
3. **Error envelope** (`Ivr.Api`): exception middleware map lỗi → `{error:{code,message,details,correlationId}}` với `code` ∈ 16 mã §1c; map HTTP status theo bảng §1b/§1c; **không leak stack/PII**. Cung cấp `IvrError` factory + `ProblemDetails`-style.
4. **RBAC** (`Ivr.Api/Auth`): `IPermissionEvaluator` + `[RequirePermission("IVR_...")]` attribute/policy; enforce server-side; ở MOCK đọc permission từ claim; deny → `403 IVR_FORBIDDEN_CALLER`. Admin action ghi `reason`.
5. **Service allowlist** (`Ivr.Api/Auth`): middleware kiểm `X-Source-System` + token cho route command; caller lạ → `403 IVR_FORBIDDEN_CALLER` (dùng ở intake P2-1).
6. **Idempotency store** (`Ivr.Infrastructure`): `IIdempotencyStore` (entity `ivr_idempotency_keys{key, payload_hash, response_snapshot, created_at}`); helper `ExecuteIdempotent(key, payloadHash, factory)`: same key+hash → trả snapshot; same key+khác hash → `IVR_IDEMPOTENCY_CONFLICT` (409).
7. **Audit log** (`Ivr.Infrastructure`): `IAuditLogger` ghi **append-only** `ivr_audit_log{id, actor, action, entity_ref, reason?, correlation_id, created_at, data_json}`; không update/delete (chỉ insert). Không ghi PII thô.
8. **Evidence registry** (`Ivr.Infrastructure`): `IEvidenceStore` ghi `ivr_evidence{evidence_ref, kind, correlation_id, payload_ref, created_at}` — dùng để link signal/blocker/callback ở các phase sau (MASTER-05).
9. **PII helper** (`Ivr.Domain` hoặc shared): `Mask(phone)` → `09****1234`; guard chặn log field cấm.
10. Đăng ký DI toàn bộ; wire middleware pipeline đúng thứ tự (correlation → error envelope → mock-header guard → auth → allowlist) để lỗi ở mọi tầng downstream đều được chuẩn hóa.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Infrastructure/Configuration/IvrOptions.cs` | Options + validation |
| `src/Ivr.Api/Middleware/CorrelationMiddleware.cs`, `ErrorEnvelopeMiddleware.cs` | Correlation + error |
| `src/Ivr.Api/Auth/*` | RBAC evaluator, RequirePermission, allowlist middleware |
| `src/Ivr.Infrastructure/Idempotency/*`, `Audit/*`, `Evidence/*` | Store + entity + interface |
| `src/Ivr.Domain/Privacy/PiiMasker.cs` | Mask helper |
| `src/Ivr.Api/Program.cs` | Wire DI + pipeline |

**Chuẩn output:** interface-first (test được bằng in-memory fake); mọi mã lỗi dùng const enum khớp §1c (không literal string rải rác).

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-FND-IDEMP-01` | unit | same key+payload → snapshot cũ; khác payload → 409 `IVR_IDEMPOTENCY_CONFLICT`. |
| `UT-FND-CORR-02` | unit | thiếu `X-Correlation-Id` → sinh mới; có → giữ nguyên; propagate outbound. |
| `UT-FND-RBAC-03` | unit | thiếu permission → `403 IVR_FORBIDDEN_CALLER`; đủ → pass. |
| `UT-FND-RBAC-08` | unit | `executionMode != MOCK` → header `X-Permissions` bị bỏ qua và request trả 403; đăng ký mock permission provider ngoài MOCK → startup fail. |
| `UT-FND-ALLOW-04` | unit | `X-Source-System` sai → 403; đúng+token → pass. |
| `UT-FND-ERR-05` | unit | exception → envelope `{error:{code,message,details,correlationId}}`, status khớp §1b/§1c, không leak stack/PII. |
| `UT-FND-AUDIT-06` | unit | audit chỉ insert (update/delete ném lỗi); có `correlationId`; không chứa phone thô. |
| `UT-FND-PII-07` | unit | `Mask("0912341234")` = `09****1234`; guard chặn log field cấm. |

Trace: `specs/testing/02` (idempotency/foundation), `specs/testing/07` (SEC-01..03 auth/PII).

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:**
- [ ] 7 test §8 xanh.
- [ ] Mã lỗi 100% dùng enum §1c; không literal.
- [ ] Audit append-only thật sự (không có path update/delete).
- [ ] Không log PII (kiểm bằng test + review).

**Reviewer:** thứ tự middleware đúng (correlation trước để mọi log/lỗi có id); idempotency conflict semantics đúng; RBAC enforce server-side (không tin client).

## 10. EVIDENCE EXPECTED
Test report 7 pass; sample envelope 403/409/500 (mask PII); audit log insert-only proof; correlation propagate log (inbound=outbound id).

## 11. FORBIDDEN
- ❌ Log/persist raw phone, dial_token, recording (D-05).
- ❌ Audit cho phép update/delete.
- ❌ Trust permission từ client mà không verify server-side.
- ❌ Mã lỗi ngoài 16 mã §1c.

## 12. DEFINITION OF DONE
- [ ] Toàn bộ primitive hoạt động + 7 test xanh trong CI (P0-2).
- [ ] Pipeline middleware wire đúng; DI đăng ký.
- [ ] Evidence §10 đủ; không vi phạm Forbidden.
