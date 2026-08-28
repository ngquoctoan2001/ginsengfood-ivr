# W-0138 — `gate-status` đếm sai số open decision

Ngày: `2026-08-28`
Baseline: `main@99594b6`
Trạng thái: `TESTS_PASS`
Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Phát hiện thế nào

Lúc làm `W-0137` tôi thêm `OD-20` vào `decisions-log.md` rồi chạy `gate-status.mjs --write` — con số
`decisions=21` **không đổi**. Truy ra generator đọc một register khác
(`specs/_review/open-decisions-register.md`) và chỉ khớp một namespace.

`OD-19`/`OD-20` nằm đúng chỗ theo thông lệ (`decisions-log.md`, cùng nơi với `OD-15/17/18`) nên đó
không phải lỗi. Nhưng lần truy đó lộ ra lỗi thật.

## 2. Lỗi thật

`collect()` khớp `/^\| \`(OD-V1-\d{2})\`/` — chỉ một namespace. Register có:

| Nhóm | Số dòng |
| --- | --- |
| `OD-V1-01..21` | 21 |
| `OD-VOICE-01..05` | **5 — chưa bao giờ được đếm** |

Board báo `21` open decision trong khi register có `26`. Năm cái vô hình đều là quyết định về
**giọng production**: nguồn giọng, phân miền, template, tự host vs vendor, chốt ba giọng.

Nguyên nhân theo git: `gate-status.mjs` sửa lần cuối **24/08** (`773c957`), `OD-VOICE-01` thêm
**27/08** (`ce800e1`). Dòng được thêm ba ngày sau khi generator đứng yên, và **không có test nào**
kiểm parity giữa register và generator.

`gate-status.mjs` chạy thật trong CI (`quality-gate.gitlab-ci.yml:58`), nên đây là gate đang báo
thiếu chứ không phải script phụ.

## 3. Lỗi trong bản sửa đầu của tôi

Bản sửa đầu chỉ nới pattern thành `OD-[A-Z0-9-]+` → `decisions=26`. **Sai theo hướng ngược lại.**

Kiểm tiếp thì **3/26 dòng đã `✅ CLOSED`** (`OD-VOICE-02`, `-03`, `-05`). Một field tên
`open_decisions` mà chứa quyết định đã đóng thì cũng sai như bỏ sót. Tôi đã đổi under-report `21`
thành over-report `26`.

Số đúng là **`23`**: 26 dòng trừ 3 dòng đã đóng. `OD-VOICE-01` (`lab APPROVED`, production chưa) và
`OD-VOICE-04` (`OPEN`) vẫn tính là mở — đúng.

Bắt được trước khi commit.

## 4. Bản sửa cuối

`collect()` giờ:

1. đọc **mọi** dòng `OD-*` kèm cột `Current`, kèm assertion rằng dòng có đủ bốn cột — đổi cấu trúc
   bảng thì đỏ chứ không phân loại nhầm;
2. **assertion parity**: mọi id đứng đầu dòng trong register phải được pattern đọc được. Thêm một
   namespace mới mà quên sửa pattern là đỏ ngay, thay vì im lặng rơi khỏi board;
3. loại các dòng có `CLOSED` khỏi `open_decisions`.

## 5. Mutation proof

| Mutation | Kết quả |
| --- | --- |
| Thêm dòng prefix lạ (`DEC-XYZ-01`) | **đỏ** — *"register contains decision rows the id pattern does not match"* |
| Thu pattern về `OD-V1-\d{2}` như cũ | **đỏ** — cùng assertion, tức không thể regress im lặng |
| Bỏ bộ lọc `CLOSED` | `23 → 26` — over-report quay lại |
| Đánh dấu `OD-VOICE-04` thành `CLOSED` | `23 → 22` — bộ lọc đọc đúng cột `Current` |

Ghi trung thực: hai lượt mutation đầu tiên tôi chạy bị hỏng do escaping (`sed` không khớp,
`python -c` nuốt backslash) và script vẫn in kết quả xanh. Những lượt đó **không** được dùng làm
bằng chứng; bảng trên là các lượt đã xác nhận mutate thật.

## 6. Verification

| Gate | Kết quả |
| --- | --- |
| `gate-status.mjs` | `GATE_STATUS_PASS` — 11 gate, 136 work item, **23** open decision |
| Trước W-0138 | `21` (thiếu 5) |
| Bản sửa đầu | `26` (thừa 3) |
| Register sau khi chạy | không bị sửa (`git status` sạch) |
| Code production | `0 file` — chỉ script CI |

## 7. Residual

- W-0138 **không** đóng hay sửa nội dung quyết định nào. Nó chỉ làm 5 quyết định giọng production
  hiện ra trên board. Chúng vẫn `OPEN` và vẫn chặn release.
- `OD-19`/`OD-20` trong `decisions-log.md` vẫn nằm ngoài register nên vẫn không lên board. Đó là
  đúng thiết kế hai namespace (register = quyết định chặn Sales/external; decisions-log = nội bộ
  IVR), nhưng đáng để Owner xác nhận là cố ý.
