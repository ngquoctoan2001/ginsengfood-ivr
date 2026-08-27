**GINSENGFOOD**

**MODULE 8 — IVR ORDER CONFIRMATION**

_Xác nhận đơn hàng bằng cuộc gọi tự động nội bộ — SIM Gateway — Scheduler — DTMF — Order Core callback_

| **Trường**             | **Giá trị**                                                                |
| ---------------------- | -------------------------------------------------------------------------- |
| Mã tài liệu            | GFD-M8-IVR-ORDER-CONFIRMATION-TECHDESC-003                                 |
| Phiên bản              | V0.3 — Clean, hiệu chỉnh từ V0.2 theo kết quả rà soát code ngày 26/08/2026 |
| Thay thế               | V0.2 Clean Final (GFD-...-TECHDESC-002). V0.2 ngừng sử dụng.               |
| Ngày                   | 26/08/2026                                                                 |
| Module trực quan       | Module 8 — IVR Order Confirmation                                          |
| Phase / Pack canonical | Phase 8 — IVR Order Confirmation / PACK-09                                 |
| Deployment model       | INTERNAL_SIM_GATEWAY_SERVER                                                |
| Trạng thái tài liệu    | READY FOR OWNER / TECH LEAD / DEV REVIEW                                   |
| Trạng thái triển khai  | IMPLEMENTATION_COMPLETE_BEHIND_MOCKS — xem §2                              |
| Global gate            | BLOCKED                                                                    |
| Real customer call     | NO                                                                         |
| Production ready       | NO                                                                         |

<div class="joplin-table-wrapper"><table><tbody><tr><th><p><strong>KẾT LUẬN KHÓA</strong></p><ul><li>Module 8 chỉ xác nhận Official Order đủ điều kiện bằng cuộc gọi tự động nội bộ.</li><li>IVR result chỉ là tín hiệu gửi về Order Core; Order Core mới là nơi quyết định trạng thái cuối.</li><li>Không dùng IVR để bán thêm, tư vấn, CRM, chăm sóc khách hàng đại trà, đọc combo hoặc đọc chương trình.</li><li>Mô hình triển khai chính thức là Internal SIM Gateway Server; không mặc định dùng Voice Brandname, SIP Trunk hoặc Cloud IVR.</li><li>Không được mở gọi khách thật trước khi smoke, evidence, security và owner sign-off PASS.</li><li>MỚI Ở V0.3: phần mềm đã xây dựng gần xong sau mock; thứ còn chặn go-live là hạ tầng viễn thông, hợp đồng Sales và chữ ký owner — không phải code. Xem §2 và §3.</li></ul></th></tr></tbody></table></div>

# **0\. Bản V0.3 sửa gì so với V0.2**

V0.2 được viết khi chưa có dòng code nào. Sau 101 commit, nhiều phát biểu trong V0.2 đã sai hoặc thiếu. Bảng dưới liệt kê từng chỗ đã sửa, để người đọc V0.2 biết chính xác cái gì đã đổi và vì sao.

| **#** | **V0.2 nói gì**                                             | **V0.3 sửa thành gì**                                                                                                                  |
| ----- | ----------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| 1     | IVR IMPLEMENTATION STATUS = NOT_STARTED                     | Sai. Đã có 5 project .NET, console Next.js, 77 hạng mục TESTS_PASS. Thay bằng §2 với số liệu đếm được.                                 |
| 2     | Không có mục nào nêu rào chắn go-live                       | Thêm §3 — Sổ rào chắn: 9 rào, mỗi rào có chủ sở hữu, gate và điều kiện thoát.                                                          |
| 3     | §8 nêu hai bộ attempt policy xung đột, để mở                | Thêm nguồn xung đột thứ ba (phase-8 docs) và chốt rule triển khai + điều kiện đóng OD-V1-16.                                           |
| 4     | Không có ngưỡng số nào ngoài "fail_count ≥ 3 trong 10 phút" | Thêm §11 — bảng đầy đủ hằng số runtime, kèm cột nguồn và cột "đã đo thật chưa".                                                        |
| 5     | §13 liệt kê 9 result code                                   | Thực tế code có 11. Bổ sung IVR_CAPACITY_EXCEPTION, IVR_OPERATIONAL_BLOCKED, IVR_POLICY_BLOCKED. Xem §16.                              |
| 6     | IVR_OPT_OUT là một result code                              | Không đúng. Opt-out chặn ở eligibility nên không bao giờ phát sinh cuộc gọi; kết quả là IVR_POLICY_BLOCKED.                            |
| 7     | IVR_NO_ANSWER_FINAL → "Order Core hủy đơn"                  | Code không xin hủy vì không nghe máy; nó gửi khuyến nghị NO_STATE_CHANGE và chờ window hết hạn. An toàn hơn. §16 ghi đúng hành vi này. |
| 8     | Bảng biến call script và bảng capacity bị lặp cột           | Dựng lại sạch ở §12 và §14.                                                                                                            |
| 9     | Không nói gì về thiết bị viễn thông vật lý                  | Thêm §13 — yêu cầu thiết bị, tiêu chí loại trừ, đường tích hợp qua SIP trunk.                                                          |
| 10    | Không có mục retention/DSAR                                 | Thêm §20.3 với trạng thái OWNER_DATA_REQUIRED và danh sách lớp dữ liệu cần chốt.                                                       |
| 11    | Hệ số capacity 35 giây / 50 giây trình bày như dữ kiện      | Đánh dấu rõ là giả định chưa đo, kèm công thức thay thế khi có số thật.                                                                |
| 12    | Không nêu ràng buộc chương trình ↔ phương thức thanh toán   | Code đang ép GOLDEN_HOUR↔ONLINE và TWENTY_FOUR_SEVEN↔COD. Ràng buộc này chưa có nguồn business đã duyệt — ghi vào §26 (OD-V1-13).      |
| 13    | (sửa 27/08) §13.2 yêu cầu thiết bị phân biệt "11 giá trị ở §16" | Trỏ nhầm bảng — §16 là result code của phần mềm, thiết bị không thấy được. Thêm §13.3 là bảng call disposition thật (11 giá trị, lấy từ SimProviderDisposition trong code). Hai bảng tình cờ cùng có 11 dòng, đó là nguồn nhầm lẫn. |
| 14    | (sửa 27/08) §14 dùng từ "phiên" mà không định nghĩa       | §14.1/§14.2/§23 chứa ba phát biểu không thể cùng đúng. Đã nêu rõ ba đơn vị thời gian khác nhau, viết lại §14.2 theo giả định phiên = 45 phút (con số duy nhất làm ba câu khớp nhau), và mở M8-OD-C để owner chốt. |
| 15    | (sửa 27/08) §10.3/§16 mô tả sai hành vi hết window        | Xem ghi chú trong §16. Kèm ba migration W-0116/0117/0118 đưa bất biến "không tính lượt" xuống tầng schema. |
| 16    | (sửa 27/08) §11 ghi DTMF timeout "15 giây (lab đặt 60)"    | Sai. 15 chỉ là default code, bị appsettings ghi đè xuống 10. Thêm §11.1 liệt kê đủ bốn nơi đặt giá trị, và cảnh báo lab đo ở 60 giây sẽ thổi phồng số liệu đưa vào §14.3. |
| 17    | (sửa 27/08) §20.2 tuyên bố chặn số thô rộng hơn thực tế   | Guard cũ chỉ bắt khi TOÀN BỘ chuỗi có hình dạng số. "tel:0912345678", "sip:...@gw", "PHONE_09..." đều lọt. Đã siết trong code (W-0119) và mô tả lại §20.2 cho đúng. |
| 18    | (sửa 27/08) §26 thiếu OD-V1-14                            | Không phải lỗi đánh số — quyết định này có thật trong sổ đăng ký và là mục nghiêm trọng nhất (có thể làm 100% task bị từ chối ngày cắm thật). Đã bổ sung, kèm con trỏ tới sổ đăng ký làm nguồn có thẩm quyền. |
| 19    | (sửa 27/08, sau review M3) §10 ghi "ba nguồn mâu thuẫn"    | Thành **bốn**. Contract submodule của M3 dùng Giờ Vàng `[0,300]`/10 phút và 24/7 3 cuộc `[0,300,600]`. Nặng hơn: M3 nêu **không nguồn nào đỡ cho `[0,150]`** — chính con số IVR đang chạy. |
| 20    | (sửa 27/08, sau review M3) OD-V1-13 ghi "chưa có nguồn business" | Sai, đã cũ. M3 dẫn Flow 04/05 khóa `24_7+COD` và `GOLDEN_HOUR+ONLINE`. Bảng ép trong code IVR khớp nghiệp vụ. Hạ từ CHẶN xuống "ký wire mapping". |

# **1\. Mục đích tài liệu**

Tài liệu mô tả kỹ thuật Module 8 — IVR Order Confirmation cho hệ thống Ginsengfood: hợp phần xác nhận đơn hàng bằng cuộc gọi tự động nội bộ, dùng SIM Gateway nội bộ, call scheduler, DTMF capture, result normalizer và callback về Commerce Order Core. Đối tượng đọc: owner, tech lead, dev backend, dev queue/worker, dev dashboard, QA và vận hành.

<div class="joplin-table-wrapper"><table><tbody><tr><th><p><strong>CẢNH BÁO KỸ THUẬT BẮT BUỘC</strong></p><ul><li>Module 8 không thể triển khai bằng cách copy-paste vài đoạn code gọi điện. Một hệ thống IVR thật phải khớp với Official Order, Order Core, state machine, scheduler, SIM channel lock, DTMF, callback, audit, idempotency, evidence và release gate.</li><li>Nếu dev tự ghép code rời rạc, hệ thống có thể gọi sai khách, hủy sai đơn, nhầm lỗi kỹ thuật thành khách không nghe, nghẽn SIM trong Giờ Vàng hoặc tạo rủi ro pháp lý về dữ liệu cá nhân.</li><li>Người dùng/owner quyết định muốn quy trình xác nhận ra sao. Dev quyết định cách triển khai kỹ thuật theo contract, resolver, guard, state machine và smoke test đã khóa.</li></ul></th></tr></tbody></table></div>

# **2\. Trạng thái triển khai thực tế**

Đây là mục quan trọng nhất mà V0.2 thiếu. Không có nó, người đọc kết luận sai rằng chưa có gì được làm, và đi hỏi lại những câu đã có câu trả lời trong code.

## **2.1. V0.2 nói gì so với thực tế**

| **Hạng mục**        | **V0.2 ghi** | **Thực tế ngày 26/08/2026**                                                                       |
| ------------------- | ------------ | ------------------------------------------------------------------------------------------------- |
| Implementation      | NOT_STARTED  | 101 commit. 5 project .NET (Api, Worker, Infrastructure, Domain, Contracts) + console Next.js.    |
| Khối lượng code     | —            | 242 file .cs; Infrastructure 137 file, Api 51, Domain 36, Worker 13, Contracts 5.                 |
| Kiểm thử            | —            | 285 unit, 211 integration, 10 contract, 8 chaos (đếm theo \[Fact\]/\[Theory\]); 179 test console. |
| Work item           | —            | 77 TESTS_PASS · 20 EVIDENCE_SUBMITTED · 5 ACCEPTED · 26 BLOCKED_EXTERNAL · 2 DEFERRED_TARGET.     |
| Cuộc gọi khách thật | NO           | Vẫn NO, và được ép bằng code: đặt YES làm ứng dụng từ chối khởi động.                             |
| Production ready    | NO           | Vẫn NO. Xem §3.                                                                                   |

_Nguồn: đếm trực tiếp trên repository và sổ tiến độ prompt-execution-tracker.md._

## **2.2. Ba mốc và mốc đang đứng**

| **Mốc**                              | **Trạng thái** | **Ý nghĩa**                                                                            |
| ------------------------------------ | -------------- | -------------------------------------------------------------------------------------- |
| IMPLEMENTATION_COMPLETE_BEHIND_MOCKS | ĐANG Ở ĐÂY     | Phần mềm chạy đủ luồng với Sales giả và SIM giả. Không cần bên ngoài để hoàn tất code. |
| LAB_REAL_SIM_VERIFIED                | CHƯA           | Cần 1 SIM thật, gateway, số test được duyệt. Chặn bởi B-01, B-02.                      |
| PRODUCTION_REAL_ELIGIBLE             | CHƯA           | Cần toàn bộ contract, auth, policy, legal, capacity và release gate.                   |

**Điểm cần nói thẳng với owner: khoảng cách từ mốc 1 sang mốc 2 không phải là công sức lập trình. Đó là mua sắm thiết bị, hợp đồng với Sales và chữ ký. Thêm người code không rút ngắn được nó.**

# **3\. Sổ rào chắn go-live**

Chín rào chắn dưới đây được rà ra từ code và sổ tiến độ ngày 26/08/2026. Mục này tồn tại để chúng không nằm im trong tracker kỹ thuật rồi bị phát hiện muộn. Mỗi rào có chủ sở hữu và điều kiện thoát; không rào nào đóng bằng suy luận hay bằng mock.

| **ID** | **Rào chắn**                                                                                                                                                                                   | **Mức**        | **Chủ sở hữu**            | **Điều kiện thoát**                                                                                                                                                    |
| ------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------- | ------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| B-01   | Không có adapter telephony thật. Chỉ có FakeSimGateway (mock) và AsteriskAriSimGateway (softphone, không ra PSTN). Nhánh còn lại của DI là UnavailableSchedulerDispatchGateway, ném exception. | CHẶN           | Infra + vendor            | Thiết bị/gateway đã lắp; adapter implement đủ 6 operation của ISimGateway; biểu mẫu nghiệm thu lab R-02 §6 điền đủ 7+15 dòng. Gate G-LAB-SIM (W-0048).                 |
| B-02   | Dial-token resolver là giả. LabDialTokenVault luôn trả về đúng một alias LAB-A; không có mapping alias → số thật.                                                                              | CHẶN           | Security + Sales + vendor | Chốt vị trí resolve dial_token → E.164 (OD-V1-18), có sơ đồ trust boundary đã duyệt, threat model và vendor capability statement. Resolver phải nằm NGOÀI process IVR. |
| B-03   | Contract Sales/Order Core vẫn DRAFT. Chín hạng mục W-0002…W-0009 ở BLOCKED_EXTERNAL: chưa có OpenAPI ký duyệt, chưa có profile JWT/mTLS.                                                       | CHẶN           | Sales API/Core + Security | OpenAPI Target V1 đã ký; contract test hai chiều xanh trên sandbox thật; profile auth đã ký và credential sandbox hoạt động. Gate G-CONTRACT, G-AUTH.                  |
| B-04   | Attempt policy chưa được owner ký. Version đang dùng là mock-lab-v1, đánh dấu CandidateMockLabOnly; chế độ production sẽ từ chối chính version này.                                            | CẦN QUYẾT ĐỊNH | Product + Order Core      | Owner ký một attempt_policy_version, kèm giải quyết xung đột ba nguồn ở §10. Nạp vào registry với approved_for_production = true. OD-V1-08, OD-V1-16.                  |
| B-05   | Chưa có rate limiting. Hiện chỉ có ánh xạ mã 429, không có middleware thực thi.                                                                                                                | CẦN QUYẾT ĐỊNH | Infra + Ops               | Owner vận hành duyệt ngưỡng (req/giây theo caller, theo endpoint). Sau đó implement middleware + test âm chứng minh chặn thật.                                         |
| B-06   | Observability mới phủ 1/5 chặng — chỉ callback có span. Export OTLP chặn bởi hạ tầng chưa cấp.                                                                                                 | CẦN XỬ LÝ      | Platform + Infra          | Cấp backend tracing/metrics/logs; instrument bốn chặng còn lại (intake, eligibility, scheduler/dispatch, normalization); chứng minh trace xuyên suốt một task. W-0063. |
| B-07   | Chưa chạy soak 4–8 giờ. Năng lực 32 kênh mới có simulator, chưa đo trên thiết bị thật.                                                                                                         | CẦN XỬ LÝ      | Infra + QA                | Soak test có báo cáo; đo throughput/failover trên số kênh thật đã mua. Gate G-ESIM32. Không suy ra năng lực n kênh từ 1 kênh.                                          |
| B-08   | Chưa pipeline deploy nào chạy thật. CI/CD mới pass ở mức cấu hình và self-test.                                                                                                                | THEO DÕI       | Platform                  | Cấp runner, registry và credential cluster; chạy thật một lượt deploy → promote → rollback và lưu evidence. W-0061, W-0063.                                            |
| B-09   | Mười hai file audio đoạn cố định chưa ai nghe thử. Ba giọng vùng miền được chốt dựa trên mô tả văn bản, không dựa trên nghe.                                                                   | THEO DÕI       | Owner + Product           | Owner nghe đủ ba giọng và ký; nghe mối nối giữa đoạn thu sẵn và đoạn tổng hợp trong một cuộc gọi thật. OD-VOICE-01, OD-VOICE-05.                                       |

<div class="joplin-table-wrapper"><table><tbody><tr><th><p><strong>CÁCH ĐỌC SỔ RÀO CHẮN</strong></p><ul><li>CHẶN nghĩa là không thể gọi khách thật khi rào còn mở, dù mọi thứ khác xanh.</li><li>B-01, B-02 và B-03 không giải quyết được bằng cách viết thêm code. Chúng cần thiết bị, hợp đồng và chữ ký từ bên ngoài đội IVR.</li><li>B-04 chỉ cần một chữ ký, nhưng nếu thiếu thì hệ thống sẽ từ chối khởi động ở chế độ production — nên nó không thể bị quên.</li><li>Trong lúc chờ, việc hợp lệ là: hoàn thiện B-05, B-06, B-08, B-09 và chạy lab softphone. Việc KHÔNG hợp lệ là nới một gate để "test cho nhanh".</li></ul></th></tr></tbody></table></div>

# **4\. Source-of-truth và thứ tự ưu tiên**

Module 8 là downstream consumer của Commerce Runtime, Operational Core, AI Advisor, Gateway, Ads và MC AI Live. Mọi quyết định trạng thái đơn phải quay về Order Core; IVR không được tự quyết định.

| **#** | **Nguồn**                               | **Vai trò trong Module 8**                                                                    |
| ----- | --------------------------------------- | --------------------------------------------------------------------------------------------- |
| 1     | MASTER Governance / PACK Registry       | Khóa owner boundary, source-of-truth, dependency, evidence, release gate.                     |
| 2     | Module 3 / Phase 3 — Commerce Runtime   | Nguồn Official Order, order_code, order state, payment/shipping/revenue boundary.             |
| 3     | Module 4 / Phase 4 — AI Advisor Runtime | Nguồn customer-facing wording, final response guard; không tự tính giá/order/payment.         |
| 4     | Module 5 / Phase 5 — Facebook Gateway   | Nguồn channel identity, Messenger handoff, public/private boundary.                           |
| 5     | Module 6 / Phase 6 — Ads Measurement    | Nguồn attribution/ROAS signal boundary; không tính revenue từ IVR.                            |
| 6     | Module 7 / Phase 7 — MC AI Live         | Nguồn live session context; Live không tự xác nhận order/payment.                             |
| 7     | Phase 8 SRS Consolidated                | Nguồn baseline IVR governance, scope, contract, scheduler, SIM adapter, normalizer, evidence. |
| 8     | PACK-09 IVR Input Baseline V1.0         | Nguồn Owner Lock cho Internal SIM Gateway, script, attempt rule, capacity.                    |

# **5\. Vai trò và ranh giới Module 8**

| **Nhóm**           | **Được làm**                                                                               | **Bị cấm**                                                                          |
| ------------------ | ------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------- |
| Order confirmation | Gọi xác nhận Official Order đủ điều kiện; ghi nhận phím 1/0; gửi IVR result về Order Core. | Tạo quote, cart, order draft, official order hoặc tự hủy đơn.                       |
| Order state        | Gửi signal + evidence cho Order Core revalidate.                                           | Tự transition order state, tự set CONFIRMED/CANCELLED/DELIVERED/VERIFIED.           |
| Payment / revenue  | Đọc payment/order state public-safe nếu Order Core đưa vào task.                           | Xác nhận PAID, COD_VERIFIED, DELIVERED, VERIFIED_REVENUE, commission, ROAS, payout. |
| Customer data      | Dùng phone_ref/dial_token/phone_masked được duyệt.                                         | Đọc hoặc log full profile, full address, payment detail, health note, CRM note.     |
| Communication      | Call script ngắn phục vụ xác nhận đơn.                                                     | Tư vấn sản phẩm, đọc combo, đọc chương trình, upsell, mời member/Diamond, CRM.      |
| Evidence           | Ghi audit/evidence cho task, attempt, result, callback, admin action.                      | Tự gọi PASS/production ready nếu evidence chưa accepted.                            |

# **6\. Kiến trúc vận hành tổng thể**

| **Bước** | **Thành phần**                | **Mô tả kỹ thuật**                                                                                       | **Output**                |
| -------- | ----------------------------- | -------------------------------------------------------------------------------------------------------- | ------------------------- |
| 1        | Order Core                    | Tạo IVR task cho Official Order đủ điều kiện, có correlation_id và idempotency_key.                      | IVRTaskRequested          |
| 2        | IVR Eligibility Rule          | Kiểm chương trình, window, trust, contact, sale lock, recall, opt-out, suppression, payment/order state. | ELIGIBLE / BLOCKED        |
| 3        | IVR Call Scheduler            | Xếp lịch theo deadline-aware rolling queue, không dồn cuối phiên.                                        | CallJob scheduled         |
| 4        | SIM Gateway Channel Pool      | Chọn SIM rảnh, khóa SIM khi đang gọi bằng lease + fencing token, health check và cooldown.               | sim_slot_id assigned      |
| 5        | GSM/SIM Call Execution        | Phát call script, bắt đầu DTMF capture. Ghi âm mặc định TẮT.                                             | attempt log               |
| 6        | DTMF Capture                  | Ghi phím 1/0/không bấm/sai phím/lỗi DTMF.                                                                | dtmf_key + raw result     |
| 7        | IVR Result Normalizer         | Chuẩn hóa thành result code, reason code, cờ tính lượt/kết thúc và evidence refs.                        | IVRResultNormalized       |
| 8        | Core Callback Adapter         | Gửi result về Order Core kèm correlation/idempotency/evidence.                                           | OrderCoreCallback         |
| 9        | Core Order State Machine      | Revalidate order và quyết định tiếp tục/hủy/hold/admin review.                                           | Core transition           |
| 10       | Audit / Evidence / Monitoring | Ghi log, dashboard, capacity incident, sim health, smoke evidence.                                       | Evidence accepted/pending |

# **7\. Entry Gate trước khi tạo IVR task**

| **Gate**            | **Điều kiện PASS**                                                                      | **Nếu FAIL**                                 |
| ------------------- | --------------------------------------------------------------------------------------- | -------------------------------------------- |
| Official Order Gate | Order đã là Official Order, có order_code, đến từ Customer Confirmation hợp lệ.         | Không tạo IVR task.                          |
| Order State Gate    | Order còn ở trạng thái được phép gọi; chưa cancel/expire/delivered/verified.            | Reject task hoặc mark stale.                 |
| Program Gate        | program_code xác định rõ: GOLDEN_HOUR hoặc TWENTY_FOUR_SEVEN hoặc policy khác đã khóa.  | Owner review / no dispatch.                  |
| Contact Gate        | Có phone_ref/dial_token hợp lệ; phone_validation_status PASS.                           | INVALID_PHONE hoặc admin review theo policy. |
| Block Gate          | Không sale lock, recall, quality hold, channel suppression, opt-out hoặc privacy block. | Không dispatch → IVR_POLICY_BLOCKED.         |
| Trust Gate          | Customer trust/risk resolver xác định cần gọi hoặc được gọi theo policy.                | Skip IVR hoặc route review theo policy.      |
| Capacity Gate       | Có SIM capacity đủ trong confirmation window.                                           | Capacity incident + admin alert.             |
| Evidence Gate       | correlation_id, idempotency_key, actor/system context sẵn sàng.                         | Reject task.                                 |

Nguyên tắc fail-closed: khi một nguồn dữ liệu của gate không đọc được, kết quả là KHÔNG gọi, không phải là gọi với giả định lạc quan. Điều này đã được thực thi trong code và có test âm.

# **8\. Runtime Object Contract**

| **Object**          | **Trường tối thiểu**                                                                                                                                                                 | **Ghi chú owner/boundary**                                            |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------- |
| ivr_task            | ivr_task_id, order_id, order_code, program_code, session_id, confirmation_window_start/end, priority, eligibility_snapshot_id, correlation_id, idempotency_key, status               | Tạo bởi Order Core / IVR intake, không nhận từ client public.         |
| ivr_call_job        | call_job_id, ivr_task_id, attempt_no, sim_slot_id, scheduled_at, deadline_at, started_at, ended_at, call_duration, ring_duration, status                                             | Scheduler quản lý; không tạo duplicate attempt.                       |
| sim_channel         | sim_slot_id, sim_number_ref, status, current_call_job_id, fail_count, last_health_check_at, cooldown_until, quarantine_until, disabled_reason, lease_token, lease_fencing_generation | Một SIM chỉ một active call, thực thi bằng row-lock và fencing token. |
| ivr_raw_call_event  | raw_event_id, call_job_id, provider_internal_payload_ref, dtmf_raw, audio_status, recording_ref, received_at                                                                         | Không lưu PII thô nếu không cần.                                      |
| ivr_result          | ivr_result_id, call_job_id, order_id, ivr_result_code, dtmf_key, failure_code, is_counted_customer_attempt, is_final_for_ivr, normalized_at, evidence_ref                            | Là signal, không phải state cuối.                                     |
| order_core_callback | callback_id, ivr_result_id, order_id, recommended_core_action, reason_code, order_core_ack, rejected_reason, correlation_id, idempotency_key                                         | Order Core quyết định accept/reject.                                  |
| capacity_incident   | incident_id, session_id, program_code, status, scope, hold_new_calls, active_sim_count, pending_call_jobs, expired_call_jobs, missed_deadline_count, shortage_reason, created_at     | Bắt buộc khi quá tải.                                                 |
| ivr_audit_evidence  | evidence_id, object_type, object_id, actor/system, action, timestamp, before/after, reason, file_refs/log_refs                                                                       | Ghi append-only.                                                      |

# **9\. IVR Eligibility Resolver**

Resolver đọc snapshot từ Order Core, Customer Trust, Official Contact, Operational Block và Program Policy. Resolver không được hardcode khách tin cậy, không tự bỏ qua recall/sale lock và không tự kéo dài window.

| **Input**                                  | **Nguồn**                        | **Rule xử lý**                                                                                                                |
| ------------------------------------------ | -------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| order_status                               | Order Core                       | Chỉ Official Order ở trạng thái cho phép IVR mới PASS.                                                                        |
| order_code                                 | Commerce Runtime                 | Bắt buộc có order_code. Không order_code thì không gọi.                                                                       |
| program_code                               | Commerce/Program Runtime         | GOLDEN_HOUR dùng window 5 phút; TWENTY_FOUR_SEVEN dùng window 15 phút. Xem §10.                                               |
| phone_ref / dial_token                     | Official Contact Resolver        | Chỉ dùng contact đã duyệt; UI/log chỉ dùng masked phone.                                                                      |
| risk evidence                              | Customer Trust Resolver          | Có thể skip IVR cho khách cũ đủ tin cậy nếu Sales gửi risk evidence có phiên bản. Không hardcode. Thiếu evidence thì vẫn gọi. |
| sale_lock / recall / suppression / opt-out | Operational Core / CRM / Gateway | Bất kỳ block nào active thì không dispatch → IVR_POLICY_BLOCKED.                                                              |
| quote_expiry / order_deadline              | Commerce Runtime                 | Không gọi nếu quote/order đã hết hiệu lực.                                                                                    |
| capacity_available                         | SIM Gateway Monitor              | Không nhận call job vượt capacity nếu chắc chắn miss deadline.                                                                |

# **10\. Attempt Policy và xung đột nguồn**

<div class="joplin-table-wrapper"><table><tbody><tr><th><p><strong>BỐN NGUỒN ĐANG MÂU THUẪN — CHƯA ĐÓNG</strong></p><ul><li>Nguồn A — tài liệu phase-8 business: Giờ Vàng 2 cuộc trong 10 phút; 24/7 3 cuộc trong 15 phút.</li><li>Nguồn B — PACK-09 IVR Input Baseline V1.0: Giờ Vàng 5 phút, 2 cuộc, cách 2 phút 30 giây; 24/7 15 phút, 2 cuộc, cách 7 phút 30 giây.</li><li>Nguồn C — code đang chạy: đúng theo nguồn B, dưới version mock-lab-v1, chỉ được phép ở chế độ MOCK và LAB.</li><li><strong>Nguồn D — contract submodule của Module 3</strong> (phát hiện 27/08/2026 qua review của M3): Giờ Vàng 2 cuộc, offsets <code>[0,300]</code>, window <strong>10 phút</strong>; 24/7 <strong>3 cuộc</strong>, offsets <code>[0,300,600]</code>, window 15 phút. Runtime backend của họ lại đang đặt <code>maxCalls=1</code>.</li><li><strong>Điểm nặng nhất:</strong> M3 nêu rằng <strong>không nguồn nào hiện có đỡ cho offsets <code>[0,150]</code></strong> — tức chính con số code IVR đang chạy. V0.3 vẫn giữ nguồn B làm rule triển khai, nhưng nay phải nói rõ nó không chỉ là "chưa ký", nó là "chưa nguồn nào xác nhận".</li><li>Đây vẫn là ĐỀ XUẤT, không phải quyết định: production sẽ từ chối version mock-lab-v1 cho tới khi owner ký một version khác. Với bốn nguồn mâu thuẫn thay vì ba, OD-V1-16 nặng thêm chứ không nhẹ đi. Xem B-04 và OD-V1-16.</li></ul></th></tr></tbody></table></div>

## **10.1. Rule chung**

- MAX_ATTEMPT_PER_ORDER = 2 (lượt gọi tính cho khách).
- ATTEMPT_INTERVAL = một nửa confirmation window.
- SIM_GATEWAY_DIRECT_ORDER_UPDATE = NO.
- ORDER_STATE_CHANGE_MUST_PASS_CORE_STATE_MACHINE = YES.
- Nếu cuộc 1 có kết quả cuối, không gọi cuộc 2.
- Lỗi kỹ thuật KHÔNG tiêu một lượt của khách; nó dùng quota technical retry riêng.

## **10.2. Bảng chương trình**

| **Chương trình** | **Window** | **Attempt 1** | **Attempt 2**  | **Hết hạn**  | **Offsets thực thi** |
| ---------------- | ---------- | ------------- | -------------- | ------------ | -------------------- |
| Giờ Vàng Tri Ân  | 5 phút     | T0            | T0 + 2 phút 30 | T0 + 5 phút  | 0s, 150s             |
| 24/7             | 15 phút    | T0            | T0 + 7 phút 30 | T0 + 15 phút | 0s, 450s             |

_Cột cuối là giá trị đang nạp trong registry attempt policy, để đối chiếu khi kiểm chứng._

## **10.3. Ánh xạ tình huống**

| **Tình huống**          | **IVR Result**                  | **Khuyến nghị gửi Order Core**                                                                                    |
| ----------------------- | ------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| Cuộc 1 nghe + bấm 1     | IVR_CONFIRMED                   | Revalidate rồi tiếp tục xử lý đơn.                                                                                |
| Cuộc 1 nghe + bấm 0     | IVR_CUSTOMER_CANCELLED          | Revalidate rồi hủy theo yêu cầu khách.                                                                            |
| Cuộc 1 không nghe       | IVR_NO_ANSWER_ATTEMPT           | Không đổi trạng thái. Scheduler gọi cuộc 2 theo interval.                                                         |
| Cuộc 2 nghe + bấm 1     | IVR_CONFIRMED                   | Revalidate rồi tiếp tục xử lý đơn.                                                                                |
| Cuộc 2 nghe + bấm 0     | IVR_CUSTOMER_CANCELLED          | Revalidate rồi hủy theo yêu cầu khách.                                                                            |
| Cuộc 2 không nghe       | IVR_NO_ANSWER_FINAL             | KHÔNG đổi trạng thái. IVR đóng phần việc của mình và chờ window hết hạn; Order Core tự xử lý theo timeout policy. |
| Hết window, khách ĐÃ được gọi ít nhất 1 lượt | IVR_CONFIRMATION_WINDOW_EXPIRED | Revalidate rồi expire. Khách đã có cơ hội xác nhận thật.                                                          |
| Hết window, khách CHƯA từng được gọi | IVR_CONFIRMATION_WINDOW_EXPIRED | KHÔNG tự expire. Giữ lại chờ admin review — đơn không được chết vì một cuộc gọi chưa từng phát sinh.               |
| Hết window vì đang xếp hàng chờ kênh, chưa dispatch lần nào | IVR_CAPACITY_EXCEPTION | Giữ lại chờ admin review, kèm capacity incident. Đây là ca DUY NHẤT được ghi là sự cố năng lực.                    |

**Khác biệt so với V0.2: V0.2 ghi rằng không nghe sau 2 cuộc thì IVR xin Order Core hủy đơn. Code không làm vậy và không nên làm vậy — không nghe máy không phải là ý chí hủy của khách. IVR chỉ báo cáo sự kiện; việc hết hạn mới là thứ dẫn tới hủy, và nó do Order Core quyết.**

# **11\. Hằng số runtime**

V0.2 chỉ nêu một ngưỡng số duy nhất, nên mọi giá trị khác nằm rải trong code mà không ai rà được. Bảng này gom đủ, kèm cột quan trọng nhất: giá trị đó đã được đo trên thiết bị thật chưa.

| **Hằng số**               | **Giá trị hiện tại**       | **Nguồn**            | **Đã đo thật chưa**              |
| ------------------------- | -------------------------- | -------------------- | -------------------------------- |
| MAX_ATTEMPT_PER_ORDER     | 2                          | PACK-09              | Chờ owner ký (B-04)              |
| Window Giờ Vàng           | 5 phút                     | PACK-09              | Chờ owner ký (B-04)              |
| Window 24/7               | 15 phút                    | PACK-09              | Chờ owner ký (B-04)              |
| Attempt offsets GH        | 0s, 150s                   | registry mock-lab-v1 | **KHÔNG NGUỒN NÀO ĐỠ** — M3 review 27/08 nêu không tài liệu nào hỗ trợ `[0,150]`. Xem §10 |
| Attempt offsets 24/7      | 0s, 450s                   | registry mock-lab-v1 | Chờ owner ký (B-04)              |
| SIM cooldown sau cuộc gọi | 5 giây                     | mặc định mock        | CHƯA — cần lab L-07              |
| Ngưỡng quarantine kênh    | fail_count ≥ 3             | mặc định mock        | CHƯA — cần lab L-08              |
| Ring timeout              | 30 giây                    | cấu hình adapter lab | CHƯA — phụ thuộc nhà mạng        |
| DTMF timeout              | **10 giây thực tế** (xem §11.1) | appsettings API + Worker | CHƯA — cần lab L-01, L-05        |
| AVERAGE_CALL_DURATION     | 35 giây                    | giả định V0.2 §11    | CHƯA — chưa có cuộc gọi thật nào |
| CONSERVATIVE_CALL_CYCLE   | 50 giây/cuộc/SIM           | giả định V0.2 §11    | CHƯA — chưa có cuộc gọi thật nào |
| Technical retry limit     | có giới hạn, cấu hình được | code normalizer      | Chưa chốt con số cuối            |
| Rate limit API            | CHƯA CÓ                    | —                    | Chưa duyệt ngưỡng (B-05)         |

## **11.1. DTMF timeout — bốn giá trị khác nhau, sửa 27/08/2026**

Bản V0.3 đầu tiên ghi "15 giây (lab đặt 60)". Sai ở con số đầu: 15 chỉ là **default trong code** của adapter Asterisk, và nó bị appsettings ghi đè xuống 10 ở cả API lẫn Worker. Giá trị thực tế đang chạy là **10 giây**, không phải 15.

| **Nơi đặt** | **Giá trị** | **Có hiệu lực khi** |
| ----------- | ----------- | ------------------- |
| `src/Ivr.Api/appsettings.json` | **10 giây** | API runtime — đây là giá trị thật |
| `src/Ivr.Worker/appsettings.json` | **10 giây** | Worker runtime — đây là giá trị thật |
| `AsteriskAriOptions.cs` (default code) | 15 giây | Chỉ khi không có appsettings nào ghi đè |
| `MockTelephonyDispatchGateway.cs` (default code) | 10 giây | Chế độ MOCK |
| `docker-compose.softphone.yml` | **60 giây** | **Chỉ lab softphone** — ghi đè bằng biến môi trường |
| Khoảng hợp lệ (cả hai adapter) | 1–120 giây | Ngoài khoảng này thì ứng dụng từ chối khởi động |

<div class="joplin-table-wrapper"><table><tbody><tr><th><p><strong>VÌ SAO CHÊNH LỆCH NÀY QUAN TRỌNG</strong></p><ul><li>Lab softphone đặt 60 giây vì người test cần thời gian nhìn màn hình rồi bấm phím. Đó là lựa chọn hợp lý cho lab, <strong>không phải lỗi</strong>.</li><li>Nhưng chính buổi lab đó là nơi sẽ đo ra thời lượng cuộc gọi thật để thay thế hai giả định 35 giây và 50 giây ở §14. Đo ở 60 giây rồi đem áp cho hệ chạy 10 giây là <strong>lệch 6 lần ở phần chờ phím</strong>, và mọi con số SIM suy ra từ đó sẽ bị thổi lên.</li><li>Trước khi lấy số đo lab đưa vào công thức §14.3, phải làm một trong hai: hạ DTMF timeout của lab về đúng giá trị production, hoặc trừ phần chờ phím dư ra khỏi thời lượng đo được. Ghi rõ đã làm cách nào vào biểu mẫu nghiệm thu lab.</li><li>Giá trị 10 giây tự nó cũng chưa được đo với khách thật — người lớn tuổi cầm điện thoại có thể cần lâu hơn. Đây là một trong các câu hỏi lab L-01/L-05 phải trả lời.</li></ul></th></tr></tbody></table></div>

**Hai dòng tô đậm cuối là nền của toàn bộ mô hình capacity ở §14. Chúng chưa từng được đo. Mọi con số SIM suy ra từ chúng đều là giả định, kể cả con số 12 cho pilot.**

# **12\. Call script, biến được phép và DTMF**

<div class="joplin-table-wrapper"><table><tbody><tr><th><p><strong>CALL PURPOSE</strong></p><ul><li>CALL_PURPOSE = ORDER_CONFIRMATION_ONLY.</li><li>IVR không bán thêm, không tư vấn sản phẩm, không đọc combo, không đọc chương trình khuyến mãi, không CRM, không mời thành viên/Diamond, không đọc địa chỉ đầy đủ và không đọc thông tin nhạy cảm.</li></ul></th></tr></tbody></table></div>

## **12.1. Mẫu lời gọi**

Ginsengfood kính chào Mình.

Ginsengfood cảm ơn Mình đã chọn Cháo Sâm Savigin.

Đơn hàng {{order_code_short}} của Mình có tổng thanh toán {{total_amount_display}}.

Để xác nhận tiếp tục xử lý đơn hàng, Mình vui lòng bấm phím 1.

Nếu Mình không đặt hoặc muốn hủy đơn này, Mình vui lòng bấm phím 0.

Ginsengfood trân trọng cảm ơn ạ.

## **12.2. Biến được phép đọc**

| **Biến**                          | **Ý nghĩa**                | **Trạng thái**             |
| --------------------------------- | -------------------------- | -------------------------- |
| order_code_short                  | Mã đơn rút gọn             | ALLOWED                    |
| total_amount_display              | Tổng thanh toán            | ALLOWED                    |
| customer_name_short               | Tên gọi ngắn nếu có        | OPTIONAL                   |
| program_name                      | Giờ Vàng / 24/7 nếu cần    | OPTIONAL                   |
| items\[\] (public_name, quantity) | Danh sách sản phẩm rút gọn | ĐANG TRANH CHẤP — OD-V1-15 |
| delivery_area_short               | Khu vực giao rút gọn       | ĐANG TRANH CHẤP — OD-V1-15 |

_Hai dòng cuối: bộ spec hẹp và bộ Target V1 đang mâu thuẫn. Mở rộng whitelist tự nó là một quyết định privacy, cần Privacy/Legal ký._

## **12.3. Thông tin cấm đọc**

| **Không được đọc**                    | **Lý do**                                                 |
| ------------------------------------- | --------------------------------------------------------- |
| FULL_ADDRESS                          | Bảo vệ PII; không cần để xác nhận ý chí đặt hàng.         |
| MEMBER_TIER / DIAMOND_REFERRAL_INFO   | Không biến IVR thành kênh member/Diamond/marketing.       |
| PAYMENT_DETAIL / ORDER_HISTORY        | Không đọc dữ liệu thanh toán/lịch sử mua không cần thiết. |
| AI_CONSULTATION_CONTENT / CRM_CONTENT | Không dùng IVR để tư vấn hoặc chăm sóc.                   |
| HEALTH_OR_SENSITIVE_NOTE              | Không đọc dữ liệu nhạy cảm.                               |

## **12.4. DTMF Key Rule**

| **Phím**  | **Ý nghĩa**                | **Hành động**                                                                    |
| --------- | -------------------------- | -------------------------------------------------------------------------------- |
| 1         | Khách xác nhận đơn         | IVR_CONFIRMED. Tính một lượt khách. Kết thúc IVR.                                |
| 0         | Khách không đặt / muốn hủy | IVR_CUSTOMER_CANCELLED. Tính một lượt khách. Kết thúc IVR.                       |
| Không bấm | Không có xác nhận hợp lệ   | Xử lý như không nghe theo attempt/window. Tính một lượt.                         |
| Sai phím  | Không có input hợp lệ      | IVR_WRONG_INPUT nếu còn lượt; nếu đã là lượt cuối thì thành IVR_NO_ANSWER_FINAL. |
| Lỗi DTMF  | Lỗi kỹ thuật               | IVR_TECHNICAL_EXCEPTION. KHÔNG tính là khách không nghe, KHÔNG tiêu lượt.        |
| 9         | Human support              | NOT_ENABLED. Rơi vào nhánh sai phím.                                             |

# **13\. Internal SIM Gateway Server**

## **13.1. Thành phần phần mềm**

| **Thành phần**              | **Vai trò**                                | **Yêu cầu kỹ thuật**                                             |
| --------------------------- | ------------------------------------------ | ---------------------------------------------------------------- |
| SIM Channel Manager         | Quản lý SIM rảnh/bận/lỗi/cooldown.         | Một SIM một active call; fail_count ≥ 3 thì quarantine và alert. |
| Call Job Queue              | Lưu call job theo deadline.                | Không batch cuối phiên; sort theo deadline/priority.             |
| Deadline-aware Scheduler    | Điều phối call job vào SIM rảnh.           | Ưu tiên đơn sắp hết window, Giờ Vàng, attempt 2, risk cao.       |
| Call Execution Adapter      | Thực hiện cuộc gọi qua GSM/SIM.            | Phát script, ghi trạng thái ringing/answered/completed.          |
| DTMF Capture Handler        | Bắt phím 1/0/sai phím/timeout.             | Ghi dtmf_key và raw outcome; lỗi DTMF là technical exception.    |
| Call Result Normalizer      | Chuẩn hóa result code.                     | Không để raw provider event đi thẳng vào Order Core.             |
| Order State Machine Adapter | Callback về Order Core.                    | Có idempotency/correlation; không tự đổi order state.            |
| Audit/Evidence Writer       | Ghi log/bằng chứng.                        | Append-only; mask PII; lưu evidence refs.                        |
| Admin Monitoring API        | Dashboard call job, SIM health, incidents. | RBAC; không cho admin fake result hoặc hủy đơn ngoài core.       |
| Capacity Incident Monitor   | Phát hiện nghẽn.                           | Tạo incident khi pending/expired/missed deadline vượt ngưỡng.    |

## **13.2. Yêu cầu thiết bị vật lý**

V0.2 nói "Internal SIM Gateway Server" nhưng không nêu một yêu cầu thiết bị nào, và đó chính là lý do hạng mục mua sắm đứng yên. Mục này nêu tiêu chí tối thiểu để một thiết bị dùng được.

| **#** | **Yêu cầu**                                                                                    | **Vì sao là điều kiện loại trừ**                                                                     |
| ----- | ---------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| 1     | Có API kiểm tra sức khỏe từng kênh, trả về được cờ trạng thái ghi âm để đọc ngược lại.         | Chính sách khóa ghi âm ở trạng thái TẮT. Không đọc ngược được thì không chứng minh được nó đang tắt. |
| 2     | Bảng mã kết thúc cuộc gọi (call disposition) phân biệt được đủ 11 giá trị ở §13.3. KHÔNG phải bảng result code ở §16. | Ánh xạ nhầm "khách bấm từ chối" thành "khách hủy đơn" là hủy đơn của khách không hề yêu cầu.         |
| 3     | DTMF hỗ trợ RFC 2833/4733; nêu rõ có bắt được phím trong lúc đang phát thoại (barge-in) không. | Không có barge-in thì cuộc gọi dài hơn, tỉ lệ khách cúp giữa chừng tăng.                             |
| 4     | Một SIM tại một thời điểm chỉ mang một cuộc gọi, hoặc nêu rõ nếu khác.                         | Toàn bộ mô hình lease/fencing và capacity dựa trên giả định này.                                     |
| 5     | Tắt được từng kênh qua API; nêu rõ hành vi khi kênh đang bận.                                  | Cần cho kill switch và cho việc thay SIM lỗi mà không dừng cả hệ thống.                              |
| 6     | Có CDR với mã tham chiếu cuộc gọi nối được sang attempt_id của IVR.                            | Không nối được thì mọi tranh chấp hóa đơn đều không giải quyết được.                                 |
| 7     | Nói SIP chuẩn, ưu tiên hơn SDK độc quyền.                                                      | Hệ thống đã có Asterisk; thiết bị SIP nối vào bằng trunk, tái dùng gần hết phần đã làm.              |

<div class="joplin-table-wrapper"><table><tbody><tr><th><p><strong>ĐƯỜNG TÍCH HỢP RẺ NHẤT</strong></p><ul><li>IVR → adapter Asterisk (đã có) → Asterisk → SIP trunk → GSM Gateway → SIM → nhà mạng.</li><li>Chỉ phần "SIP trunk → GSM Gateway" là mới. Không cần viết lại adapter từ đầu bằng SDK riêng của hãng.</li><li>Bộ resolve alias → số thật phải nằm NGOÀI process IVR, để giữ nguyên ràng buộc IVR không bao giờ cầm số điện thoại thô.</li></ul></th></tr></tbody></table></div>

## **13.3. Bảng call disposition — từ vựng để hỏi vendor**

Mục này thêm ngày 27/08/2026 để sửa một lỗi trỏ nhầm bảng. Bản V0.3 đầu tiên yêu cầu thiết bị "phân biệt được đủ 11 giá trị ở §16". Sai. §16 là bảng **result code của IVR** — thứ do phần mềm suy ra sau khi đã có kết quả cuộc gọi. Ít nhất 4 mã trong đó (IVR_POLICY_BLOCKED, IVR_OPERATIONAL_BLOCKED, IVR_CAPACITY_EXCEPTION, và phần lớn IVR_CONFIRMATION_WINDOW_EXPIRED) phát sinh **trước hoặc ngoài** cuộc gọi, nên thiết bị không bao giờ nhìn thấy chúng. Gửi §16 cho vendor sẽ nhận về câu trả lời "có" một cách hình thức.

_Nguyên nhân của nhầm lẫn: cả hai bảng tình cờ đều có đúng 11 dòng._

Cái thật sự cần hỏi là bảng dưới đây — đúng tập giá trị mà adapter IVR đang chờ nhận (`SimProviderDisposition` trong `src/Ivr.Domain/Ports/ProviderPorts.cs`). Nếu thiết bị không phân biệt được một dòng nào trong đây, dòng đó phải được nêu rõ trong hồ sơ năng lực trước khi ký.

| **#** | **Disposition** | **Nghĩa** | **IVR ánh xạ thành** | **Tính lượt khách** |
| ----- | --------------- | --------- | -------------------- | ------------------- |
| 1 | Answered | Khách nhấc máy. | Theo phím bấm: 1 → CONFIRMED, 0 → CUSTOMER_CANCELLED, không bấm → NO_ANSWER, sai phím → WRONG_INPUT | Có |
| 2 | RingTimeout | Đổ chuông hết giờ, không ai nghe. | NO_ANSWER_ATTEMPT / NO_ANSWER_FINAL | Có |
| 3 | Busy | Máy bận. | NO_ANSWER_ATTEMPT / NO_ANSWER_FINAL | Có |
| 4 | Rejected | **Khách chủ động bấm nút từ chối.** | NO_ANSWER + cờ cần review. **KHÔNG PHẢI hủy đơn.** | Có |
| 5 | Unreachable | Thuê bao không liên lạc được. | INVALID_PHONE_FINAL | Không |
| 6 | InvalidDestination | Số không tồn tại / sai định dạng. | INVALID_PHONE_FINAL | Không |
| 7 | Dropped | Cuộc gọi rớt giữa chừng. | TECHNICAL_EXCEPTION | Không |
| 8 | NetworkError | Lỗi mạng nhà mạng. | TECHNICAL_EXCEPTION | Không |
| 9 | SimError | Lỗi SIM / kênh. | TECHNICAL_EXCEPTION + quarantine kênh | Không |
| 10 | AudioError | Lỗi phát thoại. | TECHNICAL_EXCEPTION | Không |
| 11 | DtmfError | Lỗi bắt phím. | TECHNICAL_EXCEPTION | Không |

<div class="joplin-table-wrapper"><table><tbody><tr><th><p><strong>HAI DÒNG PHẢI HỎI KỸ NHẤT</strong></p><ul><li><strong>Dòng 4 (Rejected).</strong> Nhiều thiết bị gộp "khách bấm từ chối" chung với "không nghe máy" vào một mã duy nhất. Gộp thì chấp nhận được — cả hai đều ra NO_ANSWER. Nhưng nếu thiết bị lại gộp Rejected chung với Answered, hoặc trả một mã ngụ ý "khách từ chối đơn hàng", thì phải biết trước khi ký: ánh xạ nhầm dòng này thành hủy đơn là hủy đơn của khách không hề yêu cầu. Đây là M8-P0-013.</li><li><strong>Hộp thư thoại.</strong> Không có dòng riêng trong bảng trên, vì phần lớn thiết bị báo nó là Answered. Nếu vậy IVR sẽ phát script cho hộp thư và ghi nhận "khách đã nghe" — sai. Phải hỏi thiết bị có cờ phân biệt voicemail/AMD không, và nếu có thì cờ đó đọc ra sao. Đây là M8-P0-014.</li></ul></th></tr></tbody></table></div>


# **14\. Capacity và kế hoạch mở rộng**

<div class="joplin-table-wrapper"><table><tbody><tr><th><p><strong>CẢNH BÁO VỀ NGUỒN SỐ LIỆU</strong></p><ul><li>Hai hệ số nền — 35 giây mỗi cuộc và 50 giây mỗi chu kỳ — là GIẢ ĐỊNH THẬN TRỌNG, chưa từng đo trên thiết bị và mạng thật.</li><li>Mọi con số kênh dưới đây, kể cả 12 cho pilot, là hệ quả của hai giả định đó. Chúng phải được tính lại ngay sau khi lab đo được thời lượng cuộc gọi thật.</li><li>Số kênh cho pilot chưa được quyết định chính thức ở bất kỳ đâu. Con số 12 là khuyến nghị của tài liệu này, không phải kết luận đã đo.</li></ul></th></tr></tbody></table></div>

## **14.1. Năng lực theo số SIM (dưới giả định hiện tại)**

Công thức: `số cuộc = số SIM × (số giây trôi qua ÷ 50 giây mỗi chu kỳ)`. Cột nào cũng tính lại được bằng tay, để không ai phải tin bảng này.

| **Số SIM** | **Trong 5 phút trôi qua** | **Trong 15 phút trôi qua** | **Trong 45 phút trôi qua** |
| ---------- | ------------------------- | -------------------------- | -------------------------- |
| 12 SIM     | ~72 cuộc                  | ~216 cuộc                  | ~648 cuộc                  |
| 24 SIM     | ~144 cuộc                 | ~432 cuộc                  | ~1.296 cuộc                |
| 32 SIM     | ~192 cuộc                 | ~576 cuộc                  | ~1.728 cuộc                |

**Lưu ý cách đọc: đó là số CUỘC GỌI, không phải số ĐƠN. Với policy 2 lượt và tỉ lệ không nghe máy 30–40%, mỗi đơn tốn trung bình khoảng 1,4 cuộc. Nghĩa là 12 SIM phục vụ được khoảng 50 đơn trong 5 phút trôi qua, không phải 72.**

<div class="joplin-table-wrapper"><table><tbody><tr><th><p><strong>BA ĐƠN VỊ THỜI GIAN KHÁC NHAU — ĐỪNG GỘP</strong></p><ul><li><strong>Confirmation window (5 hoặc 15 phút).</strong> Thuộc về MỘT đơn. Đồng hồ bắt đầu chạy khi đơn đó cần xác nhận, và nó là hạn chót của riêng đơn đó. Đây là con số ở §10.</li><li><strong>Độ dài một phiên Giờ Vàng.</strong> Khoảng thời gian đơn LIÊN TỤC đổ vào. Nhiều đơn, mỗi đơn có window 5 phút riêng, gối lên nhau. Con số này <strong>chưa được chốt ở đâu</strong> — xem cảnh báo ngay dưới.</li><li><strong>Tốc độ đơn đổ vào.</strong> 800 đơn rải đều trong 45 phút và 800 đơn ập đến trong 5 phút là hai bài toán khác hẳn nhau, dù tổng số bằng nhau. Cái thứ hai là kịch bản M8-P0-009 ở §23, và nó PHẢI ra capacity incident.</li><li>Cột trong bảng trên là <strong>thời gian trôi qua</strong>, không phải độ dài phiên và cũng không phải confirmation window. Bản V0.3 đầu tiên viết cột thứ ba là "45 phút (rolling)" rồi ở §14.2 lại viết "cuộc/phiên", khiến người đọc tưởng hai thứ là một.</li></ul></th></tr></tbody></table></div>

## **14.2. Lộ trình theo giai đoạn**

| **Giai đoạn**    | **Khuyến nghị** | **Điều kiện**                                                                                |
| ---------------- | --------------- | -------------------------------------------------------------------------------------------- |
| Lab kỹ thuật     | 1 SIM           | Chạy đủ biểu mẫu nghiệm thu lab trước khi mua thêm. Đây là bước bắt buộc, không bỏ qua được. |
| Pilot kỹ thuật   | 12 SIM          | Chỉ sau khi lab 1 SIM PASS. Phục vụ ~50 đơn trong 5 phút trôi qua — là pilot, chưa chạy thật được. |
| Launch tháng 1–2 | 24–32 SIM       | Chạy thật nếu rolling queue ổn; 32 SIM đủ cho 800–1.200 cuộc **trải trong 45 phút** (năng lực ~1.728). Con số này KHÔNG đúng nếu 800 cuộc dồn vào 5 phút — xem cảnh báo dưới. |
| Tháng 3–4        | 64 SIM          | Nếu volume IVR tăng khoảng 100%, tức 1.600–2.400 cuộc trải trong 45 phút.                    |
| Tháng 5–6        | 96 SIM          | Nếu volume lên 2.400–3.600 cuộc trải trong 45 phút.                                          |

<div class="joplin-table-wrapper"><table><tbody><tr><th><p><strong>"PHIÊN" DÀI BAO NHIÊU — CHƯA AI CHỐT (M8-OD-C)</strong></p><ul><li>Từ "phiên" được dùng xuyên suốt §14 nhưng <strong>không được định nghĩa ở bất kỳ đâu</strong> trong tài liệu này, trong PACK-09, hay trong code. Không có hằng số nào tên session length.</li><li>Sửa ngày 27/08/2026: bản V0.3 đầu tiên chứa ba phát biểu không thể cùng đúng — §14.1 ghi 32 SIM làm được 192 cuộc trong 5 phút, §14.2 ghi 32 SIM "an toàn cho 800–1.200 cuộc/phiên", còn §23 lấy đúng "32 SIM nhận 800 job trong 5 phút" làm kịch bản QUÁ TẢI.</li><li>Ba câu đó chỉ hòa giải được nếu "phiên" nghĩa là <strong>45 phút</strong>: 32 × (2.700 ÷ 50) = 1.728 ≥ 1.200. Với phiên 15 phút chỉ được 576, với phiên 5 phút chỉ được 192 — đều không đủ.</li><li>Nhưng con số 45 phút cũng chưa có nguồn. Nó xuất hiện đúng một lần, ở tiêu đề cột bảng §14.1, không kèm căn cứ. V0.3 đã viết lại §14.2 theo giả định 45 phút để tài liệu tự nhất quán, <strong>nhưng đây là giả định, không phải quyết định</strong>.</li><li>Owner phải chốt hai con số trước khi ký đơn mua sắm: (1) một phiên Giờ Vàng kéo dài bao lâu, (2) trong phiên đó cao điểm có bao nhiêu đơn. Thiếu chúng thì mọi con số SIM ở §14 — kể cả 12 cho pilot — vẫn là phỏng đoán. Xem M8-OD-A và M8-OD-C ở §26.</li></ul></th></tr></tbody></table></div>

## **14.3. Công thức thay thế khi có số đo thật**

kênh_cần = trần(đơn_đỉnh_trong_window × lượt_gọi_trung_bình_mỗi_đơn × thời_lượng_một_cuộc_kể_cả_cooldown ÷ độ_dài_window × hệ_số_dự_phòng)

Ba đầu vào đầu tiên là câu hỏi business, không phải câu hỏi kỹ thuật. Chừng nào business chưa trả lời "một phiên Giờ Vàng cao điểm có bao nhiêu đơn", mọi con số kênh đều là phỏng đoán.

<div class="joplin-table-wrapper"><table><tbody><tr><th><p><strong>CAPACITY GUARD</strong></p><ul><li>Nếu chỉ có tối đa 32 SIM nhưng phát sinh 800–1.200 cuộc cần xử lý trong 5 phút thì không thể dồn cuối phiên.</li><li>Bắt buộc dùng rolling queue theo từng đơn ngay khi đơn cần xác nhận.</li><li>Nếu queue vượt năng lực, phải ghi IVR_CAPACITY_INCIDENT; không im lặng để đơn hết hạn mà không có log.</li></ul></th></tr></tbody></table></div>

# **15\. Scheduler và Queue Rule**

| **Rule ID** | **Rule**                                       | **FAIL IF**                                       |
| ----------- | ---------------------------------------------- | ------------------------------------------------- |
| M8-SCH-001  | BATCH_AFTER_SESSION_CALLING = PROHIBITED       | FAIL nếu dồn toàn bộ cuộc gọi cuối phiên.         |
| M8-SCH-002  | ROLLING_REAL_TIME_IVR = REQUIRED               | FAIL nếu đơn chỉ được gọi sau khi phiên kết thúc. |
| M8-SCH-003  | SCHEDULER_MODEL = DEADLINE_AWARE_ROLLING_QUEUE | FAIL nếu FIFO đơn giản làm trễ Giờ Vàng.          |
| M8-SCH-004  | ONE_SIM_ONE_ACTIVE_CALL = YES                  | FAIL nếu một SIM bị giao nhiều active call.       |
| M8-SCH-005  | SIM_COOLDOWN_AFTER_CALL = 5 giây (chờ đo lại)  | FAIL nếu không có cooldown và health check.       |
| M8-SCH-006  | FAILED_SIM_AUTO_DISABLE = YES                  | FAIL nếu SIM lỗi liên tục nhưng vẫn dispatch.     |

Thứ tự ưu tiên của scheduler:

1. Đơn sắp hết confirmation window.
2. Đơn thuộc Giờ Vàng.
3. Đơn có attempt 2 đúng hạn.
4. Đơn có risk cao theo Customer Trust / fraud policy.
5. Đơn còn đủ thời gian xử lý.

# **16\. Result Normalization — danh mục đầy đủ**

V0.2 liệt kê 9 mã. Thực tế có 11. Hai cột "Tính lượt" và "Kết thúc" là phần V0.2 thiếu hoàn toàn, nhưng lại là phần quyết định hành vi: chúng phân biệt một lần khách không nghe máy với một lần hệ thống tự hỏng.

| **Result Code**                 | **Ý nghĩa**                                          | **Tính lượt** | **Kết thúc** | **Khuyến nghị Core**                          |
| ------------------------------- | ---------------------------------------------------- | ------------- | ------------ | --------------------------------------------- |
| IVR_CONFIRMED                   | Khách bấm phím 1.                                    | Có            | Có           | Revalidate rồi tiếp tục.                      |
| IVR_CUSTOMER_CANCELLED          | Khách bấm phím 0.                                    | Có            | Có           | Revalidate rồi hủy theo yêu cầu khách.        |
| IVR_NO_ANSWER_ATTEMPT           | Một lượt không nghe, còn lượt.                       | Có            | Không        | Không đổi trạng thái.                         |
| IVR_NO_ANSWER_FINAL             | Không nghe sau lượt cuối.                            | Có            | Có           | Không đổi trạng thái; chờ hết window.         |
| IVR_WRONG_INPUT                 | Bấm phím không hợp lệ, còn lượt.                     | Có            | Không        | Không đổi trạng thái.                         |
| IVR_CONFIRMATION_WINDOW_EXPIRED | Hết window chưa có xác nhận hợp lệ.                  | Không         | Có           | Expire nếu khách đã được gọi; giữ lại chờ admin review nếu chưa. |
| IVR_INVALID_PHONE_FINAL         | Số không hợp lệ/không tồn tại.                       | Không         | Có           | Giữ lại chờ admin review.                     |
| IVR_TECHNICAL_EXCEPTION         | Lỗi SIM/server/DTMF/audio/callback/scheduler.        | Không         | Không        | Giữ lại chờ admin review hoặc retry kỹ thuật. |
| IVR_CAPACITY_EXCEPTION          | Xếp hàng chờ kênh tới hết window, chưa dispatch lần nào. MỚI so với V0.2. | Không         | Có           | Giữ lại chờ admin review + capacity incident. |
| IVR_OPERATIONAL_BLOCKED         | Bị chặn bởi ràng buộc vận hành. MỚI so với V0.2.     | Không         | Có           | Chặn theo ràng buộc vận hành.                 |
| IVR_POLICY_BLOCKED              | Bị chặn bởi policy, gồm cả opt-out. MỚI so với V0.2. | Không         | Có           | Không gọi. Tôn trọng opt-out.                 |

Về hai mã khi hết window — sửa ngày 27/08/2026: bản V0.3 đầu tiên ghi IVR_CONFIRMATION_WINDOW_EXPIRED có tính lượt khách và luôn dẫn tới expire. Cả hai đều sai. Hết window không phải là một cuộc gọi tới khách, nên nó không tiêu lượt nào. Và việc expire chỉ đúng khi khách đã thực sự được gọi ít nhất một lượt; nếu chưa từng gọi được ai thì đơn phải chờ người xem, vì để nó tự hết hạn là hủy đơn của một khách hàng chưa từng nghe chuông — đúng hình dạng thảm họa mà §18 tồn tại để ngăn.

Về ranh giới giữa hai mã: IVR_CAPACITY_EXCEPTION là một khẳng định về NGUYÊN NHÂN, nên nó chỉ được dùng khi nguyên nhân đó thực sự chứng minh được — job đã xếp hàng chờ kênh (READY_FOR_SCHEDULER) và chưa dispatch được lần nào cho tới hết window. Job bị giữ chờ admin review là quyết định vận hành có chủ đích; job dry-run theo định nghĩa không cần kênh nào. Trước ngày 27/08/2026 cả ba đều bị ghi là sự cố năng lực, làm nhiễu chính chỉ số missed_deadline dùng để trả lời M8-OD-A (pilot mua bao nhiêu SIM). Nay chỉ ca thứ nhất tạo capacity incident.

Về IVR_OPT_OUT: V0.2 liệt kê nó như một result code, nhưng điều đó ngụ ý đã có một cuộc gọi phát sinh rồi mới biết khách từ chối. Thực tế opt-out bị chặn ở tầng eligibility nên không bao giờ có cuộc gọi nào, và kết quả ghi nhận là IVR_POLICY_BLOCKED. Cách làm này đúng hơn và V0.3 ghi lại theo đúng hành vi.

<div class="joplin-table-wrapper"><table><tbody><tr><th><p><strong>BẤT BIẾN ĐƯỢC THỰC THI Ở TẦNG DỮ LIỆU</strong></p><ul><li>Kết quả kỹ thuật, capacity, operational và policy KHÔNG được đánh dấu là một lượt gọi của khách. Từ 27/08/2026 điều này được thực thi bằng CHECK constraint trên CẢ HAI bảng mang cột này: <code>ck_ivr_call_results_non_customer_not_counted</code> (W-0117) và <code>ck_ivr_call_attempts_non_customer_not_counted</code> (W-0118). Vi phạm làm lệnh ghi thất bại ngay cả khi writer không đi qua guard tầng domain. Bảng attempt là bảng quan trọng hơn trong hai: scheduler đếm chính cột này để quyết định khách còn được gọi thêm lượt nào không, nên một kết quả không-phải-của-khách bị tính ở đây không chỉ sai báo cáo — nó tiêu mất một trong hai lượt mà policy đã hứa với khách. Trước đó ràng buộc chỉ nằm ở <code>CallResultSnapshot.Create</code>, mà scheduler sweep thì không gọi hàm này — tức bất biến chỉ đúng theo thoả thuận giữa các writer, không đúng theo cấu trúc.</li><li>Kết quả không-nghe-máy không được kèm đề nghị đổi trạng thái đơn.</li><li>Một kết quả không-nghe-máy chưa phải cuối cùng thì không được đóng phần việc IVR.</li></ul></th></tr></tbody></table></div>

# **17\. Callback về Order Core và State Machine Boundary**

<div class="joplin-table-wrapper"><table><tbody><tr><th><p><strong>BOUNDARY KHÓA</strong></p><ul><li>IVR_CAN_TRIGGER_CANCEL_REQUEST = YES.</li><li>IVR_CAN_DIRECTLY_CANCEL_ORDER = NO.</li><li>SIM_GATEWAY_CAN_CANCEL_ORDER = NO.</li><li>CORE_ORDER_STATE_MACHINE_CANCEL_REQUIRED = YES.</li></ul></th></tr></tbody></table></div>

| **Trường callback**              | **Bắt buộc** | **Mô tả**                                                      |
| -------------------------------- | ------------ | -------------------------------------------------------------- |
| contract_version                 | YES          | Phiên bản hợp đồng, để hai bên phát hiện lệch.                 |
| callback_id                      | YES          | ID callback duy nhất.                                          |
| task_id                          | YES          | Task IVR tương ứng.                                            |
| order_id                         | YES          | Đơn chính thức liên quan.                                      |
| order_version_seen_by_ivr        | YES          | Phiên bản đơn mà IVR nhìn thấy, để Core phát hiện callback cũ. |
| result_type                      | YES          | Một trong 11 giá trị ở §16.                                    |
| result_reason                    | Nếu có       | Lý do chuẩn, đã lọc PII, tối đa 500 ký tự.                     |
| is_counted_customer_attempt      | YES          | Lượt này có tiêu quota của khách không.                        |
| is_final_for_ivr                 | YES          | IVR đã đóng phần việc chưa.                                    |
| attempt_number                   | YES          | Số thứ tự lượt gọi.                                            |
| occurred_at                      | YES          | Thời điểm phát sinh.                                           |
| recommended_core_action          | YES          | Chỉ là ĐỀ XUẤT; Order Core có quyền từ chối sau revalidation.  |
| evidence_ref                     | YES          | Tham chiếu bằng chứng.                                         |
| audit_ref                        | YES          | Tham chiếu audit.                                              |
| correlation_id / idempotency_key | YES          | Trace xuyên suốt và chống xử lý trùng.                         |

Order Core trả về mã xác nhận thuộc một tập đóng: chấp nhận, chấp nhận-trùng, bị chặn, cần review, từ chối vì cũ, xung đột idempotency. Hai mã đầu là hoàn tất; bốn mã còn lại chuyển sang review thủ công và KHÔNG được retry tự động.

# **18\. Technical Error Boundary**

Lỗi kỹ thuật tuyệt đối không được tính là khách không nghe. Đây là ràng buộc quan trọng nhất về mặt thương mại trong toàn bộ module: tính nhầm một lỗi hệ thống thành một lượt khách không nghe sẽ dẫn tới hủy đơn của một khách hàng chưa từng được gọi tới.

| **Lỗi kỹ thuật**        | **Không được xử lý như** | **Route đúng**                            |
| ----------------------- | ------------------------ | ----------------------------------------- |
| SIM_GATEWAY_ERROR       | CUSTOMER_NO_ANSWER       | IVR_TECHNICAL_EXCEPTION                   |
| SERVER_ERROR            | CUSTOMER_NO_ANSWER       | IVR_TECHNICAL_EXCEPTION                   |
| DTMF_CAPTURE_ERROR      | CUSTOMER_NO_ANSWER       | IVR_TECHNICAL_EXCEPTION                   |
| AUDIO_PLAYBACK_ERROR    | CUSTOMER_NO_ANSWER       | IVR_TECHNICAL_EXCEPTION                   |
| SIM_CHANNEL_FAILURE     | CUSTOMER_NO_ANSWER       | Quarantine kênh + admin alert             |
| INTERNAL_CALLBACK_ERROR | CUSTOMER_NO_ANSWER       | Retry callback có giới hạn + admin review |
| SCHEDULER_ERROR         | CUSTOMER_NO_ANSWER       | Capacity/technical incident               |

**Một trường hợp dễ ánh xạ sai và tốn tiền: khách chủ động bấm nút từ chối cuộc gọi. Đó KHÔNG phải ý chí hủy đơn — đó là không nghe máy, và có tính một lượt. Ánh xạ nó thành hủy là hủy đơn của khách không hề yêu cầu.**

# **19\. Admin Monitoring / Ops Dashboard**

| **Màn hình / API** | **Chức năng**                                                                     | **Không được làm**                   |
| ------------------ | --------------------------------------------------------------------------------- | ------------------------------------ |
| Dashboard          | Tổng quan call volume, success rate, queue depth, SIM health, capacity incidents. | Không cho sửa order state trực tiếp. |
| Queue / Call jobs  | Danh sách call job, trạng thái, attempt, deadline, result.                        | Không tạo gọi lại ngoài retry rule.  |
| Chi tiết call job  | Chi tiết task, attempts, callback, evidence refs.                                 | Không hiển thị số điện thoại đầy đủ. |
| SIM channels       | Theo dõi SIM rảnh/bận/lỗi/quarantine/cooldown.                                    | Không ép assign SIM đang bận.        |
| Capacity incidents | Nghẽn queue, miss deadline, shortage reason.                                      | Không xóa incident lịch sử.          |
| Review / Audit     | Audit/evidence theo object.                                                       | Không sửa evidence đã ghi.           |

## **19.1. Chỉ số theo dõi**

| **Chỉ số**               | **Ý nghĩa**                              | **Trạng thái**                                     |
| ------------------------ | ---------------------------------------- | -------------------------------------------------- |
| call_success_rate        | Tỷ lệ cuộc gọi kết nối thành công.       | Có                                                 |
| confirm_rate             | Tỷ lệ bấm 1 trên tổng task đủ điều kiện. | Có                                                 |
| cancel_rate              | Tỷ lệ bấm 0.                             | Có                                                 |
| no_answer_rate           | Tỷ lệ không nghe sau policy.             | Có                                                 |
| technical_exception_rate | Tỷ lệ lỗi kỹ thuật.                      | Có                                                 |
| missed_deadline_count    | Số đơn quá window chưa gọi.              | Có                                                 |
| sim_failure_rate         | SIM lỗi theo slot.                       | Có                                                 |
| cost_per_confirmed_order | Chi phí cuộc gọi trên mỗi đơn xác nhận.  | CỐ Ý CHƯA HIỆN — chưa có báo giá nhà cung cấp nào. |

Về chỉ số cuối: hiện một ô trống hay một số 0 đều tệ hơn không hiện. Cái thứ nhất trông như lỗi, cái thứ hai trông như miễn phí. Chỉ số này bật lên khi có báo giá thật, bằng cách thêm một hằng số chi phí có phiên bản vào cấu hình.

# **20\. Bảo mật, riêng tư và tuân thủ**

## **20.1. Nguyên tắc**

- Chỉ dùng phone_ref/dial_token để gọi; UI và log mặc định dùng phone đã che.
- IVR không bao giờ giữ mapping dial_token → số thật. Bộ resolve nằm ngoài process IVR.
- Không lưu số điện thoại đầy đủ ở các màn hình không cần thiết.
- Ghi âm mặc định TẮT. Bật lên là một quyết định pháp lý riêng, cần consent, retention và quyền nghe.
- Admin action phải có RBAC, actor context, reason, correlation_id và audit append-only.
- Opt-out và privacy block phải chặn dispatch trước khi phát sinh cuộc gọi.
- Release gate phải có smoke, security review, privacy review và owner sign-off.

## **20.2. Ràng buộc đã được thực thi trong code**

| **Ràng buộc**                           | **Cách thực thi**                                                                               |
| --------------------------------------- | ----------------------------------------------------------------------------------------------- |
| Số điện thoại thô không đi vào hệ thống | Kiểu `DialTokenReference` từ chối ở constructor, qua hai lớp: (1) toàn chuỗi có hình dạng số — 10–12 chữ số bắt đầu 0 hoặc 84, chấp nhận dấu cách, `-`, `.`, `()`, `+84`; (2) từ W-0119, quét thêm bằng `PiiGuard` để bắt số thô mang tiền tố hoặc hậu tố. Không lọt được `tel:09…`, `sip:09…@gw`, `PHONE_09…`, `09…@carrier`. Đo trên 10.000 ciphertext của bộ mã hoá: 0 false positive. |
| Không gọi khách thật trước release      | Đặt cờ cho phép gọi khách thành YES làm ứng dụng từ chối khởi động.                             |
| Ghi âm không bật nhầm                   | Cờ ghi âm là bất biến-tắt; API quản trị từ chối yêu cầu bật.                                    |
| Chỉ gọi số trong danh sách cho phép     | Cổng dispatch kiểm danh sách trước khi chạm nhà cung cấp.                                       |
| Nội dung thoại không rò ra log          | Đối tượng chứa lời thoại trả về chuỗi đã che khi ghi log.                                       |

## **20.3. Retention và DSAR — CHƯA CHỐT**

V0.2 chỉ nói "phải có retention policy" mà không nói giữ bao lâu, nên không ai thực hiện được. Bảng dưới liệt kê từng lớp dữ liệu cần một con số. Trạng thái hiện tại của cả bảng là OWNER_DATA_REQUIRED.

| **Lớp dữ liệu**        | **Thời hạn giữ**   | **Ghi chú**                                                    |
| ---------------------- | ------------------ | -------------------------------------------------------------- |
| ivr_task               | &lt;owner điền&gt; | Chứa tham chiếu đơn, không chứa số thô.                        |
| ivr_call_job / attempt | &lt;owner điền&gt; | Bằng chứng vận hành cho khiếu nại.                             |
| ivr_result             | &lt;owner điền&gt; | Bằng chứng ý chí khách hàng — nhiều khả năng cần giữ lâu nhất. |
| ivr_raw_call_event     | &lt;owner điền&gt; | Dữ liệu thô từ nhà cung cấp; nên giữ ngắn nhất.                |
| audit / evidence       | &lt;owner điền&gt; | Append-only; thời hạn phải khớp yêu cầu kiểm toán.             |
| recording (nếu bật)    | Không áp dụng      | Đang TẮT. Bật là quyết định pháp lý riêng.                     |

Cần chốt thêm: căn cứ pháp lý cho cuộc gọi giao dịch, hành vi khi khách yêu cầu không gọi nữa, và quy trình xóa theo yêu cầu. Gọi xác nhận đơn là cuộc gọi giao dịch chứ không phải quảng cáo, nhưng ngưỡng chống spam của nhà mạng vẫn áp dụng và cần rà cùng bộ phận pháp chế.

# **21\. Kết nối module liên quan**

| **Module**                    | **Module 8 consume gì**                                                                    | **Module 8 không được làm**                                      |
| ----------------------------- | ------------------------------------------------------------------------------------------ | ---------------------------------------------------------------- |
| Module 3 — Commerce Runtime   | Official Order, order_code, order state, payment/shipping public-safe, quote/order expiry. | Không tạo/hủy/confirm order trực tiếp; không set paid/verified.  |
| Module 4 — AI Advisor         | Context rằng đơn đang chờ IVR hoặc đã có IVR result public-safe.                           | Không để AI nói đã xác nhận khi Order Core chưa ack.             |
| Module 5 — Gateway/Messenger  | Status public-safe để nhắn khách theo guard.                                               | Không gửi notification tự động từ IVR. V1 khóa tắt notification. |
| Module 6 — Ads/ROAS           | Không dùng IVR result làm revenue.                                                         | Không tính ROAS từ IVR_CONFIRMED.                                |
| Module 7 — MC AI Live         | Live có thể nhắc đơn Giờ Vàng cần xác nhận trong thời gian quy định.                       | Live không tự xác nhận/hủy đơn qua IVR.                          |
| PACK-10 — Evidence/Completion | Evidence accepted, smoke pass, release gate.                                               | Không tự gọi production ready.                                   |

# **22\. Roadmap theo slice và trạng thái thật**

V0.2 liệt kê 8 slice nhưng không có cột trạng thái, vì lúc đó chưa có gì. Cột cuối là phần bổ sung của V0.3.

| **Slice** | **Tên**                  | **Mục tiêu**                                                                | **Trạng thái 26/08/2026**                  |
| --------- | ------------------------ | --------------------------------------------------------------------------- | ------------------------------------------ |
| M8.2A     | IVR Task Intake          | Nhận task từ Order Core, validate Official Order, correlation, idempotency. | XONG sau mock. Chờ contract thật (B-03).   |
| M8.2B     | Eligibility Resolver     | Kiểm program, contact, blocks, trust, window, capacity.                     | XONG sau mock, chặt hơn spec.              |
| M8.2C     | Scheduler & Queue        | Deadline-aware rolling queue, attempt policy.                               | XONG. Chờ owner ký policy (B-04).          |
| M8.2D     | SIM Gateway Adapter      | SIM pool, one SIM one active call, call execution.                          | Logic XONG. Adapter thật CHƯA CÓ (B-01).   |
| M8.2E     | DTMF & Result Normalizer | Capture phím 1/0, normalize, technical boundary.                            | XONG. Ánh xạ cần đối chiếu thiết bị thật.  |
| M8.2F     | Order Core Callback      | Gửi result signal kèm evidence.                                             | Contract XONG. Chưa nối Sales thật (B-03). |
| M8.2G     | Admin Monitoring         | Dashboard, call jobs, sim channels, incidents, audit.                       | XONG, có RBAC hai vai trò.                 |
| M8.2H     | Smoke / Evidence Pack    | Test nội bộ, fake SIM, DTMF, callback, capacity, security.                  | PASS ở mock/lab. Evidence thật CHƯA có.    |

# **23\. P0 Smoke Test Matrix**

| **Test ID** | **Kịch bản**                                     | **Kết quả PASS**                                              | **Trạng thái**           |
| ----------- | ------------------------------------------------ | ------------------------------------------------------------- | ------------------------ |
| M8-P0-001   | Tạo IVR task từ Quote/Cart/Order Draft.          | Bị reject; không dispatch call.                               | PASS (mock)              |
| M8-P0-002   | Official Order đủ điều kiện Giờ Vàng.            | Attempt 1 tại T0; attempt 2 tại T0+2:30; hết hạn T0+5:00.     | PASS (mock)              |
| M8-P0-003   | Official Order 24/7 đủ điều kiện.                | Attempt 1 tại T0; attempt 2 tại T0+7:30; hết hạn T0+15:00.    | PASS (mock)              |
| M8-P0-004   | Cuộc 1 bấm phím 1.                               | IVR_CONFIRMED; không có attempt 2.                            | PASS (mock + lab)        |
| M8-P0-005   | Cuộc 1 bấm phím 0.                               | IVR_CUSTOMER_CANCELLED; không có attempt 2.                   | PASS (mock + lab)        |
| M8-P0-006   | Không nghe sau 2 cuộc.                           | IVR_NO_ANSWER_FINAL; không xin đổi trạng thái.                | PASS (mock)              |
| M8-P0-007   | SIM/DTMF/server lỗi.                             | IVR_TECHNICAL_EXCEPTION; không tiêu lượt khách.               | PASS (mock)              |
| M8-P0-008   | Sale Lock/Recall active trước dispatch.          | Không gọi; task bị chặn.                                      | PASS (mock)              |
| M8-P0-009   | 32 SIM nhận 800 job DỒN trong 5 phút (năng lực 5 phút chỉ ~192). | Không batch; ghi capacity incident. Đây là kịch bản quá tải CÓ CHỦ Ý, không phải mức tải bình thường của một phiên — xem §14.1. | PASS (simulator) |
| M8-P0-010   | Duplicate callback.                              | Idempotency chặn double processing.                           | PASS (mock)              |
| M8-P0-011   | Admin cố sửa order state từ IVR dashboard.       | Bị chặn bởi RBAC/boundary.                                    | PASS (mock)              |
| M8-P0-012   | No evidence nhưng completion PASS.               | Fail Gate; không release.                                     | PASS (gate)              |
| M8-P0-013   | MỚI: khách bấm nút từ chối cuộc gọi.             | Ra không-nghe-máy có tính lượt, KHÔNG ra hủy đơn.             | CHƯA — cần thiết bị thật |
| M8-P0-014   | MỚI: cuộc gọi vào hộp thư thoại.                 | Không được coi là khách đã nghe và đã bấm.                    | CHƯA — cần thiết bị thật |
| M8-P0-015   | MỚI: kill switch bật trong lúc đang có cuộc gọi. | Cuộc tiếp theo bị chặn; hành vi cuộc đang chạy được ghi nhận. | CHƯA — cần thiết bị thật |
| M8-P0-016   | MỚI: quay số ngoài danh sách cho phép.           | IVR chặn TRƯỚC khi chạm nhà cung cấp.                         | CHƯA — cần thiết bị thật |

_Bốn kịch bản mới là những trường hợp mock không dựng được. Chúng cũng là những kịch bản có thể buộc sửa code, nên cần chạy sớm trong lịch lab._

# **24\. P0 Rules — MUST / MUST NOT**

| **P0 Rule**                       | **MUST**                                                             | **FAIL IF**                                                                                |
| --------------------------------- | -------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| P0-01 Internal SIM Gateway        | Dùng Internal SIM Gateway Server.                                    | Fail nếu dùng Cloud IVR/SIP Trunk/Voice Brandname làm mặc định khi chưa có owner decision. |
| P0-02 One SIM one active call     | Mỗi SIM chỉ xử lý một active call.                                   | Fail nếu scheduler giao trùng SIM.                                                         |
| P0-03 Program window              | Giờ Vàng 5 phút, 2 cuộc, cách 2:30; 24/7 15 phút, 2 cuộc, cách 7:30. | Fail nếu gọi dồn liên tục hoặc gọi sau khi hết window.                                     |
| P0-04 Key mapping                 | Phím 1 xác nhận; phím 0 hủy đơn.                                     | Fail nếu tự thêm phím 9 hoặc phím khác.                                                    |
| P0-05 Core State Machine          | Mọi tiếp tục/hủy phải qua Order Core.                                | Fail nếu SIM Gateway/IVR callback đổi order state trực tiếp.                               |
| P0-06 Technical error boundary    | Lỗi kỹ thuật vào IVR_TECHNICAL_EXCEPTION và không tiêu lượt khách.   | Fail nếu SIM lỗi nhưng đơn bị hủy như khách không nghe.                                    |
| P0-07 Rolling queue               | Dùng deadline-aware rolling queue.                                   | Fail nếu batch toàn bộ cuộc gọi cuối phiên.                                                |
| P0-08 No downstream dependency    | Downstream chưa phụ thuộc IVR result khi chưa release.               | Fail nếu Order/CRM/AI/Gateway dùng result production trước smoke.                          |
| P0-09 No release without evidence | Test/smoke/security/evidence là bắt buộc.                            | Fail nếu Completion Report PASS thiếu evidence.                                            |
| P0-10 No raw phone in IVR         | MỚI: IVR không bao giờ nhận, lưu hay log số điện thoại thô.          | Fail nếu bất kỳ số thô nào xuất hiện trong log, evidence hoặc database của IVR.            |
| P0-11 Recording off               | MỚI: ghi âm TẮT và trạng thái tắt phải đọc ngược lại được.           | Fail nếu không xác nhận được trạng thái ghi âm qua health check.                           |

# **25\. Done Gate / Fail Gate**

## **25.1. Done Gate**

| **Done Gate** | **Điều kiện PASS**                                             | **Trạng thái**          |
| ------------- | -------------------------------------------------------------- | ----------------------- |
| M8-DONE-001   | Có mô hình Internal SIM Gateway và SIM channel pool.           | ĐẠT (phần mềm)          |
| M8-DONE-002   | Có call script chuẩn và biến được phép.                        | ĐẠT, trừ OD-V1-15       |
| M8-DONE-003   | Có phím 1/phím 0, phím 9 không bật.                            | ĐẠT                     |
| M8-DONE-004   | Có rule Giờ Vàng và 24/7 đúng window/attempt.                  | ĐẠT, chờ chữ ký         |
| M8-DONE-005   | Có capacity baseline và incident rule.                         | ĐẠT, hệ số chưa đo      |
| M8-DONE-006   | Có Core State Machine boundary và callback contract.           | ĐẠT, contract còn DRAFT |
| M8-DONE-007   | Có technical exception boundary.                               | ĐẠT                     |
| M8-DONE-008   | Có Admin UI/Monitoring/Evidence nhưng không bypass Order Core. | ĐẠT                     |
| M8-DONE-009   | Có P0 smoke matrix pass bằng evidence thật.                    | CHƯA — mới có mock/lab  |
| M8-DONE-010   | Có owner sign-off trước khi cho phép gọi khách thật.           | CHƯA                    |
| M8-DONE-011   | MỚI: chín rào chắn ở §3 đều đóng bằng artifact thật.           | CHƯA — 3 rào ở mức CHẶN |

## **25.2. Fail Gate**

| **Fail Gate** | **Điều kiện FAIL**                                                                                 |
| ------------- | -------------------------------------------------------------------------------------------------- |
| M8-FAIL-001   | IVR gọi entity không phải Official Order.                                                          |
| M8-FAIL-002   | IVR hoặc SIM Gateway tự hủy/tự xác nhận/tự chuyển trạng thái đơn.                                  |
| M8-FAIL-003   | Payment selected, COD, Paid hoặc ORDER_VERIFIED bị xử lý bởi IVR.                                  |
| M8-FAIL-004   | Lỗi kỹ thuật bị tính là khách không nghe.                                                          |
| M8-FAIL-005   | Scheduler batch cuộc gọi cuối phiên hoặc miss deadline mà không có incident.                       |
| M8-FAIL-006   | Admin có thể sửa result giả hoặc hủy đơn ngoài Core.                                               |
| M8-FAIL-007   | Có PII nhạy cảm trong log/UI/call script.                                                          |
| M8-FAIL-008   | Tài liệu hoặc implementation tự gọi production ready khi chưa có evidence.                         |
| M8-FAIL-009   | MỚI: một rào chắn ở §3 được đóng bằng mock, bằng suy luận hoặc bằng lời hứa thay vì bằng artifact. |
| M8-FAIL-010   | MỚI: một gate an toàn bị nới ra để "test cho nhanh" mà không có owner decision.                    |

# **26\. Sổ quyết định còn mở**

V0.2 nêu 6 quyết định. Rà soát thực tế cho thấy còn nhiều hơn thế, và mỗi cái đều đang chặn một thứ cụ thể. Bảng dưới gom các quyết định có ảnh hưởng trực tiếp tới Module 8.

| **ID**        | **Câu hỏi owner cần chốt**                                                                      | **Chủ sở hữu**          | **Chặn cái gì**        |
| ------------- | ----------------------------------------------------------------------------------------------- | ----------------------- | ---------------------- |
| OD-V1-08 / 16 | Attempt policy cuối cùng dùng rule nào, khi ba nguồn đang mâu thuẫn?                            | Product + Order Core    | Production (B-04)      |
| OD-V1-09      | Giao thức lab 1 SIM, DTMF, disposition, danh sách số cho phép.                                  | Infra + vendor          | Lab (B-01)             |
| OD-V1-10      | Năng lực nhiều kênh, failover, caller ID, chi phí.                                              | Infra + Procurement     | Production (B-07)      |
| OD-V1-13      | ~~Giờ Vàng thanh toán online có thuộc phạm vi IVR không?~~ **ĐÃ CÓ NGUỒN 27/08/2026.** M3 dẫn Flow 04 (dòng 838–850) và Flow 05 (dòng 426–435): khóa `24_7+COD` và `GOLDEN_HOUR+ONLINE`, Giờ Vàng phải từ chối `COD_NOT_ALLOWED`. Bảng ép trong code IVR **khớp nghiệp vụ**. Còn lại chỉ là ký wire mapping, không phải quyết lại business. | Product/Business        | Hạ từ CHẶN xuống ký mapping (IR-06 §3.10 R3, §3.11) |
| OD-V1-14      | MỚI 27/08 (đã tồn tại trong sổ đăng ký nhưng thiếu ở bảng này): `ivr_confirmation_required` **không có nguồn business nào**. OpenAPI khai `enum:[true]` và DB ép `must be true`, tức cả hệ đang gate trên một field chưa ai định nghĩa. Nếu producer bên Sales không set field này thì **100% task bị từ chối 422 ngay ngày cắm thật**. | Product/Business + Sales Core | Tích hợp thật (B-03). Mức nghiêm trọng cao nhất bảng này. |
| OD-V1-15      | Whitelist biến đọc trong call script: bộ hẹp 4 biến hay bộ rộng có danh sách sản phẩm?          | Product + Privacy/Legal | Nghiệm thu nghiệp vụ   |
| OD-V1-17      | Một dial_token cho nhiều lượt gọi thì xử lý ra sao?                                             | Sales + Security        | Gọi thật (B-02)        |
| OD-V1-18      | Bộ resolve dial_token thành số thật đặt ở đâu?                                                  | Security + vendor       | Lab (B-02)             |
| OD-V1-19      | Chọn nhà cung cấp giọng nói, kèm rà soát dữ liệu cá nhân.                                       | Product + Privacy/Legal | Lab (B-09)             |
| OD-V1-11      | Căn cứ pháp lý, hành vi khi khách từ chối nhận cuộc gọi, thời hạn lưu trữ.                      | Legal + Privacy         | Gọi khách thật (§20.3) |
| OD-V1-12      | Ai có thẩm quyền cho phép pilot và ai được bấm kill switch.                                     | Release owner           | Production (B-08)      |
| M8-OD-A       | MỚI: pilot dùng bao nhiêu SIM, khi 12 chỉ phục vụ khoảng 50 đơn/phiên?                          | Owner + Business        | Mua sắm (§14)          |
| M8-OD-B       | MỚI: ngưỡng rate limit cho API.                                                                 | Ops                     | Production (B-05)      |
| M8-OD-C       | MỚI 27/08: một phiên Giờ Vàng kéo dài bao lâu? Từ "phiên" chưa được định nghĩa ở đâu, mà mọi con số SIM ở §14 đều tính theo nó. | Owner + Business        | Mua sắm (§14), chặn cả M8-OD-A |
| M8-OD-D       | MỚI 27/08: thiết bị có phân biệt được hộp thư thoại với khách nhấc máy không? Nếu không, IVR sẽ ghi nhận "khách đã nghe" cho một hộp thư. | Infra + vendor          | Lab (B-01), M8-P0-014  |

**Quy tắc bất biến: mock và fixture không bao giờ đóng một dòng nào trong bảng này. Mock chỉ cho phép code đi tiếp.**

_Bảng trên là bản **lọc** — chỉ những quyết định ảnh hưởng trực tiếp tới Module 8. Nguồn đầy đủ và có thẩm quyền là_ `specs/_review/open-decisions-register.md` _(21 mục tính đến 27/08/2026). Các mục OD-V1-01…07 là quyết định hợp đồng Target V1, theo dõi gộp ở rào B-03; OD-V1-20 (RBAC runtime-gate) trùng phạm vi với OD-V1-12; OD-V1-21 (provisioning GitLab) theo dõi ở B-08. Khi hai nơi lệch nhau, sổ đăng ký thắng._

# **27\. Kết luận khóa Module 8**

<div class="joplin-table-wrapper"><table><tbody><tr><th><p><strong>MODULE 8 — TRẠNG THÁI V0.3</strong></p><ul><li>MODULE 8 V0.3 = READY FOR OWNER / TECH LEAD / DEV REVIEW.</li><li>IMPLEMENTATION STATUS = IMPLEMENTATION_COMPLETE_BEHIND_MOCKS (khác V0.2, vốn ghi nhầm là NOT_STARTED).</li><li>IVR_GATE = BLOCKED.</li><li>REAL_CUSTOMER_CALL_ALLOWED = NO.</li><li>PRODUCTION_READY = NO.</li><li>DOWNSTREAM_IVR_DEPENDENCY_ALLOWED = NO cho tới khi evidence pass.</li><li>Ba rào chắn ở mức CHẶN là B-01 (thiết bị viễn thông), B-02 (bộ resolve số) và B-03 (hợp đồng Sales). Không cái nào giải quyết được bằng cách viết thêm code.</li></ul></th></tr></tbody></table></div>

Bước tiếp theo đúng thứ tự: đóng B-03 song song với mua thiết bị cho lab 1 SIM → chạy đủ biểu mẫu nghiệm thu lab → owner ký attempt policy và giọng đọc → quyết định số SIM cho pilot dựa trên số đo thật → smoke và evidence → owner sign-off → pilot giới hạn.

**Nói rõ cho dev: Module 8 không phải "viết code gọi điện". Đây là một service runtime có state, queue, scheduler, SIM capacity, DTMF, callback, audit, evidence và release gate. Phần đó đã làm gần xong. Phần còn lại không nằm trong tầm tay đội phát triển, và cách duy nhất để nó tiến là đưa ba rào chắn CHẶN ở §3 ra trước owner với ngày cam kết cụ thể.**