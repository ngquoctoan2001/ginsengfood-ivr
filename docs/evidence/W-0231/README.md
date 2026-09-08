# W-0231 — Thu lại 12 đoạn, và hai chuỗi ràng buộc tách được

Ngày: 2026-09-08 · Baseline: `main@42aa639` · Trạng thái: **TESTS_PASS**.

Owner chốt quyết định 2/4 của `m8-16 §9`: **thu lại 12 đoạn cố định bằng giọng người**. VieNeu biến
mất **hoàn toàn** — không chỉ khỏi runtime mà khỏi cả audio giao đi.

## 1. Kiểm trước khi ghi: cái gì gãy?

Câu hỏi thật là *"12 file này đang được ghim ở đâu"*. Đi kiểm thì có **hai** chuỗi, và chúng **tách
rời nhau** — đó là điều quan trọng nhất lượt này.

### Chuỗi 1 — phát lại: khoá theo **văn bản**, không theo giọng

```csharp
// TargetV1SpeechPolicy.FixedSegmentHashes — băm text của từng đoạn cố định
.Select(segment => SpeechSegment.ComputeTextHash(segment.Text))
```

```json
{ "TextHash": "4612fb85a1d0c3df431aa831b7fe2ec8deeae0a689f6ffc21237665af9aae8b2",
  "MediaReference": "sound:ivr-seg-north-4612fb85a1d0c3df",
  "DurationMilliseconds": 6400 }
```

Văn bản không đổi ⇒ `TextHash` không đổi ⇒ `MediaReference` không đổi ⇒ `ValidateSegmentation`
không đỏ. **Thay nội dung WAV, giữ nguyên tên file.**

| Phải cập nhật | Không đụng |
| --- | --- |
| bytes 12 file `ivr-seg-*.wav` | `TextHash`, `MediaReference`, tên file |
| `SHA256SUMS` | template, `FixedSegmentHashes`, cấu hình catalog |
| `DurationMilliseconds` mỗi entry | cơ chế `Segmentation` |

Danh sách ngắn hơn nhiều so với dự đoán ban đầu. Thiết kế khoá-theo-văn-bản trả công đúng lúc này.

### Chuỗi 2 — provenance VieNeu: khoá theo **model + preset**

```text
tts-provenance-gate.mjs:160         throw "voice manifest drift"
tts-voice-acceptance-lib.mjs:19     voice_manifest_sha256 → voices_v3_turbo.json
b3-telephony-evidence-validator:22  ghim voice-acceptance-manifest.json = 90927e16…
```

Chuỗi này thành **đồ thừa** — nhưng vẫn **xanh**, vì vẫn mô tả đúng artifact đang nằm trong repo.
Chỉ là artifact đó thôi được giao đi.

> **Đừng gỡ vội.** Đây chính là bộ máy đã bắt `2a4f45d` tự ký `legal_gate = PASS`, và `W-0225` vừa
> vá nốt nửa còn hở của nó. Gỡ ẩu thì mất luôn phần chống tự ký. Decommission là **work item
> riêng**, làm có chủ đích.

## 2. Chữ ký giọng `28/08` hết hiệu lực

`Ngọc Linh` / `Ngọc Trân` / `Mỹ Duyên` là **preset của model, không phải người**. Thu bằng giọng
người thì phải chọn lại — lần này là chọn **người**.

| Đóng | Mở |
| --- | --- |
| `L1` licence không có file LICENSE | **`L8`** — hợp đồng giọng người |
| `L2` training data không công bố | |
| `L3` quyền thương mại 3 preset | |
| `L4` attribution / NOTICE | |

**Bốn đóng, một mở** — và câu mở là câu Legal quen thuộc hơn nhiều câu về training data của model.

Điều khoản dễ quên nhất, đã ghi vào `L8`: **thu bổ sung khi catalog đổi.** `~40` tên hàng hôm nay
không phải `~40` tên hàng sang năm. Hợp đồng không có điều khoản đó thì bank D chết đúng lần Sales
thêm sản phẩm đầu tiên.

## 3. Chỉ đóng khi audio mới đã thay xong

`L1`–`L4` **rút khi 12 file mới thay xong**, không phải khi quyết định được ký. Tới lúc đó 12 đoạn
đang giao **vẫn là bản VieNeu render**. Nếu việc thu không diễn ra, cả bốn sống lại nguyên vẹn.

Cùng lý do đó, phiếu Security và mục `INF-A` được sửa để nói *"đừng ký, đừng đóng"* thay vì
*"đã đóng"* — đóng sớm bằng một tiền đề chưa thành sự thật là cách một hồ sơ trở nên sai.

## 4. Một mối nối bị xoá mà không phải trả giá

Nếu giữ bản VieNeu cho đoạn cố định và thu người cho đoạn động thì mỗi cuộc gọi có một **chuyển
timbre** ngay tại mối nối — model đọc câu chào, người đọc tên hàng. Thu lại cả 12 xoá hẳn nguy cơ
đó.

Nên `§3.2` của `today-03` — 6 cuộc MicroSIP — nay kiểm **hai** thứ: chất lượng mối nối, và tính
đồng nhất giọng. Cái thứ hai giờ là *"phải giống nhau vì cùng một người"*, dễ đánh giá hơn hẳn.

## 5. Và một sợi nối sang capacity

`DurationMilliseconds` của đoạn cố định cộng vào thời lượng cuộc gọi — đúng con số `W-0008` sẽ đo và
`CAP-DRIFT-05` đang ghim (`40/35/50/60`). Thu xong thì có **phép đo thật cho phần cố định**.

Không thay được `W-0008` — đó vẫn là cuộc gọi thật đo đầu-cuối — nhưng là dữ liệu vào cho nó, và là
lần đầu một trong bốn con số có nguồn không phải phỏng đoán.

## 6. Kiểm chứng

```text
gate-status.mjs                GATE_STATUS_PASS — 229 work items
contract-freeze-verifier.mjs   CONTRACT_FREEZE=PASS
docs-selftest.mjs              API_DOCS_SELFTEST_PASS
tts-provenance-gate --selftest 10 mutation PASS — chưa gỡ gì, vẫn xanh
today-03 thân gói              git hash-object = 2f6c951f… (không đổi sau P.7)
```

`dotnet test` không chạy lại: không file `.cs`/`.mjs`/`.py` nào đổi. Không sửa code, không gỡ gate
nào, không đụng `MODELS.lock`. `REAL_CUSTOMER_CALL_ALLOWED=NO`.

## 7. Còn lại — hai câu

| # | Việc | Ai |
| ---: | --- | --- |
| **3b** | món chưa có clip xử lý sao · ai báo khi catalog đổi | Owner + M3 + Vận hành |
| 4 | danh sách đơn vị và vùng giao | Vận hành |

Cả hai đổi **kích thước** bank, không đổi hướng. Sau đó là buổi thu — gộp `107` clip số + `12` đoạn
cố định + đơn vị + tên hàng + vùng giao vào **một session mỗi giọng**, thay vì hai lần vào phòng thu.
