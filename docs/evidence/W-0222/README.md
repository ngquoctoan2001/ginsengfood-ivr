# W-0222 — Xoá hai nhánh rác, giữ một mốc bằng chứng, và ghi luật xuống

Ngày: 2026-09-07 · Baseline: `main@3287eff` · Trạng thái: **TESTS_PASS**. Câu 3 vẫn mở.

Quyết định của owner cho mục `1.2`: **giữ nhánh `W-0130`, xoá hai nhánh kia**.

## 1. Kiểm trước khi xoá

Xoá ref là thao tác không hoàn tác được nếu commit trở thành unreachable, nên kiểm hai điều trước:

| Nhánh | `ahead main` | Worktree |
| --- | ---: | --- |
| `codex/p03-expand-contract` | **0** | clean |
| `worktree-gd0-fixes` | **0** | clean |

`ahead = 0` nghĩa là **mọi commit của chúng đã reachable từ `main`** — xoá ref không mất commit nào,
chỉ mất cái tên. Cả hai worktree sạch nên không có việc dở dang bị cuốn theo.

## 2. Xoá thế nào mà không đụng thư mục

Cả hai nhánh đang được checkout trong worktree, và git không cho xoá một nhánh đang được checkout.
Cách thẳng nhất là `git worktree remove` — nhưng nó **xoá thư mục**, mà một trong hai nằm **ngoài
repo** (`Desktop/ivr-p03-expand-contract`).

Owner trả lời câu 1 và câu 2, **không trả lời câu 3** (có gỡ worktree không). Nên:

```text
git -C <worktree> checkout --detach     # nhả nhánh, giữ nguyên thư mục
git branch -d codex/p03-expand-contract # → Deleted (was d5539ba)
git branch -d worktree-gd0-fixes        # → Deleted (was cc12e53)
```

`-d` chứ không `-D`: `-d` **từ chối** nếu nhánh chưa merge. Nó chấp nhận, tức git tự xác nhận lại
điều `ahead = 0` đã nói.

Kết quả: hai ref biến mất, **không thư mục nào bị xoá**.

## 3. Mốc provenance còn nguyên

```text
git rev-parse codex/w0128-w0129-candidate  → 1fa01507639c8bed5e64421b62bd2d785cbc9c26
git cat-file -t 1fa0150…                   → commit
docs/evidence/W-0130/README.md             → cùng SHA
```

Local branch nay đúng hai: `main` và mốc.

## 4. Luật đã ghi xuống — đây mới là phần bền

Owner giữ nhánh, nên mâu thuẫn phải được gỡ chứ không để treo: `CLAUDE.md` cấm nhánh tuyệt đối và
bảo xoá nhánh lạ, còn `W-0130` cần một nhánh sống lâu dài. Cả hai không thể cùng đúng mãi, và cái
giá của việc để treo đã thấy rồi — bản audit 07/09 khẳng định *"repo chỉ có main"* ở **16 hàng**,
sai theo chiều ngược lại.

Đã thêm vào `CLAUDE.md` **và** `AGENTS.md` (hai file dùng chung nội dung) một mục miễn trừ:

- đích danh **một** ref, kèm lý do và trỏ tới evidence;
- nói rõ `ahead 1` là **thiết kế**, không phải việc chưa dọn;
- và nói rõ nó **không cho phép gì cả**: không nhánh mới, không sửa hook, không nới deny rule.

Kiểm lại sau khi sửa: `core.hooksPath` vẫn trỏ `.githooks`, hai hook `pre-push` và
`reference-transaction` **không bị đụng**. Miễn trừ này là markdown, không phải cấu hình.

## 5. Còn lại — câu 3

Hai thư mục giờ là worktree **detached, không còn nhánh**:

| Thư mục | Ở đâu |
| --- | --- |
| `Desktop/ivr-p03-expand-contract` | **ngoài repo** |
| `.claude/worktrees/gd0-fixes` | trong repo, gitignored |

`git worktree list` vẫn thấy cả hai. Gỡ hay giữ là câu 3, chưa trả lời — và một trong hai nằm ngoài
repo nên tôi không tự xoá. Nếu gỡ: `git worktree remove <path>` cho từng cái.

Lưu ý nhỏ: bên trong `ivr-p03-expand-contract` còn một worktree lồng
(`ci-artifacts/expand-contract/…/previous-source`, detached ở `ba43605`) — gỡ thư mục cha thì phải
gỡ nó trước hoặc dùng `--force`.

## 6. Kiểm chứng

```text
git branch -a                          main + codex/w0128-w0129-candidate (local)
git rev-parse codex/w0128-w0129-…      1fa0150… còn nguyên, evidence W-0130 vẫn resolve
git config --get core.hooksPath        .githooks (không đổi)
dotnet test Ivr.sln                    900/900 PASS, 0 failed, 0 skipped
gate-status.mjs                        GATE_STATUS_PASS — 220 work items
contract-freeze-verifier.mjs           CONTRACT_FREEZE=PASS
docs-selftest.mjs                      API_DOCS_SELFTEST_PASS
```

Không sửa code, không sửa test. `REAL_CUSTOMER_CALL_ALLOWED=NO`.
