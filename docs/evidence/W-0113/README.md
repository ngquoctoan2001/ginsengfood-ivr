# W-0113 — Ghi lại giọng đã phát, không suy lại lúc đọc

Ngày: `2026-08-23`
Baseline: `main@64c89c0`
Trạng thái: `TESTS_PASS`
Plan: [`remaining-work-plan-2026-08-22.md` §A6](../../../plan/ivr-orther/remaining-work-plan-2026-08-22.md)

> `REAL_CUSTOMER_CALL_ALLOWED` vẫn `false` ở cả bốn môi trường. Bản này không đổi giọng nào, không
> đổi cách chọn giọng; nó chỉ làm cho câu trả lời "khách đã nghe giọng nào" trở nên có thể kiểm.

---

## 1. Lỗi đã đóng

Kế hoạch W-0106 §5 ghi thẳng: *"`voice_region` là hàm của dữ liệu đã lưu, không phải bản ghi audit
của giọng đã phát … một lần đổi config giữa lúc gọi và lúc đọc sẽ làm hai thứ lệch nhau."*

Đây là loại lỗi **im lặng**. Không có test nào đỏ, không có cảnh báo nào bật. Chỉ là: bản đồ giọng
nằm trong cấu hình, ai đó đổi nó, và từ giây phút ấy mọi evidence cũ bắt đầu mô tả một giọng không
ai từng nghe — kể cả những bản chủ sở hữu đã ký.

---

## 2. Ghi cái gì, ghi ở đâu

### 2.1 Ba cột, và cột thứ ba mới là cột quan trọng

| Cột trên `ivr_call_attempts` | Nghĩa |
| --- | --- |
| `voice_id` | Giọng thật đưa cho TTS |
| `voice_region` | `North` / `Central` / `South` |
| `voice_region_resolved` | `true` = nhận ra tỉnh trong địa chỉ; `false` = dùng giọng mặc định |

Cột thứ ba dễ bị coi là thừa. Nó không thừa: *"Nam vì nhận ra Cần Thơ"* và *"Nam vì Nam là mặc
định"* là hai điều khác nhau, và chỉ điều đầu là bằng chứng **về khách hàng này**. Một evidence
pack không phân biệt được hai điều đó là một evidence pack nói nhiều hơn nó biết.

Ràng buộc CSDL ép **cả ba cùng có hoặc cùng không**, và `voice_region` chỉ nhận đúng ba giá trị —
kiểm ở CSDL chứ không chỉ trong code, để một lệnh ghi thẳng cũng không tạo được vùng thứ tư.

### 2.2 Ghi lúc `MarkActiveAsync`, không phải lúc render

Render xong mà quay số hỏng thì **không có cuộc gọi nào**. Ghi giọng cho một lần gọi chưa từng kết
nối là một khẳng định về việc chưa xảy ra. `MarkActiveAsync` chạy ngay sau khi quay số thành công.

### 2.3 Ghi hai nơi

Cột **và** audit log (`SIM_CALL_STARTED`). Cột là thứ console đọc và có thể bị một lệnh ghi sau
đè lên; dòng audit chỉ ghi thêm. Evidence chủ sở hữu ký xứng đáng có bản không ai sửa lặng lẽ
được, và hai bản khớp nhau mới là thứ khiến một trong hai đáng tin.

`IT-TEL-VOICE-05` khoá **cả hai**.

### 2.4 Giọng đi cùng audio, không đi song song

`RenderedAudio.Voice`. Audio là thứ khách nghe; một giọng được truyền riêng bên cạnh là một giọng
có thể bị gán nhầm bản thu bởi một lần refactor sau này. Đổi lại, `RenderedAudio` tự viết
`Equals`, nên trường mới phải được thêm vào đó bằng tay — `UT-VOICE-RECORD-03` khoá đúng chỗ đó,
vì đây là loại thiếu sót không thứ gì khác bắt được.

---

## 3. Đọc ra sao

`voice_region` ở mức job đọc từ **lần gọi gần nhất có ghi giọng**; không có mới suy lại.
`voice_region_source` nói rõ `RECORDED` hay `DERIVED`.

Tách làm hai trường thay vì nhét vào một giá trị: màn hình nào không quan tâm nguồn vẫn hiện đúng
vùng, màn hình nào quan tâm thì từ chối đưa một con số suy lại vào thứ đem ký. Khi là `DERIVED`,
màn chi tiết hiện cảnh báo nói thẳng **"Không dùng để ký nghiệm thu"**.

Từng lần gọi giữ giọng của riêng nó và **không** bị điền ngược từ lần khác. Hai lần gọi của cùng
một job thật sự có thể đã dùng hai giọng khác nhau nếu cấu hình đổi ở giữa — đó chính là tình
huống công việc này tồn tại để mô tả được.

### Một điểm trái với quy ước chung của API

Ba trường giọng ở mức attempt được ghi **kể cả khi null**, ngược với luật "bỏ null" toàn cục của
API. Với người đọc, một trường vắng mặt và một trường `null` là không phân biệt được — mà "cái này
có được ghi không" lại đúng là câu hỏi công việc này sinh ra để trả lời. Một trường biến mất thì
không nói được câu "lần gọi này không ghi giọng nào", trong khi evidence pack phải nói được câu
đó. Cùng lý do với `operational_blocked_rate` trong `AnalyticsContracts`.

---

## 4. Đối chiếu yêu cầu (plan §A6)

| Yêu cầu | Test | Kết quả |
| --- | --- | --- |
| Migration thêm `voice_id` + `voice_region` vào bảng attempt | `W0113DispatchedVoice` | ✅ + `voice_region_resolved` + 2 check constraint |
| Ghi tại thời điểm dispatch | `IT-TEL-VOICE-05` | ✅ cột **và** audit log |
| `voice_region` trên contract đọc từ cột đã lưu | `IT-ADMIN-READ-11` | ✅ |
| Chỉ fallback suy-lại cho bản ghi cũ | `IT-ADMIN-READ-11` | ✅ `DERIVED` khi không có attempt nào ghi |
| Đánh dấu rõ là suy lại | `IT-ADMIN-READ-11`, `UT-UI-VOICE-05` | ✅ `voice_region_source` + cảnh báo trên màn |

### Về `IT-ADMIN-READ-11`

Giọng ghi trong test **cố ý mâu thuẫn** với thứ vùng giao hàng sẽ suy ra (ghi `Central` cho một
đơn ở Vĩnh Long — vùng Nam). Chọn một giá trị trùng khớp thì test sẽ xanh dù lối đọc chưa hề
đổi. Mâu thuẫn là cách duy nhất chứng minh cột đã lưu thắng.

---

## 5. Kết quả kiểm chứng

| Suite | Kết quả |
| --- | --- |
| `Ivr.UnitTests` | **480 / 480** (+10) |
| `Ivr.IntegrationTests` | **252 / 252** (+3) |
| `Ivr.ContractTests` | **22 / 22** |
| `Ivr.ChaosTests` | **8 / 8** |
| **Tổng .NET** | **762 / 762** |
| admin-ui | lint + `tsc` + **221 / 221** (+2) + build |
| Traceability | **464** tagged test |
| `dotnet format --verify-no-changes` | PASS |

OpenAPI `draft.15 → draft.16`: +4 field, re-pin manifest, sinh lại portal.

### Một lỗi tự tạo, bắt được trước khi kịp thành lỗi thật

Bản đầu ghi vùng dạng `NORTH`/`CENTRAL`/`SOUTH`. Sai: lối suy-lại đã phát
`North`/`Central`/`South` từ W-0106, giá trị đó được ghim trong enum OpenAPI và là **khoá** của từ
điển `voiceRegion` trong console. Hai cách viết nghĩa là giá trị ghi mới sẽ hiện thành mã thô trên
đúng những màn hình việc này sinh ra để làm cho đáng tin — và chỉ với những cuộc gọi **sau**
migration, tức loại trôi khó nhận ra nhất. `UT-VOICE-RECORD-01` giờ khoá cả hai cách viết vào cùng
một chỗ.

---

## 6. Những gì bản này **không** làm

- **Không sửa dữ liệu cũ.** Mọi lần gọi trước migration có giọng `null` và đọc ra `DERIVED`. Không
  điền ngược — suy ngược một giọng cho cuộc gọi cũ là chính xác cái sai công việc này đang gỡ.
- **Không ghi giọng cho lần gọi không kết nối.** Không có cuộc gọi thì không có giọng để ghi.
- **Không đổi cách chọn giọng.** `RegionalVoiceMap.Resolve` nguyên vẹn.
- **Không ghi tốc độ đọc (`SpeakingRate`).** Nó cũng là cấu hình và cũng suy lại được sai như vậy;
  chưa nằm trong phạm vi A6 và nên là một hạng mục riêng nếu chủ sở hữu cần ký cả tốc độ.
- **Chưa có evidence lab nào được sinh lại.** Các bản evidence lab đã có vẫn mang số suy lại; công
  việc này chỉ bảo đảm các bản **sau** đây có số ghi lại.
