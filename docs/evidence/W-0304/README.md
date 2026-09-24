# W-0304 — Lô tài liệu 11 mục, và 11 gate đỏ mà nó phát hiện

**Ngày:** `2026-09-16` · **Baseline:** `main@d207526` · **Loại:** docs + CI pins (1 file `.cs`, chỉ doc-comment)

`REAL_CUSTOMER_CALL_ALLOWED=NO`

---

## 0. Tóm tắt một đoạn

Lô này được lập kế hoạch là **11 mục sửa tài liệu**, ước tính `1,5` ngày. Mục `10` —
*"cập nhật mọi dẫn chiếu tới file `W-0297` vừa xoá"* — hoá ra không phải việc sửa link: `W-0297`
đã làm **11 CI gate đỏ** và không ai biết, vì chúng đỏ theo kiểu chỉ hiện ra khi chạy trên cây LF.

Kết quả: `10/11` mục hoàn thành, `1` mục xác định là **tiền đề sai** (không cần sửa gì), cộng
`11` gate được đưa từ đỏ về xanh — trong đó `3` gate đỏ vì **lỗi của chính tôi ở `W-0302`**.

---

## 1. Việc theo kế hoạch — `11` mục

| # | Việc | Kết quả |
| --- | --- | --- |
| 1 | Gỡ *"Chưa chốt (`W-0215`)… `End ≥ 21:07:30`"* ở `IR-06` | ✅ thay bằng *đã chốt `W-0220`, `End = 21:08`* |
| 2 | Bỏ `customer_display_name` khỏi `allowed_input_fields` | ✅ + sửa luôn câu `ProductionTargetV1FieldsApproved` mặc định `YES` (nay `NO`, `W-0299`) |
| 3 | Sửa `IVR_OPT_OUT → IVR_POLICY_BLOCKED` ở `m8-05 §3.1` | ⛔ **TIỀN ĐỀ SAI — không sửa gì**, xem §3 |
| 4 | Ghi `priority` không có trên wire + thứ tự scheduler | ✅ `IR-06 §3.5`, đọc thẳng từ `PostgresSchedulerStore` |
| 5 | Đồng bộ `§4.8` với đặc tả fence thu hồi | ✅ hai fence `W-0249`, và phần còn thiếu là endpoint M3 |
| 6 | Bảng map `result_type` → `cancellation_reason_code` | ✅ `IR-06 §4.3`, ghi rõ **đề xuất chưa ký** |
| 7 | Gỡ mâu thuẫn vị trí `E.164` | ✅ nhưng mâu thuẫn thật khác mô tả — xem §4 |
| 8 | Đổi nhãn `CLOSED` → `M8_POSITION_SIGNED / M3_NOT_RECEIVED` | ✅ `OD-V1-01/02/03/05` |
| 9 | Xoá hoặc đánh dấu `OptOutSuppressionPolicy` | ✅ **đánh dấu, không xoá** — xem §5 |
| 10 | Cập nhật dẫn chiếu tới file `W-0297` đã xoá | ✅ **hoá ra là §2 dưới đây** |
| 11 | Ghi hành vi đơn đêm (`W-0298`) vào `§3.4.2` | ✅ |

---

## 2. Mục `10` — `W-0297` đã làm `11` gate đỏ

### 2.1. Chuyện gì đã xảy ra

`W-0297` (`257cbef`) gộp `plan/ivr-orther` thành hai file trạng thái. Nó **xoá `27` file** và
**sửa `62` file**. Trong số bị xoá có `12` file là **đầu vào CI được ghim hash**, liệt kê ở
`deploy/ci/pins/external-decision-artifacts.sha256` — chính file mà phần header của nó viết:

> *"When a pinned source legitimately changes, update the matching line here in the same commit."*

Không phải "changed" mà là **deleted**, và không re-pin. Trong `62` file bị sửa có **`15` file
được ghim hash**, cũng không re-pin cái nào.

`164 KB` decision pack đã ký bị xoá; thứ thay thế là `19 KB` tóm tắt trạng thái. Nội dung
**không chuyển đi đâu cả** — nó mất.

### 2.2. `11` gate đỏ

| Gate | Đỏ vì |
| --- | --- |
| `external-decision-response-validator` | `ENOENT` `plan/ivr-orther/m8-*.md` |
| `external-decision-closure-validator` | như trên (phụ thuộc) |
| `external-decision-routing-validator` | như trên |
| `external-decision-c9-selftest` | như trên (phụ thuộc) |
| `external-decision-dial-token-selftest` | như trên (phụ thuộc) |
| `opt-out-suppression-bundle-validator` | `ENOENT` `m8-08`, rồi drift `IR-06` |
| `capacity-data-intake-validator` | `ENOENT` `m8-14` |
| `capacity-registry-decision-pack-validator` | drift `docs/evidence/W-0160/README.md` |
| `d06-revalidation-evidence-validator` | drift `IR-06` |
| `dial-token-production-bundle-validator` | drift OAS + `IR-06` |
| `upstream-session-signoff-validator` | drift `IR-06` |

### 2.3. Cách sửa: khôi phục `12` file, không repoint

Hai hướng có thể đi. **Repoint** validator sang `00-CHUA-XONG.md` là không làm được: các validator
**đọc cấu trúc** trong pack (decision id, hàng phê duyệt), mà `19 KB` tóm tắt không chứa cấu trúc
đó. Repoint sẽ phải viết lại validator **và** tái tạo nội dung đã mất.

**Khôi phục** thì đúng và rẻ: cả `12` file khôi phục ra hash **trùng khít manifest**, `0` re-pin.

```
MATCH  questions-to-module-3-od18-authority          MATCH  m8-09-revoke-freshness-decision-pack
MATCH  m8-05-program-result-contract-signoff         MATCH  m8-10-contact-dial-token-production
MATCH  m8-06-upstream-session-trace-signoff          MATCH  m8-11-attempt-policy-production
MATCH  m8-07-target-v1-shared-callback-handoff       (m8-12..m8-15 ghim ở validator riêng, xác minh bằng gate)
MATCH  m8-08-opt-out-suppression-decision-pack
```

`15` file còn lại **giữ nguyên trạng thái đã xoá** — chúng là ghi chú kế hoạch thật sự đã cũ và
không gate nào ghim.

### 2.4. Một lỗi find-and-replace mù

`W-0297` thay `00-index.md` → `00-CHUA-XONG.md` **trên toàn cây**, không giới hạn thư mục. Nhưng
file bị xoá là `plan/ivr-orther/00-index.md`, còn sáu thư mục khác **cũng** có `00-index.md` và
chúng vẫn tồn tại. Hậu quả: `12` file trỏ vào path không có thật.

| Bị hỏng thành | Đúng phải là | File còn tồn tại? |
| --- | --- | --- |
| `specs/data/00-CHUA-XONG.md` | `specs/data/00-index.md` | ✅ có |
| `specs/api/00-CHUA-XONG.md` | `specs/api/00-index.md` | ✅ có |
| `specs/ui/00-CHUA-XONG.md` | `specs/ui/00-index.md` | ✅ có |
| `specs/testing/00-CHUA-XONG.md` | `specs/testing/00-index.md` | ✅ có |
| `specs/workflows/00-CHUA-XONG.md` | `specs/workflows/00-index.md` | ✅ có |
| `prompt/00-CHUA-XONG.md` | `prompt/00-index.md` | ✅ có |

Đã sửa `11/12` file. File thứ `12` là `prompt/_execution/prompt-execution-tracker.md` — phiên
`PD-01` đang giữ lúc đó, sửa sau khi họ commit `W-0305`.

### 2.5. Hai bản ghi evidence đã đóng bị viết lại

Nghiêm trọng hơn link chết: cùng phép thay mù đó đã **sửa nội dung hai bản ghi evidence đã đóng**,
biến một lời khai thành sai:

| File | `W-0297` đổi thành | Nguyên văn |
| --- | --- | --- |
| `docs/evidence/W-0170/README.md:196` | ``…sửa đúng **một** dòng: `00-CHUA-XONG.md#m8-05` `` | ``…`m8-05-program-result-contract-signoff-2026-09-03.md` `` |
| `docs/evidence/W-0250/README.md:111` | ``…đang đỏ vì `00-CHUA-XONG.md#m8-05` lệch manifest`` | ``…vì `m8-05-…-2026-09-03.md` lệch manifest`` |

Cả hai đã trả về nguyên văn. Một bản ghi evidence nói **nó đã thấy gì vào ngày viết**; sửa nó
không phải là cập nhật, mà là làm sai lệch hồ sơ.

Ngược lại, `docs/evidence/W-0187/attested-sha256.txt` **cố ý không đụng tới**: script re-pin của
tôi có sửa nó, và tôi đã revert. Header của pin manifest nói thẳng file đó *"is meant to go stale
as sources move on"*.

---

## 3. Mục `3` là tiền đề sai — không sửa gì

Kế hoạch yêu cầu sửa `m8-05 §3.1` để `IVR_OPT_OUT` map sang `IVR_POLICY_BLOCKED`. Đọc file thật:

```
dòng 23: 4. **Result contract:** giữ đúng 11 code hiện hành. Không thêm `IVR_OPT_OUT`, không đổi
dòng 82: | `IVR_OPT_OUT` | **không phải result code** — chặn ở eligibility, ghi `IVR_POLICY_BLOCKED` |
```

`m8-05` **đã nói đúng thứ cần nói**, từ `2026-09-03`. Không sửa một ký tự nào.

Đáng ghi lại vì `m8-05` là **decision pack đã ký và được ghim hash**: sửa nó để "khắc phục" một
vấn đề không tồn tại sẽ vừa làm sai lệch hồ sơ đã ký, vừa buộc re-pin manifest — hai cái giá phải
trả cho không có gì.

---

## 4. Mục `7` — mâu thuẫn thật khác mô tả trong kế hoạch

Kế hoạch ghi: *"mâu thuẫn vị trí `E.164` giữa `IR-06 §6` và `specs/api/04-sim-adapter-contract.md`"*.
Đọc cả hai: chúng **không mâu thuẫn**. API-04 §2 và `IR-06:1252` nói cùng một điều — resolver nằm
trong tiến trình IVR, E.164 chỉ trong bộ nhớ.

Mâu thuẫn thật nằm **bên trong `IR-06 §6`**: tiêu đề mục ghi *"đã chốt hợp đồng, còn lại là vận
hành"*, nhưng thân mục vẫn trình bày `4` phương án để chọn và `5` câu hỏi để mở — trong khi cả `5`
đều đã được `OD-V1-05` / `OD-V1-17` / `OD-V1-18` quyết từ `05/09` và `09/09`. Đã thêm bảng đối
chiếu từng câu hỏi → nơi đã quyết, và chỉ định API-04 §2 là **nguồn sự thật duy nhất**.

---

## 5. Mục `9` — đánh dấu chứ không xoá

`gitnexus_impact(OptOutSuppressionPolicy, upstream)`: **HIGH**, `92` symbol, `22` trực tiếp,
**`0` execution flow**, `0` module. Cảnh báo HIGH đã ghi theo `CLAUDE.md`, kèm nghĩa thật của nó:
các cạnh là `IMPORTS` **mức file** của namespace `Ivr.Domain.Policies`, không phải `CALLS`. Đếm
call site thật của `OptOutSuppressionPolicy.Decide`: **`8`, tất cả trong `tests/`**, `0` production.

Kế hoạch cho phép *"xoá hoặc đánh dấu"*. Xoá là sai, ba lý do mỗi cái đủ một mình:

1. `opt-out-suppression-bundle-validator.mjs` **ghim file này bằng SHA-256 và đọc nó** — xoá là
   gate đỏ không có gì thay thế;
2. `OPT-01..11` trong bundle `W-0187` đang chờ quorum mà Legal/Privacy và CRM/M3 chưa cho — file
   này **chính là artifact** các quyết định đó nói về;
3. lập luận *"một cuộc gọi bị từ chối không phải là opt-out"* là phần đáng giữ dù V2 quyết gì.

Đã thêm doc-comment `DEAD_BY_OD-V1-23` nêu đủ: `0` caller production, vì sao không nên nối vào, và
điều kiện để dùng lại (mở lại `OD-V1-23` + ký `OPT-01..11`) — *"không đủ nếu chỉ thêm một caller"*.

---

## 6. Lỗi của chính tôi ở `W-0302` — pin tính trên CRLF

`W-0302` báo cáo *"`9` gate PASS"*. Sai, và đây là cách nó sai:

| | |
| --- | --- |
| OAS tôi ghim | `453bd331…` |
| OAS thật sự commit | `f9566e72…` |
| `453bd331` là gì | sha256 của **cùng file đó ở dạng CRLF** |

`core.autocrlf=true`. Tôi tính hash trên **cây làm việc Windows** (CRLF), git lưu **LF**. Pin vì vậy
xanh trên máy tôi và **đỏ trên CI** — đúng lớp lỗi `W-0126` đã ghi lại, và tôi lặp lại nó.

`6` chỗ phải sửa (kế hoạch `W-0302` nói `5`; nó bỏ sót template):

```
deploy/ci/scripts/dial-token-production-bundle-validator.mjs
docs/api/portal-manifest.json                     ← sinh lại bằng build-api-docs.mjs, không sửa tay
docs/contracts/openapi-contract-diff.md
docs/contracts/target-v1-field-inventory.md
specs/api/openapi/contract-manifest.json
docs/evidence/W-0183/dial-token-production-bundle.template.json   ← W-0302 BỎ SÓT
```

Thêm hai file nữa `W-0298`/`W-0302` sửa mà quên re-pin: `EligibilityRules.cs`,
`TaskIntakeService.cs`.

**Biện pháp, không phải lời hứa:** đã chuẩn hoá `14` file đầu vào ghim hash về LF trong cây làm
việc. Vì git lưu LF sẵn, việc này **không đổi một byte nào của commit** (`git diff` trống cho cả
`5` file kiểm mẫu) — nó chỉ làm lần chạy gate tại máy **nói thật** thay vì nói dối theo chiều an
toàn giả.

---

## 7. Kiểm chứng

### 7.1. `13/13` validator ghim hash

```
PASS external-decision-response-validator      PASS d06-revalidation-evidence-validator
PASS external-decision-closure-validator       PASS dial-token-production-bundle-validator
PASS external-decision-routing-validator       PASS upstream-session-signoff-validator
PASS external-decision-c9-selftest             PASS contract-freeze-verifier
PASS external-decision-dial-token-selftest     PASS attempt-policy-production-bundle-validator
PASS opt-out-suppression-bundle-validator      PASS capacity-data-intake-validator
PASS capacity-registry-decision-pack-validator
```

### 7.2. Bộ gate tài liệu chuẩn

```
DOC_BOUNDARY_PASS · DOC_LINKS_PASS · DOC_CI_TOPOLOGY_PASS · API_DOCS_SELFTEST_PASS
COMPLIANCE_PACK_SELFTEST_PASS · CI_CONFIG_SELFTEST_PASS · REVIEW_GATE_SELFTEST_PASS
PROGRESSIVE_SELFTEST_PASS · TEST_TRACEABILITY_CURRENT=695
```

### 7.3. Quét pin toàn cây

Sau khi sửa: **`0` pin lệch**.

> **Đính chính `W-0306` (16/09):** dòng này ban đầu viết *"`0` pin lệch trên `183` [path] ghim
> hash"*. **Con số `183` sai.** `183` là số path **ứng viên chuẩn hoá CRLF** — mọi chuỗi trông
> giống path trong `deploy/ci/scripts/*.mjs` — chứ không phải số path thật sự có hash
> ghim kèm. Số đúng, đo lại bằng chính script ở `W-0306`, là **`44`**. Kết luận *"`0` lệch"* không
> đổi — cả `44` pin đều khớp — nhưng một con số lớn hơn thực tế **`4`** lần làm phạm vi kiểm tra
> nghe rộng hơn nó thật, và đó đúng lủ lỗi mà bản đánh giá `16/09` đang bắt người khác. Sửa bằng
> một dòng đính chính thay vì lặng lẽ đổi số.

### 7.4. Build và `gitnexus`

- `dotnet build`: `0 Warning(s)`, `0 Error(s)`
- `gitnexus_detect_changes`: `risk_level: low`, **`0` affected process**, `75` file / `145` symbol,
  toàn bộ `change_type: touched` (mục tài liệu + `4` hằng `SOURCE_PINS` + doc-comment)

### 7.5. Gate cần môi trường ngoài — **không** chạy, **không** khai là xanh

`image-selftest` (Docker), `dr-selftest` (DB), `observability-runtime-selftest` (runtime) không
chạy được tại máy này. Chúng **không** liên quan tới thay đổi của lô này, và tôi không khai chúng
PASS.

---

## 8. Còn lại

| Việc | Vì sao chưa |
| --- | --- |
| Sửa `specs/api/00-CHUA-XONG.md` trong `prompt-execution-tracker.md` | phiên `PD-01` giữ file lúc đó; làm ngay sau `W-0305` |
| `DTK-02`/`DTK-06`, `D-06` endpoint thu hồi, map `cancellation_reason_code` | chờ M3 — không phải việc M8 làm thay |
| `OPT-01..11` | chờ quorum Legal/Privacy + CRM/M3 |

---

## 9. Bài học đáng ghi

**Một lượt dọn tài liệu có thể làm đỏ CI mà không ai thấy.** `W-0297` chạy xong, commit xong, và
`11` gate đỏ nằm im `một ngày` cho tới khi lô này chạm vào. Không lượt nào giữa đó chạy full sweep,
và ba lượt của chính tôi (`W-0298`…`W-0302`) đều báo "gate PASS" — vì tôi chỉ chạy các gate liên
quan tới thay đổi của mình.

**Quy tắc rút ra, cụ thể hơn "chạy gate trước khi commit":** khi một lượt **xoá hoặc đổi tên** file,
thứ phải chạy không phải gate của phạm vi mình, mà là **quét toàn bộ pin** — vì pin là thứ duy nhất
biết rằng một file nào đó ở chỗ khác đang phụ thuộc vào file bạn vừa xoá.

## Khai báo phép kiểm C2 — W-0349, 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Hồ sơ này trước không khai test hay phép kiểm nào, nên C2 không xét được. [Khai báo](acceptance-tests.json) nay ghi:

- Gate trong full sweep có self-test kiểm thay đổi của việc này: `opt-out-suppression-bundle-validator.mjs`.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0304 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Residual chỉ
còn việc bên ngoài (M3, Sales, Platform, tier GitLab, pháp chế) hoặc giới hạn đã ghi. Claude chọn
việc theo tiêu chí phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.
