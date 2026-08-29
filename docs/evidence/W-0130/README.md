# W-0130 — Exact candidate freeze cho W-0128/W-0129

Ngày: `2026-08-28`<br>
Trạng thái: `TESTS_PASS_LOCAL / NOT_PUSHED / NOT_PRODUCTION_READY`<br>
Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Lý do phải dựng candidate riêng

Gate 0 của cross-audit yêu cầu evidence W-0128/W-0129 phải nằm trên một revision bất biến và
không được trộn với dirty procurement WIP hoặc future work.

`origin/main@2a4f45d` có subject `save` và gộp `98` file: W-0128/W-0129, procurement R-00/R-06,
TTS WIP, W-0133 và generated/governance artifacts. Commit đó bất biến nhưng không đủ provenance
để dùng làm candidate cô lập W-0128/W-0129. W-0130 không rewrite hoặc force-push commit này.

## 2. Exact candidate manifest

| Field | Giá trị |
| --- | --- |
| Branch | `codex/w0128-w0129-candidate` |
| Worktree | `C:\Users\Administrator\Desktop\ivr-w0128-w0129-candidate` |
| Parent/baseline | `b4d8903acda736c1fe167d472e0796852bc5e839` |
| Candidate commit | `1fa01507639c8bed5e64421b62bd2d785cbc9c26` |
| Candidate tree | `6b636d6f78b431244cc6b4e308227d526fe07cf3` |
| Changed paths | `92` |
| Commit stat | `+2034 / -479` |
| Worktree state sau gate | clean |
| Remote state | branch chưa push; `main` không bị rewrite |

Inventory từ `git diff --name-only b4d8903..1fa0150` có `0` path thuộc:

- `docs/contracts/telephony-procurement-pack/**`;
- `deploy/tts/**`;
- W-0131, W-0132, W-0133 hoặc W-0134;
- capacity model/OD-19/decision log của các work sau.

Candidate giữ đúng source/docs/tests W-0128/W-0129 và ba evidence lịch sử cần thiết
W-0105/W-0128/W-0129. Hai link từ cross-audit tới procurement file chưa thuộc candidate được
chuyển thành dẫn chiếu text, nên việc loại WIP không tạo broken link giả.

## 3. Hai defect chỉ clean checkout mới lộ

### 3.1. UI typecheck phụ thuộc `.next/types` cũ

Trên worktree mới, `npm --prefix admin-ui run typecheck` đỏ sáu lỗi `PageProps/LayoutProps` vì CI
chạy typecheck trước build nhưng script chỉ gọi `tsc --noEmit`. `next typegen` sinh route types
thành công và `tsc` xanh. Candidate đổi script thành:

```text
next typegen && tsc --noEmit
```

Sau khi chuyển toàn bộ `.next` cũ ra thư mục tạm, chính script mới tự bootstrap và PASS. Đây là
gate reproducibility fix, không đổi runtime/UI behavior.

### 3.2. Hash-bound files bị Windows checkout đổi LF thành CRLF

Contract test ban đầu đỏ `23/24`: compat schema expected SHA-256 `ad2f6550…` nhưng working bytes
thành `cd6b9401…`. Git index là LF, Windows worktree là CRLF và path chưa có EOL policy. Cùng lớp
lỗi làm `openapi:drift` và docs selftest đỏ ở ba tài liệu nguồn/report khác.

W-0130 thêm `text eol=lf` cho:

- `specs/api/compat/**`;
- `docs/api-versioning.md`;
- `docs/integration-guide.md`;
- `docs/contracts/openapi-contract-diff.md`.

Các file được checkout lại từ chính HEAD; không thay nội dung contract. Compat working hash sau
đó trở lại đúng `ad2f6550…`; contract, drift và docs gates đều PASS.

## 4. Verification trên exact candidate `1fa0150`

| Gate | Kết quả |
| --- | --- |
| Build/format | `0 warning / 0 error`; format verify PASS |
| Unit | `490/490 PASS` |
| Integration | `232/232 PASS` |
| Contract | `24/24 PASS` sau LF pin; không đổi contract content |
| Chaos | `8/8 PASS` |
| Admin UI clean-first | lint; typegen+typecheck; `176/176`; Next production build PASS |
| OpenAPI | lint, validate, negative, drift/hash PASS; NSwag `no change detected` cho cả hai client |
| API docs | `14` artifact; docs/boundary/link/CI topology selftest PASS |
| Traceability/gate mirror | `465` tagged test; `11` gates / `127` work / `21` open decision PASS trên candidate ledger |
| Helm | lint + kubeconform dev/staging/lab/prod PASS; UI absent; prod ingress `[]`; UI/M3-selector negative guards PASS |
| Compose | `docker compose -f docker-compose.dev.yml config --quiet` PASS |
| Markdown map | `591` file / `651` resolved / `196` unresolved; excluded WIP không tạo link mới |
| PII scope | W-0105/W-0128/W-0129 evidence `3 file PASS` |
| Git hygiene | exact SHA/tree; 92 paths; forbidden paths `0`; `git diff --check` PASS; worktree clean |

GitNexus xác nhận lại trước reconstruction: `callIvrApi` `CRITICAL` (`63` impacted, `30` direct,
`26` process) và `ContactRejectionReason` `HIGH` (`19` impacted, `1` direct, `3` process); owner
đã được cảnh báo trước khi dựng candidate. Pre-commit detect trên sibling worktree báo aggregate
`CRITICAL` và đồng thời cảnh báo index được build ở worktree `main`, nên số aggregate chỉ dùng
advisory. Không re-index; direct inventory và full regression trên exact SHA là evidence chính.

## 5. Verdict và residual

W-0130 hoàn tất Gate 0 ở mức `TESTS_PASS_LOCAL`: đã có candidate bất biến, cô lập và reproducible.
Không nâng thành `ACCEPTED`, M3-integrated hoặc production-ready.

Residual giữ nguyên:

- branch chưa push và chưa merge;
- hosted GitLab CI chưa chạy trên `1fa0150`;
- M3 role mapping/client/shared E2E và reason visibility vẫn cần owner/M3;
- Platform secret custody/selectors, target DB, deploy/UAT và production vẫn external;
- `REAL_CUSTOMER_CALL_ALLOWED=NO`.
