# W-0117 — Contract và documentation hygiene sau draft.17

Ngày: `2026-08-24`

Baseline: `main@54ca239`

Trạng thái: `TESTS_PASS`

## 1. Kết quả

- `openapi:lint` từ đúng **14 lỗi** về **0**: 13 `nullable: true` kiểu
  OpenAPI 3.0 được đổi sang null union tương đương của OpenAPI 3.1.
- Contract IVR tăng `1.0.0-draft.17 → 1.0.0-draft.18` và công bố create-draft
  ở path chuẩn `POST /scripts`.
- Runtime không breaking: `POST /scripts/` vẫn được ASP.NET route group nhận;
  integration test khóa cả path chuẩn và alias cũ.
- NSwag `14.7.1` regenerate báo **no change detected** cho cả hai generated
  output. Nullability và public C# DTO surface không đổi.
- README, Admin API, readiness board generator, changelog và kế hoạch ngày
  22/8 đã được đối soát với source hiện tại.

## 2. Impact trước sửa

GitNexus index ở `main@54ca239` và up-to-date. `createScriptDraft` và
`MapIvrScriptLifecycleEndpoints` là LOW, test harness là MEDIUM (6 caller,
0 execution flow). Các generated DTO bị chấm HIGH (18–20 direct; cao nhất
`IvrSeedLoadRequest` 94 symbol/18 direct, 0 flow) vì nhiều DTO nằm chung một
generated file. Rủi ro HIGH đã được cảnh báo trước khi sửa.

Giảm thiểu: chỉ đổi cú pháp null tương đương; codegen phải không đổi; path cũ
được giữ làm alias; chạy build, focused integration/contract tests và pinned
`oasdiff` trước khi đóng work item. `renderBoard` LOW (1 direct, 0 flow).

## 3. Đồng bộ nguồn sự thật

| Drift | Sau W-0117 |
| --- | --- |
| README còn mô tả empty worker/placeholder readiness | mô tả workflow/jobs hiện hành; `/health/ready` fail-closed của W-0040 |
| Account delete ghi `/{id}:delete` | đúng source: `DELETE /accounts/{accountId}` |
| Role matrix ghi Admin 11, Operator 4 | đúng source: Admin 22, Operator 5 |
| Readiness board nói không cắt active call | nêu cơ chế W-0111, poll mặc định ≤500 ms, tách kill switch và ghi rõ mới là software/MOCK evidence |
| Kế hoạch 22/8 còn mô tả A1–A10 là backlog | thêm bảng as-built: cả A1–A10 đều `TESTS_PASS`, không đánh đồng với `ACCEPTED`/UAT |

## 4. Gate evidence

| Gate | Kết quả |
| --- | --- |
| `openapi:lint` | PASS — 2 tài liệu, 0 lỗi/0 warning |
| `openapi:validate` | PASS — 2 OpenAPI; target tasks 9; negative schema 12 + domain negative 13 |
| NSwag codegen | PASS — `OPENAPI_CODEGEN_COMPLETE=YES`; generated output không đổi |
| reviewed draft + drift | PASS — baseline updated; `OPENAPI_HASHES_PINNED=3`; human diff current |
| pinned `oasdiff v1.26.1@sha256:aae8…` | PASS — no breaking change cho cả IVR và callback; `CT-DOC-02` PASS |
| portal/docs | PASS — 12 artifact; `API_DOCS_SELFTEST_PASS`; link/topology/PII boundary PASS |
| `dotnet build Ivr.sln -c Release --no-restore` | PASS — 0 warning, 0 error |
| focused integration | PASS — 11/11 (`ScriptLifecycleApiTests` + `AdminConfigApiTests`) |
| Release contract tests | PASS — 22/22 |
| admin-ui contract drift | PASS — 16/16 |
| admin-ui | target-file lint + typecheck + production build PASS |
| gate-status mirror | PASS — tracker → YAML/readiness board, no readiness rung claimed |

## 5. Ranh giới kết luận

`TESTS_PASS` chỉ đóng hygiene nội bộ. Hosted CI/deploy, owner listening/UX,
Sales sandbox/CDC, SIM/carrier UAT và real-customer call đều `NOT_RUN` hoặc vẫn
theo external gate riêng. `REAL_CUSTOMER_CALL_ALLOWED=NO`. W-0117 không cấp
quyền mới, không mở provider, không đóng Target V1 closure pack và không tạo
bằng chứng production.

Các thay đổi UI khác đang có sẵn trong shared checkout được giữ nguyên và không
được tính vào W-0117.
