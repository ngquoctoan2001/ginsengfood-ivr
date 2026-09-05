# Gói ký quyết định OD-V1 — 2026-09-05

Trạng thái: `SIGNED` · Work ID: `W-0191` · Baseline: `main@2a6d290`
Người ký: IVR owner (`marketingssv2024@gmail.com`) · Người soạn phương án: Claude
Nguồn: [open-decisions-register.md](../../specs/_review/open-decisions-register.md)

> Đây là **hồ sơ quyết định**, không phải tracker thứ hai. Trạng thái sống của từng dòng nằm ở
> register; tiến độ nằm ở [prompt-execution-tracker.md](../../prompt/_execution/prompt-execution-tracker.md).
> File này giữ **lý do** — thứ mà một bảng trạng thái không chứa được và là thứ người đọc sáu tháng
> sau sẽ cần.

## 1. Đã ký gì

19 trong 23 dòng chuyển sang `CLOSED` trong một lượt. Owner chọn phương án được đề xuất cho toàn
bộ. Bốn dòng còn lại được **cố ý** giữ mở — xem §3.

| Dòng | Quyết định |
| --- | --- |
| `OD-V1-01` | Ma trận đúng như runtime đang thực thi: `GOLDEN_HOUR+ONLINE` và `TWENTY_FOUR_SEVEN+COD`, cả hai cần `ivr_confirmation_required=true` + `order_state=CONFIRMING` |
| `OD-V1-02` | Target V1 callback + bộ ACK 200/409; endpoint Golden Hour giữ vai trò compatibility-only |
| `OD-V1-03` | Sales phơi `order_version`; lệch phiên bản → ACK `REJECTED_STALE`, không ghi đè kết quả khách |
| `OD-V1-04` | Schema tóm tắt hiện hành; quá 3 dòng hàng thì gộp, tổng tiền không đổi |
| `OD-V1-05` | Hợp đồng token: TTL theo `OD-V1-17`, audit mỗi lần resolve, quá hạn/quá số lần → từ chối + review |
| `OD-V1-06` | No-answer là khuyến nghị; đơn tự hết hạn; IVR không hủy đơn |
| `OD-V1-07` | JWT bất đối xứng + JWKS + TTL ≤ 10 phút + scope `ivr.task.write`; mTLS hoãn |
| `OD-V1-08` · `OD-V1-16` | Bộ số `D-10` thành production `gh-247-prod-v1`; khung giờ `08:00–21:00` ICT; technical retry trần 1/backoff 60s |
| `OD-V1-12` | Owner là release authority; `REAL_CUSTOMER_CALL_ALLOWED` chỉ bật bằng quyết định ký tên |
| `OD-V1-13` | Giữ cả hai chương trình |
| `OD-V1-14` | Giữ `ivr_confirmation_required` làm cờ opt-in tường minh |
| `OD-V1-15` | Whitelist lời thoại **bộ rộng**, gồm tên món + số lượng + vùng giao rút gọn |
| `OD-V1-17` | Token dùng lại được, gắn `task_id`, trần số lần resolve |
| `OD-V1-18` | Resolver trong IVR, số E.164 chỉ tồn tại trong bộ nhớ tiến trình |
| `OD-V1-19` · `OD-VOICE-01` · `OD-VOICE-04` | Không vendor TTS lúc chạy; thu giọng người thật, ghép chữ số, **bỏ tên khách khỏi lời thoại** |
| `OD-V1-20` | `IVR_RUNTIME_GATE_ADMIN` tầng danger, bất đối xứng: bật kill switch một người, tắt/mở allowlist bốn mắt |

## 2. Bốn lý do đáng giữ lại

Ba chỗ dưới đây là nơi lựa chọn không hiển nhiên, và lý do quan trọng hơn kết luận.

### 2.1 Vì sao bỏ tên khách khỏi lời thoại (`OD-V1-19`)

Câu hỏi tưởng là “chọn vendor TTS nào”. Nó không phải. Kịch bản **cố định**; chỉ bốn giá trị thay
đổi giữa các cuộc gọi: tên khách, mã đơn, tổng tiền, vùng giao. Mã đơn và tiền là chữ số — ghép từ
ngân hàng ghi âm được, và cơ chế ghép đoạn đã dựng ở `W-0108`. Vùng giao là tập hữu hạn phường/quận
— thu trước được. **Chỉ tên khách là vô hạn**, và nó là lý do duy nhất buộc phải gửi dữ liệu khách
ra ngoài lúc chạy.

Bỏ nó đi thì toàn bộ câu hỏi PDPA, DPA và data residency biến mất thay vì phải đi đàm phán. Đổi lại
lời chào mất tính cá nhân. Đó là cái giá, và nó rẻ hơn một hợp đồng xử lý dữ liệu.

### 2.2 Vì sao token dùng lại thay vì one-use (`OD-V1-17`)

Năm tài liệu viết “one-use per attempt”, nhưng chính sách cần ≥2 lần gọi và không contract nào có
endpoint cấp lại. Dựng endpoint cấp lại là đắt nhất **và** là thứ duy nhất có thể hỏng ngay giữa
lúc đang gọi khách. Thay “one-use” bằng **trần số lần resolve** giữ nguyên tính chất an toàn muốn
có: một token rò rỉ vẫn không quay số được quá số lần chính sách cho phép.

### 2.3 Vì sao lấy bộ số `D-10` chứ không phải phase-8 (`OD-V1-16`)

Bộ phase-8 tự mâu thuẫn: nó cho “Giờ Vàng” một cửa sổ 600 giây, trong khi Giờ Vàng là lời hứa 5
phút. Bộ `D-10` mới hơn, nhất quán với cửa sổ đó, và là bộ mà scheduler/DB/test đang chạy.

Phần `D-10` **không** nói mà lượt ký này bổ sung: khung giờ được phép gọi. Hiện chưa có ràng buộc
nào — hệ thống hôm nay sẵn sàng gọi lúc 3 giờ sáng nếu có task đến. `08:00–21:00` ICT là số đề
xuất; nó phải là một giá trị cụ thể chứ không phải một khoảng trống.

### 2.4 Việc thật mà `OD-V1-13` kéo theo

Giữ `GOLDEN_HOUR+ONLINE` không miễn phí. `DS-02` đọc từ Sales nói Core chỉ chuyển trạng thái cho
đơn COD và **từ chối `422` mọi đơn non-COD**. Nếu ONLINE ở trong phạm vi thì Sales phải định nghĩa
transition cho ONLINE, nếu không mọi callback ONLINE sẽ hỏng ở production dù IVR làm đúng. Đây là
hạng mục đầu tiên phải mang sang bàn với Sales, không phải thủ tục giấy tờ.

## 3. Bốn dòng cố ý giữ mở

| Dòng | Vì sao không ký |
| --- | --- |
| `OD-V1-09` (nửa sau) | Bảng ánh xạ tín hiệu nhà mạng → result là **ứng viên**. Mã thật chỉ biết khi có SIM. Nửa đầu (giao thức `LAB-01..08`, allowlist một đích) đã ký để chuẩn bị lab được. |
| `OD-V1-10` | Con số 32 kênh là giả định, chưa phải phép đo. Mở lại sau bước 4.5 (mô hình tải) và 5.4 (thông lượng thật một kênh). |
| `OD-V1-11` | Nội dung đã ký (ghi âm TẮT, thời hạn lưu). Phần chưa giải được: `PRODUCTION_REAL` đòi **ba actor id khác nhau** — `ScriptContentContracts.EnsureApprovalAllowed` chặn người tạo tự duyệt (dòng 240) và bắt `CONTENT` ≠ `PRIVACY_LEGAL` (dòng 254). |
| `OD-V1-21` | Cấu hình GitLab đã ký và đã có bằng chứng PASS. Vế còn lại đòi **một reviewer độc lập** cho merge request. |

Hai dòng cuối cùng là một bài toán chứ không phải hai: **dự án có một người, còn hai lớp kiểm soát
này tồn tại chính vì một người không nên tự mình làm được.** Ba lối ra, chưa chọn:

1. Cử một người thật làm approver cho đúng hai thao tác này.
2. Dừng ở `LAB_REAL_SIM` — pilot chỉ cần một approval `LAB`, không cần quorum.
3. Sửa luật thành một người. **Khuyến nghị: không.** Đây đúng là lớp chặn khiến một người không thể
   tự đẩy một kịch bản sai ra gọi khách thật.

## 4. Việc mà chữ ký vừa mở ra (thuộc GĐ 2)

Ký xong không có nghĩa code đã đúng. Danh sách dưới đây là hệ quả trực tiếp, không phải mong muốn.

| Từ | Việc | Ước lượng |
| --- | --- | --- |
| `OD-V1-20` | Hiện thực `IRuntimeGateAuthorization`, `IFourEyesApprovalVerifier`, `IProductionCallGate` + bảng phê duyệt append-only + negative authz tests. Chừng nào chưa có, `POST /feature-flags/{env}` vĩnh viễn bị chặn | 2–3 ngày |
| `OD-V1-08`/`16` | Thêm `gh-247-prod-v1` vào registry, cổng kích hoạt lúc khởi động, so cờ pre-dial với snapshot policy của job, thực thi khung giờ trong scheduler | 2 ngày |
| `OD-V1-17`/`05` | Sửa vault/resolver theo ngữ nghĩa token dùng lại; test 2 lượt gọi + technical retry + replay | 1–2 ngày |
| `OD-V1-15` | Bật `ProductionTargetV1FieldsApproved=YES`; đồng bộ 3 spec đang mâu thuẫn về whitelist | 0.5 ngày |
| `OD-V1-02` | Bump OpenAPI `1.0.0-draft.22` → `1.0.0`, sinh lại client, chạy lại contract test | 1 ngày |
| `OD-V1-18` | Sửa `specs/api/04-sim-adapter-contract.md` đang nói ngược với quyết định; thêm sơ đồ trust boundary | 0.5 ngày |
| `OD-V1-19` | Thu giọng + hợp đồng license; ngân hàng ghi âm chữ số và vùng giao; bỏ biến tên khỏi template (đổi `TemplateHash` → duyệt lại kịch bản) | GĐ 5 |
| `OD-V1-04` | Quy tắc gộp dòng hàng vào renderer + test phát âm tiếng Việt | 0.5 ngày |
| `OD-V1-13` | Mang câu hỏi transition cho `ONLINE` sang Sales trước khi nối thật | — |

## 5. Cái gói này KHÔNG làm

- **Không đóng gate ngoài nào.** Chữ ký đóng *quyết định*; gate cần artifact thật.
- **Không bật cờ nào.** `REAL_CUSTOMER_CALL_ALLOWED` vẫn là `NO` ở cả bốn môi trường.
- **Không đổi runtime.** Lượt này chỉ ghi quyết định; code đi ở GĐ 2.
- **Không tạo ra người thứ hai**, nên hai dòng cần quorum vẫn mở.
