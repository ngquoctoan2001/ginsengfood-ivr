# P02 — Generate Context & Scope

## Tên nhiệm vụ
Sinh context, scope, business goals, stakeholders, actors, glossary và bản assumptions/open-questions đầu tiên.

## Bối cảnh
IVR Order Confirmation là hợp phần **gọi tự động (outbound) xác nhận Official Order** qua Internal SIM Gateway. IVR là **input signal only**, không phải lớp quyết định trạng thái đơn (Order Core quyết định cuối). Phạm vi này khác với giả định inbound/order-lookup ban đầu — phải bám baseline phase-8 (`IVR-00`, `IVR-01`).

## Input cần đọc
- `specs/srs/05-current-docs-review.md` (từ p01)
- `docs/documents/4. phase/phase-8/00-QUẢN TRỊ NGUỒN SỰ THẬT VÀ PHẠM VI.md`
- `docs/documents/4. phase/phase-8/01-MỤC ĐÍCH KINH DOANH VÀ CA SỬ DỤNG XÁC NHẬN.md`
- `docs/documents/4. phase/phase-8/02-RANH GIỚI SỞ HỮU VÀ HỆ THỐNG KẾT NỐI.md`
- `docs/documents/4. phase/phase-8/03-ĐIỀU KIỆN GỌI NIỀM TIN KHÁCH HÀNG VÀ LIÊN HỆ CHÍNH THỨC.md`
- `plan/ivr-orther/02-current-understanding.md`, `04-module-dependency-map.md`, `15-open-questions.md`

## Output cần tạo
- `specs/srs/01-context-and-scope.md`
- `specs/srs/02-business-goals.md`
- `specs/srs/03-stakeholders-and-actors.md`
- `specs/srs/04-glossary.md`
- `specs/srs/06-assumptions-and-open-questions.md` (bản v1, seed từ `plan/ivr-orther/15-open-questions.md`)

## Quy tắc
- Scope phải liệt kê rõ IN/OUT (bám mục 4 của `IVR-00`).
- Glossary phải định nghĩa: IVR task, IVR result, Official Order, Order Core, SIM Gateway, attempt, Golden Hour, 24/7, Sale Lock, Recall, Suppression, trusted customer, official contact, evidence, release gate.
- Actors phải phân biệt system actors (Order Core, Operational Core, Trust Resolver, SIM Adapter, Evidence Registry) và human actors (Ops Admin, Release Owner, Customer bị gọi).
- Mỗi mục tiêu kinh doanh gắn với 1 nguồn docs.

## Checklist hoàn thành
- [ ] Scope IN/OUT khớp `IVR-00` mục 4 & 11.
- [ ] Glossary ≥ 20 thuật ngữ.
- [ ] Actors phân loại system vs human.
- [ ] Assumptions/open-questions v1 gắn owner + tác động.

## Điều cấm
- KHÔNG mở rộng scope sang inbound/order-lookup/upsell/CRM nếu chưa có `Owner Decision Required` đồng ý.
- KHÔNG sinh functional requirements ở prompt này (để p03).

## Báo cáo cuối
1. Scope IN/OUT tóm tắt.
2. Số thuật ngữ glossary.
3. Danh sách actors.
4. Số open questions v1 và nhóm.
