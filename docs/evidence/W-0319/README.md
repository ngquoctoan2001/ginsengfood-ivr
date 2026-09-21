# W-0319 — Bằng chứng nghiệm thu phải thuộc đúng commit và chạy đủ

Ngày: `2026-09-21` · Baseline: `e33927d03257492de93727fbdcab4b33e6f3f25e`

`REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Phạm vi và lỗi đã tái hiện

Owner yêu cầu sửa W-0318: kiểm đủ gate trong manifest, ràng test/sweep vào commit đang xét,
từ chối bằng chứng cũ hoặc chạy thiếu, thêm kiểm thử hồi quy. Không đổi trạng thái nghiệm thu.

Trước sửa, assertion yêu cầu từ chối log `GATE_SWEEP_PASS 1/1 run` đã đỏ: hàm trả `ok: true`
trong khi manifest dựng cho kiểm thử có hai gate. Mã cũ cũng chỉ cảnh báo tuổi file TRX;
không kiểm SHA nguồn, hash kết quả hay tập tên gate đã chạy.

## 2. Thay đổi

- `acceptance-batches.mjs` chốt SHA đầy đủ một lần; mọi lần đọc Git dùng SHA đó, không đọc HEAD động.
- Đầu vào `--evidence` thay `--trx`/`--sweep-log`. Hai tuỳ chọn cũ bị từ chối rõ lý do.
- `acceptance-evidence-lib.mjs` kiểm SHA/tree/cây sạch trước và sau cả hai lệnh; lệnh đầy đủ;
  thời gian test trước sweep; hash manifest gate và hash từng TRX/log sweep.
- Kiểm đúng tập gate, từng tên đúng một lần, đúng pass token, số gate chạy và số skip theo manifest.
  `0/0`, `--only`, dòng kết luận đơn độc, gate thiếu/trùng/lạ, nhiều lượt trong một log đều bị từ chối.
- Suy ra các project test từ solution/csproj tại SHA đó. Mỗi project phải có TRX, counter khớp kết quả,
  mọi kết quả Passed; không nhận test bị bỏ qua. Thời gian bên trong TRX phải nằm trong lượt test.
- `collect-acceptance-evidence.mjs` chạy `dotnet test Ivr.sln` rồi full sweep, tạo thư mục mới;
  chỉ ghi `acceptance-run.json` khi mọi kiểm tra đạt. Không có chế độ gắn SHA vào kết quả có sẵn.
- `--worktree` chỉ phục vụ đọc nháp, không được dùng với gói nghiệm thu.

## 3. Cách chạy

Chạy từ checkout sạch của commit cần xét. Trên Windows dùng Git Bash để sweep tìm được `sh`.
Giữ riêng các thay đổi của phiên khác; công cụ từ chối cây có file chưa commit.
Chuẩn bị dependency Node theo lockfile và bản Release của công cụ policy trước khi thu gói.
Gate `selftest-dotnet-policy.sh` gọi công cụ đó với `--no-build`, như job CI hiện có.

```sh
npm ci --prefix deploy/ci --ignore-scripts --no-audit --no-fund
dotnet restore deploy/ci/tools/Ivr.CiPolicy/Ivr.CiPolicy.csproj --locked-mode
dotnet build deploy/ci/tools/Ivr.CiPolicy/Ivr.CiPolicy.csproj --configuration Release --no-restore
node tools/dev/collect-acceptance-evidence.mjs --out .artifacts/acceptance-run-001
node deploy/ci/scripts/acceptance-batches.mjs --evidence .artifacts/acceptance-run-001/acceptance-run.json --phase P2
```

Mỗi lượt dùng một thư mục mới. Khi commit đổi, thu gói mới. Copy/touch file không đổi commit mà nó chứng minh.
Không truyền `--evidence` thì các điều kiện về lượt chạy là CHƯA KIỂM; không thể ĐẠT/XEM chỉ nhờ README.
Gói sai làm CLI trả exit code `1`, và danh sách ghi KHÔNG ĐẠT.

## 4. Kiểm chứng

| Kiểm | Kết quả |
| --- | --- |
| Regression trên mã cũ | Đỏ đúng lỗi partial sweep |
| `acceptance-batches.mjs --self-test` | PASS, giữ các ca kiểm bốn tiêu chí cũ |
| `acceptance-evidence-selftest.mjs` | PASS `42` kiểm tra; có fixture Git và chạy CLI thật |
| Hai gate qua `gate-sweep --only` | PASS riêng từng gate; đây không phải full sweep |
| `ci-config-selftest`, docs selftest, traceability | PASS; traceability giữ `722` |
| Đọc TRX thật đã lưu | Đọc đúng `8` kết quả của integration; chỉ kiểm tương thích parser, không dùng nghiệm thu |
| PII hai gói W-0318/W-0319; gate-status | PASS; mirror `308` việc gồm WIP W-0320 của phiên khác |
| GitNexus detect-changes | LOW, `0` process; đọc cả diff chung, chưa bao gồm file mới chưa track |
| Bằng chứng trong fixture | Dữ liệu tổng hợp, không phải lượt chạy solution IVR |
| GitNexus impact | LOW; hàm trong công cụ/self-test, không có runtime process bị ảnh hưởng |

Các ca gồm sai SHA ở từng lượt, thiếu SHA, đổi tree, cây bẩn, lệnh có filter/only, exit khác 0,
thiếu project, TRX cũ/trùng/sai assembly, result bị bỏ qua, hash lệch, path thoát khỏi bundle,
và collector thất bại không để lại manifest đạt. Fixture hợp lệ được CLI nhận; sau commit mới,
gói cũ bị từ chối ngay cả khi thời gian file được đổi mới.

## 5. Giới hạn

Hash và metadata giúp phát hiện trộn hoặc sửa kết quả; đây không phải chữ ký chống một người cố ý
làm giả cả manifest và kết quả. Owner vẫn phải đọc bằng chứng và duyệt.

Phần sửa công cụ ban đầu chưa tạo gói nghiệm thu mới cho toàn bộ IVR. Không nâng trạng thái các việc
P2/P3, không sửa 55 gói cũ. Lượt thu gói thật phải chạy sau khi bản sửa nằm trong commit cần xét,
trên checkout sạch. Phần tiếp theo ghi lượt kiểm chứng owner yêu cầu sau đó.

## 6. Kiểm chứng tại commit xác định — 21/09/2026

Bản sửa công cụ đã vào `main` tại `d7a3bd4d7c13974d0f14982fd1624861de9940c0`.
Hai lượt đầu trong checkout detached sạch của commit này đều bị collector từ chối và không sinh
`acceptance-run.json`:

- `.claude/worktrees/w0319-acceptance-20260921/.artifacts/acceptance-run-001`: thiếu dependency
  Node `yaml`, integration `362/363`; không chạy sweep. Đã cài bằng `npm ci` đúng lockfile.
- `acceptance-run-002` trong cùng checkout: toàn solution `1151/1151`, sweep `40/42`.
  Gate opt-out từ chối hash W-0161 do checkout Windows đổi LF thành CRLF; gate policy chưa có
  executable Release mà nó gọi bằng `--no-build`. Cả hai là lỗi chuẩn bị checkout, không dùng làm
  bằng chứng đạt.

Hash blob Git W-0161 và hash nội dung checkout sau chuẩn hóa CRLF về LF cùng bằng
`9a9de5faad8c9fe4a7c45866ca38561dffda6423d2e63ddf9ba3595a369d832b`, đúng pin của validator.
Commit `bd9ea5c64b6a6b4a1a1f34c7043f80cfea25e08f` thêm đúng một dòng `text eol=lf`
cho file đó; không đổi pin hay nội dung bằng chứng. GitNexus staged check không có symbol thay đổi.
Checkout mới `.claude/worktrees/w0319-acceptance-lf-20260921` giữ LF ngay khi tạo; dependency đã cài
và policy Release đã build. Hai gate từng lỗi đều PASS khi kiểm riêng trước lượt thu gói đầy đủ.

### Kết quả lượt đầy đủ được chấp nhận

- Commit: `bd9ea5c64b6a6b4a1a1f34c7043f80cfea25e08f`.
- Tree: `55327d0512b5314e691655ac9d3da33761c2165f`; checkout sạch trước và sau cả hai lệnh.
- Toàn solution: `1151/1151` PASS, `0` lỗi, `0` bỏ qua; unit `756`, integration `363`,
  contract `24`, chaos `8`. Test chạy `09:23:46`–`09:35:59` ngày `21/09/2026`, UTC+7.
- Full sweep chạy sau test, `09:36:00`–`09:40:54`: `GATE_SWEEP_PASS 42/42 run, 22 skipped by manifest`.
  Cả `42` gate có `argv` đã chạy; `22` mục ngoài sweep được manifest phân loại sẵn.
- Collector exit `0`, xuất `ACCEPTANCE_EVIDENCE_COLLECTED`; consumer kiểm lại SHA/tree,
  hash manifest gate, hash TRX/log, đủ bốn project và đủ gate trước khi sinh danh sách.

Gói lưu tại `.artifacts/w0319-acceptance-bd9ea5c/` ở root repo: `acceptance-run.json`,
`tests.log`, `sweep.log`, bốn file trong `trx/` và bản sao `acceptance-batches.md`.
SHA-256 của manifest:
`5521d7d6b48d522797a823a4f4fc9068e8aa0f2416a6382ac85f5e2f72521bf4`.
Checkout đã chạy và hai lượt lỗi vẫn được giữ để đối chiếu; các artifact cục bộ không nằm trong Git.

[Danh sách mới](../../release/acceptance-batches.md) có `220` ứng viên: `0` ĐẠT, `140` XEM,
`80` KHÔNG ĐẠT, `0` CHƯA KIỂM. `80` là kết luận về điều kiện nghiệm thu của từng việc,
không phải test bị lỗi. Đợt P2 có `9` và P3 có `4` việc, đều XEM.
Snapshot tracker của commit này có `44/307` việc ACCEPTED; không chuyển trạng thái nghiệm thu.

Danh sách đọc tracker tại commit đã kiểm chứng, nên Residual W-0319 vẫn là câu trước lượt thu gói.
Tracker hiện tại được cập nhật kết quả sau đó. Commit tài liệu ghi kết quả không thay thế commit
đã chạy test; không dùng gói này chứng nhận một HEAD khác. Tái sinh đúng snapshot bằng:

```sh
node deploy/ci/scripts/acceptance-batches.mjs --root .claude/worktrees/w0319-acceptance-lf-20260921 --evidence .artifacts/w0319-acceptance-bd9ea5c/acceptance-run.json
```
