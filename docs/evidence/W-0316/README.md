# W-0316 — Lô 3 phần hai: sổ sách một lượt

**Ngày:** `2026-09-18` · **Baseline:** `main@9af20d3` · **Loại:** tài liệu và một cấu hình gate · **`0` file `.cs`**

`REAL_CUSTOMER_CALL_ALLOWED=NO`

---

## 0. Tóm tắt

Toàn: *"tiếp cho xong đi"*. Lô này làm `13` mục còn lại của Lô 3 trong
[bản vướng mắc `17/09`](../../../plan/ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md): mục `1`, `3`–`12`,
`14`, `20`. Mục `2` đã xong ở `W-0315`; mục `15`–`19` xong ở `W-0313`.

- **Mục `13` không làm.** Nó chỉ được làm sau khi Sếp ký `S2`.
- **Mục `20` là đề xuất đang chờ duyệt**, và được làm theo đề xuất: Toàn bảo làm cho xong, không hỏi lại.

Không file mã nào bị sửa, nên không có symbol nào để chạy `gitnexus_impact`. Thay đổi duy nhất ngoài tài
liệu là một trường `timeoutMs` trong manifest gate.

**Kết quả khác kế hoạch ở năm chỗ**, ghi ở §2: mục `5`, `7`, `9`, `12`, `14`.

---

## 1. Từng mục

| # | Việc | Sửa ở đâu |
| --- | --- | --- |
| 1 | `S3` — bỏ ba kỳ hạn lưu (`90` ngày · `180` ngày · `1` năm), giữ vĩnh viễn | register `OD-V1-11` (thêm *"Sửa 17/09"*, giữ nguyên văn) · `retention-period-proposal.md` (đầu phiếu, §1, §5) · tracker `W-0273` `BLOCKED_EXTERNAL` → `EVIDENCE_SUBMITTED` |
| 3 | `S7` — registry ngoài cho sổ cái dung lượng là giới hạn được chấp nhận của bản đầu | register, dòng mới **`OD-V1-24`** (`CLOSED` `2026-09-17`). **Không** sửa `m8-15` — bị ghim hash |
| 4 | `T4` — vế *"không ghi DB"* của `OD-V1-18` bị phương án B thay; ba vế *"không vào log, evidence, callback"* giữ nguyên | register `OD-V1-18` |
| 5 | `T6` — điều kiện của stage `5` | README `W-0311` §6 |
| 6 | `T8` — `W-0163` → `CANCELLED` · `W-0118` → `N/A` *(HISTORICAL, theo tiền lệ `W-0105`)* | tracker §5 |
| 7 | `IR-07` đã gửi | dòng trạng thái `IR-07` → `SENT / AWAITING_REPLY` · `00-CHUA-XONG.md` · README `W-0310` · `integration-requirements/00-index.md` |
| 8 | Phiếu cho Sếp đã trả lời mục `1`–`4`, trỏ về bản vướng mắc | `phieu-quyet-dinh-cho-sep-2026-09-17.md` (trạng thái + bảng bốn mục) · `00-CHUA-XONG.md` |
| 9 | Nhãn `W-0310` sai không chỉ ở tiêu đề `ca8d13b` | README `W-0311` (bảng đếm lại) · tracker §2 |
| 10 | `A-0637` bị cấp hai lần | tracker §7, ghi tại chỗ, **không** đánh số lại |
| 11 | Bỏ *"nguồn khoá token"* khỏi danh sách chờ Sếp · `m8-11` *"chưa có bộ số"* → `OD-V1-16` ✅ `05/09` | `00-CHUA-XONG.md` (bảng nhóm A, mục `m8-11`, bảng *"Việc gì đang chặn"*) |
| 12 | `W-0007` · `W-0121` · `W-0171` (c) | tracker §4, §5 |
| 14 | Bàn giao `17/09`: file bẩn và số gate đã cũ | `plan/BAN-GIAO-phien-moi-2026-09-17.md` |
| 20 | Trần thời gian của `dr-selftest` | `deploy/ci/gate-invocations.json`: thêm `"timeoutMs": 300000`, như `gate-status.mjs`, `validate-openapi.mjs`, `contract-freeze-selftest.mjs` đã có |

Mọi chỗ sửa trong tài liệu cũ đều ghi *"Sửa 18/09 (W-0316)"* và giữ nguyên văn cũ bên cạnh. Không xoá
lịch sử nào.

---

## 2. Năm chỗ kết quả khác kế hoạch

### Mục `9` — con số trong kế hoạch không tái tạo được

Kế hoạch ghi *"`ca8d13b` nhầm `W-0310` ở 17 dòng / 15 file"*. Đếm lại tại `53c2eb5` bằng `git grep`, bỏ
các dòng nói về `W-0310` thật và về chính chỗ nhầm này, còn lại **`18` dòng / `16` file**. Nhãn sai do
**ba** commit ghi:

| Commit | Dòng nhãn sai còn lại |
| --- | ---: |
| `ca8d13b` — stage `1`, tiêu đề sai | `7` |
| `7c4204e` — stage `2`, tiêu đề **đúng** `W-0311` | `8` *(thêm `5` dòng nữa đã bị `W-0312` viết lại)* |
| `53c2eb5` — `W-0314`, **chính phiên này** chép lại nhãn từ danh mục | `3` |

Ba dòng cuối là lỗi của tôi hôm nay: ghi chú `⚠️` trong README `W-0311` có sẵn từ `17/09`, và tôi vẫn
chép nhãn cũ vào `RetentionTargetCatalog.cs`, `ComplianceTests.cs` và `retention-period-proposal.md`.

**Không sửa dòng nào.** `4` dòng nằm trong migration đã áp (luật `4` của kế hoạch `16/09`), `1` dòng ở
baseline `draft.30` đông cứng. Sửa `13` dòng còn lại là đúng kiểu hoà giải lặng lẽ mà ghi chú gốc muốn
tránh. Bảng đầy đủ nằm ở README `W-0311`.

### Mục `12` — `W-0121` giữ `CODE_DONE`

Hosted CI **đã chạy** ở `W-0292`: push tới GitLab, runner nhận job. Nhưng pipeline `2846110576` tại
`179a5eb` ra `31` PASS · `1` Failed · `2` Skipped · `2` Canceled, và evidence `W-0292` tự ghi *"Pipeline
không phải PASS"*. Chính dòng `W-0121` đặt điều kiện đổi trạng thái là **một pipeline xanh**. Vì vậy chỉ
ghi sự thật mới, không nâng trạng thái.

Cùng lượt: `W-0171` (a) — `perm IVR_DEV_TOOLING` trong OAS — đã được `W-0312` xử lý (`T5`), dù kế hoạch
chỉ nêu (c). Ghi cả hai.

`W-0007` giữ trạng thái §4: xung đột bộ số đã giải `05/09`, còn đóng dòng §4 hay không là việc của owner
lúc nghiệm thu.

### Mục `14` — `5d5f96b` gom `4` file, không phải `7`

Bàn giao `17/09` liệt kê `7` file bẩn. `5d5f96b` (`save`) chứa `4` trong số đó: `AGENTS.md`, `CLAUDE.md`,
`06-error-codes.md` và file `.docx`. `3` file kia không nằm trong commit nào sau `17/09` và nay không còn
khác HEAD. Điều đó khớp với ghi chú *"chỉ lệch CRLF/LF"* của kế hoạch.

### Mục `7` — thêm một chỗ kế hoạch không nêu

`integration-requirements/00-index.md` vẫn ghi `IR-07` là *"21 mục, gửi `2026-09-10`"*. Phiếu nay có
`30` câu (bản gộp `W-0310`) và Toàn xác nhận đã gửi ngày `17/09`. Đã sửa; câu cũ ghi lại trong dòng.

Chỉ sửa **dòng trạng thái** của `IR-07`. Thân phiếu thì không, theo luật `14` của kế hoạch: phiếu đã gửi,
sửa bằng mục đính chính có ngày ở cuối.

### Mục `5` — lối miễn hiện có chỉ dành cho lịch sử

Gate expand có đúng một lối miễn: `deploy/ci/migration-expand-baseline.json`. Nó chỉ nhận migration có id
**trước** mốc `20260827024438_W0118AttemptCountedInvariant`, ghim bằng SHA-256 (`2` migration), kèm câu
*"No drop-table exception is permitted in the current expand phase"*. Nên điều kiện `T6` không phải
*"đi qua lối có sẵn"*: lối cho bản contract **chưa tồn tại**. README `W-0311` §6 ghi điều kiện tối thiểu,
và những lối tắt không được đi.

---

## 3. Phối hợp với phiên `ivr-a4`

Lúc lô này bắt đầu, phiên `ivr-a4` có `117` file chưa commit (`W-0315`), gồm gần hết các file Lô 3 cần sửa.
Hai phiên thống nhất qua `SendMessage`:

- Tới khi `W-0315` vào, lô này chỉ sửa ba file ngoài tập file của `ivr-a4`: README `W-0310`, README `W-0311`,
  `retention-period-proposal.md`.
- Phần còn lại sửa **sau** `9af20d3`, đọc lại nội dung mới trước khi sửa.
- `W-0309` giữ nguyên thư mục: chỉ phần §3 dính tới hướng giọng đọc cũ; phần còn lại là bằng chứng duy
  nhất của việc gom phiếu về Sếp.

---

## 4. Kiểm chứng

| Hạng mục | Kết quả |
| --- | --- |
| Ghim hash | Trước khi sửa, lấy SHA-256 bản HEAD của **cả `14` file** sẽ chạm rồi `git grep` trong repo: **`0`** chỗ ghim. Không phải ghim lại gì |
| `gate-status.mjs --write` rồi kiểm | `GATE_STATUS_PASS` — `11` gate, `304` work item, `6` OD mở. `blocked_external_count` `21` → `19` (`W-0163` huỷ, `W-0273` hết chờ bên ngoài) |
| Quét PII `docs/evidence` (lệnh CI chạy) | `PII_SCAN_PASS files=388` |
| Gate sweep (Git Bash) | **`GATE_SWEEP_PASS 40/40`** — `dr-selftest` `146.7s`, dưới trần mới `300s` |
| Traceability | `TEST_TRACEABILITY_CURRENT=722` — không đổi, vì lô này không chạm test |
| Gitleaks trên các dòng thêm vào | `no leaks found` — `zricethezav/gitleaks:v8.30.0`, `.gitleaks.toml` của repo, chạy trên một cây nháp chỉ chứa các dòng thêm vào (`~79 KB`) |
| `detect_changes` | risk `low`, `0` luồng bị ảnh hưởng, `14` file — mọi thay đổi là section tài liệu |

Không chạy `dotnet test`: lô này không chạm mã, không chạm test.

---

## 5. Còn lại

- **Lô 3 mục `13`** — ghi tên người nhận rủi ro phương án B. Chờ Sếp ký `S2`.
- **Lô 4 bước `1`, `2`, `4`** — quét lại Trivy, thử đổi bản cài nền, soạn cấu hình production nháp.
  `ivr-a4` không nhận; lô kế tiếp của phiên này.
- **Lô 5** — nghiệm thu theo đợt.

## Khai báo phép kiểm C2 — W-0349, 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Hồ sơ này trước không khai test hay phép kiểm nào, nên C2 không xét được. [Khai báo](acceptance-tests.json) nay ghi:

- Việc thuần tài liệu: Chỉ ghi các quyết định 17–18/09 vào register, tracker, kế hoạch và phiếu; thay đổi ngoài tài liệu duy nhất là trường timeoutMs của dr-selftest trong manifest gate, không đổi mã, test hay script gate. Danh sách 11 tài liệu nằm trong khai báo.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0316 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. S2 đã ký ở
W-0340; Lô 4 và Lô 5 làm ở W-0317 và W-0318. Claude chọn việc theo tiêu chí phiếu W-0348, không tự
cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.
