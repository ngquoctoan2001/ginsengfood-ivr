# W-0224 — Hai remote đồng bộ, một nhánh xoá, và hai claim của tôi bị rút

Ngày: 2026-09-08 · Baseline: `main@03a1922` · Trạng thái: **TESTS_PASS**.

Owner giao ba việc tôi đã đề xuất. **Hai trong ba dựa trên phát hiện sai của chính tôi**, nên lượt
này phần lớn là rút lại chứ không phải thi hành.

## 1. "Mirror GitHub chậm 43 commit" — sai

Tôi báo:

```text
origin/main   03a1922   OK
github/main   b61e744   thiếu W-0193..W-0223   (43 commit)
```

Chạy thật:

```text
$ git push github main
Everything up-to-date

$ git ls-remote --heads github
03a192265ef75117d2a60dedee1069d0481f152f  refs/heads/main
```

**Chưa bao giờ chậm.** `github/main` là **remote-tracking ref**, và nó chỉ đổi khi có
`git fetch github`. Push thì đi qua **pushurl thứ hai của `origin`** — đúng như `CLAUDE.md` mô tả —
nên object tới GitHub mà ref `github/*` local **không hề được đụng tới**. Lần fetch cuối là 05/09,
nên ref đứng ở đó suốt.

Nói cách khác: chính cơ chế làm cho một lệnh push tới được hai remote là cơ chế làm cho ref tracking
của remote thứ hai luôn trông cũ. Bẫy này sẽ bắn lại ở **mọi** lượt audit sau, nên đã ghi vào
`CLAUDE.md` và `AGENTS.md` kèm lệnh đúng phải dùng (`git ls-remote --heads`, hỏi remote chứ không
hỏi ref).

## 2. Nhánh remote còn sót — đúng, đã xoá

`W-0222` dọn ref **local**; nó không nhìn `refs/remotes`. Còn lại:

| | |
| --- | --- |
| ref | `origin/codex/phase-1-2-opus-remediation` |
| commit | `34340cc` (2026-08-14) |
| so với `main` | **ahead 0** / behind 173 |
| `merge-base --is-ancestor 34340cc main` | **true** |

`ahead 0` + ancestor ⇒ mọi commit đã reachable từ `main`; xoá ref mất cái tên, không mất commit.

Xoá bằng **URL GitLab tường minh** chứ không phải `git push origin --delete`: `origin` có hai
pushurl, mà GitHub không có nhánh đó, nên đi qua `origin` sẽ hỏng ở vế thứ hai.

`.githooks/pre-push` cho qua vì hook **tự miễn trừ** thao tác xoá — local oid toàn số không:

```sh
# An all-zero local oid is a deletion, which is allowed.
```

Không cần `IVR_ALLOW_NEW_BRANCH`, không đụng hook.

### Trạng thái ref sau lượt này

```text
GitLab   refs/heads/main  03a1922      (chỉ một)
GitHub   refs/heads/main  03a1922      (chỉ một)
local    main             03a1922
         codex/w0128-w0129-candidate 1fa0150   ← mốc provenance W-0130, giữ
```

## 3. "55 dòng `evidence: null` là nợ hồ sơ" — sai, đã rút

Tôi định trả nợ bằng cách viết `docs/evidence/W-0218/README.md` và `W-0219/README.md`. Đi đọc
`gate-status.mjs:478-491` trước thì thấy **không có nợ nào**, và việc tôi định làm là chính thứ
check đó từ chối.

Script tách **cố ý** hai loại dòng:

| Loại | `evidence: null` | Luật |
| --- | ---: | --- |
| prompt-backed (`^P\d+-\d+$`) | **4** | có DoD đòi evidence pack §10 → **assertion cứng** |
| remediation `UNPLANNED` | **51** | không có prompt ⇒ không có yêu cầu §10; evidence sống trong ô tracker |

Bốn dòng prompt-backed còn `null` — `W-0049` `W-0050` `W-0051` `W-0056` — đều `BLOCKED_EXTERNAL`,
và assertion chỉ soi `TESTS_PASS` / `EVIDENCE_SUBMITTED` / `ACCEPTED`. Evidence **chưa thể** tồn tại
vì việc đang chờ người ngoài. **Vi phạm thật: 0/55.**

51 dòng còn lại không bị giấu — chúng được đếm ra `rows_without_evidence_pack: 167` chính là để sự
khác biệt **nhìn thấy được** thay vì bị đọc thành thiếu.

Và comment trong script nói thẳng vì sao:

> *The first version of this check demanded a directory for all 103 rows and flagged 20 remediation
> items — a rule that would have been satisfied by creating 20 empty directories, which is the
> opposite of what it is for.*

Đó đúng là việc tôi sắp làm cho `W-0218`/`W-0219`. Nên không làm, và bullet trong worklist `3.3` bị
rút cùng lý do.

> **Ghi chú tự chỉ:** `W-0224` cũng là dòng `UNPLANNED`, tức **không bắt buộc** có pack này. Viết vì
> nội dung là một đính chính đáng giữ, không phải để lấp một ô.

## 4. Cái còn thật của `3.3`

**32 commit** subject `save`/`sa ve` reachable từ HEAD. Chưa đụng lượt này.

## 5. Kiểm chứng

```text
git ls-remote --heads origin      1 ref  -> main 03a1922
git ls-remote --heads github      1 ref  -> main 03a1922
git for-each-ref refs/heads       main + codex/w0128-w0129-candidate
git config --get core.hooksPath   .githooks   (không đổi)
dotnet test Ivr.sln               900/900 PASS, 0 failed, 0 skipped
gate-status.mjs                   GATE_STATUS_PASS — 222 work items
contract-freeze-verifier.mjs      CONTRACT_FREEZE=PASS
docs-selftest.mjs                 API_DOCS_SELFTEST_PASS
```

Không sửa code, không thêm/xoá test, không đụng hook. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 6. Bài học

**Ref tracking không phải trạng thái remote.** Cả claim (1) và claim (3) là tôi lặp lại một nguồn —
một ref cũ, một bullet audit — mà không kiểm nguồn gốc, rồi trình con số cho owner trước khi hỏi
đúng chỗ. Hai lệnh rẻ (`ls-remote`, đọc chính script) là đủ để chặn cả hai.
