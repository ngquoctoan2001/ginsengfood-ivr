# T-02 — Task data: `order_version`, `order_state`, eligibility/restriction evidence

External work `W-0002` · quyết định `OD-V1-03` · gate **real integration** · trạng thái `OPEN`

Owner: **Sales Core**.

Due: chốt **trước khi bắt đầu `P4-2`** — cùng lúc với [T-01](T-01-program-matrix.md). Ngày cam kết của owner: `<owner điền>`.

> **Current correction `OD-17` / `W-0149` (03/09/2026):** các đoạn bên dưới nói
> `sellable_status[]` còn trên wire/runtime là historical và đã bị `OD-17` supersede. Current IVR
> không đọc inventory/recall/sale-lock/quality-hold và không có Ops egress. Eligibility evidence đã
> có linked JSON schema proposal, nhưng vẫn `TARGET_DRAFT_NOT_OWNER_APPROVED`; freshness current chỉ
> kiểm `captured_at` trong confirmation window và không ở tương lai. Không có maximum age,
> `valid_until`, source revision ordering, mid-window revoke hoặc per-attempt business recheck.
> `order_version` vẫn được echo bất biến để M3 revalidate. Xem
> [M8-09 decision pack](../../../plan/ivr-orther/m8-09-revoke-freshness-decision-pack-2026-09-03.md).
>
> Vì vậy không dùng các mục historical `(b)/(c)` hay sample `sellable_status[]` dưới đây để build
> producer mới. Closure hiện tại thuộc một trong hai hướng: M3 chứng minh D-06 callback revalidation
> (A), hoặc hai bên ký revoke/freshness command/race matrix (B/hybrid).

## 1. Current evidence — đã đọc từ nguồn

**`order_version` là bắt buộc trên task và bắt buộc echo lại trên callback.**

| Field | Ở đâu | Kiểu |
| --- | --- | --- |
| `order_version` | [`ivr-order-confirmation.v1.yaml`](../../../specs/api/openapi/ivr-order-confirmation.v1.yaml) — `required` | `string`, mô tả "Required Target V1 stale-result guard snapshot" |
| `order_version_seen_by_ivr` | [`order-core-ivr-callback.target-v1.yaml:141,155`](../../../specs/api/openapi/order-core-ivr-callback.target-v1.yaml) — `required` | `string`, 1–120 ký tự |
| `REJECTED_STALE` | cùng file, `CallbackAck409` | Sales trả về khi version đã trôi |

Nghĩa là: IVR chụp version lúc nhận task, mang nguyên si trả về lúc báo kết quả, và **Sales là bên duy nhất quyết định version đó còn tươi hay không**. IVR không so sánh, không suy luận.

**`order_state` được khai là opaque nhưng IVR đang hard-code một giá trị.**

OpenAPI: `order_state: { type: string, description: Opaque enum owned by Order Core (D-02) }` — không có `enum`, cố ý.

Nhưng [`src/Ivr.Domain/Policies/EligibilityRules.cs:128`](../../../src/Ivr.Domain/Policies/EligibilityRules.cs) so sánh literal:

```csharp
if (!string.Equals(snapshot.OrderState, "CONFIRMING", StringComparison.Ordinal)
```

và dòng `118` chặn thêm `"QUOTE"`, `"CART"`, `"DRAFT"` (bảo vệ DO-01: không gọi trên giỏ/báo giá).

**Eligibility evidence: thứ được type thì optional, thứ bắt buộc thì không có type.**

| Field | Bắt buộc? | Type trên wire | IVR validate gì |
| --- | --- | --- | --- |
| `eligibility_snapshot` | **có** | `object`, `additionalProperties: true` | chỉ kiểm "là object" — [`TaskIntakeEndpoint.cs:274`](../../../src/Ivr.Api/Intake/TaskIntakeEndpoint.cs) `EnsureObject` |
| `evidence_ref` | **có** | `string` | có mặt |
| `sellable_status[]` | **không** | `SellableStatusLine` — typed đầy đủ | typed, fail-closed khi có |
| `call_restriction` | **có** | `boolean` | `true` chặn dispatch (xem [T-03](T-03-speech-summary.md) không liên quan; luật ở `EligibilityRules.cs:196`) |

## 2. Target delta — chính xác là gì

**(a) `order_state`: opaque theo hợp đồng, literal theo code.** Nếu Sales đổi tên state, tách `CONFIRMING` thành hai state, hoặc thêm state callable mới, IVR sẽ trả `ORDER_STATE_NOT_CALLABLE` cho **toàn bộ** task mới mà không có tín hiệu nào ở phía Sales. Cần một trong hai:
- Sales công bố danh sách state callable như **dữ liệu** (trong task hoặc endpoint riêng), IVR thôi hard-code; **hoặc**
- Sales cam kết `CONFIRMING` là hằng số hợp đồng, đổi phải qua deprecation của [T-08](T-08-openapi-compat-cdc.md).

**(b) `eligibility_snapshot` chưa có shape.** Đang là túi JSON tự do bắt buộc phải có. IVR không thể fail-closed trên thứ mình không hiểu — hiện chỉ kiểm nó tồn tại. Cần schema thật: quyết định nào có trong đó, freshness đo bằng field nào, `source_version` nằm ở đâu.

**(c) `sellable_status[]` optional nhưng mang chính thông tin fail-closed.** [`EligibilityRules.cs`](../../../src/Ivr.Domain/Policies/EligibilityRules.cs) có đủ reason code cho `SELLABLE_SNAPSHOT_MISSING`, `SELLABLE_SNAPSHOT_STALE`, `SELLABLE_STATUS_UNKNOWN`, `INVENTORY_NOT_SELLABLE`, `RECALL_HOLD_ACTIVE`, `SALE_LOCK_ACTIVE`. Nếu Sales không phát mảng này, IVR rơi vào nhánh `SELLABLE_SNAPSHOT_MISSING` → chặn. Cần chốt: **bắt buộc hay optional**. Nếu optional thì IVR chặn bằng gì thay thế.

**(d) `order_version` là string, không có thứ tự.** IVR không so sánh nên không sao — nhưng cần Sales xác nhận version **bump khi nào**: mỗi lần sửa đơn, hay chỉ khi sửa field ảnh hưởng xác nhận. Nếu bump theo mọi thay đổi (kể cả note nội bộ), tỉ lệ `REJECTED_STALE` sẽ cao giả tạo và kết quả khách đã bấm phím bị vứt.

## 3. Sample payload

```json
{
  "order_version": "17",
  "order_state": "CONFIRMING",
  "eligibility_snapshot": {
    "source_version": "<Sales điền>",
    "captured_at": "2026-08-18T03:00:00Z",
    "decisions": { "<Sales điền>": "<Sales điền>" }
  },
  "evidence_ref": "evidence://sales/order-0001/eligibility",
  "sellable_status": [
    {
      "sku_id": "SKU-A1",
      "batch_id": "BAT-2026-0001",
      "decision": "SELLABLE",
      "recall_hold": false,
      "sale_lock": false,
      "quality_hold": false,
      "stock_available": true,
      "trace_ready": true,
      "captured_at": "2026-08-18T02:59:59Z"
    }
  ],
  "call_restriction": false
}
```

Callback tương ứng echo lại đúng version đã thấy:

```json
{ "order_version_seen_by_ivr": "17", "result_type": "IVR_CONFIRMED" }
```

## 4. Acceptance test — phải xanh khi đóng

| Test | Ở đâu | Khẳng định |
| --- | --- | --- |
| `IT-ELIG-FAILCLOSED-08` | `tests/Ivr.IntegrationTests/EligibilityPersistenceTests.cs` | Evidence thiếu/unknown/stale đều chặn trước dispatch |
| `IT-ADMIN-READ-*` | `tests/Ivr.IntegrationTests/AdminReadApiTests.cs` | `sellable_status` per-line hiện đúng trên màn chi tiết |
| `CT-CONTRACT-TARGET-ACK-04` | `tests/Ivr.ContractTests/SalesContractScaffoldTests.cs` | Mapping ACK, gồm `REJECTED_STALE` |
| **`CDC-VERSION-01`** *(Sales viết)* | consumer test phía Sales | Callback mang version cũ → `409 REJECTED_STALE`; version hiện hành → `200` |
| **`CDC-STATE-01`** *(Sales viết)* | producer test phía Sales | Producer không phát task ở state không callable |

## 5. Mock fallback

Fake Sales phát `eligibility_snapshot` dạng túi tự do có `source_version` + `captured_at`, và `sellable_status[]` đầy đủ. WireMock trả `REJECTED_STALE` theo kịch bản. Toàn bộ nhánh fail-closed đã có test — **nhưng test trên shape do IVR tự nghĩ ra**.

## 6. Closure artifact — owner điền

- [ ] **Schema `eligibility_snapshot`** (JSON Schema hoặc mục trong OpenAPI của Sales) + ví dụ pass/block/stale/source-unavailable.
- [ ] **Quyết định `sellable_status[]`**: bắt buộc hay optional; nếu optional, IVR fail-closed bằng gì.
- [ ] **Semantics `order_version`**: bump khi nào, so sánh thế nào, ai so sánh. Kèm test `REJECTED_STALE` đã merge.
- [ ] **Danh sách `order_state` callable**: hoặc phát như dữ liệu, hoặc cam kết `CONFIRMING` là hằng số hợp đồng có deprecation policy.

## 7. Rủi ro nếu để mở

Mục (a) là loại lỗi tệ nhất: **im lặng, toàn bộ, và chỉ lộ khi Sales deploy** một thay đổi họ không biết là breaking. Không alert nào của IVR bắt được, vì với IVR thì "task ở state không callable" là hành vi đúng.
