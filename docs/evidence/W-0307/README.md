# W-0307 — `B9` `audit-evidence`, và ba chỗ kế hoạch nói sai

**Ngày:** `2026-09-16` · **Baseline:** `main@d05abe0` · **Loại:** endpoint mới + contract `draft.29`

`REAL_CUSTOMER_CALL_ALLOWED=NO`

---

## 0. Tóm tắt

`GET /v1/ivr/order-confirmation/audit-evidence` — đọc sổ audit append-only theo một đối tượng,
`reason` bắt buộc, và **chính lượt đọc ghi một dòng audit**. Contract lên `1.0.0-draft.29`,
**không breaking** (kiểm bằng image ghim).

Kế hoạch mô tả lô này trong bốn dòng. Ba trong bốn dòng đó sai khi đối chiếu với mã, và **ba lỗi
thật** đều do **máy bắt chứ không phải do đọc lại code** — một trong ba do một gate có sẵn bắt,
trong khi toàn bộ test tôi tự viết cho lô này vẫn xanh.

---

## 1. Ba chỗ kế hoạch nói sai

| Kế hoạch viết | Thực tế |
| --- | --- |
| *"đọc bảng audit theo `object_type`/`object_id`"* | Cột tên là **`TargetType`/`TargetId`**. Không có cột nào tên `object_*` |
| *"masked qua `PiiMaskingFilter`"* | `PiiMaskingFilter` **không mask** — nó là guard **fail-closed**, ném `IVR_PII_POLICY_VIOLATION` và bỏ cả response. Nếu dựa vào nó để "mask" thì đúng những dòng kiểm toán viên cần nhất sẽ trả `500` |
| *"permission riêng"* | `DF-01` **LOCKED `7` quyền, do Permission Core sở hữu** — xem §2 |

Không mục nào trong ba mục này là bắt bẻ câu chữ: mỗi cái dẫn tới một cách cài đặt khác.

### 1.1. Hệ quả của việc filter không mask

Vì filter **từ chối** thay vì che, projection phải **an toàn theo cấu trúc**, không phải nhờ được
lọc ở cuối. Đã kiểm điều kiện đó chứ không giả định: mọi writer còn sống của `ivr_audit_log` chạy
`PiiGuard` **lúc ghi** — `PostgresAuditLogger` (`7` lần gọi), `TaskIntakeStores`,
`PostgresFeatureFlagStore`, `AttemptPolicyRegistryWriter`, `CallbackOutboxRepository`,
`InternalAdminApiService`.

Ngoại lệ duy nhất là `SuppressionProposer` (`0` lần gọi) — đã đọc nội dung nó ghi: chỉ enum, số và
bool, không trường nào chở được PII; và nó **chưa được wire vào production** (`0` call site ngoài
chính file đó, cùng lý do `OptOutSuppressionPolicy` là `DEAD_BY_OD-V1-23`).

Nên ba cột JSON được **công bố nguyên trạng**, với hai cổng độc lập: `PiiGuard` lúc ghi và
`PiiMaskingFilter` lúc đọc. Bỏ chúng đi thì endpoint gần như vô dụng cho đúng việc nó sinh ra để
làm — trả lời *"cái gì đã đổi"*.

---

## 2. Permission: đã **không** tự cấp quyền thứ tám

Kế hoạch bảo làm *"permission riêng"*. Tôi không làm, và lý do là lý do quan trọng nhất của cả lô.

`DF-01` là bộ permission admin: **LOCKED, `7` quyền, do Permission Core sở hữu**, không phải
Module 8. `OD-V1-20` đã phải **mở và ký hẳn một quyết định** chỉ để thêm `IVR_RUNTIME_GATE_ADMIN`.
Tự thêm quyền thứ tám ở đây là M8 ký vào sổ của người khác — **đúng thứ bản đánh giá `16/09` của
tôi đang bắt bẻ ở chỗ khác** (`OD-V1-01/02/03/05` mang nhãn `CLOSED` trên một chữ ký).

Nên: ship trên `AdminPolicies.Read`, và **ghi thẳng vào code** rằng một permission riêng cần một
quyết định `DF-01` mà module này không có thẩm quyền ký. Đó là phần còn thiếu, được nêu tên, chứ
không phải phần bị bỏ quên.

> Đây là cùng một hình dạng với `W-0304` mục `9`: làm hết phần trong thẩm quyền, gọi tên phần
> không thuộc thẩm quyền, không lặng lẽ vượt rào.

---

## 3. Ba lỗi thật, do máy bắt

### 3.1. Ngoại lệ PII không được dịch — `500` cho một request không bao giờ hợp lệ

`PiiGuard.EnsureSafeText` báo bằng cách ném `InvalidOperationException`, **không phải** một mã
API-06. Không bắt lại thì nó tới người gọi thành `500 IVR_INTERNAL_ERROR` — mã mà producer **được
phép retry** — cho một request sẽ không bao giờ hợp lệ dù gửi bao nhiêu lần.

**Đó đúng là khiếm khuyết `W-0302` vừa sửa ở đường intake**, và nó suýt ship lại ở đây. Không phải
review bắt được; `IT-AUDITEV-04` bắt được. Nay dịch thành `IVR_PII_POLICY_VIOLATION`, đúng cách
`PiiMaskingFilter` vẫn làm với cùng ngoại lệ đó.

### 3.2. Runtime công bố `accessAuditId`, contract khai `access_audit_id`

Mỗi property trong các contract admin khác đều mang `[property: JsonPropertyName("snake_case")]`.
Bản đầu của tôi bỏ đi và dựa vào mặc định của serializer — ra **camelCase**. Tức là runtime
trả `accessAuditId` trong khi OpenAPI tôi vừa viết khai `access_audit_id`: **contract và hiện thực
nói hai thứ khác nhau**, và M3 sinh client theo contract sẽ nhận `null` ở mọi trường.

**Test tích hợp của riêng loạt này không thể bắt được** — chúng deserialize vào **chính record đó**,
và một vòng round-trip luôn tự khớp với chính nó bất kể tên trường là gì. `IT-API-MATRIX-38` bắt
được, vì nó đọc response **theo tên trường mà contract công bố**.

Đây là lý do cái gate đó tồn tại, và là lần thứ ba trong lô này một lỗi thật bị **máy** bắt chứ
không phải người đọc lại code.

### 3.3. `truncated` và phép suy sai

Bản test đầu của tôi đọc hai lần trong một test rồi khẳng định số dòng không đổi. Nó **đỏ**, và cái
đỏ đó là endpoint đang nói thật — xem §4.

---

## 4. Đọc lịch sử một đối tượng làm chính lịch sử đó dài thêm

`PostgresAuditLogger` tách `EntityRef` (`type:id`) ngược lại thành `TargetType`/`TargetId`. Nên
dòng audit mà lượt đọc tự ghi **khớp đúng bộ lọc người gọi vừa dùng**: đọc hai lần thì lần sau
nhiều hơn đúng một hàng.

Nghe lạ, nhưng **đúng**: ai đã xem một bản ghi cũng là một phần của những gì đã xảy ra với bản ghi
đó. Một kiểm toán viên thấy được mọi thay đổi nhưng không thấy lượt xem nào thì đang thiếu đúng nửa
quan trọng nhất khi câu hỏi là *"ai đã biết"*.

Đã ghim bằng `IT-AUDITEV-15` — để lần sau không ai "sửa" nó thành một điểm mù — và ghi vào `IR-06`
§4A.3 như điểm thứ `3` M3 phải biết khi dựng màn hình.

`truncated` cũng vì vậy là **một field**, không phải thứ người gọi suy từ `rows.length == limit`:
phép suy đó sai đúng lúc số dòng tình cờ bằng `limit`. Service lấy `limit + 1` dòng để **đo** thay
vì đoán. `IT-AUDITEV-16` ghim đúng biên đó.

---

## 5. Hai thứ bị từ chối, và vì sao không phải validation thường

| Từ chối | Vì sao |
| --- | --- |
| Thiếu `target_type` **hoặc** `target_id` → `400` | Một lượt đọc không bộ lọc trả về các dòng mới nhất **trên toàn hệ thống** — trích xuất hàng loạt sổ audit đội lốt tra cứu. Mặc định "tất cả" là sai chiều cho một cái sổ |
| Contact detail trong selector → `IVR_PII_POLICY_VIOLATION` | `target_id` là định danh hệ thống cấp, không phải chữ người gõ. Số điện thoại tới đây nghĩa là người gọi đang tra sai thứ — và để lọt thì nó **được ghi vào dòng audit truy cập**, tạo ra đúng lần lộ dữ liệu mà endpoint này sinh ra để điều tra |

`IT-AUDITEV-01` và `IT-AUDITEV-04` chứng minh cả hai **không chạm database**: stub factory và stub
logger đều ném nếu service với tới chúng. Một request bị từ chối không được mở kết nối và không
được ghi dòng truy cập cho một lượt gọi chưa từng được phục vụ.

---

## 6. Contract — `draft.29`

Thêm `GET /audit-evidence` cùng hai schema `IvrAuditEvidence` / `IvrAuditEvidenceRow`.

```
oasdiff changelog draft.28 -> draft.29   1 changes: 0 error, 0 warning, 1 info
                                         info [endpoint-added] GET /audit-evidence
oasdiff breaking  draft.28 -> draft.29   No breaking changes to report      exit 0
oasdiff breaking  draft.27 -> current    No breaking changes to report      exit 0   (đúng lệnh CI chạy)
```

Image ghim `tufin/oasdiff:v1.26.1`. **Thêm một endpoint không đổi một byte nào của bề mặt M3 đã sinh
client theo, nên M3 không phải sinh lại.**

**`8` nơi ghim hash đã dời**, và lần này **tính trên byte LF** — `W-0302` tính trên cây làm việc
Windows (CRLF) nên pin xanh tại máy và đỏ trên CI, và `W-0306` phải dọn:

```
specs/api/openapi/contract-manifest.json          (sinh bằng openapi-contract-drift --accept-reviewed-draft)
docs/contracts/openapi-contract-diff.md           (cùng generator)
docs/contracts/target-v1-field-inventory.md       (contract-freeze-verifier --write)
docs/api/portal-manifest.json                     (build-api-docs.mjs)
deploy/ci/scripts/dial-token-production-bundle-validator.mjs
docs/evidence/W-0183/dial-token-production-bundle.template.json  (--print-template)
+ IR-06 ở 7 nơi, hai lượt (nội dung IR-06 đổi hai lần: version pointer, rồi §4A.3)
```

**Baseline CI giữ `draft.27` — có chủ đích, và không phải của tôi.** `W-0302` chọn vậy để lượt sau
còn so được tích luỹ. `ci-config-selftest` cưỡng chế rằng con số baseline trong
`docs/api-changelog.md` phải **khớp** thứ `docs.gitlab-ci.yml` thật sự so. Bản đầu tôi đặt baseline
thành `draft.28` và gate đỏ ngay — đúng việc của nó. Đã trả về `draft.27` và để index changelog là
`27→29`, tức đúng thứ CI sinh ra, thay vì lặng lẽ dời baseline của người khác.

---

## 7. Kiểm chứng

```
dotnet build                 0 Warning(s)  0 Error(s)
traceability                 sinh lại, 708 (+11 TestId)
docs-selftest                API_DOCS_SELFTEST_PASS
ci-config-selftest           CI_CONFIG_SELFTEST_PASS   (bắt được baseline lệch, xem §6)
compliance-pack · review-gate · progressive · selftest-openapi   PASS
contract-freeze-verifier     CONTRACT_FREEZE=PASS  intake=1.0.0-draft.29 required=23
openapi-contract-drift       OPENAPI_HUMAN_DIFF_CURRENT=YES
quét pin toàn cây            44 đường dẫn · 0 lệch · 0 file thiếu
gitnexus_impact              AddIvrFoundation-style: MapIvrAdminEndpoints LOW, 0 impacted, 0 process
```

### 7.1. Mutation cả hai chiều

| Mutation | Kết quả |
| --- | --- |
| `.Take(limit + 1)` → `.Take(limit)` | `IT-AUDITEV-12` **đỏ** — `truncated` mất khả năng phân biệt |
| Bỏ dịch ngoại lệ PII | `IT-AUDITEV-04` **đỏ** cả hai case, ngoại lệ thô lọt ra |
| *(không phải mutation — lỗi thật)* thiếu `JsonPropertyName` | `IT-API-MATRIX-38` **đỏ**; test của riêng lô này **xanh**, vì chúng round-trip qua cùng một record |

### 7.2. Một ghi chú về giới hạn của test do chính mình viết

Ba lỗi thật trong lô này (`§3.1`, `§3.2`, `§3.3`) đều do **test bắt**, không do review. Nhưng
đáng nói hơn: `§3.2` do một gate **có sẵn** bắt, trong khi toàn bộ `11` test tôi tự viết cho loạt
này đều xanh với bản code sai — vì chúng kiểm hệ thống bằng chính định nghĩa của hệ thống.
Một bộ test viết cùng lúc với code sẽ chia chung mọi giả định sai của code đó; thứ phát hiện được
là gate đọc **hiện vật độc lập** — ở đây là chính file OpenAPI.

---

## 8. Còn lại

| Việc | Vì sao chưa |
| --- | --- |
| Permission riêng cho audit-evidence | Cần quyết định `DF-01`, do Permission Core sở hữu. **M8 không ký được** |
| `capacity-incidents` endpoint | Kế hoạch đã loại từ đầu: `AdminReadService` có đường đọc, nhưng phơi ra endpoint thì phải biết M3 lọc/phân trang thế nào. ⛔ Chờ M3 |
| Phân trang thật (cursor) cho trail dài | `limit` + `truncated` là đủ cho tra cứu một đối tượng. Cursor chỉ đáng làm khi có màn hình thật dùng nó — M3 dựng |
