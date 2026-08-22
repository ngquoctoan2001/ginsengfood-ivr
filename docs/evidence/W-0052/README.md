# W-0052 — Evidence: PDPA / privacy compliance & consent (`P10-1`)

Ngày: `2026-08-19` · Trạng thái: **`TESTS_PASS`** — 4/4 verification §8 xanh;
**PIA và chu kỳ retention CHƯA AI KÝ** (`W-0009`), nên **cổng pháp lý vẫn đóng**

## 1. Điều phải nói trước

`P10-1` §11 cấm go-live khi chưa có cơ sở pháp lý và PIA đã ký. Hôm nay **chưa có chữ ký nào**. Slice
này dựng phần kỹ thuật — danh mục, cổng chống trôi, DSAR, checklist — và để **ô chữ ký trống**.

Ô trống là ô trống. Điền một con số nghe hợp lý vào `retention.md` sẽ tạo ra chính xác thứ nguy hiểm
nhất: một chính sách **trông như đã ký**, mà job sẽ thi hành, và **không ai từng đồng ý**.

## 2. Danh mục ở mức **trường**, không phải mức bảng

`W-0053` phân loại **bảng**. Đó không trả lời được câu một cơ quan quản lý thực sự hỏi: *trường nào
mang dữ liệu cá nhân, tại sao các anh giữ nó, và khi có người yêu cầu xoá thì chuyện gì xảy ra*.

Nên `PersonalDataInventory` giữ 16 trường, mỗi trường có **mục đích**, **cơ sở pháp lý** và **hành vi
khi xoá**. Cổng đọc model EF: cột nào tên mang dấu hiệu dữ liệu cá nhân phải có trong danh mục hoặc
được **miễn trừ có tên, có khoá, có lý do**.

**Cổng bắt lỗi ngay lần chạy đầu**, và bắt đúng thứ nó sinh ra để bắt:

| Bị gắn cờ | Phán quyết |
| --- | --- |
| `agg_kpi_daily.invalid_phone_count` | miễn trừ — **số đếm** một result type |
| `ivr_call_attempts.invalid_phone` | miễn trừ — cờ kết quả, không chứa số nào |
| `ivr_confirmation_tasks.dial_token_expires_at` | miễn trừ — một **thời điểm**, không phải token |
| `ivr_technical_exceptions.customer_attempt_counted` | miễn trừ — boolean kế toán attempt (DT-02) |
| `ivr_confirmation_tasks.phone_validation_status` | **dữ liệu cá nhân thật, tôi đã bỏ sót** |
| `ivr_technical_exceptions.provider_error_summary` | **cột không tồn tại** — tôi viết một mục cho thứ không có |

Bốn dương tính giả có lý do, **một cột thật bị bỏ sót**, và **một mục mô tả cột tưởng tượng**. Đúng
hai loại hỏng mà một danh mục viết tay mắc phải.

## 3. `phone_validation_status` kéo theo một sửa đổi production

Nó là **dữ kiện về liên hệ của khách**. Chiến lược ẩn danh của P1-5 redact `phone_ref`,
`phone_masked`, dial token và speech summary — nhưng **để lại `VALID`**. Giữ một tín hiệu yếu về một
người mà dữ liệu của họ lẽ ra đã biến mất.

Nên nó được thêm vào câu SQL redaction, và câu SQL đó giờ là **một hằng số dùng chung** giữa retention
job và DSAR. Hai lối code cùng redact "các cột giống nhau" **sẽ** có ngày bất đồng về việc *các cột
nào*, và cái chạy ít hơn sẽ là cái cũ.

`IT-RET-PII-07` được mở rộng để khẳng định trường mới, nên lối lịch trình và lối theo-yêu-cầu
được chứng minh bằng cùng một tính chất.

## 4. Không có endpoint DSAR, và đó là quyết định

Xoá dữ liệu khách cần một **thẩm quyền IVR không sở hữu**: permission do Permission Core cấp (DF-01),
`IVR_RUNTIME_GATE_ADMIN` chưa gán cho vai trò nào (`OD-V1-20`). Treo chức năng xoá lên một permission
vận hành sẵn có nghĩa là **ai xem được hàng đợi thì xoá được dữ liệu khách**.

> **Đã bị thay thế 2026-08-22 — `OD-V1-20` đã duyệt.** `IVR_FLAG_READ` và `IVR_RUNTIME_GATE_ADMIN` nay được cấp cho role `Admin`. Câu trên ghi lại trạng thái tại baseline của gói evidence này nên giữ nguyên; trạng thái hiện tại nằm ở `plan/ivr-orther/decisions-log.md` và `specs/ui/08-role-permission-ui.md` §2.

> Kết luận **không đổi**: `IVR_RUNTIME_GATE_ADMIN` là quyền đổi runtime gate, không phải quyền DSAR. Treo xoá lên nó chỉ đổi câu trên thành *ai bật/tắt được kill switch thì xoá được dữ liệu khách*. Xem `docs/compliance/dsar-runbook.md` §1.


Nên `DsarService` chạy qua thủ tục tay **có audit** (`docs/compliance/dsar-runbook.md`), và endpoint
chờ một permission có thật.

`FindAsync` trả **số lượng, không bao giờ trả giá trị**. Một dịch vụ in ra dữ liệu cá nhân đã lưu là
**một lối đọc mới**, mở cho bất kỳ ai gọi được dịch vụ.

## 5. Ba giới hạn của quyền xoá, nói **trước**

Runbook nêu chúng trước khi xử lý yêu cầu, chứ không phát hiện giữa chừng:

1. **Audit không xoá được** — append-only ép bởi database. Một bản ghi *ai đã làm gì* mà chủ thể xoá
   được thì không phải bản ghi.
2. **`order_code` được giữ** — là khoá mà yêu cầu đi tới; xoá nó làm mọi yêu cầu sau về cùng đơn
   không trả lời được, **kể cả của chính người đó**.
3. **Payload callback giữ tới hết retention** — bỏ payload đi thì bản ghi giao nhận không còn giải
   quyết được tranh chấp nó tồn tại vì.

## 6. Do-not-call: khẳng định như một **tính chất**, không phải một ví dụ

`UT-ELIG-VOICE-15` đã phủ hai ca **không chắc chắn** (resolver chết, không ai nói). `COMP-DNC-03` phủ
ca **chắc chắn**, và phủ như một tính chất: khi resolver nói không được gọi, **không kết hợp tín hiệu
nào khác** cho ra quyết định gọi được — kể cả khách `TRUSTED` với trust-skip đang bật.

Đó là hình dạng mà cơ sở pháp lý đòi hỏi. Cuộc gọi dựa trên **thực hiện hợp đồng**, không phải lợi ích
chính đáng đem cân đo, nên do-not-call là **chặn cứng** chứ không phải một đầu vào của phép cân.

Kèm **đối chứng dương**: cùng factory, tắt cờ restriction → **gọi được**. Thiếu nó thì theory kia có
thể xanh trên một snapshot không dùng được vì lý do hoàn toàn khác.

## 7. `COMP-RETENTION-04` tìm ra một defect mô hình hoá

Cổng "mọi class retention job thực thi phải được một bảng nào đó khai" **đỏ** với `speech_snapshot`.

Nguyên nhân: `ivr_confirmation_tasks` chịu **hai** class trên **hai đồng hồ** — `speech_snapshot`
redact các trường bên trong từ sớm, `task_metadata` xoá cả dòng rất lâu sau. Tôi mô hình hoá thành
**một giá trị đơn**, nên `speech_snapshot` trông như một class **không ai phân loại** trong khi job
vẫn chạy nó — tức một lượt xoá đang diễn ra dưới một chính sách không được khai.

Sửa: `PreDeletionAnonymizeClass`, và cổng đòi **mọi** class được khai ở đâu đó.

## 8. Kiểm chứng

| Test | Kiểm âm | Kết quả |
| --- | --- | --- |
| `COMP-PII-01` | dựng một schema thêm `customer_email` | ❌ đỏ, **nêu đúng tên cột** |
| `COMP-PII-01` | (chạy thật) 6 phát hiện ở §2 | ❌ đỏ lượt đầu |
| `COMP-DSAR-02` | lý do `"ok"` (< 8 ký tự) | ❌ từ chối, không đổi gì |
| `COMP-DSAR-02` | đơn thứ hai trong cùng database | ✅ **không bị chạm** |
| `COMP-DNC-03` | đối chứng dương: tắt restriction | ✅ gọi được |
| `COMP-RETENTION-04` | (chạy thật) `speech_snapshot` | ❌ đỏ lượt đầu → §7 |

| Lệnh | Kết quả |
| --- | --- |
| `dotnet test Ivr.sln -c Release` | **442/442** — 22 contract + 255 unit + 5 chaos + 160 integration, 0 fail |
| `generate-test-traceability.mjs` | `TEST_TRACEABILITY_WRITTEN=314` |
| `docs-selftest.mjs` | `API_DOCS_SELFTEST_PASS` |
| `scan-pii.sh` | `PII_SCAN_PASS files=282` |

## 9. Cái này KHÔNG chứng minh

- **Không chữ ký nào.** PIA `DRAFT_UNSIGNED`, chu kỳ retention `UNSIGNED`, cơ sở pháp lý là **đề xuất
  kỹ thuật** chứ không phải kết luận pháp lý (`W-0009`).
- **Không xác minh danh tính người yêu cầu.** IVR không có kênh nào làm việc đó; đó là việc của Sales.
- **Không xoá được có chọn lọc bên trong một bản backup đã mã hoá.** Bản backup còn trong hạn vẫn giữ
  dữ liệu trước khi redact, và nó chỉ hết theo lịch prune. Giới hạn thật, phải nói với người yêu cầu.
- **Cổng danh mục là heuristic theo tên cột.** Nó bắt `customer_email`; nó **không** bắt một cột tên
  `notes_2` chứa số điện thoại.
- **Danh mục chỉ phủ PostgreSQL.** Log, metric và evidence file không nằm trong model EF.
- **Không endpoint DSAR** (§4) — chờ permission có thật.
- **Chưa gọi khách thật lần nào.** Mọi kết luận nói về hệ thống chạy `MOCK`; lần lab đầu tiên
  (`W-0008`) sẽ phải chạy lại toàn bộ checklist.
- **Checklist không kiểm được §2 khớp §3.** Ví dụ nó khẳng định mọi class đều được phân loại, nhưng
  **không** khẳng định số ngày ai đó điền vào config bằng số đã ký.
