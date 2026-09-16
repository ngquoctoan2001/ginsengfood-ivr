# Đã xong — quyết định đã đóng của Module 8

Cập nhật: **16/09/2026** · Thay thế 2 file ký riêng lẻ và ghi nhận các phần đã đóng của những gói khác.

Chỉ liệt kê những gì **không còn hành động nào phía Module 8**. Việc còn chờ ai đó nằm ở [00-CHUA-XONG.md](00-CHUA-XONG.md).

Toàn văn các file đã xóa nằm trong lịch sử git — lấy bằng `git log --all --full-history -- plan/ivr-orther/<tên file>`.

## OD-V1

### od-v1-signoff

**Gói ký quyết định OD-V1 · 05/09/2026 · `SIGNED` · `W-0194` · baseline `main@2a6d290`**

Người ký: IVR owner (`marketingssv2024@gmail.com`). Nguồn quyết định: [open-decisions-register.md](../../specs/_review/open-decisions-register.md).

Đóng một lượt các mục OD-V1 đã trình. Register là nguồn hiện hành — đọc register, không đọc lại gói ký.

### od-v1-17-ttl

**Gói ký vế TTL của `OD-V1-17` · 09/09/2026 · `SIGNED` · `W-0246` · baseline `main@55d552d`**

Người ký: IVR owner. **TTL của dial token bằng đúng cuối cửa sổ xác nhận**, thay mốc `+60s` cũ.

Test đóng: `IT-INTAKE-DB-03` trong `tests/Ivr.IntegrationTests/TaskIntakePersistenceTests.cs`. Không mở lại quyết định này khi làm đường gọi production.

## Các phần đã đóng của gói còn dở

Những gói dưới đây **chưa đóng toàn bộ** — phần còn lại ở [00-CHUA-XONG.md](00-CHUA-XONG.md). Ghi ở đây để khỏi làm lại.

### sip-05-phan-mem

**Điều phối đồng thời và sở hữu controller · 15/09/2026 · `W-0247`..`W-0248`**

Đã chứng minh trên code và test, không phải làm lại:

- `G2_SOFTWARE_CONCURRENCY_PROVEN` — dispatch song song có trần, `SchedulerDispatchPump.cs`; commit `56e3213`
- `POOL_BOUND_PROVEN_ON_POSTGRES` — trần dùng chung giữ được khi nhiều worker tranh việc; commit `c061001`
- Giảm tải sau chuỗi lỗi dispatch liên tiếp; commit `6eaa64b`
- `CONTROLLER_OWNERSHIP_LANDED` — khóa sở hữu ARI, lease hết hạn không tự chuyển; `AriControllerOwnership.cs`, migration `20260915085434`; commit `b1bf377`
- `TOKEN_LEDGER_DURABLE` — trần resolve bền qua restart, chỉ lưu `token_hash`; `PostgresDialTokenResolveLedger.cs`, migration `20260915114705`; commit `eb7e781`
- `PRODUCTION_GATE_ENV_SCOPED` — phê duyệt `PRODUCTION_CALL` gắn môi trường, `NULL` không mở gì; migration `20260915122953`; commit `b817fae`
- `CONTROLLER_STATUS_OBSERVABLE` — trạng thái controller trên `/healthz`; `SchedulerControllerStatus.cs`
- Runbook lấy lại quyền điều khiển Asterisk — [docs/operations/ari-controller-ownership.md](../../docs/operations/ari-controller-ownership.md); commit `ff22227`

Test: `SchedulerDispatchPumpTests.cs`, `SchedulerSharedPoolTests.cs`, `AriControllerOwnershipTests.cs`, `DialTokenResolveLedgerPersistenceTests.cs`, `SchedulerControllerStatusTests.cs`.

**Còn chặn:** trần đang đặt `MaxConcurrentDispatches = 1`; pool theo trunk chờ SIP-03; bảo vệ token chờ Platform cấp khóa. Xem [kế hoạch đường gọi production](sip-production-dial-path-plan-2026-09-16.md).

### m8-17-fence-thu-hoi

**Fence thu hồi đơn · 09/09/2026 · `W-0249`**

Hai fence đã cài đúng hai chỗ: lúc scheduler claim, và lần đọc task cuối cùng ngay trước khi quay số.

- `PostgresSchedulerStore.cs` — fence tại claim
- `PostgresTelephonyDispatchStore.cs` — fence trước dial
- Migration `20260909034715_W0249OrderRevocationFence.cs` — 3 cột revoke
- Test tái hiện revoke trước claim và revoke sau claim

**Còn chặn:** `ENDPOINT_PENDING` — M3 chưa cấp endpoint thu hồi. Nằm ở [00-CHUA-XONG.md](00-CHUA-XONG.md#m8-17).

### today-03-giong-doc

**Bộ giọng đọc · 28/08/2026**

- Nghe 11 candidate, chọn 3 miền — `OWNER_ACCEPTED 2026-08-28`: **Bắc Ngọc Linh, Trung Ngọc Trân, Nam Mỹ Duyên**
- Voice manifest gate `PASS` — manifest ký hợp lệ, khớp binding audio/model hiện tại

**Còn chặn:** phần TTS self-hosted `BLOCKED_EXTERNAL`. Nằm ở [00-CHUA-XONG.md](00-CHUA-XONG.md#today-03).

### m8-15-so-cai-dung-luong

**Ledger dung lượng có chuỗi hash · 03/09/2026 · `W-0158`, `W-0159`**

Đã dựng và chứng minh: ledger metadata append-only có internal hash chain; checkpoint chứa full-ledger hash/count/head; verifier bắt buộc một trusted checkpoint SHA-256 nằm ngoài ledger. Phát hiện được sửa byte, truncate giữa dòng, và rollback về valid prefix — **với điều kiện caller đưa đúng checkpoint mới nhất**.

**Còn chặn:** chưa có chữ ký Platform/Security/M8, chưa chọn provider, chưa được phép viết code. Nằm ở [00-CHUA-XONG.md](00-CHUA-XONG.md#m8-15).

### m8-12-dinh-tuyen-quyet-dinh

**Định tuyến quyết định ngoài · 03/09/2026**

`S-01..S-10` đã đối chiếu xong với artifact hiện hành. **`S-10` đã thực thi tại `W-0141`** — không còn là việc chờ làm.

**Còn chặn:** `S-11` (errata VoLTE/procurement) và toàn bộ việc gửi ra ngoài. Nằm ở [00-CHUA-XONG.md](00-CHUA-XONG.md#m8-12).
