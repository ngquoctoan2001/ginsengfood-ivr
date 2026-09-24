# W-0294 — Tên ca JUnit không lặp tham số fixture

Ngày 14/09/2026. **TESTS_PASS (local + Linux probe)**. Baseline `5006c0e3-5a83fd60-245f01c2-dd77861b-a605ad17` (bỏ dấu nối). Pipeline baseline [2845967257](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2845967257) còn chạy; chưa chứa sửa này.

Đã tái hiện đúng logger CI: 18 test privacy PASS nhưng XML chứa giá trị fixture trong `testcase/@name`, PII scan exit 1. Full unit probe 688/688 PASS; API report PII PASS. JunitXml.TestLogger 8.0.0 có `MethodFormat` để thêm thông tin class vào tên; [tài liệu package](https://www.nuget.org/packages/JunitXml.TestLogger/8.0.0) không liệt kê lựa chọn bỏ riêng tham số. Đối chiếu thêm README của package đã cài và XML thật.

`Ivr.CiPolicy junit <report-directory>` chỉ thay tên có tham số bằng tên method + mã băm chia nhóm. Với các tên hiển thị trùng do logger rút gọn tham số, thêm số lần xuất hiện để giữ đủ từng case; mã này định danh tên đã báo cáo, không khẳng định khôi phục tham số bị rút gọn. Giữ nguyên outcome/count/time/TestId/failure/stack trace/console output. DTD, XML lỗi, root sai và input thiếu đều bị từ chối.

Ba job `build_test_dotnet`, `schema_compat_gate`, `globalization_invariant_gate` bắt buộc xử lý JUnit ở cuối script; after_script cũng xử lý report còn lại khi test lỗi. Lỗi test vẫn làm job đỏ. CI-config guard buộc cả hai vị trí. Không sửa scanner, pattern, allowlist, dữ liệu hoặc assertion của test.

| Kiểm tra | Kết quả |
| --- | --- |
| [Artifact unit thật](unit-artifact.log) | 688 ca, 354 tên đổi, PII PASS |
| [Bảo toàn XML](preservation.log) | 688 trước/sau, 688 identity riêng; so toàn XML sau bỏ đúng thuộc tính name: không đổi field nào khác |
| [Linux SDK 10.0.201](linux-artifact.log) | chạy DLL mới và scanner thật, không mạng; 688/354 và PII PASS như Windows |
| [Policy selftest](policy-selftest.log) | test/coverage/vulnerability cũ PASS; JUnit giữ fail/skip/TestId/output, idempotent, 7 refusal; raw PII trong failure body vẫn bị scanner từ chối |
| Build/format/config | build 0 warning/0 error, formatter đúng tool exit 0, CI-config PASS |

GitNexus Usage LOW, 3 caller trong tool, 0 process. Helper `JunitReport.Run` mới chưa có trong index (`Target not found`); direct source chỉ có caller command switch của Ivr.CiPolicy. Staged detect kiểm trước commit. Runtime IVR không đổi; hosted artifact và deploy vẫn cần kết quả pipeline mới.

## Cập nhật chỉ dấu nghiệm thu — W-0325, 21/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Đây là giới hạn ủy quyền hiện hành theo tracker §2 tại commit `4346f6a`, bổ sung để kiểm C1.
Kết quả, thời điểm và phạm vi kiểm chứng lịch sử ở trên giữ nguyên; mục này không xác nhận
một lượt chạy mới và không thay chữ ký nghiệm thu. Xem [hồ sơ bổ sung W-0325](../W-0325/README.md).

## Khai báo phép kiểm C2 — W-0349, 23/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Hồ sơ này trước không khai test hay phép kiểm nào, nên C2 không xét được. [Khai báo](acceptance-tests.json) nay ghi:

- Gate trong full sweep có self-test kiểm thay đổi của việc này: `selftest-dotnet-policy.sh`, `ci-config-selftest.mjs`.

Kết quả và phạm vi lịch sử ở trên giữ nguyên.

## Owner nghiệm thu — 24/09/2026

Toàn trả lời “thì cái nào xong cho xong luôn đi” sau báo cáo W-0349 tại 11fa118. Theo [danh sách
W-0350](../W-0350/README.md), W-0294 chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm. Residual chỉ
còn việc bên ngoài (M3, Sales, Platform, tier GitLab, pháp chế) hoặc giới hạn đã ghi. Claude chọn
việc theo tiêu chí phiếu W-0348, không tự cấp phê duyệt; Toàn có thể đảo quyết định này.

Mọi giới hạn trong cột Residual của tracker giữ nguyên. Bằng chứng giữ đúng lượt collector tại
`1859243` (1200/1200 test, sweep 43/43). REAL_CUSTOMER_CALL_ALLOWED=NO.
