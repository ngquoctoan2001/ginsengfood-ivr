# R-03 — Gói production nhiều kênh eSIM

External work `W-0008` · quyết định `OD-V1-10` · gate `G-ESIM32` · trạng thái `OPEN`

Owner: **Infra** (kiến trúc kênh), **Procurement** (mua sắm, hợp đồng).

Due: chốt **trước release gate `P9-1`**. Ngày cam kết của owner: `<owner điền>`.

## 1. Con số 32 là mục tiêu, không phải kết luận

Tên gate là `G-ESIM32` vì `32` là **mục tiêu ban đầu** ghi trong tracker. Nó chưa được chứng minh là đủ, chưa được chứng minh là cần, và **số kênh cho pilot chưa được quyết định ở bất kỳ đâu**.

Mục này tồn tại để thay một con số bằng một phép tính có đầu vào thật.

## 2. Mô hình nhu cầu — công thức, chưa phải kết quả

Số kênh cần thiết bị chi phối bởi **đỉnh đồng thời**, không phải tổng số cuộc gọi/ngày. Golden Hour ép mọi cuộc vào một cửa sổ 5 phút, nên đỉnh rất nhọn.

```text
kenh_can = ceil( don_dinh_trong_cua_so
                 × lan_goi_trung_binh_moi_don
                 × thoi_luong_mot_cuoc_ke_ca_cooldown
                 / do_dai_cua_so
                 × he_so_du_phong )
```

Đầu vào bắt buộc, chưa có cái nào:

| Đầu vào | Nguồn | Giá trị |
| --- | --- | --- |
| Số đơn Golden Hour trong 1 cửa sổ 5 phút, ở giờ cao điểm | Business/Sales | `<điền>` |
| Số đơn 24/7 trong 1 cửa sổ 15 phút, ở giờ cao điểm | Business/Sales | `<điền>` |
| Số lần gọi trung bình mỗi đơn (phụ thuộc tỉ lệ nghe máy) | đo ở pilot | `<điền>` |
| Thời lượng một cuộc hoàn chỉnh, gồm đổ chuông + thoại + chờ phím | [lab §5](lab-acceptance-report-template.md) | `<điền>` |
| Cooldown tối thiểu giữa hai cuộc trên cùng kênh | [lab §5](lab-acceptance-report-template.md) | `<điền>` |
| Hệ số dự phòng cho kênh hỏng/quarantine | Infra | `<điền>` |

**Ba đầu vào đầu tiên là câu hỏi business, không phải câu hỏi kỹ thuật.** Không có chúng thì mọi con số kênh đều là phỏng đoán — kể cả `32`.

Ràng buộc cứng: **một SIM tại một thời điểm chỉ mang một cuộc gọi** (`ONE_SIM_ONE_ACTIVE_CALL`). Nếu nhà cung cấp phá được giả định này ([R-01](R-01-vendor-requirements.md) §8), toàn bộ mô hình đổi.

## 3. Vòng đời kênh và cấp phát

| Hạng mục | Câu hỏi | Trả lời |
| --- | --- | --- |
| Cấp phát | Thêm một eSIM mất bao lâu; tự phục vụ qua API hay phải yêu cầu | `<vendor điền>` |
| Định danh | `sim_channel_id` có ổn định qua lần cấp lại không | `<vendor điền>` |
| Thu hồi | Gỡ một kênh mất bao lâu; cước tính đến khi nào | `<vendor điền>` |
| Số thuê bao | Mỗi eSIM một số riêng, hay chia sẻ được số hiển thị | `<vendor điền>` |
| Giới hạn | Tối đa bao nhiêu eSIM trên một tài khoản/thiết bị | `<vendor điền>` |
| Địa lý | eSIM gắn với vùng nào; chuyển vùng có ảnh hưởng cước không | `<vendor điền>` |

Cấp phát tự phục vụ được hay không quyết định việc mở rộng theo mùa vụ có khả thi hay chỉ là mua đứt một lần.

## 4. Quota và giới hạn

| Hạng mục | Câu hỏi | Trả lời |
| --- | --- | --- |
| Trên mỗi SIM | Cuộc/phút, cuộc/giờ, cuộc/ngày, phút/tháng | `<vendor điền>` |
| Trên tài khoản | Tổng đồng thời tối đa | `<vendor điền>` |
| Nhà mạng | Nhà mạng có chặn số gọi ra nhiều không; ngưỡng bao nhiêu | `<vendor điền>` |
| Vượt hạn | Vượt quota thì bị từ chối, bị bóp, hay bị tính cước phụ | `<vendor điền>` |
| Cảnh báo | Có thông báo trước khi chạm hạn không | `<vendor điền>` |

Ngưỡng chặn của nhà mạng là rủi ro thật cho mô hình gọi ra tự động: một kênh bị nhà mạng khoá giữa giờ cao điểm không có cách khôi phục nhanh, và thường không có cảnh báo trước.

## 5. Pooling, failover, quarantine

IVR đã có sẵn mô hình trạng thái kênh và đang phơi qua `GET /sim-channels`:

| Trường | Ý nghĩa |
| --- | --- |
| `enabled` | operator bật/tắt thủ công |
| `busy` | đang mang cuộc gọi; tắt sẽ có hiệu lực sau khi cuộc kết thúc |
| `fail_count` | đếm lỗi liên tiếp; `≥ 3` → quarantine (`DT-04`) |
| `quarantined` / `quarantine_until` | tự cách ly |
| `cooldown_until` | nghỉ giữa hai cuộc (mặc định 5 giây) |
| `last_health_check_at` | lần kiểm tra sức khoẻ gần nhất |
| `disabled_reason` | vì sao bị tắt |

Chọn kênh dùng **lease token + fencing generation** — một cuộc gọi cũ không thể sống lại và chiếm kênh sau khi lease đã chuyển sang cuộc khác.

Câu hỏi cần trả lời trước production:

| # | Câu hỏi | Trả lời |
| --- | --- | --- |
| 1 | Quarantine bao lâu thì thử lại; ai quyết định thả ra | `<điền>` |
| 2 | Bao nhiêu kênh quarantine cùng lúc thì coi là sự cố hệ thống chứ không phải lỗi lẻ | `<điền>` |
| 3 | Hết kênh khả dụng giữa cửa sổ Golden Hour thì làm gì — xếp hàng, bỏ, hay báo Sales | `<điền>` |
| 4 | Có cần dự phòng nhà cung cấp thứ hai không | `<điền>` |
| 5 | Kênh phân bổ theo nhà mạng của khách được không, hay quay ngẫu nhiên | `<điền>` |

Câu 3 là quyết định business: bỏ một đơn không gọi được **im lặng** là hành vi tệ nhất, nhưng xếp hàng quá cửa sổ thì cuộc gọi thành vô nghĩa.

## 6. Throughput và độ trễ — phải đo, không suy

| Chỉ số | Phải đo ở đâu | Giá trị |
| --- | --- | --- |
| Cuộc gọi đồng thời tối đa thật sự | môi trường staging với **số kênh thật đã mua** | `<điền>` |
| Độ trễ `dial` → đổ chuông (p50/p95/p99) | như trên | `<điền>` |
| Tỉ lệ lỗi ở tải đỉnh | như trên | `<điền>` |
| Thời gian phục hồi sau khi một kênh chết | như trên | `<điền>` |
| Suy giảm khi 25% / 50% kênh quarantine | như trên | `<điền>` |

**Không suy ra từ 1 SIM lab, không suy ra từ simulator.** Load simulator hiện có chỉ mô hình hoá phía IVR — nó không chạm nhà mạng, không chạm thiết bị, không tạo ra được hành vi nghẽn thật. Đây là ranh giới mà `P11-1` §3 nêu thẳng: *"Do not infer 32-channel readiness from simulator or one-SIM results."*

## 7. Chi phí

| Thành phần | Đơn vị | Giá trị |
| --- | --- | --- |
| Thiết bị / gateway | một lần | `<điền>` |
| Thuê bao mỗi eSIM | tháng | `<điền>` |
| Cước gọi | phút hoặc block | `<điền>` |
| TTS | xem [R-05](R-05-tts-audio-capability.md) | `<điền>` |
| Hạ tầng chạy adapter | tháng | `<điền>` |
| Hỗ trợ / SLA | tháng | `<điền>` |

Chi phí mỗi đơn được xác nhận:

```text
chi_phi_moi_don = (cuoc_goi_trung_binh_moi_don × thoi_luong_trung_binh × cuoc_moi_phut)
                  + (tts_moi_don)
                  + (chi_phi_co_dinh_thang / so_don_thang)
```

Con số này là thứ quyết định dự án có đáng làm không, và hiện **chưa tính được** vì thiếu cả `cuoc_goi_trung_binh_moi_don` (đo ở pilot) lẫn đơn giá (chưa có báo giá).

## 8. Observability

Đo được ở production cần, ở mức tổng hợp — **không** log số, không log nội dung thoại:

| Nhóm | Chỉ số |
| --- | --- |
| Kênh | số kênh `enabled` / `busy` / `quarantined`, theo thời gian |
| Cuộc gọi | tỉ lệ theo từng disposition trong 11 giá trị |
| Độ trễ | `dial` → đổ chuông, thời lượng cuộc, thời gian chờ kênh rảnh |
| Hàng đợi | số task chờ kênh, thời gian chờ lâu nhất |
| Lỗi | `fail_count` theo kênh, số lần quarantine, số lần kill switch kích hoạt |
| Cước | phút đã dùng so với hạn mức, theo kênh |

Cảnh báo tối thiểu: hết kênh khả dụng; >25% kênh quarantine; tỉ lệ `TECHNICAL_EXCEPTION` vượt ngưỡng; sắp chạm quota nhà cung cấp.

Backend observability là phụ thuộc `W-0063`, hiện `BLOCKED_EXTERNAL`. Chi tiết thuộc `P6-1`/`P6-2`; mục này chỉ ghi **cần đo gì từ phía telephony**.

## 9. Chế độ thảm hoạ

| Tình huống | Hành vi mong muốn | Đã có? |
| --- | --- | --- |
| Nhà cung cấp chết hoàn toàn | IVR fail-closed, báo Sales, **không** đánh dấu đơn là đã gọi | thiết kế có; chưa diễn tập |
| Mất một nửa số kênh | Ưu tiên Golden Hour trước 24/7 | `<điền — chưa có quy tắc ưu tiên>` |
| Kill switch toàn cục | Dừng mọi cuộc mới trong bao lâu | có flag; chưa đo thời gian có hiệu lực |
| Cần chuyển nhà cung cấp khẩn | Mất bao lâu | `<điền>` |
| Vượt cước bất thường | Ai được phép dừng, dừng bằng gì | `<điền>` |

Diễn tập các tình huống này thuộc `P6-3` (chaos/gameday). Mục này ghi lại **yêu cầu** để `P6-3` có cái mà diễn tập.

## 10. Closure artifact

`OD-V1-10` và phần production của `W-0008` chỉ đóng khi có:

- [ ] **Mô hình nhu cầu §2 đã điền đủ đầu vào**, có số đơn đỉnh từ business.
- [ ] **Hợp đồng/PO đã ký** cho số kênh tính ra từ §2 — không phải con số mặc định.
- [ ] **Báo cáo đo tải §6** chạy trên **số kênh thật đã mua**, ở staging.
- [ ] **Bằng chứng failover** cho ít nhất tình huống "mất một nửa số kênh" ở §9.
- [ ] **Bảng chi phí §7 đã điền**, có chi phí mỗi đơn.

Báo giá **không** đóng gate. Kết quả lab 1 SIM **không** đóng gate. Simulator **không** đóng gate.
