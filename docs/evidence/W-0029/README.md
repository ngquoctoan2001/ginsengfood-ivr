# W-0029 — Evidence: Sales provider wiring and contract verification (`P4-1`)

Ngày: `2026-08-18` · Trạng thái đạt được: `TESTS_PASS` (mock-only — trần của slice này theo DoD `P4-1` §5)

## 1. Rà soát trước khi viết: phần lớn §3 đã tồn tại

Đọc code trước rồi mới sửa. Sáu mục build của `P4-1` §3 đối chiếu với hiện trạng:

| Mục §3 | Hiện trạng trước slice này |
| --- | --- |
| 1. Profile `FakeSales`/`CurrentGoldenHourCompat`/`TargetV1` + validate ma trận lúc khởi động | **đã có** — `SalesContractSelection` + `CallbackDeliveryOptionsValidator` |
| 2. Bind auth/idempotency/correlation, từ chối vi phạm schema/matrix/privacy/policy | **đã có** — `P2-1` intake |
| 3. Bind callback client + ACK + adapter compat | **đã có** — `P2-6` |
| 4. Service JWT acquisition/cache/refresh + mTLS hook | **đã có** — `W-0032` vừa xong |
| 5. readiness/circuit/timeout/backoff/metrics | **một nửa** — xem §2 |
| 6. CDC chạy được với fake + **ghi nhận provider version/OpenAPI hash** | **thiếu phần ghi nhận** — xem §3 |

Nên slice này không viết lại thứ đã có; nó đóng đúng hai lỗ hổng còn lại.

## 2. Lỗ hổng thứ nhất: tín hiệu readiness có thật nhưng không ai thấy được

`CallbackCircuitBreaker.Snapshot()` **đã** trả về `Readiness` (`READY` / `NOT_READY_CIRCUIT_OPEN` / `NOT_READY_CIRCUIT_HALF_OPEN`), và `CallbackDeliveryJobHost` đã đọc nó. Nhưng tín hiệu đó chỉ sống trong worker: màn integration-status của console vẫn báo `ORDER_CORE` là `NOT_WIRED`, `observed=false`, với dòng mô tả không nói gì.

Giờ card `ORDER_CORE` mang đúng những gì IVR **thật sự biết**: provider profile đang chọn, delivery bật hay tắt, trạng thái circuit, số lần lỗi transient liên tiếp — và ghi thẳng rằng endpoint thật vẫn `BLOCKED_EXTERNAL`.

**Hai ranh giới tôi cố ý không vượt:**

- **Không đụng `/health/ready`.** Nó đang khai `dependencyChecks = NOT_IMPLEMENTED_UNTIL_W-0040` — đó là một hoãn được ghi trung thực, không phải lỗi. Khung probe dependency thuộc `P6-1`/`W-0040`; lấn sang là cướp scope và làm hai work item cùng sở hữu một thứ.
- **`observed` chỉ bật khi delivery thật sự bật.** Delivery tắt thì không có gì đang được quan sát về Order Core, và nói ngược lại chính là cái bệnh placeholder ở một chỗ khác. Circuit của IVR là tín hiệu về **lối ra của chính mình**, không phải sức khoẻ của Sales — báo nó như thể là sức khoẻ Sales sẽ là cùng một lời nói dối trong hình dạng đẹp hơn.

Nhân tiện sửa hai dòng đã cũ: card `CRM_DO_NOT_CALL` còn hứa "No CRM provider wired (P4-3)" trong khi `P4-3`/`W-0031` đã xong và kết luận là IVR **không** giữ CRM client nào; và thông điệp chặn `TARGET_V1` còn ghi "blocked until P4-1/W-0006" — P4-1 giờ xong rồi, blocker thật chỉ còn `W-0006`/`OD-V1-07`.

## 3. Lỗ hổng thứ hai: CDC không ghi lại nó đã verify với bản nào

`P4-1` §3.6 yêu cầu "capture provider version/OpenAPI hash". Bộ `CT-CONTRACT-*` chạy tốt nhưng **không ghi lại** nó pass trên revision nào của provider.

Đó không phải chi tiết hành chính. Một bộ CDC xanh mà không nói mình verify với bản nào thì **sau khi Sales deploy nó chứng minh gì cũng không rõ**: fixture vẫn khớp một hợp đồng có thể không còn ai phục vụ.

`CT-CONTRACT-PINNED-08` biến việc ghi nhận đó thành thứ CI ép:

- baseline commit của Sales (`ginsengfood-business-platform` @ 40 ký tự hex thường) phải có mặt và đúng dạng;
- **mọi** contract mà IVR sinh client/DTO từ đó phải hash khớp `sha256` đã ghim — một sửa đổi upstream không thể lọt qua chỉ vì nó hợp lệ về cú pháp;
- `contractState` phải vẫn là `TARGET_CONTRACT_V1=DRAFT` — một lần chạy xanh **không bao giờ** được đọc thành hợp đồng provider đã duyệt.

## 4. Một lần suýt tạo trùng lặp

Bản đầu tôi viết thêm `CallbackCircuitState` + `Inspect()` cho readiness. Build đỏ ngay: `CS0101 — namespace already contains a definition for 'CallbackCircuitState'`.

Hoá ra `Snapshot()` đã tồn tại, đã có `Readiness`, và worker đã dùng. Tôi gỡ toàn bộ phần vừa viết và dùng cái sẵn có. Nếu compiler không bắt, kết quả sẽ là hai nguồn sự thật về cùng một trạng thái circuit — loại nợ khó thấy và khó gỡ nhất.

Lý do gốc: tôi đã grep `class CallbackCircuitBreaker` để đọc kiểu, nhưng không đọc hết phần public surface của nó trước khi thêm.

## 5. Test

| Test | Khẳng định |
| --- | --- |
| `CT-CONTRACT-PINNED-08` | CDC ghi nhận baseline commit + hash của **cả 3** contract; `contractState` vẫn DRAFT |
| `IT-ADMIN-CONFIG-03` (mở rộng) | card `ORDER_CORE` mang `provider=`, `circuit=` và `BLOCKED_EXTERNAL`; card CRM không còn hứa provider sẽ không bao giờ được wire |
| `CT-CONTRACT-CURRENT-06` | compat vẫn khớp ảnh chụp `a3aad246` |
| `SalesCallbackContractSelector` (P2-6) | compat **không thể** nhận kết quả 24/7 — `P4-1` §4 acceptance mục 2 |

Không thêm test cho những gì đã có sẵn coverage; §1 liệt kê rõ phần nào đã được phủ từ trước.

## 6. Kiểm chứng

| Lệnh | Kết quả |
| --- | --- |
| `dotnet build Ivr.sln -warnaserror` | 0 warning / 0 error |
| `dotnet test Ivr.sln` | xem §8 |
| `docs-selftest.mjs` / `openapi-contract-drift.mjs` | PASS (không đụng OpenAPI) |

## 7. Cái này KHÔNG chứng minh

- **Không đóng `W-0002`…`W-0006`.** Cả năm giữ `BLOCKED_EXTERNAL`.
- **Không bật real provider.** `CallbackDeliveryOptionsValidator` vẫn **từ chối boot** với `TARGET_V1`; thông điệp giờ nêu đúng blocker còn lại là `W-0006`/`OD-V1-07`, không phải P4-1.
- **Real sandbox evidence là hạng mục riêng, `NOT_RUN`.** Không có endpoint, không có credential, nên chưa một CDC nào chạy được với Sales thật — đúng như `P4-1` §4 mô tả.
- **Không có metric backend.** Đếm và đo thuộc `P6-1`/`W-0040`; slice này chỉ đưa tín hiệu sẵn có ra một bề mặt người xem được.
- **`/health/ready` không đổi.** Vẫn khai `NOT_IMPLEMENTED_UNTIL_W-0040`.
- **`TESTS_PASS` là trần.** Chỉ reviewer/owner chuyển `ACCEPTED`.

## 8. Số liệu test

`dotnet test Ivr.sln` — **337/337 pass, 0 fail, 0 skip**:

| Project | Sau W-0032 | Sau W-0029 | Thêm |
| --- | ---: | ---: | ---: |
| `Ivr.ContractTests` | 21 | 22 | +1 |
| `Ivr.UnitTests` | 189 | 189 | 0 |
| `Ivr.IntegrationTests` | 126 | 126 | 0 |
| **Tổng** | **336** | **337** | **+1** |

`IT-ADMIN-CONFIG-03` được **mở rộng** chứ không thay thế: mọi assertion cũ giữ nguyên, thêm assertion mới về nội dung card.

### Lần chạy đầu đỏ 5 test — và vì sao

Bản đầu tôi tiêm `CallbackCircuitBreaker` và `IOptions<CallbackDeliveryOptions>` **bắt buộc** vào `AdminConfigReadService`. Kết quả: mọi route admin-config trả `500`, 5 test đỏ — host test dựng tối giản, không đăng ký callback stack, nên DI không resolve được.

Sửa không phải bằng cách đăng ký thêm dịch vụ vào host test, mà bằng cách **nhận đúng bản chất phụ thuộc**: đây là read service cho màn admin; một host phục vụ console mà không chạy callback delivery **vẫn phải khởi động được**. Hai tham số thành optional với mặc định `null`, và khi vắng thì card `ORDER_CORE` báo `NOT_WIRED` — đúng sự thật của host đó.

Nếu tôi vá bằng cách nhồi callback stack vào host test, test sẽ xanh nhưng ràng buộc sai vẫn còn: API admin sẽ không khởi động được ở một topology mà nó lẽ ra chạy được.
