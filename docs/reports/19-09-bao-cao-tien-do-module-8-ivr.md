# BÁO CÁO TIẾN ĐỘ MODULE 8 — IVR XÁC NHẬN ĐƠN HÀNG

**Ngày báo cáo:** 19/09/2026 · **Người thực hiện:** Nguyễn Quốc Toàn.
**Phạm vi rà soát:** commit từ 12/09 đến thời điểm lập báo cáo 18/09/2026; chốt mã `main@e33927d` (18/09, 11:11 +07). Chưa bao gồm diễn biến ngày 19/09.
**Trạng thái điều hành:** `RELEASE_BLOCKED / NẤC 0`. Phần mềm đã mở rộng đường nhận việc, điều phối gọi và kiểm toán; kiểm chứng local tiến thêm. **Chưa đủ điều kiện gọi khách thật**: còn chờ M3, quyết định của Sếp, môi trường triển khai và tuyến nhà mạng. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 1. TIẾN ĐỘ

| Hạng mục | Tiến triển so với báo cáo 12/09 | Trạng thái hiện tại |
| --- | --- | --- |
| Nhận việc và hợp đồng M3 | `draft.27` → **`draft.31`**, 38 → **39 lệnh API**; nhận `phone_e164` trực tiếp, M3 không phải xây bộ cấp token; kiểm định dạng số ngay đầu vào | Code và sandbox đã kiểm; hợp đồng vẫn **DRAFT**, chờ M3 ký |
| Vòng đời và điều phối cuộc gọi | Worker tự xét điều kiện kỹ thuật; gọi song song có trần, giảm tải khi lỗi liên tiếp, khoá sở hữu tổng đài và sổ đếm bền qua restart | Đã kiểm bằng phần mềm/PostgreSQL; chưa chứng minh dung lượng nhà mạng thật |
| Đường gọi SIP | Đã dựng đường lấy số → địa chỉ SIP → Asterisk, hướng dẫn vận hành và chặn cấu hình vượt số kênh/tốc độ nhà mạng | Thiếu tuyến thật và nghiệm thu; mặc định điều phối vẫn **1 cuộc đồng thời** |
| Giọng đọc | **VieNeu tự host là engine duy nhất** cho lab và production; bỏ provider phát file cũ, giữ 12 đoạn VieNeu đã render và giọng ba miền | Production vẫn tắt; cần model, máy thật, nghe duyệt và gọi thử lại |
| Quyền riêng tư và kiểm toán | Có API tra lịch sử audit; sửa xoá dữ liệu để xoá cả số điện thoại, chạy lại an toàn và dọn dữ liệu đã ẩn danh kiểu cũ | **Chưa có endpoint/CLI vận hành việc xoá**; còn chờ quyết định `S8` |
| CI, triển khai và nghiệm thu | Hosted đã kiểm/publish một candidate; bộ kiểm tra local tăng lên 41; có tiêu chí và danh sách nghiệm thu theo đợt | **44/306 hạng mục ACCEPTED**, 19 chặn ngoài, 11 cổng mở, 6 quyết định OD-V1 còn mở |

Nguồn hiện hành: [hợp đồng API](../../specs/api/openapi/ivr-order-confirmation.v1.yaml), [bảng trạng thái phát hành](../release/readiness-board.md), [đường gọi production](../operations/production-dial-path.md). Số nghiệm thu tăng 43 → 44; không quy đổi số việc đã viết code thành phần trăm sẵn sàng vận hành.

## 2. NHỮNG VIỆC ĐÃ LÀM

**80 commit trong phạm vi đọc:** 12/09: 11 · 13/09: 0 · 14/09: 19 · 15/09: 13 · 16/09: 19 · 17/09: 12 · 18/09: 6. Trong đó **69 commit sau ngày 12/09**; phần 12/09 được nhắc để nối mốc, không tính thành thành quả mới lần hai.

| Ngày | Kết quả chính |
| --- | --- |
| 12/09 | Sửa lỗi tổng hợp số liệu do phím không hợp lệ; dựng sandbox cho M3 và hạn mức 60 lệnh/phút; sửa lỗi kết nối và vòng thăm dò tự tiêu hạn mức |
| 14/09 | Worker tự xử lý việc chờ xét điều kiện; sửa xung đột khi nhiều yêu cầu đến đồng thời; hoàn thành diễn tập 100 vòng; sửa runner/CI, bảo vệ dữ liệu trong báo cáo test; publish ba image và portal API; owner chốt nhãn hết hạn do operator pause |
| 15/09 | Điều phối gọi song song có giới hạn; giảm tải sau lỗi; kiểm trần dùng chung nhiều worker; thêm khoá sở hữu ARI, sổ đếm resolve lưu PostgreSQL và phê duyệt gọi production theo môi trường; lập phương án trung kế SIP |
| 16/09 | Từ chối sớm việc không có lượt nào nằm trong giờ gọi; trả `422` khi token quá cửa sổ thay vì lỗi `500`; sửa các cổng phê duyệt không được tự mở qua migration; hoàn thành ba phần đường gọi production; tự seed môi trường mới, giảm log; thêm API `audit-evidence` |
| 17/09 | Gom đúng một phiếu 30 câu gửi M3; thêm cột số điện thoại và đường quay số trực tiếp; phát hành `draft.30` rồi `draft.31` để task chỉ gửi số được nhận; sửa tài liệu hợp đồng, quét PII và ghi nhận các quyết định mới |
| 18/09 | Sửa luồng xoá dữ liệu và trigger bảo vệ số; thống nhất VieNeu; cập nhật sổ quyết định; vá image TTS hết 3 phát hiện CRITICAL; chuẩn bị cấu hình production nháp; lập danh sách nghiệm thu theo đợt, chưa tự nâng ACCEPTED |

Các thay đổi được đối chiếu với code hiện tại và hồ sơ [W-0286](../evidence/W-0286/README.md), [W-0306](../evidence/W-0306/README.md), [W-0307](../evidence/W-0307/README.md), [W-0308](../evidence/W-0308/README.md), [W-0312](../evidence/W-0312/README.md), [W-0314](../evidence/W-0314/README.md), [W-0315](../evidence/W-0315/README.md).

## 3. KẾT NỐI VỚI MODULE 3 — VẪN CHỜ PHẢN HỒI

| Nội dung | Trạng thái đến mốc báo cáo |
| --- | --- |
| [Phiếu IR-07](../../integration-requirements/07-module-3-decision-sheet.md) | **30 câu**, đã gửi 17/09, `SENT / AWAITING_REPLY`; thay bản 21 câu của tuần trước |
| Cách gửi số để gọi | `draft.31` cho phép gửi số trực tiếp; không bắt M3 tự cấp token. Vẫn giữ tương thích đầu vào token trong giai đoạn chuyển đổi |
| Đồng bộ bản bàn giao | Hồ sơ ghi nhận đính chính `draft.31` đã gửi 17/09; **đính chính A-9 về lưu dữ liệu chưa gửi**, cần bổ sung cùng đợt đối soát |
| Sandbox | **28/28 ví dụ HTTP đạt** ở W-0314, có task chỉ gửi số chạy tới callback giả lập; chưa phải đấu nối hai hệ thống thật |
| Phần chờ M3 | Ký hợp đồng; cung cấp địa chỉ callback và xác thực; chốt thu hồi đơn, mã phiên Giờ Vàng, nghĩa của giới hạn “hai lần trong 10 phút”; đối soát ACK và cập nhật trạng thái đơn |

**Chưa có bằng chứng end-to-end với M3 thật.** Năm cổng `G-CONTRACT`, `G-SPEECH`, `G-DIAL`, `G-AUTH`, `G-POLICY` vẫn mở. Lịch hẹn cũ là tuần 14–19/09; tại mốc 18/09 hồ sơ chưa ghi nhận phiếu đã ký trả lại.

## 4. KẾT QUẢ KIỂM CHỨNG

Các số dưới đây lấy từ hồ sơ đã lưu trong repo, gắn với đúng mốc kiểm; lần lập báo cáo này **không chạy lại toàn bộ test, hosted CI hay cuộc gọi**.

| Nhóm | Bằng chứng gần nhất và giới hạn |
| --- | --- |
| Toàn bộ kiểm thử .NET | **1.151/1.151**, 0 lỗi/0 bỏ qua tại `3d0111a`: 756 unit · 363 integration · 24 contract · 8 chaos. `e33927d` không đổi `src/` hoặc `tests/` so với mốc đó. Số test giảm từ 1.184 sau khi gỡ phần giọng đọc cũ, không phải bỏ qua test lỗi |
| Bộ kiểm tra local | [W-0318](../evidence/W-0318/README.md): **41/41 gate chạy đạt** sau khi thêm kiểm tra danh sách nghiệm thu; quét PII 390 file đạt. Không đồng nghĩa 41 job hosted đã chạy |
| Độ bền giả lập | [W-0286](../evidence/W-0286/README.md), `4483029`: **100/100 vòng, 1.130 task, 0 lỗi**, có crash/lease, dừng khẩn, mất đầu nhận, gửi lại và hàng đợi lỗi; chưa chạy lại trên HEAD mới |
| Số điện thoại và xoá dữ liệu | W-0312: 7/7 đột biến bị test bắt; W-0314: 8/8 đột biến bị bắt và **28/28 ví dụ trên stack riêng đạt**, gồm migration và xoá số điện thoại trong dữ liệu thử |
| Hosted CI / publish | [W-0292](../evidence/W-0292/README.md), `179a5eb`: **1.018/1.018 test, 31 job PASS**, image E2E/Kubernetes đạt, ba image đã publish. **Pipeline tổng thể Failed** do dev không kết nối được cluster; staging skipped. Không dùng kết quả này để chứng nhận HEAD 18/09 |
| Image VieNeu | [W-0317](../evidence/W-0317/README.md): quét 18/09 từ 57 phát hiện HIGH/CRITICAL, vá còn **44 HIGH / 0 CRITICAL**. Image thử Chainguard quét 0 nhưng **chưa được thay vào bản chính**, chưa tổng hợp giọng bằng model thật |
| Trạng thái phát hành | Kiểm lại `gate-status.mjs` khi lập báo cáo: **PASS**, 306 hạng mục, 11 cổng, 6 OD mở; **nấc 0**, cờ gọi khách thật vẫn tắt |

## 5. KẾ HOẠCH TRIỂN KHAI TIẾP THEO

| Ưu tiên | Việc cần làm | Điều kiện / người phụ trách |
| --- | --- | --- |
| 1 | Nhận IR-07 đã ký, gửi bổ sung A-9, chốt khác biệt hợp đồng; chạy M3 → IVR → callback → trạng thái đơn và đối soát retry/ACK/thu hồi | Toàn + tech lead M3; bắt đầu đấu nối khi có đầu nhận và thông tin xác thực |
| 2 | Duyệt nghiệm thu theo đợt trên đúng commit, xử lý hồ sơ còn thiếu rồi cập nhật tracker | Toàn là release owner. Danh sách đầu: **218 ứng viên, 138 cần xem, 80 chưa đạt tiêu chí, 0 tự đạt**; kết quả sàng lọc hồ sơ, không phải 80 lỗi phần mềm |
| 3 | Chốt quyền và công cụ xoá dữ liệu khách (`S8`), sau đó triển khai lối vận hành có audit và kiểm thử quyền | Sếp + Toàn; phần xử lý xoá trong code đã xong, công cụ vận hành còn phải làm |
| 4 | Hoàn tất VieNeu: chọn nền image, cấp model/máy chạy, đo độ trễ, nghe duyệt 12 đoạn và chỗ nối, thực hiện 6 cuộc gọi thử MicroSIP | Chờ `S1`, `S2`, `S5`; cấu hình production giữ tắt đến khi đủ bằng chứng |
| 5 | Cấp môi trường staging và kết nối CI; chạy lại pipeline trên candidate mới, deploy/smoke, diễn tập rollback/khôi phục, đo tải và chạy bền 24–72 giờ | Sếp chỉ định người phụ trách/cấp hạ tầng; chưa đặt ngày hoàn tất khi chưa có môi trường |
| 6 | Nhận báo giá và tuyến SIP, kiểm hiển thị số/tên, DTMF, lỗi mạng và dung lượng; hiệu chỉnh bằng số đo rồi nghiệm thu pilot giới hạn | Sếp + nhà mạng + Toàn; hướng đã chốt là **một nhà mạng, bắt đầu 8 kênh**, đo rồi mới nâng |

## 6. VIỆC CẦN CHỐT ĐỂ GIỮ LỊCH

| Bên quyết định | Đầu vào còn thiếu | Hệ quả nếu chưa có |
| --- | --- | --- |
| **Module 3** | Phiếu 30 câu đã ký, callback/xác thực và các hợp đồng còn mở | Chưa đóng được làn tích hợp, chưa kiểm chứng thay đổi trạng thái đơn thật |
| **Sếp — S1, S2** | Chỉ định đủ người duyệt; ký nhận rủi ro bản đầu, gồm lưu số trực tiếp/lưu vĩnh viễn, image và quyền dùng model VieNeu | Chưa đủ điều kiện duyệt lời thoại, bật TTS và gọi khách thật |
| **Sếp — S5, S6, S8** | Ghi rõ môi trường được cấp; chốt nhà mạng/gói thử; giao người và cách xoá dữ liệu | Chặn staging, đo năng lực thật và quy trình đáp ứng yêu cầu xoá |
| **Toàn** | Duyệt từng đợt bằng chứng và đóng phần việc còn thuộc IVR sau các quyết định trên | Test xanh vẫn chưa trở thành nghiệm thu phát hành |

Nguồn quyết định: [vướng mắc ngày 17/09, cập nhật 18/09](../../plan/ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md). Tổ chức không có các đội Platform/Security/Legal riêng; đầu việc được quy về Sếp và người được giao. **Chỉ xét mở pilot khi đủ bằng chứng trên đúng ứng viên và có phê duyệt; báo cáo này không mở quyền gọi khách thật.**
