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
