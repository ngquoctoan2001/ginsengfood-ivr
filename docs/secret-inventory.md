# Secret inventory — `W-0047` · `P7-5` §6.1

Ngày: `2026-08-19`

## 1. Cái IVR **không** giữ, nói trước

| Không giữ | Nằm ở đâu | Quyết định |
| --- | --- | --- |
| key giải mã `dial_token → số thật` | token vault / SIM adapter boundary **ngoài IVR** | D-05, `OD-V1-18` |
| bảng ánh xạ `dial_token → số thật` | như trên | D-05 |

Đây là mục đầu tiên vì nó định hình mọi thứ còn lại: IVR lưu **ciphertext mờ đục**, một
**reference** và một dạng **masked** — và **không** giữ thứ gì biến chúng ngược lại thành số.
`SEC-ROT-05` khẳng định điều đó bằng reflection trên model persistence, chứ không bằng một danh sách
ai đó bảo trì tay: một cột thêm vào ngày mai đúng là trường hợp danh sách tay bỏ sót.

`MockDialTokenVault` **có** giữ ánh xạ giả trong bộ nhớ — nhưng nó là **ranh giới tin cậy giả lập**
đứng thay cho vault ngoài, và nó chỉ ghi **fingerprint** xuống storage của IVR. Phân biệt này quan
trọng: nếu đọc nhầm nó thành "IVR giữ mapping" thì sẽ kết luận sai về D-05.

## 2. Secret IVR thật sự giữ

| Secret | Độ nhạy | Chủ sở hữu | TTL đề xuất | Trạng thái hôm nay |
| --- | --- | --- | --- | --- |
| `IVR_INTERNAL_SERVICE_TOKEN` | **cao** — mở admin API nội bộ | IVR | 90 ngày | env lúc chạy; app **từ chối boot** nếu thiếu |
| `ORDER_CORE_SERVICE_TOKEN` | **cao** — cho phép tạo task | IVR + Sales | 90 ngày | chỉ compat; `TARGET_V1` **từ chối** hoàn toàn (W-0032) |
| mật khẩu database | **cao** | Platform | 90 ngày | K8s Secret tham chiếu; chart không mang giá trị |
| `CurrentGoldenHourInternalToken` | trung bình | IVR + Sales | 90 ngày | chỉ dùng ở lối compat |
| credential gọi **dial-token resolver** | **cao nhất** (D-05) | IVR + token vault | **30 ngày** | mock; thật thuộc `W-0008` |
| SIM gateway credential | **cao** | Platform | 30 ngày | **chưa tồn tại** — `BLOCKED_EXTERNAL` (DT-01, `W-0008`) |
| khoá ký JWT service identity | cao | Platform | 30 ngày | `MockOidcIssuer` sinh RSA **theo tiến trình**; không persist, không rotate được — mock-only |

TTL là **đề xuất**, chưa chủ sở hữu duyệt. Chúng dựa trên độ nhạy chứ không dựa trên chính sách đã
chốt nào.

## 3. Least-exposure — bốn lớp đã có

| Lớp | Cơ chế | Cổng |
| --- | --- | --- |
| git | gitleaks | `P0-2` |
| build context | `.dockerignore` loại `.env*`, `*.pem`, `*.key`, `*.pfx` | `W-0043` |
| image layer | không secret nào nướng vào; app **từ chối boot** nếu thiếu | `W-0043` §8 |
| audit/log | audit rotation mang **fingerprint**, không mang giá trị | `SEC-ROT-04` |

Lớp thứ tư là lớp dễ quên nhất: một dòng audit trích dẫn giá trị **chính là** vụ rò rỉ mà nó tồn tại
để ghi lại.

## 4. Vì sao fingerprint an toàn — và điều kiện để nó an toàn

Audit ghi 12 hex đầu của SHA-256. Điều đó chỉ an toàn khi secret đủ entropy: với một secret ngắn,
fingerprint bị brute-force và **bản ghi audit trở thành oracle cho thứ nó mô tả**.

Nên `RotatingCredentialProvider` **ép** độ dài tối thiểu 24 ký tự và ném lỗi nếu thấp hơn. Đó là
điều kiện làm cho việc ghi fingerprint hợp lệ, không phải một kiểm tra trang trí.

## 5. Cái này chưa có

- **Chưa có Vault/KMS.** Prod dự kiến Vault/KMS (`W-0063`, `NEED_CONFIRMATION`); hôm nay chỉ K8s
  Secret. `deploy/secrets/` là cấu hình ExternalSecret cho một hạ tầng **chưa tồn tại**.
- **Chưa có dynamic secret / lease-renew** (§6.3) — cần Vault.
- **Khoá ký JWT không rotate được**: `MockOidcIssuer` sinh RSA theo tiến trình. Rotation thật cần
  một issuer thật, thuộc `W-0006`/`W-0063`.
- **Chưa lượt rotation nào chạy trên hệ triển khai.** `SEC-ROT-01`/`-03` chứng minh **cơ chế**
  zero-downtime trong test; chưa có drill nào trên cluster.
