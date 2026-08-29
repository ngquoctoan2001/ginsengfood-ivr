# Phiếu ý kiến — gửi Legal / Privacy

**Chủ đề:** `OD-VOICE-07` — chấp nhận hay từ chối rủi ro pháp lý/quyền riêng tư của đúng bộ
artifact VieNeu-TTS tự host (W-0122)
**Người gửi:** Team IVR / Module 8 (IVR Order Confirmation)
**Ngày lập:** `2026-08-28` · **Routing cập nhật:** `2026-08-29`

**Trạng thái:** `READY_TO_DISPATCH / NOT_SENT / EXTERNAL_RESPONSE_REQUIRED`
**Ưu tiên:** P1 — chặn production. **Không** chặn lab: lab chạy dữ liệu giả và
`REAL_CUSTOMER_CALL_ALLOWED=NO`

> Xin đọc mục 3 trước. Đó là phần dự án **biết là mình đang thiếu**, không phải phần dự án muốn
> Legal bỏ qua. Model card khai `Apache-2.0` **không** được dự án coi là kết luận pháp lý — đó
> chính là lý do có phiếu này.

---

## 1. Việc đang xin ý kiến là gì

IVR gọi khách để xác nhận đơn hàng. Mỗi cuộc gọi phát 7 đoạn; 3 đoạn phải tổng hợp giọng nói **tại
thời điểm gọi** vì nội dung là dữ liệu của chính đơn đó.

Hôm nay 3 đoạn đó được gửi ra **ElevenLabs (SaaS)**. W-0122 thay bằng một model chạy **trong hạ
tầng của dự án**, gọi qua loopback `127.0.0.1`, không mở cổng ra ngoài.

Nghĩa là câu hỏi cho Legal có **hai nửa tách rời**:

- **Nửa license:** dự án có được dùng model/giọng này cho mục đích thương mại không?
- **Nửa privacy:** việc bỏ luồng dữ liệu khách ra SaaS có làm thay đổi nghĩa vụ hiện tại không?

## 2. Bộ artifact chính xác cần ý kiến

Ý kiến xin gắn với **đúng** các pin dưới đây. Bất kỳ pin nào đổi thì ý kiến cũ hết hiệu lực và dự
án sẽ quay lại xin lại.

| Thành phần | Pin | License khai báo | Có file LICENSE trong revision? |
| --- | --- | --- | --- |
| Source `pnnbao97/VieNeu-TTS` | commit `36c4b501b0634a8f59805e6b529a058fbd30190b` | Apache-2.0 | ✅ Có — SHA-256 `c71d239df91726fc519c6eb72d318ec65820627232b2f796219e87dcf35d0ab4` (bản Apache-2.0 chuẩn) |
| Model `pnnbao-ump/VieNeu-TTS-v3-Turbo` | revision `2da0efab622a1722125991736524f080b751ef5b` | `apache-2.0` trong **metadata model card** | ❌ **Không** |
| Codec `OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX` | revision `ceff0d0749bfb3fa2d61149794ec6feef0d1e1ae` | `apache-2.0` trong **metadata model card** | ❌ **Không** |

- Codec MOSS là **dependency bắt buộc lúc chạy** của model VieNeu, không phải tuỳ chọn.
- 13 file artifact được khóa theo path + size + SHA-256 trong `deploy/tts/models/MODELS.lock`.
- Biến thể `pnnbao-ump/VieNeu-TTS-0.3B-q4-gguf` mang license **NC (non-commercial)** và đã bị gate
  chặn cứng bằng allowlist; dự án **không** dùng bản đó.
- Bộ giọng Owner đã ký ngày `2026-08-28`: Bắc **Ngọc Linh**, Trung **Ngọc Trân**, Nam
  **Mỹ Duyên**, theo `voice-acceptance-manifest.json`. Ý kiến Legal phải áp dụng cho đúng ba preset
  này và exact pin ở trên. Đây là **preset của model**, không phải clone giọng người thật. Voice
  cloning nằm ngoài phạm vi W-0122 và sẽ là work item riêng nếu cần.

## 3. Ba khoảng trống dự án đã tự xác định

1. **Hai model revision đã khóa không chứa file `LICENSE`.** Chỉ có metadata model card khai
   `apache-2.0`. Dự án không coi metadata là văn bản cấp phép.
2. **Training data không được công bố / bị gated.** Không xác minh được model được huấn luyện trên
   dữ liệu gì, kể cả giọng người thật có consent hay không.
3. **Quyền thương mại của đúng 3 preset sẽ chọn chưa có ai xác nhận.** Apache-2.0 cho *weights*
   không đương nhiên trả lời câu hỏi về *giọng*.

## 4. Dữ liệu thật sự đi qua TTS lúc chạy

| Đoạn | Loại | Nội dung | Ghi chú |
| --- | --- | --- | --- |
| 1, 3, 5, 7 | Cố định | Lời chào, câu nối, hướng dẫn phím | Render sẵn thành 12 file, **không** đi qua TTS lúc gọi |
| 2 | Động | `items_spoken` — tên hàng công khai + số lượng | |
| 4 | Động | `total_amount_display` — tổng tiền | |
| 6 | Động | `delivery_area_short` — khu vực giao | Cấp phường/quận |

Những điều dự án **đã** enforce bằng code, không phải bằng quy ước:

- Template đang dùng (`SCRIPT-ORDER-CONFIRM`, `v3-test-approved`) có **đúng 3 placeholder động ở
  trên**. `customer_display_name` tồn tại trong hệ placeholder nhưng **không** nằm trong template
  này — tức tên khách hiện **không** được đọc lên.
- Địa chỉ cấp đường/số nhà bị intake **từ chối** với `PII_POLICY_VIOLATION` `422`
  (`IT-INTAKE-PRIVACY-04`); không có job nào được tạo.
- **Số điện thoại không bao giờ nằm trong nội dung đọc.**
- Shim TTS **không cache, không ghi đĩa, không log** text/audio/traceback; body lỗi rỗng; metric
  chỉ có status code, latency bucket, queue depth.
- File audio động hết hạn theo giá trị **sớm nhất** trong: confirmation window,
  `CacheMaximumTtlSeconds` (`900` giây), `SpeechSnapshotRetentionSeconds` (`900` giây); có hook xoá
  file quá hạn.
- Trong kiến trúc mới, **không có request nào rời khỏi Pod** ở đường tổng hợp giọng.

## 5. Câu hỏi cần trả lời

Xin trả lời kèm cơ sở (điều khoản/quy định/án lệ nội bộ). Đánh dấu thẳng vào ô.

### `L1` — Metadata model card khai `apache-2.0` mà revision không có file LICENSE: có đủ để dùng thương mại không? (P1)

☐ Đủ — nêu cơ sở: `_______________________`
☐ Không đủ — cần upstream bổ sung file LICENSE vào đúng revision
☐ Không đủ — cần thư cho phép riêng bằng văn bản từ tác giả model
☐ Không đủ — cần thay bằng model khác

### `L2` — Training data không công bố: chấp nhận được không? (P1)

☐ Chấp nhận — rủi ro tồn dư ghi nhận, hết hạn review: `____________`
☐ Không chấp nhận cho production
☐ Cần hỏi upstream trước khi quyết

### `L3` — Quyền thương mại của 3 preset voice sẽ chọn (P1)

☐ Được dùng thương mại theo license của model
☐ Cần xác nhận riêng cho từng preset
☐ Cần tránh preset nào mô phỏng giọng người có thật — nếu vậy nêu tiêu chí: `______________`

> Owner đã nghe đủ 11 candidate và chọn Ngọc Linh / Ngọc Trân / Mỹ Duyên. Legal không được trả lời
> chung cho “một model bất kỳ”; quyết định phải nêu rõ có chấp nhận đúng ba preset này hay không.

### `L4` — Nghĩa vụ attribution / NOTICE khi phân phối image nội bộ (P2)

☐ Đủ với `THIRD_PARTY_NOTICES.md` + `LICENSE` đã kèm trong image
☐ Cần thêm: `_______________________`

### `L5` — Bỏ luồng dữ liệu khách ra SaaS có làm đổi nghĩa vụ hiện tại không? (P1)

☐ Giảm nghĩa vụ — không cần DPA cho TTS nữa
☐ Không đổi — vẫn cần đánh giá nội bộ, nêu loại: `____________`
☐ Cần DPIA cho việc xử lý bằng model tự host · deadline: `____________`

### `L6` — Thiết kế retention (mục 4) có được chấp nhận không? (P1)

☐ Chấp nhận `900` giây cho cache/snapshot
☐ Cần ngắn hơn: `______` giây
☐ Cần thêm ràng buộc: `_______________________`

### `L7` — Trong lúc chờ, ElevenLabs free tier ở lab có tiếp tục được không? (P2)

Bối cảnh: `A-0357` đã tách `OD-VOICE-01` — phần lab `APPROVED` (dữ liệu giả, không khách nào nghe),
phần production còn `OPEN`.

☐ Tiếp tục được ☐ Phải dừng — lý do: `_______________________`

---

## 6. Việc phía IVR sẽ làm sau khi có trả lời

| Trả lời | Hành động |
| --- | --- |
| `L1`–`L3` đều chấp nhận | Ghi reference vào `MODELS.lock.legal_gate` với `decision_authority=LEGAL_PRIVACY`, `approval_reference` và người/ngày ký; đóng `OD-VOICE-07`; các gate khác vẫn mở |
| Bất kỳ mục nào từ chối | Dừng nhánh self-host, giữ provider hiện tại, mở work item theo hướng Legal chỉ định |
| Có điều kiện | Thực hiện điều kiện rồi xin ý kiến lại trên **cùng bộ pin**; pin đổi thì xin lại từ đầu |
| Không trả lời | `MODELS.lock.legal_gate` giữ `OWNER_DATA_REQUIRED`; **không** suy ra chấp nhận từ CI xanh hay từ model card |
