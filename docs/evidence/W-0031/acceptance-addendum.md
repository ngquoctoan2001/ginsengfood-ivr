# W-0031 — Đối chiếu test lịch sử, W-0347

Ngày: 23/09/2026 · REAL_CUSTOMER_CALL_ALLOWED=NO · trạng thái hồ sơ: TESTS_PASS.

Ba test trust-skip của slice này bị xoá ở commit `6760ba614e47e1631384640209cc4acee944a8cf`, khi
quyết định OD-18 chuyển toàn bộ việc gọi hay không gọi sang Module 3. Trong
[decisions-log](../../../plan/ivr-orther/decisions-log.md), OD-18 ghi: M3 quyết định nghiệp vụ, IVR
chỉ thực thi. Đây là thay đổi yêu cầu đã được duyệt, không phải đổi tên test. Cùng quyết định này đã
được W-0324 ghim để retire test trust-skip của W-0019, và Toàn đã nghiệm thu W-0019.

| Test lịch sử | Còn đúng sau OD-18 không | Kiểm chứng thay thế hiện hành |
| --- | --- | --- |
| `IT-ELIG-TRUST-14`: đủ bằng chứng vẫn không skip khi cổng owner đóng | Mạnh hơn: không còn cổng nào để mở | `IT-M3-AUTHORITY-05`, `IT-M3-AUTHORITY-06`: metadata trust không tạo được quyết định hay job skip |
| `UT-ELIG-TRUST-16`: đủ từng phần mới được skip, thiếu phần nào thì gọi kèm advisory | Không còn điều kiện skip phía IVR | `UT-M3-AUTHORITY-01`, `UT-M3-AUTHORITY-02`, `UT-M3-AUTHORITY-10`, `IT-M3-AUTHORITY-07`: trust và risk metadata không đổi quyết định, bộ quyết định không có skip nghiệp vụ |
| `UT-ELIG-TRUST-17`: hai chiều fail-closed ngược nhau | Chiều voice còn nguyên, chiều trust-skip bị bỏ | `UT-ELIG-VOICE-15` (thiếu bằng chứng voice thì chặn), `UT-M3-AUTHORITY-10`, `UT-M3-AUTHORITY-11` (luồng quyết định trước khi gọi không đọc được trust hay risk metadata) |

Commit, hash văn bản và trích đoạn của quyết định nằm trong [acceptance-tests.json](acceptance-tests.json).
C2 tự kiểm bốn điều:
- commit nằm trong lịch sử của candidate;
- văn bản, hash và trích đoạn khớp;
- ID cũ không còn active;
- mọi ID thay thế tồn tại và chạy Passed.

Thiếu một test thay thế, test đỏ, hoặc pin sai đều làm C2 từ chối. Không đề nghị phục hồi trust-skip.
Chỉ Toàn chuyển W-0031 sang `ACCEPTED`.
