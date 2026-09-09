# W-0249 — Hai fence thu hồi đơn, đã dựng

Ngày: 2026-09-09 · Baseline: `main@82ec1b3` · Trạng thái: **TESTS_PASS**.

Owner chốt hình dạng `task_id` + `order_version` + `reason` và bảo dựng. Đây là phần code.

## 1. ⚠️ Cảnh báo trước: `CRITICAL`, 24 execution flow

`gitnexus impact TryClaimDueDispatchAsync` → **HIGH** (25 impacted, 20 direct).
`detect_changes` → **`critical`, 24 affected processes**.

**Lần này rủi ro là thật**, khác `W-0243` nơi hành vi chứng minh được là không đổi. Ở đây hành vi
**có** đổi trên đường dispatch: `LoadAsync` nay ném được, và `TryClaimDueDispatchAsync` nay trả
`null` ở chỗ trước đây trả lease.

Ba điều làm nó chịu được:

| | |
| --- | --- |
| **Additive refusal** | task có `revoked_at IS NULL` — **mọi hàng đang tồn tại** — chạy y hệt trước |
| **Trơ trong production hôm nay** | chưa có endpoint, nên **không gì đặt được** `revoked_at`; thay đổi chỉ sống trong test |
| **Suite phủ** | 951/951, gồm toàn bộ bộ dispatch và scheduler |

## 2. Migration — thuần additive

`20260909034715_W0249OrderRevocationFence`: ba cột nullable trên `ivr_confirmation_tasks`, không
backfill, không ràng buộc lên hàng cũ. Task viết trước migration đọc ra *"không bị thu hồi"* — đúng
thứ nó vốn là.

Sinh bằng `tools/dev/Add-IvrMigration.ps1` (đường được phép), rồi chỉnh cho hợp analyzer của repo:
file-scoped namespace và `ArgumentNullException.ThrowIfNull` — hai thứ EF không sinh ra và
`IDE0161`/`CA1062` chặn.

## 3. Fence 1 — claim

`PostgresSchedulerStore.TryClaimDueDispatchAsync`: **một vị từ**, `AND task.revoked_at IS NULL`.
Bảng task đã join sẵn cho các cột window, nên không thêm join, không thêm truy vấn.

`IT-SCH-REVOKE-01` ghim: task bị thu hồi → `TryClaimDueDispatchAsync` trả `null`, **không attempt
nào được tạo**, và **channel không bị đụng** — một task đã hủy không nên chiếm một slot SIM mà đơn
khác dùng được.

## 4. Fence 2 — lần đọc cuối trước dial

`PostgresTelephonyDispatchStore.LoadAsync`, ngay sau guard TTL.

`IT-TEL-REVOKE-02` ghim đúng ca fence 1 **không bắt được**: claim đã xong, channel đã reserved,
lease hợp lệ — `EnsureCurrentLease` sẽ xác nhận tất cả, vì nó trả lời một câu hỏi **kỹ thuật**.
Test khẳng định `LoadAsync` từ chối, **không** `RawCallEvent` nào được ghi, và lease vẫn nguyên vẹn
— tức fence kỹ thuật thật sự sẽ cho qua.

## 5. `order_version`: ghi và trả lại, **không** so

Đây là chỗ đề xuất `m8-17 §5` của tôi **sai**, và phải sửa trước khi dựng.

`m8-17` viết: *"revoke mang version ≤ version đã lưu thì từ chối"*. Không thi hành được, và sai
nguyên tắc:

```text
OAS       order_version: { type: string, description: Required Target V1 stale-result guard snapshot }
IR-06     "Snapshot chống race; IVR trả nguyên giá trị trong callback"
domain    OrderVersion : DomainStringValue — không IComparable, không CompareTo
grep      không chỗ nào trong repo so hai order_version với nhau
```

`order_version` là **chuỗi mờ IVR trả nguyên**. IVR **không có thứ tự** trên nó, nên **không thể**
biết revoke nào mới hơn — thứ tự thuộc **Order Core**, và đó chính là lý do trường này tồn tại.

⇒ Cột `revoke_order_version` **ghi lại và trả lại**, để bên **có** thứ tự phán. Trường vẫn cần
trong contract, nhưng vai trò của nó là **audit + echo**, không phải *"IVR từ chối revoke cũ"*.

## 6. Một test khác gãy, và nó lộ ra một điểm giòn có sẵn

`MigrationPreflightNamesLegacyRowsThatViolateTheSignedTaxonomy` đỏ:

```text
42703: column "revoke_order_version" of relation "ivr_confirmation_tasks" does not exist
```

Test ghim một **mốc schema** (migration trước `W0172`) rồi seed bằng **model entity hiện tại**. Nên
**mọi** cột thêm vào bảng được seed sau mốc đó đều làm nó gãy — ba cột của `W-0249` chỉ là bộ đầu
tiên làm vậy.

Đã sửa bằng cách thêm ba cột thủ công ở mốc đó (scaffolding, `W0172` không đụng chúng nên phần
preflight đang được kiểm không đổi). Và **ghi thẳng trong test** rằng điểm giòn còn đó: cột tiếp
theo sẽ cần đúng dòng ấy, còn cách sửa bền là seed ca này bằng SQL ghim schema thay vì model sống.

## 7. Kiểm chứng

```text
gitnexus impact TryClaimDueDispatchAsync   HIGH · 25 impacted · 20 direct
detect_changes                             critical · 24 affected processes
dotnet test Ivr.sln                        951/951 PASS, 0 failed, 0 skipped   (949 → 951)
traceability                               TEST_TRACEABILITY_CURRENT=581
gate-status.mjs                            GATE_STATUS_PASS
contract-freeze-verifier.mjs               CONTRACT_FREEZE=PASS
docs-selftest.mjs                          API_DOCS_SELFTEST_PASS
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`. OAS **không đổi** — endpoint đi cùng lượt phát hành với `7.1`/`2.2`.

## 8. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| — | endpoint revoke + OAS + IR-06 | **lượt phát hành contract**, cùng `7.1` và `2.2` |
| — | M3 gọi endpoint khi hủy đơn hoặc bật `sale_lock` | dev M3 |
| — | `m8-17 §5` cần sửa theo §5 ở trên | tôi, lượt sau |

Đến khi có endpoint, hai fence là **cơ chế nằm chờ**: đúng, có test, và trơ — vì chưa gì đặt được
`revoked_at`.
