# Phiếu yêu cầu — gửi Platform / Infra

**Chủ đề:** Ba đầu vào hạ tầng đang chặn phần còn lại của SIP-02 và SIP-04
**Người gửi:** Team IVR — Order Confirmation
**Ngày lập:** `2026-09-15` · **Mốc mã:** `main@ff22227`
**Trạng thái:** `READY_TO_DISPATCH / NOT_SENT`
**Ưu tiên:** P1 cho mục A và C; P2 cho mục B

> Ba mục độc lập, trả lời được mục nào thì đóng mục đó. Phiếu này **không** xin gì về SIM, trunk hay
> nhà mạng — việc đó có phiếu riêng và đang chờ nhà mạng.

---

## Bối cảnh chung

Phần điều phối đồng thời và sở hữu ARI controller đã xong và chạy thật (6 commit, `56e3213…b817fae`;
[bàn giao](sip-05-concurrency-handoff-2026-09-15.md)). Mọi việc còn lại **đều chờ một quyết định bên ngoài**.
Ba trong số đó là của Platform.

---

## Mục A — Nguồn khoá cho token protector production (P1)

### A.1 Hiện trạng

`IOpaqueValueProtector` hiện chỉ có bốn hiện thực, **không cái nào dùng được cho production**:

| Hiện thực | Làm gì |
| --- | --- |
| `MockOnlyOpaqueValueProtector` | fake |
| `UnavailableOpaqueValueProtector` | ném lỗi |
| `LabDialTokenVault` | băm SHA-256, **không đảo ngược được** |
| `MockDialTokenVault` | fake |

Lab băm là đúng cho lab — nó chỉ cần một fingerprint ổn định. Production thì **phải giải ngược được**,
vì cuối đường phải ra số E.164 thật để quay.

Nửa bền vững của ledger đã xong (`eb7e781`): trần resolve giờ sống qua restart và dùng chung giữa tiến
trình. Cái còn thiếu là protector.

### A.2 Cần chính xác bốn thứ

| # | Cần gì | Vì sao |
| --- | --- | --- |
| A.1 | Nguồn khoá nào (KMS nào, hay secret store nào) | IVR **không tự chọn** — chọn sai là chọn thay Platform |
| A.2 | Cơ chế **key version** và rotation | Token đã phát phải giải được sau khi rotate; cần biết rotate kiểu nào |
| A.3 | Điều gì xảy ra khi **mất khoá** | Token không giải được nữa = đơn không gọi được. Cần biết đây là chấp nhận được hay phải có escrow |
| A.4 | Xoá khoá theo retention | Token hết hạn rồi thì khoá còn giữ bao lâu |

### A.3 Ràng buộc IVR phải giữ

- Mã hoá **có xác thực** (AEAD), **tách mục đích** (`purpose`), có key version trong ciphertext.
- Số E.164 chỉ sống trong bộ nhớ của biên telephony. **Không** ghi vào DB ứng dụng, log, evidence, callback.
- Token lab đã băm **không** chuyển thành token production được — tách hẳn dữ liệu và môi trường.

---

## Mục B — Xác nhận cấu hình triển khai worker (P2)

Ba điều dưới đây đã nằm trong chart tại `c061001`, cần Platform **xác nhận là đúng ý** chứ không phải
xin cấp gì:

| # | Đã đặt trong chart | Vì sao |
| --- | --- | --- |
| B.1 | `worker.replicas: 1`, HPA worker tắt | Trên tuyến Asterisk, **đúng một** worker được giữ application. Replica thừa không chạy chậm — chúng **không quay số**. Asterisk giao application cho socket kết nối sau cùng và chỉ báo cho bên thua sau khi đã lấy |
| B.2 | `strategy: Recreate` | Pod cũ phải biến mất trước khi pod mới khởi động |
| B.3 | `terminationGracePeriodSeconds: 210` | Drain là 180s, phải phủ hết một cuộc gọi (audio 120s + ring 30s + DTMF 15s). Grace **ngắn hơn drain** thì SIGKILL rơi giữa drain: cúp máy giữa chừng với khách, và bỏ qua bước trả quyền ARI → pod sau cần **người** can thiệp |

Nếu Platform có ràng buộc khiến B.3 không đặt được, cho biết **trần thực tế** là bao nhiêu giây — con số
đó quyết định `DispatchDrainSeconds`, và validator đang ép `ControllerLeaseSeconds >= DispatchDrainSeconds`.

---

## Mục C — Định danh một bản triển khai (P1)

### C.1 Vấn đề

`PostgresProductionCallGate` giờ đã siết theo **environment** (`b817fae`): một approval cấp cho `pilot`
không còn mở `prod`. Nhưng kế hoạch đòi environment **và candidate** — tức approval phải gắn với **đúng
bản triển khai** được duyệt, không phải mọi bản trong cùng môi trường.

**Không làm được, vì không có gì định danh một bản triển khai.** `IvrTelemetry.Version` là hằng số
`"1.0.0"`. Không có image digest, release ref hay build SHA nào được nối tới runtime.

### C.2 Cần chính xác ba thứ

| # | Cần gì | Ghi chú |
| --- | --- | --- |
| C.1 | **Cái gì** định danh một bản triển khai: image digest? release ref? cả hai? | Phải là thứ **không đổi được sau khi duyệt** — một tag di động thì approval bám vào không có nghĩa |
| C.2 | **Cách nào** nó tới được runtime | Biến môi trường từ chart? Downward API? |
| C.3 | Ai cấp nó và ở bước nào của pipeline | Để approval ký được **trước** khi triển khai |

### C.3 Vì sao không tự làm

Đặt đại một định danh ở phía IVR là **bịa ra chính hợp đồng release đang chờ**. Approval sẽ gắn vào một
thứ mà pipeline không biết, và cổng cuối cùng trước khi khách thật nghe chuông sẽ bám vào một con số
không ai bảo đảm.

Dòng chặn ở [`IvrOptionsValidator.cs:53`](../../src/Ivr.Infrastructure/Configuration/IvrOptionsValidator.cs:53)
(`REAL_CUSTOMER_CALL_ALLOWED must remain NO`) **vẫn giữ nguyên** và không phụ thuộc phiếu này.

---

## Cái gì mở ra khi có trả lời

| Mục | Mở khoá |
| --- | --- |
| A | Nửa trên SIP-02 — resolver số thật; sau đó mới tới E2E có số thật |
| B | Xác nhận profile triển khai; nếu B.3 không đạt thì phải tính lại drain |
| C | Phần candidate của SIP-04 — approval bám đúng bản triển khai |

Không mục nào trong phiếu này chặn việc chờ nhà mạng, và ngược lại. Hai luồng độc lập.
