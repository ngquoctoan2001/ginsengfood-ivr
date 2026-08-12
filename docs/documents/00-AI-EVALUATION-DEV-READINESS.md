# AI Evaluation - Dev Implementation Readiness

Trạng thái: `AI_REVIEW_GUIDE_FOR_DEV_IMPLEMENTATION_READY`
Phạm vi: dùng khi gom bộ tài liệu Phase 1-8 để ChatGPT hoặc reviewer AI đánh giá.

## 1. Câu hỏi cần đánh giá

Reviewer chỉ chấm câu hỏi sau:

```text
Bộ tài liệu Phase 1-8 đã đủ rõ, đủ khóa nguồn, đủ ranh giới và đủ tiêu chí để giao dev triển khai có kiểm soát chưa?
```

Reviewer không chấm câu hỏi production trong vòng đọc này.

## 2. Không được hiểu nhầm thành production approval

Các trạng thái sau là gate vận hành sau khi dev triển khai, không phải mục tiêu chấm của vòng tài liệu:

- `PRODUCTION_READY`
- `RELEASE_READY`
- `GO_LIVE_APPROVED`
- `GLOBAL_SMOKE_PASS`
- `COMPLETION_PASS`
- `OWNER_SIGN_OFF`
- evidence accepted từ runtime thật

Nếu tài liệu ghi `NOT_RELEASE_READY`, `NOT_PRODUCTION_READY`, `PRODUCTION_BLOCKED`, `GLOBAL_GATEWAY_BLOCKED` hoặc yêu cầu smoke/evidence/owner sign-off, đó là guard đúng. Không được coi đó là lỗi làm bộ tài liệu chưa đủ để dev bắt đầu triển khai.

## 3. Cách hiểu các từ khóa trong Phase 1-8

| Cụm từ | Cách hiểu đúng khi chấm dev-readiness |
| --- | --- |
| `ANALYSIS ONLY` | Lane phân tích bắt buộc trước dev task, không phải dấu hiệu tài liệu thiếu. |
| `TECHNICAL DESIGN ONLY` | Lane thiết kế kỹ thuật trước implementation, không phải production claim. |
| `READY_FOR_OWNER_REVIEW` | Hồ sơ đủ để owner/reviewer xem xét, không phải release approval. |
| `PASS cục bộ` hoặc `local-ready` | Kết quả trong phạm vi nhỏ, không tự nâng thành global pass. |
| `Smoke`, `Evidence`, `Release gate` | Điều kiện kiểm chứng phải triển khai và thu thập sau dev, không phải bằng chứng đã pass nếu chưa có artifact runtime. |
| `Done gate` | Tiêu chí kết thúc task/lane, không phải go-live. |

## 4. Tiêu chí đạt chuẩn để giao dev triển khai

Bộ tài liệu đạt `DEV_IMPLEMENTATION_DOCUMENTATION_READY` khi có đủ:

1. Source-of-truth và owner boundary rõ.
2. Phạm vi trong/ngoài scope rõ.
3. Dependency giữa Phase/Pack/Tech rõ.
4. API, schema, event, state machine hoặc workflow cần triển khai được mô tả đủ để bóc task.
5. Data model, validation, idempotency, audit/evidence và permission được khóa theo rủi ro.
6. Negative cases và fail-closed rule rõ.
7. Smoke/evidence plan rõ để dev biết phải chứng minh gì sau khi triển khai.
8. Handoff/backlog/task sequencing đủ để giao việc theo lane.
9. Blocker, owner decision và runtime gate còn lại được ghi rõ, không bị che hoặc gọi nhầm là pass.

## 5. Kết luận mẫu đúng cho vòng đánh giá này

Nếu các tiêu chí ở mục 4 đã đủ trong bộ upload, reviewer nên kết luận:

```text
DEV_IMPLEMENTATION_DOCUMENTATION_READY: YES
READY_TO_HAND_TO_DEV: YES
PRODUCTION_READY: NO
RUNTIME_EVIDENCE_REQUIRED_AFTER_DEV: YES
OWNER_RELEASE_DECISION_REQUIRED_AFTER_EVIDENCE: YES
```

Kết luận trên có nghĩa là tài liệu đủ chuẩn để dev triển khai có kiểm soát. Nó không có nghĩa là hệ thống đã chạy production, đã pass smoke thật hoặc đã được owner duyệt go-live.
