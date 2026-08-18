# T-08 — OpenAPI compatibility, deprecation và sở hữu consumer-driven test

External work `W-0002`, `W-0005` (process ticket, không gắn `OD-V1` riêng) · gate **real integration** · trạng thái `OPEN`

Owner: **Sales API** và **IVR** — ticket duy nhất trong gói cần **cả hai bên cam kết**, không phải một bên trả lời.

Due: chốt **trước release gate `P9-1`** — cam kết đối xứng phải có trước ngày cắm thật thứ hai. Ngày cam kết của owner: `<owner điền>`.

## 1. Current evidence — đã đọc từ nguồn

**Phía IVR đã có đủ bộ máy quản trị hợp đồng, và nó đang chạy trong CI.**

| Cơ chế | Vị trí | Làm gì |
| --- | --- | --- |
| Ghim hash | [`specs/api/openapi/contract-manifest.json`](../../../specs/api/openapi/contract-manifest.json) | sha256 cho từng contract + commit Sales baseline |
| Chặn breaking | [`deploy/ci/docs.gitlab-ci.yml:22`](../../../deploy/ci/docs.gitlab-ci.yml) | `oasdiff breaking … --fail-on WARN`, image ghim theo digest `tufin/oasdiff:v1.26.1@sha256:aae8cfcf…` |
| Changelog tự sinh | `deploy/ci/scripts/generate-oasdiff-changelog.sh` | so với baseline trong `specs/api/openapi/baselines/` |
| Chặn drift | `deploy/ci/scripts/openapi-contract-drift.mjs` | spec và code sinh ra không được lệch nhau |
| Chính sách phiên bản | [`docs/api-versioning.md`](../../../docs/api-versioning.md) | SemVer, vòng đời `DRAFT→APPROVED→DEPRECATED→SUNSET→REMOVED`, header `Deprecation`/`Sunset`/`Link`, grace ≥ 90 ngày |

Chính sách này nghiêm hơn mặc định ở một điểm đáng chú ý: **thêm giá trị enum cũng bị coi là breaking** cho tới khi mọi consumer chứng minh chịu được giá trị lạ (`docs/api-versioning.md:18`).

**Phía Sales, hiện chỉ có một ảnh chụp.** [`specs/api/compat/current-golden-hour-callback.a3aad246.schema.json`](../../../specs/api/compat/current-golden-hour-callback.a3aad246.schema.json) — verify tại commit `a3aad246d986fbc273cf41aaa93eec6659669656`, trạng thái `CURRENT_COMPAT_VERIFIED_AT_PINNED_SHA`.

## 2. Target delta — chính xác là gì

**(a) Quản trị hợp đồng hiện là một chiều.** IVR không thể merge một thay đổi breaking trên contract của mình — CI chặn. Sales **có thể** merge một thay đổi breaking trên contract của họ, và IVR sẽ biết vào lúc chạy thật. Ảnh chụp `a3aad246` không phải một cái gate; nó là bằng chứng cho một thời điểm đã qua.

Cần cam kết đối xứng, tối thiểu:
- Sales công bố OpenAPI ở một vị trí IVR fetch được, có version.
- Thay đổi breaking đi kèm thông báo + grace period.
- Có ai đó phía Sales chạy CDC của IVR trước khi merge.

**(b) Chưa ai sở hữu consumer-driven test.** Trong gói này có 11 test `CDC-*` được đề xuất rải ở [T-01](T-01-program-matrix.md)…[T-07](T-07-production-auth.md). Chưa ticket nào nói **ai viết, chạy ở pipeline nào, đỏ thì ai sửa**. CDC không có chủ thì thành tài liệu.

Đề xuất phân công:

| Loại | Ai viết | Chạy ở đâu | Đỏ thì sao |
| --- | --- | --- | --- |
| IVR là consumer (task producer, ACK) | IVR viết, Sales chạy | pipeline Sales, trước merge | Sales chặn merge |
| IVR là provider (nhận task) | IVR viết, IVR chạy | pipeline IVR | IVR chặn merge |
| Auth/sandbox | Security cấp credential, IVR viết | pipeline IVR, job riêng | không chặn merge; báo owner |

**(c) `CURRENT_COMPAT` chưa có ngày tắt, và điều đó là cố ý.** `docs/api-versioning.md:52` ghi: đồng hồ grace **chỉ bắt đầu chạy** sau khi Target V1, auth, sandbox, dual-run và rollback evidence đều được cả Sales lẫn IVR duyệt. Nói cách khác, [T-01](T-01-program-matrix.md)…[T-07](T-07-production-auth.md) phải đóng trước, rồi mới đặt được ngày sunset. Cần Sales xác nhận họ hiểu và chấp nhận thứ tự này.

**(d) Baseline nào là baseline.** Nếu Sales trả lời trên một commit khác `a3aad246`, cần cập nhật `contract-manifest.json` và sinh lại fixture compat — chứ không so sánh chéo hai baseline rồi kết luận.

## 3. Sample payload

Không có payload. Artifact của ticket này là **cam kết vận hành**, ví dụ:

```yaml
sales_contract_publication:
  openapi_url: "<Sales điền>"
  versioning: semver
  breaking_change_notice_days: 90
  cdc_gate: "chạy CDC của IVR trong pipeline Sales trước khi merge"
  contact: "<Sales điền>"
```

## 4. Acceptance test — phải xanh khi đóng

| Test | Ở đâu | Khẳng định |
| --- | --- | --- |
| `CT-API-OAS-10` | `tests/Ivr.ContractTests/` | OpenAPI hợp lệ và khớp hash đã ghim |
| `oasdiff breaking` (2 contract) | `deploy/ci/docs.gitlab-ci.yml:22-23` | Không có thay đổi breaking so với baseline |
| `openapi-contract-drift.mjs` | job `docs` | Spec và client sinh ra không lệch |
| `CT-CONTRACT-CURRENT-06` | `tests/Ivr.ContractTests/SalesContractScaffoldTests.cs` | Đường compat khớp ảnh chụp `a3aad246` |
| **`CDC-*` (11 test)** | rải trong T-01…T-07 | Sau ticket này, mỗi cái có chủ và có pipeline |

## 5. Mock fallback

CI của IVR đã chặn được mọi thay đổi breaking **của chính IVR**. Đó là một nửa. Nửa còn lại không mock được: rủi ro nằm ở hệ thống của người khác.

## 6. Closure artifact — owner điền

- [ ] **Vị trí công bố OpenAPI của Sales** + cách IVR fetch theo version.
- [ ] **Cam kết deprecation hai chiều**: thời gian báo trước, header, kênh thông báo.
- [ ] **Bảng phân công CDC** ở §2(b), có tên người chịu trách nhiệm mỗi ô.
- [ ] **Xác nhận thứ tự sunset** ở §2(c).
- [ ] **Baseline commit** mà Sales trả lời trên đó, để cập nhật `contract-manifest.json`.

## 7. Rủi ro nếu để mở

Ticket này không chặn ngày cắm thật — nó chặn **ngày thứ hai trở đi**. Cắm xong mà không có cam kết đối xứng thì lần Sales deploy tiếp theo là một canh bạc, và IVR không có cách nào biết trước ngoài việc chờ lỗi.
