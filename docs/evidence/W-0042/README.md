# W-0042 — Evidence: Chaos & resilience game-days (`P6-3`)

Ngày: `2026-08-18` · Trạng thái: `TESTS_PASS` cho 5 scenario; **không có staging** — xem §5

Báo cáo game-day đầy đủ: [`docs/gameday-report.md`](../../gameday-report.md). File này ghi phần
kỹ thuật và những gì slice phát hiện.

## 1. Discovery tìm ra một lỗi thật trong `W-0041` trước khi viết scenario nào

Trước khi dựng scenario, tôi đi tìm xem DT-04 auto-disable có hiện thực không. Có — nhưng ở
`PostgresTelephonyDispatchStore:245-250`:

```csharp
channel.FailCount = channelHealthy ? 0 : channel.FailCount + 1;
channel.Status = channelHealthy ? "IDLE"
    : channel.FailCount >= 3 ? "HEALTH_FAILED" : "QUARANTINED";
```

`W-0041` chỉ đếm `ivr_channel_quarantines_total` ở **`PostgresSchedulerStore`** (lease hết hạn). Tức
là alert `IvrChannelAutoDisableBurst` mang nhãn DT-04 lại đọc một counter mà **chuyển trạng thái
DT-04 không bao giờ chạm vào**.

Đó là kiểu hỏng tệ hơn không có alert: nó **trông như đã phủ**. Sửa trong slice này — đếm ở **cả
hai** nơi kênh bị đưa ra khỏi phục vụ — và `CHAOS-SIM-03` giữ cho nó không tái diễn.

`docs/slo.md` §4 cũng phải sửa: nó gộp luật per-kênh (`fail_count>=3`, chạy **trong code**, đồng bộ,
để kênh hỏng không được cấp phát tiếp) với alert toàn đội (nửa "báo cho người") thành một thứ.

## 2. Hai cơ chế chèn lỗi, và mức chứng minh khác nhau

| Cơ chế | Scenario | Mức |
| --- | --- | --- |
| **Toxiproxy** | `CHAOS-DB-02`, `CHAOS-RECOVERY-04` | **lỗi mạng thật** — socket bị cắt bởi thứ nằm giữa tiến trình và Postgres |
| **Chèn ở tầng mã** | `CHAOS-DOWNSTREAM-01`, `CHAOS-SIM-03` | biên ngoài chưa có endpoint thật để cắt; `P6-3` §5 cho phép |

Ghi tách ra vì hai cách chứng minh hai mức khác nhau. Dừng container cũng cắt được kết nối, nhưng
nó kiểm một thứ khác: một server **ra đi sạch sẽ**, không phải một liên kết **ngừng tải traffic**.

## 3. Giới hạn blast radius là thứ được ép, không phải được hứa

Mọi container do chính lượt chạy tạo ra trên một network dùng-một-lần, xoá cùng lượt chạy, và
không có tuyến nào ra ngoài. `CHAOS-GUARD-05` đọc `deploy/chaos/toxiproxy.staging.json` và đỏ nếu
một upstream trỏ tới hostname không phải alias container hoặc loopback.

Kiểm âm: đổi upstream thành một hostname giải được → **đỏ**; khôi phục → xanh.

## 4. Kiểm chứng

| Lệnh | Kết quả |
| --- | --- |
| `dotnet test tests/chaos/` | **5/5**, chạy lặp 3 lượt đều xanh |
| `CHAOS-DB-02` | `/health/ready` **503** khi cắt link; ghi **ném lỗi** chứ không nuốt; dòng trước sự cố còn nguyên; dòng ghi hỏng không để lại rác; **recovery 8 ms** |
| `CHAOS-DOWNSTREAM-01` | `RETRY_PENDING`, `AcknowledgedAt` rỗng, `NextRetryAt` đặt; chạy lại ngay gửi **0** lần; breaker mở sau chuỗi hỏng |
| `CHAOS-SIM-03` | **cả 5** disposition lỗi thiết bị → `IVR_TECHNICAL_EXCEPTION`, không phải no-answer; `is_counted_customer_attempt=false`; lần hỏng thứ 3 → `HEALTH_FAILED` |
| `CHAOS-RECOVERY-04` | 0 lần gửi khi store mất; sau khôi phục đúng **1** lần; chạy lại **0**; một dòng duy nhất mang idempotency key |
| `CHAOS-GUARD-05` | kiểm âm đỏ đúng lý do |
| `docs-selftest.mjs` | `DOC_CI_TOPOLOGY_PASS` (đã mở rộng cho fragment chaos, kiểm âm đỏ) |
| `test:traceability` | `TEST_TRACEABILITY_CURRENT=257` (+5) |
| `scan-pii.sh` | `PII_SCAN_PASS` |

**Bộ sinh traceability phải sửa mới thấy project chaos.** Danh sách project trong
`generate-test-traceability.mjs` là cứng và project mới nằm ở `tests/chaos` chứ không phải
`tests/Ivr.ChaosTests`. Không sửa thì bảng traceability báo phủ đủ trong khi 5 scenario nằm ngoài
nó — đúng kiểu im lặng mà `P5-1` dựng bảng này để chặn.

## 6. Bốn dòng `ARCH-05` — và một phép kiểm **tự nói dối về mình**

`2026-08-19`. Các dòng "before attempt" đều hứa cùng một điều: nguồn không trả lời được ⇒
**không dispatch**.

Mọi test sẵn có chứng minh **nửa đầu**: quyết định là hold, và **chưa có dòng attempt nào**. Không
test nào chứng minh nửa sau, vì **không test nào từng chạy scheduler sau đó**.

Khác biệt không vụn: *"chưa có attempt"* là phát biểu **về quá khứ**; *"sẽ không có cuộc gọi nào"*
là khẳng định **về một component khác**, có claim query riêng và predicate riêng.

### Đặt ở `Ivr.IntegrationTests`, không phải `tests/chaos`

Bản đầu viết trong project chaos rồi bỏ, vì hai lý do đo được:

- collection chaos **không có `ResetAsync`** — các scenario dùng suffix ngẫu nhiên để khỏi giẫm chân
  nhau;
- `TryClaimDueDispatchAsync` **chọn theo deadline trên toàn bảng**, nên trong một fixture dùng
  chung, *"scheduler không nhận gì"* và *"scheduler nhận job của test khác"* đều **không phân biệt
  được** với điều cần đo.

Nơi đã có sẵn cả cô lập lẫn seeder là file eligibility. Khẳng định vẫn là của `ARCH-05`; chỉ chỗ ở
đổi.

### Kiểm âm **sống sót** — và đó mới là phát hiện

Kiểm âm đầu: gỡ `job.eligible IS TRUE` khỏi claim query, kỳ vọng đỏ. **Nó vẫn xanh.**

Vì có **ba** guard đứng giữa một hold và một dispatch, không phải một: `eligible`, `status`,
`queue_status`. Hold đóng cả ba, nên gỡ một cái thì hai cái kia vẫn từ chối.

Nghĩa là bình luận tôi vừa viết cho chính phép kiểm đó — *"nếu ai đó bỏ predicate này, test vẫn xanh
trong khi cuộc gọi vẫn đi ra"* — **mô tả sai** thứ nó kiểm. Một phép kiểm sống sót qua đúng
regression mà nó tự nhận bắt được thì **không phải phép kiểm mà bình luận của nó nói**.

Sửa: khẳng định **từng guard theo tên**, cạnh khẳng định hành vi. Nay đổi hold thành
`READY_FOR_SCHEDULER`/`QUEUED` → đỏ, kèm đúng tên trường.

### Đối chứng: cùng scheduler ấy **có** nhận một task khoẻ

*"Scheduler không nhận gì"* cũng chính là hình dạng của một scheduler **không thể nhận gì** — thiếu
kênh, cửa sổ đã đóng, policy chưa gieo. Đối chứng dùng **cùng seeder, cùng store**, đòi nó nhận đúng
job đó. Đó là thứ biến ba lần từ chối thành **bằng chứng**.

### Dòng Trust vẫn trống, và **cố ý**

`ARCH-05` ghi "Trust/Contact resolver → Hold task / review". Nhưng trust trả lời câu *"có được **bỏ
qua** cuộc gọi cho khách đã tin cậy không"* — và **không biết thì không bao giờ được suy ra là bỏ
qua**. Hiện thực để nó thành **advisory** và **vẫn gọi** (`skipFeatureEnabled = false`).

Fail-closed ở dòng này **ngược chiều**: hold sẽ để một đơn không được xác nhận chỉ vì một tính năng
tiện lợi đang hỏng. Nửa **contact** của dòng (`CONTACT_INVALID`) mới dẫn tới hold.

Tôi **không** viết khẳng định "trust unavailable ⇒ hold", vì làm thế là **mã hoá một sai lầm vào bộ
test**. Dòng này cần **chủ sở hữu quyết** ma trận nói gì — không phải một phép kiểm đoán hộ.

## 5. Cái này KHÔNG chứng minh

- **Không có staging.** `P6-3` §4 nói chaos chạy ở dev/staging. Ở đây nó chạy trong harness tự dựng
  của bộ test. Điều đó làm blast radius **chặt hơn** (không có tuyến nào ra ngoài) nhưng cũng nghĩa
  là **chưa lượt nào chạy trên một hệ đã triển khai**. `deploy/chaos/` là config cho staging chưa
  tồn tại (`W-0063`).
- **Không có alert-fire capture thật** (§10) — không có Alertmanager để bắt. Cái có là hai nửa
  ghép lại: chaos chứng minh **sự cố thật làm counter thật nhúc nhích**; `IT-SLO-ALERT-01` chứng
  minh **luật nổ** trên hình dạng đó. Không lượt nào chứng minh cả hai.
- ~~**4/7 dòng trong ma trận `ARCH-05` §1 chưa có scenario**.~~ **Đã đóng `2026-08-19`** bằng
  `IT-ELIG-NODISPATCH-15` (3/4 dòng), và **không phải trong project chaos** — xem §6 để biết vì sao,
  điều gì nó tìm ra về **chính nó**, và vì sao dòng Trust vẫn cố ý để trống.
- **Chưa có partition một phần, webhook trùng lặp / sai thứ tự** (`P6-3` §6.1). Toxic `latency` đã
  dựng nhưng chưa scenario nào dùng.
- ~~`IT-12..17` mà `P6-3` §3/§9 trỏ tới không tồn tại.~~ **Đã quyết `2026-08-19`**
  (`OD-OPEN-01`): **sửa prompt, vì spec là nguồn sự thật**. `P6-3` §3/§9 giờ trỏ tới mục **4**
  (lease/fencing + crash recovery), **8** (dependency/auth/evidence outage fail closed) và **10**
  (migration/retention/audit/outbox recovery) của `specs/testing/03-integration-test-plan.md` —
  đúng những mục scenario thực sự phủ.
- **Recovery 8 ms đo một lần, một máy.** Nó là một quan sát, không phải phân phối. Và nó đo probe
  đầu tiên sau khi nối lại — không có khoảng chờ reconnect nào để đo.
