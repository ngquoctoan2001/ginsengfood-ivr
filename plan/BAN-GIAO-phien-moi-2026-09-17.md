# Bàn giao phiên — dán nguyên file này vào phiên mới

> Copy toàn bộ nội dung dưới đây làm tin nhắn đầu tiên ở account mới.
> Lập `2026-09-17` tại `main@7c4204e`.

---

## 0. Bạn là ai trong việc này

Tôi là **Toàn**, dev **duy nhất** của **Module 8 — IVR gọi xác nhận đơn hàng** (repo
`C:\Users\Administrator\Desktop\ivr`, tên index GitNexus `ginsengfood-ivr`).

**Thẩm quyền, phải hiểu đúng ngay từ đầu:**

| Ai | Quyết gì |
| --- | --- |
| **Tôi** | **Toàn bộ quyết định kỹ thuật A–Z của Module 8** |
| **Sếp** | Tiền, dịch vụ ngoài, rủi ro cấp công ty. **Không rành kỹ thuật** — viết cho sếp phải bỏ hết thuật ngữ |
| **Tech lead** | Viết **Module 3**. **Không có quyền quyết gì trong Module 8** |

⚠️ **Tổ chức này KHÔNG có đội Platform, Security, Legal, Sales.** Nhiều tài liệu cũ trong repo
gửi phiếu cho các đội đó — chúng **không tồn tại**. Tiền lệ xử lý đã có: `OD-V1-11` đóng ngày
`10/09` với *"owner tuyên bố quorum là chính mình, nhận rủi ro, ghi thành văn bản"*. Gặp phiếu mồ
côi thì đưa về **sếp** (tiền/rủi ro) hoặc **tôi** (kỹ thuật), đừng treo chờ một phòng ban tưởng
tượng.

---

## 1. Luật cứng — đọc `CLAUDE.md` trước khi làm gì

**Repo chỉ có `main`. Tuyệt đối không tạo branch/worktree mới** — có git hook cưỡng chế, và
`codex/w0128-w0129-candidate` là nhánh **cố ý giữ**, không được xoá. Không sửa `.githooks/`,
`core.hooksPath`, hay deny rule.

**GitNexus bắt buộc:**
- Chạy `gitnexus_impact` **trước khi sửa** bất kỳ symbol nào, báo blast radius
- Chạy `gitnexus_detect_changes` **trước khi commit**
- **Cảnh báo tôi** khi `HIGH`/`CRITICAL`
- **Không bao giờ** rename bằng find-and-replace

**Mọi evidence pack và dòng tracker phải mang `REAL_CUSTOMER_CALL_ALLOWED=NO`.**

---

## 2. Đang ở đâu — `main@7c4204e`

```
7c4204e  W-0311 stage 2, option B on the wire and on the dial path   ← HEAD, của tôi
d24b92b  W-0310 the sheet was still telling M3 to build a token issuer
ca8d13b  W-0310 stage 1  ← ⚠️ TIÊU ĐỀ SAI ID, thật ra là stage 1 của W-0311
5f00838  W-0310 one sheet for Module 3, not three
d655989  W-0309 route the group-A sheets to people who actually exist
```

`NEXT_WORK_ID` = **`W-0312`** (`prompt/_execution/prompt-execution-tracker.md` §2).

**Trạng thái xanh tại HEAD:** `1151/1151` test (`781` unit · `338` integration · `24` contract ·
`8` chaos) · gate sweep **`39/39`** · traceability `721` · pin sweep `44` đường dẫn `0` lệch.

### ⚠️ `7` file trong cây **chưa commit và KHÔNG phải của tôi** — đừng đụng, đừng `git add -A`

```
 M AGENTS.md · CLAUDE.md · deploy/ci/migration-expand-baseline.json
 M docs/compliance/ivr-pdpa-legal-basis-pack.md · dotnet-tools.json · specs/api/06-error-codes.md
?? docs/reports/2026-09-15-De-xuat-phuong-an-goi-dien-xac-nhan-don-hang.docx
```

Chúng có từ trước, đã bị loại khỏi **mọi** commit của tôi. Luôn commit bằng
`git commit -- <đường dẫn cụ thể>`, **không bao giờ** `git add -A` / `git add .`.

### Có phiên Claude khác cùng ghi vào cây này

Chạy `ListAgents` trước khi commit. Phiên `ivr-98` vừa làm phiếu M3 (`W-0310`) và đã **bắt được
một lỗi của tôi** — hợp tác tốt, cứ nhắn qua `SendMessage` khi đụng cùng file.

---

## 3. Việc đang dở — `W-0311`, phương án `B`

**Quyết định của sếp `17/09`:** Module 3 **gửi thẳng số điện thoại** (`phone_e164`), thay vì phát
hành `dial_token` để M8 giải mã bằng khoá do Platform giữ.

**Được:** bỏ kho khoá · bỏ bộ phát hành token M3 chưa xây · `OD-V1-05`/`17`/`18` hết đối tượng.
**Mất:** ⚠️ **DB của IVR từ nay giữ số khách ở dạng thường** — đã ghi ở `3` nơi (entity,
migration, `docs/compliance/data-inventory.md`).

### Đã xong (stage 1 + 2)

- Migration expand `phone_e164` **nullable**, `3` cột dial-token **giữ nguyên, không drop**
- OAS `draft.29` → **`draft.30`**, field **tuỳ chọn** ⇒ **không breaking** (`oasdiff` `exit 0`)
- `DialTokenResolutionRequest` + `TelephonyDispatchContext` thêm `DirectPhoneE164` (optional, có
  mặc định) · `ProductionDialTokenVault` ưu tiên số trực tiếp
- `UT-TRUNK-DIAL-08..12` · changelog `29→30` · baseline `draft.30.yaml` đóng băng

### 🔴 Còn `3` stage — **cả ba đều CHỜ M3, không phải chờ ngày công**

| Stage | Việc | Mở khoá khi |
| --- | --- | --- |
| `3` | `phone_e164` thành **bắt buộc**, `dial_token` thành optional | **Đây mới là lần breaking.** Chỉ sau khi M3 xác nhận đã gửi số |
| `4` | `OD-V1-05` · `OD-V1-17` · `OD-V1-18` → `SUPERSEDED`; đóng luôn `B11` bản `16/09` | Cùng lúc stage `3` |
| `5` | Drop `3` cột dial-token | Sau khi M3 cắt hẳn |

**Đừng làm stage 3 sớm.** Đó là ranh giới expand/contract:
`deploy/ci/migration-expand-baseline.json` ghi rõ *"no drop-table exception in the current expand
phase"*, và một deploy gỡ cột mà pod cũ còn đọc thì sập dịch vụ suốt lượt rollout.

---

## 4. 🟡 Việc đang chờ **sếp trả lời**, tôi không tự quyết

**Mục `4` của [phiếu cho sếp](ivr-orther/phieu-quyet-dinh-cho-sep-2026-09-17.md) — VieNeu-TTS:
✅ sếp đã chốt `17/09` (`S4`), ghi xong `18/09` (`W-0315`).** VieNeu tự host là bộ đọc duy nhất,
production lẫn lab (`OD-V1-19`): chạy trên server công ty thì không có dữ liệu nào rời hệ thống.

Ba phiếu `platform-w0122`, `security-w0122-cve`, `legal-od-voice-07` đã chuyển về `S5` và `S2`
(rủi ro `3`, `4`); việc còn lại là Lô `4` của
[vướng mắc `17/09`](ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md). `16` finding (`13 HIGH` +
`3 CRITICAL`, `0` cái có bản vá, toàn bộ thuộc Debian 13.6 — `W-0185` đã đo `3 CRITICAL` **không với
tới được**). `deploy/helm/ivr/values-prod.yaml` vẫn đặt `tts.enabled: false` cho tới khi đủ cổng.

**Chờ tôi cấp:** một tài khoản GitLab thứ hai có quyền duyệt (sếp đã duyệt, chưa tạo).

---

## 5. Cách tôi muốn bạn làm việc

Đây là phần quan trọng nhất. Những thói quen dưới đây đã bắt được lỗi thật nhiều lần.

### 5.1. Kiểm bằng cây, không kiểm bằng trí nhớ

**Mọi mệnh đề chịu lực phải đối chiếu repo trước khi viết ra.** Kể cả mệnh đề trong kế hoạch **do
chính tôi viết** — `W-0307` cho thấy `3/4` dòng kế hoạch tôi viết vài giờ trước là **sai**, và
`W-0308` cho thấy chiều ngược lại (kế hoạch **đòi ít hơn** thực tế đã có).

⚠️ **Bẫy nguy hiểm nhất: một câu mình đã lặp lại nhiều lần là câu ít bị kiểm nhất** — mỗi lần lặp
nó mượn uy tín của lần trước chứ không mượn bằng chứng. Tôi đã ba lần nói *"gửi `6` phiếu nhóm A,
tốn `0` ngày công"* và **cả hai tiền đề đều sai**. Câu phá vòng đó là **"lần cuối đối chiếu với
cây là khi nào?"**, không phải *"nghe có đúng không?"*.

### 5.2. Test xanh **không** phải bằng chứng đủ

`W-0307`: **`11` test tôi tự viết đều xanh** với một bản code sai, vì chúng deserialize vào chính
record đó và vòng round-trip luôn tự khớp. Thứ bắt được là một gate đọc **file OpenAPI** — một
**hiện vật độc lập** với code.

⇒ Ưu tiên gate đọc hiện vật độc lập hơn test viết cùng lúc với code.

### 5.3. Mutation test hai chiều cho **mọi** mệnh đề hành vi

Đục thủng code, chạy lại, xác nhận test **đỏ**, rồi khôi phục. Ghi kết quả vào evidence. Không
nhận một test là bằng chứng nếu chưa thấy nó đỏ được.

### 5.4. Nói thẳng cái **cố ý không làm**, và vì sao

Mỗi lô đều có mục *"việc cố ý KHÔNG làm"*. Ví dụ: `W-0307` không tự cấp permission thứ tám vì
`DF-01` **LOCKED `7` quyền, do Permission Core sở hữu** — tự cấp là M8 ký vào sổ của người khác.

### 5.5. Tự báo lỗi của mình, kể cả khi không ai hỏi

Nếu bản đánh giá của tôi đòi người khác chính xác thì bản của tôi phải chịu cùng chuẩn. Đã tự đính
chính nhiều lần (con số `183` → `44`; `7` lô → `8` lô; tiền đề `6` phiếu). **Đính chính rõ ràng,
không lặng lẽ sửa số.**

### 5.6. Cảnh báo `CRITICAL` nghĩa là **đọc lại thiết kế**, không phải xin duyệt rồi sửa liều

`W-0311`: `IDialTokenResolver` trả `CRITICAL` (`214` ký hiệu). Đọc kỹ thì phát hiện
`DialAuthorization` cũng chặn số trần ⇒ đường quay số **chưa bao giờ mang số**, nó mang **địa
chỉ** ⇒ phương án `B` thu về **một nhánh `if`**, không đụng `214` ký hiệu kia.

### 5.7. Sổ sách — mỗi lô phải cập nhật đủ

1. `prompt/_execution/prompt-execution-tracker.md` — §2 control (`NEXT_WORK_ID`, `Last allocated`,
   `Last activity sequence`), §5 register (**`9` ô**), §7 activity log (**`7` ô**)
2. `docs/evidence/W-XXXX/README.md`
3. Kế hoạch nguồn của lô + `plan/toan-viec-can-lam-m8-2026-09-16.md` (bản của chief auditor) +
   `plan/PHAN-HOI-toan-viec-can-lam-m8-2026-09-16.md`
4. `node deploy/ci/scripts/gate-status.mjs --write`

⚠️ Status trong tracker là **từ vựng đóng**: `PLANNED` `NOT_STARTED` `IN_PROGRESS` `CODE_DONE`
`TESTS_PASS` `EVIDENCE_SUBMITTED` `ACCEPTED` `BLOCKED_INTERNAL` `BLOCKED_EXTERNAL`
`DEFERRED_TARGET` `N/A` `CANCELLED`. **Bịa status mới thì gate đỏ** (tôi đã bịa `DOCS_ONLY`).

⚠️ **Không tự chuyển mục xuống nhóm `D`** của bản `16/09` — đó là việc của chief auditor. Ghi chú
tại chỗ kèm bằng chứng, đúng như chính bản đó yêu cầu: *"sai thì phản hồi lại kèm bằng chứng,
đừng sửa im lặng."*

---

## 6. Bẫy kỹ thuật đã cắn, đừng cắn lại

| Bẫy | Cách tránh |
| --- | --- |
| **CRLF/LF khi tính hash** | `core.autocrlf=true`, git lưu **LF**. Luôn hash trên **byte LF** (`read().replace(b"\r\n", b"\n")`). Pin tính trên worktree Windows sẽ **xanh ở máy, đỏ ở CI** |
| **File lẫn CRLF và LF** | `plan/toan-viec-can-lam-m8-2026-09-16.md` lẫn hai kiểu. Dùng `splitlines(keepends=True)`, **không** `split(NL)` — nó dính dòng lại và tìm không thấy |
| **Replace version trần** | `sed 's/1.0.0-draft.29/…30/'` **rewrite luôn tên file** `…draft.28-to-v1.0.0-draft.29.md`. Neo vào backtick/khoảng trắng |
| **Heredoc trong Git Bash** | Nuốt backslash — regex Python hỏng. Dùng **Write tool** viết script rồi chạy |
| **Docker `-v` trong Git Bash** | Mangle đường dẫn container. Dùng **PowerShell tool** cho `oasdiff` |
| **Console cp1252** | `print()` tiếng Việt trong script Python **crash**. Chỉ print ASCII |
| **Đổi contract ⇒ dây chuyền `~8` nơi** | OAS · `contract-manifest.json` · `portal-manifest.json` · `docs/api-changelog.md` · `docs/api/changelog/<from>-to-<to>.md` · baseline yaml · `IR-06`/`IR-07` version pointer · `5` validator pin `IR-06`. Chạy `gate-sweep.mjs` cho tới `39/39` |
| **Hash cố ý viết dạng `\uXXXX`** | `W-0183` template có một ký tự escape — đó là **fixture** chứng minh parser giải escape trước khi so. Re-pin phải **giữ nguyên dạng đó** |

### Lệnh hay dùng

```bash
dotnet test Ivr.sln --nologo                        # 1151/1151
node deploy/ci/scripts/gate-sweep.mjs               # 39/39
node deploy/ci/scripts/gate-status.mjs --write
node deploy/ci/scripts/generate-test-traceability.mjs
pnpm db:migration:add <Ten>                         # KHÔNG dùng dotnet ef trực tiếp
```

Migration sinh ra **vi phạm analyzer** (`IDE0161` file-scoped namespace, `CA1062` null check) —
phải sửa tay theo mẫu migration gần nhất.

---

## 7. Việc đầu tiên nên làm ở phiên mới

1. Đọc `CLAUDE.md`, `plan/ivr-orther/00-CHUA-XONG.md`, và
   `docs/evidence/W-0311/README.md` (trạng thái phương án `B`)
2. Chạy `ListAgents` xem phiên nào đang cùng ghi
3. Xác nhận xanh: `dotnet test Ivr.sln` và `gate-sweep.mjs`
4. Hỏi tôi: **M3 đã phản hồi chưa** — vì stage `3` của `W-0311` chặn ở đó (VieNeu đã chốt `17/09`,
   `S4`)

**Đừng đi tìm việc code khác.** Tính tới `17/09`, mọi việc lập trình **không phụ thuộc bên ngoài
đã hết**: kế hoạch khắc phục `W-0298`→`W-0307` xong, kế hoạch đường gọi production `PD-01`/`02`/
`03` xong. Phần còn lại chờ **M3**, **nhà mạng**, hoặc **sếp**. Nếu thấy mình đang bịa việc ra
làm — dừng và hỏi tôi.
