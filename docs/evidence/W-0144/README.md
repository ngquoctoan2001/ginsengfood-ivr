# W-0144 — M8-04 DT-04 window enforcement và production-adapter preflight

Trạng thái: `TESTS_PASS / DT04_LOCAL_COMPLETE / PRODUCTION_ADAPTER_BLOCKED_EXTERNAL`  
Ngày: `2026-08-29`  
Baseline code: `main@b082ed1` + shared W-0143 documentation WIP  
Người ký: **Tôi — Module 8 / Project Owner**

## Kết luận

Mô tả DT-04 khóa `fail_count ≥ 3 trong 10 phút`, nhưng runtime trước W-0144 chỉ tăng một counter
liên tiếp không có mốc thời gian. Vì vậy hai lỗi cũ cách nhiều giờ cộng một lỗi mới vẫn có thể
`HEALTH_FAILED`; đó là triển khai sai policy, không phải chi tiết wording.

W-0144 thêm `failure_window_started_at` nullable vào `ivr_sim_channels` và dùng một policy bền vững
cho cả hai nơi ghi lỗi: provider báo channel unhealthy và scheduler thu hồi lease hết hạn.

Semantics đã khóa:

1. Lỗi đầu mở cửa sổ và đặt `fail_count=1`.
2. Lỗi thứ ba tại hoặc trước mốc 10 phút chuyển `HEALTH_FAILED`.
3. Lỗi tiếp theo sau hơn 10 phút mở cửa sổ mới với `fail_count=1`.
4. Kết quả healthy đặt `fail_count=0` và xóa mốc cửa sổ.
5. Alert tổng toàn đội theo 10 phút là cơ chế quan sát riêng, không thay counter per-channel.

## Artifact

- `src/Ivr.Infrastructure/Telephony/SimChannelFailurePolicy.cs`
- `src/Ivr.Infrastructure/Telephony/PostgresTelephonyDispatchStore.cs`
- `src/Ivr.Infrastructure/Scheduling/PostgresSchedulerStore.cs`
- `src/Ivr.Infrastructure/Persistence/Entities/IvrPersistenceEntities.cs`
- `src/Ivr.Infrastructure/Persistence/Migrations/20260829042416_W0144SimChannelFailureWindow.cs`
- `tests/Ivr.IntegrationTests/MockTelephonyPersistenceTests.cs`
- `tests/Ivr.IntegrationTests/SchedulerPersistenceTests.cs`
- `tests/chaos/ChaosFixtures.cs`

## Evidence đã chạy

| Gate | Kết quả |
| --- | --- |
| Focused PostgreSQL behavior: expired-window reset, healthy reset, lease-expiry threshold, existing unhealthy path | `4/4 PASS` |
| Migration empty DB / rollback / recreate + one-version-back compatibility | `3/3 PASS` |
| `CHAOS-SIM-03` | `1/1 PASS` |
| Migration generation/build | `PASS` |
| Full current project suites | `763/763 PASS` — Contract 24, Unit 495, Integration 236, Chaos 8 |
| Format / docs / tracker mirror | `PASS`; 11 gates, 142 work items, 23 open decisions, production flag false |

Post-change GitNexus chạy trên toàn dirty checkout báo `HIGH`, 19 tracked file/58 symbol/6 flow; scope
bao gồm W-0143 documentation WIP. Các flow code bị ảnh hưởng của W-0144 quy về
`QuarantineExpiredLeasesAsync` và `SimChannelEntity`, đúng với blast radius đã cảnh báo trước sửa.

## Production adapter: stop rule

Không có vendor đã chọn hoặc vendor response được điền trong procurement pack. Cũng chưa có signed
trust boundary cho `dial_token → E.164`, credential custody/rotation, raw disposition mapping,
recording-off capability, caller-ID/DTMF/health proof hoặc lab report. Do đó không có cơ sở trung
thực để viết production resolver/gateway.

Phải nhận đủ tối thiểu:

- vendor API/protocol + sandbox credential do secret owner cấp;
- raw code → đủ 11 disposition và recording OFF proof;
- Security-signed resolver/trust boundary, Vault/KMS custody, TTL/replay/rotation/audit;
- caller-ID, DTMF, per-channel health/disable và quota/failover capability;
- R-02 lab results + R-04 scorecard/gap disposition.

Cho tới khi đủ các artifact này: `REAL_CUSTOMER_CALL_ALLOWED=NO`, không bật `PRODUCTION_REAL`, không
gọi khách thật, không ghi `ACCEPTED` hoặc `production-ready`.

## Chữ ký

**Tôi — Module 8 / Project Owner** ký semantics DT-04 và handoff phần local ngày `2026-08-29`.
Chữ ký này không thay vendor, Security, Platform, Procurement hoặc Release approval.
