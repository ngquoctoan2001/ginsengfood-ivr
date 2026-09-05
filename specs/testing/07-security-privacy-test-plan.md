# TEST-07 — Security & Privacy Test Plan

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p11` · Nguồn: `data/05-pii-policy`, `functional/08`, `api/01`; DF-01/06, D-05, DT-05.

## 1. Auth / allowlist (DF-06)
| ID | Given | Then |
| --- | --- | --- |
| SEC-01 (neg) | non-Order-Core gọi `POST /tasks` | `403 IVR_FORBIDDEN_CALLER` |
| SEC-02 (neg) | request thiếu token | `401 IVR_UNAUTHENTICATED` |
| SEC-03 (neg) | SIM adapter thử ghi order/tạo task | forbidden — không có credential |

## 2. RBAC admin (DF-01, agents.sample)
| ID | Given | Then |
| --- | --- | --- |
| SEC-04 | AGT-VIEWER-01 gọi `queue:pause` | `403` (thiếu `IVR_QUEUE_PAUSE`) |
| SEC-05 (neg) | admin thử force confirm/cancel order | forbidden (D-02) |
| SEC-06 (neg) | enable SIM khi health fail / resume khi incident chưa xử lý | forbidden |
| SEC-07 | mọi admin action | có `reason`+`actor`+audit; `no_policy_bypass=true` |

## 3. PII / privacy (D-05, DT-05)
| ID | Given | Then |
| --- | --- | --- |
| SEC-08 (neg) | scan log/UI/DB | **không** raw phone/full profile/payment/health (chỉ `phone_masked`) |
| SEC-09 | `dial_token` | TTL ≥ window end; dùng lại được nhưng gắn `task_id` và có trần số lần resolve (`OD-V1-17`); mapping token→số **không** ở IVR (SIM vault) |
| SEC-10 | recording | OFF mặc định; `recording_ref=null`; bật cần consent+legal |
| SEC-11 | DTMF lưu trữ | chỉ semantic (`1/0/invalid`), không audio |
| SEC-12 | call script | chỉ biến whitelist; field cấm → reject |
| SEC-13 | audit | append-only; soft-delete không che audit |

## 4. Data-leak / boundary
| ID | Given | Then |
| --- | --- | --- |
| SEC-14 (neg) | task chứa full address/payment | reject (không consume) |
| SEC-15 | evidence/trace | dùng ref/id (sale_lock_id/recall_case_id/correlation_id), không PII thô |

## Báo cáo
15 security/privacy case; P0: allowlist, SIM-no-order-write, no-raw-phone, recording-OFF, admin-no-force-order. Security/privacy review là điều kiện release gate (DF-03).
