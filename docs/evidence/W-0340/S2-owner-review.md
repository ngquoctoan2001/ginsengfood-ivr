# S2 — Phiếu rà soát và nhận rủi ro bản đầu

Ngày chuẩn bị và xác nhận: 22/09/2026. **OWNER_RISK_ACCEPTED_WITH_CONDITIONS. Khách thật tiếp tục tắt.**
Người chấp thuận: **Nguyễn Quốc Toàn**.
Vai trò do người dùng cung cấp: **“Tôi là người duy nhất build module 8”**.
Thẩm quyền đại diện công ty nhận rủi ro: **người ký đã xác nhận trực tiếp trong task**.

## Bảy điểm cần duyệt

| # | Nội dung để người ký quyết định | Bằng chứng/tình trạng hiện tại |
| --- | --- | --- |
| 1 | IVR lưu số điện thoại khách ở dạng đọc được; người lấy được bản sao dữ liệu có thể đọc số | Phương án B đã chọn. Việc ký không thay đổi cách lưu hoặc quyền truy cập |
| 2 | Giữ dữ liệu khách lâu dài theo quyết định giữ vĩnh viễn; cần thực hiện xoá khi khách yêu cầu | Đã có CLI DSAR ở W-0330; yêu cầu bảo vệ và xử lý dữ liệu vẫn thuộc trách nhiệm vận hành |
| 3 | Rủi ro lỗ hổng và phụ thuộc nền chạy | Đã chọn Chainguard cho VieNeu. Quét đúng image bằng DB mới: TTS không phát hiện CVE; worker 0 HIGH/CRITICAL, còn 11 MEDIUM + 6 LOW theo cặp package/CVE (16 mục có bản sửa). Không ký theo danh sách 44 HIGH của nền Debian cũ |
| 4 | Quyền dùng thương mại model/codec, ba giọng đã chọn, 12 đoạn cố định và nội dung âm thanh sinh theo đơn thật | Model card ghi Apache-2.0; cả hai revision model/codec không có LICENSE riêng. Chưa có chứng từ đủ để đóng gate quyền sử dụng. Ký nhận rủi ro không tự cấp quyền từ chủ sở hữu |
| 5 | Người nhấc máy có thể không phải chủ đơn nhưng vẫn nghe tên món và khu vực giao | Đây là giới hạn của xác nhận qua cuộc gọi; không chứng minh danh tính người nghe |
| 6 | Bản đầu chưa có lựa chọn “đừng gọi tôi nữa” trong cuộc gọi; phím 0 hiện là huỷ đơn | Không diễn giải phím 0 thành yêu cầu ngừng mọi cuộc gọi sau này |
| 7 | Không ghi âm; khi có tranh chấp bấm phím, bằng chứng là nhật ký DTMF và kết quả hệ thống | Không cam kết có bản ghi âm để đối chứng |

Nội dung 1/2/5/6/7 giữ phạm vi từ [phiếu quyết định gốc](../../../plan/ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md).
Không đưa ra kết luận tuân thủ pháp luật từ phiếu kỹ thuật này.

## Phạm vi image và model đã kiểm

- VieNeu image `79e9106ec140`, worker `1d4f05d5e284`; full digest, config và hash archive
  nằm trong [verification.json](verification.json).
- Trivy 0.73.0; DB cập nhật 21/09/2026 lúc 19:11 UTC, tải 22/09 lúc 02:21 UTC.
  Kết quả là thời điểm quét, không bảo đảm không còn lỗ hổng chưa biết.
- TTS giữ 2 CPU/4 GiB, capacity 1, ONNX 1; profile 30/90/120 giây. Không đổi model/giọng.
- Nguồn code VieNeu có [LICENSE ở commit đã ghim](https://github.com/pnnbao97/VieNeu-TTS/blob/36c4b501b0634a8f59805e6b529a058fbd30190b/LICENSE).
  Đây là nguồn code; không dùng file đó thay cho bằng chứng của weights/codec.
- Kiểm trực tiếp metadata và toàn danh sách file của hai revision:
  [VieNeu v3 Turbo](https://huggingface.co/api/models/pnnbao-ump/VieNeu-TTS-v3-Turbo/revision/2da0efab622a1722125991736524f080b751ef5b),
  [MOSS Nano ONNX](https://huggingface.co/api/models/OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX/revision/ceff0d0749bfb3fa2d61149794ec6feef0d1e1ae).
  Cả hai trả `license=apache-2.0`, không có file LICENSE/LICENCE/COPYING/NOTICE.

## Quyết định đã nhận

Nguồn: câu trả lời trực tiếp của Nguyễn Quốc Toàn trong task này ngày 22/09/2026,
được ghi nhận lúc 02:32 UTC (09:32 giờ Việt Nam):

> Tôi có thẩm quyền và chấp thuận đủ 7 điểm, giữ các điều kiện đã ghi

Đây là xác nhận qua hội thoại của người ký, không phải chữ ký số hoặc chứng từ từ tác giả model.

- [x] Tôi xác nhận có thẩm quyền đại diện công ty nhận rủi ro cho bản đầu Module 8.
- [x] Tôi đã đọc và chấp thuận các điểm 1–7 theo đúng tình trạng, giới hạn và điều kiện ở phiếu này.
- [ ] Tôi yêu cầu thay đổi/tạm hoãn các điểm: ______________________________.

Tên người ký: **Nguyễn Quốc Toàn** · Thẩm quyền: **đại diện công ty nhận rủi ro, tự xác nhận**.
Ngày xác nhận: **22/09/2026** · Tham chiếu: **W-0340, câu trả lời hội thoại trích nguyên văn phía trên**.

**Điều kiện giữ nguyên sau khi ký nhận rủi ro:** chưa bật production hoặc khách thật;
quyền model/codec/giọng vẫn cần chứng từ được chấp thuận cho đúng revision. `MODELS.lock`
hiện yêu cầu bằng chứng license-file và phê duyệt có người/ngày/tham chiếu; các ô này không
được tự điền bằng file license của thành phần khác. Gate không được nới để thay cho chứng từ.

Bước lấy chứng từ: người có thẩm quyền liên hệ nguồn model/codec, nêu đúng hai revision,
ba preset giọng và mục đích gọi xác nhận đơn hàng thương mại; xin file/điều khoản hoặc xác
nhận áp dụng cho weights/codec/preset và audio đầu ra. Lưu bản gốc + hash để rà và ghi gate.
Phiên này chưa gửi thư hoặc liên hệ thay công ty.
