# Prompts Index — IVR-Orther

Trạng thái: `PLAN_ONLY` (chưa chạy prompt nào)
Ngôn ngữ tài liệu: Tiếng Việt (thuật ngữ kỹ thuật giữ tiếng Anh khi rõ nghĩa hơn)

## 1. Mục đích

Thư mục này chứa các **prompt con** dùng ở giai đoạn sau để sinh dần bộ `specs/srs`, `integration-requirements`, `seed`, và cuối cùng là `prompt/` chính thức cho module IVR.

Ở giai đoạn hiện tại (Giai đoạn 1 — Plan), các prompt này **chỉ là bản thiết kế/chuẩn bị**. KHÔNG chạy prompt nào cho tới khi:

1. Bộ `plan/ivr-orther/` được owner duyệt.
2. Các `NEED_CONFIRMATION` / `Owner Decision Required` trong plan được trả lời tối thiểu ở mức đủ chạy prompt tương ứng.

## 2. Danh sách prompt con

| Prompt | Sinh ra | Phụ thuộc | Điều kiện chạy |
| --- | --- | --- | --- |
| [p01](p01-generate-docs-review.md) | `specs/srs/05-current-docs-review.md` + inventory + mapping docs cũ → specs mới | plan duyệt | luôn chạy đầu tiên |
| [p02](p02-generate-context-and-scope.md) | context / scope / business goals / stakeholders / actors / glossary / assumptions | p01 | sau p01 |
| [p03](p03-generate-functional-srs.md) | functional requirements chi tiết | p02 | sau p02 |
| [p04](p04-generate-workflows.md) | workflows + sequence flow | p03 | sau p03 |
| [p05](p05-generate-api-specs.md) | API specs (IVR internal/admin, telephony webhook, sales-required, ops-required, error code, auth, idempotency) | p03, p04 | sau p04 |
| [p06](p06-generate-data-mapping.md) | data ownership + data mapping (sales, ops) + missing data + PII policy | p03, p05 | sau p05 |
| [p07](p07-generate-database-design.md) | ERD + table specs + indexes + enum/status + retention + migration plan | p06 | sau p06 |
| [p08](p08-generate-architecture-design.md) | system context + boundaries + integration/deployment architecture + resilience | p05, p06 | sau p06 |
| [p09](p09-generate-integration-requirements.md) | tài liệu yêu cầu gửi team sales / ops / telephony | p05, p06 | sau p06 |
| [p10](p10-generate-seed-data.md) | seed README + seed samples (customer/order/product/inventory/call scenario/IVR menu/agent/integration status) | p06, p07 | sau p07 |
| [p11](p11-generate-testing-specs.md) | testing strategy + unit/integration/contract/e2e/perf/security + acceptance | p03..p09 | sau p09 |
| [p12](p12-generate-ui-specs.md) | admin UI specs | p03, p05 | sau p05 |
| [p13](p13-generate-final-prompt-library.md) | thư mục `prompt/` chính thức ở root | specs ổn định | sau khi p01..p12 ổn định |
| [p14](p14-review-and-normalize-specs.md) | review + chuẩn hóa + phát hiện mâu thuẫn toàn bộ specs | tất cả | chạy cuối cùng và lặp lại |

## 3. Thứ tự khuyến nghị

p01 → p02 → p03 → p04 → p05 → p06 → p07 → p08 → p09 → p10 → p11 → p12 → p14 → (khi ổn định) p13 → p14 (lặp).

Chi tiết dependency và checklist xem [../09-specs-generation-sequence.md](../09-specs-generation-sequence.md) và [../16-prompt-roadmap.md](../16-prompt-roadmap.md).

## 4. Quy ước chung cho mọi prompt con

- Mọi tài liệu sinh ra phải dùng nhãn `CONFIRMED` / `ASSUMPTION` / `NEED_CONFIRMATION` / `TODO` / `GAP` / `RISK`.
- Mọi khẳng định có căn cứ phải trích `docs/documents/...` path.
- KHÔNG bịa API/endpoint đã tồn tại nếu docs không nói.
- KHÔNG code production trong bất kỳ prompt nào trước p13; và p13 chỉ sinh prompt library, không sinh code.
- Mọi quyết định nghiệp vụ chưa có nguồn phải ghi `Owner Decision Required` — không để implementer tự suy diễn.
- Tôn trọng ranh giới: IVR là **consumer/signal**, KHÔNG phải owner của order state, payment, inventory, recall.
