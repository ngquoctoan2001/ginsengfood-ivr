# PROMPT P0-4 — Feature-Flag & Dynamic Config Platform

## 0. Meta
| | |
| --- | --- |
| **ID** | `P0-4` · **Phase** 0 — Foundation |
| **Work ID** | `W-0013` (canonical tracker §5) |
| **Prereq** | `P0-3` |
| **Governance** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_EXECUTION_MODE=MOCK` |
| **Stack** | .NET 10 · PostgreSQL |

## 1. ROLE
Bạn là **Senior Platform Engineer**. Bạn xây hệ **feature-flag + config động** là xương sống cho toàn bộ chiến lược "target vs live" và governance: bật/tắt tính năng theo môi trường, an toàn (default off), có audit và kill-switch. Nhiều prompt sau (P4-1/3, P7-3/4, P8-2, P9-1) *tham chiếu* flag — prompt này *xây* chúng.

## 2. CONTEXT
Target V1 cần tách execution mode, Sales provider, telephony provider, policy approval và real-call permission. Cần một cấu hình typed/audited; compatibility flags không được biến thành business truth.

## 3. SOURCE SPECS (đọc trước)
- `prompt/README-governance.md` §2 (bất biến), §6 (ladder→env)
- `specs/architecture/04-deployment-architecture.md`
- `plan/ivr-orther/decisions-log.md` §DF-03 (gate), §DS-03/04 (target flags), §DC-06 · `plan/ivr-orther/production-blockers-plan.md` §B (flag mapping)

## 4. DECISIONS & CONSTRAINTS
- **Default an toàn:** mọi flag rủi ro default **off**; `REAL_CUSTOMER_CALL_ALLOWED` không bật được ngoài prod-approved (P7-3/P9-1).
- **Config/flag danh mục:** `executionMode=MOCK|LAB_REAL_SIM|PRODUCTION_REAL`, `salesProvider=FAKE_TARGET_V1|CURRENT_GOLDEN_HOUR_COMPAT|TARGET_V1`, `simProvider=MOCK|VENDOR`, `attemptPolicyVersion`, `realCustomerCallAllowed`, `labDestinationAllowlist`, `globalDialKillSwitch`, `v1NotificationEnabled=false` (immutable guard).
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
4. Admin API đổi flag với **permission tường minh** (không dùng "RBAC" chung chung):
   - `IVR_FLAG_READ` — đọc flag.
   - `IVR_RUNTIME_GATE_ADMIN` (**`OD-V1-20` đã duyệt 2026-08-22 — cấp cho role `Admin`; four-eyes chưa ký, và `IRuntimeGateAuthorization` production vẫn `false` nên mutation trả `409`**) — bắt buộc cho `labDestinationAllowlist`, `globalDialKillSwitch`, `executionMode`, `realCustomerCallAllowed`.
   - Mọi mutation cần `reason` + audit append-only; **four-eyes** (actor đề xuất ≠ actor phê duyệt) cho nhóm `IVR_RUNTIME_GATE_ADMIN`.
   - Actor thực hiện call **không** được tự mở rộng allowlist cho chính đích mình sắp gọi (self-authorization bị chặn).
   - UI dùng ở P3-3.
5. Guardrail (fail-closed khi config/audit provider unavailable): reject invalid mode/provider pairs, real adapter in MOCK, non-allowlisted LAB destination, candidate policy in PROD, notification enable, và `realCustomerCallAllowed` without P9 gate.
6. **Bất đối xứng theo chiều an toàn** (thay cho "immutable" tuyệt đối — một kill switch không bật được trong sự cố thì vô dụng):

   | Control | Chiều **an toàn hơn** | Chiều **nguy hiểm hơn** |
   | --- | --- | --- |
   | `globalDialKillSwitch` | **BẬT (ON)** — luôn cho phép ở **mọi** environment kể cả `PRODUCTION_REAL`, chỉ cần `IVR_RUNTIME_GATE_ADMIN` + `reason` + audit. **Không** four-eyes, **không** chờ deployment. | **TẮT (OFF)** — cần `IVR_RUNTIME_GATE_ADMIN` + four-eyes + `reason`; ở `PRODUCTION_REAL` chỉ qua deployment có approval (P7-3/P9-1). |
   | `labDestinationAllowlist` | **thu hẹp / làm rỗng** — cho phép ngay với `IVR_RUNTIME_GATE_ADMIN` + audit. | **mở rộng** — four-eyes + `reason`; ở `PRODUCTION_REAL` chỉ qua deployment có approval. |
   | `realCustomerCallAllowed` | **false** — cho phép ngay (đây là kill switch cấp cao). | **true** — chỉ qua P9-1 sau DF-03; endpoint admin **luôn** từ chối. |
   | `v1NotificationEnabled`, `recordingEnabled` | giữ `false` | bật lên — endpoint admin **luôn** từ chối ở mọi mode (immutable guard, cần owner+legal decision riêng). |

   Nguyên tắc: **hành động làm giảm rủi ro không bao giờ bị chặn; hành động làm tăng rủi ro luôn bị gate.** Fail-closed khi không xác định được chiều.
7. **Kill switch thắng mọi thứ:** khi `globalDialKillSwitch=ON`, scheduler/dialer dừng dispatch ngay kể cả khi allowlist và mode hợp lệ, và kể cả khi audit/config provider đang lỗi (trạng thái không đọc được ⇒ coi như ON).
8. Thêm `recordingEnabled=false` làm **immutable guard** cạnh `v1NotificationEnabled=false` (DT-05).
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
| `UT-FLAG-AUDIT-03` | unit | đổi flag ghi audit + reason (ai/cũ→mới); audit append-only, không UPDATE/DELETE. |
| `UT-FLAG-AUTHZ-05` | unit | thiếu `IVR_RUNTIME_GATE_ADMIN` → 403 khi sửa `labDestinationAllowlist`/`globalDialKillSwitch`. |
| `UT-FLAG-ALLOWLIST-06` | unit | không thể mở rộng allowlist im lặng: mọi thay đổi cần reason + four-eyes + audit; self-authorization bị từ chối. |
| `IT-FLAG-PRODGUARD-07` | integration | ở `PRODUCTION_REAL`: bật kill switch **thành công**; tắt kill switch **bị từ chối** (cần deployment); thu hẹp allowlist **thành công**; mở rộng allowlist **bị từ chối**; bật notification/recording **bị từ chối**. |
| `IT-FLAG-EMERGENCY-10` | integration | Trong sự cố, on-call có `IVR_RUNTIME_GATE_ADMIN` bật được kill switch ở `PRODUCTION_REAL` **không** cần four-eyes và **không** cần deploy; hành động được audit đầy đủ. |
| `IT-FLAG-KILLSWITCH-08` | integration | kill switch ON → dispatch bị chặn dù mode/allowlist hợp lệ. |
| `IT-FLAG-FAILCLOSED-09` | integration | config/audit provider down → fail-closed (không dispatch); không đọc được `globalDialKillSwitch` ⇒ coi như ON; không fallback sang default cho phép gọi. |
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
