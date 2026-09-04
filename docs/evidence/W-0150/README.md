# W-0150 — M8-10 contact / dial-token production-path audit evidence

Ngày: `2026-09-03`

Baseline IVR: `main@b21ec676e490`

Cross-repo snapshot: `C:\Projects\ginsengfood-business-platform` · `PhucApu` ·
`a3aad246d986fbc273cf41aaa93eec6659669656`.

## Verdict

**`EVIDENCE_SUBMITTED / LOCAL_PRIVACY_SEAM_PRESENT / PRODUCTION_PATH_FAIL_CLOSED /
CONTRACT_RUNTIME_MISMATCH_FOUND / M3_CONTACT_PRODUCER_NOT_FOUND /
EXTERNAL_DECISIONS_REQUIRED / CODE_NOT_AUTHORIZED`**

Không có source, OpenAPI, migration, Helm runtime hoặc production secret change trong W-0150.

## Artifact chính

- [M8-10 decision pack](../../../plan/ivr-orther/m8-10-contact-dial-token-production-decision-pack-2026-09-03.md)
- [T-04 dial-token closure ticket](../../contracts/target-v1-closure-pack/T-04-dial-token.md)
- [IR-03 Telephony/SIM](../../../integration-requirements/03-telephony-sim-requirements.md)
- [IR-06 Module 3 handover](../../../integration-requirements/06-module-3-api-handover.md)
- [Target worklist](../../../plan/toan-viec-can-lam-m8-2026-09-03.md)

## Bằng chứng direct-source

| Nhóm | Evidence | Kết luận |
| --- | --- | --- |
| Wire/runtime | OpenAPI task `1141-1217`; `TaskIntakeService.cs:389-438` | `phone_validation_status` optional trên wire nhưng runtime bắt buộc exact `VALID` |
| TTL/persistence | `TaskIntakeService.cs:411-414`; `PersistenceInvariantValidator.cs:120-128` | accepted persisted task ép token expiry bằng window end; ciphertext phải `enc:` |
| Resolver contract | `ProviderPorts.cs:16-45` | destination output là opaque reference, raw-phone guard ở type boundary |
| Local vault | `MockDialTokenVault.cs:64-103`; `LabDialTokenVault.cs:37-67` | one-resolve per `(token, attempt)`, cross-attempt reuse; local-only alias/fingerprint |
| Production DI | `SchedulerCapacity.cs:480-537`; `ServiceCollectionExtensions.cs:161-169` | không có production resolver/gateway/protector; fail-closed |
| Safety config | `IvrOptionsValidator.cs:49-52`; Helm values | real customer call NO, deployment profiles còn MOCK |
| Secret/network | `external-secrets.yaml:72-95`; `values.yaml:230-234` | resolver secret chỉ là template; chưa mount/use; no external egress |
| Audit/retention | no source `DIAL_TOKEN_RESOLVED`; `RetentionTargetCatalog.cs:27-31` | resolution-specific audit thiếu; ciphertext có đường redact nhưng production period chưa ký |
| M3 | exact field/resolver search trên snapshot | không thấy current contact producer/issuer/resolver implementation |

## Verification

| Gate | Kết quả 03/09/2026 |
| --- | --- |
| Focused unit — intake contact/privacy, token resolver/vault, LAB/production DI, options, inventory/secrets | **PASS `99/99`** |
| Focused contract — task intake + Sales contract scaffold | **PASS `24/24`** |
| Focused PostgreSQL integration — protected intake, missing-token fail-closed, API PII | **PASS_LOCAL_POSTGRES `3/3`** — W-0161 full integration `236/236`, 0 fail/skip; gồm `IT-INTAKE-DB-01`, `IT-TEL-TOKENFAIL-02`, `IT-INTAKE-PRIVACY-04` |
| API docs/OpenAPI | **PASS** — docs `14` artifact; invalid OpenAPI rejected; parse `2` file; `9` fixture/`12` schema-negative/`13` domain-negative/`1` current compatibility; `3` pinned hash current |
| Test traceability | **PASS `476`** |
| Tracker/readiness mirror | **PASS** — `11` gate, `148` work item, `23` open decision; no rung claimed; production flag `false` |
| Official Markdown map | **PASS** — `631` Markdown file; decision pack và evidence W-0150 đều `0` unresolved link |
| `git diff --check` | **PASS** — chỉ có line-ending conversion warnings của shared worktree |
| M3 exact source scan | **PASS audit query** — 7 field/resolver term đều `0`; snapshot/dirty state được ghi riêng, không suy thành toàn hệ không có artifact |
| Source/OpenAPI/DB/Helm/secret runtime change thuộc W-0150 | **Không có** — decision/evidence only |

Ghi chú lịch sử: lần chạy W-0150 ban đầu dừng ở fixture. W-0161 đã chạy assertion thật qua local
Docker/Testcontainers; xem [evidence W-0161](../W-0161/README.md). Kết quả không chứng minh
production issuer/resolver/vault hoặc external trust boundary.

Follow-up W-0184 đã chạy focused synthetic closure path `D-02 → S-08` trên detached clean
`5c0b170`: **PASS `1 positive / 8 refusal / 7 authority / 15 DTK decision`**, đồng thời regression
W-0164 `2/19`, W-0165 `2/27`, W-0170 `1/21` đều PASS. Xem [evidence W-0184](../W-0184/README.md).
Đây chỉ là bằng chứng validator-chain local; external decision, issuer/resolver/vault, contact
producer và shared E2E vẫn `NOT_RECEIVED/NOT_RUN`.

Follow-up W-0183 bổ sung validator metadata-only cho production decision bundle: **PASS
`1 template / 4 model valid / 64 refusal`**, khóa đủ `DTK-01..DTK-15`, `14` test-plan scenario, `9`
authority sign-off và `8` external evidence pin. Xem [evidence W-0183](../W-0183/README.md).
Output cao nhất chỉ cho phép implementation review; không phải production approval.

## Stop rule

- Không code production adapter/vault/resolver trước khi `DTK-01..DTK-15` được đúng owner ký.
- Không bật Target V1 production, external egress hoặc real-customer-call.
- Không coi ExternalSecret template, MOCK/LAB test hoặc tài liệu M8 là external acceptance.
