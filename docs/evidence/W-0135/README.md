# W-0135 — Sửa sự thật trong hồ sơ procurement

Ngày: `2026-08-28`
Baseline: `main@a6ac830`
Trạng thái: `TESTS_PASS`
Quyền gọi khách thật: `REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Mục tiêu

Cross-audit Gate 1 (F-07/F-08/F-09) chỉ ra ba lỗi trong bộ tài liệu **sẽ được dùng để duyệt chi
tiền**. W-0135 sửa chúng. **Docs-only**, không đụng code.

W-0135 **không** tự chốt số kênh hay attempt policy. Nó chỉ gỡ fact sai và trả các con số chưa ký
về đúng trạng thái chưa ký.

## 2. F-09 — mốc tắt sóng 3G sai hai năm

Có mâu thuẫn nội bộ nên **không giải bằng cách chọn một tài liệu**: `R-00:140` tự khai đã *"tra lại
nguồn chính thức"* rồi chốt 3G tắt `30/09/2026`; cross-audit nói `2028`. Đã tra nguồn ngoài.

| Mốc | Hồ sơ ghi | Thực tế | Kết luận |
| --- | --- | --- | --- |
| 2G toàn quốc | `15/09/2026` | `15/9/2026` | ✅ **đúng, giữ nguyên** |
| 3G | `30/09/2026` | **tháng 9/2028** | ❌ **sai hai năm** |

Nguồn: [VNPT](https://vnpt.vn/gioi-thieu/tin-tuc/15-9-2026-he-thong-2g-se-ngung-hoat-dong-tai-viet-nam.html) ·
[VietnamNet](https://vietnamnet.vn/thang-9-2028-se-khai-tu-cong-nghe-3g-tai-viet-nam-2303408.html) ·
[Nhân Dân](https://nhandan.vn/viet-nam-se-tat-song-3g-vao-nam-2028-post819850.html)

Ghi nhận công bằng: cross-audit chê cả nửa 2G là *"quá tuyệt đối"*, nhưng tra lại thì mốc 2G của hồ
sơ **đúng**. Chỉ nửa 3G sai.

### Hệ quả kỹ thuật hồ sơ bỏ sót

Câu *"thiết bị 3G/CSFB hết dùng được trong vòng một tháng"* **sai**. Sau khi 2G tắt 9/2026, thiết
bị 4G dùng CSFB **vẫn rơi về 3G được tới 2028**.

Yêu cầu VoLTE vẫn đúng — nhưng lý do phải viết lại: nó là **horizon** (mua thiết bị đã đếm ngược
hạn dùng cho hệ thống chạy quá mốc đó), không phải *"chết sau một tháng"*. Điều kiện loại trừ #0 và
câu hỏi #0 gửi vendor đã sửa theo.

## 3. F-07 — nói với vendor một chính sách chưa ai ký

`R-00:75` viết *"Mỗi khách chỉ được làm phiền tối đa 2 lần"* trong tài liệu vendor-facing.

Verify: `T-09` trạng thái **`OPEN`**. Và lộ thêm một chi tiết cross-audit không nêu — **ngay các bản
đề xuất trong T-09 cũng chưa thống nhất**: Giờ Vàng 2 lần, còn 24/7 là **3** lần. Nên "tối đa 2"
sai kể cả với chính candidate.

Đã viết lại: nêu rõ con số chưa ký, và chuyển yêu cầu về đúng thứ vendor thật sự phải đáp ứng —
**trả về disposition đủ phân biệt** giữa "khách đã có cơ hội nghe máy" và "lỗi thiết bị/mạng", để
bên mua tự đếm.

### Một claim đã bị rút

Bản sửa đầu của tôi có viết *"cấu hình hiện tại của Module 3 là một lần"* — lấy từ cross-audit,
**chưa tự verify** (repo M3 nằm ngoài repo này). Đã rút khỏi tài liệu vendor và thay bằng dữ kiện
đã kiểm trong chính repo này. Không đưa claim chưa verify vào tài liệu gửi ra ngoài.

## 4. F-08 — tờ trình chốt 4 kênh, README nói chưa chốt

`README:12,56` của chính bộ hồ sơ ghi *"số kênh cho pilot chưa được quyết định ở bất kỳ đâu"* và
*"Không chốt số kênh cho pilot"*. `R-06` thì có bảng mua điền sẵn `4 kênh` và §5 tiêu đề *"Vì sao
chọn 4 kênh"*.

Đã đổi §5 thành **"đề xuất cần báo giá, chưa phải con số đã chốt"** và thêm khối cảnh báo: model
năng lực tự khai `UNCALIBRATED` nên không chứng minh được 4; câu *"chênh lệch giá nhỏ hơn mua hai
lần"* **chưa có báo giá nào chống lưng**; trước khi ký phải lấy giá **cả 1 kênh và 4 kênh**.

## 5. Verification

| Gate | Kết quả |
| --- | --- |
| `grep 30/09/2026` trong pack | chỉ còn trong chính ghi chú đính chính (đúng ý đồ) |
| `grep "trong vòng một tháng"` | `0` |
| Mốc 2G `15/09/2026` | giữ nguyên, `4` chỗ mỗi file |
| `docs-selftest.mjs` | `API_DOCS_SELFTEST_PASS` |
| `compliance-pack-selftest.mjs` | `COMPLIANCE_PACK_SELFTEST_PASS` |
| `gate-status.mjs` | `GATE_STATUS_PASS` — 11 gate, 133 work item, 21 open decision |
| `capacity-selftest.mjs` | 6 check PASS |
| Code production | `0 file` |

### Một lỗi của tôi ở W-0134 đã lộ ra và đã đóng

`gate-status.yaml` là file **generated phải khớp tracker**. Commit `a6ac830` (W-0134) của tôi thêm
row tracker mà **không regenerate** nó — cùng loại lỗi tôi đã bắt được cho traceability ở W-0131
nhưng bỏ sót ở đây. Khi kiểm thì luồng song song đã regenerate sẵn trong worktree (đủ
`W-0130/0133/0134`); lượt chạy của tôi chỉ thêm đúng `W-0135`. `GATE_STATUS_PASS` hiện xanh.

## 6. Residual gate

- Số kênh pilot: `OWNER_DECISION_REQUIRED` — cần báo giá 1 kênh vs 4 kênh.
- Attempt policy: `T-09` vẫn `OPEN`, và các candidate còn tự mâu thuẫn (GH 2 / 24-7 3).
- `§13.2` của tài liệu Module 8 **vẫn dùng từ "GSM" và chưa có ràng buộc VoLTE** — `R-00:140` đã nêu
  từ lượt sửa trước, W-0135 không đụng spec nguồn. Không sửa thì lần gộp sau lại ra bản sai.
- Model năng lực vẫn `UNCALIBRATED`; không dùng để chốt mua (`W-0008`).
