# Trình Toàn duyệt 9 phạm vi local — 21/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

**Đề nghị nghiệm thu phần local của cả chín việc bên dưới. Quyết định hiện tại: PENDING.**
Đây là phần còn lại của lượt rà 13 việc W-0327 sau khi W-0029/W-0032/W-0052 được owner duyệt.
W-0042 chưa nằm trong đề nghị này vì DoD staging còn thiếu bằng chứng.

Test/sweep cùng commit sạch **aaba3d2c173c1ce3b2e6dcbf899515ea5e87c979**:
1171/1171 test và 42/42 gate, 24 mục classified không có invocation; consumer vừa kiểm lại hash.
Không gán kết quả đó cho commit tài liệu hiện tại hoặc WIP. [Verification](verification.json)
giữ đủ 22 lượt TestId, C1/C2/C4 và nguyên Residual tracker. C3 cần owner đọc bảng dưới.

## Phạm vi đề nghị và phần ngoài phạm vi

| Việc / hồ sơ | Phần đề nghị nghiệm thu | Residual đã được xử lý hoặc cần hiểu đúng | Còn làm / người phụ trách |
| --- | --- | --- | --- |
| [W-0088](../W-0088/README.md) | Sửa liveness trên PostgreSQL local: pause đúng scope, phục hồi quarantine hết hạn, đóng held job quá hạn, review item cùng transaction | “Admin pause là global hold duy nhất” nói về incident scope; kill switch/capacity/revocation/circuit vẫn có hiệu lực. W-0290 chốt pause hết giờ là WINDOW_EXPIRED | **Toàn** chỉ định người vận hành và môi trường; **người vận hành + dev IVR** diễn tập policy/UAT/on-call. Local test chưa chứng minh các bước này |
| [W-0125](../W-0125/README.md) | Công cụ SQL/preflight và hồ sơ OD-18 đã chuẩn bị, test chạy trên schema disposable | Không thiếu triển khai công cụ. “Hosted CI chưa từng chạy” đã cũ: W-0292 ghi lượt 179a5eb có 31 PASS, 1 Failed, 2 Skipped, 2 Canceled; không phải pipeline xanh. Không khẳng định máy hiện thiếu psql | **Dev M3** trả lời IR-07 đã gửi, kèm revision/payload; **Toàn** cấp target/quyền read-only/ticket và hạ tầng; **người vận hành** thu preflight, dừng nếu SCHEMA_DRIFT, thu CI đúng candidate phát hành; **dev IVR** đối chiếu kết quả. Ba external gate vẫn mở |
| [W-0196](../W-0196/README.md) | Migration expand tương thích và drill backup/restore, overlap, rollback/forward đã ghi cho **ba43605 → c8dc3c4** | Cleanup ở release sau là điều kiện cố ý. Không cần DropTable để nghiệm thu expand; phục hồi bảng rỗng không phục hồi dữ liệu đã mất. Full proof aaba3d2 không chứng nhận rollback aaba3d2 | Trước release/cleanup: **Toàn + người vận hành** kiểm consumer/replica/job, cửa sổ quan sát và rollback; **chủ consumer ngoài** xác nhận; **dev IVR** diễn tập đúng cặp binary và thiết kế contract migration riêng. Dữ liệu bị drop cần pre-drop backup thật |
| [W-0197](../W-0197/README.md) | Ma trận HTTP của composition root IVR thật + PostgreSQL local; schema/auth/tier/idempotency/response | Artifact 38 operation/417 request cũ vẫn gắn working tree lịch sử. Test matrix hiện hành Passed ở candidate sạch và gọi verifier; không sửa số cũ thành lượt mới. “Giữ WIP báo cáo tuần” là chỉ dẫn của phiên cũ. Crash/full-worker thuộc bằng chứng riêng W-0332 | **Dev M3 + dev IVR** kiểm BFF/tích hợp thật khi có sandbox; **Toàn/người vận hành** thu hosted/staging đúng candidate khi phát hành. Không dùng matrix HTTP để thay crash/soak/SIM thật |
| [W-0207](../W-0207/README.md) | **Phía IVR** của ma trận shared E2E: W-0332 mới chạy tại aaba3d2, 20/20 vòng, 330 task, 11/11 ca, 0 lỗi/trùng/đếm sai | C2 đã đọc đủ hậu tố /10 từ W-0328; mười TestId Passed. W-0332 khép thiếu proof local mới, probe đồng thời và journal đều đạt. Không nhận exit P2.2 hoặc shared readiness | **Dev M3 + dev IVR** thu producer/callback revalidation/order state/BFF hai phía cùng contract; **Toàn** cấp issuer/credential và xác định người thật có thẩm quyền cho năm vai ký, hoặc duyệt sửa quy tắc riêng. Rotation/rate limit/DNS-TLS đích thật còn thiếu; template vẫn NOT_READY |
| [W-0268](../W-0268/README.md) | Sửa rò scratch và bỏ index ExpiresAt không dùng, **giữ cột** | C1/C3/C4/C5 của audit đã được W-0269/W-0270 xử lý; MOCK, phone-validation và cấp ID đã sửa ở các việc sau. S3 giữ vĩnh viễn nên PeriodDays rỗng có chủ đích. Ba helper có khác biệt ngữ nghĩa, không buộc hợp nhất để đóng B9/B10 | **Toàn/người vận hành** thu pg_stat_user_indexes trong cửa sổ có tải; **dev IVR** phân tích trước khi đề nghị bỏ index khác. Không lấy số 109/108 lịch sử làm inventory hiện tại; S2/PIA/backup thực tế theo hồ sơ W-0052/W-0330 |
| [W-0269](../W-0269/README.md) | Quy ước async cho mã viết tay, guard và cleanup scratch theo mục đích | Ngoại lệ .g.cs có chủ đích vì generator sở hữu; không sửa generated client. Ba vị trí scratch có lý do confine/cleanup. C4/C5 đã khép ở W-0270; các ghi chú index/retention/MOCK/phone/ID/helper được đối chiếu đầy đủ như W-0268 | **Toàn/người vận hành + dev IVR** chỉ làm phần index/retention thật khi có dữ liệu/quyết định như hàng trên; không còn sửa code thuộc scope async/scratch để chờ nghiệm thu |
| [W-0272](../W-0272/README.md) | Tách ba nghĩa MOCK bằng constant, giữ nguyên wire value và hành vi | Owner W-0274 đã chọn spec theo code; W-0275/W-0278 chốt enum/sentinel. **W-0336 đã sửa XML summary còn nói chờ quyết định**. Guard constant Passed; không cần migration | Không còn việc IVR trong phạm vi tách constant. **Toàn** xét nghiệm thu; phần lab/SIM thật có hồ sơ khác, không suy từ local |
| [W-0274](../W-0274/README.md) | Spec adapter khớp code, luật enable chặn mọi adapter khác MOCK | Câu hỏi thêm enum đã được W-0275/W-0278 giải quyết: channel có ba giá trị, dashboard thêm NONE khi chưa có channel. Không còn chờ draft.25. Oasdiff 25→27 từng có 10 WARN; không kết luận chắc chắn breaking hoặc chắc chắn tương thích | **Dev M3 + Toàn** đối chiếu client với contract đã ghim và enum response, xử lý WARN liên quan khi tích hợp. Không cần sửa thêm spec IVR để khép phạm vi W-0274 |

Bản rà từng vế và commit đối chiếu: [W-0327, mục 5–13](../W-0327/README.md#5-w-0088--livenessstate-machine).
Các README cũ giữ nguyên số liệu, lỗi và giới hạn lịch sử, chỉ có addendum chỉ trạng thái rà mới.

## Nội dung quyết định để owner xét

Đề nghị Toàn duyệt **W-0088, W-0125, W-0196, W-0197, W-0207, W-0268, W-0269, W-0272,
W-0274** theo đúng cột “Phần đề nghị nghiệm thu”, gồm giới hạn cặp rollback W-0196 và chỉ phía
IVR của W-0207. Những việc tích hợp/vận hành ở cột cuối vẫn mở sau quyết định này.

Chưa ghi người ký hoặc ACCEPTED: [tracker §1, luật 6 và T7](../../../prompt/_execution/prompt-execution-tracker.md#1-operating-rules)
quy định Toàn chấp nhận bằng chứng và đọc Residual. Chín dòng hiện vẫn TESTS_PASS; tổng ACCEPTED
giữ 48. Việc phê duyệt phạm vi local không mở quyền gọi khách thật hoặc phê duyệt production.
