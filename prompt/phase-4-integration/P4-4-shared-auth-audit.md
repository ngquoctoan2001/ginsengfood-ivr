# PROMPT P4-4 — Shared Auth, Service Identity & Audit Federation

## 0. Meta
| | |
| --- | --- |
| **ID** | `P4-4` · **Phase** 4 — Real Integration |
| **Prereq** | `P0-3` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · mTLS/JWT |

## 1. ROLE
Bạn là **Senior Security/Platform Engineer**. Bạn thay auth mock (P0-3) bằng **service identity thật**: allowlist Order Core cho command intake, service credential cho outbound (Core/ops/CRM), mTLS/JWT, và audit federation. Bạn đảm bảo least-privilege và fail-closed.

## 2. CONTEXT
P0-3 dựng RBAC/allowlist với mock permission. Để tích hợp thật (P4-1/2/3) an toàn, cần service-to-service auth thật khớp foundation platform (DF-06): chỉ Order Core gọi được `POST /tasks`; SIM adapter không có order-write cred; Order Core service-cred có `SellableCheck`/`RecallHoldView`.

## 3. SOURCE SPECS (đọc trước)
- `specs/api/07-idempotency-and-correlation.md`, `specs/architecture/03-integration-architecture.md`, `integration-requirements/04-shared-auth-audit-requirements.md`
- `plan/ivr-orther/decisions-log.md` §DF-01/DF-04/DF-05/DF-06 · §D-05

## 4. DECISIONS & CONSTRAINTS
- **DF-06:** allowlist = **Order Core** cho `POST /tasks`; SIM adapter **không** order-write cred; Order Core service-cred có `SellableCheck`/`RecallHoldView` (DO-03).
- **DF-01:** RBAC `IVR_*` enforce server-side qua Permission Core thật.
- **DF-04/DF-05:** audit append-only + correlation propagate qua boundary; audit federation (liên kết correlation xuyên service).
- **Least-privilege + fail-closed:** thiếu/không hợp lệ credential → 401/403; không "mở cửa" khi auth service down.
- **D-05:** credential/secret không log; secret qua secret store (P7).

## 5. INPUTS / DEPENDENCIES
- Platform SSO/identity provider (JWT/OIDC) + service credential issuance; mTLS certs (nếu dùng).
- Permission Core thật (DF-01).

## 6. BUILD STEPS
1. Thay mock permission (P0-3) bằng **Permission Core** thật: `IPermissionEvaluator` gọi/verify token thật; cache có TTL; fail-closed nếu không verify được.
2. **Service identity**: outbound client (Core/ops/CRM) dùng service credential (client-credentials/mTLS); rotate-friendly (đọc secret runtime).
3. **Allowlist thật** cho `POST /tasks`: verify caller = Order Core (mTLS subject / JWT `azp`/`X-Source-System` + token); khác → `403 IVR_FORBIDDEN_CALLER`.
4. **Audit federation**: propagate + persist correlation xuyên service; audit action admin + service call quan trọng.
5. Secret handling: đọc từ secret store (env dev; Vault/KMS prod — nối P7); không hardcode/log.
6. Threat cases: token hết hạn/không scope/replay → deny + audit.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Api/Auth/PermissionCoreEvaluator.cs` | RBAC thật |
| `src/Ivr.Infrastructure/Auth/ServiceCredentialProvider.cs` | Outbound cred/mTLS |
| `src/Ivr.Api/Auth/OrderCoreAllowlist.cs` | Allowlist thật |
| `src/Ivr.Infrastructure/Audit/AuditFederation.cs` | Correlation xuyên service |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `SEC-AUTH-01` | security | caller ≠ Order Core → 403; đúng identity+scope → pass. |
| `SEC-AUTH-02` | security | token hết hạn/thiếu scope/replay → deny + audit. |
| `SEC-AUTH-03` | security | auth service down → fail-closed (không mở cửa). |
| `SEC-AUTH-04` | security | secret không xuất hiện trong log; outbound dùng service cred. |
| `IT-AUTH-CORR-05` | integration | correlation propagate + audit federation xuyên service. |

Trace: `specs/testing/07-security-privacy-test-plan.md`, `integration-requirements/04`.

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] least-privilege; [ ] allowlist thật; [ ] fail-closed auth; [ ] secret không log; [ ] audit federation.
**Reviewer:** SIM adapter không order-write cred; scope Order Core đúng (SellableCheck/RecallHoldView); rotate-friendly.

## 10. EVIDENCE EXPECTED
Auth deny/allow samples, expired/replay deny+audit, fail-closed demo, secret-not-logged scan, correlation federation trace.

## 11. FORBIDDEN
- ❌ Cho SIM adapter cred order-write (DF-06). ❌ Mở cửa khi auth down. ❌ Hardcode/log secret (D-05). ❌ Trust client-side quyền.

## 12. DEFINITION OF DONE
- [ ] Auth thật + service identity + audit federation; 5 test §8 xanh; evidence §10 đủ. **Kết thúc Phase 4: tích hợp thật fail-closed (vẫn MOCK SIM).**
