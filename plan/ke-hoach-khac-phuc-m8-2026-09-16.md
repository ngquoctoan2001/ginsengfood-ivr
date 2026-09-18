# KẾ HOẠCH KHẮC PHỤC — Module 8, tuần `16/09`

**Người thực hiện:** Nguyễn Quốc Toàn · **Lập ngày:** `16/09/2026` · **Baseline:** `main@37606bc`
**Căn cứ:** [`PHAN-HOI-toan-viec-can-lam-m8-2026-09-16.md`](PHAN-HOI-toan-viec-can-lam-m8-2026-09-16.md) §5

> **Phạm vi:** `17` hạng mục tôi tự đóng được, **không cần chữ ký của ai**. `24` mục còn lại chờ
> phần đơn hàng M3 hoặc báo giá nhà mạng — **không lên lịch được**, liệt kê ở §6 để theo dõi.
>
> **Tổng: `7` ngày làm việc** (`+2` ngày tuỳ chọn cho `B9`).

---

## §0 — LƯỢT `W-0297` ĐANG MỞ TRONG CÂY (chủ ý, không phải bước chặn)

Cây làm việc có **`97` thay đổi chưa commit** thuộc lượt `W-0297` — dồn `plan/ivr-orther` theo chỉ
đạo owner `16/09`. **Đây là lượt việc tôi đang làm dở, cố ý để mở**, không phải rác cần dọn.

| Loại | Số file | Là gì |
| --- | ---: | --- |
| `D` xoá | **27** | `plan/ivr-orther/*` — `m8-05`, `m8-16`, `m8-17`, `od-v1-signoff`, `questions-to-*`… |
| `M` sửa | **62** | Evidence README + contract pack — sửa link trỏ tới file vừa xoá |
| `??` mới | **8** | `00-CHUA-XONG.md` (`243` dòng), `00-DA-XONG.md` (`86` dòng) + 3 file phản hồi/kế hoạch |

### Đã kiểm: `6/7` lô code **không đụng** lượt đang mở

| Lô | File đích | Trạng thái cây |
| --- | --- | --- |
| `W-0298` `C15` | `CallingWindow.cs` · `TaskIntakeService.cs` · tests | ✅ **sạch** |
| `W-0299` `B6` | `4` file `appsettings` · `2` file test | ✅ **sạch** |
| `W-0300` `B3` | migration mới · fixture `UT-POLICY-SIGNED` | ✅ **sạch** |
| `W-0301` `B4` | `RuntimeGateApprovals.cs` · migration mới · `IT-GATE-APPROVAL-10` | ✅ **sạch** |
| `W-0302` `C6` | OAS yaml · `contract-manifest.json` · `4` script CI · changelog | ✅ **sạch** |
| `W-0304` vận hành | compose · `rollback.md` · Dockerfile migrate · cấu hình log | ✅ **sạch** |
| **`W-0303` tài liệu** | **`IR-06`** · **`open-decisions-register.md`** · `specs/ui/04` · `OptOutSuppression.cs` | ⚠️ **`2` file đang mở** |

⇒ **Không có lô nào bị chặn.** `6` lô chạy được ngay trong lúc `W-0297` còn mở.

### Luật commit khi cây có lượt khác đang mở

| # | Luật | Vì sao |
| --- | --- | --- |
| 1 | ⛔ **Không bao giờ `git add -A` / `git add .`** | Sẽ quét trọn `97` file `W-0297` vào commit code — đúng thứ `main@2a4f45d` (commit `save` trộn `98` file) từng gây ra, và là lý do phải dựng nhánh provenance `W-0130` |
| 2 | ✅ **`git add <đường dẫn cụ thể>`** cho từng lô | Một `W-ID` một commit, phạm vi đọc được |
| 3 | ✅ **`gitnexus_detect_changes()` trước mỗi commit** | Bắt được file lọt ngoài phạm vi |
| 4 | ✅ `git status --porcelain` sau commit — phần `W-0297` phải **còn nguyên** | Xác nhận không nuốt nhầm |

### Xử lý `2` file chồng lấn của `W-0303`

`W-0303` có `6/10` mục chạm **`IR-06`** (mục `1`,`4`,`5`,`6`,`7`) và **`open-decisions-register.md`**
(mục `8`) — cả hai đang nằm trong `W-0297`.

**Ba cách, chọn một:**

| | Cách | Khi nào hợp |
| --- | --- | --- |
| **A** *(đề xuất)* | Commit `W-0297` trước khi tới ngày `5`, rồi `W-0303` chạy trên cây sạch | `W-0297` là lượt dọn tài liệu — tự nó đã trọn vẹn, commit sớm được |
| **B** | Gộp `6` mục `IR-06`/register vào luôn `W-0297` | Nếu `W-0297` còn mở lâu — đằng nào cũng đang sửa hai file đó |
| **C** | `W-0303` làm trước `3` mục **không** đụng (mục `2` `specs/ui/04` · mục `9` `OptOutSuppression.cs` · nửa `specs/api/04` của mục `7`), để `7` mục kia lại sau | Nếu muốn giữ hai lượt tách bạch tuyệt đối |

> **Dù chọn cách nào**, `W-0303` mục `10` vẫn phải làm: `W-0297` xoá đúng những file mà bản phản hồi
> và các evidence README dẫn chiếu (`m8-05`, `m8-16`, `m8-17`, `od-v1-signoff`). Nội dung đã chuyển
> vào `00-CHUA-XONG.md` / `00-DA-XONG.md` — **các dẫn chiếu phải trỏ lại chỗ mới**, nếu không
> `docs-selftest.mjs` sẽ đỏ vì link chết.

### Bắt đầu từ đâu

**Chạy `W-0298` được ngay, không cần đợi gì.** Ngày `1` và ngày `2` hoàn toàn không chồng lấn.

---

## §1 — LUẬT THI HÀNH (ràng buộc mọi bước, không có ngoại lệ)

| # | Luật | Nguồn | Hậu quả nếu phá |
| --- | --- | --- | --- |
| 1 | **Chỉ làm trên `main`.** Không tạo nhánh dưới bất kỳ cách viết nào | `CLAUDE.md` | `.githooks/reference-transaction` chặn cứng |
| 2 | **Chạy `gitnexus_impact` trước khi sửa bất kỳ symbol nào**, báo blast radius | `CLAUDE.md` | Sửa mù, vỡ call site không biết |
| 3 | **Chạy `gitnexus_detect_changes` trước mỗi commit** | `CLAUDE.md` | Commit chạm ngoài phạm vi |
| 4 | **Không sửa migration đã áp dụng.** Đổi hành vi ⇒ migration **mới** | `progressive-selftest.mjs`, `migration-expand-guard.mjs` | DB đã chạy bản cũ lệch schema vĩnh viễn |
| 5 | **Không seed `ivr_audit_log` từ migration** | Bài học `W-0196` | Trigger append-only làm hàng đó **không gỡ được**; một tá test giả định log rỗng |
| 6 | **Sửa file đã ghim hash ⇒ re-pin ngay trong cùng lượt** | Bài học `W-0216` | Gate đỏ ở lượt sau, không ai biết tại sao |
| 7 | **`gate-status.yaml` và `readiness-board.md` sinh bằng `--write`**, không gõ tay | Bài học `W-0216` | Board lệch khỏi tracker |
| 8 | **Kiểm từ ngoài, không chỉ `dotnet test`** — test xanh vẫn giấu lỗi image/compose/env | Bài học `W-0282` | Đúng lớp lỗi smoke `16/09` vừa bắt (`krb5`, seed thủ công) |
| 9 | **Dùng `git commit -- <paths>`, không `git add` rồi `git commit`.** Index đã có `27` staged-deletion của `W-0297`, nên `git add <paths>` vẫn cuốn theo chúng — đã dính ở `W-0298` | §0 · `W-0298` | Trộn hai lượt việc vào một commit |
| 10 | **File dẫn xuất chỉ được commit cùng thứ nó dẫn ra.** Trước khi chạy `gate-status.mjs --write` hoặc `generate-test-traceability.mjs`, kiểm `git status`: nếu cây có file chưa track / chưa commit của phiên khác **mà generator đọc tới**, thì output của nó **không commit được đúng** — chờ họ commit, hoặc commit phần mình trước. Chạy `--check` ngay trước commit | **3 lần trong ngày 16/09** | `HEAD` trỏ tới file `HEAD` không có ⇒ gate đỏ trên checkout sạch |

> ### ⚠️ Luật `10` sinh ra từ ba lần vấp thật, không phải lo xa
>
> Hai phiên Claude dùng **chung một checkout**. Cả hai generator đều **đúng** — chúng đọc **cây**.
> Nhưng một commit chỉ là **một tập con của cây**, nên file dẫn xuất nào trải qua việc của cả hai
> phiên thì sẽ **sai ở phiên nào commit trước**.
>
> | Lần | Commit | File dẫn xuất | Trỏ tới thứ `HEAD` không có |
> | --- | --- | --- | --- |
> | 1 | `1651e8f` (tôi) | `docs/traceability-tests.md` | `16` hàng `UT-TRUNK` → `SipTrunkProductionDialTests.cs` |
> | 2 | `ffa0284` (tôi) | `docs/release/gate-status.yaml` | `evidence: docs/evidence/W-0303/README.md` |
> | 3 | *(suýt)* | `prompt-execution-tracker.md` | phiên kia đã stage hàng `W-0302` của tôi, dừng lại kịp |
>
> **Bất biến chính xác hơn "kiểm `git status`":** nếu **đầu vào** của generator gồm file bạn
> **không** commit trong lượt này, thì **đừng commit output của nó**.
>
> | File dẫn xuất | Đầu vào |
> | --- | --- |
> | `gate-status.yaml` | `prompt-execution-tracker.md` — có thể chứa hàng của phiên khác |
> | `docs/traceability-tests.md` | **mọi** file test trong cây — kể cả test chưa track |
>
> ⚠️ **Và `gate-status.yaml` + tracker không tách ra hai commit được**: `gate-status.mjs` assert
> yaml khớp tracker và đỏ khi lệch, nên chúng buộc phải đi cùng một commit với hàng vừa đổi.

**Quy ước commit:** một `W-ID` một commit, message theo mẫu đang dùng — `type(scope): W-XXXX mô tả`.
`NEXT_WORK_ID` hiện là **`W-0298`** (`prompt/_execution/prompt-execution-tracker.md`).

---

## §2 — THỨ TỰ THI HÀNH

Sắp theo **mức thiệt hại đang xảy ra**, không theo độ khó:

```text
(W-0297 dọn tài liệu vẫn mở trong cây — KHÔNG chặn lô nào, xem §0)

Ngày 1  W-0298  C15  đơn ngoài giờ              ← ĐANG MẤT ĐƠN MỖI ĐÊM
        W-0299  B6   cờ production về NO        ← 0,5 ngày, trả lời A1 bằng code

Ngày 2  W-0300  B3   hạ approved_for_production  ┐ hai migration, một lượt
        W-0301  B4   RUNTIME_GATE_ADMIN scope env┘ chạy DB test chung

Ngày 3-4 W-0302 C6   500 → 422 + phát hành OAS  ← nặng nhất: re-pin 5 nơi

Ngày 5  W-0303  lô tài liệu (10 mục)         ← 2 file chồng W-0297, xem §0

Ngày 6  W-0304  lô vận hành (4 mục, từ smoke test)

[tuỳ chọn] Ngày 7-8  W-0305  B9 audit-evidence
```

**Vì sao `C15` đứng đầu chứ không phải `A1`:** `C15` là mục duy nhất trong cả `30` mục **đang gây
thiệt hại tiền thật mỗi đêm**. `A1` phần lớn là việc của chief + một xác nhận cấp công ty; phần của
tôi trong `A1` là đổi nhãn register — nằm ở lô tài liệu ngày `5`.

---

## §3 — CHI TIẾT TỪNG LÔ

### ✅ `W-0298` — `C15`: đơn 24/7 đặt ngoài khung giờ không được gọi cuộc nào — **ĐÃ XONG `16/09`**

| | |
| --- | --- |
| **Mức** | 🔴 Đang mất đơn COD mỗi đêm |
| **Ngày** | `1` (thực tế: nửa ngày) |
| **Chặn bởi** | Không ai |
| **Trạng thái** | ✅ **XONG** — `dotnet test Ivr.sln` **`1063/1063`** pass, `0` failed, `0` skipped, exit `0` (baseline `1060` + `3` test mới) · phạm vi đúng `5` file · ✅ **đã commit `92091a1`**, lượt `W-0297` còn nguyên `97` file |

> ## ⚠️ PHƯƠNG ÁN ĐÃ ĐỔI KHI VÀO CODE — phương án `(A)` trong bản kế hoạch **không thi hành được**
>
> Kế hoạch ban đầu chọn *"IVR dời `T0`/cửa sổ tới giờ mở"*. Đọc code thì **không làm được**, và lý do
> đáng ghi lại:
>
> | Sự thật trên code | Hệ quả |
> | --- | --- |
> | `TaskIntakeService.cs:146-149` — `ConfirmationWindow.Create(source.Confirmation_window_started_at, source.Confirmation_window_expires_at)` | **`T0` và cửa sổ do M3 gửi trên wire**, IVR không tự tính |
> | `W-0246` — `dial_token_expires_at` **==** window end, ghim ở `3` tầng | Dời cửa sổ ⇒ token M3 đã cấp rơi **ra ngoài** cửa sổ mới |
> | `OD-V1-05` — M3 là bên cấp `dial_token`; chưa có resolver production | IVR **không cấp lại token được** |
> | `TaskIntakeService.cs:339-341` — từ chối nếu `Confirmation_window_started_at > now` | Cửa sổ phải **đã bắt đầu** khi task tới ⇒ M3 gửi ngay tại `T0` |
>
> ⇒ Dời cửa sổ **9 tiếng** sang sáng hôm sau sẽ làm dial token chết trước khi quay số. Phương án
> `(A)` không phải khó — nó **sai**.

#### Phương án đã thi hành: từ chối tại intake, không giả vờ nhận

Hôm nay: task được nhận → `0` cuộc gọi → `15` phút sau phát `IVR_CONFIRMATION_WINDOW_EXPIRED` — một
mã **đọc ra là "khách không xác nhận kịp"** trong khi thực tế **chưa ai gọi**. Sales không phân biệt
được với khách phớt lờ cuộc gọi thật.

Nay: intake nhìn trước — nếu **mọi** lần gọi theo policy đều rơi ngoài giờ được phép gọi thì trả
`TASK_BLOCKED_OPERATIONAL` kèm `blocked_reasons` mới, **ngay lúc M3 còn đang giữ đơn**.

| Mặt cắt wire | Có đổi không |
| --- | --- |
| `decision` — enum đóng | ❌ **Không** — `TASK_BLOCKED_OPERATIONAL` đã có sẵn trong OAS `:1365` |
| `blocked_reasons` — `array of string`, không phải enum | ❌ **Không** — thêm một chuỗi là non-breaking |
| Schema, version OAS | ❌ **Không** — `W-0298` không cần phát hành contract |

> **Đây là từ chối giả vờ, không phải lời giải cho đơn đêm.** Đơn đêm vẫn cần được gọi; giữ tới sáng
> hay cấp cửa sổ khác là **quyết định của M3**, vì cửa sổ và token đều tới trên wire đã cấp sẵn.
> Việc của tôi là không báo sai nguyên nhân.

**Triệu chứng hiện tại:**

```text
23:00        task 24/7 vào, confirmation window bắt đầu (900s)
23:00–23:15  gate gọi ĐÓNG suốt (ngoài 08:00–21:08) → 0 cuộc gọi
23:15        window hết → IVR_CONFIRMATION_WINDOW_EXPIRED gửi về M3
```

Khách **không nhận cuộc nào**, M3 nhận mã *"hết hạn xác nhận"*.
Gốc: `CallingWindow.cs:137-153` ngoài giờ chỉ trả `opensAt` — **không dời `T0`, không dời cửa sổ**.

#### ⚠️ Hai phương án — tôi chọn (A), và nói rõ vì sao

| | (A) IVR dời `T0`/cửa sổ tới giờ mở | (B) M3 giữ đơn đêm, gửi lúc `08:00` |
| --- | --- | --- |
| Đổi wire/schema | **Không** | Không |
| Cần M3 làm gì | **Không** | Đổi producer |
| Làm được tuần này | ✅ | ❌ chờ phần đơn hàng M3 |
| Đổi hành vi M3 quan sát được | `expires_at` muộn hơn M3 tính | không |

> **Chọn (A) làm chốt chặn đáy, không phải làm lời giải cuối.** Hôm nay đơn đêm **chắc chắn chết**;
> (A) biến nó thành **chắc chắn được gọi lúc `08:00`**. Đó là cải thiện tuyệt đối, không cần ai ký.
> Nếu sau này M3 muốn giữ đơn đêm ở phía họ (phương án B) thì (A) tự động trở thành đường không bao
> giờ chạy — **không phải gỡ gì cả.**
>
> ⚠️ Nhưng (A) **đổi một hành vi M3 quan sát được**: `expires_at` của task đêm sẽ muộn hơn M3 tự
> tính. **Bắt buộc ghi vào `IR-06 §3.4.2`** và nêu trong `IR-07` khi bàn giao.

#### Đã làm gì

| File | Thay đổi |
| --- | --- |
| `src/Ivr.Domain/Policies/EligibilityRules.cs` | Thêm `CallingWindowClosedForWholeConfirmationWindow`, kèm doc-comment nêu rõ vì sao đây là từ chối chứ không phải lời giải |
| `src/Ivr.Infrastructure/Intake/TaskIntakeService.cs` | Thêm `CallingWindow` vào ctor · guard mới **sau** cổng contact · hàm `AnyAttemptFallsInsideCallingHours(window, policy)` |
| `tests/Ivr.UnitTests/Intake/TaskIntakeServiceTests.cs` | `3` test mới + `CreateContext(now)` nhận đồng hồ |
| `tests/Ivr.IntegrationTests/TaskIntakePersistenceTests.cs` | `4` call site nhận `CallingWindow` **bật thật** |
| `docs/traceability-tests.md` | **Sinh lại bằng `generate-test-traceability.mjs`** (`671` dòng) — `3` `TestId` mới phải có hàng, nếu không `FailGateTests` đỏ |

**Vì sao guard nằm sau cổng contact:** token hỏng là thứ M3 **phải sửa**; khung giờ đóng thì không.
Lỗi sửa được phải hiện ra trước.

**Test dùng `CallingWindow` bật thật, không tắt đi cho dễ:** cả hai fixture sẵn có đều ở
`2026-08-13T06:00:00Z` = **13:00 giờ VN**, nằm trong khung `08:00–21:08` — nên bật thật mà các test
cũ vẫn đúng nghĩa.

| Test | Kịch bản | Kỳ vọng |
| --- | --- | --- |
| `UT-INTAKE-NIGHT-01` | 24/7 COD, `T0` = `23:00` giờ VN, cửa sổ `900s`, hai mốc `+0s`/`+450s` đều ngoài giờ | `TASK_BLOCKED_OPERATIONAL` + reason mới, `ivr_call_job_id` = null |
| `UT-INTAKE-NIGHT-02` | Cùng task, `T0` = `13:00` giờ VN | Vẫn `TASK_ACCEPTED_DRY_RUN_ONLY` — guard không bắt nhầm |
| `UT-INTAKE-NIGHT-03` | `T0` = `20:55` giờ VN — mốc `2` rơi `21:02:30`, vẫn trong khung nhờ `W-0220` dời sang `21:08` | Chấp nhận — giữ đúng một lần gọi được là đủ |

#### ⚠️ Giới hạn phải nói trước

| # | Giới hạn |
| --- | --- |
| 1 | **e2e và sandbox không phủ guard này.** `docker-compose.e2e.yml:39-40` và `docker-compose.sandbox.yml:93-94` đều đặt `0..1440` (mở 24h), nên guard **nằm im** ở đó. Phủ bằng unit test, **không** phủ bằng chạy ngoài — trái bài học *"chạy từ ngoài"*, nhưng đổi cấu hình e2e sẽ phá `W-0214` |
| 2 | **Chưa giải quyết đơn đêm.** Đơn vẫn không được gọi; chỉ khác là M3 biết ngay và biết **đúng lý do** |
| 3 | **`IR-06 §3.4.2` chưa ghi** — `IR-06` đang nằm trong lượt `W-0297` mở. Đã chuyển sang lô `W-0303` (mục `11`) |
| 4 | **Chưa cấp `W-ID` trong tracker** — `prompt/_execution/prompt-execution-tracker.md` cũng đang trong `W-0297`. Cấp khi commit |

#### ✅ Đã commit — `92091a1`

⚠️ **Một cái bẫy phát hiện lúc commit, ghi lại cho các lô sau.** `27` file `plan/ivr-orther` bị xoá
của `W-0297` **đã nằm sẵn trong index** (`D ` — staged từ trước), nên `git add` năm đường dẫn của tôi
vẫn cho ra `32` file chờ commit. Luật *"không `git add -A`"* ở **§0** **không đủ** — index có thể đã
bẩn trước khi mình chạm vào.

```bash
# SAI khi index đã có thứ khác staged sẵn:
#   git add <paths> && git commit      → cuốn theo mọi thứ đang staged

# ĐÚNG — commit theo pathspec, KHÔNG đụng index của lượt kia:
git commit -m "…" -- src/Ivr.Domain/Policies/EligibilityRules.cs                      src/Ivr.Infrastructure/Intake/TaskIntakeService.cs                      tests/Ivr.UnitTests/Intake/TaskIntakeServiceTests.cs                      tests/Ivr.IntegrationTests/TaskIntakePersistenceTests.cs                      docs/traceability-tests.md

# Kiểm sau commit:
git show --stat HEAD                          # phải đúng 5 file
git status --porcelain | wc -l                # phải vẫn 97 (W-0297 nguyên vẹn)
git status --porcelain -- plan/ivr-orther/ | grep -c '^D '   # phải vẫn 27
```

**Kết quả kiểm:** commit `5` file · cây còn `97` · `27` staged-deletion còn nguyên · `src/`+`tests/`
sạch.

---

### ✅ `W-0299` — `B6`: `ProductionTargetV1FieldsApproved` trả về `NO` — **ĐÃ XONG `16/09`**

| | |
| --- | --- |
| **Mức** | 🟠 Cờ sai mặc định, ảnh hưởng hôm nay `0` cuộc gọi |
| **Ngày** | `0,5` |
| **Chặn bởi** | Không ai |
| **Trạng thái** | ✅ **XONG** — `1064/1064` test pass · mutation hai chiều đã chạy · **không đổi một dòng code runtime nào** |

Một cờ cho phép đọc **tên món + số lượng + vùng giao** trong lời thoại tới khách, mặc định `YES`
ngay trong file cấu hình gốc. Mặc định an toàn phải là `NO`.

> ### Đây là siết thật, không phải hình thức
> Quét `deploy/` và mọi `docker-compose*.yml`: **không môi trường nào** đặt
> `IVR_PRODUCTION_TARGET_V1_FIELDS_APPROVED`. Nên **trước** lượt này mọi môi trường chạy cờ **bật**;
> **sau** lượt này mọi môi trường chạy cờ **tắt** cho tới khi có người cố ý bật.

#### Ghim bằng test đọc file đã ship, không phải object đã bind

`UT-FAILGATE-TARGETV1-DEFAULT-01` trong `FailGateTests` đọc thẳng `4` file `appsettings` trong repo.
Khuyết tật nằm ở **thứ được ship**, nên chỉ đọc thứ được ship mới bắt được nó quay lại. Test cũng đỏ
nếu khoá bị **xoá hẳn**, không chỉ khi bị đổi giá trị.

| Mutation | Kết quả |
| --- | --- |
| Đặt lại `YES` ở một file | Test **ĐỎ** ✅ |
| Khôi phục `NO` | Test **XANH** ✅ |

Hai bộ test cũ phụ thuộc cờ (`FeatureFlagApiTests`, `ScriptContentTests`) **không phải sửa** — cả hai
tự dựng `ScriptContentOptions` riêng, không dựa mặc định của file.

**File chạm (`4` file cấu hình):**

```text
src/Ivr.Api/appsettings.json:18
src/Ivr.Api/appsettings.Development.json:12
src/Ivr.Worker/appsettings.json:13
src/Ivr.Worker/appsettings.Development.json:7
```

**Các bước:**

1. `gitnexus_impact({target:"ProductionAllows"})` — `ScriptContentContracts.cs:291-304` là nơi đọc cờ.
2. Đổi `4` file về `"NO"`.
3. Kiểm `11` call site đã biết còn đúng: `ScriptContentContracts` · `ScriptContentOptions` ·
   `ServiceCollectionExtensions.cs:76-82` (đường env đã hỗ trợ sẵn) · `InMemoryScriptRegistry` ·
   `PostgresScriptRegistry` · `AdminConfigReadService` · `ScriptLifecycleApiService` ·
   `AdminConfigContracts` · `ScriptLifecycleContracts`.
4. Sửa fixture hai bộ test phụ thuộc: `tests/Ivr.IntegrationTests/FeatureFlagApiTests.cs` ·
   `tests/Ivr.UnitTests/Scripts/ScriptContentTests.cs` — bật qua env trong test thay vì dựa mặc định.
5. Thêm assertion: **mặc định phải là `NO`** — để lần sau ai đổi thì test đỏ.

**Xong khi:** `dotnet test` xanh · có test ghim mặc định `NO` · bật được qua
`IVR_PRODUCTION_TARGET_V1_FIELDS_APPROVED` trên môi trường có duyệt.

---

### ✅ `W-0300` — `B3`: gỡ phê duyệt production khỏi migration schema — **ĐÃ XONG `16/09`**

| | |
| --- | --- |
| **Mức** | 🟠 Nợ hồ sơ, ảnh hưởng hôm nay `0` cuộc gọi |
| **Ngày** | `1` |
| **Chặn bởi** | Không ai *(chọn phương án `(b)`)* |
| **Trạng thái** | ✅ **XONG** — `1065/1065` test pass · commit `641d294` · mutation hai chiều đã chạy |

> ## ⚠️ PHƯƠNG ÁN BỊ CHÍNH DATABASE TỪ CHỐI — và cái nó dạy
>
> Kế hoạch ghi *"migration mới hạ `approved_for_production = FALSE`"*. Chạy thì:
>
> ```text
> Npgsql.PostgresException : P0001: attempt-policy versions are immutable; create a new version
> ```
>
> `trg_ivr_attempt_policies_immutable` (`P1_2_InitialTargetV1Persistence.cs:1104-1116`) là
> `BEFORE UPDATE … FOR EACH ROW` và **raise vô điều kiện, không phân biệt cột**. Đó là `W-0151`
> được cưỡng chế ở tầng schema.
>
> **Luật đó đúng, nên bản sửa phải tôn trọng chứ không lách:** xoá hai hàng seed thay vì sửa chúng —
> và đó mới đúng thứ `B3` yêu cầu (*"tách INSERT khỏi migration schema"*). Đã xác minh `DELETE` hợp
> lệ: chỉ có trigger `BEFORE UPDATE`, **không FK** trỏ vào bảng, chưa task nào được nhận theo version
> này, và `W0196.Down()` vốn đã xoá đúng hai hàng đó.
>
> **Catalogue C# cố ý không đụng.** `SignedProductionAttemptPolicies` giữ `OwnerApproved`, nhưng chỉ
> tới được qua in-memory registry — mà `AddIvrFoundation` **ném lỗi** nếu execution mode khác `MOCK`
> (`ServiceCollectionExtensions.cs:94-103`) và cả hai host production truyền mặc định `false`. Lật nó
> nữa sẽ xoá một bản ghi quyết định và viết lại `5` test mà **không đổi gì ở runtime**.

#### Guard viết dạng phủ định toàn bảng

`IT-DB-POLICY-UNSIGNED-01` khẳng định **không hàng nào** claim production approval, chứ không phải
*"hai hàng này đã biến mất"*. Tên version không phải điểm chính — điểm chính là **một phê duyệt
production không được tới bằng migration, dù tên gì**; seed lại dưới tên khác sẽ lọt một test nêu
đích danh tên cũ. Đọc DB **thật sau khi chạy hết migration**, vì cột đó mới là thứ
`PostgresAttemptPolicyRegistry` biến thành `AttemptPolicyApproval`.

| Mutation | Kết quả |
| --- | --- |
| Vô hiệu `Up()` | Test **ĐỎ** ✅ |
| Khôi phục `Up()` | Test **XANH** ✅ |

`20260905120000_W0196…cs:55-59` INSERT hai hàng `approved_for_production = TRUE` kèm
`allowed_execution_modes` gồm `PRODUCTION_REAL` và `retention_class = 'LEGAL_DECISION_PENDING'` —
chính hàng đó tự khai quyết định pháp lý chưa xong.

**⚠️ Ràng buộc cứng:** **KHÔNG sửa `W0196`** (đã áp dụng). Phải ra **migration mới**.

**Các bước:**

1. Migration mới, đặt tên theo quy ước đang dùng:
   `202609xxxxxxxx_W0300LowerUnsignedProductionAttemptPolicy.cs`
2. `UPDATE ivr_attempt_policies SET approved_for_production = FALSE WHERE policy_version =
   'gh-247-prod-v1'` — **không `DELETE`**, giữ hàng để `AttemptPolicyRegistries` vẫn phân giải được.
3. **Không ghi `ivr_audit_log`** (luật §1.5).
4. `Down()` phải khôi phục `TRUE` — migration phải đảo được.
5. Sửa fixture `UT-POLICY-SIGNED` cho khớp trạng thái mới.
6. Kiểm `AttemptPolicySnapshot.EnsureEnvironmentAllowed` vẫn từ chối đúng ở `PRODUCTION_REAL`.
7. Chạy `node deploy/ci/scripts/progressive-selftest.mjs` — gate cấm `DropTable` trong `Up`.

**Xong khi:** migration áp được trên DB đã có `W0196` · `dotnet test` xanh · `progressive-selftest`
xanh · truy vấn `approved_for_production` trả `false` cho cả hai program.

---

### ✅ `W-0301` — `B4`: `RUNTIME_GATE_ADMIN` phải nêu môi trường — **ĐÃ XONG `16/09`**

| | |
| --- | --- |
| **Mức** | 🟠 Bẫy schema — ghi `environment` lên hàng admin **trông như** scope mà không phải |
| **Ngày** | `0,5` |
| **Chặn bởi** | Không ai — quyết định kỹ thuật, đã chốt **SIẾT** |
| **Trạng thái** | ✅ **XONG** — `1066/1066` · commit `1651e8f` · ⚠️ `gitnexus_impact` = **HIGH**, đã báo owner trước khi sửa |
| **Ngày thực tế** | **1** (ước lượng ban đầu `0,5` — sai, đã sửa khi thấy blast radius) |

> ## ⚠️ HAI THỨ PHẢI GHI LẠI
>
> **1. Ước lượng `0,5` ngày sai.** `gitnexus_impact` trả **HIGH**: `20` điểm chạm, `4`
> implementation, `13` test. `IRuntimeGateAuthorization.IsApprovedAsync` **không có** tham số
> `environment` (khác `IProductionCallGate`), nên phải đổi chữ ký interface.
>
> **2. Trigger schema lại quyết định thiết kế — lần thứ hai trong ngày.**
> `trg_ivr_runtime_gate_approvals_append_only` cấm **cả** `UPDATE environment` **lẫn** `DELETE`:
>
> ```text
> OLD.environment IS DISTINCT FROM NEW.environment → 'only revocation may change'
> TG_OP = 'DELETE'                                  → 'append-only; revoke instead of deleting'
> ```
>
> ⇒ Hàng seed **chỉ revoke được**. Chọn phương án `A` (owner chốt): revoke, **không seed thay thế**.
>
> Và `CHECK` phải khác bản `PRODUCTION_CALL` một vế — **bắt buộc, không phải nới lỏng**:
>
> ```sql
> CHECK (approval_kind <> 'RUNTIME_GATE_ADMIN'
>        OR environment IS NOT NULL
>        OR revoked_at IS NOT NULL)
> ```
>
> Vì hàng vừa revoke giữ `environment = NULL` **vĩnh viễn** (trigger không cho đổi). Miễn trừ hàng
> đã revoke không mất gì — nó không mở môi trường nào dù cột ghi gì.

#### Tôi tự lật lập luận của chính mình, và viết lại doc-comment

Vị trí cũ trong `RuntimeGateApprovalKinds`: *"administration is coarse on purpose"*. **Đọc thì xuôi
và sai ở đúng một chỗ** — người duyệt điền `environment = 'lab'` sẽ tin mình đã giới hạn, thực tế
đã mở luôn production. **Một cột có người điền mà không ai đọc thì tệ hơn không có cột.**

Đã **viết lại doc-comment** thay vì để lại bẫy cho người đọc sau.

#### Hệ quả vận hành — nói rõ

Quản trị runtime-gate **đóng ở mọi môi trường** tới khi có hàng scoped. Nhưng
`FeatureFlagAdminService` vốn cho **giảm rủi ro vô điều kiện** mà không hỏi cổng này — nên **kill
switch vẫn bật được, vẫn dừng được cuộc gọi, không cần phê duyệt nào**.

| Test viết lại | Trước | Sau |
| --- | --- | --- |
| `IT-GATE-APPROVAL-01` | *"approved because the signature is a row"* | Không cấp ở môi trường nào — quét cả `5`, không nêu tên một cái |
| `IT-GATE-APPROVAL-03` | Revoke hàng seed | Cấp hàng scoped trước rồi revoke |
| `IT-GATE-APPROVAL-10` | Ghim *"column recorded, never read"* | Cột **được đọc**; `lab` không mở `prod` |
| `IT-GATE-APPROVAL-13` *(mới)* | — | DB từ chối phê duyệt admin **còn sống** không nêu môi trường |

| Mutation | Kết quả |
| --- | --- |
| Trả reader về `AnyLiveAsync` | `IT-GATE-APPROVAL-10` **ĐỎ** ✅ |
| Khôi phục | **XANH** ✅ |

**Bối cảnh cần ghi lại, vì tôi đang tự lật lập luận của chính mình:**
`RuntimeGateApprovalKinds.cs:48-55` hiện ghi *"Administration being coarse is **deliberate**"*, và
`IT-GATE-APPROVAL-10` ghim lập luận đó. Nhưng tôi vừa siết đúng lớp lỗi này cho `PRODUCTION_CALL`
(`20260915122953`). Một luật đúng cho `PRODUCTION_CALL` thì lập luận *"admin thô là cố ý"* yếu đi.

**Plumbing đã có sẵn:** `RuntimeGateApprovalReader.AnyLiveForEnvironmentAsync` đang phục vụ
`PRODUCTION_CALL` — chỉ cần đấu lại cho `RUNTIME_GATE_ADMIN`.

**Các bước:**

1. `gitnexus_impact({target:"RuntimeGateApprovalReader.AnyLiveAsync", direction:"upstream"})`.
2. Đổi `PostgresRuntimeGateAuthorization` sang `AnyLiveForEnvironmentAsync`.
3. Migration mới thêm `CHECK (approval_kind <> 'RUNTIME_GATE_ADMIN' OR environment IS NOT NULL)` —
   **đúng khuôn `20260915122953`**, và **cập nhật hàng seed `OD-V1-20` cho có `environment`** trong
   cùng migration, nếu không migration sẽ dừng deploy trên DB đã có hàng `NULL`.
4. **Cập nhật doc-comment `RuntimeGateApprovalKinds.cs:48-55`** — lập luận cũ giờ sai, để nguyên là
   để lại một cái bẫy cho người đọc sau.
5. Viết lại `IT-GATE-APPROVAL-10` cho khớp hành vi mới.
6. Kiểm `RollingDeploySchemaCompatibilityTests` — smoke `16/09` nêu nghi vấn migration `CHECK` lên
   cột có sẵn có nằm trong danh sách miễn trừ không. **Kiểm thật, đừng đoán.**

**Xong khi:** `dotnet test` xanh · `IT-GATE-APPROVAL-10` mô tả hành vi mới · migration áp được trên
DB có hàng `OD-V1-20` `environment=NULL` · doc-comment không còn nói ngược.

---

### ✅ `W-0302` — `C6`: token hết hạn muộn hơn window → `422` thay vì `500` — **ĐÃ XONG `16/09`**

| | |
| --- | --- |
| **Mức** | 🔴 M3 sẽ **retry mù vô hạn** ngay khi nối |
| **Ngày** | `1,5` |
| **Chặn bởi** | Không ai *(nửa `DTK-02`/`DTK-06` với M3 vẫn chặn)* |
| **Trạng thái** | ✅ **XONG** — `1105/1105` · commit `1651e8f`→`W-0302` · `oasdiff` **không breaking** · `9` gate PASS |

> ### ⚠️ HAI BÀI HỌC MỚI
>
> **1. Baseline so sánh CI phải giữ ở bản trước.** Dời `draft.27`→`28` thì `oasdiff` so spec với
> chính nó và lượt sau **không còn gì để so**. Bảng `api-changelog.md` đọc
> `Baseline draft.27 | Current draft.28`; xoay baseline là lượt `chore` riêng theo khuôn `W-0202`.
>
> **2. `generate-test-traceability.mjs` đọc CÂY LÀM VIỆC, không đọc `HEAD`** — khuyết tật tôi tự gây
> ra ở `W-0301`. Chạy nó lúc phiên khác giữ test chưa track làm `1651e8f` mang `16` hàng `UT-TRUNK`
> trỏ tới file `HEAD` **không có**; `FailGateTests` sẽ đỏ trên checkout sạch. Phiên `PD-01` vá ở
> `a2808ce`.
>
> Luật cũ *"sinh chứ không gõ tay"* **chưa đủ**:
> **sinh lại file dẫn xuất chỉ an toàn khi cây nó đọc khớp cây sắp commit** — chạy `--check` trước
> commit, không chỉ `--write`.

#### Ràng buộc nằm ở `description`, không ở schema

JSON Schema **không so được hai giá trị anh em**. Đây là luật runtime, nên `oasdiff` báo
*"No changes to report, but the specs are different"* — đúng, không có thay đổi cấu trúc nào.

#### Giữ hai reason code riêng

`DIAL_TOKEN_EXPIRES_BEFORE_WINDOW` và `DIAL_TOKEN_EXPIRES_AFTER_WINDOW` thay vì một
`DIAL_TOKEN_EXPIRY_MISMATCH` — để câu trả lời nêu **chiều nào** producer sai, khỏi phải tự đi so hai
mốc. Guard ở persistence **giữ nguyên**: lớp cuối chứ không phải lớp đầu.

| Mutation | Kết quả |
| --- | --- |
| Gỡ guard mới | `UT-INTAKE-REASON-TAXONOMY-13` **ĐỎ** ✅ |
| Khôi phục | **XANH** ✅ |

**Đường đi tới `500`, đã xác minh đủ bốn tầng:**

| Tầng | Hành vi hiện tại |
| --- | --- |
| `TaskIntakeService.cs:418` | token hết hạn **sớm hơn** window → từ chối, có reason code ✅ |
| `PersistenceInvariantValidator.cs:120-124` | token hết hạn **muộn hơn** window → `throw InvalidOperationException` ❌ |
| `ErrorEnvelopeMiddleware.cs:38-47` | `catch (Exception)` → `IvrErrors.InternalError()` |
| `IvrErrorResponseWriter.cs:69` | `IVR_INTERNAL_ERROR` → **HTTP `500`** |

**Các bước:**

1. `gitnexus_impact` trên `PersistenceInvariantValidator` và điểm kiểm contact trong intake.
2. Đưa kiểm `dial_token_expires_at > window.ExpiresAt` **lên intake**, trả `422` kèm reason code
   mới trong `EligibilityReasonCodes` (đặt cạnh `DialTokenExpiresBeforeWindow`).
3. **Giữ nguyên guard ở persistence** — nó là lớp chặn cuối, không gỡ. Chỉ để nó không còn là nơi
   lỗi đầu tiên chạm tới.
4. Mô tả ràng buộc **bằng nhau** trong OAS: `dial_token_expires_at` == `confirmation_window_expires_at`.
5. **Phát hành contract — đây là phần nặng:**

   | Việc | Nơi |
   | --- | --- |
   | Bump version | `specs/api/openapi/ivr-order-confirmation.v1.yaml:4` → `draft.28` |
   | Re-pin hash | `specs/api/openapi/contract-manifest.json` (đang ghim `7739c425…`) |
   | Re-pin | `deploy/ci/scripts/contract-freeze-verifier.mjs` |
   | Re-pin | `deploy/ci/scripts/openapi-contract-drift.mjs` |
   | Re-pin | `deploy/ci/scripts/dial-token-production-bundle-validator.mjs` |
   | Re-pin | `docs/api/portal-manifest.json` + `build-api-docs.mjs` |
   | Changelog | `docs/api/changelog/…draft.27-to-draft.28.md` |
   | Regenerate client | `IvrServerModels.g.cs` |
   | Baseline | `…draft.28.yaml` |

6. Test: `IT-INTAKE-DTK-05` — token muộn hơn window ⇒ **`422` có reason**, không phải `500`.
7. Test: payload hợp lệ vẫn `200` (chống siết nhầm).

> ⚠️ **Cái bẫy của lô này:** sửa OAS mà quên re-pin một trong `5` nơi ⇒ gate đỏ ở lượt sau và
> **không ai biết tại sao** — đúng bài học `W-0216`. Re-pin **trong cùng commit**.

**Xong khi:** `node deploy/ci/scripts/openapi-contract-drift.mjs` xanh · `contract-freeze-verifier`
xanh · `oasdiff` xác nhận thay đổi là **non-breaking** (siết mô tả, không siết schema) · `dotnet
test` xanh.

---

### `W-0304` — Lô tài liệu (`11` mục) — ✅ **XONG** `16/09`

| | |
| --- | --- |
| **Mức** | 🟡 Mâu thuẫn tài liệu — người đọc sau làm sai theo |
| **Ngày** | `1,5` |
| **Chặn bởi** | Không ai — nhưng ⚠️ **`6/10` mục chạm `IR-06` và register, hai file đang nằm trong `W-0297`**. Chọn cách `A`/`B`/`C` ở **§0** trước khi bắt đầu |

| # | Việc | File | Chồng `W-0297`? |
| --- | --- | --- | --- |
| 1 | Gỡ *"Chưa chốt (`W-0215`)… `End ≥ 21:07:30`"* — mâu thuẫn `:325-326` đã ghi `21:08` | `integration-requirements/06-module-3-api-handover.md:354-356` | ⚠️ **CÓ** |
| 2 | Bỏ `customer_display_name` khỏi `allowed_input_fields` — trái `OD-V1-19` | `specs/ui/04-ivr-menu-config.md:17` | ✅ không |
| 3 | Sửa dòng `IVR_OPT_OUT → IVR_POLICY_BLOCKED` cho khớp `IR-07 A-13` *(V1 không có opt-out)* | `m8-05 §3.1` — **nay nằm trong `plan/ivr-orther/00-*.md` sau `W-0297`** | ⚠️ **CÓ** — file đích vừa bị `W-0297` dồn |
| 4 | Thêm: *"`priority` không có trên wire; scheduler xếp theo `expires_at → program → offset → risk_flags`"* | `IR-06 §3.5` | ⚠️ **CÓ** |
| 5 | Đồng bộ với đặc tả fence thu hồi | `IR-06 §4.8` | ⚠️ **CÓ** |
| 6 | Bảng map `result_type` → `cancellation_reason_code` *(gộp `B10`+`C7`+`C8`)* | `IR-06 §4.3` | ⚠️ **CÓ** |
| 7 | Gỡ mâu thuẫn vị trí `E.164` — một nguồn sự thật duy nhất | `IR-06 §6` + `specs/api/04-sim-adapter-contract.md` | ⚠️ **CÓ** (nửa `IR-06`) |
| 8 | Đổi nhãn dòng chạm M3: `CLOSED` → `M8_POSITION_SIGNED / M3_NOT_RECEIVED` *(`OD-V1-01/02/03/05`)* | `specs/_review/open-decisions-register.md` | ⚠️ **CÓ** |
| 9 | Xoá hoặc đánh dấu `DEAD_BY_OD-V1-23` — `0` caller runtime | `src/Ivr.Domain/Policies/OptOutSuppression.cs` | ✅ không |
| 10 | **Cập nhật mọi dẫn chiếu tới file `W-0297` vừa xoá** (`m8-05`, `m8-16`, `m8-17`, `od-v1-signoff`) sang `00-CHUA-XONG.md` / `00-DA-XONG.md` | `PHAN-HOI-…md` + evidence README liên quan | ⚠️ **CÓ** — chính là hệ quả của `W-0297` |
| 11 | **[Mới — từ `W-0298`]** Ghi `§3.4.2`: task mà mọi lần gọi rơi ngoài `08:00–21:08` bị trả `TASK_BLOCKED_OPERATIONAL` + `CALLING_WINDOW_CLOSED_FOR_WHOLE_CONFIRMATION_WINDOW` tại intake — M3 phải biết để xử đơn đêm | `IR-06 §3.4.2` | ⚠️ **CÓ** |

⇒ **`8/11` mục chồng `W-0297`, `3/11` làm ngay được** (`2`, `9`, và nửa không-`IR-06` của `7`).

**Bảng map ở mục 6 — đề xuất của tôi, M3 chốt sau:**

| M8 phát (`result_type`) | M3 dùng (`cancellation_reason_code`) |
| --- | --- |
| `IVR_NO_ANSWER_FINAL` | `IVR_NO_ANSWER_MAX` |
| `IVR_CUSTOMER_CANCELLED` | `IVR_DECLINED` |
| `IVR_INVALID_PHONE_FINAL` | `IVR_INVALID_NUMBER` |
| `IVR_CONFIRMATION_WINDOW_EXPIRED` | `IVR_CONFIRMATION_EXPIRED` |
| `IVR_CAPACITY_EXCEPTION` | **M3 chốt** — lỗi hệ thống, `fault=NONE`, **không đếm trả trước** |

⚠️ Mục `9` chạm code ⇒ phải `gitnexus_impact` trước, dù `0` caller.
⚠️ Mục `1`,`4`,`5`,`6`,`7` chạm `IR-06` — file **được ghim hash bởi `4` validator**. **Re-pin cùng
lượt** (luật §1.6), quét lại toàn bộ như `W-0216` đã làm (`32` khớp / `0` lệch).

**Xong khi:** `node deploy/ci/scripts/docs-selftest.mjs` xanh · quét pin `0` lệch · `0` dẫn chiếu
chết tới file `W-0297` đã xoá.

---

### `W-0306` — Lô vận hành (`4` mục, từ smoke test `16/09`) — ✅ **XONG** `16/09`

| | |
| --- | --- |
| **Mức** | 🟡 Nợ hạ tầng — không chặn nhưng làm môi trường mới khó dựng |
| **Ngày** | `1,5` |
| **Chặn bởi** | Không ai |

| # | Việc | Vì sao |
| --- | --- | --- |
| 1 | Dọn `Error: libgssapi_krb5.so.2: cannot open shared object file` trong ảnh migrate | Không chặn (`5/5` migration áp được) nhưng một dòng `Error` trong ảnh chiseled là nợ phải dọn |
| 2 | Đưa `deploy/docker/dev-seed/seed.sql` vào **đường chuẩn** của compose | Hiện policy mock (`mock-lab-v1`, `mock-e2e-*`) chỉ được áp bởi `image-selftest.mjs`; **môi trường mới không tự chạy e2e được**, phải seed tay |
| 3 | Hạ mức log EF của worker khỏi `Information` | `46k` dòng / `10` phút làm log **không đọc được**; không lộ PII nhưng che mất tín hiệu thật |
| 4 | **`B5`** — thêm danh sách môi trường đã chạy `W0122` bản drop vào runbook | `deploy/ci/rollback.md` — `2/3` việc của `B5` **đã xong từ trước**, đây là phần còn thiếu duy nhất |

> **Mục `2` đúng bài học *"chạy từ ngoài"*:** `dotnet test` xanh **không** chứng minh môi trường mới
> dựng lên chạy được. Smoke `16/09` bắt đúng chỗ đó.

**Xong khi:** dựng stack sạch từ `docker compose up` **không seed tay** mà e2e `3` kịch bản chạy
được · log migrate không còn dòng `Error` · worker log đọc được.

---

### `W-0307` — `B9` nửa `audit-evidence` *(tuỳ chọn, `2` ngày)* — ✅ **XONG** `16/09`

Chỉ làm khi `W-0298`→`W-0304` xong sớm.

`GET /v1/ivr/order-confirmation/admin/audit-evidence` — đọc bảng audit append-only theo
`object_type`/`object_id`, masked qua `PiiMaskingFilter`, permission riêng, **read-only tuyệt đối**.

**Quyết định phạm vi của tôi:** làm `audit-evidence` (M8 cần cho chính mình để truy vết),
**không** làm `capacity-incidents` — `AdminReadService.cs:188-216` đã có đường đọc, nhưng phơi ra
endpoint thì phải biết M3 lọc/phân trang thế nào. ⛔ Chờ M3.

---

## §4 — LỊCH THEO NGÀY

| Ngày | `W-ID` | Việc | Ngày công |
| --- | --- | --- | ---: |
| **1** | `W-0298` · `W-0299` | `C15` đơn ngoài giờ · `B6` cờ production về `NO` | `1,5` |
| **2** | `W-0300` · `W-0301` | `B3` hạ `approved_for_production` · `B4` scope environment | `1,5` |
| **3–4** | `W-0302` | `C6` `500`→`422` + phát hành OAS `draft.28` | `1,5` |
| **5** | `W-0303` | Lô tài liệu `10` mục + re-pin — ⚠️ `2` file chồng `W-0297` (§0) | `1,5` |
| **6** | `W-0304` | Lô vận hành `4` mục | `1,5` |
| | | **TỔNG** | **`7,5`** |
| *[7–8]* | *`W-0305`* | *`B9` audit-evidence (tuỳ chọn)* | *`2`* |

---

## §5 — KIỂM SAU MỖI LÔ (không lô nào đóng mà thiếu bước này)

```bash
# 1. Phạm vi thay đổi có đúng dự kiến không
gitnexus_detect_changes()

# 2. Test đầy đủ — baseline hiện tại 1060/1060
dotnet test Ivr.sln

# 3. Gate liên quan tới lô
node deploy/ci/scripts/progressive-selftest.mjs      # lô migration (W-0300, W-0301)
node deploy/ci/scripts/openapi-contract-drift.mjs    # lô contract  (W-0302)
node deploy/ci/scripts/contract-freeze-verifier.mjs  # lô contract  (W-0302)
node deploy/ci/scripts/docs-selftest.mjs             # lô tài liệu  (W-0303)

# 4. Bảng điều khiển — SINH, không gõ tay
node deploy/ci/scripts/gate-status.mjs --write

# 5. Sau commit: phần W-0297 phải CÒN NGUYÊN, không bị nuốt
git status --porcelain | wc -l      # vẫn ~97 nếu W-0297 chưa commit
```

**Và ít nhất một lần trong tuần — chạy từ ngoài:** dựng stack sạch trên `192.168.1.61`, chạy e2e
`3` kịch bản **không seed tay**, xác nhận callback không chứa số điện thoại hay dial token.

---

## §6 — VIỆC KHÔNG LÊN LỊCH ĐƯỢC (chờ bên khác)

Liệt kê để theo dõi, **không tính vào `7,5` ngày**.

### ⛔ Chờ phần đơn hàng Module 3 — `14` mục

`C1` `C3` `C4` `C5` `C9` `C10` `C11` `C13` `C14` `C16` `C17` + nửa `B9` + nửa `C6` (`DTK-02/06`) +
nửa `C15` (phương án B)

> **Phía tôi sẵn sàng bàn giao ngay:** `IR-07` (`407` dòng, mỗi câu có sẵn phương án, im lặng =
> đồng ý) · `IR-06` · `IR-08` + `docker-compose.sandbox.yml` chạy được.
>
> **Sau ngày M3 đối ký: `≈4` ngày để đóng cả `14` mục.**

**Việc cần xin:** ngày dự kiến phần đơn hàng M3 xong → lùi ngược `4` ngày = mốc đóng nhóm C.

### ⏳ Chờ báo giá nhà mạng — `2` mục

`B12` (adapter production) · `B1` (calibrate `4` số năng lực)

```text
Báo giá Vinaphone / MobiFone / Viettel
   └─> chốt gói + số kênh
          └─> có trunk thật
                 └─> adapter production (B12)  ≈2–3 tuần
                        └─> W-0008 cuộc gọi đo được
                               └─> calibrate 4 số (B1)  ≈1 ngày
```

✅ Hướng **đã chốt: Mobile SIP trunk**. Đã liên hệ đủ ba nhà mạng.

### 📋 Chờ xác nhận cấp công ty — `1` cụm

Rủi ro pháp lý V1: ghi âm cuộc gọi · thời hạn lưu · danh sách không-gọi-lại · nội dung lời thoại đọc
tên món và vùng giao. Gộp `A1`(1) + `B6`(nửa) + `C12`. **Một câu xác nhận là đủ cho cả cụm.**

### 📋 Chờ chief auditor — `3` mục

`A1` (tách trạng thái register) · `B10` (chốt tên mã) · `C7`/`C8` (từ vựng result code — tôi đã đề
xuất sẵn bảng map ở `W-0303` mục `6`).

### 🟠 Của tôi nhưng không phải việc gõ code — `B8` TTS

`8` hạng mục còn lại đều là quyết định và thao tác của tôi: nghe và chốt `3` giọng · duyệt `12` đoạn
cố định · `6` cuộc MicroSIP · retention/rollback drill · licence model · mirror · đo target hardware
· chốt media topology. **Tốn thời gian ngồi nghe, không tốn ngày công lập trình** — nên không nằm
trong lịch `§4`.

> **Cập nhật `18/09` (`W-0315`):** VieNeu-TTS tự host là bộ đọc duy nhất, production lẫn lab
> (`OD-V1-19`, `S4` ngày `17/09`). Mirror weights, `16` CVE của base image `ivr-tts` và licence model
> vẫn là cổng production — xem Lô `4` của
> [vướng mắc `17/09`](ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md).

---

## §7 — BẢNG THEO DÕI

| `W-ID` | Việc | Ngày | Trạng thái | Commit |
| --- | --- | ---: | --- | --- |

| `W-0298` | `C15` đơn ngoài khung giờ | `1` → **0,5** | ✅ **XONG** `16/09` · `1063/1063` | **`92091a1`** — 5 file, `W-0297` còn nguyên 97 |
| `W-0299` | `B6` cờ production về `NO` | `0,5` | ✅ **XONG** `16/09` · `1064/1064` | |
| `W-0300` | `B3` gỡ phê duyệt khỏi migration | `1` | ✅ **XONG** `16/09` · `1065/1065` | `641d294` |
| `W-0301` | `B4` `RUNTIME_GATE_ADMIN` scope env | `0,5` → **1** | ✅ **XONG** `16/09` · `1066/1066` | `1651e8f` |
| `W-0302` | `C6` `500`→`422` + OAS `draft.28` | `1,5` | ✅ **XONG** `16/09` · `1105/1105` | 5 pin dời, không breaking |
| `W-0304` | Lô tài liệu `11` mục | `1,5` | ✅ **XONG** `16/09` · `756/756` unit · **`13/13` gate ghim hash** | **`9dc5479`** + **`aa2c9af`** — `10/11` mục làm, `1` mục tiền đề sai; **sửa `11` gate đỏ do `W-0297`** |
| `W-0306` | Lô vận hành `4` mục | `1,5` → **`0,5`** | ✅ **XONG** `16/09` · `760/760` · kiểm trên stack sạch | `3` file cấu hình + `1` `.cs` + `1` compose service; `4/4` mục |
| `W-0307` | `B9` `audit-evidence` *(tuỳ chọn)* | `2` → **`1`** | ✅ **XONG** `16/09` · contract `draft.29` | endpoint mới + `11` test + `8` pin; **`3` lỗi thật do máy bắt** |


> ### ⚠️ `W-0304` tìm ra thứ không nằm trong kế hoạch: `W-0297` đã làm `11` CI gate đỏ
>
> Mục `10` của lô — *“cập nhật mọi dẫn chiếu tới file `W-0297` vừa xóa”* — được viết như một
> việc sửa link. Nó không phải. `W-0297` xóa `27` file, trong đó **`12` file là đầu vào CI
> được ghim hash**, và sửa `15` file ghim hash khác mà không re-pin cái nào. `164 KB` decision
> pack đã ký bị xóa; `19 KB` tóm tắt thay vào không chứa nội dung đó.
>
> `11` gate đỏ suốt một ngày mà **không lượt nào thấy**, kể cả `W-0298`…`W-0302` của chính
> tôi — vì mỗi lượt chỉ chạy gate thuộc phạm vi mình. Trong `11` gate đó, **`3` cái đỏ vì
> lỗi của tôi ở `W-0302`**: pin OAS tính trên bản CRLF của cây làm việc Windows, trong khi git
> lưu LF — pin xanh trên máy tôi và đỏ trên CI. Đúng bài học `W-0126`, lặp lại.
>
> **Bổ sung luật §1.11 — cụ thể hơn “chạy gate trước khi commit”:** khi một lượt **xóa hoặc
> đổi tên** file, thứ phải chạy **không phải** gate thuộc phạm vi mình mà là **quét toàn bộ
> pin** — vì pin là thứ duy nhất biết rằng một file ở chỗ khác đang phụ thuộc vào file bạn
> vừa xóa. Và tính hash **trên byte LF**, không trên cây làm việc Windows.


> ### ✅ `W-0306` — bốn mục, và cả bốn được chứng minh bằng một stack thật
>
> Đây là lô duy nhất trong kế hoạch mà **`dotnet test` không chứng minh được gì** — cả `4` mục
> đều đến từ smoke `16/09`, tức từ việc dựng thật rồi nhìn. Nên cách đóng cũng phải thế: dựng
> project `w0306` trên volume mới, **không seed tay một lệnh nào**, rồi đo.
>
> | Mục | Trước | Sau |
> | --- | --- | --- |
> | `1` `krb5` | `2` dòng `Error` đầu log migrate | `0` |
> | `2` seed | `0` hàng policy, task nhận rồi không bao giờ gọi | `6/6` hàng, tự động |
> | `3` log EF | `113` dòng/`120`s | **`8`** dòng/`120`s |
> | `4` `B5` | runbook không trả lời được *"DB của tôi thuộc diện nào"* | một câu SQL, đã chạy thử |
>
> **Một chỗ tôi cố ý làm khác chữ kế hoạch.** Mục `4` đòi *"thêm **danh sách môi trường** đã chạy
> `W0122` bản drop"*. Tôi không chép danh sách, vì không có cluster thật, CI luôn dựng DB rỗng, còn
> stack local thì trạng thái phụ thuộc **ai đó có `down -v` hay không** — việc không được ghi ở đâu
> cả. Một danh sách chép tay sẽ sai ngay lần sau có người dựng thêm stack, và sai theo kiểu **trông
> như một câu trả lời**. Runbook vì vậy đưa câu SQL trả lời dứt khoát, cộng bảng môi trường ghi
> thẳng *"có thể"* ở hai dòng không biết — thay vì đoán cho đủ ô.
>
> **Nợ mới phát hiện, chưa làm:** `image-selftest.mjs` cố định cổng `55433`/`58080`, nên nó **không
> chạy được** khi máy đang có một stack dev bật. Lần đầu nó đỏ chính vì thế chứ không phải do
> compose vừa sửa — đáng ghi vì đây đúng chỗ dễ đổ lỗi nhầm cho thay đổi của mình.


> ### ✅ `W-0307` — và ba chỗ chính kế hoạch này nói sai
>
> Kế hoạch mô tả lô cuối bằng bốn dòng. **Ba trong bốn sai** khi đối chiếu mã — và đây là kế
> hoạch do **chính tôi** viết, nên ghi ra đây theo đúng chuẩn tôi đang áp cho bản `16/09`:
>
> | Kế hoạch viết | Thực tế |
> | --- | --- |
> | `object_type`/`object_id` | cột là `TargetType`/`TargetId` |
> | *"masked qua `PiiMaskingFilter`"* | filter **không mask**, nó **từ chối** cả response. Dựa vào nó để che thì đúng dòng cần nhất sẽ trả `500` |
> | *"permission riêng"* | `DF-01` **LOCKED `7` quyền, Permission Core sở hữu** — M8 không ký được |
>
> **Việc không làm, được gọi tên:** không tự cấp quyền thứ tám. `OD-V1-20` đã phải mở hẳn một
> quyết định chỉ để thêm một quyền. Tự thêm ở đây là M8 ký vào sổ của người khác — đúng thứ
> bản đánh giá `16/09` của tôi đang bắt bẻ ở chỗ khác.
>
> **Ba lỗi thật, cả ba do máy bắt:** (1) ngoại lệ `PiiGuard` không được dịch ⇒ `500` cho một
> request không bao giờ hợp lệ — **đúng khiếm khuyết `W-0302` vừa sửa**, suýt ship lại;
> (2) runtime trả `accessAuditId` trong khi contract khai `access_audit_id`;
> (3) `truncated` đo sai.
>
> Điểm đáng nhớ nhất là `(2)`: **`11` test tôi tự viết đều xanh với bản code sai**, vì chúng
> deserialize vào chính record đó — một vòng round-trip luôn tự khớp với chính nó. `IT-API-MATRIX-38`
> bắt được vì nó đọc response theo **file OpenAPI**, một hiện vật độc lập với code. Test viết cùng
> lúc với code chia chung mọi giả định sai của code đó.

---

## §8 — RỦI RO ĐÃ BIẾT

| # | Rủi ro | Lô | Chặn bằng |
| --- | --- | --- | --- |
| 1 | Dời cửa sổ mà quên dời `dial_token_expires_at` ⇒ `PersistenceInvariantValidator` ném lỗi | `W-0298` | Bất biến `W-0246` ghi thành test trước khi sửa |
| 2 | Sửa OAS mà quên re-pin `1` trong `5` nơi ⇒ gate đỏ lượt sau, không ai biết tại sao | `W-0302` | Re-pin **trong cùng commit** + quét lại toàn bộ như `W-0216` |
| 3 | Migration `CHECK` lên cột có sẵn làm **dừng deploy** nếu DB có hàng không scope | `W-0301` | Cập nhật hàng seed **trong cùng migration**; kiểm `RollingDeploySchemaCompatibilityTests` thật |
| 4 | `git add -A` nuốt trọn `97` file `W-0297` vào commit code | mọi lô | **Chỉ add theo đường dẫn** (§0) + `gitnexus_detect_changes` trước commit |
| 5 | `C15` phương án (A) đổi hành vi M3 quan sát được (`expires_at` muộn hơn) | `W-0298` | Ghi `IR-06 §3.4.2`, nêu trong `IR-07` khi bàn giao |
| 6 | Sửa `IR-06` (`4` validator ghim hash) mà quên re-pin | `W-0303` | Quét pin cuối lô, mục tiêu `0` lệch |
| 7 | `W-0303` sửa `IR-06`/register trong khi `W-0297` cũng đang sửa hai file đó | `W-0303` | Chọn cách `A`/`B`/`C` ở §0 **trước** ngày `5` |
| 8 | **Thêm `[Trait("TestId", …)]` mới mà quên sinh lại bảng truy vết ⇒ `FailGateTests` đỏ** — đã dính ở `W-0298` | mọi lô có test mới | `node deploy/ci/scripts/generate-test-traceability.mjs` trước khi chạy suite |
| 9 | **Đọc kế hoạch rồi code thẳng mà không đọc code trước** — phương án `(A)` của `W-0298` sai vì `T0` tới trên wire | mọi lô | Đọc luồng thật trước; kế hoạch là giả thuyết, code mới là sự thật |

---

**Nguyễn Quốc Toàn** · Dev trực tiếp Module 8 — IVR Order Confirmation
`16/09/2026` · `main@37606bc` · **`1063/1063` test pass** sau `W-0298` (baseline `1060` + `3`)
