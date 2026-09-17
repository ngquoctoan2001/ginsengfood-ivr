# W-0313 — Lô 3 phần một: năm mục Toàn duyệt ngày `17/09`

**Ngày:** `2026-09-17` · **Baseline:** `main@628676d` · **Nguồn:** mục `15`–`19` Lô 3 của
[bản vướng mắc `17/09`](../../../plan/ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md), Toàn trả lời `17/09`

`REAL_CUSTOMER_CALL_ALLOWED=NO`

---

## 0. Toàn trả lời

| # | Câu hỏi | Trả lời |
| --- | --- | --- |
| 1 | Push `f7bb52b` và `628676d` | **Đã push** — `git ls-remote` cho thấy `origin` và `github` cùng ở `628676d` |
| 2 | Gửi Module 3 bản đính chính `draft.31` | **Đã gửi**, chưa có phản hồi |
| 3 | `S3` *"giữ dữ liệu vĩnh viễn"* là toàn bộ dữ liệu hay chỉ nhật ký quản trị | **Toàn bộ dữ liệu** |
| 4 | Đưa mục `15`–`18` vào Lô 3 | **Duyệt, làm ngay** |

---

## 1. Mục `15` — job CI `pii_scan` đỏ từ `257cbef`

### Bộ quét không sai. Thứ thiếu là không gì ở máy dev chạy nó

| Nơi | Chạy gì |
| --- | --- |
| GitLab, job `pii_scan` (`allow_failure: false`) | `scan-pii.sh docs/evidence ci-artifacts` — **quét cây** |
| Gate sweep cục bộ | `selftest-pii.sh` — **quét fixture** để chứng minh bộ quét từ chối đúng |

Manifest ghi `scan-pii.sh` là `sweepable: false`, lý do: *"selftest-pii.sh is the gate that proves it
rejects"*. Selftest chứng minh **bộ quét** đúng; nó không chứng minh **cây** sạch. Hai câu nghe giống nhau,
và khoảng cách giữa chúng là `41` dòng đỏ.

Quét lại theo từng commit: `257cbef^` **`0`** · `257cbef` **`1`** · `fe3bb19` **`41`** · `628676d` **`41`**.

### Sửa chữ, không nới mẫu

`41` dòng, `45` chỗ, `13` README `W-0297`…`W-0311`. **Không có số điện thoại hay token nào** — toàn bộ là
từ tiếng Việt thông dụng trùng với mẫu địa chỉ.

| Mẫu bị khớp | Dòng | Nghĩa của từ trong văn bản | Thay bằng |
| --- | --- | --- | --- |
| Tên phố (mẫu `2`) | `39` | *path* của tệp · *luồng* quay số, gọi, intake, e2e · *lối*, *cách*, *hướng* | `path` · `luồng` · `lối` · `cách` · `hướng` · `hàm` |
| Đơn vị hành chính (mẫu `5`) | `2` | *organization* | `[công ty]` |

Mỗi chỗ thay được kiểm là xuất hiện **đúng một lần** trên đúng dòng đó trước khi thay; ký tự xuống dòng giữ
nguyên (bản `W-0297` trong cây làm việc là CRLF). Diff: `+41 −41`, không dòng nào khác đổi.

### Trích dẫn nguyên văn: đổi chữ thì phải đánh dấu

`4` chỗ nằm **trong trích dẫn**: `2` ở `W-0309` (văn bản `OD-V1-11` và lập luận bác `A1`), `2` ở
`W-0304`/`W-0306` (tự trích một dòng cũ để đính chính). Chữ thay đặt trong ngoặc vuông — `[công ty]`,
`[path]` — quy ước biên tập cho *"chữ này không phải nguyên văn"*. Đổi lặng lẽ thì hồ sơ ghi một câu chưa
ai từng nói; ngoặc vuông giữ được cả hai: CI xanh **và** trích dẫn trung thực.

### Để không tái diễn: một gate, không phải một luật trong tài liệu

Kế hoạch ghi *"thêm `scan-pii.sh docs/evidence` vào phần Kiểm của mọi lô"*. Luật trong tài liệu phụ thuộc
người nhớ; gate trong sweep thì không — đúng bài học `W-0251`. Nên đổi manifest:

```json
"scan-pii.sh": { "argv": ["docs/evidence"], "expect": "PII_SCAN_PASS" },
```

Sweep nay chạy **`40`** gate, bỏ qua `21` (trước: `39` / `22`). Kiểm **hai chiều**:

| Cây | `gate-sweep.mjs --only scan-pii.sh` |
| --- | --- |
| Thêm một file thử trong `docs/evidence/W-0313/` có một từ bị bắt | `FAIL` — `PII_SCAN_FAIL` |
| Gỡ file thử | `GATE_SWEEP_PASS 1/1` |

Không đưa `ci-artifacts` vào gate cục bộ: ở máy dev thư mục đó chứa đồ thừa của các lần chạy local, không
phải thứ các job CI xuất ra. CI vẫn quét nó như cũ.

---

## 2. Mục `16` — `IR-06 §3.4.1` còn tả intake trước `W-0302`

Code (`TaskIntakeService.ContactRejectionReason`): expiry **sớm hơn** window end ⇒
`DIAL_TOKEN_EXPIRES_BEFORE_WINDOW`, **muộn hơn** ⇒ `DIAL_TOKEN_EXPIRES_AFTER_WINDOW`, cùng
`422 IVR_CONTACT_INVALID` — chiều muộn có từ `W-0302` (`draft.28`). Tài liệu cho Module 3 vẫn ghi chiều muộn
*"intake nhận, rồi hỏng ở persistence"*, trái với chính dòng tổng hợp `W-0302` ở cuối `IR-06`.

Sửa bảng và hai hệ quả; câu cũ giữ lại ở dạng lịch sử in nghiêng, vì nó là lý do bảng tồn tại. Thêm dòng đầu
mục: **chỉ áp dụng khi task mang token** (§3.4.0).

---

## 3. Mục `17` — `S3` là giữ toàn bộ dữ liệu

| Tài liệu | Ghi | Sửa |
| --- | --- | --- |
| `IR-07` `A-9` | Metadata cuộc gọi giữ `90` ngày | Mục **Đính chính bổ sung `2026-09-17`** riêng, đặt **sau** mục `draft.31` đã gửi — gửi riêng được |
| `IR-06`, dòng tổng hợp `OD-V1-11` | *"metadata 90 ngày"* | Sửa cùng lượt, để chỉ phải ghim lại `IR-06` một lần |

⚠️ **Chưa làm — Lô 3 mục `1`:** register `OD-V1-11`, `docs/compliance/retention-period-proposal.md`, `W-0273`.
Từ lô này, **tài liệu gửi Module 3 đi trước register nội bộ**. Ghi ra để không ai đọc register rồi tưởng
kỳ hạn `90` ngày còn hiệu lực.

---

## 4. Mục `18` — `m8-09` vẫn *"chờ owner quyết A hay B"*

Owner đã chọn **B** ngày `09/09` (`W-0248`), và `W-0249` đã dựng hai fence thu hồi — lúc claim và ở lần đọc
task cuối trước khi quay số. Dòng *Chờ* trong `plan/ivr-orther/00-CHUA-XONG.md` sửa thành: phía M8 đã quyết,
fence **trơ** tới khi có endpoint thu hồi, còn chờ Module 3 (`M3-14`, `D-06`, phần của M3 trong
`RVK-01..RVK-12`). Các nhãn trạng thái ngày `03/09` giữ nguyên, kèm ghi chú rằng chúng đã cũ.

---

## 5. Kiểm chứng

| Hạng mục | Kết quả |
| --- | --- |
| `scan-pii.sh docs/evidence` | **`PII_SCAN_PASS`**, `400` file (trước lô: `41` dòng FAIL) |
| Gate sweep (Git Bash) | **`40/40`** chạy, `21` bỏ qua theo manifest — gồm gate mới `scan-pii.sh` (`PII_SCAN_PASS`) |
| Re-pin `IR-06` | `fa082134…` → `4b6f9e77…`, `7` nơi, tính trên byte LF; `4` validator `--self-test` PASS; `CONTRACT_FREEZE=PASS` |
| Tài liệu | `API_DOCS_SELFTEST_PASS` · `CI_CONFIG_SELFTEST_PASS` |
| .NET | `0` file `.cs`; không test nào đọc các file đã sửa ⇒ không chạy lại solution |
| `gitnexus_detect_changes` | risk `low`, `0` luồng bị ảnh hưởng; `28` mục đều là heading tài liệu và hằng `SOURCE_PINS` của `4` validator |

---

## 6. Thấy trong lúc làm

| Phát hiện | Xử lý |
| --- | --- |
| `specs/api/06-error-codes.md` có `DIAL_TOKEN_EXPIRES_BEFORE_WINDOW` nhưng **thiếu** `DIAL_TOKEN_EXPIRES_AFTER_WINDOW` của `W-0302` — cả ở `HEAD` | Toàn duyệt ngày `17/09` ⇒ **đã sửa**, §7 |

---

## 7. Follow-up `17/09` — mục `19`: danh mục lý do từ chối thiếu một mã

Toàn duyệt sau khi push `ffba799` (cả `origin` và `github` ở `ffba799`).

**Trạng thái "bẩn" của file trước khi sửa:** worktree, index và `HEAD` đều là LF `115` dòng,
`git diff --ignore-cr-at-eol` rỗng; sau khi sửa, `git diff HEAD` chỉ gồm các hunk của lượt này
(`+6 −4`) — không có thay đổi nào của ai khác bị cuốn theo.

**Phạm vi bảng §2a là nhóm Policy/Contact của `W-0129`**, không phải mọi lý do intake. So với code:
mọi lý do `TaskIntakeService.ContactRejectionReason` phát ra đều có trong bảng **trừ đúng một**,
`DIAL_TOKEN_EXPIRES_AFTER_WINDOW` (`W-0302`). Lý do đơn đêm của `W-0298` và các nhóm script, attempt
policy, cửa sổ chưa bao giờ thuộc bảng này — không coi là thiếu, không thêm.

**Tìm chỗ đếm trước khi thêm** (bài học `W-0312`):

| Chỗ đếm | Trước | Sau khi thêm dòng |
| --- | --- | --- |
| `06-error-codes.md`, đoạn cutover `draft.24`: *"sáu rule contact còn lại"* | Đúng ở `draft.24`, sai từ `draft.28` | Giữ nguyên câu lịch sử, thêm *"(bảy từ `draft.28`…)"* |
| `06-error-codes.md`: *"M3 chưa nhìn thấy **chín** mã chi tiết"* | Đếm dòng bảng — `9` | **`10`** |
| Comment `TaskIntakeApiTests.cs:62`: *"lists six more triggers that reach it"* | Bảng có `5` trigger `422` ngoài ca số chưa che — comment **lệch một** | Nay đúng `6` — không cần sửa |

Dòng mới: decision `TASK_REJECTED_CONTACT_INVALID`, wire `422 IVR_CONTACT_INVALID` — xác nhận ở
`TaskIntakeService.cs:169-177`; đã có test phủ lý do này (`TaskIntakeServiceTests.cs:122`,
`TaskIntakePersistenceTests.cs:239`).

### Phần sửa đã vào `5d5f96b save` trước khi kiểm xong

Toàn bộ phần sửa trên — bảng, hai con số đếm, §7 tới đoạn *Dòng mới*, dòng plan, dòng tracker, `gate-status.yaml` — nằm
trong commit `5d5f96b save` (`15:57`, đã push lên `origin` và `github`) cùng `AGENTS.md`, `CLAUDE.md` và một
file `.docx`, lúc lượt kiểm còn đang chạy. Commit đó mang đủ nội dung mục `19`; chỉ ô bằng chứng của `A-0640`
khi ấy còn là placeholder. Commit follow-up điền kết quả thật; không sửa lịch sử.

### Kiểm chứng

| Hạng mục | Kết quả |
| --- | --- |
| Gate sweep (Git Bash) | **`39/40`** chạy, `21` bỏ qua — gate duy nhất hỏng: `dr-selftest.mjs`, hết giờ (dưới) |
| `gate-sweep.mjs --only dr-selftest.mjs` | **PASS `144.9s`** — `GATE_SWEEP_PASS 1/1` |
| ⇒ | Mọi gate PASS trên nội dung này, **qua hai lượt** — không phải `40/40` trong một lượt |
| Sau khi điền kết quả vào tracker, README, plan | `GATE_STATUS_PASS` · `PII_SCAN_PASS` · `CI_CONFIG_SELFTEST_PASS` · `API_DOCS_SELFTEST_PASS` |
| .NET | `0` file `.cs`; không test nào đọc các file đã sửa ⇒ không chạy lại solution |
| `gitnexus_detect_changes` | risk `low`, `0` luồng |

### `dr-selftest.mjs` chạm trần `180s` — nguyên nhân chưa rõ

Gate dựng PostgreSQL thật trong Docker và chạy bốn drill; nó chỉ đọc `deploy/helm/ivr`, `deploy/backup`,
`deploy/dr` — không file nào lượt này sửa.

| Lần | Cách chạy | Kết quả |
| --- | --- | --- |
| `1`–`3` | sweep đầy đủ, rồi `--only` hai lần | hết giờ ở `180.0s` |
| `4` · `15:56` | `node dr-selftest.mjs --self-test`, không qua sweep | PASS `143s` |
| `5` · `16:02` | đúng tuỳ chọn `spawnSync` của sweep, trần `600s` | PASS **`193.5s`** |
| `6` · `16:13` | `gate-sweep.mjs --only dr-selftest.mjs` | PASS `144.9s` |

Lần `5` cho thấy khi chậm, gate **vượt trần thật chứ không treo**. Tải máy không đủ giải thích: lần `6` PASS
dù lúc bắt đầu và lúc kết thúc đều có hai `testhost.exe` của repo `ginsengfood-ops-core` đang chạy. Các log
sweep ghi gate này ở `130`–`176s`; `W-0279` đã gặp đúng lỗi này một lần (*"sát trần; lượt sau xanh"*). Trần
mặc định gần như không còn khoảng dư.

**Không đổi gì.** Nâng trần của gate là việc Toàn quyết — plan mục `20`.

### Lỗi đếm của chính `W-0313`

`W-0313` thêm gate `scan-pii.sh` mà không đếm lại — đúng loại lỗi `W-0312` vừa ghi: plan còn ghi sweep
`39/39` ở phần Kiểm của Lô 3, Lô 5 và ở mục `14`. Mục `14` còn tả `13` file bẩn — cũ từ `5d5f96b`, sau commit
đó `git status` sạch. Sửa cả hai loại; câu Kiểm của Lô 1 giữ nguyên vì đúng lúc thi hành.

**Còn lại của Lô 3:** mục `1`–`14`, và mục `20` chờ Toàn.
