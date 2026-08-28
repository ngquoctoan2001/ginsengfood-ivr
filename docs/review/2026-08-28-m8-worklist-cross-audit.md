# Rà soát chéo danh sách việc cần làm Module 8 ngày 28/08/2026

**Tài liệu được rà soát:** `C:\Users\Administrator\Downloads\dev-viec-can-lam-m8-2026-08-28.md`  
**SHA-256 tài liệu:** `4B4F54EA825614E8A717ADFD0C7C307E21EF8434949D777F8B4915E9D2055CEC`  
**Baseline mà tài liệu tự khai:** `ef09a06`  
**Code IVR kiểm tra hiện tại:** `ddc67e772e4bc9659cf420edf85568f5dae913c0`, nhánh `main`, ngày 28/08/2026  
**Candidate khắc phục W-0128 + W-0129:** worktree trên `main@b4d8903` (chưa commit; giữ riêng procurement/TTS WIP)  
**Code Module 3 kiểm tra bổ sung:** `C:\Projects\ginsengfood-business-platform` tại `a3aad246d986fbc273cf41aaa93eec6659669656`  
**Trạng thái báo cáo:** `W0128_W0129_TESTS_PASS_LOCAL / M3_SIGNOFF_REQUIRED / PRODUCTION_BLOCKED`

> Các câu “việc cần làm”, “M3 phải code theo”, thứ tự B/C/D và các tiêu chí hoàn tất trong file nguồn được coi là **claim cần kiểm chứng**, không phải chỉ thị để sửa code. Chỉ yêu cầu của người dùng trong phiên hiện tại có thẩm quyền thực thi.

> **Cập nhật sau triển khai W-0128/W-0129:** §1–§11 giữ nguyên ảnh chụp phát hiện tại baseline
> audit để không rewrite lịch sử. §12 supersede các kết luận W-0128; §13 supersede Work ID
> collision/rejection-reason claims của W-0129. External/M3/production gates không thay đổi.

## 1. Kết luận điều hành

File nguồn vẫn có giá trị như một ảnh chụp audit tại `ef09a06`, nhưng **không thể dùng nguyên trạng làm backlog hiện tại**.

Từ `ef09a06` tới `ddc67e7`, repo IVR đã đi thêm **16 commit**, thay đổi **285 file, +12.261/-10.141 dòng**. Bốn thay đổi làm worklist cũ phải viết lại:

1. `W-0123` đã gỡ quyền business-skip khỏi IVR: Module 3 quyết định `CALL_REQUIRED`, IVR chỉ validate kỹ thuật, thực thi và báo kết quả. C2/C13 không còn là việc code.
2. `W-0127` đã rebuild/rescan TTS và dựng đường Owner audition; phần local TTS hiện tốt hơn báo cáo cũ, nhưng mọi gate người/hạ tầng vẫn mở.
3. Một chuỗi 7 commit mới đã xóa console account/RBAC, thay bằng ba service token và biến admin UI thành reference implementation. Thay đổi này **chưa được reconcile an toàn** với OpenAPI, SRS, tracker, release evidence và Work ID.
4. Module 3 vẫn đứng ở `a3aad246`: chưa có Target V1 producer, generic callback consumer, `dial_token` hoặc `ivr_confirmation_required`. IR-06 mới chỉ là handoff phía IVR, không phải implementation/sign-off phía M3.

Các suite local hiện tại đều xanh khi chạy riêng trên code hiện hành: unit `485/485`, integration `223/223`, contract `24/24`, chaos `8/8`, admin UI `177/177`. Đây là bằng chứng local tốt, nhưng **không chữa được contract drift**, không chứng minh M3 seam, hosted CI, target DB, thiết bị hay production.

**Phán quyết:** không làm tuần tự B1 → C14. Việc đầu tiên phải là remediation contract/governance cho auth/admin và Work ID; sau đó mới đóng procurement pack, refresh closure pack và triển khai seam M3.

## 2. Cách đọc trạng thái

| Trạng thái | Nghĩa |
| --- | --- |
| `CONFIRMED_OPEN` | Khoảng trống có thật ở code hiện tại. |
| `PARTLY_CORRECT` | Chẩn đoán đúng một phần; priority, evidence hoặc cách sửa phải đổi. |
| `STALE_DONE_LOCAL` | Đã xong trong IVR sau baseline; không làm lại. External gate có thể vẫn mở. |
| `STALE_REWRITE_REQUIRED` | Bản mô tả cũ không còn khớp kiến trúc hiện tại. |
| `DUPLICATE` | Trùng phạm vi với mục khác; phải gộp. |
| `OWNER_DECISION_REQUIRED` | Không được dev tự chốt bằng code. |
| `BLOCKED_EXTERNAL` | Thiếu hệ thống, credential, thiết bị, dữ liệu hoặc chữ ký bên ngoài. |
| `CONTRACT_DRIFT` | Code, OpenAPI, generated client hoặc tài liệu handoff đang mô tả khác nhau. |
| `TRACEABILITY_BROKEN` | Work ID/evidence/history không còn nối được tới thay đổi thật. |

## 3. Phạm vi và bằng chứng đã kiểm

### Đã kiểm trực tiếp

- Đọc toàn bộ file nguồn và toàn bộ phiên bản cũ của báo cáo này.
- Map corpus Markdown: 590 file, 653 link resolve được, 200 link chưa resolve, 2 title trùng, 16 anomaly tên/encoding và 126 orphan candidate. Số lượng unresolved không đồng nghĩa 200 lỗi thật; các anchor quan trọng được đọc trực tiếp.
- So sánh Git `ef09a06..ddc67e7`, đọc 9 commit phát sinh sau báo cáo cũ và kiểm tra dirty worktree.
- Đọc intake/eligibility, scheduler, callback, telephony, TTS, admin auth mới, admin routes/UI, OpenAPI, tracker, release board, IR-06, decisions log, Target V1 closure pack và procurement WIP.
- Kiểm tra repo Module 3 hiện tại và exact-search Target V1 producer/callback/token fields.
- Chạy lại toàn bộ bốn project test .NET, admin UI Vitest và capacity self-test.
- Kiểm chứng nguồn chính thức về lộ trình 2G/3G và trang hãng về VoLTE/model gateway.

### Chưa có bằng chứng để tuyên bố

- Không có M3 sign-off, shared environment, production auth profile, target DB, hosted CI hoặc CDC chạy giữa hai repo.
- Không có SIM gateway đã mua, vendor capability statement, PSTN/real-customer call, target-hardware benchmark hoặc measured production capacity.
- Không có Owner voice acceptance, Legal/Privacy, Security CVE disposition, internal mirror hay production media topology.
- Không kiểm chứng lại từng một trong 397 reference của file nguồn; thay vào đó kiểm tra các claim quyết định backlog và mọi vùng đã drift sau baseline.

## 4. Findings theo mức ưu tiên

### F-01 — P0: Runtime đã xóa account/session nhưng OpenAPI vẫn phát hành toàn bộ route cũ

Chuỗi commit `41f4b4d..900d699` xóa `ConsoleAccountEndpoints`, session handler, account DB tables, login UI và account/role/profile pages. Runtime hiện không map account route nào.

Nhưng [OpenAPI draft.21](../../specs/api/openapi/ivr-order-confirmation.v1.yaml) vẫn công bố:

- `/auth/sign-in`, `/auth/session`, `/auth/sign-out`;
- `/accounts`, `/accounts/{accountId}`, `/accounts/{accountId}:reset-password`;
- schema `ConsoleSession`, `ConsoleSignIn*`, account CRUD và mô tả bearer là “admin session”.

[Admin API SRS](../../specs/api/03-admin-api.md), [UI index](../../specs/ui/00-index.md) và [admin-ui README](../../admin-ui/README.md) cũng vẫn mô tả IVR-owned account/session/RBAC. Trong khi đó [IR-06 §4A](../../integration-requirements/06-module-3-api-handover.md) lại nói các route này đã bị xóa và M3 phải dùng service credentials.

Contract tests `24/24` vẫn xanh, chứng tỏ suite hiện tại kiểm schema/pin nhưng **không có route-parity gate** để bắt OpenAPI công bố endpoint không còn implementation.

**Kết luận:** đây là `CONTRACT_DRIFT` mức P0. Trước mọi feature mới phải chốt đây là breaking removal hay deprecation window, cập nhật OpenAPI/generated artifacts/portal/SRS cùng một lần và bổ sung test đối chiếu route runtime ↔ OpenAPI.

### F-02 — P0: Hai Work ID đã bị tái sử dụng và lịch sử W-0105 bị đứt

[Tracker](../../prompt/_execution/prompt-execution-tracker.md) ghi `NEXT_WORK_ID=W-0128`, `W-0122` là VieNeu-TTS và `W-0120` là lỗi globalization.

Tuy nhiên:

- Admin auth mới, migration `20260828040458_W0122DropConsoleAccounts` và IR-06 §4A đều tự ghi `W-0122`.
- Intake rejection reason mới trong `EligibilityRules`, `TaskIntakeService` và unit tests tự ghi `W-0120`.
- Plan/evidence `W-0105` bị xóa khỏi repo thay vì giữ `SUPERSEDED/HISTORICAL`, nhưng [gate-status.yaml](../release/gate-status.yaml) vẫn trỏ `W-0105` tới `docs/evidence/W-0105/README.md` không còn tồn tại.

**Kết luận:** `TRACEABILITY_BROKEN`. Đề xuất cấp `W-0128` cho admin/auth transition và `W-0129` cho rejection-reason refinement. Khôi phục dấu vết W-0105 dưới trạng thái lịch sử, không phục hồi tính năng.

Migration phải xử lý có điều kiện: nếu chưa chạy ở bất kỳ DB dùng chung nào thì có thể đổi tên/ID; nếu đã chạy thì giữ technical migration ID và ghi rõ mislabel alias trong tracker/evidence, không rewrite migration history.

### F-03 — P0: IR-06 admin handoff cần rotation và signed M3 boundary

Lượt audit đầu chỉ đọc policy handler và đã **bỏ sót** `InternalRequestGuard.RequireAdminActor`
trong handler của từng endpoint. Đọc lại direct call sites xác nhận claim IR-06 là đúng: actor được
enforce trên 29/31 operation; chỉ hai GET feature-flag không cần actor. Đây là correction factual,
không phải thay đổi code để làm report đúng.

Hai gap thật còn lại tại baseline audit:

- `X-Script-Permissions` do caller khai; chỉ an toàn nếu M3 BFF giữ token/header và browser không
  được tự viết.
- Ba token tĩnh chưa có dual-token overlap/retirement và chưa có signed role→tier mapping/custody.

W-0128 đã bổ sung rotation hữu hạn, browser isolation docs/tests và fail-closed mapping contract.
M3 sign-off/implementation vẫn chưa có, nên IR-06 là `LOCAL_CONTRACT_READY`, chưa phải shared
integration contract đã nghiệm thu.

### F-04 — P1: B3 Admin UI đã đổi ownership; đề xuất “thêm hai màn vào IVR console” không còn dùng nguyên trạng

File nguồn đếm 12 trang và đề nghị thêm audit-evidence/capacity-incidents. Hiện accounts/roles/profile/login đã bị xóa; admin UI được commit với mục tiêu reference implementation để M3 dựng lại.

Hai capability lịch sử incident và object-level audit vẫn chưa có route/page đầy đủ, nhưng nơi đặt chúng nay là quyết định kiến trúc:

- nếu M3 là operator console thật, M3 UI phải own UX/session và gọi IVR service API;
- IVR có thể vẫn cần read API backend, nhưng không mặc định phải thêm page production trong repo IVR;
- reference UI chỉ nên làm contract example, không được đồng thời bị tài liệu release gọi là production admin app.

**Kết luận:** B3 là `STALE_REWRITE_REQUIRED`, không phải ticket frontend làm ngay.

### F-05 — P1: W-0123 đã xong local; C2/C13 không được làm lại

Active `CanSkip`, `ReturningCustomerSkipEnabled` và write path `TASK_SKIPPED_TRUSTED_CUSTOMER` không còn. Runtime chỉ đọc trust metadata ở mức `LEGACY_READ`; enum/DB value được giữ để rolling compatibility và history.

**Kết luận:** C2/C13 là `STALE_DONE_LOCAL`. Không xây CustomerTrustResolver trong IVR, không khôi phục business skip, không drop legacy data cho tới khi M3 usage/target DB/rollback evidence có thật.

### F-06 — P1: Shape M8 là candidate tốt, không phải seam đã được hai bên ký

Module 3 vẫn ở `a3aad246`; main source chỉ có Golden Hour queue/callback tương thích cũ, `maxCalls=1`, program wire `24_7` và raw `phoneNumber`. Exact-search vẫn không thấy generic `/ivr-result-callbacks`, `dial_token`, `ivr_confirmation_required` hay Target V1 producer.

**Kết luận:** C1/C7/C8/C12 vẫn mở. IR-06/OpenAPI phía IVR là input để ký và build, không phải bằng chứng M3 đã nhận authority/contract.

### F-07 — P1: Attempt policy vẫn chưa production-approved; procurement draft lại hard-code “2 lần”

IVR registry hiện có candidate hai attempt `[0,150]` và `[0,450]`; M3 current queue vẫn `maxCalls=1`; [T-09](../contracts/target-v1-closure-pack/T-09-attempt-policy.md) vẫn `OPEN` và còn ví dụ 3 attempt.

Trong khi đó [R-00 dòng 75](../contracts/telephony-procurement-pack/R-00-voice-gateway-rfq.md) viết cho vendor rằng mỗi khách tối đa 2 lần, rồi chính [dòng 142](../contracts/telephony-procurement-pack/R-00-voice-gateway-rfq.md) thừa nhận attempt policy chưa ký.

**Kết luận:** D6 không phải “đã đạt”. Xóa claim “tối đa 2 lần” khỏi phần vendor-facing hoặc ghi rõ đó chỉ là test assumption; vendor chỉ cần trả disposition/counting capability, không cần bị đóng khung bởi policy chưa ký.

### F-08 — P1: Capacity chưa calibrate; tờ trình 4 kênh đang biến giả định thành quyết định mua

Capacity self-test hiện chạy cả 800 và 1.200 daily orders, trả `PASS_UNCALIBRATED`, modelled peak 21 channel và range 7..72. Exact acceptance `32 channels / 800 jobs / 5 minutes → incident` vẫn chưa có.

[R-06](../contracts/telephony-procurement-pack/R-06-to-trinh-mua-thiet-bi.md) chốt mua gateway 4 kênh và lập luận 4 kênh cần để đo concurrent calls. Nhưng procurement README vẫn ghi chưa chốt pilot channel count; chưa có báo giá, session-arrival profile hay measured call duration để chứng minh 4 là lựa chọn kinh tế tối ưu.

**Kết luận:** 4 kênh có thể là đề xuất lab hợp lý, nhưng phải được trình như một option có owner approval và quote comparison, không phải fact đã được capacity model chứng minh.

### F-09 — P1: Hồ sơ procurement có mốc 3G sai và mô tả 2G quá tuyệt đối

R-00/R-06 ghi 2G tắt toàn quốc `15/09/2026` và 3G tắt `30/09/2026`. Nguồn chính thức của Bộ TT&TT nêu 3G được duy trì tới **tháng 9/2028**, không phải 2026: [MIC English](https://english.mic.gov.vn/3g-networks-to-be-decommissioned-in-vietnam-by-2028-197240729165325049.htm) và [dự thảo/quy hoạch băng tần 2100 MHz](https://cspl.mic.gov.vn/Pages/TinTuc/tinchitiet.aspx?tintucid=138844).

Đối với 2G, nguồn Bộ mô tả giai đoạn chuyển tiếp đến tháng 9/2026 cho thuê bao 3G/4G non-VoLTE, trong khi 2G-only đã có lộ trình riêng và từng được lùi từ 15/09 sang 15/10/2024: [MIC về giai đoạn chuyển tiếp](https://english.mic.gov.vn/viet-nam-to-shut-down-2g-network-from-mid-september-197240708084755349.htm), [MIC về việc lùi mốc 2024](https://mic.gov.vn/lui-thoi-diem-tat-song-2g-den-ngay-15-10-197240913224659081.htm). Vì vậy câu “thiết bị 3G ngừng hoạt động hoàn toàn trong một tháng” là sai; câu 2G cũng cần dẫn đúng văn bản/mốc áp dụng thay vì viết tuyệt đối.

Yêu cầu VoLTE vẫn hợp lý. Trang hãng xác nhận Yeastar TG series có VoLTE khi lắp đúng LTE module và Dinstar UC2000-VE có biến thể VoLTE, nhưng phải chốt đúng SKU/band/operator profile: [Yeastar TG guide](https://help.yeastar.com/download/docs/tg200-tg400-tg800-tg1600-user-guide-en.pdf), [Dinstar UC2000-VE](https://www.dinstar.com/GSM-3G-LTE-voip-gateway/4-8-ports/).

### F-10 — P1: C10/C14 đúng về freshness gap nhưng phải thiết kế lifecycle trước taxonomy

Runtime vẫn không có business revoke/update command và scheduler claim không refresh operational decision giữa attempts. Khoảng trống là thật.

Nhưng `IVR_TASK_CANCELLED_BY_BLOCK` không được tự thêm như customer call result. Revoke là control/lifecycle command từ M3; cần ký idempotency, state transition, ACK, race/fencing, active-call behavior và audit trước. Chỉ thêm result enum nếu registry owner quyết định đây thực sự là wire outcome.

**Kết luận:** gộp C10+C14 thành một item `OWNER_DECISION_REQUIRED / DESIGN_UNAPPROVED`.

### F-11 — P2: C9 vẫn suy diễn Rejected thành opt-out quá sớm

`Rejected` hiện map thành `NO_ANSWER + REJECTED_REVIEW_REQUIRED`. `OptOutSuppressionPolicy` và `QueueOnlySuppressionProposer` đã có cùng persistence tests, nhưng không có runtime counter/caller hay CRM egress. Registry do-not-call thuộc CRM/M3, không thuộc IVR.

**Kết luận:** giữ deferred cho tới khi Legal/Privacy + CRM/M3 ký explicit-signal semantics, threshold, identity key, retention và feedback contract. Không tự tạo `IVR_OPT_OUT` chỉ từ nút từ chối cuộc gọi.

### F-12 — P2: B2 TTS đã tiến thêm nhưng vẫn hoàn toàn bị chặn ngoài local

W-0127 đã rebuild image mới, rerun Trivy (`13 HIGH`, `3 CRITICAL`, `0 fixable` tại thời điểm scan), đo reachability, probe audition profile và thêm script sinh manifest fail-closed. Vì vậy câu báo cáo cũ “image stale cần rebuild” đã hết hạn.

Nhưng Owner chưa nghe/ký, fixed catalog 12 file chưa duyệt, 6 MicroSIP calls/retention/rollback chưa chạy, target hardware/internal mirror/topology chưa có và Legal/Security chưa ký.

**Kết luận:** B2 = `LOCAL_TESTS_PASS / BLOCKED_EXTERNAL`; không cần thêm C# hoặc tự bật segmentation.

### F-13 — P2: Local tests hiện xanh, nhưng traceability giảm và không thay thế release evidence

Kết quả current run:

- unit `485/485`;
- integration `223/223`;
- contract `24/24`;
- chaos `8/8`;
- admin UI `177/177`;
- traceability document ghi `456` tagged tests.

Số test giảm so với báo cáo cũ vì account/session suite và UI auth tests đã bị xóa. Green suite chứng minh code hiện hành tự nhất quán theo tests còn lại; nó không chứng minh các route OpenAPI cũ có runtime implementation, cũng không chứng minh decision xóa coverage đó đã được owner phê duyệt.

## 5. Đánh giá lại từng mục B/C

| Mục | Phán quyết hiện tại | Đánh giá / hành động đúng |
| --- | --- | --- |
| **B1 Capacity** | `PARTLY_CORRECT / OWNER_DATA_REQUIRED` | Model đã có 800 và 1.200, nhưng chưa calibrate và thiếu exact 32/800/5m test. Không dùng để chốt mua trước W-0008 measurement. |
| **B2 VieNeu-TTS** | `LOCAL_TESTS_PASS / BLOCKED_EXTERNAL` | W-0127 đã xử lý stale rebuild và làm rõ từng external action. Owner/Legal/Security/Infra/hardware/call evidence vẫn mở. |
| **B3 Admin UI/Monitoring** | `STALE_REWRITE_REQUIRED` | UI đã thành reference implementation; account/login/roles/profile bị xóa. Chốt M3 UI ownership và admin service contract trước khi quyết định thêm audit/capacity surface ở đâu. |
| **B4 Production telephony** | `CONFIRMED_OPEN / BLOCKED_EXTERNAL` | Production branch vẫn unavailable. Reuse PostgreSQL dispatch store; chỉ implement production `ISimGateway`, token-resolver client và wiring sau vendor/trust-boundary sign-off. Sửa procurement facts trước. |
| **C1 program_code** | `CONFIRMED_OPEN` | IVR `TWENTY_FOUR_SEVEN`, M3 `24_7`; cần assembler mapping + signed provider/consumer test. |
| **C2 trust resolver** | `STALE_DONE_LOCAL` | W-0123 đã chuyển authority về M3; không xây resolver trong IVR. |
| **C3 result taxonomy** | `PARTLY_STALE` | IVR candidate có 11 values; M3 generic consumer chưa có. Ký exact semantics/schema trước migration hoặc rename. |
| **C4 session_id** | `CONFIRMED_OPEN / OWNER_DECISION_REQUIRED` | Intake chưa có upstream session ref; M3 có GH session `Long` riêng. Ký field optional cross-program, type/nullability/ownership. |
| **C5 capacity session** | `DUPLICATE C4` | Cùng một contract/propagation change; không tạo item riêng. |
| **C6 task shape/session/priority** | `PARTLY_CORRECT / DUPLICATE C4` | `eligibility_snapshot` object+hash và IVR-derived priority không phải missing mặc định. Chỉ session trace còn cần quyết định additive. |
| **C7 callback shape** | `CANDIDATE_SHAPE_VERIFIED` | Route/header/body/ACK của IVR là candidate cụ thể; M3 chưa ký/chưa build. OpenAPI source, không phải generated `.g.cs`, phải là authority. |
| **C8 callback enable** | `CONFIRMED_OPEN / BLOCKED_EXTERNAL` | Provider vẫn fake/disabled và real guard fail-closed. Chỉ mở sau endpoint M3, auth, credential, network policy và shared tests. |
| **C9 opt-out** | `OWNER_DECISION_REQUIRED / DEFERRED` | Queue-only proposal đã có nhưng runtime feedback loop chưa có. Không coi Rejected là explicit opt-out. |
| **C10 revoke/re-check** | `CONFIRMED_GAP / DESIGN_UNAPPROVED` | Thiết kế lifecycle command/ACK/race trước; không tự thêm call-result code. |
| **C11 M2 recall/sale-lock** | `OWNER_DECISION_REQUIRED` | OD-17 giữ M3 revalidate là lưới cuối. Owner chọn chấp nhận stale call hoặc yêu cầu M3 revoke/update. Không phải IVR tự gọi M2. |
| **C12 contact/dial token** | `CONFIRMED_OPEN / BLOCKED_EXTERNAL` | Validation tốt; production issuer/resolver/vault chưa có. Không lưu raw phone trong IVR. |
| **C13 W-0123 cleanup** | `STALE_DONE_LOCAL` | Đã xong; chỉ external M3/DB/CI/integration gates còn mở. |
| **C14 revoke/re-check** | `DUPLICATE C10` | Gộp thành một item. |

## 6. Đánh giá lại nhóm D “đã đạt”

| Mục | Kết luận hiện tại | Ghi chú |
| --- | --- | --- |
| **D1 Intake endpoint/auth** | `LOCAL_IMPLEMENTATION_PRESENT` | Endpoint/header/closed schema/idempotency có thật; reason code mới chi tiết hơn. M3 Target V1 producer/shared E2E chưa có. |
| **D2 Official Order Gate** | `LOCAL_IMPLEMENTATION_PRESENT` | IVR chặn quote/cart/draft và yêu cầu `CONFIRMING`; M3 current đã có `CONFIRMING`, nên ghi chú cũ “M3 chưa có” là sai. |
| **D3 Entry gates** | `PARTLY_PROVEN_LOCAL` | Nhiều gate fail-closed có thật; operational truth không được refresh giữa attempts và business authority đã chuyển về M3. |
| **D4 Callback idempotency/retry/DLQ** | `LOCAL_IMPLEMENTATION_PRESENT` | Outbox/lease/retry/review có thật; real M3 consumer/auth/traffic chưa có. |
| **D5 Không tự set payment/order state** | `VERIFIED_STATIC` | Boundary đúng; callback advisory và Core revalidate. |
| **D6 Attempt policy** | `NOT_OWNER_APPROVED` | Candidate hai attempt có code/test; M3 current là một, closure pack còn mâu thuẫn. Không được dùng làm production/vendor fact. |
| **D7 Secrets/auth** | `MIXED / CONTRACT_DRIFT` | Repo không lộ secret thật và blank token fail-closed. Nhưng admin auth mới dùng static tier tokens, thiếu signed M3 mapping/rotation và OpenAPI vẫn mô tả session auth cũ. |
| **D8 Tests/coverage** | `FULL_LOCAL_TEST_PROJECTS_PASS / EXTERNAL_NOT_PROVEN` | 740 .NET tests + 177 UI tests xanh. Hosted CI, shared M3-M8 E2E, target DB, real telephony và production vẫn `NOT_RUN`. |

## 7. Conflict giữa code, plans và tài liệu hiện tại

| Conflict | Bằng chứng | Hậu quả |
| --- | --- | --- |
| Runtime xóa account route, OpenAPI vẫn công bố | `src/Ivr.Api/Accounts/**` đã xóa; OpenAPI draft.21 vẫn có `/auth/*`, `/accounts*` | Generated client gọi route chết; contract test không bắt. |
| IR-06 nói actor bắt buộc 29/31, code chỉ enforce danger | IR-06 §4A vs `AdminScopeAuthorizationHandler`/`AdminScopeGuard` | M3 client dễ gửi/diễn giải sai 403; audit identity không nhất quán. |
| `W-0122` vừa là TTS vừa là admin auth/migration | Tracker vs comments/migration/IR-06 | Evidence/release attribution sai. |
| `W-0120` vừa là globalization vừa là intake reason refinement | Tracker vs source/test comments | Không xác định acceptance/evidence của commit mới. |
| W-0105 bị xóa nhưng release gate vẫn trỏ evidence cũ | `gate-status.yaml` vs file không tồn tại | Readiness board có dangling evidence và lịch sử bị xóa. |
| Admin UI README/SRS vẫn nói IVR-owned sessions/RBAC | README/specs vs runtime/IR-06 | Kiến trúc và vận hành mâu thuẫn trực tiếp. |
| Target V1 closure pack vẫn baseline draft.18 | README/T-02/T-07 | Còn `sellable_status`, mock admin auth và đường dẫn đã xóa; không dùng để giao M3 trước khi refresh draft.21+. |
| Procurement R-00 hard-code 2 attempts | R-00 dòng 75 vs T-09/M3 `maxCalls=1` | Vendor document biến policy chưa ký thành fact. |
| Procurement R-06 chốt 4 kênh, README nói chưa chốt | R-06 vs procurement README | Tờ trình mua trước khi có quote/capacity inputs. |
| Procurement ghi 3G tắt 30/09/2026 | R-00/R-06 vs nguồn MIC tháng 9/2028 | Lý do mua chứa fact sai, rủi ro quyết định chi tiêu. |
| IR-06/OpenAPI phía IVR có, M3 main chưa có Target V1 | IVR `ddc67e7` vs M3 `a3aad246` | Seam vẫn `NOT_BUILT_UPSTREAM`, không phải done. |

## 8. Worklist hiệu chỉnh để tiến hành khắc phục

### Gate 0 — sửa contract/governance trước mọi feature code

1. **W-0128 — Admin/auth transition remediation**
   - ghi quyết định owner rằng M3 sở hữu operator identity và IVR admin UI chỉ là reference;
   - xác định compatibility/deprecation cho route account/session đã xóa;
   - đồng bộ runtime routes, OpenAPI, generated contract, portal, SRS, UI README, IR-06 và release docs;
   - sửa claim actor-header cho đúng enforcement hoặc sửa enforcement theo signed contract;
   - chốt token custody, M3 role→tier mapping, rotation overlap/retirement, browser isolation và audit identity;
   - khôi phục W-0105 như `SUPERSEDED/HISTORICAL` và sửa dangling release evidence;
   - xử lý migration ID theo việc nó đã/ chưa chạy ở môi trường dùng chung.
2. **W-0129 — Intake rejection reason traceability**
   - thay mọi comment `W-0120` bằng ID đúng;
   - ghi baseline, acceptance, reason-code taxonomy, compatibility và test evidence;
   - xác nhận đây chỉ là chi tiết hóa `blocked_reasons`, không đổi decision semantics.
3. Freeze một exact candidate sau hai remediation trên; không tổng hợp evidence từ `ddc67e7`, dirty procurement WIP và commit tương lai thành một claim.

### Gate 1 — đóng gói procurement W-0008 an toàn

1. Sửa mốc 3G thành tháng 9/2028 và dẫn nguồn chính thức; viết lại mô tả 2G theo đúng lộ trình/ngoại lệ.
2. Bỏ “tối đa 2 lần” khỏi vendor-facing document hoặc gắn rõ `TEST_ASSUMPTION / NOT_OWNER_APPROVED`.
3. Đổi 4-channel trong tờ trình thành option cần quote/owner decision; so ít nhất 1-channel và 4-channel bằng chi phí/khả năng lab thật.
4. Bắt vendor xác nhận exact SKU, LTE bands, VoLTE không-CSFB, SIP/DTMF/disposition/CDR/API; model family/name không đủ.
5. Giữ `REAL_CUSTOMER_CALL_ALLOWED=NO`; RFQ/báo giá không đóng `G-LAB-SIM` hay `G-ESIM32`.

### Gate 2 — refresh và ký Target V1 closure pack

1. Nâng closure pack từ baseline draft.18 lên current draft.21+, xóa yêu cầu `sellable_status` đã bị OD-17 supersede và cập nhật admin auth hiện hành.
2. Ký M3 authority: chỉ push task `CALL_REQUIRED`; IVR không business-skip.
3. Ký exact intake/callback OpenAPI version+hash, program mapping, decision/reason branching, ACK/retry/revalidation.
4. Chốt attempt policy, optional session trace, revoke/freshness lifecycle, dial-token trust boundary và opt-out semantics.

### Gate 3 — implementation sau sign-off

1. **Module 3:** Target V1 producer, generic callback consumer, idempotency/revalidation, token issuer/resolver và CDC tests.
2. **IVR:** chỉ thêm session propagation/revoke state nếu contract đã ký; thêm route-parity guard; bổ sung exact capacity overload test; quyết định backend audit/capacity read APIs theo M3 UI ownership.
3. **Telephony:** implement production `ISimGateway` + token resolver client + wiring, reuse `PostgresTelephonyDispatchStore`, sau khi vendor/trust boundary được chốt.

### Gate 4 — shared/external evidence

1. M3→M8 intake E2E và M8→M3 callback E2E trên exact signed contract.
2. Revoke-between-attempt race nếu feature được duyệt.
3. Owner TTS listening, fixed catalog, six MicroSIP calls, retention/rollback, Legal/Security/Infra/target-hardware evidence.
4. One-SIM lab, DTMF/disposition/VoLTE validation, measured call duration/capacity, hosted CI và target DB preflight.

## 9. Lệnh kiểm chứng và kết quả hiện tại

| Kiểm chứng | Kết quả |
| --- | --- |
| `git rev-list --count ef09a06..HEAD` | `16` commit. |
| `git diff --shortstat ef09a06..HEAD` | 285 file, +12.261/-10.141. |
| `dotnet test tests/Ivr.UnitTests/... --no-restore` | `485/485 PASS`. |
| `dotnet test tests/Ivr.ContractTests/... --no-restore` | `24/24 PASS`; không bắt dead OpenAPI account routes. |
| `dotnet test tests/Ivr.IntegrationTests/... --no-restore` | `223/223 PASS`, 2m53s. |
| `dotnet test tests/chaos/... --no-restore` | `8/8 PASS`. |
| `node node_modules/vitest/vitest.mjs run --config vitest.config.mts` | 18 files, `177/177 PASS`. |
| `node deploy/ci/scripts/capacity-selftest.mjs` | `PASS_UNCALIBRATED`; model 21 peak, range 7..72, purchase proof = NO. |
| M3 exact-search Target V1 fields/routes | Generic producer/callback/token fields không tồn tại; current compat callback chỉ Golden Hour. |
| Runtime/OpenAPI account route comparison | Runtime 0 account/session routes; OpenAPI vẫn công bố đầy đủ route/schema cũ. |

## 10. Residual gates

Không gọi Module 8 “done”, “seam connected” hoặc “production ready” cho tới khi có evidence trên cùng exact candidate:

- `CONTRACT_DRIFT`: admin routes/auth docs/OpenAPI/generated artifacts chưa đồng bộ.
- `TRACEABILITY_BROKEN`: W-0122/W-0120 collision và W-0105 dangling history chưa sửa.
- `OWNER_SIGNOFF_REQUIRED`: admin identity boundary, attempt policy, session/revoke/dial-token/opt-out decisions.
- `NOT_BUILT_UPSTREAM`: M3 Target V1 producer và generic callback consumer.
- `BLOCKED_EXTERNAL`: production gateway/credential/hardware, shared environment, TTS approvals/mirror/topology.
- `LEGAL_SECURITY_REQUIRED`: TTS exact artifacts/CVE disposition/retention; opt-out semantics; admin token custody/rotation.
- `NOT_RUN`: hosted CI, shared M3-M8 E2E, target DB, real SIM/DTMF/disposition calls, capacity calibration và production drills.

## 11. Kết luận trả lời trực tiếp

- **File nguồn nói đúng không?** Đúng nhiều chẩn đoán ở `ef09a06`, nhưng thứ tự và một số giải pháp đã lỗi thời.
- **Conflict lớn nhất hiện tại?** Admin runtime đã xóa account/session trong khi OpenAPI/SRS/README vẫn công bố chúng; IR-06 mới cũng chưa khớp enforcement thật.
- **Việc cần sửa đầu tiên?** `W-0128` admin/auth contract-governance remediation, rồi `W-0129` intake reason traceability.
- **Sau đó làm gì?** Sửa procurement pack, refresh Target V1 closure pack, ký contract, rồi mới build M3 producer/consumer và các thay đổi IVR phụ thuộc sign-off.
- **Việc không làm lại?** C2/C13 trusted-skip cleanup; không xây CustomerTrustResolver trong IVR.
- **Trạng thái thật?** Local test projects xanh, nhưng contract/governance đang hỏng và production vẫn blocked.

## 12. Addendum sau triển khai W-0128

### 12.1. Findings đã đổi trạng thái

| Finding | Trạng thái mới | Bằng chứng hiện hành |
| --- | --- | --- |
| F-01 runtime/OpenAPI account drift | `REMEDIATED_LOCAL` | OpenAPI `draft.22` đã bỏ 11 route và 15 schema account/session; portal/hash/generated artifacts đồng bộ; `CT-API-ADMIN-PARITY-01` so 31 admin operation với runtime policy endpoints. |
| F-02 Work ID/history | `REMEDIATED_LOCAL_WITH_HISTORICAL_ALIAS` | Admin transition mang W-0128; W-0105 evidence nguyên bản được phục hồi dưới `SUPERSEDED/HISTORICAL`; tracker/gate-status trỏ đúng file. Migration `20260828040458_W0122DropConsoleAccounts` giữ nguyên technical ID vì không có bằng chứng an toàn để rewrite migration có thể đã apply. |
| F-03 actor/rotation/browser boundary | `REMEDIATED_LOCAL / M3_SIGNOFF_REQUIRED` | Correction: actor vốn đã enforce 29/31 ở endpoint handlers. W-0128 thêm current/previous retirement, min-length/distinct validation, BFF/browser boundary và role→tier deny-by-default spec. |
| F-04 UI ownership/deploy conflict | `REMEDIATED_LOCAL / M3_IMPLEMENTATION_REQUIRED` | `admin-ui` README/UI-08 là reference local; Helm từ chối `ui.enabled=true`, không render UI Deployment/Service; prod API ingress mặc định `[]`. |
| D7 secrets/auth | `LOCAL_IMPLEMENTATION_PRESENT / EXTERNAL_NOT_PROVEN` | API pod lấy ba current token từ Secret, optional previous+retirement; token không vào UI. Real secret-store custody, schedule, Module 3 mapping và shared test còn mở. |

W-0128 còn bắt được một defect không có trong audit ban đầu: reference UI đã gửi `reason` trong
body nhưng không gửi `X-Action-Reason`, nên cả 8 danger operation sẽ bị `403`. Helper HTTP chung
đã nhận `actionReason` tường minh, từng danger wrapper truyền đúng `request.reason`, và unit test
khóa cả scope lẫn header.

### 12.2. Verification trên candidate W-0128

| Gate | Kết quả |
| --- | --- |
| Build/format | 0 warning, 0 error; format verify PASS |
| .NET | unit `485/485`; integration `229/229`; contract `24/24`; chaos `8/8` |
| Admin UI | lint, typecheck, `176/176`, Next build PASS |
| OpenAPI/codegen/docs | lint/validate/negative/drift/NSwag PASS; portal 14 artifact/docs selftest PASS |
| Traceability | regenerate/check `462` tagged test PASS |
| Helm/Compose | 4 env lint/render PASS; UI absent; prod ingress empty; Compose config PASS |
| Markdown map | 594 file; 663 link resolved; 200 unresolved baseline; các file/link W-0128 không thêm unresolved mới |
| GitNexus final change audit | aggregate dirty worktree: 78 tracked file, 454 symbol, 24 process, `CRITICAL`; shared HTTP helper là blast-radius chính và đã được cảnh báo trước sửa + full UI regression |

Không ghi security PASS giả: full PII scan vẫn đỏ ở evidence W-0122/W-0124 cũ; full Gitleaks
worktree còn 45 hit trong migration designer/generated API docs/TTS artifacts và seed reference.
PII scan giới hạn W-0105/W-0128 đạt `2 file PASS`; Gitleaks report path không có
source/config/evidence mới của W-0128. Tuy vậy baseline failures vẫn phải được đóng riêng trước
release.

### 12.3. Phần không thuộc W-0128 và vẫn mở

- `W-0129`: gắn đúng Work ID/evidence cho intake rejection-reason refinement; không sửa trong W-0128.
- M3: role→tier sign-off, regenerated `draft.22` client, BFF implementation và shared positive/
  negative E2E; repo M3 tại baseline audit vẫn chưa có Target V1 seam.
- Platform: real secret-store paths, rotation owner/schedule, real namespace/pod selectors,
  hosted CI và deployment evidence.
- Procurement/capacity/revoke/attempt-policy/dial-token/opt-out findings F-06..F-11 giữ nguyên.
- TTS đã tiến thêm concurrent: Owner đã ký 3 voice sau khi nghe đủ 11 candidate và 12 fixed segment
  đã render/verify file. Sáu cuộc MicroSIP theo order, retention/rollback, Legal/Security/internal
  mirror/target hardware/topology vẫn mở; không nâng production verdict.

### 12.4. Verdict hiện hành

W-0128 đạt `TESTS_PASS_LOCAL`. Contract/admin governance trong repo IVR không còn là blocker P0
local, nhưng seam Module 3 và production vẫn `OWNER_DATA_REQUIRED / BLOCKED_EXTERNAL / NOT_RUN`.
Việc tiếp theo theo ledger là W-0129, sau đó mới freeze exact candidate và tiếp tục các gate external.

## 13. Addendum sau triển khai W-0129

### 13.1. Kết luận sau khi đối chiếu source với mô tả gốc

| Hạng mục | Kết luận hiện hành | Bằng chứng |
| --- | --- | --- |
| Work ID collision | `REMEDIATED_LOCAL` | Comment production/test của refinement đã đổi từ W-0120 sang W-0129; lịch sử globalization W-0120 không bị rewrite. |
| Reason taxonomy | `REMEDIATED_LOCAL` | Chín service reason được liệt kê tại API-06 §2a và khóa bởi `UT-INTAKE-REASON-TAXONOMY-13`. |
| Compatibility | `VERIFIED_LOCAL` | Hai pair business-approved vẫn accepted; mọi rejection giữ decision/failure code/no-job; public route vẫn 400/422. |
| `DIAL_TOKEN_ALREADY_EXPIRED` | `DEFECT_FIXED_LOCAL` | Check `<= now` chạy trước `< window.ExpiresAt`; test riêng chứng minh reason này reachable. |
| Public visibility | `CORRECTED_NOT_EXPOSED` | Required-flag/matrix bị schema chặn trước service; contact reason bị stable `422 IVR_CONTACT_INVALID` envelope che. M3 không nhận chín reason chi tiết. |

Điểm correction quan trọng: câu cũ “mọi rejection là HTTP 200 và `blocked_reasons` là tín hiệu duy
nhất” **không đúng runtime**. W-0129 không đổi runtime để khớp câu đó; API-06 và IR-06 được sửa
theo hành vi thật. Nếu muốn expose safe reason qua wire thì phải mở một contract work riêng, cập
nhật OpenAPI/CDC/client rollout và có M3/owner sign-off.

### 13.2. Verification trên candidate W-0128 + W-0129

| Gate | Kết quả |
| --- | --- |
| Focused compatibility | unit `11/11`; integration `3/3` |
| Build/format | 0 warning, 0 error; format verify PASS |
| Full .NET | unit `490/490`; integration `232/232`; contract `24/24`; chaos `8/8` |
| OpenAPI/docs | lint/validate/negative/drift PASS; API docs `14` artifact/selftest PASS; không có W-0129 contract delta |
| Traceability | regenerate/check `465` tagged test PASS; ba Test ID W-0129 có mặt |
| Markdown/PII | map `595 file / 664 resolved / 200 unresolved` (không tăng unresolved); evidence W-0129 `PII_SCAN_PASS files=1`; added lines API-06/IR-06 có `0` match |
| GitNexus final audit | shared dirty worktree `84 file / 484 symbol / 29 process / CRITICAL`; aggregate chứa W-0128 và concurrent WIP, không quy toàn bộ cho W-0129 |

### 13.3. Verdict và residual

W-0129 đạt `TESTS_PASS_LOCAL`: acceptance về attribution, taxonomy, compatibility và test evidence
đã hoàn tất trong repo. Không nâng thành integration/production ready. M3 reason visibility vẫn
`OWNER_DECISION_REQUIRED`; hosted CI, shared M3→M8 E2E và production vẫn `NOT_RUN /
BLOCKED_EXTERNAL`. Procurement WIP không thuộc W-0129 và không bị sửa trong work item này.

Impact riêng trước sửa của `ContactRejectionReason` là `HIGH` (`19` symbol, `3` process), đã cảnh
báo trước khi thay precedence. Full regression xanh nhưng không làm mất yêu cầu review candidate
chung trước commit, vì final detect trên shared worktree vẫn là aggregate `CRITICAL`.
