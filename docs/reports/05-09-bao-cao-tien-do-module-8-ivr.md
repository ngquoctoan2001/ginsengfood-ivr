# BÁO CÁO TIẾN ĐỘ MODULE IVR — 05/09/2026 (bản chiều muộn)

> **Cảnh báo đầu tiên:** `main` đang **đỏ**. Hai integration test hỏng do lần merge trưa nay, không phải do một nhánh nào sai. Chi tiết ở mục 3.1. Bản vá là sáu dòng.

## 1. Phạm vi và mốc kiểm tra

- **Phạm vi module là backend**: API, worker, dữ liệu, telephony adapter và bề mặt để Module 3 kết nối. **Admin UI không thuộc phạm vi** — Owner chốt 05/09/2026, Module 3 sẽ tự làm console. Đã kiểm lại sau merge: thay đổi Admin UI không quay trở lại `main`.
- Mốc đo: `ba43605` (merge `worktree-gd0-fixes` vào `main`), đã đẩy lên cả GitLab và GitHub, không còn commit nào chờ. Tổng 186 commit.
- Toàn bộ số liệu mục 2 là **chạy lại hôm nay trên đúng `ba43605`**, trừ ba dòng ghi rõ là không chạy được tại chỗ.
- Nhánh `worktree-gd0-fixes` đã merge xong và đã được đánh số lại thành `W-0193`–`W-0195`, dứt điểm việc trùng mã.
- Đang có một phiên khác làm `W-0196` (P0.3 migration) trong worktree riêng `ivr-p03-expand-contract`, chưa merge. Sổ tiến độ trên `main` cũng đang có một sửa đổi chưa commit của phiên đó.

## 2. Kết quả chạy thử

| Hạng mục | Kết quả hôm nay |
|---|---|
| Build .NET Release | PASS, 0 warning, 0 error |
| Unit test | PASS 516/516 |
| Integration test với PostgreSQL thật | **FAIL 2/261** (259 pass) trong 18 phút 24 giây |
| Contract test | PASS 24/24 |
| Chaos test | PASS 8/8 |
| Tổng .NET | **807/809**, 2 đỏ |
| Test traceability | 508 TestId |
| OpenAPI | 38 operation trong đặc tả hiện hành, khớp con số vẫn dùng |
| OpenAPI self-test / validate / drift | PASS cả ba |
| Kubernetes self-test | PASS |
| DR self-test | PASS_SINGLE_HOST trên PostgreSQL thật; multi-AZ vẫn chưa chạy |
| CD self-test | PASS nhưng chỉ ở mức cấu hình: chưa pipeline nào trong repo từng chạy thật |
| Capacity self-test | PASS_UNCALIBRATED — số học đúng, kết quả là một dải, chưa có số đo thật |
| Observability / review-gate / compliance-pack | PASS cả ba |
| Docs / CI-config self-test | PASS cả hai |
| Progressive-deployment self-test | **FAIL**: W0122 vẫn drop hai bảng trong `Up()` |
| Gate-status mirror | **FAIL tạm thời**: sổ tiến độ đang có sửa đổi W-0196 chưa commit nên lệch bảng sinh. Bản đã commit thì khớp. |
| Full pipeline MOCK (`pnpm e2e:local`) | **PASS 5/5, chạy lại sau merge**: xác nhận, hủy, số sai, bấm sai phím và lỗi kỹ thuật đều ra đúng kết quả. Lỗi kỹ thuật và số sai không bị tính là lượt gọi khách; chỉ 3/5 kết quả đi vào hàng đợi callback |
| Runtime MOCK bootstrap (`pnpm dev:bootstrap`) | PASS lúc 10:06 sáng nay, chưa chạy lại sau merge |
| Security | **không chạy lại được tại chỗ**: máy này chưa cài `gitleaks`. Kết quả gần nhất là 0 lỗi HIGH và không có secret, đo trên 176 commit; nay đã 186 commit |

## 3. Backend đã sẵn sàng cho Module 3 kết nối chưa?

**Chưa, và hiện còn lùi một bước vì `main` đỏ.**

### 3.1 Lỗi phải sửa trước tiên — hồi quy do merge

Hai test đỏ đều nằm trong `CompositionRootTests`, cùng một nguyên nhân:

- Bộ kiểm tra khởi động thêm ở `W-0191` bắt buộc thư mục seed phải có đủ ba file danh mục, nếu thiếu thì chặn ngay lúc khởi động kèm hướng dẫn khắc phục.
- Lần merge lại lấy bản test của nhánh kia, mà bản đó đã xóa hàm phụ tạo ba file đó cùng hai chỗ gọi nó.
- Kết quả là test dựng thư mục seed rỗng rồi bị chính validator của `main` chặn.

Không nhánh nào sai khi đứng riêng. Đây là lỗi ghép: test của một bên gặp validator của bên kia. Bản vá là khôi phục hàm phụ sáu dòng và hai chỗ gọi. Tôi chưa sửa vì đang có phiên khác thao tác trong cùng vùng.

### 3.2 Việc đã xong

1. **P0.1 feature flags — đạt** (`W-0190`): đủ năm environment đọc được ở MOCK, environment lạ trả 404 thay vì 500, fallback fail-closed có log và counter, có test chạy đúng composition root thật.
2. **P0.2 seed/scenario — đạt** (`W-0191`): đường dẫn seed neo theo content root, báo lỗi sớm khi thiếu file, một lệnh `pnpm dev:bootstrap` dựng đủ dữ liệu chạy thử.
3. **Ba cổng runtime thành thi hành được** (`W-0195`, mới trong lần merge): sau khi `OD-V1-20` được ký, ba cổng thôi trả `false` cứng và bắt đầu đọc một bảng phê duyệt trong cơ sở dữ liệu. Bảng từ chối xóa, từ chối sửa bản đã ký, và một phê duyệt bốn mắt có người duyệt trùng người đề xuất bị chặn ngay ở tầng ràng buộc dữ liệu. Kèm migration mới, nâng tổng số migration lên 22.
4. **Sửa bất đối xứng công tắc khẩn** (`W-0195`): trước đây thiếu quyền thì vừa không bật lại được cuộc gọi, vừa không **tắt** được. Nay dừng cuộc gọi luôn khả dụng, chỉ việc cho chạy lại mới cần phê duyệt. Đây là sửa an toàn đáng kể.
5. **19 quyết định OD-V1/OD-VOICE đã ký** (`W-0194`): số quyết định còn mở giảm từ 23 xuống **4**. Bốn dòng còn lại cần số đo thật hoặc người ký thứ hai, không tự đóng được.

### 3.3 Việc chưa xong

6. **P0.3 migration expand-contract — đang làm dở**: `progressive-selftest` vẫn đỏ trên `main`. Việc này đang chạy ở worktree riêng dưới mã `W-0196`, chưa merge.
7. **Bề mặt cho Module 3**: đặc tả có đủ 38 operation, nhưng chưa có ma trận chạy thật từng operation với dữ liệu sai, quyền sai và gọi trùng. Cũng chưa có sandbox hai chiều, vì Module 3 chưa xong luồng bán hàng của họ nên không cấp được môi trường thử.
8. **Ba work item mới chưa có gói bằng chứng**: `W-0193`, `W-0194` và `W-0195` đều ở `TESTS_PASS` nhưng ô evidence để trống, trong khi `W-0190` và `W-0191` đã có. Đây là khoản nợ hồ sơ, cần bù trước khi xin nghiệm thu.
9. Vẫn chưa có pipeline CI chạy thật, staging thật, chữ ký pháp lý, hay bất kỳ cuộc gọi SIM thật nào.

## 4. Ước lượng tiến độ theo hạng mục

Phần trăm là ước lượng theo Definition of Done trong kế hoạch, không phải trạng thái gate chính thức. Bảng chỉ gồm bảy hạng mục backend.

| Hạng mục | Trọng số | Local/MOCK | Production | Phần còn thiếu chính |
|---|---:|---:|---:|---|
| Đặc tả và contract | 11% | 93% | 55% | Module 3 review/sign-off; 4 quyết định cần số đo thật hoặc người thứ hai |
| API/domain/nghiệp vụ | 22% | 97% | 67% | Ma trận HTTP đủ 38 operation, sandbox Module 3 |
| Dữ liệu/migration/retention | 13% | 84% | 50% | P0.3 expand-contract, retention pháp lý, multi-AZ DR |
| Worker/scheduler/callback | 17% | 93% | 45% | Soak 100 vòng, no-answer nhiều lượt, callback sandbox |
| TTS/telephony adapter | 11% | 90% | 20% | Duyệt giọng, vendor/lab/gateway thật |
| QA/security | 14% | 92% | 55% | `main` đang đỏ; CI chạy thật; quét bảo mật chưa chạy lại |
| Deploy/observability/DR | 12% | 84% | 37% | Progressive deploy, staging, multi-AZ |
| **Tổng có trọng số** | **100%** | **91%** | **49%** | 1 hồi quy + 1 blocker P0 + 11 gate ngoài |

Hai con số tổng là trung bình có trọng số đúng của các hàng. Local đứng yên ở 91% vì phần cộng thêm từ cổng runtime bị trừ lại bởi hồi quy đang đỏ. Production nhích từ 48% lên 49% nhờ 19 quyết định đã ký.

- [Trạng thái kiểm soát chính thức](../release/gate-status.yaml): vẫn **RUNG 0 / NO-GO**. 8/190 work item `ACCEPTED`, 124 ở `TESTS_PASS`, 11 gate ngoài còn mở, và số quyết định mở đã giảm còn **4**.
- `REAL_CUSTOMER_CALL_ALLOWED=NO`. Không có phê duyệt `PRODUCTION_CALL` nào được nạp, kể cả ở production, nên quay số thật vẫn bị từ chối ở mọi môi trường.

## 5. Kế hoạch triển khai tiếp theo

1. **Sửa hồi quy merge** — sáu dòng, làm trước mọi thứ khác để `main` xanh lại.
2. **P0.3 migration expand-contract** (`W-0196`, đang làm): thay drop trực tiếp bằng chuỗi ngừng đọc ghi, deploy bản tương thích, xác nhận hết consumer, cleanup ở release sau; kèm guard CI cấm DDL phá hủy.
3. **Bù ba gói bằng chứng** cho `W-0193`, `W-0194`, `W-0195`.
4. **Đóng API/MOCK**: ma trận đủ 38 operation với dữ liệu sai, quyền sai, gọi trùng; mở rộng `pnpm e2e:local` thành soak 100 vòng có kịch bản không nghe máy nhiều lượt.
5. **Lật ngược thế phụ thuộc với Module 3**: bên đó chưa xong luồng bán hàng nên không cấp môi trường thử được. Đóng băng hợp đồng giao tiếp và dựng sẵn một môi trường để họ gọi vào khi sẵn sàng.
6. **CI và staging**: pipeline bắt buộc, image bất biến kèm SBOM cho API/Worker/Migrate, deploy staging, cảnh báo, rồi soak 24–72 giờ.
7. **Governance**: kịch bản thoại, giọng đọc, quyền riêng tư, retention, opt-out, runbook và gói go/no-go.
8. **Mua sau cùng**: chỉ khi các mục trên xanh mới mua gói lab nhỏ nhất một SIM, đo capacity thật rồi mới quyết định quy mô production.
9. **Pilot và go-live**: UAT số nội bộ, pilot giới hạn, diễn tập rollback, sau cùng mới cân nhắc bật cuộc gọi khách thật.

Kế hoạch thi công và tiêu chí thoát từng pha nằm tại [2026-09-05-ke-hoach-hoan-thien-ivr.md](./2026-09-05-ke-hoach-hoan-thien-ivr.md).
