# W-0341 — Rà quyền sử dụng VieNeu, MOSS và ba giọng đã chọn

Ngày 22/09/2026. **Đã tìm và xác minh công bố giấy phép tại đúng phiên bản đang dùng.**
**Tiếp nối:** [W-0342](../W-0342/README.md) đã đưa các bằng chứng này vào khóa và hai verifier,
kiểm 36 ca âm mỗi phía, bảo toàn hồ sơ nghe và giữ production bị chặn.
**REAL_CUSTOMER_CALL_ALLOWED=NO; production BLOCKED.** Đây là rà bằng chứng công bố,
không phải quyết định bật production hoặc thay phê duyệt Legal/Privacy của dự án.

## Kết luận và đính chính

Kết luận trước ở W-0340 rằng “chưa có chứng từ quyền dùng model/codec” là quá rộng.
Việc chỉ kiểm metadata và tìm tên file LICENSE đã bỏ sót FAQ đầy đủ trong chính model card
VieNeu đã đóng gói. Hai model card vừa tải lại từ nguồn chính thức **khớp từng byte** với
bundle S5 và hash trong `MODELS.lock`. Đây không phải giấy phép lấy từ bản mới để áp cho bản cũ.

**Có căn cứ công bố cho việc dùng thương mại hai thành phần đang chọn.** Không tìm thấy
yêu cầu mua thêm giấy phép hoặc xin thư riêng cho cách dùng các preset này để tạo lời thoại.
Không có file tên LICENSE riêng không đồng nghĩa với không có công bố cấp phép.
Nhà mạng không phải nguồn cấp quyền cho hai model do dự án tự cài này.

Phiếu S2 đã được Nguyễn Quốc Toàn chấp thuận vẫn được giữ nguyên làm lịch sử quyết định.
Đây là bổ sung/đính chính bằng chứng cho điểm 4, không thay lời người ký hoặc yêu cầu ký lại bảy điểm.

## Nguồn và phạm vi quyền

| Thành phần đang dùng | Nguồn chính thức đã ghim | Nội dung xác minh |
| --- | --- | --- |
| VieNeu v3 Turbo ONNX int8 | [README tại revision 2da0efab622a](https://huggingface.co/pnnbao-ump/VieNeu-TTS-v3-Turbo/blob/2da0efab622a1722125991736524f080b751ef5b/README.md) | Apache-2.0 áp dụng weights, ONNX, config/tokenizer và preset assets. FAQ cho phép thương mại preset và audio đầu ra, không thêm phí/giấy phép; nhà phát hành xác nhận đã có quyền và sự đồng ý của người cung cấp giọng |
| MOSS Audio Tokenizer Nano ONNX | [README tại revision ceff0d0749bf](https://huggingface.co/OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX/blob/ceff0d0749bfb3fa2d61149794ec6feef0d1e1ae/README.md) | Nhà phát hành khai Apache-2.0 ngay trong model card của bản ONNX; mô tả cả decoder graph và shared weights đang dùng. Không thấy điều khoản hạn chế chỉ nghiên cứu/phi thương mại trong card này |
| Source VieNeu và bộ preset SDK 3.3.0 | [LICENSE tại commit 36c4b501b063](https://github.com/pnnbao97/VieNeu-TTS/blob/36c4b501b0634a8f59805e6b529a058fbd30190b/LICENSE) | Apache-2.0; LICENSE và JSON preset tải tại commit này khớp từng byte bản vendored |

Ba giọng IVR ánh xạ đúng preset có sẵn trong `voices_v3_turbo.json` của SDK đã ghim:

| ID IVR | Preset | Kết quả |
| --- | --- | --- |
| `v3t-north-ngoc-linh` | Ngọc Linh | Có trong bộ 20 preset đã kiểm hash |
| `v3t-central-ngoc-tran` | Ngọc Trân | Có trong cùng bộ preset |
| `v3t-south-my-duyen` | Mỹ Duyên | Có trong cùng bộ preset |

Nhận định áp dụng: lời thoại cố định và phần sinh theo món bằng ba preset trên đều thuộc
phạm vi audio thương mại được VieNeu công bố. Không suy quyền đó sang giọng clone từ clip ngoài.
Sự đồng ý của người cung cấp giọng là **tuyên bố của nhà phát hành**; dự án không nắm hợp đồng
cá nhân hoặc toàn bộ hồ sơ dữ liệu huấn luyện. FAQ nói rõ quy trình dữ liệu không được công khai.
Đó là giới hạn kiểm chứng, không phải bằng chứng rằng chưa có giấy phép sử dụng model.

Đối chiếu bổ sung MOSS: model PyTorch gốc ở revision `6aa02b01e445` khai Apache-2.0;
[source MOSS tại commit 8c50ac4c5d72](https://github.com/OpenMOSS/MOSS-Audio-Tokenizer/blob/8c50ac4c5d7287d2ed6ea20a08c90ca439887d23/LICENSE)
có LICENSE Apache-2.0, ghi chủ sở hữu OpenMOSS Team, Fudan University, SII và MOSI.
Hai nguồn bổ sung được chốt lúc rà, **không thay revision ONNX đang dùng**. Bằng chứng chính
cho bản ONNX vẫn là công bố tại chính revision `ceff0d0749bf`.

## Nghĩa vụ khi đóng gói và phân phối

Theo [Apache License 2.0, mục 2–4 và 6–8](https://www.apache.org/licenses/LICENSE-2.0):

- Giấy phép cho quyền sử dụng/phân phối theo điều kiện, không hạn chế chỉ phi thương mại.
- Khi phân phối bản sao/model hoặc bản sửa, kèm bản giấy phép; giữ thông báo bản quyền,
  ghi công và NOTICE nếu có; ghi rõ file đã sửa. VieNeu cũng yêu cầu giữ ghi công source và gói HF.
- Không suy ra quyền dùng nhãn hiệu, bảo đảm không tranh chấp hoặc trách nhiệm bồi thường từ tác giả.

Đã lưu nguyên văn giấy phép từ Apache và thông báo gốc để dùng cho gói phát hành tiếp theo.
Không đổi tên giấy phép của source thành “LICENSE của weights”; manifest phân biệt công bố
của từng model với văn bản Apache được dẫn chiếu. Gói mirror W-0340 vẫn giữ nguyên hash;
hai model card có công bố nói trên đã nằm trong 39 file được kiểm trên vps61.

## Vì sao bộ kiểm tra vẫn chặn

Tại thời điểm rà W-0341, `deploy/tts/scripts/verify-model.py` ở chế độ production đòi mỗi artifact có
`license_file_sha256` khác null và quyết định `LEGAL_PRIVACY` với người/ngày/tham chiếu.
Đây là quy tắc triển khai nội bộ. Công bố cấp phép được tìm thấy không tự thay các trường đó.
`MODELS.lock` lúc đó còn lý do “preset commercial-use unresolved”; W-0341 xác định lý do đó đã lỗi thời.
W-0342 đã sửa cơ chế này; mô tả ở đoạn này được giữ làm lịch sử phát hiện.

Bước kỹ thuật tiếp theo là ghi nhận bằng chứng đúng nguồn trong hồ sơ/provenance: liên kết
model card chính xác + Apache được dẫn chiếu + preset/source + receipt mirror đã kiểm.
Rà cách biểu diễn bằng chứng trong schema/verifier và ghi phê duyệt đúng thẩm quyền;
không điền hash giả, không bỏ bước kiểm hash và không mở production chỉ nhờ rà giấy phép.
Nếu cần đổi lock/image, đó là ứng viên mới cần chuỗi kiểm chứng tương ứng, giữ quota/profile hiện tại.

## Bằng chứng và kiểm tra

- [source-verification.json](source-verification.json): 12 tài liệu/API gốc, URL, thời điểm,
  revision và SHA-256; hai card khớp lock và bundle; source LICENSE/preset khớp vendored.
- [review-verification.json](review-verification.json): xác minh lại hash 12 file, ba ánh xạ
  preset và card trong catalog mirror; các file scan/S2/receipt đã ràng hash được giữ nguyên.
- Raw: `.artifacts/W-0341/upstream/`; script thu: `.artifacts/W-0341/collect-rights-evidence.py`.
- Không chạy lại model, tải trọng hoặc nghe thử; lượt này chỉ tài liệu và kiểm nguồn công khai.

**Việc đã làm:** xác minh quyền công bố đúng revision, sửa nhận định thiếu chứng từ quá rộng.
**Đề xuất bước tiếp theo:** đồng bộ hồ sơ và cách kiểm bằng chứng giấy phép vào ứng viên phát hành;
giữ S2 đã nhận rủi ro, quota/profile và trạng thái khách thật tắt.
