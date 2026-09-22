# W-0339 — Chốt kiểm toàn luồng tại commit sạch

Ngày 22/09/2026. **Hoàn tất phạm vi local, đủ để đề nghị owner nghiệm thu.**
Trạng thái tracker: **EVIDENCE_SUBMITTED**. **REAL_CUSTOMER_CALL_ALLOWED=NO.**

## Bằng chứng cùng commit

Candidate: `66a6baa2019efe69a1ede3b6179fce5b49643a6d`.
Tree: `04cf8c32cdd7935a1a150328eab99e28d80d3f0a`.
Checkout detached sạch trước/sau build, test và sweep; không dùng WIP của task khác.

- **7/7 ca toàn luồng đạt** với API, worker và migration build mới từ candidate này.
- **1.196/1.196 solution tests đạt**, không lỗi hoặc bỏ qua:
  781 unit, 383 integration, 24 contract, 8 chaos.
- Sau toàn bộ test, **43/43 gate chạy đạt** theo manifest tại cùng SHA.
  24 mục được manifest phân loại riêng không được tính thành gate PASS.
- Consumer chính thức kiểm lại SHA/tree, đủ project, hash TRX và full sweep;
  W-0339 đạt **C1/C2/C4**, đủ **21 TestId** đã khai báo trong README.
  Nhãn **XEM** là bước owner xét phạm vi, không phải test hỏng.

Thời gian test: `2026-09-22T03:43:58.132Z` đến
`2026-09-22T03:57:11.260Z`; sweep kết thúc
`2026-09-22T04:03:21.259Z`.
Lab chạy `2026-09-22T03:49:23.412297+00:00` đến `2026-09-22T03:55:56.842203+00:00`.

## Bảy ca chạy mới

| Ca | Số phần đã phát | Lượt được tính theo từng attempt | Kết quả |
| --- | ---: | --- | --- |
| Xác nhận, phím 1 | 7 | 1 | Đạt |
| Khách hủy, phím 0 | 7 | 1 | Đạt |
| TTS timeout rồi retry | 7 | 0 → 1 | Đạt |
| Hết hạn khi còn chờ | 0 | 0 — không tạo attempt | Đạt |
| Hết hạn trong lúc tạo tiếng | 0 | 0 | Đạt |
| Không bấm phím | 7 | 1 | Đạt |
| Operator ngắt cuộc gọi | 1 | 0 | Đạt |

Hai ca hết hạn không dial hoặc phát playlist thiếu. TTS timeout trước dial không
tiêu lượt: cả attempt lỗi và attempt retry đều mang số 1. Operator ngắt chỉ kết thúc
lần gọi đang diễn ra: kết quả kỹ thuật nonfinal, không sinh callback; harness tạm
pause queue riêng để giữ trạng thái đối soát. Không nhận đây là Sales hủy vĩnh viễn đơn.

Đối soát toàn DB: **7 task, 7 attempt, 4 lượt được tính, 3 lỗi kỹ thuật không tính;
9 kết quả, 6 final, 6 dòng callback**. Sai số lượt, final trùng, callback trùng và
callback cho kết quả nonfinal đều **0**. Callback ở đây là bản ghi outbox, không
chứng nhận tích hợp hoặc ACK của Sales/M3 thật.

## Provenance và lưu bằng chứng

Ghim ba image mới, tám DLL lấy từ API/worker và bundle migration trong
[manifest kiểm chứng](clean-commit-closeout.json). Ba DLL dùng chung khớp giữa API
và worker. TTS/Asterisk/Postgres/fake Sales dùng image phụ thuộc đã ghim riêng;
không gán những image phụ thuộc này thành bản build từ candidate ứng dụng.

Giữ profile 30 giây/phần, 90 giây hàng chờ, 120 giây tổng, 8 chỗ chờ; TTS
**2 CPU/4 GiB**. Kiểm cấu hình container thực tế, 13/13 file model khớp lock của
candidate và manifest duyệt giọng khớp. Không yêu cầu nghe duyệt lại.

36 artifact lịch sử vẫn đúng hash. Cả 9 file speech khớp nội dung snapshot cũ;
một file khác hash byte vì trộn LF/CRLF. Lượt mới dùng toàn bộ build sạch và
giữ riêng [bằng chứng lần đầu](verification.json), không sửa hoặc gán lại SHA cho nó.

Raw runtime ở `.artifacts/W-0339-closeout-66a6baa/`; test/sweep ở
`.artifacts/w0339-acceptance-66a6baa-r1/`. JSON ghim 51 artifact.
Replay kit giữ harness và fixture; cấu hình riêng và backup DB giả chỉ lưu local.
Đã gỡ container, SIP peer và network riêng; giữ hai volume bằng chứng.
ID, thời điểm khởi động và số lần restart của sáu container lab chung không đổi.

## Phạm vi nghiệm thu và việc tiếp nối

**Đề nghị Toàn nghiệm thu W-0339 trong phạm vi toàn luồng đơn giả trên software lab.**
Phần triển khai và kiểm chứng local của W-0339 đã hoàn tất. Toàn quyết định chuyển
ACCEPTED theo quy tắc 6 của [sổ tiến độ](../../../prompt/_execution/prompt-execution-tracker.md).

| Người phụ trách | Phần tiếp nối ngoài phạm vi lần này |
| --- | --- |
| IVR/Codex + Infra | Toàn luồng scheduler/SIP trên S5, nhiều worker và môi trường triển khai |
| Sales/M3 + IVR | Tích hợp hệ thống thật; lượt này dùng đơn giả và fake Sales |
| Các owner W-0340/W-0341 + release owner | Quyền model/codec, mirror và chuỗi provenance của bản phát hành |
| IVR/Infra + release owner | Nếu dùng ba image mới để phát hành: quét đúng image và hoàn tất điều kiện vận hành; scan W-0340 của image cũ không tự áp dụng cho chúng |

Không có SIM/PSTN hoặc khách thật trong lượt này. Quyền production giữ riêng.
