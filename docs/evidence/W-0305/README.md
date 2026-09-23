# W-0305 — Sổ tay vận hành luồng quay số production

Ngày 16/09/2026. **TESTS_PASS** (docs-only). Baseline `52044ce`. `REAL_CUSTOMER_CALL_ALLOWED=NO` — không đổi.

PD-03 của [kế hoạch luồng gọi production](../../../plan/ivr-orther/sip-production-dial-path-plan-2026-09-16.md). Một file mới: [`docs/operations/production-dial-path.md`](../../operations/production-dial-path.md).

Không sửa file `.cs` nào, không migration, không OpenAPI. Không có symbol nào để chạy impact; `detect_changes` chạy trước commit theo `CLAUDE.md`.

## Năm mục kế hoạch yêu cầu

| Mục | Nội dung |
| --- | --- |
| Bảng cấu hình đã che secret | §1 — hai section, từng khóa kèm **ai cấp giá trị** và **giá trị sai trông ra sao** |
| Quy trình nâng/giảm số kênh | §2 — ba con số phải khớp nhau, thứ tự nâng, chỗ phải sửa khi hợp đồng đổi |
| Ngưỡng cảnh báo + đọc `/healthz` | §3 — hình dạng body, ngưỡng `stale`, 4 metric có thật kèm ngưỡng khởi điểm |
| Cuộc gọi trạng thái không rõ | §4 — quarantine, `RECOVERY_REQUIRED`, lối phục hồi có audit |
| Dừng khẩn | §5 — kill switch và `terminate-all` là hai việc khác nhau, kèm thời gian |

Runbook viết bằng tiếng Anh cho khớp `docs/operations/` (`ari-controller-ownership.md`, `gitlab-runner-winhost.md`); gói bằng chứng giữ tiếng Việt cho khớp các gói khác.

## Ba phát hiện trong lúc đọc mã để viết

**Enable kênh là một chiều với mọi adapter không phải MOCK.** `InternalAdminApiService.cs:760` từ chối enable bất kỳ kênh nào có `AdapterMode != SimAdapters.Mock`, và từ chối thêm khi kênh `QUARANTINED`/`HEALTH_FAILED`/còn `fail_count`. Lối disable **không** có ràng buộc nào tương ứng. Nên một kênh trunk bị quarantine **không thể đưa lại vào vận hành qua API**, chỉ còn cách tác động thẳng vào DB.

Ghi lại **đúng như quan sát**, không gọi là lỗi: đây có thể là thiết kế fail-closed cố ý, và đổi một endpoint thuộc `AdminPolicies.Danger` không phải việc của một task tài liệu. Phiên `ivr-9f` đang trình owner như một ứng viên Work ID riêng.

**Ba con số trần kênh nằm ở ba nơi và không có gate nào bắt khi chúng lệch.** `SipTrunk:ContractedChannels` (hợp đồng bán), `Scheduler:MaxConcurrentDispatches` (một tiến trình giữ), và số hàng `ivr_sim_channels` (pool dùng chung). Trần thật là **số nhỏ nhất**. Hợp đồng 32 kênh + worker cấu hình 32 + pool 4 hàng cho ra 4 cuộc, và không chỗ nào báo sai cấu hình.

**Kill switch không chịu độ trễ cache 15 giây.** `FeatureFlagPlatform` cache snapshot 15s, nhưng `DispatchGate.cs:26` đọc bằng `forceFresh: true`, nên luồng quyết định quay số luôn đọc tươi. Hệ quả ngược lại cũng đúng và đáng cảnh báo: **dashboard đọc cùng cờ đó mới là thứ có thể cũ tới 15s**. Nếu bảng điều khiển và bộ quay số nói khác nhau, bảng điều khiển là cái sai.

## Thời gian trong §5 là suy ra, không phải đo

Kế hoạch ghi "thời gian hiệu lực **đã đo**". Chưa đo được, và runbook nói thẳng điều đó ngay trong bảng.

| Con số | Nguồn |
| --- | --- |
| Kill switch → cuộc kế tiếp | Đọc mã: `forceFresh: true` bỏ qua cache |
| ~1s trước khi một cuộc lẽ ra khởi động | `PollIntervalMilliseconds` mặc định |
| `terminate-all` → cúp máy ≤ 500ms | `TerminationPollMilliseconds`, sàn 200ms |
| Drain 180s / grace 210s | `DispatchDrainSeconds` + `deploy/helm/ivr/values.yaml:45` |

Đây là **giá trị cấu hình và hằng số trong mã**, có test ghim, chạy trên đồng hồ giả. Con số **không ai có** là nhà mạng mất bao lâu để giải phóng kênh sau khi mình cúp — và chính nó quyết định lúc nào suất đó thật sự trống cho cuộc sau. Runbook liệt kê nó vào mục "trang này chưa nói được gì", kèm việc phải hỏi nhà mạng bằng văn bản.

Bịa một con số "đã đo" ở đây sẽ là thứ nguy hiểm nhất một sổ tay vận hành có thể chứa.

## Trạng thái CI: 8 validator đang đỏ, và không phải của lượt này

Phiên `ivr-9f` báo, và đã kiểm lại thay vì tin ngay:

`257cbef` (`W-0297`) gộp `plan/ivr-orther` và xóa 27 file, trong đó **12 file là nguồn bị ghim hash** trong `deploy/ci/pins/external-decision-artifacts.sha256`. Header của chính file ghim đặt ra luật: nguồn bị ghim thay đổi thì cập nhật dòng tương ứng **trong cùng commit**. Lần này là xóa, và không re-pin.

Kiểm chứng tại lượt này:

```
git cat-file -e HEAD:plan/ivr-orther/questions-to-module-3-od18-authority.md
→ fatal: exists on disk, but not in 'HEAD'
```

Quét toàn bộ 18 dòng ghim trên **cây làm việc**: `missing=0 of total=18`.

Hai kết quả đó không mâu thuẫn, và khoảng cách giữa chúng mới là điều đáng ghi: **12 file đã được `ivr-9f` khôi phục và `git add` nhưng chưa commit.** Cây làm việc xanh, `HEAD` vẫn đỏ.

Đây đúng cái bẫy đã làm hỏng `main` hai lần trong 24 giờ (`1651e8f`, `ffa0284`): file sinh ra đọc **cây làm việc**, còn CI chạy trên **HEAD**. Nên lượt này **không trích dẫn bất kỳ lần quét gate xanh nào** — một lần quét chạy bây giờ sẽ xanh nhờ file của phiên khác chưa commit, và chép kết quả đó vào gói bằng chứng là khai một điều `HEAD` không chứng minh được.

`ivr-9f` khôi phục 12 file đó trong `W-0304`; 15 file còn lại cố ý để xóa.

## Kiểm chứng

| Hạng mục | Kết quả |
| --- | --- |
| `docs-selftest` | PASS — gồm `DOC_LINKS_PASS` (mọi link nội bộ trong file mới phân giải được) và `UT-DOC-PII-03` (không số điện thoại/địa chỉ thật) |
| `gate-status` | PASS |
| Đối chiếu từng `file:dòng` trích trong runbook | 14/14 khớp mã tại `52044ce` |
| `detect_changes` | **risk low**, `0` execution flow bị ảnh hưởng; mọi symbol đổi đều là section markdown, không có symbol `.cs` nào |

Không chạy `dotnet test`: lượt này không chạm mã. Con số 755/755 thuộc `W-0303`, không chép sang đây.

## Mọi con số trong runbook đều đọc từ mã

Không có giá trị nào kế thừa từ tài liệu cũ. Nguồn của từng con số:

`SipTrunkOptions.cs:170` (biên 1–200 / 1–100), `AsteriskAriOptions.cs:118` (production cấm `DestinationAlias`), `IvrOptionsValidator.cs:51` (`RealCustomerCallAllowed` chặn khởi động) và `:82` (bộ ba mode/sales/sim), `AsteriskSchedulerDispatchGateway.cs:28` (`IsReady`) và `:233` (sàn 200ms), `SchedulerCapacity.cs:55,65,147` (trần 1..256), `WorkerLiveness.cs:76` (`stale` = 3 chu kỳ), `SimChannelFailurePolicy.cs:10` (3 lỗi/10 phút), `PostgresSchedulerStore.cs:272` (quarantine), `PersistenceModelConfiguration.cs:496` (8 trạng thái kênh), `FeatureFlagPlatform.cs:36` (cache 15s), `FeatureFlagGuardrails.cs:143` (nhả kill switch là nới quyền), `IvrTelemetry.cs:205` (tên metric), `values.yaml:45` (grace 210s).

## Phối hợp hai phiên

`W-0305` do `ivr-9f` cấp; họ nhận `W-0306` và **chưa ghi dòng `W-0304`**, nhường sổ cho lượt này. Control field tăng `NEXT_WORK_ID` → `W-0306`.

Commit stage đúng **năm** path bằng `git commit -- <paths>` (runbook, gói bằng chứng, tracker, `gate-status.yaml`, `readiness-board.md` — hai file cuối do `gate-status.mjs --write` sinh từ tracker nên phải đi cùng), **không** `git add -A`: 12 file khôi phục của `ivr-9f` đang nằm sẵn trong index, và một lần `git commit` không giới hạn path sẽ cuốn hết chúng vào lượt này — đúng cái `A-0625` đã gặp.

`gate-status.yaml` đi cùng commit với dòng tracker. `ivr-9f` đính chính lý do họ đưa ra trước đó: không chỉ con trỏ evidence, mà `gate-status.mjs` còn đọc gate row và open decision từ chính sổ này, nên **bất kỳ dòng nào** trong tracker cũng làm yaml dịch chuyển.

## Owner nghiệm thu — 23/09/2026

Toàn chỉ thị “chấp nhận nhóm A và B” theo [phiếu W-0348](../W-0348/approval-request.md) tại
`7fc9806`. W-0305 thuộc nhóm A2 và chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm: Sổ tay vận hành
luồng quay số production. Claude ghi nhận quyết định của owner, không tự cấp phê duyệt.

Còn mở, không thuộc nghiệm thu này: giá trị nhà mạng cần hợp đồng; bật lại kênh một chiều là quyết
định của owner. Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt
collector tại `ca4f442` (1200/1200 test, sweep 43/43), không coi commit tài liệu là một lượt
test/sweep mới. REAL_CUSTOMER_CALL_ALLOWED=NO.
