# W-0272 — `"MOCK"` là ba thứ, không phải hai, và một chỗ spec lệch code lộ ra khi tách

Ngày: 2026-09-10 · Baseline: `main@2ff5b92` · Trạng thái: **TESTS_PASS**.

Phần còn lại của S2 trong [`docs/review/2026-09-09-codebase-audit.md`](../../review/2026-09-09-codebase-audit.md),
theo quyết định owner ngày 2026-09-10.

## 1. Quyết định owner

Owner chốt: **tách bằng hằng số, giữ nguyên giá trị trên wire** — không migration, không đổi config,
không đổi hành vi. Đúng khuôn `ExecutionModes.ToWireValue` ở `W-0261`.

## 2. Đo lại: ba, không phải hai

Báo cáo (và cả bản tôi trình bày cho owner) nói `"MOCK"` có **hai** chủ sở hữu. Phân loại từng site
thì có **ba**:

| Nghĩa | Site | Hằng số |
| --- | ---: | --- |
| Hệ thống đang chạy ở chế độ nào | 8 | `ExecutionModes.Mock` — đã có |
| Cấu hình chọn SIM provider nào | 5 | `FeatureFlagValues.MockSimProvider` — đã có |
| **Adapter/provider sở hữu một dòng SIM channel** | 5 | **`SimAdapters.Mock` — mới** |

Cái thứ ba là `ivr_sim_channels.adapter_mode` và `provider_name`. Một dòng channel có thể là `MOCK`
trong khi execution mode **không** phải `MOCK` — nên chúng không thay cho nhau được. Đây đúng là chỗ
báo cáo cảnh báo *"thay tất cả sẽ sai ở khoảng một nửa"*, và cảnh báo đó đúng.

Ba site còn mơ hồ đã tra ngữ cảnh chứ không đoán: `ServiceCollectionExtensions:61` là **SIM
provider** (giá trị mặc định của `options.SimProvider`), `PostgresSchedulerStore:70` và
`FakeDeterministicTtsProvider:25` là **execution mode**.

**Không đụng** 12 dòng `appsettings*.json` (giá trị cấu hình, phải là chuỗi), 10 dòng
`IvrServerModels.g.cs` (generator sở hữu) và 2 dòng SQL trong migration đã đóng băng.

## 3. Phần thưởng thật sự: `ARCH-CONST-01` nhận được `"MOCK"`

Test `ARCH-CONST-01` đã cấm viết literal `"LAB_REAL_SIM"` và `"PRODUCTION_REAL"` từ trước — nhưng
**`"MOCK"` bị bỏ ra ngoài**, chính vì nó nhập nhằng. Danh sách thiếu đó là dấu vết của phát hiện S2,
nằm ngay trong test.

Tách xong thì nó vào được, với ngoại lệ đúng **ba** file định nghĩa. Đã làm cho đỏ thật: trả một site
về literal → `Failed: 1`, nêu đúng `PostgresSchedulerStore.cs` và dòng.

## 4. Một chỗ spec lệch code, tìm thấy khi phân loại

Để biết `adapter_mode` thuộc nhóm nào, tôi phải tra từ vựng của nó. Kết quả:

| Nguồn | Giá trị |
| --- | --- |
| Code (`AdapterMode`, `ProviderName`) | `MOCK`, `VENDOR`, `ASTERISK_ARI` |
| `specs/database/02-tables.md:174` | **`MOCK/REAL`** |
| `specs/database/06-migration-plan.md:26` | `adapter_mode=REAL` |

`REAL` là giá trị **không đường code nào sinh ra hoặc chấp nhận**, còn `VENDOR` và `ASTERISK_ARI`
không có trong spec.

**Không tự chọn bên** — đây là quyết định của owner về từ vựng cột, không phải refactor. Cái lượt này
làm là khiến bất đồng đó **nhìn thấy được**: nó được ghi trong `<summary>` của `SimAdapters`, thay vì
nằm rải trong những literal không grep ra nghĩa.

Đáng chú ý: chỗ lệch này tồn tại được **chính vì** magic string. Khi cả ba khái niệm cùng viết
`"MOCK"`, không ai đọc ra được cột này có từ vựng riêng để mà đối chiếu với spec.

## 5. Kết quả

```
Unit         670/670
Integration  283/283
Contract      24/24
Chaos          8/8
             ─────────
             985/985
```

`dotnet build Ivr.sln` — 0 warning, 0 error. Gate sweep **37/39 run**, 2 FAIL — cả hai là pin
handover của agent khác, đỏ từ trước lượt này.

Cascade pin: `IvrOptionsValidator.cs` được pin bởi
`docs/review/2026-09-07-m8-independent-full-review.evidence.json` — **bản ghi review đóng băng**,
không gate nào đọc (đã kiểm bằng `git grep` trong `deploy/` và `tools/` ở `W-0269`). Giữ nguyên, để
lệch đúng như thiết kế.

## 6. Còn lại

S2 đóng phần kỹ thuật. Còn **một câu hỏi mới cho owner**, sinh ra từ chính lượt này:

- **`adapter_mode` dùng từ vựng nào?** Spec nói `MOCK/REAL`; code nói `MOCK/VENDOR/ASTERISK_ARI`.
  Sửa spec cho khớp code, hay đổi code cho khớp spec? Nếu chọn spec thì `ASTERISK_ARI` — adapter lab
  của `W-0104` — mất chỗ đứng.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN
