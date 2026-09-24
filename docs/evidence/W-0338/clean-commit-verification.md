# W-0338 — Kiểm chứng tại commit sạch

Ngày 22/09/2026. **REAL_CUSTOMER_CALL_ALLOWED=NO. Production BLOCKED.**

**PASS: 1.196/1.196 solution tests, sau đó 43/43 gate trong manifest tại cùng commit.**
Candidate: `c3da2b6336cdc2e5daa607dca39303ebb5a92cf6`.
Tree: `e758a11c181b5056daee46acfad4db1a6217fc0c`.
Checkout detached riêng sạch trước/sau cả hai bước; không dùng overlay hoặc kết quả cũ.

## Kết quả cuối

| Nhóm | Đạt | Lỗi | Bỏ qua |
| --- | ---: | ---: | ---: |
| Unit | 781 | 0 | 0 |
| Integration | 383 | 0 | 0 |
| Contract | 24 | 0 | 0 |
| Chaos | 8 | 0 | 0 |
| Full gate sweep | 43 | 0 | 24 mục được phân loại riêng trong manifest |

Test chạy `02:18:07–02:26:58 UTC`; sweep chạy tiếp `02:26:58–02:32:27 UTC`.
24 mục phân loại riêng là thư viện, generator hoặc cần runner/môi trường riêng;
không được tính thành gate PASS. Danh sách và lý do nằm trong JSON bên dưới.
Kiểm thêm cùng candidate: 11/11 Python launcher tests; API matrix validator 18/18 ca
(1 hợp lệ, 17 từ chối), dùng dữ liệu HTTP mới của `IT-API-MATRIX-38`.

Collector ghim hash bốn TRX trước sweep, kiểm đủ project, mọi dòng gate, SHA/tree và
trạng thái sạch bốn lần. Consumer đã kiểm lại gói và sinh danh sách nghiệm thu:
W-0338 đạt C1/C2/C4, ở mức **XEM** vì còn Residual; chưa chuyển `ACCEPTED`.
Danh sách đọc tracker tại candidate, nên ghi chú lịch sử về S5 không thay thế hồ sơ
S5 được task khác cập nhật sau đó.

- Manifest gốc: `.artifacts/w0338-acceptance-c3da2b6-r4/acceptance-run.json` (bốn file đầu danh sách này nằm ở `.artifacts/` cục bộ trên máy chạy, không commit vì thư mục nằm trong `.gitignore`)
- Danh sách nghiệm thu tại candidate: `.artifacts/w0338-acceptance-c3da2b6-r4/acceptance-batches.md`
- Log toàn bộ test: `.artifacts/w0338-acceptance-c3da2b6-r4/tests.log`
- Log full sweep: `.artifacts/w0338-acceptance-c3da2b6-r4/sweep.log`
- [Bản ghi SHA, snapshot, kết quả và hash artifact](clean-commit-verification.json)

## Các lượt chưa đạt và sửa chữa

| Lượt | Candidate | Kết quả và nguyên nhân |
| --- | --- | --- |
| r1 | `511375c` | 1.194/1.196; `UT-SCH-PUMP-09` cho success hoàn tất trước đủ failure continuation; API matrix thiếu dependency YAML trong checkout mới. Chưa chạy sweep. |
| r2 | `25fbfeb` | 1.196/1.196; sweep 42/43 vì `Ivr.CiPolicy` Release chưa build, trong khi gate dùng `--no-build`. |
| r3 | `25fbfeb` | 1.195/1.196; `UT-TTS-DEADLINE-02` để đơn giữ queue hết hạn khi host bận. Chưa chạy sweep. |
| r4 | `c3da2b6` | 1.196/1.196 rồi 43/43; manifest được collector chấp nhận. |

Ba lượt đầu giữ nguyên raw, không có `acceptance-run.json`. Không ghép TRX hoặc gate
từ các lượt khác để tạo PASS. Hai thiếu sót môi trường đã xử lý bằng dependency lock
và build công cụ Release theo CI. Không đổi gate, manifest hoặc collector.

Commit `511375c` chốt 17 file W-0338 và sinh lại tổng TestId; `25fbfeb` sửa test scheduler
chờ đủ bảy failure hoàn tất; `c3da2b6` sửa test deadline dùng tín hiệu giữ queue, deadline
riêng cho đơn chờ, kiểm chưa gọi provider và queue tiếp tục hoạt động. Nhóm deadline
đạt 7/7 rồi 10 lượt lặp (70/70) trước commit; bằng chứng nghiệm thu là lượt r4 sạch.
Runtime trong `src`, `deploy`, `tools`, `specs` không đổi giữa `511375c` và `c3da2b6`.
GitNexus đã chạy trước sửa/commit; hai symbol test deadline mới chưa có trong index,
nên phạm vi được đối chiếu bằng source/diff, không coi kết quả graph trống là không đổi code.

## Tái lập

Tạo checkout detached của candidate, dùng Git Bash trên Windows và chỉ một sweep trên host:

```sh
npm ci --prefix deploy/ci --ignore-scripts --no-audit --no-fund
dotnet restore deploy/ci/tools/Ivr.CiPolicy/Ivr.CiPolicy.csproj --locked-mode
dotnet build deploy/ci/tools/Ivr.CiPolicy/Ivr.CiPolicy.csproj --configuration Release --no-restore
node tools/dev/collect-acceptance-evidence.mjs --out .artifacts/w0338-new-run
node deploy/ci/scripts/acceptance-batches.mjs --evidence .artifacts/w0338-new-run/acceptance-run.json
```

Lượt này chứng nhận solution và manifest sweep tại candidate. Phép đo model/SIP và
S5 có hồ sơ riêng; không gán chúng cho binary mới bằng suy luận. S2/mirror và release
vẫn cần hồ sơ tương ứng. Commit ghi bổ sung tài liệu sau lượt chạy không tự nhận bằng
chứng của candidate này. Các WIP S5/W-0340 của task khác được giữ nguyên.
