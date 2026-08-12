# REVIEW — Open Decisions Register

Trạng thái: `OPEN` · Cập nhật: `2026-08-12` (bổ sung `OD-V1-13..21` từ red-team remediation W-0062). Không đóng bằng suy luận.

> Mock/fake fixture **không bao giờ** đóng một dòng nào trong bảng này. Mock chỉ cho phép code tiếp tục.

## P0 — real Sales integration/business acceptance

| ID | Decision/data | Owner | Current | Closure evidence |
| --- | --- | --- | --- | --- |
| `OD-V1-01` | program/payment/IVR-required/callable matrix | Sales Product/Core | target proposal only | signed matrix + producer tests |
| `OD-V1-02` | generic callback path and ACK taxonomy | Sales API/Core | GH-specific endpoint only | OpenAPI + contract tests |
| `OD-V1-03` | order version exposure/bump/stale behavior | Sales Core | version internal/partial | DTO + stale tests |
| `OD-V1-04` | speech-safe summary schema/content/item limits | Sales/Product/Privacy | not implemented | schema + samples + approval |
| `OD-V1-05` | dial-token issue/resolve/TTL/one-use | Sales/Security/Telephony | not established | API/threat model/tests |
| `OD-V1-06` | no-answer/timeout/revalidation semantics | Sales Product/Core | target proposal | sequence + runtime tests |
| `OD-V1-07` | production auth and mTLS | Security/Platform | dev mock JWT only | signed auth profile + tests |

## P0 — lab/production calls

| ID | Decision/data | Owner | Gate |
| --- | --- | --- | --- |
| `OD-V1-08` | final attempt policy/version | Product/Order Core | production; candidate only MOCK/LAB |
| `OD-V1-09` | 1 SIM lab protocol/DTMF/disposition/allowlist | Infra/vendor | LAB_REAL_SIM |
| `OD-V1-10` | 32 eSIM capacity/failover/caller-ID/cost | Infra/procurement | production |
| `OD-V1-11` | script/legal/do-not-call/retention | Legal/Privacy | customer calls |
| `OD-V1-12` | pilot/release authority/kill switch | Release owner | production |

## P0 — mở bởi red-team review 2026-08-12 (W-0062)

| ID | Decision/data | Owner | Current | Closure evidence | Gate |
| --- | --- | --- | --- | --- | --- |
| `OD-V1-13` | **Golden Hour ONLINE có thuộc scope IVR không.** Business source hiện đọc được là COD-only: `plan/ivr-orther/decisions-log.md` DS-01 (“IVR-callable = CHỈ `CONFIRMING` VÀ CHỈ khi `payment_method_snapshot=COD`”, source-read từ Sales platform). Target V1 §4 đề xuất thêm `GOLDEN_HOUR+ONLINE`. Delta này **chưa được owner phê duyệt**. | Product/Business + Sales Core | `TARGET_DRAFT` — IVR build song song sau mock, không được coi là đã duyệt | Signed program matrix + Sales producer phát `GOLDEN_HOUR+ONLINE` task | real integration |
| `OD-V1-14` | **`ivr_confirmation_required` không có business source.** `grep -rln ivr_confirmation_required docs/documents/` → 0 hit. Cả OpenAPI (`enum:[true]`) và DB (`must be true`) đang gate trên một field chưa có nguồn business đã khóa. | Product/Business + Sales Core | unsourced | Định nghĩa field + owner sign-off + producer test | real integration |
| `OD-V1-15` | **Speech variable whitelist.** Hai bộ spec active mâu thuẫn: bộ hẹp 4 biến (`specs/data/05-pii-policy.md`, `specs/ui/04-ivr-menu-config.md`, `specs/api/04-sim-adapter-contract.md`) vs bộ Target V1 cần thêm `items[]` (public_name, quantity) và `delivery_area_short` (`target-contract-v1-draft.md` §6, governance §2.7). Business source `PACK-09 §9.1` hậu thuẫn bộ hẹp. **Mở rộng whitelist tự nó là một quyết định privacy.** | Product + Privacy/Legal | `OWNER_DECISION_REQUIRED` — fixture mock dùng bộ rộng privacy-safe, không đóng gate | Approved whitelist + PIA/privacy sign-off + cập nhật đồng bộ 3 spec | business acceptance |
| `OD-V1-16` | **Attempt policy delta vs business source.** `docs/documents/4. phase/phase-8/10-…:121-122` và `16-…:26-27` (đều **không** có banner D-10) ghi GH = 2 attempts/**10 phút**, 24/7 = **3** attempts/15 phút. D-10 và candidate `mock-lab-v1` ghi 2 attempts, GH 5 phút, 24/7 15 phút. Sales dev đã nêu xung đột này là `OWNER_DECISION_REQUIRED`. | Product/Order Core | conflict chưa giải quyết | Signed `attempt_policy_version` + banner/sửa trên hai file business (do owner business thực hiện, IVR không sửa `docs/documents/`) | production |
| `OD-V1-17` | **Dial-token reuse semantics.** Task mang đúng **một** `dial_token` scalar, nhưng policy cần ≥2 customer dial cộng technical retry. Năm tài liệu ghi “one-use/attempt”. Không có endpoint re-issue/refresh trong bất kỳ contract nào. Phương án: (a) `dial_tokens[]` per-attempt, (b) reissue endpoint, (c) token bundle, (d) reusable token có TTL/risk control ghi rõ. | Sales/Security/Telephony | `NOT_ESTABLISHED` | Chọn phương án + issue/resolve/reissue contract + TTL/replay/audit tests | real call |
| `OD-V1-18` | **Vị trí resolve `dial_token→E.164`.** `specs/api/04-sim-adapter-contract.md` nói adapter **không** nhận số; `P2-4` đặt resolver trong IVR. Gateway GSM/SIP thương mại quay số E.164. Trust boundary chưa được định nghĩa ở đâu. | Security + Telephony vendor | contradictory | Sơ đồ trust boundary đã duyệt + threat model + vendor capability statement | LAB_REAL_SIM |
| `OD-V1-19` | **TTS/speech synthesis provider.** Không prompt nào implement audio thật; `P8-1` gọi `play` mà không có nguồn audio. Chọn vendor kéo theo PDPA (nội dung đơn rời mạng), cost và pronunciation acceptance. | Product + Infra + Privacy/Legal | `NOT_SELECTED` | Vendor decision + DPA/privacy review + pronunciation acceptance set + cost model | LAB_REAL_SIM |
| `OD-V1-20` | **Production RBAC cho runtime-gate controls.** Bộ permission `DF-01` (LOCKED, 7 quyền) không có quyền nào cho phép sửa `labDestinationAllowlist` hoặc `globalDialKillSwitch`. Cần permission mới + four-eyes. | Security/Platform + Release owner | gap | Approved permission set + four-eyes policy + negative authz tests | LAB_REAL_SIM |
| `OD-V1-21` | **GitLab platform provisioning.** TV1-12 khóa GitLab CI nhưng remote duy nhất hiện tại là GitHub. Cần GitLab project/mirror, Runner, Container Registry, protected branch, MR approvals, “Pipelines must succeed”, masked/protected variables. | Platform/Infra | `BLOCKED_EXTERNAL` (W-0061) | GitLab project URL + remote verification + runner identity + hosted MR pipeline + protected-branch export + registry push/pull proof | P0-2 hosted evidence |

## Explicit non-decisions

- V1 notification is disabled; no notification template/event is required. Bất biến này được enforce ở `P0-4` (`v1NotificationEnabled=false` immutable guard), fail-gate 3 trong `specs/testing/08-acceptance-criteria.md`, và `IT-FAILGATE-*` ở `P5-1` — **không** chỉ dựa vào `P4-5`.
- IVR remains a standalone .NET service; Sales remains Java and owns order truth.
- Current Golden Hour callback remains compatibility-only và không được nhận kết quả 24/7.
- `IMPLEMENTATION_COMPLETE_BEHIND_MOCKS` may be reached while all external rows remain open, but integration/production states must remain blocked.
- Internal API (`specs/api/openapi/ivr-order-confirmation.v1.yaml`) và outbound Sales callback (`order-core-ivr-callback.target-v1.yaml`) là **hai surface riêng biệt**; naming khác nhau là chủ ý và phải map bằng mapper tường minh.
