# FR — Admin / Ops Operations

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p03`
Nguồn: `phase-8/08` (giám sát/audit/privacy), `phase-8/11 §5,§8` (admin API + permission), `docx` §16 (admin dashboard).

**Actor:** Ops Admin / Incident Manager (qua RBAC).
**Precondition:** Authenticated + có permission server-side.
**Trigger:** Vận hành queue/SIM/incident/review.
**Postcondition:** Hành động thực thi có `reason` + audit + evidence; không bypass P0 blocker.

## Màn/endpoint & permission (docx §16; phase-8/11 §5)
| Chức năng | Permission | Không được làm |
| --- | --- | --- |
| Dashboard (call volume, success rate, queue depth, SIM health, incidents) | `IVR_QUEUE_VIEW` | Sửa order state trực tiếp |
| Call jobs list/detail (task, attempts, callback, evidence) | `IVR_QUEUE_VIEW` | Tạo gọi lại ngoài retry rule; lộ full phone |
| Queue pause / resume | `IVR_QUEUE_PAUSE` / `IVR_QUEUE_RESUME` | Resume khi capacity incident/blocker chưa xử lý |
| SIM enable / disable | `IVR_SIM_ENABLE` / `IVR_SIM_DISABLE` | Enable SIM đang fail health; ép assign SIM bận |
| Technical retry | `IVR_MANUAL_RETRY` | Reset customer attempt; bypass blocker; vượt max |
| Admin review / annotation | `IVR_RESULT_REVIEW` | Force confirm/cancel order; fake result |
| Capacity incidents | `IVR_QUEUE_VIEW` | Xóa incident lịch sử |
| Audit/evidence view | `IVR_QUEUE_VIEW` | Sửa evidence đã ghi |

## FR
| ID | Yêu cầu | Nguồn | Acceptance hint |
| --- | --- | --- | --- |
| FR-IVR-ADM-001 | Mọi admin action: authenticated actor + permission **server-side** (không tin client) + `reason` + `target_type/id` + audit + evidence (nếu ảnh hưởng queue/SIM/retry/result) | phase-8/11 §8; docx §16,§17 | Thiếu permission → `403`; thiếu reason → reject |
| FR-IVR-ADM-002 | Admin **KHÔNG** được: gọi ngoài attempt policy, reset customer attempt count, force confirm/cancel order, enable SIM khi health fail, resume khi capacity incident chưa xử lý | phase-8/11 §8; docx §16 | Vi phạm → chặn (P0) |
| FR-IVR-ADM-003 | Admin action `no_policy_bypass = true`; không override P0 blocker (Sale Lock/Recall/Suppression) | phase-8/02 §12; docx §22 | Override blocker → FAIL |
| FR-IVR-ADM-004 | Override result (nếu có) yêu cầu restricted admin + Core approval + **dual evidence** | phase-8/15 | Override thiếu dual evidence → chặn |
| FR-IVR-ADM-005 | UI hiển thị `phone_masked`; ẩn full phone/address/payment/member/health/CRM mặc định | phase-8/08 §6; docx §16,§17 | Full PII hiển thị → FAIL (P0-IVR-007) |
| FR-IVR-ADM-006 | Admin action idempotent (Idempotency-Key cho POST rủi ro) | phase-8/11 §2 | Duplicate action → no double effect |

## Owner Decision
- `Owner Decision Required` Q-U1 (nền tảng admin UI + nơi tạo permission `IVR_*`), Q-A2 (service identity allowlist).
Chi tiết UI: sẽ sinh ở p12 (`specs/srs/ui/*`).
