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
