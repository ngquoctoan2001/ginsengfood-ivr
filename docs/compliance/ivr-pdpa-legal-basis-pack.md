# PDPA / legal basis pack — `W-0059` · `P11-3`

Ngày: `2026-08-19` · Trạng thái: **`LEGAL_SIGNOFF_REQUIRED`** · Người đọc: Legal, Privacy

## 0. Đây không phải tư vấn pháp lý

Tài liệu này do bên **xây hệ thống** viết. Nó mô tả hệ thống làm gì và đề xuất cơ sở pháp lý nào phù
hợp, để Legal có thứ cụ thể mà đồng ý hoặc bác. Không mục nào là kết luận pháp lý.

## 1. Cơ sở pháp lý đề xuất

| Hoạt động | Cơ sở đề xuất | Lập luận |
| --- | --- | --- |
| Gọi xác nhận đơn COD / Golden Hour | **thực hiện hợp đồng** | Cuộc gọi là **một bước thực hiện** đơn khách đã đặt, không phải tiếp thị về nó. Không gọi thì đơn COD không xác nhận được. |
| Giữ audit về hành động quản trị | **ghi chép pháp lý** | Bản ghi về việc *đã quyết gì* phải tồn tại **độc lập** với bên mà nó nói tới. |
| Giữ payload callback đã gửi Sales | **ghi chép pháp lý** | Bản ghi giao nhận; bỏ payload thì không giải quyết được tranh chấp nó tồn tại vì. |
| Ghi âm | **không có** | DT-05: TẮT. Cơ sở của cuộc gọi **không bao trùm** việc ghi lại cuộc gọi đó. |

**Không mục nào dựa trên đồng ý**, và đó là khẳng định có kiểm chứng: IVR **không bao giờ** hiển thị
hộp thoại xin đồng ý và **không lưu** quyết định đồng ý nào. `COMP-PII-01` đỏ nếu một trường khai cơ
sở là đồng ý.

## 2. Hệ quả trực tiếp của cơ sở đã chọn

Vì cơ sở là **thực hiện hợp đồng** chứ không phải **lợi ích chính đáng**:

- **Do-not-call là chặn cứng**, không phải một đầu vào đem cân đo. `COMP-DNC-03` khẳng định như một
  **tính chất**: khi resolver nói không được gọi, **không kết hợp tín hiệu nào khác** — kể cả khách
  `TRUSTED` với trust-skip bật — cho ra quyết định gọi được.
- **Ba trạng thái đều dừng dispatch**: bị chặn, **không rõ**, và **nguồn không trả lời**. "Resolver
  không trả lời được" không phải là sự cho phép, và "chưa ai nói" cũng vậy.
- **Không có quyền phản đối kiểu marketing** để cân, vì đây không phải marketing — nhưng cũng vì thế
  **không được** dùng cuộc gọi này cho bất cứ mục đích nào ngoài xác nhận đơn.

## 3. Quyền của chủ thể dữ liệu — cái làm được và cái không

| Quyền | Trong phạm vi IVR | Cơ chế |
| --- | --- | --- |
| Truy cập | **có** — trả về **số lượng theo bảng**, không trả giá trị | `DsarService.FindAsync` |
| Xoá | **một phần** — redact trường liên hệ trong `ivr_confirmation_tasks` | `DsarService.EraseAsync` |
| Đính chính | **không** — IVR không sở hữu dữ liệu gốc; yêu cầu đi tới Sales | — |
| Hạn chế xử lý | **có** — `legal_hold_until` loại bản ghi khỏi mọi batch retention | `IT-RET-HOLD-04` |
| Di chuyển dữ liệu | **không áp dụng** — IVR không giữ dữ liệu do chủ thể cung cấp | — |

**Ba giới hạn của quyền xoá**, nói **trước** khi xử lý yêu cầu:

1. **Audit không xoá được** — append-only ép bởi database. Một bản ghi *ai đã làm gì* mà chủ thể xoá
   được thì không phải bản ghi.
2. **`order_code` được giữ** — là khoá mà yêu cầu đi tới; xoá nó làm **mọi** yêu cầu sau về cùng đơn
   không trả lời được, kể cả của chính người đó.
3. **Payload callback giữ tới hết retention** — xem §1.

Và một giới hạn thứ tư ở tầng hạ tầng: **không xoá chọn lọc được bên trong một bản backup đã mã
hoá**. Bản backup còn hạn vẫn giữ dữ liệu trước khi redact.

## 4. Bằng chứng kỹ thuật cho từng khẳng định

| Khẳng định | Bằng chứng |
| --- | --- |
| Không lưu số điện thoại thật | check constraint `ck_ivr_confirmation_tasks_masked_phone`; `docs/evidence/W-0012` |
| Dial token chỉ giải ra số ở ranh giới SIM (D-05) | `SEC-ROT-05` (reflection trên model persistence); `docs/evidence/W-0047` |
| PII guard fail-closed | `UT-FND-PII-12`; `docs/evidence/W-0040` |
| Do-not-call chặn ở cả ba trạng thái | `COMP-DNC-03`, `UT-ELIG-VOICE-15`; `docs/evidence/W-0052` |
| Danh mục khớp schema đang ship | `COMP-PII-01`; `docs/evidence/W-0052` |
| DSAR đúng phạm vi, audit bất biến | `COMP-DSAR-02`; `docs/evidence/W-0052` |
| Kho phân tích không chứa PII | `BI-PII-01`; `docs/evidence/W-0055` |
| Recording OFF | `specs/decisions/DT-05-recording-off-policy.md` |
| Retention cơ chế đã kiểm | `IT-RET-*`; `docs/evidence/W-0064` |
| Backup mã hoá + xác thực | `DG-BACKUP-02`; `docs/evidence/W-0053` |

## 5. Cái pack này KHÔNG khẳng định

- **Chưa gọi khách thật lần nào.** `REAL_CUSTOMER_CALL_ALLOWED=NO`, `MOCK`, seed dùng dải test
  `84xxxxx…`. Mọi phân tích ở trên nói về một hệ thống **chưa từng xử lý dữ liệu khách thật**.
- **Không xác định luật áp dụng.** Nghị định/PDPA nào và deadline phản hồi bao lâu là đầu vào của
  Legal (`W-0009`); ghi ra để trống thay vì điền một con số nghe hợp lý.
- **Không xác minh danh tính người yêu cầu** — IVR không có kênh nào.
- **Whitelist trường script chưa ký** (`OD-V1-15`) — nên "không đọc địa chỉ/thanh toán/sức khoẻ" là
  một **cơ chế đã dựng** chứ không phải một danh sách đã duyệt.
