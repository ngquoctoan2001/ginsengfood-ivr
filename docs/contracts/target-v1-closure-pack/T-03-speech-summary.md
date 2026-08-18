# T-03 — Privacy-safe order summary và whitelist biến lời thoại

External work `W-0003` · quyết định `OD-V1-04`, `OD-V1-15` · gate **business acceptance** · trạng thái `OPEN`

Owner: **Sales/Product** (nội dung + schema) và **Privacy/Legal** (mở rộng whitelist).

Due: whitelist (`OD-V1-15`) chốt **trước khi duyệt script production** trong lifecycle `P2-7`; nội dung summary chốt **trước pilot `P8-2`**. Ngày cam kết của owner: `<owner điền>`.

## 1. Current evidence — đã đọc từ nguồn

**Schema đã có, đầy đủ, và `additionalProperties: false`.** [`ivr-order-confirmation.v1.yaml`](../../../specs/api/openapi/ivr-order-confirmation.v1.yaml) — `PrivacySafeOrderSummary`:

| Field | Bắt buộc | Ràng buộc |
| --- | --- | --- |
| `customer_display_name` | có | 1–80 |
| `order_code_short` | có | 1–40 |
| `items[]` | có | `minItems: 1`, mỗi item cần `public_name` (1–160) + `quantity` (> 0), optional `unit_label` |
| `total_amount` | có | số ≥ 0 |
| `currency` | có | `enum: [VND]` |
| `delivery_area_short` | có | 1–160 + regex (xem dưới) |
| `program_display_name` | có | 1–80 |
| `locale` | có | `enum: [vi-VN]` |
| `pronunciation_hints` | không | map string→string |

**Whitelist: ba spec đã hết mâu thuẫn, nhưng quyết định owner thì chưa.** `W-0024` đã chuẩn hoá: cả [`specs/data/05-pii-policy.md`](../../../specs/data/05-pii-policy.md), [`specs/ui/04-ivr-menu-config.md:17`](../../../specs/ui/04-ivr-menu-config.md) và [`specs/api/04-sim-adapter-contract.md:11`](../../../specs/api/04-sim-adapter-contract.md) giờ đều phân biệt rõ hai bộ và trỏ về một nguồn canonical duy nhất (`specs/functional/04`):

| Bộ | Biến | Trạng thái |
| --- | --- | --- |
| **Current approved (hẹp)** | `order_code_short`, `total_amount_display`, opt `customer_name_short`, `program_name` | đã duyệt, hậu thuẫn bởi business source `PACK-09 §9.1` |
| **Target V1 proposal (rộng)** | thêm `items[].public_name`, `items[].quantity`, opt `items[].unit_label`, `delivery_area_short` | `OWNER_DECISION_REQUIRED` — `OD-V1-15` |

**Code đang chạy bộ rộng.** [`src/Ivr.Domain/Confirmation/PrivacySafeSpeech.cs`](../../../src/Ivr.Domain/Confirmation/PrivacySafeSpeech.cs) enforce whitelist và có detector ném `IVR_PII_POLICY_VIOLATION` khi phát hiện địa chỉ đầy đủ (dòng `118`). Fixture MOCK dùng bộ rộng. **Điều này không đóng gate** — nó chỉ chứng minh IVR đọc được bộ rộng nếu bộ rộng được duyệt.

## 2. Target delta — chính xác là gì

**(a) Mở whitelist tự nó là một quyết định privacy, không phải quyết định kỹ thuật.** Bộ hẹp đọc cho khách nghe: mã đơn rút gọn + tổng tiền. Bộ rộng đọc thêm **khách mua gì** và **giao về khu vực nào**. Ai nghe được cuộc gọi đó — người nhà, đồng nghiệp, người cầm máy hộ — nghe được luôn hai thông tin đó. Đây là câu hỏi cho Privacy/Legal, không phải cho dev.

**(b) `delivery_area_short` — regex chỉ chặn được một nửa.** Pattern trên wire:

```
^(?!\s*\d)(?!.*\d+\s*/\s*\d+).*$
```

Nó từ chối số nhà đứng đầu (`123 <tên phố>`) và số nhà dạng gạch chéo (`12/3 <tên phố>`), đồng thời **vẫn chấp nhận** đơn vị hành chính có số (`Quan 7`, `Phuong 12`) — đúng như mô tả trong schema. Nhưng một địa chỉ phố **không có chữ số** vẫn lọt qua schema. Nửa còn lại do detector ngữ nghĩa phía IVR bắt lúc intake (`FR-IVR-INTAKE-005`).

Hệ quả cần Sales xác nhận rõ: **Sales chịu trách nhiệm normalise giá trị này**, không phải "đẩy sang IVR lọc". Nếu Sales phát địa chỉ đầy đủ, IVR sẽ từ chối task — im lặng với Sales trừ khi Sales đọc mã lỗi.

**(c) `items[]` chưa có giới hạn trên.** `minItems: 1` nhưng không có `maxItems`. Đơn 40 dòng sẽ thành câu thoại dài vài phút; khách cúp máy trước khi tới phần bấm phím. Cần chốt: đọc tối đa bao nhiêu dòng, phần dư diễn đạt thế nào ("và 12 sản phẩm khác"), và ai quyết định thứ tự dòng.

**(d) `public_name` là tên nào.** 160 ký tự là tên marketing đầy đủ. Cần xác nhận đây là tên rút gọn dành để **đọc**, không phải tên SKU thương mại. Kèm quy tắc: tên có ký tự đặc biệt, tên tiếng Anh, tên có dung tích (`500ml`) đọc ra sao.

**(e) `total_amount` là số, không phải chuỗi đã format.** IVR tự format khi đọc. Cần xác nhận cách đọc số tiền được business chấp nhận (làm tròn? đọc "năm trăm sáu mươi nghìn" hay "năm trăm sáu mươi ngàn"?) — thuộc acceptance phát âm ở `OD-V1-19`, nhưng nguồn số thì thuộc ticket này.

## 3. Sample payload

Hợp lệ theo contract hiện tại (bộ rộng):

```json
{
  "privacy_safe_order_summary": {
    "customer_display_name": "chị An",
    "order_code_short": "0001",
    "items": [
      { "public_name": "Nước hồng sâm", "quantity": 2, "unit_label": "hộp" }
    ],
    "total_amount": 560000,
    "currency": "VND",
    "delivery_area_short": "Phường Bến Nghé, Quận Một",
    "program_display_name": "Giờ Vàng",
    "locale": "vi-VN"
  }
}
```

Nếu `OD-V1-15` chốt bộ **hẹp**, payload trên phải đổi: `items[]` và `delivery_area_short` rời khỏi `required`, và schema + code + fixture + ba spec phải sửa đồng bộ.

## 4. Acceptance test — phải xanh khi đóng

| Test | Ở đâu | Khẳng định |
| --- | --- | --- |
| `SpeechPiiFailsClosedWithoutPersistingWork` | [`tests/Ivr.UnitTests/Intake/TaskIntakeServiceTests.cs:150`](../../../tests/Ivr.UnitTests/Intake/TaskIntakeServiceTests.cs) | Địa chỉ đầy đủ → từ chối **trước khi** ghi bất cứ thứ gì |
| `PersistedMetadataPiiFailsBeforeAnyWorkIsCreated` | cùng file, dòng `176` | PII trong metadata cũng fail-closed |
| `IT-API-PII-05` | `tests/Ivr.IntegrationTests/` | Không rò PII qua API surface |
| `CT-INTAKE-FIXTURES-02` | `tests/Ivr.ContractTests/TaskIntakeContractTests.cs` | Fixture khớp schema đã ghim |
| **`CDC-SPEECH-01`** *(Sales viết)* | producer test phía Sales | Mọi summary phát ra đều pass schema **và** pass detector địa chỉ |

## 5. Mock fallback

Fixture MOCK dùng bộ rộng privacy-safe, đã enforce whitelist + test (`W-0024`). Recording OFF. Không có khách thật. Fixture **không** đóng gate production — ghi rõ ở [`specs/data/05-pii-policy.md`](../../../specs/data/05-pii-policy.md).

## 6. Closure artifact — owner điền

- [ ] **Whitelist đã duyệt** (`OD-V1-15`): chốt bộ hẹp hay bộ rộng, có chữ ký **Product và Privacy/Legal**. Kèm PIA hoặc privacy sign-off cho phần mở rộng.
- [ ] **Schema + ví dụ từ Sales** (`OD-V1-04`): ai sinh summary, sinh lúc nào, normalise `delivery_area_short` bằng quy tắc gì.
- [ ] **Giới hạn `items[]`**: số dòng tối đa đọc ra + cách diễn đạt phần dư.
- [ ] **Cập nhật đồng bộ**: nếu whitelist đổi, sửa cùng lúc OpenAPI, `PrivacySafeSpeech.cs`, fixture và ba spec. Sửa lệch một chỗ là tạo lỗ hổng.

## 7. Rủi ro nếu để mở

Đây là ticket duy nhất trong gói mà **rủi ro là pháp lý chứ không phải kỹ thuật**. Nếu chạy thật với bộ rộng chưa duyệt, mỗi cuộc gọi là một lần đọc thông tin đơn hàng cho một người chưa xác thực danh tính, trên một kênh không ghi âm nên không chứng minh được đã đọc gì. Không rollback được sau khi đã gọi.
