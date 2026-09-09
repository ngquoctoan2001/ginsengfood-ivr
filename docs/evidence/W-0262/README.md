# W-0262 — 942 MB rác dọn xong, và một phát hiện tôi phải rút lại

Ngày: 2026-09-09 · Baseline: `main@d9e82e4` · Trạng thái: **TESTS_PASS**.

S7 và S6 trong [`docs/review/2026-09-09-codebase-audit.md`](../../review/2026-09-09-codebase-audit.md).

**S7 xong. S6 sai — rút lại, không phải hoãn.**

## 1. S7 — worktree rác

### 1.1 Kiểm trước, vì `CLAUDE.md` cấm đụng một ref

`CLAUDE.md` nêu đúng một ngoại lệ: `codex/w0128-w0129-candidate` là **provenance anchor** của
`W-0130`, và xoá ref đó sẽ làm commit `1fa0150` không còn với tới được, phá chuỗi bằng chứng của
`W-0128`/`W-0129`.

Worktree của nó nằm trong danh sách `prunable`. Nên trước khi chạy bất cứ thứ gì:

```
$ git worktree prune --dry-run --verbose
Removing worktrees/ivr-p03-expand-contract: gitdir file points to non-existent location
Removing worktrees/ivr-w0128-w0129-candidate: gitdir file points to non-existent location
Removing worktrees/previous-source: gitdir file points to non-existent location
```

`prune` gỡ **bản ghi worktree**, không gỡ ref. Đã xác nhận sau khi chạy:

```
$ git rev-parse --verify codex/w0128-w0129-candidate
1fa01507639c8bed5e64421b62bd2d785cbc9c26
$ git cat-file -t 1fa0150
commit
```

Ref còn nguyên, commit vẫn với tới được.

Ba bản ghi bị gỡ đều trỏ vào thư mục **đã biến mất khỏi đĩa**. Đáng chú ý nhất là cái thứ ba:
`ci-artifacts/expand-contract/…/previous-source` — một worktree đăng ký **bên trong** một thư mục
gitignore bị xoá định kỳ. Nó thành `prunable` vì chính lý do đó. Đăng ký worktree vào thư mục
artifact là footgun kiến trúc, không phải tai nạn một lần.

### 1.2 Bản sao repo 942 MB

`.claude/worktrees/gd0-fixes` không `prunable` — nó tồn tại thật, ở commit cũ `cc12e53`, kèm
`node_modules`, `.next`, `third_party/vieneu-tts` và toàn bộ `admin-ui` mà `c0e6609` đã xoá khỏi
`main`.

```
942 MB · 31.966 tệp
```

Kiểm hai điều trước khi xoá:

```
$ git merge-base --is-ancestor cc12e53 main   # → ancestor của main
$ git -C .claude/worktrees/gd0-fixes status --porcelain | wc -l   # → 0
```

`cc12e53` là **ancestor của main** và cây làm việc **không có thay đổi chưa commit**. Nó không giữ
gì mà `main` chưa có. Gỡ bằng `git worktree remove --force` chứ không phải `rm -rf`, để bản ghi
admin được dọn cùng.

Còn sót một thư mục rỗng tên `gd0-fixes;W` — dấu vết của một lần xoá hỏng trên Windows, 0 tệp, đã
dọn.

`git fsck` sau đó báo vài `dangling tree`/`dangling blob`: đó là object mất tham chiếu chờ `gc`,
không phải hỏng hóc.

### 1.3 Không có gì để commit

`.claude/worktrees/` bị gitignore và `.git/worktrees/` là admin state, nên toàn bộ mục 1 **không tạo
ra diff nào**. Ghi lại ở đây vì kết quả có thật (942 MB, 31.966 tệp, 4 bản ghi worktree) nhưng
`git log` sẽ không bao giờ cho thấy nó.

### 1.4 Một dòng `.gitignore` trùng — và ba dòng KHÔNG phải rác

`.claude/worktrees/` xuất hiện hai lần (dòng 16 và 61). Giữ bản có ghi work-id (`W-0193`), xoá bản
không. Đã kiểm quy tắc vẫn hiệu lực sau đó:

```
$ git check-ignore -v .claude/worktrees/probe/x
.gitignore:16:.claude/worktrees/    .claude/worktrees/probe/x
```

**Nhưng `admin-ui/out/`, `admin-ui/.env*` và `admin-ui/` trong `.dockerignore` thì giữ nguyên** —
và `C2` trong bản rà soát gọi chúng là "lạc hậu" là **sai một nửa**. Chúng là **guard**, đúng theo
tiền lệ mà chính repo đã viết ra cho `deployment-ui.yaml`:

> `ui.enabled=true is forbidden: W-0128 removed the UI from the deployable topology and W-0253
> deleted the directory.`

Template đó được giữ lại **cố ý**, vì xoá nó biến cờ `ui.enabled` thành no-op im lặng. Cùng lập
luận áp cho các dòng ignore: nếu ai đó dựng lại `admin-ui`, chúng giữ nó ngoài git và ngoài image.
Xoá đi là đổi một guard lấy vẻ gọn gàng.

## 2. S6 — rút lại

### 2.1 Tôi đã kết luận sai từ một mẫu

Bản rà soát viết `docs/documents/` là "434.000 dòng tài liệu **không liên quan**" và đề xuất "tách
sang repo riêng".

Tôi lấy bốn file lớn nhất theo số dòng, thấy Facebook gateway / ads ROAS / MC AI live-sales /
commerce runtime, rồi suy rộng cho cả 179 file. Đó là kết luận rút từ đầu một danh sách đã sắp theo
kích thước.

Đo lại cho đủ:

| | |
| --- | ---: |
| Tài liệu đặt tên riêng cho IVR | **8 file / 17.810 dòng** |
| Tài liệu có nhắc IVR | **105 / 179** |

Trong đó có `2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md` (7.385 dòng) và
`3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-...md` (8.337 dòng) — **đặc tả
nghiệp vụ của chính module này**.

### 2.2 Và nó là source of truth được tuyên bố

`prompt/README-governance.md:20`:

> `docs/documents/` **là business source để truy nguyên**; nó không tự chứng minh implementation
> hiện tại. Nếu Target V1 khác business source, delta phải được ghi ở
> `specs/_review/open-decisions-register.md` kèm owner, không được im lặng.

Bảy bản ghi evidence trích dẫn tài liệu cụ thể trong đó: `W-0057`, `W-0058`, `W-0136`, `W-0151`,
`W-0217`, `W-0238`, `W-0247`. `W-0217` trích thẳng `PACK-09-IVR-ORDER-CONFIRMATION.md` mục *"Các kết
quả tạm thời"*; `W-0247` ghi *"chỗ duy nhất trong toàn bộ `docs/documents/` nêu
`golden_hour_session_id`"*.

Làm theo đề xuất của tôi sẽ **cắt đứt chuỗi truy nguyên mà governance yêu cầu và làm mồ côi bảy
trích dẫn evidence** — đúng loại thiệt hại mà `W-0251`/`W-0252` tồn tại để tránh, và là thứ tôi đã
cẩn thận giữ suốt `W-0258`, `W-0260`, `W-0261`.

### 2.3 Cái còn đúng

434.464 dòng thật sự làm chậm `clone`, `grep -r` và mỗi lần index. Nhưng đó là **cái giá của việc
giữ corpus truy nguyên**, không phải một khoản lãng phí để cắt. Tên thư mục có dấu cách
(`2. pack`, `3. tech`) cũng thật — nhưng gate sweep xanh, nên hiện chưa có gì vỡ vì nó.

Phát hiện được đánh dấu **RÚT LẠI** tại chỗ trong bản rà soát, kèm lý do, chứ không lặng lẽ gỡ
xuống.

## 3. Kết quả

```
Unit         663/663
Integration  278/278
Contract      24/24
Chaos          8/8
             ─────────
             973/973
```

`GATE_SWEEP_PASS` · `dotnet build Ivr.sln` — 0 warning, 0 error.

Không đổi mã nguồn. Diff duy nhất là một dòng `.gitignore` trùng, cộng bản rà soát và evidence này.

## 4. Còn lại

**15/26 đã đóng, 1 rút lại vì sai.** Không còn HIGH.

Cần owner:

- **P2** — 109 index. Cần `pg_stat_user_indexes.idx_scan` từ staging hoặc production.
- **Retention chưa arm** — `PeriodDays` rỗng.
- **`"MOCK"` có hai chủ sở hữu** — tách tên xong thì 22 site thay được an toàn.

Thuần kỹ thuật, không nguy hiểm: **S1** (God class 1.265 dòng / interface 16 method), **S3** (5
background host copy-paste), **S5** (schema wire định nghĩa hai lần), phần S4 còn lại, **B9**,
**B10**, **C1**, **C3**, **C4**, **C5**.

> `C2` cũng cần đính chính: dòng `.claude/worktrees/` trùng là thật và đã xoá, nhưng các entry
> `admin-ui` là guard chứ không lạc hậu — xem mục 1.4.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN
