# W-0325 — Bổ sung chỉ dấu C1 và phân loại hồ sơ nghiệm thu còn thiếu

Ngày: 21/09/2026 · Codex · REAL_CUSTOMER_CALL_ALLOWED=NO.

Owner yêu cầu tiếp tục sau [W-0324](../W-0324/README.md). Baseline:
`4346f6afd5920c447ff95adfc8466513edfb283c`. Cả tám phiếu P2 và bốn đề nghị đóng P3 vẫn chờ Toàn;
yêu cầu tiếp tục công việc không được ghi thành chữ ký ACCEPTED/CANCELLED.

## Phần thực hiện

Thêm một mục có ngày vào **55 README** thiếu đúng marker. Mục mới dẫn giới hạn ủy quyền hiện hành
ở tracker §2: real customer calls NO. Nó không khẳng định lại cách chạy của quá khứ, không đổi
PASS/FAIL/NOT_RUN đã ghi và không chứng nhận một lượt test mới. Các gói thiếu cách diễn đạt cũ
rõ ràng vẫn dùng cùng nguồn policy hiện hành, không được suy lịch sử gọi điện từ sự im lặng.

[Nhật ký từng file](c1-additions.json) lưu hash trước/sau, byte count, phần thêm nguyên văn và
tham chiếu dòng/hash về phạm vi trong nguồn. Đã kiểm toàn bộ prefix trước sửa giữ nguyên. Không thay
trạng thái của 55 work; không thêm chữ ký hay repin artifact lịch sử. Các path được đọc từ danh sách
nghiệm thu đã ghim, không thêm marker vào WIP của phiên khác.

Hash SHA-256 được ghi theo nhóm tám ký tự; bỏ dấu nối để khôi phục đúng 64 ký tự hex.
Cách ghi này tránh nhầm dãy chữ số trong hash thành số điện thoại; scanner giữ nguyên.

Trong số 55 gói, 13 có C2 xanh tại **candidate trước `3cf6959`**:
W-0029, W-0032, W-0042, W-0052, W-0088, W-0125, W-0196, W-0197, W-0207,
W-0268, W-0269, W-0272, W-0274. Đây là thứ tự ưu tiên rà đầy đủ Residual tiếp theo;
chỉ thêm marker chưa đủ để đề nghị Toàn nghiệm thu các việc này.

## Kiểm chứng

**PASS tại `b6bd852cecdfb2df9188fc41b97c94f080639b2c`**, checkout sạch: **1151/1151 test**, sau đó
**42/42 gate**, 24 entry không chạy độc lập đúng manifest. Test chạy từ `2026-09-21T04:40:26.680Z`
đến `2026-09-21T04:45:53.925Z`; sweep từ `2026-09-21T04:45:55.177Z` đến
`2026-09-21T04:51:15.894Z` (UTC). Các gate công cụ nghiệm thu dùng TestId
CT-ACCEPTANCE-C2-01 và CT-ACCEPTANCE-EVIDENCE-01, với invocation đã ghim ở manifest.
Kiểm bổ sung cho tài liệu: đủ 55 marker, 55 prefix/hash nguyên vẹn, 55 trạng thái không đổi,
liên kết nội bộ, PII và generated mirror.

## Phần còn lại và owner

Codex đối chiếu từng lỗi C1/C2 trong danh sách mới và ghi hành động tương ứng. Toàn đọc Residual
và quyết định nghiệm thu; các việc cần M3/nhà cung cấp/hạ tầng chỉ đóng bằng bằng chứng thật.
Không sửa công cụ C2 hoặc runtime ở lượt này. Bằng chứng local không chứng nhận hosted CI,
shared M3, lab SIM hay production.

## Kết quả và đợt tiếp theo

[Danh sách sinh lại](../../release/acceptance-batches.md) có **222 ứng viên: 71 XEM, 151 KHÔNG ĐẠT,
0 ĐẠT, 0 CHƯA KIỂM**. Cả 55 marker thiếu đã được xử lý; 13 việc nêu trên qua C1/C2/C4 và cần
đọc Residual trước khi trình Toàn. XEM tăng từ 57 lên 71: 13 gói được sửa C1 và W-0324 mới
chuyển EVIDENCE_SUBMITTED từ commit báo cáo trước. Đây là thay đổi hồ sơ, không phải 14 chữ ký nghiệm thu.

| Nhóm còn thiếu | Số việc | Hành động và người phụ trách |
| --- | ---: | --- |
| Chưa có gói bằng chứng riêng | 23 | Codex truy commit/artifact của đúng work, phục hồi hoặc chạy lại phạm vi thiếu; không tạo gói chỉ có marker |
| Chưa khai báo TestId/phép kiểm | 63 | Codex xác định phép kiểm đúng loại việc; Toàn chốt tiêu chí nếu là tài liệu/quyết định; không gắn test bất kỳ |
| Có TestId chưa đối chiếu được | 61 | Codex phân biệt gate ngoài .NET, ID đã đổi/retire và test chưa triển khai; bổ sung đúng mapping/quyết định/test |
| UI đã retire, có closeout | 4 | Toàn quyết định kết thúc bốn việc P3 trong W-0324; M3 xác nhận bàn giao khi thật sự nhận |

[Bảng chi tiết từng việc](remaining-assessment.json) lưu ID, loại thiếu, hành động, owner gốc,
TestId còn thiếu và tham chiếu đúng dòng/hash của tracker tại candidate. Mỗi việc vào một nhóm theo
lỗi cần xử lý đầu tiên; có thể còn thiếu sót khác sau đó. Đây là phân loại tự động theo bằng chứng,
chưa thay việc đọc toàn bộ Residual hay rà code từng việc.
Phần ghi chú không được chép khỏi tracker thành một sổ tiến độ thứ hai.

**Đề xuất đợt tiếp:** đọc toàn bộ bằng chứng/Residual và commit sau của 13 việc vừa qua C1/C2/C4;
bắt đầu W-0029/W-0032 (P4). Với 151 hồ sơ còn thiếu, ưu tiên truy 23 gói riêng trước, sau đó
đối chiếu TestId của 61 việc; 63 việc chưa khai báo phép kiểm cần tiêu chí phù hợp loại công việc.
Tám phiếu P2 vẫn chờ Toàn; chưa có quyết định ACCEPTED/CANCELLED nào được thêm ở lượt này.

Bundle local: `.artifacts/w0325-acceptance-b6bd852/acceptance-run.json`.
Hash SHA-256 bundle (ghép nhóm): `d5546cdd-ca67def0-90b927c1-48f9fe2b-a353776c-b9d6b3ca-5bdb0218-7d903046`.
Đã đối chiếu mọi verdict với CLI gốc. Commit báo cáo sau lượt chạy chỉ ghi kết quả;
không gắn kết quả b6bd852 cho một commit khác. WIP VieNeu/lab được giữ riêng.
