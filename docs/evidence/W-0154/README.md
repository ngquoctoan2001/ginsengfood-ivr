# W-0154 — B1 capacity calibration unified data-intake evidence

> Ngày: `2026-09-03`  
> Trạng thái: `EVIDENCE_SUBMITTED / LOCAL_INTAKE_BUNDLE_READY / EXTERNAL_DATA_NOT_RECEIVED / CALIBRATION_NOT_RUN / MODEL_UNCALIBRATED / NO_GATE_PROMOTION`

## 1. Phạm vi đã hoàn thành

- Audit khoảng trống dữ liệu hiệu chỉnh B1 trên capacity model, W-0142, lab report, procurement package và attempt-policy decision pack.
- Lập một intake contract chung cho bốn nhóm: `TIMING`, `ARRIVAL`, `POLICY_OUTCOME`, `INFRA_RESERVE`.
- Chuẩn hóa envelope chung gồm provenance, exact SHA-256, observation window, filter, PII statement, signer và authority source.
- Chuẩn hóa row schema và acceptance rules riêng cho từng nhóm dữ liệu.
- Lập ledger fail-closed: cả bốn nhóm hiện `NOT_RECEIVED / NOT_RUN / BLOCKED_EXTERNAL`.
- Soạn D-06 để owner có thể gửi nguyên văn qua kênh được phê duyệt.
- Giữ nguyên ranh giới: không tạo `docs/evidence/W-0008/`, không sửa model/runtime/config/policy/channel count và không promote gate.

Decision/intake pack:

- `plan/ivr-orther/m8-14-capacity-calibration-data-intake-bundle-2026-09-03.md`

## 2. Source manifest

| Nguồn | SHA-256 tại lúc audit |
|---|---|
| `docs/evidence/W-0142/README.md` | `eeed64d945b71de41dc60443ac7bac0a970a5d379d220a954929df6da112058c` |
| `docs/contracts/telephony-procurement-pack/lab-acceptance-report-template.md` | `5b7ab1e0b1a796f7c1e0bb8643fefb313a76afa626a6200bf17519e306c2dbaa` |
| `docs/contracts/telephony-procurement-pack/R-03-esim32-package.md` | `e4129c3f8daa72ce7c1db7ce925f2bdd56afcae6b6cb02a2f13a1b72313dbf66` |
| `plan/ivr-orther/m8-11-attempt-policy-production-decision-pack-2026-09-03.md` | `6dcf7516ba4af0f2746eacb8240618d19a4bf4828aba90abd89e3a8b6a8640a1` |
| `docs/contracts/target-v1-closure-pack/T-09-attempt-policy.md` | `f9be81414aa6aa66e2fb401e422b389081fd09528c2e179477ea5133647e2945` |
| `docs/capacity-model.md` | `d96a2a7177dba508274625b181f30fb722f7f1ebe84be1f1edbe1fdfb017ef81` |
| `docs/review/2026-08-28-capacity-volume-unit-and-session-length.md` | `9ee9e880ce8d8b5ca2d75f104c1f7a3e3316590c9dfaa42a19a276ea9b5dc893` |

Deliverable `plan/ivr-orther/m8-14-capacity-calibration-data-intake-bundle-2026-09-03.md` có SHA-256 `933c55255c538987d1b86ff6d8f46b6657c68821cd00a232a55827cc751fa879` tại lúc bàn giao.

## 3. Verification record

| Kiểm tra | Kết quả |
|---|---|
| Recompute 7 source SHA-256 | `PASS — 7/7 khớp current bytes` |
| `docs/evidence/W-0008/` absent | `PASS — absent có chủ ý; chưa có measured/signed evidence thật` |
| M8-14 có đủ 4 data-group schema + ledger + D-06 | `PASS — 4 group heading, 4 fail-closed ledger row, 1 D-06; ledger 0/4 NOT_RECEIVED` |
| Capacity self-test | `PASS — 6/6; CAPACITY_SELFTEST_PASS_UNCALIBRATED` |
| Docs self-test | `PASS — API_DOCS_GENERATED=14; API_DOCS_SELFTEST_PASS` |
| Traceability test | `PASS — TEST_TRACEABILITY_CURRENT=476` |
| Gate mirror | `PASS — 11 gates, 152 work items, 23 open decisions, production=false` |
| Markdown mapper unresolved links | `PASS — 639 Markdown files; M8-14, W-0154 và target worklist đều 0 unresolved` |
| Prompt/docs diff check | `PASS — git diff --check` |

Local verification chỉ chứng minh tính nhất quán của tài liệu và trạng thái fail-closed; không thay thế measured input, signer authority, owner approval, M3 integration hoặc production readiness.

## 4. Non-inference

- Không có submission external nào được ghi nhận là đã nhận.
- Không có timing, arrival, outcome hoặc reserve datum nào được giả định.
- Không có calibration run hoặc calibrated output nào được tạo.
- Không có recipient identity, delivery receipt hoặc chữ ký nào được suy diễn.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` và `production=false` không được thay đổi bởi gói này.

## 5. Handoff

Owner gửi D-06 qua kênh được phê duyệt. Khi đủ 4/4 submission, người thực hiện phải freeze exact hashes, validate schema/provenance/PII/authority, rồi mới chạy calibration và xin review/approval theo M8-14.
