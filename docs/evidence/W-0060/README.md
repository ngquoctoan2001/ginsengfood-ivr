# W-0060 — Evidence: Production readiness command center (`P11-4`)

Ngày: `2026-08-19` · Trạng thái: **`EVIDENCE_SUBMITTED`**
· **Đang ở nấc 0/4** — không nấc ladder nào được tuyên bố đạt

## 1. Điều phải nói trước

Xong hết prompt **không phải** sẵn sàng go-live. Đó là toàn bộ lý do prompt này tồn tại, và board
nói thẳng: **nấc 1 trong bốn nấc còn chưa đạt**, vì nó đòi mọi prompt có evidence **`ACCEPTED`** —
và hôm nay 4/102 work item ở trạng thái đó.

`MASTER-05`: **evidence đã nộp không phải evidence đã được chấp nhận**. Chỉ Release owner chuyển.

## 2. Gương, không phải backlog thứ hai

`P11-4` §3 cấm tạo tracker thứ hai. Nhưng cách một tấm gương **biến thành** backlog thứ hai không
phải là một quyết định — nó là **một tháng tracker đi tiếp còn board thì không**.

Nên board và `gate-status.yaml` được **sinh** từ tracker, và CI chạy generator ở chế độ đối chiếu:
sửa tay board là **pipeline đỏ**, không phải một sự thật thứ hai. Điều đó làm việc trôi lệch trở nên
**bất khả**, chứ không phải bị khuyến cáo.

## 3. Cổng của chính tôi bắt hai lỗi trong luật của chính tôi

**(1) Luật "không phần trăm" đỏ trên chính đoạn giải thích vì sao cấm phần trăm.**

Luật `P11-4` §3 nói về readiness **biểu diễn bằng phần trăm** — nó không nói về chữ `%` xuất hiện
trong văn xuôi **phản đối** phần trăm. Bản đầu kiểm cả file và đỏ ngay trên câu "94% xong đọc như
gần xong". Siết lại thành: chỉ kiểm **giá trị** YAML (bỏ dòng comment) và **ô bảng** của board.

**(2) Luật "mọi item phải có evidence pack" gắn cờ 20 dòng remediation.**

`W-0067`..`W-0084` là fix/remediation không có prompt, nên không có DoD §10 nào đòi evidence pack;
evidence của chúng luôn nằm trong chính dòng tracker. Một luật mà cách thoả mãn là **tạo 20 thư mục
rỗng** là một luật ngược hẳn mục đích của nó.

Thu về **chỉ item có prompt** (`PX-Y`), và **đếm** số dòng còn lại vào YAML
(`rows_without_evidence_pack`) để chúng vẫn nhìn thấy được thay vì biến mất.

Và luật siết lại **vẫn còn răng**: nó đỏ ngay khi `W-0060` chuyển sang `EVIDENCE_SUBMITTED` mà chưa
có file này.

## 4. Một lỗi parser thật, lộ ra vì board nhóm theo trạng thái

§4 (external request register) và §5 (planned implementation register) của tracker **không cùng bố
cục cột**. Bản đầu quét cả file, nên cột 4 của §4 — vốn là **mô tả deliverable** — rơi vào ô status,
và board hiển thị nó **như một trạng thái**:

```
| `GH ONLINE + 24/7 COD matrix, callable states, required flag, task OpenAPI/tests` | 1 |
```

Chỉ lộ ra vì board **nhóm theo trạng thái** và ba câu văn xuất hiện ở chỗ đáng lẽ là một từ. Nếu
board chỉ liệt kê từng dòng thì con số sai vẫn nằm im.

Sửa: cắt từng section rồi đọc với **bản đồ cột riêng**, kèm một cổng đòi mọi status thuộc **từ vựng
đóng** ở tracker §1. Một board dựng trên status đọc sai **còn tệ hơn không có board**.

Sau sửa: **102** work item (không phải 103), **15** `BLOCKED_EXTERNAL` (10 ngoài + 5 planned).

## 5. Ba điều board từ chối làm

| Điều | Vì sao |
| --- | --- |
| **không phần trăm** ở bất kỳ giá trị nào | "94% xong" mời người đọc hiểu là gần xong, trong khi 6% còn lại là **toàn bộ** những cổng không ai đóng được |
| **không trạng thái riêng** | mỗi dòng mang đúng Work ID và trạng thái của tracker; không có phép tính nào ở đây |
| **không tuyên bố nấc nào** | cả bốn nấc `attained: false`, kèm **điều kiện vào** để người đọc kiểm được thay vì phải tin |

Và một điều nữa: **file không tự bật cờ nào**. `real_customer_call_allowed: false` là một **báo
cáo**, kèm `mutable_by_this_file: false` ghi thẳng trong YAML.

## 6. Kiểm chứng

| Check | Kiểm âm | Kết quả |
| --- | --- | --- |
| drift board ↔ tracker | sửa tay board | ❌ đỏ |
| không nấc nào đạt | (đọc YAML) `attained: true` ở đâu đó | ❌ đỏ |
| không phần trăm trong giá trị | (chạy thật) đoạn giải thích của chính tôi | ❌ đỏ lượt đầu → §3 |
| evidence pack cho item có prompt | (chạy thật) `W-0060` vừa chuyển trạng thái | ❌ đỏ, nêu đúng `W-0060` |
| status thuộc từ vựng đóng | (chạy thật) mô tả deliverable của §4 | ❌ đỏ → §4 |

| Lệnh | Kết quả |
| --- | --- |
| `gate-status.mjs` | `GATE_STATUS_PASS` — 11 gate, 102 work item, 21 `OD-V1-*` |
| `docs-selftest.mjs` | `DOC_CI_TOPOLOGY_PASS` — `gate_status_mirror` root-included, `allow_failure: false` |

## 7. Cái này KHÔNG chứng minh

- **Không đọc nội dung evidence.** Cổng kiểm evidence **có tồn tại** và trạng thái được mirror
  đúng. Một file evidence rỗng vẫn qua.
- **`LADDER` và `GO_NO_GO` là danh sách viết tay trong script.** Chưa cổng nào khẳng định chúng
  khớp `P11-4` §2.5 — cùng lớp khoảng trống với `DATA_CLASSES` ở `W-0059`.
- **Không đóng cổng nào.** Board báo cáo; nó không chuyển trạng thái, không ký, không bật cờ.
- **Không kiểm được tracker có đúng không.** Nếu một dòng tracker khai sai trạng thái, board mirror
  đúng cái sai đó.
- **Nấc 1 chưa đạt**, nên mọi nấc trên đều chưa. Lối tới nấc 1 đi qua **reviewer chấp nhận
  evidence**, không qua thêm việc kỹ thuật nào.
