# W-0312 — `draft.31`: Module 3 gửi số là nhận được

**Ngày:** `2026-09-17` · **Baseline:** `main@fe3bb19` · **Nguồn:** Lô 1 của
[`plan/ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md`](../../../plan/ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md)
(`T1` `T3` `T5`, Toàn duyệt `17/09`)

`REAL_CUSTOMER_CALL_ALLOWED=NO`

---

## 0. Vì sao lô này đi đầu

`IR-07` **đã gửi** Module 3, và câu `A-5` hứa: *gửi số **hoặc** cặp token*. Đọc code thì lời hứa đó
**không chạy được**, và hỏng ở nhiều tầng hơn ta tưởng:

| # | Chỗ hỏng ở `draft.30` | Hậu quả |
| --- | --- | --- |
| 1 | OAS, endpoint và DB vẫn bắt buộc `dial_token` + `dial_token_expires_at` | Body chỉ có số bị `400` |
| 2 | Intake **luôn** mã hoá token. Ngoài MOCK, bộ mã hoá là `Unavailable`, và phương án `B` đã bỏ kho khoá | Ở production **không task nào được nhận** — kể cả task gửi cả số lẫn token |
| 3 | Sổ đếm lần quay gắn một tham chiếu với **task đầu tiên** dùng nó | Nếu mọi task chỉ-gửi-số dùng **chung** một giá trị thay thế, từ task thứ hai trở đi bị từ chối |
| 4 | `phone_e164` có pattern trong contract nhưng runtime không kiểm | Số sai định dạng được nhận, rồi hỏng lúc quay |
| 5 | Ba summary dev-tooling ghi quyền `IVR_DEV_TOOLING` | Quyền đó **không tồn tại**; thứ runtime đòi là scope `ivr.admin.write` |

Mục `2` và `3` không nằm trong bản kế hoạch `W-0311`; chúng lộ ra khi đọc lại luồng intake để lập Lô 1.
Mục `3` là loại lỗi **không test đơn vị nào thấy**: nó chỉ hiện ra ở task **thứ hai**, trên sổ đếm thật.

---

## 1. Cảnh báo GitNexus, và thiết kế đi vòng

`gitnexus_impact` (upstream) trên các symbol bị sửa, chạy lại ngày `17/09`:

| Symbol | Risk | Ký hiệu · trực tiếp · luồng |
| --- | --- | --- |
| `TargetV1TaskMapper.CreateDomainSnapshot` | 🔴 **CRITICAL** | `26` · `2` · `5` |
| `TaskIntakeService.EvaluateAsync` | 🟠 **HIGH** | `23` · `1` · `3` |
| `TaskIntakeService.ContactRejectionReason` | 🟠 **HIGH** | `23` · `1` · `4` |
| `TaskIntakeEndpoint.ValidateSchema` | LOW | `1` · `1` · `1` |

Cả năm luồng GitNexus liệt kê đều thuộc **phía nhận task**: `HandleAsync` của intake, hai
`LoadSeedAsync` của dev-tooling, `EvaluateAsync`, `ToDomainAsync`. Không có luồng quay số nào trong danh
sách.

Theo luật `12` và `13` của kế hoạch: **không đổi chữ ký** của `CreateDomainSnapshot`,
`DialTokenReference`, `ConfirmationTaskSnapshot`, `IDialTokenResolver`. Chỉ đầu vào **hôm nay đang
hỏng** đổi hành vi:

| Đầu vào | `draft.30` | `draft.31` |
| --- | --- | --- |
| Chỉ token | Nhận ở MOCK/LAB · từ chối ở production | **Không đổi** |
| Token + số, có bộ mã hoá (MOCK/LAB) | Nhận, lưu token đã mã hoá | **Không đổi** |
| Token + số, bộ mã hoá `Unavailable` (production) | Từ chối `DIAL_TOKEN_PROTECTION_UNAVAILABLE` | **Nhận** — lưu tham chiếu riêng của task, quay bằng số |
| Chỉ số | `400` | **Nhận** — lưu tham chiếu riêng của task, quay bằng số |
| Không số, không token · nửa cặp token | `400` | **Không đổi** — nửa cặp **kể cả khi có số** vẫn `400` |
| Số sai pattern | Nhận, hỏng lúc quay | **`400`** |

**Token đi kèm số vẫn bị kiểm như token.** Đã gửi thì phải đủ cặp và `dial_token_expires_at` phải bằng
window end (`W-0302`). Đó là đầu vào **đang chạy được**, nên luật `13` giữ nguyên nó; `UT-INTAKE-NUMBER-04`
ghim điều này.

### Tham chiếu riêng của task

`enc:direct:` + SHA-256 (hex viết hoa) của `task_id`, hết hạn đúng `confirmation_window_expires_at`.
Một lớp `DirectDialReference` duy nhất sinh ra nó, dùng ở **cả hai** chỗ cần: mapper và intake.

- **Duy nhất theo task** — nên sổ đếm không bao giờ gắn hai task vào một giá trị (mục `3` ở trên).
- **Không bị bộ chặn số trần bắt nhầm** — dãy chữ số nằm giữa ký tự hex không khớp lookaround của
  guard (`src/Ivr.Domain/Confirmation/Identifiers.cs:106-110`). `UT-INTAKE-NUMBER-06` thử `10.000`
  `task_id`, gồm cả `task_id` **toàn chữ số** dài `10`.
- Thoả `ck_ivr_confirmation_tasks_token_ttl` và luật `W-0302` mà **không đổi schema DB**.

---

## 2. Contract `draft.30` → `draft.31`

```
### POST /tasks
-  added `subschema #1, subschema #2` to the request body `anyOf` list
-  the request property `dial_token` became optional
-  the request property `dial_token_expires_at` became optional
```

`oasdiff` image ghim `v1.26.1`, chạy như CI (`--fail-on WARN`): `30→31` và `27→current` đều **`exit 0`**.

### Chọn cách viết luật theo thứ CI chấp nhận, sau khi chứng minh hai cách tương đương

| Cách viết | `oasdiff` (`--fail-on WARN`) |
| --- | --- |
| `anyOf` lồng trong `allOf` + `dependentRequired` cho cặp token | **`3` ERROR** — CI đỏ |
| **Một `anyOf`** ở mức schema, nhánh số mang `not` cho hai field token | `0` error · `0` warning · `3` info |

Chạy cả hai qua Ajv 2020 trên `11` hình dạng body — chỉ token · chỉ số · số + đủ cặp · số + nửa cặp ×2 ·
nửa cặp ×2 · không gì cả · số sai ×3 — thì **kết quả giống hệt nhau** ở cả `11`. Chọn cách thứ hai vì nó
giữ nguyên nghĩa **và** không đòi nới gate. Cặp token nằm **trong cùng** nhánh với số, nên *"số bên cạnh
nửa cặp"* không khớp nhánh nào.

### Pattern ở runtime sinh từ chính model, không chép tay

`TaskIntakeEndpoint` đọc mọi `[RegularExpression]` trên `IvrConfirmationTaskV1` (model NSwag sinh từ
OAS) rồi kiểm từng field. Hai chi tiết:

- JSON Schema `pattern` theo ECMAScript: `$` chỉ khớp **cuối chuỗi**. Ở .NET, `$` còn khớp **trước
  một `\n` cuối** — nên số kèm xuống dòng sẽ lọt. `$` cuối pattern được đổi thành `\z`. Đột biến `M4`
  chứng minh test bắt đúng ca đó và **chỉ** ca đó.
- `IT-INTAKE-NUMBER-04` là dây báo động: tập field có pattern trên model phải bằng `["phone_e164"]`.
  Pattern mới thêm vào OAS sẽ làm test đỏ cho tới khi có ca từ chối cho nó.

`400` **không** nêu tên field — mô tả `phone_e164` ở `draft.30` hứa điều đó. Sửa lời hứa trong OAS thay
vì thêm hành vi.

---

## 3. Đột biến hai chiều

Mỗi đột biến sửa đúng một chỗ, build, chạy test tương ứng, rồi **trả file về từng byte** (so SHA-256
trước/sau). Cả `7` lần đều trả về khớp; `TaskIntakeService.cs` vẫn là `1d20ff2d…`, đúng hash đang ghim.

| # | Đột biến | Test đỏ | Kết quả |
| --- | --- | --- | --- |
| `M1` | Task chỉ-gửi-số vẫn gọi `Protect` | `UT-INTAKE-NUMBER-01` · `IT-INTAKE-NUMBER-DB-01` | ✅ đỏ cả hai — `Opaque-value encryption is not configured` |
| `M2` | Mọi task dùng **chung** một tham chiếu | `UT-INTAKE-NUMBER-05` · `IT-INTAKE-NUMBER-DB-04` | ✅ đỏ cả hai |
| `M2b` | Như `M2`, **và** gỡ assert *"hai tham chiếu khác nhau"* để chạy tới sổ đếm | `IT-INTAKE-NUMBER-DB-04` | ✅ đỏ — Postgres thật từ chối task thứ hai: `DIAL_TOKEN_TASK_MISMATCH` |
| `M3` | Bỏ vòng kiểm pattern ở endpoint | `IT-INTAKE-NUMBER-03` | ✅ `7/8` ca đỏ; ca *not-a-string* vẫn `400` vì deserializer chặn trước |
| `M4` | Dùng `$` của .NET thay cho `\z` | `IT-INTAKE-NUMBER-03` | ✅ đúng `1/8` ca đỏ: *trailing-newline* |
| `M5` | Bỏ nhánh *"có số thì dùng tham chiếu riêng"* khi `Protect` ném lỗi | `UT-INTAKE-NUMBER-02` · `IT-INTAKE-NUMBER-DB-02` | ✅ đỏ cả hai — ra `TASK_BLOCKED_OPERATIONAL`, **đúng hành vi production trước lô này** |
| `M6` | Bỏ kiểm token khi có số đi kèm | `UT-INTAKE-NUMBER-04` | ✅ đỏ |

`M2b` là phép đo đáng giữ nhất: mục `3` của §0 trước đó là **suy luận từ đọc code**. Giờ nó là kết quả
chạy trên Postgres thật.

`M3` ca *empty*: không có vòng kiểm pattern, body mang `phone_e164` là chuỗi rỗng được nhận `200`. Vòng
kiểm pattern là thứ duy nhất chặn nó.

---

## 4. Kiểm chứng

| Hạng mục | Kết quả |
| --- | --- |
| Toàn bộ solution | **`1176/1176`** (`787` unit · `357` integration · `24` contract · `8` chaos), `0` failed `0` skipped, `0` warning — `1151` → `1176` |
| Gate sweep (Git Bash) | **`39/39`** chạy, `22` bỏ qua theo manifest. Lần đầu `38/39` — §4.1 |
| `TestId` mới | **`14`**: `UT-INTAKE-NUMBER-01..06` · `IT-INTAKE-NUMBER-01..04` · `IT-INTAKE-NUMBER-DB-01..04` |
| Traceability | `721` → **`735`** |
| Contract freeze | `CONTRACT_FREEZE=PASS`, `required=21`, bảng `IR-06 §3.4` khớp OAS |
| `validate-openapi.mjs` | `10` task seed · `16` ca âm, gồm `3` ca mới |
| Chạy từ ngoài | **`28/28`** ví dụ qua HTTP thật trên stack riêng `ivr-w0312`, gồm `TASK-M3-NUMBER` trọn vòng tới callback và `NEG-HALF-PAIR` bị `400`. Gỡ stack **và volume của nó** sau khi chạy; volume dev dùng chung không bị đụng |
| Re-pin | `IR-06`, OAS và `TaskIntakeService.cs` đổi ⇒ `13` hash ở `7` file (`4` validator · `3` template `W-0181`/`W-0183`/`W-0187`), tính trên **byte LF** · `contract-manifest.json` qua `--accept-reviewed-draft`. `IR-06` ghim **hai lần** trong lô — lần hai sau khi sửa số fixture (§4.1); hash cuối `fa082134…`, `4` validator `--self-test` PASS |

### Dây chuyền contract

OAS `1.0.0-draft.31` · baseline `draft.31.yaml` · model NSwag sinh lại · `contract-manifest.json` ·
`docs/api-changelog.md` · changelog `30→31` và `27→31` · trang HTML và `portal-manifest.json` ·
`docs/contracts/` (diff + inventory) · `IR-06` (con trỏ version, §3.4 còn `21` field, §3.4.0 mới) ·
`IR-08` (ví dụ `TASK-M3-NUMBER`, `28` ví dụ) · seed thêm task chỉ-gửi-số và `3` ca âm.

**Sửa một link gãy có từ `W-0311`:** hàng changelog trong `IR-06` trỏ `28→29` tới
`…draft.28-to-…draft.30.md` — file không tồn tại, dấu vết của lần thay version trần. Lần này thay có neo
ngữ cảnh.

### `IR-07` đã gửi ⇒ đính chính, không sửa câu

Luật `14`. Thêm mục **Đính chính `2026-09-17`** ở cuối phiếu; phần trên giữ nguyên từng chữ. Mục đó có
`8` dòng: phiên bản hiện hành · `A-5` (sai ở `draft.30`, đúng từ `draft.31`) · `A-5` *"token bị bỏ
qua"* (bỏ qua lúc quay, **không** bỏ qua lúc nhận) · `A-6` *"không DB"* (sai từ `W-0311`) · `A-12`
`23` → `21` · số fixture `35` → `39` · Phần C mục `2` · `D-8` (*"bắt buộc ở bản kế tiếp"* — `draft.31`
chưa bắt buộc số).

`A-6` đáng nói riêng: nó **đã sai từ trước lô này** — `W-0311` bắt đầu lưu số vào DB. Đưa vào đính
chính vì Module 3 đang cầm một câu nói ngược lại, về chính dữ liệu khách họ gửi sang.

### 4.1 · Lỗi của tôi trong lô này — thêm một fixture mà không đếm chỗ đang đếm fixture

Thêm task `golden-hour-online-number-only` vào seed là đúng: đó là **đúng dạng body** Module 3 sẽ gửi, và
seed là bộ họ dùng để tự kiểm producer. Nhưng thành phần của seed (`9` task · `8` job · `35` ca) được
**ghi cứng ở `11` chỗ**. Lượt đầu tôi sửa `2` comment; lần chạy toàn solution thứ nhất bắt chỗ thứ ba:

```
ApiBehaviorMatrixTests.AllOpenApiOperationsHaveExecutedBehaviorEvidence
  loadDevSeed: happy response did not reach its expected business outcome   (38/39)
```

Quét lại cả repo thì ra thêm `8` chỗ, trong đó `2` chỗ **không test nào chạy tới**:
`tools/dev/Invoke-DevBootstrap.ps1` sẽ `throw` ngay lần tới có người chạy `pnpm dev:bootstrap`, và
`tools/dev/Test-ExpandContract.ps1` cũng vậy. Đã sửa cả `8`: `2` script dev · `2` comment code ·
`README.md` · `IR-06` (và ghim lại) · `IR-08` · đính chính `IR-07`. Bản ghi lịch sử
`docs/reports/2026-09-05-…` giữ nguyên.

Cùng một loại lỗi, ở tầng ví dụ sandbox: `IR-08` vẫn ghi *"24 ví dụ"* và *"Sáu tình huống cuộc gọi"* sau
khi tôi thêm ví dụ thứ `25`–`28`. Đã sửa; `TASK-M3-NUMBER` chuyển từ bảng sáu tình huống xuống một đoạn
riêng, vì nó là **cách gửi**, không phải tình huống cuộc gọi thứ bảy.

Và một mắt trong dây chuyền contract: tôi đã thêm mục `draft.31` vào `docs/api-changelog.md` nhưng quên
dòng **so sánh** ngay bên dưới, vẫn ghi bản hiện hành `draft.30`. Gate sweep lần đầu ra `38/39` —
`ci-config-selftest` bắt. Sửa, sinh lại portal (`portal-manifest.json` ghim hash của file nguồn đó), sweep
lại `39/39`.

Bài học cụ thể hơn *"chạy toàn bộ test"*: **thêm một phần tử vào một tập mà nhiều nơi đếm thì phải tìm
chỗ đếm trước khi thêm**, vì script dev và tài liệu không có test nào đọc giùm.

---

## 5. Cố ý không làm

| Việc | Vì sao |
| --- | --- |
| Bắt `phone_e164` thành bắt buộc | Vế **breaking** — chờ Module 3 xác nhận đã gửi số |
| Xoá `3` cột token | Stage `5`, sau khi Module 3 cắt hẳn |
| Đổi hình dạng `DialTokenReference` · `ConfirmationTaskSnapshot` · `IDialTokenResolver` | Luật `12`: thay đổi hẹp nhất |
| Cho `400` nêu tên field | Sửa lời hứa thay vì thêm hành vi |
| Cho LAB quay task chỉ-gửi-số | Vault LAB chỉ nhận alias trong allowlist — cố ý |

---

## 6. Thấy trong lúc làm, **chưa sửa** — ghi để quyết

| # | Phát hiện | Mức | Đề xuất |
| --- | --- | --- | --- |
| 1 | `pii_scan` trên GitLab (`allow_failure: false`) quét `docs/evidence`: **`41` dòng** ở **`13`** README `W-0297`…`W-0311` khớp mẫu. Tất cả là **báo nhầm** — `39` dòng chứa một từ tiếng Việt thông dụng trùng mẫu *tên phố*, `2` dòng trùng mẫu *đơn vị hành chính*; không có số điện thoại hay token nào. Quét lại cây theo commit: `257cbef^` **`0`** · `257cbef` **`1`** · `fe3bb19` **`41`**. Gate sweep cục bộ chỉ chạy `selftest-pii.sh`, nên `39/39` không thấy | 🔴 job CI này đỏ ở mọi commit từ `257cbef` | Lô 3: đổi từ trong `13` README, **không** nới mẫu. README này tự quét: `0` dòng |
| 2 | `IR-06 §3.4.1` vẫn ghi intake chỉ chặn token hết hạn **sớm**, chiều **muộn** *"hỏng ở persistence"* — trái với `W-0302` và với chính dòng `1311` của `IR-06` | 🟡 tài liệu cho M3 | Lô 3, kèm re-pin `IR-06` |
| 3 | `IR-07` `A-9` *"metadata giữ 90 ngày"* lệch với quyết định `S3` | 🟡 | Đính chính sau khi chốt cách hiểu `S3` |

## Owner nghiệm thu — 23/09/2026

Toàn chỉ thị “chấp nhận nhóm A và B” theo [phiếu W-0348](../W-0348/approval-request.md) tại
`7fc9806`. W-0312 thuộc nhóm A2 và chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm: draft.31: M3
gửi số là nhận được. Claude ghi nhận quyết định của owner, không tự cấp phê duyệt.

Còn mở, không thuộc nghiệm thu này: owner gửi changelog; stage bắt buộc số chờ M3. Mọi giới hạn
trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại `ca4f442`
(1200/1200 test, sweep 43/43), không coi commit tài liệu là một lượt test/sweep mới.
REAL_CUSTOMER_CALL_ALLOWED=NO.
