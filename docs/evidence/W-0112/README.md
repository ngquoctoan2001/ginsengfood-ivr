# W-0112 — Seed loader / scenario runner / integration-status profile (UI-07)

Ngày: `2026-08-23`
Baseline: `main@31242a5`
Trạng thái: `TESTS_PASS`
Plan: [`remaining-work-plan-2026-08-22.md` §A5](../../../plan/ivr-orther/remaining-work-plan-2026-08-22.md)

> `REAL_CUSTOMER_CALL_ALLOWED` vẫn `false` ở cả bốn môi trường. Toàn bộ bản này **không tồn tại**
> ở production — không phải bị chặn, mà là không được đăng ký.

---

## 1. Vấn đề đã đóng

`vi.json` ghi: *"Chưa có API nạp seed hay chạy scenario. Việc này hiện thực hiện qua CLI/SQL"*,
trong khi `specs/ui/07` khai ba action.

Lịch tháng 9 có hai đợt nghiệm thu và một đợt chạy thử toàn tuyến. Mỗi lần dựng lại tình huống
demo bằng SQL tay là một lần có thể dựng sai, và người nghiệm thu không tự bấm lại được.

---

## 2. Ba điểm làm khác bản draft — và lý do

### 2.1 Quyền riêng, không dùng lại quyền SIM

`specs/ui/07` đề xuất `IVR_SIM_ENABLE`/`IVR_SIM_DISABLE`. Nạp seed và chạy scenario không phải
thao tác SIM; gộp lại nghĩa là một operator được phép tắt kênh hỏng cũng ghi được dữ liệu vào cơ
sở dữ liệu. Đã thêm `IVR_DEV_TOOLING`, **chỉ Admin**, và đưa vào `ConsoleSessionOnly` — seam MOCK
cấp bất cứ quyền nào header xin, còn MOCK lại đúng là chế độ mọi môi trường non-prod đang chạy.

### 2.2 Production trả `404`, không phải `403`

Route **không được đăng ký**. `403` sẽ xác nhận với người gọi rằng ở địa chỉ này có một seed
loader và chỉ còn một cái quyền chắn giữa họ với nó. `404` không nói gì, và nó đúng theo nghĩa đen.

Điều kiện phục vụ là **danh sách cho phép**: tên môi trường ∈ `{Development, Testing, Test,
Staging, Lab}`, mode ∈ `{MOCK, LAB_REAL_SIM}`, `REAL_CUSTOMER_CALL_ALLOWED = NO`. Mỗi điều kiện tự
nó đủ để từ chối. Quên cập nhật danh sách khi thêm môi trường mới ⇒ **mất công cụ dev**, không
phải mở seed loader vào một môi trường không ai kiểm.

Kiểm **hai lần**: lúc đăng ký route và lại trong service. Cái thứ hai phòng đúng một tình huống —
một thay đổi sau này thêm route hoặc caller mà quên chốt.

### 2.3 "Áp profile" chỉ thi hành được một phần

Bốn trong năm phụ thuộc (`ORDER_CORE`, `OPS_SELLABLE_GATE`, `CRM_DO_NOT_CALL`,
`EVIDENCE_REGISTRY`) báo `NOT_WIRED` vì IVR **không thăm dò** chúng. Không có gì trong hệ đang
chạy để một profile bật/tắt. Chỉ `SIM_GATEWAY` là thật, và nó được thi hành bằng cách gọi lại
đúng lối bật/tắt kênh sẵn có — nên chốt fail-closed lúc bật lại vẫn nguyên, thay vì có bản sao
thứ hai để lệch dần.

Phản hồi tách `enforced` khỏi `declared_only`, và màn hình cảnh báo **trước khi bấm**. Một màn
hiện năm phụ thuộc như đã áp cả năm là một người vận hành rời đi với niềm tin rằng vừa diễn tập
xong một lối fail-closed không hề chạy.

---

## 3. Hai thứ phát hiện khi triển khai, không đoán trước được

### 3.1 File mẫu đã hết hạn từ 11 ngày trước

`sales-target-v1.sample.json` ghi mốc tuyệt đối 12/8/2026 với cửa sổ 5–15 phút. Nạp nguyên trạng
⇒ **cả 9 tác vụ bị từ chối** `ORDER_NOT_CALLABLE_OR_WINDOW_EXPIRED`. Một loader trung thành với
file sẽ chạy xong và không giao được gì — đúng thứ buổi nghiệm thu tháng 9 cần.

Loader dời cửa sổ **từng tác vụ** về hiện tại. Dời chung một khoảng giữ nguyên độ lệch 2 giờ 20
phút của file, tức mỗi lúc chỉ đúng một tác vụ gọi được. Độ lệch đó là cách file mô tả một dòng
thời gian để **phát lại**, không phải hình dạng một môi trường demo cần có. Cái bị mất được ghi
trong phản hồi, không để ai tự phát hiện. Tắt được bằng `rebase_windows: false`.

### 3.2 Nạp tác vụ mà không nạp thứ nó phụ thuộc

Trên cơ sở dữ liệu mới, cả 9 tác vụ trả `TASK_HELD_POLICY_MISSING`: file khai `mock-lab-v1`, và
tiếp nhận **tra registry** chứ không tin thân bản tin. Loader giờ đăng ký sẵn candidate attempt
policy (chỉ MOCK/LAB).

---

## 4. Đối chiếu tiêu chí nghiệm thu

| Tiêu chí (plan §A5) | Test | Kết quả |
| --- | --- | --- |
| Gọi ở production ⇒ `404`, có test | `IT-DEV-PRODGUARD-01` | ✅ cả ba route, và **không** ghi gì |
| Dry-run không phát cuộc gọi nào | `IT-DEV-DRYRUN-05` + `UT-DRYRUN-01` | ✅ đếm bằng dòng dữ liệu, và chứng minh bằng cấu trúc |
| Seed vào prod ⇒ không thể, có test | `IT-DEV-PRODGUARD-01/02` | ✅ số tác vụ = 0 sau khi bị từ chối |
| Giữ khoá `adapter_mode=REAL` | — | ✅ không thêm control nào; màn vẫn chỉ đọc cho mục này |

### Về "dry-run không phát cuộc gọi"

Đếm dòng chứng minh **lần chạy này** không gọi. Cái đáng tin hơn là cấu trúc:
`CallScenarioDryRun` nằm ở `Ivr.Domain`, chỉ phụ thuộc `DispositionMapper`, không giữ cổng
telephony nào. Không có mã gọi điện trên lối đó để mà tắt. `UT-DRYRUN-01` khoá tính chất ấy bằng
reflection thay vì bằng một lần chạy mẫu.

### Về `NOT_REPLAYABLE`

`SCN-007` mong đợi `IVR_CONFIRMATION_WINDOW_EXPIRED` từ một lần `ring_timeout`. Bộ chuẩn hoá
disposition **không bao giờ** sinh ra kết quả đó — luồng quét hết hạn mới sinh. Báo "lệch" sẽ đẩy
người đọc đi tìm lỗi ở sai file, nên runner trả `NOT_REPLAYABLE` và **không đưa phán quyết**.
Tập kết quả phát lại được suy ra từ chính đầu ra của mapper, không phải từ một danh sách tên
scenario chép tay.

---

## 5. Điều loader **không** đi vòng qua

`TASK-TARGET-247-0005` mang `call_restriction: true` và `BLOCKED_DO_NOT_CALL`. Nó trở về
`IVR_OPERATIONAL_BLOCKED` — 8/9 được nhận, và cái thứ 9 là điểm chính. Loader đi qua
`ITaskIntakeService` chứ không ghi thẳng dòng dữ liệu; một seed loader có thể đưa khách đã từ
chối nhận cuộc gọi vào hàng đợi sẽ là tiện lợi đắt nhất trong repo này. `IT-DEV-SEED-03` khoá nó
theo tên.

---

## 6. Kết quả kiểm chứng

| Suite | Kết quả |
| --- | --- |
| `Ivr.UnitTests` | **470 / 470** (+21) |
| `Ivr.IntegrationTests` | **249 / 249** (+11) |
| `Ivr.ContractTests` | **22 / 22** |
| `Ivr.ChaosTests` | **8 / 8** |
| **Tổng .NET** | **749 / 749** |
| admin-ui | lint + `tsc` + **219 / 219** (+1) + build |
| Traceability | **455** tagged test |
| `dotnet format --verify-no-changes` | PASS |

OpenAPI `draft.14 → draft.15`: +3 path, +8 schema, re-pin manifest, sinh lại portal.

### Guard bắt được đúng thứ nó sinh ra để bắt

- `UT-L10N-COVER-03` (W-0107) chặn vì enum `coverage` chưa có nhãn tiếng Việt ⇒ thêm họ
  `scenarioCoverage`, không xin miễn trừ.
- `UI-I18N-02` chặn vì `seed.loaderUnavailable` thành khóa mồ côi sau khi câu "chưa có API" hết
  đúng ⇒ xoá khóa.
- `IT-ACCOUNT-*` chặn vì tập quyền Admin được ghim theo danh sách chính xác ⇒ thêm
  `IVR_DEV_TOOLING` **tường minh**, kèm lý do, đúng như test đó tồn tại để buộc.
- `UT-UI-SEED-PROD-03` đỏ vì nó khẳng định *"console không có lối ghi dữ liệu"* — câu W-0112 cố ý
  đảo ngược. Đã viết lại để khoá cái phải đúng **sau** thay đổi (người vận hành được báo trước
  loader sửa gì, và chạy lại không làm mới gì), không nới lỏng.

---

## 7. Những gì bản này **không** làm

- **Không đổi được `adapter_mode`.** Sang `REAL` cần mua SIM (DT-01) + release gate (DF-03); vẫn
  chỉ đọc, đúng như `specs/ui/07` §P0.
- **Không làm mới cửa sổ khi nạp lại.** Lần hai báo `IVR_IDEMPOTENCY_CONFLICT` từng tác vụ và
  không thêm gì; muốn cửa sổ mới thì dựng lại cơ sở dữ liệu. Ghi ở màn hình, không giấu.
- **Không diễn tập được fail-closed của 4/5 phụ thuộc.** Cần probe thật — `W-0040`.
- **Không hiển thị kết quả chi tiết trên màn.** Console hiện tóm tắt (`8/9`, `REPLAYED`); bảng
  từng tác vụ và từng lần gọi có trong phản hồi API nhưng chưa được render.
- **Không có e2e chạy console và API thật cùng lúc.** Hai lớp được chứng minh riêng, như W-0110
  và W-0111.
- **Chưa chạy trên môi trường staging thật.** Toàn bộ chạy trên host `Testing` + PostgreSQL thật.
