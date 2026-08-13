# FR — Call Execution, Speech and DTMF

Trạng thái: `TARGET_V1_DRAFT`.

## Lời thoại cần hỗ trợ

Ý nghĩa: “Xin chào anh/chị {customer}. Anh/chị có đơn {code} gồm {items}, tổng tiền {amount}, giao đến {area}. Bấm 1 để xác nhận, bấm 0 để hủy.”

Allowed input chỉ từ `privacy_safe_order_summary`:

- `customer_display_name`, `order_code_short`;
- `items[].public_name`, `items[].quantity`, optional `unit_label`;
- `total_amount`, `currency`;
- `delivery_area_short`, `program_display_name`, `locale`, optional pronunciation hints.

Forbidden: raw/full address, raw phone, payment credentials/details, order history, member/referral, health/sensitive note, CRM/AI free text. Script version/content/privacy approval bắt buộc trước customer calls.

## Lifecycle và mode gate (W-0024)

- Mỗi `(template_id, version)` đi theo `DRAFT → IN_REVIEW → APPROVED → RETIRED`; version đã có approval không được sửa nội dung và approval là append-only.
- `MOCK` cần `MOCK_TEST`; `LAB_REAL_SIM` cần `LAB`; `PRODUCTION_REAL` cần đồng thời `CONTENT` và `PRIVACY_LEGAL` từ hai actor khác nhau.
- Template Target V1 dùng items/short area còn cần khóa owner `OD-V1-15`; cấu hình mặc định `ProductionTargetV1FieldsApproved=NO`, vì vậy PROD fail-closed dù đã có hai approval.
- Runtime phải resolve chính xác `template_id + version + mode`; không chọn ngẫu nhiên/A-B và không fallback sang version chưa duyệt. Không resolve được thì P2-1 trả `IVR_SCRIPT_NOT_APPROVED`.
- Preview trả đúng text đã sanitize, duration ước tính, snapshot input PUBLIC-SAFE và deterministic input/content hash; audit chỉ giữ ref/status/hash, không ghi nội dung lời thoại khách hàng.

## DTMF

| Input | Normalized signal |
| --- | --- |
| `1` | `IVR_CONFIRMED` |
| `0` | `IVR_CUSTOMER_CANCELLED` |
| no input | attempt/no-answer policy |
| invalid key | `IVR_WRONG_INPUT` |
| DTMF/audio/SIM/network error | `IVR_TECHNICAL_EXCEPTION`, not customer attempt |

## Requirements

| ID | Yêu cầu |
| --- | --- |
| `FR-IVR-CALL-001` | Chỉ render script/template version đã approve đúng execution mode; deterministic snapshot/hash per task |
| `FR-IVR-CALL-002` | Item collapse policy rõ ràng, không đổi total/meaning; pronunciation test tiếng Việt |
| `FR-IVR-CALL-003` | Chỉ dial qua token resolver boundary; không persist/log raw phone |
| `FR-IVR-CALL-004` | Capture provider events rồi normalize; raw provider payload không đi Sales callback |
| `FR-IVR-CALL-005` | SIM adapter không có order-write hay notification credential |
| `FR-IVR-CALL-006` | Recording OFF |
| `FR-IVR-CALL-007` | MOCK no real dial; LAB enforce allowlist + kill switch; PROD enforce release gates |

Lab với 1 SIM thật phải kiểm answer/1, answer/0, no input, invalid key, technical failure và privacy-safe evidence.
