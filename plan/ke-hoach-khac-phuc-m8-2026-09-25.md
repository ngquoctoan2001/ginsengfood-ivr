# KẾ HOẠCH KHẮC PHỤC — Module 8, theo danh sách chief `25/09`

**Người thực hiện:** Nguyễn Quốc Toàn (qua agent) · **Lập:** `25/09/2026` · **Baseline:** `main@f3e26a6` — `W-0354` đã commit lúc 11:44, **chưa push**
**Căn cứ:** [`toan-viec-can-lam-m8-2026-09-25.md`](toan-viec-can-lam-m8-2026-09-25.md) (chief đo tại `5603204`) · rà soát lại toàn bộ cây bằng **10 sub-agent chỉ-đọc** chạy song song (11:00–11:46) · đối chiếu lại từng lỗ hổng trên `f3e26a6` (11:50).

> **Phạm vi:** mọi mã chief nêu — `A1–A4`, `B1–B17`, `C1–C23`, `N1–N5`, `V6-1…V6-17`, `D1–D11`, dòng 714 — cộng **các phát hiện mới ngoài audit** (đánh dấu 🆕).
>
> | Phần | Số mục | Công dev |
> | --- | ---: | --- |
> | I — Không cần quyết định (`K-01…K-51`) | 51 (1 tuỳ chọn) | ≈ 42 giờ ≈ 5 ngày công |
> | II — Cần quyết định (`Q-01…Q-36`) | 36 — Toàn 19 · chief/Tech Lead 10 · Sếp 3 · M3/anh Mạnh 4 | ≈ 33 giờ sau khi Toàn quyết · ≈ 48 giờ sau khi chief/Tech Lead quyết (31 giờ là N1) |
> | III — Chờ bên khác (`CB-01…CB-20`) | 20 | ≈ 170 giờ khi mở khoá (B12 chiếm 70–100) |
>
> **Giới hạn của lượt rà:** chỉ đọc — không build, không chạy test hay gate, vì soak `W-0037` đang đo tới ~14:10.
> Số test trong file là số `W-0354` tự đo, chưa chạy lại độc lập. Phát hiện 🆕 rút ra từ đọc code và DI; chỗ nào
> đã tái hiện thì ghi rõ.

---

## §0 — Cách dùng file này

### Trạng thái — cột cuối mỗi bảng, sửa ngay khi đổi

| Ký hiệu | Nghĩa | Ghi kèm |
| --- | --- | --- |
| ⬜ | Chưa làm | — |
| 🟡 | Đang làm | ngày + tên phiên. **Đánh dấu trước khi bắt tay**, để phiên khác không làm trùng |
| 🔵 | Xong trong cây, chưa commit | — |
| ✅ | Xong | `sha` commit + số test, ví dụ `✅ a1b2c3d · 1260/1260` |
| ⏸ | Chờ quyết định | ai quyết |
| ✔ | Đã quyết | phương án + ngày + người, ví dụ `✔ PA2 · 26/09 · Toàn` |
| ⛔ | Chờ bên khác | ai giữ nước đi |
| ➖ | Gộp vào mục khác / không phải việc dev | trỏ tới mục |

### Mã

| Mã | Nghĩa |
| --- | --- |
| `K-xx` | Không cần quyết định — dev làm được ngay |
| `Q-xx` | Cần quyết định: **3 phương án + 1 khuyến nghị**. `Q-xx.n` là việc làm sau khi quyết |
| `CB-xx` | Chờ bên khác — không nằm trong tay dev |
| `L0…L8` | Lô commit. **Một lô = một `W-ID`**, cấp ngay trước commit (`NEXT_WORK_ID` hiện là `W-0355`) |
| 🔥 | Phải xong trước khi gửi lại IR-07 cho M3 |
| 🆕 | Phát hiện mới, ngoài audit của chief |

### Thứ tự

Phần I (không cần quyết định, dễ → khó) → Phần II (cần quyết định, dễ → khó trong từng nhóm người quyết) → Phần III
(chờ bên khác). **Phần IV là bảng truy vết: mọi mã của chief đều có một dòng** — dùng nó để kiểm không sót mục nào.

---

## §1 — Hiện trạng sau `W-0354`

`W-0354` (`f3e26a6`, phiên "Tiếp tục soát việc cần làm") đã làm phần dev tự làm được của danh sách chief. Evidence
của lượt đó ghi: build 0 lỗi; test **unit `813/813` · contract `24/24` · integration `409/409` · chaos `8/8`**; đột
biến `9/9`; gate sweep `44/44`. Rà lại thấy phần đã làm **đúng hướng**, nhưng còn sót:

| Mục chief | `W-0354` đã làm (✅ `f3e26a6`) | Rà lại thấy còn thiếu | Task bù |
| --- | --- | --- | --- |
| `C15` + `C20` | Mở giờ gọi cho `ivr-api` ở compose e2e/sandbox và profile `LocalMockE2E`; reason vào error codes; IR-06 `202→200` | IR-07 dòng `M3-02` có 2 câu sai; 🆕 chỗ thứ 4–5 cùng lỗi: 2 host test + 2 script `tools/dev`; chưa có ví dụ sandbox ca đêm | `K-05`, `K-11`, `Q-10` |
| `C7` (+`C8`, `B10`) | Đính chính IR-07: rút đề xuất chờ 24 giờ ở `M3-13`, `A-10`, nhãn action | IR-06 còn 3 câu nói ngược (`:846`, `:852`, `:1516`); bảng điền nhanh vẫn hỏi `M3-13`; sổ `OD-V1-06` chưa sửa | `K-12`, `K-18`, `Q-01` |
| `C9`, `C14`, `C2`, `C21` | Đính chính tài liệu | `M3-30` dẫn `§3.4` (đúng là `§3.5`); checklist IR-06 còn câu cũ; `C21` phần code chưa làm | `K-07`, `K-13`, `Q-19` |
| `B16` | Intake chặn số lẻ; contract `draft.33` (`multipleOf: 1`); lỗi dữ liệu lúc render không còn cách ly SIM | IR-06 bảng field + bảng từ chối; fixture âm; test gateway Asterisk; 🆕 `quantity` >9 chữ số lẻ vẫn cách ly SIM; 🆕 lớp lỗi "intake nhận – lúc quay từ chối" | `K-14`, `K-26`, `K-30`, `Q-12`, `Q-16` |
| `B15` | `ToString` hai record che số | Serialize JSON vẫn in số; chưa có test chặn record thứ ba | `K-38` |
| `B11` bước 0 | `IT-PHONE-CONTAIN-01` | Chưa phủ đường HTTP intake, đường quay production, ETL analytics, log worker | `K-41`, `Q-28.2` |
| `B13` (+`B14`) | Gateway Asterisk dùng chế độ của deployment | Mock gateway còn hằng; chưa có test cấp gateway; 🆕 DispatchGate chặn mọi địa chỉ production | `K-42`, `Q-28` |
| `B5` | §3b đúng ba ý chief | Chính §3b còn 4 câu mâu thuẫn; 🆕 schema **phân biệt được** DB đã drop (thứ tự cột) | `K-20`, `Q-09` |
| `B1` bước 1 | Ghi nguồn `D07` | Assert không bao giờ đỏ; chú thích cũ | `K-21` |
| `B9` bước 2 | `X-Actor-Id` bắt buộc trên `31/33` endpoint | Khối "chuỗi perm là nhãn" sai với `IVR_SCRIPT_*`; thiếu 2 chuỗi | `K-15` |
| `A1` bước 4, 7 | "Tự nghiệm thu" ghi vào tracker/board; pii-policy về `NO` | Câu "chỉ bật qua biến môi trường" chưa đúng hết | `K-06` |
| Dòng 714 (CI hosted) | Tìm và sửa nguyên nhân `openapi_lint` đỏ (`draft.33` + gate lint local) | 🆕 `deploy_dev` luôn đỏ + 2 job manual chặn; 🆕 test đỏ theo giờ; luồng promote hỏng | `K-01`, `K-05`, `Q-14` |

> **Bổ sung 12:30 (phiên "rà soát tiếp việc cần làm") — `W-0355` ✅ `c2435b9`, chưa push.** Log hai worker của soak
> `W-0037` lộ ba lỗi mà kế hoạch này chưa có, đã sửa và có test: (1) normalizer nhận lại việc worker kia vừa xong,
> vấp `PK_ivr_call_results`; (2) hai lượt ETL analytics chạy cùng lúc, vấp `PK_fact_call_outcome`; (3) **hồi quy của
> `W-0353`**: câu đối chiếu job fact quét cả bảng attempt cho mỗi job fact — 34.472 ms ở 7.623 job fact, vượt timeout
> 30 giây, nên lúc 12:15 không lượt ETL nào hoàn tất trên cả hai worker; sửa xong còn 19 ms. W-0055 đã được ghi
> `ACCEPTED` sáng nay dựa trên phần này — **Toàn giữ nghiệm thu (25/09)**, bản sửa đi cùng W-0355. Chi tiết: `docs/evidence/W-0355/README.md`.
> Lô commit kế tiếp dùng `NEXT_WORK_ID` = `W-0356`.

### Mười phát hiện mới nặng nhất (🆕)

| # | Phát hiện | Mức | Task |
| --- | --- | --- | --- |
| 1 | API **luôn nhận token tĩnh** `ORDER_CORE_SERVICE_TOKEN`, kể cả khi `SALES_PROVIDER=TARGET_V1` ở production: `OrderCoreAllowlistMiddleware` đọc `CallbackDeliveryOptions`, mà API không bind options này (chỉ worker bind). **Ba agent độc lập cùng tìm ra** | Cao | `Q-23` |
| 2 | `DispatchGate` ném lỗi với mọi địa chỉ production `sip:+84…@host` (PiiGuard khớp số) và còn đòi allowlist của lab ⇒ mở `IsReady` xong production vẫn **không quay được số nào** | Vừa | `Q-28` |
| 3 | Mất luồng sự kiện ARI giữa cuộc gọi ⇒ cuộc đang mở bị ghi là khách không nghe/không bấm **có tính lượt** ⇒ có thể thành `IVR_NO_ANSWER_FINAL` ⇒ M3 hủy đơn COD (trái `N3`, `V6-6`) | Vừa | `K-47` |
| 4 | Tên hàng W-0243 cho phép ("Tổ yến", "Đường phèn") bị guard toàn văn chặn lúc quay ⇒ đơn **không bao giờ được gọi**, M3 chỉ thấy `WINDOW_EXPIRED` | Vừa–cao | `Q-12` |
| 5 | Q.850 cause 20 (khách tắt máy/ngoài vùng) bị coi là **số sai**, kết thúc ngay sau cuộc 1 ⇒ M3 hủy đơn như số sai | Vừa | `Q-27` |
| 6 | Pipeline main **không thể xanh theo cấu trúc**: `deploy_dev` cần cluster, 2 job manual `allow_failure:false`; luồng promote hỏng 3 chỗ | Cao (quy trình) | `Q-14` |
| 7 | Test tích hợp đỏ khi chạy 21:09–07:58 (`IT-API-MATRIX-38`, `IT-DEV-SEED-03/04`); `pnpm e2e:local` và bootstrap dev hỏng ban đêm | Vừa | `K-05` |
| 8 | Ba biến "an toàn" (`IVR_KILL_SWITCH_ENABLED`, `IVR_ADAPTER_MODE`, `IVR_LAB_DESTINATION_ALLOWLIST`) **không code nào đọc**, nhưng gate `k8s-selftest` và chính audit (dòng 106) dùng làm bằng chứng | Vừa | `Q-15` |
| 9 | Đơn 24/7 có T0 từ 21:00:30 tới 21:07:59 chỉ được gọi **1 cuộc** rồi `WINDOW_EXPIRED` ⇒ M3 hủy — tệ hơn đơn đặt sau 21:08 (giữ tới sáng, gọi đủ 2 cuộc) | Thấp–vừa | `Q-22` |
| 10 | N1 thực tế **≈ 36–44 giờ** (chief ước 20); khôi phục `StaticFileTtsProvider` là sai cơ chế (nó phát một file cho cả cuộc); đường phát clip động **chưa từng tồn tại**; kịch bản v3 lệch PACK-09 (không đọc mã đơn) | Vừa (kế hoạch) | `Q-29` |

---

## §2 — Luật thi hành (ràng buộc mọi task)

| # | Luật | Vì sao / chi tiết |
| --- | --- | --- |
| 1 | Chỉ làm trên `main`; không tạo nhánh dưới bất kỳ cách viết nào | `CLAUDE.md`; `.githooks/reference-transaction` chặn cứng |
| 2 | `gitnexus_impact` (upstream) trước khi sửa symbol; báo blast radius; **dừng và báo** nếu HIGH/CRITICAL | Đã biết HIGH: guard W-0298 `AnyAttemptFallsInsideCallingHours`, `TryClaimDueDispatchAsync`, `CloseMissedDeadlinesAsync`, `SpeechSynthesisService.SynthesizeAsync` |
| 3 | `gitnexus_detect_changes` trước mỗi commit | `CLAUDE.md` |
| 4 | Không sửa migration đã áp; đổi hành vi ⇒ migration mới | Kỷ luật `B5` |
| 5 | Sửa file bị ghim ⇒ ghim lại **trong cùng commit**, hash trên byte LF | IR-06: 4 validator (`d06`, `dial-token`, `opt-out`, `upstream-session`) + 3 template (`W-0181`, `W-0183`, `W-0187`) + con trỏ `FREEZE-03`. OAS: `contract-manifest.json` (qua `openapi-contract-drift.mjs --accept-reviewed-draft`), dial-token validator, template W-0183, field inventory, baseline, changelog (sinh bằng `generate-oasdiff-changelog.sh` trong image oasdiff đã ghim — không sửa tay), portal (`build-api-docs.mjs`). `ProviderPorts.cs`: dial-token validator + W-0183. `MODELS.lock`: xem `Q-17.1`. m8-05: **không sửa** (chuỗi external-decision 3 tầng) |
| 6 | Gom mọi lần sửa IR-06 của một lô vào **một** lượt | Mỗi lượt sửa IR-06 = ghim lại 7 nơi |
| 7 | File dẫn xuất sinh bằng `--write`, commit cùng thứ nó dẫn ra | `gate-status.yaml` + `readiness-board.md` + tracker cùng commit; `traceability-tests.md` sinh lại khi thêm TestId |
| 8 | TestId trong README evidence là lời khẳng định C2 | Chỉ ghi ID của chính lượt đó; ví dụ/ID của việc khác để trong JSON |
| 9 | Commit theo pathspec; trước commit kiểm phiên khác (`ListAgents`, `list_sessions`, mtime) | Hai phiên dùng chung một checkout |
| 10 | **Không push, không chạy tải nặng khi soak đang đo** | gitlab-runner chạy trên chính máy này; soak `W-0037` tới ~14:10 |
| 11 | Chạy test tích hợp **ban ngày** cho tới khi xong `K-05` | Test phụ thuộc giờ gọi 08:00–21:08 |
| 12 | Gate sweep chạy bằng Git Bash, không song song `dotnet test` | Từ PowerShell các gate `sh` chết (ENOENT) |
| 13 | Kiểm từ ngoài (dựng stack thật), không chỉ `dotnet test` | Bài học W-0282: test xanh vẫn giấu lỗi image/compose/env |
| 14 | `REAL_CUSTOMER_CALL_ALLOWED=NO` giữ nguyên ở mọi lô | Khoá K1 |
| 15 | Thấy chief sai thì phản hồi kèm bằng chứng, không sửa im lặng; không tự đổi tên event/endpoint/enum/field dùng chung | File chief, dòng 13 |
| 16 | Kế hoạch là giả thuyết đo tại `f3e26a6`: **đọc lại luồng code thật trước khi sửa** | Bài học W-0298 (phương án A sai vì T0 tới trên wire) |

---

## §3 — Bắt đầu từ đâu

| Bước | Việc | Ai | Khi |
| --- | --- | --- | --- |
| 1 | Chốt 4 quyết định 🔥 của Toàn: `Q-01`, `Q-02`, `Q-03`, `Q-13` (đều có khuyến nghị, ≈10 phút đọc) | Toàn | ngay |
| 2 | ~~Gửi chief một tin gộp (mẫu ở §7)~~ — bỏ 25/09: Toàn chốt `Q-20…Q-29` theo đề xuất M8, không gửi; `CB-02` chưa xin | Toàn | ✔ 25/09 |
| 3 | `L0` — push `f3e26a6` | agent/Toàn | **sau 14:15** |
| 4 | `L1` — vệ sinh test | agent | ngày công 1 |
| 5 | `L2` — tài liệu gửi M3 + sổ quyết định, gộp `Q-01.1`, `Q-02.1`, `Q-03.1`, `Q-13.1` | agent | ngày công 2 |
| 6 | `CB-01` — gửi lại IR-07 cho anh Mạnh, ghi mốc gửi | Toàn | ngay sau `L2` |
| 7 | `Q-08` — tờ trình cho Sếp (qua chief) kèm `Q-30…Q-32` | Toàn | ngày công 2 |
| 8 | `L3…L8`, rồi các việc sau quyết định (Phần II) | agent | ngày công 3 trở đi |

---

## §4 — PHẦN I: Không cần quyết định (dễ → khó)

### `L0` — Đẩy `W-0354` · 0,25 giờ

| Mã | Nguồn | Việc | Kiểm | Giờ | Trạng thái |
| --- | --- | --- | --- | ---: | --- |
| `K-01` | dòng 714 | Push `f3e26a6` lên `origin` (2 pushurl); kiểm `git ls-remote --heads origin` và `github` cùng trỏ `f3e26a6`; ghi pipeline ID vào activity tracker ở lô sau. Kỳ vọng `openapi_lint` xanh, `deploy_dev` đỏ (xem `Q-14`) | pipeline hosted | 0,25 | ✅ 25/09 · một lần push, dời tới sau 14:42 cho quý đầu của soak: `f3e26a6` (W-0354) … `3d04eee` (W-0358) và commit cập nhật kế hoạch này; sha đỉnh và `ls-remote` hai remote báo trong tin cho Toàn; pipeline ID ghi ở lô sau |

### `L1` — Vệ sinh test · 2,25 giờ

| Mã | Nguồn | Việc | File chính | Kiểm | Giờ | Trạng thái |
| --- | --- | --- | --- | --- | ---: | --- |
| `K-02` | 🆕 | TestId trùng: `IT-OBS-BACKLOG-15` gắn cho 3 test khác nhau (`SchedulerPersistenceTests.cs:445, :484, :514`, từ W-0353) → 3 ID riêng; chú thích `SchedulerQueueBacklog.cs:32` trỏ đúng test canh vị từ `revoked_at` | tests/Scheduling | traceability | 0,25 | ✅ a6293d2 (W-0356) · IT-OBS-BACKLOG-15/16/17 |
| `K-03` | 🆕 | 10 test lõi intake chưa có TestId (`TaskIntakeServiceTests.cs:36, 55, 75, 254, 290, 318, 335, 351`; `CallResultAndMapperTests.cs:51`; `DomainPolicyAndPrivacyTests.cs:174`) → gắn TestId | tests/Intake | `generate-test-traceability.mjs` | 0,25 | ✅ a6293d2 (W-0356) · 10 TestId mới, unit 815/815 |
| `K-04` | 🆕 | Profile `LocalMockE2E`: API chạy mặc định 1 kênh/60 s, worker 8 kênh/5 s → chép khối `Ivr:Scheduler` sang profile API; `UT-CALLWINDOW-PARITY-02` so thêm các khoá này | `src/Ivr.Api/appsettings.Profile.LocalMockE2E.json` | UT-CALLWINDOW-PARITY-02 | 0,25 | ✅ a6293d2 (W-0356) · UT-CALLWINDOW-PARITY-02 (đột biến API 1 kênh → đỏ) |
| `K-05` | 🆕 `C15` | Test/tool phụ thuộc giờ gọi (chỗ thứ 4–5 cùng lỗi C15): `ApiMatrixFixture.cs:225` (`IT-API-MATRIX-38`), `DevToolingApiTestApplication.cs` (`IT-DEV-SEED-03/04`), `tools/dev/Invoke-LocalE2E.ps1:92-134`, `Invoke-DevBootstrap.ps1:183-190` → khung `0..1440` cho cả api lẫn worker, hoặc ghim đồng hồ 13:00 giờ VN; thêm 1 test đêm dùng `FakeTimeProvider`; mở rộng parity sang `tools/dev`; sửa chú thích cũ "20:52:30" ở `CallingWindowTests.cs:221-223` | tests, `tools/dev` | chạy lại 2 test với đồng hồ giả 23:00 | 1,5 | ✅ a6293d2 (W-0356) · khung giờ đóng → đúng IT-API-MATRIX-38, IT-DEV-SEED-03/04 đỏ; UT-CALLWINDOW-PARITY-03 |

### `L2` — Tài liệu gửi M3 + sổ quyết định 🔥 · 5,5 giờ · ghim IR-06 **một** lần

> Gộp luôn `Q-01.1`, `Q-02.1`, `Q-03.1`, `Q-13.1` nếu Toàn đã chốt — tránh ghim IR-06 hai lần. Chưa chốt thì lô
> này commit phần chắc chắn, đính chính còn lại đi lượt sau (thêm một lần ghim, ~15 phút).

| Mã | Nguồn | Việc | File chính | Kiểm | Giờ | Trạng thái |
| --- | --- | --- | --- | --- | ---: | --- |
| `K-06` | `A1` bước 7 | `05-pii-policy.md:12`: thêm "hoặc khoá cấu hình `Ivr:ProductionTargetV1FieldsApproved`; code không kiểm chữ ký" — hiện câu "chỉ bật qua biến môi trường" chưa đúng hết | `specs/data/05-pii-policy.md` | — | 0,1 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) |
| `K-07` | `C9` | IR-07 dòng đính chính `M3-30` dẫn "IR-06 §3.4" → sửa thành `§3.5` (`:426`, `:447-464`) | IR-07 | — | 0,1 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) |
| `K-08` | `D11` | IR-06 `:352`: T0 muộn nhất còn đủ hai cuộc là `21:00:29` / `21:05:29`, không phải `:30` (đã có `CallingWindowTests.cs:201-211` chứng minh) | IR-06 | — | 0,1 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) |
| `K-09` | 🆕 | IR-07 `:698`: viết lại câu "chief lập… thay mặt Tech Lead" để khỏi đọc thành M8 viết thay Tech Lead. `:712`: trỏ `docs/api-changelog.md` thay vì báo cáo oasdiff 32→33 (báo cáo đó không nhắc `total_amount`) | IR-07 | — | 0,1 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) |
| `K-10` | 🆕 🔥 | IR-07 thêm một dòng đính chính: `A-5`, `A-6`, `A-9`, `D-8` (phương án B — M3 gửi `phone_e164`) **phụ thuộc Sếp trả lời mục B2 phiếu 25/09**; tới lúc đó M3 chưa nối producer gửi số thật (khớp FIX_M3 ngày 25/09). Hiện phiếu trình phương án B như đã chốt (`:10-14`, `:136`, `:582`; IR-08 `:161-163`) | IR-07, IR-08 | — | 0,1 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) |
| `K-11` | 🆕 🔥 `C15` | IR-07 dòng `M3-02`: bỏ câu "đúng với kill switch và hết dung lượng" — intake chỉ phát `200 TASK_BLOCKED_OPERATIONAL` cho `CALLING_WINDOW_CLOSED_…` và `DIAL_TOKEN_PROTECTION_UNAVAILABLE`, cả hai đều không retry được (`call_restriction` là `409`); bỏ "Giờ Vàng không gặp ca này" (trái bảng so-sánh `:93-94`). Thêm `DIAL_TOKEN_PROTECTION_UNAVAILABLE` vào `06-error-codes` §2a và IR-06 §3.9. Sửa `06-error-codes:48` và `:78` ("gửi task mới" → "được dùng lại `task_id`, `Idempotency-Key` mới"). Thêm câu: gửi dồn lúc 08:00 vượt dung lượng sẽ nhận `IVR_CAPACITY_EXCEPTION` ở eligibility — nên rải | IR-07, IR-06, `specs/api/06-error-codes.md` | CT-CI-10 (đếm 16 dòng mã lỗi) | 0,5 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) — câu "gửi dồn 08:00 nhận IVR_CAPACITY_EXCEPTION ở eligibility" sai với code (eligibility chỉ giữ job, không callback) nên viết lại; lỗ hổng tách thành K-52. `DIAL_TOKEN_PROTECTION_UNAVAILABLE` chỉ xảy ra ở production (lab có bộ mã hoá), và hôm nay production còn giữ task ở `TASK_HELD_ADMIN_REVIEW` trước bước đó |
| `K-12` | 🔥 `C7` | IR-06 còn nói ngược đính chính `A-10`/`M3-13`: checklist `:1516` "Chốt timeout worker sau `IVR_NO_ANSWER_FINAL`", `:846` "chờ timeout policy", `:852` "advisory" → sửa hoặc ghi chú trỏ về đính chính 25/09 | IR-06 | — | 0,25 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) |
| `K-13` | 🔥 `C14` `C21` `C2` `C13` | Checklist IR-06 `:1525-1526` còn câu cũ của C14; `:1521-1522` và IR-07 Phần E thiếu ca "phát lại muộn" + lời hứa giới hạn tuổi (C21); IR-06 §4.3 thêm câu trỏ m8-05 và ghi rõ trên dây M3 nhận `409 IVR_OPERATIONAL_BLOCKED` cho `call_restriction` (C2); ai mở endpoint thu hồi: `00-CHUA-XONG.md:254`, `00-DA-XONG.md:61`, IR-06 `:954-956` nói ngược IR-07 `E-2` (IVR mở) — sửa; dòng `A-11` thêm `draft.33`; dòng trạng thái `M3-14` (`:317`) bỏ lời hứa "lượt phát hành kế tiếp" | IR-06, IR-07, `plan/ivr-orther/00-*.md` | — | 0,5 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) |
| `K-14` | 🔥 `B16` | IR-06 bảng field `:506` (`total_amount` = số đồng nguyên khách trả, sau lần làm tròn cuối, cùng nguồn final payable của M3) và bảng từ chối `:685-690`; thêm dòng "`total_amount` có phần lẻ → `400`" vào `06-error-codes`; fixture âm `NEG-SCHEMA-AMOUNT-01` vào `seed/sales-target-v1.sample.json` + `requiredSchemaNegativeIds` (`validate-openapi.mjs:85-99`); ô xác nhận `M3-31` (nghĩa `total_amount`) trong IR-07 | IR-06, IR-07, seed, `validate-openapi.mjs` | CT-OAS-01, `validate-openapi` | 0,75 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) |
| `K-15` | 🔥 `B9` bước 2 | IR-06 §4A.7 (`:1203-1215`): `IVR_SCRIPT_*` là giá trị header `X-Script-Permissions` **được kiểm thật** (`ScriptLifecycleApiService.cs:88-94`) — bỏ câu "M3 không cần ánh xạ vai trò" (làm theo thì mọi thao tác kịch bản bị từ chối); thêm `IVR_FLAG_READ`, `IVR_RUNTIME_GATE_ADMIN` (gắn cổng phê duyệt; `OD-V1-20` đã thu hồi 16/09); `analytics/export` cũng ghi audit; báo M3 trong IR-07. Chữ OAS `:972` "granted to Admin per OD-V1-20" sửa ở lần bump contract kế tiếp (`Q-19.1`) | IR-06, IR-07 | — | 0,5 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) — `OD-V1-20` KHÔNG bị thu hồi (sổ vẫn CLOSED); cái bị thu hồi 16/09 là dòng approval do W0195 seed. IR-06 ghi đúng như vậy |
| `K-16` | 🆕 | Số cũ: IR-07 `:115`, `:341`, `:382` (số endpoint/danger), `:147` ("2/28" → số mở thật sau `K-18`); IR-08 `:5` ("24/24" → 28), `:207` ("38 lệnh" → 40); `06-error-codes:117` ("18 `code`" → 16). Ghim lại template `W-0178` (`b676a3…`, lỗi thời từ `98e94dc`) | IR-07, IR-08, error codes, template W-0178 | W0178 selftest | 0,25 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) — KHÔNG ghim lại template W-0178: nó nằm trong `docs/evidence/W-0178/attested-sha256.txt` (hồ sơ đã đóng, không được ghim lại); `06-error-codes` sửa 18 → 16 mã (18 là số trước W-0128) |
| `K-17` | 🆕 `B9` bước 1 | IR-07 thêm câu hỏi `M3-32`: màn capacity-incidents cần lọc/phân trang gì; đề xuất mặc định: lọc `status/scope/program/opened_at`, `page/page_size`, không trả `session_id`/`reason`. B9 đang "chờ M3" nhưng **chưa ai hỏi M3** (grep "incident" trong IR-07 = 0) | IR-07 | — | 0,25 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) |
| `K-18` | `A1` bước 3 | Sổ quyết định theo chốt chief 25/09, 11 dòng: `OD-V1-02/03/06` owner Tech Lead (vai M3), "chờ bản ký Tech Lead do chief chuyển", giữ hậu tố `M3_NOT_RECEIVED`; `OD-V1-06` viết lại theo flow 04; `OD-V1-07` chờ Tech Lead ký sau dòng ủy quyền N14; `OD-V1-08/16` `M8_POSITION_SIGNED` / chờ Sếp (N16); `OD-V1-17/18` bỏ `CLOSED` → "phụ thuộc phương án B — chờ Sếp B2"; `OD-V1-11` sửa câu "Sếp ký nhận ở S2"; `OD-V1-12` lần bật gọi khách thật đầu tiên cần chữ ký Sếp; đầu sổ `:3-13`; `OD-VOICE-04 :113`, `OD-V1-19` ("16 CVE"), `OD-V1-16` ("3 giờ sáng"). Ô Current của dòng mở lại **không còn chữ `CLOSED`**, không ký tự gạch đứng. Đồng bộ các chỗ nhắc trạng thái sổ: IR-06 `:56, :333, :1317, :1435, :1437, :1440`; IR-07 `:147, :306`; IR-08 `:232`; `specs/api/04-sim-adapter-contract.md:34`; `pia.md:18`; `release-compliance-checklist.md:16`. Hỏi chief một câu: vế S3 (giữ vĩnh viễn) của `OD-V1-11` có mở lại như 17/18 không | `specs/_review/open-decisions-register.md` + các file trên | `gate-status.mjs --write` (quyết định mở 6 → 12) | 1,5 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) — sổ còn 12 dòng mở / 29 (24 OD-V1 + 5 OD-VOICE): 01, 02, 03, 05, 06, 07, 08, 09, 10, 16, 17, 18 |
| `K-19` | — | Kết lô: ghim lại hash IR-06 ở 4 validator + 3 template; kiểm con trỏ phiên bản cho `FREEZE-03` | `deploy/ci/scripts/*`, templates | `contract-freeze-verifier`, 4 validator, `docs-selftest`, `gate-status`, gate sweep | 0,5 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) — ghim lại 3 file: IR-06 (7 nơi), `TaskIntakeService.cs` (4 nơi), `EligibilityRules.cs` (2 nơi, do sửa chú thích); W0178/W0181/W0183/W0187 self-test + check-template, contract-freeze, ci-config, validate-openapi đạt |

### `L3` — Tài liệu nội bộ · 2,25 giờ

| Mã | Nguồn | Việc | File chính | Kiểm | Giờ | Trạng thái |
| --- | --- | --- | --- | --- | ---: | --- |
| `K-20` | `B5` | `deploy/ci/rollback.md` §3b còn mâu thuẫn: `:82-83` ("trả lời dứt khoát"), `:111-112` (DB mới thực ra rơi vào ô "không xác định được"), `:122` (`audit-0907` đã có P03 → trỏ bước `p03_applied`), `:126` ("luôn đúng"), `:128-134` (phải ghi kết quả vào nhật ký **trước** khi nâng cấp — migrate kế tiếp tự chạy P03 và xoá dấu vết). Đính chính có ngày ở `docs/evidence/W-0306/README.md:169, :178`, không sửa nội dung cũ | `rollback.md`, W-0306 | — (file không bị ghim) | 0,5 | ✅ 627dda6 (W-0357) |
| `K-21` | `B1` | Assert `capacity-selftest.mjs:458-463` không bao giờ đỏ (so hằng với chính nó) → kiểm thật; chú thích `capacity-model.mjs:95-96` ("It is unanswered"); `candidateSource` dẫn thêm `FIX_OWNER_CORE.md:160`; tên thuộc tính cũ ở `docs/evidence/W-0134/README.md:37` | `tools/capacity-sim`, selftest | `capacity-selftest` | 0,25 | ✅ 627dda6 (W-0357) · capacity-selftest đỏ khi bỏ D07; chưa dẫn FIX_OWNER_CORE.md:160 (file không có trên máy dev) |
| `K-22` | 🆕 | Tài liệu nói sai code: `README.md` (console Next.js / `pnpm dev` / cổng 3005 đã xoá; badge CI "NOT_RUN"; `/health/ready` hứa trả 503 khi circuit mở — API không có breaker), `deploy/ci/README.md:220`, `Dockerfile.worker:50-56` ("opens no socket") | README, deploy | `docs-selftest` | 0,5 | ✅ 627dda6 (W-0357) · docs-selftest, ci-config-selftest |
| `K-23` | 🆕 | `docs/secret-inventory.md` §2 thiếu `IVR_ADMIN_READ/WRITE/DANGER_TOKEN` (TTL, chủ sở hữu); runbook xoay vòng thêm 3 token này (token Danger đang nằm ngoài quy trình xoay) | `docs/secret-inventory.md`, `secret-rotation-runbook.md` | — | 0,25 | ✅ 627dda6 (W-0357) |
| `K-24` | 🆕 `A2` | Hash payload intake (outbox, idempotency, audit append-only) tính trên body có `phone_e164` và còn lại sau DSAR → ghi là dữ liệu giả danh hoá trong `docs/compliance/data-inventory.md` | `data-inventory.md` | — | 0,25 | ✅ 627dda6 (W-0357) · compliance-pack-selftest |
| `K-25` | 🆕 `B12` | Runbook `docs/operations/production-dial-path.md` §"Read this before the rest" (`:13-21`) thiếu điều kiện giữ production đóng: DispatchGate (`Q-28`), N1, S1 (`Q-30`), kiểm lại gate sát lệnh quay (`K-44`), số điện thoại nằm trong query string gửi ARI (cấu hình log Asterisk/proxy) | runbook | — | 0,5 | ✅ 627dda6 (W-0357) |

### `L4` — Intake & dựng lời thoại · 3,75 giờ

| Mã | Nguồn | Việc | File chính | Kiểm | Giờ | Trạng thái |
| --- | --- | --- | --- | --- | ---: | --- |
| `K-26` | 🆕 `B16` | `SpellQuantity` cắt chuỗi `…Split('.')[1]` (`VietnameseNumberSpeller.cs:177-180`) ném `IndexOutOfRangeException` với `quantity` >9 chữ số lẻ → rơi nhánh `_` → **cách ly SIM** (B16 chưa đóng hết). Tách phần lẻ bằng phép tính `decimal`; `FormatQuantity` bắt đủ loại lỗi | `VietnameseNumberSpeller.cs` | `UT-SPELL-QTY-01..03` (1.0000000001 · 2.9999999999 · 0,0005 đang đọc thành "0") | 0,5 | ✅ 3f71a1d · 1318/1318 (W-0359) |
| `K-27` | 🆕 | Tên hàng không dấu chứa `duong/thon/ap`: `PiiGuard.EnsureSafeText(summaryJson)` (`TaskIntakeService.cs:749`) ném lỗi ngoài `try` → **500** → M3 retry mãi. Bắt lỗi, trả mã `4xx` có sẵn (không thêm enum) kèm reason; ghi vào `06-error-codes`. Chính sách guard tên hàng quyết ở `Q-12` | `TaskIntakeService.cs` | test intake mới | 0,75 | ✅ 3f71a1d · 1318/1318 (W-0359) |
| `K-28` | 🆕 | Lỗi 400 schema không nói field nào sai (`TaskIntakeEndpoint.cs:433-440`, `details` rỗng) → thêm tên field, không kèm giá trị; không phá contract | `TaskIntakeEndpoint.cs` | test intake | 0,5 | ✅ 3f71a1d · 1318/1318 (W-0359) |
| `K-29` | 🆕 | Render ném `InvalidOperationException` (kịch bản chưa duyệt, template sai, bản thoại >1.200 ký tự, guard PII) bị ghi là `*_POLICY_OR_TOKEN_REJECTED` → người trực đi tìm lỗi token. Đặt mã kỹ thuật riêng `SPEECH_RENDER_POLICY_REJECTED` (nếu mã nằm trong enum OAS thì gộp vào bump `draft.34`); sửa chú thích quá tay `ConfigurableExternalTtsProvider.cs:24` | 2 gateway, renderer | `UT-RENDER-DATA-04` | 0,5 | ✅ 3f71a1d · 1318/1318 (W-0359) |
| `K-30` | `B16` | Test bù: `UT-AST-RENDER-DATA-10` (gateway Asterisk — đường lab/production); `IT-INTAKE-AMOUNT-17` (`210636.0`, `2.10636E5` được nhận); `IT-TEL-RENDER-DATA-10` (2 lần render bị từ chối → `HELD_ADMIN_REVIEW` → `WINDOW_EXPIRED`; kênh `IDLE`, FailCount không đổi) | tests | 3 test mới | 1,5 | ✅ 3f71a1d · 1318/1318 (W-0359) |

### `L5` — Vận hành, quan sát, cấu hình · 5,75 giờ

| Mã | Nguồn | Việc | File chính | Kiểm | Giờ | Trạng thái |
| --- | --- | --- | --- | --- | ---: | --- |
| `K-31` | 🆕 | Lỗi bị nuốt không log: `CallbackDispatcher.cs:125-139` (bug lập trình → `RETRY_EXHAUSTED` + mở circuit 30 s, không để lại loại lỗi); `RuntimeGateApprovals.cs:155-161, :218-221, :335-338` + `RuntimeGateDefaults.cs:66-69` (truy vấn hỏng thành "không" im lặng); `TryHangupAsync` ở 2 gateway → log loại lỗi + `RecordFailClosed`/metric | Callbacks, FeatureFlags, Telephony | test logger thu | 1,25 | ✅ dispatcher và audit store 5aec684, `RuntimeGateApprovals.cs` 82188e6 sau khi Toàn duyệt 25/09 (W-0360) · UT-CALLBACK-TRANSPORT-LOGGED-18, UT-OBS-FAILCLOSED-11/12/13; `TryHangupAsync` ở 2 gateway ✅ 3f71a1d (W-0359, L4) |
| `K-32` | 🆕 | `Retry-After` không có trần (`CallbackDispatcher.cs:277-280`, `TargetV1CallbackTransport.cs:265-269`): một phản hồi 429 kèm Retry-After lớn làm callback tới muộn tùy ý, lách giới hạn tuổi của C21 → chặn trần ở `MaxRetryDelay` | Callbacks | `UT-CB-RETRY-AFTER-01` | 1 | ✅ 5aec684 (W-0360) · UT-CALLBACK-RETRY-AFTER-17; trần 5 phút, không phải MaxRetryDelay (5 s, 09B cấm) |
| `K-33` | 🆕 dòng 121 | Cảnh báo giả "IVR worker loops have stopped ticking: (none registered)" lúc khởi động: `IvrHeartbeat.cs:25-37` đọc registry ở tick đầu (`do/while`) trước khi các JobHost đăng ký; dòng "N loops turning" đếm cả vòng đã tắt → startup grace ≤30 s qua `TimeProvider`, chỉ đếm vòng đang bật | `src/Ivr.Worker/IvrHeartbeat.cs`, `WorkerLiveness.cs` | `IT-WORKER-LIVENESS-13` | 1,5 | ✅ 5aec684 (W-0360) · UT-WORKER-HEARTBEAT-01/02 (unit, thay cho IT đề xuất) |
| `K-34` | 🆕 | Helm: `deployment-api.yaml:44-48` không đặt `Ivr__Speech__Tts__Provider`, ảnh API bake `FAKE` ⇒ **API không khởi động** ở chế độ không-MOCK → đặt `UNSELECTED`. Worker thiếu `IVR_INTERNAL_SERVICE_TOKEN` ⇒ bật EligibilityPolling qua helm là worker không khởi động | `deploy/helm/ivr/templates/*` | `k8s-selftest` + ca render mới | 0,75 | ✅ 5aec684 (W-0360) · IT-K8S-TTS-08, IT-K8S-ELIG-09 (phần render; gate đầy đủ ở lượt collector --extended) |
| `K-35` | 🆕 | `src/Ivr.Api/appsettings.Development.json` chứa 5 token dev công khai (có tầng Danger) được đóng vào ảnh API, không có guard → `CopyToPublishDirectory="Never"` hoặc guard lúc khởi động | `Ivr.Api.csproj` | `image-selftest` | 0,5 | ✅ 5aec684 (W-0360) · IT-IMG-BUILD-01 kiểm ảnh API không còn file |
| `K-36` | 🆕 | Trạng thái circuit callback chỉ có ở worker; `/health/ready` của API luôn `not_configured` → đưa trạng thái circuit vào body `/healthz` của worker (README sửa ở `K-22`) | Worker health | test health worker | 0,75 | ✅ 5aec684 (W-0360) · UT-WORKER-HEALTH-CIRCUIT-01/02 |

### `L6` — Số điện thoại / PII · 7,5 giờ

| Mã | Nguồn | Việc | File chính | Kiểm | Giờ | Trạng thái |
| --- | --- | --- | --- | --- | ---: | --- |
| `K-37` | 🆕 | `PiiGuard` thiếu tên field `phonee164` / `msisdn` (`PiiGuard.cs:7-18`) → kiểm tên field trong audit (`PostgresAuditLogger.cs:98`) cho qua key `phone_e164` | `PiiGuard.cs` | test tên field | 0,25 | ✅ 1e7951c · 1337/1337 (W-0361) · UT-PII-FIELD-01/02 |
| `K-38` | `B15` | Bù B15: `[property: JsonIgnore]` cho `DirectPhoneE164` ở 2 record (+ ghim lại `ProviderPorts.cs` ở dial-token validator và template W-0183); `UT-PHONE-TOSTRING-04` (JSON không in số), `-05` (reflection: mọi record có property E164/Phone/Msisdn phải tự che `ToString`); `PiiGuard.IsSafeText(printed)` + ca null; che `ToString` của `ScriptPreview`, `ScriptInputSnapshot` (`VietnameseOrderScriptRenderer.cs:66-91` đang in tên khách, vùng giao) | `ProviderPorts.cs`, dispatch store, renderer | UT-PHONE-TOSTRING-04/05 | 1,25 | ✅ 1e7951c · 1337/1337 (W-0361) · UT-PHONE-TOSTRING-04/05, UT-SCRIPT-TOSTRING-01 |
| `K-39` | 🆕 | `SEC-ROT-05` ("không cột persist nào giữ số thật") xanh vô nghĩa từ W-0310: chỉ lọc tên `PhoneNumber/RawPhone/Msisdn/Destination`, bỏ sót `PhoneE164` → chuyển sang danh sách cho phép tường minh (`PhoneE164` là ngoại lệ duy nhất, gắn `OD-V1-18`); thêm cột E.164 mới ⇒ test đỏ. Câu §1 của `secret-inventory` sửa ở `CB-10` | `SecretRotationTests.cs:214-247` | SEC-ROT-05 | 1 | ✅ 1e7951c · 1337/1337 (W-0361) · SEC-ROT-05 đọc model EF |
| `K-40` | 🆕 `A2` | MOCK/LAB/sandbox vẫn lưu `phone_e164` dù không bao giờ đọc (chỉ `ProductionDialTokenVault` đọc — đã kiểm 25/09) → lưu `NULL` khi không phải `PRODUCTION_REAL` (`TaskIntakeService.cs:799`); chỉnh assert `PhoneNumberContainmentTests.cs:151` và seed của `COMP-DSAR-08..12` (ghi số bằng SQL). Không đổi contract. Nếu Sếp chọn (ii) ở `Q-32` thì đây là bước 2 của (ii) | `TaskIntakeService.cs` | IT-PHONE-CONTAIN-01, COMP-DSAR-* | 2 | ✅ 1e7951c · 1337/1337 (W-0361) · IT-INTAKE-NUMBER-DB-05; COMP-DSAR-08..12 không cần sửa (seed ghi thẳng entity) |
| `K-41` | `B11` bước 0 | Mở rộng test chặn số: `IT-PHONE-CONTAIN-02` (POST HTTP chỉ có số; replay 200; đổi body 409; số sai pattern 400 — body và log không chứa NSN `900000001`); test 01 chạy `AnalyticsEtlJob` một lượt trước khi quét, gọi thêm `/sim-channels`, `/scripts`, `/integration-status`, GET feature-flags, GET `/call-jobs/{id}`; tìm NSN và chạy `PiiGuard.IsSafeText` trên mọi ô text; bộ ghi log lưu `exception.ToString()`. `IT-PHONE-CONTAIN-03` (đường quay production) làm ở `Q-28.2` | `tests/Ivr.IntegrationTests/Governance` | IT-PHONE-CONTAIN-01/02 | 3 | ✅ 1e7951c · 1337/1337 (W-0361) · IT-PHONE-CONTAIN-01/02; lối quay production vẫn ở Q-28.2 |

### `L7` — Đường quay số · 8,5 giờ · điều kiện trước SIP-04

| Mã | Nguồn | Việc | File chính | Kiểm | Giờ | Trạng thái |
| --- | --- | --- | --- | --- | ---: | --- |
| `K-42` | `B13` | Bù B13: `MockTelephonyDispatchGateway.cs:219, :226` dùng `executionContext.ToDomainMode()`; test kiến trúc `ARCH-DISPATCH-MODE-01` (cấm `ExecutionMode.LabRealSim/ProductionReal` trong `*DispatchGateway.cs`); `UT-AST-SCRIPT-01` (kịch bản thiếu phê duyệt Lab bị từ chối lúc dispatch, `DialAsync` không được gọi); phòng thủ kép ở `SpeechSynthesisService.cs:59-64` — coi là production khi tham số **hoặc** chế độ deployment là `PRODUCTION_REAL` (`UT-TTS-WHITELIST-08`); tuỳ chọn gom 3 bản đổi chuỗi→enum về `ExecutionModes.Parse` | Telephony, Speech | 3 test mới | 2,25 | ⬜ |
| `K-43` | 🆕 | `store.LoadAsync` nằm ngoài `try` ở cả 2 gateway (Asterisk `:58-64`, Mock `:207-213`) → lỗi lúc load bỏ lease treo: kênh `RESERVED` 120 s → `QUARANTINED` 600 s + 1 lỗi DT-04; job thành `HELD_LEASE_RECOVERY` → đưa vào `try`, `FailAsync` với kênh lành. Kết cục riêng cho task bị thu hồi làm trong gói C13 | 2 gateway | `IT-TEL-LOAD-FAIL-01` | 1,25 | ⬜ |
| `K-44` | 🆕 | Gate chỉ kiểm một lần, trước bước chuẩn bị audio (TTS tới 120 s) → bấm dừng khẩn cấp lúc đang tổng hợp giọng thì cuộc gọi vẫn đi → gọi lại `DispatchGate` sát `DialAsync` (kế hoạch SIP `:191` đã đòi) | `AsteriskSchedulerDispatchGateway.cs` | `UT-AST-GATE-03` | 1 | ⬜ |
| `K-45` | 🆕 | Kết cục `healthy=true` gọi `RecordHealthy` xoá `FailCount` dù kênh chưa hề được dùng (lỗi render, lỗi dữ liệu) → xoá luôn 2 lỗi SIM thật trước đó, kênh hỏng lại được ưu tiên → thêm kết cục "không đụng kênh" | `SimChannelFailurePolicy.cs:34-39`, dispatch store | test kênh có FailCount=2 | 1 | ⬜ |
| `K-46` | 🆕 | Có store ghi thẳng `AuditLogEntity`, không qua `PostgresAuditLogger`/PiiGuard (`PostgresTelephonyDispatchStore.cs:519-535`, `PostgresSchedulerStore.cs:636`) → `PersistenceInvariantValidator` kiểm mọi `AuditLogEntity` | Persistence | test audit PII | 1 | ⬜ |
| `K-47` | 🆕 `V6-6` | Mất luồng sự kiện ARI: `ReceiveAsync` ném `WebSocketException` ra ngoài (`AsteriskAriSimGateway.cs:445-447, :475`), bỏ qua `FailOpenCalls` → cuộc đang mở rơi vào timeout, bị ghi là khách không nghe/không bấm **có tính lượt** → bọc `try/finally`, gọi `FailOpenCalls("ASTERISK_EVENT_STREAM_LOST")` ⇒ `NETWORK_ERROR`, `is_counted=false` | `AsteriskAriSimGateway.cs` | test WS server giả ngắt TCP giữa lúc chờ DTMF | 2 | ⬜ |
| `K-52` | 🆕 (tìm thấy 25/09 lúc làm L2) | Job bị eligibility giữ vì hết dung lượng (`CAPACITY_HELD`, `eligible=false` — `EligibilityRepository.cs:287-289`) **không bao giờ đóng và Module 3 không nhận callback nào**: `CAPACITY_HELD` chỉ được ghi, không nơi nào đọc lại (ngoài CHECK), còn sweep hết hạn chỉ quét job `eligible IS TRUE` ở 4 trạng thái (`PostgresSchedulerStore.cs:372-386`). Đơn 24/7 COD treo mà Module 3 không biết, trái bảng map (`IVR_CAPACITY_EXCEPTION` → người trực). Sửa: cho sweep đóng cả job `CAPACITY_HELD` khi hết cửa sổ với `IVR_CAPACITY_EXCEPTION` như nhánh capacityMiss (sửa đồng bộ bản chép vị từ ở `SchedulerQueueBacklog.cs`); `gitnexus_impact` `CloseMissedDeadlinesAsync` = HIGH. Làm cùng/sau gói C13 nếu chạm cùng câu SQL | `PostgresSchedulerStore.cs`, `SchedulerQueueBacklog.cs` | `IT-SCH-CAPACITY-HELD-01` (giữ ở eligibility → hết cửa sổ → một kết quả `IVR_CAPACITY_EXCEPTION` + callback) | 2 | ⬜ |
| `K-53` | 🆕 (tìm thấy 25/09 lúc làm L4) | `PiiSafeLogRecordProcessor` chỉ xuất thuộc tính có trong allowlist, và `ExceptionType` không có trong đó, nên tên loại lỗi của log hangup (`K-31`, W-0359) và của `FeatureFlagPlatform` không tới OTLP; dòng log chỉ còn `ReasonCode`, `AttemptId`. Thêm `ExceptionType` vào allowlist: giá trị là tên kiểu .NET, không phải dữ liệu khách | `src/Ivr.Infrastructure/Observability/IvrObservability.cs` | test processor giữ `ExceptionType` | 0,5 | ⬜ |

### `L8` — N1: phần không phụ thuộc quyết định · 6 giờ

> Luật quá độ ngày 24/09 do Tech Lead chốt; chief ghi "làm ngay", và luật này thắng S4 cho production (FIX_M8:154).
> Clip do **VieNeu render offline** vẫn đúng quyết định "chỉ VieNeu" của owner — chỉ đổi thời điểm render, không
> có giọng người. Phần cần quyết định (phạm vi đọc, nguồn clip, nơi lưu, cách fail) ở `Q-11`, `Q-25`, `Q-26`, `Q-29`.

| Mã | Nguồn | Việc | File chính | Kiểm | Giờ | Trạng thái |
| --- | --- | --- | --- | --- | ---: | --- |
| `K-48` | 🆕 `N1` | Playlist >64 mục hoặc >5 phút (`RenderedAudio.cs:136-148`) ném lỗi rơi nhánh `_` → cách ly SIM → bọc thành `TtsSynthesisException` `TTS_PLAYLIST_TOO_LONG` (kênh lành) | Speech, gateway | `UT-N1-PLAYLIST-01` | 0,5 | ⬜ |
| `K-49` | `N1` bước 4 | Chặn cứng sinh giọng ở `PRODUCTION_REAL` trên **mọi** đường (cả đường nguyên câu, không chỉ đoạn động): DI đăng ký `RuntimeSynthesisForbiddenTtsProvider`, không đăng ký HttpClient TTS; validator từ chối `EXTERNAL_CONFIGURABLE` ở `PRODUCTION_REAL`; `SpeechSynthesisService` ở ProductionReal không gọi `ITtsProvider`; thiếu audio ⇒ fail-closed (`AudioError`, kênh lành, `IVR_TECHNICAL_EXCEPTION` không tính lượt). Nhánh LAB giữ nguyên từng byte | Speech DI, `SpeechSynthesisService.cs` | `UT-N1-NOSYNTH-01..03` | 3 | ⬜ |
| `K-50` | `N1` bước 5–6 | Helm: guard prod **từ chối** `tts.enabled=true` (nêu lý do luật 24/09); bỏ wire endpoint loopback trong template prod; đánh dấu `values-prod-tts.draft.yaml` SUPERSEDED; gate mới `speech-transition-gate.mjs` (đọc `values-prod.yaml`, không cần docker) + đăng ký `gate-invocations.json`; đảo ca dương prod của `tts-helm-selftest.mjs` | `deploy/helm`, `deploy/ci` | `SPEECH_TRANSITION_GATE_PASS`, `TTS_HELM_SELFTEST_PASS` | 2,5 | ⬜ |

### Tuỳ chọn

| Mã | Nguồn | Việc | Kiểm | Giờ | Trạng thái |
| --- | --- | --- | --- | ---: | --- |
| `K-51` | `C12` | Test kiến trúc cấm tham chiếu `OptOutSuppressionPolicy`/`SuppressionProposer` từ `src` (hai file bị ghim — không sửa file) | `ARCH-OPTOUT-DEAD-01` | 0,5 | ⬜ |

---

## §5 — PHẦN II: Cần quyết định

### Bảng tổng (trạng thái quyết định cập nhật ở đây)

| Mã | Quyết định | Nguồn | Người quyết | Khuyến nghị | Công sau khi quyết | Trạng thái |
| --- | --- | --- | --- | --- | ---: | --- |
| `Q-01` | Hình thức phát hành lại IR-07 🔥 | C7, C9, C15 | Toàn (báo chief) | PA2 | 0,25 | ✔ PA2 · 25/09 · Toàn |
| `Q-02` | Câu chữ đính chính C23 🔥 | C23, N1 | Toàn | PA2 | 0,5 | ✔ PA2 · 25/09 · Toàn |
| `Q-03` | C22 khi chưa có nguyên văn bảng v1 🔥 | C22 | Toàn | PA3 | 1 | ✔ PA3 · 25/09 · Toàn |
| `Q-04` | Các ghi "Sếp + Toàn 17/09" | A1 bước 5 | Toàn | PA2 | 0,3 | ⏸ Toàn |
| `Q-05` | Thẻ kill switch trên console 🆕 | — | Toàn (báo Tech Lead) | PA1 | 0,5 | ⏸ Toàn |
| `Q-06` | Vị từ `revoked_at` khi nào | B7 | chief | PA2 | 0 | ✔ PA2 · 16/09 · chief |
| `Q-07` | Endpoint capacity-incidents | B9 bước 1 | Toàn (chief duyệt) | PA3 | 0 (8 sau) | ⏸ Toàn |
| `Q-08` | Tờ trình rủi ro cho Sếp | A1 bước 1, A3, A4, C5 | Toàn (chief chuyển) | PA1 | 1,5 | ⏸ Toàn |
| `Q-09` | Phép thử schema B5 🆕 | B5 | Toàn (báo chief) | PA2 | 1,5 | ⏸ Toàn |
| `Q-10` | Ví dụ sandbox ca đơn đêm | C15 bước 2 | Toàn | PA2 | 2 | ⏸ Toàn |
| `Q-11` | Nguồn clip N1 | N1 | Toàn | PA1 | 0 | ⏸ Toàn |
| `Q-12` | Guard tên hàng lệch intake/lúc quay 🆕 | B16 | Toàn (báo chief) | PA1 | 2 | ⏸ Toàn |
| `Q-13` | Mô hình biên buổi sáng 🔥 | B17 | Toàn (báo chief) | PA1 | 3 | ✔ PA1 · 25/09 · Toàn |
| `Q-14` | Pipeline main không thể xanh 🆕 | dòng 714 | Toàn + chief (PA1 cần Sếp) | PA2 → PA1 | 4 | ⏸ Toàn |
| `Q-15` | 3 biến "an toàn" không ai đọc 🆕 | dòng 106 | Toàn (báo chief) | PA1 | 2,5 | ⏸ Toàn |
| `Q-16` | Lớp lỗi "intake nhận – lúc quay từ chối" | B16 | Toàn đề xuất, chief duyệt | PA2 | 3 | ⏸ Toàn |
| `Q-17` | Hạ S2 / `legal_gate` / mirror | A1 bước 2, A3, A4, B8 | Toàn (theo hướng chief) | PA1 | 3 | ⏸ Toàn |
| `Q-18` | Siết gate tts-provenance | A1 bước 6 | Toàn đề xuất, chief duyệt | PA1 | 3,5 | ⏸ Toàn |
| `Q-19` | Giới hạn tuổi phát lại callback | C21 | Toàn (báo chief) | PA2 | 4,5 | ⏸ Toàn |
| `Q-20` | Thu hồi task có phát callback không | C13 | chief | PA1 | 0 | ✔ PA1 · 25/09 · Toàn (theo đề xuất M8, không gửi chief) |
| `Q-21` | Tên trường callback trên dây | C10 | chief | PA1 | 0 | ✔ PA1 · 25/09 · Toàn (theo đề xuất M8, không gửi chief) |
| `Q-22` | Vùng tối 21:00:30–21:08 🆕 | C15, B17 | chief | PA2 | 1,75 | ✔ PA2 · 25/09 · Toàn (theo đề xuất M8, không gửi chief) |
| `Q-23` | Token tĩnh Order Core luôn được nhận 🆕 | — | Tech Lead qua chief (+Platform) | PA1 | 1,5 | ✔ PA1 · 25/09 · Toàn (theo đề xuất M8, không gửi chief) |
| `Q-24` | Tài nguyên server test ngoài cấp phát | C18 | chief | PA3 | 2 | ✔ PA3 · 25/09 · Toàn (theo đề xuất M8, không gửi chief) |
| `Q-25` | Nơi lưu ngân hàng clip | N1 | Toàn + chief + Platform | PA1 | trong N1 | ✔ PA1 · 25/09 · Toàn (theo đề xuất M8, không gửi chief) |
| `Q-26` | Đơn không đọc trọn được | N1 | Tech Lead | PA1 | trong N1 | ✔ PA1 · 25/09 · Toàn (theo đề xuất M8, không gửi chief) |
| `Q-27` | Q.850 cause 20 🆕 | B12 | chief trình Sếp; M3 đối ký | PA2 | 3,5 | ✔ PA2 · 25/09 · Toàn (theo đề xuất M8, không gửi chief) — chốt hẳn sau khi đo MobiFone thật (B12) |
| `Q-28` | DispatchGate nhánh production 🆕 | C16, B12 | Tech Lead; Sếp ký pilot → mở | PA2 | 8 | ✔ PA2 · 25/09 · Toàn (theo đề xuất M8, không gửi chief) — khoá HMAC còn chờ Platform thì W-0008 đi PA3 |
| `Q-29` | Phạm vi đoạn động N1 | N1, C23 | Tech Lead; chief đối chiếu PACK-09 | PA2 | 31 | ✔ PA2 · 25/09 · Toàn (theo đề xuất M8, không gửi chief) |
| `Q-30` | Ba người duyệt kịch bản (S1) | V6-2 | Sếp | PA2 | 1 | ⏸ Sếp |
| `Q-31` | Mô hình phiên cho capacity | B1 bước 3 | Sếp + M3 | PA1 | 0 | ⏸ Sếp |
| `Q-32` | Phương án B `phone_e164` | A2, B11, C16, C19 | **Sếp** (phiếu 25/09 mục B2) | PA2 | 3–20 | ⏸ Sếp |
| `Q-33` | `program_code` | C1 | M3 chọn, chief chốt | PA1 | 0 | ⏸ M3 |
| `Q-34` | Phát hành `golden_hour_session_id` | C3, C4, C9 | chief + M3 | PA2 | 10–12 | ⏸ M3 |
| `Q-35` | Xác thực callback sang M3 | C11 | anh Mạnh/Sếp + Platform | PA1 | trong C11 | ⏸ M3 |
| `Q-36` | 4 file softphone văn phòng | B2 | anh Mạnh | PA2 | 0 (không phải việc dev) | ⏸ anh Mạnh |

> **25/09 — `Q-20…Q-29`:** Toàn chốt theo khuyến nghị M8 thay cho việc gửi chief; tin gộp §7 không gửi. Phần của
> các phương án cần người khác làm vì thế **chưa ai được báo**: chief sửa §14 trỏ về OAS (`Q-21`), ngoại lệ tạm trên
> server test (`Q-24`), M3 đối ký bảng map (`Q-27`), Platform giữ khoá HMAC và Sếp ký chuyển pilot → mở (`Q-28`).

### Nhóm 1 — Toàn quyết (`Q-06` đã có phán quyết chief)

#### `Q-01` — Hình thức phát hành lại IR-07 🔥 · Toàn (báo chief)

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Chỉ thêm đính chính ở cuối phiếu, như chief giao | Đúng chữ chỉ đạo | Bảng điền nhanh (`:25-54`), câu `:182`, đầu phiếu (`draft.30`, `SENT`) vẫn sai. Luật 2 của phiếu (`:68`: ô trống = đồng ý mặc định) ⇒ M3 điền bảng đầu có thể thành "đồng ý chờ 24 giờ" | 0 |
| PA2 | Cuối phiếu + đánh dấu tại chỗ tối thiểu: `M3-13`, `M3-30` ghi "RÚT — xem 25/09"; `M3-02` ghi "xem 25/09"; đầu phiếu ghi `draft.33` và "gửi lại 25/09" (có tiền lệ sửa tại chỗ ngày 18/09 ở `:634`, `:684`) | Người điền không thể bỏ sót | Lệch nhẹ nguyên tắc giữ nguyên chữ (anh Mạnh báo chưa từng nhận bản 17/09) | 0,25 giờ |
| PA3 | Phát hành một IR-07 mới sạch | Dễ đọc nhất | Rà lại mọi con trỏ (IR-08 `:241`, `00-CHUA-XONG:13`); dễ chép sai | 1–2 giờ |

**Khuyến nghị: PA2** — rẻ nhất mà chặn được việc M3 ký nhầm theo luật im lặng.

| Việc sau khi quyết | Giờ | Trạng thái |
| --- | ---: | --- |
| `Q-01.1` Đánh dấu tại chỗ theo PA đã chọn (gộp `L2`) | 0,25 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) |

#### `Q-02` — Câu chữ đính chính C23 (lời hứa VieNeu đọc lúc gọi) 🔥 · Toàn

Lời hứa nằm ở IR-07 `A-8 :139`, `M3-05 :214`, `E-5 :598`, đính chính 18/09 `:689`, `:692`; IR-06 `:503`
(`pronunciation_hints`); `00-CHUA-XONG:52, :262-265`; register `:74`, `:110`. Luật quá độ 24/09 cấm sinh giọng lúc
gọi ở production.

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Theo nguyên văn chief: audio dựng sẵn, ghép clip, danh mục 19 SKU, E-5 đổi sang nghe bản ghép, `pronunciation_hints` chỉ dùng render trước | Cụ thể, M3 biết chuẩn bị gì | Hứa một ngân hàng clip chưa có; chief ghi 19 SKU, repo ghi 20; kịch bản v3 đọc món/vùng trong khi PACK-09 đọc mã đơn + tổng tiền — nếu `Q-29` chọn (b) phải đính chính lần nữa | 0,75 giờ |
| PA2 | Trung tính: rút lời hứa đọc lúc gọi; ghi "lời thoại production là audio dựng sẵn từ template đã duyệt; phạm vi phần động (tên hàng/vùng hay mã đơn) Tech Lead đang chốt; `pronunciation_hints` không dùng lúc gọi; M3 chưa dựng dữ liệu cho việc đọc tên hàng" | M3 dừng đúng việc sai mà không nhận lời hứa mới | Phải thêm một dòng khi `Q-29` chốt | 0,5 giờ |
| PA3 | Chờ `Q-29` rồi đính chính một lần | Chỉ sửa một lần | IR-07 gửi lại vẫn mang lời hứa trái luật quá độ; M3 dựng dữ liệu sai | 0 |

**Khuyến nghị: PA2**, nâng lên PA1 khi `Q-29` chốt.

| Việc sau khi quyết | Giờ | Trạng thái |
| --- | ---: | --- |
| `Q-02.1` Viết đính chính 4 chỗ IR-07 + IR-06 `:503` + `00-CHUA-XONG` + register (gộp `L2`) | 0,5 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) |

#### `Q-03` — C22 khi chưa có nguyên văn bảng `ivr-cancel-reason-map.v1` 🔥 · Toàn (chief cấp văn bản)

Bảng v1 nằm ở `D:\9 module\_SPEC\FIX_M8.md` — **không có trên máy này**. Bảng v0 còn nguyên ở IR-06 `:821-827`:
`IVR_CONFIRMATION_WINDOW_EXPIRED → IVR_CONFIRMATION_EXPIRED` cho cả hai chương trình, bất kể khách đã được gọi tới
hay chưa.

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Chờ nguyên văn rồi chép một lần | Đúng chỉ đạo; ghim một lần | Bảng v0 nằm đó tới lúc có văn bản; nếu IR-07 gửi trước, M3 đọc v0 và **hủy hàng loạt đơn 24/7 COD mỗi khi IVR gặp sự cố** | 0,5 giờ (sau) |
| PA2 | Tự dựng v1 từ lời chief | Nhanh | Trái chỉ đạo "chép đúng, không tự dựng"; ít nhất 4 ô chưa có nguồn | 0,75 giờ |
| PA3 | Vô hiệu v0 ngay bằng đính chính có ngày, **chỉ gồm các dòng chắc chắn**, trỏ tới bảng chuẩn ở FIX_M8; dán nguyên văn khi nhận | Gỡ ngay dòng gây hủy hàng loạt; không bịa ô nào | Ghim IR-06 hai lần | 0,75 + 0,25 giờ |

Các dòng chắc chắn (lấy từ lời chief ở audit `:366-368`, `:374`, `:406-408`): đơn 24/7 COD — `NO_ANSWER_FINAL` → hủy
`IVR_NO_ANSWER_MAX`; `WINDOW_EXPIRED` + EXPIRE → `IVR_CONFIRMATION_EXPIRED`; `WINDOW_EXPIRED` + HOLD_ADMIN_REVIEW
hoặc `CAPACITY_EXCEPTION` → không tự hủy, chuyển người trực, nếu hủy thì `IVR_CONFIRMATION_INVALID` fault NONE.
Đơn Giờ Vàng — hết hiệu lực xác nhận theo flow 05, lỗi phía IVR → `IVR_CONFIRMATION_INVALID`. Khoá bảng =
chương trình + `result_type`, riêng `WINDOW_EXPIRED` thêm action; liệt kê 5 `result_reason` của `NO_ANSWER_FINAL`;
sửa câu "WRONG_INPUT không bao giờ tới M3" (`:829-831`) và "action chỉ là advisory" (`:852`). Ô **chờ nguyên văn**:
`CUSTOMER_CANCELLED`, `INVALID_PHONE_FINAL` ở cả hai chương trình; mã lý do `WINDOW_EXPIRED` + EXPIRE của Giờ Vàng.

**Khuyến nghị: PA3**, và xin chief nguyên văn trong ngày (`CB-02`).

| Việc sau khi quyết | Giờ | Trạng thái |
| --- | ---: | --- |
| `Q-03.1` Đính chính vô hiệu v0 (gộp `L2`) | 0,75 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) |
| `Q-03.2` Dán nguyên văn v1 + ca sandbox "IVR hỏng suốt cửa sổ → M3 không hủy đơn 24/7 COD" | 0,25 | ⛔ chief (`CB-02`) — chưa xin: 25/09 Toàn quyết không gửi tin gộp |

#### `Q-04` — Các ghi "Sếp + Toàn 17/09" (T7, S3, S4, phương án B, OD-V1-24) · Toàn

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Chỉ chờ Sếp ký N04/B2 | 0 công | Repo tiếp tục khẳng định "Sếp + Toàn" như sự thật — đúng lỗi A1 | 0 |
| PA2 | Ghi chú "chưa có chữ ký Sếp trong repo — chờ phiếu 25/09 mục N04/B2" ở tài liệu gốc: `vuong-mac-va-quyet-dinh-2026-09-17.md:3-4` + §1, phiếu 17/09 `:6-17`, các dòng sổ liên quan, `retention-period-proposal.md:3-4` (không file nào bị ghim) | Sửa đúng nguồn sự thật, rẻ | — | 0,3 giờ |
| PA3 | Ghi chú cả trong code và contract (mô tả OAS, chú thích migration) | Nhất quán tối đa | Migration đã áp không được sửa; đổi mô tả OAS kéo theo bump contract + portal | ≥3 giờ |

**Khuyến nghị: PA2** (gộp `L2` hoặc `L3`). Repo còn tự mâu thuẫn về người quyết S3 (`vuong-mac:15` "Toàn xác nhận",
`:350` "Toàn chốt", `retention-period-proposal.md:17` "Toàn quyết định") — ghi chú cùng lượt.

#### `Q-05` — Thẻ `DIAL_KILL_SWITCH` trên console 🆕 · Toàn (báo Tech Lead)

Thẻ ghi "no call is dispatched in any mode" (`AdminConfigReadService.cs:325-333`), nhưng mock gateway không đọc cờ DB
(chỉ gateway Asterisk gọi `DispatchGate`).

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Sửa chữ trên thẻ cho đúng | Rẻ, trung thực | Sandbox M3 vẫn không diễn tập được kill switch | 0,5 giờ |
| PA2 | Cho mock gateway tôn trọng cờ DB (seed e2e hạ cờ) | Sandbox giống production | Đổi setup e2e/sandbox; dễ làm e2e ngừng quay | 2 giờ |
| PA3 | Cả hai | Đầy đủ | Tốn nhất | 2,5 giờ |

**Khuyến nghị: PA1** ngay; PA2 khi M3 cần diễn tập kill switch trên sandbox.

#### `Q-06` — B7: thêm vị từ `revoked_at` vào deadline sweep khi nào · chief — **✔ PA2 · 16/09**

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Thêm ngay | Test được ngay (3 test đã tự ghi `revoked_at`) | Vô tác dụng khi chưa có chỗ ghi; đổi lỗi "đóng sai" thành "không bao giờ đóng" (job treo, vẫn bị đếm trong `pendingJobs`) | 1 giờ |
| PA2 | Chờ, ship cùng gói C13 | Không rủi ro (lỗ chưa kích hoạt được) | B7 treo theo nhịp M3 | 2 giờ trong gói |
| PA3 | Làm phần nội bộ ngay (sweep đóng im lặng, fence 2 trả kênh), để lại endpoint | Gói C13 còn ~2 ngày | Tự chốt "không callback" (quyền chief); fence 2 sạch cần migration mở CHECK trước chữ ký — lặp lỗi W0249 | 6–8 giờ |

**Khuyến nghị: PA2 — đã là phán quyết chief 16/09.** Kèm hai việc giấy: sửa dòng "việc cần làm" của B7 thành "đóng
job lúc thu hồi + sweep đóng im lặng" (bỏ "chỉ thêm vị từ"), và xin chief chốt `Q-20` ngay (không phụ thuộc M3-14).

#### `Q-07` — B9 bước 1: endpoint capacity-incidents · Toàn (chief duyệt)

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Chờ M3 như phán quyết 16/09 | 0 | Chưa ai hỏi M3 ⇒ chờ vô hạn; B9 treo ở 55% | 0 |
| PA2 | Làm ngay bản tối thiểu: route tầng Read, lọc `status/scope/program/opened_at`, `page/page_size`, không trả `session_id`/`reason`; không cần migration (đã có index `(status, opened_at)`); contract `draft.34` | M3 có API thật khi dựng console | Thêm một lần đổi contract khi M3 chưa nhận `draft.30…33` | 8 giờ |
| PA3 | Hỏi M3 (`K-17`) với shape của PA2 làm mặc định; code khi M3 trả lời | Gần như 0 công bây giờ, đúng quy trình, có điểm kết thúc | Chậm hơn | 0 bây giờ · 8 giờ sau |

**Khuyến nghị: PA3.**

#### `Q-08` — Tờ trình rủi ro cho Sếp ký (A1 bước 1; gộp A3, A4, C5) · Toàn (chief chuyển)

Nội dung bắt buộc, một trang:
1. **Điều đã đổi so với lần báo 16–17/09:** số lưu dạng đọc được + giữ vĩnh viễn (trước là tham chiếu đục, lưu 90 ngày). Phiếu 17/09 `:69-73` còn mô tả mô hình cũ.
2. **Bảy rủi ro S2**, câu chữ từ `vuong-mac:73-81`, tình trạng 22/09 từ `docs/evidence/W-0340/S2-owner-review.md:10-18`; điểm 4 ghi VieNeu chỉ render trước theo luật 24/09.
3. **Giá của lối lùi:** Sếp mua dịch vụ giữ khoá + M3 dựng bộ cấp token; tới lúc đó production nhận 0 task.
4. **Ô chọn:** B2 (`Q-32`); S2 (Sếp tự ký / ủy quyền bằng văn bản cho Toàn kiêm Legal/Privacy, ghi phạm vi + thời hạn / hoãn); quyền dùng VieNeu; S1 (`Q-30`).
5. **Nói thẳng:** bản xác nhận 22/09 là Toàn tự xác nhận, không thay chữ ký Sếp; khách thật vẫn tắt; gọi rõ "PA-B số điện thoại" để khỏi lẫn với "S7-B" ở OD-V1-24.

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | File md `plan/ivr-orther/to-trinh-sep-rui-ro-2026-09-25.md` + PDF vào `.artifacts/`; commit + push; chief chép sang folder của chief; bản ký lưu ở chief, repo giữ tham chiếu + sha256 | Truy vết theo commit; không cần kênh riêng; hash bản ký chính là `approval_reference` ngoài repo mà `Q-18` cần | Tài liệu pháp lý nằm trong repo dev (không có PII) | 1,5 giờ |
| PA2 | Chỉ gửi PDF ngoài repo; repo ghi tham chiếu + hash | Văn bản nằm ngoài repo ngay từ đầu | Không đối chiếu nội dung bằng git; dễ lệch bản | 1,5 giờ |
| PA3 | Không làm tờ trình riêng; gửi đoạn nội dung để chief chèn vào phiếu 25/09 mục B2 | Sếp chỉ đọc một văn bản | Trái chỉ đạo 17/09 (Sếp ký trên tờ trình của M8); chief phải biên tập phần kỹ thuật | 1 giờ |

**Khuyến nghị: PA1.**

#### `Q-09` — B5: phép thử schema cho DB đã từng drop bảng console 🆕 · Toàn (báo chief: trái tiền đề audit dòng 208)

W0105 tạo `ivr_console_accounts` với cột theo thứ tự `id, username, display_name…`; P03 và bản `Down` cũ của W0122
tạo theo alphabet `id, anonymized_at, created_at…` ⇒ `attnum = 2` phân biệt được (suy từ code, **chưa chạy thử**).

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Giữ bản W-0354, chỉ dọn câu mâu thuẫn (`K-20`) | Rẻ, đúng chữ audit | DB đã ở HEAD vẫn "không xác định được"; phải dựa nhật ký mà runbook tự nhận là không có (`:124-126`) | 0 |
| PA2 | Thêm phép thử `attnum` + assert ở cả hai nhánh `IT-SCHEMA-EXPAND-07` | Trả lời dứt khoát cho mọi DB, bền qua dump/restore, có test giữ | Dựa thứ tự cột vật lý — ai "chuẩn hoá" P03 thì test đỏ (đúng ý) | 1,5 giờ |
| PA3 | Ghi `attnum` như "tín hiệu phụ", không có test | Rẻ | Đưa khẳng định chưa kiểm vào runbook — đúng loại lỗi B5 vừa sửa | 0,5 giờ |

**Khuyến nghị: PA2.**

#### `Q-10` — C15 bước 2: ví dụ sandbox cho ca đơn đêm · Toàn

Sau `W-0354`, e2e và sandbox **không bao giờ** chạm nhánh từ chối W-0298 nữa — M3 mất chỗ diễn tập ca đêm.

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Overlay "giờ thật": API về 08:00–21:08 | Đúng chữ audit | Chỉ thấy ca đêm khi chạy lúc đêm — phụ thuộc đồng hồ, trái chú thích của sandbox | 0,5–1 giờ |
| PA2 | Overlay API "gần như luôn đóng" (Start=0/End=1) + ví dụ tự động `TASK-M3-NIGHT` trong `sandbox-examples.mjs`: `200 TASK_BLOCKED_OPERATIONAL` → eligibility 404 → gửi lại vào sandbox thường với cửa sổ + key mới → ACK; thêm một mục IR-08 | Tất định; diễn tập đúng đường "giữ rồi gửi lại" của M3; không đụng test parity | Thêm một file | 1,5–2 giờ |
| PA3 | Tham số hoá `${IVR_SANDBOX_API_WINDOW_END:-1440}` | Một file | Phải dạy test parity đọc `${…}`; quên biến ⇒ sandbox đóng im lặng | 1–1,5 giờ |

**Khuyến nghị: PA2**, làm sau `Q-13` để ghi đúng mốc biên.

#### `Q-11` — N1: nguồn clip · Toàn

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | VieNeu render offline, 3 giọng OD-VOICE-06 | Đúng S4 ("chỉ VieNeu"); dùng lại `render-fixed-speech.mjs` và manifest nghe duyệt | Số clip × 3; `legal_gate` phủ cả ngân hàng clip | trong N1 |
| PA2 | VieNeu offline, 1 giọng cho mọi miền | Clip và việc nghe còn 1/3 | Đảo OD-VOICE-05; phải chọn một bộ từ nghìn/ngàn cho cả nước; render lại đoạn cố định | trong N1 |
| PA3 | Giọng người hoặc vendor | — | S4 cấm; cần Sếp đảo S4 + hợp đồng giọng | — |

**Khuyến nghị: PA1.**

#### `Q-12` — Guard tên hàng lệch giữa intake và lúc quay 🆕 · Toàn (báo chief: nới guard trên câu thoại cuối)

Intake kiểm tên hàng bằng guard sản phẩm (bỏ 4 từ tổ/đường/thôn/ấp — W-0243, `UT-PII-PRODUCT-01`); lúc quay,
renderer (`VietnameseOrderScriptRenderer.cs:189`) và `SpeechPrivacyGuard.cs:16` chạy guard **toàn văn** ⇒ "Tổ yến",
"Đường phèn", "Ấp trứng" bị chặn ⇒ 2 lần technical exception rồi `WINDOW_EXPIRED`, trong khi M3 nhận ACCEPTED lúc gửi.
(Đọc code, chưa tái hiện.)

🆕 25/09 (`W-0359`): cùng câu hỏi ở **tên khách**. `customer_display_name` không dấu có một trong các từ địa danh
mơ hồ bị guard toàn văn của chính field đó từ chối `422` ở intake, trái tinh thần `W-0105` (không bắt ai đổi họ).
Quyết `Q-12` nên nói luôn cho tên khách.

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Guard theo đoạn: đoạn `items_spoken` dùng guard sản phẩm, phần còn lại guard toàn văn | Khớp quyết định W-0243; thay đổi nhỏ | Nới guard trên câu thoại cuối — cần test chéo | 2 giờ |
| PA2 | Intake chặt như guard toàn văn | Nhất quán tức thì | Đảo W-0243; tên sản phẩm thật bị từ chối | 1 giờ |
| PA3 | Chỉ nhận tên SKU từ danh mục công khai đã duyệt | Chặt nhất | Cần danh mục (chưa có — `CB-18`); chỉ hợp khi `Q-29` chọn đọc tên hàng | 4 giờ + danh mục |

**Khuyến nghị: PA1**; chuyển PA3 nếu `Q-29` chọn đọc tên hàng.

| Việc sau khi quyết | Giờ | Trạng thái |
| --- | ---: | --- |
| `Q-12.1` Sửa guard + test xuyên intake → dispatch với "Tổ yến" | 2 | ⬜ |

#### `Q-13` — B17: mô hình biên buổi sáng 🔥 · Toàn (audit giao dev chọn; báo chief vì đổi tài liệu gửi M3)

Guard W-0298 xét **từng** thời điểm T0+offset; scheduler thì **gọi bù** attempt quá hạn ngay khi mở giờ (chú thích
`TaskIntakeService.cs:867-869` nói ngược — sai). Hành vi thật (đọc code, giả định lượt quét ≤1 s):

| T0 (24/7, cửa sổ 900 s) | Guard hiện tại | Scheduler hiện tại | PA1 | PA2 | PA3 |
| --- | --- | --- | --- | --- | --- |
| 07:45:00 | từ chối | — | từ chối | từ chối | từ chối |
| 07:50 / 07:52:29 | từ chối **oan** | (nếu nhận) A1 08:00 + A2 ≈ 08:00:35 | từ chối → M3 gửi lại 08:00 | nhận, 1 cuộc rồi `WINDOW_EXPIRED` | từ chối |
| 07:52:30 | nhận | A1 08:00:00 + A2 ≈ 08:00:35 — **gọi liền** | từ chối → gửi lại 08:00 | 1 cuộc | 1 cuộc 08:00 |
| 07:55 | nhận | cách 150 s | từ chối → gửi lại 08:00 | 2 cuộc, cách 450 s | 1 cuộc 08:02:30 |
| 07:59:59 | nhận | cách 449 s | từ chối → gửi lại 08:00 | 2 cuộc, cách 450 s | 1 cuộc (mất 1 cuộc tốt) |
| 08:00 | chuẩn | chuẩn | chuẩn | chuẩn | chuẩn |

Giờ Vàng cùng lỗi, dải hẹp hơn: từ chối oan (07:55:00, 07:57:30); 2 cuộc gần nhau từ 07:57:30.

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Guard: T0 phải nằm trong giờ gọi (`!callingWindow.Enabled` hoặc `Evaluate(T0).Open`) | Một vị từ; không đụng hot path scheduler; không đổi contract (giữ decision + reason); luật hai bên trùng khít luật chief 25/09 (M3 giữ đơn tới 08:00) ⇒ không mất đơn; xoá hẳn đường gọi liền | Đơn T0 07:45–07:59:59 bị từ chối, M3 gửi lại lúc 08:00 | 2,5–3 giờ · độ khó 2/5 |
| PA2 | Guard "có thời điểm mở trước hạn" + scheduler gọi bù có giãn cách (450 s / 150 s) | Không từ chối oan | Ba điểm HIGH (`TryClaimDueDispatchAsync`, `CalculateCapacity`, backlog); thêm dải "1 cuộc rồi `WINDOW_EXPIRED`"; đỉnh tải 08:00 gấp đôi | 7–10 giờ · 4/5 |
| PA3 | Scheduler bỏ attempt có lịch ngoài giờ | Bảng so-sánh `:91` thành đúng | Migration mở CHECK trạng thái attempt; đổi ngữ nghĩa kết quả (`NO_ANSWER_FINAL` sau 1 cuộc ⇒ M3 hủy); cần M3 ký | 10–13 giờ + M3 · 5/5 |

**Khuyến nghị: PA1**, giữ reason `CALLING_WINDOW_CLOSED_FOR_WHOLE_CONFIRMATION_WINDOW` và ghi rõ điều kiện trong
tài liệu. Nếu `Q-22` chọn PA2 thì vị từ thành "**mọi** attempt nằm trong giờ gọi" — cùng một chỗ sửa. (Một agent đề
xuất dời lịch attempt vào giờ gọi ngay tại intake — bị loại vì cửa sổ cố định từ T0 khiến đơn sáng chỉ được 1 cuộc
rồi `WINDOW_EXPIRED`.)

| Việc sau khi quyết | Giờ | Trạng thái |
| --- | ---: | --- |
| `Q-13.1` Sửa `AnyAttemptFallsInsideCallingHours` (`TaskIntakeService.cs:878-896`) + chú thích sai `:862-876`, `:180-184`; test `UT-INTAKE-MORNING-01/02`, `UT-INTAKE-EVENING-01`, `UT-INTAKE-WINDOW-SWEEP-01` (quét từng giây trong ngày cho cả hai chương trình); tài liệu IR-06 `:392` + §3.4.2 (thêm bảng buổi sáng), `06-error-codes:78`, IR-07 dòng `M3-02` + một dòng đổi hành vi, so-sánh `:88-94`. `gitnexus_impact` = HIGH (32 symbol) — chạy đủ test intake. Gộp `L2` | 3 | ✅ 3d04eee · 1276/1276 (W-0358; sweep 44/44) |

#### `Q-14` — Pipeline main không thể xanh 🆕 (dòng 714, W-0121) · Toàn (release owner) + chief; PA1 cần Sếp

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Cấp cluster dev (phiếu `platform-ci-staging`) | Xanh thật | Tốn tiền, vài ngày; phụ thuộc Sếp + Platform | — |
| PA2 | Cổng `deploy_*` bằng biến (`$IVR_DEPLOY_TARGET_READY == "YES"`); thêm job luôn chạy, `allow_failure: true`, in `BLOCKED_EXTERNAL`; tách 2 job manual sang pipeline phát hành (hoặc `allow_failure: true` + sửa `IT-CD-GATE-02`); định nghĩa lại tiêu chí W-0121 = "main xanh, deploy bỏ qua có khai báo" | Màu đỏ có nghĩa trở lại ngay | Deploy bị bỏ qua lâu mà không ai để ý — job đánh dấu giảm rủi ro này | 2 giờ |
| PA3 | Giữ nguyên, lấy pipeline MR làm tín hiệu xanh | 0 công | Main đỏ mãi — đúng cơ chế làm lỗi `nullable` bị bỏ sót 9 ngày | 0 |

**Khuyến nghị: PA2 ngay, PA1 khi phiếu Platform được duyệt.**

| Việc sau khi quyết | Giờ | Trạng thái |
| --- | ---: | --- |
| `Q-14.1` Sửa `cd.gitlab-ci.yml`, `observability.gitlab-ci.yml`, `promote.gitlab-ci.yml` theo PA2; `cd-selftest` | 2 | ⬜ |
| `Q-14.2` Luồng promote hỏng 3 chỗ: `.promote_base` thiếu `entrypoint: [""]` + kubectl (lỗi W-0292 mới sửa cho `.deploy_base`); workflow loại pipeline tag trong khi `promote_prod`/`rollback_prod` cần tag và `needs: promote_lab`; `promote_prod` ghi `ci-artifacts/cd/` không `mkdir` (after_script tự rollback production). Luồng tag cần Tech Lead/Platform chọn | 1,5 | ⬜ |
| `Q-14.3` Tìm gate `gate_sweep` hosted đỏ 22–24/09 (cần log GitLab) | 0,5 | ⬜ |

#### `Q-15` — Ba biến "an toàn" không code nào đọc 🆕 · Toàn (báo chief: audit dòng 106, 123 dùng làm bằng chứng)

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Xoá `IVR_ADAPTER_MODE`, `IVR_KILL_SWITCH_ENABLED`, `IVR_LAB_DESTINATION_ALLOWLIST` khỏi helm (`_helpers.tpl:31-38, :190-202`), Dockerfile, compose; chuyển `k8s-selftest.mjs:201`, `image-selftest.mjs:81-82`, `docs-selftest.mjs:321-322` và tài liệu (`deploy/docker/README.md:82`, `docs/review-checklist.md:33`, `specs/api/04-sim-adapter-contract.md:59`) sang khoá thật (`IVR_EXECUTION_MODE`, `SIM_PROVIDER`, `Ivr:Telephony:*:Enabled`, cờ DB `globalDialKillSwitch`) | Một nguồn sự thật; gate kiểm đúng thứ có tác dụng | Sửa nhiều file | 2–3 giờ |
| PA2 | Cho biến có tác dụng (từ chối khởi động khi kill switch ≠ true ở non-MOCK) | Giữ tên quen | Hai nguồn sự thật cho kill switch (env + DB) | 3 giờ |
| PA3 | Giữ, ghi rõ "không có tác dụng" | Rẻ | Gate vẫn báo xanh giả | 0,5 giờ |

**Khuyến nghị: PA1.**

#### `Q-16` — Đóng hẳn lớp lỗi "intake nhận – lúc quay từ chối" (B16) · Toàn đề xuất, chief duyệt

Lớp gồm: tổng tiền >999.999.999.999, `quantity` nhiều chữ số lẻ, tên hàng dính guard, bản thoại >1.200 ký tự.

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Giới hạn số trong OAS (`maximum` cho `total_amount`; `maximum` + `multipleOf: 0.001` cho `quantity`) | M3 thấy ngay trong schema | Không phủ tên hàng và độ dài; `maximum` nhiều khả năng làm job `api_contract_diff` (`--fail-on WARN`) đỏ | 2 giờ |
| PA2 | Render thử ngay tại intake, sau khi đã có `approvedScript` (`TaskIntakeService.cs:224-247`); lỗi ⇒ từ chối với reason mới (vd. `SPEECH_SUMMARY_NOT_RENDERABLE`), dùng decision/mã có sẵn, không thêm enum `ErrorCode`; giới hạn số chỉ ghi trong description | Phủ cả lớp; M3 thấy lý do ngay khi gửi; không phụ thuộc điểm mù của oasdiff | Đính chính IR-07 + error codes; phải làm **sau** `Q-12` (nếu không, "Tổ yến" bị chặn ngay tại cửa) | 3 giờ |
| PA3 | Giữ như W-0354, chỉ vá các chỗ còn cách ly SIM (`K-26`, `K-45`) | Không đổi contract | Đơn lỗi dữ liệu vẫn không được gọi; M3 chỉ thấy `WINDOW_EXPIRED` không lý do | 0 (đã tính ở K) |

**Khuyến nghị: PA2, sau `Q-12`.**

#### `Q-17` — Hạ S2 / `legal_gate` / `internal_mirror_gate` đang PASS bằng chữ ký tự kiêm (A1 bước 2; A3, A4, B8 bước 2) · Toàn (chief đã chỉ hướng; nhánh ủy quyền do Sếp)

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Hạ về "Toàn đã rà — chờ Sếp ký" (`legal_gate` → `PENDING_SEP_SIGNATURE`); mirror → `PENDING_CHIEF_ALLOCATION`; gate in `release_blockers=LEGAL,INTERNAL_MIRROR`, vẫn exit 0 | Đúng lệnh chief; không ảnh hưởng vận hành (TTS production đang tắt); khớp tracker `G-LEGAL` `BLOCKED_EXTERNAL` | Một lượt ghim lại chuỗi; ghim lại lần nữa khi Sếp ký | 3 giờ |
| PA2 | Giữ PASS, xin Sếp văn bản ủy quyền cho Toàn kiêm Legal/Privacy | Không phải ghim bây giờ | Trong lúc chờ, repo trình PASS dựa trên tự xác nhận (trái lệnh chief); mirror PASS trỏ server test vẫn sai | 0,5 giờ + ghim sau |
| PA3 | Giữ PASS cho lab + thêm chặn production qua trường `production_activation_allowed` | Không đổi lock | Vẫn dán nhãn `LEGAL_PRIVACY PASS` cho chữ ký tự kiêm; phải sửa 56 ca kiểm | 2 giờ |

**Khuyến nghị: PA1**, làm cùng `Q-08` (tờ trình có ô "ủy quyền" để mở lại PA2 đúng cách về sau).

| Việc sau khi quyết | Giờ | Trạng thái |
| --- | ---: | --- |
| `Q-17.1` **Một commit: 8 file sửa + 1 file mới** (theo checklist `d87c96a`). `MODELS.lock` (`:5`, `:10-21`, `:22-29`). Ghim lại tầng 1: `deploy/tts/shim/voices.json:9`, `docs/evidence/W-0122/voice-acceptance-manifest.template.json:20`, `b3-telephony-evidence-validator.mjs:22-24`, `docs/evidence/W-0185/b3-telephony-evidence.template.json:10-12` (sinh bằng `--print-template`), `values-prod-tts.draft.yaml:47` (+`:19`, `:28`, `:56`). Tầng 2: hash `voices.json` ở gate `:17`, b3 `:24`, W-0185 `:12`; hash template W-0122 ở gate `:18`. **Tạo** manifest nghe giọng dẫn xuất mới thay vì sửa `W-0343/voice-acceptance-manifest.json` (bị runtime + b3 ghim); không sửa `W-0343/verification.json`. Tài liệu: `S2-owner-review.md:3-6`; `vuong-mac :62-68, :85-91, :166`; `00-CHUA-XONG :281-282, :309`; `deploy/tts/README.md:17-19, :35`; register OD-V1-19, OD-VOICE-04; residual W-0340/W-0343 trong tracker + mục `BLOCKED_EXTERNAL`; `gate-status --write`. Ghi rõ ảnh `676133c7` trên vps61 còn lock PASS cũ, không dùng làm bằng chứng. Kiểm: `tts-provenance-gate --selftest`, `tts-voice-acceptance-gate --selftest` + `--acceptance <manifest mới>`, b3 `--self-test` + `--check-template`, gate sweep | 3 | ⬜ |

#### `Q-18` — Siết gate `tts-provenance`: chặn người build tự ký (A1 bước 6) · Toàn đề xuất, chief duyệt — **làm cùng hoặc sau `Q-17`**

Làm trước `Q-17` thì gate ném lỗi trên lock đang PASS (`:115-117`) ⇒ CI đỏ. Nguồn danh tính "người build" hiện có:
`CODEOWNERS` chỉ có tên nhóm giữ chỗ; `git log` 487/487 commit cùng một email, tên không dấu hoặc `nqt20102001`,
trong khi `decided_by` ghi có dấu.

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Danh sách tĩnh `deploy/ci/governance/m8-builders.json` (tên có/không dấu, bí danh "owner", "IVR Owner", `nqt20102001`, email, agent); chuẩn hoá tên (bỏ dấu, đ→d, chữ thường, gộp khoảng trắng); `approval_reference` phải là tham chiếu ngoài repo + `approval_document_sha256` 64 ký tự hex; người build chỉ được ký khi có `delegation_reference` ngoài repo kèm hash | Offline, tất định, chạy được trong sweep và CI clone nông | Danh sách do chính người build giữ — chief phải duyệt file | 3–3,5 giờ |
| PA2 | Suy người build từ `git log` (tác giả, người commit, `Co-Authored-By`) | Không phải giữ danh sách | Cần `.git` (trong image không có ⇒ bản Python lệch mãi); tên không dấu; không bắt được bí danh | 3–4 giờ |
| PA3 | Chief/Sếp ký file phê duyệt bằng khoá SSH/GPG (`ssh-keygen -Y sign`, `allowed_signers` trong repo) | Cách duy nhất chứng minh người ngoài dev đã duyệt | Chief/Sếp cần khoá + quy trình ký; image CI thêm công cụ; làm lại bản Python | 5–6 giờ + thời gian chief |

**Khuyến nghị: PA1**, thêm lớp phụ: khi có `.git`, self-test kiểm tác giả HEAD (đã chuẩn hoá) nằm trong danh sách.
Để dành PA3 cho trước lần bật production (N04). Self-test thêm: bản ghi dạng W-0343 phải bị từ chối; tên không dấu
và viết hoa; bí danh "Owner module IVR"; tham chiếu trong repo; thiếu/sai hash; ~10 ca `legal/*` trong
`release-approval-cases.json` (ca âm đánh dấu `python_drift`).

#### `Q-19` — C21: giới hạn tuổi khi phát lại callback đã chết · Toàn (báo chief; M3-10 chỉnh con số sau)

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Chỉ tài liệu (W-0354 đã làm) | 0 | Một người tầng Danger phát lại `IVR_CONFIRMED` sau nhiều ngày; an toàn dồn hết lên revalidate của M3 (M3-11 chưa ký, D-06 "chưa chứng minh runtime") | 0 |
| PA2 | Chặn cứng: `now − callback.CreatedAt > ReplayMaxAge` (mặc định 7 ngày = đề xuất M3-10, cấu hình 1–30) ⇒ `409 IVR_VERSION_CONFLICT`, không ghi gì. Options `CallbackReplayOptions` bind ở API (không dùng `CallbackDeliveryOptions` — API không bind) | Rẻ, fail-closed như W-0006; không đổi enum | Thêm một cấu hình | 3–4 giờ |
| PA3 | Chặn mềm: quá N ngày phải gửi cờ `acknowledge_key_retention_expired`, audit đánh dấu | Linh hoạt | Dễ thành thói quen bấm cho qua; thêm schema request | 5–6 giờ |

**Khuyến nghị: PA2.**

| Việc sau khi quyết | Giờ | Trạng thái |
| --- | ---: | --- |
| `Q-19.1` Code + `IT-API-DEADLETTER-15/16`, `UT-CB-REPLAY-AGE-01`; cho phép phát lại `AUTH_REJECTED` (hiện chỉ nhận `RETRY_EXHAUSTED`/`INVALID_DEAD_LETTER` — khi bật C11, cấu hình credential sai là mọi kết quả chết, chỉ gỡ được bằng UPDATE tay) + `IT-API-DEADLETTER-17`; bump contract `draft.34`: mô tả 409 quá tuổi + sửa chữ OAS `:972` (`K-15`); IR-06 §4A.3 + IR-07 `:721` bỏ câu "chưa đặt giới hạn"; chuỗi bump đầy đủ (luật 5) | 4,5 | ⬜ |

### Nhóm 2 — chief / Tech Lead quyết (dev chuẩn bị phương án)

#### `Q-20` — C13: thu hồi task có phát callback không · chief — quyết được ngay, không phụ thuộc M3-14

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Không callback; ACK đồng bộ của endpoint là xác nhận duy nhất | Khớp RVK-10; M3 là bên ra lệnh nên đã biết; không đổi contract callback, không đổi CHECK | — | 0 thêm |
| PA2 | Loại kết quả mới `IVR_TASK_REVOKED` | Tường minh | Đổi enum OAS callback (M3 sở hữu), mở CHECK DB, `ResultContractPolicy` 6 → 7; M3 sửa consumer | +8–12 giờ + chữ ký M3 |
| PA3 | Dùng lại `IVR_OPERATIONAL_BLOCKED` | Không thêm enum | Phá lời hứa OAS callback (V1 không phát giá trị này); vẫn mở CHECK; lẫn nghĩa với "chặn trước khi gọi" | +4 giờ |

**Khuyến nghị: PA1.**

#### `Q-21` — C10: tên trường callback trên dây · chief

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Giữ shape M8 (`order-core-ivr-callback.target-v1.yaml` là chuẩn); chief sửa §14 trỏ về OAS | Đã đóng băng qua gate; 11 ca E2E + smoke chief 25/09 đạt | — | 0 |
| PA2 | Đổi sang tên §14 ngay, trước khi M3 viết consumer | Khớp chữ spec | Sửa mapper, OAS, NSwag, receiver giả, validator; chạy lại toàn bộ bằng chứng | 12–16 giờ + M3 duyệt |
| PA3 | Chờ OAS chính thức của M3 (M3-08) rồi ánh xạ | Không làm trước | D-5 bị chặn vô thời hạn | 0 bây giờ |

**Khuyến nghị: PA1.**

#### `Q-22` — Vùng tối: đơn 24/7 T0 21:00:30–21:07:59 chỉ được 1 cuộc rồi `WINDOW_EXPIRED` 🆕 · chief

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Giữ biên 21:08, chỉ ghi rõ vùng này được 1 cuộc | Không đổi điều đã chốt | Đơn trong vùng có thể bị hủy sau đúng 1 cuộc | 0,1 giờ |
| PA2 | **Mọi** attempt phải nằm trong giờ gọi: IVR từ chối T0 ≥ 21:00:30 (24/7) / ≥ 21:05:30 (Giờ Vàng); M3 giữ tới 08:00 như đơn đêm | Mọi đơn COD đủ 2 cuộc; `NO_ANSWER_FINAL` luôn nghĩa là đã gọi 2 cuộc; cùng chỗ sửa với `Q-13` | Đơn 21:00:30–21:08 chờ tới sáng; biên M3 đổi từ 21:08 sang 21:00:30 cho 24/7 | 0,5 giờ + việc nhỏ phía M3 |
| PA3 | Tách bảng map theo tình huống | — | M3 cần dữ kiện không có trên dây | ~1 giờ |

**Khuyến nghị: PA2.**

| Việc sau khi quyết | Giờ | Trạng thái |
| --- | ---: | --- |
| `Q-22.1` Vị từ + test + tài liệu (gộp với `Q-13.1` nếu kịp) | 0,5 | ⬜ |
| `Q-22.2` Task nhận ở giây cuối trước 21:08 không kịp claim bị tính `capacityMiss` (incident OPEN + metric sizing SIM phồng, `PostgresSchedulerStore.cs:446-474`) → phân loại lại chỉ ở incident/metric, **không** đổi result gửi M3 | 1,25 | ⬜ |

#### `Q-23` — Token tĩnh Order Core luôn được nhận 🆕 · Tech Lead (vai M3) qua chief; Platform (OD-V1-07)

`OrderCoreAllowlistMiddleware.cs:13, :71` hỏi `LegacyCredentialAccepted(CallbackDeliveryOptions.Provider)`; API không
bind section này (chỉ `Ivr.Worker/Program.cs:68` bind) ⇒ luôn là mặc định `FAKE_TARGET_V1` ⇒ token tĩnh luôn được
nhận, kể cả `PRODUCTION_REAL` (vốn bắt buộc `TARGET_V1`). Trái W-0032 (đã ACCEPTED), OD-V1-07, IR-06 `:1386`,
`secret-inventory:26`. Cờ `CurrentCompatTokenEnabled` (`ServiceIdentity.cs:73`) không ai đọc. JWT thật đang
`BLOCKED_EXTERNAL`, tức hôm nay token tĩnh là đường xác thực **duy nhất** ở production.

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Sửa ngay, fail-closed: middleware đọc `IvrOptions.SalesProvider` (API đã bind); intake production đóng tới khi có issuer thật (E-1) | Code khớp quyết định đã ký; production chưa chạy nên hôm nay không mất gì | Đường tới đơn thật đầu tiên thêm phụ thuộc issuer (vốn đã có trong C11) | 1–2 giờ |
| PA2 | Sửa + nối cờ có sẵn `CurrentCompatTokenEnabled` (mặc định tắt; bật phải có approval + audit) cho pilot | Có đường pilot có kiểm soát | Thêm một lối vòng chính thức cho secret tĩnh | 3 giờ |
| PA3 | Giữ, ghi nhận "production tạm dùng token tĩnh" vào W-0032, secret-inventory, IR-06 | 0,5 giờ | Secret tĩnh dùng chung, không danh tính, cho endpoint tạo lệnh gọi khách có số điện thoại | 0,5 giờ |

**Khuyến nghị: PA1.** Trước khi sửa: kiểm mọi compose/profile đang chạy `SALES_PROVIDER=TARGET_V1` (lab S5, W-0345)
để khỏi gãy lab. Test `IT-AUTH-COMPAT-12 StaticOrderCoreTokenIsRefusedUnderTargetV1` (đỏ ở code hiện tại); xoá dòng
cấu hình không tác dụng `docker-compose.dev.yml:142`.

#### `Q-24` — C18: tài nguyên server test 192.168.1.61 ngoài cấp phát · chief

Công cụ đang tạo tài nguyên ngoài `/home/ssv/m8`: `deploy/lab/full-flow-s5/launcher.py` (cổng 58443, project
`ivr-w0344-*`, giữ volume sau lượt chạy), `run-vieneu-s5.py:161`, `run-vieneu-worker-s5.py:133`; 13 URI
`sftp://ssv@192.168.1.61/home/ssv/ivr-artifact-mirror/…` trong `MODELS.lock`; 31 file evidence W-0333…W-0345.

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Chief cấp chỗ riêng ngoài `/m8` và ghi vào bảng cấp phát | Ít việc nhất | Hai quy ước tên trên máy chung; session khác vẫn có thể dọn nhầm | 0,5–1 giờ |
| PA2 | Dời hết về `/home/ssv/m8` + cổng 6800–6899 + tiền tố `m8_`: sửa 3 công cụ + 3 test, chuyển mirror, sửa 13 URI, ghim lại artifact set + chuỗi lock, dọn tài nguyên cũ | Tuân thủ đủ | Mirror vẫn trên server test ⇒ `internal_mirror_gate` vẫn không PASS được; ghim lại lần hai | 5–6 giờ |
| PA3 | Công cụ cho các lượt sau đổi theo PA2 (tên `m8_`, cổng 68xx, thư mục `/m8`); tài nguyên cũ giữ chỉ-đọc như ngoại lệ tạm rồi dọn theo nhãn; mirror production **không** dời trong server test mà chờ kho S5 thật | Giảm va chạm ngay; không ghim lại hai lần; trung thực về kho production | — | ~2 giờ bây giờ |

**Khuyến nghị: PA3.** Trong lúc chờ: không chạy thêm lượt S5 bằng công cụ hiện tại.

#### `Q-25` — N1: nơi lưu ngân hàng clip · Toàn + chief (kho, C18) + Platform (S5)

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Đóng vào ảnh media Asterisk có digest; worker/api chỉ mang manifest (ConfigMap, ghim sha) | Bất biến, phiên bản theo digest, không cần RWX, giống hệt lab | Đổi bank là build lại ảnh | trong N1 |
| PA2 | Volume RWX dùng chung Asterisk + worker | Đổi bank không phải build | Cần RWX (OD-VOICE-08 từng vướng) | trong N1 |
| PA3 | Object store/mirror; Asterisk kéo về và kiểm sha lúc khởi động | Linh hoạt | Thêm phụ thuộc lúc khởi động; mirror chưa có chỗ (C18) | trong N1 |

**Khuyến nghị: PA1.**

#### `Q-26` — N1: khi một đơn không đọc trọn được (thiếu clip) · Tech Lead

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Fail-closed lúc quay: `IVR_TECHNICAL_EXCEPTION` không tính lượt; hết cửa sổ thì chuyển admin review | Không đổi contract | Đơn đó không được gọi | trong N1 |
| PA2 | Chặn ở intake với reason mới `SPEECH_CLIP_UNAVAILABLE` | M3 biết ngay | Thêm giá trị contract; API phải có manifest | trong N1 + contract |
| PA3 | Đọc bớt ("và N sản phẩm khác", quy tắc 3b) | Cuộc gọi vẫn chạy | Nội dung ít đi; câu chữ phải duyệt; chỉ hợp khi đọc tên hàng | trong N1 |

**Khuyến nghị: PA1** làm nền. Nếu `Q-29` chọn (b), mọi đầu vào đã bị chặn bằng schema (`multipleOf: 1`, pattern mã
đơn) cộng kiểm đủ bank lúc khởi động — PA1 chỉ còn là lưới cuối.

#### `Q-27` — Q.850 cause 20 (và 3): khách tạm thời không liên lạc được 🆕 · chief trình Sếp; M3 đối ký bảng map

`AsteriskAriSimGateway.cs:411` map `1 or 3 or 20 ⇒ Unreachable` ⇒ `DispositionMapper.cs:77` ⇒
`IVR_INVALID_PHONE_FINAL` (kết thúc, không tính lượt) ⇒ theo bảng map M3 hủy với `IVR_INVALID_NUMBER`. Cause 20 là
"thuê bao vắng mặt" — trạng thái tạm thời. `MapHangup` chưa có test nào.

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Giữ nguyên | 0 | Khách tắt máy lúc cuộc 1 mất cuộc 2; bị hủy như số sai | 0 |
| PA2 | Tách: cause 1, 28 ⇒ số sai; cause 20 (và 3 nếu đo thấy tương tự) ⇒ không nghe máy, tính lượt, còn lượt thì gọi lại | Đúng nghĩa nghiệp vụ; khách có cuộc 2 | Tăng bộ đếm "không nghe máy" mà ngưỡng còn chờ Sếp (N06) | 3–4 giờ + đo |
| PA3 | Cause 20 ⇒ lỗi kỹ thuật, không tính lượt | Không tiêu lượt khách | Lẫn lỗi của khách vào lỗi hệ thống; retry dồn tới `WINDOW_EXPIRED` | 3–4 giờ |

**Khuyến nghị: PA2**, chốt sau khi đo MobiFone thật trả mã gì cho máy tắt/ngoài vùng (làm cùng DT-02 trong B12).
Thêm test dạng bảng cho `MapHangup` (sửa R-00 §4 — file bị ghim, phải ghim lại).

#### `Q-28` — DispatchGate ở nhánh production 🆕 (C16 — audit ghi "không còn việc lõi trong tay dev" là sai) · Tech Lead thiết kế; Sếp ký bước pilot → mở

Vault tạo địa chỉ `sip:{số}@{host}` (`ProductionDialTokenVault.cs:110-116`); gateway đưa nó vào gate (`:75-79`);
`DispatchGate.cs:20` chạy `PiiGuard.EnsureSafeText` ⇒ ném lỗi; nếu lọt, `:55-58` còn đòi allowlist lab — allowlist
này không thể chứa số (guardrails bắt mọi mục qua PiiGuard). Nhánh production (`:65-73`) chưa có test nào.

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Nhánh production bỏ allowlist lab; gate nhận tham chiếu không chứa số; chỉ kiểm kill switch + approval `PRODUCTION_CALL` theo môi trường + `RealCustomerCallAllowed` | Nhanh; không số nào vào cấu hình/audit | Phía IVR không có rào nào giữ lần gọi đầu chỉ tới số nội bộ | 3–4 giờ |
| PA2 | Như PA1 + danh sách pilot lưu **vân tay HMAC** của số (chuẩn hoá bằng `VietnameseDestinationNumber`, khoá là bí mật triển khai, hai người duyệt danh sách); một approval có chữ ký chuyển pilot → mở | Khớp kế hoạch SIP của chính dev (`:191`); W-0008 chỉ tới được số nội bộ, không phụ thuộc M3 | Thêm một khoá bí mật + luồng admin | 6–8 giờ |
| PA3 | Giữ production đóng; đo W-0008 bằng hồ sơ `LAB_REAL_SIM` qua trunk (alias Asterisk tới số nội bộ) | Không đụng gate bây giờ | Không chạy code đường production; sau vẫn phải làm PA1/PA2 | ~1 giờ + cấu hình Asterisk |

**Khuyến nghị: PA2**; nếu khoá bí mật phải chờ Platform thì dùng PA3 cho W-0008 trong lúc chờ.

| Việc sau khi quyết | Giờ | Trạng thái |
| --- | ---: | --- |
| `Q-28.1` Sửa gate + `IT-FLAG-PRODGATE-15/16`, `UT-TRUNK-GATE-01/02`; sửa `InternalAdminApiService.cs:994-1000` (retry kỹ thuật tay cũng đòi allowlist lab ở mọi chế độ) | 6 | ⬜ |
| `Q-28.2` `IT-PHONE-CONTAIN-03`: số chỉ dùng ở mép nhà mạng | 1 | ⬜ |
| `Q-28.3` Dựng lời thoại **trước** khi resolve số (hiện từ chối tất định xảy ra sau resolve, tiêu lượt resolve, ghi nhầm là lỗi mạng; thông báo còn ghi "lab" trên đường production) — làm cùng SIP-04 | 1 | ⬜ |

#### `Q-29` — N1: phạm vi đoạn động ở production quá độ · Tech Lead; chief đối chiếu PACK-09; Sếp xác nhận nếu chọn (a) hoặc (c)

| | Phương án | Ưu | Nhược | Số clip / công thêm |
| --- | --- | --- | --- | --- |
| PA1 | (a) Đầy đủ: món + số lượng + tổng tiền + tỉnh (giữ v3) | Giữ v3 + 12 đoạn đã render; khách nghe tên món | Lệch PACK-09 §18; cần tên SKU (PACK-02 còn placeholder; repo ghi 20 SKU, không phải 19), cần DR-12, quy trình thêm SKU mới; vùng chỉ có quận thì không đọc được | ~433–436 clip · +6 giờ |
| PA2 | (b) Tối thiểu theo PACK-09 §17.2: mã đơn + tổng tiền | Đúng nguồn của Sếp; không cần danh mục SKU; không cần DR-12; mọi đầu vào khoá được bằng schema; cuộc gọi ngắn (nền 35 s) | Đổi `RequiredPlaceholders` (`TargetV1SpeechPolicy.cs:57-62`), template mới qua duyệt 3 người (S1); M3 chốt định dạng mã đơn (OAS `:1268` đang tự do tới 40 ký tự); bỏ 12 đoạn cũ; khách không nghe tên món | ~327–405 clip · +4 giờ |
| PA3 | (c) Không đọc đoạn động nào | Nhanh nhất | Trái PACK-09 §18.2 (thiếu mã đơn hoặc tổng thì không gọi); xác nhận mù, yếu trước đơn ảo | 3–6 clip · +2 giờ |

**Khuyến nghị: PA2 (b).** Phần lõi (L4–L5 dưới) dùng chung, nên sau này vẫn thêm (a) thành một phiên bản template
mới được khi đủ điều kiện.

| Việc sau khi quyết (≈ 27–36 giờ) | Giờ | Trạng thái |
| --- | ---: | --- |
| `Q-29.1` Lấy lại từ `9af20d3^` phần có ích: `SpeechNumberClip`, `SpellClips`, `SpellQuantityClips`, `TotalAmountClips` + "đồng" (nếu (a): thêm `TryResolveProvinceName`, `RecordedSpeechComposer` → đổi tên `ClipComposer`, sửa chú thích đang giả định thu giọng người). **Không** lấy `StaticFileTtsProvider` (phát một file cho cả cuộc). So hash văn bản như `9af20d3` đã làm; khôi phục cả `UT-VOICE-AREA-01..04` mà audit không liệt kê | 3 | ⬜ |
| `Q-29.2` Viết mới đường prerendered: catalog + loader/validator manifest; renderer chế độ clip (`ExactText` bằng đúng thứ khách nghe); `SpeechSegment.ClipIds`; nhánh prerendered không queue/cache/provider; mã lỗi fail-closed; chặn placeholder không có bank (`customer_display_name`); dựng lời trước khi resolve số | 12–16 | ⬜ |
| `Q-29.3` Công cụ bank: `clip-plan` sinh từ Domain; `render-speech-clips.mjs` (mở rộng `render-fixed-speech.mjs`); manifest + `SHA256SUMS`; gate bank (đủ clip, đúng sha, người duyệt khác người build); tên file mờ (log Asterisk `-vvv` không lộ số tiền) | 6–8 | ⬜ |
| `Q-29.4` Render thật, bộ nghe (người nghe do Sếp/Tech Lead chỉ định — không phải Toàn), 6 cuộc MicroSIP Bắc/Trung/Nam; chuyển compose mock/sandbox/lab sang prerendered (chứng minh `ivr_tts_provider_requests_total = 0`); đo trên máy render (B8 bước 4) | 4–6 + 2–3 nghe | ⬜ |
| `Q-29.5` Tài liệu: OD-V1-19/S4 (OD-V1-15 nếu (b)), `00-CHUA-XONG`, IR-06, `capacity-model` (thời gian chuẩn bị trước quay ≈ 0), `specs/ui/04`; helm khối `worker.speech.prerendered` | 2–3 | ⬜ |

**Không mở SIP-04 trước khi xong `Q-29.2` và `Q-29.4`.**

### Nhóm 3 — Sếp quyết (qua chief; đưa vào tờ trình `Q-08`)

#### `Q-30` — S1: ba người khác nhau duyệt kịch bản production (V6-2) · Sếp

Code ép luật 3 người (`ScriptContentContracts.cs:237-255, :298-303`); thiếu S1 thì mọi kịch bản production bị từ
chối cả ở intake lẫn lúc quay.

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Sếp chỉ định hai người mới (Content + Privacy/Legal) | Đúng luật 3 người | Hai tài khoản; mỗi lần sửa kịch bản cần hai người rảnh | ~1 giờ thiết lập |
| PA2 | Người thứ hai Sếp đã đồng ý 17/09 giữ vai Content, Sếp giữ Privacy/Legal, Toàn soạn | Chỉ cần một người mới; vai pháp lý đúng người chịu trách nhiệm (khớp A1) | Sếp thao tác console mỗi lần duyệt | ~1 giờ |
| PA3 | Nới code xuống hai người | Chạy ngay | Trái IR-07 A-14; hạ rào đúng chỗ lời thoại đọc dữ liệu cá nhân | 2–3 giờ |

**Khuyến nghị: PA2.**

#### `Q-31` — B1 bước 3: mô hình phiên Giờ Vàng cho capacity · Sếp + M3

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Giữ cửa sổ 300 s tới khi có dữ liệu phiên thật; bắt đầu 8 kênh (S6); đo bằng metric backlog/capacity incident (W-0353) | Không thiếu kênh | Tính dư kênh | 0 |
| PA2 | Nạp 2700 s, giả định đơn đến đều | Rẻ nhất (2 kênh) | Thiếu kênh ~8 lần nếu đơn dồn đỉnh — chính model đã cảnh báo (`capacity-model.mjs:98-107`) | 2 giờ |
| PA3 | Nạp 2700 s với phân bố dồn đỉnh M3 lấy từ phiên thật | Có căn cứ | Dữ liệu chưa tồn tại | 2–3 giờ khi có |

**Khuyến nghị: PA1**, chuyển PA3 khi có dữ liệu.

#### `Q-32` — Phương án B: M3 gửi `phone_e164`, IVR lưu dạng đọc được, giữ vĩnh viễn (A2, B11, C16, C19) · **Sếp** (phiếu 25/09 mục B2)

| | Phương án | Ưu | Nhược | Chi phí dev |
| --- | --- | --- | --- | --- |
| PA1 | (i) Giữ B + giữ vĩnh viễn | Rẻ nhất (đồng bộ ~12 chỗ tài liệu); không thêm phụ thuộc | Danh bạ khách dạng đọc được tích luỹ không giới hạn: mọi dump, replica, DR standby, bản sao test đều thấy số; DSAR là đường xoá duy nhất; có thể trái nguyên tắc giới hạn thời gian lưu (NĐ 13/2023) | 3 giờ |
| PA2 | (ii) Giữ B + mã hoá cột ở tầng ứng dụng (AES-256-GCM, AAD = `task_id`, khoá trong K8s Secret; thiếu khoá ở production ⇒ intake từ chối số) + lưu `NULL` ngoài production (`K-40`) + giải mã muộn nhất tại `ProductionDialTokenVault` + migration additive thay function trigger để cho `phone_e164 → NULL` + worker xoá số sau cửa sổ + N ngày. Biến thể rẻ: chỉ làm phần xoá-sau-N trước | Gỡ cả hai rủi ro nặng; không đổi contract (M3 không phải làm gì); không cần Platform; đời hợp lệ của số ≤ 15 phút nên xoá sớm gần như miễn phí | Khoá cùng miền quản trị với DB: chống được dump/backup/replica/người chỉ đọc DB, không chống admin cluster | 16–20 giờ (biến thể 6–8 giờ) |
| PA3 | (iii) Quay về token | Đúng mô hình spec §17 gốc | Cần Platform (bộ mã hoá production) + M3 (bộ cấp token) — chưa ai nhận; tới lúc đó production nhận 0 task; đảo lời đã gửi M3 (IR-07 A-5) | 12–16 giờ + M3 |

**Khuyến nghị kỹ thuật (để trình; Sếp quyết): PA2.** Nếu chưa có người giữ khoá thì làm biến thể xoá-sau-N trước.

### Nhóm 4 — M3 / anh Mạnh quyết

#### `Q-33` — C1: `program_code` · M3 chọn, chief chốt

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | (a) Không registry; enum M8 là wire contract; M3 map `24_7 → TWENTY_FOUR_SEVEN` ở assembler | 0 công dev; đúng mặc định IR-07 M3-04 và FIX_M3:263 | — | 0 |
| PA2 | (b) Có registry: thêm mô tả `ProgramCode` trỏ registry, bump chỉ-mô-tả (không breaking) | Truy vết được | Một lần phát hành | 2–3 giờ |
| PA3 | (c) M3 gửi `24_7` trên dây | Theo tên M3 | Breaking, phải qua chief; map tại intake | 6–10 giờ |

**Khuyến nghị: PA1.**

#### `Q-34` — C3/C4 (+C9 phần session): phát hành `golden_hour_session_id` (sau M3-07) · chief + M3

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | Bắt buộc ngay trong một bản | Một lần phát hành | Breaking; sandbox M3 gãy nếu producer chưa kịp | 8–10 giờ |
| PA2 | Hai bước: optional trước, bắt buộc khi producer M3 đã gửi | Không làm gãy ai | Hai lần phát hành | 10–12 giờ |
| PA3 | Optional mãi, chỉ cảnh báo khi thiếu | Rẻ | Không gom được incident theo phiên — đúng điều C4 muốn tránh | 8 giờ |

**Khuyến nghị: PA2.** Không thêm CHECK "Giờ Vàng bắt buộc có giá trị" trên bảng đã có dữ liệu; cột nullable riêng ở
`ivr_capacity_incidents` (không đè `session_id`); sửa fixture Giờ Vàng (seed 6 chỗ, `sandbox-examples.mjs` 1,
`image-selftest.mjs` 6, `deploy/lab/full-flow-s5/cases.py` 2).

#### `Q-35` — C11: IVR xác thực khi gọi callback sang M3 · anh Mạnh/Sếp + Platform

| | Phương án | Ưu | Nhược | Chi phí |
| --- | --- | --- | --- | --- |
| PA1 | JWT client-credentials, chung issuer với E-1; M3 kiểm chữ ký qua JWKS | Khớp thiết kế có sẵn (`MockClientCredentialsTokenProvider`, `ServiceIdentity.cs:507`) | Cần một issuer thật | trong C11 |
| PA2 | mTLS | Mạnh nhất | Cần PKI + xoay vòng cert, tốn vận hành | trong C11 + PKI |
| PA3 | Bearer tĩnh dùng chung, có cặp xoay vòng | Rẻ nhất | Trái chủ đích W-0032 ("đường target không chạy bằng credential tĩnh") | trong C11 |

**Khuyến nghị: PA1.**

#### `Q-36` — B2: 4 file softphone ở checkout văn phòng · anh Mạnh (không phải việc dev)

| | Phương án | Ưu | Nhược |
| --- | --- | --- | --- |
| PA1 | `git stash -u` → `pull` → `stash pop`, gỡ xung đột 2 file | Nhanh | Dễ quên stash; 2 file xung đột với `9af20d3` |
| PA2 | Sao 4 file ra ngoài repo → trả 3 file tracked về HEAD và bỏ file untracked → `pull` → so lại bằng tay | Không mất gì, không đụng lịch sử, không tạo nhánh | Làm tay |
| PA3 | Bỏ 4 file (W-0315 đã thay softphone lab) | Nhanh nhất | Mất chỉnh sửa chưa ai xem |

**Khuyến nghị: PA2.** Không dùng nhánh riêng — repo chỉ làm trên `main`.

---

## §6 — PHẦN III: Chờ bên khác

| Mã | Chờ ai / chờ gì | Việc dev sau khi mở khoá | Giờ | Trạng thái |
| --- | --- | --- | ---: | --- |
| `CB-01` | Toàn gửi lại IR-07 cho anh Mạnh (kèm tờ bìa `IR07_TO_BIA_M3_2026-09-25.md` chief chuyển); chỉ gửi **sau `L2`** và ít nhất đã vô hiệu bảng v0 (`Q-03`) + đính chính C23 (`Q-02`); ghi mốc gửi ở IR-07 `:703`; giữ hậu tố `M3_NOT_RECEIVED` tới khi anh Mạnh xác nhận đã nhận | Đổi sang `PENDING_M3_SIGNOFF` khi M3 xác nhận | 0,1 | ⬜ |
| `CB-02` | Chief gửi nguyên văn `ivr-cancel-reason-map.v1` | `Q-03.2` | 0,25 | ⛔ chief — chưa xin: 25/09 Toàn quyết không gửi tin gộp |
| `CB-03` | M3 trả lời M3-14 (+ chief chốt `Q-20`) | Gói C13 + C17 + B7: endpoint `POST …/tasks/{taskId}:revoke` (auth Order Core, idempotency scope `task-revoke`, thu hồi một chiều — `revoked_at` đầu tiên thắng, `Idempotency-Key` là command id cho RVK-05/06), đóng job ở **mọi** trạng thái mở (kể cả task bị thu hồi trước eligibility — hiện treo mãi), sweep đóng im lặng (không result/callback/incident/metric capacity), fence 2 kết thúc có chủ đích (ngoại lệ riêng, trả kênh, không FailCount, migration mở CHECK trạng thái attempt), pump coi là trung tính; OAS + changelog + IR-06/07/08 + `sandbox-examples`; test `IT-API-REVOKE-01..06`, `IT-ELIG-REVOKE-02`, viết lại `IT-TEL-REVOKE-02`, `UT-SCH-PUMP-REVOKE-01`, `IT-REVOKE-ATTEMPT2-01`, `IT-SCH-REVOKE-03/04`; sửa đồng bộ bản chép vị từ ở `SchedulerQueueBacklog.cs:56-95` | 24–26 | ⛔ M3 |
| `CB-04` | M3 trả lời M3-07 | C3/C4/C9-session theo `Q-34` | 10–12 | ⛔ M3 |
| `CB-05` | M3 trả lời M3-04 / có registry không | C1 theo `Q-33` | 0–10 | ⛔ M3 |
| `CB-06` | M3 trả lời M3-08/09/10/21, dựng consumer + OAS chính thức; credential + issuer (`Q-35`) | C10: chạy D-5 đủ ca trên môi trường chung (4–6 giờ); ca `TV1-E2E-12` phát lại muộn (3–4 giờ, sau `Q-19`). C11: bộ cấp token thật (lỗi lấy token là lỗi tạm, không phải `AUTH_REJECTED`), gỡ nhánh fail W-0006 bằng điều kiện dương, secret + egress, readiness callback ở `/healthz` worker, một `IVR_CONFIRMED` nhận ACCEPTED trên môi trường chung (12–16 giờ) | 20–26 | ⛔ M3 + Platform |
| `CB-07` | M3 đối ký bảng map v1; Sếp + M3 chốt ngưỡng bộ đếm không nghe máy (có tính ca bấm sai hay không) | Không có việc code (M8 giữ: lỗi provider/model không tính lượt khách) | 0 | ⛔ M3 + Sếp |
| `CB-08` | M3 dựng việc giữ đơn 24/7 COD phát sinh ngoài giờ tới 08:00 (biên theo `Q-22`) và rải việc gửi lại | — | 0 | ⛔ M3 |
| `CB-09` | M3 xác nhận `phone_validation_status` + quy tắc TTL (C6) | 0 nếu chấp nhận; ~3 giờ nếu M3 xin dung sai TTL | 0–3 | ⛔ M3 |
| `CB-10` | Sếp trả lời B2 (`Q-32`) | B11 bước 2 — một nguồn sự thật, sửa ≥12 chỗ: API-04 §2, IR-06 §6 (`:1347`, `:1363-1368`), register OD-V1-18, `05-pii-policy` `:8/:15/:30/:47`, `data-inventory` `:41/:84`, `PersonalDataInventory.cs:108-112`, `ProductionDialTokenVault.cs:10-12`, `AsteriskAriSimGateway.cs:28`, `secret-inventory` §1, chính sách SEC-ROT-05; + việc theo phương án (ii: 16–20 giờ, biến thể 6–8; iii: 12–16 giờ + M3). **Không** chạy stage 3–5 của W-0311 trước đó | 3–20 | ⛔ Sếp |
| `CB-11` | Sếp ký S2 (7 rủi ro, gồm điểm 6 = X14/C12) trên tờ trình `Q-08` | Nâng lại `legal_gate` (ghim chuỗi lần 2, gộp với `CB-15` nếu kịp); đóng C5 | 1 | ⛔ Sếp |
| `CB-12` | Sếp chọn nhà mạng + ký hợp đồng trunk (S6; MobiFone đồng ý, chờ báo giá) | B12: SIP-01/07a (8–12), DT-02 map disposition từng mã + `Q-27` (12–16), W-0008 (6–10), B1 hiệu chỉnh 35/40/50/60 (8), SIP-04 (12–16), SIP-09 tải ≥2 giờ + đối soát cước (16–24), SIP-10 bàn giao/pilot (8–16); B1 bước 4 số kênh | 70–100 | ⛔ Sếp |
| `CB-13` | Bản duyệt SIP-04 (mở `IsReady` production) — Sếp | Điều kiện trước (đều phải xong): B13 ✅, `K-42`, `K-43`, `K-44`, `K-47`, `Q-28`, N1 (`K-49`, `K-50`, `Q-29.2`, `Q-29.4`), S1 (`Q-30`), `Q-32`, bộ tạo hàng `ivr_sim_channels` cho PRODUCTION_REAL (runbook `:121` đang hướng dẫn thêm tay), cấu hình log Asterisk (số trong query string ARI) | trong CB-12 | ⛔ Sếp |
| `CB-14` | Lần bật `REAL_CUSTOMER_CALL_ALLOWED` đầu tiên — chữ ký Sếp (OD-V1-12, N04) | Mở khoá K1 theo quy trình SIP-04 | — | ⛔ Sếp |
| `CB-15` | Chief cấp kho/chỗ (`Q-24`) + S5 có kho thật | B8 bước 3: chép 13 artifact sang kho được cấp, kiểm theo mẫu `verify-mirror.py` (W-0340), sửa 13 URI `MODELS.lock` (`:44, 61, 78, 95, 112, 129, 146, 163, 180, 197, 214, 231, 248`), ghim lại `expectedArtifactSetSha256` + chuỗi lock, `internal_mirror_gate` PASS với người quyết là người cấp kho | 2–3 | ⛔ chief |
| `CB-16` | Chief: phân xử §13/DR-02 (C2), sửa FIX_M8 mục 8 (C12), ghi phương án B vào ma trận S7 + sửa ghi nhận B11 (C19), rà các ô "owner" sau N04 | Đồng bộ tài liệu theo phân xử | 0,5 | ⛔ chief |
| `CB-17` | W-0121 — pipeline hosted xanh | Sau `K-01` + `Q-14` + `K-05`: ghi pipeline/job ID, đổi trạng thái W-0121 | 0,5 | ⛔ (sau L0, Q-14) |
| `CB-18` | N1, phần không phải code: người nghe duyệt clip (Sếp/Tech Lead chỉ định, không phải Toàn); danh mục tên công khai SKU (M1/Sếp — chỉ khi `Q-29` chọn (a); repo ghi 20 SKU); định dạng mã đơn (M3 — nếu `Q-29` chọn (b)); máy render (chief) | `Q-29.3`, `Q-29.4` | — | ⛔ Sếp/M3/chief |
| `CB-19` | B10 — giữ constraint `ck_ivr_call_results_action_matches_type` (chief chấp nhận 16/09) | Không có việc | 0 | ➖ |
| `CB-20` | B2 — anh Mạnh xử lý 4 file ở checkout văn phòng (`Q-36`) | Không có việc dev | 0 | ➖ |

---

## §7 — Tin nhắn cần gửi (mỗi người một lần, gộp)

| Gửi ai | Nội dung | Mở khoá |
| --- | --- | --- |
| Chief | **Không gửi** (25/09, Toàn): `Q-20…Q-29` đã chốt theo đề xuất M8 ở §5. Nội dung giữ để tham khảo: (1) Nguyên văn `ivr-cancel-reason-map.v1`. (2) Xin quyết `Q-20…Q-29`. (3) Báo hai lỗi an toàn/đường production: `Q-23` (token tĩnh luôn được nhận) và `Q-28` (DispatchGate chặn mọi địa chỉ production). (4) Chỗ file 25/09 lệch cây: C16 "không còn việc lõi" (sai — `Q-28`), N1 20 giờ (thực tế 36–44), 19 SKU (repo 20), lấy lại `StaticFileTtsProvider` (sai cơ chế), B5 "schema không phân biệt được" (`Q-09`), ca Giờ Vàng của B17 chưa có. (5) Câu hỏi vế S3 của OD-V1-11. (6) Chuyển tờ trình `Q-08` cho Sếp | `CB-02`, `Q-20…Q-29`, `Q-30…Q-32` |
| Anh Mạnh | IR-07 gửi lại (sau `L2`); nhắc các ô `M3-07`, `M3-14`, `M3-31`, `M3-32` | `CB-01`, `CB-03…CB-09` |
| Sếp (qua chief) | Tờ trình `Q-08`: B2 (`Q-32`), S2 bảy rủi ro, S1 (`Q-30`), lần bật gọi thật đầu tiên (N04), nhà mạng (S6) | `CB-10…CB-14` |

---

## §8 — Kiểm sau mỗi lô (không lô nào đóng mà thiếu bước này)

```bash
# Git Bash · ngoài giờ soak · test tích hợp chạy ban ngày cho tới khi xong K-05
# 0. Trước khi sửa symbol: gitnexus_impact({target: "<symbol>", direction: "upstream"}) — HIGH/CRITICAL thì dừng, báo

# 1. Test đầy đủ — baseline 1254 (813 unit + 24 contract + 409 integration + 8 chaos)
dotnet test Ivr.sln

# 2. Có TestId mới
node deploy/ci/scripts/generate-test-traceability.mjs

# 3. Gate sweep — baseline 44/44; không chạy song song dotnet test
node deploy/ci/scripts/gate-sweep.mjs

# 4. Có sửa tracker hoặc sổ quyết định
node deploy/ci/scripts/gate-status.mjs --write

# 5. Phạm vi thay đổi
gitnexus_detect_changes()

# 6. Commit theo pathspec, một W-ID một lô (cấp W-ID ngay trước bước này)
git commit -m "type(scope): W-XXXX mô tả" -- <paths>

# 7. Sau commit: gitleaks chạy trên commit; file sinh lại báo nhầm thì thêm fingerprint theo dòng ở commit thứ hai
```

| Lô có… | Chạy thêm |
| --- | --- |
| Sửa IR-06 | 4 validator (`d06`, `dial-token`, `opt-out`, `upstream-session`) + `--check-template` 3 template; `contract-freeze-verifier.mjs` |
| Sửa OAS (`draft.34`) | `openapi-contract-drift.mjs --accept-reviewed-draft`; `generate-oasdiff-changelog.sh` (image oasdiff đã ghim); `build-api-docs.mjs`; `openapi-lint-gate.mjs`; `validate-openapi.mjs`; baseline mới trong `specs/api/openapi/baselines/`; `ApiBehaviorMatrixTests` (IT-API-MATRIX-38) nếu thêm path |
| Sửa `MODELS.lock` | `tts-provenance-gate.mjs --selftest`; `tts-voice-acceptance-gate.mjs --selftest`; `b3-telephony-evidence-validator.mjs --self-test` + `--check-template` |
| Sửa compose/helm | `k8s-selftest`, `image-selftest` (cổng cố định `55433`/`58080` — tắt stack dev trước), `docs-selftest` |
| Đổi hành vi thấy được từ ngoài | Dựng stack sạch trên server test (thư mục `/home/ssv/m8`, cổng 68xx, tiền tố `m8_`), chạy e2e **không seed tay**, thêm ca đêm và ca biên sáng |

---

## §9 — Lịch đề xuất (ngày công)

| Ngày công | Việc | Giờ |
| --- | --- | ---: |
| 1 | Toàn chốt `Q-01`, `Q-02`, `Q-03`, `Q-13`, rồi `Q-20…Q-29` theo đề xuất M8 (không gửi tin §7); `L0` (sau 14:42); `L1` | ~3 |
| 2 | `L2` (+`Q-01.1`, `Q-02.1`, `Q-03.1`, `Q-13.1`) → `CB-01` gửi IR-07; `Q-08` tờ trình | ~9 |
| 3 | `L3` + `L4` | 6 |
| 4 | `L5` + đầu `L6` | 8 |
| 5 | Hết `L6` + đầu `L7` | 8 |
| 6 | Hết `L7` + `L8` | 8 |
| 7–10 | Việc sau quyết định của Toàn: `Q-04`, `Q-05`, `Q-09`, `Q-10`, `Q-12`, `Q-14`, `Q-15`, `Q-16`, `Q-17` + `Q-18`, `Q-19` | ~30 |
| 11–12 | Việc sau quyết định 25/09: `Q-22.1–2`, `Q-23`, `Q-24`, `Q-27` (sau khi đo), `Q-28.1–3` | ~17 |
| sau đó | N1 `Q-29.1…5` (`Q-29` đã chốt PA2 25/09); gói C13 (`CB-03`); C3/C4 (`CB-04`); C11 (`CB-06`); B12 (`CB-12`) | theo mở khoá |

---

## §10 — Rủi ro đã biết

| # | Rủi ro | Chặn bằng |
| --- | --- | --- |
| 1 | Hai phiên cùng sửa cây, lô này nuốt việc dở của lô kia | Đánh 🟡 trong file này trước khi làm; luật 9 (pathspec, kiểm phiên khác) |
| 2 | Push hoặc chạy tải nặng khi soak đang đo | Luật 10; gitlab-runner chạy trên chính máy này |
| 3 | Sửa IR-06 nhiều lượt, quên ghim lại 1 trong 7 nơi | Gom một lượt mỗi lô; `K-19` là bước bắt buộc cuối `L2` |
| 4 | Hạ `legal_gate` thiếu một điểm ghim ⇒ CI đỏ | Checklist `Q-17.1` lấy từ `d87c96a` (8 sửa + 1 mới), kể cả tầng 2 |
| 5 | Siết gate tts-provenance trước khi hạ PASS ⇒ CI đỏ | `Q-18` chỉ làm cùng hoặc sau `Q-17` |
| 6 | Sửa token tĩnh (`Q-23`) làm gãy lab đang dùng `TARGET_V1` | Kiểm mọi compose/profile trước khi sửa; chỉ làm sau khi chief/Tech Lead xác nhận thứ tự |
| 7 | `Q-13` đụng guard HIGH (mọi task đi qua) | `UT-INTAKE-WINDOW-SWEEP-01` quét từng giây trong ngày + toàn bộ test intake |
| 8 | Test đỏ theo giờ bị đọc nhầm là lỗi của lô | Luật 11 tới khi `K-05` xong |
| 9 | N1 chặn cứng làm đổi nhánh LAB | Giữ nhánh LAB không đổi một byte; so hash văn bản; chạy lại `AsteriskLabTelephonyTests`, `SegmentedSpeechTests`, `MockTelephonyPersistenceTests`, `SipTrunkProductionDialTests` |
| 10 | Gửi IR-07 trước khi xong `L2` ⇒ M3 dựng theo tài liệu sai | `CB-01` chỉ sau `L2` + `Q-02` + `Q-03` |
| 11 | Kế hoạch này là giả thuyết đo tại `f3e26a6`, sẽ cũ dần | Luật 16: đọc lại code trước khi sửa; khẳng định nào sai thì ghi vào file này, không sửa im lặng |

---

## §11 — PHẦN IV: Bảng truy vết — mọi mã của chief

| Mã chief | Tên ngắn | Trạng thái 25/09 (sau `f3e26a6`; dòng có `3d04eee` hoặc ✔ 25/09 cập nhật chiều 25/09) | Task trong kế hoạch |
| --- | --- | --- | --- |
| `A1` | Owner IVR tự ký quyết định ngoài M8 | Bước 4, 7 ✅; còn 1, 2, 3, 5, 6 | `Q-08`, `Q-17`, `K-18`, `Q-04`, `Q-18`, `K-06` |
| `A2` | `phone_e164` đọc được, giữ vĩnh viễn | ⛔ Sếp (B2) | `Q-32`, `K-40`, `K-24`, `CB-10` |
| `A3`, `A4` | Tự nhận thẩm quyền ký S2, kiêm Legal/Privacy | ➖ gộp A1 | `Q-08`, `Q-17`, `Q-18` |
| `B1` | Capacity 800–1200 cuộc/phiên | Bước 1 ✅ + sót | `K-21`, `Q-31`, `CB-12` |
| `B2` | 4 file softphone văn phòng | ➖ không giao dev | `Q-36`, `CB-20` |
| `B3`, `B4`, `B6` | Đã xong vòng 16/09 | ✅ | `K-06` (dòng chữ còn sót của B6) |
| `B5` | Runbook rollback W0122/P03 | Một phần ✅ | `K-20`, `Q-09` |
| `B7` | Deadline sweep không biết task bị thu hồi | ✔ chờ gói C13 | `Q-06`, `CB-03` |
| `B8` | TTS VieNeu | Chuyển sang N1 | `K-48…K-50`, `Q-11`, `Q-17`, `Q-24`, `Q-25`, `Q-26`, `Q-29`, `CB-15` |
| `B9` | Admin UI/monitoring §16 | Bước 2 ✅ + sót; bước 1 chưa hỏi M3 | `K-15`, `K-17`, `Q-07` |
| `B10` | Constraint khoá hành vi V0.3 | ➖ gộp C7; giữ constraint | `K-12`, `CB-19` |
| `B11` | Vị trí số E.164 | Bước 0 ✅ + sót; bước 1–4 ⛔ Sếp | `K-39`, `K-41`, `Q-28`, `Q-32`, `CB-10` |
| `B12` | Telephony adapter production | ⛔ nhà mạng; `Q-27`, `Q-28` ✔ PA2 25/09 | `K-25`, `K-42…K-47`, `Q-27`, `Q-28`, `CB-12`, `CB-13` |
| `B13`, `B14` | `LabRealSim` gán cứng | ✅ + bù | `K-42` |
| `B15` | `ToString` record mang số | ✅ + bù | `K-37`, `K-38` |
| `B16` | `total_amount` số lẻ, cách ly SIM | Một phần ✅ | `K-14`, `K-26`, `K-27`, `K-29`, `K-30`, `Q-12`, `Q-16` |
| `B17` | Guard W-0298 ở biên buổi sáng | `Q-13` ✅ `3d04eee`; `Q-22` ✔ PA2 25/09, chưa làm | `Q-13`, `Q-22` |
| `C1` | `program_code` | ⛔ M3 | `Q-33`, `CB-05` |
| `C2` | 9 result code, dòng m8-05 | ✅ đính chính + còn câu trỏ; §13 ⛔ chief | `K-13`, `CB-16` |
| `C3`, `C4` | `session_id` | ⛔ M3-07 | `Q-34`, `CB-04` |
| `C5` | Hồ sơ trình Owner | ➖ gộp A1 | `Q-08`, `CB-11` |
| `C6` | Contract contact | ⛔ M3 | `CB-09` |
| `C7`, `C8` | Bảng map hủy; rút 24 giờ | IR-07 ✅ + sót | `K-12`, `K-18`, `Q-01`, `Q-03`, `CB-07` |
| `C9` | Shape `ivr_task` | ✅ + sửa dẫn chiếu | `K-07`, `CB-04` |
| `C10` | Shape callback | `Q-21` ✔ PA1 25/09; ⛔ M3 (`CB-06`) | `Q-21`, `CB-06` |
| `C11` | Đường callback thật | ⛔ M3 + credential | `Q-35`, `CB-06` |
| `C12` | `IVR_OPT_OUT` | ⛔ Sếp S2 điểm 6 + chief | `K-51`, `CB-11`, `CB-16` |
| `C13`, `C17` | Thu hồi task; re-check trước attempt 2 | `Q-20` ✔ PA1 25/09; ⛔ M3-14 | `Q-20`, `CB-03` |
| `C14` | Kết nối M2 | ✅ + sót checklist | `K-13` |
| `C15`, `C20` | Đơn đêm | Bước 1, 3, 4 ✅ + sót | `K-05`, `K-11`, `Q-10`, `Q-13`, `Q-22`, `CB-08` |
| `C16` | Contact Gate / dial token | ⛔ Sếp + SIP-04; 🆕 có việc dev mới (`Q-28` ✔ PA2 25/09, chưa làm) | `Q-28`, `CB-10`, `CB-13` |
| `C18` | Server test ngoài cấp phát | `Q-24` ✔ PA3 25/09, chưa làm | `Q-24`, `CB-15` |
| `C19` | Phương án B trên contract | ➖ gộp A2 | `Q-32` |
| `C21` | Phát lại callback không giới hạn tuổi | Tài liệu ✅; code ⏸ | `K-13`, `Q-19` |
| `C22` | Bảng map v0 bỏ phân biệt runtime | ⏸ chờ nguyên văn v1 | `Q-03`, `CB-02` |
| `C23` | IR-07 hứa VieNeu đọc lúc gọi | Đính chính ✅ `3d04eee` (`Q-02`); phần N1 theo `Q-29` PA2 | `Q-02` |
| `N1` | Không sinh giọng lúc gọi | ⬜ (`Q-25`, `Q-26` PA1 và `Q-29` PA2 ✔ 25/09) | `K-48`, `K-49`, `K-50`, `Q-11`, `Q-25`, `Q-26`, `Q-29` |
| `N2…N5` | ORDER_VERIFIED, bộ đếm, MB, giá | ✅ còn đúng (kiểm lại 25/09); N3 đầu-cuối chờ C22 | — |
| `V6-1`/`V6-9`, `V6-4`/`V6-12`, `V6-8`/`V6-17` | Tất định, không classifier, không ORDER_VERIFIED | ✅ phân tích | — |
| `V6-2`/`V6-10` | Template duyệt, có version | B13 ✅; S1 ⛔ Sếp | `Q-30` |
| `V6-3`/`V6-11`, `V6-16` | Model Gateway; luật quá độ | → N1 | như N1 |
| `V6-5`/`V6-13` | Correlation ngữ cảnh nội dung | Chờ seam V6-d/e (M3/M5) | — |
| `V6-6`/`V6-14` | Provider hỏng → fail an toàn | Đạt vế không tính lượt; 🆕 lỗ luồng ARI | `K-47`, N1 |
| `V6-7`/`V6-15` | Classifier P1 | Không làm trước cổng V6.4 | — |
| `D1…D11` | Đã đạt | ✅ còn đúng (kiểm lại 25/09) | `K-08`, `K-05` (chữ D11) |
| Dòng 106, 123 | `IVR_ADAPTER_MODE` dùng làm bằng chứng | 🆕 biến không có tác dụng | `Q-15` |
| Dòng 120, 717 | Thiếu `X-Actor-Id` → 403 | ✅ `f3e26a6` (IR-06, IR-08) | — |
| Dòng 121 | Cảnh báo "(none registered)" lúc khởi động | ✅ 5aec684 | `K-33` |
| Dòng 714 | CI hosted không xanh | `openapi_lint` ✅; còn `deploy_dev` | `K-01`, `K-05`, `Q-14`, `CB-17` |

---

**Lập bởi:** agent (phiên "Codebase review và implementation plan") cho Nguyễn Quốc Toàn · `25/09/2026` ·
`main@f3e26a6` · rà bằng 10 sub-agent chỉ-đọc, đối chiếu lại trên `f3e26a6`; chưa chạy build/test/gate (soak đang đo).
