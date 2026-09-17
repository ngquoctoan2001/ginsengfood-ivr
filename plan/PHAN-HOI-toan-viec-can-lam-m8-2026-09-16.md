# ĐÁNH GIÁ CỦA NGUYỄN QUỐC TOÀN — Dev trực tiếp Module 8

## Phản hồi chính thức cho `toan-viec-can-lam-m8-2026-09-16.md`

> **Người viết:** **Nguyễn Quốc Toàn** — dev trực tiếp và **duy nhất** của Module 8 (IVR Order
> Confirmation). Tôi viết code, viết test, viết migration, viết contract, dựng CI, dựng lab
> Asterisk, viết tài liệu bàn giao và chạy server test của module này. Không có ai khác trong
> Module 8.
>
> **Ngày:** 16/09/2026 · **Baseline kiểm:** `main@37606bc` · **Đối tượng:**
> [`plan/toan-viec-can-lam-m8-2026-09-16.md`](toan-viec-can-lam-m8-2026-09-16.md)
>
> **Đây là đánh giá của tôi, không phải của AI review và không phải của chief auditor.** Mọi con số
> trong file này tôi đọc ra từ repo và chạy lệnh kiểm được — lệnh tái lập ở **Phụ lục A**. Chỗ nào
> bản `16/09` đúng, tôi ghi **ĐỒNG Ý** và nhận việc. Chỗ nào sai, tôi **BÁC** kèm bằng chứng
> `file:dòng`. Không có mục nào tôi để trống.
>
> **▸ Cập nhật `16/09` — đã bắt đầu khắc phục.** Kế hoạch thi hành ở
> [`ke-hoach-khac-phuc-m8-2026-09-16.md`](ke-hoach-khac-phuc-m8-2026-09-16.md).
> ✅ **`C15`** · ✅ **`B6`** · ✅ **`B3`** · ✅ **`B4`** · ✅ **`C6`** (`W-0298`→`W-0302`) — hết `3,75` ngày,
> `1105/1105` test pass.
>
> **▸ `W-0304` — lô tài liệu `11` mục, xong `16/09`.** Đóng thêm ✅ **`B10`** · ✅ **`C7`** ·
> ✅ **`C8`** — một bảng map `result_type` → `cancellation_reason_code` ở `IR-06 §4.3` đóng cả ba
> ô, vì `C7`≡`C8` và `B10` hỏi cùng một thứ. Cộng **một vế của `A1` tôi công nhận và đã sửa**:
> `OD-V1-01/02/03/05` từ ✅ `CLOSED` đổi thành ⏳ `M8_POSITION_SIGNED / M3_NOT_RECEIVED`, vì chữ
> ký là của tôi một mình trong khi owner của chúng ghi là M3. Nội dung quyết định **không đổi một
> chữ**, `0` dòng code đổi — chỉ cái nhãn trở nên đúng.
>
> ⚠️ **Lô này tìm ra thứ không ai biết: `11` CI gate đang đỏ.** Lượt dọn tài liệu `W-0297`
> xóa `27` file, trong đó `12` file là **đầu vào CI được ghim hash**, và không re-pin cái nào.
> Đã khôi phục và cả `11` gate xanh lại. **`3` trong số đó là lỗi của chính tôi ở `W-0302`**: tôi
> tính hash trên bản CRLF của cây làm việc Windows trong khi git lưu LF, nên pin xanh trên máy
> tôi và đỏ trên CI. Tôi ghi thẳng ra đây: đã đòi bản `16/09` phải chính xác thì bản này phải
> chịu cùng chuẩn đó.
>
>
> **▸ `W-0306` — lô vận hành `4` mục, xong `16/09` trong `0,5` ngày** (ước `1,5`). Đóng ✅ **`B5`**
> cộng cả `3` phát hiện smoke ngoài bảng (`krb5` · seed compose · log EF). Đây là lô duy nhất mà
> `dotnet test` **không chứng minh được gì** — cả `4` mục đều đến từ việc anh **dựng thật rồi
> nhìn**, nên tôi đóng chúng cũng bằng cách dựng thật rồi đo:
>
> | | Trước | Sau |
> | --- | --- | --- |
> | `krb5` ở log migrate | `2` dòng `Error` | **`0`** |
> | policy mock sau `compose up` | `0` hàng — task nhận rồi không bao giờ gọi | **`6/6`**, tự động |
> | log worker / `120`s | `113` dòng | **`8`** dòng |
>
> ⚠️ **Một chỗ tôi cố ý làm khác chữ task, và nói rõ để anh bác được nếu không đồng ý.** `B5`
> đòi *danh sách môi trường đã chạy `W0122` bản drop*. Tôi không chép danh sách: không có cluster
> thật, CI luôn dựng DB rỗng, còn stack local thì trạng thái phụ thuộc ai đó có `down -v` hay
> không — việc không được ghi ở đâu. Một danh sách chép tay sẽ sai ngay lần sau, **và sai theo kiểu
> trông như một câu trả lời**. Runbook vì vậy đưa một câu SQL luôn đúng, đã chạy thử.
>
> **Và tôi đính chính một con số sai của chính mình** ở gói bằng chứng `W-0304`: tôi viết *“`0` pin
> lệch trên `183` đường dẫn ghim hash”*, số đúng là **`44`**. Kết luận không đổi, nhưng con số lớn
> hơn thực tế `4` lần làm phạm vi kiểm tra nghe rộng hơn nó thật — đúng loại lỗi bản đánh giá này
> đang bắt người khác, nên tôi sửa bằng dòng đính chính chứ không lặng lẽ đổi số.
>
>
> **▸ `W-0307` — `B9` `audit-evidence`, xong `16/09`.** Ô `B9` gộp *toàn bộ màn hình Admin UI*, mà
> **UI không thuộc Module 8** — M3 dựng console, tôi cấp API. Phần thuộc tôi và thực sự còn
> thiếu là `audit-evidence`: nay đã có, contract `1.0.0-draft.29`, **không breaking** nên M3 không
> phải sinh lại client. Toàn bộ solution **`1130/1130`**.
>
> ⚠️ **Một việc tôi cố ý không làm, và nói rõ vì sao.** Kế hoạch đòi *permission riêng* cho
> endpoint này. Tôi không tự cấp: bộ permission là `DF-01` — **LOCKED `7` quyền, do Permission
> Core sở hữu, không phải Module 8**. `OD-V1-20` đã phải mở và ký hẳn một quyết định chỉ để
> thêm một quyền. **Tự cấp quyền thứ tám sẽ là M8 ký vào sổ của người khác** — đúng thiếu sót
> tôi đang nêu ở `A1`. Nó là việc cần một quyết định, không phải việc bỏ quên.
>
> **Và tôi tự báo ba lỗi của mình trong chính lô này**, vì đã đòi bản `16/09` chính xác thì bản
> này phải chịu cùng chuẩn: (1) kế hoạch do tôi viết mô tả lô này bằng `4` dòng thì **`3` dòng
> sai**; (2) ngoại lệ PII không được dịch ⇒ `500` — **đúng khiếm khuyết `W-0302` vừa sửa**, suýt
> ship lại; (3) runtime trả `accessAuditId` trong khi contract khai `access_audit_id`.
>
> Điểm đáng nhớ nhất: **`11` test tôi tự viết đều xanh với bản code sai ở `(3)`** — chúng
> deserialize vào chính record đó, mà một vòng round-trip luôn tự khớp với chính nó. Thứ bắt được
> là một gate có sẵn đọc **file OpenAPI**, tức một hiện vật độc lập với code. Đó cũng là lý do tôi
> không nhận *"test xanh"* là bằng chứng đủ ở bất kỳ ô nào trong bản đánh giá này.
>
> **Hết việc trong kế hoạch khắc phục.** `W-0298` → `W-0307`, **`8` lô** (`W-0298` `W-0299`
> `W-0300` `W-0301` `W-0302` `W-0304` `W-0306` `W-0307` — bản trước tôi ghi `7`, đếm thiếu một),
> xong trong `1` ngày thay vì `7,5–9,5` ngày ước.
>
> **▸ `W-0308` — `PD-02`, xong `16/09`. Đây mới là dòng cuối.** Ngoài kế hoạch khắc phục còn một
> kế hoạch nữa không chờ ai: [đường gọi production](ivr-orther/sip-production-dial-path-plan-2026-09-16.md),
> `3` việc, ước `4–7` ngày công. `PD-01` và `PD-03` đã xong trong ngày (`W-0303`, `W-0305`);
> `PD-02` là việc cuối. **Nay cả ba đã xong.**
>
> Kế hoạch `PD-02` có `6` gạch đầu dòng và **`4` đã đạt sẵn từ lượt `15/09`** — ngược hẳn với
> `W-0307`, nơi `3/4` dòng kế hoạch của tôi *sai*. Hai kết luận trái chiều từ cùng một thói quen:
> đọc mã trước, tin kế hoạch sau. Thói quen đó mới là thứ đáng giữ, không phải kết luận nào.
>
> **Phần đáng giá nhất của lượt này không phải test.** Hai tài liệu trong repo **đã tự chỉ ra một
> lỗ hổng** và không ai đóng: doc-comment của `ContractedChannels` nói nó tách khỏi trần scheduler
> *“so that the two can be compared”* rồi không có phép so nào; runbook `PD-03` kết thúc mục
> *When the contract changes* bằng *“nothing warns you when they disagree — which is worth a gate
> of its own and does not have one yet.”* Ba con số cùng nghĩa *“bao nhiêu cuộc cùng lúc”* nằm ở
> ba nơi, trần hiệu dụng là nhỏ nhất trong ba, và **không gì so chúng với nhau**.
>
> Nay một worker cấu hình giữ nhiều cuộc hơn số kênh hợp đồng **không khởi động được**, và thông
> báo nêu cả hai con số. Là **từ chối boot** chứ không phải log, vì cấu hình vượt hợp đồng biểu
> hiện thành **lỗi mạng chập chờn** phía nhà mạng — đổ cho tuyến, nặng nhất đúng lúc tải cao
> nhất, và **qua được mọi test không có nhà mạng**, tức là qua được tất cả.
>
> Đây cũng là câu trả lời thẳng cho dòng `[m8_loi_dadat]` của bản `16/09` — *“không kiểm giá trị
> cấu hình thật trên server test/staging”*. Nhận xét đó đúng. Nhưng cách sửa không phải đi kiểm
> tay trên staging — staging chưa chạy, Platform đang chặn — mà là làm cho **cấu hình sai không
> khởi động được**.
>
> ⚠️ **Cần, chưa đủ, và tôi ghi rõ ở cả code lẫn runbook:** trần là *mỗi tiến trình*, hợp đồng là
> *toàn hệ thống*. Hai pod cùng đặt `32` trên hợp đồng `32` thì **từng pod đều qua**. Ranh giới
> toàn hệ thống vẫn do số hàng `ivr_sim_channels` dưới `SKIP LOCKED` giữ, và **không validator
> cấu hình nào nhìn thấy được một cái bảng**.
>
> **Từ đây, mọi thứ trong repo không phụ thuộc bên ngoài đã hết.** Phần còn lại của bản `16/09`
> **không phụ thuộc tôi**: chờ M3 phản hồi, chờ nhà mạng báo giá, và các phiếu hỏi.
> Toàn bộ solution **`1143/1143`**.
>
> **▸ Đính chính `17/09` (`W-0309`) — câu *“`6` phiếu nhóm A chưa gửi”* tôi viết ở đây là sai, và
> tôi đã lặp nó ba lần trước khi kiểm.** Hai tiền đề đều hỏng: (a) `W-0297` đã **xoá cả `6` file**
> khỏi cây khi dồn tài liệu — không còn gì để gửi; (b) `5/6` gửi cho **Platform / Security / Legal**,
> ba phòng ban **không tồn tại** trong tổ chức này — đúng lập luận tôi dùng để bác `A1` ở chính bản
> này. Tôi bác nó ở một chỗ rồi quay sang dựa vào nó ở chỗ khác.
>
> Đã gom lại theo tiền lệ `OD-V1-11` (`10/09`: *không có đội Legal, owner tự nhận rủi ro, ghi thành
> văn bản*): `2` phiếu **đóng hẳn** vì hỏi về một nhánh kỹ thuật đã bỏ (`values-prod.yaml` đặt
> `tts: enabled: false`), `3` phiếu gom thành **một phiếu cho Sếp** (`4` mục tiền/người/rủi ro,
> không thuật ngữ), `1` phiếu khôi phục nguyên văn gửi M3. **Câu đúng là: gửi `2` phiếu, cả hai
> đang nằm trong cây.**

---

# PHẦN 1 — TOÀN BỘ NHÓM C BỊ CHẶN BỞI PHẦN ĐƠN HÀNG CỦA MODULE 3

> # ⛔ 14/17 MỤC NHÓM C CHỜ PHẦN ĐƠN HÀNG CỦA MODULE 3
>
> # KHÔNG PHẢI "DEV CHƯA LÀM". LÀ **PHÍA BÊN KIA CỦA ĐƯỜNG NỐI CHƯA XONG.**

## 1.1 — Tình trạng thật, nói thẳng

**Module 3 đã được xây dựng và đang chạy — nhưng phần đơn hàng chưa xong.** Đây là thông tin từ
chính phía M3, không phải suy đoán của tôi.

Và **toàn bộ seam S7 gắn đúng vào phần đơn hàng đó.** Không phải một phần nhỏ — mà đúng cái phần
chưa xong:

| Nhóm C cần gì từ M3 | Thuộc phần nào của M3 |
| --- | --- |
| Producer **tạo IVR task từ một đơn hàng** | 🟠 **Phần đơn hàng** |
| Consumer **nhận kết quả gọi để đổi trạng thái đơn** | 🟠 **Phần đơn hàng** |
| `order_version` — phiên bản đơn để chống ghi đè đơn mới hơn | 🟠 **Phần đơn hàng** |
| `cancellation_reason_code` — từ vựng **lý do hủy đơn** | 🟠 **Phần đơn hàng** |
| Endpoint **thu hồi task khi đơn bị hủy / recall / sale-lock** | 🟠 **Phần đơn hàng** |
| Bên cấp `dial_token` từ **contact của đơn** | 🟠 **Phần đơn hàng** |
| Registry `program_code` — chương trình bán gắn với đơn | 🟠 **Phần đơn hàng** |
| Quyết định **đơn 24/7 đặt ban đêm** xử lý ra sao | 🟠 **Phần đơn hàng** |

> **`8/8` thứ nhóm C cần đều nằm trong phần đơn hàng.** Seam S7 **chính là** đường nối giữa đơn hàng
> của M3 và cuộc gọi xác nhận của M8. Phần đơn hàng chưa xong nghĩa là **chưa có đầu bên kia để nối
> vào** — không phải nối chậm, mà là chưa có chỗ nối.

**Vì vậy bộ tài liệu bàn giao seam tôi đã viết xong nhưng chưa gửi.** Tôi ghi rõ điều này để không
ai hiểu nhầm: gửi một bộ contract `400` dòng về đơn hàng cho một đội đang còn dở chính phần đơn hàng
thì bộ tài liệu đó **bị đọc lướt rồi bỏ đấy**. Tôi chờ phần đơn hàng gần xong rồi bàn giao, để nó
tới tay đúng lúc người ta đang code phần cần nó — đó là lúc nó có tác dụng.

> **Và chuyện "chưa gửi" không làm yếu kết luận:**
>
> Dù tôi có gửi hôm nay, M3 cũng **không trả lời dứt điểm được** các câu về `order_version`,
> `cancellation_reason_code`, hay bên cấp `dial_token` — vì những thứ đó **chưa chốt xong ở phía họ**.
>
> Cả hai đường đều dẫn tới cùng một chỗ: **`14` mục nhóm C không đóng được bằng ngày công của tôi.**

## 1.2 — Phía tôi đã chuẩn bị xong những gì

Toàn bộ nằm sẵn trong repo, bàn giao được ngay trong ngày phần đơn hàng của M3 sẵn sàng:

| Tài liệu | Nội dung | Trạng thái |
| --- | --- | --- |
| **`integration-requirements/07-module-3-decision-sheet.md`** | **Bộ đầy đủ nhất — `407` dòng.** Phần 0: tài liệu cần lấy + **3 thay đổi breaking** phải biết trước khi code · Phần A: `14` mục đã chốt, M3 chỉ cần biết · Phần B: câu hỏi chia `4` nhóm (`B1` producer · `B2` callback · `B3` bề mặt quản trị · `B4` môi trường) · Phần C: `3` chuỗi lệch phải sửa ở lớp assembler + `2` điều kiện chỉ đọc được từ code · Phần D: M3 làm gì sau khi ký · Phần E: IVR làm gì sau khi ký · Ô ký | ✅ **Xong, sẵn sàng bàn giao** |
| `integration-requirements/06-module-3-api-handover.md` | Bàn giao API chi tiết: intake, callback, `§4A` bề mặt quản trị `31` endpoint | ✅ Xong |
| `integration-requirements/08-module-3-sandbox-guide.md` | Hướng dẫn M3 **tự dựng stack IVR** bằng `docker-compose.sandbox.yml` và gọi thử, nhận payload callback do **chính runtime M8 phát** | ✅ Xong, chạy được |
| `docker-compose.sandbox.yml` | Sandbox thật + quota theo tài khoản dịch vụ trả `429 IVR_RATE_LIMITED` | ✅ Xong |
| `plan/ivr-orther/questions-to-module-3-*` | Hai phiếu hỏi hẹp: thẩm quyền `OD-18`, giới hạn số cuộc gọi | ✅ Xong |

**`IR-07` được thiết kế để trả lời trong một lượt:** mỗi câu đều **có sẵn phương án đề xuất**, M3
chỉ cần chọn **Ý** hoặc **KHÁC**, và **im lặng = đồng ý**. Đây là dạng phiếu tốn ít thời gian nhất
cho bên trả lời mà tôi biết cách làm — và nó **đợi được** cho tới lúc phần đơn hàng xong.

## 1.3 — ⛔ Vì sao vẫn phải ghi là "chặn bởi Module 3"

`C.0` của **chính bản `16/09`** — mục bắt buộc đọc trước khi làm bất kỳ mục C nào — tự định nghĩa:

> *"**Định nghĩa xong của một mục C:** hai phía M3-M8 cùng tham chiếu **một schema đã phát hành**,
> e2e `task → gọi → callback → order state` chạy trên **server test chung**."*

Đọc kỹ chuỗi e2e đó: `task → gọi → callback → **order state**`. **Mắt xích cuối cùng là trạng thái
đơn hàng.** Định nghĩa "xong" của chính bản audit **kết thúc ở phần M3 chưa làm xong**.

| Nhóm C cần | Ai làm | Có chưa |
| --- | --- | --- |
| Schema đã phát hành, hai phía cùng tham chiếu | M8 phát hành → **M3 đối ký** | M8 ✅ · M3 ⏳ |
| Producer tạo task từ đơn | **M3 — phần đơn hàng** | ⏳ chưa xong |
| Consumer nhận callback đổi `order state` | **M3 — phần đơn hàng** | ⏳ chưa xong |
| e2e hai chiều trên server test chung | **Cả hai** | ⏳ chờ nửa kia |
| Registry `program_code` | **M3 — phần đơn hàng** | ⏳ chưa xong |
| Bên cấp `dial_token` (`OD-V1-05`) | **M3 — contact của đơn** | ⏳ chưa xong |
| Endpoint revoke — M3 nhận vai producer (`M3-14`) | **M3 — hủy đơn** | ⏳ chưa xong |
| `cancellation_reason_code` — từ vựng của M3 | **M3 — hủy đơn** | ⏳ chưa xong |

### ⇒ Vậy `41%` của nhóm C là con số gì?

> Tôi có ngồi code `24/7` suốt một tháng thì `C1` vẫn `40%`, `C3` vẫn `25%`, `C13` vẫn `40%` — vì
> phần còn lại **nằm trong phần đơn hàng của một module khác**.
>
> Chấm `%` cho một người trên những mục mà **định nghĩa xong kết thúc ở `order state` của module
> khác** thì con số ấy không đo năng suất của người đó. Nó đo **tiến độ phần đơn hàng của M3**.
>
> Và chấm rồi **giao lại `59%` còn lại vào danh sách việc tuần sau của tôi** thì đó là giao một việc
> mà đầu bên kia chưa có.

## 1.4 — Điều tôi đề nghị, thay vì một con số `%`

Nhóm C nên được **gỡ ra khỏi bảng điểm của tôi** và chuyển thành **một dòng phụ thuộc**:

> *"Seam S7 (M8 ↔ M3): phía M8 sẵn sàng bàn giao — `IR-07` `407` dòng + sandbox chạy được.
> **Mở khoá khi phần đơn hàng của M3 xong và hai bên đối ký.** Từ ngày đó, phía M8 cần thêm
> `≈4` ngày để đóng toàn bộ `14` mục."*

Đó là câu trả lời thật cho *"chừng nào xong"*: **`4` ngày kể từ ngày phần đơn hàng M3 xong và hai
bên đối ký** — không phải một con số `%` đứng yên mỗi tuần rồi bị đọc thành dev chậm.

**Và tôi đề nghị một việc cụ thể, làm được ngay:** cho tôi **ngày dự kiến phần đơn hàng của M3
xong**. Có ngày đó thì tôi lùi ngược `4` ngày và ra được mốc đóng nhóm C cho cả hệ — một mốc thật,
cam kết được, thay cho một ô `%`.

Dưới đây tôi vẫn đánh giá **từng mục** nhóm C — nhưng mỗi mục đều có dòng **⛔ CHẶN BỞI M3**, và
dòng đó là câu trả lời cho *"chừng nào xong"* của mục đó.

---

# PHẦN 1A — BẢNG TỔNG HỢP: NHỮNG MỤC BẢN `16/09` GHI SAI

> Phần này gom **tất cả** chỗ bản `16/09` ghi sai trạng thái công việc của tôi — mục tôi **đã làm
> rồi** mà bị ghi là chưa, mục **không có thật**, mục **đếm trùng**, và mục **bị trừ điểm vì tôi
> tuân thủ đúng luật**. Mỗi dòng đều có bằng chứng `file:dòng` kiểm lại được, chi tiết đầy đủ ở
> **PHẦN 2**.

---

## Bảng 1 — ❌ MỤC SAI SỰ THẬT — phải rút khỏi hàng đợi

| Mã | Bản `16/09` ghi | Sự thật trên repo | Bằng chứng | Kết luận |
| --- | --- | --- | --- | --- |
| **`B2`** | `chưa làm` — *"dirty tree kéo dài **17 ngày**, không commit cũng không revert"*, nhãn `Dễ` | **Working tree sạch.** `3` file tracked và **unmodified**. File thứ tư — `Start-AndroidSoftphoneLab.ps1` — **chưa từng tồn tại ở bất kỳ commit nào**, kể cả tại `ff22227` là commit bản audit tự khai đã kiểm | `git status --porcelain` · `git ls-files -m deploy/lab/` = rỗng · `git log --all -- deploy/lab/Start-AndroidSoftphoneLab.ps1` = rỗng · `git cat-file -e ff22227:…` → `fatal: path does not exist` | ⛔ **XOÁ KHỎI DANH SÁCH.** Không phải `0%`, không phải `100%` — **mục này không tồn tại**. Đây là **lần thứ hai** nó được giao, sau khi đã bị bác có bằng chứng ở bản `07/09` |
| **Ghi chú `evidence: null` tăng `51 → 56`** | Nêu trong *"Chú ý mới `16/09`"* như một khoản nợ đang phình ra | Số `56` đúng, **nhưng không phải defect**. `gate-status.mjs:477-492` **cố ý** chỉ assert dòng prompt-backed (`^P\d+-\d+$`); dòng remediation `UNPLANNED` được đếm ra YAML để **nhìn thấy được**, không phải để bị đọc thành nợ | `gate-status.mjs:477-492`, comment tại chỗ: *"a rule that would have been satisfied by creating 20 empty directories, **which is the opposite of what it is for**"* | ⛔ **KHÔNG PHẢI VIỆC.** Vi phạm thật: **`0`**. Bản `07/09` đã rút claim này (`W-0224`). ⚠️ Đừng "trả nợ" bằng cách tạo thư mục rỗng — đó đúng là thứ gate từ chối |
| **`B12`** *(nửa ghi chú)* | Ô *"Ghi chú"* vẫn chép lại: *"auto-disable SIM `fail_count>=3` có nhưng **không giới hạn cửa sổ 10 phút** như spec §10 — lệch nhỏ, nêu để dev cân nhắc"* | **Đã có cửa sổ 10 phút từ `W0144`.** `SimChannelFailurePolicy.cs:10-12`: `AutoDisableThreshold = 3`, `FailureWindow = TimeSpan.FromMinutes(10)`, và `RecordFailure` mở cửa sổ mới khi quá hạn | `src/Ivr.Infrastructure/Telephony/SimChannelFailurePolicy.cs:10-12,19-26` | 🟨 **Tự bản audit đã biết** — dòng đính chính *"đã lỗi thời từ `W0144`"* nằm ở cột **"Nguồn spec"**, không nằm cạnh ghi chú sai. Người đọc lướt sẽ thấy lệch mà không thấy đính chính |
| **`B9`** *(nửa bằng chứng)* | Cột bằng chứng còn chép: *"`admin-ui/src/app/(console)/` có **12 trang**"*, *"còn **9 trang**"* | **`admin-ui/` không còn tồn tại trong repo.** Đã xoá hẳn ở `c0e6609` (`W-0253`) | `ls admin-ui` → `No such file or directory` · `git log --diff-filter=D -- 'admin-ui/*'` → `c0e6609` | 🟨 Ô *"Cập nhật 16/09"* có nói *"phần UI nay rời khỏi repo"*, nhưng cột bằng chứng phía trên vẫn mô tả thư mục đã bị xoá |
| **Phần `E`** | *"không build, không `dotnet test`… **Mọi số pass (`895/895`, `951/951`, `986…`) là dev tự khai**"* | **Tôi đã chạy thật:** `1060/1060` pass, `0` failed, `0` skipped, exit `0`. Số tôi khai không những đúng mà còn **thấp hơn thực tế** — bản `07/09` khai `900`, nay `1060` | `dotnet test Ivr.sln` trên `main@37606bc`: ContractTests `24` · UnitTests `713` · ChaosTests `8` · IntegrationTests `315` | ⛔ **Nghi vấn không có cơ sở.** `10` phút chạy test rẻ hơn nhiều so với một dòng nghi ngờ tính trung thực nằm trong tài liệu bàn giao |

---

## Bảng 2 — ✅ MỤC TÔI ĐÃ LÀM RỒI MÀ BỊ GHI LÀ "CHƯA LÀM"

| Mã | Bản `16/09` ghi | Việc yêu cầu | Trạng thái thật | Bằng chứng |
| --- | --- | --- | --- | --- |
| **`B5`** | **`chưa làm`** = **`0%`** | **(1)** Không sửa thêm migration đã áp dụng | ✅ **ĐÃ GIỮ** — và **chính bản `16/09` xác nhận**: *"kỷ luật 'không sửa migration đã áp dụng' được giữ trong `172` commit mới"*, *"`git diff --name-status`: chỉ `A` + `M IvrDbContextModelSnapshot.cs`"* | Mục *"Chú ý mới 16/09"* của chính bản đó |
| **`B5`** | " | **(3)** Thêm assertion rằng `__EFMigrationsHistory` + shape console khớp ở **cả hai lịch sử** | ✅ **ĐÃ CÓ TỪ TRƯỚC** — `[Theory]` với tham số **`bool dropAlreadyApplied`**, tức **chạy đúng cả hai lịch sử**; assert `GetPendingMigrationsAsync` rỗng, assert hàng legacy còn nguyên, assert constraint ném lỗi | `tests/Ivr.IntegrationTests/ExpandContractMigrationTests.cs:15-19,57,59-67,82` |
| **`B5`** | " | **(2)** Ghi vào runbook rollback | 🟨 **CÓ FILE, THIẾU MỘT MỤC** — `deploy/ci/rollback.md` tồn tại và `:62` bàn đúng hai miễn trừ drop bảng `W0122`. Thiếu: dòng liệt kê môi trường cụ thể đã chạy bản drop | `deploy/ci/rollback.md:62` |
| **`B5`** | " | *(ngoài yêu cầu)* | ✅ `docs/database/expand-contract.md:22-33` **mô tả đủ `4` tình huống database** — đúng thứ bản `07/09` đã ghi, và bản `16/09` chép lại phần hiện trạng nhưng vẫn chấm `chưa làm` | `docs/database/expand-contract.md:22-33` |
| **`A1`** | `chưa làm`, ưu tiên **số 1** | *"M8 **không bật / không code thêm** phần phụ thuộc (production resolver, callback enable, policy production) tới khi có chữ ký thật"* | ✅ **ĐÃ TUÂN — và chính bản `16/09` xác nhận**: *"mệnh lệnh 'không bật thêm phần phụ thuộc' vẫn giữ (chưa có resolver production, callback `TARGET_V1` vẫn fail-closed, `RealCustomerCallAllowed=NO`)"* | Phản biện trong chính ô `A1` |
| **`B1`** | `~80%`, *"ba việc còn lại"* | **(3)** Load test đúng kịch bản `M8-P0-009` — `32` kênh nhận `800` job trong `5` phút | ✅ **ĐÃ XONG TỪ `29/08`** — `PT-CAP-02` tồn tại, gắn `Trait("TestId","PT-CAP-02")`, comment dẫn thẳng `W-0131 / M8-P0-009 (spec §23)`. Bản `07/09` đã ghi *"**đừng giao lại như chưa làm**"* | `tests/Ivr.IntegrationTests/SchedulerPersistenceTests.cs:1246-1249,1329` |
| **`B4`** | `chưa làm` | *"gate seed theo environment"* + *"thêm test khẳng định prod không có `RUNTIME_GATE_ADMIN` sau migration"* | ❌ **HAI ĐỀ XUẤT NÀY KHÔNG THI HÀNH ĐƯỢC, đã retire ở `07/09` (`W-0213`)** — seed theo env vô nghĩa khi reader không đọc cột; test "prod không có row" sai tiền đề vì row seed mang `environment=NULL` và áp mọi env theo thiết kế. Bản `16/09` công nhận *"dev phản biện **đúng một nửa**"* rồi vẫn chấm `0%` | `RuntimeGateApprovalKinds.cs:48-55` · `IT-GATE-APPROVAL-10` |
| **`C11`** | `~15%` | — | 🟨 Ô đó tự viết *"Chặn production giữ nguyên, **có chủ đích** và chờ ngoài"*. Cổng `fail-closed` (`CallbackDeliveryOptions.cs:72-77`) là **tính năng an toàn tôi cố ý dựng**, không phải `85%` việc còn dang dở | `src/Ivr.Infrastructure/Callbacks/CallbackDeliveryOptions.cs:72-77` |

> ### ⇒ Riêng sửa `B2` (xoá) và `B5` (`0%` → `70%`), con số tổng đã đổi:
>
> | | B | C | **Tổng** |
> | --- | ---: | ---: | ---: |
> | Bản `16/09` ghi | `22%` | `41%` | **`31%`** |
> | Sau khi sửa hai mục trên | **`30%`** | `41%` | **`35%`** |
>
> **`+4` điểm chỉ từ hai mục.** Và đó mới chỉ là phần sai *sự thật* — chưa tính phần sai *cách đo*.

---

## Bảng 3 — 🔁 MỘT VIỆC BỊ ĐẾM NHIỀU LẦN

| Nhóm mục | Thực chất là gì | Đếm mấy lần | Hệ quả |
| --- | --- | ---: | --- |
| **`C7`** và **`C8`** | **Cùng một bảng map** `result_type` → mã lý do hủy đơn. Cùng `4` mã đích (`IVR_NO_ANSWER_MAX` `IVR_DECLINED` `IVR_INVALID_NUMBER` `IVR_CONFIRMATION_INVALID`), cùng bằng chứng (flow 04 `:1149-1152`, `:1200-1203`), cùng người làm (**chief**). Khác nhau đúng một chỗ: `C7` **đề xuất** mã cho `WINDOW_EXPIRED`, `C8` **hỏi** mã cho `WINDOW_EXPIRED` | **2** | *"17 việc kết nối"* thật ra là **16**. Và **cả hai đều ghi người làm là chief, không phải tôi** |
| **`C3`** · **`C4`** · **`C9`** *(nửa session)* | **Cùng một chữ ký M3** — `M3-07` / `golden_hour_session_id`. Cùng một field, cùng một điều kiện mở khoá | **3** | Một chữ ký trễ, **ba mục bị trừ điểm** |
| **`C13`** và **`C17`** | Hai mặt của **cùng một việc**: `C13` = cơ chế thu hồi, `C17` = re-check trước attempt. Cùng chặn bởi `M3-14`, cùng đã có `2` fence, cùng chờ **một** endpoint | **2** | Một chữ ký trễ, **hai mục bị trừ điểm** |
| **`B7`** và **`C13`** | `B7` (sweep chưa biết `revoked_at`) **không sửa lẻ được** — phải ship cùng endpoint revoke của `C13`. Chính ô `B7` viết vậy | **2** | `B7` xếp **ưu tiên 2 của tuần** cho một việc chưa kích hoạt được |

---

## Bảng 4 — ⚖️ BỊ TRỪ ĐIỂM VÌ TUÂN THỦ ĐÚNG LUẬT

Đây là nhóm khó chịu nhất: **bản `16/09` khen tôi làm đúng trong cùng một ô mà nó trừ điểm.**

| Mã | Điểm chấm | Lời khen trong chính ô đó | Luật tôi đang tuân |
| --- | ---: | --- | --- |
| **`C3`** | `25%` | *"Dev **giữ đúng stop rule**, không thêm field đơn phương"* | Spec V0.3 §6: *"`W-0146` đề xuất `golden_hour_session_id` nhưng **chưa được M3 ký và chưa được phép triển khai**"* |
| **`C4`** | `0%` | *"Chưa làm — **đúng thứ tự** chờ contract M3"* | Cùng luật với `C3` |
| **`C9`** | `40%` | *"vẫn không có session/priority trên wire (**đúng vì chờ M3**)"* | Cùng luật với `C3` |
| **`C10`** | `65%` | *"Shape vẫn giữ, **dev không đổi đơn phương**"* | `C.0` nguyên tắc số 1: không đổi shape/result code/tên field cho tới khi M3 nối xong |
| **`C11`** | `15%` | *"Chặn production giữ nguyên, **có chủ đích** và chờ ngoài"* | `W-0006`/`OD-V1-07` fail-closed |
| **`C12`** | `35%` | *"dev không wire policy là **đúng với quyết định đó**"* | `OD-V1-23`: opt-out explicit-only |
| **`B1`** | `80%` | *"Dev làm **đúng hướng**: giữ uncalibrated"* | Không bịa số calibrated khi chưa có phép đo |
| **`B12`** | `15%` | *"**đúng thẩm quyền** (trình lãnh đạo, không tự chốt)"* | Không tự quyết việc mua sắm |

> **`8` mục, tổng cộng bị trừ khoảng `5,7` điểm trên thang `26` — tức `~22%` của cả bảng — cho việc
> làm đúng.** Nếu tôi phá luật (thêm field khi chưa có chữ ký, đổi shape đơn phương, bật cổng
> fail-closed, bịa số calibrated) thì `%` của tôi sẽ **cao hơn**. Một thước đo thưởng cho việc phá
> luật là thước đo hỏng.

---

## Bảng 5 — 🔢 LỖI SỐ LIỆU VÀ THAM CHIẾU TRONG CHÍNH TÀI LIỆU

| Chỗ | Ghi gì | Đúng là gì | Bằng chứng |
| --- | --- | --- | --- |
| **`A1`** thân mục | *"số dòng tự đóng tăng từ `19` lên **`22`**"* | **`21`** | Đếm `open-decisions-register.md`: `21 CLOSED` · `1 HALF_SIGNED` (`OD-V1-09`) · `1 NOT_SIGNED` (`OD-V1-10`). **Phản biện ở cuối cùng ô `A1` cũng ghi `21`** — hai con số mâu thuẫn trong **cùng một ô**, ở mục xếp ưu tiên `1` |
| **`B7`** | *"phải sửa cùng lúc với việc mở endpoint revoke **(C11)**"* | **`C13`** | Bản `16/09` **đã đánh số lại**: `C11` mới = *"đường gửi callback đang tắt"*; mục revoke là `C13` (cũ `C11`). Tham chiếu chéo còn sót từ bảng số cũ |
| **Bảng biểu / định dạng** | — | ✅ **KHÔNG CÓ LỖI** — `10` bảng, `57` hàng, **`0` hàng vỡ cột** | Mọi ký tự gạch đứng bên trong các đoạn `grep` đều được escape đúng chuẩn GFM. Bản nháp đầu của tôi ghi mục này là lỗi nặng số một — **sai, tôi đã tự rút**; xem **§3.1** |
| **Toàn tài liệu** | — | **`0` ước lượng · `0` mốc · `0` ngày tháng** | `grep -oniE "(ước tính\|estimate\|man-day\|ngày công\|hạn chót\|sprint)"` = **`0` kết quả**. Chỉ có `Dễ` ×15 · `Vừa` ×12 · `Khó` ×3 — ba nhãn đo **kích thước code**, không đo **thời gian chờ** |

---

## Tổng kết PHẦN 1A

| Loại lỗi | Số mục | Mã |
| --- | ---: | --- |
| ❌ Sai sự thật, phải rút khỏi hàng đợi | **2** | `B2` · ghi chú `evidence: null = 56` |
| 🟨 Bằng chứng lạc hậu trong ô | **2** | `B12` (cửa sổ 10 phút) · `B9` (`admin-ui` đã xoá) |
| ✅ Đã làm rồi mà ghi `chưa làm` / chấm thiếu | **4** | `B5` (2/3 xong) · `A1` (đã tuân) · `B1` (việc 3 xong từ `29/08`) · `B4` (đề xuất đã retire) |
| 🔁 Đếm trùng | **4 cặp** | `C7`≡`C8` · `C3`+`C4`+`C9` · `C13`+`C17` · `B7`+`C13` |
| ⚖️ Trừ điểm vì tuân thủ đúng luật | **8** | `C3` `C4` `C9` `C10` `C11` `C12` `B1` `B12` |
| 🔢 Lỗi số liệu / tham chiếu | **3** | `A1` (`22` vs `21`) · `B7` (trỏ `C11` thay vì `C13`) · `0` ước lượng trong toàn tài liệu |
| ⛔ Nghi vấn không có cơ sở | **1** | *"số pass là dev tự khai"* → `1060/1060` |

> ### Trên `30` mục được giao, **`12` mục có vấn đề về độ chính xác trạng thái** — chưa kể `14` mục nhóm C bị chấm `%` trong khi phía bên kia chưa xong.
>
> Tôi **không** nói bản `16/09` vô giá trị — nó tìm ra `4` thứ thật và đáng giá (`C15`, `C6`,
> `IR-06:354-356`, `3` việc vận hành từ smoke), và độ chính xác `file:dòng` của nó cao. Tôi chỉ nói:
> **trạng thái công việc trong bảng không kiểm được bằng `git log` và `grep` một chiều** — phải mở
> test ra đọc, và phải chạy nó.

---

# PHẦN 2 — ĐÁNH GIÁ TỪNG TASK

Quy ước đọc:

| Ký hiệu | Nghĩa |
| --- | --- |
| ✅ **ĐỒNG Ý** | Nhận xét đúng, tôi nhận việc |
| 🟨 **ĐÚNG MỘT NỬA** | Quan sát đúng, kết luận hoặc cách sửa sai |
| ❌ **BÁC** | Sai sự thật, có bằng chứng |
| 🔵 **SAI ĐỊA CHỈ** | Việc có thật nhưng **không phải việc code của tôi** — thuộc M3, thuộc chief auditor, hoặc chờ một bên ngoài |
| ⛔ **CHẶN BỞI M3** | Không có hành động nào của tôi tồn tại cho tới khi M3 trả lời |

---

## NHÓM A — "Việc khẩn"

### `A1` — "Owner IVR tự ký 19 quyết định OD-V1 trong một lượt"

**Bản `16/09` ghi:** `chưa làm`. Xếp **ưu tiên số 1** của tuần. Yêu cầu: *"Chief auditor tách trong
register: dòng owner ngoài M8 → `M8_POSITION_SIGNED / <owner> NOT_RECEIVED`; đưa 5 dòng
(`01/02/03/05/07`) vào `FIX_M3` và `FIX_OWNER_CORE` để đúng người ký; Owner tuyên bố thẩm quyền ký;
M8 không bật/không code thêm phần phụ thuộc."*

**Sự thật trên repo** (`specs/_review/open-decisions-register.md`, đếm tại `HEAD`):

| | |
| --- | --- |
| Tổng dòng `OD-V1` | `23` |
| `CLOSED` | **`21`** |
| `HALF_SIGNED` | `1` — `OD-V1-09` |
| `NOT_SIGNED` | `1` — `OD-V1-10` |

### 🟨 ĐÁNH GIÁ CỦA TÔI — ĐÚNG MỘT NỬA, VÀ NỬA SAI LÀ NỬA QUAN TRỌNG

**Nửa đúng:** register có ghi `CLOSED` trên những dòng mà cột Owner đề tên khác. Quan sát bề mặt
chính xác. Và tôi đồng ý rằng nếu sau này có tranh chấp *"ai cho phép gọi khách thật"*, một dòng
`CLOSED` mà không rõ ai ký là chỗ yếu.

**Nửa sai — và đây là chỗ tôi bác:**

Cột `Owner` trong register ghi *"Sales Product/Core"*, *"Security/Platform"*, *"Legal/Privacy"*,
*"Product + CRM/M3"*, *"Infra/procurement"*, *"Release owner"*. **Sáu vai đó không tồn tại như sáu
đội riêng trong dự án này.** Chúng là vai mẫu chép từ khuôn spec chuẩn — hữu ích để không bỏ sót góc
nhìn nào, nhưng **không phải sáu địa chỉ có thật để gửi hồ sơ tới**.

Yêu cầu *"đưa 5 dòng vào `FIX_M3` và `FIX_OWNER_CORE` để đúng người ký"* — **đúng người ký là ai?**
Không có đội Sales. Không có đội Security. Đây là yêu cầu chuyển hồ sơ cho những phòng ban không có
thật.

**Và register đã xử lý đúng chuyện này rồi — chief chưa đọc kỹ.** Nguyên văn `OD-V1-11`, dòng `38`:

> *"**owner tuyên bố quorum là chính mình.** **Tổ chức này không có đội Legal/Privacy** và owner
> chọn **không** mua ý kiến pháp lý ngoài cho V1… **Đóng cái gì và KHÔNG đóng cái gì:** đóng
> **quyết định chính sách** (ghi âm, thời hạn lưu). **Không** đóng được ràng buộc kỹ thuật —
> `PRODUCTION_REAL` vẫn cần **ba actor id khác nhau** và code vẫn từ chối với một người, nên duyệt
> script cho production **vẫn chặn**."*

Dòng đó **tự khai thiếu đội Legal**, **tự giới hạn phạm vi**, và **tự nói phần nào nó không đóng
được**. Đó không phải chữ ký khống. Đó là hồ sơ trung thực hơn mức chief mô tả.

**Bằng chứng mạnh hơn nữa — hai dòng dính tiền, tôi KHÔNG ký:**

| Dòng | Trạng thái | Lý do ghi trong register |
| --- | --- | --- |
| `OD-V1-10` — 32 eSIM, capacity, cost | ⛔ **`NOT_SIGNED`** | *"cố ý. Con số 32 là giả định chứ chưa phải phép đo; **ký bây giờ là ký một điều chưa biết**"* |
| `OD-V1-09` — SIM lab protocol/vendor | ⏳ **`HALF_SIGNED`** | đã ký phần giao thức; **chưa ký** bảng ánh xạ tín hiệu nhà mạng vì *"chỉ đo được khi có SIM thật"* |

Hai dòng tốn tiền và cần nhà cung cấp là **hai dòng duy nhất tôi để mở**. Đó chính xác là ranh giới
nội bộ/bên ngoài. Nếu tôi "tự ký bừa" thì hai dòng này đã `CLOSED` từ lâu — chúng là những dòng dễ
ký nhất và mở khoá nhiều nhất. Tôi để mở **vì chúng phụ thuộc một phép đo và một báo giá chưa có**.

### Phần còn lại của `A1` mà tôi CÔNG NHẬN — nhỏ, cụ thể, và tôi nhận

Bỏ hết vai không tồn tại đi, còn lại **đúng hai thứ có thật**:

| # | Vấn đề thật | Ai giải quyết | Chi phí |
| --- | --- | --- | --- |
| **1** | `OD-V1-11` — *"nhận rủi ro pháp lý"* về ghi âm cuộc gọi, danh sách không gọi, thời hạn lưu. Đây là **rủi ro ở cấp công ty**, không phải một lựa chọn kỹ thuật, nên nó nên được **ban lãnh đạo biết và xác nhận** chứ không nằm im trong một dòng register | Một câu xác nhận ở cấp công ty | 1 câu |
| **2** | `OD-V1-01/02/03/05` ràng buộc **wire contract mà M3 phải implement**. Đây **không phải** vấn đề *phê duyệt*, mà là vấn đề **đồng thuận**: tôi quyết được phía IVR, M3 phải chấp nhận phía kia | **M3 đối ký `IR-07`** | ⛔ **chờ phần đơn hàng M3 xong** |

**Và tôi sẽ tự làm phần của mình:** đổi cột trạng thái các dòng chạm M3 sang
`M8_POSITION_SIGNED / M3_NOT_RECEIVED`. Không rollback, không mất vị trí kỹ thuật — chỉ nói đúng sự
thật về chữ ký. **0,5 ngày.**

### ❌ BÁC hai chi tiết cụ thể trong ô `A1`

1. **Con số sai.** Thân mục ghi *"tăng từ `19` lên `22`"*; phản biện ở cuối cùng ô ghi *"là `21`
   (không phải 22)"*. Đếm thật: **`21`**. Hai con số mâu thuẫn **trong cùng một ô**, giao cho cùng
   một người, ở mục xếp ưu tiên `1`.
2. **Mệnh lệnh *"M8 không bật/không code thêm phần phụ thuộc"* — tôi đã tuân, và chính bản `16/09`
   xác nhận:** `RealCustomerCallAllowed=NO` (`IvrOptionsValidator.cs:51-53` từ chối boot nếu YES),
   callback `TARGET_V1` vẫn fail-closed (`CallbackDeliveryOptions.cs:72-77`), chưa có resolver
   production. Phản biện trong chính ô `A1` viết: *"mệnh lệnh 'không bật thêm phần phụ thuộc' vẫn
   giữ"*. **Vậy phần tôi bị yêu cầu, tôi đã làm đúng.**

### ⇒ Kết luận `A1`

> **Đây không phải "việc khẩn số 1".** Việc duy nhất của tôi trong mục này là **đổi nhãn trạng thái
> register — nửa ngày**. Hai phần còn lại là **một câu xác nhận rủi ro ở cấp công ty** và **một chữ
> ký đối ứng từ Module 3 — mà phần đơn hàng bên đó chưa xong**.
>
> Xếp một mục mà mệnh lệnh dành cho dev là **"đừng làm gì"** lên ưu tiên `1` của một tuần làm việc
> là sai thứ tự. Ưu tiên `1` đúng phải là **`C15`** — đơn COD ban đêm đang chết lặng.

---

## NHÓM B — Code lõi

### `B1` — Capacity model 800–1200 cuộc/phiên

**Bản `16/09` ghi:** `~80%`. Còn lại: (1) calibrate 4 số sau khi có `W-0008`; (2) Owner xác nhận
`800–1200`; (3) load test — *đã xong*.

**Sự thật trên repo:** `SchedulerCapacity.cs:30` `ExpectedCallDurationSeconds = 60` (giữ nguyên, có
chủ đích) · `docs/evidence/W-0008` **không tồn tại** (`ls`) · `OD-V1-10` `NOT_SIGNED`.

### 🟨 `80%` ĐÚNG — NHƯNG `20%` CÒN LẠI ĐANG CHỜ BÁO GIÁ NHÀ MẠNG

`80%` thì tôi không cãi. Nhưng **`20%` còn lại không có một dòng code nào**, và nó đang chờ đúng một
thứ: **báo giá từ nhà mạng**.

> **Cập nhật tình hình, tính tới `16/09`:** tôi **đã liên hệ đủ cả ba nhà mạng — Vinaphone, MobiFone
> và Viettel** — và **đang chờ các bên phản hồi kèm báo giá**. Có báo giá thì mới chốt được gói,
> mới có kênh thật, mới có cuộc gọi đo được.

| Việc còn lại | Cần gì trước | Đang chờ ai |
| --- | --- | --- |
| Calibrate `35/40/50/60` | Có **cuộc gọi đo được** (`W-0008`) | ⏳ **Báo giá nhà mạng** ⇒ có trunk ⇒ đo được |
| Chốt số kênh | Biết giá từng gói | ⏳ **Báo giá nhà mạng** |
| Xác nhận đỉnh `800–1200` cuộc/phiên | Số liệu đơn hàng thực tế | Tôi đối chiếu khi có dữ liệu vận hành |

Tôi đã làm hết phần làm được **mà không cần cuộc gọi thật**: giữ model `UNCALIBRATED` thay vì bịa số
(`CAP-CALIB-03 PASS_UNCALIBRATED`), khai con số thứ tư `35s` mà `W-0132` bỏ sót (`W-0212`), sửa
checklist calibrate cho đủ **bốn** số (`W-0223`), và dựng gate chống trôi `CAP-DRIFT-05` đọc ngược
default `60s` từ code.

> **Bịa một con số calibrated khi chưa có phép đo là cách nhanh nhất để mục này lên `100%` và là cách
> nhanh nhất để sập sizing ở production.** Tôi không làm — model tự khai `UNCALIBRATED` là trạng
> thái đúng cho tới khi có phép đo thật.

**✅ Tôi nhận:** `0` ngày lúc này. **Mở lại ngay khi có báo giá và kênh thật** — lúc đó calibrate cả
bốn số (`35/40/50/60`) là **`1` ngày**, vì gate `CAP-DRIFT-05` và checklist `4` số đã dựng sẵn để
nhận phép đo.

---

### `B2` — "Chốt số phận working tree softphone lab (4 file chưa commit)"

**Bản `16/09` ghi:** `chưa làm` — *"Không đổi từ mốc: **dirty tree kéo dài 17 ngày**, không commit
cũng không revert."* Nhãn `Dễ`.

### ❌ BÁC HOÀN TOÀN — BỐN FILE ĐÓ KHÔNG TỒN TẠI

```text
$ git status --porcelain
?? docs/reports/2026-09-15-De-xuat-phuong-an-goi-dien-xac-nhan-don-hang.docx
?? plan/toan-viec-can-lam-m8-2026-09-16.md          ← chính file audit này

$ git ls-files -m deploy/lab/
(rỗng — KHÔNG có file tracked nào bị sửa)

$ git log --all --oneline -- deploy/lab/Start-AndroidSoftphoneLab.ps1
(rỗng — file này CHƯA TỪNG TỒN TẠI ở bất kỳ commit nào)

$ git cat-file -e ff22227:deploy/lab/Start-AndroidSoftphoneLab.ps1
fatal: path does not exist in 'ff22227'     ← kể cả tại commit audit tự khai đã kiểm
```

**Working tree sạch.** Ba file kia tracked và **unmodified**. File thứ tư chưa từng tồn tại ở bất kỳ
commit nào, bất kỳ checkout nào, **kể cả tại `ff22227`** — đúng commit mà bản audit tự khai là đã
kiểm.

**Mục này đã được đóng bằng bằng chứng từ `07/09`**, nguyên văn bản cũ:

> *"~~B2 4 file softphone lab dirty~~ — **hết tiền đề**. `git status` rỗng; 3 file tracked/
> unmodified; file thứ tư **không tồn tại ở bất kỳ checkout nào**."*

**Và bản `16/09` tự khai vì sao nó sai**, ở phần `E`:

> *"Working tree 4 file lab sửa dở: **chỉ `git status` và `diff --stat`, không đọc nội dung; bỏ qua
> theo yêu cầu**."*

`git status` **không thể** trả ra kết quả đó trên cây này. Dòng này **được chép nguyên từ bản
`29/08`** rồi gắn thêm chữ *"kéo dài 17 ngày"* để trông như một quan sát mới.

> ### ⇒ `B2` phải bị xóa khỏi danh sách. Không phải `0%`, không phải `100%` — **nó không tồn tại**.
> Đây là lần thứ hai mục này được giao. Lần thứ nhất đã bị bác có bằng chứng.

---

### `B3` — Attempt policy `gh-247-prod-v1` seed `approved_for_production = TRUE`

**Bản `16/09` ghi:** `chưa làm`. Hai lựa chọn: (a) Owner hệ + M3 đối ký; (b) dev tách INSERT khỏi
migration schema hoặc hạ `FALSE`.

**Sự thật trên repo** — `20260905120000_W0196SignedProductionAttemptPolicy.cs:55-59`:

```sql
('gh-247-prod-v1', 'GOLDEN_HOUR', 2, '[0, 150]'::jsonb, 300,
 '["MOCK", "LAB_REAL_SIM", "PRODUCTION_REAL"]'::jsonb, TRUE,   -- approved_for_production
 TIMESTAMPTZ '2026-09-05 00:00:00+00', 'LEGAL_DECISION_PENDING'),
```

### ✅ ĐỒNG Ý — TÔI NHẬN VIỆC NÀY

Đúng. Một hàng `approved_for_production = TRUE` đi vào **mọi database** qua migration schema, kèm
`retention_class = 'LEGAL_DECISION_PENDING'` — tức là chính hàng đó tự khai quyết định pháp lý chưa
xong. Đặt nó trong migration là sai chỗ, bất kể ai ký.

**Nhưng phải nói cho đủ:** hôm nay nó **không mở được cuộc gọi nào**. `PRODUCTION_REAL` còn bị chặn
độc lập ở `IvrOptionsValidator.cs:51-53`, `DispatchGate.cs:65-73`, `SchedulerCapacity.cs:704`. Ảnh
hưởng thực tế: **0 cuộc gọi**. Đây là nợ hồ sơ, không phải lỗ hổng runtime — và bản `16/09` cũng ghi
đúng như vậy.

**Tôi chọn phương án (b), không phải (a)** — vì (a) đòi *"Owner hệ + M3 đối ký"* mà **M3 chưa khởi
động**, nên chờ (a) là để một hàng `TRUE` nằm trong mọi database vô thời hạn. Phương án (b) đóng
được ngay: migration **mới** hạ `approved_for_production = FALSE`, **không sửa `W0196` đã áp dụng**,
sửa fixture `UT-POLICY-SIGNED` cho khớp.

### ✅ ĐÃ LÀM XONG — `16/09`, `W-0300` (commit `641d294`)

> ### ⚠️ Database bác phương án đầu — và nó đúng
> Định `UPDATE … SET approved_for_production = FALSE`. Bị từ chối:
> `P0001: attempt-policy versions are immutable; create a new version`.
> `trg_ivr_attempt_policies_immutable` là `BEFORE UPDATE … FOR EACH ROW`, **raise vô điều kiện,
> không phân biệt cột** — `W-0151` cưỡng chế ở tầng schema.
>
> **Luật đúng nên tôn trọng chứ không lách:** xoá hai hàng seed — và đó mới đúng thứ `B3` yêu cầu
> (*"tách INSERT khỏi migration schema"*). `DELETE` hợp lệ: không FK, chưa task nào nhận theo version
> này, `W0196.Down()` vốn đã xoá đúng hai hàng đó.

`IT-DB-POLICY-UNSIGNED-01` khẳng định **không hàng nào** claim production approval — không nêu tên
version, để seed lại dưới tên khác cũng không lọt. Mutation hai chiều đã chạy. `1065/1065` test pass.

**Catalogue C# cố ý không đụng:** chỉ tới được qua in-memory registry, mà `AddIvrFoundation` ném lỗi
ngoài `MOCK` và hai host production đều truyền `false` — lật nó nữa là xoá một bản ghi quyết định mà
không đổi gì runtime.

**Còn lại:** `OD-V1-08` vẫn cần **Product + Order Core + M3** đối ký. Lượt này không thay cho ba chữ
ký đó; nó chỉ làm cho việc thiếu chữ ký **không còn tự cấp phê duyệt cho mọi database**.

---

### `B4` — `RUNTIME_GATE_ADMIN` seed vào mọi môi trường (`environment = NULL`)

**Bản `16/09` ghi:** `chưa làm` — *"Dev phản biện đúng một nửa: seed theo environment vô nghĩa khi
reader không đọc cột… Nhưng phương án còn lại chưa làm."*

**Sự thật trên repo** — `20260905034908_W0195RuntimeGateApprovals.cs:148-152`: `approval_kind =
'RUNTIME_GATE_ADMIN'`, `environment = NULL`, `approver_actor_id = 'ivr-owner'`.

### 🟨 ĐÚNG MỘT NỬA — VÀ NỬA CÒN LẠI LÀ LẬP LUẬN THIẾT KẾ, KHÔNG PHẢI VIỆC BỎ QUÊN

Lý do coarse-by-design **đã ghim trong code từ trước**, `RuntimeGateApprovalKinds.cs:48-55`:

> *"The other two kinds go through `AnyLiveAsync`… **Administration being coarse is deliberate** —
> the environment-specific decision is the four-eyes row on each individual change — but writing an
> environment on one of those rows **looks** like scoping and is not. Narrowing an existing row is
> refused outright by the append-only trigger… `IT-GATE-APPROVAL-10` holds both halves of that."*

Nghĩa là: quyền **quản trị** cố ý thô; quyết định **theo môi trường** nằm ở hàng four-eyes của từng
thay đổi cụ thể, và hàng đó băm cả `Environment` vào fingerprint. Ghi `environment` lên hàng admin
**trông như** scope mà **không phải** — đó chính là cái bẫy tôi đã viết test để ghim lại.

**Đây là bất đồng thiết kế cần một phán quyết, không phải "dev chưa làm".**

**Nhưng tôi công nhận một điều:** tôi vừa siết đúng lớp lỗi này cho `PRODUCTION_CALL`
(`20260915122953_ProductionCallApprovalIsEnvironmentScoped`, `AnyLiveForEnvironmentAsync`). Nếu luật
đó đúng cho `PRODUCTION_CALL` thì lập luận *"admin thô là cố ý"* yếu đi. **Và vì tôi là thẩm quyền
kỹ thuật của Module 8, phán quyết này là của tôi và tôi ra quyết định ngay tại đây thay vì đẩy nó
thành một câu hỏi treo.**

**Quyết định của tôi: SIẾT.** Plumbing đã có sẵn, chi phí thấp, và *"trông như scope mà không phải"*
là một cái bẫy tôi không muốn để lại.

### ✅ ĐÃ LÀM XONG — `16/09`, `W-0301` (commit `1651e8f`)

> ### ⚠️ `gitnexus_impact` = **HIGH** — `20` điểm chạm, `4` implementation, `13` test
> Ước lượng `0,5` ngày của tôi **sai**; thực tế `1` ngày. `IRuntimeGateAuthorization.IsApprovedAsync`
> không có tham số `environment` (khác `IProductionCallGate`) nên phải đổi chữ ký interface.

> ### ⚠️ Trigger schema lại quyết định thiết kế — lần thứ hai trong ngày
> `trg_ivr_runtime_gate_approvals_append_only` cấm **cả** đổi `environment` **lẫn** `DELETE`, chỉ cho
> **revoke**. Nên: revoke hàng seed, **không seed thay thế**. Và `CHECK` phải thêm vế
> `OR revoked_at IS NOT NULL` — **bắt buộc**, vì hàng vừa revoke giữ `NULL` vĩnh viễn.

**Tôi tự lật lập luận của chính mình.** Doc-comment cũ viết *"administration is coarse on purpose"* —
đọc thì xuôi và sai ở đúng một chỗ: người duyệt điền `environment = 'lab'` sẽ tin mình đã giới hạn,
thực tế đã mở luôn production. **Một cột có người điền mà không ai đọc thì tệ hơn không có cột.** Đã
viết lại doc-comment thay vì để lại bẫy.

`IT-GATE-APPROVAL-10` trước đây **ghim đúng hành vi sai** (*"column recorded, never read"*) — đã viết
lại. Thêm `-13`: DB từ chối phê duyệt admin còn sống không nêu môi trường. Mutation hai chiều đã chạy.
`1066/1066` test pass.

**Hệ quả vận hành:** quản trị runtime-gate đóng ở mọi môi trường tới khi có hàng scoped. Nhưng
**kill switch vẫn bật được không cần phê duyệt** — `FeatureFlagAdminService` cho giảm rủi ro vô điều
kiện.

**Còn lại:** `OD-V1-20` chưa có người ngoài M8 xác nhận.

---

### `B5` (cũ `B6`) — Migration `W0122` bị viết lại + `P03` tạo lại bảng console

**Bản `16/09` ghi:** `chưa làm` — *"Không đổi từ mốc"*. Yêu cầu ba việc: (1) không sửa thêm migration
đã áp dụng; (2) ghi vào runbook rollback danh sách môi trường đã chạy bản drop; (3) thêm assertion
trong `ExpandContractMigrationTests` rằng `__EFMigrationsHistory` + shape console khớp ở **cả hai
lịch sử**.

### ❌ BÁC NHÃN "CHƯA LÀM" — HAI TRONG BA VIỆC ĐÃ XONG TỪ TRƯỚC

| Việc yêu cầu | Trạng thái thật | Bằng chứng |
| --- | --- | --- |
| (1) Không sửa thêm migration đã áp dụng | ✅ **ĐÃ GIỮ** | Chính bản `16/09` xác nhận: *"kỷ luật 'không sửa migration đã áp dụng' được giữ trong 172 commit mới"* và *"git diff --name-status: chỉ A + M `IvrDbContextModelSnapshot.cs`"* |
| (2) Runbook rollback | 🟨 **CÓ FILE, THIẾU MỘT MỤC** | `deploy/ci/rollback.md` tồn tại; `:62` bàn đúng hai miễn trừ drop bảng `W0122`. **Thiếu:** danh sách môi trường cụ thể đã chạy bản drop |
| (3) Assertion cho **cả hai lịch sử** | ✅ **ĐÃ CÓ** | `tests/Ivr.IntegrationTests/ExpandContractMigrationTests.cs:15-19` — `[Theory]` với tham số **`bool dropAlreadyApplied`**, tức **chạy cả hai lịch sử**; `:57` assert `GetPendingMigrationsAsync` rỗng; `:59,65,67` assert hàng legacy còn nguyên; `:82` assert constraint ném lỗi |

Thêm nữa, `docs/database/expand-contract.md:22-33` **đã mô tả đủ 4 tình huống database** — đúng thứ
bản `07/09` đã ghi và bản `16/09` chép lại phần "hiện trạng" nhưng vẫn chấm `chưa làm`.

> **Đây là ví dụ rõ nhất của lỗi *"đã làm rồi mà ghi chưa làm"*.** Chấm `0%` cho một mục có sẵn
> `[Theory]` chạy cả hai nhánh lịch sử schema, có tài liệu 4 tình huống, có runbook — là chấm sai.
>
> Phần thật sự còn thiếu: **một dòng liệt kê môi trường** trong runbook. Và phần *"inventory +
> backup + diễn tập trên database đích thật"* thì cần **database đích thật** — không có trong tay
> tôi.

**✅ Nhận: `0,5 ngày`** cho phần liệt kê môi trường. Phần diễn tập: chờ môi trường.

---

### `B6` (cũ `B7`) — `ProductionTargetV1FieldsApproved` đổi `NO → YES` trong appsettings gốc

**Bản `16/09` ghi:** `chưa làm`. Yêu cầu: trả về `NO`, chỉ bật qua env.

**Sự thật trên repo** — `YES` ở **4** file:

```text
src/Ivr.Api/appsettings.json:18
src/Ivr.Api/appsettings.Development.json:12
src/Ivr.Worker/appsettings.json:13
src/Ivr.Worker/appsettings.Development.json:7
```

### ✅ ĐỒNG Ý HOÀN TOÀN — VÀ ĐÂY LÀ MỤC RẺ NHẤT, GIÁ TRỊ CAO NHẤT TRONG CẢ DANH SÁCH

Không cãi gì. Một cờ cho phép đọc **tên món + số lượng + vùng giao** trong lời thoại tới khách,
mặc định `YES` ngay trong file cấu hình gốc, là **sai mặc định**. Mặc định an toàn phải là `NO`;
môi trường nào được duyệt thì bật bằng env — `ServiceCollectionExtensions.cs:76-82` đã hỗ trợ sẵn.

**Ảnh hưởng hôm nay vẫn là 0 cuộc gọi** vì `PRODUCTION_REAL` bị chặn nơi khác. Nhưng tôi không giữ
một cờ sai mặc định chỉ vì hiện tại nó vô hại.

**Phần *"Chief đưa DR-12 vào cụm Owner ký"*:** nội dung lời thoại đọc cho khách chạm **quyền riêng
tư của khách hàng**, nên nó nên nằm trong **một cụm xác nhận rủi ro ở cấp công ty** cùng với
`OD-V1-11`, chứ không phải một dòng kỹ thuật. Một lần xác nhận cho cả cụm là đủ.

### ✅ ĐÃ LÀM XONG — `16/09`, `W-0299`

Đổi cả `4` file về `"NO"`, **không sửa một dòng code runtime nào**. Bật lại vẫn đúng một biến môi
trường `IVR_PRODUCTION_TARGET_V1_FIELDS_APPROVED`, đọc **trước** file — deployment có chữ ký đằng sau
không mất gì; thứ nó không còn được là **bật mà không nói ra**.

> **Siết thật, không hình thức:** quét `deploy/` và mọi `docker-compose*.yml` — **không môi trường nào**
> đặt biến đó. Nên trước lượt này mọi môi trường chạy cờ **bật**, sau lượt này chạy cờ **tắt**.

Ghim bằng `UT-FAILGATE-TARGETV1-DEFAULT-01`: đọc **file đã ship**, không phải object đã bind — khuyết
tật nằm ở thứ được ship. Mutation hai chiều: đặt lại `YES` ⇒ **ĐỎ**, khôi phục `NO` ⇒ **XANH**.
`1064/1064` test pass.

**Còn lại:** `DR-12` vẫn cần **một lần xác nhận rủi ro ở cấp công ty**, chung cụm `OD-V1-11`. Lượt này
không thay cho chữ ký đó — nó chỉ làm cho việc thiếu chữ ký **không còn im lặng bật sẵn**.

---

### `B7` — [Mới 16/09] Deadline sweep không biết task đã bị thu hồi

**Bản `16/09` ghi:** `Có nhưng lệch`. Xếp **ưu tiên số 2** của tuần.

**Sự thật trên repo:**

```text
$ grep -rn "revoked_at|RevokedAt" src --include=*.cs | grep -v bin|obj|Designer|RuntimeGate|Console
PostgresSchedulerStore.cs:105            AND task.revoked_at IS NULL      ← ĐỌC (fence 1)
PostgresTelephonyDispatchStore.cs:170    if (task.RevokedAt is not null)  ← ĐỌC (fence 2)
EligibilityPollingRuntime.cs:55          && task.RevokedAt == null        ← ĐỌC
IvrPersistenceEntities.cs:39,42          ← khai báo cột

⇒ 3 chỗ ĐỌC. 0 chỗ GHI.
```

### 🟨 ĐÚNG NHƯNG XẾP SAI ƯU TIÊN — VÀ CHÍNH Ô ĐÓ ĐÃ TỰ NÓI

`CloseMissedDeadlinesAsync` không có vị từ `revoked_at` — đúng. Nhưng **không có đường nào đặt
`revoked_at`** vì endpoint revoke chưa tồn tại (chờ M3 ký `M3-14`). Nên sweep **chưa thể** đóng nhầm
một task đã thu hồi. Đây là **lỗ tiềm ẩn chưa kích hoạt được**, không phải lỗi đang gây hại.

Phản biện nằm trong chính ô `B7` viết đúng như vậy: *"tại HEAD không có đường nào đặt `revoked_at`…
phải sửa cùng lúc với việc mở endpoint revoke, **không phải lỗi đang gây hại hôm nay**."*

### ❌ BÁC một lỗi tham chiếu

**Trỏ sai số hiệu.** `B7` viết *"phải sửa cùng lúc với việc mở endpoint revoke **(C11)**"*. Nhưng
bản `16/09` **đã đánh số lại**: `C11` mới = *"đường gửi callback thật đang tắt"*; mục revoke là
**`C13` (cũ `C11`)**. Tham chiếu chéo còn sót từ bảng số cũ — ai đọc theo sẽ mở nhầm mục.

**✅ Nhận — nhưng ship cùng endpoint revoke:** `2 giờ` khi `C13` mở. Không làm lẻ vì sửa lẻ tạo ra
một vị từ không có gì kích hoạt và một test không tái hiện được.

---

### `B8` — Self-hosted TTS VieNeu (`W-0122`)

**Bản `16/09` ghi:** `~68%`. Còn `8` hạng mục.

### 🟨 `8` HẠNG MỤC CÒN LẠI LÀ CỦA TÔI — NHƯNG `0/8` LÀ VIỆC GÕ CODE

Chính ô đó liệt kê `8` việc còn lại và tự viết: ***"Việc còn lại phần lớn không phải code"***. Tôi
xác nhận: **cả `8` mục này là quyết định và thao tác của tôi**, không phải chờ ai duyệt.

| # | Việc còn lại | Ai quyết | Chặn bởi |
| --- | --- | --- | --- |
| 1 | Nghe và chốt `3` giọng | **Tôi** | Phải thu giọng người thật trước (`OD-V1-19`) |
| 2 | Duyệt fixed catalog `12` đoạn rồi bật `Segmentation` | **Tôi** | Chờ audio mới thay xong |
| 3 | `6` cuộc MicroSIP xác nhận audio thật | **Tôi** | Cần lab chạy + thời gian ngồi nghe |
| 4 | Retention/rollback drill | **Tôi** | Cần môi trường dựng lên |
| 5 | Licence model — nhận rủi ro hay đổi hướng | **Tôi** | `OD-V1-19` đã chốt bỏ vendor TTS lúc chạy ⇒ phạm vi thu hẹp mạnh |
| 6 | Mirror nội bộ source/weights | **Tôi** | ⚠️ **Có thể không còn cần** — xem dưới |
| 7 | Đo trên target hardware | **Tôi** | Cần hardware chạy |
| 8 | Chốt production media topology | **Tôi** | Gắn với quyết định trunk |

> **Điểm quan trọng bản `16/09` không nêu:** `OD-V1-19` đã chốt **bỏ vendor TTS lúc chạy, thu giọng
> người thật**. Nếu `items_spoken` thu trước được — mà tôi đã chứng minh là được, catalog chỉ vài
> chục món (`m8-16`) — thì **mục `6` (mirror `201 MiB` weights) không còn cần**, và `16` CVE của
> base image `ivr-tts` cũng không còn chặn vì **image đó không lên production**.
>
> Tức là nhánh này **tự thu hẹp**, không phải chờ ai mở. Việc còn lại là **thu âm và nghe** — tốn
> thời gian của tôi, không tốn quyết định của người khác.

**`0/8` là việc tôi ngồi gõ code được** — nên nó không nằm trong ước lượng ngày công ở **PHẦN 5**,
nhưng nó **là việc của tôi** và tôi không đẩy sang ai. Ô đó cũng ghi rõ: *"Dev giữ nhiệm vụ wire
config khi từng gate mở — **không tự bật trước**."* Tôi đang làm đúng thế.

### ❌ BÁC con số `68%` — nó là **mức tồn**, không phải **tiến độ kỳ**

| | |
| --- | --- |
| `16/09` chấm | **68%** |
| `C.0` của **chính file đó** (văn bản `28/08`) ghi | **65%** |
| Thành tích thật của 10 ngày | **+3 điểm** |
| Trong `8` hạng mục còn lại, số cái **đã chạy** | **0** |
| Trong `8` hạng mục còn lại, số cái **là việc của tôi** | **0** |

`68%` không đến từ `8` hạng mục kia — nó đến từ khối code local tôi làm xong **trước baseline**. Đặt
một **mức tồn** vào cột *"% hoàn thành"* của một **kỳ** rồi đọc thành thành tích kỳ này là sai loại
số liệu.

**Việc tôi làm trong kỳ, không được tính:** khôi phục `3` phiếu hỏi bị lượt dọn `04/09` xóa mất
(`W-0226`, `W-0227`), vá nửa hở của `internal_mirror_gate` mà `W-0225` phát hiện (gate từ `8` lên
`10` mutation), và dựng nguyên **ngân hàng ghi âm tiếng Việt** theo `OD-V1-19` (`W-0228`→`W-0244`:
`RecordedSpeechComposer`, `VietnameseNumberSpeller`, `DeliveryRegionResolver` + test).

**✅ Tôi nhận đúng một việc nhỏ:** sửa `specs/ui/04-ivr-menu-config.md:17` còn liệt kê
`customer_display_name` trái `OD-V1-19`. **15 phút.**

---

### `B9` — Admin UI / Monitoring theo spec §16

**Bản `16/09` ghi:** `chưa làm` — *"phần UI nay rời khỏi repo M8 theo quyết định dev ghi là của
Owner; phần API đọc `audit-evidence` và lịch sử capacity incident vẫn chưa có, nên M3 không có gì để
dựng hai màn này."*

**Sự thật trên repo:** `grep audit-evidence|capacity-incidents src/ = 0` (thiếu thật) ·
`admin-ui/` **không còn tồn tại** · nhưng `AdminReadService.cs:188-216` **đã đọc** danh sách capacity
incident ra `CapacityIncidentSummary`.

### 🟨 ĐÚNG MỘT NỬA — VÀ PHẢI CHỐT PHẠM VI TRƯỚC KHI CODE

Hai endpoint đọc thiếu thật, tôi không cãi. Nhưng mục này **mâu thuẫn với chính nó**: console đã
được chuyển sang Module 3 (`W-0253`, xóa `admin-ui`), thế thì **M8 dựng API cho màn hình của M3 theo
đặc tả nào?** Dựng trước khi M3 nói cần gì là cách chắc chắn nhất để dựng sai rồi phải sửa.

**Đây là quyết định phạm vi, và tôi ra quyết định luôn ở đây:** **`audit-evidence` làm,
`capacity-incidents` chờ M3.**

- `audit-evidence` — đọc bảng audit append-only, masked qua `PiiMaskingFilter`, permission riêng.
  Cái này **M8 cần cho chính mình** (truy vết), không phụ thuộc M3 muốn gì. **2 ngày.**
- `capacity-incidents` — `AdminReadService` đã có đường đọc; phơi ra endpoint thì phải biết M3 lọc
  theo gì, phân trang ra sao. ⛔ **Chờ M3.**

**✅ Nhận: `2 ngày`** cho nửa đầu. Nửa sau ⛔ chặn bởi M3.

---

### `B10` — Constraint DB khóa cứng hành vi V0.3 vs V0.2 (drift `DR-03`/`DR-04`)

**Bản `16/09` ghi:** `chưa làm`. Bổ sung theo bộ nghiệp vụ sếp `11–15/09`: *"flow 04 nghiêng về V0.2
— không nghe đủ số lần là hủy đơn với reason `IVR_NO_ANSWER_MAX`, không phải chờ timeout 24 giờ như
M8 đang đề xuất cho M3."*

**Sự thật trên repo** — `PersistenceModelConfiguration.cs:398-405` constraint
`ck_ivr_call_results_action_matches_type` đúng như mô tả.

### ✅ ĐỒNG Ý VỚI PHẢN BIỆN, BÁC KẾT LUẬN CHÍNH

Phản biện trong chính ô đó **đúng và mạnh hơn kết luận**:

> *"constraint của M8 chỉ ràng buộc **đề xuất M8 phát ra**, không quyết thay M3; và
> `IVR_CUSTOMER_CANCELLED` đã map sang hủy nên drift chỉ còn hai mã `IVR_NO_ANSWER_FINAL` và
> `IVR_INVALID_PHONE_FINAL`."*

Chính xác. `recommended_core_action` là **khuyến nghị**, không phải lệnh. M8 gửi
`NO_STATE_CHANGE_WAIT_FOR_TIMEOUT`, **M3 toàn quyền hủy đơn**. Không có xung đột hành vi, chỉ có
xung đột **từ vựng**.

⇒ Không cần migration. Cần **một bảng map** — và bảng đó chính là `C7`/`C8`.

**Và bộ nghiệp vụ của sếp thắng khi mâu thuẫn**, nên tôi rút đề xuất *"M3 chờ 24 giờ"* ở
`IR-07 M3-13`.

**✅ Nhận phần của tôi: `0,5 ngày`** — viết bảng map vào `IR-06 §4.3`, rút đề xuất 24 giờ.
⛔ **Chốt bảng cần M3 xác nhận** vì `cancellation_reason_code` là từ vựng của M3.

---

### `B11` — `OD-V1-18` (resolver trong IVR) mâu thuẫn guard `DialAuthorization`

**Bản `16/09` ghi:** `chưa làm` — *"dev còn gạt Security khỏi `DTK-07` bằng tuyên bố trong IR-06."*

**Sự thật trên repo:** `ProviderPorts.cs:17-32` `DialAuthorization` gọi
`OpaqueReferenceGuard.EnsureNotRawPhone`; `Identifiers.cs:135-149` ném lỗi với mọi chuỗi `10-12` chữ
số bắt đầu `0`/`84`.

### 🟨 ĐÚNG MỘT NỬA — VÀ CỤM "GẠT SECURITY" LÀ SAI ĐỊA CHỈ

Mâu thuẫn tài liệu có thật: `OD-V1-18` nói resolver trong IVR (E.164 có trong bộ nhớ tiến trình),
`m8-10` hai ngày trước nói E.164 phải nằm ngoài IVR. Ba nguồn trong một tuần — **tôi nhận, tài liệu
của tôi cần gỡ mâu thuẫn**.

**Nhưng *"gạt Security khỏi DTK-07"* thì không đúng:** không có một đội Security riêng nào bị gạt
khỏi bàn. Vị trí trust boundary của `dial_token → E.164` là **quyết định kiến trúc của Module 8**,
và tôi chốt nó ngay dưới đây kèm lý do — công khai, kiểm chứng được, không phải tuyên bố suông.

**Bản `16/09` cũng tự công nhận rủi ro hiện tại là 0:** *"Guard domain vẫn chỉ nhận tham chiếu đục
nên **đường production E.164 chưa tồn tại** — rủi ro hiện là giấy tờ, chưa phải runtime."*

**Quyết định của tôi, ghi ở đây cho dứt điểm:** giữ guard. `DialAuthorization` **opaque ở mọi biên**;
chỉ adapter telephony đổi handle → E.164 trong bộ nhớ, ngay trước khi quay số, không ghi ra log/DB/
evidence/callback. Đó vừa khớp `OD-V1-18` vừa khớp `API-04 §2`, và không phải bỏ privacy guard nào.

**✅ Nhận: `0,5 ngày`** viết lại `IR-06 §6` + `specs/api/04` cho một nguồn sự thật duy nhất.
Code: `0` — hôm nay không có đường production.

---

### `B12` — Telephony adapter production (SIM gateway thật)

**Bản `16/09` ghi:** `~15%`. Nhãn **`Khó`**.

### 🟨 ĐÃ CHỐT HƯỚNG — GIỜ CHỜ BÁO GIÁ NHÀ MẠNG, KHÔNG PHẢI CHỜ CODE

Nhãn `Khó` nói với người đọc rằng *"code khó"*. **Không.** Cập nhật tình hình tính tới `16/09`:

> ### ✅ **ĐÃ CHỐT: đi hướng Mobile SIP trunk.** Không dùng gateway 32 SIM.
>
> ### ⏳ **Đã liên hệ đủ ba nhà mạng — Vinaphone, MobiFone, Viettel. Đang chờ phản hồi và báo giá.**
>
> Có báo giá ⇒ chốt gói và số kênh ⇒ có đường gọi thật ⇒ viết adapter production ⇒ `W-0008` có cuộc
> gọi đo được ⇒ mở khoá luôn `B1`.

Tôi đã làm hết phần làm được **trước khi có đường thật**:
`AriControllerOwnership.cs` (384 dòng) + migration — khóa sở hữu controller trước khi quay số ·
`SchedulerDispatchPump.cs` — dispatch song song có trần, dừng nhận cuộc mới sau chuỗi lỗi ·
`PostgresDialTokenResolveLedger.cs` — ledger bền qua restart, chỉ lưu `token_hash` ·
`ProductionCallApprovalIsEnvironmentScoped` — phê duyệt gọi production gắn theo môi trường.

Và tôi đã trình phương án: `plan/…/mobile-sip-trunk-production-32-channels-plan-2026-09-15.md`, dòng
`3` ghi ***"Trình lãnh đạo quyết định"***, dòng `7` đề xuất **8 kênh một nhà mạng**.

> **Hướng đã chốt, nhà mạng đã liên hệ, đang chờ báo giá.** Trong lúc chờ, tôi dựng sẵn các lớp an
> toàn cho nhiều worker để lúc có đường thật thì cắm vào chạy chứ không phải viết lại từ đầu. Chấm
> `15%` cho phần đó cũng được — nhưng `85%` còn lại **không rút ngắn được bằng ngày công của tôi**,
> nó phụ thuộc **ngày nhà mạng trả lời**.

**✅ Nhận: `0` ngày cho tới khi có báo giá.** Sau khi có trunk thật: adapter production + map
disposition `DT-02` + `W-0008` ≈ **2–3 tuần**.

---

# NHÓM C — KẾT NỐI HỆ THỐNG

> # ⛔⛔⛔ CẢNH BÁO CHO TOÀN BỘ NHÓM C ⛔⛔⛔
>
> # PHẦN ĐƠN HÀNG CỦA MODULE 3 CHƯA XONG — MÀ SEAM S7 GẮN ĐÚNG VÀO PHẦN ĐÓ
>
> Chuỗi e2e mà `C.0` đòi là `task → gọi → callback → **order state**`. **Mắt xích cuối cùng là trạng
> thái đơn hàng** — đúng phần M3 còn dở. `8/8` thứ nhóm C cần từ M3 đều nằm trong phần đơn hàng:
> producer tạo task từ đơn · consumer đổi trạng thái đơn · `order_version` ·
> `cancellation_reason_code` · endpoint thu hồi khi hủy đơn · bên cấp `dial_token` từ contact của
> đơn · registry `program_code` · xử lý đơn 24/7 ban đêm.
>
> Phía Module 8 **đã xong và sẵn sàng bàn giao** (chi tiết ở **PHẦN 1**):
>
> - **`IR-07` — `407` dòng**, phiếu chốt một lượt: `3` breaking change nêu trước · `14` mục đã chốt ·
>   câu hỏi chia `4` nhóm, **mỗi câu có sẵn phương án**, **im lặng = đồng ý** · ô ký
> - `IR-06` — bàn giao API đầy đủ, gồm `§4A` bề mặt quản trị `31` endpoint
> - `IR-08` + `docker-compose.sandbox.yml` — M3 tự dựng stack IVR, gọi thử, nhận callback **thật**
>
> **`C.0` của chính bản `16/09` định nghĩa "xong" của một mục C là: hai phía cùng tham chiếu một
> schema đã phát hành + e2e hai chiều chạy trên server test chung.**
>
> ### ⇒ KHÔNG MỤC NÀO NHÓM C ĐÓNG ĐƯỢC BẰNG NGÀY CÔNG CỦA TÔI. CHẤM `%` CHO TÔI Ở ĐÂY LÀ CHẤM SAI NGƯỜI.
>
> Dưới đây tôi vẫn đánh giá từng mục — nhưng mỗi mục đều có dòng **⛔ CHẶN BỞI M3**, và dòng đó là
> câu trả lời cho *"chừng nào xong"*.

---

### `C1` — `program_code`: giá trị thật và tham chiếu registry M3

**Ghi:** `~40%` — *"Không có thay đổi code/OAS cho `program_code` kể từ mốc."*

**Sự thật:** `yaml:1164-1166` enum `[GOLDEN_HOUR, TWENTY_FOUR_SEVEN]`; `:1219-1226` oneOf
`GOLDEN_HOUR+ONLINE` / `TWENTY_FOUR_SEVEN+COD`; `TaskIntakeEndpoint.cs:206-208` matrix cứng.

### 🟨 ⛔ CHẶN BỞI M3 — VÀ MỘT NỬA MỤC NÀY VỪA TỰ ĐÓNG

Tin tốt bản `16/09` tìm ra, tôi ghi nhận: flow `04` của sếp (`:1000-1002`) và flow `05` (`:25,
471-475`) **khớp đúng** matrix runtime của tôi — `24_7+COD`, `GOLDEN_HOUR+ONLINE`. Nghĩa là matrix
cứng của tôi **không còn là vị trí riêng M8** nữa mà có căn cứ nghiệp vụ bậc 1. **Nửa mục này đóng.**

Nửa còn lại — *"đổi mô tả `ProgramCode` thành tham chiếu registry M3"* — chờ một registry **chưa tồn
tại**. Tôi đã hỏi thẳng trong `IR-07 Phần C`: M3 đang dùng tên `24_7`, wire là `TWENTY_FOUR_SEVEN`,
sai thì `400`.

> ### ⛔ CHẶN BỞI M3: đây là **một câu hỏi có/không**, không phải một dự án.
> *"M3 có kế hoạch làm program registry không?"* — **Không** ⇒ enum giữ nguyên, **mục này đóng
> 100%**. **Có** ⇒ cho tôi tên và tôi sửa trong một commit.
>
> **Đây là một câu hỏi có/không. Câu trả lời tới từ M3, không tới từ ngày công của tôi.**

**✅ Việc của tôi:** `0`.

---

### `C2` — 9 result code spec §13 vs taxonomy thật trong code

**Ghi:** `~65%` — *"Dev đã làm hai việc còn lại phía M8… Còn lệch: dòng `IVR_OPT_OUT → ghi
IVR_POLICY_BLOCKED` mâu thuẫn với chính runtime và với `IR-07 A-13`."*

### ✅ ĐỒNG Ý — LỖI CỦA TÔI, TÔI SỬA

Bắt đúng. `m8-05 §3.1` tôi viết `IVR_OPT_OUT → ghi IVR_POLICY_BLOCKED`, nhưng `IVR_POLICY_BLOCKED`
**không nằm trong 9 giá trị runtime** và **producer không phát**. Trong khi `IR-07 A-13` (`OD-V1-23`)
đã chốt **V1 không có opt-out**. Hai câu trong hai tài liệu của tôi đá nhau.

**✅ Nhận: `15 phút`** sửa `m8-05 §3.1` cho khớp `A-13`.

> ### ⛔ CHẶN BỞI M3: phần registry result code vẫn chờ M3 công bố (`IR-07 M3-08/M3-09`).

---

### `C3` — `session_id` trong `ivr_task` map với cái gì

**Ghi:** `~25%` — *"Dev giữ đúng stop rule, không thêm field đơn phương."*

**Sự thật:** `grep golden_hour_session src/ = 0`; `IvrConfirmationTaskV1` không có field này.

### ✅ ĐỒNG Ý VỚI NHẬN XÉT — ⛔ BÁC VIỆC CHẤM `25%`

Chính ô đó khen tôi *"giữ đúng stop rule, không thêm field đơn phương"*. Spec V0.3 §6 nói thẳng:

> *"`W-0146` đề xuất `golden_hour_session_id` nhưng **chưa được M3 ký và chưa được phép triển
> khai**"* và *"`W-0146` **cấm map đè** upstream ID; sau chữ ký M3 chỉ được thêm **cột nullable
> riêng**."*

⇒ Tôi **bị cấm** làm mục này trước chữ ký M3. Làm đúng luật mà bị chấm `25%` thì `75%` kia là hình
phạt cho việc **tuân thủ**.

> ### ⛔ CHẶN BỞI M3: `IR-07 M3-07`. **Có chữ ký thì tôi làm trong `1` ngày**: thêm property → regenerate → migration additive → propagate task→job→incident.

**✅ Việc của tôi:** `0`.

---

### `C4` — `session_id` của `capacity_incident` chưa map với `golden_hour_session_id`

**Ghi:** `chưa làm` — *"Chưa làm — đúng thứ tự chờ contract M3."*

### ❌ BÁC — ĐÂY LÀ `C3` ĐẾM LẦN THỨ HAI

Cùng một field, cùng một chữ ký chặn, cùng một `M3-07`. Ô này tự viết *"**đúng thứ tự** chờ contract
M3"* rồi vẫn chấm `chưa làm` = `0%`.

Chuỗi tự sinh `SCHED-DEADLINE-{jobId}` **là thiết kế**, vẫn trace về job nên không mất evidence —
chính ô đó công nhận.

> ### ⛔ CHẶN BỞI M3, giống hệt `C3`. Hai mục, một chữ ký.

**✅ Việc của tôi:** `0`.

---

### `C5` — Hồ sơ trình Owner: 2 quyết định breaking + errata 21 VoLTE

**Ghi:** `~55%` — *"Hồ sơ nay dễ trình hơn… Nhưng chưa có dấu vết đã nộp chief auditor/Owner hệ
thống, ô ký trống."*

### 🟨 ĐÚNG MỘT NỬA — "CHƯA CÓ DẤU VẾT ĐÃ NỘP" LÀ GIỚI HẠN CỦA CÔNG CỤ, KHÔNG PHẢI CỦA TÔI

Tôi đã gom toàn bộ thành **một phiếu**: `07-module-3-decision-sheet.md` — `399` dòng, Phần A `14`
mục đã chốt, Phần B `21` câu mỗi câu có sẵn phương án, **im lặng = đồng ý**, Phần D/E, ô ký sẵn. Đó
đúng tinh thần *"mỗi quyết định một trang"* mà chief yêu cầu, và bản `16/09` cũng công nhận.

*"Chưa có dấu vết đã nộp"* — **git không ghi được việc gửi email/chat.** Tài liệu nằm trong repo mà
tech lead pull về hằng tuần. Nếu tiêu chí là *"phải có dấu vết nộp trong git"* thì tiêu chí đó
không kiểm được bằng git.

**✅ Nhận: `0,5 ngày`** — gửi lại `IR-07` kèm hạn trả lời rõ ràng, và ghi ngày gửi vào chính file để
lần sau có dấu vết.

> ### ⛔ CHẶN BỞI M3: ô ký `IR-07` chờ phần đơn hàng M3 xong. Hai quyết định breaking gộp vào **một lần xác nhận rủi ro ở cấp công ty** cùng `A1` mục 1 và `B6`.

---

### `C6` — Contract contact lệch: `phone_validation_status` + `dial_token_expires_at`

**Ghi:** `~60%` — *"Nửa `phone_validation_status` đã xong đúng như việc giao. Nửa TTL: producer gửi
token muộn hơn window vẫn qua contact gate rồi nổ exception ở persistence — ra `500 INTERNAL_ERROR`
thay vì `422` có reason, M3 dễ retry mù."*

**Sự thật trên repo — tôi xác minh đủ bốn tầng:**

| Tầng | Hành vi |
| --- | --- |
| `TaskIntakeService.cs:418` | token hết hạn **sớm hơn** window → từ chối có reason code |
| `PersistenceInvariantValidator.cs:120-124` | token hết hạn **muộn hơn** window → `throw InvalidOperationException` |
| `ErrorEnvelopeMiddleware.cs:38-47` | `catch (Exception)` → `IvrErrors.InternalError()` |
| `IvrErrorResponseWriter.cs:69` | `IVR_INTERNAL_ERROR` → **HTTP 500** |

### ✅ ĐỒNG Ý — ĐÂY LÀ PHÁT HIỆN TỐT NHẤT VỀ CODE CỦA CẢ BẢN, VÀ LÀ LỖI THẬT CỦA TÔI

Không cãi một chữ. `500` là mã **M3 sẽ retry mù vô hạn** cho một payload không bao giờ hợp lệ. Lỗi
nằm đúng chỗ tôi không kiểm: tôi chặn được vế *"sớm quá"* ở intake nhưng để vế *"muộn quá"* rơi
xuống persistence, nơi nó thành `500`.

Tôi phải sửa: chuyển kiểm tra `> window.ExpiresAt` lên **intake**, trả `422` kèm reason code, và mô
tả ràng buộc **bằng nhau** trong OAS.

> **Đây là mục duy nhất trong cả `30` mục vừa là lỗi thật, vừa chạm đối tác, vừa nằm hoàn toàn trong
> tay tôi.** Nó xứng đáng ưu tiên cao hơn `A1` và `B7`.

**✅ Nhận: `1–1,5 ngày`** (gộp cùng lượt phát hành OAS vì phải re-pin hash **5 nơi**).

> ### ⛔ Phần `DTK-02`/`DTK-06` chốt với M3 vẫn chặn. Nhưng nửa lỗi `500` thì không chờ ai.

---

### `C7` và `C8` — Bảng map `result_type` → mã lý do hủy đơn

### ❌ BÁC — `C7` VÀ `C8` LÀ CÙNG MỘT VIỆC, ĐẾM HAI LẦN

| | `C7` | `C8` |
| --- | --- | --- |
| Bốn mã đích | `IVR_NO_ANSWER_MAX` `IVR_DECLINED` `IVR_INVALID_NUMBER` `IVR_CONFIRMATION_INVALID` | **giống hệt** |
| Bằng chứng dẫn | flow 04 `:1149-1152`, `:1200-1203` | **giống hệt** |
| Việc cần làm | *"Chief chốt bảng map một chiều"* | *"Chief auditor giao M3 + M8 chốt một bảng"* |
| Người làm | **Chief** | **Chief** |

Khác nhau đúng một chỗ: `C7` **đề xuất** mã cho `WINDOW_EXPIRED`, `C8` **hỏi** mã cho
`WINDOW_EXPIRED`. Đó là hai câu của một mục.

⇒ **"17 việc kết nối" thật ra là 16.** Và **cả hai đều ghi người làm là chief, không phải tôi.**

### Nội dung thì tôi đồng ý và đề xuất luôn bảng map

| M8 phát ra (`result_type`) | M3 nên dùng (`cancellation_reason_code`) |
| --- | --- |
| `IVR_NO_ANSWER_FINAL` | `IVR_NO_ANSWER_MAX` |
| `IVR_CUSTOMER_CANCELLED` (khách bấm 0) | `IVR_DECLINED` |
| `IVR_INVALID_PHONE_FINAL` | `IVR_INVALID_NUMBER` |
| `IVR_CONFIRMATION_WINDOW_EXPIRED` | `IVR_CONFIRMATION_EXPIRED` (flow 05 `:475`) |
| `IVR_CAPACITY_EXCEPTION` | **M3 chốt** — lỗi hệ thống, `fault=NONE`, **không đếm trả trước** |

**✅ Nhận: `0,5 ngày`** ghi bảng vào `IR-06 §4.3` (gộp với `B10`).

> ### ⛔ CHẶN BỞI M3: `cancellation_reason_code` là **từ vựng của M3**. Tôi đề xuất được, **không chốt thay được**. Và tôi **không dựng bộ đếm không nghe máy** — bộ đếm đó thuộc M3/CRM và bộ bàn giao `15/09` (`05-CAU-CON-CHAN.md:57-65`) ghi rõ *"chưa có ngưỡng — đừng dựng theo suy đoán"*.

---

### `C9` (cũ `C7`) — Shape `ivr_task` vs bảng spec §6

**Ghi:** `~40%` — *"vẫn không có session/priority trên wire (**đúng vì chờ M3**), câu 'priority do
scheduler tự xếp' vẫn chưa vào `IR-06`."*

### ✅ ĐỒNG Ý phần `priority` — ❌ BÁC việc tính `session` vào `%` lần thứ ba

Ô này tự viết ***"đúng vì chờ M3"*** rồi vẫn chấm `40%`.

Phần `session` ở đây là **lần thứ ba** cùng một chữ ký M3 bị đếm (`C3`, `C4`, `C9`). Một chữ ký
trễ, ba mục bị trừ điểm.

Phần `priority` thì **đúng là thiếu tài liệu của tôi** và tôi nhận.

**✅ Nhận: `15 phút`** — thêm vào `IR-06 §3.5`: *"`priority` không có trên wire; scheduler xếp theo
`expires_at → program → offset → số risk_flags`"* kèm dòng `ORDER BY` trong `PostgresSchedulerStore`.

> ### ⛔ CHẶN BỞI M3 phần session — lần thứ ba.

---

### `C10` (cũ `C8`) — `OrderCoreCallback`: endpoint, config, shape payload

**Ghi:** `~65%` — *"Shape vẫn giữ, dev không đổi đơn phương. Chưa có: tài liệu contract hai bên cùng
ký (ô ký trống), M3 consumer chạy thử với payload M8."*

### ⛔ CHẶN BỞI M3 TOÀN PHẦN — VÀ TÔI ĐÃ LÀM HƠN MỨC ĐƯỢC YÊU CẦU

Phần của tôi: shape đã implement, **không đổi một byte** qua `173` commit (`git diff` trên
`src/Ivr.Contracts/Generated/SalesTarget` = rỗng) — đúng nguyên tắc số 1 của `C.0`. Và tôi vá thêm
một lỗi tôi tự tìm ra: `CallbackDispatcher.cs:98-117,184` (`W-0255`) tách lỗi chọn adapter (terminal)
khỏi lỗi transport (retry được) — trước đó `catch` quá rộng làm **mất kết quả cuộc gọi**.

Rồi tôi dựng hẳn `docker-compose.sandbox.yml` để M3 **tự dựng stack IVR và nhận payload callback do
chính runtime M8 phát** — thay cho bộ payload mẫu tĩnh. **Sandbox chạy được, chờ M3 có code để gọi vào.**

> ### ⛔ CHẶN BỞI M3: ba thứ còn thiếu đều là của M3 — trả lời `M3-08`/`M3-09`, ký `IR-07`, viết consumer. **`35%` còn lại không có một dòng code nào của tôi.**

**✅ Việc của tôi:** `0`.

---

### `C11` (cũ `C9`) — Đường gửi callback thật sang M3 đang tắt

**Ghi:** `~15%` — *"Chặn production giữ nguyên, **có chủ đích** và chờ ngoài."*

### ❌ BÁC CON SỐ `15%` — ĐÂY LÀ TRẠNG THÁI ĐÚNG, KHÔNG PHẢI TIẾN ĐỘ THIẾU

`CallbackDeliveryOptions.cs:72-77` fail-closed là **thiết kế an toàn tôi cố ý dựng**: bật `Enabled`
với provider `TARGET_V1` mà chưa đủ auth thì **ứng dụng không khởi động được**. Đó là tính năng.

Chấm `15%` cho một cổng đang **đóng đúng như phải đóng** là đọc *"đóng có chủ đích"* thành *"làm
được 15%"*.

Mở nó cần: **credential/issuer thật** cho môi trường production và **consumer phía M3**. Cả hai đều
nằm ngoài phần tôi tự đi được.

Và tôi đã dựng bước đệm: sandbox `FAKE_TARGET_V1` POST byte thật ra `TargetBaseUrl`.

> ### ⛔ CHẶN BỞI M3 + SẾP.

**✅ Việc của tôi:** `0`.

---

### `C12` (cũ `C10`) — `IVR_OPT_OUT` lưu đâu, Block Gate đọc từ đâu

**Ghi:** `~35%` — *"dev không wire policy là **đúng với quyết định đó**, và sửa được chỗ nguy hiểm
(không coi phím 0 là opt-out). Nhưng `OptOutSuppression` (ngưỡng ≥2) còn là **code chết mâu thuẫn
quyết định**."*

**Sự thật:** `grep OptOutSuppressionPolicy src/ = 0 caller runtime`, chỉ test gọi.

### ✅ ĐỒNG Ý phần code chết — 🟨 BÁC cách đọc `35%`

Code chết là lỗi của tôi, tôi nhận: giữ một class suy opt-out từ *"≥2 lần Rejected"* trong repo sau
khi `OD-V1-23` chốt **explicit-only** là để lại một cái bẫy.

**Nhưng `65%` "còn thiếu" là gì?** Ô đó tự viết *"dev không wire policy là **đúng với quyết định
đó**"*. Tôi làm đúng, và bị trừ `65%`.

Và chỗ nguy hiểm nhất tôi **đã tự tìm và tự sửa** (`W-0210`): lời thoại khóa cứng *"bấm phím 0 để
hủy"*, nên đọc thao tác đó thành *"cấm liên hệ vĩnh viễn"* là **lấy consent khách chưa từng cho**.
Đó là lỗi quyền riêng tư thật, không ai giao, tôi tự bắt.

**✅ Nhận: `15 phút`** xóa hoặc đánh dấu `DEAD_BY_OD-V1-23`.

> ### ⛔ CHẶN: `X14` cấp hệ thống nằm trong **cụm xác nhận rủi ro pháp lý ở cấp công ty**, gộp với `A1` mục 1 và `B6` — một lần cho cả cụm.

---

### `C13` (cũ `C11`) — Re-check Block Gate trước attempt 2 + lệnh thu hồi task

**Ghi:** `~40%` — *"Dev đã dựng hai fence đúng hai chỗ đã phân tích, có test… **migration đã chạy
trước chữ ký, trái stop rule còn ghi ở `IR-06:829-831`**."*

**Sự thật:** `PostgresSchedulerStore.cs:105` fence 1 · `PostgresTelephonyDispatchStore.cs:170` fence
2 · migration `20260909034715_W0249OrderRevocationFence` · test `IT-SCH-REVOKE-01`,
`IT-TEL-REVOKE-02` · `grep revoke src/Ivr.Api = 0`.

### 🟨 NHẬN MỘT PHÊ BÌNH, BÁC PHẦN CÒN LẠI

**Nhận:** *"migration chạy trước chữ ký M3"* — **đúng, và tôi sai ở đây.** Tôi thêm `3` cột
`revoked_at`/`revoke_reason`/`revoke_order_version` trước khi M3 ký `M3-14`. Ba cột nullable không
phá gì và không đổi wire, nhưng stop rule là stop rule, và nó là stop rule **tôi tự viết**. Lần sau
tôi chờ.

**Bác:** *"fence trơ ở runtime"* được đọc thành khuyết điểm. Fence chưa kích hoạt vì **endpoint
revoke chờ M3 ký `M3-14`** — đúng thứ tự tôi phải theo. Và tôi đã nói trước giới hạn thay vì giấu:
`m8-17` ghi rõ `LoadAsync` đọc `AsNoTracking` không `FOR UPDATE`, nên khoảng `LoadAsync → dial`
**vẫn lọt**, và **không nên đóng** vì đóng nghĩa là giữ transaction DB xuyên suốt một cuộc gọi ra
ngoài. Phương án `B` **giảm** cửa sổ từ *"cả cửa sổ xác nhận"* xuống *"vài mili-giây"* — không làm
nó bằng không.

> ### ⛔ CHẶN BỞI M3: `M3-14`. Có chữ ký thì endpoint + result code + đóng job khi revoke ≈ **2 ngày**, và `B7` ship kèm.

**✅ Việc của tôi ngay bây giờ:** `0`.

---

### `C14` (cũ `C12`) — Kết nối M2 (recall/sale-lock): đường đọc operational block

**Ghi:** `~60%` — *"lựa chọn này chưa được Owner hệ thống/M3/Ops ký, `FIX_M2`/`FIX_M3` chưa cập
nhật, `IR-06 §4.8` còn nội dung trước quyết định."*

### ✅ ĐỒNG Ý phần `IR-06 §4.8` — 🔵 phần còn lại sai địa chỉ

`IR-06 §4.8` lạc hậu so với `m8-17` là **lỗi tài liệu của tôi**, nhận.

Phần *"chưa được Owner hệ thống/Ops ký"*: không có một đội Ops riêng. Chọn phương án `B` (M3 phát
revoke khi recall/sale_lock/hủy đơn) là **quyết định kiến trúc của Module 8** và tôi đã chốt, có đặc
tả `m8-17` kèm giới hạn nói trước. Cái còn thiếu là **M3 chấp nhận vai producer** — tức lại là
`IR-07`/`M3-14`.

**✅ Nhận: `0,5 ngày`** sửa `IR-06 §4.8` khớp `m8-17`.

> ### ⛔ CHẶN BỞI M3: nhận vai phát revoke. Cùng chữ ký với `C13`.

---

### `C15` — [Mới 16/09] Đơn 24/7 (COD) đặt ngoài khung `08:00–21:08`

**Ghi:** `Chưa có`. Nhãn `Vừa`. Mục thứ `15/17` của nhóm cuối.

**Sự thật trên repo** — `CallingWindow.cs:137-153`: ngoài giờ chỉ trả `opensAt` của ngày hôm sau,
**không dời `T0`, không dời cửa sổ xác nhận**. Cửa sổ 24/7 = `900s` (`W0196:57-59`).

```text
23:00        task vào, confirmation window bắt đầu
23:00–23:15  gate gọi ĐÓNG suốt (ngoài 08:00–21:08) → 0 cuộc gọi
23:15        window hết → IVR_CONFIRMATION_WINDOW_EXPIRED
```

### ✅ ĐỒNG Ý — ĐÂY LÀ PHÁT HIỆN GIÁ TRỊ NHẤT CỦA CẢ BẢN `16/09`, VÀ NÓ PHẢI LÀ ƯU TIÊN SỐ 1

Khách **không nhận cuộc nào**, mà M3 nhận mã *"hết hạn xác nhận"*. Trên đơn COD ban đêm đây là
**mất đơn hàng loạt mỗi đêm** — thứ duy nhất trong cả danh sách **chạm tiền của công ty**.

Tôi thừa nhận: tôi test khung giờ ở biên `21:00`/`21:08` (`UT-SCH-WINDOW-09`) nhưng **không test
task đến lúc `23:00`**. Đó là lỗ trong tư duy test của tôi.

### Nhưng nó bị xếp ở đâu?

| | |
| --- | --- |
| Vị trí trong danh sách | nhóm **C-vừa**, mục thứ **15/17** |
| Nhãn | `Vừa` |
| Ưu tiên tuần | **không có mặt** trong 4 mục "Bắt đầu từ đâu" |
| Có trong bảng `%` không | **Không** — là mục mới, *"không nằm trong baseline"* |
| Nội dung hướng dẫn | ✅ Đầy đủ và đúng — nêu cả hai phương án cho M3 và cho IVR |

> **Phát hiện đáng giá nhất của cả bản audit đang nằm ở vị trí `15/17` của nhóm cuối, nhãn `Vừa`,
> và không có mặt trong bảng điểm.** Trong khi `A1` — phần lớn là việc của chief — chiếm ưu tiên số
> `1`, và `B2` — mục không tồn tại — được giao lần thứ hai.
>
> ### Đây là bằng chứng rõ nhất rằng bản `16/09` không được sắp theo mức thiệt hại.

### ✅ ĐÃ LÀM XONG — `16/09`, `W-0298`

| | |
| --- | --- |
| Trạng thái | ✅ **XONG** — `dotnet test Ivr.sln` **`1063/1063`** pass, `0` failed, `0` skipped, exit `0` · phạm vi đúng `5` file · ✅ **đã commit `92091a1`** |
| Công bỏ ra | **nửa ngày**, ước lượng `1` ngày |

> ### ⚠️ Phương án đã đổi khi vào code — đây là chỗ tôi phải tự sửa mình
>
> Kế hoạch tôi viết buổi sáng chọn *"IVR dời `T0`/cửa sổ tới giờ mở"*. Đọc code thì **không làm
> được**:
>
> | Sự thật trên code | Hệ quả |
> | --- | --- |
> | `TaskIntakeService.cs:146-149` gọi `ConfirmationWindow.Create(source.Confirmation_window_started_at, …)` | **`T0` và cửa sổ do M3 gửi trên wire**, IVR không tự tính |
> | `W-0246`: `dial_token_expires_at` **==** window end, ghim `3` tầng | Dời cửa sổ ⇒ token M3 đã cấp rơi **ra ngoài** cửa sổ mới |
> | `OD-V1-05`: M3 là bên cấp token; chưa có resolver production | IVR **không cấp lại token được** |
> | `TaskIntakeService.cs:339-341` từ chối nếu `Confirmation_window_started_at > now` | Cửa sổ phải **đã bắt đầu** khi task tới ⇒ M3 gửi ngay tại `T0` |
>
> ⇒ Dời cửa sổ sang sáng hôm sau làm **dial token chết trước khi quay số**. Phương án đó không khó —
> nó **sai**. May là phát hiện lúc đọc code, không phải lúc gọi khách.

**Đã thi hành:** intake nhìn trước — nếu **mọi** lần gọi theo policy đều rơi ngoài giờ được phép gọi
thì trả `TASK_BLOCKED_OPERATIONAL` kèm `blocked_reasons` mới
`CALLING_WINDOW_CLOSED_FOR_WHOLE_CONFIRMATION_WINDOW`, **ngay lúc M3 còn đang giữ đơn**.

| Mặt cắt wire | Đổi không |
| --- | --- |
| `decision` — enum đóng | ❌ **Không** — `TASK_BLOCKED_OPERATIONAL` đã có trong OAS `:1365` |
| `blocked_reasons` — `array of string` | ❌ **Không** — thêm một chuỗi là non-breaking |
| Schema / version OAS | ❌ **Không** — không phải phát hành contract |

| Test mới | Kịch bản | Kết quả |
| --- | --- | --- |
| `UT-INTAKE-NIGHT-01` | 24/7 COD, `T0` `23:00` VN, hai mốc `+0s`/`+450s` đều ngoài giờ | ✅ chặn đúng |
| `UT-INTAKE-NIGHT-02` | Cùng task, `T0` `13:00` VN | ✅ vẫn nhận — không bắt nhầm |
| `UT-INTAKE-NIGHT-03` | `T0` `20:55` VN — mốc `2` rơi `21:02:30`, còn trong khung nhờ `W-0220` | ✅ nhận |

> **Đây là từ chối giả vờ, không phải lời giải cho đơn đêm.** Đơn vẫn chưa được gọi. Khác biệt là
> Sales không còn nhận `IVR_CONFIRMATION_WINDOW_EXPIRED` — mã đọc ra là *"khách không xác nhận
> kịp"* — cho một đơn **chưa ai gọi**.

> ### ⛔ Vế còn lại vẫn cần M3: giữ đơn đêm tới sáng, hay cấp cửa sổ khác.
> Cửa sổ và dial token đều tới trên wire **đã cấp sẵn**, nên quyết định đó **không phải của IVR**.
> Việc của tôi là không báo sai nguyên nhân — phần đó xong rồi.

**Còn lại của mục này:** `IR-06 §3.4.2` chưa ghi (file đang trong lượt `W-0297`; đã chuyển sang
`W-0303` mục `11`). Và e2e/sandbox **không phủ** guard này vì cả hai đặt `CallingWindow 0..1440`.

---

### `C16` (cũ `C13`) — Contact Gate: nguồn `phone_ref`/`dial_token`

**Ghi:** `~45%` — *"Tiến bộ ở phần M8 tự chủ… Phần lõi vẫn chưa: không có resolver/vault production."*

### 🔵 ⛔ SAI ĐỊA CHỈ + CHẶN BỞI M3

Phần tự chủ tôi đã làm: ledger resolve bền qua restart (`PostgresDialTokenResolveLedger.cs` +
migration), `phone_validation_status` khớp runtime, TTL chốt bằng đúng window end.

Phần lõi — *"ai là bên cấp `dial_token`"* — là **`OD-V1-05`**, và câu trả lời đề xuất là **M3**, vì
M3 giữ contact. Tôi **không thể tự nhận vai cấp token thay M3**.

Viết resolver production trước khi biết token do ai cấp, định dạng gì, xoay khóa ra sao = viết để
vứt.

> ### ⛔ CHẶN BỞI M3: `IR-07 Phần A-6` — M3 nhận vai cấp `dial_token`.

**✅ Việc của tôi:** `0` cho phần lõi; phần `E.164` gộp vào `B11` (`0,5 ngày` tài liệu).

---

### `C17` (cũ `C14`) — Block Gate / eligibility re-check trước attempt 2

**Ghi:** `~30%`. Phản biện: *"mới có đúng một điều kiện thật sự mới (fence revoke) và nó chưa kích
hoạt được."*

### 🟨 ĐÚNG NHƯNG — ĐÂY LÀ LẦN THỨ HAI CÙNG MỘT CHỮ KÝ BỊ ĐẾM

`C17` và `C13` là hai mặt của cùng một việc: `C13` = cơ chế thu hồi, `C17` = re-check trước attempt.
Cả hai chặn bởi **cùng một** `M3-14`, cả hai đã có **cùng hai** fence, cả hai chờ **cùng một**
endpoint.

Phản biện *"điều kiện window vốn có từ trước, điều kiện token chỉ áp ở lab/mock"* — đúng. Tôi không
cãi. Nhưng lý do token chỉ áp ở lab/mock là **chưa có vault production**, mà cái đó là `C16`, mà
`C16` chặn bởi `OD-V1-05`, mà `OD-V1-05` chờ M3.

> ### ⛔ CHẶN BỞI M3: `M3-14` + tên result code. Ba mục (`C13`, `C17`, `B7`) ship cùng nhau trong **2 ngày** kể từ ngày M3 ký.

**✅ Việc của tôi:** `0`.

---

# NHÓM D — "Đã đạt, đừng làm thừa"

### ✅ ĐỒNG Ý CẢ `11` MỤC — nhưng nhóm này đã không làm được việc của nó

Mục đích nhóm D là **chống làm thừa**. Nó có mặt, có bằng chứng đầy đủ, và tôi đồng ý với cả `11`
dòng. Nhưng nó **vẫn không chặn được `B2`** — một mục đã bị bác có bằng chứng ở bản `07/09` lại được
giao lần thứ hai ở bản `16/09`, vì `B2` nằm ở nhóm **B**, không nằm ở nhóm D.

> **Đề nghị:** mục nào đã bị bác ở lượt trước thì lượt sau đưa thẳng vào nhóm D kèm lý do bác, thay
> vì để nó quay lại nhóm việc. Đó là cách duy nhất nhóm D chống được làm thừa **giữa các lượt**.

**Nội dung thì tôi ĐỒNG Ý cả `11` mục** (`D1` intake · `D2` Official Order Gate · `D3` 8 entry gate ·
`D4` idempotency callback · `D5` M8 không tự set `COD_VERIFIED`/`PAID` · `D6` attempt policy theo
program · `D7` secrets · `D8` test offline · `D9` `W-0118`/`OD-15` · `D10` `customer_trust_level` ·
`D11` khung giờ `21:08`). Đây là những mục tôi làm xong và được kiểm đúng.

**Riêng `D11`** là mục **duy nhất** trong toàn bộ baseline được chấm `100%`. Nó chính là mục `1.1`
tôi đã đóng trong bản `07/09` — và nó cần `W-0215 → W-0220`, phát hiện ra rằng `21:07:30` **không
biểu diễn được** (`EndMinuteOfLocalDay` là phút, `Evaluate` bỏ giây, `1267 < 1267` là sai) nên
`21:07` sẽ **hỏng thầm lặng**. Tôi chọn `21:08` — giá trị nhỏ nhất thi hành được, đắt thêm 30 giây.

---

# PHẦN 3 — ĐÁNH GIÁ CÁC MỤC NHẬN XÉT NGOÀI BẢNG TASK

## 3.1 — ⚠️ MỘT PHÁT HIỆN TÔI TỰ RÚT: bảng biểu của bản `16/09` **không có lỗi**

Trong bản nháp đầu của đánh giá này, tôi khẳng định bản `16/09` có **`38/57` hàng bảng vỡ cột** và
**`57.491` ký tự không hiển thị khi render**. Tôi để nó ở vị trí lỗi nặng số một.

**Khẳng định đó sai. Tôi rút hoàn toàn.**

### Vì sao tôi sai

Script kiểm của tôi tách ô bằng mọi ký tự `|`. Nhưng GitHub-flavored Markdown quy định `\|` là
**escape hợp lệ cho một ký tự `|` nguyên văn**, kể cả bên trong backtick — và nó **không tách ô**.
Tác giả bản `16/09` **đã escape đúng** ở mọi đoạn `grep 'a\|b'` dán vào ô bằng chứng.

Chạy lại bằng parser tôn trọng `\|`:

| File | Bảng | Hàng | **Hàng vỡ cột** | Ký tự bị nuốt |
| --- | ---: | ---: | ---: | ---: |
| `toan-viec-can-lam-m8-2026-09-16.md` | `10` | `57` | **`0`** | **`0`** |

Và kiểm trực tiếp hai ô tôi từng nêu đích danh:

| Mục | Số ô | Cột *"Việc cần làm"* thật sự chứa gì |
| --- | ---: | --- |
| `B7` | **`5`** ✅ | *"Khi mở endpoint revoke: đóng job ngay lúc revoke… hoặc thêm `AND task.revoked_at IS NULL` vào `CloseMissedDeadlinesAsync`…"* — **đúng hướng dẫn, đúng cột** |
| `C15` | **`5`** ✅ | *"Owner/M3 chốt một trong hai: M3 giữ task 24/7 phát sinh 21:08–08:00 và gửi lúc 08:00, hoặc IVR nhận nhưng dời T0/cửa sổ tới giờ mở…"* — **đúng hướng dẫn, đúng cột** |

⇒ Cả năm khẳng định phái sinh đều sai và **đã gỡ khỏi bản này**:

- ~~`38/57` hàng vỡ cột~~
- ~~`57.491` ký tự không render~~
- ~~`11/11` hàng nhóm D mất bằng chứng~~
- ~~cột *"Việc cần làm"* của `15/30` mục hiển thị sai~~
- ~~`B7` hiện lời phản bác thay cho hướng dẫn; `C15` hướng dẫn nằm ở ô thứ `6`~~

### Tôi ghi lại chuyện này thay vì lặng lẽ xoá đi

Vì đúng thứ tôi đang phê bình bản `16/09` là **kết luận sai do công cụ kiểm sai** — `B2` bị khẳng
định *"dirty 17 ngày"* mà không đọc `git status` thật, và *"số pass là dev tự khai"* mà không chạy
`dotnet test`. Tôi vừa mắc **đúng lớp lỗi đó**, chỉ khác công cụ.

Nên luật rút ra áp cho cả hai phía, và tôi nhận phần của mình trước:

> **Một phát hiện mang tính cơ học thì phải kiểm bằng công cụ hiểu đúng định dạng — và phải chạy thử
> trên một mẫu đã biết kết quả trước khi tin nó.** Tôi đã không làm bước thứ hai.

### Điều còn đúng sau khi rút

Phần định dạng bản `16/09` **sạch**. Những gì còn lại trong đánh giá này **không** phụ thuộc vào
khẳng định đã rút:

| Phát hiện | Vẫn đứng |
| --- | --- |
| `B2` sai tiền đề — `git status` sạch, file thứ tư chưa từng tồn tại | ✅ độc lập |
| `B5` ghi `chưa làm` nhưng `2/3` đã xong — `[Theory] dropAlreadyApplied` | ✅ độc lập |
| `C7` ≡ `C8`, `C3`+`C4`+`C9`, `C13`+`C17` đếm trùng | ✅ độc lập |
| `A1` ghi `22` trong khi đếm thật là `21` | ✅ độc lập |
| `B7` trỏ `(C11)` trong khi mục revoke là `C13` | ✅ độc lập |
| `evidence: null = 56` không phải defect | ✅ độc lập |
| `1060/1060` test pass | ✅ độc lập |
| `14` mục nhóm C chặn bởi phần đơn hàng M3 | ✅ độc lập |
| `0` ước lượng trong toàn tài liệu giao việc | ✅ độc lập |

## 3.2 — Bảng tiến độ `31%`

**Số học đúng.** Tôi dựng lại được từng ô: B `2,63/12` = `21,9%` · C `5,75/14` = `41,1%` · Tổng
`8,38/27` = `31,0%`. Không có lỗi tính toán.

**Nhưng nó đo nhầm đại lượng.** Sáu lỗi thiết kế:

| # | Lỗi | Bằng chứng |
| --- | --- | --- |
| 1 | **Mẫu số chứa việc của người khác** | `25/27` mục cần chữ ký/quyết định/code của người khác |
| 2 | **Tử số bị khoét bằng một dòng luật** | *"Việc dev làm ngoài danh sách (**không tính vào %**)"* — **42 bullet**, gồm `W-0221` là mục `7.1` **đã đóng** |
| 3 | **`%` mục "một phần" là mức tồn, không phải tiến độ kỳ** | `B8` = `68%` nhưng `C.0` cùng file ghi `65%`; `0/8` hạng mục còn lại đã chạy |
| 4 | **Mục mới bị loại khỏi cả tử lẫn mẫu** | `C15` — nặng nhất — **không có mặt trong ô nào** |
| 5 | **Không có trục thời gian** | `%` là vị trí; muốn ra thời gian phải có tốc độ, mà tốc độ phải biết **ai đang đi** |
| 6 | **`100%` bất khả thi theo luật của chính tài liệu** | `C.0` đòi e2e hai chiều với M3 ⇒ nhóm C **không thể** đạt `100%` |

**Sửa hết lỗi sự thật tôi tìm được (bỏ `B2`), con số chỉ nhúc nhích `31% → 32%`.** Nghĩa là nó
**bền vững trước sai sót** — không phải vì nó chắc, mà vì **nó không đo công việc của tôi ngay từ
đầu**.

## 3.3 — Các "Chú ý mới của ngày 16/09"

| Chú ý | Đánh giá của tôi |
| --- | --- |
| *"Phản biện đối kháng: 2 trụ vững, 8 chỉnh, 0 bác"* | ✅ Tôi đánh giá cao việc tự phản biện. **Nhưng phản biện lại được dán vào sai cột** ở `B7` nên nó hiện ra như *"việc cần làm"* |
| *"Kill-switch/`REAL_CUSTOMER_CALL_ALLOWED` vẫn nhiều lớp đóng"* | ✅ **ĐÚNG, và đây là lời khen tôi nhận.** `IvrOptionsValidator.cs:51-53`, `DispatchGate.cs:36,68-73`, `FeatureFlagCatalog.cs:75`, appsettings cả 3 profile — tôi dựng nhiều lớp có chủ đích |
| *"172 commit không viết lại migration đã áp dụng nào"* | ✅ **ĐÚNG — và mâu thuẫn với việc chấm `B5` là `chưa làm`.** Đây chính là việc (1) của `B5` |
| *"Bảy mốc quyết định sếp; matrix M8 khớp flow 04/05 — không cần sửa"* | ✅ ĐÚNG. Đóng nửa `C1`. Ghi nhận |
| *"flow 04 `:1008` → `CANCELLED` vs M8 khóa `NO_STATE_CHANGE`"* | 🟨 Phản biện cùng ô đúng hơn: constraint chỉ ràng buộc **đề xuất** M8 phát ra, M3 toàn quyền hủy. Xem `B10` |
| *"Bốn mã `IVR_*` có `counts_toward_prepay=KHÔNG`; bộ đếm không nghe máy chưa có ngưỡng"* | ✅ ĐÚNG và tôi **tuân**: không dựng bộ đếm theo suy đoán |
| *"Giờ Vàng biên 30 giây; nếu M3 tính `T0` sau IVR extension 5 phút thì có thể vượt"* | ✅ **ĐÚNG, phát hiện tốt.** Biên `21:05` vs `21:05:30`. ⛔ Cần M3 xác nhận cách tính `T0` — thêm một câu vào `IR-07` |
| *"`specs/ui/04-ivr-menu-config.md:17` còn `customer_display_name`"* | ✅ ĐÚNG — và rộng hơn: `TargetV1SpeechPolicy.cs:47-49,75-77` vẫn giữ nó trong allowlist (vì template `v1`/`v2` còn dùng). Tôi nhận **15 phút** |
| *"`IR-06:354-356` mâu thuẫn `:325-326`"* | ✅ **ĐÚNG, lỗi tài liệu của tôi.** `:325` ghi đã chốt `21:08`, `:355` vẫn ghi *"Chưa chốt (`W-0215`)"*. Nhận **15 phút** |
| *"`evidence: null` tăng `51 → 56`"* | ❌ **BÁC — không phải defect.** `gate-status.mjs:477-492` **cố ý** chỉ assert dòng prompt-backed. Comment tại chỗ: *"a rule that would have been satisfied by creating 20 empty directories, **which is the opposite of what it is for**"*. Bản `07/09` đã rút claim này (`W-0224`). **Vi phạm thật: 0.** ⚠️ Đừng "trả nợ" bằng cách tạo thư mục rỗng |
| *"Không tìm thấy 'Bổ sung 09/09'/'10/09'"* | ✅ Ghi nhận — không có quyết định chief nào điều chỉnh mục M8 |
| *"'M8' trong bộ bàn giao 15/09 là voucher lễ tết, không phải IVR"* | ✅ **Phát hiện tốt.** Tránh được một lần nhầm số hiệu module |
| *"Pattern 'owner' tự đóng; chief cần xác minh 'owner' là ai"* | 🔵 **Trả lời dứt điểm ở đây, khỏi phải điều tra: `owner IVR` trong mọi commit và mọi dòng register là Nguyễn Quốc Toàn — tôi.** Tôi ký các quyết định kỹ thuật của Module 8 vì tôi là người viết và chịu trách nhiệm toàn bộ module. Phần **không** thuộc tôi thì tôi **để mở**: `OD-V1-10` (`NOT_SIGNED`, cần phép đo và báo giá), `OD-V1-09` (`HALF_SIGNED`), rủi ro pháp lý `OD-V1-11` (cần xác nhận cấp công ty), và `4` dòng chạm contract M3 (cần M3 đối ký) |
| *"Chỗ dev tự ký quyết định thay Owner"* (danh sách dài) | 🟨 Xem `A1`. Tóm tắt: các vai *Sales/Security/Platform/Legal* **không tồn tại như các đội riêng** trong dự án này; `OD-V1-10` (tiền) tôi **để `NOT_SIGNED`**, `OD-V1-09` **`HALF_SIGNED`** — đúng ranh giới. Residual thật: `OD-V1-11` (rủi ro pháp lý → **xác nhận cấp công ty**) và `4` dòng chạm contract M3 (→ **M3 đối ký**) |

## 3.4 — Bảng smoke test server `192.168.1.61`

**`12/12` ĐẠT. ✅ Tôi ghi nhận và đánh giá cao phần này** — đây là phần tốt nhất của bản `16/09`: có
chạy thật, có log, có bằng chứng, có cả phần "Vấn đề" trung thực.

Kết quả đáng chú ý: `5` migration mới áp được trên volume cũ · e2e `3` kịch bản
CONFIRM/CANCEL/NOANSWER chạy trọn trong `~20` giây · **callback body `14` field, quét số `10-12` chữ
số = rỗng, log `0` lần xuất hiện `dial-token-`/`phone-ref-`** — tức **không rò PII**, đúng thứ tôi
dựng `PiiGuard` để bảo đảm · `9/9` đường Swagger trả `404`.

`6` "Vấn đề" nêu ra, đánh giá của tôi:

| Vấn đề | Đánh giá |
| --- | --- |
| `gh-247-prod-v1` vẫn `approved_for_production=true` | ✅ ĐÚNG = `B3`. Nhận |
| `RUNTIME_GATE_ADMIN` vẫn `environment=NULL` | ✅ ĐÚNG = `B4`. Nhận, và tôi chọn siết |
| `ProductionTargetV1FieldsApproved=YES` bake vào ảnh | ✅ ĐÚNG = `B6`. Nhận |
| Log migrate in `libgssapi_krb5.so.2` trước khi chạy | ✅ **ĐÚNG, và tôi nhận — đây là phát hiện mới tôi chưa biết.** Không chặn (`5/5` migration áp được) nhưng một dòng `Error` trong ảnh chiseled là thứ phải dọn. **0,5 ngày** |
| Policy mock chỉ có trong `dev-seed/seed.sql`, phải seed thủ công | ✅ **ĐÚNG, phát hiện tốt.** Môi trường mới không tự chạy được e2e. Đưa seed dev vào đường chuẩn của compose. **0,5 ngày** |
| Worker log `46k` dòng/`10` phút vì log toàn bộ SQL EF ở mức Information | ✅ **ĐÚNG, nhận.** Không lộ PII nhưng làm log không đọc được. Hạ mức log EF. **0,5 ngày** |

> **Ba mục cuối là việc thật của tôi mà bảng task không có.** Chúng không nằm trong `30` mục A/B/C,
> không nằm trong `%`, nhưng đều là thứ tôi sẽ sửa. Cộng **1,5 ngày**.

## 3.5 — Phần `E` (giới hạn đợt kiểm) — và điều tôi phải đính chính

Tôi đánh giá cao việc bản `16/09` **tự khai giới hạn**. Nhưng có một dòng tôi phải đính chính vì nó
đặt nghi vấn lên toàn bộ bằng chứng test của tôi:

> *"Chỉ đọc tĩnh tại `ff22227`; **không build, không `dotnet test`**… **Mọi số pass (`895/895`,
> `951/951`, `986…`) là dev tự khai.**"*

**Tôi đã chạy thật trên `main@37606bc`:**

```text
Ivr.ContractTests      Failed: 0, Passed:   24, Skipped: 0      1 s
Ivr.UnitTests          Failed: 0, Passed:  713, Skipped: 0      5 s
Ivr.ChaosTests         Failed: 0, Passed:    8, Skipped: 0     53 s
Ivr.IntegrationTests   Failed: 0, Passed:  315, Skipped: 0     10 m 24 s
──────────────────────────────────────────────────────────────────────
                       Failed: 0, Passed: 1060, Skipped: 0   exit 0
```

**`1060/1060` pass, `0` fail, `0` skip.** Số tôi khai không những đúng mà còn **thấp hơn thực tế**:
bản `07/09` khai `900`, nay là `1060` — **tăng `160` test trong `10` ngày**.

> ### ⇒ Câu *"là dev tự khai"* nay đã có kiểm chứng độc lập, và nó xác nhận phía tôi.
> `10` phút chạy `dotnet test` rẻ hơn nhiều so với việc để một dòng nghi vấn về tính trung thực nằm
> trong tài liệu bàn giao. **Đợt sau xin chạy trước khi viết.**

Các giới hạn khác tôi đồng ý và ghi nhận: không đọc code M3, không đọc toàn văn flow `04`/`05`,
không mở `FeatureFlagAdminService.cs`, không kiểm cấu hình thật trên staging. Đó là những giới hạn
trung thực và tôi không trách.

## 3.6 — "Bắt đầu từ đâu (đề xuất tuần này)"

| Đề xuất của bản `16/09` | Đánh giá của tôi |
| --- | --- |
| **1.** `A1` register | ❌ **BÁC.** Phần lớn là việc của chief auditor + một xác nhận cấp công ty. Phần của tôi: đổi nhãn, **0,5 ngày**. Không thể là ưu tiên 1 khi mệnh lệnh cho dev là *"đừng làm gì"* |
| **2.** `B7` deadline sweep | 🟨 **Hạ ưu tiên.** Chưa kích hoạt được (`0` writer cho `revoked_at`). Ship cùng `C13` khi M3 ký |
| **3.** *"Giữ thành quả"* | ✅ Ghi nhận |
| **4.** Đưa seed dev vào đường chuẩn compose | ✅ **ĐÚNG, nhận. `0,5 ngày`** |

### Thứ tự đúng theo tôi

| # | Mục | Vì sao |
| --- | --- | --- |
| **1** | **`C15`** — đơn COD ban đêm | Thứ duy nhất **đang mất tiền của công ty mỗi đêm** |
| **2** | **`C6`** — `500` → `422` | Lỗi thật M3 sẽ đâm vào ngay khi nối. Nằm hoàn toàn trong tay tôi |
| **3** | **`B6`** — cờ production về `NO` | Nửa ngày, gỡ một cờ sai mặc định, **trả lời `A1` bằng code** |
| **4** | **`B3`** — hạ `approved_for_production` | Cùng lý do với `B6` |
| 5 | `B4` siết env · `B5` runbook · `B9` audit-evidence | Code, tự làm được |
| 6 | Cụm tài liệu (`A1` nhãn · `B8` · `B10` · `B11` · `C2` · `C9` · `C12` · `C14` · `IR-06`) | Gộp một lượt |
| 7 | 3 việc vận hành từ smoke (`krb5` · seed compose · log EF) | Dọn nợ hạ tầng |

---

# PHẦN 4 — BẢNG TIẾN ĐỘ THAY THẾ

## 4.1 — Cùng cấu trúc, chỉ sửa sự thật đã kiểm (bỏ `B2`)

| Nhóm | Tổng mục | Đã xong | Một phần | Chưa làm | % hoàn thành | Δ |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| A — việc khẩn | 1 | 0 | 0 | 1 | 0% | — |
| B — code lõi | **11** *(−B2)* | 1 | 3 | **7** | **24%** | **+2** |
| C — kết nối | 14 | 0 | 13 | 1 | 41% | — |
| **Tổng** | **26** | **1** | **16** | **9** | **32%** | **+1** |

## 4.2 — Trục còn thiếu: **ai giữ nước đi tiếp theo**

| Ai giữ nước đi tiếp | Mục | Mã |
| --- | ---: | --- |
| ✅ Đã xong | **1** | `D11` |
| 🟢 **Tôi — và nước đi đó ĐÓNG mục** | **4** | `B3` `B5` `B6` `C6` |
| 🟡 Tôi có một lát cắt, mục đóng ở chỗ khác | **6** | `B8` `C2` `C5` `C9` `C12` `C14` |
| 🟠 Cần một phán quyết trước | **3** | `B4` `B9` `B10` |
| 🔴 **Không có nước đi nào của tôi** | **12** | `A1` `B1` `B11` `B12` `C1` `C3` `C4` `C10` `C11` `C13` `C16` `C17` |

**`4/26` (`15%`)** là thứ tuần làm việc của tôi đóng lại được.
**`12/26` (`46%`)** không phải "chưa làm" — là **không có gì để làm**.

## 4.3 — Ai đang chặn `12` mục đỏ

| Đang chờ ai | Mục | Cần gì | Tình trạng |
| --- | --- | --- | --- |
| ⛔ **Module 3** | `C1` `C3` `C4` `C10` `C13` `C16` `C17` | Phần đơn hàng xong, rồi đối ký `IR-07` (`407` dòng, mỗi câu có sẵn phương án) | **Phần đơn hàng của M3 chưa xong** |
| ⛔ **Module 3** | phần seam của `C11` `C14` `B9` `C6` `C15` | Producer tạo task từ đơn + consumer đổi `order state` | **Phần đơn hàng của M3 chưa xong** |
| ⏳ **Nhà mạng** | `B12` `B1` | **Báo giá Vinaphone / MobiFone / Viettel** — hướng Mobile SIP trunk **đã chốt** | Đã liên hệ đủ ba, **đang chờ phản hồi** |
| 📋 **Cấp công ty** | `A1`(1) `B6`(nửa) `C12` | Một câu xác nhận rủi ro pháp lý V1 (ghi âm · không-gọi-lại · thời hạn lưu) | Cần nêu rõ một lần |
| 📋 **Chief auditor** | `A1`(nhãn register) `B10`(tên mã) `C7`/`C8` | Phán quyết từ vựng result code | Tôi đã đề xuất sẵn bảng map |
| 🔒 Credential/issuer | `C11` | Issuer thật cho callback production | Chặn **có chủ đích**, fail-closed |

## 4.4 — **Trần**: tôi đi hết sức thì con số dừng ở đâu

| Mốc | Tôi bỏ ra | Điều kiện | `%` | Ai phải hành động để đi tiếp |
| --- | --- | --- | ---: | --- |
| **Hôm nay** | — | — | **32%** | — |
| **+1 tuần** | `5–7` ngày | Không cần ai | **≈45%** | — |
| **+2,5 tuần** | thêm `≈4` ngày | `2` phán quyết của tôi | **≈51%** | — |
| **Sau đó** | **∞ ngày công** | — | ⛔ **51%** | **Phần đơn hàng M3 · báo giá nhà mạng** |

> ### TRẦN TUYỆT ĐỐI CỦA TÔI LÀ `≈51%`.
>
> `49%` còn lại **không phải việc chưa làm**. Đó là `12` mục **không có một hành động nào của tôi
> tồn tại**, cộng phần đóng-mục của `6` mục nữa nằm ở **phần đơn hàng của Module 3** và ở **báo giá
> nhà mạng**.
>
> **Thêm dev-tuần sau mốc `51%` sản sinh đúng `0` điểm.** Muốn qua `51%` thì thứ phải thay đổi
> **không phải tốc độ của tôi**, mà là **ngày phần đơn hàng của Module 3 xong** và **ngày nhà mạng
> báo giá**.

## 4.5 — Bảng tôi đề nghị dùng thay bảng `31%`

| Chỉ số | Giá trị |
| --- | ---: |
| Mục tôi đóng được tuần này | **4 / 26** |
| Ngày công cho `17` hạng mục (§5.1–5.3) | **7–7,5 ngày** |
| Trần tôi tự đi tới | **≈51%** |
| Mục chờ một bên khác | **21 / 26** |
| **Mục chờ riêng Module 3** | **14** |
| **Mục chờ báo giá nhà mạng** | **2** (`B12` · `B1`) |
| Phía M8 đã sẵn sàng bàn giao seam | **`IR-07` `407` dòng + sandbox chạy được** |
| Thời gian M8 cần **sau ngày phần đơn hàng M3 xong + đối ký** để đóng `14` mục C | **≈4 ngày** |
| Test | **`1060/1060` pass** |
| Sản lượng `10` ngày | **`173` commit · `+28.796` dòng `src`+`tests`** |

---

# PHẦN 5 — VIỆC THẬT CỦA TÔI, CÓ ƯỚC LƯỢNG

Bản `16/09` **không có một ngày công, một mốc, một ngày tháng nào** (`grep` = `0` kết quả). Chỉ có
`Dễ` ×15 · `Vừa` ×12 · `Khó` ×3 — ba nhãn đo **kích thước code**, không đo **thời gian chờ**. `B12`
là `Khó` vì **phải mua SIM**, không phải vì code khó.

Nên tôi tự ước lượng:

## 5.1 — Code (`5` mục) — `4–4,5` ngày

| # | Việc | Mục | Ngày |
| --- | --- | --- | ---: |
| 1 | ✅ **XONG** — chặn tại intake khi mọi lần gọi rơi ngoài giờ (`W-0298`, `3` test) | `C15` | ~~`1`~~ → **0,5** |
| 2 | Token hết hạn muộn hơn window → `422` có reason thay vì `500`; OAS mô tả ràng buộc bằng | `C6` | **1–1,5** |
| 3 | ✅ **XONG** — migration xoá hai hàng seed + guard phủ định toàn bảng (`W-0300`) | `B3` | ~~`1`~~ → **0,5** |
| 4 | ✅ **XONG** — `4` file về `NO` + test ghim mặc định, mutation hai chiều (`W-0299`) | `B6` | ~~`0,5`~~ → **0,25** |
| 5 | ✅ **XONG** — reader lọc env + revoke hàng seed + `CHECK` + `4` test (`W-0301`) | `B4` | ~~`0,5`~~ → **1** |

## 5.2 — Tài liệu (`9` mục, gộp một lượt) — `1,5` ngày

`IR-06:354-356` gỡ *"Chưa chốt (W-0215)"* · `specs/ui/04:17` bỏ `customer_display_name` ·
`m8-05 §3.1` sửa dòng `IVR_OPT_OUT` · `IR-06 §3.5` thêm đoạn `priority` · `IR-06 §4.8` khớp `m8-17` ·
`IR-06 §4.3` bảng map result → reason (`B10`+`C7`/`C8`) · `IR-06 §6`+`specs/api/04` gỡ mâu thuẫn
`E.164` · register đổi nhãn `M8_POSITION_SIGNED / M3_NOT_RECEIVED` · xóa `OptOutSuppressionPolicy`.

## 5.3 — Vận hành (`3` mục từ smoke test, không có trong bảng task) — `1,5` ngày

Dọn `libgssapi_krb5.so.2` trong ảnh migrate · đưa `dev-seed/seed.sql` vào đường chuẩn compose ·
hạ mức log EF của worker.

## 5.4 — Sau đó, nếu tôi tự phán quyết phạm vi — `2` ngày

`B9` nửa `audit-evidence` (endpoint đọc audit append-only, masked, permission riêng).

## 5.5 — TỔNG

> # `7–7,5` ngày công cho **`17` hạng mục** (§5.1 + §5.2 + §5.3).
> # ✅ **Đã xong `5/17`** — `C15`, `B6`, `B3`, `B4`, `C6`. Hết **`3,75` ngày**. Còn `3,25–3,75` ngày.
> # `+2` ngày nếu làm thêm `B9` nửa `audit-evidence` (§5.4) ⇒ **`9–9,5` ngày**.
> #
> # `24` mục còn lại: **`0` ngày công của tôi. Chúng không chờ tôi.**

---

# PHẦN 6 — KẾT LUẬN

## 6.1 — Bản `16/09` làm được gì tốt

Tôi không phủ nhận. Bốn thứ có giá trị thật:

1. **`C15`** — đơn COD ban đêm chết lặng. Lỗ thật, chạm tiền, tôi không tự thấy. **Phát hiện giá trị
   nhất.**
2. **`C6` nửa sau** — `500` thay vì `422`. Lỗi thật trong code tôi viết.
3. **Smoke test `12/12`** trên server thật, có log, có phần "Vấn đề" trung thực — trong đó `3` việc
   vận hành tôi chưa biết.
4. **`IR-06:354-356`, `specs/ui/04:17`, biên `30` giây Giờ Vàng** — mâu thuẫn tài liệu có thật.

Và độ chính xác `file:dòng` cao. Tôi kiểm khoảng `40` tham chiếu, sai không đáng kể.

## 6.2 — Bản `16/09` sai ở đâu

| # | Lỗi | Mức |
| --- | --- | --- |
| 1 | **Ưu tiên sắp sai mức thiệt hại** — `A1` (phần lớn là việc chief) xếp số `1`; `C15` (mất đơn COD mỗi đêm) xếp `15/17` nhóm cuối, nhãn `Vừa`, **không có trong bảng điểm** | **Nặng** |
| 2 | **`B2` sai tiền đề** — working tree sạch, file thứ tư chưa từng tồn tại. Giao lần thứ hai sau khi đã bị bác | **Nặng** |
| 3 | **`B5` ghi `chưa làm` nhưng `2/3` việc đã xong** — `[Theory]` cả hai lịch sử schema, tài liệu 4 tình huống, runbook | **Nặng** |
| 4 | **Chấm `%` cho tôi trên `14` mục chặn bởi M3**, trong khi `C.0` tự định nghĩa "xong" cần M3 | **Nặng** |
| 5 | **`C7` ≡ `C8`** — một việc đếm hai lần; `C3`/`C4`/`C9` — một chữ ký M3 đếm ba lần | Vừa |
| 6 | **`A1` đo tôi bằng sơ đồ tổ chức không tồn tại**; và tự mâu thuẫn `22` vs `21` | Vừa |
| 7 | **`evidence: null = 56`** nêu lại như nợ, dù đã bị rút ở `07/09` và gate cố ý cho phép | Vừa |
| 8 | **Bằng chứng lạc hậu còn trong ô** — `B12` (cửa sổ `10` phút đã có từ `W0144`) · `B9` (mô tả `admin-ui/` đã bị xoá ở `c0e6609`) | Vừa |
| 9 | **`0` ước lượng, `0` mốc** trong toàn bộ tài liệu giao việc | **Nặng** |
| 10 | **Nghi vấn *"số pass là dev tự khai"*** mà không chạy `10` phút `dotnet test` để kiểm | Vừa |

## 6.3 — Về seam với Module 3 — điều tôi cần để trả lời "chừng nào xong"

> Tôi đã đọc hết bản `16/09` và kiểm từng mục trên code.
>
> **Tôi nhận `17` hạng mục, ước lượng `7–7,5` ngày công**, gồm cả `4` phát hiện mới của bản audit
> (`C15`, `C6`, `IR-06:354-356`, và `3` việc vận hành từ smoke test). **`C15` tôi xếp ưu tiên `1`**
> vì đơn COD ban đêm đang mất thật mỗi đêm.
>
> **Ba mục tôi trả lại, có bằng chứng:**
>
> - **`B2`** — working tree sạch; `Start-AndroidSoftphoneLab.ps1` **chưa từng tồn tại ở bất kỳ
>   commit nào**, kể cả tại `ff22227`. Đây là lần thứ hai mục này được giao sau khi đã bị bác có
>   bằng chứng ở `07/09`.
> - **`B5`** ghi `chưa làm` nhưng `2/3` việc đã có: `[Theory] dropAlreadyApplied` chạy **cả hai lịch
>   sử schema**, `docs/database/expand-contract.md` đủ `4` tình huống, `deploy/ci/rollback.md` tồn
>   tại. Còn thiếu đúng một dòng liệt kê môi trường.
> - **`evidence: null = 56`** không phải defect — `gate-status.mjs:477-492` **cố ý** chỉ assert dòng
>   prompt-backed. Vi phạm thật: `0`.
>
> **Ba lỗi số liệu nhỏ, sửa là dùng được ngay:**
>
> 1. `C7` và `C8` là **cùng một việc** — cùng `4` mã đích, cùng bằng chứng, cùng người làm ⇒
>    *"17 việc kết nối"* thật ra là **`16`**. Tương tự `C3`+`C4`+`C9` cùng chờ `M3-07`, và
>    `C13`+`C17` cùng chờ `M3-14` — một chữ ký trễ đang bị trừ điểm **ba lần** và **hai lần**.
> 2. `A1` ghi `22` dòng tự đóng ở thân mục và `21` ở phản biện — **trong cùng một ô**, ở mục xếp ưu
>    tiên `1`. Đếm thật: **`21` `CLOSED`** (`+1 HALF_SIGNED`, `+1 NOT_SIGNED`).
> 3. `B7` trỏ *"mở endpoint revoke **(C11)**"*, nhưng sau khi đánh số lại mục revoke là **`C13`**.
>
> **Và một đính chính từ phía tôi, nói trước cho sòng phẳng:** bản nháp đầu của đánh giá này khẳng
> định bảng biểu của bạn vỡ cột hàng loạt và mất `57.491` ký tự khi render. **Sai — tôi đã rút
> hoàn toàn.** Script kiểm của tôi không hiểu escape của GFM. Chạy lại bằng parser đúng: `10` bảng,
> `57` hàng, **`0` lỗi** — định dạng của bạn sạch. Chi tiết và lý do tôi giữ lại vết ở **§3.1**.
>
> ### Và đây là điều quan trọng nhất
>
> **`14/17` mục nhóm C là seam giữa Module 8 và Module 3 — và nó gắn đúng vào phần đơn hàng, phần
> bên M3 chưa xong.**
>
> Chuỗi e2e mà `C.0` đòi là `task → gọi → callback → **order state**` — **mắt xích cuối cùng là
> trạng thái đơn hàng**. `8/8` thứ nhóm C cần từ M3 đều nằm trong phần đó: producer tạo task từ đơn ·
> consumer đổi trạng thái đơn · `order_version` · `cancellation_reason_code` · endpoint thu hồi khi
> hủy đơn · bên cấp `dial_token` từ contact của đơn · registry `program_code` · xử lý đơn 24/7 đặt
> ban đêm.
>
> Phía tôi đã xong và sẵn sàng bàn giao ngay trong ngày phần đơn hàng bên đó sẵn sàng:
>
> | | |
> | --- | --- |
> | `IR-07` phiếu chốt một lượt | **`407` dòng** — `3` breaking change nêu trước · `14` mục đã chốt · câu hỏi chia `4` nhóm, **mỗi câu có sẵn phương án**, chọn *Ý* hoặc *KHÁC*, **im lặng = đồng ý** · việc M3 làm sau khi ký · ô ký |
> | `IR-06` | Bàn giao API đầy đủ, gồm `§4A` bề mặt quản trị `31` endpoint |
> | `IR-08` + `docker-compose.sandbox.yml` | M3 **tự dựng stack IVR, gọi thử, nhận callback thật** do runtime M8 phát |
>
> `C.0` của chính bản `16/09` định nghĩa "xong" của một mục C là *"**hai phía** cùng tham chiếu một
> schema đã phát hành + e2e hai chiều trên server test chung"*.
>
> **Một phía chưa tồn tại.** Nên `14` mục đó **không thể** đạt `100%`, bất kể tôi bỏ ra bao nhiêu
> ngày công. Chấm `41%` rồi giao lại `59%` vào danh sách tuần sau của tôi là giao một việc chưa ai
> làm được.
>
> ### Đề nghị của tôi
>
> **Gỡ nhóm C ra khỏi bảng điểm, chuyển thành một dòng phụ thuộc:**
>
> > *"Seam S7 (M8 ↔ M3): phía M8 sẵn sàng, `IR-07` `407` dòng + sandbox chạy được.
> > **Mở khoá khi phần đơn hàng của M3 xong và hai bên đối ký. Từ ngày đó, M8 cần thêm `≈4` ngày
> > để đóng cả `14` mục.**"*
>
> Đó là câu trả lời thật cho *"chừng nào xong"*: **`4` ngày kể từ ngày M3 đối ký** — một con số cam
> kết được, thay cho một `%` đứng yên mỗi tuần.
>
> **Cho tôi ngày dự kiến phần đơn hàng của M3 xong.** Có ngày đó tôi lùi ngược `4` ngày và ra được
> mốc đóng nhóm C cho cả hệ — một mốc thật, cam kết được, thay cho một ô `%`. Không có ngày đó thì
> tôi chỉ ước lượng được `17` mục của mình, và trần tôi tự đi tới là **`≈51%`** — sau đó thêm bao
> nhiêu ngày công cũng ra `0` điểm.
>
> Một đính chính: phần `E` ghi *"mọi số pass là dev tự khai"*. Tôi vừa chạy `dotnet test Ivr.sln`
> trên `main@37606bc`: **`1060/1060` pass, `0` failed, `0` skipped, exit `0`**.

## 6.4 — Tình hình đường gọi thật và việc cần quyết ở cấp công ty

### Đường gọi thật — đã chốt hướng, đang chờ báo giá

> ### ✅ **Đã chốt: đi hướng Mobile SIP trunk**, không dùng gateway 32 SIM.
> ### ⏳ **Đã liên hệ đủ ba nhà mạng — Vinaphone, MobiFone, Viettel. Đang chờ phản hồi và báo giá.**

Chuỗi phụ thuộc, để mọi người thấy vì sao hai mục này đứng yên:

```text
Báo giá nhà mạng
      └─> chốt gói + số kênh
             └─> có trunk thật
                    └─> viết adapter production (B12)  ≈2–3 tuần
                           └─> W-0008 có cuộc gọi đo được
                                  └─> calibrate 4 số năng lực (B1)  ≈1 ngày
```

Phần chuẩn bị tôi đã làm xong **trước khi có đường thật**, để lúc có trunk là cắm vào chạy chứ không
phải viết lại: khoá sở hữu controller ARI trước khi quay số · dispatch song song có trần + dừng nhận
cuộc mới sau chuỗi lỗi · ledger dial-token bền qua restart chỉ lưu `token_hash` · phê duyệt gọi
production gắn theo môi trường. Phương án đề xuất bắt đầu **`8` kênh một nhà mạng rồi đo thật trước
khi mở rộng** — ở `plan/…/mobile-sip-trunk-production-32-channels-plan-2026-09-15.md`.

### Một việc cần xác nhận ở cấp công ty

| Việc | Vì sao không nên để nằm trong một dòng kỹ thuật |
| --- | --- |
| **Xác nhận công ty nhận rủi ro pháp lý cho V1**: ghi âm cuộc gọi tới khách · thời hạn lưu dữ liệu · danh sách không-gọi-lại · nội dung lời thoại đọc tên món và vùng giao | Đây là **rủi ro pháp lý của công ty**, không phải một lựa chọn kỹ thuật. Hiện nó đang được ghi nhận ở `OD-V1-11` và `DR-12` như một dòng trong register. Một câu xác nhận ở cấp công ty là đủ, và nó đóng gọn cả cụm `A1`(1) · `B6` · `C12` |

### Trạng thái an toàn hiện tại, để mọi người yên tâm

Hệ thống **chưa và không thể gọi khách hàng thật** — có chủ đích, nhiều lớp:

| Lớp chặn | Ở đâu |
| --- | --- |
| Từ chối khởi động nếu bật cờ gọi khách thật | `IvrOptionsValidator.cs:51-53` |
| `REAL_CUSTOMER_CALL_ALLOWED=NO` trong mọi profile | `appsettings` Api · Worker · `LocalMockE2E` |
| Kill switch + `PRODUCTION_RELEASE_NOT_APPROVED` theo từng môi trường | `DispatchGate.cs:36,68-73` |
| Mặc định an toàn của feature flag | `FeatureFlagCatalog.cs:75` |
| Nhánh không-mock-không-lab gắn gateway từ chối dispatch | `SchedulerCapacity.cs:704` |
| Lab Asterisk chỉ quay đúng một alias đích, không ra PSTN | `AsteriskAriSimGateway.cs:81-88` |
| Đường gửi callback production fail-closed | `CallbackDeliveryOptions.cs:72-77` |

Và trên server test: e2e `3` kịch bản chạy trọn, **callback không chứa số điện thoại hay dial token**
(quét `10-12` chữ số = rỗng, log `0` lần xuất hiện `dial-token-`/`phone-ref-`), `1060/1060` test pass.

---

# PHỤ LỤC A — Lệnh tái lập mọi con số

```bash
# PHẦN 1 — bộ tài liệu bàn giao seam S7 đã sẵn sàng tới đâu
wc -l integration-requirements/07-module-3-decision-sheet.md   # 407 dòng
grep -n '^## ' integration-requirements/07-module-3-decision-sheet.md
ls docker-compose.sandbox.yml integration-requirements/08-module-3-sandbox-guide.md

# B2 — sai tiền đề
git status --porcelain
git ls-files -m deploy/lab/
git log --all --oneline -- deploy/lab/Start-AndroidSoftphoneLab.ps1
git cat-file -e ff22227:deploy/lab/Start-AndroidSoftphoneLab.ps1

# B5 — hai trong ba việc đã xong
sed -n '15,19p' tests/Ivr.IntegrationTests/ExpandContractMigrationTests.cs   # [Theory] dropAlreadyApplied
sed -n '22,33p' docs/database/expand-contract.md                              # 4 tình huống
grep -n W0122 deploy/ci/rollback.md

# B3 / B4 / B6 — ba cờ production
sed -n '48,62p'   src/Ivr.Infrastructure/Persistence/Migrations/20260905120000_W0196SignedProductionAttemptPolicy.cs
sed -n '143,163p' src/Ivr.Infrastructure/Persistence/Migrations/20260905034908_W0195RuntimeGateApprovals.cs
grep -rn ProductionTargetV1FieldsApproved src --include=*.json | grep -v /bin/

# B7 — revoked_at không có writer
grep -rn "revoked_at\|RevokedAt" src --include=*.cs | grep -v "/bin/\|/obj/\|Designer\|RuntimeGate\|Console"

# C6 — đường đi tới 500
sed -n '118,125p' src/Ivr.Infrastructure/Persistence/PersistenceInvariantValidator.cs
sed -n '38,47p'   src/Ivr.Api/Middleware/ErrorEnvelopeMiddleware.cs
grep -n InternalError src/Ivr.Api/Middleware/IvrErrorResponseWriter.cs

# C15 — ngoài giờ không dời T0
sed -n '130,155p' src/Ivr.Infrastructure/Scheduling/CallingWindow.cs

# A1 — register thật
grep -c 'CLOSED' specs/_review/open-decisions-register.md
sed -n '36,38p'  specs/_review/open-decisions-register.md    # OD-V1-09/10/11

# evidence:null là cố ý
sed -n '477,492p' deploy/ci/scripts/gate-status.mjs

# Sản lượng 10 ngày
git rev-list --count 9f13bea..HEAD
git diff --shortstat 9f13bea..HEAD -- src/ tests/

# Không có ước lượng nào trong bản giao việc
grep -oniE "(ước tính|estimate|man-day|ngày công|hạn chót|sprint)" plan/toan-viec-can-lam-m8-2026-09-16.md

# Test
dotnet test Ivr.sln        # 1060 passed, 0 failed, 0 skipped, exit 0
```

```python
# Kiểm bảng Markdown ĐÚNG CHUẨN GFM — \| là escape, KHÔNG tách ô.
# (Script cũ của tôi thiếu negative-lookbehind này và đã cho kết quả sai — xem §3.1)
import re
PAT = re.compile(r"(?<!\\)\|")
lines = open(r'plan/toan-viec-can-lam-m8-2026-09-16.md', encoding='utf-8').read().split('\n')
broken = rows = 0
i = 0
while i < len(lines):
    if (lines[i].startswith('|') and i + 1 < len(lines) and lines[i + 1].startswith('|')
            and set(lines[i + 1].strip().strip('|').replace('|', '')) <= set('-: ')):
        hdr = len(PAT.split(lines[i].strip().strip('|')))
        j = i + 2
        while j < len(lines) and lines[j].startswith('|'):
            rows += 1
            if len(PAT.split(lines[j].strip().strip('|'))) > hdr:
                broken += 1
            j += 1
        i = j
    else:
        i += 1
print('hàng:', rows, '| hàng vỡ cột:', broken)   # → 57 | 0
```

# PHỤ LỤC B — Giới hạn của chính bản đánh giá này

Tôi làm phần này vì bản `16/09` có làm, và nó đáng được làm:

- Kiểm trên `main@37606bc`. **Có** chạy `dotnet test Ivr.sln` đầy đủ (`1060/1060`, exit `0`).
- **Không** dựng lại server test `192.168.1.61` — tôi nhận nguyên `12` dòng smoke của bản `16/09`.
- **Không** chạy compose, `oasdiff`, `image-selftest.mjs`, trivy trong lượt này.
- **Không** đọc code Module 3 và không đọc toàn văn flow `04`/`05` của bộ bàn giao `15/09`. Các mục
  dẫn nguồn đó — `C1`, `C7`, `C8`, `C15`, `B10` — tôi chỉ kiểm được **nửa M8**.
- Ước lượng ở **PHẦN 5** là ước lượng của tôi, không phải cam kết đã thương lượng. `C6` có thể trượt
  nếu lượt phát hành OAS kéo theo re-pin nhiều hơn `5` nơi đã biết.
- Số ngày chờ ở **PHẦN 1** tính từ **ngày commit tạo file** trong git. Nếu tài liệu được gửi qua
  kênh khác sớm hơn hoặc muộn hơn thì con số đó xê dịch — nhưng ô ký trống thì không xê dịch.

---

**Nguyễn Quốc Toàn**
Dev trực tiếp Module 8 — IVR Order Confirmation
`16/09/2026` · `main@37606bc` · **`1063/1063` test pass** sau `W-0298`
