# Module 8 — Worklist đã hiệu chỉnh sau đối chiếu code

**Ngày rà soát:** 29/08/2026

**Snapshot đối chiếu ban đầu:** main @ **0baed74cd384cd661aed068c263a92ef97ead1f4**

**Cập nhật thực thi gần nhất:** `W-0150` trên baseline `main@b21ec676e490`; trạng thái hiện hành lấy từ tracker/readiness mirror, không lấy từ snapshot ban đầu.

**Mục đích:** dùng để phản hồi bản giao việc cũ và tiếp tục công việc hôm nay/tuần sau.

**Không phải nguồn trạng thái chính thức:** trạng thái chuẩn vẫn nằm trong [prompt execution tracker](../../prompt/_execution/prompt-execution-tracker.md) và [readiness board](../../docs/release/readiness-board.md).

> **Kết luận thẳng:** bản giao việc cũ **không được chấp nhận nguyên trạng**. Nó trộn việc đã xong, việc đang bị chặn và quyết định chưa thuộc thẩm quyền IVR; có đầu việc trùng nhau, mô tả lỗi thời và phần trăm tiến độ không có nguồn chuẩn. Không dùng bản cũ để đánh giá tiến độ hoặc yêu cầu triển khai.

---

## 1. Những điểm phải sửa trong bản giao việc cũ

| Nội dung cũ | Kết luận sau đối chiếu | Cách ghi đúng |
|---|---|---|
| Module 8 hoàn thành khoảng 24% | **Không hợp lệ.** Repo không dùng tỷ lệ cảm tính làm nguồn trạng thái. | Chỉ dùng trạng thái và bằng chứng trong tracker/readiness board. |
| “Phía M8 đã chuẩn”, M3 có thể build và “không còn gì phải chờ” | **Nói quá mức bằng chứng.** Seam vẫn là **TARGET_V1_DRAFT**, chưa có chữ ký hai phía, auth/consumer/sandbox và shared E2E còn thiếu. | Chỉ được gọi là **local candidate**, chưa phải signed seam hay production-ready. |
| B2: softphone lab còn là WIP/untracked | **Lỗi thời.** Các file Asterisk/PJSIP/compose đã tracked; file PowerShell nêu trong bản cũ không tồn tại; worktree tại thời điểm rà soát sạch. | Xóa B2 khỏi active backlog. |
| C1: ma trận program/payment chưa có nguồn nghiệp vụ | **Sai.** Flow 04/05 đã có nguồn cho **24_7 + COD** và **GOLDEN_HOUR + ONLINE**. | Không hỏi Product quyết định lại; chỉ cần M3 ký wire mapping/policy version. |
| C2: còn 5 chỗ Customer Trust Resolver | **Sai số lượng.** Source hiện chỉ còn một tham chiếu tài liệu cần dọn; runtime authority đã được W-0123 xử lý. | Không xây lại trusted-skip resolver; chỉ dọn tài liệu và chốt compatibility. |
| C3: thiếu result code IVR_OPT_OUT | **Sai mô hình.** V0.3 có 11 result codes; opt-out chặn trước cuộc gọi và hiện thuộc **IVR_POLICY_BLOCKED**. | Không tự thêm IVR_OPT_OUT và không suy diễn Rejected thành opt-out. |
| C4, C5 và một phần C7 là ba việc riêng | **Trùng gốc vấn đề.** Cả ba đều xoay quanh upstream/session trace. | Gộp thành một quyết định contract duy nhất về session reference. |
| C11 và C14 là hai việc riêng | **Trùng gốc vấn đề.** Cả hai cùng là revoke/freshness lifecycle. | Gộp thành một workstream có command, ACK, idempotency, race/fencing và audit. |
| D6 attempt policy đã hoàn tất | **Sai trạng thái.** Cấu hình hiện tại mới là **mock-lab-v1 candidate**; production policy còn chờ Owner. | Ghi **LOCAL_CANDIDATE_VERIFIED / OWNER_POLICY_REQUIRED**. |
| “Chỉ có main branch” | **Sai thực tế Git.** Repo còn branch local/remote khác. | Không dùng nhận định này làm bằng chứng phạm vi. |
| Intake/callback “không đổi một byte” | **Sai theo literal diff.** Có thay đổi mô tả/comment/admin; wire shape chính vẫn giữ. | Chỉ được nói **wire shape không đổi**, và contract vẫn là draft. |
| Không được sửa file V0.3 Markdown | **Sai phạm vi bảo toàn.** Vùng bất biến là docs/documents, không phải file clean Markdown đang dùng. | Được sửa tài liệu clean khi có bằng chứng; không tự ý sửa business source bất biến. |
| B4/M8-03 phải “ký ownership rồi bổ sung audit-evidence/capacity-incident endpoints” | **Lỗi thời và giao trùng.** S-03 phía M8 đã ký; W-0128/IR-06 đã giao identity/UI cho M3. Dashboard đã trả capacity incidents; call-job detail đã trả evidence/audit refs; OpenAPI, tests và reference UI đều có. | Không thêm route/UI để diễn lại cái đã tồn tại. M8 bàn giao exact route/field/test contract; M3/Security/Platform phải ký và chạy shared evidence. Route mới chỉ mở khi có signed use case + data/security contract. |

**Phản hồi cần gửi lại bên giao việc:** mọi đầu việc phải chỉ ra source of truth, owner có quyền quyết định, dependency và bằng chứng hoàn tất. Các câu kiểu “đã chuẩn”, “không phải chờ” hoặc phần trăm tự ước lượng sẽ bị bác bỏ nếu không có gate tương ứng.

---

## 2. Trạng thái đã xác minh tại snapshot

- Bản cũ tự bám mốc **79f17b0**. Từ mốc đó tới snapshot hiện tại không có thay đổi dưới **src/**, **tests/**, **admin-ui/** hoặc OpenAPI làm đảo ngược các phát hiện runtime; thay đổi chủ yếu là tài liệu/CI.
- Local test đã chạy xanh:
  - .NET: **490 unit + 233 integration + 24 contract + 8 chaos = 755/755**.
  - Admin UI: **176/176**, lint pass, typecheck pass.
  - OpenAPI lint/validate/drift: pass.
  - Traceability: **TEST_TRACEABILITY_CURRENT=466**.
- Capacity self-test chỉ đạt **PASS_UNCALIBRATED**; mô hình 40/50/60 giây chưa được thay bằng dữ liệu thật.
- Readiness hiện vẫn ở **Rung 0**; readiness board ghi **8/141 ACCEPTED** sau W-0143.
- Gate mirror hiện có **11 external gates** và **23 open decisions**.
- **REAL_CUSTOMER_CALL_ALLOWED=NO**.

Các số trên là snapshot bằng chứng ngày 29/08/2026, không phải phần trăm tiến độ và không thay thế tracker.

---

## 3. Luật thực thi không được vượt

1. **M3 quyết định call/no-call và business classification.** IVR chỉ validate, execute và report.
2. Không tự thêm field, enum, result code, route hay semantics vào shared contract khi M3/Owner chưa ký.
3. Không dùng **Rejected** để suy ra opt-out.
4. Không dựng lại active trusted-skip authority trong IVR; các field cũ chỉ còn mục đích compatibility/audit.
5. Không bật Target V1 delivery, production telephony hoặc secret path chỉ vì local test xanh.
6. Không đổi số migration đã phát hành nếu chưa có target-DB inventory chứng minh an toàn.
7. Không gọi local candidate là **ACCEPTED**, **CONTRACT_LOCKED**, **PRODUCTION_READY** hoặc release-ready.
8. Mọi thay đổi trạng thái phải được cập nhật vào tracker/readiness board bằng bằng chứng cụ thể; file này chỉ là worklist điều hành.

---

## 4. Việc phải làm hôm nay

### TODAY-01 — Gửi lại decision/sign-off pack

**Trạng thái:** **M8_OWNER_SIGNED / HANDOFF_READY / EXTERNAL_SIGNATURES_REQUIRED**

**Ánh xạ bản cũ:** C1, C3, C6 và các quyết định contract của C4..C14.

**Phạm vi:** M3 authority, program wire mapping, admin ownership, session trace, callback auth/consumer, opt-out, revoke/freshness, dial token và attempt policy.

Việc làm:

- Dùng [Module 3 API handover](../../integration-requirements/06-module-3-api-handover.md) làm seam draft.
- Giữ nguyên nguồn nghiệp vụ cho **24_7 + COD** và **GOLDEN_HOUR + ONLINE**; chỉ yêu cầu M3 ký mapping **24_7 → TWENTY_FOUR_SEVEN** và policy version.
- Tách rõ ba hồ sơ C6: admin/auth handoff W-0128; OD-18 đã khóa nội bộ IVR nhưng còn chờ M3 sign-off; errata VoLTE và cách xử lý bản DOCX V0.3 đang lệch bản Markdown theo OD-20.
- Ghi rõ câu hỏi nào thuộc Owner, M3, Security, Legal hoặc Platform. Không đẩy các quyết định đó sang IVR developer.
- Yêu cầu câu trả lời có người ký, ngày ký và scope. Im lặng không được tính là approval.

**Điều kiện hoàn tất phía IVR:** pack hiện hành đã có đúng owner, artifact, stop rule và mẫu phản hồi; không tự đóng quyết định ngoài thẩm quyền.

**Điều kiện đóng các external decision:** từng quyết định có ACCEPTED bằng văn bản và evidence link; nếu chưa có thì tiếp tục giữ **OWNER_SIGNOFF_REQUIRED/BLOCKED_EXTERNAL** trong tracker.

> [!IMPORTANT]
> **HANDOFF TODAY-01 — READY FOR EXTERNAL SIGN-OFF**
>
> - **Pack bàn giao:** [TODAY-01 — Decision / Sign-off Pack hiện hành](today-01-decision-signoff-pack-2026-08-29.md)
> - **Evidence snapshot:** main @ **0baed74cd384cd661aed068c263a92ef97ead1f4**; không nhận W-0139 WIP đang chạy song song vào bằng chứng của task này.
> - **Đã chuẩn bị:** 10 decision sheet cho M3, Owner, CRM/M3.1, Security/Platform, Legal/Privacy và Telephony; kèm artifact bắt buộc, stop rule, routing order và mẫu tin nhắn gửi.
> - **Đã sửa khi đóng gói:** không dùng closure pack draft.18 nguyên trạng; không hỏi lại business pair đã có nguồn; admin handoff bám W-0128; không phát minh session/revoke/opt-out semantics.
> - **Người ký phía Module 8:** **Tôi — Module 8 / Project Owner**, xác nhận trực tiếp ngày **29/08/2026**. Phạm vi chữ ký nằm tại §8 của pack và không thay chữ ký external owner.
> - **Vị trí Owner đã ký:** S-07 chọn phương án A, giữ D-06 revalidation phía M3 là bắt buộc; S-09 không duyệt mock-lab-v1 cho production; S-10 chọn phương án A — thu hồi DOCX lỗi thời, controlled execution còn chờ đồng bộ ledger sau W-0139.
> - **External dispatch:** **NOT_PERFORMED**. **External approval:** **NOT_RECEIVED**.
> - **Tracker mutation:** **NOT_PERFORMED** vì tracker đang có W-0139 WIP song song và chưa có external decision mới để ghi. Trạng thái worklist đã được cập nhật tại đây.
> - **Việc kế tiếp:** Owner gửi đúng sheet cho từng bên; bên nhận trả lại decision + chữ ký + commit/OpenAPI/test/evidence theo mẫu §4 của pack.
> - **Điều kiện IVR resume:** chỉ mở nhánh code tương ứng sau khi đúng owner trả lời đủ artifact. Im lặng hoặc “OK” bằng văn xuôi không phải approval.

### TODAY-02 — Dọn các mâu thuẫn tài liệu còn lại

**Trạng thái:** **DONE — LOCAL_VERIFIED / DOC_CORRECTION_ONLY**

**Ánh xạ bản cũ:** C2 và phần priority/documentation của C7.

Việc làm:

- Xóa hoặc sửa một tham chiếu Customer Trust Resolver còn sót trong [V0.3 clean spec](../../docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md).
- Ghi rõ **priority** hiện là dữ liệu IVR-derived, không phải upstream contract field.
- Đồng bộ wording: M3 là business authority; IVR không sở hữu quyết định trusted skip.
- Không đổi result taxonomy hoặc wire schema trong bước dọn tài liệu này.

**Hoàn tất khi:** exact-search không còn mô tả IVR là active trusted-skip authority và tài liệu không tự mâu thuẫn về priority/session.

> [!IMPORTANT]
> **HANDOFF TODAY-02 — HOÀN TẤT, KHÔNG CÒN CHỖ CHO DIỄN GIẢI SAI AUTHORITY**
>
> - **Evidence snapshot:** `main@0baed74cd384cd661aed068c263a92ef97ead1f4`.
> - **Nguồn đối chiếu:** `OD-18` trong [decisions-log.md](decisions-log.md), compatibility boundary trong [TaskIntakeEndpoint.cs](../../src/Ivr.Api/Intake/TaskIntakeEndpoint.cs), và thứ tự `risk_flags`/synthetic session trong [PostgresSchedulerStore.cs](../../src/Ivr.Infrastructure/Scheduling/PostgresSchedulerStore.cs).
> - **Đã sửa tại:** [MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md](../../docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md) — thay `Trust Gate` bằng M3 Decision Gate; xóa active Customer Trust Resolver; khóa M3 là business authority và IVR chỉ validate/execute/report.
> - **Priority:** active upstream contract không có field `priority`; scheduler chỉ dùng `RiskScore` do IVR suy từ `risk_flags` cho thứ tự thực thi, tuyệt đối không được đảo quyết định call/skip của M3.
> - **Session:** active intake không có upstream `session_id`; `capacity_incident.session_id` hiện là internal/synthetic. Việc thêm session upstream vẫn thuộc S-04/M8-06 và chỉ được triển khai sau khi contract được ký.
> - **Exact-search:** **PASS** — `0` active hit trong V0.3 cho `Customer Trust Resolver`, `Customer trust/risk resolver`, `skip IVR cho khách cũ` và `risk cao theo Customer Trust`.
> - **Không thay đổi:** result taxonomy, OpenAPI/wire schema, DB schema và runtime code. Các tài liệu historical/superseded vẫn giữ nguyên để bảo toàn audit trail; không được lấy chúng làm contract hiện hành.
> - **Bảo toàn WIP song song:** hunk B-06 observability của W-0139 trong cùng file được giữ nguyên, không thuộc TODAY-02.
> - **Người xác nhận hoàn tất phía Module 8:** **Tôi — Module 8 / Project Owner**, ngày **29/08/2026**. Đây là xác nhận cho doc correction, không thay chữ ký contract của M3 hoặc external owner.
> - **Việc kế tiếp:** TODAY-03. Không mở code cho S-04/M8-06 chỉ vì tài liệu đã sạch.

### TODAY-03 — Chốt phần TTS có thể làm ngay

**Trạng thái:** **M8_LOCAL_COMPLETE / HANDOFF_READY / BLOCKED_EXTERNAL**

**Ánh xạ bản cũ:** B3.

Điều đã có:

- Ba voice profile đã có bằng chứng **OWNER_ACCEPTED**; không mở lại vòng chọn voice nếu không có yêu cầu thay đổi chính thức.

Việc còn làm:

- Owner chuyển ba phiếu đã khóa đến đúng Legal/Privacy, Security/Release và Platform/Infra/Telephony; hiện chưa có bằng chứng dispatch/receipt.
- Tổ chức Owner nghe đủ 12 fixed segments và các mối nối fixed ↔ dynamic đại diện.
- Chạy và ghi bằng chứng 6 cuộc gọi MicroSIP.
- Thực hiện retention/rollback drill.
- Chốt target hardware, internal mirror/topology và CVE disposition.

**Hoàn tất khi:** đủ approval/evidence theo gate; không dùng container proof thay cho human listening, Legal/Security hoặc real-call proof.

> [!IMPORTANT]
> **HANDOFF TODAY-03 — PHẦN LOCAL ĐÃ XONG; PRODUCTION VẪN BỊ CHẶN**
>
> - **Evidence snapshot:** `main@0baed74cd384cd661aed068c263a92ef97ead1f4`.
> - **Gói bàn giao:** [today-03-tts-handoff-pack-2026-08-29.md](today-03-tts-handoff-pack-2026-08-29.md) — routing ba bên, bảng nghe 12 fixed segments, bảng 6 MicroSIP calls, retention/rollback checklist và stop rule.
> - **Voice selection:** đã xác minh `OWNER_ACCEPTED 2026-08-28`; đủ 11 candidate; chọn Bắc Ngọc Linh, Trung Ngọc Trân, Nam Mỹ Duyên. Không mở lại nếu binding không đổi.
> - **Governance defect đã sửa:** commit `2a4f45d` tự đổi `MODELS.lock.legal_gate=PASS` bằng chữ ký `Owner module IVR`, sai authority và làm hỏng hash binding. TODAY-03 đã khôi phục `OWNER_DATA_REQUIRED`; lock hash trở lại đúng `bba41ea…`, Owner manifest PASS mà không bị sửa. CI/prod gate nay còn bắt buộc `decision_authority=LEGAL_PRIVACY` kèm người/ngày ký và `approval_reference`.
> - **Local verification:** provenance selftest PASS, gồm negative `legal-authority`; current output là `release_blockers=LEGAL,INTERNAL_MIRROR`. Nonprod bundle verify PASS với đúng hai blocker; production verify **EXPECTED_FAIL**.
> - **Impact boundary:** hai gate symbol đã phân tích ở mức **LOW**. Repo-wide unstaged detect đang báo `CRITICAL` vì 56-file W-0139/concurrent WIP; đó không phải blast radius của TODAY-03 và không được gộp vào commit này.
> - **Ba phiếu:** đã chuyển trạng thái tài liệu sang `READY_TO_DISPATCH / NOT_SENT / EXTERNAL_RESPONSE_REQUIRED`; không ghi “chờ trả lời” khi chưa có bằng chứng đã gửi.
> - **External dispatch/approval:** `NOT_PERFORMED / NOT_RECEIVED`.
> - **Owner lab evidence còn thiếu:** 12 fixed-segment listening, 6 fake-order MicroSIP calls, media round-trip, retention drill và rollback drill đều `NOT_RUN`.
> - **External gate còn thiếu:** Legal/Privacy; Security/Release disposition cho 13 HIGH + 3 CRITICAL; Platform/Infra/Telephony cho internal mirror, target hardware và `OD-VOICE-08`.
> - **Người ký handoff phía Module 8:** **Tôi — Module 8 / Project Owner**, ngày **29/08/2026**. Chữ ký này không thay chữ ký của ba nhóm external.
> - **Release boundary:** `REAL_CUSTOMER_CALL_ALLOWED=NO`; không được ghi `ACCEPTED` hoặc `PRODUCTION_READY`.
> - **Việc kế tiếp trong worklist:** TODAY-04 target-DB preflight; TODAY-03 vẫn mở ở nhánh external/human evidence.

### TODAY-04 — Chuẩn bị và chạy target-DB preflight nếu có quyền truy cập

**Trạng thái:** **COMPLETE_AS_BLOCKED — PREFLIGHT_READY / OWNER_DATA_REQUIRED / TARGET_DB_NOT_RUN**

**Nguồn gate:** OD-18/W-0123 target-DB evidence; đây không phải C6 của bản cũ.

Việc làm:

- Dùng OD18 preflight để kiểm tra inventory migration và legacy schema trên target DB.
- Không truyền credential trên command line; dùng secret mechanism được duyệt.
- Ghi lại target, thời gian, người chạy, kết quả và bằng chứng.
- Nếu chưa có credential/authority, dừng ở **OWNER_DATA_REQUIRED**; không đoán trạng thái DB và không đổi tên migration.

**Hoàn tất khi:** có target-DB evidence hoặc blocker được Owner xác nhận.

Kết quả triển khai `29/08/2026`:

- Đã bổ sung migration inventory, legacy column/constraint inventory và schema-drift stop rules vào
  OD18 preflight. Query đã khóa tại SHA-256
  `203c5fd173384cc0c09e51b115ff841fdf40eb91b8cd6510d7a962c84961dd7a`.
- Static gate: `PASS` — PowerShell 0 parse error; SQL 18/18 câu `SELECT`, 0 non-SELECT.
- PostgreSQL integration gate `IT-M3-AUTHORITY-13`: `PASS` — 1/1 trên migrated test schema của
  working tree hiện tại; không phải target DB hoặc immutable release candidate.
- Target run: **không chạy**. Access audit không tìm thấy `psql`, secret provider, target endpoint,
  target credential hoặc authority/ticket. PostgreSQL local không có authority xác nhận là target
  IVR nên không được dùng thay.
- Handoff/evidence pack:
  [`today-04-target-db-preflight-handoff-2026-08-29.md`](today-04-target-db-preflight-handoff-2026-08-29.md).

> ## **HANDOFF TODAY-04 — OWNER BLOCKER CONFIRMED**
>
> **Kết luận:** phần chuẩn bị trong repo đã xong; target gate vẫn mở ở
> `OWNER_DATA_REQUIRED / TARGET_DB_NOT_RUN`. Không có target count thì không được tuyên bố DB sạch,
> không được đổi migration và không được dùng local test để đóng gate.
>
> **Người ký:** **Tôi — Module 8 / Project Owner** · **29/08/2026**.
>
> Chữ ký xác nhận blocker/handoff, không thay target authority hoặc target-DB evidence.

### TODAY-05 — Cập nhật nguồn trạng thái, không tạo ledger thứ hai

**Trạng thái:** **DONE — W-0140 TESTS_PASS / STATUS_SOURCES_SYNCED / EXTERNAL_GATES_UNCHANGED**

Việc làm:

- Chỉ đổi trạng thái trong tracker/readiness board sau khi TODAY-01..04 có bằng chứng mới.
- Không ghi phần trăm.
- Không chuyển blocked item thành done chỉ vì đã viết proposal hoặc local test pass.

Kết quả triển khai `29/08/2026`:

- Đã cấp `W-0140` trong tracker chuẩn; không tạo tracker/backlog mới.
- Chỉ một status cũ được đổi: `W-0122 IN_PROGRESS → BLOCKED_EXTERNAL`. Phần local đã xong nhưng
  listening/MicroSIP/retention/Legal/Security/Platform evidence chưa có.
- `W-0123` và `W-0125` giữ `TESTS_PASS`; target DB vẫn
  `OWNER_DATA_REQUIRED / TARGET_DB_NOT_RUN`.
- `W-0137` giữ `TESTS_PASS`; `OD-20` được cập nhật thành
  `DECIDED / OPTION_1_WITHDRAW` theo chữ ký Owner. File DOCX chưa bị di chuyển/xoá trong TODAY-05.
- Readiness mirror được sinh lại từ tracker: Rung 0; `8/138 ACCEPTED`; `90 TESTS_PASS`;
  `16 BLOCKED_EXTERNAL`; 11 external gate; 23 open decision; real customer call vẫn `NO`.
- Evidence: [`docs/evidence/W-0140/README.md`](../../docs/evidence/W-0140/README.md).
- Follow-up đã thực thi tại W-0141: DOCX V0.3 lỗi thời được rename `_SUPERSEDED`, không xóa và
  không đổi bytes. Xem [`docs/evidence/W-0141/README.md`](../../docs/evidence/W-0141/README.md).

> ## **HANDOFF TODAY-05 — CANONICAL STATUS SYNCED, KHÔNG TÔ HỒNG TIẾN ĐỘ**
>
> Tracker tiếp tục là nguồn duy nhất; readiness board/gate-status chỉ là mirror được generate.
> Không Work ID nào được nâng `ACCEPTED`, không external gate nào được đóng và không có phần trăm.
>
> **Người ký:** **Tôi — Module 8 / Project Owner** · **29/08/2026**.
>
> Chữ ký xác nhận status audit/handoff; không thay M3, Legal, Security, Platform hoặc Release owner.

### FOLLOW-UP-01 — W-0141 thực thi OD-20, thu hồi DOCX V0.3 lỗi thời

**Trạng thái:** **DONE — TESTS_PASS / WITHDRAWAL_EXECUTED / EXTERNAL_GATES_UNCHANGED**

Kết quả:

- Đã đổi tên `MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.docx` thành
  `MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN_SUPERSEDED.docx`.
- Không xóa hoặc sửa binary: kích thước trước/sau `45.101` byte; SHA-256 cùng là
  `b2b95c9cb62e14b8138538b8447117040207641e5c565e4e1881f3a55af0935c`.
- `OD-20` đã chuyển `IMPLEMENTED / OPTION_1_WITHDRAW`; bản Markdown V0.3 là nguồn có hiệu lực.
- W-0137/W-0141 vẫn `TESTS_PASS`; không Work ID nào được nâng `ACCEPTED`.
- Readiness sau mirror: Rung 0; `8/139 ACCEPTED`; `91 TESTS_PASS`; `16 BLOCKED_EXTERNAL`;
  11 external gate; 23 open decision; real customer call vẫn `NO`.
- Evidence: [`docs/evidence/W-0141/README.md`](../../docs/evidence/W-0141/README.md).

> ## **HANDOFF W-0141 — DOCX LỖI THỜI ĐÃ ĐƯỢC THU HỒI KHỎI TÊN HIỆN HÀNH**
>
> Artifact `_SUPERSEDED` chỉ được giữ để audit/recovery. Không được gửi nó cho vendor hoặc viện dẫn
> như bản Word hiện hành.
>
> **Người ký:** **Tôi — Module 8 / Project Owner** · **29/08/2026**.
>
> Chữ ký xác nhận controlled withdrawal; không thay external approval hoặc Release acceptance.

---

## 5. Workstream duy nhất cho hôm nay và tuần sau

| ID mới | Ánh xạ bản cũ | Trạng thái đúng | Việc tiếp theo |
|---|---|---|---|
| M8-01 Capacity calibration | B1 | **DATA_INTAKE_READY / BLOCKED_EXTERNAL / CALIBRATION_NOT_RUN** | W-0142 đã sửa đường calibrated để không tính cooldown hai lần và khóa schema/stop rule. Còn phải nhận W-0008 per-attempt timing, M3 arrival buckets/session, production attempt policy + outcome rate và Infra reserve/failure factor; sau đó mới thay assumption, chạy calibrated model và trình Owner chốt kênh. PT-CAP-02 đã pass nhưng không phải capacity measurement. |
| M8-02 TTS production closure | B3 | **LOCAL_HANDOFF_READY / OWNER/LEGAL/SECURITY/PLATFORM_REQUIRED** | Local gate đã fail-closed đúng authority; handoff pack đã khóa. Vẫn phải hoàn thành listening, 6 MicroSIP calls, retention/rollback, target hardware, mirror/topology và CVE disposition trước khi bật production config. |
| M8-03 Admin audit/capacity surface | B4, liên quan admin handoff trong C6 | **M8_LOCAL_COMPLETE / EXISTING_SURFACE_VERIFIED / M3_SECURITY_ACCEPTANCE_REQUIRED** | W-0143 xác nhận endpoint/field/test đã tồn tại và bác yêu cầu làm lại. M3 phải nhận IR-06 §4A, regenerate client, giữ token trong BFF, map role→tier và chạy shared E2E; Security/Platform phải giao custody/network/rotation evidence. Không thêm raw/global audit-evidence hoặc capacity route nếu chưa có signed use case/data contract. |
| M8-04 Production telephony adapter | B5 | **DT04_LOCAL_COMPLETE / PRODUCTION_ADAPTER_BLOCKED_EXTERNAL** | W-0144 đã sửa/persist policy auto-disable 3 lỗi theo từng kênh trong cửa sổ 10 phút và phủ provider failure + lease-expiry. Production adapter vẫn chưa được phép viết: phải có vendor đã chọn, vendor code/disposition matrix, recording-off proof, trust boundary, resolver/credential custody và Security/Platform sign-off. |
| M8-05 Program/result contract sign-off | C1, C3 | **M8_OWNER_SIGNED / M3_PRODUCT_SIGNOFF_REQUIRED / PRODUCTION_POLICY_PENDING** | W-0145 đã khóa receiver matrix, ba wire mapping và taxonomy `11 contract / 9 runtime producer / 6 final callback / 2 pre-call compatibility`; sửa tài liệu cũ gán sai window-expired cho Sales. M3 phải giao producer/consumer commit + CDC; Product/Order Core phải ký production policy. Không phát minh result code mới. |
| M8-06 Upstream session trace | C4, C5, phần C7 | **M8_POSITION_SIGNED / M3_CONTRACT_SIGNOFF_REQUIRED / CODE_NOT_AUTHORIZED** | W-0146 đề xuất đúng một upstream field `golden_hour_session_id`: required/non-null cho Golden Hour, prohibited/absent cho 24/7. Giữ `capacity_incident.session_id` là capacity scope ID nội bộ; không map đè. Chỉ mở code/OpenAPI/DB sau chữ ký M3 + producer CDC/cutover. |
| M8-07 Target V1 shared callback | C8, C9 | **M8_LOCAL_CALLBACK_READY / RETRY_AFTER_FIXED / M3_SECURITY_PLATFORM_REQUIRED / SHARED_E2E_NOT_RUN / DELIVERY_DISABLED** | W-0147 đã trace callback seam và sửa defect `429` bỏ qua `Retry-After`; local retry nay giữ immutable key/body và không chạy sớm hơn server delay. M3 phải giao consumer/OAS/CDC; Security giao auth/custody; Platform giao sandbox/network/TLS; sau đó chạy full shared E2E exact SHA trước khi xem xét gỡ guard. |
| M8-08 Opt-out feedback loop | C10 | **M8_POSITION_SIGNED / CURRENT_LOOP_NOT_WIRED / EXPLICIT_ONLY_V1_PROPOSED / CRM_M3_LEGAL_SIGNOFF_REQUIRED / RUNTIME_NOT_AUTHORIZED** | W-0148 xác nhận inbound `call_restriction` đã fail-closed, nhưng outbound chỉ có dormant policy/proposer: không counter/orchestrator/delivery/ACK/terminal retention. `Rejected` và DTMF `0` không phải opt-out; threshold 2/3 chỉ TEST_ONLY candidate. CRM/M3/Legal/Product phải trả `OPT-01..11` trước code. |
| M8-09 Revoke/freshness lifecycle | C10, C11, C13 | **W-0149 EVIDENCE_SUBMITTED / CURRENT_OPTION_A_BEHAVIOR_PRESENT / OWNER_PROVENANCE_REQUIRED / M3_D06_RUNTIME_NOT_FOUND / OPTION_B_NOT_IMPLEMENTED / CODE_NOT_AUTHORIZED** | Audit xác nhận current IVR chỉ kiểm snapshot lúc intake; scheduler/claim→dial không recheck business state và không có revoke route/state/fence. Option A chỉ an toàn khi M3 thực thi D-06, nhưng consumer/runtime đó không thấy ở snapshot M3 hiện có. Owner/M3 phải ký A/B/hybrid và trả `RVK-01..12`; chưa sửa code. |
| M8-10 Contact/dial token production path | B5, C12 | **W-0150 EVIDENCE_SUBMITTED / LOCAL_PRIVACY_SEAM_PRESENT / PRODUCTION_PATH_FAIL_CLOSED / CONTRACT_RUNTIME_MISMATCH_FOUND / EXTERNAL_DECISIONS_REQUIRED / CODE_NOT_AUTHORIZED** | Audit đã khóa contact requiredness mismatch, TTL equality, scalar/per-attempt reuse, opaque destination output, production DI/secret/network/audit gaps và M3 producer chưa thấy. M3/Security/Platform/Telephony ký `DTK-01..15` trước mọi adapter/vault/OpenAPI/runtime change. |
| M8-11 Attempt policy | D6 | LOCAL_CANDIDATE_VERIFIED / OWNER_POLICY_REQUIRED | Owner ký attempt count, spacing, quiet hours, terminal behavior và policy version; mock-lab-v1 không được coi là production policy. |
| M8-12 Authority/compatibility cleanup | C2, D9 | **DONE — LOCAL_VERIFIED / DOC_CLEANUP_ONLY** | W-0123 đã loại active trusted-skip authority khỏi runtime; TODAY-02 đã dọn reference còn sót. Legacy fields tiếp tục read-only cho compatibility/audit. |

### M8-01 — W-0142 capacity calibration preflight

> ## **HANDOFF M8-01 — ĐƯỜNG CHẠY ĐÃ SẴN, DỮ LIỆU THẬT CHƯA CÓ**
>
> - **Đã làm:** đối chiếu W-0008/W-0131..0134 và OD-19/M8-OD-C; sửa calibrated path để
>   `model/runtime = channel occupancy`, còn `spec cycle = occupancy + cooldown`; cập nhật lab
>   timing template và công thức procurement theo rolling window.
> - **Chưa làm và không được nói là đã làm:** real-call calibration, arrival-profile calibration,
>   multi-channel load/failover, chốt 4/12/32 kênh hoặc production readiness.
> - **Bên phải giao dữ liệu:** Business/M3 (session + arrival buckets), Product/Order Core
>   (production attempt policy), W-0008 lab/pilot (per-attempt timing + outcome distribution),
>   Infra/vendor (cooldown/quota/reserve/failure factor).
> - **Reject thẳng:** chỉ giao “800–1200 cuộc/phiên”, “45 phút”, một số trung bình, kết quả
>   simulator/PT-CAP-02 hoặc phép nhân từ 1 SIM mà không có dòng nguồn/arrival profile.
> - **Evidence/runbook:** [W-0142](../../docs/evidence/W-0142/README.md).
>
> **Trạng thái:** **`EVIDENCE_SUBMITTED / DATA_INTAKE_READY / BLOCKED_EXTERNAL`**.
>
> **Người ký:** **Tôi — Module 8 / Project Owner** · **29/08/2026**.
>
> Chữ ký xác nhận handoff và stop rule; không ký thay external owner và không phê duyệt số kênh.

### M8-03 — W-0143 admin audit/capacity surface reconciliation

> ## **HANDOFF M8-03 — SURFACE ĐÃ CÓ; KHÔNG GIAO IVR LÀM LẠI**
>
> - **Kết luận:** mô tả cũ “ký ownership rồi bổ sung endpoint” đã lỗi thời. S-03 phía M8 đã ký;
>   W-0128/IR-06 §4A đã khóa M3 sở hữu operator identity/UI và IVR sở hữu admin API.
> - **Capacity:** `GET /dashboard` đã trả `open_incidents` + `missed_deadline_count`; integration
>   test đã seed/assert capacity incident và reference dashboard đã render bảng incident.
> - **Audit/evidence:** `GET /call-jobs/{ivrCallJobId}/detail` đã trả `evidence_refs` +
>   `audit_refs`; integration test và reference call-detail UI đã dùng trực tiếp.
> - **Không làm:** không thêm endpoint tổng quát để dump audit/evidence; không dựng hoặc deploy
>   IVR-hosted UI; không đưa admin token xuống browser; không nhận lại identity/RBAC của M3.
> - **M3 phải làm:** ký §4A, regenerate client draft.22, map role/claim → tier, giữ token trong BFF,
>   map actor từ authenticated subject và chạy shared positive/negative E2E.
> - **Security/Platform phải làm:** cung cấp secret custody/rotation, selector/NetworkPolicy/ingress
>   thật và credential smoke. Chưa có các artifact này thì không được ghi `ACCEPTED`.
> - **Evidence/contract handoff:** [W-0143](../../docs/evidence/W-0143/README.md).
>
> **Trạng thái:** **`EVIDENCE_SUBMITTED / M3_SECURITY_ACCEPTANCE_REQUIRED`**.
>
> **Người ký:** **Tôi — Module 8 / Project Owner** · **29/08/2026**.
>
> Chữ ký này xác nhận phần IVR và stop rule; không thay chữ ký M3, Security, Platform hoặc Privacy.

### M8-04 — W-0144 DT-04 window enforcement và production-adapter preflight

> ## **HANDOFF M8-04 — DT-04 ĐÃ SỬA; PRODUCTION ADAPTER VẪN BỊ CHẶN**
>
> - **Đã làm:** thêm `failure_window_started_at` nullable; dùng một policy chung cho provider failure
>   và lease-expiry; lỗi thứ ba trong 10 phút chuyển `HEALTH_FAILED`; healthy hoặc khoảng cách hơn
>   10 phút reset. Migration up/down và compatibility gate đã chạy trên PostgreSQL.
> - **Đã test:** provider failure trong/hết cửa sổ, healthy reset, lease-expiry chạm ngưỡng và chaos
>   `CHAOS-SIM-03`. Không dùng test mock/chaos này để giả nhận là đã quay qua vendor thật.
> - **Không làm:** không dựng `ProductionSimGateway`, không nối E.164/resolver, không thêm secret,
>   không đo caller-ID/DTMF/recording/capacity và không bật `PRODUCTION_REAL`.
> - **Bên giao task phải cung cấp:** tên + API/spec/credential sandbox của vendor; bảng code thô →
>   11 disposition; xác nhận recording OFF; resolver/trust-boundary đã Security ký; Vault/KMS owner,
>   rotation/audit; caller-ID/DTMF/health/disable capability; kết quả lab R-02 và scorecard R-04.
> - **Reject thẳng:** “chọn vendor sau”, screenshot marketing, credential gửi tay, mapping suy đoán,
>   hoặc yêu cầu code production trước khi các artifact trên tồn tại.
> - **Evidence:** [W-0144](../../docs/evidence/W-0144/README.md).
>
> **Trạng thái:** **`TESTS_PASS / DT04_LOCAL_COMPLETE / PRODUCTION_ADAPTER_BLOCKED_EXTERNAL`**.
>
> **Người ký:** **Tôi — Module 8 / Project Owner** · **29/08/2026**.
>
> Chữ ký phê duyệt semantics DT-04 và phần local; không ký thay vendor, Security, Platform,
> Procurement hoặc Release.

### M8-05 — W-0145 program/result contract sign-off

> ## **HANDOFF M8-05 — M8 ĐÃ KÝ; M3/PRODUCT CHƯA GIAO ARTIFACT THÌ CHƯA ĐƯỢC NHẬN LÀ XONG**
>
> - **Program đã khóa:** chỉ nhận `GOLDEN_HOUR + ONLINE` và
>   `TWENTY_FOUR_SEVEN + COD`. M3 map `24_7 → TWENTY_FOUR_SEVEN`,
>   `PHONE_VALID → VALID`, `ELIGIBLE_FOR_IVR → ELIGIBLE`; IVR không nhận alias để che lỗi producer.
> - **Result đã khóa:** giữ đúng 11 code. Runtime IVR có 9 producer path; 6 final result vào callback
>   outbox; `NO_ANSWER_ATTEMPT`, `WRONG_INPUT`, `TECHNICAL_EXCEPTION` là non-final; hai blocked code
>   là compatibility/pre-call và bị cấm gửi như call result.
> - **Đã sửa factual error:** `IVR_CONFIRMATION_WINDOW_EXPIRED` hiện do scheduler IVR persist final
>   result + enqueue callback. Core vẫn phải revalidate và là bên duy nhất đổi order state.
> - **M3 phải giao:** assembler commit, producer CDC, generic callback consumer cho cả hai program,
>   ACK/idempotency/revalidation contract, auth/sandbox và shared E2E. “OK” bằng văn xuôi không tính.
> - **Product/Order Core phải giao:** production `attempt_policy_version`, window/attempt/offsets,
>   source-of-truth và CDC version-parameters. `mock-lab-v1` không được promote thành production.
> - **Reject thẳng:** thêm alias/result code cho vừa code M3, gọi local mock là integration, hoặc gộp
>   chữ ký M8 thành chữ ký của M3/Product/Security.
> - **Gói ký:** [M8-05 contract sign-off](m8-05-program-result-contract-signoff-2026-09-03.md).
> - **Evidence:** [W-0145](../../docs/evidence/W-0145/README.md).
>
> **Trạng thái:** **`EVIDENCE_SUBMITTED / M8_OWNER_SIGNED / M3_PRODUCT_SIGNOFF_REQUIRED / SHARED_E2E_NOT_RUN`**.
>
> **Người ký:** **Tôi — Module 8 / Project Owner** · **03/09/2026**.
>
> Chữ ký này xác nhận contract/stop rule phía Module 8; không đóng `G-CONTRACT`, `G-POLICY`,
> không ký thay M3/Product/Security và không cho phép real customer call.

### M8-06 — W-0146 upstream session trace

> ## **HANDOFF M8-06 — M8 ĐÃ CHỌN MỘT FIELD; KHÔNG ĐƯỢC TRỘN BUSINESS SESSION VỚI CAPACITY SCOPE ID**
>
> - **Đề xuất M8:** đúng một upstream wire field `golden_hour_session_id`, lấy theo master
>   traceability; không dùng alias `session_id` hoặc `source_session_id`.
> - **Program semantics:** Golden Hour bắt buộc/non-null; 24/7 phải vắng mặt, kể cả `null` cũng
>   không hợp lệ. M3/Golden Hour Core phát ID trước khi tạo task và giữ ổn định qua retry/replay.
> - **Factual boundary:** active task/OpenAPI/domain/task DB/job DB chưa có upstream session.
>   `capacity_incident.session_id` hiện nhận `MOCK-SCHED-*`, `SCHED-*`, `SCHED-DEADLINE-*`,
>   `ADMIN-QUEUE-*` và unavailable/error marker; có incident `ProgramCode=ALL`.
> - **Cấm map đè:** current `capacity_incident.session_id` tiếp tục là internal scope ID. Sau chữ
>   ký chỉ được thêm cột nullable riêng `golden_hour_session_id` cho task/job/incident liên quan.
> - **Migration/cutover:** additive nullable store phase trước; M3 producer + CDC/shared E2E; sau
>   đó mới enforce required cho Golden Hour. Không backfill từ task/order/correlation/internal ID.
> - **M3 phải giao:** signer/name/date/scope, xác nhận namespace/format/uniqueness/stability,
>   producer commit + client revision, OpenAPI acceptance, cutover/rollback và CDC exact SHA.
> - **Reject thẳng:** “cứ dùng `session_id` cho gọn”, gửi hai alias, code trước ký sau, hoặc gọi
>   required-field cutover là non-breaking.
> - **Gói ký:** [M8-06 upstream session sign-off](m8-06-upstream-session-trace-signoff-2026-09-03.md).
> - **Evidence:** [W-0146](../../docs/evidence/W-0146/README.md).
>
> **Trạng thái:** **`EVIDENCE_SUBMITTED / M8_POSITION_SIGNED / M3_CONTRACT_SIGNOFF_REQUIRED / CODE_NOT_AUTHORIZED`**.
>
> **Người ký:** **Tôi — Module 8 / Project Owner** · **03/09/2026**.
>
> Chữ ký này khóa đề xuất và stop rule phía M8; không ký thay M3, không đóng `G-CONTRACT`, không
> cho phép sửa shared contract/DB hoặc gọi khách thật.

### M8-07 — W-0147 Target V1 shared callback

> ## **HANDOFF M8-07 — M8 ĐÃ SỬA PHẦN CỦA M8; CONSUMER/AUTH/SANDBOX KHÔNG PHẢI THỨ CÓ THỂ GIAO MỒM**
>
> - **Đã làm:** trace final result → immutable outbox → dispatcher → Target transport →
>   ACK/retry/circuit → audit/review; xác nhận Target V1 dùng cho cả Golden Hour + 24/7 và current
>   Golden Hour endpoint chỉ là compatibility.
> - **Defect đã sửa:** `429` trước đây retryable nhưng bỏ qua `Retry-After`; nay transport mang
>   positive server delay sang dispatcher và retry không chạy sớm hơn header. Retry budget, payload,
>   hash, callback ID và idempotency key không đổi.
> - **Local proof:** focused callback unit `38/38`, Sales contract `20/20`; full gate/evidence ghi tại
>   W-0147. Local mock/fake không phải M3 integration.
> - **M3 phải giao:** consumer commit cho đúng generic endpoint, authoritative OAS, ACK/idempotency/
>   revalidation decision, CDC và owner/signature. Endpoint compat không được dùng để lấp 24/7.
> - **Security/Platform phải giao:** issuer/JWKS/audience/scope/TTL/rotation/mTLS, secret reference,
>   reachable sandbox, DNS/TLS/network policy, smoke và rollback/runbook.
> - **Shared E2E bắt buộc:** hai program, accepted, exact replay, changed-body conflict, stale,
>   block/review, auth, invalid, `429 Retry-After`, outage/circuit/recovery; report ghim SHA hai repo.
> - **Reject thẳng:** yêu cầu M8 dựng consumer Sales, gỡ fail-start guard vì unit test xanh, dùng GH
>   compat cho 24/7, gọi WireMock/Postman là shared E2E, hoặc coi `ACCEPTED` là order đã confirmed.
> - **Gói ký:** [M8-07 callback handoff](m8-07-target-v1-shared-callback-handoff-2026-09-03.md).
> - **Evidence:** [W-0147](../../docs/evidence/W-0147/README.md).
>
> **Trạng thái:** **`M8_LOCAL_CALLBACK_READY / RETRY_AFTER_FIXED / M3_SECURITY_PLATFORM_REQUIRED / SHARED_E2E_NOT_RUN / DELIVERY_DISABLED`**.
>
> **Người ký:** **Tôi — Module 8 / Project Owner** · **03/09/2026**.
>
> Chữ ký này xác nhận local callback behavior, fix và stop rule; không ký thay M3/Security/Platform/
> Release, không đóng external gate và không cho phép real customer call.

### M8-08 — W-0148 opt-out / suppression

> ## **HANDOFF M8-08 — ĐỪNG GỌI HAI COMPONENT ĐỨNG RIÊNG LÀ “FEEDBACK LOOP”**
>
> - **Inbound:** M3/CRM cung cấp `call_restriction`; IVR chặn restricted/unknown/unavailable theo
>   fail-closed. Đây là pre-call veto, không phải call result.
> - **Rejected:** giữ `NO_ANSWER` counted + review. Không phải customer cancellation và không phải
>   explicit opt-out; DTMF `0/1` cũng không được đổi nghĩa.
> - **Dormant local pieces:** capture review item, `OptOutSuppressionPolicy` và queue-only proposer có
>   test, nhưng production không gọi policy/proposer; không có counter, stable CRM key, orchestration,
>   delivery, ACK hoặc reversal.
> - **Queue chưa vận hành:** proposal ở `PENDING_CRM`; admin mutation chỉ nhận `OPEN`; không writer
>   nào chuyển `ACCEPTED_BY_CRM`; unresolved row chưa có terminal retention path.
> - **Threshold:** `AbsoluteFloor=2`/`Default=3` chỉ `TEST_ONLY_CANDIDATE`, không phải policy đã ký.
> - **Đề xuất M8:** V1 explicit-only; chỉ customer action có wording/script/proof đã Product + Legal
>   ký mới được tạo proposal. Inference từ weak signal phải là contract V2 riêng.
> - **CRM current snapshot:** registry/read/user-consent primitives có thật, nhưng chưa có signed
>   service proposal contract cho M3/IVR. IVR V1 vẫn bị architecture guard cấm CRM mutation egress.
> - **External phải giao:** `OPT-01..OPT-11` — explicit signal, threshold/window, identity key,
>   route, idempotency, ACK, writer authority, reversal, retention, freshness và admin permission.
> - **Reject thẳng:** thêm `IVR_OPT_OUT`, map repeated rejection thành consent mutation, tạo bảng
>   trước retention/key contract, gọi thẳng CRM hoặc coi W-0034 test là shared integration.
> - **Gói ký:** [M8-08 opt-out/suppression decision pack](m8-08-opt-out-suppression-decision-pack-2026-09-03.md).
> - **Evidence:** [W-0148](../../docs/evidence/W-0148/README.md).
>
> **Trạng thái:** **`EVIDENCE_SUBMITTED / M8_POSITION_SIGNED / CURRENT_LOOP_NOT_WIRED / EXPLICIT_ONLY_V1_PROPOSED / CRM_M3_LEGAL_SIGNOFF_REQUIRED / RUNTIME_NOT_AUTHORIZED`**.
>
> **Người ký:** **Tôi — Module 8 / Project Owner** · **03/09/2026**.
>
> Chữ ký này khóa M8 position/stop rule; không ký thay CRM/M3/Legal/Product/Security, không mở
> runtime hoặc cho phép real customer call.

### M8-09 — W-0149 revoke / recall / freshness

> ## **HANDOFF M8-09 — BEHAVIOR A ĐANG CÓ, NHƯNG LƯỚI AN TOÀN D-06 CHƯA CÓ BẰNG CHỨNG RUNTIME**
>
> - **Current IVR:** chỉ `POST /tasks`; re-POST body mới conflict, không phải update. Snapshot freshness
>   được kiểm lúc intake; scheduler và pre-dial fence không đọc current recall/sale-lock/order state.
> - **Option A:** khớp behavior hiện tại và trade-off ghi trong `OD-17`: stale call có thể xảy ra,
>   còn M3 phải revalidate mọi callback trước business transition.
> - **Gap quan trọng:** snapshot M3 `PhucApu@a3aad246d986` không có exact hit cho generic Target V1
>   callback consumer, `BLOCKED_BY_CORE`/`REJECTED_STALE` hoặc revoke path. Fixture IVR tự ghi blocked
>   ACK không phải shared runtime proof.
> - **Option B:** chưa có revoke command, persisted generation/state, claim filter, pre-dial fence,
>   active-call policy hoặc ACK contract. Chỉ thêm scheduler condition không đóng claim→dial race.
> - **Admin controls:** queue pause là global operational control; terminate chỉ cho live attempt.
>   Không tái dùng hai đường này thành business revoke.
> - **Owner phải trả:** `RVK-01..RVK-12` về strategy, triggers, authority, command/auth, version,
>   idempotency/order, state/race, fencing, active-call behavior, ACK/result, audit/retention và rollout.
> - **Gói ký:** [M8-09 revoke/freshness decision pack](m8-09-revoke-freshness-decision-pack-2026-09-03.md).
> - **Evidence:** [W-0149](../../docs/evidence/W-0149/README.md).
>
> **Trạng thái:** **`EVIDENCE_SUBMITTED / CURRENT_OPTION_A_BEHAVIOR_PRESENT /
> M8_POSITION_RECORDED / OWNER_PROVENANCE_REQUIRED / M3_D06_RUNTIME_NOT_FOUND /
> OPTION_B_NOT_IMPLEMENTED / CODE_NOT_AUTHORIZED`**.
>
> Không sửa source/OpenAPI/DB trong W-0149; không bật Target delivery hoặc real customer call.

### M8-10 — W-0150 contact / dial-token production path

> ## **HANDOFF M8-10 — LOCAL SEAM FAIL-CLOSED; PRODUCTION TRUST BOUNDARY CHƯA ĐƯỢC KÝ**
>
> - **Contact contract gap:** OpenAPI có `phone_validation_status` optional string; runtime chỉ nhận
>   exact `VALID`. Không tự sửa wire trước M3 field matrix/CDC.
> - **TTL current:** intake đòi token phủ hết window, persistence không cho vượt window; accepted
>   persisted task vì vậy ép expiry bằng đúng window end, dù shared contract chưa nói rõ invariant.
> - **Scalar/retry:** một task có một token nhưng nhiều attempt/retry. MOCK/LAB chỉ chặn duplicate
>   cùng `(fingerprint, attempt_id)` và cho reuse ở attempt khác; không phải globally one-use proof.
> - **Privacy boundary:** token được fingerprint/protect trước persistence; destination trong domain
>   là opaque reference và cấm raw phone. Nếu vendor cần E.164, nó chỉ được lộ sau external
>   vault/gateway boundary đã duyệt.
> - **Production path:** non-MOCK foundation/gateway đều unavailable; không production resolver,
>   protector, adapter, credential mount, endpoint/egress hoặc resolution audit. Helm prod vẫn MOCK,
>   real call NO.
> - **M3 snapshot:** exact search không thấy contact producer/issuer fields/resolver; M3 phải giao
>   artifact owner + exact SHA hoặc chỉ rõ repo/service authoritative khác.
> - **Owner phải trả:** `DTK-01..DTK-15` về producer/requiredness, issuer/scope, token model/TTL,
>   resolver output/topology, auth/custody/rotation, replay/failure, audit/retention, vendor và rollout.
> - **Gói ký:** [M8-10 contact/dial-token decision pack](m8-10-contact-dial-token-production-decision-pack-2026-09-03.md).
> - **Evidence:** [W-0150](../../docs/evidence/W-0150/README.md).
>
> **Trạng thái:** **`EVIDENCE_SUBMITTED / LOCAL_PRIVACY_SEAM_PRESENT /
> PRODUCTION_PATH_FAIL_CLOSED / CONTRACT_RUNTIME_MISMATCH_FOUND /
> M3_CONTACT_PRODUCER_NOT_FOUND / EXTERNAL_DECISIONS_REQUIRED / CODE_NOT_AUTHORIZED`**.
>
> Không sửa source/OpenAPI/DB/Helm/secret trong W-0150; không bật production adapter/vault,
> external egress, Target delivery hoặc real customer call.

---

## 6. Thứ tự triển khai tuần sau

Chỉ triển khai nhánh tương ứng khi gate của nhánh đó đã có chữ ký hoặc dữ liệu thật.

1. **Contract trước code:** chốt M8-05, M8-06, M8-08, M8-09, M8-10 và M8-11.
2. **Schema/OpenAPI/migration sau chữ ký:** triển khai additive change, backward compatibility và test cho contract đã khóa; không sửa draft theo phỏng đoán.
3. **Admin ownership:** M8-03/W-0143 đã xong phần IVR; M3/Security/Platform nhận exact contract và hoàn thành client/BFF/custody/shared E2E. Không mở endpoint hoặc UI trùng lặp.
4. **Shared integration:** nối M8-07 với M3 sandbox, auth thật và shared E2E.
5. **Production infrastructure:** thực hiện M8-04 và M8-02 sau procurement/Security/Legal/Platform approvals.
6. **Capacity closure:** thực hiện M8-01 sau khi có dữ liệu real-call/session profile.
7. **Regression và gate update:** chạy đầy đủ test, contract drift, traceability và chỉ cập nhật tracker bằng evidence mới.

Nếu owner/gate chưa được chốt trong tuần sau, kết quả đúng là **BLOCKED_EXTERNAL** hoặc **OWNER_DECISION_REQUIRED**. Không viết code giả để tạo cảm giác có tiến độ.

---

## 7. Những phần đã local-verified — không giao làm lại

- **D1 — Intake:** endpoint, internal auth, closed schema và idempotency đã có local proof.
- **D2 — Official Order Gate:** đã reject quote/cart/draft và giữ fail-closed boundary.
- **D3 — Entry gates:** tám gate hiện có reason/evidence path và đã có local proof; chưa được suy ra là upstream state registry đã ký.
- **D4 — Callback mechanics:** outbox, idempotency, bounded retry và terminal handling đã có local proof.
- **D5 — Order/payment boundary:** IVR không trực tiếp mutate order/payment.
- **D7 — Secrets:** secret handling hiện fail-closed trong local candidate; production issuer/custody/rotation vẫn là external gate.
- **D8 — Test coverage:** bộ test local nêu ở mục 2 đã pass; không dùng việc chạy lại cùng một bộ test để thay cho target DB, shared E2E, UAT hoặc production proof.
- **D9/C2 — Business authority:** W-0123 đã xác lập M3 là business authority; legacy trusted-skip fields chỉ còn read-only compatibility/audit.
- Capacity guard và PT-CAP-02 đã tồn tại; phần thiếu là calibration bằng dữ liệu thật, không phải thêm lại guard/test.
- Ba TTS voice profile đã được Owner accept; phần thiếu là full acceptance/gate closure.

---

## 8. Các mục bị xóa hoặc gộp khỏi backlog

- **Xóa B2:** mô tả softphone WIP/untracked đã lỗi thời.
- **Đóng C2 tại TODAY-02:** runtime authority đã xử lý ở W-0123; phần DOC_CLEANUP_ONLY của M8-12 đã hoàn tất và không được dùng làm cớ mở lại trusted-skip trong IVR.
- **Gộp C4 + C5 + phần C7 → M8-06:** một quyết định session trace duy nhất.
- **Gộp C10 + C11 + C13 → M8-09:** một lifecycle revoke/freshness duy nhất.
- **Gộp B5 + C12 → M8-10:** production telephony và contact/dial-token cùng một trust boundary.
- **Sửa C3 → M8-05/M8-08:** taxonomy 11 codes không thiếu IVR_OPT_OUT; opt-out loop là quyết định riêng.
- **Sửa D6 → M8-11:** local candidate chưa phải production policy.
- **Bỏ toàn bộ phần trăm tiến độ:** không có nguồn chuẩn và dễ gây hiểu sai.
- **Bỏ nhận định “chỉ main branch” và “không đổi một byte”:** cả hai đều không đúng theo Git/source.

Không có đầu việc hợp lệ nào bị bỏ: các việc còn giá trị đã được giữ trong M8-01..M8-12, nhưng được gắn đúng owner, dependency và stop rule.

---

## 9. Điều kiện được phép tuyên bố hoàn tất

Chỉ được chuyển một workstream sang done khi đồng thời có:

1. Contract/decision đã được đúng owner ký nếu workstream có shared semantics.
2. Code, migration, tests và docs cùng khớp với quyết định đã ký.
3. Shared E2E/target DB/real-call evidence được chạy tại đúng môi trường nếu gate yêu cầu.
4. Tracker/readiness board được cập nhật bằng evidence link.
5. Không còn external gate liên quan bị mở.

Cho tới lúc đó, trạng thái tổng thể vẫn là:

- **Rung 0**
- **REAL_CUSTOMER_CALL_ALLOWED=NO**
- **Không CONTRACT_LOCKED**
- **Không PRODUCTION_READY**

---

## 10. Nguồn kiểm chứng chính

- [Control order và luật tuyên bố trạng thái](00-index.md)
- [Decision log](decisions-log.md)
- [Module 3 API handover](../../integration-requirements/06-module-3-api-handover.md)
- [V0.3 clean specification](../../docs/MODULE_8_IVR_ORDER_CONFIRMATION_V0.3_CLEAN.md)
- [Attempt policy T-09](../../docs/contracts/target-v1-closure-pack/T-09-attempt-policy.md)
- [W-0122 TTS evidence](../../docs/evidence/W-0122/README.md)
- [W-0130 exact-candidate evidence](../../docs/evidence/W-0130/README.md)
- [W-0137 OD20/target-DB evidence](../../docs/evidence/W-0137/README.md)
- [W-0138 current gate snapshot](../../docs/evidence/W-0138/README.md)
- [Readiness board](../../docs/release/readiness-board.md)
- [Prompt execution tracker](../../prompt/_execution/prompt-execution-tracker.md)

---

## Chốt

Module 8 không thiếu một danh sách task dài hơn. Module 8 đang thiếu **quyết định có owner, dữ liệu thật và external evidence** cho các seam sản xuất.

Phần IVR đã local-verified thì không giao làm lại. Phần thuộc M3/Owner/Security/Legal/Platform thì phải được họ quyết định và ký. Sau khi gate mở, IVR mới triển khai đúng contract và chịu trách nhiệm về execution/reporting.
