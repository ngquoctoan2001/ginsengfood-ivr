# PROMPT P0-4 — Feature-Flag & Dynamic Config Platform

## 0. Meta
| | |
| --- | --- |
| **ID** | `P0-4` · **Phase** 0 — Foundation |
| **Prereq** | `P0-3` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_ADAPTER_MODE=MOCK` |
| **Stack** | .NET 10 · PostgreSQL |

## 1. ROLE
Bạn là **Senior Platform Engineer**. Bạn xây hệ **feature-flag + config động** là xương sống cho toàn bộ chiến lược "target vs live" và governance: bật/tắt tính năng theo môi trường, an toàn (default off), có audit và kill-switch. Nhiều prompt sau (P4-1/3, P7-3/4, P8-2, P9-1) *tham chiếu* flag — prompt này *xây* chúng.

## 2. CONTEXT
Bộ specs có nhiều "target vs implemented" (order_version race-guard, richCallbackCodes, trustResolver, richDoNotCall) + governance gate (`REAL_CUSTOMER_CALL_ALLOWED`, pilot limits, kill-switch). Nếu rải `if` rời rạc sẽ loạn. Cần **1 hệ flag** thống nhất: nguồn config, đánh giá, audit, thay đổi runtime an toàn.

## 3. SOURCE SPECS (đọc trước)
- `prompt/README-governance.md` §2 (bất biến), §6 (ladder→env)
- `specs/architecture/04-deployment-architecture.md`
- `plan/ivr-orther/decisions-log.md` §DF-03 (gate), §DS-03/04 (target flags), §DC-06 · `plan/ivr-orther/production-blockers-plan.md` §B (flag mapping)

## 4. DECISIONS & CONSTRAINTS
- **Default an toàn:** mọi flag rủi ro default **off**; `REAL_CUSTOMER_CALL_ALLOWED` không bật được ngoài prod-approved (P7-3/P9-1).
- **Flag danh mục:** `orderVersionRaceGuard`, `richCallbackCodes`, `noAnswerTransition` (OC3), `trustResolver` (DC-06), `richDoNotCall` (IR-CRM-01), `pilotMode`+limits, `realCustomerCallAllowed`, `simAdapterMode`.
- **Audit:** đổi flag = admin action có `reason` + audit (DF-04); ai/bao giờ/giá trị cũ→mới.
- **Runtime-safe:** thay đổi có hiệu lực nhanh nhưng không gây trạng thái nửa vời; kill-switch (REAL→MOCK) tức thời (nối P8-2).
- **Scope theo env** (dev/staging/pilot/prod) + guardrail (không cho bật flag nguy hiểm ở env thấp).

## 5. INPUTS / DEPENDENCIES
- Foundation P0-3 (config, audit); Postgres (flag store) hoặc config provider.
- `NEED_CONFIRMATION`: dùng flag lib (OpenFeature/.NET) hay tự xây bảng `ivr_feature_flags` — default tự xây gọn + interface OpenFeature-compatible.

## 6. BUILD STEPS
1. `IFeatureFlags` + `IDynamicConfig`: đọc flag/config theo env + context; cache + refresh; fail-safe (không đọc được → default off/an toàn).
2. Flag store (`ivr_feature_flags{key, env, enabled, value_json, updated_by, updated_at, reason}`) + seed default per-env (guardrail).
3. **Kill-switch primitive**: `IKillSwitch.RealCallsEnabled` — flip → propagate nhanh; verify propagation API.
4. Admin API (RBAC + audit + reason) đổi flag; UI dùng ở P3-3.
5. Guardrail: từ chối bật `realCustomerCallAllowed`/`simAdapterMode=REAL` ngoài env cho phép + thiếu điều kiện (P9-1 gate).
6. Đăng ký DI; wire vào các consumer (chuẩn cho P4/P7/P8).

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Infrastructure/FeatureFlags/**` | IFeatureFlags/IDynamicConfig + store |
| `src/Ivr.Infrastructure/FeatureFlags/KillSwitch.cs` | Kill-switch |
| `src/Ivr.Api/Admin/FeatureFlagEndpoint.cs` | Admin đổi flag (audit+reason) |
| migration `ivr_feature_flags` | Store |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-FLAG-DEFAULT-01` | unit | flag rủi ro default off; đọc lỗi → an toàn. |
| `UT-FLAG-GUARD-02` | unit | không bật `realCustomerCallAllowed` ngoài prod-approved (guardrail). |
| `UT-FLAG-AUDIT-03` | unit | đổi flag ghi audit + reason (ai/cũ→mới). |
| `IT-FLAG-KILL-04` | integration | kill-switch REAL→MOCK propagate nhanh; verify. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:** [ ] default an toàn; [ ] guardrail env; [ ] audit+reason; [ ] kill-switch verify.
**Reviewer:** không flag nào bật gọi thật lén; interface đủ cho P4/P7/P8; fail-safe.

## 10. EVIDENCE EXPECTED
Flag store + seed default, guardrail-block demo, audit records, kill-switch propagation test.

## 11. FORBIDDEN
- ❌ Flag rủi ro default on. ❌ Bật gọi thật ngoài gate (DF-03/P9-1). ❌ Đổi flag không audit/reason. ❌ Rải `if` flag rời rạc thay vì hệ tập trung.

## 12. DEFINITION OF DONE
- [ ] Flag/config platform + kill-switch + admin/audit; 4 test §8 xanh; evidence §10 đủ.
