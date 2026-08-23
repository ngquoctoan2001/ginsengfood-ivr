# UI-03 — Call Detail (trace task→job→attempt→result→callback)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p12` · Permission xem: `IVR_QUEUE_VIEW`; action review/retry cần permission riêng. Nguồn: `database/*`, `api/*`, `data/05`.

## Mục đích
Xem toàn bộ vòng đời một task để review/khiếu nại: task intake → eligibility → call jobs → attempts → raw event → result → callback → evidence.

## Bố cục
```
[ Header: order_code_short · phone_masked · program · order_state(đục) · order_version(target/optional) ]
[ Timeline:
   Task intake: decision · blocked_reasons · sellable_status[] (per-line: sku/decision/recall_hold/sale_lock) · captured_at
   Eligibility: PASS/skip/block + reasons
   Attempts[1..2]: scheduled_at · status · disposition · dtmf_key · is_counted · sim_channel · technical_exception_type
   Result: result_type · is_final · recommended_core_action(advisory)
   Callback: result_state · core_http_status(current) · core_response_code(target) · retry_count
 ]
[ Evidence/Audit refs: evidence_ref · audit_ref · sale_lock_id/recall_case_id (nếu block) · correlation_id ]
```

## Dữ liệu hiển thị / ẩn
- Hiển thị: refs/ids, trạng thái, disposition semantic, DTMF semantic (`1/0/invalid`), evidence refs.
- **Ẩn**: raw phone/token, full address, payment, health, audio recording (OFF).

## Actions (theo permission)
| Action | Permission | API | Ràng buộc |
| --- | --- | --- | --- |
| Ghi review/annotation | `IVR_RESULT_REVIEW` | `POST /admin-reviews` | reason + audit |
| Yêu cầu technical retry | `IVR_MANUAL_RETRY` | `POST /technical-retries` | không tăng customer attempt; không bypass blocker |

## P0
- **Không** nút force confirm/cancel order (D-02). Không hiển thị/chỉnh order state. `recommended_core_action` chỉ là advisory (Core quyết).

## Giọng đã phát — ghi lại, không suy lại (W-0113)

Trước bản này `voice_region` được **suy lại lúc đọc màn hình** từ vùng giao hàng đã lưu. Nghĩa là
nó là hàm của cấu hình *hôm nay*, không phải bản ghi của cuộc gọi. Một lần đổi bản đồ giọng giữa
lúc gọi và lúc đọc làm mọi evidence cũ mô tả một giọng không ai từng nghe — **không có gì đỏ, chỉ
có số sai**, và đó là con số chủ sở hữu ký.

Giờ mỗi lần gọi ghi lại giọng của nó tại thời điểm dispatch:

| Cột trên `ivr_call_attempts` | Nghĩa |
| --- | --- |
| `voice_id` | Giọng thật đưa cho TTS |
| `voice_region` | `North` / `Central` / `South` |
| `voice_region_resolved` | `true` = nhận ra tỉnh trong địa chỉ; `false` = dùng giọng mặc định |

Cột thứ ba là cột dễ bị bỏ qua nhất và cũng cần nhất: *"Nam vì nhận ra Cần Thơ"* và *"Nam vì Nam
là mặc định"* là hai điều khác nhau, và chỉ điều đầu là bằng chứng về khách hàng này.

Ràng buộc CSDL ép **cả ba cùng có hoặc cùng không**, và `voice_region` chỉ nhận đúng ba giá trị.

### `voice_region` ở mức job đọc từ đâu

Đọc từ **lần gọi gần nhất có ghi giọng**; không có thì mới suy lại. Trường mới
`voice_region_source` nói rõ là `RECORDED` hay `DERIVED`.

Không gộp hai thứ vào một giá trị: màn hình nào không quan tâm nguồn thì vẫn hiện đúng vùng, màn
hình nào quan tâm thì từ chối đưa một con số suy lại vào thứ đem ký. Khi là `DERIVED`, màn hình
hiện cảnh báo nói thẳng rằng **không dùng để ký nghiệm thu**.

Từng lần gọi giữ giọng của riêng nó và **không** bị điền ngược từ lần khác — hai lần gọi của cùng
một job thật sự có thể đã dùng hai giọng khác nhau nếu cấu hình đổi ở giữa.

### Không ghi ở đâu

Ghi tại `MarkActiveAsync` (ngay sau khi quay số thành công), không phải lúc render. Render xong mà
quay số hỏng thì không có cuộc gọi nào; ghi giọng cho một lần gọi chưa từng kết nối là một khẳng
định về việc chưa xảy ra.

Đồng thời ghi vào **audit log** (`SIM_CALL_STARTED`) vì log đó chỉ ghi thêm. Cột có thể bị ghi đè
bởi một lệnh ghi sau; dòng audit thì không, và evidence chủ sở hữu ký xứng đáng có bản không ai
sửa lặng lẽ được.
