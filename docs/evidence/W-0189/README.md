# W-0189 — Current documentation-to-code alignment cleanup

Ngày: `2026-09-04`

Baseline: clean `main@6cae4eda49ab03516805583bee8cf0f9172473f0` trước cleanup.

Trạng thái: **`TESTS_PASS_LOCAL / ACTIVE_DOCS_ALIGNED / ACTUAL_MISSING_LINKS_0 /
HISTORICAL_EVIDENCE_PRESERVED / B1_EXTERNAL_INTAKE_DEFERRED_BY_OWNER`**

## 1. Phạm vi

- Đối chiếu active Markdown với source, artifact hiện có và current Git state.
- Sửa link trỏ tới file đã bị xóa; không phục hồi plan superseded chỉ để làm link xanh.
- Cập nhật worklist/tracker/readiness theo current commit và quyết định hoãn B1 external intake.
- Không sửa `src/`, `tests/`, `admin-ui/`, OpenAPI, runtime config hoặc production state.

## 2. Findings đã xử lý

1. Official map ban đầu báo `102` unresolved link. Direct existence audit tách được `85` link tới
   source/JSON/YAML đang tồn tại và `17` link thật sự gãy.
2. Các link gãy trỏ vào Target V1 draft cũ, ba plan W-0106/W-0107/W-0124, ba phiếu W-0122,
   TODAY-04, worklist 29/08 và một `benchmark.py` không có trong vendored test tree.
3. Worklist còn ghi baseline/action `8ed62e9` dù W-0185..W-0188 đã được commit tới `6cae4ed`.
4. B1 external input chưa sẵn sàng và owner yêu cầu để sau; trạng thái nay là
   `EXTERNAL_INTAKE_DEFERRED_BY_OWNER`, không phải yêu cầu đang chờ dispatch ngay.

## 3. Cách cleanup

- Target V1 active pointer chuyển sang `docs/contracts/target-v1-closure-pack/README.md` và các
  OAS/code source hiện hành.
- Plan/evidence link đã xóa chuyển sang README evidence hoặc source file còn tồn tại.
- Bỏ dòng inventory `third_party/vieneu-tts/tests/benchmark.py` vì file không tồn tại.
- Giữ nguyên historical baseline/test claim; cleanup không nâng local result thành external acceptance.
- `integration-requirements/01-sales-platform-requirements.md` vẫn giữ hai tên plan lịch sử dạng
  inline code vì file này được W-0178/W-0187 khóa SHA. Thử sửa đã làm cả hai validator fail closed;
  thay đổi được hoàn tác và self-test trở lại PASS, tránh xoay provenance chỉ vì cleanup câu dẫn.

## 4. Verification

| Gate | Kết quả |
| --- | --- |
| Git state trước cleanup | `main@6cae4ed`, worktree clean, local ahead GitLab/GitHub `6` commit |
| Runtime/API delta từ `59597e2` | `0` file trong `src/`, `tests/`, `admin-ui/`, OpenAPI |
| Validator regression | W-0174 `1/46`; W-0178 `1/1/31`; W-0180 `1/35`; W-0181 `1/1/32`; W-0182 `1/1/56`; W-0183 `1/4/64`; W-0185 `1/34`; W-0187 `1/2/52`; B1 intake chain PASS |
| Official Markdown map | `599` Markdown / `666` resolved / `91` reported unresolved; direct existence audit `0` actual missing |
| Worklist shape | `1` table / `17` ordered row |
| Readiness mirror | `11` gate / `187` work item / `23` open decision; production flag `false` |
| PII | Current W-0189/worklist/B1 deliverables `4/4` PASS; scanner controls `CT-CI-06..06h` PASS |
| Diff / aggregate impact | `git diff --check` PASS; GitNexus advisory LOW `32 file / 49 symbol / 0 process` |
| External boundary | `REAL_CUSTOMER_CALL_ALLOWED=NO`; không dispatch, deploy hoặc production mutation |

## 5. Bước tiếp theo

Mở Work ID riêng để freeze candidate gồm W-0189, chạy clean-checkout full offline suite, UI/LF và
security wrapper trên đúng SHA. Hosted CI và external owner evidence vẫn là gate độc lập.
