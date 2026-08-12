# P13 — Generate Final Prompt Library

## Tên nhiệm vụ
Sinh thư mục `prompt/` chính thức ở root sau khi specs đã ổn định.

## Bối cảnh
Sau khi `specs/srs/*` và `integration-requirements/*` đã qua p14 review và tương đối ổn định, tạo bộ prompt chính thức để dev/codex/copilot triển khai từng phần theo `TECH-13` (dev prompt pack) và `TECH-11/12` (roadmap/backlog).

## Input cần đọc
- Toàn bộ `specs/srs/*`, `integration-requirements/*`, `seed/*`
- `docs/documents/3. tech/14-TECH-13-CODEX-COPILOT-DEV-PROMPT-PACK-...md`
- `docs/documents/3. tech/12-TECH-11-...ROADMAP...md`, `13-TECH-12-...PHASE-BACKLOG...md`
- `plan/ivr-orther/16-prompt-roadmap.md`

## Output cần tạo
- `prompt/` (root, chính thức) — theo cấu trúc TECH-13, mỗi prompt gắn requirement ID + source path + evidence expected.
- `prompt/00-index.md` mô tả thứ tự dev handoff.

## Quy tắc
- Mỗi dev prompt phải trace về requirement ID (`IVR-xx-FR-xxx`) + source doc + test/evidence.
- Không prompt nào cho phép bỏ qua release gate hay gọi khách thật.
- Prompt tuân foundation (RBAC/audit/idempotency/evidence).

## Checklist hoàn thành
- [ ] `prompt/` root tồn tại và có index.
- [ ] Mỗi prompt có traceability + evidence expected.
- [ ] Không mâu thuẫn với specs.

## Điều cấm
- KHÔNG chạy p13 trước khi specs ổn định (p01..p12 + p14 xong).
- KHÔNG sinh code production trong prompt library.

## Báo cáo cuối
1. Số dev prompt.
2. Coverage requirement → prompt.
3. Prompt nào chặn vì thiếu specs.
