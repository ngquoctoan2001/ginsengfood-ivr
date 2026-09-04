# W-0181 — Offline upstream-session sign-off intake validator

Ngày: `2026-09-04`

Baseline: `main@5c0b17085030cd69722a8422fe635bbcfbd9f5de` + shared WIP được bảo toàn.

Trạng thái: **`TESTS_PASS_LOCAL / OFFLINE_M3_SIGNOFF_VALIDATOR_READY /
M3_CONTRACT_SIGNOFF_REQUIRED / CODE_NOT_AUTHORIZED`**

## 1. Kết quả audit

Audit lại `C3 + C4 + C6 / W-0146` xác nhận trạng thái production chưa đổi:

- active task wire/generated model/domain/task/job persistence chưa có upstream business session;
- `CapacityIncidentEntity.SessionId` là capacity/admin scope ID nội bộ và có writer synthetic, nên
  không được đổi nghĩa hoặc dùng làm Golden Hour session;
- M8 đã đề xuất `golden_hour_session_id`, nhưng M3 chưa gửi chữ ký, producer artifact, CDC hay
  cutover evidence;
- chưa có căn cứ để sửa OpenAPI, generated client, domain, database, scheduler hoặc callback.

Phần local có giá trị còn lại là chuẩn hóa intake của gói quyết định thật. Vì vậy W-0181 tạo CLI
offline metadata-only tại `deploy/ci/scripts/upstream-session-signoff-validator.mjs` và mẫu pending
tại `docs/evidence/W-0181/upstream-session-signoff-input.template.json`.

## 2. Contract mà validator khóa

Chế độ completed chỉ PASS khi quyết định là `ACCEPT` và giữ đúng M8 proposal:

| Thuộc tính | Giá trị bắt buộc |
| --- | --- |
| Field | `golden_hour_session_id` |
| Kiểu/độ dài | opaque string `1..128`, case-sensitive, không control hoặc edge whitespace |
| Golden Hour | required và non-null |
| 24/7 | field bị cấm, kể cả `null` |
| Issuer/owner | `MODULE3_GOLDEN_HOUR_CORE` |
| Stability | giữ nguyên qua retry/replay và các task cùng session; không phải idempotency key |
| Capacity boundary | không map vào `capacity_incident.session_id` hiện hữu |
| Privacy | technical identifier, không PII |

Mọi proposal khác phải quay lại W-0146 để sửa decision pack và source pin; validator không tự
chấp nhận alias hoặc replacement field.

## 3. Evidence bắt buộc

Completed input phải có:

1. M3 repository/commit, generated-client revision và authoritative contract artifact;
2. CDC report trên cùng M8/M3 candidate, đủ năm case: Golden Hour, 24/7, replay, changed-session
   conflict và capacity-session separation;
3. cutover theo thứ tự `store -> producer -> enforce`, compatibility window, rollback và target DB
   inventory; `migration_not_started=true` tại bước intake;
4. chữ ký đúng role `MODULE3_GOLDEN_HOUR_CONTRACT_OWNER`, authority/signature hash và independent
   verifier khác signer;
5. toàn bộ cờ raw payload, contact detail, credential, OpenAPI/runtime/DB change, gate promotion và
   real-customer call đều `false`.

Reviewer phải truyền bốn pin độc lập qua CLI: M8 candidate SHA, M3 producer SHA, CDC SHA-256 và
signature SHA-256. Giá trị tự khai trong JSON không được dùng làm trust anchor.

## 4. Trust boundary và fail-closed rules

- Input phải là regular non-symlink file bên trong repository, tối đa 256 KiB, strict UTF-8 không
  BOM và không có duplicate JSON key.
- Ref/alias bị từ chối khi giống email, số điện thoại, địa chỉ hoặc credential/secret.
- Validator không có network client, DB client hoặc code path gọi runtime.
- Template pending chỉ trả `UPSTREAM_SESSION_TEMPLATE_VALID_NOT_READY` và không thể dùng như sign-off.
- Output completed chỉ là
  `UPSTREAM_SESSION_SIGNOFF_VALID_ELIGIBLE_FOR_IMPLEMENTATION_REVIEW_ONLY`; nó không authorize code,
  migration, release hay production.

File sign-off M8-06 dưới `plan/ivr-orther/` hiện đang bị xóa trong external WIP. W-0181 không restore,
không stage deletion và không dùng working-tree path đó làm trust anchor; hai source sống còn được pin
trực tiếp là W-0146 evidence và IR-06.

## 5. Cách dùng khi M3 gửi dữ liệu thật

1. Copy template thành file mới và chỉ điền alias/ref/hash metadata; không dán raw payload, token,
   secret hoặc dữ liệu khách hàng.
2. Lấy bốn expected pin từ nguồn độc lập với người lập JSON.
3. Chạy:

```powershell
node deploy/ci/scripts/upstream-session-signoff-validator.mjs `
  --input <m3-signoff.json> `
  --expected-m8-candidate-sha <40hex> `
  --expected-m3-producer-sha <40hex> `
  --expected-cdc-sha <64hex> `
  --expected-signature-sha <64hex>
```

Chỉ sau output eligible mới mở một implementation review riêng. Review đó phải chạy impact analysis
trước khi sửa từng symbol và vẫn phải quyết định migration/cutover/shared E2E độc lập.

## 6. Verification local

| Gate | Kết quả |
| --- | --- |
| Node syntax | **PASS** |
| Positive/mutation self-test | **PASS `1 template / 1 valid / 32 refusal`** |
| Pending template | **PASS** — `UPSTREAM_SESSION_TEMPLATE_VALID_NOT_READY` |
| Template SHA-256 | `c95902cc27268c0947b672b27f6c675d99b0c49cf875747756da852102e44292` |
| Validator SHA-256 | `01f967629afc066cd8315a3622b3b277b76402ef1d082f2f1d9292639ea7d350` |
| W-0146 evidence SHA-256 LF | `0bce4b3fcc0e6d1145f676e619405d8319e0480d4d0af255fd693adfb73b849b` |
| IR-06 SHA-256 LF | `b676a32d4ba51b9f345eb3d32e21d793216f4011e98bbfc9dc8d2867997ba08a` |
| Artifact manifest | `docs/evidence/W-0181/artifact-sha256.txt` |
| PII scan | **PASS `4 files / 0 binary`** — validator và W-0181 evidence scope |
| API docs self-test | **PASS** — 14 generated artifacts |
| CI config self-test | **PASS** |
| Test traceability | **PASS `485` current entries** |
| Readiness mirror | **PASS `11 gates / 179 work items / 23 open decisions`**, production=false |
| MR traceability | `NOT_APPLICABLE_NO_MR` — local work item, không tạo metadata MR giả |
| Markdown map | **PASS W-0181 `0 unresolved`**; aggregate map vẫn phản ánh external plan-document deletion |
| GitNexus detect | **LOW aggregate tracked tree `10 files / 10 symbols / 0 process`**; CLI mới chưa có trong stale index và không có runtime caller |
| Scoped diff check | **PASS** |

Self-test refusal bao phủ schema/source drift, missing/extra key, field alias, sai owner/program rule/
normalization/length, bốn independent pin mismatch, cross-repo mismatch, CDC thiếu, cutover sai thứ tự,
migration đã bắt đầu, signer=verifier, sai role/timestamp, unsafe flags, placeholder, PII/phone/secret,
malformed/duplicate-key/oversized input và path ngoài repository.

## 7. Phần còn lại

- M3 contract-owner sign-off: `NOT_RECEIVED`.
- M3 producer/client/contract artifact: `NOT_RECEIVED`.
- CDC năm case trên exact candidates: `NOT_RUN`.
- Cutover/rollback/target DB inventory: `NOT_RECEIVED`.
- Shared E2E và implementation review: `NOT_RUN`.
- OpenAPI/domain/DB/runtime change: **`NOT_AUTHORIZED`**.
- `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 8. Bước tiếp theo

M3 gửi completed metadata bundle và reviewer cung cấp bốn pin độc lập. Chạy W-0181; chỉ nếu PASS mới
mở work item implementation riêng, chạy GitNexus impact rồi mới cân nhắc OpenAPI/domain/DB/tests.
