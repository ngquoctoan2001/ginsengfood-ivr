# W-0297 — Dồn `plan/ivr-orther` thành hai file trạng thái

Ngày 16/09/2026. **DOCS_ONLY**. Baseline `37606bc`. Không đổi file `.cs`/`.mjs`, không đổi runtime, `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Owner yêu cầu gom kế hoạch trong `plan/ivr-orther`: việc đã xong dồn một file, việc chưa xong dồn một file. Đã chọn phương án dồn triệt để sau khi trình bày ba mức và hệ quả từng mức.

## Đã làm

Xóa **27 file**, thêm **2 file trạng thái** và **1 kế hoạch mới**. Thư mục còn 5 file: `00-DA-XONG.md`, `00-CHUA-XONG.md`, `decisions-log.md`, `mobile-sip-trunk-production-32-channels-plan-2026-09-15.md`, `sip-production-dial-path-plan-2026-09-16.md`.

Viết lại **179 dòng link tại 63 file** bằng một lượt map tên file cũ → `00-DA-XONG.md#anchor` hoặc `00-CHUA-XONG.md#anchor`. Anchor đặt ngắn và cố định (`#m8-05`, `#today-01`, `#legal-od-voice-07`) để link sau này không gãy khi sửa tiêu đề. Nơi chịu nhiều sửa nhất: `prompt/_execution/prompt-execution-tracker.md` (35 dòng), `docs/review/2026-09-07-m8-worklist-claude-annotated.md` (19), `plan/toan-viec-can-lam-m8-2026-09-16.md` (19).

## Phân loại đã sửa một lần trước khi ghi

Lượt đọc đầu xếp `m8-05`…`m8-10` vào nhóm **đã xong** vì dòng trạng thái mở đầu bằng `M8_OWNER_SIGNED` / `đã ký`. Đọc hết chuỗi trạng thái thì sai: cả sáu đều kèm `M3_..._SIGNOFF_REQUIRED` hoặc `CODE_NOT_AUTHORIZED`, tức Module 8 mới ký phần mình. Đã chuyển cả sáu sang nhóm **chưa xong**.

Kết quả: chỉ **hai** gói đóng hẳn là `od-v1-signoff` (`W-0194`) và `od-v1-17-ttl-signoff` (`W-0246`). Vì vậy `00-DA-XONG.md` có thêm mục "các phần đã đóng của gói còn dở" — ghi phần đã chứng minh của `sip-05`, `m8-17`, `m8-15`, `m8-12`, `today-03` để khỏi làm lại, và trỏ ngược sang phần còn chặn.

## `decisions-log.md` không dồn được

File này có **66 link trỏ vào, 18 trong số đó từ `docs/documents/`** — nguồn nghiệp vụ do owner cung cấp, thuộc diện không sửa. Dồn nó đi là làm gãy link trong tài liệu không có quyền sửa lại. Giữ nguyên tên và vị trí; cả hai file mới đều trỏ về nó.

## Hệ quả còn lại

`.codex-doc-memory/markdown-doc-map.json` và `.artifacts/w0286/doc-map-final/markdown-doc-map.json` nay **stale**: còn 113 + 75 + 73 … tham chiếu tới các path đã xóa. Không tự sinh lại — theo `W-0075` file này chỉ được tạo bởi mapper `markdown-doc-reader` bên ngoài, viết bộ sinh thay thế là bị cấm. Đã kiểm: **không job CI nào đọc hai file này**, nên không gate nào đỏ vì chúng. Cần chạy mapper chính thức khi có dịp.

Toàn văn 27 file đã xóa lấy lại bằng `git log --all --full-history -- plan/ivr-orther/<tên file>`.

## Kiểm chứng

| Gate | Kết quả |
| --- | --- |
| `gate-status` | PASS |
| `compliance-pack-selftest` | PASS |
| `docs-selftest` | PASS |
| `ci-config-selftest` | PASS |
| `review-gate-selftest` | PASS |
| `generate-test-traceability --check` | PASS, `TEST_TRACEABILITY_CURRENT=671` — kiểm lúc soạn là `668`; `W-0298` (`92091a1`) vào trước lượt này và thêm `3` test, nên số đã ghi lại theo trạng thái thật lúc commit |

Quét link treo sau khi xóa: không còn link markdown nào trỏ tới 27 file đã xóa. Các chuỗi còn khớp tên cũ đều là tên bị cắt trong văn xuôi (`m8-16-…md`), glob trong lệnh git (`questions-to-*`), hoặc file đã xóa từ đợt trước — không phải link.

## Khai báo phép kiểm C2 — W-0349, 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Hồ sơ này trước không khai test hay phép kiểm nào, nên C2 không xét được. [Khai báo](acceptance-tests.json) nay ghi:

- Việc thuần tài liệu: Chỉ dồn, xoá tài liệu kế hoạch trong plan/ivr-orther và viết lại link; commit 257cbef không chạm src/, tests/ hay script gate nên không có khẳng định phần mềm nào để test kiểm. Danh sách 3 tài liệu nằm trong khai báo.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.

## Bản đồ tài liệu đã sinh lại — W-0353, 24/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Mục "Hệ quả để lại" ghi hai bản đồ `markdown-doc-map.json` bị cũ và cần chạy mapper chính thức khi có dịp.

- **Bản đồ tài liệu sinh lại bằng mapper chính thức `markdown-doc-reader`** (W-0075 chỉ cho phép bản do mapper này
  sinh). Chạy trên một clone sạch của commit `e95ba64`, tức chỉ các tệp được track: 834 tệp Markdown, 1564 link giải
  được, 493 link không giải được. Cả 493 đều trỏ tới tệp hoặc thư mục có thật mà mapper không lập chỉ mục (`.cs`,
  `.json`, `.yaml`, thư mục); không link nào trỏ tới đích không tồn tại. Kiểm bằng cách giải từng `rawTarget` so với
  cây của clone. Dòng `Root` của bản đồ ghi path của clone đó, vì bản checkout chính có các worktree chưa track dưới
  `.claude/worktrees/` mà mapper sẽ quét cả vào. Trước khi sinh lại, 18 link hỏng ở 9 tệp đã được sửa: link tới
  `admin-ui/` đã xoá ở W-0253, link tới thư mục `.artifacts/` chỉ có trên máy, và link tương đối cũ của
  `docs/review/2026-09-07-m8-worklist-claude-annotated.md`.

Bản thứ hai, `.artifacts/w0286/doc-map-final/markdown-doc-map.json`, nằm trong `.artifacts/`: thư mục này bị gitignore,
chỉ có trên một máy và không thuộc repo, nên không có gì để sinh lại ở đó.

## Owner nghiệm thu — 25/09/2026

Toàn chỉ thị “tiếp đi cho full luôn”, trong khuôn ủy quyền “thì cái nào xong cho xong luôn đi”. Theo
[danh sách W-0353](../W-0353/README.md), W-0297 chuyển **EVIDENCE_SUBMITTED → ACCEPTED** cho phần đã
làm. Bản đồ tài liệu sinh lại bằng mapper chính thức ở b92942e (W-0353); bản thứ hai nằm trong
.artifacts/, chỉ có trên máy và không thuộc repo. Các mục khác trong Residual là đính chính đã ghi.
Claude chọn theo tiêu chí phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`b92942e`. REAL_CUSTOMER_CALL_ALLOWED=NO.
