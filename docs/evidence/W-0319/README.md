# W-0319 — Bằng chứng nghiệm thu phải thuộc đúng commit và chạy đủ

Ngày: `2026-09-21` · Baseline: `e33927d03257492de93727fbdcab4b33e6f3f25e`

`REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Phạm vi và lỗi đã tái hiện

Owner yêu cầu sửa W-0318: kiểm đủ gate trong manifest, ràng test/sweep vào commit đang xét,
từ chối bằng chứng cũ hoặc chạy thiếu, thêm kiểm thử hồi quy. Không đổi trạng thái nghiệm thu.

Trước sửa, assertion yêu cầu từ chối log `GATE_SWEEP_PASS 1/1 run` đã đỏ: hàm trả `ok: true`
trong khi manifest dựng cho kiểm thử có hai gate. Mã cũ cũng chỉ cảnh báo tuổi file TRX;
không kiểm SHA nguồn, hash kết quả hay tập tên gate đã chạy.

## 2. Thay đổi

- `acceptance-batches.mjs` chốt SHA đầy đủ một lần; mọi lần đọc Git dùng SHA đó, không đọc HEAD động.
- Đầu vào `--evidence` thay `--trx`/`--sweep-log`. Hai tuỳ chọn cũ bị từ chối rõ lý do.
- `acceptance-evidence-lib.mjs` kiểm SHA/tree/cây sạch trước và sau cả hai lệnh; lệnh đầy đủ;
  thời gian test trước sweep; hash manifest gate và hash từng TRX/log sweep.
- Kiểm đúng tập gate, từng tên đúng một lần, đúng pass token, số gate chạy và số skip theo manifest.
  `0/0`, `--only`, dòng kết luận đơn độc, gate thiếu/trùng/lạ, nhiều lượt trong một log đều bị từ chối.
- Suy ra các project test từ solution/csproj tại SHA đó. Mỗi project phải có TRX, counter khớp kết quả,
  mọi kết quả Passed; không nhận test bị bỏ qua. Thời gian bên trong TRX phải nằm trong lượt test.
- `collect-acceptance-evidence.mjs` chạy `dotnet test Ivr.sln` rồi full sweep, tạo thư mục mới;
  chỉ ghi `acceptance-run.json` khi mọi kiểm tra đạt. Không có chế độ gắn SHA vào kết quả có sẵn.
- `--worktree` chỉ phục vụ đọc nháp, không được dùng với gói nghiệm thu.

## 3. Cách chạy

Chạy từ checkout sạch của commit cần xét. Trên Windows dùng Git Bash để sweep tìm được `sh`.
Giữ riêng các thay đổi của phiên khác; công cụ từ chối cây có file chưa commit.

```sh
node tools/dev/collect-acceptance-evidence.mjs --out .artifacts/acceptance-run-001
node deploy/ci/scripts/acceptance-batches.mjs --evidence .artifacts/acceptance-run-001/acceptance-run.json --phase P2
```

Mỗi lượt dùng một thư mục mới. Khi commit đổi, thu gói mới. Copy/touch file không đổi commit mà nó chứng minh.
Không truyền `--evidence` thì các điều kiện về lượt chạy là CHƯA KIỂM; không thể ĐẠT/XEM chỉ nhờ README.
Gói sai làm CLI trả exit code `1`, và danh sách ghi KHÔNG ĐẠT.

## 4. Kiểm chứng

| Kiểm | Kết quả |
| --- | --- |
| Regression trên mã cũ | Đỏ đúng lỗi partial sweep |
| `acceptance-batches.mjs --self-test` | PASS, giữ các ca kiểm bốn tiêu chí cũ |
| `acceptance-evidence-selftest.mjs` | PASS `42` kiểm tra; có fixture Git và chạy CLI thật |
| Hai gate qua `gate-sweep --only` | PASS riêng từng gate; đây không phải full sweep |
| `ci-config-selftest`, docs selftest, traceability | PASS; traceability giữ `722` |
| Đọc TRX thật đã lưu | Đọc đúng `8` kết quả của integration; chỉ kiểm tương thích parser, không dùng nghiệm thu |
| PII hai gói W-0318/W-0319; gate-status | PASS; mirror `308` việc gồm WIP W-0320 của phiên khác |
| GitNexus detect-changes | LOW, `0` process; đọc cả diff chung, chưa bao gồm file mới chưa track |
| Bằng chứng trong fixture | Dữ liệu tổng hợp, không phải lượt chạy solution IVR |
| GitNexus impact | LOW; hàm trong công cụ/self-test, không có runtime process bị ảnh hưởng |

Các ca gồm sai SHA ở từng lượt, thiếu SHA, đổi tree, cây bẩn, lệnh có filter/only, exit khác 0,
thiếu project, TRX cũ/trùng/sai assembly, result bị bỏ qua, hash lệch, path thoát khỏi bundle,
và collector thất bại không để lại manifest đạt. Fixture hợp lệ được CLI nhận; sau commit mới,
gói cũ bị từ chối ngay cả khi thời gian file được đổi mới.

## 5. Giới hạn

Hash và metadata giúp phát hiện trộn hoặc sửa kết quả; đây không phải chữ ký chống một người cố ý
làm giả cả manifest và kết quả. Owner vẫn phải đọc bằng chứng và duyệt.

Chưa tạo gói nghiệm thu mới cho toàn bộ IVR trong lô sửa công cụ này. Không nâng trạng thái các việc
P2/P3, không sửa 55 gói cũ, không thay danh sách lịch sử tại `3d0111a`. Lượt thu gói thật tiếp theo
phải chạy sau khi bản sửa nằm trong commit cần xét, trên checkout sạch.
