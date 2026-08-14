# UI-06 — Admin Review & Technical Retry

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p12` · Permission: `IVR_RESULT_REVIEW` / `IVR_MANUAL_RETRY`. Nguồn: `functional/06,07`, `api/03`.

## Mục đích
Hàng đợi các case cần con người: callback needs-review, evidence-failed, capacity/technical exception, stale.

## Bố cục
```
[ Review queue: case_type · order_code_short · result_type · reason · correlation_id ]
[ Detail: trace (link UI-03) · vì sao vào review (evidence_failed/needs_admin_review/technical) ]
[ Action bar: Ghi review | Technical retry | (link) Xem chi tiết ]
```

## Actions
| Action | Permission | API | Ràng buộc |
| --- | --- | --- | --- |
| Ghi review/annotation | `IVR_RESULT_REVIEW` | `POST /admin-reviews` | reason + audit; không đổi result giả |
| Technical retry | `IVR_MANUAL_RETRY` | `POST /technical-retries` | `customer_attempt_counted=false`; bounded; không bypass blocker |

P2-8 response cho review có `result_unchanged=true`; response retry có `technical_retry_count`, `customer_attempt_counted=false`, `queue_status` và `no_policy_bypass=true`. UI phải hiển thị đây là annotation/requeue kỹ thuật, không phải override result hay order transition.

## P0
- **Không** cho: sửa result giả, force confirm/cancel order (D-02), reset customer attempt count, vượt max attempt (D-10). Mọi action audit + `no_policy_bypass=true`.
- Khi queue/capacity hold, final result, hết confirmation window hoặc retry limit đã đạt: ẩn/disable action và vẫn chấp nhận backend trả `409` fail-closed nếu state đổi sau khi render.
