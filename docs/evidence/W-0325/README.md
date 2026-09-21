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

PENDING — chốt commit tài liệu, thu toàn solution rồi full gate sweep trong checkout sạch,
xác minh bundle và sinh lại danh sách. Các gate công cụ nghiệm thu dùng TestId
CT-ACCEPTANCE-C2-01 và CT-ACCEPTANCE-EVIDENCE-01, với invocation đã ghim ở manifest.
Kiểm bổ sung cho tài liệu: đủ 55 marker, 55 prefix/hash nguyên vẹn, 55 trạng thái không đổi,
liên kết nội bộ, PII và generated mirror.

## Phần còn lại và owner

Codex đối chiếu từng lỗi C1/C2 trong danh sách mới và ghi hành động tương ứng. Toàn đọc Residual
và quyết định nghiệm thu; các việc cần M3/nhà cung cấp/hạ tầng chỉ đóng bằng bằng chứng thật.
Không sửa công cụ C2 hoặc runtime ở lượt này. Bằng chứng local không chứng nhận hosted CI,
shared M3, lab SIM hay production.
