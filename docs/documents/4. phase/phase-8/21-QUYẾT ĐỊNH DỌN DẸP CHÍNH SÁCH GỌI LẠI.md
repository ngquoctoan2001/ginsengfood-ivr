# IVR-21 - Attempt Policy Cleanup Decision

> [!IMPORTANT]
> Superseded by D-10 (2026-07-02): cleanup decision cu trong file nay chi con gia tri lich su. Rule hien hanh: `MAX_ATTEMPT = 2` cho ca `GOLDEN_HOUR` va `TWENTY_FOUR_SEVEN`; GH window 5 phut A2@`T0+2:30`; 24/7 window 15 phut A2@`T0+7:30`. Xem `plan/ivr-orther/decisions-log.md` D-10.

Ngày lập: 2026-06-05
Trạng thái: `CLEANUP_APPLIED__EVIDENCE_PENDING`
Scope: Pack 09 / Pack 07 embedded IVR fragments + Phase 8 canonical attempt policy

## 1. Kết luận canonical hiện hành

Phase 8 SRS và Pack 09 source hiện tại thống nhất attempt policy:

| Program | Window | Attempts | Expiry |
| --- | --- | --- | --- |
| `GOLDEN_HOUR` | 10 phút | Attempt 1 tại `T0`; attempt 2 tại `T0 + 5 phút` | `T0 + 10 phút` |
| `TWENTY_FOUR_SEVEN` | 15 phút | Attempt 1 tại `T0`; attempt 2 tại `T0 + 5 phút`; attempt 3 tại `T0 + 10 phút` | `T0 + 15 phút` |

Technical retry không được tính là customer attempt. Capacity incident không được kéo dài window thương mại.

## 2. Cleanup đã áp dụng trong Pack 09 / Pack 07

Các fragment legacy đã được thay bằng rule hiện hành:

| Rule | Current value |
| --- | --- |
| `GOLDEN_HOUR` attempt 2 | `T0 + 5 phút` |
| `GOLDEN_HOUR` expiry | `T0 + 10 phút` |
| `GOLDEN_HOUR` attempt interval | `5 phút` |
| Smoke expected | attempt 1 `T0`, attempt 2 `T0 + 5:00`, expiry `T0 + 10:00` |
| `TWENTY_FOUR_SEVEN` | `T0`, `T0 + 5`, `T0 + 10`, expiry `T0 + 15` |

Không còn fixture legacy active được phép dùng làm spec implementation cho scheduler.

## 3. Runtime rule

Trước khi gọi IVR implementation-ready:

1. Scheduler/test phải đọc `IVR_ATTEMPT_POLICY_V2026_06_CANONICAL_001`.
2. Smoke/test expected phải theo `T0/T0+5/T0+10` và expiry `T0+10/T0+15`.
3. TECH-09, Pack 09 và Phase 8 không được tạo rule đối nghịch.
4. Regenerate consolidated reading file nếu có file gôm Phase 8.
5. Nếu Golden Hour schedule hoặc attempt policy thay đổi, phải tạo policy version mới và update MASTER-08.

## 4. Release block

Không được:

- Bật real customer call.
- Gọi IVR smoke pass toàn hệ.
- Dùng fixture legacy để sinh worker/scheduler.

Cho đến khi IVR smoke/evidence accepted.

## 5. P0 smoke sau cleanup

| Test ID | Scenario | Expected |
| --- | --- | --- |
| IVR21-P0-001 | Golden Hour schedule | Exactly 2 attempts: `T0`, `T0+5`; expiry `T0+10`. |
| IVR21-P0-002 | 24/7 schedule | Exactly 3 attempts: `T0`, `T0+5`, `T0+10`; expiry `T0+15`. |
| IVR21-P0-003 | Legacy timing fixture | Không còn fixture legacy active; nếu xuất hiện phải fail. |
| IVR21-P0-004 | Technical SIM failure | No customer attempt count increment. |
| IVR21-P0-005 | Capacity miss | Expired/block signal, no window extension. |
