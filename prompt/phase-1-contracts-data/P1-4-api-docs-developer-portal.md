# PROMPT P1-4 — API Docs & Developer Portal

## 0. Meta
| | |
| --- | --- |
| **ID** | `P1-4` · **Phase** 1 — Contracts & Data |
| **Prereq** | `P1-1` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | OpenAPI · docs tooling |

## 1. ROLE
Bạn là **Developer Experience / API Docs Engineer**. Bạn biến OpenAPI thành tài liệu API sống, changelog contract, và portal cho team tích hợp (Order Core/ops/CRM) hiểu contract IVR. Bạn giữ docs đồng bộ với spec (single source), có versioning/deprecation policy.

## 2. CONTEXT
IVR là ranh giới tích hợp nhiều team. Contract không có tài liệu rõ = tích hợp sai/chậm. Cần docs sinh từ OpenAPI (P1-1), có changelog khi contract đổi, và policy versioning/deprecation để không phá downstream. Non-prod portal cho dev.

## 3. SOURCE SPECS (đọc trước)
- `specs/api/00-index.md`, `specs/api/01-conventions.md`, `specs/api/06-error-codes.md`, `specs/api/openapi/ivr-order-confirmation.v1.yaml`
- `specs/api/05-order-core-contracts.md` (caveat DS-03/04 — ghi rõ target vs live trong docs)
- `plan/ivr-orther/decisions-log.md` §DF-02

## 4. DECISIONS & CONSTRAINTS
- **Single source:** docs sinh từ OpenAPI; không viết tay lệch spec.
- **Target vs live:** đánh dấu rõ endpoint/field target (order_version, `CALLBACK_*`) vs live (200/422) — DS-03/04.
- **Versioning:** SemVer contract; deprecation policy (Sunset header, thời gian ân hạn).
- **Non-prod:** portal chỉ non-prod; không lộ ví dụ chứa PII thật.

## 5. INPUTS / DEPENDENCIES
- OpenAPI + generated (P1-1); changelog tooling; docs renderer (Redoc/Scalar/Backstage — `NEED_CONFIRMATION`).

## 6. BUILD STEPS
1. Render docs từ OpenAPI (Redoc/Scalar static) — host non-prod; ví dụ request/response (mask PII).
2. **Contract changelog**: tự sinh diff giữa version (oasdiff) → `docs/api-changelog.md`; breaking-change cảnh báo.
3. **Versioning/deprecation policy** `docs/api-versioning.md`: SemVer, Sunset header, ân hạn.
4. Trang "integration guide" cho Order Core/ops/CRM: cách gọi task/callback, auth, idempotency, error codes §1c, target vs live.
5. CI: publish docs khi merge; fail nếu docs lệch spec (drift từ P1-1).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `docs/api/**` (rendered) | API reference |
| `docs/api-changelog.md`, `docs/api-versioning.md` | Changelog + policy |
| `docs/integration-guide.md` | Hướng dẫn team tích hợp |
| `deploy/ci/docs.yml` | Publish + drift check |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `CT-DOC-01` | ci | docs sinh từ OpenAPI; drift (docs≠spec) → fail. |
| `CT-DOC-02` | ci | oasdiff phát hiện breaking change → cảnh báo. |
| `UT-DOC-PII-03` | unit | ví dụ trong docs không chứa PII thật. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] docs = spec (drift-check); [ ] target vs live rõ; [ ] versioning policy; [ ] non-prod + no PII.
**Reviewer:** integration guide đủ để team khác dùng; deprecation an toàn.

## 10. EVIDENCE EXPECTED
Rendered docs, changelog diff sample, drift-check fail demo, integration guide.

## 11. FORBIDDEN
- ❌ Docs viết tay lệch spec. ❌ PII thật trong ví dụ. ❌ Portal ở prod. ❌ Breaking change không changelog/deprecation.

## 12. DEFINITION OF DONE
- [ ] Docs + changelog + versioning + integration guide; 3 test §8 xanh; evidence §10 đủ.
