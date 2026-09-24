# W-0252 — Cái tên cũng phải nói đúng vai

Ngày: 2026-09-09 · Baseline: `main@0030087` · Trạng thái: **TESTS_PASS**.

`W-0251` tách bản ghi đóng băng khỏi pin sống theo **vị trí**. Lượt này tách nốt theo **tên**.

Tên cũ `artifact-sha256.txt` trùng khuôn với tệp pin, và **chính sự trùng ấy** là thứ để `W-0217`
sửa nhầm chỗ mà không ai thấy suốt ba mươi work item. Sau lượt này, vai đọc được ngay từ tên chứ
không phải suy ra từ chỗ tệp nằm.

## 1. Đổi gì

`git mv` **12** tệp `docs/evidence/*/artifact-sha256.txt` → `attested-sha256.txt`:
`W-0152`, `W-0170`, `W-0174`, `W-0178`, `W-0180`, `W-0181`, `W-0182`, `W-0183`, `W-0185`,
`W-0186`, `W-0187`, `W-0188`. Cộng **36** tham chiếu vị trí file trong **18** tệp.

Rename an toàn vì **đổi tên không đổi hash nội dung**: mọi pin `*_sha256` lên các tệp ấy vẫn đúng
nguyên; chỉ chuỗi vị trí file phải đi theo. Kiểm trước khi làm: **không script nào** đọc bất kỳ tệp nào
trong mười hai — đúng như `W-0251` để lại.

## 2. Luật áp dụng, viết ra để lần sau khỏi cãi

> Cập nhật **con trỏ** — vị trí file dùng để tìm tệp. Không đụng **chứng thực** — dòng hash.

Ba hệ quả của luật đó, cả ba đều kiểm được:

| Trường hợp | Làm gì | Vì sao |
| --- | --- | --- |
| 10/12 tệp | không đổi một byte | chúng không nhắc tên tệp nào cả |
| `W-0186`, `W-0188` | đổi **cột vị trí file**, giữ nguyên **cột hash** | chúng tự liệt kê manifest khác; nội dung tệp được trỏ tới không đổi, chỉ chỗ nó nằm |
| Tường thuật trong `W-0250`/`W-0251` | **giữ nguyên tên cũ** | câu *"bốn script gate đọc `artifact-sha256.txt` như một danh sách pin sống"* chỉ đúng với tên cũ; viết lại thành tên mới sẽ thành **sai**, vì `attested-sha256.txt` chưa bao giờ là pin sống — đó là cả lý do đổi tên |

Chỗ cuối thay bằng **con trỏ tiến**: `W-0251` được thêm một dòng nói tên tệp trong trang đó là tên
tại thời điểm ấy, và `W-0252` đã đổi sang gì.

## 3. Một sót của `W-0251` lộ ra ở đây

`m8-12` và `m8-13` vẫn ghi *"manifest máy đọc hiện hành nằm tại `docs/evidence/W-0170/…`"*. Câu đó
sai **cả vai** chứ không chỉ sai tên: `W-0251` đã dời pin sống sang `deploy/ci/pins/` mà **không**
sửa hai tài liệu ấy. Tôi chỉ thấy vì lượt này phải rà mọi chỗ nhắc tên tệp.

Nay cả hai nói rõ hai vai:

```text
Manifest máy đọc hiện hành      deploy/ci/pins/external-decision-artifacts.sha256
Bản ghi đóng băng ngày W-0170   docs/evidence/W-0170/attested-sha256.txt   (được phép cũ đi)
```

## 4. Dây chuyền pin, và lần dùng thật đầu tiên của sweep

Sửa hai tài liệu được ghim thì nổ dây chuyền y như lượt trước, ba tầng:

```text
m8-12 + m8-13 đổi   -> lệch pin trong 3 validator + 3 template
header tệp pin đổi  -> lệch chính hash tệp pin ở 6 nơi
2 validator đổi     -> lệch pin-của-pin trong closure validator + template
```

`gate-sweep.mjs` của `W-0251` **bắt được toàn bộ dây chuyền ngay lượt chạy đầu**, mỗi tầng một thông
báo nêu đúng tệp lệch. Đây là lần nó được dùng thật đầu tiên, và nó làm đúng việc nó sinh ra để làm:
không phải "có gì đó hỏng" mà là "tệp này lệch pin này".

## 5. Kiểm chứng

```text
gate-sweep.mjs                             GATE_SWEEP_PASS 39/39 run, 18 skipped (exit=0)
external-decision                          W0164 · W0165 · W0170 · W0179 · W0184 đều PASS
10/12 tệp đổi tên                          byte-identical với bản cũ tại HEAD
2/12 còn lại                               chỉ cột vị trí file đổi, cột hash nguyên vẹn
tham chiếu `artifact-sha256` còn lại        chỉ trong phần tường thuật, có chủ ý
dotnet test Ivr.sln                        952/952 PASS, 0 failed, 0 skipped
GATE_STATUS_PASS                           250 work items
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`. `TARGET_CONTRACT_V1` vẫn `DRAFT`.

## 6. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| — | endpoint revoke + OAS + IR-06 — `2.5`, đi cùng `7.1`/`2.2` ở lượt phát hành sau | tôi |
| — | `2.3` hỏi dev M3 xem có định làm registry `ProgramCode` không | tôi + dev M3 |
| — | `4d` điền 20 giá trị `public_product_name` trong `PACK-02 §32.2` | owner + Product Master |

## Khai báo phép kiểm C2 — W-0349, 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Hồ sơ này trước không khai test hay phép kiểm nào, nên C2 không xét được. [Khai báo](acceptance-tests.json) nay ghi:

- Gate trong full sweep có self-test kiểm thay đổi của việc này: `external-decision-closure-validator.mjs`, `external-decision-response-validator.mjs`, `external-decision-routing-validator.mjs`.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0252 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Residual chỉ
còn việc bên ngoài (M3, Sales, Platform, tier GitLab, pháp chế) hoặc giới hạn đã ghi. Claude chọn
việc theo tiêu chí phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.
