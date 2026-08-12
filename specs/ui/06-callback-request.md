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

## P0
- **Không** cho: sửa result giả, force confirm/cancel order (D-02), reset customer attempt count, vượt max attempt (D-10). Mọi action audit + `no_policy_bypass=true`.
