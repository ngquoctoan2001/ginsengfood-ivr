# W-0110 — Màn hình cổng vận hành (feature flag / runtime gate)

Ngày: `2026-08-23`
Baseline: `main@37d84dc`
Trạng thái: `TESTS_PASS`
Plan: [`remaining-work-plan-2026-08-22.md` §A3](../../../plan/ivr-orther/remaining-work-plan-2026-08-22.md)
Nối tiếp: `OD-V1-20`

> `REAL_CUSTOMER_CALL_ALLOWED` vẫn `false` ở cả bốn môi trường. Bản này **không** đổi giá trị
> cờ nào; nó dựng chỗ để bấm và các chốt quanh việc bấm.

---

## 1. Vấn đề đã đóng

`OD-V1-20` chuyển ràng buộc P0 từ *"không vai trò nào giữ quyền này"* sang *"mỗi lần bấm phải có
four-eyes, actor khớp subject, và audit"*. Nhưng không có màn nào để bấm, nên cách duy nhất là
gọi API bằng `curl` — tức là đúng lối **không** có four-eyes ở tầng người dùng, và không có gì
nói cho người bấm biết họ đang đẩy rủi ro theo chiều nào.

Màn này không phải tiện lợi. Nó là chỗ luật bất đối xứng của `specs/api/03` được **nhìn thấy**.

---

## 2. Phạm vi đã triển khai

### 2.1 `/flags` — chỉ Admin

`requireAdmin()` phía server. Operator không nhận được một trang bị ẩn nút mà **không nhận được
trạng thái cổng nào cả** — trạng thái kill switch tự nó đã là thông tin vận hành.

### 2.2 Đọc fail-closed

Hai lệnh đọc, và thứ tự có chủ đích:

| Lệnh | Khi provider hỏng |
| --- | --- |
| `GET /feature-flags/{env}/kill-switch` | trả `providerReadable:false`, **không** ném lỗi |
| `GET /feature-flags/{env}` | **ném** `IVR_OPERATIONAL_BLOCKED` |

Nên ô trạng thái hiệu lực dựng từ lệnh thứ nhất. Không đọc được ⇒ hiển thị **ĐANG BẬT**, kèm
cảnh báo nói rõ đây là giả định an toàn chứ không phải giá trị đã đọc. Ô trống hay chữ "đã nhả"
lúc đó sẽ đọc thành "không có gì đang chặn cuộc gọi" — sai theo đúng chiều nguy hiểm nhất.

`realCallsEnabled` cũng lấy từ lệnh thứ nhất chứ không từ snapshot, vì khi snapshot không đọc
được thì câu trả lời trung thực vẫn là "không".

### 2.3 Hai chiều, hai nhóm nút

| Chiều | Thao tác | Cần gì |
| --- | --- | --- |
| Giảm rủi ro | bật kill switch · thu hồi quyền gọi khách thật · xoá rỗng allowlist | chỉ `reason` |
| Tăng rủi ro | nhả kill switch · mở rộng allowlist | `reason` **+ mã tham chiếu phê duyệt** |

Gộp chung một nhóm sẽ đặt "bật" cạnh "nhả" như thể chúng cùng trọng lượng. Chúng không cùng:
một cái luôn được phép, cái kia cần người thứ hai. Bố cục là chỗ đầu tiên người vận hành gặp sự
bất đối xứng đó, nên bố cục phải mang nó.

Mã phê duyệt là chuỗi **đục** do máy chủ xác minh, không phải tên người duyệt do chính người
thao tác gõ vào — máy chủ từ chối nếu người duyệt trùng actor.

### 2.4 Chặn theo **môi trường**, không chỉ theo execution mode

Chiều tăng rủi ro bị chặn hẳn khi môi trường không thuộc allowlist non-production, **hoặc** khi
execution mode là `PRODUCTION_REAL`.

Chỉ chặn theo mode là chưa đủ: đổi mode *sang* `PRODUCTION_REAL` **tự nó** là một thay đổi tăng
rủi ro, và ở deployment production nó vẫn với tới được trong khi mode còn là `MOCK`.
`isNonProductionEnvironment` là allowlist nên một nhãn lạ sẽ **khoá** chứ không mở.

Chiều giảm rủi ro vẫn dùng được ở production. Sự cố là đúng lúc người ta cần dừng quay số.

Đây là một lỗi tôi tự tạo rồi tự sửa: bản đầu chặn theo `executionMode`, và e2e ở môi trường
production đỏ vì server đó chạy `MOCK`. Giả định sai lộ ra qua test chứ không qua suy luận.

---

## 3. Phát hiện: một chốt an toàn hiện **không** có hiệu lực

`FeatureFlagAdminService.RejectSelfAuthorization` từ chối actor thêm chính đích mình sắp gọi vào
allowlist. Nhưng nó bắt đầu bằng:

```csharp
if (string.IsNullOrWhiteSpace(command.ActorDestinationReference)) { return; }
```

Và `ActorDestinationReference` lấy từ claim `ivr_destination_ref`, mà claim đó **chỉ** do
`MockPermissionAuthenticationHandler` cấp (từ header `X-Mock-Destination-Ref`).
`ConsoleSessionAuthenticationHandler` cấp bảy claim, không có claim này.

⇒ **Với mọi phiên đăng nhập console, chốt này im lặng không chạy** — đúng nhóm actor mà
`OD-V1-20` vừa trao quyền mở allowlist.

Bản này **không** tự vá, vì vá đúng cần một ánh xạ tài khoản → đích gọi, và đó là quyết định mô
hình dữ liệu của owner chứ không phải của màn hình. Việc màn hình làm được là nói thẳng: cảnh báo
trên form ghi rõ máy chủ hiện không tự chặn được, và người duyệt thứ hai là chốt duy nhất còn
hiệu lực ở đây. Hiển thị một cảnh báo như thể nó được thi hành sẽ tệ hơn không có.

Đề xuất mở `OD-FLAG-01`: cấp claim đích cho phiên console, hay chấp nhận four-eyes là chốt duy nhất.

---

## 4. Đối chiếu tiêu chí nghiệm thu

| Tiêu chí (plan §A3) | Test | Kết quả |
| --- | --- | --- |
| Operator vào `/flags` ⇒ không render dữ liệu, không chỉ ẩn nút | `E2E-UI-FLAGS-06` | ✅ không có `kill-switch-state`, không có `LAB-A` |
| Mutation tăng rủi ro thiếu four-eyes ⇒ **UI chặn và API trả lỗi** | `UT-FLAGS-ASYMMETRY-01` + `IT-FLAG-FOUREYES-14` | ✅ cả hai lớp, xem §5 |
| `REAL_CUSTOMER_CALL_ALLOWED` vẫn `false` ở cả 4 môi trường | `values-{dev,staging,lab,prod}.yaml` | ✅ không file nào bị đụng |
| E2E: bật kill switch từ UI ⇒ audit có bản ghi với actor khớp subject | `UT-FLAGS-ASYMMETRY-01` + `IT-FLAG-EMERGENCY-10` | ⚠️ tách hai lớp, xem §5 |

---

## 5. Hai lớp, và cái mỗi lớp thật sự chứng minh

Tiêu chí 2 và 4 đều đòi "cả hai lớp", nên phải nói rõ lớp nào chứng minh cái gì.

**Lớp console** — `UT-FLAGS-ASYMMETRY-01`: action từ chối thay đổi tăng rủi ro thiếu mã phê
duyệt **trước khi** chạm mạng. Client API bị mock để **ném lỗi**, nên "chưa gọi API" là một
assertion chứ không phải một giả định. Có thêm ca thuận: giảm rủi ro đi lọt, mang đúng change
set nó khai và đúng `reason`, và **không** kèm mã phê duyệt — nếu thiếu ca này thì ba ca từ chối
kia sẽ xanh y hệt kể cả khi action không bao giờ gọi gì.

**Lớp API** — `IT-FLAG-FOUREYES-14` (mới): ở LAB, nơi tăng rủi ro **có** lối đi, nó chỉ đi
được khi có phê duyệt đã xác minh. `IT-FLAG-PRODGUARD-07` sẵn có chỉ chứng minh production chặn
hẳn — mà một test không bao giờ cho thay đổi lọt qua thì vẫn xanh kể cả khi phần kiểm phê duyệt
không tồn tại. Test mới cũng khẳng định kill switch **vẫn bật** sau một mutation bị từ chối:
từ chối không được đẩy cổng đi nửa chặng.

**Về audit (tiêu chí 4).** Console không ghi audit; Ivr.Api ghi. Nên phần console chứng minh
được là **request nó gửi** — `X-Actor-Id` do `callIvrApi` gắn từ subject đã đăng nhập, và máy chủ
trả 403 khi lệch (`FeatureFlagEndpoint.ExecuteMutationAsync`). Việc dòng audit thật sự được ghi
là `IT-FLAG-EMERGENCY-10`, chạy trên cơ sở dữ liệu thật. Không có e2e nào chạy console **và**
API thật cùng lúc, nên bản này **không** khẳng định một chuỗi liền mạch UI → audit row.

---

## 6. Kết quả kiểm chứng

| Suite | Kết quả |
| --- | --- |
| `Ivr.UnitTests` | **449 / 449** |
| `Ivr.IntegrationTests` | **235 / 235** (+1 mới) |
| `Ivr.ContractTests` | **22 / 22** |
| `Ivr.ChaosTests` | **6 / 6** |
| **Tổng .NET** | **712 / 712** |
| admin-ui | lint + `tsc` + **217 / 217** (+7 mới) + build |
| `dotnet format Ivr.sln` | PASS |

---

## 7. Những gì bản này **không** làm

- **Không đổi backend feature-flag.** Ba endpoint, guardrail và four-eyes đều đã có; W-0110 chỉ
  thêm một test cho nhánh chưa được phủ.
- **Không đổi giá trị cờ nào**, ở môi trường nào.
- **Không vá chốt tự-duyệt-đích** (§3). Cần quyết định của owner.
- **Không có ảnh chụp màn hình.** Cần stack có dữ liệu.
- **Không có màn đổi `executionMode`, `salesProvider`, `simProvider`, `attemptPolicyVersion`.**
  Bốn cờ đó đọc được trên màn nhưng không sửa được từ đây. Có chủ đích: chúng là thay đổi mức
  deployment, và một form console cho chúng sẽ mời người ta đổi chế độ chạy giữa ca trực.

---

## 8. Lệnh tái lập

```bash
dotnet test tests/Ivr.IntegrationTests/Ivr.IntegrationTests.csproj --filter "FullyQualifiedName~FeatureFlagApiTests" --nologo
npm --prefix admin-ui run lint
npm --prefix admin-ui test
npm --prefix admin-ui run build
```
