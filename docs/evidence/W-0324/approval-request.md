# Phiếu trình Toàn duyệt tám việc P2 — 21/09/2026

**Đề nghị nghiệm thu tám phần việc dưới đây trong phạm vi phần mềm local/MOCK.**
Quyết định từng dòng đang PENDING; chưa đổi trạng thái ACCEPTED. REAL_CUSTOMER_CALL_ALLOWED=NO.

Bằng chứng mới tại `3cf695928ce0e2602ae660a7c6f999b08f7dee0b`: **1151/1151 test, full sweep 42/42**,
24 entry được manifest phân loại không chạy riêng. C1/C2/C4 của cả tám việc đều đạt;
C3 đã rà tay và chờ quyết định của Toàn. Xem [W-0324](README.md) và [đối chiếu test](test-crosscheck.json).
[W-0322](../W-0322/README.md#4-đối-chiếu-từng-việc) lưu nguyên toàn bộ Residual và đối chiếu
commit sau; bảng dưới chốt phạm vi duyệt và công việc ngoài phạm vi còn phải có owner.

| Việc và bằng chứng | Phần trình duyệt | Residual đã được giải quyết | Phần còn lại và người phụ trách | Quyết định của Toàn |
| --- | --- | --- | --- | --- |
| [W-0018 / P2-1](../W-0018/README.md) | Intake, validation, idempotency, persist trong MOCK | Eligibility có từ `6e0f9d3`, capacity scheduler có từ `d23ab98` | M3 cung cấp producer/schema/payload thật; Toàn phụ trách credential/hạ tầng; IVR dev chạy shared E2E | PENDING |
| [W-0020 / P2-3](../W-0020/README.md) | Scheduler và attempt policy local | Gateway có ở `ec459a3`; policy M8 ký ở `5cffea3`, giờ gọi cập nhật ở `6993e3f` | Toàn cung cấp SIM/lịch đo/đầu mối nhà mạng; IVR dev đo capacity W-0008/W-0048; M3 xác nhận producer policy | PENDING |
| [W-0021 / P2-4](../W-0021/README.md) | Mock adapter và no-egress | MOCK-only là phạm vi gốc; không còn thiếu triển khai adapter MOCK | Toàn/nhà cung cấp cấp hạ tầng; IVR dev kiểm adapter thật ở W-0008/W-0048 | PENDING |
| [W-0022 / P2-5](../W-0022/README.md) | Normalization/mapping/persist local | Callback đã có ở `2412cf6`; attempt policy M8 đã ký | Toàn chỉ định đầu mối nhà mạng; IVR dev thu DTMF/disposition thật và đối chiếu DT-02 trước khi mở lab | PENDING |
| [W-0023 / P2-6](../W-0023/README.md) | Callback/outbox fake Target và GH compatibility; [22 TestId](../W-0023/acceptance-test-matrix.md) | Bổ sung ánh xạ test để C2 xét đủ; delivery disabled là chặn có chủ đích | M3 cung cấp consumer/OpenAPI/ACK/sandbox; Toàn cấp secret/network; IVR dev triển khai auth thật và shared E2E ở work tích hợp | PENDING |
| [W-0024 / P2-7](../W-0024/README.md) | Script lifecycle/privacy và MOCK fixture | OD-V1-15 đã chốt ở `4baad09`; lời chào chung ở `f7c9be9`; ghi chú push-block là lịch sử | M3 cấp speech-safe payload; Toàn bố trí đủ actor duyệt script production hoặc quyết định đổi luật; không dùng fixture cũ làm script hiện hành | PENDING |
| [W-0065 / P2-8](../W-0065/README.md) | API internal/admin, authz/idempotency/audit/PII trong MOCK | UI chuyển M3 và bị xoá không xoá API; role/tier vẫn do M3 cung cấp | M3 chốt BFF/role→tier/actor và consumer tests; Toàn cấp secret; IVR dev kiểm shared authz/E2E và hosted pipeline đúng SHA ở work release | PENDING |
| [W-0066 / P2-9](../W-0066/README.md) | TTS port/fake/cache/privacy; [chín TestId](../W-0066/test-report.md) | Whitelist và lựa chọn VieNeu đã chốt; C2 nay đọc test-report liên kết | IVR dev kiểm model/image/media path hiện hành; Toàn ký phần nghe/CVE/quyền model và cấp máy đích/kho nội bộ theo S2/S5 | PENDING |

## Cách ghi quyết định

Toàn có thể duyệt từng ID hoặc xác nhận cả tám ID trong phạm vi ghi ở bảng. Mỗi quyết định cần
nêu đồng ý/chưa đồng ý, phạm vi, ngày và tham chiếu phản hồi; sau đó mới cập nhật tracker.
Một dòng còn việc tích hợp bên ngoài không được hiểu là đã production-ready. Chữ ký M8 không
thay xác nhận M3. Hosted pipeline và hạ tầng thật phải có gói bằng chứng riêng.

## Năm hồ sơ bổ sung, không gộp vào tám quyết định trên

- [W-0019](../W-0019/acceptance-addendum.md): đã giải thích hai ID retired bằng quyết định ghim
  và 11 test thay thế. Test mới đã Passed, C2 đạt; đủ để xét riêng sau tám việc này.
- [W-0025](../W-0025/scope-closeout.md), [W-0026](../W-0026/scope-closeout.md),
  [W-0027](../W-0027/scope-closeout.md), [W-0028](../W-0028/scope-closeout.md): hồ sơ hoàn thiện,
  đề nghị CANCELLED vì UI không còn là deliverable IVR theo W-0253. Các ô quyết định vẫn PENDING;
  đầu mối M3 chưa được xác nhận và chưa có thông điệp bàn giao gửi ra ngoài.
