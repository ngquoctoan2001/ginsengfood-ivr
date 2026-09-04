# W-0188 — B1 current-head capacity provenance restoration

Ngày: `2026-09-04`

Baseline: `main@8ed62e93f5ec0ff7a4c694181ac73ee04f1eb34b` + shared W-0185..W-0187 WIP được bảo toàn.

Trạng thái: **`TESTS_PASS_LOCAL / CAPACITY_INTAKE_CHAIN_CLEAN /
REGISTRY_VALIDATOR_CLEAN / DATA_0_OF_4 / CALIBRATION_NOT_RUN /
EXTERNAL_INTAKE_DEFERRED_BY_OWNER / EXTERNAL_SIGNATURES_REQUIRED / CODE_NOT_AUTHORIZED`**

## 1. Root cause

Commit `8ed62e9` xóa M8-14 và M8-15 nhưng giữ các validator/evidence phụ thuộc. Trước khắc phục:

- `capacity-data-intake-validator.mjs --self-test` exit `1` tại lúc đọc M8-14;
- `capacity-registry-decision-pack-validator.mjs --self-test` vẫn PASS vì local source-pair list
  recompute W-0160 evidence và capacity validator nhưng bỏ sót M8-15;
- capacity arithmetic vẫn `PASS_UNCALIBRATED`, nên lỗi là provenance artifact chứ không phải model.

## 2. Khắc phục

1. Phục hồi đúng byte M8-14/M8-15 từ parent của `8ed62e9`.
2. Xác minh SHA-256 khớp chính xác hash đã công bố ở W-0154/W-0160/W-0182.
3. Bổ sung cặp `m8_15_contract_path` / `m8_15_contract_sha256` vào
   `LOCAL_SOURCE_PAIRS` của W-0182; không đổi schema, decision, quorum hoặc output semantics.
4. Xoay duy nhất validator hash trong manifest W-0182.
5. Không sửa capacity model, scheduler, database, OpenAPI, registry adapter hoặc external state.

## 3. Current hash set

| Artifact | SHA-256 |
| --- | --- |
| M8-14 intake contract | `933c55255c538987d1b86ff6d8f46b6657c68821cd00a232a55827cc751fa879` |
| M8-15 registry contract | `e1d0fd37d610a1696b8e6b4117469ea3f8e929eff72dc95121e3ce9679200417` |
| W-0159 capacity validator | `4208614b44f55e8b9dc39b304021a7004e693b7dbb72ead84ab6d2cc2ed9ef83` |
| W-0182 registry validator | `7ff3a7798fffdf2afec0ba1083685bb655ccb988f3d945e053a9b317fde7f78b` |
| W-0182 pending template | `de94b9b39103682fd338903302625eb47269af821edde994518f4547d6e8859e` |
| W-0160 evidence | `01d27f785fd96e7aadfad2ac659b26c6247d7cba8a72174cdba2270ebafe02e7` |
| W-0182 artifact manifest | `ce32fa252ad5987bc3fa90a1ea238252fecd50945499ddf160ce492c7f0b0a43` |

Machine-readable copy: `docs/evidence/W-0188/artifact-sha256.txt`.

## 4. Verification

| Gate | Kết quả |
| --- | --- |
| GitNexus impact `LOCAL_SOURCE_PAIRS` | `LOW`, `0` impacted process |
| GitNexus impact `verifySourcePins` | `LOW`, `5` impacted / `1` direct / `0` process |
| M8-14/M8-15 exact source | `PASS 2/2` |
| W-0155..W-0159 combined self-test | `PASS valid=1 mode=2 template=1 receipt=7 receipt_verify=12 ledger=9 checkpoint=13 refusals=14` |
| W-0182 registry self-test | `PASS template=1 valid=1 refusals=56 decisions=15 approvals=3` |
| W-0182 pending template | `VALID_NOT_READY`; completed-input mode phải từ chối |
| Capacity arithmetic | `PASS 6/6`, vẫn `PASS_UNCALIBRATED` |
| W-0182/W-0188 manifests | `PASS 4/4` và `PASS 7/7`, drift `0` |
| Pending-input refusal | W-0155 bundle và W-0182 completed mode đều exit `1` |
| PII | Current deliverables `6/6 PASS`; scanner negative-control `CT-CI-06..06h PASS` |
| Docs / CI / traceability | API docs `14` artifact PASS; CI config PASS; traceability `485` |
| Gate mirror | `11` gate / `186` work item / `23` open decision; production flag `false` |
| Markdown map | `598` Markdown file / `653` link resolved; B1 critical set `0` unresolved |
| Worklist | Đúng `1` bảng / `16` row; B1 là `LOCAL_INTAKE_AND_REGISTRY_CHAIN_CLEAN` |
| Diff / aggregate impact | `git diff --check` PASS; GitNexus LOW `29 file / 47 symbol / 0 process` |

Scan rộng trên exact M8-14 có một false positive do một từ chỉ đơn vị hành chính xuất hiện trong cụm
mô tả vai trò của bên ký, không phải địa chỉ. Artifact được giữ đúng byte/hash; không nới PII rule hoặc
sửa contract lịch sử để ép gate xanh. Current deliverables vẫn PASS và scanner behavior được kiểm
riêng bằng `CT-CI-06..06h`.

## 5. Boundary

- Không có external submission, receipt, ledger, checkpoint hoặc registry record thật.
- Không có provider capability, IAM/KMS, recovery drill, sandbox/cutover evidence hoặc chữ ký owner.
- Local self-test dùng dữ liệu synthetic trong thư mục tạm và không phải calibration.
- Không chọn provider, không kết nối trust store và không code adapter.
- `DATA_0_OF_4`; `CALIBRATION_NOT_RUN`; `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Bước tiếp theo — deferred

Owner quyết định ngày `2026-09-04`: chưa thực hiện external intake ở giai đoạn này. Không dispatch
yêu cầu bốn submission, không yêu cầu chữ ký và không mở calibration/adapter review. Khi owner mở lại,
nhận đủ `TIMING`, `ARRIVAL`, `POLICY_OUTCOME`, `INFRA_RESERVE`; đồng thời Platform/Security/M8 giao
completed W-0182 bundle cùng sáu evidence artifact và bảy trusted hash. Chỉ sau hai nhánh PASS mới
freeze calibration input hoặc mở provider-adapter review.
