# Đánh giá độc lập đầy đủ — worklist M8 ngày 07/09/2026

**Người đánh giá:** Codex. **Code baseline:** main@9603ca55aff6353912eda8cc056d9348aa17917d. **Ngày kiểm tra:** 07/09/2026.

**Kết luận: bản worklist và nhận xét Claude đều đúng một phần; chưa thể dùng nguyên trạng làm danh sách giao việc hoặc kết luận readiness.** Claude xác nhận đúng nhiều chi tiết code và số test, nhưng bỏ sót một số nhánh thực thi, biện pháp đã có và mâu thuẫn trong tài liệu bàn giao. Nhận xét Codex đầu tiên cũng chưa đủ sâu; báo cáo này thay thế nó.

## 1. Phạm vi và cách kiểm tra

- Đọc hết bản worklist: phần đầu, lịch sử cập nhật, bảng smoke, **37 mục A1/B1–B12/C1–C14/D1–D10**, phần giới hạn và phụ lục; sau đó đọc riêng **71 nhận xét Claude**.
- Đọc toàn văn spec V0.3 (**697 dòng**), IR-06 (**1.148 dòng**), các decision pack M8-05 đến M8-11, sign-off 05/09 và open-decisions register. Đối chiếu thêm worklist 03/09, capacity, migration/runbook, TTS, W-0207, contract manifest và gate ledger.
- Đọc các luồng code liên quan: HTTP intake/auth → policy/contact → persistence → eligibility → scheduler/lease/dispatch → result → callback/outbox; runtime approvals, admin projection, TTS và cấu hình. Không dùng “file không có diff” thay cho kiểm tra behavior.
- Chạy lại toàn solution và các gate liên quan; viết probe trong thư mục tạm để gọi chính validator đã build, tái hiện hai sai lệch. Không sửa source/config, không gọi khách, không gửi hồ sơ cho external owner.
- Đây là audit **checkout hiện tại có thay đổi tài liệu**, không phải chứng nhận clean checkout, hosted CI, M3 sandbox, server 192.168.1.61 hoặc production. Không tuyên bố đọc mọi file trong toàn repository, mọi test method hoặc mọi tài liệu ngoài repo.
- Dòng Lxxx trong mục 5 là vị trí nhận xét Claude trong bản đầu vào đã lưu; không phải số dòng của code được Claude dẫn.

## 2. Các kết luận cần sửa trước khi giao việc

### F01 — C6/C13: lỗi TTL có thật; còn một guard ở dispatch mà hai đánh giá trước chưa nêu

Contact gate chỉ từ chối token hết hạn **trước** window end; snapshot giữ nguyên expiry, không clamp. Persistence lại từ chối expiry **sau** window end. Probe trực tiếp trên assembly hiện tại cho kết quả:

~~~text
expiry=window+0s  contact=PASS  persistence=PASS
expiry=window+60s contact=PASS  persistence=InvalidOperationException:
Dial-token expiry must remain inside the confirmation window.
~~~

Ngoài persistence, **PostgresTelephonyDispatchStore.LoadAsync:157–159 cũng từ chối token expiry vượt call deadline**. Vì vậy sửa riêng OpenAPI/persistence theo OD-V1-17 “+60s” vẫn chưa đủ. Cần thống nhất issuer TTL, thời hạn được phép dial và mọi guard xuyên suốt luồng. Đây là lỗi nhất quán có thể tái hiện; probe chưa phải HTTP/database E2E. [Intake][S01] · [Persistence][S02] · [Dispatch][S03]

IR-06 tự mâu thuẫn: L219 ghi ≥ window end, L522/L534 yêu cầu >, L983 mô tả equality. Cần sửa cả hướng dẫn producer. Với phone_validation_status, thiếu/khác VALID thực sự bị 422; nhưng payload hợp schema không mặc nhiên được business chấp nhận. Việc đổi optional thành required enum VALID là thay đổi contract cần xét compatibility, không phải phép sửa hiển nhiên không ảnh hưởng producer. [IR-06][S05]

### F02 — B4: seed approval có thật; kết luận về quyền thay đổi prod và cách sửa đang thiếu nhánh code

Migration seed RUNTIME_GATE_ADMIN với environment NULL; không seed PRODUCTION_CALL. Tuy nhiên **FeatureFlagAdminService:74–79 từ chối mọi risk increase tại prod trước bước kiểm four-eyes**. Có thêm approval row cũng không làm HTTP API cho phép tắt kill switch/mở rộng allowlist ở prod. Vì vậy không nên xác nhận toàn bộ B4 là “đúng tuyệt đối”. [Migration][S08] · [Admin service][S06]

Điểm cần sửa trong đề xuất “chỉ seed dev/lab”: RuntimeGateApprovalReader.AnyLiveAsync:110–119 **không lọc environment hoặc actor**; nó chỉ xét kind, revocation và expiry trong DB đang đọc. Chỉ đổi environment trên row không tạo được giới hạn môi trường trong cùng DB. Nếu cần approval theo môi trường, phải thay cả contract/reader và test; vẫn giữ khả năng giảm rủi ro khẩn cấp. Đây không phải bằng chứng đang có đường gọi production vượt gate. [Approval reader][S07]

### F03 — B6: đúng về hai lịch sử W0122; sai khi coi runbook và test hai lịch sử là việc chưa làm

W0122 đã đổi Up thành no-op, cùng ID; EF không chạy lại migration đã ghi nhận. Nhưng P03 đã thêm repair, và **expand-contract.md:21–32 đã mô tả đủ DB chưa chạy drop, đã chạy drop, phục hồi từ pre-drop backup và partial schema**. ExpandContractMigrationTests có hai case false/true và nằm trong suite integration vừa pass. [Runbook][S09] · [Tests][S10] · [Evidence W-0196][S11]

Cần giữ phần còn mở là inventory/rehearsal trên database đích và backup dữ liệu thật. Repair schema không phục hồi account/session đã xóa; không thể từ migration ID suy database đích đủ dữ liệu rollback. Hai migration trùng timestamp nhưng khác full ID vẫn có thứ tự xác định; chưa có bằng chứng đây là lỗi dependency. Đổi nội dung migration trong commit mới cũng **không đồng nghĩa rewrite Git history** như cách diễn đạt ở D7.

### F04 — C10: tiêu chí cũ sai, và OD-V1-23 mới còn nhầm phím 0 với opt-out

Worklist giữ tiêu chí “2 lần rejected → do-not-call” trong khi M8-08 yêu cầu explicit-only. Code hiện map Rejected thành NO_ANSWER + review; DTMF 0 thành **IVR_CUSTOMER_CANCELLED**, không phải cấm liên hệ. [Mapper][S12] · [M8-08][S14]

**Phát hiện thêm sau khi đọc toàn bộ register:** OD-V1-23 L64 gọi “DTMF-0 / handoff” là tín hiệu opt-out tường minh. Điều này trái code, M8-08 L65 và script/spec hiện hành. “Explicit-only” không đủ nếu định nghĩa explicit signal sai. Phải có tín hiệu riêng được các owner ký; không lấy thao tác hủy một đơn làm yêu cầu ngừng gọi về sau. Approval table của M8-08 vẫn chưa nhận đủ chữ ký; OD-V1-23 cũng ghi QUORUM_PENDING. [Register][S13]

### F05 — B1: 40/50/60 không phải ba số cùng nghĩa; external intake còn đang được owner hoãn

40s và 60s là occupancy; 50s là **full cycle có cooldown**. Capacity model và tài liệu đã yêu cầu khi calibration: model/runtime dùng occupancy, spec dùng occupancy + cooldown. Yêu cầu “đưa cả ba về một số” sẽ sai đơn vị hoặc tính cooldown hai lần. CAP-DRIFT-05 vừa pass với chính trạng thái DECLARED_DISAGREEMENT. [Model][S16] · [Capacity docs][S15]

PT-CAP-02 đã có trong phần cập nhật của chính worklist; không tiếp tục giao “thêm test” như chưa làm. W-0189 và worklist 03/09 còn ghi **EXTERNAL_INTAKE_DEFERRED_BY_OWNER**: chưa có 4 input không đồng nghĩa phải tự mở lại dispatch/provider intake. Các % 80/65 chỉ là ước lượng chủ quan. [W-0189][S42] · [Worklist 03/09][S20]

### F06 — B11: guard opaque không chứng minh thiết kế resolver trong adapter không thể triển khai

DialAuthorization cấm raw phone đi qua type này. Nhưng API-04 §2 hiện mô tả resolve **bên trong adapter**, E.164 chỉ ở memory tại đó, các boundary khác vẫn opaque. Một adapter có thể nhận opaque handle rồi resolve nội bộ mà không đưa số thô vào DialAuthorization. Do đó câu Claude “code làm OD-V1-18 không thi hành được” quá mạnh. [Port][S18] · [API-04][S17]

Mâu thuẫn tài liệu với M8-10/spec V0.3 về vị trí resolver là có thật và cần reconciliate authority/threat model. Production adapter/resolver vẫn chưa có. Không đề xuất bỏ privacy guard để làm tài liệu đúng.

### F07 — B9: ownership UI đã chuyển, nhưng projection hiện có không phải lịch sử audit đầy đủ

Worklist 03/09 đã đóng implementation local và chuyển operator UI/BFF cho M3. API hiện trả **tối đa 20 incident OPEN** trên dashboard; call detail có evidence_refs/audit_refs. Các read surface này đáp ứng handoff đã ghi nhận. [Worklist cũ][S20] · [Admin reads][S19]

Tuy nhiên chúng không tương đương endpoint xem toàn bộ lịch sử incident/audit. **Tôi rút lại câu cũ “hai endpoint đã tồn tại”**: chính xác là dữ liệu/projection cần cho handoff hiện tại đã có, còn yêu cầu lịch sử đầy đủ phải có use case/contract riêng. Không tự dựng thêm hai route chỉ để giữ B9 ở nhóm code lõi; không gắn công việc UI M3 vào tỷ lệ code M8 còn thiếu.

### F08 — A1/B3/B7: phân biệt bản ghi owner chọn phương án, quyền đồng ký và khả năng thực thi

Có thật bản ghi chọn 19 quyết định; có thật policy gh-247-prod-v1 seed approved_for_production=true; có thật ProductionTargetV1FieldsApproved=YES trong bốn appsettings. Đây là trạng thái cần làm rõ với các owner được liệt kê. Nhưng email commit **không chứng minh hoặc bác bỏ** quyền sở hữu/thẩm quyền của email signer; lời Claude khẳng định danh tính cũng chưa là bằng chứng độc lập. [Sign-off][S21] · [Policy][S22] · [Migration policy][S23]

Sign-off tự ghi quyết định nội bộ không đóng external gate. M8-11/closure pack vẫn đòi Product/Core/M3 và các artifact khác. Không đủ cơ sở để coi cả 19 quyết định vô hiệu hoặc tự lùi toàn bộ về OPEN. Nên đối chiếu từng quyết định, scope và approval reference. **Runtime hiện vẫn chặn real calls khi boot; production gateway chưa có; TargetV1 delivery thật vẫn bị options validator chặn.** “Flag/policy có chữ production” không phải “production đã mở”. [Options][S40] · [Delivery options][S47] · [DI][S46]

### F09 — B5 và smoke: cutoff có thật; W-0203 giải quyết profile local, không chứng minh server đã sửa

CallingWindow đóng tại 21:00 và scheduler kiểm trước claim. Ví dụ task GH T0=20:59, window=5 phút, attempt 2=21:01:30: attempt 2 nằm trong window của task nhưng ngoài giờ gọi mặc định. Đây là tương tác policy cần test lifecycle và quyết định rõ. CallingWindowTests hiện chủ yếu kiểm hàm thời gian/config, chưa chứng minh luồng task vắt mốc đóng. [Window][S25] · [Runtime][S24]

W-0203 có LocalMockE2E chạy cả ngày; compose.e2e cũ không tự nhận profile đó. Kết quả local mới không xóa lịch sử smoke server tại baseline khác; chưa đọc config/log triển khai thật thì chưa xác nhận nguyên nhân server tuyệt đối. “1 tham số + 1 test” chỉ là dự đoán, không phải estimate đã chứng minh. LOCK-05 cần nguồn M3/owner.

Claude còn suy quá mức “window đóng → ba counter đều 0 → không dòng log”: quarantine/deadline closing chạy **trước** hour gate, nên vẫn có thể phát log khi có công việc. Khoảng trống đúng là worker không log rõ lý do window-closed khi idle. [Worker host][S26]

### F10 — D1/C8 và tài liệu bàn giao: phát hiện thêm giới hạn header và hướng dẫn đã cũ

IR-06 L157–158 cho Idempotency-Key 8–200 và X-Correlation-Id 1–200 ký tự; TaskIntakeEndpoint.RequiredHeader chỉ nhận **1–128** ký tự an toàn. Probe gọi helper thật xác nhận cả hai header: 128 PASS, 129 bị IvrFailureException. OpenAPI parameter hiện chỉ type:string, không nêu cùng constraint này. Cần đồng bộ hướng dẫn/schema/runtime trước khi M3 sinh request từ tài liệu. [Endpoint][S04] · [IR-06][S05] · [OAS][S48]

IR-06 còn hướng dẫn lấy draft.22 tại L8/L25/L914/L1129 dù manifest hiện draft.23; §4A.7 nói bảng console không còn trong DB dù P03 giữ/recreate bảng compatibility. Spec V0.3 §16 và database-enums cũng còn trình bày 11 mã như runtime trong khi runtime subset là 9. Vì vậy FREEZE PASS chỉ xác nhận những invariant gate kiểm (pins, field inventory, draft state, ACK matrix), không chứng minh mọi câu trong docs đã khớp code. [Spec đầy đủ][S43] · [Result policy][S32] · [Manifest][S28]

### F11 — Phần trăm, lịch sử và Git: cần sửa cả phản biện của Claude

Phép tính của bản gốc **tái lập được**:

- B: (0,80 + 0,65) / 5 = 29%.
- C: (0,40 + 0,50 + 0,20 + 0,55 + 0,40 + 0,55 + 0,25 + 0,50 + 0,35 + 0,15) / 13 = 29,615% ≈ 30%.
- Tổng: (1,45 + 3,85) / 18 = 29,444% ≈ 29%, phần còn lại ≈ 71%.

Claude bỏ sót bốn % nên phản biện tại L26 sai. Nhưng đây vẫn là trung bình ước lượng của danh sách cũ, có mục trùng/đã đóng/chờ external, không đo phần trăm module hay năng suất.

Baseline 9f13bea chậm HEAD 8 commit là đúng; b21ec67..9f13bea đúng 54 commit. B2 không còn bốn file WIP trên checkout này. Có 4 local branch, dù luật chỉ cho làm việc trên main. Câu “không nhánh nào” xuất hiện **16 hàng B/C**, không phải 14; commit subject save/sa ve trên lịch sử reachable từ HEAD là **32, đã gồm HEAD**, không phải 33.

Candidate w0128-w0129 ahead 1 nhưng **behind 73**, commit riêng chạm **92 file**, không chỉ admin-ui. Không suy “ahead 1” thành thay đổi nhỏ có thể merge mù. Audit này không merge/delete branch. Không tìm thấy file 30/08 trong Git cũng chưa bác bỏ được tài liệu bên ngoài. “3 phiếu TTS bị xóa” không mâu thuẫn “10 phiếu tổng cộng”; không được gán câu về subset đó sang phiếu OD-18 còn sống.

### F12 — B8/D8/E: test và artifact đúng phải được giữ đúng phạm vi

Đã xác minh độc lập con số **891/891** Claude báo. Voice acceptance gate cũng xác nhận manifest ba giọng OWNER_ACCEPTED còn khớp; provenance structure gate vẫn báo LEGAL,INTERNAL_MIRROR. Owner chọn giọng, nghe mối nối/sáu cuộc thử, target hardware và release approval là các việc khác nhau. Không mở lại phần đã ký khi binding không đổi; cũng không dùng artifact chọn giọng để đóng lab/production. [W-0122][S39]

Các số 481/466/535 ở ô D8 là lịch sử có ngày; cần tách khỏi headline current, không gọi chúng là mâu thuẫn số học nếu khác snapshot. HEAD có 550 tagged test declarations nhưng 891 executed cases vì Theory mở nhiều case; hai số không thay nhau. Lời “tôi không chạy test trong phiên cũ” vẫn đúng về phiên cũ, cần bổ sung kết quả mới, không sửa lại lịch sử người khác.

## 3. Đối chiếu đủ 37 mục

| Mục | Đánh giá sau đọc tài liệu và code | Việc thực sự còn lại |
| --- | --- | --- |
| A1 | **Một phần.** Có sign-off 19 dòng; thiếu bằng chứng quyền đồng ký, chưa chứng minh ký giả/sai toàn bộ. F08. | Reconcile từng scope và external approval; không rollback đồng loạt. |
| B1 | **Một phần, action cũ.** Model/test đã có; occupancy khác full cycle; external intake đang hoãn. F05. | Giữ UNCALIBRATED; chỉ mở calibration khi owner mở lại và đủ dữ liệu. |
| B2 | **Đã hết tiền đề ở checkout này.** Ba file tracked không đổi, file thứ tư không hiện hữu. | Bỏ khỏi hàng đợi WIP current; giữ ghi nhận lịch sử. |
| B3 | **Đúng trạng thái code; chưa đủ để kết luận production mở.** Version production tách candidate. F08. | Đối chiếu approval theo ATP và rollout, không gọi mock-lab-v1 đã bị promote. |
| B4 | **Sai một phần quan trọng.** Prod risk increase bị API chặn; reader không scope environment. F02. | Chốt scope và sửa cả reader nếu thực sự cần phân môi trường. |
| B5 | **Tương tác cutoff có thật; cách sửa chưa được chốt.** F09. | Test vòng đời qua 21:00; thống nhất T0/window/quiet hours với owner. |
| B6 | **Biện pháp local đã có.** Hai lịch sử có runbook/test/repair. F03. | Target DB inventory/backup/rehearsal và rollback floor. |
| B7 | **Đúng flag YES.** Không chứng minh đã có đầy đủ Privacy/Legal; real-call gate vẫn đóng. F08. | Reconcile whitelist approval và deployment scope. |
| B8 | **Local artifact có; external còn chặn.** Chọn giọng không đồng nghĩa nghe mối nối. F12. | Theo gate từng artifact; quyết định giọng người thu sẵn không tự tạo ra audio/evidence. |
| B9 | **Không còn là ticket UI M8.** Projection hiện có; full history khác scope. F07. | M3 UI/BFF/authz; contract mới nếu muốn history rộng hơn. |
| B10 | **Đúng constraint.** No-answer không xin đổi state; expired có expire/hold. [S32] | Chốt semantics với Core; giữ mapping hiện hành tới khi contract thay đổi. |
| B11 | **Có drift tài liệu; kết luận không thể implement quá mạnh.** F06. | Ký trust boundary; giữ opaque domain và implement adapter khi đủ input. |
| B12 | **Đúng: production adapter còn thiếu.** DI chỉ mock/lab/unavailable. [S46] | Vendor/custody/network và lab; không coi Asterisk softphone là real-SIM proof. |
| C1 | **Enum/matrix đúng.** IR-06 §3.10 R3 đã ghi nguồn business cho hai pair; không phải phát hiện business mới hoàn toàn. [S05] | Ký wire mapping/producer; không tự nhận thêm alias hoặc cặp payment. |
| C2 | **9 runtime / 6 final / 5 counted đúng; 11 là vocabulary compat.** F10. | Sửa docs theo các tập con; không ép mọi tầng về cùng số lượng. |
| C3 | **Upstream session chưa có, đúng.** Trùng bài toán C4/C7. [S33] | Signed golden_hour_session_id và producer mapping. |
| C4 | **Đúng còn thiếu upstream trace; đề xuất map đè session_id sai.** [S33] | Giữ internal capacity ID, thêm field riêng theo contract đã ký. |
| C5 | **Có pack, chưa đủ external approvals.** 0/5 dispatch là trạng thái ledger, không chứng minh mọi trao đổi ngoài repo đều bằng 0. [S49] | Theo owner/receipt; không coi một chữ ký IVR là mọi bên đã nhận. |
| C6 | **TTL lỗi được tái hiện; requiredness cần quyết định compatibility.** F01. | Đồng bộ intake/persistence/dispatch/OAS/producer và boundary tests. |
| C7 | **Current wire chưa có priority/session upstream.** Phase required GH là breaking, dù store nullable additive. [S33] | Gộp C3/C4; store → producer/CDC → enforce, không hứa toàn bộ non-breaking. |
| C8 | **Callback IVR đã implement; W-0207 đã sửa ACK matrix.** [S34] · [S35] · [S27] | M3 consumer/auth/shared E2E; sửa handover stale theo F10. |
| C9 | **Chặn TARGET_V1 thật có chủ đích.** [S47] | Đủ auth/sandbox/approval rồi mới mở delivery thật. |
| C10 | **Tiêu chí rejection cũ và ví dụ DTMF-0 của OD23 đều cần sửa.** F04. | Signed explicit signal, CRM identity/lifecycle/reversal và feedback contract. |
| C11 | **Chưa có business revoke/recheck giữa window.** ACK callback không thay command. [S36] · [S31] | Chọn A/B/hybrid; nếu B phải fence tới trước dial, gộp C14. |
| C12 | **Đúng chỉ đọc evidence M3; không direct Ops egress.** [S30] · [S36] | M3 D-06/freshness/recall evidence; không tự thêm Ops client. |
| C13 | **Gate contact hiện có, production issuer/resolver thiếu.** Ledger local W-0199 đã có task binding/ceiling/audit. [S37] · [S50] | Chốt production custody/issuer và sửa TTL xuyên tầng; local ledger không phải distributed replay proof. |
| C14 | **Đúng thiếu business fence; lặp C11.** Technical lease generation hiện có không phải order-revocation generation. [S03] · [S36] | Một lifecycle design chung cho C11/C12/C14. |
| D1 | **Core intake/auth có; chưa thể gọi docs/contract sạch.** Header drift F10; middleware có auth thực dù endpoint AllowAnonymous. [S04] · [S51] | Đồng bộ hướng dẫn, OAS và giới hạn runtime. |
| D2 | **Giữ DONE_LOCAL.** Intake từ chối QUOTE/CART/DRAFT; eligibility yêu cầu CONFIRMING. [S01] · [S30] | Producer thật/shared negative evidence thuộc M3. |
| D3 | **Giữ implementation local của các gate.** Đọc code xác nhận fail-closed; không suy mid-window freshness đã có. [S30] | External evidence và freshness theo C11/C14. |
| D4 | **Giữ DONE_LOCAL.** Immutable payload/key, bounded retry, terminal ACK, Retry-After đã có. [S35] · [S34] | M3 xử lý idempotency/ACK thật và shared evidence. |
| D5 | **Giữ DONE_LOCAL.** Result là advisory; không có đường update payment/order trong luồng đã đọc. [S12] · [S34] | Chứng minh Core revalidation ở phía M3. |
| D6 | **Giữ code policy local, mở governance.** Candidate và signed-production version có cùng số nhưng khác identity. [S22] · [S38] | ATP lifecycle/canonical producer/approval/cutover. |
| D7 | **Không phát hiện secret mới trong phạm vi đã đọc; chưa chứng nhận mọi secret/history.** Inventory còn stale trust-boundary wording. [S29] | Custody/rotation deployed và source scan trên candidate release riêng. |
| D8 | **Xác nhận 891/891 local.** Không phải 550 tests “thiếu”; tagged declarations khác executed cases. | Hosted CI, UI/clean-checkout và external evidence chưa được chạy trong audit này. |
| D9 | **Giữ DONE_LOCAL về bỏ trusted-skip.** Metadata còn đọc cho history/counter, không đảo eligibility. [S01] · [S30] | OD-18 M3 producer/consumer sign-off; giữ historical evidence. |
| D10 | **Giữ DONE_LOCAL về authority.** M3 quyết CALL_REQUIRED; risk_flags chỉ audit/priority. [S01] · [S31] · [S05] | Xác nhận runtime phía M3; không kéo trust resolver vào IVR. |

## 4. Kiểm chứng đã chạy trong lượt này

| Kiểm tra | Kết quả | Giới hạn |
| --- | --- | --- |
| dotnet test Ivr.sln --no-restore | **891/891 PASS**, 0 failed, 0 skipped, exit 0: Unit 593; Integration 266; Contract 24; Chaos 8 | PostgreSQL/chaos fixtures cô lập; source HEAD hiện tại, checkout có docs WIP; không phải server/M3/production |
| TTL probe | Equality qua cả hai validator; +60s qua contact rồi bị persistence từ chối | Gọi method thật qua reflection, dữ liệu giả, không DB/HTTP |
| Header probe | Idempotency-Key và X-Correlation-Id: 128 PASS, 129 reject | Gọi helper HTTP thật, không dựng server |
| capacity-selftest.mjs | 6 nhóm PASS; UNCALIBRATED / DECLARED_DISAGREEMENT / UNANSWERED giữ nguyên | Không đo cuộc gọi hoặc sizing thiết bị thật |
| contract-freeze-verifier.mjs | PASS; TARGET_CONTRACT_V1=DRAFT; intake draft.23; 22/13 required fields | Chỉ các invariant của verifier |
| contract-freeze-selftest.mjs | 14/14 PASS | Mutation trong temp, không thay working tree |
| progressive-selftest.mjs | 4 nhóm PASS; 23 migrations; expand guard 15 negative + 4 additive controls | Canary/blue-green là cấu hình; không triển khai Argo |
| tts-voice-acceptance-gate.mjs --acceptance | PASS; manifest 90927e16… còn khớp | Artifact acceptance, không nghe lại audio |
| tts-provenance-gate.mjs | STRUCTURE_PASS; 13 artifacts; blockers LEGAL,INTERNAL_MIRROR | Không biến structure pass thành production provenance |

Log/TRX, hash và command được ghi trong [evidence JSON](2026-09-07-m8-independent-full-review.evidence.json). Full solution kết thúc 09:39:15 +07:00; Integration mất khoảng 7 phút 38 giây theo console. Không chạy lại 20/100 vòng W-0207/W-0203, không chạy UI/browser, real-SIM, hosted CI, legal/license verification hay smoke server. Các bằng chứng lịch sử đó được đọc dưới đúng nhãn lịch sử.

## 5. Đối chiếu toàn bộ 71 nhận xét Claude

“Một phần” nghĩa là có chi tiết đúng nhưng kết luận/phạm vi quá rộng; “chưa xác minh” không đồng nghĩa sai. Các Fxx ở trên và mục việc ở bảng 3 cung cấp căn cứ.

| # | Dòng Claude | Kết luận Codex |
| --- | --- | --- |
| 01 | L7 | Đúng: 54 commit tới baseline, HEAD thêm 8; cần tách snapshot. |
| 02 | L12 | Một phần: repo không có file 30/08, chưa bác bỏ được baseline ngoài repo. |
| 03 | L19 | Đúng: tổng 37 mục, chia 1/12/14/10. |
| 04 | L26 | **Sai**: đủ 10 số C, phép tính tái lập được. F11. |
| 05 | L35 | Đúng về giới hạn phần trăm; không dùng làm năng suất/khối lượng module. |
| 06 | L42 | Đúng giá trị 60; thiếu phân biệt occupancy/full-cycle. F05. |
| 07 | L63 | Đúng về trigger/check của migration; không thay kiểm consumer. F02. |
| 08 | L70 | Đúng 9/6/5 và các constraint. |
| 09 | L95 | Một phần: **32 đã gồm HEAD**, không thành 33. F11. |
| 10 | L128 | Chưa xác minh độc lập danh tính email/authority; Git author không giải quyết câu hỏi đó. |
| 11 | L133 | Một phần: profile local mới có; không chứng minh config server hay mọi idle log. F09. |
| 12 | L142 | Đúng thiếu test lifecycle qua cutoff; file vẫn có Fact, không chỉ Theory. |
| 13 | L147 | Đúng: real calls NO và boot/DI vẫn chặn. |
| 14 | L158 | Đúng: 23 migrations, draft.23, có thay đổi W-0199. |
| 15 | L163 | **Sai cách so phạm vi**: 3 phiếu TTS là subset của 10 phiếu; OD-18 không thuộc subset đó. |
| 16 | L168 | Đúng 8 entry được nêu có evidence:null ở HEAD; W-0200 CANCELLED mà TESTS_PASS cần tách nghĩa. |
| 17 | L173 | Đúng: FailureWindow 10 phút có thật. |
| 18 | L178 | Đúng diff các file được chỉ định; không suy mọi dependency/behavior đều không đổi. |
| 19 | L185 | Đúng nội dung sign-off/register; chưa xác nhận quyền ký thay tất cả owner. |
| 20 | L190 | Một phần: raw-phone guard đúng, suy thành cấm resolver trong adapter là quá mạnh. F06. |
| 21 | L199 | Đúng trùng timestamp; full ID khác, thứ tự xác định, chưa chứng minh lỗi migration. |
| 22 | L204 | Đúng ba health route; /health trần không phải route tương đương. |
| 23 | L216 | Đúng fixture confirm và compose thiếu hour override; chưa rerun server. |
| 24 | L218 | Đúng fixture no-answer. |
| 25 | L220 | Đúng fixture cancel bằng phím 0. |
| 26 | L224 | Đúng giá trị row policy; production readiness vẫn chưa có. |
| 27 | L226 | Đúng draft.23; schema file không đồng nghĩa có route serve runtime. |
| 28 | L231 | Đúng thứ tự close-deadline rồi kiểm window trước claim. |
| 29 | L236 | Đúng phân biệt MockSchedulerDispatchGateway và DB DispatchGate của nhánh lab. |
| 30 | L241 | **Sai suy luận counter luôn 0** khi window đóng. F09. |
| 31 | L250 | **Sai quan hệ nhân quả**: intake resolve theo version trong payload, không đọc feature-flag version. Dev seed cũng có mock-lab-v1. |
| 32 | L257 | Đúng số 891/891; đã chạy lại độc lập. Không sửa lời khai lịch sử của phiên cũ. |
| 33 | L272 | Một phần: cần review approval; chưa đủ cơ sở lùi toàn bộ 19 quyết định. F08. |
| 34 | L279 | Một phần: chỉ ra tham số đúng; “1 tham số + 1 test chính xác” chưa được chứng minh. |
| 35 | L284 | Đúng B2 không còn WIP tại checkout này và không được tạo nhánh lab; không khẳng định lại mọi clone lịch sử. |
| 36 | L289 | Đúng diff mặt cắt; giới hạn như L178. |
| 37 | L297 | Đúng các record/owner cột trong tài liệu; authority vẫn là câu hỏi khác. |
| 38 | L306 | Một phần: số/default đúng; sửa yêu cầu calibration về một số, giữ owner-deferred. |
| 39 | L308 | Đúng B2 ở checkout hiện tại; phạm vi máy khác/historical không tái xác minh. |
| 40 | L310 | Đúng INSERT; không đồng nghĩa bypass release. F08. |
| 41 | L312 | Một phần quan trọng: seed đúng; đề xuất environment-only thiếu reader và bỏ sót prod guard. F02. |
| 42 | L314 | Đúng cutoff/constraint; thiếu lifecycle proof và source LOCK-05. |
| 43 | L316 | **Thiếu bằng chứng đã có**: runbook và tests hai lịch sử hiện hữu. F03. |
| 44 | L318 | Đúng bốn YES và chỉnh line Api về 18. |
| 45 | L325 | Một phần: external blockers thật; “không việc nào chạy được bằng code” quá rộng, phải tách từng gate. |
| 46 | L327 | Đúng ownership UI; đề xuất thêm hai route chưa có use case mới, projection đã có. F07. |
| 47 | L329 | Đúng constraint action/finality; đổi business semantics cần Core ký. |
| 48 | L331 | **Sai kết luận bất khả thi** từ opaque guard; tài liệu đã mô tả adapter boundary. F06. |
| 49 | L338 | Đúng fail window và production adapter unavailable; không phải real-SIM evidence. |
| 50 | L365 | Đúng enum/matrix; IR-06/V0.3 đã ghi business source, còn wire sign-off. |
| 51 | L367 | Đúng 9/6/5 và drift docs; không sửa bằng cách ép mọi tầng có cùng 11 mã. |
| 52 | L369 | Đúng thiếu upstream session; internal capacity scope không phải business session. |
| 53 | L372 | Một phần: ledger dispatch chưa có; “mọi mục C chỉ chờ gửi” bỏ qua TTL và runtime gaps. |
| 54 | L374 | Đúng contact/TTL mismatch; bổ sung dispatch guard và compatibility nuance. F01. |
| 55 | L382 | Đúng route/header callback và sửa ACK matrix W-0207; local không phải M3 E2E. |
| 56 | L384 | Đúng options chủ động chặn TARGET_V1 thật. |
| 57 | L388 | Đúng không direct Ops lookup; source snapshot không phải dữ liệu order hiện thời. |
| 58 | L395 | Đúng bảy reason contact; “validate tốt hơn spec” không giải quyết TTL failure. |
| 59 | L397 | Đúng cần business fence sau claim; technical lease fence không thay revoke. |
| 60 | L406 | Đúng diff/phiên bản; D1 vẫn có drift handover header mới tìm thấy. F10. |
| 61 | L412 | Đúng số của signed policy; approval scope không suy từ tên OwnerApproved. |
| 62 | L415 | Đúng 891/550 tại HEAD; số các ngày cũ khác nhau không tự mâu thuẫn. F12. |
| 63 | L423 | Hợp lý về cách nêu giới hạn; không bảo đảm các giả định của E đều đúng. |
| 64 | L428 | Đúng môi trường hiện tại chạy được .NET; xác nhận bằng full run mới. |
| 65 | L433 | Đúng B2 tại checkout này; chưa chứng minh mọi checkout lịch sử. |
| 66 | L440 | Đúng repo không tự chứng minh danh tính; khẳng định ngoài repo của Claude chưa có chứng cứ độc lập. |
| 67 | L445 | **Quá mạnh**: empty diff không đủ bảo đảm phương pháp “không sinh lỗi”; phải đọc caller/config/guard. |
| 68 | L452 | Đúng thiếu config server; profile W-0203 không tự sửa deployment server. |
| 69 | L463 | Đúng tiền đề WIP current lỗi thời; giữ phạm vi checkout. |
| 70 | L472 | Đúng có branch khác; **16 hàng**, không phải 14; candidate ahead 1/behind 73 chạm 92 file. F11. |
| 71 | L479 | File tồn tại không xác nhận mức độ đã đọc của người khác; báo cáo này chỉ cam kết phạm vi mục 1. |

## 6. Cách dùng lại worklist

Giữ bản gốc và comment Claude làm lịch sử; dùng đánh giá này để cập nhật một danh sách current sau khi owner yêu cầu. Trước hết sửa các hướng dẫn có thể làm producer/implementation đi sai: TTL xuyên tầng, header constraints, DTMF opt-out, approval environment semantics và phân biệt occupancy/full cycle. Tách B2/B9 đã đóng hoặc chuyển owner, B6 đã có mitigation local; gộp session C3/C4/C7 và revoke C11/C12/C14 theo thiết kế chung.

Giữ các phần code local đã được chứng minh ở D, kèm giới hạn phù hợp. Chưa có căn cứ nâng TARGET_CONTRACT_V1 khỏi DRAFT, bật TARGET_V1 delivery thật hay đặt REAL_CUSTOMER_CALL_ALLOWED=YES.

## Nguồn đối chiếu

Các liên kết dưới trỏ vào checkout đã kiểm tra; số dòng áp dụng cho source tại HEAD nêu đầu báo cáo.

[S01]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Intake/TaskIntakeService.cs:389>
[S02]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Persistence/PersistenceInvariantValidator.cs:108>
[S03]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/PostgresTelephonyDispatchStore.cs:139>
[S04]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Api/Intake/TaskIntakeEndpoint.cs:114>
[S05]: <C:/Users/Administrator/Desktop/ivr/integration-requirements/06-module-3-api-handover.md:151>
[S06]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/FeatureFlags/FeatureFlagAdminService.cs:49>
[S07]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/FeatureFlags/RuntimeGateApprovals.cs:89>
[S08]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Persistence/Migrations/20260905034908_W0195RuntimeGateApprovals.cs:131>
[S09]: <C:/Users/Administrator/Desktop/ivr/docs/database/expand-contract.md:21>
[S10]: <C:/Users/Administrator/Desktop/ivr/tests/Ivr.IntegrationTests/ExpandContractMigrationTests.cs:13>
[S11]: <C:/Users/Administrator/Desktop/ivr/docs/evidence/W-0196/README.md:1>
[S12]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Domain/Confirmation/DispositionMapper.cs:71>
[S13]: <C:/Users/Administrator/Desktop/ivr/specs/_review/open-decisions-register.md:64>
[S14]: <C:/Users/Administrator/Desktop/ivr/plan/ivr-orther/m8-08-opt-out-suppression-decision-pack-2026-09-03.md:64>
[S15]: <C:/Users/Administrator/Desktop/ivr/docs/capacity-model.md:114>
[S16]: <C:/Users/Administrator/Desktop/ivr/tools/capacity-sim/capacity-model.mjs:17>
[S17]: <C:/Users/Administrator/Desktop/ivr/specs/api/04-sim-adapter-contract.md:16>
[S18]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Domain/Ports/ProviderPorts.cs:16>
[S19]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Api/Application/AdminReadService.cs:171>
[S20]: <C:/Users/Administrator/Desktop/ivr/plan/toan-viec-can-lam-m8-2026-09-03.md:12>
[S21]: <C:/Users/Administrator/Desktop/ivr/plan/ivr-orther/od-v1-signoff-2026-09-05.md:1>
[S22]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Intake/AttemptPolicyRegistries.cs:52>
[S23]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Persistence/Migrations/20260905120000_W0196SignedProductionAttemptPolicy.cs:48>
[S24]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Scheduling/SchedulerRuntime.cs:70>
[S25]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Scheduling/CallingWindow.cs:110>
[S26]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Worker/Jobs/SchedulerJobHost.cs:42>
[S27]: <C:/Users/Administrator/Desktop/ivr/docs/evidence/W-0207/README.md:17>
[S28]: <C:/Users/Administrator/Desktop/ivr/specs/api/openapi/contract-manifest.json:1>
[S29]: <C:/Users/Administrator/Desktop/ivr/docs/secret-inventory.md:1>
[S30]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Domain/Policies/EligibilityRules.cs:163>
[S31]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Scheduling/PostgresSchedulerStore.cs:90>
[S32]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Domain/Confirmation/ResultContractPolicy.cs:8>
[S33]: <C:/Users/Administrator/Desktop/ivr/plan/ivr-orther/m8-06-upstream-session-trace-signoff-2026-09-03.md:60>
[S34]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Callbacks/TargetV1CallbackTransport.cs:114>
[S35]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Callbacks/CallbackDispatcher.cs:169>
[S36]: <C:/Users/Administrator/Desktop/ivr/plan/ivr-orther/m8-09-revoke-freshness-decision-pack-2026-09-03.md:43>
[S37]: <C:/Users/Administrator/Desktop/ivr/plan/ivr-orther/m8-10-contact-dial-token-production-decision-pack-2026-09-03.md:18>
[S38]: <C:/Users/Administrator/Desktop/ivr/plan/ivr-orther/m8-11-attempt-policy-production-decision-pack-2026-09-03.md:1>
[S39]: <C:/Users/Administrator/Desktop/ivr/docs/evidence/W-0122/README.md:72>
[S40]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Configuration/IvrOptionsValidator.cs:49>
[S41]: <C:/Users/Administrator/Desktop/ivr/deploy/tts/models/MODELS.lock:1>
[S42]: <C:/Users/Administrator/Desktop/ivr/docs/evidence/W-0189/README.md:17>
[S43]: <C:/Users/Administrator/Desktop/ivr/docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md:1>
[S44]: <C:/Users/Administrator/Desktop/ivr/docs/release/gate-status.yaml:874>
[S45]: <C:/Users/Administrator/Desktop/ivr/deploy/ci/scripts/contract-freeze-verifier.mjs:95>
[S46]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Scheduling/SchedulerCapacity.cs:501>
[S47]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Callbacks/CallbackDeliveryOptions.cs:62>
[S48]: <C:/Users/Administrator/Desktop/ivr/specs/api/openapi/ivr-order-confirmation.v1.yaml:989>
[S49]: <C:/Users/Administrator/Desktop/ivr/plan/ivr-orther/m8-12-external-decision-provenance-dispatch-pack-2026-09-03.md:34>
[S50]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Infrastructure/Telephony/DialTokenResolveLedger.cs:79>
[S51]: <C:/Users/Administrator/Desktop/ivr/src/Ivr.Api/Auth/OrderCoreAllowlistMiddleware.cs:24>
