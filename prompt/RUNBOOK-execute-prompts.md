# RUNBOOK — Execute IVR Prompts P0-P11

Trạng thái: `LIVING` · Áp dụng trước khi chạy bất kỳ prompt triển khai nào trong `prompt/phase-*`.

## Mục tiêu
Runbook này biến thư viện prompt từ "đủ để đọc" thành "đủ để chạy có kiểm soát". Mỗi prompt phải đi qua code, test, review, evidence và acceptance. Không prompt nào tự được coi là done chỉ vì agent đã tạo file hoặc test cục bộ xanh.

## Đọc trước bắt buộc
1. [`00-index.md`](00-index.md) — thứ tự phase, prereq, scope P0-P11.
2. [`README-governance.md`](README-governance.md) — bất biến không được phá.
3. [`_review/phase-0-11-spec-alignment-review.md`](_review/phase-0-11-spec-alignment-review.md) — kết quả khớp spec hiện hành.
4. [`_execution/defaults-and-confirmations.md`](_execution/defaults-and-confirmations.md) — default/toolchain cần chốt.
5. [`_execution/prompt-execution-tracker.md`](_execution/prompt-execution-tracker.md) — tracker trạng thái khi chạy.

## Source-of-truth khi có xung đột
Thứ tự ưu tiên:
1. `plan/ivr-orther/decisions-log.md`
2. `specs/_review/open-decisions-register.md`
3. `specs/api/openapi/ivr-order-confirmation.v1.yaml`
4. `specs/**` hiện hành
5. `integration-requirements/**`
6. `prompt/phase-*`
7. archive/legacy docs chỉ dùng làm lịch sử

Nếu prompt xung đột với spec hiện hành, dừng prompt đó, ghi `BLOCKED_SPEC_CONFLICT` vào tracker, rồi sửa prompt/spec trước khi code.

## Trạng thái chuẩn
| Status | Ý nghĩa | Có được chạy prompt phụ thuộc? |
| --- | --- | --- |
| `NOT_STARTED` | Chưa chạy | Không |
| `IN_PROGRESS` | Đang code/test | Không |
| `CODE_DONE` | Code chính đã xong, test chưa đủ | Không |
| `TESTS_PASS` | Test bắt buộc đã xanh | Chưa, nếu thiếu evidence |
| `EVIDENCE_SUBMITTED` | Evidence đã nộp nhưng chưa accept | Không cho gate quan trọng |
| `ACCEPTED` | Reviewer/owner accept code + test + evidence | Có |
| `BLOCKED_INTERNAL` | Chặn bởi spec/code/test trong repo | Không |
| `BLOCKED_EXTERNAL` | Chặn bởi owner/vendor/legal/team khác | Tùy prompt, phải có mitigation/parallel path |
| `DEFERRED_TARGET` | Target flag hoặc provider chưa giao, được defer có chứng cứ | Có nếu current path an toàn |
| `N/A` | Không áp dụng có lý do | Có |

## Execution lanes
Chạy theo lane để không nghẽn:
| Lane | Prompt | Cách chạy |
| --- | --- | --- |
| Core foundation | P0-1..P2-7 | Chạy tuần tự theo prereq, test từ sớm |
| Admin UI | P3-1..P3-4 | Bắt đầu sau P2-1/P3 prereq; dùng mock data đến khi API thật sẵn |
| Integration | P4-1..P4-6 | Bắt đầu sau P2/P1 contract; mọi target provider bật bằng flag |
| Quality | P5-1..P5-5 | Chạy song song từ P2, nhưng accept sau khi target slice có code |
| Observability/deploy | P6-1..P7-5 | Bắt đầu khi runtime có metrics/health và container baseline |
| Pilot/release | P8-1..P9-2 | Chỉ sau SIM/prod gate tương ứng |
| Compliance/maturity | P10-1..P10-5 | Chạy song song, nhưng gate trước PROD |
| External closure | P11-1..P11-4 | Chạy từ ngày đầu, không đợi code xong |

## Thứ tự đề xuất
1. Chốt các dòng `MUST_DECIDE_BEFORE_P0/P1` trong defaults sheet.
2. Khởi động P11-1, P11-2, P11-3, P11-4 song song để mở blocker ngoài code sớm.
3. Chạy P0-1 → P0-2 → P0-3 → P0-4.
4. Chạy P1-1/P1-2 song song sau P0, rồi P1-3, P1-4.
5. Chạy P2-1 → P2-2 → P2-3 → P2-4 → P2-5 → P2-6; P2-7 sau P2-1.
6. Chạy P3 UI song song với P2 sau khi P3 prereq có mock/API contracts.
7. Chạy P5 test suites ngay khi slice tương ứng có code; không dồn test đến cuối.
8. Chạy P4 integration với current contract trước; target OC1/OC2/DC-05/DC-06/IR-CRM-01 để `DEFERRED_TARGET` nếu provider chưa giao.
9. Chạy P6/P7 khi service có health/metrics/container baseline.
10. Chạy P8 chỉ khi P11-1 có lab acceptance đủ để dùng REAL SIM trong pilot scope.
11. Chạy P9 chỉ khi P8 evidence accepted, P11-3 sign-off package đủ, P11-4 readiness board không còn HARD blocker.
12. Chạy P10 xuyên suốt, nhưng P10-1/P10-2/P10-5 phải accepted trước production gate.

## Per-prompt execution loop
Mỗi prompt phải theo vòng này:
1. Đọc `## 3. SOURCE SPECS` của prompt và các spec liên quan.
2. Ghi tracker row thành `IN_PROGRESS`, điền owner/agent.
3. Implement đúng `## 6. BUILD STEPS`.
4. Viết và chạy test ở `## 8. TESTS TO WRITE`.
5. Tạo evidence theo `## 10. EVIDENCE EXPECTED`.
6. Self-review `## 9` và `## 11 FORBIDDEN`.
7. Cập nhật tracker: code/test/evidence links, status.
8. Reviewer/owner chỉ chuyển `ACCEPTED` khi evidence đọc được và khớp spec.

## Evidence convention
Mọi evidence phải có đường dẫn ổn định:
```text
docs/evidence/<PromptId>/<EvidenceId>.md
docs/evidence/<PromptId>/test-report.<ext>
docs/evidence/<PromptId>/screenshots/<name>.png
docs/evidence/<PromptId>/logs/<name>.log
```

Mỗi evidence note tối thiểu có:
- prompt ID
- requirement/decision IDs
- command đã chạy
- kết quả pass/fail
- link artifact/log/screenshot
- người/agent tạo
- ngày giờ

## Stop/no-go rules
Dừng ngay và không tự đoán nếu gặp một trong các điều kiện:
- Spec hiện hành xung đột với prompt.
- Prompt yêu cầu target provider chưa giao nhưng không có current fallback.
- Test fail ở invariant P0: D-10, DS-01, D-02, D-05, DO-06, DF-03, DF-04/05.
- Có nguy cơ log/raw phone/recording/token mapping lọt khỏi adapter boundary.
- Có thay đổi làm IVR transition order trực tiếp.
- Có thay đổi bật `REAL_CUSTOMER_CALL_ALLOWED=true` ngoài P9 gate.
- Evidence thiếu nhưng agent muốn chuyển `ACCEPTED`.

## Gate trước khi mở phase lớn
| Gate | Điều kiện tối thiểu |
| --- | --- |
| Vào P1 | P0-1..P0-3 accepted; defaults codegen/DB/CI chốt |
| Vào P2 | P1-1..P1-3 accepted; OpenAPI lint + DB migration baseline xanh |
| Vào P3 | P3 API/admin contracts hoặc mocks ổn định; RBAC/error envelope có baseline |
| Vào P4 | P2-6 accepted; P11-2 current contracts/tickets tồn tại |
| Vào P5 full | P2/P3/P4 target slice có code hoặc mock rõ |
| Vào P7 | P5-4 gate, container baseline, secrets/defaults chốt |
| Vào P8 | P11-1 lab acceptance; P7 deploy; P6 alert baseline |
| Vào P9 | P8 pilot evidence accepted; P11-3 legal/sign-off package; P11-4 go/no-go |
| PROD | Không còn HARD/LEGAL blocker; DF-03 + DF-07 accepted; rollback/kill-switch tested |

## Rerun bắt buộc sau mỗi batch
Sau mỗi batch thay đổi docs/prompt:
```powershell
node "C:\Users\Administrator\.codex\skills\markdown-doc-reader\scripts\md_doc_map.js" "C:\Users\Administrator\Desktop\ivr" --out "C:\Users\Administrator\Desktop\ivr\.codex-doc-memory"
```

Acceptance cho batch docs: `Unresolved links = 0`, không tạo duplicate title, và `prompt/00-index.md` còn trỏ được tới artifacts mới.
