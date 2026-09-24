# W-0318 — Lô 5: nghiệm thu theo đợt, phần của agent

**Ngày:** `2026-09-18` · **Baseline:** `main@3d0111a` · **Loại:** tiêu chí, script chỉ đọc, danh sách đầu · **`0` file `.cs`**

`REAL_CUSTOMER_CALL_ALLOWED=NO`

> Bổ sung `2026-09-21` — [W-0319](../W-0319/README.md) sửa kiểm đủ gate và provenance của kết quả.
> Cách chạy hiện hành dùng `collect-acceptance-evidence.mjs` rồi `--evidence <acceptance-run.json>`.
> `--trx`/`--sweep-log` bên dưới là cách chạy lịch sử, nay bị từ chối vì không chứng minh đúng commit.

---

## 0. Tóm tắt

Lô 5 của [bản vướng mắc `17/09`](../../../plan/ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md) (`T7`) có bốn
bước. Hai bước đầu là của agent:

| Bước | Ai | Trạng thái |
| --- | --- | --- |
| `1` Ghi tiêu chí nghiệm thu vào tracker §1 | agent | ✅ |
| `2` Script chỉ đọc lập danh sách đề nghị theo đợt | agent | ✅ `deploy/ci/scripts/acceptance-batches.mjs`, cùng danh sách đầu |
| `3` Duyệt từng đợt, chuyển `ACCEPTED` | **Toàn** | chờ |
| `4` `gate-status.mjs --write` sau mỗi đợt | Toàn, hoặc agent khi Toàn bảo | chờ |

Danh sách đầu tại `3d0111a`: **`218` việc ứng viên — `0` ĐẠT, `138` XEM, `80` KHÔNG ĐẠT**. Nằm ở
`docs/release/acceptance-batches.md`, **không** nằm trong gói này: file đó chép nguyên văn cột Residual của
tracker, mà quét PII của CI chạy trên `docs/evidence`.

---

## 1. Tiêu chí

Ghi ở §1 của tracker, đúng như kế hoạch, trừ **một chỗ**. Điều `4` của kế hoạch ghi *"gate sweep `40/40`"*;
tracker ghi *"`GATE_SWEEP_PASS`, mọi gate trong manifest đều chạy"*. Lý do: chính lô này thêm gate thứ `41`.
Một tiêu chí có con số sẽ sai từ commit đầu tiên của nó.

| # | Điều | Script kiểm thế nào |
| --- | --- | --- |
| `C1` | README evidence có ở path mà `gate-status.yaml` ghi, và mang `REAL_CUSTOMER_CALL_ALLOWED=NO` | Đọc file tại HEAD. Gói cũ ghi *"Real customer calls: NO"* vẫn **không đạt**, nhưng lý do nói rõ là có câu tương đương, để việc sửa trông như sửa một dòng chứ không như thiếu bằng chứng |
| `C2` | Mọi `TestId` README nêu có trong `docs/traceability-tests.md` và xanh ở commit nghiệm thu | Tìm `TestId` theo tiền tố mà bảng traceability biết, mở rộng khoảng `X-08..12`. Tra kết quả trong `.trx` theo **method** trong định nghĩa test, không theo tên hiển thị |
| `C3` | Cột Residual không còn việc của IVR | Script **không** phán. Ô có chữ thì dòng thành `XEM` để Toàn đọc; ô chỉ còn `REAL_CUSTOMER_CALL_ALLOWED=NO` coi như trống |
| `C4` | Gate sweep `GATE_SWEEP_PASS`, mọi gate đều chạy, tại commit nghiệm thu | Đọc dòng kết luận của log. Log **cũ hơn** commit HEAD thì không tính |

Script đọc tracker, `gate-status.yaml`, bảng traceability và các README **từ HEAD** bằng một lượt
`git cat-file --batch`, nên file chưa commit của một phiên khác không đổi được kết luận. Cây làm việc bẩn thì
đầu danh sách có cảnh báo. Script **không bao giờ** ghi tracker; chỉ Toàn chuyển `ACCEPTED` (luật `6`).

---

## 2. Script và self-test

`node deploy/ci/scripts/acceptance-batches.mjs --trx <thư mục .trx> --sweep-log <file> [--phase P2] [--worktree]`

Self-test chạy trong gate sweep (`ACCEPTANCE_BATCHES_SELFTEST_PASS`), trên dữ liệu dựng sẵn, không phụ thuộc
repo:

| Ca | Phải ra |
| --- | --- |
| Đủ bốn điều | ĐẠT |
| Residual có chữ | XEM — không bao giờ ĐẠT |
| README thiếu dòng đánh dấu, có câu dạng cũ | KHÔNG ĐẠT, lý do nêu câu dạng cũ |
| `gate-status.yaml` ghi `evidence: null` | KHÔNG ĐẠT |
| `TestId` không có trong traceability | KHÔNG ĐẠT — kể cả khi chưa có `.trx` |
| Test `Failed` trong `.trx`, tên hiển thị bị `DisplayName` thay | KHÔNG ĐẠT — tra theo method |
| Khoảng `UT-X-01..03`, test `03` không có kết quả | KHÔNG ĐẠT, nêu cả `02` lẫn `03` |
| Không có `.trx` / không có log sweep | CHƯA KIỂM |
| Log sweep `FAIL` hoặc không chạy đủ gate | KHÔNG ĐẠT cho mọi dòng |
| Trạng thái ngoài bộ từ vựng đóng | Script dừng — bảng cột đã lệch |
| Dòng `ACCEPTED`, `CODE_DONE` | Không phải ứng viên |

Trước khi đưa vào repo, bộ đọc `.trx` chạy trên `.trx` thật của lượt test dưới đây: `4` file, `1151` kết quả,
lớp và method khớp đúng.

---

## 3. Danh sách đầu — tại `3d0111a`

Kết quả test và log sweep đều chạy **trên cây sạch tại `3d0111a`**, trước khi sửa file nào của lô này: test
trước, sweep sau, không bao giờ song song.

| Nguồn | Kết quả |
| --- | --- |
| `dotnet test Ivr.sln --logger trx` | **`1151/1151`** — `756` unit · `363` integration · `24` contract · `8` chaos · `0` failed · `0` skipped |
| Gate sweep | **`GATE_SWEEP_PASS 40/40`** |

| Đợt | Ứng viên | ĐẠT | XEM | KHÔNG ĐẠT |
| --- | ---: | ---: | ---: | ---: |
| `P0` | 3 | 0 | 0 | 3 |
| `P1` | 4 | 0 | 1 | 3 |
| `P2` | 9 | 0 | 9 | 0 |
| `P3` | 4 | 0 | 4 | 0 |
| `P4` | 4 | 0 | 1 | 3 |
| `P5` | 5 | 0 | 3 | 2 |
| `P6` | 3 | 0 | 0 | 3 |
| `P7` | 5 | 0 | 3 | 2 |
| `P10` | 4 | 0 | 0 | 4 |
| `UNPLANNED` | 177 | 0 | 117 | 60 |
| **Tổng** | **218** | **0** | **138** | **80** |

**`0` ĐẠT** là đúng, không phải lỗi: mọi dòng ứng viên đều còn chữ ở cột Residual, và việc đọc cột đó là của
Toàn. `XEM` là những dòng đã qua `C1`, `C2`, `C4` — đợt nào Toàn đọc xong thì duyệt được ngay.

**Vì sao `80` dòng không đạt** — mọi lý do là thật, không lý do nào đến từ việc đọc `.trx`:

| Lý do | Dòng |
| --- | ---: |
| README thiếu `REAL_CUSTOMER_CALL_ALLOWED=NO` | `55` — trong đó `5` ghi câu tương đương dạng cũ |
| Không có gói bằng chứng (bằng chứng nằm trong dòng tracker) | `23` |
| `TestId` được nêu nhưng không tồn tại | `2` dòng, `3` ID — `UT-SCHEMA-BACKCOMPAT-04` ở `W-0115` (khoảng `01..04`, chỉ có `3` test), và `2` test mà `W-0315` ghi là **đã gỡ** |
| `TestId` không thấy trong `.trx` | **`0`** |

Một dòng có thể hỏng nhiều điều, nên các số trên không cộng thành `80`.

**Giới hạn đã biết:**

- README nêu một test **đã gỡ** vẫn bị tính là nêu test không tồn tại — trường hợp của `W-0315`. Script không
  phân biệt được *"nêu làm bằng chứng"* với *"nêu là đã xoá"*.
- Một nhóm `TestId` mà cả tiền tố đã biến khỏi bảng traceability thì không bị bắt.

**Nấc 1:** prompt đã lên kế hoạch đã xong `6/54`, `BLOCKED_INTERNAL` `0`.

---

## 4. Kiểm chứng

| Hạng mục | Kết quả |
| --- | --- |
| Self-test | `ACCEPTANCE_BATCHES_SELFTEST_PASS` — `4` tiêu chí, `8` dòng ứng viên, `2` dòng không phải ứng viên |
| Gate sweep sau khi thêm gate (Git Bash) | **`GATE_SWEEP_PASS 41/41`** — gate mới `acceptance-batches.mjs` có trong đó; `dr-selftest` `126.7s` |
| `gate-status.mjs --write` rồi kiểm | `GATE_STATUS_PASS` — `11` gate, `306` work item, `6` OD mở |
| Quét PII `docs/evidence` | `PII_SCAN_PASS files=390` |
| Gitleaks trên các dòng thêm vào | `no leaks found` — `zricethezav/gitleaks:v8.30.0`, `.gitleaks.toml` của repo, trên một cây nháp chỉ chứa các dòng thêm vào |
| `detect_changes` | risk `low`, `0` luồng bị ảnh hưởng — không symbol .NET nào đổi |

Không chỗ nào trong repo ghi cứng số gate trong code hay test (đã `git grep`), nên gate thứ `41` không làm đỏ gì.

---

## 5. Còn lại

- **Toàn duyệt từng đợt** (bước `3`). Gợi ý thứ tự: bắt đầu từ `P2` và `P3` — mọi ứng viên ở đó là `XEM`.
- Sau mỗi đợt: `gate-status.mjs --write` (bước `4`), và chạy lại danh sách trên commit mới.
- `55` gói thiếu dòng đánh dấu: thêm dòng đó là việc máy móc, nhưng nó sửa gói bằng chứng cũ. Chỉ làm khi
  Toàn bảo.

## Khai báo phép kiểm C2 — W-0351, 24/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

[Khai báo](acceptance-tests.json) nay ghi:

- 4 ID chỉ được nhắc, không phải claim của việc này; lý do từng ID nằm trong khai báo.
- Gate trong full sweep có self-test kiểm thay đổi của việc này: `acceptance-batches.mjs`.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.

## Owner nghiệm thu — 24/09/2026

Toàn hỏi “29 việc này cần quyết gì không? ko thì clean đi”, sau chỉ thị “thì cái nào xong cho xong
luôn đi”. Theo [danh sách W-0351](../W-0351/README.md), W-0318 chuyển **TESTS_PASS → ACCEPTED** cho
phần đã làm. Gate acceptance-batches.mjs xanh; ID ví dụ chỉ được nhắc. Claude chọn theo tiêu chí
phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`a8a3e57`. REAL_CUSTOMER_CALL_ALLOWED=NO.
