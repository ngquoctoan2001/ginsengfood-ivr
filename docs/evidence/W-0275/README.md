# W-0275 — Siết `adapter_mode` thành enum, và một field không siết được

Ngày: 2026-09-10 · Baseline: `main@9379cbe` · Trạng thái: **TESTS_PASS** (xem mục 6 — hai điều chưa
kiểm được ở máy dev).

Owner chốt ngày 2026-09-10: siết `adapter_mode` thành `enum` trong OpenAPI. Contract
`1.0.0-draft.25 → 1.0.0-draft.26`.

## 1. Đính chính điều tôi nói ở lượt trước

Tôi mô tả việc này là *"breaking change với Module 3"*. **Sai.**

`adapter_mode` và `provider_name` chỉ xuất hiện trong hai schema, và cả hai chỉ được tham chiếu từ
**response**: `IvrSimChannelList` qua `'200'`, và `IvrDashboardProjection.sim`. **Module 3 không gửi
field này.** Enum ràng buộc thứ **IVR phát ra**, không ràng buộc thứ Module 3 gửi — với phía đọc nó
là được thêm bảo đảm, không phải bị lấy đi.

## 2. Một field không siết được, và lý do

Tôi siết **ba** chỗ, rồi phải rút **một**.

`IvrDashboardSimPanel.adapter_mode` giữ nguyên `type: string`. `AdminReadService.cs:650`:

```csharp
channels.Count > 0 ? channels[0].AdapterMode : executionMode
```

Field đó mang **adapter của channel đầu** (`MOCK`/`VENDOR`/`ASTERISK_ARI`) khi có channel, và
**execution mode** (`MOCK`/`LAB_REAL_SIM`/`PRODUCTION_REAL`) khi **không có channel nào**. Hai từ
vựng trong một field, chọn theo số dòng.

Nó chỉ "chạy được" vì hai tập cùng chứa giá trị `MOCK`, và hệ thống luôn ở `MOCK` với không channel
thật. Một lượt lab ở `LAB_REAL_SIM` chưa provision channel sẽ phát ra `adapter_mode: "LAB_REAL_SIM"`.

Đây đúng là hình dạng của S2 — cùng một chữ mang hai nghĩa — lần này nằm trong **hợp đồng trên
wire**. Sửa nó là đổi hành vi (`AdminReadService` phải trả gì khi không có channel?), nên **không tự
quyết**. Lý do được ghi ngay trên dòng đó trong file yaml.

Phát hiện được **chính vì** siết enum: build đỏ ở đúng dòng test khẳng định fallback đó.

## 3. Làm gì

| Bước | |
| --- | --- |
| `enum [MOCK, VENDOR, ASTERISK_ARI]` | `IvrSimChannel.adapter_mode`, `IvrSimChannel.provider_name` |
| Version | `draft.25 → draft.26` (spec, handover doc 5 chỗ, contract test) |
| Codegen | `regenerate-openapi.ps1` — nswag sinh 2 enum C# |
| Pin | drift `--accept-reviewed-draft`, freeze `--write`, `dial-token.task_oas_sha256` |
| Docs | `build-api-docs.mjs`, baseline `draft.26.yaml`, `docs.gitlab-ci.yml` trỏ baseline mới |

`provider_name` siết kèm dù owner chỉ hỏi `adapter_mode`: cùng từ vựng, cùng object, và một lượt bump
riêng cho nó tốn đúng 15 file như lượt này.

**Mã sản phẩm không đổi một dòng.** DTO sinh tự động chỉ được contract/integration test dùng.

## 4. Dọn luôn cascade treo của agent khác

Ba file `opt-out-suppression-bundle-validator.mjs`, `upstream-session-signoff-validator.mjs` và
template `W-0187` mang pin `768583…` chưa commit của agent kia — giá trị **không khớp cả tài liệu
hiện tại lẫn `HEAD`**, tức việc dở dang. Hai gate đã đỏ vì nó suốt phiên này.

Sửa handover doc buộc phải re-pin cả 5 chỗ, nên tôi dọn luôn. Căn cứ để làm thay: commit cuối của họ
lúc **08:40**, ghi file cuối **11:27**, lúc làm việc này là **18:08** — im gần 7 tiếng. Bản của họ đã
sao lưu trước khi ghi đè.

Kết quả: **`GATE_SWEEP_PASS 39/39`** — lần đầu xanh hết trong phiên này.

## 5. Ba chỗ nói ba số khác nhau

Khi cập nhật changelog mới thấy:

| Nguồn | Baseline nó cho là hiện hành |
| --- | --- |
| `docs/api-changelog.md` | **`draft.23`** |
| `docs/api/changelog/ivr-order-confirmation.md` | **`draft.24`** |
| `deploy/ci/docs.gitlab-ci.yml` | **`draft.25`** |

Không chỗ nào khớp chỗ nào, và cả ba đã lệch **trước** lượt này. Đã kéo cả ba về `draft.26`, nhưng
**không có guard** nào giữ chúng đồng bộ — đó là lý do chúng trôi ba nhịp khác nhau. Đề xuất một
`CT-CI-12` buộc ba nguồn khớp; chưa làm ở lượt này vì đã đủ dài.

## 6. Hai điều tôi **không** kiểm được ở máy này

**`oasdiff breaking … --fail-on WARN`** (`docs.gitlab-ci.yml:23`) chưa từng chạy trên thay đổi này.
`oasdiff` chỉ có trong image CI, không có trên máy dev — `gate-invocations.json:131` ghi đúng điều
đó. Nên **chưa xác nhận** enum mới không bị phân loại là breaking.

**Bản so sánh máy sinh `draft.25 → draft.26`** không có trong commit này, cùng lý do. Không tự viết
tay: `docs.gitlab-ci.yml:20` `diff -u` byte-for-byte với đầu ra generator, nên một bản viết tay sẽ
vừa sai vừa bị bắt.

Kèm theo, một lỗ hổng của chính quy ước: baseline được đẩy lên `draft.26` **trong cùng commit** làm
spec thành `draft.26`, nên diff `draft.25 → draft.26` **không bao giờ được `oasdiff breaking` soi**.
Agent kia làm y hệt cho `draft.25`. Không phải lỗi tôi tạo ra, nhưng lượt này thừa hưởng nó.

## 7. Kết quả

```
Unit         670/670
Integration  283/283
Contract      24/24
Chaos          8/8
             ─────────
             985/985
```

`dotnet build Ivr.sln` — 0 warning, 0 error. **`GATE_SWEEP_PASS 39/39`**.

`contract-freeze-selftest` từng đỏ với *"mutation changed nothing — its anchor text has drifted"*:
case rotation viết cứng `"version: 1.0.0-draft.25"` nên khi spec sang `draft.26` nó không thay gì cả.
Đúng loại lỗi mà chính comment trong `editText` mô tả, và guard đó bắt được. Đã cho nó **đọc phiên
bản** thay vì viết cứng — viết cứng số kế tiếp chỉ dời cái bẫy đi một release.

Ngược lại, `CT-M3-AUTHORITY-08` cũng viết cứng phiên bản nhưng **chỉ bump, không làm động**: nó hỏng
**ồn ào**, đó đúng là việc của một pin. Cái phải sửa là cái hỏng **im lặng**.

## 8. Còn lại

- **`IvrDashboardSimPanel.adapter_mode` mang hai từ vựng** — cần owner quyết `AdminReadService` trả
  gì khi không có channel.
- **Không guard nào giữ ba nguồn baseline đồng bộ** (mục 5).
- **Chạy hosted CI** để đóng hai điều ở mục 6.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN
