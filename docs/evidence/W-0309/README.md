# W-0309 — Gom `6` phiếu nhóm A về đúng người, sau khi phát hiện `5/6` gửi cho phòng ban không tồn tại

**Ngày:** `2026-09-17` · **Baseline:** `main@fbe6edb` · **Loại:** tài liệu · **`0` file `.cs`**

`REAL_CUSTOMER_CALL_ALLOWED=NO`

---

## 0. Tóm tắt

Lượt này bắt đầu từ việc kiểm lại **một câu của chính tôi** — *"việc gỡ được nhiều nhất là gửi `6`
phiếu nhóm A; chúng đã soạn xong, không tốn ngày công nào"*. Tôi đã lặp câu đó **ba lần** trong ba
phiên trả lời khác nhau mà không kiểm.

**Cả hai tiền đề đều sai.**

| Tiền đề | Thực tế tại `fbe6edb` |
| --- | --- |
| *"đã soạn xong, chỉ còn việc gửi"* | `W-0297` **đã xoá cả `6` file** khi dồn `25` tài liệu vào một index. Còn `1` file trong cây, và đó là phiếu **đã gửi rồi** |
| *"gửi đi là xong"* | `5/6` phiếu gửi cho **Platform · Security · Legal** — ba phòng ban **không tồn tại như đội riêng** |

Vế thứ hai là chỗ tệ nhất: **đó chính là lập luận tôi dùng để bác `A1`** của bản `16/09`
(*"không có đội Security/Platform nào trong [công ty] này để mà đợi họ ký"*). Tôi bác nó ở một ô, rồi
quay sang dựa vào nó ở ô khác.

Và `m8-13` mục `1` đã cấm sẵn từ `03/09`: *"không gửi tới một mailbox chung nếu không xác định được
owner có thẩm quyền."*

**Kết quả:** `6` phiếu → **`2` phiếu gửi được** + `3` phiếu VieNeu gom về Sếp + `1` phiếu đã gửi
đang chờ.

---

## 1. Tiền lệ xử lý đã có sẵn, không phải tôi nghĩ ra

`OD-V1-11` đóng ngày `2026-09-10`:

> *"Owner tuyên bố quorum là chính mình. [Công ty] này **không có đội Legal/Privacy** và owner chọn
> **không** mua ý kiến pháp lý ngoài cho V1; owner nhận rủi ro pháp lý của nội dung đã ký."*

Đó là cách đúng để xử lý một phiếu mồ côi: **không treo nó chờ một phòng ban tưởng tượng**, mà đưa
về người có thẩm quyền thật và ghi thành văn bản. `W-0309` làm y như thế cho `5` phiếu còn lại.

---

## 2. Khôi phục `6` phiếu từ lịch sử git

Cả `6` bị xoá trong cùng một commit. Toàn văn lấy từ `257cbef^`:

| Phiếu | Dòng |
| --- | ---: |
| `questions-to-platform-w0122-infrastructure` | `258` |
| `questions-to-legal-od-voice-07` | `233` |
| `questions-to-security-w0122-cve-disposition` | `181` |
| `questions-to-platform-key-source-and-release-identity-2026-09-15` | `112` |
| `questions-to-module-3-call-limit-2026-09-15` | `109` |
| `questions-to-platform-ci-and-staging-2026-09-12` | `89` |
| **Tổng** | **`982`** |

Đọc hết `982` dòng trước khi quyết định phiếu nào sống, phiếu nào chết. **Không phiếu nào bị đóng vì
"trông có vẻ cũ"** — mỗi lần đóng đều có bằng chứng trong mã hoặc cấu hình.

---

## 3. Ba phiếu VieNeu — gom về Sếp, không tạo lại

`platform-w0122` (máy chạy model, kho bản cài nội bộ), `security-w0122-cve` (`16` finding
`HIGH`/`CRITICAL` của base image `ivr-tts`) và `legal-od-voice-07` (quyền dùng model và `12` đoạn đã
render) đều hỏi về **VieNeu-TTS tự host** — bộ đọc duy nhất của hệ thống (`OD-V1-19`, `S4` ngày
`17/09`). Cả ba vẫn còn đối tượng để hỏi, nhưng không có phòng ban nào để gửi.

| Phiếu | Câu hỏi còn sống nay ở đâu |
| --- | --- |
| `platform-w0122` | `S5` — máy chạy thật + kho bản cài nội bộ |
| `security-w0122-cve` | `S2` rủi ro `3` — thử đổi bản cài nền trước khi ký |
| `legal-od-voice-07` | `S2` rủi ro `4` — quyền dùng thương mại model VieNeu và `12` đoạn đã render |

*Sửa `18/09` (`W-0315`): bản đầu của mục này đóng hai phiếu với lý do nhánh VieNeu không lên
production. Owner đã chốt ngược lại ngày `17/09`; mục này nay ghi đúng quyết định đó.*

---

## 4. Một phiếu cho Sếp thay cho ba phiếu mồ côi

`plan/ivr-orther/phieu-quyet-dinh-cho-sep-2026-09-17.md` — `4` mục, **không thuật ngữ kỹ thuật nào
cần tra**, mỗi mục có *cần gì · để làm gì · nếu không có thì sao*.

| Mục | Loại | Gốc từ phiếu |
| --- | --- | --- |
| `1` Chỗ chạy thử (cụm · DB riêng · nơi giữ khoá · tên miền+TLS · bảng theo dõi) | 💰 Tiền | `platform-ci-staging` §B |
| `2` Nguồn khoá mở số điện thoại (chọn dịch vụ · xoay khoá · mất khoá thì sao) | 💰 Tiền | `platform-key-source` §A |
| `3` Một tài khoản duyệt thứ hai | 👤 Người | `platform-ci-staging` §A4 |
| `4` Quyền dùng VieNeu | ⚖️ Rủi ro | `legal-od-voice-07` |

**Mục `3` đáng nói riêng.** Ràng buộc bốn mắt là **tôi tự đặt và tự ép trong code**, bảo vệ đúng hai
thao tác nguy hiểm nhất: tắt công tắc dừng khẩn và mở rộng danh sách số được phép gọi. Bất đối xứng
theo `W-0068`: **bật** kill switch cần một người (chiều giảm rủi ro), **tắt** cần hai. Tôi **không
tự gỡ**, dù gỡ được — nên một mình tôi thì vế thứ hai không thực hiện được. *Chữ ký không tạo ra
người.*

---

## 5. Một phiếu khôi phục nguyên văn gửi M3

`questions-to-module-3-call-limit-2026-09-15.md` — phiếu nhóm A **duy nhất có người nhận thật**.

Đã kiểm lại tiền đề trước khi khôi phục: hai lịch nêu trong phiếu vẫn khớp mã tại `fbe6edb` —
`AttemptPolicyRegistries.cs` giữ `GOLDEN_HOUR` `0s`/`150s` và `TWENTY_FOUR_SEVEN` `0s`/`450s`. Nên
cả `3` câu còn nguyên hiệu lực. **Nội dung không sửa một chữ**; chỉ thêm khối ghi chú khôi phục ở đầu.

---

## 6. Sửa sổ sách ở `4` chỗ

| File | Sửa gì |
| --- | --- |
| `00-CHUA-XONG.md` nhóm A | Viết lại toàn bộ: bảng định tuyến `7` phiếu |
| `00-CHUA-XONG.md` `today-03` | *"chờ Platform và Security — hai phiếu"* → câu hỏi về VieNeu chuyển về Sếp |
| `00-CHUA-XONG.md` bảng *"chặn nhiều thứ nhất"* | `5` bên → **`3` bên tồn tại**; gạch bỏ Platform/Security/Legal; sửa câu *"gửi `6` phiếu"* |
| `PHAN-HOI-…-16.md` | Đính chính câu *"`6` phiếu nhóm A chưa gửi"* của chính tôi, nói rõ tôi đã lặp nó ba lần |

---

## 7. Kiểm chứng

| Hạng mục | Kết quả |
| --- | --- |
| Gate sweep | `39/39` |
| `gate-status` | `PASS` |
| Test | **không chạy lại** — lượt này `0` file `.cs`, `0` file test, `0` thay đổi contract |
| `gitnexus_impact` | **không áp dụng** — không symbol nào bị sửa |
| Quét câu sai còn sót | `0` |

**Không mục nào của bản `16/09` đóng nhờ lượt này.** Cái đổi là: `5` phiếu đang treo chờ một phòng
ban không tồn tại nay thành `2` phiếu có người nhận thật, cộng câu hỏi về VieNeu đã gom về Sếp.
