# W-0367 — `K-57` của kế hoạch khắc phục `25/09`: mất luồng ARI không phải lỗi của SIM

Ngày 26/09/2026 · Claude, phiên lập kế hoạch, theo yêu cầu của Toàn *"tiếp tục K-57"* · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Vì sao có việc này

`K-56` (`W-0365`) để lộ lỗi này. Khi luồng sự kiện ARI mất, cả nhánh bắt phím (từ `K-47`) lẫn nhánh phát (`K-56`) đều báo
kênh không lành, nên `FinalizeAsync` cộng một lỗi vào chuỗi lỗi của SIM. Một luồng sự kiện phục vụ mọi SIM của worker: một
lần chập mạng cộng lỗi cho mọi SIM đang có cuộc gọi, ba lần trong 10 phút (DT-04) là `HEALTH_FAILED`. Lối bật kênh của
admin từ chối mọi kênh không phải MOCK, nên kênh Asterisk đã `HEALTH_FAILED` không có lối trở lại. Cùng chỗ: raw event ghi
audio `PLAYED` cho mọi cuộc đã quay, kể cả cuộc chưa ai nhấc máy hay cuộc bị từ chối phát.

Lô được viết trên bản sao `git archive` của `main@701148f4`. Trong lúc làm, phiên kia commit `W-0366` (`Q-28.1`, `Q-28.2`),
sửa bốn file chung (`ProviderPorts.cs`, `AsteriskSchedulerDispatchGateway.cs` và hai file test). Lô được dựng lại trên
`c51976df` bằng gộp ba chiều, không xung đột, rồi land lên `main@81e08121`.

## Đã làm

| Phần | Đã làm | Phép kiểm |
| --- | --- | --- |
| Kết cục trung tính | `SimDispositionReport.ChannelHealthy`, `AsteriskAriOperationException.ChannelHealthy` và tham số tương ứng của `ITelephonyDispatchStore.FailAsync` thành `bool?`: `null` là kết cục không nói gì về SIM. Adapter ARI trả `null` cho mọi cuộc phía IVR tự kết thúc vì mất luồng (`EndedWithoutAsterisk`), ở nhánh bắt phím lẫn nhánh phát; dispatch gateway chuyển nguyên câu trả lời đó cho store. `FinalizeAsync` với `null`: không cộng lỗi, không xoá chuỗi lỗi, kênh về `IDLE` sau cooldown, không cách ly, không đếm metric cách ly; audit ghi `channel_healthy` là `null`. Mọi nơi khác vẫn truyền `true`/`false` như cũ | `UT-AST-HEALTH-01`, `IT-TEL-HEALTH-NEUTRAL-10`; `UT-AST-EVENTS-11`, `UT-AST-EVENTS-14` đổi kỳ vọng từ "không lành" sang "không nói gì" |
| Audio trong raw event | `PLAYED` chỉ khi lệnh phát đã chạy. Hai dispatch gateway báo cho `FailAsync` lệnh phát đã chạy hay chưa; `CompleteAsync` suy ra từ việc cuộc gọi có được nhấc máy (hai gateway chỉ phát khi đã nhấc máy, và chỉ hoàn tất sau lệnh phát). Cuộc đã quay mà chưa phát ghi `NOT_PLAYED`, giá trị mới: cột không có ràng buộc giá trị, và không code nào trong `src` đọc nó | `IT-TEL-AUDIO-STATUS-11`, `UT-TEL-PLAYBACK-01` |
| Spec | `specs/api/04-sim-adapter-contract.md`: một câu dưới quy tắc DT-04, rằng chỉ lỗi của chính kênh SIM mới vào `fail_count` | — |
| Ghim hash | `ProviderPorts.cs` đổi, nên `resolver_port_sha256` được ghim lại ở `dial-token-production-bundle-validator.mjs` và template `W-0183` | self-test của `dial-token-production-bundle-validator.mjs` |

## Hành vi đổi

- **Kênh SIM:** mất luồng ARI không còn cộng lỗi, không còn cách ly kênh, và cũng không xoá chuỗi lỗi đang có. Trước
  `K-57`, mỗi lần mất luồng cách ly mọi kênh đang có cuộc gọi trong thời gian cooldown và cộng cho mỗi kênh một lỗi.
- **Khách:** không đổi. Cuộc gọi vẫn kết thúc là lỗi kỹ thuật không tính lượt, vẫn được cúp máy.
- **Metric cách ly kênh** không còn đếm mất luồng. **Audit** `SIM_PROVIDER_EVENT_CAPTURED` có thể mang `channel_healthy`
  là `null`.
- **Raw event:** `audio_status` có thêm `NOT_PLAYED`; dòng cũ giữ nguyên.

## Phát hiện mới

- **`K-62`:** Asterisk không truy cập được vẫn tính vào SIM ở lần quay sau: `ASTERISK_EVENT_STREAM_UNAVAILABLE` và
  `ASTERISK_HTTP_UNAVAILABLE` báo kênh không lành. Lý lẽ của `K-57` (không phải lỗi của SIM) áp cả ở đây, nhưng việc cách ly
  đang vô tình làm cầu dao: nó dừng dispatch khi Asterisk sập, thay vì để mỗi job đốt lượt thử kỹ thuật. Cần quyết định:
  giữ cầu dao nhưng không làm hỏng SIM vĩnh viễn, hay một cầu dao riêng ở cấp worker.

## Kiểm chứng

| Phần | Kết quả |
| --- | --- |
| Build | `0` cảnh báo, `0` lỗi, trên cây chính sau khi land |
| Impact | Trước khi sửa: `ITelephonyDispatchStore.FailAsync` HIGH (32 symbol; hai luồng dispatch và hai kịch bản chaos), `AsteriskAriOperationException` HIGH (161 symbol), `CompleteAsync` MEDIUM; `PostgresTelephonyDispatchStore.FinalizeAsync` đo CRITICAL ở lô `L7` (chỉ mục lần này trả kết quả không đủ); đã báo Toàn, và đã chạy chaos cùng toàn bộ suite |
| Đột biến | 9/9 bị bắt, trên bản dựng lại: báo cáo của adapter lại tính mất luồng vào SIM; `null` tính là lỗi; `null` xoá chuỗi lỗi; từng gateway quên đánh dấu lệnh phát đã chạy (Asterisk, MOCK); mọi cuộc đã quay là `PLAYED`; mọi cuộc hoàn tất là `PLAYED`; gateway đổi `null` thành "không lành"; `FailAsync` bỏ mất cờ lệnh phát |
| Test trên bản sao | trên `c51976df` cộng lô: unit `921/922` (ca đỏ là bảng traceability chưa sinh lại), integration telephony, scheduler, chuẩn hoá kết quả `94/94`, chaos `8/8` |
| Test trên cây chính sau khi land | `main@8305ad5e` (sau `W-0366` của phiên kia) cộng lô: unit `922/922`, contract `24/24`, integration `454/454`, chaos `8/8`, tổng `1408/1408`, build `0` cảnh báo; HEAD không đổi suốt lượt |
| Test mới | 4 dòng traceability mới; bảng sinh lại trên cây chính bằng `generate-test-traceability.mjs` (`947` → `951`) |
| Gate sweep | `GATE_SWEEP_PASS 46/46 run, 26 skipped by manifest` (Git Bash, trên cây đã land); trong đó `dial-token-production-bundle-validator.mjs` (`W0183_SELFTEST_PASS`), `gate-status.mjs`, `generate-test-traceability.mjs` và `scan-pii.sh` đạt |
| Sổ trạng thái | `gate-status.mjs --write` rồi `--check`: `GATE_STATUS_PASS`, 11 gate, 355 work item |
| Phạm vi | `gitnexus detect_changes` trước commit: 110 symbol trong 15 file đã theo dõi (gồm file kế hoạch đang sửa dở của phiên khác, không thuộc lô, bảng traceability, spec và validator), 20 luồng bị ảnh hưởng, rủi ro `critical`: tất cả là hai vòng dispatch (MOCK và Asterisk); đúng với impact đã báo trước khi sửa, và đều nằm trong lượt test trên cây chính |

## Chưa làm

`K-62` như trên.
