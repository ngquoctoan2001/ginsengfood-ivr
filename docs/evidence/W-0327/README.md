# W-0327 — Rà đủ Residual của 13 việc sau C1/C2/C4

Ngày: 21/09/2026 · Codex · REAL_CUSTOMER_CALL_ALLOWED=NO.

**Kết luận: 10 việc đủ để đề nghị nghiệm thu phạm vi local đã nêu; 2 việc thiếu bằng chứng
(W-0042, W-0207); 1 việc còn triển khai (W-0052).** Đây là kết luận rà soát để Toàn xem,
không phải chữ ký ACCEPTED. Không đổi trạng thái của 13 việc, không đóng gate ngoài.

## Phát hiện cần xử lý trước

1. **W-0052: quy trình xoá dữ liệu chưa vận hành được.** `DsarService` có test và lệnh xoá đã sửa,
   nhưng không có DI/CLI/endpoint gọi dịch vụ ngoài test. W-0314 đã ghi S8 và owner cố ý để quyết
   riêng. Sếp/Toàn cần chọn người được xoá và công cụ; Codex triển khai sau quyết định. Không dùng
   kết quả SQL chạy tay hoặc test service để tuyên bố có quy trình vận hành.
2. **W-0042: chưa có bằng chứng nối sự cố → cảnh báo trong cùng lượt diễn tập.** Test counter và
   promtool xanh riêng; staging game-day và alert-fire capture mà P6-3 yêu cầu chưa có. Toàn bố trí
   môi trường/người vận hành; Codex thu bằng chứng hoặc trình một ngoại lệ phạm vi rõ ràng. Không
   tự thu hẹp DoD để ký toàn bộ P6-3.
3. **W-0207: C2 xanh không kiểm lại toàn bộ 11 ca worker E2E.** Có bằng chứng lịch sử tốt hơn ở
   W-0286/`4483029`, nhưng chưa có lượt 11 ca trên candidate `b6bd852` sau các thay đổi intake/
   contract/scheduler. Codex cần chạy lại harness và ghim SHA/hash trước khi trình phần IVR hiện
   hành; M3 vẫn phải chứng minh nửa của họ trên môi trường chung.
4. **Có nhiều ghi chú hết hiệu lực:** auth profile đã ký; readiness đã có; Trust do M3 quyết;
   tham chiếu IT-12..17 đã sửa; hosted CI đã chạy; từ vựng adapter và enum đã chốt. Ngược lại,
   issuer/credential, target DB, staging và dữ liệu M3 thật chưa được các hồ sơ này chứng minh.

## Mốc và phương pháp

- Source/tracker/lịch sử được chốt tại `3545e10e5aabf5d26ddd2997adfb32fa1c0d846f`.
- Test/sweep giữ nguyên commit `b6bd852cecdfb2df9188fc41b97c94f080639b2c`:
  **1151/1151 test**, **42/42 gate chạy**, **24 entry được manifest phân loại không chạy độc lập**.
  Đã đọc lại và xác minh hash, SHA/tree, đủ project/gate bằng consumer gốc. Không chạy lại solution
  và không đổi nhãn kết quả thành lượt kiểm tại commit của báo cáo này.
- Giữa hai mốc chỉ có tài liệu/tracker/mirror; 13 gói gốc và code/test/manifest không đổi.
  WIP TTS/lab/timeout, gồm W-0326, nằm ngoài kết luận.
- Đọc trọn 13 README, toàn bộ cột Residual trong tracker, bốn prompt P4-1/P4-4/P6-3/P10-1,
  các artifact đính kèm và quyết định/code/test liên quan. Các bảng dưới xử lý từng vế Residual
  và cả phần còn lại đáng kể trong README; không chỉ đọc câu rút gọn của danh sách tự động.
- [Phụ lục kiểm chứng](source-crosscheck.json) lưu con trỏ dòng/hash của nguyên dòng tracker và
  Residual, mapping TestId → source class/method → TRX Passed, cùng hash bundle. Không tạo sổ
  trạng thái thứ hai. [Danh sách nguồn](source-manifest.json) và [commit](commit-crosscheck.txt)
  giữ provenance của phần đối chiếu.
- GitNexus query trả về không có kết quả và cảnh báo thiếu FTS. Kết luận dựa vào Git/source/test,
  không suy từ việc graph không tìm thấy caller. Không sửa symbol runtime trong lượt rà.

“Đủ đề nghị” chỉ áp vào phần mềm/local được ghi rõ dưới đây. Không chứng nhận rolling deployment
của HEAD, shared M3 E2E, SIM thật hay production. Các bước tích hợp tương lai vẫn có cả phần dev IVR
phải làm sau khi nhận đầu vào; chúng được ghi đúng owner, không gọi tất cả là việc của bên ngoài.

## Danh sách trình xem

| Việc | Phân loại | Phạm vi được đề nghị / hành động kế tiếp |
| --- | --- | --- |
| W-0029 | Đủ đề nghị | Provider/status/contract pin ở MOCK; Toàn xét scope P4-1 local |
| W-0032 | Đủ đề nghị | JWT abstraction, mock issuer, validation/cache/refresh; Toàn xét P4-4 local |
| W-0042 | Thiếu bằng chứng | Codex thu game-day + alert capture; Toàn cấp môi trường và xác định người vận hành |
| W-0052 | Còn triển khai | Sếp/Toàn quyết S8; Codex làm lối chạy DSAR và kiểm quyền/audit/dry-run |
| W-0088 | Đủ đề nghị | Sửa liveness/state machine, không giữ cả hàng đợi vì incident một job |
| W-0125 | Đủ đề nghị | Công cụ SQL/read-only preflight và hồ sơ M3; không nghiệm thu target DB/hosted deployment |
| W-0196 | Đủ đề nghị | Expand giữ bảng/repair shape; drill đúng cặp ba43605→c8dc3c4, không chứng nhận rollback HEAD |
| W-0197 | Đủ đề nghị | Ma trận HTTP sinh theo OpenAPI hiện hành, kiểm trên composition root thật/local PostgreSQL |
| W-0207 | Thiếu bằng chứng | Codex thu lại 11 ca IVR E2E tại SHA cố định; dev M3 cung cấp nửa shared E2E |
| W-0268 | Đủ đề nghị | Dọn temp khi lỗi + gỡ duy nhất index ExpiresAt thừa, giữ cột |
| W-0269 | Đủ đề nghị | Quy ước async và thư mục tạm, guard chống quay lại |
| W-0272 | Đủ đề nghị | Tách ba nghĩa MOCK, giữ nguyên wire; quyết định từ vựng đã có W-0274 |
| W-0274 | Đủ đề nghị | Spec đúng từ vựng và luật khác MOCK; câu hỏi enum đã có W-0275/W-0278 |

Tổng **10 / 2 / 1**. Tám phiếu P2 của W-0324 vẫn là một đợt riêng đang chờ Toàn.

## 1. W-0029 — Provider Sales và CDC

[Gói gốc](../W-0029/README.md) · [prompt P4-1](../../../prompt/phase-4-integration/P4-1-order-core-wiring.md).

| Vế Residual / giới hạn | Đối chiếu và kết luận | Còn làm / người phụ trách |
| --- | --- | --- |
| MOCK là trần | P4-1 §5 tách real sandbox thành hạng mục riêng. Status card và pin contract còn code/test; 3 TestId gốc Passed tại b6bd852 | Toàn xét phần local |
| Real provider từ chối boot vì W-0006/OD-V1-07 | Nhánh từ chối vẫn có ở [CallbackDeliveryOptions](../../../src/Ivr.Infrastructure/Callbacks/CallbackDeliveryOptions.cs). Lý do “chưa duyệt profile” đã cũ: `4baad09`, W-0254/`e2d534f`, [IR-06 §7](../../../integration-requirements/06-module-3-api-handover.md#7-auth-production) xác nhận JWT đã chốt | Toàn bố trí issuer/JWKS/credential; dev M3 cấp receiver; dev IVR nối provider thật và thay guard bằng validation đầy đủ sau khi có đầu vào |
| Sandbox CDC chưa chạy | W-0282 có sandbox HTTP/fake token; W-0312 có payload chỉ số. Không phải chứng cứ producer/consumer và JWT thật của M3; [register](../../../specs/_review/open-decisions-register.md) vẫn M3_NOT_RECEIVED ở các quyết định hai bên | Dev M3 + dev IVR chạy hai chương trình, ACK/stale/idempotency/auth negatives trên cùng contract; Toàn nhận hồ sơ |
| Readiness thuộc W-0040 | Đã làm từ `c1491dd`: [IvrReadinessProbe](../../../src/Ivr.Api/Health/IvrReadinessProbe.cs) kiểm DB/schema/circuit, trả 503 khi cần. IT-OBS-HEALTH-04 hiện Passed | Không còn việc readiness của P4-1 cần triển khai |
| Không metric backend / không đóng W-0002..6 | W-0139/W-0286/W-0292 đã có observability local/hosted CI tại SHA lịch sử; không có staging đích. Contract pin DRAFT không phải M3 approval | Giữ gate tích hợp và deployment riêng; Codex nên chỉnh lời giải thích cũ ở T-07/conventions trước gói tích hợp thật |

**Đủ đề nghị cho P4-1 MOCK.** Không đề nghị bật TARGET_V1 hay đóng W-0006.

**Owner duyệt 21/09, sau baseline hồ sơ 701ed28:** Toàn yêu cầu chốt W-0029 và W-0032 trong
phạm vi Sales/JWT chạy MOCK đã rà đủ bằng chứng. W-0029 được ghi **ACCEPTED** theo phạm vi
nêu trên; các đầu vào và công việc tích hợp thật trong bảng vẫn còn mở.

## 2. W-0032 — Service JWT và mTLS

[Gói gốc](../W-0032/README.md) · [prompt P4-4](../../../prompt/phase-4-integration/P4-4-shared-auth-audit.md).

| Vế Residual / giới hạn | Đối chiếu và kết luận | Còn làm / người phụ trách |
| --- | --- | --- |
| Auth profile BLOCKED_EXTERNAL | Quyết định đã CLOSED 05/09; JWT bất đối xứng/JWKS, TTL tối đa 10 phút, scope intake. Phần dựng issuer/credential thật vẫn thiếu. Không tiếp tục gửi yêu cầu cho một đội Security/Platform giả định: IR-06 đã chỉ rõ người thật là owner IVR và dev M3 | Toàn chốt người vận hành issuer/secret; dev IVR triển khai binding JWKS/token acquisition thật và kiểm TTL/rotation; dev M3 kiểm credential hai chiều |
| Mode=Real từ chối boot | [ServiceIdentityOptionsValidator](../../../src/Ivr.Infrastructure/Auth/ServiceIdentity.cs) vẫn từ chối vô điều kiện; mock key source/token provider không tự thành provider thật chỉ bằng đổi flag | Giữ guard hiện tại; mở riêng sau triển khai và kiểm chứng integration, không chỉ xoá câu từ chối |
| mTLS bật khi chưa duyệt bị từ chối | Guard còn đúng; quyết định mới là **hoãn mTLS**, không phải chưa chọn có/không | Không buộc dựng mTLS để nghiệm thu mock JWT hoặc bắt đầu sandbox theo phương án đã ký |
| Chưa ngày tắt X-Internal-Token | Vẫn là E-4 của [phiếu M3](../../../integration-requirements/07-module-3-decision-sheet.md); TARGET_V1 đã từ chối secret tĩnh | Toàn phối hợp dev M3 chốt lịch chuyển compat và bằng chứng caller đã đổi |
| Key không deterministic / admin identity riêng | Key sinh theo process là sai lệch prompt đã ghi, không commit private key. 12 TestId gốc Passed; admin token tier là bề mặt khác, source header chỉ metadata | Toàn đọc và chấp nhận sai lệch trong scope local; không lấy test này xác nhận identity production |

**Đủ đề nghị phần P4-4 MOCK.** Các comment còn nói OD-V1-07 chưa ký là nợ mô tả, không phải một
quyết định phải xin ký lại. T-07 cũng giữ nhiều câu trước khi W-0032 tồn tại; cần addendum khi làm gói auth thật.

**Owner duyệt 21/09, cùng quyết định trên:** W-0032 được ghi **ACCEPTED** cho phạm vi JWT
MOCK đã trình, gồm sai lệch khoá sinh theo process. Issuer/credential/rotation thật và lịch
chuyển compat vẫn giữ đúng trách nhiệm trong bảng; không mở Mode=Real hoặc nghiệm thu production.

## 3. W-0042 — Chaos/game-day

[Gói gốc](../W-0042/README.md) · [game-day](../../gameday-report.md) · [P6-3](../../../prompt/phase-6-observability/P6-3-chaos-resilience-gamedays.md).

| Vế Residual / giới hạn | Đối chiếu và kết luận | Còn làm / người phụ trách |
| --- | --- | --- |
| Chưa staging / alert capture | W-0286 có LGTM runtime, W-0292 có hosted observability; chúng không nối scenario chaos tới alert-fire capture. W-0292 còn deploy Failed, staging Skipped | Toàn bố trí đích triển khai/người vận hành; Codex chạy scenario có thời gian, counter, alert firing/resolved, backlog/no-loss trong cùng gói |
| Ba dòng Ops/CRM/evidence đã phủ | IT-ELIG-NODISPATCH-15 có **hai** method (hold và positive control), đều Passed. Không cần chuyển chúng sang collection chaos | Giữ mapping tại integration test; không coi Ops API ngoài đã được IVR gọi |
| Trust cố ý trống, chờ owner | OD-18/`6760ba6` đã bỏ quyền IVR quyết call/no-call theo trust; không cần viết test “trust down ⇒ hold”. ARCH-05 còn dòng gộp Trust/Contact lỗi thời | Codex chỉnh ma trận theo OD-18, tách contact guard còn hiệu lực; không phục hồi logic trust-skip |
| Partial partition / duplicate | Đã có CHAOS-DUPLICATE-06 từ W-0103/`7195ba8`, hiện Passed; worker lease cũ bị chặn và payload retry byte-identical | Đã đóng về local. Hai câu cuối game-day còn nói chưa làm cần addendum |
| Out-of-order cùng task | Một FINAL/task; outbox chỉ nhận FINAL, nên bài đảo hai FINAL không có tiền đề hợp lệ. Không suy điều này thành mọi loại webhook đều không cần thứ tự | Không thêm test giả; M3 tự chứng minh revalidation/order-version trong shared E2E |
| IT-12..17 không tồn tại | Đã có OD-OPEN-01 và prompt sửa trong `f5c12d3`; không còn chờ owner chọn sửa prompt/spec | Codex cập nhật dòng chỉ dẫn ở game-day/tracker bằng đính chính có ngày khi làm closeout |
| Counter thiếu call site / recovery 8ms | `PostgresSchedulerStore` gọi RecordAttempt/RecordResult, `ResultRepository` gọi RecordResult; ghi chú “chưa có call site” đã cũ. 8ms vẫn chỉ là quan sát lịch sử, không phải SLO/phân phối | Không dùng số này nghiệm thu recovery trên staging |

**Thiếu bằng chứng theo DoD P6-3 §10/12**, dù phần harness và các test local đạt. Không mặc nhiên
xin một ngoại lệ; trước hết chuẩn bị lượt capture có thể thực hiện, rồi Toàn quyết phạm vi cần duyệt.

**Bổ sung 21/09 — [W-0334](../W-0334/README.md):** đã thu tại aaba3d2 sạch bốn nhóm scenario
SIM/DB/downstream/recovery, nối counter thật tới Prometheus với luật nguyên byte. Counter 0→3,
alert inactive→firing→inactive qua cửa sổ thật; 69 capture, target không mất và counter không reset.
Đính chính ARCH-05/game-day đã xong. Đây là harness dùng cầu MeterListener; còn cần deployment
OTLP, Alertmanager/thông báo và staging game-day phù hợp, nên không tự nhận DoD P6-3 hoàn tất.

## 4. W-0052 — Privacy/DSAR

[Gói gốc](../W-0052/README.md) · [runbook](../../compliance/dsar-runbook.md) · [W-0314](../W-0314/README.md).

| Vế Residual / giới hạn | Đối chiếu và kết luận | Còn làm / người phụ trách |
| --- | --- | --- |
| “Không chữ ký nào”, retention chưa điền | Đã cũ: W-0266/`a3688c7` ghi owner tự làm quorum cho chính sách; S3/W-0316/`a6b7063` thay các kỳ hạn bằng giữ vĩnh viễn, PeriodDays={} có chủ đích. PIA vẫn DRAFT_UNSIGNED và S2 còn ô nhận rủi ro | Toàn/Sếp xử lý phần phê duyệt còn thiếu; Codex đối soát PIA/checklist theo chính sách mới, không suy S3 thành Legal/go-live approval |
| Không endpoint vì OD-V1-20 | Quyền runtime-gate đã cấp từ 22/08, nhưng không phải quyền DSAR. **Thiếu cả lối chạy** ngoài test, không chỉ endpoint; S8 đã xác nhận | Sếp/Toàn chọn người được xoá và CLI/nút quản trị/quy trình; Codex triển khai đúng lựa chọn, kiểm phân quyền, dry-run, audit, phạm vi một đơn, lặp lại an toàn |
| Danh tính do Sales xác minh | Vẫn đúng ranh giới; FindAsync chỉ trả số lượng | Bộ phận bán hàng xác minh chủ thể; người được giao thực hiện xoá phải gắn hồ sơ xác minh |
| Không xoá chọn lọc backup | Vẫn là giới hạn. Không được hứa “backup tự hết theo hạn” khi S3 giữ vĩnh viễn; cần xử lý quyền xoá và restore theo quyết định thực tế | Toàn/Sếp chốt giới hạn và cách xử lý backup/restore; dev/ops lập bước chứng minh, không tự đặt hạn |
| Inventory heuristic, chỉ EF/PostgreSQL | COMP-PII-01 không phát hiện PII giấu trong cột tên vô hại. W-0314 bổ sung COMP-PII-02 đối chiếu hai chiều inventory/SQL; không biến nó thành kiểm mọi log/file | Codex giữ rà nội dung log/evidence và gate PII riêng; mở inventory khi thêm data surface, không quảng bá heuristic thành bảo đảm toàn hệ |
| Luồng xoá thiếu trường / chạy lại | `9d20d61` và `53c2eb5` sửa query/phone/contact/legacy trust/repeat erase; 7 TestId bổ sung của W-0314 đều Passed tại b6bd852 | Phần sửa service đã có bằng chứng; không cần làm lại. Không chạy xoá trong rollout pod cũ/mới |
| Chưa khách thật / retention theo chính sách | Không có customer-call proof trong scope này; COMP-RETENTION-04 kiểm classification, không chứng minh thời hạn đã được người có thẩm quyền duyệt | Giữ NO và recording OFF; không suy từ test thành tuân thủ pháp lý hay nghiệm thu vận hành |

**Còn triển khai, chưa trình ACCEPTED W-0052.** Bước triển khai phụ thuộc S8 thật sự chưa trả lời;
yêu cầu “rà Residual” không phải quyết định chọn công cụ hay cấp quyền xoá.

**Cập nhật 21/09 — W-0330, sau lượt rà trên:** owner đã trả lời S8 rằng tự chạy theo từng bước.
[CLI và hướng dẫn](../../compliance/dsar-cli-step-by-step.md) đã được bổ sung, cùng kiểm quyền,
preview, một đơn, lặp lại và rollback nguyên tử nếu audit lỗi. [Hồ sơ W-0330](../W-0330/README.md)
ghim bằng chứng mới; PIA/checklist/retention có đối soát kỹ thuật mới. Vế “chưa chọn công cụ/chưa có
lối chạy” đã được xử lý. Lượt owner chạy, Sales xác minh chủ thể, S2/PIA và quy trình backup/restore
thật vẫn còn; không tự đổi W-0052 sang ACCEPTED. Bảng 10/2/1 ở đầu là kết quả tại mốc rà cũ.

**Bổ sung sau thực hành owner, 21/09:** owner đã tự preview/execute đơn giả A và đối chứng B;
[W-0330 owner-practice](../W-0330/owner-practice.json) kiểm trực tiếp bốn audit, A được redact,
58 cột B không đổi và 43 file gói khớp aaba3d2. Vế thiếu lượt owner chạy đã khép ở phạm vi local.
W-0052 đủ đề nghị xét phần công cụ/thao tác local; không suy thành xác minh Sales, S2/PIA,
quyền trên máy vận hành hay backup/restore thật. Trạng thái gốc và bảng lịch sử giữ nguyên.

**Owner duyệt sau thực hành, 21/09:** Toàn yêu cầu “xong rồi thì đóng accepted cho t đi” sau
khi nhận hồ sơ `2b79674`. W-0052 và W-0330 được ghi ACCEPTED cho phạm vi local đã trình;
xem [quyết định trong W-0330](../W-0330/README.md#owner-nghiệm-thu--21092026).
Các phần vận hành/phê duyệt bên ngoài nêu trên vẫn mở; bảng phân loại đầu là lịch sử lượt rà.

## 5. W-0088 — Liveness/state machine

[Gói gốc](../W-0088/README.md), commit `fb1eb4c`; [PostgresSchedulerStore](../../../src/Ivr.Infrastructure/Scheduling/PostgresSchedulerStore.cs)
vẫn lọc incident global bằng `scope='ADMIN_QUEUE_PAUSE'`, phục hồi quarantine hết hạn và đóng HELD
quá deadline; [EligibilityRepository](../../../src/Ivr.Infrastructure/Repositories/EligibilityRepository.cs)
ghi review item trong transaction.

- Residual “explicit admin pause remains the only global queue hold” là **bất biến về incident scope**,
  không phải TODO gỡ pause và không có nghĩa bỏ qua kill switch/capacity/revocation/circuit guard khác.
- IT-SCH-DEADLINE-09/HOLD-10 Passed. W-0290/`d44834e` chốt operator pause hết giờ là WINDOW_EXPIRED;
  không lấy CAPACITY_EXCEPTION của mô tả cũ làm tiêu chí hiện hành.
- Production incident policy, operator UAT và diễn tập on-call trong README chưa được local test
  chứng minh. Toàn chỉ định người vận hành; người đó cùng dev IVR diễn tập trên môi trường được cấp.

**Đủ đề nghị phần sửa liveness local.** Không phát hiện nhánh phục hồi bắt buộc còn thiếu trong scope đó.

## 6. W-0125 — M3 authority, DB preflight, CI

[Gói gốc](../W-0125/README.md), `9ab10ce`; [SQL](../../../tools/ops/od18-legacy-skip-preflight.sql)
và [runner](../../../tools/ops/Invoke-Od18Preflight.ps1) còn tồn tại; IT-M3-AUTHORITY-13 Passed.

| Vế Residual | Kết luận | Còn làm / người phụ trách |
| --- | --- | --- |
| Không gate nào đóng / M3 sign-off | Đúng với phạm vi công cụ W-0125; M3 producer filtering/consumer usage chưa được ký thay bằng M8. Phiếu cũ đã được gom vào IR-07, trạng thái SENT/AWAITING_REPLY ở W-0316 | Dev M3 trả lời kèm revision/payload; Toàn theo dõi phiếu đã gửi, không gửi lại từ lượt rà này |
| Target DB thiếu psql/endpoint/quyền | “Máy không có psql” là quan sát cũ, không được khẳng định lại hôm nay. Chưa tìm thấy target preflight có output và authority thật; test chạy schema disposable | Toàn cấp đích/quyền read-only/ticket; người vận hành chạy script, lưu count/migration inventory, dừng nếu SCHEMA_DRIFT; Codex đối chiếu predicate |
| Hosted CI NOT_RUN | Đã hết đúng. W-0292/`2667f42` ghi 31 PASS/1 Failed/2 Skipped/2 Canceled tại 179a5eb, không phải pipeline PASS. Không được lấy hosted SHA đó chứng minh b6bd852 | Toàn bố trí hạ tầng CI/deploy còn thiếu; người vận hành thu pipeline đúng candidate khi làm release |
| Local PG không thay target; real calls NO | Còn đúng | Giữ nguyên ranh giới |

**Đủ đề nghị công cụ/hồ sơ local**, không phải nghiệm thu ba external gate.

## 7. W-0196 — Expand/contract migration

[Gói gốc](../W-0196/README.md), [machine drill](../W-0196/rollback-evidence.json),
[runbook](../../database/expand-contract.md).

- “Cleanup only in a later release” là **điều kiện cố ý**, không phải thiếu một DropTable cần làm ngay.
  Hai migration W0122/P03 và test ExpandContractMigrationTests không đổi từ `c8dc3c4` đến b6bd852;
  IT-SCHEMA-EXPAND-07 và ba guard BACKCOMPAT Passed ở b6bd852.
- Drill có hai SHA thật `ba43605` → `c8dc3c4`, backup/DLL hash, overlap/rollback/forward recovery.
  Chỉ chứng minh cặp đó; **không chứng minh rollout b6bd852**. Script hiện hành đã đổi seed 9/8→10/9,
  nên cũng không chạy script mới rồi gán ngược cho cặp cũ.
- Trước cleanup: Toàn/người vận hành kiểm đủ consumer nội bộ/ngoài, mọi replica/job, thời gian quan
  sát và rollback floor; chủ consumer ngoài xác nhận; dev IVR thiết kế migration contract ở release
  riêng. Nếu cần dữ liệu đã bị drop, phải có pre-drop backup; tạo lại bảng rỗng không phục hồi dữ liệu.
- MOCK/NO còn đúng. Hosted/cluster rollback và cleanup không được ký bằng drill local.

**Đủ đề nghị phần expand gốc, với giới hạn cặp rollback ghi rõ.** Mỗi candidate phát hành sau phải
diễn tập lại đúng cặp binary của nó, kể cả khi W-0196 được Toàn nghiệm thu.

## 8. W-0197 — Ma trận HTTP API

[Gói gốc](../W-0197/README.md), commit `729392e`; test
[ApiBehaviorMatrixTests](../../../tests/Ivr.IntegrationTests/ApiBehaviorMatrixTests.cs) Passed trong TRX ghim b6bd852.

- “Content-hash-bound working tree, chưa clean exact SHA”: đúng với artifact cũ 38/38, 417 request.
  Nay có test chạy trên checkout sạch b6bd852 và tự gọi verifier. Không sửa artifact cũ thành kết quả mới.
- Artifact bổ sung còn ở checkout của lượt chạy: **draft.31, 39 operation, 470 case entry, 0 behavior
  failures**. Case entry gồm cả N/A; **không gọi 470 là số HTTP request**. JSON này không được collector
  cũ ghim riêng; hash chụp hôm nay chỉ làm phụ lục, TRX Passed đã ghim mới là bằng chứng nghiệm thu C2.
- W-0307/`6029b19` thêm audit-evidence và W-0312/`f7bb52b` đổi intake đã đi qua matrix hiện hành.
- Crash/full worker thuộc P1.2: W-0286 đã có 100 vòng tại 4483029, không dùng matrix thay full worker
  và không gán 100 vòng đó cho b6bd852. Hosted current candidate, vendor/customer calls vẫn ngoài phạm vi.
- “Giữ WIP báo cáo tuần” là chỉ dẫn bảo toàn cây làm việc của phiên cũ, không phải nợ sản phẩm.

**Đủ đề nghị ma trận HTTP local hiện hành.** Toàn xét scope; dev M3 chạy luồng BFF thật khi tích hợp.

## 9. W-0207 — Nửa IVR của shared E2E

[Gói gốc](../W-0207/README.md), `aaf3e69`; [W-0286](../W-0286/README.md).

| Vế Residual / artifact | Đối chiếu và kết luận | Còn làm / người phụ trách |
| --- | --- | --- |
| P2.2 chưa đạt exit, thiếu producer/callback/BFF M3 | Còn đúng. W-0282 mở sandbox và W-0312 có ví dụ HTTP không chứng minh hệ M3 đã tạo task, revalidate/đổi trạng thái và gọi BFF | Dev M3 + dev IVR thu cả hai phía trên cùng contract/hash; Toàn nhận báo cáo |
| 11/11 ca IVR, 20 vòng cũ | Artifact gốc working-tree không có commit field. W-0286 nâng lên 100 vòng/1130 task tại 4483029, giữ 11 ca. Sau đó intake/contract/scheduler đổi; full sweep b6bd852 không chạy tools/dev/Invoke-LocalMockE2E.mjs | Codex chạy lại 11 ca cùng crash/lease/outage theo harness tại clean SHA, ghi source/contract hash, thời gian, journal và mọi lỗi; không cần M3 để hoàn thành nửa local |
| 500 đồng thời trong JSON gốc | W-0286/`4483029` sửa phối hợp receipt/transaction và probe không client retry đạt; đây là lỗi đã sửa, không còn mục triển khai mới từ W-0207 | Giữ artifact cũ, dẫn bản sửa và regression mới |
| Năm vai ký, template NOT_READY | Validator vẫn yêu cầu M8_OWNER/M3_OWNER/SECURITY/PLATFORM/RELEASE_OWNER. W-0254/W-0266 nói nhóm thực có owner IVR và dev M3; **chưa có quyết định riêng đổi validator này** | Toàn xác định người thực sự có thẩm quyền cho từng vai hoặc duyệt thay đổi quy tắc; Codex chỉ sửa sau quyết định, không bịa người ký/nhân chữ ký |
| Rotation/rate limit/DNS-TLS mới fake | 429/quota sandbox có W-0282; vẫn không chứng minh issuer/rotation hay DNS/TLS của đích M3 thật | Toàn bố trí issuer/credential; dev M3 + dev IVR đo trên đích chung |
| M8 ký taxonomy như M3 đã ký | W-0304/`9dc5479` sửa về M8_POSITION_SIGNED/M3_NOT_RECEIVED; ACK taxonomy vẫn đúng với contract, UT-CALLBACK-TARGET-ACK-CROSS-01 Passed | Không nâng shared readiness từ chữ ký một bên |

**Thiếu bằng chứng mới cho phần local cần trình tại b6bd852; shared E2E vẫn chờ M3.** C2 chỉ xét
9 TestId từ README. Ký hiệu `IT-API-TERMINATE-09/10` làm extractor bỏ sót hậu tố `/10`; đã kiểm tay
IT-API-TERMINATE-10 hiện tồn tại và Passed, lưu ở phụ lục. Codex cần bổ sung parser/regression cho
kiểu viết tắt này trong việc công cụ nghiệm thu tiếp theo; không bỏ qua TestId chỉ vì script bỏ sót.

**Cập nhật 21/09:** W-0328 đã sửa parser và có regression; [W-0332](../W-0332/README.md) đã chạy
E2E mới tại aaba3d2 sạch: 20/20 vòng, 330 task, 11/11 ca IVR, 0 lỗi; cả mười TestId gốc Passed
trong bộ 1171 test và 42 gate cùng candidate. Khoảng trống local nêu trên đã khép, đủ đề nghị Toàn
xét phần IVR. Nửa M3 và quy tắc signer thật vẫn còn như bảng; không tự ACCEPTED.

## 10. W-0268 — Temp và index thừa

[Gói gốc](../W-0268/README.md), `307fd9c`. Residual tracker LOCAL_ONLY/không đóng external/NO vẫn đúng.
Hai TestId index/backcompat Passed; full sweep mới 42/42 thay cho lần cũ 37/39, không xoá lịch sử đỏ.

| Phần còn lại trong README | Kết luận và owner/action |
| --- | --- |
| Index khác cần đo | Chưa có thống kê target đủ để xoá tiếp. Toàn/người vận hành cung cấp pg_stat_user_indexes qua một cửa sổ có tải; dev IVR phân tích trước quyết định. Không lấy con số 109/108 lịch sử làm inventory hiện hành |
| Retention chưa arm | S3/W-0316 đã chọn giữ vĩnh viễn; không tự điền PeriodDays. Rủi ro/DSAR theo W-0052/S8 |
| MOCK nhập nhằng | W-0272/00b7c93 đã tách ba khái niệm; W-0274 sửa spec, W-0278 sửa dashboard |
| phone_validation_status required nhưng được thiếu | W-0250/d68a471 siết intake; W-0312/f7bb52b giữ validation khi mở luồng chỉ số. Không còn là quyết định chưa chọn như dòng lịch sử |
| NEXT_WORK_ID lệch 18 | W-0284/fa7877c phục hồi dòng; tracker hiện đã qua W-0325. Không dùng ID lịch sử để cấp tiếp |
| assertString/ExactKeys/readStrictJson | Census guard đã giữ mức trùng; khác biệt có ngữ nghĩa theo review S4. Không cần hợp nhất để đóng B9/B10; nếu đổi accepted input thì Toàn duyệt phạm vi, Codex có test bảo vệ |
| C1/C3/C4/C5 kỹ thuật | W-0269/3e2c785 và W-0270/98aa11b đã xử lý; không còn TODO của B9/B10 |

**Đủ đề nghị B9/B10 local.** Không xin xoá các index khác hay cột ExpiresAt.

## 11. W-0269 — Async và scratch

[Gói gốc](../W-0269/README.md), `3e2c785`. ARCH-ASYNC-01/ARCH-CONST-01/UT-BOOT-05 Passed.

- LOCAL_ONLY/NO/không đóng external còn đúng. `.g.cs` được loại khỏi quy ước mã viết tay vì generator
  sở hữu; không phải trường hợp sót phải xoá ConfigureAwait trong generated client.
- C4/C5 được W-0270 xử lý: UID trong Dockerfile và guard DI. C1/C3 của audit là tên finding cũ,
  **không phải** các tiêu chí C1/C3 của công cụ nghiệm thu.
- Toàn bộ các ghi chú index/retention/MOCK/phone-validation/NEXT_WORK_ID/ba helper còn lại được
  đối chiếu như bảng W-0268 trên; không bỏ chúng chỉ vì tracker rút xuống một câu LOCAL_ONLY.
- Quy ước scratch giữ ba nơi theo mục đích; API-matrix cleanup có finally. Không buộc hợp nhất mọi
  scratch sang một nơi vì validator confine cần thư mục trong repo. Pin attested lịch sử giữ nguyên;
  pin sống được full sweep kiểm. Hai gate đỏ cũ không còn ở b6bd852.

**Đủ đề nghị C1/C3 của audit local.** Toàn đọc ngoại lệ generated code và ranh giới cleanup đã ghi.

## 12. W-0272 — Ba nghĩa MOCK

[Gói gốc](../W-0272/README.md), `00b7c93`; [SimAdapters](../../../src/Ivr.Infrastructure/Telephony/SimAdapters.cs).

- LOCAL_ONLY/NO còn đúng. ExecutionModes.Mock, FeatureFlagValues.MockSimProvider, SimAdapters.Mock
  giữ ba nghĩa riêng, cùng giá trị wire; ARCH-CONST-01 Passed, không cần migration cho phép tách tên.
- Câu hỏi “code hay spec đúng” **đã đóng** bằng owner W-0274/`16e44b7`: sửa spec theo code.
  W-0275/`50a83c5` và W-0278/`e984983` tiếp tục siết enum/sentinel.
- XML summary trong SimAdapters vẫn kể known divergence như hiện tại; đó là comment cũ cần Codex
  sửa khi dọn tài liệu, không phải một câu hỏi từ vựng còn chờ owner.
- Các lỗi gate handover lịch sử đã không còn trong full sweep b6bd852. Không áp kết quả local cho lab/production.

**Đủ đề nghị phần tách hằng số, giữ nguyên hành vi.**

## 13. W-0274 — Spec adapter và luật enable

[Gói gốc](../W-0274/README.md), `16e44b7`; [W-0275](../W-0275/README.md), [W-0278](../W-0278/README.md).

- LOCAL_ONLY/NO vẫn đúng. Luật enable chặn mọi adapter khác MOCK được IT-API-SIM-09 chứng minh;
  bảng spec/permission/prompt đã sửa, UI IVR cũ đã retire và M3 sở hữu console.
- Câu hỏi “thêm enum OpenAPI?” **đã được owner chốt và triển khai**: channel MOCK/VENDOR/ASTERISK_ARI;
  dashboard thêm NONE khi không có channel. Không còn phải chờ commit draft.25.
- Kết luận cũ “chắc chắn breaking” đã được W-0275 rút lại vì đây là response. Nhưng không suy tiếp
  “chắc chắn không breaking”: W-0284 đã chạy oasdiff thật và ghi **10 WARN**, exit 1 cho 25→27.
  Baseline/changelog được W-0276/W-0279 sửa; warn lịch sử vẫn phải được M3 đọc khi tích hợp.
- Toàn/dev M3 đối chiếu client với contract draft.31 và các enum response; không cần sửa code IVR
  thêm để hoàn tất phạm vi chỉnh spec W-0274.

**Đủ đề nghị phần chỉnh spec.** Không chứng nhận client M3 đã xử lý enum.

## Hành động sau lượt rà

1. **Codex, làm được local:** thu lại E2E W-0207 tại SHA cố định; bổ sung parser TestId dạng `/10`
   với regression; lập addendum cho các lời chỉ dẫn cũ đã chỉ ra. Không đổi verdict của bằng chứng cũ.
2. **Toàn/Sếp:** quyết S8 về người/công cụ xoá; bố trí đích game-day; xác định signer thật của shared
   E2E. Codex chuẩn bị nội dung kỹ thuật theo quyết định, không ký hoặc gửi phiếu thay owner.
3. **Toàn:** xem 10 phạm vi đề nghị ở bảng đầu. Cả 13 trạng thái gốc và tám phiếu P2 giữ nguyên.

Kiểm lượt rà: consumer chứng cứ cũ PASS; mapping bổ sung Passed; **86 tệp nguồn/hash, 35 commit
ancestor, 39 liên kết local** đã kiểm. `docs-selftest` PASS; PII PASS bốn file; tracker/mirror và
`git diff --check` PASS; GitNexus staged **LOW, 7 file, 0 process**. Cả 13 dòng gốc và tám dòng P2
giữ nguyên. Hash chia nhóm để scanner không nhầm thành số liên hệ; không sửa rule quét.
Không có lượt test/runtime mới để thay các phần NOT_RUN đã ghi.
