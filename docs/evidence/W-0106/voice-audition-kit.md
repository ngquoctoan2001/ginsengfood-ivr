# W-0106 — Bộ nghe thử giọng nữ 3 miền (ElevenLabs web app, 0đ)

Ngày: `2026-08-22`
Trạng thái: `VOICES_SELECTED_WITHOUT_LISTENING` — owner chốt bỏ qua bước nghe ngày `2026-08-22`
Tài khoản: ElevenLabs **free tier** — 10.000 credits/tháng, dùng trên web app

> Đây là lượt **chọn giọng**, không phải evidence LAB và không phải production.
> Dữ liệu **fake toàn bộ**. `REAL_CUSTOMER_CALL_ALLOWED=NO` không thay đổi.
> Free tier **không có commercial license** — audio sinh ở bước này chỉ để nghe và chọn,
> tuyệt đối không dùng cho cuộc gọi thật. Xem `OD-VOICE-01`.

---

## 1. Vì sao lại là ElevenLabs, không phải free tier khác

Yêu cầu 3 miền loại sạch mọi phương án free:

| Nguồn | Giọng nữ Bắc | **Giọng nữ Trung** | Giọng nữ Nam |
| --- | --- | --- | --- |
| Edge / Azure neural (free 500k ký tự/tháng) | `HoaiMy` | ❌ **không có** | ❌ không có |
| Google Cloud | ✅ | ❌ **không có** | ❌ không có |
| Zalo AI (free) | ✅ | ❌ **không có** | ✅ |
| FPT.AI | `banmai`, `thuminh` | `myan` — owner đánh giá không đạt | `lannhi`, `linhsan` |
| **ElevenLabs** | nhiều | **hàng chục** | nhiều |

Và phương án free tốt nhất **đã được thử và bị từ chối rồi**:
[`voice-modernization-proposal.md`](../W-0104/voice-modernization-proposal.md) §6 ghi A/B
`vi-VN-HoaiMyNeural` + `vi-VN-NamMinhNeural` được dựng, nghe qua MicroSIP và **owner từ chối
cả hai**. Sau đó owner chọn ElevenLabs. Không đi lại vòng đó.

---

## 2. Ngân sách credits

Owner chốt bỏ qua bước nghe (`2026-08-22`), nên chỉ còn **3 lần render** — mỗi miền một
giọng đã chọn, để lấy file audio cho Giai đoạn 4.

| Hạng mục | Ký tự | Credits |
| --- | ---: | ---: |
| Một lần render kịch bản v3 | ~300 | ~300 |
| Zara (Trung) + Thắm (Bắc) + Giang (Nam) | 900 | **900 / 10.000** |

Còn dư ~9.100 credits — thừa sức render lại nếu sau này đổi giọng.

---

## 3. Cấu hình render (áp dụng cho cả 3 giọng)

Giữ **y hệt nhau** ở cả ba miền. Khác cấu hình giữa các miền là ba giọng nghe lệch nhau vì
lý do không ai truy được.

| Tham số | Giá trị | Lý do |
| --- | --- | --- |
| Model | **Eleven v3** | Đúng model đã dùng cho voice C được owner chấp nhận ở W-0104 |
| Language | Auto detect | Như W-0104 |
| `Stability` | **0.35 – 0.50** | Thấp hơn mặc định để có nhấn nhá. Dưới 0.30 giọng trôi giữa các lần render, phá tính lặp lại của evidence |
| `Similarity` | **~0.75** | |
| `Style` | thấp – vừa | Style cao tạo kịch tính giả, nghe như quảng cáo chứ không như gọi xác nhận đơn |
| Speed | **-3% đến -5%** | Chậm hơn mặc định một chút |

Không nhạc nền. Không hiệu ứng.

---

## 4. Kịch bản — dán ĐÚNG bản của từng miền

Ba bản chỉ khác nhau hai chỗ: **`nghìn`/`ngàn`** và **địa chỉ**. Đây chính là đầu ra của
`VietnameseNumberSpeller` đã code ở Giai đoạn 2 — nghe bằng lexicon sai miền là nghe sai
thứ sắp chạy thật.

### 4.1 MIỀN TRUNG — nghe trước tiên

```
Xin chào Quý khách. Đây là cuộc gọi tự động để xác nhận đơn hàng từ Ginsengfood. Quý khách có đơn hàng gồm hai hộp Cháo sâm diêm mạch - hạt sen, tổng tiền năm trăm sáu mươi ngàn đồng, giao đến phường Hải Châu, thành phố Đà Nẵng. Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng.
```

### 4.2 MIỀN BẮC

```
Xin chào Quý khách. Đây là cuộc gọi tự động để xác nhận đơn hàng từ Ginsengfood. Quý khách có đơn hàng gồm hai hộp Cháo sâm diêm mạch - hạt sen, tổng tiền năm trăm sáu mươi nghìn đồng, giao đến phường Cửa Nam, thành phố Hà Nội. Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng.
```

### 4.3 MIỀN NAM

```
Xin chào Quý khách. Đây là cuộc gọi tự động để xác nhận đơn hàng từ Ginsengfood. Quý khách có đơn hàng gồm hai hộp Cháo sâm diêm mạch - hạt sen, tổng tiền năm trăm sáu mươi ngàn đồng, giao đến phường Phú Khương, tỉnh Vĩnh Long. Bấm phím một để xác nhận đơn hàng, hoặc bấm phím không để hủy đơn hàng.
```

---

## 5. Ba giọng đã chốt (kèm chuỗi fallback)

Lọc **Language = Vietnamese**, **Gender = Female**, rồi tìm **theo tên**.

Ba giọng dưới được chọn theo hai tiêu chí nhất quán, không phải bốc ngẫu nhiên:

1. **Giọng an toàn của vùng** — Đà Nẵng cho Trung (không Huế), Sài Gòn cho Nam (không miền
   Tây đặc sệt). Miền Trung trải từ Thanh Hóa tới Khánh Hòa nên giọng phải dễ nghe toàn miền.
2. **Ấm + tự nhiên, KHÔNG phải giọng phát thanh viên.** `Hien` và `Thanh` bị đẩy xuống
   fallback dù "chuyên nghiệp hơn" — giọng bản tin chính là cái "công nghiệp" sếp đã bác.

Nếu một giọng cần đổi, đi thẳng xuống fallback 1 rồi fallback 2, không cần hỏi lại.

⚠️ **Voice ID trong bảng này CHƯA được xác minh.** Chúng lấy từ catalog bên thứ ba, và catalog
đó đã sai ít nhất một lần: nó gán `ueSxRO0nLF1bj93J2hVt` cho một giọng nam miền Bắc tên khác,
trong khi [`manifest.txt`](../../../deploy/lab/asterisk/audio/manifest.txt) của chính repo ghi
ID đó là `Trung Caha`. **Phải copy ID thật từ ElevenLabs app** khi chốt.

### 5.1 Miền Trung

| | Tên | Voice ID (chưa verify) | Mô tả |
| --- | --- | --- | --- |
| ✅ **ĐÃ CHỌN** | **Zara** | `QocxxnxEa0x8mrL2d4VT` | Giọng Đà Nẵng rõ, ấm, tự nhiên, biểu cảm |
| fallback 1 | Huyen | `foH7s9fX31wFFH2yqrFa` | Đà Nẵng, bình tĩnh, thân thiện, rõ |
| fallback 2 | Duyen | `DVQIYWzpAqd5qcoIlirg` | Sáng, rõ, ấm chất Nam Trung Bộ |

> **Chọn giọng Đà Nẵng, không chọn Huế.** Miền Trung trải từ Thanh Hóa tới Khánh Hòa; giọng
> Huế/Quảng Trị rất đặc trưng và khó nghe với người ngoài vùng. Đà Nẵng là "giọng Trung an
> toàn" — dễ hiểu nhất trên toàn miền.

### 5.2 Miền Bắc

| | Tên | Voice ID (chưa verify) | Mô tả |
| --- | --- | --- | --- |
| ✅ **ĐÃ CHỌN** | **Thắm** | `0ggMuQ1r9f9jqBu50nJn` | Trẻ, dịu, ấm, giọng đáng tin |
| fallback 1 | Mai | `d5HVupAWCwe4e6GvMCAL` | Hà Nội, tự nhiên, sáng |
| fallback 2 | Hien | `jdlxsPOZOHdGEfcItXVu` | Phát thanh viên chuyên nghiệp, Hà Nội |

> **Ưu tiên Thắm hơn Hien.** Giọng phát thanh viên đúng là chuẩn, nhưng đó chính xác là cái
> "công nghiệp" sếp không muốn. Cuộc gọi xác nhận đơn cần cảm giác *người thật gọi cho mình*,
> không phải bản tin thời sự.

### 5.3 Miền Nam

| | Tên | Voice ID (chưa verify) | Mô tả |
| --- | --- | --- | --- |
| ✅ **ĐÃ CHỌN** | **Giang** | `X0V9HEDEuaVhVqzVPUKM` | Sài Gòn, ấm, tự tin |
| fallback 1 | HTN | `s06eec3OqspIDuOznMK4` | Bình tĩnh, ấm, tự nhiên |
| fallback 2 | Thanh | `N0Z0aL8qHhzwUHwRBcVo` | Giọng Sài Gòn chất lượng cao |

> **Chọn giọng Sài Gòn, không chọn miền Tây đặc sệt.** Miền Nam gồm cả Đồng bằng sông Cửu
> Long; giọng Sài Gòn trung tính phủ tốt cả vùng.

---

## 6. Bước nghe — ĐÃ HOÃN

Owner chốt ngày `2026-08-22`: **không nghe trước, chốt luôn ba giọng ở §5, đổi sau nếu cần.**

Đổi giọng rẻ vì thiết kế cho phép: voice nằm trong config, đổi = sửa
`Ivr:Speech:Tts:RegionalVoices` + thay file PCM. **Không sửa code, không migration.**

Hai hệ quả được ghi lại, không phải để cản mà để không ai bất ngờ sau này:

1. **Đổi giọng ở LAB đắt hơn đổi config.** File PCM được ghim SHA-256 trong image Asterisk,
   nên đổi giọng = render lại + hash lại + build lại image + chụp lại evidence MicroSIP. Đây
   đúng là chuyện đã xảy ra ở W-0104: A/B bị từ chối sau khi đã có evidence, phải làm lại từ đầu với voice C.
2. **W-0106 không thể đạt `ACCEPTED` nếu thiếu chữ ký của sếp.** Tiền lệ W-0104 là owner nghe
   qua MicroSIP rồi mới ghi `ACCEPTED`. Bỏ bước nghe ⇒ trạng thái cao nhất W-0106 đạt được là
   `TESTS_PASS`. Đây là ghi nhận trạng thái đúng sự thật, không phải một cổng chặn thêm.

Bảng chấm dưới giữ lại để dùng khi nào sếp nghe — dù là bây giờ hay sau khi đã chạy lab.

Mỗi giọng chấm 5 mục. Giọng nào có mục nào `KHÔNG` thì loại, không cần cân nhắc thêm.

| # | Tiêu chí | Vì sao quan trọng |
| --- | --- | --- |
| 1 | **"phím một"** và **"phím không"** nghe rõ, tách bạch | Đây là chỗ khách phải hành động. Nghe nhầm là sai disposition |
| 2 | Số tiền đọc liền mạch, không vụn | Khách đang được hỏi để xác nhận đúng con số này |
| 3 | Tên địa danh đọc đúng | `Phú Khương`, `Hải Châu`, `Cửa Nam` |
| 4 | `Cháo sâm diêm mạch - hạt sen` — dấu gạch ngang không làm ngắt kỳ cục | |
| 5 | **Không "công nghiệp"**, không "thuần AI" | Tiêu chí gốc của sếp |

### Bảng chấm

| Miền | Giọng | 1 | 2 | 3 | 4 | 5 | GIỮ? |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Trung | **Zara** | | | | | | |
| Bắc | **Thắm** | | | | | | |
| Nam | **Giang** | | | | | | |

Giọng nào trượt thì thay bằng fallback 1 ở §5, render lại, đổi config + file PCM.

---

## 7. Sau khi chọn xong — ghi lại đủ 5 thứ

Thiếu bất kỳ mục nào thì Giai đoạn 4 không chạy được:

1. **Tên giọng** đúng như hiển thị trong ElevenLabs
2. **Voice ID thật**, copy từ app — không lấy từ bảng §5
3. **Model + settings** đã dùng (v3, stability, similarity, style, speed)
4. **SHA-256 của file MP3** nguồn
5. **Ngày và tài khoản** đã sinh

MP3 nguồn **để ngoài repo** theo tiền lệ W-0104. Repo chỉ chứa PCM 8 kHz đã ghim checksum.

---

## 8. Ranh giới

Free tier ElevenLabs **không có commercial license**. Bước này chỉ chứng minh *chất lượng
giọng*, không cấp quyền dùng cho cuộc gọi thật. Trước khi lên production vẫn phải đóng
`OD-VOICE-01`: mua gói có commercial license, xác nhận điều khoản còn hiệu lực với audio đã
sinh, DPA/privacy, data residency, và fallback khi voice ID biến mất khỏi Voice Library.
