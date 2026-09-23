# W-0012 — Đối chiếu test lịch sử, W-0347

Ngày: 23/09/2026 · REAL_CUSTOMER_CALL_ALLOWED=NO · trạng thái hồ sơ: TESTS_PASS.

Hai test RBAC của slice này bị xoá ở commit `3e36b46` (28/08), khi W-0128 bỏ tài khoản console và
mock permission provider. Từ đó Module 3 giữ console và danh tính operator, và gọi IVR như một dịch vụ
ngang hàng bằng ba token theo tầng `read/write/danger`.
[Changelog API](../../../docs/api-changelog.md) bản `1.0.0-draft.22` ghi việc gỡ này là
"a removal the owner had already ordered". Đây là thay đổi yêu cầu, không phải đổi tên test.

| Test lịch sử | Còn đúng sau W-0128 không | Kiểm chứng thay thế hiện hành |
| --- | --- | --- |
| `UT-FND-RBAC-03`: thiếu quyền thì 403 kèm mã forbidden cố định, đủ quyền thì qua | Còn, kiểm trên credential theo tầng | `IT-API-AUTHZ-01`: tầng đủ đọc queue được 200. Tầng thiếu gọi 6 thao tác ghi đều 403 `IVR_FORBIDDEN_CALLER`. Không credential thì 401. `IT-API-AUTHZ-02`: mặt internal đòi đúng danh tính dịch vụ, scope và nguồn |
| `UT-FND-RBAC-08`: ngoài MOCK, header mock bị bỏ qua và không đăng ký được mock provider | Mạnh hơn: mock provider không còn tồn tại ở chế độ nào | `IT-DEV-AUTHZ-08`: header `X-Permissions` tự khai quyền bị 401, không seed được gì. Credential thật nhưng thiếu tầng thì 403 |

Commit, hash văn bản và trích đoạn của quyết định nằm trong [acceptance-tests.json](acceptance-tests.json).
C2 tự kiểm bốn điều:
- commit nằm trong lịch sử của candidate;
- văn bản, hash và trích đoạn khớp;
- ID cũ không còn active;
- mọi ID thay thế tồn tại và chạy Passed.

Không đề nghị phục hồi tài khoản console. Chỉ Toàn chuyển W-0012 sang `ACCEPTED`.
