# W-0310 — Một phiếu duy nhất gửi Module 3

Ngày 17/09/2026. **TESTS_PASS** (docs-only). Baseline `d655989`. `REAL_CUSTOMER_CALL_ALLOWED=NO` — không đổi.

Owner yêu cầu: *"cập nhật lại 1 file duy nhất… bắt nó chốt 1 lần 1 rồi gửi lại."*

Gộp **ba** phiếu rời thành **một** [`IR-07`](../../../integration-requirements/07-module-3-decision-sheet.md), từ `21` câu lên **`30`**.

## Gộp cái gì vào đâu

| Phiếu cũ | Nay | Trạng thái trước khi gộp |
| --- | --- | --- |
| `IR-07` — 21 câu tích hợp | nhóm `B1`–`B4` | chưa gửi |
| Giới hạn số cuộc gọi (`15/09`) | nhóm **`B5`**, `M3-22`…`M3-25` | chưa gửi |
| `OD-18` thẩm quyền (`27/08`) | nhóm **`B6`**, `M3-26`…`M3-30` | **đã gửi, `3` tuần chưa hồi đáp** |

Thêm **bảng điền nhanh `30` dòng** ngay đầu phiếu — mỗi câu một dòng tóm tắt, một ô trả lời. Đó là thứ biến "đọc `630` dòng" thành "điền một bảng"; phần thân chỉ mở ra khi M3 muốn biết **vì sao** hoặc định trả lời khác.

## Phát hiện: phiếu đang chỉ sai phiên bản contract hai bản

`IR-07` ghi `1.0.0-draft.27` và dặn M3 *"sinh lại client từ bản hiện hành `draft.27`"*. Spec thực tế tại `d655989` đã là **`draft.29`**.

Không sửa trơ con số. Đọc cả hai changelog để nói đúng mức độ, vì "bump hai bản" và "phải làm lại client" là hai việc khác nhau:

| Bước | Kết quả `oasdiff` |
| --- | --- |
| `27 → 28` | *No changes to report, but the specs are different* — `W-0302` siết ràng buộc bằng `description`, JSON Schema không so được hai giá trị anh em |
| `28 → 29` | `1 info` — thêm `GET /audit-evidence`. Kiểm lại trong spec: có thật, dòng `974` |

Nên **`27 → 29` không breaking**, và phiếu nay nói thẳng: client sinh từ `draft.27` trở lên **không phải sinh lại vì hai bản này**. Chỉ đổi con số mà không nói điều đó sẽ tạo ra một việc thừa cho bên kia — đúng loại việc thừa phiếu này sinh ra để tránh.

Ba thay đổi breaking thật vẫn ở `draft.24`/`draft.25`, giữ nguyên trong phiếu.

Cập nhật kèm: `D-1` đổi sang `draft.29`; `M3-29` đổi mốc "draft kế tiếp sau `draft.22`" → "sau `draft.29`"; Phần C mục `2` viết lại theo `W-0302` (nay **cả hai chiều** đều `422`, mỗi chiều một reason code, thay vì vế "muộn hơn" rơi xuống `500`).

## Hai phiếu nguồn: giữ nguyên, không xoá, không sửa một byte

Sở thích dọn dẹp của repo là **xoá hẳn** chứ không lưu trữ. Lần này **không** áp dụng, và lý do đủ mạnh cho cả hai:

**Bản `OD-18` đang bị ghim hash.** Nó nằm trong `deploy/ci/pins/external-decision-artifacts.sha256`. Sửa hay xoá mà không re-pin trong cùng commit là **đúng cái đã làm đỏ 8 validator từ `257cbef`** — và `W-0304` vừa mới dọn xong hậu quả đó. Kiểm lại sau khi xong lượt này:

```
sha256  plan/ivr-orther/questions-to-module-3-od18-authority.md
        fed2fe7a68dc41ac6f658fc6479163ac89e007e1ea1b6fa0126522bab54c6b0d
pin     fed2fe7a68dc41ac6f658fc6479163ac89e007e1ea1b6fa0126522bab54c6b0d   ✅ khớp
quét 18 dòng ghim: missing=0 of total=18
```

**Bản `call-limit` là bản ghi khôi phục của `W-0309`.** Gói bằng chứng `W-0309` trỏ thẳng vào nó và ghi *"đã khôi phục nguyên văn từ lịch sử git"*. Xoá nó cùng ngày nó được khôi phục sẽ làm hỏng chính gói vừa ghi câu đó. Bản ghi lịch sử đứng trên việc dọn cho gọn.

Thay vào đó sửa **bảng định tuyến** trong `00-CHUA-XONG.md`: hai dòng nhóm A và dòng "Module 3" của bảng chặn nay đều trỏ về `IR-07` và nói rõ **không gửi phiếu nào khác nữa**.

## Kiểm chứng

| Hạng mục | Kết quả |
| --- | --- |
| Đánh số nội bộ | **`30 / 30 / 30`** — mã `M3-xx` duy nhất, dòng bảng điền nhanh, tiêu đề câu ở Phần B; `M3-01`…`M3-30` liên tục, không hụt số |
| Link đổi thư mục trong `IR-07` | `4/4` phân giải được |
| Artefact dẫn ở Phần 0 | `4/4` tồn tại (`seed/…json`, `enum-labels.vi.json`, callback OAS, `IR-06`) |
| `GET /audit-evidence` | có thật trong spec, dòng `974` |
| Hash `OD-18` | khớp pin, `18/18` dòng ghim còn nguyên |
| `docs-selftest` | PASS — `DOC_LINKS_PASS`, `UT-DOC-PII-03` |
| `gate-status` | PASS |
| `detect_changes` | docs-only |

Không chạy `dotnet test`: lượt này không chạm mã.

## Còn lại

Phiếu ở trạng thái **`READY_TO_DISPATCH / NOT_SENT`**. Việc gửi là của owner — repo không gửi thay.

> **Sửa `18/09` (`W-0316`): đã gửi.** Toàn đã gửi phiếu cho Module 3 — ghi nhận ở `T1` của
> [bản vướng mắc `17/09`](../../../plan/ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md). Đính chính
> `draft.31` (`W-0312`) cũng đã gửi ngày `17/09`. Tới `18/09` Module 3 chưa phản hồi. Trạng thái nay là
> **`SENT / AWAITING_REPLY`** — cùng chữ với dòng trạng thái của `IR-07` và `00-CHUA-XONG.md`.

Ô ký đã điền sẵn phía M8 (`Toàn — Module 8`, `17/09`); phía M3 để trống.

## Sửa hai link hỏng do chính tôi gây ra

Lúc kiểm độ sâu path tương đối của gói này, phát hiện `W-0303` và `W-0305` — **cả hai đều của phiên PD** — trỏ `../../plan/ivr-orther/…`, tức `docs/plan/…`, một thư mục không tồn tại. Đúng phải là `../../../`. `W-0305` kế thừa lỗi từ `W-0303` vì tôi chép khuôn.

`docs-selftest` **không** bắt được: `DOC_LINKS_PASS` chỉ kiểm link portal sinh ra, không kiểm link tương đối trong gói bằng chứng. Nên nó xanh suốt hai lượt trong khi link vẫn hỏng.

Đã sửa cả hai trong lượt này (không file nào bị ghim hash — đã kiểm). Không sửa nội dung kết luận, chỉ sửa độ sâu path.
