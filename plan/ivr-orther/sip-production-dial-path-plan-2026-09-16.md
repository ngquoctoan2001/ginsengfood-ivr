# Kế hoạch hoàn thiện đường gọi production — Module 8

Ngày lập: **16/09/2026** · Trạng thái: **✅ XONG `16/09/2026`** — cả ba việc · Dev: Toàn

Baseline đã đối chiếu: `main@37606bc` (16/09). Mọi `file:dòng` dưới đây đọc tại baseline này — kiểm lại trước khi sửa.

**Phạm vi:** đúng ba việc còn lại mà **không phụ thuộc Module 3 hay hợp đồng nhà mạng**. Tổng **4–7 ngày công** — thực tế **xong cả ba trong `16/09`**.

Ngoài phạm vi: nối M3 thật (chờ phiếu 21 câu), cấu hình PJSIP với nhà mạng thật, đối soát cước, nghiệm thu tải qua tuyến thật. Những việc đó nằm ở [kế hoạch 32 kênh](mobile-sip-trunk-production-32-channels-plan-2026-09-15.md) và chỉ mở khóa khi có hợp đồng.

## 1. Hiện trạng đã xác minh

| Chỗ chặn | Bằng chứng tại `37606bc` |
| --- | --- |
| Đích gọi ghim alias lab | `AsteriskAriOptions.cs:10` — `DefaultDestinationAlias = "LAB-A"` |
| Validator từ chối mọi mode khác lab | `AsteriskAriOptions.cs:69` — `"Asterisk ARI is restricted to LAB_REAL_SIM execution."` |
| Caller ID cứng | `AsteriskAriSimGateway.cs:115` — `["callerId"] = "IVR-LAB"` |
| Nhánh production rơi về gateway không khả dụng | `SchedulerCapacity.cs:704` — `UnavailableSchedulerDispatchGateway` |
| Chưa có vault production | Thư mục `Telephony/` chỉ có `LabDialTokenVault.cs` và `MockDialTokenVault.cs` |
| ~~Trần gọi đồng thời đang là 1~~ | `SchedulerCapacity.cs:55` — mặc định vẫn `1` **có chủ đích**; `W-0308` chứng minh từng bậc tới `32` và cấm cấu hình vượt hợp đồng |

**Đã xong ngày 15/09, không phải làm lại:** bộ điều phối song song có trần (`SchedulerDispatchPump.cs`), giảm tải sau chuỗi lỗi, khóa sở hữu controller ARI (`AriControllerOwnership.cs`), ledger token bền qua restart (`PostgresDialTokenResolveLedger.cs`), phê duyệt gọi production gắn môi trường.

## 2. PD-01 — Đường quay số production (2–4 ngày công) — ✅ **XONG `16/09`** (`W-0303`, `a2808ce`)

Việc lớn nhất. Làm được ngay bằng **SIP peer giả lập trong mạng nội bộ**, chưa cần nhà mạng.

### 2.1. Vault và resolver production

Thêm `ProductionDialTokenVault` cạnh vault lab. Nhiệm vụ: nhận quyền quay số opaque, trả số E.164 **chỉ sống trong bộ nhớ** tại biên telephony.

- Dùng lại `PostgresDialTokenResolveLedger` đã có cho trần resolve và tính bền qua restart — **không viết ledger thứ hai**.
- Giữ nguyên `IOpaqueValueProtector` và khóa do Platform quản lý.
- Số thật **không** được ghi vào DB ứng dụng, log, trace, evidence hay callback. Kiểm cả nhánh HTTP lỗi và body lỗi của ARI.
- TTL giữ đúng bằng cuối cửa sổ xác nhận theo `00-DA-XONG.md#od-v1-17-ttl` — không mở lại quyết định đã ký.

### 2.2. Cấu hình tuyến

Thêm vào options: endpoint trunk, danh tính gọi ra được nhà mạng cho phép, transport, environment. Giá trị thật lấy từ secret store, **không commit**.

Khi chưa có nhà mạng, trỏ vào SIP peer thử trong mạng cô lập và ghi rõ trong evidence đây là **tuyến giả lập**, không phải nghiệm thu carrier.

### 2.3. Mở khóa validator

`AsteriskAriOptions.cs:69` hiện chặn cứng. Thêm một execution mode production với bộ trường bắt buộc riêng.

**Điều kiện bắt buộc:** hành vi nhánh `LAB_REAL_SIM` phải **không đổi một byte nào**. Chạy lại `AsteriskLabTelephonyTests.cs` trước và sau, so kết quả.

Không gỡ `RealCustomerCallAllowed` ở `IvrOptionsValidator.cs:51` trong đợt này — quyền gọi khách thật vẫn đóng cho tới khi có phê duyệt release.

### 2.4. Nhánh DI

`SchedulerCapacity.cs:704` cho production chọn `AsteriskSchedulerDispatchGateway` thay vì `Unavailable`, kèm đủ vault/resolver/store. Cấu hình thiếu hoặc sai phải bị từ chối lúc khởi động với mã lỗi an toàn, **không** im lặng rơi về lab.

### 2.5. Dựng địa chỉ quay số

Thay alias cứng bằng endpoint dựng từ cấu hình trunk; caller ID lấy từ danh tính được cấp thay cho `IVR-LAB`. Chuẩn hóa và kiểm định dạng số ngay trong adapter.

### Đạt khi

- Test composition root: MOCK / lab / production chọn đúng implementation; cấu hình thiếu bị từ chối.
- Hai task thử quay tới **hai đích khác nhau** trên peer giả lập.
- Sai task, hết hạn, quá trần, replay đều bị từ chối; hai tiến trình tranh cùng token không làm reset bộ đếm.
- Grep toàn bộ log/trace/evidence của lượt chạy: **không có số điện thoại, không có token thô**.
- Bài lab `LAB-A` giữ nguyên phạm vi và hành vi.

## 3. PD-02 — Nâng trần gọi đồng thời (1–2 ngày công) — ✅ **XONG `16/09`** (`W-0308`)

Máy đã dựng xong hôm 15/09. Việc còn lại là nâng số và chứng minh.

> ### Bốn trong sáu gạch đầu dòng dưới đây **đã đạt sẵn từ lượt 15/09**
>
> Kiểm lại từng dòng thay vì tin bản kế hoạch — bản này do tôi viết, và `W-0307` vừa cho thấy
> ba trong bốn dòng tôi viết cho lô đó sai. Kết quả ở đây ngược lại: kế hoạch **đòi ít hơn**
> thực tế đã có.
>
> | Gạch đầu dòng | Tình trạng thật tại `6029b19` |
> | --- | --- |
> | Trần dùng chung giữa nhiều worker | ✅ `IT-SCH-POOL-01/02/03` trên Postgres thật |
> | Vòng bảo trì vẫn tiến triển khi đầy | ✅ `UT-SCH-PUMP-02` |
> | Drain lúc tắt | ✅ `UT-SCH-PUMP-07` + `SchedulerJobHost.StopAsync` (dừng vòng **trước** khi drain, chỉ trả scope khi drain xong) |
> | Tách `MaxCallStartsPerSecond` | ✅ đã là option riêng, có validator riêng, `UT-SCH-PUMP-03` |
> | **Bậc `1→4→8→16→32`** | ❌ chỉ `32` và `8` được chứng minh **đúng tính chất trần**; `16` **không xuất hiện ở đâu cả** |
> | **Bậc cuối phải khớp số kênh hợp đồng** | ❌ là một câu văn, **không có gì cưỡng chế** |
>
> ### Việc thật của `W-0308` là hai dòng đỏ đó
>
> **`UT-SCH-PUMP-11`** — `[Theory]` năm bậc `1/4/8/16/32`, mỗi bậc: giữ đúng `N`, `N+1` **không
> bao giờ được claim**, một cuộc kết thúc mở đúng **một** suất. Đột biến `>=` → `>` trong pump
> làm **cả 5 bậc đỏ**.
>
> **`SchedulerTrunkCapacityValidator`** — process **từ chối khởi động** khi một worker được cấu
> hình giữ nhiều cuộc hơn `ContractedChannels`, hoặc mở nhanh hơn `SipTrunk:MaxCallStartsPerSecond`.
> Doc-comment của `ContractedChannels` **tự nói** nó tách ra *“so that the two can be compared”*,
> và runbook `PD-03` kết thúc đúng bằng câu *“nothing warns you when they disagree — which is
> worth a gate of its own and does not have one yet.”* Nay đã có.
>
> ⚠️ **Cần, chưa đủ — nói rõ để không ai đọc thành hơn thế.** `MaxConcurrentDispatches` là
> **mỗi tiến trình**, hợp đồng là **toàn hệ thống**. Hai pod cùng đặt `32` trên hợp đồng `32`
> thì **từng pod đều qua** được kiểm tra này. Thứ giữ được ranh giới toàn hệ thống vẫn là số
> hàng `ivr_sim_channels` dưới `SKIP LOCKED` — một cái bảng, dùng chung, và **không validator
> cấu hình nào nhìn thấy được**. Kiểm tra này bắt đúng một lỗi: một worker bị bảo giữ nhiều
> hơn số đã mua.

- Nâng `MaxConcurrentDispatches` qua cấu hình theo bậc **1 → 4 → 8 → 16 → 32**, mỗi bậc chạy lại bộ test.
- Dùng `SchedulerDispatchPumpTests.cs` và `SchedulerSharedPoolTests.cs` đã có; thêm case cho bậc mới thay vì chỉ sửa hằng số.
- Kiểm trần dùng chung giữa nhiều worker vẫn giữ: tổng active ≤ N, task thứ N+1 chờ và được chạy khi có suất trống.
- Kiểm vòng bảo trì lease/deadline/heartbeat **vẫn tiến triển** trong lúc N cuộc đang chờ phím.
- Kiểm drain lúc tắt: ngừng claim, chờ có thời hạn, không trả suất khi chưa xác nhận cuộc cũ kết thúc.
- Tách `MaxCallStartsPerSecond` khỏi số cuộc đồng thời — có N suất trống không có nghĩa được mở N cuộc trong một giây.

**Lưu ý:** bậc cuối cùng đặt bao nhiêu **phải khớp số kênh hợp đồng thật**. Trước khi có hợp đồng thì dừng ở mức chứng minh phần mềm, không cấu hình 32 vào môi trường chạy.

### Đạt khi

Test giữ đúng N tác vụ chạy đồng thời ở mỗi bậc; tác vụ N+1 chờ; hai worker tranh việc không vượt trần chung và không originate hai lần cho một attempt; maintenance không đứng.

## 4. PD-03 — Tài liệu vận hành và bàn giao (1 ngày công) — ✅ **XONG `16/09`** (`W-0305`, `d207526`)

Đã có: [runbook lấy lại quyền điều khiển Asterisk](../../docs/operations/ari-controller-ownership.md) (commit `ff22227`).

Còn thiếu:

- Bảng cấu hình production đã che secret, kèm ý nghĩa từng tham số.
- Quy trình nâng/giảm số kênh và chỗ phải sửa khi hợp đồng đổi.
- Ngưỡng cảnh báo và cách đọc trạng thái controller trên `/healthz`.
- Quy trình xử lý cuộc gọi trạng thái không rõ: cách ly suất, đối soát, đường phục hồi có audit.
- Cách dừng khẩn: kill switch, thời gian hiệu lực đã đo.

## 5. Thứ tự và phụ thuộc

```text
PD-01.1 vault  ─┐
PD-01.2 config ─┼→ PD-01.3 validator → PD-01.4 DI → PD-01.5 địa chỉ → PD-01 đạt
                │
PD-02 ──────────┘  (chạy song song được, chỉ cần pump đã có)

PD-03 làm sau khi PD-01 và PD-02 đạt
```

| Việc | Ngày công | Chặn bởi |
| --- | --- | --- |
| PD-01 | 2–4 | ✅ **XONG** `16/09` — `a2808ce` (`W-0303`) |
| PD-02 | 1–2 → **0,5** | ✅ **XONG** `16/09` — `W-0308` |
| PD-03 | 1 | ✅ **XONG** `16/09` — `d207526` (`W-0305`) |
| **Tổng** | **4–7 → xong trong `1` ngày** | |

## 6. Sau khi ba việc này xong thì còn gì

Không phải xong Module 8. Còn hai khối **không do dev quyết được**:

| Khối | Ngày công | Mở khóa khi |
| --- | --- | --- |
| Nối M3 thật + môi trường triển khai | 4–6 | M3 trả lời [phiếu 21 câu](../../integration-requirements/07-module-3-decision-sheet.md) và Platform cấp môi trường |
| Cấu hình trunk thật, đối soát cước, nghiệm thu tải | 8–12 | Ký được hợp đồng nhà mạng |

Ngoài ra bản kiểm 16/09 (`plan/toan-viec-can-lam-m8-2026-09-16.md`) còn liệt kê các mục thuộc phạm vi rộng hơn của Module 8 — TTS self-hosted, program_code, taxonomy result code, contact gate, kết nối M2. **Không gộp chúng vào con số 4–7 ngày này.**

## 7. Quy tắc giữ nguyên

1. Repo chỉ làm trên `main`, không tạo branch hay worktree nhánh mới.
2. Trước khi sửa function/class/method: chạy GitNexus impact và báo phạm vi ảnh hưởng. Trước commit: chạy detect changes.
3. Không đổi tên event/endpoint/enum/field mà module khác tiêu thụ. Vướng contract thì dừng và ghi ESCALATION.
4. Không mở `RealCustomerCallAllowed`, không thêm số khách vào allowlist lab, không đổi tên môi trường để né guard.
5. Mỗi mốc ghi evidence theo tracker; thiếu tuyến hoặc quyền thì để `VENDOR_INPUT_REQUIRED` / `BLOCKED_EXTERNAL`, tiếp tục phần cô lập còn làm được.
