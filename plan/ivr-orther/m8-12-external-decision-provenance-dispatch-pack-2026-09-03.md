# M8-12 — External decision provenance and dispatch-ready pack

Work: `W-0152` · ngày lập: `2026-09-03`

Pin rotation hiện hành: `W-0170` ngày `2026-09-04`. Bản W-0152 gốc và hash cũ vẫn được giữ
trong git history; rotation chỉ thay artifact `OD-18` đã được sửa liên kết tới tài liệu superseded.

Baseline code được audit: `main@b21ec676e490`

Trạng thái:
**`LOCAL_HANDOFF_PACKAGE_READY / EXTERNAL_DISPATCH_NOT_PERFORMED /
EXTERNAL_APPROVAL_NOT_RECEIVED / NO_GATE_PROMOTION`**

Người lập: **Codex — provenance/dispatch preparation**. Tài liệu này không ký thay Module 8
Owner hoặc bất kỳ external owner nào.

## 1. Kết luận

Phần tự chủ còn lại của `C5` đã được gom thành một đầu mối:

1. `S-01..S-10` được đối chiếu với artifact hiện hành; `S-10` đã thực thi tại `W-0141`, không còn
   là việc chờ làm.
2. Bổ sung `S-11` để route **errata VoLTE/procurement** vốn bị thiếu khỏi TODAY-01. W-0135 chỉ sửa
   fact; nó không phải approval model/SKU, vendor capability hoặc quyết định mua.
3. Mỗi sheet có owner, artifact exact-hash, câu trả lời cần nhận, trạng thái dispatch/approval và
   stop rule.
4. Chưa có hành động gửi ra ngoài hoặc chữ ký mới trong W-0152. Mọi external gate giữ fail-closed.

Hash được tính trên byte hiện tại của từng artifact. Nếu một artifact đổi dù chỉ một byte, chữ ký
gắn với hash cũ không còn dùng được; phải tạo hash mới và yêu cầu signer xác nhận lại exact version.
Manifest máy đọc hiện hành nằm tại `docs/evidence/W-0170/artifact-sha256.txt`.

## 2. Dispatch matrix chuẩn

| Sheet | Quyết định / artifact chính | Owner phải trả lời | Câu trả lời hoặc bằng chứng bắt buộc | Dispatch | Approval | Stop rule |
| --- | --- | --- | --- | --- | --- | --- |
| `S-01` | [OD-18 authority form](questions-to-module-3-od18-authority.md) | M3 contract/business owner; Privacy cho `OD18-C3` | `OD18-C1..C5`, producer/consumer evidence, retention purpose nếu giữ field, signer provenance | `NOT_PERFORMED` | `M8_POSITION_RECORDED / M3_PRIVACY_NOT_RECEIVED` | Không remove field/enum hoặc nâng W-0123 `ACCEPTED` trước đủ chữ ký và CDC |
| `S-02` | [M8-05](m8-05-program-result-contract-signoff-2026-09-03.md) + [T-01](../../docs/contracts/target-v1-closure-pack/T-01-program-matrix.md) | M3 + Product/Order Core | Producer mapping `24_7`, `PHONE_VALID`, `ELIGIBLE_FOR_IVR`; consumer handling; signed production-policy ref | `NOT_PERFORMED` | `M8_SIGNED / EXTERNAL_NOT_RECEIVED` | Không đổi business pair/enum hoặc gọi local tests là production proof |
| `S-03` | [W-0128 evidence](../../docs/evidence/W-0128/README.md) | M3 + Security + Platform | Operator UI/BFF owner, authn/authz/audit model, deprecation/cutover/rollback, acceptance refs | `NOT_PERFORMED` | `M8_HANDOFF_RECORDED / EXTERNAL_NOT_RECEIVED` | Không khôi phục console accounts hay mở production admin access từ giả định |
| `S-04` | [M8-06](m8-06-upstream-session-trace-signoff-2026-09-03.md) | M3 contract/producer owner | Exact `golden_hour_session_id` contract, cardinality, producer SHA, CDC, cutover and shared tests | `NOT_PERFORMED` | `M8_POSITION_SIGNED / M3_NOT_RECEIVED` | Không sửa OpenAPI/domain/DB hoặc tổng hợp ID thay upstream trước chữ ký |
| `S-05` | [M8-07](m8-07-target-v1-shared-callback-handoff-2026-09-03.md) | M3 + Security + Platform | Generic endpoint/OAS/consumer, ACK semantics, auth issuer/custody, sandbox/network/TLS, shared E2E | `NOT_PERFORMED` | `M8_LOCAL_READY / EXTERNAL_NOT_RECEIVED` | Delivery giữ disabled; không gọi fake/local callback là shared E2E |
| `S-06` | [M8-08](m8-08-opt-out-suppression-decision-pack-2026-09-03.md) | Project Owner + CRM/M3.1 + M3 + Legal/Privacy + Product; Security/Platform cho transport/store | `OPT-01..OPT-11`, explicit signal, subject/scope, store/lifecycle, ACK/reversal, retention and audit | `NOT_PERFORMED` | `M8_POSITION_RECORDED / EXTERNAL_NOT_RECEIVED` | Không suy `Rejected` thành opt-out, không thêm wire/DB/runtime trước contract |
| `S-07` | [M8-09](m8-09-revoke-freshness-decision-pack-2026-09-03.md) | Project Owner + M3/Order Core + Product; technical owners theo strategy | `RVK-01..RVK-12`, chọn A/B/hybrid, D-06 consumer evidence nếu A hoặc revoke/race/fencing contract nếu B | `NOT_PERFORMED` | `POSITION_RECORDED / OWNER_PROVENANCE_REQUIRED` | Không sửa scheduler; không coi behavior A hiện tại là approval |
| `S-08` | [M8-10](m8-10-contact-dial-token-production-decision-pack-2026-09-03.md) + [T-04](../../docs/contracts/target-v1-closure-pack/T-04-dial-token.md) | M3 + Security + Platform + Telephony; Product/Privacy/Release theo dòng | `DTK-01..DTK-15`, producer/issuer/resolver/TTL/reissue/custody/trust boundary, vendor API, egress/audit/rollback | `NOT_PERFORMED` | `EXTERNAL_NOT_RECEIVED` | Không code adapter/vault, mở egress/secret hoặc bật `PRODUCTION_REAL` |
| `S-09` | [M8-11](m8-11-attempt-policy-production-decision-pack-2026-09-03.md) + [T-09](../../docs/contracts/target-v1-closure-pack/T-09-attempt-policy.md) | Product + Order Core + M3; Platform/M8/Release cho technical controls | `ATP-01..ATP-15`, canonical two-program bundle/hash, M3 producer/CDC, lifecycle/four-eyes/cutover/pre-dial/shared tests | `NOT_PERFORMED` | `EXTERNAL_NOT_RECEIVED` | Không promote/rename `mock-lab-v1`, chọn số production hoặc sửa scheduler/registry |
| `S-10` | [W-0141 evidence](../../docs/evidence/W-0141/README.md) | Module 8 Owner | Không cần decision mới: kiểm chứng rename `_SUPERSEDED`, size/hash equality và ledger | `NOT_APPLICABLE` | `OPTION_A_EXECUTED_W0141 / BYTES_PRESERVED` | Không gửi DOCX `_SUPERSEDED` như tài liệu hiện hành; external gates không đổi |
| `S-11` | [W-0135 factual correction](../../docs/evidence/W-0135/README.md) + [Errata 21](../../docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md) + [R-00](../../docs/contracts/telephony-procurement-pack/R-00-voice-gateway-rfq.md) + [R-06](../../docs/contracts/telephony-procurement-pack/R-06-to-trinh-mua-thiet-bi.md) | Module 8 Owner + Product + Infra/Procurement + Telephony/vendor | Xác nhận mốc 2G/3G, long-term VoLTE requirement, model/SKU + datasheet/vendor capability, báo giá/channels, acceptance test, procurement approval và source-spec update owner | `NOT_PERFORMED` | `M8_ROUTING_PREPARED_W0152 / EXTERNAL_NOT_RECEIVED` | Không mua/duyệt thiết bị 2G/WCDMA/CSFB-only cho horizon sau 09/2028; không gọi đề xuất là model đã duyệt |

## 3. Exact artifact hashes

| Artifact | SHA-256 |
| --- | --- |
| `plan/ivr-orther/questions-to-module-3-od18-authority.md` | `fed2fe7a68dc41ac6f658fc6479163ac89e007e1ea1b6fa0126522bab54c6b0d` |
| `plan/ivr-orther/m8-05-program-result-contract-signoff-2026-09-03.md` | `6525d2df4ef0894ded69190e3d72af1e2482e752cbba5ae63b5520a43318a540` |
| `plan/ivr-orther/m8-06-upstream-session-trace-signoff-2026-09-03.md` | `a742a657d88eba2c876257a3296149063349d3138cefe6383bc1c4f5a44ab85e` |
| `plan/ivr-orther/m8-07-target-v1-shared-callback-handoff-2026-09-03.md` | `c4bb79fa8b06c0f06a8b959b698084f9d02444a5cb1a25e14413b87ae74c1aa0` |
| `plan/ivr-orther/m8-08-opt-out-suppression-decision-pack-2026-09-03.md` | `ec0c4e9be8500b094295a66809a8994f5c2724a74bff05f299b87fdb8becd047` |
| `plan/ivr-orther/m8-09-revoke-freshness-decision-pack-2026-09-03.md` | `3d95031375026d7fd7c902ca17cded084eb789fc300df2af975194fd6af8e820` |
| `plan/ivr-orther/m8-10-contact-dial-token-production-decision-pack-2026-09-03.md` | `816392f8ae8b8b486564dd282ddf8e2e9d639936d4bca4f2e0df013f23cb6249` |
| `plan/ivr-orther/m8-11-attempt-policy-production-decision-pack-2026-09-03.md` | `6dcf7516ba4af0f2746eacb8240618d19a4bf4828aba90abd89e3a8b6a8640a1` |
| `docs/evidence/W-0128/README.md` | `d01445ec717f4a2a4321151a74f1b600b2200b4c2b109e5310a2da1e55772238` |
| `docs/evidence/W-0135/README.md` | `3bab4d201d84c414f682253b8f00b52a7cd252fb7f7e995bdb893bee35070276` |
| `docs/evidence/W-0141/README.md` | `e1c8268ffed06d551f72ddd86c80ca958109a8097e69e020a8bcc42af46e52e1` |
| `docs/contracts/target-v1-closure-pack/T-01-program-matrix.md` | `60bac1b121d25ada01dc26854e2900e3b19d4e2dddce64d0bf1c435e29d71512` |
| `docs/contracts/target-v1-closure-pack/T-04-dial-token.md` | `e7df35e7711a59ed21076a4330a2e6c67e08d3213cd62813981cd89efc184f9f` |
| `docs/contracts/target-v1-closure-pack/T-09-attempt-policy.md` | `f9be81414aa6aa66e2fb401e422b389081fd09528c2e179477ea5133647e2945` |
| `docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md` | `0be9c9e8be2b05d043e060374cb526c81f349a9ace45b2efd6f8025116fd3635` |
| `docs/contracts/telephony-procurement-pack/R-00-voice-gateway-rfq.md` | `ae483fa33e5741b49a0d6c3e71bfedf51df14eec46d2a69935f45809f0e7ab37` |
| `docs/contracts/telephony-procurement-pack/R-06-to-trinh-mua-thiet-bi.md` | `341153903009c37fb7fcf5ea3c5bdb253a6c7cd49f908676ff7c0612e681eea4` |

## 4. Dispatch batches

Không gửi một message chung rồi coi mọi owner đã nhận đúng phần. Route theo các batch sau và ghi
evidence gửi/nhận riêng:

| Batch | Sheets | Người nhận bắt buộc | Artifact gửi kèm tối thiểu |
| --- | --- | --- | --- |
| `D-01` | `S-01,S-02,S-04,S-05,S-07` | M3 contract/business/producer owner | OD-18 form, M8-05, M8-06, M8-07, M8-09 và hash manifest |
| `D-02` | `S-03,S-05,S-08` | Security + Platform; Telephony thêm cho S-08 | W-0128 evidence, M8-07, M8-10/T-04 và hash manifest |
| `D-03` | `S-06` | CRM/M3.1 + M3 + Legal/Privacy + Product | M8-08 và hash manifest |
| `D-04` | `S-09` | Product + Order Core + M3; technical rows tới Platform/M8/Release | M8-11/T-09 và hash manifest |
| `D-05` | `S-11` | Module 8 Owner + Product + Infra/Procurement + Telephony/vendor | W-0135, Errata 21, R-00, R-06 và hash manifest |

`S-10` không dispatch lại để xin quyết định; chỉ giữ W-0141 làm audit trail.

## 5. Signature intake template

Mỗi signer trả một record cho đúng sheet và exact hash:

| Field | Giá trị bắt buộc |
| --- | --- |
| Sheet / decision IDs | `S-xx` + `OD/OPT/RVK/DTK/ATP/T-xx` áp dụng |
| Decision | Giá trị dứt khoát; không dùng “OK”, “tùy dev” hoặc im lặng |
| Signer identity | Họ tên hoặc định danh đơn vị có thể kiểm chứng |
| Role / organization | Vai trò và đơn vị chịu trách nhiệm cho quyết định |
| Authority source | Charter, ticket, role assignment hoặc approval chain trao quyền ký |
| Artifact path + SHA-256 | Exact artifact/hash mà signer đã đọc và chấp nhận |
| Approval timestamp | ISO-8601 có timezone |
| Scope / environment | Contract, LAB, STAGING, PRODUCTION hoặc phạm vi procurement cụ thể |
| Effective / cutover | Ngày hiệu lực, producer/consumer ordering và compatibility window |
| Rollback / rejection path | Điều phải khôi phục/tắt nếu decision bị rút hoặc rollout lỗi |
| Evidence references | Commit, OpenAPI, CDC, test run, vendor datasheet, quote hoặc audit log |
| Residual blocker | `NONE` hoặc danh sách blocker còn mở và owner của từng blocker |

Record không có authority source, exact hash, scope hoặc evidence phù hợp được giữ
`PROVENANCE_INCOMPLETE`; không dùng nó để mở code/release gate.

## 6. Dispatch and approval ledger

| Sheet | Dispatch evidence | Response evidence | Trạng thái kết luận |
| --- | --- | --- | --- |
| `S-01..S-09` | `NOT_PERFORMED` | `NOT_RECEIVED` từ external owner | `BLOCKED_EXTERNAL`; các M8 position cũ không thay external signature |
| `S-10` | `NOT_APPLICABLE` | [W-0141](../../docs/evidence/W-0141/README.md) | `EXECUTED_LOCAL / EXTERNAL_GATES_UNCHANGED` |
| `S-11` | `NOT_PERFORMED` | `NOT_RECEIVED` | `M8_ROUTING_PREPARED_W0152 / EXTERNAL_SIGNATURE_REQUIRED` |

Khi thực sự gửi, ledger phải được append với channel/message/ticket ID, người nhận, timestamp và
exact manifest hash. Khi nhận phản hồi, phải lưu nguyên artifact trả lời và đối chiếu authority,
scope, hash, evidence trước khi cập nhật sheet tương ứng. W-0152 không thực hiện hai hành động đó.

## 7. Exit và bước kế tiếp

W-0152 hoàn tất phía local khi pack, manifest, TODAY-01, target worklist, tracker/readiness và
Markdown map khớp nhau. Trạng thái tối đa là `EVIDENCE_SUBMITTED`, không phải `ACCEPTED`.

**Bước kế tiếp cần làm:** Module 8 Owner/chief auditor route `D-01..D-05`, thu signature record theo
§5, kiểm exact hashes rồi mở riêng work item implementation cho từng sheet đã đủ authority. Không
gộp sự im lặng hoặc một chữ ký ngoài phạm vi thành approval toàn pack.
